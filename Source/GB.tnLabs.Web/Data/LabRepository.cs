using System.Linq;
using Breeze.ContextProvider;
using Breeze.ContextProvider.EF6;
using GB.tnLabs.Web.Repository;
using Newtonsoft.Json.Linq;
using GB.tnLabs.AzureFacade.Models;
using System.Collections.Generic;
using GB.tnLabs.Core.Repository;
using System;
using GB.tnLabs.Web.Infrastructure;
using GB.tnLabs.Core;
using GB.tnLabs.Core.SBDtos;

namespace GB.tnLabs.Web.Data
{
	//TODO: what does this do??
    public class LabContext : ApplicationDbContext
    {
        public LabContext()
        {
            base.Configuration.ProxyCreationEnabled = false;
        }
    }

	public class LabContextProvider : EFContextProvider<ApplicationDbContext>
	{
		private readonly Lazy<IUnitOfWork> _unitOfWorkLazy;
		private readonly Lazy<ServiceBusMessageHandler> _messagingLazy;

		protected IUnitOfWork UnitOfWork
		{
			get { return _unitOfWorkLazy.Value; }
		}

		protected ServiceBusMessageHandler Messaging
		{
			get { return _messagingLazy.Value; }
		}

		public LabContextProvider(Lazy<IUnitOfWork> unitOfWorkLazy, 
			Lazy<ServiceBusMessageHandler> messagingLazy)
			: base() 
		{
			_unitOfWorkLazy = unitOfWorkLazy;
			_messagingLazy = messagingLazy;
		}

		protected override bool BeforeSaveEntity(EntityInfo entityInfo)
		{
			//NOTE: OriginalValuesMap for SubscriptionId is not set because, we only try to avoid 
			//an unauthorised modification of the SubscriptinoId, if it doesnt' change then the db will
			//not be updated each time.

			// return false if we don’t want the entity saved.
			// prohibit any additions of entities of type 'Role'
			if (entityInfo.Entity.GetType() == typeof(User) && 
				(entityInfo.EntityState == EntityState.Modified || entityInfo.EntityState == EntityState.Added))
			{
				User user = (User)entityInfo.Entity;
				user.SubscriptionId = UnitOfWork.ActiveSubscriptionId;
			}
			else if (entityInfo.Entity.GetType() == typeof(Session) &&
				(entityInfo.EntityState == EntityState.Modified || entityInfo.EntityState == EntityState.Added))
			{
				//var s = this.ObjectContext.ObjectStateManager.GetObjectStateEntry(entityInfo.Entity);
				Session session = (Session)entityInfo.Entity;
				session.SubscriptionId = UnitOfWork.ActiveSubscriptionId;

				session.Version = Guid.NewGuid().ToString();
				entityInfo.OriginalValuesMap["Version"] = null;
			  
			}
			else if (entityInfo.Entity.GetType() == typeof(Lab) &&
				(entityInfo.EntityState == EntityState.Modified || entityInfo.EntityState == EntityState.Added))
			{
				Lab lab = (Lab)entityInfo.Entity;
				lab.SubscriptionId = UnitOfWork.ActiveSubscriptionId;
			}
           
		    return true;
		}

	    protected override void AfterSaveEntities(Dictionary<Type, List<EntityInfo>> saveMap, List<KeyMapping> keyMappings)
        {
            List<EntityInfo> sessionList;
            if (saveMap.TryGetValue(typeof(Session), out sessionList))
            {
                foreach (EntityInfo sessionEntity in sessionList)
                {
                    Session session = (Session)sessionEntity.Entity;
                    if (!session.Removed && session.SessionId > 0)
                    {
                        //we will not wait for the completion to avoid a lag in the response
                        Messaging.SendSession(session.StartDate, session.EndDate, session.SessionId, new Guid(session.Version));
                    }
                }
            }

            base.AfterSaveEntities(saveMap, keyMappings);
        }

		protected override Dictionary<Type, List<EntityInfo>> BeforeSaveEntities(Dictionary<Type, List<EntityInfo>> saveMap)
		{
			return saveMap;
		}
	}

    public class LabRepository
	{
		#region private fields

		private readonly LabContextProvider //EFContextProvider<tnLabsDBEntities>
           _contextProvider;// = new LabContextProvider(); //EFContextProvider<tnLabsDBEntities>();
		private readonly Lazy<IUnitOfWork> _unitOfWorkLazy;

		#endregion privat efields

		#region properties

		protected ApplicationDbContext Context { get { return _contextProvider.Context; } }

		protected IUnitOfWork UnitOfWork
		{
			get { return _unitOfWorkLazy.Value; }
		}

		#endregion properties

		#region .ctor

		public LabRepository(Lazy<IUnitOfWork> unitOfWorkLazy, LabContextProvider contextProvider)
		{
			_unitOfWorkLazy = unitOfWorkLazy;
			_contextProvider = contextProvider;
		}

		#endregion .ctor

		public string Metadata
        {
            get { return _contextProvider.Metadata(); }
        }

        public SaveResult SaveChanges(JObject saveBundle)
        {
            return _contextProvider.SaveChanges(saveBundle);
        }

        public IQueryable<Lab> Labs
        {
            get { return Context.Labs; }
        }

        public IQueryable<Session> Sessions
        {
            get { return Context.Sessions; }
        }

        public IQueryable<SessionUser> SessionUsers
        {
            get { return Context.SessionUsers; }
        }

        public IQueryable<VirtualMachine> VirtualMachines
        {
            get { return Context.VirtualMachines; }
        }

        public IQueryable<User> Users
        {
            get { return Context.Users; }
        }

    }


}
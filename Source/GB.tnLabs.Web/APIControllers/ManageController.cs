using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using System.Text.RegularExpressions;
using GB.tnLabs.Core.Components;
using GB.tnLabs.Web.Data;
using GB.tnLabs.Core.Repository;
using Microsoft.AspNet.Identity.EntityFramework;
using GB.tnLabs.Web.Infrastructure;

namespace GB.tnLabs.Web.APIControllers
{
    public class ManageController : ApiController
    {
        private readonly Lazy<IUnitOfWork> _unitOfWorkLazy;

        protected IUnitOfWork UnitOfWork { get { return _unitOfWorkLazy.Value; } }


        public ManageController(Lazy<IUnitOfWork> unitOfWorkLazy)
        {
            _unitOfWorkLazy = unitOfWorkLazy;
        }

        #region Invite Users

        [System.Web.Http.HttpPost]
        public int SendInvites()
        {
            try
            {
                var emailList = new List<string>();
                var requestContent = HttpUtility.UrlDecode(Request.Content.ReadAsStringAsync().Result);

                //TODO:find a better way
                requestContent = requestContent.Replace("value=", string.Empty);

                if (!string.IsNullOrWhiteSpace(requestContent))
                {
                    //check to see if the request contains multiple emails
                    if (requestContent.Contains(";"))
                    {
                        var emails = requestContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        //check emails format, if they are valid they are addded to the invitation email list
                        foreach (var email in emails)
                        {
                            var trimmedEmail = email.TrimStart().TrimEnd();
                            if (IsEmailValid(trimmedEmail))
                                emailList.Add(trimmedEmail);
                        }
                    }
                    else
                    {
                        //the request contains only one email which if it is vaild, it is added to the invitation email list
                        var trimmedEmail = requestContent.TrimStart().TrimEnd();
                        if (IsEmailValid(trimmedEmail))
                            emailList.Add(trimmedEmail);
                    }

                    //build, send and save the invitations, if there is at least one valid email in the emailList
                    if (emailList.Count > 0 && Email.BuildInviteToTnLabs(emailList).Send())
                        SaveInvites(emailList);
                }
                return emailList.Count;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion

        #region Handle User Roles

        [System.Web.Http.HttpGet]
        public List<string> GetLoggedInUserSubscriptionRoles()
        {
            var loggedInUserRoles = new List<string>();
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var identity = context.Identities.Where(x => x.IdentityId == UnitOfWork.CurrentIdentity.IdentityId).FirstOrDefault();
                    var subscription = context.Subscriptions.Where(x => x.SubscriptionId == UnitOfWork.ActiveSubscriptionId).FirstOrDefault();
                    if(identity != null && subscription != null)
                    {
                        var roles = context.SubscriptionIdentityRoles.Where(x =>
                            x.IdentityId == identity.IdentityId && x.SubscriptionId == subscription.SubscriptionId);
                        if (roles.Any())
                            roles.ToList().ForEach(x => loggedInUserRoles.Add(x.Role));
                    }
                }
            }
            catch (Exception)
            {
                loggedInUserRoles.Add("Error");
            }
            return loggedInUserRoles;
        }

        [System.Web.Http.HttpGet]
        public List<string> GetUserSubscriptionRoles(string identityId)
        {
            var userRoles = new List<string>();
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var subscription = context.Subscriptions.Where(x => x.SubscriptionId == UnitOfWork.ActiveSubscriptionId).FirstOrDefault();
                    var identity = context.Identities.Where(x => x.IdentityId.ToString() == identityId).FirstOrDefault();
                    if (subscription != null && identity != null)
                    {
                        var roles = context.SubscriptionIdentityRoles.Where(x =>
                            x.IdentityId == identity.IdentityId && x.SubscriptionId == subscription.SubscriptionId);
                        if (roles.Any())
                            roles.ToList().ForEach(x => userRoles.Add(x.Role));
                    }
                }
            }
            catch (Exception)
            {
                userRoles.Add("Error");
            }
            return userRoles;
        }

        [System.Web.Http.HttpPost]
        public bool AddUserSubscriptionRole(string role, string identityId)
        {
            var success = false;
            try
            {
                if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(identityId))
                    return false;

                if (role == RoleTypes.Trainer || role == RoleTypes.Owner)
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var subscription = context.Subscriptions.Where(x => x.SubscriptionId == UnitOfWork.ActiveSubscriptionId).FirstOrDefault();
                        var identity = context.Identities.Where(x => x.IdentityId.ToString() == identityId).FirstOrDefault();

                        if (subscription != null && identity != null && !context.SubscriptionIdentityRoles.Any(x => x.Role == role &&
                                x.IdentityId == identity.IdentityId && x.SubscriptionId == subscription.SubscriptionId))
                        {
                            context.SubscriptionIdentityRoles.Add(new SubscriptionIdentityRole()
                            {
                                SubscriptionId = subscription.SubscriptionId,
                                IdentityId = identity.IdentityId,
                                Role = role
                            });

                            context.SaveChanges();
                            success = true;
                        }
                    }
                }
                return success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [System.Web.Http.HttpPost]
        public bool RemoveUserSubscriptionRole(string role, string identityId)
        {
            var success = false;
            try
            {
                if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(identityId))
                    return false;

                if (role == RoleTypes.Trainer || role == RoleTypes.Owner)
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var subscription = context.Subscriptions.Where(x => x.SubscriptionId == UnitOfWork.ActiveSubscriptionId).FirstOrDefault();
                        var identity = context.Identities.Where(x => x.IdentityId.ToString() == identityId).FirstOrDefault();

                        if (subscription == null || identity == null)
                            return false;

                        var subscriptionRole = context.SubscriptionIdentityRoles.Where(x => x.Role == role &&
                            x.IdentityId == identity.IdentityId && x.SubscriptionId == subscription.SubscriptionId).FirstOrDefault();

                        if (subscriptionRole != null)
                        {
                            context.SubscriptionIdentityRoles.Remove(subscriptionRole);
                            context.SaveChanges();
                            success = true;
                        }
                    }
                }
                return success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Private Methods

        private bool IsEmailValid(string email)
        {
            try
            {
                return Regex.Match(email, @"[\w+-]+(?:\.[\w+-]+)*@[\w+-]+(?:\.[\w+-]+)*(?:\.[a-zA-Z]{2,4})").Success;
            }
            catch(Exception)
            {
                return false;
            }
        }

        private void SaveInvites(IList<string> emailList)
        {
            using (var context = new ApplicationDbContext())
            {
                var identity = context.Identities.Where(x => x.IdentityId == UnitOfWork.CurrentIdentity.IdentityId).FirstOrDefault();
                var subscription = context.Subscriptions.Where(x => x.SubscriptionId == UnitOfWork.ActiveSubscriptionId).FirstOrDefault();

                if (identity != null && subscription != null)
                {
                    foreach (var email in emailList)
                    {
                        if (!context.Invitations.Where(x => x.Identity.IdentityId == identity.IdentityId &&
                             x.Subscription.SubscriptionId == subscription.SubscriptionId && x.Email == email).Any())
                        {
                            context.Invitations.Add(new Invitation()
                            {
                                Email = email,
                                Identity = identity,
                                Subscription = subscription,
                                IdentityId = identity.IdentityId,
                                SubscriptionId = subscription.SubscriptionId,
                                Processed = false
                            });
                            context.SaveChanges();
                        }
                    }
                }
            }
        }

        #endregion Private Methods
    }
}
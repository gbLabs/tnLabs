using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;

namespace GB.tnLabs.Core.Service
{
	public partial class Service1 : ServiceBase
	{
		private IWindsorContainer _container;
		private ServiceBusMessageHandler handler;

		public Service1()
		{
			InitializeComponent();

			_container = new WindsorContainer().Install(new DependencyConventions());


			handler = _container.Resolve<ServiceBusMessageHandler>();
			
		}

		protected override void OnStart(string[] args)
		{
			handler.Listen();
		}

		protected override void OnStop()
		{
			handler.Stop();
		}
	}
}

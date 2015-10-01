using Castle.Windsor;
using GB.tnLabs.Core;
using GB.tnLabs.Core.Components;
using GB.tnLabs.Core.SBDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.CoreHost
{
	static class Program
	{
		private static IWindsorContainer _container;

		static void Main(string[] args)
		{
			_container = new WindsorContainer().Install(new DependencyConventions());


			ServiceBusMessageHandler handler = _container.Resolve<ServiceBusMessageHandler>();
			handler.Listen();

			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace GB.tnLabs.Core.Service
{
	static class Program
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static int Main(string[] args)
		{
			Logger.Info("Starting application.");

			if (Environment.UserInteractive)
			{
				Logger.Info("Running in User Interactive mode.");
				// we only care about the first two characters
				string arg = args.Any() ? args[0].ToLowerInvariant().Substring(0, 2) : string.Empty;

				switch (arg)
				{
					case "/i":  // install
						return InstallService();

					case "/u":  // uninstall
						return UninstallService();

					default:  // unknown option
						Logger.Warn("Argument not recognized: {0}", arg);
						return 1;
				}
			}
			else
			{
				Logger.Info("Starting service.");
				ServiceBase[] servicesToRun =
				{ 
					new Service1() 
				};
				ServiceBase.Run(servicesToRun);
			}

			return 0;
		}

		private static int InstallService()
		{
			//var service = new Service1();

			try
			{
				// perform specific install steps for our queue service.
				//service.InstallService();

				// install the service with the Windows Service Control Manager (SCM)
				ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
				Logger.Info("Instalation done");
			}
			catch (Exception ex)
			{
				if (ex.InnerException != null && ex.InnerException.GetType() == typeof(Win32Exception))
				{
					Win32Exception wex = (Win32Exception)ex.InnerException;
					Logger.Error("Error(0x{0:X}): Service already installed!", ex);
					return wex.ErrorCode;
				}
				else
				{
					Logger.Error(ex);
					return -1;
				}
			}

			return 0;
		}

		private static int UninstallService()
		{
			//var service = new MyQueueService();

			try
			{
				// perform specific uninstall steps for our queue service
				//service.UninstallService();

				// uninstall the service from the Windows Service Control Manager (SCM)
				ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
			}
			catch (Exception ex)
			{
				if (ex.InnerException.GetType() == typeof(Win32Exception))
				{
					Win32Exception wex = (Win32Exception)ex.InnerException;
					Logger.Error("Error(0x{0:X}): Service not installed!", ex);
					return wex.ErrorCode;
				}
				else
				{
					Logger.Error(ex);
					return -1;
				}
			}

			return 0;
		}
	}
}

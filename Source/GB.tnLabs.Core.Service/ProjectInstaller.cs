using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Service
{
	[RunInstaller(true)]
	public partial class ProjectInstaller : System.Configuration.Install.Installer
	{
		public ProjectInstaller()
		{
			InitializeComponent();
		}

		private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
		{
			using (ServiceController sc = new ServiceController(serviceInstaller1.ServiceName))
			{
				sc.Start();
			}
		}
	}
}

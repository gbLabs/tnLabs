using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DbUp;

namespace Gb.tnLabs.Database
{
	class Program
	{
		static int Main(string[] args)
		{
			var connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

			var upgrader =
				DeployChanges.To
					.SqlDatabase(connectionString)
					.WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
					.WithTransaction()
					.LogToConsole()
					.Build();

			var result = upgrader.PerformUpgrade();

			if (!result.Successful)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(result.Error);
				Console.ResetColor();
				return -1;
			}

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Success!");
			Console.ResetColor();
			return 0;
		}
	}
}

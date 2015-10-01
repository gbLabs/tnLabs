using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;

namespace GB.Utils.Extensions
{
	public static class LoggerExtensions
	{
		public static void Info(this ILogger logger, string formatMessage, params object[] info)
		{
			string message = string.Format(formatMessage, info);
			logger.Info(message);
		}
	}
}

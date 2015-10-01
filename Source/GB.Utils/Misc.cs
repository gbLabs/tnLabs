using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GB.Utils
{
	public static class Misc
	{
		/// <summary>
		/// Takes a string as input, and outputs the same string with only the letters, numbers and underline character set. All other charaters are removed. Empty space character is replaced by underline character.
		/// </summary>
		public static string GetSafeString(string input)
		{
			Regex rgx = new Regex("[^a-zA-Z0-9 ]");
			string result = rgx.Replace(input, "");
			result = result.Replace(' ', '_');
			
			return result;
		}
	}
}

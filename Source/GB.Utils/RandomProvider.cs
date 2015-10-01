using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GB.Utils
{
	//TODO: figure out a way to make it thread safe
	public class RandomProvider
	{
		private static readonly Random _randSeed = new Random();

		[ThreadStatic]private readonly Random _rand;

		public RandomProvider()
		{
			_rand = new Random(_randSeed.Next());
		}

		public string AlphaNumeric(int length)
		{
			string def = "abcdefghijklmnopqrstuvwxyz0123456789";
			StringBuilder ret = new StringBuilder();
			for (int i = 0; i < length; i++)
				ret.Append(def.Substring(_rand.Next(def.Length), 1));
			return ret.ToString();
		}

		public string SecurePassword()
		{ 
			//TODO: impelent SecurePassword()
			throw new NotImplementedException();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public string SecurePassword(int length)
        {
            string password = string.Empty;
            char[] characters = { 'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
                                  'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                                  '0','1','2','3','4','5','6','7','8','9','?','!','@','#','$','%','^','&','*','~'};

            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z])";
            Regex regex = new Regex(pattern);

            while (!regex.Match(password).Success)
            {
                password = string.Empty;
                for (int i = 0; i < length; i++)
                {
                    int index = _randSeed.Next(0, characters.Length);
                    password += characters[index];
                }
            }

            return password;
        }
    }
}

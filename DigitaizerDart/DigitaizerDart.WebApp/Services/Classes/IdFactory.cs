using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitaizerDart.WebApp.Services.Classes
{
    public class IdFactory : IIdFactory
    {
        private Random random = new Random();
        public string IdGenerator()
        {
            const string chars = "1234567890ABCDEFGHIJKMNLOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

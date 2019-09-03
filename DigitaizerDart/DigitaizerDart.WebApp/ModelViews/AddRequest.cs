using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitaizerDart.WebApp.ModelViews
{
    public class AddRequest
    {
        public string Frame { get; set; }

        public int PeopleIndex { get; set; }

        public float X { get; set; }

        public float Y { get; set; }
    }
}

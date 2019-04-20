using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitaizerDart.WebApp.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public List<VersionJson> Versions { get; set; }
    }
}

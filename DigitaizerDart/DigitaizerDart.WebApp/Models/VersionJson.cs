using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitaizerDart.WebApp.Models
{
    public class VersionJson
    {
        public Guid VersionJsonId { get; set; }
        public Guid UserId { get; set; }
        public User Username { get; set; }
        public string Body { get; set; }
        public Guid FileId { get; set; }
        public File FileName { get; set; }
    }
}

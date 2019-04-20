using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitaizerDart.WebApp.Models
{
    public class File
    {
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
        public string JsonPath { get; set; }
        public List<VersionJson> Versions { get; set; }
    }
}

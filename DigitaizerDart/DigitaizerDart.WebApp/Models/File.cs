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
        public byte[] MarkArray { get; set; }
        public string Path { get; set; }
        public List<Version> Versions { get; set; }
    }
}

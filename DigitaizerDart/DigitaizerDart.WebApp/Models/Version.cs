using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitaizerDart.WebApp.Models
{
    public class Version
    {
        public Guid VersionId { get; set; }
        public Guid UserId { get; set; }
        public User Username { get; set; }
        public Guid FileId { get; set; }
        public File FileName { get; set; }
        public byte[] MarkArray { get; set; }
        public DateTime DateTime { get; set; }
    }
}

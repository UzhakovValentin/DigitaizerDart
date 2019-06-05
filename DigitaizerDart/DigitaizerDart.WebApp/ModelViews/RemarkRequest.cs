using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitaizerDart.WebApp.ModelViews
{
    public class RemarkRequest
    {
        public Guid UserId { get; set; }
        public Guid FileId { get; set; }
        public int Frame { get; set; }
        public int CameraRacurs { get; set; }
        public int PeopleId { get; set; }
        public string Bone { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}

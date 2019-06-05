using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitaizerDart.WebApp
{
    public class MarkJsonObject
    {
        public string DeviceType { get; set; }
        public List<DeviceData> DeviceData { get; set; }
    }

    public class DeviceData
    {
        public int CameraAt { get; set; }
        public List<CameraData> Data { get; set; }
    }

    public class CameraData
    {
        public string Type { get; set; }
        public List<PointInfo> PointInfo { get; set; }
    }

    public class PointInfo
    {
        public int Frame { get; set; }
        public Position Position { get; set; }
    }

    public class Position
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}

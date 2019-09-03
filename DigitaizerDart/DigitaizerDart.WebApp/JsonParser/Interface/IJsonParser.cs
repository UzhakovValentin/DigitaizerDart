using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitaizerDart.WebApp.ModelViews;
using Newtonsoft.Json.Linq;

namespace DigitaizerDart.WebApp.JsonParser.Interface
{
    interface IJsonParser
    {
        JObject Edit();
        JObject AddPoint(string jsonString);
        JObject DeletePoint(string jsonString);
    }
}

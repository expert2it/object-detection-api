using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CiliAPI.NetCore.Models
{
    public class DetectionResult
    {
        public DetectionResult(string json)
        {
            JObject jObject = JObject.Parse(json);
            //JToken jDetection = jObject["Detection"];
            Count = (int)jObject["Count"];
            URLs = jObject["URLs"].ToArray();
            //JObject my_obj = JsonConvert.DeserializeObject<JObject>(json);
            //JToken outer = JToken.Parse(json);
            JArray inner = jObject["Detection"]["objects"].Value<JArray>();
            if (inner.Count > 0)
            {
                Detection = new List<KeyValuePair<string, List<string>>>();
                foreach (JObject obj in inner)
                {
                    List<string> keys = obj.Properties().Select(p => p.Name).ToList();
                    foreach (string k in keys)
                    {
                        Debug.Print(k);
                        var v = obj[k];
                        Detection.Add(new KeyValuePair<string, List<string>>(k, v.ToObject<List<string>>()));
                    }
                }
            }

        }
        public int Count { get; set; }
        public Array URLs { get; set; }
        //public Detection Detection { get; set; }
        public List<KeyValuePair<string, List<string>>> Detection { get; set; }
    }
}

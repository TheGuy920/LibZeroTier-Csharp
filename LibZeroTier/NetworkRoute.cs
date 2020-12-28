using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace LibZeroTier
{
    [Serializable]
    public class NetworkRoute : ISerializable
    {

        public NetworkRoute(SerializationInfo info = null, StreamingContext ctx = new StreamingContext())
        {
            try
            {
                Flags = info.GetInt32("flags");
                Metric = info.GetInt32("metric");
                Target = info.GetString("target");
                Via = info.GetString("via");
            }
            catch(Exception e) { Debug.WriteLine(e); }
        }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue("target", Target);
            info.AddValue("via", Via);
            info.AddValue("flags", Flags);
            info.AddValue("metric", Metric);
        }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("via")]
        public string Via { get; set; }

        [JsonProperty("flags")]
        public int Flags { get; set; }

        [JsonProperty("metric")]
        public int Metric { get; set; }
    }
}

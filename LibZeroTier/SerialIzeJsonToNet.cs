using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using LibZeroTier.CustomDNS;
using System.Diagnostics;

namespace LibZeroTier
{
    public class SerialIzeJsonToNet
    {
        public static ZeroTierNetwork Serialize(JObject JsonContents)
        {
            ZeroTierNetwork Network = new ZeroTierNetwork();
            if (JsonContents["allowDNS"] != null)
                Network.AllowDNS = bool.Parse(JsonContents["allowDNS"].ToString());
            if (JsonContents["allowGlobal"] != null)
                Network.AllowGlobal = bool.Parse(JsonContents["allowGlobal"].ToString());
            if (JsonContents["allowManaged"] != null)
                Network.AllowManaged = bool.Parse(JsonContents["allowManaged"].ToString());
            if (JsonContents["assignedAddresses"] != null)
                Network.AssignedAddresses = JsonContents["assignedAddresses"].ToString().Replace(" ", "").Split(',');
            if (JsonContents["broadcastEnabled"] != null)
                Network.BroadcastEnabled = bool.Parse(JsonContents["broadcastEnabled"].ToString());
            if (JsonContents["dhcp"] != null)
                Network.DHCP = bool.Parse(JsonContents["dhcp"].ToString());
            if (JsonContents["dns"] != null)
                Network.Dns = new DNS() { };
            if (JsonContents["id"] != null)
                Network.Id = JsonContents["id"].ToString();
            if (JsonContents["mac"] != null)
                Network.MacAddress = JsonContents["mac"].ToString();
            if (JsonContents["mtu"] != null)
                Network.MTU = int.Parse(JsonContents["mtu"].ToString());
            if (JsonContents["multicastSubscriptions"] != null)
                Network.MulticastSubscriptions = new MulticastSubscription[] {  };
            if (JsonContents["name"] != null)
                Network.NetworkName = JsonContents["name"].ToString();
            if (JsonContents["netconfRevision"] != null)
                Network.NetconfRevision = int.Parse(JsonContents["netconfRevision"].ToString());
            if (JsonContents["nwid"] != null)
                Network.NetworkId = JsonContents["nwid"].ToString();
            if (JsonContents["portDeviceName"] != null)
                Network.DeviceName = JsonContents["portDeviceName"].ToString();
            if (JsonContents["portError"] != null)
                Network.PortError = int.Parse(JsonContents["portError"].ToString());
            if (JsonContents["routes"] != null)
                Network.Routes = new NetworkRoute[] { };
            if (JsonContents["status"] != null)
                Network.NetworkStatus = JsonContents["status"].ToString();
            if (JsonContents["type"] != null)
                Network.NetworkType = JsonContents["type"].ToString();

            Network.IsConnected = JsonContents["status"].ToString().Equals("OK");


            Debug.WriteLine(JsonContents["dns"]["servers"].ToString());
            return Network;
        }
    }
}

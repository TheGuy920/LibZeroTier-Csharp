using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using LibZeroTier.CustomDNS;
using Newtonsoft.Json;

namespace LibZeroTier
{
    [Serializable]
    public class ZeroTierNetwork : ISerializable, IEquatable<ZeroTierNetwork>, IComparable<ZeroTierNetwork>
    {
        private DNS? dns;
        private string? networkId;
        private string? macAddress;
        private string? networkName;
        private string? networkStatus;
        private string? networkType;
        private Int32? mtu;
        private bool? dhcp;
        private bool? bridge;
        private bool? broadcastEnabled;
        private Int32? portError;
        private Int32? netconfRevision;
        private string[]? assignedAddresses;
        private NetworkRoute[]? routes;
        private MulticastSubscription[]? multicastSubscriptions;
        private string? deviceName;
        private bool? allowManaged;
        private bool? allowGlobal;
        private bool? allowDefault;
        private bool? allowDns;
        private bool? isConnected;

        public delegate void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e, object value);
        public event PropertyChangedEventHandler PropertyChanged;
        public ZeroTierNetwork()
        {

        }
        protected ZeroTierNetwork(SerializationInfo info, StreamingContext ctx)
        {
            try
            {
                Dns = info.GetValue("dns", typeof(DNS)) as DNS;
                MulticastSubscriptions = info.GetValue("multicastSubscriptions", typeof(MulticastSubscription[])) as MulticastSubscription[];
                NetworkId = info.GetString("nwid");
                MacAddress = info.GetString("mac");
                NetworkName = info.GetString("name");
                NetworkStatus = info.GetString("status");
                NetworkType = info.GetString("type");
                MTU = info.GetInt32("mtu");
                DHCP = info.GetBoolean("dhcp");
                Bridge = info.GetBoolean("bridge");
                BroadcastEnabled = info.GetBoolean("broadcastEnabled");
                PortError = info.GetInt32("portError");
                NetconfRevision = info.GetInt32("netconfRevision");
                AssignedAddresses = (string[])info.GetValue("assignedAddresses", typeof(string[]));
                DeviceName = info.GetString("portDeviceName");
                AllowManaged = info.GetBoolean("allowManaged");
                AllowGlobal = info.GetBoolean("allowGlobal");
                AllowDefault = info.GetBoolean("allowDefault");
                AllowDNS = info.GetBoolean("allowDNS");
                Routes = (NetworkRoute[])info.GetValue("routes", typeof(NetworkRoute[]));
                IsConnected = true;
            }
            catch (Exception e) { Debug.WriteLine(e); }
        }
        public void AddEventHandler(PropertyChangedEventHandler eventHandler)
        {
            this.PropertyChanged -= eventHandler;
            this.PropertyChanged += eventHandler;
        }
        protected void NotifyPropertyChanged(object value, [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(Enum.Parse(typeof(StatusChange), propertyName), new PropertyChangedEventArgs(propertyName), value);
        }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue("nwid", NetworkId);
            info.AddValue("mac", MacAddress);
            info.AddValue("name", NetworkName);
            info.AddValue("status", NetworkStatus);
            info.AddValue("type", NetworkType);
            info.AddValue("mtu", MTU);
            info.AddValue("dhcp", DHCP);
            info.AddValue("bridge", Bridge);
            info.AddValue("broadcastEnabled", BroadcastEnabled);
            info.AddValue("portError", PortError);
            info.AddValue("netconfRevision", NetconfRevision);
            info.AddValue("assignedAddresses", AssignedAddresses);
            info.AddValue("routes", Routes);
            info.AddValue("portDeviceName", DeviceName);
            info.AddValue("allowManaged", AllowManaged);
            info.AddValue("allowGlobal", AllowGlobal);
            info.AddValue("allowDefault", AllowDefault);
        }

        public void UpdateNetwork(ZeroTierNetwork network)
        {
            if (network != null)
            {
                if (!string.IsNullOrWhiteSpace(this.NetworkId))
                {
                    if (this.NetworkId.Equals(network.NetworkId))
                    {

                        AllowDNS = network.AllowDNS;

                        AllowDefault = network.AllowDefault;

                        AllowGlobal = network.AllowGlobal;

                        AllowManaged = network.AllowManaged;

                        AssignedAddresses = network.AssignedAddresses;

                        Bridge = network.Bridge;

                        BroadcastEnabled = network.BroadcastEnabled;

                        DHCP = network.DHCP;

                        DeviceName = network.DeviceName;

                        Dns = network.Dns;

                        IsConnected = network.IsConnected;

                        MTU = network.MTU;

                        MacAddress = network.MacAddress;

                        MulticastSubscriptions = network.MulticastSubscriptions;

                        NetconfRevision = network.NetconfRevision;

                        NetworkId = network.NetworkId;

                        NetworkName = network.NetworkName;

                        NetworkStatus = network.NetworkStatus;

                        NetworkType = network.NetworkType;

                        PortError = network.PortError;

                        Routes = network.Routes;
                    }
                }
            }
        }
        [JsonProperty("allowDNS")]
        public bool? AllowDNS
        {
            get
            {
                return allowDns;
            }
            set
            {
                if (allowDns?.Equals(value) != true && allowDns != null)
                    NotifyPropertyChanged(value);
                allowDns = value;
            }
        }

        [JsonProperty("allowDefault")]
        public bool? AllowDefault
        {
            get
            {
                return allowDefault;
            }
            set
            {
                if (allowDefault?.Equals(value) != true && allowDefault != null)
                    NotifyPropertyChanged(value);
                allowDefault = value;
            }
        }

        [JsonProperty("allowGlobal")]
        public bool? AllowGlobal
        {
            get
            {
                return allowGlobal;
            }
            set
            {
                if (allowGlobal?.Equals(value) != true && allowGlobal != null)
                    NotifyPropertyChanged(value);
                allowGlobal = value;
            }
        }

        [JsonProperty("allowManaged")]
        public bool? AllowManaged
        {
            get
            {
                return allowManaged;
            }
            set
            {

                if (allowManaged?.Equals(value) != true && allowManaged != null)
                    NotifyPropertyChanged(value);
                allowManaged = value;
            }
        }

        [JsonProperty("assignedAddresses")]
        public string[]? AssignedAddresses
        {
            get
            {
                return assignedAddresses;
            }
            set
            {
                if (value != null)
                {
                    if (assignedAddresses?.SequenceEqual(value) != true)
                        NotifyPropertyChanged(value);
                    assignedAddresses = value;
                }
            }
        }

        [JsonProperty("bridge")]
        public bool? Bridge
        {
            get
            {
                return bridge;
            }
            set
            {
                if (bridge?.Equals(value) != true)
                    NotifyPropertyChanged(value);
                bridge = value;
            }
        }

        [JsonProperty("broadcastEnabled")]
        public bool? BroadcastEnabled
        {
            get
            {
                return broadcastEnabled;
            }
            set
            {
                if (broadcastEnabled?.Equals(value) != true)
                    NotifyPropertyChanged(value);
                broadcastEnabled = value;
            }
        }

        [JsonProperty("dhcp")]
        public bool? DHCP
        {
            get
            {
                return dhcp;
            }
            set
            {
                if (dhcp?.Equals(value) != true)
                    NotifyPropertyChanged(value);
                dhcp = value;
            }
        }

        [JsonProperty("dns")]
        public DNS? Dns
        {
            get
            {
                return dns;
            }
            set
            {
                if (dns != null && value != null)
                    if (dns.Domain.Equals(value.Domain) != true || dns.Servers.SequenceEqual(value.Servers) != true)
                        NotifyPropertyChanged(value);
                dns = value;
            }
        }

        [JsonProperty("id")]
        public string? Id
        {
            get
            {
                return networkId;
            }
            set
            {
                if (networkId?.Equals(value) != true)
                    NotifyPropertyChanged(value);
                networkId = value;
            }
        }

        [JsonProperty("mac")]
        public string? MacAddress
        {
            get
            {
                return macAddress;
            }
            set
            {
                if (macAddress?.Equals(value) != true)
                    NotifyPropertyChanged(value);
                macAddress = value;
            }
        }

        [JsonProperty("mtu")]
        public int? MTU
        {
            get
            {
                return mtu;
            }
            set
            {
                if (mtu?.Equals(value) != true)
                    NotifyPropertyChanged(value);
                mtu = value;
            }
        }

        [JsonProperty("multicastSubscriptions")]
        public MulticastSubscription[]? MulticastSubscriptions
        {
            get
            {
                return multicastSubscriptions;
            }
            set
            {
                if (isArrayEqual(multicastSubscriptions, value) != true)
                {
                    NotifyPropertyChanged(value);
                }
                multicastSubscriptions = value;
            }
        }

        [JsonProperty("name")]
        public string? NetworkName
        {
            get
            {
                return networkName;
            }
            set
            {
                if (networkName?.Equals(value) != true)
                    NotifyPropertyChanged(value);
                networkName = value;
            }
        }

        [JsonProperty("netconfRevision")]
        public int? NetconfRevision
        {
            get
            {
                return netconfRevision;
            }
            set
            {
                if (netconfRevision?.Equals(value) != true)
                    NotifyPropertyChanged(value);
                netconfRevision = value;
            }
        }

        [JsonProperty("nwid")]
        public string? NetworkId
        {
            get
            {
                return networkId;
            }
            set
            {
                if (networkId?.Equals(value) != true)
                    NotifyPropertyChanged(value);
                networkId = value;
            }
        }

        [JsonProperty("portDeviceName")]
        public string? DeviceName
        {
            get
            {
                return deviceName;
            }
            set
            {
                if (deviceName?.Equals(value) != true && !string.IsNullOrWhiteSpace(deviceName))
                    NotifyPropertyChanged(value);
                deviceName = value;
            }
        }

        [JsonProperty("portError")]
        public int? PortError
        {
            get
            {
                return portError;
            }
            set
            {
                if (portError?.Equals(value) != true)
                    NotifyPropertyChanged(value);
                portError = value;
            }
        }

        [JsonProperty("routes")]
        public NetworkRoute[]? Routes
        {
            get
            {
                return routes;
            }
            set
            {
                if (value != null)
                {
                    if (isArrayEqual(routes, value) != true)
                        NotifyPropertyChanged(value);
                    routes = value;
                }
            }
        }

        [JsonProperty("status")]
        public string? NetworkStatus
        {
            get
            {
                return networkStatus;
            }
            set
            {
                if (networkStatus?.Equals(value) != true)
                    NotifyPropertyChanged(value);
                networkStatus = value;
            }
        }

        [JsonProperty("type")]
        public string? NetworkType
        {
            get
            {
                return networkType;
            }
            set
            {
                if (networkType?.Equals(value) != true)
                    NotifyPropertyChanged(value);
                networkType = value;
            }
        }
        public bool? IsConnected
        {
            get
            {
                return isConnected;
            }
            set
            {
                if (isConnected?.Equals(value) != true && isConnected != null)
                    NotifyPropertyChanged(value);
                isConnected = value;
            }
        }
        /*
        public string Title
        {
            get
            {

                if (NetworkName != null && NetworkName.Length > 0)
                {
                    return NetworkId + " (" + NetworkName + ")";
                }
                else
                {
                    return NetworkId;
                }
            }
        }
        */

        public bool Equals(ZeroTierNetwork network)
        {
            if (NetworkId == null || network == null)
                return false;

            return NetworkId.Equals(network.NetworkId);
        }

        public int CompareTo(ZeroTierNetwork network)
        {
            if (NetworkId == null || network == null)
                return -1;

            UInt64 thisNwid = UInt64.Parse(NetworkId, System.Globalization.NumberStyles.HexNumber);
            UInt64 otherNwid = UInt64.Parse(network.NetworkId, System.Globalization.NumberStyles.HexNumber);

            if (thisNwid > otherNwid)
            {
                return 1;
            }
            else if (thisNwid < otherNwid)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        public int GetHashCode(ZeroTierNetwork obj)
        {
            return obj.NetworkId.GetHashCode();
        }
        private bool isArrayEqual(object[] array1, object[] array2)
        {
            if (array1 == null || array2 == null)
                return array1 == array2;

            if (array1.Length != array2.Length)
                return false;

            try
            {
                MulticastSubscription[] Newarray1 = array1 as MulticastSubscription[];
                MulticastSubscription[] Newarray2 = array2 as MulticastSubscription[];
                for (var i = Newarray1.Length - 1; i >= 0; i--)
                {
                    if (!JsonConvert.SerializeObject(Newarray1[i]).Equals(JsonConvert.SerializeObject(Newarray2[i])))
                        return false;
                }
                return true;
            }
            catch { }

            try
            {
                NetworkRoute[] Newarray1 = array1 as NetworkRoute[];
                NetworkRoute[] Newarray2 = array2 as NetworkRoute[];
                for (var i = Newarray1.Length - 1; i >= 0; i--)
                {
                    if (!JsonConvert.SerializeObject(Newarray1[i]).Equals(JsonConvert.SerializeObject(Newarray2[i])))
                        return false;
                }
                return true;
            } catch { }

            for (var i = array1.Length - 1; i >= 0; i--)
                if (!ReferenceEquals(array1[i], array2[i]))
                    return false;

            return true;
        }
    }
}
namespace LibZeroTier.CustomDNS 
{
    [Serializable]
    public class DNS
    {
        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("servers")]
        public List<object> Servers { get; set; }
    }
    [Serializable]
    public class MulticastSubscription
    {
        [JsonProperty("adi")]
        public long Adi { get; set; }

        [JsonProperty("mac")]
        public string Mac { get; set; }
    }
}

        
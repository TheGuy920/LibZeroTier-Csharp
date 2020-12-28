using System;
using Newtonsoft.Json;

namespace LibZeroTier
{
    public class ZeroTierStatus
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("publicIdentity")]
        public string PublicIdentity { get; set; }

        [JsonProperty("online")]
        public bool Online { get; set; }

        [JsonProperty("tcpFallbackActive")]
        public bool TcpFallbackActive { get; set; }

        [JsonProperty("versionMajor")]
        public int VersionMajor { get; set; }

        [JsonProperty("versionMinor")]
        public int VersionMinor { get; set; }

        [JsonProperty("versionRev")]
        public int VersionRev { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("clock")]
        public UInt64 Clock { get; set; }
    }
    public enum StatusChange
    {
        AllowDefault,
        AllowDNS,
        AllowGlobal,
        AllowManaged,
        AssignedAddresses,
        Bridge,
        BroadcastEnabled,
        ConnectionTimeout,
        CreatedNetwork,
        DHCP,
        DestroyedNetwork,
        DeviceName,
        Dns,
        GenericPropertyChange,
        IsConnected,
        JoinedNetwork,
        LeftNetwork,
        MTU,
        MacAddress,
        MulticastSubscriptions,
        NetconfRevision,
        NetworkId,
        NetworkList,
        NetworkName,
        NetworkStatus,
        NetworkType,
        PortError,
        Routes,
        UnexpectedShutdown,
        UserStatus
    }
}

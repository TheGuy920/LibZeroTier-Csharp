using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Globalization;

namespace LibZeroTier.CustomNetworkTypes
{
    class ZeroTierNetworkingAPI
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("clock", NullValueHandling = NullValueHandling.Ignore)]
        public long? Clock { get; set; }

        [JsonProperty("config", NullValueHandling = NullValueHandling.Ignore)]
        public Config Config { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("rulesSource", NullValueHandling = NullValueHandling.Ignore)]
        public string RulesSource { get; set; }

        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
        public string Permissions { get; set; }

        [JsonProperty("ownerId", NullValueHandling = NullValueHandling.Ignore)]
        public string? OwnerId { get; set; }

        [JsonProperty("onlineMemberCount", NullValueHandling = NullValueHandling.Ignore)]
        public long? OnlineMemberCount { get; set; }

        [JsonProperty("authorizedMemberCount", NullValueHandling = NullValueHandling.Ignore)]
        public long? AuthorizedMemberCount { get; set; }

        [JsonProperty("totalMemberCount", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalMemberCount { get; set; }

        [JsonProperty("capabilitiesByName", NullValueHandling = NullValueHandling.Ignore)]
        public SByName CapabilitiesByName { get; set; }

        [JsonProperty("tagsByName", NullValueHandling = NullValueHandling.Ignore)]
        public SByName TagsByName { get; set; }

        [JsonProperty("ui")]
        public object Ui { get; set; }
    }

    public partial class SByName
    {

    }

    public partial class Config
    {
        [JsonProperty("authTokens")]
        public object AuthTokens { get; set; }

        [JsonProperty("creationTime", NullValueHandling = NullValueHandling.Ignore)]
        public long? CreationTime { get; set; }

        [JsonProperty("capabilities", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Capabilities { get; set; }

        [JsonProperty("enableBroadcast", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EnableBroadcast { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("ipAssignmentPools", NullValueHandling = NullValueHandling.Ignore)]
        public List<IpAssignmentPool> IpAssignmentPools { get; set; }

        [JsonProperty("lastModified", NullValueHandling = NullValueHandling.Ignore)]
        public long? LastModified { get; set; }

        [JsonProperty("mtu", NullValueHandling = NullValueHandling.Ignore)]
        public long? Mtu { get; set; }

        [JsonProperty("multicastLimit", NullValueHandling = NullValueHandling.Ignore)]
        public long? MulticastLimit { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("private", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Private { get; set; }

        [JsonProperty("remoteTraceLevel", NullValueHandling = NullValueHandling.Ignore)]
        public long? RemoteTraceLevel { get; set; }

        [JsonProperty("remoteTraceTarget")]
        public object RemoteTraceTarget { get; set; }

        [JsonProperty("routes", NullValueHandling = NullValueHandling.Ignore)]
        public List<NetworkRoute> Routes { get; set; }

        [JsonProperty("rules", NullValueHandling = NullValueHandling.Ignore)]
        public List<Rule> Rules { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Tags { get; set; }

        [JsonProperty("v4AssignMode", NullValueHandling = NullValueHandling.Ignore)]
        public V4AssignMode V4AssignMode { get; set; }

        [JsonProperty("v6AssignMode", NullValueHandling = NullValueHandling.Ignore)]
        public V6AssignMode V6AssignMode { get; set; }

        [JsonProperty("dns", NullValueHandling = NullValueHandling.Ignore)]
        public Dns Dns { get; set; }
    }

    public partial class Dns
    {
        [JsonProperty("domain", NullValueHandling = NullValueHandling.Ignore)]
        public string Domain { get; set; }

        [JsonProperty("servers", NullValueHandling = NullValueHandling.Ignore)]
        public object Servers { get; set; }
    }

    public partial class IpAssignmentPool
    {
        [JsonProperty("ipRangeStart", NullValueHandling = NullValueHandling.Ignore)]
        public string IpRangeStart { get; set; }

        [JsonProperty("ipRangeEnd", NullValueHandling = NullValueHandling.Ignore)]
        public string IpRangeEnd { get; set; }
    }

    public partial class Rule
    {
        [JsonProperty("etherType", NullValueHandling = NullValueHandling.Ignore)]
        public long? EtherType { get; set; }

        [JsonProperty("not", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Not { get; set; }

        [JsonProperty("or", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Or { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }

    public partial class V4AssignMode
    {
        [JsonProperty("zt", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Zt { get; set; }
    }

    public partial class V6AssignMode
    {
        [JsonProperty("6plane", NullValueHandling = NullValueHandling.Ignore)]
        public bool? The6Plane { get; set; }

        [JsonProperty("rfc4193", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Rfc4193 { get; set; }

        [JsonProperty("zt", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Zt { get; set; }
    }

    internal class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
                {
                    new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
                },
        };
    }
}

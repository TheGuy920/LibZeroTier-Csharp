using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using LibZeroTier.CustomNetworkTypes;
using System.IO;

namespace LibZeroTier
{
    public class ZeroTierAPI
    {
        public string NetworkId;
        public APIHandler ZeroTierHandler;
        public HttpClient client = new HttpClient();
        public event EventHandler<NetworkChangedEventArgs> NetworkChangeEvent;
        public event EventHandler<string> LogNetworkInfoEvent;
        private bool IsAppAdministrator = false;
        public ZeroTierAPI(bool isAdmin = false)
        {
            IsAppAdministrator = isAdmin;
        }
        protected virtual void NetworkChange(NetworkChangedEventArgs details)
        {
            EventHandler<NetworkChangedEventArgs> handler = NetworkChangeEvent;
            handler?.Invoke(this, details);
        }
        public void ZeroTierHandler_NetworkChangeEvent(object sender, APIHandler.NetworkPropertyChangedEventArgs e)
        {
            this.NetworkChange(new NetworkChangedEventArgs()
            {
                Change = e.Change,
                Property = e.Property,
                Value = e.Value
            });
        }

        protected virtual void LogNetworkInfo(string message)
        {
            EventHandler<string> handler = LogNetworkInfoEvent;
            handler?.Invoke(this, message);
        }
        private void NetworkInfoLog(string message)
        {
            LogNetworkInfo(message);
        }
        public class NetworkChangedEventArgs : EventArgs
        {
            public StatusChange Change { get; set; }
            public string Property { get; set; }
            public object Value { get; set; }
        }
        public ZeroTierNetwork GetNetworkById(string id, List<ZeroTierNetwork> NetworkList)
        {
            foreach (var network in NetworkList)
            {
                if (network.NetworkId == id)
                {
                    return network;
                }
            }
            return null;
        }
        public string GetNetStatus()
        {
            try
            {
                return this.ZeroTierHandler.GetNetworks()[0].NetworkStatus;
            }
            catch
            {
                return "[ZeroTier] [API] API Handler Not Running!";
            }
        }
        public string GetZeroStatus()
        {
            try
            {
                return JsonConvert.SerializeObject(this.ZeroTierHandler.GetStatus());
            }
            catch
            {
                return "[ZeroTier] [API] API Handler Not Running!";
            }
        }
        public async Task StartServerAsync(string Network = null)
        {
            // get zero teir process(es) and kill em
            Process[] Zero = Process.GetProcessesByName("ZeroTier One");
            foreach (var item in Zero) { item.Kill(); item.WaitForExit(); }
            NetworkInfoLog("[ZeroTier] [API] Stopped all ZeroTier processes");
            NetworkInfoLog("[ZeroTier] [API] Locating...");
            // Delete all network history
            DeleteAllNonConnectedNetworks();
            // start zero teir
            Process proccess = new Process();
            proccess.StartInfo.FileName = @"C:\Program Files (x86)\ZeroTier\One\ZeroTier One.exe";
            proccess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proccess.StartInfo.CreateNoWindow = true;
            proccess.Start();
            // log sum info
            NetworkInfoLog("[ZeroTier] [API] Initialized!");
            NetworkInfoLog("[ZeroTier] [API] Loading API...");
            // instantiate the API manager if it has not been done already
            if (ZeroTierHandler == null)
            {
                ZeroTierHandler = new APIHandler(IsAppAdministrator);
                ZeroTierHandler.AddEventHandler(ZeroTierHandler_NetworkChangeEvent);
                NetworkInfoLog("[ZeroTier] [API] API Created!");
            }
            else
            {
                NetworkInfoLog("[ZeroTier] [API] API Loaded!");
            }
            // if there is a network provided, dont create one
            if (Network == null)
                Network = await CreateNewNetwork();
            NetworkInfoLog("[ZeroTier] [API-NET] P2P Network Created!");
            // leave any joined networks
            foreach (var LocalNet in ZeroTierHandler.GetNetworks())
                ZeroTierHandler.LeaveNetwork(LocalNet.NetworkId);
            // make sure the network is added, and joined properly
            bool Connected = false;
            while (!Connected)
            {
                ZeroTierHandler.JoinNetwork(Network);
                foreach (var LocalNet in ZeroTierHandler.GetNetworks())
                    if (LocalNet.NetworkId == Network && LocalNet.IsConnected == true && LocalNet.NetworkStatus.Equals("OK"))
                        Connected = true;
            }
            NetworkId = Network;
            NetworkInfoLog("[ZeroTier] [Network] Joined Network: " + NetworkId);
            NetworkInfoLog("[ZeroTier] [Network] P2P Connection Established!");
        }
        public async Task JoinServerAsync(string NetworkId)
        {
            await StartServerAsync(NetworkId);
        }
        public void LeaveAllServers()
        {
            foreach (var LocalNet in this.ZeroTierHandler.GetNetworks())
                this.ZeroTierHandler.LeaveNetwork(LocalNet.NetworkId);
        }
        public async Task StopServerAsync(bool DeleteWebServer = true)
        {
            if(this.ZeroTierHandler != null)
                this.ZeroTierHandler.IsCheckingForUpdates = false;
            string netId = this.NetworkId;
            // leave all servers
            LeaveAllServers();
            // get zero teir process(es) and kill em
            Process[] Zero = Process.GetProcessesByName("ZeroTier One");
            foreach (var item in Zero) { item.Kill(); item.WaitForExit(); }
            NetworkInfoLog("[ZeroTier] [API] Stopped all ZeroTier processes");
            // delete network on website
            if(DeleteWebServer)
                await DeleteNetwork(netId);
            NetworkInfoLog("[ZeroTier] [API-NET] Deleted P2P Network");
            // Delete all network history
            DeleteAllNonConnectedNetworks();
            NetworkInfoLog("[ZeroTier] [Network] Deleted Network History");
            NetworkInfoLog("[ZeroTier] [Network] P2P Connection Removed");
            this.ZeroTierHandler = null;
        }
        public async Task DeleteAllNetworks()
        {
            // setup headers
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", "XlOrMp71uEQdxFT1D0bzjkkBjJCOCbvA");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // get network list
            var Get = client.GetAsync("https://my.zerotier.com/api/network");
            string content = await Get.Result.Content.ReadAsStringAsync();
            var temp = JObject.Parse("{ \"Array\":" + content + "}");
            // delete all networks
            foreach (var item in temp["Array"])
            {
                var netid = item["id"].ToString();
                await DeleteNetwork(netid);
            }
        }
        public async Task UpdateNetworkDescription(string NetworkId, string Description)
        {
            // setup headers
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", "XlOrMp71uEQdxFT1D0bzjkkBjJCOCbvA");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // get network list
            var Get = client.GetAsync("https://my.zerotier.com/api/network");
            string content = await Get.Result.Content.ReadAsStringAsync();
            var temp = JObject.Parse("{ \"Array\":" + content + "}");
            // find and update network
            foreach (var item in temp["Array"])
            {
                var netid = item["id"].ToString();
                if (netid == NetworkId)
                {
                    await CreateNewNetwork("Multiplayer Server", Description, netid);
                }
            }
        }
        private void DeleteAllNonConnectedNetworks()
        {
            // delete save file of network history
            string AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string NetworksFile = Path.Combine(AppData, "ZeroTier", "One", "Networks.dat");
            File.Delete(NetworksFile);
        }

        private async Task<string> CreateNewNetwork(string NetworkName = "Multiplayer Server", string NetworkDescription = "", string NetworkId = "")
        {
            try
            {
                bool IsNewNet = true;
                // setup headers
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", "XlOrMp71uEQdxFT1D0bzjkkBjJCOCbvA");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // get network list
                var Get = client.GetAsync("https://my.zerotier.com/api/network");
                string content = await Get.Result.Content.ReadAsStringAsync();
                var temp = JObject.Parse("{ \"Array\":" + content + "}");
                List<string> NetworkIds = new List<string>();
                // append network names to list
                foreach (var item in temp["Array"])
                {
                    var netid = item["id"].ToString();
                    NetworkIds.Add(netid);
                }
                // if net id is empty, create one
                if (string.IsNullOrWhiteSpace(NetworkId))
                {
                    // generate new network id until the network id is unique
                    while (NetworkIds.Contains(NetworkId) || NetworkId.Length < 16)
                    {
                        NetworkId = Math.Floor((new Random().NextDouble() + 0.01) * Math.Pow(10, 16)).ToString();
                    }
                }
                else
                {
                    IsNewNet = false;
                }
                // setup new network json post
                long CurrentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                // setup last access time if null
                if (string.IsNullOrWhiteSpace(NetworkDescription))
                    NetworkDescription = "Last Accessed: " + CurrentTime;
                ZeroTierNetworkingAPI net = new ZeroTierNetworkingAPI()
                {
                    Id = NetworkId,
                    Type = "Network",
                    Clock = 1608235389104,
                    Config = new Config()
                    {
                        AuthTokens = null,
                        CreationTime = CurrentTime,
                        Capabilities = new List<object>(),
                        EnableBroadcast = true,
                        Id = NetworkId,
                        IpAssignmentPools = new List<IpAssignmentPool>()
                        {
                            new IpAssignmentPool()
                            {
                                IpRangeStart = "10.10.10.0",
                                IpRangeEnd = "10.10.10.254"
                            }
                        },
                        LastModified = CurrentTime,
                        Mtu = 2800,
                        MulticastLimit = 32,
                        Name = NetworkName,
                        Private = false,
                        RemoteTraceLevel = 0,
                        RemoteTraceTarget = null,
                        Routes = new List<NetworkRoute>()
                        {
                            new NetworkRoute()
                            {
                                Target = "10.10.10.0/24",
                                Via = "10.10.10.1"
                            }
                        },
                        Rules = new List<Rule>()
                        {
                            new Rule()
                            {
                                EtherType = 2048,
                                Not = true,
                                Or = false,
                                Type = "MATCH_ETHERTYPE"
                            },
                            new Rule()
                            {
                                EtherType = 2054,
                                Not = true,
                                Or = false,
                                Type = "MATCH_ETHERTYPE"
                            },
                            new Rule()
                            {
                                EtherType = 34525,
                                Not = true,
                                Or = false,
                                Type = "MATCH_ETHERTYPE"
                            },
                            new Rule()
                            {
                                Type = "ACTION_DROP"
                            },
                            new Rule()
                            {
                                Type = "ACTION_ACCEPT"
                            }
                        },
                        Tags = new List<object>(),
                        V4AssignMode = new V4AssignMode()
                        {
                            Zt = true
                        },
                        V6AssignMode = new V6AssignMode()
                        {
                            The6Plane = false,
                            Rfc4193 = false,
                            Zt = false
                        },
                        Dns = new Dns()
                        {
                            Domain = "",
                            Servers = null
                        }
                    },
                    Description = NetworkDescription,
                    RulesSource = "",
                    Permissions = null,
                    OwnerId = "56e2eba4-db42-4082-9456-fe22051d54b8",
                    OnlineMemberCount = 0,
                    AuthorizedMemberCount = null,
                    TotalMemberCount = 0,
                    CapabilitiesByName = new SByName(),
                    TagsByName = new SByName(),
                    Ui = null
                };
                Task<HttpResponseMessage> res = null;
                if (IsNewNet)
                {
                    // post new network
                    res = client.PostAsync("https://my.zerotier.com/api/network", new StringContent(JsonConvert.SerializeObject(net, Converter.Settings)));
                }
                else
                {
                    // update network
                    res = client.PostAsync("https://my.zerotier.com/api/network/" + NetworkId, new StringContent(JsonConvert.SerializeObject(net, Converter.Settings)));
                }
                // deal with network errors or network creation success
                try
                {
                    res.Result.EnsureSuccessStatusCode();
                    var netId = JObject.Parse(res.Result.Content.ReadAsStringAsync().Result);
                    NetworkId = netId["id"].ToString();
                    if(IsNewNet)
                        NetworkInfoLog("[ZeroTier] [WebApi] Network Creation Success");
                    else
                        NetworkInfoLog("[ZeroTier] [WebApi] Network Update Success");
                }
                catch (Exception ex)
                {
                    NetworkInfoLog("[ZeroTier] [WebApi] [Error] " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                // log network, or json/data handling errors
                NetworkInfoLog("[ZeroTier] [WebApi] [Error] " +  ex.ToString());
            }
            return NetworkId;
        }
        public async Task<bool> IsValidNetwork(string NetworkId)
        {
            // setup headers
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", "XlOrMp71uEQdxFT1D0bzjkkBjJCOCbvA");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // get network list
            var Get = client.GetAsync("https://my.zerotier.com/api/network");
            string content = await Get.Result.Content.ReadAsStringAsync();
            var temp = JObject.Parse("{ \"Array\":" + content + "}");
            // find network
            foreach (var item in temp["Array"])
                if (item["id"].ToString() == NetworkId)
                    return true;
            return false;
        }
        private async Task DeleteNetwork(string NetworkId)
        {
            // setup headers
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", "XlOrMp71uEQdxFT1D0bzjkkBjJCOCbvA");
            // delete network
            await client.DeleteAsync("https://my.zerotier.com/api/network/" + NetworkId);
        }
    }
}


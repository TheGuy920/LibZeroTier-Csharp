using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace LibZeroTier
{
    public class APIHandler
    {
        private static string authtoken;

        private static string url = null;
        public bool IsCheckingForUpdates = false;
        private static List<ZeroTierNetwork> Networks;
        public bool UseStandardSerialize = true;

        public delegate void NetworkListCallback(List<ZeroTierNetwork> networks);
        public delegate void StatusCallback(ZeroTierStatus status);

        private string ZeroTierAddress;
        public event NetworkPropertyChangedEventHandler NetworkChangeEvent;
        public delegate void NetworkPropertyChangedEventHandler(object sender, NetworkPropertyChangedEventArgs e);

        public virtual void NetworkPropertyChange(object sender, PropertyChangedEventArgs e, object value)
        {
            StatusChange change = StatusChange.GenericPropertyChange;
            try { change = (StatusChange)sender; } catch { }
            NetworkPropertyChangedEventHandler handler = NetworkChangeEvent;
            handler?.Invoke(this, new NetworkPropertyChangedEventArgs()
            {
                Change = change,
                Property = e.PropertyName,
                Value = value
            });
        }
        public void AddEventHandler(NetworkPropertyChangedEventHandler eventHandler)
        {
            this.NetworkChangeEvent -= eventHandler;
            this.NetworkChangeEvent += eventHandler;
        }
        public class NetworkPropertyChangedEventArgs : EventArgs
        {
            public StatusChange Change { get; set; }
            public string Property { get; set; }
            public object Value { get; set; }
        }
        /// <summary>
        /// Checks whether a specific network ID is connected on your system.
        /// </summary>
        /// <param name="id">Hexadecimal ID of the network you're looking for</param>
        /// <returns>Returns the network if the ID exists in the network list, null if not</returns>
        private ZeroTierNetwork findNetwork(string id, List<ZeroTierNetwork> currentNetworks)
        {
            if (GetStatus().Online)
            {
                foreach (var i in currentNetworks)
                {
                    if (i.NetworkId == id)
                    {
                        return i;
                    }
                }

                return null;
            }
            else
            {
                throw new LibZeroTierException("ZeroTier appears to be offline.");
            }
        }
        private void UpdateNetworkList()
        {
            Task.Factory.StartNew(() =>
            {
                this.IsCheckingForUpdates = true;
                while (this.IsCheckingForUpdates)
                {
                    NetworkPropertyChange(null, null, "UPDATE");
                    Task.Delay(500).Wait();
                    if (!this.IsCheckingForUpdates)
                        return;
                    List<ZeroTierNetwork> NewNets = GetNetworks();
                    int count;

                    if (Networks == null)
                        Networks = NewNets;
                    if (Networks.Count > NewNets.Count)
                        count = Networks.Count;
                    else
                        count = NewNets.Count;

                    for (int i = count - 1; i >= 0; i--)
                    {
                        if ((Networks.Count-1) >= i)
                        {

                            Networks[i].AddEventHandler(NetworkPropertyChange);

                            if (findNetwork(Networks[i].NetworkId, NewNets) == null)
                            {
                                NetworkPropertyChange(StatusChange.NetworkList, new PropertyChangedEventArgs("Network Removed"), Networks[i].NetworkId);
                                Networks.Remove(Networks[i]);
                            }
                        }
                        if ((NewNets.Count - 1) >= i)
                        {

                            findNetwork(NewNets[i].NetworkId, Networks)?.UpdateNetwork(NewNets[i]);

                            if (findNetwork(NewNets[i].NetworkId, Networks) == null)
                            {
                                Networks.Add(NewNets[i]);
                                NetworkPropertyChange(StatusChange.NetworkList, new PropertyChangedEventArgs("Network Added"), Networks[i].NetworkId);
                            }
                        }
                    }
                }
            });
        }

        private bool initHandler(bool resetToken = false)
        {
            string localZtDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ZeroTier\\One";
            string globalZtDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\ZeroTier\\One";

            if (resetToken)
            {
                if (File.Exists(localZtDir + "\\authtoken.secret"))
                {
                    File.Delete(localZtDir + "\\authtoken.secret");
                }

                if (File.Exists(localZtDir + "\\zerotier-one.port"))
                {
                    File.Delete(localZtDir + "\\zerotier-one.port");
                }
            }

            if (!File.Exists(localZtDir + "\\authtoken.secret") || !File.Exists(localZtDir + "\\zerotier-one.port"))
            {
                string curPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\ZeroTier\\One";
                ProcessStartInfo startInfo = new ProcessStartInfo(curPath + "\\copyutil.exe", "\"" + globalZtDir + "\"" + " " + "\"" + localZtDir + "\"")
                {
                    Verb = "runas",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                Process process = Process.Start(startInfo);
                process.WaitForExit();
            }

            string authToken = readAuthToken(localZtDir + "\\authtoken.secret");

            if ((authToken == null) || (authToken.Length <= 0))
            {
                throw new LibZeroTierException("Unable to read ZeroTier One authtoken");
            }

            int port = readPort(localZtDir + "\\zerotier-one.port");
            setVars(port, authToken);

            return true;
        }

        private static void setVars(int port, string auth)
        {
            url = "http://127.0.0.1:" + port;
            authtoken = auth;
        }

        private static string readAuthToken(string path)
        {
            string authToken = "";
            if (File.Exists(path))
            {
                try
                {
                    byte[] tmp = File.ReadAllBytes(path);
                    authToken = Encoding.UTF8.GetString(tmp).Trim();
                }
                catch
                {
                    throw new LibZeroTierException("Unable to read ZeroTier One Auth Token from:\r\n" + path);
                }
            }

            return authToken;
        }

        private static int readPort(string path)
        {
            int port = 9993;
            try
            {
                byte[] tmp = File.ReadAllBytes(path);
                port = int.Parse(Encoding.ASCII.GetString(tmp).Trim());
                if ((port <= 0) || (port > 65535))
                    port = 9993;
            } catch { }
            return port;
        }


        public APIHandler(bool resetAuthToken = false)
        {
            NetworkPropertyChange(null, null, "INITIALIZATION");
            url = "http://127.0.0.1:9993";
            initHandler(resetAuthToken);
            UpdateNetworkList();
        }

        public APIHandler(int port, string authToken)
        {
            NetworkPropertyChange(null, null, "INITIALIZATION");
            url = "http://127.0.0.1:" + port;
            authtoken = authToken;
            UpdateNetworkList();
        }


        /// <summary>
        /// Gets the status response from the ZeroTier service.
        /// </summary>
        /// <returns></returns>
        public ZeroTierStatus GetStatus()
        {
            HttpWebRequest request = WebRequest.Create(url + "/status" + "?auth=" + authtoken) as HttpWebRequest;
            request.Headers.Add("X-ZT1-Auth",authtoken);
            if (request != null)
            {
                request.Method = "GET";
                request.ContentType = "application/json";
            }

            try
            {
                var httpResponse = (HttpWebResponse)request.GetResponse();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    using StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream());
                    var responseText = streamReader.ReadToEnd();

                    ZeroTierStatus status = null;
                    try
                    {
                        try
                        {
                            status = JsonConvert.DeserializeObject<ZeroTierStatus>(responseText);
                        }
                        catch { }

                        if (ZeroTierAddress != status.Address)
                        {
                            ZeroTierAddress = status.Address;
                        }
                    }
                    catch (JsonReaderException e)
                    {
                        Debug.WriteLine(e.ToString());
                    }

                    return status;
                }
                else if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    initHandler(true);
                    return null;
                }

            }
            catch (System.Net.Sockets.SocketException ex)
            {
                throw new LibZeroTierException("ZeroTier Exception:", ex);
            }
            catch (WebException e)
            {
                HttpWebResponse res = (HttpWebResponse)e.Response;
                if (res != null && res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    initHandler(true);
                    return null;
                }
                else
                {
                    throw new LibZeroTierException("ZeroTier Exception:", e);
                }
            }

            return null;
        }



        /// <summary>
        /// Returns the current networks you're connected to.
        /// </summary>
        /// <returns>List of ZeroTierNetwork objects</returns>
        public List<ZeroTierNetwork> GetNetworks()
        {
            NetworkPropertyChange(null, null, "BEGIN");
            HttpWebRequest request = WebRequest.Create(url + "/network" + "?auth=" + authtoken) as HttpWebRequest;
            if (request == null)
            {
                throw new LibZeroTierException("ZeroTier Request Response Empty");
            }

            request.Method = "GET";
            request.ContentType = "application/json";
            request.Timeout = 10000;
            NetworkPropertyChange(null, null, "SECTION 2");
            try
            {
                var httpResponse = (HttpWebResponse)request.GetResponse();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    using StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream());
                    var responseText = streamReader.ReadToEnd();
                    List<ZeroTierNetwork> networkList = new List<ZeroTierNetwork>();
                    NetworkPropertyChange(null, null, responseText);
                    if (UseStandardSerialize)
                    {
                        networkList = JsonConvert.DeserializeObject<List<ZeroTierNetwork>>(responseText);
                    }
                    else
                    {
                        networkList.Clear();
                        foreach (var item in JArray.Parse(responseText))
                        {
                            networkList.Add(SerialIzeJsonToNet.Serialize(JObject.Parse(item.ToString())));
                        }
                    }
                    return networkList;
                }
                else if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    initHandler(true);
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                throw new LibZeroTierException("ZeroTier Request Response Empty");
            }
            catch (WebException e)
            {
                HttpWebResponse res = (HttpWebResponse)e.Response;
                if (res != null && res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    initHandler(true);
                }
                else
                {
                    throw new LibZeroTierException("ZeroTier Request Response Empty");
                }
            }
            return null;
        }

        /// <summary>
        /// Sends the request to join a network
        /// </summary>
        /// <param name="nwid">Hexadecimal Network ID</param>
        /// <param name="allowManaged">Specify if it's managed</param>
        /// <param name="allowGlobal">Specify if it's global</param>
        /// <param name="allowDefault">Specify if it's allowed to be the default route</param>
        public void JoinNetwork(string nwid, bool allowManaged = true, bool allowGlobal = false, bool allowDefault = false)
        {
            Task.Factory.StartNew(() =>
            {
                HttpWebRequest request = WebRequest.Create(url + "/network/" + nwid + "?auth=" + authtoken) as HttpWebRequest;
                if (request == null)
                {
                    return;
                }

                request.Method = "POST";
                request.ContentType = "applicaiton/json";
                request.Timeout = 30000;
                try
                {
                    using (var streamWriter = new StreamWriter((request).GetRequestStream()))
                    {
                        string json = "{\"allowManaged\":" + (allowManaged ? "true" : "false") + "," +
                                "\"allowGlobal\":" + (allowGlobal ? "true" : "false") + "," +
                                "\"allowDefault\":" + (allowDefault ? "true" : "false") + "}";
                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }
                catch (WebException)
                {
                    throw new LibZeroTierException("Error Joining Network: Cannot connect to ZeroTier service.");
                }

                try
                {
                    var httpResponse = (HttpWebResponse)request.GetResponse();

                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        initHandler(true);
                    }
                    else if (httpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new LibZeroTierException("Error sending join network message");
                    }
                }
                catch (System.Net.Sockets.SocketException)
                {
                    throw new LibZeroTierException("Error Joining Network: Cannot connect to ZeroTier service.");
                }
                catch (WebException e)
                {
                    HttpWebResponse res = (HttpWebResponse)e.Response;
                    if (res != null && res.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        initHandler(true);
                    }
                    throw new LibZeroTierException("Error Joining Network: Cannot connect to ZeroTier service.");
                }
            });
        }

        /// <summary>
        /// Leaves a specified network
        /// </summary>
        /// <param name="nwid">Hexadecimal network ID</param>
        public void LeaveNetwork(string nwid)
        {
            Task.Factory.StartNew(() =>
            {
                var request = WebRequest.Create(url + "/network/" + nwid + "?auth=" + authtoken) as HttpWebRequest;
                if (request == null)
                {
                    return;
                }

                request.Method = "DELETE";
                request.Timeout = 30000;

                try
                {
                    var httpResponse = (HttpWebResponse)request.GetResponse();

                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        initHandler(true);
                    }
                    else if (httpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        Debug.WriteLine("Error sending leave network message");
                    }
                }
                catch (System.Net.Sockets.SocketException)
                {
                    throw new LibZeroTierException("Error Leaving Network: Cannot connect to ZeroTier service.");
                }
                catch (WebException e)
                {
                    HttpWebResponse res = (HttpWebResponse)e.Response;
                    if (res != null && res.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        initHandler(true);
                    }
                    throw new LibZeroTierException("Error Leaving Network: Cannot connect to ZeroTier service.");
                }
                catch
                {
                    Debug.WriteLine("Error leaving network: Unknown error");
                }
            });
        }

        //public delegate void PeersCallback(List<ZeroTierPeer> peers);

        /*public void GetPeers(PeersCallback cb)
        {
            var request = WebRequest.Create(url + "/peer" + "?auth=" + authtoken) as HttpWebRequest;
            if (request == null)
            {
                cb(null);
            }

            request.Method = "GET";
            request.ContentType = "application/json";

            try
            {
                var httpResponse = (HttpWebResponse)request.GetResponse();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var responseText = streamReader.ReadToEnd();
                        //Console.WriteLine(responseText);
                        List<ZeroTierPeer> peerList = null;
                        try
                        {
                            peerList = JsonConvert.DeserializeObject<List<ZeroTierPeer>>(responseText);
                        }
                        catch (JsonReaderException e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        cb(peerList);
                    }
                }
                else if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    APIHandler.initHandler(true);
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                cb(null);
            }
            catch (System.Net.WebException e)
            {
                HttpWebResponse res = (HttpWebResponse)e.Response;
                if (res != null && res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    APIHandler.initHandler(true);
                }
                else
                {
                    cb(null);
                }
            }
        }*/

        public string NodeAddress()
        {
            return ZeroTierAddress;
        }
    }
}

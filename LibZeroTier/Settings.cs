using System;
using System.Collections.Generic;
using System.Text;

namespace LibZeroTier
{
    public class API_Settings
    {
        public string Web_API_Key { get; set; }
        public string Internal_Id { get; set; }

    }
    public class Network_Settings
    {
        public string IP_Address_Prefix { get; set; }
        public string Network_Name { get; set; }
        public string Network_Description { get; set; }
        public Security Network_Privacy { get; set; }
        public string Network_Id { get; set; }
    }
    public enum Security
    {
        Public,
        Private
    }
}

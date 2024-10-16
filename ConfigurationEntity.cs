using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network_Configuration_Switching_Tool
{
    public class ConfigurationEntity
    {
        public string Ipv4Address
        {
            get; set;
        }
        public string Ipv4Mask
        {
            get; set; 
        }
        public string Ipv4Gateway
        {
            get; set;
        }
        public string Ipv4DNSserver
        {
            get; set;
        }
    }
}

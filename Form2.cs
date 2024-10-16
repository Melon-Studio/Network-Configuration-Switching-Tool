using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Network_Configuration_Switching_Tool
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!IsValidIPAddress(textBox1.Text))
            {
                MessageBox.Show("IP 地址不合法");
                return;
            }
            if (!IsValidSubnetMask(textBox2.Text))
            {
                MessageBox.Show("子网掩码不合法");
                return;
            }
            if (!IsValidIPAddress(textBox3.Text))
            {
                MessageBox.Show("网关不合法");
                return;
            }
            if (!IsValidIPAddress(textBox4.Text)) 
            { 
                MessageBox.Show("DNS 不合法"); 
                return;
            }
            var data = new ConfigurationEntity();
            data.Ipv4Address = textBox1.Text;
            data.Ipv4Mask = textBox2.Text;
            data.Ipv4Gateway = textBox3.Text;
            data.Ipv4DNSserver = textBox4.Text;

            ConfigXmlHandler.AddItem(data);
            this.Close();
        }

        private static bool IsValidIPAddress(string ip)
        {
            string pattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            if (!Regex.IsMatch(ip, pattern))
                return false;

            string[] parts = ip.Split('.');
            if (int.Parse(parts[0]) == 127) return false; // Loop address
            if (int.Parse(parts[0]) >= 224 && int.Parse(parts[0]) <= 239) return false; // Multicast address
            if (int.Parse(parts[0]) == 0) return false; // Special

            return true;
        }

        private static bool IsValidSubnetMask(string subnetMask)
        {
            string pattern = @"^(255|254|252|248|240|224|192|128|0)\.(255|254|252|248|240|224|192|128|0)\.(255|254|252|248|240|224|192|128|0)\.(255|254|252|248|240|224|192|128|0)$";
            if (!Regex.IsMatch(subnetMask, pattern))
                return false;

            string[] parts = subnetMask.Split('.');
            int[] maskParts = new int[4];
            for (int i = 0; i < 4; i++)
            {
                maskParts[i] = int.Parse(parts[i]);
            }

            bool isContinuousOnes = false;
            int onesCount = 0;
            for (int i = 0; i < 32; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                if ((maskParts[byteIndex] & (1 << (7 - bitIndex))) != 0)
                {
                    onesCount++;
                    isContinuousOnes = true;
                }
                else
                {
                    if (isContinuousOnes)
                        break;
                }
            }

            return onesCount > 0 && onesCount < 32 && isContinuousOnes;
        }
    }
}

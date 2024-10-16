using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Management;
using System.Net;

namespace Network_Configuration_Switching_Tool
{
    public partial class Form1 : Form
    {
        private static string ApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "NetworkConfigurationSwitchingTool";
        private static string ConfigFile = ApplicationData + "\\config.xml";
        List<NetworkInterface> adapterList = new List<NetworkInterface>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Table init
            InitDataGridView();
            // Get Network adapter information
            GetAllDeviceNetworkAdapterInfo();
            // Get IPv4 configuration
            if (!Directory.Exists(ApplicationData))
            {
                Directory.CreateDirectory(ApplicationData);
            }

            if (!File.Exists(ConfigFile))
            {
                File.Create(ConfigFile);
            }

            try
            {
                ConfigXmlHandler.ReadItems();
            }
            catch
            {
                ConfigXmlHandler.InitializeXml();
            }
            // Load data
            LoadData();
        }

        private void LoadData() 
        {
            while (this.dataGridView1.Rows.Count != 0)
            {
                this.dataGridView1.Rows.RemoveAt(0);
            }
            List<ConfigurationEntity> list = ConfigXmlHandler.ReadItems();
            foreach (var item in list)
            {
                string[] rowData = { item.Ipv4Address, item.Ipv4Mask, item.Ipv4Gateway, item.Ipv4DNSserver };
                dataGridView1.Rows.Add(rowData.ToArray());
            }
        }

        private void InitDataGridView()
        {
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.BorderStyle = BorderStyle.FixedSingle;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            string[] header = { "IP 地址", "子网掩码", "网关", "DNS"};
            string[] column_name = { "Ipv4Address", "Ipv4Mask", "Ipv4Gateway", "Ipv4DNSserver" };

            dataGridView1.Columns.AddRange(header.Select((h, index) => new DataGridViewTextBoxColumn
            {
                HeaderText = h,
                Name = column_name[index]
            }).ToArray());
        }

        private void GetAllDeviceNetworkAdapterInfo()
        {
            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            
            if (nics == null || nics.Length < 1)
            {
                label3.Text += 0 + "";
                comboBox1.Enabled = false;
                button3.Enabled = false;
                return;
            }

            label3.Text += nics.Length;

            foreach (NetworkInterface adapter in nics)
            {
                adapterList.Add(adapter);
            }

            comboBox1.DataSource = adapterList;
            comboBox1.DisplayMember = "Description";
            comboBox1.ValueMember = "Description";
            comboBox1.SelectedIndex = 0;
        }

        private void ConfigureNetworkAdapter(string adapterName, ConfigurationEntity config)
        {
            try
            {
                ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = wmi.GetInstances();
                ManagementBaseObject inPar = null;
                ManagementBaseObject outPar = null;
                foreach (ManagementObject obj in moc)
                {
                    if (obj["Description"].Equals(adapterName))
                    {
                        if (!(bool)obj["IPEnabled"])
                        {
                            MessageBox.Show("该适配器禁用了 IP 配置，已取消配置。");
                        }

                        if (config.Ipv4Address != null && config.Ipv4Mask != null)
                        {
                            inPar = obj.GetMethodParameters("EnableStatic");
                            inPar["IPAddress"] = new string[] {config.Ipv4Address };
                            inPar["SubnetMask"] = new string[] {config.Ipv4Mask };
                            outPar = obj.InvokeMethod("EnableStatic", inPar, null);
                        }
                        if (config.Ipv4Gateway != null)
                        {
                            inPar = obj.GetMethodParameters("SetGateways");
                            inPar["DefaultIPGateway"] = new string[] {config.Ipv4Gateway };
                            outPar = obj.InvokeMethod("SetGateways", inPar, null);
                        }
                        if (config.Ipv4DNSserver != null)
                        {
                            inPar = obj.GetMethodParameters("SetDNSServerSearchOrder");
                            inPar["DNSServerSearchOrder"] = new string[] {config.Ipv4DNSserver };
                            outPar = obj.InvokeMethod("SetDNSServerSearchOrder", inPar, null);
                        }

                        MessageBox.Show("已配置选择的网络配置到对应的适配器。");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("发生错误：" + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dialog = new Form2();
            dialog.ShowDialog();
            LoadData();
            dialog.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult MsgBoxResult;
            MsgBoxResult = MessageBox.Show("你确定要删除吗？", 
            "提示",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Exclamation,
            MessageBoxDefaultButton.Button2);
            if (MsgBoxResult == DialogResult.Yes)
            {
                ConfigXmlHandler.DeleteItemByIndex(dataGridView1.CurrentRow.Index);
                LoadData();
            }
            if (MsgBoxResult == DialogResult.No)
            {
                return;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var config = new ConfigurationEntity();
            config.Ipv4Address = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            config.Ipv4Mask = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            config.Ipv4Gateway = dataGridView1.SelectedRows[0].Cells[2].Value.ToString();
            config.Ipv4DNSserver = dataGridView1.SelectedRows[0].Cells[3].Value.ToString();
            ConfigureNetworkAdapter(comboBox1.SelectedValue.ToString(), config);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(ApplicationData);
        }

        private void githubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Melon-Studio/Network-configuration-switching-tool");
        }

        private void poJieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.52pojie.cn/home.php?mod=space&uid=1214056");
        }

        private void label4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Melon-Studio");
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Copyright © 2024-present Melon-Studio All Rights Reserved. Released under the GPLv3 License.");
        }
    }
}

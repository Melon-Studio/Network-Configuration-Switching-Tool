using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Network_Configuration_Switching_Tool
{
    public static class ConfigXmlHandler
    {
        private static string ApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "NetworkConfigurationSwitchingTool";
        private static string filePath = ApplicationData + "\\config.xml";

        public static List<ConfigurationEntity> ReadItems()
        {
            if (!File.Exists(filePath))
            {
                return new List<ConfigurationEntity>();
            }

            XDocument doc = XDocument.Load(filePath);
            return doc.Descendants("item")
               .Select(item => new ConfigurationEntity
               {
                   Ipv4Address = item.Element("ipv4Address")?.Value,
                   Ipv4Mask = item.Element("ipv4Mask")?.Value,
                   Ipv4Gateway = item.Element("ipv4Gateway")?.Value,
                   Ipv4DNSserver = item.Element("ipv4DNSserver")?.Value,
                   Remark = item.Element("remark")?.Value
               })
               .ToList();
        }

        public static void WriteItems(List<ConfigurationEntity> items)
        {
            XDocument doc = new XDocument(
                new XElement("network",
                    items.Select(item => new XElement("item",
                        new XElement("ipv4Address", item.Ipv4Address),
                        new XElement("ipv4Mask", item.Ipv4Mask),
                        new XElement("ipv4Gateway", item.Ipv4Gateway),
                        new XElement("ipv4DNSserver", item.Ipv4DNSserver),
                        new XElement("remark", item.Remark)
                    ))
                )
            );

            doc.Save(filePath);
        }

        public static void AddItem(ConfigurationEntity item)
        {
            List<ConfigurationEntity> items = ReadItems();
            items.Add(item);
            WriteItems(items);
        }

        public static void InitializeXml()
        {
            XDocument doc = new XDocument(
                new XElement("network")
            );
            doc.Save(filePath);
        }

        public static void DeleteItemByIndex(int index)
        {
            List<ConfigurationEntity> items = ReadItems();
            if (index >= 0 && index < items.Count)
            {
                items.RemoveAt(index);
                WriteItems(items);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index), "索引超出范围。");
            }
        }

        public static void ModifyItemByIndex(int index, ConfigurationEntity item)
        {
            List<ConfigurationEntity> items = ReadItems();
            if (index >= 0 && index < items.Count)
            {
                items[index] = item;
                WriteItems(items);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index), "索引超出范围。");
            }
        }
    }
}

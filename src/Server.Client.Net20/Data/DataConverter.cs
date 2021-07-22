
using System;
using System.IO;
using System.Xml;

namespace Gibraltar.Server.Client.Data
{
    /// <summary>
    /// Convert between data representations of common repository objects
    /// </summary>
    internal static class DataConverter
    {
        private const string LogCategory = "Loupe.Server.Data";

        /// <summary>
        /// Convert a byte array to a Hub Configuration XML object without relying on Xml Serializer
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static HubConfigurationXml ByteArrayToHubConfigurationXml(byte[] rawData)
        {
            HubConfigurationXml configurationXml = new HubConfigurationXml();

            using (MemoryStream documentStream = new MemoryStream(rawData))
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(documentStream);
                //            xml.LoadXml(Encoding.UTF8.GetString(rawData));

                XmlNode hubConfigurationNode = xml.DocumentElement;

                if (hubConfigurationNode == null)
                {
                    throw new GibraltarException("There is no server configuration data in the provided raw data");
                }

                //read up our attributes
                XmlAttribute attribute = hubConfigurationNode.Attributes["id"];
                if (attribute != null)
                {
                    configurationXml.id = attribute.InnerText;
                }

                attribute = hubConfigurationNode.Attributes["redirectRequested"];
                if (attribute != null)
                {
                    if (string.IsNullOrEmpty(attribute.InnerText) == false)
                    {
                        configurationXml.redirectRequested = bool.Parse(attribute.InnerText);
                    }
                }

                attribute = hubConfigurationNode.Attributes["status"];
                if (attribute != null)
                {
                    if (string.IsNullOrEmpty(attribute.InnerText) == false)
                    {
                        configurationXml.status = (HubStatusXml)Enum.Parse(typeof(HubStatusXml), attribute.InnerText, true);
                    }
                }

                attribute = hubConfigurationNode.Attributes["timeToLive"];
                if (attribute != null)
                {
                    if (string.IsNullOrEmpty(attribute.InnerText) == false)
                    {
                        configurationXml.timeToLive = int.Parse(attribute.InnerText);
                    }
                }

                attribute = hubConfigurationNode.Attributes["protocolVersion"];
                if (attribute != null)
                {
                    configurationXml.protocolVersion = attribute.InnerText;
                }

                //we only read redirect information if we actually got a redirect request.
                if (configurationXml.redirectRequested)
                {
                    attribute = hubConfigurationNode.Attributes["redirectApplicationBaseDirectory"];
                    if (attribute != null)
                    {
                        configurationXml.redirectApplicationBaseDirectory = attribute.InnerText;
                    }

                    attribute = hubConfigurationNode.Attributes["redirectCustomerName"];
                    if (attribute != null)
                    {
                        configurationXml.redirectCustomerName = attribute.InnerText;
                    }

                    attribute = hubConfigurationNode.Attributes["redirectHostName"];
                    if (attribute != null)
                    {
                        configurationXml.redirectHostName = attribute.InnerText;
                    }

                    attribute = hubConfigurationNode.Attributes["redirectPort"];
                    if ((attribute != null) && (string.IsNullOrEmpty(attribute.InnerText) == false))
                    {
                        configurationXml.redirectPort = int.Parse(attribute.InnerText);
                        configurationXml.redirectPortSpecified = true;
                    }

                    attribute = hubConfigurationNode.Attributes["redirectUseGibraltarSds"];
                    if ((attribute != null) && (string.IsNullOrEmpty(attribute.InnerText) == false))
                    {
                        configurationXml.redirectUseGibraltarSds = bool.Parse(attribute.InnerText);
                        configurationXml.redirectUseGibraltarSdsSpecified = true;
                    }

                    attribute = hubConfigurationNode.Attributes["redirectUseSsl"];
                    if ((attribute != null) && (string.IsNullOrEmpty(attribute.InnerText) == false))
                    {
                        configurationXml.redirectUseSsl = bool.Parse(attribute.InnerText);
                        configurationXml.redirectUseSslSpecified = true;
                    }
                }

                //now move on to the child elements..  I'm avoiding XPath to avoid failure due to Xml schema variations
                if (hubConfigurationNode.HasChildNodes)
                {
                    XmlNode expirationDtNode = null;
                    XmlNode publicKeyNode = null;
                    XmlNode liveStreamNode = null;
                    foreach (XmlNode curChildNode in hubConfigurationNode.ChildNodes)
                    {
                        if (curChildNode.NodeType == XmlNodeType.Element)
                        {
                            switch (curChildNode.Name)
                            {
                                case "expirationDt":
                                    expirationDtNode = curChildNode;
                                    break;
                                case "publicKey":
                                    publicKeyNode = curChildNode;
                                    break;
                                case "liveStream":
                                    liveStreamNode = curChildNode;
                                    break;
                                default:
                                    break;
                            }

                            if ((expirationDtNode != null) && (publicKeyNode != null) && (liveStreamNode != null))
                                break;
                        }
                    }

                    if (expirationDtNode != null)
                    {
                        attribute = expirationDtNode.Attributes["DateTime"];
                        string dateTimeRaw = attribute.InnerText;

                        attribute = expirationDtNode.Attributes["Offset"];
                        string timeZoneOffset = attribute.InnerText;

                        if ((string.IsNullOrEmpty(dateTimeRaw) == false) && (string.IsNullOrEmpty(timeZoneOffset) == false))
                        {
                            configurationXml.expirationDt = new DateTimeOffsetXml();
                            configurationXml.expirationDt.DateTime = DateTime.Parse(dateTimeRaw);
                            configurationXml.expirationDt.Offset = int.Parse(timeZoneOffset);
                        }
                    }

                    if (publicKeyNode != null)
                    {
                        configurationXml.publicKey = publicKeyNode.InnerText;
                    }

                    if (liveStreamNode != null)
                    {
                        attribute = liveStreamNode.Attributes["agentPort"];
                        string agentPortRaw = attribute.InnerText;

                        attribute = liveStreamNode.Attributes["clientPort"];
                        string clientPortRaw = attribute.InnerText;

                        attribute = liveStreamNode.Attributes["useSsl"];
                        string useSslRaw = attribute.InnerText;

                        if ((string.IsNullOrEmpty(agentPortRaw) == false)
                            && (string.IsNullOrEmpty(clientPortRaw) == false)
                            && (string.IsNullOrEmpty(useSslRaw) == false))
                        {
                            configurationXml.liveStream = new LiveStreamServerXml();
                            configurationXml.liveStream.agentPort = int.Parse(agentPortRaw);
                            configurationXml.liveStream.clientPort = int.Parse(clientPortRaw);
                            configurationXml.liveStream.useSsl = bool.Parse(useSslRaw);
                        }
                    }
                }
            }

            return configurationXml;
        }

        private static void DateTimeOffsetXmlToXmlWriter(XmlWriter xmlWriter, string elementName, DateTimeOffsetXml dateTimeOffsetXml)
        {
            if (dateTimeOffsetXml == null)
                return;

            xmlWriter.WriteStartElement(elementName);
            WriteXmlAttribute(xmlWriter, "DateTime", dateTimeOffsetXml.DateTime);
            WriteXmlAttribute(xmlWriter, "Offset", dateTimeOffsetXml.Offset);
            xmlWriter.WriteEndElement();            
        }

        private static void DateTimeOffsetToXmlWriter(XmlWriter xmlWriter, string elementName, DateTimeOffset dateTimeOffset)
        {
            DateTimeOffsetXmlToXmlWriter(xmlWriter, elementName, ToDateTimeOffsetXml(dateTimeOffset));
        }

        /// <summary>
        /// Convert a DateTimeOffset value to its XML equivalent.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTimeOffsetXml ToDateTimeOffsetXml(DateTimeOffset dateTime)
        {
            DateTimeOffsetXml newObject = new DateTimeOffsetXml();
            newObject.DateTime = dateTime.DateTime;
            newObject.Offset = (int)dateTime.Offset.TotalMinutes;

            return newObject;
        }

        /// <summary>
        /// Convert the DateTimeOffset XML structure to its native form
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTimeOffset FromDateTimeOffsetXml(DateTimeOffsetXml dateTime)
        {
            if (dateTime == null)
                return DateTimeOffset.MinValue;

            var sourceDateTime = dateTime.DateTime;
            if (sourceDateTime.Kind != DateTimeKind.Unspecified)
                sourceDateTime = DateTime.SpecifyKind(sourceDateTime, DateTimeKind.Unspecified); //otherwise the DTO constructor will fail.

            return new DateTimeOffset(sourceDateTime, new TimeSpan(0, dateTime.Offset, 0));
        }

        private static void WriteXmlAttribute(XmlWriter xmlWriter, string name, string value)
        {
            if (value == null)
                return; //the correct way to indicate a null 

            xmlWriter.WriteStartAttribute(name);
            xmlWriter.WriteValue(value);
            xmlWriter.WriteEndAttribute();
        }

        private static void WriteXmlAttribute(XmlWriter xmlWriter, string name, long value)
        {
            xmlWriter.WriteStartAttribute(name);
            xmlWriter.WriteValue(value);
            xmlWriter.WriteEndAttribute();
        }

        private static void WriteXmlAttribute(XmlWriter xmlWriter, string name, bool value)
        {
            xmlWriter.WriteStartAttribute(name);
            xmlWriter.WriteValue(value);
            xmlWriter.WriteEndAttribute();
        }

        private static void WriteXmlAttribute(XmlWriter xmlWriter, string name, DateTime value)
        {
            xmlWriter.WriteStartAttribute(name);
            xmlWriter.WriteValue(value);
            xmlWriter.WriteEndAttribute();
        }
    }
}

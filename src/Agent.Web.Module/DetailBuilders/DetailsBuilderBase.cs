#region File Header

// <copyright file="DetailsBuilderBase.cs" company="Gibraltar Software Inc.">
// Gibraltar Software Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

#endregion

namespace Gibraltar.Agent.Web.Module.DetailBuilders
{
    public class DetailsBuilderBase
    {
        private readonly XmlSerializerNamespaces _xmlNamespaces;
        private readonly XmlWriterSettings _xmlWriterSettings;
        protected StringBuilder DetailBuilder;

        protected DetailsBuilderBase()
        {
            DetailBuilder = new StringBuilder();
            _xmlNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            _xmlWriterSettings = new XmlWriterSettings();
            _xmlWriterSettings.OmitXmlDeclaration = true;
        }

        protected string JObjectToXmlString(string details)
        {
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter, _xmlWriterSettings))
            {
                var detailsDoc = JsonConvert.DeserializeXmlNode(details, "UserSupplied");
                detailsDoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        protected string ObjectToXmlString<T>(T detailsObject)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));

            try
            {
                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = XmlWriter.Create(stringWriter, _xmlWriterSettings))
                {
                    xmlSerializer.Serialize(xmlTextWriter, detailsObject, _xmlNamespaces);
                    xmlTextWriter.Flush();
                    return stringWriter.GetStringBuilder().ToString();
                }
            }
            catch (System.Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                    Log.Write(LogMessageSeverity.Error, "Loupe", 0, ex, LogWriteMode.Queued,
                        "", "Loupe.Internal", "Failed to serialize object",
                        "Exception thrown whilst attempting to serialize {0} for this request", typeof(T).Name);
#endif
            }

            return null;
        }
    }
}
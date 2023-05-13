#region File Header

// <copyright file="HostCORSConfiguration.cs" company="Gibraltar Software Inc.">
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

using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Xml.Linq;

#endregion

namespace Gibraltar.Agent.Web.Module.Infrastructure
{
    public class HostCORSConfiguration
    {
        const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";
        const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";
        const string AccessControlMaxAge = "Access-Control-Max-Age";

        private Dictionary<string, string> GlobalHeaders { get; set; }


        public virtual bool GlobalAllowOrigin
        {
            get
            {
                return HasHeader(AccessControlAllowOrigin);    
            }
        }

        public virtual bool GlobalAllowHeaders
        {
            get
            {
                return HasHeader(AccessControlAllowHeaders);
            }
        }

        public virtual bool GlobalAllowMethods
        {
            get
            {
                return HasHeader(AccessControlAllowMethods);  
            }
        }

        public virtual bool GlobalAllowCredentials
        {
            get { return HasHeader(AccessControlAllowCredentials); }
        }

        public virtual bool GlobalMaxAge
        {
            get { return HasHeader(AccessControlMaxAge); }
        }

        private bool HasHeader(string header)
        {
            if (GlobalHeaders == null) LoadConfigValues();

            return GlobalHeaders.ContainsKey(header);            
        }

        private void LoadConfigValues()
        {
            GlobalHeaders = new Dictionary<string, string>();

            var doc = LoadXmlForSystemWebServerSection();

            var headers = GetElementByPath(doc, "httpProtocol.customHeaders");

            PopulateHeaders(headers);
        }

        private void PopulateHeaders(XElement headers)
        {
            if (headers != null)
            {
                foreach (var node in headers.Descendants()
                                            .Where(x => x.HasAttributes && ((string) x.Attribute("name")).Contains("Access-Control-Allow-")))
                {
                    var headerName = node.Attribute("name").Value;
                    var headerValue = node.Attribute("value").Value;

                    GlobalHeaders.Add(headerName, headerValue);
                }
            }
        }

        private XDocument LoadXmlForSystemWebServerSection()
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration("~");

            var webServerConfig = configuration.GetSection("system.webServer");

            var xml = webServerConfig.SectionInformation.GetRawXml();


            var doc = XDocument.Parse(xml);
            return doc;
        }

        public XElement GetElementByPath(XDocument document, string path)
        {
            var elementNames = path.Split('.');
            var currentElement = document.Root;

            foreach (var elementName in elementNames)
            {
                var element = GetElement(currentElement, elementName);
                if (element == null)
                {
                    return null;
                }

                currentElement = element;
            }

            return currentElement;
        }

        public XElement GetElement(XElement parentElement, string name)
        {
            return parentElement.Descendants(name).FirstOrDefault();
        }

    }
}
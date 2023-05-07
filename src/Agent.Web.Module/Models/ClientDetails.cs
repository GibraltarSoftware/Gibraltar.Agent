#region File Header

// <copyright file="ClientDetails.cs" company="Gibraltar Software Inc.">
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

using Newtonsoft.Json;

#endregion

namespace Gibraltar.Agent.Web.Module.Models
{
    public class ClientDetails
    {
        public string Description { get; set; }

        public string Layout { get; set; }

        public string Manufacturer { get; set; }

        public string Name { get; set; }

        public string Prerelease { get; set; }

        public string Product { get; set; }

        [JsonProperty("ua")]
        public string UserAgentString { get; set; }

        public string Version { get; set; }

        public ClientOS OS { get; set; }

        public ClientDimensions Size { get; set; }

    }

    public class ClientOS
    {
        public int Architecture { get; set; }

        public string Family { get; set; }

        public string Version { get; set; }
    }

    public class ClientDimensions
    {
        public long Height { get; set; }
        public long Width { get; set; }
    }
}
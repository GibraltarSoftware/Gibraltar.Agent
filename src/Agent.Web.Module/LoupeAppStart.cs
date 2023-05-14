#region File Header

// <copyright file="LoupeAppStart.cs" company="Gibraltar Software Inc.">
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

using System.Web.Http;
using Gibraltar.Agent.Web.Module;

#endregion

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(LoupeConfig),"LoupePreStart")]
namespace Gibraltar.Agent.Web.Module
{
    public static class LoupeConfig
    {
        public static void LoupePreStart()
        {
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "LoupeLog",
                routeTemplate: "loupe/log",
                defaults: new { controller="Loupe", action="Log"}
                );
        }
    }
}


#region File Header
// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 - 2015 by Gibraltar Software, Inc.  
//  *    All rights reserved.
//  *******************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;

namespace Gibraltar
{
    /// <summary>
    /// Encapsulation of application session summary information to be handled by lower-level code such as LicensingContext
    /// which can't access the full SessionSummary.
    /// </summary>
    public class ContextSummary
    {
        /// <summary>
        /// A default value for when the product name is unknown.
        /// </summary>
        public const string UnknownProduct = "Unknown Product";

        /// <summary>
        /// A default value for when the application name is unknown.
        /// </summary>
        public const string UnknownApplication = "Unknown Application";

        private readonly bool m_HasUnknowns;
        private readonly string m_Product;
        private readonly string m_Application;
        private readonly Version m_ApplicationVersion;
        private readonly Version m_OSVersion;
        private readonly string m_HostName;
        private readonly string m_DnsDomainName;
        private readonly string m_UserDomainName;
        private readonly string m_UserName;
        private readonly string m_FullyQualifiedUserName;

        /// <summary>
        /// Construct a ContextSummary instance with basic context information about this app domain session (for when the
        /// full SessionSummary can not be accessed).
        /// </summary>
        public ContextSummary()
        {
            string description;
            GetApplicationNameSafe(out m_Product, out m_Application, out m_ApplicationVersion, out description);
            if (m_Product == UnknownProduct || m_Application == UnknownApplication)
                m_HasUnknowns = true;

            m_UserDomainName = Environment.UserDomainName;
            m_UserName = Environment.UserName;
            m_FullyQualifiedUserName = string.IsNullOrEmpty(m_UserDomainName) ? m_UserName : StringReference.GetReference(m_UserDomainName + "\\" + m_UserName);

            // Try to read the OS Version.
            try
            {
                OSVersionInfo osVersionInfo = new OSVersionInfo(); // Our fancy logic to translate it.
                m_OSVersion = new Version(osVersionInfo.MajorVersion, osVersionInfo.MinorVersion, osVersionInfo.BuildNumber);
            }
            catch
            {
                //if we failed to get stuff the sweet way with PInvoke we'll instead do it the native .NET way.
                OperatingSystem osVersion = Environment.OSVersion; // Fall-back to the basic info available.
                m_OSVersion = osVersion.Version;
            }

            // Try to find the host name and domain.
            try
            {
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                m_HostName = ipGlobalProperties.HostName;
                m_DnsDomainName = ipGlobalProperties.DomainName ?? string.Empty;
            }
            catch
            {
                //fallback to environment names
                try
                {
                    m_HostName = Environment.MachineName;
                }
                catch (Exception ex)
                {
                    //we really don't want an init error to fail us, not here!
                    GC.KeepAlive(ex);

                    m_HostName = "unknown";
                }
                m_DnsDomainName = string.Empty;
            }
        }

        /// <summary>
        /// True if the caller needs to fall-back to other means to get the product and application names (and version).
        /// </summary>
        public bool HasUnknowns { get { return m_HasUnknowns; } }

        /// <summary>
        /// The product name of the application that recorded the session.
        /// </summary>
        public string Product { get { return m_Product; } }

        /// <summary>
        /// The title of the application that recorded the session.
        /// </summary>
        public string Application { get { return m_Application; } }

        /// <summary>
        /// The version of the application that recorded the session.
        /// </summary>
        public Version ApplicationVersion { get { return m_ApplicationVersion; } }

        /// <summary>
        /// The version information of the installed operating system (without service pack or patches).
        /// </summary>
        public Version OSVersion { get { return m_OSVersion; } }

        /// <summary>
        /// The host name / NetBIOS name of the computer that recorded the session.
        /// </summary>
        /// <remarks>Does not include the domain name portion of the fully qualified DNS name.</remarks>
        public string HostName { get { return m_HostName; } }

        /// <summary>
        /// The DNS domain name of the computer that recorded the session.  May be empty.
        /// </summary>
        /// <remarks>Does not include the host name portion of the fully qualified DNS name.</remarks>
        public string DnsDomainName { get { return m_DnsDomainName; } }

        /// <summary>
        /// The domain of the user id that was used to run the session.
        /// </summary>
        public string UserDomainName { get { return m_UserDomainName; } }

        /// <summary>
        /// The user Id that was used to run the session.
        /// </summary>
        public string UserName { get { return m_UserName; } }

        /// <summary>
        /// The fully qualified user name of the user id that was used to run the session.
        /// </summary>
        public string FullyQualifiedUserName { get { return m_FullyQualifiedUserName; } }

        /// <summary>
        /// Determine the correct application name for logging purposes of the current process.
        /// </summary>
        /// <param name="productName">The product name the logging system will use for this process.</param>
        /// <param name="applicationName">The application name the logging system will use for this process.</param>
        /// <param name="applicationVersion">The version of the application the logging system will use for this process.</param>
        /// <param name="applicationDescription">A description of the current application.</param>
        /// <remarks>This method isn't the complete story; the SessionSummary constructor has a more complete mechanism that takes into account
        /// the full scope of overrides.  This method will not throw exceptions if it is unable to determine suitable values.  Instead, default values of product name 'Unknown Product'
        /// application name 'Unknown Application', version 0.0, and an empty description will be used.</remarks>
        public static void GetApplicationNameSafe(out string productName, out string applicationName, out Version applicationVersion, out string applicationDescription)
        {
            productName = UnknownProduct;
            applicationName = UnknownApplication;
            applicationVersion = new Version(0, 0);
            applicationDescription = string.Empty;

            try
            {
                //we generally work off the top executable that started the whole thing.
                Assembly topAssembly = Assembly.GetEntryAssembly();

                if (topAssembly == null)
                {
                    //we must be either ASP.NET or some bizarre reflected environment.
                    return;
                }
                else
                {
                    //the version isn't a custom attribute so we can get it directly.
                    applicationVersion = topAssembly.GetName().Version;

                    //now get the attributes we need.
                    AssemblyFileVersionAttribute[] fileVersionAttributes = topAssembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true) as AssemblyFileVersionAttribute[];
                    AssemblyProductAttribute[] productAttributes = topAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true) as AssemblyProductAttribute[];
                    AssemblyTitleAttribute[] titleAttributes = topAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true) as AssemblyTitleAttribute[];
                    AssemblyDescriptionAttribute[] descriptionAttributes = topAssembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true) as AssemblyDescriptionAttribute[];

                    //and interpret this information
                    if ((fileVersionAttributes != null) && (fileVersionAttributes.Length > 0))
                    {
                        //try to parse this value (it's text so it might not parse)
                        string rawFileVersion = fileVersionAttributes[0].Version;
                        try
                        {
                            applicationVersion = new Version(rawFileVersion);
                        }
                        // ReSharper disable EmptyGeneralCatchClause
                        catch
                        // ReSharper restore EmptyGeneralCatchClause
                        {
                        }
                    }

                    if ((productAttributes != null) && (productAttributes.Length > 0))
                    {
                        productName = productAttributes[0].Product ?? string.Empty; //protected against null explicit values
                    }

                    if ((titleAttributes != null) && (titleAttributes.Length > 0))
                    {
                        applicationName = titleAttributes[0].Title ?? string.Empty; //protected against null explicit values
                    }

                    if ((descriptionAttributes != null) && (descriptionAttributes.Length > 0))
                    {
                        applicationDescription = descriptionAttributes[0].Description ?? string.Empty; //protected against null explicit values
                    }
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
        }
    }
}

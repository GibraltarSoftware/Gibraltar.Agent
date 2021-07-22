

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Gibraltar.Data;
using Gibraltar.Monitor.Internal;
using Loupe.Extensibility.Data;
using Microsoft.VisualBasic.Devices;

namespace Gibraltar.Monitor
{
    /// <summary>
    /// Summary information about the entire session.
    /// </summary>
    /// <remarks>This information is available from sessions without loading the entire session into memory.</remarks>
    public class SessionSummary: ISessionSummary
    {
        private readonly bool m_IsLive;
        private readonly SessionSummaryPacket m_Packet;
        private long m_CriticalCount;
        private long m_ErrorCount;
        private long m_WarningCount;
        private long m_MessageCount;
        volatile private SessionStatus m_SessionStatus;
        private OSVersionInfo m_OsVersionInfo;
        private ApplicationType m_AgentAppType;
        private Framework? m_Framework; //the primary framework that recorded the session

        private readonly bool m_PrivacyEnabled;

        /// <summary>
        /// Raised whenever a property changes on the object
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Create a new session summary as the live collection session for the current process
        /// </summary>
        /// <remarks>This constructor figures out all of the summary information when invoked, which can take a moment.</remarks>
        internal SessionSummary(AgentConfiguration configuration)
        {
            m_IsLive = true;
            m_Packet = new SessionSummaryPacket();
            m_SessionStatus = SessionStatus.Running;

            m_PrivacyEnabled = configuration.Publisher.EnableAnonymousMode;

            try
            {
                m_Packet.ID = Guid.NewGuid();
                m_Packet.Caption = null;

                //this stuff all tends to succeed
                m_Packet.UserName = m_PrivacyEnabled ? string.Empty : System.Environment.UserName;
                m_Packet.UserDomainName = m_PrivacyEnabled ? string.Empty : System.Environment.UserDomainName;
                m_Packet.TimeZoneCaption = TimeZone.CurrentTimeZone.StandardName;
                m_Packet.EndDateTime = StartDateTime; //we want to ALWAYS have an end time, and since we just created our start time we need to move that over to end time

                //this stuff, on the other hand, doesn't always succeed

                //Lets see if the user has already picked some things for us...
                PublisherConfiguration publisherConfig = configuration.Publisher;
                string productName = null, applicationName = null, applicationDescription = null;
                Version applicationVersion = null;

                //what kind of process are we?
                if (publisherConfig.ApplicationType != ApplicationType.Unknown)
                {
                    // They specified an application type, so just use that.
                    m_AgentAppType = publisherConfig.ApplicationType; // Start with the type they specified.

                    // Now sanity-check what they gave us.
                    if (Log.IsMonoRuntime == false) // Mono always says UserInteractive is false, so we can't fully sanity-check.
                    {
                        // Interactive can't be a service.  Non-interactive can't be WinForms.  Treat as Console for safe operation.
                        if (m_AgentAppType == (System.Environment.UserInteractive ? ApplicationType.Service : ApplicationType.Windows))
                            m_AgentAppType = ApplicationType.Console; // Disable most unsafe Agent features.
                    }
                    else
                    {
                        // Interactive can't be a service.  Treat as Console for safe operation.
                        if (System.Environment.UserInteractive && m_AgentAppType == ApplicationType.Service)
                            m_AgentAppType = ApplicationType.Console;
                    }
                }
                else
                {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly == null)
                    {
                        // We are ASP.NET or some other odd hosting environment that uses reflection/dynamic loading.
                        string appVirtualPath = null;
                        try
                        {
                            //appVirtualPath = HttpRuntime.AppDomainAppVirtualPath; // Can't do this with .NET 4.0 Client Profile.

                            // We want to try this with reflection so we don't need to reference any web stuff for 
                            // compatibility with the .NET 4 Client Profile.

                            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies(); // Get all loaded assemblies.
                            Assembly systemWeb = null;
                            Version bestVersion = new Version(0, 0);
                            foreach (Assembly assembly in assemblies)
                            {
                                AssemblyName assemblyName = assembly.GetName();
                                if (assemblyName.Name == "System.Web") // Find any with the right name.
                                {
                                    // In case there's more than one--normally shouldn't be--look for the highest version.
                                    Version currentVersion = assemblyName.Version;
                                    if (systemWeb == null || currentVersion > bestVersion)
                                    {
                                        systemWeb = assembly;
                                        bestVersion = currentVersion;
                                    }
                                }
                            }

                            Type httpRuntime = null;
                            if (systemWeb != null)
                            {
                                httpRuntime = systemWeb.GetType("System.Web.HttpRuntime"); // Try dynamically from that assembly.
                            }

                            if (httpRuntime != null)
                            {
                                // We found the alleged type we're looking for.  Now find the property we want via reflection.
                                const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.Public;
                                PropertyInfo propInfo = httpRuntime.GetProperty("AppDomainAppVirtualPath", staticFlags);

                                if (propInfo != null)
                                {
                                    // We found the property, so we can finally read it through reflection.
                                    appVirtualPath = propInfo.GetValue(null, null) as string; // A static non-indexed property.
                                }
                            }
                        }
                        // ReSharper disable EmptyGeneralCatchClause
                        catch
                        // ReSharper restore EmptyGeneralCatchClause
                        {
                        }

                        if (string.IsNullOrEmpty(appVirtualPath))
                        {
                            //we have no idea what we are...
                            m_AgentAppType = ApplicationType.Unknown;
                        }
                        else
                        {
                            productName = "ASP.NET";
                            applicationName = appVirtualPath;
                            applicationDescription = "ASP.NET Web application";
                            m_AgentAppType = ApplicationType.AspNet;
                        }
                    }
                    else
                    {
                        //MethodInfo entryPoint = entryAssembly.EntryPoint;
                        //if ((entryPoint != null) && (typeof(ServiceBase).IsAssignableFrom(entryPoint.ReflectedType)))

                        int sessionId;
                        if (Log.IsMonoRuntime)
                        {
                            try
                            {
                                sessionId = Process.GetCurrentProcess().SessionId; // May not work for Mono.
                            }
                            catch
                            {
                                sessionId = 0; // XP tends to always be on 0 anyway, so this is a safe value to use by default for Mono.
                            }
                        }
                        else
                        {
                            sessionId = Process.GetCurrentProcess().SessionId;
                        }

                        if (Log.IsMonoRuntime == false && sessionId == 0 && System.Environment.UserInteractive == false)
                        {
                            // Mono always says UserInteractive is false and may have sessionId as 0 as well, so skip this.
                            // If we're in SessionId 0 then we're started by the kernel.  If also non-interactive, call it a service.
                            m_AgentAppType = ApplicationType.Service;
                        }
                        else
                        {
                            m_AgentAppType = ApplicationType.Console;
                            if (Log.IsMonoRuntime || System.Environment.UserInteractive) // Mono always thinks it's not interactive.
                            {
                                AssemblyName[] referencedAssemblies = entryAssembly.GetReferencedAssemblies();
                                foreach (AssemblyName assemblyName in referencedAssemblies)
                                {
                                    if (assemblyName.Name == "System.Windows.Forms")
                                    {
                                        m_AgentAppType = ApplicationType.Windows;
                                        break;
                                    }

                                    if (assemblyName.Name == "System.Windows.Presentation")
                                    {
                                        m_AgentAppType = ApplicationType.Windows; // Should be WPF instead?
                                        break;
                                    }
                                }
                                // If it doesn't reference System.Windows.Forms, it can't be a winforms app.
                            }
                            // Otherwise, non-interactive can't be a winforms app, so leave it as Console.
                        }
                    }

                }

                m_Packet.ApplicationType = m_AgentAppType; // Finally, set the application type from our determined type.
                if (m_AgentAppType != ApplicationType.AspNet)
                {
                    //we want to find our entry assembly and get default product/app info from it.
                    ContextSummary.GetApplicationNameSafe(out productName, out applicationName, out applicationVersion, out applicationDescription);                    
                }

                //OK, now apply configuration overrides or what we discovered...
                m_Packet.ProductName = string.IsNullOrEmpty(publisherConfig.ProductName) ? productName : publisherConfig.ProductName;
                m_Packet.ApplicationName = string.IsNullOrEmpty(publisherConfig.ApplicationName) ? applicationName : publisherConfig.ApplicationName;
                m_Packet.ApplicationVersion = publisherConfig.ApplicationVersion ?? applicationVersion;
                m_Packet.ApplicationDescription = string.IsNullOrEmpty(publisherConfig.ApplicationDescription) ? applicationDescription : publisherConfig.ApplicationDescription;
                m_Packet.EnvironmentName = publisherConfig.EnvironmentName;
                m_Packet.PromotionLevelName = publisherConfig.PromotionLevelName;

                //Finally, no nulls allowed! Fix any...
                m_Packet.ProductName = string.IsNullOrEmpty(m_Packet.ProductName) ? "Unknown" : m_Packet.ProductName;
                m_Packet.ApplicationName = string.IsNullOrEmpty(m_Packet.ApplicationName) ? "Unknown" : m_Packet.ApplicationName;
                m_Packet.ApplicationVersion = m_Packet.ApplicationVersion ?? new Version(0, 0);
                m_Packet.ApplicationDescription = m_Packet.ApplicationDescription ?? string.Empty;
                m_Packet.EnvironmentName = m_Packet.EnvironmentName ?? string.Empty;
                m_Packet.PromotionLevelName = m_Packet.PromotionLevelName ?? string.Empty;

                m_Packet.ComputerId = GetComputerIdSafe(m_Packet.ProductName, configuration);
                m_Packet.AgentVersion = GetAgentVersionSafe();
            }
            catch (Exception ex)
            {
                //we really don't want an init error to fail us, not here!
                GC.KeepAlive(ex);
            }

            if (m_PrivacyEnabled == false)
            {
                try
                {
                    IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                    m_Packet.HostName = ipGlobalProperties.HostName;
                    m_Packet.DnsDomainName = ipGlobalProperties.DomainName ?? string.Empty;
                }
                catch
                {
                    //fallback to environment names
                    try
                    {
                        m_Packet.HostName = System.Environment.MachineName;
                    }
                    catch (Exception ex)
                    {
                        //we really don't want an init error to fail us, not here!
                        GC.KeepAlive(ex);

                        m_Packet.HostName = "unknown";
                    }
                    m_Packet.DnsDomainName = string.Empty;
                }
            }
            else
            {
                // Privacy mode.  Don't store "personally-identifying information".
                m_Packet.HostName = "anonymous";
                m_Packet.DnsDomainName = string.Empty;
            }

            //This class uses PInvoke so we'll treat it with particular suspicion
            try
            {
                OSVersionInfo osVersionInfo = new OSVersionInfo();

                m_Packet.OSPlatformCode = osVersionInfo.PlatformId;
                m_Packet.OSVersion = new Version(osVersionInfo.MajorVersion, osVersionInfo.MinorVersion, osVersionInfo.BuildNumber);
                m_Packet.OSServicePack = osVersionInfo.ServicePack;
                m_Packet.OSSuiteMask = osVersionInfo.SuiteMask;
                m_Packet.OSProductType = osVersionInfo.ProductType;
            }
            catch
            {
                //if we failed to get stuff the sweet way with PInvoke we'll instead do it the native .NET way.
                OperatingSystem osVersion = System.Environment.OSVersion;

                m_Packet.OSPlatformCode = (int)osVersion.Platform;
                m_Packet.OSVersion = osVersion.Version;
                m_Packet.OSServicePack = osVersion.ServicePack;
            }

            try
            {
                //NOTE:  This seems like a bizarre way, but I can't find another and this is actually listed in the MSDN doc as a valid approach.
                m_Packet.RuntimeArchitecture = (IntPtr.Size == 4) ? ProcessorArchitecture.X86 : ProcessorArchitecture.Amd64;

                m_Packet.OSCultureName = CultureInfo.InstalledUICulture.ToString();
                m_Packet.CurrentCultureName = CultureInfo.CurrentCulture.ToString();
                m_Packet.CurrentUICultureName = CultureInfo.CurrentUICulture.ToString();

                //NOTE:  I think this sucks...
                string architecture = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");

                if ((string.IsNullOrEmpty(architecture) == false) && (architecture.Equals("x86", StringComparison.OrdinalIgnoreCase)))
                {
                    //On Wow64 it reports the OS as x86 for compatibility so we have to check a secondary variable
                    string architectureWow64 = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432");

                    if ((string.IsNullOrEmpty(architectureWow64) == false) && (architectureWow64.Equals("AMD64", StringComparison.OrdinalIgnoreCase)))
                        architecture = "AMD64";
                }

                ProcessorArchitecture processorArchitecture;

                if (string.IsNullOrEmpty(architecture))
                {
                    processorArchitecture = m_Packet.RuntimeArchitecture; // Use the runtime by default if we can't tell
                }
                else if (architecture.Equals("amd64", StringComparison.OrdinalIgnoreCase))
                {
                    processorArchitecture = ProcessorArchitecture.Amd64;
                }
                else
                {
                    processorArchitecture = ProcessorArchitecture.X86;
                }

                m_Packet.OSArchitecture = processorArchitecture;
                m_Packet.OSBootMode = SystemInformation.BootMode;
                m_Packet.RuntimeVersion = System.Environment.Version;

                try
                {
                    long totalMemory = 0;

                    //computer info isn't really implemented on MONO
                    if (Log.IsMonoRuntime)
                    {
                        //try the performance counter
                        PerformanceCounter counter = new PerformanceCounter("Mono Memory", "Total Physical Memory");
                        totalMemory = counter.RawValue;
                    }
                    else
                    {
                        ComputerInfo info = new ComputerInfo();
                        totalMemory = Convert.ToInt64(info.TotalPhysicalMemory); //it's a ULONG. I kid you not.
                    }
                    m_Packet.MemoryMB = Convert.ToInt32(totalMemory / 1048576); //convert is more tolerant of loss of accuracy
                }
                catch (Exception ex)
                {
                    GC.KeepAlive(ex);
                }

                m_Packet.Processors = System.Environment.ProcessorCount;
                m_Packet.ProcessorCores = m_Packet.Processors; //BUG
                m_Packet.UserInteractive = System.Environment.UserInteractive; // Note: Mono always returns false.

                //find the active screen resolution
                if (!Log.IsMonoRuntime || (m_AgentAppType == ApplicationType.Windows)) //this avoids problems on Mono where it fails if X is not available.
                {
                    m_Packet.TerminalServer = SystemInformation.TerminalServerSession;
                    Size screenSize = Screen.PrimaryScreen.Bounds.Size;
                    m_Packet.ColorDepth = Screen.PrimaryScreen.BitsPerPixel; // Note: Mono always returns 32.
                    m_Packet.ScreenHeight = screenSize.Height;
                    m_Packet.ScreenWidth = screenSize.Width;
                }

                m_Packet.CommandLine = m_PrivacyEnabled || configuration.Listener.EnableCommandLine == false ? string.Empty : System.Environment.CommandLine;
            }
            catch (Exception ex)
            {
                //we really don't want an init error to fail us, not here!
                GC.KeepAlive(ex);
            }

            //now do user defined properties
            try
            {
                foreach (KeyValuePair<string, string> keyValuePair in configuration.Properties)
                {
                    m_Packet.Properties.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            catch (Exception ex)
            {
                //we aren't expecting any errors, but best be safe.
                GC.KeepAlive(ex);
            }

            m_Packet.Caption = m_Packet.ApplicationName;
        }

        internal SessionSummary(SessionSummaryPacket packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            m_Packet = packet;
            m_SessionStatus = SessionStatus.Unknown; //it should be set for us in a minute...

            var framework = Framework; //force this to evaluate now.
        }

        #region Public Properties and Methods

        /// <summary>
        /// Overrides the native recorded product and application information with the specified values to reflect the server rules.
        /// </summary>
        public void ApplyMappingOverrides(string productName, string applicationName, Version applicationVersion, string environmentName, string promotionLevelName)
        {
            m_Packet.ProductName = productName;
            m_Packet.ApplicationName = applicationName;
            m_Packet.ApplicationVersion = applicationVersion;
            m_Packet.EnvironmentName = environmentName;
            m_Packet.PromotionLevelName = promotionLevelName;

            m_Packet.Caption = applicationName; // this is what the packet constructor does.
        }

        /// <summary>
        /// Get a copy of the full session detail this session refers to.  
        /// </summary>
        /// <remarks>Session objects can be large in memory.  This method will return a new object
        /// each time it is called which should be released by the caller as soon as feasible to control memory usage.</remarks>
        ISession ISessionSummary.Session()
        {
            throw new NotSupportedException("Retrieving the full session from a SessionSummary is not supported");
        }

        /// <summary>
        /// The unique Id of the session
        /// </summary>
        public Guid Id => m_Packet.ID;

        /// <summary>
        /// The link to this item on the server
        /// </summary>
        public Uri Uri => throw new NotSupportedException("Links are not supported in this context");

        /// <summary>
        /// Indicates if the session has ever been viewed or exported
        /// </summary>
        bool ISessionSummary.IsNew => true;

        /// <summary>
        /// Indicates if all of the session data is stored that is expected to be available
        /// </summary>
        bool ISessionSummary.IsComplete => true;

        /// <summary>
        /// Indicates if the session is currently running and a live stream is available.
        /// </summary>
        bool ISessionSummary.IsLive => false; //we presume in this context we're not a live stream.

        /// <summary>
        /// Indicates if session data is available.
        /// </summary>
        /// <remarks>The session summary can be transfered separately from the session details
        /// and isn't subject to pruning so it may be around long before or after the detailed data is.</remarks>
        bool ISessionSummary.HasData => false; //we are just a header, we presume we stand alone.

        /// <summary>
        /// The unique Id of the local computer.
        /// </summary>
        public Guid? ComputerId => m_Packet.ComputerId;

        /// <summary>
        /// The display caption of the time zone where the session was recorded
        /// </summary>
        public string TimeZoneCaption => m_Packet.TimeZoneCaption;

        /// <summary>
        /// The date and time the session started
        /// </summary>
        public DateTimeOffset StartDateTime => m_Packet.Timestamp;

        /// <summary>
        /// The date and time the session started
        /// </summary>
        public DateTimeOffset DisplayStartDateTime => StartDateTime;

        /// <summary>
        /// The date and time the session ended or was last confirmed running
        /// </summary>
        public DateTimeOffset EndDateTime
        {
            get
            {
                if (m_IsLive)
                {
                    //we're the live session and still kicking - we haven't ended yet!
                    m_Packet.EndDateTime = DateTimeOffset.Now;
                }

                return m_Packet.EndDateTime;
            } 

            internal set => m_Packet.EndDateTime = value;
        }

        /// <summary>
        /// The date and time the session ended or was last confirmed running in the time zone the user has requested for display
        /// </summary>
        public DateTimeOffset DisplayEndDateTime => EndDateTime;

        /// <summary>
        /// The time range between the start and end of this session, or the last message logged if the session ended unexpectedly.
        /// </summary>
        public TimeSpan Duration => EndDateTime - StartDateTime;

        /// <summary>
        /// The date and time the session was added to the repository
        /// </summary>
        DateTimeOffset ISessionSummary.AddedDateTime => StartDateTime;

        /// <summary>
        /// The date and time the session was added to the repository in the time zone the user has requested for display
        /// </summary>
        DateTimeOffset ISessionSummary.DisplayAddedDateTime => StartDateTime;

        /// <summary>
        /// The date and time the session was added to the repository
        /// </summary>
        DateTimeOffset ISessionSummary.UpdatedDateTime => EndDateTime;

        /// <summary>
        /// The date and time the session header was last updated locally in the time zone the user has requested for display
        /// </summary>
        DateTimeOffset ISessionSummary.DisplayUpdatedDateTime => EndDateTime;

        /// <summary>
        /// The time range between the start and end of this session, or the last message logged if the session ended unexpectedly.
        /// Formatted as a string in HH:MM:SS format.
        /// </summary>
        public string DurationShort
        {
            get
            {
                string formattedDuration;

                TimeSpan duration = Duration;

                //we have to format it manually; I couldn't find anything built-in that would format a timespan.
                if (duration.Days > 0)
                {
                    // It spans at least a day, so put Days in front, too
                    formattedDuration = string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}:{3:00}",
                                                      duration.Days, duration.Hours, duration.Minutes, duration.Seconds);
                }
                else
                {
                    // It spans less than a day, so leave Days off
                    formattedDuration = string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}",
                                                      duration.Hours, duration.Minutes, duration.Seconds);
                }

                return formattedDuration;
            }
        }


        /// <summary>
        /// A display caption for the session
        /// </summary>
        public string Caption
        {
            get => m_Packet.Caption;
            set
            {
                if (m_Packet.Caption != value)
                {
                    m_Packet.Caption = value;

                    //and signal that we changed a property we expose
                    SendPropertyChanged("Caption");
                }
            }
        }

        /// <summary>
        /// The product name of the application that recorded the session
        /// </summary>
        public string Product => m_Packet.ProductName;

        /// <summary>
        /// The title of the application that recorded the session
        /// </summary>
        public string Application => m_Packet.ApplicationName;

        /// <summary>
        /// Optional.  The environment this session is running in.
        /// </summary>
        /// <remarks>Environments are useful for categorizing sessions, for example to 
        /// indicate the hosting environment. If a value is provided it will be 
        /// carried with the session data to upstream servers and clients.  If the 
        /// corresponding entry does not exist it will be automatically created.</remarks>
        public string Environment => m_Packet.EnvironmentName;

        /// <summary>
        /// Optional.  The promotion level of the session.
        /// </summary>
        /// <remarks>Promotion levels are useful for categorizing sessions, for example to 
        /// indicate whether it was run in development, staging, or production. 
        /// If a value is provided it will be carried with the session data to upstream servers and clients.  
        /// If the corresponding entry does not exist it will be automatically created.</remarks>
        public string PromotionLevel => m_Packet.PromotionLevelName;

        /// <summary>
        /// The type of process the application ran as (as declared or detected for recording).  (See AgentAppType for internal Agent use.)
        /// </summary>
        public ApplicationType ApplicationType => m_Packet.ApplicationType;

        /// <summary>
        /// The type of process the application ran as (as seen by the Agent internally).
        /// </summary>
        public ApplicationType AgentAppType => m_AgentAppType;

        /// <summary>
        /// The description of the application from its manifest.
        /// </summary>
        public string ApplicationDescription => m_Packet.ApplicationDescription;

        /// <summary>
        /// The version of the application that recorded the session
        /// </summary>
        public Version ApplicationVersion => m_Packet.ApplicationVersion;

        /// <summary>
        /// The version of the Gibraltar Agent used to monitor the session
        /// </summary>
        public Version AgentVersion => m_Packet.AgentVersion;

        /// <summary>
        /// The host name / NetBIOS name of the computer that recorded the session
        /// </summary>
        /// <remarks>Does not include the domain name portion of the fully qualified DNS name.</remarks>
        public string HostName => m_Packet.HostName;

        /// <summary>
        /// The DNS domain name of the computer that recorded the session.  May be empty.
        /// </summary>
        /// <remarks>Does not include the host name portion of the fully qualified DNS name.</remarks>
        public string DnsDomainName => m_Packet.DnsDomainName;

        /// <summary>
        /// The fully qualified user name of the user the application was run as.
        /// </summary>
        public string FullyQualifiedUserName => m_Packet.FullyQualifiedUserName;

        /// <summary>
        /// The user Id that was used to run the session
        /// </summary>
        public string UserName => m_Packet.UserName;

        /// <summary>
        /// The domain of the user id that was used to run the session
        /// </summary>
        public string UserDomainName => m_Packet.UserDomainName;

        /// <summary>
        /// The version information of the installed operating system (without service pack or patches)
        /// </summary>
        public Version OSVersion => m_Packet.OSVersion;

        /// <summary>
        /// The operating system service pack, if any.
        /// </summary>
        public string OSServicePack => m_Packet.OSServicePack;

        /// <summary>
        /// The culture name of the underlying operating system installation
        /// </summary>
        public string OSCultureName => m_Packet.OSCultureName;

        /// <summary>
        /// The processor architecture of the operating system.
        /// </summary>
        public ProcessorArchitecture OSArchitecture => m_Packet.OSArchitecture;

        /// <summary>
        /// The boot mode of the operating system.
        /// </summary>
        public BootMode OSBootMode => m_Packet.OSBootMode;

        /// <summary>
        /// The OS Platform code, nearly always 1 indicating Windows NT
        /// </summary>
        public int OSPlatformCode => m_Packet.OSPlatformCode;

        /// <summary>
        /// The OS product type code, used to differentiate specific editions of various operating systems.
        /// </summary>
        public int OSProductType => m_Packet.OSProductType;

        /// <summary>
        /// The OS Suite Mask, used to differentiate specific editions of various operating systems.
        /// </summary>
        public int OSSuiteMask => m_Packet.OSSuiteMask;

        /// <summary>
        /// The well known operating system family name, like Windows Vista or Windows Server 2003.
        /// </summary>
        public string OSFamilyName
        {
            get
            {
                EnsureOsVersionInfoLoaded();

                return m_OsVersionInfo.FamilyName;
            }
        }

        /// <summary>
        /// The edition of the operating system without the family name, such as Workstation or Standard Server.
        /// </summary>
        public string OSEditionName
        {
            get
            {
                EnsureOsVersionInfoLoaded();

                return m_OsVersionInfo.EditionName;
            }
        }

        /// <summary>
        /// The well known OS name and edition name
        /// </summary>
        public string OSFullName
        {
            get
            {
                EnsureOsVersionInfoLoaded();

                return m_OsVersionInfo.FullName;
            }
        }

        /// <summary>
        /// The well known OS name, edition name, and service pack like Windows XP Professional Service Pack 3
        /// </summary>
        public string OSFullNameWithServicePack
        {
            get
            {
                EnsureOsVersionInfoLoaded();

                return m_OsVersionInfo.FullNameWithServicePack;
            }
        }

        /// <summary>
        /// The version of the .NET runtime that the application domain is running as.
        /// </summary>
        public Version RuntimeVersion => m_Packet.RuntimeVersion;

        /// <summary>
        /// The processor architecture the process is running as.
        /// </summary>
        public ProcessorArchitecture RuntimeArchitecture => m_Packet.RuntimeArchitecture;

        /// <summary>
        /// The current application culture name.
        /// </summary>
        public string CurrentCultureName => m_Packet.CurrentCultureName;

        /// <summary>
        /// The current user interface culture name.
        /// </summary>
        public string CurrentUICultureName => m_Packet.CurrentUICultureName;

        /// <summary>
        /// The number of megabytes of installed memory in the host computer.
        /// </summary>
        public int MemoryMB => m_Packet.MemoryMB;

        /// <summary>
        /// The number of physical processor sockets in the host computer.
        /// </summary>
        public int Processors => m_Packet.Processors;

        /// <summary>
        /// The total number of processor cores in the host computer.
        /// </summary>
        public int ProcessorCores => m_Packet.ProcessorCores;

        /// <summary>
        /// Indicates if the session was run in a user interactive mode.
        /// </summary>
        public bool UserInteractive => m_Packet.UserInteractive;

        /// <summary>
        /// Indicates if the session was run through terminal server.  Only applies to User Interactive sessions.
        /// </summary>
        public bool TerminalServer => m_Packet.TerminalServer;

        /// <summary>
        /// The number of pixels wide of the virtual desktop.
        /// </summary>
        public int ScreenWidth => m_Packet.ScreenWidth;

        /// <summary>
        /// The number of pixels tall for the virtual desktop.
        /// </summary>
        public int ScreenHeight => m_Packet.ScreenHeight;

        /// <summary>
        /// The number of bits of color depth.
        /// </summary>
        public int ColorDepth => m_Packet.ColorDepth;

        /// <summary>
        /// The complete command line used to execute the process including arguments.
        /// </summary>
        public string CommandLine => m_Packet.CommandLine;


        /// <summary>
        /// The final status of the session.
        /// </summary>
        public SessionStatus Status { get => m_SessionStatus;
            internal set => m_SessionStatus = value;
        }

            /// <summary>
        /// The number of messages in the messages collection.
        /// </summary>
        /// <remarks>This value is cached for high performance and reflects all of the known messages.  If only part
        /// of the files for a session are loaded, the totals as of the latest file loaded are used.  This means the
        /// count of items may exceed the actual number of matching messages in the messages collection if earlier
        /// files are missing.</remarks>
        public int MessageCount { get => (int)m_MessageCount;
                internal set => m_MessageCount = value;
            }

        /// <summary>
        /// The number of critical messages in the messages collection.
        /// </summary>
        /// <remarks>This value is cached for high performance and reflects all of the known messages.  If only part
        /// of the files for a session are loaded, the totals as of the latest file loaded are used.  This means the
        /// count of items may exceed the actual number of matching messages in the messages collection if earlier
        /// files are missing.</remarks>
        public int CriticalCount { get => (int)m_CriticalCount;
            internal set => m_CriticalCount = value;
        }

        /// <summary>
        /// The number of error messages in the messages collection.
        /// </summary>
        /// <remarks>This value is cached for high performance and reflects all of the known messages.  If only part
        /// of the files for a session are loaded, the totals as of the latest file loaded are used.  This means the
        /// count of items may exceed the actual number of matching messages in the messages collection if earlier
        /// files are missing.</remarks>
        public int ErrorCount { get => (int)m_ErrorCount;
            internal set => m_ErrorCount = value;
        }

        /// <summary>
        /// The number of error messages in the messages collection.
        /// </summary>
        /// <remarks>This value is cached for high performance and reflects all of the known messages.  If only part
        /// of the files for a session are loaded, the totals as of the latest file loaded are used.  This means the
        /// count of items may exceed the actual number of matching messages in the messages collection if earlier
        /// files are missing.</remarks>
        public int WarningCount { get => (int)m_WarningCount;
            internal set => m_WarningCount = value;
        }

        /// <summary>
        /// A collection of application specific properties.
        /// </summary>
        public IDictionary<string, string> Properties => m_Packet.Properties;

        /// <summary>
        /// Optional. Represents the computer that sent the session
        /// </summary>
        public IComputer Computer => null;

        /// <summary>
        /// The primary application framework that recorded the session
        /// </summary>
        public Framework Framework
        {
            get
            {
                if (m_Framework.HasValue == false)
                {
                    //KM: WARNING: THIS CODE BLOCK IS DUPLICATED IN CORE.WINDOWS
                    m_Framework = Framework.DotNet;

                    //see if we have a good reason to be something other than .NET Framework...
                    if (Properties.TryGetValue("LOUPE_INTERNAL_PLATFORM", out var value))
                    {
                        if (string.Equals(value, "java", StringComparison.OrdinalIgnoreCase))
                        {
                            m_Framework = Framework.Java;
                        }
                        else if (string.Equals(value, "dotNetCore", StringComparison.OrdinalIgnoreCase))
                        {
                            m_Framework = Framework.DotNetCore;
                        }

                        //and remove the property...
                        Properties.Remove("LOUPE_INTERNAL_PLATFORM");
                    }
                }

                return m_Framework.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Generates a reasonable default caption for the provided session that has no caption
        /// </summary>
        /// <param name="sessionSummary">The session summary object to generate a default caption for</param>
        /// <returns>The default caption</returns>
        public static string DefaultCaption(SessionSummary sessionSummary)
        {
            string defaultCaption = string.Empty;

            //We are currently shooting for <appname> <Short Date> <Short time>
            if (string.IsNullOrEmpty(sessionSummary.Application))
            {
                defaultCaption += "(Unknown app)";
            }
            else
            {
                //we want to truncate the application if it's over a max length
                if (sessionSummary.Application.Length > 32)
                {
                    defaultCaption += sessionSummary.Application.Substring(0, 32);
                }
                else
                {
                    defaultCaption += sessionSummary.Application;
                }
            }

            defaultCaption += " " + sessionSummary.StartDateTime.DateTime.ToShortDateString();

            defaultCaption += " " + sessionSummary.StartDateTime.DateTime.ToShortTimeString();

            return defaultCaption;
        }

        #endregion

        #region Internal Properties and Methods

        internal SessionSummaryPacket Packet => m_Packet;

        internal bool PrivacyEnabled => m_PrivacyEnabled;

        /// <summary>
        /// Inspect the provided packet to update relevant statistics
        /// </summary>
        /// <param name="packet">A Log message packet to count</param>
        internal void UpdateMessageStatistics(LogMessagePacket packet)
        {
            m_MessageCount++;

            switch (packet.Severity)
            {
                case LogMessageSeverity.Critical:
                    m_CriticalCount++;
                    break;
                case LogMessageSeverity.Error:
                    m_ErrorCount++;
                    break;
                case LogMessageSeverity.Warning:
                    m_WarningCount++;
                    break;
            }
        }

        /// <summary>
        /// Clear the existing statistic counters
        /// </summary>
        /// <remarks>Typically used before the messages are recounted to ensure
        /// they can be correctly updated.</remarks>
        internal void ClearMessageStatistics()
        {
            m_MessageCount = 0;
            m_CriticalCount = 0;
            m_ErrorCount = 0;
            m_WarningCount = 0;
        }

        #endregion

        #region Private Properties and Methods

        private void EnsureOsVersionInfoLoaded()
        {
            if (m_OsVersionInfo == null)
            {
                m_OsVersionInfo = new OSVersionInfo(OSPlatformCode, OSVersion, OSServicePack, OSSuiteMask, OSProductType);
            }
        }

        private static Version GetAgentVersionSafe()
        {
            Version version = new Version(1, 0);

            try
            {
                //We're in the agent assembly, so we just need to get our executing assembly.
                Assembly monitorAssembly = Assembly.GetExecutingAssembly();
                SessionAssemblyInfo assemblyInfo = new SessionAssemblyInfo(monitorAssembly, false);
                string rawVersion = assemblyInfo.FileVersion;

                //since this is our assembly, I can't imagine we'd ever get a null.  BUT...
                if (string.IsNullOrEmpty(rawVersion))
                    rawVersion = assemblyInfo.Version;

                if (string.IsNullOrEmpty(rawVersion) == false)
                    version = new Version(rawVersion);
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
            }

            return version;
        }

        private static Guid GetComputerIdSafe(string product, AgentConfiguration configuration)
        {
            Guid computerId = Guid.Empty;  //we can't fail, this is a good default value since upstream items will treat it as a "don't know"
            try
            {
                //first see if we have a GUID file in the system-wide location.
                var preferredPath = PathManager.FindBestPath(PathType.Collection);
                var computerIdFile = Path.Combine(preferredPath, LocalRepository.ComputerKeyFile);

                if (!File.Exists(computerIdFile))
                {
                    //see if we have a repository Id we should copy...
                    var repositoryPath = LocalRepository.CalculateRepositoryPath(product, configuration.SessionFile.Folder);
                    var repositoryIdFile = Path.Combine(repositoryPath, LocalRepository.RepositoryKeyFile);
                    if (File.Exists(repositoryIdFile))
                    {
                        //try to copy it as a candidate..
                        try
                        {
                            File.Copy(repositoryIdFile, computerIdFile, false);
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            if (Debugger.IsAttached)
                                Debugger.Break();
#endif
                            GC.KeepAlive(ex);
                        }
                    }
                }

                if (File.Exists(computerIdFile))
                {
                    //read back the existing repository id
                    string rawComputerId = File.ReadAllText(computerIdFile, Encoding.UTF8);
                    computerId = new Guid(rawComputerId);
                }

                //create a new repository id
                if (computerId == Guid.Empty)
                {
                    computerId = Guid.NewGuid();
                    File.WriteAllText(computerIdFile, computerId.ToString(), Encoding.UTF8);
                    File.SetAttributes(computerIdFile, FileAttributes.Hidden);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                if (Debugger.IsAttached)
                    Debugger.Break();
#endif
                GC.KeepAlive(ex);
                computerId = Guid.Empty; //we shouldn't trust anything we have- it's probably a dynamically created id.
            }

            return computerId;
        }

        private void SendPropertyChanged(String propertyName)
        {
            if ((PropertyChanged != null))
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}

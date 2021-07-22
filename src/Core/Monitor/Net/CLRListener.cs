
#region File Header

/********************************************************************
 * COPYRIGHT:
 *    This software program is furnished to the user under license
 *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
 *    U.S. and international law. This software program may not be 
 *    reproduced, transmitted, or disclosed to third parties, in 
 *    whole or in part, in any form or by any manner, electronic or
 *    mechanical, without the express written consent of Gibraltar Software, Inc,
 *    except to the extent provided for by applicable license.
 *
 *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Loupe.Extensibility.Data;
using Microsoft.Win32;

#endregion File Header

namespace Gibraltar.Monitor.Net
{
    /// <summary>
    /// Monitors the Common Language Runtime for noteworthy events.
    /// </summary>
    /// <remarks>This listener is automatically activated by the PerformanceMonitor class.</remarks>
    public class CLRListener : IDisposable
    {
        private const string ThisLogSystem = "Gibraltar";

        private bool m_Disposed;

        private bool m_MasterEventsEnabled; //protected by lock
        private bool m_AssemblyEventsEnabled; //protected by LOCK
        private bool m_AssemblyLoadFailureEventsEnabled; //protected by LOCK
        private bool m_NetworkEventsEnabled; //protected by LOCK
        private bool m_PowerEventsEnabled; //protected by LOCK
        private bool m_UserEventsEnabled; //protected by LOCK

        private bool m_SuppressEvents;
        private PowerLineStatus m_CurrentPowerLineStatus;
        private BatteryChargeStatus m_CurrentChargeStatus;

        //The assemblies dictionary is protected by ASSEMBLYLOCK
        private readonly Dictionary<string, SessionAssemblyInfo> m_Assemblies = new Dictionary<string, SessionAssemblyInfo>(StringComparer.OrdinalIgnoreCase);

        //the network states dictionary is protected by NETWORKSTATESLOCK
        private readonly Dictionary<string, NetworkState> m_NetworkStates = new Dictionary<string, NetworkState>(StringComparer.OrdinalIgnoreCase);

        private readonly object m_Lock = new object();
        private readonly object m_AssemblyLock = new object();
        private readonly object m_NetworkStatesLock = new object();

        #region Private Class MessageSource

        /// <summary>
        /// Provides method source to log method to prevent normal call stack interpretation 
        /// </summary>
        /// <remarks>Since this listener deals with CLR events the message source information isn't
        /// very interesting.  We don't want to pay the performance price of it doing its normal
        /// lookup so we'll override the behavior.</remarks>
        private class MessageSource : IMessageSourceProvider
        {
            public MessageSource(string className, string methodName)
            {
                MethodName = methodName;
                ClassName = className;
                FileName = null;
                LineNumber = 0;
            }

            /// <summary>
            /// Should return the simple name of the method which issued the log message.
            /// </summary>
            public string MethodName { get; private set; }

            /// <summary>
            /// Should return the full name of the class (with namespace) whose method issued the log message.
            /// </summary>
            public string ClassName { get; private set; }

            /// <summary>
            /// Should return the name of the file containing the method which issued the log message.
            /// </summary>
            public string FileName { get; private set; }

            /// <summary>
            /// Should return the line within the file at which the log message was issued.
            /// </summary>
            public int LineNumber { get; private set; }
        }

        #endregion

        #region Private Class NetworkState

        private class NetworkState
        {
            public NetworkState(NetworkInterface nic)
            {
                Id = nic.Id;
                Name = nic.Name;
                NetworkInterfaceType = nic.NetworkInterfaceType;
                Description = nic.Description;
                Speed = nic.Speed;
                OperationalStatus = nic.OperationalStatus;

                IPInterfaceProperties ipProperties = null;
                try
                {
                    ipProperties = nic.GetIPProperties();
                    DnsAddresses = ipProperties.DnsAddresses;
                    WinsServersAddresses = ipProperties.WinsServersAddresses;
                    GatewayAddresses = ipProperties.GatewayAddresses;
                    UnicastIPAddresses = ipProperties.UnicastAddresses;
                }
                catch
                {
                }

                /* KM: Not in use yet, waiting to see if we really wait this kind of detail.
                if (ipProperties != null)
                {
                    if (nic.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        try
                        {
                            IP4Properties = ipProperties.GetIPv4Properties();
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        IP4Properties = null;
                    }

                    if (nic.Supports(NetworkInterfaceComponent.IPv6))
                    {
                        try
                        {
                            IP6Properties = ipProperties.GetIPv6Properties();
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        IP6Properties = null;
                    }
                }
                */
            }

            public string Id { get; private set; }

            public string Name { get; private set; }

            public NetworkInterfaceType NetworkInterfaceType { get; private set; }

            public string Description { get; private set; }

            public long Speed { get; private set; }

            public OperationalStatus OperationalStatus { get; private set; }

            public IPAddressCollection DnsAddresses { get; private set; }

            public GatewayIPAddressInformationCollection GatewayAddresses { get; private set; }

            public IPAddressCollection WinsServersAddresses { get; private set; }

            public UnicastIPAddressInformationCollection UnicastIPAddresses { get; private set; }

            //            public IPv4InterfaceProperties IP4Properties { get; private set; }

            //            public IPv6InterfaceProperties IP6Properties { get; private set; }
        }

        #endregion

        #region Public Properties and Methods


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// <remarks>Calling Dispose() (automatic when a using statement ends) will generate the metric.</remarks>
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);

            //SuppressFinalize because there won't be anything left to finalize
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initialize the Common Language Runtime Listener with the provided configuration
        /// </summary>
        /// <param name="configuration"></param>
        public void Initialize(ListenerConfiguration configuration)
        {
            lock (m_Lock)
            {
                //make sure before we do anything that we enable our ability to monitor for reasons we need to spontaneously unregister.
                if (m_MasterEventsEnabled == false)
                    RegisterMasterEvents();

                if (configuration.EnableAssemblyEvents)
                {
                    if (m_AssemblyEventsEnabled == false)
                        RegisterAssemblyEvents();
                }
                else
                {
                    if (m_AssemblyEventsEnabled)
                        UnregisterAssemblyEvents();
                }

                if (configuration.EnableAssemblyLoadFailureEvents)
                {
                    if (m_AssemblyLoadFailureEventsEnabled == false)
                        RegisterAssemblyLoadFailureEvents();
                }
                else
                {
                    if (m_AssemblyLoadFailureEventsEnabled)
                        UnregisterAssemblyLoadFailureEvents();
                }

                if (configuration.EnableNetworkEvents)
                {
                    if (m_NetworkEventsEnabled == false)
                        RegisterNetworkEvents();
                }
                else
                {
                    if (m_NetworkEventsEnabled)
                        UnregisterNetworkEvents();
                }

                if (configuration.EnablePowerEvents)
                {
                    if (m_PowerEventsEnabled == false)
                        RegisterPowerEvents();
                }
                else
                {
                    if (m_PowerEventsEnabled)
                        UnregisterPowerEvents();
                }

                if (configuration.EnableUserEvents)
                {
                    if (m_UserEventsEnabled == false)
                        RegisterUserEvents();
                }
                else
                {
                    if (m_UserEventsEnabled)
                        UnregisterUserEvents();
                }
            }            
        }

        #endregion

        #region Protected Properties and Methods

        /// <summary>
        /// Performs the actual releasing of managed and unmanaged resources.
        /// Most usage should instead call Dispose(), which will call Dispose(true) for you
        /// and will suppress redundant finalization.
        /// </summary>
        /// <param name="releaseManaged">Indicates whether to release managed resources.
        /// This should only be called with true, except from the finalizer which should call Dispose(false).</param>
        protected virtual void Dispose(bool releaseManaged)
        {
            if (!m_Disposed)
            {
                if (releaseManaged)
                {
                    // Free managed resources here (normal Dispose() stuff, which should itself call Dispose(true))
                    // Other objects may be referenced in this case
                }
                
                // Free native resources here (alloc's, etc)
                // May be called from within the finalizer, so don't reference other objects here

                //because we're interfacing with system events we're going to go ahead and potentially reference
                //other objects because we're only talking to runtime internal objects (like the AppDomain itself)
                //NOTE: We are deliberately NOT using the lock because we don't want to risk a deadlock.
                if (m_MasterEventsEnabled)
                    UnregisterMasterEvents();

                if (m_AssemblyEventsEnabled)
                    UnregisterAssemblyEvents();

                if (m_NetworkEventsEnabled)
                    UnregisterNetworkEvents();

                if (m_PowerEventsEnabled)
                    UnregisterPowerEvents();

                if (m_UserEventsEnabled)
                    UnregisterUserEvents();

                m_Disposed = true; // Make sure we only do this once
            }
        }

        #endregion

        #region Private Properties and Methods

        private void RegisterAssemblyEvents()
        {
            try
            {
                m_AssemblyEventsEnabled = true;

                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyLoad += AppDomain_AssemblyLoad;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }


            try
            {
                //Now we need to ensure we've recorded the information on assemblies that have been loaded so far.
                EnsureAssembliesRecorded();
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        private void UnregisterAssemblyEvents()
        {
            try
            {
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyLoad -= AppDomain_AssemblyLoad;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
            finally
            {
                m_AssemblyEventsEnabled = false;
            }
        }

        private void RegisterAssemblyLoadFailureEvents()
        {
            try
            {
                m_AssemblyLoadFailureEventsEnabled = true;

                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyResolve += AppDomain_AssemblyResolve;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        private void UnregisterAssemblyLoadFailureEvents()
        {
            try
            {
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyResolve -= AppDomain_AssemblyResolve;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
            finally
            {
                m_AssemblyLoadFailureEventsEnabled = false;
            }
        }

        private void RegisterMasterEvents()
        {
            try
            {
                m_MasterEventsEnabled = true;
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.DomainUnload += AppDomain_DomainUnload;
                currentDomain.ProcessExit += AppDomain_ProcessExit;
                SystemEvents.EventsThreadShutdown += SystemEvents_EventsThreadShutdown;
                Application.ApplicationExit += Application_ApplicationExit;
                SystemEvents.TimeChanged += SystemEvents_TimeChanged;
                SystemEvents.SessionEnding += SystemEvents_SessionEnding; // Apparently we don't get SessionEnded soon enough (or ever?).
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        private void UnregisterMasterEvents()
        {
            try
            {
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.DomainUnload -= AppDomain_DomainUnload;
                currentDomain.ProcessExit -= AppDomain_ProcessExit;
                SystemEvents.EventsThreadShutdown -= SystemEvents_EventsThreadShutdown; //used so we know when to unregister our events.
                Application.ApplicationExit -= Application_ApplicationExit;
                SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
                SystemEvents.TimeChanged -= SystemEvents_TimeChanged;
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
            }
            finally
            {
                m_MasterEventsEnabled = false;
            }
        }

        private void RegisterNetworkEvents()
        {
            try
            {
                m_NetworkEventsEnabled = true;
                NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
                NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
            }

            try
            {
                EnsureNetworkInterfacesRecorded();
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        private void UnregisterNetworkEvents()
        {
            try
            {
                NetworkChange.NetworkAddressChanged -= NetworkChange_NetworkAddressChanged;
                NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged;
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
            }
            finally
            {
                m_NetworkEventsEnabled = false;
            }
        }

        private void RegisterPowerEvents()
        {
            //this event never works for ASP.NET (no windows form/message pump to work with), but otherwise it MIGHT work.
            if (Log.SessionSummary.AgentAppType != ApplicationType.AspNet)
            {
                //some of these events will be hit & miss depending on current permissions & process type.
                try
                {
                    m_PowerEventsEnabled = true;
                    SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
                }
                catch (Exception ex)
                {
                    GC.KeepAlive(ex);
#if DEBUG
                Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Gibraltar.Agent", "Failed to Register System Event",
                          "One or more system events couldn't be registered due to an exception, which can happen in certain processes and security scenarios.\r\nException ({0}): {1}",
                          ex.GetType().FullName, ex.Message);
#endif
                }
            }
        }

        private void UnregisterPowerEvents()
        {
            try
            {
                SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            }
            catch
            {
            }
            finally
            {
                m_PowerEventsEnabled = false;
            }
        }

        private void RegisterUserEvents()
        {
            //some of these events will be hit & miss depending on current permissions & process type.
            try
            {
                m_UserEventsEnabled = true;

                if ((Log.SessionSummary.AgentAppType == ApplicationType.Console) // TODO: Should this just check Environment.UserInteractive instead?
                    || (Log.SessionSummary.AgentAppType == ApplicationType.Windows))
                {
                    SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
                    SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
                }
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Gibraltar.Agent", "Failed to Register System Event",
                          "One or more system events couldn't be registered due to an exception, which can happen in certain processes and security scenarios.\r\nException ({0}): {1}",
                          ex.GetType().FullName, ex.Message);
#endif
            }
        }

        private void UnregisterUserEvents()
        {
            try
            {
                SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
                SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            }
            catch
            {
            }
            finally
            {
                m_UserEventsEnabled = false;
            }
        }

        /// <summary>
        /// Make sure we've recorded an SessionAssemblyInfo object for every assembly.
        /// </summary>
        private void EnsureAssembliesRecorded()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;

            //this is thread safe because we are getting an array of the assemblies, then iterating THAT.
            foreach (Assembly curAssembly in currentDomain.GetAssemblies())
            {
                RecordAssembly(curAssembly);
            }
        }

        private void EnsureNetworkInterfacesRecorded()
        {
            RecordNetworkState(false); // false if we want to record the initial baseline state without logging it.
            // It may not log it anyway, because we get initialized during Log initialization, so it just drops the writes.
        }

        private SessionAssemblyInfo RecordAssembly(Assembly newAssembly)
        {
            SessionAssemblyInfo newSessionAssemblyInfo = null;

            lock (m_AssemblyLock) // a little pathological locking in case we get an assembly load event.
            {
                string fullName = newAssembly.FullName;
                if ((string.IsNullOrEmpty(fullName) == false) && (m_Assemblies.ContainsKey(fullName) == false))
                {
                    newSessionAssemblyInfo = new SessionAssemblyInfo(newAssembly, Log.SessionSummary.PrivacyEnabled == false);
                    m_Assemblies.Add(fullName, newSessionAssemblyInfo);
                    Log.Write(newSessionAssemblyInfo.Packet);
                }

                System.Threading.Monitor.PulseAll(m_AssemblyLock);
            }

            return newSessionAssemblyInfo;
        }

        /// <summary>
        /// Convert a raw data rate number (in bps) into human-readable form.
        /// </summary>
        /// <remarks>Exact multiples of 1000 are bumped up to the next larger units, as are values exceeding four digits.
        /// Fractional units are displayed, if applicable, up to three digits (using InvariantCulture).
        /// Rates less than 1 Kbps (or less than 1 Mbps and containing fractional Kbps) are displayed as fractional Kbps.</remarks>
        /// <param name="bpsRate">The data rate (in bps--bits per second) to be displayed.</param>
        /// <returns>A string formatted as a rate with units.</returns>
        public static string FormatDataRate(long bpsRate)
        {
            // Notice that data rates use 1000 not 1024 for K/M/G factors; they are generally not powers of 2.

            long rate = bpsRate;
            long fraction = rate % 1000; // Get the fractional portion of Kbps.
            rate /= 1000; // Get the whole portion of Kbps.
            string unitString = "Kbps"; // Use these units by default

            if (rate >= 10000 || (rate >= 1000 && fraction == 0))
            {
                // At least 10 Mbps or it's some Mbps with exact Kbps fraction, let's use Mbps units...
                fraction = rate % 1000; // Get the fractional portion of Mbps.
                rate /= 1000; // Get the whole portion of Mbps.

                if (rate >= 10000 || (rate >= 1000 && fraction == 0))
                {
                    // At least 10 Gbps or it's some Gbps with exact Mbps fraction, let's use Gbps units...
                    fraction = rate % 1000; // Get the fractional portion of Gbps.
                    rate /= 1000; // Get the whole portion of Gbps.
                    unitString = "Gbps";
                }
                else
                {
                    // Otherwise, we're going with Mbps.
                    unitString = "Mbps";
                }
            }
            // Otherwise, we're going with the default Kbps.

            string formatString;

            if (fraction == 0)
            {
                // It's an exact amount of the selected units.  Display without fraction.
                formatString = string.Format(CultureInfo.InvariantCulture, "{0} {1}", rate, unitString); // No need for {0:N0} ?
            }
            else
            {
                // It's a fractional amount of the selected units.  Format for fractional decimal display.
                string fractionString = string.Empty;

                while (fraction > 0)
                {
                    fractionString += fraction / 100; // Append the next fractional digit.
                    fraction = (fraction * 10) % 1000; // Shift next digit to 100ths place, discard previous digit.
                }

                formatString = string.Format(CultureInfo.InvariantCulture, "{0}.{1} {2}", rate, fractionString, unitString);
            }

            return formatString;
        }

        private void RecordNetworkState(bool logChanges)
        {
            NetworkInterface[] adapters;
            try
            {
                // This "Only works on Linux and Windows" under Mono, so do in a try/catch to be safe.
                adapters = NetworkInterface.GetAllNetworkInterfaces();
            }
            catch
            {
                UnregisterNetworkEvents(); // Disable further network logging?
                return;
            }

            lock (m_NetworkStatesLock)
            {
                //now check each one (except loopback) to see if it's in our collection.
                foreach (NetworkInterface adapter in adapters)
                {
                    if ((adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        && (adapter.NetworkInterfaceType != NetworkInterfaceType.Tunnel))
                    {
                        NetworkState previousState;

                        if (m_NetworkStates.TryGetValue(adapter.Id, out previousState) == false)
                        {
                            //it's brand new - need to add it and record it as new.
                            previousState = new NetworkState(adapter);
                            m_NetworkStates.Add(previousState.Id, previousState);

                            if (logChanges)
                            {
                                string interfaceInfo = FormatNetworkAdapterState(previousState);
                                LogEvent(LogMessageSeverity.Verbose, "System.Events.Network", "Network Interface Detected", interfaceInfo);
                            }
                        }
                        else
                        {
                            //see if it changed.
                            bool hasChanged = false;
                            string changes = string.Empty;

                            NetworkState newState = new NetworkState(adapter);

                            if (newState.OperationalStatus != previousState.OperationalStatus)
                            {
                                hasChanged = true;
                                changes += string.Format(CultureInfo.InvariantCulture, "Operational Status Changed from {0} to {1}\r\n", previousState.OperationalStatus, newState.OperationalStatus);
                            }

                            if (newState.Speed != previousState.Speed)
                            {
                                hasChanged = true;
                                changes += string.Format(CultureInfo.InvariantCulture, "Speed Changed from {0} to {1}\r\n",
                                                         FormatDataRate(previousState.Speed), FormatDataRate(newState.Speed));
                            }

                            //find any IP configuration change.
                            if (IPConfigurationChanged(previousState, newState))
                            {
                                hasChanged = true;
                                changes += "TCP/IP Configuration Changed.\r\n";
                            }

                            if (hasChanged)
                            {
                                //replace the item in the collection with the new item
                                m_NetworkStates.Remove(previousState.Id);
                                m_NetworkStates.Add(newState.Id, newState);

                                if (logChanges)
                                {
                                    string interfaceInfo = FormatNetworkAdapterState(newState);
                                    LogEvent(LogMessageSeverity.Information, "System.Events.Network", "Network Interface Changes Detected", "\r\n{0}\r\nNew State:\r\n{1}", changes, interfaceInfo);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool IPConfigurationChanged(NetworkState previousState, NetworkState newState)
        {
            //Gateways            
            if ((previousState.GatewayAddresses != null) && (newState.GatewayAddresses == null))
            {
                return true;
            }

            if ((previousState.GatewayAddresses == null) && (newState.GatewayAddresses != null))
            {
                return true;
            }

            if ((previousState.GatewayAddresses != null) && (newState.GatewayAddresses != null))
            {
                //need to check addresses.
                if (previousState.GatewayAddresses.Count != newState.GatewayAddresses.Count)
                    return true;

                foreach (GatewayIPAddressInformation ipAddressInformation in newState.GatewayAddresses)
                {
                    bool foundOurAddress = false;
                    foreach (GatewayIPAddressInformation previousAddress in newState.GatewayAddresses)
                    {
                        //if this address is our previous address, we're ready to check the next address.
                        if (ipAddressInformation.Address.Equals(previousAddress.Address))
                        {
                            foundOurAddress = true;
                            break;
                        }
                    }

                    if (foundOurAddress == false)
                    {
                        //if we got through all of the gateways and didn't find our address, this is a change.
                        return true;
                    }
                }
            }

            //IP Addresses
            if ((previousState.UnicastIPAddresses != null) && (newState.UnicastIPAddresses == null))
            {
                return true;
            }

            if ((previousState.UnicastIPAddresses == null) && (newState.UnicastIPAddresses != null))
            {
                return true;
            }

            if ((previousState.UnicastIPAddresses != null) && (newState.UnicastIPAddresses != null))
            {
                //need to check addresses.
                if (previousState.UnicastIPAddresses.Count != newState.UnicastIPAddresses.Count)
                    return true;

                foreach (UnicastIPAddressInformation ipAddressInformation in newState.UnicastIPAddresses)
                {
                    bool foundOurAddress = false;
                    foreach (UnicastIPAddressInformation previousAddress in newState.UnicastIPAddresses)
                    {
                        //if this address is our previous address, we're ready to check the next address.
                        if (ipAddressInformation.Address.Equals(previousAddress.Address))
                        {
                            foundOurAddress = true;
                            break;
                        }
                    }

                    if (foundOurAddress == false)
                    {
                        //if we got through all of the gateways and didn't find our address, this is a change.
                        return true;
                    }
                }
            }

            //DNS            
            if ((previousState.DnsAddresses != null) && (newState.DnsAddresses == null))
            {
                return true;
            }

            if ((previousState.DnsAddresses == null) && (newState.DnsAddresses != null))
            {
                return true;
            }

            if ((previousState.DnsAddresses != null) && (newState.DnsAddresses != null))
            {
                //need to check addresses.
                if (previousState.DnsAddresses.Count != newState.DnsAddresses.Count)
                    return true;

                foreach (IPAddress ipAddressInformation in newState.DnsAddresses)
                {
                    bool foundOurAddress = false;
                    foreach (IPAddress previousAddress in newState.DnsAddresses)
                    {
                        //if this address is our previous address, we're ready to check the next (outer) address.
                        if (ipAddressInformation.Equals(previousAddress))
                        {
                            foundOurAddress = true;
                            break;
                        }
                    }

                    if (foundOurAddress == false)
                    {
                        //if we got through all of the gateways and didn't find our address, this is a change.
                        return true;
                    }
                }
            }

            //WINS            
            if ((previousState.WinsServersAddresses != null) && (newState.WinsServersAddresses == null))
            {
                return true;
            }

            if ((previousState.WinsServersAddresses == null) && (newState.WinsServersAddresses != null))
            {
                return true;
            }

            if ((previousState.WinsServersAddresses != null) && (newState.WinsServersAddresses != null))
            {
                //need to check addresses.
                if (previousState.WinsServersAddresses.Count != newState.WinsServersAddresses.Count)
                    return true;

                foreach (IPAddress ipAddressInformation in newState.WinsServersAddresses)
                {
                    bool foundOurAddress = false;
                    foreach (IPAddress previousAddress in newState.WinsServersAddresses)
                    {
                        //if this address is our previous address, we're ready to check the next (outer) address.
                        if (ipAddressInformation.Equals(previousAddress))
                        {
                            foundOurAddress = true;
                            break;
                        }
                    }

                    if (foundOurAddress == false)
                    {
                        //if we got through all of the gateways and didn't find our address, this is a change.
                        return true;
                    }
                }
            }


            return false; //if we got this far with nothing, no changes.
        }


        private static string FormatNetworkAdapterState(NetworkState adapterState)
        {
            StringBuilder stringBuild = new StringBuilder(1024);

            stringBuild.AppendFormat("Name: {0}\r\n", adapterState.Name);
            stringBuild.AppendFormat("Interface Type: {0}\r\n", adapterState.NetworkInterfaceType);
            stringBuild.AppendFormat("Description: {0}\r\n", adapterState.Description);


            string displayStatus;
            switch (adapterState.OperationalStatus)
            {
                case OperationalStatus.Up:
                    displayStatus = "UP: The network interface is up; it can transmit data packets.";
                    break;
                case OperationalStatus.Down:
                    displayStatus = "DOWN: The network interface is unable to transmit data packets.";
                    break;
                case OperationalStatus.Testing:
                    displayStatus = "TESTING: The network interface is running tests.";
                    break;
                case OperationalStatus.Unknown:
                    displayStatus = "UNKNOWN: The network interface status is not known.";
                    break;
                case OperationalStatus.Dormant:
                    displayStatus = "DORMANT: The network interface is not in a condition to transmit data packets; it is waiting for an external event.";
                    break;
                case OperationalStatus.NotPresent:
                    displayStatus = "NOT PRESENT: The network interface is unable to transmit data packets because of a missing component, typically a hardware component.";
                    break;
                case OperationalStatus.LowerLayerDown:
                    displayStatus = "LOWER LAYER DOWN: The network interface is unable to transmit data packets because it runs on top of one or more other interfaces, and at least one of these 'lower layer' interfaces is down.";
                    break;
                default:
                    displayStatus = adapterState.OperationalStatus.ToString();
                    break;
            }

            stringBuild.AppendFormat("Status: {0}\r\n", displayStatus);

            if (adapterState.OperationalStatus == OperationalStatus.Up)
            {

                //convert speed to Kbps
                stringBuild.AppendFormat("Maximum Speed: {0}\r\n", FormatDataRate(adapterState.Speed));

                //since we have at least one IP protocol, output general IP stuff
                stringBuild.AppendFormat("DNS Servers: {0}\r\n", FormatIPAddressList(adapterState.DnsAddresses));
                stringBuild.AppendFormat("WINS Servers: {0}\r\n", FormatIPAddressList(adapterState.WinsServersAddresses));
                stringBuild.AppendFormat("Gateways: {0}\r\n", FormatGatewayIPAddressList(adapterState.GatewayAddresses));

                stringBuild.AppendFormat("IPv4 Addresses: {0}\r\n", FormatUnicastAddressList(adapterState.UnicastIPAddresses, AddressFamily.InterNetwork));
                stringBuild.AppendFormat("IPV6 Addresses: {0}\r\n", FormatUnicastAddressList(adapterState.UnicastIPAddresses, AddressFamily.InterNetworkV6));

                //check the quality of the IP addresses:
                if (adapterState.UnicastIPAddresses != null)
                {
                    foreach (UnicastIPAddressInformation addressInformation in adapterState.UnicastIPAddresses)
                    {
                        if ((addressInformation.DuplicateAddressDetectionState != DuplicateAddressDetectionState.Preferred)
                            && (addressInformation.DuplicateAddressDetectionState != DuplicateAddressDetectionState.Deprecated))
                        {
                            string reason;

                            switch (addressInformation.DuplicateAddressDetectionState)
                            {
                                case DuplicateAddressDetectionState.Invalid:
                                    reason = "the address is not valid. A nonvalid address is expired and no longer assigned to an interface; applications should not send data packets to it.";
                                    break;
                                case DuplicateAddressDetectionState.Tentative:
                                    reason = "the duplicate address detection procedure's evaluation of the address has not completed successfully. Applications should not use the address because it is not yet valid and packets sent to it are discarded.";
                                    break;
                                case DuplicateAddressDetectionState.Duplicate:
                                    reason = "the address is not unique. This address should not be assigned to the network interface.";
                                    break;
                                default:
                                    reason = addressInformation.DuplicateAddressDetectionState.ToString();
                                    break;
                            }

                            stringBuild.AppendFormat("\r\nThe IP address {0} is not currently usable because {1}\r\n", addressInformation.Address, reason);
                        }
                    }
                }
            }

            return stringBuild.ToString();
        }

        private static string FormatUnicastAddressList(UnicastIPAddressInformationCollection addressCollection, AddressFamily family)
        {
            //figure out the IP4 addressees
            string ipAddresses;
            if (addressCollection == null)
            {
                ipAddresses = "NONE";
            }
            else if (addressCollection.Count == 0)
            {
                ipAddresses = "NONE";
            }
            else
            {
                ipAddresses = string.Empty;
                foreach (UnicastIPAddressInformation addressInformation in addressCollection)
                {
                    if (addressInformation.Address.AddressFamily == family)
                    {
                        if (string.IsNullOrEmpty(ipAddresses))
                        {
                            ipAddresses = addressInformation.Address.ToString();
                        }
                        else
                        {
                            ipAddresses += ", " + addressInformation.Address;
                        }
                    }
                }
            }

            return ipAddresses;
        }

        private static string FormatIPAddressList(IPAddressCollection addressCollection)
        {
            if (addressCollection == null)
                return "NONE";

            if (addressCollection.Count == 0)
                return "NONE";

            string addresses = string.Empty;

            foreach (IPAddress ipAddress in addressCollection)
            {
                if (string.IsNullOrEmpty(addresses))
                {
                    addresses = ipAddress.ToString();
                }
                else
                {
                    addresses += ", " + ipAddress;
                }
            }

            return addresses;
        }

        private static string FormatGatewayIPAddressList(GatewayIPAddressInformationCollection addressCollection)
        {
            if (addressCollection == null)
                return "NONE";

            if (addressCollection.Count == 0)
                return "NONE";

            string addresses = "";

            foreach (GatewayIPAddressInformation ipAddress in addressCollection)
            {
                if (string.IsNullOrEmpty(addresses))
                {
                    addresses += ipAddress.Address.ToString();
                }
                else
                {
                    addresses += ", " + ipAddress.Address;
                }
            }

            return addresses;
        }

        private static string FormatBatteryChargeDescription(PowerStatus powerStatus)
        {
            TimeSpan batteryLife = new TimeSpan(0, 0, powerStatus.BatteryLifeRemaining);

            string description = string.Format("Battery Charge Capacity: {0}%\r\nEstimated Runtime Remaining: {1}\r\nBattery Status: {2}\r\n",
                                               powerStatus.BatteryLifePercent, batteryLife, powerStatus.BatteryChargeStatus);

            return description;
        }

        private static void LogEvent(LogMessageSeverity severity, LogWriteMode mode, string category, string caption, string description, params object[] args)
        {
            MessageSource source = new MessageSource("Gibraltar.Agent.Net.CLRListener", "LogEvent");
            Log.WriteMessage(severity, mode, ThisLogSystem, category, source, null, null, null, caption, description, args);
        }

        private static void LogEvent(LogMessageSeverity severity, string category, string caption, string description, params object[] args)
        {
            MessageSource source = new MessageSource("Gibraltar.Agent.Net.CLRListener", "LogEvent");
            Log.WriteMessage(severity, LogWriteMode.Queued, ThisLogSystem, category, source, null, null, null, caption, description, args);
        }

        #endregion

        #region Event Handlers

        private void SystemEvents_EventsThreadShutdown(object sender, EventArgs e)
        {
            try
            {
                UnregisterUserEvents();
                UnregisterPowerEvents();

                //and if we haven't already received our session end call then log this.
                if (!Log.IsSessionEnding)
                {
                    LogEvent(LogMessageSeverity.Verbose, "System.Events", "System Event Recording Stopping", "The events thread is shutting down, no more system events will be recorded.");
                }
            }
            catch (Exception ex)
            {
                ErrorNotifier.Notify(null, ex);
            }
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (m_SuppressEvents)
                return;

            try
            {
                LogEvent(LogMessageSeverity.Information, "System.Events.Display", "Display Settings Changed", "The active display settings have been changed.");
            }
            catch (Exception ex)
            {
                ErrorNotifier.Notify(null, ex);
            }
        }

        private void SystemEvents_TimeChanged(object sender, EventArgs e)
        {
            if (m_SuppressEvents)
                return;

            try
            {
                LogEvent(LogMessageSeverity.Information, "System.Events.Time", "System Time Changed", "The new time is {0} in time zone {1}",
                         DateTimeOffset.Now, TimeZone.CurrentTimeZone.StandardName);
            }
            catch (Exception ex)
            {
                ErrorNotifier.Notify(null, ex);
            }
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (m_SuppressEvents)
                return;

            try
            {
                string reason = "(Unknown)";
                switch (e.Reason)
                {
                    case SessionEndReasons.Logoff:
                        reason = "user logoff";
                        break;
                    case SessionEndReasons.SystemShutdown:
                        reason = "system shutdown";
                        break;
                }

                LogEvent(LogMessageSeverity.Information, "System.Events.Session", "Current Logon Session Ending", "The current logon session is ending due to {0}.", reason);
                
                // Apparently we may not get our usual exit events in this case, so we need to do an EndSession() call here.
                // We can't wait for the SessionEnded event because it either never comes or it comes too late to be effective.
                MessageSource source = new MessageSource("Gibraltar.Agent.Net.CLRListener", "ExitEvent");
                Log.EndSession(SessionStatus.Normal, source, reason);
            }
            catch (Exception ex)
            {
                ErrorNotifier.Notify(null, ex);
            }
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (m_SuppressEvents)
                return;

            try
            {
                string reason;
                switch (e.Reason)
                {
                    case SessionSwitchReason.ConsoleConnect:
                        reason = "a session has been connected from the console.";
                        break;
                    case SessionSwitchReason.ConsoleDisconnect:
                        reason = "a session has been disconnected from the console.";
                        break;
                    case SessionSwitchReason.RemoteConnect:
                        reason = "a session has been connected from a remote connection.";
                        break;
                    case SessionSwitchReason.RemoteDisconnect:
                        reason = "a session has been disconnected from a remote connection.";
                        break;
                    case SessionSwitchReason.SessionLogon:
                        reason = "a user has logged on to a session.";
                        break;
                    case SessionSwitchReason.SessionLogoff:
                        reason = "a user has logged off from a session.";
                        break;
                    case SessionSwitchReason.SessionLock:
                        reason = "a session has been locked.";
                        break;
                    case SessionSwitchReason.SessionUnlock:
                        reason = "a session has been unlocked.";
                        break;
                    case SessionSwitchReason.SessionRemoteControl:
                        reason = "a session has changed its status to or from remote controlled mode.";
                        break;
                    default:
                        reason = e.Reason.ToString();
                        break;
                }

                LogEvent(LogMessageSeverity.Information, "System.Events.Session", "Current Logon Session Switching", "The current logon session is switching because {0}", reason);
            }
            catch (Exception ex)
            {
                ErrorNotifier.Notify(null, ex);
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            try
            {
                LogMessageSeverity severity = LogMessageSeverity.Information;
                string caption = null, description = null;
                LogWriteMode writeMode = LogWriteMode.Queued;
                switch (e.Mode)
                {
                    case PowerModes.Resume:
                        caption = "System Resuming from Sleep";
                        description = "The operating system is about to resume from a suspended state.";
                        m_SuppressEvents = false; //now we re-enable events because most of the noise should be past us.
                        break;
                    case PowerModes.Suspend:
                        caption = "System Going to Sleep";
                        description = "The operating system is about to be suspended.";
                        writeMode = LogWriteMode.WaitForCommit;
                        m_SuppressEvents = true; // so we don't record the wide range of events that will now happen...
                        break;
                    case PowerModes.StatusChange:
                        //is the System Information object available? it's in the Forms namespace, so we might be hosed.
                        PowerStatus newPowerStatus = SystemInformation.PowerStatus;
                        if ((m_CurrentChargeStatus != newPowerStatus.BatteryChargeStatus)
                            || (m_CurrentPowerLineStatus != newPowerStatus.PowerLineStatus))
                        {
                            //it's a real change.  We tend to get bogus change events...
                            //we ignore the battery status if we're not on battery.
                            if ((m_CurrentPowerLineStatus == PowerLineStatus.Online) 
                                && (newPowerStatus.PowerLineStatus != PowerLineStatus.Online))
                            {
                                //we are transitioning to battery power, report that and the power percentage.
                                caption = "Switching to Battery Power";
                                description = FormatBatteryChargeDescription(newPowerStatus);
                            }
                            else if ((newPowerStatus.PowerLineStatus == PowerLineStatus.Online)
                                && (m_CurrentPowerLineStatus != PowerLineStatus.Online))
                            {
                                //we are transitioning from offline or unknown to AC, so report that.
                                caption = "Switching to Line Power";
                                description = "The system is transitioning from battery or other backup power to normal line power.";
                            }
                            else if ((m_CurrentPowerLineStatus == PowerLineStatus.Online)
                                && (newPowerStatus.PowerLineStatus == PowerLineStatus.Online))
                            {
                                //we are and were online, so the only event we might care about is if the battery is now fully charged.
                                //but really, who cares about that.  We record the capacity when we go on battery.
                            }
                            else if (m_CurrentChargeStatus != newPowerStatus.BatteryChargeStatus)
                            {
                                //we are and were offline.  Now we care about battery charge status.
                                switch (newPowerStatus.BatteryChargeStatus)
                                {
                                    case BatteryChargeStatus.High:
                                        caption = "Battery fully charged";
                                        description = FormatBatteryChargeDescription(newPowerStatus);
                                        break;
                                    case BatteryChargeStatus.Low:
                                        caption = "Battery is Low";
                                        description = FormatBatteryChargeDescription(newPowerStatus);
                                        break;
                                    case BatteryChargeStatus.Critical:
                                        severity = LogMessageSeverity.Warning;
                                        caption = "Battery is Critically Low";
                                        description = FormatBatteryChargeDescription(newPowerStatus);
                                        break;
                                    case BatteryChargeStatus.Charging:
                                        caption = "Battery is Charging";
                                        description = FormatBatteryChargeDescription(newPowerStatus);
                                        break;
                                    case BatteryChargeStatus.NoSystemBattery:
                                        caption = "No System Battery";
                                        description = "The battery has been removed / no battery detected.";
                                        break;
                                }
                            }

                            //and whatever state we now have, it's the current and reported state.
                            m_CurrentPowerLineStatus = newPowerStatus.PowerLineStatus;
                            m_CurrentChargeStatus = newPowerStatus.BatteryChargeStatus;
                        }
                        break;
                }

                if (string.IsNullOrEmpty(caption) == false)
                    LogEvent(severity, writeMode, "System.Events.Power", caption, description);
            }
            catch (Exception ex)
            {
                ErrorNotifier.Notify(null, ex);
            }
        }

        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (m_SuppressEvents)
                return;

            try
            {
                string caption, description;
                if (e.IsAvailable)
                {
                    caption =  "Network is Now Available";
                    description = "One or more network interfaces are now connected to a network.";
                }
                else
                {
                    caption = "Network Not Available";
                    description = "No network interfaces are connected to a network.";
                }

                LogEvent(LogMessageSeverity.Information, "System.Events.Network", caption, description);
            }
            catch (Exception ex)
            {
                ErrorNotifier.Notify(null, ex);
            }
        }

        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            if (m_SuppressEvents)
                return;

            try
            {
                //we don't get any specific information from the event so we'll have to figure it out.
                RecordNetworkState(true);
            }
            catch (Exception ex)
            {
                ErrorNotifier.Notify(null, ex);
            }
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            // This will handle application exits regardless of which log system we are set up on and won't shut down
            // our central Log until the process actually does a clean exit, even if they dynamically remove our
            // listeners/appenders/etc from a given log system (and perhaps add them back in!).

            if (Log.ExplicitStartSessionCalled == false) // Don't EndSession here if the client had called StartSession().
            {
                // We have to detach our event handler(s) because the application exit event is a static event,
                // and we don't know if we might be bound to an AppDomain which can exit while the process continues.
                // Hmmm, maybe that doesn't apply for a static handler.  But unregister anyway, we only need it once.
                Application.ApplicationExit -= Application_ApplicationExit;

                Log.EndSession(SessionStatus.Normal, new MessageSourceProvider("Gibraltar.Monitor.Net.CLRListener", "ExitEvent"), "ApplicationExit event received.");
            }
        }

        private static void AppDomain_DomainUnload(object sender, EventArgs e)
        {
            Application.ApplicationExit -= Application_ApplicationExit;

            // If this flushes successfully then we consider it a normal exit.
            Log.EndSession(SessionStatus.Normal, new MessageSourceProvider("Gibraltar.Monitor.Net.CLRListener", "ExitEvent"), "DomainUnload event received.");
        }

        private static void AppDomain_ProcessExit(object sender, EventArgs e)
        {
            Application.ApplicationExit -= Application_ApplicationExit;

            // If this flushes successfully then we consider it a normal exit.
            // (This needs to be fast, limited to 3 seconds for all ProcessExit handlers.)
            Log.EndSession(SessionStatus.Normal, new MessageSourceProvider("Gibraltar.Monitor.Net.CLRListener", "ExitEvent"), "ProcessExit event received.");
        }

        private void AppDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            try
            {
                SessionAssemblyInfo newAssembly = RecordAssembly(args.LoadedAssembly);

                //only log it if we got a new assembly back - otherwise it was a dupe or irrelevant.
                if (newAssembly != null)
                {
                    string location = newAssembly.Location;
                    if (string.IsNullOrEmpty(location))
                    {
                        if (Log.SessionSummary.PrivacyEnabled)
                            location = "(suppressed for privacy)";
                        else
                            location = "(unknown)";
                    }

                    LogEvent(LogMessageSeverity.Verbose, "System.Events.Assembly",
                        (string.IsNullOrEmpty(newAssembly.Name) ? "New Assembly Loaded" : "New Assembly Loaded - " + newAssembly.Name),
                        "Name: {0}\r\nFull name: {1}\r\nLocation: {2}\r\n",
                        newAssembly.Name, newAssembly.FullName, location);
                }
            }
            catch (Exception ex)
            {
                ErrorNotifier.Notify(null, ex);
            }
        }

        private Assembly AppDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                bool ignoreResolution = false;
                //wait:  the .NET runtime is constantly generating this event for XML Serialization...
                if (args.Name.Contains(".XmlSerializers"))
                    ignoreResolution = true;

                if (ignoreResolution == false)
                {
                    LogEvent(LogMessageSeverity.Warning, "System.Events.Assembly", string.Format("Assembly '{0}' Not Found", args.Name),
                             "The .NET runtime is attempting to load a referenced assembly and couldn't find it using normal resolution methods.\r\nIf the application supports dynamic assembly registration through the AssemblyResolve event it may still be located, in which case a log message indicating it was loaded will be recorded after this message.");
                }
            }
            catch (Exception ex)
            {
                ErrorNotifier.Notify(null, ex);
            }
            return null; //we aren't modifying the state of whether it could resolve or not.
        }

        #endregion
    }
}

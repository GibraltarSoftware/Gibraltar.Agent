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
using System.Xml;
using Gibraltar.Agent;
using Gibraltar.Monitor;
using Gibraltar.Monitor.Windows;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Messaging
{
    /// <summary>
    /// Configuration information for the Live Viewer.
    /// </summary>
    public class ViewerMessengerConfiguration : MessengerConfiguration
    {
        private XmlNode m_GibraltarNode;

        /// <summary>
        /// Load the viewer messenger configuration from the application configuration
        /// </summary>
        /// <param name="name"></param>
        /// <param name="configuration"></param>
        internal ViewerMessengerConfiguration(string name, ViewerElement configuration)
            : base(name, typeof(MonitorMessenger).AssemblyQualifiedName)
        {
            Initialize(configuration);
        }

        /// <summary>
        /// Load the viewer messenger configuration from an XML document
        /// </summary>
        /// <param name="name"></param>
        /// <param name="gibraltarNode"></param>
        internal ViewerMessengerConfiguration(string name, XmlNode gibraltarNode)
            : base(name, typeof(MonitorMessenger).AssemblyQualifiedName)
        {
            m_GibraltarNode = gibraltarNode;

            //create an element object so we have something to draw defaults from.
            ViewerElement baseline = new ViewerElement();
            Initialize(baseline);

            //see if we have any configuration node for the listener...
            XmlNode node = gibraltarNode.SelectSingleNode("viewer");

            //copy the provided configuration
            if (node != null)
            {
                ClearMessagesButtonText = AgentConfiguration.ReadValue(node, "clearMessagesButtonText", baseline.ClearMessagesButtonText);
                ClearMessagesButtonTextVisible = AgentConfiguration.ReadValue(node, "clearMessagesButtonTextVisible", baseline.ClearMessagesButtonTextVisible);
                DefaultFilterLevel = AgentConfiguration.ReadValue(node, "defaultFilterLevel", baseline.DefaultFilterLevel);
                EnableIndependentSeverityFilters = AgentConfiguration.ReadValue(node, "enableIndependentSeverityFilters", baseline.EnableIndependentSeverityFilters);
                EnableMultiSelection = AgentConfiguration.ReadValue(node, "enableMultiSelection", baseline.EnableMultiSelection);
                ForceSynchronous = AgentConfiguration.ReadValue(node, "forceSynchronous", baseline.ForceSynchronous);
                FormTitleText = AgentConfiguration.ReadValue(node, "formTitleText", baseline.FormTitleText);
                ShowToolBar = AgentConfiguration.ReadValue(node, "showToolBar", baseline.ShowToolBar);
                ShowVerboseMessages = AgentConfiguration.ReadValue(node, "showVerboseMessages", baseline.ShowVerboseMessages);
                HotKey = AgentConfiguration.ReadValue(node, "hotKey", baseline.HotKey);
                PauseButtonText = AgentConfiguration.ReadValue(node, "pauseButtonText", baseline.PauseButtonText);
                PauseButtonTextVisible = AgentConfiguration.ReadValue(node, "pauseButtonTextVisible", baseline.PauseButtonTextVisible);
                ResetSearchButtonText = AgentConfiguration.ReadValue(node, "resetSearchButtonText", baseline.ResetSearchButtonText);
                ResetSearchButtonTextVisible = AgentConfiguration.ReadValue(node, "resetSearchButtonTextVisible", baseline.ResetSearchButtonTextVisible);
                RunButtonText = AgentConfiguration.ReadValue(node, "runButtonText", baseline.RunButtonText);
                RunButtonTextVisible = AgentConfiguration.ReadValue(node, "runButtonTextVisible", baseline.RunButtonTextVisible);
                ShowDetailsButton = AgentConfiguration.ReadValue(node, "showDetailsButton", baseline.ShowDetailsButton);
                ShowDetailsInGrid = AgentConfiguration.ReadValue(node, "showDetailsInGrid", baseline.ShowDetailsInGrid);
                ShowDetailsInTooltips = AgentConfiguration.ReadValue(node, "showDetailsInTooltips", baseline.ShowDetailsInTooltips);
                ShowMessageCounters = AgentConfiguration.ReadValue(node, "showMessageCounters", baseline.ShowMessageCounters);

                Enabled = AgentConfiguration.ReadValue(node, "enabled", baseline.Enabled);
                MaxQueueLength = AgentConfiguration.ReadValue(node, "maxQueueLength", baseline.MaxQueueLength);
            }

            //and clean up values
            Sanitize();
        }

        #region Public Properties and Methods

        /// <summary>
        /// The default HotKey configuration string.
        /// </summary>
        public const string DefaultHotKey = ViewerElement.DefaultHotKey;

        /// <summary>
        /// The key sequence used to pop up the live viewer.
        /// </summary>
        public string HotKey { get; set; }

        /// <summary>
        /// Specifies how many messages to buffer in the viewer.  Set to zero for unlimited buffer size.
        /// </summary>
        public int MaxMessages { get; set; }

        /// <summary>
        /// If false Suppresses the display of verbose messages.
        /// </summary>
        public bool ShowVerboseMessages { get; set; }

        /// <summary>
        /// Caption for the live viewer form.
        /// </summary>
        public string FormTitleText { get; set; }

        /// <summary>
        /// Specifies the default value for the filter.  If not set, no messages will be filtered.
        /// </summary>
        public LogMessageSeverity DefaultFilterLevel { get; set; }

        /// <summary>
        /// Causes each of the message severity filter buttons to operate independently
        /// </summary>
        public bool EnableIndependentSeverityFilters { get; set; }

        /// <summary>
        /// Specifies whether the Show Details button should be visible in the toolbar
        /// </summary>
        public bool ShowDetailsButton { get; set; }

        /// <summary>
        /// Specifies whether the grid includes developer details about threads and calling method
        /// </summary>
        public bool ShowDetailsInGrid { get; set; }

        /// <summary>
        /// Specifies whether tooltips include developer details about threads and calling method
        /// </summary>
        public bool ShowDetailsInTooltips { get; set; }

        /// <summary>
        /// Specifies whether the severity filter buttons should display message counts next to the icon
        /// </summary>
        public bool ShowMessageCounters { get; set; }

        /// <summary>
        /// Caption text for Run button
        /// </summary>
        public string RunButtonText { get; set; }

        /// <summary>
        /// Specifies whether the Run button should display caption text next to the icon
        /// </summary>
        public bool RunButtonTextVisible { get; set; }

        /// <summary>
        /// Caption text for Pause button
        /// </summary>
        public string PauseButtonText { get; set; }

        /// <summary>
        /// Specifies whether the Pause button should display caption text next to the icon
        /// </summary>
        public bool PauseButtonTextVisible { get; set; }

        /// <summary>
        /// Caption text for Reset Search button
        /// </summary>
        public string ResetSearchButtonText { get; set; }

        /// <summary>
        /// Specifies whether the Reset Search button should display caption text next to the icon
        /// </summary>
        public bool ResetSearchButtonTextVisible { get; set; }

        /// <summary>
        /// Caption text for Clear Messages button
        /// </summary>
        public string ClearMessagesButtonText { get; set; }

        /// <summary>
        /// Specifies whether the Clear Messages button should display caption text next to the icon
        /// </summary>
        public bool ClearMessagesButtonTextVisible { get; set; }

        /// <summary>
        /// Shows or hides the built-in Tool Bar
        /// </summary>
        public bool ShowToolBar { get; set; }

        /// <summary>
        /// Enables selection of multiple rows or regions in the grid.  Use with ctrl-C to copy.
        /// </summary>
        public bool EnableMultiSelection { get; set; }

        /// <summary>
        /// Save the configuration to the specified XML node.
        /// </summary>
        /// <param name="gibraltarNode"></param>
        public void Save(XmlNode gibraltarNode)
        {
            //create an instance of the element so we can compare against baseline.
            ViewerElement baseline = new ViewerElement();

            XmlNode newNode = gibraltarNode.OwnerDocument.CreateElement("sessionFile");
            AgentConfiguration.WriteValue(newNode, "enabled", Enabled, baseline.Enabled);
            AgentConfiguration.WriteValue(newNode, "forceSynchronous", ForceSynchronous, baseline.ForceSynchronous);
            AgentConfiguration.WriteValue(newNode, "maxQueueLength", MaxQueueLength, baseline.MaxQueueLength);

            AgentConfiguration.WriteValue(newNode, "hotKey", HotKey ?? string.Empty, baseline.HotKey);
            AgentConfiguration.WriteValue(newNode, "maxMessages", MaxMessages, baseline.MaxMessages);
            AgentConfiguration.WriteValue(newNode, "clearMessagesButtonText", ClearMessagesButtonText ?? string.Empty, baseline.ClearMessagesButtonText);
            AgentConfiguration.WriteValue(newNode, "clearMessagesButtonTextVisible", ClearMessagesButtonTextVisible, baseline.ClearMessagesButtonTextVisible);
            AgentConfiguration.WriteValue(newNode, "defaultFilterLevel", DefaultFilterLevel, baseline.DefaultFilterLevel);
            AgentConfiguration.WriteValue(newNode, "enableIndependentSeverityFilters", EnableIndependentSeverityFilters, baseline.EnableIndependentSeverityFilters);
            AgentConfiguration.WriteValue(newNode, "enableMultiSelection", EnableMultiSelection, baseline.EnableMultiSelection);
            AgentConfiguration.WriteValue(newNode, "formTitleText", FormTitleText ?? string.Empty, baseline.FormTitleText);
            AgentConfiguration.WriteValue(newNode, "pauseButtonText", PauseButtonText ?? string.Empty, baseline.PauseButtonText);
            AgentConfiguration.WriteValue(newNode, "pauseButtonTextVisible", PauseButtonTextVisible, baseline.PauseButtonTextVisible);
            AgentConfiguration.WriteValue(newNode, "resetSearchButtonText", ResetSearchButtonText ?? string.Empty, baseline.ResetSearchButtonText);
            AgentConfiguration.WriteValue(newNode, "resetSearchButtonTextVisible", ResetSearchButtonTextVisible, baseline.ResetSearchButtonTextVisible);
            AgentConfiguration.WriteValue(newNode, "runButtonText", RunButtonText ?? string.Empty, baseline.RunButtonText);
            AgentConfiguration.WriteValue(newNode, "runButtonTextVisible", RunButtonTextVisible, baseline.RunButtonTextVisible);
            AgentConfiguration.WriteValue(newNode, "showToolBar", ShowToolBar, baseline.ShowToolBar);
            AgentConfiguration.WriteValue(newNode, "showVerboseMessages", ShowVerboseMessages, baseline.ShowVerboseMessages);
            AgentConfiguration.WriteValue(newNode, "showDetailsButton", ShowDetailsButton, baseline.ShowDetailsButton);
            AgentConfiguration.WriteValue(newNode, "showDetailsInGrid", ShowDetailsInGrid, baseline.ShowDetailsInGrid);
            AgentConfiguration.WriteValue(newNode, "showDetailsInTooltips", ShowDetailsInTooltips, baseline.ShowDetailsInTooltips);
            AgentConfiguration.WriteValue(newNode, "showMessageCounters", ShowMessageCounters, baseline.ShowMessageCounters);

            //now, only add this node to the gibraltar node if we actually wrote out an attribute (e.g. we have at least one non-default value)
            if (newNode.Attributes.Count > 0)
            {
                gibraltarNode.AppendChild(newNode);
            }
        }

        #endregion
        
        #region Internal Properties and Methods

        internal void Sanitize()
        {
            if (string.IsNullOrEmpty(HotKey))
                HotKey = DefaultHotKey;

            if (MaxMessages <= 0)
                MaxMessages = LiveLogViewer.DefaultMaxMessages;

            if (string.IsNullOrEmpty(FormTitleText))
                FormTitleText = "Gibraltar Live Log Viewer";

            if (string.IsNullOrEmpty(PauseButtonText))
                FormTitleText = "Pause";

            if (string.IsNullOrEmpty(ResetSearchButtonText))
                FormTitleText = "Reset";

            if (string.IsNullOrEmpty(ClearMessagesButtonText))
                FormTitleText = "Clear";

            if (string.IsNullOrEmpty(RunButtonText))
                FormTitleText = "Click to Auto Refresh";

            if (MaxQueueLength <= 0)
                MaxQueueLength = 2000;

            if (MaxQueueLength > 50000)
                MaxQueueLength = 50000;
        }

        #endregion

        #region Private Properties and methods

        private void Initialize(ViewerElement configuration)
        {
            //stock base configuration
            Enabled = configuration.Enabled;
            ForceSynchronous = configuration.ForceSynchronous;
            MaxQueueLength = configuration.MaxQueueLength;

            //and our extensions
            ClearMessagesButtonText = configuration.ClearMessagesButtonText;
            ClearMessagesButtonTextVisible = configuration.ClearMessagesButtonTextVisible;
            DefaultFilterLevel = configuration.DefaultFilterLevel;
            EnableIndependentSeverityFilters = configuration.EnableIndependentSeverityFilters;
            EnableMultiSelection = configuration.EnableMultiSelection;
            MaxMessages = configuration.MaxMessages;
            FormTitleText = configuration.FormTitleText;
            ShowToolBar = configuration.ShowToolBar;
            ShowVerboseMessages = configuration.ShowVerboseMessages;
            HotKey = configuration.HotKey;
            PauseButtonText = configuration.PauseButtonText;
            PauseButtonTextVisible = configuration.PauseButtonTextVisible;
            ResetSearchButtonText = configuration.ResetSearchButtonText;
            ResetSearchButtonTextVisible = configuration.ResetSearchButtonTextVisible;
            RunButtonText = configuration.RunButtonText;
            RunButtonTextVisible = configuration.RunButtonTextVisible;
            ShowDetailsButton = configuration.ShowDetailsButton;
            ShowDetailsInGrid = configuration.ShowDetailsInGrid;
            ShowDetailsInTooltips = configuration.ShowDetailsInTooltips;
            ShowMessageCounters = configuration.ShowMessageCounters;            
        }
        
        #endregion
    }
}

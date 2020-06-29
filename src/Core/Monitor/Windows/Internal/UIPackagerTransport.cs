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

#region Usings

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Gibraltar.Windows.UI;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Monitor.Windows.Internal
{
    internal partial class UIPackagerTransport : UserControl, IUIWizardInputPage
    {
        private const string DefaultTitle = "Where do you want to send this information?";
        private const string DefaultInstructions = "Pick where you want to send this information from the list below then click Next to package and send your information.";
        private const string SingleOptionInstructions = "Complete the following then click Next to package and send your information.";

        private readonly PackagerConfiguration m_Configuration;
        private PackagerRequest m_Request;
        private bool m_NextEnabled;
        private string m_Title;
        private string m_Instructions;

        private sealed class SafeDriveInfo
        {
            public SafeDriveInfo(DriveInfo driveInfo)
            {
                DriveInfo = driveInfo;

                //determine our caption.
                if (DriveInfo.IsReady)
                {
                    string volumeLabel;
                    try
                    {
                        volumeLabel = DriveInfo.VolumeLabel; // Mono: This "only works on Unix".  Not clear if an exception is possible.
                    }
                    catch (Exception ex)
                    {
                        // ToDo: Perhaps remove this warning after testing confirms whether or not exceptions occur under Mono for each platform.
                        Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Gibraltar.Agent.Packager", "Unable to read VolumeLabel",
                                  "An attempt to read the VolumeLabel of a drive threw an exception.");
                        volumeLabel = string.Empty;
                    }

                    Caption = string.Format("{0}: {1} ({2:N0} free)", DriveInfo.Name, volumeLabel, DriveInfo.AvailableFreeSpace);
                }
                else
                {
                    Caption = string.Format("{0}: {1}", DriveInfo.Name, DriveInfo.DriveType);
                }
            }

            public DriveInfo DriveInfo { get; private set; }

            public string Caption { get; private set; }
        }

        /// <summary>
        /// Create a new packager transport wizard step
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="request"></param>
        /// <remarks>We need the request to be provided so we can check whether different transport options are allowed
        /// for this specific request (due to caller overrides and configuration), but our internal request object 
        /// has not yet been set (will be during Initialize).</remarks>
        public UIPackagerTransport(PackagerConfiguration configuration, PackagerRequest request)
        {
            m_Configuration = configuration;

            InitializeComponent();

            FormTools.ApplyOSFont(this);

            //apply some changes based on the configuration.  

            if (request.AllowEmail == false)
            {
                optSendToEmail.Visible = false;
                emailOptionsPanel.Visible = false;
            }
            else
            {
                //sub-variations:  They can't enter things that are specified in configuration.
                if (string.IsNullOrEmpty(m_Configuration.DestinationEmailAddress) == false)
                {
                    lblDestinationEmailAddress.Visible = false;
                    txtEmailAddress.Visible = false;

                    //move up the from option in case it's visible.
                    fromEmailOptionsPanel.Top = 0;
                    emailOptionsPanel.Height = fromEmailOptionsPanel.Height;

                    txtEmailAddress.Text = m_Configuration.DestinationEmailAddress; //so we still pass our audit check
                }

                if (string.IsNullOrEmpty(m_Configuration.FromEmailAddress) == false)
                {
                    fromEmailOptionsPanel.Visible = false;

                    emailOptionsPanel.Height = optSaveToFile.Height; //now it's the same height as the rest - at least we're constantly spaced.

                    txtFromEmailAddress.Text = m_Configuration.FromEmailAddress; //so we still pass our audit check
                }
            }

            if (request.AllowFile == false)
            {
                optSaveToFile.Visible = false;
                fileOptionsPanel.Visible = false;
            }

            if (request.AllowRemovableMedia == false)
            {
                optSaveToRemovableMedia.Visible = false;
                removableMediaOptionsPanel.Visible = false;
            }
        }

        #region Public Properties and Methods

#pragma warning disable 0067
        /// <summary>
        /// Raised whenever the IsValid property changes
        /// </summary>
        public event EventHandler IsValidChanged;

#pragma warning restore 0067


        /// <summary>
        /// Initialize the page to start a new wizard working with the provided request data.
        /// </summary>
        /// <param name="requestData"></param>
        public void Initialize(object requestData)
        {
            m_Request = (PackagerRequest)requestData;
            m_NextEnabled = false; // a safe default value
            InitializeDriveCombo();
        }

        /// <summary>
        /// The end-user title for this page in the wizard.
        /// </summary>
        public string Title { get { return m_Title; } }

        /// <summary>
        /// End user instructions for completing this page of the wizard.
        /// </summary>
        public string Instructions { get { return m_Instructions; } }

        /// <summary>
        /// Query if the wizard page allows moving next.
        /// </summary>
        public bool IsValid { get { return m_NextEnabled; } }

        /// <summary>
        /// The user has requested to move next
        /// </summary>
        public bool Store()
        {
            //store the user's selections
            if (optSaveToRemovableMedia.Checked)
            {
                m_Request.Transport = PackageTransport.RemovableMedia;

                DriveInfo selectedDrive = (DriveInfo)cboVolumeSelect.SelectedValue;
                m_Request.FileNamePath = selectedDrive.RootDirectory.FullName;
            }
            else if (optSaveToFile.Checked)
            {
                m_Request.Transport = PackageTransport.File;
                m_Request.FileNamePath = txtFileNamePath.Text;
            }
            else if (optSendToEmail.Checked)
            {
                m_Request.Transport = PackageTransport.Email;
                m_Request.DestinationEmailAddress = txtEmailAddress.Text;
                m_Request.FromEmailAddress = txtFromEmailAddress.Text;
            }

            return true;
        }

        /// <summary>
        /// The wizard controller is entering this page.
        /// </summary>
        public void OnEnter()
        {
            //if the transport is server we'll have to fix that - we can't transfer to server here 
            //(if the server could be contacted, we'd have already done it)
            if (m_Request.Transport == PackageTransport.Server)
            {
                //uh oh.  We have to pick something to see what we CAN be
                if (m_Request.AllowEmail)
                {
                    m_Request.Transport = PackageTransport.Email;
                }
                else if (m_Request.AllowFile)
                {
                    m_Request.Transport = PackageTransport.File;
                }
                else if (m_Request.AllowRemovableMedia)
                {
                    m_Request.Transport = PackageTransport.RemovableMedia;
                }
                else
                {
                    //we will not be able to initialize correctly.
                    throw new InvalidOperationException("The server couldn't be contacted, no other communication method is allowed, and somehow we got to the package transport page.  huh.");
                }
            }

            //hide/show areas that they are allowed to select.
            int transportModesAllowed = 0;
            string singleOptionTitle = null;
            if (m_Request.AllowEmail)
            {
                transportModesAllowed++;
                optSendToEmail.Visible = true;
                lblSendToEmail.Visible = true;
                emailOptionsPanel.Visible = true;
                singleOptionTitle = lblSendToEmail.Text;
            }
            else
            {
                optSendToEmail.Visible = false;
                lblSendToEmail.Visible = false;
                emailOptionsPanel.Visible = false;
            }

            if (m_Request.AllowFile)
            {
                transportModesAllowed++;
                optSaveToFile.Visible = true;
                lblSaveToFile.Visible = true;
                fileOptionsPanel.Visible = true;
                singleOptionTitle = lblSaveToFile.Text;
            }
            else
            {
                optSaveToFile.Visible = false;
                lblSaveToFile.Visible = false;
                fileOptionsPanel.Visible = false;
            }

            if (m_Request.AllowRemovableMedia)
            {
                transportModesAllowed++;
                optSaveToRemovableMedia.Visible = true;
                lblSaveToRemovableMedia.Visible = true;
                removableMediaOptionsPanel.Visible = true;
                singleOptionTitle = lblSaveToRemovableMedia.Text;
            }
            else
            {
                optSaveToRemovableMedia.Visible = false;
                lblSaveToRemovableMedia.Visible = false;
                removableMediaOptionsPanel.Visible = false;
            }

            if (transportModesAllowed == 1)
            {
                m_Title = singleOptionTitle;
                m_Instructions = SingleOptionInstructions;
                optSendToEmail.Visible = false;
                optSaveToFile.Visible = false;
                optSaveToRemovableMedia.Visible = false;
            }
            else
            {
                m_Title = DefaultTitle;
                m_Instructions = DefaultInstructions;
                lblSendToEmail.Visible = false;
                lblSaveToFile.Visible = false;
                lblSaveToRemovableMedia.Visible = false;
            }

            switch(m_Request.Transport)
            {
                case PackageTransport.File:
                    optSaveToFile.Checked = true;
                    break;
                case PackageTransport.RemovableMedia:
                    optSaveToRemovableMedia.Checked = true;
                    break;
                case PackageTransport.Email:
                    optSendToEmail.Checked = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            txtEmailAddress.Text = m_Request.DestinationEmailAddress;
            txtFileNamePath.Text = m_Request.FileNamePath;

            ValidateData();
        }

        #endregion

        #region Protected properties and Methods

        protected void OnIsValidChanged()
        {
            EventHandler tempEvent = IsValidChanged;

            if (tempEvent != null)
            {
                tempEvent.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Private Properties and Methods

        private void ActionFileBrowse()
        {
            using(SaveFileDialog fileSaveDialog = new SaveFileDialog())
            {
                fileSaveDialog.Filter = Log.FileFilterPackagesOnly;
                fileSaveDialog.RestoreDirectory = true;

                if (string.IsNullOrEmpty(txtFileNamePath.Text) == false)
                {
                    fileSaveDialog.FileName = txtFileNamePath.Text;
                }

                DialogResult result = fileSaveDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    txtFileNamePath.Text = fileSaveDialog.FileName;
                    ValidateData();
                }
            }
        }

        private void ValidateData()
        {
            bool valid = true;

            //one option must be selected
            if ((optSaveToRemovableMedia.Checked == false) 
                && (optSendToEmail.Checked == false) 
                && (optSaveToFile.Checked == false))
            {
                valid = false;
                txtEmailAddress.Enabled = false;
                txtFileNamePath.Enabled = false;
                cboVolumeSelect.Enabled = false;
                btnBrowse.Enabled = false;
            }
            else
            {
                //we process all of the UI enable/disable in one blow at the bottom of this else case.
                bool emailOptionsEnabled = false;
                bool fileOptionsEnabled = false;
                bool removableMediaOptionsEnabled = false;

                if (optSaveToRemovableMedia.Checked)
                {
                    removableMediaOptionsEnabled = true;

                    if (cboVolumeSelect.SelectedValue == null)
                    {
                        valid = false;
                    }
                    else
                    {
                        //make sure the selected volume is writeable - put in a test file.
                        DriveInfo selectedDrive = (DriveInfo)cboVolumeSelect.SelectedValue;
                        if (IsPathValid(selectedDrive.RootDirectory.FullName) == false)
                        {
                            valid = false;
                            lblRemovableMediaNotValid.Visible = true;
                        }
                        else
                        {
                            lblRemovableMediaNotValid.Visible = false;
                        }
                    }
                }
                else if (optSendToEmail.Checked)
                {
                    emailOptionsEnabled = true;

                    if (string.IsNullOrEmpty(txtEmailAddress.Text))
                    {
                        valid = false;
                    }
                }
                else if (optSaveToFile.Checked)
                {
                    fileOptionsEnabled = true;

                    string trimmedPath = (txtFileNamePath.Text ?? string.Empty).Trim();
                    if (string.IsNullOrEmpty(trimmedPath))
                    {
                        valid = false;
                    }
                    else
                    {
                        //lets see if the path is plausible
                        try
                        {
                            string providedPath = Path.GetDirectoryName(trimmedPath);
                            valid = IsPathValid(providedPath);
                        }
                        catch
                        {
                            valid = false;
                        }
                    }
                }

                //all of the enable/disable based on user selection.
                lblDestinationEmailAddress.Enabled = emailOptionsEnabled;
                lblFromEmailAddress.Enabled = emailOptionsEnabled;
                lblFromEmailAddressInstructions.Enabled = emailOptionsEnabled;
                txtEmailAddress.Enabled = emailOptionsEnabled;
                txtFromEmailAddress.Enabled = emailOptionsEnabled;

                lblFileName.Enabled = fileOptionsEnabled;
                txtFileNamePath.Enabled = fileOptionsEnabled;
                btnBrowse.Enabled = fileOptionsEnabled;

                lblDrive.Enabled = removableMediaOptionsEnabled;
                cboVolumeSelect.Enabled = removableMediaOptionsEnabled;
                lblNoRemovableMedia.Enabled = removableMediaOptionsEnabled;
                lblRemovableMediaNotValid.Enabled = removableMediaOptionsEnabled;
                btnRefreshRemovableMedia.Enabled = removableMediaOptionsEnabled;
            }

            SetNextEnabled(valid);
        }

        private static bool IsPathValid(string fullPath)
        {
            bool pathIsValid = true;
            string testFileNamePath = Path.Combine(fullPath, Guid.NewGuid().ToString());

            try
            {
                File.WriteAllText(testFileNamePath, "Gibraltar Packager Test File");
            }
            catch (Exception ex)
            {
                string description;
                object[] args;
                if (Log.SessionSummary.PrivacyEnabled)
                {
                    // Anonymous mode is enabled, just output a simple message with no file paths.
                    description = "While verifying write access to the selected path, an exception was thrown. The path will not be considered valid yet.";
                    args = null;
                }
                else
                {
                    description = "While verifying write access to the selected path, an exception was thrown. The path will not be considered valid yet.\r\nPath: {0}\r\nException: {1}";
                    args = new object[]
                    {
                        fullPath,
                        ex.Message
                    };
                }

                Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Gibraltar.Packager", "Error while Validating Packager Input", description, args);
                pathIsValid = false;
            }
            finally
            {
                //try to safe delete the file in case it's partially there.
                try
                {
                    File.Delete(testFileNamePath);
                }
                catch
                {
                }
            }

            return pathIsValid;
        }

        private void SetNextEnabled(bool enabled)
        {
            if (enabled == m_NextEnabled)
                return;

            m_NextEnabled = enabled;

            OnIsValidChanged();
        }

        private void InitializeDriveCombo()
        {
            DriveInfo[] allDrives = null;
            if (m_Configuration.AllowRemovableMedia)
            {
                cboVolumeSelect.DisplayMember = "Caption";
                cboVolumeSelect.ValueMember = "DriveInfo";

                try
                {
                    allDrives = DriveInfo.GetDrives(); // Note: Mono only implements for Linux.
                }
                catch (Exception ex)
                {
                    // ToDo: Reduce severity of this warning to Information before release?
                    Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Gibraltar.Agent.Packager", "Unable to scan drive information",
                              "An exception prevented scanning of available drives.  This may be a limitation of the runtime on this platform.  " +
                              "The packager will not be able to detect any removable media on this system.");
                    allDrives = null;
                    m_Configuration.AllowRemovableMedia = false; // Disable because we can't read it.
                }
            }

            if (allDrives != null) // Only if GetDrives() is supported...
            {
                BindingList<SafeDriveInfo> safeDriveInfos = new BindingList<SafeDriveInfo>();
                foreach (DriveInfo currentDrive in allDrives)
                {
                    //we don't want to stumble into an odd machine-specific problem accessing the 
                    //properties of a drive that's not ready or something.
                    try
                    {
                        if ((currentDrive.DriveType == DriveType.Removable) || (currentDrive.DriveType == DriveType.Network))
                        {
                            //we definitely want this one in our list, and we might want to select it
                            SafeDriveInfo newDrive = new SafeDriveInfo(currentDrive);
                            safeDriveInfos.Add(newDrive);
                        }
                    }
                    catch
                    {                        
                    }
                }
                cboVolumeSelect.DataSource = safeDriveInfos;

                //now go find the best value we can.
                foreach (SafeDriveInfo safeDriveInfo in safeDriveInfos)
                {
                    //we don't want to stumble into an odd machine-specific problem accessing the 
                    //properties of a drive that's not ready or something.
                    try
                    {
                        if ((safeDriveInfo.DriveInfo.IsReady) && (cboVolumeSelect.SelectedItem == null))
                        {
                            cboVolumeSelect.SelectedItem = safeDriveInfo;
                        }
                    }
                    catch
                    {
                    }
                }

                if (safeDriveInfos.Count == 0)
                {
                    //display our no drives label.
                    lblDrive.Visible = false;
                    cboVolumeSelect.Visible = false;
                    lblRemovableMediaNotValid.Visible = false; 
                    lblNoRemovableMedia.Location = lblDrive.Location;
                    lblNoRemovableMedia.Visible = true;
                }
                else
                {
                    //hide the no media label
                    lblNoRemovableMedia.Visible = false;
                    lblDrive.Visible = true;
                    cboVolumeSelect.Visible = true;
                }
            }
        }

        #endregion

        #region Event Handlers

        private void txtEmailAddress_TextChanged(object sender, EventArgs e)
        {
            ValidateData();
        }

        private void txtFileNamePath_TextChanged(object sender, EventArgs e)
        {
            ValidateData();
        }

        private void cboVolumeSelect_SelectedValueChanged(object sender, EventArgs e)
        {
            ValidateData();
        }

        private void Option_Click(object sender, EventArgs e)
        {
            ValidateData();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            ActionFileBrowse();
        }

        private void mainLayoutPanel_Resize(object sender, EventArgs e)
        {
            if (Visible)
            {
                int interiorWidth = mainLayoutPanel.Width - mainLayoutPanel.Margin.Left - mainLayoutPanel.Margin.Right;

                //resize our contained panels
                if (emailOptionsPanel.Visible)
                    emailOptionsPanel.Width = interiorWidth;

                if (fileOptionsPanel.Visible)
                    fileOptionsPanel.Width = interiorWidth;

                if (removableMediaOptionsPanel.Visible)
                    removableMediaOptionsPanel.Width = interiorWidth;
            }
        }

        private void btnRefreshRemovableMedia_Click(object sender, EventArgs e)
        {
            InitializeDriveCombo();
        }

        #endregion
    }
}

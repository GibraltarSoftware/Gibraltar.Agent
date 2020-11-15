
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
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

#endregion File Header

namespace Gibraltar.Windows.UI
{
    /// <summary>
    /// Maintains user-specific application preference information.
    /// </summary>
    /// <remarks>This class will automatically persist users preferences and will automatically
    /// resize the form on form load as well as when the characteristics of the system change (such as logging in 
    /// through terminal server or when a laptop changes screen resolution or number of screens).</remarks>
    public class FormUserPreferences : ApplicationSettingsBase, IDisposable
    {
        private readonly Form m_MainWindow;

        private bool m_RecalulateDisplaySize;
        private bool m_EventsAttached; //keeps us from attaching the events twice by some bizarre sequence of events.

        private delegate void ApplyUserDisplayPreferencesCallback();

        /// <summary>
        /// Create a new form preferences object for the specified form.
        /// </summary>
        /// <remarks>Settings are managed individually by form type, so in the same application
        /// multiple user preference objects can be used, however there will only be one set of 
        /// preferences maintained for a given form type regardless of the number of instances.</remarks>
        /// <param name="mainWindow">The sizable non-MDI Child form to manage.</param>
        /// <param name="preserveFormDefaults">True to not change the form's designer size on first time use</param>
        /// <param name="settingsKey">Optional. An explicit settings key to use instead of the default type-specific value</param>
        public FormUserPreferences(Form mainWindow, bool preserveFormDefaults = false, string settingsKey = null)
            : base(mainWindow, settingsKey ?? mainWindow.GetType().Name)
        {
            m_MainWindow = mainWindow;

            //Rationalize user preferences with the current window environment
            RationalizeMainWindowSettings(preserveFormDefaults);

            //now apply user preferences
            ApplyMainWindowSettings();

            //we now need to start tracking screen resolution changes to be good & proper.
            SystemEvents.DisplaySettingsChanging += SystemEvents_DisplaySettingsChanging;

            //we need to bind up a bunch of events to the form so we can monitor subsequent resizes, etc.
            AttachEvents();
        }

        #region Public Properties and Methods

        /// <summary>
        /// The height of the form.
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("0")]
        public int MainWindowHeight
        {
            get { return (int)this["MainWindowHeight"]; }
            set
            {
                this["MainWindowHeight"] = value;
            }
        }

        /// <summary>
        /// The width of the form.
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("0")]
        public int MainWindowWidth
        {
            get { return (int)this["MainWindowWidth"]; }
            set
            {
                this["MainWindowWidth"] = value;
            }
        }


        /// <summary>
        /// The offset from the screen desktop to the left edge of the form.
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("40")]
        public int MainWindowLeft
        {
            get { return (int)this["MainWindowLeft"]; }
            set
            {
                this["MainWindowLeft"] = value;
            }
        }

        /// <summary>
        /// The offset from the screen desktop to the top of the form.
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("30")]
        public int MainWindowTop
        {
            get { return (int)this["MainWindowTop"]; }
            set
            {
                this["MainWindowTop"] = value;
            }
        }

        /// <summary>
        /// Indicates if the form should be maximized.
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("false")]
        public bool MainWindowMaximized
        {
            get { return (bool)this["MainWindowMaximized"]; }
            set
            {
                this["MainWindowMaximized"] = value;
            }
        }

        /// <summary>
        /// Indicates if the user has approved sending data
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("false")]
        public bool AutoSendSessionData
        {
            get { return (bool)this["AutoSendSessionData"]; }
            set
            {
                this["AutoSendSessionData"] = value;
            }
        }


        /// <summary>
        /// Release resources immediately.
        /// </summary>
        public void Dispose()
        {
            // Call the underlying implementation
            Dispose(true);

            //SuppressFinalize because there won't be anything left to finalize
            GC.SuppressFinalize(this);
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
            if (releaseManaged)
            {
                // Free managed resources here (normal Dispose() stuff, which should itself call Dispose(true))
                // Other objects may be referenced in this case

                //release our system event handle, if it was registered.  If not registered, this won't generate an error.
                SystemEvents.DisplaySettingsChanging -= SystemEvents_DisplaySettingsChanging;
            }
            // Free native resources here (alloc's, etc)
            // May be called from within the finalizer, so don't reference other objects here
        }

        /// <summary>
        /// Applies the current user display preferences to the current form in a thread-safe manner
        /// </summary>
        protected void ApplyMainWindowSettings()
        {
            //we may need to use invoke because SystemEvents have a habit of coming from a thread other than the UI thread.
            if (m_MainWindow.InvokeRequired)
            {
                //we are in fact not on the right thread.
                ApplyUserDisplayPreferencesCallback d = ApplyMainWindowSettings;

                //There are some timing cases where this invoke could throw an exception - like we're right in the middle of
                //hiding because we're already complete.  We don't want these exceptions.
                try
                {
                    m_MainWindow.Invoke(d);
                }
                catch (Exception ex)
                {
                    //we don't care, but log it in debug mode.
#if DEBUG
                    Trace.TraceError(ex.ToString());
#else
                    GC.KeepAlive(ex);
#endif
                }
            }
            else
            {
                //we know we're on the right thread, we're good to go.
                m_MainWindow.Location = new Point(MainWindowLeft, MainWindowTop);
                m_MainWindow.Size = new Size(MainWindowWidth, MainWindowHeight);

                if (MainWindowMaximized)
                {
                    m_MainWindow.WindowState = FormWindowState.Maximized;
                }
            }
        }

        /// <summary>
        /// Rationalizes the main window location and size based on the current display configuration.
        /// </summary>
        /// <returns>True if any settings were changed, false otherwise.</returns>
        protected virtual bool RationalizeMainWindowSettings(bool preserveFormDefaults)
        {
            bool settingsChanged = false;

            //if this is the first time we start up we actually want to maximize and then give the user an alternate size for non-maximized.
            bool firstTimeStartup = false;
            if ((MainWindowWidth == 0) && (MainWindowHeight == 0))
            {
                if (preserveFormDefaults)
                {
                    MainWindowWidth = m_MainWindow.Width;
                    MainWindowHeight = m_MainWindow.Height;
                }
                else
                {
                    firstTimeStartup = true;
                    MainWindowMaximized = true;
                }
            }

            //First, find out what screen we should be on now - our window point may be off screen
            int windowMidLeft = 0, windowMidTop = 0;

            //we need to find where the center of the preferred window is to map to a target screen. 
            //this is consistent with where Maximize would go.
            if (MainWindowWidth > 0)
            {
                windowMidLeft = MainWindowLeft + (MainWindowWidth / 2);
            }

            if (MainWindowHeight > 0)
            {
                windowMidTop = MainWindowTop + (MainWindowHeight / 2);
            }
            Screen targetScreen = Screen.FromPoint(new Point(windowMidLeft, windowMidTop));

            //screen from point will always return a screen, but we might be on the extreme edge of it.  We need to check our bounds to the bounds of this screen.
            //size is more important that location.

            //make sure our individual dimensions fit within the available working area, but don't bother with maximizing
            if (targetScreen.WorkingArea.Height < MainWindowHeight)
            {
                settingsChanged = true;
                MainWindowHeight = targetScreen.WorkingArea.Height;
            }
            else if (firstTimeStartup)
            {
                MainWindowHeight = (int)(0.75 * targetScreen.WorkingArea.Height);
            }

            if (targetScreen.WorkingArea.Width < MainWindowWidth)
            {
                settingsChanged = true;
                MainWindowWidth = targetScreen.WorkingArea.Width;
            }
            else if (firstTimeStartup)
            {
                MainWindowWidth = (int)(0.75 * targetScreen.WorkingArea.Width);
            }

            //now we know we can fit on the screen so we just need to make sure we adjust our location to fit the
            //"three corners rule":  The top right & left and either of the bottom corners has to be on the screen.
            //ORDER IS IMPORTANT TO THE NEXT FOUR EVALUATIONS

            //To figure out the farthest right we can be, we have to offset by where the target screen starts (which could even be in negative space)
            int maxLeft = targetScreen.WorkingArea.Left + targetScreen.WorkingArea.Width - MainWindowWidth;
            if (maxLeft < MainWindowLeft)
            {
                //too far right - we'd be off the screen, so slide left.
                settingsChanged = true;
                MainWindowLeft = maxLeft;
            }


            //To figure out the farthest down we can be, we have to offset by where the target screen starts (which could even be in negative space)
            int maxTop = targetScreen.WorkingArea.Top + targetScreen.WorkingArea.Height - MainWindowHeight;
            if (maxTop < MainWindowTop)
            {
                //too far down - we'd be off the screen, so slide up.
                settingsChanged = true;
                MainWindowTop = maxTop;
            }


            //to figure out the farthest left we can be, we need our left to be greater or equal to the left wherever that window is
            Screen leftScreen = Screen.FromPoint(new Point(MainWindowLeft, MainWindowTop));
            if (leftScreen.WorkingArea.Left > MainWindowLeft)
            {
                //too far left - we'd be off the screen, so slide right.
                settingsChanged = true;
                MainWindowLeft = leftScreen.WorkingArea.Left;
            }

            //to figure out the farthest UP we can be, we need our top to be greater or equal to the top for either corner
            //the RIGHT is important because that's where the window buttons are, do that first.
            Screen topScreen = Screen.FromPoint(new Point(MainWindowLeft + MainWindowWidth, MainWindowTop));
            if (topScreen.WorkingArea.Top > MainWindowTop)
            {
                //too far up - we'd be off the screen, so slide down.
                settingsChanged = true;
                MainWindowTop = topScreen.WorkingArea.Top;
            }

            topScreen = Screen.FromPoint(new Point(MainWindowLeft, MainWindowTop));
            if (topScreen.WorkingArea.Top > MainWindowTop)
            {
                //too far up - we'd be off the screen, so slide down.
                settingsChanged = true;
                MainWindowTop = topScreen.WorkingArea.Top;
            }

            //let our caller know if we came up with a change.
            return settingsChanged;
        }
#endregion

        #region Private Properties and Methods

        private void AttachEvents()
        {
            if (m_EventsAttached == false)
            {
                m_EventsAttached = true;
                m_MainWindow.FormClosing += form_FormClosing;
                m_MainWindow.LocationChanged += form_LocationChanged;
                m_MainWindow.Resize += form_Resize;
                m_MainWindow.SizeChanged += form_SizeChanged;
            }
        }

        private void DetatchEvents()
        {
            SystemEvents.DisplaySettingsChanging -= SystemEvents_DisplaySettingsChanging;
            m_MainWindow.FormClosing -= form_FormClosing;
            m_MainWindow.LocationChanged -= form_LocationChanged;
            m_MainWindow.Resize -= form_Resize;
            m_MainWindow.SizeChanged -= form_SizeChanged;
        }

        #endregion

        #region Event Handlers

        void SystemEvents_DisplaySettingsChanging(object sender, EventArgs e)
        {
            //we need to recalculate the user's display preferences based on the current display values and if they are different
            //put them into effect.  This only matters if we are a normal window; the others are handled normally.
            if  (m_MainWindow.WindowState == FormWindowState.Normal)
            {
                if (RationalizeMainWindowSettings(false))
                {
                    ApplyMainWindowSettings();
                }
            }
        }

        private void form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Trace.TraceInformation("Form {0} closing\r\nClose reason: {1}",m_MainWindow.GetType().Name, e.CloseReason);

            //and if it's a successful close, save out our settings
            if (e.CloseReason == CloseReason.UserClosing)
            {
                //update our stored preferences.  Do NOT update height/width/etc here - we just want to update window state
                //this is because on Vista the height/width/left/top aren't accurate for maximized windows (they go beyond the window boundaries)
                Save();
            }

            //and detach all of our event handlers so we no longer have any references to it.
            DetatchEvents();
        }

        private void form_LocationChanged(object sender, EventArgs e)
        {
            //update our stored location preferences, as long as we are a normal window (and not being minimized/maximized)
            if (m_MainWindow.WindowState == FormWindowState.Normal)
            {
                MainWindowLeft = m_MainWindow.Left;
                MainWindowTop = m_MainWindow.Top;
            }
        }

        private void form_SizeChanged(object sender, EventArgs e)
        {
            //update our stored size preferences, as long as we are a normal window (and not being minimized/maximized)
            switch (m_MainWindow.WindowState)
            {
                case FormWindowState.Normal:
                    //store off the new size in our in-memory preferences.
                    MainWindowMaximized = false;
                    MainWindowHeight = m_MainWindow.Height;
                    MainWindowWidth = m_MainWindow.Width;
                    break;
                case FormWindowState.Minimized:
                    //because we're not going to be in a normal window state, we have to remind ourself to recalculate
                    //our window dimensions the next time we go back to normal.  I couldn't find any way to catch
                    //the transition to normal from an event.
                    m_RecalulateDisplaySize = true;
                    break;
                case FormWindowState.Maximized:
                    MainWindowMaximized = true;

                    //because we're not going to be in a normal window state, we have to remind ourself to recalculate
                    //our window dimensions the next time we go back to normal.  I couldn't find any way to catch
                    //the transition to normal from an event.
                    m_RecalulateDisplaySize = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void form_Resize(object sender, EventArgs e)
        {
            //if we are normal we may need to make sure our sizes aren't out of bounds.
            if ((m_MainWindow.WindowState == FormWindowState.Normal) && (m_RecalulateDisplaySize))
            {
                m_RecalulateDisplaySize = false;    //no loops for us!

                //we should re-rationalize our display settings 
                //just a little bogus:  We need to tell it right now that we're not maximized both for the rationalization calc
                //and because when we apply user preferences we don't want to go back to being maximized.
                MainWindowMaximized = false;
                if (RationalizeMainWindowSettings(false))
                {
                    //they changed, apply the changes
                    ApplyMainWindowSettings();
                }
            }
        }


        #endregion
    }
}
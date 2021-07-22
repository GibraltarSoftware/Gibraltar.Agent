#region File Header

using System.Windows.Forms;
using Gibraltar.Messaging;
using Gibraltar.Windows.UI;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Monitor.Windows.Internal
{
    /// <summary>
    /// This form hosts a copy of the LiveLogViewer control
    /// </summary>
    internal partial class LiveViewerForm : Form
    {
        private readonly LiveLogViewer m_Viewer;
        private readonly bool m_LocalViewer;

        /// <summary>
        /// Default constructor, generating its own LiveLogViewer Control.
        /// </summary>
        public LiveViewerForm()
            : this(null)
        {
            // Handled in the null case.
        }

        /// <summary>
        /// Construct a LiveViewerForm containing a pre-constructed LiveLogViewer Control.
        /// </summary>
        /// <param name="viewer">A LiveLogViewer Control created on the current thread.</param>
        internal LiveViewerForm(LiveLogViewer viewer)
        {
            m_Viewer = viewer;

            InitializeComponent();

            //set the correct OS font for ourself and our controls
            FormTools.ApplyOSFont(this);

            ApplyConfigurationSettings();

            if (m_Viewer == null)
            {
                m_Viewer = new LiveLogViewer(); // We weren't given one; we have to make our own local viewer.
                m_LocalViewer = true; // Mark that we own it, it's not from outside.
            }
            
            // Now attach the viewer control we were given.
            Controls.Add(m_Viewer);
            m_Viewer.Dock = DockStyle.Fill;
            m_Viewer.Visible = true;

            m_Viewer.SetAutoScroll(true); // Jump to end when the form is created.
        }

        /// <summary>
        /// Restore the form to the front of the Z order.
        /// </summary>
        /// <remarks>This must be called from the UI thread for this form.  The form will be restored from a Minimized
        /// state to Normal (but Maximized left unchanged), pushed to the front, and ensured to be Visible.  If the
        /// form was not already Visible, and if it was created with an externally-provided viewer control, then the
        /// viewer control will also be asked to jump to the last displayed log message.</remarks>
        internal void RestoreToFront()
        {
            // For a minimized window, restore it.
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;

            if (Visible == false && m_LocalViewer == false && m_Viewer != null)
                m_Viewer.SetAutoScroll(true); // If restoring from hidden, treat like a new form instance: resume AutoScroll.

            // This little bit with TopMost ensures that the window, if already open,
            // but beneath other windows, will be raised to the front.
            TopMost = true; // Raise us to the front...
            Show(); // Is this better than Visible = true ?  Probably can't do less, and might do a little more.
            TopMost = false; // ...But don't stick to the front!
        }

        /// <summary>
        /// Determine whether a given CloseReason indicates that the application is exiting or only this Form is closing.
        /// </summary>
        /// <param name="reason">The CloseReason from the FormClosedEventArgs.</param>
        /// <returns>True if the application is exiting.  False if only this Form is closing.</returns>
        internal static bool ReasonIsAppExiting(CloseReason reason)
        {
            switch (reason)
            {
                case CloseReason.WindowsShutDown:
                case CloseReason.TaskManagerClosing:
                case CloseReason.ApplicationExitCall:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Called by the Form when it is about to close, for any reason.
        /// </summary>
        /// <param name="e">FormClosingEventArgs providing the CloseReason.</param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            CloseReason reason = e.CloseReason;

#if DEBUG
            Log.Write(LogMessageSeverity.Verbose, "Gibraltar.Agent.Live Viewer", "Live Viewer Form closed",
                      "Local Viewer:  {0}\r\nClose Reason:  {1}", m_LocalViewer, reason);
#endif
            
            // Don't detach the viewer if we own it locally; let it close with us.
            // Also, don't bother to detach the viewer if we're closing because of an Application.Exit() call
            // or other type of general exit, because we may not be on the right UI thread anyway.
            if (m_LocalViewer == false && ReasonIsAppExiting(reason) == false && m_Viewer != null)
            {
                // If someone calls Close() from the wrong thread, let's try not break it.  Just in case.
                if (InvokeRequired)
                    Invoke(new MethodInvoker(DetachViewerControl)); // Synchronous invoke.  Get it done before we call base.
                else
                    DetachViewerControl(); // Remove our dynamically added LiveLogViewer Control.  It needs to stay around!
            }
#if DEBUG
            else
            {
                Log.Write(LogMessageSeverity.Verbose, "Gibraltar.Agent.Live Viewer", "Live Viewer control not detaching", null);
            }
#endif
            base.OnFormClosed(e);
        }

        private void DetachViewerControl()
        {
            // Don't detach the viewer if we own it locally; let it close with us.
            if (m_LocalViewer == false && m_Viewer != null && Controls.Contains(m_Viewer))
            {
#if DEBUG
                Log.Write(LogMessageSeverity.Verbose, "Gibraltar.Agent.Live Viewer", "Live Viewer control detaching", null);
#endif
                m_Viewer.Visible = false; // Before we remove it, stop showing it, so it doesn't do something stupid.
                Controls.Remove(m_Viewer);
                m_Viewer.Dock = DockStyle.None; // Before or after remove?
            }
        }

        protected override void OnVisibleChanged(System.EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (m_Viewer != null && Controls.Contains(m_Viewer))
                m_Viewer.Visible = Visible;
        }

        private void ApplyConfigurationSettings()
        {
            try
            {
                ViewerMessengerConfiguration configuration = Log.Configuration.Viewer;
                Text = configuration.FormTitleText;
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
            }
        }
    }
}

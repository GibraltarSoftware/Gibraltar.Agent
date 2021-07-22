
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Gibraltar.Monitor.Windows.Internal
{
    /// <summary>
    /// Utility class for properly disposing ToolStripTextBoxes.
    /// </summary>
    internal sealed class ToolStripTextBoxDisposeHelper
    {
        TextBox m_TextBox;
        int m_VisibleCount;

        /// <summary>
        /// Helper method for creating a ToolStripTextBoxDisposeHelper
        /// for each ToolStripTextBox in a ToolStrip.
        /// </summary>
        /// <param name="toolStrip">A ToolStrip containing ToolStripTextBoxes
        /// that should be tracked</param>
        public static void CreateToolStripDisposeHelpers(ToolStrip toolStrip)
        {
            foreach (ToolStripItem item in toolStrip.Items)
            {
                ToolStripTextBox toolStripTextBox = item as ToolStripTextBox;
                if (toolStripTextBox != null)
                {
                    new ToolStripTextBoxDisposeHelper(toolStripTextBox);
                }
            }
        }

        /// <summary>
        /// Initializes a new ToolStripTextBoxDisposeHelper.
        /// </summary>
        /// <remarks>
        /// Creating a new ToolStripTextBoxDisposeHelper will track
        /// visibility of the provided ToolStripTextBox and make sure
        /// that it is correctly disposed. There is no
        /// need to store a reference to the created instance. The
        /// helper will be kept alive by event handlers in the
        /// ToolStripTextBox.
        /// </remarks>
        /// <param name="textBox">The ToolStripTextBox to track.</param>
        public ToolStripTextBoxDisposeHelper(ToolStripTextBox textBox)
        {
            m_TextBox = textBox.TextBox;
            m_TextBox.VisibleChanged += textBox_VisibleChanged;
            m_TextBox.Disposed += textBox_Disposed;
        }

        void textBox_Disposed(object sender, EventArgs e)
        {
            // Remove all of the event handers we can get to, we don't want a leak...
            try
            {
                UserPreferenceChangedEventHandler eventHandler =
                  (UserPreferenceChangedEventHandler)Delegate.CreateDelegate(
                    typeof(UserPreferenceChangedEventHandler),
                    m_TextBox, "OnUserPreferenceChanged");
                for (int i = 0; i < m_VisibleCount + 1; i++)
                {
                    SystemEvents.UserPreferenceChanged -= eventHandler;
                }
            }
            catch (MissingMethodException)
            {
                // The UserPreferencesChanged implementation in the framework has
                // changed. Hopefully this memory leak has been fixed and we can
                // ignore that this hack didn't succeed.
            }
            catch (MemberAccessException)
            {
                // We don't have permissions to fix this problem using reflection.
                // Handle this somehow (currently we just rethrow the exception)
                throw;
            }
        }

        /// <summary>
        /// Count the number of times the TextBox has become visible
        /// (according to VisibleChanged)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void textBox_VisibleChanged(object sender, EventArgs e)
        {
            if (m_TextBox.Visible)
                m_VisibleCount++;
            else
                m_VisibleCount--;
        }
    }
}

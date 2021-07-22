
#region File Header

// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software, Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
//  *******************************************************************/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Gibraltar.Windows.UI;

#endregion

namespace Gibraltar.Monitor.Windows.Internal
{
    /// <summary>
    /// Displays a scrollable list of exceptions in a compact format
    /// </summary>
    internal partial class ExceptionListViewer : UserControl
    {
        private bool m_DisplayDate;
        private readonly List<ExceptionDisplayRequest> m_Exceptions = new List<ExceptionDisplayRequest>();

        /// <summary>
        /// Create a new exception viewer
        /// </summary>
        public ExceptionListViewer()
        {
            InitializeComponent();

            FormTools.ApplyOSFont(this);

            exceptionListTable.RowCount = 0;

            //this little beauty of a trick will prevent it from making the horizontal scroll.
            exceptionListTable.HorizontalScroll.Maximum = 0;
            exceptionListTable.AutoScroll = true;
        }

        /// <summary>
        /// Add the specified exception information to the list
        /// </summary>
        /// <param name="exception">The exception display request to add to the viewer</param>
        public void AddException(ExceptionDisplayRequest exception)
        {
            m_Exceptions.Add(exception);

            //See if we need to redraw the whole table because we now have multiple days involved and need
            //to show the date.
            if ((m_DisplayDate == false) && (exception.Timestamp.Date < DateTimeOffset.Now.Date))
            {
                //clear the table and redisplay all of the exceptions so we can display the right dates.
                m_DisplayDate = true; //so we don't do this each time.
                try
                {
                    exceptionListTable.SuspendLayout();

                    //remove all of the controls
                    exceptionListTable.Controls.Clear();

                    //set the row count to 0 to get rid of the visual rows
                    exceptionListTable.RowCount = 0;

                    foreach (ExceptionDisplayRequest currentException in m_Exceptions)
                    {
                        DisplayException(currentException);
                    }
                }
                finally
                {
                    exceptionListTable.ResumeLayout(true);
                }
            }
            else
            {
                //just add a new row to the existing table
                DisplayException(exception);
            }

            if (exceptionListTable.VerticalScroll.Visible)
            {
                Padding currentPadding = exceptionListTable.Padding;
                exceptionListTable.Padding = new Padding(currentPadding.Left, currentPadding.Top, SystemInformation.VerticalScrollBarWidth, currentPadding.Bottom);
            }
            else
            {
                Padding currentPadding = exceptionListTable.Padding;
                exceptionListTable.Padding = new Padding(currentPadding.Left, currentPadding.Top, currentPadding.Left, currentPadding.Bottom);
            }
        }

        /// <summary>
        /// Clear the list of exceptions.
        /// </summary>
        public void Clear()
        {
            try
            {
                m_Exceptions.Clear();

                //stop layout while we mess with it
                exceptionListTable.SuspendLayout();

                //remove all of the controls
                exceptionListTable.Controls.Clear();

                //set the row count to 0 to get rid of the visual rows
                exceptionListTable.RowCount = 0;
            }
            finally
            {
                //and resume layout
                exceptionListTable.ResumeLayout();
            }
        }

        #region Private Properties and Methods

        /// <summary>
        /// Add one exception to the exceptions table
        /// </summary>
        /// <param name="exception"></param>
        private void DisplayException(ExceptionDisplayRequest exception)
        {
            //add another row to the table
            exceptionListTable.RowCount++;
            Label dateTimeLabel = new Label();
            dateTimeLabel.Text = (exception.Timestamp.Date < DateTimeOffset.Now.Date) ? exception.Timestamp.ToString("G") : exception.Timestamp.ToString("T");
            dateTimeLabel.AutoSize = true;
            FormTools.ApplyOSFont(dateTimeLabel);

            Label captionLabel = new Label();
            captionLabel.Text = exception.Exception.Message;
            captionLabel.AutoSize = true;
            FormTools.ApplyOSFont(captionLabel);

            exceptionListTable.Controls.Add(dateTimeLabel, 0, exceptionListTable.RowCount - 1);
            exceptionListTable.Controls.Add(captionLabel, 1, exceptionListTable.RowCount - 1);
        }

        #endregion
    }
}

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
using Gibraltar.Data;
using Gibraltar.Windows.UI;

#endregion File Header

namespace Gibraltar.Monitor.Windows.Internal
{
    /// <summary>
    /// Performs custom sequencing for the configuration wizard
    /// </summary>
    internal class PackagerSequencer : IUIWizardSequencer
    {
        private readonly PackagerConfiguration m_Configuration;

        private WizardEngine m_Engine;
        private PackagerRequest m_Request;

        internal PackagerSequencer(PackagerConfiguration configuration)
        {
            m_Configuration = configuration;
        }

        #region Public Properties and Methods

        /// <summary>
        /// Called by the wizard engine to initialize the sequencer.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="request"></param>
        public void Initialize(WizardEngine engine, object request)
        {
            m_Engine = engine;
            m_Request = (PackagerRequest)request;
        }

        /// <summary>
        /// Move to the next page of the wizard from the current page, if possible
        /// </summary>
        /// <returns>The new page the wizard moved to</returns>
        /// <remarks>If moving next isn't valid at this point an exception will be thrown.</remarks>
        public IUIWizardPage MoveNext()
        {
            IUIWizardPage nextPage;

            //first, calculate the page we would normally go to.
            if (m_Engine.CurrentPageIndex < m_Engine.Pages.Count - 1)
            {
                nextPage = m_Engine.Pages[m_Engine.CurrentPageIndex + 1];

                //but, our override is that we skip the transport page if we can send via server.
                if ((nextPage is UIPackagerTransport) && (CanUseServer()))
                {
                    //skip the packager transport and move on...
                    m_Request.Transport = PackageTransport.Server;
                    nextPage = (m_Engine.CurrentPageIndex < m_Engine.Pages.Count - 2) ? m_Engine.Pages[m_Engine.CurrentPageIndex + 2] : (IUIWizardPage)m_Engine.FinishedPage;
                }
            }
            else
            {
                //no override once you get to the last page.
                nextPage = m_Engine.FinishedPage;
            }

            return nextPage;
        }

        /// <summary>
        /// Move to the previous page of the wizard from the current page, if possible
        /// </summary>
        /// <returns>The new page the wizard moved to</returns>
        /// <remarks>If moving previous isn't valid at this point an exception will be thrown.</remarks>
        public IUIWizardInputPage MovePrevious()
        {
            //we don't bother with any fun override here; you can't go back from the finished page so we'll let you progress through.
            if ((m_Engine.CurrentPageIndex > 0) && (m_Engine.CurrentPage is IUIWizardInputPage))
                return m_Engine.Pages[m_Engine.CurrentPageIndex - 1];

            return null;
        }

        #endregion

        #region Private Properties and Methods

        private bool CanUseServer()
        {
            bool canUseServer = m_Request.AllowServer;

            if (canUseServer)
            {
                //ok, we're allowed - but can we connect?
                string message;
                canUseServer = Packager.CanSendToServer(out message);
            }
            return canUseServer;
        }

        #endregion
    }
}

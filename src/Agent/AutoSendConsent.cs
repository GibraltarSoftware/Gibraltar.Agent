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
using System.Diagnostics;

#endregion

namespace Gibraltar.Agent
{
    /// <summary>
    /// The end-user consent for the local computer and a specific product or product and application pair
    /// </summary>
    [DebuggerDisplay("Opt In: {OptIn} Selection Made: {SelectionMade} Id: {Id}")]
    public class AutoSendConsent
    {
        private readonly Gibraltar.Data.AutoSendConsent m_WrappedObject;

        /// <summary>
        /// The log category for auto send consent data.
        /// </summary>
        public const string LogCategory = "Gibraltar.Agent.Consent";

        internal AutoSendConsent(Gibraltar.Data.AutoSendConsent wrappedObject)
        {
            m_WrappedObject = wrappedObject;
        }

        #region Public Properties and Methods

        /// <summary>
        /// The unique id of the underlying consent
        /// </summary>
        public Guid Id { get { return m_WrappedObject.Id; } }

        /// <summary>
        /// The product name that consent was recorded for
        /// </summary>
        public string ProductName { get { return m_WrappedObject.ProductName; } }

        /// <summary>
        /// Optional.  The application within the product that consent was recorded for.
        /// </summary>
        public string ApplicationName { get { return m_WrappedObject.ApplicationName; } }

        /// <summary>
        /// The version of the application when consent was last recorded
        /// </summary>
        public Version ApplicationVersion { get { return m_WrappedObject.ApplicationVersion; } }

        /// <summary>
        /// The number of times the user has been prompted to make a decision for this version.
        /// </summary>
        public int UserPrompts { get { return m_WrappedObject.UserPrompts; } }

        /// <summary>
        /// True if the user has made a selection (and the OptIn value is now valid) or false if no decision has been made yet.
        /// </summary>
        public bool SelectionMade { get { return m_WrappedObject.SelectionMade; } }

        /// <summary>
        /// True to opt into auto send, false to opt out.
        /// </summary>
        public bool OptIn { get { return m_WrappedObject.OptIn; } }

        /// <summary>
        /// True to include details in the auto send, false to only send summary, anonymous information
        /// </summary>
        public bool IncludeDetails { get { return m_WrappedObject.IncludeDetails; } }

        /// <summary>
        /// The date &amp; time the consent was last updated
        /// </summary>
        public DateTimeOffset UpdatedDt { get { return m_WrappedObject.UpdatedDt; } }

        /// <summary>
        /// The full user name that recorded the last update
        /// </summary>
        public string UpdatedUser { get { return m_WrappedObject.UpdatedUser; } }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return m_WrappedObject.ToString();
        }

        /// <summary>
        /// Indicates if the specified consent contains a selection or not.
        /// </summary>
        /// <param name="consent"></param>
        /// <returns></returns>
        public static bool IsNullOrNoSelection(AutoSendConsent consent)
        {
            return ((consent == null) || (consent.SelectionMade == false));
        }

        #endregion
    }
}

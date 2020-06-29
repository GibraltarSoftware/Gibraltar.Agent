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

using System.ComponentModel;

#endregion File Header

namespace Gibraltar
{
    /// <summary>
    /// A binding list that marshals all events to the provided invocation object (and thread)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SyncBindingList<T> : BindingList<T>
    {
        private readonly System.Action<ListChangedEventArgs> m_FireEventAction; //just here so we can get around declaring a delegate on each event

        private ISynchronizeInvoke m_SyncObject;

        /// <summary>
        /// Create a new binding list without specifying an external synchronizer
        /// </summary>
        public SyncBindingList()
            : this(null)
        {
        }

        /// <summary>
        /// Create a new binding list with the specified synchronizer, typically a form.
        /// </summary>
        /// <param name="syncObject"></param>
        public SyncBindingList(ISynchronizeInvoke syncObject)
        {
            m_SyncObject = syncObject;
            m_FireEventAction = FireEvent;
        }

        /// <summary>
        /// The synchronization object (if provided when the list was created)
        /// </summary>
        public ISynchronizeInvoke SyncObject { get { return m_SyncObject; } set { m_SyncObject = value; } }

        /// <summary>
        /// Raises the <see cref="E:System.ComponentModel.BindingList`1.ListChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.ListChangedEventArgs"/> that contains the event data. 
        ///                 </param>
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if ((m_SyncObject == null) || (m_SyncObject.InvokeRequired == false))
            {
                FireEvent(e);
            }
            else
            {
                m_SyncObject.BeginInvoke(m_FireEventAction, new object[] { e });
            }
        }

        private void FireEvent(ListChangedEventArgs args)
        {
            base.OnListChanged(args);
        }
    }
}

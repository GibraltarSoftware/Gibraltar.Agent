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
using System.Collections.Generic;

#endregion File Header

namespace Gibraltar
{
    /// <summary>
    /// A central arbitrator to ensure that only one object of a particular type is active at the same time.
    /// </summary>
    public static class SingletonArbitrator<T> where T : class 
    {
        private static readonly object m_Lock = new object();
        private static readonly List<T> m_RegisteredObjects = new List<T>();
        
        /// <summary>
        /// Raised by the central trace arbitrator to signal that the active listener has changed
        /// </summary>
        public static event EventHandler ActiveObjectChanged;

        #region Public Properties and Methods

        /// <summary>
        /// Register a new trace object with the arbitrator
        /// </summary>
        /// <param name="newObject"></param>
        /// <returns>True if this new object is the new active object</returns>
        public static bool Register(T newObject)
        {
            if (newObject == null)
                throw new ArgumentNullException(nameof(newObject));

            lock(m_Lock)
            {
                if (m_RegisteredObjects.Contains(newObject) == false)
                    m_RegisteredObjects.Add(newObject);

                if (m_RegisteredObjects.Count == 1)
                {
                    SetActiveObject(newObject);
                }

                //it's the active object if it's in the first slot.
                return (ReferenceEquals(ActiveObject, newObject));
            }
        }

        /// <summary>
        /// Unregister the specified object.  If it is the active object, the active object will change.
        /// </summary>
        /// <param name="closingObject"></param>
        public static void Unregister(T closingObject)
        {
            if (closingObject == null)
                throw new ArgumentNullException(nameof(closingObject));

            lock (m_Lock)
            {

                m_RegisteredObjects.Remove(closingObject);

                //If this is our current active object we'll need to change to the first item still in the list, if any.
                if (ReferenceEquals(ActiveObject, closingObject))
                {
                    T newActiveObject = null;
                    if (m_RegisteredObjects.Count > 0)
                        newActiveObject = m_RegisteredObjects[0];

                    SetActiveObject(newActiveObject);
                }
            }
        }

        /// <summary>
        /// The current active object, which may be null.
        /// </summary>
        public static T ActiveObject { get; private set; }

        #endregion

        #region Private Properties and Methods

        /// <summary>
        /// Set the new active object to a new value which may be null.
        /// </summary>
        /// <param name="newObject"></param>
        private static void SetActiveObject(T newObject)
        {
            if (ReferenceEquals(ActiveObject, newObject))
                return; //no change.

            ActiveObject = newObject;

            EventHandler tempEvent = ActiveObjectChanged;
            if (tempEvent != null)
            {
                tempEvent(newObject, EventArgs.Empty);
            }
        }

        #endregion
    }
}
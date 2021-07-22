
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
    /// Wrappers a list of items to allow them to be disposed as a unit.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DisposableList<T> : IDisposable where T : IDisposable
    {
        private List<T> m_Items = new List<T>();

        #region Public Properties and Methods

        /// <summary>
        /// The raw list of items.  If you remove an item from this list directly, you must dispose it.
        /// </summary>
        public List<T> List { get { return m_Items; } }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        /// <summary>
        /// Perform Dispose-related tasks.
        /// </summary>
        /// <param name="disposing"></param>
        /// <remarks>Note to inheritors:  If overriding this method you must call base to avoid resource leaks</remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (T item in m_Items)
                {
                    //do a best effort dispose.
                    try
                    {
                        item.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}

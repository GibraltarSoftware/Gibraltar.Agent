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
 *    Copyright © 2008-2010 by Gibraltar Software, Inc.  All rights reserved.
 *******************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Gibraltar.Agent.Data;

#endregion File Header

namespace Gibraltar.Agent.Internal
{
    /// <summary>
    /// A read-only collection of info describing one or more log messages.
    /// </summary>
    internal class LogMessageInfoCollection : ReadOnlyCollection<ILogMessage>, ILogMessageCollection
    {
        internal LogMessageInfoCollection(IList<ILogMessage> messages)
            : base(messages)
        {
        }

        /// <summary>
        /// Not a valid operation.  This is a read-only collection.
        /// </summary>
        [Obsolete("Not a valid operation.  This is a read-only collection.", true)]
        public void Clear()
        {
            ((IList<ILogMessage>)this).Clear();
        }

        /// <summary>
        /// Not a valid operation.  This is a read-only collection.
        /// </summary>
        [Obsolete("Not a valid operation.  This is a read-only collection.", true)]
        public void Add(ILogMessage item)
        {
            ((IList<ILogMessage>)this).Add(item);
        }

        /// <summary>
        /// Not a valid operation.  This is a read-only collection.
        /// </summary>
        [Obsolete("Not a valid operation.  This is a read-only collection.", true)]
        public void Remove(ILogMessage item)
        {
            ((IList<ILogMessage>)this).Remove(item);
        }

        /// <summary>
        /// Not a valid operation.  This is a read-only collection.
        /// </summary>
        [Obsolete("Not a valid operation.  This is a read-only collection.", true)]
        public void Insert(int index, ILogMessage item)
        {
            ((IList<ILogMessage>)this).Insert(index, item);
        }

        /// <summary>
        /// Not a valid operation.  This is a read-only collection.
        /// </summary>
        [Obsolete("Not a valid operation.  This is a read-only collection.", true)]
        public void RemoveAt(int index)
        {
            ((IList<ILogMessage>)this).RemoveAt(index);
        }
    }
}
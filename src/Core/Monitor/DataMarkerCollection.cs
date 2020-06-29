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

using System;
using System.Collections.Generic;

#endregion

#pragma warning disable 1591
namespace Gibraltar.Monitor
{
    [Serializable]
    public class DataMarkerCollection : Dictionary<Marker, DataMarker>, IEnumerable<DataMarker>
    {
        // Note: Dictionary is not internally threadsafe.  We need to be sure to handle this class properly if we use it.
        private readonly Analysis m_Analysis;

        public DataMarkerCollection(Analysis analysis)
        {
            m_Analysis = analysis;
        }

        #region IEnumerable<DataMarker> Members

        IEnumerator<DataMarker> IEnumerable<DataMarker>.GetEnumerator()
        {
            return base.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return base.Values.GetEnumerator();
        }

        #endregion
    }

}

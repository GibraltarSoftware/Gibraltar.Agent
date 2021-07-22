
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
using Gibraltar.Monitor.Internal;

#endregion File Header

#pragma warning disable 1591
namespace Gibraltar.Monitor
{
    /// <summary>
    /// Associates a marker to an extension object as part of an analysis.
    /// </summary>
    public class DataMarker : IDisplayable
    {
        private readonly Analysis m_Analysis;
        private readonly Marker m_Marker;

        public DataMarker(Analysis analysis, Marker Marker)
        {
            m_Analysis = analysis;
            
            //little trick- we want to guarantee that the marker is active & from the same analysis
            m_Marker = m_Analysis.Markers[Marker.Id];
        }

        #region Public Properties and Methods

        public Marker Marker
        {
            get { return m_Marker; }
        }

        public string Caption
        {
            get { return m_Marker.Caption; }
        }

        public string Description
        {
            get { return m_Marker.Description; }
        }

        #endregion

    }

}

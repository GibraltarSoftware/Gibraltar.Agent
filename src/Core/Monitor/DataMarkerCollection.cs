
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

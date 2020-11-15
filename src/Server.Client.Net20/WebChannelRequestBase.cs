

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// An abstract implementation of a web request that simplifies making new requests.
    /// </summary>
    public abstract class WebChannelRequestBase : IWebRequest
    {
        /// <summary>
        /// The standard content type for GZip'd data.
        /// </summary>
        public const string GZipContentType = "application/gzip";

        /// <summary>
        /// The standard content type for raw binary data.
        /// </summary>
        public const string BinaryContentType = "application/octet-stream";

        /// <summary>
        /// The standard content type for a zip file
        /// </summary>
        public const string ZipContentType = "application/zipfile";

        /// <summary>
        /// The standard content type for XML data
        /// </summary>
        protected const string XmlContentType = "text/xml";

        private readonly object m_StatusLock = new object();
        private readonly bool m_SupportsAuthentication;
        private readonly bool m_RequiresAuthentication;

        private bool m_Canceled;

        /// <summary>
        /// Raised when the request is canceled prior to successful execution completion
        /// </summary>
        public event WebRequestEventHandler Canceled;

        /// <summary>
        /// Raised whenever a request completes (even if it is canceled)
        /// </summary>
        public event WebRequestEventHandler Complete;

        /// <summary>
        /// Create a new web channel request
        /// </summary>
        /// <param name="supportsAuthentication"></param>
        /// <param name="requiresAuthentication"></param>
        protected WebChannelRequestBase(bool supportsAuthentication, bool requiresAuthentication)
        {
            m_SupportsAuthentication = supportsAuthentication;
            m_RequiresAuthentication = requiresAuthentication;
        }

        /// <summary>
        /// Indicates if the web request requires authentication (so the channel should authenticate before attempting the request)
        /// </summary>
        public bool RequiresAuthentication
        {
            get { return m_RequiresAuthentication; }
        }

        /// <summary>
        /// Indicates if the web request supports authentication, so if the server requests credentials the request can provide them.
        /// </summary>
        public bool SupportsAuthentication
        {
            get { return m_SupportsAuthentication; }
        }

        /// <summary>
        /// Perform the request against the specified web client connection.
        /// </summary>
        /// <param name="connection"></param>
        public void ProcessRequest(IWebChannelConnection connection)
        {
            OnProcessRequest(connection);
            OnComplete();
        }

        /// <summary>
        /// Called to cancel a request, even if its currently being attempted
        /// </summary>
        public void Cancel()
        {
            lock(m_StatusLock)
            {
                if (m_Canceled == false)
                {
                    m_Canceled = true;
                    OnCancel();
                }

                System.Threading.Monitor.PulseAll(m_StatusLock);
            }
        }

        #region Protected Properties and Methods

        /// <summary>
        /// Implemented by inheritors to perform the request on the provided web client.
        /// </summary>
        /// <param name="connection"></param>
        protected abstract void OnProcessRequest(IWebChannelConnection connection);

        /// <summary>
        /// Called to raise the Canceled event
        /// </summary>
        protected virtual void OnCancel()
        {
            WebRequestEventHandler tempEvent = Canceled;
            if (tempEvent != null)
            {
                tempEvent(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called to raise the Complete event.
        /// </summary>
        protected virtual void OnComplete()
        {
            WebRequestEventHandler tempEvent = Complete;
            if (tempEvent != null)
            {
                tempEvent(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Convert the provided XML fragment to a byte array of UTF8 data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlFragment"></param>
        /// <returns></returns>
        protected static byte[] ConvertXmlToByteArray<T>(T xmlFragment)
        {
            //we want to get a byte array
            using (MemoryStream outputStream = new MemoryStream(2048))
            {
                using (TextWriter textWriter = new StreamWriter(outputStream, Encoding.UTF8))
                {
                    XmlTextWriter xmlWriter = new XmlTextWriter(textWriter);
                    // Write XML using xmlWriter
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    XmlSerializerNamespaces xmlNsEmpty = new XmlSerializerNamespaces();
                    xmlSerializer.Serialize(xmlWriter, xmlFragment, xmlNsEmpty);
                    xmlWriter.Flush(); // to make sure it writes it all out now.
                }

                return outputStream.ToArray();
            }
        }

        #endregion
    }
}

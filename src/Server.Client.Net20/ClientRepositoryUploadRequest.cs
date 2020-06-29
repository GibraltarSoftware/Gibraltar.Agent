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
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Gibraltar.Server.Client.Data;

namespace Gibraltar.Server.Client
{
    /// <summary>
    /// Uploads the state of a client repository, adding it if necessary.
    /// </summary>
    public class ClientRepositoryUploadRequest : WebChannelRequestBase
    {
        /// <summary>
        /// Create a new sessions version request
        /// </summary>
        public ClientRepositoryUploadRequest(ClientRepositoryXml repositoryXml)
            : base(true, true)
        {
            InputRepository = repositoryXml;
        }

        #region Public Properties and Methods

        /// <summary>
        /// The repository data to commit to the server
        /// </summary>
        public ClientRepositoryXml InputRepository { get; private set; }

        /// <summary>
        /// The repository data returned by the server as a result of the request.
        /// </summary>
        public ClientRepositoryXml ResponseRepository { get; private set; }

        #endregion

        #region Protected Properties and Methods

        /// <summary>
        /// Implemented by inheritors to perform the request on the provided web client.
        /// </summary>
        /// <param name="connection"></param>
        protected override void OnProcessRequest(IWebChannelConnection connection)
        {
            byte[] requestedRepositoryRawData = connection.UploadData(GenerateResourceUri(), "PUT", XmlContentType, ConvertXmlToByteArray(InputRepository));

            //now we deserialize the response which is the new state of the document.

            //now, this is supposed to be a sessions list...
            using (MemoryStream inputStream = new MemoryStream(requestedRepositoryRawData))
            {
                XmlSerializerNamespaces xmlNsEmpty = new XmlSerializerNamespaces();
                xmlNsEmpty.Add("", "http://www.gibraltarsoftware.com/Gibraltar/Repository.xsd"); //gets rid of the default namespaces we'd otherwise generate

                XmlTextReader xmlReader = new XmlTextReader(inputStream);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ClientRepositoryXml), "http://www.gibraltarsoftware.com/Gibraltar/Repository.xsd");

                ClientRepositoryXml repositoryXml = (ClientRepositoryXml)xmlSerializer.Deserialize(xmlReader);
                ResponseRepository = repositoryXml;
            }

        }

        #endregion

        #region Private Properties and Methods

        private string GenerateResourceUri()
        {
            Guid repositoryId = new Guid(InputRepository.id); //to make sure we have a valid GUID
            return string.Format("/Repositories/{0}/Repository.xml", repositoryId);
        }

        #endregion
    }
}

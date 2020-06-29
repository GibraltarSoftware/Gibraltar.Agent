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
using System.Diagnostics;
using System.IO;
using Gibraltar.Messaging;
using Gibraltar.Monitor;
using Gibraltar.Server.Client;
using Gibraltar.Server.Client.Data;
using Gibraltar.Server.Data;
using Loupe.Extensibility.Data;

#endregion File Header

namespace Gibraltar.Data.Internal
{
    /// <summary>
    /// Publishes sessions from the specified repository to a remote destination repository.
    /// </summary>
    internal class RepositoryPublishClient: IDisposable
    {
        internal const string LogCategory = "Loupe.Repository.Publish";

        private readonly LocalRepository m_SourceRepository;
        private readonly string m_ProductName;
        private readonly string m_ApplicationName;
        private readonly HubConnection m_HubConnection;

        private volatile bool m_IsActive;
        private volatile bool m_Disposed;

        /// <summary>
        /// Create a new repository publish engine for the specified repository.
        /// </summary>
        /// <param name="source">The repository to publish</param>
        public RepositoryPublishClient(LocalRepository source)
            : this(source, null, null, Log.Configuration.Server)
        {
            //everything was handled in the constructor overload we passed off to
        }

        /// <summary>
        /// Create a new repository publish engine for the specified repository.
        /// </summary>
        /// <param name="source">The repository to publish</param>
        /// <param name="productName">Optional.  A product name to restrict operations to.</param>
        /// <param name="applicationName">Optional.  An application name within a product to restrict operations to.</param>
        /// <param name="configuration">The server connection information.</param>
        public RepositoryPublishClient(LocalRepository source, string productName, string applicationName, ServerConfiguration configuration)
            : this(source, configuration.ApplicationKey, configuration.UseGibraltarService, configuration.CustomerName, configuration.Server, 
                configuration.Port, configuration.UseSsl,
                configuration.ApplicationBaseDirectory, configuration.Repository)
        {
            m_ProductName = productName;
            m_ApplicationName = applicationName;
        }

        /// <summary>
        /// Create a new repository publish engine for the specified repository.
        /// </summary>
        /// <param name="source">The repository to publish</param>
        /// <param name="useGibraltarService">Indicates if the Gibraltar Loupe Service should be used instead of a private server</param>
        /// <param name="applicationKey">The application key to use to communicate with the Loupe Server</param>
        /// <param name="customerName">The unique customer name when using the Gibraltar Loupe Service</param>
        /// <param name="server">The full DNS name of the server where the service is located.  Only applies to a private server.</param>
        /// <param name="port"> An optional port number override for the server.  Only applies to a private server.</param>
        /// <param name="useSsl">Indicates if the connection should be encrypted with Ssl.  Only applies to a private server.</param>
        /// <param name="applicationBaseDirectory">The virtual directory on the host for the private service.  Only applies to a private server.</param>
        /// <param name="repository">The specific repository on the server for a private server.  Only applies to a private server.</param>
        public RepositoryPublishClient(LocalRepository source, string applicationKey, bool useGibraltarService, string customerName, 
            string server, int port, bool useSsl, string applicationBaseDirectory, string repository)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            m_SourceRepository = source;
            m_HubConnection = new HubConnection(applicationKey, useGibraltarService, customerName, server, port, useSsl, applicationBaseDirectory, repository);
        }

        #region Public Properties and Methods

        /// <summary>
        /// The repository this publish engine is associated with.
        /// </summary>
        public LocalRepository Repository { get { return m_SourceRepository; } }

        /// <summary>
        /// Indicates if this is the active repository publish engine for the specified repository.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return m_IsActive;
            }
        }

        /// <summary>
        /// Attempts to connect to the server and returns information about the connection status.
        /// </summary>
        /// <param name="status">The server status of the final server connected to.</param>
        /// <param name="statusMessage">An end-user display message providing feedback on why a connection is not available</param>
        /// <returns>True if the configuration is valid and the server is available, false otherwise.</returns>
        public bool CanConnect(out HubStatus status, out string statusMessage)
        {
            return m_HubConnection.CanConnect(out status, out statusMessage);
        }

        /// <summary>
        /// Publish qualifying local sessions and upload any details requested by the server
        /// </summary>
        /// <param name="async"></param>
        /// <param name="purgeSentSessions">Indicates if the session should be purged from the repository once it has been sent successfully.</param>
        public void PublishSessions(bool async, bool purgeSentSessions)
        {
            if (m_IsActive)
                return; //we're already publishing, we can't queue more.

            //we do the check for new sessions on the foreground thread since it won't block.
            var sessions = GetSessions();

            if ((sessions != null) && (sessions.Count > 0))
            {
                //go ahead and use the threadpool to publish the sessions.
                m_IsActive = true; //this gets set to false by the publish sessions routine when it's done.

                object[] state = new object[] { sessions, -1, purgeSentSessions };//retry until successful (-1)
                if (async)
                {
                    System.Threading.ThreadPool.QueueUserWorkItem(AsyncPublishSessions, state); 
                }
                else
                {
                    AsyncPublishSessions(state);
                }
            }
        }

        /// <summary>
        /// Send the specified session with details, even if other publishers are running.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="maxRetries"></param>
        /// <param name="purgeSentSession">Indicates if the session should be purged from the repository once it has been sent successfully.</param>
        /// <remarks>Throws an exception if it fails</remarks>
        public void UploadSession(Guid sessionId, int maxRetries, bool purgeSentSession)
        {
            PerformSessionDataUpload(sessionId, maxRetries, purgeSentSession);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Properties and Methods

        private void Dispose(bool releaseManaged)
        {
            if (releaseManaged && !m_Disposed)
            {
                if (m_HubConnection != null)
                {
                    m_HubConnection.Dispose();
                }

                m_Disposed = true;
            }
        }

        /// <summary>
        /// Publish the latest session data and find out what sessions should be uploaded.
        /// </summary>
        private void AsyncPublishSessions(object state)
        {
            try
            {
                object[] arguments = (object[])state;

                IList<ISessionSummary> sessions = (IList<ISessionSummary>)arguments[0];
                int maxRetries = (int)arguments[1];
                bool purgeSentSessions = (bool)arguments[2];

                if (sessions == null)
                    return;

                //lets make sure the server is connectible.
                HubStatus status;
                string statusMessage;
                if ((m_HubConnection.CanConnect(out status, out statusMessage) == false) && (maxRetries >=0))
                {
                    //we are stopping right here because the server isn't there, so no point in trying anything else.
#if DEBUG
                    Log.Write(LogMessageSeverity.Warning, LogCategory, "Unable to publish sessions because the server is not available", "While verifying that the server is available the following status information was returned:\r\nStatus: {0}\r\nMessage: {1}", status, statusMessage);
#endif
                }
                else
                {
                    //OK, now we've released the session information from RAM (all we wanted were the GUID's anyway)
                    //and we can send these one by one as long as the connection is up.
                    foreach (ISessionSummary session in sessions)
                    {
                        //try to upload it.
                        try
                        {
                            PerformSessionHeaderUpload(session);
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex);
#if DEBUG
                            Log.RecordException(0, ex, null, LogCategory, true);
#endif
                        }
                    }

                    //now find out what sessions they want us to upload
                    List<Guid> requestedSessions = GetRequestedSessions();

                    foreach (Guid sessionId in requestedSessions)
                    {
                        //we want to try each, even if they fail.
                        try
                        {
                            PerformSessionDataUpload(sessionId, maxRetries, purgeSentSessions);
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex);
#if DEBUG
                            Log.RecordException(0, ex, null, LogCategory, true);
#endif
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Log.RecordException(0, ex, null, LogCategory, true);
#endif
            }
            finally
            {
                m_IsActive = false; //so others can go now.
            }
        }

        /// <summary>
        /// Find out what sessions the server wants details for.
        /// </summary>
        private List<Guid> GetRequestedSessions()
        {
            RequestedSessionsGetRequest request = new RequestedSessionsGetRequest(m_SourceRepository.Id);

            try
            {
                m_HubConnection.ExecuteRequest(request, 1);
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Log.RecordException(0, ex, null, LogCategory, true);
#endif
            }

            List<Guid> requestedSessions = new List<Guid>();

            if ((request.RequestedSessions != null) 
                && (request.RequestedSessions.sessions != null)
                && (request.RequestedSessions.sessions.Length > 0))
            {
                foreach (SessionXml requestedSession in request.RequestedSessions.sessions)
                {
                    //we want to either queue the session to be sent (if not queued already) or
                    //mark the session as complete on the server is no data is available.
                    try
                    {
                        Guid sessionId = new Guid(requestedSession.id);
                        if (m_SourceRepository.SessionDataExists(sessionId))
                        {
                            //queue for transmission
                            requestedSessions.Add(sessionId);
                        }
                        else
                        {
                            if (m_HubConnection.ProtocolVersion < HubConnection.Hub30ProtocolVersion)
                            {
                                if (!Log.SilentMode)
                                    Log.Write(LogMessageSeverity.Information, LogCategory, "Server requesting completed session that's no longer available", "There's no way for us to tell the server that it should stop asking for this session.\r\nSession Id: {0}", sessionId);
                            }
                            else
                            {
                                //it's complete, there's nothing more we can give them.
                                //KM: We can't assume there is no data - it could be in another local repository, so we shouldn't do this.
                                //PerformSessionMarkComplete(sessionId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GC.KeepAlive(ex);
#if DEBUG
                        Log.RecordException(0, ex, null, LogCategory, true);
#endif
                    }
                }
            }

            return requestedSessions;
        }

        /// <summary>
        /// Find the list of all sessions that haven't been published yet and match our filter
        /// </summary>
        /// <returns></returns>
        private IList<ISessionSummary> GetSessions()
        {
            //find the list of all sessions that haven't been published yet and match our filter
            IList<ISessionSummary> sessions = null;

            try
            {
                m_SourceRepository.Refresh(); //we want a picture of the latest data as of the start of this process.

                sessions = m_SourceRepository.Find(UnsentSessionsPredicate);
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Log.RecordException(0, ex, null, LogCategory, true);
#endif
            }

            return sessions;
        }

        /// <summary>
        /// A predicate filter for the repository to identify unsent, qualifying sessions
        /// </summary>
        /// <param name="candidateSession"></param>
        /// <returns></returns>
        private bool UnsentSessionsPredicate(ISessionSummary candidateSession)
        {
            bool matchesPredicate = candidateSession.IsNew;

            if (matchesPredicate)
                matchesPredicate = candidateSession.Product.Equals(m_ProductName, StringComparison.OrdinalIgnoreCase);

            if (matchesPredicate && !string.IsNullOrEmpty(m_ApplicationName))
                matchesPredicate = candidateSession.Application.Equals(m_ApplicationName, StringComparison.OrdinalIgnoreCase);

            return matchesPredicate;
        }

        /// <summary>
        /// Sends a session, either as a single stream or a set of fragments, to the server.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="maxRetries">The maximum number of times to retry the session data upload.</param>
        /// <param name="purgeSentSessions">Indicates whether to purge sessions that have been successfully sent from the repository</param>
        /// <returns>Throws an exception if the upload fails.</returns>
        private void PerformSessionDataUpload(Guid sessionId, int maxRetries, bool purgeSentSessions)
        {
            m_SourceRepository.Refresh(); //we want a picture of the latest data as of the start of this process.

            //this can get a little complicated:  Do we use the new fragment-based protocol or the old single session mode?
            bool useSingleStreamMode;
            Dictionary<Guid, SessionFileXml> serverFiles = new Dictionary<Guid, SessionFileXml>();
            if (m_HubConnection.ProtocolVersion < HubConnection.Hub30ProtocolVersion)
            {
                //use the legacy single stream mode
                useSingleStreamMode = true;
            }
            else
            {
                //see if we can use the more efficient mode, and if so what files we should send.
                var sessionFilesRequest = new SessionFilesGetRequest(m_SourceRepository.Id, sessionId);
                m_HubConnection.ExecuteRequest(sessionFilesRequest, maxRetries);

                useSingleStreamMode = sessionFilesRequest.Files.singleStreamOnly;

                if ((!useSingleStreamMode) && (sessionFilesRequest.Files.files != null))
                {
                    //since individual files are immutable we don't need to upload any file the server already has.
                    foreach (var sessionFileXml in sessionFilesRequest.Files.files)
                    {
                        serverFiles.Add(new Guid(sessionFileXml.id), sessionFileXml);
                    }
                }
            }

            if (useSingleStreamMode)
            {
                PerformSessionFileUpload(sessionId, null, maxRetries, purgeSentSessions);
            }
            else
            {
                //it's a bit more complicated:  We need to update each file they don't have.  
                SessionHeader sessionHeader;
                IList<FileInfo> sessionFragments;
                m_SourceRepository.LoadSessionFiles(sessionId, out sessionHeader, out sessionFragments);

                //the session files may no longer be present by now
                if (sessionFragments != null)
                {
                    foreach (var sessionFragment in sessionFragments)
                    {
                        //if they already have this one, skip it.
                        var fileHeader = LocalRepository.LoadSessionHeader(sessionFragment.FullName);
                        if (fileHeader == null)
                            break; //the file must be gone, it certainly isn't valid.

                        if (serverFiles.ContainsKey(fileHeader.FileId))
                        {
                            //skip this file.  If we're supposed to be purging sent data then drop this fragment.
                            if (purgeSentSessions)
                                m_SourceRepository.Remove(sessionId, fileHeader.FileId);
                        }
                        else
                        {
                            //ohhkay, lets upload this bad boy.
                            PerformSessionFileUpload(sessionId, fileHeader.FileId, maxRetries, purgeSentSessions);
                        }
                    }

                    if (!m_SourceRepository.SessionIsRunning(sessionId))
                        //finally, mark this session as complete.  We've sent all the data we have.
                        PerformSessionMarkComplete(sessionId);
                }
            }
        }

        /// <summary>
        /// Sends a merged session stream or a single session fragment file to the server.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="fileId"></param>
        /// <param name="maxRetries">The maximum number of times to retry the session data upload.</param>
        /// <param name="purgeSentSessions">Indicates whether to purge sessions that have been successfully sent from the repository</param>
        /// <returns>Throws an exception if the upload fails.</returns>
        private void PerformSessionFileUpload(Guid sessionId, Guid? fileId, int maxRetries, bool purgeSentSessions)
        {
            using (var request = new SessionUploadRequest(m_SourceRepository.Id, m_SourceRepository, sessionId, fileId, purgeSentSessions))
            {
                //because upload request uses a multiprocess lock we put it in a using to ensure it gets disposed.
                //explicitly prepare the session - this returns true if we got the lock meaning no one else is actively transferring this session right now.
                if (request.PrepareSession() == false)
                {
                    if (!Log.SilentMode)
                        Log.Write(LogMessageSeverity.Information, LogCategory, "Skipping sending session to server because another process already is transferring it", "We weren't able to get a transport lock on the session '{0}' so we assume another process is currently sending it.", sessionId);
                }
                else
                {
                    m_HubConnection.ExecuteRequest(request, maxRetries);
                }
            }
            
        }

        /// <summary>
        /// Upload the session summary for one session.
        /// </summary>
        /// <param name="sessionSummary"></param>
        private void PerformSessionHeaderUpload(ISessionSummary sessionSummary)
        {
            SessionXml sessionSummaryXml = DataConverter.ToSessionXml(sessionSummary); 
            PerformSessionHeaderUpload(sessionSummaryXml);
        }

        /// <summary>
        /// Upload the session summary for one session.
        /// </summary>
        /// <param name="sessionSummary"></param>
        private void PerformSessionHeaderUpload(SessionXml sessionSummary)
        {
            Guid sessionId = new Guid(sessionSummary.id);

            Debug.Assert(!String.IsNullOrEmpty(sessionSummary.sessionDetail.productName));
            Debug.Assert(!String.IsNullOrEmpty(sessionSummary.sessionDetail.applicationName));
            Debug.Assert(!String.IsNullOrEmpty(sessionSummary.sessionDetail.applicationVersion));

            //we consider a session complete (since we're the source repository) with just the header if there
            //is no session file.
            sessionSummary.sessionDetail.isComplete = !m_SourceRepository.SessionDataExists(sessionId);

            SessionHeaderUploadRequest uploadRequest = new SessionHeaderUploadRequest(sessionSummary, m_SourceRepository.Id);

            //get our web channel to upload this request for us.
            m_HubConnection.ExecuteRequest(uploadRequest, -1);

            //and if we were successful (must have been - we got to here) then mark the session as not being new any more.
            m_SourceRepository.SetSessionsNew(new[] { sessionId }, false);
        }

        /// <summary>
        /// Mark the specified session as being complete.
        /// </summary>
        private void PerformSessionMarkComplete(Guid sessionId)
        {
            var uploadRequest = new SessionMarkComplete(sessionId, m_SourceRepository.Id);

            //get our web channel to upload this request for us.
            m_HubConnection.ExecuteRequest(uploadRequest, -1);
        }

        #endregion
    }
}

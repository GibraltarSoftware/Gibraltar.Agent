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
//  *    Copyright © 2008 - 2019 by Gibraltar Software, Inc.  
//  *    All rights reserved.
//  *******************************************************************/
#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using Gibraltar.Monitor;
using Ionic.Zip;
using Ionic.Zlib;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Data
{
    /// <summary>
    /// A very simple implementation of the Package type for use within the agent
    /// </summary>
    /// <remarks>Unlike the full package implementation this form has no index and does not merge session fragments.</remarks>
    public class SimplePackage: IDisposable
    {
        private const string LogCategory = "Loupe.Repository.Package";
        private const string FragmentsFolder = "SessionFragments";

        private readonly object m_Lock = new object();
        private readonly Dictionary<Guid, SessionFileInfo<ZipEntry>> m_Sessions = new Dictionary<Guid, SessionFileInfo<ZipEntry>>();

        private ZipFile m_Archive;
        private string m_FileNamePath;
        private string m_TemporaryWorkingDirectory; //the working directory we were using that needs to be cleaned up when we die.
        volatile private bool m_Disposed;

        private string m_Caption;
        private string m_Description;
        private bool m_IsDirty;


        /// <summary>
        /// Create a new, empty package.
        /// </summary>
        public SimplePackage()
        {
            //we are a new package, don't know what we are yet
            Caption = "New Package";

            OnInitialize();
        }

        /// <summary>
        /// Create a new package by loading the specified file.  It must be a package file, not a log file.
        /// </summary>
        /// <remarks>To load any single GLF or GLP file into a new Package object see the static method LoadFileAsPackage().</remarks>
        /// <param name="fileNamePath">The full path to the package file to open.</param>
        public SimplePackage(string fileNamePath)
        {
            if (string.IsNullOrEmpty(fileNamePath))
            {
                throw new ArgumentNullException(nameof(fileNamePath), "Unable to create package from file because no file was specified.");
            }

            if (File.Exists(fileNamePath) == false)
            {
                throw new FileNotFoundException("No existing package could be found with the provided file name", fileNamePath);
            }

            //open the zip archive - if the file doesn't exist we want to fail now.
            m_Archive = new ZipFile(fileNamePath);
            m_Archive.ParallelDeflateThreshold = -1; //disable due to corruption defect in feature

            m_FileNamePath = fileNamePath;

            Caption = Path.GetFileNameWithoutExtension(m_FileNamePath);

            //and we start out not dirty (setting properties can make us dirty).
            IsDirty = false;

            OnInitialize();
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

        /// <summary>
        /// Adds the provided session to the package
        /// </summary>
        /// <param name="sessionStream"></param>
        public void AddSession(Stream sessionStream)
        {           
            using (GLFReader glfReader = new GLFReader(sessionStream)) // This will dispose the stream when it is disposed.
            {
                if (!glfReader.IsSessionStream)
                    throw new GibraltarException("The data stream provided is not a valid session data stream.");

                if (!Log.SilentMode)
                    Log.Write(LogMessageSeverity.Verbose, LogCategory, "Stream is session file, attempting to load", null);

                lock (m_Lock)
                {
                    //copy this stream into our temp space so we don't require the caller to leave it open through now until save.
                    string fileName = glfReader.SessionHeader.HasFileInfo ? string.Format("{0}~{1}.{2}", glfReader.SessionHeader.Id, glfReader.SessionHeader.FileId, Log.LogExtension)
                                          : string.Format("{0}.{1}", glfReader.SessionHeader.Id, Log.LogExtension);

                    string tempFileNamePath = Path.Combine(m_TemporaryWorkingDirectory, fileName);
                    using (FileStream newStream = new FileStream(tempFileNamePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                    {
                        FileSystemTools.StreamContentCopy(sessionStream, newStream, true); // Copy the stream into our package's temp directory.
                    }

                    //now add this to the zip file.
                    string zipFilePath = GenerateFragmentPath(glfReader.SessionHeader.FileId);
                    var fragmentEntry = m_Archive[zipFilePath];

                    if (fragmentEntry != null)
                    {
                        m_Archive.RemoveEntry(zipFilePath); //wipe out any existing entry.                              
                    }

                    fragmentEntry = m_Archive.AddFile(tempFileNamePath, FragmentsFolder); //relies on the original file having the right name
                    fragmentEntry.CompressionLevel = CompressionLevel.None; //session files are already highly compressed, no reason to waste effort.

                    //and pop it in our index.
                    AddSessionHeaderToIndex(glfReader.SessionHeader, fragmentEntry);

                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Save the package, overwriting any existing data
        /// </summary>
        /// <param name="progressMonitors"></param>
        public void Save(ProgressMonitorStack progressMonitors)
        {
            if (string.IsNullOrEmpty(m_FileNamePath))
            {
                throw new FileNotFoundException("Unable to save the current package because no path has been set to save to.");
            }

            Save(progressMonitors, m_FileNamePath);            
        }

        /// <summary>
        /// Save the package to the specified file name and path, overwriting any existing data
        /// </summary>
        /// <param name="progressMonitors"></param>
        /// <param name="fileNamePath"></param>
        public void Save(ProgressMonitorStack progressMonitors, string fileNamePath)
        {
            if (string.IsNullOrEmpty(fileNamePath))
            {
                throw new ArgumentNullException(nameof(fileNamePath), "Unable to save the current package because no path has been set to save to.");
            }

            //make sure the path exists so we can save into it.
            FileSystemTools.EnsurePathExists(fileNamePath);

            lock (m_Lock)
            {
                int steps = 3;
                int completedSteps = 0;
                using (ProgressMonitor ourMonitor = progressMonitors.NewMonitor(this, "Saving Package", steps))
                {
                    //Do the save (if this fails, we failed)
                    ourMonitor.Update("Saving Package File to Disk", completedSteps++);

                    m_Archive.Comment = m_Description;
                    m_Archive.Save(fileNamePath); // This closes the archive's file stream and locks out further reads...
                    m_Archive.Dispose(); // ...So we need to dispose and reopen the archive.
                    m_Archive = new ZipFile(fileNamePath); // Now we should again be able to read any entry.
                    m_Archive.ParallelDeflateThreshold = -1; //disable due to corruption defect in feature

                    m_FileNamePath = fileNamePath;

                    ourMonitor.Update("Confirming package contents", completedSteps++);
                    LoadIndex();

                    //Since we were successful at saving, this is our new path and we are no longer dirty.
                    m_FileNamePath = fileNamePath;
                    IsDirty = false;

                    //now we can clear our temp folder cache
                    ourMonitor.Update("Removing temporary storage", completedSteps++);
                    foreach (var fileName in Directory.GetFiles(m_TemporaryWorkingDirectory))
                    {
                        try
                        {
                            Directory.Delete(fileName);
                        }
                        catch (Exception ex)
                        {
                            if (!Log.SilentMode)
                            {
                                string description;
                                object[] args;
                                if (Log.SessionSummary.PrivacyEnabled)
                                {
                                    // Anonymous mode is enabled, just output a simple message with no file paths.
                                    description = "While removing the temporary file from the temp file cache an exception was thrown. This file may be leaked and use up disk space after the package process completes.";
                                    args = null;
                                }
                                else
                                {
                                    description = "While removing the temporary file '{0}' from the temp file cache an exception was thrown. This file may be leaked and use up disk space after the package process completes.\r\nException: {1}";
                                    args = new object[]
                                    {
                                        fileName,
                                        ex.Message
                                    };
                                }

                                Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, LogCategory, "Unable to clean up temporary file from cache", description, args);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Write the entire package to the provided stream (which must be seekable)
        /// </summary>
        /// <param name="stream"></param>
        public void Write(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            lock(m_Lock)
            {
                m_Archive.Comment = m_Description;
                m_Archive.Save(stream);
            }
        }

        /// <summary>
        /// The display caption for the package
        /// </summary>
        public string Caption
        {
            get { return m_Caption; }
            set { m_Caption = value; }
        }

        /// <summary>
        /// The end user display description for the package
        /// </summary>
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        /// <summary>
        /// Indicates if there is unsaved data in the repository.
        /// </summary>
        public bool IsDirty { get { return m_IsDirty; } private set { m_IsDirty = value; } }

        /// <summary>
        /// The current full path to the package.  It may be null or empty if this package has never been saved.
        /// </summary>
        public string FileNamePath { get { return m_FileNamePath; } }

        /// <summary>
        /// Clear all of the sessions from the package
        /// </summary>
        public void Clear()
        {
            lock (m_Lock)
            {
                m_Sessions.Clear();

                //now wipe them out the fragments
                m_Archive.RemoveSelectedEntries("*." + Log.LogExtension, FragmentsFolder);
            }
        }

        /// <summary>
        /// Get summary statistics about the sessions in the repository
        /// </summary>
        /// <param name="sessions">The number of sessions in the entire repository</param>
        /// <param name="files">The number of files for all of the sessions in the repository</param>
        /// <param name="fileBytes">The total number of bytes used by files in the repository</param>
        /// <param name="problemSessions">The number of sessions with problems in the folder and all folders it contains</param>
        /// <remarks>Problems are crashed sessions or sessions with critical or error messages.</remarks>
        public void GetStats(out int sessions, out int problemSessions, out int files, out long fileBytes)
        {
            sessions = m_Sessions.Count;
            problemSessions = 0;
            fileBytes = 0;
            files = 0;

            lock (m_Lock)
            {
                foreach (var session in m_Sessions.Values)
                {
                    if ((session.Header.ErrorCount > 0) || (session.Header.CriticalCount > 0) || (session.Header.StatusName.Equals(SessionStatus.Crashed.ToString(), StringComparison.OrdinalIgnoreCase)))
                    {
                        problemSessions++;
                    }

                    files += session.Fragments.Count;
                    foreach (var fragment in session.Fragments)
                    {
                        fileBytes += fragment.UncompressedSize;
                    }
                }
            }
        }

        /// <summary>
        /// Get the set of all of the session headers in the package
        /// </summary>
        /// <returns></returns>
        public IList<SessionHeader> GetSessions()
        {
            lock (m_Lock)
            {                
                var sessionList = new List<SessionHeader>(m_Sessions.Count);

                foreach (var session in m_Sessions.Values)
                {
                    sessionList.Add(session.Header);
                }

                return sessionList;
            }
        }

        /// <summary>
        /// Load the specified session
        /// </summary>
        /// <returns>The loaded session.  If no session can be found with the specified Id an ArgumentOutOfRangeException will be thrown.</returns>
        public Session GetSession(Guid sessionId, Guid? fileId = null)
        {
            Session requestedSession = null;
            lock (m_Lock)
            {
                SessionFileInfo<ZipEntry> sessionFileInfo;
                if (m_Sessions.TryGetValue(sessionId, out sessionFileInfo) == false)
                {
                    throw new ArgumentOutOfRangeException(nameof(sessionId), "There is no session in the package with the provided id");
                }

                //now load up all of the session fragments.
                var loadingCollection = new SessionCollection();

                foreach (var fragment in sessionFileInfo.Fragments)
                {
                    //we need a seek-able stream - so we'll extract the fragment into a temp file and go with that.
                    if (fileId != null)
                    {
                        //we need to check the file Id to see if it's what they requested
                        using (var reader = new GLFReader(FileSystemTools.GetTempFileStreamCopy(fragment.OpenReader())))
                        {
                            if (reader.SessionHeader.FileId != fileId.Value)
                                continue;
                        }
                    }

                    requestedSession = loadingCollection.Add(FileSystemTools.GetTempFileStreamCopy(fragment.OpenReader()), true);
                }
            }

            if (requestedSession == null)
                throw new ArgumentOutOfRangeException(nameof(fileId), "There is no session file in the package with the provided file id");

            return requestedSession;
        }

        /// <summary>
        /// Retrieve the ids of the sessions files known locally for the specified session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public IList<Guid> GetSessionFileIds(Guid sessionId)
        {
            lock(m_Lock)
            {
                SessionFileInfo<ZipEntry> sessionFileInfo;
                if (m_Sessions.TryGetValue(sessionId, out sessionFileInfo) == false)
                {
                    throw new ArgumentOutOfRangeException(nameof(sessionId), "There is no session in the package with the provided id");
                }

                var fileIds = new List<Guid>();
                foreach (var fragment in sessionFileInfo.Fragments)
                {
                    //this is kinda crappy - we have to make a copy of the whole fragment to get a seekable stream to read the id.
                    using (var reader = new GLFReader(FileSystemTools.GetTempFileStreamCopy(fragment.OpenReader())))
                    {
                        fileIds.Add(reader.SessionHeader.FileId);
                    }
                }

                return fileIds;
            }
        }

        #region Protected Properties and Methods

        /// <summary>
        /// Dispose managed and unmanaged resources
        /// </summary>
        /// <param name="releaseManaged">Indicates if its safe to release all of the managed resources</param>
        protected virtual void Dispose(bool releaseManaged)
        {
            if (m_Disposed == false)
            {
                m_Disposed = true;

                if (releaseManaged)
                {
                    //and we have to dispose of our zip file so we can be sure we release the lock on it immediately.
                    if (m_Archive != null)
                    {
                        m_Archive.Dispose();
                        m_Archive = null;
                    }

                    //and delete our temporary directory
                    if (string.IsNullOrEmpty(m_TemporaryWorkingDirectory) == false)
                    {
                        if (Directory.Exists(m_TemporaryWorkingDirectory))
                        {
                            try
                            {
                                Directory.Delete(m_TemporaryWorkingDirectory, true);
                            }
                            catch (Exception ex)
                            {
                                if (!Log.SilentMode) Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, LogCategory,
                                          "Unable to Delete Package Temporary Directory due to " + ex.GetType(),
                                          "Unable to delete the temporary working directory '{0}'. This means we'll use more disk space than we should but otherwise should not impact the application.",
                                          m_TemporaryWorkingDirectory);
                            }
                        }
                        else
                        {
                            if (!Log.SilentMode) Log.Write(LogMessageSeverity.Warning, LogCategory, "Package Temporary Directory already deleted",
                                      "The temporary working directory '{0}' does not exist when trying to dispose of it.  This is unexpected, but not generally a problem unless the detection is in error.",
                                      m_TemporaryWorkingDirectory);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called to initialize the package on creation
        /// </summary>
        protected virtual void OnInitialize()
        {
            lock (m_Lock)
            {
                //assigning a temporary directory we'll extract everything into
                m_TemporaryWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                //and create it now, just so we're in an easy known state if we need to extract things.
                Directory.CreateDirectory(m_TemporaryWorkingDirectory);

                //is this a NEW package or an EXISTING package?
                if (m_Archive == null)
                {
                    if (!Log.SilentMode)
                        Log.Write(LogMessageSeverity.Information, LogCategory, "Creating new package.", null);

                    m_Archive = new ZipFile();
                }
                else
                {
                    m_Description = m_Archive.Comment;

                    LoadIndex();
                }
            }
        }

        private void AddSessionHeaderToIndex(SessionHeader sessionHeader, ZipEntry sessionFragment)
        {
            lock (m_Lock)
            {
                SessionFileInfo<ZipEntry> sessionFileInfo;
                if (m_Sessions.TryGetValue(sessionHeader.Id, out sessionFileInfo))
                {
                    //add this file fragment to the existing session info
                    sessionFileInfo.AddFragment(sessionHeader, sessionFragment, true);
                }
                else
                {
                    //create a new session file info - this is the first we've seen this session.
                    sessionFileInfo = new SessionFileInfo<ZipEntry>(sessionHeader, sessionFragment, true);
                    m_Sessions.Add(sessionFileInfo.Id, sessionFileInfo);
                }
            }
        }

        /// <summary>
        /// Load the index from the current archive by reading all of the file fragments.
        /// </summary>
        private void LoadIndex()
        {
            lock (m_Lock)
            {
                if (!Log.SilentMode)
                    Log.Write(LogMessageSeverity.Information, LogCategory, "Loading package file", "File name: {0}", m_FileNamePath);

                m_Sessions.Clear();

                //we need to load up an index of these items
                foreach (var zipEntry in m_Archive)
                {
                    //we're looking for a specific pattern of names
                    if ((!zipEntry.IsDirectory) && 
                        ((zipEntry.FileName.StartsWith(FragmentsFolder, StringComparison.OrdinalIgnoreCase)) //Where a simple package stores its fragments
                        || (zipEntry.FileName.EndsWith(Log.PackageExtension, StringComparison.OrdinalIgnoreCase)))) //A normal package with a damaged index
                    {
                        try
                        {
                            var sessionHeader = LoadSessionHeader(zipEntry);

                            if (sessionHeader != null)
                            {
                                AddSessionHeaderToIndex(sessionHeader, zipEntry);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!Log.SilentMode)
                                Log.Write(LogMessageSeverity.Error, LogWriteMode.Queued, ex, true, LogCategory, 
                                    "Unable to parse file fragment name in simple package due to " + ex.GetType(), "File Name: {0}\r\nException: {1}", zipEntry.FileName, ex.Message);
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Properties and Methods

        private static string GenerateFragmentPath(Guid fileId)
        {
            return string.Format("{0}/{1}.{2}", FragmentsFolder, fileId, Log.LogExtension);
        }

        /// <summary>
        /// Attempt to load the session header from the specified entry, returning null if it can't be loaded
        /// </summary>
        /// <param name="zipEntry">The current entry</param>
        /// <returns>The session header, or null if it can't be loaded</returns>
        private static SessionHeader LoadSessionHeader(ZipEntry zipEntry)
        {
            if (!Log.SilentMode)
                Log.Write(LogMessageSeverity.Verbose, LogCategory, "Opening session file from zip file to read header", "Opening zip entry '{0}'.", zipEntry.FileName);

            SessionHeader header = null;

            try
            {
                using (var sourceFile = zipEntry.OpenReader())
                {
                    //we need a SEEKABLE stream, so we need to make a memory stream to be seek-able.


                    //read the file header first to find out how large the session header is (which can be any size)
                    var fileHeaderBuffer = new byte[FileHeader.HeaderSize];
                    sourceFile.Read(fileHeaderBuffer, 0, fileHeaderBuffer.Length);
                    FileHeader fileHeader = null;
                    using (var memoryStream = new MemoryStream(fileHeaderBuffer))
                    {
                        if (GLFReader.IsGLF(memoryStream, out fileHeader) == false)
                        {
                            if (!Log.SilentMode)
                                Log.Write(LogMessageSeverity.Verbose, LogCategory, "Session file does not check out as a GLF file, skipping", "Zip entry: '{0}'.", zipEntry.FileName);
                            return header;
                        }
                    }

                    var sessionHeaderBuffer = new byte[fileHeader.DataOffset];
                    //we have to copy the file header into it because we can't re-read those bytes (non-seek-able stream)
                    fileHeaderBuffer.CopyTo(sessionHeaderBuffer, 0);
                    sourceFile.Read(sessionHeaderBuffer, fileHeaderBuffer.Length, sessionHeaderBuffer.Length - fileHeaderBuffer.Length);
                    using (var memoryStream = new MemoryStream(sessionHeaderBuffer))
                    {
                        using (var sourceGlfFile = new GLFReader(memoryStream))
                        {
                            if (sourceGlfFile.IsSessionStream)
                                header = sourceGlfFile.SessionHeader;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!Log.SilentMode)
                    Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, LogCategory, "Unexpected exception while attempting to load a session header",
                        "While opening the zip file entry '{0}' an exception was thrown reading the session header. Since this routine is designed to not generate exceptions this may indicate a flaw in the logic of the routine.\r\nException: {1}\r\n",
                        zipEntry.FileName, ex.Message);
            }

            return header;
        }

        #endregion
    }
}

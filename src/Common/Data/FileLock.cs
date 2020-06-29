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
using System.IO;
using Microsoft.Win32.SafeHandles;

#endregion File Header

namespace Gibraltar.Data
{
    /// <summary>
    /// A wrapper for conveniently holding a file lock where the stream access is not necessarily needed.
    /// </summary>
    public sealed class FileLock : IDisposable
    {
        // Or these could be used in release, too, if access to them is needed.
        private readonly string m_FileName;
        private readonly FileMode m_CreationMode;
        private readonly FileShare m_FileShare;
        private readonly FileAccess m_FileAccess;
        private readonly bool m_DeleteOnClose;

        private readonly SafeFileHandle m_FileHandle;
        private FileStream m_FileStream;
        private bool m_HaveStream;

        private FileLock(string fileName, FileMode creationMode, FileAccess fileAccess, FileShare fileShare, bool manualDeleteOnClose)
        {
            m_FileName = fileName;
            m_CreationMode = creationMode;
            m_FileShare = fileShare;
            m_FileAccess = fileAccess;
            m_DeleteOnClose = manualDeleteOnClose;
        }

        internal FileLock(SafeFileHandle fileHandle, string fileName, FileMode creationMode, FileAccess fileAccess, FileShare fileShare, bool manualDeleteOnClose)
            : this(fileName, creationMode, fileAccess, fileShare, manualDeleteOnClose)
        {
            m_FileHandle = fileHandle;
            m_FileStream = null;
            m_HaveStream = false;
        }

        internal FileLock(FileStream fileStream, string fileName, FileMode creationMode, FileAccess fileAccess, FileShare fileShare, bool manualDeleteOnClose)
            : this(fileName, creationMode, fileAccess, fileShare, manualDeleteOnClose)
        {
            m_FileHandle = null;
            m_FileStream = fileStream;
            m_HaveStream = (m_FileStream != null);
        }

#if DEBUG
        /// <summary>
        /// The file name locked by this instance. (DEBUG only)
        /// </summary>
        public string FileName { get { return m_FileName; } }

        /// <summary>
        /// The CreationMode used to obtain this lock. (DEBUG only)
        /// </summary>
        public FileMode CreationMode { get { return m_CreationMode; } }

        /// <summary>
        /// The FileAccess used by this lock. (DEBUG only)
        /// </summary>
        public FileAccess FileAccess { get { return m_FileAccess; } }

        /// <summary>
        /// The FileShare allowed by this lock. (DEBUG only)
        /// </summary>
        public FileShare FileShare { get { return m_FileShare; } }
#endif

        /// <summary>
        /// Get the FileStream for this lock instance.
        /// </summary>
        /// <returns></returns>
        public FileStream GetFileStream()
        {
            if (m_HaveStream == false && m_FileHandle != null &&
                m_FileHandle.IsInvalid == false && m_FileHandle.IsClosed == false)
            {
                try
                {
                    m_FileStream = new FileStream(m_FileHandle, m_FileAccess);
                    m_HaveStream = true;
                }
                catch (Exception ex)
                {
                    GC.KeepAlive(ex);
                }
            }

            return m_FileStream;
        }

        /// <summary>
        /// Release the file lock and the resources held by this instance.
        /// </summary>
        public void Dispose()
        {
            if (m_DeleteOnClose && CommonCentralLogic.IsMonoRuntime)
            {
                // For Mono, delete it while we still have it open (exclusively) to avoid a race condition.
                Win32Helper.SafeDeleteFile(m_FileName); // Opens don't stop deletes!
            }

            if (m_HaveStream)
                m_FileStream.Dispose();
            else if ((m_FileHandle != null) && (!m_FileHandle.IsClosed) && (!m_FileHandle.IsInvalid)) //this is a punt to solve a MONO crash.
                m_FileHandle.Dispose();

            m_HaveStream = false;
            m_FileStream = null;

            //and now we try to delete it if we were supposed to.
            if (m_DeleteOnClose && CommonCentralLogic.IsMonoRuntime == false)
            {
                // Not Mono, we can only delete it after we have closed it.
                Win32Helper.SafeDeleteFile(m_FileName); // Delete will fail if anyone else has it open.  That's okay.
            }
        }
    }
}

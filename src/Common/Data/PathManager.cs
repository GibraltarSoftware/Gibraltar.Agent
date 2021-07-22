
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
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

#endregion File Header

namespace Gibraltar.Data
{
    /// <summary>
    /// Determines the correct physical paths to use for various Gibraltar scenarios
    /// </summary>
    public static class PathManager
    {
        /// <summary>
        /// The subfolder of the selected path used for the repository
        /// </summary>
        public const string RepositoryFolder = "Repository";

        /// <summary>
        /// The subfolder of the selected path used for server repositories
        /// </summary>
        public const string ServerRepositoryFolder = "Server Repositories";

        /// <summary>
        /// The subfolder of the selected path used for local session log collection
        /// </summary>
        public const string CollectionFolder = "Local Logs";

        /// <summary>
        /// The subfolder of the selected path used for licensing
        /// </summary>
        public const string LicensingFolder = "Licensing";

        /// <summary>
        /// The subfolder of the selected path used for configuration data
        /// </summary>
        public const string ConfigurationFolder = "Configuration";

        /// <summary>
        /// The subfolder of the selected path used for application Extensions
        /// </summary>
        public const string ExtensionsFolder = "Extensions";

        /// <summary>
        /// The subfolder of the selected path used for discovery information
        /// </summary>
        public const string DiscoveryFolder = "Discovery";

        /// <summary>
        /// Determine the best path of the provided type for the current user
        /// </summary>
        /// <param name="pathType">The path type to retrieve a path for</param>
        /// <returns>The best accessible path of the requested type.</returns>
        /// <remarks>The common application data folder is used if usable
        /// then the local application data folder as a last resort.</remarks>
        public static string FindBestPath(PathType pathType)
        {
            return FindBestPath(pathType, null);
        }

        /// <summary>
        /// Determine the best path of the provided type for the current user
        /// </summary>
        /// <param name="pathType">The path type to retrieve a path for</param>
        /// <param name="preferredPath">The requested full path to use if available.</param>
        /// <returns>The best accessible path of the requested type.</returns>
        /// <remarks>If the preferred path is usable it is used, otherwise the common application data folder is used
        /// then the local application data folder as a last resort.</remarks>
        public static string FindBestPath(PathType pathType, string preferredPath)
        {
            string bestPath = null;

            //first, if they provided an override path we'll start with that.
            if (string.IsNullOrEmpty(preferredPath) == false)
            {
                bestPath = preferredPath;
                if (PathIsUsable(bestPath) == false)
                {
                    //the override path is no good, ignore it.
                    bestPath = null;
                }
            }

            if (string.IsNullOrEmpty(bestPath))
            {
                string pathFolder = PathTypeToFolderName(pathType);

                //First, we want to try to use the all users data directory if this is not the user-repository.
                if (pathType != PathType.Repository && pathType != PathType.ServerRepository)
                {
                    bestPath = CreatePath(Environment.SpecialFolder.CommonApplicationData, pathFolder);
                }

                //Did we get a good path? If not go to the user's folder. (But not for licensing.)
                if (string.IsNullOrEmpty(bestPath))
                {
                    if (pathType != PathType.Licensing)
                    {
                        //nope, we need to switch to the user's LOCAL app data path as our first backup. (not appdata - that may be part of a roaming profile)
                        bestPath = CreatePath(Environment.SpecialFolder.LocalApplicationData, pathFolder);
                    }
                }
                else
                {
                    if (pathType == PathType.Licensing)
                    {
                        // Try to make sure the directory is hidden.
                        try
                        {
                            FileAttributes attr = File.GetAttributes(bestPath);
                            if ((attr & FileAttributes.Hidden) == 0)
                            {
                                attr |= FileAttributes.Hidden;
                                File.SetAttributes(bestPath, attr);
                            }
                        }
                        catch (Exception ex)
                        {
                            GC.KeepAlive(ex); // Just to suppress ReSharper's complaints.
                        }
                    }
                }
            }

            return bestPath;
        }

        /// <summary>
        /// Find a specified file by searching the preferredPath (if provided), central repository folder, and user folder.
        /// </summary>
        /// <param name="pathType">The path type to retrieve a path for</param>
        /// <param name="preferredPath">The requested full path to use if available.</param>
        /// <param name="fileName">The file name to look for.</param>
        /// <returns>The full path to the desired file in the best accessible path to contain it.</returns>
        /// <remarks>If the preferred path contains the file it is returned, otherwise the common application data folder is
        /// searched and then the local application data folder is searched as a last resort.</remarks>
        public static string FindFilePath(PathType pathType, string preferredPath, string fileName)
        {
            string bestPath = null;

            //first, if they provided an override path we'll start with that.
            if (string.IsNullOrEmpty(preferredPath) == false)
            {
                bestPath = Path.Combine(preferredPath, fileName);
                if (File.Exists(bestPath) == false)
                {
                    //the override path doesn't have it, keep looking.
                    bestPath = null;
                }
            }

            if (string.IsNullOrEmpty(bestPath))
            {
                string pathFolder = PathTypeToFolderName(pathType);

                //First, we want to try to use the all users data directory if this is not the user-repository.
                if (pathType != PathType.Repository && pathType != PathType.ServerRepository)
                {
                    bestPath = ComputePath(Environment.SpecialFolder.CommonApplicationData, pathFolder);
                    bestPath = Path.Combine(bestPath, fileName);

                    if (File.Exists(bestPath) == false)
                    {
                        // Not found there, either, keep looking.
                        bestPath = null;
                    }
                }

                //Did we get a good path? If not go to the user's folder. (But not for licensing.)
                if (string.IsNullOrEmpty(bestPath) && pathType != PathType.Licensing)
                {
                    //nope, we need to look in the user's LOCAL app data path as our first backup. (not appdata - that may be part of a roaming profile)
                    bestPath = ComputePath(Environment.SpecialFolder.LocalApplicationData, pathFolder);
                    bestPath = Path.Combine(bestPath, fileName);

                    if (File.Exists(bestPath) == false)
                    {
                        // Not found there.  The file doesn't exist in any searched directory.
                        bestPath = null;
                    }
                }
            }

            return bestPath;
        }

        /// <summary>
        /// Find the full path for the provided subfolder name within a special folder, and make sure it's usable (return null if fails).
        /// </summary>
        /// <param name="specialFolder"></param>
        /// <param name="folderName"></param>
        /// <returns>The full path to the requested folder if it is usable, null otherwise.</returns>
        public static string CreatePath(Environment.SpecialFolder specialFolder, string folderName)
        {
            string bestPath = ComputePath(specialFolder, folderName);

            if (PathIsUsable(bestPath) == false)
                bestPath = null;

            return bestPath;
        }

        /// <summary>
        /// Compute the full path for the provided subfolder name within a special folder.
        /// </summary>
        /// <param name="specialFolder"></param>
        /// <param name="folderName"></param>
        /// <returns>The full path to the requested folder, which may or may not exist.</returns>
        public static string ComputePath(Environment.SpecialFolder specialFolder, string folderName)
        {
            string bestPath = Environment.GetFolderPath(specialFolder);
            bestPath = Path.Combine(bestPath, "Gibraltar");
            bestPath = Path.Combine(bestPath, folderName);

            return bestPath;
        }

        /// <summary>
        /// Determines if the provided full path is usable for the current user
        /// </summary>
        /// <param name="path"></param>
        /// <returns>True if the path is usable, false otherwise</returns>
        /// <remarks>The path is usable if the current user can access the path, create files and write to existing files.</remarks>
        public static bool PathIsUsable(string path)
        {
            //I suck.  I can't figure out a way to easily check if we can create a file and write to it other than to... create a file and try to write to it.
            bool pathIsWritable = true;

            //create a random file name that won't already exist.
            string fileNamePath = Path.Combine(path, Guid.NewGuid().ToString() + ".txt");

            try
            {
                //first, we have to make sure the directory exists.
                if (Directory.Exists(path) == false)
                {
                    //it doesn't - we'll need to create it AND sent the right permissions on it.
                    DirectoryInfo newDirectory = Directory.CreateDirectory(path);

                    if (CommonCentralLogic.IsMonoRuntime == false) // DirectorySecurity isn't implemented in Mono.
                    {
                        // This part could fail on some systems with unexpected permissions structure.
                        try
                        {
                            // Get a DirectorySecurity object that represents the 
                            // current security settings.
                            DirectorySecurity directorySecurity = newDirectory.GetAccessControl();

                            SecurityIdentifier usersSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                            // Add the FileSystemAccessRule to the security settings. 
                            directorySecurity.AddAccessRule(new FileSystemAccessRule(usersSid,
                                                                                     FileSystemRights.Modify |
                                                                                     FileSystemRights.CreateFiles |
                                                                                     FileSystemRights.Delete |
                                                                                     FileSystemRights.ListDirectory |
                                                                                     FileSystemRights.Read |
                                                                                     FileSystemRights.Write,
                                                                                     InheritanceFlags.ContainerInherit |
                                                                                     InheritanceFlags.ObjectInherit,
                                                                                     PropagationFlags.None, AccessControlType.Allow));

                            // Set the new access settings.
                            newDirectory.SetAccessControl(directorySecurity);
                        }
                        catch(Exception ex)
                        {
                            // If we fail setting permissions, it could be that we don't have full control to our own creation!
                            // But we may still have sufficient write permissions to proceed, so continue to our write test.
                            // Continuing here is little different than another time after the directory was already created.
                            if (!CommonCentralLogic.SilentMode)
                            {
                                // This used to be category "Gibraltar.Data.Directory.Permissions", but we had to remove
                                // the Gibraltar API call to move this class into Common for access from Licensing.
                                Trace.TraceWarning("Unable to set proper permissions on new directory\r\n" +
                                                   "An attempt to set necessary all-users write permission on a new folder failed.\r\n" +
                                                   "If the directory does not already have sufficient permission then it may cause failures, " +
                                                   "and an administrator may have to adjust the permissions on the folder to fix it.\r\n" +
                                                   "Path: {0}\r\nException: {1} {2}", path, ex, ex.Message);
                            }
                        }
                    }
                }

                using (StreamWriter testFile = File.CreateText(fileNamePath))
                {
                    //OK, we can CREATE a file, can we WRITE to it?
                    testFile.WriteLine("This is a test file created by Loupe to verify that the directory is writable.");
                    testFile.Flush();
                }

                //we've written it and closed it, now open it again.
                using (StreamReader testFile = File.OpenText(fileNamePath))
                {
                    testFile.ReadToEnd();
                }

                //no exception there, we're good to go.  we'll delete it in a minute outside of our pass/fail handler.
            }
            catch
            {
                //if we can't do it, it's not writable for some reason.
                pathIsWritable = false;
            }

            try
            {
                File.Delete(fileNamePath);
            }
            catch
            {
                Debug.WriteLine("While the path {0} is usable because we can create, write, and read files we can't delete files, so purging won't work.");
            }

            return pathIsWritable;
        }

        private static string PathTypeToFolderName(PathType pathType)
        {
            string pathFolder;
            switch (pathType)
            {
                case PathType.Collection:
                    pathFolder = CollectionFolder;
                    break;
                case PathType.Repository:
                    pathFolder = RepositoryFolder;
                    break;
                case PathType.Licensing:
                    pathFolder = LicensingFolder;
                    break;
                case PathType.Configuration:
                    pathFolder = ConfigurationFolder;
                    break;
                case PathType.Extensions:
                    pathFolder = ExtensionsFolder;
                    break;
                case PathType.Discovery:
                    pathFolder = DiscoveryFolder;
                    break;
                case PathType.ServerRepository:
                    pathFolder = ServerRepositoryFolder;
                    break;
                default:
                    throw new InvalidDataException("The current path type is unknown, indicating a programming error.");
            }

            return pathFolder;
        }
    }
}
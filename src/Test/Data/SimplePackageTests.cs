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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Gibraltar.Data;
using Gibraltar.Monitor;
using NUnit.Framework;

namespace Gibraltar.Test.Data
{
    [TestFixture]
    public class SimplePackageTests
    {
        private string m_OutputFileNamePath;

        [OneTimeSetUp]
        public void Init()
        {
            if (string.IsNullOrEmpty(m_OutputFileNamePath) == false)
            {
                //we're re-initing:  delete any existing temp file.
                File.Delete(m_OutputFileNamePath);
            }

            m_OutputFileNamePath = Path.GetTempFileName();
            File.Delete(m_OutputFileNamePath); //get temp file name creates the file as part of allocating the name.
            m_OutputFileNamePath += "." + Log.PackageExtension;
        }

        [Test]
        public void CreateEmptyPackage()
        {
            using(var package = new SimplePackage())
            {
                using (ProgressMonitorStack stack = new ProgressMonitorStack("saving Package"))
                {
                    package.Save(stack, m_OutputFileNamePath);
                }
            }

            Assert.IsTrue(File.Exists(m_OutputFileNamePath), "Package was not created");

            File.Delete(m_OutputFileNamePath);
        }

        [Test]
        public void CreateLargePackage()
        {
            using (var package = new SimplePackage())
            {
                DirectoryInfo repository = new DirectoryInfo(LocalRepository.CalculateRepositoryPath(Log.SessionSummary.Product));

                FileInfo[] allExistingFileFragments = repository.GetFiles("*." + Log.LogExtension, SearchOption.TopDirectoryOnly);

                foreach (var fileFragment in allExistingFileFragments)
                {
                    //sourceFile = new FileStream(fileFragment, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    var sourceFile = Win32Helper.OpenFileStream(fileFragment.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                    if (sourceFile != null)
                    {
                        package.AddSession(sourceFile);
                    }
                }

                using (ProgressMonitorStack stack = new ProgressMonitorStack("Saving Package"))
                {
                    package.Save(stack, m_OutputFileNamePath);
                }
            }

            Assert.IsTrue(File.Exists(m_OutputFileNamePath), "Package was not created");
            Assert.Greater(new FileInfo(m_OutputFileNamePath).Length, 100, "The package was likely empty but should have contained multiple sessions.");

            File.Delete(m_OutputFileNamePath);            
        }
    }
}

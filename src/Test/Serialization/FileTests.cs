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
#if DEBUG

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
using System.Globalization;
using System.IO;
using Gibraltar.Serialization.Internal;
using NUnit.Framework;

#endregion

#pragma warning disable 1591

namespace Gibraltar.Serialization.UnitTests
{
    [TestFixture]
    public class FileTests
    {
        [Test]
        public void CheckBufferedMemoryStream()
        {
            const int repeatCount = 100;
            const int sequenceLength = 251; // choose a prime number so that it each buffer is different 
            byte[] expected = new byte[sequenceLength];
            for ( int i = 0; i < sequenceLength; i++)
                expected[i] = (byte)i;

            MemoryStream testBuffer = new MemoryStream();
            for ( int i = 0; i < repeatCount; i++ )
                testBuffer.Write(expected, 0, sequenceLength);

            testBuffer.Position = 0;
            BufferedMemoryStream fancyBuffer = new BufferedMemoryStream(testBuffer);
            for (int i = 0; i < repeatCount; i++)
            {
                byte[] actual = new byte[expected.Length];
                int bytesRead = fancyBuffer.Read(actual, 0, sequenceLength );
                Trace.WriteLine("Completed read packet " + i + " ( " + bytesRead + " bytes)");
                if (bytesRead != sequenceLength)
                    Assert.AreEqual( bytesRead, sequenceLength);
                for (int j = 0; j < sequenceLength; j++)
                {
                    if ( expected[j] != actual[j])
                        Assert.AreEqual(expected[j], actual[j]);
                }
            }
        }

        //[Test]
        public void TimeTest()
        {
            string fileName = Path.GetTempFileName();
            const int LoopCount = 1024;
            byte[] buffer = new byte[1024];

            FileStream file = new FileStream(fileName, FileMode.Create);
            DateTime startTime = DateTime.UtcNow;
            for (int i = 0; i < LoopCount; i++)
                for (int j = 0; j < buffer.Length; j++ )
                    file.WriteByte(buffer[j]);
            file.Close();
            TimeSpan writeTimeNormal = DateTime.UtcNow - startTime;

            file = new FileStream(fileName, FileMode.Open);
            while (file.Position < file.Length)
            {
                int b = file.ReadByte();
            }
            TimeSpan readTimeNormal = DateTime.UtcNow - startTime;
            file.Close();

            file = new FileStream(fileName, FileMode.Open);
            BufferedMemoryStream stream = new BufferedMemoryStream(file);
            startTime = DateTime.UtcNow;
            while (file.Position < file.Length)
            {
                long i = file.Position;
                int b = stream.ReadByte();

            }
            TimeSpan readTimeBuffered = DateTime.UtcNow - startTime;
            file.Close();
            File.Delete(fileName);

            string msg = string.Format(CultureInfo.CurrentCulture, "TimeSpan 1 = {0}\nTimeSpan 2 = {1}", readTimeNormal, readTimeBuffered);
            Assert.Fail(msg);
        }
    }
}

#endif
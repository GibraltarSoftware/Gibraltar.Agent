
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
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Gibraltar.Monitor;
using Gibraltar.Data;
using NUnit.Framework;

#endregion File Header

namespace Gibraltar.Test
{
    [TestFixture]
    public class StringReferenceTests
    {
        private static string GetTestString(int number)
        {
            return "test " + number;
        }

        private static void GarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        [Test]
#if !DEBUG
        [Ignore("debugging test only")] // Until we can fix the Release optimizations breaking the GC expectations.
#endif
        public void WeakStringCollectionTest()
        {
            string primeZero = GetTestString(0);
            string primeOne = GetTestString(1);
            string primeTwo = GetTestString(2);
            string primeThree = GetTestString(3);

            StringReference.WeakStringCollection testCollection = new StringReference.WeakStringCollection(primeZero);
            Assert.AreEqual(1, testCollection.Count);
            primeZero = null;
            GC.Collect(); // And make sure GC kills it.
            Assert.AreEqual(0, testCollection.Pack()); // Confirm that it's gone.
            primeZero = GetTestString(0);
            Assert.AreEqual(1, testCollection.PackAndOrAdd(ref primeZero)); // Add it back.

            string testZero = GetTestString(0);
            Assert.IsFalse(ReferenceEquals(testZero, primeZero));
            Assert.AreEqual(1, testCollection.PackAndOrAdd(ref testZero));
            Assert.IsTrue(ReferenceEquals(testZero, primeZero));

            primeZero = null; // Remove the original reference.
            GC.Collect(); // And make sure GC kills it (but won't kill the new copy).
            Assert.AreEqual(1, testCollection.Pack());

            testZero = null; // Remove the new reference.
            GC.Collect(); // And make sure GC kills it.
            Assert.AreEqual(1, testCollection.PackAndOrAdd(ref primeOne));

            primeZero = GetTestString(0);
            Assert.AreEqual(2, testCollection.PackAndOrAdd(ref primeZero));
            testZero = GetTestString(0);
            Assert.IsFalse(ReferenceEquals(testZero, primeZero));
            Assert.AreEqual(2, testCollection.PackAndOrAdd(ref testZero));
            Assert.IsTrue(ReferenceEquals(testZero, primeZero));

            primeOne = null; // Remove the original reference.
            GC.Collect(); // And make sure GC kills it.
            Assert.AreEqual(2, testCollection.PackAndOrAdd(ref primeTwo));
            primeOne = GetTestString(1);
            Assert.AreEqual(3, testCollection.PackAndOrAdd(ref primeOne));
            Assert.AreEqual(4, testCollection.PackAndOrAdd(ref primeThree));

            testZero = GetTestString(0);
            Assert.IsFalse(ReferenceEquals(testZero, primeZero));
            Assert.AreEqual(4, testCollection.PackAndOrAdd(ref testZero));
            Assert.IsTrue(ReferenceEquals(testZero, primeZero));

            primeZero = null; // Remove the original reference (but not the testZero reference).
            primeOne = null; // Remove the only reference.
            primeTwo = null; // Remove the only reference.
            GC.Collect(); // And make sure GC kills them.
            Assert.AreEqual(2, testCollection.Pack());

            primeZero = GetTestString(0);
            Assert.IsFalse(ReferenceEquals(testZero, primeZero));
            Assert.AreEqual(2, testCollection.PackAndOrAdd(ref primeZero));
            Assert.IsTrue(ReferenceEquals(testZero, primeZero));
        }
    }
}

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
using System.Text;

namespace Gibraltar.Data
{
    /// <summary>
    /// A central class to collect common elements of the generic LogicTree&lt;D&gt; classes (q.v.)
    /// </summary>
    public abstract class LogicTree
    {
        /// <summary>
        /// A list of all user-selectable operations.
        /// </summary>
        public static readonly MatchOperationType[] AllOperations = new[]
            {
                MatchOperationType.Equals,
                MatchOperationType.NotEquals,
                MatchOperationType.LessThan,
                MatchOperationType.GreaterThan,
                MatchOperationType.LessOrEquals,
                MatchOperationType.GreaterOrEquals,
                MatchOperationType.InRange,
                MatchOperationType.NotInRange,
                MatchOperationType.Between,
                MatchOperationType.NotBetween,
                MatchOperationType.StartsWith,
                MatchOperationType.NotStartsWith,
                MatchOperationType.EndsWith,
                MatchOperationType.NotEndsWith,
                MatchOperationType.Contains,
                MatchOperationType.NotContains,
                MatchOperationType.IncludeList,
                MatchOperationType.ExcludeList
            };

        #region Provided standard Compare methods

        /// <summary>
        /// A general method to compare a data value against a configured field value of the same given class type.
        /// </summary>
        /// <remarks>This generic method can be used as a LogicNode.ComparisonDelegate&lt;T&gt; for most field
        /// types which implement IComparable&lt;T&gt; where T is a reference type (class).</remarks>
        /// <typeparam name="T">The type of the data and field values.</typeparam>
        /// <param name="data">The data value to compare.</param>
        /// <param name="fieldValue">The field value to compare the data value against.</param>
        /// <returns>A comparison integer which is less-than, equal-to, or greater-than zero.</returns>
        public static int CompareReference<T>(T data, T fieldValue)
            where T : class, IComparable<T>
        {
            if (fieldValue == null)
                return (data == null) ? 0 : 1; // Everything but null data is greater than null fieldValue.

            if (data == null)
                return -1; // Null data is less than any non-null fieldValue.

            // Here, neither is null, so now we can safely call the object's CompareTo().
            // Notice that we call it in fieldValue, but that reverses the order of comparison, so we must negate it.
            int compareResult = -(fieldValue.CompareTo(data));

            return compareResult;
        }

        /// <summary>
        /// A general method to compare a data value against a configured field value of the same given struct type.
        /// </summary>
        /// <remarks>This generic method can be used as a LogicNode.ComparisonDelegate&lt;T&gt; for most field
        /// types which implement IComparable&lt;T&gt; where T is a value type (struct).</remarks>
        /// <typeparam name="T">The type of the data and field values.</typeparam>
        /// <param name="data">The data value to compare.</param>
        /// <param name="fieldValue">The field value to compare the data value against.</param>
        /// <returns>A comparison integer which is less-than, equal-to, or greater-than zero.</returns>
        public static int CompareValue<T>(T data, T fieldValue)
            where T : struct, IComparable<T>
        {
            // Notice that we call it in fieldValue, but that reverses the order of comparison, so we must negate it.
            int compareResult = -(fieldValue.CompareTo(data));

            return compareResult;
        }

        /// <summary>
        /// A method to compare a Version data value against a configured Version field value.
        /// </summary>
        /// <remarks>This method can be used as a LogicNode.ComparisonDelegate&lt;T&gt; for Version
        /// match operations.  This method supports automatic prefix truncation (of build number or revision)
        /// when the configured field value leaves later components undefined, but does not support embedded
        /// wildcards.  Also, the System.Version class does not allow major or minor version to be undefined,
        /// so minor version can not be implicitly wildcarded with this method, either.  See the Gibraltar
        /// VersionComparison class for a more flexible data field type for version comparison predicates.</remarks>
        /// <param name="data">The Version data value to compare.</param>
        /// <param name="fieldValue">The Version field value to compare the data value against.</param>
        /// <returns>A comparison integer which is less-than, equal-to, or greater-than zero.</returns>
        public static int CompareVersion(Version data, Version fieldValue)
        {
            if (fieldValue == null)
                return (data == null) ? 0 : int.MaxValue;

            if (data == null)
                return int.MinValue;

            // Notice that we call it in fieldValue, but that reverses the order of comparison, so we must negate it.
            int compareResult = -fieldValue.Major.CompareTo(data.Major);

            if (compareResult == 0)
            {
                compareResult = -fieldValue.Minor.CompareTo(data.Minor);

                if (compareResult == 0 && fieldValue.Build >= 0)
                {
                    compareResult = -fieldValue.Build.CompareTo(data.Build);

                    if (compareResult == 0 && fieldValue.Revision >= 0)
                        compareResult = -fieldValue.Revision.CompareTo(data.Revision);
                }
            }

            return compareResult;
        }

        /// <summary>
        /// A method to compare a Version data value against a configured Version field value by Build only.
        /// </summary>
        /// <remarks>This method can be used as a LogicNode.ComparisonDelegate&lt;T&gt; for Version
        /// match operations.  This method supports comparison by monotonically-increasing build number, ignoring
        /// major and minor version.  See the Gibraltar VersionComparison class for a more flexible data field type
        /// for version comparison predicates.</remarks>
        /// <param name="data">The Version data value whose Build property to compare.</param>
        /// <param name="fieldValue">The Version field value whose Build property to compare the data value against.</param>
        /// <returns>A comparison integer which is less-than, equal-to, or greater-than zero.</returns>
        public static int CompareVersionBuildOnly(Version data, Version fieldValue)
        {
            if (fieldValue == null)
                return (data == null) ? 0 : int.MaxValue;

            if (data == null)
                return int.MinValue;

            // Notice that we call it in fieldValue, but that reverses the order of comparison, so we must negate it.
            int compareResult = -fieldValue.Build.CompareTo(data.Build);

            return compareResult;
        }

        /// <summary>
        /// A method to compare a string data value against a configured field value prefix.
        /// </summary>
        /// <remarks>This method can be used as a LogicNode.ComparisonDelegate&lt;T&gt; for string StartsWith
        /// match operations.</remarks>
        /// <param name="data">The string data value to compare.</param>
        /// <param name="fieldValue">The field value prefix to compare the data value against.</param>
        /// <returns>A comparison integer which is less-than, equal-to, or greater-than zero.</returns>
        public static int ComparePrefix(string data, string fieldValue)
        {
            if (fieldValue == null)
                return (data == null) ? 0 : 1;

            if (data == null)
                return int.MinValue;

            int startLength = fieldValue.Length;
            int dataLength = data.Length;

            if (startLength == 0)
                return 0; // Every non-null string starts with an empty-string suffix.

            if (startLength > dataLength)
                return int.MinValue; // data string is too short for the test prefix; it can't start with it.

            string dataStart = data.Substring(0, startLength); // Get prefix of same length to test.

            // Notice that we call it in fieldValue, but that reverses the order of comparison, so we must negate it.
            int compareResult = -(fieldValue.CompareTo(dataStart));

            return compareResult;
        }

        /// <summary>
        /// A method to compare a string data value against a configured field value suffix.
        /// </summary>
        /// <remarks>This method can be used as a LogicNode.ComparisonDelegate&lt;T&gt; for string EndsWith
        /// match operations.</remarks>
        /// <param name="data">The string data value to compare.</param>
        /// <param name="fieldValue">The field value suffix to compare the data value against.</param>
        /// <returns>A comparison integer which is less-than, equal-to, or greater-than zero.</returns>
        public static int CompareSuffix(string data, string fieldValue)
        {
            if (fieldValue == null)
                return (data == null) ? 0 : int.MaxValue;

            if (data == null)
                return int.MinValue;

            int endLength = fieldValue.Length;
            int dataLength = data.Length;
            int endIndex = dataLength - endLength;

            if (endLength == 0)
                return 0; // Every non-null string ends with an empty-string suffix.

            if (endIndex < 0)
                return int.MinValue; // data string is too short for the test suffix; it can't end with it.

            string dataEnd = (endIndex == 0) ? data : data.Substring(endIndex); // Get ending of same length to test.

            // Notice that we call it in fieldValue, but that reverses the order of comparison, so we must negate it.
            int compareResult = -(fieldValue.CompareTo(dataEnd));

            return compareResult;
        }

        /// <summary>
        /// A method to compare if a string data value contains a configured field value substring.
        /// </summary>
        /// <remarks>This method can be used as a LogicNode.ComparisonDelegate&lt;T&gt; for string Contains
        /// match operations.</remarks>
        /// <param name="data">The string data value to compare.</param>
        /// <param name="fieldValue">The field value substring to compare the data value against.</param>
        /// <returns>A comparison integer which is less-than, equal-to, or greater-than zero.</returns>
        public static int CompareContains(string data, string fieldValue)
        {
            if (fieldValue == null)
                return (data == null) ? 0 : int.MaxValue;

            if (data == null)
                return int.MinValue;

            int subLength = fieldValue.Length;
            int dataLength = data.Length;

            if (subLength == 0)
                return 0; // Every non-null string contains an empty-string substring.

            if (dataLength < subLength)
                return int.MinValue; // data string is too short for the test substring; it can't contain it.

            int compareResult = data.Contains(fieldValue) ? 0 : int.MinValue; // Convert the bool result into a comparison int.

            return compareResult;
        }

        #endregion

    }

    /// <summary>
    /// Represents a logic tree in a user-configured search query of dataset type D.
    /// </summary>
    /// <typeparam name="D">The type of dataset (or of key to a logical dataset) targeted by this LogicTree.</typeparam>
    /// <remarks>This class supports the building of a tree of ILogicNode&lt;D&gt; to evaluate a compound search query
    /// expression.</remarks>
    public class LogicTree<D> : LogicTree
    {
        #region Private members

        private ILogicNode<D> m_RootNode = null;

        #endregion

        #region Public properties

        #endregion

        #region Tree-building methods

        /// <summary>
        /// Clear this LogicTree instance, discarding all expressions and logic nodes.
        /// </summary>
        /// <remarks>This does not perform any Dispose() of any field values assigned to logic nodes even if they
        /// happen to be IDisposable types (the vast majority should not be).</remarks>
        public void Clear()
        {
            m_RootNode = null;
        }

        /*
         * ToDo: Implement a more Janus-style expression building system, starting from these notes.
         * (See Monitor.Windows.UI\UILogMessageTreeView.cs (MatchByInflectionNodes()) for example Janus usage.)
         * 
         * Notes on how to build a logic tree for a search query, for dataset type D:
         * 
         * Build up a LogicTree ala Janus conditions (editable).
         * Ask LogicTree to construct a fixed LogicTreeExpression (need to reverse names?),
         * which compiles the chained LogicNodes.
         * Ask the LogicTreeExpression to evaluate on a given dataset (eg. session info instance),
         * which evaluates from the LogicNodes.
         * OR, ask the LogicTreeExpression for its RootNode (LTE can be discarded)
         * and use static LogicTree.EvaluateMatch(RootNode, dataset).
         * 
         * LogicTree (or LTE if names reversed) needs to have constructors for predicate construction, empty container,
         * wrapping container?, and possibly a pre-populated container (by a single(?) logical op, AND or OR).
         * These last three may all work as one params overload?  Janus does not do pre-populated container construction.
         * It probably also needs to support full cloning (separate copy of tree with same logic and predicates).
         */

        #endregion

        #region Predicate-building methods

        /// <summary>
        /// Create a LogicNode&lt;D,F&gt; using standard compare methods for normal reference (class) types.
        /// </summary>
        /// <typeparam name="F">The type of field to be tested by the new node.</typeparam>
        /// <param name="getDataValue">A GetDataValueDelegate&lt;D,F&gt; to extract the intended data field from a parameter dataset.</param>
        /// <param name="operationType">The desired MatchOperationType enum.</param>
        /// <param name="fieldValue">The single configured field value to compare against.</param>
        /// <returns>A new LogicNode&lt;D,F&gt; to evaluate the requested match predicate.</returns>
        public LogicNode<D,F> CreateReferencePredicateNode<F>(LogicNode.GetDataValueDelegate<D, F> getDataValue,
                                                              MatchOperationType operationType, F fieldValue)
            where F : class, IComparable<F>
        {
            return CreateStandardReferencePredicateNode(getDataValue, operationType, fieldValue, fieldValue);
        }

        /// <summary>
        /// Create a LogicNode&lt;D,F&gt; using standard compare methods for normal reference (class) types.
        /// </summary>
        /// <typeparam name="F">The type of field to be tested by the new node.</typeparam>
        /// <param name="getDataValue">A GetDataValueDelegate&lt;D,F&gt; to extract the intended data field from a parameter dataset.</param>
        /// <param name="operationType">The desired MatchOperationType enum.</param>
        /// <param name="fieldA">The primary configured field value to compare against.</param>
        /// <param name="fieldB">A secondary configured field value to compare against.</param>
        /// <returns>A new LogicNode&lt;D,F&gt; to evaluate the requested match predicate.</returns>
        public LogicNode<D,F> CreateStandardReferencePredicateNode<F>(LogicNode.GetDataValueDelegate<D,F> getDataValue,
                                                                      MatchOperationType operationType, F fieldA, F fieldB)
            where F : class, IComparable<F>
        {
            if ((operationType & MatchOperationType.SpecialOperationsMask) != 0)
            {
                throw new GibraltarException(string.Format("Non-standard operation {0:X4} ({0:F}) not supported by CreateStandardReferencePredicateNode.",
                                                           operationType));
            }

            return new LogicNode<D,F>(getDataValue, CompareReference, operationType, fieldA, fieldB);
        }

        /// <summary>
        /// Create a LogicNode&lt;D,F&gt; using standard compare methods for normal value (struct) types.
        /// </summary>
        /// <typeparam name="F">The type of field to be tested by the new node.</typeparam>
        /// <param name="getDataValue">A GetDataValueDelegate&lt;D,F&gt; to extract the intended data field from a parameter dataset.</param>
        /// <param name="operationType">The desired MatchOperationType enum.</param>
        /// <param name="fieldValue">The single configured field value to compare against.</param>
        /// <returns>A new LogicNode&lt;D,F&gt; to evaluate the requested match predicate.</returns>
        public LogicNode<D,F> CreateValuePredicateNode<F>(LogicNode.GetDataValueDelegate<D, F> getDataValue,
                                                          MatchOperationType operationType, F fieldValue)
            where F : struct, IComparable<F>
        {
            return CreateStandardValuePredicateNode(getDataValue, operationType, fieldValue, fieldValue);
        }

        /// <summary>
        /// Create a LogicNode&lt;D,F&gt; using standard compare methods for normal value (struct) types.
        /// </summary>
        /// <typeparam name="F">The type of field to be tested by the new node.</typeparam>
        /// <param name="getDataValue">A GetDataValueDelegate&lt;D,F&gt; to extract the intended data field from a parameter dataset.</param>
        /// <param name="operationType">The desired MatchOperationType enum.</param>
        /// <param name="fieldA">The primary configured field value to compare against.</param>
        /// <param name="fieldB">A secondary configured field value to compare against.</param>
        /// <returns>A new LogicNode&lt;D,F&gt; to evaluate the requested match predicate.</returns>
        public LogicNode<D,F> CreateStandardValuePredicateNode<F>(LogicNode.GetDataValueDelegate<D,F> getDataValue,
                                                                  MatchOperationType operationType, F fieldA, F fieldB)
            where F : struct, IComparable<F>
        {
            if ((operationType & MatchOperationType.SpecialOperationsMask) != 0)
            {
                throw new GibraltarException(string.Format("Non-standard operation {0:X4} ({0:F}) not supported by CreateStandardValuePredicateNode.",
                                                           operationType));
            }

            return new LogicNode<D,F>(getDataValue, CompareValue, operationType, fieldA, fieldB);
        }

        /// <summary>
        /// Create a LogicNode&lt;D,F&gt; using compare methods for special string match operations.
        /// </summary>
        /// <param name="getDataValue">A GetDataValueDelegate&lt;D,F&gt; to extract the intended data field from a parameter dataset.</param>
        /// <param name="operationType">The desired MatchOperationType enum.</param>
        /// <param name="fieldA">The primary configured field value to compare against.</param>
        /// <param name="fieldB">A secondary configured field value to compare against.</param>
        /// <returns>A new LogicNode&lt;D,F&gt; to evaluate the requested match predicate.</returns>
        public LogicNode<D,string> CreateSpecialStringPredicateNode(LogicNode.GetDataValueDelegate<D,string> getDataValue,
                                                                    MatchOperationType operationType, string fieldA, string fieldB)
        {
            if ((operationType & MatchOperationType.SpecialOperationsMask) == 0)
            {
                // This is actually covered in the standard compare operations, so just punt it to them.
                return CreateStandardReferencePredicateNode(getDataValue, operationType, fieldA, fieldB);
            }

            LogicNode.ComparisonDelegate<string> comparisonDelegate;
            switch (operationType)
            {
                case MatchOperationType.StartsWith:
                case MatchOperationType.NotStartsWith:
                    comparisonDelegate = ComparePrefix;
                    break;
                case MatchOperationType.EndsWith:
                case MatchOperationType.NotEndsWith:
                    comparisonDelegate = CompareSuffix;
                    break;
                case MatchOperationType.Contains:
                case MatchOperationType.NotContains:
                    comparisonDelegate = CompareContains;
                    break;
                default:
                    throw new GibraltarException(string.Format("Unrecognized operation {0:X4} ({0:F}) not supported by CreateSpecialStringPredicateNode.",
                                                               operationType));
            }

            return new LogicNode<D, string>(getDataValue, comparisonDelegate, operationType, fieldA, fieldB);
        }

        #endregion

        #region Evaluation Methods
        /// <summary>
        /// Evaluate this LogicTree&lt;D&gt; on a specific dataset instance.
        /// </summary>
        /// <remarks>The dataset parameter can also be a key to select a logical dataset.  All nodes in the tree
        /// must operate on the same dataset (or key) type, but can access different data values (of different types)
        /// within that dataset and/or compare against different configured field values.</remarks>
        /// <param name="dataset">A dataset instance (or a key to a logical dataset) to test for a match.</param>
        /// <returns>True if the dataset instance matches on this LogicNode tree, false if it does not match.</returns>
        public bool EvaluateMatch(D dataset)
        {
            return EvaluateMatch(m_RootNode, dataset);
        }

        /// <summary>
        /// Evaluate an ILogicNode&lt;D&gt; tree on a specific dataset instance.
        /// </summary>
        /// <remarks>The dataset parameter can also be a key to select a logical dataset.  All nodes in the tree
        /// must operate on the same dataset (or key) type, but can access different data values (of different types)
        /// within that dataset and/or compare against different configured field values.</remarks>
        /// <param name="rootNode">The root node of the logic tree to begin evaluation.</param>
        /// <param name="dataset">A dataset instance (or a key to a logical dataset) to test for a match.</param>
        /// <returns>True if the dataset instance matches on this LogicNode tree, false if it does not match.</returns>
        public static bool EvaluateMatch(ILogicNode<D> rootNode, D dataset)
        {
            bool match = false; // Null rootNode doesn't match anything.

            ILogicNode<D> currentNode = rootNode;
            while (currentNode != null)
            {
                match = currentNode.EvaluateNode(dataset);

                if (match)
                    currentNode = currentNode.NextOnTrue;
                else
                    currentNode = currentNode.NextOnFalse;
            }

            return match;
        }

        #endregion
    }

    #region VersionComparison class

    /// <summary>
    /// A class to represent a version value for comparison with simple wildcarding.
    /// </summary>
    public class VersionComparison : IComparable<VersionComparison>
    {
        private readonly int[] m_VersionArray;

        /// <summary>
        /// Construct a VersionComparison instance from a given System.Version instance.
        /// </summary>
        /// <param name="version">The System.Version object to represent for comparison.</param>
        public VersionComparison(Version version)
            : this(version.ToString())
        {
            // Just defer to the string-based constructor
        }

        /// <summary>
        /// Construct a VersionComparison instance from a string, optionally including simple wildcards.
        /// </summary>
        /// <param name="versionString">A version string (with optional "*" components) for comparison.</param>
        public VersionComparison(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
            {
                m_VersionArray = new[] {-1}; // Set us to a wildcard that matches everything.
                return;
            }

            string[] splitString = versionString.Split('.');
            int length = splitString.Length;

            m_VersionArray = new int[length];
            for (int i=0; i<length; i++)
            {
                int component;
                if (int.TryParse(splitString[i], out component) == false)
                {
                    if (splitString[i] == "*" || (i == (length - 1) && string.IsNullOrEmpty(splitString[i])))
                        component = -1;
                    else
                        throw new GibraltarException(string.Format("Unrecognized component [{0}] \"{1}\" in version string \"{2}\".",
                                                                   i, splitString[i], versionString));
                }
                m_VersionArray[i] = component;
            }
        }

        /// <summary>
        /// Compares this VersionComparison instance (with optional wildcards) to another representing an actual version.
        /// </summary>
        /// <remarks>The partial or wildcarded VersionComparison instance should be used as the basis for the call and
        /// passed the specific VersionComparison instance representing an actual version.</remarks>
        /// <returns>A comparison integer which is less-than, equal-to, or greater-than zero.</returns>
        /// <param name="other">A VersionComparison representing an actual version to compare against this one.</param>
        public int CompareTo(VersionComparison other)
        {
            if (other == null)
                return int.MaxValue; // We can't be null, so we are "greater than" a null other.

            int compareResult = 0;

            for (int i=0; i < m_VersionArray.Length; i++)
            {
                // Negative values in our array designate a simple wildcard, matching on anything in the other one.
                // So we only need to compare against the other one if our current component is non-negative.
                if (m_VersionArray[i] >= 0)
                {
                    // Make sure we don't access past the end of the other array.
                    if (other.m_VersionArray.Length > i && other.m_VersionArray[i] >= 0)
                    {
                        // Real numbers on both sides, do a normal comparison by difference.
                        compareResult = m_VersionArray[i] - other.m_VersionArray[i];
                    }
                    else
                    {
                        // Hmmm, we have a real number but it's being compared against a wildcard component in other.
                        // Our argument is supposed to be an actual version, so should this be considered a mismatch?
                        // We'll treat it as if it's a 0 ("Release 2.3" really means "2.3.0.0.etc"?).
                        compareResult = m_VersionArray[i]; // our value minus 0...

#if DEBUG
                        if (Debugger.IsAttached)
                            Debugger.Break(); // Catch this if we're debugging.
#endif
                    }
                }

                // If we have an unequal result, we need to stop looking and return that.
                if (compareResult != 0)
                    break;
            }

            return compareResult;
        }

        /// <summary>
        /// Convert this VersionComparison instance to a string representation.
        /// </summary>
        /// <returns>A string representing the version (with possible wildcards) indicated by this instance.</returns>
        public override string ToString()
        {
            const string wildcard = "*";
            StringBuilder builder = new StringBuilder();

            // We always have at least one entry, so stuff that in.
            builder.Append(m_VersionArray[0] < 0 ? wildcard : m_VersionArray[0].ToString());
            for (int i=1; i < m_VersionArray.Length; i++)
            {
                builder.Append(".");
                builder.Append(m_VersionArray[i] < 0 ? wildcard : m_VersionArray[i].ToString());
            }

            return builder.ToString();
        }
    }

    #endregion
}

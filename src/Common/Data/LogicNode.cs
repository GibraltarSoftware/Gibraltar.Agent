
using System;
using System.Collections.Generic;
using System.Text;

namespace Gibraltar.Data
{
    /// <summary>
    /// A central class to collect common elements of the generic LogicNode&lt;D,F&gt; classes (q.v.)
    /// </summary>
    public abstract class LogicNode
    {
        /// <summary>
        /// A delegate to return a data value of type R given a dataset (or key) of type K.
        /// </summary>
        /// <typeparam name="K">The type of dataset (or key) passed to the delegate.</typeparam>
        /// <typeparam name="R">The type of data returned by the delegate.</typeparam>
        /// <param name="key">A key (of type K) to select the specific instance of data to test.</param>
        /// <returns>The desired data value (of type R) to be tested by a given LogicNode.</returns>
        public delegate R GetDataValueDelegate<K,R>(K key);

        /// <summary>
        /// A delegate to compare a data value against a configured field value of the same given type.
        /// </summary>
        /// <typeparam name="T">The type of the data and field values.</typeparam>
        /// <param name="data">The data value to compare.</param>
        /// <param name="fieldValue">The field value to compare the data value against.</param>
        /// <returns>A comparison integer which is less-than, equal-to, or greater-than zero.</returns>
        public delegate int ComparisonDelegate<T>(T data, T fieldValue);
    }

    /// <summary>
    /// Represents a place-holder logic node in a user-configured search query of dataset type D returning a configured result.
    /// </summary>
    /// <remarks>These dummy nodes (Always-match or Never-match) are primarily useful for negation (used in pairs).</remarks>
    /// <typeparam name="D">The type of the dataset (or of a key to a logical dataset) for this search query.</typeparam>
    public class LogicNode<D> : LogicNode, ILogicNode<D>
    {
        private readonly bool m_Result;
        private ILogicNode<D> m_NextOnTrue;
        private ILogicNode<D> m_NextOnFalse;

        /// <summary>
        /// The next ILogicNode&lt;D&gt; to evaluate if this node's evaluation is true, or null to conclude with true.
        /// </summary>
        public ILogicNode<D> NextOnTrue { get { return m_NextOnTrue; } set { m_NextOnTrue = value; } }

        /// <summary>
        /// The next ILogicNode&lt;D&gt; to evaluate if this node's evaluation is false, or null to conclude with false.
        /// </summary>
        public ILogicNode<D> NextOnFalse { get { return m_NextOnFalse; } set { m_NextOnFalse = value; } }

        /// <summary>
        /// Constructor for a place-holder LogicNode which always returns its configured boolean result.
        /// </summary>
        /// <param name="result">The boolean result to be returned by this node if evaluated.</param>
        public LogicNode(bool result)
        {
            m_Result = result;
            m_NextOnTrue = null;
            m_NextOnFalse = null;
        }

        /// <summary>
        /// Constructor for a LogicNode clone based on an original LogicNode&lt;D&gt;'s configured result.
        /// </summary>
        /// <param name="original">The original LogicNode&lt;D&gt; whose local behavior to clone.</param>
        public LogicNode(LogicNode<D> original)
        {
            m_Result = original.m_Result; // Copy the configured result of the original LogicNode<D>.
            m_NextOnTrue = null; // But not a part of its evaluation chain.
            m_NextOnFalse = null; // So these both start fresh.
        }

        /// <summary>
        /// Return a shallow stand-alone copy of this LogicNode&lt;D&gt; with the same local behavior.
        /// </summary>
        /// <returns>A new node returning the same configured result but with no connections to other nodes.</returns>
        public ILogicNode<D> Clone()
        {
            return new LogicNode<D>(this);
        }

        /// <summary>
        /// Evaluate this ILogicNode&lt;D&gt; on a data set represented by a key of type D.
        /// </summary>
        /// <remarks>This node represents no field comparison and always returns the result passed in the constructor.</remarks>
        /// <param name="dataset">A dataset instance (or key to a logical dataset) to test for a match.</param>
        /// <returns>The boolean value configured in the constructor.</returns>
        public bool EvaluateNode(D dataset)
        {
            return m_Result;
        }

        /// <summary>
        /// Convert this LogicNode&lt;D&gt; to a string representation.
        /// </summary>
        /// <returns>A string representing the match configuration of this LogicNode&lt;D&gt;.</returns>
        public override string ToString()
        {
            return m_Result.ToString();
        }
    }

    /// <summary>
    /// Represents a logic node in a user-configured search query of dataset type D matching against field type F.
    /// </summary>
    /// <remarks>The dataset type D can be a data-encapsulation object or a key to select a specific dataset instance
    /// (eg. from a dictionary lookup or by array index).  The delegates configured in the constructor should know
    /// how to extract a specific data value (of type F) of interest and how to perform a standard comparison between
    /// a specific data value (of type F) and a configured field value (which may be non-specific).</remarks>
    /// <typeparam name="D">The type of the dataset (or of a key to a logical dataset) for this search query.</typeparam>
    /// <typeparam name="F">The type of data field value compared by this logic node.</typeparam>
    public class LogicNode<D,F> : LogicNode, ILogicNode<D>
    {
        private readonly GetDataValueDelegate<D,F> m_GetDataValue;
        private readonly ComparisonDelegate<F> m_Compare;
        private readonly MatchOperationType m_MatchOperation;
        private readonly F m_FieldA;
        private readonly F m_FieldB;

        private ILogicNode<D> m_NextOnTrue;
        private ILogicNode<D> m_NextOnFalse;

        /// <summary>
        /// The next ILogicNode&lt;D&gt; to evaluate if this node's evaluation is true, or null to conclude with true.
        /// </summary>
        public ILogicNode<D> NextOnTrue { get { return m_NextOnTrue; } set { m_NextOnTrue = value; } }
        
        /// <summary>
        /// The next ILogicNode&lt;D&gt; to evaluate if this node's evaluation is false, or null to conclude with false.
        /// </summary>
        public ILogicNode<D> NextOnFalse { get { return m_NextOnFalse; } set { m_NextOnFalse = value; } }

        /// <summary>
        /// Constructor for a LogicNode with a single configured field value.
        /// </summary>
        /// <param name="getDataValue">A GetDataDelegate&lt;D,F&gt; providing the field data of type F from data key type D.</param>
        /// <param name="comparer">A ComparisonDelegate&lt;F&gt; to compare fields of type F, such as IComparer&lt;F&gt;.Compare(F,F).</param>
        /// <param name="operationType">A MatchOperationType enum flags value defining the operation of this LogicNode.</param>
        /// <param name="fieldValue">The single configured field value which this logic node compares data against.</param>
        public LogicNode(GetDataValueDelegate<D,F> getDataValue, ComparisonDelegate<F> comparer,
                         MatchOperationType operationType, F fieldValue)
            : this(getDataValue, comparer, operationType, fieldValue, fieldValue)
        {
        }

        /// <summary>
        /// Constructor for a LogicNode with two configured field values.
        /// </summary>
        /// <param name="getDataValue">A GetDataDelegate&lt;D,F&gt; providing the field data of type F from data key type D.</param>
        /// <param name="comparer">A ComparisonDelegate&lt;F&gt; to compare fields of type F, such as IComparer&lt;F&gt;.Compare(F,F).</param>
        /// <param name="operationType">A MatchOperationType enum flags value defining the operation of this LogicNode.</param>
        /// <param name="fieldA">The first configured field value which this logic node compares data against.</param>
        /// <param name="fieldB">The second configured field value which this logic node compares data against.</param>
        public LogicNode(GetDataValueDelegate<D,F> getDataValue, ComparisonDelegate<F> comparer,
                         MatchOperationType operationType, F fieldA, F fieldB)
        {
            m_GetDataValue = getDataValue;
            m_Compare = comparer;
            m_MatchOperation = operationType;
            m_FieldA = fieldA;
            m_FieldB = fieldB;
            m_NextOnTrue = null;
            m_NextOnFalse = null;
        }

        /// <summary>
        /// Constructor for a LogicNode clone based on an original LogicNode&lt;D,F&gt;'s predicate.
        /// </summary>
        /// <param name="original">The original LogicNode&lt;D,F&gt; whose local predicate comparison to clone.</param>
        public LogicNode(LogicNode<D, F> original)
        {
            // Copy each of these from original, perform the same local predicate match.
            m_GetDataValue = original.m_GetDataValue;
            m_Compare = original.m_Compare;
            m_MatchOperation = original.m_MatchOperation;
            m_FieldA = original.m_FieldA;
            m_FieldB = original.m_FieldB;
            // But not part of that evaluate chain, so these start fresh.
            m_NextOnTrue = null;
            m_NextOnFalse = null;
        }

        /// <summary>
        /// Return a shallow stand-alone copy of this LogicNode&lt;D&gt; performing the same local predicate comparison.
        /// </summary>
        /// <returns>A new node performing the same predicate comparison but with no connections to other nodes.</returns>
        public ILogicNode<D> Clone()
        {
            return new LogicNode<D,F>(this);
        }

        /// <summary>
        /// Convert this LogicNode&lt;D,F&gt; to a string representation.
        /// </summary>
        /// <returns>A string representing the basic match operation being performed by this LogicNode&lt;D,F&gt;.</returns>
        public override string ToString()
        {
            if (m_FieldA.Equals(m_FieldB))
                return string.Format("{{{0}}} {1:F} {2}", typeof(F), m_MatchOperation, m_FieldA);
            else
                return string.Format("{{{0}}} {1:F} ({2}, {3})", typeof(F), m_MatchOperation, m_FieldA, m_FieldB);
        }

        /// <summary>
        /// Evaluate this ILogicNode&lt;D&gt; on a specific dataset instance.
        /// </summary>
        /// <remarks>The dataset parameter is merely passed to the configured GetDataDelegate, and so can also be a key
        /// from which the configured delegate knows how to find the desired data value for the indicated dataset instance
        /// represented by that key.</remarks>
        /// <param name="dataset">A dataset instance (or a key to a logical dataset) to test for a match.</param>
        /// <returns>True if the dataset instance matches on this LogicNode, false if it does not match.</returns>
        public bool EvaluateNode(D dataset)
        {
            F dataValue = m_GetDataValue(dataset); // Get the specific data value this LogicNode will test for the given key.
            int compareA = m_Compare(dataValue, m_FieldA); // Compare actual data value against configured FieldA value.
            bool matchA;

            if (compareA < 0)
            {
                matchA = (m_MatchOperation & MatchOperationType.LessThanFieldA) != 0; // Matching on less-than?
            }
            else if (compareA == 0)
            {
                matchA = (m_MatchOperation & MatchOperationType.EqualFieldA) != 0; // Matching on equals?
            }
            else
            {
                matchA = (m_MatchOperation & MatchOperationType.GreaterThanFieldA) != 0; // Matching on greater-than?
            }

            // Now see if we need to evaluate for FieldB or we can skip it.

            // Set matchB by default to the short-circuit value:  If we must match both, false, otherwise true.
            bool matchB = (m_MatchOperation & MatchOperationType.BothFieldsMatching) == 0; // Set to inverse of need-both.

            // Now see if matchA is the same as this short-circuit value.  If it is the same, we don't need to evaluate
            // FieldB.  So only do this section if matchA is the opposite of the short-circuit value in matchB.
            if (matchA != matchB)
            {
                // See if it can never match on FieldB or will always match on FieldB...
                if ((m_MatchOperation & MatchOperationType.FieldBMask) == 0)
                {
                    matchB = false;
                }
                else if ((m_MatchOperation & MatchOperationType.FieldBMask) == MatchOperationType.FieldBMask)
                {
                    matchB = true;
                }
                else
                {
                    // ...Nope.  We have to actually compare it.
                    int compareB;

                    // Notice that we compare equality of FieldA and FieldB with Equals, not with CompareTo because
                    // CompareTo may match (return 0) on wildcards implicit in the field, but we want to know if the
                    // two fields are configured to the same thing (ReferenceEquals() or have same CompareTo test).
                    // Note: We rely on Equals() to be more strict than CompareTo() returning 0 (when applicable).
                    if (m_FieldA.Equals(m_FieldB)) // These are the same if only one value was configured.
                    {
                        compareB = compareA; // Comparison would be the same, so just copy the result.
                    }
                    else
                    {
                        compareB = m_Compare(dataValue, m_FieldB); // Compare actual data value against configured FieldB value.
                    }

                    // Is FieldB a special-case endpoint? (Bug: I'm not sure this flag is implemented usefully and correct.)
                    if ((m_MatchOperation & MatchOperationType.ImplicitIncrementB) != 0 && compareB == 0)
                        compareB--; // Equality is considered LessThan in this special case.

                    if (compareB < 0)
                    {
                        matchB = (m_MatchOperation & MatchOperationType.LessThanFieldB) != 0; // Matching on less-than?
                    }
                    else if (compareB == 0)
                    {
                        matchB = (m_MatchOperation & MatchOperationType.EqualFieldB) != 0; // Matching on equals?
                    }
                    else
                    {
                        matchB = (m_MatchOperation & MatchOperationType.GreaterThanFieldB) != 0; // Matching on greater-than?
                    }
                }
            }

            // If it short-circuited evaluating a match on FieldB, then matchA is the same as matchB.
            // If it had to evaluate a match on FieldB, then matchB decides the overall match answer.
            // Either way, matchB holds the correct result, so return that.
            return matchB;
        }
    }
}

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
using System.Text;

namespace Gibraltar.Data
{
    /// <summary>
    /// Represents a structured expression for search query matches for dataset of type D.
    /// </summary>
    /// <typeparam name="D">The type of dataset (or of key to a logical dataset) targeted by this expression.</typeparam>
    public class LogicTreeExpression<D>
    {
        private readonly ILogicNode<D> m_RootNode;
        private readonly List<ILogicNode<D>> m_UnboundTrueList;
        private readonly List<ILogicNode<D>> m_UnboundFalseList;

        private readonly List<LogicTreeExpression<D>> m_SubExpressionList;
        private readonly MatchOperationType m_ExpressionOperation;

        internal ILogicNode<D> RootNode { get { return m_RootNode; } }

        internal ILogicNode<D>[] UnboundTrueList { get { return m_UnboundTrueList.ToArray(); } }

        internal ILogicNode<D>[] UnboundFalseList { get { return m_UnboundFalseList.ToArray(); } }

        internal MatchOperationType Operation { get { return m_ExpressionOperation; } }

        internal LogicTreeExpression<D>[] SubExpressionList { get { return m_SubExpressionList.ToArray(); } }

        /// <summary>
        /// Evaluate this LogicTreeExpression&lt;D&gt; on a given dataset instance.
        /// </summary>
        /// <param name="dataset">A dataset instance (or a key to a logical dataset) to test for a match.</param>
        public void EvaluateMatch(D dataset)
        {
            LogicTree<D>.EvaluateMatch(m_RootNode, dataset);
        }

        /// <summary>
        /// Construct an expression consisting of a single logic node.
        /// </summary>
        /// <param name="singleNode">A predicate ILogicNode&lt;D&gt; to be this expression.</param>
        public LogicTreeExpression(ILogicNode<D> singleNode)
        {
            m_RootNode = singleNode;
            ILogicNode<D>[] nodeArray = new[] { m_RootNode };
            m_UnboundTrueList = new List<ILogicNode<D>>(nodeArray);
            m_UnboundFalseList = new List<ILogicNode<D>>(nodeArray);
            m_SubExpressionList = new List<LogicTreeExpression<D>>(0);
            m_ExpressionOperation = MatchOperationType.Equals; // Special mark for a leaf-node expression.
        }

        /// <summary>
        /// Construct an expression as the negation of a single subexpression.
        /// </summary>
        /// <param name="negationExpression">The expression whose results to invert.</param>
        public LogicTreeExpression(LogicTreeExpression<D> negationExpression)
        {
            m_RootNode = negationExpression.RootNode;
            m_ExpressionOperation = MatchOperationType.NotEquals; // Special mark for a negation expression.
            m_SubExpressionList = new List<LogicTreeExpression<D>>(new[] { negationExpression });

            ILogicNode<D> never = new LogicNode<D>(false);
            ILogicNode<D> always = new LogicNode<D>(true);

            foreach (ILogicNode<D> unboundTrue in negationExpression.UnboundTrueList)
            {
                if (unboundTrue.NextOnTrue != null) // Sanity check.
                {
                    // The list of nodes with an unbound NextOnTrue contains a node whose NextOnTrue is bound!
                    throw new GibraltarException("UnboundTrueList contains a bound member.");
                }
                unboundTrue.NextOnTrue = never; // Bind all true results to a false to invert them.
            }
            foreach (ILogicNode<D> unboundFalse in negationExpression.UnboundFalseList)
            {
                if (unboundFalse.NextOnFalse != null) // Sanity check.
                {
                    // The list of nodes with an unbound NextOnFalse contains a node whose NextOnFalse is bound!
                    throw new GibraltarException("UnboundFalseList contains a bound member.");
                }
                unboundFalse.NextOnFalse = always; // Bind all false results to a true to invert them.
            }

            m_UnboundTrueList = new List<ILogicNode<D>>(new[] { always });
            m_UnboundFalseList = new List<ILogicNode<D>>(new[] { never });
        }

        /// <summary>
        /// Construct an expression as an AND or OR of two subexpressions.
        /// </summary>
        /// <param name="leftExpression">The left-hand subexpression.</param>
        /// <param name="operation">The logical operator (must be And or Or).</param>
        /// <param name="rightExpression">The right-hand subexpression.</param>
        public LogicTreeExpression(LogicTreeExpression<D> leftExpression, MatchOperationType operation,
                                   LogicTreeExpression<D> rightExpression)
            : this(operation, new []{leftExpression, rightExpression})
        {
            // Just redirect it to the array version, for convenience.
        }

        /// <summary>
        /// Construct an expression as a logical AND or OR of an array of subexpressions chained together.
        /// </summary>
        /// <param name="operation">The logical operator (must be And or Or).</param>
        /// <param name="expressionArray">The array of subexpressions to chain together with that operator.</param>
        public LogicTreeExpression(MatchOperationType operation, LogicTreeExpression<D>[] expressionArray)
        {
            if (expressionArray == null || expressionArray.Length == 0)
            {
                throw new GibraltarException("Illegal argument to LogicTreeExpression constructor: Null or empty expressionArray.");
            }

            m_SubExpressionList = new List<LogicTreeExpression<D>>(expressionArray);

            LogicTreeExpression<D> firstExpression = expressionArray[0];
            m_RootNode = firstExpression.RootNode; // Set our root node to start evaluation of the first subexpression.
            // Initialize our unbound lists from the first subexpression.
            m_UnboundTrueList = new List<ILogicNode<D>>(firstExpression.UnboundTrueList);
            m_UnboundFalseList = new List<ILogicNode<D>>(firstExpression.UnboundFalseList);

            if (operation == MatchOperationType.And)
            {
                m_ExpressionOperation = operation;
                for (int i = 1; i < expressionArray.Length; i++)
                {
                    LogicTreeExpression<D> subexpression = expressionArray[i];
                    ILogicNode<D> chainToNode = subexpression.RootNode;

                    // In an AND operation, any true result from the left subexpression requires the evaluation of the right.
                    // But any false result from the left subexpression results in a false result for the outer expression. So
                    // we need to go through the unbound NextOnTrue list for the previous expression and bind them to the new one.
                    foreach (ILogicNode<D> unboundTrue in m_UnboundTrueList)
                    {
                        if (unboundTrue.NextOnTrue != null) // Sanity check.
                        {
                            // The list of nodes with an unbound NextOnTrue contains a node whose NextOnTrue is bound!
                            throw new GibraltarException("UnboundTrueList contains a bound member.");
                        }
                        unboundTrue.NextOnTrue = chainToNode;
                    }

                    // We bound everything in the current UnboundTrueList, so copy it from the new subexpression.
                    m_UnboundTrueList = new List<ILogicNode<D>>(subexpression.UnboundTrueList);
                    // The UnboundFalseList is cumulative for AND operations.
                    m_UnboundFalseList.AddRange(subexpression.UnboundFalseList);
                }
            }
            else if (operation == MatchOperationType.Or)
            {
                m_ExpressionOperation = operation;
                for (int i = 1; i < expressionArray.Length; i++)
                {
                    LogicTreeExpression<D> subexpression = expressionArray[i];
                    ILogicNode<D> chainToNode = subexpression.RootNode;

                    // In an OR operation, any false result from the left subexpression requires the evaluation of the right.
                    // But any true result from the left subexpression results in a true result for the outer expression. So
                    // we need to go through the unbound NextOnFalse list for leftExpression and bind them to rightExpression.
                    foreach (ILogicNode<D> unboundFalse in m_UnboundFalseList)
                    {
                        if (unboundFalse.NextOnFalse != null) // Sanity check.
                        {
                            // The list of nodes with an unbound NextOnFalse contains a node whose NextOnFalse is bound!
                            throw new GibraltarException("UnboundFalseList contains a bound member.");
                        }
                        unboundFalse.NextOnFalse = chainToNode;
                    }

                    // We bound everything in the current UnboundFalseList, so copy it from the new subexpression.
                    m_UnboundFalseList = new List<ILogicNode<D>>(subexpression.UnboundFalseList);
                    // The UnboundTrueList is cumulative for OR operations.
                    m_UnboundTrueList.AddRange(subexpression.UnboundTrueList);
                }
            }
            else
            {
                throw new GibraltarException(string.Format("Invalid operation {0:X4} ({0:F}) not supported by LogicTreeExpression.",
                                                           operation));
            }
        }

        /// <summary>
        /// Convert this (nested) LogicTreeExpression&lt;D&gt; to a string representation.
        /// </summary>
        /// <returns>A string representing the compound expression of this (nested) LogicTreeExpression&lt;D&gt;</returns>
        public override string ToString()
        {
            string operatorString;
            string leftParen;
            string rightParen;
            if (m_ExpressionOperation == MatchOperationType.Equals)
            {
                if (m_SubExpressionList.Count == 0)
                    return m_RootNode.ToString();
                else
                    return "INVALID LogicTreeExpression SubExpressionList.Count for leaf node";
            }

            if (m_ExpressionOperation == MatchOperationType.NotEquals)
            {
                if (m_SubExpressionList.Count == 1)
                    return "NOT " + m_SubExpressionList[0].ToString();
                else
                    return "INVALID LogicTreeExpression SubExpressionList.Count for NOT operator";
            }

            if (m_ExpressionOperation == MatchOperationType.And)
            {
                operatorString = "AND";
                leftParen = "(";
                rightParen = ")";
            }
            else if (m_ExpressionOperation == MatchOperationType.Or)
            {
                operatorString = "OR";
                leftParen = "[";
                rightParen = "]";
            }
            else
            {
                return "INVALID LogicTreeExpression OPERATION";
            }

            StringBuilder builder = new StringBuilder(leftParen);
            LogicTreeExpression<D>[] subexpressionArray = SubExpressionList;

            if (subexpressionArray.Length > 0)
                builder.Append(subexpressionArray[0].ToString());

            for (int i=1; i<subexpressionArray.Length; i++)
            {
                builder.AppendFormat(" {0} {1}", operatorString, subexpressionArray[i]);
            }

            builder.Append(rightParen);

            return builder.ToString();
        }
    }
}

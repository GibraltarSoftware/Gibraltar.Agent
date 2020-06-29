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

#endregion


namespace Gibraltar.Monitor
{
    /// <summary>
    /// hierarchal text parsing for dot-delimited strings
    /// </summary>
    public static class TextParse
    {
        /// <summary>
        /// Splits a dot-delimited category name, removing blank entries as well as - and _ characters
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public static string[] CategoryName(string categoryName)
        {
            return SplitStringWithTrim(categoryName, null, new char[] {'-', '_', ' '});
        }

        /// <summary>
        /// Splits a dot-delimited class name
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string[] ClassName(string className)
        {
            return SplitStringWithTrim(className);
        }

        /// <summary>
        /// Split string on separators, trimming each and eliminating empty strings.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="separators"></param>
        /// <param name="additionalIgnoreCharacters"></param>
        /// <returns></returns>
        public static string[] SplitStringWithTrim(string source, char[] separators = null, char[] additionalIgnoreCharacters = null)
        {
            if (source == null)
                return new string[ 0 ];

            if (separators == null)
                separators = new char[] {'.'};

            string[] sourceFragments = source.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            //now double check its perspective on empty entries:  Remove any that are empty when trimmed.
            int validFragmentsCount = sourceFragments.Length; //by default all our valid names

            for (int curFragmentIndex = 0; curFragmentIndex < validFragmentsCount; curFragmentIndex++)
            {
                //Clean up this value, removing redundant demarcation characters and leading/training whitespace
                string curCategoryName = sourceFragments[curFragmentIndex].Trim();

                if (additionalIgnoreCharacters != null)
                    curCategoryName = curCategoryName.Trim(additionalIgnoreCharacters);

                //and if it is still not null or empty, just set it back. Otherwise, we need to remove this string.
                if (string.IsNullOrEmpty(curCategoryName))
                {
                    //move every remaining string up one to fill in the gap.
                    for (int futureCategoryNameIndex = curFragmentIndex + 1; futureCategoryNameIndex < sourceFragments.Length; futureCategoryNameIndex++)
                    {
                        sourceFragments[futureCategoryNameIndex - 1] = sourceFragments[futureCategoryNameIndex];
                    }

                    //and the last one is now null, because we moved everything up.
                    sourceFragments[sourceFragments.Length - 1] = null;
                    //we have one less valid name..
                    validFragmentsCount--;
                    //but we just moved something up into *our* spot so we need to recheck our current position.
                    curFragmentIndex--;
                }
                else
                {
                    sourceFragments[curFragmentIndex] = curCategoryName;
                }
            }

            //do we need to shorten the array before we return it?  
            if (validFragmentsCount < sourceFragments.Length)
            {
                //yes, we must have found one or more names to be empty so we need to trim them.
                string[] remainingFragments = new string[ validFragmentsCount ];

                //I admit it, this looks silly - but I couldn't find a built in way of getting a limited # of the 
                //array elements without doing my own iteration, so here it is.
                for (int curCategoryNameIndex = 0; curCategoryNameIndex < remainingFragments.Length; curCategoryNameIndex++)
                {
                    remainingFragments[curCategoryNameIndex] = sourceFragments[curCategoryNameIndex];
                }

                return remainingFragments;
            }
            else
            {
                //return the array as is - it's just fine.
                return sourceFragments;
            }
        }
    }
}

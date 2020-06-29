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

namespace Gibraltar.Data
{
    /// <summary>
    /// Provides field manipulator tools
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Field<T>
    {
        /// <summary>
        /// Updates the specified value with the new value if they're different.
        /// </summary>
        /// <returns>True if the value is different and was updated, false otherwise.</returns>
        public static bool SetValue(ref T originalValue, T newValue, IEqualityComparer<T> equalityComparer = null)
        {
            bool ignoreValue = false;
            return SetValue(ref originalValue, newValue, ref ignoreValue, equalityComparer);
        }
        
        /// <summary>
        /// Updates the specified value with the new value if they're different.
        /// </summary>
        /// <returns>True if the value is different and was updated, false otherwise.</returns>
        public static bool SetValue(object syncObject, ref T originalValue, T newValue, IEqualityComparer<T> equalityComparer = null)
        {
            lock (syncObject)
            {
                return SetValue(ref originalValue, newValue, equalityComparer);
            }
        }

        /// <summary>
        /// Updates the specified value with the new value if they're different, setting the modification tracking flag to true only if changed.
        /// </summary>
        /// <returns>True if the value is different and was updated, false otherwise.</returns>
        /// <remarks>The modification tracking value is set to true if changed and left unchanged if there is no change.</remarks>
        public static bool SetValue(object syncObject, ref T originalValue, T newValue, ref bool modificationTracking, IEqualityComparer<T> equalityComparer = null)
        {
            lock (syncObject)
            {
                return SetValue(ref originalValue, newValue, ref modificationTracking, equalityComparer);
            }
        }

        /// <summary>
        /// Updates the specified value with the new value if they're different, setting the modification tracking flag to true only if changed.
        /// </summary>
        /// <returns>True if the value is different and was updated, false otherwise.</returns>
        /// <remarks>The modification tracking value is set to true if changed and left unchanged if there is no change.</remarks>
        public static bool SetValue(ref T originalValue, T newValue, ref bool modificationTracking, IEqualityComparer<T> equalityComparer = null)
        {
            //handle null/reference scenarios
            if (ReferenceEquals(originalValue, newValue))
                return false;

            bool modified = false;

            //check if the original value is null (if both null we already returned)
            if (ReferenceEquals(originalValue, null))
            {
                originalValue = newValue;
                modificationTracking = true;
                modified = true;
            }
            else
            {
                //original value is not null...  so we can do a type-specific equals.
                bool isEqual;
                if (equalityComparer == null)
                {
                    isEqual = originalValue.Equals(newValue);
                }
                else
                {
                    isEqual = equalityComparer.Equals(originalValue, newValue);
                }
                if (!isEqual)
                {
                    originalValue = newValue;
                    modificationTracking = true;
                    modified = true;
                }
            }

            return modified;
        }
    }

    /// <summary>
    /// Provides more precise equality comparison for date time offsets.
    /// </summary>
    public class DateTimeOffsetEqualityComparer : IEqualityComparer<DateTimeOffset>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(DateTimeOffset x, DateTimeOffset y)
        {
            return x.EqualsExact(y);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        public int GetHashCode(DateTimeOffset obj)
        {
            return obj.GetHashCode();
        }
    }
}
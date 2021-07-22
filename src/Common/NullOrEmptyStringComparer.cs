
using System;
using System.Collections.Generic;
using System.Text;

namespace Gibraltar
{
    /// <summary>
    /// Performs string comparison where null and empty are coalesced together.
    /// </summary>
    public class NullOrEmptyStringComparer : EqualityComparer<string>
    {
        private readonly StringComparer m_BaseComparer;

        private NullOrEmptyStringComparer(StringComparer baseComparer)
        {
            m_BaseComparer = baseComparer;
        }

        /// <summary>
        /// When overridden in a derived class, determines whether two objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public override bool Equals(string x, string y)
        {
            //our whole goal is to treat null and empty as the same...
            if (string.IsNullOrEmpty(x) && string.IsNullOrEmpty(y))
                return true;

            return m_BaseComparer.Equals(x, y);
        }

        /// <summary>
        /// When overridden in a derived class, serves as a hash function for the specified object for hashing algorithms and data structures, such as a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The object for which to get a hash code.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public override int GetHashCode(string obj)
        {
            return m_BaseComparer.GetHashCode(obj);
        }

        /// <summary>
        /// Perform ordinal, case-insensitive comparison
        /// </summary>
        public static IEqualityComparer<string> OrdinalIgnoreCase { get { return new NullOrEmptyStringComparer(StringComparer.OrdinalIgnoreCase); } }
    }
}

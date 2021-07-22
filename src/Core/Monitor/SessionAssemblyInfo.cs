
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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Gibraltar.Monitor.Internal;

#endregion File Header

namespace Gibraltar.Monitor
{
    /// <summary>
    /// Detail about .NET assemblies.
    /// </summary>
    public class SessionAssemblyInfo : IDisplayable, IComparable<SessionAssemblyInfo>, IEquatable<SessionAssemblyInfo>
    {
        private readonly AssemblyInfoPacket m_Packet;

        /// <summary>
        /// Create a new assembly information object with information from the provided assembly.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="includeLocation">Whether to include the assembly's full path location.</param>
        public SessionAssemblyInfo(Assembly target, bool includeLocation)
        {
            m_Packet = new AssemblyInfoPacket();
            m_Packet.FullName = target.FullName;
            m_Packet.GlobalAssemblyCache = target.GlobalAssemblyCache;
            m_Packet.ImageRuntimeVersion = target.ImageRuntimeVersion;

            // Location path could contain user identity, so suppress it in privacy mode.  (Or should we test it?)
            if (includeLocation && !(target.ManifestModule is System.Reflection.Emit.ModuleBuilder))
            {
                //location can be iffy.
                try
                {
                    m_Packet.Location = target.CodeBase;
                }
                    // ReSharper disable EmptyGeneralCatchClause
                catch
                    // ReSharper restore EmptyGeneralCatchClause
                {
                }
            }

            AssemblyName targetName = target.GetName();
            m_Packet.Name = targetName.Name;
            m_Packet.ProcessorArchitecture = targetName.ProcessorArchitecture; // Note: Could be null (or meaningless) under Mono?
            m_Packet.CultureName = targetName.CultureInfo.Name;
            m_Packet.Version = targetName.Version.ToString();

            //and now try to get the file version.  This is risky.
            try
            {
                //use the new get custom attributes static function to avoid problems with getting
                //types loaded via ReflectionOnlyGetType
                IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(target);
                foreach (CustomAttributeData attribute in attributes)
                {
                    //we need to find the assembly file version attribute, if there is one..
                    if (attribute.Constructor.ReflectedType == typeof(AssemblyFileVersionAttribute))
                    {
                        m_Packet.FileVersion = attribute.ConstructorArguments[0].Value as string;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorNotifier.Notify(this, ex);
            }
        }

        internal SessionAssemblyInfo(AssemblyInfoPacket packet)
        {
            m_Packet = packet;
        }

        #region Public Properties and Methods

        /// <summary>
        /// A display caption for the assembly.
        /// </summary>
        public string Caption
        {
            get
            {
                return m_Packet.Name;
            }
        }

        /// <summary>
        /// The full name of the assembly.
        /// </summary>
        public string Description
        {
            get
            {
                return m_Packet.FullName;
            }
        }

        /// <summary>
        /// The unique Id of this assembly information within the session.
        /// </summary>
        public Guid Id { get { return m_Packet.ID; } }

        /// <summary>
        /// The standard full name for the culture (like EN-US)
        /// </summary>
        public string CultureName { get { return m_Packet.CultureName; } }

        /// <summary>
        /// The full name for the assembly, generally unique within an application domain.
        /// </summary>
        public string FullName { get { return m_Packet.FullName; } }

        /// <summary>
        /// The .NET Runtime version the assembly image was compiled against.
        /// </summary>
        public string ImageRuntimeVersion { get { return m_Packet.ImageRuntimeVersion; } }

        /// <summary>
        /// The full location to the assembly.
        /// </summary>
        public string Location { get { return m_Packet.Location; } }

        /// <summary>
        /// The short name of the assembly (typically the same as the file name without extension).  Not unique within an application domain.
        /// </summary>
        public string Name { get { return m_Packet.Name; } }

        /// <summary>
        /// The processor architecture the assembly was compiled for.
        /// </summary>
        public ProcessorArchitecture ProcessorArchitecture { get { return m_Packet.ProcessorArchitecture; } }

        /// <summary>
        /// Indicates of the assembly was loaded out of the Global Assembly Cache.
        /// </summary>
        public bool GlobalAssemblyCache { get { return m_Packet.GlobalAssemblyCache; } }

        /// <summary>
        /// The Assembly Version that was loaded.
        /// </summary>
        public string Version { get { return m_Packet.Version; } }

        /// <summary>
        /// The file version recorded in the manifest assembly, if available.  May be null.
        /// </summary>
        public string FileVersion { get { return m_Packet.FileVersion; } }

        /// <summary>
        /// The date &amp; time the assembly was loaded by the runtime.
        /// </summary>
        public DateTimeOffset LoadedTimeStamp { get { return m_Packet.Timestamp; } }

        #endregion

        #region Internal Properties and Methods

        internal AssemblyInfoPacket Packet { get { return m_Packet; } }

        #endregion

        #region IComparable and IEquatable Methods

        /// <summary>
        /// Compares this SessionAssemblyInfo object to another to determine sorting order.
        /// </summary>
        /// <remarks>SessionAssemblyInfo instances are sorted by their ThreadId property.</remarks>
        /// <param name="other">The other SessionAssemblyInfo object to compare this object to.</param>
        /// <returns>An int which is less than zero, equal to zero, or greater than zero to reflect whether
        /// this SessionAssemblyInfo should sort as being less-than, equal to, or greater-than the other
        /// SessionAssemblyInfo, respectively.</returns>
        public int CompareTo(SessionAssemblyInfo other)
        {
            //we compare based on ThreadId.
            return Name.CompareTo(other.Name);
        }

        /// <summary>
        /// Determines if the provided SessionAssemblyInfo object is identical to this object.
        /// </summary>
        /// <param name="other">The SessionAssemblyInfo object to compare this object to</param>
        /// <returns>True if the objects represent the same data.</returns>
        public bool Equals(SessionAssemblyInfo other)
        {
            // Careful, it could be null; check it without recursion
            if (ReferenceEquals(other, null))
            {
                return false; // Since we're a live object we can't be equal to a null instance.
            }

            //they are the same if they have the same FullName
            return (FullName == other.FullName);
        }

        /// <summary>
        /// Determines if the provided object is identical to this object.
        /// </summary>
        /// <param name="obj">The object to compare this object to</param>
        /// <returns>True if the other object is also a SessionAssemblyInfo and represents the same data.</returns>
        public override bool Equals(object obj)
        {
            SessionAssemblyInfo otherSessionAssemblyInfo = obj as SessionAssemblyInfo;

            return Equals(otherSessionAssemblyInfo); // Just have type-specific Equals do the check (it even handles null)
        }

        /// <summary>
        /// Provides a representative hash code for objects of this type to spread out distribution
        /// in hash tables.
        /// </summary>
        /// <remarks>Objects which consider themselves to be Equal (a.Equals(b) returns true) are
        /// expected to have the same hash code.  Objects which are not Equal may have the same
        /// hash code, but minimizing such overlaps helps with efficient operation of hash tables.
        /// </remarks>
        /// <returns>
        /// An int representing the hash code calculated for the contents of this object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_Packet.GetHashCode();
        }

        /// <summary>
        /// Compares two SessionAssemblyInfo instances for equality.
        /// </summary>
        /// <param name="left">The SessionAssemblyInfo to the left of the operator</param>
        /// <param name="right">The SessionAssemblyInfo to the right of the operator</param>
        /// <returns>True if the two AssemblyInfos are equal.</returns>
        public static bool operator ==(SessionAssemblyInfo left, SessionAssemblyInfo right)
        {
            // We have to check if left is null (right can be checked by Equals itself)
            if (ReferenceEquals(left, null))
            {
                // If right is also null, we're equal; otherwise, we're unequal!
                return ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two SessionAssemblyInfo instances for inequality.
        /// </summary>
        /// <param name="left">The SessionAssemblyInfo to the left of the operator</param>
        /// <param name="right">The SessionAssemblyInfo to the right of the operator</param>
        /// <returns>True if the two AssemblyInfos are not equal.</returns>
        public static bool operator !=(SessionAssemblyInfo left, SessionAssemblyInfo right)
        {
            // We have to check if left is null (right can be checked by Equals itself)
            if (ReferenceEquals(left, null))
            {
                // If right is also null, we're equal; otherwise, we're unequal!
                return !ReferenceEquals(right, null);
            }
            return !left.Equals(right);
        }

        /// <summary>
        /// Compares if one SessionAssemblyInfo instance should sort less than another.
        /// </summary>
        /// <param name="left">The SessionAssemblyInfo to the left of the operator</param>
        /// <param name="right">The SessionAssemblyInfo to the right of the operator</param>
        /// <returns>True if the SessionAssemblyInfo to the left should sort less than the SessionAssemblyInfo to the right.</returns>
        public static bool operator <(SessionAssemblyInfo left, SessionAssemblyInfo right)
        {
            return (left.CompareTo(right) < 0);
        }

        /// <summary>
        /// Compares if one SessionAssemblyInfo instance should sort greater than another.
        /// </summary>
        /// <param name="left">The SessionAssemblyInfo to the left of the operator</param>
        /// <param name="right">The SessionAssemblyInfo to the right of the operator</param>
        /// <returns>True if the SessionAssemblyInfo to the left should sort greater than the SessionAssemblyInfo to the right.</returns>
        public static bool operator >(SessionAssemblyInfo left, SessionAssemblyInfo right)
        {
            return (left.CompareTo(right) > 0);
        }

        #endregion
    }
}


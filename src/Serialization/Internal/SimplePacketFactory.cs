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
using System.Collections.Generic;
using System.Reflection;
using Gibraltar.Serialization;

#endregion File Header

namespace Gibraltar.Serialization.Internal
{
    /// <summary>
    /// SimplePacketFactory is the IPacketFactory used when an IPacket
    /// implementation knows how to to use when a type
    /// </summary>
    internal class SimplePacketFactory : IPacketFactory
    {
        private readonly ConstructorInfo m_Constructor;
        private readonly List<MethodInfo> m_ReadMethods;

        /// <summary>
        /// Creates an IPacketFactory wrappering a type tht implements IPacket.
        /// </summary>
        /// <param name="type">The type must implement IPacket and provide a default constructor</param>
        public SimplePacketFactory(Type type)
        {
            if (!(typeof(IPacket).IsAssignableFrom(type)))
            {
                ErrorNotifier.Notify(this,
                                     new ArgumentException(
                                         "SimplePacketFactory constructor requires a type that implements IPacket"));
                return;
            }

            const BindingFlags flags = BindingFlags.DeclaredOnly |
                                       BindingFlags.Instance |
                                       BindingFlags.InvokeMethod |
                                       BindingFlags.Public | BindingFlags.NonPublic;

            // the type must provide a default constructor, but this constructor can be
            // private if it should not be called directly (other than during deserialization)
            m_Constructor = type.GetConstructor(flags, null, new Type[0], null);
            if (m_Constructor == null)
            {
                ErrorNotifier.Notify(this,
                                     new ArgumentException(
                                         "SimplePacketFactory is only compatible with IPacket objects that have a default constructor"));
                return;
            }

            m_ReadMethods = new List<MethodInfo>();

            // walk down the hierarchy till we get to a base object that no longer implements IPacket
            while (typeof(IPacket).IsAssignableFrom(type))
            {
                // Even though the current type implements IPacket, it may not have a GetPacketDefinition at this level
                MethodInfo method = type.GetMethod("ReadFields", flags, null,
                                                   new Type[] {typeof(PacketDefinition), typeof(IFieldReader)},
                                                   new ParameterModifier[0]);
                m_ReadMethods.Add(method);
                type = type.BaseType;
            }
        }

        /// <summary>
        /// This method is used by caller to detect if the constructor failed.
        /// This is necessary because we suppress exceptions in release builds.
        /// </summary>
        public bool IsValid
        {
            get { return m_Constructor != null && m_ReadMethods != null; }
        }

        #region IPacketFactory Members

        /// <summary>
        /// This is the method that is invoked on an IPacketFactory to create an IPacket
        /// from the data in an IFieldReader given a specified PacketDefinition.
        /// </summary>
        /// <param name="definition">Definition of the fields expected in the next packet</param>
        /// <param name="reader">Data stream to be read</param>
        /// <returns>An IPacket corresponding to the PacketDefinition and the stream data</returns>
        public IPacket CreatePacket(PacketDefinition definition, IFieldReader reader)
        {
            IPacket packet = (IPacket)m_Constructor.Invoke(new object[0]);
            definition.ReadFields(packet, reader);
            return packet;
        }

        #endregion
    }
}

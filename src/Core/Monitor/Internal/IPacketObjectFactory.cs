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
namespace Gibraltar.Monitor.Internal
{
    /// <summary>
    /// Implement to support derived object creation from serialized packets
    /// </summary>
    /// <remarks>
    /// Some objects, such as metrics, have abstract base classes that need to be derived from to create useful
    /// features.  To support third party developers deriving new objects, this interface is used to allow a
    /// raw persistable packet to specify the correct derived type of its associated data object.
    /// </remarks>
    /// <typeparam name="DataObjectType">The base object</typeparam>
    /// <typeparam name="ParentObjectType">The base type of object that collects this base object</typeparam>
    internal interface IPacketObjectFactory<DataObjectType, ParentObjectType>
    {
        /// <summary>
        /// Called to create the wrapping data object for a packet object.
        /// </summary>
        /// <remarks>
        /// For collected objects, the parent collection owner is provided in the optional parent section.  Review
        /// specific usage documentation to know which format of this interface to implement for a given base data object.
        /// For example, when overriding MetricPacket you will have to implement one form, for MetricSamplePacket a different one.</remarks>
        /// <param name="optionalParent">The object that will own the newly created data object</param>
        /// <returns></returns>
        DataObjectType GetDataObject(ParentObjectType optionalParent);
    }
}

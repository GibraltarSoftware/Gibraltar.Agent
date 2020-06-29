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


#endregion File Header

namespace Gibraltar.Serialization
{
    /// <remarks>
    /// Most packets have a static structure of fields that is the same for all
    /// packet instances.  But some packets are dynamic in that the number and
    /// type of fields can vary across different packet instances.  A great 
    /// example of this is EventMetricDefinitionPacket.  Each event metric
    /// has a different set of fields.  So, in terms of caching PacketDefinition
    /// objects, each instance can be thought of as a dynamic type.  On the
    /// other hand, only a single PacketFactory need be registered that should
    /// be invoked for all dynamic packets of that base type.
    /// </remarks>
    public interface IDynamicPacket : IPacket
    {
        /// <summary>
        /// The consistent, unique type name for the packet
        /// </summary>
        string DynamicTypeName { get; set; }
    }
}
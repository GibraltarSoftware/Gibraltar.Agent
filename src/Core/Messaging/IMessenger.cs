
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

#endregion File Header


namespace Gibraltar.Messaging
{
    /// <summary>
    /// Implement this interface to be a packet sink for the messaging system. 
    /// </summary>
    internal interface IMessenger : IEquatable<IMessenger>, IDisposable
    {
        /// <summary>
        /// A display caption for this messenger
        /// </summary>
        /// <remarks>End-user display caption for this messenger.  Captions are typically
        /// not unique to a given instance of a messenger.</remarks>
        string Caption { get; }

        /// <summary>
        /// A display description for this messenger
        /// </summary>
        /// <remarks></remarks>
        string Description { get; }

        /// <summary>
        /// Called by the publisher every time the configuration has been updated.
        /// </summary>
        /// <param name="configuration">The configuration block for this messenger</param>
        void ConfigurationUpdated(MessengerConfiguration configuration);

        /// <summary>
        /// Initialize the messenger so it is ready to accept packets.
        /// </summary>
        /// <param name="publisher">The publisher that owns the messenger</param>
        /// <param name="configuration">The configuration block for this messenger</param>
        void Initialize(Publisher publisher, MessengerConfiguration configuration);

        /// <summary>
        /// A name for this messenger
        /// </summary>
        /// <remarks>The name is unique and specified by the publisher during initialization.</remarks>
        string Name { get; }

        /// <summary>
        /// Write the provided packet to this messenger.
        /// </summary>
        /// <remarks>The packet may depend on other packets.  If the messenger needs those packets they are available from the publisher's packet cache.</remarks>
        /// <param name="packet">The packet to write through the messenger.</param>
        /// <param name="writeThrough">True if the information contained in packet should be committed synchronously, false if the messenger should use write caching (if available).</param>
        void Write(IMessengerPacket packet, bool writeThrough);
    }
}

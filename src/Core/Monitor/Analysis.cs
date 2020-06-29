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
using Gibraltar.Monitor.Internal;

#endregion

#pragma warning disable 1591
namespace Gibraltar.Monitor
{
    public class Analysis : IDisplayable
    {
        private readonly AnalysisPacket m_Packet;
        private readonly MarkerCollection m_Markers;
        private readonly DataExtensionCollection m_DataExtensions;

        public Analysis()
        {
            m_Packet = new AnalysisPacket();
            m_Packet.Caption = string.Empty;
            m_Packet.Description = string.Empty;

            m_Markers = new MarkerCollection(this);
            m_DataExtensions = new DataExtensionCollection(this);
        }

        #region Public Properties and Methods

        public DataExtensionCollection DataExtensions
        {
            get { return m_DataExtensions; }
        }

        public MarkerCollection Markers
        {
            get { return m_Markers; }
        }

        /// <summary>
        /// The short display caption for this comment (entered by the user)
        /// </summary>
        public string Caption
        {
            get
            {
                return m_Packet.Caption;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    m_Packet.Caption = value;
                }
                else
                {
                    m_Packet.Caption = value.Trim();
                }
            }
        }

        /// <summary>
        /// The body of the comment
        /// </summary>
        public string Description
        {
            get
            {
                return m_Packet.Description;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    m_Packet.Description = value;
                }
                else
                {
                    m_Packet.Description = value.Trim();
                }
            }
        }

        #endregion

        /*
        private void ReadVersion1(SerializationReader reader)
        {
            Trace.TraceInformation("Reading analysis information from serialization reader.");

            //find out how many packets we wrote out so we can read exactly that many packets.
            int packetCount = reader.ReadInt32();
            int packetsRead = 0;

            //then convert to the packet reader for ease of use
            PacketReader packetReader = new PacketReader(reader);
            CommentCollection newComments = null;  //this is deliberately null
            while (packetsRead < packetCount)
            {
                object nextObject;

                //read the packet; we don't need to check at this point if it worked.
                packetReader.ReadNext(out nextObject);
                
                //increment our packets read counter since we read one.
                packetsRead++;
                try
                {
                    if ( nextObject is Marker )
                    {
                        //Marker is a simple enough type it doesn't need the whole packet abstraction.
                        Marker newMarker = (Marker)nextObject;
                        m_Markers.Add(newMarker);
                    }
                    else if (nextObject is Comment)
                    {
                        //we're working on a comment collection - we collect all of the comments for a data extension object
                        //before we make the data extension object
                        Comment newComment = (Comment)nextObject;

                        //do we have an existing comments collection we're working on?
                        if (newComments == null)
                        {
                            //make a new comments collection, we must have used up the last one (or this is the first)
                            newComments = new CommentCollection();
                        }

                        //add this comment to our collection.
                        newComments.Add(newComment);
                    }
                    else if (nextObject is DataExtensionPacket)
                    {
                        //we have to do a dance to get our data extension object
                        DataExtensionPacket newDataExtensionPacket = (DataExtensionPacket)nextObject;

                        //and now initialize a new data extension object from the packet and whatever collection we have
                        DataExtension newDataExtension = new DataExtension(this, newDataExtensionPacket, newComments);
                        m_DataExtensions.Add(newDataExtension);

                        //be sure we clear our comments collection since we've now used it
                        newComments = null;
                    }
                    else if (nextObject == null)
                    {
                        Trace.TraceError("Fewer packets than expected while reading analysis; only read {0} out of {1}", packetsRead, packetCount);
                    }
                    else
                    {
                        Trace.TraceWarning("Unexpected Object: " + nextObject);
                    }
                }
                catch ( Exception ex )
                {
                    Trace.TraceError("While deserializing the {0} object, the following exception was thrown: {1}", nextObject, ex);
                }
            }

            Trace.TraceInformation("Analysis deserialization completed.");
        }

        public override void Serialize(SerializationWriter writer)
        {
            //now we have to write out all of our objects 

            //First, we need to count them all - we have to write out the count first before
            //we write out the objects or the read-back will fail.
            int packetCount = 0, packetsWritten = 0;

            packetCount += m_Markers.Count;
            packetCount += m_DataExtensions.Count;
            foreach (DataExtension curExtension in m_DataExtensions)
            {
                packetCount += curExtension.Comments.Count;
            }

            //write out the packet count
            writer.Write(packetCount);

            //write out all of our markers
            foreach (Marker curMarker in m_Markers)
            {
                writer.Write(curMarker);
                packetsWritten++;
            }

            //Writing out data extensions is more difficult - we need to do a deep iteration, depth first.
            foreach (DataExtension curExtension in m_DataExtensions)
            {
                //we have to write out the comments first, then the data extension object
                foreach (Comment curComment in curExtension.Comments)
                {
                    //write out each comment.
                    writer.Write(curComment);
                    packetsWritten++;
                }

                //BUGBUG:  We should write out data markers here.

                //and finally write out a data extension packet for this extension
                writer.Write(curExtension.Packet);
                packetsWritten++;
            }

            //now double-check that what we wrote out agrees with what we predicted.  This is mostly a 
            //maintenance/programming error check.
            Debug.Assert((packetsWritten == packetCount), "The number of packets written isn't the same as the number we predicted, which will produce a corrupt file.");
        }*/
    }
}

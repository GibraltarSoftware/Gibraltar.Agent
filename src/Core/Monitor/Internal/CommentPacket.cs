
using System;
using System.Diagnostics;
using System.Reflection;
using Gibraltar.Serialization;

namespace Gibraltar.Monitor.Internal
{
    internal class CommentPacket : GibraltarPacket, IPacket, IEquatable<CommentPacket>
    {
        private Guid m_ID;
        private string m_Caption;
        private string m_Description;
        private string m_UserName;

        #region Public Properties and Methods

        /// <summary>
        /// Create a new comment packet for deserialization
        /// </summary>
        public CommentPacket()
        {
            
        }

        /// <summary>
        /// Create a new comment packet for serialization
        /// </summary>
        /// <param name="extensionPacket">The extension packet that owns this comment packet</param>
        public CommentPacket(DataExtensionPacket extensionPacket)
        {
            DataExtensionPacket = extensionPacket;
        }

        /// <summary>
        /// The data extension packet that owns this packet.  Required to serialize a packet.
        /// </summary>
        public DataExtensionPacket DataExtensionPacket { get; set; }


        /// <summary>
        /// The unique Id of this comment
        /// </summary>
        public Guid Id { get { return m_ID; } set { m_ID = value; } }


        /// <summary>
        /// The short display caption for this comment (entered by the user)
        /// </summary>
        public string Caption { get { return m_Caption; } set { m_Caption = value; } }


        /// <summary>
        /// The body of the comment
        /// </summary>
        public string Description { get { return m_Description; } set { m_Description = value; } }


        /// <summary>
        /// The fully qualified user name (DOMAIN\USER) of the user that created the comment.
        /// </summary>
        public string UserName { get { return m_UserName; } set { m_UserName = value; } }

        #endregion

        #region IPacket Members

        private const int SerializationVersion = 1;

        /// <summary>
        /// The list of packets that this packet depends on.
        /// </summary>
        /// <returns>An array of IPackets, or null if there are no dependencies.</returns>
        IPacket[] IPacket.GetRequiredPackets()
        {
            //to deserialize a comment you need to have an extension packet.
            Debug.Assert(DataExtensionPacket != null);
            return new IPacket[] {DataExtensionPacket};
        }

        PacketDefinition IPacket.GetPacketDefinition()
        {
            string typeName = MethodBase.GetCurrentMethod().DeclaringType.Name;
            PacketDefinition definition = new PacketDefinition(typeName, SerializationVersion, true);
            //we explicitly have ID because we are not a cached packet.
            definition.Fields.Add("ID", FieldType.Guid);
            definition.Fields.Add("Caption", FieldType.String);
            definition.Fields.Add("Description", FieldType.String);
            definition.Fields.Add("UserName", FieldType.String);
            return definition;
        }


        /// <summary>
        /// Create a comment object from the specified deserialization data stream
        /// </summary>
        void IPacket.ReadFields(PacketDefinition definition, SerializedPacket packet)
        {
            switch (definition.Version)
            {
                case 1:
                    //we explicitly have ID because we are not a cached packet.
                    packet.GetField("ID", out m_ID);
                    packet.GetField("Caption", out m_Caption);
                    packet.GetField("Description", out m_Description);
                    packet.GetField("UserName", out m_UserName);
                    break;
            }
        }

        /// <summary>
        /// Serialize this comment object into the specified serialization data stream
        /// </summary>
        void IPacket.WriteFields(PacketDefinition definition, SerializedPacket packet)
        {
            packet.SetField("ID", m_ID);
            packet.SetField("Caption", m_Caption);
            packet.SetField("Description", m_Description);
            packet.SetField("UserName", m_UserName);
        }

        #endregion

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public override bool Equals(object other)
        {
            //use our type-specific override
            return Equals(other as CommentPacket);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(CommentPacket other)
        {
            //Careful - can be null
            if (other == null)
            {
                return false; // since we're a live object we can't be equal.
            }

            return ((Id == other.Id)
                && (Caption == other.Caption)
                && (Description == other.Description)
                && (UserName == other.UserName)
                && base.Equals(other));
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
        /// an int representing the hash code calculated for the contents of this object
        /// </returns>
        public override int GetHashCode()
        {
            int myHash = base.GetHashCode(); // Fold in hash code for inherited base type

            myHash ^= m_ID.GetHashCode(); // Fold in hash code for our GUID
            if (m_Caption != null) myHash ^= m_Caption.GetHashCode(); // Fold in hash code for string Caption
            if (m_Description != null) myHash ^= m_Description.GetHashCode(); // Fold in hash code for string Description
            if (m_UserName != null) myHash ^= m_UserName.GetHashCode(); // Fold in hash code for string UserName

            return myHash;
        }
    }
}

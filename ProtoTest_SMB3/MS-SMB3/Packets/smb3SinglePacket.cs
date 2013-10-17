//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3SinglePacket
// Description: smb3SinglePacket defination.
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// Every SinglePacket must inherit this packet, it contains a smb3 header, but no 
    /// payload
    /// </summary>
    public abstract class smb3SinglePacket : smb3Packet
    {
        private byte[] sessionKey;
        private bool isInCompoundPacket;
        private bool isLast;
        private smb3CompoundPacket outerCompoundPacket;

        //If the packet is the last packet in a compound packet
        //there may be some padding at the end of the message.
        internal byte[] padding;

        //Indicate if this packet has fileId
        internal bool hasFileId;

        //from where this packet is received
        internal int connectionId;

        //the global context
        internal smb3ClientGlobalContext globalContext;

        /// <summary>
        /// The header of the packet
        /// </summary>
        public Packet_Header Header;


        /// <summary>
        /// The sessionKey used to sign the packet
        /// </summary>
        internal byte[] SessionKey
        {
            get
            {
                return sessionKey;
            }
            set
            {
                sessionKey = value;
            }
        }

        /// <summary>
        /// Indicate if the packet is in compoundPacket
        /// </summary>
        internal bool IsInCompoundPacket
        {
            get
            {
                return isInCompoundPacket;
            }
            set
            {
                isInCompoundPacket = value;
            }
        }

        /// <summary>
        /// Indicate if the packet is the last packet in a compound packet
        /// </summary>
        internal bool IsLast
        {
            get
            {
                return isLast;
            }
            set
            {
                isLast = value;
            }
        }


        /// <summary>
        /// The compound packet who contains this packet
        /// </summary>
        /// <returns>The compound packet who contains this packet</returns>
        internal smb3CompoundPacket OuterCompoundPacket
        {
            get
            {
                return outerCompoundPacket;
            }
            set
            {
                outerCompoundPacket = value;
            }
        }


        /// <summary>
        /// Set fileId to { 0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF }
        /// if this packet contains fileid
        /// </summary>
        internal virtual void SetFileIdToMaxValue()
        {
        }


        /// <summary>
        /// Get the fileId of the packet.
        /// If it is a packet in related compoundpacket, field must be retrieved from the first
        /// packet. if the packet does not contains fileId, it will throw NotSupportException
        /// </summary>
        /// <returns></returns>
        internal virtual FILEID GetFileId()
        {
            throw new NotSupportedException("This packet does not have fileId");
        }


        /// <summary>
        /// Get session id of the packet.
        /// If it is a packet in related compoundpacket, sessionId must be retrieved from the first
        /// packet. otherwise it will return the sessionId in its own header
        /// </summary>
        /// <returns>The sessionId</returns>
        internal abstract ulong GetSessionId();


        /// <summary>
        /// Get tree id of the packet.
        /// If it is a packet in related compoundpacket, treeId must be retrieved from the first
        /// packet. otherwise it will return the treeId in its own header
        /// </summary>
        /// <returns>The treeId</returns>
        internal abstract uint GetTreeId();

        /// <summary>
        /// Get the index in compoundPacket
        /// </summary>
        /// <returns></returns>
        internal int GetPacketIndexInCompoundPacket()
        {
            if (outerCompoundPacket == null)
            {
                return 0;
            }

            for (int i = 0; i < OuterCompoundPacket.Packets.Count; i++)
            {
                if (OuterCompoundPacket.Packets[i] == this)
                {
                    return i;
                }
            }

            throw new InvalidOperationException(
                "The OuterCompoundPacket is not the packet which contains this packet");
        }
    }
}

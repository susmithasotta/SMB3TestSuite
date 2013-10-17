//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3SetInfoRequestPacket
// Description: smb3SetInfoRequestPacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 SET_INFO Request packet is sent by a client to set information on a file or underlying object store
    /// </summary>
    public class smb3SetInfoRequestPacket : smb3StandardPacket<SET_INFO_Request>, IHasInterrelatedFields
    {
        /// <summary>
        /// Update all fields which is related to other fields which has been changed
        /// </summary>
        public void UpdateInterrelatedFields()
        {
            PayLoad.BufferLength = (uint)PayLoad.Buffer.Length;
        }


        /// <summary>
        /// Set fileId to { 0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF }
        /// if this packet contains fileid
        /// </summary>
        internal override void SetFileIdToMaxValue()
        {
            PayLoad.FileId.Persistent = ulong.MaxValue;
            PayLoad.FileId.Volatile = ulong.MaxValue;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public smb3SetInfoRequestPacket()
        {
            hasFileId = true;
        }


        /// <summary>
        /// Get the fileId of the packet.
        /// If it is a packet in related compoundpacket, field must be retrieved from the first
        /// packet which has the fileId. 
        /// </summary>
        /// <returns></returns>
        internal override FILEID GetFileId()
        {
            if ((Header.Flags & Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS)
                == Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS)
            {
                if (PayLoad.FileId.Persistent != ulong.MaxValue || PayLoad.FileId.Volatile != ulong.MaxValue)
                {
                    return PayLoad.FileId;
                }

                int indexOfThisPacket = GetPacketIndexInCompoundPacket();

                for (int i = indexOfThisPacket - 1; i >= 0; i--)
                {
                    if (OuterCompoundPacket.Packets[i].hasFileId)
                    {
                        FILEID fileId = OuterCompoundPacket.Packets[i].GetFileId();

                        if (fileId.Persistent != ulong.MaxValue || fileId.Volatile != ulong.MaxValue)
                        {
                            return fileId;
                        }
                    }
                }

                return PayLoad.FileId;
            }
            else
            {
                return PayLoad.FileId;
            }
        }
    }
}

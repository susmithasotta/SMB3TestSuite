//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3IOCtlResponsePacket
// Description: smb3IOCtlResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 IOCTL Response packet is sent by the server to transmit the results of a client smb3 IOCTL Request
    /// </summary>
    public class smb3IOCtlResponsePacket : smb3StandardPacket<IOCTL_Response>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public smb3IOCtlResponsePacket()
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

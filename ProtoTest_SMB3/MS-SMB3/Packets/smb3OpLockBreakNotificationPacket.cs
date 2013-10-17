//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3OpLockBreakNotificationPacket
// Description: smb3OpLockBreakNotificationPacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 Oplock Break Notification packet is sent by the server 
    /// when the underlying object store indicates that an opportunistic lock (oplock) is being broken,
    /// representing a change in the oplock level
    /// </summary>
    public class smb3OpLockBreakNotificationPacket : smb3StandardPacket<OPLOCK_BREAK_Notification_Packet>
    {
        /// <summary>
        /// Verify signature to see if the signature is correct
        /// </summary>
        /// <returns>True indicates the signature is correct, otherwise false</returns>
        public override bool VerifySignature()
        {
            //In 3.2.5.1.2 Verifying the Signature, TD mentioned that 
            //If the message is an interim response or an smb3 OPLOCK_BREAK notification, 
            //signing validation MUST NOT occur
            return true;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public smb3OpLockBreakNotificationPacket()
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

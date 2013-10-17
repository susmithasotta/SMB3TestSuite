//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3CreateResponsePacket
// Description: smb3CreateResponsePacket defination.
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 CREATE Response packet is sent by the server to notify the 
    /// client of the status of its smb3 CREATE Request.
    /// </summary>
    public class smb3CreateResponsePacket : smb3StandardPacket<CREATE_Response>, IHasInterrelatedFields
    {
        /// <summary>
        /// Update all fields which is related to other fields which has been changed
        /// </summary>
        public void UpdateInterrelatedFields()
        {
            PayLoad.CreateContextsLength = (uint)PayLoad.Buffer.Length;
        }


        /// <summary>
        /// Get create contexts structure from payload.Buffer.
        /// </summary>
        /// <returns>The lease key or null</returns>
        public CREATE_CONTEXT_Values[] GetCreateContexts()
        {
            if (PayLoad.CreateContextsLength == 0)
            {
                return null;
            }
            else
            {
                byte[] createContextArray = new byte[PayLoad.CreateContextsLength];

                Array.Copy(PayLoad.Buffer, (int)(PayLoad.CreateContextsOffset - smb3Consts.CreateResponseBufferStartIndex),
                    createContextArray, 0, createContextArray.Length);

                return smb3Utility.ConvertByteArrayToCreateContexts(createContextArray);
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public smb3CreateResponsePacket()
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

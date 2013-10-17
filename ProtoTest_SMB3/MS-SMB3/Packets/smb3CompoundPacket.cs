//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3CompoundPacket
// Description: smb3CompoundPacket defination.
//-------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Protocols.TestTools.StackSdk.Messages;
using Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// CompoundPacket is composed by several single packet
    /// </summary>
    public class smb3CompoundPacket : smb3Packet
    {
        private List<smb3SinglePacket> packets;
        internal smb3Decoder decoder;

        /// <summary>
        /// The packets the compoundPacket contains
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<smb3SinglePacket> Packets
        {
            get
            {
                return packets;
            }
            set
            {
                packets = value;
            }
        }

        /// <summary>
        /// Get another copy of this instance.
        /// </summary>
        /// <returns>The copy of this instance</returns>
        public override StackPacket Clone()
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Convert the object to a byte array
        /// </summary>
        /// <returns>The converted byte array</returns>
        public override byte[] ToBytes()
        {
            if (Packets == null)
            {
                return null;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                for (int i = 0; i < Packets.Count; i++)
                {
                    byte[] temp;

                    if (i != (Packets.Count - 1))
                    {
                        temp = new byte[Packets[i].Header.NextCommand];
                        byte[] packetRealBytes = Packets[i].ToBytes();
                        Array.Copy(packetRealBytes, temp, packetRealBytes.Length);
                    }
                    else
                    {
                        temp = Packets[i].ToBytes();
                    }
                    
                    ms.Write(temp, 0, temp.Length);

                    //only last packet will have padding
                    if (packets[i].padding != null)
                    {
                        ms.Write(packets[i].padding, 0, packets[i].padding.Length);
                    }
                }

                return ms.ToArray();
            }
        }


        /// <summary>
        /// Build a smb3Packet from a byte array
        /// </summary>
        /// <param name="data">The byte array</param>
        /// <param name="consumedLen">The consumed data length</param>
        /// <param name="expectedLen">The expected data length</param>
        /// <returns>The smb3Packet</returns>
        internal override void FromBytes(byte[] data, out int consumedLen, out int expectedLen)
        {
            this.Packets = new List<smb3SinglePacket>();

            Packet_Header header;

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (Channel c = new Channel(null, ms))
                {
                    header = c.Read<Packet_Header>();

                    int innerConsumedLen = 0;
                    int innerExpectedLen = 0;

                    consumedLen = 0;

                    byte[] temp = null;
                    smb3SinglePacket singlePacket = null;

                    while (header.NextCommand != 0)
                    {
                        temp = new byte[header.NextCommand];

                        Array.Copy(data, consumedLen, temp, 0, temp.Length);
                        singlePacket = decoder.DecodeCompletePacket(
                            temp, 
                            decoder.DecodeRole,
                            true,
                            GetRealSessionId(header),
                            GetRealTreeId(header),
                            out innerConsumedLen,
                            out innerExpectedLen
                            ) as smb3SinglePacket;
                        singlePacket.OuterCompoundPacket = this;
                        singlePacket.IsInCompoundPacket = true;
                        singlePacket.IsLast = false;

                        Packets.Add(singlePacket);

                        if (decoder.DecodeRole == smb3Role.Client)
                        {
                            //only client need to update context immediately.
                            //server will update context if ExpectPacket invoked.
                            smb3Event smb3Event = new smb3Event();
                            smb3Event.Type = smb3EventType.PacketReceived;
                            smb3Event.Packet = singlePacket;
                            smb3Event.ConnectionId = decoder.connectionId;

                            try
                            {
                                decoder.globalContext.Lock();
                                decoder.globalContext.UpdateContext(smb3Event);
                            }
                            finally
                            {
                                decoder.globalContext.Unlock();
                            }
                        }
                        //If a packet is in compound packet, there may be some padding at the end,
                        //here we do not rely on the innerConsumedLen but header.NextCommand
                        consumedLen += temp.Length;

                        //skip the data DecodeCompletePacket already comsumed.
                        c.ReadBytes((int)(consumedLen - c.Stream.Position));

                        header = c.Read<Packet_Header>();
                    }

                    temp = new byte[data.Length - consumedLen];
                    Array.Copy(data, consumedLen, temp, 0, temp.Length);

                    singlePacket = decoder.DecodeCompletePacket(
                        temp, 
                        decoder.DecodeRole,
                        true,
                        GetRealSessionId(header),
                        GetRealTreeId(header),
                        out innerConsumedLen,
                        out innerExpectedLen
                        ) as smb3SinglePacket;

                    // There may some padding at the last packet of compound packet
                    singlePacket.padding = new byte[temp.Length - innerConsumedLen];
                    Array.Copy(temp, innerConsumedLen, singlePacket.padding, 0, singlePacket.padding.Length);

                    singlePacket.OuterCompoundPacket = this;
                    singlePacket.IsInCompoundPacket = true;
                    singlePacket.IsLast = true;

                    Packets.Add(singlePacket);

                    if (decoder.DecodeRole == smb3Role.Client)
                    {
                        //only client need to update context immediately.
                        //server will update context if ExpectPacket invoked.
                        smb3Event lastsmb3Event = new smb3Event();
                        lastsmb3Event.Type = smb3EventType.PacketReceived;
                        lastsmb3Event.Packet = singlePacket;
                        lastsmb3Event.ConnectionId = decoder.connectionId;

                        try
                        {
                            decoder.globalContext.Lock();
                            decoder.globalContext.UpdateContext(lastsmb3Event);
                        }
                        finally
                        {
                            decoder.globalContext.Unlock();
                        }
                    }

                    consumedLen += innerConsumedLen;
                    expectedLen = 0;
                }
            }
        }


        /// <summary>
        /// Sign the packet with the session key
        /// </summary>
        public override void Sign()
        {
            if (Packets == null)
            {
                return;
            }

            foreach (smb3Packet packet in Packets)
            {
                packet.Sign();
            }
        }


        /// <summary>
        /// Verify The signature to see if the signature is correct
        /// </summary>
        /// <returns>If the signature is correct, return true, else false</returns>
        public override bool VerifySignature()
        {
            if (Packets == null)
            {
                return true;
            }

            bool verified = true;
            foreach (smb3Packet packet in Packets)
            {
                verified = verified && packet.VerifySignature();
            }

            return verified;
        }


        /// <summary>
        /// Get the real sessionId granted by server.
        /// </summary>
        /// <param name="header">The header of packet</param>
        /// <returns>The real sessionId</returns>
        private ulong GetRealSessionId(Packet_Header header)
        {
            ulong sessionId = header.SessionId;

            if ((header.Flags & Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS) 
                == Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS)
            {
                if (header.SessionId == ulong.MaxValue)
                {
                    for (int i = packets.Count - 1; i >= 0; i--)
                    {
                        if (packets[i].Header.SessionId != ulong.MaxValue)
                        {
                            sessionId = packets[i].Header.SessionId;
                            break;
                        }
                    }
                }
            }

            return sessionId;
        }


        /// <summary>
        /// Get the real treeId granted by server.
        /// </summary>
        /// <param name="header">The header of packet</param>
        /// <returns>The real treeId</returns>
        private uint GetRealTreeId(Packet_Header header)
        {
            uint treeId = header.TreeId;

            if ((header.Flags & Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND)
                == Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND)
            {
                treeId = GetTreeIdFromAsyncPacket(header);
            }

            if ((header.Flags & Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS)
                == Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS)
            {
                if (treeId == uint.MaxValue)
                {
                    for (int i = packets.Count - 1; i >= 0; i--)
                    {
                        if ((packets[i].Header.Flags & Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND)
                            == Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND)
                        {
                            treeId = GetTreeIdFromAsyncPacket(packets[i].Header);
                        }
                        else
                        {
                            treeId = packets[i].Header.TreeId;
                        }

                        //0xffffffff is not a real treeId
                        //keep searching the real treeId if it 
                        //is oxffffffff
                        if (treeId != uint.MaxValue)
                        {
                            break;
                        }
                    }
                }
            }

            return treeId;
        }


        /// <summary>
        /// Get treeId from the async packet
        /// </summary>
        /// <param name="header">The packet header</param>
        /// <returns>The treeId</returns>
        private uint GetTreeIdFromAsyncPacket(Packet_Header header)
        {
            try
            {
                decoder.globalContext.Lock();

                smb3OutStandingRequest outStandingRequest =
                    decoder.globalContext.connectionTable[decoder.connectionId].outstandingRequests[header.MessageId];

                if (outStandingRequest.request is SmbNegotiateRequestPacket)
                {
                    return (outStandingRequest.request as SmbNegotiateRequestPacket).Header.Tid;
                }
                else
                {
                    return (outStandingRequest.request as smb3SinglePacket).Header.TreeId;
                }
            }
            finally
            {
                decoder.globalContext.Unlock();
            }
        }
    }
}

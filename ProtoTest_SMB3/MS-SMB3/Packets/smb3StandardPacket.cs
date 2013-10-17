//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3StandardPacket
// Description: smb3StandardPacket defination.
//-------------------------------------------------------------------------

using System;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

using Microsoft.Protocols.TestTools.StackSdk.Messages;
using Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling;
using System.Text;
using Microsoft.Protocols.TestTools.StackSdk.Security.Cryptographic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// This is a genetic class which every smb3 packet will inherit
    /// </summary>
    /// <typeparam name="T">The payload type</typeparam>
    public class smb3StandardPacket<T> : smb3SinglePacket
    {
        //unknown padding, some packet may pad some data at the end.
        public byte[] UnknownPadding;

        /// <summary>
        /// The payload of the packet
        /// </summary>
        public T PayLoad;

        /// <summary>
        /// Get a copy of this object
        /// </summary>
        /// <returns>The cloned object</returns>
        public override StackPacket Clone()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Covert to a byte array
        /// </summary>
        /// <returns>The byte array</returns>
        public override byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (Channel channel = new Channel(null, ms))
                {
                    channel.Write(this.Header);
                    channel.Write(this.PayLoad);

                    if (UnknownPadding != null)
                    {
                        channel.WriteBytes(UnknownPadding);
                    }

                    return ms.ToArray();
                }
            }
        }


        /// <summary>
        /// Build a smb3Packet from a byte array
        /// </summary>
        /// <param name="data">The byte array</param>
        /// <param name="consumedLen">The consumed data length</param>
        /// <param name="expectedLen">The expected data length</param>
        internal override void FromBytes(byte[] data, out int consumedLen, out int expectedLen)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (Channel c = new Channel(null, ms))
                {
                    this.Header = c.Read<Packet_Header>();
                    this.PayLoad = c.Read<T>();

                    consumedLen = (int)c.Stream.Position;
                    expectedLen = 0;
                }
            }
        }


        /// <summary>
        /// Sign the message with the sessionKey
        /// </summary>
        public override void Sign()
        {
            if ((Header.Flags & Packet_Header_Flags_Values.FLAGS_SIGNED) == Packet_Header_Flags_Values.FLAGS_SIGNED)
            {
                Header.Signature = new byte[smb3Consts.SignatureSize];

                byte[] packetBuffer = ToBytes();

                if (IsInCompoundPacket)
                {
                    if (!IsLast)
                    {
                        byte[] buffer = new byte[Header.NextCommand];
                        Array.Copy(packetBuffer, buffer, packetBuffer.Length);
                        Sign(buffer);
                    }
                    else
                    {
                        if (padding == null)
                        {
                            Sign(packetBuffer);
                        }
                        else
                        {
                            byte[] paddingBuffer = new byte[packetBuffer.Length + padding.Length];
                            Array.Copy(packetBuffer, paddingBuffer, packetBuffer.Length);
                            Array.Copy(padding, 0, paddingBuffer, packetBuffer.Length, padding.Length);

                           // Sign(paddingBuffer);
                            SignAES(packetBuffer);
                           
                        }
                    }
                }
                else
                {
                   // Sign(packetBuffer);
                    SignAES(packetBuffer);
                   
                }
            }
        }

        public void Sign(bool IsAESSigning)
        {
            if ((Header.Flags & Packet_Header_Flags_Values.FLAGS_SIGNED) == Packet_Header_Flags_Values.FLAGS_SIGNED)
            {
                Header.Signature = new byte[smb3Consts.SignatureSize];

                byte[] packetBuffer = ToBytes();

                if (IsInCompoundPacket)
                {
                    if (!IsLast)
                    {
                        byte[] buffer = new byte[Header.NextCommand];
                        Array.Copy(packetBuffer, buffer, packetBuffer.Length);
                        Sign(buffer);
                    }
                    else
                    {
                        if (padding == null)
                        {
                            Sign(packetBuffer);
                        }
                        else
                        {
                            byte[] paddingBuffer = new byte[packetBuffer.Length + padding.Length];
                            Array.Copy(packetBuffer, paddingBuffer, packetBuffer.Length);
                            Array.Copy(padding, 0, paddingBuffer, packetBuffer.Length, padding.Length);

                            //Sign(paddingBuffer);
                            SignAES(paddingBuffer);
                        }
                    }
                }
                else
                {
                    // Sign(packetBuffer);
                    SignAES(packetBuffer);
                }
            }
        }
        /// <summary>
        /// Sign the message
        /// </summary>
        /// <param name="message">The message</param>
        private void Sign(byte[] message)
        {
            Header.Signature = new byte[smb3Consts.SignatureSize];

            byte[] sessionKey16Bytes = new byte[smb3Consts.SignatureSize];

            Array.Copy(SessionKey, sessionKey16Bytes, sessionKey16Bytes.Length);
            
            HMACSHA256 hmacSha = new HMACSHA256(sessionKey16Bytes);
            
            byte[] signature256 = hmacSha.ComputeHash(message);

            Array.Copy(signature256, Header.Signature, Header.Signature.Length);
        }

        private void SignAES(byte[] message)
        {
            byte[] SigningKey;
                      
            byte[] sessionKey16Bytes = new byte[smb3Consts.SignatureSize];

       
            Array.Copy(SessionKey, sessionKey16Bytes, sessionKey16Bytes.Length);
            SigningKey = SP8001008KeyDerivation.CounterModeHmacSha256KeyDerive(
                                sessionKey16Bytes,
                                Encoding.ASCII.GetBytes("SMB2AESCMAC\0"),
                                Encoding.ASCII.GetBytes("SmbSign\0"),
                                128);

            byte[] signature256 = AesCmac128.ComputeHash(SigningKey,message);
           Array.Copy(signature256, Header.Signature, Header.Signature.Length);
        }
        /// <summary>
        /// Verify signature to see if the signature is correct
        /// </summary>
        /// <returns>True indicates the signature is correct, otherwise false</returns>
        public override bool VerifySignature()
        {
            //If MessageId is 0xFFFFFFFFFFFFFFFF, no verification is necessary
            if (Header.MessageId == ulong.MaxValue)
            {
                return true;
            }

            if ((Header.Flags & Packet_Header_Flags_Values.FLAGS_SIGNED) == Packet_Header_Flags_Values.FLAGS_SIGNED)
            {
                byte[] signature = (byte[])Header.Signature.Clone();

                Sign();

                bool isMatch = smb3Utility.AreEqual(signature, Header.Signature);

                Header.Signature = signature;

                return isMatch;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// Get session id of the packet.
        /// If it is a packet in related compoundpacket, sessionId must be retrieved from the first
        /// packet. otherwise it will return the sessionId in its own header
        /// </summary>
        /// <returns>The sessionId</returns>
        internal override ulong GetSessionId()
        {
            if ((Header.Flags & Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS)
                == Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS)
            {
                int indexOfThisPacket = GetPacketIndexInCompoundPacket();

                for (int i = indexOfThisPacket; i >= 0; i--)
                {
                    if (OuterCompoundPacket.Packets[i].Header.SessionId != ulong.MaxValue)
                    {
                        return OuterCompoundPacket.Packets[i].Header.SessionId;
                    }
                }

                return Header.SessionId;
            }
            else
            {
                return Header.SessionId;
            }
        }


        /// <summary>
        /// Get tree id of the packet.
        /// If it is a packet in related compoundpacket, treeId must be retrieved from the first
        /// packet. otherwise it will return the treeId in its own header
        /// </summary>
        /// <returns>The treeId</returns>
        internal override uint GetTreeId()
        {
            uint treeId = 0;

            if ((Header.Flags & Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS)
                == Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS)
            {
                int indexOfThisPacket = GetPacketIndexInCompoundPacket();

                for (int i = indexOfThisPacket; i >= 0; i--)
                {
                    if ((OuterCompoundPacket.Packets[i].Header.Flags & Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND)
                        == Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND)
                    {
                        treeId = GetTreeIdFromAsyncPacket(OuterCompoundPacket.Packets[i].Header);

                        //0xffffffff is not a real treeId
                        //keep searching the real treeId if it 
                        //is oxffffffff
                        if (treeId != uint.MaxValue)
                        {
                            return treeId;
                        }
                    }
                    else if (OuterCompoundPacket.Packets[i].Header.TreeId != uint.MaxValue)
                    {
                        return OuterCompoundPacket.Packets[i].Header.TreeId;
                    }
                }
            }
            else if ((Header.Flags & Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND)
                    == Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND)
            {
                return GetTreeIdFromAsyncPacket(Header);
            }

            return Header.TreeId;
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
                globalContext.Lock();

                smb3OutStandingRequest outStandingRequest =
                    globalContext.connectionTable[connectionId].outstandingRequests[header.MessageId];

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
                globalContext.Unlock();
            }
        }
    }
}

//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: SmbNegotiateRequestPacket
// Description: SmbNegotiateRequestPacket defination.
//-------------------------------------------------------------------------

using System;
using System.IO;

using Microsoft.Protocols.TestTools.StackSdk.Messages;
using Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The packet is sent by client to handle dialect and capability negotiation
    /// </summary>
    public class SmbNegotiateRequestPacket : smb3Packet, IHasInterrelatedFields
    {
        /// <summary>
        /// The header of this packet
        /// </summary>
        public SmbHeader Header;

        /// <summary>
        /// The payload of this packet
        /// </summary>
        public SmbNegotiateRequest PayLoad;

        /// <summary>
        /// Get a copy of this object
        /// </summary>
        /// <returns>The copy of this object</returns>
        public override StackPacket Clone()
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Convert to a byte array
        /// </summary>
        /// <returns>The converted byte array</returns>
        public override byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (Channel channel = new Channel(null, ms))
                {
                    channel.Write(Header);
                    channel.Write(PayLoad);

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
                    this.Header = c.Read<SmbHeader>();
                    this.PayLoad = c.Read<SmbNegotiateRequest>();

                    consumedLen = (int)c.Stream.Position;
                    expectedLen = 0;
                }
            }
        }


        /// <summary>
        /// Sign the message using the sessionKey
        /// </summary>
        public override void Sign()
        {
            //smb negotiate does not need to be signed
        }


        /// <summary>
        /// Update all fields which is related to other fields which has been changed
        /// </summary>
        public void UpdateInterrelatedFields()
        {
            PayLoad.ByteCount = (ushort)PayLoad.DialectName.Length;
        }


        /// <summary>
        /// Verify the signature to see if the message is a valid message
        /// </summary>
        /// <returns>True indicate the signature is correct, otherwise false</returns>
        public override bool VerifySignature()
        {
            return true;
        }
    }
}

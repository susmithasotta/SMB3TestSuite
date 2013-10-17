//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3Packet
// Description: smb3Packet defination.
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// smb3Packet is the class which every single smb3 packet will inherit
    /// </summary>
    public abstract class smb3Packet : StackPacket
    {
        private smb3Endpoint endpoint;

        internal smb3Endpoint Endpoint
        {
            get
            {
                return endpoint;
            }
            set
            {
                endpoint = value;
            }
        }

        /// <summary>
        /// Build a smb3Packet from a byte array
        /// </summary>
        /// <param name="data">The byte array</param>
        /// <param name="consumedLen">The consumed data length</param>
        /// <param name="expectedLen">The expected data length</param>
        internal abstract void FromBytes(byte[] data, out int consumedLen, out int expectedLen);


        /// <summary>
        /// Sign the message with the sessionKey
        /// </summary>
        public abstract void Sign();


        /// <summary>
        /// Verify The signature to see if the signature is correct
        /// </summary>
        /// <returns>If the signature is correct, return true, else false</returns>
        public abstract bool VerifySignature();
    }
}

//------------------------------------------------------------------------------------------
// Copyright(c) 2010 Microsoft Corporation
// All rights reserved.
//
// Module Name: UnkeyedChecksum Class
//
// Abstract:
// Provide methods for UnkeyedChecksum Checksum Generation:
// CRC32, rsa_md4, rsa_md5, sha1
//
// Build Date: 01/12/2010(18:15PM)
// Updated by: Li Zhang (v-lzha@microsoft.com)
//-------------------------------------------------------------------------------------------

using System;
using System.Security.Cryptography;
using Microsoft.Protocols.TestTools.StackSdk.Security.Cryptographic;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile
{
    /// <summary>
    /// AES Key Generator
    /// </summary>
    internal static class UnkeyedChecksum
    {
        /// <summary>
        /// Caculate a unkeyed checksum (CRC32, rsa_md4, rsa_md5, sha1)
        /// </summary>
        /// <param name="input">input data</param>
        /// <param name="type">checksum type</param>
        /// <returns>the calculated checksum</returns>
        internal static byte[] GetMic(byte[] input, ChecksumType checksumType)
        {
            switch (checksumType)
            {
                case ChecksumType.CRC32:
                    return CryptoUtility.ComputeCRC32(input);
                    
                case ChecksumType.rsa_md4:
                    return CryptoUtility.ComputeMd4(input);
                    
                case ChecksumType.rsa_md5:
                    return CryptoUtility.ComputeMd5(input);
                    
                case ChecksumType.sha1:
                    return CryptoUtility.ComputeSha1(input);

                default:
                    throw new ArgumentException(
                        "Not a valid unkeyed checksum type.");
            }
        }
    }
}
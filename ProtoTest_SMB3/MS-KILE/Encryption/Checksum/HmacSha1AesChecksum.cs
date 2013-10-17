//------------------------------------------------------------------------------------------
// Copyright(c) 2010 Microsoft Corporation
// All rights reserved.
//
// Module Name: HmacSha1Aes Class
//
// Abstract:
// Provide methods for HmacSha1Aes Checksum Generation:
// hmac_sha1_96_aes128, and hmac_sha1_96_aes256
//
// Build Date: 01/12/2010(18:15PM)
// Updated by: Li Zhang (v-lzha@microsoft.com)
//-------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile
{
    /// <summary>
    /// Hmac-Sha1-Aes Checksum
    /// </summary>
    public static class HmacSha1AesChecksum
    {
        /// <summary>
        /// Get Hmac-Sha1-Aes Checksum
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="input">input data</param>
        /// <param name="usage">usage number</param>
        /// <param name="aesKeyType">aes key type which decides key size</param>
        /// <returns>the calculated checksum</returns>
        public static byte[] GetMic(
            byte[] key,
            byte[] input,
            int usage,
            AesKeyType aesKeyType)
        {
            // make the kc key from aes key generation
            byte[] kcKey = AesKey.MakeDerivedKey(
                key, (KeyUsageNumber)usage, DerivedKeyType.Kc, aesKeyType);
            
            // get mic from kc key and input
            byte[] hash = CryptoUtility.ComputeHmacSha1(kcKey, input);
            return ArrayUtility.SubArray(hash, 0, ConstValue.HMAC_HASH_OUTPUT_SIZE);
        }
    }
}
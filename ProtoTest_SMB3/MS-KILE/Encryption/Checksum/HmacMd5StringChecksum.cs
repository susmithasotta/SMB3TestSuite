//------------------------------------------------------------------------------------------
// Copyright(c) 2010 Microsoft Corporation
// All rights reserved.
//
// Module Name: HmacMd5StringChecksum Class
//
// Abstract:
// Provide methods for HmacMd5String Checksum Generation
//
// Build Date: 01/12/2010(18:15PM)
// Updated by: Li Zhang (v-lzha@microsoft.com)
//-------------------------------------------------------------------------------------------

using System;
using System.Text;
using System.Security.Cryptography;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile
{
    /// <summary>
    /// Hmac-Md5-String Checksum
    /// </summary>
    public static class HmacMd5StringChecksum
    {
        /// <summary>
        /// Get Hmac-Md5-String Checksum
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="input">input data</param>
        /// <param name="usage">key usage number</param>
        /// <returns>the caculated checksum</returns>
        public static byte[] GetMic(
            byte[] key,
            byte[] input,
            int usage)
        {
            // get sign key
            byte[] signatureData = Encoding.ASCII.GetBytes(ConstValue.SIGNATURE_KEY);
            HMACMD5 hmacMd5 = new HMACMD5(key);
            byte[] signKey = hmacMd5.ComputeHash(signatureData);
            hmacMd5.Key = signKey;

            // toBeHashedData = keyUsageData + inputData
            byte[] usageData = BitConverter.GetBytes(usage);
            byte[] toBeHashedData = ArrayUtility.ConcatenateArrays(usageData, input);

            // hash result
            byte[] md5Hash = CryptoUtility.ComputeMd5(toBeHashedData);
            return hmacMd5.ComputeHash(md5Hash);
        }
    }
}
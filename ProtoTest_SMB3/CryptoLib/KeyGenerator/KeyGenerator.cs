﻿//------------------------------------------------------------------------------------------
// Copyright(c) 2010 Microsoft Corporation
// All rights reserved.
//
// Module Name: Key Generator Class
//
// Abstract:
// Provide methods for Key Generation in MS-KILE Stack SDK
//
// Build Date: 12/31/2009(12:34PM)
// Updated by: Li Zhang (v-lzha@microsoft.com)
//-------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Cryptographic
{
    /// <summary>
    /// Key Generator
    /// (called by KilePdu and KileDecoder)
    /// </summary>
    public static class KeyGenerator
    {
        /// <summary>
        /// Generate key according to password, salt and encryption type
        /// </summary>
        /// <param name="type">encryption type</param>
        /// <param name="password">password</param>
        /// <param name="salt">salt</param>
        /// <returns>the generated key in bytes</returns>
        public static byte[] MakeKey(EncryptionType type, string password, string salt)
        {
            switch (type)
            {
                case EncryptionType.AES128_CTS_HMAC_SHA1_96:
                    {
                        return AesKey.MakeStringToKey(password, salt, 
                            AesKey.DEFAULT_ITERATION_COUNT, AesKeyType.Aes128BitsKey);
                    }

                case EncryptionType.AES256_CTS_HMAC_SHA1_96:
                    {
                        return AesKey.MakeStringToKey(password, salt, 
                            AesKey.DEFAULT_ITERATION_COUNT, AesKeyType.Aes256BitsKey);
                    }

                case EncryptionType.DES_CBC_CRC:
                case EncryptionType.DES_CBC_MD5:
                    {
                        return DesKey.MakeStringToKey(password, salt);
                    }

                case EncryptionType.RC4_HMAC:
                case EncryptionType.RC4_HMAC_EXP:
                    {
                        return Rc4Key.MakeStringToKey(password);
                    }

                default:
                    throw new ArgumentException("Unsupported encryption type.");
            }
        }
    }
}
//------------------------------------------------------------------------------------------
// Copyright(c) 2010 Microsoft Corporation
// All rights reserved.
//
// Module Name: RC4 Key Class
//
// Abstract:
// Provide methods for RC4 Key Generation in MS-KILE
//
// Build Date: 12/31/2009(12:34PM)
// Updated by: Li Zhang (v-lzha@microsoft.com)
//-------------------------------------------------------------------------------------------

using System;
using System.Text;
using Microsoft.Protocols.TestTools.StackSdk.Security.Cryptographic;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile
{
    /// <summary>
    /// RC4 key generator
    /// </summary>
    internal static class Rc4Key
    {
        /// <summary>
        /// Derive an RC4 key based on password
        /// </summary>
        /// <param name="password">user password</param>
        /// <returns>the generated RC4 key of 16 bytes</returns>
        internal static byte[] MakeStringToKey(string password)
        {
            if (null == password)
            {
                throw new ArgumentException("Input password should not be null.");
            }

            // password bytes
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);

            // compute md4 hash
            return CryptoUtility.ComputeMd4(passwordBytes);
        }
    }
}

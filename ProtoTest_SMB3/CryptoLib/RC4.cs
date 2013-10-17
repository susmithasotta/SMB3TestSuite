//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Description: the class will be inherited by all class who implement RC4.
//------------------------------------------------------------------------------


using System;
using System.Security.Cryptography;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Cryptographic
{
    /// <summary>
    /// A abstract class which all RC4 Algorithm implemention will inherit.
    /// </summary>
    public abstract class RC4 : SymmetricAlgorithm
    {
        /// <summary>
        /// Generate default RC4 instance
        /// </summary>
        /// <returns>default RC4 instance</returns>
        public new static RC4 Create()
        {
            return new RC4CryptoServiceProvider();
        }
    }
}

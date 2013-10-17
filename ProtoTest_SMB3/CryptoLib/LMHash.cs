//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Description: the class will be inherited by all class who implement Lm hash.
//------------------------------------------------------------------------------


using System;
using System.Security.Cryptography;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Cryptographic
{
    /// <summary>
    /// The abstract LM hash class, every LM hash implemention should inherit this class
    /// </summary>
    public abstract class LMHash : HashAlgorithm
    {
        /// <summary>
        /// Generate a default lm hash instance
        /// </summary>
        /// <returns>The default lm hash instance</returns>
        public new static LMHash Create()
        {
            return new LMHashManaged();
        }
    }
}

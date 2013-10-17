//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Description: the class will be inherited by all class who implement MD4.
//------------------------------------------------------------------------------


using System;
using System.Security.Cryptography;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Cryptographic
{
    /// <summary>
    /// A abstract class which every MD4 algorithm implementation will inherit.
    /// </summary>
    public abstract class MD4 : HashAlgorithm
    {
        /// <summary>
        /// Generate a default MD4 instance
        /// </summary>
        /// <returns>a default MD4 instance</returns>
        public new static MD4 Create()
        {
            return new MD4CryptoServiceProvider();
        }
    }
}

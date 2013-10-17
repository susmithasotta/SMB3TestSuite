//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------


using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Sspi
{
    /// <summary>
    /// Abstract base class for client SecurityContext, SecurityContext used by client must be derived from this class.
    /// </summary>
    public abstract class ClientSecurityContext : SecurityContext
    {
        /// <summary>
        /// Initialize SecurityContext by server token.
        /// </summary>
        /// <param name="token">Server token</param>
        /// <exception cref="SspiException">If initialize fail, this exception will be thrown.</exception>
        public abstract void Initialize(byte[] token);
    }
}

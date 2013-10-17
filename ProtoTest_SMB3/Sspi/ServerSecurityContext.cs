﻿//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------


using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Sspi
{
    /// <summary>
    /// Abstract base class for server SecurityContext, SecurityContext used by server must be derived from this class.
    /// </summary>
    public abstract class ServerSecurityContext : SecurityContext
    {
        /// <summary>
        /// Accept client token.
        /// </summary>
        /// <param name="token">Client token</param>
        /// <exception cref="SspiException">If accept fail, this exception will be thrown.</exception>
        public abstract void Accept(byte[] token);
    }
}

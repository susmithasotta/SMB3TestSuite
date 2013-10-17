﻿//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------



namespace Microsoft.Protocols.TestTools.StackSdk.Security.Sspi
{
    /// <summary>
    /// The SecPkgContext_Sizes structure indicates the sizes of important structures used in the message support 
    /// functions. The QueryContextAttributes (General) function uses this structure.
    /// </summary>
    public struct SecurityPackageContextSizes
    {
        /// <summary>
        /// Specifies the maximum size of the security token used in the authentication exchanges
        /// </summary>
        public uint MaxTokenSize;

        /// <summary>
        /// Specifies the maximum size of the signature created by the MakeSignature function. This member must
        /// be zero if integrity services are not requested or available.
        /// </summary>
        public uint MaxSignatureSize;

        /// <summary>
        /// Specifies the preferred integral size of the messages. For example, eight indicates that messages should
        /// be of size zero mod eight for optimal performance. Messages other than this block size can be padded.
        /// </summary>
        public uint BlockSize;

        /// <summary>
        /// Size of the security trailer to be appended to messages. This member should be zero if the relevant 
        /// services are not requested or available.
        /// </summary>
        public uint SecurityTrailerSize;
    }
}

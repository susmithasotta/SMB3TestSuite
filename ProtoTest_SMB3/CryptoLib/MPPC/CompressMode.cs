//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Description: Mppc compress mode definition
//------------------------------------------------------------------------------


using System;

namespace Microsoft.Protocols.TestTools.StackSdk.Compression.Mppc
{
    /// <summary>
    /// compress option of Mpcc compression
    /// </summary>
    [Flags()]
    public enum CompressMode
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Sliding window should be flushed
        /// </summary>
        Flush = 1,

        /// <summary>
        /// set sliding window's position to zero
        /// </summary>
        SetToFront = 2,

        /// <summary>
        /// indicate the data is compressed
        /// </summary>
        Compressed = 4
    }
}

//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Description: compress mode definition
//------------------------------------------------------------------------------


using System;

namespace Microsoft.Protocols.TestTools.StackSdk.Compression.Mppc
{
    /// <summary>
    /// Compressed mode, 8k or 64k
    /// </summary>
    public enum SlidingWindowSize
    {
        /// <summary>
        /// use 8k sliding window
        /// </summary>
        EightKB,

        /// <summary>
        /// use 64k sliding window
        /// </summary>
        SixtyFourKB,
    }
}

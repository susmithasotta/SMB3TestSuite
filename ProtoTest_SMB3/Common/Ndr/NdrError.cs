﻿//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk
{
    /// <summary>
    /// Copied from RpcNtErr.h, defined NDR funtions return value.
    /// </summary>
    public static class NdrError
    {
        /// <summary>
        /// The call succeeded.
        /// Defined as ERROR_SUCCESS in WinError.h
        /// </summary>
        public const int RPC_S_OK = 0;

        /// <summary>
        /// Out of memory.
        /// Defined as ERROR_OUTOFMEMORY in WinError.h
        /// </summary>
        public const int RPC_S_OUT_OF_MEMORY = 14;

        /// <summary>
        /// The argument was not valid.
        /// Defined as ERROR_INVALID_PARAMETER in WinError.h
        /// </summary>
        public const int RPC_S_INVALID_ARG = 87;

        /// <summary>
        /// The buffer was not valid.
        /// Defined as ERROR_INVALID_USER_BUFFER in WinError.h
        /// </summary>
        public const int RPC_X_INVALID_BUFFER = 1784;
    }
}

﻿using System;

namespace Microsoft.Protocols.TestTools.StackSdk
{
    /// <summary>
    /// it is used in context to memery the state of the transport.  
    /// </summary>
    public enum StackTransportState
    {
        /// <summary>
        /// Initialized
        /// </summary>
        Init,

        /// <summary>
        /// connecting
        /// </summary>
        ConnectionRequested,

        /// <summary>
        /// connectied
        /// </summary>
        ConnectionEstablished,

        /// <summary>
        /// disconnected
        /// </summary>
        ConnectionDown,

        /// <summary>
        /// timeout
        /// </summary>
        ConnectionTimeout
    }

    /// <summary>
    /// The Ip protocol version
    /// </summary>
    public enum IpVersion
    {
        /// <summary>
        /// Ip version 4.
        /// </summary>
        Ipv4 = 0,

        /// <summary>
        /// Ip version 6.
        /// </summary>
        Ipv6 = 1,

        /// <summary>
        /// Uncertain
        /// </summary>
        Any = 2
    }

}

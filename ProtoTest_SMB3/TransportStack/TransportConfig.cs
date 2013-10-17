// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: Transport parameters configured by user. 
// ------------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// the role of transport
    /// </summary>
    public enum Role : int
    {
        /// <summary>
        /// none
        /// </summary>
        None,

        /// <summary>
        /// client role
        /// </summary>
        Client,

        /// <summary>
        /// server role
        /// </summary>
        Server,

        /// <summary>
        /// p2p role
        /// </summary>
        P2P
    }


    /// <summary>
    /// the type of transport.
    /// </summary>
    public enum StackTransportType
    {
        /// <summary>
        /// none
        /// </summary>
        None,

        /// <summary>
        /// TCP transport
        /// </summary>
        Tcp,

        /// <summary>
        /// UDP transport
        /// </summary>
        Udp,

        /// <summary>
        /// Stream transport
        /// </summary>
        Stream,

        /// <summary>
        /// Netbios transport
        /// </summary>
        Netbios,
    }


    /// <summary>
    /// TransportConfig stores the configurable parameters used by transport.
    /// </summary>
    public abstract class TransportConfig
    {
        #region fields
        // Default buffer size
        private const int DefaultBufferSize = 8 * 1024;

        private Role role;
        private StackTransportType type;
        private int bufferSize;

        #endregion


        #region properties

        /// <summary>
        /// the transport role.
        /// </summary>
        public Role Role
        {
            get
            {
                return this.role;
            }
            set
            {
                this.role = value;
            }
        }


        /// <summary>
        /// the transport type.
        /// </summary>
        public StackTransportType Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }


        /// <summary>
        /// the size of buffer used for receiving data.
        /// </summary>
        public int BufferSize
        {
            get
            {
                return this.bufferSize;
            }
            set
            {
                this.bufferSize = value;
            }
        }

        #endregion


        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        protected TransportConfig()
        {
            this.bufferSize = DefaultBufferSize;
            this.role = Role.None;
            this.type = StackTransportType.None;
        }

        #endregion
    }
}

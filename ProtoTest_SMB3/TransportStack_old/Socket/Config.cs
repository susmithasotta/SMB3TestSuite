// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: Transport parameters configured by user. 
// ------------------------------------------------------------------------------

using System;
using System.Net;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// TransportConfig stores the configurable parameters used by transport.
    /// </summary>
    public class SocketTransportConfig : TransportConfig
    {
        #region fields

        private int maxConnections;
        private IPAddress localIpAddress;
        private int localIpPort;
        private IPAddress remoteIpAddress;
        private int remoteIpPort;

        #endregion


        #region properties

        /// <summary>
        /// the max connections supported by the transport.
        /// </summary>
        public int MaxConnections
        {
            get
            {
                return this.maxConnections;
            }
            set
            {
                this.maxConnections = value;
            }
        }


        /// <summary>
        /// the local IPAddress.
        /// </summary>
        public IPAddress LocalIpAddress
        {
            get
            {
                return this.localIpAddress;
            }
            set
            {
                this.localIpAddress = value;
            }
        }


        /// <summary>
        /// the local IP port.
        /// </summary>
        public int LocalIpPort
        {
            get
            {
                return this.localIpPort;
            }
            set
            {
                this.localIpPort = value;
            }
        }


        /// <summary>
        /// the remote IPAddress.
        /// </summary>
        public IPAddress RemoteIpAddress
        {
            get
            {
                return this.remoteIpAddress;
            }
            set
            {
                this.remoteIpAddress = value;
            }
        }


        /// <summary>
        /// the remote IP port.
        /// </summary>
        public int RemoteIpPort
        {
            get
            {
                return this.remoteIpPort;
            }
            set
            {
                this.remoteIpPort = value;
            }
        }

        #endregion


        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        public SocketTransportConfig()
            : base()
        {
        }

        #endregion
    }
}

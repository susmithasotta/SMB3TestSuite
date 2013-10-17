// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: Transport parameters configured by user. 
// ------------------------------------------------------------------------------

using System;
using System.Net;
using Microsoft.Protocols.TestTools.StackSdk;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// TransportConfig stores the configurable parameters used by transport.
    /// </summary>
    public class NetbiosTransportConfig : TransportConfig
    {
        #region fields

        private int maxSessions;
        private int maxNames;
        private string localNetbiosName;
        private string remoteNetbiosName;
        private byte adapterIndex;

        #endregion


        #region properties

        /// <summary>
        /// the max sessions supported by the transport.
        /// </summary>
        public int MaxSessions
        {
            get
            {
                return this.maxSessions;
            }
            set
            {
                this.maxSessions = value;
            }
        }


        /// <summary>
        /// the max Netbios names used to initialize the NCB. It is only used in NetBios tranport.
        /// </summary>
        public int MaxNames
        {
            get
            {
                return this.maxNames;
            }
            set
            {
                this.maxNames = value;
            }
        }


        /// <summary>
        /// the remote Betbios name. It is only used in NetBios tranport.
        /// </summary>
        public string RemoteNetbiosName
        {
            get
            {
                return this.remoteNetbiosName;
            }
            set
            {
                this.remoteNetbiosName = value;
            }
        }


        /// <summary>
        /// the local Netbios name. It is only used in NetBios tranport.
        /// </summary>
        public string LocalNetbiosName
        {
            get
            {
                return this.localNetbiosName;
            }
            set
            {
                this.localNetbiosName = value;
            }
        }


        /// <summary>
        /// Indicate which network adapter will be used
        /// </summary>
        public byte AdapterIndex
        {
            get
            {
                return adapterIndex;
            }
            set
            {
                adapterIndex = value;
            }
        }

        #endregion


        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        public NetbiosTransportConfig()
        {
        }

        #endregion
    }
}

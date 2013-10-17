//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ServerConfig
// Description: The configuration of smb3 server
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The configuration of smb3 server
    /// </summary>
    public struct smb3ServerConfig
    {
        private bool requireMessageSigning;
        private int serverTcpListeningPort;
        private smb3TransportType transportType;
        private string localNetbiosName;
        private bool isDfsCapable;

        /// <summary>
        /// Indicate whether the packet need to be signed
        /// </summary>
        public bool RequireMessageSigning
        {
            get
            {
                return requireMessageSigning;
            }
            set
            {
                requireMessageSigning = value;
            }
        }


        /// <summary>
        /// The tcp listening port of the server, this parameter is used when TransportType is TCP
        /// </summary>
        public int ServerTcpListeningPort
        {
            get
            {
                return serverTcpListeningPort;
            }
            set
            {
                serverTcpListeningPort = value;
            }
        }


        /// <summary>
        /// The unerlying transport type
        /// </summary>
        public smb3TransportType TransportType
        {
            get
            {
                return transportType;
            }
            set
            {
                transportType = value;
            }
        }


        /// <summary>
        /// The local netbios name, this is used when transportType is netbios
        /// </summary>
        public string LocalNetbiosName
        {
            get
            {
                return localNetbiosName;
            }
            set
            {
                localNetbiosName = value;
            }
        }


        /// <summary>
        /// A Boolean that, if set, indicates that the server supports the Distributed File System.
        /// </summary>
        public bool IsDfsCapable
        {
            get
            {
                return isDfsCapable;
            }
            set
            {
                isDfsCapable = value;
            }
        }
    }
}

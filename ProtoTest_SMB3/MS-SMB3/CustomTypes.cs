﻿//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: Custom Type
// Description: Contains some structures used by sdk
//-------------------------------------------------------------------------

using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The type of transport which smb3 will use.
    /// </summary>
    public enum smb3TransportType
    {
        /// <summary>
        /// Tcp transport
        /// </summary>
        Tcp,

        /// <summary>
        /// Netbios transport
        /// </summary>
        NetBios
    }

    /// <summary>
    /// Contain information about underlying tranport 
    /// </summary>
    public class smb3Endpoint
    {
        private smb3TransportType transportType;
        private IPEndPoint remoteEndpoint;
        private byte sessionId;
        private int endpointId;


        /// <summary>
        /// The transport type of the connection
        /// </summary>
        public smb3TransportType TransportType
        {
            get
            {
                return transportType;
            }
        }


        /// <summary>
        /// The remote endpoint
        /// </summary>
        public IPEndPoint RemoteEndpoint
        {
            get
            {
                return remoteEndpoint;
            }
        }

        /// <summary>
        /// Netbios session id
        /// </summary>
        public byte SessionId
        {
            get
            {
                return sessionId;
            }
        }

        public int EndpointId
        {
            get
            {
                return endpointId;
            }
            set
            {
                endpointId = value;
            }
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        public smb3Endpoint()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="transportType">The transport type of the connection</param>
        /// <param name="remoteEndpoint">The remote endpoint</param>
        /// <param name="sessionId">Netbios session id</param>
        /// <param name="endpointId">Identify this endpoint</param>
        internal smb3Endpoint( smb3TransportType transportType, IPEndPoint remoteEndpoint,
            byte sessionId, int endpointId)
        {
            this.transportType = transportType;
            this.remoteEndpoint = remoteEndpoint;
            this.sessionId = sessionId;
            this.endpointId = endpointId;
        }
    }


    /// <summary>
    /// The security protocol used by GSS-API
    /// </summary>
    public enum SecurityPackage
    {
        /// <summary>
        /// NLMP
        /// </summary>
        Nlmp,


        /// <summary>
        /// Kerberos
        /// </summary>
        Kerberos,

        /// <summary>
        /// the Negotiate security package selects between Kerberos and NTLM
        /// </summary>
        Negotiate
    }


    /// <summary>
    /// The version of smb
    /// </summary>
    public enum SmbVersion
    {
        /// <summary>
        /// Smb
        /// </summary>
        Version1,

        /// <summary>
        /// smb3
        /// </summary>
        Version2
    }

    /// <summary>
    /// The enumeration type of client context attributes.
    /// </summary>
    [Flags]
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public enum ClientContextAttribute
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Delegate
        /// </summary>
        Delegate = 1,

        /// <summary>
        /// MutualAuthentication
        /// </summary>
        MutualAuthentication = 2,

        /// <summary>
        /// ReplayDetect
        /// </summary>
        ReplayDetect = 4,

        /// <summary>
        /// SequenceDetect
        /// </summary>
        SequenceDetect = 8,

        /// <summary>
        /// Confidentiality
        /// </summary>
        Confidentiality = 16,

        /// <summary>
        /// UseSessionKey
        /// </summary>
        UseSessionKey = 32,

        /// <summary>
        /// PromptForCreds
        /// </summary>
        PromptForCreds = 64,

        /// <summary>
        /// UseSuppliedCreds
        /// </summary>
        UseSuppliedCreds = 128,

        /// <summary>
        /// AllocMemory
        /// </summary>
        AllocMemory = 256,

        /// <summary>
        /// UseDceStyle
        /// </summary>
        UseDceStyle = 512,

        /// <summary>
        /// Datagram
        /// </summary>
        Datagram = 1024,

        /// <summary>
        /// Connection
        /// </summary>
        Connection = 2048,

        /// <summary>
        /// CallLevel
        /// </summary>
        CallLevel = 4096,

        /// <summary>
        /// FragmentSupplied
        /// </summary>
        FragmentSupplied = 8192,

        /// <summary>
        /// ExtendedError
        /// </summary>
        ExtendedError = 16384,

        /// <summary>
        /// Stream
        /// </summary>
        Stream = 32768,

        /// <summary>
        /// Integrity
        /// </summary>
        Integrity = 65536,

        /// <summary>
        /// Identify
        /// </summary>
        Identify = 131072,
    }

    /// <summary>
    /// The enumeration type of server context attributes. Note that client context
    /// and server context don't share the same attributes, so two ContextAttribute
    /// are defined respectively in ClientSecurityContext and ServerContext
    /// </summary>
    [Flags]
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public enum ServerContextAttribute
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Delegate
        /// </summary>
        Delegate = 1,

        /// <summary>
        /// MutualAuthentication
        /// </summary>
        MutualAuthentication = 2,

        /// <summary>
        /// ReplayDetect
        /// </summary>
        ReplayDetect = 4,

        /// <summary>
        /// SequenceDetect
        /// </summary>
        SequenceDetect = 8,

        /// <summary>
        /// Confidentiality
        /// </summary>
        Confidentiality = 16,

        /// <summary>
        /// UseSessionKey
        /// </summary>
        UseSessionKey = 32,

        /// <summary>
        /// AllocMemory
        /// </summary>
        AllocMemory = 256,

        /// <summary>
        /// UseDceStyle
        /// </summary>
        UseDceStyle = 512,

        /// <summary>
        /// Datagram
        /// </summary>
        Datagram = 1024,

        /// <summary>
        /// Connection
        /// </summary>
        Connection = 2048,

        /// <summary>
        /// CallLevel
        /// </summary>
        CallLevel = 4096,

        /// <summary>
        /// ExtendedError
        /// </summary>
        ExtendedError = 32768,

        /// <summary>
        /// Stream
        /// </summary>
        Stream = 65536,

        /// <summary>
        /// Integrity
        /// </summary>
        Integrity = 131072,

        /// <summary>
        /// Identify
        /// </summary>
        Identify = 524288,
    }

    /// <summary>
    /// Decode role
    /// </summary>
    public enum smb3Role
    {
        /// <summary>
        /// Client decoder
        /// </summary>
        Client,

        /// <summary>
        /// Server decoder
        /// </summary>
        Server
    }

    /// <summary>
    /// Used to identify a file in server
    /// </summary>
    public struct FilePath
    {
        /// <summary>
        /// The name of the server
        /// </summary>
        public string serverName;

        /// <summary>
        /// The name of the share, SMB2_TREEC0NNECT packet
        /// </summary>
        public string shareName;

        /// <summary>
        /// The name of filePath, it is a parameter of SMB2_CREATE packet
        /// </summary>
        public string filePathName;
    }
}

//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3Client
// Description: smb3Client mocks the client functionality of smb3, It is
//              used to create all kinds of packet, send the packet to 
//              server, and receive packets from server.
//-------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;


using Microsoft.Protocols.TestTools.StackSdk.Transport;
using Microsoft.Protocols.TestTools.StackSdk.Security.Sspi;
using Microsoft.Protocols.TestTools.StackSdk.Security.Nlmp;


namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// smb3Client mocks the client functionality of smb3, It is
    /// used to create all kinds of packet, send the packet to 
    /// server, and receive packets from server.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class smb3Client : IDisposable
    {
        //the underlying transport;
        private TransportStack [] transportList = new TransportStack[64];
        private TransportStack transport;
        private bool isConnected;
        public static int connectionId;
        public static int binding;
        public static smb3ClientGlobalContext globalContext = new smb3ClientGlobalContext();

        private smb3Decoder decoder;
        private bool disposed;
        private smb3TransportType transportType;
        private string serverPrincipleName;

        private string lastShareName;

        /// <summary>
        /// The global context contains states about all clients
        /// </summary>
        public virtual smb3ClientGlobalContext GlobalContext
        {
            get
            {
                return globalContext;
            }
        }


        /// <summary>
        /// The connection contains context of this client
        /// </summary>
        public virtual smb3ClientConnection Connection
        {
            get
            {
                return globalContext.connectionTable[connectionId];
            }
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        public smb3Client()
        {
            decoder = new smb3Decoder(smb3Role.Client);
            decoder.globalContext = globalContext;
        }


        /// <summary>
        /// This function need to be called to lock globalContext before accessing the globalContext or smb3ClientConnection.
        /// Otherwise may read dirty data from context.
        /// </summary>
        public virtual void LockGlobalContext()
        {
            globalContext.Lock();
        }


        /// <summary>
        /// This function should be called after accessing the context as soon as possible so other thread can accessing the same 
        /// region.
        /// </summary>
        public virtual void UnlockGlobalContext()
        {
            globalContext.Unlock();
        }


        /// <summary>
        /// Connect to server using specified endpoint.
        /// </summary>
        /// <param name="endpoint">the server ip endpoint</param>
        public virtual int Connect(IPEndPoint endpoint)
        {
            if (isConnected)
            {
              //  throw new InvalidOperationException("smb3 server has already been connected.");
                connectionId++;
            }

            transportType = smb3TransportType.Tcp;
            decoder.TransportType = smb3TransportType.Tcp;

            SocketTransportConfig config = new SocketTransportConfig();
            config.BufferSize = smb3Consts.MaxTcpBufferSize;
            config.RemoteIpAddress = endpoint.Address;
            config.RemoteIpPort = endpoint.Port;
            config.Role = Role.Client;
            config.Type = StackTransportType.Tcp;

            transport = new TransportStack(config, decoder.smb3DecodePacketCallback);
            transport.Connect();
            transportList[connectionId] = transport;
            

            isConnected = true;
            SetHostName(endpoint);

            try
            {
                globalContext.Lock();
                connectionId = globalContext.GenerateNewConnectionId();
                decoder.connectionId = connectionId;
            }
            finally
            {
                globalContext.Unlock();
            }

            //update context
            smb3Event smb3Event = new smb3Event();

            smb3Event.Type = smb3EventType.Connected;
            smb3Event.ConnectionId = connectionId;

            //extraInfo contains server name, transport type name
            smb3Event.ExtraInfo = Encoding.Unicode.GetBytes(serverPrincipleName 
                + smb3Consts.ExtraInfoSeperator + transportType);

            try
            {
                globalContext.Lock();
                globalContext.UpdateContext(smb3Event);
            }
            finally
            {
                globalContext.Unlock();
            }
            return connectionId;
        }


        /// <summary>
        /// Connect to server using server name and server tcp port
        /// </summary>
        /// <param name="serverName">The name of server</param>
        /// <param name="isIpv4">Indicate whether the packet is ipv4 or ipv6</param>
        /// <param name="remotePort">The port of server</param>
        public virtual int Connect(string serverName, bool isIpv4, int remotePort)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(serverName);

            IPAddress suitableAddress = null;
            int currnetConnectionId = 0;
            foreach (IPAddress address in addresses)
            {
                if (isIpv4)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        suitableAddress = address;
                        break;
                    }
                }
                else
                {
                    if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        suitableAddress = address;
                        break;
                    }
                }
            }

            if (suitableAddress == null)
            {
                throw new InvalidOperationException("Can't find suitable ip address based on serverName and ip version, "
                    + "check the network please.");
            }

            currnetConnectionId=Connect(new IPEndPoint(suitableAddress, remotePort));;
            return currnetConnectionId;
        }


        /// <summary>
        /// Connect to server using Netbios
        /// </summary>
        /// <param name="localNetbiosName">The client local netbios name</param>
        /// <param name="serverNetbiosName">Netbios name of server</param>
        public virtual int Connect(string localNetbiosName, string serverNetbiosName)
        {
            if (isConnected)
            {
          //      throw new InvalidOperationException("smb3 server has already been connected.");
            }

            transportType = smb3TransportType.NetBios;
            decoder.TransportType = smb3TransportType.NetBios;

            NetbiosTransportConfig config = new NetbiosTransportConfig();
            config.BufferSize = smb3Consts.MaxNetbiosBufferSize;
            config.MaxNames = smb3Consts.MaxNames;
            config.MaxSessions = smb3Consts.MaxSessions;
            config.LocalNetbiosName = localNetbiosName;
            config.RemoteNetbiosName = serverNetbiosName;
            config.Role = Role.Client;
            config.Type = StackTransportType.Netbios;

            transport = new TransportStack(config, decoder.smb3DecodePacketCallback);

            transport.Connect();

            isConnected = true;
            serverPrincipleName = serverNetbiosName;

            try
            {
                globalContext.Lock();
                connectionId = globalContext.GenerateNewConnectionId();
                decoder.connectionId = connectionId;
            }
            finally
            {
                globalContext.Unlock();
            }

            //update context
            smb3Event smb3Event = new smb3Event();

            smb3Event.Type = smb3EventType.Connected;
            smb3Event.ConnectionId = connectionId;
            smb3Event.ExtraInfo = Encoding.Unicode.GetBytes(serverPrincipleName
                + smb3Consts.ExtraInfoSeperator + transportType);

            try
            {
                globalContext.Lock();
                globalContext.UpdateContext(smb3Event);
                smb3Event.ConnectionId = connectionId;
                return connectionId;
            }
            finally
            {
                globalContext.Unlock();
            }
        }


        /// <summary>
        /// Disconnect with server
        /// </summary>
        public virtual void Disconnect()
        {
            if (isConnected)
            {
                try
                {
                    globalContext.Lock();
                    smb3Event smb3Event = new smb3Event();

                    smb3Event.ConnectionId = connectionId;
                    smb3Event.Type = smb3EventType.Disconnected;

                    globalContext.UpdateContext(smb3Event);
                }
                finally
                {
                    globalContext.Unlock();
                }

                // The transport won't be reused.
                transport.Dispose();
                isConnected = false;
            }
        }


        /// <summary>
        /// Is any data can be readed
        /// </summary>
        public virtual bool IsDataAvailable
        {
            get
            {
                if (isConnected)
                {
                    return transport.IsDataAvailable;
                }
                else
                {
                    return false;
                }
            }
        }


        /// <summary>
        /// Send a packet to server
        /// </summary>
        /// <param name="packet">The packet</param>
        public virtual void SendPacket(smb3Packet packet)
        {
            try
            {
                globalContext.Lock();
                smb3Event smb3Event = new smb3Event();
                smb3Event.ConnectionId = connectionId;
                smb3Event.Packet = packet;
                smb3Event.Type = smb3EventType.PacketSent;

                globalContext.UpdateContext(smb3Event);

                transport = transportList[connectionId];

                //Update context before actually sending packet to server
                //This is because if sending packet first, Decode thread 
                //will not get the updated context.
                SendPacket(packet.ToBytes());
            }
            finally
            {
                globalContext.Unlock();
            }
        }

        /// <summary>
        /// Send packet to client
        /// </summary>
        /// <param name="packet">The packet data</param>
        public virtual void SendPacket(byte[] packet)
        {
            if (transportType == smb3TransportType.NetBios)
            {
                
                transport.SendBytes(packet);
                
            }
            else
            {

                transport.SendBytes(smb3Utility.GenerateTcpTransportPayLoad(packet));
            }
        }


        /// <summary>
        /// Expect a packet from server
        /// </summary>
        /// <param name="timeout">the waiting time</param>
        /// <returns>The packet</returns>
        public virtual smb3Packet ExpectPacket(TimeSpan timeout)
        {

            //smb3Event smb3Event = new smb3Event();
            //smb3Event.ConnectionId = connectionId;
            //globalContext.UpdateContext(smb3Event);

            TransportEvent transEvent = transport.ExpectTransportEvent(timeout);

            if (transEvent.EventType == EventType.ReceivedPacket)
            {
                smb3Packet packet = (smb3Packet)transEvent.EventObject;
                return packet;
            }
            else if (transEvent.EventType == EventType.Exception)
            {
                throw (Exception)transEvent.EventObject;
            }
            else
            {
                throw new InvalidOperationException("Received an un-expected event, the event type is: " + transEvent.EventType);
            }
        }


        /// <summary>
        /// Set global context
        /// </summary>
        /// <param name="config">The config used to set the global context</param>
        public virtual void SetGlobalContext(smb3ClientGlobalConfig config)
        {
            try
            {
                globalContext.Lock();
                globalContext.requireMessageSigning = config.RequireMessageSigning;
            }
            finally
            {
                globalContext.Unlock();
            }
        }


        /// <summary>
        /// Create SmbNegotiateRequestPacket
        /// </summary>
        /// <param name="dialects">
        /// A enum indicates which dialect should be contained in the negotiate request packet
        /// </param>
        /// <returns>A SmbNegotiateRequestPacket</returns>
        public virtual SmbNegotiateRequestPacket CreateSmbNegotiateRequest(
            params string[] dialects)
        {
            const byte dialectFormatCharactor = 0x02;

            SmbNegotiateRequestPacket packet = new SmbNegotiateRequestPacket();

            //Smb mark: 0XFF, 'S', 'M', 'B'
            packet.Header.Protocol = 0x424d53ff;

            packet.Header.Command = (byte)SmbCommand.Negotiate;

            try
            {
                globalContext.Lock();
                packet.Header.Mid = (ushort)globalContext.connectionTable[connectionId].GetNextSequenceNumber();

                globalContext.connectionTable[connectionId].globalRequireMessageSignCopy =
                    globalContext.requireMessageSigning;
            }
            finally
            {
                globalContext.Unlock();
            }

            using (MemoryStream ms = new MemoryStream())
            {
                foreach (string dialect in dialects)
                {
                    ms.Write(new byte[] { dialectFormatCharactor }, 0, sizeof(byte));
                    byte[] dialectArray = Encoding.ASCII.GetBytes(dialect + '\0');
                    ms.Write(dialectArray, 0, dialectArray.Length);

                }

                packet.PayLoad.DialectName = ms.ToArray();
            }

            packet.PayLoad.ByteCount = (ushort)packet.PayLoad.DialectName.Length;

            return packet;
        }


        /// <summary>
        /// Create smb3NegotiateRequestPacket
        /// </summary>
        /// <param name="dialects">An array of one or more supported dialect revision numbers.</param>
        /// <returns>A smb3NegotiateRequestPacket</returns>
        public virtual smb3NegotiateRequestPacket CreateNegotiateRequest(
            NEGOTIATE_Request_Capabilities_Values Capabilities, params ushort[] dialects  
            )
        {
            smb3NegotiateRequestPacket packet = new smb3NegotiateRequestPacket();

            packet.PayLoad.Capabilities = Capabilities;
            SetHeader(packet, smb3Command.NEGOTIATE, 0, 0);

            try
            {
                globalContext.Lock();
                globalContext.connectionTable[connectionId].globalRequireMessageSignCopy =
                    globalContext.requireMessageSigning;

                if (globalContext.requireMessageSigning)
                {
                    packet.PayLoad.SecurityMode = SecurityMode_Values.NEGOTIATE_SIGNING_REQUIRED;
                }
                else
                {
                    packet.PayLoad.SecurityMode = SecurityMode_Values.NEGOTIATE_SIGNING_ENABLED;
                }

                //2.1 feature
               
                if (dialects != null)
                {
                    for (int i = 0; i < dialects.Length; i++)
                    {
                        if (dialects[i] == smb3Consts.SMB2_1Dialect)
                        {
                            packet.PayLoad.ClientGuid = globalContext.ClientGuid;
                        }
                    }
                }
            }
            finally
            {
                globalContext.Unlock();
            }

            packet.Header.Command = smb3Command.NEGOTIATE;
            //Set ProcessId to 0xFEFF
            packet.Header.ProcessId = 0xFEFF;

            packet.PayLoad.ClientGuid = smb3Client.globalContext.ClientGuid;    //new Guid("00000000000000000000000000000000");//new Guid(smb3Utility.CreateGuid());//smb3Client.globalContext.ClientGuid; 
            packet.PayLoad.StructureSize = NEGOTIATE_Request_StructureSize_Values.V1;

            if (dialects != null)
            {
                packet.PayLoad.Dialects = new ushort[dialects.Length];

                Array.Copy(dialects, packet.PayLoad.Dialects, dialects.Length);
            }
            else
            {
                packet.PayLoad.Dialects = new ushort[0];
            }

            packet.PayLoad.DialectCount = (ushort)packet.PayLoad.Dialects.Length;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3SessionSetupRequestPacket, this is first round session setup, and should be
        /// use after receiving Negotiate response.
        /// </summary>
        /// <param name="previousSessionId">A previously established session identifier, this field is 0 for new autenticate,
        /// and must be set for re-authenticate</param>
        /// <param name="capability">Specifies protocol capabilities for the client.</param>
        /// <param name="securityPackage">Indicate the security protocol to be used</param>
        /// <param name="contextAttribute">The security context is used by underlying gss-api</param>
        /// <param name="userName">The username of the client, the userName can't prefix domain, the user name such as 
        /// "fareast\userName" is not a valid userName, for this function, just use user name</param>
        /// <param name="domain">The domain of user account</param>
        /// <param name="password">The password of the client</param>
        /// <param name="useServerTokenInNegotiateResponse">Indicate whether use the security token in Negotiate response</param>
        /// <returns>A smb3SessionSetupRequestPacket</returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public virtual smb3SessionSetupRequestPacket CreateFirstSessionSetupRequest(
            ulong previousSessionId,
            byte BindFlag,
            SESSION_SETUP_Request_Capabilities_Values capability,
            SecurityPackage securityPackage,
            ClientContextAttribute contextAttribute,
            string domain,
            string userName,
            string password,
            bool useServerTokenInNegotiateResponse
            )
        {
            smb3SessionSetupRequestPacket packet = new smb3SessionSetupRequestPacket();

           
           
            // The ProcessId field MUST be set to 0xFEFF
            SetHeader(packet, smb3Command.SESSION_SETUP, previousSessionId, 0);
            packet.Header.ProcessId = 0xfeff;

            packet.PayLoad.StructureSize = SESSION_SETUP_Request_StructureSize_Values.V1;
                packet.PayLoad.VcNumber = BindFlag;
            if (BindFlag == 1)
            {
                packet.Header.SessionId = previousSessionId;
                packet.PayLoad.PreviousSessionId = 0;
                packet.Header.Flags |= Packet_Header_Flags_Values.FLAGS_SIGNED;
               

                for (int i = 0; i <= globalContext.connectionTable.Count; i++)
                {
                    if (globalContext.connectionTable[i].sessionTable.ContainsKey(previousSessionId))
                    {
                        (packet as smb3SinglePacket).SessionKey = globalContext.connectionTable[i].sessionTable[previousSessionId].sessionKey;
                        break;
                    }
                }

                
            }
            else
            {
                packet.PayLoad.PreviousSessionId = 0;//previousSessionId;

            } 
            try
            {
                globalContext.Lock();
                if (globalContext.requireMessageSigning)
                {
                    packet.PayLoad.SecurityMode = SESSION_SETUP_Request_SecurityMode_Values.NEGOTIATE_SIGNING_REQUIRED;
                }
                else
                {
                    packet.PayLoad.SecurityMode = SESSION_SETUP_Request_SecurityMode_Values.NEGOTIATE_SIGNING_ENABLED;
                }
            }
            finally
            {
                globalContext.Unlock();
            }

            packet.PayLoad.Capabilities = capability;


            SecurityPackageType packageType = SecurityPackageType.Negotiate;

            if (securityPackage == SecurityPackage.Kerberos)
            {
                packageType = SecurityPackageType.Kerberos;
            }
            else if (securityPackage == SecurityPackage.Nlmp)
            {
                packageType = SecurityPackageType.Ntlm;
            }
            else if (securityPackage == SecurityPackage.Negotiate)
            {
                packageType = SecurityPackageType.Negotiate;
            }
            else
            {
                throw new ArgumentException("Only Kerberos, NTLM, Negotiate is supported");
            }

            try
            {
                globalContext.Lock();
                AccountCredential credential = null;

                if (securityPackage == SecurityPackage.Nlmp)
                {
                    NlmpClientCredential nlmpCredential = new NlmpClientCredential(serverPrincipleName, domain, userName, password);
                    globalContext.connectionTable[connectionId].Gss = new NlmpClientSecurityContext(nlmpCredential);

                    if (!useServerTokenInNegotiateResponse)
                    {
                        globalContext.connectionTable[connectionId].Gss.Initialize(null);
                    }
                    else
                    {
                        throw new ArgumentException("useServerTokenInNegotiateResponse should not be false for NLMP securityPacket",
                            "useServerTokenInNegotiateResponse");
                    }
                }
                else
                {
                    credential = new AccountCredential(domain, userName, password);

                    globalContext.connectionTable[connectionId].Gss = new SspiClientSecurityContext(packageType,
                        credential, serverPrincipleName, (ClientSecurityContextAttribute)contextAttribute,
                        SecurityTargetDataRepresentation.SecurityNativeDrep);

                    if (!useServerTokenInNegotiateResponse)
                    {
                        globalContext.connectionTable[connectionId].Gss.Initialize(null);
                    }
                    else if (packageType == SecurityPackageType.Negotiate)
                    {
                        globalContext.connectionTable[connectionId].Gss.Initialize(
                            globalContext.connectionTable[connectionId].gssNegotiateToken);
                    }
                    else
                    {
                        throw new InvalidOperationException("kerberos does not support using the token in smb3NegotiatePacket");
                    }
                }

                packet.PayLoad.Buffer = globalContext.connectionTable[connectionId].Gss.Token;
            }
            finally
            {
                globalContext.Unlock();
            }

            packet.PayLoad.SecurityBufferOffset = smb3Consts.SecurityBufferOffsetInNegotiateRequest;
            packet.PayLoad.SecurityBufferLength = (ushort)packet.PayLoad.Buffer.Length;

            if (BindFlag != 1)
                 packet.Sign();
            else
             packet.Sign(true);
            

            return packet;
        }


        /// <summary>
        /// Create smb3SessionSetupRequestPacket for second round, this is packet is used 
        /// when client receive packet indicating "NEED_MORE_PROCESS"
        /// </summary>
        /// <param name="sessionId">The sessionId of the last session setup response</param>
        /// <returns>A smb3SessionSetupRequestPacket</returns>
        public virtual smb3SessionSetupRequestPacket CreateSecondSessionSetupRequest(
            ulong sessionId, 
            byte BindFlag
            )
        {
            smb3SessionSetupRequestPacket packet = new smb3SessionSetupRequestPacket();

            try
            {
                globalContext.Lock();

                Dictionary<ulong, smb3OutStandingRequest> outstandingRequests =
                    globalContext.connectionTable[connectionId].outstandingRequests;

                ulong[] keys = new ulong[outstandingRequests.Count];

                outstandingRequests.Keys.CopyTo(keys, 0);

                for (int i = (keys.Length - 1); i >= 0; i--)
                {
                    smb3SessionSetupRequestPacket lastSessionSetupPacket = outstandingRequests[keys[i]].request
                        as smb3SessionSetupRequestPacket;

                    if (lastSessionSetupPacket != null)
                    {
                        packet.Header = lastSessionSetupPacket.Header;
                        packet.PayLoad = lastSessionSetupPacket.PayLoad;
                        break;
                    }
                }


                packet.Header.SessionId = sessionId;
            packet.Header.MessageId = globalContext.connectionTable[connectionId].GetNextSequenceNumber();
            byte[] lastSecurityResponseToken =
                globalContext.connectionTable[connectionId].sessionResponses[sessionId].PayLoad.Buffer;
            globalContext.connectionTable[connectionId].Gss.Initialize(lastSecurityResponseToken);
            packet.PayLoad.Buffer = globalContext.connectionTable[connectionId].Gss.Token;
            packet.PayLoad.SecurityBufferLength = (ushort)packet.PayLoad.Buffer.Length;
         
            }
            finally
            {
                globalContext.Unlock();
            }

            if (BindFlag == 1)
            {
                packet.PayLoad.PreviousSessionId = 0;
                packet.Header.Flags |= Packet_Header_Flags_Values.FLAGS_SIGNED;

                for (int i = 0; i <= globalContext.connectionTable.Count; i++)
                {
                    if (globalContext.connectionTable[i].sessionTable.ContainsKey(sessionId))
                    {
                        (packet as smb3SinglePacket).SessionKey = globalContext.connectionTable[i].sessionTable[sessionId].sessionKey;
                        break;
                    }
                }
            }
            if (BindFlag != 1)
            {
                packet.Sign();
            }
            else
                packet.Sign(true); 

            return packet;
        }


        /// <summary>
        /// Create smb3LogOffRequestPacket
        /// </summary>
        /// <param name="sessionId">Uniquely identifies the established session for the command</param>
        /// <returns>A smb3LogOffRequestPacket</returns>
        public virtual smb3LogOffRequestPacket CreateLogOffRequest(
            ulong sessionId
            )
        {
            smb3LogOffRequestPacket packet = new smb3LogOffRequestPacket();

            SetHeader(packet, smb3Command.LOGOFF, sessionId, 0);

            //The ProcessId field is set to 0xFEFF
            packet.Header.ProcessId = 0xfeff;

            packet.PayLoad.Reserved = LOGOFF_Request_Reserved_Values.V1;
            packet.PayLoad.StructureSize = LOGOFF_Request_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3TreeConnectRequestPacket
        /// </summary>
        /// <param name="sessionId">Uniquely identifies the established session for the command</param>
        /// <param name="path">the path name of the share, server name is not include</param>
        /// <returns>A smb3TreeConnectRequestPacket</returns>
        public virtual smb3TreeConnectRequestPacket CreateTreeConnectRequest(
            ulong sessionId,
            string path
            )
        {
            lastShareName = path;

            smb3TreeConnectRequestPacket packet = new smb3TreeConnectRequestPacket();
            //removed the sessionId and replaced that with 0 for TDI 67761
            SetHeader(packet, smb3Command.TREE_CONNECT, sessionId, 0);
            //SetHeader(packet, smb3Command.TREE_CONNECT, 0 , 0);
            
            packet.Header.ProcessId = 0xfeff;
            packet.PayLoad.Buffer = Encoding.Unicode.GetBytes("\\\\" + serverPrincipleName + "\\" + path);
            packet.PayLoad.StructureSize = TREE_CONNECT_Request_StructureSize_Values.V1;
            packet.PayLoad.Reserved = TREE_CONNECT_Request_Reserved_Values.V1;
            packet.PayLoad.PathOffset = smb3Consts.TreeConnectPathOffset;
            packet.PayLoad.PathLength = (ushort)packet.PayLoad.Buffer.Length;

            packet.Sign();

            return packet;
        }

        public virtual smb3TreeConnectRequestPacket CreateTreeConnectRequest(
           ulong sessionId,
           string ServerName,
           string path
           )
        {
            lastShareName = path;

            smb3TreeConnectRequestPacket packet = new smb3TreeConnectRequestPacket();

            SetHeader(packet, smb3Command.TREE_CONNECT, sessionId, 0);

            packet.Header.ProcessId = 0xfeff;
            packet.PayLoad.Buffer = Encoding.Unicode.GetBytes("\\\\" + ServerName + "\\" + path);
            packet.PayLoad.StructureSize = TREE_CONNECT_Request_StructureSize_Values.V1;
            packet.PayLoad.Reserved = TREE_CONNECT_Request_Reserved_Values.V1;
            packet.PayLoad.PathOffset = smb3Consts.TreeConnectPathOffset;
            packet.PayLoad.PathLength = (ushort)packet.PayLoad.Buffer.Length;

            packet.Sign();

            return packet;
        }

        /// <summary>
        /// Create smb3TreeDisconnectRequestPacket
        /// </summary>
        /// <param name="sessionId">Uniquely identifies the established session for the command</param>
        /// <param name="treeId">Uniquely identifies the tree connect for the command</param>
        /// <returns>A smb3TreeDisconnectRequestPacket</returns>
        public virtual smb3TreeDisconnectRequestPacket CreateTreeDisconnectRequest(
            ulong sessionId,
            uint treeId
            )
        {
            smb3TreeDisconnectRequestPacket packet = new smb3TreeDisconnectRequestPacket();

            SetHeader(packet, smb3Command.TREE_DISCONNECT, sessionId, treeId);

            packet.Header.ProcessId = 0xfeff;
            packet.PayLoad.StructureSize = TREE_DISCONNECT_Request_StructureSize_Values.V1;
            packet.PayLoad.Reserved = TREE_DISCONNECT_Request_Reserved_Values.V1;

            packet.Sign();

            return packet;
        }

        /// <summary>
        /// Create smb3CreateRequestPacket. NOTE: This function will ignore 
        /// LeaseKey in SMB2_CREATE_REQUEST_LEASE create context.
        /// </summary>
        /// <param name="sessionId">Uniquely identifies the established session for the command</param>
        /// <param name="treeId">Uniquely identifies the tree connect for the command</param>
        /// <param name="requestedOplockLevel">The requested oplock level</param>
        /// <param name="impersonationLevel">
        /// This field specifies the impersonation level of the application
        /// that is issuing the create request
        /// </param>
        /// <param name="desiredAccess">The level of access that is required</param>
        /// <param name="fileAttributes">
        /// This field MUST be a combination of the values specified in [MS-FSCC] section 2.6
        /// </param>
        /// <param name="shareAccess">Specifies the sharing mode for the open</param>
        /// <param name="createDisposition">
        /// Defines the action the server MUST take 
        /// if the file that is specified in the name field already exists</param>
        /// <param name="createOptions">Specifies the options to be applied when creating or 
        /// opening the file. Combinations of the bit positions listed below are valid, 
        /// unless otherwise noted</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="createContexts">create contexts, LeaseKey in SMB2_CREATE_REQUEST_LEASE will be ignored,
        /// Sdk will generate the lease key</param>
        /// <returns>A smb3CreateRequestPacket</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public virtual smb3CreateRequestPacket CreateCreateRequest(bool replay,
            ulong sessionId,
            uint treeId,
            RequestedOplockLevel_Values requestedOplockLevel,
            ImpersonationLevel_Values impersonationLevel,
            uint desiredAccess,
            File_Attributes fileAttributes,
            ShareAccess_Values shareAccess,
            CreateDisposition_Values createDisposition,
            CreateOptions_Values createOptions,
            string fileName,
            params CREATE_CONTEXT_Values[] createContexts
            )
        {
            #region Add File object to GlobalFileTable

            byte[] leaseKey = null;

            //Add a file object to global file table
            try
            {
                globalContext.Lock();

                smb3ClientConnection connection = globalContext.connectionTable[connectionId];
                if (connection.supportLeasing)
                {
                    string shareName = string.Empty;
                    if (connection.sessionTable.ContainsKey(sessionId)
                        && connection.sessionTable[sessionId].treeConnectTable.ContainsKey(treeId))
                    {
                        shareName = connection.sessionTable[sessionId].treeConnectTable[treeId].shareName;
                    }
                    else
                    {
                        shareName = lastShareName;
                    }

                    leaseKey = globalContext.AddFile(serverPrincipleName, shareName, fileName);
                }
            }
            finally
            {
                globalContext.Unlock();
            }

            #endregion

            smb3CreateRequestPacket packet = new smb3CreateRequestPacket();

            SetHeader(packet, smb3Command.CREATE, sessionId, treeId);
            if (replay)
                packet.Header.Flags = Packet_Header_Flags_Values.SMB2_FLAGS_REPLAY_OPERATION;
            packet.PayLoad.StructureSize = CREATE_Request_StructureSize_Values.V1;
            packet.PayLoad.RequestedOplockLevel = requestedOplockLevel;
            packet.PayLoad.ImpersonationLevel = impersonationLevel;
            packet.PayLoad.DesiredAccess = new _ACCESS_MASK();
            packet.PayLoad.DesiredAccess.ACCESS_MASK = desiredAccess;
            packet.PayLoad.FileAttributes = fileAttributes;
            packet.PayLoad.ShareAccess = shareAccess;
            packet.PayLoad.CreateDisposition = createDisposition;
            packet.PayLoad.CreateOptions = createOptions;

            #region construct buffer

            byte[] fileNameByteArray = null;

            if (fileName == null)
            {
                fileNameByteArray = new byte[0];
                packet.PayLoad.NameOffset = smb3Consts.NameOffsetInCreateRequestPacket;
            }
            else
            {
                fileNameByteArray = Encoding.Unicode.GetBytes(fileName);
                packet.PayLoad.NameOffset = smb3Consts.NameOffsetInCreateRequestPacket;
                packet.PayLoad.NameLength = (ushort)fileNameByteArray.Length;
            }

            if (createContexts == null)
            {
                packet.PayLoad.CreateContextsOffset = 0;
                packet.PayLoad.CreateContextsLength = 0;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                int index, indexOriginal;
                index = indexOriginal = smb3Consts.NameOffsetInCreateRequestPacket;

                int alignedLen = 0;
                int nextContextOffsetFromHeader = 0;

                ms.Write(fileNameByteArray, 0, fileNameByteArray.Length);

                index = indexOriginal + (int)ms.Position;

                if (createContexts != null)
                {
                    packet.PayLoad.CreateContextsOffset = (uint)(smb3Consts.NameOffsetInCreateRequestPacket +
                        smb3Utility.AlignBy8Bytes(fileNameByteArray.Length));

                    nextContextOffsetFromHeader = (int)packet.PayLoad.CreateContextsOffset;

                    PaddingZero(ms, (int)(packet.PayLoad.CreateContextsOffset - index));

                    for (int i = 0; i < createContexts.Length; i++)
                    {
                        CreateContextTypeValue contextType = smb3Utility.GetContextType(createContexts[i]);

                        if (contextType == CreateContextTypeValue.SMB2_CREATE_REQUEST_LEASE
                            && leaseKey != null)
                        {
                            //lease key is put in the first 16 bytes of SMB2_CREATE_REQUEST_LEASE
                            Array.Copy(leaseKey, 0, createContexts[i].Buffer,
                                createContexts[i].DataOffset - createContexts[i].NameOffset,
                                leaseKey.Length);
                        }

                        byte[] createContext = smb3Utility.ToBytes(createContexts[i]);

                        if (i != (createContexts.Length - 1))
                        {
                            alignedLen = smb3Utility.AlignBy8Bytes(createContext.Length);
                            nextContextOffsetFromHeader += alignedLen;

                            byte[] nextValue = BitConverter.GetBytes(alignedLen);
                            Array.Copy(nextValue, createContext, nextValue.Length);

                            ms.Write(createContext, 0, createContext.Length);

                            index = indexOriginal + (int)ms.Position;

                            PaddingZero(ms, nextContextOffsetFromHeader - index);
                        }
                        else
                        {
                            nextContextOffsetFromHeader += createContext.Length;
                            ms.Write(createContext, 0, createContext.Length);
                        }
                    }

                    packet.PayLoad.CreateContextsLength = (uint)nextContextOffsetFromHeader - packet.PayLoad.CreateContextsOffset;
                }

                packet.PayLoad.Buffer = ms.ToArray();
            }

            #endregion

            //To pad some bytes to conform with windows implementation
            if (packet.PayLoad.NameLength == 0 && packet.PayLoad.Buffer.Length == 0)
            {
                packet.UnknownPadding = new byte[1];
            }

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3CloseRequestPacket
        /// </summary>
        /// <param name="flags">A Flags field, which indicates how the operation MUST be processed</param>
        /// <param name="fileId">An SMB2_FILEID structure, as specified in section 2.2.14.1
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <returns>A smb3CloseRequestPacket</returns>
        public virtual smb3CloseRequestPacket CreateCloseRequest(
            Flags_Values flags,
            FILEID fileId
            )
        {
            smb3CloseRequestPacket packet = new smb3CloseRequestPacket();

            SetHeader(packet, smb3Command.CLOSE, fileId);

            packet.PayLoad.FileId = fileId;
            packet.PayLoad.StructureSize = CLOSE_Request_StructureSize_Values.V1;
            packet.PayLoad.Flags = flags;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3FlushRequestPacket
        /// </summary>
        /// <param name="fileId">An SMB2_FILEID, as specified in section 2.2.14.1
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <returns>A smb3FlushRequestPacket</returns>
        public virtual smb3FlushRequestPacket CreateFlushRequest(
            FILEID fileId
            )
        {
            smb3FlushRequestPacket packet = new smb3FlushRequestPacket();

            SetHeader(packet, smb3Command.FLUSH, fileId);

            packet.PayLoad.FileId = fileId;
            packet.PayLoad.Reserved1 = Reserved1_Values.V1;
            packet.PayLoad.Reserved2 = FLUSH_Request_Reserved2_Values.V1;
            packet.PayLoad.StructureSize = FLUSH_Request_StructureSize_Values.V1;

            packet.Sign();
            return packet;
        }


        /// <summary>
        /// Create smb3QueryDirectoryRequestPacket
        /// </summary>
        /// <param name="fileInformationClass">The file information class describing the 
        /// format that data MUST be returned in</param>
        /// <param name="flags">Flags indicating how the query directory operation MUST be processed</param>
        /// <param name="fileIndex">Index number received in a previous enumeration from
        ///  where to resume the enumeration.  This MUST be used
        ///  if SMB2_INDEX_SPECIFIED is set in Flags.
        /// </param>
        /// <param name="fileId">An SMB2_FILEID identifier of the directory on which to perform the enumeration
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <param name="outputBufferLength">The maximum number of bytes the server is 
        /// allowed to return in the smb3 QUERY_DIRECTORY Response</param>
        /// <param name="fileName">search pattern for the request</param>
        /// <returns>A smb3QueryDirectoryRequestPacket</returns>
        public virtual smb3QueryDirectoryRequestPacket CreateQueryDirectoryRequest(
            FileInformationClass_Values fileInformationClass,
            QUERY_DIRECTORY_Request_Flags_Values flags,
            uint fileIndex,
            FILEID fileId,
            uint outputBufferLength,
            string fileName
            )
        {
            smb3QueryDirectoryRequestPacket packet = new smb3QueryDirectoryRequestPacket();

            SetHeader(packet, smb3Command.QUERY_DIRECTORY, fileId);

            packet.PayLoad.FileInformationClass = fileInformationClass;
            packet.PayLoad.FileId = fileId;
            packet.PayLoad.FileIndex = fileIndex;
            packet.PayLoad.StructureSize = QUERY_DIRECTORY_Request_StructureSize_Values.V1;
            packet.PayLoad.OutputBufferLength = outputBufferLength;
            packet.PayLoad.Flags = flags;

            if (string.IsNullOrEmpty(fileName))
            {
                packet.PayLoad.Buffer = new byte[0];
            }
            else
            {
                packet.PayLoad.Buffer = Encoding.Unicode.GetBytes(fileName);
                packet.PayLoad.FileNameLength = (ushort)packet.PayLoad.Buffer.Length;
                packet.PayLoad.FileNameOffset = smb3Consts.FileNameOffsetInQueryDirectoryRequest;
            }

            SetCreditCharge(packet, 0, (int)outputBufferLength);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3ReadRequestPacket
        /// </summary>
        /// <param name="length">The length, in bytes, of the data to read from the specified file or pipe</param>
        /// <param name="offset">The offset, in bytes, into the file from which the data MUST be read</param>
        /// <param name="fileId">An SMB2_FILEID, as specified in section 2.2.14.1
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <param name="minimumCount">The minimum number of bytes to be read for this operation to be successful</param>
        /// <returns>A smb3ReadRequestPacket</returns>
        public virtual smb3ReadRequestPacket CreateReadRequest(
            uint length,
            ulong offset,
            FILEID fileId,
            uint minimumCount
            )
        {
            smb3ReadRequestPacket packet = new smb3ReadRequestPacket();

            SetHeader(packet, smb3Command.READ, fileId);

            packet.PayLoad.Channel = Channel_Values.V1;
            packet.PayLoad.FileId = fileId;
            packet.PayLoad.Length = length;
            packet.PayLoad.Offset = offset;
            packet.PayLoad.MinimumCount = minimumCount;
            packet.PayLoad.ReadChannelInfoLength = ReadChannelInfoLength_Values.V1;
            packet.PayLoad.ReadChannelInfoOffset = ReadChannelInfoOffset_Values.V1;
            packet.PayLoad.Reserved = READ_Request_Reserved_Values.V1;
            packet.PayLoad.StructureSize = READ_Request_StructureSize_Values.V1;
            //The client MUST set one byte of this field to 0, and the server MUST ignore it on receipt.
            packet.PayLoad.Buffer = new byte[1];

            SetCreditCharge(packet, 0, (int)length);

            packet.Sign();

            return packet;
        }

        public virtual smb3ReadRequestPacket CreateReadRequest(
            ulong sessionId,
            uint treeId,
            uint length,
            ulong offset,
            FILEID fileId,
            uint minimumCount
            )
        {
            smb3ReadRequestPacket packet = new smb3ReadRequestPacket();

            SetHeader(packet, smb3Command.READ, sessionId, treeId, fileId);

            packet.PayLoad.Channel = Channel_Values.V1;
            packet.PayLoad.FileId = fileId;
            packet.PayLoad.Length = length;
            packet.PayLoad.Offset = offset;
            packet.PayLoad.MinimumCount = minimumCount;
            packet.PayLoad.ReadChannelInfoLength = ReadChannelInfoLength_Values.V1;
            packet.PayLoad.ReadChannelInfoOffset = ReadChannelInfoOffset_Values.V1;
            packet.PayLoad.Reserved = READ_Request_Reserved_Values.V1;
            packet.PayLoad.StructureSize = READ_Request_StructureSize_Values.V1;
            //The client MUST set one byte of this field to 0, and the server MUST ignore it on receipt.
            packet.PayLoad.Buffer = new byte[1];

            SetCreditCharge(packet, 0, (int)length);

            packet.Sign();

            return packet;
        }

        /// <summary>
        /// Create smb3WriteRequestPacket
        /// </summary>
        /// <param name="offset">The offset, in bytes, of where to write the data in the destination file</param>
        /// <param name="fileId">An SMB2_FILEID, as specified in section 2.2.14.1
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <param name="remainingBytes">The number of subsequent bytes the client intends to 
        /// write to the file after this operation completes</param>
        /// <param name="buffer">A variable-length buffer that contains the data to write and the write channel information</param>
        /// <returns>A smb3WriteRequestPacket</returns>
        public virtual smb3WriteRequestPacket CreateWriteRequest(
            ulong offset,
            FILEID fileId,
            uint remainingBytes,
            byte[] buffer
            )
        {
            smb3WriteRequestPacket packet = new smb3WriteRequestPacket();

            SetHeader(packet, smb3Command.WRITE, fileId);

            packet.PayLoad.WriteChannelInfoOffset = WriteChannelInfoOffset_Values.V1;
            packet.PayLoad.WriteChannelInfoLength = WriteChannelInfoLength_Values.V1;
            packet.PayLoad.StructureSize = WRITE_Request_StructureSize_Values.V1;
            packet.PayLoad.RemainingBytes = remainingBytes;
            packet.PayLoad.Offset = offset;
            packet.PayLoad.FileId = fileId;
            packet.PayLoad.Flags = WRITE_Request_Flags_Values.V1;

            if (buffer == null)
            {
                packet.PayLoad.DataOffset = 0;
                packet.PayLoad.Length = 0;
                
                packet.PayLoad.Buffer = new byte[0];
            }
            else
            {
                packet.PayLoad.DataOffset = smb3Consts.DataOffsetInWriteRequest;
                packet.PayLoad.Length = (uint)buffer.Length;
                packet.PayLoad.Buffer = buffer;
            }

            SetCreditCharge(packet, (int)packet.PayLoad.Length, 0);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3OpLockBreakAckPacket
        /// </summary>
        /// <param name="oplockLevel">The resulting oplock level</param>
        /// <param name="fileId">An SMB2_FILEID, as specified in section 2.2.14.1
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <returns>A smb3OpLockBreakAckPacket</returns>
        public virtual smb3OpLockBreakAckPacket CreateOplockBreakAckRequest(
            OPLOCK_BREAK_Acknowledgment_OplockLevel_Values oplockLevel,
            FILEID fileId
            )
        {
            smb3OpLockBreakAckPacket packet = new smb3OpLockBreakAckPacket();

            SetHeader(packet, smb3Command.OPLOCK_BREAK, fileId);

            //The client MUST set ProcessId to 0xFEFF.
            packet.Header.ProcessId = 0xFEFF;
            packet.PayLoad.FileId = fileId;
            packet.PayLoad.Reserved = OPLOCK_BREAK_Acknowledgment_Reserved_Values.V1;
            packet.PayLoad.Reserved2 = OPLOCK_BREAK_Acknowledgment_Reserved2_Values.V1;
            packet.PayLoad.StructureSize = OPLOCK_BREAK_Acknowledgment_StructureSize_Values.V1;

            try
            {
                globalContext.Lock();

                packet.PayLoad.OplockLevel = oplockLevel;
            }
            finally
            {
                globalContext.Unlock();
            }

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3LeaseBreakAckPacket
        /// </summary>
        /// <param name="leaseKey">A unique key which identifies the owner of the lease</param>
        /// <param name="leaseState">The lease state in the Lease Break Acknowledgment message 
        /// MUST be a subset of the lease state granted by the server via the preceding Lease Break
        /// Notification message</param>
        /// <returns>A smb3LeaseBreakAckPacket</returns>
        public virtual smb3LeaseBreakAckPacket CreateLeaseBreakAckRequest(
            byte[] leaseKey,
            LeaseStateValues leaseState
            )
        {
            smb3LeaseBreakAckPacket packet = new smb3LeaseBreakAckPacket();

            FILEID fileId;

            try
            {
                globalContext.Lock();

                fileId = globalContext.FindFileIdByLeaseKey(leaseKey);
            }
            finally
            {
                globalContext.Unlock();
            }

            SetHeader(packet, smb3Command.OPLOCK_BREAK, fileId);

            //The client MUST set ProcessId to 0xFEFF
            packet.Header.ProcessId = 0xFEFF;

            packet.PayLoad.Flags = LEASE_BREAK_Acknowledgment_Packet_Flags_Values.V1;
            packet.PayLoad.LeaseDuration = LEASE_BREAK_Acknowledgment_Packet_LeaseDuration_Values.V1;
            packet.PayLoad.LeaseKey = leaseKey;
            packet.PayLoad.LeaseState = leaseState;
            packet.PayLoad.Reserved = LEASE_BREAK_Acknowledgment_Reserved_Values.V1;
            packet.PayLoad.StructureSize = LEASE_BREAK_Acknowledgment_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }

        
        /// <summary>
        /// Create smb3LockRequestPacket
        /// </summary>
        /// <param name="fileId">An SMB2_FILEID that identifies the file on which to perform
        /// the byte range locks or unlocks
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <param name="locks">An array of LockCount (SMB2_LOCK_ELEMENT) structures
        /// that define the ranges to be locked or unlocked</param>
        /// <returns>A smb3LockRequestPacket</returns>
        public virtual smb3LockRequestPacket CreateLockRequest(
            FILEID fileId,
            uint LockSequence,  //prasanna
            LOCK_ELEMENT[] locks
            )
        {
            if (locks == null || locks.Length == 0)
            {
                throw new ArgumentException("The count of locks must be greater than or equal to 1", "locks");
            }

            smb3LockRequestPacket packet = new smb3LockRequestPacket();

            SetHeader(packet, smb3Command.LOCK, fileId);

            packet.PayLoad.FileId = fileId;
            packet.PayLoad.LockCount = (ushort)locks.Length;
            packet.PayLoad.Locks = locks;
            packet.PayLoad.StructureSize = LOCK_Request_StructureSize_Values.V1;
            packet.PayLoad.LockSequence = LockSequence; //prasanna

            //try
            //{
            //    globalContext.Lock();

            //    int foundedBucketIndex = globalContext.GetFirstFreeOperationBucketIndex(fileId.Persistent);

            //    smb3ClientOpen open = globalContext.globalOpenTable[fileId.Persistent];

            //    if (foundedBucketIndex != -1)
            //    {
            //        //The LockSequence field of the smb3 lock request MUST be set to ((BucketIndex + 1) << 4) + BucketSequence
            //        packet.PayLoad.LockSequence = (uint)(((foundedBucketIndex + 1) << 4) + open.operationBuckets[foundedBucketIndex].sequenceNumber);

            //        //Increment the sequence number of the element chosen above using MOD 16 arithmetic
            //        open.operationBuckets[foundedBucketIndex].sequenceNumber = (byte)((open.operationBuckets[foundedBucketIndex].sequenceNumber + 1) % 16);
            //    }
            //    else if (open.resilientHandle)
            //    {
            //        throw new InvalidOperationException("Can't find a operationbreak");
            //    }
            //    else
            //    {
            //        //if open.resilientHandle is false, lockSequence do not need to be set
            //    }
            //}
            //finally
            //{
            //    globalContext.Unlock();
            //}

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3EchoRequestPacket
        /// </summary>
        /// <returns>A smb3EchoRequestPacket</returns>
        public virtual smb3EchoRequestPacket CreateEchoRequest(
            )
        {
            smb3EchoRequestPacket packet = new smb3EchoRequestPacket();

            SetHeader(packet, smb3Command.ECHO, 0, 0);

            packet.PayLoad.Reserved = ECHO_Request_Reserved_Values.V1;
            packet.PayLoad.StructureSize = ECHO_Request_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3CancelRequestPacket
        /// </summary>
        /// <param name="messageId">A value that identifies a message request and response 
        /// uniquely across all messages that are sent on the same SMB 2 Protocol transport
        /// connection</param>
        /// <returns>A smb3CancelRequestPacket</returns>
        public virtual smb3CancelRequestPacket CreateCancelRequest(
            ulong messageId
            )
        {
            smb3CancelRequestPacket packet = new smb3CancelRequestPacket();

            try
            {
                globalContext.Lock();

                smb3SinglePacket requestPacket = globalContext.connectionTable[connectionId].outstandingRequests[messageId].request
                    as smb3SinglePacket;

                //An smb3 CANCEL Request is the only request received by the server 
                //that is not signed and does not contain a sequence number that must be checked.
                packet.Header.Flags = requestPacket.Header.Flags & ~Packet_Header_Flags_Values.FLAGS_SIGNED;
                packet.Header.SessionId = requestPacket.Header.SessionId;
                packet.Header.TreeId = requestPacket.Header.TreeId;
            }
            finally
            {
                globalContext.Unlock();
            }

            packet.Header.ProtocolId = smb3Consts.smb3ProtocolId;
            packet.Header.StructureSize = Packet_Header_StructureSize_Values.V1;
            packet.Header.CreditCharge = smb3Consts.DefaultCreditCharge;
            packet.Header.Command = smb3Command.CANCEL;
            packet.Header.ProcessId = (uint)Thread.CurrentThread.ManagedThreadId;
            packet.Header.Signature = new byte[smb3Consts.SignatureSize];
            packet.Header.MessageId = messageId;

            try
            {
                globalContext.Lock();

                smb3OutStandingRequest outStandingRequest = globalContext.connectionTable[connectionId].outstandingRequests[messageId];

                if (outStandingRequest.isHandleAsync)
                {
                    packet.Header.Flags |= Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND;
                    packet.Header.ProcessId = (uint)outStandingRequest.asyncId;
                    packet.Header.TreeId = (uint)(outStandingRequest.asyncId >> 32);
                }
            }
            finally
            {
                globalContext.Unlock();
            }

            packet.PayLoad.Reserved = CANCEL_Request_Reserved_Values.V1;
            packet.PayLoad.StructureSize = CANCEL_Request_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3IOCtlRequestPacket
        /// </summary>
        /// <param name="ctlCode">The control code of the FSCTL/IOCTL method</param>
        /// <param name="fileId">An SMB2_FILEID identifier of the file on which to perform the command
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <param name="maxInputResponse">The maximum number of bytes that the server can return
        /// for the input data in the smb3 IOCTL Response</param>
        /// <param name="maxOutputResponse">The maximum number of bytes that the server can return
        /// for the output data in the smb3 IOCTL Response</param>
        /// <param name="flags">Flags indicating how the operation must be processed</param>
        /// <param name="input">contains input information about the IO control</param>
        /// <returns>A smb3IOCtlRequestPacket</returns>
        public virtual smb3IOCtlRequestPacket CreateIOCtlRequest(
            CtlCode_Values ctlCode,
            FILEID fileId,
            uint maxInputResponse,
            uint maxOutputResponse,
            IOCTL_Request_Flags_Values flags,
            byte[] input
            )
        {
            smb3IOCtlRequestPacket packet = new smb3IOCtlRequestPacket();

            SetHeader(packet, smb3Command.IOCTL, fileId);
            packet.PayLoad.CtlCode = ctlCode;
            packet.PayLoad.FileId = fileId;
            packet.PayLoad.Flags = flags;
            packet.PayLoad.MaxInputResponse = maxInputResponse;
            packet.PayLoad.MaxOutputResponse = maxOutputResponse;

            if (input == null)
            {
                packet.PayLoad.Buffer = new byte[0];
            }
            else
            {
                packet.PayLoad.Buffer = input;
                packet.PayLoad.InputOffset = smb3Consts.InputOffsetInIOCtlRequest;
                packet.PayLoad.InputCount = (uint)input.Length;
            }
            packet.PayLoad.Reserved = IOCTL_Request_Reserved_Values.V1;
            packet.PayLoad.Reserved2 = IOCTL_Request_Reserved2_Values.V1;
            packet.PayLoad.StructureSize = IOCTL_Request_StructureSize_Values.V1;

            SetCreditCharge(packet, (int)packet.PayLoad.InputCount, (int)packet.PayLoad.MaxOutputResponse);
            packet.Sign();

            return packet;
        }

        public virtual smb3IOCtlRequestPacket CreateIOCtlRequest(
            bool isSigned,
            ulong SessionID,
            uint TreeID,
            CtlCode_Values ctlCode,
            FILEID fileId,
            uint maxInputResponse,
            uint maxOutputResponse,
            IOCTL_Request_Flags_Values flags,
            byte[] input
            )
        {
            smb3IOCtlRequestPacket packet = new smb3IOCtlRequestPacket();

            SetHeader(packet, smb3Command.IOCTL, fileId);
            packet.Header.SessionId = SessionID;
            if( isSigned)
                packet.Header.Flags |= Packet_Header_Flags_Values.FLAGS_SIGNED;
            packet.Header.TreeId = TreeID;
            packet.PayLoad.CtlCode = ctlCode;
            packet.PayLoad.FileId = fileId;
            packet.PayLoad.Flags = flags;
            packet.PayLoad.MaxInputResponse = maxInputResponse;
            packet.PayLoad.MaxOutputResponse = maxOutputResponse;

            if (input == null)
            {
                packet.PayLoad.Buffer = new byte[0];
            }
            else
            {
                packet.PayLoad.Buffer = input;
                packet.PayLoad.InputOffset = smb3Consts.InputOffsetInIOCtlRequest;
                packet.PayLoad.InputCount = (uint)input.Length;
                packet.PayLoad.OutputOffset = smb3Consts.OutputOffsetInIOCtlRequest;
            }
            packet.PayLoad.Reserved = IOCTL_Request_Reserved_Values.V1;
            packet.PayLoad.Reserved2 = IOCTL_Request_Reserved2_Values.V1;
            packet.PayLoad.StructureSize = IOCTL_Request_StructureSize_Values.V1;
            if (isSigned)
            {
               
                packet.Header.Flags |= Packet_Header_Flags_Values.FLAGS_SIGNED;


                for (int i = 0; i <= globalContext.connectionTable.Count; i++)
                {
                    if (globalContext.connectionTable[i].sessionTable.ContainsKey(SessionID))
                    {
                        (packet as smb3SinglePacket).SessionKey = globalContext.connectionTable[i].sessionTable[SessionID].sessionKey;
                        break;
                    }
                }


            }

            SetCreditCharge(packet, (int)packet.PayLoad.InputCount, (int)packet.PayLoad.MaxOutputResponse);
            packet.Sign();

            return packet;
        }
        /// <summary>
        /// Create smb3IOCtlRequestPacket for FSCTL_DFS_GET_REFERRALS
        /// </summary>
        /// <param name="sessionId">Uniquely identifies the established session for the command</param>
        /// <param name="treeId">Uniquely identifies the tree connect for the command</param>
        /// <param name="maxOutputResponse">The maximum number of bytes that the server can return
        /// for the output data in the smb3 IOCTL Response</param>
        /// <param name="input">A buffer contains REQ_GET_DFS_REFERRAL structure</param>
        /// <returns>A smb3IOCtlRequestPacket</returns>
        public virtual smb3IOCtlRequestPacket CreateDfsReferralIOCtlRequest(
            ulong sessionId,
            uint treeId,
            uint maxOutputResponse,
            byte[] input
            )
        {
            smb3IOCtlRequestPacket packet = new smb3IOCtlRequestPacket();

            SetHeader(packet, smb3Command.IOCTL, sessionId, treeId);

            packet.PayLoad.CtlCode = CtlCode_Values.FSCTL_DFS_GET_REFERRALS;
            packet.PayLoad.FileId = new FILEID();
            packet.PayLoad.FileId.Volatile = ulong.MaxValue;
            packet.PayLoad.FileId.Persistent = ulong.MaxValue;
            packet.PayLoad.Flags = IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL;
            packet.PayLoad.MaxInputResponse = 0;
            packet.PayLoad.MaxOutputResponse = maxOutputResponse;

            if (input == null)
            {
                packet.PayLoad.Buffer = new byte[0];
            }
            else
            {
                packet.PayLoad.Buffer = input;
                packet.PayLoad.InputOffset = smb3Consts.InputOffsetInIOCtlRequest;
                packet.PayLoad.InputCount = (uint)input.Length;
            }
            packet.PayLoad.Reserved = IOCTL_Request_Reserved_Values.V1;
            packet.PayLoad.Reserved2 = IOCTL_Request_Reserved2_Values.V1;
            packet.PayLoad.StructureSize = IOCTL_Request_StructureSize_Values.V1;

            SetCreditCharge(packet, (int)packet.PayLoad.InputCount, (int)packet.PayLoad.MaxOutputResponse);
            packet.Sign();

            return packet;
        }

        /// <summary>
        /// Create smb3IOCtlRequestPacket for SRV_COPYCHUNK_COPY
        /// </summary>
        /// <param name="fileId">An SMB2_FILEID identifier of the file on which to perform the command
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <param name="maxOutputResponse">The maximum number of bytes that the server can return
        /// for the output data in the smb3 IOCTL Response</param>
        /// <param name="copyChunk">The copy chuck information</param>
        /// <returns>A smb3IOCtlRequestPacket</returns>
        public virtual smb3IOCtlRequestPacket CreateCopyChucnkIOCtlRequest(
            FILEID fileId,
            uint maxOutputResponse,
            SRV_COPYCHUNK_COPY copyChunk
            )
        {
            byte[] input = smb3Utility.ToBytes(copyChunk);

            smb3IOCtlRequestPacket packet = CreateIOCtlRequest(CtlCode_Values.FSCTL_SRV_COPYCHUNK, fileId, 0, maxOutputResponse,
                IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL, input);

            return packet;
        }


        /// <summary>
        /// Create smb3IOCtlRequestPacket for SRV_READ_HASH
        /// </summary>
        /// <param name="fileId">An SMB2_FILEID identifier of the file on which to perform the command
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <param name="maxOutputResponse">The maximum number of bytes that the server can return
        /// for the output data in the smb3 IOCTL Response</param>
        /// <param name="readHash">The readHash information</param>
        /// <returns>A smb3IOCtlRequestPacket</returns>
        public virtual smb3IOCtlRequestPacket CreateReadHashIOCtlRequest(
            FILEID fileId,
            uint maxOutputResponse,
            SRV_READ_HASH_Request readHash
            )
        {
            byte[] input = smb3Utility.ToBytes(readHash);

            smb3IOCtlRequestPacket packet = CreateIOCtlRequest(CtlCode_Values.FSCTL_SRV_READ_HASH, fileId, 0, maxOutputResponse,
                IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL, input);

            return packet;
        }


        /// <summary>
        /// Create smb3IOCtlRequestPacket for NETWORK_RESILIENCY_REQUEST
        /// </summary>
        /// <param name="fileId">An SMB2_FILEID identifier of the file on which to perform the command
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <param name="networkResiliency">The networkResiliency information</param>
        /// <returns>A smb3IOCtlRequestPacket</returns>
        public virtual smb3IOCtlRequestPacket CreateNetworkResiliencyIOCtlRequest(
            FILEID fileId,
            NETWORK_RESILIENCY_Request networkResiliency
            )
        {
            byte[] input = smb3Utility.ToBytes(networkResiliency);

            smb3IOCtlRequestPacket packet = CreateIOCtlRequest(CtlCode_Values.FSCTL_LMR_REQUEST_RESILIENCY, fileId, 0, 0,
                IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL, input);

            return packet;
        }


        /// <summary>
        /// Create smb3ChangeNotifyRequestPacket
        /// </summary>
        /// <param name="flags">Flags indicating how the operation MUST be processed</param>
        /// <param name="outputBufferLength">The maximum number of bytes the server is allowed to return</param>
        /// <param name="fileId">An SMB2_FILEID identifier of the directory to monitor for changes
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <param name="completionFilter">Specifies the types of changes to monitor</param>
        /// <returns>A smb3ChangeNotifyRequestPacket</returns>
        public virtual smb3ChangeNotifyRequestPacket CreateChangeNotifyRequest(
            CHANGE_NOTIFY_Request_Flags_Values flags,
            uint outputBufferLength,
            FILEID fileId,
            CompletionFilter_Values completionFilter
            )
        {
            smb3ChangeNotifyRequestPacket packet = new smb3ChangeNotifyRequestPacket();

            SetHeader(packet, smb3Command.CHANGE_NOTIFY, fileId);

            packet.PayLoad.FileId = fileId;
            packet.PayLoad.CompletionFilter = completionFilter;
            packet.PayLoad.Flags = flags;
            packet.PayLoad.OutputBufferLength = outputBufferLength;
            packet.PayLoad.Reserved = CHANGE_NOTIFY_Request_Reserved_Values.V1;
            packet.PayLoad.StructureSize = CHANGE_NOTIFY_Request_StructureSize_Values.V1;

            SetCreditCharge(packet, 0, (int)outputBufferLength);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3QueryInfoRequestPacket
        /// </summary>
        /// <param name="infoType">The type of information queried</param>
        /// <param name="fileInfoClass">fileInfoClass defined in [MS-FSCC]</param>
        /// <param name="outputBufferLength">The maximum number of bytes of information the server 
        /// can send in the response</param>
        /// <param name="additionalInformation">Provides additional information to the server</param>
        /// <param name="flags">The flags MUST be set to a combination of zero 
        /// or more of these bit values for a FileFullEaInformation query</param>
        /// <param name="fileId">An SMB2_FILEID identifier of the file or named pipe on which to perform the query
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <param name="buffer">A variable-length buffer containing the input buffer for the request,
        /// as described by the InputBufferOffset and InputBufferLength fields</param>
        /// <returns>A smb3QueryInfoRequestPacket</returns>
        public virtual smb3QueryInfoRequestPacket CreateQueryInfoRequest(
            InfoType_Values infoType,
            byte fileInfoClass,
            uint outputBufferLength,
            AdditionalInformation_Values additionalInformation,
            QUERY_INFO_Request_Flags_Values flags,
            FILEID fileId,
            byte[] buffer
            )
        {
            smb3QueryInfoRequestPacket packet = new smb3QueryInfoRequestPacket();

            SetHeader(packet, smb3Command.QUERY_INFO, fileId);

            packet.PayLoad.AdditionalInformation = additionalInformation;
            packet.PayLoad.FileId = fileId;
            packet.PayLoad.FileInfoClass = fileInfoClass;
            packet.PayLoad.Flags = QUERY_INFO_Request_Flags_Values.V1;
            packet.PayLoad.InfoType = infoType;

            if (buffer == null)
            {
                packet.PayLoad.Buffer = new byte[0];
                packet.PayLoad.InputBufferOffset = 0;
            }
            else
            {
                packet.PayLoad.Buffer = buffer;
                packet.PayLoad.InputBufferOffset = smb3Consts.InputBufferOffsetInQueryInfoRequest;
            }

            packet.PayLoad.InputBufferLength = (uint)packet.PayLoad.Buffer.Length;
            packet.PayLoad.OutputBufferLength = outputBufferLength;
            packet.PayLoad.Reserved = QUERY_INFO_Request_Reserved_Values.V1;
            packet.PayLoad.StructureSize = QUERY_INFO_Request_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3QueryInfoRequestPacket for SMB2_QUERY_QUOTA_INFO
        /// </summary>
        /// <param name="outputBufferLength">The maximum number of bytes of information the server 
        /// can send in the response</param>
        /// <param name="fileId">An SMB2_FILEID identifier of the file or named pipe on which to perform the query
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <param name="queryQuotaInfo">The SMB2_QUERY_QUOTA_INFO packet that specifies the quota information to return.</param>
        /// <returns>A smb3QueryInfoRequestPacket</returns>
        public virtual smb3QueryInfoRequestPacket CreateQueryQuotaInfoRequest(
            uint outputBufferLength,
            FILEID fileId,
            QUERY_QUOTA_INFO queryQuotaInfo
            )
        {
            byte[] buffer = smb3Utility.ToBytes(queryQuotaInfo);

            smb3QueryInfoRequestPacket packet = CreateQueryInfoRequest(InfoType_Values.SMB2_0_INFO_QUOTA, 0, outputBufferLength,
                0, QUERY_INFO_Request_Flags_Values.V1, fileId, buffer);

            return packet;
        }


        /// <summary>
        /// Create smb3SetInfoRequestPacket
        /// </summary>
        /// <param name="infoType">The type of information being set</param>
        /// <param name="fileInfoClass">For setting file information, 
        /// this field MUST contain one of the following FILE_INFORMATION_CLASS values, 
        /// as specified in section 3.3.5.21.1 and [MS-FSCC] section 2.4</param>
        /// <param name="additionalInformation">Provides additional information to the server</param>
        /// <param name="fileId">An SMB2_FILEID identifier of the file or named pipe on which 
        /// to perform the set. Set operations for underlying object store and quota information
        /// are directed to the volume on which the file resides
        /// If this packet is a packet in Related CompoundPacket, pass a new FILEID() which is all zero
        /// to this function, sdk will ignore this parameter
        /// </param>
        /// <param name="buffer">A variable-length buffer that contains the information being set for the request, 
        /// as described by the BufferOffset and BufferLength fields</param>
        /// <returns>A smb3SetInfoRequestPacket</returns>
        public virtual smb3SetInfoRequestPacket CreateSetInfoRequest(
            SET_INFO_Request_InfoType_Values infoType,
            byte fileInfoClass,
            SET_INFO_Request_AdditionalInformation_Values additionalInformation,
            FILEID fileId,
            byte[] buffer
            )
        {
            smb3SetInfoRequestPacket packet = new smb3SetInfoRequestPacket();

            SetHeader(packet, smb3Command.SET_INFO, fileId);

            packet.PayLoad.AdditionalInformation = additionalInformation;
            packet.PayLoad.FileId = fileId;
            packet.PayLoad.FileInfoClass = fileInfoClass;
            packet.PayLoad.InfoType = infoType;
            packet.PayLoad.Reserved = SET_INFO_Request_Reserved_Values.V1;
            packet.PayLoad.StructureSize = SET_INFO_Request_StructureSize_Values.V1;

            if (buffer == null)
            {
                packet.PayLoad.BufferOffset = 0;
                packet.PayLoad.Buffer = new byte[0];
            }
            else
            {
                packet.PayLoad.BufferOffset = smb3Consts.BufferOffsetInSetInfoRequest;
                packet.PayLoad.Buffer = buffer;
                packet.PayLoad.BufferLength = (uint)buffer.Length;
            }

            SetCreditCharge(packet, (int)packet.PayLoad.BufferLength, 0);
            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create compound request packet, compound packet contains several single packet
        /// </summary>
        /// <param name="isRelated">Is these packet related</param>
        /// <param name="packets">The single packet</param>
        /// <returns></returns>
        public virtual smb3CompoundPacket CreateCompoundRequest(
            bool isRelated,
            params smb3SinglePacket[] packets
            )
        {
            if (packets == null)
            {
                throw new ArgumentNullException("packets");
            }

            if (packets.Length < 2)
            {
                throw new ArgumentException("The number of packet should be larger than 1", "packets");
            }

            smb3CompoundPacket compoundPacket = new smb3CompoundPacket();
            compoundPacket.Packets = new List<smb3SinglePacket>();

            int messageIndex = 0;

            for (int i = 0; i < packets.Length; i++)
            {
                try
                {
                    globalContext.Lock();

                    //The packets passed in is not correct, the messageId is wrong. 
                    //when creating single packet, there is no enough information to set messageId,
                    //it only can be set here.
                    packets[i].Header.MessageId = globalContext.connectionTable[connectionId].sequenceWindow[messageIndex];

                    if (packets[i].Header.CreditCharge != 0)
                    {
                        messageIndex += packets[i].Header.CreditCharge;
                    }
                    else
                    {
                        messageIndex++;
                    }
                }
                finally
                {
                    globalContext.Unlock();
                }

                if (isRelated)
                {
                    if (i != 0)
                    {
                        //because when create the single packet, the flag is not set. it must be
                        //set here. So does sessionKey. the first packet is a complete packet.
                        //its data can be used to set other packet.
                        packets[i].Header.Flags = packets[0].Header.Flags;
                        packets[i].SessionKey = packets[0].SessionKey;

                        packets[i].Header.Flags |= Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS;
                        packets[i].Header.SessionId = ulong.MaxValue;
                        packets[i].Header.TreeId = uint.MaxValue;
                        packets[i].SetFileIdToMaxValue();
                        packets[i].OuterCompoundPacket = compoundPacket;
                    }
                }

                if (i != (packets.Length - 1))
                {
                    packets[i].Header.NextCommand = (uint)smb3Utility.AlignBy8Bytes(packets[i].ToBytes().Length);
                }
                else
                {
                    packets[i].IsLast = true;
                }

                packets[i].IsInCompoundPacket = true;

                compoundPacket.Packets.Add(packets[i]);
            }

            compoundPacket.Sign();

            return compoundPacket;
        }

        #region help function

        /// <summary>
        /// Set fields in header
        /// </summary>
        /// <param name="packet">The smb3 packet</param>
        /// <param name="command">The command of this packet</param>
        /// <param name="fileId">An SMB2_FILEID structure, as specified in section 2.2.14.1</param>
        private void SetHeader(smb3SinglePacket packet, smb3Command command, FILEID fileId)
        {
            packet.Header.ProtocolId = smb3Consts.smb3ProtocolId;
            packet.Header.StructureSize = Packet_Header_StructureSize_Values.V1;
            packet.Header.CreditCharge = smb3Consts.DefaultCreditCharge;
            packet.Header.Command = command;
            packet.Header.ProcessId = (uint)Thread.CurrentThread.ManagedThreadId;
            packet.Header.Signature = new byte[smb3Consts.SignatureSize];

            try
            {
                globalContext.Lock();

                packet.Header.CreditRequest_47_Response = smb3Utility.CaculateRequestCredits(
                   globalContext.connectionTable[connectionId].sequenceWindow.Count);

             // packet.Header.MessageId = 100; 
              packet.Header.MessageId = globalContext.connectionTable[connectionId].GetNextSequenceNumber();


                Dictionary<FILEID, smb3ClientOpen> openTable = globalContext.connectionTable[connectionId].openTable;

                if (openTable.ContainsKey(fileId))
                {
                    packet.Header.TreeId = openTable[fileId].treeConnect.treeConnectId;
                    packet.Header.SessionId = openTable[fileId].treeConnect.session.sessionId;

                    if (openTable[fileId].treeConnect.session.shouldSign)
                    {
                        packet.Header.Flags |= Packet_Header_Flags_Values.FLAGS_SIGNED;
                        (packet as smb3SinglePacket).SessionKey = openTable[fileId].treeConnect.session.sessionKey;
                    }
                }
                else
                {
                    // Do not throw exception, the packet may be a packet in relative CompoundPacket,
                    // if so, the fileId, sessionId, treeId will not be set here, it will be set when 
                    // calling createCompoundPacket()
                   
                }
            }
            finally
            {
                globalContext.Unlock();
            }
        }

        private void SetHeader(smb3SinglePacket packet, smb3Command command,ulong sessionId,
            uint treeId, FILEID fileId)
        {
            packet.Header.ProtocolId = smb3Consts.smb3ProtocolId;
            packet.Header.StructureSize = Packet_Header_StructureSize_Values.V1;
            packet.Header.CreditCharge = smb3Consts.DefaultCreditCharge;
            packet.Header.Command = command;
            packet.Header.ProcessId = (uint)Thread.CurrentThread.ManagedThreadId;
            packet.Header.Signature = new byte[smb3Consts.SignatureSize];
            int credits;
            try
            {
                globalContext.Lock();

                credits = globalContext.connectionTable[connectionId].sequenceWindow.Count;
                packet.Header.CreditRequest_47_Response = smb3Utility.CaculateRequestCredits(credits);

                packet.Header.MessageId = globalContext.connectionTable[connectionId].GetNextSequenceNumber();
                Dictionary<FILEID, smb3ClientOpen> openTable = globalContext.connectionTable[connectionId].openTable;
                if (globalContext.connectionTable[connectionId].sessionTable.ContainsKey(sessionId))
                {
                    if (globalContext.connectionTable[connectionId].sessionTable[sessionId].treeConnectTable.ContainsKey(treeId))
                    {
                     //   connectionTable[smb3Event.ConnectionId].sessionTable[session.SessionId].treeConnectTable[treeConnect.TreeConnectId].openTable.Add(responsePacket.GetFileId(), open);
                         openTable = globalContext.connectionTable[connectionId].sessionTable[sessionId].treeConnectTable[treeId].openTable;

                    }
                }
 

                if (openTable.ContainsKey(fileId))
                {
                    packet.Header.TreeId = openTable[fileId].treeConnect.treeConnectId;
                    packet.Header.SessionId = openTable[fileId].treeConnect.session.sessionId;

                    if (openTable[fileId].treeConnect.session.shouldSign)
                    {
                        packet.Header.Flags |= Packet_Header_Flags_Values.FLAGS_SIGNED;
                        (packet as smb3SinglePacket).SessionKey = openTable[fileId].treeConnect.session.sessionKey;
                    }
                }
                else
                {
                    // Do not throw exception, the packet may be a packet in relative CompoundPacket,
                    // if so, the fileId, sessionId, treeId will not be set here, it will be set when 
                    // calling createCompoundPacket()

                }
               
            }
            finally
            {
                globalContext.Unlock();
            }
        }

        /// <summary>
        /// Set fields in header
        /// </summary>
        /// <param name="packet">The smb3 packet</param>
        /// <param name="command">The command of this packet</param>
        /// <param name="sessionId">Uniquely identifies the established session for the command</param>
        /// <param name="treeId">Uniquely identifies the tree connect for the command</param>
        private void SetHeader(smb3SinglePacket packet, smb3Command command, ulong sessionId, uint treeId)
        {
            packet.Header.ProtocolId = smb3Consts.smb3ProtocolId;
            packet.Header.StructureSize = Packet_Header_StructureSize_Values.V1;
            packet.Header.CreditCharge = smb3Consts.DefaultCreditCharge;
            packet.Header.Command = command;
            //sessionid commented for tdi67761 to test user session deleted to send some invalid session id
            packet.Header.SessionId = sessionId;
            packet.Header.TreeId = treeId;
            packet.Header.ProcessId = (uint)Thread.CurrentThread.ManagedThreadId;
            packet.Header.Signature = new byte[smb3Consts.SignatureSize];

            try
            {
                globalContext.Lock();

                packet.Header.CreditRequest_47_Response = smb3Utility.CaculateRequestCredits(
                    globalContext.connectionTable[connectionId].sequenceWindow.Count);

                packet.Header.MessageId = globalContext.connectionTable[connectionId].GetNextSequenceNumber();

                if (sessionId != 0)
                {
                    if (globalContext.connectionTable[connectionId].sessionTable.ContainsKey(sessionId))
                    {
                        if (globalContext.connectionTable[connectionId].sessionTable[sessionId].shouldSign)
                        {
                            packet.Header.Flags |= Packet_Header_Flags_Values.FLAGS_SIGNED;
                            (packet as smb3SinglePacket).SessionKey =
                                globalContext.connectionTable[connectionId].sessionTable[sessionId].sessionKey;
                        }

                        //if ((treeId != 0) && !globalContext.connectionTable[connectionId].sessionTable[sessionId].treeConnectTable.ContainsKey(treeId))
                        //{
                        //    throw new ArgumentException("TreeId is not valid");
                        //}
                    }
                    //else
                    //{
                    //    throw new ArgumentException("SessionId is not valid");
                    //}
                }
            }
            finally
            {
                globalContext.Unlock();
            }
        }


        /// <summary>
        /// Set host name based on endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint of server</param>
        private void SetHostName(IPEndPoint endpoint)
        {
            IPHostEntry host = Dns.GetHostEntry(endpoint.Address);

            //if host name has domain, remove it. serverPrincipleName will be used in session setup.
            int pos = host.HostName.IndexOf('.');
            if (pos == -1)
            {
                serverPrincipleName = host.HostName;
            }
            else
            {
                serverPrincipleName = host.HostName.Substring(0, pos);
            }
        }


        /// <summary>
        /// If negotiated protocol is 2.100 and server support multi credits, client should set CreditCharge in header 
        /// currently SMB2_READ, SMB2_WRITE, SMB2_QUERY_Directory, SMB2_CHANGE_NOTIFY use this
        /// </summary>
        /// <param name="packet">The packet to be set</param>
        /// <param name="sendPayloadSize">The input length of packet</param>
        /// <param name="expectedResponsePayloadSize">The expected response data length</param>
        private void SetCreditCharge(smb3SinglePacket packet, int sendPayloadSize, int expectedResponsePayloadSize)
        {
            try
            {
                globalContext.Lock();

                //if the dialect is not 2.100(now the protocol versions are 2.002, 2.100)
                //or server do not support large mtu, or transportType is netbios, do not set CreditCharge
                if (transportType == smb3TransportType.NetBios
                    || Connection.dialect == smb3Consts.NegotiateDialect2_02String
                    || !Connection.supportLargeMtu)
                {
                    return;
                }
            }
            finally
            {
                globalContext.Unlock();
            }

            //CreditCharge >= (max(SendPayloadSize, Expected ResponsePayloadSize) - 1)/ 65536 + 1
            int maxValue = Math.Max(sendPayloadSize, expectedResponsePayloadSize);
            int creditCharge = (maxValue - 1) / 65536 + 1;

            packet.Header.CreditCharge = (ushort)creditCharge;
        }


        /// <summary>
        /// Padding zero at the end of the stream
        /// </summary>
        /// <param name="ms">The memory stream</param>
        /// <param name="number">Indicate how many zeros will be padding</param>
        private void PaddingZero(MemoryStream ms, int number)
        {
            byte[] zeroArray = new byte[number];

            ms.Write(zeroArray, 0, zeroArray.Length);
        }

        #endregion

        #region IDispose

        /// <summary>
        /// Release all resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Release all resources
        /// </summary>
        /// <param name="disposing">Indicate user or GC calling this method</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Disconnect();
                }

                disposed = true;
            }
        }


        /// <summary>
        /// Deconstructor
        /// </summary>
        ~smb3Client()
        {
            Dispose(false);
        }

        #endregion
    }
}

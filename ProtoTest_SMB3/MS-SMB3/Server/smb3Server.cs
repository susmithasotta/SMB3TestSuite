//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3Server
// Description: smb3Server is resposable for expecting client connection, 
//              create packet, send packet and so on.
//-------------------------------------------------------------------------

using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Protocols.TestTools.StackSdk.Transport;
using Microsoft.Protocols.TestTools.StackSdk.Security.Sspi;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The server role of smb3 protocol
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class smb3Server : IDisposable
    {
        private smb3ServerContext context;
        private smb3Decoder decoder;

        private smb3TransportType transportType;
        private TransportStack transport;
        private List<smb3Endpoint> clientEndpoints;
        private int endpointId;

        private bool disposed;

        /// <summary>
        /// The context contain state information of server
        /// </summary>
        public smb3ServerContext Context
        {
            get
            {
                return context; 
            }
        }


        /// <summary>
        /// Indicate whether there is any packet can be read
        /// </summary>
        public bool IsDataAvailable
        {
            get
            {
                return transport.IsDataAvailable;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The config, contains information about transport type, tcp listening port and so on</param>
        public smb3Server(smb3ServerConfig config)
        {
            decoder = new smb3Decoder(smb3Role.Server);
            decoder.TransportType = config.TransportType;

            clientEndpoints = new List<smb3Endpoint>();
            context = new smb3ServerContext();
            context.transportType = config.TransportType;
            context.requireMessageSigning = config.RequireMessageSigning;
            context.isDfsCapable = config.IsDfsCapable;

            transportType = config.TransportType;

            if (transportType == smb3TransportType.NetBios)
            {
                NetbiosTransportConfig netbiosConfig = new NetbiosTransportConfig();
                netbiosConfig.BufferSize = smb3Consts.MaxNetbiosBufferSize;

                netbiosConfig.LocalNetbiosName = config.LocalNetbiosName;
                netbiosConfig.MaxNames = smb3Consts.MaxNames;
                netbiosConfig.MaxSessions = smb3Consts.MaxSessions;
                netbiosConfig.Type = StackTransportType.Netbios;
                netbiosConfig.Role = Role.Server;

                transport = new TransportStack(netbiosConfig, decoder.smb3DecodePacketCallback);
            }
            else if (transportType == smb3TransportType.Tcp)
            {
                SocketTransportConfig socketConfig = new SocketTransportConfig();

                socketConfig.BufferSize = smb3Consts.MaxNetbiosBufferSize;
                socketConfig.MaxConnections = smb3Consts.MaxConnectionNumer;
                socketConfig.LocalIpAddress = IPAddress.Any;
                socketConfig.LocalIpPort = config.ServerTcpListeningPort;
                socketConfig.Role = Role.Server;
                socketConfig.Type = StackTransportType.Tcp;

                transport = new TransportStack(socketConfig, decoder.smb3DecodePacketCallback);
            }
            else
            {
                throw new ArgumentException("config contains invalid transport type", "config");
            }
        }


        /// <summary>
        /// Start listen for client connection
        /// </summary>
        public void StartListening()
        {
            transport.Start();
        }


        /// <summary>
        /// Disconnect the client
        /// </summary>
        /// <param name="endpoint">The endpoint of the client</param>
        public void Disconnect(smb3Endpoint endpoint)
        {
            Disconnect(endpoint, true);
        }


        /// <summary>
        /// Disconnect the client
        /// </summary>
        /// <param name="endpoint">The endpoint of the client</param>
        /// <param name="removeEndpoint">Indicate whether the endpoint should be removed from endpointList</param>
        private void Disconnect(smb3Endpoint endpoint, bool removeEndpoint)
        {
            if (!clientEndpoints.Contains(endpoint))
            {
                throw new ArgumentException("The endpoint is not in the server's client endpoint list.", "endpoint");
            }

            if (removeEndpoint)
            {
                clientEndpoints.Remove(endpoint);
            }

            smb3Event smb3Event = new smb3Event();
            smb3Event.Type = smb3EventType.Disconnected;
            smb3Event.Packet = null;
            smb3Event.ConnectionId = endpoint.EndpointId;

            context.UpdateContext(smb3Event);

            if (transportType == smb3TransportType.NetBios)
            {
                transport.Disconnect(endpoint.SessionId);
            }
            else
            {
                transport.Disconnect(endpoint.RemoteEndpoint);
            }
        }

        /// <summary>
        /// Disconnect all clients
        /// </summary>
        public void Disconnect()
        {
            foreach (smb3Endpoint endpoint in clientEndpoints)
            {
                Disconnect(endpoint, false);
            }

            clientEndpoints.Clear();
        }


        /// <summary>
        /// Expect a client connection
        /// </summary>
        /// <param name="timeOut">The waiting time</param>
        /// <returns>The endpoint of the client</returns>
        public smb3Endpoint ExpectConnection(TimeSpan timeOut)
        {
            TransportEvent transEvent = transport.ExpectTransportEvent(timeOut);

            if (transEvent.EventType != EventType.Connected)
            {
                throw new InvalidOperationException("Reveived an un-expected transport event");
            }

            smb3Endpoint endpoint;

            if (transportType == smb3TransportType.NetBios)
            {
                endpoint = new smb3Endpoint(smb3TransportType.NetBios, null, (byte)transEvent.EndPoint, endpointId++);
            }
            else
            {
                endpoint = new smb3Endpoint(smb3TransportType.Tcp, (IPEndPoint)transEvent.EndPoint, 0, endpointId++);
            }

            smb3Event smb3Event = new smb3Event();
            smb3Event.Type = smb3EventType.Connected;
            smb3Event.Packet = null;
            smb3Event.ConnectionId = endpoint.EndpointId;

            context.UpdateContext(smb3Event);

            clientEndpoints.Add(endpoint);
            return endpoint;
        }


        /// <summary>
        /// Send packet to a client
        /// </summary>
        /// <param name="packet">The packet</param>
        public void SendPacket(smb3Packet packet)
        {
            SendPacket(packet.Endpoint, packet);
        }


        /// <summary>
        /// Send packet to a client specified by the endpoint, this method is for negtive test, for normal use, please use
        /// SendPacket(smb3Packet packet)
        /// </summary>
        /// <param name="endpoint">The client endpoint</param>
        /// <param name="packet">The packet</param>
        public void SendPacket(smb3Endpoint endpoint, smb3Packet packet)
        {
            
            smb3Event smb3Event = new smb3Event();
            smb3Event.ConnectionId = endpoint.EndpointId;
            smb3Event.Packet = packet;
            smb3Event.Type = smb3EventType.PacketSent;

            context.UpdateContext(smb3Event);

            SendPacket(endpoint, packet.ToBytes());
        }


        /// <summary>
        /// Send packet data to client
        /// </summary>
        /// <param name="endpoint">The client endpoint</param>
        /// <param name="packet">The packet data</param>
        public void SendPacket(smb3Endpoint endpoint, byte[] packet)
        {
            if (transportType == smb3TransportType.NetBios)
            {
                transport.SendBytes(endpoint.SessionId, packet);
            }
            else
            {
                transport.SendBytes(endpoint.RemoteEndpoint, smb3Utility.GenerateTcpTransportPayLoad(packet));
            }
        }


        /// <summary>
        /// Expect a packet from a client specified by the endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint of the client</param>
        /// <param name="timeout">The waiting time</param>
        /// <returns>A smb3Packet</returns>
        /// <exception cref="System.InvalidOperationException">throw when receive a packet which does not pass signature check</exception>
        /// <exception cref="System.InvalidOperationException">Receive unexpected packet</exception>
        public smb3Packet ExpectPacket(out smb3Endpoint endpoint, TimeSpan timeout)
        {
            TransportEvent transEvent = transport.ExpectTransportEvent(timeout);

            if (transEvent.EventType == EventType.Exception)
            {
                throw new InvalidOperationException("Received un-expected packet, packet type is not correct", (Exception)transEvent.EventObject);
            }

            if (transEvent.EventType != EventType.ReceivedPacket)
            {
                throw new InvalidOperationException("Received un-expected packet, packet type is not correct");
            }

            smb3Event smb3Event = new smb3Event();
            smb3Event.Type = smb3EventType.PacketReceived;

            foreach (smb3Endpoint smb3endpoint in clientEndpoints)
            {
                if (smb3endpoint.TransportType == smb3TransportType.NetBios)
                {
                    if ((object)smb3endpoint.SessionId == transEvent.EndPoint)
                    {
                        endpoint = smb3endpoint;

                        smb3Event.Packet = (smb3Packet)transEvent.EventObject;
                        smb3Event.ConnectionId = smb3endpoint.EndpointId;

                        context.UpdateContext(smb3Event);

                        return (smb3Packet)transEvent.EventObject;
                    }
                }
                else if (smb3endpoint.TransportType == smb3TransportType.Tcp)
                {
                    if ((object)smb3endpoint.RemoteEndpoint == transEvent.EndPoint)
                    {
                        endpoint = smb3endpoint;

                        smb3Event.Packet = (smb3Packet)transEvent.EventObject;
                        smb3Event.ConnectionId = smb3endpoint.EndpointId;

                        context.UpdateContext(smb3Event);

                        return (smb3Packet)transEvent.EventObject;
                    }
                }
            }

            throw new InvalidOperationException("Recieved an un-expected packet, endpoint is not correct");
        }


        #region Sync Message

        /// <summary>
        /// Create smb3ErrorResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="status">The status code for a response</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="errorData">A variable-length data field that contains extended error information</param>
        /// <returns>A smb3ErrorResponsePacket</returns>
        public smb3ErrorResponsePacket CreateErrorResponse(
            smb3Endpoint endpoint,
            uint status,
            ulong messageId,
            byte[] errorData
            )
        {
            smb3ErrorResponsePacket packet = new smb3ErrorResponsePacket();

            SetHeader(packet, status, endpoint, messageId);

            packet.PayLoad.StructureSize = ERROR_Response_packet_StructureSize_Values.V1;
            packet.PayLoad.Reserved = ERROR_Response_packet_Reserved_Values.V1;

            if (errorData == null)
            {
                packet.PayLoad.ByteCount = 0;
                //If the ByteCount field is zero then the server MUST supply an ErrorData 
                //field that is one byte in length
                packet.PayLoad.ErrorData = new byte[1];
            }
            else
            {
                packet.PayLoad.ErrorData = errorData;
                packet.PayLoad.ByteCount = (uint)errorData.Length;
            }

            packet.Header.Flags = packet.Header.Flags & ~Packet_Header_Flags_Values.FLAGS_SIGNED;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3NegotiateResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="dialectRevision">The preferred common SMB 2 Protocol dialect number from the Dialects 
        /// array that is sent in the smb3 NEGOTIATE Request (SECTION 2.2.3) or the smb3 wildcard revision number</param>
        /// <param name="securityPackage">Indicate which security protocol packet will be used</param>
        /// <param name="contextAttribute">The security context used by underlying gss api</param>
        /// <returns>A smb3NegotiateResponsePacket</returns>
        public smb3NegotiateResponsePacket CreateNegotiateResponse(
            smb3Endpoint endpoint,
            DialectRevision_Values dialectRevision,
            SecurityPackage securityPackage,
            ServerContextAttribute contextAttribute
            )
        {
            smb3NegotiateResponsePacket packet = new smb3NegotiateResponsePacket();

            SetHeader(packet, endpoint, 0);

            packet.PayLoad.SecurityMode |= NEGOTIATE_Response_SecurityMode_Values.NEGOTIATE_SIGNING_ENABLED;

            if (context.requireMessageSigning)
            {
                packet.PayLoad.SecurityMode |= NEGOTIATE_Response_SecurityMode_Values.NEGOTIATE_SIGNING_REQUIRED;
            }

            packet.PayLoad.StructureSize = NEGOTIATE_Response_StructureSize_Values.V1;
            packet.PayLoad.DialectRevision = dialectRevision;
            packet.PayLoad.ServerGuid = context.serverGuid;

            if (context.isDfsCapable)
            {
                packet.PayLoad.Capabilities |= NEGOTIATE_Response_Capabilities_Values.GLOBAL_CAP_DFS;
            }

            packet.PayLoad.MaxTransactSize = uint.MaxValue;
            packet.PayLoad.MaxWriteSize = uint.MaxValue;
            packet.PayLoad.MaxReadSize = uint.MaxValue;

            packet.PayLoad.SystemTime = smb3Utility.DateTimeToFileTime(DateTime.Now);
            packet.PayLoad.ServerStartTime = smb3Utility.DateTimeToFileTime(context.serverStartTime);

            SecurityPackageType package = SecurityPackageType.Negotiate;

            if (securityPackage == SecurityPackage.Kerberos)
            {
                package = SecurityPackageType.Kerberos;
            }
            else if (securityPackage == SecurityPackage.Nlmp)
            {
                package = SecurityPackageType.Ntlm;
            }

            AccountCredential credential = new AccountCredential(null, null, null);

            context.connectionList[endpoint.EndpointId].credential = credential;
            context.connectionList[endpoint.EndpointId].packageType = package;
            context.connectionList[endpoint.EndpointId].contextAttribute = (ServerSecurityContextAttribute)contextAttribute;

            if (package == SecurityPackageType.Negotiate)
            {
                context.connectionList[endpoint.EndpointId].gss = new SspiServerSecurityContext(
                    package, 
                    credential,
                    null,
                    context.connectionList[endpoint.EndpointId].contextAttribute, 
                    SecurityTargetDataRepresentation.SecurityNativeDrep);

                //Generate the first token
                context.connectionList[endpoint.EndpointId].gss.Accept(null);

                packet.PayLoad.Buffer = context.connectionList[endpoint.EndpointId].gss.Token;
                packet.PayLoad.SecurityBufferOffset = smb3Consts.SecurityBufferOffsetInNegotiateResponse;
            }
            else
            {
                packet.PayLoad.Buffer = new byte[0];
            }
            packet.PayLoad.SecurityBufferLength = (ushort)packet.PayLoad.Buffer.Length;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3SessionSetupResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="sessionId">Uniquely identifies the established session for the command</param>
        /// <param name="sessionFlags">A flags field that indicates additional information about the session</param>
        /// <returns>A smb3SessionSetupResponsePacket</returns>
        public smb3SessionSetupResponsePacket CreateSessionSetupResponse(
            smb3Endpoint endpoint,
            ulong messageId,
            ulong sessionId,
            SessionFlags_Values sessionFlags
            )
        {
            //This is for re-authenticate. the state is used to indicate user that
            //the authenticate process in not complete.
            if (context.globalSessionTable.ContainsKey(sessionId))
            {
                context.globalSessionTable[sessionId].state = SessionState.InProgress;
            }

            smb3SessionSetupResponsePacket packet = new smb3SessionSetupResponsePacket();

            SetHeader(packet, 0, endpoint, messageId);

            packet.Header.SessionId = sessionId;
            packet.PayLoad.StructureSize = SESSION_SETUP_Response_StructureSize_Values.V1;
            packet.PayLoad.SessionFlags = sessionFlags;

            smb3SessionSetupRequestPacket requestPacket = context.FindRequestPacket(endpoint.EndpointId, messageId)
                as smb3SessionSetupRequestPacket;

            if (context.connectionList[endpoint.EndpointId].gss == null)
            {
                context.connectionList[endpoint.EndpointId].gss = new SspiServerSecurityContext(
                    context.connectionList[endpoint.EndpointId].packageType,
                    context.connectionList[endpoint.EndpointId].credential,
                    null,
                    context.connectionList[endpoint.EndpointId].contextAttribute,
                    SecurityTargetDataRepresentation.SecurityNativeDrep);

                context.connectionList[endpoint.EndpointId].gss.Accept(requestPacket.PayLoad.Buffer);
            }
            else
            {
                context.connectionList[endpoint.EndpointId].gss.Accept(requestPacket.PayLoad.Buffer);
            }

            if (context.connectionList[endpoint.EndpointId].gss.NeedContinueProcessing)
            {
                packet.Header.Status = (uint)smb3Status.STATUS_MORE_PROCESSING_REQUIRED;
            }

            packet.PayLoad.Buffer = context.connectionList[endpoint.EndpointId].gss.Token;

            packet.PayLoad.SecurityBufferOffset = smb3Consts.SecurityBufferOffsetInSessionSetup;
            packet.PayLoad.SecurityBufferLength = (ushort)packet.PayLoad.Buffer.Length;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3LogOffResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <returns>A smb3LogOffResponsePacket</returns>
        public smb3LogOffResponsePacket CreateLogoffResponse(
            smb3Endpoint endpoint,
            ulong messageId
            )
        {
            smb3LogOffResponsePacket packet = new smb3LogOffResponsePacket();

            SetHeader(packet, endpoint, messageId);

            packet.PayLoad.Reserved = LOGOFF_Response_Reserved_Values.V1;
            packet.PayLoad.StructureSize = LOGOFF_Response_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3TreeConnectResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="treeId">Uniquely identifies the tree connect for the command</param>
        /// <param name="shareType">The type of share being accessed. This field MUST contain one of the following values</param>
        /// <param name="shareFlags">This field contains properties for this share</param>
        /// <param name="capabilities">Indicates various capabilities for this share</param>
        /// <param name="maximalAccess">Contains the maximal access for the user that
        /// establishes the tree connect on the share based on the share's permissions</param>
        /// <returns>A smb3TreeConnectResponsePacket</returns>
        public smb3TreeConnectResponsePacket CreateTreeConnectResponse(
            smb3Endpoint endpoint,
            ulong messageId,
            uint treeId,
            ShareType_Values shareType,
            ShareFlags_Values shareFlags,
            Capabilities_Values capabilities,
            _ACCESS_MASK maximalAccess
            )
        {
            smb3TreeConnectResponsePacket packet = new smb3TreeConnectResponsePacket();

            SetHeader(packet, endpoint, messageId);

            packet.Header.TreeId = treeId;
            packet.PayLoad.Capabilities = capabilities;
            packet.PayLoad.MaximalAccess = maximalAccess;
            packet.PayLoad.Reserved = Reserved_Values.V1;
            packet.PayLoad.ShareFlags = shareFlags;
            packet.PayLoad.ShareType = shareType;
            packet.PayLoad.StructureSize = StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3TreeDisconnectResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <returns>A smb3TreeDisconnectResponsePacket</returns>
        public smb3TreeDisconnectResponsePacket CreateTreeDisconnectResponse(
            smb3Endpoint endpoint,
            ulong messageId
            )
        {
            smb3TreeDisconnectResponsePacket packet = new smb3TreeDisconnectResponsePacket();

            SetHeader(packet, endpoint, messageId);

            packet.PayLoad.Reserved = TREE_DISCONNECT_Response_Reserved_Values.V1;
            packet.PayLoad.StructureSize = TREE_DISCONNECT_Response_StructureSize_Values.CorrectValue;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3CreateResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="oplockLevel">The oplock level that is granted to the client for this open</param>
        /// <param name="createAction">The action taken in establishing the open</param>
        /// <param name="creationTime">The time when the file was created</param>
        /// <param name="lastAccessTime">The time the file was last accessed</param>
        /// <param name="lastWriteTime">The time when data was last written to the file</param>
        /// <param name="changeTime">The time when the file was last modified</param>
        /// <param name="allocationSize">The size, in bytes, of the data that is allocated to the file</param>
        /// <param name="endofFile">The size, in bytes, of the file</param>
        /// <param name="fileAttributes">The attributes of the file</param>
        /// <param name="fileId">An SMB2_FILEID, as specified in section 2.2.14.1</param>
        /// <param name="createContexts">Create context</param>
        /// <returns>A smb3CreateResponsePacket</returns>
        public smb3CreateResponsePacket CreateCreateResponse(
            smb3Endpoint endpoint,
            ulong messageId,
            OplockLevel_Values oplockLevel,
            CreateAction_Values createAction,
            _FILETIME creationTime,
            _FILETIME lastAccessTime,
            _FILETIME lastWriteTime,
            _FILETIME changeTime,
            ulong allocationSize,
            ulong endofFile,
            File_Attributes fileAttributes,
            FILEID fileId,
            params CREATE_CONTEXT_Values[] createContexts
            )
        {
            smb3CreateResponsePacket packet = new smb3CreateResponsePacket();

            SetHeader(packet, endpoint, messageId);

            packet.PayLoad.StructureSize = CREATE_Response_StructureSize_Values.V1;
            packet.PayLoad.OplockLevel = oplockLevel;
            packet.PayLoad.Reserved = 0;
            packet.PayLoad.CreateAction = createAction;
            packet.PayLoad.CreationTime = creationTime;
            packet.PayLoad.LastAccessTime = lastAccessTime;
            packet.PayLoad.LastWriteTime = lastWriteTime;
            packet.PayLoad.ChangeTime = changeTime;
            packet.PayLoad.AllocationSize = allocationSize;
            packet.PayLoad.EndofFile = endofFile;
            packet.PayLoad.FileAttributes = fileAttributes;
            packet.PayLoad.Reserved2 = 0;
            packet.PayLoad.FileId = fileId;

            if (createContexts == null)
            {
                packet.PayLoad.CreateContextsOffset = 0;
                packet.PayLoad.CreateContextsLength = 0;
                packet.PayLoad.Buffer = new byte[0];
            }
            else
            {
                packet.PayLoad.CreateContextsOffset = smb3Consts.CreateContextOffsetInCreateResponse;

                using (MemoryStream ms = new MemoryStream())
                {
                    for (int i = 0; i < createContexts.Length; i++)
                    {
                        byte[] createContext = smb3Utility.ToBytes(createContexts[i]);

                        if (i != (createContexts.Length - 1))
                        {
                            int alignedLen = smb3Utility.AlignBy8Bytes(createContext.Length);

                            byte[] nextValue = BitConverter.GetBytes(alignedLen);
                            Array.Copy(nextValue, createContext, nextValue.Length);

                            ms.Write(createContext, 0, createContext.Length);

                            //write the padding 0;
                            for (int j = 0; j < (alignedLen - createContext.Length); j++)
                            {
                                ms.WriteByte(0);
                            }
                        }
                        else
                        {
                            ms.Write(createContext, 0, createContext.Length);
                        }
                    }

                    packet.PayLoad.Buffer = ms.ToArray();
                    packet.PayLoad.CreateContextsLength = (uint)packet.PayLoad.Buffer.Length;
                }
            }

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3CloseResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="flags">A Flags field that indicates how the operation MUST be processed</param>
        /// <param name="creationTime">The time when the file was created</param>
        /// <param name="lastAccessTime">The time when the file was last accessed</param>
        /// <param name="lastWriteTime">The time when data was last written to the file</param>
        /// <param name="changeTime">The time when the file was last modified</param>
        /// <param name="allocationSize">The size, in bytes, of the data that is allocated to the file</param>
        /// <param name="endofFile">The size, in bytes, of the file</param>
        /// <param name="fileAttributes">The attributes of the file</param>
        /// <returns>A smb3CloseResponsePacket</returns>
        public smb3CloseResponsePacket CreateCloseResponse(
            smb3Endpoint endpoint,
            ulong messageId,
            CLOSE_Response_Flags_Values flags,
            _FILETIME creationTime,
            _FILETIME lastAccessTime,
            _FILETIME lastWriteTime,
            _FILETIME changeTime,
            ulong allocationSize,
            ulong endofFile,
            File_Attributes fileAttributes
            )
        {
            smb3CloseResponsePacket packet = new smb3CloseResponsePacket();

            SetHeader(packet, endpoint, messageId);

            packet.PayLoad.AllocationSize = allocationSize;
            packet.PayLoad.ChangeTime = changeTime;
            packet.PayLoad.CreationTime = creationTime;
            packet.PayLoad.EndofFile = endofFile;
            packet.PayLoad.FileAttributes = fileAttributes;

            if (fileAttributes == File_Attributes.NONE)
            {
                packet.PayLoad.Flags = CLOSE_Response_Flags_Values.NONE;
            }
            else
            {
                packet.PayLoad.Flags = CLOSE_Response_Flags_Values.V1;
            }

            packet.PayLoad.LastAccessTime = lastAccessTime;
            packet.PayLoad.LastWriteTime = lastWriteTime;
            packet.PayLoad.Reserved = CLOSE_Response_Reserved_Values.V1;
            packet.PayLoad.StructureSize = CLOSE_Response_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3FlushResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <returns>A smb3FlushResponsePacket</returns>
        public smb3FlushResponsePacket CreateFlushResponse(
            smb3Endpoint endpoint,
            ulong messageId
            )
        {
            smb3FlushResponsePacket packet = new smb3FlushResponsePacket();

            SetHeader(packet, endpoint, messageId);
            packet.PayLoad.Reserved = FLUSH_Response_Reserved_Values.V1;
            packet.PayLoad.StructureSize = FLUSH_Response_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3ReadResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="status">The status code for a response</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely across 
        /// all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="buffer">A variable-length buffer that contains the data read for the response</param>
        /// <returns>A smb3ReadResponsePacket</returns>
        public smb3ReadResponsePacket CreateReadResponse(
            smb3Endpoint endpoint,
            uint status,
            ulong messageId,
            byte[] buffer
            )
        {
            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentException("buffer should at least contains 1 byte.", "buffer");
            }

            smb3ReadResponsePacket packet = new smb3ReadResponsePacket();
            packet.Header.Status = status;

            SetHeader(packet, endpoint, messageId);

            packet.PayLoad.StructureSize = READ_Response_StructureSize_Values.V1;
            packet.PayLoad.Reserved = READ_Response_Reserved_Values.V1;
            packet.PayLoad.Reserved2 = READ_Response_Reserved2_Values.V1;
            packet.PayLoad.DataRemaining = DataRemaining_Values.V1;
            packet.PayLoad.DataOffset = smb3Consts.DataOffsetInReadResponse;
            packet.PayLoad.DataLength = (uint)buffer.Length;
            packet.PayLoad.Buffer = buffer;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3WriteResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely across 
        /// all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="count">The number of bytes written</param>
        /// <returns>A smb3WriteResponsePacket</returns>
        public smb3WriteResponsePacket CreateWriteResponse(
            smb3Endpoint endpoint,
            ulong messageId,
            uint count
            )
        {
            smb3WriteResponsePacket packet = new smb3WriteResponsePacket();

            SetHeader(packet, endpoint, messageId);

            packet.PayLoad.Count = count;
            packet.PayLoad.Remaining = Remaining_Values.V1;
            packet.PayLoad.Reserved = WRITE_Response_Reserved_Values.V1;
            packet.PayLoad.StructureSize = WRITE_Response_StructureSize_Values.V1;
            packet.PayLoad.WriteChannelInfoLength = WRITE_Response_WriteChannelInfoLength_Values.V1;
            packet.PayLoad.WriteChannelInfoOffset = WRITE_Response_WriteChannelInfoOffset_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3OpLockBreakNotificationPacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="oplockLevel">The server MUST set this to the maximum value of the OplockLevel 
        /// that the server will accept for an acknowledgment from the client</param>
        /// <param name="fileId">An SMB2_FILEID, as specified in section 2.2.14.1</param>
        /// <returns>A smb3OpLockBreakNotificationPacket</returns>
        public smb3OpLockBreakNotificationPacket CreateOpLockBreakNotificationResponse(
            smb3Endpoint endpoint,
            OPLOCK_BREAK_Notification_Packet_OplockLevel_Values oplockLevel,
            FILEID fileId
            )
        {
            smb3OpLockBreakNotificationPacket packet = new smb3OpLockBreakNotificationPacket();

            packet.Header.Flags = Packet_Header_Flags_Values.FLAGS_SERVER_TO_REDIR;
            packet.Header.Command = smb3Command.OPLOCK_BREAK;
            packet.Header.MessageId = ulong.MaxValue;
            packet.Header.ProtocolId = smb3Consts.smb3ProtocolId;
            packet.Header.Signature = new byte[smb3Consts.SignatureSize];
            packet.Header.StructureSize = Packet_Header_StructureSize_Values.V1;

            packet.Endpoint = endpoint;
            packet.PayLoad.FileId = fileId;
            packet.PayLoad.OplockLevel = oplockLevel;
            packet.PayLoad.Reserved = OPLOCK_BREAK_Notification_Packet_Reserved_Values.V1;
            packet.PayLoad.Reserved2 = OPLOCK_BREAK_Notification_Packet_Reserved2_Values.V1;
            packet.PayLoad.StructureSize = OPLOCK_BREAK_Notification_Packet_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3LeaseBreakNotificationPacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="flags">The field MUST be constructed using the following values</param>
        /// <param name="leaseKey">A unique key which identifies the owner of the lease</param>
        /// <param name="currentLeaseState">The current lease state of the open</param>
        /// <param name="newLeaseState">The new lease state for the open</param>
        /// <returns>A smb3LeaseBreakNotificationPacket</returns>
        public smb3LeaseBreakNotificationPacket CreateLeaseBreakNotificationResponse(
            smb3Endpoint endpoint,
            LEASE_BREAK_Notification_Packet_Flags_Values flags,
            byte[] leaseKey,
            LeaseStateValues currentLeaseState,
            LeaseStateValues newLeaseState
            )
        {
            smb3LeaseBreakNotificationPacket packet = new smb3LeaseBreakNotificationPacket();

            packet.Header.Flags = Packet_Header_Flags_Values.FLAGS_SERVER_TO_REDIR;
            packet.Header.Command = smb3Command.OPLOCK_BREAK;
            packet.Header.MessageId = ulong.MaxValue;
            packet.Header.ProtocolId = smb3Consts.smb3ProtocolId;
            packet.Header.Signature = new byte[smb3Consts.SignatureSize];
            packet.Header.StructureSize = Packet_Header_StructureSize_Values.V1;

            packet.Endpoint = endpoint;

            packet.PayLoad.AccessMaskHint = LEASE_BREAK_Notification_Packet_AccessMaskHint_Values.V1;
            packet.PayLoad.BreakReason = LEASE_BREAK_Notification_Packet_BreakReason_Values.V1;
            packet.PayLoad.CurrentLeaseState = currentLeaseState;
            packet.PayLoad.Flags = LEASE_BREAK_Notification_Packet_Flags_Values.SMB2_NOTIFY_BREAK_LEASE_FLAG_ACK_REQUIRED;
            packet.PayLoad.LeaseKey = leaseKey;
            packet.PayLoad.NewLeaseState = newLeaseState;
            packet.PayLoad.Reserved = LEASE_BREAK_Notification_Packet_Reserved_Values.V1;
            packet.PayLoad.ShareMaskHint = LEASE_BREAK_Notification_Packet_ShareMaskHint_Values.V1;
            packet.PayLoad.StructureSize = LEASE_BREAK_Notification_Packet_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3OpLockBreakResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="oplockLevel">The resulting oplock level</param>
        /// <returns>A smb3OpLockBreakResponsePacket</returns>
        public smb3OpLockBreakResponsePacket CreateOpLockBreakResponse(
            smb3Endpoint endpoint,
            ulong messageId,
            OPLOCK_BREAK_Response_OplockLevel_Values oplockLevel
            )
        {
            smb3OpLockBreakResponsePacket packet = new smb3OpLockBreakResponsePacket();

            SetHeader(packet, endpoint, messageId);

            smb3OpLockBreakAckPacket oplockAck = context.FindRequestPacket(endpoint.EndpointId, messageId)
                as smb3OpLockBreakAckPacket;
            packet.PayLoad.FileId = oplockAck.GetFileId();
            packet.PayLoad.OplockLevel = oplockLevel;
            packet.PayLoad.Reserved = OPLOCK_BREAK_Response_Reserved_Values.V1;
            packet.PayLoad.Reserved2 = OPLOCK_BREAK_Response_Reserved2_Values.V1;
            packet.PayLoad.StructureSize = OPLOCK_BREAK_Response_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3LeaseBreakResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="leaseState">The requested lease state</param>
        /// <returns>A smb3LeaseBreakResponsePacket</returns>
        public smb3LeaseBreakResponsePacket CreateLeaseBreakResponse(
            smb3Endpoint endpoint,
            ulong messageId,
            LeaseStateValues leaseState
            )
        {
            smb3LeaseBreakResponsePacket packet = new smb3LeaseBreakResponsePacket();

            SetHeader(packet, endpoint, messageId);

            smb3LeaseBreakAckPacket leaseBreakAck = context.FindRequestPacket(endpoint.EndpointId, messageId)
                as smb3LeaseBreakAckPacket;

            packet.PayLoad.Flags = LEASE_BREAK_Response_Packet_Flags_Values.V1;
            packet.PayLoad.LeaseDuration = LEASE_BREAK_Response_Packet_LeaseDuration_Values.V1;
            packet.PayLoad.LeaseKey = leaseBreakAck.PayLoad.LeaseKey;
            packet.PayLoad.LeaseState = leaseState;
            packet.PayLoad.Reserved = LEASE_BREAK_Response_Reserved_Values.V1;
            packet.PayLoad.StructureSize = LEASE_BREAK_Response_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3LockResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <returns>A smb3LockResponsePacket</returns>
        public smb3LockResponsePacket CreateLockResponse(
            smb3Endpoint endpoint,
            ulong messageId
            )
        {
            smb3LockResponsePacket packet = new smb3LockResponsePacket();

            SetHeader(packet, endpoint, messageId);

            packet.PayLoad.Reserved = LOCK_Response_Reserved_Values.V1;
            packet.PayLoad.StructureSize = LOCK_Response_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3EchoResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <returns>A smb3EchoResponsePacket</returns>
        public smb3EchoResponsePacket CreateEchoResponse(
            smb3Endpoint endpoint,
            ulong messageId
            )
        {
            smb3EchoResponsePacket packet = new smb3EchoResponsePacket();

            SetHeader(packet, endpoint, messageId);

            packet.PayLoad.Reserved = ECHO_Response_Reserved_Values.V1;
            packet.PayLoad.StructureSize = ECHO_Response_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3IOCtlResponsePacket, This is for pass-through IOCTL which need input information
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="input">The input information about this IO control</param>
        /// <param name="output">The output information about this IO control</param>
        /// <returns>A smb3IOCtlResponsePacket</returns>
        public smb3IOCtlResponsePacket CreateIOCtlResponse(
            smb3Endpoint endpoint,
            ulong messageId,
            byte[] input,
            byte[] output
            )
        {
            smb3IOCtlResponsePacket packet = new smb3IOCtlResponsePacket();

            smb3IOCtlRequestPacket requestPacket = context.FindRequestPacket(endpoint.EndpointId, messageId)
                as smb3IOCtlRequestPacket;

            SetHeader(packet, endpoint, messageId);

            packet.PayLoad.CtlCode = (uint)requestPacket.PayLoad.CtlCode;
            packet.PayLoad.FileId = requestPacket.PayLoad.FileId;
            packet.PayLoad.Flags = IOCTL_Response_Flags_Values.V1;

            int bufferLen = 0;

            if (input != null)
            {
                packet.PayLoad.InputCount = (uint)input.Length;
                packet.PayLoad.InputOffset = smb3Consts.InputOffsetInIOCtlResponse;
                bufferLen += smb3Utility.AlignBy8Bytes(input.Length);
            }

            if (output != null)
            {
                packet.PayLoad.OutputCount = (uint)output.Length;
                packet.PayLoad.OutputOffset = (uint)(smb3Consts.InputOffsetInIOCtlResponse + bufferLen);
                bufferLen += output.Length;
            }

            byte[] buffer = new byte[bufferLen];

            if (input != null)
            {
                Array.Copy(input, buffer, input.Length);
            }

            if (output != null)
            {
                Array.Copy(output, 0, buffer, packet.PayLoad.OutputOffset - smb3Consts.InputOffsetInIOCtlResponse,
                    output.Length);
            }

            packet.PayLoad.Reserved = IOCTL_Response_Reserved_Values.V1;
            packet.PayLoad.Reserved2 = IOCTL_Response_Reserved2_Values.V1;
            packet.PayLoad.StructureSize = IOCTL_Response_StructureSize_Values.V1;
            packet.PayLoad.Buffer = buffer;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3IOCtlResponsePacket for SRV_COPYCHUNK_RESPONSE
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="copyChuck">The copyChuck information</param>
        /// <returns>A smb3IOCtlResponsePacket</returns>
        public smb3IOCtlResponsePacket CreateCopyChunkIOCtlResponse(
            smb3Endpoint endpoint,
            ulong messageId,
            SRV_COPYCHUNK_RESPONSE copyChuck
            )
        {
            byte[] output = smb3Utility.ToBytes(copyChuck);

            return CreateIOCtlResponse(endpoint, messageId, null, output);
        }


        /// <summary>
        /// Create smb3IOCtlResponsePacket for SRV_SNAPSHOT_ARRAY
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="snapshots">The snapshot information</param>
        /// <returns>A smb3IOCtlResponsePacket</returns>
        public smb3IOCtlResponsePacket CreateSnapshotIOCtlResponse(
            smb3Endpoint endpoint,
            ulong messageId,
            SRV_SNAPSHOT_ARRAY snapshots
            )
        {
            byte[] output = smb3Utility.ToBytes(snapshots);

            return CreateIOCtlResponse(endpoint, messageId, null, output);
        }


        /// <summary>
        /// Create smb3IOCtlResponsePacket for SRV_REQUEST_RESUME_KEY
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="resumeKey">The resumeKey information</param>
        /// <returns>A smb3IOCtlResponsePacket</returns>
        public smb3IOCtlResponsePacket CreateResumeKeyIOCtlResponse(
            smb3Endpoint endpoint,
            ulong messageId,
            SRV_REQUEST_RESUME_KEY_Response resumeKey
            )
        {
            byte[] output = smb3Utility.ToBytes(resumeKey);

            return CreateIOCtlResponse(endpoint, messageId, null, output);
        }


        /// <summary>
        /// Create smb3IOCtlResponsePacket for SRV_READ_HASH
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="readHash">The readHash information</param>
        /// <returns>A smb3IOCtlResponsePacket</returns>
        public smb3IOCtlResponsePacket CreateReadHashIOCtlResponse(
            smb3Endpoint endpoint,
            ulong messageId,
            SRV_READ_HASH_Response readHash
            )
        {
            byte[] output = smb3Utility.ToBytes(readHash);

            return CreateIOCtlResponse(endpoint, messageId, null, output);
        }


        /// <summary>
        /// Create smb3QueryDirectoryResponePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="buffer">A variable-length buffer containing the directory enumeration being returned in the response</param>
        /// <returns>A smb3QueryDirectoryResponePacket</returns>
        public smb3QueryDirectoryResponePacket CreateQueryDirectoryResponse(
            smb3Endpoint endpoint,
            ulong messageId,
            byte[] buffer
            )
        {
            smb3QueryDirectoryResponePacket packet = new smb3QueryDirectoryResponePacket();

            SetHeader(packet, endpoint, messageId);

            packet.PayLoad.StructureSize = QUERY_DIRECTORY_Response_StructureSize_Values.V1;

            if (buffer == null)
            {
                packet.PayLoad.Buffer = new byte[0];
            }
            else
            {
                packet.PayLoad.Buffer = buffer;
                packet.PayLoad.OutputBufferOffset = smb3Consts.OutputBufferOffsetInQueryInfoResponse;
                packet.PayLoad.OutputBufferLength = (uint)buffer.Length;
            }

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3ChangeNotifyResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="status">The status code for a response</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="notifyInfo">contains the change information being returned in the response</param>
        /// <returns>A smb3ChangeNotifyResponsePacket</returns>
        public smb3ChangeNotifyResponsePacket CreateChangeNotifyResponse(
            smb3Endpoint endpoint,
            uint status,
            ulong messageId,
            params FILE_NOTIFY_INFORMATION[] notifyInfo
            )
        {
            smb3ChangeNotifyResponsePacket packet = new smb3ChangeNotifyResponsePacket();

            SetHeader(packet, status, endpoint, messageId);

            if (notifyInfo == null)
            {
                packet.PayLoad.Buffer = new byte[0];
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    foreach (FILE_NOTIFY_INFORMATION oneNotifyInfo in notifyInfo)
                    {
                        byte[] oneNotifyInfoArray = smb3Utility.ToBytes(oneNotifyInfo);

                        ms.Write(oneNotifyInfoArray, 0, oneNotifyInfoArray.Length);
                    }

                    packet.PayLoad.Buffer = ms.ToArray();
                    packet.PayLoad.OutputBufferLength = (uint)packet.PayLoad.Buffer.Length;
                    packet.PayLoad.OutputBufferOffset = smb3Consts.OutputBufferOffsetInChangeNotifyResponse;
                }
            }

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3QueryInfoResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="status">The status code for a response</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="buffer">A variable-length buffer that contains the information that is returned in the response</param>
        /// <returns>A smb3QueryInfoResponsePacket</returns>
        public smb3QueryInfoResponsePacket CreateQueryInfoResponse(
            smb3Endpoint endpoint,
            uint status,
            ulong messageId,
            byte[] buffer
            )
        {
            smb3QueryInfoResponsePacket packet = new smb3QueryInfoResponsePacket();

            SetHeader(packet, endpoint, messageId);

            packet.Header.Status = status;
            packet.PayLoad.StructureSize = QUERY_INFO_Response_StructureSize_Values.V1;

            if (buffer == null)
            {
                packet.PayLoad.OutputBufferOffset = 0;
                packet.PayLoad.Buffer = new byte[0];
            }
            else
            {
                packet.PayLoad.OutputBufferOffset = smb3Consts.OutputBufferOffsetInQueryInfoResponse;
                packet.PayLoad.Buffer = buffer;
                packet.PayLoad.OutputBufferLength = (uint)buffer.Length;
            }

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3SetInfoResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <returns>A smb3SetInfoResponsePacket</returns>
        public smb3SetInfoResponsePacket CreateSetInfoResponse(
            smb3Endpoint endpoint,
            ulong messageId
            )
        {
            smb3SetInfoResponsePacket packet = new smb3SetInfoResponsePacket();

            SetHeader(packet, endpoint, messageId);

            packet.PayLoad.StructureSize = SET_INFO_Response_StructureSize_Values.V1;

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3CompoundPacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="packets">Variable number of Single packets</param>
        /// <returns>A smb3CompoundPacket</returns>
        public smb3CompoundPacket CreateCompoundResponse(
            smb3Endpoint endpoint,
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

            smb3CompoundPacket packet = new smb3CompoundPacket();
            packet.Packets = new List<smb3SinglePacket>();

            //The endpoint of the compoundpacket comes from innerPacket.
            packet.Endpoint = packets[0].Endpoint;

            for (int i = 0; i < packets.Length; i++)
            {
                if (((packets[0].Header.Flags & Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS)
                    == Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS))
                {
                    packets[i].OuterCompoundPacket = packet;
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

                packet.Packets.Add(packets[i]);
            }

            packet.Sign();

            return packet;
        }

        #endregion

        #region Async Message


        /// <summary>
        /// Create smb3ErrorResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="asyncId">A unique identification number that is created by the server
        /// to handle operations asynchronously</param>
        /// <param name="status">The status code for a response</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="errorData">A variable-length data field that contains extended error information</param>
        /// <returns>A smb3ErrorResponsePacket</returns>
        public smb3ErrorResponsePacket CreateErrorResponseAsync(
            smb3Endpoint endpoint,
            ulong asyncId,
            uint status,
            ulong messageId,
            byte[] errorData
            )
        {
            smb3ErrorResponsePacket packet = CreateErrorResponse(endpoint, status, messageId, errorData);

            packet.Header.Flags |= Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND;
            packet.Header.ProcessId = (uint)asyncId;
            packet.Header.TreeId = (uint)(asyncId >> 32);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3CreateResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="asyncId">A unique identification number that is created by the server
        /// to handle operations asynchronously</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="oplockLevel">The oplock level that is granted to the client for this open</param>
        /// <param name="createAction">The action taken in establishing the open</param>
        /// <param name="creationTime">The time when the file was created</param>
        /// <param name="lastAccessTime">The time the file was last accessed</param>
        /// <param name="lastWriteTime">The time when data was last written to the file</param>
        /// <param name="changeTime">The time when the file was last modified</param>
        /// <param name="allocationSize">The size, in bytes, of the data that is allocated to the file</param>
        /// <param name="endofFile">The size, in bytes, of the file</param>
        /// <param name="fileAttributes">The attributes of the file</param>
        /// <param name="fileId">An SMB2_FILEID, as specified in section 2.2.14.1</param>
        /// <param name="contexts">Variable number of context</param>
        /// <returns>A smb3CreateResponsePacket</returns>
        public smb3CreateResponsePacket CreateCreateResponseAsync(
            smb3Endpoint endpoint,
            ulong asyncId,
            ulong messageId,
            OplockLevel_Values oplockLevel,
            CreateAction_Values createAction,
            _FILETIME creationTime,
            _FILETIME lastAccessTime,
            _FILETIME lastWriteTime,
            _FILETIME changeTime,
            ulong allocationSize,
            ulong endofFile,
            File_Attributes fileAttributes,
            FILEID fileId,
            params CREATE_CONTEXT_Values[] contexts
            )
        {
            smb3CreateResponsePacket packet = CreateCreateResponse(endpoint, messageId, oplockLevel,
                createAction, creationTime, lastAccessTime, lastWriteTime, changeTime, allocationSize,
                endofFile, fileAttributes, fileId, contexts);

            ModifyAsyncHeader(packet, endpoint, asyncId);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3FlushResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="asyncId">A unique identification number that is created by the server
        /// to handle operations asynchronously</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <returns>A smb3FlushResponsePacket</returns>
        public smb3FlushResponsePacket CreateFlushResponseAsync(
            smb3Endpoint endpoint,
            ulong asyncId,
            ulong messageId
            )
        {
            smb3FlushResponsePacket packet = CreateFlushResponse(endpoint, messageId);

            ModifyAsyncHeader(packet, endpoint, asyncId);

            packet.Sign();

            return packet;
        }



        /// <summary>
        /// Create smb3ReadResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="asyncId">A unique identification number that is created by the server
        /// to handle operations asynchronously</param>
        /// <param name="status">The status code for a response</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely across 
        /// all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="buffer">A variable-length buffer that contains the data read for the response</param>
        /// <returns>A smb3ReadResponsePacket</returns>
        public smb3ReadResponsePacket CreateReadResponseAsync(
            smb3Endpoint endpoint,
            ulong asyncId,
            uint status,
            ulong messageId,
            byte[] buffer
            )
        {
            smb3ReadResponsePacket packet = CreateReadResponse(endpoint, status, messageId, buffer);

            ModifyAsyncHeader(packet, endpoint, asyncId);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3WriteResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="asyncId">A unique identification number that is created by the server
        /// to handle operations asynchronously</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely across 
        /// all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="count">The number of bytes written</param>
        /// <returns>A smb3WriteResponsePacket</returns>
        public smb3WriteResponsePacket CreateWriteResponseAsync(
            smb3Endpoint endpoint,
            ulong asyncId,
            ulong messageId,
            uint count
            )
        {
            smb3WriteResponsePacket packet = CreateWriteResponse(endpoint, messageId, count);

            ModifyAsyncHeader(packet, endpoint, asyncId);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3LockResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="asyncId">A unique identification number that is created by the server
        /// to handle operations asynchronously</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <returns>A smb3LockResponsePacket</returns>
        public smb3LockResponsePacket CreateLockResponseAsync(
            smb3Endpoint endpoint,
            ulong asyncId,
            ulong messageId
            )
        {
            smb3LockResponsePacket packet = CreateLockResponse(endpoint, messageId);

            ModifyAsyncHeader(packet, endpoint, asyncId);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3EchoResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="asyncId">A unique identification number that is created by the server
        /// to handle operations asynchronously</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <returns>A smb3EchoResponsePacket</returns>
        public smb3EchoResponsePacket CreateEchoResponseAsync(
            smb3Endpoint endpoint,
            ulong asyncId,
            ulong messageId
            )
        {
            smb3EchoResponsePacket packet = CreateEchoResponse(endpoint, messageId);

            ModifyAsyncHeader(packet, endpoint, asyncId);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3IOCtlResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="asyncId">A unique identification number that is created by the server
        /// to handle operations asynchronously</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="input">The input data</param>
        /// <param name="output">The output information about this IO control</param>
        /// <returns>A smb3IOCtlResponsePacket</returns>
        public smb3IOCtlResponsePacket CreateIOCtlResponseAsync(
            smb3Endpoint endpoint,
            ulong asyncId,
            ulong messageId,
            byte[] input,
            byte[] output
            )
        {
            smb3IOCtlResponsePacket packet = CreateIOCtlResponse(endpoint, messageId, input, output);

            ModifyAsyncHeader(packet, endpoint, asyncId);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3ChangeNotifyResponsePacket
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="asyncId">A unique identification number that is created by the server
        /// to handle operations asynchronously</param>
        /// <param name="status">The status code for a response</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="notifyInfo">contains the change information being returned in the response</param>
        /// <returns>A smb3ChangeNotifyResponsePacket</returns>
        public smb3ChangeNotifyResponsePacket CreateChangeNotifyResponseAsync(
            smb3Endpoint endpoint,
            ulong asyncId,
            uint status,
            ulong messageId,
            params FILE_NOTIFY_INFORMATION[] notifyInfo
            )
        {
            smb3ChangeNotifyResponsePacket packet = CreateChangeNotifyResponse(endpoint, status, messageId, notifyInfo);

            ModifyAsyncHeader(packet, endpoint, asyncId);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3IOCtlResponsePacket for SRV_COPYCHUNK_RESPONSE
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="asyncId">A unique identification number that is created by the server
        /// to handle operations asynchronously</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="copyChuck">The copyChuck information</param>
        /// <returns>A smb3IOCtlResponsePacket</returns>
        public smb3IOCtlResponsePacket CreateCopyChunkIOCtlResponseAsync(
            smb3Endpoint endpoint,
            ulong asyncId,
            ulong messageId,
            SRV_COPYCHUNK_RESPONSE copyChuck
            )
        {
            smb3IOCtlResponsePacket packet = CreateCopyChunkIOCtlResponse(endpoint, messageId, copyChuck);

            ModifyAsyncHeader(packet, endpoint, asyncId);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3IOCtlResponsePacket for SRV_SNAPSHOT_ARRAY
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="asyncId">A unique identification number that is created by the server
        /// to handle operations asynchronously</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="snapshots">The snapshot information</param>
        /// <returns>A smb3IOCtlResponsePacket</returns>
        public smb3IOCtlResponsePacket CreateSnapshotIOCtlResponseAsync(
            smb3Endpoint endpoint,
            ulong asyncId,
            ulong messageId,
            SRV_SNAPSHOT_ARRAY snapshots
            )
        {
            smb3IOCtlResponsePacket packet = CreateSnapshotIOCtlResponse(endpoint, messageId, snapshots);

            ModifyAsyncHeader(packet, endpoint, asyncId);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3IOCtlResponsePacket for SRV_REQUEST_RESUME_KEY
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="asyncId">A unique identification number that is created by the server
        /// to handle operations asynchronously</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="resumeKey">The resumeKey information</param>
        /// <returns>A smb3IOCtlResponsePacket</returns>
        public smb3IOCtlResponsePacket CreateResumeKeyIOCtlResponseAsync(
            smb3Endpoint endpoint,
            ulong asyncId,
            ulong messageId,
            SRV_REQUEST_RESUME_KEY_Response resumeKey
            )
        {
            smb3IOCtlResponsePacket packet = CreateResumeKeyIOCtlResponse(endpoint, messageId, resumeKey);

            ModifyAsyncHeader(packet, endpoint, asyncId);

            packet.Sign();

            return packet;
        }


        /// <summary>
        /// Create smb3IOCtlResponsePacket for SRV_READ_HASH
        /// </summary>
        /// <param name="endpoint">represents where this packet will be sent</param>
        /// <param name="asyncId">A unique identification number that is created by the server
        /// to handle operations asynchronously</param>
        /// <param name="messageId">A value that identifies a message request and response uniquely 
        /// across all messages that are sent on the same SMB 2 Protocol transport connection</param>
        /// <param name="readHash">The readHash information</param>
        /// <returns>A smb3IOCtlResponsePacket</returns>
        public smb3IOCtlResponsePacket CreateReadHashIOCtlResponseAsync(
            smb3Endpoint endpoint,
            ulong asyncId,
            ulong messageId,
            SRV_READ_HASH_Response readHash
            )
        {
            smb3IOCtlResponsePacket packet = CreateReadHashIOCtlResponse(endpoint, messageId, readHash);

            ModifyAsyncHeader(packet, endpoint, asyncId);

            packet.Sign();

            return packet;
        }

        #endregion

        #region help function

        /// <summary>
        /// Set packet header field
        /// </summary>
        /// <param name="packet">The packet</param>
        /// <param name="endpoint">The client endpoint</param>
        /// <param name="messageId">The messageId of request packet</param>
        private void SetHeader(smb3SinglePacket packet, smb3Endpoint endpoint, ulong messageId)
        {
            SetHeader(packet, 0, endpoint, messageId);
        }


        /// <summary>
        /// Set packet header field
        /// </summary>
        /// <param name="packet">The packet</param>
        /// <param name="status">The status code for a response</param>
        /// <param name="endpoint">The client endpoint</param>
        /// <param name="messageId">The messageId of request packet</param>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void SetHeader(smb3SinglePacket packet, uint status, smb3Endpoint endpoint, ulong messageId)
        {
            packet.Endpoint = endpoint;

            smb3SinglePacket singleRequestPacket = context.FindRequestPacket(endpoint.EndpointId, messageId)
                as smb3SinglePacket;

            bool isRequestSigned = false;
            ushort clientRequestCredits = 0;

            if (singleRequestPacket == null)
            {
                packet.Header.MessageId = 0;
                packet.Header.Command = smb3Command.NEGOTIATE;
            }
            else
            {
                packet.Header.MessageId = singleRequestPacket.Header.MessageId;
                packet.Header.SessionId = singleRequestPacket.Header.SessionId;
                packet.Header.TreeId = singleRequestPacket.Header.TreeId;
                packet.Header.ProcessId = singleRequestPacket.Header.ProcessId;
                packet.Header.Command = singleRequestPacket.Header.Command;

                if (((singleRequestPacket).Header.Flags & Packet_Header_Flags_Values.FLAGS_SIGNED)
                    == Packet_Header_Flags_Values.FLAGS_SIGNED)
                {
                    isRequestSigned = true;
                }

                clientRequestCredits = singleRequestPacket.Header.CreditRequest_47_Response;
            }

            packet.Header.CreditRequest_47_Response = smb3Utility.CaculateResponseCredits(clientRequestCredits,
                context.connectionList[endpoint.EndpointId].commandSequenceWindow.Count);

            packet.Header.ProtocolId = smb3Consts.smb3ProtocolId;
            packet.Header.Signature = new byte[smb3Consts.SignatureSize];
            packet.Header.StructureSize = Packet_Header_StructureSize_Values.V1;
            packet.Header.Status = status;

            packet.Header.Flags |= Packet_Header_Flags_Values.FLAGS_SERVER_TO_REDIR;

            if (packet.Header.SessionId != 0 && 
                (isRequestSigned || context.ShouldPacketBeSigned(singleRequestPacket.GetSessionId())))
            {
                packet.Header.Flags |= Packet_Header_Flags_Values.FLAGS_SIGNED;

                (packet as smb3SinglePacket).SessionKey = context.globalSessionTable[singleRequestPacket.GetSessionId()].sessionKey;
            }
        }


        /// <summary>
        /// modify the header of packet because it is a async packet
        /// </summary>
        /// <param name="packet">The packet</param>
        /// <param name="endpoint">The endpoint of client</param>
        /// <param name="asyncId">The asyncId</param>
        private void ModifyAsyncHeader(smb3SinglePacket packet, smb3Endpoint endpoint, ulong asyncId)
        {
            packet.Header.Flags |= Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND;

            packet.Header.ProcessId = (uint)asyncId;
            packet.Header.TreeId = (uint)(asyncId >> 32);

            //for finnal async response, if Interim Response has been send, it does not grand
            //any credits because credits has been granded in Interim Response
            if (context.connectionList[endpoint.EndpointId].asyncCommandList.ContainsKey(asyncId))
            {
                packet.Header.CreditRequest_47_Response = 0;
            }
            else
            {
                //grand credits as normal
            }
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
                    transport.Dispose();
                    context.Dispose();
                }

                disposed = true;
            }
        }


        /// <summary>
        /// Deconstructure
        /// </summary>
        ~smb3Server()
        {
            Dispose(false);
        }

        #endregion
    }
}

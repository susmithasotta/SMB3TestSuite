//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ClientContext
// Description: smb3ClientContext contains the global setting and state
//              information of client
//-------------------------------------------------------------------------

using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// Contains gloal setting of client
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class smb3ClientGlobalContext : IDisposable
    {
        #region Fields

        internal bool requireMessageSigning;
        internal Dictionary<int, smb3ClientConnection> connectionTable;
        internal Dictionary<ulong, smb3ClientOpen> globalOpenTable;
        internal Dictionary<FilePath, smb3ClientFile> globalFileTable;
        internal Guid clientGuid;

        private readonly object contextLocker = new object();
        private int connectionCount;
        private bool disposed;

        #endregion

        #region Properties

        /// <summary>
        /// A Boolean that, if set, indicates that this node requires that messages MUST be signed 
        /// if the message is sent with a user security context that is neither anonymous nor guest.
        /// If not set, this node does not require that any messages be signed, but MAY still choose
        /// to do so if the other node requires it
        /// </summary>
        public bool RequireMessageSigning
        {
            get
            {
                return requireMessageSigning;
            }
        }


        /// <summary>
        /// A table of active smb3 transport connections, as specified in section 3.2.1.4,
        /// that are established to remote servers, indexed by the textual server name. 
        /// The textual server name is a fully qualified domain name, a NetBIOS name, or an IP address
        /// </summary>
        public ReadOnlyDictionary<int, smb3ClientConnection> ConnectionTable
        {
            get
            {
                return new ReadOnlyDictionary<int,smb3ClientConnection>(connectionTable);
            }
        }


        /// <summary>
        /// A table of the active opens to remote files or named pipes across all connections.
        /// </summary>
        public ReadOnlyDictionary<ulong, smb3ClientOpen> GlobalOpenTable
        {
            get
            {
                return new ReadOnlyDictionary<ulong, smb3ClientOpen>(globalOpenTable);
            }
        }


        /// <summary>
        /// A table of uniquely opened files, as specified in section 3.2.1.7, 
        /// indexed by a concatenation of the ServerName, ShareName, and the share-relative FileName,
        /// and also indexed by File.LeaseKey.
        /// </summary>
        public ReadOnlyDictionary<FilePath,smb3ClientFile> GlobalFileTable
        {
            get
            {
                return new ReadOnlyDictionary<FilePath, smb3ClientFile>(globalFileTable);
            }
        }


        /// <summary>
        /// A global identifier for this client
        /// </summary>
        public Guid ClientGuid
        {
            get
            {
                return clientGuid;
            }
            set
            {
                clientGuid = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public smb3ClientGlobalContext()
        {
            connectionTable = new Dictionary<int, smb3ClientConnection>();
            globalOpenTable = new Dictionary<ulong, smb3ClientOpen>();
            globalFileTable = new Dictionary<FilePath, smb3ClientFile>();
            clientGuid = Guid.NewGuid();
        }

        #endregion

        #region SDK Related Function

        /// <summary>
        /// Lock the global context
        /// </summary>
        internal void Lock()
        {
             Monitor.Enter(contextLocker);
        }


        /// <summary>
        /// Unlock the global context
        /// </summary>
        internal void Unlock()
        {
            Monitor.Exit(contextLocker);
        }


        /// <summary>
        /// Generate a new connectionId
        /// </summary>
        /// <returns>The new generated connectionId</returns>
        internal int GenerateNewConnectionId()
        {
            return connectionCount++;
        }


        /// <summary>
        /// Find the fileId of the open whose leaseKey matchs the input one
        /// </summary>
        /// <param name="leaseKey">The leaseKey</param>
        /// <returns>The fileId</returns>
        /// <exception cref="System.InvalidOperationException">Throw when the fileId can't be find</exception>
        internal FILEID FindFileIdByLeaseKey(byte[] leaseKey)
        {
            FILEID fileId = new FILEID();

            foreach (smb3ClientFile file in globalFileTable.Values)
            {
                if (smb3Utility.AreEqual(leaseKey, file.leaseKey))
                {
                    foreach (FILEID fileIdItem in file.openTable.Keys)
                    {
                        fileId = fileIdItem;
                        return fileId;
                    }
                }
            }

            throw new InvalidOperationException("Can't find a open based on the leaseKey");
        }


        /// <summary>
        /// Get the first free operation bucket in one open indexed by openKey
        /// </summary>
        /// <param name="openKey">Used to find the open</param>
        /// <returns>The index of founded bucket, -1 if not found</returns>
        internal int GetFirstFreeOperationBucketIndex(ulong openKey)
        {
            smb3ClientOpen open = globalOpenTable[openKey];

            if (open.resilientHandle)
            {
                for (int i = 0; i < open.operationBuckets.Length; i++)
                {
                    if (open.operationBuckets[i].free)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        #endregion

        #region Protocol Related Function

        /// <summary>
        /// Update context based on the endpoint and the packet
        /// </summary>
        /// <param name="smb3Event">contain the update information</param>
        internal void UpdateContext(smb3Event smb3Event)
        {
            //connectionId will never < 0, if it is really < 0,
            //un-expected situation happens or someone intents to do so,
            //just return and do not update context.
            if (smb3Event.ConnectionId < 0)
            {
                return;
            }

            switch (smb3Event.Type)
            {
                case smb3EventType.Connected:
                    HandleConnectedEvent(smb3Event);
                    break;
                case smb3EventType.PacketReceived:
                    HandlePacketReceivedEvent(smb3Event);
                    break;
                case smb3EventType.PacketSent:
                    HandlePacketSentEvent(smb3Event);
                    break;
                case smb3EventType.Disconnected:
                    HandleDisconnectedEvent(smb3Event);
                    break;
                default:
                    break;
            }
        }

        #region Connected Event

        /// <summary>
        /// Handle new connection event
        /// </summary>
        /// <param name="smb3Event">contain the update information</param>
        private void HandleConnectedEvent(smb3Event smb3Event)
        {
            smb3ClientConnection connection = new smb3ClientConnection();

            connection.sessionTable = new Dictionary<ulong, smb3ClientSession>();
            connection.outstandingRequests = new Dictionary<ulong, smb3OutStandingRequest>();
            connection.sequenceWindow = new List<ulong>();
            //when new connection established, the server will grand client 1 credit.
            connection.GrandCredit(1);
            connection.openTable = new Dictionary<FILEID, smb3ClientOpen>();
            connection.gssNegotiateToken = new byte[0];
            //Connection.Dialect MUST be set to "Unknown"
            connection.dialect = "Unknown";
            connection.requireSigning = false;
            connection.sessionResponses = new Dictionary<ulong, smb3SessionSetupResponsePacket>();
            //extraInfo contains server name, transport type
            string[] extraInfos = Encoding.Unicode.GetString(smb3Event.ExtraInfo).Split(
                new string[] { smb3Consts.ExtraInfoSeperator },
                StringSplitOptions.RemoveEmptyEntries);
            connection.serverName = extraInfos[0];
            connection.transportType = (smb3TransportType)Enum.Parse(typeof(smb3TransportType), extraInfos[1]);

            connectionTable.Add(smb3Event.ConnectionId, connection);
        }

        #endregion

        #region Packet Sent Event

        /// <summary>
        /// Handle packet sent event, it will be called when packet is sent
        /// </summary>
        /// <param name="smb3Event">contain the event information</param>
        private void HandlePacketSentEvent(smb3Event smb3Event)
        {
            if (smb3Event.Packet is smb3CompoundPacket)
            {
                smb3CompoundPacket compoundPacket = smb3Event.Packet as smb3CompoundPacket;

                foreach (smb3SinglePacket innerPacket in compoundPacket.Packets)
                {
                    smb3Event compoundPacketSentEvent = new smb3Event();
                    compoundPacketSentEvent.ConnectionId = smb3Event.ConnectionId;
                    compoundPacketSentEvent.Packet = innerPacket;
                    compoundPacketSentEvent.Type = smb3Event.Type;

                    HandlePacketSentEvent(compoundPacketSentEvent);
                }
            }
            else
            {
                SmbNegotiateRequestPacket smbNegotiatePacket = smb3Event.Packet as SmbNegotiateRequestPacket;
                ulong messageId = 0;
                ushort creditCharge = 0;

                if (smbNegotiatePacket != null)
                {
                    messageId = smbNegotiatePacket.Header.Mid;
                }
                else
                {
                    smb3SinglePacket singlePacket = smb3Event.Packet as smb3SinglePacket;
                    messageId = singlePacket.Header.MessageId;
                    creditCharge = singlePacket.Header.CreditCharge;

                    Handlesmb3SinglePacketSentEvent(smb3Event);
                }

                ComsumeSequenceNumber(smb3Event.ConnectionId, messageId, creditCharge);

                if (!(smb3Event.Packet is smb3CancelRequestPacket))
                {
                    AddPacketToOutStandingRequestList(smb3Event.ConnectionId, messageId, smb3Event.Packet);
                }
            }
        }


        private void ComsumeSequenceNumber(int connectionId, ulong messageId, ushort creditCharge)
        {
            int index =  -1;
            if (connectionTable[connectionId].sequenceWindow.Contains(messageId))
            {
                index = connectionTable[connectionId].sequenceWindow.IndexOf(messageId);
                connectionTable[connectionId].sequenceWindow.Remove(messageId);
            }

            if (connectionTable[connectionId].dialect != smb3Consts.NegotiateDialect2_02String
                && connectionTable[connectionId].supportLargeMtu
                && connectionTable[connectionId].transportType == smb3TransportType.Tcp)
            {
                for (int i = 0; i < (creditCharge - 1); i++)
                {
                    //connectionTable[connectionId].sequenceWindow.RemoveAt(index);
                }
            }
        }


        /// <summary>
        /// Handle sending single packet evnet
        /// </summary>
        /// <param name="smb3Event">contain the event information</param>
        private void Handlesmb3SinglePacketSentEvent(smb3Event smb3Event)
        {
            smb3SinglePacket singlePacket = smb3Event.Packet as smb3SinglePacket;

            switch (singlePacket.Header.Command)
            {
                case smb3Command.CREATE:
                    HandleCreateRequestSentEvent(smb3Event);
                    break;
                case smb3Command.OPLOCK_BREAK:
                    HandleOplockAckSentEvent(smb3Event);
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// Handle the event of sending create request packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleCreateRequestSentEvent(smb3Event smb3Event)
        {
            //currently no context need to be updated
        }


        /// <summary>
        /// Handle the event of sending oplock ack packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleOplockAckSentEvent(smb3Event smb3Event)
        {
            smb3OpLockBreakAckPacket oplockAck = smb3Event.Packet as smb3OpLockBreakAckPacket;

            if (oplockAck != null)
            {
                smb3ClientOpen open = globalOpenTable[oplockAck.GetFileId().Persistent];
                open.oplockLevel = (OplockLevel_Values)oplockAck.PayLoad.OplockLevel;
            }
            else
            {
                //lease break ack
                smb3LeaseBreakAckPacket leaseBreakAck = smb3Event.Packet as smb3LeaseBreakAckPacket;

                foreach (smb3ClientFile file in globalFileTable.Values)
                {
                    if (smb3Utility.AreEqual(file.leaseKey, leaseBreakAck.PayLoad.LeaseKey))
                    {
                        file.leaseState = leaseBreakAck.PayLoad.LeaseState;
                    }
                }
            }
        }


        /// <summary>
        /// Add packet to connection.OutStandingRequestList
        /// </summary>
        /// <param name="connectionId">The connectionId used to look up the connection</param>
        /// <param name="messageId">The messageId of the request</param>
        /// <param name="packet">The request packet</param>
        private void AddPacketToOutStandingRequestList(int connectionId, ulong messageId, smb3Packet packet)
        {
            smb3OutStandingRequest request = new smb3OutStandingRequest();

            request.request = packet;
            request.messageId = messageId;
            request.timeStamp = DateTime.Now;

            connectionTable[connectionId].outstandingRequests.Add(request.messageId, request);
        }

        #endregion

        #region Packet Received Event

        /// <summary>
        /// Handle PacketReceived event
        /// </summary>
        /// <param name="smb3Event">contain the update information</param>
        private void HandlePacketReceivedEvent(smb3Event smb3Event)
        {
            smb3CompoundPacket compoundPacket = smb3Event.Packet as smb3CompoundPacket;

            if (compoundPacket != null)
            {
                for (int i = 0; i < compoundPacket.Packets.Count; i++)
                {
                    if (i != 0)
                    {
                        compoundPacket.Packets[i].OuterCompoundPacket = compoundPacket;
                    }

                    compoundPacket.Packets[i].IsInCompoundPacket = true;

                    if (i == (compoundPacket.Packets.Count - 1))
                    {
                        compoundPacket.Packets[i].IsLast = true;
                    }

                    smb3Event innerEvent = new smb3Event();
                    innerEvent.Packet = compoundPacket.Packets[i];
                    innerEvent.ConnectionId = smb3Event.ConnectionId;
                    innerEvent.Type = smb3Event.Type;

                    HandlePacketReceivedEvent(innerEvent);
                }
            }
            else
            {

                       
                //3.2.5.1.1 Finding the Application Request for This Response
                bool isResponseValid = IsAValidResponse(smb3Event.Packet, smb3Event.ConnectionId);

                if (!isResponseValid)
                {
                    throw new InvalidOperationException("Received a response which client did not request before");
                }

                //3.2.5.1.3 Granting Message Credits
                GrandMessageCredit(smb3Event.Packet, smb3Event.ConnectionId);

                HandleConcretePacketReceivedEvent(smb3Event);

                SetSessionKeyInPacket(smb3Event.ConnectionId, smb3Event.Packet);

                //3.2.5.1.2 Verifying the Signature
                bool isSignatureMatch = smb3Event.Packet.VerifySignature();

                if (!isSignatureMatch)
                {
                   // throw new InvalidOperationException("Received a response whose signature is not valid");
                }
            }
        }


        /// <summary>
        /// Do context update based on receiving a 
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleConcretePacketReceivedEvent(smb3Event smb3Event)
        {
            if (smb3Event.Packet is smb3ErrorResponsePacket)
            {
                HandleErrorResponseEvent(smb3Event);
                return;
            }

            smb3Command command = (smb3Event.Packet as smb3SinglePacket).Header.Command;

            switch (command)
            {
                case smb3Command.NEGOTIATE:
                    HandleNegotiateResponseEvent(smb3Event);
                    break;
                case smb3Command.SESSION_SETUP:
                    HandleSessionSetupResponseEvent(smb3Event);
                    break;
                case smb3Command.LOGOFF:
                    HandleLogoffResponseEvent(smb3Event);
                    break;
                case smb3Command.TREE_CONNECT:
                    HandleTreeConnectResponseEvent(smb3Event);
                    break;
                case smb3Command.TREE_DISCONNECT:
                    HandleTreeDisconnectResponseEvent(smb3Event);
                    break;
                case smb3Command.CREATE:
                    HandleCreateResponseEvent(smb3Event);
                    break;
                case smb3Command.CLOSE:
                    HandleCloseResponseEvent(smb3Event);
                    break;
                case smb3Command.OPLOCK_BREAK:
                    HandleOplockBreakResponseEvent(smb3Event);
                    break;
                case smb3Command.LOCK:
                    HandleLockResponseEvent(smb3Event);
                    break;
                case smb3Command.IOCTL:
                    HandleIOCtlResponseEvent(smb3Event);
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// Handle the event of receiving negotiate response packet
        /// </summary>
        /// <param name="smb3Event">The event information</param>
        private void HandleNegotiateResponseEvent(smb3Event smb3Event)
        {
            smb3NegotiateResponsePacket packet = smb3Event.Packet as smb3NegotiateResponsePacket;

            if (packet.Header.Status == 0)
            {
                if ((packet.PayLoad.SecurityMode & NEGOTIATE_Response_SecurityMode_Values.NEGOTIATE_SIGNING_ENABLED)
                    == NEGOTIATE_Response_SecurityMode_Values.NEGOTIATE_SIGNING_ENABLED)
                {
                    connectionTable[smb3Event.ConnectionId].maxReadSize = packet.PayLoad.MaxReadSize;
                    connectionTable[smb3Event.ConnectionId].maxWriteSize = packet.PayLoad.MaxWriteSize;
                    connectionTable[smb3Event.ConnectionId].maxTransactSize = packet.PayLoad.MaxTransactSize;
                    connectionTable[smb3Event.ConnectionId].serverGuid = packet.PayLoad.ServerGuid;
                    connectionTable[smb3Event.ConnectionId].gssNegotiateToken = (byte[])packet.PayLoad.Buffer.Clone();
                }

                if ((packet.PayLoad.SecurityMode & NEGOTIATE_Response_SecurityMode_Values.NEGOTIATE_SIGNING_REQUIRED)
                    == NEGOTIATE_Response_SecurityMode_Values.NEGOTIATE_SIGNING_REQUIRED)
                {
                    connectionTable[smb3Event.ConnectionId].requireSigning = true;
                }

                if (packet.PayLoad.DialectRevision != DialectRevision_Values.V3)
                {
                    if (packet.PayLoad.DialectRevision == DialectRevision_Values.V1)
                    {
                        connectionTable[smb3Event.ConnectionId].dialect = "2.002";
                    }
                    else
                    {
                        connectionTable[smb3Event.ConnectionId].dialect = "2.100";
                    }

                    if ((packet.PayLoad.Capabilities & 
                        NEGOTIATE_Response_Capabilities_Values.SMB2_GLOBAL_CAP_LEASING)
                        == NEGOTIATE_Response_Capabilities_Values.SMB2_GLOBAL_CAP_LEASING)
                    {
                        connectionTable[smb3Event.ConnectionId].supportLeasing = true;
                    }

                    if ((packet.PayLoad.Capabilities & NEGOTIATE_Response_Capabilities_Values.SMB2_GLOBAL_CAP_LARGE_MTU)
                        == NEGOTIATE_Response_Capabilities_Values.SMB2_GLOBAL_CAP_LARGE_MTU)
                    {
                        connectionTable[smb3Event.ConnectionId].supportLargeMtu = true;
                    }
                }
             }
        }


        /// <summary>
        /// Handle the event of receiving sessionSetup response
        /// </summary>
        /// <param name="smb3Event">The event information</param>
        private void HandleSessionSetupResponseEvent(smb3Event smb3Event)
        {
            smb3SessionSetupResponsePacket packet = (smb3SessionSetupResponsePacket)smb3Event.Packet;
            ulong sessionID = packet.GetSessionId();

            //only keep the latest one if the sessionId is the same
            if (connectionTable[smb3Event.ConnectionId].sessionResponses.ContainsKey(sessionID))
            {
                connectionTable[smb3Event.ConnectionId].sessionResponses.Remove(sessionID);
            }

            connectionTable[smb3Event.ConnectionId].sessionResponses.Add(sessionID, packet);

            if (!connectionTable[smb3Event.ConnectionId].sessionTable.ContainsKey(sessionID))
            {
                HandleNewAuthenticateEvent(packet, smb3Event.ConnectionId);
            }
        }

        /// <summary>
        /// Handle the event of receiving session setup response and the session is a new session
        /// </summary>
        /// <param name="packet">The response packet</param>
        /// <param name="connectionId">Used to look up the connection</param>
        private void HandleNewAuthenticateEvent(smb3SessionSetupResponsePacket packet, int connectionId)
        {
            if (packet.Header.Status == 0)
            {
                smb3ClientConnection connection = connectionTable[connectionId];

                if (connection.Gss.NeedContinueProcessing)
                {
                    connection.Gss.Initialize(packet.PayLoad.Buffer);
                }

                if (!connection.Gss.NeedContinueProcessing)
                {
                    smb3ClientSession session = new smb3ClientSession();
                    session.sessionId = packet.GetSessionId();
                    session.treeConnectTable = new Dictionary<uint, smb3ClientTreeConnect>();
                    session.sessionKey = connection.Gss.SessionKey;
                    session.connection = connection;

                    if (connection.globalRequireMessageSignCopy || connection.requireSigning)
                    {
                        session.shouldSign = true;
                    }

                    if ((packet.PayLoad.SessionFlags & SessionFlags_Values.SESSION_FLAG_IS_NULL)
                        == SessionFlags_Values.SESSION_FLAG_IS_NULL)
                    {
                        session.shouldSign = false;
                    }

                    if (((packet.PayLoad.SessionFlags & SessionFlags_Values.SESSION_FLAG_IS_GUEST)
                        == SessionFlags_Values.SESSION_FLAG_IS_GUEST) && session.shouldSign)
                    {
                        throw new InvalidOperationException("SMB2_SESSION_FLAG_IS_GUEST bit is set in the SessionFlags field"
                        + "of the smb3 SESSION_SETUP Response AND if Session.ShouldSign is TRUE, this state is not valid");
                    }
                    if (smb3Client.binding == 0)
                    {
                        connectionTable[connectionId].sessionTable.Add(session.sessionId, session);
                    }
                    else
                        connectionTable[connectionId].sessionTable.Add(session.sessionId, connectionTable[connectionId-1].sessionTable[session.sessionId]);

                    //release it, set to null;
                    connection.ReleaseSspiClient();
                }
            }
        }

        /// <summary>
        /// Handle the event of receiving log off response packet
        /// </summary>
        /// <param name="smb3Event">The event information</param>
        private void HandleLogoffResponseEvent(smb3Event smb3Event)
        {
            smb3LogOffResponsePacket logOff = smb3Event.Packet as smb3LogOffResponsePacket;

            //because after remove the session from session table, the session key will not be 
            //accessable, but verify signature need this value, so set it first before it is removed
            SetSessionKeyInPacket(smb3Event.ConnectionId, logOff);

            connectionTable[smb3Event.ConnectionId].sessionTable.Remove(logOff.GetSessionId());
        }


        /// <summary>
        /// Handle the event of receiving treeConnect response
        /// </summary>
        /// <param name="smb3Event">Contain event information</param>
        private void HandleTreeConnectResponseEvent(smb3Event smb3Event)
        {
            smb3TreeConnectResponsePacket packet = smb3Event.Packet as smb3TreeConnectResponsePacket;

            smb3TreeConnectRequestPacket requestPacket = connectionTable[smb3Event.ConnectionId].outstandingRequests[
                packet.Header.MessageId].request as smb3TreeConnectRequestPacket;

            string shareFullName = Encoding.Unicode.GetString(requestPacket.PayLoad.Buffer);
            int shareNameIndex = shareFullName.LastIndexOf('\\');

            smb3ClientTreeConnect treeConnect = new smb3ClientTreeConnect();
            treeConnect.treeConnectId = packet.GetTreeId();
            treeConnect.maximalAccess = packet.PayLoad.MaximalAccess;
            treeConnect.shareName = shareFullName.Substring(shareNameIndex + 1, shareFullName.Length - shareNameIndex - 1);

            treeConnect.session = connectionTable[smb3Event.ConnectionId].sessionTable[packet.GetSessionId()];

            ulong sessionID = packet.GetSessionId();

            connectionTable[smb3Event.ConnectionId].sessionTable[sessionID].treeConnectTable.Add(
                treeConnect.treeConnectId,
                treeConnect);
            Console.WriteLine("TreeConnect on : Connection: " + smb3Event.ConnectionId + " Session : " + sessionID + " TreeID :" + treeConnect.treeConnectId);
            treeConnect.openTable = new Dictionary<FILEID, smb3ClientOpen>();
        }


        /// <summary>
        /// Handle the event of receiving treeDisconnect response packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleTreeDisconnectResponseEvent(smb3Event smb3Event)
        {
            smb3TreeDisconnectResponsePacket packet = smb3Event.Packet as smb3TreeDisconnectResponsePacket;

            smb3ClientSession session = connectionTable[smb3Event.ConnectionId].sessionTable[packet.GetSessionId()];

            session.treeConnectTable.Remove(packet.GetTreeId());
        }


        /// <summary>
        /// Handle the event of receiving create response packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleCreateResponseEvent(smb3Event smb3Event)
        {
            smb3CreateResponsePacket responsePacket = smb3Event.Packet as smb3CreateResponsePacket;

            smb3CreateRequestPacket requestPacket = connectionTable[smb3Event.ConnectionId].outstandingRequests[
                responsePacket.Header.MessageId].request as smb3CreateRequestPacket;

            CREATE_CONTEXT_Values[] createContextsRequest = requestPacket.GetCreateContexts();

            bool isReestablishedOpen = false;

            if (createContextsRequest != null)
            {
                foreach (CREATE_CONTEXT_Values createContext in createContextsRequest)
                {
                    if (smb3Utility.GetContextType(createContext) == CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_RECONNECT)
                    {
                        isReestablishedOpen = true;
                        break;
                    }
                }
            }

            smb3ClientSession session = connectionTable[smb3Event.ConnectionId].sessionTable[responsePacket.GetSessionId()];
            smb3ClientTreeConnect treeConnect = session.treeConnectTable[responsePacket.GetTreeId()];

            //Receiving an smb3 CREATE Response for an Open Reestablishment
            if (isReestablishedOpen)
            {
                #region action for re-establish open

                smb3ClientOpen open = globalOpenTable[responsePacket.GetFileId().Persistent];

                open.fileId = responsePacket.GetFileId();
                open.treeConnect = treeConnect;
                open.connection = connectionTable[smb3Event.ConnectionId];

                connectionTable[smb3Event.ConnectionId].openTable.Add(responsePacket.GetFileId(), open);

                #endregion
            }
            else
            {
                #region Common action for all new create response

                //Receiving an smb3 CREATE Response for a New Create Operation
                smb3ClientOpen open = new smb3ClientOpen();
                open.clientLocalId = (uint)smb3Event.ConnectionId;
                open.fileId = responsePacket.GetFileId();
                open.treeConnect = treeConnect;
                open.connection = connectionTable[smb3Event.ConnectionId];
                open.oplockLevel = responsePacket.PayLoad.OplockLevel;
                open.durable = false;
                open.maximalAccess = 0;
                open.fileName = new FilePath();
                open.fileName.serverName = connectionTable[smb3Event.ConnectionId].serverName;
                open.fileName.shareName = treeConnect.shareName;
                open.fileName.filePathName = requestPacket.RetreivePathName();
                open.operationBuckets = new OperationBucket[smb3Consts.OperationBucketsCount];

                for (int i = 0; i < open.operationBuckets.Length; i++)
                {
                    open.operationBuckets[i] = new OperationBucket();
                    open.operationBuckets[i].free = true;
                    open.operationBuckets[i].sequenceNumber = 0;
                }

                #endregion

                #region special action for special createcontext

                CREATE_CONTEXT_Values[] createContextsResponse = responsePacket.GetCreateContexts();

                if (createContextsResponse != null)
                {
                    foreach (CREATE_CONTEXT_Values createContextResponse in createContextsResponse)
                    {
                        CreateContextTypeValue createContextResponseType = smb3Utility.GetContextType(createContextResponse);
                        if (createContextResponseType
                            == CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_REQUEST)
                        {
                            foreach (CREATE_CONTEXT_Values createContextRequest in createContextsRequest)
                            {
                                if (smb3Utility.GetContextType(createContextRequest) 
                                    == CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_REQUEST)
                                {
                                    open.durable = true;
                                    break;
                                }
                            }
                        }
                        else if (createContextResponseType == CreateContextTypeValue.SMB2_CREATE_QUERY_MAXIMAL_ACCESS_REQUEST)
                        {
                            byte[] queryMaximalAccessBuffer = smb3Utility.GetDataFieldInCreateContext(createContextResponse);

                            CREATE_QUERY_MAXIMAL_ACCESS queryMaximalAccess = smb3Utility.ToStructure<CREATE_QUERY_MAXIMAL_ACCESS>(
                                queryMaximalAccessBuffer);

                            if (queryMaximalAccess.QueryStatus == 0)
                            {
                                open.maximalAccess = queryMaximalAccess.MaximalAccess.ACCESS_MASK;
                            }
                        }
                        else if (createContextResponseType == CreateContextTypeValue.SMB2_CREATE_REQUEST_LEASE)
                        {
                            byte[] responseLeaseArray = smb3Utility.GetDataFieldInCreateContext(createContextResponse);

                            CREATE_RESPONSE_LEASE responseLease = smb3Utility.ToStructure<CREATE_RESPONSE_LEASE>(responseLeaseArray);
                            globalFileTable[open.fileName].leaseState = responseLease.LeaseState;
                        }
                        else
                        {
                            //do nothing
                        }
                    }
                }

                #endregion

                #region add the open to globoal open table, Connection.opentable, and File.OpenTable

                globalOpenTable.Add(responsePacket.GetFileId().Persistent, open);
                connectionTable[smb3Event.ConnectionId].openTable.Add(responsePacket.GetFileId(), open);

                // Binding handle

                 if (connectionTable[smb3Event.ConnectionId].sessionTable.ContainsKey(session.SessionId))
                {
                    if (connectionTable[smb3Event.ConnectionId].sessionTable[session.SessionId].treeConnectTable.ContainsKey(treeConnect.TreeConnectId))
                    {
                        connectionTable[smb3Event.ConnectionId].sessionTable[session.SessionId].treeConnectTable[treeConnect.TreeConnectId].openTable.Add(responsePacket.GetFileId(), open);
                        Console.WriteLine("Open on : Connection: " + smb3Event.ConnectionId + " Session : " + session.SessionId + " TreeID :" + treeConnect.TreeConnectId + " Open or File ID :" + responsePacket.GetFileId());
                    }
                }
                if (connectionTable[smb3Event.ConnectionId].supportLeasing)
                {
                    globalFileTable[open.fileName].openTable.Add(open.fileId, open);
                }

                #endregion
            }
        }


        /// <summary>
        /// Handle the event of receiving close response packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleCloseResponseEvent(smb3Event smb3Event)
        {
            smb3CloseResponsePacket packet = smb3Event.Packet as smb3CloseResponsePacket;
            smb3CloseRequestPacket requestPacket = connectionTable[smb3Event.ConnectionId].outstandingRequests[packet.Header.MessageId].request
                as smb3CloseRequestPacket;

            FILEID fileId = requestPacket.GetFileId();
            // if the requestPacket is in a compound packet, fileId may not be granted.
            // in this situtation, the fileId must retrieved from compound response
            if (fileId.Persistent == ulong.MaxValue && fileId.Volatile == ulong.MaxValue)
            {
                //second chance
                fileId = smb3Utility.ResolveFileIdInCompoundResponse(fileId, packet);
            }

            smb3ClientOpen open = connectionTable[smb3Event.ConnectionId].openTable[fileId];

            if (connectionTable[smb3Event.ConnectionId].supportLeasing)
            {
                smb3ClientFile file = globalFileTable[open.fileName];
                file.openTable.Remove(open.fileId);

                if (file.openTable.Count == 0)
                {
                    globalFileTable.Remove(open.fileName);
                }
            }

            globalOpenTable.Remove(fileId.Persistent);
            connectionTable[smb3Event.ConnectionId].openTable.Remove(fileId);
        }


        /// <summary>
        /// Handle the event of receiving oplock break notification or oplock break response
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleOplockBreakResponseEvent(smb3Event smb3Event)
        {
            smb3SinglePacket singlePacket = smb3Event.Packet as smb3SinglePacket;

            if (singlePacket.Header.MessageId == ulong.MaxValue)
            {
                //update context will be done when sending oplock_ack
            }
            else
            {
                //no action required.
            }
        }


        private void HandleLockResponseEvent(smb3Event smb3Event)
        {
            smb3LockResponsePacket packet = smb3Event.Packet as smb3LockResponsePacket;

            smb3LockRequestPacket requestPacket = connectionTable[smb3Event.ConnectionId].outstandingRequests[packet.Header.MessageId].request
                as smb3LockRequestPacket;

            FILEID fileId = requestPacket.GetFileId();
            // if the requestPacket is in a compound packet, fileId may not be granted.
            // in this situtation, the fileId must retrieved from compound response
            if (fileId.Persistent == ulong.MaxValue && fileId.Volatile == ulong.MaxValue)
            {
                //second chance
                fileId = smb3Utility.ResolveFileIdInCompoundResponse(fileId, packet);
            }

            smb3ClientOpen open = globalOpenTable[fileId.Persistent];

            if (open.resilientHandle)
            {
                //The LockSequence field of the smb3 lock request MUST be set to ((BucketIndex + 1) << 4) + BucketSequence.
                int bucketIndex = ((int)requestPacket.PayLoad.LockSequence >> 4) - 1;
                open.operationBuckets[bucketIndex].free = false;
            }
        }


        private void HandleIOCtlResponseEvent(smb3Event smb3Event)
        {
            smb3IOCtlResponsePacket packet = smb3Event.Packet as smb3IOCtlResponsePacket;

            switch ((CtlCode_Values)packet.PayLoad.CtlCode)
            {
                case CtlCode_Values.FSCTL_LMR_REQUEST_RESILIENCY:
                    globalOpenTable[packet.GetFileId().Persistent].resilientHandle = true;
                    globalOpenTable[packet.GetFileId().Persistent].durable = true;
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// Handle the event of receiving error response
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleErrorResponseEvent(smb3Event smb3Event)
        {
            smb3ErrorResponsePacket packet = smb3Event.Packet as smb3ErrorResponsePacket;

            if (IsInterimResponse(packet))
            {
                smb3OutStandingRequest outStandingRequest = 
                    connectionTable[smb3Event.ConnectionId].outstandingRequests[packet.Header.MessageId];

                outStandingRequest.isHandleAsync = true;
                outStandingRequest.asyncId = smb3Utility.AssembleToAsyncId(
                    packet.Header.ProcessId, packet.Header.TreeId);
            }
        }


        /// <summary>
        /// Test if the error packet is an interim response packet
        /// </summary>
        /// <param name="packet">The error packet</param>
        /// <returns>True if it is an interim packet, otherwise false</returns>
        private bool IsInterimResponse(smb3ErrorResponsePacket packet)
        {
            if ((smb3Status)packet.Header.Status == smb3Status.STATUS_PENDING
                && ((packet.Header.Flags & Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND)
                == Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Test the messageId to see if the response packet has corresponding request.
        /// </summary>
        /// <param name="packet">The response packet</param>
        /// <param name="connectionId">The connectionId</param>
        /// <returns>True if it is valid, else false</returns>
        private bool IsAValidResponse(smb3Packet packet,int connectionId)
        {
            smb3SinglePacket singlePacket = packet as smb3SinglePacket;
            bool IsValidReponse = false;

            if (singlePacket.Header.MessageId == ulong.MaxValue)
            {
                return true;
            }
            else
            {
                for (int i = 0; i <= connectionTable.Count; i++)
                {
                     if (connectionTable[i].outstandingRequests.ContainsKey(singlePacket.Header.MessageId))
                     {
                         IsValidReponse= true;
                         break;
                     }
                }
                if (IsValidReponse)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        /// <summary>
        /// Set the sessionkey field of the packet
        /// </summary>
        /// <param name="connectionId">Used to find the connection</param>
        /// <param name="packet">The smb3 packet</param>
        private void SetSessionKeyInPacket(int connectionId, smb3Packet packet)
        {
            smb3SinglePacket singlePacket = packet as smb3SinglePacket;

            if (singlePacket != null)
            {
                if ((singlePacket.Header.Flags & Packet_Header_Flags_Values.FLAGS_SIGNED)
                    != Packet_Header_Flags_Values.FLAGS_SIGNED)
                {
                    return;
                }

                if ((singlePacket is smb3LogOffResponsePacket) && 
                    !connectionTable[connectionId].sessionTable.ContainsKey(singlePacket.GetSessionId()))
                {
                    //the packet.sessionKey has been set before.
                }
                else
                {
                    if(connectionTable[connectionId].sessionTable.ContainsKey(singlePacket.GetSessionId()))
                    {
                        singlePacket.SessionKey = connectionTable[connectionId].sessionTable[singlePacket.GetSessionId()].sessionKey;
                    }
                    else
                    {
                        for (int i = 0; i <= connectionTable.Count; i++)
                        {
                            if (connectionTable[i].sessionTable.ContainsKey(singlePacket.GetSessionId()))
                            {
                                singlePacket.SessionKey = connectionTable[i].sessionTable[singlePacket.GetSessionId()].sessionKey;
                                break;
                            }
                    
                        }
                    }
                }
            }
            else
            {
                //it is smb negotiate packet, do not need verify signature.
            }
        }


        /// <summary>
        /// Grand credit to client, the credit comes from response packet
        /// </summary>
        /// <param name="packet">The response packet</param>
        /// <param name="connectionId">Used to find the connection</param>
        private void GrandMessageCredit(smb3Packet packet, int connectionId)
        {
             smb3SinglePacket singlePacket = packet as smb3SinglePacket;

             connectionTable[connectionId].GrandCredit(singlePacket.Header.CreditRequest_47_Response);
        }


        /// <summary>
        /// Add File to globalFileTable, if the File indexed by serverName, shareName, and fileName
        /// Exists, do not add it again.
        /// </summary>
        /// <param name="serverName">The server name</param>
        /// <param name="shareName">The share name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>The lease key of the file</returns>
        internal byte[] AddFile(string serverName, string shareName, string fileName)
        {
            byte[] leaseKey = null;

            FilePath filePath = new FilePath();
            filePath.serverName = serverName;
            filePath.shareName = shareName;
            filePath.filePathName = fileName;

            if (!globalFileTable.ContainsKey(filePath))
            {
                smb3ClientFile clientFile = new smb3ClientFile();
                clientFile.openTable = new Dictionary<FILEID, smb3ClientOpen>();
                clientFile.leaseState = LeaseStateValues.SMB2_LEASE_NONE;
                clientFile.leaseKey = smb3Utility.GenerateLeaseKey();
                leaseKey = clientFile.leaseKey;

                globalFileTable.Add(filePath, clientFile);
            }
            else
            {
                leaseKey = globalFileTable[filePath].leaseKey;
            }

            return leaseKey;
        }


        #endregion

        #region Disconnected Event

        /// <summary>
        /// Handle disconnected event
        /// </summary>
        /// <param name="smb3Event">contain the update information</param>
        private void HandleDisconnectedEvent(smb3Event smb3Event)
        {
            if (connectionTable != null)
            {
                if (connectionTable.ContainsKey(smb3Event.ConnectionId))
                {
                    foreach (KeyValuePair<FILEID, smb3ClientOpen> openPair in connectionTable[smb3Event.ConnectionId].openTable)
                    {
                        if (globalOpenTable[openPair.Key.Persistent].durable)
                        {
                            globalOpenTable[openPair.Key.Persistent].connection = null;
                            globalOpenTable[openPair.Key.Persistent].treeConnect = null;
                        }
                        else
                        {
                            globalOpenTable.Remove(openPair.Key.Persistent);
                        }
                    }

                    connectionTable[smb3Event.ConnectionId].Dispose();
                    connectionTable.Remove(smb3Event.ConnectionId);
                }
            }
        }

        #endregion

        #endregion

        #region Implement IDispose interface

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
        /// <param name="disposing">Indicate if calling this function mannually</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free managed resources & other reference types
                    if (connectionTable != null)
                    {
                        foreach (smb3ClientConnection connection in connectionTable.Values)
                        {
                            connection.Dispose();
                        }
                    }
                }

                // Call the appropriate methods to clean up unmanaged resources.
                // If disposing is false, only the following code is executed.

                disposed = true;
            }
        }


        /// <summary>
        /// Deconstructor
        /// </summary>
        ~smb3ClientGlobalContext()
        {
            Dispose(false);
        }

        #endregion
    }
}

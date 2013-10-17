//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ServerContext
// Description: A stucture contains information about context
//-------------------------------------------------------------------------

using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// A stucture contains information about context
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class smb3ServerContext : IDisposable
    {
        #region Field

        internal bool requireMessageSigning;
        internal Dictionary<string, smb3ServerShare> shareList;
        internal Dictionary<ulong, smb3ServerOpen> globalOpenTable;
        internal Dictionary<ulong, smb3ServerSession> globalSessionTable;
        internal Dictionary<int, smb3ServerConnection> connectionList;
        internal Guid serverGuid;
        internal DateTime serverStartTime;
        internal bool isDfsCapable;

        //2.1 dialect feature
        private Dictionary<Guid, smb3LeaseTable> globalLeaseTableList;

        internal smb3TransportType transportType;
        private bool disposed;

        #endregion

        #region Properties

        /// <summary>
        /// A Boolean that, if set, indicates that this node requires that messages MUST be signed 
        /// if the message is sent with a user security context that is neither anonymous nor guest.
        /// If not set, this node does not require that any messages be signed, 
        /// but MAY still choose to do so if the other node requires it
        /// </summary>
        public bool RequireMessageSigning
        {
            get
            {
                return requireMessageSigning;
            }
        }

        /// <summary>
        /// A list of available shares for the system. 
        /// The structure of a share is as specified in section 3.3.1.7 and is uniquely indexed by the share name.
        /// </summary>
        public ReadOnlyDictionary<string, smb3ServerShare> ShareList
        {
            get
            {
                return new ReadOnlyDictionary<string, smb3ServerShare>(shareList);
            }
        }

        /// <summary>
        /// A table containing all the files opened by remote clients on the server, indexed by Open.DurableFileId.
        /// The structure of an open is as specified in section 3.3.1.11. The table MUST support enumeration of all 
        /// entries in the table.
        /// </summary>
        public ReadOnlyDictionary<ulong, smb3ServerOpen> GlobalOpenTable
        {
            get
            {
                return new ReadOnlyDictionary<ulong, smb3ServerOpen>(globalOpenTable);
            }
        }

        /// <summary>
        /// A list of all the active sessions established to this server, indexed by the Session.SessionId.
        /// The server MUST also be able to search the list by security principal,
        /// and the list MUST allow for multiple sessions with the same security principal on different connections
        /// </summary>
        public ReadOnlyDictionary<ulong, smb3ServerSession> GlobalSessionTable
        {
            get
            {
                return new ReadOnlyDictionary<ulong, smb3ServerSession>(globalSessionTable);
            }
        }

        /// <summary>
        /// A list of all open connections on the server, indexed by the connection endpoint addresses.
        /// </summary>
        public ReadOnlyDictionary<int, smb3ServerConnection> ConnectionList
        {
            get
            {
                return new ReadOnlyDictionary<int, smb3ServerConnection>(connectionList);
            }
        }

        /// <summary>
        /// A global identifier for this server
        /// </summary>
        public Guid ServerGuid
        {
            get
            {
                return serverGuid;
            }
        }

        /// <summary>
        /// The start time of the smb3 server
        /// </summary>
        public DateTime ServerStartTime
        {
            get
            {
                return serverStartTime;
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
        }

        /// <summary>
        /// A list of all the lease tables as described in 3.3.1.12, indexed by the ClientGuid.
        /// </summary>
        public ReadOnlyDictionary<Guid, smb3LeaseTable> GlobalLeaseTableList
        {
            get
            {
                return new ReadOnlyDictionary<Guid, smb3LeaseTable>(globalLeaseTableList);
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public smb3ServerContext()
        {
            globalOpenTable = new Dictionary<ulong, smb3ServerOpen>();
            globalSessionTable = new Dictionary<ulong, smb3ServerSession>();
            serverGuid = Guid.NewGuid();
            connectionList = new Dictionary<int, smb3ServerConnection>();
            serverStartTime = DateTime.Now;

            shareList = new Dictionary<string, smb3ServerShare>();
            globalLeaseTableList = new Dictionary<Guid, smb3LeaseTable>();
        }

        #endregion

        #region Protocol Related Functions

        /// <summary>
        /// Update context based on the endpoint and the packet
        /// </summary>
        /// <param name="smb3Event">contain the update information</param>
        internal void UpdateContext(smb3Event smb3Event)
        {
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
            }
        }


        /// <summary>
        /// Handle new connection event
        /// </summary>
        /// <param name="smb3Event">contain the update information</param>
        private void HandleConnectedEvent(smb3Event smb3Event)
        {
            smb3ServerConnection connection = new smb3ServerConnection();

            connection.commandSequenceWindow = new List<ulong>();
            //when a new connection established, the sequncewindow will contain one sequnce number.
            connection.GrandCredit(1);
            connection.asyncCommandList = new Dictionary<ulong, AsyncCommand>();
            connection.requestList = new Dictionary<ulong,smb3Packet>();
            connection.clientCapabilities = 0;
            connection.negotiateDialect = 0xffff;
            connection.dialect = "Unknown";
            connection.shouldSign = false;
            connection.connectionId = smb3Event.ConnectionId;

            connectionList.Add(smb3Event.ConnectionId, connection);
        }


        /// <summary>
        /// Handle PacketReceived event
        /// </summary>
        /// <param name="smb3Event">contain the update information</param>
        private void HandlePacketReceivedEvent(smb3Event smb3Event)
        {
            //An smb3 CANCEL Request is the only request received by the server that
            //is not signed and does not contain a sequence number that must be checked.
            //Thus, the server MUST NOT process the received packet as specified in sections 3.3.5.2.2 and 3.3.5.2.3.
            if (smb3Event.Packet is smb3CancelRequestPacket)
            {
                return;
            }

            if (smb3Event.Packet is smb3CompoundPacket)
            {
                smb3CompoundPacket compoundPacket = smb3Event.Packet as smb3CompoundPacket;

                foreach (smb3SinglePacket innerPacket in compoundPacket.Packets)
                {
                    innerPacket.OuterCompoundPacket = compoundPacket;
                    smb3Event compoundPacketReceivedEvent = new smb3Event();
                    compoundPacketReceivedEvent.ConnectionId = smb3Event.ConnectionId;
                    compoundPacketReceivedEvent.Packet = innerPacket;
                    compoundPacketReceivedEvent.Type = smb3Event.Type;

                    HandlePacketReceivedEvent(compoundPacketReceivedEvent);
                }
            }
            else
            {
                bool sequenceIdAllowed = VerifyMessageId(smb3Event.Packet, smb3Event.ConnectionId);

                if (!sequenceIdAllowed)
                {
                    throw new InvalidOperationException("Received a packet whose messageId is not valid");
                }

                SetSessionKeyInPacket(smb3Event.ConnectionId, smb3Event.Packet);

                bool isMatch = smb3Event.Packet.VerifySignature();

                if (!isMatch)
                {
                    throw new InvalidOperationException("signature is not correct.");
                }

                ulong messageId = 0;

                smb3SinglePacket singlePacket = smb3Event.Packet as smb3SinglePacket;

                if (singlePacket != null)
                {
                    messageId = singlePacket.Header.MessageId;
                }

                connectionList[smb3Event.ConnectionId].requestList.Add(messageId, smb3Event.Packet);

                if (singlePacket != null)
                {
                    switch (singlePacket.Header.Command)
                    {
                        case smb3Command.QUERY_DIRECTORY:
                            HandleReceiveQueryDirectoryRequestEvent(smb3Event);
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Handle the event of receiving query directory request
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleReceiveQueryDirectoryRequestEvent(smb3Event smb3Event)
        {
            smb3QueryDirectoryRequestPacket packet = smb3Event.Packet as smb3QueryDirectoryRequestPacket;

            byte[] fileNameArray = new byte[0];

            string fileName = string.Empty;

            if (packet.PayLoad.FileNameLength != 0)
            {
                fileNameArray = new byte[packet.PayLoad.FileNameLength];

                Array.Copy(packet.PayLoad.Buffer, packet.PayLoad.FileNameOffset - smb3Consts.FileNameOffsetInQueryDirectoryRequest,
                    fileNameArray, 0, fileNameArray.Length);

                fileName = Encoding.Unicode.GetString(fileNameArray);
            }

            smb3ServerOpen open = globalSessionTable[packet.GetSessionId()].openTable[packet.GetFileId()];

            if ((packet.PayLoad.Flags & QUERY_DIRECTORY_Request_Flags_Values.REOPEN)
                == QUERY_DIRECTORY_Request_Flags_Values.REOPEN)
            {
                open.enumerationLocation = 0;
                open.enumerationSearchPattern = string.Empty;
            }

            if ((packet.PayLoad.Flags & QUERY_DIRECTORY_Request_Flags_Values.RESTART_SCANS)
                == QUERY_DIRECTORY_Request_Flags_Values.RESTART_SCANS)
            {
                open.enumerationLocation = 0;
            }

            if (open.enumerationLocation == 0 && string.IsNullOrEmpty(open.enumerationSearchPattern))
            {
                open.enumerationSearchPattern = fileName;
            }

            if ((packet.PayLoad.Flags & QUERY_DIRECTORY_Request_Flags_Values.INDEX_SPECIFIED)
                == QUERY_DIRECTORY_Request_Flags_Values.INDEX_SPECIFIED)
            {
                open.enumerationLocation = (int)packet.PayLoad.FileIndex;

                if (string.IsNullOrEmpty(fileName))
                {
                    open.enumerationSearchPattern = fileName;
                }
            }
        }


        /// <summary>
        /// Handle event of receiving packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandlePacketSentEvent(smb3Event smb3Event)
        {
            smb3CompoundPacket compoundPacket = smb3Event.Packet as smb3CompoundPacket;

            if (compoundPacket != null)
            {
                foreach (smb3Packet innerPacket in compoundPacket.Packets)
                {
                    smb3Event compoundPacketEvent = new smb3Event();

                    compoundPacketEvent.ConnectionId = smb3Event.ConnectionId;
                    compoundPacketEvent.Packet = innerPacket;
                    compoundPacketEvent.Type = smb3Event.Type;

                    HandlePacketSentEvent(compoundPacketEvent);
                }
            }
            else
            {
                smb3SinglePacket singlePacket = smb3Event.Packet as smb3SinglePacket;

                GrandCredit(singlePacket, smb3Event.ConnectionId);

                if ((singlePacket.Header.Flags & Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND)
                    == Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND)
                {
                    HandleSendFinnalAsyncResponseEvent(smb3Event);
                }

                smb3ErrorResponsePacket errorResponse = singlePacket as smb3ErrorResponsePacket;

                if (errorResponse != null)
                {
                    HandleSendErrorResponseEvent(smb3Event);
                    return;
                }

                switch (singlePacket.Header.Command)
                {
                    case smb3Command.NEGOTIATE:
                        HandleSendNegotiateResponseEvent(smb3Event);
                        break;
                    case smb3Command.SESSION_SETUP:
                        HandleSendSessionSetupResponseEvent(smb3Event);
                        break;
                    case smb3Command.LOGOFF:
                        HandleSendLogOffResponseEvent(smb3Event);
                        break;
                    case smb3Command.TREE_CONNECT:
                        HandleSendTreeConnectResponseEvent(smb3Event);
                        break;
                    case smb3Command.TREE_DISCONNECT:
                        HandleSendTreeDisconnectResponseEvent(smb3Event);
                        break;
                    case smb3Command.CREATE:
                        HandleSendCreateResponseEvent(smb3Event);
                        break;
                    case smb3Command.CLOSE:
                        HandleSendCloseResponseEvent(smb3Event);
                        break;
                    case smb3Command.OPLOCK_BREAK:
                        HandleSendOplockBreakResponseEvent(smb3Event);
                        break;
                    case smb3Command.LOCK:
                        HandleSendLockResponseEvent(smb3Event);
                        break;
                    case smb3Command.IOCTL:
                        HandleSendIOCtlResponseEvent(smb3Event);
                        break;
                    default:
                        break;
                }
            }
        }


        /// <summary>
        /// Handle sending error response event
        /// </summary>
        /// <param name="smb3Event"></param>
        private void HandleSendErrorResponseEvent(smb3Event smb3Event)
        {
            smb3ErrorResponsePacket packet = smb3Event.Packet as smb3ErrorResponsePacket;

            // It is an interim response packet
            if (packet.Header.Status == (uint)smb3Status.STATUS_PENDING)
            {
                ulong asyncId = smb3Utility.AssembleToAsyncId(packet.Header.ProcessId, packet.Header.TreeId);
                AsyncCommand asyncCommand = new AsyncCommand();
                asyncCommand.asyncId = asyncId;
                asyncCommand.requestPacket = smb3Event.Packet;

                connectionList[smb3Event.ConnectionId].asyncCommandList.Add(asyncId, asyncCommand);
            }
        }


        /// <summary>
        /// Handle sending finnal async response event
        /// </summary>
        /// <param name="smb3Event"></param>
        private void HandleSendFinnalAsyncResponseEvent(smb3Event smb3Event)
        {
            smb3SinglePacket packet = smb3Event.Packet as smb3SinglePacket;

            ulong asyncId = smb3Utility.AssembleToAsyncId(packet.Header.ProcessId, packet.Header.TreeId);

            connectionList[smb3Event.ConnectionId].asyncCommandList.Remove(asyncId);
        }


        /// <summary>
        /// Grand credit to client
        /// </summary>
        /// <param name="packet">The response packet</param>
        /// <param name="connectionId">Used to find the connection</param>
        private void GrandCredit(smb3SinglePacket packet, int connectionId)
        {
            connectionList[connectionId].GrandCredit(packet.Header.CreditRequest_47_Response);
        }


        /// <summary>
        /// Handle event of sending negotiate response packet based on receiving a smb3 negotiate request
        /// packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleSendNegotiateResponseEvent(smb3Event smb3Event)
        {
            smb3Packet packet = FindRequestPacket(smb3Event.ConnectionId, 0);

            if (packet is SmbNegotiateRequestPacket)
            {
                HandleSendsmb3NegotiateResponsev1Event(smb3Event);
            }
            else
            {
                HandleSendsmb3NegotiateResponsev2Event(smb3Event);
            }
        }


        /// <summary>
        /// Handle event of sending smb3 negotiate response packet based on receiving smb negotiate request packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleSendsmb3NegotiateResponsev1Event(smb3Event smb3Event)
        {
            smb3NegotiateResponsePacket packet = smb3Event.Packet as smb3NegotiateResponsePacket;
            int connectionId = smb3Event.ConnectionId;

            if (connectionList.ContainsKey(connectionId) && connectionList[connectionId].negotiateDialect != 0xffff)
            {
                //The protocol version has been negotiated, the event is not valid, but sdk can't throw exception for
                //this situation, because maybe user means to do that.
                return;
            }

            if (packet.PayLoad.DialectRevision == DialectRevision_Values.V1)
            {
                connectionList[connectionId].negotiateDialect = smb3Consts.NegotiateDialect2_02;
                connectionList[connectionId].dialect = smb3Consts.NegotiateDialect2_02String;
            }
            else if (packet.PayLoad.DialectRevision == DialectRevision_Values.V3)
            {
                connectionList[connectionId].negotiateDialect = smb3Consts.NegotiateDialect2_XX;
            }
            else
            {
                //do nothing to invalid dialectRevision
            }
        }


        /// <summary>
        /// Handle event of sending smb3 negotiate response packet based on receiving a smb3 negotiate
        /// request packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleSendsmb3NegotiateResponsev2Event(smb3Event smb3Event)
        {
            smb3NegotiateResponsePacket packet = smb3Event.Packet as smb3NegotiateResponsePacket;
            int connectionId = smb3Event.ConnectionId;

            if (connectionList.ContainsKey(connectionId) && (connectionList[connectionId].negotiateDialect == 0x0202 ||
                connectionList[connectionId].negotiateDialect == 0x0210))
            {
                // if the negotiate is complete before, td says this connection MUST disconnect, but this is not sdk's duty to 
                // disconnect it, user must do that. so here we just ignore it.
                return;
            }

            if (packet.PayLoad.DialectRevision == DialectRevision_Values.V1)
            {
                connectionList[connectionId].negotiateDialect = smb3Consts.NegotiateDialect2_02;
                connectionList[connectionId].dialect = smb3Consts.NegotiateDialect2_02String;
            }
            else if (packet.PayLoad.DialectRevision == DialectRevision_Values.V2)
            {
                connectionList[connectionId].negotiateDialect = smb3Consts.NegotiateDialect2_10;
                connectionList[connectionId].dialect = smb3Consts.NegotiateDialect2_10String;
            }
            else
            {
                //the other value is not correct, but for the negtive test, we ignore it here.
            }
        }


        /// <summary>
        /// Handle the event of sending sesssion setup response packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleSendSessionSetupResponseEvent(smb3Event smb3Event)
        {
            smb3SessionSetupResponsePacket packet = smb3Event.Packet as smb3SessionSetupResponsePacket;

            smb3SessionSetupRequestPacket requestPacket = FindRequestPacket(smb3Event.ConnectionId, packet.Header.MessageId)
                as smb3SessionSetupRequestPacket;

            if (requestPacket.PayLoad.PreviousSessionId != 0)
            {
                HandleReAuthenticateEvent(packet, smb3Event.ConnectionId);
            }
            else
            {
                HandleNewAuthenticateEvent(packet, smb3Event.ConnectionId);
            }
        }


        /// <summary>
        /// Handle the event of sending log off response packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleSendLogOffResponseEvent(smb3Event smb3Event)
        {
            smb3LogOffResponsePacket packet = smb3Event.Packet as smb3LogOffResponsePacket;

            globalSessionTable.Remove(packet.GetSessionId());
        }


        /// <summary>
        /// Handle the event of sending treeConnect response packet
        /// </summary>
        /// <param name="smb3Event">Contain event information</param>
        private void HandleSendTreeConnectResponseEvent(smb3Event smb3Event)
        {
            smb3TreeConnectResponsePacket packet = smb3Event.Packet as smb3TreeConnectResponsePacket;
            smb3ServerTreeConnect treeConnect = new smb3ServerTreeConnect();

            treeConnect.treeId = packet.GetTreeId();
            globalSessionTable[packet.GetSessionId()].treeConnectTable.Add(treeConnect.treeId, treeConnect);
        }


        /// <summary>
        /// Handle the event of sending tree disconnect response packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleSendTreeDisconnectResponseEvent(smb3Event smb3Event)
        {
            smb3TreeDisconnectResponsePacket packet = smb3Event.Packet as smb3TreeDisconnectResponsePacket;

            globalSessionTable[packet.GetSessionId()].treeConnectTable.Remove(packet.GetTreeId());
        }


        /// <summary>
        /// Handle the event of authenticate
        /// </summary>
        /// <param name="packet">The session setup response packet</param>
        /// <param name="connectionId">Used to find the connection</param>
        private void HandleNewAuthenticateEvent(smb3SessionSetupResponsePacket packet, int connectionId)
        {
            if (!globalSessionTable.ContainsKey(packet.GetSessionId()))
            {
                smb3ServerSession session = new smb3ServerSession();

                session.connection = connectionList[connectionId];
                session.state = SessionState.InProgress;
                session.securityContext = null;
                session.sessionId = packet.GetSessionId();
                session.openTable = new Dictionary<FILEID, smb3ServerOpen>();
                session.treeConnectTable = new Dictionary<uint, smb3ServerTreeConnect>();

                globalSessionTable.Add(session.sessionId, session);
            }

            smb3SessionSetupRequestPacket requestPacket = FindRequestPacket(connectionId, packet.Header.MessageId)
                as smb3SessionSetupRequestPacket;

            if (packet.Header.Status == 0)
            {
                if (connectionList[connectionId].clientCapabilities == 0)
                {
                    connectionList[connectionId].clientCapabilities = requestPacket.PayLoad.Capabilities;
                }

                if (((packet.PayLoad.SessionFlags & SessionFlags_Values.SESSION_FLAG_IS_GUEST) == SessionFlags_Values.SESSION_FLAG_IS_GUEST)
                    || ((packet.PayLoad.SessionFlags & SessionFlags_Values.SESSION_FLAG_IS_NULL) == SessionFlags_Values.SESSION_FLAG_IS_NULL))
                {
                    //should sign set to false. do not need to set it manually.
                }
                else
                {
                    if (((requestPacket.PayLoad.SecurityMode & SESSION_SETUP_Request_SecurityMode_Values.NEGOTIATE_SIGNING_REQUIRED)
                        == SESSION_SETUP_Request_SecurityMode_Values.NEGOTIATE_SIGNING_REQUIRED) && (this.requireMessageSigning ||
                        connectionList[connectionId].shouldSign))
                    {
                        globalSessionTable[packet.GetSessionId()].shouldSign = true;
                    }
                }

                globalSessionTable[packet.GetSessionId()].sessionKey = connectionList[connectionId].gss.SessionKey;
                globalSessionTable[packet.GetSessionId()].state = SessionState.Valid;
                //Set it to null because if another authentiate request arrives, gss must be
                //set to a new one. set to null as a flag to indicate gss must re-construct.

                //release gss, set to null
                connectionList[connectionId].ReleaseSspiServer();
            }
        }


        /// <summary>
        /// Handle re-authenticate event
        /// </summary>
        /// <param name="packet">The session setup response packet</param>
        /// <param name="connectionId">Used to find the connection</param>
        private void HandleReAuthenticateEvent(smb3SessionSetupResponsePacket packet, int connectionId)
        {
            if (packet.Header.Status == 0)
            {
                globalSessionTable[packet.GetSessionId()].state = SessionState.Valid;
            }
        }


        /// <summary>
        /// handle the event of sending back createResponse packet to client
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleSendCreateResponseEvent(smb3Event smb3Event)
        {
            smb3CreateResponsePacket packet = smb3Event.Packet as smb3CreateResponsePacket;
            smb3CreateRequestPacket requestPacket = FindRequestPacket(smb3Event.ConnectionId, packet.Header.MessageId)
                as smb3CreateRequestPacket;

            #region Handle SMB2_CREATE_DURABLE_HANDLE_RECONNECT create context

            CREATE_CONTEXT_Values[] responseCreateContexts = packet.GetCreateContexts();
            CREATE_CONTEXT_Values[] requestCreateContexts = requestPacket.GetCreateContexts();

            if (requestCreateContexts != null)
            {
                foreach (CREATE_CONTEXT_Values createContext in requestCreateContexts)
                {
                    CreateContextTypeValue createContextType = smb3Utility.GetContextType(createContext);

                    if (createContextType == CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_RECONNECT)
                    {
                        smb3ServerOpen existOpen = globalOpenTable[packet.PayLoad.FileId.Persistent];
                        existOpen.connection = connectionList[smb3Event.ConnectionId];
                        existOpen.fileId = packet.PayLoad.FileId.Volatile;
                        globalSessionTable[packet.GetSessionId()].openTable.Add(packet.PayLoad.FileId, existOpen);

                        //The "Successful Open Initialization" and "Oplock Acquisition" phases MUST be skipped
                        return;
                    }
                }
            }

            #endregion

            #region Successful Open Initialization phase

            smb3ServerOpen open = new smb3ServerOpen();

            open.fileId = packet.PayLoad.FileId.Volatile;
            open.durableFileId = packet.PayLoad.FileId.Persistent;
            open.session = globalSessionTable[packet.GetSessionId()];
            open.connection = connectionList[smb3Event.ConnectionId];

            //we do not open the underlying object store actually, so just set the open handle to 0. 
            open.localOpen = 0;

            //It MUST be equal to the DesiredAccess specified in the request, 
            //except in the case where MAXIMUM_ALLOWED is included in the DesiredAccess
            //BECAUSE we do not implement underlying object store, we do not know the finnal grantedAccess,
            //so assuming it equals to request access
            open.grantedAccess = requestPacket.PayLoad.DesiredAccess.ACCESS_MASK;

            open.oplockLevel = OplockLevel_Values.OPLOCK_LEVEL_NONE;
            open.oplockState = OplockState.None;
            open.oplockTimeout = new TimeSpan(0, 0, 0);
            open.isDurable = false;
            open.durableOpenTimeout = new TimeSpan(0, 0, 0);
            open.durableOwner = null;
            open.enumerationLocation = 0;
            open.enumerationSearchPattern = null;

            //Open.CurrentEaIndex is set to 1.
            open.currentEaIndex = 1;
            //Open.CurrentQuotaIndex is set to 1.
            open.currentQuotaIndex = 1;
            open.treeConnect = globalSessionTable[packet.GetSessionId()].treeConnectTable[packet.GetTreeId()];
            open.treeConnect.openCount++;
            open.lockCount = 0;
            open.pathName = requestPacket.RetreivePathName();
            open.lockSequenceArray = new byte[smb3Consts.LockSequenceCountInServerOpen];

            globalOpenTable.Add(packet.PayLoad.FileId.Persistent, open);
            globalSessionTable[packet.GetSessionId()].openTable.Add(packet.PayLoad.FileId, open);

            #endregion

            #region Oplock Acquisition phase

            globalSessionTable[packet.GetSessionId()].openTable[packet.PayLoad.FileId].oplockLevel = packet.PayLoad.OplockLevel;

            #endregion

            #region SMB2_CREATE_REQUEST_LEASE Create Context

            if (responseCreateContexts == null)
            {
                return;
            }

            foreach (CREATE_CONTEXT_Values createContext in responseCreateContexts)
            {
                CreateContextTypeValue createContextType = smb3Utility.GetContextType(createContext);

                if (createContextType == CreateContextTypeValue.SMB2_CREATE_REQUEST_LEASE)
                {
                    if (connectionList[smb3Event.ConnectionId].dialect != smb3Consts.NegotiateDialect2_10String)
                    {
                        //In case sdk user do not do the thing described in td
                        return;
                    }

                    if (!globalLeaseTableList.ContainsKey(connectionList[smb3Event.ConnectionId].clientGuid))
                    {
                        smb3LeaseTable leaseTable = new smb3LeaseTable();

                        leaseTable.clientGuid = connectionList[smb3Event.ConnectionId].clientGuid;
                        leaseTable.leaseList = new Dictionary<Guid,smb3Lease>();

                        globalLeaseTableList.Add(leaseTable.ClientGuid, leaseTable);
                    }

                    byte[] leaseContextBuffer = smb3Utility.GetDataFieldInCreateContext(createContext);
                    CREATE_RESPONSE_LEASE leaseContext = smb3Utility.ToStructure<CREATE_RESPONSE_LEASE>(leaseContextBuffer);
                    Guid leaseKey = new Guid(leaseContext.LeaseKey);

                    if (!globalLeaseTableList[connectionList[smb3Event.ConnectionId].clientGuid].LeaseList.ContainsKey(leaseKey))
                    {
                        smb3Lease lease = new smb3Lease();
                        lease.leaseKey = leaseKey;
                        lease.fileName = requestPacket.RetreivePathName();
                        lease.leaseBreakTimeout = new TimeSpan(0, 0, 0);
                        lease.leaseOpens = new Dictionary<FILEID, smb3ServerOpen>();
                        lease.leaseState = LeaseStateValues.SMB2_LEASE_NONE;
                        lease.breaking = false;

                        globalLeaseTableList[connectionList[smb3Event.ConnectionId].clientGuid].leaseList.Add(leaseKey, lease);
                    }

                    globalLeaseTableList[connectionList[smb3Event.ConnectionId].clientGuid].LeaseList[leaseKey].leaseState =
                        leaseContext.LeaseState;
                    globalLeaseTableList[connectionList[smb3Event.ConnectionId].clientGuid].LeaseList[leaseKey].leaseOpens.Add(
                        packet.PayLoad.FileId, open);

                    globalSessionTable[packet.GetSessionId()].openTable[packet.PayLoad.FileId].oplockLevel = OplockLevel_Values.SMB2_OPLOCK_LEVEL_LEASE;
                    globalSessionTable[packet.GetSessionId()].openTable[packet.PayLoad.FileId].lease =
                        globalLeaseTableList[connectionList[smb3Event.ConnectionId].clientGuid];
                }
                else if (createContextType == CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_REQUEST)
                {
                    globalSessionTable[packet.GetSessionId()].openTable[packet.PayLoad.FileId].isDurable = true;
                }
            }

            #endregion
        }


        /// <summary>
        /// Handle the event of sending close response
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleSendCloseResponseEvent(smb3Event smb3Event)
        {
            smb3CloseResponsePacket packet = smb3Event.Packet as smb3CloseResponsePacket;
            smb3CloseRequestPacket requestPacket = FindRequestPacket(smb3Event.ConnectionId, packet.Header.MessageId)
                as smb3CloseRequestPacket;

            FILEID fileId = requestPacket.GetFileId();

            if (fileId.Persistent == ulong.MaxValue && fileId.Volatile == ulong.MaxValue)
            {
                fileId = smb3Utility.ResolveFileIdInCompoundResponse(fileId, packet);
            }

            smb3ServerOpen open = globalSessionTable[packet.GetSessionId()].openTable[fileId];
            globalSessionTable[packet.GetSessionId()].openTable.Remove(fileId);
            globalOpenTable.Remove(fileId.Persistent);
            open.treeConnect.openCount--;

            Guid foundLeaseGuid = new Guid();

            if (open.lease != null)
            {
                foreach (KeyValuePair<Guid, smb3Lease> lease in open.lease.leaseList)
                {
                    foreach (FILEID id in lease.Value.leaseOpens.Keys)
                    {
                        if ((id.Volatile == fileId.Volatile) && (id.Persistent == fileId.Persistent))
                        {
                            foundLeaseGuid = lease.Key;
                            break;
                        }
                    }
                }
            }

            if (open.lease != null)
            {
                open.lease.leaseList[foundLeaseGuid].leaseOpens.Remove(fileId);

                if (open.lease.leaseList[foundLeaseGuid].leaseOpens.Count == 0)
                {
                    if (open.lease.leaseList[foundLeaseGuid].breaking)
                    {
                        open.lease.leaseList[foundLeaseGuid].leaseState = LeaseStateValues.SMB2_LEASE_NONE;
                    }
                }
            }
        }


        /// <summary>
        /// Handle the event of sending OplockBreak notification or response packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleSendOplockBreakResponseEvent(smb3Event smb3Event)
        {
            smb3SinglePacket singlePacket = smb3Event.Packet as smb3SinglePacket;

            if (singlePacket.Header.MessageId == ulong.MaxValue)
            {
                smb3OpLockBreakNotificationPacket oplockNotification = singlePacket as smb3OpLockBreakNotificationPacket;

                if (oplockNotification != null)
                {
                    //oplock notification
                    globalOpenTable[oplockNotification.PayLoad.FileId.Persistent].oplockLevel = (OplockLevel_Values)oplockNotification.PayLoad.OplockLevel;
                    globalOpenTable[oplockNotification.PayLoad.FileId.Persistent].oplockState = OplockState.Breaking;
                }
                else
                {
                    if (connectionList[smb3Event.ConnectionId].dialect == smb3Consts.NegotiateDialect2_10String)
                    {
                        //lease break notification
                        smb3LeaseBreakNotificationPacket leaseBreakNotification = singlePacket as smb3LeaseBreakNotificationPacket;

                        Guid clientGuid = connectionList[smb3Event.ConnectionId].clientGuid;
                        Guid leaseKey = new Guid(leaseBreakNotification.PayLoad.LeaseKey);

                        smb3Lease lease = globalLeaseTableList[clientGuid].leaseList[leaseKey];
                        lease.breaking = true;
                        lease.breakToLeaseState = leaseBreakNotification.PayLoad.NewLeaseState;

                        foreach (smb3ServerOpen open in lease.leaseOpens.Values)
                        {
                            open.oplockState = OplockState.Breaking;
                        }
                    }
                }
            }
            else
            {
                smb3OpLockBreakResponsePacket oplockResponse = singlePacket as smb3OpLockBreakResponsePacket;

                if (oplockResponse != null)
                {
                    //oplock response
                    smb3ServerOpen open = globalSessionTable[oplockResponse.GetSessionId()].openTable[oplockResponse.GetFileId()];

                    if (oplockResponse.PayLoad.OplockLevel == OPLOCK_BREAK_Response_OplockLevel_Values.OPLOCK_LEVEL_II)
                    {
                        open.oplockLevel = OplockLevel_Values.OPLOCK_LEVEL_II;
                        open.oplockState = OplockState.Held;
                    }
                    else if (oplockResponse.PayLoad.OplockLevel == OPLOCK_BREAK_Response_OplockLevel_Values.OPLOCK_LEVEL_NONE)
                    {
                        open.oplockLevel = OplockLevel_Values.OPLOCK_LEVEL_NONE;
                        open.oplockState = OplockState.None;
                    }
                    else
                    {
                        //invalid oplock level, but we do nothing here because maybe it is a nagtive test.
                    }
                }
                else
                {
                    if (connectionList[smb3Event.ConnectionId].dialect == smb3Consts.NegotiateDialect2_10String)
                    {
                        //lease break response
                        smb3LeaseBreakResponsePacket leaseBreakResponse = singlePacket as smb3LeaseBreakResponsePacket;

                        smb3Lease lease = globalLeaseTableList[connectionList[smb3Event.ConnectionId].clientGuid].leaseList[
                            new Guid(leaseBreakResponse.PayLoad.LeaseKey)];

                        lease.leaseState = leaseBreakResponse.PayLoad.LeaseState;
                        lease.breaking = false;
                    }
                }
            }
        }


        /// <summary>
        /// Handle the event of sending lock response packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleSendLockResponseEvent(smb3Event smb3Event)
        {
            smb3LockResponsePacket packet = smb3Event.Packet as smb3LockResponsePacket;

            smb3LockRequestPacket requestPacket = FindRequestPacket(smb3Event.ConnectionId, packet.Header.MessageId)
                as smb3LockRequestPacket;

            smb3ServerOpen open = globalOpenTable[requestPacket.PayLoad.FileId.Persistent];

            bool isUnlock = (requestPacket.PayLoad.Locks[0].Flags & LOCK_ELEMENT_Flags_Values.LOCKFLAG_UNLOCK)
                == LOCK_ELEMENT_Flags_Values.LOCKFLAG_UNLOCK;

            if (open.isResilient && (connectionList[smb3Event.ConnectionId].dialect == smb3Consts.NegotiateDialect2_10String))
            {
                //The LockSequence field of the smb3 lock request MUST be set to ((BucketIndex + 1) << 4) + BucketSequence
                int lockSequenceIndex = ((int)requestPacket.PayLoad.LockSequence >> 4) - 1;

                if (lockSequenceIndex < open.lockSequenceArray.Length)
                {
                    open.lockSequenceArray[lockSequenceIndex] = (byte)(requestPacket.PayLoad.LockSequence & 0xf);
                }
            }

            for (int i = 0; i < requestPacket.PayLoad.Locks.Length; i++)
            {
                if (isUnlock)
                {
                    open.lockCount--;
                }
                else
                {
                    open.lockCount++;
                }
            }
        }


        /// <summary>
        /// Handle the event of sending IOCtl response packet
        /// </summary>
        /// <param name="smb3Event">Contains event information</param>
        private void HandleSendIOCtlResponseEvent(smb3Event smb3Event)
        {
            smb3IOCtlResponsePacket packet = smb3Event.Packet as smb3IOCtlResponsePacket;

            smb3IOCtlRequestPacket requestPacket = FindRequestPacket(smb3Event.ConnectionId, packet.Header.MessageId)
                as smb3IOCtlRequestPacket;

            switch ((CtlCode_Values)packet.PayLoad.CtlCode)
            {
                case CtlCode_Values.FSCTL_LMR_REQUEST_RESILIENCY:
                    smb3ServerOpen open = globalOpenTable[packet.PayLoad.FileId.Persistent];
                    open.isDurable = false;
                    open.isResilient = true;

                    byte[] resiliencyArray = new byte[requestPacket.PayLoad.InputCount];

                    Array.Copy(requestPacket.PayLoad.Buffer, requestPacket.PayLoad.InputOffset - smb3Consts.InputOffsetInIOCtlRequest,
                        resiliencyArray, 0, resiliencyArray.Length);

                    NETWORK_RESILIENCY_Request resiliency = smb3Utility.ToStructure<NETWORK_RESILIENCY_Request>(resiliencyArray);

                    //resiliency is in 1 millisecond, and timespan accept the value in 100 nanosecond
                    open.resilientOpenTimeout = new TimeSpan(10000 * resiliency.Timeout);
                    break;
            }
        }


        /// <summary>
        /// Indicate whether the packet need to be signed
        /// </summary>
        /// <param name="sessionId">The sessionId of the packet</param>
        /// <returns>True indicates the packet need to be signed, otherwise false</returns>
        internal bool ShouldPacketBeSigned(ulong sessionId)
        {
            return globalSessionTable[sessionId].shouldSign;
        }


        /// <summary>
        /// Verify if the messageId in the packet is valid
        /// </summary>
        /// <param name="packet">The received packet</param>
        /// <param name="connectionId">Used to find the connection</param>
        /// <returns>True indicate it is a valid packet, otherwise false</returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private bool VerifyMessageId(smb3Packet packet, int connectionId)
        {
            ulong messageId = 0;

            List<ulong> messageIds = new List<ulong>();
            bool allMessageIdValid = true;

            if (packet is SmbNegotiateRequestPacket)
            {
                messageId = (packet as SmbNegotiateRequestPacket).Header.Mid;
                messageIds.Add(messageId);
            }
            else
            {
                smb3SinglePacket singlePacket = packet as smb3SinglePacket;

                if (singlePacket is smb3CancelRequestPacket)
                {
                    return true;
                }
                else
                {
                    messageId = singlePacket.Header.MessageId;

                    messageIds.Add(messageId);

                    int messageIdIndex = connectionList[connectionId].commandSequenceWindow.IndexOf(messageId);

                    if (messageIdIndex == -1)
                    {
                        return false;
                    }

                    uint maxLen = 0;

                    if (transportType == smb3TransportType.Tcp && connectionList[connectionId].dialect
                        == smb3Consts.NegotiateDialect2_10String && singlePacket.Header.CreditCharge != 0)
                    {
                        switch (singlePacket.Header.Command)
                        {
                            case smb3Command.READ:
                                smb3ReadRequestPacket readRequest = singlePacket as smb3ReadRequestPacket;
                                maxLen = readRequest.PayLoad.Length;
                                break;
                            case smb3Command.WRITE:
                                smb3WriteRequestPacket writeRequet = singlePacket as smb3WriteRequestPacket;
                                maxLen = writeRequet.PayLoad.Length;
                                break;
                            case smb3Command.CHANGE_NOTIFY:
                                smb3ChangeNotifyRequestPacket changeNotifyRequest = singlePacket as smb3ChangeNotifyRequestPacket;
                                maxLen = changeNotifyRequest.PayLoad.OutputBufferLength;
                                break;
                            case smb3Command.QUERY_DIRECTORY:
                                smb3QueryDirectoryRequestPacket queryDirectory = singlePacket as smb3QueryDirectoryRequestPacket;
                                maxLen = queryDirectory.PayLoad.OutputBufferLength;
                                break;
                        }

                        //CreditCharge >= (max(SendPayloadSize, Expected ResponsePayloadSize) – 1)/ 65536 + 1
                        int expectedCreditCharge = 1 + ((int)maxLen - 1) / 65536;

                        if (expectedCreditCharge > singlePacket.Header.CreditCharge)
                        {
                            throw new InvalidOperationException(string.Format("The CreditCharge in header is not valid. The expected value is {0}, "
                                + "and the actual value is {1}", expectedCreditCharge, singlePacket.Header.CreditCharge));
                        }

                        for (int i = 1; i < singlePacket.Header.CreditCharge; i++)
                        {
                            if ((messageIdIndex + i) < connectionList[connectionId].commandSequenceWindow.Count)
                            {
                                messageIds.Add(connectionList[connectionId].commandSequenceWindow[messageIdIndex + i]);
                            }
                            else
                            {
                                allMessageIdValid = false;
                                break;
                            }
                        }
                    }
                }
            }

            foreach (ulong item in messageIds)
            {
                if (connectionList[connectionId].commandSequenceWindow.Contains(item))
                {
                    connectionList[connectionId].RemoveMessageId(item);
                }
                else
                {
                    allMessageIdValid = false;
                }
            }

            return allMessageIdValid;
        }


        /// <summary>
        /// Set the sessionKey field of the packet
        /// </summary>
        /// <param name="connectionId">Used to find the connection</param>
        /// <param name="packet">The packet</param>
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

                singlePacket.SessionKey = globalSessionTable[singlePacket.GetSessionId()].sessionKey;
            }
            else
            {
                //it is smb negotiate packet, do not need verify signature.
            }
        }


        /// <summary>
        /// Find request packet based on the connectionId and the messageId
        /// </summary>
        /// <param name="connectionId">Used to find the connection</param>
        /// <param name="messageId">Used to find the message</param>
        /// <returns>The founded message</returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        internal smb3Packet FindRequestPacket(int connectionId, ulong messageId)
        {
            return connectionList[connectionId].requestList[messageId];
        }


        /// <summary>
        /// Handle disconnected event
        /// </summary>
        /// <param name="smb3Event">contain the update information</param>
        private void HandleDisconnectedEvent(smb3Event smb3Event)
        {
            List<ulong> sessionIds = new List<ulong>();

            foreach (KeyValuePair<ulong, smb3ServerSession> sessionItem in globalSessionTable)
            {
                if (sessionItem.Value.connection.connectionId == smb3Event.ConnectionId)
                {
                    sessionIds.Add(sessionItem.Key);

                    sessionItem.Value.treeConnectTable.Clear();

                    foreach (KeyValuePair<FILEID, smb3ServerOpen> openItem in sessionItem.Value.openTable)
                    {
                        if (openItem.Value.isResilient || (openItem.Value.oplockLevel == OplockLevel_Values.OPLOCK_LEVEL_BATCH &&
                            openItem.Value.oplockState == OplockState.Held && openItem.Value.isDurable))
                        {
                            openItem.Value.connection = null;
                            openItem.Value.treeConnect = null;
                            openItem.Value.session = null;
                        }
                        else
                        {
                            globalOpenTable.Remove(openItem.Key.Persistent);
                        }
                    }
                }
            }

            foreach (ulong sessionId in sessionIds)
            {
                globalSessionTable.Remove(sessionId);
            }

            connectionList[smb3Event.ConnectionId].Dispose();
            connectionList.Remove(smb3Event.ConnectionId);
        }

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
                    if (connectionList != null)
                    {
                        foreach (smb3ServerConnection connection in connectionList.Values)
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
        ~smb3ServerContext()
        {
            Dispose(false);
        }

        #endregion
    }
}

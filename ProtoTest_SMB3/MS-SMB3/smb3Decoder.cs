//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3Decoder
// Description: smb3Decoder is used to decode smb3 packet
//-------------------------------------------------------------------------

using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Protocols.TestTools.StackSdk.Messages;
using Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// smb3Decoder is used to decode smb3 packet
    /// </summary>
    internal class smb3Decoder
    {
        //the decode role tell the decoder the packet should be decoded as request packet or response packet
        private smb3Role decodeRole;
        //the payload of Tcp transport will have extra 4 bytes in front of the message to indicate the packet length
        private smb3TransportType transportType;
        //the global context contains information which will be used to decode
        internal smb3ClientGlobalContext globalContext;
        //the connectionId of smb3client which uses this decoder
        internal int connectionId;

        //The share name of named pipe
        private const string NamedPipeShareName = "IPC$";

        /// <summary>
        /// The underlying transport type
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
        /// the decode role tell the decoder the packet should be decoded as request packet or response packet
        /// </summary>
        public smb3Role DecodeRole
        {
            get
            {
                return decodeRole;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="decodeRole">The decode role, client or server</param>
        public smb3Decoder(smb3Role decodeRole)
        {
            this.decodeRole = decodeRole;
        }


        /// <summary>
        /// This function is called by transport stack as a callback when the transport receive any message
        /// </summary>
        /// <param name="endPoint">Where the packet is received</param>
        /// <param name="messageBytes">The received packet</param>
        /// <param name="consumedLength">[OUT]The consumed length of the message</param>
        /// <param name="expectedLength">[OUT]The expected length</param>
        /// <returns>A array of stackpacket</returns>
        public StackPacket[] smb3DecodePacketCallback(
            object endPoint, 
            byte[] messageBytes,
            out int consumedLength,
            out int expectedLength)
        {
            if (messageBytes.Length == 0)
            {
                consumedLength = 0;
                expectedLength = 0;
                return null;
            }

            smb3Packet packet = DecodeTransportPayload(
                messageBytes,
                decodeRole,
                transportType,
                false, 
                out consumedLength,
                out expectedLength);

            if (packet == null)
            {
                return null;
            }
            else
            {
                return new StackPacket[] { packet };
            }
        }


        /// <summary>
        /// Decode the payload which transport received.
        /// </summary>
        /// <param name="messageBytes">The received packet</param>
        /// <param name="role">The role of this decoder, client or server</param>
        /// <param name="transportType">The underlying transport type</param>
        /// <param name="ignoreCompoundFlag">indicate whether decode the packet as a single packet or a compound packet
        /// when compound flag is set</param>
        /// <param name="consumedLength">[OUT]The consumed length of the message</param>
        /// <param name="expectedLength">[OUT]The expected length</param>
        /// <returns>A smb3Packet</returns>
        [SuppressMessage(
            "Microsoft.Maintainability", 
            "CA1500:VariableNamesShouldNotMatchFieldNames",
            MessageId = "transportType")]
        public smb3Packet DecodeTransportPayload(
            byte[] messageBytes, 
            smb3Role role,
            smb3TransportType transportType,
            bool ignoreCompoundFlag,
            out int consumedLength, 
            out int expectedLength
            )
        {
            //tcp transport will prefix 4 bytes length in the beginning. and netbios won't do this.
            if (transportType == smb3TransportType.Tcp)
            {
                if (messageBytes.Length < smb3Consts.TcpPrefixedLenByteCount)
                {
                    consumedLength = 0;
                    expectedLength = 4;
                    return null;
                }

                //in the header of tcp payload, there are 4 bytes(in fact only 3 bytes are used) which indicate
                //the length of smb3
                int dataLenShouldHave = (messageBytes[1] << 16) + (messageBytes[2] << 8) + messageBytes[3];

                if (dataLenShouldHave > (messageBytes.Length - smb3Consts.TcpPrefixedLenByteCount))
                {
                    consumedLength = 0;
                    expectedLength = smb3Consts.TcpPrefixedLenByteCount + dataLenShouldHave;
                    return null;
                }

                byte[] smb3Message = new byte[messageBytes.Length - smb3Consts.TcpPrefixedLenByteCount];

                Array.Copy(messageBytes, smb3Consts.TcpPrefixedLenByteCount, smb3Message, 0, smb3Message.Length);

                smb3Packet packet = DecodeCompletePacket(
                    smb3Message,
                    role, 
                    ignoreCompoundFlag,
                    0, 
                    0, 
                    out consumedLength, 
                    out expectedLength);

                // Here we ignore the consumedLength returned by DecodeCompletePacket(), there may be some tcp padding data 
                // at the end which we are not interested.
                consumedLength = dataLenShouldHave + smb3Consts.TcpPrefixedLenByteCount;

                return packet;
            }
            else
            {
                smb3Packet packet = DecodeCompletePacket(
                    messageBytes,
                    role,
                    ignoreCompoundFlag,
                    0,
                    0, 
                    out consumedLength, 
                    out expectedLength);

                //Some packet has unknown padding data at the end.
                consumedLength = messageBytes.Length;

                return packet;
            }
        }


        /// <summary>
        /// Decode the the message except length field which may exist if transport is tcp
        /// </summary>
        /// <param name="messageBytes">The received packet</param>
        /// <param name="role">The role of this decoder, client or server</param>
        /// <param name="ignoreCompoundFlag">indicate whether decode the packet as a single packet or a compound packet
        /// when compound flag is set</param>
        /// <param name="realSessionId">The real sessionId for this packet</param>
        /// <param name="realTreeId">The real treeId for this packet</param>
        /// <param name="consumedLength">[OUT]The consumed length of the message</param>
        /// <param name="expectedLength">[OUT]The expected length</param>
        /// <returns>A smb3Packet</returns>
        public smb3Packet DecodeCompletePacket(
            byte[] messageBytes,
            smb3Role role, 
            bool ignoreCompoundFlag,
            ulong realSessionId, 
            uint realTreeId,
            out int consumedLength, 
            out int expectedLength
            )
        {
            //protocol version is of 4 bytes len
            byte[] protocolVersion = new byte[sizeof(uint)];
            Array.Copy(messageBytes, 0, protocolVersion, 0, protocolVersion.Length);

            SmbVersion version = DecodeVersion(protocolVersion);

            if (version == SmbVersion.Version1)
            {
                return DecodeSmbPacket(messageBytes, out consumedLength, out expectedLength);
            }
            else
            {
                return Decodesmb3Packet(
                    messageBytes,
                    role, 
                    ignoreCompoundFlag,
                    realSessionId,
                    realTreeId,
                    out consumedLength,
                    out expectedLength
                    );
            }
        }


        /// <summary>
        /// Decode the packet as smb packet
        /// </summary>
        /// <param name="messageBytes">The received packet</param>
        /// <param name="consumedLen">[OUT]The consumed length of the message</param>
        /// <param name="expectedLen">[OUT]The expected length</param>
        /// <returns>A smb3Packet</returns>
        private static smb3Packet DecodeSmbPacket(byte[] messageBytes, out int consumedLen, out int expectedLen)
        {
            //smb3 only uses smb negotiate packet
            SmbNegotiateRequestPacket packet = new SmbNegotiateRequestPacket();

            packet.FromBytes(messageBytes, out consumedLen, out expectedLen);

            return packet;
        }


        /// <summary>
        /// Decode the message as smb3 packet
        /// </summary>
        /// <param name="messageBytes">The received packet</param>
        /// <param name="role">The role of this decoder, client or server</param>
        /// <param name="ignoreCompoundFlag">indicate whether decode the packet as a single packet or a compound packet
        /// when compound flag is set</param>
        /// <param name="realSessionId">The real sessionId for this packet</param>
        /// <param name="realTreeId">The real treeId for this packet</param>
        /// <param name="consumedLength">[OUT]The consumed length of the message</param>
        /// <param name="expectedLength">[OUT]The expected length</param>
        /// <returns>A smb3Packet</returns>
        private smb3Packet Decodesmb3Packet(
            byte[] messageBytes, 
            smb3Role role,
            bool ignoreCompoundFlag,
            ulong realSessionId,
            uint realTreeId,
            out int consumedLength, 
            out int expectedLength
            )
        {
            Packet_Header smb3Header;

            using (MemoryStream ms = new MemoryStream(messageBytes))
            {
                using (Channel decoderChannel = new Channel(null, ms))
                {
                    smb3Header = decoderChannel.Read<Packet_Header>();
                }

                if ((smb3Header.NextCommand != 0) && !ignoreCompoundFlag)
                {
                    return DecodeCompoundPacket(messageBytes, role, out consumedLength, out expectedLength);
                }
                else
                {
                    return DecodeSinglePacket(
                        messageBytes, 
                        role, 
                        ignoreCompoundFlag,
                        realSessionId,
                        realTreeId,
                        out consumedLength,
                        out expectedLength
                        );
                }
            }

            //return packet;
        }


        /// <summary>
        /// Decode the message as smb3 compound packet
        /// </summary>
        /// <param name="messageBytes">The received packet</param>
        /// <param name="role">The role of this decoder, client or server</param>
        /// <param name="consumedLength">[OUT]The consumed length of the message</param>
        /// <param name="expectedLength">[OUT]The expected length</param>
        /// <returns>A smb3Packet</returns>
        private smb3Packet DecodeCompoundPacket(
            byte[] messageBytes,
            smb3Role role,
            out int consumedLength, 
            out int expectedLength
            )
        {
            smb3CompoundPacket compoundPacket = new smb3CompoundPacket();
            compoundPacket.decoder = this;

            compoundPacket.FromBytes(messageBytes, out consumedLength, out expectedLength);

            return compoundPacket;
        }


        /// <summary>
        /// Decode the message as smb3 single packet
        /// </summary>
        /// <param name="messageBytes">The received packet</param>
        /// <param name="role">The role of this decoder, client or server</param>
        /// <param name="ignoreCompoundFlag">indicate whether decode the packet as a single packet or a compound packet
        /// when compound flag is set</param>
        /// <param name="realSessionId">The real sessionId for this packet</param>
        /// <param name="realTreeId">The real treeId for this packet</param>
        /// <param name="consumedLength">[OUT]The consumed length of the message</param>
        /// <param name="expectedLength">[OUT]The expected length</param>
        /// <returns>A smb3Packet</returns>
        private smb3Packet DecodeSinglePacket(
            byte[] messageBytes,
            smb3Role role, 
            bool ignoreCompoundFlag,
            ulong realSessionId,
            uint realTreeId,
            out int consumedLength,
            out int expectedLength
            )
        {
            if (role == smb3Role.Client)
            {
                return DecodeSingleResponsePacket(
                    messageBytes, 
                    ignoreCompoundFlag,
                    realSessionId,
                    realTreeId,
                    out consumedLength,
                    out expectedLength
                    );
            }
            else if (role == smb3Role.Server)
            {
                return DecodeSingleRequestPacket(messageBytes, out consumedLength, out expectedLength);
            }
            else
            {
                throw new ArgumentException("role should be client or server", "role");
            }
        }


        /// <summary>
        /// Decode the message as smb3 single request packet
        /// </summary>
        /// <param name="messageBytes">The received packet</param>
        /// <param name="consumedLength">[OUT]The consumed length of the message</param>
        /// <param name="expectedLength">[OUT]The expected length</param>
        /// <returns>A smb3Packet</returns>
        private static smb3Packet DecodeSingleRequestPacket(byte[] messageBytes, out int consumedLength, out int expectedLength)
        {
            Packet_Header smb3Header;

            bool isLeaseBreakPacket = false;

            using (MemoryStream ms = new MemoryStream(messageBytes))
            {
                using (Channel decoderChannel = new Channel(null, ms))
                {
                    smb3Header = decoderChannel.Read<Packet_Header>();

                    if (smb3Header.Command == smb3Command.OPLOCK_BREAK)
                    {
                        ushort structureSize = decoderChannel.Read<ushort>();

                        if (structureSize == (ushort)LEASE_BREAK_Acknowledgment_StructureSize_Values.V1)
                        {
                            isLeaseBreakPacket = true;
                        }
                    }
                }
            }

            smb3Packet packet = null;

            switch (smb3Header.Command)
            {
                case smb3Command.CANCEL:
                    packet = new smb3CancelRequestPacket();
                    break;
                case smb3Command.CHANGE_NOTIFY:
                    packet = new smb3ChangeNotifyRequestPacket();
                    break;
                case smb3Command.CLOSE:
                    packet = new smb3CloseRequestPacket();
                    break;
                case smb3Command.CREATE:
                    packet = new smb3CreateRequestPacket();
                    break;
                case smb3Command.ECHO:
                    packet = new smb3EchoRequestPacket();
                    break;
                case smb3Command.FLUSH:
                    packet = new smb3FlushRequestPacket();
                    break;
                case smb3Command.IOCTL:
                    packet = new smb3IOCtlRequestPacket();
                    break;
                case smb3Command.LOCK:
                    packet = new smb3LockRequestPacket();
                    break;
                case smb3Command.LOGOFF:
                    packet = new smb3LogOffRequestPacket();
                    break;
                case smb3Command.NEGOTIATE:
                    packet = new smb3NegotiateRequestPacket();
                    break;
                case smb3Command.OPLOCK_BREAK:
                    if (!isLeaseBreakPacket)
                    {
                        packet = new smb3OpLockBreakAckPacket();
                    }
                    else
                    {
                        packet = new smb3LeaseBreakAckPacket();
                    }
                    break;
                case smb3Command.QUERY_DIRECTORY:
                    packet = new smb3QueryDirectoryRequestPacket();
                    break;
                case smb3Command.QUERY_INFO:
                    packet = new smb3QueryInfoRequestPacket();
                    break;
                case smb3Command.READ:
                    packet = new smb3ReadRequestPacket();
                    break;
                case smb3Command.SESSION_SETUP:
                    packet = new smb3SessionSetupRequestPacket();
                    break;
                case smb3Command.SET_INFO:
                    packet = new smb3SetInfoRequestPacket();
                    break;
                case smb3Command.TREE_CONNECT:
                    packet = new smb3TreeConnectRequestPacket();
                    break;
                case smb3Command.TREE_DISCONNECT:
                    packet = new smb3TreeDisconnectRequestPacket();
                    break;
                case smb3Command.WRITE:
                    packet = new smb3WriteRequestPacket();
                    break;
                default:
                    throw new InvalidOperationException("Received an unknown packet! the type of the packet is "
                        + smb3Header.Command.ToString());
            }

            packet.FromBytes(messageBytes, out consumedLength, out expectedLength);

            return packet;
        }


        /// <summary>
        /// Decode the message as smb3 single response packet
        /// </summary>
        /// <param name="messageBytes">The received packet</param>
        /// <param name="ignoreCompoundFlag">indicate whether decode the packet as a single packet or a compound packet
        /// when compound flag is set</param>
        /// <param name="realSessionId">The real sessionId for this packet</param>
        /// <param name="realTreeId">The real treeId for this packet</param>
        /// <param name="consumedLength">[OUT]The consumed length of the message</param>
        /// <param name="expectedLength">[OUT]The expected length</param>
        /// <returns>A smb3Packet</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private smb3Packet DecodeSingleResponsePacket(
            byte[] messageBytes,
            bool ignoreCompoundFlag,
            ulong realSessionId, 
            uint realTreeId,
            out int consumedLength, 
            out int expectedLength
            )
        {
            Packet_Header smb3Header;

            bool isLeaseBreakPacket = false;

            using (MemoryStream ms = new MemoryStream(messageBytes))
            {
                using (Channel decoderChannel = new Channel(null, ms))
                {
                    smb3Header = decoderChannel.Read<Packet_Header>();

                    if (smb3Header.Command == smb3Command.OPLOCK_BREAK)
                    {
                        ushort structureSize = decoderChannel.Read<ushort>();

                        if (structureSize == (ushort)LEASE_BREAK_Notification_Packet_StructureSize_Values.V1 || 
                            structureSize == (ushort)LEASE_BREAK_Response_StructureSize_Values.V1)
                        {
                            isLeaseBreakPacket = true;
                        }
                    }
                }
            }

            smb3SinglePacket packet = null;
            ushort structSize = BitConverter.ToUInt16(messageBytes, smb3Consts.smb3HeaderLen);

            if (IsErrorPacket(smb3Header, realSessionId, realTreeId, structSize))
            {
                packet = new smb3ErrorResponsePacket();
            }
            else
            {
                switch (smb3Header.Command)
                {
                    case smb3Command.CANCEL:
                        //No Cancel response
                        throw new InvalidOperationException(
                            "Received an unknown packet, the type of the packet is "
                            + smb3Header.Command.ToString());
                    case smb3Command.CHANGE_NOTIFY:
                        packet = new smb3ChangeNotifyResponsePacket();
                        break;
                    case smb3Command.CLOSE:
                        packet = new smb3CloseResponsePacket();
                        break;
                    case smb3Command.CREATE:
                        packet = new smb3CreateResponsePacket();
                        break;
                    case smb3Command.ECHO:
                        packet = new smb3EchoResponsePacket();
                        break;
                    case smb3Command.FLUSH:
                        packet = new smb3FlushResponsePacket();
                        break;
                    case smb3Command.IOCTL:
                        packet = new smb3IOCtlResponsePacket();
                        break;
                    case smb3Command.LOCK:
                        packet = new smb3LockResponsePacket();
                        break;
                    case smb3Command.LOGOFF:
                        packet = new smb3LogOffResponsePacket();
                        break;
                    case smb3Command.NEGOTIATE:
                        packet = new smb3NegotiateResponsePacket();
                        break;
                    case smb3Command.OPLOCK_BREAK:
                        if (smb3Header.MessageId == ulong.MaxValue)
                        {
                            if (!isLeaseBreakPacket)
                            {
                                packet = new smb3OpLockBreakNotificationPacket();
                            }
                            else
                            {
                                packet = new smb3LeaseBreakNotificationPacket();
                            }
                        }
                        else
                        {
                            if (!isLeaseBreakPacket)
                            {
                                packet = new smb3OpLockBreakResponsePacket();
                            }
                            else
                            {
                                packet = new smb3LeaseBreakResponsePacket();
                            }
                        }
                        break;
                    case smb3Command.QUERY_DIRECTORY:
                        packet = new smb3QueryDirectoryResponePacket();
                        break;
                    case smb3Command.QUERY_INFO:
                        packet = new smb3QueryInfoResponsePacket();
                        break;
                    case smb3Command.READ:
                        packet = new smb3ReadResponsePacket();
                        break;
                    case smb3Command.SESSION_SETUP:
                        packet = new smb3SessionSetupResponsePacket();
                        break;
                    case smb3Command.SET_INFO:
                        packet = new smb3SetInfoResponsePacket();
                        break;
                    case smb3Command.TREE_CONNECT:
                        packet = new smb3TreeConnectResponsePacket();
                        break;
                    case smb3Command.TREE_DISCONNECT:
                        packet = new smb3TreeDisconnectResponsePacket();
                        break;
                    case smb3Command.WRITE:
                        packet = new smb3WriteResponsePacket();
                        break;
                    default:
                        throw new InvalidOperationException("Received an unknown packet! the type of the packet is "
                            + smb3Header.Command.ToString());
                }
            }

            packet.FromBytes(messageBytes, out consumedLength, out expectedLength);
            packet.connectionId = connectionId;
            packet.globalContext = globalContext;

            //if ignoreCompoundFlag is false, means the process of decoding this packet
            //is not part of the process of decoding a compound packet. We will update
            //context here.
            if (!ignoreCompoundFlag)
            {
                smb3Event smb3Event = new smb3Event();
                smb3Event.Type = smb3EventType.PacketReceived;
                smb3Event.Packet = packet;
                smb3Event.ConnectionId = smb3Client.connectionId;

                try
                {
                    globalContext.Lock();
                    globalContext.UpdateContext(smb3Event);
                }
                finally
                {
                    globalContext.Unlock();
                }
            }

            return packet;
        }


        /// <summary>
        /// Get version information
        /// </summary>
        /// <param name="message">The received message, the array must be 4 bytes, else it will throw exception</param>
        /// <returns></returns>
        private static SmbVersion DecodeVersion(byte[] message)
        {
            //This field MUST be the 4-byte header (0xFF/0xFE, S, M, B) with the letters represented by their ASCII characters in network byte order
            if (message[1] == 'S' && message[2] == 'M' && message[3] == 'B')
            {
                if (message[0] == 0xFF)
                {
                    return SmbVersion.Version1;
                }
                else if (message[0] == 0xFE)
                {
                    return SmbVersion.Version2;
                }
                else
                {
                    throw new FormatException("The packet is not a valid smb or smb3 packet");
                }
            }
            else
            {
                throw new FormatException("The packet is not a valid smb or smb3 packet");
            }
        }


        /// <summary>
        /// Test if the packet is a smb3ErrorPacket
        /// </summary>
        /// <param name="header">The header of the packet</param>
        /// <returns>True if it is a smb3ErrorPacket, otherwise return false</returns>
        /// <param name="realSessionId">The real sessionId for this packet</param>
        /// <param name="realTreeId">The real treeId for this packet</param>
        /// <param name="structSize">The structSize value of the packet</param>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private bool IsErrorPacket(Packet_Header header, ulong realSessionId, uint realTreeId, ushort structSize)
        {
            ulong sessionId;
            uint treeId;

            if ((header.Flags & Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS)
                == Packet_Header_Flags_Values.FLAGS_RELATED_OPERATIONS)
            {
                sessionId = realSessionId;
                treeId = realTreeId;
            }
            else
            {
                sessionId = header.SessionId;
                treeId = header.TreeId;
            }

            try
            {
                globalContext.Lock();

                smb3OutStandingRequest outstandingRequest = null;
                smb3SinglePacket smb3SinglePacket = null;

                switch ((smb3Status)header.Status)
                {
                    case smb3Status.STATUS_SUCCESS:
                        return false;

                    case smb3Status.STATUS_MORE_PROCESSING_REQUIRED:
                        return (header.Command != smb3Command.SESSION_SETUP) ? true : false;

                    case smb3Status.STATUS_BUFFER_OVERFLOW:
                        switch (header.Command)
                        {
                            case smb3Command.QUERY_INFO:
                                return false;

                            case smb3Command.IOCTL:
                                outstandingRequest =
                                    globalContext.connectionTable[connectionId].outstandingRequests[header.MessageId];
                                smb3SinglePacket = outstandingRequest.request as smb3SinglePacket;
                                smb3IOCtlRequestPacket ioCtlRequest = smb3SinglePacket as smb3IOCtlRequestPacket;

                                if (((ioCtlRequest.PayLoad.Flags & IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL)
                                    == IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL)
                                    && (ioCtlRequest.PayLoad.CtlCode == CtlCode_Values.FSCTL_PIPE_TRANSCEIVE
                                    || ioCtlRequest.PayLoad.CtlCode == CtlCode_Values.FSCTL_PIPE_PEEK
                                    || ioCtlRequest.PayLoad.CtlCode == CtlCode_Values.FSCTL_DFS_GET_REFERRALS))
                                {
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }

                            case smb3Command.READ:
                                string shareName =
                                    globalContext.connectionTable[connectionId].sessionTable[sessionId].treeConnectTable[treeId].shareName;
                                if (shareName == NamedPipeShareName)
                                {
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }

                            default:
                                return true;
                        }

                    case smb3Status.STATUS_NOTIFY_ENUM_DIR:
                    case smb3Status.STATUS_NOTIFY_CLEANUP:
                        return header.Command != smb3Command.CHANGE_NOTIFY;

                    default:
                        if (header.Command == smb3Command.IOCTL)
                        {
                            outstandingRequest =
                                globalContext.connectionTable[connectionId].outstandingRequests[header.MessageId];
                            smb3SinglePacket = outstandingRequest.request as smb3SinglePacket;
                            smb3IOCtlRequestPacket ioCtlRequest = smb3SinglePacket as smb3IOCtlRequestPacket;

                            if (((ioCtlRequest.PayLoad.Flags & IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL)
                                == IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL)
                                && (ioCtlRequest.PayLoad.CtlCode == CtlCode_Values.FSCTL_SRV_COPYCHUNK
                                || ioCtlRequest.PayLoad.CtlCode == CtlCode_Values.FSCTL_SRV_COPYCHUNK_WRITE)
                                && structSize != (ushort)ERROR_Response_packet_StructureSize_Values.V1)
                            {
                                return false;
                            }
                        }
                        return true;
                }
            }
            finally
            {
                globalContext.Unlock();
            }
        }
    }
}

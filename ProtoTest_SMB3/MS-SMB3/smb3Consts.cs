//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3Consts
// Description: smb3Consts contains the const variable used by sdk
//-------------------------------------------------------------------------

using System;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// smb3Consts contains the const variable used by sdk
    /// </summary>
    internal static class smb3Consts
    {
        /// <summary>
        /// Used to seperate smb3Event.ExtraInfo
        /// </summary>
        public const string ExtraInfoSeperator = "^SeP^";

        /// <summary>
        /// The max tcp buffer size used by underlying tcp transport
        /// </summary>
        public const int MaxTcpBufferSize = 1024 * 100;

        /// <summary>
        /// The max netbios buffer size used by underlying netbios transport
        /// </summary>
        public const int MaxNetbiosBufferSize = 1024 * 100;

        /// <summary>
        /// The max names underlying netbios transport can use.
        /// </summary>
        public const int MaxNames = 30;

        /// <summary>
        /// The max sessions the underlying netbios transport can establish.
        /// </summary>
        public const int MaxSessions = 30;

        /// <summary>
        /// The header length defined in td is always 64 bytes
        /// </summary>
        public const int smb3HeaderLen = 64;

        /// <summary>
        /// The max client connection server can accept.
        /// </summary>
        public const int MaxConnectionNumer = 30;

        /// <summary>
        /// The ID of smb3 2.1
        /// </summary>
        public const ushort SMB2_1Dialect = 0x0210;

        /// <summary>
        /// The offset, in bytes, from the beginning of the smb3 header to the security buffer
        /// </summary>
        public const ushort SecurityBufferOffsetInNegotiateResponse = 128;

        /// <summary>
        /// The default credit server will grand is 1;
        /// </summary>
        public const ushort DefaultCreditResponse = 1;

        /// <summary>
        /// The protocol identifier. The value MUST be (in network order) 0xFE, 'S', 'M', and 'B'.
        /// </summary>
        public static readonly byte[] smb3ProtocolId = new byte[] { 0xfe, (byte)'S', (byte)'M', (byte)'B' };

        /// <summary>
        /// The signature size for smb3 is always 16 bytes
        /// </summary>
        public const int SignatureSize = 16;

        /// <summary>
        /// The default CreditCharge in Header
        /// </summary>
        public const ushort DefaultCreditCharge = 0;

        /// <summary>
        /// The default creditrequest in header
        /// </summary>
        public const ushort DefaultCreditRequest = 100;

        /// <summary>
        /// The offset, in bytes, from the beginning of the smb3 header to the security buffer
        /// </summary>
        public const ushort SecurityBufferOffsetInSessionSetup = 72;

        /// <summary>
        /// The securityBufferOffset in negotiate request
        /// </summary>
        public const ushort SecurityBufferOffsetInNegotiateRequest = 88;


        /// <summary>
        /// The protocol version is 2.02
        /// </summary>
        public const uint NegotiateDialect2_02 = 0x0202;

        /// <summary>
        /// The smb3 version 2.002
        /// </summary>
        public const string NegotiateDialect2_02String = "2.002";

        /// <summary>
        /// The protocol version is larger than 2.0
        /// </summary>
        public const uint NegotiateDialect2_XX = 0x02ff;

        /// <summary>
        /// The negotiated smb3 version is 2.100
        /// </summary>
        public const uint NegotiateDialect2_10 = 0x0210;

        /// <summary>
        /// The smb3 version 2.100
        /// </summary>
        public const string NegotiateDialect2_10String = "2.100";

        /// <summary>
        /// The offset, in bytes, of the full share path name from the beginning of the packet header
        /// </summary>
        public const ushort TreeConnectPathOffset = 72;

        /// <summary>
        /// If use Tcp as transport, the message will have 4 byte messageLen information.
        /// </summary>
        public const int TcpPrefixedLenByteCount = 4;

        /// <summary>
        /// The NameOffset value in createRequestPacket if name exist.
        /// </summary>
        public const ushort NameOffsetInCreateRequestPacket = 120;

        /// <summary>
        /// The offset from the beginning of this structure(SMB2_CREATE_CONTEXT Request Values) to its 8-byte aligned name value
        /// </summary>
        public const ushort NameOffsetInCreateContextValues = 16;


        /// <summary>
        /// Open.OperationBuckets[x].Free = TRUE for all x = 0 through 63; 
        /// Open.OperationBuckets[x].SequenceNumber = 0 for all x = 0 through 63.
        /// so the OperationBucketsCount is always 64
        /// </summary>
        public const int OperationBucketsCount = 64;

        /// <summary>
        /// The buffer start index of smb3 CREATE Response start from header
        /// </summary>
        public const int CreateResponseBufferStartIndex = 152;

        /// <summary>
        /// The buffer start index of smb3 CREATE request start from header
        /// </summary>
        public const int CreateRequestBufferStartIndex = 120;

        /// <summary>
        /// The buffer start index in 2.2.13.2 SMB2_CREATE_CONTEXT Request Values
        /// </summary>
        public const int CreateContextBufferStartIndex = 16;

        /// <summary>
        /// The offset, in bytes, from the beginning of the smb3 header to the first
        /// 8-byte aligned SMB2_CREATE_CONTEXT response that is contained in this response.
        /// </summary>
        public const int CreateContextOffsetInCreateResponse = 152;

        /// <summary>
        /// The offset, in bytes, from the beginning of the smb3 header to the data being written in 
        /// smb3 WRITE Request
        /// </summary>
        public const ushort DataOffsetInWriteRequest = 112;

        /// <summary>
        /// The offset, in bytes, from the beginning of the header to the data read being returned in this response
        /// in smb3 READ Response
        /// </summary>
        public const byte DataOffsetInReadResponse = 80;

        /// <summary>
        /// The offset, in bytes, from the beginning of the smb3 header to the input data buffer in smb3 IOCTL Request
        /// </summary>
        public const uint InputOffsetInIOCtlRequest = 120;

        public const uint OutputOffsetInIOCtlRequest = 120;
        /// <summary>
        /// The offset, in bytes, from the beginning of the smb3 header to the input data buffer in smb3 IOCTL Response
        /// </summary>
        public const uint InputOffsetInIOCtlResponse = 112;

        /// <summary>
        /// he offset, in bytes, from the beginning of the smb3 header to the input buffer in smb3 QUERY_INFO Request
        /// </summary>
        public const ushort InputBufferOffsetInQueryInfoRequest = 104;

        /// <summary>
        /// The offset, in bytes, from the beginning of the smb3 header to the information to be set in smb3 SET_INFO Request
        /// </summary>
        public const ushort BufferOffsetInSetInfoRequest = 96;

        /// <summary>
        /// The offset, in bytes, from the beginning of the smb3 header to the information being returned in smb3 QUERY_INFO Response
        /// </summary>
        public const ushort OutputBufferOffsetInQueryInfoResponse = 72;

        /// <summary>
        /// The offset, in bytes, from the beginning of the smb3 header to the search pattern to be used for the enumeration in 
        /// smb3 QUERY_DIRECTORY Request
        /// </summary>
        public const ushort FileNameOffsetInQueryDirectoryRequest = 96;

        /// <summary>
        /// The offset, in bytes, from the beginning of the smb3 header to the directory enumeration data being returned 
        /// in smb3 QUERY_DIRECTORY Response
        /// </summary>
        public const ushort OutputBufferOffsetInQueryDirectoryResponse = 72;

        /// <summary>
        /// The offset, in bytes, from the beginning of the smb3 header to the change information being returned 
        /// in smb3 CHANGE_NOTIFY Response
        /// </summary>
        public const ushort OutputBufferOffsetInChangeNotifyResponse = 72;


        /// <summary>
        /// 12 is the size of SubstituteNameOffset, SubstituteNameLength, PrintNameOffset, PrintNameLength, and Flags.
        /// </summary>
        public const int StaticPortionSizeInSymbolicLinkErrorResponse = 12;


        /// <summary>
        /// The count of lockSequence in smb3ServerOpen
        /// </summary>
        public const int LockSequenceCountInServerOpen = 64;
    }
}

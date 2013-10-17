using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Protocols.TestTools.StackSdk;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.Smb22;
using Microsoft.Protocols.TestTools.StackSdk.Security.Sspi;

namespace ProtocolTest_SMB3
{
    /// <summary>
    /// SMB2 client for functional testing. 
    /// By default signing is enabled and encryption is disabled.
    /// </summary>
    public class Smb2FunctionalTestClient
    {
        #region Private fields

        private Smb2Client client;

        private ulong messageId;
        private ulong sessionId;
        private byte[] sessionKey;

        private byte[] serverGssToken;

        #endregion

        #region Constructor

        public Smb2FunctionalTestClient(TimeSpan timeout)
        {
            client = new Smb2Client(timeout);
        }

        #endregion

        #region Properties

        public Smb2Client Smb2Client
        {
            get
            {
                return client;
            }
        }

        public ulong SessionId
        {
            get
            {
                return sessionId;
            }
        }

        #endregion

        #region Connect and Disconnect
        public void ConnectToServerOverTCP(IPAddress serverIp)
        {
            client.ConnectOverTCP(serverIp);
        }

        public void ConnectToServerOverTCP(IPAddress serverIp, IPAddress clientIp)
        {
            client.ConnectOverTCP(serverIp, clientIp);
        }

        public void Disconnect()
        {
            client.Disconnect();
        }

        #endregion

        #region Encryption and Signing Settings

        public void SetSessionSigningAndEncryption(bool enableSigning, bool enableEncryption)
        {
            client.SetSessionSigningAndEncryption(sessionId, sessionKey, enableSigning, enableEncryption);
        }

        public void SetTreeEncryption(uint treeId, bool enableEncryption)
        {
            client.SetTreeEncryption(sessionId, treeId, enableEncryption);
        }

        #endregion

        #region Negotiate

        public uint MultiProtocolNegotiate(string[] dialects, out DialectRevision selectedDialect)
        {
            Packet_Header header;
            NEGOTIATE_Response negotiateResponse;

            uint status = client.MultiProtocolNegotiate(
                dialects,
                out selectedDialect,
                out serverGssToken,
                out header,
                out negotiateResponse);

            this.messageId++;

            return status;
        }

        public uint Negotiate(DialectRevision[] dialects, SecurityMode_Values securityMode, Capabilities_Values capabilityValue, Guid clientGuid, out DialectRevision selectedDialect, out NEGOTIATE_Response negotiateResponse)
        {
            Packet_Header header;
         //   NEGOTIATE_Response negotiateResponse;

            return client.Negotiate(
                0,
                1,
                Packet_Header_Flags_Values.NONE,
                messageId++,
                dialects,
                securityMode,
                capabilityValue,
                clientGuid,
                out selectedDialect,
                out serverGssToken,
                out header,
                out negotiateResponse);
        }

        #endregion

        #region Echo
        //TODO: ECHO meassage should not expect a treeId and sessionId, potential TDI
        public uint Echo(uint treeId)
        {
            uint status;
            Packet_Header responseHeader;
            ECHO_Response responsePayload;
            status = client.Echo(
                1,
                1,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                out responseHeader,
                out responsePayload);

            return status;
        }
        #endregion

        #region Session Setup and Logoff

        public uint SessionSetup(
            SESSION_SETUP_Request_SecurityMode_Values securityMode,
            SESSION_SETUP_Request_Capabilities_Values capabilities,
            SecurityPackageType securityPackageType,
            string serverName,
            AccountCredential credential,
            bool useServerGssToken)
        {
            return SessionSetup(
                Packet_Header_Flags_Values.NONE,
                SESSION_SETUP_Request_Flags.NONE,
                securityMode,
                capabilities,
                0,
                securityPackageType,
                serverName,
                credential,
                useServerGssToken);
        }

        public uint AlternativeChannelSessionSetup(
            Smb2FunctionalTestClient mainChannelClient,
            SESSION_SETUP_Request_SecurityMode_Values securityMode,
            SESSION_SETUP_Request_Capabilities_Values capabilities,
            SecurityPackageType securityPackageType,
            string serverName,
            AccountCredential credential,
            bool useServerGssToken)
        {
            sessionId = mainChannelClient.sessionId;
            sessionKey = mainChannelClient.sessionKey;

            SetSessionSigningAndEncryption(true, false);

            return SessionSetup(
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                SESSION_SETUP_Request_Flags.SESSION_FLAG_BINDING,
                securityMode,
                capabilities,
                0,
                securityPackageType,
                serverName,
                credential,
                useServerGssToken);
        }

        public uint ReconnectSessionSetup(
            Smb2FunctionalTestClient previousClient,
            SESSION_SETUP_Request_SecurityMode_Values securityMode,
            SESSION_SETUP_Request_Capabilities_Values capabilities,
            SecurityPackageType securityPackageType,
            string serverName,
            AccountCredential credential,
            bool useServerGssToken)
        {
            return SessionSetup(
                Packet_Header_Flags_Values.NONE,
                SESSION_SETUP_Request_Flags.NONE,
                securityMode,
                capabilities,
                previousClient.sessionId,
                securityPackageType,
                serverName,
                credential,
                useServerGssToken);
        }

        private uint SessionSetup(
            Packet_Header_Flags_Values headerFlags,
            SESSION_SETUP_Request_Flags sessionSetupFlags,
            SESSION_SETUP_Request_SecurityMode_Values securityMode,
            SESSION_SETUP_Request_Capabilities_Values capabilities,
            ulong previousSessionId,
            SecurityPackageType securityPackageType,
            string serverName,
            AccountCredential credential,
            bool useServerGssToken)
        {
            Packet_Header header;
            SESSION_SETUP_Response sessionSetupResponse;

            SspiClientSecurityContext sspiClientGss =
                new SspiClientSecurityContext(
                    securityPackageType,
                    credential,
                    Smb2Utility.GetCifsServicePrincipalName(serverName),
                    ClientSecurityContextAttribute.None,
                    SecurityTargetDataRepresentation.SecurityNativeDrep);

            // Server GSS token is used only for Negotiate authentication when enabled
            if (securityPackageType == SecurityPackageType.Negotiate && useServerGssToken)
                sspiClientGss.Initialize(serverGssToken);
            else
                sspiClientGss.Initialize(null);

            uint status;
            do
            {
                status = client.SessionSetup(
                    1,
                    64,
                    headerFlags,
                    messageId++,
                    sessionId,
                    sessionSetupFlags,
                    securityMode,
                    capabilities,
                    previousSessionId,
                    sspiClientGss.Token,
                    out sessionId,
                    out serverGssToken,
                    out header,
                    out sessionSetupResponse);

                if ((status == Smb2Status.STATUS_MORE_PROCESSING_REQUIRED || status == Smb2Status.STATUS_SUCCESS) &&
                    serverGssToken != null && serverGssToken.Length > 0)
                {
                    sspiClientGss.Initialize(serverGssToken);
                }
            } while (status == Smb2Status.STATUS_MORE_PROCESSING_REQUIRED);

            if (status == Smb2Status.STATUS_SUCCESS)
            {
                sessionKey = sspiClientGss.SessionKey;
                SetSessionSigningAndEncryption(true, false);
            }

            return status;
        }

        public uint LogOff()
        {
            Packet_Header header;
            LOGOFF_Response logoffResponse;

            uint status = client.LogOff(
                1,
                64,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                out header,
                out logoffResponse);

            return status;
        }

        #endregion

        #region Tree Connect and Disconnect

        public uint TreeConnect(string uncSharePath, out uint treeId)
        {
            Packet_Header header;
            TREE_CONNECT_Response treeConnectResponse;

            uint status = client.TreeConnect(
                    1,
                    1,
                    Packet_Header_Flags_Values.FLAGS_SIGNED,
                    messageId++,
                    sessionId,
                    uncSharePath,
                    out treeId,
                    out header,
                    out treeConnectResponse);

            return status;
        }

        public uint TreeDisconnect(uint treeId)
        {
            Packet_Header header;
            TREE_DISCONNECT_Response treeDisconnectResponse;

            uint status = client.TreeDisconnect(
                1,
                64,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                out header,
                out treeDisconnectResponse);

            return status;
        }

        #endregion

        #region Create and Close

        public uint Create(
            uint treeId,
            string fileName,
            CreateOptions_Values createOptions,
            RequestedOplockLevel_Values requestedOplockLevel_Values,
            Smb2CreateContextRequest[] createContexts,
            out FILEID fileId,
            out OplockLevel_Values serverOplockLevel,
            ShareAccess_Values shareAccess = ShareAccess_Values.FILE_SHARE_READ | ShareAccess_Values.FILE_SHARE_WRITE | ShareAccess_Values.FILE_SHARE_DELETE)
        {
            Packet_Header header;
            CREATE_Response createResponse;
            Smb2CreateContextResponse[] serverCreateContexts;

            uint status = client.Create(
                1,
                64,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                fileName,
                AccessMask.GENERIC_READ | AccessMask.GENERIC_WRITE | AccessMask.DELETE,
                shareAccess,
                createOptions,
                CreateDisposition_Values.FILE_OPEN_IF,
                File_Attributes.NONE,
                ImpersonationLevel_Values.Impersonation,
                SecurityFlags_Values.NONE,
                requestedOplockLevel_Values,
                createContexts,
                out fileId,
                out serverCreateContexts,
                out header,
                out createResponse);

            serverOplockLevel = createResponse.OplockLevel;

            return status;
        }

        public uint Create(
            uint treeId,
            string fileName,
            CreateOptions_Values createOptions,
            AccessMask accessMask,
            ShareAccess_Values shareAccess,
            RequestedOplockLevel_Values requestedOplockLevel_Values,
            Smb2CreateContextRequest[] createContexts,
            out FILEID fileId,
            out CREATE_Response createResponse)
        {
            Packet_Header header;
            Smb2CreateContextResponse[] serverCreateContexts;

            uint status = client.Create(
                1,
                64,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                fileName,
                accessMask,
                shareAccess,
                createOptions,
                CreateDisposition_Values.FILE_OPEN_IF,
                File_Attributes.NONE,
                ImpersonationLevel_Values.Impersonation,
                SecurityFlags_Values.NONE,
                requestedOplockLevel_Values,
                createContexts,
                out fileId,
                out serverCreateContexts,
                out header,
                out createResponse);

            return status;
        }

        public uint Close(uint treeId, FILEID fileId)
        {
            Packet_Header header;
            CLOSE_Response closeResponse;

            uint status = client.Close(
                1,
                64,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                fileId,
                Flags_Values.NONE,
                out header,
                out closeResponse);

            return status;
        }

        #endregion

        #region Read and Write

        public uint Read(uint treeId, FILEID fileId, ulong offset, uint lengthToRead, out string data)
        {
            byte[] content;

            uint status = Read(treeId, fileId, offset, lengthToRead, out content);

            data = Encoding.ASCII.GetString(content);

            return status;
        }

        public uint Read(uint treeId, FILEID fileId, ulong offset, uint lengthToRead, out byte[] data)
        {
            Packet_Header header;
            READ_Response readResponse;

            //If a client the implements the SMB 2.1 dialect requests reading from a file and if Connection.SupportsMultiCredit is TRUE, 
            //the CreditCharge field in the SMB2 header MUST be set to ( 1 + (Length – 1) / 65536 ).
            int creditCharge = 1 + (((int)lengthToRead - 1) / 65535);
            uint status = client.Read(
                (ushort)creditCharge,
                64,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId,
                sessionId,
                treeId,
                lengthToRead,
                offset,
                fileId,
                0,
                Channel_Values.CHANNEL_NONE,
                0,
                new byte[0],
                out data,
                out header,
                out readResponse);

            messageId += (ulong)creditCharge;

            return status;
        }

        public uint Write(uint treeId, FILEID fileId, string data)
        {
            return Write(treeId, fileId, Encoding.ASCII.GetBytes(data));
        }

        public uint Write(uint treeId, FILEID fileId, byte[] data)
        {
            return Write(treeId, fileId, data, 0);
        }

        public uint Write(uint treeId, FILEID fileId, byte[] data, ulong offset)
        {
            Packet_Header header;
            WRITE_Response writeResponse;

            //If a client that implements the SMB 2.1 dialect requests writing to a file and if Connection.SupportsMultiCredit is TRUE, 
            //the CreditCharge field in the SMB2 header MUST be set to ( 1 + (Length – 1) / 65536 ).
            int creditCharge = 1 + ((data.Length - 1) / 65535);
            uint status = client.Write(
                (ushort)creditCharge,
                64,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId,
                sessionId,
                treeId,
                offset,
                fileId,
                Channel_Values.CHANNEL_NONE,
                WRITE_Request_Flags_Values.None,
                new byte[0],
                data,
                out header,
                out writeResponse);

            messageId += (ulong)creditCharge;

            return status;
        }

        #endregion

        #region Flush
        public uint Flush(uint treeId, FILEID fileId)
        {
            uint status;
            Packet_Header responseHeader;
            FLUSH_Response responsePayload;
            status = client.Flush(
                1,
                1,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                fileId,
                out responseHeader,
                out responsePayload);

            return status;
        }
        #endregion

        #region IOCTL

        public uint QueryNetworkInterfaceInfo(uint treeId, out List<string> ipList)
        {
            Packet_Header header;
            FILEID ioCtlFileId = new FILEID();
            ioCtlFileId.Persistent = 0xFFFFFFFFFFFFFFFF;
            ioCtlFileId.Volatile = 0xFFFFFFFFFFFFFFFF;
            IOCTL_Response ioCtlResponse;
            byte[] respInput = new byte[1024];
            byte[] respOutput = new byte[1024];
            byte[] buffer = new byte[1024];

            uint status = client.IoCtl(
                1,
                1,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                CtlCode_Values.FSCTL_QUERY_NETWORK_INTERFACE_INFO,
                ioCtlFileId,
                0,
                buffer,
                64 * 1024,
                IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL,
                out respInput,
                out respOutput,
                out header,
                out ioCtlResponse);

            NETWORK_INTERFACE_INFO_Response[] networkInfoResponses = Smb2Utility.UnmarshalNetworkInterfaceInfoResponse(respOutput);
            ipList = new List<string>();
            foreach (NETWORK_INTERFACE_INFO_Response netInfoResp in networkInfoResponses)
            {
                if (netInfoResp.AddressStorage.Address != null)
                {
                    ipList.Add(netInfoResp.AddressStorage.Address);
                }
            }
            return status;
        }

        public uint ReadHash(
            uint treeId,
            FILEID fileId,
            SRV_READ_HASH_Request_HashType_Values hashType,
            SRV_READ_HASH_Request_HashVersion_Values hashVersion,
            SRV_READ_HASH_Request_HashRetrievalType_Values hashRetrievalType,
            ulong hashOffset,
            uint hashLength,
            uint maxOutputResponse
            )
        {
            SRV_READ_HASH_Request readHash = new SRV_READ_HASH_Request();
            readHash.HashType = hashType;
            readHash.HashVersion = hashVersion;
            readHash.HashRetrievalType = hashRetrievalType;
            readHash.Offset = hashOffset;
            readHash.Length = hashLength;

            byte[] requestInput = TypeMarshal.ToBytes(readHash);
            byte[] responseInput;
            byte[] responseOutput;

            Packet_Header header;
            IOCTL_Response ioCtlResponse;

            uint status = client.IoCtl(
                1,
                1,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                CtlCode_Values.FSCTL_SRV_READ_HASH,
                fileId,
                0,
                requestInput,
                maxOutputResponse,
                IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL,
                out responseInput,
                out responseOutput,
                out header,
                out ioCtlResponse);

            return status;
        }

        public uint OffloadRead(
            uint treeId,
            FILEID fileId,
            ulong fileOffset,
            ulong copyLength,
            out ulong transferLength,
            out STORAGE_OFFLOAD_TOKEN token)
        {
            FSCTL_OFFLOAD_READ_INPUT offloadReadInput = new FSCTL_OFFLOAD_READ_INPUT();
            offloadReadInput.Size = 32;
            offloadReadInput.FileOffset = fileOffset;
            offloadReadInput.CopyLength = copyLength;

            byte[] requestInput = TypeMarshal.ToBytes(offloadReadInput);
            byte[] responseInput;
            byte[] responseOutput;

            Packet_Header header;
            IOCTL_Response ioCtlResponse;

            uint status = client.IoCtl(
                1,
                1,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                CtlCode_Values.FSCTL_OFFLOAD_READ,
                fileId,
                0,
                requestInput,
                32000,
                IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL,
                out responseInput,
                out responseOutput,
                out header,
                out ioCtlResponse);

            var offloadReadOutput = TypeMarshal.ToStruct<FSCTL_OFFLOAD_READ_OUTPUT>(responseOutput);
            transferLength = offloadReadOutput.TransferLength;
            token = offloadReadOutput.Token;

            return status;
        }

        public uint OffloadWrite(
            uint treeId,
            FILEID fileId,
            ulong fileOffset,
            ulong copyLength,
            ulong transferOffset,
            STORAGE_OFFLOAD_TOKEN token)
        {
            FSCTL_OFFLOAD_WRITE_INPUT offloadWriteInput = new FSCTL_OFFLOAD_WRITE_INPUT();
            offloadWriteInput.Size = 544;
            offloadWriteInput.FileOffset = fileOffset;
            offloadWriteInput.CopyLength = copyLength;
            offloadWriteInput.TransferOffset = transferOffset;
            offloadWriteInput.Token = token;

            

            byte[] requestInput = TypeMarshal.ToBytes(offloadWriteInput);
            byte[] responseInput;
            byte[] responseOutput;

            Packet_Header header;
            IOCTL_Response ioCtlResponse;
            
            uint status = client.IoCtl(
                1,
                1,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                CtlCode_Values.FSCTL_OFFLOAD_WRITE,
                fileId,
                0,
                requestInput,
                32000,
                IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL,
                out responseInput,
                out responseOutput,
                out header,
                out ioCtlResponse);

            return status;
        }

        #endregion

        #region Query Directory
        public uint QueryDirectory(
            uint treeId, 
            FileInformationClass_Values fileInfoClass,
            QUERY_DIRECTORY_Request_Flags_Values queryDirectoryFlags,
            uint fileIndex,
            FILEID fileId,
            out byte[] outputBuffer)
        {
            uint status;
            uint maxOutputBufferLength = 1024;
            Packet_Header responseHeader;
            QUERY_DIRECTORY_Response responsePayload;
            status = client.QueryDirectory(
                1,
                1,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                fileInfoClass,
                queryDirectoryFlags,
                fileIndex,
                fileId,
                "*",
                maxOutputBufferLength,
                out outputBuffer,
                out responseHeader,
                out responsePayload);
            return status;
        }
        #endregion

        #region Query and Set Info
        public uint QueryFileAttributes(
            uint treeId, 
            byte fileInfoClass, 
            QUERY_INFO_Request_Flags_Values queryInfoFlags, 
            FILEID fileId,
            byte[] inputBuffer,
            out byte[] outputBuffer)
        {
            uint maxOutputBufferLength = 1024;
            Packet_Header responseHeader;
            QUERY_INFO_Response responsePayload;

            uint status = client.QueryInfo(
                1,
                1,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                InfoType_Values.SMB2_0_INFO_FILE,
                fileInfoClass,
                maxOutputBufferLength,
                AdditionalInformation_Values.NONE,
                queryInfoFlags,
                fileId,
                inputBuffer,
                out outputBuffer,
                out responseHeader,
                out responsePayload);

            return status;
        }

        public uint SetFileAttributes(
                    uint treeId,
                    byte fileInfoClass,
                    FILEID fileId,
                    byte[] inputBuffer)
        {
            Packet_Header responseHeader;
            SET_INFO_Response responsePayload;

            uint status = client.SetInfo(
                1,
                1,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                SET_INFO_Request_InfoType_Values.SMB2_0_INFO_FILE,
                fileInfoClass,
                SET_INFO_Request_AdditionalInformation_Values.NONE,
                fileId,
                inputBuffer,
                out responseHeader,
                out responsePayload);

            return status;
        }

        #endregion

        #region Change Notify
        public void ChangeNotify(uint treeId, FILEID fileId, CompletionFilter_Values completionFilter)
        {
            uint maxOutputBufferLength = 1024;
            client.ChangeNotify(
                1,
                1,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                maxOutputBufferLength,
                fileId,    
                CHANGE_NOTIFY_Request_Flags_Values.NONE,
                completionFilter);          
        }
        #endregion

        #region Lock and Unlock
        public uint Lock(uint treeId, uint lockSequence, FILEID fileId, LOCK_ELEMENT[] locks)
        {
            uint status;

            Packet_Header responseHeader;
            LOCK_Response responsePayload;
            status = client.Lock(
                1,
                1,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                lockSequence,
                fileId,
                locks,
                out responseHeader,
                out responsePayload);

            return status;
        }

        #endregion

        #region Cancel
        public void Cancel()
        {
            client.Cancel(Packet_Header_Flags_Values.NONE, messageId - 1, sessionId);
        }
        #endregion

        #region Lease Break

        public uint LeaseBreakAcknowledgment(uint treeId, Guid leaseKey, LeaseStateValues leaseState)
        {
            Packet_Header header;
            LEASE_BREAK_Response leaseBreakResp;

            uint status = client.LeaseBreakAcknowledgment(
                1,
                1,
                Packet_Header_Flags_Values.FLAGS_SIGNED,
                messageId++,
                sessionId,
                treeId,
                leaseKey,
                leaseState,
                out header,
                out leaseBreakResp);

            return status;
        }

        #endregion
    }
}

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3;

namespace ProtocolTest_SMB3
{
    [TestClass]
    public class CreateRepaly
    {
        internal static smb3Client smb3ClientStack = new smb3Client();//smb3ClientConfig);

        internal static TimeSpan timeout = TimeSpan.FromMilliseconds(int.Parse("1000000"));
        [TestMethod]
        public void CreateRepalyMethod()
        {
            string serverName = "prasana-w2k12"; 
            string clientName = "prasanna-win8";
            string username = "Administrator";
            string password = "Welcome!123";
            string shareName = "Share";
            string fileName = "federer.txt";

            string domain = ".\\";
            ushort[] dialects = new ushort[1];
            int[] ConnectionID = new int[5];

            // Create Connections

            ConnectionID[0] = smb3ClientStack.Connect("172.25.220.106", true, 445);
            ConnectionID[1] = smb3ClientStack.Connect("172.25.220.104", true, 445);

            #region On Connection 0 ;
            smb3Client.connectionId = ConnectionID[0];
            smb3Client.globalContext.ClientGuid = new Guid(smb3Utility.CreateGuid());

            // Create smb3_NEGOTIATE request packet on Connection 0
            smb3Packet requestNegotiate = smb3ClientStack.CreateNegotiateRequest(NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_ALL, new ushort[] { 0x202, 0x210, 0x300 });


            smb3ClientStack.SendPacket(requestNegotiate);
            smb3Packet response = smb3ClientStack.ExpectPacket(timeout);
            smb3NegotiateResponsePacket negotiateResponse = (smb3NegotiateResponsePacket)response;


            // Create first smb3_SESSION_SETUP request packet
            smb3Packet requestSessionsetup = smb3ClientStack.CreateFirstSessionSetupRequest(
                0, // previousSessionId should be 0 for first session setup
                0, // bind flag/Vc number
                SESSION_SETUP_Request_Capabilities_Values.GLOBAL_CAP_DFS,
                SecurityPackage.Negotiate,
                ClientContextAttribute.None,
                domain,
                username,
                password,
                true);
            smb3ClientStack.SendPacket(requestSessionsetup);
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3SessionSetupResponsePacket sessionSetupResponse = (smb3SessionSetupResponsePacket)response;

            ulong sessionId = sessionSetupResponse.Header.SessionId;
            smb3Packet requestSessionsetup2 = smb3ClientStack.CreateSecondSessionSetupRequest(sessionId, 0);
            smb3ClientStack.SendPacket(requestSessionsetup2);
            response = smb3ClientStack.ExpectPacket(timeout);
            sessionSetupResponse = (smb3SessionSetupResponsePacket)response;

            ulong MasterSessionId = sessionSetupResponse.Header.SessionId;

            smb3TreeConnectRequestPacket requestTreeconnect = smb3ClientStack.CreateTreeConnectRequest(
                sessionId,
                shareName);
            smb3ClientStack.SendPacket(requestTreeconnect);
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3TreeConnectResponsePacket treeConnectResponse = (smb3TreeConnectResponsePacket)response;

            uint treeId = treeConnectResponse.Header.TreeId;


            VALIDATE_NEGOTIATE_INFO_Request validateNegotiate = new VALIDATE_NEGOTIATE_INFO_Request();
            validateNegotiate.Capabilities = NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_ALL;
            validateNegotiate.SecurityMode = SecurityMode_Values.NEGOTIATE_SIGNING_ENABLED;
            validateNegotiate.DialectCount = 3;
            validateNegotiate.Dialects = new ushort[] { 0x202, 0x210, 0x300 };
            validateNegotiate.ClientGuid = smb3Client.globalContext.ClientGuid;

            FILEID fileID;
            fileID.Persistent = 0xFFFFFFFFFFFFFFFF;
            fileID.Volatile = 0xFFFFFFFFFFFFFFFF;

            smb3Packet requestIOCTL = smb3ClientStack.CreateIOCtlRequest(true,
            sessionId, treeId, CtlCode_Values.FSCTL_VALIDATE_NEGOTIATE_INFO,
            fileID,
            0,
            24,
            IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL,
            smb3Utility.ToBytes<VALIDATE_NEGOTIATE_INFO_Request>(validateNegotiate));

            smb3ClientStack.SendPacket(requestIOCTL);
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3IOCtlResponsePacket IOCTResponse = (smb3IOCtlResponsePacket)response;

            requestIOCTL = smb3ClientStack.CreateIOCtlRequest(false, sessionId, treeId, CtlCode_Values.FSCTL_QUERY_NETWORK_INTERFACE_INFO,
            fileID,
            0,
            1000,
            IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL,
            null);

            smb3ClientStack.SendPacket(requestIOCTL);
            response = smb3ClientStack.ExpectPacket(timeout);
            IOCTResponse = (smb3IOCtlResponsePacket)response;

            uint desiredAccess = 0x0012019f;

            CREATE_REQUEST_LEASE_V2 requestLeaseV2 = new CREATE_REQUEST_LEASE_V2();
            requestLeaseV2.LeaseKey = smb3Utility.GenerateLeaseKey();
            requestLeaseV2.LeaseState = LeaseStateValues.SMB2_LEASE_WRITE_CACHING |
                LeaseStateValues.SMB2_LEASE_READ_CACHING |
                LeaseStateValues.SMB2_LEASE_HANDLE_CACHING;
            requestLeaseV2.ParentLeaseKey = smb3Utility.GenerateLeaseKey();
            requestLeaseV2.Epoch = 0;
            requestLeaseV2.Reserved = 0;
            requestLeaseV2.LeaseFlags = LeaseV2Flags.SMB2_LEASE_FLAG_PARENT_LEASE_KEY_SET;
            CREATE_CONTEXT_Values createRequestLeaseV2 = smb3Utility.CreateCreateContextValues(
                CreateContextTypeValue.SMB2_CREATE_REQUEST_LEASE,
                smb3Utility.ToBytes<CREATE_REQUEST_LEASE_V2>(requestLeaseV2));

            CREATE_DURABLE_HANDLE_REQUEST_V2 requestDurableV2 = new CREATE_DURABLE_HANDLE_REQUEST_V2();
            requestDurableV2.TimeOut = 0;
            requestDurableV2.Flags = PersistentFlags.SMB2_DHANDLE_NONE;
            requestDurableV2.Reserved = 0;
            requestDurableV2.CreateGuid = smb3Utility.CreateGuid();

            CREATE_CONTEXT_Values createRequestDurableV2 = smb3Utility.CreateCreateContextValues(
               CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_REQUEST_V2,
               smb3Utility.ToBytes<CREATE_DURABLE_HANDLE_REQUEST_V2>(requestDurableV2));


            smb3CreateRequestPacket requestCreate = smb3ClientStack.CreateCreateRequest(false,
                sessionId,
                treeId,
                RequestedOplockLevel_Values.SMB2_OPLOCK_LEVEL_LEASE,
                ImpersonationLevel_Values.Impersonation,
                desiredAccess,
                File_Attributes.FILE_ATTRIBUTE_ARCHIVE,
                ShareAccess_Values.FILE_SHARE_READ | ShareAccess_Values.FILE_SHARE_WRITE | ShareAccess_Values.FILE_SHARE_DELETE,
                CreateDisposition_Values.FILE_OPEN_IF,
                CreateOptions_Values.FILE_NON_DIRECTORY_FILE,
                fileName,
                new CREATE_CONTEXT_Values[] { createRequestDurableV2, createRequestLeaseV2 });


            // Send SMB2_CREATE request packet

            smb3ClientStack.SendPacket(requestCreate);


            // Receive SMB2_CREATE response
            //response = (smb3CreateResponsePacket)smb3ClientStack.ExpectPacket(timeout);
            # endregion

            #region On Connection 1 ;
            smb3Client.connectionId = ConnectionID[1];

            NEGOTIATE_Request_Capabilities_Values Capabilities = NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_CAP_DFS | NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_CAP_DIRECTORY_LEASING |
              NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_CAP_LARGE_MTU | NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_CAP_LEASING | NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_CAP_MULTI_CHANNEL | NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_CAP_PERSISTENT_HANDLES;

            requestNegotiate = smb3ClientStack.CreateNegotiateRequest(Capabilities, new ushort[] { 0x202, 0x210, 0x0300 });


            smb3ClientStack.SendPacket(requestNegotiate);
            response = smb3ClientStack.ExpectPacket(timeout);
            negotiateResponse = (smb3NegotiateResponsePacket)response;

            requestSessionsetup = smb3ClientStack.CreateFirstSessionSetupRequest(
                MasterSessionId, // MasterSessionId should be for session binding
                1,// Bindflag
                SESSION_SETUP_Request_Capabilities_Values.GLOBAL_CAP_DFS,
                SecurityPackage.Negotiate,
                ClientContextAttribute.None,
                domain,
                username,
                password,
                true);

            smb3ClientStack.SendPacket(requestSessionsetup);
            response = smb3ClientStack.ExpectPacket(timeout);
            sessionSetupResponse = (smb3SessionSetupResponsePacket)response;

            sessionId = sessionSetupResponse.Header.SessionId;

            requestSessionsetup2 = smb3ClientStack.CreateSecondSessionSetupRequest(sessionId, 1);
            smb3ClientStack.SendPacket(requestSessionsetup2);
            response = smb3ClientStack.ExpectPacket(timeout);
            sessionSetupResponse = (smb3SessionSetupResponsePacket)response;

            //requestTreeconnect = smb3ClientStack.CreateTreeConnectRequest(
            //    sessionId,
            //    shareName);
            //smb3ClientStack.SendPacket(requestTreeconnect);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //treeConnectResponse = (smb3TreeConnectResponsePacket)response;

            //treeId = treeConnectResponse.Header.TreeId;
            // Create smb3_CREATE request packet

            requestCreate = smb3ClientStack.CreateCreateRequest(true,
               sessionId,
               treeId,
               RequestedOplockLevel_Values.SMB2_OPLOCK_LEVEL_LEASE,
               ImpersonationLevel_Values.Impersonation,
               desiredAccess,
               File_Attributes.FILE_ATTRIBUTE_ARCHIVE,
               ShareAccess_Values.FILE_SHARE_READ | ShareAccess_Values.FILE_SHARE_WRITE | ShareAccess_Values.FILE_SHARE_DELETE,
               CreateDisposition_Values.FILE_OPEN_IF,
               CreateOptions_Values.FILE_NON_DIRECTORY_FILE,
               fileName,
               new CREATE_CONTEXT_Values[] { createRequestDurableV2, createRequestLeaseV2 });


            // Send SMB2_CREATE request packet

            smb3ClientStack.SendPacket(requestCreate);


            // Receive SMB2_CREATE response
            response = smb3ClientStack.ExpectPacket(timeout);
            # endregion
        }
    }
}

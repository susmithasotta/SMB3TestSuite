using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3;

namespace ProtocolTest_SMB3
{
    [TestClass]
    public class CreateReplay
    {
        internal static smb3Client smb3ClientStack = new smb3Client();//smb3ClientConfig);

        internal static TimeSpan timeout = TimeSpan.FromMilliseconds(int.Parse("1000000"));

        [TestMethod]
        public void CreateReplayTest()
        {
            string serverName = "Win8-SrvNode1";  
            string clientName = "Veeru-wipro";
            string username = "Administrator"; 
            string password = "Welcome!";
            string shareName = "share";
            string fileName = "sample.txt";



            string domain = ".\\";
            ushort[] dialects = new ushort[1];
            int[] ConnectionID = new int[5];

            // First Connection
            //  smb3ClientStack.Connect(clientName, serverName);

            smb3ClientStack.Connect(serverName, true, 445);

            // Create SMB2_NEGOTIATE request packet
            smb3Packet requestNegotiate = smb3ClientStack.CreateNegotiateRequest(NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_ALL, new ushort[] { 0x0224 });
        

            // Send SMB2_NEGOTIATE request packet
            smb3ClientStack.SendPacket(requestNegotiate);

            // Receive SMB2_NEGOTIATE response packet
            smb3Packet response = smb3ClientStack.ExpectPacket(timeout);

            smb3NegotiateResponsePacket negotiateResponse = (smb3NegotiateResponsePacket)response;


            // Create first SMB2_SESSION_SETUP request packet


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


            // Send first SMB2_SESSION_SETUP request packet
            smb3ClientStack.SendPacket(requestSessionsetup);

            // Receive first SMB2_SESSION_SETUP response
            response = smb3ClientStack.ExpectPacket(timeout);

            smb3SessionSetupResponsePacket sessionSetupResponse = (smb3SessionSetupResponsePacket)response;

            ulong sessionId = sessionSetupResponse.Header.SessionId;

            // Create second SMB2_SESSION_SETUP request packet
            smb3Packet requestSessionsetup2 = smb3ClientStack.CreateSecondSessionSetupRequest(sessionId, 0);


            // Send second SMB2_SESSION_SETUP request packet
            smb3ClientStack.SendPacket(requestSessionsetup2);

            // Receive second SMB2_SESSION_SETUP response
            response = smb3ClientStack.ExpectPacket(timeout);

            sessionSetupResponse = (smb3SessionSetupResponsePacket)response;

            ulong PreviousSessionId = sessionSetupResponse.Header.SessionId;
            smb3TreeConnectRequestPacket requestTreeconnect = smb3ClientStack.CreateTreeConnectRequest(
               sessionId,
               shareName);

            // Send SMB2_TREE_CONNECT request packet
            smb3ClientStack.SendPacket(requestTreeconnect);

            // Receive SMB2_TREE_CONNECT response
            response = smb3ClientStack.ExpectPacket(timeout);

            smb3TreeConnectResponsePacket treeConnectResponse = (smb3TreeConnectResponsePacket)response;

            uint treeId = treeConnectResponse.Header.TreeId;
            // Create SMB2_CREATE request packet
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
            response = smb3ClientStack.ExpectPacket(timeout);

            //smb3ClientStack.Disconnect();

            // Call Create Replay 
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

            smb3ClientStack.SendPacket(requestCreate);

            smb3CreateResponsePacket reponseCreate = (smb3CreateResponsePacket)response;
            

        }
    }
}

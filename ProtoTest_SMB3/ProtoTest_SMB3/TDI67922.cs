using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3;

namespace ProtoTest_SMB369124
{
    [TestClass]
    public class TDI67922
    {
        internal static smb3Client smb3ClientStack = new smb3Client();//smb3ClientConfig);

        internal static TimeSpan timeout = TimeSpan.FromMilliseconds(int.Parse("1000000"));

        [TestMethod]
        public void SMB3_GeneralTest67922()
        {
            string serverName = "pef1";
            string clientName = "susmitha-wipro";
            string username1 = "TestAdmin"; //"TestAdmin"; 
            string password1 = "Welcome!"; //"Welcome!"; 
            string username2 = "";
            string password2 = "";
            string shareName = "share2";
            string fileName = "abcd.txt";
            string filename1 = "test.txt";
            string destfileName = "dest.txt";
            //string fileName = "dir";
            string domain = ".\\";
            ushort[] dialects = new ushort[1];
            int[] ConnectionID = new int[5];
            int[] conn1 = new int[5];
            ConnectionID[0] = smb3ClientStack.Connect(serverName, true, 445);

            // NEGOTIATE
            smb3Packet requestNegotiate = smb3ClientStack.CreateNegotiateRequest(NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_ALL, new ushort[] { 0x0300 });


            smb3ClientStack.SendPacket(requestNegotiate);
            smb3Packet response = smb3ClientStack.ExpectPacket(timeout);
            smb3NegotiateResponsePacket negotiateResponse = (smb3NegotiateResponsePacket)response;


            // SESSION_SETUP
            smb3Packet requestSessionsetup = smb3ClientStack.CreateFirstSessionSetupRequest(
                0,
                0,
                SESSION_SETUP_Request_Capabilities_Values.GLOBAL_CAP_DFS,
                SecurityPackage.Negotiate,
                ClientContextAttribute.None,
                domain,
                username1,
                password1,
                true);
            smb3ClientStack.SendPacket(requestSessionsetup);
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3SessionSetupResponsePacket sessionSetupResponse = (smb3SessionSetupResponsePacket)response;

            ulong sessionId = sessionSetupResponse.Header.SessionId;

            smb3Packet requestSessionsetup2 = smb3ClientStack.CreateSecondSessionSetupRequest(sessionId, 0);
            smb3ClientStack.SendPacket(requestSessionsetup2);
            response = smb3ClientStack.ExpectPacket(timeout);
            sessionSetupResponse = (smb3SessionSetupResponsePacket)response;


  

            //TREE_CONNECT_Request
            smb3TreeConnectRequestPacket requestTreeconnect = smb3ClientStack.CreateTreeConnectRequest(
                sessionId,
                shareName);
            smb3ClientStack.SendPacket(requestTreeconnect);
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3TreeConnectResponsePacket treeConnectResponse = (smb3TreeConnectResponsePacket)response;

            uint treeId = treeConnectResponse.Header.TreeId;
            uint desiredAccess = 0xc0010000;

            ////SMB2_CREATE_DURABLE_HANDLE_REQUEST_V2

            CREATE_DURABLE_HANDLE_REQUEST_V2 requestDurableV2 = new CREATE_DURABLE_HANDLE_REQUEST_V2();
            requestDurableV2.TimeOut = 0;
            requestDurableV2.Flags = PersistentFlags.SMB2_DHANDLE_NONE;
            requestDurableV2.Reserved = 0;
            requestDurableV2.CreateGuid = smb3Utility.CreateGuid();

            CREATE_CONTEXT_Values createRequestDurableV2 = smb3Utility.CreateCreateContextValues(
               CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_REQUEST_V2,
               smb3Utility.ToBytes<CREATE_DURABLE_HANDLE_REQUEST_V2>(requestDurableV2));

            ////SMB2_CREATE_REQUEST_LEASE_V2

            CREATE_REQUEST_LEASE_V2 requestLeaseV2 = new CREATE_REQUEST_LEASE_V2();
            requestLeaseV2.LeaseKey = smb3Utility.GenerateLeaseKey();
            requestLeaseV2.LeaseState = LeaseStateValues.SMB2_LEASE_READ_CACHING |
                LeaseStateValues.SMB2_LEASE_HANDLE_CACHING;
            requestLeaseV2.ParentLeaseKey = smb3Utility.GenerateLeaseKey();
            requestLeaseV2.Epoch = 0;
            requestLeaseV2.Reserved = 0;
            requestLeaseV2.LeaseFlags = LeaseV2Flags.SMB2_LEASE_FLAG_PARENT_LEASE_KEY_SET;
            CREATE_CONTEXT_Values createRequestLeaseV2 = smb3Utility.CreateCreateContextValues(
                CreateContextTypeValue.SMB2_CREATE_REQUEST_LEASE,
                smb3Utility.ToBytes<CREATE_REQUEST_LEASE_V2>(requestLeaseV2));

            //SMB2_CREATE_DURABLE_HANDLE_REQUEST
            //CREATE_DURABLE_HANDLE_REQUEST requestDurable = new CREATE_DURABLE_HANDLE_REQUEST();
            //requestDurable.DurableRequest = new byte[16];

            //CREATE_CONTEXT_Values createRequestDurable = smb3Utility.CreateCreateContextValues(
            // CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_REQUEST,
            // smb3Utility.ToBytes<CREATE_DURABLE_HANDLE_REQUEST>(requestDurable));

            //SMB2 CREATE Request with durable handle context values

            smb3CreateRequestPacket requestCreate = smb3ClientStack.CreateCreateRequest(false,
                sessionId,
                treeId,
                RequestedOplockLevel_Values.SMB2_OPLOCK_LEVEL_LEASE,
                ImpersonationLevel_Values.Impersonation,
                desiredAccess,
                File_Attributes.NONE,
                ShareAccess_Values.NONE,
                CreateDisposition_Values.FILE_OPEN_IF,
                CreateOptions_Values.FILE_NON_DIRECTORY_FILE,
                fileName,
               new CREATE_CONTEXT_Values[] { createRequestDurableV2, createRequestLeaseV2 } //null
               );

            // Send SMB2_CREATE request packet

            smb3ClientStack.SendPacket(requestCreate);

            // Receive SMB2_CREATE response
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3CreateResponsePacket reponseCreate = (smb3CreateResponsePacket)response;

            FILEID fileID = new FILEID();
            fileID = reponseCreate.PayLoad.FileId;


            conn1[0] = smb3ClientStack.Connect(serverName, true, 445);

            // NEGOTIATE
            smb3Packet requestNegotiate2 = smb3ClientStack.CreateNegotiateRequest(NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_ALL, new ushort[] { 0x0300 });
            smb3ClientStack.SendPacket(requestNegotiate2);
            smb3Packet response1 = smb3ClientStack.ExpectPacket(timeout);
            smb3NegotiateResponsePacket negotiateResponse2 = (smb3NegotiateResponsePacket)response1;

            // SESSION_SETUP
            smb3Packet requestSessionsetup1 = smb3ClientStack.CreateFirstSessionSetupRequest(
                0,
                0,
                SESSION_SETUP_Request_Capabilities_Values.GLOBAL_CAP_DFS,
                SecurityPackage.Negotiate,
                ClientContextAttribute.None,
                domain,
                username1,
                password1,
                true);
            smb3ClientStack.SendPacket(requestSessionsetup1);
            response1 = smb3ClientStack.ExpectPacket(timeout);
            smb3SessionSetupResponsePacket sessionSetupResponse2 = (smb3SessionSetupResponsePacket)response1;

            ulong sessionId1 = sessionSetupResponse2.Header.SessionId;

            smb3Packet requestSessionsetup22 = smb3ClientStack.CreateSecondSessionSetupRequest(sessionId1, 0);
            smb3ClientStack.SendPacket(requestSessionsetup22);
            response1 = smb3ClientStack.ExpectPacket(timeout);
            sessionSetupResponse2 = (smb3SessionSetupResponsePacket)response1;
            //sessionId = sessionSetupResponse.Header.SessionId;


            // TREE_CONNECT
            smb3TreeConnectRequestPacket requestTreeconnect1 = smb3ClientStack.CreateTreeConnectRequest(
                sessionId1,
                shareName);
            smb3ClientStack.SendPacket(requestTreeconnect1);
            response1 = smb3ClientStack.ExpectPacket(timeout);
            smb3TreeConnectResponsePacket treeConnectResponse1 = (smb3TreeConnectResponsePacket)response1;

            uint treeId1 = treeConnectResponse.Header.TreeId;
            uint desiredAccess1 = 0xc0010000;

            //reconnect v1
          //  CREATE_DURABLE_HANDLE_RECONNECT dhnc = new CREATE_DURABLE_HANDLE_RECONNECT();
          //  dhnc.Data = fileID;

          //  CREATE_CONTEXT_Values createRequestDurablereconnect = smb3Utility.CreateCreateContextValues(
          //CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_RECONNECT,
          //smb3Utility.ToBytes<CREATE_DURABLE_HANDLE_RECONNECT>(dhnc));

            //reconnect v2

            CREATE_DURABLE_HANDLE_RECONNECT_V2 v2 = new CREATE_DURABLE_HANDLE_RECONNECT_V2();

            v2.fileid.Persistent = fileID.Persistent;
            v2.fileid.Volatile = 0x00000000;
            v2.Flags = PersistentFlags.SMB2_DHANDLE_NONE;
            v2.CreateGuid = smb3Utility.CreateGuid();

            CREATE_CONTEXT_Values createRequestDurablereconnectV2 = smb3Utility.CreateCreateContextValues(
                CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_RECONNECT_V2,
                smb3Utility.ToBytes<CREATE_DURABLE_HANDLE_RECONNECT_V2>(v2));

            //create2 with reconnect handle

            smb3CreateRequestPacket requestCreate1 = smb3ClientStack.CreateCreateRequest(false,
               sessionId1,
               treeId1,
               RequestedOplockLevel_Values.OPLOCK_LEVEL_NONE,
               ImpersonationLevel_Values.Impersonation,
               desiredAccess1,
               File_Attributes.NONE,
               ShareAccess_Values.NONE,
               CreateDisposition_Values.FILE_OPEN_IF,
               CreateOptions_Values.FILE_NON_DIRECTORY_FILE,
               fileName,
              new CREATE_CONTEXT_Values[] { createRequestDurablereconnectV2 } //null
              );

            // Send SMB2_CREATE request packet

            smb3ClientStack.SendPacket(requestCreate1);

            // Receive SMB2_CREATE response
            response1 = smb3ClientStack.ExpectPacket(timeout);
            smb3CreateResponsePacket reponseCreate2 = (smb3CreateResponsePacket)response1;

        }
    }
}

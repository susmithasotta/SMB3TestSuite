using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3;

namespace ProtoTest_SMB369124
{
    [TestClass]
    public class TDI67764
    {
        internal static smb3Client smb3ClientStack = new smb3Client();//smb3ClientConfig);

        internal static TimeSpan timeout = TimeSpan.FromMilliseconds(int.Parse("1000000"));

        [TestMethod]
        public void SMB3_GeneralTest67764()
        {
            string serverName = "naveen-wipro";
            string clientName = "susmitha-wipro";
            string username1 = "TestAdmin"; //"TestAdmin"; 
            string password1 = "Welcome!"; //"Welcome!"; 
            string username2 = "";
            string password2 = "";
            string shareName = "share1";
            string fileName = "abcd.txt";
            string destfileName = "dest.txt";
            //string fileName = "dir";
            string domain = ".\\";
            ushort[] dialects = new ushort[1];
            int[] ConnectionID = new int[5];

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

            ////TREE_CONNECT_Request
            //smb3TreeConnectRequestPacket requestTreeconnect = smb3ClientStack.CreateTreeConnectRequest(
            //    sessionId,
            //    shareName);
            //smb3ClientStack.SendPacket(requestTreeconnect);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //smb3TreeConnectResponsePacket treeConnectResponse = (smb3TreeConnectResponsePacket)response;

            //uint treeId = treeConnectResponse.Header.TreeId;

            //smb3LogOffRequestPacket loff_req = smb3ClientStack.CreateLogOffRequest(sessionId);
            //smb3ClientStack.SendPacket(loff_req);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //smb3LogOffResponsePacket loff_resp = (smb3LogOffResponsePacket)response;
            
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

            // CREATE
            smb3CreateRequestPacket createRequest = smb3ClientStack.CreateCreateRequest(
                false,
                sessionId,
                treeId,
                RequestedOplockLevel_Values.OPLOCK_LEVEL_NONE,
                ImpersonationLevel_Values.Impersonation,
                0x12019F,
                File_Attributes.FILE_ATTRIBUTE_NORMAL,
                ShareAccess_Values.FILE_SHARE_READ,
                CreateDisposition_Values.FILE_OPEN_IF,
                CreateOptions_Values.FILE_NON_DIRECTORY_FILE,
                fileName,
                null);

            smb3ClientStack.SendPacket(createRequest);
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3CreateResponsePacket createResponse = (smb3CreateResponsePacket)response;

            FILEID fileID;
            fileID = createResponse.PayLoad.FileId;


            smb3Packet requestSessionsetup3 = smb3ClientStack.CreateFirstSessionSetupRequest(
                sessionId,
                0,
                SESSION_SETUP_Request_Capabilities_Values.GLOBAL_CAP_DFS,
                SecurityPackage.Negotiate,
                ClientContextAttribute.None,
                domain,
                username1,
                password1,
                true);
            smb3ClientStack.SendPacket(requestSessionsetup3);
            response = smb3ClientStack.ExpectPacket(timeout);
            sessionSetupResponse = (smb3SessionSetupResponsePacket)response;

            ////create

            smb3CreateRequestPacket createRequest1 = smb3ClientStack.CreateCreateRequest(
                false,
                sessionId,
                treeId,
                RequestedOplockLevel_Values.OPLOCK_LEVEL_NONE,
                ImpersonationLevel_Values.Impersonation,
                0x12019F,
                File_Attributes.FILE_ATTRIBUTE_NORMAL,
                ShareAccess_Values.FILE_SHARE_READ,
                CreateDisposition_Values.FILE_OPEN_IF,
                CreateOptions_Values.FILE_NON_DIRECTORY_FILE,
                destfileName,
                null);

            smb3ClientStack.SendPacket(createRequest1);
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3CreateResponsePacket createResponse1 = (smb3CreateResponsePacket)response;

            FILEID fileID1;
            fileID1 = createResponse.PayLoad.FileId;

            ////Logoff

            //smb3LogOffRequestPacket loff_req = smb3ClientStack.CreateLogOffRequest(sessionId);
            //smb3ClientStack.SendPacket(loff_req);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //smb3LogOffResponsePacket loff_resp = (smb3LogOffResponsePacket)response;

            

            ////FILEID fileid;
            ////fileid.Persistent = 0x0000000;
            ////fileid.Volatile = 0x000000000;
            //////CLOSE - fails
            ////smb3CloseRequestPacket closeRequest = smb3ClientStack.CreateCloseRequest(Flags_Values.CLOSE_FLAG_POSTQUERY_ATTRIB, fileid);
            ////smb3ClientStack.SendPacket(closeRequest);
            ////response = smb3ClientStack.ExpectPacket(timeout);
            ////smb3CloseResponsePacket closeResponse = (smb3CloseResponsePacket)response;

            //sessionSetupResponse = (smb3SessionSetupResponsePacket)response;
            //sessionId = sessionSetupResponse.Header.SessionId;


            ////Logoff

            //smb3LogOffRequestPacket loff_req = smb3ClientStack.CreateLogOffRequest(sessionId);
            //smb3ClientStack.SendPacket(loff_req);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //smb3LogOffResponsePacket loff_resp = (smb3LogOffResponsePacket)response;
            //ulong sessionId = 3;

             //TREE_CONNECT_Request
            //smb3TreeConnectRequestPacket requestTreeconnect = smb3ClientStack.CreateTreeConnectRequest(
            //    sessionId,
            //    shareName);
            //smb3ClientStack.SendPacket(requestTreeconnect);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //smb3TreeConnectResponsePacket treeConnectResponse = (smb3TreeConnectResponsePacket)response;

            //uint treeId = treeConnectResponse.Header.TreeId;

            //FILEID fileid;
            //fileid.Persistent=0x0000000;
            //fileid.Volatile=0x000000000;
            ////CLOSE - fails
            //smb3CloseRequestPacket closeRequest = smb3ClientStack.CreateCloseRequest(Flags_Values.CLOSE_FLAG_POSTQUERY_ATTRIB, fileid);
            //smb3ClientStack.SendPacket(closeRequest);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //smb3CloseResponsePacket closeResponse = (smb3CloseResponsePacket)response;

            //smb3LogOffRequestPacket loff_req = smb3ClientStack.CreateLogOffRequest(sessionId);
            //smb3ClientStack.SendPacket(loff_req);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //smb3LogOffResponsePacket loff_resp = (smb3LogOffResponsePacket)response;



            //smb3Packet requestSessionsetup2 = smb3ClientStack.CreateSecondSessionSetupRequest(sessionId, 0);
            //smb3ClientStack.SendPacket(requestSessionsetup2);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //sessionSetupResponse = (smb3SessionSetupResponsePacket)response;
            //sessionId = sessionSetupResponse.Header.SessionId;

            ////Logoff

            //smb3LogOffRequestPacket loff_req = smb3ClientStack.CreateLogOffRequest(sessionId);
            //smb3ClientStack.SendPacket(loff_req);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //smb3LogOffResponsePacket loff_resp = (smb3LogOffResponsePacket)response;
        }
    }
}


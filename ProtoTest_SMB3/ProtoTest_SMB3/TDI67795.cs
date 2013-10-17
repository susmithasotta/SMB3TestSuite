using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3;

namespace ProtoTest_SMB369124
{
      [TestClass]
    public class TDI67795
    {
        internal static smb3Client smb3ClientStack = new smb3Client();//smb3ClientConfig);

        internal static TimeSpan timeout = TimeSpan.FromMilliseconds(int.Parse("1000000"));

        [TestMethod]
        public void SMB3_GeneralTest67795()
        {
            string serverName = "v-susott-dev";
            string clientName = "susmitha-wipro";
            string username1 = "TestAdmin"; //"TestAdmin"; 
            string password1 = "Welcome!"; //"Welcome!"; 
            string username2 = "";
            string password2 = "";
            string shareName = "share";
            string fileName = "test2.txt";
            string destfileName = "dest.txt";
            string dirname = "\\share1\\share";
            //string fileName = "dir";
            string domain = ".\\";
            ushort[] dialects = new ushort[1];
            int[] ConnectionID = new int[5];

            ConnectionID[0] = smb3ClientStack.Connect(serverName, true, 445);
            //ConnectionID[0]=smb3ClientStack.Connect(clientName, serverName);

            // NEGOTIATE
            smb3Packet requestNegotiate = smb3ClientStack.CreateNegotiateRequest(NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_ALL, new ushort[] { 0x0210});


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
            sessionId = sessionSetupResponse.Header.SessionId;

            // TREE_CONNECT
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

            //write request

            byte[] buffer = new byte[1024];
            for (int i = 0; i <= buffer.Length; i++)
            {
                smb3Utility.ToBytes(" ");
            }
            smb3WriteRequestPacket requestWrite = smb3ClientStack.CreateWriteRequest((ulong)0, fileID, (uint)0, buffer);
            smb3ClientStack.SendPacket(requestWrite);
            response = smb3ClientStack.ExpectPacket(timeout);
            response = (smb3WriteResponsePacket)response;

            //smb3QueryDirectoryRequestPacket qd = new smb3QueryDirectoryRequestPacket();
            //byte[] buffer = new byte[1024];
            //for (int i = 0; i <= buffer.Length; i++)
            //{
            //    smb3Utility.ToBytes("test");
            //}
            //smb3QueryDirectoryRequestPacket qdir_req = smb3ClientStack.CreateQueryDirectoryRequest(FileInformationClass_Values.FileDirectoryInformation,
            //QUERY_DIRECTORY_Request_Flags_Values.NONE, 0, fileID,
            //    1024, dirname
            // );

            //smb3ClientStack.SendPacket(qdir_req);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //response = (smb3QueryDirectoryResponePacket)response;
        }
    }
}

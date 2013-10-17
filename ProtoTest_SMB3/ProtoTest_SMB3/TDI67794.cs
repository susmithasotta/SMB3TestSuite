using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3;

namespace ProtoTest_SMB369124
{
    [TestClass]
    public class TDI67794
    {
        internal static smb3Client smb3ClientStack = new smb3Client();//smb3ClientConfig);

        internal static TimeSpan timeout = TimeSpan.FromMilliseconds(int.Parse("1000000"));

        [TestMethod]
        public void SMB3_GeneralTest67794()
        {
            string serverName = "v-susott-dev";
            string clientName = "susmitha-wipro";
            string username1 = "TestAdmin"; //"TestAdmin"; 
            string password1 = "Welcome!"; //"Welcome!"; 
            string username2 = "";
            string password2 = "";
            string shareName = "share";
            string fileName = "abcd.txt";
            string destfileName = "readclose.txt";
            string dirname = "dir";
            string domain = ".\\";
            ushort[] dialects = new ushort[1];
            int[] ConnectionID = new int[5];

            ConnectionID[0] = smb3ClientStack.Connect(serverName, true, 445);

            // NEGOTIATE
            smb3Packet requestNegotiate = smb3ClientStack.CreateNegotiateRequest(NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_ALL,new ushort[] { 0x0210 });


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
            //sessionId = sessionSetupResponse.Header.SessionId;

            // TREE_CONNECT
            smb3TreeConnectRequestPacket requestTreeconnect = smb3ClientStack.CreateTreeConnectRequest(
                sessionId,
                shareName);
            smb3ClientStack.SendPacket(requestTreeconnect);
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3TreeConnectResponsePacket treeConnectResponse = (smb3TreeConnectResponsePacket)response;

            uint treeId = treeConnectResponse.Header.TreeId;


          //create
          
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
               destfileName,
               null);

            smb3ClientStack.SendPacket(createRequest);
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3CreateResponsePacket createResponse = (smb3CreateResponsePacket)response;
            FILEID fileID;
            fileID = createResponse.PayLoad.FileId;
            
            //write request

            //byte[] buffer = new byte[131073];
            //for (int i = 0; i <= buffer.Length; i++)
            //{
            //    smb3Utility.ToBytes("test");

            //}
            //smb3WriteRequestPacket requestWrite = smb3ClientStack.CreateWriteRequest((ulong)0, fileID, (uint)buffer.Length, buffer);
            //smb3ClientStack.SendPacket(requestWrite);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //response = (smb3WriteResponsePacket)response;

            
            //read

            //smb3ReadRequestPacket read = smb3ClientStack.CreateReadRequest(131073, 0, fileID, 0);
            //smb3ClientStack.SendPacket(read);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //smb3ReadResponsePacket readResponse = (smb3ReadResponsePacket)response;

            

            //set info request
            //byte[] buffer = new byte[1048577];
            //for (int i = 0; i <= buffer.Length; i++)
            //{
            //    smb3Utility.ToBytes("test");
            //}
            //smb3SetInfoRequestPacket sinfo_req = smb3ClientStack.CreateSetInfoRequest(SET_INFO_Request_InfoType_Values.SMB2_0_INFO_FILE,
            //        4, SET_INFO_Request_AdditionalInformation_Values.NONE, fileID, buffer);
            //smb3ClientStack.SendPacket(sinfo_req);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //response = (smb3SetInfoResponsePacket)response;

            byte[] buffer = new byte[1048577];
            for (int i = 0; i <= buffer.Length; i++)
            {
                smb3Utility.ToBytes("test");
            }
            smb3QueryInfoRequestPacket qinfo_req = smb3ClientStack.CreateQueryInfoRequest(InfoType_Values.SMB2_0_INFO_FILE,
              4, 65537, AdditionalInformation_Values.NONE, QUERY_INFO_Request_Flags_Values.SL_INDEX_SPECIFIED, fileID, buffer);
            smb3ClientStack.SendPacket(qinfo_req);
            response = smb3ClientStack.ExpectPacket(timeout);
            response = (smb3QueryInfoResponsePacket)response;

            //if i am sending ioctl request for DFS FSCTL then it is throwing error file system driver required ---> but we are creating the share in disk
            //ioctl for snapshot is working fine.


            //inputbufferlength and outputbufferlength ---queryinfo request
    

        }
    }
}

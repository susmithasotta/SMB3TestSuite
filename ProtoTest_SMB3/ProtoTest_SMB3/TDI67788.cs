using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3;

namespace ProtoTest_SMB369124
{
    [TestClass]
    public class TDI67788
    {

        internal static smb3Client smb3ClientStack = new smb3Client();//smb3ClientConfig);

        internal static TimeSpan timeout = TimeSpan.FromMilliseconds(int.Parse("1000000"));

        [TestMethod]
        public void SMB3_GeneralTest()
        {
            string serverName = "naveen-2k8r2";
            string clientName = "v-susott-dev";
            string username1 = "Administrator"; //"TestAdmin"; 
            string password1 = "Welcome!"; //"Welcome!"; 
            string shareName = "share";
            string fileName = "abcd.txt";
            string destfileName = "dest.txt";
            //string fileName = "dir";
            string domain = ".\\";
            ushort[] dialects = new ushort[1];
            int[] ConnectionID = new int[5];

            ConnectionID[0] = smb3ClientStack.Connect(serverName, true, 445);

            // NEGOTIATE
            smb3Packet requestNegotiate = smb3ClientStack.CreateNegotiateRequest(NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_ALL, new ushort[] { 0x0202 });
        
                      
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
         
            FILEID fileID;
            fileID.Persistent = 0xFFFFFFFFFFFFFFFF;
            fileID.Volatile = 0xFFFFFFFFFFFFFFFF;

            
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
            fileID = createResponse.PayLoad.FileId;

            //IOCTL request

           byte[] inputBuffer = new byte[56];
           smb3IOCtlRequestPacket ioctlreq = smb3ClientStack.CreateIOCtlRequest(false, sessionId, treeId,
             CtlCode_Values.FSCTL_DFS_GET_REFERRALS, fileID,0,4096,IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL,inputBuffer );
           smb3ClientStack.SendPacket(ioctlreq);

           response = smb3ClientStack.ExpectPacket(timeout);
           smb3IOCtlResponsePacket ioctlResponse = (smb3IOCtlResponsePacket)response;
          

            //CLOSE - fails
            smb3CloseRequestPacket closeRequest = smb3ClientStack.CreateCloseRequest(Flags_Values.CLOSE_FLAG_POSTQUERY_ATTRIB, fileID);
            smb3ClientStack.SendPacket(closeRequest);
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3CloseResponsePacket closeResponse = (smb3CloseResponsePacket)response;

            //TREE_DISCONNECT
            smb3TreeDisconnectRequestPacket treeDisconnectReq = smb3ClientStack.CreateTreeDisconnectRequest(sessionId, treeId);
            smb3ClientStack.SendPacket(treeDisconnectReq);
            smb3ClientStack.ExpectPacket(timeout);           
        }
    }
}

    
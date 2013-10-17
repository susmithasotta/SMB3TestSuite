using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3;

namespace ProtoTest_SMB369124
{
    /// <summary>
    /// Summary description for MultiChannel
    /// </summary>
    [TestClass]
    public class General67741
    {
        internal static smb3Client smb3ClientStack = new smb3Client();//smb3ClientConfig);

        internal static TimeSpan timeout = TimeSpan.FromMilliseconds(int.Parse("1000000"));

        [TestMethod]
        public void SMB3_GeneralTest67741()
        {
            string serverName = "fsmtserver";
            string clientName = "susmitha-wipro";
            string username1 = "fsmtuser"; //"TestAdmin"; 
            string password1 = "wipro@123"; //"Welcome!"; 
            string username2 = "";
            string password2 = "";
            string shareName = "share2";
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

                    
            FILEID fileID = new FILEID();
            fileID.Persistent = 0xFFFFFFFFFFFFFFFF;
            fileID.Volatile = 0xFFFFFFFFFFFFFFFF;
         
            //FSCTL_VALIDATE_NEGOTIATE
            VALIDATE_NEGOTIATE_INFO_Request validateNegotiate = new VALIDATE_NEGOTIATE_INFO_Request();
            validateNegotiate.Capabilities = NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_ALL;
            validateNegotiate.SecurityMode = SecurityMode_Values.NEGOTIATE_SIGNING_ENABLED;
            validateNegotiate.DialectCount = 1;
            validateNegotiate.Dialects = new ushort[] {0x0300};
            validateNegotiate.ClientGuid =smb3Client.globalContext.ClientGuid;


            smb3Packet requestIOCTL = smb3ClientStack.CreateIOCtlRequest(true, sessionId, treeId, CtlCode_Values.FSCTL_VALIDATE_NEGOTIATE_INFO,
            fileID, 0, 65536, IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL,
            smb3Utility.ToBytes<VALIDATE_NEGOTIATE_INFO_Request>(validateNegotiate));

            smb3ClientStack.SendPacket(requestIOCTL);
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

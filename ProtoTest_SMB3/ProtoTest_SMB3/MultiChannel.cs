using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3;

namespace ProtocolTest_SMB3
{
    /// <summary>
    /// Summary description for MultiChannel
    /// </summary>
    [TestClass]
    public class MultiChannel
    {
        internal static smb3Client smb3ClientStack = new smb3Client();//smb3ClientConfig);

        internal static TimeSpan timeout = TimeSpan.FromMilliseconds(int.Parse("1000000"));
        [TestMethod]
        public void MultiChannelTest()
        {
            string serverName = "prasanna-win8"; 
            string clientName = "prasanna-msft";
            string username = "Administrator";
            string password = "Welcome@123";
            string shareName = "Share";
            string fileName = "federer.txt";
            
            string domain = ".\\";
            ushort[] dialects = new ushort[1];
            int[] ConnectionID = new int[5];

            // Create Connections
           
            ConnectionID[0] = smb3ClientStack.Connect("172.25.220.106", true, 445);
            ConnectionID[1] = smb3ClientStack.Connect("172.25.220.107", true, 445);

            #region On Connection 0 ;
            smb3Client.connectionId = ConnectionID[0];

            // Create smb3_NEGOTIATE request packet on Connection 0
            smb3Packet requestNegotiate = smb3ClientStack.CreateNegotiateRequest(NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_ALL, new ushort[] { 0x0300 });
        
                      
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

            VALIDATE_NEGOTIATE_INFO_Request validateNegotiate = new VALIDATE_NEGOTIATE_INFO_Request();
            validateNegotiate.Capabilities = (NEGOTIATE_Request_Capabilities_Values)0x7F;
            validateNegotiate.SecurityMode = SecurityMode_Values.NEGOTIATE_SIGNING_ENABLED;
            validateNegotiate.DialectCount = 4;
            validateNegotiate.Dialects = new ushort[] {0x200,0x202,0x210,0x300};
            validateNegotiate.ClientGuid = smb3Client.globalContext.ClientGuid;
         
                      
            FILEID fileID;
            fileID.Persistent = 0xFFFFFFFFFFFFFFFF;
            fileID.Volatile = 0xFFFFFFFFFFFFFFFF;

            smb3Packet requestIOCTL = smb3ClientStack.CreateIOCtlRequest(true,sessionId, treeId, CtlCode_Values.FSCTL_VALIDATE_NEGOTIATE_INFO,
            fileID,
            0,
            32,
            IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL,
            smb3Utility.ToBytes<VALIDATE_NEGOTIATE_INFO_Request>(validateNegotiate));

           smb3ClientStack.SendPacket(requestIOCTL);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //smb3IOCtlResponsePacket IOCTResponse = (smb3IOCtlResponsePacket)response;

            requestIOCTL = smb3ClientStack.CreateIOCtlRequest(false,sessionId,treeId,CtlCode_Values.FSCTL_QUERY_NETWORK_INTERFACE_INFO,
            fileID,
            0,
            1000,
            IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL,
            null);

            smb3ClientStack.SendPacket(requestIOCTL);
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3IOCtlResponsePacket IOCTResponse = (smb3IOCtlResponsePacket)response;
# endregion
            #region On Connection 1 ;
            smb3Client.connectionId = ConnectionID[1];

            NEGOTIATE_Request_Capabilities_Values Capabilities = NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_CAP_DFS | NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_CAP_DIRECTORY_LEASING |
              NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_CAP_LARGE_MTU | NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_CAP_LEASING | NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_CAP_MULTI_CHANNEL | NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_CAP_PERSISTENT_HANDLES;
          
            requestNegotiate = smb3ClientStack.CreateNegotiateRequest(Capabilities, new ushort[] { 0x0300 });


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


/*
            // Create smb3_CREATE request packet
            uint desiredAccess = 0x2010000;
            #endregion
            #region On connection 0
            smb3Client.connectionId = 0;

            smb3Packet requestCreate = smb3ClientStack.CreateCreateRequest(false,
                sessionId,
                treeId,
                RequestedOplockLevel_Values.OPLOCK_LEVEL_NONE,
                ImpersonationLevel_Values.Impersonation,
                desiredAccess,
                File_Attributes.NONE,
                ShareAccess_Values.FILE_SHARE_READ | ShareAccess_Values.FILE_SHARE_WRITE | ShareAccess_Values.FILE_SHARE_DELETE,
                CreateDisposition_Values.FILE_CREATE | CreateDisposition_Values.FILE_OPEN,
                CreateOptions_Values.NONE,
                "temp123.txt",
                null); // No need for Create contexts



            // Send smb3_CREATE request packet
            smb3ClientStack.SendPacket(requestCreate);

            // Receive smb3_CREATE response
            response = smb3ClientStack.ExpectPacket(timeout);

            smb3CreateResponsePacket createResponse = (smb3CreateResponsePacket)response;
            FILEID fileId = createResponse.PayLoad.FileId;


            smb3Client.connectionId = 0;
            uint lenghtRead = 10;
            ulong offset = 0;
            uint minCount = 10;
            smb3ReadRequestPacket requestRead = smb3ClientStack.CreateReadRequest(sessionId,
                treeId,
                lenghtRead,
                offset,
                fileId,
                minCount);

            smb3ClientStack.SendPacket(requestRead);

            // Receive smb3_CREATE response
            response = smb3ClientStack.ExpectPacket(timeout);

            smb3ReadResponsePacket readResponse = (smb3ReadResponsePacket)response;*/

            #endregion
        }
    }
}

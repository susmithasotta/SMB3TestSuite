using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3;

namespace ProtoTest_SMB3
{
    /// <summary>
    /// Summary description for MultiChannel
    /// </summary>
    [TestClass]
    public class General
    {
        internal static smb3Client smb3ClientStack = new smb3Client();//smb3ClientConfig);

        internal static TimeSpan timeout = TimeSpan.FromMilliseconds(int.Parse("1000000"));

        [TestMethod]
        public void SMB3_GeneralTest()
        {
            string serverName = "prasanna-win8rc";
            //string clientName = "prasanna-msft";
            //string username = "Administrator";
            string username = "prasanna";
            string password = "Welcome@123";
            string shareName = "Share";
            string fileName = "federer.txt";
            
            string domain = ".\\";
            ushort[] dialects = new ushort[1];
            int ConnectionID = smb3ClientStack.Connect(serverName, true, 445);
            
            smb3Client.connectionId = ConnectionID;

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
            //sessionId = sessionSetupResponse.Header.SessionId;

            // TREE_CONNECT
            smb3TreeConnectRequestPacket requestTreeconnect = smb3ClientStack.CreateTreeConnectRequest(
                sessionId,
                shareName);
            smb3ClientStack.SendPacket(requestTreeconnect);
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3TreeConnectResponsePacket treeConnectResponse = (smb3TreeConnectResponsePacket)response;

            uint treeId = treeConnectResponse.Header.TreeId;

            //FSCTL_VALIDATE_NEGOTIATE
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
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3IOCtlResponsePacket ioctlResponse = (smb3IOCtlResponsePacket)response;

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

            //NETWORK_RESILIENCY
            NETWORK_RESILIENCY_Request networkRes = new NETWORK_RESILIENCY_Request();
            networkRes.Timeout = 100;
            smb3IOCtlRequestPacket networkResiliencyReq = smb3ClientStack.CreateNetworkResiliencyIOCtlRequest(fileID, networkRes);
            smb3ClientStack.SendPacket(networkResiliencyReq);
            smb3ClientStack.ExpectPacket(timeout);

            // Create SMB2_LOCK request packet
            ulong readLength = 10;
            LOCK_ELEMENT lockElement = new LOCK_ELEMENT();
            lockElement.Flags = LOCK_ELEMENT_Flags_Values.LOCKFLAG_EXCLUSIVE_LOCK;
            lockElement.Offset = 0;
            lockElement.Length = readLength;
            smb3LockRequestPacket lockRequest = smb3ClientStack.CreateLockRequest(
                fileID,
                100,
                new LOCK_ELEMENT[] { lockElement });

            // Send SMB2_LOCK request packet
            smb3ClientStack.SendPacket(lockRequest);

            // Receive SMB2_LOCK response
            response = smb3ClientStack.ExpectPacket(timeout);
            smb3LockResponsePacket lockResponse = (smb3LockResponsePacket)response;

            // Send SMB2_LOCK request packet AGAIN - Likewise issue at plugfest ###
            // Create SMB2_LOCK request packet
            ulong readLength2 = 10;
            LOCK_ELEMENT lockElement2 = new LOCK_ELEMENT();
            lockElement.Flags = LOCK_ELEMENT_Flags_Values.LOCKFLAG_EXCLUSIVE_LOCK;
            lockElement.Offset = 0;
            lockElement.Length = readLength2;
            smb3LockRequestPacket lockRequest2 = smb3ClientStack.CreateLockRequest(
                fileID,
                100,
                new LOCK_ELEMENT[] { lockElement2 });

            smb3ClientStack.SendPacket(lockRequest2);

            // Receive SMB2_LOCK response
            response = smb3ClientStack.ExpectPacket(timeout);
            lockResponse = (smb3LockResponsePacket)response;

            //TREE_DISCONNECT
            smb3TreeDisconnectRequestPacket treeDisconnectReq = smb3ClientStack.CreateTreeDisconnectRequest(sessionId, treeId);
            smb3ClientStack.SendPacket(treeDisconnectReq);
            smb3ClientStack.ExpectPacket(timeout);

            ////CLOSE - fails
            //smb3CloseRequestPacket closeRequest = smb3ClientStack.CreateCloseRequest(Flags_Values.CLOSE_FLAG_POSTQUERY_ATTRIB, fileID);
            //smb3ClientStack.SendPacket(createRequest);
            //response = smb3ClientStack.ExpectPacket(timeout);
            //smb3CloseResponsePacket closeResponse = (smb3CloseResponsePacket)response;
            
        }
    }
}

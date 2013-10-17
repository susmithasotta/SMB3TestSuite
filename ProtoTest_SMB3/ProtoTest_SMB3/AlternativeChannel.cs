using System;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Protocols.TestTools.StackSdk.Security.Sspi;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.Smb22;

namespace ProtocolTest_SMB3
{
    [TestClass]
    public class AlternativeChannel
    {
        static Smb2FunctionalTestClient mainChannelClient;
        static Smb2FunctionalTestClient alternativeChannelClient;
        static uint status;
        protected string fileName;
        protected IPAddress clientIps;
        protected IPAddress serverIps;
        TimeSpan timeout = new TimeSpan(10000000);
        [TestMethod]
        public void AlternativeChannelMenthod()
        {
            byte [] Sip =  {172,25,220,92};
            serverIps= new IPAddress(Sip);
            byte [] Cip = {172,25,220,44} ;
            clientIps =  new IPAddress (Cip);
            uint status = 0;
            DialectRevision[] requestDialect = { DialectRevision.Smb2002, DialectRevision.Smb21,DialectRevision.Smb224 } ;
            DialectRevision selectedDialect;
            mainChannelClient = new Smb2FunctionalTestClient(timeout);
            alternativeChannelClient = new Smb2FunctionalTestClient(timeout);
            mainChannelClient.ConnectToServerOverTCP(serverIps, clientIps);

            NEGOTIATE_Response negotiateResponse = new NEGOTIATE_Response();

             status = mainChannelClient.Negotiate(
                requestDialect, 
                SecurityMode_Values.NEGOTIATE_SIGNING_ENABLED,
                Capabilities_Values.GLOBAL_CAP_DFS | Capabilities_Values.GLOBAL_CAP_DIRECTORY_LEASING | Capabilities_Values.GLOBAL_CAP_LARGE_MTU | Capabilities_Values.GLOBAL_CAP_LEASING | Capabilities_Values.GLOBAL_CAP_MULTI_CHANNEL | Capabilities_Values.GLOBAL_CAP_PERSISTENT_HANDLES,
                Guid.NewGuid(),
                out selectedDialect,
                out negotiateResponse);

                    

            status = mainChannelClient.SessionSetup(
                SESSION_SETUP_Request_SecurityMode_Values.NEGOTIATE_SIGNING_ENABLED,
                SESSION_SETUP_Request_Capabilities_Values.GLOBAL_CAP_DFS,
                SecurityPackageType.Negotiate,
                "Win8-SRVNode1",
                new AccountCredential("","Administrator","Welcome!"),
                true);
            SecurityPackageType securityPackageType1;
        }
    }
}

//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3NegotiateRequestPacket
// Description: smb3NegotiateRequestPacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 NEGOTIATE Request packet is used by the client 
    /// to notify the server what dialects of the SMB 2 Protocol the client understands
    /// </summary>
    public class smb3NegotiateRequestPacket : smb3StandardPacket<NEGOTIATE_Request>, IHasInterrelatedFields
    {
        /// <summary>
        /// Update all fields which is related to other fields which has been changed
        /// </summary>
        public void UpdateInterrelatedFields()
        {
            PayLoad.DialectCount = (ushort)PayLoad.Dialects.Length;
        }
    }
}

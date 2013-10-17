//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3SessionSetupRequestPacket
// Description: smb3SessionSetupRequestPacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 SESSION_SETUP Request packet is sent by the client to request a new authenticated 
    /// session within a new or existing SMB 2 Protocol transport connection to the server
    /// </summary>
    public class smb3SessionSetupRequestPacket : smb3StandardPacket<SESSION_SETUP_Request>, IHasInterrelatedFields 
    {
        /// <summary>
        /// Update all fields which is related to other fields which has been changed
        /// </summary>
        public void UpdateInterrelatedFields()
        {
            PayLoad.SecurityBufferLength = (ushort)PayLoad.Buffer.Length;
        }
    }
}

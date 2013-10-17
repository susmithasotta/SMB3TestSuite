//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3SessionSetupResponsePacket
// Description: smb3SessionSetupResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 SESSION_SETUP Response packet is sent by the server in response to an smb3 SESSION_SETUP Request packet
    /// </summary>
    public class smb3SessionSetupResponsePacket : smb3StandardPacket<SESSION_SETUP_Response>, IHasInterrelatedFields
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

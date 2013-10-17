//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ReadResponsePacket
// Description: smb3ReadResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 READ Response packet is sent in response to an smb3 READ Request (section 2.2.19) packet
    /// </summary>
    public class smb3ReadResponsePacket : smb3StandardPacket<READ_Response>, IHasInterrelatedFields
    {
        /// <summary>
        /// Update all fields which is related to other fields which has been changed
        /// </summary>
        public void UpdateInterrelatedFields()
        {
            PayLoad.DataLength = (uint)PayLoad.Buffer.Length;
        }
    }
}

//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3NegotiateResponsePacket
// Description: smb3NegotiateResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 NEGOTIATE Response packet is sent by the server to notify the client of the preferred common dialect
    /// </summary>
    public class smb3NegotiateResponsePacket : smb3StandardPacket<NEGOTIATE_Response>, IHasInterrelatedFields
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

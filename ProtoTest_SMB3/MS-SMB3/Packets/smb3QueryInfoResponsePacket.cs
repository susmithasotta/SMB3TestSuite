//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3QueryInfoResponsePacket
// Description: smb3QueryInfoResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 QUERY_INFO Response packet is sent by the server in response to an smb3 QUERY_INFO Request packet
    /// </summary>
    public class smb3QueryInfoResponsePacket : smb3StandardPacket<QUERY_INFO_Response>, IHasInterrelatedFields
    {
        /// <summary>
        /// Update all fields which is related to other fields which has been changed
        /// </summary>
        public void UpdateInterrelatedFields()
        {
            PayLoad.OutputBufferLength = (uint)PayLoad.Buffer.Length;
        }
    }
}
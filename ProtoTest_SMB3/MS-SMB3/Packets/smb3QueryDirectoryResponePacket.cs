//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3QueryDirectoryResponePacket
// Description: smb3QueryDirectoryResponePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 QUERY_DIRECTORY Response packet is sent by a server
    /// in response to an smb3 QUERY_DIRECTORY Request (section 2.2.33)
    /// </summary>
    public class smb3QueryDirectoryResponePacket : smb3StandardPacket<QUERY_DIRECTORY_Response>, IHasInterrelatedFields
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

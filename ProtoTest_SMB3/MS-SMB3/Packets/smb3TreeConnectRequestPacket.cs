//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3TreeConnectRequestPacket
// Description: smb3TreeConnectRequestPacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 TREE_DISCONNECT Request packet is sent by the client to request that the tree connect
    /// that is specified in the TreeId within the smb3 header be disconnected
    /// </summary>
    public class smb3TreeConnectRequestPacket : smb3StandardPacket<TREE_CONNECT_Request>, IHasInterrelatedFields
    {
        /// <summary>
        /// Update all fields which is related to other fields which has been changed
        /// </summary>
        public void UpdateInterrelatedFields()
        {
            PayLoad.PathLength = (ushort)PayLoad.Buffer.Length;
        }
    }
}

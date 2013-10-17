//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3TreeConnectResponsePacket
// Description: smb3TreeConnectResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 TREE_CONNECT Response packet is sent by the server when an smb3 TREE_CONNECT request is processed successfully by the server
    /// </summary>
    public class smb3TreeConnectResponsePacket : smb3StandardPacket<TREE_CONNECT_Response>
    {
    }
}

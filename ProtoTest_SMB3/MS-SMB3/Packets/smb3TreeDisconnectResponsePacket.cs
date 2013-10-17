//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3TreeDisconnectResponsePacket
// Description: smb3TreeDisconnectResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 TREE_DISCONNECT Response packet is sent by the server to confirm 
    /// that an smb3 TREE_DISCONNECT Request (section 2.2.11) was successfully processed
    /// </summary>
    public class smb3TreeDisconnectResponsePacket : smb3StandardPacket<TREE_DISCONNECT_Response>
    {
    }
}

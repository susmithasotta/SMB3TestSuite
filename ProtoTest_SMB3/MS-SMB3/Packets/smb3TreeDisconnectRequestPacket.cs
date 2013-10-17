//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3OpLockBreakAckPacket
// Description: smb3OpLockBreakAckPacket defination.
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 TREE_DISCONNECT Request packet is sent by the client to request that the tree connect that is specified 
    /// in the TreeId within the smb3 header be disconnected
    /// </summary>
    public class smb3TreeDisconnectRequestPacket : smb3StandardPacket<TREE_DISCONNECT_Request>
    {
    }
}

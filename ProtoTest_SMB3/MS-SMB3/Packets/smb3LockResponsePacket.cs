//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3LockResponsePacket
// Description: smb3LockResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 LOCK Response packet is sent by a server in response to an smb3 LOCK Request (section 2.2.26) packet
    /// </summary>
    public class smb3LockResponsePacket : smb3StandardPacket<LOCK_Response>
    {
    }
}

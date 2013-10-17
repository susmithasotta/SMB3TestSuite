//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3LeaseBreakAckPacket
// Description: smb3LeaseBreakAckPacket defination.
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 Lease Break Acknowledgment packet is sent by the client in response 
    /// to an smb3 Lease Break Notification packet sent by the server
    /// </summary>
    public class smb3LeaseBreakAckPacket : smb3StandardPacket<LEASE_BREAK_Acknowledgment>
    {
    }
}

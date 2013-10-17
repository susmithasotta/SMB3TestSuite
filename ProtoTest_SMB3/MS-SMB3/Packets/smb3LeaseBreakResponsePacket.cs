//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3LeaseBreakResponsePacket
// Description: smb3LeaseBreakResponsePacket defination.
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 Lease Break Response packet is sent by the server in response to a Lease Break Acknowledgment from the client
    /// </summary>
    public class smb3LeaseBreakResponsePacket : smb3StandardPacket<LEASE_BREAK_Response>
    {
    }
}

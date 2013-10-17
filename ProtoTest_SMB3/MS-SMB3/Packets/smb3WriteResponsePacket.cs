//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3WriteResponsePacket
// Description: smb3WriteResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 WRITE Response packet is sent in response to an smb3 WRITE Request (section 2.2.21) packet
    /// </summary>
    public class smb3WriteResponsePacket : smb3StandardPacket<WRITE_Response>
    {
    }
}

//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3FlushResponsePacket
// Description: smb3FlushResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 FLUSH Response packet is sent by the server to confirm 
    /// that an smb3 FLUSH Request (section 2.2.17) was successfully processed
    /// </summary>
    public class smb3FlushResponsePacket : smb3StandardPacket<FLUSH_Response>
    {
    }
}

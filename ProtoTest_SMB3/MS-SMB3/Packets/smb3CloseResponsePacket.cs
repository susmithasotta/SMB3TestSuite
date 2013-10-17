//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3CloseResponsePacket
// Description: smb3CloseResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 CLOSE Response packet is sent by the server to indicate that an 
    /// smb3 CLOSE Request was processed successfully
    /// </summary>
    public class smb3CloseResponsePacket : smb3StandardPacket<CLOSE_Response>
    {
    }
}

//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3SetInfoResponsePacket
// Description: smb3SetInfoResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 SET_INFO Response packet is sent by the server in response to an smb3 SET_INFO Request
    /// (section 2.2.39) to notify the client that its request has been successfully processed
    /// </summary>
    public class smb3SetInfoResponsePacket : smb3StandardPacket<SET_INFO_Response>
    {
    }
}

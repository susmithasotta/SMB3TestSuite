//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3EchoRequestPacket
// Description: smb3EchoRequestPacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 ECHO Request packet is sent by a client to determine whether a server is processing requests.
    /// </summary>
    public class smb3EchoRequestPacket : smb3StandardPacket<ECHO_Request>
    {
    }
}

//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3CancelRequestPacket
// Description: smb3CancelRequestPacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 CANCEL Request packet is sent by the client to cancel a previously sent 
    /// message on the same smb3 transport connection.
    /// </summary>
    public class smb3CancelRequestPacket : smb3StandardPacket<CANCEL_Request>
    {
    }
}

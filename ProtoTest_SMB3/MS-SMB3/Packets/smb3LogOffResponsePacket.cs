//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3LogOffResponsePacket
// Description: smb3LogOffResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 LOGOFF Response packet is sent by the server
    /// to confirm that an smb3 LOGOFF Request (section 2.2.7) was completed successfully
    /// </summary>
    public class smb3LogOffResponsePacket : smb3StandardPacket<LOGOFF_Response>
    {
    }
}

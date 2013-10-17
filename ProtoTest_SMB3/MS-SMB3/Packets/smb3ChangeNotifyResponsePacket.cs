//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ChangeNotifyResponsePacket
// Description: smb3ChangeNotifyResponsePacket defination.
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 CHANGE_NOTIFY Response packet is sent by the server to transmit the results 
    /// of a client's smb3 CHANGE_NOTIFY Request (section 2.2.35).
    /// </summary>
    public class smb3ChangeNotifyResponsePacket : smb3StandardPacket<CHANGE_NOTIFY_Response>, IHasInterrelatedFields
    {
        /// <summary>
        /// Update all fields which is related to other fields which has been changed
        /// </summary>
        public void UpdateInterrelatedFields()
        {
            PayLoad.OutputBufferLength = (uint)PayLoad.Buffer.Length;
        }
    }
}

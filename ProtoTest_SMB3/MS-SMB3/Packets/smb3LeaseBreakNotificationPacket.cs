//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3LeaseBreakNotificationPacket
// Description: smb3LeaseBreakNotificationPacket defination.
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 Lease Break Notification packet is sent by the server when the underlying object 
    /// store indicates that a lease is being broken, representing a change in the lease state.
    /// </summary>
    public class smb3LeaseBreakNotificationPacket : smb3StandardPacket<LEASE_BREAK_Notification_Packet>
    {
        /// <summary>
        /// Sign the message with the sessionKey
        /// </summary>
        public override void Sign()
        {
            //Pending means it is a interim response. 
            //In 3.2.5.1.2 Verifying the Signature, TD mentions that 
            //If the message is an interim response or an smb3 OPLOCK_BREAK notification, 
            //signing validation MUST NOT occur
            Header.Signature = new byte[smb3Consts.SignatureSize];
        }


        /// <summary>
        /// Verify signature to see if the signature is correct
        /// </summary>
        /// <returns>True indicates the signature is correct, otherwise false</returns>
        public override bool VerifySignature()
        {
            //In 3.2.5.1.2 Verifying the Signature, TD mentions that 
            //If the message is an interim response or an smb3 OPLOCK_BREAK notification, 
            //signing validation MUST NOT occur
            return true;
        }
    }
}

//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ErrorResponsePacket
// Description: smb3ErrorResponsePacket defination.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The smb3 ERROR Response packet is sent by the server to respond to a request that has failed or encountered an error.
    /// </summary>
    public class smb3ErrorResponsePacket : smb3StandardPacket<ERROR_Response_packet>, IHasInterrelatedFields
    {
        /// <summary>
        /// Update all fields which is related to other fields which has been changed
        /// </summary>
        public void UpdateInterrelatedFields()
        {
            PayLoad.ByteCount = (uint)PayLoad.ErrorData.Length;
        }


        /// <summary>
        /// Sign the message with the sessionKey
        /// </summary>
        public override void Sign()
        {
            if (Header.Status == (uint)smb3Status.STATUS_PENDING)
            {
                //Pending means it is a interim response. 
                //In 3.2.5.1.2 Verifying the Signature, TD mentions that 
                //If the message is an interim response or an smb3 OPLOCK_BREAK notification, 
                //signing validation MUST NOT occur
                Header.Signature = new byte[smb3Consts.SignatureSize];
            }
            else
            {
                base.Sign();
            }
        }


        /// <summary>
        /// Verify signature to see if the signature is correct
        /// </summary>
        /// <returns>True indicates the signature is correct, otherwise false</returns>
        public override bool VerifySignature()
        {
            if (Header.Status == (uint)smb3Status.STATUS_PENDING)
            {
                //Pending means it is a interim response. 
                //In 3.2.5.1.2 Verifying the Signature, TD mentions that 
                //If the message is an interim response or an smb3 OPLOCK_BREAK notification, 
                //signing validation MUST NOT occur
                return true;
            }
            else
            {
                return base.VerifySignature();
            }
        }
    }
}

//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ClientGlobalConfig
// Description: smb3ClientGlobalConfig contains parameters to config global
//              context.
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// Contains config about global context
    /// </summary>
    public class smb3ClientGlobalConfig
    {
        private bool requireMessageSigning;

        /// <summary>
        /// A Boolean that, if set, indicates that this node requires that messages MUST be signed 
        /// if the message is sent with a user security context that is neither anonymous nor guest.
        /// If not set, this node does not require that any messages be signed, but MAY still choose
        /// to do so if the other node requires it
        /// </summary>
        public bool RequireMessageSigning
        {
            get
            {
                return requireMessageSigning;
            }
            set
            {
                requireMessageSigning = value;
            }
        }
    }
}

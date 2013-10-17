//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: IHasInterrelatedFields
// Description: IHasInterrelatedFields defination.
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk
{
    /// <summary>
    /// IHasInterrelatedFields extends stackpacket, add a interface to update relatied field in stackpacket
    /// </summary>
    public interface IHasInterrelatedFields
    {
        /// <summary>
        /// Update Inter related field because some field of packet changed
        /// </summary>
        void UpdateInterrelatedFields();
    }
}

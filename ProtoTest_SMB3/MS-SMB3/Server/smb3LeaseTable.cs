﻿//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: LeaseTable
// Description: A stucture contains information about LeaseTable
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// A stucture contains information about LeaseTable
    /// </summary>
    public class smb3LeaseTable
    {
        internal Guid clientGuid;
        internal Dictionary<Guid, smb3Lease> leaseList;

        /// <summary>
        /// A global identifier to associate which connections MUST use this LeaseTable
        /// </summary>
        public Guid ClientGuid
        {
            get
            {
                return clientGuid;
            }
        }

        /// <summary>
        /// A list of lease structures, as defined in section 3.3.1.13, indexed by LeaseKey.
        /// </summary>
        public ReadOnlyDictionary<Guid, smb3Lease> LeaseList
        {
            get
            {
                if (leaseList == null)
                {
                    return null;
                }
                else
                {
                    return new ReadOnlyDictionary<Guid, smb3Lease>(leaseList);
                }
            }
        }
    }
}

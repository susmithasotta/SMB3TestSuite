//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ClientTreeConnect
// Description: A stucture contains information about treeConnect
//-------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// A stucture contains information about treeConnect
    /// </summary>
    public class smb3ClientTreeConnect
    {
        internal uint treeConnectId;
        internal Dictionary<FILEID, smb3ClientOpen> openTable;
        internal _ACCESS_MASK maximalAccess;
        internal smb3ClientSession session;
        internal string shareName;

        /// <summary>
        /// A 4-byte identifier returned by the server to identify this tree connect.
        /// </summary>
        public uint TreeConnectId
        {
            get
            {
                return treeConnectId;
            }
        }

        public ReadOnlyDictionary<FILEID, smb3ClientOpen> OpenTable
        {
            get
            {
                if (openTable == null)
                {
                    return null;
                }
                else
                {
                    return new ReadOnlyDictionary<FILEID, smb3ClientOpen>(openTable);
                }
            }
        }
        /// <summary>
        /// The maximal rights that the security principal that is described by TreeConnect.Session has on the target share.
        /// This MUST be an access mask value, as specified in section 2.2.13.1.
        /// </summary>
        public _ACCESS_MASK MaximalAccess
        {
            get
            {
                return maximalAccess;
            }
        }

        /// <summary>
        /// A reference to the session on which this tree connect was established.
        /// </summary>
        public smb3ClientSession Session
        {
            get
            {
                return session;
            }
        }

        /// <summary>
        /// The name of the share which the treeconnect established
        /// </summary>
        public string ShareName
        {
            get
            {
                return shareName;
            }
        }
    }
}

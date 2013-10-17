//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ClientFile
// Description: A stucture contains information about File
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// A stucture contains information about File
    /// </summary>
    public class smb3ClientFile
    {
        internal Dictionary<FILEID, smb3ClientOpen> openTable;
        internal byte[] leaseKey;
        internal LeaseStateValues leaseState;
        internal string serverName;
        internal string shareName;
        internal string pathName;

        /// <summary>
        /// A table of pointers to open handles to this file in the GlobalOpenTable
        /// </summary>
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
                    return new ReadOnlyDictionary<FILEID,smb3ClientOpen>(openTable);
                }
            }
        }

        /// <summary>
        /// A unique 128-bit key identifying this file on the client
        /// </summary>
        public byte[] LeaseKey
        {
            get
            {
                if (leaseKey == null)
                {
                    return null;
                }
                else
                {
                    return (byte[])leaseKey.Clone();
                }
            }
        }

        /// <summary>
        /// The lease level state granted for this file by the server as described in 2.2.13.2.8
        /// </summary>
        public LeaseStateValues LeaseState
        {
            get
            {
                return leaseState;
            }
        }

        /// <summary>
        /// The server name of the file opened.
        /// The combination of ServerName, ShareName, PathName uniquely identifies 
        /// this file object
        /// </summary>
        public string ServerName
        {
            get
            {
                return serverName;
            }
        }

        /// <summary>
        /// The share name of the file opened.
        /// The combination of ServerName, ShareName, PathName uniquely identifies 
        /// this file object
        /// </summary>
        public string ShareName
        {
            get
            {
                return shareName;
            }
        }

        /// <summary>
        /// The path name of the file opened.
        /// The combination of ServerName, ShareName, PathName uniquely identifies 
        /// this file object
        /// </summary>
        public string PathName
        {
            get
            {
                return pathName;
            }
        }
    }
}

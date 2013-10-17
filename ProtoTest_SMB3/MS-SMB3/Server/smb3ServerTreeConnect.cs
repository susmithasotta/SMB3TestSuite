//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ServerTreeConnect
// Description: A stucture contains information about tree connect
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// A stucture contains information about tree connect
    /// </summary>
    public class smb3ServerTreeConnect
    {
        internal uint treeId;
        internal smb3ServerSession session;
        internal smb3ServerShare share;
        internal int openCount;

        /// <summary>
        /// A numeric value that uniquely identifies a tree connect within the scope of the session over 
        /// which it was established. This value is typically represented as a 32-bit TreeId in the smb3 header.
        /// </summary>
        public uint TreeId
        {
            get
            {
                return treeId;
            }
        }

        /// <summary>
        /// A pointer to the authenticated session that established this tree connect.
        /// </summary>
        public smb3ServerSession Session
        {
            get
            {
                return session;
            }
        }

        /// <summary>
        /// A pointer to the share that this tree connect was established for.
        /// </summary>
        public smb3ServerShare Share
        {
            get
            {
                return share;
            }
        }

        /// <summary>
        /// A numeric value that indicates the number of files that are currently opened on TreeConnect.
        /// </summary>
        public int OpenCount
        {
            get
            {
                return openCount;
            }
        }
    }
}

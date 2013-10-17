//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ClientSession
// Description: A stucture contains information about session
//-------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// A stucture contains information about session
    /// </summary>
    public class smb3ClientSession
    {
        internal ulong sessionId;
        internal Dictionary<uint, smb3ClientTreeConnect> treeConnectTable;
        internal byte[] sessionKey;
        internal bool shouldSign;
        internal smb3ClientConnection connection;
        internal byte[] securityContext;

        /// <summary>
        /// An 8-byte identifier returned by the server to identify this session on this smb3 transport connection.
        /// </summary>
        public ulong SessionId
        {
            get
            {
                return sessionId;
            }
        }

        /// <summary>
        /// A table of tree connects, as specified in section 3.2.1.6.
        /// The table MUST allow lookup by both TreeConnect.TreeConnectId and by share name
        /// </summary>
        public ReadOnlyDictionary<uint, smb3ClientTreeConnect> TreeConnectTable
        {
            get
            {
                if (treeConnectTable == null)
                {
                    return null;
                }
                else
                {
                    return new ReadOnlyDictionary<uint,smb3ClientTreeConnect>(treeConnectTable);
                }
            }
        }

        /// <summary>
        /// The first 16 bytes of the cryptographic key for this authenticated context.
        /// If the cryptographic key is less than 16 bytes, it is right-padded with zero bytes.
        /// </summary>
        public byte[] SessionKey
        {
            get
            {
                if (sessionKey == null)
                {
                    return null;
                }
                else
                {
                    return (byte[])sessionKey.Clone();
                }
            }
        }

        /// <summary>
        /// A Boolean that indicates whether this session MUST sign communication if signing is enabled on this connection.
        /// </summary>
        public bool ShouldSign
        {
            get
            {
                return shouldSign;
            }
        }

        /// <summary>
        /// A reference to the connection on which this session was established.
        /// </summary>
        public smb3ClientConnection Connection
        {
            get
            {
                return connection;
            }
        }

        /// <summary>
        /// An OS-specific security context of the user who initiated the session.
        /// </summary>
        public byte[] SecurityContext
        {
            get
            {
                if (securityContext == null)
                {
                    return null;
                }
                else
                {
                    return (byte[])securityContext.Clone();
                }
            }
        }
    }
}

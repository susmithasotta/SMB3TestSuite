//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ServerSession
// Description: A stucture contains information about session
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The current activity state of this session
    /// </summary>
    public enum SessionState
    {
        /// <summary>
        /// InProgress
        /// </summary>
        InProgress,

        /// <summary>
        /// Valid
        /// </summary>
        Valid,

        /// <summary>
        /// Expired
        /// </summary>
        Expired
    }

    /// <summary>
    /// A stucture contains information about session
    /// </summary>
    public class smb3ServerSession
    {
        internal ulong sessionId;
        internal smb3ServerConnection connection;
        internal SessionState state;
        internal byte[] securityContext;
        internal byte[] sessionKey;
        internal bool shouldSign;
        internal Dictionary<FILEID, smb3ServerOpen> openTable;
        internal Dictionary<uint, smb3ServerTreeConnect> treeConnectTable;
        internal DateTime expirationTime;

        /// <summary>
        /// A numeric value that is used as an index in GlobalSessionTable,
        /// and (transformed into a 64-bit number) is typically sent to clients as the SessionId in the smb3 header
        /// </summary>
        public ulong SessionId
        {
            get
            {
                return sessionId;
            }
        }

        /// <summary>
        /// The connection on which this session was established (see also sections 3.3.5.5.1 and 3.3.4.4).
        /// </summary>
        public smb3ServerConnection Connection
        {
            get
            {
                return connection;
            }
        }

        /// <summary>
        /// The current activity state of this session. This value MUST be either InProgress, Valid, or Expired.
        /// </summary>
        public SessionState State
        {
            get
            {
                return state;
            }
        }

        /// <summary>
        /// The security context of the user that authenticated this session.
        /// This value MUST be in a form that allows for evaluating security descriptors within the server, 
        /// as well as being passed to the underlying object store to handle security evaluation that may happen there
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

        /// <summary>
        /// The first 16 bytes of the cryptographic key for this authenticated context. 
        /// If the cryptographic key is less than 16 bytes, it is right-padded with zero bytes
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
        /// A Boolean that, if set, indicates that this session MUST sign communication if signing is
        /// enabled on this connection
        /// </summary>
        public bool ShouldSign
        {
            get
            {
                return shouldSign;
            }
        }

        /// <summary>
        /// A table of opens of files or named pipes, as specified in section 3.3.1.11, 
        /// that have been opened by this authenticated session and indexed by Open.FileId.
        /// </summary>
        public ReadOnlyDictionary<FILEID, smb3ServerOpen> OpenTable
        {
            get
            {
                if (openTable == null)
                {
                    return null;
                }
                else
                {
                    return new ReadOnlyDictionary<FILEID, smb3ServerOpen>(openTable);
                }
            }
        }

        /// <summary>
        /// A table of tree connects that have been established by this authenticated session to shares on this server, 
        /// indexed by TreeConnect.TreeId
        /// </summary>
        public ReadOnlyDictionary<uint, smb3ServerTreeConnect> TreeConnectTable
        {
            get
            {
                if (treeConnectTable == null)
                {
                    return null;
                }
                else
                {
                    return new ReadOnlyDictionary<uint, smb3ServerTreeConnect>(treeConnectTable);
                }
            }
        }

        /// <summary>
        /// A value that specifies the time after which the client must reauthenticate with the server.
        /// </summary>
        public DateTime ExpirationTime
        {
            get
            {
                return expirationTime;
            }
        }
    }
}

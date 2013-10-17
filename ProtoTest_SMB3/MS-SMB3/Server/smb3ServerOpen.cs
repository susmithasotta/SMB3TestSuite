//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ServerOpen
// Description: A stucture contains information about open
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// Oplock state
    /// </summary>
    public enum OplockState
    {
        /// <summary>
        /// Held
        /// </summary>
        Held,

        /// <summary>
        /// Breaking
        /// </summary>
        Breaking,

        /// <summary>
        /// None
        /// </summary>
        None
    }

    /// <summary>
    /// A stucture contains information about open
    /// </summary>
    public class smb3ServerOpen
    {
        internal ulong fileId;
        internal ulong durableFileId;
        internal smb3ServerSession session;
        internal smb3ServerTreeConnect treeConnect;
        internal smb3ServerConnection connection;
        internal uint localOpen;
        internal uint grantedAccess;
        internal OplockLevel_Values oplockLevel;
        internal OplockState oplockState;
        internal TimeSpan oplockTimeout;
        internal bool isDurable;
        internal TimeSpan durableOpenTimeout;
        internal string durableOwner;
        internal int enumerationLocation;
        internal string enumerationSearchPattern;
        internal int currentEaIndex;
        internal int currentQuotaIndex;
        internal int lockCount;
        internal string pathName;

        //2.1 dialect feature
        internal smb3LeaseTable lease;
        internal bool isResilient;
        internal TimeSpan resilientOpenTimeout;
        internal byte[] lockSequenceArray;

        /// <summary>
        /// A numeric value that uniquely identifies the open handle to a file or a pipe within the scope 
        /// of a session over which the handle was opened. This value is the volatile portion of the identifier.
        /// </summary>
        public ulong FileId
        {
            get
            {
                return fileId;
            }
        }

        /// <summary>
        /// A numeric value that uniquely identifies the open handle to a file or a pipe within the scope of 
        /// all opens granted by the server, as described by the GlobalOpenTable.
        /// </summary>
        public ulong DurableFileId
        {
            get
            {
                return durableFileId;
            }
        }

        /// <summary>
        /// A reference to the authenticated session
        /// </summary>
        public smb3ServerSession Session
        {
            get
            {
                return session;
            }
        }

        /// <summary>
        /// A reference to the TreeConnect, as specified in section 3.3.1.10
        /// </summary>
        public smb3ServerTreeConnect TreeConnect
        {
            get
            {
                return treeConnect;
            }
        }

        /// <summary>
        /// A reference to the connection, as specified in section 3.3.1.8, that created this open. 
        /// If the file is not attached to a connection at this time, this value MUST be NULL
        /// </summary>
        public smb3ServerConnection Connection
        {
            get
            {
                return connection;
            }
        }

        /// <summary>
        /// An open of a file or named pipe in the underlying local resource that is used to perform the local operations,
        /// such as reading or writing, to the underlying object.
        /// </summary>
        public uint LocalOpen
        {
            get
            {
                return localOpen;
            }
        }

        /// <summary>
        /// The access granted on this open, as defined in section 2.2.13.1.
        /// </summary>
        public uint GrantedAccess
        {
            get
            {
                return grantedAccess;
            }
        }

        /// <summary>
        /// The current oplock level for this open. This value MUST be one of the OplockLevel values defined in section 2.2.14:
        /// SMB2_OPLOCK_LEVEL_NONE, SMB2_OPLOCK_LEVEL_II, SMB2_OPLOCK_LEVEL_EXCLUSIVE, SMB2_OPLOCK_LEVEL_BATCH, or OPLOCK_LEVEL_LEASE.
        /// </summary>
        public OplockLevel_Values OplockLevel
        {
            get
            {
                return oplockLevel;
            }
        }

        /// <summary>
        /// The current oplock state of the file. This value MUST be Held, Breaking, or None.
        /// </summary>
        public OplockState OplockState
        {
            get
            {
                return oplockState;
            }
        }

        /// <summary>
        /// The time-out value that indicates when an oplock that is breaking and has not received an acknowledgment
        /// from the client will be acknowledged by the server.
        /// </summary>
        public TimeSpan OplockTimeout
        {
            get
            {
                return oplockTimeout;
            }
        }

        /// <summary>
        /// A Boolean that indicates whether this open has requested durable operation.
        /// </summary>
        public bool IsDurable
        {
            get
            {
                return isDurable;
            }
        }

        /// <summary>
        /// A time-out value that indicates when a handle that has been preserved for durability will be closed 
        /// by the system if a client has not reclaimed it.
        /// </summary>
        public TimeSpan DurableOpenTimeout
        {
            get
            {
                return durableOpenTimeout;
            }
        }

        /// <summary>
        /// A security descriptor that holds the original opener of the file.
        /// </summary>
        public string DurableOwner
        {
            get
            {
                return durableOwner;
            }
        }

        /// <summary>
        /// For directories, this value indicates the current location in a directory enumeration 
        /// and allows for the continuing of an enumeration across multiple requests.
        /// </summary>
        public int EnumerationLocation
        {
            get
            {
                return enumerationLocation;
            }
        }

        /// <summary>
        /// For directories, this value holds the search pattern that is used in directory enumeration and allows 
        /// for the continuing of an enumeration across multiple requests. For files, this value is unused.
        /// </summary>
        public string EnumerationSearchPattern
        {
            get
            {
                return enumerationSearchPattern;
            }
        }

        /// <summary>
        /// For extended attribute information, this value indicates the current location in an extended attribute 
        /// information list and allows for the continuing of an enumeration across multiple requests.
        /// </summary>
        public int CurrentEaIndex
        {
            get
            {
                return currentEaIndex;
            }
        }

        /// <summary>
        /// For quota queries, this value indicates the current index in the quota information list 
        /// and allows for the continuation of an enumeration across multiple requests.
        /// </summary>
        public int CurrentQuotaIndex
        {
            get
            {
                return currentQuotaIndex;
            }
        }

        /// <summary>
        /// A numeric value that indicates the number of locks that are held by current open.
        /// </summary>
        public int LockCount
        {
            get
            {
                return lockCount;
            }
        }

        /// <summary>
        /// A variable-length string that contains the Unicode path name that the open is performed on.
        /// </summary>
        public string PathName
        {
            get
            {
                return pathName;
            }
        }

        /// <summary>
        /// The lease associated with this open, as defined in 3.3.1.12. 
        /// This value MUST point to a valid lease, or be set to NULL.
        /// </summary>
        public smb3LeaseTable Lease
        {
            get
            {
                return lease;
            }
        }

        /// <summary>
        /// A Boolean that indicates whether this open has requested resilient operation.
        /// </summary>
        public bool IsResilient
        {
            get
            {
                return isResilient;
            }
        }

        /// <summary>
        /// A time-out value that indicates when a handle that has been preserved for resiliency will be closed 
        /// by the system if a client has not reclaimed it.
        /// </summary>
        public TimeSpan ResilientOpenTimeout
        {
            get
            {
                return resilientOpenTimeout;
            }
        }

        /// <summary>
        /// An array of lock sequence entries (1 byte) that have been successfully processed by the server for resilient opens.
        /// </summary>
        public byte[] LockSequenceArray
        {
            get
            {
                if (lockSequenceArray == null)
                {
                    return null;
                }
                else
                {
                    return (byte[])lockSequenceArray.Clone();
                }
            }
        }
    }
}

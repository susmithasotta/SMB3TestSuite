//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ClientConnection
// Description: A stucture contains information about one connection
//-------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using Microsoft.Protocols.TestTools.StackSdk.Security.Nlmp;
using Microsoft.Protocols.TestTools.StackSdk.Security.Sspi;


namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// Contains information to track one connection between server and client
    /// </summary>
    public class smb3ClientConnection : IDisposable
    {
        internal Dictionary<ulong, smb3ClientSession> sessionTable;
        internal Dictionary<ulong, smb3OutStandingRequest> outstandingRequests;
        internal List<ulong> sequenceWindow;
        internal Dictionary<FILEID, smb3ClientOpen> openTable;
        internal byte[] gssNegotiateToken;
        internal uint maxTransactSize;
        internal uint maxReadSize;
        internal uint maxWriteSize;
        internal Guid serverGuid;
        internal bool requireSigning;

        //SMB 2.1 supported field
        internal string dialect;
        internal bool supportLeasing;
        internal bool supportLargeMtu;

        //The next sequenceNumber will be added to sequenceWindow
        private ulong allocatedSequnceNum;

        private ClientSecurityContext gss;
        internal Dictionary<ulong, smb3SessionSetupResponsePacket> sessionResponses;

        internal string serverName;
        //just a copy of requireMessageSign in createContext.
        //once negotiate request packet has read this value. it should be kept
        //unchanged.
        internal bool globalRequireMessageSignCopy;

        internal smb3TransportType transportType;

        private bool disposed;

        /// <summary>
        /// A table of authenticated sessions, as specified in section 3.2.1.5, 
        /// that the client has established on this smb3 transport connection
        /// </summary>
        public ReadOnlyDictionary<ulong, smb3ClientSession> SessionTable
        {
            get
            {
                if (sessionTable == null)
                {
                    return null;
                }
                else
                {
                    return new ReadOnlyDictionary<ulong,smb3ClientSession>(sessionTable);
                }
            }
        }

        /// <summary>
        /// A table of requests that have been issued on this connection and are awaiting a response
        /// </summary>
        public ReadOnlyDictionary<ulong, smb3OutStandingRequest> OutstandingRequests
        {
            get
            {
                if (outstandingRequests == null)
                {
                    return null;
                }
                else
                {
                    return new ReadOnlyDictionary<ulong,smb3OutStandingRequest>(outstandingRequests);
                }
            }
        }

        /// <summary>
        /// A table of available sequence numbers for sending requests to the server, as specified in section 3.2.1.1
        /// </summary>
        public ReadOnlyCollection<ulong> SequenceWindow
        {
            get
            {
                if (sequenceWindow != null)
                {
                    return sequenceWindow.AsReadOnly();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// A table of opens, as specified in section 3.2.1.8.
        /// The table MUST allow lookup by either file name or by Open.FileId.
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
        /// A byte array containing the token received during a negotiation and remembered for authentication
        /// </summary>
        public byte[] GssNegotiateToken
        {
            get
            {
                if (gssNegotiateToken == null)
                {
                    return null;
                }
                else
                {
                    return (byte[])gssNegotiateToken.Clone();
                }
            }
        }

        /// <summary>
        /// The maximum buffer size, in bytes, that the server will accept on this connection for QUERY_INFO,
        /// QUERY_DIRECTORY, SET_INFO and CHANGE_NOTIFY operations
        /// </summary>
        public uint MaxTransactSize
        {
            get
            {
                return maxTransactSize;
            }
        }

        /// <summary>
        /// The maximum read size, in bytes, that the server will accept in an smb3 READ Request on this connection
        /// </summary>
        public uint MaxReadSize
        {
            get
            {
                return maxReadSize;
            }
        }

        /// <summary>
        /// The maximum write size, in bytes, that the server will accept in an smb3 WRITE Request on this connection
        /// </summary>
        public uint MaxWriteSize
        {
            get
            {
                return maxWriteSize;
            }
        }

        /// <summary>
        /// A globally unique identifier that is generated by the remote server to uniquely identify the remote server
        /// </summary>
        public Guid ServerGuid
        {
            get
            {
                return serverGuid;
            }
        }

        /// <summary>
        /// A Boolean indicating whether the server requires all requests/responses on this connection to be signed
        /// </summary>
        public bool RequireSigning
        {
            get
            {
                return requireSigning;
            }
        }

        /// <summary>
        /// The dialect of smb3 negotiated with the server. This value MUST be either "2.002", "2.100", or "Unknown"
        /// </summary>
        public string Dialect
        {
            get
            {
                return dialect;
            }
        }

        /// <summary>
        /// A Boolean indicating whether the server supports leasing functionality
        /// </summary>
        public bool SupportLeasing
        {
            get
            {
                return supportLeasing;
            }
        }

        /// <summary>
        /// The next sequence number can be used
        /// </summary>
        internal ulong GetNextSequenceNumber()
        {
            if (sequenceWindow.Count > 0)
            {
                ulong nextSequnceNumber = sequenceWindow[0];

                return nextSequnceNumber;
            }
            //if (sequenceWindow.Count == 0)
            //{

            //    return 5;

            //}

            else
            {
                return ulong.MaxValue;
            }
        }

        /// <summary>
        /// The underlying gss-api
        /// </summary>
        internal ClientSecurityContext Gss
        {
            get
            {
                return gss;
            }
            set
            {
                gss = value;
            }
        }

        /// <summary>
        /// Grand credit to client, the sequence window will be modified
        /// </summary>
        /// <param name="credit">The credit server granded to server</param>
        internal void GrandCredit(int credit)
        {
            if (credit < 0)
            {
                throw new ArgumentOutOfRangeException("credit", "credit must be larger or equal to 0.");
            }

            for (int i = 0; i < credit; i++)
            {
                sequenceWindow.Add(allocatedSequnceNum++);
            }
        }

        /// <summary>
        /// The name of server which the connection established
        /// </summary>
        public string ServerName
        {
            get
            {
                return serverName;
            }
        }


        /// <summary>
        /// Release Sspi resources
        /// </summary>
        internal void ReleaseSspiClient()
        {
            if (gss != null)
            {
                if (gss is SspiClientSecurityContext)
                {
                    (gss as SspiClientSecurityContext).Dispose();
                }

                gss = null;
            }
        }

        #region Implement IDispose interface

        /// <summary>
        /// Release all resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Release all resources
        /// </summary>
        /// <param name="disposing">Indicate if calling this function mannually</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free managed resources & other reference types
                    ReleaseSspiClient();
                }

                // Call the appropriate methods to clean up unmanaged resources.
                // If disposing is false, only the following code is executed.

                disposed = true;
            }
        }


        /// <summary>
        /// Deconstructor
        /// </summary>
        ~smb3ClientConnection()
        {
            Dispose(false);
        }

        #endregion
    }
}

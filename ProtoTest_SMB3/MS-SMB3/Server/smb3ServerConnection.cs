//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3ServerConnection
// Description: A stucture contains information about connection
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Protocols.TestTools.StackSdk.Security.Sspi;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The request which is handled async.
    /// </summary>
    public class AsyncCommand
    {
        internal ulong asyncId;
        internal smb3Packet requestPacket;

        /// <summary>
        /// The asyncId server granted.
        /// </summary>
        public ulong AsyncId
        {
            get
            {
                return asyncId;
            }
        }

        /// <summary>
        /// The request packet
        /// </summary>
        public smb3Packet RequestPacket
        {
            get
            {
                return requestPacket;
            }
        }
    }

    /// <summary>
    /// A stucture contains information about connection
    /// </summary>
    public class smb3ServerConnection : IDisposable
    {
        internal List<ulong> commandSequenceWindow;
        internal Dictionary<ulong, smb3Packet> requestList;
        internal SESSION_SETUP_Request_Capabilities_Values clientCapabilities;
        internal uint negotiateDialect;
        internal Dictionary<ulong, AsyncCommand> asyncCommandList;
        internal string dialect;
        internal bool shouldSign;
        internal Guid clientGuid;
        internal string clientName;
        internal int connectionId;

        //for adding sequence number
        internal ulong sequenceId;
        internal AccountCredential credential;
        internal SecurityPackageType packageType;
        internal ServerSecurityContextAttribute contextAttribute;
        internal ServerSecurityContext gss;
        private bool disposed;

        /// <summary>
        /// A list of the sequence numbers that are valid to receive from the client at this time.
        /// </summary>
        public ReadOnlyCollection<ulong> CommandSequenceWindow
        {
            get
            {
                if (commandSequenceWindow != null)
                {
                    return commandSequenceWindow.AsReadOnly();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// A list of all client requests being processed. Each request MUST include a Boolean value that 
        /// indicates whether it is being handled asynchronously
        /// </summary>
        public ReadOnlyDictionary<ulong, smb3Packet> RequestList
        {
            get
            {
                if (requestList == null)
                {
                    return null;
                }
                else
                {
                    return new ReadOnlyDictionary<ulong, smb3Packet>(requestList);
                }
            }
        }

        /// <summary>
        /// The capabilities of the client of this connection in a form that MUST follow the syntax as specified in section 2.2.5.
        /// </summary>
        public SESSION_SETUP_Request_Capabilities_Values ClientCapabilities
        {
            get
            {
                return clientCapabilities;
            }
        }

        /// <summary>
        /// A numeric value representing the current state of dialect negotiation between the client and server on this transport connection
        /// </summary>
        public uint NegotiateDialect
        {
            get
            {
                return negotiateDialect;
            }
        }

        /// <summary>
        /// A list of client requests being handled asynchronously. Each request MUST have been assigned an AsyncId
        /// </summary>
        public ReadOnlyDictionary<ulong, AsyncCommand> AsyncCommandList
        {
            get
            {
                if (asyncCommandList == null)
                {
                    return null;
                }
                else
                {
                    return new ReadOnlyDictionary<ulong, AsyncCommand>(asyncCommandList);
                }
            }
        }

        /// <summary>
        /// The dialect of smb3 negotiated with the client. This value MUST be either "2.002", "2.100", or "Unknown".
        /// </summary>
        public string Dialect
        {
            get
            {
                return dialect;
            }
        }

        /// <summary>
        /// A Boolean that, if set, indicates that all sessions on this connection MUST have signing enabled.
        /// </summary>
        public bool ShouldSign
        {
            get
            {
                return shouldSign;
            }
        }

        /// <summary>
        /// An identifier for the client machine.
        /// </summary>
        public Guid ClientGuid
        {
            get
            {
                return clientGuid;
            }
        }

        /// <summary>
        /// A null-terminated Unicode UTF-16IP address string, or NetBIOS host name of the client machine.
        /// </summary>
        public string ClientName
        {
            get
            {
                return clientName;
            }
        }


        /// <summary>
        /// Increase sequence window based one the credit server granted.
        /// </summary>
        /// <param name="credit">The credit server granted</param>
        internal void GrandCredit(int credit)
        {
            if (credit < 0)
            {
                throw new ArgumentOutOfRangeException("credit", "credit should not less than 0.");
            }

            for (int i = 0; i < credit; i++)
            {
                commandSequenceWindow.Add(sequenceId++);

                if (sequenceId == 0)
                {
                    throw new InvalidOperationException("The 64-bit sequence wraps, the connection MUST be terminated");
                }
            }
        }


        /// <summary>
        /// Remove messageId from sequence window
        /// </summary>
        /// <param name="messageId">the messageId</param>
        internal void RemoveMessageId(ulong messageId)
        {
            commandSequenceWindow.Remove(messageId);
        }


        /// <summary>
        /// Release Sspi resources
        /// </summary>
        internal void ReleaseSspiServer()
        {
            if (gss != null)
            {
                if (gss is SspiServerSecurityContext)
                {
                    (gss as SspiServerSecurityContext).Dispose();
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
                // Free managed resources & other reference types
                if (disposing)
                {
                    ReleaseSspiServer();
                }

                // Call the appropriate methods to clean up unmanaged resources.
                // If disposing is false, only the following code is executed.

                disposed = true;
            }
        }


        /// <summary>
        /// Deconstructor
        /// </summary>
        ~smb3ServerConnection()
        {
            Dispose(false);
        }

        #endregion
    }
}

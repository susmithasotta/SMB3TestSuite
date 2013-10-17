//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile
{
    /// <summary>
    /// Base class of KileClientContext and KileServerContext
    /// Maintain the important parameters during KILE transport.
    /// </summary>
    public abstract class KileContext
    {
        #region Members

        /// <summary>
        /// The connection type, TCP or UDP.
        /// </summary>
        protected KileConnectionType transportType;

        /// <summary>
        /// Password of the logon user.
        /// </summary>
        protected string password;

        /// <summary>
        /// The principle name of client. Get from AS request.
        /// </summary>
        protected PrincipalName cName;

        /// <summary>
        /// The realm of client. Get from AS request.
        /// </summary>
        protected Realm cRealm;

        /// <summary>
        /// The salt used in encryption and checksum.
        /// </summary>
        protected string salt;

        /// <summary>
        /// The session key returned by AS response.
        /// </summary>
        protected EncryptionKey tgsSessionKey;

        /// <summary>
        /// Whether the sender is the security context initiator.
        /// </summary>
        internal bool isInitiator;

        /// <summary>
        /// Current local sequence number
        /// </summary>
        protected internal ulong currentLocalSequenceNumber;

        /// <summary>
        /// Current remote sequence number
        /// While act as client, this represents server's.
        /// While act as server, this represents client's.
        /// </summary>
        protected internal ulong currentRemoteSequenceNumber;

        /// <summary>
        /// The checksum flag of Authenticator in AP request.
        /// </summary>
        protected ChecksumFlags checksumFlag;

        /// <summary>
        /// The sub key returned by AP response.
        /// </summary>
        protected EncryptionKey acceptorSubKey;

        /// <summary>
        /// The sub key created by AP request.
        /// </summary>
        protected EncryptionKey apSubKey;

        /// <summary>
        /// The session key returned by TGS response.
        /// </summary>
        protected EncryptionKey apSessionKey;

        /// <summary>
        /// The current time on the client's host. Created by AP request.
        /// </summary>
        protected KerberosTime apRequestCtime;

        /// <summary>
        /// The microsecond part of the client's timestamp. Created by AP request.
        /// </summary>
        protected Microseconds apRequestCusec;

        #endregion

        #region Properties

        /// <summary>
        /// The connection type, TCP or UDP.
        /// </summary>
        public KileConnectionType TransportType
        {
            get
            {
                return transportType;
            }
            set
            {
                transportType = value;
            }
        }


        /// <summary>
        /// Password of the logon user.
        /// </summary>
        public string Password
        {
            get
            {
                return password;
            }
            internal set
            {
                password = value;
            }
        }


        /// <summary>
        /// User Principle Name (UPN)
        /// </summary>
        public PrincipalName UserName
        {
            get
            {
                return cName;
            }
        }


        /// <summary>
        /// The domain name which the user belongs to
        /// </summary>
        public Realm UserRealm
        {
            get
            {
                return cRealm;
            }
        }


        /// <summary>
        /// The salt used in encryption and checksum
        /// </summary>
        public string Salt
        {
            get
            {
                return salt;
            }
            set
            {
                salt = value;
            }
        }


        /// <summary>
        /// TGS Session Key received from AS response
        /// </summary>
        public EncryptionKey TgsSessionKey
        {
            get
            {
                return tgsSessionKey;
            }
            set
            {
                tgsSessionKey = value;
            }
        }


        /// <summary>
        /// The key currently used.
        /// </summary>
        public EncryptionKey ContextKey
        {
            get
            {
                return acceptorSubKey ?? apSubKey ?? apSessionKey;
            }
        }


        /// <summary>
        /// The sub key returned by AP response.
        /// </summary>
        public EncryptionKey AcceptorSubKey
        {
            get
            {
                return acceptorSubKey;
            }
        }


        /// <summary>
        /// The sub key created by AP request.
        /// </summary>
        public EncryptionKey ApSubKey
        {
            get
            {
                return apSubKey;
            }
        }


        /// <summary>
        /// AP Session Key received from TGS Response
        /// </summary>
        public EncryptionKey ApSessionKey
        {
            get
            {
                return apSessionKey;
            }
        }


        /// <summary>
        /// The local message sequence number
        /// </summary>
        public ulong CurrentLocalSequenceNumber
        {
            get
            {
                return currentLocalSequenceNumber;
            }
        }


        /// <summary>
        /// Current remote sequence number
        /// While act as client, this represents server's.
        /// While act as server, this represents client's.
        /// </summary>
        public ulong CurrentRemoteSequenceNumber
        {
            get
            {
                if ((checksumFlag & ChecksumFlags.GSS_C_SEQUENCE_FLAG) == ChecksumFlags.GSS_C_SEQUENCE_FLAG)
                {
                    return currentRemoteSequenceNumber;
                }
                else
                {
                    return 0;
                }
            }
        }


        /// <summary>
        /// TimeStamp: ctime
        /// Get from AP request
        /// </summary>
        public KerberosTime Time
        {
            get
            {
                return apRequestCtime;
            }
        }


        /// <summary>
        /// TimeStamp: cusec
        /// Get from AP request
        /// </summary>
        public Microseconds Cusec
        {
            get
            {
                return apRequestCusec;
            }
        }


        /// <summary>
        /// The checksum flag of Authenticator in AP request.
        /// </summary>
        public ChecksumFlags ChecksumFlag
        {
            get
            {
                return checksumFlag;
            }
        }

        #endregion


        #region Methods

        /// <summary>
        /// Update the context.
        /// </summary>
        /// <param name="pdu">The PDU to update the context.</param>
        internal abstract void UpdateContext(KilePdu pdu);


        /// <summary>
        /// Increase currentLocalSequenceNumber by 1.
        /// </summary>
        internal void IncreaseLocalSequenceNumber()
        {
            currentLocalSequenceNumber++;
        }


        /// <summary>
        /// Increase currentRemoteSequenceNumber by 1.
        /// </summary>
        internal void IncreaseRemoteSequenceNumber()
        {
            if ((checksumFlag & ChecksumFlags.GSS_C_SEQUENCE_FLAG) == ChecksumFlags.GSS_C_SEQUENCE_FLAG)
            {
                currentRemoteSequenceNumber++;
            }
        }

        #endregion
    }
}
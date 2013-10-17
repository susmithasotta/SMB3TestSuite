//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Description: This file implements KILE client context, which maintains the   
//              important parameters during KILE transport.
//
//------------------------------------------------------------------------------

using System;

using Com.Objsys.Asn1.Runtime;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile
{
    /// <summary>
    /// Maintain the important parameters during KILE transport, including the main sent or received PDUs, 
    /// TGS session key, application session key, checksum algorithm etc. It is called by KileClient,
    /// KilePdu and KileDecoder.
    /// </summary>
    public class KileClientContext : KileContext
    {
        #region private members
        /// <summary>
        /// The lock of the context.
        /// </summary>
        private object contextLock;

        /// <summary>
        /// The key of the ticket returned by AS response.
        /// </summary>
        private byte[] asTgtKey;

        /// <summary>
        /// The encryption types supported by client. Get from AS request
        /// </summary>
        private _SeqOfInt32 eType;

        /// <summary>
        /// The ticket returned from AS response.
        /// </summary>
        private Ticket tgsTicket;

        /// <summary>
        /// The ticket returned by TGS response.
        /// </summary>
        private Ticket apTicket;

        #endregion private members


        #region properties

        /// <summary>
        /// The TGT Key used to decrypt AS response Ticket.
        /// </summary>
        public byte[] AsTgtKey
        {
            get
            {
                lock (contextLock)
                {
                    return (asTgtKey == null) ? null : (byte[])asTgtKey.Clone();
                }
            }
            set
            {
                lock (contextLock)
                {
                    asTgtKey = (value == null) ? null : (byte[])value.Clone();
                }
            }
        }


        /// <summary>
        /// The encryption types supported by client.
        /// </summary>
        public _SeqOfInt32 ClientEncryptionTypes
        {
            get
            {
                lock (contextLock)
                {
                    return eType;
                }
            }
        }


        /// <summary>
        /// TGS ticket received from AS response
        /// </summary>
        public Ticket TgsTicket
        {
            get
            {
                return tgsTicket;
            }
            set
            {
                tgsTicket = value;
            }
        }


        /// <summary>
        /// AP Ticket received from TGS response
        /// </summary>
        public Ticket ApTicket
        {
            get
            {
                lock (contextLock)
                {
                    return apTicket;
                }
            }
        }

        #endregion properties


        #region constructor

        /// <summary>
        /// Create a KileClientContext instance.
        /// </summary>
        internal KileClientContext()
        {
            contextLock = new object();
            isInitiator = true;
        }
        #endregion constructor


        /// <summary>
        /// Update the context.
        /// </summary>
        /// <param name="pdu">The PDU to update the context.</param>
        internal override void UpdateContext(KilePdu pdu)
        {
            if (pdu == null)
            {
                return;
            }

            lock (contextLock)
            {
                Type pduType = pdu.GetType();

                if (pduType == typeof(KileAsRequest))
                {
                    KileAsRequest request = (KileAsRequest)pdu;
                    if (request.Request != null && request.Request.req_body != null)
                    {
                        cName = request.Request.req_body.cname;
                        cRealm = request.Request.req_body.realm;
                        eType = request.Request.req_body.etype;
                    }
                }
                else if (pduType == typeof(KileAsResponse))
                {
                    KileAsResponse response = (KileAsResponse)pdu;
                    if (response.EncPart != null)
                    {
                        tgsSessionKey = response.EncPart.key;
                    }

                    if (response.Response != null)
                    {
                        tgsTicket = response.Response.ticket;
                        if (tgsTicket != null && tgsTicket.sname != null
                            && tgsTicket.sname.name_string != null && tgsTicket.sname.name_string.elements != null
                            && tgsTicket.sname.name_string.elements.Length > 1)
                        {
                            int count = tgsTicket.sname.name_string.elements.Length;
                            cRealm = new Realm(tgsTicket.sname.name_string.elements[count - 1].mValue);
                        }

                        if (response.Response.padata != null && response.Response.padata.elements != null)
                        {
                            foreach (PA_DATA paData in response.Response.padata.elements)
                            {
                                if (paData.padata_type != null
                                    && paData.padata_type.mValue == (long)PaDataType.PA_ETYPE_INFO2)
                                {
                                    Asn1BerDecodeBuffer buffer = new Asn1BerDecodeBuffer(paData.padata_value.mValue);
                                    ETYPE_INFO2 eTypeInfo2 = new ETYPE_INFO2();
                                    eTypeInfo2.Decode(buffer);
                                    if (eTypeInfo2.elements != null && eTypeInfo2.elements.Length > 0)
                                    {
                                        // the salt is received from KDC
                                        salt = eTypeInfo2.elements[0].salt.mValue;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (pduType == typeof(KileTgsResponse))
                {
                    KileTgsResponse response = (KileTgsResponse)pdu;
                    if (response.Response != null)
                    {
                        apTicket = response.Response.ticket;
                        if (apTicket != null && apTicket.sname != null
                            && apTicket.sname.name_string != null && apTicket.sname.name_string.elements != null
                            && apTicket.sname.name_string.elements.Length > 1)
                        {
                            int count = apTicket.sname.name_string.elements.Length;
                            cRealm = new Realm(apTicket.sname.name_string.elements[count - 1].mValue);
                        }
                    }

                    if (response.EncPart != null)
                    {
                        apSessionKey = response.EncPart.key;
                    }
                }
                else if (pduType == typeof(KileApRequest))
                {
                    KileApRequest request = (KileApRequest)pdu;
                    if (request.Authenticator != null)
                    {
                        apSubKey = request.Authenticator.subkey;
                        apRequestCtime = request.Authenticator.ctime;
                        apRequestCusec = request.Authenticator.cusec;

                        if (request.Authenticator.cksum != null
                            && request.Authenticator.cksum.cksumtype.mValue == (int)ChecksumType.ap_authenticator_8003
                            && request.Authenticator.cksum.checksum != null
                            && request.Authenticator.cksum.checksum.mValue != null
                            && request.Authenticator.cksum.checksum.mValue.Length == ConstValue.AUTH_CHECKSUM_SIZE)
                        {
                            int flag = BitConverter.ToInt32(request.Authenticator.cksum.checksum.mValue,
                                ConstValue.AUTHENTICATOR_CHECKSUM_LENGTH + sizeof(int));
                            checksumFlag = (ChecksumFlags)flag;
                        }

                        if (request.Authenticator.seq_number != null)
                        {
                            currentLocalSequenceNumber = (ulong)request.Authenticator.seq_number.mValue;
                            currentRemoteSequenceNumber = currentLocalSequenceNumber;
                        }
                    }
                }
                else if (pduType == typeof(KileApResponse))
                {
                    KileApResponse response = (KileApResponse)pdu;
                    if (response.ApEncPart != null)
                    {
                        if (response.ApEncPart.seq_number != null)
                        {
                            currentRemoteSequenceNumber = (ulong)response.ApEncPart.seq_number.mValue;
                        }

                        if (response.ApEncPart.subkey != null)
                        {
                            acceptorSubKey = response.ApEncPart.subkey;
                        }
                    }
                }
                // else do nothing
            }
        }
    }
}
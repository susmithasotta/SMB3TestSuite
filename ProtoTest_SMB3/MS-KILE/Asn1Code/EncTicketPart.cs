// 
// This file was generated by the Objective Systems ASN1C Compiler
// (http://www.obj-sys.com).  Version: 6.10, Date: 01-Jul-2008.
// 
using System;
using Com.Objsys.Asn1.Runtime;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile {

   public class EncTicketPart : Asn1Type {
      public new readonly static Asn1Tag _TAG = new Asn1Tag (Asn1Tag.APPL, Asn1Tag.CONS, 3);

      public TicketFlags flags;
      public EncryptionKey key;
      public Realm crealm;
      public PrincipalName cname;
      public TransitedEncoding transited;
      public KerberosTime authtime;
      public KerberosTime starttime;  // optional
      public KerberosTime endtime;
      public KerberosTime renew_till;  // optional
      public HostAddresses caddr;  // optional
      public AuthorizationData authorization_data;  // optional

      public EncTicketPart () : base()
      {
         Init();
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor sets all elements to references to the 
      /// given objects
      /// </summary>
      public EncTicketPart (
         TicketFlags flags_,
         EncryptionKey key_,
         Realm crealm_,
         PrincipalName cname_,
         TransitedEncoding transited_,
         KerberosTime authtime_,
         KerberosTime starttime_,
         KerberosTime endtime_,
         KerberosTime renew_till_,
         HostAddresses caddr_,
         AuthorizationData authorization_data_
      )
         : base ()
      {
         flags = flags_;
         key = key_;
         crealm = crealm_;
         cname = cname_;
         transited = transited_;
         authtime = authtime_;
         starttime = starttime_;
         endtime = endtime_;
         renew_till = renew_till_;
         caddr = caddr_;
         authorization_data = authorization_data_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor is for required elements only.  It sets 
      /// all elements to references to the given objects
      /// </summary>
      public EncTicketPart (
         TicketFlags flags_,
         EncryptionKey key_,
         Realm crealm_,
         PrincipalName cname_,
         TransitedEncoding transited_,
         KerberosTime authtime_,
         KerberosTime endtime_
      )
         : base ()
      {
         flags = flags_;
         key = key_;
         crealm = crealm_;
         cname = cname_;
         transited = transited_;
         authtime = authtime_;
         endtime = endtime_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor allows primitive data to be passed for all 
      /// primitive elements.  It will create new object wrappers for 
      /// the primitive data and set other elements to references to 
      /// the given objects 
      /// </summary>
      public EncTicketPart (TicketFlags flags_,
         EncryptionKey key_,
         string crealm_,
         PrincipalName cname_,
         TransitedEncoding transited_,
         string authtime_,
         string starttime_,
         string endtime_,
         string renew_till_,
         HostAddresses caddr_,
         AuthorizationData authorization_data_
      )
         : base ()
      {
         flags = flags_;
         key = key_;
         crealm = new Realm (crealm_);
         cname = cname_;
         transited = transited_;
         authtime = new KerberosTime (authtime_);
         starttime = new KerberosTime (starttime_);
         endtime = new KerberosTime (endtime_);
         renew_till = new KerberosTime (renew_till_);
         caddr = caddr_;
         authorization_data = authorization_data_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor is for required elements only.  It allows 
      /// primitive data to be passed for all primitive elements.  
      /// It will create new object wrappers for the primitive data 
      /// and set other elements to references to the given objects. 
      /// </summary>
      public EncTicketPart (
         TicketFlags flags_,
         EncryptionKey key_,
         string crealm_,
         PrincipalName cname_,
         TransitedEncoding transited_,
         string authtime_,
         string endtime_
      )
         : base ()
      {
         flags = flags_;
         key = key_;
         crealm = new Realm (crealm_);
         cname = cname_;
         transited = transited_;
         authtime = new KerberosTime (authtime_);
         endtime = new KerberosTime (endtime_);
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      public void Init () {
         flags = null;
         key = null;
         crealm = null;
         cname = null;
         transited = null;
         authtime = null;
         starttime = null;
         endtime = null;
         renew_till = null;
         caddr = null;
         authorization_data = null;
      }

      public override void Decode
         (Asn1BerDecodeBuffer buffer, bool explicitTagging, int implicitLength)
      {
         int llen = (explicitTagging) ?
            MatchTag (buffer, _TAG) : implicitLength;

         int llen2 = llen;
         llen = MatchTag (buffer, Asn1Tag.SEQUENCE);

         Init ();

         // decode SEQUENCE

         Asn1BerDecodeContext _context =
            new Asn1BerDecodeContext (buffer, llen);

         IntHolder elemLen = new IntHolder();

         // decode flags

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 0, elemLen, true)) {
            flags = new TicketFlags();
            flags.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode key

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 1, elemLen, true)) {
            key = new EncryptionKey();
            key.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode crealm

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 2, elemLen, true)) {
            crealm = new Realm();
            crealm.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode cname

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 3, elemLen, true)) {
            cname = new PrincipalName();
            cname.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode transited

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 4, elemLen, true)) {
            transited = new TransitedEncoding();
            transited.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode authtime

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 5, elemLen, true)) {
            authtime = new KerberosTime();
            authtime.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode starttime

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 6, elemLen, true)) {
            starttime = new KerberosTime();
            starttime.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }

         // decode endtime

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 7, elemLen, true)) {
            endtime = new KerberosTime();
            endtime.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode renew_till

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 8, elemLen, true)) {
            renew_till = new KerberosTime();
            renew_till.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }

         // decode caddr

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 9, elemLen, true)) {
            caddr = new HostAddresses();
            caddr.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }

         // decode authorization_data

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 10, elemLen, true)) {
            authorization_data = new AuthorizationData();
            authorization_data.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }

         if (llen == Asn1Status.INDEFLEN) {
            MatchTag (buffer, Asn1Tag.EOC);
         }
         if (explicitTagging && llen2 == Asn1Status.INDEFLEN) {
            MatchTag (buffer, Asn1Tag.EOC);
         }
      }

      public override int Encode (Asn1BerEncodeBuffer buffer, bool explicitTagging)
      {
         int _aal = 0, len;

         // encode authorization_data

         if (authorization_data != null) {
            len = authorization_data.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 10, len);
            _aal += len;
         }

         // encode caddr

         if (caddr != null) {
            len = caddr.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 9, len);
            _aal += len;
         }

         // encode renew_till

         if (renew_till != null) {
            len = renew_till.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 8, len);
            _aal += len;
         }

         // encode endtime

         len = endtime.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 7, len);
         _aal += len;

         // encode starttime

         if (starttime != null) {
            len = starttime.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 6, len);
            _aal += len;
         }

         // encode authtime

         len = authtime.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 5, len);
         _aal += len;

         // encode transited

         len = transited.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 4, len);
         _aal += len;

         // encode cname

         len = cname.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 3, len);
         _aal += len;

         // encode crealm

         len = crealm.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 2, len);
         _aal += len;

         // encode key

         len = key.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 1, len);
         _aal += len;

         // encode flags

         len = flags.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 0, len);
         _aal += len;

         _aal += buffer.EncodeTagAndLength (Asn1Tag.SEQUENCE, _aal);

         if (explicitTagging) {
            _aal += buffer.EncodeTagAndLength (_TAG, _aal);
         }

         return (_aal);
      }

   }
}

// 
// This file was generated by the Objective Systems ASN1C Compiler
// (http://www.obj-sys.com).  Version: 6.10, Date: 01-Jul-2008.
// 
using System;
using Com.Objsys.Asn1.Runtime;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile {

   public class EncKrbCredPart : Asn1Type {
      public new readonly static Asn1Tag _TAG = new Asn1Tag (Asn1Tag.APPL, Asn1Tag.CONS, 29);

      public _SeqOfKrbCredInfo ticket_info;
      public UInt32 nonce;  // optional
      public KerberosTime timestamp;  // optional
      public Microseconds usec;  // optional
      public HostAddress s_address;  // optional
      public HostAddress r_address;  // optional

      public EncKrbCredPart () : base()
      {
         Init();
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor sets all elements to references to the 
      /// given objects
      /// </summary>
      public EncKrbCredPart (
         _SeqOfKrbCredInfo ticket_info_,
         UInt32 nonce_,
         KerberosTime timestamp_,
         Microseconds usec_,
         HostAddress s_address_,
         HostAddress r_address_
      )
         : base ()
      {
         ticket_info = ticket_info_;
         nonce = nonce_;
         timestamp = timestamp_;
         usec = usec_;
         s_address = s_address_;
         r_address = r_address_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor is for required elements only.  It sets 
      /// all elements to references to the given objects
      /// </summary>
      public EncKrbCredPart (
         _SeqOfKrbCredInfo ticket_info_
      )
         : base ()
      {
         ticket_info = ticket_info_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor allows primitive data to be passed for all 
      /// primitive elements.  It will create new object wrappers for 
      /// the primitive data and set other elements to references to 
      /// the given objects 
      /// </summary>
      public EncKrbCredPart (_SeqOfKrbCredInfo ticket_info_,
         long nonce_,
         string timestamp_,
         long usec_,
         HostAddress s_address_,
         HostAddress r_address_
      )
         : base ()
      {
         ticket_info = ticket_info_;
         nonce = new UInt32 (nonce_);
         timestamp = new KerberosTime (timestamp_);
         usec = new Microseconds (usec_);
         s_address = s_address_;
         r_address = r_address_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      public void Init () {
         ticket_info = null;
         nonce = null;
         timestamp = null;
         usec = null;
         s_address = null;
         r_address = null;
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

         // decode ticket_info

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 0, elemLen, true)) {
            ticket_info = new _SeqOfKrbCredInfo();
            ticket_info.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode nonce

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 1, elemLen, true)) {
            nonce = new UInt32();
            nonce.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }

         // decode timestamp

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 2, elemLen, true)) {
            timestamp = new KerberosTime();
            timestamp.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }

         // decode usec

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 3, elemLen, true)) {
            usec = new Microseconds();
            usec.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }

         // decode s_address

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 4, elemLen, true)) {
            s_address = new HostAddress();
            s_address.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }

         // decode r_address

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 5, elemLen, true)) {
            r_address = new HostAddress();
            r_address.Decode (buffer, true, elemLen.mValue);
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

         // encode r_address

         if (r_address != null) {
            len = r_address.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 5, len);
            _aal += len;
         }

         // encode s_address

         if (s_address != null) {
            len = s_address.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 4, len);
            _aal += len;
         }

         // encode usec

         if (usec != null) {
            len = usec.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 3, len);
            _aal += len;
         }

         // encode timestamp

         if (timestamp != null) {
            len = timestamp.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 2, len);
            _aal += len;
         }

         // encode nonce

         if (nonce != null) {
            len = nonce.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 1, len);
            _aal += len;
         }

         // encode ticket_info

         len = ticket_info.Encode (buffer, true);
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

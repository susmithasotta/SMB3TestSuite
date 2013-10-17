// 
// This file was generated by the Objective Systems ASN1C Compiler
// (http://www.obj-sys.com).  Version: 6.10, Date: 01-Jul-2008.
// 
using System;
using Com.Objsys.Asn1.Runtime;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile {

   public class EncKrbPrivPart : Asn1Type {
      public new readonly static Asn1Tag _TAG = new Asn1Tag (Asn1Tag.APPL, Asn1Tag.CONS, 28);

      public Asn1OctetString user_data;
      public KerberosTime timestamp;  // optional
      public Microseconds usec;  // optional
      public UInt32 seq_number;  // optional
      public HostAddress s_address;
      public HostAddress r_address;  // optional

      public EncKrbPrivPart () : base()
      {
         Init();
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor sets all elements to references to the 
      /// given objects
      /// </summary>
      public EncKrbPrivPart (
         Asn1OctetString user_data_,
         KerberosTime timestamp_,
         Microseconds usec_,
         UInt32 seq_number_,
         HostAddress s_address_,
         HostAddress r_address_
      )
         : base ()
      {
         user_data = user_data_;
         timestamp = timestamp_;
         usec = usec_;
         seq_number = seq_number_;
         s_address = s_address_;
         r_address = r_address_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor is for required elements only.  It sets 
      /// all elements to references to the given objects
      /// </summary>
      public EncKrbPrivPart (
         Asn1OctetString user_data_,
         HostAddress s_address_
      )
         : base ()
      {
         user_data = user_data_;
         s_address = s_address_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor allows primitive data to be passed for all 
      /// primitive elements.  It will create new object wrappers for 
      /// the primitive data and set other elements to references to 
      /// the given objects 
      /// </summary>
      public EncKrbPrivPart (byte[] user_data_,
         string timestamp_,
         long usec_,
         long seq_number_,
         HostAddress s_address_,
         HostAddress r_address_
      )
         : base ()
      {
         user_data = new Asn1OctetString (user_data_);
         timestamp = new KerberosTime (timestamp_);
         usec = new Microseconds (usec_);
         seq_number = new UInt32 (seq_number_);
         s_address = s_address_;
         r_address = r_address_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor is for required elements only.  It allows 
      /// primitive data to be passed for all primitive elements.  
      /// It will create new object wrappers for the primitive data 
      /// and set other elements to references to the given objects. 
      /// </summary>
      public EncKrbPrivPart (
         byte[] user_data_,
         HostAddress s_address_
      )
         : base ()
      {
         user_data = new Asn1OctetString (user_data_);
         s_address = s_address_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      public void Init () {
         user_data = null;
         timestamp = null;
         usec = null;
         seq_number = null;
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

         // decode user_data

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 0, elemLen, true)) {
            user_data = new Asn1OctetString();
            user_data.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode timestamp

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 1, elemLen, true)) {
            timestamp = new KerberosTime();
            timestamp.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }

         // decode usec

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 2, elemLen, true)) {
            usec = new Microseconds();
            usec.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }

         // decode seq_number

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 3, elemLen, true)) {
            seq_number = new UInt32();
            seq_number.Decode (buffer, true, elemLen.mValue);
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
         else throw new Asn1MissingRequiredException (buffer);

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

         len = s_address.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 4, len);
         _aal += len;

         // encode seq_number

         if (seq_number != null) {
            len = seq_number.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 3, len);
            _aal += len;
         }

         // encode usec

         if (usec != null) {
            len = usec.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 2, len);
            _aal += len;
         }

         // encode timestamp

         if (timestamp != null) {
            len = timestamp.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 1, len);
            _aal += len;
         }

         // encode user_data

         len = user_data.Encode (buffer, true);
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

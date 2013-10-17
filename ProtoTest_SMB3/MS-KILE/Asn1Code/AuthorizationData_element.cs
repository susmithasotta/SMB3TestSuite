// 
// This file was generated by the Objective Systems ASN1C Compiler
// (http://www.obj-sys.com).  Version: 6.10, Date: 01-Jul-2008.
// 
using System;
using Com.Objsys.Asn1.Runtime;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile {

   public class AuthorizationData_element : Asn1Type {
      public Int32 ad_type;
      public Asn1OctetString ad_data;

      public AuthorizationData_element () : base()
      {
         Init();
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor sets all elements to references to the 
      /// given objects
      /// </summary>
      public AuthorizationData_element (
         Int32 ad_type_,
         Asn1OctetString ad_data_
      )
         : base ()
      {
         ad_type = ad_type_;
         ad_data = ad_data_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor allows primitive data to be passed for all 
      /// primitive elements.  It will create new object wrappers for 
      /// the primitive data and set other elements to references to 
      /// the given objects 
      /// </summary>
      public AuthorizationData_element (long ad_type_,
         byte[] ad_data_
      )
         : base ()
      {
         ad_type = new Int32 (ad_type_);
         ad_data = new Asn1OctetString (ad_data_);
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      public void Init () {
         ad_type = null;
         ad_data = null;
      }

      public override void Decode
         (Asn1BerDecodeBuffer buffer, bool explicitTagging, int implicitLength)
      {
         int llen = (explicitTagging) ?
            MatchTag (buffer, Asn1Tag.SEQUENCE) : implicitLength;

         Init ();

         // decode SEQUENCE

         Asn1BerDecodeContext _context =
            new Asn1BerDecodeContext (buffer, llen);

         IntHolder elemLen = new IntHolder();

         // decode ad_type

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 0, elemLen, true)) {
            ad_type = new Int32();
            ad_type.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode ad_data

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 1, elemLen, true)) {
            ad_data = new Asn1OctetString();
            ad_data.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         if (explicitTagging && llen == Asn1Status.INDEFLEN) {
            MatchTag (buffer, Asn1Tag.EOC);
         }
      }

      public override int Encode (Asn1BerEncodeBuffer buffer, bool explicitTagging)
      {
         int _aal = 0, len;

         // encode ad_data

         len = ad_data.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 1, len);
         _aal += len;

         // encode ad_type

         len = ad_type.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 0, len);
         _aal += len;

         if (explicitTagging) {
            _aal += buffer.EncodeTagAndLength (Asn1Tag.SEQUENCE, _aal);
         }

         return (_aal);
      }

   }
}
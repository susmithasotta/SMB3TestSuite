// 
// This file was generated by the Objective Systems ASN1C Compiler
// (http://www.obj-sys.com).  Version: 6.07, Date: 09-Jul-2009.
// 
using System;
using Com.Objsys.Asn1.Runtime;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile {

   public class KERB_AD_RESTRICTION_ENTRY : Asn1Type {
      public Int32 restriction_type;
      public LSAP_TOKEN_INFO_INTEGRITY restriction;

      public KERB_AD_RESTRICTION_ENTRY () : base()
      {
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor sets all elements to references to the 
      /// given objects
      /// </summary>
      public KERB_AD_RESTRICTION_ENTRY (
         Int32 restriction_type_,
         LSAP_TOKEN_INFO_INTEGRITY restriction_
      )
         : base ()
      {
         restriction_type = restriction_type_;
         restriction = restriction_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor allows primitive data to be passed for all 
      /// primitive elements.  It will create new object wrappers for 
      /// the primitive data and set other elements to references to 
      /// the given objects 
      /// </summary>
      public KERB_AD_RESTRICTION_ENTRY (long restriction_type_,
         LSAP_TOKEN_INFO_INTEGRITY restriction_
      )
         : base ()
      {
         restriction_type = new Int32 (restriction_type_);
         restriction = restriction_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      public void Init () {
         restriction_type = null;
         restriction = null;
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

         // decode restriction_type

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 0, elemLen, true)) {
            restriction_type = new Int32();
            restriction_type.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode restriction

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 1, elemLen, true)) {
            restriction = new LSAP_TOKEN_INFO_INTEGRITY();
            restriction.Decode (buffer, true, elemLen.mValue);
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

         // encode restriction

         len = restriction.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 1, len);
         _aal += len;

         // encode restriction_type

         len = restriction_type.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 0, len);
         _aal += len;

         if (explicitTagging) {
            _aal += buffer.EncodeTagAndLength (Asn1Tag.SEQUENCE, _aal);
         }

         return (_aal);
      }

   }
}
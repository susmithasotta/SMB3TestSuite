// 
// This file was generated by the Objective Systems ASN1C Compiler
// (http://www.obj-sys.com).  Version: 6.10, Date: 01-Jul-2008.
// 
using System;
using Com.Objsys.Asn1.Runtime;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile {

   public class ETYPE_INFO2_ENTRY : Asn1Type {
      public Int32 etype;
      public KerberosString salt;  // optional
      public Asn1OctetString s2kparams;  // optional

      public ETYPE_INFO2_ENTRY () : base()
      {
         Init();
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor sets all elements to references to the 
      /// given objects
      /// </summary>
      public ETYPE_INFO2_ENTRY (
         Int32 etype_,
         KerberosString salt_,
         Asn1OctetString s2kparams_
      )
         : base ()
      {
         etype = etype_;
         salt = salt_;
         s2kparams = s2kparams_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor is for required elements only.  It sets 
      /// all elements to references to the given objects
      /// </summary>
      public ETYPE_INFO2_ENTRY (
         Int32 etype_
      )
         : base ()
      {
         etype = etype_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor allows primitive data to be passed for all 
      /// primitive elements.  It will create new object wrappers for 
      /// the primitive data and set other elements to references to 
      /// the given objects 
      /// </summary>
      public ETYPE_INFO2_ENTRY (long etype_,
         string salt_,
         byte[] s2kparams_
      )
         : base ()
      {
         etype = new Int32 (etype_);
         salt = new KerberosString (salt_);
         s2kparams = new Asn1OctetString (s2kparams_);
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor is for required elements only.  It allows 
      /// primitive data to be passed for all primitive elements.  
      /// It will create new object wrappers for the primitive data 
      /// and set other elements to references to the given objects. 
      /// </summary>
      public ETYPE_INFO2_ENTRY (
         long etype_
      )
         : base ()
      {
         etype = new Int32 (etype_);
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      public void Init () {
         etype = null;
         salt = null;
         s2kparams = null;
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

         // decode etype

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 0, elemLen, true)) {
            etype = new Int32();
            etype.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode salt

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 1, elemLen, true)) {
            salt = new KerberosString();
            salt.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }

         // decode s2kparams

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 2, elemLen, true)) {
            s2kparams = new Asn1OctetString();
            s2kparams.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }

         if (explicitTagging && llen == Asn1Status.INDEFLEN) {
            MatchTag (buffer, Asn1Tag.EOC);
         }
      }

      public override int Encode (Asn1BerEncodeBuffer buffer, bool explicitTagging)
      {
         int _aal = 0, len;

         // encode s2kparams

         if (s2kparams != null) {
            len = s2kparams.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 2, len);
            _aal += len;
         }

         // encode salt

         if (salt != null) {
            len = salt.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 1, len);
            _aal += len;
         }

         // encode etype

         len = etype.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 0, len);
         _aal += len;

         if (explicitTagging) {
            _aal += buffer.EncodeTagAndLength (Asn1Tag.SEQUENCE, _aal);
         }

         return (_aal);
      }

   }
}

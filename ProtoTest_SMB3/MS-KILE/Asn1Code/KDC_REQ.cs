// 
// This file was generated by the Objective Systems ASN1C Compiler
// (http://www.obj-sys.com).  Version: 6.10, Date: 01-Jul-2008.
// 
using System;
using Com.Objsys.Asn1.Runtime;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile {

   public class KDC_REQ : Asn1Type {
      public Asn1Integer pvno;
      public Asn1Integer msg_type;
      public _SeqOfPA_DATA padata;  // optional
      public KDC_REQ_BODY req_body;

      public KDC_REQ () : base()
      {
         Init();
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor sets all elements to references to the 
      /// given objects
      /// </summary>
      public KDC_REQ (
         Asn1Integer pvno_,
         Asn1Integer msg_type_,
         _SeqOfPA_DATA padata_,
         KDC_REQ_BODY req_body_
      )
         : base ()
      {
         pvno = pvno_;
         msg_type = msg_type_;
         padata = padata_;
         req_body = req_body_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor is for required elements only.  It sets 
      /// all elements to references to the given objects
      /// </summary>
      public KDC_REQ (
         Asn1Integer pvno_,
         Asn1Integer msg_type_,
         KDC_REQ_BODY req_body_
      )
         : base ()
      {
         pvno = pvno_;
         msg_type = msg_type_;
         req_body = req_body_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor allows primitive data to be passed for all 
      /// primitive elements.  It will create new object wrappers for 
      /// the primitive data and set other elements to references to 
      /// the given objects 
      /// </summary>
      public KDC_REQ (long pvno_,
         long msg_type_,
         _SeqOfPA_DATA padata_,
         KDC_REQ_BODY req_body_
      )
         : base ()
      {
         pvno = new Asn1Integer (pvno_);
         msg_type = new Asn1Integer (msg_type_);
         padata = padata_;
         req_body = req_body_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor is for required elements only.  It allows 
      /// primitive data to be passed for all primitive elements.  
      /// It will create new object wrappers for the primitive data 
      /// and set other elements to references to the given objects. 
      /// </summary>
      public KDC_REQ (
         long pvno_,
         long msg_type_,
         KDC_REQ_BODY req_body_
      )
         : base ()
      {
         pvno = new Asn1Integer (pvno_);
         msg_type = new Asn1Integer (msg_type_);
         req_body = req_body_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      public void Init () {
         pvno = null;
         msg_type = null;
         padata = null;
         req_body = null;
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

         // decode pvno

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 1, elemLen, true)) {
            pvno = new Asn1Integer();
            pvno.Decode (buffer, true, elemLen.mValue);
            if (!(pvno.mValue == 5)) {
               throw new Asn1ConsVioException ("pvno.mValue", pvno.mValue);
            }

            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode msg_type

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 2, elemLen, true)) {
            msg_type = new Asn1Integer();
            msg_type.Decode (buffer, true, elemLen.mValue);
            if (!((msg_type.mValue == 10 || msg_type.mValue == 12))) {
               throw new Asn1ConsVioException ("msg_type.mValue", msg_type.mValue);
            }

            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }
         else throw new Asn1MissingRequiredException (buffer);

         // decode padata

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 3, elemLen, true)) {
            padata = new _SeqOfPA_DATA();
            padata.Decode (buffer, true, elemLen.mValue);
            if (elemLen.mValue == Asn1Status.INDEFLEN) {
               MatchTag (buffer, Asn1Tag.EOC);
            }
         }

         // decode req_body

         if (_context.MatchElemTag (Asn1Tag.CTXT, Asn1Tag.CONS, 4, elemLen, true)) {
            req_body = new KDC_REQ_BODY();
            req_body.Decode (buffer, true, elemLen.mValue);
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

         // encode req_body

         len = req_body.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 4, len);
         _aal += len;

         // encode padata

         if (padata != null) {
            len = padata.Encode (buffer, true);
            _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 3, len);
            _aal += len;
         }

         // encode msg_type

         if (!((msg_type.mValue == 10 || msg_type.mValue == 12))) {
            throw new Asn1ConsVioException ("msg_type.mValue", msg_type.mValue);
         }

         len = msg_type.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 2, len);
         _aal += len;

         // encode pvno

         if (!(pvno.mValue == 5)) {
            throw new Asn1ConsVioException ("pvno.mValue", pvno.mValue);
         }

         len = pvno.Encode (buffer, true);
         _aal += buffer.EncodeTagAndLength (Asn1Tag.CTXT, Asn1Tag.CONS, 1, len);
         _aal += len;

         if (explicitTagging) {
            _aal += buffer.EncodeTagAndLength (Asn1Tag.SEQUENCE, _aal);
         }

         return (_aal);
      }

   }
}

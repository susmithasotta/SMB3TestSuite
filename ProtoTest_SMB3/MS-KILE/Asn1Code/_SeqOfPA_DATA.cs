// 
// This file was generated by the Objective Systems ASN1C Compiler
// (http://www.obj-sys.com).  Version: 6.10, Date: 01-Jul-2008.
// 
using System;
using Com.Objsys.Asn1.Runtime;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile {

   public class _SeqOfPA_DATA : Asn1Type {
      public PA_DATA[] elements;

      public _SeqOfPA_DATA () : base()
      {
         elements = null;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor initializes the internal array to hold the 
      /// given number of elements.  The element values must be manually 
      /// populated.
      /// </summary>
      public _SeqOfPA_DATA (int numRecords) : base()
      {
         elements = new PA_DATA [numRecords];
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      /// <summary>
      /// This constructor initializes the internal array to hold the 
      /// given the array.  
      /// </summary>
      public _SeqOfPA_DATA (PA_DATA[] elements_) : base()
      {
         elements = elements_;
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      public override void Decode
         (Asn1BerDecodeBuffer buffer, bool explicitTagging, int implicitLength)
      {
         int llen = (explicitTagging) ?
            MatchTag (buffer, Asn1Tag.SEQUENCE) : implicitLength;

         // decode SEQUENCE OF or SET OF

         System.Collections.ArrayList llist = new System.Collections.ArrayList();
         Asn1BerDecodeContext _context =
             new Asn1BerDecodeContext (buffer, llen);
         PA_DATA element;
         int elemLen = 0;

         while (!_context.Expired()) {
            element = new PA_DATA();
            element.Decode (buffer, true, elemLen);
            llist.Add (element);
         }

         elements = new PA_DATA [llist.Count];
         Asn1Util.ToArray(llist, elements);

         if (explicitTagging && llen == Asn1Status.INDEFLEN) {
            MatchTag (buffer, Asn1Tag.EOC);
         }
      }

      public override int Encode (Asn1BerEncodeBuffer buffer, bool explicitTagging)
      {
         int _aal = 0, len;

         // encode SEQUENCE OF or SET OF

         for (int i = elements.Length - 1; i >= 0; i--) {
            len = elements[i].Encode (buffer, true);
            _aal += len;
         }

         if (explicitTagging) {
            _aal += buffer.EncodeTagAndLength (Asn1Tag.SEQUENCE, _aal);
         }

         return (_aal);
      }

   }
}

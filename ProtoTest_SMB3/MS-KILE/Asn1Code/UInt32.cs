// 
// This file was generated by the Objective Systems ASN1C Compiler
// (http://www.obj-sys.com).  Version: 6.10, Date: 01-Jul-2008.
// 
using System;
using Com.Objsys.Asn1.Runtime;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile {

   public class UInt32 : Asn1Integer {
      public UInt32 () : base()
      {
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      public UInt32 (long value_) : base(value_)
      {
         SetKey (_KerberosV5Spec2Values._rtkey);
      }

      public override void Decode
         (Asn1BerDecodeBuffer buffer, bool explicitTagging, int implicitLength)
      {
         base.Decode (buffer, explicitTagging, implicitLength);
         if (!((mValue >= 0 && mValue <= 4294967295))) {
            throw new Asn1ConsVioException ("mValue", mValue);
         }

      }

      public override int Encode (Asn1BerEncodeBuffer buffer, bool explicitTagging)
      {
         if (!((mValue >= 0 && mValue <= 4294967295))) {
            throw new Asn1ConsVioException ("mValue", mValue);
         }

         int _aal = base.Encode (buffer, explicitTagging);

         return (_aal);
      }

   }
}

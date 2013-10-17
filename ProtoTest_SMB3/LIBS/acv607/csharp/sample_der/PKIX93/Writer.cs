using System;

using Com.Objsys.Asn1.Runtime;
namespace Sample_der.PKIX93
{
	
	public class Writer
	{
		internal static byte[] g_SubjectPublicKeyValue = new byte[]{(byte) 0x02, 
      (byte) (0x81), (byte) (0x80), (byte) (0xaa), (byte) (0x98), (byte) (0xea),
      (byte) 0x13, (byte) (0x94), (byte) (0xa2), (byte) (0xdb), (byte) (0xf1),
      (byte) 0x5b, (byte) 0x7f, (byte) (0x98), (byte) 0x2f, (byte) 0x78, 
      (byte) (0xe7), (byte) (0xd8), (byte) (0xe3), (byte) (0xb9), (byte) 0x71,
      (byte) (0x86), (byte) (0xf6), (byte) (0x80), (byte) 0x2f, (byte) 0x40, 
      (byte) 0x39, (byte) (0xc3), (byte) (0xda), (byte) 0x3b, (byte) 0x4b, 
      (byte) 0x13, (byte) 0x46, (byte) 0x26, (byte) (0xee), (byte) 0x0d, 
      (byte) 0x56, (byte) (0xc5), (byte) (0xa3), (byte) 0x3a, (byte) 0x39,
      (byte) (0xb7), (byte) 0x7d, (byte) 0x33, (byte) (0xc2), (byte) 0x6b, 
      (byte) 0x5c, (byte) 0x77, (byte) (0x92), (byte) (0xf2), (byte) 0x55, 
      (byte) 0x65, (byte) (0x90), (byte) 0x39, (byte) (0xcd), (byte) 0x1a, 
      (byte) 0x3c, (byte) (0x86), (byte) (0xe1), (byte) 0x32, (byte) (0xeb),
      (byte) 0x25, (byte) (0xbc), (byte) (0x91), (byte) (0xc4), (byte) (0xff),
      (byte) (0x80), (byte) 0x4f, (byte) 0x36, (byte) 0x61, (byte) (0xbd), 
      (byte) (0xcc), (byte) (0xe2), (byte) 0x61, (byte) 0x04, (byte) (0xe0),
      (byte) 0x7e, (byte) 0x60, (byte) 0x13, (byte) (0xca), (byte) (0xc0), 
      (byte) (0x9c), (byte) (0xdd), (byte) (0xe0), (byte) (0xea), (byte) 0x41,
      (byte) (0xde), (byte) 0x33, (byte) (0xc1), (byte) (0xf1), (byte) 0x44, 
      (byte) (0xa9), (byte) (0xbc), (byte) 0x71, (byte) (0xde), (byte) (0xcf),
      (byte) 0x59, (byte) (0xd4), (byte) 0x6e, (byte) (0xda), (byte) 0x44, 
      (byte) (0x99), (byte) 0x3c, (byte) 0x21, (byte) 0x64, (byte) (0xe4),
      (byte) 0x78, (byte) 0x54, (byte) (0x9d), (byte) (0xd0), (byte) 0x7b, 
      (byte) (0xba), (byte) 0x4e, (byte) (0xf5), (byte) 0x18, (byte) 0x4d, 
      (byte) 0x5e, (byte) 0x39, (byte) 0x30, (byte) (0xbf), (byte) (0xe0),
      (byte) (0xd1), (byte) (0xf6), (byte) (0xf4), (byte) (0x83), (byte) 0x25,
      (byte) 0x4f, (byte) 0x14, (byte) (0xaa), (byte) 0x71, (byte) (0xe1)};
		
		internal static byte[] g_Extn1Value = new byte[]{(byte) 0x30, (byte) 0x03,
      (byte) 0x01, (byte) 0x01, (byte) (0xff)};
		
		internal static byte[] g_Extn2Value = new byte[]{(byte) 0x04, (byte) 0x14,
      (byte) (0xe7), (byte) 0x26, (byte) (0xc5), (byte) 0x54, (byte) (0xcd),
      (byte) 0x5b, (byte) (0xa3), (byte) 0x6f, (byte) 0x35, (byte) 0x68, 
      (byte) (0x95), (byte) (0xaa), (byte) (0xd5), (byte) (0xff), (byte) 0x1c,
      (byte) 0x21, (byte) (0xe4), (byte) 0x22, (byte) 0x75, (byte) (0xd6)};
		
		internal static byte[] g_Signature = new byte[]{(byte) 0x30, (byte) 0x2c,
      (byte) 0x02, (byte) 0x14, (byte) (0xa0), (byte) 0x66, (byte) (0xc1), 
      (byte) 0x76, (byte) 0x33, (byte) (0x99), (byte) 0x13, (byte) 0x51, 
      (byte) (0x8d), (byte) (0x93), (byte) 0x64, (byte) 0x2f, (byte) (0xca), 
      (byte) 0x13, (byte) 0x73, (byte) (0xde), (byte) 0x79, (byte) 0x1a,
      (byte) 0x7d, (byte) 0x33, (byte) 0x02, (byte) 0x14, (byte) 0x5d, 
      (byte) (0x90), (byte) (0xf6), (byte) (0xce), (byte) (0x92), (byte) 0x4a,
      (byte) (0xbf), (byte) 0x29, (byte) 0x11, (byte) 0x24, (byte) (0x80), 
      (byte) 0x28, (byte) (0xa6), (byte) 0x5a, (byte) (0x8e), (byte) 0x73, 
      (byte) (0xb6), (byte) 0x76, (byte) 0x02, (byte) 0x68};
		
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.dat").ToString();
			bool trace = true;
			
			// Process command line arguments
			
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].Equals("-v"))
						Diag.Instance().SetEnabled(true);
					else if (args[i].Equals("-o"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
					else if (args[i].Equals("-notrace"))
						trace = false;
					else
					{
						System.Console.Out.WriteLine("usage: Writer [ -v ] [ -o <filename>");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -o <filename>  " + "write encoded msg to <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			// Encode names types that will be required in RDN structure
			
			Asn1BerEncodeBuffer encodeBuffer = new Asn1BerEncodeBuffer();
			Asn1PrintableString ps = new Asn1PrintableString();
         ps.SetKey(_PKIX1Implicit93Values._rtkey);
			byte[] encodedUS, encodedGov, encodedNist;
			
			try
			{
				ps.mValue = "US";
				ps.Encode(encodeBuffer, true);
				encodedUS = encodeBuffer.MsgCopy;
				
				encodeBuffer.Reset();
				ps.mValue = "gov";
				ps.Encode(encodeBuffer, true);
				encodedGov = encodeBuffer.MsgCopy;
				
				encodeBuffer.Reset();
				ps.mValue = "nist";
				ps.Encode(encodeBuffer, true);
				encodedNist = encodeBuffer.MsgCopy;
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(e.Message);
				Asn1Util.WriteStackTrace(e, Console.Error);
				return ;
			}
			
			// Create relative distinguished name object that will be used in 
			// issuer and subject RDN fields..
			
			RelativeDistinguishedName[] rdn = new RelativeDistinguishedName[3];
			rdn[0] = new RelativeDistinguishedName(1);
			rdn[0].elements[0] = new AttributeTypeAndValue();
			rdn[0].elements[0].type = new Asn1ObjectIdentifier(_PKIX1Explicit93Values.id_at_countryName);
			rdn[0].elements[0].value_ = new Asn1OpenType(encodedUS);
			
			rdn[1] = new RelativeDistinguishedName(1);
			rdn[1].elements[0] = new AttributeTypeAndValue();
			rdn[1].elements[0].type = new Asn1ObjectIdentifier(_PKIX1Explicit93Values.id_at_organizationName);
			rdn[1].elements[0].value_ = new Asn1OpenType(encodedGov);
			
			rdn[2] = new RelativeDistinguishedName(1);
			rdn[2].elements[0] = new AttributeTypeAndValue();
			rdn[2].elements[0].type = new Asn1ObjectIdentifier(_PKIX1Explicit93Values.id_at_organizationalUnitName);
			rdn[2].elements[0].value_ = new Asn1OpenType(encodedNist);
			
			RDNSequence nameRDNSeq = new RDNSequence();
			nameRDNSeq.elements = rdn;
			
			// Encode DSS parameters for Subject Public Key field
			
			Dss_Parms dssParms = new Dss_Parms(
            new Asn1BigInteger("d43802c5357bd50ba17e5d72596355d34556eae2251a6bc5a4abaa0bd462b4d221b195a2c601c9c3fa016f7986833d0361e1f192acbc034e89a3c9534af7e2a648cf421e21b15c2b3a7fbabe6b5af70a26d88e1bebecbf1e5a3f45c0bd3123be6971a7c290fea5d680b524dc449ceb4df9daf0c8e8a24c99075c8e352b7d578d", 16), 
            new Asn1BigInteger("a7839bf3bd2c2007fc4ce7e89ff33983510ddcdd", 16), 
            new Asn1BigInteger("0e3b46318a0a58864084e3a1220d88ca908857649f0121e01505942482e21090d9e14e105ce7546bd40c2b1b590aa0b5a17db507e3657cea90d88e3042e485bbacfa4e764b780edf6ce5a6e1bd59777da69759c529a7b33f953e9df1592df74287623ff1b86fc73d4bb88d74c4ca4490cf67dbde1460974ad1f76d9e0994c40d", 16));
			
			try
			{
				encodeBuffer.Reset();
				dssParms.Encode(encodeBuffer, true);
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(e.Message);
				Asn1Util.WriteStackTrace(e, Console.Error);
				return ;
			}
			
			byte[] encodedDssParms = encodeBuffer.MsgCopy;
			
			// Create a certificate data object and populate it with the data 
			// to be encoded
			
			Certificate certificate = new Certificate();
			certificate.toBeSigned = new Certificate_toBeSigned();
			certificate.toBeSigned.version = new Version(Version.v3);
			certificate.toBeSigned.serialNumber = new CertificateSerialNumber(17);
			certificate.toBeSigned.signature = new AlgorithmIdentifier(_PKIX1Explicit93Values.id_dsa_with_sha1, null);
			
			// Issuer
			
			certificate.toBeSigned.issuer = new Name();
			certificate.toBeSigned.issuer.Set_rdnSequence(nameRDNSeq);
			
			// Validity
			
			certificate.toBeSigned.validity = new Validity();
			certificate.toBeSigned.validity.notBefore = new Time();
			certificate.toBeSigned.validity.notBefore.Set_utcTime(new Asn1UTCTime("970730000000Z"));
			certificate.toBeSigned.validity.notAfter = new Time();
			certificate.toBeSigned.validity.notAfter.Set_utcTime(new Asn1UTCTime("971201000000Z"));
			
			// Subject
			
			certificate.toBeSigned.subject = new Name();
			certificate.toBeSigned.subject.Set_rdnSequence(nameRDNSeq);
			
			// SubjectPublicKeyInfo
			
			certificate.toBeSigned.subjectPublicKeyInfo = new SubjectPublicKeyInfo();
			certificate.toBeSigned.subjectPublicKeyInfo.algorithm = new AlgorithmIdentifier();
			certificate.toBeSigned.subjectPublicKeyInfo.algorithm.algorithm = new Asn1ObjectIdentifier(_PKIX1Explicit93Values.id_dsa);
			certificate.toBeSigned.subjectPublicKeyInfo.algorithm.parameters = new Asn1OpenType(encodedDssParms);
			certificate.toBeSigned.subjectPublicKeyInfo.subjectPublicKey = new Asn1BitString(g_SubjectPublicKeyValue.Length * 8, g_SubjectPublicKeyValue);
			
			// Extensions
			
			certificate.toBeSigned.extensions = new Extensions(2);
			
			certificate.toBeSigned.extensions.elements[0] = new Extension(_PKIX1Implicit93Values.id_ce_basicConstraints, true, g_Extn1Value);
			
			certificate.toBeSigned.extensions.elements[1] = new Extension(_PKIX1Implicit93Values.id_ce_subjectKeyIdentifier, false, g_Extn2Value);
			
			// Algorithm
			
			certificate.algorithm = new AlgorithmIdentifier();
			certificate.algorithm.algorithm = new Asn1ObjectIdentifier(_PKIX1Explicit93Values.id_dsa_with_sha1);
			
			// Signature
			
			certificate.signature = new Asn1BitString(g_Signature.Length * 8, g_Signature);
			
			// Encode the certificate
			
			encodeBuffer.Reset();
			
			try
			{
				certificate.Encode(encodeBuffer, true);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					encodeBuffer.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer.BinDump();
				}
				
				// Write the encoded record to a file
				
				encodeBuffer.Write(new System.IO.FileStream(filename, System.IO.FileMode.Create));
				
				// Generate a dump file for comparisons
				
				System.IO.StreamWriter messagedmp = new System.IO.StreamWriter(new System.IO.FileStream("message.dmp", System.IO.FileMode.Create));
				messagedmp.AutoFlush = true;
				encodeBuffer.HexDump(messagedmp);
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(e.Message);
				Asn1Util.WriteStackTrace(e, Console.Error);
				return ;
			}
		}
	}
}
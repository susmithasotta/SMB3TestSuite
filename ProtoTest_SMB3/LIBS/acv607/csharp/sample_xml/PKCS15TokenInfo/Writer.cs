using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_xml.PKCS15TokenInfo
{
	
	public class Writer
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.xml").ToString();
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
						System.Console.Out.WriteLine("usage: Writer [ -cxer ] [ -v ] " + "[ -o <filename>");
						System.Console.Out.WriteLine("   -cxer  Use Canonical XER");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -o <filename>  " + "write encoded msg to <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			try
			{
				// Encode open type parameters
				
				Asn1XmlEncodeBuffer encbuf1 = new Asn1XmlEncodeBuffer();
				Asn1Null nullElement = new Asn1Null();
            nullElement.SetKey(_PKCS15TokenInfoValues._rtkey);
				nullElement.Encode(encbuf1, null, null);
				Asn1XerOpenType nullOpenType = new Asn1XerOpenType(encbuf1);
				
				Asn1XmlEncodeBuffer encbuf2 = new Asn1XmlEncodeBuffer();
				byte[] octStrValue = new byte[]{0, 0, 0, 0, 0, 0, 0, 0};
				Asn1OctetString octStr = new Asn1OctetString(octStrValue);
            octStr.SetKey(_PKCS15TokenInfoValues._rtkey);
				octStr.Encode(encbuf2, null, null);
				Asn1XerOpenType octStrOpenType = new Asn1XerOpenType(encbuf2);
				
				// Algorithm info
				
				_SeqOfAlgorithmInfo algoInfo = new _SeqOfAlgorithmInfo(4);
				
				int[] algOID0 = new int[]{1, 3, 14, 3, 2, 26};
				algoInfo.elements[0] = new AlgorithmInfo();
				algoInfo.elements[0].reference = new Reference(1);
				algoInfo.elements[0].algorithm = new Asn1Integer(1);
				algoInfo.elements[0].parameters = nullOpenType;
				algoInfo.elements[0].supportedOperations = new Operations();
				algoInfo.elements[0].supportedOperations.Set(Operations.hash, true);
				algoInfo.elements[0].algId = new Asn1ObjectIdentifier(algOID0);
				
				int[] algOID1 = new int[]{1, 3, 36, 3, 4, 3, 2, 1};
				algoInfo.elements[1] = new AlgorithmInfo();
				algoInfo.elements[1].reference = new Reference(2);
				algoInfo.elements[1].algorithm = new Asn1Integer(2);
				algoInfo.elements[1].parameters = nullOpenType;
				algoInfo.elements[1].supportedOperations = new Operations();
				algoInfo.elements[1].supportedOperations.Set(Operations.compute_signature, true);
				algoInfo.elements[1].algId = new Asn1ObjectIdentifier(algOID1);
				
				int[] algOID2 = new int[]{1, 0, 0};
				algoInfo.elements[2] = new AlgorithmInfo();
				algoInfo.elements[2].reference = new Reference(3);
				algoInfo.elements[2].algorithm = new Asn1Integer(3);
				algoInfo.elements[2].parameters = nullOpenType;
				algoInfo.elements[2].supportedOperations = new Operations();
				algoInfo.elements[2].supportedOperations.Set(Operations.compute_checksum, true);
				algoInfo.elements[2].supportedOperations.Set(Operations.verify_checksum, true);
				algoInfo.elements[2].algId = new Asn1ObjectIdentifier(algOID2);
				
				int[] algOID3 = new int[]{1, 2, 840, 113549, 3, 7};
				byte[] params3 = new byte[]{4, 8, 0, 0, 0, 0, 0, 0, 0, 0};
				algoInfo.elements[3] = new AlgorithmInfo();
				algoInfo.elements[3].reference = new Reference(4);
				algoInfo.elements[3].algorithm = new Asn1Integer(4);
				algoInfo.elements[3].parameters = octStrOpenType;
				algoInfo.elements[3].supportedOperations = new Operations();
				algoInfo.elements[3].supportedOperations.Set(Operations.encipher, true);
				algoInfo.elements[3].supportedOperations.Set(Operations.decipher, true);
				algoInfo.elements[3].algId = new Asn1ObjectIdentifier(algOID3);
				
				// TokenInfo
				
				TokenInfo tokenInfo = new TokenInfo();
				tokenInfo.version = new TokenInfo_version(TokenInfo_version.v1);
				try
				{
					tokenInfo.serialNumber = new Asn1OctetString("'159752222515401240'H");
				}
				catch (Asn1ValueParseException)
				{
					System.Console.Out.WriteLine("unable to parse serial number value");
					return ;
				}
				tokenInfo.manufacturerID = new Label("XY, Inc.");
				tokenInfo.label = new Label("Digital signature card");
				tokenInfo.tokenflags = new TokenFlags();
				tokenInfo.tokenflags.Set(TokenFlags.prnGeneration, true);
				tokenInfo.seInfo = new _SeqOfSecurityEnvironmentInfo(1);
				int[] oid = new int[]{1, 0, 0};
				tokenInfo.seInfo.elements[0] = new SecurityEnvironmentInfo(1, oid);
				tokenInfo.supportedAlgorithms = algoInfo;
				tokenInfo.issuerId = new Label("wxy");
				tokenInfo.holderId = new Label("vwx");
				
				// Create a message buffer object and encode the record
				
				Asn1XmlEncodeBuffer encodeBuffer = new Asn1XmlEncodeBuffer();
				
				tokenInfo.Encode(encodeBuffer);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					encodeBuffer.Write(System.Console.OpenStandardOutput());
				}
				
				// Write the encoded record to a file
				
				encodeBuffer.Write(new System.IO.FileStream(filename, System.IO.FileMode.Create));
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
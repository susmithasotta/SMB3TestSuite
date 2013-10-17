using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_xer.InfoObject
{
	
	public class Writer
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.xml").ToString();
			bool cxer = false, trace = true;
			
			// Process command line arguments
			
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].Equals("-v"))
						Diag.Instance().SetEnabled(true);
					else if (args[i].Equals("-o"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
					else if (args[i].Equals("-cxer"))
						cxer = true;
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
			
			// Encode a format type
			
			Asn1Boolean b = new Asn1Boolean(true);
			Asn1XerEncodeBuffer encodeBuffer = new Asn1XerEncodeBuffer();
			try
			{
				encodeBuffer.Canonical = true;
				
				b.Encode(encodeBuffer, null);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding of FormatType was successful");
					encodeBuffer.Write(System.Console.OpenStandardOutput());
					System.Console.Out.WriteLine("");
				}
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(e.Message);
				Asn1Util.WriteStackTrace(e, Console.Error);
				return ;
			}
			
			// Create a data object and populate it with the data to be encoded
			
			BIOMETRIC_IDENTIFIER bioID = new BIOMETRIC_IDENTIFIER();
			bioID.Set_id(new Asn1RelativeOID(_TestValues.id_ibia_SecuGen));
			
			Format format = new Format(bioID, new Asn1XerOpenType(encodeBuffer));
			
			// Encode final message
			
			encodeBuffer.Reset();
			encodeBuffer.Canonical = cxer;
			try
			{
				encodeBuffer.EncodeStartDocument();
				
				format.Encode(encodeBuffer, null);
				
				encodeBuffer.EncodeEndDocument();
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding of format was successful");
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
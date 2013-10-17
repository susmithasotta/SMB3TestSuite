using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_xer.BigInteger
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
			
			// Create a data object and populate it with the data to be encoded
			
			Dss_Parms dssParms = new Dss_Parms(new Asn1BigInteger("112233445566778899aabbccddeeff", 16), new Asn1BigInteger("-1234567890123456789012345678901234567890"), new Asn1BigInteger("1234567890123456789012345678901234567890"));
			
			if (trace)
			{
				System.Console.Out.WriteLine("dssParms.p = " + dssParms.p);
				System.Console.Out.WriteLine("dssParms.q = " + dssParms.q);
				System.Console.Out.WriteLine("dssParms.g = " + dssParms.g + "\n");
			}
			
			// Create a message buffer object and encode the record
			
			try
			{
				Asn1XerEncodeBuffer encodeBuffer = new Asn1XerEncodeBuffer(cxer, 0);
				
				encodeBuffer.EncodeStartDocument();
				
				dssParms.Encode(encodeBuffer, null);
				
				encodeBuffer.EncodeEndDocument();
				
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
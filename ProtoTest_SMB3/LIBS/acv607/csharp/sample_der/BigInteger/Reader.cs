using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_der.BigInteger
{
	
	public class Reader
	{
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
					else if (args[i].Equals("-i"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
					else if (args[i].Equals("-notrace"))
						trace = false;
					else
					{
						System.Console.Out.WriteLine("usage: Reader [ -v ] [ -i <filename> ]");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -i <filename>  " + "read Encoded msg from <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			try
			{
				// Create an input file stream object
				
				System.IO.FileStream ins = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				
				// Create a Decode buffer object
				
				Asn1DerDecodeBuffer DecodeBuffer = new Asn1DerDecodeBuffer(ins);
				
				// Read and Decode the message
				
				Dss_Parms dssParms = new Dss_Parms();
				dssParms.Decode(DecodeBuffer);
				if (trace)
				{
					System.Console.Out.WriteLine("Decode was successful");
					dssParms.Print("dssParms");
				}
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
using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_ber.EventHandler
{
	
	public class Reader
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.dat").ToString();
			
			// Process command line arguments
			
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].Equals("-v"))
						Diag.Instance().SetEnabled(true);
					else if (args[i].Equals("-i"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
					else if (args[i].Equals("-notrace")) { continue; }
					else
					{
						System.Console.Out.WriteLine("usage: Reader [ -v ] [ -i <filename> ]");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -i <filename>  " + "read Encoded msg from <filename>");
						return ;
					}
				}
			}
			
			try
			{
				// Create an input file stream object
				
				System.IO.FileStream ins = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				
				// Create a Decode buffer object
				
				Asn1BerDecodeBuffer decodeBuffer = new Asn1BerDecodeBuffer(ins);
				
				// Register event handler object
				
				PrintHandler printHandler = new PrintHandler("personnelRecord");
				decodeBuffer.AddNamedEventHandler(printHandler);
				
				// Read and Decode the message
				
				PersonnelRecord personnelRecord = new PersonnelRecord();
				
				personnelRecord.Decode(decodeBuffer);
				
				System.Console.Out.WriteLine("}"); // needed to close print block
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
using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_per.ExtChoice
{
	
	public class Reader
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.dat").ToString();
			bool aligned = true, trace = true;
			int tvalue = 2;
			
			// Process command line arguments
			
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].Equals("-v"))
						Diag.Instance().SetEnabled(true);
					else if (args[i].Equals("-i"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
					else if (args[i].Equals("-a"))
						aligned = true;
					else if (args[i].Equals("-u"))
						aligned = false;
					else if (args[i].Equals("-t"))
						tvalue = System.Int32.Parse(args[++i]);
					else if (args[i].Equals("-notrace"))
						trace = false;
					else
					{
						System.Console.Out.WriteLine("usage: Reader [ -a | -u ] " + "[ -v ] [ -i <filename>");
						System.Console.Out.WriteLine("   -a  PER aligned encoding (default)");
						System.Console.Out.WriteLine("   -u  PER unaligned encoding");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -i <filename>  " + "read encoded msg from <filename>");
						System.Console.Out.WriteLine("   -t <option>  select option number");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			try
			{
				// Create an input file stream object
				
				System.IO.FileStream ins = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				
				// Create a decode buffer object
				
				Asn1PerDecodeBuffer decodeBuffer = new Asn1PerDecodeBuffer(ins, aligned);
				
				// Enable bit field tracing
				
				if (trace)
				{
					decodeBuffer.TraceHandler.Enable();
				}
				
				// Read and decode the message
				
				AliasAddress aliasAddress = new AliasAddress();
				aliasAddress.Decode(decodeBuffer);
				if (trace)
				{
					System.Console.Out.WriteLine("Decode was successful");
					aliasAddress.Print("aliasAddress");
					System.Console.Out.WriteLine("");
					System.Console.Out.WriteLine("Binary trace:");
					decodeBuffer.BinDump("aliasAddress");
				}
				
				// To get the selected value, you would do the following:
				
				Asn1BMPString element;
				if (aliasAddress.ChoiceID == AliasAddress._H323_ID)
				{
					element = (Asn1BMPString) aliasAddress.GetElement();
				}
				else
				{
					System.Console.Out.WriteLine("Unexpected value.\n");
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
using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_per.Employee3
{
	
	public class Reader
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.dat").ToString();
			bool aligned = true, trace = true;
			
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
					else if (args[i].Equals("-notrace"))
						trace = false;
					else
					{
						System.Console.Out.WriteLine("usage: Reader [ -a | -u ] " + "[ -v ] [ -i <filename>");
						System.Console.Out.WriteLine("   -a  PER aligned encoding (default)");
						System.Console.Out.WriteLine("   -u  PER unaligned encoding");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -i <filename>  " + "read encoded msg from <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			// Create an input file stream object
			
			System.IO.FileStream ins = null;
			try
			{
				ins = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(e.Message);
				Asn1Util.WriteStackTrace(e, Console.Error);
				return ;
			}
			
			// Create a decode buffer object
			
			Asn1PerDecodeBuffer decodeBuffer = new Asn1PerDecodeBuffer(ins, aligned);
			
			PersonnelRecord personnelRecord = new PersonnelRecord();
			
			try
			{
				// Enable bit field tracing
				
				if (trace)
				{
					decodeBuffer.TraceHandler.Enable();
				}
				
				// Read and decode the message
				
				personnelRecord.Decode(decodeBuffer);
				if (trace)
				{
					System.Console.Out.WriteLine("Decode was successful");
					personnelRecord.Print("personnelRecord");
					System.Console.Out.WriteLine("");
					System.Console.Out.WriteLine("Binary trace:");
					decodeBuffer.BinDump("personnelRecord");
				}
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(e.Message);
				Asn1Util.WriteStackTrace(e, Console.Error);
				System.Console.Out.WriteLine("");
				System.Console.Out.WriteLine("Decode failed.");
				System.IO.StreamWriter temp_writer2;
				temp_writer2 = new System.IO.StreamWriter(System.Console.OpenStandardOutput(), System.Console.Out.Encoding);
				temp_writer2.AutoFlush = true;
				personnelRecord.Print(temp_writer2, "personnelRecord", 0);
				System.Console.Out.WriteLine("");
				System.Console.Out.WriteLine("Binary trace:");
				decodeBuffer.BinDump("personnelRecord");
				return ;
			}
		}
	}
}
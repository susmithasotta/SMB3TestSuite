using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_ber.Real
{
	
	public class Writer
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
					else if (args[i].Equals("-o"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
					else if (args[i].Equals("-notrace"))
						trace = false;
					else
					{
						System.Console.Out.WriteLine("usage: Writer [ -v ] [ -o <filename>");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -o <filename>  " + "write Encoded msg to <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			// Create a data object and populate it with the data to be Encoded
			
			PersonnelRecord personnelRecord = new PersonnelRecord();
			
			try
			{
				personnelRecord.bits = new Asn1BitString("'11010010111001'B");
				personnelRecord.name = new Asn1VisibleString("Director");
				personnelRecord.fp = new FloatingPoint(- 0.7654321e+139);
				//personnelRecord.fp = new FloatingPoint(Double.POSITIVE_INFINITY);
				//personnelRecord.fp = new FloatingPoint (Double.NEGATIVE_INFINITY);
			
				// Create a message buffer object and Encode the record
				
				Asn1BerEncodeBuffer encodeBuffer = new Asn1BerEncodeBuffer();
				
				personnelRecord.Encode(encodeBuffer, true);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of Encoded record:");
					encodeBuffer.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer.BinDump();
				}
				
				// Write the Encoded record to a file
				
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
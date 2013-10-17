using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_ber.TimeString
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
						System.Console.Out.WriteLine("   -o <filename>  " + "write encoded msg to <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			// Create a data object and populate it with the data to be encoded
			TimesSeq tmSeq = new TimesSeq();
			tmSeq.genTime = new GenTime("19991224085044");
			tmSeq.utcTime = new UtcTime("9912140850Z");
			
			// Create a message buffer object and encode the record
			Asn1BerEncodeBuffer encodeBuffer = new Asn1BerEncodeBuffer(16);
			
			try
			{
				tmSeq.genTime.SetTime(System.DateTime.Now);
				tmSeq.genTime.Fraction ="2";
				
				tmSeq.utcTime.Year = 1985;
				tmSeq.utcTime.Month = Asn1Time.February;
				tmSeq.utcTime.Day = 22;
				tmSeq.utcTime.Hour = 5;
				tmSeq.utcTime.Minute = 35;
				tmSeq.utcTime.UTC = true;
				
				tmSeq.Encode(encodeBuffer, true);
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
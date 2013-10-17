using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_ber.TimeString
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
						System.Console.Out.WriteLine("   -i <filename>  " + "read encoded msg from <filename>");
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
				
				Asn1BerDecodeBuffer decodeBuffer = new Asn1BerDecodeBuffer(ins);
				
				// Read and Decode the message
				TimesSeq tmSeq = new TimesSeq();
				tmSeq.Decode(decodeBuffer);
				if (trace)
				{
					System.Console.Out.WriteLine("Decode was successful");
					tmSeq.Print("tmSeq");
               System.Console.Out.WriteLine("GenTime:");
               System.Console.Out.WriteLine("Year: " + tmSeq.genTime.Year);
               System.Console.Out.WriteLine("Month: " + tmSeq.genTime.Month);
               System.Console.Out.WriteLine("Day: " + tmSeq.genTime.Day);
               System.Console.Out.WriteLine("Hour: " + tmSeq.genTime.Hour);
               System.Console.Out.WriteLine("Minute: " + tmSeq.genTime.Minute);
               System.Console.Out.WriteLine("Second: " + tmSeq.genTime.Second);
               System.Console.Out.WriteLine("Fraction: " + tmSeq.genTime.Fraction);
               System.Console.Out.WriteLine("DiffH: " + tmSeq.genTime.DiffHour);
               System.Console.Out.WriteLine("DiffM: " + tmSeq.genTime.DiffMinute);
               
               System.Console.Out.WriteLine("");
               System.Console.Out.WriteLine("UtcTime:");
               System.Console.Out.WriteLine("Year: " + tmSeq.utcTime.Year);
               System.Console.Out.WriteLine("Month: " + tmSeq.utcTime.Month);
               System.Console.Out.WriteLine("Day: " + tmSeq.utcTime.Day);
               System.Console.Out.WriteLine("Hour: " + tmSeq.utcTime.Hour);
               System.Console.Out.WriteLine("Minute: " + tmSeq.utcTime.Minute);
               System.Console.Out.WriteLine("Second: " + tmSeq.utcTime.Second);
               System.Console.Out.WriteLine("DiffH: " + tmSeq.utcTime.DiffHour);
               System.Console.Out.WriteLine("DiffM: " + tmSeq.utcTime.DiffMinute);
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
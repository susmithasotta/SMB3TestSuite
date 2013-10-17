using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_per.CMLogon
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
			
			try
			{
				// Create an input file stream object
				
				System.IO.FileStream ins = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				
				// Create an ACSE decode buffer object
				
				Asn1PerDecodeBuffer acseDecodeBuffer = new Asn1PerDecodeBuffer(ins, aligned);
				
				// Enable bit field tracing
				
				if (trace)
				{
					acseDecodeBuffer.TraceHandler.Enable();
				}
				
				// Decode the ACSE header
				
				ACSE_apdu acse = new ACSE_apdu();
				
				acse.Decode(acseDecodeBuffer);
				
				if (trace)
				{
					System.Console.Out.WriteLine("ACSE decode was successful");
					acse.Print("acse");
					System.Console.Out.WriteLine("");
					System.Console.Out.WriteLine("Binary trace:");
					acseDecodeBuffer.BinDump("acse");
				}
				
				// Decode CM Aircraft Message 
				
				AARQ_apdu aarq = (AARQ_apdu) acse.GetElement();
				Asn1External ext = aarq.user_information.elements[0];
				Asn1BitString externalData = (Asn1BitString) ext.encoding.GetElement();
				
				Asn1PerDecodeBuffer cmDecodeBuffer = new Asn1PerDecodeBuffer(externalData.mValue, aligned);
				
				CMAircraftMessage cmAircraftMessage = new CMAircraftMessage();
				
				if (trace)
				{
					cmDecodeBuffer.TraceHandler.Enable();
				}
				
				cmAircraftMessage.Decode(cmDecodeBuffer);
				
				if (trace)
				{
					System.Console.Out.WriteLine("CMAircraftMessage decode was successful");
					System.IO.StreamWriter temp_writer2;
					temp_writer2 = new System.IO.StreamWriter(System.Console.OpenStandardOutput(), System.Console.Out.Encoding);
					temp_writer2.AutoFlush = true;
					cmAircraftMessage.Print(temp_writer2, "cmAircraftMessage", 0);
					System.Console.Out.WriteLine("");
					System.Console.Out.WriteLine("Binary trace:");
					cmDecodeBuffer.BinDump("cmAircraftMessage");
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
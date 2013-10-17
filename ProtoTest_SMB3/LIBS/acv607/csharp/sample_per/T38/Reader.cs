using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_per.T38
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
				
				// Create a decode buffer object
				
				Asn1PerDecodeBuffer decodeBuffer1 = new Asn1PerDecodeBuffer(ins, aligned);
				
				// Enable bit field tracing
				
				if (trace)
				{
					decodeBuffer1.TraceHandler.Enable();
				}
				
				// First, decode the UDPTLPacket structure
				
				UDPTLPacket udptlPacket = new UDPTLPacket();
				
				udptlPacket.Decode(decodeBuffer1);
				
				if (trace)
				{
					System.Console.Out.WriteLine("UDPTL packet decode was successful");
					udptlPacket.Print("udptlPacket");
					System.Console.Out.WriteLine("");
					System.Console.Out.WriteLine("Binary trace:");
					decodeBuffer1.BinDump("udptlPacket");
				}
				
				// Decode primary IFPPacket
				
				IFPPacket primaryPacket = new IFPPacket();
				
				Asn1PerDecodeBuffer decodeBuffer2 = 
               new Asn1PerDecodeBuffer(
                  udptlPacket.primary_ifp_packet.mValue, aligned);
				
				if (trace)
				{
					decodeBuffer2.TraceHandler.Enable();
				}
				
				primaryPacket.Decode(decodeBuffer2);
				
				if (trace)
				{
					System.Console.Out.WriteLine("");
					System.Console.Out.WriteLine("Primary packet decode was successful");
					primaryPacket.Print("primaryPacket");
					System.Console.Out.WriteLine("");
					System.Console.Out.WriteLine("Binary trace:");
					decodeBuffer2.BinDump("primaryPacket");
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
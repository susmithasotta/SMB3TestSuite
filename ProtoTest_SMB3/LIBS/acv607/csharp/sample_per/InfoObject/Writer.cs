using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_per.InfoObject
{
	
	public class Writer
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
					else if (args[i].Equals("-o"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
					else if (args[i].Equals("-a"))
						aligned = true;
					else if (args[i].Equals("-u"))
						aligned = false;
					else if (args[i].Equals("-notrace"))
						trace = false;
					else
					{
						System.Console.Out.WriteLine("usage: Writer [ -a | -u ] [ -v ] " + "[ -o <filename>");
						System.Console.Out.WriteLine("   -a  PER aligned encoding (default)");
						System.Console.Out.WriteLine("   -u  PER unaligned encoding");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -o <filename>  " + "write encoded msg to <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			try
			{
				// Populate and encode Cause data type
				
				Asn1PerEncodeBuffer encodeBuffer1 = new Asn1PerEncodeBuffer(aligned);
				
				if (trace)
				{
					encodeBuffer1.TraceHandler.Enable();
				}
				
				Cause cause = new Cause();
				cause.Set_radioNetwork(new CauseRadioNetwork(CauseRadioNetwork.relocation_triggered));
				
				cause.Encode(encodeBuffer1);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					encodeBuffer1.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer1.BinDump("cause");
					System.Console.Out.WriteLine("");
				}
				
				// Populate ReleaseCommand data type
				
				ProtocolIE_Container container = new ProtocolIE_Container(1);
				
				container.elements[0] = 
               new ProtocolIE_Field(12345, Criticality.notify, 
               new Asn1OpenType(encodeBuffer1.Buffer, 0, encodeBuffer1.MsgByteCnt));
				
				Iu_ReleaseCommand releaseCommand = new Iu_ReleaseCommand(container);
				
				// Encode
				
				Asn1PerEncodeBuffer encodeBuffer2 = new Asn1PerEncodeBuffer(aligned);
				
				if (trace)
				{
					encodeBuffer2.TraceHandler.Enable();
				}
				
				releaseCommand.Encode(encodeBuffer2);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					encodeBuffer2.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer2.BinDump("releaseCommand");
				}
				
				// Write the encoded record to a file
				
				encodeBuffer2.Write(new System.IO.FileStream(filename, System.IO.FileMode.Create));
				
				// Generate a dump file for comparisons
				
				System.String fileSuffix = (aligned)?"a":"u";
				System.IO.StreamWriter messagedmp = new System.IO.StreamWriter(new System.IO.FileStream("message"+ fileSuffix +".dmp", System.IO.FileMode.Create));
				messagedmp.AutoFlush = true;
				encodeBuffer2.HexDump(messagedmp);
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
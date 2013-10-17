using System;
using Com.Objsys.Asn1.Runtime;
using Sample_ber.SimpleROSE;
namespace Sample_ber.CSTA
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
			
			MakeCallArgument makeCallArgument = new MakeCallArgument();
			
			makeCallArgument.callingDevice = new DeviceID();
			makeCallArgument.callingDevice.Set_dialingNumber(new NumberDigits("555-1212"));
			
			makeCallArgument.calledDirectoryNumber = new CalledDeviceID();
			makeCallArgument.calledDirectoryNumber.Set_notRequired();
			
			// Create a message buffer object and encode the record
			
			Asn1BerEncodeBuffer encodeBuffer = new Asn1BerEncodeBuffer();
			
			try
			{
				makeCallArgument.Encode(encodeBuffer, true);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					encodeBuffer.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer.BinDump();
				}
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(e.Message);
				Asn1Util.WriteStackTrace(e, Console.Error);
				return ;
			}
			
			// Create a ROSE Invoke PDU object to wrap the encoded call argument. 
			// Note the optimization with the open type.  By constructing the 
			// object using only the length, a copy of the already-encoded 
			// message component is avoided..
			
			Asn1OpenType openType = new Asn1OpenType(encodeBuffer);
			
			RosePDU rosePDU = new RosePDU();
			rosePDU.Set_invokePDU(new InvokePDU(new InvokeIDType(1), null, new Asn1Integer(10), openType)); // encoded argument
			
			// Encode the final message
			
			try
			{
				rosePDU.Encode(encodeBuffer, true);
				
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
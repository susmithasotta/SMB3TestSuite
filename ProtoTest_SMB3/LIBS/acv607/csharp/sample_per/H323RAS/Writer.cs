using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_per.H323RAS
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
			
			// Create a data object and populate it with the data to be encoded
			
			int[] protocolID = new int[]{0, 0, 8, 2250, 0, 2};
			byte[] ip = new byte[]{(byte) (0xc0), (byte) (0xa8), (byte) 0x00,
            (byte) 0x01};
			
			RegistrationConfirm registrationConfirm = new RegistrationConfirm();
			
			registrationConfirm.requestSeqNum = new RequestSeqNum(1);
			registrationConfirm.protocolIdentifier = 
            new ProtocolIdentifier(protocolID);
			
			registrationConfirm.callSignalAddress = new _SeqOfTransportAddress(1);
			registrationConfirm.callSignalAddress.elements[0] = 
            new TransportAddress();
			registrationConfirm.callSignalAddress.elements[0].Set_ipAddress(
            new TransportAddress_ipAddress(ip, 1720));
			
			registrationConfirm.terminalAlias = new _SeqOfAliasAddress(1);
			registrationConfirm.terminalAlias.elements[0] = new AliasAddress();
			registrationConfirm.terminalAlias.elements[0].Set_h323_ID(
            new Asn1BMPString("anH323ID"));
			
			registrationConfirm.gatekeeperIdentifier = 
            new GatekeeperIdentifier("aGatekeeperID");
			
			registrationConfirm.endpointIdentifier = 
            new EndpointIdentifier("anEndpointID");
			
			RasMessage rasMessage = new RasMessage();
			rasMessage.Set_registrationConfirm(registrationConfirm);
			
			// Create a message buffer object 
			
			Asn1PerEncodeBuffer encodeBuffer = new Asn1PerEncodeBuffer(aligned);
			
			// Enable bit field tracing
			
			if (trace)
			{
				encodeBuffer.TraceHandler.Enable();
			}
			
			// Encode
			
			try
			{
				rasMessage.Encode(encodeBuffer);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					encodeBuffer.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer.BinDump("rasMessage");
				}
				
				// Write the encoded record to a file
				
				encodeBuffer.Write(new System.IO.FileStream(filename, System.IO.FileMode.Create));
				
				// Generate a dump file for comparisons
				
				System.String fileSuffix = (aligned)?"a":"u";
				System.IO.StreamWriter messagedmp = new System.IO.StreamWriter(new System.IO.FileStream("message"+ fileSuffix +".dmp", System.IO.FileMode.Create));
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
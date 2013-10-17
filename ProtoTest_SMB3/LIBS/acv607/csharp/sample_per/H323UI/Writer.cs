using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_per.H323UI
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
			byte[] guid = new byte[16];
			for (int i = 0; i < guid.Length; i++)
			{
				guid[i] = (byte) (i + 1);
			}
			
			Setup_UUIE setup = new Setup_UUIE();
			setup.protocolIdentifier = new ProtocolIdentifier(protocolID);
			setup.sourceInfo = new EndpointType();
			setup.sourceInfo.mc = Asn1Boolean.FALSE_VALUE;
			setup.sourceInfo.undefinedNode = Asn1Boolean.FALSE_VALUE;
			setup.activeMC = Asn1Boolean.FALSE_VALUE;
			setup.conferenceID = new ConferenceIdentifier(guid);
			setup.conferenceGoal = new Setup_UUIE_conferenceGoal();
			setup.conferenceGoal.Set_create();
			setup.callType = new CallType();
			setup.callType.Set_pointToPoint();
			setup.callIdentifier = new CallIdentifier(guid);
			setup.mediaWaitForConnect = Asn1Boolean.FALSE_VALUE;
			setup.canOverlapSend = Asn1Boolean.FALSE_VALUE;
			
			H323_UserInformation userInfo = new H323_UserInformation();
			userInfo.h323_uu_pdu = new H323_UU_PDU();
			userInfo.h323_uu_pdu.h323_message_body = new H323_UU_PDU_h323_message_body();
			userInfo.h323_uu_pdu.h323_message_body.Set_setup(setup);
			
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
				userInfo.Encode(encodeBuffer);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					encodeBuffer.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer.BinDump("userInfo");
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
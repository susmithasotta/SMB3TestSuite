using System;
using Com.Objsys.Asn1.Runtime;
using Sample_ber.AIN;
using Sample_ber.TCAP;
namespace Sample_ber.TcapInvoke
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
			
			CallInfoFromResource_ARGUMENT param = new CallInfoFromResource_ARGUMENT();
			
			// IPReturnBlock
			
			byte[] ipData = new byte[120];
			for (int i = 0; i < 120; i++)
			{
				ipData[i] = (byte) i;
			}
			param.iPReturnBlock = new IPReturnBlock(ipData);
			
			// Amp1
			
			byte[] amp1Data = new byte[6];
			for (int i = 0; i < 6; i++)
			{
				amp1Data[i] = (byte) i;
			}
			param.amp1 = new Amp1(amp1Data);
			
			// Amp2
			
			try
			{
				param.amp2 = new Amp2();
				param.amp2.ampAINNodeID = new AmpAINNodeID();
				param.amp2.ampAINNodeID.Set_spcID(new SpcID("'123'"));
				param.amp2.ampCLogSeqNo = new AmpCLogSeqNo(12345);
				param.amp2.ampCLogRepInd = new AmpCLogRepInd(AmpCLogRepInd.requestReport);
				param.amp2.ampCallProgInd = new AmpCallProgInd(AmpCallProgInd.callProgressVoiceAnnouncements);
				param.amp2.ampTestReqInd = new AmpTestReqInd(111);
				param.amp2.ampCLogName = new AmpCLogName("'TestUser'");
			}
			catch (Asn1ValueParseException e)
			{
				System.Console.Out.WriteLine(e.Message);
				Asn1Util.WriteStackTrace(e, Console.Error);
				return ;
			}
			
			// Encode argument
			
			Asn1BerEncodeBuffer encodeBuffer = new Asn1BerEncodeBuffer();
			
			try
			{
				param.Encode(encodeBuffer, true);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					encodeBuffer.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer.BinDump();
					System.Console.Out.WriteLine("---");
				}
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(e.Message);
				Asn1Util.WriteStackTrace(e, Console.Error);
				return ;
			}
			
			// Create a TCAP Invoke PDU object to wrap the encoded argument. 
			// Note the optimization with the open type.  By constructing the 
			// object using only the length, a copy of the already-encoded 
			// message component is avoided..
			
			Asn1OpenType openType = new Asn1OpenType(encodeBuffer);
			
			Invoke invoke = new Invoke();
			invoke.invokeID = new InvokeIdType(1);
			invoke.operationCode = new Asn1Integer(_AIN_OperationValues.callInfoFromResource);
			invoke.parameter = openType;
			
			Undirectional undirectional = new Undirectional();
			undirectional.components = new ComponentPortion(1);
			undirectional.components.elements[0] = new Component();
			undirectional.components.elements[0].Set_invoke(invoke);
			
			MessageType messageType = new MessageType();
			messageType.Set_undirectional(undirectional);
			
			// Encode TCAP header
			
			try
			{
				messageType.Encode(encodeBuffer, true);
				
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
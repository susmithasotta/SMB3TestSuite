using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_per.T38
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
			
			byte[] testData1 = new byte[]{(byte) 0x01, (byte) 0x02, (byte) 0x03,
            (byte) 0x04, (byte) 0x05};
			byte[] testData2 = new byte[]{(byte) 0x11, (byte) 0x22, (byte) 0x33};
			byte[] testData3 = new byte[]{(byte) (0xaa), (byte) (0xbb), 
            (byte) (0xcc), (byte) (0xdd)};
			byte[] testData4 = new byte[]{(byte) (0xee), (byte) (0xff)};
			
			try
			{
				IFPPacket ifpPacket = new IFPPacket();
				
				ifpPacket.type_of_msg = new Type_of_msg();
				ifpPacket.type_of_msg.Set_data(new Type_of_msg_data(Type_of_msg_data.v29_9600));
				
				ifpPacket.data_field = new Data_Field(4);
				
				ifpPacket.data_field.elements[0] = new Data_Field_element(
               Data_Field_element_field_type.hdlc_sig_end, testData1);
				
				ifpPacket.data_field.elements[1] = new Data_Field_element(
               Data_Field_element_field_type.hdlc_fcs_OK, testData2);
				
				ifpPacket.data_field.elements[2] = new Data_Field_element(
               Data_Field_element_field_type.t4_non_ecm_data, testData3);
				
				ifpPacket.data_field.elements[3] = new Data_Field_element(
               Data_Field_element_field_type.t4_non_ecm_sig_end, testData4);
				
				// Create a message buffer object 
				
				Asn1PerEncodeBuffer encodeBuffer1 = new Asn1PerEncodeBuffer(aligned);
				
				// Enable bit field tracing
				
				if (trace)
				{
					encodeBuffer1.TraceHandler.Enable();
				}
				
				// Encode
				
				ifpPacket.Encode(encodeBuffer1);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					encodeBuffer1.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer1.BinDump("ifpPacket");
				}
				
				// Populate and encode some secondary packet structures
				
				IFPPacket secPacket1 = new IFPPacket();
				
				secPacket1.type_of_msg = new Type_of_msg();
				secPacket1.type_of_msg.Set_data(new Type_of_msg_data(Type_of_msg_data.v27_2400));
				
				secPacket1.data_field = new Data_Field(1);
				
				secPacket1.data_field.elements[0] = new Data_Field_element(Data_Field_element_field_type.hdlc_sig_end, testData4);
				
				// Encode
				
				Asn1PerEncodeBuffer encodeBuffer2 = new Asn1PerEncodeBuffer(aligned);
				if (trace)
				{
					encodeBuffer2.TraceHandler.Enable();
				}
				
				secPacket1.Encode(encodeBuffer2);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					encodeBuffer2.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer2.BinDump("secPacket1");
				}
				
				// Populate a 2nd secondary packet structure
				
				IFPPacket secPacket2 = new IFPPacket();
				
				secPacket2.type_of_msg = new Type_of_msg();
				secPacket2.type_of_msg.Set_data(new Type_of_msg_data(Type_of_msg_data.v17_14400));
				
				secPacket2.data_field = new Data_Field(1);
				
				secPacket2.data_field.elements[0] = new Data_Field_element(Data_Field_element_field_type.hdlc_sig_end);
				
				// Encode
				
				Asn1PerEncodeBuffer encodeBuffer3 = new Asn1PerEncodeBuffer(aligned);
				if (trace)
				{
					encodeBuffer3.TraceHandler.Enable();
				}
				
				secPacket2.Encode(encodeBuffer3);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					encodeBuffer3.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer3.BinDump("secPacket2");
				}
				
				// Populate UDPTL packet structure
				
				UDPTLPacket udptlPacket = new UDPTLPacket();
				
				udptlPacket.seq_number = new Asn1Integer(1);
				
				udptlPacket.primary_ifp_packet = new Asn1OpenType(
               encodeBuffer1.Buffer, 0, encodeBuffer1.MsgByteCnt);
				
				UDPTLPacket_error_recovery_secondary_ifp_packets secIFPPkts = 
               new UDPTLPacket_error_recovery_secondary_ifp_packets(2);
				
				secIFPPkts.elements[0] = new Asn1OpenType(
               encodeBuffer2.Buffer, 0, encodeBuffer2.MsgByteCnt);
				
				secIFPPkts.elements[1] = new Asn1OpenType(
               encodeBuffer3.Buffer, 0, encodeBuffer3.MsgByteCnt);
				
				udptlPacket.error_recovery = new UDPTLPacket_error_recovery();
				udptlPacket.error_recovery.Set_secondary_ifp_packets(secIFPPkts);
				
				// Encode
				
				Asn1PerEncodeBuffer encodeBuffer4 = new Asn1PerEncodeBuffer(aligned);
				if (trace)
				{
					encodeBuffer4.TraceHandler.Enable();
					System.Console.Out.WriteLine("");
					System.Console.Out.WriteLine("Encoding UDPTLPacket..");
					System.Console.Out.WriteLine("");
				}
				
				udptlPacket.Encode(encodeBuffer4);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					encodeBuffer4.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer4.BinDump("udptlPacket");
				}
				
				// Write the encoded record to a file
				
				encodeBuffer4.Write(new System.IO.FileStream(filename, System.IO.FileMode.Create));
				
				// Generate a dump file for comparisons
				
				System.String fileSuffix = (aligned)?"a":"u";
				System.IO.StreamWriter messagedmp = new System.IO.StreamWriter(new System.IO.FileStream("message"+ fileSuffix +".dmp", System.IO.FileMode.Create));
				messagedmp.AutoFlush = true;
				encodeBuffer4.HexDump(messagedmp);
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
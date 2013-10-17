using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_ber.SnmpV2
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
			
			// Populate variable bindings structure:
			// This is a GetResponse for the UDB MIB returning the 
			// the udpLocalAddress field 
			
			int[] udpLocalAddressIdx = new int[]{192, 180, 140, 202, 520};
			ObjectName udpLocalAddressName = new ObjectName(_UDP_MIBValues.udpLocalAddress);
			udpLocalAddressName.Append(udpLocalAddressIdx);
			
			byte[] ipAddress = new byte[] {
            (byte)135, (byte)180, (byte)140, (byte)202 };
			
			ApplicationSyntax applicationSyntax = new ApplicationSyntax();
			applicationSyntax.Set_ipAddress_value(new IpAddress(ipAddress));
			
			ObjectSyntax objectSyntax = new ObjectSyntax();
			objectSyntax.Set_application_wide(applicationSyntax);
			
			VarBind_aChoice udpLocalAddressValue = new VarBind_aChoice();
			udpLocalAddressValue.Set_value_(objectSyntax);
			
			VarBindList varBindList = new VarBindList(1);
			varBindList.elements[0] = new VarBind(udpLocalAddressName, udpLocalAddressValue);
			
			// Populate get_response PDU structure
			
			PDUs pdus = new PDUs();
			pdus.Set_response(new Response_PDU(1827802204, 0, 0, varBindList));
			
			// Populate main message structure
			
			Asn1OctetString community;
			try
			{
				community = new Asn1OctetString("public");
			}
			catch (Asn1ValueParseException e)
			{
				System.Console.Out.WriteLine(e.Message);
            System.Console.Error.Write(e.StackTrace);
            System.Console.Error.Flush();
				return ;
			}
			
			Message snmpMessage = new Message(new Message_version(Message_version.version_1), community, pdus);
			
			// Encode 
			
			Asn1BerEncodeBuffer encodeBuffer = new Asn1BerEncodeBuffer();
			
			try
			{
				snmpMessage.Encode(encodeBuffer, true);
				
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
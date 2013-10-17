using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_ber.SnmpV1
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
			// Set up to retrieve sysDescr object..
			
			SimpleSyntax simpleSyntax = new SimpleSyntax();
			simpleSyntax.Set_empty();
			
			ObjectSyntax objectSyntax = new ObjectSyntax();
			objectSyntax.Set_simple(simpleSyntax);
			
			int[] sysDescrIdx = new int[]{0};
			ObjectName sysDescr = new ObjectName(_RFC1213_MIBValues.sysDescr);
			sysDescr.Append(sysDescrIdx);
			
			VarBindList varBindList = new VarBindList(1);
			varBindList.elements[0] = new VarBind(sysDescr, objectSyntax);
			
			// Populate get_request PDU structure
			
			PDUs pdus = new PDUs();
			pdus.Set_get_request(new GetRequest_PDU(1, 0, 0, varBindList));
			
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
					System.Console.Out.WriteLine("---");
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
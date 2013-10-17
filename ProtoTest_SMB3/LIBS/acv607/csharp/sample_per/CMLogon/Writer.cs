using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_per.CMLogon
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
				// Populate CMLogonRequest structure
				
				CMLogonRequest cmLogonRequest = new CMLogonRequest();
				
				cmLogonRequest.aircraftFlightIdentification = 
               new AircraftFlightIdentification("UA901");
				
				cmLogonRequest.cMLongTSAP = new LongTsap(
               new Asn1OctetString("AB901"), 
               new ShortTsap(null, new Asn1OctetString("4440900901")));
				
				cmLogonRequest.facilityDesignation = 
               new FacilityDesignation("KIADIZDS");
				
				// Plug structure into CMAircraftMessage variable
				
				CMAircraftMessage cmAircraftMessage = new CMAircraftMessage();
				cmAircraftMessage.Set_cmLogonRequest(cmLogonRequest);
				
				// Create a message buffer object 
				
				Asn1PerEncodeBuffer cmEncodeBuffer =
               new Asn1PerEncodeBuffer(aligned);
				
				// Enable bit field tracing
				
				if (trace)
				{
					cmEncodeBuffer.TraceHandler.Enable();
				}
				
				// Encode
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding CM message..");
					System.Console.Out.WriteLine("");
				}
				
				cmAircraftMessage.Encode(cmEncodeBuffer);
				
				if (trace)
				{
					System.Console.Out.WriteLine("CM Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					cmEncodeBuffer.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					cmEncodeBuffer.BinDump("cmAircraftMessage");
				}
				
				// Populate AARQ PDU structure
				
				int[] applCtxtNameOID = new int[]{1, 3, 27, 3, 1};
				int[] callingAPTitleOID = new int[]{1, 3, 27, 1, 500, 0};
				
				AARQ_apdu aarq = new AARQ_apdu();
				
				aarq.application_context_name = 
               new Application_context_name(applCtxtNameOID);
				
				aarq.calling_AP_title = new AP_title();
				aarq.calling_AP_title.Set_ap_title_form2(
               new AP_title_form2(callingAPTitleOID));
				
				aarq.calling_AE_qualifier = new AE_qualifier();
				aarq.calling_AE_qualifier.Set_ae_qualifier_form2(
               new AE_qualifier_form2(1));
				
				// Populate external type with info in the encoded request and plug 
				// into the structure..
				
				Asn1BitString externalData = new Asn1BitString();
				externalData.numbits = cmEncodeBuffer.MsgBitCnt;
				externalData.mValue = cmEncodeBuffer.Buffer;
				
				aarq.user_information = new Association_information(1);
				Asn1External_encoding ee = new Asn1External_encoding();
				ee.Set_arbitrary(externalData);
				aarq.user_information.elements[0] = 
               new Asn1External(null, null, null, ee);
				
				// Populate the ACSE structure and encode
				
				ACSE_apdu acse = new ACSE_apdu();
				acse.Set_aarq(aarq);
				
				if (trace)
				{
					System.Console.Out.WriteLine("");
					System.Console.Out.WriteLine("Encoding ACSE AARQ APDU..");
					System.Console.Out.WriteLine("");
				}
				
				Asn1PerEncodeBuffer acseEncodeBuffer = 
               new Asn1PerEncodeBuffer(aligned);
				
				if (trace)
				{
					acseEncodeBuffer.TraceHandler.Enable();
				}
				
				acse.Encode(acseEncodeBuffer);
				
				if (trace)
				{
					System.Console.Out.WriteLine("ACSE Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					acseEncodeBuffer.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					acseEncodeBuffer.BinDump("acse");
				}
				
				// Write the encoded record to a file
				
				acseEncodeBuffer.Write(new System.IO.FileStream(filename, System.IO.FileMode.Create));
				
				// Generate a dump file for comparisons
				
				System.String fileSuffix = (aligned)?"a":"u";
				System.IO.StreamWriter messagedmp = new System.IO.StreamWriter(new System.IO.FileStream("message"+ fileSuffix +".dmp", System.IO.FileMode.Create));
				messagedmp.AutoFlush = true;
				acseEncodeBuffer.HexDump(messagedmp);
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
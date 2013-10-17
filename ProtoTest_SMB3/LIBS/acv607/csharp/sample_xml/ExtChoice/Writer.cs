using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_xml.ExtChoice
{
	
	public class Writer
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.xml").ToString();
			int tvalue = 2;
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
						System.Console.Out.WriteLine("usage: Writer [ -cxer ] [ -v ] " + "[ -o <filename>");
						System.Console.Out.WriteLine("   -cxer  Use Canonical XER");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -o <filename>  " + "write encoded msg to <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			// Create a data object and populate it with the data to be encoded
			AliasAddressList aliasAddressList = new AliasAddressList(9);
			
			for (tvalue = 1; tvalue <= aliasAddressList.elements.Length; tvalue++)
			{
				AliasAddress aliasAddress = new AliasAddress();
				
				switch (tvalue)
				{
					
					case 1: 
						aliasAddress.Set_e164(new Asn1IA5String("#39047752,937"));
						break;
					
					case 2: 
						aliasAddress.Set_h323_ID(new Asn1BMPString("TESTSTRING"));
						break;
					
					case 3: 
						aliasAddress.Set_aNull();
						break;
					
					case 4: 
						aliasAddress.Set_url_ID(new Asn1IA5String("http://www.obj-sys.com"));
						break;
					
					case 5: 
						aliasAddress.Set_transportID(new AliasAddress_transportID("Truck", 111));
						break;
					
					case 6: 
						aliasAddress.Set_email_ID(new Asn1IA5String("objective@obj-sys.com"));
						break;
					
					case 7: 
						aliasAddress.Set_partyNumber(new PartyNumber(555));
						break;
					
					case 8: 
						aliasAddress.Set_boolVal(new Asn1Boolean(true));
						break;
					
					case 9: 
						aliasAddress.Set_enumVal(new EnumVal(EnumVal.two));
						break;
					
					default: 
						System.Console.Out.WriteLine("invalid element number " + tvalue);
						return ;
					
				}
				aliasAddressList.elements[tvalue - 1] = aliasAddress;
			}
			if (trace)
			{
				System.Console.Out.WriteLine("The following record will be encoded:");
				aliasAddressList.Print("aliasAddressList");
				System.Console.Out.WriteLine("");
			}
			
			// Create a message buffer object 
			try
			{
				Asn1XmlEncodeBuffer encodeBuffer = new Asn1XmlEncodeBuffer();
				
				aliasAddressList.Encode(encodeBuffer);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					encodeBuffer.Write(System.Console.OpenStandardOutput());
				}
				
				// Write the encoded record to a file
				
				encodeBuffer.Write(new System.IO.FileStream(filename, System.IO.FileMode.Create));
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
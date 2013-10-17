using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_ber.TAP0311Batch
{
	
	public class Reader
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
					else if (args[i].Equals("-i"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
					else if (args[i].Equals("-notrace"))
						trace = false;
					else
					{
						System.Console.Out.WriteLine("usage: Reader [ -v ] [ -i <filename> ]");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -i <filename>  " + "read encoded msg from <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			try
			{
				// Create an input file stream object
				
				System.IO.FileStream ins = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				
				// Create a decode buffer object
				
				Asn1BerDecodeBuffer buffer = new Asn1BerDecodeBuffer(ins);
				
				/* Loop to read and process each record in the batch file */
				
				Asn1Tag tag = new Asn1Tag();
				buffer.Mark();
				int len = buffer.DecodeTagAndLength(tag);
				
				if (tag.Equals(Asn1Tag.APPL, Asn1Tag.CONS, 1))
				{
					buffer.Reset();
					DecodeTransferBatch(buffer, true, len, trace);
				}
				else if (tag.Equals(Asn1Tag.APPL, Asn1Tag.CONS, 2))
				{
					buffer.Reset();
					Notification notification = new Notification();
					notification.Decode(buffer, true, len);
					if (trace)
					{
						notification.Print("notification");
					}
				}
				else
				{
					throw new Asn1InvalidChoiceOptionException(buffer, tag);
				}
				
				if(trace) {
               System.Console.Out.WriteLine("Decode was successful");
            }
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(e.Message);
				Asn1Util.WriteStackTrace(e, Console.Error);
				return ;
			}
		}
		
		public static void  DecodeTransferBatch(Asn1BerDecodeBuffer buffer, bool explicitTagging, int implicitLength, bool trace)
		{
			BatchControlInfo batchControlInfo; // optional
			AccountingInfo accountingInfo; // optional
			NetworkInfo networkInfo; // optional
			MessageDescriptionInfoList messageDescriptionInfo; // optional
			AuditControlInfo auditControlInfo; // optional
			
			int llen = (explicitTagging)?matchTag(buffer, TransferBatch._TAG):implicitLength;
			
			// decode SEQUENCE
			
			Asn1BerDecodeContext context = new Asn1BerDecodeContext(buffer, llen);
			
			IntHolder elemLen = new IntHolder();
			
			// decode batchControlInfo
			
			if (context.MatchElemTag(Asn1Tag.APPL, Asn1Tag.CONS, 4, elemLen, false))
			{
				batchControlInfo = new BatchControlInfo();
				batchControlInfo.Decode(buffer, true, elemLen.mValue);
				if (trace)
				{
					batchControlInfo.Print("batchControlInfo");
				}
			}
			
			// decode accountingInfo
			
			if (context.MatchElemTag(Asn1Tag.APPL, Asn1Tag.CONS, 5, elemLen, false))
			{
				accountingInfo = new AccountingInfo();
				accountingInfo.Decode(buffer, true, elemLen.mValue);
				if (trace)
				{
					accountingInfo.Print("accountingInfo");
				}
			}
			
			// decode networkInfo
			
			if (context.MatchElemTag(Asn1Tag.APPL, Asn1Tag.CONS, 6, elemLen, false))
			{
				networkInfo = new NetworkInfo();
				networkInfo.Decode(buffer, true, elemLen.mValue);
				if (trace)
				{
					networkInfo.Print("networkInfo");
				}
			}
			
			// decode messageDescriptionInfo
			
			if (context.MatchElemTag(Asn1Tag.APPL, Asn1Tag.CONS, 8, elemLen, false))
			{
				messageDescriptionInfo = new MessageDescriptionInfoList();
				messageDescriptionInfo.Decode(buffer, true, elemLen.mValue);
				if (trace)
				{
					messageDescriptionInfo.Print("messageDescriptionInfo");
				}
			}
			
			// decode callEventDetails
			
			if (context.MatchElemTag(Asn1Tag.APPL, Asn1Tag.CONS, 3, elemLen, false))
			{
				DecodeCallEventDetailList(buffer, true, elemLen.mValue, trace);
			}
			
			// decode auditControlInfo
			
			if (context.MatchElemTag(Asn1Tag.APPL, Asn1Tag.CONS, 15, elemLen, false))
			{
				auditControlInfo = new AuditControlInfo();
				auditControlInfo.Decode(buffer, true, elemLen.mValue);
				if (trace)
				{
					auditControlInfo.Print("auditControlInfo");
				}
			}
			
			if (explicitTagging && llen == Asn1Status.INDEFLEN)
			{
				matchTag(buffer, Asn1Tag.EOC);
			}
		}
		
		public static void  DecodeCallEventDetailList(Asn1BerDecodeBuffer buffer, bool explicitTagging, int implicitLength, bool trace)
		{
			int llen = (explicitTagging)?matchTag(buffer, CallEventDetailList._TAG):implicitLength;
			
			// decode SEQUENCE OF or SET OF
			//LinkedList llist = new LinkedList ();
			Asn1BerDecodeContext context = new Asn1BerDecodeContext(buffer, llen);
			CallEventDetail element;
			int elemLen = 0;
			
			while (!context.Expired())
			{
				element = new CallEventDetail();
				element.Decode(buffer, true, elemLen);
				if (trace)
				{
					element.Print("callEventDetail");
				}
				//llist.add (element);
			}
			
			//CallEventDetail[] elements = new CallEventDetail [llist.size()];
			//llist.toArray (elements);
			
			if (explicitTagging && llen == Asn1Status.INDEFLEN)
			{
				matchTag(buffer, Asn1Tag.EOC);
			}
		}
		
		public static int matchTag(Asn1BerDecodeBuffer buffer, Asn1Tag tag)
		{
			Asn1Tag parsedTag = new Asn1Tag();
			IntHolder parsedLen = new IntHolder();
			if (buffer.MatchTag(tag.mClass, tag.mForm, tag.mIDCode, parsedTag, parsedLen))
			{
				return parsedLen.mValue;
			}
			else
				throw new Asn1TagMatchFailedException(buffer, new Asn1Tag(tag.mClass, tag.mForm, tag.mIDCode), parsedTag);
		}
	}
}

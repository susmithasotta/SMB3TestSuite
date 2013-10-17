using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_ber.TAP0311
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
						System.Console.Out.WriteLine("   -o <filename>  " + "write Encoded msg to <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			// Create a data object and populate it with the data to be Encoded
			
			Asn1BerOutputStream outs = null;
			
			try
			{
				outs = new Asn1BerOutputStream(new System.IO.FileStream(filename, System.IO.FileMode.Create));
				
				outs.EncodeTagAndIndefLen(TransferBatch._TAG);
				
				Encode_BatchControlInfo(outs);
				Encode_AccountingInfo(outs);
				Encode_NetworkInfo(outs);
				Encode_MessageDescriptionInfoList(outs);
				
				/* The call detail list is handled differently.  It is possible to   */
				/* have a very large number of these records which would make        */
				/* encoding the whole list in memory difficult.  The solution is to  */
				/* drop another indefinite tag/length into the file and then Encode  */
				/* and write each call detail record.  See this routine for further  */
				/* details..                                                         */
				
				Encode_CallDetailList(outs);
				
				Encode_AuditControlInfo(outs);
				
				outs.EncodeEOC();
				
				if (trace)
				{
					outs.Close();
					outs = null;
					
					Asn1BerDecodeBuffer decodeBuffer = new Asn1BerDecodeBuffer(new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read));
					
					try
					{
                  System.IO.StreamWriter messagedmp = new System.IO.StreamWriter(System.Console.OpenStandardOutput());
                  messagedmp.AutoFlush = true;
						decodeBuffer.Parse(new Asn1BerMessageDumpHandler(messagedmp));
					}
					catch (System.Exception e)
					{
						System.Console.Out.WriteLine(e.Message);
						Asn1Util.WriteStackTrace(e, Console.Error);
					}
				}
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(e.Message);
				Asn1Util.WriteStackTrace(e, Console.Error);
				return ;
			}
			finally
			{
				try
				{
					if (outs != null)
						outs.Close();
				}
				catch (System.Exception)
				{
				}
			}
		}
		
		internal static void  Encode_BatchControlInfo(Asn1BerOutputStream outs)
		{
			BatchControlInfo bci = new BatchControlInfo();
			
			bci.sender = new Asn1VisibleString("USA27");
			bci.recipient = new Asn1VisibleString("DEUD2");
			bci.fileSequenceNumber = new Asn1NumericString("00272");
			
			bci.fileCreationTimeStamp = new DateTimeLong(new Asn1NumericString("20000307080756"), new Asn1VisibleString("-0500"));
			bci.transferCutOffTimeStamp = new DateTimeLong(new Asn1NumericString("20000307080751"), new Asn1VisibleString("-0500"));
			bci.fileAvailableTimeStamp = new DateTimeLong(new Asn1NumericString("20000307080756"), new Asn1VisibleString("-0500"));
			
			bci.specificationVersionNumber = new Asn1Integer(3);
			bci.releaseVersionNumber = new Asn1Integer(1);
			
			bci.Encode(outs, true);
		}
		
		internal static void  Encode_AccountingInfo(Asn1BerOutputStream outs)
		{
			AccountingInfo accountingInfo = new AccountingInfo();
			
			accountingInfo.taxation = new TaxationList(2);
			
			accountingInfo.taxation.elements[0] = new Taxation(new Asn1Integer(0),
            new Asn1VisibleString("03"), new Asn1NumericString("0225343"),
            new Asn1NumericString(""));
            
			accountingInfo.taxation.elements[1] = new Taxation(new Asn1Integer(1),
            new Asn1VisibleString("02"), new Asn1NumericString("0600343"),
            new Asn1NumericString(""));
			
			accountingInfo.localCurrency = new Asn1VisibleString("USD");
			accountingInfo.tapCurrency = new Asn1VisibleString("USD");
			
			accountingInfo.currencyConversionInfo = new CurrencyConversionList(1);
			accountingInfo.currencyConversionInfo.elements[0] = new CurrencyConversion(0L, 6L, 1000000L);
			
			accountingInfo.tapDecimalPlaces = new Asn1Integer(2);
			
			accountingInfo.Encode(outs, true);
		}
		
		internal static void  Encode_NetworkInfo(Asn1BerOutputStream outs)
		{
			NetworkInfo networkInfo = new NetworkInfo();
			
			networkInfo.utcTimeOffsetInfo = new UtcTimeOffsetInfoList(1);
			networkInfo.utcTimeOffsetInfo.elements[0] = new UtcTimeOffsetInfo(new Asn1Integer(0), new Asn1VisibleString("-0500"));
			
			networkInfo.recEntityInfo = new RecEntityInfoList(3);
			networkInfo.recEntityInfo.elements[0] = 
            new RecEntityInformation(0, 1, "");
			// networkInfo.recEntityInfo.elements[0].recEntityId.Set_msisdn(
         //   new Asn1OctetString(Asn1Util.StringToBCD("80952416046")));
			
			networkInfo.recEntityInfo.elements[1] = new RecEntityInformation(1, 1, "");
			//networkInfo.recEntityInfo.elements[1].recEntityId.Set_msisdn(new Asn1OctetString(Asn1Util.StringToBCD("80952416046")));
			
			networkInfo.recEntityInfo.elements[2] = new RecEntityInformation(2, 1, "");
			//networkInfo.recEntityInfo.elements[2].recEntityId.Set_msisdn(new Asn1OctetString(Asn1Util.StringToBCD("80952416046")));
			
			/* networkInfo.calledNumAnalysis = new CalledNumAnalysisList(1);
			networkInfo.calledNumAnalysis.elements[0] = new CalledNumAnalysis(null, new CountryCodeList(1), new IacList(1));
			networkInfo.calledNumAnalysis.elements[0].countryCodeTable.elements[0] = new Asn1OctetString("1");
			networkInfo.calledNumAnalysis.elements[0].iacTable.elements[0] = new Asn1OctetString("011"); */
			
			networkInfo.Encode(outs, true);
		}
		
		/* internal static void  Encode_VasInfoList(Asn1BerOutputStream outs)
		{
			VasInfoList vasInfo = new VasInfoList(1);
			
			vasInfo.elements[0] = new VasInformation(new Asn1Integer(1), new Asn1OctetString("Short"), new Asn1OctetString("Description"));
			
			vasInfo.Encode(outs, true);
		} */
      
		
		internal static void  Encode_MessageDescriptionInfoList(Asn1BerOutputStream outs)
		{
			MessageDescriptionInfoList messageDescriptionInfo = new MessageDescriptionInfoList(1);
			
			messageDescriptionInfo.elements[0] = new MessageDescriptionInformation(new Asn1Integer(1), new Asn1VisibleString("Description"));
			
			messageDescriptionInfo.Encode(outs, true);
		}
		
		/* Encode call detail list.  This part of the sample gets its data from */
		/* a data input file.  This could just as easily be a database or other */
		/* storage device containing large numbers of call detail records.      */
		/* The records are sequentially read and Encoded and written to the     */
		/* output file.                                                         */
		
		internal static void  Encode_CallDetailList(Asn1BerOutputStream outs)
		{
			System.IO.StreamReader reader = new System.IO.StreamReader(new System.IO.StreamReader("input.dat", System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader("input.dat", System.Text.Encoding.Default).CurrentEncoding);
			
			outs.EncodeTagAndIndefLen(CallEventDetailList._TAG);
			
			try
			{
				System.String str;
				while ((System.Object) (str = reader.ReadLine()) != null)
				{
               if(str.Length == 0) { break; }
					if (str.IndexOf("CALL DATA", 0) != - 1)
					{
						Encode_MobileOriginatedCall(reader, outs);
					}
				}
			}
			catch (System.IO.IOException)
			{
			}
			
			reader.Close();
			
			outs.EncodeEOC();
		}
		
		internal static void  Encode_MobileOriginatedCall(System.IO.StreamReader reader, Asn1BerOutputStream outs)
		{
			MobileOriginatedCall mOC = new MobileOriginatedCall();
			CallEventDetail cED = new CallEventDetail();
			
			cED.Set_mobileOriginatedCall(mOC);
			
			/* This procedure expects the call data to be layed out in three     */
			/* lines like the input file provided.  It is not set up to handle   */
			/* any format and is not truely dynamic in nature                    */
			
			mOC.basicCallInformation = new MoBasicCallInformation();
			/* chargeable subscriber */
			mOC.basicCallInformation.chargeableSubscriber = new ChargeableSubscriber();
			mOC.basicCallInformation.chargeableSubscriber.Set_simChargeableSubscriber(new SimChargeableSubscriber(new Asn1OctetString(Asn1Util.StringToBCD("250123000113003")), new Asn1OctetString(Asn1Util.StringToBCD("79024113000"))));
			
			
			/* destination */
			mOC.basicCallInformation.destination = new Destination();
			System.String line = reader.ReadLine();
			Tokenizer toker = new Tokenizer(line, " ");
			System.String calledNumber = toker.NextToken();
			System.String calledPlace = toker.NextToken();
			System.String localTimeStamp = toker.NextToken();
			long timeOffset = System.Int64.Parse(toker.NextToken());
			long totalCall = System.Int64.Parse(toker.NextToken());
			
			mOC.basicCallInformation.destination.calledNumber = new Asn1OctetString(calledNumber);
			mOC.basicCallInformation.destination.calledPlace = new Asn1VisibleString(calledPlace);
			
			/* Call event start time stamp */
			mOC.basicCallInformation.callEventStartTimeStamp = new DateTime(new Asn1NumericString(localTimeStamp), new Asn1Integer(timeOffset));
			
			/* Event Duration */
			mOC.basicCallInformation.totalCallEventDuration = new Asn1Integer(totalCall);
			
			
			/* location information */
			mOC.locationInformation = new LocationInformation();
			mOC.locationInformation.networkLocation = new NetworkLocation();
			
			line = reader.ReadLine();
			toker = new Tokenizer(line, " ");
			long recEntityCode = System.Int64.Parse(toker.NextToken());
			long locationArea = System.Int64.Parse(toker.NextToken());
			long cellId = System.Int64.Parse(toker.NextToken());
			System.String servingBid = toker.NextToken();
			System.String locationDescription = toker.NextToken();
			
			mOC.locationInformation.networkLocation.recEntityCode = new Asn1Integer(recEntityCode);
			mOC.locationInformation.networkLocation.locationArea = new Asn1Integer(locationArea);
			mOC.locationInformation.networkLocation.cellId = new Asn1Integer(cellId);
			
			mOC.locationInformation.geographicalLocation = new GeographicalLocation ();
         mOC.locationInformation.geographicalLocation.servingBid = new Asn1VisibleString(servingBid);
			mOC.locationInformation.geographicalLocation.servingLocationDescription = new Asn1VisibleString(locationDescription);
			
			/* equipment information */
			mOC.equipmentIdentifier = new ImeiOrEsn();
			mOC.equipmentIdentifier.Set_imei(new Asn1OctetString(Asn1Util.StringToBCD("49013810114356")));
			
			/* basic service used list */
			
			mOC.basicServiceUsedList = new BasicServiceUsedList(1);
			BasicServiceUsed bsu = new BasicServiceUsed();
			mOC.basicServiceUsedList.elements[0] = bsu;
			
			bsu.basicService = new BasicService();
			bsu.basicService.serviceCode = new BasicServiceCode();
			
			line = reader.ReadLine();
			toker = new Tokenizer(line, " ");
			System.String teleServiceCode = toker.NextToken();
			long radioChannelRequested = System.Int64.Parse(toker.NextToken());
			long radioChannelUsed = System.Int64.Parse(toker.NextToken());
			System.String chargedItem = toker.NextToken();
			long exchangeRateCode = System.Int64.Parse(toker.NextToken());
			System.String callType = toker.NextToken();
			
			bsu.basicService.serviceCode.Set_teleServiceCode(new Asn1VisibleString(teleServiceCode));
			
			/* build charge information list */
			bsu.chargeInformationList = new ChargeInformationList(1);
			
			ChargeInformation chargeInfo = new ChargeInformation();
			bsu.chargeInformationList.elements[0] = chargeInfo;
			chargeInfo.chargedItem = new Asn1VisibleString(chargedItem);
			chargeInfo.exchangeRateCode = new Asn1Integer(exchangeRateCode);
			chargeInfo.callTypeGroup = new CallTypeGroup(null, null, null);
			
			System.Collections.ArrayList chargeDetailList = new System.Collections.ArrayList();
			
			do 
			{
				/* when you get to the next call this call will read the       */
				/* first two characters of the next call line.  This will be   */
				/* the number before CALL DATA.  Again another short cut.      */
				
				line = reader.ReadLine();
				if ((System.Object) line == null || line.Length == 0)
					break;
				
				toker = new Tokenizer(line, " ");
				try
				{
					System.String chargeType = toker.NextToken();
					long charge = System.Int64.Parse(toker.NextToken());
					long chargeableUnits = System.Int64.Parse(toker.NextToken());
					long chargedUnits = System.Int64.Parse(toker.NextToken());
					System.String dayCategory = toker.NextToken();
					System.String timeBand = toker.NextToken();
					
					
					/* ChargeDetail chargeDetail = new ChargeDetail();
					
					load_ChargeDetail(chargeDetail, chargeType, charge, chargeableUnits, chargedUnits, dayCategory, timeBand);
					
					chargeDetailList.Add(chargeDetail); */
				}
				catch (System.ArgumentOutOfRangeException)
				{
					break;
				}
			}
			while (true);
			
			chargeInfo.chargeDetailList = new ChargeDetailList(chargeDetailList.Count);
			Asn1Util.ToArray(chargeDetailList, chargeInfo.chargeDetailList.elements);
			
			cED.Encode(outs, true);
		}
		
		/* internal static void  load_ChargeDetail(ChargeDetail chargeDetail, System.String chargeType, long charge, long chargeableUnits, long chargedUnits, System.String dayCategory, System.String timeBand)
		{
			chargeDetail.chargeType = new Asn1OctetString(chargeType);
			chargeDetail.charge = new Asn1Integer(charge);
			chargeDetail.chargeableUnits = new Asn1Integer(chargeableUnits);
			chargeDetail.chargedUnits = new Asn1Integer(chargedUnits);
			chargeDetail.dayCategory = new Asn1OctetString(dayCategory);
			chargeDetail.timeBand = new Asn1OctetString(timeBand);
		} */
		
		internal static void  Encode_AuditControlInfo(Asn1BerOutputStream outs)
		{
			AuditControlInfo auditControlInfo = new AuditControlInfo();
			
			auditControlInfo.earliestCallTimeStamp = new DateTimeLong(new Asn1NumericString("20000305203647"), new Asn1VisibleString("-0500"));
			auditControlInfo.latestCallTimeStamp = new DateTimeLong(new Asn1NumericString("20000305222654"), new Asn1VisibleString("-0500"));

			auditControlInfo.totalTaxValue = new Asn1Integer(63);
			auditControlInfo.totalDiscountValue = new Asn1Integer(0);
			auditControlInfo.callEventDetailsCount = new Asn1Integer(7);
			
			auditControlInfo.Encode(outs, true);
		}
	}
}

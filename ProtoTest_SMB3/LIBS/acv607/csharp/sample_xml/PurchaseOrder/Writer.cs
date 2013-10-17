using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_xml.PurchaseOrder
{
   
   public class Writer
   {
      
      [STAThread]
      public static void  Main(System.String[] args)
      {
         
         System.String filename = new System.Text.StringBuilder("message.xml").ToString();
         bool trace = true;
         
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
                  System.Console.Out.WriteLine("usage: Writer [-v] [-o <filename>] [-notrace]");
                  System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
                  System.Console.Out.WriteLine("   -o <filename>  " + "write encoded msg to <filename>");
                  System.Console.Out.WriteLine("   -notrace  do not display trace info");
                  return ;
               }
            }
         }
         
         // Create a message buffer object
         Asn1XmlEncodeBuffer encodeBuffer = new Asn1XmlEncodeBuffer();
         
         try
         {
            // Populate purchase order object
            
            PurchaseOrder purchaseOrder = new PurchaseOrder();
            purchaseOrder.orderDate = new Asn1VisibleString("2003-04-01");
            
            purchaseOrder.shipTo = new USAddress();
            purchaseOrder.shipTo.name = new Asn1UTF8String("Joe Smith");
            purchaseOrder.shipTo.street = new Asn1UTF8String("1 Morning Light Rd");
            purchaseOrder.shipTo.city = new Asn1UTF8String("Glenmoore");
            purchaseOrder.shipTo.state = new Asn1UTF8String("PA");
            purchaseOrder.shipTo.zip = new Asn1Integer(19343);
            
            purchaseOrder.billTo = new USAddress();
            purchaseOrder.billTo.name = new Asn1UTF8String("Bill Jones");
            purchaseOrder.billTo.street = new Asn1UTF8String("102 Pickering Way, Suite 506");
            purchaseOrder.billTo.city = new Asn1UTF8String("Exton");
            purchaseOrder.billTo.state = new Asn1UTF8String("PA");
            purchaseOrder.billTo.zip = new Asn1Integer(19341);
            
            purchaseOrder.comment = new Comment("Need this ASAP!");
            
            purchaseOrder.items = new Items(2);
            purchaseOrder.items.elements[0] = new Items_item();
            purchaseOrder.items.elements[0].partNum = new SKU("111-AA");
            purchaseOrder.items.elements[0].productName = new Asn1UTF8String("Acme Widget");
            purchaseOrder.items.elements[0].quantity = new Asn1Integer(50);
            purchaseOrder.items.elements[0].uSPrice = new Asn1Integer(200);
            
            purchaseOrder.items.elements[1] = new Items_item();
            purchaseOrder.items.elements[1].partNum = new SKU("222-BB");
            purchaseOrder.items.elements[1].productName = new Asn1UTF8String("Wizbang Gadget");
            purchaseOrder.items.elements[1].quantity = new Asn1Integer(99);
            purchaseOrder.items.elements[1].uSPrice = new Asn1Integer(777);
            purchaseOrder.items.elements[1].comment = new Comment("I would like these in the color red");
            purchaseOrder.items.elements[1].shipDate = new Asn1VisibleString("2003-03-26");
            
            // Encode
            
            purchaseOrder.Encode(encodeBuffer);
            
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
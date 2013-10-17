// This is an implementation of a simple print handler class.  It outputs 
// the fields of an encoded BER message to stdout in a structured output 
// format..
using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_per.ToXML
{
	
	class XMLHandler : Asn1NamedEventHandler
	{
		private const int START = 1;
		private const int CHARS = 2;
		private const int END = 3;
		
		protected internal System.String mVarName;
		protected internal int mIndentSpaces = 0;
		protected internal int mLastItem;
		
		public XMLHandler(System.String varName)
		{
			mVarName = varName;
			System.Console.Out.Write("<" + mVarName + ">");
			mLastItem = START;
		}
		
		~XMLHandler()
		{
			System.Console.Out.WriteLine("</" + mVarName + ">");
		}

      public virtual void  StartElement(System.String name, int index)
		{
			if (mLastItem == START)
			{
				System.Console.Out.WriteLine("");
				mIndentSpaces += 3;
			}
			Indent();
			System.Console.Out.Write("<" + name + ">");
			mLastItem = START;
		}
		
		public virtual void  EndElement(System.String name, int index)
		{
			if (mLastItem == END)
			{
				mIndentSpaces -= 3;
				Indent();
			}
			System.Console.Out.WriteLine("</" + name + ">");
			mLastItem = END;
		}
		
		public virtual void  Characters(System.String svalue, short typeCode)
		{
			System.Console.Out.Write(svalue);
			mLastItem = CHARS;
		}
		
		public virtual void  Finished()
		{
			System.Console.Out.WriteLine("</" + mVarName + ">");
		}
		
		private void  Indent()
		{
			for (int i = 0; i < mIndentSpaces; i++)
				System.Console.Out.Write(" ");
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public enum TokenType
    {
        EndOfFile,
        Invalid,
        Unknown,
        Empty,

        Plus, // "+"
        Minus, // "-"
        Multiply, // "*"
        Divide, // "/"
        Mod, // "%"

        ShiftLeft, // "<<"
        ShiftRight, // ">>"

        Equal, // "=="
        NotEqual, //"!="
        Lesser, // "<"
        Greater, // ">"
        LesserOrEqual, // "<="
        GreaterOrEqual, // ">="

        BitXor, // "^"
        BitAnd, // "&"
        BitOr, // "|"

        And, // "&&"
        Or, // "||"

        Conditional, // "?"
        Colon, // ":"

        Integer,
        String,
        Separator,
        SizeOf,
        Bracket,
        Comma,
        Variable,

        Primary,
        Function,

        Not,
        BitNot,

        Negative,
        Positive,

        Dereference, //"*"
    }

    public interface IToken
    {
        TokenType Type
        {
            get;
            set;
        }

        string Text
        {
            get;
            set;
        }
    }
}

//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling;

namespace Microsoft.Protocols.TestTools.StackSdk.Dtyp
{
    /// <summary>
    /// The OLD_LARGE_INTEGER structure is used to represent 
    /// a 64-bit signed integer value as two 32-bit integers.
    /// </summary>
    public partial struct _OLD_LARGE_INTEGER
    {
        /// <summary>
        /// Low-order 32 bits.
        /// </summary>
        public uint LowPart;

        /// <summary>
        /// High-order 32 bits. The sign of this member determines the sign of the 64-bit integer.
        /// </summary>
        public int HighPart;
    }

    /// <summary>
    ///  The LARGE_INTEGER structure is used to represent a 64-bit
    ///  signed integer value.
    /// </summary>
    //  <remarks>
    //   MS-DTYP\e904b1ba-f774-4203-ba1b-66485165ab1a.xml
    //  </remarks>
    public partial struct _LARGE_INTEGER
    {

        /// <summary>
        ///  QuadPart member.
        /// </summary>
        public long QuadPart;
    }

    /// <summary>
    ///  The RPC_UNICODE_STRING structure specifies a Unicode
    ///  string. This structure is defined in IDL as follows:
    /// </summary>
    //  <remarks>
    //   MS-DTYP\94a16bb6-c610-4cb9-8db6-26f15f560061.xml
    //  </remarks>
    public partial struct _RPC_UNICODE_STRING
    {

        /// <summary>
        ///  The length, in bytes, of the string pointed to by the
        ///  Buffer member, not including the terminating null character
        ///  if any. The length MUST be a multiple of 2. The length
        ///  SHOULD equal the entire size of the Buffer, in which
        ///  case there is no terminating null character. Any method
        ///  that accesses this structure MUST use the Length specified
        ///  instead of relying on the presence or absence of a
        ///  null character.
        /// </summary>
        public ushort Length;

        /// <summary>
        ///  The maximum size, in bytes, of the string pointed to
        ///  by Buffer. The size MUST be a multiple of 2. If not,
        ///  the size MUST be decremented by 1 prior to use. This
        ///  value MUST not be less than Length.
        /// </summary>
        public ushort MaximumLength;

        /// <summary>
        ///  A pointer to a string buffer. If MaximumLength is greater
        ///  than zero, the buffer MUST contain a non-null value.
        /// </summary>
        [Length("Length / 2")]
        [Size("MaximumLength / 2")]
        public ushort[] Buffer;
    }

    /// <summary>
    ///  The RPC_SID_IDENTIFIER_AUTHORITY structure is a representation
    ///  of a security identifier (SID), as specified by the
    ///  SID_IDENTIFIER_AUTHORITY structure. This structure
    ///  is defined in IDL as follows.
    /// </summary>
    //  <remarks>
    //   MS-DTYP\d7e6e5a5-437c-41e5-8ba1-bdfd43e96cbc.xml
    //  </remarks>
    public partial struct _RPC_SID_IDENTIFIER_AUTHORITY
    {

        /// <summary>
        ///  Value member.
        /// </summary>
        [Inline()]
        [StaticSize(6, StaticSizeMode.Elements)]
        public byte[] Value;
    }

    /// <summary>
    ///  The RPC_SID structure is a representation of a security
    ///  identifier (SID), as specified by the SID structure.
    ///  This structure is defined in IDL as follows.
    /// </summary>
    //  <remarks>
    //   MS-DTYP\5cb97814-a1c2-4215-b7dc-76d1f4bfad01.xml
    //  </remarks>
    public partial struct _RPC_SID
    {

        /// <summary>
        ///  Revision member.
        /// </summary>
        public byte Revision;

        /// <summary>
        ///  SubAuthorityCount member.
        /// </summary>
        public byte SubAuthorityCount;

        /// <summary>
        ///  IdentifierAuthority member.
        /// </summary>
        public _RPC_SID_IDENTIFIER_AUTHORITY IdentifierAuthority;

        /// <summary>
        ///  SubAuthority member.
        /// </summary>
        [Inline()]
        [Size("SubAuthorityCount")]
        public uint[] SubAuthority;
    }

}

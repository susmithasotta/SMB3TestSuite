//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------


using System;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Dtyp
{
    /// <summary>
    /// An utility class of MS-DTYP.
    /// </summary>
    public static class DtypUtility
    {
        // Length of SID authority byte array.
        private const int SID_AUTHORITY_LENGTH = 6;


        /// <summary>
        /// Specifies the NULL SID authority. It defines only the 
        /// NULL well-known-SID: S-1-0-0.
        /// </summary>
        public static byte[] NULL_SID_AUTHORITY
        {
            get
            {
                return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            }
        }


        /// <summary>
        /// Specifies the World SID authority. It only defines the 
        /// Everyone well-known-SID: S-1-1-0.
        /// </summary>
        public static byte[] WORLD_SID_AUTHORITY
        {
            get
            {
                return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
            }
        }


        /// <summary>
        /// Specifies the Local SID authority. It defines only the 
        /// Local well-known-SID: S-1-2-0.
        /// </summary>
        public static byte[] LOCAL_SID_AUTHORITY
        {
            get
            {
                return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 };
            }
        }


        /// <summary>
        /// Specifies the Creator SID authority. It defines the 
        /// Creator Owner, Creator Group, and Creator Owner Server 
        /// well-known-SIDs: S-1-3-0, S-1-3-1, and S-1-3-2. 
        /// These SIDs are used as placeholders in an access control 
        /// list (ACL) and are replaced by the user, group, and machine 
        /// SIDs of the security principal.
        /// </summary>
        public static byte[] CREATOR_SID_AUTHORITY
        {
            get
            {
                return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x03 };
            }
        }


        /// <summary>
        /// Not used.
        /// </summary>
        public static byte[] NON_UNIQUE_AUTHORITY
        {
            get
            {
                return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x04 };
            }
        }


        /// <summary>
        /// Specifies the Windows NT security subsystem SID authority. 
        /// It defines all other SIDs in the forest.
        /// </summary>
        public static byte[] NT_AUTHORITY
        {
            get
            {
                return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x05 };
            }
        }


        /// <summary>
        /// Specifies the Mandatory label authority. It defines the integrity level SIDs.
        /// </summary>
        public static byte[] SECURITY_MANDATORY_LABEL_AUTHORITY
        {
            get
            {
                return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x10 };
            }
        }


        /// <summary>
        /// Create an instance of RPC_SID.
        /// </summary>
        /// <param name="identifierAuthority">
        /// Six element arrays of 8-bit unsigned integers that specify 
        /// the top-level authority of a RPC_SID.
        /// </param>
        /// <param name="subAuthorities">
        /// A variable length array of unsigned 32-bit integers that 
        /// uniquely identifies a principal relative to the IdentifierAuthority.
        /// </param>
        /// <returns>Created RPC_SID structure.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when identifierAuthority or subAuthorities is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when length of identifierAuthority is not 6 bytes.
        /// </exception>
        public static _RPC_SID CreateRpcSid(
            byte[] identifierAuthority,
            params uint[] subAuthorities)
        {
            if (identifierAuthority == null)
            {
                throw new ArgumentNullException("identifierAuthority");
            }
            if (identifierAuthority.Length != SID_AUTHORITY_LENGTH)
            {
                throw new ArgumentException("Incorrect size of identifierAuthority.", "identifierAuthority");
            }
            if (subAuthorities == null)
            {
                throw new ArgumentNullException("subAuthorities");
            }

            const byte DEFAULT_REVISION = 1;

            _RPC_SID rpcSid = new _RPC_SID();
            rpcSid.Revision = DEFAULT_REVISION;
            rpcSid.SubAuthorityCount = (byte)subAuthorities.Length;
            rpcSid.IdentifierAuthority.Value = identifierAuthority;
            rpcSid.SubAuthority = subAuthorities;
            return rpcSid;
        }


        /// <summary>
        /// Create an instance of PRC_UNICODE_STRING.
        /// </summary>
        /// <param name="s">
        /// A string. 
        /// If it's null, Length and maximumLength is 0, Buffer is NULL.</param>
        /// <returns>Created RPC_UNICODE_STRING structure.</returns>
        public static _RPC_UNICODE_STRING ToRpcUnicodeString(string s)
        {
            _RPC_UNICODE_STRING rpcUnicodeString = new _RPC_UNICODE_STRING();

            if (s == null)
            {
                rpcUnicodeString.Length = 0;
                rpcUnicodeString.MaximumLength = 0;
                rpcUnicodeString.Buffer = null;
            }
            else
            {
                byte[] buf = Encoding.Unicode.GetBytes(s);
                rpcUnicodeString.Length = (ushort)buf.Length;
                rpcUnicodeString.MaximumLength = (ushort)buf.Length;
                rpcUnicodeString.Buffer = new ushort[buf.Length / sizeof(ushort)];
                Buffer.BlockCopy(buf, 0, rpcUnicodeString.Buffer, 0, buf.Length);
            }

            return rpcUnicodeString;
        }


        /// <summary>
        /// Read string from PRC_UNICODE_STRING, null terminating char is not included.
        /// </summary>
        /// <param name="s">
        /// A PRC_UNICODE_STRING structure. 
        /// Return value is null if Length and MaximumLength is 0 and Buffer is NULL.</param>
        /// <returns>The string in the structure.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when s is invalid.
        /// </exception>
        public static string ToString(_RPC_UNICODE_STRING s)
        {
            if (s.Length == 0 && s.MaximumLength == 0 && s.Buffer == null)
            {
                return null;
            }

            if (s.Buffer.Length * sizeof(ushort) < s.Length)
            {
                throw new ArgumentException("RPC_UNICODE_STRING is invalid.", "s");
            }

            byte[] buf = new byte[s.Length];
            Buffer.BlockCopy(s.Buffer, 0, buf, 0, buf.Length);
            return Encoding.Unicode.GetString(buf);
        }


        /// <summary>
        /// Create an instance of OLD_LARGE_INTEGER.
        /// </summary>
        /// <param name="value">A int64 value.</param>
        /// <returns>Created OLD_LARGE_INTEGER structure.</returns>
        public static _OLD_LARGE_INTEGER ToOldLargeInteger(long value)
        {
            _OLD_LARGE_INTEGER integer = new _OLD_LARGE_INTEGER();
            byte[] buf = BitConverter.GetBytes(value);
            integer.LowPart = BitConverter.ToUInt32(buf, 0);
            integer.HighPart = BitConverter.ToInt32(buf, 4);
            return integer;
        }


        /// <summary>
        /// Read int64 value from _OLD_LARGE_INTEGER.
        /// </summary>
        /// <param name="value">A _OLD_LARGE_INTEGER structure.</param>
        /// <returns>The value in the structure.</returns>
        public static long ToInt64(_OLD_LARGE_INTEGER value)
        {
            return ((long)value.HighPart << 32) + (long)value.LowPart;
        }


    }
}

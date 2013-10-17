using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Protocols.TestTools.StackSdk.Security.Cryptographic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    internal class Smb3CryptoInfo
    {
        internal string Dialect;
        internal byte[] SessionKey;
        internal byte[] SigningKey;
        internal byte[] ServerInKey;
        internal byte[] ServerOutKey;

        internal bool EnableSessionSigning;
        internal bool EnableSessionEncryption;
        internal List<uint> EnableTreeEncryption = new List<uint>();

        internal Smb3CryptoInfo(string dialect, byte[] cryptographicKey, bool enableSigning, bool enableEncryption)
        {
            Dialect = dialect;

            SessionKey = cryptographicKey;

            // TD indicates that when signing the message the protocol uses
            // the first 16 bytes of the cryptographic key for this authenticated context. 
            // If the cryptographic key is less than 16 bytes, it is right-padded with zero bytes.
            if (SessionKey.Length < 16)
                SessionKey = SessionKey.Concat(new byte[16 - SessionKey.Length]).ToArray();
            else if (SessionKey.Length > 16)
                SessionKey = SessionKey.Take(16).ToArray();

            if (dialect != "2.24")
                SigningKey = SessionKey;
            else
                SigningKey = SP8001008KeyDerivation.CounterModeHmacSha256KeyDerive(
                                SessionKey,
                                Encoding.ASCII.GetBytes("SMB2AESCMAC\0"),
                                Encoding.ASCII.GetBytes("SmbSign\0"),
                                128);

            ServerInKey = SP8001008KeyDerivation.CounterModeHmacSha256KeyDerive(
                            SessionKey,
                            Encoding.ASCII.GetBytes("SMB2AESCCM\0"),
                            Encoding.ASCII.GetBytes("ServerIn \0"),
                            128);

            ServerOutKey = SP8001008KeyDerivation.CounterModeHmacSha256KeyDerive(
                            SessionKey,
                            Encoding.ASCII.GetBytes("SMB2AESCCM\0"),
                            Encoding.ASCII.GetBytes("ServerOut\0"),
                            128);

            EnableSessionSigning = enableSigning;
            EnableSessionEncryption = enableEncryption;
        }
    }
}

//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Description: This file is the exposed interfaces of nlmp.
//------------------------------------------------------------------------------

using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Protocols.TestTools.StackSdk.Security.Sspi;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Nlmp
{
    /// <summary>
    /// The security context of nlmp server sspi. This class is used to provide sspi APIs, specified in GSS-API, such
    /// as functions Accept/Sign/Verify/Encrypt/Decrypt.
    /// </summary>
    public class NlmpServerSecurityContext : ServerSecurityContext
    {
        #region Consts

        /// <summary>
        /// the length of ntlm v1 NtChallengeResponse of authenticate packet. see TD 2.2.2.6 NTLM v1 Response:
        /// NTLM_RESPONSE.
        /// </summary>
        private const int NTLM_V1_NT_CHALLENGE_RESPONSE_LENGTH = 24;

        /// <summary>
        /// the time stamp length. see TD 2.2.2.6 NTLM v1 Response:
        /// NTLM_RESPONSE, and TD 2.2.2.7 NTLM v2: NTLMv2_CLIENT_CHALLENGE. see TimeStamp in TD 2.2.2.7 NTLM v2:
        /// NTLMv2_CLIENT_CHALLENGE.
        /// </summary>
        private const int TIME_STAMP_LENGTH = 8;

        /// <summary>
        /// the offset of ntlm v2 Timestamp in the NtChallengeResponse of authenticate packet. see TimeStamp in TD
        /// 2.2.2.7 NTLM v2: NTLMv2_CLIENT_CHALLENGE.
        /// </summary>
        private const int NTLM_V2_TIME_STAMP_OFFSET_IN_NT_CHALLENGE_RESPONSE = 24;

        /// <summary>
        /// the client challenge length. see ChallengeFromClient in TD 2.2.2.7 NTLM v2: NTLMv2_CLIENT_CHALLENGE.
        /// </summary>
        private const int TIME_CLIENT_CHALLENGE_LENGTH = 8;

        /// <summary>
        /// the offset of ntlm v2 ClientChallenge in the NtChallengeResponse of authenticate packet. see
        /// ChallengeFromClient in TD 2.2.2.7 NTLM v2: NTLMv2_CLIENT_CHALLENGE.
        /// </summary>
        private const int NTLM_V2_CLIENT_CHALLENGE_OFFSET_IN_NT_CHALLENGE_RESPONSE = 32;

        /// <summary>
        /// the offset of ntlm v2 server name in the NtChallengeResponse of authenticate packet. see AvPairs in TD
        /// 2.2.2.7 NTLM v2: NTLMv2_CLIENT_CHALLENGE. 
        /// </summary>
        private const int NTLM_V2_SERVER_NAME_OFFSET_IN_NT_CHALLENGE_RESPONSE = 44;

        /// <summary>
        /// the reserved length of ntlm v2 server name in the NtChallengeResponse of authenticate packet. see Reserved3
        /// in TD 2.2.2.7 NTLM v2: NTLMv2_CLIENT_CHALLENGE. 
        /// </summary>
        private const int NTLM_V2_SERVER_NAME_RESERVED_LENGTH_IN_NT_CHALLENGE_RESPONSE = 4;

        #endregion

        #region Fields

        /// <summary>
        /// the nlmp server.
        /// </summary>
        private NlmpServer nlmpServer;

        /// <summary>
        /// the challenge packet server responsed.
        /// </summary>
        private NlmpChallengePacket challenge;

        /// <summary>
        /// the version client selected. NTLMv1 or NTLMv2.
        /// </summary>
        private NlmpVersion version;

        /// <summary>
        /// Whether to continue process.
        /// </summary>
        private bool needContinueProcessing;

        /// <summary>
        /// The token returned by Sspi.
        /// </summary>
        private byte[] token;

        /// <summary>
        /// the sequence number of server.<para/>
        /// it's used for server to sign/encrypt message<para/>
        /// In the case of connection-oriented authentication, the SeqNum parameter MUST start at 0 and is incremented
        /// by one for each message sent.
        /// </summary>
        private uint serverSequenceNumber;

        /// <summary>
        /// the sequence number of client.<para/>
        /// it's used for server to verify/decrypt message from client<para/>
        /// The receiver expects the first received message to have SeqNum equal to 0, and to be one greater for each
        /// subsequent message received.
        /// </summary>
        private uint clientSequenceNumber;

        #endregion

        #region Properties

        /// <summary>
        /// the nlmp server context.
        /// </summary>
        public NlmpServerContext Context
        {
            get
            {
                return this.nlmpServer.Context;
            }
        }


        /// <summary>
        /// Whether to continue process.
        /// </summary>
        public override bool NeedContinueProcessing
        {
            get
            {
                return this.needContinueProcessing;
            }
        }


        /// <summary>
        /// The session key.
        /// </summary>
        public override byte[] SessionKey
        {
            get
            {
                return this.nlmpServer.Context.ExportedSessionKey;
            }
        }


        /// <summary>
        /// The token returned by Sspi.
        /// </summary>
        public override byte[] Token
        {
            get
            {
                return this.token;
            }
        }


        /// <summary>
        /// Gets or sets sequence number for Verify, Encrypt and Decrypt message.
        /// For Digest SSP, it must be 0.
        /// </summary>
        public override uint SequenceNumber
        {
            get
            {
                return this.serverSequenceNumber;
            }
            set
            {
                this.serverSequenceNumber = value;
            }
        }


        /// <summary>
        /// Package type
        /// </summary>
        public override SecurityPackageType PackageType
        {
            get
            {
                return SecurityPackageType.Ntlm;
            }
        }


        /// <summary>
        /// Queries the sizes of the structures used in the per-message functions.
        /// </summary>
        public override SecurityPackageContextSizes ContextSizes
        {
            get
            {
                SecurityPackageContextSizes size = new SecurityPackageContextSizes();
                size.MaxSignatureSize = NlmpUtility.NLMP_SECURITY_PACKAGE_CONTEXT_SIZES;

                return size;
            }
        }


        #endregion

        #region Constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="flags">the negotiate flags indicates the capabilities of server or client</param>
        /// <param name="clientCredential">
        /// the credential of client. server sdk can not retrieve password from AD/Account Database;<para/>
        /// instead, server sdk get the user credential from this parameter.
        /// </param>
        /// <param name="isDomainJoined">whether the server joined to domain</param>
        /// <param name="netbiosDomainName">the netbios domain name of server</param>
        /// <param name="netbiosMachineName">the netbios machine name of server</param>
        public NlmpServerSecurityContext(
            NegotiateTypes flags,
            NlmpClientCredential clientCredential,
            bool isDomainJoined,
            string netbiosDomainName,
            string netbiosMachineName)
        {
            this.version = new NlmpVersion();
            this.nlmpServer = new NlmpServer();

            this.nlmpServer.Context.NegFlg = flags;
            this.nlmpServer.Context.ClientCredential = clientCredential;
            this.nlmpServer.Context.IsDomainJoined = isDomainJoined;
            this.nlmpServer.Context.NbDomainName = netbiosDomainName;
            this.nlmpServer.Context.NbMachineName = netbiosMachineName;

            this.needContinueProcessing = true;
        }


        #endregion

        #region Gss Apis

        /// <summary>
        /// The function enables the server component of a transport application to establish a security context
        /// between the server and a remote client.
        /// </summary>
        /// <param name="inToken">The token to be used in context.</param>
        /// <exception cref="ArgumentException">
        /// inalid packet type of inToken, must be NEGOTIATE or AUTHENTICATE
        /// </exception>
        public override void Accept(byte[] inToken)
        {
            NlmpEmptyPacket packet = new NlmpEmptyPacket(inToken);
            if (packet.Header.MessageType == MessageType_Values.NEGOTIATE)
            {
                AcceptNegotiatePacket(new NlmpNegotiatePacket(inToken));
            }
            else if (packet.Header.MessageType == MessageType_Values.AUTHENTICATE)
            {
                AcceptAuthenticatePacket(new NlmpAuthenticatePacket(inToken));

                this.needContinueProcessing = false;
            }
            else
            {
                throw new ArgumentException(
                    "inalid packet type of inToken, must be NEGOTIATE or AUTHENTICATE", "inToken");
            }
        }


        /// <summary>
        /// Encrypts Message. User decides what SecBuffers are used.
        /// </summary>
        /// <param name="securityBuffers">
        /// the security buffer array to encrypt.<para/>
        /// it can contain none or some data security buffer, that are combine to one message to encrypt.<para/>
        /// it can contain none or some token security buffer, in which the signature will be stored.
        /// </param>
        /// <exception cref="ArgumentNullException">the securityBuffers must not be null</exception>
        public override void Encrypt(params SecurityBuffer[] securityBuffers)
        {
            NlmpUtility.GssApiEncrypt(
                this.version,
                this.nlmpServer.Context.NegFlg,
                this.nlmpServer.Context.ServerHandle,
                this.nlmpServer.Context.ServerSigningKey,
                ref this.serverSequenceNumber,
                securityBuffers);
        }


        /// <summary>
        /// This takes the given SecBuffers, which are used by SSPI method DecryptMessage.
        /// </summary>
        /// <param name="securityBuffers">
        /// the security buffer array to decrypt.<para/>
        /// it can contain none or some data security buffer, that are combine to one message to decrypt.<para/>
        /// it can contain none or some token security buffer, in which the signature is stored.
        /// </param>
        /// <returns>the encrypt result, if verify, it's the verify result.</returns>
        /// <exception cref="ArgumentNullException">the securityBuffers must not be null</exception>
        public override bool Decrypt(params SecurityBuffer[] securityBuffers)
        {
            return NlmpUtility.GssApiDecrypt(
                this.version,
                this.nlmpServer.Context.NegFlg,
                this.nlmpServer.Context.ClientHandle,
                this.nlmpServer.Context.ClientSigningKey,
                ref this.clientSequenceNumber,
                securityBuffers);
        }


        /// <summary>
        /// Sign data according SecBuffers.
        /// </summary>
        /// <param name="securityBuffers">
        /// the security buffer array to sign.<para/>
        /// it can contain none or some data security buffer, that are combine to one message to sign.<para/>
        /// it must contain token security buffer, in which the signature will be stored.
        /// </param>
        /// <exception cref="ArgumentNullException">the securityBuffers must not be null</exception>
        /// <exception cref="ArgumentException">securityBuffers must contain signature to store signature</exception>
        public override void Sign(params SecurityBuffer[] securityBuffers)
        {
            NlmpUtility.GssApiSign(
                this.version,
                this.nlmpServer.Context.NegFlg,
                this.nlmpServer.Context.ServerHandle,
                this.nlmpServer.Context.ServerSigningKey,
                ref this.serverSequenceNumber,
                securityBuffers);
        }


        /// <summary>
        /// Encrypts Message. User decides what SecBuffers are used.
        /// </summary>
        /// <param name="securityBuffers">
        /// the security buffer array to verify.<para/>
        /// it can contain none or some data security buffer, that are combine to one message to verify.<para/>
        /// it must contain token security buffer, in which the signature is stored.
        /// </param>
        /// <exception cref="ArgumentNullException">the securityBuffers must not be null</exception>
        /// <exception cref="ArgumentException">securityBuffers must contain signature to verify</exception>
        public override bool Verify(params SecurityBuffer[] securityBuffers)
        {
            return NlmpUtility.GssApiVerify(
                this.version,
                this.nlmpServer.Context.NegFlg,
                this.nlmpServer.Context.ClientHandle,
                this.nlmpServer.Context.ClientSigningKey,
                ref this.clientSequenceNumber,
                securityBuffers);
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// accept the negotiate packet, generated the challenge packet.
        /// </summary>
        /// <param name="negotiate">the negotiate packet</param>
        private void AcceptNegotiatePacket(NlmpNegotiatePacket negotiate)
        {
            // generated negotiate flags for challenge packet
            NegotiateTypes negotiateFlags = GeneratedNegotiateFlags(negotiate);

            // initialize target name
            string targetName = GenerateTargetName();

            // initialize av pairs.
            ICollection<AV_PAIR> targetInfo = GenerateTargetInfo();

            VERSION sspiVersion = NlmpUtility.GetVersion();
            // the serverChallenge is 8 bytes.
            ulong serverChallenge = BitConverter.ToUInt64(NlmpUtility.Nonce(8), 0);

            NlmpChallengePacket challengePacket = this.nlmpServer.CreateChallengePacket(
                negotiateFlags, sspiVersion, serverChallenge, targetName, targetInfo);

            this.challenge = challengePacket;
            this.token = challengePacket.ToBytes();
        }


        /// <summary>
        /// generate the target info of challenge packet
        /// </summary>
        /// <returns>the target info in the challenge packet</returns>
        private ICollection<AV_PAIR> GenerateTargetInfo()
        {
            ICollection<AV_PAIR> targetInfo = new Collection<AV_PAIR>();
            // nb machine name
            if (this.nlmpServer.Context.NbMachineName != null)
            {
                byte[] nbMachineName = NlmpUtility.Unicode(this.nlmpServer.Context.NbMachineName);
                NlmpUtility.AddAVPair(
                    targetInfo, AV_PAIR_IDs.MsvAvNbComputerName, (ushort)nbMachineName.Length, nbMachineName);
            }
            // nb domain name
            if (this.nlmpServer.Context.NbDomainName != null)
            {
                byte[] nbDomainName = NlmpUtility.Unicode(this.nlmpServer.Context.NbDomainName);
                NlmpUtility.AddAVPair(
                    targetInfo, AV_PAIR_IDs.MsvAvNbDomainName, (ushort)nbDomainName.Length, nbDomainName);
            }
            // dns machine name
            if (this.nlmpServer.Context.DnsMachineName != null)
            {
                byte[] dnsMachineName = NlmpUtility.Unicode(this.nlmpServer.Context.DnsMachineName);
                NlmpUtility.AddAVPair(
                    targetInfo, AV_PAIR_IDs.MsvAvDnsComputerName, (ushort)dnsMachineName.Length, dnsMachineName);
            }
            // dns domain name
            if (this.nlmpServer.Context.DnsDomainName != null)
            {
                byte[] dnsDomainName = NlmpUtility.Unicode(this.nlmpServer.Context.DnsDomainName);
                NlmpUtility.AddAVPair(
                    targetInfo, AV_PAIR_IDs.MsvAvDnsDomainName, (ushort)dnsDomainName.Length, dnsDomainName);
            }
            // dns forest name
            if (this.nlmpServer.Context.DnsForestName != null)
            {
                byte[] dnsForestName = NlmpUtility.Unicode(this.nlmpServer.Context.DnsForestName);
                NlmpUtility.AddAVPair(
                    targetInfo, AV_PAIR_IDs.MsvAvDnsTreeName, (ushort)dnsForestName.Length, dnsForestName);
            }
            // eol
            NlmpUtility.AddAVPair(targetInfo, AV_PAIR_IDs.MsvAvEOL, 0, null);
            return targetInfo;
        }


        /// <summary>
        /// generate the target name of challenge packet.
        /// </summary>
        /// <returns>the target name in the challenge packet</returns>
        private string GenerateTargetName()
        {
            string targetName = string.Empty;
            // domain or netbios
            if (this.nlmpServer.Context.IsDomainJoined)
            {
                targetName = this.nlmpServer.Context.NbDomainName;
            }
            else
            {
                targetName = this.nlmpServer.Context.NbMachineName;
            }
            return targetName;
        }


        /// <summary>
        /// generated negotiate flags for challenge packet
        /// </summary>
        /// <param name="negotiate">the negotiate packet from client</param>
        /// <returns>the negotiate flags to generate challenge packet</returns>
        private NegotiateTypes GeneratedNegotiateFlags(NlmpNegotiatePacket negotiate)
        {
            NegotiateTypes negotiateFlags = this.nlmpServer.Context.NegFlg;

            // in the connectionless mode, the negotiate is null, return the flags in context.
            if (negotiate == null)
            {
                return negotiateFlags;
            }

            // unicode or oem
            if (NegotiateTypes.NTLMSSP_NEGOTIATE_UNICODE ==
                (negotiate.Payload.NegotiateFlags & NegotiateTypes.NTLMSSP_NEGOTIATE_UNICODE))
            {
                negotiateFlags |= NegotiateTypes.NTLMSSP_NEGOTIATE_UNICODE;
            }
            else if (NegotiateTypes.NTLM_NEGOTIATE_OEM ==
                (negotiate.Payload.NegotiateFlags & NegotiateTypes.NTLM_NEGOTIATE_OEM))
            {
                negotiateFlags |= NegotiateTypes.NTLM_NEGOTIATE_OEM;
            }
            // extended security or lm
            if (NegotiateTypes.NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY ==
                (negotiate.Payload.NegotiateFlags &
                NegotiateTypes.NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY))
            {
                negotiateFlags |= NegotiateTypes.NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY;
            }
            else if (NegotiateTypes.NTLMSSP_NEGOTIATE_LM_KEY ==
                (negotiate.Payload.NegotiateFlags & NegotiateTypes.NTLMSSP_NEGOTIATE_LM_KEY))
            {
                negotiateFlags |= NegotiateTypes.NTLMSSP_NEGOTIATE_LM_KEY;
            }
            // target info
            if (NegotiateTypes.NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY ==
                (negotiateFlags & NegotiateTypes.NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY))
            {
                negotiateFlags |= NegotiateTypes.NTLMSSP_NEGOTIATE_TARGET_INFO;
            }

            return negotiateFlags;
        }


        /// <summary>
        /// accept the authenticate packet, if failed, throw exception.
        /// </summary>
        /// <param name="authenticate">authenciate packet</param>
        /// <exception cref="InvalidOperationException">INVALID message error</exception>
        private void AcceptAuthenticatePacket(NlmpAuthenticatePacket authenticate)
        {
            // retrieve the client authenticate infomation
            ClientAuthenticateInfomation authenticateInformation = RetrieveClientAuthenticateInformation(authenticate);

            // update the version of context
            this.version = authenticateInformation.Version;

            // valid user name
            if (authenticateInformation.UserName != this.nlmpServer.Context.ClientCredential.AccountName)
            {
                throw new InvalidOperationException(
                    "the user name is invalid!"
                    + " the user name retreived form authenticate packet is not equal to the context.");
            }

            // calc the basekeys
            byte[] responseKeyLm;
            byte[] expectedNtChallengeResponse;
            byte[] expectedLmChallengeResponse;
            byte[] sessionBaseKey;
            byte[] keyExchangeKey;
            CalculateBaseKeys(
                authenticateInformation.ClientChallenge,
                authenticateInformation.ClientTime,
                authenticateInformation.ServerName,
                authenticateInformation.DomainName,
                authenticateInformation.UserName,
                this.nlmpServer.Context.ClientCredential.Password,
                out responseKeyLm, out expectedNtChallengeResponse, out expectedLmChallengeResponse,
                out sessionBaseKey, out keyExchangeKey);

            // valid message
            ValidAuthenticateMessage(authenticate, expectedNtChallengeResponse, expectedLmChallengeResponse);

            // valid mic
            byte[] messageMic = authenticate.Payload.MIC;
            if (messageMic != null)
            {
                AUTHENTICATE_MESSAGE payload = authenticate.Payload;
                payload.MIC = null;
                authenticate.Payload = payload;
            }

            // generate keys.
            byte[] encryptedRandomSessionKey = null;
            byte[] exportedSessionKey = null;
            NlmpUtility.GetEncryptedRandomSessionKey(this.version,
                this.nlmpServer.Context.NegFlg,
                sessionBaseKey,
                expectedLmChallengeResponse,
                responseKeyLm,
                challenge.Payload.ServerChallenge,
                out encryptedRandomSessionKey,
                out keyExchangeKey,
                out exportedSessionKey);

            // save keys
            this.nlmpServer.Context.ExportedSessionKey = exportedSessionKey;

            // initialize keys and handles
            InitializeKeys(exportedSessionKey);
        }


        /// <summary>
        /// valid the authenticate message
        /// </summary>
        /// <param name="authenticate">
        /// the authenticate packet of client, contains the challenge response calculated by client
        /// </param>
        /// <param name="expectedNtChallengeResponse">the nt challenge response calculated by server</param>
        /// <param name="expectedLmChallengeResponse">the lm challenge response calculated by server</param>
        private static void ValidAuthenticateMessage(
            NlmpAuthenticatePacket authenticate,
            byte[] expectedNtChallengeResponse, byte[] expectedLmChallengeResponse)
        {
            if (authenticate.Payload.NtChallengeResponseFields.Len != 0
                && !ArrayUtility.CompareArrays<byte>(expectedNtChallengeResponse,
                authenticate.Payload.NtChallengeResponse))
            {
                if (authenticate.Payload.LmChallengeResponseFields.Len != 0
                    || !ArrayUtility.CompareArrays<byte>(expectedLmChallengeResponse,
                    authenticate.Payload.LmChallengeResponse))
                {
                    throw new InvalidOperationException("INVALID message error");
                }
            }
        }


        /// <summary>
        /// calculate the base keys to initalize the other keys
        /// </summary>
        /// <param name="clientChallenge">the challenge generated by client</param>
        /// <param name="time">the time generated by client</param>
        /// <param name="serverName">the server name from the authenticate packet</param>
        /// <param name="domainName">the domain name from the authenticate packet</param>
        /// <param name="userName">the user name from the authenticate packet</param>
        /// <param name="userPassword">the user password from the authenticate packet</param>
        /// <param name="responseKeyLm">output the lm key</param>
        /// <param name="expectedNtChallengeResponse">output the expected nt challenge response of server</param>
        /// <param name="expectedLmChallengeResponse">output the expected lm challenge response of server</param>
        /// <param name="sessionBaseKey">output the session base key of server</param>
        /// <param name="keyExchangeKey">output the key exchange key of server</param>
        private void CalculateBaseKeys(
            ulong clientChallenge,
            ulong time,
            byte[] serverName,
            string domainName,
            string userName,
            string userPassword,
            out byte[] responseKeyLm,
            out byte[] expectedNtChallengeResponse,
            out byte[] expectedLmChallengeResponse,
            out byte[] sessionBaseKey,
            out byte[] keyExchangeKey)
        {
            // calculate the response key nt and lm
            byte[] responseKeyNt = NlmpUtility.GetResponseKeyNt(this.version, domainName, userName, userPassword);
            responseKeyLm = NlmpUtility.GetResponseKeyLm(this.version, domainName, userName, userPassword);

            // calcute the expected key
            expectedNtChallengeResponse = null;
            expectedLmChallengeResponse = null;
            sessionBaseKey = null;
            NlmpUtility.ComputeResponse(this.version, this.nlmpServer.Context.NegFlg, responseKeyNt, responseKeyLm,
                this.challenge.Payload.ServerChallenge, clientChallenge, time, serverName,
                out expectedNtChallengeResponse, out expectedLmChallengeResponse, out sessionBaseKey);

            // update session key
            this.nlmpServer.Context.SessionBaseKey = new byte[sessionBaseKey.Length];
            Array.Copy(sessionBaseKey, this.nlmpServer.Context.SessionBaseKey, sessionBaseKey.Length);

            // key exchange key
            keyExchangeKey = NlmpUtility.KXKey(
                this.version, sessionBaseKey, expectedLmChallengeResponse, this.challenge.Payload.ServerChallenge,
                this.nlmpServer.Context.NegFlg, responseKeyLm);
        }


        /// <summary>
        /// retrieve the domain name from client. client encode the domain name in the authenticate packet.
        /// </summary>
        /// <param name="authenticate">the authenticate packet contains the domain name</param>
        /// <returns>the authentication information of client</returns>
        private ClientAuthenticateInfomation RetrieveClientAuthenticateInformation(NlmpAuthenticatePacket authenticate)
        {
            ClientAuthenticateInfomation authenticateInformation = new ClientAuthenticateInfomation();

            // retrieve the version of client
            if (authenticate.Payload.NtChallengeResponseFields.Len == NTLM_V1_NT_CHALLENGE_RESPONSE_LENGTH)
            {
                authenticateInformation.Version = NlmpVersion.v1;
            }
            else
            {
                authenticateInformation.Version = NlmpVersion.v2;
            }

            // retrieve the client challenge
            if (authenticateInformation.Version == NlmpVersion.v1)
            {
                authenticateInformation.ClientChallenge = BitConverter.ToUInt64(ArrayUtility.SubArray<byte>(
                    authenticate.Payload.LmChallengeResponse, 0, TIME_CLIENT_CHALLENGE_LENGTH), 0);
            }
            else
            {
                authenticateInformation.ClientChallenge = BitConverter.ToUInt64(
                    ArrayUtility.SubArray<byte>(authenticate.Payload.NtChallengeResponse,
                    NTLM_V2_CLIENT_CHALLENGE_OFFSET_IN_NT_CHALLENGE_RESPONSE, TIME_CLIENT_CHALLENGE_LENGTH), 0);
            }

            // retrieve the domain name of client
            if (NlmpUtility.IsUnicode(authenticate.Payload.NegotiateFlags))
            {
                authenticateInformation.DomainName = Encoding.Unicode.GetString(authenticate.Payload.DomainName);
            }
            else
            {
                authenticateInformation.DomainName = Encoding.ASCII.GetString(authenticate.Payload.DomainName);
            }

            // retrieve the user name of client
            if (NlmpUtility.IsUnicode(authenticate.Payload.NegotiateFlags))
            {
                authenticateInformation.UserName = Encoding.Unicode.GetString(authenticate.Payload.UserName);
            }
            else
            {
                authenticateInformation.UserName = Encoding.ASCII.GetString(authenticate.Payload.UserName);
            }

            // retrieve the server name of client
            if (authenticateInformation.Version == NlmpVersion.v2)
            {
                authenticateInformation.ServerName =
                    ArrayUtility.SubArray<byte>(authenticate.Payload.NtChallengeResponse,
                     NTLM_V2_SERVER_NAME_OFFSET_IN_NT_CHALLENGE_RESPONSE,
                     authenticate.Payload.NtChallengeResponseFields.Len -
                     NTLM_V2_SERVER_NAME_OFFSET_IN_NT_CHALLENGE_RESPONSE -
                     NTLM_V2_SERVER_NAME_RESERVED_LENGTH_IN_NT_CHALLENGE_RESPONSE);
            }

            // retrieve the time of client
            ICollection<AV_PAIR> targetInfo =
                NlmpUtility.BytesGetAvPairCollection(this.challenge.Payload.TargetInfo);

            // retrieve the time
            authenticateInformation.ClientTime = NlmpUtility.GetTime(targetInfo);

            // if server did not response the timestamp, use the client time stamp
            if (!NlmpUtility.AvPairContains(targetInfo, AV_PAIR_IDs.MsvAvTimestamp)
                && authenticateInformation.Version == NlmpVersion.v2)
            {
                authenticateInformation.ClientTime = BitConverter.ToUInt64(
                    ArrayUtility.SubArray<byte>(authenticate.Payload.NtChallengeResponse,
                    NTLM_V2_TIME_STAMP_OFFSET_IN_NT_CHALLENGE_RESPONSE, TIME_STAMP_LENGTH), 0);
            }

            return authenticateInformation;
        }


        /// <summary>
        /// after successfully authenticate, initialize the keys and handles
        /// </summary>
        /// <param name="exportedSessionKey">the exported key to initialize the keys and handles</param>
        private void InitializeKeys(byte[] exportedSessionKey)
        {
            // initialize keys
            this.nlmpServer.Context.ClientSigningKey =
                NlmpUtility.SignKey(this.nlmpServer.Context.NegFlg, exportedSessionKey, "Client");
            this.nlmpServer.Context.ServerSigningKey =
                NlmpUtility.SignKey(this.nlmpServer.Context.NegFlg, exportedSessionKey, "Server");
            this.nlmpServer.Context.ClientSealingKey =
                NlmpUtility.SealKey(this.nlmpServer.Context.NegFlg, exportedSessionKey, "Client");
            this.nlmpServer.Context.ServerSealingKey =
                NlmpUtility.SealKey(this.nlmpServer.Context.NegFlg, exportedSessionKey, "Server");

            // initialize handles
            NlmpUtility.RC4Init(this.nlmpServer.Context.ClientHandle, this.nlmpServer.Context.ClientSealingKey);
            NlmpUtility.RC4Init(this.nlmpServer.Context.ServerHandle, this.nlmpServer.Context.ServerSealingKey);
        }


        #endregion
    }
}
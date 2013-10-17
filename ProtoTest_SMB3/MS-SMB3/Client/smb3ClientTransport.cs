// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// Used as transport layer of MS-DFSC protocol
    /// </summary>
    public class smb3ClientTransport : FileServiceClientTransport
    {
        #region Fields

        //Underlying transport
        private smb3Client smb3Client;

        //If user does not specify timeout value, this will be used
        private TimeSpan internalTimeout;

        //User identifier
        private ulong sessionId;

        //Share identifier
        private uint treeId;

        //File identifier
        private FILEID fileId;

        //Indicate if this object has been disposed
        private bool disposed;

        //The share name of ipc
        private const string IPC_CONNECT_STRING = "IPC$";

        //Max output buffer length of dfsc response
        private const uint DEFAULT_MAX_OUTPUT_RESPONSE = 4096;

        //The internal timeout seconds
        private const int INTERNAL_TIMEOUT_SECS = 20;

        //The max input buffer length in ioctl response
        private const uint MAX_INPUT_RESPONSE_IN_IOCTL = 1024 * 40;

        //The max input buffer length in ioctl response
        private const uint MAX_OUTPUT_RESPONSE_IN_IOCTL = 1024 * 40;
        #endregion


        #region Properties

        /// <summary>
        /// To detect whether there are packets cached in the queue of Transport.
        /// Usually, it should be called after Disconnect to assure all events occursed in transport
        /// have been handled.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The transport is not connected.</exception>
        public override bool IsDataAvailable
        {
            get
            {
                return this.smb3Client.IsDataAvailable;
            }
        }

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public smb3ClientTransport()
            : base()
        {
            smb3Client = new smb3Client();
            internalTimeout = new TimeSpan(0, 0, INTERNAL_TIMEOUT_SECS);
        }

        #endregion


        #region Dispose

        /// <summary>
        /// Release resources.
        /// </summary>
        /// <param name="disposing">If disposing equals true, Managed and unmanaged resources are disposed.
        /// if false, Only unmanaged resources can be disposed.</param>
        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed and unmanaged resources.
                if (disposing)
                {
                    // Free managed resources & other reference types:
                }

                // Call the appropriate methods to clean up unmanaged resources.
                if (this.smb3Client != null)
                {
                    this.smb3Client.Dispose();
                    this.smb3Client = null;
                }
                base.Dispose(disposing);
                this.disposed = true;
            }
        }

        #endregion


        #region Methods

        /// <summary>
        /// Set up connection with server.
        /// Including 4 steps: 1. Tcp connection 2. Negotiation 3. SesstionSetup 4. TreeConnect in order
        /// </summary>
        /// <param name="server">server name of ip address</param>
        /// <param name="client">client name of ip address</param>
        /// <param name="domain">user's domain</param>
        /// <param name="userName">user's name</param>
        /// <param name="password">user's password</param>
        /// <param name="timeout">The pending time to get server's response in step 2, 3 or 4</param>
        /// <exception cref="System.Net.ProtocolViolationException">Fail to set up connection with server</exception>
        public override void Connect(
            string server,
            string client,
            string domain,
            string userName,
            string password,
            TimeSpan timeout)
        {

            smb3Client.Connect(client, server);
            InternalConnectShare(domain, userName, password, IPC_CONNECT_STRING, timeout);
        }


        /// <summary>
        /// Send a Dfs request to server
        /// </summary>
        /// <param name="payload">REQ_GET_DFS_REFERRAL structure in byte array</param>
        /// <exception cref="System.ArgumentNullException">the payload to be sent is null.</exception>
        /// <exception cref="System.InvalidOperationException">The transport is not connected</exception>
        public override void SendDfscPayload(byte[] payload)
        {
            if (this.smb3Client == null)
            {
                throw new InvalidOperationException("The transport is not connected.");
            }

            if (payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            smb3Packet request = this.smb3Client.CreateDfsReferralIOCtlRequest(
                this.sessionId,
                this.treeId,
                DEFAULT_MAX_OUTPUT_RESPONSE,
                payload);
            this.smb3Client.SendPacket(request);
        }


        /// <summary>
        /// Wait for the Dfs response packet from server.
        /// </summary>
        /// <param name="status">The status of response</param>
        /// <param name="payload">RESP_GET_DFS_REFERRAL structure in byte array</param>
        /// <param name="timeout">The pending time to get server's response</param>
        /// <exception cref="System.InvalidOperationException">The transport is not connected</exception>
        public override void ExpectDfscPayload(TimeSpan timeout, out uint status, out byte[] payload)
        {
            if (this.smb3Client == null)
            {
                throw new InvalidOperationException("The transport is not connected.");
            }

            smb3Packet response = this.smb3Client.ExpectPacket(timeout);
            smb3IOCtlResponsePacket ioCtlResponse = response as smb3IOCtlResponsePacket;
            if (ioCtlResponse != null)
            {
                status = ioCtlResponse.Header.Status;
                payload = ioCtlResponse.PayLoad.Buffer;
            }
            else
            {
                status = (response as smb3ErrorResponsePacket).Header.Status;
                payload = null;
            }
        }


        /// <summary>
        /// Disconnect from server.
        /// Including 3 steps: 1. TreeDisconnect 2. Logoff 3. Tcp disconnection in order.
        /// </summary>
        /// <param name="timeout">The pending time to get server's response in step 1 or 2</param>
        /// <exception cref="System.Net.ProtocolViolationException">Fail to disconnect from server</exception>
        /// <exception cref="System.InvalidOperationException">The transport is not connected</exception>
        public override void Disconnect(TimeSpan timeout)
        {
            if (this.smb3Client == null)
            {
                throw new InvalidOperationException("The transport is not connected.");
            }
            smb3Packet request;
            smb3Packet response;
            uint status;

            // Tree disconnect:
            request = this.smb3Client.CreateTreeDisconnectRequest(this.sessionId, this.treeId);
            this.smb3Client.SendPacket(request);
            response = this.smb3Client.ExpectPacket(timeout);
            smb3TreeDisconnectResponsePacket treeDisconnectResponse = response as smb3TreeDisconnectResponsePacket;

            if (treeDisconnectResponse != null)
            {
                status = treeDisconnectResponse.Header.Status;
            }
            else
            {
                status = (response as smb3ErrorResponsePacket).Header.Status;
            }

            if (status != 0)
            {
                throw new ProtocolViolationException("Tree Disconnect Failed. ErrorCode: " + status);
            }

            // Log off:
            request = this.smb3Client.CreateLogOffRequest(this.sessionId);
            this.smb3Client.SendPacket(request);
            response = this.smb3Client.ExpectPacket(timeout);

            smb3LogOffResponsePacket logoffResponse = response as smb3LogOffResponsePacket;

            if (logoffResponse != null)
            {
                status = logoffResponse.Header.Status;
            }
            else
            {
                status = (response as smb3ErrorResponsePacket).Header.Status;
            }

            if (status != 0)
            {
                throw new ProtocolViolationException("Log off Failed. ErrorCode: " + status);
            }

            this.smb3Client.Disconnect();
        }


        /// <summary>
        /// Connect to a share indicated by shareName in server
        /// This will use smb over tcp as transport. Only one server
        /// can be connected at one time
        /// </summary>
        /// <param name="serverName">The server Name</param>
        /// <param name="port">The server port</param>
        /// <param name="ipVersion">The ip version</param>
        /// <param name="domain">The domain name</param>
        /// <param name="userName">The user name</param>
        /// <param name="password">The password</param>
        /// <param name="shareName">The share name</param>
        /// <exception cref="System.InvalidOperationException">Thrown if there is any error occurred</exception>
        public override void ConnectShare(string serverName, int port, IpVersion ipVersion, string domain,
            string userName, string password, string shareName)
        {

            smb3Client.Connect(serverName, true, port);
            InternalConnectShare(domain, userName, password, shareName, internalTimeout);
        }


        /// <summary>
        /// Connect to a share indicated by shareName in server.
        /// This will use smb over netbios as transport. Only one server
        /// can be connected at one time.
        /// </summary>
        /// <param name="serverNetBiosName">The server netbios name</param>
        /// <param name="clientNetBiosName">The client netbios name</param>
        /// <param name="domain">The domain name</param>
        /// <param name="userName">The user name</param>
        /// <param name="password">The password</param>
        /// <param name="shareName">The share name</param>
        /// <exception cref="System.InvalidOperationException">Thrown if there is any error occurred</exception>
        public override void ConnectShare(string serverNetBiosName, string clientNetBiosName, string domain,
            string userName, string password, string shareName)
        {
            smb3Client.Connect(clientNetBiosName, serverNetBiosName);
            InternalConnectShare(domain, userName, password, shareName, internalTimeout);
        }


        /// <summary>
        /// Create File, named pipe, directory. One transport can only create one file.
        /// </summary>
        /// <param name="fileName">The file, namedpipe, directory name</param>
        /// <param name="desiredAccess">The desired access</param>
        /// <param name="impersonationLevel">The impersonation level</param>
        /// <param name="fileAttribute">The file attribute, this field is only valid when create file.
        /// </param>
        /// <param name="createDisposition">Defines the action the server MUST take if the file that is
        /// specified in the name field already exists</param>
        /// <param name="createOption">Specifies the options to be applied when creating or opening the file</param>
        /// <exception cref="System.InvalidOperationException">Thrown if there is any error occurred</exception>
        public override void Create(string fileName, FsFileDesiredAccess desiredAccess, FsImpersonationLevel impersonationLevel,
            FsFileAttribute fileAttribute, FsCreateDisposition createDisposition, FsCreateOption createOption)
        {
            if ((createOption & FsCreateOption.FILE_DIRECTORY_FILE) == FsCreateOption.FILE_DIRECTORY_FILE)
            {
                throw new ArgumentException("createOption can not contain FILE_DIRECTORY_FILE when creating file.");
            }

            InternalCreate(fileName, (uint)desiredAccess, impersonationLevel, fileAttribute, createDisposition, createOption);
        }


        /// <summary>
        /// Create directory. One transport can only create one directory
        /// </summary>
        /// <param name="directoryName">The directory name</param>
        /// <param name="desiredAccess">The desired access</param>
        /// <param name="impersonationLevel">The impersonation level</param>
        /// <param name="fileAttribute">The file attribute, this field is only valid when create file.
        /// </param>
        /// <param name="createDisposition">Defines the action the server MUST take if the file that is
        /// specified in the name field already exists</param>
        /// <param name="createOption">Specifies the options to be applied when creating or opening the file</param>
        /// <exception cref="System.InvalidOperationException">Thrown if there is any error occurred</exception>
        public override void Create(string directoryName, FsDirectoryDesiredAccess desiredAccess, FsImpersonationLevel impersonationLevel,
            FsFileAttribute fileAttribute, FsCreateDisposition createDisposition, FsCreateOption createOption)
        {
            if ((createOption & FsCreateOption.FILE_NON_DIRECTORY_FILE) == FsCreateOption.FILE_NON_DIRECTORY_FILE)
            {
                throw new ArgumentException("createOption can not contain FILE_NON_DIRECTORY_FILE when creating file.");
            }

            InternalCreate(directoryName, (uint)desiredAccess, impersonationLevel, fileAttribute, createDisposition, createOption);
        }


        /// <summary>
        /// Write data to server. cifs/smb implementation of this interface should pay attention to offset.
        /// They may not accept ulong as offset
        /// </summary>
        /// <param name="timeout">Waiting time of this operation</param>
        /// <param name="offset">The offset of the file from where client wants to start writing</param>
        /// <param name="data">The data which will be written to server</param>
        /// <exception cref="System.InvalidOperationException">Thrown if there is any error occurred</exception>
        public override void Write(TimeSpan timeout, ulong offset, byte[] data)
        {
            uint maxWriteSize = 0;

            try
            {
                smb3Client.LockGlobalContext();
                maxWriteSize = smb3Client.Connection.MaxWriteSize;
            }
            finally
            {
                smb3Client.UnlockGlobalContext();
            }

            List<byte[]> dataList = new List<byte[]>();
            long index = 0;
            int count = 0;
            if (data.Length > maxWriteSize)
            {
                byte[] buffer = new byte[maxWriteSize];
                count = (int)(data.Length / maxWriteSize);

                for (int i = 0; i < count; i++)
                {
                    Array.Copy(data, index, buffer, 0, buffer.Length);
                    index += maxWriteSize;
                    dataList.Add(buffer);
                }

                if (index < data.Length)
                {
                    byte[] lastBlock = new byte[data.Length - index];
                    Array.Copy(data, index, lastBlock, 0, lastBlock.Length);
                    dataList.Add(lastBlock);
                }
            }
            else
            {
                dataList.Add(data);
            }

            foreach (byte[] item in dataList)
            {
                smb3WriteRequestPacket writeRequest = smb3Client.CreateWriteRequest(offset, fileId, 0, item);
                smb3Client.SendPacket(writeRequest);

                smb3SinglePacket singlePacket = smb3Client.ExpectPacket(internalTimeout) as smb3SinglePacket;

                if (singlePacket is smb3ErrorResponsePacket)
                {
                    if (IsInterimResponsePacket(singlePacket))
                    {
                        singlePacket = smb3Client.ExpectPacket(internalTimeout) as smb3SinglePacket;

                        if (singlePacket is smb3ErrorResponsePacket)
                        {
                            throw new InvalidOperationException(
                                string.Format("Fails with error code: 0x{0:x}.", singlePacket.Header.Status));
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            string.Format("Fails with error code: 0x{0:x}.", singlePacket.Header.Status));
                    }
                }
            }
        }


        /// <summary>
        /// Read data from server, start at the positon indicated by offset
        /// </summary>
        /// <param name="timeout">Waiting time of this operation</param>
        /// <param name="offset">From where it will read</param>
        /// <param name="length">The length of the data client wants to read</param>
        /// <param name="data">The read data</param>
        /// <exception cref="System.InvalidOperationException">Thrown if there is any error occurred</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public override void Read(TimeSpan timeout, ulong offset, uint length, out byte[] data)
        {
            uint maxReadSize = 0;
            try
            {
                smb3Client.LockGlobalContext();
                maxReadSize = smb3Client.Connection.MaxReadSize;
            }
            finally
            {
                smb3Client.UnlockGlobalContext();
            }

            using (MemoryStream ms = new MemoryStream())
            {
                smb3ReadResponsePacket readResponse = null;
                smb3ReadRequestPacket readRequest = null;
                smb3SinglePacket singlePacket = null;
                uint index = 0;
                uint oneTimeReadLen = 0;

                while ((length - index) > 0)
                {
                    if ((length - index) > maxReadSize)
                    {
                        readRequest = smb3Client.CreateReadRequest(
                            maxReadSize,
                            offset + index,
                            fileId,
                            0);

                        oneTimeReadLen = maxReadSize;
                        index += maxReadSize;
                    }
                    else
                    {
                        readRequest = smb3Client.CreateReadRequest(
                            length - index,
                            offset + index,
                            fileId,
                            0);

                        oneTimeReadLen = length - index;
                        index = length;
                    }

                    smb3Client.SendPacket(readRequest);
                    singlePacket = smb3Client.ExpectPacket(internalTimeout)
                        as smb3SinglePacket;

                    if (singlePacket is smb3ErrorResponsePacket)
                    {
                        if (IsInterimResponsePacket(singlePacket))
                        {
                            singlePacket = smb3Client.ExpectPacket(internalTimeout)
                                as smb3SinglePacket;

                            if (singlePacket is smb3ErrorResponsePacket)
                            {
                                throw new InvalidOperationException(
                                    string.Format("Fails with error code: 0x{0:x}", singlePacket.Header.Status));
                            }
                            else
                            {
                                readResponse = singlePacket as smb3ReadResponsePacket;
                                if (readResponse.PayLoad.Buffer != null)
                                {
                                    ms.Write(readResponse.PayLoad.Buffer, 0, readResponse.PayLoad.Buffer.Length);

                                    //means there is no data can be readed. so we do not issue another read request
                                    if (readResponse.PayLoad.Buffer.Length < oneTimeReadLen)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                string.Format("Fails with error code: 0x{0:x}", singlePacket.Header.Status));
                        }
                    }
                    else
                    {
                        readResponse = singlePacket as smb3ReadResponsePacket;
                        if (readResponse.PayLoad.Buffer != null)
                        {
                            ms.Write(readResponse.PayLoad.Buffer, 0, readResponse.PayLoad.Buffer.Length);

                            //means there is no data can be readed. so we do not issue another read request
                            if (readResponse.PayLoad.Buffer.Length < oneTimeReadLen)
                            {
                                break;
                            }
                        }
                    }
                }

                data = ms.ToArray();
            }
        }


        /// <summary>
        /// Do IO control on server, this function does not accept file system control code as control code.
        /// for that use, use FileSystemControl() function instead
        /// </summary>
        /// <param name="timeout">Waiting time of this operation</param>
        /// <param name="ioControlCode">The IO control code</param>
        /// <param name="input">The input data of this control operation</param>
        /// <param name="output">The output data of this control operation</param>
        /// <exception cref="System.InvalidOperationException">Thrown if there is any error occurred</exception>
        public override void IoControl(TimeSpan timeout, uint ioControlCode, byte[] input, out byte[] output)
        {
            InternalIoControl(timeout, ioControlCode, false, input, out output);
        }


        /// <summary>
        /// Do File system control on server
        /// </summary>
        /// <param name="timeout">Waiting time of this operation</param>
        /// <param name="fsControlCode">The file system control code</param>
        /// <param name="input">The input data of this control operation</param>
        /// <param name="output">The output data of this control operation</param>
        /// <exception cref="System.InvalidOperationException">Thrown if there is any error occurred</exception>
        public override void IoControl(TimeSpan timeout, FsCtlCode fsControlCode, byte[] input, out byte[] output)
        {
            InternalIoControl(timeout, (uint)fsControlCode, true, input, out output);
        }


        /// <summary>
        /// Close file, named pipe, directory
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if there is any error occurred</exception>
        public override void Close()
        {
            smb3CloseRequestPacket closeRequest = smb3Client.CreateCloseRequest(Flags_Values.NONE, fileId);
            smb3Client.SendPacket(closeRequest);

            smb3SinglePacket singlePacket = smb3Client.ExpectPacket(internalTimeout) as smb3SinglePacket;
            if (singlePacket.Header.Status != 0)
            {
                throw new InvalidOperationException(
                    string.Format("Fails with error code: 0x{0:x}", singlePacket.Header.Status));
            }
        }


        /// <summary>
        /// Disconnect share
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if there is any error occurred</exception>
        public override void DisconnetShare()
        {
            //tree disconnect
            smb3TreeDisconnectRequestPacket treeDisconnectPacket = smb3Client.CreateTreeDisconnectRequest(sessionId, treeId);
            smb3Client.SendPacket(treeDisconnectPacket);

            smb3SinglePacket responsePacket = smb3Client.ExpectPacket(internalTimeout)
                as smb3SinglePacket;
            if (responsePacket.Header.Status != 0)
            {
                throw new InvalidOperationException(
                    string.Format("Fails with error code: 0x{0:x}.", responsePacket.Header.Status));
            }

            //log off
            smb3LogOffRequestPacket logOffRequestPacket = smb3Client.CreateLogOffRequest(sessionId);
            smb3Client.SendPacket(logOffRequestPacket);

            responsePacket = smb3Client.ExpectPacket(internalTimeout) as smb3SinglePacket;
            if (responsePacket.Header.Status != 0)
            {
                throw new InvalidOperationException(
                    string.Format("Fails with error code: 0x{0:x}.", responsePacket.Header.Status));
            }

            smb3Client.Disconnect();
        }


        /// <summary>
        /// Connect to the share, not including tcp or netbios connect process
        /// </summary>
        /// <param name="domain">The domain</param>
        /// <param name="userName">The user name</param>
        /// <param name="password">The password</param>
        /// <param name="shareName">The share name</param>
        /// <param name="timeout">The waiting time for response</param>
        /// <exception cref="System.Net.ProtocolViolationException">Thrown when meets a connection error</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void InternalConnectShare(string domain, string userName, string password, string shareName, TimeSpan timeout)
        {
            smb3Packet request;
            smb3Packet response;
            uint status;

            // Negotiate:
            ushort smb3002dialect = 0x202;
            request = this.smb3Client.CreateNegotiateRequest(NEGOTIATE_Request_Capabilities_Values.SMB2_GLOBAL_ALL,smb3002dialect);
            this.smb3Client.SendPacket(request);
            response = this.smb3Client.ExpectPacket(timeout);
            smb3NegotiateResponsePacket negotiateResponse = response as smb3NegotiateResponsePacket;

            if (negotiateResponse != null)
            {
                status = negotiateResponse.Header.Status;
            }
            else
            {
                status = (response as smb3ErrorResponsePacket).Header.Status;
            }

            if (status != 0)
            {
                throw new ProtocolViolationException("Negotiate Failed. ErrorCode: " + status);
            }

            // Session setup:
            ulong previousSessionId = 0;
            bool useServerTokenInNegotiateResponse = false;

            request = this.smb3Client.CreateFirstSessionSetupRequest(previousSessionId,
                 0, //Bindflag for multichannel
                SESSION_SETUP_Request_Capabilities_Values.GLOBAL_CAP_DFS, SecurityPackage.Nlmp,
                ClientContextAttribute.None, domain, userName, password, useServerTokenInNegotiateResponse);
            this.smb3Client.SendPacket(request);

            response = this.smb3Client.ExpectPacket(timeout);
            smb3SessionSetupResponsePacket sessionSetupResponse = response as smb3SessionSetupResponsePacket;

            if (sessionSetupResponse != null)
            {
                status = sessionSetupResponse.Header.Status;
            }
            else
            {
                status = (response as smb3ErrorResponsePacket).Header.Status;
            }

            while (status != 0)
            {
                if (status == (uint)smb3Status.STATUS_MORE_PROCESSING_REQUIRED)
                {
                    this.sessionId = sessionSetupResponse.Header.SessionId;

                    request = this.smb3Client.CreateSecondSessionSetupRequest(this.sessionId,0);
                    this.smb3Client.SendPacket(request);

                    response = this.smb3Client.ExpectPacket(timeout);
                    sessionSetupResponse = response as smb3SessionSetupResponsePacket;
                    if (sessionSetupResponse != null)
                    {
                        status = sessionSetupResponse.Header.Status;
                    }
                    else
                    {
                        status = (response as smb3ErrorResponsePacket).Header.Status;
                    }
                }
                else
                {
                    throw new ProtocolViolationException(
                        string.Format("Session Setup Failed. ErrorCode: 0x{0:x}", status));
                }
            }
            this.sessionId = sessionSetupResponse.Header.SessionId;

            // Tree connect:
            request = this.smb3Client.CreateTreeConnectRequest(this.sessionId, shareName);
            this.smb3Client.SendPacket(request);

            response = this.smb3Client.ExpectPacket(timeout);

            if (response is smb3ErrorResponsePacket)
            {
                throw new ProtocolViolationException(
                    string.Format(
                    "Tree Connect Failed. ErrorCode: 0x{0:x}",
                    (response as smb3ErrorResponsePacket).Header.Status)
                    );
            }

            smb3TreeConnectResponsePacket treeConnectResponse = response as smb3TreeConnectResponsePacket;
            this.treeId = treeConnectResponse.Header.TreeId;
        }


        /// <summary>
        /// Do IO control on remote server
        /// </summary>
        /// <param name="timeout">The waiting time for response</param>
        /// <param name="ctlCode">The control code</param>
        /// <param name="isFsCtl">Indicate if the control code is a file system control code</param>
        /// <param name="input">The input data of io control</param>
        /// <param name="output">The output data of io control</param>
        /// <exception cref="System.InvalidOperationException">Throw when meet an transport error</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void InternalIoControl(TimeSpan timeout, uint ctlCode, bool isFsCtl, byte[] input, out byte[] output)
        {
            IOCTL_Request_Flags_Values ioCtlRequestFlag = IOCTL_Request_Flags_Values.NONE;

            if (isFsCtl)
            {
                ioCtlRequestFlag = IOCTL_Request_Flags_Values.SMB2_0_IOCTL_IS_FSCTL;
            }

            smb3IOCtlRequestPacket ioCtlRequest = smb3Client.CreateIOCtlRequest(
                (CtlCode_Values)ctlCode,
                fileId,
                MAX_INPUT_RESPONSE_IN_IOCTL,
                MAX_OUTPUT_RESPONSE_IN_IOCTL,
                ioCtlRequestFlag,
                input);
            smb3Client.SendPacket(ioCtlRequest);

            smb3SinglePacket responsePacket = smb3Client.ExpectPacket(timeout) as smb3SinglePacket;
            if (responsePacket is smb3ErrorResponsePacket)
            {
                if (IsInterimResponsePacket(responsePacket))
                {
                    responsePacket = smb3Client.ExpectPacket(timeout) as smb3SinglePacket;

                    if (responsePacket is smb3ErrorResponsePacket)
                    {
                        output = null;
                        throw new InvalidOperationException(
                            string.Format("Fails with error code: 0x{0:x}", responsePacket.Header.Status)
                            );
                    }
                    else
                    {
                        output = (responsePacket as smb3IOCtlResponsePacket).PayLoad.Buffer;
                    }
                }
                else
                {
                    output = null;
                    throw new InvalidOperationException(
                        string.Format("Fails with error code: 0x{0:x}", responsePacket.Header.Status)
                        );
                }
            }
            else
            {
                output = (responsePacket as smb3IOCtlResponsePacket).PayLoad.Buffer;
            }
        }


        /// <summary>
        /// Create File, named pipe, directory. One transport can only create one file.
        /// </summary>
        /// <param name="fileName">The file, namedpipe, directory name</param>
        /// <param name="desiredAccess">The desired access</param>
        /// <param name="impersonationLevel">The impersonation level</param>
        /// <param name="fileAttribute">The file attribute, this field is only valid when create file.
        /// </param>
        /// <param name="createDisposition">Defines the action the server MUST take if the file that is
        /// specified in the name field already exists</param>
        /// <param name="createOption">Specifies the options to be applied when creating or opening the file</param>
        /// <exception cref="System.InvalidOperationException">Thrown if there is any error occurred</exception>
        private void InternalCreate(string fileName, uint desiredAccess, FsImpersonationLevel impersonationLevel,
            FsFileAttribute fileAttribute, FsCreateDisposition createDisposition, FsCreateOption createOption)
        {
            smb3CreateRequestPacket createRequest = smb3Client.CreateCreateRequest(false,
                sessionId,
                treeId,
                RequestedOplockLevel_Values.OPLOCK_LEVEL_NONE,
                (ImpersonationLevel_Values)impersonationLevel,
                desiredAccess,
                (File_Attributes)fileAttribute,
                ShareAccess_Values.NONE,
                (CreateDisposition_Values)createDisposition,
                (CreateOptions_Values)createOption,
                fileName,
                null);

            smb3Client.SendPacket(createRequest);

            smb3SinglePacket singlePacket = smb3Client.ExpectPacket(internalTimeout) as smb3SinglePacket;
            smb3CreateResponsePacket createResponse = singlePacket as smb3CreateResponsePacket;

            if (!(singlePacket is smb3ErrorResponsePacket))
            {
                fileId = createResponse.PayLoad.FileId;
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format("Fails with error code 0x{0:x}.", singlePacket.Header.Status));
            }
        }


        /// <summary>
        /// Test if the response packet is an interim response packet
        /// </summary>
        /// <param name="singlePacket">The single response packet</param>
        /// <returns>True if it is a interim packet, otherwise false.</returns>
        private bool IsInterimResponsePacket(smb3SinglePacket singlePacket)
        {
            if (((smb3Status)singlePacket.Header.Status == smb3Status.STATUS_PENDING)
                    && ((singlePacket.Header.Flags & Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND)
                    == Packet_Header_Flags_Values.FLAGS_ASYNC_COMMAND))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
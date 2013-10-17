// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: Wrapper native netbios function
// ------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// Wrapper native netbios function, the class can represent one client and multiply server
    /// and one server multiply client.
    /// </summary>
    internal class NetbiosTransport : IDisposable
    {
        //because listen call must be terminated using cancel. so keep the 
        //previous listen call ncb point. It does not need to be freeed because it just 
        //keep a copy point of the original one. the original one will be freeed in
        //Netbios(ref NCB) function, so when disposing, it does not need to be freeed.
        private IntPtr listeningNcbPtr;
        private readonly object listeningNcbLocker = new object();
        private int listeningNcbSize;
        private volatile bool disposed;
        private ushort maxBufferSize;

        private byte ncbNum;
        private string localNetbiosName;

        //Enviroment setting
        private static bool hasInitialized;
        private static byte networkAdapterId;
        //protect global setting
        private static readonly object netbiosEnvLocker = new object();


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localName">local bios name</param>
        /// <param name="adapterIndex">Indicates which network adapter to be used</param>
        /// <param name="maxBufferSize">max send and receive buffer size</param>
        /// <param name="maxSessionNum">max session number</param>
        /// <param name="maxNames">max names user can regist</param>
        public NetbiosTransport(string localName, byte adapterIndex, ushort maxBufferSize, byte maxSessionNum, byte maxNames)
        {
            localNetbiosName = localName;
            this.maxBufferSize = maxBufferSize;

            //reset need only been called once. it will reset all netbios static data in one process.
            //so if it is called twice, some changes between the two Reset call will be lost.
            lock (netbiosEnvLocker)
            {
                if (!hasInitialized)
                {
                    // use the first adapter
                    networkAdapterId = GetAdapterId(adapterIndex);
                    ResetAdapter(maxSessionNum, maxNames);
                    hasInitialized = true;
                }
            }

            RegisterName();
        }


        /// <summary>
        /// Connect to remote machine
        /// </summary>
        /// <param name="remoteName">The remote machine bios name</param>
        /// <returns>The session id</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Throw when connect fails.
        /// </exception>
        public int Connect(string remoteName)
        {
            NCB ncb = new NCB();

            try
            {
                NetbiosUtility.InitNcb(ref ncb);
                ncb.ncb_command = (byte)NcbCommand.NCBCALL;
                ncb.ncb_lana_num = networkAdapterId;
                ncb.ncb_name = NetbiosUtility.ToNetbiosName(localNetbiosName);
                ncb.ncb_callname = NetbiosUtility.ToNetbiosName(remoteName);

                InvokeNetBios(ref ncb);

                if (ncb.ncb_retcode != (byte)NcbReturnCode.NRC_GOODRET)
                {
                    throw new InvalidOperationException("Failed in NCBCALL command, error is "
                        + ((NcbReturnCode)ncb.ncb_retcode).ToString());
                }
            }
            finally
            {
                NetbiosUtility.FreeNcbNativeFields(ref ncb);
            }

            return ncb.ncb_lsn;
        }


        /// <summary>
        /// Receive data
        /// </summary>
        /// <param name="sessionId">Specified the session indentify</param>
        /// <returns>The received data</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Throw when receive encounts error.
        /// </exception>
        public byte[] Receive(int sessionId)
        {
            NCB ncb = new NCB();

            try
            {
                NetbiosUtility.InitNcb(ref ncb);
                ncb.ncb_command = (byte)NcbCommand.NCBRECV;
                ncb.ncb_lana_num = networkAdapterId;
                ncb.ncb_lsn = (byte)sessionId;
                ncb.ncb_buffer = Marshal.AllocHGlobal(maxBufferSize);
                ncb.ncb_length = maxBufferSize;

                InvokeNetBios(ref ncb);

                if (ncb.ncb_retcode == (byte)NcbReturnCode.NRC_SCLOSED)
                {
                    return null;
                }

                if (ncb.ncb_retcode != (byte)NcbReturnCode.NRC_GOODRET)
                {
                    throw new InvalidOperationException("Failed in NCBRECV command, error is "
                        + ((NcbReturnCode)ncb.ncb_retcode).ToString());
                }

                byte[] receivedData = new byte[ncb.ncb_length];
                Marshal.Copy(ncb.ncb_buffer, receivedData, 0, receivedData.Length);

                return receivedData;
            }
            finally
            {
                NetbiosUtility.FreeNcbNativeFields(ref ncb);
            }
        }


        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="sessionId">The session id</param>
        /// <param name="buffer">The data buffer</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Throw when the buffer is larger than the max buffer size
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Throw when call native netbios api fails
        /// </exception>
        public void Send(int sessionId, byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (buffer.Length > maxBufferSize)
            {
                throw new ArgumentOutOfRangeException(
                    "buffer",
                    "buffer is too large! The max buffer size configed is " + maxBufferSize);
            }

            NCB ncb = new NCB();

            try
            {
                NetbiosUtility.InitNcb(ref ncb);
                ncb.ncb_command = (byte)NcbCommand.NCBSEND;
                ncb.ncb_buffer = Marshal.AllocHGlobal(buffer.Length);
                Marshal.Copy(buffer, 0, ncb.ncb_buffer, buffer.Length);
                ncb.ncb_length = (ushort)buffer.Length;
                ncb.ncb_lana_num = networkAdapterId;
                ncb.ncb_lsn = (byte)sessionId;

                InvokeNetBios(ref ncb);

                if (ncb.ncb_retcode != (byte)NcbReturnCode.NRC_GOODRET)
                {
                    throw new InvalidOperationException("Failed in NCBSEND command, error is "
                        + ((NcbReturnCode)ncb.ncb_retcode).ToString());
                }
            }
            finally
            {
                NetbiosUtility.FreeNcbNativeFields(ref ncb);
            }
        }


        /// <summary>
        /// Disconnect the session
        /// </summary>
        /// <param name="sessionId">The session id</param>
        /// <exception cref="System.InvalidOperationException">
        /// Throw when disconnect fails
        /// </exception>
        public void Disconnect(int sessionId)
        {
            NCB ncb = new NCB();

            try
            {
                NetbiosUtility.InitNcb(ref ncb);
                ncb.ncb_command = (byte)NcbCommand.NCBHANGUP;
                ncb.ncb_lana_num = networkAdapterId;
                ncb.ncb_lsn = (byte)sessionId;

                InvokeNetBios(ref ncb);

                //if remote machine disconnect the session first, local call disconnect will return NRC_SNUMOUT,
                //and this is not a error which we need to notify user.
                if ((ncb.ncb_retcode != (byte)NcbReturnCode.NRC_GOODRET)
                    && (ncb.ncb_retcode != (byte)NcbReturnCode.NRC_SNUMOUT)
                    && (ncb.ncb_retcode != (byte)NcbReturnCode.NRC_SCLOSED))
                {
                    throw new InvalidOperationException("Failed in NCBHANGUP command, error is "
                        + ((NcbReturnCode)ncb.ncb_retcode).ToString());
                }
            }
            finally
            {
                NetbiosUtility.FreeNcbNativeFields(ref ncb);
            }
        }


        /// <summary>
        /// Listen to local netbios endpoint
        /// </summary>
        /// <returns>The connected session id</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Throw when listen fails
        /// </exception>
        public byte Listen()
        {
            NCB ncb = new NCB();

            try
            {
                NetbiosUtility.InitNcb(ref ncb);
                ncb.ncb_command = (byte)NcbCommand.NCBLISTEN;
                ncb.ncb_lana_num = networkAdapterId;
                //* means it can accept any connection.
                ncb.ncb_callname = NetbiosUtility.ToNetbiosName("*");
                ncb.ncb_name = NetbiosUtility.ToNetbiosName(localNetbiosName);

                InvokeNetBios(ref ncb);

                if (ncb.ncb_retcode != (byte)NcbReturnCode.NRC_GOODRET)
                {
                    throw new InvalidOperationException("Failed in NCBLISTEN command, error is "
                        + ((NcbReturnCode)ncb.ncb_retcode).ToString());
                }
            }
            finally
            {
                NetbiosUtility.FreeNcbNativeFields(ref ncb);
            }

            return ncb.ncb_lsn;
        }


        /// <summary>
        /// Cancel the previous pending listen call
        /// </summary>
        public void CancelListen()
        {
            lock (listeningNcbLocker)
            {
                if (listeningNcbPtr == IntPtr.Zero)
                {
                    return;
                }

                NCB ncb = new NCB();

                NetbiosUtility.InitNcb(ref ncb);
                ncb.ncb_command = (byte)NcbCommand.NCBCANCEL;
                ncb.ncb_buffer = listeningNcbPtr;
                ncb.ncb_length = (ushort)listeningNcbSize;
                ncb.ncb_lana_num = networkAdapterId;

                InvokeNetBios(ref ncb);
            }
        }


        /// <summary>
        /// Call native Netbios interface
        /// </summary>
        /// <param name="ncb">The NCB structure</param>
        private void InvokeNetBios(ref NCB ncb)
        {
            IntPtr ncbPtr = IntPtr.Zero;
            int ncbSize = 0;

            try
            {
                lock (listeningNcbLocker)
                {
                    ncbPtr = NetbiosUtility.MarshalNcb(ref ncb, out ncbSize);

                    if (ncb.ncb_command == (byte)NcbCommand.NCBLISTEN)
                    {
                        listeningNcbPtr = ncbPtr;
                        listeningNcbSize = ncbSize;
                    }
                }
              
                NetbiosNativeMethods.Netbios(ncbPtr);

                ncb = NetbiosUtility.UnMarshalNcb(ncbPtr, ncbSize);
            }
            finally
            {
                lock (listeningNcbLocker)
                {
                    Marshal.FreeHGlobal(ncbPtr);

                    if (ncb.ncb_command == (byte)NcbCommand.NCBLISTEN)
                    {
                        listeningNcbPtr = IntPtr.Zero;
                        listeningNcbSize = 0;
                    }
                }
            }
        }


        /// <summary>
        /// Get the adapter id
        /// </summary>
        /// <param name="adapterIndex">the adapter index</param>
        /// <returns>The adapter id</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Throw when Get adapterId fails
        /// </exception>
        private byte GetAdapterId(int adapterIndex)
        {
            NCB ncb = new NCB();

            try
            {
                NetbiosUtility.InitNcb(ref ncb);
                ncb.ncb_command = (byte)NcbCommand.NCBENUM;
                ncb.ncb_buffer = Marshal.AllocHGlobal(maxBufferSize);
                ncb.ncb_length = maxBufferSize;

                InvokeNetBios(ref ncb);

                if (ncb.ncb_retcode != (byte)NcbReturnCode.NRC_GOODRET)
                {
                    throw new InvalidOperationException("Failed in NCBENUM command, error is "
                        + ((NcbReturnCode)ncb.ncb_retcode).ToString());
                }

                LANA_ENUM lenum = new LANA_ENUM();
                lenum.length = Marshal.ReadByte(ncb.ncb_buffer, 0);
                lenum.lanaNum = new byte[lenum.length];

                for (int i = 0; i < lenum.length; i++)
                {
                    lenum.lanaNum[i] = Marshal.ReadByte(ncb.ncb_buffer, i + 1);
                }

                return lenum.lanaNum[adapterIndex];
            }
            finally
            {
                NetbiosUtility.FreeNcbNativeFields(ref ncb);
            }
        }


        /// <summary>
        /// Reset the adapter, it will clear all registed names, and reset the maxsession,
        /// maxName
        /// </summary>
        /// <param name="maxSession">The max Session the adapter can accept</param>
        /// <param name="maxName">The max name the adapter can accept</param>
        /// <exception cref="System.InvalidOperationException">Throw when reset adapter fails</exception>
        public void ResetAdapter(byte maxSession, byte maxName)
        {
            NCB ncb = new NCB();

            try
            {
                NetbiosUtility.InitNcb(ref ncb);
                ncb.ncb_command = (byte)NcbCommand.NCBRESET;
                ncb.ncb_lana_num = networkAdapterId;
                Marshal.WriteByte(ncb.ncb_callname, 0, maxSession);
                Marshal.WriteByte(ncb.ncb_callname, 2, maxName);

                InvokeNetBios(ref ncb);

                if (ncb.ncb_retcode != (byte)NcbReturnCode.NRC_GOODRET)
                {
                    throw new InvalidOperationException("Failed in NCBRESET command, error is "
                        + ((NcbReturnCode)ncb.ncb_retcode).ToString());
                }
            }
            finally
            {
                NetbiosUtility.FreeNcbNativeFields(ref ncb);
            }
        }


        /// <summary>
        /// Register bios name for further call
        /// </summary>
        /// <returns>The name index</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Throw when register name fails
        /// </exception>
        private byte RegisterName()
        {
            NCB ncb = new NCB();

            try
            {
                NetbiosUtility.InitNcb(ref ncb);
                ncb.ncb_command  = (byte)NcbCommand.NCBADDNAME;
                ncb.ncb_lana_num = networkAdapterId;
                ncb.ncb_name = NetbiosUtility.ToNetbiosName(localNetbiosName);

                InvokeNetBios(ref ncb);

                if (ncb.ncb_retcode != (byte)NcbReturnCode.NRC_GOODRET)
                {
                    throw new InvalidOperationException("Failed in NCBADDNAME command, error is "
                        + ((NcbReturnCode)ncb.ncb_retcode).ToString());
                }

                this.ncbNum = ncb.ncb_num;
                return ncb.ncb_num;
            }
            finally
            {
                NetbiosUtility.FreeNcbNativeFields(ref ncb);
            }
        }


        /// <summary>
        /// Unregister the bios name
        /// </summary>
        private void UnRegisterName()
        {
            NCB ncb = new NCB();

            try
            {
                NetbiosUtility.InitNcb(ref ncb);
                ncb.ncb_command = (byte)NcbCommand.NCBDELNAME;
                ncb.ncb_lana_num = networkAdapterId;
                ncb.ncb_num = ncbNum;
                ncb.ncb_name = NetbiosUtility.ToNetbiosName(localNetbiosName);

                InvokeNetBios(ref ncb);
            }
            finally
            {
                NetbiosUtility.FreeNcbNativeFields(ref ncb);
            }
        }


        /// <summary>
        /// Release all resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Release all resources
        /// </summary>
        /// <param name="disposing">Indicate GC or user calling this function</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                }

                UnRegisterName();

                disposed = true;
            }
        }

        
        /// <summary>
        /// Deconstructure
        /// </summary>
        ~NetbiosTransport()
        {
            Dispose(false);
        }
    }
}

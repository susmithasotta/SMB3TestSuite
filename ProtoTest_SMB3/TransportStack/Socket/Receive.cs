// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: It is used to receive raw data from a socket.
// ------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// It is used to receive raw data from a socket.
    /// </summary>
    internal sealed class SocketReceive : IReceive, IDisposable
    {
        #region fields

        private bool disposed;
        private object endPointIdentity;
        private Socket socket;

        #endregion


        #region Constructor and Dispose

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="endPoint">a network endpoint from which the data is received.</param>
        /// <param name="receivingHost">It is used to receive raw data from transport.</param>
        public SocketReceive(object endPoint, Socket receivingHost)
        {
            this.endPointIdentity = endPoint;
            this.socket = receivingHost;
        }


        /// <summary>
        /// Release resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            //Take this object out of the finalization queue of the GC:
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Release resources.
        /// </summary>
        /// <param name="disposing">If disposing equals true, Managed and unmanaged resources are disposed.
        /// if false, Only unmanaged resources can be disposed.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed and unmanaged resources.
                if (disposing)
                {
                    // Free managed resources & other reference types:
                    if (this.socket != null)
                    {
                        this.socket.Close();
                        this.socket = null;
                    }
                }

                // Call the appropriate methods to clean up unmanaged resources.
                // If disposing is false, only the following code is executed:

                this.disposed = true;
            }
        }


        /// <summary>
        /// This destructor will get called only from the finalization queue.
        /// </summary>
        ~SocketReceive()
        {
            this.Dispose(false);
        }

        #endregion


        #region Methods

        /// <summary>
        /// To receive the raw data from transport. 
        /// </summary>
        /// <param name="buffer">buffer used to cache the received data.</param>
        /// <param name="numBytesReceived">if the returned status is Success, it is the length in bytes of
        /// the received data. Otherwise, it is meaningless.</param>
        /// <param name="endPoint">a network endpoint from which the data is received.</param>
        /// <returns>the status of transport.</returns>
        public ReceiveStatus Receive(byte[] buffer, out int numBytesReceived, out object endPoint)
        {
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            switch (this.socket.ProtocolType)
            {
                case ProtocolType.Udp:
                    numBytesReceived = this.socket.ReceiveFrom(buffer, ref remoteEndPoint);
                    endPoint = remoteEndPoint;
                    break;
                case ProtocolType.Tcp:
                    numBytesReceived = this.socket.Receive(buffer);
                    endPoint = this.endPointIdentity;
                    break;
                default:
                    throw new NotSupportedException("This ProtocolType of Socket is not supported.");
            }


            if (numBytesReceived == 0)
            {
                return ReceiveStatus.Disconnected;
            }
            else
            {
                return ReceiveStatus.Success;
            }
        }


        /// <summary>
        /// interupt the blocked receiver. the receive thread will accept a exception and exit normally.
        /// </summary>
        public void Interupt()
        {
            this.socket.Close();
        }


        #endregion
    }
}

// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: This is the TCP client transport.
// ------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Protocols.TestTools.StackSdk;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    internal sealed class TcpClientTransport : IConnection, IDisposable
    {
        #region fields

        private bool disposed;
        private SocketTransportConfig config;
        private DecodePacketCallback decoder;
        private QueueManager packetQueue;
        private ReceiveThread receiveThread;
        private Socket socket;
        private IPEndPoint localEndPoint;
        private IPEndPoint remoteEndPoint;

        #endregion


        #region properties

        /// <summary>
        /// ConnectionFilter filter.
        /// This filter is used when receiving connection. 
        /// Packets from the connection that are in filter will be dropped.
        /// </summary>
        public ConnectionFilter ConnectionFilter
        {
            get
            {
                throw new NotSupportedException("ConnectionFilter is not supported by cleint role.");
            }
            set
            {
                throw new NotSupportedException("ConnectionFilter is not supported by cleint role.");
            }
        }

        #endregion


        #region Constructor and Dispose

        /// <summary>
        /// the construct of TcpClientTransport. Initialize variables.
        /// </summary>
        /// <param name="transportConfig">Provides the transport parameters.</param>
        /// <param name="packetQueueManager">Store all event packets generated in receiving loop.</param>
        /// <param name="decodePacketCallback">Callback of decoding packet.</param>
        public TcpClientTransport(
            TransportConfig transportConfig,
            QueueManager packetQueueManager,
            DecodePacketCallback decodePacketCallback)
        {
            this.config = transportConfig as SocketTransportConfig;
            if (this.config == null)
            {
                throw new System.InvalidCastException("TcpClientTransport needs SocketTransportConfig.");
            }

            this.packetQueue = packetQueueManager;
            this.decoder = decodePacketCallback;

            SocketType sockType = SocketType.Stream;

            if (this.config.LocalIpPort != 0)
            {
                this.socket = new Socket(this.config.LocalIpAddress.AddressFamily, sockType, ProtocolType.Tcp);

                this.localEndPoint = new IPEndPoint(this.config.LocalIpAddress, this.config.LocalIpPort);
                this.socket.Bind(this.localEndPoint);
            }
            else
            {
                //if user does not supply local ip address and port, socket will create a arbitrary one.
                this.socket = new Socket(this.config.RemoteIpAddress.AddressFamily, sockType, ProtocolType.Tcp);
            }

            this.remoteEndPoint = new IPEndPoint(this.config.RemoteIpAddress, this.config.RemoteIpPort);
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

                    if (this.receiveThread != null)
                    {
                        this.receiveThread.Dispose();
                        this.receiveThread = null;
                    }

                    if (this.packetQueue != null)
                    {
                        this.packetQueue.Dispose();
                        this.packetQueue = null;
                    }
                }

                this.disposed = true;
            }
        }


        /// <summary>
        /// This destructor will get called only from the finalization queue.
        /// </summary>
        ~TcpClientTransport()
        {
            this.Dispose(false);
        }

        #endregion


        #region methods

        /// <summary>
        /// to update the config of transport at runtime.
        /// </summary>
        /// <param name="transportConfig">the config for transport</param>
        public void UpdateConfig(TransportConfig transportConfig)
        {
            throw new NotImplementedException("Tcp client role does not implement this operation.");
        }


        /// <summary>
        /// to start the transport.
        /// Genarally, it is only used for server role.
        /// </summary>
        public void Start()
        {
            throw new NotSupportedException("Tcp client role does not support this operation.");
        }


        /// <summary>
        /// connect to remote endpoint.
        /// </summary>
        /// <returns>the identity of the connected endpoint.</returns>
        public object Connect()
        {
            this.socket.Connect(this.remoteEndPoint);

            this.localEndPoint = this.socket.LocalEndPoint as IPEndPoint;

            this.receiveThread = new ReceiveThread(this.packetQueue, this.decoder,
                new SocketReceive(this.localEndPoint, this.socket), this.config.BufferSize);
            this.receiveThread.Start();

            return this.localEndPoint;
        }


        /// <summary>
        /// disconnect from remote host.
        /// </summary>
        public void Disconnect()
        {
            if (this.receiveThread != null)
            {
                this.receiveThread.Abort();
            }

            if (this.socket.Connected)
            {
                this.socket.Disconnect(false);
            }
        }


        /// <summary>
        /// disconnect from the remote host according to the given endPoint.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endPoint">endpoint to be desconnected.</param>
        public void Disconnect(object endPoint)
        {
            throw new NotSupportedException("Tcp client role does not support this operation.");
        }


        /// <summary>
        /// Send a packet against the transport.
        /// </summary>
        /// <param name="packet">the packet to be sent.</param>
        public void SendPacket(StackPacket packet)
        {
            // If we use raw bytes to construct the packet, we send the raw bytes out directly, 
            // otherwise we convert the packet to bytes and send it out.
            byte[] writeBytes = (packet.PacketBytes == null) ? packet.ToBytes() : packet.PacketBytes;

            if (!this.socket.Connected)
            {
                throw new InvalidOperationException("The transport is not connected.");
            }

            int sentLength = this.socket.Send(writeBytes, 0, writeBytes.Length, 0);

            if (sentLength != writeBytes.Length)
            {
                throw new InvalidOperationException("Fail to send data against the socket.");
            }
        }


        /// <summary>
        /// Send arbitrary message against the transport.
        /// If server role, the packet is only be sent to the first connected remote host.
        /// </summary>
        /// <param name="message">the message to be sent.</param>
        public void SendBytes(byte[] message)
        {
            int sentLength = this.socket.Send(message);

            if (sentLength != message.Length)
            {
                throw new InvalidOperationException("Fail to send data against the socket.");
            }
        }


        /// <summary>
        /// Send a packet to a special remote host.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endPoint">the remote host to which the packet will be sent.</param>
        /// <param name="packet">the packet to be sent.</param>
        public void SendPacket(object endPoint, StackPacket packet)
        {
            throw new NotSupportedException("Tcp client role does not support this operation.");
        }


        /// <summary>
        /// Send arbitrary message to a special remote host.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endpoint">the remote host to which the packet will be sent.</param>
        /// <param name="message">the message to be sent.</param>
        public void SendBytes(object endpoint, byte[] message)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// To close the transport and release resources.
        /// </summary>
        public void Release()
        {
            this.Dispose();
        }

        #endregion
    }
}

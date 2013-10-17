//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Description: This file describes Udp transport.
//
//------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// Udp transport implementation.
    /// </summary>
    internal sealed class UdpTransport : IConnection, IDisposable
    {
        private bool disposed;

        /// <summary>
        /// The config for transport.
        /// </summary>
        private SocketTransportConfig udpConfig;

        /// <summary>
        /// The decode packet call back delegate.
        /// </summary>
        private DecodePacketCallback decoder;

        /// <summary>
        /// The receive message thread.
        /// </summary>
        private ReceiveThread receiveThread;

        /// <summary>
        /// The queue to store packets.
        /// </summary>
        private QueueManager packetQueue;

        /// <summary>
        /// Udp client for sending packet.
        /// </summary>
        private Socket udpSocket;

        /// <summary>
        /// The construct of UdpTransport. Initialize variables.
        /// </summary>
        /// <param name="transportConfig">Provides the transport parameters.</param>
        /// <param name="packetQueueManager">Store all event packets generated in receiving loop.</param>
        /// <param name="decodePacketCallback">Callback of decoding packet.</param>
        public UdpTransport(
            TransportConfig transportConfig,
            QueueManager packetQueueManager,
            DecodePacketCallback decodePacketCallback)
        {
            this.udpConfig = transportConfig as SocketTransportConfig;
            if (this.udpConfig == null)
            {
                throw new ArgumentException("UdpClientTransport needs SocketTransportConfig.", "transportConfig");
            }
            this.packetQueue = packetQueueManager;
            this.decoder = decodePacketCallback;

            this.udpSocket = new Socket(
                udpConfig.LocalIpAddress.AddressFamily,
                SocketType.Dgram,
                ProtocolType.Udp);
            this.udpSocket.Bind(new IPEndPoint(this.udpConfig.LocalIpAddress, this.udpConfig.LocalIpPort));
            this.receiveThread = new ReceiveThread(
                this.packetQueue, 
                this.decoder,
                new SocketReceive(null, this.udpSocket), 
                this.udpConfig.BufferSize);
        }


        /// <summary>
        /// Desctructor
        /// </summary>
        ~UdpTransport()
        {
            Dispose(false);
        }


        /// <summary>
        /// Release managed and un-managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
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
                    //Free managed resources & other reference types.
                    if (receiveThread != null)
                    {
                        this.receiveThread.Dispose();
                        receiveThread = null;
                    }

                    if (udpSocket != null)
                    {
                        this.udpSocket.Close();
                        udpSocket = null;
                    }

                    if (packetQueue != null)
                    {
                        this.packetQueue.Dispose();
                        packetQueue = null;
                    }
                }

                // Call the appropriate methods to clean up unmanaged resources.
                // If disposing is false, only the following code is executed:
                this.disposed = true;
            }
        }


        #region IConnection
        public ConnectionFilter ConnectionFilter
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotSupportedException("ConnectionFilter is not supported by UdpTransport.");
            }
        }


        /// <summary>
        /// to update the config of transport at runtime.
        /// </summary>
        /// <param name="config">the config for transport</param>
        public void UpdateConfig(TransportConfig config)
        {
            throw new NotImplementedException("Udp does not implement this operation.");
        }


        /// <summary>
        /// Start receiving thread.
        /// </summary>
        public void Start()
        {
            this.receiveThread.Start();
        }


        /// <summary>
        /// connect to remote endpoint.
        /// Genarally, it is only used for client role.
        /// </summary>
        /// <returns>the identity of the connected endpoint.</returns>
        public object Connect()
        {
            throw new NotSupportedException("This operation is not supported by UdpTransport.");
        }


        /// <summary>
        /// disconnect from remote host.
        /// </summary>
        public void Disconnect()
        {
            throw new NotSupportedException("This operation is not supported by UdpTransport.");
        }


        /// <summary>
        /// disconnect from the remote host according to the given endPoint.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endPoint">endpoint to be disconnected.</param>
        public void Disconnect(object endPoint)
        {
            throw new NotSupportedException("This operation is not supported by UdpTransport.");
        }


        /// <summary>
        /// Send a packet against the transport.
        /// </summary>
        /// <param name="packet">The packet to be sent.</param>
        /// <exception cref="ArgumentNullException">An error occurred when paramter is null reference.</exception>
        /// <exception cref="SocketException">An error occurred when accessing the socket.</exception>
        /// <exception cref="InvalidOperationException">An error occurred when length of </exception>
        public void SendPacket(StackPacket packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }
            IPEndPoint remoteEndpoint = new IPEndPoint(this.udpConfig.RemoteIpAddress, this.udpConfig.RemoteIpPort);

            this.udpSocket.SendTo(packet.ToBytes(), remoteEndpoint);
        }


        /// <summary>
        /// Send arbitrary message against the transport.
        /// If server role, the packet is only be sent to the first connected remote host.
        /// </summary>
        /// <param name="message">the message to be sent.</param>
        /// <exception cref="ArgumentNullException">An error occurred when paramter is null reference.</exception>
        /// <exception cref="SocketException">An error occurred when accessing the socket.</exception>
        /// <exception cref="InvalidOperationException">An error occurred when length of </exception>
        public void SendBytes(byte[] message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            IPEndPoint remoteEndpoint = new IPEndPoint(this.udpConfig.RemoteIpAddress, this.udpConfig.RemoteIpPort);

            this.udpSocket.SendTo(message, remoteEndpoint);
        }


        /// <summary>
        /// Send a packet to a special remote host.
        /// </summary>
        /// <param name="endPoint">The remote endpoint to which the packet will be sent.</param>
        /// <param name="packet">The packet to be sent.</param>
        /// <exception cref="ArgumentNullException">An error occurred when paramter is null reference.</exception>
        /// <exception cref="SocketException">An error occurred when accessing the socket.</exception>
        /// <exception cref="InvalidOperationException">An error occurred when length of </exception>
        public void SendPacket(object endPoint, StackPacket packet)
        {
            EndPoint remoteEndPoint = endPoint as EndPoint;

            if (remoteEndPoint == null)
            {
                throw new ArgumentNullException("endPoint", "The endPoint is not an EndPoint.");
            }
            if (packet == null)
            {
                throw new ArgumentNullException("packet", "The packet should not be null.");
            }

            this.udpSocket.SendTo(packet.ToBytes(), remoteEndPoint);
        }


        /// <summary>
        /// Send arbitrary message to a special remote host.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endpoint">the remote host to which the packet will be sent.</param>
        /// <param name="message">the message to be sent.</param>
        public void SendBytes(object endpoint, byte[] message)
        {
            EndPoint remoteEndPoint = endpoint as EndPoint;

            this.udpSocket.SendTo(message, remoteEndPoint);
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

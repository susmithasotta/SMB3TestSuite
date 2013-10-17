// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: This is the TCP server transport.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Protocols.TestTools.StackSdk;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// Implement methods of interface IConnection to perform the (dis)connect, Send
    /// and receive operations in the server-side.
    /// </summary>
    internal sealed class TcpServerTransport : IConnection, IDisposable
    {
        #region fields

        private bool disposed;
        private SocketTransportConfig config;
        private DecodePacketCallback decoder;
        private QueueManager packetQueue;
        private Dictionary<Socket, ReceiveThread> receivingThreads;
        private ConnectionFilter connectionFilter;
        private Socket listenSock;
        private Thread acceptThread;
        private IPEndPoint localEndPoint;
        private bool isTcpServerTransportStarted;
        private volatile bool exitLoop;

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
                return this.connectionFilter;
            }
            set
            {
                this.connectionFilter = value;
            }
        }

        #endregion


        #region Constructor and Dispose

        /// <summary>
        ///  Constructor. Initialize member variables.
        /// </summary>
        /// <param name="transportConfig">Provides the transport parameters.</param>
        /// <param name="packetQueueManager">Store all event packets generated in receiving loop.</param>
        /// <param name="decodePacketCallback">Callback of decoding packet.</param>
        public TcpServerTransport(
            TransportConfig transportConfig,
            QueueManager packetQueueManager,
            DecodePacketCallback decodePacketCallback)
        {
            this.config = transportConfig as SocketTransportConfig;
            if (this.config == null)
            {
                throw new System.InvalidCastException("TcpServerTransport needs SocketTransportConfig.");
            }

            this.decoder = decodePacketCallback;
            this.packetQueue = packetQueueManager;

            SocketType sockType = SocketType.Stream;
            this.localEndPoint = new IPEndPoint(this.config.LocalIpAddress, this.config.LocalIpPort);
            this.listenSock = new Socket(this.config.LocalIpAddress.AddressFamily, sockType, ProtocolType.Tcp);
            this.listenSock.Bind(this.localEndPoint);
            this.listenSock.Listen(this.config.MaxConnections);
            this.acceptThread = new Thread(new ThreadStart(AcceptLoop));
            this.receivingThreads = new Dictionary<Socket,ReceiveThread>();
        }


        /// <summary>
        /// Release the managed and unmanaged resources.
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
                    // Call the appropriate methods to clean up unmanaged resources.
                    // If disposing is false, only the following code is executed:
                    if (this.listenSock != null)
                    {
                        this.listenSock.Close();
                        this.listenSock = null;
                    }

                    lock (this.receivingThreads)
                    {
                        if (this.receivingThreads != null)
                        {
                            foreach (KeyValuePair<Socket, ReceiveThread> kvp in this.receivingThreads)
                            {
                                kvp.Value.Dispose();
                                kvp.Key.Close();
                            }
                            this.receivingThreads = null;
                        }
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
        ~TcpServerTransport()
        {
            this.Dispose(false);
        }

        #endregion


        #region public methods

        /// <summary>
        /// to update the config of transport at runtime.
        /// </summary>
        /// <param name="transportConfig">the config for transport</param>
        public void UpdateConfig(TransportConfig transportConfig)
        {
            throw new NotImplementedException("Tcp server role does not implement this operation.");
        }


        /// <summary>
        /// to start the server.
        /// </summary>
        public void Start()
        {
            this.acceptThread.Start();
            this.isTcpServerTransportStarted = true;
        }


        /// <summary>
        /// connect to remote endpoint.
        /// Genarally, it is only used for client role.
        /// </summary>
        /// <returns>the identity of the connected endpoint.</returns>
        public object Connect()
        {
            throw new NotSupportedException("Tcp server role does not support this operation.");
        }


        /// <summary>
        /// disconnect from all remote hosts.
        /// </summary>
        public void Disconnect()
        {
            lock (this.receivingThreads)
            {
                foreach (KeyValuePair<Socket, ReceiveThread> kvp in this.receivingThreads)
                {
                    kvp.Value.Abort();
                    kvp.Key.Close();
                }
                this.receivingThreads.Clear();
            }
        }


        /// <summary>
        /// disconnect from the remote host according to the given endPoint.
        /// </summary>
        /// <param name="endPoint">endpoint to be desconnected.</param>
        public void Disconnect(object endPoint)
        {
            IPEndPoint ipEndPoint = endPoint as IPEndPoint;
            if (ipEndPoint == null)
            {
                throw new ArgumentException("The endPoint is not an IPEndPoint.", "endPoint");
            }

            lock (this.receivingThreads)
            {
                Socket sock = this.GetSocket(ipEndPoint.Address, ipEndPoint.Port);

                this.receivingThreads[sock].Abort();
                sock.Close();
                this.receivingThreads.Remove(sock);
            }
        }


        /// <summary>
        /// Send a packet to the first connected remote host.
        /// </summary>
        /// <param name="packet">packet to be sent.</param>
        [Obsolete("Use SendPacket(object endPoint, StackPacket packet) instead.")]
        public void SendPacket(StackPacket packet)
        {
            this.ValidServerHasStarted();

            lock (this.receivingThreads)
            {
                if (this.receivingThreads.Count < 1)
                {
                    throw new InvalidOperationException("No valid connection.");
                }

                Dictionary<Socket, ReceiveThread>.KeyCollection.Enumerator enumrator = this.receivingThreads.Keys.GetEnumerator();
                enumrator.MoveNext();
                Socket sock = enumrator.Current;

                this.SendPacket(sock.RemoteEndPoint as IPEndPoint, packet);
            }
        }


        /// <summary>
        /// Send arbitrary message against the transport.
        /// If server role, the packet is only be sent to the first connected remote host.
        /// </summary>
        /// <param name="message">the message to be sent.</param>
        public void SendBytes(byte[] message)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Send a packet to a special remote host.
        /// </summary>
        /// <param name="endPoint">the remote host to which the packet will be sent.</param>
        /// <param name="packet">the packet to be sent.</param>
        public void SendPacket(object endPoint, StackPacket packet)
        {
            ValidServerHasStarted();

            IPEndPoint ipEndPoint = endPoint as IPEndPoint;
            if (ipEndPoint == null)
            {
                throw new ArgumentException("The endPoint is not an IPEndPoint.", "endPoint");
            }

            // If we use raw bytes to construct the packet, we send the raw bytes out directly, 
            // otherwise we convert the packet to bytes and send it out.
            byte[] writeBytes = (packet.PacketBytes == null) ? packet.ToBytes() : packet.PacketBytes;

            Socket socket = GetSocket(ipEndPoint.Address, ipEndPoint.Port);

            if (socket == null)
            {
                throw new InvalidOperationException("The endPoint is not in the connect list.");
            }

            if (!socket.Connected)
            {
                throw new InvalidOperationException("The endPoint is not in state of connect.");
            }

            int sentLength = socket.Send(writeBytes, 0, writeBytes.Length, 0);

            if (sentLength != writeBytes.Length)
            {
                throw new InvalidOperationException("Fail to send data against the socket.");
            }
        }


        /// <summary>
        /// Send arbitrary message to a special remote host.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endpoint">the remote host to which the packet will be sent.</param>
        /// <param name="message">the message to be sent.</param>
        public void SendBytes(object endpoint, byte[] message)
        {
            IPEndPoint ipEndPoint = endpoint as IPEndPoint;

            Socket socket = GetSocket(ipEndPoint.Address, ipEndPoint.Port);

            int sentLength = socket.Send(message);

            if (sentLength != message.Length)
            {
                throw new InvalidOperationException("Fail to send data against the socket.");
            }
        }


        /// <summary>
        /// To close the transport and release resources.
        /// </summary>
        public void Release()
        {
            this.Dispose();
        }

        #endregion


        #region private methods

        /// <summary>
        /// Creates a new Socket for a newly created connection and  a new thread to receive packet in the loop.
        /// </summary>
        private void AcceptLoop()
        {
            while (!exitLoop)
            {
                if (this.receivingThreads.Count >= this.config.MaxConnections)
                {
                    // not listen untill the current connections are less than the max value.
                    // the interval to query is 1 seconds:
                    Thread.Sleep(1000);
                    continue;
                }

                Socket socket = null;
                try
                {
                    socket = this.listenSock.Accept();
                }
                catch (SocketException)
                {
                    exitLoop = true;
                    continue;
                }

                // if filter invalid, drop it.
                if (this.ConnectionFilter != null
                    && this.ConnectionFilter.FilterConnection((IPEndPoint)socket.RemoteEndPoint))
                {
                    socket.Close();
                    continue;
                }

                ReceiveThread receiveThread = new ReceiveThread(this.packetQueue, this.decoder,
                    new SocketReceive(socket.RemoteEndPoint, socket), this.config.BufferSize);

                TransportEvent connectEvent = new TransportEvent(EventType.Connected, socket.RemoteEndPoint, null);
                this.packetQueue.AddObject(connectEvent);

                lock (this.receivingThreads)
                {
                    this.receivingThreads.Add(socket, receiveThread);
                }

                receiveThread.Start();
            }
        }


        /// <summary>
        /// to confirm the server is started
        /// </summary>
        private void ValidServerHasStarted()
        {
            if (!this.isTcpServerTransportStarted)
            {
                throw new InvalidOperationException(
                    "the server is not started! please call Start() to start the server first.");
            }
        }


        /// <summary>
        /// Get a given socket object in the socket list through the ip address and port.
        /// </summary>
        /// <param name="address">The IP address of the remote host.</param>
        /// <param name="port">The port of the remote host.</param>
        /// <returns>
        /// Return the given socket if it exists in the remoteSockets list.
        /// Otherwise return null.
        /// </returns>
        private Socket GetSocket(IPAddress address, int port)
        {
            lock (this.receivingThreads)
            {
                foreach (Socket sock in this.receivingThreads.Keys)
                {
                    IPEndPoint endPoint = (IPEndPoint)sock.RemoteEndPoint;
                    if ((address == endPoint.Address) && (port == endPoint.Port))
                    {
                        return sock;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}

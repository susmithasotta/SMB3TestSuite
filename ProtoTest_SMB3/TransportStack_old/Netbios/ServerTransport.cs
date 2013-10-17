// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: Implement functions defined in IConnection for NetBios
// ------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.Protocols.TestTools.StackSdk;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// Implement functions defined in IConnection for NetBios
    /// </summary>
    internal class NetbiosServerTransport : IConnection, IDisposable
    {
        private NetbiosTransportConfig config;
        private QueueManager queue;
        private DecodePacketCallback decodeCallback;
        private NetbiosTransport transport;
        private volatile bool disposed;
        private Dictionary<byte, ReceiveThread> receiveThreads;
        private volatile bool exitLoop;
        private readonly object receiveThreadsLocker = new object();


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


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="transportConfig">The transport config</param>
        /// <param name="packetQueueManager">The message queue</param>
        /// <param name="decodePacketCallback">The decode callback</param>
        /// <exception cref="System.ArgumentException">
        /// Throw when the type of transportConfig is not NetbiosTransportConfig
        /// </exception>
        public NetbiosServerTransport(
            TransportConfig transportConfig,
            QueueManager packetQueueManager,
            DecodePacketCallback decodePacketCallback
            )
        {
            config = (NetbiosTransportConfig)transportConfig;

            if (config == null)
            {
                throw new ArgumentException("transportConfig is not a valid NetbiosTransportConfig");
            }

            this.queue = packetQueueManager;
            this.decodeCallback = decodePacketCallback;

            transport = new NetbiosTransport(config.LocalNetbiosName, config.AdapterIndex,
                (ushort)config.BufferSize, (byte)config.MaxSessions, (byte)config.MaxNames);

            receiveThreads = new Dictionary<byte, ReceiveThread>();
        }


        /// <summary>
        /// to update the config of transport at runtime.
        /// </summary>
        /// <param name="transportConfig">the config for transport</param>
        public void UpdateConfig(TransportConfig transportConfig)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// to start the transport.
        /// Genarally, it is only used for server role.
        /// </summary>
        public void Start()
        {
            Thread acceptThread = new Thread(AcceptLoop);
            acceptThread.Start();
        }


        /// <summary>
        /// connect to remote endpoint.
        /// Genarally, it is only used for client role.
        /// </summary>
        /// <returns>the identity of the connected endpoint.</returns>
        public object Connect()
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// disconnect from remote host.
        /// </summary>
        public void Disconnect()
        {
            lock (receiveThreads)
            {
                foreach (byte sessionNumber in receiveThreads.Keys)
                {
                    //it is a way to abort the receiving thread.
                    //another way is to cancel the RECEIVE Netbios command, but is more 
                    //complicated and need big change to the whole code, so here we do not
                    //use that.
                    this.transport.Disconnect(sessionNumber);
                }

                receiveThreads.Clear();
            }
        }


        /// <summary>
        /// disconnect from the remote host according to the given endPoint.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endPoint">endpoint to be desconnected.</param>
        public void Disconnect(object endPoint)
        {
            lock (receiveThreadsLocker)
            {
                this.transport.Disconnect((byte)endPoint);

                receiveThreads.Remove((byte)endPoint);
            }
        }


        /// <summary>
        /// Send a packet against the transport.
        /// If server role, the packet is only be sent to the first connected remote host.
        /// </summary>
        /// <param name="packet">the packet to be sent.</param>
        public void SendPacket(StackPacket packet)
        {
            throw new NotSupportedException();
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
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endPoint">the remote host to which the packet will be sent.</param>
        /// <param name="packet">the packet to be sent.</param>
        public void SendPacket(object endPoint, StackPacket packet)
        {
            this.transport.Send((byte)endPoint, packet.ToBytes());
        }


        /// <summary>
        /// Send arbitrary message to a special remote host.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endpoint">the remote host to which the packet will be sent.</param>
        /// <param name="message">the message to be sent.</param>
        public void SendBytes(object endpoint, byte[] message)
        {
            this.transport.Send((byte)endpoint, message);
        }

        /// <summary>
        /// To close the transport and release resources.
        /// </summary>
        public void Release()
        {
            Dispose();
        }


        /// <summary>
        /// This routine waiting for connection of clients
        /// </summary>
        private void AcceptLoop()
        {
            while (!exitLoop)
            {
                if (this.receiveThreads.Count >= this.config.MaxSessions)
                {
                    // not listen untill the current connections are less than the max value.
                    // the interval to query is 1 seconds:
                    Thread.Sleep(1000);
                    continue;
                }

                byte lsn;
                try
                {
                    lsn = transport.Listen();
                }
                catch (InvalidOperationException)
                {
                    exitLoop = true;
                    break;
                }

                TransportEvent transEvent = new TransportEvent(EventType.Connected, lsn, null);
                queue.AddObject(transEvent);

                ReceiveThread receiveThread = new ReceiveThread(queue, decodeCallback,
                    new NetbiosReceive(this.transport, lsn), config.BufferSize);

                lock (receiveThreadsLocker)
                {
                    receiveThreads.Add(lsn, receiveThread);
                }

                receiveThread.Start();
            }
        }

        #region IDisposable

        /// <summary>
        /// Release all resources and stop receiving and listening thread
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Release resources
        /// </summary>
        /// <param name="disposing">Indicate GC or user calling the function</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                Disconnect();
                exitLoop = true;
                transport.CancelListen();

                if (disposing)
                {
                    Disconnect();
                    this.transport.Dispose();
                }

                disposed = true;
            }
        }


        /// <summary>
        /// Deconstrucor
        /// </summary>
        ~NetbiosServerTransport()
        {
            Dispose(false);
        }


        #endregion
    }
}

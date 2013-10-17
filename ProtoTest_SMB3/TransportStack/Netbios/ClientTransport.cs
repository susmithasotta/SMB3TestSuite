// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: This is the NetBios client transport.
// ------------------------------------------------------------------------------

using System;
using Microsoft.Protocols.TestTools.StackSdk;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    internal sealed class NetbiosClientTransport : IConnection, IDisposable
    {
        #region fields

        private bool disposed;
        private NetbiosTransportConfig config;
        private DecodePacketCallback decoder;
        private QueueManager packetQueue;
        private ReceiveThread receiveThread;
        private NetbiosTransport transport;
        private string remoteNetbiosName;
        private int connectionId;
        private bool isConnected;

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


        #region constructor and dispose

        /// <summary>
        /// the constructor of NetbiosClientTransport. Initialize variables.
        /// </summary>
        /// <param name="transportConfig">Provides the transport parameters.</param>
        /// <param name="packetQueueManager">Store all event packets generated in receiving loop.</param>
        /// <param name="decodePacketCallback">Callback of decoding packet.</param>
        public NetbiosClientTransport(
            TransportConfig transportConfig,
            QueueManager packetQueueManager,
            DecodePacketCallback decodePacketCallback)
        {
            this.config = transportConfig as NetbiosTransportConfig;
            if (this.config == null)
            {
                throw new System.InvalidCastException("NetbiosClientTransport needs NetbiosTransportConfig.");
            }

            this.packetQueue = packetQueueManager;
            this.decoder = decodePacketCallback;

            this.transport = new NetbiosTransport(config.LocalNetbiosName, config.AdapterIndex, (ushort)transportConfig.BufferSize,
                (byte)this.config.MaxSessions, (byte)this.config.MaxNames);

            this.remoteNetbiosName = this.config.RemoteNetbiosName;
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
                    Disconnect();

                    transport.Dispose();

                    if (this.receiveThread != null)
                    {
                        this.receiveThread.Dispose();
                        this.receiveThread = null;
                    }

                    this.packetQueue.Dispose();
                    // Free managed resources & other reference types:
                }

                // Call the appropriate methods to clean up unmanaged resources.
                // If disposing is false, only the following code is executed:

                this.disposed = true;
            }
        }


        /// <summary>
        /// This destructor will get called only from the finalization queue.
        /// </summary>
        ~NetbiosClientTransport()
        {
            this.Dispose(false);
        }

        #endregion


        #region methods in IConnection

        /// <summary>
        /// to update the config of transport at runtime.
        /// </summary>
        /// <param name="transportConfig">the config for transport</param>
        public void UpdateConfig(TransportConfig transportConfig)
        {
            throw new NotImplementedException("Netbios client role does not implement this operation.");
        }


        /// <summary>
        /// to start the transport.
        /// Genarally, it is only used for server role.
        /// </summary>
        public void Start()
        {
            throw new NotSupportedException("Netbios client role does not support this operation.");
        }


        /// <summary>
        /// connect to remote endpoint.
        /// </summary>
        /// <returns>the identity of the connected endpoint.</returns>
        public object Connect()
        {
            if (isConnected)
            {
                throw new InvalidOperationException("Already connect to remote server");
            }

            connectionId= this.transport.Connect(this.remoteNetbiosName);
            isConnected = true;

            this.receiveThread = new ReceiveThread(this.packetQueue, this.decoder,
                new NetbiosReceive(transport, connectionId), this.config.BufferSize);
            this.receiveThread.Start();

            return connectionId;
        }


        /// <summary>
        /// disconnect from remote host.
        /// </summary>
        public void Disconnect()
        {
            if (!isConnected)
            {
                return;
            }

            transport.Disconnect(connectionId);

            isConnected = false;
        }


        /// <summary>
        /// disconnect from the remote host according to the given endPoint.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endPoint">endpoint to be desconnected.</param>
        public void Disconnect(object endPoint)
        {
            throw new NotSupportedException("Netbios client role does not support this operation.");
        }


        /// <summary>
        /// Send a packet against the transport.
        /// </summary>
        /// <param name="packet">the packet to be sent.</param>
        public void SendPacket(StackPacket packet)
        {
            if (!isConnected)
            {
                throw new InvalidOperationException("Netbios server has not been connected.");
            }

            // If we use raw bytes to construct the packet, we send the raw bytes out directly, 
            // otherwise we convert the packet to bytes and send it out.
            byte[] writeBytes = (packet.PacketBytes == null) ? packet.ToBytes() : packet.PacketBytes;

            this.transport.Send(connectionId, writeBytes);
        }


        /// <summary>
        /// Send arbitrary message against the transport.
        /// If server role, the packet is only be sent to the first connected remote host.
        /// </summary>
        /// <param name="message">the message to be sent.</param>
        public void SendBytes(byte[] message)
        {
            this.transport.Send(connectionId, message);
        }


        /// <summary>
        /// Send a packet to a special remote host.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endPoint">the remote host to which the packet will be sent.</param>
        /// <param name="packet">the packet to be sent.</param>
        public void SendPacket(object endPoint, StackPacket packet)
        {
            throw new NotSupportedException("Netbios client role does not support this operation.");
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

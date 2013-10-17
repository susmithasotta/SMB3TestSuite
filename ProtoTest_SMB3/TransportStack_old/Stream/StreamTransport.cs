//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Description: This file describes stream transport.
//
//
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Protocols.TestTools.StackSdk;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// Perform Stream transport.
    /// </summary>
    class StreamTransport : IConnection, IDisposable
    {
        #region Private fields

        /// <summary>
        /// the identity of stream.
        /// </summary>
        private static int streamIdentity = 1;

        /// <summary>
        /// the identity of stream.
        /// </summary>
        private static int StreamIdentity
        {
            get
            {
                return streamIdentity++;
            }
        }

        /// <summary>
        /// a endpoint from which the data is received.
        /// </summary>
        private object endpoint;

        /// <summary>
        /// the dispose flag.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// the receive message thread.
        /// </summary>
        private ReceiveThread receiveThread;

        /// <summary>
        /// the queue to store packets.
        /// </summary>
        private QueueManager packetQueue;

        /// <summary>
        /// the decode packet call back delegate.
        /// </summary>
        private DecodePacketCallback decoder;

        /// <summary>
        /// the config for transport.
        /// </summary>
        private StreamConfig config;

        /// <summary>
        /// the stream for read and write.
        /// </summary>
        private Stream stream;

        /// <summary>
        /// the stream for receive data.
        /// </summary>
        private StreamReceiver streamReceive;

        #endregion

        #region Constructor and Dispose

        /// <summary>
        /// the construct of StreamTransport. Initialize variables.
        /// </summary>
        /// <param name="config">TransportConfig</param>
        /// <param name="packetQueue">store all kinds of packets inherit from StackPacket</param>
        /// <param name="decodePacketCallback">Callback of packet decode</param>
        /// <exception cref="ArgumentNullException">the config MUST NOT be null.</exception>
        public StreamTransport(
            TransportConfig config,
            QueueManager packetQueue,
            DecodePacketCallback decodePacketCallback)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            this.config = config as StreamConfig;

            if (this.config == null)
            {
                throw new InvalidCastException("the config for StreamTransport must be StreamConfig");
            }

            this.packetQueue = packetQueue;
            this.decoder = decodePacketCallback;

            this.stream = this.config.Stream;

            this.endpoint = StreamIdentity;
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
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed and unmanaged resources.
                if (disposing)
                {
                    // Free managed resources & other reference types:
                    if (this.receiveThread != null)
                    {
                        this.receiveThread.Dispose();
                        this.receiveThread = null;
                    }

                    if (this.streamReceive != null)
                    {
                        this.streamReceive.Dispose();
                        this.streamReceive = null;
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
        ~StreamTransport()
        {
            this.Dispose(false);
        }


        #endregion

        #region IConnection Members

        /// <summary>
        /// ConnectionFilter filter.
        /// This filter is used when receiving connection. 
        /// Packets from the connection that are in filter will be dropped.
        /// </summary>
        public ConnectionFilter ConnectionFilter
        {
            get
            {
                throw new NotSupportedException("ConnectionFilter is not supported by StreamTransport.");
            }
            set
            {
                throw new NotSupportedException("ConnectionFilter is not supported by StreamTransport.");
            }
        }

        /// <summary>
        /// to update the config of transport at runtime.
        /// </summary>
        /// <param name="transportConfig">the config for transport</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void UpdateConfig(TransportConfig transportConfig)
        {
            if (transportConfig == null)
            {
                throw new ArgumentNullException("transportConfig");
            }

            this.config = transportConfig as StreamConfig;

            if (this.config.Stream == null)
            {
                throw new ArgumentException("Stream of StreamTransportConfig must not be null", "transportConfig");
            }

            this.stream = this.config.Stream;
            if (this.streamReceive != null)
            {
                this.streamReceive.Stream = this.stream;
            }
        }


        /// <summary>
        /// Send a packet to remote host.
        /// </summary>
        /// <param name="packet">packet to be sent.</param>
        public void SendPacket(StackPacket packet)
        {
            if (this.stream == null)
            {
                throw new InvalidOperationException("When stream is null, You must initialize it in config.");
            }

            // If we use raw bytes to construct the packet, we send the raw bytes out directly, 
            // otherwise we convert the packet to bytes and send it out.
            byte[] writeBytes = (packet.PacketBytes == null) ? packet.ToBytes() : packet.PacketBytes;

            this.stream.Write(writeBytes, 0, writeBytes.Length);
        }

        /// <summary>
        /// Send arbitrary message against the transport.
        /// If server role, the packet is only be sent to the first connected remote host.
        /// </summary>
        /// <param name="message">the message to be sent.</param>
        public void SendBytes(byte[] message)
        {
            this.stream.Write(message, 0, message.Length);
        }

        /// <summary>
        /// Send a packet to a remote host. client role do nothing.
        /// </summary>
        /// <param name="endPoint">the remote host.</param>
        /// <param name="packet">packet to be sent.</param>
        public void SendPacket(object endPoint, StackPacket packet)
        {
            throw new NotSupportedException("StreamTransport does not support this operation.");
        }


        /// <summary>
        /// Send arbitrary message to a special remote host.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endpoint">the remote host to which the packet will be sent.</param>
        /// <param name="message">the message to be sent.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames")]
        public void SendBytes(object endpoint, byte[] message)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// to start the transport.client role do nothing.
        /// </summary>
        public void Start()
        {
            throw new NotSupportedException("StreamTransport does not support this operation.");
        }


        /// <summary>
        /// connect to stream.
        /// </summary>
        public object Connect()
        {
            this.streamReceive = new StreamReceiver(this.endpoint, this.stream);

            // new and start the receive thread
            this.receiveThread = new ReceiveThread(this.packetQueue, this.decoder,
                this.streamReceive, this.config.BufferSize);

            receiveThread.Start();

            return this.endpoint;
        }


        /// <summary>
        /// disconnect from remote netbios.
        /// </summary>
        public void Disconnect()
        {
            if (this.stream != null)
            {
                this.stream.Close();
            }
        }


        /// <summary>
        /// disconnect to a client. client role do nothing.
        /// </summary>
        /// <param name="endPoint">the client endpint to desconnect.</param>
        public void Disconnect(object endPoint)
        {
            throw new NotSupportedException("StreamTransport does not support this operation.");
        }

        /// <summary>
        /// To close the transport and release resources.
        /// </summary>
        public void Release()
        {
            this.Dispose();
        }


        #endregion

        #region Other methods



        #endregion
    }
}

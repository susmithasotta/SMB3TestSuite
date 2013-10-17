// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: This is the main transport stack class provided for user to create
//              and operate all types of transport.
// ------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using Microsoft.Protocols.TestTools.StackSdk;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// to decode stack packet from the received message bytes.
    /// </summary>
    /// <param name="endPoint">the endpoint from which the message bytes are received.</param>
    /// <param name="messageBytes">the received message bytes to be decoded.</param>
    /// <param name="consumedLength">the length of message bytes consumed by decoder.</param>
    /// <param name="expectedLength">the length of message bytes the decoder expects to receive.</param>
    /// <returns>the stack packets decoded from the received message bytes.</returns>
    public delegate StackPacket[] DecodePacketCallback(
        object endPoint,
        byte[] messageBytes,
        out int consumedLength,
        out int expectedLength);


    /// <summary>
    /// Implement all methods in the interface of IConnection to perform the (dis)connect, send and receive 
    /// operations.
    /// </summary>
    public class TransportStack : IDisposable
    {
        #region fields

        private bool disposed;
        private IConnection transport;
        private QueueManager packetQueue;
        private PacketFilter packetFilter;

        #endregion


        #region properties

        /// <summary>
        /// To detect whether there are packets cached in the queue of TransportStack.
        /// </summary>
        public virtual bool IsDataAvailable
        {
            get
            {
                ReadOnlyCollection<object> objectList = this.packetQueue.ObjectList;

                foreach (object obj in objectList)
                {
                    TransportEvent transportEvent = obj as TransportEvent;
                    if (obj != null && transportEvent.EventType == EventType.ReceivedPacket)
                    {
                        return true;
                    }
                }

                return false;
            }
        }


        /// <summary>
        /// Packet filter. This filter is used in the call of ExpectPacket. 
        /// Packets in types of the filter will be dropped.
        /// </summary>
        public virtual PacketFilter PacketFilter
        {
            get
            {
                return this.packetFilter;
            }
            set
            {
                this.packetFilter = value;
            }
        }


        /// <summary>
        /// ConnectionFilter filter. This filter is used when receiving connection. 
        /// Packets from the connections of the filter will be dropped.
        /// </summary>
        public virtual ConnectionFilter ConnectionFilter
        {
            get
            {
                return this.transport.ConnectionFilter;
            }
            set
            {
                this.transport.ConnectionFilter = value;
            }
        }

        #endregion


        #region constructor and dispose

        /// <summary>
        /// the default constructor, do nothing.<para/>
        /// the mock class can invoke this constructor.
        /// </summary>
        protected TransportStack()
        {
        }


        /// <summary>
        /// Constructor. To create the instance of transport specified in the transportConfig.
        /// </summary>
        /// <param name="transportConfig">It specifies the transport type and parameters.</param>
        /// <param name="decodePacketCallback">The delegate of decoder.</param>
        public TransportStack(TransportConfig transportConfig, DecodePacketCallback decodePacketCallback)
        {
            if (transportConfig == null)
            {
                throw new ArgumentNullException("transportConfig");
            }

            if (decodePacketCallback == null)
            {
                throw new ArgumentNullException("decodePacketCallback");
            }

            this.packetQueue = new QueueManager();

            switch (transportConfig.Type)
            {
                case StackTransportType.Stream:
                    switch (transportConfig.Role)
                    {
                        case Role.Client:
                            transport = new StreamTransport(transportConfig, this.packetQueue, decodePacketCallback);
                            break;

                        default:
                            throw new NotSupportedException("This type of role is not supported.");
                    }
                    break;

                case StackTransportType.Tcp:
                    switch (transportConfig.Role)
                    {
                        case Role.Client:
                            this.transport = new TcpClientTransport(transportConfig, this.packetQueue,
                                decodePacketCallback);
                            break;

                        case Role.Server:
                            this.transport = new TcpServerTransport(transportConfig, this.packetQueue,
                                decodePacketCallback);
                            break;

                        case Role.P2P:
                            throw new NotImplementedException("The role of P2P has not implemented.");

                        default:
                            throw new NotSupportedException("This type of role is not supported.");
                    }
                    break;

                case StackTransportType.Udp:
                    switch (transportConfig.Role)
                    {
                        case Role.Client:
                        case Role.Server:
                            this.transport = new UdpTransport(transportConfig, this.packetQueue, decodePacketCallback);
                            break;
                        case Role.P2P:
                            throw new NotImplementedException("The role of P2P has not implemented.");
                        default:
                            throw new NotSupportedException("This type of role is not supported.");
                    }
                    break;

                case StackTransportType.Netbios:
                    switch (transportConfig.Role)
                    {
                        case Role.Client:
                            this.transport = new NetbiosClientTransport(transportConfig, this.packetQueue,
                                decodePacketCallback);
                            break;
                        case Role.Server:
                            this.transport = new NetbiosServerTransport(transportConfig, this.packetQueue,
                                decodePacketCallback);
                            break; 
                        case Role.P2P:
                            throw new NotImplementedException("The role of P2P has not implemented.");

                        default:
                            throw new NotSupportedException("This type of role is not supported.");
                    }
                    break;

                default:
                    throw new NotSupportedException("This type of transport is not supported.");
            }
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
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed and unmanaged resources.
                if (disposing)
                {
                    // Free managed resources & other reference types
                    if (this.transport != null)
                    {
                        this.transport.Release();
                        this.transport = null;
                    }

                    if (this.packetQueue != null)
                    {
                        this.packetQueue.Dispose();
                        this.packetQueue = null;
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
        ~TransportStack()
        {
            this.Dispose(false);
        }

        #endregion


        #region methods

        /// <summary>
        /// add event, StackSdk can notify TSD by call this method.
        /// </summary>
        /// <param name="transportEvent">the event to add to the queue</param>
        public virtual void AddEvent(TransportEvent transportEvent)
        {
            // No filter or filter valid, add packet to QueueManager.
            this.packetQueue.AddObject(transportEvent);
        }


        /// <summary>
        /// to update the config of transport at runtime.
        /// </summary>
        /// <param name="config">the config for transport</param>
        public virtual void UpdateConfig(TransportConfig config)
        {
            this.transport.UpdateConfig(config);
        }


        /// <summary>
        /// to start the transport. 
        /// Genarally, it is only used for server role.
        /// </summary>
        public virtual void Start()
        {
            this.transport.Start();
        }


        /// <summary>
        /// connect to remote endpoint.
        /// Genarally, it is only used for client role.
        /// </summary>
        /// <returns>the identity of the connected endpoint.</returns>
        public virtual object Connect()
        {
            return this.transport.Connect();
        }


        /// <summary>
        /// disconnect from remote host.
        /// </summary>
        public virtual void Disconnect()
        {
            this.transport.Disconnect();
        }


        /// <summary>
        /// disconnect from a special remote host.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endPoint">endpoint to be desconnected.</param>
        public virtual void Disconnect(object endPoint)
        {
            this.transport.Disconnect(endPoint);
        }


        /// <summary>
        /// Send a packet against the transport.
        /// If server role, the packet is only be sent to the first connected remote host.
        /// </summary>
        /// <param name="packet">the packet to be sent.</param>
        public virtual void SendPacket(StackPacket packet)
        {
            this.transport.SendPacket(packet);
        }


        /// <summary>
        /// Send a arbitrary message against the transport.
        /// If server role, the packet is only be sent to the first connected remote host.
        /// </summary>
        /// <param name="message">the message to be sent.</param>
        public virtual void SendBytes(byte[] message)
        {
            this.transport.SendBytes(message);
        }


        /// <summary>
        /// Send a packet to a special remote host.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endPoint">the remote host to which the packet will be sent.</param>
        /// <param name="packet">the packet to be sent.</param>
        public virtual void SendPacket(object endPoint, StackPacket packet)
        {
            this.transport.SendPacket(endPoint, packet);
        }


        /// <summary>
        /// Send arbitrary message to a special remote host.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endpoint">the remote host to which the packet will be sent.</param>
        /// <param name="message">the message to be sent.</param>
        public virtual void SendBytes(object endpoint, byte[] message)
        {
            this.transport.SendBytes(endpoint, message);
        }


        /// <summary>
        /// to receive an event packet from the transport. If the PacketFilter has been set and the type of
        /// TransportEvent is ReceivedPacket, PacketFilter will be used here: if the type of received packet
        /// is in types of the PacketFilter or the CustomizeFilter returns true, this packet will be dropped.
        /// </summary>
        /// <param name="timeout">Timeout of waiting for a a packet or event from the transport.</param>
        /// <returns>An transport event if existed. otherwise, throw timeout exception. If it recieved
        /// Exception Event, it will throw exception rather than return it back to caller.</returns>
        public virtual TransportEvent ExpectTransportEvent(TimeSpan timeout)
        {
            while (true)
            {
                TransportEvent eventPacket = this.packetQueue.GetObject(ref timeout) as TransportEvent;

                if (eventPacket == null)
                {
                    throw new InvalidOperationException("Invalid object are cached in the queue.");
                }

                // Use PacketFilter if ReceivedPacket:
                // if the type of received packet is in types of the PacketFilter or the CustomizeFilter returns true,
                // this packet will be dropped.
                if (eventPacket.EventType == EventType.ReceivedPacket)
                {
                    StackPacket packet = eventPacket.EventObject as StackPacket;

                    if (this.packetFilter != null
                        && (this.packetFilter.FilterPacket(packet) || this.packetFilter.CustomizeFilter(packet)))
                    {
                        continue;
                    }
                }
                else if (eventPacket.EventType == EventType.Exception)
                {
                    throw new InvalidOperationException(
                        "There's an exception thrown when receiving a packet.",
                        (Exception)eventPacket.EventObject);
                   
                }

                return eventPacket;
            }
        }

        #endregion
    }
}

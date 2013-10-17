// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: This is the generic receiving thread which is used to receive
//              data by all type of transport.
// ------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Protocols.TestTools.StackSdk;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// ReceiveThread is a new thread class used to receive data.
    /// </summary>
    internal sealed class ReceiveThread : IDisposable
    {
        #region fields

        private bool disposed;
        private QueueManager packetQueue;
        private DecodePacketCallback decoder;
        private Thread receivingThread;
        private IReceive receivingHost;
        private int maxBufferSize;
        private bool exitLoop;

        #endregion


        #region properties

        // none

        #endregion


        #region Constructor and Dispose

        /// <summary>
        /// Create a new thread to receive the data from remote host.
        /// </summary>
        /// <param name="packetQueueManager">Store all event packets generated in receiving loop.</param>
        /// <param name="decodePacketCallback">Callback of packet decode</param>
        /// <param name="receiveHost">the host used to receive data</param>
        /// <param name="bufferSize">Buffer size used in receiving data</param>
        public ReceiveThread(
            QueueManager packetQueueManager,
            DecodePacketCallback decodePacketCallback,
            IReceive receiveHost,
            int bufferSize)
        {
            if (packetQueueManager == null)
            {
                throw new ArgumentNullException("packetQueueManager");
            }

            if (decodePacketCallback == null)
            {
                throw new ArgumentNullException("decodePacketCallback");
            }

            // Initialize variable.
            this.packetQueue = packetQueueManager;
            this.decoder = decodePacketCallback;
            this.receivingHost = receiveHost;
            this.maxBufferSize = bufferSize;
            this.receivingThread = new Thread((new ThreadStart(ReceiveLoop)));
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
                    // Free managed resources & other reference types:
                    if (this.receivingThread != null)
                    {
                        this.Abort();
                        this.receivingThread = null;
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
        ~ReceiveThread()
        {
            this.Dispose(false);
        }

        #endregion

       
        #region methods

        /// <summary>
        /// to start the receiving thread.
        /// </summary>
        public void Start()
        {
            this.receivingThread.Start();
        }


        /// <summary>
        /// to abort the receiving thread.
        /// </summary>
        public void Abort()
        {
            this.exitLoop = true;
            this.receivingHost.Interupt();
            if (this.receivingThread.ThreadState != ThreadState.Unstarted)
            {
                this.receivingThread.Join();
            }
        }


        /// <summary>
        /// Receive data, decode Packet and add them to QueueManager in the loop.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ReceiveLoop()
        {
            StackPacket[] packets = null;
            ReceiveStatus receiveStatus = ReceiveStatus.Success;
            object endPoint = null;
            int bytesRecv = 0;
            int leftCount = 0;
            int expectedLength = this.maxBufferSize;
            byte[] receivedCaches = new byte[0];

            while (!exitLoop)
            {
                if (expectedLength <= 0)
                {
                    expectedLength = this.maxBufferSize;
                }

                byte[] receivingBuffer = new byte[expectedLength];
                try
                {
                    receiveStatus = this.receivingHost.Receive(receivingBuffer, out bytesRecv, out endPoint);
                }
                // The upper layer(SDK) is responsible for handling all exceptions:
                catch (Exception e)
                {
                    // Do not care exceptions thrown when the receing loop is exited by user or destructor:
                    if (!exitLoop)
                    {
                        if (this.packetQueue != null)
                        {
                            TransportEvent exceptionEvent = new TransportEvent(EventType.Exception, endPoint, e);
                            this.packetQueue.AddObject(exceptionEvent);
                            break;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (receiveStatus == ReceiveStatus.Success)
                {
                    while (true)
                    {
                        // link up caches and received bytes to decode function
                        byte[] data = new byte[bytesRecv + receivedCaches.Length];
                        Array.Copy(receivedCaches, data, receivedCaches.Length);
                        Array.Copy(receivingBuffer, 0, data, receivedCaches.Length, bytesRecv);

                        int consumedLength;

                        try
                        {
                            packets = this.decoder(endPoint, data, out consumedLength, out expectedLength);
                        }
                        catch (Exception e)
                        {
                            TransportEvent exceptionEvent = new TransportEvent(EventType.Exception, endPoint, e);
                            this.packetQueue.AddObject(exceptionEvent);

                            //if decoder throw exception, we think decoder will throw exception again when it decode
                            //subsequent received data So here we terminate the receive thread.
                            return;
                        }

                        leftCount = data.Length - consumedLength;
                        if (leftCount <= 0)
                        {
                            receivedCaches = new byte[0];
                        }
                        else
                        {
                            // update caches contents to the bytes which is not consumed
                            receivedCaches = new byte[leftCount];
                            Array.Copy(data, consumedLength, receivedCaches, 0, leftCount);
                        }
                        if (packets != null && packets.Length > 0)
                        {
                            AddPacketToQueueManager(endPoint, packets);
                            bytesRecv = 0;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else if (receiveStatus == ReceiveStatus.Disconnected)
                {
                    if (this.packetQueue != null)
                    {
                        TransportEvent disconnectEvent = new TransportEvent(EventType.Disconnected, endPoint, null);
                        this.packetQueue.AddObject(disconnectEvent);
                        break;
                    }
                    else
                    {
                        throw new InvalidOperationException("The transport is disconnected by remote host.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unknown status returned from receiving method.");
                }
            }
        }


        /// <summary>
        /// Add the given stack packets to the QueueManager object if packet type not in filters or Customize filter.
        /// </summary>
        /// <param name="endPoint">the endpoint of the packets</param>
        /// <param name="packets">decoded packets</param>
        private void AddPacketToQueueManager(object endPoint, StackPacket[] packets)
        {
            if (packets == null)
            {
                return;
            }

            foreach (StackPacket packet in packets)
            {
                TransportEvent packetEvent = new TransportEvent(EventType.ReceivedPacket, endPoint, packet);
                this.packetQueue.AddObject(packetEvent);
            }
        }

        #endregion
    }
}

// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: It is used to receive raw data from a Netbios session.
// ------------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// It is used to receive raw data from a Netbios session. 
    /// </summary>
    internal sealed class NetbiosReceive : IReceive
    {
        #region fields

        private NetbiosTransport transport;
        private int sessionId;

        #endregion


        #region Constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="transport">The adpter number</param>
        /// <param name="sessionId">The session number</param>
        public NetbiosReceive(NetbiosTransport transport, int sessionId)
        {
            this.transport = transport;
            this.sessionId = sessionId;
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
            byte[] receivedData = transport.Receive(sessionId);

            if (receivedData == null)
            {
                numBytesReceived = 0;
                endPoint = sessionId;
                return ReceiveStatus.Disconnected;
            }
            else
            {
                numBytesReceived = receivedData.Length;
                endPoint = sessionId;
                Array.Copy(receivedData, buffer, receivedData.Length);
                return ReceiveStatus.Success;
            }
        }


        /// <summary>
        /// interupt the blocked receiver. the receive thread will accept a exception and exit normally.
        /// </summary>
        public void Interupt()
        {
            this.transport.Disconnect(this.sessionId);
        }


        #endregion
    }
}

// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: This is the interface which must be implemented by each type of 
//              receiving host.
// ------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// the status of transport returned from receiving method.
    /// </summary>
    public enum ReceiveStatus : int
    {
        /// <summary>
        /// All transport should use this value to notify the caller that the transport has received
        /// valid data from the remost host.
        /// </summary>
        Success,

        /// <summary>
        /// All transport should use this value to notify the caller that the transport is disconnected
        /// by the remost host.
        /// </summary>
        Disconnected,
    }


    /// <summary>
    /// IReceivingHost defines actions for receiving raw data from different transport host. 
    /// </summary>
    public interface IReceive
    {
        /// <summary>
        /// To receive the raw data from transport. 
        /// </summary>
        /// <param name="buffer">buffer used to cache the received data.</param>
        /// <param name="numBytesReceived">if the returned status is Success, it is the length in bytes of
        /// the received data. Otherwise, it is meaningless.</param>
        /// <param name="endPoint">a network endpoint from which the data is received.</param>
        /// <returns>the status of transport.</returns>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        ReceiveStatus Receive(byte[] buffer, out int numBytesReceived, out object endPoint);


        /// <summary>
        /// interupt the blocked receiver. the receive thread will accept an exception and exit normally.
        /// </summary>
        void Interupt();
    }
}

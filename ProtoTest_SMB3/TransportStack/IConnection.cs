// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: This is the interface which must be implemented by each type of 
//              transport.
// ------------------------------------------------------------------------------

using System;
using Microsoft.Protocols.TestTools.StackSdk;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// IConnection defines all actions when interacting between the roles. 
    /// </summary>
    public interface IConnection
    {
        #region properties

        /// <summary>
        /// ConnectionFilter filter.
        /// This filter is used when receiving connection. 
        /// Packets from the connection that are in filter will be dropped.
        /// </summary>
        ConnectionFilter ConnectionFilter
        {
            get;
            set;
        }

        #endregion


        #region methods

        /// <summary>
        /// to update the config of transport at runtime.
        /// </summary>
        /// <param name="config">the config for transport</param>
        void UpdateConfig(TransportConfig config);


        /// <summary>
        /// to start the transport.
        /// Genarally, it is only used for server role.
        /// </summary>
        void Start();


        /// <summary>
        /// connect to remote endpoint.
        /// Genarally, it is only used for client role.
        /// </summary>
        /// <returns>the identity of the connected endpoint.</returns>
        object Connect();


        /// <summary>
        /// disconnect from remote host.
        /// </summary>
        void Disconnect();


        /// <summary>
        /// disconnect from the remote host according to the given endPoint.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endPoint">endpoint to be desconnected.</param>
        void Disconnect(object endPoint);


        /// <summary>
        /// Send a packet against the transport.
        /// If server role, the packet is only be sent to the first connected remote host.
        /// </summary>
        /// <param name="packet">the packet to be sent.</param>
        void SendPacket(StackPacket packet);


        /// <summary>
        /// Send arbitrary message against the transport.
        /// If server role, the packet is only be sent to the first connected remote host.
        /// </summary>
        /// <param name="message">the message to be sent.</param>
        void SendBytes(byte[] message);


        /// <summary>
        /// Send a packet to a special remote host.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endPoint">the remote host to which the packet will be sent.</param>
        /// <param name="packet">the packet to be sent.</param>
        void SendPacket(object endPoint, StackPacket packet);


        /// <summary>
        /// Send arbitrary message to a special remote host.
        /// Genarally, it is only used for server role.
        /// </summary>
        /// <param name="endpoint">the remote host to which the packet will be sent.</param>
        /// <param name="message">the message to be sent.</param>
        void SendBytes(object endpoint, byte[] message);


        /// <summary>
        /// To close the transport and release resources.
        /// </summary>
        void Release();

        #endregion
    }
}

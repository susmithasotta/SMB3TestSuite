//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3Event
// Description: smb3Event define event type and event information
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// smb3Event define event type and event information
    /// </summary>
    internal class smb3Event
    {
        private smb3EventType type;
        private smb3Packet packet;
        private int connectionId;
        private byte[] extraInfo;


        /// <summary>
        /// The type of the event
        /// </summary>
        public smb3EventType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }


        /// <summary>
        /// The received smb3Packet
        /// </summary>
        public smb3Packet Packet
        {
            get
            {
                return packet;
            }
            set
            {
                packet = value;
            }
        }


        /// <summary>
        /// The connectionID is used to find smb3Connection
        /// </summary>
        public int ConnectionId
        {
            get
            {
                return connectionId;
            }
            set
            {
                connectionId = value;
            }
        }


        /// <summary>
        /// The extra information about this event
        /// </summary>
        public byte[] ExtraInfo
        {
            get
            {
                return extraInfo;
            }
            set
            {
                extraInfo = value;
            }
        }
    }


    /// <summary>
    /// The type value of smb3Event
    /// </summary>
    internal enum smb3EventType
    {
        /// <summary>
        /// The target has been connected
        /// </summary>
        Connected,

        /// <summary>
        /// Receive a smb3 packet
        /// </summary>
        PacketReceived,

        /// <summary>
        /// Send a smb3 packet
        /// </summary>
        PacketSent,

        /// <summary>
        /// The target has been disconnected
        /// </summary>
        Disconnected,
    }

}

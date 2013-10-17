// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: It is used to notify user that a special event occurs in transport.
// ------------------------------------------------------------------------------

using System;
using Microsoft.Protocols.TestTools.StackSdk;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// the type of event maybe occur in transport.
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// A connect event occured in transport.
        /// </summary>
        Connected,

        /// <summary>
        /// A disconnect event occured in transport.
        /// </summary>
        Disconnected,

        /// <summary>
        /// A packet was received in transport.
        /// </summary>
        ReceivedPacket,

        /// <summary>
        /// An exception was thrown in transport.
        /// </summary>
        Exception,
    }


    /// <summary>
    /// This class is used to notify user that a special event occurs in transport.
    /// </summary>
    public class TransportEvent
    {
        #region fields

        private EventType eventType;
        private object endPoint;
        private object eventObject;

        #endregion


        #region properties

        /// <summary>
        /// the type of the occued event.
        /// </summary>
        public EventType EventType
        {
            get
            {
                return this.eventType;
            }
        }


        /// <summary>
        /// the endPoint from which the event occured.
        /// </summary>
        public object EndPoint
        {
            get
            {
                return this.endPoint;
            }
        }


        /// <summary>
        /// To contain some details of the occured event.
        /// </summary>
        public object EventObject
        {
            get
            {
                return this.eventObject;
            }
        }

        #endregion


        #region contructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">the type of the occued event.</param>
        /// <param name="endPoint">the endPoint from which the event occured.</param>
        /// <param name="detail">the details of the occured event. It may be null if no detail needed.</param>
        public TransportEvent(EventType type, object endPoint, object detail)
            : base()
        {
            this.eventType = type;
            this.endPoint = endPoint;
            this.eventObject = detail;
        }

        #endregion
    }
}

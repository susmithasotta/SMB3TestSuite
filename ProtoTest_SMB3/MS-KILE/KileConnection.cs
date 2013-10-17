//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------
using System;
using System.Net;

namespace Microsoft.Protocols.TestTools.StackSdk.Security.Kile
{
    /// <summary>
    /// Maintains the connection with a client
    /// </summary>
    public class KileConnection
    {
        /// <summary>
        /// Represents the target ip endpoint while sending packets
        /// </summary>
        private IPEndPoint targetEndPoint;


        /// <summary>
        /// Represents the target ip endpoint while sending packets
        /// </summary>
        public IPEndPoint TargetEndPoint
        {
            get
            {
                return targetEndPoint;
            }
        }

        #region Constructor

        /// <summary>
        /// Create a KileConnection object
        /// </summary>
        /// <param name="ipEndPoint">Represents the target ip endpoint while sending packets</param>
        public KileConnection(IPEndPoint ipEndPoint)
        {
            targetEndPoint = ipEndPoint;
        }

        #endregion
    }
}
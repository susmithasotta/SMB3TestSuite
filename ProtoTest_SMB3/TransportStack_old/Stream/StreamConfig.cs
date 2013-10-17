// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Description: Transport parameters configured by user. 
// ------------------------------------------------------------------------------

using System;
using System.IO;

namespace Microsoft.Protocols.TestTools.StackSdk.Transport
{
    /// <summary>
    /// StreamConfig stores the configurable parameters used by stream transport.
    /// </summary>
    public class StreamConfig : TransportConfig
    {
        #region fields

        private Stream stream;

        #endregion


        #region properties

        /// <summary>
        /// the stream for StreamTransport.
        /// </summary>
        public Stream Stream
        {
            get
            {
                return this.stream;
            }
            set
            {
                this.stream = value;
            }
        }

        #endregion


        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        public StreamConfig()
        {
            this.Type = StackTransportType.Stream;
        }

        #endregion
    }
}
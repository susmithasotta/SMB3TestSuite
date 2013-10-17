//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: OutStandingRequest
// Description: A stucture contains information about the request waiting
//              for server response
//-------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// The request which is waiting for response, this is a field in smb3ClientConnection
    /// </summary>
    public class smb3OutStandingRequest
    {
        internal ulong messageId;
        internal DateTime timeStamp;
        internal bool isHandleAsync;
        internal ulong asyncId;
        internal smb3Packet request;

        /// <summary>
        /// The messageId of the request
        /// </summary>
        public ulong MessageId
        {
            get
            {
                return messageId;
            }
        }

        /// <summary>
        /// A time stamp of when the request was sent
        /// </summary>
        public DateTime TimeStamp
        {
            get
            {
                return timeStamp;
            }
        }
        
        /// <summary>
        /// Indicate if this request is handled async
        /// </summary>
        public bool IsHandleAsync
        {
            get
            {
                return isHandleAsync;
            }
        }

        /// <summary>
        /// The asyncId of this request
        /// </summary>
        public ulong AsyncId
        {
            get
            {
                return asyncId;
            }
        }

        /// <summary>
        /// The request packet
        /// </summary>
        public smb3Packet Request
        {
            get
            {
                return request;
            }
        }
    }
}

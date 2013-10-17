// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Description: The transport of FileService Server
// ------------------------------------------------------------------------------

using System;
using System.Threading;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService
{
    /// <summary>
    /// The transport of FileService Server
    /// </summary>
    public abstract class FileServiceServerTransport : IDisposable
    {
        /// <summary>
        /// Start listen for client connection
        /// </summary>
        public abstract void Start();


        /// <summary>
        /// Expect tcp or netbios connection
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>The endpoint of client</returns>
        public abstract FsEndpoint ExpectConnect(TimeSpan timeout);


        /// <summary>
        /// Expect client to connect share "$IPC", tcp or netbios connect is not included
        /// </summary>
        /// <param name="timeout">timeout</param>
        /// <returns>The client endpoint</returns>
        public abstract FsEndpoint ExpectConnectIpcShare(TimeSpan timeout);


        /// <summary>
        /// Disconnect the connection specified by endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint</param>
        public abstract void Disconnect(FsEndpoint endpoint);


        /// <summary>
        /// Expect client send treedisconnect and server will send back treeDisconnect response
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>The endpoint of client</returns>
        public abstract FsEndpoint ExpectTreeDisconnect(TimeSpan timeout);


        /// <summary>
        /// Expect log off and server will send back log off response
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>The endpoint of client</returns>
        public abstract FsEndpoint ExpectLogOff(TimeSpan timeout);


        /// <summary>
        /// Expect tcp disconnect or netbios disconnect
        /// </summary>
        /// <param name="timeout">timeout</param>
        /// <returns>The endpoint of client</returns>
        public abstract FsEndpoint ExpectDisconnect(TimeSpan timeout);

        #region IDisposable

        /// <summary>
        /// Release all resources
        /// </summary>
        /// <param name="disposing">Indicate user or gc calling this function</param>
        protected virtual void Dispose(bool disposing)
        {
            //base class has nothing to dispose
        }


        /// <summary>
        /// Release all resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Deconstructor
        /// </summary>
        ~FileServiceServerTransport()
        {
            Dispose(false);
        }

        #endregion
    }
}

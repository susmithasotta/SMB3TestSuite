// ------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Description: The definitions of SmbVersion and SmbFamilyPacket used by FileAccessService
// ------------------------------------------------------------------------------

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService
{
    /// <summary>
    /// the family version: smb or smb2
    /// </summary>
    public enum SmbVersion
    {
        /// <summary>
        /// possible value
        /// </summary>
        SMB,

        /// <summary>
        /// possible value
        /// </summary>
        SMB2,
    }


    /// <summary>
    /// the abstract packet of smb family
    /// </summary>
    public abstract class SmbFamilyPacket : StackPacket
    {
        /// <summary>
        /// default constructor.
        /// </summary>
        protected SmbFamilyPacket()
            : base()
        {
        }


        /// <summary>
        /// constructor.
        /// </summary>
        /// <param name="data">the raw binary data of the packet.</param>
        protected SmbFamilyPacket(byte[] data)
            : base(data)
        {
        }


        /// <summary>
        /// he family version: smb or smb2
        /// </summary>
        public abstract SmbVersion Version
        {
            get;
        }
    }
}
//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: FSCC message
// Description: FSCC message defination
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Microsoft.Protocols.TestTools;
using Microsoft.Protocols.TestTools.StackSdk.Messages;
using Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// File information classes are numerical values (specified by the Level column in the following table) 
    /// that specify what information for a file is to be queried or set
    /// </summary>
    public enum FileInformationClasses
    {
        /// <summary>
        /// This information class is used to query the access rights of a file.
        /// </summary>
        FileAccessInformation = 8,

        /// <summary>
        /// The buffer alignment required by the underlying device.
        /// </summary>
        FileAlignmentInformation = 17,

        /// <summary>
        /// This information class is used to query a collection of file information structures.
        /// </summary>
        FileAllInformation = 18,

        /// <summary>
        /// This information class is used to query alternate name information for a file.
        /// </summary>
        FileAlternateNameInformation = 21,

        /// <summary>
        /// This information class is used to query for attribute and reparse tag information for a file.
        /// </summary>
        FileAttributeTagInformation = 35,

        /// <summary>
        /// This information class is used to query or set file information.
        /// </summary>
        FileBasicInformation = 4,

        /// <summary>
        /// This information class is used to query compression information for a file
        /// </summary>
        FileCompressionInformation = 28,

        /// <summary>
        /// This information class is used to query for the size of the extended attributes (EA) for a file.
        /// </summary>
        FileEaInformation = 7,

        /// <summary>
        /// This information class is used to query or set extended attribute (EA) information for a file.
        /// </summary>
        FileFullEaInformation = 15,

        /// <summary>
        /// This information class is used to query NTFS hard links to an existing file.
        /// </summary>
        FileHardLinkInformation = 46,

        /// <summary>
        /// This information class is used to query transactional visibility information for the files in a directory
        /// </summary>
        FileIdGlobalTxDirectoryInformation = 50,

        /// <summary>
        /// This information class is used to query for the file system's 8-byte file reference number for a file.
        /// </summary>
        FileInternalInformation = 6,

        /// <summary>
        /// This information class is used to query or set the mode of the file.
        /// </summary>
        FileModeInformation = 16,

        /// <summary>
        /// This information class is used to query for information on a network file open.
        /// </summary>
        FileNetworkOpenInformation = 34,

        /// <summary>
        /// Windows file systems do not implement this file information class; 
        /// the server will fail it with STATUS_NOT_SUPPORTED.
        /// </summary>
        FileNormalizedNameInformation = 48,

        /// <summary>
        /// This information class is used to query or set information on a named pipe that is not
        /// specific to one end of the pipe or another.
        /// </summary>
        FilePipeInformation = 23,

        /// <summary>
        /// This information class is used to query information on a named pipe 
        /// that is associated with the end of the pipe that is being queried.
        /// </summary>
        FilePipeLocalInformation = 24,

        /// <summary>
        /// This information class is used to query or set information on a named pipe 
        /// that is associated with the client end of the pipe that is being queried.
        /// </summary>
        FilePipeRemoteInformation = 25,

        /// <summary>
        /// This information class is used to query or set the position of the file pointer within a file.
        /// </summary>
        FilePositionInformation = 14,

        /// <summary>
        /// The information class is used to query quota information.
        /// </summary>
        FileQuotaInformation = 32,

        /// <summary>
        /// This information class is used to query or set reserved bandwidth for a file handle.
        /// </summary>
        FileSfioReserveInformation = 44,

        /// <summary>
        /// This information class is used to query file information
        /// </summary>
        FileStandardInformation = 5,

        /// <summary>
        /// This information class is used to query file link information
        /// </summary>
        FileStandardLinkInformation = 54,

        /// <summary>
        /// This information class is used to enumerate the data streams for a file.
        /// </summary>
        FileStreamInformation = 22
    }

    /// <summary>
    /// File system information classes are numerical values 
    /// (specified by the Level column in the following table) that specify what information
    /// on a particular instance of a file system on a volume is to be queried.
    /// </summary>
    public enum FileSystemInformationClasses
    {
        /// <summary>
        /// This information class is used to query attribute information for a file system.
        /// </summary>
        FileFsAttributeInformation = 5,

        /// <summary>
        /// This information class is used to query device information associated with a file system volume.
        /// </summary>
        FileFsDeviceInformation = 4,

        /// <summary>
        /// This information class is used to query sector size information for a file system volume.
        /// </summary>
        FileFsFullSizeInformation = 7,

        /// <summary>
        /// This information class is used to query or set the object ID for a file system data element.
        /// </summary>
        FileFsObjectIdInformation = 8,

        /// <summary>
        /// This information class is used to query sector size information for a file system volume.
        /// </summary>
        FileFsSizeInformation = 3,

        /// <summary>
        /// This information class is used to query information on a volume on which a file system is mounted.
        /// </summary>
        FileFsVolumeInformation = 1
    }

    /// <summary>
    /// This information class is used to query or set information on a named pipe 
    /// that is associated with the client end of the pipe that is being queried
    /// </summary>
    public struct FilePipeRemoteInformation
    {
        /// <summary>
        /// A LARGE_INTEGER that MUST contain the maximum amount of time counted 
        /// in 100-nanosecond intervals that will elapse before transmission of 
        /// data from the client machine to the server.
        /// </summary>
        public ulong CollectDataTime;

        /// <summary>
        /// A ULONG that MUST contain the maximum size in bytes of data that will 
        /// be collected on the client machine before transmission to the server.
        /// </summary>
        public uint MaximumCollectionCount;
    }

    /// <summary>
    /// A 32-bit unsigned integer referring to the current state of the pipe
    /// </summary>
    public enum Named_Pipe_State_Value
    {
        /// <summary>
        /// The specified named pipe is in the disconnected state
        /// </summary>
        FILE_PIPE_DISCONNECTED_STATE = 0x01,

        /// <summary>
        /// The specified named pipe is in the listening state
        /// </summary>
        FILE_PIPE_LISTENING_STATE = 0x02,

        /// <summary>
        /// The specified named pipe is in the connected state.
        /// </summary>
        FILE_PIPE_CONNECTED_STATE = 0x03,

        /// <summary>
        /// The specified named pipe is in the closing state.
        /// </summary>
        FILE_PIPE_CLOSING_STATE = 0x04
    }

    /// <summary>
    /// The FSCTL_PIPE_PEEK response returns data from the pipe server's output buffer in the FSCTL output buffer
    /// </summary>
    public struct FSCTL_PIPE_PEEK_Reply
    {
        /// <summary>
        /// A 32-bit unsigned integer referring to the current state of the pipe
        /// </summary>
        public Named_Pipe_State_Value NamedPipeState;

        /// <summary>
        /// A 32-bit unsigned integer that specifies the size, in bytes, of the data available to read from the pipe
        /// </summary>
        public uint ReadDataAvailable;

        /// <summary>
        /// A 32-bit unsigned integer that specifies the number of messages available
        /// in the pipe if the pipe has been created as a message-type pipe
        /// </summary>
        public uint NumberOfMessages;

        /// <summary>
        /// A 32-bit unsigned integer that specifies the length of the first message
        /// available in the pipe if the pipe has been created as a message-type pipe. 
        /// Otherwise, this field is 0
        /// </summary>
        public uint MessageLength;

        /// <summary>
        /// A byte buffer of preview data from the pipe.
        /// The length of the buffer is indicated by the value of the ReadDataAvailable field
        /// </summary>
        public byte[] Data;
    }
}

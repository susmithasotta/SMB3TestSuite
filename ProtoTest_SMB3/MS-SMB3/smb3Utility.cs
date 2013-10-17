//-------------------------------------------------------------------------
// Copyright(c) 2009 Microsoft Corporation
// All rights reserved
// 
// Module Name: smb3Utility
// Description: Utility function of smb3, used to marshal and unmarshal some
//              structure which is not covered by CreateXXX function
//-------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Protocols.TestTools.StackSdk.Messages;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.smb3
{
    /// <summary>
    /// Utility function of smb3
    /// </summary>
    public static class smb3Utility
    {
        /// <summary>
        /// Compounded requests MUST be aligned on 8-byte boundaries, so the alignment factor is 8.
        /// </summary>
        private const int alignmentFactor8 = 8;

        /// <summary>
        /// aligned on 4 bytes boundaries.
        /// </summary>
        private const int alignmentFactor4 = 4;

        // The max credits client can have.
        private const int maxCredits = 400;

        // If current credits < maxCredits - 50, the credits client will request.
        private const ushort creditsA = 10;

        // If maxCredits - 50 <= current credits < maxCredits, the credits the client will request.
        private const ushort creditsB = 1;

        //If current credits >= maxCredits, the credits the client will request.
        private const ushort creditsC = 0;

        //The max credits server will grand for one request.
        private const ushort maxCreditsServerGrandedInOneResponse = 100;

        private const int sizeOfWChar = 2;

        /// <summary>
        /// Convert a structure to byte array, this method mainly is used for marshal the structure which is
        /// not defined in smb3
        /// </summary>
        /// <typeparam name="T">The structure type</typeparam>
        /// <param name="dataStructure">The structure</param>
        /// <returns>The byte array</returns>
        public static byte[] ToBytes<T>(T dataStructure)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (Channel channel = new Channel(null, ms))
                {
                    channel.BeginWriteGroup();

                    channel.Write<T>(dataStructure);

                    channel.EndWriteGroup();

                    return ms.ToArray();
                }
            }
        }


        /// <summary>
        /// Convert byte array to structure,  this method mainly is used for un-marshal the structure which is
        /// not defined in smb3
        /// </summary>
        /// <typeparam name="T">The structure type</typeparam>
        /// <param name="data">The data buffer to be converted</param>
        /// <returns>The converted structure</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T ToStructure<T>(byte[] data)
        {
            return ToStructure<T>(data, 0, data.Length);
        }


        /// <summary>
        /// Convert byte array to structure,  this method mainly is used for un-marshal the structure which is
        /// not defined in smb3
        /// </summary>
        /// <typeparam name="T">The structure type</typeparam>
        /// <param name="data">The data buffer to be converted</param>
        /// <param name="offset">The offset of data from where the data will be used</param>
        /// <param name="count">The used dataLen</param>
        /// <returns>The converted structure</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T ToStructure<T>(byte[] data, int offset, int count)
        {
            using (MemoryStream ms = new MemoryStream(data, offset, count))
            {
                using (Channel channel = new Channel(null, ms))
                {
                    T marshaledStructure = channel.Read<T>();

                    return marshaledStructure;
                }
            }
        }


        /// <summary>
        /// Convert time format
        /// </summary>
        /// <param name="time">The DateTime format time</param>
        /// <returns>The _FILETIME format</returns>
        public static _FILETIME DateTimeToFileTime(DateTime time)
        {
            long dateTime = time.ToFileTimeUtc();

            _FILETIME fileTime = new _FILETIME();

            fileTime.dwLowDateTime = (uint)dateTime;
            fileTime.dwHighDateTime = (uint)(dateTime >> 32);

            return fileTime;
        }


        /// <summary>
        /// Create SymbolicLinkReparseBuffer
        /// </summary>
        /// <param name="flags">The flag</param>
        /// <param name="unparsedPathLength">The length, in bytes, of the unparsed portion of the path.</param>
        /// <param name="substituteName">The substitute name of the symbol link</param>
        /// <param name="printName">A friendly name</param>
        /// <returns>A SymbolicLinkReparseBuffer object</returns>
        public static SymbolicLinkReparseBuffer CreateSymbolicLinkReparseBuffer(
            SymbolicLinkReparseBuffer_Flags_Values flags,
            ushort unparsedPathLength,
            string substituteName,
            string printName
            )
        {
            if (substituteName == null)
            {
                throw new ArgumentNullException("substituteName");
            }

            if (printName == null)
            {
                throw new ArgumentNullException("printName");
            }

            SymbolicLinkReparseBuffer sybolicLinkReparse = new SymbolicLinkReparseBuffer();

            sybolicLinkReparse.Flags = flags;
            sybolicLinkReparse.PathBuffer = new byte[substituteName.Length * sizeOfWChar + printName.Length * sizeOfWChar];
            sybolicLinkReparse.PrintNameLength = (ushort)(printName.Length * sizeOfWChar);
            sybolicLinkReparse.ReparseDataLength = (ushort)(smb3Consts.StaticPortionSizeInSymbolicLinkErrorResponse
                + sybolicLinkReparse.PathBuffer.Length);
            sybolicLinkReparse.ReparseTag = SymbolicLinkReparseBuffer_ReparseTag_Values.IO_REPARSE_TAG_SYMLINK;
            sybolicLinkReparse.Reserved = 0;
            sybolicLinkReparse.SubstituteNameLength = (ushort)(substituteName.Length * sizeOfWChar);
            sybolicLinkReparse.SubstituteNameOffset = 0;
            sybolicLinkReparse.PrintNameOffset = (ushort)sybolicLinkReparse.SubstituteNameLength;

            Array.Copy(Encoding.Unicode.GetBytes(substituteName), sybolicLinkReparse.PathBuffer, sybolicLinkReparse.SubstituteNameLength);
            Array.Copy(Encoding.Unicode.GetBytes(printName), 0, sybolicLinkReparse.PathBuffer, sybolicLinkReparse.PrintNameOffset,
                sybolicLinkReparse.PrintNameLength);

            return sybolicLinkReparse;
        }


        /// <summary>
        /// Create CREATE_CONTEXT_Request_Values or Create CREATE_CONTEXT_Response_Values
        /// </summary>
        /// <param name="contextType">The context type</param>
        /// <param name="contextData">The context data, corresponding to the buffer TD 2.2.13.2 
        /// dataOffset, dataLen refered</param>
        /// <returns>The CREATE_CONTEXT_Values</returns>
        public static CREATE_CONTEXT_Values CreateCreateContextValues(
            CreateContextTypeValue contextType,
            byte[] contextData
            )
        {
            string contextName = null;

            switch (contextType)
            {
                case CreateContextTypeValue.SMB2_CREATE_EA_BUFFER:
                    contextName = "ExtA";
                    break;
                case CreateContextTypeValue.SMB2_CREATE_SD_BUFFER:
                    contextName = "SecD";
                    break;
                case CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_REQUEST:
                    contextName = "DHnQ";
                    break;
                case CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_RECONNECT:
                    contextName = "DHnC";
                    break;
                case CreateContextTypeValue.SMB2_CREATE_ALLOCATION_SIZE:
                    contextName = "AlSi";
                    break;
                case CreateContextTypeValue.SMB2_CREATE_QUERY_MAXIMAL_ACCESS_REQUEST:
                    contextName = "MxAc";
                    break;
                case CreateContextTypeValue.SMB2_CREATE_TIMEWARP_TOKEN:
                    contextName = "TWrp";
                    break;
                case CreateContextTypeValue.SMB2_CREATE_QUERY_ON_DISK_ID:
                    contextName = "QFid";
                    break;
                case CreateContextTypeValue.SMB2_CREATE_REQUEST_LEASE:
                    contextName = "RqLs";
                    break;
                case CreateContextTypeValue.SMB2_CREATE_REQUEST_LEASE_V2:
                    contextName = "RqLs";
                    break;
                case CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_REQUEST_V2:
                    contextName = "DH2Q";
                    break;
                case CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_RECONNECT_V2:
                    contextName = "DH2C";
                    break;
                case CreateContextTypeValue.SMB2_CREATE_APP_INSTANCE_ID:
                    contextName = "0x45BCA66AEFA7F74A9008FA462E144D74";
                    break;
            }

            if (contextName == null)
            {
                throw new ArgumentException("The contextType is not supported", "contextType");
            }

            CREATE_CONTEXT_Values createContext = new CREATE_CONTEXT_Values();

            createContext.NameOffset = smb3Consts.NameOffsetInCreateContextValues;

            createContext.NameLength = (ushort)contextName.Length;

            if (contextData != null)
            {
                createContext.DataOffset = (ushort)(createContext.NameOffset + smb3Utility.AlignBy8Bytes(
                    createContext.NameLength));

                createContext.DataLength = (uint)contextData.Length;
            }

            byte[] nameArray = Encoding.ASCII.GetBytes(contextName);

            if (createContext.DataOffset != 0)
            {
                createContext.Buffer = new byte[createContext.DataOffset - createContext.NameOffset
                    + createContext.DataLength];

                Array.Copy(nameArray, createContext.Buffer, nameArray.Length);
                Array.Copy(contextData, 0, createContext.Buffer, createContext.DataOffset - createContext.NameOffset,
                    contextData.Length);
            }
            else
            {
                createContext.Buffer = new byte[createContext.NameLength];

                Array.Copy(nameArray, createContext.Buffer, nameArray.Length);
            }

            return createContext;
        }


        /// <summary>
        /// Create FILE_NOTIFY_INFORMATION
        /// </summary>
        /// <param name="action">The changes that occurred on the file.  This field MUST
        ///  contain one of the following values.</param>
        /// <param name="fileName">The fileName</param>
        /// <returns>A FILE_NOTIFY_INFORMATION object</returns>
        public static FILE_NOTIFY_INFORMATION CreateFileNotifyInformation(
             Action_Values action,
             string fileName
            )
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            FILE_NOTIFY_INFORMATION fileNotifyInfo = new FILE_NOTIFY_INFORMATION();

            fileNotifyInfo.Action = action;
            fileNotifyInfo.FileName = Encoding.Unicode.GetBytes(fileName);
            fileNotifyInfo.FileNameLength = (uint)fileNotifyInfo.FileName.Length;

            return fileNotifyInfo;
        }


        /// <summary>
        /// Set offset field of FILE_NOTIFY_INFORMATION
        /// </summary>
        /// <param name="fileNotifyInformations">A list of FILE_NOTIFY_INFORMATION data</param>
        public static void SetOffset(FILE_NOTIFY_INFORMATION[] fileNotifyInformations)
        {
            if (fileNotifyInformations == null)
            {
                throw new ArgumentNullException("fileNotifyInformations");
            }

            for (int i = 0; i < fileNotifyInformations.Length - 1; i++)
            {
                fileNotifyInformations[i].NextEntryOffset = (uint)AlignBy4Bytes(ToBytes(fileNotifyInformations[i]).Length);
            }
        }


        /// <summary>
        /// Generate lease key for client to user in smb3CreateCreateRequest with
        /// SMB2_CREATE_REQUEST_LEASE createContext
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateLeaseKey()
        {
            Guid leaseKey = new Guid("12345678912345678912345678912345");//smb3Utility.CreateGuid()); //Guid.NewGuid();

            return leaseKey.ToByteArray();
        }

        public static byte[] GenerateLeaseKey1()
        {
            Guid leaseKey = new Guid("00000000000000000000000000000000 ");//("10987654321109876543211098765432");//smb3Client.globalContext.ClientGuid; //Guid.NewGuid();

            return leaseKey.ToByteArray();
        }
        /// <summary>
        /// A GUID that identifies the create request with
        /// SMB2_CREATE_DURABLE_HANDLE_REQUEST_V2 createContext
        /// </summary>
        /// <returns></returns>
        public static byte[] CreateGuid()
        {
            Guid CreateID = Guid.NewGuid(); //smb3Client.globalContext.ClientGuid;

            return CreateID.ToByteArray();
        }

        /// <summary>
        /// Padding bytes to the end of the originalBuffer to make sure the length is multiple of 8
        /// </summary>
        /// <param name="originalBuffer">The orginal buffer</param>
        /// <returns>The padded buffer</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Throw when originalBuffer is null.
        /// </exception>
        internal static byte[] PaddingTo8BytesAlignedBuffer(byte[] originalBuffer)
        {
            if (originalBuffer == null)
            {
                throw new ArgumentNullException("originalBuffer");
            }

            if (originalBuffer.Length % alignmentFactor8 != 0)
            {
                int paddingLen = alignmentFactor8 - originalBuffer.Length % alignmentFactor8;

                byte[] alignedBuffer = new byte[originalBuffer.Length + paddingLen];

                originalBuffer.CopyTo(alignedBuffer, 0);

                return alignedBuffer;
            }
            else
            {
                return (byte[])originalBuffer.Clone();
            }
        }


        /// <summary>
        /// Caculate the 8 aligned length for the given length
        /// </summary>
        /// <param name="originalLength">The orginal length</param>
        /// <returns>The caculated aligned length</returns>
        internal static int AlignBy8Bytes(int originalLength)
        {
            return AlignByNBytes(originalLength, alignmentFactor8);
        }


        /// <summary>
        /// Caculate the 4 aligned length for the given length
        /// </summary>
        /// <param name="originalLength">The orginal length</param>
        /// <returns>The caculated aligned length</returns>
        internal static int AlignBy4Bytes(int originalLength)
        {
            return AlignByNBytes(originalLength, alignmentFactor4);
        }


        /// <summary>
        /// Caculate aligned length for the orginal length
        /// </summary>
        /// <param name="originalLength">The original</param>
        /// <param name="alignmentFactor">The alignment factor</param>
        /// <returns>The caculated aligned length</returns>
        internal static int AlignByNBytes(int originalLength, int alignmentFactor)
        {
            if (originalLength < 0)
            {
                throw new ArgumentException("originalLength must larger than zero", "originalLength");
            }

            if (alignmentFactor <= 0)
            {
                throw new ArgumentException("originalLength must larger than zero", "alignmentFactor");
            }

            int remainder = originalLength % alignmentFactor;

            if (remainder > 0)
            {
                return originalLength + alignmentFactor - remainder;
            }
            else
            {
                return originalLength;
            }
        }


        /// <summary>
        /// Assemble processId and treeId to asyncId
        /// </summary>
        /// <param name="processId">The processId in smb3 packet header</param>
        /// <param name="treeId">The treeId in smb3 packt header</param>
        /// <returns></returns>
        internal static ulong AssembleToAsyncId(uint processId, uint treeId)
        {
            ulong asyncId = treeId * uint.MaxValue + processId;

            return asyncId;
        }


        /// <summary>
        /// Generate Tcp transport payload, this function is used to prefix length header to smb3 packet
        /// </summary>
        /// <param name="packet">The smb3 packet data</param>
        /// <returns>The generated tcp transport payload</returns>
        internal static byte[] GenerateTcpTransportPayLoad(byte[] packet)
        {
            byte[] temp = BitConverter.GetBytes(packet.Length);

            byte[] streamProtoclLen = new byte[temp.Length];

            //The length, in bytes, of the SMB message. This length is formatted as a 3-byte integer in network byte order.
            streamProtoclLen[1] = temp[2];
            streamProtoclLen[2] = temp[1];
            streamProtoclLen[3] = temp[0];

            byte[] tcpPayLoad = new byte[packet.Length + streamProtoclLen.Length];

            Array.Copy(streamProtoclLen, tcpPayLoad, streamProtoclLen.Length);
            Array.Copy(packet, 0, tcpPayLoad, streamProtoclLen.Length, packet.Length);

            return tcpPayLoad;
        }


        /// <summary>
        /// Test if the contents of the two array are matched.
        /// </summary>
        /// <param name="array1">The array to be tested</param>
        /// <param name="array2">The array to be tested</param>
        /// <returns>True if they contain same content, else false</returns>
        internal static bool AreEqual(byte[] array1, byte[] array2)
        {
            if (array1 == null && array2 == null)
            {
                return true;
            }

            if (array1 == null || array2 == null)
            {
                return false;
            }

            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Caculate the credits client will request or server will grand
        /// </summary>
        /// <param name="requestCredits">The credits client request</param>
        /// <param name="currentCredits">The currect credits client has</param>
        /// <returns>The credits client will request or server will grand</returns>
        internal static ushort CaculateResponseCredits(ushort requestCredits, int currentCredits)
        {
            if (currentCredits < (maxCredits - 5 * creditsA))
            {
                if (requestCredits == 0)
                {
                    if (currentCredits == 0)
                    {
                        //at lease make sure client will have 1 credits
                        return creditsB;
                    }
                    else
                    {
                        return requestCredits;
                    }
                }
                else if (requestCredits < maxCreditsServerGrandedInOneResponse)
                {
                    return requestCredits;
                }
                else
                {
                    return 1; // return creditsA;
                }
            }
            else if (currentCredits < maxCredits)
            {
                return creditsB;
            }
            else
            {
                return creditsC;
            }
        }


        /// <summary>
        /// Caculate the credits client will ask server for based on current credits it has
        /// </summary>
        /// <param name="currentCredits">The current credits it has</param>
        /// <returns>The credits it will request</returns>
        internal static ushort CaculateRequestCredits(int currentCredits)
        {
            return CaculateResponseCredits(maxCreditsServerGrandedInOneResponse, currentCredits);
        }


        /// <summary>
        /// Get the type of this create_context
        /// </summary>
        /// <param name="createContext">The create_context used in SMB2_CREATE packet</param>
        /// <returns>The type of the createContext</returns>
        public static CreateContextTypeValue GetContextType(CREATE_CONTEXT_Values createContext)
        {
            string name = Encoding.ASCII.GetString(createContext.Buffer, createContext.NameOffset - smb3Consts.CreateContextBufferStartIndex,
                createContext.NameLength);
            
            switch (name)
            {
                case "ExtA":
                    return CreateContextTypeValue.SMB2_CREATE_EA_BUFFER;
                case "SecD":
                    return CreateContextTypeValue.SMB2_CREATE_SD_BUFFER;
                case "DHnQ":
                    return CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_REQUEST;
                case "DHnC":
                    return CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_RECONNECT;
                case "AlSi":
                    return CreateContextTypeValue.SMB2_CREATE_ALLOCATION_SIZE;
                case "MxAc":
                    return CreateContextTypeValue.SMB2_CREATE_QUERY_MAXIMAL_ACCESS_REQUEST;
                case "TWrp":
                    return CreateContextTypeValue.SMB2_CREATE_TIMEWARP_TOKEN;
                case "QFid":
                    return CreateContextTypeValue.SMB2_CREATE_QUERY_ON_DISK_ID;
                case "RqLs":
                    {
                        if (createContext.DataLength == 32)
                            return CreateContextTypeValue.SMB2_CREATE_REQUEST_LEASE;
                         else
                            return CreateContextTypeValue.SMB2_CREATE_REQUEST_LEASE_V2;
                    }
             
                case "DH2Q":
                   return CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_REQUEST_V2;
                case "DH2C":
                   return CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_RECONNECT_V2;
                case "0x45BCA66AEFA7F74A9008FA462E144D74":
                   return CreateContextTypeValue.SMB2_CREATE_APP_INSTANCE_ID;
                default:
                    throw new ArgumentException("Can't determine the type according to name in buffer", "createContext");
            }
        }


        /// <summary>
        /// Get the data field in CREATE_CONTEXT_Values structure
        /// </summary>
        /// <param name="createContext">CREATE_CONTEXT_Values contains the data</param>
        /// <returns>The data</returns>
        internal static byte[] GetDataFieldInCreateContext(CREATE_CONTEXT_Values createContext)
        {
            if (createContext.DataLength == 0)
            {
                return null;
            }

            byte[] data = new byte[createContext.DataLength];
            Array.Copy(createContext.Buffer, createContext.DataOffset - smb3Consts.CreateContextBufferStartIndex,
                data, 0, data.Length);

            return data;
        }


        /// <summary>
        /// Convert byte array to a sequence of CREATE_CONTEXT_Values structure; the createContextArray is comes
        /// from the buffer field of SMB2_CREATE packet received. Internal use only
        /// </summary>
        /// <param name="createContextArray">The byte array of create context</param>
        /// <returns>a sequence of CREATE_CONTEXT_Values structure</returns>
        internal static CREATE_CONTEXT_Values[] ConvertByteArrayToCreateContexts(byte[] createContextArray)
        {
            List<CREATE_CONTEXT_Values> createContexts = new List<CREATE_CONTEXT_Values>();

            int nextOffset = -1;
            int contextIndex = 0;
            int singleContextLength = 0;

            while (nextOffset != 0)
            {
                nextOffset = BitConverter.ToInt32(createContextArray, contextIndex);

                if (nextOffset == 0)
                {
                    singleContextLength = createContextArray.Length - contextIndex;
                }
                else
                {
                    singleContextLength = nextOffset;
                }

                CREATE_CONTEXT_Values createContext = smb3Utility.ToStructure<CREATE_CONTEXT_Values>(createContextArray, contextIndex, 
                    singleContextLength);
                createContexts.Add(createContext);

                contextIndex += singleContextLength;
            }

            return createContexts.ToArray();
        }


        /// <summary>
        /// In the case of request does not have valid fileId, the fileId can be got from
        /// the compound response
        /// </summary>
        /// <param name="requestFileId">The fileId in request packet</param>
        /// <param name="singlePacket">The single response in compound packet</param>
        /// <returns>The real fileId of the request</returns>
        internal static FILEID ResolveFileIdInCompoundResponse(FILEID requestFileId, smb3SinglePacket singlePacket)
        {
            if (requestFileId.Persistent == ulong.MaxValue && requestFileId.Volatile == ulong.MaxValue)
            {
                int indexOfThisPacket = singlePacket.GetPacketIndexInCompoundPacket();

                for (int i = indexOfThisPacket; i >= 0; i--)
                {
                    if (singlePacket.OuterCompoundPacket.Packets[i].hasFileId)
                    {
                        FILEID fileId = singlePacket.OuterCompoundPacket.Packets[i].GetFileId();

                        if (fileId.Persistent != ulong.MaxValue || fileId.Volatile != ulong.MaxValue)
                        {
                            return fileId;
                        }
                    }
                }

                throw new InvalidOperationException("Can't get a valid fileId");
            }
            else
            {
                return requestFileId;
            }
        }
    }
}

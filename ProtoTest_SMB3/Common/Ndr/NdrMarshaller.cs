//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Protocols.TestTools.StackSdk
{
    #region NDR Engine Internal Structures

    /// <summary>
    /// Delegation function to allocate memory, used by NDR engine.
    /// Copied from midl.exe generated stub code.
    /// </summary>
    /// <param name="s">Buffer size.</param>
    /// <returns>Pointer to allocated memory.</returns>
    internal delegate IntPtr PfnAllocate(uint s);

    /// <summary>
    /// Delegation function to free allocated memory , used by NDR engine.
    /// Copied from midl.exe generated stub code.
    /// </summary>
    /// <param name="f">Pointer to allocated memory.</param>
    internal delegate void PfnFree(IntPtr f);

    /// <summary>
    /// Copied from midl.exe generated stub code.
    /// </summary>
    internal struct MIDL_TYPE_PICKLING_INFO
    {
        public uint Version;
        public uint Flags;
        [MarshalAs(
            UnmanagedType.ByValArray,
            ArraySubType = UnmanagedType.U4,
            SizeConst = 3)]
        public uint[] Reserved;
    }

    /// <summary>
    /// Copied from midl.exe generated stub code.
    /// </summary>
    internal struct RPC_VERSION
    {
        public ushort MajorVersion;
        public ushort MinorVersion;
    }

    /// <summary>
    /// Copied from midl.exe generated stub code.
    /// </summary>
    internal struct RPC_SYNTAX_IDENTIFIER
    {
        public Guid SyntaxGUID;
        public RPC_VERSION SyntaxVersion;
    }

    /// <summary>
    /// Copied from midl.exe generated stub code.
    /// </summary>
    internal struct RPC_CLIENT_INTERFACE
    {
        public uint Length;
        public RPC_SYNTAX_IDENTIFIER InterfaceId;
        public RPC_SYNTAX_IDENTIFIER TransferSyntax;
        public IntPtr DispatchTable;
        public uint RpcProtseqEndpointCount;
        public IntPtr RpcProtseqEndpoint;
        public uint Reserved;
        public IntPtr InterpreterInfo;
        public uint Flags;
    }

    /// <summary>
    /// Copied from midl.exe generated stub code.
    /// </summary>
    // suppress message "warning CA1049 : Implement IDisposable on 
    // 'Microsoft.Protocols.TestTools.StackSdk.MIDL_STUB_DESC'." because
    // this is a NDR engine defined structure.
    // NdrMarshaller will release unmanaged resources.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable")]
    internal struct MIDL_STUB_DESC
    {
        public IntPtr RpcInterfaceInformation;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public PfnAllocate pfnAllocate;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public PfnFree pfnFree;

        public IntPtr pHandle;

        public IntPtr apfnNdrRundownRoutines;
        public IntPtr aGenericBindingRoutinePairs;
        public IntPtr apfnExprEval;
        public IntPtr aXmitQuintuple;

        public IntPtr pFormatTypes;

        public int fCheckBounds;
        public uint Version;
        public IntPtr pMallocFreeStruct;
        public int MIDLVersion;
        public IntPtr CommFaultOffsets;
        public IntPtr aUserMarshalQuadruple;
        public IntPtr NotifyRoutineTable;
        public uint mFlags;
        public IntPtr CsRoutineTables;
        public IntPtr ProxyServerInfo;
        public IntPtr pExprInfo;
    }
    #endregion

    /// <summary>
    /// Convert unmanaged memory pointed by the IntPtr pointer
    /// to structure.
    /// The structure must be defined as struct, not class.
    /// If the structure contains no IntPtr members,
    /// the implementation can simply call Marshal.NdrPtrToStructure(pointer, type);
    /// </summary>
    /// <param name="pointer">The pointer to unmanaged memory.</param>
    /// <returns>Structure converted from unmanaged menory.</returns>
    public delegate object NdrPtrToStructure(IntPtr pointer);

    /// <summary>
    /// <para>Provides methods for encoding unmanged object to NDR-encoded data,
    /// and decoding NDR-encoded data to unmanged object.</para>
    /// 
    /// <para>Basic knowledges for NDR encoding/decoding please refer to:</para>
    /// <para>http://msdn.microsoft.com/en-us/library/ms764233(VS.85).aspx</para>
    /// <para>http://msdn.microsoft.com/en-us/library/aa378670(VS.85).aspx</para>
    /// 
    /// <para>To encode/decode your data type using this class correctly, 
    /// you need do following things:</para>
    /// 
    /// <para>1. Make sure your data type definition conforms to NDR standard:
    /// http://www.opengroup.org/onlinepubs/9629399/chap14.htm</para>
    /// 
    /// <para>2. Write IDL contains your data type, make sure the topmost data type
    /// has definition of pointer, for example:
    /// typedef struct ... MyType, *PMyType;</para>
    /// 
    /// <para>3. Write ACF using the [encode] and [decode] attributes as interface attributes
    /// for the topmost data type pointer, for example:
    /// typedef [encode, decode] PMyType;</para>
    /// 
    /// <para>4. Compile the IDL and ACF using midl.exe to generate stub code.
    /// More details please install Microsoft Platform SDK, 
    /// and refer to files under Samples\NetDS\RPC\Pickle\picklt.</para>
    /// 
    /// <para>5. Open the generated stub code with postfix "_c.c", locate at MIDL_TypeFormatString.
    /// It's a static const byte array. Copy this byte array to your C# code as the type format string.
    /// The type format string is the only parameter to construct this class.
    /// Note: You need to aplly NdrFcShort and NdrFcLong macros manually or by a regex tool.
    /// For example, NdrFcShort(0x1234) results {0x34, 0x12} and NdrFcLong(0x1234) results {0x34, 0x12, 0x0, 0x0}.</para>
    /// 
    /// <para>6. Open the generated stub code with postfix "_c.c", locate at function with postfix "_Encode" or "_Decode".</para>
    /// <para>It must looks like:                                                                                         </para>                               
    /// <para>PMyType_Encode(PMyType * _pType)                                                                            </para>  
    /// <para>    {                                                                                                       </para>          
    /// <para>          NdrMesTypeEncode2(                                                                                </para>  
    /// <para>                ...                                                                                         </para>  
    /// <para>                ( PFORMAT_STRING  )&amp;PrimitiveTypes__MIDL_TypeFormatString.Format[2],                    </para>  
    /// <para>                _pType);                                                                                    </para>     
    /// <para>    }                                                                                                       </para>  
    /// <para>The array index number in MIDL_TypeFormatString.Format[] is the format string offset.
    /// In your C# code, the format string offset is one of the parameters to call this class'
    /// Encode/Decode methods.</para>
    /// 
    /// <para>7. Define your data type in C# code. Be careful of the type mapping from C to C#,
    /// for example, C long is C# int (4 bytes), not C# long (8 bytes). Use MarshalAs attribute when appropriate.</para>
    /// 
    /// <para>8. Call this class using defined data type, type format string and offset to accomplish the 
    /// encoding/decoding tasks.</para>
    /// 
    /// <para>You can define multiple data types in IDL/ACF file according to above steps.
    /// They share the same type format string, each has its own format string offset.</para>
    /// </summary>
    public sealed class NdrMarshaller : IDisposable
    {
        #region variable

        /// <summary>
        /// Indicates the instance has been disposed or not.
        /// </summary>
        private bool disposed;

        private MIDL_TYPE_PICKLING_INFO __MIDL_TypePicklingInfo;
        private MIDL_STUB_DESC stubDesc;
        private byte[] typeFormatString;

        #endregion

        /// <summary>
        /// Initializes an instance of NdrMarshaller class from a type format string.
        /// </summary>
        /// <param name="midlTypeFormatString">The type format string used to initialize the object.
        /// More details about format string and how to generate it please refer to current class' summary.</param>
        public NdrMarshaller(byte[] midlTypeFormatString)
        {
            if (midlTypeFormatString == null || midlTypeFormatString.Length == 0)
            {
                throw new ArgumentNullException("midlTypeFormatString");
            }

            typeFormatString = new byte[midlTypeFormatString.Length];
            Buffer.BlockCopy(midlTypeFormatString, 0, typeFormatString, 0, midlTypeFormatString.Length);

            #region MIDL_TYPE_PICKLING_INFO __MIDL_TypePicklingInfo
            // Following values are assigned according to midl.exe generated rpc stub code:
            //    static MIDL_TYPE_PICKLING_INFO __MIDL_TypePicklingInfo =
            //    {
            //    0x33205054, /* Signature & version: TP 1 */
            //    0x3, /* Flags: Oicf NewCorrDesc */
            //    0,
            //    0,
            //    0,
            //    };
            // See summary of current class for details on how to generate rpc stub code.
            __MIDL_TypePicklingInfo = new MIDL_TYPE_PICKLING_INFO();
            __MIDL_TypePicklingInfo.Version = 0x33205054; /* Signature & version: TP 1 */
            __MIDL_TypePicklingInfo.Flags = 0x3; /* Flags: Oicf NewCorrDesc */
            __MIDL_TypePicklingInfo.Reserved = new uint[] { 0, 0, 0 };
            #endregion

            #region RPC_CLIENT_INTERFACE rpcClientInterface
            // Following values are assigned according to midl.exe generated rpc stub code:
            //        static const RPC_CLIENT_INTERFACE type_pickle___RpcClientInterface =
            //        {
            //        sizeof(RPC_CLIENT_INTERFACE),
            //        {{0x906B0CE0,0xC70B,0x1067,{0xB3,0x17,0x00,0xDD,0x01,0x06,0x62,0xDA}},{1,0}},
            //        {{0x8A885D04,0x1CEB,0x11C9,{0x9F,0xE8,0x08,0x00,0x2B,0x10,0x48,0x60}},{2,0}},
            //        0,
            //        0,
            //        0,
            //        0,
            //        0,
            //        0x00000000
            //        };
            // See summary of current class for details on how to generate rpc stub code.
            RPC_CLIENT_INTERFACE rpcClientInterface = new RPC_CLIENT_INTERFACE();
            rpcClientInterface.Length = (uint)Marshal.SizeOf(typeof(RPC_CLIENT_INTERFACE));
            rpcClientInterface.InterfaceId = new RPC_SYNTAX_IDENTIFIER();
            rpcClientInterface.InterfaceId.SyntaxGUID = new Guid(0x906B0CE0, 0xC70B, 0x1067, 0xB3, 0x17, 0x00, 0xDD, 0x01, 0x06, 0x62, 0xDA);
            rpcClientInterface.InterfaceId.SyntaxVersion = new RPC_VERSION();
            rpcClientInterface.InterfaceId.SyntaxVersion.MajorVersion = 1;
            rpcClientInterface.InterfaceId.SyntaxVersion.MinorVersion = 0;
            rpcClientInterface.TransferSyntax = new RPC_SYNTAX_IDENTIFIER();
            rpcClientInterface.TransferSyntax.SyntaxGUID = new Guid(0x8A885D04, 0x1CEB, 0x11C9, 0x9F, 0xE8, 0x08, 0x00, 0x2B, 0x10, 0x48, 0x60);
            rpcClientInterface.TransferSyntax.SyntaxVersion = new RPC_VERSION();
            rpcClientInterface.TransferSyntax.SyntaxVersion.MajorVersion = 2;
            rpcClientInterface.TransferSyntax.SyntaxVersion.MinorVersion = 0;
            rpcClientInterface.DispatchTable = IntPtr.Zero;
            rpcClientInterface.RpcProtseqEndpointCount = 0;
            rpcClientInterface.RpcProtseqEndpoint = IntPtr.Zero;
            rpcClientInterface.Reserved = 0;
            rpcClientInterface.InterpreterInfo = IntPtr.Zero;
            rpcClientInterface.Flags = 0x00000000;
            #endregion

            #region MIDL_STUB_DESC stubDesc
            // Following values are assigned according to midl.exe generated rpc stub code:
            //    static const MIDL_STUB_DESC type_pickle_StubDesc = 
            //    {
            //    (void *)& type_pickle___RpcClientInterface,
            //    MIDL_user_allocate,
            //    MIDL_user_free,
            //    &ImplicitPicHandle,
            //    0,
            //    0,
            //    0,
            //    0,
            //    Picklt__MIDL_TypeFormatString.Format,
            //    1, /* -error bounds_check flag */
            //    0x50004, /* Ndr library version */
            //    0,
            //    0x70001f4, /* MIDL Version 7.0.500 */
            //    0,
            //    0,
            //    0,  /* notify & notify_flag routine table */
            //    0x1, /* MIDL flag */
            //    0, /* cs routines */
            //    0,   /* proxy/server info */
            //    0
            //    };
            // See summary of current class for details on how to generate rpc stub code.
            stubDesc = new MIDL_STUB_DESC();
            stubDesc.RpcInterfaceInformation = Marshal.AllocHGlobal(Marshal.SizeOf(rpcClientInterface));
            Marshal.StructureToPtr(rpcClientInterface, stubDesc.RpcInterfaceInformation, false);
            stubDesc.pfnAllocate = new PfnAllocate(MIDL_user_allocate);
            stubDesc.pfnFree = new PfnFree(MIDL_user_free);
            stubDesc.apfnNdrRundownRoutines = IntPtr.Zero;
            stubDesc.aGenericBindingRoutinePairs = IntPtr.Zero;
            stubDesc.apfnExprEval = IntPtr.Zero;
            stubDesc.aXmitQuintuple = IntPtr.Zero;
            stubDesc.pFormatTypes = Marshal.AllocHGlobal(midlTypeFormatString.Length);
            Marshal.Copy(typeFormatString, 0, stubDesc.pFormatTypes, typeFormatString.Length);
            stubDesc.fCheckBounds = 1; /* -error bounds_check flag */
            stubDesc.Version = 0x50004; /* Ndr library version */
            stubDesc.pMallocFreeStruct = IntPtr.Zero;
            stubDesc.MIDLVersion = 0x600016e; /* MIDL Version 6.0.366 */
            stubDesc.CommFaultOffsets = IntPtr.Zero;
            stubDesc.aUserMarshalQuadruple = IntPtr.Zero;
            stubDesc.NotifyRoutineTable = IntPtr.Zero;  /* notify & notify_flag routine table */
            stubDesc.mFlags = 0x1; /* MIDL flag */
            stubDesc.CsRoutineTables = IntPtr.Zero; /* cs routines */
            stubDesc.ProxyServerInfo = IntPtr.Zero; /* proxy/server native */
            stubDesc.pExprInfo = IntPtr.Zero; /* Reserved5 */
            #endregion
        }


        /// <summary>
        /// Decodes a block of NDR-encoded data to a structure.
        /// </summary>
        /// <param name="type">Type of the decoded object.</param>
        /// <param name="buffer">The byte array containing NDR-encoded data.</param>
        /// <param name="formatStringOffset">Format string offset of the unmanged object data type.</param>
        /// <returns>The decoded structure.</returns>
        /// <exception cref="NdrException">Internal NDR function failed.</exception>
        public object Decode(Type type, byte[] buffer, int formatStringOffset)
        {
            return Decode(type, buffer, 0, buffer.Length, formatStringOffset);
        }


        /// <summary>
        /// Decodes a block of NDR-encoded data to a structure.
        /// </summary>
        /// <param name="type">Type of the decoded object.</param>
        /// <param name="buffer">The byte array containing NDR-encoded data.</param>
        /// <param name="index">Beginning of the NDR-encoded data in the buffer.</param>
        /// <param name="count">Length of the NDR-encoded data.</param>
        /// <param name="formatStringOffset">Format string offset of the unmanged object data type.</param>
        /// <returns>The decoded structure.</returns>
        /// <exception cref="NdrException">Internal NDR function failed.</exception>
        public object Decode(Type type, byte[] buffer, int index, int count, int formatStringOffset)
        {
            NdrPtrToStructure converter = delegate(IntPtr ptr)
            {
                return Marshal.PtrToStructure(ptr, type);
            };

            return Decode(converter, buffer, index, count, formatStringOffset);
        }

        
        /// <summary>
        /// Decodes a block of NDR-encoded data to a structure. If the <paramref name="converter"/> simply
        /// calls Marshal.PtrToStructure(IntPtr ptr, Type structureType) internally, use another overload method
        /// Decode(Type type, byte[] buffer, int index, int count, int formatStringOffset) instead.
        /// </summary>
        /// <param name="converter">Delegation to convert unmanaged pointer into object.</param>
        /// <param name="buffer">The byte array containing NDR-encoded data.</param>
        /// <param name="formatStringOffset">Format string offset of the unmanged object data type.</param>
        /// <returns>The decoded structure.</returns>
        /// <exception cref="NdrException">Internal NDR function failed.</exception>
        public object Decode(NdrPtrToStructure converter, byte[] buffer, int formatStringOffset)
        {
            return Decode(converter, buffer, 0, buffer.Length, formatStringOffset);
        }


        /// <summary>
        /// Decodes a block of NDR-encoded data to a structure. If the <paramref name="converter"/> simply
        /// calls Marshal.PtrToStructure(IntPtr ptr, Type structureType) internally, use another overload method
        /// Decode(Type type, byte[] buffer, int index, int count, int formatStringOffset) instead.
        /// </summary>
        /// <param name="converter">Delegation to convert unmanaged pointer into object.</param>
        /// <param name="buffer">The byte array containing NDR-encoded data.</param>
        /// <param name="index">Beginning of the NDR-encoded data in the buffer.</param>
        /// <param name="count">Length of the NDR-encoded data.</param>
        /// <param name="formatStringOffset">Format string offset of the unmanged object data type.</param>
        /// <returns>The decoded structure.</returns>
        /// <exception cref="NdrException">Internal NDR function failed.</exception>
        public object Decode(NdrPtrToStructure converter, byte[] buffer, int index, int count, int formatStringOffset)
        {
            IntPtr ndrHandle = IntPtr.Zero;
            IntPtr pObj = IntPtr.Zero;
            IntPtr pBuf = IntPtr.Zero;
            try
            {
                #region init handle and environment

                pBuf = Marshal.AllocHGlobal(buffer.Length);
                Marshal.Copy(buffer, index, pBuf, count);

                int rt = NativeMethods.MesDecodeBufferHandleCreate(pBuf, (uint)buffer.Length, out ndrHandle);
                if (rt != NdrError.RPC_S_OK)
                {
                    throw new NdrException(rt, "Failed to create handle on given buffer.\n");
                }

                if (stubDesc.pHandle != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(stubDesc.pHandle);
                }
                stubDesc.pHandle = Marshal.AllocHGlobal(Marshal.SizeOf(ndrHandle));
                Marshal.WriteIntPtr(stubDesc.pHandle, ndrHandle);

                #endregion

                byte[] format = new byte[typeFormatString.Length - formatStringOffset];
                Buffer.BlockCopy(typeFormatString, formatStringOffset, format, 0, format.Length);

                NativeMethods.NdrMesTypeDecode2(
                     ndrHandle,
                     ref __MIDL_TypePicklingInfo,
                     ref stubDesc,
                     format,
                     ref pObj);

                if (pObj == IntPtr.Zero)
                {
                    throw new NdrException("Failed to decode on given buffer and formatStringOffset.\n");
                }
                return converter(pObj);
            }
            finally
            {
                if (ndrHandle != IntPtr.Zero)
                {
                    int rt = NativeMethods.MesHandleFree(ndrHandle);
                    if (rt != NdrError.RPC_S_OK)
                    {
                        // This is in final block, the error should not cause another exception.
                        // TODO: log the error when there's common log method to use.
                    }
                }
                if (stubDesc.pHandle != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(stubDesc.pHandle);
                    stubDesc.pHandle = IntPtr.Zero;
                }
                if (pObj != IntPtr.Zero)
                {
                    MIDL_user_free(pObj);
                    pObj = IntPtr.Zero;
                }
                if (pBuf != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pBuf);
                    pBuf = IntPtr.Zero;
                }
            }
        }


        /// <summary>
        /// Encodes an unmanged structure to NDR-encoded data.
        /// </summary>
        /// <param name="pObj">Pointer to an unmanged structure.</param>
        /// <param name="formatStringOffset">Format string offset of the unmanged structure data type.</param>
        /// <returns>The byte array containing NDR-encoded data.</returns>
        /// <exception cref="NdrException">Input structure and offset are not NDR-standard.</exception>
        public byte[] Encode(IntPtr pObj, int formatStringOffset)
        {
            IntPtr buf = IntPtr.Zero;
            uint encodedSize = 0;
            IntPtr ndrHandle = IntPtr.Zero;
            try
            {
                #region init handle and environment

                int rt = NativeMethods.MesEncodeDynBufferHandleCreate(out buf, out encodedSize, out ndrHandle);
                if (rt != NdrError.RPC_S_OK)
                {
                    throw new NdrException(rt, "Failed to encode on given structure and formatStringOffset.\n");
                }

                if (stubDesc.pHandle != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(stubDesc.pHandle);
                }
                stubDesc.pHandle = Marshal.AllocHGlobal(Marshal.SizeOf(ndrHandle));
                Marshal.WriteIntPtr(stubDesc.pHandle, ndrHandle);

                #endregion

                byte[] format = new byte[typeFormatString.Length - formatStringOffset];
                Buffer.BlockCopy(typeFormatString, formatStringOffset, format, 0, format.Length);

                NativeMethods.NdrMesTypeEncode2(
                     ndrHandle,
                     ref __MIDL_TypePicklingInfo,
                     ref stubDesc,
                     format,
                     ref pObj);

                byte[] managedBuf = new byte[encodedSize];
                Marshal.Copy(buf, managedBuf, 0, (int)encodedSize);

                return managedBuf;
            }
            finally
            {
                if (ndrHandle != IntPtr.Zero)
                {
                    int rt = NativeMethods.MesHandleFree(ndrHandle);
                    if (rt != NdrError.RPC_S_OK)
                    {
                        // This is in final block, the error should not cause another exception.
                        // TODO: log the error when there's common log method to use.
                    }
                }
                if (stubDesc.pHandle != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(stubDesc.pHandle);
                    stubDesc.pHandle = IntPtr.Zero;
                }
            }
        }

        #region private methods

        /// <summary>
        /// Callback function to allocate memory, used by NDR engine.
        /// Copied from midl.exe generated stub code.
        /// </summary>
        /// <param name="s">Buffer size.</param>
        /// <returns>Pointer to allocated memory.</returns>
        private static IntPtr MIDL_user_allocate(uint s)
        {
            IntPtr pcAllocated = Marshal.AllocHGlobal((int)s + 15);
            IntPtr pcUserPtr;

            // 32 bits platform
            if (IntPtr.Size == sizeof(Int32))
            {
                // NDR engine will pad buffer according to NDR standard,
                // and leaves the padding as uninitialized.
                // So, to get consistency across test runs,
                // we must manually fill the allocated buffer with 0.
                for (int i = 0; i < (int)s + 15; i++)
                {
                    Marshal.WriteByte(new IntPtr(pcAllocated.ToInt32() + i), 0x0);
                }

                // align to 8
                pcUserPtr = new IntPtr((pcAllocated.ToInt32() + 7) & ~7);
                if (pcUserPtr == pcAllocated)
                {
                    pcUserPtr = new IntPtr(pcAllocated.ToInt32() + 8);
                }

                // record the offset
                byte offset = (byte)(pcUserPtr.ToInt32() - pcAllocated.ToInt32());
                Marshal.WriteByte(new IntPtr(pcUserPtr.ToInt32() - 1), offset);
            }
            // 64 bits platform
            else if (IntPtr.Size == sizeof(Int64))
            {
                // NDR engine will pad buffer according to NDR standard,
                // and leaves the padding as uninitialized.
                // So, to get consistency across test runs,
                // we must manually fill the allocated buffer with 0.
                for (int i = 0; i < (int)s + 15; i++)
                {
                    Marshal.WriteByte(new IntPtr(pcAllocated.ToInt64() + i), 0x0);
                }

                // align to 8
                pcUserPtr = new IntPtr((pcAllocated.ToInt64() + 7) & ~7);
                if (pcUserPtr == pcAllocated)
                {
                    pcUserPtr = new IntPtr(pcAllocated.ToInt64() + 8);
                }

                // record the offset
                byte offset = (byte)(pcUserPtr.ToInt64() - pcAllocated.ToInt64());
                Marshal.WriteByte(new IntPtr(pcUserPtr.ToInt64() - 1), offset);
            }
            // not supported
            else
            {
                throw new NotSupportedException("Platform is neither 32 bits nor 64 bits.");
            }

            return (pcUserPtr);
        }


        /// <summary>
        /// Callback function to free allocated memory , used by NDR engine.
        /// Copied from midl.exe generated stub code.
        /// </summary>
        /// <param name="f">Pointer to allocated memory.</param>
        private static void MIDL_user_free(IntPtr f)
        {
            byte offset;
            IntPtr pcAllocated;

            // 32 bits platform
            if (IntPtr.Size == sizeof(Int32))
            {
                offset = Marshal.ReadByte(new IntPtr(f.ToInt32() - 1));
                pcAllocated = new IntPtr(f.ToInt32() - (int)offset);
            }
            // 64 bits platform
            else if (IntPtr.Size == sizeof(Int64))
            {
                offset = Marshal.ReadByte(new IntPtr(f.ToInt64() - 1));
                pcAllocated = new IntPtr(f.ToInt64() - (int)offset);
            }
            // not supported
            else
            {
                throw new NotSupportedException("Platform is neither 32 bits nor 64 bits.");
            }
            
            Marshal.FreeHGlobal(pcAllocated);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Implement IDisposable.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // safely release RpcInterfaceInformation
                if (stubDesc.RpcInterfaceInformation != IntPtr.Zero)
                {
                    // Call the appropriate methods to clean up
                    // unmanaged resources here.
                    // If disposing is false,
                    // only the following code is executed.
                    Marshal.FreeHGlobal(stubDesc.RpcInterfaceInformation);
                    stubDesc.RpcInterfaceInformation = IntPtr.Zero;
                }

                // safely release pFormatTypes
                if (stubDesc.pFormatTypes != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(stubDesc.pFormatTypes);
                    stubDesc.pFormatTypes = IntPtr.Zero;
                }

                // Note disposing has been done.
                disposed = true;
            }
        }


        /// <summary>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in class derived from this class.
        /// </summary>
        ~NdrMarshaller()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
        #endregion
    }
}

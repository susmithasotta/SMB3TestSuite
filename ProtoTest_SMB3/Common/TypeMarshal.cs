//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------


using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Protocols.TestTools.StackSdk.Messages;
using Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling;
using Microsoft.Protocols.TestTools.StackSdk.Messages.Runtime.Marshaling;

namespace Microsoft.Protocols.TestTools.StackSdk
{
    /// <summary>
    /// RpceTypeMarshal is a type marshal/unmarshal class. 
    /// It can marshal a struct to c style unmanaged memory, or block memory.
    /// It can also unmarshal a unmanged c style memory or block memory to a struct.
    /// </summary>
    public static class TypeMarshal
    {
        /// <summary>
        /// Create native marshaller.
        /// </summary>
        /// <param name="type">Type of struct.</param>
        /// <param name="switchValue">Switch attribute value if presents.</param>
        /// <param name="sizeValue">Size attribute value if presents.</param>
        /// <param name="lengthValue">Length attribute value if presents.</param>
        /// <param name="marshalDesccriptor">marshal descriptor</param>
        /// <returns>created marshaller</returns>
        private static Marshaler CreateNativeMarshaller(
            Type type, 
            object switchValue, 
            object sizeValue, 
            object lengthValue, 
            out MarshalingDescriptor marshalDesccriptor)
        {
            Marshaler marshaller = new Marshaler(NativeMarshalingConfiguration.Configuration);

            SwitchAttribute switchAttr = null;
            SizeAttribute sizeAttr = null;
            LengthAttribute lengthAttr = null;
            string nameSuffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            if (switchValue != null)
            {
                string switchSymbolName = "switch_" + nameSuffix;
                marshaller.DefineSymbol(switchSymbolName, switchValue);
                switchAttr = new SwitchAttribute(switchSymbolName);
            }
            if (sizeValue != null)
            {
                string sizeSymbolName = "size_" + nameSuffix;
                marshaller.DefineSymbol(sizeSymbolName, sizeValue);
                sizeAttr = new SizeAttribute(sizeSymbolName);
            }
            if (lengthValue != null)
            {
                string lengthSymbolName = "length_" + nameSuffix;
                marshaller.DefineSymbol(lengthSymbolName, lengthValue);
                lengthAttr = new LengthAttribute(lengthSymbolName);
            }
            TypeCustomAttributeProvider attrProvider
                = new TypeCustomAttributeProvider(switchAttr, sizeAttr, lengthAttr);

            marshalDesccriptor = new MarshalingDescriptor(type, attrProvider);

            return marshaller;
        }


        /// <summary>
        /// Marshal a managed struct to unmanaged memory.
        /// </summary>
        /// <typeparam name="T">Type of struct.</typeparam>
        /// <param name="t">A value of a struct to marshal.</param>
        /// <returns>Marshalled unmanaged memory.</returns>
        public static SafeIntPtr ToIntPtr<T>(T t) where T : struct
        {
            return ToIntPtr(t, null, null, null);
        }


        /// <summary>
        /// Marshal a managed struct to unmanaged memory.
        /// </summary>
        /// <typeparam name="T">Type of struct.</typeparam>
        /// <param name="t">A struct to marshal.</param>
        /// <param name="switchValue">Switch attribute value if presents.</param>
        /// <param name="sizeValue">Size attribute value if presents.</param>
        /// <param name="lengthValue">Length attribute value if presents.</param>
        /// <returns>Marshalled unmanaged memory.</returns>
        public static SafeIntPtr ToIntPtr<T>(
            T t, 
            object switchValue, 
            object sizeValue, 
            object lengthValue) where T : struct
        {
            MarshalingDescriptor marshalDesc;
            Marshaler marshaller = CreateNativeMarshaller(
                typeof(T),
                switchValue,
                sizeValue,
                lengthValue,
                out marshalDesc);

            int size = marshaller.GetSize(marshalDesc, t);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            IRegion region = marshaller.MakeRegion(ptr, size);
            marshaller.MarshalInto(marshalDesc, region, t);

            return new TypeMarshalSafeIntPtr(marshaller, ptr);
        }


        /// <summary>
        /// Marshal a managed array to unmanaged memory.
        /// </summary>
        /// <typeparam name="T">Type of elements in the array.</typeparam>
        /// <param name="array">An array to marshal.</param>
        /// <returns>Marshalled unmanaged memory.</returns>
        public static SafeIntPtr ToIntPtr<T>(T[] array) where T : struct
        {
            if (array == null)
            {
                return IntPtr.Zero;
            }

            MarshalingDescriptor marshalDesc;
            Marshaler marshaller = CreateNativeMarshaller(
                typeof(T[]),
                null,
                array.Length,
                null,
                out marshalDesc);

            int size = marshaller.GetSize(marshalDesc, array);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            IRegion region = marshaller.MakeRegion(ptr, size);
            marshaller.MarshalInto(marshalDesc, region, array);

            return new TypeMarshalSafeIntPtr(marshaller, ptr);
        }


        /// <summary>
        /// Marshal a managed nullable structure to unmanaged memory.
        /// </summary>
        /// <typeparam name="T">Type of structure.</typeparam>
        /// <param name="t">A nullable structure to marshal.</param>
        /// <returns>Marshalled unmanaged memory.</returns>
        public static SafeIntPtr ToIntPtr<T>(T? t) where T : struct
        {
            if (t == null)
            {
                return IntPtr.Zero;
            }
            else
            {
                return ToIntPtr(t.Value);
            }
        }


        /// <summary>
        /// Marshal a managed nullable struct to unmanaged memory.
        /// </summary>
        /// <typeparam name="T">Type of struct.</typeparam>
        /// <param name="t">A nullable struct to marshal.</param>
        /// <param name="switchValue">Switch attribute value if presents.</param>
        /// <param name="sizeValue">Size attribute value if presents.</param>
        /// <param name="lengthValue">Length attribute value if presents.</param>
        /// <returns>Marshalled unmanaged memory.</returns>
        public static SafeIntPtr ToIntPtr<T>(
            T? t,
            object switchValue,
            object sizeValue,
            object lengthValue) where T : struct
        {
            if (t == null)
            {
                return IntPtr.Zero;
            }
            else
            {
                return ToIntPtr(t.Value, switchValue, sizeValue, lengthValue);
            }
        }


        /// <summary>
        /// Unmarshal unmanaged memory to a managed struct.
        /// </summary>
        /// <typeparam name="T">Type of struct.</typeparam>
        /// <param name="ptr">Unmanaged memory to unmarshal.</param>
        /// <returns>Unmarshalled struct.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T ToStruct<T>(IntPtr ptr) where T : struct
        {
            return ToStruct<T>(ptr, null, null, null);
        }


        /// <summary>
        /// Unmarshal unmanaged memory to a managed struct.
        /// </summary>
        /// <typeparam name="T">Type of struct.</typeparam>
        /// <param name="ptr">Unmanaged memory to unmarshal.</param>
        /// <param name="switchValue">Switch attribute value if presents.</param>
        /// <param name="sizeValue">Size attribute value if presents.</param>
        /// <param name="lengthValue">Length attribute value if presents.</param>
        /// <returns>Unmarshalled struct.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T ToStruct<T>(
            IntPtr ptr, 
            object switchValue, 
            object sizeValue,
            object lengthValue) where T : struct
        {
            MarshalingDescriptor marshalDesc;
            Marshaler marshaller = CreateNativeMarshaller(
                typeof(T),
                switchValue, 
                sizeValue, 
                lengthValue, 
                out marshalDesc);

            int size = marshaller.GetSize(marshalDesc, null);
            IRegion region = marshaller.MakeRegion(ptr, size);
            return (T)marshaller.UnmarshalFrom(marshalDesc, region);
        }


        /// <summary>
        /// Unmarshal unmanaged memory to a managed array.
        /// </summary>
        /// <typeparam name="T">Type of elements in array.</typeparam>
        /// <param name="ptr">Unmanaged memory to unmarshal.</param>
        /// <param name="size">Number of elements in the array.</param>
        /// <returns>Unmarshalled array.</returns>
        // Suppress CA1004 because we want to restrict to use struct only.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T[] ToArray<T>(IntPtr ptr, int size) where T : struct
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            if (size == 0)
            {
                return new T[0];
            }

            MarshalingDescriptor marshalDesc;
            Marshaler marshaller = CreateNativeMarshaller(
                typeof(T[]),
                null,
                size,
                null,
                out marshalDesc);

            int arraySize = marshaller.GetSize(marshalDesc, null);
            IRegion region = marshaller.MakeRegion(ptr, arraySize);
            return (T[])marshaller.UnmarshalFrom(marshalDesc, region);
        }


        /// <summary>
        /// Unmarshal unmanaged memory to a managed nullable struct.
        /// </summary>
        /// <typeparam name="T">Type of struct.</typeparam>
        /// <param name="ptr">Unmanaged memory to unmarshal.</param>
        /// <returns>Unmarshalled struct.</returns>
        // Suppress CA1004 because we want to restrict to use struct only.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T? ToNullableStruct<T>(IntPtr ptr) where T : struct
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            else
            {
                return ToStruct<T>(ptr);
            }
        }


        /// <summary>
        /// Unmarshal unmanaged memory to a managed nullable struct.
        /// </summary>
        /// <typeparam name="T">Type of struct.</typeparam>
        /// <param name="ptr">Unmanaged memory to unmarshal.</param>
        /// <param name="switchValue">Switch attribute value if presents.</param>
        /// <param name="sizeValue">Size attribute value if presents.</param>
        /// <param name="lengthValue">Length attribute value if presents.</param>
        /// <returns>Unmarshalled struct.</returns>
        // Suppress CA1004 because we want to restrict to use struct only.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T? ToNullableStruct<T>(
            IntPtr ptr,
            object switchValue, 
            object sizeValue,
            object lengthValue) where T : struct
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            else
            {
                return ToStruct<T>(ptr, switchValue, sizeValue, lengthValue);
            }
        }


        /// <summary>
        /// Get the size of native memory of a struct, 
        /// only the size of top level struct is counted, any pointer to memory is not included.
        /// </summary>
        /// <typeparam name="T">Type of struct.</typeparam>
        /// <returns>The size of native memory.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when type is not value type.
        /// </exception>
        // Suppress CA1004 because we want to restrict to use struct only.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static int GetNativeMemorySize<T>() where T : struct
        {
            return GetNativeMemorySize<T>(null, null, null);
        }


        /// <summary>
        /// Get the size of native memory of a struct, 
        /// only the size of top level struct is counted, any pointer to memory is not included.
        /// </summary>
        /// <typeparam name="T">Type of struct.</typeparam>
        /// <param name="switchValue">Switch attribute value if presents.</param>
        /// <param name="sizeValue">Size attribute value if presents.</param>
        /// <param name="lengthValue">Length attribute value if presents.</param>
        /// <returns>The size of native memory.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when type is not value type.
        /// </exception>
        // Suppress CA1004 because we want to restrict to use struct only.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static int GetNativeMemorySize<T>(
            object switchValue, 
            object sizeValue, 
            object lengthValue) where T : struct
        {
            MarshalingDescriptor marshalDesc;
            Marshaler marshaller = CreateNativeMarshaller(
                typeof(T),
                switchValue,
                sizeValue,
                lengthValue,
                out marshalDesc);

            return marshaller.GetSize(marshalDesc, null);
        }


        /// <summary>
        /// Marshal a struct to managed byte array.
        /// </summary>
        /// <typeparam name="T">Type of struct.</typeparam>
        /// <param name="t">A struct to marshal.</param>
        /// <returns>Marshalled managed byte array.</returns>
        public static byte[] ToBytes<T>(T t) where T : struct
        {
            int size;
            using (MessageUtils utils = new MessageUtils(null))
            {
                size = utils.GetSize(t);
            }

            byte[] buf = new byte[size];
            using (MemoryStream memoryStream = new MemoryStream(buf))
            {
                using (Channel channel = new Channel(null, memoryStream))
                {
                    channel.BeginWriteGroup();
                    channel.Write<T>(t);
                    channel.EndWriteGroup();
                    int actualSize = (int)channel.Stream.Position;
                    return ArrayUtility.SubArray(buf, 0, actualSize);
                }
            }
        }


        /// <summary>
        /// Unmarshal managed byte array to a struct.
        /// </summary>
        /// <typeparam name="T">Type of struct.</typeparam>
        /// <param name="data">byte array of data.</param>
        /// <returns>Unmarshalled struct.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when data is null.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T ToStruct<T>(byte[] data) where T : struct
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                using (Channel channel = new Channel(null, memoryStream))
                {
                    return channel.Read<T>();
                }
            }
        }


        /// <summary>
        /// Get the size of block memory of a struct.
        /// </summary>
        /// <typeparam name="T">Type of struct.</typeparam>
        /// <param name="t">A struct to marshal.</param>
        /// <returns>The size of block memory.</returns>
        public static int GetBlockMemorySize<T>(T t) where T : struct
        {
            return ToBytes(t).Length;
        }
    }
}

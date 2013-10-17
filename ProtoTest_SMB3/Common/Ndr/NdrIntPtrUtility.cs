//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Protocols.TestTools.StackSdk
{
    /// <summary>
    /// Provides a collection of methods for manipulating unmanaged memory, converting
    /// between unmanaged and managed types.
    /// </summary>
    public static class NdrIntPtrUtility
    {
        /// <summary>
        /// Frees a block of unmanaged memory pointed by a pointer,
        /// and set pointer to zero.
        /// Note: this method only applies to CoTask Memory.
        /// </summary>
        /// <param name="p">Pointer to a block of CoTask memory.</param>
        public static void FreePtr(ref IntPtr p)
        {
            if (p != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(p);
                p = IntPtr.Zero;
            }
        }


        /// <summary>
        /// Unmarshals data from an unmanaged block of memory to an array of managed object.
        /// </summary>
        /// <typeparam name="T">The type of the element of the array.</typeparam>
        /// <param name="ptr">Pointer to an unmanged block of memory.</param>
        /// <param name="count">Count of elements to be marshaled.</param>
        /// <returns>An array of managed object.</returns>
        // suppress CA1004 because this is utility method for array,
        // and returning object[] is not user-friendly.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T[] PtrToArray<T>(IntPtr ptr, uint count)
            where T : struct
        {
            return PtrToArray<T>(ptr, 0, count);
        }


        /// <summary>
        /// Unmarshals data from an unmanaged block of memory to an array of managed object,
        /// begins at the offset.
        /// </summary>
        /// <typeparam name="T">The type of the element of the array.</typeparam>
        /// <param name="ptr">Pointer to an unmanged block of memory.</param>
        /// <param name="count">Count of elements to be marshaled.</param>
        /// <param name="offset">The array address' offset from <paramref name="ptr"/></param>
        /// <returns>An array of managed object.</returns>
        // suppress CA1004 because this is utility method for array,
        // and returning object[] is not user-friendly.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T[] PtrToArray<T>(IntPtr ptr, int offset, uint count)
            where T : struct
        {
            if (ptr == IntPtr.Zero)
            {
                if (count == 0)
                {
                    return new T[0];
                }
                else
                {
                    throw new ArgumentNullException("ptr");
                }
            }

            T[] array = new T[count];

            int elementSize = Marshal.SizeOf(typeof(T));

            // 32 bits platform
            if (IntPtr.Size == sizeof(Int32))
            {
                int elementAddress = ptr.ToInt32() + offset;
                for (uint i = 0; i < count; i++)
                {
                    array[i] = (T)Marshal.PtrToStructure(new IntPtr(elementAddress), typeof(T));
                    elementAddress += elementSize;
                }
            }
            // 64 bits platform
            else if (IntPtr.Size == sizeof(Int64))
            {
                long elementAddress = ptr.ToInt64() + offset;
                for (uint i = 0; i < count; i++)
                {
                    array[i] = (T)Marshal.PtrToStructure(new IntPtr(elementAddress), typeof(T));
                    elementAddress += elementSize;
                }
            }
            // not supported
            else
            {
                throw new NotSupportedException("Platform is neither 32 bits nor 64 bits.");
            }

            return array;
        }


        /// <summary>
        /// Marshals data from an array of managed object to an unmanaged block of memory.
        /// </summary>
        /// <typeparam name="T">The type of the element of the array.</typeparam>
        /// <param name="array">An array of managed object.</param>
        /// <returns>Pointer to an unmanged block of memory.</returns>
        // suppress CA1004 because this is utility method for array,
        // and returning object[] is not user-friendly.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static IntPtr ArrayToPtr<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                return IntPtr.Zero;
            }

            int elementSize = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocCoTaskMem(elementSize * array.Length);

            ArrayToPtr<T>(array, ptr, 0);

            return ptr;
        }


        /// <summary>
        /// Marshals data from an array of managed object to an unmanaged block of memory,
        /// writes the unmanaged data to <paramref name="offset"/> from given <paramref name="ptr"/>.
        /// </summary>
        /// <typeparam name="T">The type of the element of the array.</typeparam>
        /// <param name="array">An array of managed object.</param>
        /// <param name="ptr">The unmanaged base address to write, must 
        /// have been allocated of enough memory by the caller.</param>
        /// <param name="offset">The offset to write to.</param>
        public static void ArrayToPtr<T>(T[] array, IntPtr ptr, int offset)
        {
            int elementSize = Marshal.SizeOf(typeof(T));

            // 32 bits platform
            if (IntPtr.Size == sizeof(Int32))
            {
                int ptrValue = ptr.ToInt32() + offset;
                for (uint i = 0; i < array.Length; i++)
                {
                    Marshal.StructureToPtr(array[i], new IntPtr(ptrValue), false);
                    ptrValue += elementSize;
                }
            }
            // 64 bits platform
            else if (IntPtr.Size == sizeof(Int64))
            {
                long ptrValue = ptr.ToInt64() + offset;
                for (uint i = 0; i < array.Length; i++)
                {
                    Marshal.StructureToPtr(array[i], new IntPtr(ptrValue), false);
                    ptrValue += elementSize;
                }
            }
            // not supported
            else
            {
                throw new NotSupportedException("Platform is neither 32 bits nor 64 bits.");
            }
        }
    }
}

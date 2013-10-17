///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  This source file is originally from the C# Multiprecision Arithmetic Library.
//  See http://codebox/csmal for details and updates.
//

using System;
using System.Diagnostics;

namespace Microsoft.Protocols.TestTools.StackSdk.MultiprecisionArithmetic
{
    /// <summary>
    /// An MpIntInPlace wraps an MpInt with an interface that makes it
    /// convenient to do operations that are not normally in-place, such
    /// as multiplication, without performing allocation.
    /// </summary>
    /// <remarks>
    /// The class works by copying the result back and forth between
    /// two MpInts.
    /// </remarks>
    public class MpIntInPlace
    {
        bool mpi1Active;
        MpInt mpi1;
        MpInt mpi2;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="mpi">The MpInt value</param>
        public MpIntInPlace(MpInt mpi)
        {
            this.mpi1 = mpi;
            this.mpi2 = MpInt.Zero;
            this.mpi1Active = true;
        }

        /// <summary>
        /// Get the wrapped MpInt.
        /// </summary>
        /// <remarks>
        /// It's safe to modify the returned MpInt and to pass it to
        /// methods of this class, but because this class works by
        /// copying back and forth between two MpInts, you must
        /// re-extract the property after calling any member of this
        /// class.
        /// </remarks>
        public MpInt MpInt
        {
            get
            {
                return mpi1Active ? mpi1 : mpi2;
            }
            set
            {
                if (mpi1Active)
                {
                    mpi1 = value;
                }
                else
                {
                    mpi2 = value;
                }
            }
        }


        /// <summary>
        /// Multipy the current MpInt value by the value specified by right
        /// </summary>
        /// <param name="right">The right side value of the Multipy operation</param>
        public void MultiplyInPlace(MpInt right)
        {
            if (mpi1Active)
            {
                mpi1.Multiply(right, mpi2);
            }
            else
            {
                mpi2.Multiply(right, mpi1);
            }
            mpi1Active = !mpi1Active;
        }


        /// <summary>
        /// Modulus the current MpInt value by the value specified by right
        /// </summary>
        /// <param name="right">The right side value of the Modulus operation</param>
        public void ModulusInPlace(MpInt right)
        {
            if (mpi1Active)
            {
                mpi1.DivideInPlace(right, mpi2);
            }
            else
            {
                mpi2.DivideInPlace(right, mpi1);
            }
            mpi1Active = !mpi1Active;
        }


        /// <summary>
        /// Reserve bits specified by bits
        /// </summary>
        /// <param name="bits">The count of the reserved bits</param>
        public void ReserveBits(int bits)
        {
            mpi1.ReserveBits(bits);
            mpi2.ReserveBits(bits);
        }


        /// <summary>
        /// Convert current object to string
        /// </summary>
        /// <returns>The converted string</returns>
        public override string ToString()
        {
            return MpInt.ToString();
        }
    }
}

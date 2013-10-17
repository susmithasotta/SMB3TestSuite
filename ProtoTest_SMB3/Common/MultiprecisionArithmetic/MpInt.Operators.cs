///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  This source file is originally from the C# Multiprecision Arithmetic Library.
//  See http://codebox/csmal for details and updates.
//

// Summary: The part of the MpInt class defining overloaded operators,
// for syntactic convenience. None of these perform much work, instead
// calling into named methods.

// Remarks:
// * Avoid using the +=, -=, etc. operators on MpInts. Instead, use the
//   provided AddInPlace, SubtractInPlace, etc. methods, which avoid
//   allocation of an intermediate result (provided they have allocated
//   enough bits reserved - use ReserveBits to ensure this).

using System;
using System.Diagnostics;

namespace Microsoft.Protocols.TestTools.StackSdk.MultiprecisionArithmetic
{
    public partial class MpInt : IComparable
    {
        /// <summary>
        /// The plus operation of two MpInt value
        /// </summary>
        /// <param name="left">The left side MpInt value of the plus operation</param>
        /// <param name="right">The right side MpInt value of the plus operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator +(MpInt left, MpInt right)
        {
            MpInt result = new MpInt(left);
            result.AddInPlace(right);
            return result;
        }


        /// <summary>
        /// The plus operation of MpInt value and int value
        /// </summary>
        /// <param name="left">The left side MpInt value of the plus operation</param>
        /// <param name="right">The right side int value of the plus operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator +(MpInt left, int right)
        {
            return left + new MpInt(right);
        }


        /// <summary>
        /// The plus operation of MpInt value and uint value
        /// </summary>
        /// <param name="left">The left side MpInt value of the plus operation</param>
        /// <param name="right">The right side uint value of the plus operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator +(MpInt left, uint right)
        {
            return left + new MpInt(right);
        }


        /// <summary>
        /// The plus operation of MpInt value and long value
        /// </summary>
        /// <param name="left">The left side MpInt value of the plus operation</param>
        /// <param name="right">The right side long value of the plus operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator +(MpInt left, long right)
        {
            return left + new MpInt(right);
        }


        /// <summary>
        /// The plus operation of MpInt value and ulong value
        /// </summary>
        /// <param name="left">The left side MpInt value of the plus operation</param>
        /// <param name="right">The right side ulong value of the plus operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator +(MpInt left, ulong right)
        {
            return left + new MpInt(right);
        }


        /// <summary>
        /// The subtract operation of two MpInt number
        /// </summary>
        /// <param name="left">The left side MpInt number of the subtract operation</param>
        /// <param name="right">The right side MpInt number of the subtract operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator -(MpInt left, MpInt right)
        {
            MpInt result = new MpInt(left);
            result.SubtractInPlace(right);
            return result;
        }


        /// <summary>
        /// The subtract operation of MpInt number and int number
        /// </summary>
        /// <param name="left">The left side MpInt number of the subtract operation</param>
        /// <param name="right">The right side int number of the subtract operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator -(MpInt left, int right)
        {
            return left - new MpInt(right);
        }


        /// <summary>
        /// The subtract operation of MpInt number and uint number
        /// </summary>
        /// <param name="left">The left side MpInt number of the subtract operation</param>
        /// <param name="right">The right side uint number of the subtract operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator -(MpInt left, uint right)
        {
            return left - new MpInt(right);
        }


        /// <summary>
        /// The subtract operation of MpInt number and long number
        /// </summary>
        /// <param name="left">The left side MpInt number of the subtract operation</param>
        /// <param name="right">The right side long number of the subtract operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator -(MpInt left, long right)
        {
            return left - new MpInt(right);
        }


        /// <summary>
        /// The subtract operation of MpInt number and ulong number
        /// </summary>
        /// <param name="left">The left side MpInt number of the subtract operation</param>
        /// <param name="right">The right side ulong number of the subtract operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator -(MpInt left, ulong right)
        {
            return left - new MpInt(right);
        }


        /// <summary>
        /// The subtract operation of current MpInt number and another MpInt number
        /// </summary>
        /// <param name="mpi">The number to be subtracted from current MpInt number</param>
        /// <returns>The operation result</returns>
        public static MpInt operator -(MpInt mpi)
        {
            MpInt result = new MpInt(mpi);
            result.NegateInPlace();
            return result;
        }


        /// <summary>
        /// The multipy operation of current MpInt number and another int number
        /// </summary>
        /// <param name="mpi">The left side MpInt number of the multipy operation</param>
        /// <param name="multiplier">The right side int number of the multipy operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator *(MpInt mpi, int multiplier)
        {
            MpInt result = new MpInt(mpi);
            result.MultiplyInPlace(multiplier);
            return result;
        }


        /// <summary>
        /// The multipy operation of current MpInt number and another uint number
        /// </summary>
        /// <param name="mpi">The left side MpInt number of the multipy operation</param>
        /// <param name="multiplier">The right side uint number of the multipy operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator *(MpInt mpi, uint multiplier)
        {
            MpInt result = new MpInt(mpi);
            result.MultiplyInPlace(multiplier);
            return result;
        }


        /// <summary>
        /// The multipy operation of current MpInt number and another MpInt number
        /// </summary>
        /// <param name="left">The left side MpInt number of the multipy operation</param>
        /// <param name="right">The right side MpInt number of the multipy operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator *(MpInt left, MpInt right)
        {
            return left.Multiply(right);
        }


        /// <summary>
        /// The division operation of current MpInt number and another int number
        /// </summary>
        /// <param name="mpi">The left side MpInt number of the division operation</param>
        /// <param name="divisor">The right side int number of the division operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator /(MpInt mpi, int divisor)
        {
            MpInt result = new MpInt(mpi);
            int remainder;
            result.DivideInPlace(divisor, out remainder);
            return result;
        }


        /// <summary>
        /// The division operation of current MpInt number and another uint number
        /// </summary>
        /// <param name="mpi">The left side MpInt number of the division operation</param>
        /// <param name="divisor">The right side uint number of the division operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator /(MpInt mpi, uint divisor)
        {
            MpInt result = new MpInt(mpi);
            uint remainder;
            result.DivideInPlace(divisor, out remainder);
            return result;
        }


        /// <summary>
        /// The division operation of current MpInt number and another MpInt number
        /// </summary>
        /// <param name="dividend">The left side MpInt number of the division operation</param>
        /// <param name="divisor">The right side MpInt number of the division operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator /(MpInt dividend, MpInt divisor)
        {
            MpInt result = new MpInt(dividend);
            MpInt remainder;
            result.DivideInPlace(divisor, out remainder);
            return result;
        }


        /// <summary>
        /// The modulo operation of current MpInt number and another uint number
        /// </summary>
        /// <param name="mpi">The left side MpInt number of the modulo operation</param>
        /// <param name="divisor">The right side uint number of the modulo operation</param>
        /// <returns>The operation result</returns>
        public static uint operator %(MpInt mpi, uint divisor)
        {
            return mpi.Modulus(divisor);
        }


        /// <summary>
        /// The modulo operation of current MpInt number and another MpInt number
        /// </summary>
        /// <param name="dividend">The left side MpInt number of the modulo operation</param>
        /// <param name="divisor">The right side MpInt number of the modulo operation</param>
        /// <returns>The operation result</returns>
        public static MpInt operator %(MpInt dividend, MpInt divisor)
        {
            MpInt dividendCopy = new MpInt(dividend);
            MpInt remainder;
            dividendCopy.DivideInPlace(divisor, out remainder);
            return remainder;
        }


        /// <summary>
        /// Left shift MpInt value
        /// </summary>
        /// <param name="left">The MpInt value</param>
        /// <param name="shiftAmount">The shift amount</param>
        /// <returns>The shifted MpInt value</returns>
        public static MpInt operator<<(MpInt left, int shiftAmount)
        {
            MpInt result = new MpInt(left);
            result.ShiftLeftInPlace(shiftAmount);
            return result;
        }


        /// <summary>
        /// Right shift MpInt value
        /// </summary>
        /// <param name="left">The MpInt value</param>
        /// <param name="shiftAmount">The shift amount</param>
        /// <returns>The shifted MpInt value</returns>
        public static MpInt operator>>(MpInt left, int shiftAmount)
        {
            MpInt result = new MpInt(left);
            result.ShiftRightInPlace(shiftAmount);
            return result;
        }


        /// <summary>
        /// The equal operation of the two MpInt number
        /// </summary>
        /// <param name="left">The left side MpInt number of the equal operation</param>
        /// <param name="right">The right side MpInt number of the equal operation</param>
        /// <returns>Return true if the two MpInt number is equal, otherwise false</returns>
        public static bool operator ==(MpInt left, MpInt right) 
        {
            return Compare(left, right) == 0;
        }


        /// <summary>
        /// The not equal operation of the two MpInt number
        /// </summary>
        /// <param name="left">The left side MpInt number of the not equal operation</param>
        /// <param name="right">The right side MpInt number of the not equal operation</param>
        /// <returns>Return true if the two MpInt number is not equal, otherwise false</returns>
        public static bool operator !=(MpInt left, MpInt right)
        {
            return Compare(left, right) != 0;
        }


        /// <summary>
        /// The less than operation of the two MpInt number
        /// </summary>
        /// <param name="left">The left side MpInt number of the less than operation</param>
        /// <param name="right">The right side MpInt number of the less than operation</param>
        /// <returns>Return true if the left is less than the right, otherwise false</returns>
        public static bool operator <(MpInt left, MpInt right) 
        {
            return Compare(left, right) < 0;
        }


        /// <summary>
        /// The larger than operation of the two MpInt number
        /// </summary>
        /// <param name="left">The left side MpInt number of the larger than operation</param>
        /// <param name="right">The right side MpInt number of the larger than operation</param>
        /// <returns>Return true if the left is larger than the right, otherwise false</returns>
        public static bool operator>(MpInt left, MpInt right) 
        {
            return Compare(left, right) > 0;
        }


        /// <summary>
        /// The less than or eqaul operation of the two MpInt number
        /// </summary>
        /// <param name="left">The left side MpInt number of the less than or eqaul operation</param>
        /// <param name="right">The right side MpInt number of the less than or eqaul operation</param>
        /// <returns>Return true if the left is less than or eqaul the right, otherwise false</returns>
        public static bool operator<=(MpInt left, MpInt right) 
        {
            return Compare(left, right) <= 0;
        }


        /// <summary>
        /// The larger than or eqaul operation of the two MpInt number
        /// </summary>
        /// <param name="left">The left side MpInt number of the larger than or eqaul operation</param>
        /// <param name="right">The right side MpInt number of the larger than or eqaul operation</param>
        /// <returns>Return true if the left is larger than or eqaul the right, otherwise false</returns>
        public static bool operator >=(MpInt left, MpInt right) 
        {
            return Compare(left, right) >= 0;
        }

    }
}

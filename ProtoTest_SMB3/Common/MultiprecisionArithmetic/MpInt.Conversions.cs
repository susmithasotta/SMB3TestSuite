///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  This source file is originally from the C# Multiprecision Arithmetic Library.
//  See http://codebox/csmal for details and updates.
//

// Summary: The part of the MpInt class defining conversions to/from
// built-in types and strings.

using System;
using System.Diagnostics;
using StringBuilder = System.Text.StringBuilder;

namespace Microsoft.Protocols.TestTools.StackSdk.MultiprecisionArithmetic
{
    public partial class MpInt : IComparable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="i">the initialize value</param>
        public MpInt(int i)
        {
            Initialize(i);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="u">the initialize value</param>
        public MpInt(uint u)
        {
            negative = false;
            AllocateBitsErase(sizeof(uint) * BitsPerByte);
            digits[0] = u;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lng">the initialize value</param>
        public MpInt(long lng)
        {
            negative = lng < 0;
            AllocateBitsErase(sizeof(long) * BitsPerByte);
#if CSMAL_64_BIT_DIGITS
            digits[0] = negative ? unchecked((ulong)(-lng)) : (ulong)lng;
#else
            digits[0] = unchecked(negative ? (uint)(-lng) : (uint)lng);
            digits[1] = unchecked(negative ? (uint)((-lng) >> 32) : (uint)(lng >> 32));
#endif
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ulng">the initialize value</param>
        public MpInt(ulong ulng)
        {
            negative = false;
            AllocateBitsErase(sizeof(ulong) * BitsPerByte);
#if CSMAL_64_BIT_DIGITS
            digits[0] = ulng;
#else
            digits[0] = unchecked((uint)ulng);
            digits[1] = unchecked((uint)(ulng >> 32));
#endif
        }

        private void Initialize(int i)
        {
            negative = i < 0;
            AllocateBitsErase(sizeof(int) * BitsPerByte);
            Initialize(negative ? (uint)(-i) : (uint)i);
        }

        private void Initialize(uint u)
        {
            AllocateBitsErase(sizeof(uint) * BitsPerByte);
            for (int i = 0; DigitBits * i < sizeof(uint) * BitsPerByte; i++)
            {
                digits[i] = u >> (DigitBits * i);
            }
        }


        /// <summary>
        /// Convert the decimalString to MpInt
        /// </summary>
        /// <param name="decimalString">The decimal string</param>
        /// <returns>The converted MpInt</returns>
        public static MpInt FromDecimalString(string decimalString)
        {
            if (decimalString.Length == 0)
            {
                throw new ArgumentException("Empty string is not a valid decimal numeral");
            }

            int i = 0;
            bool negative = false;
            if (decimalString[i] == '-')
            {
                negative = true;
                i++;
            }

            MpInt result = MpInt.Zero;

            for ( ; i < decimalString.Length; i++)
            {
                if (!Char.IsDigit(decimalString[i]))
                {
                    throw new ArgumentException(String.Format("Invalid character '{0}' in decimal numeral", decimalString[i]));
                }
                result.MultiplyInPlace(10);
                result.AddInPlace(new MpInt((int)decimalString[i] - '0'));
            }

            if (negative)
            {
                result.NegateInPlace();
            }

            return result;
        }

        private static MpInt intMinValue = new MpInt(int.MinValue);


        /// <summary>
        /// Indicate if it is int
        /// </summary>
        /// <returns>true if it is int, otherwise false</returns>
        public bool IsInt()
        {
            return this.Equals(intMinValue) || NumBits <= 31;
        }

        /// <summary>
        /// Converts an MpInt to an int, or throws an
        /// OverflowException if it's out of range.
        /// </summary>
        public static explicit operator int(MpInt mpi)
        {
            int nonzeroLength = mpi.GetNonzeroLength();
            if (nonzeroLength == 0)
            {
                return 0;
            }
            if (mpi.IsInt())
            {
                return mpi.negative ? (int)(-(long)(mpi.digits[0]))
                                    : (int)(mpi.digits[0]);
            }
            throw new OverflowException("Out of range converting MpInt to int");
        }

        /// <summary>
        /// Converts an MpInt to a uint, or throws an
        /// OverflowException if it's out of range.
        /// </summary>
        public static explicit operator uint(MpInt mpi)
        {
            int nonzeroLength = mpi.GetNonzeroLength();
            if (mpi.IsNonNegative() && 
                (nonzeroLength == 0 ||
                (nonzeroLength == 1 && mpi.digits[0] <= uint.MaxValue)))
            {
                return (uint)mpi.digits[0];
            }
            throw new OverflowException("Out of range converting MpInt to uint");
        }

        /// <summary>
        /// Converts an MpInt to a uint, or throws an
        /// OverflowException if it's out of range.
        /// </summary>
        public static explicit operator ulong(MpInt mpi)
        {
            int nonzeroLength = mpi.GetNonzeroLength();
            if (nonzeroLength == 0)
            {
                return (ulong)0;
            }
            if (mpi.IsNonNegative() && nonzeroLength == 1)
            {
                return mpi.digits[0];
            }

#if !CSMAL_64_BIT_DIGITS
            if (mpi.IsNonNegative() && nonzeroLength == 2)
            {

                return (mpi.digits[1] << 32) | mpi.digits[0];
            }
#endif
            throw new OverflowException("Out of range converting MpInt to uint");
        }


        /// <summary>
        /// Convert the current object to string
        /// </summary>
        /// <returns>The converted string</returns>
        public override string ToString()
        {
            return ToDecimalString();
        }

        private string ToDecimalString()
        {
            if (IsZero())
            {
                return negative ? "-0" : "0";
            }

            StringBuilder sb = new StringBuilder();
            MpInt copy = new MpInt(this);
            uint decimalDigit;

            while (!copy.IsZero())
            {
                copy.DivideInPlace(10u, out decimalDigit);
                sb.Append(String.Format("{0}", decimalDigit));
            }

            if (negative)
            {
                sb.Append("-");
            }

            // Digits are in reverse order - reverse them in place
            for (int i = 0; i < sb.Length / 2; i++)
            {
                char temp = sb[i];
                sb[i] = sb[sb.Length - 1 - i];
                sb[sb.Length - 1 - i] = temp;
            }

            return sb.ToString();
        }

        private string ToHexString()
        {
            if (IsZero())
            {
                return negative ? "-0" : "0";
            }

            StringBuilder sb = new StringBuilder();
            if (negative)
            {
                sb.Append("-");
            }

            int i = GetNonzeroLength() - 1;
            sb.Append(digits[i].ToString("X"));
            i--;

#if CSMAL_64_BIT_DIGITS
            const string formatString = "X16";
#else
            const string formatString = "X8";
#endif
            for ( ; i >= 0; i--)
            {
                sb.Append(digits[i].ToString(formatString));
            }

            return sb.ToString();
        }

        internal static MpInt FromComplexDoubleSequence(ComplexDoubleSequence complexDoubleSequence, int baseBits)
        {
            MpInt result = MpInt.Zero;
            result.ReserveBits(complexDoubleSequence.Length * baseBits);
            ulong carry = 0;
            int i;
            for (i = 0; i < complexDoubleSequence.Length; i++)
            {
                ulong sum = ((ulong)Math.Round(complexDoubleSequence[i].Re)) + carry;
                result.PackDigitOverZeros(i, baseBits,
                                          (uint)(sum & ((1UL << baseBits) - 1UL)));
                carry = sum >> baseBits;
            }
            result.PackDigitOverZeros(i, baseBits, unchecked((uint)carry));
            return result;
        }
    }
}

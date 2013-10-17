///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  This source file is originally from the C# Multiprecision Arithmetic Library.
//  See http://codebox/csmal for details and updates.
//

// Remarks:
// * Define CSMAL_64_BIT_DIGITS at the project level to use 64-bit ulongs in the
//   digit arrays. You can set DigitBits to determine the base used (the
//   number of bits used in each digit), but be sure to set
//   CSMAL_AT_LEAST_32_BIT_DIGITS if DigitBits is >= 32.

using System;
using System.Diagnostics;

namespace Microsoft.Protocols.TestTools.StackSdk.MultiprecisionArithmetic
{
    /// <summary>
    /// A multi-precision integer (BigInt), allowing the representation of
    /// signed integers of arbitrarily large magnitude without overflow and
    /// basic arithmetic operations on those integers.
    /// </summary>
    public partial class MpInt : IComparable
    {
        //
        // We use a signed-magnitude representation. The magnitude is represented
        // as a standard radix 2^k place value representation, where 2^k is the
        // digit size, least significant digit at index zero.
        //

        const int BitsPerByte = 8;
        bool negative = false;
#if CSMAL_64_BIT_DIGITS
        ulong[] digits = new ulong[0];
        const int DigitBytes = sizeof(ulong);
        internal const int DigitBits = 63;
        const ulong DigitMask = (~0UL) >> (sizeof(ulong) * BitsPerByte - DigitBits);
        const ulong HighBitMask = 0x8000000000000000UL;
#else
        uint[] digits = new uint[0];
        const int DigitBytes = sizeof(uint);
        internal const int DigitBits = 30;
        const uint DigitMask = (~0U) >> (sizeof(uint) * BitsPerByte - DigitBits);
        const uint HighBitMask = 0x80000000u;
#endif

        internal static int IntDivCeiling(int num, int denom)
        {
            return (num + denom - 1) / denom;
        }

        internal static int RoundUpToNearestMultiple(int num, int multiplier)
        {
            return IntDivCeiling(num, multiplier) * multiplier;
        }

        // Private constructor for directly filling in the fields
        // of a new object.
        private MpInt(bool negative,
#if CSMAL_64_BIT_DIGITS
                      ulong[] digits
#else
                      uint[] digits
#endif
                    )
        {
            this.negative = negative;
            this.digits = digits;
        }


        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="copyFrom">The original MpInt</param>
        public MpInt(MpInt copyFrom)
        {
            this.negative = copyFrom.negative;
#if CSMAL_64_BIT_DIGITS
            digits = new ulong[copyFrom.digits.Length];
#else
            digits = new uint[copyFrom.digits.Length];
#endif
            Array.Copy(copyFrom.digits, digits, digits.Length);
        }


        /// <summary>
        /// MpInt Zero
        /// </summary>
        public static MpInt Zero
        {
            get
            {
                return new MpInt(0);
            }
        }


        /// <summary>
        /// Create a random MInt
        /// </summary>
        /// <param name="r">Random number generator</param>
        /// <param name="bits">The MInt bits count</param>
        /// <returns>A random MInt</returns>
        public static MpInt RandomBits(Random r, int bits)
        {
            byte[] buffer = new byte[IntDivCeiling(bits, DigitBits) * DigitBytes];
            r.NextBytes(buffer);
#if CSMAL_64_BIT_DIGITS
            ulong[] digits = new ulong[buffer.Length / DigitBytes];
#else
            uint[] digits = new uint[buffer.Length / DigitBytes];
#endif
            for (int i = 0; i < digits.Length; i++)
            {
                for (int byteNum=0; byteNum < DigitBytes; byteNum++)
                {
                    digits[i] = (digits[i] << 8) | buffer[DigitBytes*i + byteNum];
                }
                digits[i] &= DigitMask;
            }

            if (bits % DigitBits > 0)
            {
                digits[digits.Length - 1] >>= DigitBits - (bits % DigitBits);
            }
            
            return new MpInt(false/*negative*/, digits);
        }


        /// <summary>
        /// Create a random MpInt which is not larger than upperBound
        /// </summary>
        /// <param name="r">The random generator</param>
        /// <param name="upperBound">The MpInt upper bound</param>
        /// <returns>A random MpInt which is not larger than upperBound</returns>
        public static MpInt Random(Random r, MpInt upperBound)
        {
            int upperBoundNumBits = upperBound.NumBits;
            int lastDigit = upperBound.GetNonzeroLength() - 1;
            MpInt result;
            do
            {
                result = RandomBits(r, upperBoundNumBits);
                if (unchecked(upperBound.digits[lastDigit] != 0))
                {
                    result.digits[result.digits.Length - 1] %= upperBound.digits[lastDigit] + 1;
                }
            }
            while (Compare(result, upperBound) >= 0);
            return result;
        }


        /// <summary>
        /// Make sure the MpInt bits count equals bits, if not padding zero bit
        /// at the end.
        /// </summary>
        /// <param name="bits">The bits count</param>
        public void ReserveBits(int bits)
        {
            if (digits.Length*DigitBits >= bits)
            {
                // Already sufficient bits reserved
                return;
            }
#if CSMAL_64_BIT_DIGITS
            ulong[] oldDigits;
#else
            uint[] oldDigits;
#endif

            oldDigits = digits;
            AllocateBitsErase(bits);
            Array.Copy(oldDigits, 0, digits, 0, oldDigits.Length);
            // Leave remaining entries set to default value of zero
        }

        internal static int FindFirstOne(ulong digit)
        {
            int result = 0;
            if ((digit & 0xFFFFFFFF00000000UL) != 0)
            {
                result |= 0x20;
                digit >>= 32;
            }
            if ((digit & 0xFFFF0000) != 0)
            {
                result |= 0x10;
                digit >>= 16;
            }
            if ((digit & 0xFF00) != 0)
            {
                result |= 0x08;
                digit >>= 8;
            }
            if ((digit & 0xF0) != 0)
            {
                result |= 0x04;
                digit >>= 4;
            }
            if ((digit & 0xC) != 0)
            {
                result |= 0x02;
                digit >>= 2;
            }
            if ((digit & 0x2) != 0)
            {
                result |= 0x01;
            }
            return result;
        }


        internal static int FindFirstOne(uint digit)
        {
            int result = 0;
            if ((digit & 0xFFFF0000) != 0)
            {
                result |= 0x10;
                digit >>= 16;
            }
            if ((digit & 0xFF00) != 0)
            {
                result |= 0x08;
                digit >>= 8;
            }
            if ((digit & 0xF0) != 0)
            {
                result |= 0x04;
                digit >>= 4;
            }
            if ((digit & 0xC) != 0)
            {
                result |= 0x02;
                digit >>= 2;
            }
            if ((digit & 0x2) != 0)
            {
                result |= 0x01;
            }
            return result;
        }


        /// <summary>
        /// The bits number of the current MpInt
        /// </summary>
        public int NumBits
        {
            get
            {
                int i;
                for (i = digits.Length - 1; i >= 0; i--)
                {
                    if (digits[i] != 0)
                    {
                        int result = DigitBits * i;
                        result += FindFirstOne(digits[i]) + 1;
                        return result;
                    }
                }
                return i + 1;
            }
        }

        private void AllocateBitsErase(int bits)
        {
#if CSMAL_64_BIT_DIGITS
            digits = new ulong[IntDivCeiling(bits, DigitBits)];
#else
            digits = new uint[IntDivCeiling(bits, DigitBits)];
#endif
        }

        private void ReserveDigits(int numDigits)
        {
            if (digits.Length >= numDigits)
            {
                // Already sufficient digits reserved
                return;
            }
#if CSMAL_64_BIT_DIGITS
            ulong[] oldDigits = digits;
            digits = new ulong[numDigits];
#else
            uint[] oldDigits = digits;
            digits = new uint[numDigits];
#endif

            Array.Copy(oldDigits, 0, digits, 0, oldDigits.Length);
            // Leave remaining entries set to default value of zero
        }

        internal int GetNonzeroLength()
        {
            int i;
            for (i = digits.Length - 1; i >= 0; i--)
            {
                if (digits[i] != 0)
                {
                    break;
                }
            }
            return i + 1;
        }


        /// <summary>
        /// Get the bit value at position of bitNum
        /// </summary>
        /// <param name="bitNum">The position of the bit</param>
        /// <returns>true indicates the bit is 1, false indicates the bit is 0</returns>
        public bool GetBit(int bitNum)
        {
            if (bitNum / DigitBits >= digits.Length)
            {
                return false;
            }
            else
            {
                return ((digits[bitNum / DigitBits] >> (bitNum % DigitBits)) & 1) != 0;
            }
        }


        /// <summary>
        /// Set the bit value at the position of bitNum
        /// </summary>
        /// <param name="bitNum">The position of the bit</param>
        /// <param name="bitValue">true indicates the bit will be set to 1, othersize 0</param>
        public void SetBit(int bitNum, bool bitValue)
        {
#if CSMAL_64_BIT_DIGITS
            ulong mask = 1UL << (bitNum % DigitBits);
#else
            uint mask = 1U << (bitNum % DigitBits);
#endif
            if (bitValue)
            {
                digits[bitNum / DigitBits] |= mask;
            }
            else
            {
                digits[bitNum / DigitBits] &= ~mask;
            }
        }

        /// <summary>
        /// Trims any leading zeros in the representation to save memory.
        /// Useful to call if you won't be using the number for a while.
        /// </summary>
        public void Trim()
        {
            int nonzeroLength = GetNonzeroLength();
            if (nonzeroLength >= digits.Length)
            {
                // No leading zeros to trim
                return;
            }

#if CSMAL_64_BIT_DIGITS
            ulong[] oldDigits;
#else
            uint[] oldDigits;
#endif

            oldDigits = digits;
            AllocateBitsErase(nonzeroLength * DigitBits);
            Array.Copy(oldDigits, 0, digits, 0, digits.Length);
        }


        /// <summary>
        /// Caculate how many zeros is there at the end of the current MpInt
        /// </summary>
        /// <returns>the zero count</returns>
        public int CountTrailingZeros()
        {
            if (IsZero())
            {
                return 0; // Special case - define as zero if it's zero
            }

            int result;
            int i = 0;
            while (digits[i] == 0)
            {
                i++;
            }

            result = i * DigitBits;
#if CSMAL_64_BIT_DIGITS
            ulong digit = digits[i];
#else
            uint digit = digits[i];
#endif
            while ((digit & 1) == 0)
            {
                result++;
                digit >>= 1;
            }
            return result;
        }


        /// <summary>
        /// Caculate how many ones is there at the end of the current MpInt
        /// </summary>
        /// <returns>the one count</returns>
        public int CountTrailingOnes()
        {
            if (IsZero())
            {
                return 0; // Special case - define as zero if it's zero
            }

            int result;
            int i = 0;
            while (i < digits.Length && unchecked(digits[i] + 1) == 0)
            {
                i++;
            }

            result = i * DigitBits;
            if (i < digits.Length)
            {
#if CSMAL_64_BIT_DIGITS
                ulong digit = digits[i];
#else
                uint digit = digits[i];
#endif
                while ((digit & 1) != 0)
                {
                    result++;
                }
            }
            return result;
        }


        /// <summary>
        /// Assign one MpInt to the current MpInt, it will lose precision if
        /// the one assigned from is larger than the one assigned to.
        /// </summary>
        /// <param name="from">The MpInt which is assigned from</param>
        /// <returns>The current MpInt</returns>
        public MpInt AssignFrom(MpInt from)
        {
            if (Object.ReferenceEquals(from, this))
            {
                return this;
            }

            negative = from.negative;
            ReserveBits(from.NumBits);
            if (this.digits.Length > from.digits.Length)
            {
                Array.Copy(from.digits, this.digits, from.digits.Length);
                Array.Clear(this.digits, from.digits.Length,
                            this.digits.Length - from.digits.Length);
            }
            else
            {
                Array.Copy(from.digits, this.digits, this.digits.Length);
            }
            return this;
        }

        #region Object members

        /// <summary>
        /// Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current System.Object.</param>
        /// <returns>true if the specified System.Object is equal to the current System.Object;
        /// otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return this.CompareTo(obj) == 0;
        }


        /// <summary>
        /// Get the hash code of this object
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return ToHexString().GetHashCode();
        }

        #endregion

        #region IComparable members

        /// <summary>
        /// Compare the current object to the specified object.
        /// </summary>
        /// <param name="obj">The object to be compared</param>
        /// <returns>-1 if left less than right, 0 if equal and 1 if left larger
        /// than right</returns>
        public int CompareTo(object obj)
        {
            return Compare(this, (MpInt)obj);
        }

        #endregion

        #region Comparison and sign checking

        /// <summary>
        /// Compare left with right to see if the left is larger than right
        /// </summary>
        /// <param name="left">The one to be compared</param>
        /// <param name="right">The one to be compared</param>
        /// <returns>-1 if left less than right, 0 if equal and 1 if left larger
        /// than right</returns>
        public static int Compare(MpInt left, MpInt right)
        {
            // The IsZero conditions deal with positive and negative zero
            if (left.negative && !right.negative && !left.IsZero())
            {
                return -1;
            }
            else if (!left.negative && right.negative && !right.IsZero())
            {
                return 1;
            }
            else if (left.negative)
            {
                return -CompareMagnitude(left, right);
            }
            else
            {
                return CompareMagnitude(left, right);
            }
        }


        /// <summary>
        /// Test if the current MpInt is zero
        /// </summary>
        /// <returns>true if this is zero, otherwise return false</returns>
        public bool IsZero()
        {
            return GetNonzeroLength() == 0;
        }


        /// <summary>
        /// Test if the current MpInt is a negative value
        /// </summary>
        /// <returns>True if it is negative, otherwise false</returns>
        public bool IsNegative()
        {
            return negative && !IsZero();
        }


        /// <summary>
        /// Test if the current MpInt is a positive value
        /// </summary>
        /// <returns>True if it is positive, otherwise false</returns>
        public bool IsPositive()
        {
            return !negative && !IsZero();
        }


        /// <summary>
        /// Test if the current MpInt is a positive value or zero
        /// </summary>
        /// <returns>True if it is positive or zero, otherwise false</returns>
        public bool IsNonNegative()
        {
            return !negative || IsZero();
        }


        /// <summary>
        /// Test if the current MpInt is a negative value or zero
        /// </summary>
        /// <returns>True if it is negative or zero, otherwise false</returns>
        public bool IsNonPositive()
        {
            return negative || IsZero();
        }


        /// <summary>
        /// Compare the absolute value of the two MpInt value
        /// </summary>
        /// <param name="left">The MpInt value to be compared</param>
        /// <param name="right">The MpInt value to be compared</param>
        /// <returns>return 1 if the absolute value of left is larger than the
        /// absolute value of right, 0 if equal and -1 if less</returns>
        public static int CompareMagnitude(MpInt left, MpInt right)
        {
            int minDigits = Math.Min(left.digits.Length, right.digits.Length);
            int i = Math.Max(left.digits.Length, right.digits.Length) - 1;

            for (; i >= minDigits; i--)
            {
                if (i < right.digits.Length && 0 < right.digits[i])
                {
                    return -1;
                }
                if (i < left.digits.Length && left.digits[i] > 0)
                {
                    return 1;
                }
            }

            for ( ; i >= 0; i--)
            {
                if (left.digits[i] < right.digits[i])
                {
                    return -1;
                }
                else if (left.digits[i] > right.digits[i])
                {
                    return 1;
                }
            }

            return 0;
        }

        #endregion
    }
}

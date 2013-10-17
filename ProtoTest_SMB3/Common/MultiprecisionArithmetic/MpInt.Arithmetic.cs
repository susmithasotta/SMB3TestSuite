///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  This source file is originally from the C# Multiprecision Arithmetic Library.
//  See http://codebox/csmal for details and updates.
//

// Summary: The part of the MpInt class defining basic arithmetic operations
// on MpInts (addition, subtraction, multiplication, division, shifts).
// Many are of the ...InPlace variety which overwrites "this" with the
// result, avoiding allocation. Some programs may prefer to access these
// methods through the overloaded operators defined in MpInt.Operators.cs.
//
// Some methods that are not in place optionally accept a "result"
// parameter.  If supplied, the result will be written into it,
// without performing allocation if there is room. The result object
// is also returned.

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
        /// <summary>
        /// Add current MpInt with another one, and put the result 
        /// to the current MpInt
        /// </summary>
        /// <param name="mpi">A MpInt</param>
        public void AddInPlace(MpInt mpi)
        {
            if (negative != mpi.negative)
            {
                if (CompareMagnitude(this, mpi) >= 0)
                {
                    SubtractInPlaceMagnitude(mpi);
                }
                else
                {
                    NegateInPlace();
                    SubtractFromInPlaceMagnitude(mpi);
                }
                return;
            }

            AddInPlaceMagnitude(mpi);
        }

        /// <summary>
        /// Nagative the current MpInt
        /// </summary>
        public void NegateInPlace()
        {
            negative = !negative;
        }


        /// <summary>
        /// Subtract current MpInt with another one, and put the result 
        /// to the current MpInt
        /// </summary>
        /// <param name="mpi">A MpInt number</param>
        public void SubtractInPlace(MpInt mpi)
        {
            if (negative != mpi.negative)
            {
                AddInPlaceMagnitude(mpi);
            }
            else if (CompareMagnitude(this, mpi) >= 0)
            {
                SubtractInPlaceMagnitude(mpi);
            }
            else
            {
                NegateInPlace();
                SubtractFromInPlaceMagnitude(mpi);
            }
        }


        /// <summary>
        /// Substract the current MpInt value from mpi and set the result to
        /// the current MpInt
        /// </summary>
        /// <param name="mpi">The MpInt value to be substracted from</param>
        public void SubtractFromInPlace(MpInt mpi)
        {
            if (negative != mpi.negative)
            {
                AddInPlaceMagnitude(mpi);
            }
            else if (CompareMagnitude(this, mpi) <= 0)
            {
                SubtractFromInPlaceMagnitude(mpi);
            }
            else
            {
                NegateInPlace();
                SubtractInPlaceMagnitude(mpi);
            }
        }

        private void AddInPlaceMagnitude(MpInt mpi)
        {
#if CSMAL_AT_LEAST_32_BIT_DIGITS
            ulong sumDigit;
#else
            uint sumDigit;
#endif
            ReserveDigits(Math.Max(digits.Length, mpi.digits.Length));
            int minDigitLengths = Math.Min(digits.Length, mpi.digits.Length);

            uint carry = 0;
            int i = 0;
            for (; i < minDigitLengths; i++)
            {
#if CSMAL_AT_LEAST_32_BIT_DIGITS
                sumDigit = (ulong)digits[i] + mpi.digits[i] + carry;
#else
                sumDigit = digits[i] + mpi.digits[i] + carry;
#endif
                carry = (uint)(sumDigit >> DigitBits);
#if CSMAL_64_BIT_DIGITS
                digits[i] = sumDigit & DigitMask;
#else
                digits[i] = unchecked((uint)sumDigit) & DigitMask;
#endif
            }
            for (; carry > 0 && i < digits.Length; i++)
            {
#if CSMAL_AT_LEAST_32_BIT_DIGITS
                sumDigit = (ulong)digits[i] + carry;
#else
                sumDigit = digits[i] + carry;
#endif
                carry = (uint)(sumDigit >> DigitBits);
#if CSMAL_64_BIT_DIGITS
                digits[i] = sumDigit & DigitMask;
#else
                digits[i] = unchecked((uint)sumDigit) & DigitMask;
#endif
            }
            if (carry > 0)
            {
                if (i == digits.Length)
                {
                    ReserveDigits(digits.Length + 1);
                }
                digits[i] = carry;
            }
        }

        private void SubtractInPlaceMagnitude(MpInt mpi)
        {
            Debug.Assert(CompareMagnitude(this, mpi) >= 0);

#if CSMAL_AT_LEAST_32_BIT_DIGITS
            ulong diffDigit;
#else
            uint diffDigit;
#endif
            int minDigitLengths = Math.Min(digits.Length, mpi.digits.Length);

            uint borrow = 0;
            int i = 0;
            for (; i < minDigitLengths; i++)
            {
#if CSMAL_AT_LEAST_32_BIT_DIGITS
                diffDigit = unchecked((ulong)digits[i] - borrow - mpi.digits[i]);
#else
                diffDigit = unchecked(digits[i] - borrow - mpi.digits[i]);
#endif
                borrow = (uint)((diffDigit >> DigitBits) & 1);
#if CSMAL_64_BIT_DIGITS
                digits[i] = diffDigit & DigitMask;
#else
                digits[i] = unchecked((uint)diffDigit) & DigitMask;
#endif
            }
            for (; borrow > 0 && i < digits.Length; i++)
            {
#if CSMAL_AT_LEAST_32_BIT_DIGITS
                diffDigit = unchecked((ulong)digits[i] - borrow);
#else
                diffDigit = unchecked(digits[i] - borrow);
#endif
                borrow = (uint)((diffDigit >> DigitBits) & 1);
#if CSMAL_64_BIT_DIGITS
                digits[i] = diffDigit & DigitMask;
#else
                digits[i] = unchecked((uint)diffDigit) & DigitMask;
#endif
            }

#if DEBUG
            // The following asserts should hold provided the magnitude of
            // "this" is at least the magnitude of mpi.
            for (; i < mpi.digits.Length; i++)
            {
                Debug.Assert(mpi.digits[i] == 0);
            }
            Debug.Assert(borrow == 0);
#endif
        }

        private void SubtractFromInPlaceMagnitude(MpInt mpi)
        {
            Debug.Assert(CompareMagnitude(this, mpi) <= 0);

#if CSMAL_AT_LEAST_32_BIT_DIGITS
            ulong diffDigit;
#else
            uint diffDigit;
#endif
            ReserveDigits(mpi.digits.Length);
            int minDigitLengths = mpi.digits.Length;

            uint borrow = 0;
            int i = 0;
            for (; i < minDigitLengths; i++)
            {
#if CSMAL_AT_LEAST_32_BIT_DIGITS
                diffDigit = unchecked((ulong)mpi.digits[i] - borrow - digits[i]);
#else
                diffDigit = unchecked(mpi.digits[i] - borrow - digits[i]);
#endif
                borrow = (uint)((diffDigit >> DigitBits) & 1);
#if CSMAL_64_BIT_DIGITS
                digits[i] = diffDigit & DigitMask;
#else
                digits[i] = unchecked((uint)diffDigit) & DigitMask;
#endif
            }

#if DEBUG
            // The following asserts should hold provided the magnitude of
            // mpi is at least the magnitude of "this".
            for (; i < digits.Length; i++)
            {
                Debug.Assert(digits[i] == 0);
            }
            Debug.Assert(borrow == 0);
#endif
        }


        /// <summary>
        /// Multipy the current MpInt value with the multiplier, and set the result to
        /// the current MpInt value
        /// </summary>
        /// <param name="multiplier">The int multiplier</param>
        public void MultiplyInPlace(int multiplier)
        {
            if (multiplier < 0)
            {
                negative = !negative;
                MultiplyInPlace((uint)(-multiplier));
            }
            else
            {
                MultiplyInPlace((uint)multiplier);
            }
        }


        /// <summary>
        /// Multipy the current MpInt value with the multiplier, and set the result to
        /// the current MpInt value
        /// </summary>
        /// <param name="multiplier">The uint multiplier</param>
        public void MultiplyInPlace(uint multiplier)
        {
            uint carry = 0;
            int i;

#if CSMAL_64_BIT_DIGITS
            for (i = 0; i < digits.Length; i++)
            {
                //
                // Two stage multiply on halves of digits[i].
                //

                ulong mask = (1UL << 32) - 1UL;
                ulong product = (digits[i] & mask) * multiplier + carry;
                ulong newDigitLowWord = product & mask;
                carry = (uint)(product >> 32);

                product = (digits[i] >> 32) * multiplier + carry;
                digits[i] = ((product << 32) | newDigitLowWord) & DigitMask;
                carry = (uint)(product >> (DigitBits - 32));
            }
#else
            for (i = 0; i < digits.Length; i++)
            {
                // The maximum carry is 0xFFFFFFFE, and 
                // 0xFFFFFFFF*0xFFFFFFFF + 0xFFFFFFFE = 0xFFFFFFFEFFFFFFFF.
                ulong product = (ulong)digits[i] * multiplier + carry;
                digits[i] = unchecked((uint)product) & DigitMask;
                carry = (uint)(product >> DigitBits);
            }
#endif

            if (carry > 0)
            {
                ReserveDigits(digits.Length + 1);
                digits[i] = carry;
            }
        }

        /// <summary>
        /// Multiply the two multiprecision integers <code>this</code> and
        /// right, allocating and returning the result.
        /// </summary>
        /// <param name="right">The right side value of Multiply operation</param>
        /// <returns>The multipied value</returns>
        public MpInt Multiply(MpInt right)
        {
            return Multiply(right, MpInt.Zero);
        }

        /// <summary>
        /// Multiply the two multiprecision integers <code>this</code> and
        /// right and place the result in (and return) destination.
        /// </summary>
        /// <remarks>
        /// This method just does sign handling, delegating to
        /// specific algorithm methods for the multiplying based
        /// on the operand size.
        /// </remarks>
        /// <param name="right">The right side value of multipy operation</param>
        /// <param name="result">The result of the multipy operation</param>
        /// <returns>The result of the multipy operation</returns>
        public MpInt Multiply(MpInt right, MpInt result)
        {
            MpInt left = this;
            if (Object.ReferenceEquals(result, left) ||
                Object.ReferenceEquals(result, right))
            {
                throw new ArgumentException("Result of multiply must be different from operands (cannot multiply in-place)");
            }

            bool leftNegative = left.negative;
            if (left.negative)
            {
                left.NegateInPlace();
            }
            bool rightNegative = right.negative;
            if (right.negative)
            {
                right.NegateInPlace();
            }
            if (result.negative)
            {
                result.NegateInPlace();
            }

            try
            {
#if DONT_USE_ADVANCED_MULTIPLICATION
                MultiplyGradeSchool(left, right, result);
#else
                MpIntAdvancedMultiplication.MultiplyMagnitudes(left, right, result);
#endif
            }
            finally
            {
                if (leftNegative)
                {
                    left.NegateInPlace();
                }
                if (rightNegative)
                {
                    right.NegateInPlace();
                }
            }
            result.negative = (leftNegative != rightNegative);
            return result;
        }

        /// <remarks>
        /// This method simply uses the O(n^2) grade-school multiplication
        /// algorithm, using ulong arithmetic to get the 64-bit result of
        /// 32-bit multiplications. It also uses ideas from David M. Smith's
        /// "A Fortran Package For Floating-Point Multiple-Precision Arithmetic"
        /// (ACM Transactions on Mathematical Software, 1991), in particular:
        /// 
        /// "The speed of these [O(n^2) multiplication] operations is improved
        /// by minimizing the time spent normalizing partial results. When
        /// the base used is not too big the partial results need not be
        /// normalized after each step, since we can guarantee that integer
        /// overflow cannot happen on the next step. The smaller the base, the
        /// longer normalization may be postponed. In Brent's MP the base is
        /// restricted so that 8b^2 - 1 is representable. This allows
        /// normalization to be done only once for each eight steps. In FM
        /// larger values of b are allowed, and the program computes during
        /// the operation whether the next step can be done before
        /// normalizing. This uses the digits actually being multiplied,
        /// instead of worst-case upper bounds. For example, on a 32-bit
        /// machine if b = 10,000 and t is large then normalization is done
        /// only about once each 40 steps."
        ///
        /// Since we're using 64-bit multiplies here, we can get away
        /// with a much larger base - not quite 32 bits, but something
        /// a bit less. We then normalize at each step if the next step
        /// might overflow. For this reason, if the number of bits per
        /// digit is 32 or more, we must first unpack the representation
        /// into one with less bits per digit, trading some copying time
        /// for quicker multiplication. Benchmarks show that 30 bits
        /// produces the best perf (times in milliseconds):
        /// 
        ///                          Operand size in bits
        /// Bits 1.00E+02       1.00E+03    1.00E+04    1.00E+05    1.00E+06
        /// 28   0.000762193    0.01119118  0.78248362  74.770090   7521.998662
        /// 29   0.000667267    0.01068365  0.73132691  70.065285   7046.99862
        /// 30   0.000736604    0.01027708  0.70257187  67.745310   6845.798644
        /// 31   0.000692915    0.01047780  0.74917837  72.098653   7277.198653
        /// 
        /// Overall this optimization gave about a 20% improvement.
        /// </remarks>
        internal static void MultiplyGradeSchool(MpInt left, MpInt right, MpInt result)
        {
#if CSMAL_AT_LEAST_32_BIT_DIGITS
            const int unpackedBaseBits = 30;
            const uint maxUnpackedDigit = (1u << unpackedBaseBits) - 1;
            MpInt leftUnpacked = left.Unpack(unpackedBaseBits);
            MpInt rightUnpacked = right.Unpack(unpackedBaseBits);
#else
            const int unpackedBaseBits = DigitBits;
            const uint maxUnpackedDigit = DigitMask;
            MpInt leftUnpacked = left;
            MpInt rightUnpacked = right;
#endif

            // Worst-case size occurs when multiplying all 1 bits,
            // and (2^(m+1) - 1)(2^(n+1) - 1) < 2^((m+1)+(n+1)),
            // so m + n + 2 bits suffice for the result.
            result.ReserveBits(left.NumBits + right.NumBits + 2);
            int resultUnpackedDigits = IntDivCeiling(left.NumBits + right.NumBits, unpackedBaseBits);
            int leftLength = left.GetNonzeroLength();
            int rightLength = right.GetNonzeroLength();

#if CSMAL_AT_LEAST_32_BIT_DIGITS
            // Need this for PackDigitsOverZeros to work and so any
            // high bytes are correctly zero.
            Array.Clear(result.digits, 0, result.digits.Length);
#else
            Array.Clear(result.digits, resultUnpackedDigits, result.digits.Length - resultUnpackedDigits);
#endif

            ulong carry = 0;
            int resultIdx;
            for (resultIdx = 0; resultIdx < resultUnpackedDigits; resultIdx++) {
                int i, j;
                int jBound = Math.Min(resultIdx + 1, rightLength);
                ulong sum = carry & maxUnpackedDigit;
                carry >>= unpackedBaseBits;
                for (i = Math.Min(leftLength - 1, resultIdx), j = resultIdx - i;
                     j < jBound;
                     i--, j++)
                {
                    if (sum >= ulong.MaxValue - (ulong)maxUnpackedDigit * maxUnpackedDigit) {
                        carry += sum >> unpackedBaseBits;
                        sum &= maxUnpackedDigit;
                    }
                    sum += (ulong)leftUnpacked.digits[i] * (ulong)rightUnpacked.digits[j];
                }
                carry += sum >> unpackedBaseBits;
                sum &= maxUnpackedDigit;
#if CSMAL_AT_LEAST_32_BIT_DIGITS
                result.PackDigitOverZeros(resultIdx, unpackedBaseBits, unchecked((uint)sum));
#else
                result.digits[resultIdx] = unchecked((uint)sum);
#endif
            }

            // If there's carry remaining, just tack it on the end of the result.
            // By the same argument as above, it can be at most 2 bits at this point.
            if (carry != 0) {
#if CSMAL_AT_LEAST_32_BIT_DIGITS
                result.PackDigitOverZeros(resultIdx, unpackedBaseBits,
                                          unchecked((uint)carry));
#else
                result.digits[resultIdx] = unchecked((uint)carry);
#endif
            }
        }

        /// <summary>
        /// The simple grade-school algorithm, taken from MpInt and optimized
        /// for more efficient squaring. This is particularly useful for
        /// taking large powers in cryptography.
        /// </summary>
        /// <remarks>
        /// The main work-saving trick we use here is that every column is
        /// symmetric about its center, so it just has to be added up to
        /// that point and then doubled, e.g.:
        /// 
        ///        A  B  C
        ///      x A  B  C
        ///      ---------
        ///       AC BC CC
        ///    AB BB BC
        /// AA AB AC
        /// 
        /// </remarks>
        internal static void SquareGradeSchool(MpInt mpi, MpInt result)
        {
#if CSMAL_AT_LEAST_32_BIT_DIGITS
            const int unpackedBaseBits = 30;
            const uint maxUnpackedDigit = (1u << unpackedBaseBits) - 1;
            MpInt mpiUnpacked = mpi.Unpack(unpackedBaseBits);
#else
            const int unpackedBaseBits = DigitBits;
            const uint maxUnpackedDigit = DigitMask;
            MpInt mpiUnpacked = mpi;
#endif

            // Worst-case size occurs when multiplying all 1 bits,
            // and (2^(m+1) - 1)^2 < 2^(2(m+1)), so 2(m + 1) bits
            // suffice for the result.
            result.ReserveBits(2*(mpi.NumBits + 1));
            int resultUnpackedDigits = IntDivCeiling(2*mpi.NumBits, unpackedBaseBits);
            int mpiLength = mpi.GetNonzeroLength();

#if CSMAL_AT_LEAST_32_BIT_DIGITS
            // Need this for PackDigitsOverZeros to work and so any
            // high bytes are correctly zero.
            Array.Clear(result.digits, 0, result.digits.Length);
#else
            Array.Clear(result.digits, resultUnpackedDigits, result.digits.Length - resultUnpackedDigits);
#endif

            ulong carry = 0;
            int resultIdx;
            for (resultIdx = 0; resultIdx < resultUnpackedDigits; resultIdx++) {
                int i, j;
                ulong prevCarry = carry;
                ulong sum = 0;
                carry = 0;
                for (i = Math.Min(mpiUnpacked.digits.Length - 1, resultIdx), j = resultIdx - i;
                     j < i;
                     i--, j++) {
                    if (sum >= ulong.MaxValue - (ulong)maxUnpackedDigit * maxUnpackedDigit) {
                        carry += sum >> unpackedBaseBits;
                        sum &= maxUnpackedDigit;
                    }
                    sum += (ulong)mpiUnpacked.digits[i] * (ulong)mpiUnpacked.digits[j];
                }

                if (sum >= (ulong.MaxValue >> 1)) {
                    carry += sum >> unpackedBaseBits;
                    sum &= maxUnpackedDigit;
                }
                sum <<= 1;
                carry <<= 1;

                carry += prevCarry >> unpackedBaseBits;
                carry += sum >> unpackedBaseBits;
                sum &= maxUnpackedDigit;
                sum += prevCarry & maxUnpackedDigit;
                if (i == j)
                {
                    // There's a center digit to add. Overflow can (just barely) not
                    // happen since 0xFFFFFFFF^2 + 2*0xFFFFFFFF == 0xFFFFFFFFFFFFFFFF.
                    sum += (ulong)mpiUnpacked.digits[i] * (ulong)mpiUnpacked.digits[i];
                }

                carry += sum >> unpackedBaseBits;
                sum &= maxUnpackedDigit;
#if CSMAL_AT_LEAST_32_BIT_DIGITS
                result.PackDigitOverZeros(resultIdx, unpackedBaseBits, unchecked((uint)sum));
#else
                result.digits[resultIdx] = unchecked((uint)sum);
#endif
            }

            // If there's carry remaining, just tack it on the end of the result.
            // By the same argument as above, it can be at most 2 bits at this point.
            if (carry != 0) {
#if CSMAL_AT_LEAST_32_BIT_DIGITS
                result.PackDigitOverZeros(resultIdx, unpackedBaseBits,
                                          unchecked((uint)carry));
#else
                result.digits[resultIdx] = unchecked((uint)carry);
#endif
            }
        }

        /// <summary>
        /// Forms a new integer whose digits are an inclusive subrange
        /// of the digits of <code>this</code>.
        /// </summary>
        internal MpInt DigitSubrange(int startDigit, int endDigit)
        {
            if (startDigit <= endDigit && startDigit < this.digits.Length)
            {
#if CSMAL_64_BIT_DIGITS
                ulong[] digitsRange = new ulong[endDigit - startDigit + 1];
#else
                uint[] digitsRange = new uint[endDigit - startDigit + 1];
#endif
                Array.Copy(this.digits, startDigit, digitsRange, 0,
                           Math.Min(endDigit - startDigit + 1, this.digits.Length - startDigit));
                return new MpInt(false/*negative*/, digitsRange);
            }
            else
            {
                return MpInt.Zero;
            }
        }

        /// <summary>
        /// Forms a new integer whose bits are an inclusive subrange
        /// of the bits of <code>this</code>.
        /// </summary>
        internal MpInt BitSubrange(int startBit, int endBit)
        {
            if (startBit <= endBit && startBit < digits.Length*DigitBits)
            {
#if CSMAL_64_BIT_DIGITS
                ulong[] resultDigits = new ulong[IntDivCeiling(endBit - startBit + 1, DigitBits)];
                ulong highDigit;
#else
                uint[] resultDigits = new uint[IntDivCeiling(endBit - startBit + 1, DigitBits)];
                uint highDigit;
#endif
                int shift = startBit % DigitBits;
                int startDigit = startBit / DigitBits;
                int lastDigitBits = (endBit - startBit + 1) % DigitBits;
                if (lastDigitBits == 0)
                {
                    lastDigitBits = DigitBits;
                }

                if (shift > 0)
                {
                    for (int i=0; i < resultDigits.Length && startDigit + i < digits.Length; i++)
                    {
                        highDigit = (i + startDigit + 1 >= digits.Length) ? 0 : digits[startDigit + i + 1];
                        resultDigits[i] = ((highDigit << (DigitBits - shift)) |
                                           (digits[startDigit + i] >> shift)) &
                                           DigitMask;
                    }
                }
                else
                {
                    Array.Copy(this.digits, startDigit, resultDigits, 0,
                               Math.Min(resultDigits.Length, this.digits.Length - startDigit));
                }

#if CSMAL_64_BIT_DIGITS
                resultDigits[resultDigits.Length - 1] &= (1UL << lastDigitBits) - 1UL;
#else
                resultDigits[resultDigits.Length - 1] &= (1U << lastDigitBits) - 1U;
#endif

                return new MpInt(false/*negative*/, resultDigits);
            }
            else
            {
                return MpInt.Zero;
            }
        }


        // Set a digit in the unpacked representation of this number,
        // packing it into its packed representation, assuming that
        // the affected bits are currently all zero. For example, if
        // baseBits is 29 and DigitBits is 32, setting digit 3 would
        // modify indexes 2 and 3 of this.digits.
        private void PackDigitOverZeros(int idx, int baseBits, uint value)
        {
            if (baseBits == DigitBits)
            {
                digits[idx] = value;
                return;
            }
            Debug.Assert(baseBits < DigitBits,
                         "Unpacked representation digits must have smaller number of bits");
            int lowBit = idx * baseBits;
            int highBit = lowBit + baseBits;
            int lowDigit = lowBit / DigitBits;
            int highDigit = highBit / DigitBits;

#if CSMAL_64_BIT_DIGITS
            digits[lowDigit] |= ((ulong)value << (lowBit % DigitBits)) & DigitMask;
#else
            digits[lowDigit] |= (value << (lowBit % DigitBits)) & DigitMask;
#endif
            uint highDigitValue = value >> (baseBits - (highBit % DigitBits));
            if (lowDigit != highDigit)
            {
                if (highDigit < digits.Length)
                {
                    digits[highDigit] |= highDigitValue;
                }
                else
                {
                    Debug.Assert(highDigitValue == 0);
                }
            }
        }


        /// <summary>
        /// Divide the current MpInt value by divisor
        /// </summary>
        /// <param name="divisor">The divisor</param>
        /// <param name="remainder">The remainer</param>
        public void DivideInPlace(int divisor, out int remainder)
        {
            uint uremainder;
            if (divisor < 0)
            {
                negative = !negative;
                DivideInPlace((uint)(-divisor), out uremainder);
                remainder = (int)uremainder;
            }
            else
            {
                DivideInPlace((uint)divisor, out uremainder);
                remainder = (int)uremainder;
            }
        }


        /// <summary>
        /// divide the current MpInt value by the divisor
        /// </summary>
        /// <param name="divisor">The divisor</param>
        /// <param name="remainder">The remainder</param>
        public void DivideInPlace(uint divisor, out uint remainder)
        {
            if (divisor == 0)
            {
                throw new DivideByZeroException();
            }

            uint rem = 0; // Always less than divisor, so uint
            int i;

#if CSMAL_64_BIT_DIGITS
            for (i = digits.Length - 1; i >= 0; i--)
            {
                //
                // Two stage divide - divide digits[i] into two parts
                //

                ulong mask = ((1UL << 32) - 1UL);
                ulong dividend1 = ((ulong)rem << (DigitBits-32)) | (digits[i] >> 32);
                rem = (uint)(dividend1 % divisor);

                ulong dividend2 = ((ulong)rem << 32) | (digits[i] & mask);
                digits[i] = ((dividend1 / divisor) << 32) | (dividend2 / divisor);
                rem = (uint)(dividend2 % divisor);
            }
#else
            for (i = digits.Length - 1; i >= 0; i--)
            {
                ulong dividend = ((ulong)rem << DigitBits) | (ulong)digits[i];
                digits[i] = (uint)(dividend / divisor);
                rem = (uint)(dividend % divisor);
            }
#endif
            remainder = rem;
        }

        /// <summary>
        /// Divides this by divisor, overwriting it with the quotient
        /// and allocating and yielding the remainder.
        /// </summary>
        public void DivideInPlace(MpInt divisor, out MpInt remainder)
        {
            remainder = MpInt.Zero;
            DivideInPlace(divisor, remainder);
        }

        /// <summary>
        /// Divides this by divisor, overwriting it with the quotient
        /// and yielding the remainder. Remainder should not be the same
        /// object as <code>this</code> or divisor.
        /// </summary>
        public void DivideInPlace(MpInt divisor, MpInt remainder)
        {
            if (divisor.IsZero())
            {
                throw new DivideByZeroException();
            }

            MpInt dividend = this;

            if (Object.ReferenceEquals(remainder, dividend) ||
                Object.ReferenceEquals(remainder, divisor))
            {
                throw new ArgumentException("Remainder cannot be stored over divisor or remainder");
            }
            
            if (Object.ReferenceEquals(dividend, divisor))
            {
                // Aliasing causes difficulties - and we already know the answer
                AllocateBitsErase(1);
                digits[0] = 1;
                negative = false;
                Array.Clear(remainder.digits, 0, remainder.digits.Length);
                return;
            }

#if DONT_USE_ADVANCED_MULTIPLICATION
            DivideGradeSchool(dividend, divisor, remainder);
#else
            MpIntAdvancedMultiplication.DivideMagnitudesInPlace(dividend, divisor, remainder);
#endif

            if (divisor.negative)
            {
                dividend.NegateInPlace();
            }
        }

        /// <summary>
        /// Implements the grade-school left-to-right division algorithm.
        /// </summary>
        /// <remarks>
        /// The implementation works by moving a digit at a time from the
        /// dividend to the remainder, then estimating the remainder divided
        /// by the divisor (the next quotient digit) using double-precision
        /// floating-point, and subtracting out the estimate times the
        /// divisor, correcting it if it's slightly too big.
        /// </remarks>
        internal static void DivideGradeSchool(MpInt dividend, MpInt divisor, MpInt remainder)
        {
            int divisorLength = divisor.GetNonzeroLength();
            double divisorDouble = divisor.GetScaledDouble(divisorLength - 1);

            MpInt divisorMultiple = MpInt.Zero;
            divisorMultiple.ReserveDigits(divisor.digits.Length + 1);

            // Start out by copying just enough digits into rem to ensure rem >= divisor
            int dividendNonZeroLength = dividend.GetNonzeroLength();
            int divisorBits = divisor.NumBits;
            int initialRemainderDigits =
                Math.Min(IntDivCeiling(divisorBits, DigitBits), dividendNonZeroLength);
            remainder.ReserveDigits(initialRemainderDigits);
            Array.Copy(dividend.digits, dividendNonZeroLength - initialRemainderDigits,
                       remainder.digits, 0, initialRemainderDigits);
            Array.Clear(remainder.digits, initialRemainderDigits,
                        remainder.digits.Length - initialRemainderDigits);

            int initialDigitNum = dividendNonZeroLength - initialRemainderDigits;
            Array.Clear(dividend.digits, initialDigitNum + 1,
                        dividend.digits.Length - (initialDigitNum + 1));

            // Now do the main division
            for (int digitNum = initialDigitNum;
                 digitNum >= 0; digitNum--) {
                if (digitNum < initialDigitNum) {
                    remainder.ShiftLeftInPlace(DigitBits);
                    remainder.digits[0] = dividend.digits[digitNum];
                }

                // Estimate next quotient digit
                double remainderDouble = remainder.GetScaledDouble(divisorLength - 1);
                uint quotientDigitEstimate = (uint)(remainderDouble / divisorDouble);
                dividend.digits[digitNum] = quotientDigitEstimate;
                if (quotientDigitEstimate == 0) {
                    continue;
                }

                // Subtract divisor * quotientDigitEstimate from remainder,
                // correcting it if necessary to ensure it's < remainder.
                divisorMultiple.AssignFrom(divisor);
                divisorMultiple.MultiplyInPlace(quotientDigitEstimate);
                if (MpInt.CompareMagnitude(remainder, divisorMultiple) < 0) {
                    quotientDigitEstimate--;
                    dividend.digits[digitNum]--;
                    divisorMultiple.SubtractInPlace(divisor);
                }
                remainder.SubtractInPlace(divisorMultiple);
            }
        }

        /// <summary>
        /// Get an approximation of this MpInt as a double, scaled
        /// by a power of 2 so that the digit at the specified index
        /// has the place value 1 (is the least significant part of
        /// the integer part).
        /// </summary>
        private double GetScaledDouble(int unitPlaceDigit)
        {
            double result = 0.0, prevResult = double.PositiveInfinity;
            int initialIndex = GetNonzeroLength() - 1;
            double placeValue = Math.Pow(2.0, DigitBits*(initialIndex - unitPlaceDigit));
            double baseMult = Math.Pow(2.0, DigitBits);
            for (int i=initialIndex; i >= 0; i--)
            {
                result += digits[i]*placeValue;
                if (prevResult == result)
                {
                    break;
                }
                prevResult = result;
                placeValue /= baseMult;
            }
            return result;
        }

        /// <remarks>This method is used by ToString() to convert an
        /// MpInt to a decimal numeral.</remarks>
        public uint Modulus(uint divisor)
        {
            //
            // This method is almost just like DivideInPlace, but does
            // not update "this".
            //

            if (divisor == 0)
            {
                throw new DivideByZeroException();
            }

            uint rem = 0;
            int i;

#if CSMAL_64_BIT_DIGITS
            for (i = digits.Length - 1; i >= 0; i--)
            {
                //
                // Two stage divide - divide digits[i] into two parts
                //

                ulong mask = ((1UL << 32) - 1UL);
                ulong dividend1 = ((ulong)rem << (DigitBits-32)) | (digits[i] >> 32);
                rem = (uint)(dividend1 % divisor);

                ulong dividend2 = ((ulong)rem << 32) | (digits[i] & mask);
                rem = (uint)(dividend2 % divisor);
            }
#else
            for (i = digits.Length - 1; i >= 0; i--)
            {
                ulong dividend = ((ulong)rem << DigitBits) | (ulong)digits[i];
                rem = (uint)(dividend % divisor);
            }
#endif

            return rem;
        }


        /// <summary>
        /// shift left the current MpInt value by the specified shiftAmount
        /// </summary>
        /// <param name="shiftAmount">The shift amount</param>
        public void ShiftLeftInPlace(int shiftAmount)
        {
            if (shiftAmount < 0)
            {
                throw new ArgumentException("shiftAmount for shifting MpInt left must be nonnegative");
            }
            if (shiftAmount == 0 )
            {
                return;
            }

            int oldMax = GetNonzeroLength() - 1;
            if (oldMax < 0)
            {
                return;
            }

            int shiftDigits = shiftAmount / DigitBits;
            int shiftPart = shiftAmount % DigitBits;
            bool needExtraDigit = shiftPart > 0 &&
                                  (digits[oldMax] >> (DigitBits - shiftPart)) != 0;
            ReserveDigits(oldMax + 1 + shiftDigits + (needExtraDigit ? 1 : 0));

#if CSMAL_64_BIT_DIGITS
            ulong lowDigit, highDigit;
#else
            uint lowDigit, highDigit;
#endif

            // We need the condition shiftPart == 0 below to deal with a C# oddity -
            // it only looks at the low five bits of the shift amount, and so
            // lowDigit >> 32 == lowDigit.
            for (int to = digits.Length - 1; to >= 0; to--)
            {
                int from = to - shiftDigits;
                highDigit = (from < 0 || from > oldMax) ? 0 : digits[from];
                lowDigit = (from - 1 < 0 || shiftPart == 0) ? 0 : digits[from - 1];
                digits[to] = ((highDigit << shiftPart) |
                              (lowDigit >> (DigitBits - shiftPart))) &
                              DigitMask;
            }
        }

        internal void ShiftLeftDigitsInPlace(int shiftDigits)
        {
            if (shiftDigits < 0)
            {
                throw new ArgumentException("shiftAmount for shifting MpInt left must be nonnegative");
            }
            if (shiftDigits == 0 )
            {
                return;
            }

            int oldMax = GetNonzeroLength() - 1;
            if (oldMax < 0)
            {
                return;
            }
            ReserveDigits(oldMax + 1 + shiftDigits);

            for (int to = digits.Length - 1; to >= 0; to--)
            {
                int from = to - shiftDigits;
                digits[to] = (from < 0 || from > oldMax) ? 0 : digits[from];
            }
        }

        /// <summary>
        /// Shifts the magnitude of an integer right by shiftAmount bits,
        /// effectively floor-dividing it by 2^shiftAmount.
        /// </summary>
        /// <remarks>If you shift right by a large amount, you may wish
        /// to consider calling Trim() afterwards to reclaim memory.</remarks>
        public void ShiftRightInPlace(int shiftAmount)
        {
            if (shiftAmount < 0)
            {
                throw new ArgumentException("shiftAmount for shifting MpInt right must be nonnegative");
            }
            if (shiftAmount == 0)
            {
                return;
            }

            int shiftDigits = shiftAmount / DigitBits;
            int shiftPart = shiftAmount % DigitBits;

#if CSMAL_64_BIT_DIGITS
            ulong lowDigit, highDigit;
#else
            uint lowDigit, highDigit;
#endif

            // We need the condition shiftPart == 0 below to deal with a C# oddity -
            // it only looks at the low five bits of the shift amount, and so
            // lowDigit >> 32 == lowDigit.
            for (int to = 0; to < digits.Length; to++)
            {
                int from = to + shiftDigits;
                highDigit = (from + 1 >= digits.Length || shiftPart == 0) ? 0 : digits[from + 1];
                lowDigit = (from >= digits.Length) ? 0 : digits[from];
                digits[to] = ((highDigit << (DigitBits - shiftPart)) |
                              (lowDigit >> shiftPart)) &
                              DigitMask;
            }
        }
    }
}

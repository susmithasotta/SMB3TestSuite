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
    /// Implements advanced multiplication algorithms with better
    /// asymptotic efficiency than the grade-school method for
    /// very large inputs.
    /// </summary>
    /// <remarks>
    /// These are separated out into their own module in case someone
    /// doesn't want to drag in all this complexity. They can simply
    /// define DONT_USE_ADVANCED_MULTIPLICATION and take out this
    /// source file to omit this support and stick with the basic
    /// algorithm.
    /// </remarks>
    internal static class MpIntAdvancedMultiplication
    {
        // These shouldn't normally be modified, but are exposed as
        // internal to enable the benchmarker to find good cutoff values.
        internal static int Toom2MinimumBits = 2500;
        // Toom-3 currently appears broken as shown by TestDivideNewtonIteration.
        // This disables it for now.
        internal static int Toom3MinimumBits = 80000;
        internal static int SchonhageStrassen6MinimumBits = 10000000;
        internal static int SchonhageStrassen7MinimumBits = 50000000;

        /// <summary>
        /// Selects a multiplication algorithm and applies it. Used for
        /// recursive calls. Entry point for this class.
        /// </summary>
        internal static void MultiplyMagnitudes(MpInt left, MpInt right, MpInt result)
        {
            // Special cases: zero times anything is zero.
            int leftBits = left.NumBits;
            int rightBits = right.NumBits;
            if (leftBits == 0 || rightBits == 0)
            {
                result.AssignFrom(MpInt.Zero);
                return;
            }

            if (Object.ReferenceEquals(left, right))
            {
                Square(left, result);
                return;
            }

            // Grade-school algorithm is best for small numbers
            if (leftBits < Toom2MinimumBits &&
                rightBits < Toom2MinimumBits)
            {
                MpInt.MultiplyGradeSchool(left, right, result);
                return;
            }

            // Make sure left is bigger
            if (leftBits < rightBits)
            {
                MpInt temp = left;
                left = right;
                right = temp;

                int tempBits = leftBits;
                leftBits = rightBits;
                rightBits = tempBits;
            }

            if (leftBits < Toom3MinimumBits)
            {
                if (rightBits < leftBits / 2)
                {
                    MultiplyToom1_5(left, right, result);
                }
                else
                {
                    MultiplyToom2(left, right, result);
                }
            }
            else if (leftBits < SchonhageStrassen6MinimumBits)
            {
                if (rightBits < leftBits * 2 / 3)
                {
                    MultiplyToom2_5(left, right, result);
                }
                else
                {
                    MultiplyToom3(left, right, result);
                }
            }
            else if (leftBits < SchonhageStrassen7MinimumBits)
            {
                MultiplySchonhageStrassenTopLevel(left, right, result, 6);
            }
            else
            {
                MultiplySchonhageStrassenTopLevel(left, right, result, 7);
            }
        }


        /// <summary>
        /// Selects a squaring algorithm and applies it.
        /// </summary>
        private static void Square(MpInt mpi, MpInt result)
        {
            // Special cases: zero times anything is zero.
            int mpiBits = mpi.NumBits;

            // Grade-school algorithm is best for small numbers
            if (mpiBits < Toom2MinimumBits)
            {
                MpInt.SquareGradeSchool(mpi, result);
            }
            else if (mpiBits  < Toom3MinimumBits)
            {
                SquareToom2(mpi, result);
            }
            else if (mpiBits < SchonhageStrassen6MinimumBits)
            {
                SquareToom3(mpi, result);
            }
            else if (mpiBits < SchonhageStrassen7MinimumBits)
            {
                MultiplySchonhageStrassenTopLevel(mpi, mpi, result, 6);
            }
            else
            {
                MultiplySchonhageStrassenTopLevel(mpi, mpi, result, 7);
            }
        }

        private static MpInt Square(MpInt mpi)
        {
            MpInt result = MpInt.Zero;
            Square(mpi, result);
            return result;
        }

        internal static int SchonhageStrassenModFermatMinimumBits = 70000;

        /// <summary>
        /// Multiplies left by right mod 2^exponent + 1, using the most
        /// efficient method for the size of the operands.
        /// </summary>
        /// <remarks>
        /// Currently resorts to the Schonhage method in all cases, but should break
        /// over to a specialized long multiplication procedure that directly computes
        /// the result by subtracting the last D digits of the result from the first D.
        /// This method will be used with suitably large exponent to perform regular
        /// multiplication for very large numbers, since Schonhage-Strassen scales better.
        /// </remarks>
        internal static MpInt MultiplyModFermat(MpInt left, MpInt right, int exponent, int logNumParts, MpInt modulus)
        {
            MpInt result;

            if (left.NumBits < SchonhageStrassenModFermatMinimumBits &&
                right.NumBits < SchonhageStrassenModFermatMinimumBits)
            {
                result = MultiplySimpleModFermat(left, right, exponent, modulus);
            }
            else
            {
                result = MultiplySchonhageStrassen(left, right, exponent, logNumParts, modulus);
            }

            return result;
        }

        private static MpInt MultiplySimpleModFermat(MpInt left, MpInt right, int exponent, MpInt modulus)
        {
            MpInt product = MpInt.Zero;
            MultiplyMagnitudes(left, right, product);
            return ReduceModFermat(product, exponent, modulus);
        }

        private static MpInt ReduceModFermat(MpInt product, int exponent, MpInt modulus)
        {
            if (product >= modulus)
            {
                // Using 2^n = -1 (mod 2^n + 1)
                MpInt result = product.BitSubrange(0, exponent - 1);
                MpInt subtractBits = product.BitSubrange(exponent, 2 * exponent - 1);
                result.SubtractInPlace(subtractBits);
                MpInt addBits = product.BitSubrange(2 * exponent, 3 * exponent - 1);
                result.AddInPlace(addBits);

                if (result.IsNegative())
                {
                    result.AddInPlace(modulus);
                }
                else if (result >= modulus)
                {
                    result.SubtractInPlace(modulus);
                }
                return result;
            }
            else
            {
                return product;
            }
        }

        internal static MpInt SquareModFermat(MpInt mpi, int exponent, int logNumParts, MpInt modulus)
        {
            MpInt result;

            if (mpi.NumBits < SchonhageStrassenModFermatMinimumBits)
            {
                result = SquareSimpleModFermat(mpi, exponent, modulus);
            }
            else
            {
                result = MultiplySchonhageStrassen(mpi, mpi, exponent, logNumParts, modulus);
            }

            return result;
        }

        private static MpInt SquareSimpleModFermat(MpInt mpi, int exponent, MpInt modulus)
        {
            return ReduceModFermat(Square(mpi), exponent, modulus);
        }

        /// <remarks>
        /// This explanation is based on a simplification of the paper
        /// "Towards Optimal Toom-Cook Multiplication for Univariate and
        /// Multivariate Polynomials in Characteristic 2 and 0" by Marco Bodrato.
        /// 
        /// Toom-Cook multiplication operates by first conceptually converting
        /// each integer into a polynomial p of some fixed degree (Toom-Cook
        /// with a degree k-1 polynomial is called "Toom-k"). Its coefficients
        /// are groups of z digits in the integer and p(base^z) = the integer.
        /// For example, if the base is 10^2, the integer is 1234567890, and
        /// we want Toom-3, we could choose z=2, producing the polynomial:
        ///    
        ///    p(x) = 12x^2 + 3456x + 7890
        /// 
        /// Let the polynomials be p and q, let n = deg(p)+1, m = deg(q)+1,
        /// and let p_i and q_i be the coefficient of x^i in p and q. We
        /// then choose some m + n - 1 points a_0...a_k to evaluate
        /// p and q at, and perform the evaluation using a matrix multiplication:
        /// 
        /// (p(a_0))   (1 a_0 a_0^2 ... a_0^n)(  p_0  )
        /// (p(a_1))   (1 a_1 a_1^2 ... a_1^n)(  p_1  )
        /// ( ...  ) = (            ...      )(  ...  )
        /// ( ...  )   (1 a_k a_k^2 ... a_k^n)(p_(n-1))
        /// (p(a_k))
        /// 
        /// We pointwise multiply the p(a_i) and q(a_i) recursively using
        /// our top-level multiplication function, then interpolate to
        /// get a degree k product polynomial r using the matrix inverse and
        /// multiplication:
        /// 
        /// (r_0)   (1 a_0 a_0^2 ... a_0^k)^(-1)(r(a_0))
        /// (r_1)   (1 a_1 a_1^2 ... a_1^k)     (r(a_1))
        /// (...) = (            ...      )     ( ...  )
        /// (r_k)   (1 a_k a_k^2 ... a_k^k)     (r(a_k))
        /// 
        /// Finally, we evaluate this polynomial at base^z using shifts and adds.
        /// 
        /// The degree k determines both the complexity and overhead of the method.
        /// In practice, nothing above Toom-3 is used because the resulting overhead
        /// makes it inferior to FFT-based multiplication on the range of inputs
        /// where it does better than Toom-3. It's possible to use different
        /// degrees for the two inputs, which may be useful if they have very
        /// different sizes. We might want to implement a "Toom-2.5" using
        /// degree 3 for the larger operand and degree 2 for the smaller, and
        /// perhaps even a "Toom-1.5" using degree 2 for the larger operand
        /// and degree 1 for the smaller.
        /// 
        /// This method implements 2-way Toom-Cook multiplication, which is the
        /// same thing as the Karatsuba algorithm when we evaluate at
        /// 0, 1, and 1/0. 1/0 is a special value obeying the property that
        /// (1/0)^i = 0 unless i is the degree of the polynomial, in which
        /// case it's 1. Our matrix products degenerate to:
        /// 
        /// ((1/0)^0 (1/0)^1) = (0 1)
        /// 
        /// (p(0)  )   (1 0)(p_0)   (   p_0   )
        /// (p(1)  ) = (1 1)(p_1) = (p_0 + p_1)
        /// (p(1/0))   (0 1)        (   p_1   )
        /// 
        /// ((1/0)^0 (1/0)^1 (1/0)^2) = (0 0 1)
        /// 
        /// (r_0)   (1 0 0)^(-1)( r(0) )   ( 1 0  0)( r(0) )   (     r(0)       )
        /// (r_1) = (1 1 1)     ( r(1) ) = (-1 1 -1)( r(1) ) = (r(1)-r(0)-r(1/0))
        /// (r_2)   (0 0 1)     (r(1/0))   ( 0 0  1)(r(1/0))   (    r(1/0)      )
        /// </remarks>
        private static void MultiplyToom2(MpInt left, MpInt right, MpInt result)
        {
            int leftLength = left.GetNonzeroLength();
            int rightLength = right.GetNonzeroLength();
            int z = Math.Max(leftLength/2, rightLength/2);

            MpInt p_0 = left.DigitSubrange(0, z - 1);
            MpInt p_1 = left.DigitSubrange(z, leftLength-1);
            MpInt q_0 = right.DigitSubrange(0, z - 1);
            MpInt q_1 = right.DigitSubrange(z, rightLength-1);

            MpInt r_at_0   = p_0.Multiply(q_0);
            MpInt r_at_inf = p_1.Multiply(q_1);

            MpInt p_sum = p_0;
            p_sum.AddInPlace(p_1);
            MpInt q_sum = q_0;
            q_sum.AddInPlace(q_1);
            MpInt r_at_1 = p_sum.Multiply(q_sum);

            Toom2Interpolate(z, r_at_0, r_at_1, r_at_inf, result);
        }

        /// <summary>
        /// This is a version of MultiplyToom2 for squaring a single number. It's
        /// faster not only because it's simpler, but because it uses Square() for
        /// recursive calls as well.
        /// </summary>
        private static void SquareToom2(MpInt mpi, MpInt result)
        {
            int mpiLength = mpi.GetNonzeroLength();
            int z = mpiLength/2;

            MpInt p_0 = mpi.DigitSubrange(0, z - 1);
            MpInt p_1 = mpi.DigitSubrange(z, mpiLength-1);

            MpInt r_at_0   = Square(p_0);
            MpInt r_at_inf = Square(p_1);

            MpInt p_sum = p_0;
            p_sum.AddInPlace(p_1);
            MpInt r_at_1 = Square(p_sum);

            Toom2Interpolate(z, r_at_0, r_at_1, r_at_inf, result);
        }

        private static void Toom2Interpolate(int z, MpInt r_at_0, MpInt r_at_1, MpInt r_at_inf, MpInt result)
        {
            MpInt r_0 = r_at_0;
            MpInt r_1 = r_at_1;
            r_1.SubtractInPlace(r_at_0);
            r_1.SubtractInPlace(r_at_inf);
            MpInt r_2 = r_at_inf;

            result.AssignFrom(r_2);
            result.ShiftLeftDigitsInPlace(z);
            result.AddInPlace(r_1);
            result.ShiftLeftDigitsInPlace(z);
            result.AddInPlace(r_0);
        }

        /// <remarks>
        /// The trivial Toom-1.5 version of Toom-Cook (split the larger
        /// operand in 2 and don't split the smaller one). See remarks on
        /// MultiplyToom2 for details on the algorithm. We evaluate
        /// at the 2 points 0 and 1/0. Our matrix products degenerate to:
        /// 
        /// (p(0)  ) = (1 0)(p_0) = (p_0)
        /// (p(1/0))   (0 1)(p_1)   (p_1)
        /// 
        /// (q(0)  ) = (1)(q_0) = (q_0)
        /// (q(1/0))   (1)(q_1)   (q_0)
        /// 
        /// (r_0) = (1  0)^(-1)(r(0)) = (r(0)) = (p_0 q_0)
        /// (r_1)   (0  1)     (r(1))   (r(1))   (p_1 q_0)
        /// </remarks>
        private static void MultiplyToom1_5(MpInt left, MpInt right, MpInt result)
        {
            int leftLength = left.GetNonzeroLength();
            int rightLength = right.GetNonzeroLength();
            int z = Math.Max(leftLength/2, rightLength);

            MpInt p_0 = left.DigitSubrange(0, z - 1);
            MpInt p_1 = left.DigitSubrange(z, leftLength-1);
            MpInt q = right;

            MpInt r_0   = p_0.Multiply(q);
            MpInt r_1   = p_1.Multiply(q);

            result.AssignFrom(r_1);
            result.ShiftLeftDigitsInPlace(z);
            result.AddInPlace(r_0);
        }

        /// <remarks>
        /// Toom-2.5, a version of Toom-Cook that splits the larger operand
        /// into 3 parts and the smaller operand into 2. See remarks on
        /// MultiplyToom2 for details on the algorithm. We evaluate at
        /// the 4 points 0, 1, -1, and 1/0 (the special value that
        /// equals 1 only when raised to the power of the polynomial
        /// degree and 0 otherwise). Our matrix products degenerate to:
        /// 
        /// (p(0)  )   (1  0  0)(p_0)   (p_0)
        /// (p(1)  ) = (1  1  1)(p_1) = (p_0 + p_1 + p_2)
        /// (p(-1) )   (1 -1  1)(p_2)   (p_0 - p_1 + p_2)
        /// (p(1/0))   (0  0  1)        (p_2)
        /// 
        /// (q(0)  )   (1  0)(q_0)   (q_0)
        /// (q(1)  ) = (1  1)(q_1) = (q_0 + q_1)
        /// (q(-1) )   (1 -1)        (q_0 - q_1)
        /// (q(1/0))   (0  1)        (q_1)
        /// 
        /// (r_0)   (1  0  0  0)^(-1)( r(0) ) = ( 1  0    0   0)( r(0) )
        /// (r_1) = (1  1  1  1)     ( r(1) )   ( 0 1/2 -1/2 -1)( r(1) )
        /// (r_2)   (1 -1  1 -1)     (r(-1) )   (-1 1/2  1/2  0)(r(-1) )
        /// (r_3)   (0  0  0  1)     (r(1/0))   ( 0  0    0   1)(r(1/0))
        /// 
        /// To compute this product efficiently we use this sequence:
        /// 
        /// r_0 = r(0)
        /// r_3 = r(1/0)
        /// r_1 = (r(1) - r(-1))/2
        /// r_2 = r_1 + r(-1) - r(0)
        /// r_1 = r_1 - r(1/0)
        /// /</remarks>
        private static void MultiplyToom2_5(MpInt left, MpInt right, MpInt result)
        {
            int leftLength = left.GetNonzeroLength();
            int rightLength = right.GetNonzeroLength();
            int z = Math.Max(leftLength/3, rightLength/2);

            MpInt p_0 = left.DigitSubrange(0, z - 1);
            MpInt p_1 = left.DigitSubrange(z, 2*z - 1);
            MpInt p_2 = left.DigitSubrange(2*z, leftLength-1);
            MpInt q_0 = right.DigitSubrange(0, z - 1);
            MpInt q_1 = right.DigitSubrange(z, rightLength-1);

            // Evaluate p at the 4 points.

            // (p(0)  )   (1  0  0)(p_0)   (p_0)
            // (p(1)  ) = (1  1  1)(p_1) = (p_0 + p_1 + p_2)
            // (p(-1) )   (1 -1  1)(p_2)   (p_0 - p_1 + p_2)
            // (p(1/0))   (0  0  1)        (p_2)

            MpInt p_at_0    = p_0;
            MpInt p_at_inf  = p_2;
            MpInt p_at_1    = p_0 + p_2;
            MpInt p_at_neg1 = p_at_1 - p_1;
            p_at_1.AddInPlace(p_1);

            // Evaluate q at the 4 points.
            
            // (q(0)  )   (1  0)(q_0)   (q_0)
            // (q(1)  ) = (1  1)(q_1) = (q_0 + q_1)
            // (q(-1) )   (1 -1)        (q_0 - q_1)
            // (q(1/0))   (0  1)        (q_1)
            MpInt q_at_0    = q_0;
            MpInt q_at_1    = q_0 + q_1;
            MpInt q_at_neg1 = q_0 - q_1;
            MpInt q_at_inf  = q_1;
            
            // Pointwise multiply
            MpInt r_at_0    = p_at_0.Multiply(q_at_0);
            MpInt r_at_1    = p_at_1.Multiply(q_at_1);
            MpInt r_at_neg1 = p_at_neg1.Multiply(q_at_neg1);
            MpInt r_at_inf  = p_at_inf.Multiply(q_at_inf);

            // Interpolate r's coefficients from the 5 points.
            // r_0 = r(0)
            MpInt r_0 = r_at_0;

            // r_3 = r(1/0)
            MpInt r_3 = r_at_inf;

            // r_1 = (r(1) - r(-1))/2
            MpInt r_1 = new MpInt(r_at_1); // Safe to alias, not used again
            r_1.SubtractInPlace(r_at_neg1);
            Debug.Assert(r_1.GetBit(0) == false);
            r_1.ShiftRightInPlace(1);

            // r_2 = r_1 + r(-1) - r(0)
            MpInt r_2 = new MpInt(r_1);
            r_2.AddInPlace(r_at_neg1);
            r_2.SubtractInPlace(r_at_0);

            // r_1 -= r(1/0)
            r_1.SubtractInPlace(r_at_inf);

            // Evaluate r at 2^(z*DigitBits)
            result.AddInPlace(r_3);
            result.ShiftLeftDigitsInPlace(z);
            result.AddInPlace(r_2);
            result.ShiftLeftDigitsInPlace(z);
            result.AddInPlace(r_1);
            result.ShiftLeftDigitsInPlace(z);
            result.AddInPlace(r_0);
        }

        /// <remarks>
        /// The 3-way version of Toom-Cook. See remarks on MultiplyToom2 for
        /// details on the algorithm. We evaluate at the 5 points 0, 1, -1, -2,
        /// and 1/0 (the special value that equals 1 only when raised to the
        /// power of the polynomial degree and 0 otherwise). Our matrix
        /// products degenerate to:
        /// 
        /// (p(0)  )   (1  0  0)(p_0)   (p_0)
        /// (p(1)  )   (1  1  1)(p_1)   (p_0 + p_1 + p_2)
        /// (p(-1) ) = (1 -1  1)(p_2) = (p_0 - p_1 + p_2)
        /// (p(-2) )   (1 -2  4)        (p_0 - p_1*2 + p_2*4)
        /// (p(1/0))   (0  0  1)        (p_2)
        /// 
        /// (r_0)   (1  0  0  0  0)^(-1)( r(0) )   (  1    0     0    0    0)( r(0) )
        /// (r_1)   (1  1  1  1  1)     ( r(1) )   ( 1/2  1/3   -1   1/6  -2)( r(1) )
        /// (r_2) = (1 -1  1 -1  1)     (r(-1) ) = ( -1   1/2   1/2   0   -1)(r(-1) )
        /// (r_3)   (1 -2  4 -8 16)     (r(-2) )   (-1/2  1/6   1/2 -1/6   2)(r(-2) )
        /// (r_4)   (0  0  0  0  1)     (r(1/0))   (  0    0     0    0    1)(r(1/0))
        /// 
        /// The trick is now to compute this product efficiently. One sequence that
        /// works well (from the Wikipedia article on Toom-Cook multiplication) is:
        /// 
        /// r_0 = r(0)
        /// r_4 = r(1/0)
        /// r_3 = (r(-2) - r(1))/3
        /// r_1 = (r(1) - r(-1))/2
        /// r_2 = r(-1) - r(0)
        /// r_3 = (r_2 - r_3)/2 + 2*r(1/0)
        /// r_2 = r_2 + r_1 - r(1/0)
        /// r_1 = r_1 - r_3
        /// </remarks>
        private static void MultiplyToom3(MpInt left, MpInt right, MpInt result)
        {
            int leftLength = left.GetNonzeroLength();
            int rightLength = right.GetNonzeroLength();
            int z = Math.Max(leftLength/3, rightLength/3);

            MpInt p_0 = left.DigitSubrange(0, z - 1);
            MpInt p_1 = left.DigitSubrange(z, 2*z - 1);
            MpInt p_2 = left.DigitSubrange(2*z, leftLength-1);
            MpInt q_0 = right.DigitSubrange(0, z - 1);
            MpInt q_1 = right.DigitSubrange(z, 2*z - 1);
            MpInt q_2 = right.DigitSubrange(2*z, rightLength-1);

            // Evaluate p at the 5 points.
            // p(0)   = p_0
            MpInt p_at_0 = p_0; // Can alias, we don't modify p_0

            // p(1/0) = p_2
            MpInt p_at_inf = new MpInt(p_2);

            // p(1)   = p_0 + p_1 + p_2
            MpInt p_at_1 = new MpInt(p_0);
            p_at_1.AddInPlace(p_1);
            p_at_1.AddInPlace(p_2);

            // p(-1)  = p_0 - p_1 + p_2 = p(1) - p_1*2
            p_1.ShiftLeftInPlace(1); // Now p_1*2
            MpInt p_at_neg1 = p_at_1 - p_1;

            // p(-2)  = p_0 - p_1*2 + p_2*4
            p_2.ShiftLeftInPlace(2); // Now p_2*4
            MpInt p_at_neg2 = new MpInt(p_0);
            p_at_neg2.SubtractInPlace(p_1);
            p_at_neg2.AddInPlace(p_2);

            // Evaluate q at the 5 points.
            // q(0)   = q_0
            MpInt q_at_0 = q_0; // Can alias, we don't modify q_0

            // q(1/0) = q_2
            MpInt q_at_inf = new MpInt(q_2);

            // q(1)   = q_0 + q_1 + q_2
            MpInt q_at_1 = new MpInt(q_0);
            q_at_1.AddInPlace(q_1);
            q_at_1.AddInPlace(q_2);

            // q(-1)  = q_0 - q_1 + q_2 = q(1) - q_1*2
            q_1.ShiftLeftInPlace(1); // Now q_1*2
            MpInt q_at_neg1 = q_at_1 - q_1;

            // q(-2)  = q_0 - q_1*2 + q_2*4
            q_2.ShiftLeftInPlace(2); // Now q_2*4
            MpInt q_at_neg2 = new MpInt(q_0);
            q_at_neg2.SubtractInPlace(q_1);
            q_at_neg2.AddInPlace(q_2);

            // Pointwise multiply
            MpInt r_at_0    = p_at_0.Multiply(q_at_0);
            MpInt r_at_1    = p_at_1.Multiply(q_at_1);
            MpInt r_at_neg1 = p_at_neg1.Multiply(q_at_neg1);
            MpInt r_at_neg2 = p_at_neg2.Multiply(q_at_neg2);
            MpInt r_at_inf  = p_at_inf.Multiply(q_at_inf);

            Toom3Interpolate(z, r_at_0, r_at_1, r_at_neg1, r_at_neg2, r_at_inf, result);
        }

        /// <remarks>
        /// Simplified version of Toom-3 for squaring. Also faster because it
        /// makes recursive calls to Square().
        /// </remarks>
        private static void SquareToom3(MpInt mpi, MpInt result)
        {
            int mpiLength = mpi.GetNonzeroLength();
            int z = mpiLength/3;

            MpInt p_0 = mpi.DigitSubrange(0, z - 1);
            MpInt p_1 = mpi.DigitSubrange(z, 2*z - 1);
            MpInt p_2 = mpi.DigitSubrange(2*z, mpiLength-1);

            // Evaluate p at the 5 points.
            // p(0)   = p_0
            MpInt p_at_0 = p_0; // Can alias, we don't modify p_0

            // p(1/0) = p_2
            MpInt p_at_inf = new MpInt(p_2);

            // p(1)   = p_0 + p_1 + p_2
            MpInt p_at_1 = new MpInt(p_0);
            p_at_1.AddInPlace(p_1);
            p_at_1.AddInPlace(p_2);

            // p(-1)  = p_0 - p_1 + p_2 = p(1) - p_1*2
            p_1.ShiftLeftInPlace(1); // Now p_1*2
            MpInt p_at_neg1 = p_at_1 - p_1;

            // p(-2)  = p_0 - p_1*2 + p_2*4
            p_2.ShiftLeftInPlace(2); // Now p_2*4
            MpInt p_at_neg2 = new MpInt(p_0);
            p_at_neg2.SubtractInPlace(p_1);
            p_at_neg2.AddInPlace(p_2);

            // Pointwise multiply
            MpInt r_at_0    = Square(p_at_0);
            MpInt r_at_1    = Square(p_at_1);
            MpInt r_at_neg1 = Square(p_at_neg1);
            MpInt r_at_neg2 = Square(p_at_neg2);
            MpInt r_at_inf  = Square(p_at_inf);

            Toom3Interpolate(z, r_at_0, r_at_1, r_at_neg1, r_at_neg2, r_at_inf, result);
        }

        private static void Toom3Interpolate(int z, MpInt r_at_0, MpInt r_at_1, MpInt r_at_neg1,
                                                    MpInt r_at_neg2, MpInt r_at_inf, MpInt result)
        {
            uint remainder;
            // Interpolate r's coefficients from the 5 points.
            // r_0 = r(0)
            MpInt r_0 = r_at_0;

            // r_4 = r(1/0)
            MpInt r_4 = r_at_inf;

            // r_3 = (r(-2) - r(1))/3
            MpInt r_3 = r_at_neg2 - r_at_1;
            r_3.DivideInPlace(3u, out remainder);
            Debug.Assert(remainder == 0);

            // r_1 = (r(1) - r(-1))/2
            MpInt r_1 = r_at_1 - r_at_neg1;
            Debug.Assert(r_1.GetBit(0) == false);
            r_1.ShiftRightInPlace(1);

            // r_2 = r(-1) - r(0)
            MpInt r_2 = r_at_neg1 - r_at_0;

            // r_3 = (r_2 - r_3)/2 + 2*r(1/0)
            r_3.NegateInPlace();
            r_3.AddInPlace(r_2);
            r_3.ShiftRightInPlace(1);
            r_3.AddInPlace(r_at_inf << 1);

            // r_2 = r_2 + r_1 - r(1/0)
            r_2.AddInPlace(r_1);
            r_2.SubtractInPlace(r_at_inf);

            // r_1 = r_1 - r_3
            r_1.SubtractInPlace(r_3);

            // Evaluate r at 2^(z*DigitBits)
            result.AssignFrom(r_4);
            result.ShiftLeftDigitsInPlace(z);
            result.AddInPlace(r_3);
            result.ShiftLeftDigitsInPlace(z);
            result.AddInPlace(r_2);
            result.ShiftLeftDigitsInPlace(z);
            result.AddInPlace(r_1);
            result.ShiftLeftDigitsInPlace(z);
            result.AddInPlace(r_0);
        }

        // The largest value of the convolution is bounded above by the
        // largest digit value squared times the number of digits in the
        // middle column (plus one for the carry). In order for the
        // intermediate results of convolution to fit in a double, which
        // only has a 53-bit mantissa (we hit issues with precision before
        // range), we must choose the value k such that
        //   (min(ceil(leftBits/k), ceil(rightBits/k)) + 1)*2^(2k) < 2^54
        // This doesn't really have a closed form solution, but the choice
        // of k=10 addresses a large range of leftBits and rightBits.
        const int BitsPerComplexDouble = 10;

        private static void MultiplySchonhageStrassenTopLevel(MpInt left, MpInt right, MpInt result, int logNumParts)
        {
            int exponent = MpInt.RoundUpToNearestMultiple(left.NumBits + right.NumBits + 1,
                                                          1 << logNumParts);

            MpInt modulus = new MpInt(1);
            modulus.ShiftLeftInPlace(exponent);
            modulus.AddInPlace(new MpInt(1));

            result.AssignFrom(MultiplyModFermat(left, right, exponent, logNumParts, modulus));
        }
        
        /// <summary>
        /// Multiplies left by right mod 2^exponent + 1 using the
        /// Schonhage-Strassen algorithm.
        /// </summary>
        /// <remarks>
        /// Based on two main sources:
        /// 1. Richard Crandall and Carl Pomerance. Prime Numbers: A Computational
        /// Perspective. Section 9.5.
        /// 2. Pierrick Gaudry, Alexander Kruppa, Paul Zimmermann. A GMP-based
        /// Implementation of Schönhage-Strassen’s Large Integer Multiplication
        /// Algorithm. http://www.loria.fr/~gaudry/publis/issac07.pdf
        /// </remarks>
        internal static MpInt MultiplySchonhageStrassen(MpInt left, MpInt right, int exponent, int logNumParts, MpInt modulus)
        {
            Debug.Assert(logNumParts >= 1);
            int fftSize = 1 << logNumParts; // Called D in _Prime Numbers_
            Debug.Assert((exponent % fftSize) == 0, "Exponent must have last logNumParts bits set to zero");

            int bitsPerPart = exponent >> logNumParts; // Called M in _Prime Numbers_

            int exponentRecursive = MpInt.RoundUpToNearestMultiple(2 * bitsPerPart + logNumParts, fftSize); // Called n' in _Prime Numbers_
            Debug.Assert(exponentRecursive <= exponent); // Exponent should strictly decrease with each recursive call, except in unit testing where it can be the same
            int MPrime = exponentRecursive >> logNumParts;

            // Compute modulus for exponentRecursive
            MpInt modulusRecursive = new MpInt(1);
            modulusRecursive.ShiftLeftInPlace(exponentRecursive);
            modulusRecursive.AddInPlace(new MpInt(1));

            // We don't mod inputs, they must already be in range. This is an
            // assertion because the caller is in this class and must ensure it.
            Debug.Assert(left.IsNonNegative() && left < modulus, "left out of range");
            Debug.Assert(right.IsNonNegative() && right < modulus, "right out of range");

            // Initialize A and B
            MpInt[] A = new MpInt[fftSize];
#if DEBUG
            MpInt[] expectedNegacyclicConvolution;
#endif
            if (Object.ReferenceEquals(left, right))
            {
                for (int i = 0; i < fftSize; i++)
                {
                    A[i] = left.BitSubrange(i * bitsPerPart, (i + 1) * bitsPerPart - 1);
                    A[i].ReserveBits(exponentRecursive + 1);
                }
#if DEBUG
                expectedNegacyclicConvolution = NegacyclicConvolution(A, A);
#endif
                // Weight the A for negacyclic convolution
                for (int j = 0; j < fftSize; j++)
                {
                    ShiftLeftModFermatInPlace(A[j], j * MPrime, exponentRecursive, modulusRecursive);
                }

                // Compute negacyclic convolution of A and B by finding their DFTs,
                // pointwise multiplying them recursively, then inversing the DFT.
                DftModFermatInPlaceScramble(A, 2 * MPrime, exponentRecursive, modulusRecursive);
                for (int i = 0; i < fftSize; i++)
                {
                    A[i] = SquareModFermat(A[i], exponentRecursive, logNumParts, modulusRecursive);
                }
            }
            else
            {
                MpInt[] B = new MpInt[fftSize];
                for (int i = 0; i < fftSize; i++)
                {
                    A[i] = left.BitSubrange(i * bitsPerPart, (i + 1) * bitsPerPart - 1);
                    A[i].ReserveBits(exponentRecursive + 1);
                    B[i] = right.BitSubrange(i * bitsPerPart, (i + 1) * bitsPerPart - 1);
                    B[i].ReserveBits(exponentRecursive + 1);
                }
#if DEBUG
                expectedNegacyclicConvolution = NegacyclicConvolution(A, B);
#endif
                // Weight the A for negacyclic convolution
                for (int j = 0; j < fftSize; j++)
                {
                    ShiftLeftModFermatInPlace(A[j], j * MPrime, exponentRecursive, modulusRecursive);
                    ShiftLeftModFermatInPlace(B[j], j * MPrime, exponentRecursive, modulusRecursive);
                }

                // Compute negacyclic convolution of A and B by finding their DFTs,
                // pointwise multiplying them recursively, then inversing the DFT.
                DftModFermatInPlaceScramble(A, 2 * MPrime, exponentRecursive, modulusRecursive);
                DftModFermatInPlaceScramble(B, 2 * MPrime, exponentRecursive, modulusRecursive);
                for (int i = 0; i < fftSize; i++)
                {
                    A[i] = MultiplyModFermat(A[i], B[i], exponentRecursive, logNumParts, modulusRecursive);
                }
            }
            DftModFermatInPlaceUnscramble(A, -2 * MPrime, exponentRecursive, modulusRecursive);

            // Normalization, undo weighting for negacyclic convolution,
            // and convert to negative if > modulusRecursive/2
            for (int j = 0; j < fftSize; j++)
            {
                ShiftRightModFermatInPlace(
                    A[j], logNumParts + j * MPrime, exponentRecursive, modulusRecursive);

                MpInt maxValue = new MpInt(j + 1);
                maxValue.ShiftLeftInPlace(2 * bitsPerPart);
                if (A[j] >= maxValue)
                {
                    A[j].SubtractInPlace(modulusRecursive);
                }
#if DEBUG
                Debug.Assert(A[j] == expectedNegacyclicConvolution[j]);
#endif
            }

            // Carry adjustment
            MpInt result = new MpInt(0);
            for (int i = A.Length - 1; i >= 0; i--)
            {
                ShiftLeftModFermatInPlace(result, bitsPerPart, exponent, modulus);
                result.AddInPlace(A[i]);
                if (result >= modulus)
                {
                    result.SubtractInPlace(modulus);
                }
            }

            return result;
        }

#if DEBUG
        private static MpInt[] NegacyclicConvolution(MpInt[] left, MpInt[] right)
        {
            MpInt[] result = new MpInt[left.Length];

            for (int col = 0; col < left.Length; col++)
            {
                MpInt colSum = MpInt.Zero;
                for (int j = 0; j <= col; j++)
                {
                    int i = col - j;
                    colSum += left[i] * right[j];
                }
                for (int j = 0; j < left.Length; j++)
                {
                    int i = col + left.Length - j;
                    if (i < left.Length)
                    {
                        colSum -= left[i] * right[j];
                    }
                }
                result[col] = colSum;
            }

            return result;
        }
#endif

        /// <summary>
        /// Uses the Gentleman-Sande algorithm (decimation-in-frequency) to
        /// simultaneously take the FFT in-place and scramble the array entries
        /// in a way that can be undone by DftModFermatInPlaceUnscramble.
        /// </summary>
        internal static void DftModFermatInPlaceScramble(MpInt[] signal, int rootUnityTwoExponent, int exponent, MpInt modulus)
        {
            int MPrime = exponent / signal.Length;

            for (int m = (signal.Length >> 1); m > 0; m >>= 1)
            {
                for (int j = 0; j < m; j++)
                {
                    // g^(-jn)/2m = (2^(2M'))^(-jn)/2m = 2^(-2M'jn/2m) = 1 >> 2M'j(n/2m),
                    // so we'll do multiplication by a with a shift right.
                    int shiftRightBits = rootUnityTwoExponent * j * signal.Length / (2 * m);
                    for (int i = j; i < signal.Length; i += 2 * m)
                    {
                        MpInt signal_i = signal[i];
                        signal[i] = signal_i + signal[i + m];
                        if (signal[i] >= modulus)
                        {
                            signal[i].SubtractInPlace(modulus);
                        }
                        signal[i + m] = signal_i - signal[i + m];
                        if (signal[i + m].IsNegative())
                        {
                            signal[i + m].AddInPlace(modulus);
                        }
                        ShiftRightModFermatInPlace(signal[i + m], shiftRightBits, exponent, modulus);
                    }
                }
            }
        }

        /// <summary>
        /// Uses the Cooley-Tukey algorithm (decimation-in-time) to
        /// simultaneously take the FFT in-place and unscramble the array entries
        /// from the order DftModFermatInPlaceScramble placed them in.
        /// </summary>
        internal static void DftModFermatInPlaceUnscramble(MpInt[] signal, int rootUnityTwoExponent, int exponent, MpInt modulus)
        {
            for (int m = 1; m < signal.Length; m <<= 1)
            {
                for (int j = 0; j < m; j++)
                {
                    // g^(-jn)/2m = (2^(2M'))^(-jn)/2m = 2^(-2M'jn/2m) = 1 >> 2M'j(n/2m)
                    int shiftRightBits = rootUnityTwoExponent * j * signal.Length / (2 * m);
                    for (int i = j; i < signal.Length; i += 2 * m)
                    {
                        ShiftRightModFermatInPlace(signal[i + m], shiftRightBits, exponent, modulus);
                        MpInt signalICopy = new MpInt(signal[i]);
                        signal[i] = signalICopy + signal[i + m];
                        if (signal[i] >= modulus)
                        {
                            signal[i].SubtractInPlace(modulus);
                        }
                        signal[i + m] = signalICopy - signal[i + m];
                        if (signal[i + m].IsNegative())
                        {
                            signal[i + m].AddInPlace(modulus);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes (2^shiftAmount)(mpi) mod (modulus), where modulus
        /// is 2^exponent + 1.
        /// </summary>
        internal static MpInt ShiftLeftModFermat(MpInt x, int shiftAmount, int exponent, MpInt modulus)
        {
            if (shiftAmount < 0)
            {
                return ShiftRightModFermat(x, -shiftAmount, exponent, modulus);
            }

            // Suppose m = shiftAmount, n = exponent, and m = qn + r with r < n. Then:
            //   x << m = x(2^(qn + r))     (mod 2^n + 1)
            //          = x((2^n)^q (2^r))  (mod 2^n + 1)
            //          = (-1)^q (x << r)   (mod 2^n + 1)
            //          = (-1)^q [(low n bits of (x << r)) - (high n bits of (x << r))]
            // We then normalize it to a positive value by adding 2^n+1 if it's negative.

            int q = shiftAmount / exponent;
            int r = shiftAmount % exponent;

            MpInt result = x.BitSubrange(0, exponent - r - 1);
            result.ShiftLeftInPlace(r);
            MpInt highBits = x.BitSubrange(exponent - r, exponent);
            result.SubtractInPlace(highBits);

            if ((q % 2) == 1)
            {
                result.NegateInPlace();
            }

            if (result.IsNegative())
            {
                result.AddInPlace(modulus);
            }
            // Deal with negative zero
            if (result.IsZero())
            {
                result.AssignFrom(MpInt.Zero);
            }

            return result;
        }

        /// <summary>
        /// Replaces mpi with (2^shiftAmount)(mpi) mod (modulus), where modulus
        /// is 2^exponent + 1.
        /// </summary>
        internal static void ShiftLeftModFermatInPlace(MpInt x, int shiftAmount, int exponent, MpInt modulus)
        {
            x.AssignFrom(ShiftLeftModFermat(x, shiftAmount, exponent, modulus));
        }

        /// <summary>
        /// Computes (2^-shiftAmount)(mpi) mod (modulus), where modulus
        /// is 2^exponent + 1.
        /// </summary>
        internal static MpInt ShiftRightModFermat(MpInt x, int shiftAmount, int exponent, MpInt modulus)
        {
            if (shiftAmount < 0)
            {
                return ShiftLeftModFermat(x, -shiftAmount, exponent, modulus);
            }

            // Suppose m = shiftAmount, n = exponent, and m = qn + r with r < n. Then
            // since 2^2n = 1 (mod 2^n + 1), x >> m = x << (2n(ceil(q/2) + 1) - m).
            int q = shiftAmount / exponent;
            return ShiftLeftModFermat(x, 2 * exponent * ((q + 1) / 2 + 1) - shiftAmount, exponent, modulus);
        }

        /// <summary>
        /// Replaces mpi with (2^-shiftAmount)(mpi) mod (modulus), where modulus
        /// is 2^exponent + 1.
        /// </summary>
        public static void ShiftRightModFermatInPlace(MpInt x, int shiftAmount, int exponent, MpInt modulus)
        {
            x.AssignFrom(ShiftRightModFermat(x, shiftAmount, exponent, modulus));
        }

        internal static int DivideNewtonIterationMinimumBits = 350000;

        internal static void DivideMagnitudesInPlace(MpInt dividend, MpInt divisor, MpInt remainder)
        {
            int dividendBits = dividend.NumBits;
            if (dividendBits < DivideNewtonIterationMinimumBits)
            {
                MpInt.DivideGradeSchool(dividend, divisor, remainder);
            }
            else
            {
                DivideNewtonIteration(dividend, divisor, remainder);
            }
        }

        /// <summary>
        /// Implements a division algorithm based on Newton iteration.
        /// </summary>
        /// <remarks>
        /// This is a well-known classical method for reducing division
        /// to multiplication, described e.g. by Alan H. Karp and Peter
        /// Markstein in "High Precision Division and Square Root". It
        /// computes 1/A using the recurrence:
        ///    x_{n+1} = x_n + x_n(1 - Ax_n)
        /// It converges quadratically. When our approximation is good
        /// enough, we multiply the result by B and shift right.
        /// </remarks>
        internal static void DivideNewtonIteration(MpInt dividend, MpInt divisor, MpInt remainder)
        {
            bool dividendNegative = dividend.IsNegative();

            // Special case - convergence too slow when they're equal
            if (MpInt.CompareMagnitude(dividend, divisor) == 0)
            {
                remainder.AssignFrom(MpInt.Zero);
                if (dividend.IsNegative())
                {
                    dividend.AssignFrom(new MpInt(-1));
                }
                else
                {
                    dividend.AssignFrom(new MpInt(1));
                }
                return;
            }

            MpInt originalDivisor = divisor;
            int originalDivisorBits = originalDivisor.NumBits;
            int lsbDivisor = Math.Max(0, originalDivisorBits - 8);
            divisor = originalDivisor.BitSubrange(lsbDivisor, originalDivisorBits - 1);

            // We need an initial estimate and it must be between 0
            // and 2/D to assure convergence. We note that if
            // n = D.NumBits, then D < 2^n, so 1/2^n < 1/D. The
            // variable decimalPosition indicates the decimal
            // point for both approx and one, but not divisor,
            // which is always an integer.
            int decimalPosition = divisor.NumBits;
            MpInt approx = new MpInt(1);
            MpInt one = new MpInt(1);
            one.ShiftLeftInPlace(decimalPosition);
            const int guardBits = 8;

            // Reasoning: once we have this many bits correct, the product
            // of the dividend with the estimate will be at most 1 off.
            int targetBits = dividend.NumBits + guardBits;
            MpInt delta;
            for(int iteration = 0; ; iteration++)
            {
                delta = divisor.Multiply(approx); // Does not move decimal
                delta.SubtractFromInPlace(one);
                delta = delta.Multiply(approx); // Does move decimal, update

                // We keep expanding until we have all the bits
                // we want, then just wait for delta to reach zero.
                // We delay expansion if delta is still big, to
                // exploit smaller multiplications more.
                if (decimalPosition >= targetBits ||
                    delta.NumBits >= decimalPosition + guardBits)
                {
                    delta.ShiftRightInPlace(decimalPosition);
                    if (delta.IsZero())
                    {
                        break;
                    }
                }
                else
                {
                    approx.ShiftLeftInPlace(decimalPosition);
                    one.ShiftLeftInPlace(decimalPosition);
                    decimalPosition += decimalPosition;
                    if (lsbDivisor > 0)
                    {
                        int newLsbDivisor = Math.Max(0, lsbDivisor - decimalPosition);
                        divisor = originalDivisor.BitSubrange(newLsbDivisor, originalDivisorBits - 1);
                        decimalPosition += lsbDivisor - newLsbDivisor;
                        one.ShiftLeftInPlace(lsbDivisor - newLsbDivisor);
                        lsbDivisor = newLsbDivisor;
                    }
                }
                approx.AddInPlace(delta);
            }

            divisor = originalDivisor;

            // We have 1/divisor approximation, compute final quotient and remainder.
            MpInt quotient = dividend.Multiply(approx);
            quotient.ShiftRightInPlace(decimalPosition);
            dividend.SubtractInPlace(quotient.Multiply(divisor));
            remainder.AssignFrom(dividend);
            if (remainder >= divisor)
            {
                // Correct estimate
                remainder.SubtractInPlace(divisor);
                quotient.AddInPlace(new MpInt(1));
            }

            dividend.AssignFrom(quotient);
            if (dividendNegative && !dividend.IsNegative())
            {
                dividend.NegateInPlace();
            }
        }
    }
}

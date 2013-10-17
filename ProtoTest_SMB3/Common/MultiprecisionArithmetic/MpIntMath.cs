///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  This source file is originally from the C# Multiprecision Arithmetic Library.
//  See http://codebox/csmal for details and updates.
//

// Summary: Analogous to System.Math, provides a number of special functions that
// are computed on MpInts through their public interface.

using System;
using System.Diagnostics;

namespace Microsoft.Protocols.TestTools.StackSdk.MultiprecisionArithmetic
{
    /// <summary>
    /// Analogous to System.Math, provides a number of special functions that
    /// are computed on MpInts through their public interface.
    /// </summary>
    public static class MpIntMath
    {
        /// <summary>
        /// Raises an MpInt to a 32-bit nonnegative integer power.
        /// </summary>
        public static MpInt Pow(MpInt mpi, uint power)
        {
            int resultBits = checked((int)(mpi.NumBits*power));
            MpIntInPlace result = new MpIntInPlace(new MpInt(1));
            result.ReserveBits(resultBits);

            MpIntInPlace mpiRaised = new MpIntInPlace(new MpInt(mpi));
            mpiRaised.ReserveBits(resultBits);
            if (mpiRaised.MpInt.IsNegative())
            {
                mpiRaised.MpInt.NegateInPlace();
            }
            bool oddPower = (power & 1) != 0;

            while (power != 0)
            {
                if ((power & 1) != 0)
                {
                    result.MultiplyInPlace(mpiRaised.MpInt);
                }
                power /= 2;
                mpiRaised.MultiplyInPlace(mpiRaised.MpInt);
            }

            if (mpi.IsNegative() && oddPower)
            {
                result.MpInt.NegateInPlace();
            }

            return result.MpInt;
        }

        /// <summary>
        /// Raises an MpInt to an MpInt power mod an MpInt modulus.
        /// </summary>
        public static MpInt PowMod(MpInt mpi, MpInt power, MpInt modulus)
        {
            MpInt result = MpInt.Zero;
            PowMod(mpi, power, modulus, result);
            return result;
        }

        /// <summary>
        /// Raises an MpInt to an MpInt power mod an MpInt modulus, placing result in result.
        /// </summary>
        public static void PowMod(MpInt mpi, MpInt power, MpInt modulus, MpInt result)
        {
            if (mpi.IsNegative())
            {
                throw new ArgumentException("number being powered must be nonnegative when using a modulus");
            }
            if (modulus.IsNonPositive())
            {
                throw new ArgumentException("modulus must be positive");
            }
            if (mpi.CompareTo(modulus) >= 0)
            {
                throw new ArgumentException("value being powered must be less than modulus");
            }
            if (Object.ReferenceEquals(result, power) ||
                Object.ReferenceEquals(result, modulus))
            {
                throw new ArgumentException("Cannot write result of PowMod over power or modulus");
            }

            // Reserve modulus.NumBits * 2 bits in each, since before
            // reducing each can hold a product of two numbers less
            // than modulus.
            MpIntInPlace mpiRaised = new MpIntInPlace(new MpInt(mpi));
            mpiRaised.ReserveBits(modulus.NumBits * 2);
            MpIntInPlace resultInPlace = new MpIntInPlace(result);
            resultInPlace.ReserveBits(modulus.NumBits * 2);
            result.AssignFrom(new MpInt(1));

            for (int i=0; i < power.NumBits; i++)
            {
                if (power.GetBit(i))
                {
                    resultInPlace.MultiplyInPlace(mpiRaised.MpInt);
                    resultInPlace.ModulusInPlace(modulus);
                }
                mpiRaised.MultiplyInPlace(mpiRaised.MpInt);
                mpiRaised.ModulusInPlace(modulus);
            }

            result.AssignFrom(resultInPlace.MpInt);
        }

        private static double DefaultPrimalityFalsePositiveProbability = 1e-6;
        private static Random builtInRandom = new Random();
        private static int trialDivisionCrossoverBits = 20;

        /// <summary>
        /// Determine primality of an MpInt with the default false
        /// positive probability bound and built-in random number source.
        /// </summary>
        public static bool IsPrime(MpInt mpi)
        {
            return IsPrime(builtInRandom, mpi);
        }

        /// <summary>
        /// Determine primality of an MpInt with the default false
        /// positive probability bound.
        /// </summary>
        public static bool IsPrime(Random random, MpInt mpi)
        {
            return IsPrime(random, mpi, DefaultPrimalityFalsePositiveProbability);
        }

        /// <summary>
        /// Determine primality of a positive MpInt with a false positive probability
        /// bounded by a given number between 0 and 1.
        /// </summary>
        public static bool IsPrime(Random random, MpInt n, double falsePositiveProbability)
        {
            if (n.IsNonPositive())
            {
                throw new ArgumentException("Can only test primality of positive numbers");
            }
            
            // 1 is a special case
            if (n.IsInt() && (int)n == 1)
            {
                return false;
            }

            if (!n.GetBit(0))
            {
                // n is even, prime only if it's 2
                return n.IsInt() && (int)n == 2;
            }

            if (n.NumBits <= trialDivisionCrossoverBits)
            {
                // Perform simple trial division
                Debug.Assert(trialDivisionCrossoverBits <= 64);
                ulong nlng = (ulong)n;
                for(ulong p = 3; p < nlng/p; p += 2)
                {
                    if ((nlng % p) == 0)
                    {
                        return false;
                    }
                }
                return true;
            }

            return IsRabinMillerPrime(random, n, falsePositiveProbability);
        }

        private static bool IsRabinMillerPrime(Random random, MpInt n, double falsePositiveProbability)
        {
            MpInt nMinusOne = n - 1;
            int s = nMinusOne.CountTrailingZeros();
            MpInt d = new MpInt(nMinusOne);
            d.ShiftRightInPlace(s);

            // At least 3/4s of a < n are witnesses for composite n.
            for (; falsePositiveProbability < 1.0; falsePositiveProbability *= 4.0) {
                MpIntInPlace a = new MpIntInPlace(MpInt.Zero);
                do {
                    a.MpInt = MpInt.Random(random, n);
                }
                while (a.MpInt.IsZero());

                PowMod(a.MpInt, d, n, a.MpInt);
                if (a.MpInt.IsInt() && (int)a.MpInt == 1) {
                    continue;
                }

                int r;
                for (r = 0; r < s; r++) {
                    if (a.MpInt == nMinusOne) {
                        break;
                    }
                    a.MultiplyInPlace(a.MpInt);
                    a.ModulusInPlace(n);
                }

                if (r == s) {
                    // It's composite
                    return false;
                }
            }

            // It's a strong probable prime
            return true;
        }

        /// <summary>
        /// Given a and b, finds gcd(a,b) and x and y such that xa+yb = gcd(a,b).
        /// In the case where gcd(a,b) = 1, x = a^-1 (mod b) and y = b^-1 (mod a).
        /// </summary>
        public static void ExtendedEuclidean(MpInt a, MpInt b,
                                             out MpInt x, out MpInt y, out MpInt gcd)
        {
            MpInt origA = a, origB = b;

            x = MpInt.Zero;
            MpInt prevX = new MpInt(1);
            y = new MpInt(1);
            MpInt prevY = MpInt.Zero;

            while (!b.IsZero())
            {
                MpInt bCopy = new MpInt(b);
                MpInt aDivB = new MpInt(a);
                aDivB.DivideInPlace(bCopy, out b);
                a = bCopy;

                MpInt currentX = x;
                x = prevX - aDivB * x;
                prevX = currentX;

                MpInt currentY = y;
                y = prevY - aDivB * y;
                prevY = currentY;
            }

            x = prevX;
            y = prevY;
            gcd = a;

            Debug.Assert(x * origA + y * origB == gcd);
        }

        /// <summary>
        /// Given a and modulus coprime, finds inverse mod modulus of a. Common
        /// special case of ExtendedEuclidean.
        /// </summary>
        public static MpInt ModularInverse(MpInt a, MpInt modulus)
        {
            MpInt result, y, gcd;
            ExtendedEuclidean(a, modulus, out result, out y, out gcd);
            if (!gcd.IsInt() || (int)gcd != 1)
            {
                throw new ArgumentException("Arguments not coprime");
            }
            return result;
        }
    }
}

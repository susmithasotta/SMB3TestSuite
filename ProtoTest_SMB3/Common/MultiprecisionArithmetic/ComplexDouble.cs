///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  This source file is originally from the C# Multiprecision Arithmetic Library.
//  See http://codebox/csmal for details and updates.
//

using System;
using System.Diagnostics;

// A simple class describing the real and imaginary parts of a complex
// number each with double precision floating-point precision.

namespace Microsoft.Protocols.TestTools.StackSdk.MultiprecisionArithmetic
{
    /// <summary>
    /// Double precision complexnumber
    /// </summary>
    public struct ComplexDouble
    {
        double re;
        double im;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="realPart">The real part of the complexnumber</param>
        /// <param name="imaginaryPart">The imaginary part of the complexnumber</param>
        public ComplexDouble(double realPart, double imaginaryPart)
        {
            this.re = realPart;
            this.im = imaginaryPart;
        }

        /// <summary>
        /// The real part of the complexnumber
        /// </summary>
        public double Re
        {
            get
            {
                return re;
            }
            set
            {
                re = value;
            }
        }

        /// <summary>
        /// The imaginary part of the complexnumber
        /// </summary>
        public double Im
        {
            get
            {
                return im;
            }
            set
            {
                im = value;
            }
        }


        /// <summary>
        /// The plus operation of two complexnumbers
        /// </summary>
        /// <param name="left">The left side complexnumber of the plus operation</param>
        /// <param name="right">The right side complexnumber of the plus operation</param>
        /// <returns>The operation result</returns>
        public static ComplexDouble operator +(ComplexDouble left, ComplexDouble right)
        {
            return new ComplexDouble(left.re + right.re, left.im + right.im);
        }


        /// <summary>
        /// The subtract operation of two complexnumbers
        /// </summary>
        /// <param name="left">The left side complexnumber of the subtract operation</param>
        /// <param name="right">The right side complexnumber of the subtract operation</param>
        /// <returns>The operation result</returns>
        public static ComplexDouble operator -(ComplexDouble left, ComplexDouble right)
        {
            return new ComplexDouble(left.re - right.re, left.im - right.im);
        }


        /// <summary>
        /// The multiply operation of two complexnumbers
        /// </summary>
        /// <param name="left">The left side complexnumber of the multiply operation</param>
        /// <param name="right">The right side complexnumber of the multiply operation</param>
        /// <returns>The operation result</returns>
        public static ComplexDouble operator *(ComplexDouble left, ComplexDouble right)
        {
            return new ComplexDouble(left.re * right.re - left.im * right.im,
                                     left.re * right.im + left.im * right.re);
        }


        /// <summary>
        /// The multiply operation of complexnumber and double type number
        /// </summary>
        /// <param name="left">The left side complexnumber of the multiply operation</param>
        /// <param name="right">The right side double type number of the multiply operation</param>
        /// <returns>The operation result</returns>
        public static ComplexDouble operator *(ComplexDouble left, double right)
        {
            return new ComplexDouble(left.re * right, left.im * right);
        }


        /// <summary>
        /// Raise to a power
        /// </summary>
        /// <param name="z">The base number</param>
        /// <param name="power">The power exponent</param>
        /// <returns>The result of this operation</returns>
        public static ComplexDouble Pow(ComplexDouble z, int power)
        {
            if (power < 0)
            {
                throw new ArgumentException("Can only raise complex numbers to nonnegative powers");
            }
            ComplexDouble result = new ComplexDouble(1.0, 0.0);
            ComplexDouble zRaised = z;
            while (power != 0)
            {
                if ((power & 1) != 0)
                {
                    result *= zRaised;
                }
                zRaised = zRaised * zRaised;
                power >>= 1;
            }
            return result;
        }


        /// <summary>
        /// Root Of Unity
        /// </summary>
        /// <param name="rootOrder">The root order</param>
        /// <returns>The root</returns>
        public static ComplexDouble RootOfUnity(int rootOrder)
        {
            double radians = 2*Math.PI/rootOrder;
            return new ComplexDouble(Math.Cos(radians), Math.Sin(radians));
        }


        /// <summary>
        /// Returns a String that represents the current ComplexDouble
        /// </summary>
        /// <returns>A String that represents the current ComplexDouble</returns>
        public override string ToString()
        {
            if (im < 0)
            {
                return String.Format("{0} - {1}i", re, -im);
            }
            else
            {
                return String.Format("{0} + {1}i", re, im);
            }
        }
    }


    /// <summary>
    /// A ComplexDouble squences
    /// </summary>
    public class ComplexDoubleSequence
    {
        ComplexDouble[] signal;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="signal"></param>
        public ComplexDoubleSequence(ComplexDouble[] signal)
        {
            this.signal = signal;
        }

        /// <summary>
        /// The length of the ComplexDoubleSequence
        /// </summary>
        public int Length
        {
            get
            {
                return signal.Length;
            }
        }


        /// <summary>
        /// Get the ComplexDouble indexed by index
        /// </summary>
        /// <param name="index">the index</param>
        /// <returns>The ComplexDouble</returns>
        public ComplexDouble this[int index]
        {
            get
            {
                return signal[index];
            }
        }


        /// <summary>
        /// The plus operation of two ComplexDoubleSequences
        /// </summary>
        /// <param name="left">The left side ComplexDoubleSequence of the plus operation</param>
        /// <param name="right">The right side ComplexDoubleSequence of the plus operation</param>
        /// <returns>The operation result</returns>
        public static ComplexDoubleSequence operator +(ComplexDoubleSequence left, ComplexDoubleSequence right)
        {
            ComplexDouble[] result = new ComplexDouble[Math.Max(left.signal.Length, right.signal.Length)];
            int i;
            for (i = 0; i < left.signal.Length && i < right.signal.Length; i++)
            {
                result[i] = left.signal[i] + right.signal[i];
            }
            for ( ; i < left.signal.Length; i++)
            {
                result[i] = left.signal[i];
            }
            for ( ; i < right.signal.Length; i++)
            {
                result[i] = right.signal[i];
            }
            return new ComplexDoubleSequence(result);
        }


        /// <summary>
        /// For every ComplexDouble, get the square of that.
        /// </summary>
        public void PointwiseSquareInPlace()
        {
            for (int i = 0; i < signal.Length; i++)
            {
                signal[i] *= signal[i];
            }
        }


        /// <summary>
        /// Multiply every ComplexDouble of this ComplexDoubleSequence by the 
        /// one of the right side, and store the result to this ComplexDoubleSequence
        /// </summary>
        /// <param name="right">The right side of the Multiply operation</param>
        public void PointwiseMultiplyInPlace(ComplexDoubleSequence right)
        {
            if (signal.Length != right.Length)
            {
                throw new ArgumentException("Operands must be same length for pointwise multiply");
            }
            for (int i = 0; i < signal.Length; i++)
            {
                signal[i] *= right.signal[i];
            }
        }


        /// <summary>
        /// Pad the ComplexDoubleSequence
        /// </summary>
        /// <param name="length">The length of the padded ComplexDoubleSequence</param>
        public void Pad(int length)
        {
            if (length > signal.Length)
            {
                ComplexDouble[] newSignal = new ComplexDouble[length];
                Array.Copy(signal, newSignal, signal.Length);
                signal = newSignal;
            }
        }

        /// <summary>
        /// Recursive discrete Fourier transform (DFT)
        /// </summary>
        /// <returns>The result of this operation</returns>
        public ComplexDoubleSequence DftRecursive()
        {
            Debug.Assert(IsPowerOfTwo(signal.Length));
            return new ComplexDoubleSequence(
                FftRecursive(signal,
                             ComplexDouble.RootOfUnity(signal.Length),
                             ComplexDouble.RootOfUnity(-signal.Length)));
        }


        /// <summary>
        /// Inverse recursive discrete Fourier transform (DFT)
        /// </summary>
        /// <returns>The result of this operation</returns>
        public ComplexDoubleSequence InverseDftRecursive()
        {
            Debug.Assert(IsPowerOfTwo(signal.Length));
            ComplexDouble[] result = FftRecursive(signal,
                                                  ComplexDouble.RootOfUnity(-signal.Length),
                                                  ComplexDouble.RootOfUnity(signal.Length));
            for (int i = 0; i < result.Length; i++)
            {
                result[i] *= 1.0 / signal.Length;
            }
            return new ComplexDoubleSequence(result);
        }

        /// <summary>
        /// Take discrete Fourier transform in-place and scramble the array entries
        /// in a way that can be undone by InverseDftInPlaceScramble.
        /// </summary>
        public void DftInPlaceScramble()
        {
            Debug.Assert(IsPowerOfTwo(signal.Length));
            FftInPlaceScramble(ComplexDouble.RootOfUnity(signal.Length),
                               ComplexDouble.RootOfUnity(-signal.Length));
        }


        /// <summary>
        /// take the discrete Fourier transform in-place and unscramble the array entries
        /// from the order DftInPlaceScramble placed them in.
        /// </summary>
        public void InverseDftInPlaceScramble()
        {
            Debug.Assert(IsPowerOfTwo(signal.Length));
            FftInPlaceUnscramble(ComplexDouble.RootOfUnity(-signal.Length),
                                 ComplexDouble.RootOfUnity(signal.Length));
            for (int i = 0; i < signal.Length; i++)
            {
                signal[i] *= 1.0 / signal.Length;
            }
        }

        private static bool IsPowerOfTwo(int x)
        {
            while (x > 1)
            {
                if ((x & 1) != 0)
                {
                    return false;
                }
                x >>= 1;
            }
            return true;
        }

        // Assumes signal.Length = 2^D and rootUnity is a 2^Dth root of unity.
        private static ComplexDouble[] FftRecursive(
            ComplexDouble[] signal, ComplexDouble rootUnity, ComplexDouble rootUnityInverse)
        {
            if (signal.Length == 1)
            {
                ComplexDouble[] resultOne = new ComplexDouble[1];
                Array.Copy(signal, resultOne, 1);
                return resultOne;
            }

            ComplexDouble[] evenPart = new ComplexDouble[signal.Length / 2];
            ComplexDouble[] oddPart = new ComplexDouble[signal.Length / 2];
            int i;
            for (i = 0; i < signal.Length; i += 2)
            {
                evenPart[i / 2] = signal[i];
                oddPart[i / 2] = signal[i + 1];
            }
            ComplexDouble rootUnitySquared = rootUnity * rootUnity;
            ComplexDouble rootUnityInverseSquared = rootUnityInverse * rootUnityInverse;
            evenPart = FftRecursive(evenPart, rootUnitySquared, rootUnityInverseSquared);
            oddPart = FftRecursive(oddPart, rootUnitySquared, rootUnityInverseSquared);

            ComplexDouble[] result = new ComplexDouble[signal.Length];
            ComplexDouble twiddle = new ComplexDouble(1.0, 0);
            int halfLen = result.Length / 2;
            for (i = 0; i < halfLen; i++)
            {
                result[i] = evenPart[i] + twiddle * oddPart[i];
                twiddle *= rootUnityInverse;
            }
            for (; i < result.Length; i++)
            {
                result[i] = evenPart[i - halfLen] + twiddle * oddPart[i - halfLen];
                twiddle *= rootUnityInverse;
            }

            return result;
        }

        /// <summary>
        /// Uses the Gentleman-Sande algorithm (decimation-in-frequency) to
        /// simultaneously take the FFT in-place and scramble the array entries
        /// in a way that can be undone by FftInPlaceUnscramble.
        /// </summary>
        private void FftInPlaceScramble(ComplexDouble rootUnity, ComplexDouble rootUnityInverse)
        {
            ComplexDouble inverseRaised = rootUnityInverse;
            for (int m = (signal.Length >> 1); m > 0; m >>= 1)
            {
                ComplexDouble a = new ComplexDouble(1.0, 0.0);
                for (int j = 0; j < m; j++, a *= inverseRaised)
                {
                    for (int i = j; i < signal.Length; i += 2 * m)
                    {
                        ComplexDouble signal_i = signal[i];
                        signal[i] = signal_i + signal[i + m];
                        signal[i + m] = a * (signal_i - signal[i + m]);
                    }
                }
                inverseRaised *= inverseRaised;
            }
        }

        /// <summary>
        /// Uses the Cooley-Tukey algorithm (decimation-in-time) to
        /// simultaneously take the FFT in-place and unscramble the array entries
        /// from the order FftInPlaceScramble placed them in.
        /// </summary>
        private void FftInPlaceUnscramble(ComplexDouble rootUnity, ComplexDouble rootUnityInverse)
        {
            int multiplier = signal.Length / 2;
            for (int m = 1; m < signal.Length; m <<= 1)
            {
                ComplexDouble inverseRaised = ComplexDouble.Pow(rootUnityInverse, multiplier);
                ComplexDouble a = new ComplexDouble(1.0, 0.0);
                for (int j = 0; j < m; j++, a *= inverseRaised)
                {
                    for (int i = j; i < signal.Length; i += 2 * m)
                    {
                        ComplexDouble signal_i = signal[i];
                        signal[i] = signal_i + a * signal[i + m];
                        signal[i + m] = signal_i - a * signal[i + m];
                    }
                }
                multiplier /= 2;
            }
        }
    }
}

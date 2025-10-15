using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace ConsoleApp
{
    internal class UserService
    {
    }


    internal class LoginService
    {
    }


    internal class DataService
    {
    }



    internal class ProductService
    {
    }


    internal class SthService
    {
    }


    internal class SignalProcessor
    {
        private double[] samples;
        private double samplingFrequency;

        public SignalProcessor(double[] samples, double samplingFrequency)
        {
            this.samples = samples;
            this.samplingFrequency = samplingFrequency;
        }

        public double CalculateRMS(double lowFreq, double highFreq)
        {
            // Perform FFT
            var complexSamples = samples.Select(s => new Complex(s, 0)).ToArray();
            Fourier.Forward(complexSamples, FourierOptions.Default);

            // Calculate frequency resolution
            double freqResolution = samplingFrequency / samples.Length;

            // Find indices for the frequency band
            int lowIndex = (int)Math.Ceiling(lowFreq / freqResolution);
            int highIndex = (int)Math.Floor(highFreq / freqResolution);

            // Zero out components outside the band
            for (int i = 0; i < complexSamples.Length; i++)
            {
                if (i < lowIndex || i > highIndex)
                {
                    complexSamples[i] = Complex.Zero;
                }
            }

            // Perform inverse FFT
            Fourier.Inverse(complexSamples, FourierOptions.Default);

            // Extract real part and calculate RMS
            var filteredSamples = complexSamples.Select(c => c.Real).ToArray();
            double sumSquares = filteredSamples.Sum(s => s * s);
            double rms = Math.Sqrt(sumSquares / filteredSamples.Length);

            return rms;
        }
    }

}

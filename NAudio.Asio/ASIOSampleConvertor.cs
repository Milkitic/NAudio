using System;
using System.Runtime.CompilerServices;

namespace NAudio.Wave.Asio
{
    /// <summary>
    /// This class stores convertors for different interleaved WaveFormat to ASIOSampleType separate channel
    /// format.
    /// </summary>
    internal class AsioSampleConvertor
    {
        public delegate void SampleConvertor(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples);

        /// <summary>
        /// Selects the sample convertor based on the input WaveFormat and the output ASIOSampleTtype.
        /// </summary>
        /// <param name="waveFormat">The wave format.</param>
        /// <param name="asioType">The type.</param>
        /// <returns></returns>
        public static SampleConvertor SelectSampleConvertor(WaveFormat waveFormat, AsioSampleType asioType)
        {
            SampleConvertor convertor = null;
            bool is2Channels = waveFormat.Channels == 2;

            // TODO : IMPLEMENTS OTHER CONVERTOR TYPES
            switch (asioType)
            {
                case AsioSampleType.Int32LSB:
                    switch (waveFormat.BitsPerSample)
                    {
                        case 16:
                            convertor = (is2Channels) ? (SampleConvertor)ConvertorShortToInt2Channels : (SampleConvertor)ConvertorShortToIntGeneric;
                            break;
                        case 32:
                            if (waveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                                convertor = (is2Channels) ? (SampleConvertor)ConvertorFloatToInt2Channels : (SampleConvertor)ConvertorFloatToIntGeneric;
                            else
                                convertor = (is2Channels) ? (SampleConvertor)ConvertorIntToInt2Channels : (SampleConvertor)ConvertorIntToIntGeneric;
                            break;
                    }
                    break;
                case AsioSampleType.Int16LSB:
                    switch (waveFormat.BitsPerSample)
                    {
                        case 16:
                            convertor = (is2Channels) ? (SampleConvertor)ConvertorShortToShort2Channels : (SampleConvertor)ConvertorShortToShortGeneric;
                            break;
                        case 32:
                            if (waveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                                convertor = (is2Channels) ? (SampleConvertor)ConvertorFloatToShort2Channels : (SampleConvertor)ConvertorFloatToShortGeneric;
                            else
                                convertor = (is2Channels) ? (SampleConvertor)ConvertorIntToShort2Channels : (SampleConvertor)ConvertorIntToShortGeneric;
                            break;
                    }
                    break;
                case AsioSampleType.Int24LSB:
                    switch (waveFormat.BitsPerSample)
                    {
                        case 16:
                            throw new ArgumentException("Not a supported conversion");
                        case 32:
                            if (waveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                                convertor = ConverterFloatTo24LSBGeneric;
                            else
                                throw new ArgumentException("Not a supported conversion");
                            break;
                    }
                    break;
                case AsioSampleType.Float32LSB:
                    switch (waveFormat.BitsPerSample)
                    {
                        case 16:
                            throw new ArgumentException("Not a supported conversion");
                        case 32:
                            if (waveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                                convertor = ConverterFloatToFloatGeneric;
                            else
                                convertor = ConvertorIntToFloatGeneric;
                            break;
                    }
                    break;

                default:
                    throw new ArgumentException(
                        String.Format("ASIO Buffer Type {0} is not yet supported.",
                                      Enum.GetName(typeof(AsioSampleType), asioType)));
            }
            return convertor;
        }

        /// <summary>
        /// Optimized convertor for 2 channels SHORT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertorShortToInt2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.ShortToIntSampleConverter.Instance
                .Convert2Channels(inputInterleavedBuffer, asioOutputBuffers, nbSamples);
        }

        /// <summary>
        /// Generic convertor for SHORT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertorShortToIntGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.ShortToIntSampleConverter.Instance
                .ConvertGeneric(inputInterleavedBuffer, asioOutputBuffers, nbChannels, nbSamples);
        }

        /// <summary>
        /// Optimized convertor for 2 channels FLOAT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertorFloatToInt2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.FloatToIntSampleConverter.Instance
                .Convert2Channels(inputInterleavedBuffer, asioOutputBuffers, nbSamples);
        }

        /// <summary>
        /// Generic convertor Float to INT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertorFloatToIntGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.FloatToIntSampleConverter.Instance
                .ConvertGeneric(inputInterleavedBuffer, asioOutputBuffers, nbChannels, nbSamples);
        }

        /// <summary>
        /// Optimized convertor for 2 channels INT to INT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertorIntToInt2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.IntToIntSampleConverter.Instance
                .Convert2Channels(inputInterleavedBuffer, asioOutputBuffers, nbSamples);
        }

        /// <summary>
        /// Generic convertor INT to INT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertorIntToIntGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.IntToIntSampleConverter.Instance
                .ConvertGeneric(inputInterleavedBuffer, asioOutputBuffers, nbChannels, nbSamples);
        }

        /// <summary>
        /// Optimized convertor for 2 channels INT to SHORT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertorIntToShort2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.IntToShortSampleConverter.Instance
                .Convert2Channels(inputInterleavedBuffer, asioOutputBuffers, nbSamples);
        }

        /// <summary>
        /// Generic convertor INT to SHORT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertorIntToShortGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.IntToShortSampleConverter.Instance
                .ConvertGeneric(inputInterleavedBuffer, asioOutputBuffers, nbChannels, nbSamples);
        }

        /// <summary>
        /// Generic convertor INT to FLOAT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertorIntToFloatGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.IntToFloatSampleConverter.Instance
                .ConvertGeneric(inputInterleavedBuffer, asioOutputBuffers, nbChannels, nbSamples);
        }

        /// <summary>
        /// Optimized convertor for 2 channels SHORT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertorShortToShort2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.ShortToShortSampleConverter.Instance
                .Convert2Channels(inputInterleavedBuffer, asioOutputBuffers, nbSamples);
        }

        /// <summary>
        /// Generic convertor for SHORT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertorShortToShortGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.ShortToShortSampleConverter.Instance
                .ConvertGeneric(inputInterleavedBuffer, asioOutputBuffers, nbChannels, nbSamples);
        }

        /// <summary>
        /// Optimized convertor for 2 channels FLOAT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertorFloatToShort2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.FloatToShortSampleConverter.Instance
                .Convert2Channels(inputInterleavedBuffer, asioOutputBuffers, nbSamples);
        }

        /// <summary>
        /// Generic convertor SHORT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertorFloatToShortGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.FloatToShortSampleConverter.Instance
                .ConvertGeneric(inputInterleavedBuffer, asioOutputBuffers, nbChannels, nbSamples);
        }

        /// <summary>
        /// Generic converter 24 LSB
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConverterFloatTo24LSBGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.FloatTo24LSBSampleConverter.Instance
                .ConvertGeneric(inputInterleavedBuffer, asioOutputBuffers, nbChannels, nbSamples);
        }

        /// <summary>
        /// Generic convertor for float
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConverterFloatToFloatGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers, int nbChannels, int nbSamples)
        {
            NAudio.Asio.SampleConverters.FloatToFloatSampleConverter.Instance
                .ConvertGeneric(inputInterleavedBuffer, asioOutputBuffers, nbChannels, nbSamples);
        }
    }
}

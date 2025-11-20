using System;

namespace NAudio.Asio.SampleConverters;

public sealed class FloatTo24LSBSampleConverter : SampleConverterBase
{
    private FloatTo24LSBSampleConverter()
    {
    }

    public static FloatTo24LSBSampleConverter Instance { get; } = new();

    public override unsafe void Convert2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers,
        int nbSamples)
    {
        float* inputSamples = (float*)inputInterleavedBuffer;
        byte* left = (byte*)asioOutputBuffers[0];
        byte* right = (byte*)asioOutputBuffers[1];

        for (int i = 0; i < nbSamples; i++)
        {
            int sampleL = clampTo24Bit(inputSamples[0]);
            int sampleR = clampTo24Bit(inputSamples[1]);

            *left++ = (byte)(sampleL);
            *left++ = (byte)(sampleL >> 8);
            *left++ = (byte)(sampleL >> 16);

            *right++ = (byte)(sampleR);
            *right++ = (byte)(sampleR >> 8);
            *right++ = (byte)(sampleR >> 16);

            inputSamples += 2;
        }
    }

    public override unsafe void ConvertGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers,
        int nbChannels, int nbSamples)
    {
        float* inputSamples = (float*)inputInterleavedBuffer;

        byte*[] samples = new byte*[nbChannels];
        for (int i = 0; i < nbChannels; i++)
        {
            samples[i] = (byte*)asioOutputBuffers[i];
        }

        for (int i = 0; i < nbSamples; i++)
        {
            for (int j = 0; j < nbChannels; j++)
            {
                int sample24 = clampTo24Bit(*inputSamples++);
                *(samples[j]++) = (byte)(sample24);
                *(samples[j]++) = (byte)(sample24 >> 8);
                *(samples[j]++) = (byte)(sample24 >> 16);
            }
        }
    }
}
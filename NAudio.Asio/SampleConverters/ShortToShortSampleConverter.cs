using System;

namespace NAudio.Asio.SampleConverters;

public sealed class ShortToShortSampleConverter : SampleConverterBase
{
    private ShortToShortSampleConverter()
    {
    }

    public static ShortToShortSampleConverter Instance { get; } = new();

    public override unsafe void Convert2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers,
        int nbSamples)
    {
        short* inputSamples = (short*)inputInterleavedBuffer;
        short* leftSamples = (short*)asioOutputBuffers[0];
        short* rightSamples = (short*)asioOutputBuffers[1];

        int i = 0;

        const int vectorSize = 4; // 每次处理4个音频帧
        while (i <= nbSamples - vectorSize)
        {
            leftSamples[0] = inputSamples[0];
            rightSamples[0] = inputSamples[1];
            leftSamples[1] = inputSamples[2];
            rightSamples[1] = inputSamples[3];
            leftSamples[2] = inputSamples[4];
            rightSamples[2] = inputSamples[5];
            leftSamples[3] = inputSamples[6];
            rightSamples[3] = inputSamples[7];

            inputSamples += (vectorSize * 2);
            leftSamples += vectorSize;
            rightSamples += vectorSize;
            i += vectorSize;
        }

        for (; i < nbSamples; i++)
        {
            *leftSamples++ = inputSamples[0];
            *rightSamples++ = inputSamples[1];
            inputSamples += 2;
        }
    }

    public override unsafe void ConvertGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers,
        int nbChannels, int nbSamples)
    {
        short* inputSamples = (short*)inputInterleavedBuffer;
        short*[] samples = new short*[nbChannels];
        for (int i = 0; i < nbChannels; i++)
        {
            samples[i] = (short*)asioOutputBuffers[i];
        }

        for (int i = 0; i < nbSamples; i++)
        {
            for (int j = 0; j < nbChannels; j++)
            {
                *samples[j]++ = *inputSamples++;
            }
        }
    }
}
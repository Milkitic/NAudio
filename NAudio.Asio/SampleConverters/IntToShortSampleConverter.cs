using System;

namespace NAudio.Asio.SampleConverters;

public sealed class IntToShortSampleConverter : SampleConverterBase
{
    private IntToShortSampleConverter()
    {
    }

    public static IntToShortSampleConverter Instance { get; } = new();

    public override unsafe void Convert2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers,
        int nbSamples)
    {
        int* inputSamples = (int*)inputInterleavedBuffer;
        short* leftSamples = (short*)asioOutputBuffers[0];
        short* rightSamples = (short*)asioOutputBuffers[1];

        int i = 0;

        const int vectorSize = 4; // 每次处理4个音频帧
        while (i <= nbSamples - vectorSize)
        {
            leftSamples[0] = (short)(inputSamples[0] / (1 << 16));
            rightSamples[0] = (short)(inputSamples[1] / (1 << 16));
            leftSamples[1] = (short)(inputSamples[2] / (1 << 16));
            rightSamples[1] = (short)(inputSamples[3] / (1 << 16));
            leftSamples[2] = (short)(inputSamples[4] / (1 << 16));
            rightSamples[2] = (short)(inputSamples[5] / (1 << 16));
            leftSamples[3] = (short)(inputSamples[6] / (1 << 16));
            rightSamples[3] = (short)(inputSamples[7] / (1 << 16));

            inputSamples += (vectorSize * 2);
            leftSamples += vectorSize;
            rightSamples += vectorSize;
            i += vectorSize;
        }

        for (; i < nbSamples; i++)
        {
            *leftSamples++ = (short)(inputSamples[0] / (1 << 16));
            *rightSamples++ = (short)(inputSamples[1] / (1 << 16));
            inputSamples += 2;
        }
    }

    public override unsafe void ConvertGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers,
        int nbChannels, int nbSamples)
    {
        int* inputSamples = (int*)inputInterleavedBuffer;
        int** samples = stackalloc int*[nbChannels];
        for (int i = 0; i < nbChannels; i++)
        {
            samples[i] = (int*)asioOutputBuffers[i];
        }

        for (int i = 0; i < nbSamples; i++)
        {
            for (int j = 0; j < nbChannels; j++)
            {
                *samples[j]++ = (short)(*inputSamples++ / (1 << 16));
            }
        }
    }
}
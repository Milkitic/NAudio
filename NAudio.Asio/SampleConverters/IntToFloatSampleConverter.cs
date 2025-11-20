using System;

namespace NAudio.Asio.SampleConverters;

public sealed class IntToFloatSampleConverter : SampleConverterBase
{
    private IntToFloatSampleConverter()
    {
    }

    public static IntToFloatSampleConverter Instance { get; } = new();

    public override unsafe void Convert2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers,
        int nbSamples)
    {
        int* inputSamples = (int*)inputInterleavedBuffer;
        float* leftSamples = (float*)asioOutputBuffers[0];
        float* rightSamples = (float*)asioOutputBuffers[1];

        int i = 0;

        const int vectorSize = 4; // 每次处理4个音频帧
        while (i <= nbSamples - vectorSize)
        {
            leftSamples[0] = inputSamples[0] / (1 << (32 - 1));
            rightSamples[0] = inputSamples[1] / (1 << (32 - 1));
            leftSamples[1] = inputSamples[2] / (1 << (32 - 1));
            rightSamples[1] = inputSamples[3] / (1 << (32 - 1));
            leftSamples[2] = inputSamples[4] / (1 << (32 - 1));
            rightSamples[2] = inputSamples[5] / (1 << (32 - 1));
            leftSamples[3] = inputSamples[6] / (1 << (32 - 1));
            rightSamples[3] = inputSamples[7] / (1 << (32 - 1));

            inputSamples += (vectorSize * 2);
            leftSamples += vectorSize;
            rightSamples += vectorSize;
            i += vectorSize;
        }

        for (; i < nbSamples; i++)
        {
            *leftSamples++ = inputSamples[0] / (1 << (32 - 1));
            *rightSamples++ = inputSamples[1] / (1 << (32 - 1));
            inputSamples += 2;
        }
    }

    public override unsafe void ConvertGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers,
        int nbChannels, int nbSamples)
    {
        int* inputSamples = (int*)inputInterleavedBuffer;
        float*[] samples = new float*[nbChannels];
        for (int i = 0; i < nbChannels; i++)
        {
            samples[i] = (float*)asioOutputBuffers[i];
        }

        for (int i = 0; i < nbSamples; i++)
        {
            for (int j = 0; j < nbChannels; j++)
            {
                *samples[j]++ = *inputSamples++ / (1 << (32 - 1));
            }
        }
    }
}
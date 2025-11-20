using System;

namespace NAudio.Asio.SampleConverters;

public sealed class ShortToIntSampleConverter : SampleConverterBase
{
    private ShortToIntSampleConverter()
    {
    }

    public static ShortToIntSampleConverter Instance { get; } = new();

    public override unsafe void Convert2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers,
        int nbSamples)
    {
        short* inputSamples = (short*)inputInterleavedBuffer;
        // 使用指针技巧：通过 short* 写入 int 的高16位，避免实际的 16->32 位转换
        short* leftSamples = (short*)asioOutputBuffers[0];
        short* rightSamples = (short*)asioOutputBuffers[1];

        // 指向32位整数的高16位
        leftSamples++;
        rightSamples++;

        int i = 0;

        const int vectorSize = 4; // 每次处理4个音频帧
        while (i <= nbSamples - vectorSize)
        {
            // 逐帧复制到高16位（保持与标量实现相同的流程与结果）
            leftSamples[0] = inputSamples[0];
            rightSamples[0] = inputSamples[1];
            leftSamples[2] = inputSamples[2];
            rightSamples[2] = inputSamples[3];
            leftSamples[4] = inputSamples[4];
            rightSamples[4] = inputSamples[5];
            leftSamples[6] = inputSamples[6];
            rightSamples[6] = inputSamples[7];

            inputSamples += (vectorSize * 2);
            leftSamples += (vectorSize * 2);
            rightSamples += (vectorSize * 2);
            i += vectorSize;
        }

        for (; i < nbSamples; i++)
        {
            *leftSamples = inputSamples[0];
            *rightSamples = inputSamples[1];
            inputSamples += 2;
            leftSamples += 2; // 跳过低16位
            rightSamples += 2;
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
            // 指向32位整数的高16位
            samples[i]++;
        }

        for (int i = 0; i < nbSamples; i++)
        {
            for (int j = 0; j < nbChannels; j++)
            {
                *samples[j] = *inputSamples++;
                samples[j] += 2; // 跳过低16位
            }
        }
    }
}
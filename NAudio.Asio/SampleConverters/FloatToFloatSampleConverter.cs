using System;
using System.Runtime.Intrinsics;

namespace NAudio.Asio.SampleConverters;

public sealed class FloatToFloatSampleConverter : SampleConverterBase
{
    private static readonly Vector256<int> VShuffleMask;

    static FloatToFloatSampleConverter()
    {
        if (!Vector256.IsHardwareAccelerated) return;

        // 重排掩码
        // 输入: [L0, R0, L1, R1, L2, R2, L3, R3]
        // 目标: [L0, L1, L2, L3, R0, R1, R2, R3]
        // 索引: 0, 2, 4, 6 (放低位), 1, 3, 5, 7 (放高位)
        VShuffleMask = Vector256.Create(0, 2, 4, 6, 1, 3, 5, 7);
    }

    private FloatToFloatSampleConverter()
    {
    }

    public static FloatToFloatSampleConverter Instance { get; } = new();

    public override unsafe void Convert2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers,
        int nbSamples)
    {
        float* inputSamples = (float*)inputInterleavedBuffer;
        float* leftSamples = (float*)asioOutputBuffers[0];
        float* rightSamples = (float*)asioOutputBuffers[1];

        int i = 0;

        if (Vector256.IsHardwareAccelerated)
        {
            const int vectorSize = 4; // 每次处理4个音频帧

            while (i <= nbSamples - vectorSize)
            {
                // 加载 8 个 float (包含 4个左, 4个右)
                Vector256<float> vSrc = Vector256.Load(inputSamples);

                // 重排，将 L 和 R 分离到同一个寄存器的两端
                // [L0, L1, L2, L3 | R0, R1, R2, R3]
                Vector256<float> vOrdered = Vector256.Shuffle(vSrc, VShuffleMask);

                // 写入
                vOrdered.GetLower().Store(leftSamples); // 低128位 (L0-L3) 写入左声道
                vOrdered.GetUpper().Store(rightSamples); // 高128位 (R0-R3) 写入右声道

                inputSamples += (vectorSize * 2);
                leftSamples += vectorSize;
                rightSamples += vectorSize;
                i += vectorSize;
            }
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
        float* inputSamples = (float*)inputInterleavedBuffer;
        float** samples = stackalloc float*[nbChannels];
        for (int i = 0; i < nbChannels; i++)
        {
            samples[i] = (float*)asioOutputBuffers[i];
        }

        for (int i = 0; i < nbSamples; i++)
        {
            for (int j = 0; j < nbChannels; j++)
            {
                *(samples[j]++) = *inputSamples++;
            }
        }
    }
}
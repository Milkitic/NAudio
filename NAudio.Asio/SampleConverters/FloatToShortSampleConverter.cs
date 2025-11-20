using System;
using System.Runtime.Intrinsics;

namespace NAudio.Asio.SampleConverters;

public sealed class FloatToShortSampleConverter : SampleConverterBase
{
    private static readonly Vector256<float> VScale;
    private static readonly Vector256<int> VShuffleMask;

    static FloatToShortSampleConverter()
    {
        if (!Vector256.IsHardwareAccelerated) return;

        // 缩放因子: 32767.0f
        VScale = Vector256.Create(32767.0f);
        // 重排掩码
        // 输入: [L0, R0, L1, R1, L2, R2, L3, R3]
        // 目标: [L0, L1, L2, L3, R0, R1, R2, R3]
        // 索引: 0, 2, 4, 6 (放低位), 1, 3, 5, 7 (放高位)
        VShuffleMask = Vector256.Create(0, 2, 4, 6, 1, 3, 5, 7);
    }

    private FloatToShortSampleConverter()
    {
    }

    public static FloatToShortSampleConverter Instance { get; } = new();

    public override unsafe void Convert2Channels(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers,
        int nbSamples)
    {
        float* inputSamples = (float*)inputInterleavedBuffer;
        short* leftSamples = (short*)asioOutputBuffers[0];
        short* rightSamples = (short*)asioOutputBuffers[1];

        int i = 0;

        if (Vector256.IsHardwareAccelerated)
        {
            const int vectorSize = 4; // 每次处理4个音频帧

            while (i <= nbSamples - vectorSize)
            {
                // 加载 8 个 float (包含 4个左, 4个右)
                Vector256<float> vSrc = Vector256.Load(inputSamples);

                // Clamp (Min/Max)
                Vector256<float> vClamped = Vector256.Min(Vector256.Max(vSrc, VMin), VMax);

                // 缩放到 short 的量化范围
                Vector256<int> vInts = Vector256.ConvertToInt32(vClamped * VScale);

                // 重排，将 L 和 R 分离到同一个寄存器的两端
                // [L0, L1, L2, L3 | R0, R1, R2, R3]
                Vector256<int> vOrdered = Vector256.Shuffle(vInts, VShuffleMask);

                // 写入（按元素写入，保持与标量路径一致的舍入/截断行为）
                var l = vOrdered.GetLower();
                var r = vOrdered.GetUpper();
                leftSamples[0] = (short)l.GetElement(0);
                leftSamples[1] = (short)l.GetElement(1);
                leftSamples[2] = (short)l.GetElement(2);
                leftSamples[3] = (short)l.GetElement(3);
                rightSamples[0] = (short)r.GetElement(0);
                rightSamples[1] = (short)r.GetElement(1);
                rightSamples[2] = (short)r.GetElement(2);
                rightSamples[3] = (short)r.GetElement(3);

                inputSamples += (vectorSize * 2);
                leftSamples += vectorSize;
                rightSamples += vectorSize;
                i += vectorSize;
            }
        }

        for (; i < nbSamples; i++)
        {
            *leftSamples++ = clampToShort(inputSamples[0]);
            *rightSamples++ = clampToShort(inputSamples[1]);
            inputSamples += 2;
        }
    }

    public override unsafe void ConvertGeneric(IntPtr inputInterleavedBuffer, IntPtr[] asioOutputBuffers,
        int nbChannels, int nbSamples)
    {
        float* inputSamples = (float*)inputInterleavedBuffer;
        // Use a trick (short instead of int to avoid any convertion from 16Bit to 32Bit)
        short*[] samples = new short*[nbChannels];
        for (int i = 0; i < nbChannels; i++)
        {
            samples[i] = (short*)asioOutputBuffers[i];
        }

        for (int i = 0; i < nbSamples; i++)
        {
            for (int j = 0; j < nbChannels; j++)
            {
                *(samples[j]++) = clampToShort(*inputSamples++);
            }
        }
    }
}
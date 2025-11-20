using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace NAudio.Asio.SampleConverters;

public abstract class SampleConverter
{
    protected static readonly Vector256<float> VMin;
    protected static readonly Vector256<float> VMax;

    static SampleConverter()
    {
        if (!Vector256.IsHardwareAccelerated) return;
        VMin = Vector256.Create(-1.0f);
        VMax = Vector256.Create(1.0f);
    }

    public abstract void Convert2Channels(
        IntPtr inputInterleavedBuffer,
        IntPtr[] asioOutputBuffers,
        int nbSamples);

    public abstract void ConvertGeneric(
        IntPtr inputInterleavedBuffer,
        IntPtr[] asioOutputBuffers,
        int nbChannels,
        int nbSamples);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int clampTo24Bit(double sampleValue)
    {
        sampleValue = Math.Clamp(sampleValue, -1.0, 1.0);
        return (int)(sampleValue * 8388607.0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int clampToInt(double sampleValue)
    {
        sampleValue = Math.Clamp(sampleValue, -1.0, 1.0);
        return (int)(sampleValue * 2147483647.0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short clampToShort(double sampleValue)
    {
        sampleValue = Math.Clamp(sampleValue, -1.0, 1.0);
        return (short)(sampleValue * 32767.0);
    }
}
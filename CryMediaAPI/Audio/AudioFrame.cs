﻿using System;
using System.IO;
using CryMediaAPI.BaseClasses;

namespace CryMediaAPI.Audio
{
    /// <summary>
    /// Audio frame containing multiple audio samples in signed PCM format with given bit depth.
    /// </summary>
    public class AudioFrame : IDisposable, IMediaFrame
    {     
        int size, offset = 0;

        public int Channels { get; }
        public int SampleCount { get; }
        public int BytesPerSample { get; }
        public int LoadedSamples { get; private set; }


        byte[] frameBuffer;
        public Memory<byte> RawData { get; }

        /// <summary>
        /// Creates an empty audio frame with fixed sample count and given bit depth using signed PCM format.
        /// </summary>
        /// <param name="bitDepth">Bits per sample (16, 24 or 32)</param>
        /// <param name="channels">Number of channels</param>
        /// <param name="sampleCount">Number of samples to store within this frame</param>
        public AudioFrame(int channels, int sampleCount = 1024, int bitDepth = 16)
        {
            if (bitDepth != 16 && bitDepth != 24 && bitDepth != 32) throw new InvalidOperationException("Acceptable bit depths are 16, 24 and 32");
            if (channels <= 0) throw new InvalidDataException("Channel count has to be bigger than 0!");
            if (sampleCount <= 0) throw new InvalidDataException("Sample count has to be bigger than 0!");

            this.Channels = channels;
            this.SampleCount = sampleCount;
            this.BytesPerSample = bitDepth / 8;
            size = sampleCount * channels * BytesPerSample;

            frameBuffer = new byte[size];
            RawData = frameBuffer.AsMemory();
        }

        /// <summary>
        /// Loads audio samples from stream.
        /// </summary>
        /// <param name="str">Stream containing raw audio samples in signed PCM format</param>
        public bool Load(Stream str)
        {
            offset = 0;

            while (offset < size)
            {
                var r = str.Read(frameBuffer, offset, size - offset);
                if (r <= 0) return false;
                offset += r;
            }

            LoadedSamples = offset / (BytesPerSample * Channels);
            return true;
        }

        /// <summary>
        /// Returns part of memory that contains the sample value
        /// </summary>
        /// <param name="index">Sample index</param>
        /// <param name="channel">Channel index</param>
        public Memory<byte> GetSample(int index, int channel) 
        {
            int i = (index * Channels  + channel) * BytesPerSample;
            return RawData.Slice(i, BytesPerSample);
        }

        public void Dispose()
        {
            frameBuffer = null;
        }
    }
}

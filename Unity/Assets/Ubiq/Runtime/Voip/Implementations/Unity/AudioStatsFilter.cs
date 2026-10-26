#if UNITY_WEBRTC || UNITY_WEBRTC_UBIQ_FORK
using System;
using System.Buffers;
using UnityEngine;
using System.Collections.Concurrent;

namespace Ubiq.Voip.Implementations.Unity
{
    /// <summary>
    /// Gather audio stats and samples from the attached AudioSource
    /// </summary>
    public class AudioStatsFilter : MonoBehaviour
    {
        private struct Buffer
        {
            public float[] samples; // Rented from ArrayPool, may be oversized
            public int length;
            public int channels;
        }

        private ConcurrentQueue<Buffer> bufferQueue = new ConcurrentQueue<Buffer>();
        private int sampleRate;
        private Action<AudioStats> statsPushed;
        private AudioSamplesHandler samplesPushed;

        private void OnDestroy()
        {
            statsPushed = null;
            samplesPushed = null;
        }

        private void OnEnable()
        {
            OnAudioConfigurationChanged(false);
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
        }

        private void OnDisable()
        {
            AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
        }

        private void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            sampleRate = AudioSettings.outputSampleRate;
        }

        private void Update()
        {
            while (bufferQueue.TryDequeue(out var buffer))
            {
                var samples = new ReadOnlySpan<float>(buffer.samples,0,buffer.length);
                var volumeSum = 0.0f;
                for (int i = 0; i < samples.Length; i+=buffer.channels)
                {
                    volumeSum += Mathf.Abs(samples[i]);
                }

                statsPushed?.Invoke(new AudioStats(
                    samples.Length/buffer.channels,volumeSum/buffer.channels,sampleRate
                ));
                samplesPushed?.Invoke(samples,buffer.channels,sampleRate);

                ArrayPool<float>.Shared.Return(buffer.samples);
            }
        }

        public void SetCallbacks(Action<AudioStats> statsPushed,
            AudioSamplesHandler samplesPushed)
        {
            this.statsPushed = statsPushed;
            this.samplesPushed = samplesPushed;
        }

        /// <summary>
        /// </summary>
        /// <note>
        /// Called on the audio thread, not the main thread.
        /// </note>
        /// <param name="data"></param>
        /// <param name="channels"></param>
        private void OnAudioFilterRead(float[] data, int channels)
        {
            var samples = ArrayPool<float>.Shared.Rent(data.Length);
            Array.Copy(data,samples,data.Length);
            bufferQueue.Enqueue(new Buffer
            {
                samples = samples,
                length = data.Length,
                channels = channels
            });
        }
    }
}
#endif

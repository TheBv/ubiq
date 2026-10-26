using System;
using UnityEngine;

namespace Ubiq.Voip.Implementations.Web
{
    /// <summary>
    /// Pushes samples from the browser microphone shared by all peer
    /// connections. Once started, the microphone stays on.
    /// </summary>
    public class PeerConnectionMicrophone : MonoBehaviour, IMicrophoneImpl
    {
        public event AudioSamplesHandler samplesPushed;

        private float[] buffer = new float[4096];

        private void Awake()
        {
            PeerConnectionImpl.JS_WebRTC_StartMicrophone();

            // Discard samples queued before anyone was listening
            while (PeerConnectionImpl.JS_WebRTC_GetRecordSamples(buffer,buffer.Length) > 0) {}
        }

        private void Update()
        {
            PeerConnectionImpl.JS_WebRTC_ResumeAudioContext();

            var sampleRate = PeerConnectionImpl.JS_WebRTC_GetRecordStatsSampleRate();
            int count;
            while ((count = PeerConnectionImpl.JS_WebRTC_GetRecordSamples(buffer,buffer.Length)) > 0)
            {
                samplesPushed?.Invoke(new ReadOnlySpan<float>(buffer,0,count),1,sampleRate);
            }
        }

        /// <summary>
        /// Get the microphone shared by all peer connections under manager,
        /// adding it if needed.
        /// </summary>
        public static PeerConnectionMicrophone Require(GameObject manager)
        {
            var microphone = manager.GetComponent<PeerConnectionMicrophone>();
            if (!microphone)
            {
                microphone = manager.AddComponent<PeerConnectionMicrophone>();
            }
            return microphone;
        }
    }
}

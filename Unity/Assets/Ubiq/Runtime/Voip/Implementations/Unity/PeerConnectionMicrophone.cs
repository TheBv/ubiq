#if UNITY_WEBRTC || UNITY_WEBRTC_UBIQ_FORK
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace Ubiq.Voip.Implementations.Unity
{
    public class PeerConnectionMicrophone : MonoBehaviour, IMicrophoneImpl
    {
        public enum State
        {
            Idle,
            Starting,
            Running
        }

        public State state
        {
            get
            {
                if (!audioSource || audioSource.clip == null)
                {
                    return State.Idle;
                }
                if (audioSource.isPlaying)
                {
                    return State.Running;
                }
                return State.Starting;
            }
        }

        public AudioStreamTrack audioStreamTrack { get; private set; }
        public event Action<AudioStats> statsPushed;
        public event AudioSamplesHandler samplesPushed;

        private AudioSource audioSource;
        private List<GameObject> users = new List<GameObject>();
        private bool inUse => users.Count > 0 || samplesPushed != null;
        private bool microphoneAuthorized;
        private AudioStatsFilter statsFilter;

        private void Awake()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
#endif
        }

        private void Update()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Wait for microphone permissions before processing any audio
            if (!microphoneAuthorized)
            {
                microphoneAuthorized = Permission.HasUserAuthorizedPermission(Permission.Microphone);

                if (!microphoneAuthorized)
                {
                    return;
                }
            }
#endif

            if (state == State.Idle && inUse)
            {
                RequireAudioSource();
                audioSource.clip = Microphone.Start("",true,1,16000);
            }

            if (state == State.Starting)
            {
                if (Microphone.GetPosition("") > audioSource.clip.frequency / 8.0f)
                {
                    audioSource.loop = true;
                    audioSource.Play();
                    audioStreamTrack = new AudioStreamTrack(audioSource);
                }
            }

            if (state == State.Running && !inUse)
            {
                audioSource.Stop();
                Microphone.End("");
                Destroy(audioSource.clip);
                audioSource.clip = null;
                audioStreamTrack.Dispose();
                audioStreamTrack = null;
            }
        }

        private void StatsFilter_StatsPushed(AudioStats stats)
        {
            statsPushed?.Invoke(stats);
        }

        private void StatsFilter_SamplesPushed(ReadOnlySpan<float> samples,
            int channels, int sampleRate)
        {
            samplesPushed?.Invoke(samples,channels,sampleRate);
        }

        private void RequireAudioSource()
        {
            if(!audioSource)
            {
                audioSource = GetComponent<AudioSource>();

                if (!audioSource)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    statsFilter = gameObject.AddComponent<AudioStatsFilter>();
                    statsFilter.hideFlags = HideFlags.HideInInspector;
                    statsFilter.SetCallbacks(StatsFilter_StatsPushed,StatsFilter_SamplesPushed);
                }
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

        /// <summary>
        /// Indicate a new user to the microphone. The microphone is ready to
        /// be used once state is Running.
        /// </summary>
        public void AddUser(GameObject user)
        {
            if (!users.Contains(user))
            {
                users.Add(user);
            }
        }

        /// <summary>
        /// Remove a user from the microphone. If user count reaches zero, the
        /// microphone will be stopped.
        /// </summary>
        public void RemoveUser(GameObject user)
        {
            users.Remove(user);
        }
    }
}
#endif
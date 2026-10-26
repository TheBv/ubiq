using UnityEngine;

namespace Ubiq.Voip.Implementations
{
    public static class PeerConnectionImplFactory
    {
        public static IPeerConnectionImpl Create()
        {
#if UNITY_WEBRTC || UNITY_WEBRTC_UBIQ_FORK
            return new Ubiq.Voip.Implementations.Unity.PeerConnectionImpl();
#else
            return new Ubiq.Voip.Implementations.NullPeerConnectionImpl();
#endif
        }

        public static IMicrophoneImpl RequireMicrophone(GameObject manager)
        {
#if UNITY_WEBRTC || UNITY_WEBRTC_UBIQ_FORK
            return Ubiq.Voip.Implementations.Unity.PeerConnectionMicrophone.Require(manager);
#else
            return new Ubiq.Voip.Implementations.NullMicrophoneImpl();
#endif
        }
    }
}

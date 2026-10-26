using UnityEngine;

namespace Ubiq.Voip.Implementations
{
    public static class PeerConnectionImplFactory
    {
        public static IPeerConnectionImpl Create()
        {
            return new Ubiq.Voip.Implementations.Web.PeerConnectionImpl();
        }

        public static IMicrophoneImpl RequireMicrophone(GameObject manager)
        {
            return Ubiq.Voip.Implementations.Web.PeerConnectionMicrophone.Require(manager);
        }
    }
}

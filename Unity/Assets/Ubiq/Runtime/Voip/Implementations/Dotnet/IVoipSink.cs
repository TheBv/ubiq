using System;
using UnityEngine;

namespace Ubiq.Voip.Implementations.Dotnet
{
    public interface IVoipSink : SIPSorceryMedia.Abstractions.IAudioSink
    {
        event Action<AudioStats> statsPushed;
        event AudioSamplesHandler samplesPushed;
        void UpdateSpatialization(Vector3 sourcePosition, Quaternion sourceRotation,
            Vector3 listenerPosition, Quaternion listenerRotation);
    }
}
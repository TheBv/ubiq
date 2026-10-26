using System;

namespace Ubiq.Voip.Implementations.Dotnet
{
    public interface IVoipSource : SIPSorceryMedia.Abstractions.IAudioSource, IMicrophoneImpl
    {
        event Action<AudioStats> statsPushed;
    }
}
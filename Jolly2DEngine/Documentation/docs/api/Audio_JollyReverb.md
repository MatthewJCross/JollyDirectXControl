# `JollyReverb`

Reverb effect with presets and adjustable decay, damping, pre-delay and wet mix.

Namespace: `Jolly2DEngine.Audio`.

## Public API

```csharp
public sealed class JollyReverb
public bool Enabled { get; set; }
public float DecayTime { get; set; }
public float Damping { get; set; }
public float PreDelayMilliseconds { get; set; }
public float WetMix { get; set; }
public float DryMix { get; }
public JollyReverbPreset Preset { get; set; }
public JollyReverb(int sampleRate, int channels)
public void Process(Span<float> buffer)
public void Clear()
```

## Usage notes

See [Audio subsystem guide](../AUDIO_SUBSYSTEM_GUIDE.md) for setup, processing order, validation rules, and examples. This page summarises the API surface; consult the source for implementation details.

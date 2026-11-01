# `JollyPitchShifter`

Pitch shifting controls used by JollyAudioEffects.

Namespace: `Jolly2DEngine.Audio`.

## Public API

```csharp
public sealed class JollyPitchShifter
public bool Enabled { get; set; }
public float PitchFactor { get; set; }
public float PitchSemitones { get; set; }
public float PitchCents { get; set; }
public int LatencySamples { get; }
public JollyPitchShifter(int channels, int windowSize = 2048)
public float ProcessSample(float input, int channel)
public void Reset()
```

## Usage notes

See [Audio subsystem guide](../AUDIO_SUBSYSTEM_GUIDE.md) for setup, processing order, validation rules, and examples. This page summarises the API surface; consult the source for implementation details.

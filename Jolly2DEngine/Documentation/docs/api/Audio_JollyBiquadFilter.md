# `JollyBiquadFilter`

Lower-level biquad filter used by the equaliser.

Namespace: `Jolly2DEngine.Audio`.

## Public API

```csharp
public enum JollyBiquadFilterType
public sealed class JollyBiquadFilter
public float ProcessSample(float input)
public void Reset()
public void SetLowPass(float sampleRate, float frequency, float q = 0.7071f)
public void SetHighPass(float sampleRate, float frequency, float q = 0.7071f)
public void SetPeaking(float sampleRate, float frequency, float gainDb, float q = 1.0f)
public void SetLowShelf(float sampleRate, float frequency, float gainDb)
public void SetHighShelf(float sampleRate, float frequency, float gainDb)
```

## Usage notes

See [Audio subsystem guide](../AUDIO_SUBSYSTEM_GUIDE.md) for setup, processing order, validation rules, and examples. This page summarises the API surface; consult the source for implementation details.

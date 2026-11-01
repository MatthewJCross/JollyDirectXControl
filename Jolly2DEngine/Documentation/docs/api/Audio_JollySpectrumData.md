# `JollySpectrumData`

Immutable-property snapshot of a spectrum calculation; the array values are exposed for reading/visualisation.

Namespace: `Jolly2DEngine.Audio`.

## Public API

```csharp
public sealed class JollySpectrumData
public int SampleRate { get; }
public int FftSize { get; }
public float[] Magnitudes { get; }
public float[] Frequencies { get; }
public float Rms { get; }
public float Peak { get; }
```

## Usage notes

See [Audio subsystem guide](../AUDIO_SUBSYSTEM_GUIDE.md) for setup, processing order, validation rules, and examples. This page summarises the API surface; consult the source for implementation details.

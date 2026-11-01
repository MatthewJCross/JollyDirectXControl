# `JollyAnalyserSampleProvider`

NAudio sample-provider adapter that feeds samples to the analyser while forwarding the stream.

Namespace: `Jolly2DEngine.Audio`.

## Public API

```csharp
public sealed class JollyAnalyserSampleProvider : ISampleProvider
public WaveFormat WaveFormat { get; }
public float LeftLevel { get; }
public float RightLevel { get; }
```

## Usage notes

See [Audio subsystem guide](../AUDIO_SUBSYSTEM_GUIDE.md) for setup, processing order, validation rules, and examples. This page summarises the API surface; consult the source for implementation details.

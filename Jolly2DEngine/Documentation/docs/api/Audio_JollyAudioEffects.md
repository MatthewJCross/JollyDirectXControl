# `JollyAudioEffects`

Coordinates the sample-provider effects chain and exposes each effect plus the analyser.

Namespace: `Jolly2DEngine.Audio`.

## Public API

```csharp
public sealed class JollyAudioEffects : IDisposable
public JollyEqualiser Equaliser { get; }
public JollyPitchShifter PitchShifter { get; }
public JollyEcho Echo { get; }
public JollyChorus Chorus { get; }
public JollyReverb Reverb { get; }
public JollyAudioAnalyser Analyser { get; }
public JollyAnalyserSampleProvider? AnalyserProvider { get; private set; }
public JollyAudioEffects(int sampleRate, int channels, int fftSize = 2048, int pitchWindowSize = 2048)
public ISampleProvider CreateSampleProvider(ISampleProvider source)
public void Dispose()
```

## Usage notes

See [Audio subsystem guide](../AUDIO_SUBSYSTEM_GUIDE.md) for setup, processing order, validation rules, and examples. This page summarises the API surface; consult the source for implementation details.

# `JollyAudioAnalyser`

Background FFT/spectrum analysis, level measurements and beat/BPM estimates.

Namespace: `Jolly2DEngine.Audio`.

## Public API

```csharp
public sealed class JollyAudioAnalyser : IDisposable
public bool Enabled { get; set; }
public JollyFourierWindow Window { get; set; }
public int FftSize { get; }
public int SampleRate { get; }
public float Bpm { get; }
public float BeatConfidence { get; }
public bool IsBeat { get; }
public JollyAudioAnalyser(int sampleRate, int fftSize = 2048)
public void AddSample(float sample)
public JollySpectrumData? GetSpectrum()
public void Dispose()
```

## Usage notes

See [Audio subsystem guide](../AUDIO_SUBSYSTEM_GUIDE.md) for setup, processing order, validation rules, and examples. This page summarises the API surface; consult the source for implementation details.

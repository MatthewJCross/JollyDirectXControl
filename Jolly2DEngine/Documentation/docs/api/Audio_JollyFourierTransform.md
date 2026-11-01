# `JollyFourierTransform`

Static Fourier transform helpers for power-of-two sample buffers.

Namespace: `Jolly2DEngine.Audio`.

## Public API

```csharp
public static class JollyFourierTransform
public static void Transform(ReadOnlySpan<float> samples, Span<Complex> spectrum, JollyFourierWindow window = JollyFourierWindow.Hann)
public static void GetMagnitudeSpectrum(ReadOnlySpan<Complex> spectrum, Span<float> magnitudes)
public static float GetFrequencyForBin(int bin, int fftSize, int sampleRate)
```

## Usage notes

See [Audio subsystem guide](../AUDIO_SUBSYSTEM_GUIDE.md) for setup, processing order, validation rules, and examples. This page summarises the API surface; consult the source for implementation details.

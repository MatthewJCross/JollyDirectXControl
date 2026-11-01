# `JollyEqualiser`

Configurable multi-band peaking-filter equaliser. Band count and frequencies can be chosen by the caller.

Namespace: `Jolly2DEngine.Audio`.

## Public API

```csharp
public sealed class JollyEqualiser
public bool Enabled { get; set; }
public IReadOnlyList<JollyEqualiserBand> Bands { get; }
public JollyEqualiser(int sampleRate, int channels)
public int AddBand(float frequency, float gainDb = 0.0f, float q = 1.0f)
public void SetBand(int index, float frequency, float gainDb, float q = 1.0f)
public void SetBandFrequency(int index, float frequency)
public void SetBandQ(int index, float q)
public void SetBandGain(int index, float gainDb)
public void RemoveBand(int index)
public void ClearBands()
public void ConfigureBands(IEnumerable<float> frequencies, float gainDb = 0.0f, float q = 1.0f)
public void ConfigureLogarithmicBands(int bandCount, float minimumFrequency, float maximumFrequency, float gainDb = 0.0f, float q = 1.0f)
public float ProcessSample(float sample, int channel)
public void Reset()
```

## Usage notes

See [Audio subsystem guide](../AUDIO_SUBSYSTEM_GUIDE.md) for setup, processing order, validation rules, and examples. This page summarises the API surface; consult the source for implementation details.

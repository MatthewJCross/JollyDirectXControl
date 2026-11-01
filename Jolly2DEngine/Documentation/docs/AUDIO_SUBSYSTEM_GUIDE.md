# Audio subsystem guide

This guide describes the audio implementation in `Jolly2DEngine.Audio` from the supplied source. Audio playback uses NAudio; the engine adds cached sound playback, music control, effect processing, and analysis data suitable for visualisers.

## Choose the right entry point

- **`JollyAudioManager`** — load and play sound effects, control music, and set master/music/SFX volume.
- **`JollyAudioEffects`** — create an effect chain for a sample stream. It exposes an equaliser, pitch shifter, chorus, echo, reverb and analyser.
- **`JollyEqualiser`** — configure a variable number of peaking-filter bands. Frequencies are explicitly supplied or generated logarithmically.
- **`JollyAudioAnalyser`** — analyse incoming mono samples, publish a spectrum snapshot, and estimate beat/BPM information.
- **`JollyFourierTransform`** — run a Fourier transform on a power-of-two sample buffer when you want to manage analysis yourself.

## Basic playback

```csharp
using Jolly2DEngine.Audio;

JollyAudioManager audio = new();
audio.MasterVolume = 1.0f;
audio.MusicVolume = 0.65f;
audio.SfxVolume = 0.85f;

audio.LoadSound("laser", "Assets/Audio/laser.wav");
audio.PlaySound("laser");
audio.PlayMusic("Assets/Audio/music.mp3", loop: true);

// When the owning game or service shuts down:
audio.Dispose();
```

`LoadSound` caches sound data so subsequent playback can overlap. Music has a separate playback path from sound effects. Dispose the manager when its owner shuts down.

## Add an effects chain to a sample provider

`JollyAudioEffects` must be constructed for the sample rate and channel count of the provider it will process. `CreateSampleProvider` checks those values and throws if they do not match.

```csharp
using Jolly2DEngine.Audio;
using NAudio.Wave;

using AudioFileReader source = new("Assets/Audio/music.wav");
JollyAudioEffects effects = new(source.WaveFormat.SampleRate, source.WaveFormat.Channels);

effects.Equaliser.ConfigureLogarithmicBands(
    bandCount: 10,
    minimumFrequency: 40.0f,
    maximumFrequency: 16000.0f,
    gainDb: 0.0f,
    q: 1.0f);

effects.Equaliser.SetBandGain(0, 3.0f);
effects.PitchShifter.PitchSemitones = 0.0f;
effects.Chorus.Enabled = false;
effects.Echo.Enabled = false;
effects.Reverb.Enabled = false;

ISampleProvider processed = effects.CreateSampleProvider(source);
// Connect `processed` to the output device used by your application.
// Keep `source` and `effects` alive for the lifetime of playback.
```

The chain order in the current implementation is equaliser → pitch shift → chorus → echo → reverb → analyser. Individual effects can be disabled without removing the chain. `JollyAudioEffects.Dispose()` disposes its analyser; it does not dispose the supplied source or an output device.

## Configuring equaliser bands

Bands are indexed from zero. `AddBand` returns the new band index; `SetBand`, `SetBandGain`, `SetBandFrequency` and `SetBandQ` update a band. `ConfigureBands` replaces the existing bands with the supplied frequency list. `ConfigureLogarithmicBands` creates frequencies distributed logarithmically between the requested endpoints.

Constraints enforced by the implementation:

- Sample rate and channel count must be positive.
- Each band frequency must be greater than zero and below the Nyquist frequency (`sampleRate / 2`).
- Gain must be between -24 dB and +24 dB.
- Q must be greater than zero and no greater than 20.
- Logarithmic band count must be at least one.

`JollyEqualiserBand.Frequency`, `GainDb` and `Q` are readable by callers but have internal setters. Change them through the equaliser methods rather than assigning to the band properties.

## Read spectrum and beat information

`JollyAudioAnalyser` accepts individual samples using `AddSample`. Once enough samples have arrived, a background worker publishes the most recent `JollySpectrumData` through `GetSpectrum()`.

```csharp
JollySpectrumData? spectrum = effects.Analyser.GetSpectrum();

if (spectrum is not null)
{
    float rms = spectrum.Rms;
    float peak = spectrum.Peak;

    for (int i = 0; i < spectrum.Magnitudes.Length; i++)
    {
        float frequencyHz = spectrum.Frequencies[i];
        float magnitude = spectrum.Magnitudes[i];
        // Map frequencyHz/magnitude to your visualiser or equaliser display.
    }
}

float bpm = effects.Analyser.Bpm;
float confidence = effects.Analyser.BeatConfidence;
bool isBeat = effects.Analyser.IsBeat;
```

`Magnitudes` and `Frequencies` use matching indices. `FftSize` is the analysis window size; the spectrum contains `FftSize / 2 + 1` bins. `Rms` and `Peak` describe signal level. `IsBeat` is a short-lived beat indicator, not a guaranteed musical beat grid. Treat BPM and confidence as estimates.

The analyser's FFT size must be a power of two and at least 256. The default is 2048. Its `Window` can be `Rectangular`, `Hann`, `Hamming` or `Blackman`. Dispose the analyser when finished so its worker can stop.

## Direct Fourier transform

`JollyFourierTransform.Transform` takes a `ReadOnlySpan<float>` input and a `Span<Complex>` output. The input length must be a power of two greater than one and the output buffer must be at least that long. After the transform, use `GetMagnitudeSpectrum` to fill a buffer of at least `inputLength / 2 + 1` magnitudes. `GetFrequencyForBin` maps a bin index to Hz.

## Effect controls

- **Pitch shift:** `PitchFactor` accepts 0.5–2.0; `PitchSemitones` and `PitchCents` are alternate representations. The implementation preserves stream rate and duration and can produce modulation artefacts.
- **Chorus:** use `Enabled`, `WetMix` and the exposed maximum-delay information to control the effect.
- **Echo:** use `Enabled`, `WetMix`, delay and feedback controls exposed by `JollyEcho`.
- **Reverb:** `Preset` selects Room, Studio, Hall, Cathedral or Plate settings. `DecayTime`, `Damping`, `PreDelayMilliseconds` and `WetMix` are also configurable. These setters clamp values to implementation-defined ranges.
- **Biquad filter:** `JollyBiquadFilter` provides the lower-level filter implementation, including peaking and shelf configuration.

See the individual API pages for each type's public members.

## Threading and lifetime notes

- The analyser processes its work on a dedicated background thread. Read the latest published spectrum rather than calling internal processing methods.
- Keep source providers, effects, and output devices alive for the duration of playback.
- Dispose `JollyAudioManager`, `JollyAudioEffects`, and standalone `JollyAudioAnalyser` instances when their owner shuts down.
- Do not assume the analyser's BPM estimate is immediately available at startup; it needs incoming samples and history.

# `JollyChorus`

Chorus effect controls.

Namespace: `Jolly2DEngine.Audio`.

## Public API

```csharp
public sealed class JollyChorus
public bool Enabled { get; set; }
public float Rate { get; set; }
public float Depth { get; set; }
public float DelayMilliseconds { get; set; }
public float Feedback { get; set; }
public float WetMix { get; set; }
public float DryMix { get; }
public float MaximumDelayMilliseconds { get; }
public JollyChorus(int sampleRate, int channels, float maximumDelayMilliseconds = 50.0f)
public void Process(Span<float> buffer)
public void Clear()
```

## Usage notes

See [Audio subsystem guide](../AUDIO_SUBSYSTEM_GUIDE.md) for setup, processing order, validation rules, and examples. This page summarises the API surface; consult the source for implementation details.

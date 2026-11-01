# `JollyEqualiserBand`

Read-only-to-consumers description of an equaliser band. Its values are changed through JollyEqualiser.

Namespace: `Jolly2DEngine.Audio`.

## Public API

```csharp
public sealed class JollyEqualiserBand
public float Frequency { get; }
public float GainDb { get; }
public float Q { get; }
```

## Usage notes

See [Audio subsystem guide](../AUDIO_SUBSYSTEM_GUIDE.md) for setup, processing order, validation rules, and examples. This page summarises the API surface; consult the source for implementation details.

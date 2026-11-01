# `TvStaticColourMode`

Colour modes used by `TvStaticGenerator`.

```csharp
public enum TvStaticColourMode
{
    Monochrome,
    FullColour,
    BlueTint,
    GreenTint,
    RedTint
}
```

The default mode is `Monochrome`. The selected mode is read when each new static frame is generated.

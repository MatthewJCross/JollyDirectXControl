# `Graphics/JollyLineSnapshot.cs`

Public API declarations from `Graphics/JollyLineSnapshot.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public readonly struct JollyLineSnapshot(Vector2 start, Vector2 end, JollyColor color, float thickness, float startThickness = 1.0f, float endThickness = 1.0f)
```

```csharp
public Vector2 Start { get; } = start;
```

```csharp
public Vector2 End { get; } = end;
```

```csharp
public JollyColor Color { get; } = color;
```

```csharp
public float Thickness { get; } = thickness;
```

```csharp
public float StartThickness { get; } = startThickness;
```

```csharp
public float EndThickness { get; } = endThickness;
```

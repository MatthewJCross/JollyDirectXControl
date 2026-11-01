# `Math/JollyColor.cs`

Public API declarations from `Math/JollyColor.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public readonly struct JollyColor : IEquatable<JollyColor>
```

```csharp
public float R { get; } public float G { get; } public float B { get; } public float A { get; }  public JollyColor(float r, float g, float b, float a = 1.0f)
```

```csharp
public static JollyColor White => new(1.0f, 1.0f, 1.0f, 1.0f);
```

```csharp
public static JollyColor Black => new(0.0f, 0.0f, 0.0f, 1.0f);
```

```csharp
public static JollyColor Red => new(1.0f, 0.0f, 0.0f, 1.0f);
```

```csharp
public static JollyColor Green => new(0.0f, 1.0f, 0.0f, 1.0f);
```

```csharp
public static JollyColor Blue => new(0.0f, 0.0f, 1.0f, 1.0f);
```

```csharp
public static JollyColor Yellow => new(1.0f, 1.0f, 0.0f, 1.0f);
```

```csharp
public static JollyColor Transparent => new(0.0f, 0.0f, 0.0f, 0.0f);
```

```csharp
public static JollyColor FromBytes(byte r, byte g, byte b, byte a = 255)
```

```csharp
public bool Equals(JollyColor other)
```

```csharp
public override bool Equals(object? obj)
```

```csharp
public override int GetHashCode()
```

```csharp
public static bool operator ==(JollyColor left, JollyColor right)
```

```csharp
public static bool operator !=(JollyColor left, JollyColor right)
```

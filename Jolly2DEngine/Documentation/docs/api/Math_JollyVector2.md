# `Math/JollyVector2.cs`

Public API declarations from `Math/JollyVector2.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public struct JollyVector2
```

```csharp
public float X;
```

```csharp
public float Y;
```

```csharp
public JollyVector2(float x, float y)
```

```csharp
public static JollyVector2 Zero => new JollyVector2(0.0f, 0.0f);
```

```csharp
public static JollyVector2 One => new JollyVector2(1.0f, 1.0f);
```

```csharp
public static JollyVector2 operator +(JollyVector2 left, JollyVector2 right)
```

```csharp
public static JollyVector2 operator -(JollyVector2 left, JollyVector2 right)
```

```csharp
public static JollyVector2 operator *(JollyVector2 value, float scalar)
```

```csharp
public static JollyVector2 operator /(JollyVector2 value, float scalar)
```

```csharp
public float Length()
```

```csharp
public float LengthSquared()
```

```csharp
public void Normalize()
```

```csharp
public static JollyVector2 Normalize(JollyVector2 value)
```

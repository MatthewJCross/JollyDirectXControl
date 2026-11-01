# `Physics/JollyCollisionDebug.cs`

Public API declarations from `Physics/JollyCollisionDebug.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public static class JollyCollisionDebug
```

```csharp
public static bool Enabled { get; set; }  public static bool LogPixelChecks { get; set; }  public static bool LogPixelHits { get; set; } = true;
```

```csharp
public static bool DrawColliders { get; set; }  public static void CollisionEnter(JollyGameObject objectA, JollyGameObject objectB)
```

```csharp
public static void CollisionStay(JollyGameObject objectA, JollyGameObject objectB)
```

```csharp
public static void CollisionExit(JollyGameObject objectA, JollyGameObject objectB)
```

```csharp
public static void TriggerEnter(JollyGameObject objectA, JollyGameObject objectB)
```

```csharp
public static void TriggerStay(JollyGameObject objectA, JollyGameObject objectB)
```

```csharp
public static void TriggerExit(JollyGameObject objectA, JollyGameObject objectB)
```

```csharp
public static void PixelHit(JollySprite spriteA, JollySprite spriteB, int x, int y)
```

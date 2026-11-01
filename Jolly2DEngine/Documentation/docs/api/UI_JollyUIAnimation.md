# `UI/JollyUIAnimation.cs`

Public API declarations from `UI/JollyUIAnimation.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public enum JollyUIAnimationProperty
```

```csharp
public sealed class JollyUIAnimation
```

```csharp
public JollyUIElement Element { get; } public JollyUIAnimationProperty Property { get; } public float Duration { get; } public bool IsComplete { get; private set; }  internal JollyUIAnimation(JollyUIElement element, JollyUIAnimationProperty property, Vector2 start, Vector2 end, float duration)
```

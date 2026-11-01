# `Graphics/JollyTexture.cs`

Public API declarations from `Graphics/JollyTexture.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyTexture
```

```csharp
public int Width { get; } public int Height { get; }  internal object RendererTexture { get; }  internal byte[] AlphaMask { get; }  internal JollyTexture(object rendererTexture, int width, int height, byte[] alphaMask)
```

```csharp
public byte GetAlpha(int x, int y)
```

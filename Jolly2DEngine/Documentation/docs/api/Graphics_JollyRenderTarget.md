# `Graphics/JollyRenderTarget.cs`

Public API declarations from `Graphics/JollyRenderTarget.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyRenderTarget : IDisposable
```

```csharp
public int Width { get; } public int Height { get; }  internal JollyRenderTarget(ID3D11Texture2D texture, ID3D11RenderTargetView renderTargetView, ID3D11ShaderResourceView shaderResourceView, int width, int height)
```

```csharp
public void Dispose()
```

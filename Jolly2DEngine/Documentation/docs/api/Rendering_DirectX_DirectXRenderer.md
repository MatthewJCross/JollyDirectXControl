# `Rendering/DirectX/DirectXRenderer.cs`

Low-level Direct3D 11 renderer. It owns device/swap-chain resources, shaders, buffers, render targets and the render loop. Prefer `DirectXJollyRenderer` for normal engine use.

```csharp
public enum DirectXImageFilter
public enum DirectXStretchMode
public sealed class DirectXRenderer : IDisposable
public void Initialize(IntPtr hwnd, int width, int height)
public void StartRenderLoop()
public void StopRenderLoop()
public void SetStretchMode(DirectXStretchMode mode)
public void SetImageFilter(DirectXImageFilter filter)
public void LoadImage(string filePath)
public void SetImage(BitmapSource source)
public void SetImage(byte[] pixels, int width, int height)
public void SetImage(DirectXImage image)
public DirectXTexture CreateTextureResource(string filePath)
public DirectXTexture CreateTextureResource(DirectXImage image)
public void SubmitRenderFrame(JollyRenderFrame frame, Camera2D camera)
public void Clear()
public void ClearScreen()
public void Render()
public void Resize(int width, int height)
public void Dispose()
```

The implementation has separate sprite, line, image and triangle shader paths. `Resize` recreates size-dependent swap-chain/render-target resources. Image filtering/stretch settings are separate from the high-level sprite camera transform.

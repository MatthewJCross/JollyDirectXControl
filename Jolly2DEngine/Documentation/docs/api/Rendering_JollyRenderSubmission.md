# `Rendering/JollyRenderSubmission.cs`

Packages a render frame with the camera used for its submission.

```csharp
public sealed class JollyRenderSubmission
public JollyRenderSubmission(JollyRenderFrame frame, Camera2D camera)
```

The submission object is an internal render-pipeline handoff abstraction; use `DirectXJollyRenderer.SubmitFrame` for normal application rendering.

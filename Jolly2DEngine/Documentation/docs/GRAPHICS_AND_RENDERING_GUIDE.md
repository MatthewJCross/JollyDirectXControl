# Graphics and Rendering Guide

This guide describes the graphics APIs in the supplied Jolly2DEngine source. The high-level API lives mainly in `Jolly2DEngine.Graphics` and `Jolly2DEngine.Rendering`; the DirectX 11 implementation is under `Jolly2DEngine.Rendering.DirectX`.

## Rendering architecture

```text
Game/update code
   |
   +-- JollySprite objects
   +-- JollyLineSnapshot values
   +-- JollyTriangleSnapshot values
   |
   v
JollyRenderFrame (captures visible sprites and primitive lists)
   |
   v
DirectXJollyRenderer.SubmitFrame(...)
   |
   v
DirectXRenderer.SubmitRenderFrame(frame, camera)
   |
   +-- sprite pipeline
   +-- line pipeline
   +-- triangle pipeline
   +-- Direct3D 11 swap chain / render target
```

`DirectXControl` hosts the DirectX renderer in WPF. `DirectXJollyRenderer` is the engine-facing renderer and owns the `Camera2D`. `DirectXRenderer` performs the lower-level Direct3D 11 work, including shader and buffer management, frame submission, rendering and swap-chain resizing.

## Textures and sprites

`JollyTexture` is the engine's renderer-backed texture handle. Its public surface exposes pixel dimensions and `GetAlpha(x, y)`, which returns zero for coordinates outside the texture. The underlying renderer resource and alpha mask are internal implementation details; applications should normally obtain textures through `JollyRenderer.LoadTexture`, `DirectXJollyRenderer` texture creation methods, or the asset manager.

`JollySprite` combines a texture with position, size, scale, rotation, opacity, tint, visibility, layer ordering, source-rectangle selection, flipping and animation state. Sprite positions are interpreted by the renderer/camera transform. `JollyRenderFrame` snapshots visible sprites and sorts them by `Layer` before submission. Keep layer values intentional: higher values are submitted later in the sprite list and therefore appear over lower-layer sprites when normal alpha blending is used.

A sprite can use a sub-rectangle of an atlas by setting `SourceX`, `SourceY`, `SourceWidth` and `SourceHeight` in normalized texture coordinates. `JollySpriteSheet` provides frame indexing for fixed-size atlas cells, and `JollyAnimation` can bind named frame sequences to a sprite.

## Sprite-sheet text

`SpriteFontRenderer` builds one sprite per supported character using a fixed-cell font atlas. Configure the atlas texture, number of columns, cell width/height and first character code. `DrawText` creates a new group of character sprites; the `Sprites` enumerable exposes all accumulated groups. `Clear()` removes the generated groups. `CharacterSpacing`, `LineSpacing` and `Scale` adjust layout. Newline characters move to the next line; carriage returns are ignored. Characters outside the atlas range fall back to `?` when that glyph exists.

This is distinct from `DirectXJollyRenderer.CreateTextTexture`, which rasterises a complete text string through WPF text formatting and returns one texture.

## Text textures and measurement

Use `CreateTextTexture(text, fontFamily, fontSize, color, bold, italic, ultraBold)` to rasterise text into a texture. `MeasureTextWidth` uses the same WPF font measurement path and includes trailing whitespace. `ultraBold` selects the black font weight; otherwise `bold` selects bold, and the default is normal weight. These APIs are useful for labels, HUDs and static text baked into a texture.

`JollyTextureComposer.DrawText` uses this text-texture path, then alpha-composites the result into its canvas. Text is rasterised at the requested font size; its destination coordinates are top-left pixel coordinates in the composition canvas.

## Sprite composition, overlays and opacity

`JollyTextureComposer` builds a single BGRA texture in CPU memory. Construct it with a renderer and positive pixel dimensions, optionally providing a background colour. `Fill` replaces the whole canvas. `DrawTexture` composites a source texture at a top-left pixel position, optionally scaling it and applying an additional opacity multiplier. Source alpha and requested opacity are combined during alpha blending. Parts outside the canvas are clipped. `CreateTexture` uploads a new texture; `UpdateTexture` updates an existing texture and requires exactly matching dimensions.

The composer accepts `JollyTexture` instances created by the DirectX renderer; a texture backed by another renderer causes an `InvalidOperationException`. Opacity must be between 0 and 1 inclusive. Composition is a CPU-side operation followed by texture creation/update, so it is best suited to static or infrequently changed composites rather than per-pixel work every frame.

For ordinary live sprite fades, use `JollySprite.Opacity`. For image tinting use `JollySprite.Color`. Use `JollySprite.Visible` to omit a sprite from the frame snapshot entirely.

## Lines and triangles

`JollyLineSnapshot` is an immutable line submission value with start/end positions, colour, base thickness and independent start/end thickness multipliers. `GlowEnabled` defaults to true. A `JollyRenderFrame` excludes lines whose base `Thickness` is not greater than zero. The renderer's line-glow settings are exposed by `DirectXJollyRenderer` (`LineGlowEnabled`, `LineGlowWidth`, `LineGlowStrength` and `LineGlowFalloff`).

`JollyTriangleSnapshot` stores three vertices and one colour. Pass triangle snapshots to the `JollyRenderFrame` constructor to submit filled triangles. The DirectX renderer uses a triangle-list pipeline and the triangle vertex/pixel shaders. The current snapshot API represents a flat-colour triangle; it does not expose per-vertex colour, texture coordinates or a material.

## TV static generation

`TvStaticGenerator` creates a generated texture and a sprite that can be added to a normal frame. Its width and height are pixel dimensions. `FramesPerSecond` is constrained to 1–120. `Update(deltaSeconds)` advances its frame timer and returns `true` only when it generates a new frame; call it from the game update loop. A fixed `randomSeed` makes the generated noise repeatable for a given sequence.

`TvStaticColourMode` supports `Monochrome`, `FullColour`, `BlueTint`, `GreenTint` and `RedTint`. The generator adds horizontal dark scanlines every fourth row. `ColourMode` can be changed at runtime; the next generated frame uses the new mode.

`AddOverlay` adds an existing texture at top-left pixel coordinates with an opacity multiplier. `AddText` creates a text texture and registers it as an overlay. Overlays are composited when a new static frame is generated. `ClearOverlays` removes the registered overlays. The generator exposes its resulting `Texture` and `Sprite`; add the sprite to the frame as you would any other sprite.

## Camera and viewport

`Camera2D` transforms between world and screen coordinates and supports position, zoom, rotation, viewport sizing, optional world bounds, immediate and smoothed following, and screen shake. Use `SetViewport` when the render viewport changes. Use `SetVirtualResolution` when the game should render against a fixed logical resolution; `CameraState` captures the transform and viewport scale/offset used for a render submission. `WorldToScreen` and `ScreenToWorld` are the corresponding conversion helpers.

The DirectX control and renderer have their own pixel dimensions and swap-chain lifecycle. `DirectXRenderer.Resize` releases/recreates size-dependent swap-chain resources. The high-level `DirectXJollyRenderer.Resize` forwards the requested dimensions to the DirectX control/renderer path. Keep the camera viewport in step with the drawable viewport; do not assume the WPF control's initial size remains valid after a window resize.

## Image and texture creation

`DirectXImage` represents CPU-side image data and validates that the supplied memory contains exactly four bytes per pixel. It can be loaded from a file or created from a WPF `BitmapSource`. `DirectXTexture` is the DirectX-backed resource wrapper used internally by `JollyTexture`.

`DirectXJollyRenderer` provides helpers for loading a file texture, creating solid/circle textures, creating textures from BGRA pixel arrays, updating those pixel arrays, and creating text textures. Pixel-array creation/update validates dimensions and buffer length. The method names explicitly say BGRA; do not assume that arbitrary RGBA byte arrays can be passed unchanged.

`JollyTextureManager` caches textures by normalized full file path using a case-insensitive dictionary. Repeated `Load` calls for the same path return the cached object. `Clear` drops the cache entries; the current implementation does not expose a per-texture unload or dispose method on `JollyTextureManager`.

## Low-level DirectX controls

Use `DirectXControl` as the WPF host. `WaitForReadyAsync` allows callers to wait until the device/renderer is ready. The control exposes image-loading and texture-creation entry points as well as `CreateJollyRenderer`. `DirectXRenderer` is the low-level implementation and manages the Direct3D device, swap chain, render targets, shaders, vertex buffers and render loop. Prefer the high-level renderer for normal engine code; direct low-level calls are for specialised integrations.

The supplied shader set includes sprite, line, image and triangle vertex/pixel shaders. The compiled `.cso` shader assets are required at runtime alongside the source `.hlsl` files; retaining the shader content in package/publish output is essential.

## Example: compose a reusable title texture

```csharp
JollyTextureComposer composer = new(renderer, 640, 180, new JollyColor(0.02f, 0.03f, 0.08f, 1.0f));
composer.DrawTexture(logoTexture, 20, 20, opacity: 0.9f);
composer.DrawText("JOLLY ARCADE", "Segoe UI", 42.0f, JollyColor.White, 190, 55, bold: true, ultraBold: true);
JollyTexture titleTexture = composer.CreateTexture();
JollySprite titleSprite = renderer.CreateSprite(titleTexture);
titleSprite.Position = new JollyVector2(320.0f, 90.0f);
titleSprite.Layer = 100;
```

## Example: submit a filled triangle

```csharp
List<JollyTriangleSnapshot> triangles =
[
    new JollyTriangleSnapshot(
        new Vector2(320.0f, 100.0f),
        new Vector2(250.0f, 220.0f),
        new Vector2(390.0f, 220.0f),
        JollyColor.Cyan)
];

JollyRenderFrame frame = new(sprites, lines, triangles);
renderer.SubmitFrame(frame);
```

## Resource lifetime and practical guidance

- Create DirectX-backed textures after the control reports readiness.
- Keep references to textures and sprites for as long as they are needed; a sprite refers to its texture.
- Avoid rebuilding composed textures every frame unless the CPU cost is intentional.
- `JollyRenderTarget` implements `IDisposable`; dispose render targets when finished.
- `JollyTexture` itself does not implement `IDisposable` in the supplied source.
- Keep WPF control access on the appropriate UI/dispatcher thread; the game update thread should submit engine data, not manipulate WPF controls directly.
- Ensure the DirectX shader `.cso` files are copied/published with the application.

## Validation status

This guide was checked against the supplied source declarations and implementation paths. It has not been validated by compiling the engine, running a DirectX device, resizing a live window, or executing the examples. See `GRAPHICS_RENDERING_DOCUMENTATION_AUDIT.md` for specific follow-up checks.

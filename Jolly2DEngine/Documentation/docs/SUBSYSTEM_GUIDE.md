# Jolly2DEngine Subsystem Guide

## Core lifecycle

`JollyGame` is the application-level owner. It creates `JollySceneManager`, `JollyInput`, `JollyGameTime`, and `JollyDiagnostics`.

The lifecycle is:

1. `Start()` creates the game thread.
2. `OnInitialize()` is called once on that thread.
3. Each frame updates `JollyGameTime`.
4. `Scenes.Update(gameTime)` runs.
5. `OnUpdate(gameTime)` runs.
6. Input's frame state is advanced with `EndFrame()`.
7. On stop, `OnShutdown()` runs and scenes are shut down.

`GameLoopFPS = 0` means the game loop is not deliberately capped. A positive value supplies a target update rate. `RenderFPS` is forwarded to the renderer through `OnRenderFPSChanged`.

## Game objects

`JollyGameObject` is the base world entity. Its transform is `Position`, `Scale`, and `Rotation`. `Velocity`, `Acceleration`, `Gravity`, `Drag`, and the physics flags control simulation. `Collider` is optional.

Override:

- `Initialize()` for one-time object setup.
- `Update(JollyGameTime)` for object logic.
- `Draw()` for custom draw integration.
- `Shutdown()` for cleanup.
- collision/trigger callbacks for interaction.

Call `Destroy()` to mark the object destroyed and inactive. The scene removes destroyed objects as part of its update lifecycle.

`JollySpriteObject` adds a `JollySprite` and synchronises the sprite transform automatically. Its collision helpers are the recommended way to configure sprite-based collision.

## Scene lifecycle

`JollyScene` owns an object list and supports deferred add/remove while an update is in progress. This prevents collection modification problems during object callbacks.

`OnEnter()` and `OnExit()` are scene transitions. `Initialize()` initialises objects once. `SetTileMap()` associates a tile map and world offset with the scene.

## Rendering model

The normal path is:

```text
WPF Window
   -> DirectXControl
      -> DirectXRenderer
   -> DirectXJollyRenderer
      -> JollyTexture / JollySprite
      -> JollyRenderFrame
      -> Camera2D
```

A `JollySprite` is a high-level render object. At frame submission it becomes a `JollySpriteSnapshot`; line primitives become `JollyLineSnapshot` values. The renderer consumes the resulting `JollyRenderFrame`.

The renderer batches sprites and exposes counters such as `RenderSpriteCount`, `RenderBatchCount`, and `RenderDrawCallCount` for profiling.

## Coordinate system

World and screen coordinates use `float` values. `Camera2D` transforms world positions into screen coordinates and supports zoom, rotation, viewport sizing, bounds and screen shake.

`Follow()` immediately targets a position. `FollowSmooth()` applies a delta-time-aware interpolation. `Shake(intensity, duration)` creates a temporary offset; call `UpdateShake(deltaSeconds)` when driving a camera manually.

## Graphics and rendering

The high-level render path is `JollySprite` / line and triangle snapshots → `JollyRenderFrame` → `DirectXJollyRenderer` → `DirectXRenderer`. Frames snapshot visible sprites and order them by layer. The renderer supports sprite, line and filled-triangle pipelines. Text can be rasterised to a texture through the high-level renderer or built character-by-character from a sprite-font atlas. `JollyTextureComposer` performs CPU-side alpha composition; `TvStaticGenerator` produces animated noise and can composite image/text overlays. `Camera2D` provides world/screen transforms, follow, bounds, zoom, rotation and shake. See the [Graphics and Rendering guide](GRAPHICS_AND_RENDERING_GUIDE.md) for usage and limitations.

## Physics

Collision filtering uses bitmasks. An object collides only when both masks accept the opposing layer:

```text
(moving.Mask & other.Layer) != 0
and
(other.Mask & moving.Layer) != 0
```

Collider shapes are rectangle, circle and pixel. Trigger colliders participate in overlap callbacks but are not resolved as solid physical collisions.

`JollyPhysicsSystem` performs the engine's movement/collision resolution. `JollyCollisionSystem.Check` provides direct collision tests for colliders.

## Input state semantics

`IsKeyDown`/`IsButtonDown` are level tests: true while held.

`IsKeyPressed`/`IsButtonPressed` are edge tests for the current frame.

`IsKeyReleased`/`IsButtonReleased` are release-edge tests.

The game calls `Input.EndFrame()` after update processing, so perform input queries during the update phase.

## Audio

Sound effects are cached in memory after `LoadSound`. `PlaySound` creates a playback instance from the cached samples, allowing overlapping effects. Music is handled separately through an `AudioFileReader` and can loop. Master volume multiplies both music and SFX volume. `Muted` forces effective output volume to zero without losing the configured volume values.

For configurable equaliser bands, sample-provider effects, FFT spectrum data, beat/BPM estimates and detailed examples, see the [Audio subsystem guide](AUDIO_SUBSYSTEM_GUIDE.md). The guide documents the effect order and sample-rate/channel compatibility requirements.

## Assets

`JollyAssetManager` centralises asset paths and texture lifetime. Texture handles can be reference-counted using `AcquireTexture`/`JollyAssetHandle<T>.Dispose()`.

`GetTexturePath`, `GetAudioPath`, `GetMapPath`, `GetAnimationPath` and `GetFontPath` resolve names against the configured asset root.

## Save system

`JollySaveManager` wraps JSON save files with a format version and game version. A backup is maintained by the manager and can be restored through `RestoreBackup`.

`JollySceneSaveState` captures engine-owned object state. It does not magically serialize arbitrary fields added by a custom game object; store custom game data in your own payload type.

## UI

UI elements are screen-space objects. `JollyUIManager` tracks the hovered and pressed element and feeds mouse interaction into controls. Buttons expose `Click`; sliders expose `ValueChanged`.

Nine-slice elements are intended for scalable panels/buttons where the corners should retain their original dimensions.

## Particles

`JollyParticleEmitter` creates transient particles according to an emitter configuration. Presets are convenient starting configurations, not special renderer modes. Adjust the preset properties after construction when you need a custom effect.

## Networking

Networking is TCP-based. `JollyNetworkMessage.Payload` is the raw message body; `Text` decodes it as UTF-8. TCP provides ordered reliable delivery but does not define a game protocol, message schema, authentication or prediction model. Build those above the supplied transport layer.

## Threading and disposal

Dispose long-lived engine resources in reverse ownership order: stop the game, stop/dispose networking, stop audio, then release renderer/assets as appropriate. Never assume a WPF control can be touched from the game thread.

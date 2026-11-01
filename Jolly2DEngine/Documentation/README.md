# Jolly2DEngine 1.2.0 Documentation

Jolly2DEngine is a .NET 10 Windows/WPF 2D game engine using DirectX 11 for rendering and NAudio for audio.

This documentation is intended to be the **developer manual and API reference**, not just a project description.

## Start here

1. **[Project setup and first game](docs/SETUP_AND_USING_THE_ENGINE.md)** — create a WPF project, add the engine, create `DirectXControl`, initialise the renderer, start `JollyGame`, create scenes and objects, and use the major systems.
2. **[Subsystem guide](docs/SUBSYSTEM_GUIDE.md)** — understand lifecycle, threading, rendering, physics, input, audio, assets, saving, UI, particles and networking.
3. **[Graphics and Rendering guide](docs/GRAPHICS_AND_RENDERING_GUIDE.md)** — sprites, composition, transparency, text, triangles, textures, TV static, camera and DirectX rendering.
4. **[Audio subsystem guide](docs/AUDIO_SUBSYSTEM_GUIDE.md)** — configure playback, equaliser bands, audio effects, Fourier analysis, spectrum data and beat detection.
5. **[Tile-map guide](docs/TILE_MAP_GUIDE.md)** — create tile grids, assign an atlas, render visible tiles, configure collision, animation, hazards and conveyors.
6. **[Complete public API reference](docs/API_REFERENCE.md)** — every public declaration from the supplied source, grouped by source file.
7. **[API usage and parameter reference](docs/API_USAGE_REFERENCE.md)** — every public method/constructor with a parameter-by-parameter explanation and return description.
8. **[Enumeration reference](docs/ENUMS.md)** — every public enum and its values.

## API documentation by subsystem

- `docs/api/Core_*.md` — game loop, game objects and game time
- `docs/api/Graphics_*.md` — sprites, textures, composition, static generation and render snapshots
- `docs/api/Rendering_*.md` — camera, DirectX, texture management and sprite sheets
- `docs/api/Input_*.md` — keyboard, mouse, gamepad and input facade
- `docs/api/Physics_*.md` — colliders, collision tests, physics and character controllers
- `docs/api/Scene_*.md` — scenes and scene management
- `docs/api/UI_*.md` — UI elements and controls
- `docs/api/Audio_*.md` — audio manager, effect chain, equaliser, analysis, Fourier transform and effects
- `docs/api/Assets_*.md` — asset loading and lifetime
- `docs/api/Particles_*.md` — particle emitters and systems
- `docs/api/Save_*.md` — save-state and save-slot APIs
- `docs/api/Networking_*.md` — TCP client/server/message APIs
- `docs/api/Diagnostics_*.md` — profiling and diagnostics
- `docs/api/World_*.md` — tile maps

Each API page contains the declarations exactly as implemented by the engine source, while the setup and subsystem guides explain what the parameters are for and how the APIs fit together.

## Engine architecture

```text
                    WPF Application
                          |
                    DirectXControl
                          |
                    DirectXRenderer
                          |
                 DirectXJollyRenderer
                          |
        +-----------------+-----------------+
        |                 |                 |
     Camera2D        JollyRenderer      DirectX 11
        |                 |
        +---------- JollyRenderFrame
                          |
                 JollySprite / Lines

 JollyGame
    |
    +-- JollySceneManager
    |      +-- JollyScene
    |             +-- JollyGameObject
    |                    +-- JollySpriteObject
    |                    +-- Physics / Collider
    |
    +-- JollyInput
    +-- JollyGameTime
    +-- JollyDiagnostics
    +-- Audio / Assets / Save / Networking / UI / Particles
```

## Important implementation facts

- Target: `net10.0-windows`.
- WPF and Windows Forms are enabled because the DirectX host uses WPF/WinForms interop.
- The project targets x64.
- DirectX shader bytecode is packaged under `Rendering/DirectX/Shaders`.
- The normal game update loop runs on a dedicated `Jolly2D Game Thread`.
- Input edge states should be read during update before `EndFrame()` advances them.
- `DirectXJollyRenderer` is the recommended high-level renderer; use `DirectXRenderer` directly only when building lower-level integrations.

## API conventions

Unless a specific type says otherwise:

- Positions, sizes and velocities are expressed in engine world units.
- `float` values are used for world/render quantities.
- Colours use `JollyColor` with components in the `0..1` range; `JollyColor.FromBytes` is provided for `0..255` colour input.
- Audio volumes are clamped to `0..1`.
- Sprite opacity is clamped to `0..1`.
- Boolean input methods named `Is...Down` represent held state; `Is...Pressed` and `Is...Released` represent frame transitions.

## Source of truth

The documentation was generated and written against the supplied Jolly2DEngine 1.2.0 source. If the API changes, regenerate/update the API inventory alongside the code.

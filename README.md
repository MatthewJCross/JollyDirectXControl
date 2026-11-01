# Jolly2DEngine

Jolly2DEngine is a **DirectX 11 2D engine for .NET 10 Windows applications**, designed to be embedded in WPF projects through a dedicated WPF control.

The aim is to provide a reusable foundation for building 2D games, visualisations, simulations and multimedia applications without having to build the rendering and supporting systems from scratch for every project.

> **Project status:** Jolly2DEngine is actively evolving. The engine and its APIs may change as features are added and refined. The included demos are useful examples of the engine in practice, but some may be incomplete or still under development. The editor is also included as an ongoing project and is not yet complete.

## Highlights

- **DirectX 11 rendering** hosted inside a WPF application.
- **Sprites and textures**, including sprite sheets, animation and generated/composed textures.
- **2D rendering primitives**, including sprites, lines and triangles.
- **Camera and viewport handling** for scaling and positioning the game world.
- **Game loop and object model** with scenes, game objects and time-step information.
- **Input** support for keyboard, mouse and gamepad.
- **Physics and collision** systems, including colliders and character-controller functionality.
- **Particle systems** with configurable emitters and presets.
- **Audio playback and processing** using NAudio, including equalisation, Fourier analysis, spectrum data, pitch shifting, chorus, echo and reverb.
- **UI components** for creating in-engine interface elements.
- **Tile maps** for grid-based worlds and levels.
- **Asset management** for loading and managing game resources.
- **Save/load support** for game and scene state.
- **TCP networking** with client, server and message classes.
- **Diagnostics and profiling** to help inspect engine performance.

The systems are intended to work together, while remaining useful individually where that makes sense. For example, the audio analyser and effects can also be used in a WPF music-player application rather than only in a game.

## Using the engine in a WPF application

A central part of the design is the ability to host the engine in an ordinary WPF window. Add the engine project or package to a WPF application, then place `DirectXControl` in the window's XAML.

```xml
<Window x:Class="MyGame.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:dx="clr-namespace:Jolly2DEngine.Rendering.DirectX;assembly=Jolly2DEngine"
        Title="My Jolly2D Application"
        Width="1280"
        Height="720">
    <Grid>
        <dx:DirectXControl x:Name="DirectX" />
    </Grid>
</Window>
```

Once the control is ready, create the high-level renderer and use it with a class derived from `JollyGame`:

```csharp
await DirectX.WaitForReadyAsync();

DirectXJollyRenderer renderer = DirectX.CreateJollyRenderer();
MyGame game = new(renderer);
game.Start();
```

`JollyGame` runs its update loop on a dedicated game thread. WPF controls should remain on the WPF UI thread; use an appropriate thread-safe communication method when the game and WPF interface need to exchange information.

For the complete setup sequence, lifecycle details and working examples, see the [Project Setup and First Game guide](Documentation/docs/SETUP_AND_USING_THE_ENGINE.md).

## Demos and editor

The repository includes demonstration projects intended to show how the engine's systems can be used in practical applications. They provide examples to explore, learn from and build upon. **The demos are works in progress: some features or projects may be incomplete, experimental or subject to change.**

An **editor project is also included** as part of the wider Jolly2D tooling. It is still under development and **is not yet complete**; expect missing features and changes as development continues.

These projects are provided as examples and development tools, rather than being presented as finished commercial applications. The engine can also be referenced directly by your own WPF application without using the demos or editor.

## Technology and requirements

| Item | Details |
|---|---|
| Framework | .NET 10 for Windows (`net10.0-windows`) |
| UI integration | WPF, with Windows Forms interop enabled by the engine project |
| Graphics | Direct3D 11 / DXGI through Vortice |
| Audio | NAudio |
| Platform | x64 |

Main package dependencies:

- [Vortice.Direct3D11](https://www.nuget.org/packages/Vortice.Direct3D11) 3.8.1
- [Vortice.DXGI](https://www.nuget.org/packages/Vortice.DXGI) 3.8.1
- [NAudio](https://www.nuget.org/packages/NAudio) 3.1.0

The engine project includes the compiled DirectX shader files required by its renderer. Keep these files available when building, packaging or publishing applications that use the engine.

## Documentation

The developer documentation is in the [`Documentation`](Documentation/README.md) folder.

Recommended starting points:

1. [Project setup and first game](Documentation/docs/SETUP_AND_USING_THE_ENGINE.md) â€” add the engine to a WPF application and get a game running.
2. [Subsystem guide](Documentation/docs/SUBSYSTEM_GUIDE.md) â€” an overview of the engine systems and how they fit together.
3. [Tile-map guide](Documentation/docs/TILE_MAP_GUIDE.md) â€” working with tile maps.
4. [Complete API reference](Documentation/docs/API_REFERENCE.md) â€” public API declarations organised by subsystem.
5. [API usage reference](Documentation/docs/API_USAGE_REFERENCE.md) â€” methods, constructors and parameters.
6. [Enums reference](Documentation/docs/ENUMS.md) â€” public enumerations.

## Design goals

Jolly2DEngine is intended to be:

- **Reusable:** build the engine once and use it in multiple WPF applications.
- **Practical:** provide useful systems for games, tools, simulations and visual applications.
- **Extensible:** allow applications to build their own behaviours and workflows on top of the engine.
- **Inspectable:** provide diagnostics and documentation to make development easier.

The engine is an evolving project. Documentation and examples will be updated as the implementation develops.

## Building

Open `Jolly2DEngine.csproj` in Visual Studio 2026 or a compatible .NET 10 development environment, restore the NuGet packages and build for **x64**.

For application integration, follow the [setup guide](Documentation/docs/SETUP_AND_USING_THE_ENGINE.md), which includes the WPF control, renderer initialisation and game lifecycle.

## License

Add the repository's chosen licence here before publishing or redistributing the project. No licence terms are implied by this README.

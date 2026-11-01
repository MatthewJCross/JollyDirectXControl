# Jolly2DEngine Project Setup Guide

## 1. Requirements

The supplied engine targets **.NET 10 for Windows**, WPF and x64. It uses DirectX 11 through Vortice and NAudio for audio.

| Requirement | Version / setting |
|---|---|
| Target framework | `net10.0-windows` |
| Platform | `x64` recommended / required for the supplied renderer build |
| UI | WPF + Windows Forms enabled |
| DirectX | Direct3D 11 / DXGI through Vortice 3.8.1 |
| Audio | NAudio 3.1.0 |

The engine project itself already declares:

```xml
<TargetFramework>net10.0-windows</TargetFramework>
<UseWPF>true</UseWPF>
<UseWindowsForms>true</UseWindowsForms>
<PlatformTarget>x64</PlatformTarget>
```

If you consume the NuGet package, prefer an x64 WPF application so the native graphics stack and the engine's x64 assumptions match.

## 2. Create the game project

Create a WPF application targeting .NET 10 for Windows, then reference `Jolly2DEngine` either as a project reference or NuGet package.

For a local project reference:

```xml
<ItemGroup>
  <ProjectReference Include="..\Jolly2DEngine\Jolly2DEngine.csproj" />
</ItemGroup>
```

For a package reference:

```xml
<ItemGroup>
  <PackageReference Include="Jolly2DEngine" Version="1.2.0" />
</ItemGroup>
```

The engine package carries its DirectX shader bytecode as content. Do not remove the shader content from the published package.

## 3. Add the DirectX control to the main window

`DirectXControl` is the WPF host for the engine's DirectX renderer.

`MainWindow.xaml`:

```xml
<Window x:Class="MyGame.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:dx="clr-namespace:Jolly2DEngine.Rendering.DirectX;assembly=Jolly2DEngine"
        Title="My Jolly2D Game"
        Width="1280"
        Height="720">
    <Grid>
        <dx:DirectXControl x:Name="DirectX" />
    </Grid>
</Window>
```

The control creates/owns the low-level DirectX renderer. Wait for it to be ready before creating resources that require the DirectX device.

```csharp
private async void Window_Loaded(object sender, RoutedEventArgs e)
{
    await DirectX.WaitForReadyAsync();
    // Safe place to create DirectX-backed resources.
}
```

## 4. Create the Jolly renderer

```csharp
DirectXJollyRenderer renderer = DirectX.CreateJollyRenderer();
```

The renderer owns a `Camera2D` and exposes the higher-level Jolly rendering API.

```csharp
renderer.Camera.SetViewport((float)DirectX.ActualWidth, (float)DirectX.ActualHeight);
renderer.ClearColor = Colors.Black;
renderer.LineGlowEnabled = true;
renderer.LineGlowWidth = 2.0f;
renderer.LineGlowStrength = 1.0f;
renderer.LineGlowFalloff = 2.0f;
```

## 5. Create the game loop

Derive from `JollyGame` and override `OnInitialize`, `OnUpdate`, and optionally `OnShutdown`.

```csharp
public sealed class MyGame : JollyGame
{
    private readonly DirectXJollyRenderer _renderer;

    public MyGame(DirectXJollyRenderer renderer)
    {
        _renderer = renderer;
        GameLoopFPS = 240.0;
        RenderFPS = 120.0;
    }

    protected override void OnInitialize()
    {
        // Create scenes and game objects here.
    }

    protected override void OnUpdate(JollyGameTime gameTime)
    {
        // Game logic runs here.
    }

    protected override void OnShutdown()
    {
        // Release game-specific resources here.
    }
}
```

Start and stop it from the WPF lifetime:

```csharp
private MyGame? _game;

private void Window_Loaded(object sender, RoutedEventArgs e)
{
    _game = new MyGame(DirectX.CreateJollyRenderer());
    _game.Start();
}

private void Window_Closed(object? sender, EventArgs e)
{
    _game?.Stop();
    _game?.Dispose();
}
```

### Important threading rule

`JollyGame.Start()` creates a dedicated **Jolly2D Game Thread**. `OnUpdate` and scene updates therefore run on that thread, not the WPF UI thread. Avoid directly modifying WPF controls from `OnUpdate`.

The DirectX renderer has its own render-side processing. Treat WPF objects as UI-thread owned and use engine state/events for communication between game and UI.

## 6. Create a scene

A scene derives from `JollyScene`.

```csharp
public sealed class GameScene : JollyScene
{
    public override void OnEnter()
    {
        // Scene became active.
    }

    public override void Initialize()
    {
        base.Initialize();
        // Create or initialise scene objects.
    }

    public override void OnExit()
    {
        // Scene is leaving the active state.
    }
}
```

Add it through `JollySceneManager`:

```csharp
GameScene scene = new();
scene.Name = "Game";
Scenes.Add(scene);
Scenes.SetActive(scene);
```

The exact scene-manager operations available in your source are listed in `API_REFERENCE.md` under `Scene/JollySceneManager.cs`.

## 7. Create textures and sprites

Use `DirectXJollyRenderer.LoadTexture` for file-backed textures:

```csharp
JollyTexture texture = _renderer.LoadTexture("Assets/Player.png");
JollySprite sprite = _renderer.CreateSprite(texture);
sprite.Position = new JollyVector2(100.0f, 100.0f);
sprite.Scale = new JollyVector2(2.0f, 2.0f);
sprite.Color = JollyColor.White;
```

For generated graphics:

```csharp
JollyTexture white = _renderer.CreateSolidTexture(255, 255, 255);
JollyTexture circle = _renderer.CreateCircleTexture(0, 255, 255, 255, 32);
```

## 8. Use `JollySpriteObject`

`JollySpriteObject` wraps a `JollySprite` in the scene/game-object lifecycle and keeps the sprite synchronized with `Position`, `Scale`, `Rotation`, and `Visible`.

```csharp
JollySpriteObject player = new(_renderer.CreateSprite(texture));
player.Name = "Player";
player.Position = new JollyVector2(200.0f, 500.0f);
player.SetRectangleCollider(player.Sprite.Width, player.Sprite.Height);
scene.Add(player);
```

## 9. Input

Attach the engine input system to the WPF window:

```csharp
_game.Input.Attach(this);
```

Keyboard:

```csharp
if (_game.Input.IsKeyDown(Key.A))
{
    player.Position = new JollyVector2(player.Position.X - 200.0f * (float)_game.GameTime.DeltaSeconds, player.Position.Y);
}

if (_game.Input.IsKeyPressed(Key.Space))
{
    // One-shot press.
}
```

Mouse:

```csharp
System.Windows.Point mouse = _game.Input.MousePosition;
Vector2 world = _game.Input.GetMouseWorldPosition(_renderer.Camera);
```

Gamepads are exposed through `Input.Gamepads`; button and analogue-stick helpers are provided by `JollyGamepad`.

Detach when the window is no longer using the input source:

```csharp
_game.Input.Detach(this);
```

## 10. Camera

The renderer exposes `renderer.Camera`.

```csharp
renderer.Camera.Position = new Vector2(400.0f, 300.0f);
renderer.Camera.Zoom = 1.0f;
renderer.Camera.Rotation = 0.0f;
renderer.Camera.Follow(player.Position);
```

For smooth following:

```csharp
renderer.Camera.FollowSmooth(player.Position, (float)gameTime.DeltaSeconds);
```

World/screen conversion:

```csharp
Vector2 screen = renderer.Camera.WorldToScreen(worldPosition);
Vector2 world = renderer.Camera.ScreenToWorld(screenPosition);
```

## 11. Physics

Enable physics on a game object and give it a collider:

```csharp
player.UsePhysics = true;
player.UseGravity = true;
player.Gravity = new JollyVector2(0.0f, 1200.0f);
player.SetRectangleCollider(32.0f, 48.0f);
```

Circle collider:

```csharp
player.SetCircleCollider(16.0f);
```

Trigger:

```csharp
player.SetTrigger(true);
```

Collision layers:

```csharp
player.SetCollisionLayers(
    JollyCollisionLayer.Layer0,
    JollyCollisionLayer.Layer1 | JollyCollisionLayer.Layer2);
```

Collision callbacks are overridden on the game object:

```csharp
public override void OnCollisionEnter(JollyGameObject other)
{
    // Physical collision started.
}

public override void OnTriggerEnter(JollyGameObject other)
{
    // Trigger overlap started.
}
```

## 12. Character controller

```csharp
JollyCharacterController controller = player.AddCharacterController();
controller.MaxMoveSpeed = 300.0f;
controller.JumpVelocity = 450.0f;

controller.Move(horizontalInput);
controller.SetJumpHeld(jumpHeld);

if (jumpPressed)
{
    controller.RequestJump();
}
```

The controller provides coyote time and jump buffering. `JollyGameObject.IsGrounded` is updated by collision resolution.

## 13. Animations

Create a `JollySpriteSheet`, then a `JollyAnimation` describing the frame range and playback rate. Add it to the sprite and play it using the sprite's animation API.

```csharp
JollyAnimation animation = new(
    "Run",
    sheet,
    startFrame: 0,
    frameCount: 8,
    fps: 12.0,
    loop: true);
```

Subscribe to completion when using non-looping animations:

```csharp
sprite.AnimationCompleted += (sender, name) =>
{
    // Animation named 'name' finished.
};
```

See `Animation/JollyAnimation.cs` and `Graphics/JollySprite.cs` in the API reference for the complete animation API.

## 14. Audio

Create a `JollyAudioManager`, load effects, then play them by name:

```csharp
JollyAudioManager audio = new();
audio.LoadSound("Shoot", "Assets/Audio/Shoot.wav");
audio.PlaySound("Shoot");
```

Music is streamed from its path:

```csharp
audio.PlayMusic("Assets/Audio/Music.wav", loop: true);
audio.PauseMusic();
audio.ResumeMusic();
audio.StopMusic();
```

Volume is normalised to `0..1`:

```csharp
audio.MasterVolume = 1.0f;
audio.MusicVolume = 0.75f;
audio.SfxVolume = 1.0f;
audio.Muted = false;
```

Dispose the manager during shutdown.

## 15. Asset manager

For larger projects use `JollyAssetManager` to standardise paths and reference-count textures.

```csharp
JollyAssetManager assets = new(renderer, audio, "Assets");
JollyTexture playerTexture = assets.LoadTexture("Player.png");
```

The default asset folders are:

- `Textures`
- `Audio`
- `Fonts`
- `Maps`
- `Animations`

Use `AcquireTexture` when you want an explicit reference-counted handle and dispose the returned `JollyAssetHandle<T>` when finished.

## 16. Tile maps

`JollyTileMap` stores tile occupancy/data and `JollyTileMapRenderer` renders it. A scene can associate a tile map with:

```csharp
scene.SetTileMap(tileMap, worldX, worldY);
```

The physics system uses solid tiles during collision resolution for physics-enabled objects.

## 17. Particles

Use `JollyParticleSystem` and `JollyParticleEmitter` for effects. The built-in presets include:

- `Sparks`
- `Smoke`
- `Fire`
- `Explosion`

Emitter shapes include point, circle, rectangle and line. Configure lifetime, speed, size, direction, spread, gravity, drag, rotation speed, colour and opacity through the emitter/preset properties.

## 18. UI

The UI system is separate from world sprites. `JollyUIManager` owns `JollyUIElement` instances and handles hover/press state using screen-space mouse coordinates.

Available controls include:

- `JollyUIPanel`
- `JollyUIText`
- `JollyUIButton`
- `JollyUISlider`
- `JollyUIProgressBar`
- `JollyUI9Slice`

Use the element layout/anchor APIs rather than manually converting every control position from world coordinates.

## 19. Saving

Capture a scene:

```csharp
JollySceneSaveState state = JollySceneSaveState.Capture(scene);
```

Persist arbitrary game state with `JollySaveManager`:

```csharp
JollySaveManager saves = new();
saves.SaveGame("slot1", new MySaveData { Level = 3, Score = 12500 }, gameVersion: 1);

MySaveData loaded = saves.LoadGame<MySaveData>("slot1", expectedGameVersion: 1);
```

Use `TryLoadGame` when a missing or invalid save is expected and should not throw.

## 20. Diagnostics and profiling

`JollyGame` creates `Diagnostics` automatically.

```csharp
JollyProfile? profile = game.Diagnostics.RegisterProfile("AI");

using (game.Diagnostics.MeasureUpdate())
{
    using (new JollyProfileScope(profile))
    {
        UpdateAI();
    }
}
```

The diagnostics system tracks update/render timing and entity counts and exposes snapshots/formatters for displaying the information.

## 21. Line rendering

For Beam Rider, Tron-style trails and other vector effects, submit `JollyLineSnapshot` instances as part of a `JollyRenderFrame`.

The renderer supports configurable line glow:

```csharp
renderer.LineGlowEnabled = true;
renderer.LineGlowWidth = 2.0f;
renderer.LineGlowStrength = 1.0f;
renderer.LineGlowFalloff = 2.0f;
```

`JollyLineSnapshot` supports separate start/end thickness values, allowing tapered projectiles and trails.

## 22. Networking

`JollyNetworkServer` listens for clients and raises connection/message events. `JollyNetworkClient` connects to a server. `JollyNetworkMessage` supports raw bytes or UTF-8 text.

Server:

```csharp
JollyNetworkServer server = new();
server.ClientConnected += connection => { };
server.MessageReceived += (connection, message) => { };
server.Start(7777);
server.BroadcastText("Welcome");
```

Client:

```csharp
JollyNetworkClient client = new();
client.MessageReceived += message => { };
await client.ConnectAsync("127.0.0.1", 7777);
client.SendText("Hello");
```

Always stop/dispose networking objects during application shutdown.

## 23. Low-level DirectX API

Most games should use `DirectXJollyRenderer` rather than `DirectXRenderer` directly. The low-level renderer exists for engine-level integrations and exposes:

- device/window initialisation
- render-loop control
- image/texture creation
- frame submission
- resize
- image filtering
- stretch modes
- VSync/FPS control
- line glow
- render statistics

Use `DirectXControl.CreateJollyRenderer()` for the normal application path.

## 24. Typical project structure

```text
MyGame/
  Assets/
    Textures/
    Audio/
    Maps/
    Animations/
  Scenes/
    MainMenuScene.cs
    GameScene.cs
  Objects/
    Player.cs
    Enemy.cs
    Projectile.cs
  UI/
    Hud.cs
  MainWindow.xaml
  MainWindow.xaml.cs
  MyGame.cs
```

Keep engine-independent gameplay code in scene/game-object classes. Keep WPF code in the window/UI layer and keep DirectX resource creation behind the renderer/asset layer.

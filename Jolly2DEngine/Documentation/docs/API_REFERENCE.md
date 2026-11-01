# Jolly2DEngine API Reference

This is the public API inventory for the engine source supplied with this documentation. Every public declaration is listed with its source file. Use the subsystem guides for explanations and examples.

## How to read this reference

- **Properties/fields** are values you configure or read.
- **Methods** perform an engine operation; parameters are shown exactly as declared by the source.
- **Events** are callbacks raised by the engine.
- **Constructors** show the values required to create the type.
- `CancellationToken cancellationToken = default` means cancellation is optional.

## `Animation/JollyAnimation.cs`

```csharp
public sealed class JollyAnimation
```
```csharp
public string Name { get; }  public JollySpriteSheet Sheet { get; }  public int StartFrame { get; }  public int FrameCount { get; }  public double FPS { get; }  public bool Loop { get; }  public JollyAnimation(string name, JollySpriteSheet sheet, int startFrame, int frameCount, double fps, bool loop = true)
```
```csharp
public JollySpriteSheetFrame GetFrame(int animationFrame)
```

## `Assets/JollyAssetHandle.cs`

```csharp
public sealed class JollyAssetHandle<T> : IDisposable where T : class
```
```csharp
public T Asset { get; }  public bool IsReleased => _release == null;
```
```csharp
public void Dispose()
```

## `Assets/JollyAssetManager.cs`

```csharp
public sealed class JollyAssetManager : IDisposable
```
```csharp
public const string TexturesFolder = "Textures";
```
```csharp
public const string AudioFolder = "Audio";
```
```csharp
public const string FontsFolder = "Fonts";
```
```csharp
public const string MapsFolder = "Maps";
```
```csharp
public const string AnimationsFolder = "Animations";
```
```csharp
public JollyAssetManager(DirectXJollyRenderer renderer, JollyAudioManager? audio = null, string? rootPath = null)
```
```csharp
public string RootPath { get; }  public string TexturesPath => Path.Combine(RootPath, TexturesFolder);
```
```csharp
public string AudioPath => Path.Combine(RootPath, AudioFolder);
```
```csharp
public string FontsPath => Path.Combine(RootPath, FontsFolder);
```
```csharp
public string MapsPath => Path.Combine(RootPath, MapsFolder);
```
```csharp
public string AnimationsPath => Path.Combine(RootPath, AnimationsFolder);
```
```csharp
public JollyTexture LoadTexture(string assetName)
```
```csharp
public async Task<JollyTexture> LoadTextureAsync(string assetName, CancellationToken cancellationToken = default)
```
```csharp
public JollyAssetHandle<JollyTexture> AcquireTexture(string assetName)
```
```csharp
public void ReleaseTexture(string assetName)
```
```csharp
public bool IsTextureLoaded(string assetName)
```
```csharp
public void LoadSound(string assetName)
```
```csharp
public void PlaySound(string assetName)
```
```csharp
public void PlayMusic(string assetName, bool loop = true)
```
```csharp
public void UnloadSound(string assetName)
```
```csharp
public string GetTexturePath(string assetName) => ResolveAssetPath(TexturesFolder, NormalizeAssetName(assetName));
```
```csharp
public string GetAudioPath(string assetName) => ResolveAssetPath(AudioFolder, NormalizeAssetName(assetName));
```
```csharp
public string GetFontPath(string assetName) => ResolveAssetPath(FontsFolder, NormalizeAssetName(assetName));
```
```csharp
public string GetMapPath(string assetName) => ResolveAssetPath(MapsFolder, NormalizeAssetName(assetName));
```
```csharp
public string GetAnimationPath(string assetName) => ResolveAssetPath(AnimationsFolder, NormalizeAssetName(assetName));
```
```csharp
public bool AssetExists(string category, string assetName)
```
```csharp
public void Validate(params JollyAssetRequirement[] requirements)
```
```csharp
public void Dispose()
```
```csharp
public TextureEntry(JollyTexture texture)
```
```csharp
public JollyTexture Texture { get; } public int ReferenceCount { get; set; } } }  public sealed record JollyAssetRequirement(string Category, string Name);
```

## Graphics and Rendering API pages

- [JollyTextureComposer](api/Graphics_JollyTextureComposer.md) — CPU-side alpha composition and text overlays
- [TvStaticGenerator](api/Graphics_TvStaticGenerator.md) and [TvStaticColourMode](api/Graphics_TvStaticColourMode.md) — generated animated static
- [JollyTriangleSnapshot](api/Graphics_JollyTriangleSnapshot.md) — filled triangle submissions
- [SpriteFontRenderer](api/Graphics_SpriteFontRenderer.md) — bitmap-font atlas rendering
- [JollySpriteSheet](api/Rendering_JollySpriteSheet.md) — fixed-cell frame indexing
- [JollyTextureManager](api/Rendering_JollyTextureManager.md) — path-based texture cache
- [DirectXImage](api/Rendering_DirectX_DirectXImage.md) — CPU-side image data
- [JollyRenderSubmission](api/Rendering_JollyRenderSubmission.md) — frame/camera submission wrapper

For end-to-end behaviour and examples, see the [Graphics and Rendering guide](GRAPHICS_AND_RENDERING_GUIDE.md).

## Audio subsystem API pages

The manager declarations below cover playback control. The additional audio APIs now have dedicated pages:

- [JollyAudioEffects](api/Audio_JollyAudioEffects.md)
- [JollyEqualiser](api/Audio_JollyEqualiser.md) and [JollyEqualiserBand](api/Audio_JollyEqualiserBand.md)
- [JollyAudioAnalyser](api/Audio_JollyAudioAnalyser.md) and [JollySpectrumData](api/Audio_JollySpectrumData.md)
- [JollyFourierTransform](api/Audio_JollyFourierTransform.md) and [JollyFourierWindow](api/Audio_JollyFourierWindow.md)
- [JollyPitchShifter](api/Audio_JollyPitchShifter.md), [JollyChorus](api/Audio_JollyChorus.md), [JollyEcho](api/Audio_JollyEcho.md), and [JollyReverb](api/Audio_JollyReverb.md)
- [JollyReverbPreset](api/Audio_JollyReverbPreset.md) and [JollyBiquadFilter](api/Audio_JollyBiquadFilter.md)
- [Analyser/effect sample providers](api/Audio_JollyAnalyserSampleProvider.md), [equaliser provider](api/Audio_JollyEqualiserSampleProvider.md), [pitch provider](api/Audio_JollyPitchShiftSampleProvider.md), [chorus provider](api/Audio_JollyChorusSampleProvider.md), [echo provider](api/Audio_JollyEchoSampleProvider.md), [reverb provider](api/Audio_JollyReverbSampleProvider.md)

## `Audio/JollyAudioManager.cs`

```csharp
public sealed class JollyAudioManager : IDisposable
```
```csharp
public float MasterVolume
```
```csharp
public float MusicVolume
```
```csharp
public float SfxVolume
```
```csharp
public bool Muted
```
```csharp
public bool IsMusicPlaying => _musicOutput?.PlaybackState == PlaybackState.Playing;
```
```csharp
public string? CurrentMusic => _musicPath;
```
```csharp
public void LoadSound(string name, string path)
```
```csharp
public void UnloadSound(string name)
```
```csharp
public void PlaySound(string name)
```
```csharp
public void StopAllSounds()
```
```csharp
public void PlayMusic(string path, bool loop = true)
```
```csharp
public void PauseMusic()
```
```csharp
public void ResumeMusic()
```
```csharp
public void StopMusic()
```
```csharp
public void ClearSounds()
```
```csharp
public void Dispose()
```
```csharp
public CachedSound Sound { get; }  public WaveOut Output { get; }  public SoundInstance(CachedSound sound, float volume)
```
```csharp
public void Dispose()
```
```csharp
public WaveFormat WaveFormat { get; }  public float[] AudioData { get; }  public CachedSound(string audioFileName)
```
```csharp
public WaveFormat WaveFormat => _sound.WaveFormat;
```
```csharp
public CachedSoundSampleProvider(CachedSound sound)
```
```csharp
public int Read(Span<float> buffer)
```
```csharp
public LoopStream(WaveStream source)
```
```csharp
public override WaveFormat WaveFormat => _source.WaveFormat;
```
```csharp
public override long Length => _source.Length;
```
```csharp
public override long Position
```
```csharp
public override int Read(byte[] buffer, int offset, int count)
```

## `Core/JollyGame.cs`

```csharp
public abstract class JollyGame : IDisposable
```
```csharp
public JollySceneManager Scenes { get; }  public JollyInput Input { get; }  public JollyGameTime GameTime { get; } = new JollyGameTime();
```
```csharp
public JollyDiagnostics Diagnostics { get; }  public double GameLoopFPS
```
```csharp
public double GameLoopActualFPS => Volatile.Read(ref _gameLoopActualFPS);
```
```csharp
public double GameFrameTime => GameTime.FrameTime;
```
```csharp
public double RenderFPS
```
```csharp
public bool IsRunning => _running;
```
```csharp
public void Start()
```
```csharp
public void Stop()
```

## `Core/JollyGameObject.cs`

```csharp
public abstract class JollyGameObject
```
```csharp
public string SaveId { get; set; } = Guid.NewGuid().ToString("N");
```
```csharp
public string Name { get; set; } = string.Empty;
```
```csharp
public string Tag { get; set; } = string.Empty;
```
```csharp
public bool Active { get; set; } = true;
```
```csharp
public JollyVector2 Position { get; set; } public JollyVector2 Scale { get; set; } = JollyVector2.One;
```
```csharp
public float Rotation { get; set; } public bool Visible { get; set; } = true;
```
```csharp
public JollyVector2 Velocity { get; set; } public JollyVector2 Acceleration { get; set; } public JollyVector2 Gravity { get; set; } public float Drag { get; set; } public bool UsePhysics { get; set; } public bool UseGravity { get; set; } public bool IsGrounded { get; set; } public bool WasGrounded { get; set; } public float GroundFriction { get; set; } = 12.0f;
```
```csharp
public float AirFriction { get; set; } = 0.0f;
```
```csharp
public float Restitution { get; set; } = 0.0f;
```
```csharp
public JollyCollider? Collider { get; private set; } private JollyCharacterController? _characterController;
```
```csharp
public JollyCharacterController? CharacterController => _characterController;
```
```csharp
public bool IsDestroyed { get; private set; } internal JollyScene? Scene { get; set; }  public virtual void Initialize()
```
```csharp
public virtual void Update(JollyGameTime gameTime)
```
```csharp
public virtual void Draw()
```
```csharp
public virtual void Shutdown()
```
```csharp
public void SetCollider(JollyCollider collider)
```
```csharp
public void RemoveCollider()
```
```csharp
public virtual void OnCollisionEnter(JollyGameObject other)
```
```csharp
public virtual void OnCollisionStay(JollyGameObject other)
```
```csharp
public virtual void OnCollisionExit(JollyGameObject other)
```
```csharp
public virtual void OnTriggerEnter(JollyGameObject other)
```
```csharp
public virtual void OnTriggerStay(JollyGameObject other)
```
```csharp
public virtual void OnTriggerExit(JollyGameObject other)
```
```csharp
public JollyCharacterController AddCharacterController()
```
```csharp
public void Destroy()
```

## `Core/JollyGameTime.cs`

```csharp
public sealed class JollyGameTime
```
```csharp
public double TotalSeconds { get; internal set; } public double DeltaSeconds { get; internal set; } public double DeltaMilliseconds => DeltaSeconds * 1000.0;
```
```csharp
public double FPS { get; internal set; } public double FrameTime => DeltaMilliseconds;
```
```csharp
public long FrameCount { get; internal set; } } }
```

## `Core/JollySpriteObject.cs`

```csharp
public class JollySpriteObject : JollyGameObject
```
```csharp
public JollySprite Sprite { get; } public bool IsColliding => _collisions.Count > 0;
```
```csharp
public bool IsTriggered => _triggers.Count > 0;
```
```csharp
public IReadOnlyCollection<JollyGameObject> Collisions => _collisions;
```
```csharp
public IReadOnlyCollection<JollyGameObject> Triggers => _triggers;
```
```csharp
public JollyCollisionLayer CollisionLayer => Collider?.Layer ?? JollyCollisionLayer.None;
```
```csharp
public JollyCollisionLayer CollisionMask => Collider?.Mask ?? JollyCollisionLayer.None;
```
```csharp
public JollySpriteObject(JollySprite sprite)
```
```csharp
public override void Initialize()
```
```csharp
public override void Update(JollyGameTime gameTime)
```
```csharp
public override void Draw()
```
```csharp
public override void Shutdown()
```
```csharp
public void EnableCollision(JollyCollisionMode mode = JollyCollisionMode.Bounds, byte alphaThreshold = 1)
```
```csharp
public void SetRectangleCollider(float width, float height)
```
```csharp
public void SetCircleCollider(float radius)
```
```csharp
public void SetPixelCollider(byte alphaThreshold = 1)
```
```csharp
public void SetCollisionLayers(JollyCollisionLayer layer, JollyCollisionLayer mask)
```
```csharp
public void SetTrigger(bool isTrigger = true)
```
```csharp
public void DisableCollision()
```
```csharp
public override void OnCollisionEnter(JollyGameObject other)
```
```csharp
public override void OnCollisionStay(JollyGameObject other)
```
```csharp
public override void OnCollisionExit(JollyGameObject other)
```
```csharp
public override void OnTriggerEnter(JollyGameObject other)
```
```csharp
public override void OnTriggerStay(JollyGameObject other)
```
```csharp
public override void OnTriggerExit(JollyGameObject other)
```

## `Diagnostics/JollyDiagnosticScope.cs`

```csharp
public struct JollyDiagnosticScope : IDisposable
```
```csharp
public void Dispose()
```

## `Diagnostics/JollyDiagnostics.cs`

```csharp
public sealed class JollyDiagnostics
```
```csharp
public void BeginFrame()
```
```csharp
public void EndFrame()
```
```csharp
public JollyDiagnosticScope MeasureUpdate()
```
```csharp
public JollyDiagnosticScope MeasureRender()
```
```csharp
public void SetEntityCount(int count)
```
```csharp
public void SetSpriteCount(int count)
```
```csharp
public void SetTextureCount(int count)
```
```csharp
public void SetRenderBatchCount(int count)
```
```csharp
public void SetRenderDrawCallCount(int count)
```
```csharp
public void SetRenderMaxBatchSize(int count)
```
```csharp
public void SetConnectedClients(int count)
```
```csharp
public void RecordNetworkMessageSent(int byteCount)
```
```csharp
public void RecordNetworkMessageReceived(int byteCount)
```
```csharp
public void SetNetworkLatency(double milliseconds)
```
```csharp
public JollyProfile RegisterProfile(string name)
```
```csharp
public JollyProfileScope Profile(JollyProfile? profile)
```
```csharp
public JollyDiagnosticsSnapshot GetSnapshot()
```
```csharp
public void Reset()
```

## `Diagnostics/JollyDiagnosticsFormatter.cs`

```csharp
public static class JollyDiagnosticsFormatter
```
```csharp
public static string Format(JollyDiagnosticsSnapshot snapshot)
```

## `Diagnostics/JollyDiagnosticsSnapshot.cs`

```csharp
public sealed class JollyDiagnosticsSnapshot
```
```csharp
public sealed class JollyProfileSnapshot
```
```csharp
public string Name { get; init; } = string.Empty;
```
```csharp
public long CallCount { get; init; } public double LastMilliseconds { get; init; } public double TotalMilliseconds { get; init; } public double AverageMilliseconds { get; init; } public double MaximumMilliseconds { get; init; } } }
```

## `Diagnostics/JollyDiagnosticsViewModel.cs`

```csharp
public sealed class JollyDiagnosticsViewModel : INotifyPropertyChanged
```
```csharp
public event PropertyChangedEventHandler? PropertyChanged;
```
```csharp
public double FPS
```
```csharp
public double FrameTimeMilliseconds
```
```csharp
public double UpdateTimeMilliseconds
```
```csharp
public double RenderTimeMilliseconds
```
```csharp
public long FrameCount
```
```csharp
public long UpdateCount
```
```csharp
public long RenderCount
```
```csharp
public int EntityCount
```
```csharp
public int SpriteCount
```
```csharp
public int TextureCount
```
```csharp
public int RenderBatchCount
```
```csharp
public int RenderDrawCallCount
```
```csharp
public int RenderMaxBatchSize
```
```csharp
public int ConnectedClients
```
```csharp
public long NetworkMessagesSent
```
```csharp
public long NetworkMessagesReceived
```
```csharp
public long NetworkBytesSent
```
```csharp
public long NetworkBytesReceived
```
```csharp
public double NetworkLatencyMilliseconds
```
```csharp
public IReadOnlyDictionary<string, JollyProfileSnapshot> Profiles
```
```csharp
public void Refresh(JollyDiagnostics diagnostics)
```
```csharp
public string GetSummary()
```
```csharp
public string GetRenderSummary()
```
```csharp
public JollyProfileSnapshot? GetProfile(string name)
```
```csharp
public long GetProfileCallCount(string name)
```

## `Diagnostics/JollyProfile.cs`

```csharp
public sealed class JollyProfile
```
```csharp
public string Name { get; }  internal long CallCount;
```

## `Diagnostics/JollyProfileScope.cs`

```csharp
public struct JollyProfileScope : IDisposable
```
```csharp
public void Dispose()
```

## `Graphics/JollyLineSnapshot.cs`

```csharp
public readonly struct JollyLineSnapshot(Vector2 start, Vector2 end, JollyColor color, float thickness, float startThickness = 1.0f, float endThickness = 1.0f)
```
```csharp
public Vector2 Start { get; } = start;
```
```csharp
public Vector2 End { get; } = end;
```
```csharp
public JollyColor Color { get; } = color;
```
```csharp
public float Thickness { get; } = thickness;
```
```csharp
public float StartThickness { get; } = startThickness;
```
```csharp
public float EndThickness { get; } = endThickness;
```

## `Graphics/JollyRenderFrame.cs`

```csharp
public sealed class JollyRenderFrame
```
```csharp
public IReadOnlyList<JollySpriteSnapshot> Sprites => _sprites;
```
```csharp
public IReadOnlyList<JollyLineSnapshot> Lines => _lines;
```
```csharp
public JollyRenderFrame(IEnumerable<JollySprite> sprites, IEnumerable<JollyLineSnapshot>? lines = null)
```

## `Graphics/JollyRenderTarget.cs`

```csharp
public sealed class JollyRenderTarget : IDisposable
```
```csharp
public int Width { get; } public int Height { get; }  internal JollyRenderTarget(ID3D11Texture2D texture, ID3D11RenderTargetView renderTargetView, ID3D11ShaderResourceView shaderResourceView, int width, int height)
```
```csharp
public void Dispose()
```

## `Graphics/JollyRenderer.cs`

```csharp
public abstract class JollyRenderer
```
```csharp
public abstract JollyTexture LoadTexture(string filePath);
```
```csharp
public abstract JollySprite CreateSprite(JollyTexture texture);
```
```csharp
public JollySprite CreateSprite(string filePath)
```

## `Graphics/JollySprite.cs`

```csharp
public sealed class JollySprite
```
```csharp
public event Action<JollySprite, string>? AnimationCompleted;
```
```csharp
public JollyCollisionMode CollisionMode { get; set; } = JollyCollisionMode.None;
```
```csharp
public byte CollisionAlphaThreshold { get; set; } = 1;
```
```csharp
public string Name { get; set; } = string.Empty;
```
```csharp
public JollyTexture Texture { get; }  public JollyVector2 Position
```
```csharp
public JollyVector2 Size
```
```csharp
public JollyVector2 Scale
```
```csharp
public float X
```
```csharp
public float Y
```
```csharp
public float Width
```
```csharp
public float Height
```
```csharp
public float Rotation
```
```csharp
public float Opacity
```
```csharp
public JollyColor Color { get; set; } = JollyColor.White;
```
```csharp
public bool Visible
```
```csharp
public int Layer
```
```csharp
public bool FlipHorizontal
```
```csharp
public bool FlipVertical
```
```csharp
public float SourceX
```
```csharp
public float SourceY
```
```csharp
public float SourceWidth
```
```csharp
public float SourceHeight
```
```csharp
public bool AnimationPlaying => _animationPlaying;
```
```csharp
public int AnimationFrame => _animationFrame;
```
```csharp
public float AnimationFPS => _animationFPS;
```
```csharp
public int AnimationFrameCount => _currentAnimation?.FrameCount ?? _animationSheet?.FrameCount ?? 0;
```
```csharp
public bool AnimationLoop
```
```csharp
public JollySpriteSheet? AnimationSheet => _animationSheet;
```
```csharp
public string? CurrentAnimation => _currentAnimation?.Name;
```
```csharp
public bool IsAnimationComplete
```
```csharp
public IReadOnlyDictionary<string, JollyAnimation> Animations => _animations;
```
```csharp
public void AddAnimation(JollyAnimation animation)
```
```csharp
public bool RemoveAnimation(string name)
```
```csharp
public bool HasAnimation(string name)
```
```csharp
public JollyAnimation? GetAnimation(string name)
```
```csharp
public void Play(string animationName)
```
```csharp
public void SetAnimation(JollySpriteSheet sheet, float framesPerSecond, bool loop = true)
```
```csharp
public void Play()
```
```csharp
public void Pause()
```
```csharp
public void Stop()
```
```csharp
public void SetAnimationFrame(int frame)
```
```csharp
public void SetFrame(JollySpriteSheetFrame frame, JollySpriteSheet sheet)
```

## `Graphics/JollySpriteSnapshot.cs`

```csharp
public readonly struct JollySpriteSnapshot
```
```csharp
public string Name { get; } public JollyTexture Texture { get; } public float X { get; } public float Y { get; } public float Width { get; } public float Height { get; } public float Rotation { get; } public float Opacity { get; } public JollyColor Color { get; init; } public bool Visible { get; } public int Layer { get; } public bool FlipHorizontal { get; } public bool FlipVertical { get; } public float SourceX { get; } public float SourceY { get; } public float SourceWidth { get; } public float SourceHeight { get; } public JollyVector2 Scale { get; }  internal JollySpriteSnapshot(JollySprite sprite)
```

## `Graphics/JollyTexture.cs`

```csharp
public sealed class JollyTexture
```
```csharp
public int Width { get; } public int Height { get; }  internal object RendererTexture { get; }  internal byte[] AlphaMask { get; }  internal JollyTexture(object rendererTexture, int width, int height, byte[] alphaMask)
```
```csharp
public byte GetAlpha(int x, int y)
```

## `Input/JollyGamepad.cs`

```csharp
public sealed class JollyGamepad
```
```csharp
public int Index { get; }  public bool IsConnected
```
```csharp
public float LeftStickX
```
```csharp
public float LeftStickY
```
```csharp
public float RightStickX
```
```csharp
public float RightStickY
```
```csharp
public float LeftTrigger
```
```csharp
public float RightTrigger
```
```csharp
public JollyGamepad(int index)
```
```csharp
public bool IsButtonDown(JollyGamepadButton button)
```
```csharp
public bool IsButtonPressed(JollyGamepadButton button)
```
```csharp
public bool IsButtonReleased(JollyGamepadButton button)
```
```csharp
public uint PacketNumber;
```
```csharp
public XInputGamepad Gamepad;
```
```csharp
public ushort Buttons;
```
```csharp
public byte LeftTrigger;
```
```csharp
public byte RightTrigger;
```
```csharp
public short ThumbLX;
```
```csharp
public short ThumbLY;
```
```csharp
public short ThumbRX;
```
```csharp
public short ThumbRY;
```

## `Input/JollyGamepadButton.cs`

```csharp
public enum JollyGamepadButton
```

## `Input/JollyInput.cs`

```csharp
public sealed class JollyInput
```
```csharp
public JollyKeyboard Keyboard => _keyboard;
```
```csharp
public JollyMouse Mouse => _mouse;
```
```csharp
public IReadOnlyList<JollyGamepad> Gamepads => _gamepads;
```
```csharp
public void Attach(Window window)
```
```csharp
public void Detach(Window window)
```
```csharp
public bool IsKeyDown(Key key)
```
```csharp
public bool IsKeyPressed(Key key)
```
```csharp
public bool IsKeyReleased(Key key)
```
```csharp
public double MouseX => _mouse.X;
```
```csharp
public double MouseY => _mouse.Y;
```
```csharp
public System.Windows.Point MousePosition => _mouse.Position;
```
```csharp
public int MouseWheel => _mouse.Wheel;
```
```csharp
public bool IsMouseButtonDown(JollyMouseButton button)
```
```csharp
public bool IsMouseButtonPressed(JollyMouseButton button)
```
```csharp
public bool IsMouseButtonReleased(JollyMouseButton button)
```
```csharp
public Vector2 GetMouseWorldPosition(Camera2D camera)
```

## `Input/JollyKeyboard.cs`

```csharp
public sealed class JollyKeyboard
```
```csharp
public void Attach(Window window)
```
```csharp
public void Detach(Window window)
```
```csharp
public bool IsKeyDown(Key key)
```
```csharp
public bool IsKeyPressed(Key key)
```
```csharp
public bool IsKeyReleased(Key key)
```

## `Input/JollyMouse.cs`

```csharp
public enum JollyMouseButton
```
```csharp
public sealed class JollyMouse
```
```csharp
public double X
```
```csharp
public double Y
```
```csharp
public System.Windows.Point Position
```
```csharp
public int Wheel
```
```csharp
public void Attach(Window window)
```
```csharp
public void Detach(Window window)
```
```csharp
public bool IsButtonDown(JollyMouseButton button)
```
```csharp
public bool IsButtonPressed(JollyMouseButton button)
```
```csharp
public bool IsButtonReleased(JollyMouseButton button)
```

## `Math/JollyColor.cs`

```csharp
public readonly struct JollyColor : IEquatable<JollyColor>
```
```csharp
public float R { get; } public float G { get; } public float B { get; } public float A { get; }  public JollyColor(float r, float g, float b, float a = 1.0f)
```
```csharp
public static JollyColor White => new(1.0f, 1.0f, 1.0f, 1.0f);
```
```csharp
public static JollyColor Black => new(0.0f, 0.0f, 0.0f, 1.0f);
```
```csharp
public static JollyColor Red => new(1.0f, 0.0f, 0.0f, 1.0f);
```
```csharp
public static JollyColor Green => new(0.0f, 1.0f, 0.0f, 1.0f);
```
```csharp
public static JollyColor Blue => new(0.0f, 0.0f, 1.0f, 1.0f);
```
```csharp
public static JollyColor Yellow => new(1.0f, 1.0f, 0.0f, 1.0f);
```
```csharp
public static JollyColor Transparent => new(0.0f, 0.0f, 0.0f, 0.0f);
```
```csharp
public static JollyColor FromBytes(byte r, byte g, byte b, byte a = 255)
```
```csharp
public bool Equals(JollyColor other)
```
```csharp
public override bool Equals(object? obj)
```
```csharp
public override int GetHashCode()
```
```csharp
public static bool operator ==(JollyColor left, JollyColor right)
```
```csharp
public static bool operator !=(JollyColor left, JollyColor right)
```

## `Math/JollyVector2.cs`

```csharp
public struct JollyVector2
```
```csharp
public float X;
```
```csharp
public float Y;
```
```csharp
public JollyVector2(float x, float y)
```
```csharp
public static JollyVector2 Zero => new JollyVector2(0.0f, 0.0f);
```
```csharp
public static JollyVector2 One => new JollyVector2(1.0f, 1.0f);
```
```csharp
public static JollyVector2 operator +(JollyVector2 left, JollyVector2 right)
```
```csharp
public static JollyVector2 operator -(JollyVector2 left, JollyVector2 right)
```
```csharp
public static JollyVector2 operator *(JollyVector2 value, float scalar)
```
```csharp
public static JollyVector2 operator /(JollyVector2 value, float scalar)
```
```csharp
public float Length()
```
```csharp
public float LengthSquared()
```
```csharp
public void Normalize()
```
```csharp
public static JollyVector2 Normalize(JollyVector2 value)
```

## `Networking/JollyNetworkClient.cs`

```csharp
public sealed class JollyNetworkClient : IDisposable
```
```csharp
public bool IsConnected => _connection?.IsConnected == true;
```
```csharp
public string Host { get; private set; } = string.Empty;
```
```csharp
public int Port { get; private set; }  public event Action? Connected;
```
```csharp
public event Action? Disconnected;
```
```csharp
public event Action<JollyNetworkMessage>? MessageReceived;
```
```csharp
public void Connect(string host, int port)
```
```csharp
public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
```
```csharp
public void Send(JollyNetworkMessage message)
```
```csharp
public void SendText(string text)
```
```csharp
public void Disconnect()
```
```csharp
public void Dispose()
```

## `Networking/JollyNetworkConnection.cs`

```csharp
public sealed class JollyNetworkConnection : IDisposable
```
```csharp
public string Id { get; } = Guid.NewGuid().ToString("N");
```
```csharp
public EndPoint? RemoteEndPoint { get; }  public bool IsConnected => !_disposed && _client.Connected;
```
```csharp
public event Action<JollyNetworkConnection, JollyNetworkMessage>? MessageReceived;
```
```csharp
public event Action<JollyNetworkConnection>? Disconnected;
```
```csharp
public void Send(JollyNetworkMessage message)
```
```csharp
public void SendText(string text)
```
```csharp
public void Disconnect()
```
```csharp
public void Dispose()
```

## `Networking/JollyNetworkMessage.cs`

```csharp
public sealed class JollyNetworkMessage
```
```csharp
public JollyNetworkMessage(byte[] payload)
```
```csharp
public byte[] Payload { get; }  public string Text => Encoding.UTF8.GetString(Payload);
```
```csharp
public static JollyNetworkMessage FromText(string text)
```

## `Networking/JollyNetworkServer.cs`

```csharp
public sealed class JollyNetworkServer : IDisposable
```
```csharp
public bool IsRunning => _listener != null;
```
```csharp
public int Port { get; private set; }  public IReadOnlyList<JollyNetworkConnection> Clients
```
```csharp
public event Action<JollyNetworkConnection>? ClientConnected;
```
```csharp
public event Action<JollyNetworkConnection>? ClientDisconnected;
```
```csharp
public event Action<JollyNetworkConnection, JollyNetworkMessage>? MessageReceived;
```
```csharp
public void Start(int port)
```
```csharp
public void Stop()
```
```csharp
public void Broadcast(JollyNetworkMessage message)
```
```csharp
public void BroadcastText(string text)
```
```csharp
public void Dispose()
```

## `Particles/JollyParticle.cs`

```csharp
public sealed class JollyParticle
```
```csharp
public System.Numerics.Vector2 Position { get; internal set; } public System.Numerics.Vector2 Velocity { get; internal set; } public System.Numerics.Vector2 Acceleration { get; internal set; } public float Drag { get; internal set; } public float Life { get; internal set; } public float MaximumLife { get; internal set; } public float Size { get; internal set; } public float StartSize { get; internal set; } public float EndSize { get; internal set; } public float Rotation { get; internal set; } public float RotationSpeed { get; internal set; } public float Opacity { get; internal set; } public JollyColor Color { get; internal set; } = JollyColor.White;
```
```csharp
public int Layer { get; internal set; } public bool IsAlive => Life > 0.0f;
```
```csharp
public float NormalizedLife
```
```csharp
public float Age
```

## `Particles/JollyParticleEmitter.cs`

```csharp
public sealed class JollyParticleEmitter
```
```csharp
public Vector2 Position { get; set; } public bool Enabled { get; set; } = true;
```
```csharp
public float ParticlesPerSecond { get; set; } = 20.0f;
```
```csharp
public float MinimumLife { get; set; } = 0.5f;
```
```csharp
public float MaximumLife { get; set; } = 1.0f;
```
```csharp
public float MinimumSize { get; set; } = 4.0f;
```
```csharp
public float MaximumSize { get; set; } = 8.0f;
```
```csharp
public float MinimumSpeed { get; set; } = 20.0f;
```
```csharp
public float MaximumSpeed { get; set; } = 60.0f;
```
```csharp
public float Direction { get; set; } public float Spread { get; set; } = MathF.PI * 2.0f;
```
```csharp
public Vector2 Gravity { get; set; } public float Drag { get; set; } public float MinimumRotationSpeed { get; set; } public float MaximumRotationSpeed { get; set; } public float Opacity { get; set; } = 1.0f;
```
```csharp
public JollyColor Color { get; internal set; } = JollyColor.White;
```
```csharp
public int Layer { get; set; } public JollyParticleEmitterShape Shape { get; set; } = JollyParticleEmitterShape.Point;
```
```csharp
public float ShapeWidth { get; set; } = 32.0f;
```
```csharp
public float ShapeHeight { get; set; } = 32.0f;
```
```csharp
public float ShapeRadius { get; set; } = 16.0f;
```
```csharp
public int Update(float deltaSeconds, List<JollyParticle> particles)
```
```csharp
public void Emit(int count, List<JollyParticle> particles)
```
```csharp
public void Reset()
```
```csharp
public void Burst(int count, List<JollyParticle> particles)
```
```csharp
public void ApplyPreset(JollyParticlePreset preset)
```

## `Particles/JollyParticleEmitterShape.cs`

```csharp
public enum JollyParticleEmitterShape
```

## `Particles/JollyParticlePreset.cs`

```csharp
public enum JollyParticlePreset
```

## `Particles/JollyParticleRenderer.cs`

```csharp
public sealed class JollyParticleRenderer
```
```csharp
public IReadOnlyList<JollySprite> Sprites => _sprites;
```
```csharp
public JollyParticleRenderer(JollyParticleSystem particleSystem, Func<JollyTexture, JollySprite> spriteFactory, Func<JollyColor, JollyTexture> textureFactory)
```
```csharp
public void Update(JollyTexture texture)
```
```csharp
public void Clear()
```

## `Particles/JollyParticleSystem.cs`

```csharp
public sealed class JollyParticleSystem
```
```csharp
public float Drag { get; internal set; }  public IReadOnlyList<JollyParticle> Particles => _particles;
```
```csharp
public IReadOnlyList<JollyParticleEmitter> Emitters => _emitters;
```
```csharp
public int ParticleCount => _particles.Count;
```
```csharp
public void AddEmitter(JollyParticleEmitter emitter)
```
```csharp
public void RemoveEmitter(JollyParticleEmitter emitter)
```
```csharp
public void Update(float deltaSeconds)
```
```csharp
public void Clear()
```
```csharp
public void Emit(JollyParticleEmitter emitter, int count)
```

## `Physics/JollyCharacterController.cs`

```csharp
public sealed class JollyCharacterController
```
```csharp
public JollyGameObject GameObject => _gameObject;
```
```csharp
public float MoveAcceleration { get; set; } = 2200.0f;
```
```csharp
public float MaxMoveSpeed { get; set; } = 300.0f;
```
```csharp
public float GroundDeceleration { get; set; } = 2600.0f;
```
```csharp
public float AirDeceleration { get; set; } = 800.0f;
```
```csharp
public float JumpVelocity { get; set; } = 450.0f;
```
```csharp
public float JumpGravity { get; set; } = 980.0f;
```
```csharp
public float FallGravity { get; set; } = 1400.0f;
```
```csharp
public float JumpCutGravity { get; set; } = 1800.0f;
```
```csharp
public float MaxFallSpeed { get; set; } = 900.0f;
```
```csharp
public float CoyoteTime { get; set; } = 0.10f;
```
```csharp
public float JumpBufferTime { get; set; } = 0.10f;
```
```csharp
public bool JumpHeld { get; private set; }  public bool IsGrounded => _gameObject.IsGrounded;
```
```csharp
public JollyCharacterController(JollyGameObject gameObject)
```
```csharp
public void Reset()
```
```csharp
public void Move(float direction)
```
```csharp
public void Stop()
```
```csharp
public void SetJumpHeld(bool held)
```
```csharp
public void RequestJump()
```
```csharp
public void Update(JollyGameTime gameTime)
```

## `Physics/JollyCollider.cs`

```csharp
public sealed class JollyCollider
```
```csharp
public JollySprite Sprite { get; } public bool Enabled { get; set; } = true;
```
```csharp
public bool IsTrigger { get; set; } public JollyCollisionLayer Layer { get; set; } = JollyCollisionLayer.Layer0;
```
```csharp
public JollyCollisionLayer Mask { get; set; } = JollyCollisionLayer.All;
```
```csharp
public JollyColliderShape Shape { get; set; } = JollyColliderShape.Rectangle;
```
```csharp
public float Width { get; set; } public float Height { get; set; } public float Radius { get; set; } public byte AlphaThreshold { get; set; } = 1;
```
```csharp
public JollyCollisionMode Mode
```
```csharp
public JollyCollider(JollySprite sprite)
```
```csharp
public bool CanCollideWith(JollyCollider other)
```
```csharp
public bool CollidesWith(JollyCollider other)
```
```csharp
public bool Check(JollyCollider other)
```

## `Physics/JollyColliderShape.cs`

```csharp
public enum JollyColliderShape
```

## `Physics/JollyCollisionDebug.cs`

```csharp
public static class JollyCollisionDebug
```
```csharp
public static bool Enabled { get; set; }  public static bool LogPixelChecks { get; set; }  public static bool LogPixelHits { get; set; } = true;
```
```csharp
public static bool DrawColliders { get; set; }  public static void CollisionEnter(JollyGameObject objectA, JollyGameObject objectB)
```
```csharp
public static void CollisionStay(JollyGameObject objectA, JollyGameObject objectB)
```
```csharp
public static void CollisionExit(JollyGameObject objectA, JollyGameObject objectB)
```
```csharp
public static void TriggerEnter(JollyGameObject objectA, JollyGameObject objectB)
```
```csharp
public static void TriggerStay(JollyGameObject objectA, JollyGameObject objectB)
```
```csharp
public static void TriggerExit(JollyGameObject objectA, JollyGameObject objectB)
```
```csharp
public static void PixelHit(JollySprite spriteA, JollySprite spriteB, int x, int y)
```

## `Physics/JollyCollisionLayer.cs`

```csharp
public enum JollyCollisionLayer : uint
```

## `Physics/JollyCollisionMode.cs`

```csharp
public enum JollyCollisionMode
```

## `Physics/JollyCollisionResult.cs`

```csharp
public readonly struct JollyCollisionResult
```
```csharp
public JollySprite SpriteA { get; } public JollySprite SpriteB { get; } public bool Collided { get; }  public JollyCollisionResult(JollySprite spriteA, JollySprite spriteB, bool collided)
```

## `Physics/JollyCollisionSystem.cs`

```csharp
public static class JollyCollisionSystem
```
```csharp
public static bool Check(JollyCollider colliderA, JollyCollider colliderB)
```

## `Physics/JollyPhysicsSystem.cs`

```csharp
public static class JollyPhysicsSystem
```
```csharp
public static void Update(JollyGameObject gameObject, JollyGameTime gameTime)
```
```csharp
public static void ResolveTileMap(JollyGameObject gameObject, JollyTileMap tileMap, float tileMapX, float tileMapY, float deltaTime)
```
```csharp
public static bool Jump(JollyGameObject gameObject, float velocity)
```
```csharp
public static void Resolve(JollyGameObject objectA, JollyGameObject objectB, float deltaTime)
```

## `Physics/JollyProjectileObject.cs`

```csharp
public sealed class JollyProjectileObject : JollySpriteObject
```
```csharp
public float Lifetime { get; set; } = 5.0f;
```
```csharp
public int Damage { get; set; } = 1;
```
```csharp
public bool DestroyOnCollision { get; set; } = true;
```
```csharp
public bool IsExpired { get; private set; }  public JollyProjectileObject(JollySprite sprite) : base(sprite)
```
```csharp
public override void Initialize()
```
```csharp
public override void Update(JollyGameTime gameTime)
```
```csharp
public override void OnCollisionEnter(JollyGameObject other)
```

## `Rendering/Camera/Camera2D.cs`

```csharp
public sealed class Camera2D
```
```csharp
public Vector2 Position
```
```csharp
public float X
```
```csharp
public float Y
```
```csharp
public float Zoom
```
```csharp
public float Rotation
```
```csharp
public float FollowSpeed
```
```csharp
public Vector2 ViewportSize { get; private set; } public bool BoundsEnabled => _boundsEnabled;
```
```csharp
public float BoundsLeft => _boundsLeft;
```
```csharp
public float BoundsTop => _boundsTop;
```
```csharp
public float BoundsRight => _boundsRight;
```
```csharp
public float BoundsBottom => _boundsBottom;
```
```csharp
public bool IsShaking => _shakeTime > 0.0f;
```
```csharp
public Vector2 ShakeOffset => _shakeOffset;
```
```csharp
public void SetViewport(float width, float height)
```
```csharp
public void SetBounds(float left, float top, float right, float bottom)
```
```csharp
public void ClearBounds()
```
```csharp
public void Follow(Vector2 targetPosition)
```
```csharp
public void FollowSmooth(Vector2 targetPosition, float deltaSeconds)
```
```csharp
public Vector2 WorldToScreen(Vector2 worldPosition)
```
```csharp
public Vector2 ScreenToWorld(Vector2 screenPosition)
```
```csharp
public Matrix3x2 GetWorldToScreenMatrix()
```
```csharp
public CameraState CaptureState()
```
```csharp
public void Shake(float intensity, float duration)
```
```csharp
public void UpdateShake(float deltaSeconds)
```

## `Rendering/Camera/CameraState.cs`

```csharp
public readonly struct CameraState
```
```csharp
public Vector2 Position { get; }  public float Zoom { get; }  public float Rotation { get; }  public Vector2 ViewportSize { get; }  public CameraState( Vector2 position, float zoom, float rotation, Vector2 viewportSize)
```
```csharp
public Matrix3x2 GetWorldToScreenMatrix()
```
```csharp
public Vector2 WorldToScreen(Vector2 worldPosition)
```
```csharp
public Vector2 ScreenToWorld(Vector2 screenPosition)
```

## `Rendering/DirectX/DirectXControl.xaml.cs`

```csharp
public partial class DirectXControl : System.Windows.Controls.UserControl//, System.ComponentModel.INotifyPropertyChanged
```
```csharp
public event Action<double, double>? MouseMoved;
```
```csharp
public event Action? MouseButtonDown;
```
```csharp
public event Action? MouseButtonUp;
```
```csharp
public event Action<float, float>? RenderSizeChanged;
```
```csharp
public bool LineGlowEnabled
```
```csharp
public float LineGlowWidth
```
```csharp
public float LineGlowStrength
```
```csharp
public float LineGlowFalloff
```
```csharp
public JollyDiagnostics? Diagnostics { get; set; }  public DirectXControl()
```
```csharp
public DirectXStretchMode StretchMode
```
```csharp
public DirectXImageFilter ImageFilter
```
```csharp
public double MaxFPS
```
```csharp
public bool VSync
```
```csharp
public bool ClearEveryFrame
```
```csharp
public System.Windows.Media.Color ClearColor
```
```csharp
public static readonly System.Windows.DependencyProperty FPSProperty = System.Windows.DependencyProperty.Register(nameof(FPS), typeof(double), typeof(DirectXControl), new System.Windows.PropertyMetadata(0.0));
```
```csharp
public double FPS
```
```csharp
public static readonly System.Windows.DependencyProperty FrameTimeProperty = System.Windows.DependencyProperty.Register(nameof(FrameTime), typeof(double), typeof(DirectXControl), new System.Windows.PropertyMetadata(0.0));
```
```csharp
public double FrameTime
```
```csharp
public bool IsDirectXReady => _renderer != null && _renderer.IsInitialized;
```
```csharp
public event EventHandler<System.Windows.Point>? MouseScreenPositionChanged;
```
```csharp
public async Task WaitForReadyAsync()
```
```csharp
public void LoadImage(string filePath)
```
```csharp
public void SetImage(BitmapSource source)
```
```csharp
public void SetImage(byte[] pixels, int width, int height)
```
```csharp
public void SetImage(DirectXImage image)
```
```csharp
public void ClearScreen()
```
```csharp
public void Clear()
```
```csharp
public DirectXTexture CreateTexture(string filePath)
```
```csharp
public DirectXTexture CreateTexture(DirectXImage image)
```
```csharp
public DirectXJollyRenderer CreateJollyRenderer()
```

## `Rendering/DirectX/DirectXImage.cs`

```csharp
public sealed class DirectXImage
```
```csharp
public int Width { get; } public int Height { get; } public ReadOnlyMemory<byte> Pixels { get; }  public DirectXImage(int width, int height, ReadOnlyMemory<byte> pixels)
```
```csharp
public static DirectXImage FromFile(string filePath)
```
```csharp
public static DirectXImage FromBitmapSource(BitmapSource source)
```

## `Rendering/DirectX/DirectXRenderer.cs`

```csharp
public enum DirectXImageFilter
```
```csharp
public enum DirectXStretchMode
```
```csharp
public readonly struct SpriteVertex
```
```csharp
public readonly float X;
```
```csharp
public readonly float Y;
```
```csharp
public readonly float Z;
```
```csharp
public readonly float U;
```
```csharp
public readonly float V;
```
```csharp
public SpriteVertex(float x, float y, float z, float u, float v)
```
```csharp
public struct SpriteConstants
```
```csharp
public float PositionX;
```
```csharp
public float PositionY;
```
```csharp
public float Width;
```
```csharp
public float Height;
```
```csharp
public float Rotation;
```
```csharp
public float Opacity;
```
```csharp
public float ViewportWidth;
```
```csharp
public float ViewportHeight;
```
```csharp
public float FlipHorizontal;
```
```csharp
public float FlipVertical;
```
```csharp
public float SourceX;
```
```csharp
public float SourceY;
```
```csharp
public float SourceWidth;
```
```csharp
public float SourceHeight;
```
```csharp
public float Padding0;
```
```csharp
public float Padding1;
```
```csharp
public struct SpriteInstance
```
```csharp
public float PositionX;
```
```csharp
public float PositionY;
```
```csharp
public float Width;
```
```csharp
public float Height;
```
```csharp
public float Rotation;
```
```csharp
public float Opacity;
```
```csharp
public float FlipHorizontal;
```
```csharp
public float FlipVertical;
```
```csharp
public float SourceX;
```
```csharp
public float SourceY;
```
```csharp
public float SourceWidth;
```
```csharp
public float SourceHeight;
```
```csharp
public float Padding0;
```
```csharp
public float Padding1;
```
```csharp
public float Padding2;
```
```csharp
public float Padding3;
```
```csharp
public struct LineVertex
```
```csharp
public float X;
```
```csharp
public float Y;
```
```csharp
public float R;
```
```csharp
public float G;
```
```csharp
public float B;
```
```csharp
public float A;
```
```csharp
public float Edge;
```
```csharp
public float CoreRatio;
```
```csharp
public LineVertex(float x, float y, JollyColor color, float edge, float coreRatio)
```
```csharp
public struct LineConstants
```
```csharp
public float ViewportWidth;
```
```csharp
public float ViewportHeight;
```
```csharp
public float GlowStrength;
```
```csharp
public float GlowFalloff;
```
```csharp
public float GlowEnabled;
```
```csharp
public float Padding0;
```
```csharp
public float Padding1;
```
```csharp
public float Padding2;
```
```csharp
public sealed class DirectXRenderer : IDisposable
```
```csharp
public double FPS => Volatile.Read(ref _fps);
```
```csharp
public double FrameTime => Volatile.Read(ref _frameTime);
```
```csharp
public bool IsInitialized => _initialized && !_disposed;
```
```csharp
public int RenderSpriteCount => _renderSpriteCount;
```
```csharp
public int RenderBatchCount => _renderBatchCount;
```
```csharp
public int RenderDrawCallCount => _renderDrawCallCount;
```
```csharp
public int RenderMaxBatchSize => _renderMaxBatchSize;
```
```csharp
public int RenderInstanceBufferCapacity => _spriteInstanceCapacity;
```
```csharp
public DirectXImageFilter ImageFilter { get; private set; } = DirectXImageFilter.Linear;
```
```csharp
public DirectXStretchMode StretchMode { get; private set; } = DirectXStretchMode.Uniform;
```
```csharp
public double MaxFPS
```
```csharp
public bool VSync
```
```csharp
public bool ClearEveryFrame
```
```csharp
public MediaColor ClearColor
```
```csharp
public bool LineGlowEnabled
```
```csharp
public float LineGlowWidth
```
```csharp
public float LineGlowStrength
```
```csharp
public float LineGlowFalloff
```
```csharp
public JollyDiagnostics? Diagnostics
```
```csharp
public void Initialize(IntPtr hwnd, int width, int height)
```
```csharp
public void StartRenderLoop()
```
```csharp
public void StopRenderLoop()
```
```csharp
public void SetStretchMode(DirectXStretchMode mode)
```
```csharp
public void SetImageFilter(DirectXImageFilter filter)
```
```csharp
public void LoadImage(string filePath)
```
```csharp
public void SetImage(BitmapSource source)
```
```csharp
public void SetImage(byte[] pixels, int width, int height)
```
```csharp
public void SetImage(DirectXImage image)
```
```csharp
public DirectXTexture CreateTextureResource(string filePath)
```
```csharp
public DirectXTexture CreateTextureResource(DirectXImage image)
```
```csharp
public void SubmitRenderFrame(JollyRenderFrame frame, Camera2D camera)
```
```csharp
public void Clear()
```
```csharp
public void ClearScreen()
```
```csharp
public void Render()
```
```csharp
public void Resize(int width, int height)
```
```csharp
public readonly float X;
```
```csharp
public readonly float Y;
```
```csharp
public readonly float Z;
```
```csharp
public readonly float U;
```
```csharp
public readonly float V;
```
```csharp
public ImageVertex(float x, float y, float z, float u, float v)
```
```csharp
public void Dispose()
```

## `Rendering/DirectX/DirectXSprite.cs`

```csharp
public sealed class DirectXSprite
```
```csharp
public DirectXTexture Texture { get; } public float X { get; set; } public float Y { get; set; } public float Width { get; set; } public float Height { get; set; } public float Rotation { get; set; } public float Opacity { get; set; } = 1.0f;
```
```csharp
public bool Visible { get; set; } = true;
```

## `Rendering/DirectX/DirectXSpriteBinding.cs`

```csharp
public JollySprite Sprite { get; }  public DirectXSpriteState State { get; }  public DirectXSpriteBinding(JollySprite sprite, DirectXTexture texture)
```
```csharp
public void Synchronize()
```

## `Rendering/DirectX/DirectXSpriteState.cs`

```csharp
public DirectXTexture Texture { get; } public float X;
```
```csharp
public float Y;
```
```csharp
public float Width;
```
```csharp
public float Height;
```
```csharp
public float Rotation;
```
```csharp
public float Opacity;
```
```csharp
public bool Visible;
```
```csharp
public int Layer;
```
```csharp
public DirectXSpriteState(DirectXTexture texture)
```

## `Rendering/DirectX/DirectXTexture.cs`

```csharp
public sealed class DirectXTexture
```
```csharp
public DirectXImage Image { get; }  internal DirectXTexture(DirectXImage image)
```

## `Rendering/DirectX/JollyPhysicsDebugRenderer.cs`

```csharp
public sealed class JollyPhysicsDebugRenderer
```
```csharp
public bool Enabled { get; set; } = true;
```
```csharp
public bool TileMapEnabled { get; set; } = true;
```
```csharp
public JollyPhysicsDebugRenderer(DirectXJollyRenderer renderer)
```
```csharp
public IReadOnlyList<JollySprite> Sprites => _sprites;
```
```csharp
public void Update(IReadOnlyList<JollyGameObject> objects)
```
```csharp
public void Update(IReadOnlyList<JollyGameObject> objects, JollyTileMap? tileMap, float tileMapX, float tileMapY)
```

## `Rendering/DirectXJollyRenderer.cs`

```csharp
public sealed class DirectXJollyRenderer : JollyRenderer
```
```csharp
public Camera2D Camera { get; } = new();
```
```csharp
public bool LineGlowEnabled
```
```csharp
public float LineGlowWidth
```
```csharp
public float LineGlowStrength
```
```csharp
public float LineGlowFalloff
```
```csharp
public JollyDiagnostics? Diagnostics
```
```csharp
public System.Windows.Media.Color ClearColor
```
```csharp
public DirectXJollyRenderer(DirectXControl control)
```
```csharp
public override JollyTexture LoadTexture(string filePath)
```
```csharp
public JollyTexture CreateSolidTexture(byte red, byte green, byte blue, byte alpha = 255)
```
```csharp
public JollyTexture CreateCircleTexture(byte red, byte green, byte blue, byte alpha = 255, int size = 32)
```
```csharp
public override JollySprite CreateSprite(JollyTexture texture)
```
```csharp
public void SubmitFrame(JollyRenderFrame frame)
```
```csharp
public JollyTexture CreateTextTexture(string text, string fontFamily, float fontSize, JollyColor color, bool bold = false, bool italic = false)
```
```csharp
public void Resize(float width, float height)
```

## `Rendering/JollyRenderSubmission.cs`

```csharp
public sealed class JollyRenderSubmission
```
```csharp
public JollyRenderFrame Frame { get; }  public CameraState Camera { get; }  public JollyRenderSubmission(JollyRenderFrame frame, Camera2D camera)
```

## `Rendering/JollySpriteSheet.cs`

```csharp
public enum JollySpriteSheetOrder
```
```csharp
public readonly struct JollySpriteSheetFrame
```
```csharp
public int Index { get; } public int Column { get; } public int Row { get; } public int X { get; } public int Y { get; } public int Width { get; } public int Height { get; }  internal JollySpriteSheetFrame(int index, int column, int row, int x, int y, int width, int height)
```
```csharp
public sealed class JollySpriteSheet
```
```csharp
public JollyTexture Texture { get; } public int FrameWidth { get; } public int FrameHeight { get; } public int Columns => _columns;
```
```csharp
public int Rows => _rows;
```
```csharp
public int FrameCount => _columns * _rows;
```
```csharp
public JollySpriteSheetOrder Order { get; }  public JollySpriteSheet(JollyTexture texture, int frameWidth, int frameHeight, JollySpriteSheetOrder order = JollySpriteSheetOrder.Horizontal)
```
```csharp
public JollySpriteSheetFrame GetFrame(int index)
```

## `Rendering/JollyTextureManager.cs`

```csharp
public sealed class JollyTextureManager
```
```csharp
public JollyTextureManager(JollyRenderer renderer)
```
```csharp
public int Count => _textures.Count;
```
```csharp
public JollyTexture Load(string filePath)
```
```csharp
public bool TryGet(string filePath, out JollyTexture? texture)
```
```csharp
public bool Contains(string filePath)
```
```csharp
public void Clear()
```

## `Save/JollyGameObjectSaveState.cs`

```csharp
public sealed class JollyGameObjectSaveState
```
```csharp
public string Id { get; set; } = string.Empty;
```
```csharp
public string Name { get; set; } = string.Empty;
```
```csharp
public string Tag { get; set; } = string.Empty;
```
```csharp
public bool Active { get; set; } public JollyVector2 Position { get; set; } public JollyVector2 Scale { get; set; } public float Rotation { get; set; } public bool Visible { get; set; } public JollyVector2 Velocity { get; set; } public JollyVector2 Acceleration { get; set; } public JollyVector2 Gravity { get; set; } public float Drag { get; set; } public bool UsePhysics { get; set; } public bool UseGravity { get; set; } public bool IsGrounded { get; set; } public bool WasGrounded { get; set; } public float GroundFriction { get; set; } public float AirFriction { get; set; } public float Restitution { get; set; } public JollyColliderSaveState? Collider { get; set; }  public static JollyGameObjectSaveState Capture(JollyGameObject gameObject)
```
```csharp
public void ApplyTo(JollyGameObject gameObject)
```
```csharp
public sealed class JollyColliderSaveState
```
```csharp
public bool Enabled { get; set; } public bool IsTrigger { get; set; } public JollyCollisionLayer Layer { get; set; } public JollyCollisionLayer Mask { get; set; } public JollyColliderShape Shape { get; set; } public float Width { get; set; } public float Height { get; set; } public float Radius { get; set; } public byte AlphaThreshold { get; set; }  public static JollyColliderSaveState Capture(JollyCollider collider)
```
```csharp
public void ApplyTo(JollyCollider collider)
```

## `Save/JollySaveManager.cs`

```csharp
public sealed class JollySaveManager
```
```csharp
public const int CurrentFormatVersion = 1;
```
```csharp
public string SaveDirectory { get; }  public JollySaveManager(string? saveDirectory = null)
```
```csharp
public void SaveGame<T>(string slot, T state, int gameVersion = 1)
```
```csharp
public T LoadGame<T>(string slot, int? expectedGameVersion = null)
```
```csharp
public bool TryLoadGame<T>(string slot, out T? state, int? expectedGameVersion = null)
```
```csharp
public bool Exists(string slot)
```
```csharp
public bool HasBackup(string slot)
```
```csharp
public bool RestoreBackup(string slot)
```
```csharp
public void Delete(string slot)
```
```csharp
public IReadOnlyList<string> GetSlots()
```
```csharp
public int FormatVersion { get; set; } public int GameVersion { get; set; } public DateTimeOffset SavedUtc { get; set; } public T? Payload { get; set; } } }  public sealed class JollySaveException : Exception
```
```csharp
public JollySaveException(string message) : base(message)
```
```csharp
public JollySaveException(string message, Exception innerException) : base(message, innerException)
```

## `Save/JollySceneSaveState.cs`

```csharp
public sealed class JollySceneSaveState
```
```csharp
public string SceneName { get; set; } = string.Empty;
```
```csharp
public List<JollyGameObjectSaveState> Objects { get; set; } = new();
```
```csharp
public static JollySceneSaveState Capture(JollyScene scene)
```
```csharp
public int ApplyTo(JollyScene scene)
```

## `Scene/JollyScene.cs`

```csharp
public abstract class JollyScene
```
```csharp
public IReadOnlyList<JollyGameObject> Objects => _objects;
```
```csharp
public int EntityCount => _objects.Count;
```
```csharp
public string Name { get; set; } = string.Empty;
```
```csharp
public bool IsInitialized => _initialized;
```
```csharp
public JollyTileMap? TileMap => _tileMap;
```
```csharp
public void SetTileMap(JollyTileMap tileMap, float x, float y)
```
```csharp
public void ClearTileMap()
```
```csharp
public virtual void OnEnter()
```
```csharp
public virtual void OnExit()
```
```csharp
public virtual void Initialize()
```
```csharp
public void Add(JollyGameObject gameObject)
```
```csharp
public void Spawn(JollyGameObject gameObject)
```
```csharp
public void Remove(JollyGameObject gameObject)
```
```csharp
public void Despawn(JollyGameObject gameObject)
```
```csharp
public virtual void Update(JollyGameTime gameTime)
```
```csharp
public virtual void Draw()
```
```csharp
public IReadOnlyList<JollySprite> GetSprites()
```
```csharp
public JollyGameObject? FindBySaveId(string saveId)
```
```csharp
public JollyGameObject? Find(string name)
```
```csharp
public T? Find<T>(string name) where T : JollyGameObject
```
```csharp
public IReadOnlyList<T> FindAll<T>() where T : JollyGameObject
```
```csharp
public bool Contains(JollyGameObject gameObject)
```
```csharp
public virtual void Shutdown()
```
```csharp
public static void ResolveTileMap(JollyGameObject gameObject, JollyTileMap tileMap, float tileMapX, float tileMapY, float deltaTime)
```
```csharp
public JollyGameObject ObjectA { get; }  public JollyGameObject ObjectB { get; }  public CollisionPair(JollyGameObject objectA, JollyGameObject objectB)
```
```csharp
public bool Equals(CollisionPair other)
```
```csharp
public override bool Equals(object? obj)
```
```csharp
public override int GetHashCode()
```

## `Scene/JollySceneManager.cs`

```csharp
public sealed class JollySceneManager
```
```csharp
public JollyScene? CurrentScene { get; private set; } public JollyScene? PreviousScene { get; private set; } public int EntityCount => CurrentScene?.EntityCount ?? 0;
```
```csharp
public bool IsChangingScene => _pendingScenes.Count > 0;
```
```csharp
public event Action<JollyScene?, JollyScene?>? SceneChanged;
```
```csharp
public void ChangeScene(JollyScene scene)
```
```csharp
public JollyGameObject? Find(string name)
```
```csharp
public T? Find<T>(string name) where T : JollyGameObject
```
```csharp
public IReadOnlyList<T> FindAll<T>() where T : JollyGameObject
```
```csharp
public void Update(JollyGameTime gameTime)
```
```csharp
public void Draw()
```
```csharp
public void Shutdown()
```

## `UI/JollyUI9Slice.cs`

```csharp
public sealed class JollyUI9Slice
```
```csharp
public JollyTexture Texture { get; } public float Left { get; set; } public float Top { get; set; } public float Right { get; set; } public float Bottom { get; set; }  public JollyUI9Slice(JollyTexture texture, float left, float top, float right, float bottom)
```

## `UI/JollyUIAnchor.cs`

```csharp
public enum JollyUIAnchor
```

## `UI/JollyUIAnimation.cs`

```csharp
public enum JollyUIAnimationProperty
```
```csharp
public sealed class JollyUIAnimation
```
```csharp
public JollyUIElement Element { get; } public JollyUIAnimationProperty Property { get; } public float Duration { get; } public bool IsComplete { get; private set; }  internal JollyUIAnimation(JollyUIElement element, JollyUIAnimationProperty property, Vector2 start, Vector2 end, float duration)
```

## `UI/JollyUIButton.cs`

```csharp
public sealed class JollyUIButton : JollyUIElement
```
```csharp
public string Text { get; set; } = "Button";
```
```csharp
public JollyColor NormalColor { get; set; } = JollyColor.FromBytes(60, 60, 60, 255);
```
```csharp
public JollyColor HoverColor { get; set; } = JollyColor.FromBytes(80, 80, 80, 255);
```
```csharp
public JollyColor PressedColor { get; set; } = JollyColor.FromBytes(40, 40, 40, 255);
```
```csharp
public JollyColor DisabledColor { get; set; } = JollyColor.FromBytes(35, 35, 35, 255);
```
```csharp
public JollyColor TextColor { get; set; } = JollyColor.White;
```
```csharp
public float FontSize { get; set; } = 18.0f;
```
```csharp
public bool Bold { get; set; } public bool IsHovered { get; internal set; } public bool IsPressed { get; internal set; }  public event Action<JollyUIButton>? Click;
```
```csharp
public JollyColor CurrentColor
```
```csharp
public JollyTexture? NormalTexture { get; set; } public JollyTexture? HoverTexture { get; set; } public JollyTexture? PressedTexture { get; set; } public JollyTexture? DisabledTexture { get; set; } public JollyTexture? CurrentTexture
```
```csharp
public JollyUI9Slice? NormalNineSlice { get; set; } public JollyUI9Slice? HoverNineSlice { get; set; } public JollyUI9Slice? PressedNineSlice { get; set; } public JollyUI9Slice? DisabledNineSlice { get; set; }  public JollyUI9Slice? CurrentNineSlice
```

## `UI/JollyUIElement.cs`

```csharp
public class JollyUIElement
```
```csharp
public JollyUIElement? Parent { get; private set; } public IReadOnlyList<JollyUIElement> Children => _children;
```
```csharp
public Vector2 Position { get; set; } public Vector2 Size { get; set; } public bool Visible { get; set; } = true;
```
```csharp
public bool Enabled { get; set; } = true;
```
```csharp
public int Layer { get; set; } public float Opacity { get; set; } = 1.0f;
```
```csharp
public JollyUIAnchor Anchor { get; set; } = JollyUIAnchor.TopLeft;
```
```csharp
public float MarginLeft { get; set; } public float MarginTop { get; set; } public float MarginRight { get; set; } public float MarginBottom { get; set; }  public float X
```
```csharp
public float Y
```
```csharp
public float Width
```
```csharp
public float Height
```
```csharp
public float AbsoluteX
```
```csharp
public float AbsoluteY
```
```csharp
public Vector2 AbsolutePosition
```
```csharp
public bool ContainsPoint(Vector2 point)
```
```csharp
public void AddChild(JollyUIElement child)
```
```csharp
public bool RemoveChild(JollyUIElement child)
```
```csharp
public void RemoveAllChildren()
```
```csharp
public void SetMargin(float left, float top, float right, float bottom)
```

## `UI/JollyUILayout.cs`

```csharp
public enum JollyUILayout
```

## `UI/JollyUIManager.cs`

```csharp
public sealed class JollyUIManager
```
```csharp
public IReadOnlyList<JollyUIElement> Elements => _elements;
```
```csharp
public System.Numerics.Vector2 MousePosition { get; private set; } public JollyUIElement? HoveredElement => _hoveredElement;
```
```csharp
public JollyUIElement? PressedElement => _pressedElement; public float ScreenWidth { get; private set; }
```
```csharp
public float ScreenHeight { get; private set; }  public void SetScreenSize(float width, float height)
```
```csharp
public void Add(JollyUIElement element)
```
```csharp
public bool Remove(JollyUIElement element)
```
```csharp
public void Clear()
```
```csharp
public JollyUIElement? HitTest(Vector2 point)
```
```csharp
public void MouseMove(float x, float y)
```
```csharp
public void MouseDown()
```
```csharp
public void MouseUp()
```
```csharp
public JollyUIAnimation AnimatePosition(JollyUIElement element, Vector2 target, float duration)
```
```csharp
public JollyUIAnimation AnimateSize(JollyUIElement element, Vector2 target, float duration)
```
```csharp
public JollyUIAnimation AnimateOpacity(JollyUIElement element, float target, float duration)
```
```csharp
public void Update(float deltaSeconds)
```
```csharp
public void UpdateLayout()
```

## `UI/JollyUIPanel.cs`

```csharp
public sealed class JollyUIPanel : JollyUIElement
```
```csharp
public JollyColor Color { get; set; } = JollyColor.White;
```
```csharp
public JollyColor BackgroundColor
```
```csharp
public JollyUILayout Layout { get; set; } = JollyUILayout.None;
```
```csharp
public float Spacing { get; set; } public float PaddingLeft { get; set; } public float PaddingTop { get; set; } public float PaddingRight { get; set; } public float PaddingBottom { get; set; } public bool StretchChildren { get; set; } } }
```

## `UI/JollyUIProgressBar.cs`

```csharp
public enum JollyUIProgressBarOrientation
```
```csharp
public sealed class JollyUIProgressBar : JollyUIElement
```
```csharp
public float Minimum
```
```csharp
public float Maximum
```
```csharp
public float Value
```
```csharp
public float Percentage
```
```csharp
public JollyUIProgressBarOrientation Orientation { get; set; } = JollyUIProgressBarOrientation.Horizontal;
```
```csharp
public JollyColor BackgroundColor
```
```csharp
public JollyColor FillColor
```
```csharp
public JollyTexture? BackgroundTexture { get; set; }  public JollyTexture? FillTexture { get; set; }  private bool _showText;
```
```csharp
public bool ShowText
```
```csharp
public string Text
```
```csharp
public JollyColor TextColor
```
```csharp
public float FontSize
```
```csharp
public bool Bold
```
```csharp
public bool TextDirty { get; internal set; } = true;
```

## `UI/JollyUIRenderer.cs`

```csharp
public sealed class JollyUIRenderer
```
```csharp
public IReadOnlyList<JollySprite> Sprites => _sprites;
```
```csharp
public JollyUIRenderer(JollyUIManager uiManager, Func<JollyTexture, JollySprite> spriteFactory, Func<byte, byte, byte, byte, JollyTexture> textureFactory, Func<string, string, float, JollyColor, bool, bool, JollyTexture> textTextureFactory)
```
```csharp
public void Update()
```
```csharp
public void Clear()
```

## `UI/JollyUISlider.cs`

```csharp
public enum JollyUISliderOrientation
```
```csharp
public sealed class JollyUISlider : JollyUIElement
```
```csharp
public JollyUISliderOrientation Orientation { get; set; } = JollyUISliderOrientation.Horizontal;
```
```csharp
public float Minimum
```
```csharp
public float Maximum
```
```csharp
public float Value
```
```csharp
public float Percentage
```
```csharp
public JollyColor TrackColor { get; set; } = JollyColor.FromBytes(50, 50, 50, 255);
```
```csharp
public JollyColor FillColor { get; set; } = JollyColor.FromBytes(80, 160, 240, 255);
```
```csharp
public JollyColor HandleColor { get; set; } = JollyColor.White;
```
```csharp
public float TrackThickness { get; set; } = 8.0f;
```
```csharp
public float HandleSize { get; set; } = 20.0f;
```
```csharp
public JollyTexture? TrackTexture { get; set; }  public JollyTexture? FillTexture { get; set; }  public JollyTexture? HandleTexture { get; set; }  public bool IsDragging
```
```csharp
public event Action<JollyUISlider, float>? ValueChanged;
```

## `UI/JollyUIText.cs`

```csharp
public sealed class JollyUIText : JollyUIElement
```
```csharp
public string Text
```
```csharp
public string FontFamily
```
```csharp
public float FontSize
```
```csharp
public JollyColor Color
```
```csharp
public bool Bold
```
```csharp
public bool Italic
```
```csharp
public bool Dirty { get; internal set; } = true;
```

## `World/JollyTileMap.cs`

```csharp
public sealed class JollyTileMap
```
```csharp
public event Action<JollyGameObject, int, int, int>? HazardContact;
```
```csharp
public JollyTexture? Texture { get; private set; } public int TextureColumns { get; private set; } public int TextureRows { get; private set; } public int TextureTileCount => TextureColumns * TextureRows;
```
```csharp
public int Width { get; } public int Height { get; } public float TileWidth { get; } public float TileHeight { get; }  public JollyTileMap(int width, int height, float tileWidth, float tileHeight)
```
```csharp
public void NotifyHazardContact(JollyGameObject gameObject, int tileX, int tileY, int tileIndex)
```
```csharp
public bool IsSolid(int x, int y)
```
```csharp
public void SetSolid(int x, int y, bool solid)
```
```csharp
public int GetTile(int x, int y)
```
```csharp
public void SetTile(int x, int y, int tileIndex)
```
```csharp
public Vector2 GetTilePosition(int x, int y)
```
```csharp
public Vector2 WorldToTile(float x, float y)
```
```csharp
public bool IsSolidAtWorldPosition(float x, float y)
```
```csharp
public bool TryGetTileAtWorldPosition(float x, float y, out int tileIndex)
```
```csharp
public void SetTexture(JollyTexture texture)
```
```csharp
public bool TryGetTileSourceRectangle(int tileIndex, out int x, out int y, out int width, out int height)
```
```csharp
public void SetAnimatedTile(int tileIndex, int startFrame, int frameCount, float framesPerSecond, bool loop = true)
```
```csharp
public void RemoveAnimatedTile(int tileIndex)
```
```csharp
public bool IsAnimatedTile(int tileIndex)
```
```csharp
public void SetConveyorTile(int tileIndex, float speed)
```
```csharp
public void RemoveConveyorTile(int tileIndex)
```
```csharp
public bool IsConveyorTile(int tileIndex)
```
```csharp
public float GetConveyorSpeed(int tileIndex)
```
```csharp
public int GetAnimatedTileIndex(int tileIndex, float elapsedSeconds)
```
```csharp
public void SetHazardTile(int tileIndex, bool hazard)
```
```csharp
public bool IsHazardTile(int tileIndex)
```
```csharp
public int StartFrame { get; } public int FrameCount { get; } public float FramesPerSecond { get; } public bool Loop { get; }  public JollyTileAnimation(int startFrame, int frameCount, float framesPerSecond, bool loop)
```

## `World/JollyTileMapRenderer.cs`

```csharp
public sealed class JollyTileMapRenderer
```
```csharp
public IReadOnlyList<JollySprite> Sprites => _sprites;
```
```csharp
public int Layer { get; set; } public float X { get; set; } public float Y { get; set; }  public JollyTileMapRenderer(JollyTileMap tileMap, Func<JollyTexture, JollySprite> spriteFactory)
```
```csharp
public void Rebuild()
```
```csharp
public void Update(Camera2D camera, float deltaSeconds)
```

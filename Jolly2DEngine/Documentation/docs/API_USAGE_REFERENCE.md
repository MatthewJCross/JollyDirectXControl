# Complete API Usage Reference

This reference complements `API_REFERENCE.md` by documenting **what the parameters mean**, expected ranges where the implementation enforces them, and the return/event role for every public method/constructor found in the supplied source.

## `Animation/JollyAnimation.cs`

### `public string Name { get; }  public JollySpriteSheet Sheet { get; }  public int StartFrame { get; }  public int FrameCount { get; }  public double FPS { get; }  public bool Loop { get; }  public JollyAnimation(string name, JollySpriteSheet sheet, int startFrame, int frameCount, double fps, bool loop = true)`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |
| `sheet` | sprite sheet supplying animation frames. |
| `startFrame` | first sprite-sheet frame used by the animation. |
| `frameCount` | number of frames used by the animation. |
| `fps` | frames/updates per second. |
| `loop` | whether playback loops. |

**Return:** Returns `string` as declared by the API.

### `public JollySpriteSheetFrame GetFrame(int animationFrame)`

| Parameter | Meaning |
|---|---|
| `animationFrame` | zero-based frame index within the animation. |

**Return:** Returns `JollySpriteSheetFrame` as declared by the API.

## `Assets/JollyAssetHandle.cs`

### `public void Dispose()`

**Return:** No value is returned.

## `Assets/JollyAssetManager.cs`

### `public JollyAssetManager(DirectXJollyRenderer renderer, JollyAudioManager? audio = null, string? rootPath = null)`

| Parameter | Meaning |
|---|---|
| `renderer` | the renderer used to create or submit graphics resources. |
| `audio` | value supplied to the operation. |
| `rootPath` | root directory containing the engine asset folders. |

**Return:** Returns `JollyAssetManager` as declared by the API.

### `public string RootPath { get; }  public string TexturesPath => Path.Combine(RootPath, TexturesFolder);`

| Parameter | Meaning |
|---|---|
| `RootPath` | root directory containing the engine asset folders. |
| `TexturesFolder` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

### `public string AudioPath => Path.Combine(RootPath, AudioFolder);`

| Parameter | Meaning |
|---|---|
| `RootPath` | root directory containing the engine asset folders. |
| `AudioFolder` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

### `public string FontsPath => Path.Combine(RootPath, FontsFolder);`

| Parameter | Meaning |
|---|---|
| `RootPath` | root directory containing the engine asset folders. |
| `FontsFolder` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

### `public string MapsPath => Path.Combine(RootPath, MapsFolder);`

| Parameter | Meaning |
|---|---|
| `RootPath` | root directory containing the engine asset folders. |
| `MapsFolder` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

### `public string AnimationsPath => Path.Combine(RootPath, AnimationsFolder);`

| Parameter | Meaning |
|---|---|
| `RootPath` | root directory containing the engine asset folders. |
| `AnimationsFolder` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

### `public JollyTexture LoadTexture(string assetName)`

| Parameter | Meaning |
|---|---|
| `assetName` | value supplied to the operation. |

**Return:** Returns `JollyTexture` as declared by the API.

### `public async Task<JollyTexture> LoadTextureAsync(string assetName, CancellationToken cancellationToken = default)`

| Parameter | Meaning |
|---|---|
| `assetName` | value supplied to the operation. |
| `cancellationToken` | token used to cancel the asynchronous operation. |

**Return:** Returns `async` as declared by the API.

### `public JollyAssetHandle<JollyTexture> AcquireTexture(string assetName)`

| Parameter | Meaning |
|---|---|
| `assetName` | value supplied to the operation. |

**Return:** Returns `JollyAssetHandle<JollyTexture>` as declared by the API.

### `public void ReleaseTexture(string assetName)`

| Parameter | Meaning |
|---|---|
| `assetName` | value supplied to the operation. |

**Return:** No value is returned.

### `public bool IsTextureLoaded(string assetName)`

| Parameter | Meaning |
|---|---|
| `assetName` | value supplied to the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public void LoadSound(string assetName)`

| Parameter | Meaning |
|---|---|
| `assetName` | value supplied to the operation. |

**Return:** No value is returned.

### `public void PlaySound(string assetName)`

| Parameter | Meaning |
|---|---|
| `assetName` | value supplied to the operation. |

**Return:** No value is returned.

### `public void PlayMusic(string assetName, bool loop = true)`

| Parameter | Meaning |
|---|---|
| `assetName` | value supplied to the operation. |
| `loop` | whether playback loops. |

**Return:** No value is returned.

### `public void UnloadSound(string assetName)`

| Parameter | Meaning |
|---|---|
| `assetName` | value supplied to the operation. |

**Return:** No value is returned.

### `public string GetTexturePath(string assetName) => ResolveAssetPath(TexturesFolder, NormalizeAssetName(assetName));`

| Parameter | Meaning |
|---|---|
| `assetName)` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

### `public string GetAudioPath(string assetName) => ResolveAssetPath(AudioFolder, NormalizeAssetName(assetName));`

| Parameter | Meaning |
|---|---|
| `assetName)` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

### `public string GetFontPath(string assetName) => ResolveAssetPath(FontsFolder, NormalizeAssetName(assetName));`

| Parameter | Meaning |
|---|---|
| `assetName)` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

### `public string GetMapPath(string assetName) => ResolveAssetPath(MapsFolder, NormalizeAssetName(assetName));`

| Parameter | Meaning |
|---|---|
| `assetName)` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

### `public string GetAnimationPath(string assetName) => ResolveAssetPath(AnimationsFolder, NormalizeAssetName(assetName));`

| Parameter | Meaning |
|---|---|
| `assetName)` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

### `public bool AssetExists(string category, string assetName)`

| Parameter | Meaning |
|---|---|
| `category` | asset category/folder name. |
| `assetName` | value supplied to the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public void Validate(params JollyAssetRequirement[] requirements)`

| Parameter | Meaning |
|---|---|
| `requirements` | asset requirements that must exist. |

**Return:** No value is returned.

### `public void Dispose()`

**Return:** No value is returned.

### `public TextureEntry(JollyTexture texture)`

| Parameter | Meaning |
|---|---|
| `texture` | the texture used by the sprite or render operation. |

**Return:** Returns `TextureEntry` as declared by the API.

### `public JollyTexture Texture { get; } public int ReferenceCount { get; set; } } }  public sealed record JollyAssetRequirement(string Category, string Name);`

| Parameter | Meaning |
|---|---|
| `Category` | asset category/folder name. |
| `Name` | name used to identify the object, asset, animation, sound or slot. |

**Return:** Returns `type` as declared by the API.

## `Audio/JollyAudioManager.cs`

### `public void LoadSound(string name, string path)`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |
| `path` | file-system path for the resource. |

**Return:** No value is returned.

### `public void UnloadSound(string name)`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |

**Return:** No value is returned.

### `public void PlaySound(string name)`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |

**Return:** No value is returned.

### `public void StopAllSounds()`

**Return:** No value is returned.

### `public void PlayMusic(string path, bool loop = true)`

| Parameter | Meaning |
|---|---|
| `path` | file-system path for the resource. |
| `loop` | whether playback loops. |

**Return:** No value is returned.

### `public void PauseMusic()`

**Return:** No value is returned.

### `public void ResumeMusic()`

**Return:** No value is returned.

### `public void StopMusic()`

**Return:** No value is returned.

### `public void ClearSounds()`

**Return:** No value is returned.

### `public void Dispose()`

**Return:** No value is returned.

### `public CachedSound Sound { get; }  public WaveOut Output { get; }  public SoundInstance(CachedSound sound, float volume)`

| Parameter | Meaning |
|---|---|
| `sound` | value supplied to the operation. |
| `volume` | value supplied to the operation. |

**Return:** Returns `CachedSound` as declared by the API.

### `public void Dispose()`

**Return:** No value is returned.

### `public WaveFormat WaveFormat { get; }  public float[] AudioData { get; }  public CachedSound(string audioFileName)`

| Parameter | Meaning |
|---|---|
| `audioFileName` | value supplied to the operation. |

**Return:** Returns `WaveFormat` as declared by the API.

### `public CachedSoundSampleProvider(CachedSound sound)`

| Parameter | Meaning |
|---|---|
| `sound` | value supplied to the operation. |

**Return:** Returns `CachedSoundSampleProvider` as declared by the API.

### `public int Read(Span<float> buffer)`

| Parameter | Meaning |
|---|---|
| `buffer` | value supplied to the operation. |

**Return:** Returns `int` as declared by the API.

### `public LoopStream(WaveStream source)`

| Parameter | Meaning |
|---|---|
| `source` | source image/stream/resource. |

**Return:** Returns `LoopStream` as declared by the API.

### `public override int Read(byte[] buffer, int offset, int count)`

| Parameter | Meaning |
|---|---|
| `buffer` | value supplied to the operation. |
| `offset` | value supplied to the operation. |
| `count` | number of items/frames to process. |

**Return:** Returns `override` as declared by the API.

## `Core/JollyGame.cs`

### `public JollySceneManager Scenes { get; }  public JollyInput Input { get; }  public JollyGameTime GameTime { get; } = new JollyGameTime();`

**Return:** Returns `JollySceneManager` as declared by the API.

### `public double GameLoopActualFPS => Volatile.Read(ref _gameLoopActualFPS);`

| Parameter | Meaning |
|---|---|
| `_gameLoopActualFPS` | value supplied to the operation. |

**Return:** Returns `double` as declared by the API.

### `public void Start()`

**Return:** No value is returned.

### `public void Stop()`

**Return:** No value is returned.

## `Core/JollyGameObject.cs`

### `public string SaveId { get; set; } = Guid.NewGuid().ToString("N");`

| Parameter | Meaning |
|---|---|
| `).ToString("N"` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

### `public bool IsDestroyed { get; private set; } internal JollyScene? Scene { get; set; }  public virtual void Initialize()`

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public virtual void Update(JollyGameTime gameTime)`

| Parameter | Meaning |
|---|---|
| `gameTime` | the current game-time/update information. |

**Return:** Returns `virtual` as declared by the API.

### `public virtual void Draw()`

**Return:** Returns `virtual` as declared by the API.

### `public virtual void Shutdown()`

**Return:** Returns `virtual` as declared by the API.

### `public void SetCollider(JollyCollider collider)`

| Parameter | Meaning |
|---|---|
| `collider` | value supplied to the operation. |

**Return:** No value is returned.

### `public void RemoveCollider()`

**Return:** No value is returned.

### `public virtual void OnCollisionEnter(JollyGameObject other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `virtual` as declared by the API.

### `public virtual void OnCollisionStay(JollyGameObject other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `virtual` as declared by the API.

### `public virtual void OnCollisionExit(JollyGameObject other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `virtual` as declared by the API.

### `public virtual void OnTriggerEnter(JollyGameObject other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `virtual` as declared by the API.

### `public virtual void OnTriggerStay(JollyGameObject other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `virtual` as declared by the API.

### `public virtual void OnTriggerExit(JollyGameObject other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `virtual` as declared by the API.

### `public JollyCharacterController AddCharacterController()`

**Return:** Returns `JollyCharacterController` as declared by the API.

### `public void Destroy()`

**Return:** No value is returned.

## `Core/JollySpriteObject.cs`

### `public JollySpriteObject(JollySprite sprite)`

| Parameter | Meaning |
|---|---|
| `sprite` | value supplied to the operation. |

**Return:** Returns `JollySpriteObject` as declared by the API.

### `public override void Initialize()`

**Return:** Returns `override` as declared by the API.

### `public override void Update(JollyGameTime gameTime)`

| Parameter | Meaning |
|---|---|
| `gameTime` | the current game-time/update information. |

**Return:** Returns `override` as declared by the API.

### `public override void Draw()`

**Return:** Returns `override` as declared by the API.

### `public override void Shutdown()`

**Return:** Returns `override` as declared by the API.

### `public void EnableCollision(JollyCollisionMode mode = JollyCollisionMode.Bounds, byte alphaThreshold = 1)`

| Parameter | Meaning |
|---|---|
| `mode` | operation or collision mode to use. |
| `alphaThreshold` | minimum alpha value used by pixel collision. |

**Return:** No value is returned.

### `public void SetRectangleCollider(float width, float height)`

| Parameter | Meaning |
|---|---|
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |

**Return:** No value is returned.

### `public void SetCircleCollider(float radius)`

| Parameter | Meaning |
|---|---|
| `radius` | circle radius in world units. |

**Return:** No value is returned.

### `public void SetPixelCollider(byte alphaThreshold = 1)`

| Parameter | Meaning |
|---|---|
| `alphaThreshold` | minimum alpha value used by pixel collision. |

**Return:** No value is returned.

### `public void SetCollisionLayers(JollyCollisionLayer layer, JollyCollisionLayer mask)`

| Parameter | Meaning |
|---|---|
| `layer` | collision layer assigned to the collider. |
| `mask` | collision layers this collider accepts. |

**Return:** No value is returned.

### `public void SetTrigger(bool isTrigger = true)`

| Parameter | Meaning |
|---|---|
| `isTrigger` | value supplied to the operation. |

**Return:** No value is returned.

### `public void DisableCollision()`

**Return:** No value is returned.

### `public override void OnCollisionEnter(JollyGameObject other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `override` as declared by the API.

### `public override void OnCollisionStay(JollyGameObject other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `override` as declared by the API.

### `public override void OnCollisionExit(JollyGameObject other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `override` as declared by the API.

### `public override void OnTriggerEnter(JollyGameObject other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `override` as declared by the API.

### `public override void OnTriggerStay(JollyGameObject other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `override` as declared by the API.

### `public override void OnTriggerExit(JollyGameObject other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `override` as declared by the API.

## `Diagnostics/JollyDiagnosticScope.cs`

### `public void Dispose()`

**Return:** No value is returned.

## `Diagnostics/JollyDiagnostics.cs`

### `public void BeginFrame()`

**Return:** No value is returned.

### `public void EndFrame()`

**Return:** No value is returned.

### `public JollyDiagnosticScope MeasureUpdate()`

**Return:** Returns `JollyDiagnosticScope` as declared by the API.

### `public JollyDiagnosticScope MeasureRender()`

**Return:** Returns `JollyDiagnosticScope` as declared by the API.

### `public void SetEntityCount(int count)`

| Parameter | Meaning |
|---|---|
| `count` | number of items/frames to process. |

**Return:** No value is returned.

### `public void SetSpriteCount(int count)`

| Parameter | Meaning |
|---|---|
| `count` | number of items/frames to process. |

**Return:** No value is returned.

### `public void SetTextureCount(int count)`

| Parameter | Meaning |
|---|---|
| `count` | number of items/frames to process. |

**Return:** No value is returned.

### `public void SetRenderBatchCount(int count)`

| Parameter | Meaning |
|---|---|
| `count` | number of items/frames to process. |

**Return:** No value is returned.

### `public void SetRenderDrawCallCount(int count)`

| Parameter | Meaning |
|---|---|
| `count` | number of items/frames to process. |

**Return:** No value is returned.

### `public void SetRenderMaxBatchSize(int count)`

| Parameter | Meaning |
|---|---|
| `count` | number of items/frames to process. |

**Return:** No value is returned.

### `public void SetConnectedClients(int count)`

| Parameter | Meaning |
|---|---|
| `count` | number of items/frames to process. |

**Return:** No value is returned.

### `public void RecordNetworkMessageSent(int byteCount)`

| Parameter | Meaning |
|---|---|
| `byteCount` | value supplied to the operation. |

**Return:** No value is returned.

### `public void RecordNetworkMessageReceived(int byteCount)`

| Parameter | Meaning |
|---|---|
| `byteCount` | value supplied to the operation. |

**Return:** No value is returned.

### `public void SetNetworkLatency(double milliseconds)`

| Parameter | Meaning |
|---|---|
| `milliseconds` | value supplied to the operation. |

**Return:** No value is returned.

### `public JollyProfile RegisterProfile(string name)`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |

**Return:** Returns `JollyProfile` as declared by the API.

### `public JollyProfileScope Profile(JollyProfile? profile)`

| Parameter | Meaning |
|---|---|
| `profile` | value supplied to the operation. |

**Return:** Returns `JollyProfileScope` as declared by the API.

### `public JollyDiagnosticsSnapshot GetSnapshot()`

**Return:** Returns `JollyDiagnosticsSnapshot` as declared by the API.

### `public void Reset()`

**Return:** No value is returned.

## `Diagnostics/JollyDiagnosticsFormatter.cs`

### `public static string Format(JollyDiagnosticsSnapshot snapshot)`

| Parameter | Meaning |
|---|---|
| `snapshot` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

## `Diagnostics/JollyDiagnosticsSnapshot.cs`

### `public double FPS { get; init; } public double FrameTimeMilliseconds { get; init; } public double UpdateTimeMilliseconds { get; init; } public double RenderTimeMilliseconds { get; init; }  public long FrameCount { get; init; } public long UpdateCount { get; init; } public long RenderCount { get; init; }  public int EntityCount { get; init; } public int SpriteCount { get; init; } public int TextureCount { get; init; } public int RenderBatchCount { get; init; } public int RenderDrawCallCount { get; init; } public int RenderMaxBatchSize { get; init; }  public int ConnectedClients { get; init; } public long NetworkMessagesSent { get; init; } public long NetworkMessagesReceived { get; init; } public long NetworkBytesSent { get; init; } public long NetworkBytesReceived { get; init; }  public double NetworkLatencyMilliseconds { get; init; }  public IReadOnlyDictionary<string, JollyProfileSnapshot> Profiles { get; init; } = new Dictionary<string, JollyProfileSnapshot>();`

**Return:** Returns `double` as declared by the API.

## `Diagnostics/JollyDiagnosticsViewModel.cs`

### `public void Refresh(JollyDiagnostics diagnostics)`

| Parameter | Meaning |
|---|---|
| `diagnostics` | value supplied to the operation. |

**Return:** No value is returned.

### `public string GetSummary()`

**Return:** Returns `string` as declared by the API.

### `public string GetRenderSummary()`

**Return:** Returns `string` as declared by the API.

### `public JollyProfileSnapshot? GetProfile(string name)`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |

**Return:** Returns `JollyProfileSnapshot?` as declared by the API.

### `public long GetProfileCallCount(string name)`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |

**Return:** Returns `long` as declared by the API.

## `Diagnostics/JollyProfileScope.cs`

### `public void Dispose()`

**Return:** No value is returned.

## `Graphics/JollyLineSnapshot.cs`

### `public readonly struct JollyLineSnapshot(Vector2 start, Vector2 end, JollyColor color, float thickness, float startThickness = 1.0f, float endThickness = 1.0f)`

| Parameter | Meaning |
|---|---|
| `start` | line start position. |
| `end` | line end position. |
| `color` | colour applied to the resource. |
| `thickness` | line thickness. |
| `startThickness` | line thickness at the start of the line. |
| `endThickness` | line thickness at the end of the line. |

**Return:** Returns `type` as declared by the API.

## `Graphics/JollyRenderFrame.cs`

### `public JollyRenderFrame(IEnumerable<JollySprite> sprites, IEnumerable<JollyLineSnapshot>? lines = null)`

| Parameter | Meaning |
|---|---|
| `sprites` | value supplied to the operation. |
| `lines` | value supplied to the operation. |

**Return:** Returns `JollyRenderFrame` as declared by the API.

## `Graphics/JollyRenderTarget.cs`

### `public int Width { get; } public int Height { get; }  internal JollyRenderTarget(ID3D11Texture2D texture, ID3D11RenderTargetView renderTargetView, ID3D11ShaderResourceView shaderResourceView, int width, int height)`

| Parameter | Meaning |
|---|---|
| `texture` | the texture used by the sprite or render operation. |
| `renderTargetView` | value supplied to the operation. |
| `shaderResourceView` | value supplied to the operation. |
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |

**Return:** Returns `int` as declared by the API.

### `public void Dispose()`

**Return:** No value is returned.

## `Graphics/JollyRenderer.cs`

### `public abstract JollyTexture LoadTexture(string filePath);`

| Parameter | Meaning |
|---|---|
| `filePath` | path to the source file. |

**Return:** Returns `JollyTexture` as declared by the API.

### `public abstract JollySprite CreateSprite(JollyTexture texture);`

| Parameter | Meaning |
|---|---|
| `texture` | the texture used by the sprite or render operation. |

**Return:** Returns `JollySprite` as declared by the API.

### `public JollySprite CreateSprite(string filePath)`

| Parameter | Meaning |
|---|---|
| `filePath` | path to the source file. |

**Return:** Returns `JollySprite` as declared by the API.

## `Graphics/JollySprite.cs`

### `public void AddAnimation(JollyAnimation animation)`

| Parameter | Meaning |
|---|---|
| `animation` | value supplied to the operation. |

**Return:** No value is returned.

### `public bool RemoveAnimation(string name)`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool HasAnimation(string name)`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public JollyAnimation? GetAnimation(string name)`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |

**Return:** Returns `JollyAnimation?` as declared by the API.

### `public void Play(string animationName)`

| Parameter | Meaning |
|---|---|
| `animationName` | value supplied to the operation. |

**Return:** No value is returned.

### `public void SetAnimation(JollySpriteSheet sheet, float framesPerSecond, bool loop = true)`

| Parameter | Meaning |
|---|---|
| `sheet` | sprite sheet supplying animation frames. |
| `framesPerSecond` | value supplied to the operation. |
| `loop` | whether playback loops. |

**Return:** No value is returned.

### `public void Play()`

**Return:** No value is returned.

### `public void Pause()`

**Return:** No value is returned.

### `public void Stop()`

**Return:** No value is returned.

### `public void SetAnimationFrame(int frame)`

| Parameter | Meaning |
|---|---|
| `frame` | value supplied to the operation. |

**Return:** No value is returned.

### `public void SetFrame(JollySpriteSheetFrame frame, JollySpriteSheet sheet)`

| Parameter | Meaning |
|---|---|
| `frame` | value supplied to the operation. |
| `sheet` | sprite sheet supplying animation frames. |

**Return:** No value is returned.

## `Graphics/JollySpriteSnapshot.cs`

### `public string Name { get; } public JollyTexture Texture { get; } public float X { get; } public float Y { get; } public float Width { get; } public float Height { get; } public float Rotation { get; } public float Opacity { get; } public JollyColor Color { get; init; } public bool Visible { get; } public int Layer { get; } public bool FlipHorizontal { get; } public bool FlipVertical { get; } public float SourceX { get; } public float SourceY { get; } public float SourceWidth { get; } public float SourceHeight { get; } public JollyVector2 Scale { get; }  internal JollySpriteSnapshot(JollySprite sprite)`

| Parameter | Meaning |
|---|---|
| `sprite` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

## `Graphics/JollyTexture.cs`

### `public int Width { get; } public int Height { get; }  internal object RendererTexture { get; }  internal byte[] AlphaMask { get; }  internal JollyTexture(object rendererTexture, int width, int height, byte[] alphaMask)`

| Parameter | Meaning |
|---|---|
| `rendererTexture` | value supplied to the operation. |
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |
| `alphaMask` | value supplied to the operation. |

**Return:** Returns `int` as declared by the API.

### `public byte GetAlpha(int x, int y)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |

**Return:** Returns `byte` as declared by the API.

## `Input/JollyGamepad.cs`

### `public JollyGamepad(int index)`

| Parameter | Meaning |
|---|---|
| `index` | zero-based index. |

**Return:** Returns `JollyGamepad` as declared by the API.

### `public bool IsButtonDown(JollyGamepadButton button)`

| Parameter | Meaning |
|---|---|
| `button` | mouse/gamepad button being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool IsButtonPressed(JollyGamepadButton button)`

| Parameter | Meaning |
|---|---|
| `button` | mouse/gamepad button being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool IsButtonReleased(JollyGamepadButton button)`

| Parameter | Meaning |
|---|---|
| `button` | mouse/gamepad button being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

## `Input/JollyInput.cs`

### `public void Attach(Window window)`

| Parameter | Meaning |
|---|---|
| `window` | value supplied to the operation. |

**Return:** No value is returned.

### `public void Detach(Window window)`

| Parameter | Meaning |
|---|---|
| `window` | value supplied to the operation. |

**Return:** No value is returned.

### `public bool IsKeyDown(Key key)`

| Parameter | Meaning |
|---|---|
| `key` | keyboard key being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool IsKeyPressed(Key key)`

| Parameter | Meaning |
|---|---|
| `key` | keyboard key being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool IsKeyReleased(Key key)`

| Parameter | Meaning |
|---|---|
| `key` | keyboard key being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool IsMouseButtonDown(JollyMouseButton button)`

| Parameter | Meaning |
|---|---|
| `button` | mouse/gamepad button being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool IsMouseButtonPressed(JollyMouseButton button)`

| Parameter | Meaning |
|---|---|
| `button` | mouse/gamepad button being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool IsMouseButtonReleased(JollyMouseButton button)`

| Parameter | Meaning |
|---|---|
| `button` | mouse/gamepad button being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public Vector2 GetMouseWorldPosition(Camera2D camera)`

| Parameter | Meaning |
|---|---|
| `camera` | camera used for coordinate conversion. |

**Return:** Returns `Vector2` as declared by the API.

## `Input/JollyKeyboard.cs`

### `public void Attach(Window window)`

| Parameter | Meaning |
|---|---|
| `window` | value supplied to the operation. |

**Return:** No value is returned.

### `public void Detach(Window window)`

| Parameter | Meaning |
|---|---|
| `window` | value supplied to the operation. |

**Return:** No value is returned.

### `public bool IsKeyDown(Key key)`

| Parameter | Meaning |
|---|---|
| `key` | keyboard key being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool IsKeyPressed(Key key)`

| Parameter | Meaning |
|---|---|
| `key` | keyboard key being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool IsKeyReleased(Key key)`

| Parameter | Meaning |
|---|---|
| `key` | keyboard key being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

## `Input/JollyMouse.cs`

### `public void Attach(Window window)`

| Parameter | Meaning |
|---|---|
| `window` | value supplied to the operation. |

**Return:** No value is returned.

### `public void Detach(Window window)`

| Parameter | Meaning |
|---|---|
| `window` | value supplied to the operation. |

**Return:** No value is returned.

### `public bool IsButtonDown(JollyMouseButton button)`

| Parameter | Meaning |
|---|---|
| `button` | mouse/gamepad button being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool IsButtonPressed(JollyMouseButton button)`

| Parameter | Meaning |
|---|---|
| `button` | mouse/gamepad button being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool IsButtonReleased(JollyMouseButton button)`

| Parameter | Meaning |
|---|---|
| `button` | mouse/gamepad button being queried. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

## `Math/JollyColor.cs`

### `public float R { get; } public float G { get; } public float B { get; } public float A { get; }  public JollyColor(float r, float g, float b, float a = 1.0f)`

| Parameter | Meaning |
|---|---|
| `r` | red component. |
| `g` | green component. |
| `b` | blue component. |
| `a` | alpha component. |

**Return:** Returns `float` as declared by the API.

### `public static JollyColor White => new(1.0f, 1.0f, 1.0f, 1.0f);`

| Parameter | Meaning |
|---|---|
| `1.0f` | value supplied to the operation. |
| `1.0f` | value supplied to the operation. |
| `1.0f` | value supplied to the operation. |
| `1.0f` | value supplied to the operation. |

**Return:** Returns `JollyColor` as declared by the API.

### `public static JollyColor Black => new(0.0f, 0.0f, 0.0f, 1.0f);`

| Parameter | Meaning |
|---|---|
| `0.0f` | value supplied to the operation. |
| `0.0f` | value supplied to the operation. |
| `0.0f` | value supplied to the operation. |
| `1.0f` | value supplied to the operation. |

**Return:** Returns `JollyColor` as declared by the API.

### `public static JollyColor Red => new(1.0f, 0.0f, 0.0f, 1.0f);`

| Parameter | Meaning |
|---|---|
| `1.0f` | value supplied to the operation. |
| `0.0f` | value supplied to the operation. |
| `0.0f` | value supplied to the operation. |
| `1.0f` | value supplied to the operation. |

**Return:** Returns `JollyColor` as declared by the API.

### `public static JollyColor Green => new(0.0f, 1.0f, 0.0f, 1.0f);`

| Parameter | Meaning |
|---|---|
| `0.0f` | value supplied to the operation. |
| `1.0f` | value supplied to the operation. |
| `0.0f` | value supplied to the operation. |
| `1.0f` | value supplied to the operation. |

**Return:** Returns `JollyColor` as declared by the API.

### `public static JollyColor Blue => new(0.0f, 0.0f, 1.0f, 1.0f);`

| Parameter | Meaning |
|---|---|
| `0.0f` | value supplied to the operation. |
| `0.0f` | value supplied to the operation. |
| `1.0f` | value supplied to the operation. |
| `1.0f` | value supplied to the operation. |

**Return:** Returns `JollyColor` as declared by the API.

### `public static JollyColor Yellow => new(1.0f, 1.0f, 0.0f, 1.0f);`

| Parameter | Meaning |
|---|---|
| `1.0f` | value supplied to the operation. |
| `1.0f` | value supplied to the operation. |
| `0.0f` | value supplied to the operation. |
| `1.0f` | value supplied to the operation. |

**Return:** Returns `JollyColor` as declared by the API.

### `public static JollyColor Transparent => new(0.0f, 0.0f, 0.0f, 0.0f);`

| Parameter | Meaning |
|---|---|
| `0.0f` | value supplied to the operation. |
| `0.0f` | value supplied to the operation. |
| `0.0f` | value supplied to the operation. |
| `0.0f` | value supplied to the operation. |

**Return:** Returns `JollyColor` as declared by the API.

### `public static JollyColor FromBytes(byte r, byte g, byte b, byte a = 255)`

| Parameter | Meaning |
|---|---|
| `r` | red component. |
| `g` | green component. |
| `b` | blue component. |
| `a` | alpha component. |

**Return:** Returns `JollyColor` as declared by the API.

### `public bool Equals(JollyColor other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public override bool Equals(object? obj)`

| Parameter | Meaning |
|---|---|
| `obj` | value supplied to the operation. |

**Return:** Returns `override` as declared by the API.

### `public override int GetHashCode()`

**Return:** Returns `override` as declared by the API.

### `public static bool operator ==(JollyColor left, JollyColor right)`

| Parameter | Meaning |
|---|---|
| `left` | left world bound. |
| `right` | right world bound. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public static bool operator !=(JollyColor left, JollyColor right)`

| Parameter | Meaning |
|---|---|
| `left` | left world bound. |
| `right` | right world bound. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

## `Math/JollyVector2.cs`

### `public JollyVector2(float x, float y)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |

**Return:** Returns `JollyVector2` as declared by the API.

### `public static JollyVector2 Zero => new JollyVector2(0.0f, 0.0f);`

| Parameter | Meaning |
|---|---|
| `0.0f` | value supplied to the operation. |
| `0.0f` | value supplied to the operation. |

**Return:** Returns `JollyVector2` as declared by the API.

### `public static JollyVector2 One => new JollyVector2(1.0f, 1.0f);`

| Parameter | Meaning |
|---|---|
| `1.0f` | value supplied to the operation. |
| `1.0f` | value supplied to the operation. |

**Return:** Returns `JollyVector2` as declared by the API.

### `public static JollyVector2 operator +(JollyVector2 left, JollyVector2 right)`

| Parameter | Meaning |
|---|---|
| `left` | left world bound. |
| `right` | right world bound. |

**Return:** Returns `JollyVector2` as declared by the API.

### `public static JollyVector2 operator -(JollyVector2 left, JollyVector2 right)`

| Parameter | Meaning |
|---|---|
| `left` | left world bound. |
| `right` | right world bound. |

**Return:** Returns `JollyVector2` as declared by the API.

### `public static JollyVector2 operator *(JollyVector2 value, float scalar)`

| Parameter | Meaning |
|---|---|
| `value` | value being assigned or changed. |
| `scalar` | value supplied to the operation. |

**Return:** Returns `JollyVector2` as declared by the API.

### `public static JollyVector2 operator /(JollyVector2 value, float scalar)`

| Parameter | Meaning |
|---|---|
| `value` | value being assigned or changed. |
| `scalar` | value supplied to the operation. |

**Return:** Returns `JollyVector2` as declared by the API.

### `public float Length()`

**Return:** Returns `float` as declared by the API.

### `public float LengthSquared()`

**Return:** Returns `float` as declared by the API.

### `public void Normalize()`

**Return:** No value is returned.

### `public static JollyVector2 Normalize(JollyVector2 value)`

| Parameter | Meaning |
|---|---|
| `value` | value being assigned or changed. |

**Return:** Returns `JollyVector2` as declared by the API.

## `Networking/JollyNetworkClient.cs`

### `public void Connect(string host, int port)`

| Parameter | Meaning |
|---|---|
| `host` | remote server host name or IP address. |
| `port` | TCP listening or destination port. |

**Return:** No value is returned.

### `public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)`

| Parameter | Meaning |
|---|---|
| `host` | remote server host name or IP address. |
| `port` | TCP listening or destination port. |
| `cancellationToken` | token used to cancel the asynchronous operation. |

**Return:** Returns `async` as declared by the API.

### `public void Send(JollyNetworkMessage message)`

| Parameter | Meaning |
|---|---|
| `message` | network message to send/broadcast. |

**Return:** No value is returned.

### `public void SendText(string text)`

| Parameter | Meaning |
|---|---|
| `text` | text content to encode, render or send. |

**Return:** No value is returned.

### `public void Disconnect()`

**Return:** No value is returned.

### `public void Dispose()`

**Return:** No value is returned.

## `Networking/JollyNetworkConnection.cs`

### `public string Id { get; } = Guid.NewGuid().ToString("N");`

| Parameter | Meaning |
|---|---|
| `).ToString("N"` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

### `public void Send(JollyNetworkMessage message)`

| Parameter | Meaning |
|---|---|
| `message` | network message to send/broadcast. |

**Return:** No value is returned.

### `public void SendText(string text)`

| Parameter | Meaning |
|---|---|
| `text` | text content to encode, render or send. |

**Return:** No value is returned.

### `public void Disconnect()`

**Return:** No value is returned.

### `public void Dispose()`

**Return:** No value is returned.

## `Networking/JollyNetworkMessage.cs`

### `public JollyNetworkMessage(byte[] payload)`

| Parameter | Meaning |
|---|---|
| `payload` | value supplied to the operation. |

**Return:** Returns `JollyNetworkMessage` as declared by the API.

### `public byte[] Payload { get; }  public string Text => Encoding.UTF8.GetString(Payload);`

| Parameter | Meaning |
|---|---|
| `Payload` | value supplied to the operation. |

**Return:** Returns `byte[]` as declared by the API.

### `public static JollyNetworkMessage FromText(string text)`

| Parameter | Meaning |
|---|---|
| `text` | text content to encode, render or send. |

**Return:** Returns `JollyNetworkMessage` as declared by the API.

## `Networking/JollyNetworkServer.cs`

### `public void Start(int port)`

| Parameter | Meaning |
|---|---|
| `port` | TCP listening or destination port. |

**Return:** No value is returned.

### `public void Stop()`

**Return:** No value is returned.

### `public void Broadcast(JollyNetworkMessage message)`

| Parameter | Meaning |
|---|---|
| `message` | network message to send/broadcast. |

**Return:** No value is returned.

### `public void BroadcastText(string text)`

| Parameter | Meaning |
|---|---|
| `text` | text content to encode, render or send. |

**Return:** No value is returned.

### `public void Dispose()`

**Return:** No value is returned.

## `Particles/JollyParticleEmitter.cs`

### `public int Update(float deltaSeconds, List<JollyParticle> particles)`

| Parameter | Meaning |
|---|---|
| `deltaSeconds` | elapsed time in seconds for the current update. |
| `particles` | value supplied to the operation. |

**Return:** Returns `int` as declared by the API.

### `public void Emit(int count, List<JollyParticle> particles)`

| Parameter | Meaning |
|---|---|
| `count` | number of items/frames to process. |
| `particles` | value supplied to the operation. |

**Return:** No value is returned.

### `public void Reset()`

**Return:** No value is returned.

### `public void Burst(int count, List<JollyParticle> particles)`

| Parameter | Meaning |
|---|---|
| `count` | number of items/frames to process. |
| `particles` | value supplied to the operation. |

**Return:** No value is returned.

### `public void ApplyPreset(JollyParticlePreset preset)`

| Parameter | Meaning |
|---|---|
| `preset` | particle preset to apply. |

**Return:** No value is returned.

## `Particles/JollyParticleRenderer.cs`

### `public JollyParticleRenderer(JollyParticleSystem particleSystem, Func<JollyTexture, JollySprite> spriteFactory, Func<JollyColor, JollyTexture> textureFactory)`

| Parameter | Meaning |
|---|---|
| `particleSystem` | value supplied to the operation. |
| `spriteFactory` | value supplied to the operation. |
| `textureFactory` | value supplied to the operation. |

**Return:** Returns `JollyParticleRenderer` as declared by the API.

### `public void Update(JollyTexture texture)`

| Parameter | Meaning |
|---|---|
| `texture` | the texture used by the sprite or render operation. |

**Return:** No value is returned.

### `public void Clear()`

**Return:** No value is returned.

## `Particles/JollyParticleSystem.cs`

### `public void AddEmitter(JollyParticleEmitter emitter)`

| Parameter | Meaning |
|---|---|
| `emitter` | value supplied to the operation. |

**Return:** No value is returned.

### `public void RemoveEmitter(JollyParticleEmitter emitter)`

| Parameter | Meaning |
|---|---|
| `emitter` | value supplied to the operation. |

**Return:** No value is returned.

### `public void Update(float deltaSeconds)`

| Parameter | Meaning |
|---|---|
| `deltaSeconds` | elapsed time in seconds for the current update. |

**Return:** No value is returned.

### `public void Clear()`

**Return:** No value is returned.

### `public void Emit(JollyParticleEmitter emitter, int count)`

| Parameter | Meaning |
|---|---|
| `emitter` | value supplied to the operation. |
| `count` | number of items/frames to process. |

**Return:** No value is returned.

## `Physics/JollyCharacterController.cs`

### `public JollyCharacterController(JollyGameObject gameObject)`

| Parameter | Meaning |
|---|---|
| `gameObject` | game object being operated on. |

**Return:** Returns `JollyCharacterController` as declared by the API.

### `public void Reset()`

**Return:** No value is returned.

### `public void Move(float direction)`

| Parameter | Meaning |
|---|---|
| `direction` | movement/emission direction. |

**Return:** No value is returned.

### `public void Stop()`

**Return:** No value is returned.

### `public void SetJumpHeld(bool held)`

| Parameter | Meaning |
|---|---|
| `held` | whether an input is currently held. |

**Return:** No value is returned.

### `public void RequestJump()`

**Return:** No value is returned.

### `public void Update(JollyGameTime gameTime)`

| Parameter | Meaning |
|---|---|
| `gameTime` | the current game-time/update information. |

**Return:** No value is returned.

## `Physics/JollyCollider.cs`

### `public JollyCollider(JollySprite sprite)`

| Parameter | Meaning |
|---|---|
| `sprite` | value supplied to the operation. |

**Return:** Returns `JollyCollider` as declared by the API.

### `public bool CanCollideWith(JollyCollider other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool CollidesWith(JollyCollider other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool Check(JollyCollider other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

## `Physics/JollyCollisionDebug.cs`

### `public static bool DrawColliders { get; set; }  public static void CollisionEnter(JollyGameObject objectA, JollyGameObject objectB)`

| Parameter | Meaning |
|---|---|
| `objectA` | value supplied to the operation. |
| `objectB` | value supplied to the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public static void CollisionStay(JollyGameObject objectA, JollyGameObject objectB)`

| Parameter | Meaning |
|---|---|
| `objectA` | value supplied to the operation. |
| `objectB` | value supplied to the operation. |

**Return:** No value is returned.

### `public static void CollisionExit(JollyGameObject objectA, JollyGameObject objectB)`

| Parameter | Meaning |
|---|---|
| `objectA` | value supplied to the operation. |
| `objectB` | value supplied to the operation. |

**Return:** No value is returned.

### `public static void TriggerEnter(JollyGameObject objectA, JollyGameObject objectB)`

| Parameter | Meaning |
|---|---|
| `objectA` | value supplied to the operation. |
| `objectB` | value supplied to the operation. |

**Return:** No value is returned.

### `public static void TriggerStay(JollyGameObject objectA, JollyGameObject objectB)`

| Parameter | Meaning |
|---|---|
| `objectA` | value supplied to the operation. |
| `objectB` | value supplied to the operation. |

**Return:** No value is returned.

### `public static void TriggerExit(JollyGameObject objectA, JollyGameObject objectB)`

| Parameter | Meaning |
|---|---|
| `objectA` | value supplied to the operation. |
| `objectB` | value supplied to the operation. |

**Return:** No value is returned.

### `public static void PixelHit(JollySprite spriteA, JollySprite spriteB, int x, int y)`

| Parameter | Meaning |
|---|---|
| `spriteA` | value supplied to the operation. |
| `spriteB` | value supplied to the operation. |
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |

**Return:** No value is returned.

## `Physics/JollyCollisionResult.cs`

### `public JollySprite SpriteA { get; } public JollySprite SpriteB { get; } public bool Collided { get; }  public JollyCollisionResult(JollySprite spriteA, JollySprite spriteB, bool collided)`

| Parameter | Meaning |
|---|---|
| `spriteA` | value supplied to the operation. |
| `spriteB` | value supplied to the operation. |
| `collided` | value supplied to the operation. |

**Return:** Returns `JollySprite` as declared by the API.

## `Physics/JollyCollisionSystem.cs`

### `public static bool Check(JollyCollider colliderA, JollyCollider colliderB)`

| Parameter | Meaning |
|---|---|
| `colliderA` | value supplied to the operation. |
| `colliderB` | value supplied to the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

## `Physics/JollyPhysicsSystem.cs`

### `public static void Update(JollyGameObject gameObject, JollyGameTime gameTime)`

| Parameter | Meaning |
|---|---|
| `gameObject` | game object being operated on. |
| `gameTime` | the current game-time/update information. |

**Return:** No value is returned.

### `public static void ResolveTileMap(JollyGameObject gameObject, JollyTileMap tileMap, float tileMapX, float tileMapY, float deltaTime)`

| Parameter | Meaning |
|---|---|
| `gameObject` | game object being operated on. |
| `tileMap` | value supplied to the operation. |
| `tileMapX` | value supplied to the operation. |
| `tileMapY` | value supplied to the operation. |
| `deltaTime` | elapsed time in seconds for the current update. |

**Return:** No value is returned.

### `public static bool Jump(JollyGameObject gameObject, float velocity)`

| Parameter | Meaning |
|---|---|
| `gameObject` | game object being operated on. |
| `velocity` | velocity vector or initial velocity. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public static void Resolve(JollyGameObject objectA, JollyGameObject objectB, float deltaTime)`

| Parameter | Meaning |
|---|---|
| `objectA` | value supplied to the operation. |
| `objectB` | value supplied to the operation. |
| `deltaTime` | elapsed time in seconds for the current update. |

**Return:** No value is returned.

## `Physics/JollyProjectileObject.cs`

### `public bool IsExpired { get; private set; }  public JollyProjectileObject(JollySprite sprite) : base(sprite)`

| Parameter | Meaning |
|---|---|
| `base(sprite` | value supplied to the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public override void Initialize()`

**Return:** Returns `override` as declared by the API.

### `public override void Update(JollyGameTime gameTime)`

| Parameter | Meaning |
|---|---|
| `gameTime` | the current game-time/update information. |

**Return:** Returns `override` as declared by the API.

### `public override void OnCollisionEnter(JollyGameObject other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `override` as declared by the API.

## `Rendering/Camera/Camera2D.cs`

### `public void SetViewport(float width, float height)`

| Parameter | Meaning |
|---|---|
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |

**Return:** No value is returned.

### `public void SetBounds(float left, float top, float right, float bottom)`

| Parameter | Meaning |
|---|---|
| `left` | left world bound. |
| `top` | top world bound. |
| `right` | right world bound. |
| `bottom` | bottom world bound. |

**Return:** No value is returned.

### `public void ClearBounds()`

**Return:** No value is returned.

### `public void Follow(Vector2 targetPosition)`

| Parameter | Meaning |
|---|---|
| `targetPosition` | target world position for camera following. |

**Return:** No value is returned.

### `public void FollowSmooth(Vector2 targetPosition, float deltaSeconds)`

| Parameter | Meaning |
|---|---|
| `targetPosition` | target world position for camera following. |
| `deltaSeconds` | elapsed time in seconds for the current update. |

**Return:** No value is returned.

### `public Vector2 WorldToScreen(Vector2 worldPosition)`

| Parameter | Meaning |
|---|---|
| `worldPosition` | world-space position. |

**Return:** Returns `Vector2` as declared by the API.

### `public Vector2 ScreenToWorld(Vector2 screenPosition)`

| Parameter | Meaning |
|---|---|
| `screenPosition` | screen-space position. |

**Return:** Returns `Vector2` as declared by the API.

### `public Matrix3x2 GetWorldToScreenMatrix()`

**Return:** Returns `Matrix3x2` as declared by the API.

### `public CameraState CaptureState()`

**Return:** Returns `CameraState` as declared by the API.

### `public void Shake(float intensity, float duration)`

| Parameter | Meaning |
|---|---|
| `intensity` | strength of a visual/physical effect. |
| `duration` | duration in seconds. |

**Return:** No value is returned.

### `public void UpdateShake(float deltaSeconds)`

| Parameter | Meaning |
|---|---|
| `deltaSeconds` | elapsed time in seconds for the current update. |

**Return:** No value is returned.

## `Rendering/Camera/CameraState.cs`

### `public Vector2 Position { get; }  public float Zoom { get; }  public float Rotation { get; }  public Vector2 ViewportSize { get; }  public CameraState( Vector2 position, float zoom, float rotation, Vector2 viewportSize)`

| Parameter | Meaning |
|---|---|
| `position` | world or screen position. |
| `zoom` | value supplied to the operation. |
| `rotation` | rotation in radians unless the containing API specifies otherwise. |
| `viewportSize` | render viewport dimensions. |

**Return:** Returns `Vector2` as declared by the API.

### `public Matrix3x2 GetWorldToScreenMatrix()`

**Return:** Returns `Matrix3x2` as declared by the API.

### `public Vector2 WorldToScreen(Vector2 worldPosition)`

| Parameter | Meaning |
|---|---|
| `worldPosition` | world-space position. |

**Return:** Returns `Vector2` as declared by the API.

### `public Vector2 ScreenToWorld(Vector2 screenPosition)`

| Parameter | Meaning |
|---|---|
| `screenPosition` | screen-space position. |

**Return:** Returns `Vector2` as declared by the API.

## `Rendering/DirectX/DirectXControl.xaml.cs`

### `public JollyDiagnostics? Diagnostics { get; set; }  public DirectXControl()`

**Return:** Returns `JollyDiagnostics?` as declared by the API.

### `public static readonly System.Windows.DependencyProperty FPSProperty = System.Windows.DependencyProperty.Register(nameof(FPS), typeof(double), typeof(DirectXControl), new System.Windows.PropertyMetadata(0.0));`

| Parameter | Meaning |
|---|---|
| `nameof(FPS)` | value supplied to the operation. |
| `typeof(double)` | value supplied to the operation. |
| `typeof(DirectXControl)` | value supplied to the operation. |
| `System.Windows.PropertyMetadata(0.0)` | value supplied to the operation. |

**Return:** Returns `readonly` as declared by the API.

### `public static readonly System.Windows.DependencyProperty FrameTimeProperty = System.Windows.DependencyProperty.Register(nameof(FrameTime), typeof(double), typeof(DirectXControl), new System.Windows.PropertyMetadata(0.0));`

| Parameter | Meaning |
|---|---|
| `nameof(FrameTime)` | value supplied to the operation. |
| `typeof(double)` | value supplied to the operation. |
| `typeof(DirectXControl)` | value supplied to the operation. |
| `System.Windows.PropertyMetadata(0.0)` | value supplied to the operation. |

**Return:** Returns `readonly` as declared by the API.

### `public async Task WaitForReadyAsync()`

**Return:** Returns `async` as declared by the API.

### `public void LoadImage(string filePath)`

| Parameter | Meaning |
|---|---|
| `filePath` | path to the source file. |

**Return:** No value is returned.

### `public void SetImage(BitmapSource source)`

| Parameter | Meaning |
|---|---|
| `source` | source image/stream/resource. |

**Return:** No value is returned.

### `public void SetImage(byte[] pixels, int width, int height)`

| Parameter | Meaning |
|---|---|
| `pixels` | pixel buffer. |
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |

**Return:** No value is returned.

### `public void SetImage(DirectXImage image)`

| Parameter | Meaning |
|---|---|
| `image` | value supplied to the operation. |

**Return:** No value is returned.

### `public void ClearScreen()`

**Return:** No value is returned.

### `public void Clear()`

**Return:** No value is returned.

### `public DirectXTexture CreateTexture(string filePath)`

| Parameter | Meaning |
|---|---|
| `filePath` | path to the source file. |

**Return:** Returns `DirectXTexture` as declared by the API.

### `public DirectXTexture CreateTexture(DirectXImage image)`

| Parameter | Meaning |
|---|---|
| `image` | value supplied to the operation. |

**Return:** Returns `DirectXTexture` as declared by the API.

### `public DirectXJollyRenderer CreateJollyRenderer()`

**Return:** Returns `DirectXJollyRenderer` as declared by the API.

## `Rendering/DirectX/DirectXImage.cs`

### `public int Width { get; } public int Height { get; } public ReadOnlyMemory<byte> Pixels { get; }  public DirectXImage(int width, int height, ReadOnlyMemory<byte> pixels)`

| Parameter | Meaning |
|---|---|
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |
| `pixels` | pixel buffer. |

**Return:** Returns `int` as declared by the API.

### `public static DirectXImage FromFile(string filePath)`

| Parameter | Meaning |
|---|---|
| `filePath` | path to the source file. |

**Return:** Returns `DirectXImage` as declared by the API.

### `public static DirectXImage FromBitmapSource(BitmapSource source)`

| Parameter | Meaning |
|---|---|
| `source` | source image/stream/resource. |

**Return:** Returns `DirectXImage` as declared by the API.

## `Rendering/DirectX/DirectXRenderer.cs`

### `public SpriteVertex(float x, float y, float z, float u, float v)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |
| `z` | value supplied to the operation. |
| `u` | value supplied to the operation. |
| `v` | value supplied to the operation. |

**Return:** Returns `SpriteVertex` as declared by the API.

### `public LineVertex(float x, float y, JollyColor color, float edge, float coreRatio)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |
| `color` | colour applied to the resource. |
| `edge` | value supplied to the operation. |
| `coreRatio` | value supplied to the operation. |

**Return:** Returns `LineVertex` as declared by the API.

### `public double FPS => Volatile.Read(ref _fps);`

| Parameter | Meaning |
|---|---|
| `_fps` | value supplied to the operation. |

**Return:** Returns `double` as declared by the API.

### `public double FrameTime => Volatile.Read(ref _frameTime);`

| Parameter | Meaning |
|---|---|
| `_frameTime` | value supplied to the operation. |

**Return:** Returns `double` as declared by the API.

### `public void Initialize(IntPtr hwnd, int width, int height)`

| Parameter | Meaning |
|---|---|
| `hwnd` | value supplied to the operation. |
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |

**Return:** No value is returned.

### `public void StartRenderLoop()`

**Return:** No value is returned.

### `public void StopRenderLoop()`

**Return:** No value is returned.

### `public void SetStretchMode(DirectXStretchMode mode)`

| Parameter | Meaning |
|---|---|
| `mode` | operation or collision mode to use. |

**Return:** No value is returned.

### `public void SetImageFilter(DirectXImageFilter filter)`

| Parameter | Meaning |
|---|---|
| `filter` | texture sampling filter. |

**Return:** No value is returned.

### `public void LoadImage(string filePath)`

| Parameter | Meaning |
|---|---|
| `filePath` | path to the source file. |

**Return:** No value is returned.

### `public void SetImage(BitmapSource source)`

| Parameter | Meaning |
|---|---|
| `source` | source image/stream/resource. |

**Return:** No value is returned.

### `public void SetImage(byte[] pixels, int width, int height)`

| Parameter | Meaning |
|---|---|
| `pixels` | pixel buffer. |
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |

**Return:** No value is returned.

### `public void SetImage(DirectXImage image)`

| Parameter | Meaning |
|---|---|
| `image` | value supplied to the operation. |

**Return:** No value is returned.

### `public DirectXTexture CreateTextureResource(string filePath)`

| Parameter | Meaning |
|---|---|
| `filePath` | path to the source file. |

**Return:** Returns `DirectXTexture` as declared by the API.

### `public DirectXTexture CreateTextureResource(DirectXImage image)`

| Parameter | Meaning |
|---|---|
| `image` | value supplied to the operation. |

**Return:** Returns `DirectXTexture` as declared by the API.

### `public void SubmitRenderFrame(JollyRenderFrame frame, Camera2D camera)`

| Parameter | Meaning |
|---|---|
| `frame` | value supplied to the operation. |
| `camera` | camera used for coordinate conversion. |

**Return:** No value is returned.

### `public void Clear()`

**Return:** No value is returned.

### `public void ClearScreen()`

**Return:** No value is returned.

### `public void Render()`

**Return:** No value is returned.

### `public void Resize(int width, int height)`

| Parameter | Meaning |
|---|---|
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |

**Return:** No value is returned.

### `public ImageVertex(float x, float y, float z, float u, float v)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |
| `z` | value supplied to the operation. |
| `u` | value supplied to the operation. |
| `v` | value supplied to the operation. |

**Return:** Returns `ImageVertex` as declared by the API.

### `public void Dispose()`

**Return:** No value is returned.

## `Rendering/DirectX/DirectXSpriteBinding.cs`

### `public JollySprite Sprite { get; }  public DirectXSpriteState State { get; }  public DirectXSpriteBinding(JollySprite sprite, DirectXTexture texture)`

| Parameter | Meaning |
|---|---|
| `sprite` | value supplied to the operation. |
| `texture` | the texture used by the sprite or render operation. |

**Return:** Returns `JollySprite` as declared by the API.

### `public void Synchronize()`

**Return:** No value is returned.

## `Rendering/DirectX/DirectXSpriteState.cs`

### `public DirectXSpriteState(DirectXTexture texture)`

| Parameter | Meaning |
|---|---|
| `texture` | the texture used by the sprite or render operation. |

**Return:** Returns `DirectXSpriteState` as declared by the API.

## `Rendering/DirectX/DirectXTexture.cs`

### `public DirectXImage Image { get; }  internal DirectXTexture(DirectXImage image)`

| Parameter | Meaning |
|---|---|
| `image` | value supplied to the operation. |

**Return:** Returns `DirectXImage` as declared by the API.

## `Rendering/DirectX/JollyPhysicsDebugRenderer.cs`

### `public JollyPhysicsDebugRenderer(DirectXJollyRenderer renderer)`

| Parameter | Meaning |
|---|---|
| `renderer` | the renderer used to create or submit graphics resources. |

**Return:** Returns `JollyPhysicsDebugRenderer` as declared by the API.

### `public void Update(IReadOnlyList<JollyGameObject> objects)`

| Parameter | Meaning |
|---|---|
| `objects` | value supplied to the operation. |

**Return:** No value is returned.

### `public void Update(IReadOnlyList<JollyGameObject> objects, JollyTileMap? tileMap, float tileMapX, float tileMapY)`

| Parameter | Meaning |
|---|---|
| `objects` | value supplied to the operation. |
| `tileMap` | value supplied to the operation. |
| `tileMapX` | value supplied to the operation. |
| `tileMapY` | value supplied to the operation. |

**Return:** No value is returned.

## `Rendering/DirectXJollyRenderer.cs`

### `public Camera2D Camera { get; } = new();`

**Return:** Returns `Camera2D` as declared by the API.

### `public DirectXJollyRenderer(DirectXControl control)`

| Parameter | Meaning |
|---|---|
| `control` | the WPF DirectX control hosting the renderer. |

**Return:** Returns `DirectXJollyRenderer` as declared by the API.

### `public override JollyTexture LoadTexture(string filePath)`

| Parameter | Meaning |
|---|---|
| `filePath` | path to the source file. |

**Return:** Returns `override` as declared by the API.

### `public JollyTexture CreateSolidTexture(byte red, byte green, byte blue, byte alpha = 255)`

| Parameter | Meaning |
|---|---|
| `red` | red colour component (0..255). |
| `green` | green colour component (0..255). |
| `blue` | blue colour component (0..255). |
| `alpha` | alpha component, normally 0..255 for byte-based creation APIs. |

**Return:** Returns `JollyTexture` as declared by the API.

### `public JollyTexture CreateCircleTexture(byte red, byte green, byte blue, byte alpha = 255, int size = 32)`

| Parameter | Meaning |
|---|---|
| `red` | red colour component (0..255). |
| `green` | green colour component (0..255). |
| `blue` | blue colour component (0..255). |
| `alpha` | alpha component, normally 0..255 for byte-based creation APIs. |
| `size` | size of the generated or configured object. |

**Return:** Returns `JollyTexture` as declared by the API.

### `public override JollySprite CreateSprite(JollyTexture texture)`

| Parameter | Meaning |
|---|---|
| `texture` | the texture used by the sprite or render operation. |

**Return:** Returns `override` as declared by the API.

### `public void SubmitFrame(JollyRenderFrame frame)`

| Parameter | Meaning |
|---|---|
| `frame` | value supplied to the operation. |

**Return:** No value is returned.

### `public JollyTexture CreateTextTexture(string text, string fontFamily, float fontSize, JollyColor color, bool bold = false, bool italic = false)`

| Parameter | Meaning |
|---|---|
| `text` | text content to encode, render or send. |
| `fontFamily` | font family name used to render text. |
| `fontSize` | font size used to render text. |
| `color` | colour applied to the resource. |
| `bold` | whether text should use a bold typeface. |
| `italic` | whether text should use an italic typeface. |

**Return:** Returns `JollyTexture` as declared by the API.

### `public void Resize(float width, float height)`

| Parameter | Meaning |
|---|---|
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |

**Return:** No value is returned.

## `Rendering/JollyRenderSubmission.cs`

### `public JollyRenderFrame Frame { get; }  public CameraState Camera { get; }  public JollyRenderSubmission(JollyRenderFrame frame, Camera2D camera)`

| Parameter | Meaning |
|---|---|
| `frame` | value supplied to the operation. |
| `camera` | camera used for coordinate conversion. |

**Return:** Returns `JollyRenderFrame` as declared by the API.

## `Rendering/JollySpriteSheet.cs`

### `public int Index { get; } public int Column { get; } public int Row { get; } public int X { get; } public int Y { get; } public int Width { get; } public int Height { get; }  internal JollySpriteSheetFrame(int index, int column, int row, int x, int y, int width, int height)`

| Parameter | Meaning |
|---|---|
| `index` | zero-based index. |
| `column` | value supplied to the operation. |
| `row` | value supplied to the operation. |
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |

**Return:** Returns `int` as declared by the API.

### `public JollySpriteSheetOrder Order { get; }  public JollySpriteSheet(JollyTexture texture, int frameWidth, int frameHeight, JollySpriteSheetOrder order = JollySpriteSheetOrder.Horizontal)`

| Parameter | Meaning |
|---|---|
| `texture` | the texture used by the sprite or render operation. |
| `frameWidth` | value supplied to the operation. |
| `frameHeight` | value supplied to the operation. |
| `order` | value supplied to the operation. |

**Return:** Returns `JollySpriteSheetOrder` as declared by the API.

### `public JollySpriteSheetFrame GetFrame(int index)`

| Parameter | Meaning |
|---|---|
| `index` | zero-based index. |

**Return:** Returns `JollySpriteSheetFrame` as declared by the API.

## `Rendering/JollyTextureManager.cs`

### `public JollyTextureManager(JollyRenderer renderer)`

| Parameter | Meaning |
|---|---|
| `renderer` | the renderer used to create or submit graphics resources. |

**Return:** Returns `JollyTextureManager` as declared by the API.

### `public JollyTexture Load(string filePath)`

| Parameter | Meaning |
|---|---|
| `filePath` | path to the source file. |

**Return:** Returns `JollyTexture` as declared by the API.

### `public bool TryGet(string filePath, out JollyTexture? texture)`

| Parameter | Meaning |
|---|---|
| `filePath` | path to the source file. |
| `texture` | the texture used by the sprite or render operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool Contains(string filePath)`

| Parameter | Meaning |
|---|---|
| `filePath` | path to the source file. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public void Clear()`

**Return:** No value is returned.

## `Save/JollyGameObjectSaveState.cs`

### `public bool Active { get; set; } public JollyVector2 Position { get; set; } public JollyVector2 Scale { get; set; } public float Rotation { get; set; } public bool Visible { get; set; } public JollyVector2 Velocity { get; set; } public JollyVector2 Acceleration { get; set; } public JollyVector2 Gravity { get; set; } public float Drag { get; set; } public bool UsePhysics { get; set; } public bool UseGravity { get; set; } public bool IsGrounded { get; set; } public bool WasGrounded { get; set; } public float GroundFriction { get; set; } public float AirFriction { get; set; } public float Restitution { get; set; } public JollyColliderSaveState? Collider { get; set; }  public static JollyGameObjectSaveState Capture(JollyGameObject gameObject)`

| Parameter | Meaning |
|---|---|
| `gameObject` | game object being operated on. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public void ApplyTo(JollyGameObject gameObject)`

| Parameter | Meaning |
|---|---|
| `gameObject` | game object being operated on. |

**Return:** No value is returned.

### `public bool Enabled { get; set; } public bool IsTrigger { get; set; } public JollyCollisionLayer Layer { get; set; } public JollyCollisionLayer Mask { get; set; } public JollyColliderShape Shape { get; set; } public float Width { get; set; } public float Height { get; set; } public float Radius { get; set; } public byte AlphaThreshold { get; set; }  public static JollyColliderSaveState Capture(JollyCollider collider)`

| Parameter | Meaning |
|---|---|
| `collider` | value supplied to the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public void ApplyTo(JollyCollider collider)`

| Parameter | Meaning |
|---|---|
| `collider` | value supplied to the operation. |

**Return:** No value is returned.

## `Save/JollySaveManager.cs`

### `public string SaveDirectory { get; }  public JollySaveManager(string? saveDirectory = null)`

| Parameter | Meaning |
|---|---|
| `saveDirectory` | value supplied to the operation. |

**Return:** Returns `string` as declared by the API.

### `public void SaveGame<T>(string slot, T state, int gameVersion = 1)`

| Parameter | Meaning |
|---|---|
| `slot` | save-slot identifier. |
| `state` | state object to serialize or apply. |
| `gameVersion` | game-defined save version. |

**Return:** No value is returned.

### `public T LoadGame<T>(string slot, int? expectedGameVersion = null)`

| Parameter | Meaning |
|---|---|
| `slot` | save-slot identifier. |
| `expectedGameVersion` | optional game version that must match when loading. |

**Return:** Returns `T` as declared by the API.

### `public bool TryLoadGame<T>(string slot, out T? state, int? expectedGameVersion = null)`

| Parameter | Meaning |
|---|---|
| `slot` | save-slot identifier. |
| `state` | state object to serialize or apply. |
| `expectedGameVersion` | optional game version that must match when loading. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool Exists(string slot)`

| Parameter | Meaning |
|---|---|
| `slot` | save-slot identifier. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool HasBackup(string slot)`

| Parameter | Meaning |
|---|---|
| `slot` | save-slot identifier. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool RestoreBackup(string slot)`

| Parameter | Meaning |
|---|---|
| `slot` | save-slot identifier. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public void Delete(string slot)`

| Parameter | Meaning |
|---|---|
| `slot` | save-slot identifier. |

**Return:** No value is returned.

### `public IReadOnlyList<string> GetSlots()`

**Return:** Returns `IReadOnlyList<string>` as declared by the API.

### `public JollySaveException(string message) : base(message)`

| Parameter | Meaning |
|---|---|
| `base(message` | value supplied to the operation. |

**Return:** Returns `JollySaveException` as declared by the API.

### `public JollySaveException(string message, Exception innerException) : base(message, innerException)`

| Parameter | Meaning |
|---|---|
| `message` | network message to send/broadcast. |
| `innerException` | value supplied to the operation. |

**Return:** Returns `JollySaveException` as declared by the API.

## `Save/JollySceneSaveState.cs`

### `public List<JollyGameObjectSaveState> Objects { get; set; } = new();`

**Return:** Returns `List<JollyGameObjectSaveState>` as declared by the API.

### `public static JollySceneSaveState Capture(JollyScene scene)`

| Parameter | Meaning |
|---|---|
| `scene` | value supplied to the operation. |

**Return:** Returns `JollySceneSaveState` as declared by the API.

### `public int ApplyTo(JollyScene scene)`

| Parameter | Meaning |
|---|---|
| `scene` | value supplied to the operation. |

**Return:** Returns `int` as declared by the API.

## `Scene/JollyScene.cs`

### `public void SetTileMap(JollyTileMap tileMap, float x, float y)`

| Parameter | Meaning |
|---|---|
| `tileMap` | value supplied to the operation. |
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |

**Return:** No value is returned.

### `public void ClearTileMap()`

**Return:** No value is returned.

### `public virtual void OnEnter()`

**Return:** Returns `virtual` as declared by the API.

### `public virtual void OnExit()`

**Return:** Returns `virtual` as declared by the API.

### `public virtual void Initialize()`

**Return:** Returns `virtual` as declared by the API.

### `public void Add(JollyGameObject gameObject)`

| Parameter | Meaning |
|---|---|
| `gameObject` | game object being operated on. |

**Return:** No value is returned.

### `public void Spawn(JollyGameObject gameObject)`

| Parameter | Meaning |
|---|---|
| `gameObject` | game object being operated on. |

**Return:** No value is returned.

### `public void Remove(JollyGameObject gameObject)`

| Parameter | Meaning |
|---|---|
| `gameObject` | game object being operated on. |

**Return:** No value is returned.

### `public void Despawn(JollyGameObject gameObject)`

| Parameter | Meaning |
|---|---|
| `gameObject` | game object being operated on. |

**Return:** No value is returned.

### `public virtual void Update(JollyGameTime gameTime)`

| Parameter | Meaning |
|---|---|
| `gameTime` | the current game-time/update information. |

**Return:** Returns `virtual` as declared by the API.

### `public virtual void Draw()`

**Return:** Returns `virtual` as declared by the API.

### `public IReadOnlyList<JollySprite> GetSprites()`

**Return:** Returns `IReadOnlyList<JollySprite>` as declared by the API.

### `public JollyGameObject? FindBySaveId(string saveId)`

| Parameter | Meaning |
|---|---|
| `saveId` | value supplied to the operation. |

**Return:** Returns `JollyGameObject?` as declared by the API.

### `public JollyGameObject? Find(string name)`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |

**Return:** Returns `JollyGameObject?` as declared by the API.

### `public T? Find<T>(string name) where T : JollyGameObject`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |

**Return:** Returns `T?` as declared by the API.

### `public IReadOnlyList<T> FindAll<T>() where T : JollyGameObject`

**Return:** Returns `IReadOnlyList<T>` as declared by the API.

### `public bool Contains(JollyGameObject gameObject)`

| Parameter | Meaning |
|---|---|
| `gameObject` | game object being operated on. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public virtual void Shutdown()`

**Return:** Returns `virtual` as declared by the API.

### `public static void ResolveTileMap(JollyGameObject gameObject, JollyTileMap tileMap, float tileMapX, float tileMapY, float deltaTime)`

| Parameter | Meaning |
|---|---|
| `gameObject` | game object being operated on. |
| `tileMap` | value supplied to the operation. |
| `tileMapX` | value supplied to the operation. |
| `tileMapY` | value supplied to the operation. |
| `deltaTime` | elapsed time in seconds for the current update. |

**Return:** No value is returned.

### `public JollyGameObject ObjectA { get; }  public JollyGameObject ObjectB { get; }  public CollisionPair(JollyGameObject objectA, JollyGameObject objectB)`

| Parameter | Meaning |
|---|---|
| `objectA` | value supplied to the operation. |
| `objectB` | value supplied to the operation. |

**Return:** Returns `JollyGameObject` as declared by the API.

### `public bool Equals(CollisionPair other)`

| Parameter | Meaning |
|---|---|
| `other` | the other game object or collider involved in the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public override bool Equals(object? obj)`

| Parameter | Meaning |
|---|---|
| `obj` | value supplied to the operation. |

**Return:** Returns `override` as declared by the API.

### `public override int GetHashCode()`

**Return:** Returns `override` as declared by the API.

## `Scene/JollySceneManager.cs`

### `public void ChangeScene(JollyScene scene)`

| Parameter | Meaning |
|---|---|
| `scene` | value supplied to the operation. |

**Return:** No value is returned.

### `public JollyGameObject? Find(string name)`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |

**Return:** Returns `JollyGameObject?` as declared by the API.

### `public T? Find<T>(string name) where T : JollyGameObject`

| Parameter | Meaning |
|---|---|
| `name` | name used to identify the object, asset, animation, sound or slot. |

**Return:** Returns `T?` as declared by the API.

### `public IReadOnlyList<T> FindAll<T>() where T : JollyGameObject`

**Return:** Returns `IReadOnlyList<T>` as declared by the API.

### `public void Update(JollyGameTime gameTime)`

| Parameter | Meaning |
|---|---|
| `gameTime` | the current game-time/update information. |

**Return:** No value is returned.

### `public void Draw()`

**Return:** No value is returned.

### `public void Shutdown()`

**Return:** No value is returned.

## `UI/JollyUI9Slice.cs`

### `public JollyTexture Texture { get; } public float Left { get; set; } public float Top { get; set; } public float Right { get; set; } public float Bottom { get; set; }  public JollyUI9Slice(JollyTexture texture, float left, float top, float right, float bottom)`

| Parameter | Meaning |
|---|---|
| `texture` | the texture used by the sprite or render operation. |
| `left` | left world bound. |
| `top` | top world bound. |
| `right` | right world bound. |
| `bottom` | bottom world bound. |

**Return:** Returns `JollyTexture` as declared by the API.

## `UI/JollyUIAnimation.cs`

### `public JollyUIElement Element { get; } public JollyUIAnimationProperty Property { get; } public float Duration { get; } public bool IsComplete { get; private set; }  internal JollyUIAnimation(JollyUIElement element, JollyUIAnimationProperty property, Vector2 start, Vector2 end, float duration)`

| Parameter | Meaning |
|---|---|
| `element` | value supplied to the operation. |
| `property` | UI animation property. |
| `start` | line start position. |
| `end` | line end position. |
| `duration` | duration in seconds. |

**Return:** Returns `JollyUIElement` as declared by the API.

## `UI/JollyUIButton.cs`

### `public JollyColor NormalColor { get; set; } = JollyColor.FromBytes(60, 60, 60, 255);`

| Parameter | Meaning |
|---|---|
| `60` | value supplied to the operation. |
| `60` | value supplied to the operation. |
| `60` | value supplied to the operation. |
| `255` | value supplied to the operation. |

**Return:** Returns `JollyColor` as declared by the API.

### `public JollyColor HoverColor { get; set; } = JollyColor.FromBytes(80, 80, 80, 255);`

| Parameter | Meaning |
|---|---|
| `80` | value supplied to the operation. |
| `80` | value supplied to the operation. |
| `80` | value supplied to the operation. |
| `255` | value supplied to the operation. |

**Return:** Returns `JollyColor` as declared by the API.

### `public JollyColor PressedColor { get; set; } = JollyColor.FromBytes(40, 40, 40, 255);`

| Parameter | Meaning |
|---|---|
| `40` | value supplied to the operation. |
| `40` | value supplied to the operation. |
| `40` | value supplied to the operation. |
| `255` | value supplied to the operation. |

**Return:** Returns `JollyColor` as declared by the API.

### `public JollyColor DisabledColor { get; set; } = JollyColor.FromBytes(35, 35, 35, 255);`

| Parameter | Meaning |
|---|---|
| `35` | value supplied to the operation. |
| `35` | value supplied to the operation. |
| `35` | value supplied to the operation. |
| `255` | value supplied to the operation. |

**Return:** Returns `JollyColor` as declared by the API.

## `UI/JollyUIElement.cs`

### `public bool ContainsPoint(Vector2 point)`

| Parameter | Meaning |
|---|---|
| `point` | value supplied to the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public void AddChild(JollyUIElement child)`

| Parameter | Meaning |
|---|---|
| `child` | value supplied to the operation. |

**Return:** No value is returned.

### `public bool RemoveChild(JollyUIElement child)`

| Parameter | Meaning |
|---|---|
| `child` | value supplied to the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public void RemoveAllChildren()`

**Return:** No value is returned.

### `public void SetMargin(float left, float top, float right, float bottom)`

| Parameter | Meaning |
|---|---|
| `left` | left world bound. |
| `top` | top world bound. |
| `right` | right world bound. |
| `bottom` | bottom world bound. |

**Return:** No value is returned.

## `UI/JollyUIManager.cs`

### `public float ScreenHeight { get; private set; }  public void SetScreenSize(float width, float height)`

| Parameter | Meaning |
|---|---|
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |

**Return:** Returns `float` as declared by the API.

### `public void Add(JollyUIElement element)`

| Parameter | Meaning |
|---|---|
| `element` | value supplied to the operation. |

**Return:** No value is returned.

### `public bool Remove(JollyUIElement element)`

| Parameter | Meaning |
|---|---|
| `element` | value supplied to the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public void Clear()`

**Return:** No value is returned.

### `public JollyUIElement? HitTest(Vector2 point)`

| Parameter | Meaning |
|---|---|
| `point` | value supplied to the operation. |

**Return:** Returns `JollyUIElement?` as declared by the API.

### `public void MouseMove(float x, float y)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |

**Return:** No value is returned.

### `public void MouseDown()`

**Return:** No value is returned.

### `public void MouseUp()`

**Return:** No value is returned.

### `public JollyUIAnimation AnimatePosition(JollyUIElement element, Vector2 target, float duration)`

| Parameter | Meaning |
|---|---|
| `element` | value supplied to the operation. |
| `target` | value supplied to the operation. |
| `duration` | duration in seconds. |

**Return:** Returns `JollyUIAnimation` as declared by the API.

### `public JollyUIAnimation AnimateSize(JollyUIElement element, Vector2 target, float duration)`

| Parameter | Meaning |
|---|---|
| `element` | value supplied to the operation. |
| `target` | value supplied to the operation. |
| `duration` | duration in seconds. |

**Return:** Returns `JollyUIAnimation` as declared by the API.

### `public JollyUIAnimation AnimateOpacity(JollyUIElement element, float target, float duration)`

| Parameter | Meaning |
|---|---|
| `element` | value supplied to the operation. |
| `target` | value supplied to the operation. |
| `duration` | duration in seconds. |

**Return:** Returns `JollyUIAnimation` as declared by the API.

### `public void Update(float deltaSeconds)`

| Parameter | Meaning |
|---|---|
| `deltaSeconds` | elapsed time in seconds for the current update. |

**Return:** No value is returned.

### `public void UpdateLayout()`

**Return:** No value is returned.

## `UI/JollyUIRenderer.cs`

### `public JollyUIRenderer(JollyUIManager uiManager, Func<JollyTexture, JollySprite> spriteFactory, Func<byte, byte, byte, byte, JollyTexture> textureFactory, Func<string, string, float, JollyColor, bool, bool, JollyTexture> textTextureFactory)`

| Parameter | Meaning |
|---|---|
| `uiManager` | value supplied to the operation. |
| `spriteFactory` | value supplied to the operation. |
| `textureFactory` | value supplied to the operation. |
| `textTextureFactory` | value supplied to the operation. |

**Return:** Returns `JollyUIRenderer` as declared by the API.

### `public void Update()`

**Return:** No value is returned.

### `public void Clear()`

**Return:** No value is returned.

## `UI/JollyUISlider.cs`

### `public JollyColor TrackColor { get; set; } = JollyColor.FromBytes(50, 50, 50, 255);`

| Parameter | Meaning |
|---|---|
| `50` | value supplied to the operation. |
| `50` | value supplied to the operation. |
| `50` | value supplied to the operation. |
| `255` | value supplied to the operation. |

**Return:** Returns `JollyColor` as declared by the API.

### `public JollyColor FillColor { get; set; } = JollyColor.FromBytes(80, 160, 240, 255);`

| Parameter | Meaning |
|---|---|
| `80` | value supplied to the operation. |
| `160` | value supplied to the operation. |
| `240` | value supplied to the operation. |
| `255` | value supplied to the operation. |

**Return:** Returns `JollyColor` as declared by the API.

## `World/JollyTileMap.cs`

### `public int Width { get; } public int Height { get; } public float TileWidth { get; } public float TileHeight { get; }  public JollyTileMap(int width, int height, float tileWidth, float tileHeight)`

| Parameter | Meaning |
|---|---|
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |
| `tileWidth` | value supplied to the operation. |
| `tileHeight` | value supplied to the operation. |

**Return:** Returns `int` as declared by the API.

### `public void NotifyHazardContact(JollyGameObject gameObject, int tileX, int tileY, int tileIndex)`

| Parameter | Meaning |
|---|---|
| `gameObject` | game object being operated on. |
| `tileX` | value supplied to the operation. |
| `tileY` | value supplied to the operation. |
| `tileIndex` | value supplied to the operation. |

**Return:** No value is returned.

### `public bool IsSolid(int x, int y)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public void SetSolid(int x, int y, bool solid)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |
| `solid` | value supplied to the operation. |

**Return:** No value is returned.

### `public int GetTile(int x, int y)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |

**Return:** Returns `int` as declared by the API.

### `public void SetTile(int x, int y, int tileIndex)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |
| `tileIndex` | value supplied to the operation. |

**Return:** No value is returned.

### `public Vector2 GetTilePosition(int x, int y)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |

**Return:** Returns `Vector2` as declared by the API.

### `public Vector2 WorldToTile(float x, float y)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |

**Return:** Returns `Vector2` as declared by the API.

### `public bool IsSolidAtWorldPosition(float x, float y)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public bool TryGetTileAtWorldPosition(float x, float y, out int tileIndex)`

| Parameter | Meaning |
|---|---|
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |
| `tileIndex` | value supplied to the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public void SetTexture(JollyTexture texture)`

| Parameter | Meaning |
|---|---|
| `texture` | the texture used by the sprite or render operation. |

**Return:** No value is returned.

### `public bool TryGetTileSourceRectangle(int tileIndex, out int x, out int y, out int width, out int height)`

| Parameter | Meaning |
|---|---|
| `tileIndex` | value supplied to the operation. |
| `x` | X coordinate or horizontal position. |
| `y` | Y coordinate or vertical position. |
| `width` | width in engine/world pixels or source pixels, depending on the API. |
| `height` | height in engine/world pixels or source pixels, depending on the API. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public void SetAnimatedTile(int tileIndex, int startFrame, int frameCount, float framesPerSecond, bool loop = true)`

| Parameter | Meaning |
|---|---|
| `tileIndex` | value supplied to the operation. |
| `startFrame` | first sprite-sheet frame used by the animation. |
| `frameCount` | number of frames used by the animation. |
| `framesPerSecond` | value supplied to the operation. |
| `loop` | whether playback loops. |

**Return:** No value is returned.

### `public void RemoveAnimatedTile(int tileIndex)`

| Parameter | Meaning |
|---|---|
| `tileIndex` | value supplied to the operation. |

**Return:** No value is returned.

### `public bool IsAnimatedTile(int tileIndex)`

| Parameter | Meaning |
|---|---|
| `tileIndex` | value supplied to the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public void SetConveyorTile(int tileIndex, float speed)`

| Parameter | Meaning |
|---|---|
| `tileIndex` | value supplied to the operation. |
| `speed` | value supplied to the operation. |

**Return:** No value is returned.

### `public void RemoveConveyorTile(int tileIndex)`

| Parameter | Meaning |
|---|---|
| `tileIndex` | value supplied to the operation. |

**Return:** No value is returned.

### `public bool IsConveyorTile(int tileIndex)`

| Parameter | Meaning |
|---|---|
| `tileIndex` | value supplied to the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public float GetConveyorSpeed(int tileIndex)`

| Parameter | Meaning |
|---|---|
| `tileIndex` | value supplied to the operation. |

**Return:** Returns `float` as declared by the API.

### `public int GetAnimatedTileIndex(int tileIndex, float elapsedSeconds)`

| Parameter | Meaning |
|---|---|
| `tileIndex` | value supplied to the operation. |
| `elapsedSeconds` | value supplied to the operation. |

**Return:** Returns `int` as declared by the API.

### `public void SetHazardTile(int tileIndex, bool hazard)`

| Parameter | Meaning |
|---|---|
| `tileIndex` | value supplied to the operation. |
| `hazard` | value supplied to the operation. |

**Return:** No value is returned.

### `public bool IsHazardTile(int tileIndex)`

| Parameter | Meaning |
|---|---|
| `tileIndex` | value supplied to the operation. |

**Return:** Returns `true` when the requested condition/operation succeeds; otherwise `false`.

### `public int StartFrame { get; } public int FrameCount { get; } public float FramesPerSecond { get; } public bool Loop { get; }  public JollyTileAnimation(int startFrame, int frameCount, float framesPerSecond, bool loop)`

| Parameter | Meaning |
|---|---|
| `startFrame` | first sprite-sheet frame used by the animation. |
| `frameCount` | number of frames used by the animation. |
| `framesPerSecond` | value supplied to the operation. |
| `loop` | whether playback loops. |

**Return:** Returns `int` as declared by the API.

## `World/JollyTileMapRenderer.cs`

### `public int Layer { get; set; } public float X { get; set; } public float Y { get; set; }  public JollyTileMapRenderer(JollyTileMap tileMap, Func<JollyTexture, JollySprite> spriteFactory)`

| Parameter | Meaning |
|---|---|
| `tileMap` | value supplied to the operation. |
| `spriteFactory` | value supplied to the operation. |

**Return:** Returns `int` as declared by the API.

### `public void Rebuild()`

**Return:** No value is returned.

### `public void Update(Camera2D camera, float deltaSeconds)`

| Parameter | Meaning |
|---|---|
| `camera` | camera used for coordinate conversion. |
| `deltaSeconds` | elapsed time in seconds for the current update. |

**Return:** No value is returned.

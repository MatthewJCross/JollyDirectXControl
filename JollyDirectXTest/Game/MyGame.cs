using Jolly2DEngine.Audio;
using Jolly2DEngine.Assets;
using Jolly2DEngine.Core;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Graphics.Scene;
using Jolly2DEngine.Input;
using Jolly2DEngine.Math;
using Jolly2DEngine.Particles;
using Jolly2DEngine.Physics;
using Jolly2DEngine.UI;
using Jolly2DEngine.World;
using Jolly2DEngine.Rendering;
using Jolly2DEngine.Rendering.DirectX;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Input;

namespace JollyDirectXTestCtrl
{
    public sealed class MyGame : JollyGame
    {
        private const JollyCollisionLayer PLAYER_LAYER = JollyCollisionLayer.Layer0;
        private const JollyCollisionLayer WORLD_LAYER = JollyCollisionLayer.Layer2;

        private readonly DirectXControl _directXControl;
        private readonly GameScene _scene = new();
        private readonly List<JollyGameObject> _physicsObjects = [];

        private DirectXJollyRenderer? _renderer;
        private JollyPhysicsDebugRenderer? _physicsDebugRenderer;
        private JollyAssetManager? _assets;
        private JollySpriteObject? _cheetah;
        private JollyTexture? _platformTexture;
        private bool _physicsDebugEnabled = true;

        private JollyAudioManager? _audio;
        
        private const float SpawnX = 400.0f;
        private const float SpawnY = 300.0f;

        private JollyParticleSystem? _particleSystem;
        private JollyParticleRenderer? _particleRenderer;
        private JollyTexture? _particleTexture;
        private JollyParticleEmitter? _particleEmitter;
        public int ParticleCount => _particleSystem?.ParticleCount ?? 0;

        private JollyUIManager? _uiManager;
        private JollyUIRenderer? _uiRenderer;
        
        public MyGame(DirectXControl directXControl)
        {
            ArgumentNullException.ThrowIfNull(directXControl);

            _directXControl = directXControl;
            GameLoopFPS = 1920.0;
            RenderFPS = 480.0;
        }

        public float CameraWorldX => _renderer?.Camera.X ?? 0.0f;
        public float CameraWorldY => _renderer?.Camera.Y ?? 0.0f;
        public float CameraZoom => _renderer?.Camera.Zoom ?? 1.0f;
        public float CameraRotationDegrees => _renderer != null ? _renderer.Camera.Rotation * (180.0f / MathF.PI) : 0.0f;
        public float PlayerX => _cheetah?.Position.X ?? 0.0f;
        public float PlayerY => _cheetah?.Position.Y ?? 0.0f;
        public float PlayerVelocityX => _cheetah?.Velocity.X ?? 0.0f;
        public float PlayerVelocityY => _cheetah?.Velocity.Y ?? 0.0f;
        public bool PlayerGrounded => _cheetah?.IsGrounded ?? false;
        public int PhysicsObjectCount => _physicsObjects.Count;
        public bool PhysicsDebugEnabled => _physicsDebugEnabled;

        private JollyTileMap _tileMap = null!;
        private JollyTileMapRenderer _tileMapRenderer = null!;
        
        private const float MoveAcceleration = 2200.0f;
        private const float GroundDeceleration = 2600.0f;
        private const float AirDeceleration = 800.0f;
        private const float MaxMoveSpeed = 300.0f;
        
        protected override void OnRenderFPSChanged(double renderFPS)
        {
            if (renderFPS > 0.0)
            {
                _directXControl.VSync = false;
                _directXControl.MaxFPS = renderFPS;
            }
            else
            {
                _directXControl.VSync = true;
                _directXControl.MaxFPS = 0.0;
            }
        }

        protected override void OnInitialize()
        {
            OnRenderFPSChanged(RenderFPS);

            JollyCollisionDebug.Enabled = true;
            JollyCollisionDebug.LogPixelChecks = false;
            JollyCollisionDebug.LogPixelHits = true;

            _renderer = new DirectXJollyRenderer(_directXControl);
            _renderer.Diagnostics = Diagnostics;
            _physicsDebugRenderer = new JollyPhysicsDebugRenderer(_renderer);
            _audio = new JollyAudioManager();
            _assets = new JollyAssetManager(_renderer, _audio);

            _renderer.Camera.SetBounds(0.0f, 0.0f, 2000.0f, 1200.0f);
            _renderer.Camera.Position = new Vector2(500.0f, 200.0f);
            _renderer.Camera.Zoom = 1.0f;
            _renderer.Camera.Rotation = 0.0f;

            JollyTexture cheetahTexture = _assets.LoadTexture("cheetah.png");

            _platformTexture = _renderer.CreateSolidTexture(100, 100, 100, 255);

            _particleTexture = _renderer.CreateCircleTexture(255, 255, 255, 255, 32);
            _particleSystem = new JollyParticleSystem();
            _particleRenderer = new JollyParticleRenderer(
                _particleSystem,
                texture => _renderer.CreateSprite(texture),
                color => _renderer.CreateCircleTexture((byte)(color.R * 255.0f), (byte)(color.G * 255.0f), (byte)(color.B * 255.0f), (byte)(color.A * 255.0f), 32));

            _particleEmitter = new JollyParticleEmitter
            {
                Enabled = false,
                Layer = 150,
                Shape = JollyParticleEmitterShape.Rectangle,
                ShapeWidth = 32.0f,
                ShapeHeight = 8.0f
            };

            _particleEmitter.ApplyPreset(JollyParticlePreset.Fire);
            _particleSystem.AddEmitter(_particleEmitter);

            JollyTexture tileTexture = _assets.LoadTexture("tileset.bmp");
            _tileMap = new JollyTileMap(20, 12, 16.0f, 16.0f);
            _tileMap.SetTexture(tileTexture); 
            
            CreatePlayer(cheetahTexture);
            CreatePlatforms();
            CreateTileMap(tileTexture);

            _tileMap.SetAnimatedTile(1, 1, 4, 8.0f);

            Scenes.ChangeScene(_scene);

            _assets.LoadSound("jump.wav");
            _assets.PlayMusic("unreleased.mp3", true);

            _uiManager = new JollyUIManager();

            if (_uiManager != null)
            {
                _uiManager.SetScreenSize((float)_directXControl.ActualWidth, (float)_directXControl.ActualHeight);
                _uiManager.UpdateLayout();
            }

            _uiRenderer = new JollyUIRenderer(_uiManager, texture => _renderer.CreateSprite(texture), (red, green, blue, alpha) => _renderer.CreateSolidTexture(red, green, blue, alpha), (text, fontFamily, fontSize, color, bold, italic, ultrabold) => _renderer.CreateTextTexture(text, fontFamily, fontSize, color, bold, italic));

            JollyUIPanel testPanel = new()
            {
                Position = new System.Numerics.Vector2(20.0f, 20.0f),
                Size = new System.Numerics.Vector2(300.0f, 100.0f),
                Color = JollyColor.FromBytes(40, 40, 40, 220),
                Layer = 20000
            };

            _uiManager.Add(testPanel);

            JollyUIText testText = new() 
            {
                Text = "JOLLY 2D UI",
                Position = new System.Numerics.Vector2(40.0f, 50.0f),
                FontFamily = "Arial",
                FontSize = 24.0f,
                Color = JollyColor.White,
                Bold = true,
                Layer = 20001
            };

            _uiManager.Add(testText);

            JollyTexture normalButtonTexture = _assets.LoadTexture("Enabled.png");
            JollyTexture hoverButtonTexture = _assets.LoadTexture("over.png");
            JollyTexture pressedButtonTexture = _assets.LoadTexture("down.png");
            JollyTexture disabledButtonTexture = _assets.LoadTexture("Disabled.png");
            JollyUIButton testButton = new()
            {
                Text = "PLAY GAME",
                Position = new System.Numerics.Vector2(20.0f, 140.0f),
                Size = new System.Numerics.Vector2(300.0f, 60.0f),
                NormalTexture = normalButtonTexture,
                HoverTexture = hoverButtonTexture,
                PressedTexture = pressedButtonTexture,
                DisabledTexture = disabledButtonTexture,
                TextColor = JollyColor.White,
                FontSize = 20.0f,
                Bold = true,
                Layer = 20000
            };

            testButton.Click += button =>
            {
                System.Diagnostics.Debug.WriteLine("PLAY GAME BUTTON CLICKED");
            };

            _uiManager.Add(testButton);

            JollyUIButton disabledButton = new()
            {
                Text = "DISABLED",
                Position = new System.Numerics.Vector2(340.0f, 140.0f),
                Size = new System.Numerics.Vector2(200.0f, 60.0f),
                Enabled = false,
                NormalColor = JollyColor.FromBytes(50, 50, 50),
                HoverColor = JollyColor.FromBytes(70, 70, 70),
                PressedColor = JollyColor.FromBytes(35, 35, 35),
                DisabledColor = JollyColor.FromBytes(25, 25, 25),
                TextColor = JollyColor.FromBytes(130, 130, 130),
                FontSize = 20.0f,
                Bold = true,
                Layer = 20000
            };

            _uiManager.Add(disabledButton);

            JollyTexture roundedTexture = _assets.LoadTexture("round.png");
            JollyUI9Slice roundedSlice = new(roundedTexture, 16.0f, 16.0f, 16.0f, 16.0f);
            JollyUIButton roundedButton = new()
            {
                Text = "ROUNDED BUTTON",
                Position = new System.Numerics.Vector2(20.0f, 300.0f),
                Size = new System.Numerics.Vector2(350.0f, 70.0f),
                NormalTexture = roundedTexture,
                NormalNineSlice = roundedSlice,
                TextColor = JollyColor.White,
                FontSize = 20.0f,
                Bold = true,
                Layer = 20000
            };

            _uiManager.Add(roundedButton);

            JollyUIProgressBar testProgressBar = new()
            {
                Position = new System.Numerics.Vector2(20.0f, 380.0f),
                Size = new System.Numerics.Vector2(400.0f, 30.0f),
                Minimum = 0.0f,
                Maximum = 100.0f,
                Value = 75.0f,
                BackgroundColor = JollyColor.FromBytes(40, 40, 40, 255),
                FillColor = JollyColor.FromBytes(60, 180, 80, 255),
                ShowText = true,
                TextColor = JollyColor.White,
                FontSize = 16.0f,
                Bold = true,
                Layer = 20000
            };
            _uiManager.Add(testProgressBar);

            JollyUIProgressBar verticalProgressBar = new()
            {
                Position = new System.Numerics.Vector2(450.0f, 250.0f),
                Size = new System.Numerics.Vector2(40.0f, 200.0f),
                Minimum = 0.0f,
                Maximum = 100.0f,
                Value = 65.0f,
                Orientation = JollyUIProgressBarOrientation.Vertical,
                BackgroundColor = JollyColor.FromBytes(40, 40, 40, 255),
                FillColor = JollyColor.Transparent,
                ShowText = true,
                Layer = 50000
            };
            _uiManager.Add(verticalProgressBar);

            JollyUISlider testSlider = new()
            {
                Position = new System.Numerics.Vector2(20.0f, 450.0f),
                Size = new System.Numerics.Vector2(400.0f, 40.0f),
                Minimum = 0.0f,
                Maximum = 100.0f,
                Value = 60.0f,
                TrackColor = JollyColor.FromBytes(40, 40, 40, 255),
                FillColor = JollyColor.FromBytes(60, 160, 240, 255),
                HandleColor = JollyColor.White,
                TrackThickness = 8.0f,
                HandleSize = 24.0f,
                Layer = 20000
            };

            testSlider.ValueChanged += (slider, value) =>
            {
                System.Diagnostics.Debug.WriteLine($"SLIDER VALUE: {value:0.00}");
            };

            _uiManager.Add(testSlider);

            JollyUIPanel demoPanel = new()
            {
                Position = new Vector2(650.0f, 20.0f),
                Size = new Vector2(520.0f, 500.0f),
                Color = JollyColor.FromBytes(30, 30, 30, 235),
                Layer = 19000
            };
            _uiManager.Add(demoPanel);

            JollyUIText demoTitle = new()
            {
                Text = "JOLLY 2D UI DEMO",
                Position = new Vector2(680.0f, 45.0f),
                FontFamily = "Arial",
                FontSize = 28.0f,
                Color = JollyColor.White,
                Bold = true,
                Layer = 21000,
                Opacity = 0.0f
            };
            _uiManager.Add(demoTitle);

            JollyUIText demoDescription = new()
            {
                Text = "Interactive UI regression test",
                Position = new Vector2(680.0f, 85.0f),
                FontFamily = "Arial",
                FontSize = 16.0f,
                Color = JollyColor.FromBytes(180, 180, 180, 255),
                Layer = 21000
            };
            _uiManager.Add(demoDescription);

            JollyUIText demoStatus = new()
            {
                Text = "STATUS: READY",
                Position = new Vector2(680.0f, 135.0f),
                FontFamily = "Arial",
                FontSize = 20.0f,
                Color = JollyColor.FromBytes(100, 220, 120, 255),
                Bold = true,
                Layer = 21000
            };
            _uiManager.Add(demoStatus);

            JollyUIButton demoButton = new()
            {
                Text = "TEST BUTTON",
                Position = new Vector2(680.0f, 180.0f),
                Size = new Vector2(220.0f, 55.0f),
                NormalColor = JollyColor.FromBytes(55, 55, 55, 255),
                HoverColor = JollyColor.FromBytes(75, 75, 75, 255),
                PressedColor = JollyColor.FromBytes(35, 35, 35, 255),
                TextColor = JollyColor.White,
                FontSize = 18.0f,
                Bold = true,
                Layer = 21000
            };
            demoButton.Click += button =>
            {
                demoStatus.Text = "STATUS: BUTTON CLICKED";
                _uiManager?.AnimateOpacity(demoStatus, 0.35f, 0.15f);
            };
            _uiManager.Add(demoButton);

            JollyUIText demoControls = new()
            {
                Text = "Controls: Button / Slider / Progress / Text / 9-Slice",
                Position = new Vector2(680.0f, 270.0f),
                FontFamily = "Arial",
                FontSize = 16.0f,
                Color = JollyColor.FromBytes(200, 200, 200, 255),
                Layer = 21000
            };
            _uiManager.Add(demoControls);

            JollyUIText demoAudio = new()
            {
                Text = "Audio: NAudio playback active",
                Position = new Vector2(680.0f, 310.0f),
                FontFamily = "Arial",
                FontSize = 16.0f,
                Color = JollyColor.FromBytes(200, 200, 200, 255),
                Layer = 21000
            };
            _uiManager.Add(demoAudio);

            JollyUIText demoParticles = new()
            {
                Text = "Particles: Colour tinting active",
                Position = new Vector2(680.0f, 345.0f),
                FontFamily = "Arial",
                FontSize = 16.0f,
                Color = JollyColor.FromBytes(200, 200, 200, 255),
                Layer = 21000
            };
            _uiManager.Add(demoParticles);

            JollyUIText demoLayout = new()
            {
                Text = "UI: Anchors / margins / layout / animation",
                Position = new Vector2(680.0f, 380.0f),
                FontFamily = "Arial",
                FontSize = 16.0f,
                Color = JollyColor.FromBytes(200, 200, 200, 255),
                Layer = 21000
            };
            _uiManager.Add(demoLayout);

            testSlider.ValueChanged += (slider, value) =>
            {
                testProgressBar.Value = value;
                demoStatus.Text = $"STATUS: SLIDER {value:0}";
            };

            _uiManager.AnimateOpacity(demoTitle, 1.0f, 0.75f);
            _uiManager.UpdateLayout();
            
            _directXControl.MouseMoved += OnUIMouseMoved;
            _directXControl.MouseButtonDown += OnUIMouseButtonDown;
            _directXControl.MouseButtonUp += OnUIMouseButtonUp;
        }

        private void CreatePlayer(JollyTexture cheetahTexture)
        {
            ArgumentNullException.ThrowIfNull(_renderer);

            JollySpriteSheet sheet = new(cheetahTexture, 512, 249, JollySpriteSheetOrder.Horizontal);
            JollySpriteSheetFrame jollySpriteSheetFrame = sheet.GetFrame(4);
            JollySpriteSheetFrame frame = jollySpriteSheetFrame;

            JollySprite sprite = _renderer.CreateSprite(cheetahTexture);
            sprite.Name = "CheetahSprite";
            sprite.Position = new JollyVector2(SpawnX, SpawnY);
            sprite.Size = new JollyVector2(512.0f, 249.0f);
            sprite.Rotation = 0.0f;
            sprite.Opacity = 1.0f;
            sprite.Visible = true;
            sprite.Layer = 100;
            sprite.FlipHorizontal = false;
            sprite.FlipVertical = false;
            sprite.SetAnimation(sheet, 8.0f, true);
            sprite.Play();

            JollySpriteObject jollySpriteObject = new(sprite);
            _cheetah = jollySpriteObject;
            _cheetah.Name = "Cheetah";
            _cheetah.Position = new JollyVector2(SpawnX, SpawnY);
            _cheetah.Rotation = 0.0f;
            _cheetah.SetRectangleCollider(80.0f, 110.0f);
            _cheetah.SetCollisionLayers(PLAYER_LAYER, WORLD_LAYER);
            _cheetah.UsePhysics = true;
            _cheetah.UseGravity = true;
            _cheetah.Gravity = new JollyVector2(0.0f, 980.0f);
            _cheetah.Velocity = new JollyVector2(0.0f, 0.0f);
            _cheetah.GroundFriction = 12.0f;
            _cheetah.AirFriction = 0.0f;
            _cheetah.Restitution = 0.0f;

            _scene.Add(_cheetah);
            _physicsObjects.Add(_cheetah);

            JollyCharacterController controller = _cheetah.AddCharacterController();

            controller.MoveAcceleration = 2200.0f;
            controller.MaxMoveSpeed = 300.0f;
            controller.GroundDeceleration = 2600.0f;
            controller.AirDeceleration = 800.0f;
            controller.JumpVelocity = 450.0f;
            controller.JumpGravity = 980.0f;
            controller.FallGravity = 1400.0f;
            controller.JumpCutGravity = 1800.0f;
            controller.MaxFallSpeed = 900.0f;
            controller.CoyoteTime = 0.10f;
            controller.JumpBufferTime = 0.10f;
        }

        private void CreatePlatforms()
        {
            CreatePlatform("Ground", 800.0f, 700.0f, 1600.0f, 80.0f);
            CreatePlatform("LeftWall", 40.0f, 400.0f, 80.0f, 680.0f);
            CreatePlatform("RightWall", 1560.0f, 400.0f, 80.0f, 680.0f);
            CreatePlatform("Platform1", 350.0f, 560.0f, 350.0f, 40.0f);
            CreatePlatform("Platform2", 850.0f, 480.0f, 350.0f, 40.0f);
            CreatePlatform("Platform3", 1350.0f, 560.0f, 350.0f, 40.0f);
            CreatePlatform("Platform4", 550.0f, 350.0f, 220.0f, 40.0f);
            CreatePlatform("Platform5", 1100.0f, 300.0f, 220.0f, 40.0f);
        }

        private void CreatePlatform(string name, float x, float y, float width, float height)
        {
            ArgumentNullException.ThrowIfNull(_renderer);
            ArgumentNullException.ThrowIfNull(_platformTexture);

            JollySprite sprite = _renderer.CreateSprite(_platformTexture);
            sprite.Name = $"{name}Sprite";
            sprite.Position = new JollyVector2(x, y);
            sprite.Size = new JollyVector2(width, height);
            sprite.Rotation = 0.0f;
            sprite.Opacity = 1.0f;
            sprite.Visible = true;
            sprite.Layer = 10;

            JollySpriteObject platform = new(sprite)
            {
                Name = name,
                Position = new JollyVector2(x, y),
                Rotation = 0.0f
            };
            platform.SetRectangleCollider(width, height);
            platform.SetCollisionLayers(WORLD_LAYER, PLAYER_LAYER);
            platform.UsePhysics = false;
            platform.UseGravity = false;

            _scene.Add(platform);
            _physicsObjects.Add(platform);
        }

        private void CreateTileMap(JollyTexture tileTexture)
        {
            ArgumentNullException.ThrowIfNull(_renderer);
            ArgumentNullException.ThrowIfNull(_tileMap);

            _tileMapRenderer = new JollyTileMapRenderer(_tileMap, texture => _renderer.CreateSprite(texture))
            {
                X = 200.0f,
                Y = 150.0f,
                Layer = -10
            };
            _scene.SetTileMap(_tileMap, _tileMapRenderer.X, _tileMapRenderer.Y);

            for (int x = 0; x < _tileMap.Width; x++)
            {
                _tileMap.SetTile(x, 10, 0);
                _tileMap.SetSolid(x, 10, true);
            }

            for (int x = 5; x < 10; x++)
            {
                _tileMap.SetTile(x, 7, 1);
                _tileMap.SetSolid(x, 7, true);
            }

            for (int x = 12; x < 17; x++)
            {
                _tileMap.SetTile(x, 5, 2);
                _tileMap.SetSolid(x, 5, true);
            }

            _tileMap.SetHazardTile(10, true);
            _tileMap.HazardContact += OnTileHazardContact;
        }

        protected override void OnUpdate(JollyGameTime gameTime)
        {
            float moveInput = 0.0f;

            if (Input.IsKeyDown(Key.A) || Input.IsKeyDown(Key.Left))
            {
                moveInput -= 1.0f;
            }

            if (Input.IsKeyDown(Key.D) || Input.IsKeyDown(Key.Right))
            {
                moveInput += 1.0f;
            }

            if (Input.Gamepads[0].IsConnected)
            {
                moveInput = Input.Gamepads[0].LeftStickX;

                if (Input.Gamepads[0].IsButtonDown(JollyGamepadButton.DPadLeft))
                {
                    moveInput = -1.0f;
                }

                if (Input.Gamepads[0].IsButtonDown(JollyGamepadButton.DPadRight))
                {
                    moveInput = 1.0f;
                }
            }

            if (Input.IsKeyDown(Key.Up))
            {
                _renderer?.Camera.Shake(20.0f, 0.5f);
            }

            if (Input.IsKeyDown(Key.Down))
            {
                _renderer?.Camera.Shake(20.0f, 0.5f);
            }

            _cheetah!.CharacterController!.Move(moveInput);
            _cheetah!.CharacterController!.SetJumpHeld(Input.IsKeyDown(Key.Space));

            if (Input.IsKeyPressed(Key.Space))
            {
                _cheetah!.CharacterController!.RequestJump();
                _audio?.PlaySound("jump");
            }

            if (Input.IsKeyPressed(Key.P))
            {
                _audio?.PauseMusic();
            }

            if (Input.IsKeyPressed(Key.R))
            {
                _audio?.ResumeMusic();
            }

            if (Input.IsKeyPressed(Key.S))
            {
                _audio?.StopMusic();
            }

            if (Input.IsKeyPressed(Key.R))
            {
                ResetPlayer();
            }

            if (Input.IsKeyPressed(Key.Right))
            {
                _renderer?.Camera.Shake(20.0f, 0.5f);
            }

            if (Input.IsKeyPressed(Key.Left))
            {
                _renderer?.Camera.Shake(20.0f, 0.5f);
            }

            if (Input.IsKeyPressed(Key.F3))
            {
                _physicsDebugEnabled = !_physicsDebugEnabled;
            }

            if (Input.IsKeyPressed(Key.F5))
            {
                _renderer?.Camera.Shake(20.0f, 0.5f);
            }

            if (Input.IsKeyPressed(Key.F6))
            {
                Debug.WriteLine("F6 PRESSED");

                if (_particleSystem == null)
                {
                    Debug.WriteLine("F6: ParticleSystem NULL");
                }

                if (_particleEmitter == null)
                {
                    Debug.WriteLine("F6: ParticleEmitter NULL");
                }

                if (_cheetah == null)
                {
                    Debug.WriteLine("F6: Cheetah NULL");
                }

                if (_particleSystem != null && _particleEmitter != null && _cheetah != null)
                {
                    _particleEmitter.Position = new Vector2(_cheetah.Position.X, _cheetah.Position.Y);
                    Debug.WriteLine($"F6: Before Emit = {_particleSystem.ParticleCount}");
                    _particleSystem.Emit(_particleEmitter, 40);
                    Debug.WriteLine($"F6: After Emit = {_particleSystem.ParticleCount}");
                }
            }

            if (Input.IsKeyPressed(Key.F7))
            {
                if (_particleEmitter != null)
                {
                    _particleEmitter.Enabled = !_particleEmitter.Enabled;
                    _particleEmitter.Reset();
                    _particleEmitter.ParticlesPerSecond = 20.0f;
                }
            }

            if (Input.IsMouseButtonPressed(JollyMouseButton.Left))
            {
                Vector2 mouseWorld = Input.GetMouseWorldPosition(camera: _renderer.Camera);
                Debug.WriteLine($"Clicked world position: {mouseWorld.X}, {mouseWorld.Y}");
            }

            if (_particleEmitter != null && _cheetah != null)
            {
                _particleEmitter.Position = new Vector2(_cheetah.Position.X, _cheetah.Position.Y);
            }

            _particleSystem?.Update((float)gameTime.DeltaSeconds);

            _uiManager?.Update((float)gameTime.DeltaSeconds);
            _uiRenderer?.Update();
            
            if (_renderer != null && _physicsDebugRenderer != null)
            {
                _renderer.Camera.UpdateShake((float)gameTime.DeltaSeconds);
                _tileMapRenderer.Update(_renderer.Camera, (float)gameTime.DeltaSeconds);

                _physicsDebugRenderer.Enabled = _physicsDebugEnabled;
                _physicsDebugRenderer.TileMapEnabled = _physicsDebugEnabled;
                _physicsDebugRenderer.Update(_scene.Objects, _tileMap, _tileMapRenderer.X, _tileMapRenderer.Y);

                IReadOnlyList<JollySprite> sceneSprites = _scene.GetSprites();

                List<JollySprite> renderSprites =
                [
                    .. sceneSprites,
                    .. _physicsDebugRenderer.Sprites,
                    .. _tileMapRenderer.Sprites,
                ];

                if (_particleRenderer != null && _particleTexture != null)
                {
                    _particleRenderer.Update(_particleTexture);
                    renderSprites.AddRange(_particleRenderer.Sprites);
                }

                if (_uiRenderer != null)
                {
                    renderSprites.AddRange(_uiRenderer.Sprites);
                }

                renderSprites.Sort((left, right) => left.Layer.CompareTo(right.Layer));

                _renderer.SubmitFrame(new JollyRenderFrame(renderSprites));
            }
        }

        private void ResetPlayer()
        {
            if (_cheetah == null)
            {
                return;
            }

            _cheetah.Position = new JollyVector2(SpawnX, SpawnY);
            _cheetah.Velocity = new JollyVector2(0.0f, 0.0f);
            _cheetah.IsGrounded = false;
        }

        protected override void OnShutdown()
        {
            _scene.Shutdown();
            _physicsObjects.Clear();

            _audio?.Dispose();
            _audio = null;

            _particleRenderer?.Clear();
            _particleSystem?.Clear();

            _particleEmitter = null;
            _particleRenderer = null;
            _particleSystem = null;
            _particleTexture = null;

            _cheetah = null;
            _physicsDebugRenderer = null;
            _platformTexture = null;
            _assets?.Dispose();
            _assets = null;
            _renderer = null;
        }

        public void SetCameraPosition(float x, float y, float zoom, float rotationDegrees)
        {
            if (_renderer == null)
            {
                return;
            }

            if (zoom <= 0.01f)
            {
                zoom = 0.01f;
            }

            _renderer.Camera.Position = new Vector2(x, y);
            _renderer.Camera.Zoom = zoom;
            _renderer.Camera.Rotation = rotationDegrees * (MathF.PI / 180.0f);
        }

        public Vector2 ScreenToWorld(float x, float y)
        {
            if (_renderer == null)
            {
                return Vector2.Zero;
            }

            return _renderer.Camera.ScreenToWorld(new Vector2(x, y));
        }

        private void OnTileHazardContact(JollyGameObject gameObject, int tileX, int tileY, int tileIndex)
        {
            if (!ReferenceEquals(gameObject, _cheetah))
            {
                return;
            }

            gameObject.Position = new JollyVector2(SpawnX, SpawnY);
            gameObject.Velocity = new JollyVector2(0.0f, 0.0f);
            gameObject.Acceleration = new JollyVector2(0.0f, 0.0f);
            gameObject.IsGrounded = false;
            gameObject.WasGrounded = false;
            gameObject.CharacterController?.Reset();
        }

        private sealed class GameScene : JollyScene
        {
        }

        private void OnUIMouseMoved(double x, double y)
        {
            _uiManager?.MouseMove((float)x, (float)y);
        }

        private void OnUIMouseButtonDown()
        {
            _uiManager?.MouseDown();
        }

        private void OnUIMouseButtonUp()
        {
            _uiManager?.MouseUp();
        }
    }
}
using Jolly2DEngine.Assets;
using Jolly2DEngine.Audio;
using Jolly2DEngine.Core;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;
using Jolly2DEngine.UI;
using System.Numerics;
using System.Windows.Input;

namespace Jolly2DBeamRider.Game
{
    public sealed class BeamRiderGame : JollyGame
    {
        private enum BeamRiderGameState
        {
            Title,
            Intro,
            Playing,
            GameOver
        }

        private BeamRiderGameState _gameState = BeamRiderGameState.Title;

        private const float LogicalWidth = 1040.0f;
        private const float LogicalHeight = 780.0f;

        private const int MaximumMissiles = 3;
        private const double ScoreFlashDuration = 0.45;

        private readonly DirectXControl _gameView;

        private DirectXJollyRenderer? _renderer;
        private JollyAudioManager? _audio;
        private JollyAssetManager? _assets;

        private JollyUIManager? _uiManager;
        private JollyUIRenderer? _uiRenderer;

        private JollyUIText? _titleText;
        private JollyUIText? _messageText;
        private JollyUIText? _enemiesText;
        private JollyUIText? _scoreText;

        private BeamRiderArena? _arena;
        private BeamRiderPlayer? _player;
        private BeamRiderSector? _sector;

        private readonly List<BeamRiderProjectile> _projectiles = [];
        private readonly List<BeamRiderEnemy> _enemies = [];

        private JollySoundPlayback? _introSound;

        private bool _spaceWasDown;
        private double _gameOverFlashTime;
        private bool _gameOverFlashVisible;

        private int _score;
        private int _scoreDisplayValue;
        private double _scoreFlashTime;
        private bool _scoreShowingAward;

        public BeamRiderGame(DirectXControl gameView)
        {
            ArgumentNullException.ThrowIfNull(gameView);
            _gameView = gameView;
            GameLoopFPS = 240.0;
            RenderFPS = 120.0;
        }

        public void StartGame()
        {
            StartNewGame();
        }

        public void RestartGame()
        {
            StartNewGame();
        }

        public void ShowRestartPage()
        {
            _introSound = null;
            _gameState = BeamRiderGameState.GameOver;
            _gameOverFlashTime = 0.0;
            _gameOverFlashVisible = true;
            _titleText?.Text = "GAME OVER";
            _titleText?.Color = JollyColor.Red;
            _titleText?.FontSize = 72.0f;
            _messageText?.Text = "PRESS SPACE TO RESTART";
            _messageText?.Color = JollyColor.White;
            _messageText?.FontSize = 36.0f;
            UpdateGameStateUI();
            UpdateUIPositions();
            UpdateHUD();
        }

        protected override void OnInitialize()
        {
            _renderer = new DirectXJollyRenderer(_gameView)
            {
                LineGlowEnabled = true,
                LineGlowWidth = 5.0f,
                LineGlowStrength = 0.8f,
                LineGlowFalloff = 2.0f,
                ClearColor = System.Windows.Media.Color.FromRgb(8, 12, 50)
            };
            _gameView.RenderSizeChanged += OnRenderSizeChanged;
            _audio = new JollyAudioManager();
            _assets = new JollyAssetManager(_renderer, _audio);
            ConfigureCamera();
            CreateUI();
            UpdateGameStateUI();
            UpdateHUD();
            RenderFrame();
            LoadSounds();
        }

        private void LoadSounds()
        {
            _assets.LoadSound("IntroLevel.wav");
            _assets.LoadSound("RunLoop.wav");
            _assets.LoadSound("Fire.wav");
        }

        private void CreateUI()
        {
            if (_renderer is null || _uiManager is not null)
            {
                return;
            }

            _uiManager = new JollyUIManager();
            _uiRenderer = new JollyUIRenderer(_uiManager, texture => _renderer.CreateSprite(texture), (r, g, b, a) => _renderer.CreateSolidTexture(r, g, b, a), (text, fontFamily, fontSize, color, bold, italic) => _renderer.CreateTextTexture(text, fontFamily, fontSize, color, bold, italic));
            _titleText = new JollyUIText
            {
                Text = "BEAM RIDER",
                Position = new Vector2(LogicalWidth * 0.5f - 230.0f, LogicalHeight * 0.5f - 120.0f),
                FontFamily = "Arial",
                FontSize = 72.0f,
                Color = JollyColor.Cyan,
                Bold = true,
                Layer = 300
            };
            _messageText = new JollyUIText
            {
                Text = "PRESS SPACE TO START",
                Position = new Vector2(LogicalWidth * 0.5f - 220.0f, LogicalHeight * 0.5f + 10.0f),
                FontFamily = "Arial",
                FontSize = 36.0f,
                Color = JollyColor.White,
                Bold = true,
                Layer = 300
            };
            _enemiesText = new JollyUIText
            {
                Text = "0",
                Position = new Vector2(230.0f, 8.0f),
                FontFamily = "Arial",
                FontSize = 48.0f,
                Color = JollyColor.Green,
                Bold = true,
                Layer = 300
            };
            _scoreText = new JollyUIText
            {
                Text = "0",
                Position = new Vector2(740.0f, 8.0f),
                FontFamily = "Arial",
                FontSize = 48.0f,
                Color = JollyColor.Yellow,
                Bold = true,
                Layer = 300
            };
            _uiManager.Add(_titleText);
            _uiManager.Add(_messageText);
            _uiManager.Add(_enemiesText);
            _uiManager.Add(_scoreText);
        }

        private void StartNewGame()
        {
            if (_renderer is null || _assets is null)
            {
                return;
            }

            _introSound = null;
            DisposeEnemies();
            _projectiles.Clear();
            if (_sector is not null)
            {
                _sector.AllSaucersDestroyed -= OnAllSaucersDestroyed;
                _sector.Dispose();
                _sector = null;
            }

            _player?.Dispose();
            _player = null;
            _arena?.Dispose();
            _arena = null;
            _arena = new BeamRiderArena(LogicalWidth, LogicalHeight, _renderer, _assets);
            _score = 0;
            _scoreDisplayValue = 0;
            _scoreFlashTime = 0.0;
            _scoreShowingAward = false;
            _gameState = BeamRiderGameState.Intro;
            _spaceWasDown = Input.IsKeyDown(Key.Space);
            _gameOverFlashTime = 0.0;
            _gameOverFlashVisible = true;
            _introSound = _audio?.PlaySound("IntroLevel");
            UpdateGameStateUI();
            UpdateHUD();
            RenderFrame();
        }

        private void StartGameplay()
        {
            if (_arena is null)
            {
                return;
            }

            _player = new BeamRiderPlayer(_arena);
            _player.Create();
            _sector = new BeamRiderSector(_arena, 1);
            _sector.AllSaucersDestroyed += OnAllSaucersDestroyed;
            _gameState = BeamRiderGameState.Playing;
            _introSound = null;
            UpdateGameStateUI();
            UpdateHUD();
            _assets.PlayMusic("RunLoop", true);
            RenderFrame();
        }

        protected override void OnUpdate(JollyGameTime gameTime)
        {
            bool spacePressed = IsSpacePressed();
            if (_gameState == BeamRiderGameState.Title)
            {
                if (spacePressed)
                {
                    StartGame();
                    return;
                }

                UpdateUI();
                RenderFrame();
                return;
            }

            if (_gameState == BeamRiderGameState.Intro)
            {
                UpdateIntro(gameTime);
                UpdateUI();
                RenderFrame();
                return;
            }

            if (_gameState == BeamRiderGameState.GameOver)
            {
                _gameOverFlashTime += gameTime.DeltaSeconds;
                if (_gameOverFlashTime >= 0.4)
                {
                    _gameOverFlashTime = 0.0;
                    _gameOverFlashVisible = !_gameOverFlashVisible;
                }

                if (_titleText is not null)
                {
                    _titleText.Color = _gameOverFlashVisible ? JollyColor.Red : JollyColor.Transparent;
                }

                if (spacePressed)
                {
                    RestartGame();
                    return;
                }

                UpdateUI();
                RenderFrame();
                return;
            }

            if (_arena is null || _player is null || _sector is null)
            {
                return;
            }

            float deltaTime = (float)gameTime.DeltaSeconds;
            if (deltaTime <= 0.0f)
            {
                return;
            }

            _arena.Update(deltaTime);
            _player.Update(Input, deltaTime);
            if (_player.ShouldFire())
            {
                BeamRiderProjectile? projectile = _player.CreateProjectile();
                if (projectile is not null)
                {
                    _projectiles.Add(projectile);
                    _audio?.PlaySound("Fire");
                }
            }

            UpdateEnemies(deltaTime);
            UpdateProjectiles(deltaTime);
            UpdateSector(deltaTime);
            UpdateScoreDisplay(deltaTime);
            UpdateHUD();
            UpdateUI();
            RenderFrame();
        }

        private void UpdateIntro(JollyGameTime gameTime)
        {
            if (_arena is null)
            {
                return;
            }

            float deltaTime = (float)gameTime.DeltaSeconds;
            if (deltaTime <= 0.0f)
            {
                return;
            }

            _arena.Update(deltaTime);
            if (_introSound is null)
            {
                return;
            }

            if (!_introSound.IsFinished)
            {
                return;
            }

            _introSound = null;
            StartGameplay();
        }

        private void UpdateEnemies(float deltaTime)
        {
            for (int enemyIndex = _enemies.Count - 1;
                 enemyIndex >= 0;
                 enemyIndex--)
            {
                BeamRiderEnemy enemy = _enemies[enemyIndex];
                if (!enemy.Alive)
                {
                    continue;
                }

                enemy.Update(deltaTime);
            }
        }

        private void UpdateProjectiles(float deltaTime)
        {
            for (int projectileIndex = _projectiles.Count - 1;
                 projectileIndex >= 0;
                 projectileIndex--)
            {
                BeamRiderProjectile projectile = _projectiles[projectileIndex];
                projectile.Update(deltaTime);
                if (!projectile.Active)
                {
                    _projectiles.RemoveAt(projectileIndex);
                    _player?.RestoreHalo();
                    continue;
                }

                for (int enemyIndex = _enemies.Count - 1;
                     enemyIndex >= 0;
                     enemyIndex--)
                {
                    BeamRiderEnemy enemy = _enemies[enemyIndex];
                    if (!enemy.Alive)
                    {
                        continue;
                    }

                    bool wasAlive = enemy.Alive;
                    bool hit = projectile.TryHitEnemy(enemy);
                    if (hit)
                    {
                        if (wasAlive && !enemy.Alive)
                        {
                            AddScoreForEnemy(enemy);
                        }

                        break;
                    }
                }

                if (!projectile.Active)
                {
                    _projectiles.RemoveAt(projectileIndex);
                    _player?.RestoreHalo();
                }
            }
        }

        private void AddScoreForEnemy(BeamRiderEnemy enemy)
        {
            int scoreValue = enemy.Type switch 
            { 
                BeamRiderEnemyType.Saucer => 44, 
                BeamRiderEnemyType.Debris => 50, 
                BeamRiderEnemyType.Chirper => 150, 
                BeamRiderEnemyType.Blocker => 200, 
                BeamRiderEnemyType.BounceCraft => 250, 
                BeamRiderEnemyType.Charger => 300, 
                BeamRiderEnemyType.Tracker => 350, 
                BeamRiderEnemyType.ZigBomb => 400, 
                BeamRiderEnemyType.Rejuvenator => 500, 
                BeamRiderEnemyType.Sentinel => 1000, 
                _ => 100 
            };

            _score += scoreValue;
            _scoreDisplayValue = scoreValue;
            _scoreFlashTime = ScoreFlashDuration;
            _scoreShowingAward = true;
            if (_scoreText is not null)
            {
                _scoreText.Text = $"{scoreValue}";
                _scoreText.Color = JollyColor.Red;
            }
        }

        private void UpdateScoreDisplay(double deltaTime)
        {
            if (_scoreText is null)
            {
                return;
            }

            if (_scoreShowingAward)
            {
                _scoreFlashTime -= deltaTime;
                if (_scoreFlashTime > 0.0)
                {
                    _scoreText.Text = $"{_scoreDisplayValue}";
                    _scoreText.Color = JollyColor.Red;
                    return;
                }

                _scoreFlashTime = 0.0;
                _scoreShowingAward = false;
            }

            _scoreText.Text = _score.ToString();
            _scoreText.Color = JollyColor.Yellow;
        }

        private void UpdateSector(float deltaTime)
        {
            if (_sector is null)
            {
                return;
            }

            _sector.Update(deltaTime, _enemies);
        }

        private void OnAllSaucersDestroyed()
        {
            if (_sector is null)
            {
                return;
            }

            if (!_sector.SentinelReady)
            {
                return;
            }

            /*
             * The original Beamrider now enters the
             * Sector Sentinel phase.
             *
             * We deliberately stop here until
             * BeamRiderSentinel and BeamRiderTorpedo
             * are added.
             */
        }

        private void DisposeEnemies()
        {
            for (int index = _enemies.Count - 1;
                 index >= 0;
                 index--)
            {
                _enemies[index].Dispose();
            }

            _enemies.Clear();
        }

        private bool IsSpacePressed()
        {
            bool spaceDown = Input.IsKeyDown(Key.Space);
            bool pressed = spaceDown && !_spaceWasDown;
            _spaceWasDown = spaceDown;
            return pressed;
        }

        private void UpdateGameStateUI()
        {
            if (_titleText is null || _messageText is null)
            {
                return;
            }

            switch (_gameState)
            {
                case BeamRiderGameState.Title:
                    _titleText.Text = "BEAM RIDER";
                    _titleText.Color = JollyColor.Cyan;
                    _titleText.FontSize = 72.0f;

                    _messageText.Text = "PRESS SPACE TO START";
                    _messageText.Color = JollyColor.White;
                    _messageText.FontSize = 36.0f;

                    break;

                case BeamRiderGameState.Intro:
                    _titleText.Text = "BEAM RIDER";
                    _titleText.Color = JollyColor.Cyan;
                    _titleText.FontSize = 72.0f;

                    _messageText.Text = string.Empty;

                    break;

                case BeamRiderGameState.Playing:
                    _titleText.Text = string.Empty;
                    _messageText.Text = string.Empty;

                    break;

                case BeamRiderGameState.GameOver:
                    _titleText.Text = "GAME OVER";
                    _titleText.Color = JollyColor.Red;
                    _titleText.FontSize = 72.0f;

                    _messageText.Text = "PRESS SPACE TO RESTART";
                    _messageText.Color = JollyColor.White;
                    _messageText.FontSize = 36.0f;

                    break;
            }

            UpdateHUDVisibility();
            UpdateUIPositions();
        }

        private void UpdateHUD()
        {
            if (_enemiesText is null || _scoreText is null)
            {
                return;
            }

            int enemiesRemaining = 0;
            foreach (BeamRiderEnemy enemy in _enemies)
            {
                if (enemy.Alive)
                {
                    enemiesRemaining++;
                }
            }

            _enemiesText.Text = enemiesRemaining.ToString();
            if (!_scoreShowingAward)
            {
                _scoreText.Text = _score.ToString();
            }

            UpdateHUDVisibility();
        }

        private void UpdateHUDVisibility()
        {
            if (_enemiesText is null || _scoreText is null)
            {
                return;
            }

            bool visible = _gameState == BeamRiderGameState.Playing;
            _enemiesText.Color = visible ? JollyColor.Green : JollyColor.Transparent;
            if (!visible)
            {
                _scoreText.Color = JollyColor.Transparent;
                return;
            }

            if (_scoreShowingAward)
            {
                _scoreText.Color = JollyColor.Red;
            }
            else
            {
                _scoreText.Color = JollyColor.Yellow;
            }
        }

        private int GetAvailableMissileCount()
        {
            int available = MaximumMissiles - _projectiles.Count;
            return Math.Clamp(available, 0, MaximumMissiles);
        }

        private void AddMissileHudLines(List<JollyLineSnapshot> lines)
        {
            if (_gameState != BeamRiderGameState.Playing)
            {
                return;
            }

            int missileCount = GetAvailableMissileCount();
            const float centreX = LogicalWidth * 0.5f;
            const float topY = 30.0f;
            const float spacing = 24.0f;
            float startX = centreX - (((missileCount - 1) * spacing) * 0.5f);
            for (int index = 0; index < missileCount; index++)
            {
                float x = startX + (index * spacing);
                AddMissileGlyph(lines, new Vector2(x, topY));
            }
        }

        private static void AddMissileGlyph(List<JollyLineSnapshot> lines, Vector2 position)
        {
            const float width = 7.0f;
            const float height = 18.0f;
            const float thickness = 2.0f;
            JollyColor color = JollyColor.Cyan;
            Vector2 nose = new Vector2(position.X, position.Y - height * 0.5f);
            Vector2 tail = new Vector2(position.X, position.Y + height * 0.5f);
            Vector2 leftFin = new Vector2(position.X - width * 0.5f, position.Y + height * 0.25f);
            Vector2 rightFin = new Vector2(position.X + width * 0.5f, position.Y + height * 0.25f);
            Vector2 leftBody = new Vector2(position.X - width * 0.5f, position.Y - height * 0.15f);
            Vector2 rightBody = new Vector2(position.X + width * 0.5f, position.Y - height * 0.15f);
            lines.Add(new JollyLineSnapshot(nose, leftBody, color, thickness));
            lines.Add(new JollyLineSnapshot(nose, rightBody, color, thickness));
            lines.Add(new JollyLineSnapshot(leftBody, leftFin, color, thickness));
            lines.Add(new JollyLineSnapshot(rightBody, rightFin, color, thickness));
            lines.Add(new JollyLineSnapshot(leftFin, tail, color, thickness));
            lines.Add(new JollyLineSnapshot(rightFin, tail, color, thickness));
            lines.Add(new JollyLineSnapshot(leftBody, rightBody, color, thickness));
        }

        private void UpdateUIPositions()
        {
            if (_renderer is null || _titleText is null || _messageText is null || _enemiesText is null || _scoreText is null)
            {
                return;
            }

            float centreX = LogicalWidth * 0.5f;
            float centreY = LogicalHeight * 0.5f;
            _enemiesText.Position = new Vector2(230.0f, 8.0f);
            _scoreText.Position = new Vector2(740.0f, 8.0f);
            if (_gameState == BeamRiderGameState.Title || _gameState == BeamRiderGameState.Intro)
            {
                _titleText.Position = new Vector2(centreX - 230.0f, centreY - 120.0f);
                _messageText.Position = new Vector2(centreX - 220.0f, centreY + 10.0f);
            }
            else if (_gameState == BeamRiderGameState.GameOver)
            {
                _titleText.Position = new Vector2(centreX - 195.0f, centreY - 120.0f);
                _messageText.Position = new Vector2(centreX - 325.0f, centreY + 10.0f);
            }
        }

        private void UpdateUI()
        {
            _uiRenderer?.Update();
        }

        private void RenderFrame()
        {
            if (_renderer is null)
            {
                return;
            }

            List<JollySprite> sprites = [];
            if (_arena is not null)
            {
                sprites.AddRange(_arena.Sprites);
            }

            List<JollyLineSnapshot> lines = [];
            if (_arena is not null)
            {
                lines.AddRange(_arena.Lines);
            }

            if (_player is not null)
            {
                lines.AddRange(_player.Lines);
            }

            foreach (BeamRiderEnemy enemy in _enemies)
            {
                if (!enemy.Alive)
                {
                    continue;
                }

                lines.AddRange(enemy.GetLines());
            }

            foreach (BeamRiderProjectile projectile in _projectiles)
            {
                if (!projectile.Active)
                {
                    continue;
                }

                lines.AddRange(projectile.GetLines());
            }

            AddMissileHudLines(lines);
            sprites.AddRange(_uiRenderer?.Sprites ?? []);
            JollyRenderFrame frame = new(sprites, lines);
            _renderer.SubmitFrame(frame);
        }

        protected override void OnShutdown()
        {
            _gameView.RenderSizeChanged -= OnRenderSizeChanged;
            _introSound = null;
            if (_sector is not null)
            {
                _sector.AllSaucersDestroyed -= OnAllSaucersDestroyed;
                _sector.Dispose();
                _sector = null;
            }

            _uiRenderer?.Clear();
            _uiRenderer = null;
            _uiManager?.Clear();
            _uiManager = null;
            DisposeEnemies();
            _projectiles.Clear();
            _arena?.Dispose();
            _arena = null;
            _player?.Dispose();
            _player = null;
            _audio?.Dispose();
            _audio = null;
            _assets?.Dispose();
            _assets = null;
            _renderer = null;
        }

        private void ConfigureCamera()
        {
            if (_renderer is null)
            {
                return;
            }

            float width = (float)_gameView.ActualWidth;
            float height = (float)_gameView.ActualHeight;
            if (width <= 0.0f || height <= 0.0f)
            {
                return;
            }

            _renderer.Camera.SetViewport(width, height);
            _renderer.Camera.Position = new Vector2(LogicalWidth * 0.5f, LogicalHeight * 0.5f);
            _renderer.Camera.Zoom = MathF.Min(width / LogicalWidth, height / LogicalHeight);
        }

        private void OnRenderSizeChanged(float width, float height)
        {
            if (_renderer is null || width <= 0.0f || height <= 0.0f)
            {
                return;
            }

            _renderer.Camera.SetViewport(width, height);
            _renderer.Camera.Position = new Vector2(LogicalWidth * 0.5f, LogicalHeight * 0.5f);
            _renderer.Camera.Zoom = MathF.Min(width / LogicalWidth, height / LogicalHeight);
            UpdateUIPositions();
        }
    }
}

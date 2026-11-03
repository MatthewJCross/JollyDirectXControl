using Jolly2DEngine.Assets;
using Jolly2DEngine.Audio;
using Jolly2DEngine.Core;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;
using Jolly2DEngine.UI;
using System.IO;
using System.Numerics;
using System.Windows.Input;

namespace Jolly2DTronCycles.Game
{
    public sealed class TronGame : JollyGame
    {
        private enum TronGameState
        {
            Intro,
            Playing,
            GameOver
        }

        private TronGameState _gameState = TronGameState.Intro;

        private readonly DirectXControl _gameView;

        private DirectXJollyRenderer? _renderer;
        private JollyAudioManager? _audio;
        private JollyAssetManager? _assets;

        private JollyUIManager? _uiManager;
        private JollyUIRenderer? _uiRenderer;
        private JollyUIText? _scoreText;
        private JollyUIText? _highScoreText;
        private JollyUIText? _levelText;
        private JollyUIText? _livesText;
        private JollyUIText? _titleText;
        private JollyUIText? _messageText;

        private TronArena _arena;
        private TronCycle? _blueTronCycle;

        private readonly List<TronCycle> _aiCycles = [];
        private readonly List<TronCycle> _aiCyclePool = [];

        private JollyTexture[]? _aiCycleTextures;
        private JollyTexture? _aiTrailTexture;

        private readonly Dictionary<TronCycle, float> _aiTurnCooldowns = [];

        private const int PointsPerSecond = 50;
        private const int PointsPerYellowDeath = 500;
        private const int PointsPerCompletedLevel = 1000;
        private const int StartingLives = 3;
        private const double LevelAnnouncementDuration = 2.0;
        private const int ExtraLifeScore = 10000;
        private bool _extraLifeAwarded;

        private double _survivalTime;
        private int _score;
        private int _bonusScore;
        private int _highScore;
        private int _currentLevel = 1;
        private int _lastScoreSecond = -1;
        private int _lives;

        private double _levelAnnouncementTime;

        private bool _spaceWasDown;
        private bool _boostWasDown;
        private bool _leftWasDown;
        private bool _rightWasDown;
        private bool _upWasDown;
        private bool _downWasDown;

        private double _gameOverFlashTime;
        private bool _gameOverFlashVisible;

        private const double DeathRestartDelay = 1.0;
        private double _deathRestartTimer;
        private bool _pendingLevelRestart;
        private bool _pendingLevelAdvance;

        private static string HighScorePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jolly2D", "TronCycles", "highscore.txt");

        public int Score => _score;

        public int HighScore => _highScore;

        public int CurrentLevel => _currentLevel;

        public TronGame(DirectXControl gameView)
        {
            ArgumentNullException.ThrowIfNull(gameView);

            _gameView = gameView;

            GameLoopFPS = 240.0;
            RenderFPS = 120.0;
        }

        protected override void OnInitialize()
        {
            _renderer = new DirectXJollyRenderer(_gameView);
            _gameView.RenderSizeChanged += OnRenderSizeChanged;
            _audio = new JollyAudioManager();
            _assets = new JollyAssetManager(_renderer, _audio);

            float worldWidth = (float)_gameView.ActualWidth;
            float worldHeight = (float)_gameView.ActualHeight;

            _renderer.Camera.SetBounds(0.0f, 0.0f, worldWidth, worldHeight);
            _renderer.Camera.Position = new Vector2(worldWidth * 0.5f, worldHeight * 0.5f);
            _renderer.Camera.Zoom = 1.0f;
            _renderer.Camera.Rotation = 0.0f;

            _uiManager = new JollyUIManager();
            _uiRenderer = new JollyUIRenderer(_uiManager, texture => _renderer.CreateSprite(texture), (r, g, b, a) => _renderer.CreateSolidTexture(r, g, b, a), (text, fontFamily, fontSize, color, bold, italic, ultrabold) => _renderer.CreateTextTexture(text, fontFamily, fontSize, color, bold, italic));

            CreateUI();
            LoadHighScore();
            StartScore();
            UpdateScoreText();

            _arena = new TronArena(_renderer, worldWidth, worldHeight);

            LoadAIAssets();
            LoadSounds();
            RespawnBlueCycle();
        }

        private void LoadSounds()
        {
            _assets!.LoadSound("RunLoop.wav");
            _assets!.LoadSound("Victory.wav");
            _assets!.LoadSound("Turn.wav");
            _assets!.LoadSound("Turbo.wav");
            _assets!.LoadSound("Die.wav");
        }

        private bool IsSpacePressed()
        {
            bool spaceDown = Input.IsKeyDown(Key.Space);
            bool pressed = spaceDown && !_spaceWasDown;

            _spaceWasDown = spaceDown;

            return pressed;
        }

        private void CreateUI()
        {
            ArgumentNullException.ThrowIfNull(_renderer);
            ArgumentNullException.ThrowIfNull(_uiManager);

            float worldWidth = (float)_gameView.ActualWidth;
            float worldHeight = (float)_gameView.ActualHeight;

            float left = _renderer.Camera.Position.X - worldWidth * 0.5f;
            float top = _renderer.Camera.Position.Y - worldHeight * 0.5f;

            _scoreText = new JollyUIText
            {
                Text = "SCORE: 00000",
                Position = new Vector2(left + 5.0f, top + 10.0f),
                FontFamily = "Arial",
                FontSize = 24.0f,
                Color = JollyColor.White,
                Bold = true,
                Layer = 200
            };

            _highScoreText = new JollyUIText
            {
                Text = "HI-SCORE: 00000",
                Position = new Vector2(left + 200.0f, top + 10.0f),
                FontFamily = "Arial",
                FontSize = 24.0f,
                Color = JollyColor.White,
                Bold = true,
                Layer = 200
            };

            _levelText = new JollyUIText
            {
                Text = "LEVEL: 1-RPG",
                Position = new Vector2(left + 420.0f, top + 10.0f),
                FontFamily = "Arial",
                FontSize = 24.0f,
                Color = JollyColor.White,
                Bold = true,
                Layer = 200
            };

            _livesText = new JollyUIText
            {
                Text = "LIVES: 3",
                Position = new Vector2(left + 685.0f, top + 10.0f),
                FontFamily = "Arial",
                FontSize = 24.0f,
                Color = JollyColor.White,
                Bold = true,
                Layer = 200
            };

            _titleText = new JollyUIText
            {
                Text = "TRON CYCLES",
                Position = new Vector2(left + worldWidth * 0.5f - 260.0f, top + worldHeight * 0.5f - 120.0f),
                FontFamily = "Arial",
                FontSize = 72.0f,
                Color = JollyColor.Yellow,
                Bold = true,
                Layer = 300
            };

            _messageText = new JollyUIText
            {
                Text = "PRESS SPACE TO START",
                Position = new Vector2(left + worldWidth * 0.5f - 220.0f, top + worldHeight * 0.5f),
                FontFamily = "Arial",
                FontSize = 36.0f,
                Color = JollyColor.White,
                Bold = true,
                Layer = 300
            };

            _uiManager.Add(_titleText);
            _uiManager.Add(_messageText);
            _uiManager.Add(_scoreText);
            _uiManager.Add(_highScoreText);
            _uiManager.Add(_levelText);
            _uiManager.Add(_livesText);
        }

        private void LoadAIAssets()
        {
            ArgumentNullException.ThrowIfNull(_renderer);
            ArgumentNullException.ThrowIfNull(_assets);

            _aiCycleTextures =
            [
                _assets.LoadTexture("YellowCycleUp.png"),
                _assets.LoadTexture("YellowCycleDown.png"),
                _assets.LoadTexture("YellowCycleLeft.png"),
                _assets.LoadTexture("YellowCycleRight.png")
            ];

            _aiTrailTexture = _renderer.CreateSolidTexture(255, 255, 0, 255);
        }

        private void CreateAICycles()
        {
            ArgumentNullException.ThrowIfNull(_renderer);
            ArgumentNullException.ThrowIfNull(_aiCycleTextures);
            ArgumentNullException.ThrowIfNull(_aiTrailTexture);

            _aiCycles.Clear();
            _aiTurnCooldowns.Clear();

            TronLevel level = TronLevels.Get(_currentLevel);

            for (int i = 0; i < level.AICount; i++)
            {
                (float x, float y, TronDirection direction) = GetAISpawn(i, level.AICount);
                TronCycle aiCycle;

                if (i < _aiCyclePool.Count)
                {
                    aiCycle = _aiCyclePool[i];
                }
                else
                {
                    aiCycle = new TronCycle(_renderer, x, y, _aiCycleTextures, _aiTrailTexture, direction);
                    _aiCyclePool.Add(aiCycle);
                }

                aiCycle.SlowSpeed = level.AISlowSpeed;
                aiCycle.Speed = level.AISpeed;
                aiCycle.BoostSpeed = level.AIBoostSpeed;
                aiCycle.Reset(x, y, direction);

                _aiCycles.Add(aiCycle);
                _aiTurnCooldowns[aiCycle] = 0.0f;
            }
        }

        private (float X, float Y, TronDirection Direction) GetAISpawn(int index, int count)
        {
            float width = (float)_gameView.ActualWidth;
            float height = (float)_gameView.ActualHeight;

            return index switch
            {
                0 => (60.0f, height * 0.5f, TronDirection.Right),
                1 => (width - 60.0f, height * 0.5f, TronDirection.Left),
                2 => (width * 0.5f, 120.0f, TronDirection.Down),
                3 => (width * 0.5f, height - 60.0f, TronDirection.Up),
                4 => (width * 0.25f, height * 0.25f, TronDirection.Right),
                5 => (width * 0.75f, height * 0.75f, TronDirection.Left),
                _ => (60.0f, 60.0f, TronDirection.Right)
            };
        }

        private void UpdateGameStateUI()
        {
            if (_titleText is null || _messageText is null)
            {
                return;
            }

            switch (_gameState)
            {
                case TronGameState.Intro:
                    _titleText.Text = "TRON CYCLES";
                    _titleText.Color = JollyColor.White;
                    _titleText.Position = new Vector2(_renderer!.Camera.Position.X - 180.0f, _titleText.Position.Y);
                    _messageText.Text = "PRESS SPACE TO START";
                    break;

                case TronGameState.Playing:
                    _titleText.Text = string.Empty;
                    _messageText.Text = string.Empty;
                    break;

                case TronGameState.GameOver:
                    _titleText.Text = "GAME OVER";
                    _titleText.Color = JollyColor.Red;
                    _titleText.Position = new Vector2(_renderer!.Camera.Position.X - 195.0f, _titleText.Position.Y);
                    _messageText.Position = new Vector2(_renderer!.Camera.Position.X - 325.0f, _messageText.Position.Y);
                    _messageText.Text = "PRESS SPACE TO RESTART";
                    break;
            }
        }

        private void ShowLevelAnnouncement()
        {
            if (_titleText is null || _messageText is null || _renderer is null)
            {
                return;
            }

            TronLevel level = TronLevels.Get(_currentLevel);

            _levelAnnouncementTime = LevelAnnouncementDuration;

            _titleText.Text = $"LEVEL {_currentLevel}";
            _titleText.Color = JollyColor.Yellow;
            _titleText.FontSize = 64.0f;
            _titleText.Position = new Vector2(_renderer.Camera.Position.X - 130.0f, _renderer.Camera.Position.Y - 100.0f);

            _messageText.Text = level.Name;
            _messageText.Color = JollyColor.White;
            _messageText.FontSize = 48.0f;
            _messageText.Position = new Vector2(_renderer.Camera.Position.X - 50.0f, _renderer.Camera.Position.Y + 10.0f);
        }

        private void UpdateLevelAnnouncement(double deltaSeconds)
        {
            if (_levelAnnouncementTime <= 0.0)
            {
                return;
            }

            _levelAnnouncementTime -= deltaSeconds;

            if (_levelAnnouncementTime > 0.0)
            {
                return;
            }

            _levelAnnouncementTime = 0.0;

            if (_gameState == TronGameState.Playing)
            {
                _titleText!.Text = string.Empty;
                _messageText!.Text = string.Empty;
            }
        }

        protected override void OnUpdate(JollyGameTime gameTime)
        {
            bool spacePressed = IsSpacePressed();

            if (_gameState == TronGameState.Intro)
            {
                if (spacePressed)
                {
                    StartGame();
                }

                _uiRenderer?.Update();
                SubmitUIFrame();

                return;
            }

            if (_gameState == TronGameState.GameOver)
            {
                _gameOverFlashTime += gameTime.DeltaSeconds;

                if (_gameOverFlashTime >= 0.4)
                {
                    _gameOverFlashTime = 0.0;
                    _gameOverFlashVisible = !_gameOverFlashVisible;
                }

                _titleText?.Color = _gameOverFlashVisible ? JollyColor.Red : JollyColor.Transparent;

                if (Input.IsKeyDown(Key.Space))
                {
                    StartGame();
                }

                _uiRenderer?.Update();
                SubmitUIFrame();

                return;
            }

            UpdateLevelAnnouncement(gameTime.DeltaSeconds);

            UpdateRestartDelay(gameTime.DeltaSeconds);

            if (_pendingLevelRestart)
            {
                _uiRenderer?.Update();
                SubmitUIFrame();

                return;
            }

            if (_blueTronCycle is null)
            {
                return;
            }

            if (_blueTronCycle.IsDead)
            {
                return;
            }

            bool leftDown = Input.IsKeyDown(Key.A) || Input.IsKeyDown(Key.Left);
            bool rightDown = Input.IsKeyDown(Key.D) || Input.IsKeyDown(Key.Right);
            bool upDown = Input.IsKeyDown(Key.W) || Input.IsKeyDown(Key.Up);
            bool downDown = Input.IsKeyDown(Key.S) || Input.IsKeyDown(Key.Down);

            if (leftDown && !_leftWasDown)
            {
                _blueTronCycle.SetDirection(TronDirection.Left);
                _audio?.PlaySound("Turn");
            }

            if (rightDown && !_rightWasDown)
            {
                _blueTronCycle.SetDirection(TronDirection.Right);
                _audio?.PlaySound("Turn");
            }

            if (upDown && !_upWasDown)
            {
                _blueTronCycle.SetDirection(TronDirection.Up);
                _audio?.PlaySound("Turn");
            }

            if (downDown && !_downWasDown)
            {
                _blueTronCycle.SetDirection(TronDirection.Down);
                _audio?.PlaySound("Turn");
            }

            _leftWasDown = leftDown;
            _rightWasDown = rightDown;
            _upWasDown = upDown;
            _downWasDown = downDown;

            bool boostDown = Input.IsKeyDown(Key.Space);
            bool slowDown = Input.IsKeyDown(Key.LeftCtrl);

            if (boostDown && !slowDown && !_boostWasDown)
            {
                _audio?.PlaySound("Turbo");
            }

            _boostWasDown = boostDown;

            _blueTronCycle.SetBoost(boostDown && !slowDown);
            _blueTronCycle.SetSlow(slowDown && !boostDown);
            _blueTronCycle.Update((float)gameTime.DeltaSeconds);

            UpdateScore(gameTime.DeltaSeconds);
            UpdateAICycles((float)gameTime.DeltaSeconds);
            CheckCollisions();

            _uiRenderer?.Update();
            SubmitUIFrame();
        }

        private void SubmitUIFrame()
        {
            if (_renderer is null)
            {
                return;
            }

            List<JollySprite> renderSprites =
            [
                .. _arena.Sprites,
            .. (_blueTronCycle?.Trail.Sprites ?? []),
            .. (_blueTronCycle?.Sprites ?? [])
            ];

            foreach (TronCycle aiCycle in _aiCycles)
            {
                if (aiCycle.IsDead)
                {
                    continue;
                }

                renderSprites.AddRange(aiCycle.Trail.Sprites);
                renderSprites.AddRange(aiCycle.Sprites);
            }

            renderSprites.AddRange(_uiRenderer?.Sprites ?? []);

            _renderer.SubmitFrame(new JollyRenderFrame(renderSprites));
        }

        private void StartGame()
        {
            ClearGameObjects();

            _spaceWasDown = Input.IsKeyDown(Key.Space);

            _gameState = TronGameState.Playing;
            _currentLevel = 1;
            _lives = StartingLives;
            _extraLifeAwarded = false;

            StartScore();
            RespawnBlueCycle();
            CreateAICycles();

            UpdateScoreText();
            UpdateGameStateUI();
            ShowLevelAnnouncement();

            _assets.PlayMusic("RunLoop", true);
        }

        private const float AIDecisionInterval = 0.05f;
        private void UpdateAICycles(float deltaSeconds)
        {
            foreach (TronCycle aiCycle in _aiCycles)
            {
                if (aiCycle.IsDead)
                {
                    continue;
                }

                if (!_aiTurnCooldowns.TryGetValue(aiCycle, out float cooldown))
                {
                    cooldown = 0.0f;
                }

                cooldown -= deltaSeconds;

                if (cooldown <= 0.0f)
                {
                    UpdateAIDirection(aiCycle);
                    cooldown = AIDecisionInterval;
                }

                _aiTurnCooldowns[aiCycle] = cooldown;

                aiCycle.Update(deltaSeconds);
            }
        }

        private void UpdateAIDirection(TronCycle aiCycle)
        {
            TronDirection currentDirection = aiCycle.CurrentDirection;

            float halfWidth = aiCycle.Sprite.Size.X * 0.5f;
            float halfHeight = aiCycle.Sprite.Size.Y * 0.5f;

            float wallClearance = currentDirection switch
            {
                TronDirection.Up => aiCycle.Y - halfHeight - _arena.Top,
                TronDirection.Down => _arena.Bottom - (aiCycle.Y + halfHeight),
                TronDirection.Left => aiCycle.X - halfWidth - _arena.Left,
                TronDirection.Right => _arena.Right - (aiCycle.X + halfWidth),
                _ => 0.0f
            };

            float currentSpeed = MathF.Max(MathF.Abs(aiCycle.VelocityX), MathF.Abs(aiCycle.VelocityY));
            float turnDistance = MathF.Max(90.0f, currentSpeed * 0.45f);

            if (wallClearance > turnDistance)
            {
                float forwardClearance = GetDirectionClearance(aiCycle, currentDirection);

                if (forwardClearance > turnDistance)
                {
                    return;
                }
            }

            TronDirection leftDirection = TurnLeft(currentDirection);
            TronDirection rightDirection = TurnRight(currentDirection);

            float leftClearance = GetDirectionClearance(aiCycle, leftDirection);
            float rightClearance = GetDirectionClearance(aiCycle, rightDirection);

            const float minimumTurnClearance = 40.0f;

            bool leftSafe = leftClearance >= minimumTurnClearance;
            bool rightSafe = rightClearance >= minimumTurnClearance;

            if (!leftSafe && !rightSafe)
            {
                return;
            }

            TronDirection selectedDirection;

            if (leftSafe && !rightSafe)
            {
                selectedDirection = leftDirection;
            }
            else if (rightSafe && !leftSafe)
            {
                selectedDirection = rightDirection;
            }
            else if (leftClearance >= rightClearance)
            {
                selectedDirection = leftDirection;
            }
            else
            {
                selectedDirection = rightDirection;
            }

            aiCycle.SetDirection(selectedDirection);
        }

        private float GetDirectionClearance(TronCycle cycle, TronDirection direction)
        {
            const float maxLookAhead = 400.0f;
            const float step = 8.0f;

            float x = cycle.X;
            float y = cycle.Y;

            float halfWidth = cycle.Sprite.Size.X * 0.5f;
            float halfHeight = cycle.Sprite.Size.Y * 0.5f;
            float radius = MathF.Min(cycle.Sprite.Size.X, cycle.Sprite.Size.Y) * 0.2f;

            switch (direction)
            {
                case TronDirection.Up:
                    y -= halfHeight;
                    break;

                case TronDirection.Down:
                    y += halfHeight;
                    break;

                case TronDirection.Left:
                    x -= halfWidth;
                    break;

                case TronDirection.Right:
                    x += halfWidth;
                    break;
            }

            for (float distance = 0.0f; distance <= maxLookAhead; distance += step)
            {
                switch (direction)
                {
                    case TronDirection.Up:
                        y -= step;
                        break;

                    case TronDirection.Down:
                        y += step;
                        break;

                    case TronDirection.Left:
                        x -= step;
                        break;

                    case TronDirection.Right:
                        x += step;
                        break;
                }

                if (x - radius <= _arena.Left)
                {
                    return distance;
                }

                if (x + radius >= _arena.Right)
                {
                    return distance;
                }

                if (y - radius <= _arena.Top)
                {
                    return distance;
                }

                if (y + radius >= _arena.Bottom)
                {
                    return distance;
                }

                if (_blueTronCycle is not null && _blueTronCycle.Trail.IsCollidingIncludingCurrent(x, y, radius))
                {
                    return distance;
                }

                if (cycle.Trail.IsColliding(x, y, radius))
                {
                    return distance;
                }

                foreach (TronCycle otherCycle in _aiCycles)
                {
                    if (ReferenceEquals(otherCycle, cycle))
                    {
                        continue;
                    }

                    if (otherCycle.IsDead)
                    {
                        continue;
                    }

                    if (otherCycle.Trail.IsCollidingIncludingCurrent(x, y, radius))
                    {
                        return distance;
                    }
                }
            }

            return maxLookAhead;
        }

        private bool IsCycleOutsideArena(TronCycle cycle)
        {
            float halfWidth = cycle.Sprite.Size.X * 0.5f;
            float halfHeight = cycle.Sprite.Size.Y * 0.5f;

            return cycle.X - halfWidth <= _arena.Left || cycle.X + halfWidth >= _arena.Right || cycle.Y - halfHeight <= _arena.Top || cycle.Y + halfHeight >= _arena.Bottom;
        }
        
        private void CheckCollisions()
        {
            if (_blueTronCycle is null)
            {
                return;
            }

            bool blueDead = false;
            bool yellowDied = false;

            if (IsCycleOutsideArena(_blueTronCycle))
            {
                blueDead = true;
            }

            if (_blueTronCycle.IsCollidingWithTrail(_blueTronCycle.Trail, true))
            {
                blueDead = true;
            }

            foreach (TronCycle aiCycle in _aiCycles)
            {
                if (aiCycle.IsDead)
                {
                    continue;
                }

                if (IsCycleOutsideArena(aiCycle))
                {
                    KillYellowCycle(aiCycle);
                    yellowDied = true;
                    continue;
                }

                if (aiCycle.IsCollidingWithTrail(aiCycle.Trail, false))
                {
                    KillYellowCycle(aiCycle);
                    yellowDied = true;
                    continue;
                }

                if (aiCycle.IsCollidingWithTrail(_blueTronCycle.Trail, true))
                {
                    KillYellowCycle(aiCycle);
                    yellowDied = true;
                    continue;
                }

                if (_blueTronCycle.IsCollidingWithTrail(aiCycle.Trail, true))
                {
                    blueDead = true;
                    break;
                }

                if (aiCycle.IsCollidingWithCycle(_blueTronCycle))
                {
                    KillYellowCycle(aiCycle);
                    yellowDied = true;
                    blueDead = true;
                    continue;
                }

                foreach (TronCycle otherAI in _aiCycles)
                {
                    if (ReferenceEquals(aiCycle, otherAI))
                    {
                        continue;
                    }

                    if (otherAI.IsDead)
                    {
                        continue;
                    }

                    if (otherAI.IsCollidingWithTrail(aiCycle.Trail, true))
                    {
                        KillYellowCycle(otherAI);
                        KillYellowCycle(aiCycle);
                        yellowDied = true;
                        continue;
                    }

                    if (aiCycle.IsCollidingWithTrail(otherAI.Trail, true))
                    {
                        KillYellowCycle(aiCycle);
                        yellowDied = true;
                        continue;
                    }

                    if (aiCycle.IsCollidingWithCycle(otherAI))
                    {
                        KillYellowCycle(aiCycle);
                        KillYellowCycle(otherAI);
                        yellowDied = true;
                    }
                }
            }

            if (blueDead)
            {
                _audio?.PlaySound("Die");
                _blueTronCycle.Die();
                _lives--;
                UpdateScoreText();

                if (_lives > 0)
                {
                    StartRestartDelay(false);
                    return;
                }

                _audio?.StopAllSounds();
                _audio?.StopMusic();
                SaveHighScore();
                ClearGameObjects();
                _gameState = TronGameState.GameOver;
                _gameOverFlashTime = 0.0;
                _gameOverFlashVisible = true;
                UpdateGameStateUI();
                return;
            }

            if (yellowDied && _aiCycles.All(cycle => cycle.IsDead))
            {
                StartRestartDelay(true);
            }
        }

        private void KillYellowCycle(TronCycle aiCycle)
        {
            if (aiCycle.IsDead)
            {
                return;
            }

            aiCycle.Die();
            _bonusScore += PointsPerYellowDeath;
            _score += PointsPerYellowDeath;
            UpdateScoreText();
            _audio?.PlaySound("Victory");
        }

        private void AdvanceLevel()
        {
            if (_currentLevel < TronLevels.All.Length)
            {
                _currentLevel++;
                _bonusScore += PointsPerCompletedLevel;
                _score += PointsPerCompletedLevel;
            }

            UpdateScoreText();
            ShowLevelAnnouncement();
        }

        private void LoadHighScore()
        {
            try
            {
                if (!File.Exists(HighScorePath))
                {
                    _highScore = 0;
                    return;
                }

                string text = File.ReadAllText(HighScorePath);

                if (int.TryParse(text, out int highScore) && highScore >= 0)
                {
                    _highScore = highScore;
                }
                else
                {
                    _highScore = 0;
                }
            }
            catch
            {
                _highScore = 0;
            }
        }

        private void SaveHighScore()
        {
            try
            {
                string? directory = Path.GetDirectoryName(HighScorePath);

                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(HighScorePath, _highScore.ToString());
            }
            catch
            {
            }
        }

        private void StartScore()
        {
            _survivalTime = 0.0;
            _score = 0;
            _bonusScore = 0;
            _lives = StartingLives;
            _lastScoreSecond = -1;
            UpdateScoreText();
        }

        private void UpdateScore(double deltaSeconds)
        {
            if (_blueTronCycle is null || _blueTronCycle.IsDead)
            {
                return;
            }

            _survivalTime += deltaSeconds;
            int currentSecond = (int)_survivalTime;

            if (currentSecond == _lastScoreSecond)
            {
                return;
            }

            _lastScoreSecond = currentSecond;
            _score = (currentSecond * PointsPerSecond) + _bonusScore;

            if (_score > _highScore)
            {
                _highScore = _score;
            }

            CheckExtraLife();
            UpdateScoreText();
        }

        private void CheckExtraLife()
        {
            if (_extraLifeAwarded)
            {
                return;
            }

            if (_score < ExtraLifeScore)
            {
                return;
            }

            _extraLifeAwarded = true;
            _lives++;
            UpdateScoreText();
        }

        private void UpdateScoreText()
        {
            TronLevel level = TronLevels.Get(_currentLevel);

            _scoreText?.Text = $"SCORE: {_score:D5}";
            _highScoreText?.Text = $"HI-SCORE: {_highScore:D5}";
            _levelText?.Text = $"LEVEL: {_currentLevel}-{level.Name}";
            _livesText?.Text = $"LIVES: {_lives}";
        }

        private static TronDirection GetCurrentDirection(TronCycle cycle)
        {
            if (cycle.VelocityX > 0.0f)
            {
                return TronDirection.Right;
            }

            if (cycle.VelocityX < 0.0f)
            {
                return TronDirection.Left;
            }

            if (cycle.VelocityY > 0.0f)
            {
                return TronDirection.Down;
            }

            return TronDirection.Up;
        }

        private static TronDirection TurnLeft(TronDirection direction)
        {
            return direction switch
            {
                TronDirection.Up => TronDirection.Left,
                TronDirection.Left => TronDirection.Down,
                TronDirection.Down => TronDirection.Right,
                TronDirection.Right => TronDirection.Up,
                _ => TronDirection.Right
            };
        }

        private static TronDirection TurnRight(TronDirection direction)
        {
            return direction switch
            {
                TronDirection.Up => TronDirection.Right,
                TronDirection.Right => TronDirection.Down,
                TronDirection.Down => TronDirection.Left,
                TronDirection.Left => TronDirection.Up,
                _ => TronDirection.Right
            };
        }

        protected override void OnShutdown()
        {
            _gameView.RenderSizeChanged -= OnRenderSizeChanged;

            SaveHighScore();

            _uiRenderer?.Clear();
            _uiRenderer = null;

            _uiManager?.Clear();
            _uiManager = null;

            _audio?.Dispose();
            _audio = null;

            _assets?.Dispose();
            _assets = null;

            _renderer = null;
        }

        private void RespawnBlueCycle()
        {
            ArgumentNullException.ThrowIfNull(_renderer);
            ArgumentNullException.ThrowIfNull(_assets);

            if (_blueTronCycle is null)
            {
                JollyTexture[] blueCycleTextures =
                [
                    _assets.LoadTexture("BlueCycleUp.png"),
                    _assets.LoadTexture("BlueCycleDown.png"),
                    _assets.LoadTexture("BlueCycleLeft.png"),
                    _assets.LoadTexture("BlueCycleRight.png")
                ];

                JollyTexture blueTrailTexture = _renderer.CreateSolidTexture(0, 128, 255, 255);
                _blueTronCycle = new TronCycle(_renderer, 720.0f, 120.0f, blueCycleTextures, blueTrailTexture, TronDirection.Down);
                _blueTronCycle.Speed = 160.0f;
                _blueTronCycle.BoostSpeed = 240.0f;
                _blueTronCycle.BoostAvailable = true;
            }

            _blueTronCycle.Reset(720.0f, 120.0f, TronDirection.Down);
        }

        private void StartRestartDelay(bool advanceLevel)
        {
            _deathRestartTimer = DeathRestartDelay;
            _pendingLevelRestart = true;
            _pendingLevelAdvance = advanceLevel;
        }

        private void UpdateRestartDelay(double deltaTime)
        {
            if (!_pendingLevelRestart)
            {
                return;
            }

            _deathRestartTimer -= deltaTime;

            if (_deathRestartTimer > 0.0)
            {
                return;
            }

            _deathRestartTimer = 0.0;
            _pendingLevelRestart = false;

            bool advanceLevel = _pendingLevelAdvance;
            _pendingLevelAdvance = false;

            ClearGameObjects();

            if (advanceLevel)
            {
                AdvanceLevel();
            }

            RespawnBlueCycle();
            CreateAICycles();
        }

        private void ClearGameObjects()
        {
            _blueTronCycle?.Deactivate();

            foreach (TronCycle aiCycle in _aiCycles)
            {
                aiCycle.Deactivate();
            }

            _aiCycles.Clear();
            _aiTurnCooldowns.Clear();
        }

        private void OnRenderSizeChanged(float width, float height)
        {
            if (_renderer is null)
            {
                return;
            }

            _renderer.Camera.SetViewport(width, height);
            _arena.Resize(width, height);
        }
    }
}

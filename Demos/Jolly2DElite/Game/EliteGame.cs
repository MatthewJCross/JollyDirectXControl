using Jolly2DElite.Game.Graphics;
using Jolly2DElite.Game.Trading;
using Jolly2DElite.Graphics;
using Jolly2DEngine.Assets;
using Jolly2DEngine.Audio;
using Jolly2DEngine.Core;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;
using Jolly2DEngine.UI;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Input;

namespace Jolly2DElite.Game
{
    public sealed class EliteGame : JollyGame
    {
        private const float WorldWidth = 680f;
        private const float WorldHeight = 578f;
        private const float CockpitHeight = 149.0f;
        private const float FlightViewportHeight = WorldHeight - CockpitHeight;
        private const float YawRate = 1.5f;
        private const float PitchRate = 1.2f;
        private const float RollRate = 1.8f;
        private const float ThrottleRate = 4.0f;

        private readonly DirectXControl _gameView;
        private DirectXJollyRenderer? _renderer;
        private JollyAudioManager? _audio;
        private JollyAssetManager? _assets;

        private enum EliteGameState
        {
            StartGame,
            RestartAfterDeath,
            LoadCommanderTitle,
            LoadCommander,
            LoadDefaultCommander,
            DefaultCommanderTitle,
            Docked,
            Market,
            GalacticChart, 
            LaunchSequence,
            Flight
        }

        private EliteGameState _nextGameState = EliteGameState.StartGame;
        private EliteGameState _currentGameState = EliteGameState.StartGame;
        private bool _gameStatePending = true;

        private bool _titleKeyWasDown;
        private bool _spaceWasDown;
        private bool _f1WasDown;


        private JollyUIManager? _uiManager;
        private JollyUIRenderer? _uiRenderer;
        private readonly ShipRenderer _shipRenderer = new();
        private readonly WorldRenderer _worldRenderer = new();
        private readonly StarFieldRenderer _starFieldRenderer = new();
        private readonly FlightOverlayRenderer _flightOverlayRenderer = new();
        
        private readonly ShipModel _cougarModel = new FerDeLanceModel();
        private float _cougarRotationY;
        private float _cougarRotationX;
        private float _cougarZoom = 0.1f;

        private JollyUIText? _titleText;
        private JollyUIText? _messageText;
        private JollyUIText? _commanderText;
        private JollyUIText? _dockedStatusText;
        private JollyUIText? _flightStatusText;
        private JollyUIText? _instrumentText;
        private JollyUIText? _radarText;
        private JollyUIText? _commanderListText;
        private JollyLineSnapshot? _commanderUnderline;
        private JollyUIText? _marketText;
        private readonly TradingService _tradingService = new();
        private int _marketSelected;
        private int _marketQuantity = 1;


        private List<JollySprite> _borderSprites = [];

        
        private CockpitDashboard? _cockpitDashboard;


        private bool _marketUpWasDown;
        private bool _marketDownWasDown;
        private bool _marketLeftWasDown;
        private bool _marketRightWasDown;
        private bool _marketBuyWasDown;
        private bool _marketSellWasDown;
        private bool _marketEscapeWasDown;
        private bool _marketOpenWasDown;

        private int _laserTemperature; 
        private float _laserCooldown;
        private float _laserPulseTime;
        private const float LaserPulseDuration = 0.10f;
        private const float LaserCooldownDuration = 0.20f;

        private bool _hyperspaceKeyWasDown;
        private int _hyperspaceCountdown;
        private int _hyperspaceTicks;

        private bool _galacticChartKeyWasDown;
        private string _pendingFlightMessage = string.Empty;
        private bool _returningFromChart;
        private int _chartCrosshairX;
        private int _chartCrosshairY;

        private float _launchSequenceTime;
        private const float LaunchSequenceDuration = 1.5f;
        private const int LaunchTunnelRingCount = 10;
        private const int LaunchTunnelSides = 10;

        private int _commanderSelected;
        private bool _commanderKeyWasDown;
        private CommanderStore? _commanderStore;
        private readonly Commander _commander = new();
        private readonly Galaxy _galaxy = new();
        private readonly PlayerShip _playerShip = new();

        private readonly List<WorldObject> _worldObjects = [];
        private readonly EliteFlightSimulation _flightSimulation = new();

        public EliteGame(DirectXControl gameView)
        {
            ArgumentNullException.ThrowIfNull(gameView);

            _gameView = gameView;

            GameLoopFPS = 50.0;
            RenderFPS = 50.0;

            gameView.RenderSizeChanged += GameView_RenderSizeChanged;

        }

        private void GameView_RenderSizeChanged(float width, float height)
        {
            if (width <= 0.0f || height <= 0.0f)
            {
                return;
            }

            _renderer!.Camera.SetViewport(width, height);

            UpdateCameraScale();
        }

        private void UpdateCameraScale()
        {
            float viewportWidth = _renderer!.Camera.ViewportSize.X;
            float viewportHeight = _renderer!.Camera.ViewportSize.Y;

            float scaleX = viewportWidth / WorldWidth;
            float scaleY = viewportHeight / WorldHeight;

            _renderer.Camera.Zoom = MathF.Min(scaleX, scaleY);
        }

        protected override void OnInitialize()
        {
            _renderer = new DirectXJollyRenderer(_gameView);
            _audio = new JollyAudioManager();
            _assets = new JollyAssetManager(_renderer, _audio);

            string dataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jolly2DElite");
            _commanderStore = new CommanderStore(dataFolder);

            _renderer.Camera.SetBounds(0.0f, 0.0f, WorldWidth, WorldHeight);
            _renderer.Camera.Position = new Vector2(WorldWidth * 0.5f, WorldHeight * 0.5f);
            _renderer.Camera.Zoom = 1.0f;
            _renderer.Camera.Rotation = 0.0f;




            _uiManager = new JollyUIManager();
            _uiRenderer = new JollyUIRenderer(_uiManager, texture => _renderer.CreateSprite(texture), (r, g, b, a) => _renderer.CreateSolidTexture(r, g, b, a), (text, fontFamily, fontSize, color, bold, italic, ultraBold) => _renderer.CreateTextTexture(text, fontFamily, fontSize, color, bold, italic, ultraBold));
            CreateBorder();  
            CreateCockpit();
            CreateUI();
        }

        private void ResetGame()
        {
            _playerShip.Reset();
            _worldObjects.Clear();

            SetupSystemUniverse();

            _worldObjects.AddRange(UniverseFactory.CreateInitialObjects(_galaxy.SelectedSeeds));

            _flightSimulation.Reset(_worldObjects, _galaxy.SelectedSeeds, _galaxy.SelectedTechLevel, _galaxy.SelectedGovernment);

            _laserCooldown = 0.0f;
            _laserPulseTime = 0.0f;
        }

        private void SetupSystemUniverse()
        {
            _worldObjects.Clear();

            int[] seeds = _galaxy.SelectedSeeds;

            int zSign = (seeds[1] & 3) + 3;
            int xySign = zSign >> 1;

            Vector3 planetPosition = new(xySign * 256.0f, xySign * 256.0f, zSign * 256.0f);

            WorldObject planet = new(ShipCatalogue.CreatePlanet(), planetPosition)
            {
                IsPersistent = true
            };

            _worldObjects.Add(planet);

            int sunZSign = (seeds[3] & 7) | 1;
            int sunXHigh = seeds[5] & 3;

            Vector3 sunPosition = new(sunXHigh * 257.0f, planetPosition.Y, -sunZSign * 256.0f);

            WorldObject sun = new(ShipCatalogue.CreateSun(), sunPosition)
            {
                IsPersistent = true
            };

            _worldObjects.Add(sun);
        }

        protected override void OnUpdate(JollyGameTime gameTime)
        {
            float deltaSeconds = (float)gameTime.DeltaSeconds;

            List<JollySprite> sprites = [];
            List<JollyTriangleSnapshot> triangles = [];
            List<JollyLineSnapshot> lines = [];

            ProcessGameState(deltaSeconds);

            bool chartKeyDown = Input.IsKeyDown(Key.F5);

            if (chartKeyDown && !_galacticChartKeyWasDown)
            {
                if (_currentGameState == EliteGameState.Flight)
                {
                    _chartCrosshairX = _galaxy.CrosshairX;
                    _chartCrosshairY = _galaxy.CrosshairY;

                    JumpTo(EliteGameState.GalacticChart);
                }
                else if (_currentGameState == EliteGameState.GalacticChart)
                {
                    _returningFromChart = true;
                    JumpTo(EliteGameState.Flight);
                }
            }

            _galacticChartKeyWasDown = chartKeyDown;

            if (_currentGameState == EliteGameState.LoadCommanderTitle || _currentGameState == EliteGameState.DefaultCommanderTitle)
            {
                _cougarRotationY += deltaSeconds * 0.8f;
                _cougarRotationX += deltaSeconds * 0.35f;
                _cougarZoom = MathF.Min(1.0f, _cougarZoom + deltaSeconds * 0.35f);
            }

            _uiRenderer?.Update();

            if (_borderSprites is not null)
            {
                sprites.AddRange(_borderSprites);
            }

            if (_uiRenderer is not null)
            {
                sprites.AddRange(_uiRenderer.Sprites);
            }

            if (_commanderUnderline.HasValue)
            {
                lines.Add(_commanderUnderline.Value);
            }

            if (_currentGameState == EliteGameState.LoadCommanderTitle || _currentGameState == EliteGameState.DefaultCommanderTitle)
            {
                DrawShip(lines, triangles, _cougarModel, new Vector2(WorldWidth * 0.5f, WorldHeight * 0.4f), _cougarZoom, _cougarRotationY, _cougarRotationX);
            }
            else if (_currentGameState == EliteGameState.LaunchSequence)
            {
                DrawLaunchTunnel(lines);
            }
            else if (_currentGameState == EliteGameState.Flight)
            {
                UpdateFlightInput(deltaSeconds);

                _flightSimulation.Update(_worldObjects, _playerShip, deltaSeconds, _galaxy.SelectedTechLevel, _galaxy.SelectedGovernment);

                if (CanDockAtStation())
                {
                    JumpTo(EliteGameState.Docked);
                }

                if (_playerShip.Energy <= 0.0f)
                {
                    JumpTo(EliteGameState.RestartAfterDeath);
                }

                Matrix4x4 viewMatrix = _worldRenderer.CreateViewMatrix(_playerShip);
                Vector2 screenCenter = new(WorldWidth * 0.5f, FlightViewportHeight * 0.5f);

                _starFieldRenderer.UpdateAndDraw(triangles, _playerShip, viewMatrix, screenCenter, WorldWidth, FlightViewportHeight, deltaSeconds);
                _worldRenderer.DrawUniverse(lines, triangles, _worldObjects, viewMatrix, screenCenter);
                _flightOverlayRenderer.DrawCrosshair(lines, screenCenter);
                _flightOverlayRenderer.DrawShieldBars(lines, WorldHeight - CockpitHeight, _playerShip.ForwardShield, _playerShip.AftShield);

                if (_laserPulseTime > 0.0f)
                {
                    _flightOverlayRenderer.DrawLaser(lines, screenCenter);
                }

                if (_laserTemperature > 0)
                {
                    _laserTemperature--;
                }
            }
            else if (_currentGameState == EliteGameState.GalacticChart)
            {
                bool hyperspaceDown = Input.IsKeyDown(Key.H);

                if (hyperspaceDown && !_hyperspaceKeyWasDown)
                {
                    StartHyperspace();
                }

                _hyperspaceKeyWasDown = hyperspaceDown;

                if (_hyperspaceCountdown > 0)
                {
                    UpdateHyperspaceCountdown();
                }
                else
                {
                    UpdateGalacticChartInput(deltaSeconds);
                }

                DrawGalacticChart(lines, triangles);
            }

            _cockpitDashboard?.Draw(sprites, (int)_playerShip.Speed, _playerShip.Roll, _playerShip.Pitch, (int)_playerShip.ForwardShield, (int)_playerShip.AftShield, (int)_playerShip.Energy, _commander.Fuel, _commander.Missiles, _laserTemperature, _worldObjects, _playerShip);

            JollyRenderFrame frame = new(sprites, lines, triangles);

            _renderer?.SubmitFrame(frame);
        }

        private void ProcessGameState(float deltaSeconds)
        {
            if (_gameStatePending)
            {
                _gameStatePending = false;
                _currentGameState = _nextGameState;

                switch (_currentGameState)
                {
                    case EliteGameState.StartGame:
                        StartGame();
                        break;

                    case EliteGameState.RestartAfterDeath:
                        RestartAfterDeath();
                        break;

                    case EliteGameState.LoadCommanderTitle:
                        ShowLoadCommanderTitle();
                        break;

                    case EliteGameState.LoadCommander:
                        ShowLoadCommander();
                        break;

                    case EliteGameState.LoadDefaultCommander:
                        LoadDefaultCommander();
                        break;

                    case EliteGameState.DefaultCommanderTitle:
                        ShowDefaultCommanderTitle();
                        break;

                    case EliteGameState.Docked:
                        ShowDockedScreen();
                        break;

                    case EliteGameState.Market:
                        ShowMarketScreen();
                        break;

                    case EliteGameState.LaunchSequence:
                        ShowLaunchSequence();
                        break;

                    case EliteGameState.GalacticChart:
                        _chartCrosshairX = _galaxy.CrosshairX;
                        _chartCrosshairY = _galaxy.CrosshairY;
                        _messageText!.Visible = true;
                        break;

                    case EliteGameState.Flight:
                        if (_returningFromChart)
                        {
                            _returningFromChart = false;
                            _messageText!.Visible = false;
                        }
                        else
                        {
                            ShowFlightScreen();
                        }
                        break;
                }
            }

            UpdateActiveGameState(deltaSeconds);
        }

        private void UpdateActiveGameState(float deltaSeconds)
        {
            switch (_currentGameState)
            {
                case EliteGameState.LoadCommander:
                    UpdateLoadCommander();
                    break;

                case EliteGameState.LoadCommanderTitle:
                    UpdateLoadCommanderTitle();
                    break;

                case EliteGameState.DefaultCommanderTitle:
                    UpdateDefaultCommanderTitle();
                    break;

                case EliteGameState.Docked:
                    UpdateDocked();
                    break;

                case EliteGameState.Market:
                    UpdateMarket();
                    break;

                case EliteGameState.LaunchSequence:
                    UpdateLaunchSequence(deltaSeconds);
                    break;

                case EliteGameState.Flight:
                    break;
            }
        }

        private void JumpTo(EliteGameState target)
        {
            _nextGameState = target;
            _gameStatePending = true;
        }

        private void StartGame()
        {
            JumpTo(EliteGameState.LoadCommanderTitle);
        }

        private void RestartAfterDeath()
        {
            _hyperspaceCountdown = 0;
            _hyperspaceTicks = 0;
            _hyperspaceKeyWasDown = false;
            _laserCooldown = 0.0f;
            _laserPulseTime = 0.0f;
            _laserTemperature = 0;

            _playerShip.Reset();
            _playerShip.SetInitialSpeed(2.0f);
            ResetGame();

            _pendingFlightMessage = "SHIP DESTROYED - FLIGHT RESET";
            JumpTo(EliteGameState.Flight);
        }

        private void LoadDefaultCommander()
        {
            _commander.Reset();

            _galaxy.CurrentSystemX = _commander.CurrentSystemX;
            _galaxy.CurrentSystemY = _commander.CurrentSystemY;
            _galaxy.SetGalaxySeeds(_commander.GalaxySeeds);

            _galaxy.MoveCrosshairTo(_commander.CurrentSystemX, _commander.CurrentSystemY);
            _galaxy.SelectNearestSystem();

            _commander.CurrentSystemX = _galaxy.CrosshairX;
            _commander.CurrentSystemY = _galaxy.CrosshairY;
            _commander.MarketRandom = _galaxy.SelectedSeeds[2] & 0xFF;

            CommodityMarket.GenerateMarket(_galaxy.SelectedEconomy, _commander.MarketRandom, _commander.MarketAvailability);

            ResetGame();

            JumpTo(EliteGameState.DefaultCommanderTitle);
        }

        private void ShowLoadCommanderTitle()
        {
            _titleText!.Text = "---- E L I T E ----";
            _titleText.Position = new Vector2((WorldWidth - DirectXJollyRenderer.MeasureTextWidth(_titleText.Text, _titleText.FontFamily, _titleText.FontSize, _titleText.Bold, _titleText.UltraBold)) * 0.5f, 20);
            _titleText!.Visible = true;

            _messageText!.Text = "LOAD NEW COMMANDER  (Y/N)?";
            _messageText.Position = new Vector2((WorldWidth - DirectXJollyRenderer.MeasureTextWidth(_messageText.Text, _messageText.FontFamily, _messageText.FontSize, _messageText.Bold, _messageText.UltraBold)) * 0.5f, 400);
            _messageText!.Visible = true;

            _commanderListText!.Visible = false;

            _flightStatusText!.Visible = false;
            _instrumentText!.Visible = false;
            _radarText!.Visible = false;

            _commanderText!.Visible = false;
            _commanderUnderline = null;
            _marketText!.Visible = false;
        }

        private void ShowDefaultCommanderTitle()
        {
            _titleText!.Text = "---- E L I T E ----";
            _titleText.Position = new Vector2((WorldWidth - DirectXJollyRenderer.MeasureTextWidth(_titleText.Text, _titleText.FontFamily, _titleText.FontSize, _titleText.Bold, _titleText.UltraBold)) * 0.5f, 20);
            _titleText.Visible = true;

            _commanderText!.Visible = false;
            _dockedStatusText!.Visible = false;
            _flightStatusText!.Visible = false;
            _instrumentText!.Visible = false;
            _radarText!.Visible = false;
            
            _messageText!.Text = "Press Space Or Fire, Commander.";
            _messageText.Position = new Vector2((WorldWidth - DirectXJollyRenderer.MeasureTextWidth(_messageText.Text, _messageText.FontFamily, _messageText.FontSize, _messageText.Bold, _messageText.UltraBold)) * 0.5f, 400);

            _commanderUnderline = null;
            _commanderListText!.Visible = false;

            _marketText!.Visible = false;
        }

        private void ShowLoadCommander()
        {
            _titleText!.Text = "LOAD COMMANDER";
            _titleText.Position = new Vector2((WorldWidth - DirectXJollyRenderer.MeasureTextWidth(_titleText.Text, _titleText.FontFamily, _titleText.FontSize, _titleText.Bold, _titleText.UltraBold)) * 0.5f, 20);

            _commanderUnderline = new JollyLineSnapshot(new Vector2(60, 60), new Vector2(WorldWidth - 60, 60), JollyColor.Yellow, 2.0f)
            {
                GlowEnabled = false
            };

            _messageText!.Visible = true;
            _messageText.Text = "UP/DOWN SELECT      ENTER LOAD      ESC BACK";
            _messageText.Position = new Vector2(40, 80);

            List<CommanderSave> saves = LoadSaves();
            List<(string Commander, List<CommanderSave> Saves)> commanders = CommanderStore.GroupByCommander(saves);
            List<string> rows = commanders.Select(commander => $"{commander.Commander}  {commander.Saves[0].SavedDateText}").ToList();

            rows.Add("START AGAIN");

            _commanderSelected = Math.Clamp(_commanderSelected, 0, rows.Count - 1);
            _commanderListText!.Visible = true;
            _commanderListText.Position = new Vector2(40, 110);
            _commanderListText.Text = string.Join("\n", rows.Select((row, index) => $"{(index == _commanderSelected ? ">" : " ")} {row}"));

            _marketText!.Visible = false;
        }

        private void ShowDockedScreen()
        {
            _titleText!.Visible = false;
            _messageText!.Visible = false;
            _flightStatusText!.Visible = false;
            _instrumentText!.Visible = false;
            _radarText!.Visible = false;
            _marketText!.Visible = false;

            _commanderText!.Visible = true;
            _commanderText.Text = $"COMMANDER {_commander.Name}";
            _commanderText.Position = new Vector2((WorldWidth - DirectXJollyRenderer.MeasureTextWidth(_commanderText.Text, _commanderText.FontFamily, _commanderText.FontSize, _commanderText.Bold, _commanderText.UltraBold)) * 0.5f, 20);

            _commanderUnderline = new JollyLineSnapshot(new Vector2(60, 50), new Vector2(WorldWidth-60, 50), JollyColor.Yellow, 2.0f)
            {
                GlowEnabled = false
            };

            byte[] galaxySeeds = Array.ConvertAll(_commander.GalaxySeeds, seed => (byte)seed);
            string systemName = CommanderStore.SystemNameAt(galaxySeeds, _commander.CurrentSystemX, _commander.CurrentSystemY);

            _dockedStatusText!.Text =
                $"{"PRESENT SYSTEM",-16}: {systemName}\n" +
                $"{"HYPERSPACE TO",-16}: {systemName}\n" +
                $"{"CONDITION",-16}: DOCKED\n" +
                $"{"FUEL",-16}: {_commander.Fuel}\n" +
                $"{"CASH",-16}: {_commander.Cash:N0} CR\n" +
                $"{"LEGAL STATUS",-16}: {GetLegalStatus(_commander.LegalStatus)}\n" +
                $"{"RATING",-16}: {GetCombatRating(_commander.KillTally)}\n" +
                $"{"EQUIPMENT"}: {GetEquipmentList()}";

            _dockedStatusText.Position = new Vector2(60, 80);
            _dockedStatusText.Visible = true;
        }

        private void ShowMarketScreen()
        {
            _titleText!.Visible = false;
            _messageText!.Visible = false;
            _dockedStatusText!.Visible = false;
            _flightStatusText!.Visible = false;
            _instrumentText!.Visible = false;
            _radarText!.Visible = false;
            _commanderListText!.Visible = false;

            _commanderText!.Visible = true;
            _commanderText.Text = $"GALACTIC MARKET - {_commander.Name}";
            _commanderText.Position = new Vector2(60, 20);

            _marketText!.Visible = true;
            _marketText.Position = new Vector2(35, 63);

            UpdateMarketDisplay();
        }

        private void ShowLaunchSequence()
        {
            _titleText!.Visible = false;
            _commanderText!.Visible = false;
            _dockedStatusText!.Visible = false;
            _commanderListText!.Visible = false;
            _messageText!.Visible = false;

            _flightStatusText!.Visible = false;
            _instrumentText!.Visible = false;
            _radarText!.Visible = false;

            _commanderUnderline = null;
        }

        private void ShowFlightScreen()
        {
            _playerShip.Reset();
            _playerShip.SetInitialSpeed(2.0f);

            _titleText!.Visible = false;
            _messageText!.Visible = !string.IsNullOrWhiteSpace(_pendingFlightMessage);
            if (_messageText.Visible)
            {
                _messageText.Text = _pendingFlightMessage;
            }
            _pendingFlightMessage = string.Empty;
            _commanderListText!.Visible = false;
            _dockedStatusText!.Visible = false;
            _commanderUnderline = null;

            _commanderText!.Visible = true;
            _commanderText.Text = $"COMMANDER {_commander.Name}\nCREDITS {_commander.Cash:N0} CR";

            _flightStatusText!.Visible = false;
            _instrumentText!.Visible = true;
            _radarText!.Visible = true;
            _marketText!.Visible = false;

            _hyperspaceCountdown = 0;
            _hyperspaceKeyWasDown = false;
            
            UpdateFlightInstruments();
        }

        private void StartHyperspace()
        {
            if (_hyperspaceCountdown != 0)
            {
                return;
            }

            if (!_galaxy.TrySelectHyperspaceDestination())
            {
                _messageText!.Text = "NO HYPERSPACE DESTINATION";
                _messageText.Visible = true;
                return;
            }

            int distance = _galaxy.DestinationDistance;

            if (distance > 255 || distance > _commander.Fuel)
            {
                _messageText!.Text = "HYPERSPACE OUT OF RANGE";
                _messageText.Visible = true;
                return;
            }

            _hyperspaceCountdown = 15;
            _hyperspaceTicks = 15;

            _messageText!.Text = "HYPERSPACE 15";
            _messageText.Visible = true;
        }

        private void UpdateLoadCommanderTitle()
        {
            bool y = Input.IsKeyDown(Key.Y);
            bool n = Input.IsKeyDown(Key.N);
            bool keyDown = y || n;

            if (keyDown && !_titleKeyWasDown)
            {
                _titleKeyWasDown = true;

                if (y)
                {
                    _commanderSelected = 0;
                    JumpTo(EliteGameState.LoadCommander);
                }
                else if (n)
                {
                    JumpTo(EliteGameState.LoadDefaultCommander);
                }

                return;
            }

            if (!keyDown)
            {
                _titleKeyWasDown = false;
            }
        }

        private void UpdateLoadCommander()
        {
            List<CommanderSave> saves = LoadSaves();
            List<(string Commander, List<CommanderSave> Saves)> commanders = CommanderStore.GroupByCommander(saves);

            int startAgainIndex = commanders.Count;
            int lastIndex = startAgainIndex;

            bool up = Input.IsKeyDown(Key.Up);
            bool down = Input.IsKeyDown(Key.Down);
            bool enter = Input.IsKeyDown(Key.Enter);
            bool escape = Input.IsKeyDown(Key.Escape);

            bool keyDown = up || down || enter || escape;

            if (keyDown && !_commanderKeyWasDown)
            {
                if (up)
                {
                    _commanderSelected = Math.Max(0, _commanderSelected - 1);
                }
                else if (down)
                {
                    _commanderSelected = Math.Min(lastIndex, _commanderSelected + 1);
                }
                else if (enter)
                {
                    if (_commanderSelected == startAgainIndex)
                    {
                        JumpTo(EliteGameState.LoadDefaultCommander);
                    }
                    else if (_commanderSelected >= 0 && _commanderSelected < commanders.Count)
                    {
                        CommanderSave save = commanders[_commanderSelected].Saves[0];

                        _commander.ApplySavedCommander(save);

                        _galaxy.CurrentSystemX = _commander.CurrentSystemX;
                        _galaxy.CurrentSystemY = _commander.CurrentSystemY;
                        _galaxy.SetGalaxySeeds(_commander.GalaxySeeds);
                        _galaxy.MoveCrosshairTo(_commander.CurrentSystemX, _commander.CurrentSystemY);
                        _galaxy.SelectNearestSystem();

                                    _commander.CurrentSystemX = _galaxy.CrosshairX;
                        _commander.CurrentSystemY = _galaxy.CrosshairY;

                        ResetGame();

                        JumpTo(EliteGameState.DefaultCommanderTitle);
                    }
                }
                else if (escape)
                {
                    JumpTo(EliteGameState.LoadCommanderTitle);
                }

                ShowLoadCommander();
            }

            _commanderKeyWasDown = keyDown;
        }

        private void UpdateDefaultCommanderTitle()
        {
            bool spaceDown = Input.IsKeyDown(Key.Space);

            if (spaceDown && !_spaceWasDown)
            {
                JumpTo(EliteGameState.Docked);
            }

            _spaceWasDown = spaceDown;
        }

        private void UpdateDocked()
        {
            bool f1Down = Input.IsKeyDown(Key.F1);
            bool marketDown = Input.IsKeyDown(Key.M);

            if (f1Down && !_f1WasDown)
            {
                _launchSequenceTime = 0.0f;
                JumpTo(EliteGameState.LaunchSequence);
            }
            else if (marketDown && !_marketOpenWasDown)
            {
                _marketSelected = 0;
                _marketQuantity = 1;
                JumpTo(EliteGameState.Market);
            }
            _f1WasDown = f1Down;
            _marketOpenWasDown = marketDown;
        }

        private void UpdateMarketDisplay()
        {
            int economy = _galaxy.SelectedEconomy;

            List<string> rows =
            [
                "COMMODITY       PRICE    STOCK    CARGO",
                "---------------------------------------"
            ];

            for (int i = 0; i < CommodityCatalogue.Commodities.Count; i++)
            {
                CommodityDefinition commodity = CommodityCatalogue.Commodities[i];

                decimal price = _tradingService.GetPriceCredits(_commander, i, economy);
                int stock = _commander.MarketAvailability[i];
                int cargo = _commander.Cargo[i];

                if (i == _marketSelected)
                {
                    rows.Add($"> {commodity.Name,-13} {price,6:0.0} {stock,7} {cargo,8}");
                }
                else
                {
                    rows.Add($" {commodity.Name,-14} {price,6:0.0} {stock,7} {cargo,8}");
                }
            }

            rows.Add(string.Empty);
            rows.Add($"CASH: {_commander.Cash / 10.0m:0.0} CR");
            rows.Add($"QUANTITY: {_marketQuantity}");
            rows.Add(string.Empty);
            rows.Add("UP/DOWN: SELECT COMMODITY");
            rows.Add("LEFT/RIGHT: CHANGE QUANTITY");
            rows.Add("B: BUY   S: SELL   ESC: RETURN");

            _marketText!.Text = string.Join("\n", rows);
        }

        private void UpdateLaunchSequence(float deltaSeconds)
        {
            _launchSequenceTime += deltaSeconds;

            if (_launchSequenceTime >= LaunchSequenceDuration)
            {
                JumpTo(EliteGameState.Flight);
            }
        }

        private void UpdateFlightInput(float deltaSeconds)
        {
            float yaw = 0.0f;
            float pitch = 0.0f;
            float roll = 0.0f;
            float throttle = 0.0f;

            bool hyperspaceDown = Input.IsKeyDown(Key.H);
            if (hyperspaceDown && !_hyperspaceKeyWasDown)
            {
                StartHyperspace();
            }
            _hyperspaceKeyWasDown = hyperspaceDown;

            if (_hyperspaceCountdown > 0)
            {
                UpdateHyperspaceCountdown();
                return;
            }

            if (Input.IsKeyDown(Key.Left)) yaw = -YawRate;
            if (Input.IsKeyDown(Key.Right)) yaw = YawRate;
            if (Input.IsKeyDown(Key.Up)) pitch = -PitchRate;
            if (Input.IsKeyDown(Key.Down)) pitch = PitchRate;
            if (Input.IsKeyDown(Key.W)) throttle = ThrottleRate;
            if (Input.IsKeyDown(Key.S)) throttle = -ThrottleRate;
            if (Input.IsKeyDown(Key.A)) roll = -RollRate;
            if (Input.IsKeyDown(Key.D)) roll = RollRate;

            _laserCooldown = MathF.Max(0.0f, _laserCooldown - deltaSeconds);
            _laserPulseTime = MathF.Max(0.0f, _laserPulseTime - deltaSeconds);

            if (Input.IsKeyDown(Key.Space) && _laserTemperature < 242 && _laserCooldown <= 0.0f)
            {
                _laserPulseTime = LaserPulseDuration;
                _laserCooldown = LaserCooldownDuration;
                FireLaser();
            }

            _playerShip.Rotate(yaw, pitch, roll, deltaSeconds);
            _playerShip.AdjustSpeed(throttle, deltaSeconds);
            _playerShip.Update(deltaSeconds);

            UpdateFlightInstruments();
        }

        private void UpdateFlightInstruments()
        {
            if (_instrumentText is null)
            {
                return;
            }

            int displayedSpeed = (int)MathF.Round(_playerShip.Speed * 10.0f);
            _instrumentText.Text = $"SPEED  {displayedSpeed:000}\nENERGY  {_playerShip.Energy:0000}\nSHIELDS {_playerShip.ForwardShield:0000}\nLASER  FRONT";
        }

        private void UpdateMarket()
        {
            bool up = Input.IsKeyDown(Key.Up);
            bool down = Input.IsKeyDown(Key.Down);
            bool left = Input.IsKeyDown(Key.Left);
            bool right = Input.IsKeyDown(Key.Right);
            bool buy = Input.IsKeyDown(Key.B);
            bool sell = Input.IsKeyDown(Key.S);
            bool escape = Input.IsKeyDown(Key.Escape);

            if (up && !_marketUpWasDown)
            {
                _marketSelected = (_marketSelected - 1 + CommodityCatalogue.Commodities.Count) % CommodityCatalogue.Commodities.Count;
                UpdateMarketDisplay();
            }

            if (down && !_marketDownWasDown)
            {
                _marketSelected = (_marketSelected + 1) % CommodityCatalogue.Commodities.Count;
                UpdateMarketDisplay();
            }

            if (left && !_marketLeftWasDown)
            {
                _marketQuantity = Math.Max(1, _marketQuantity - 1);
                UpdateMarketDisplay();
            }

            if (right && !_marketRightWasDown)
            {
                _marketQuantity = Math.Min(200, _marketQuantity + 1);
                UpdateMarketDisplay();
            }

            if (buy && !_marketBuyWasDown)
            {
                if (_tradingService.TryBuy(_commander, _marketSelected, _marketQuantity, _galaxy.SelectedEconomy, out string message))
                {
                    UpdateMarketDisplay();
                }
                else
                {
                    _marketText!.Text = $"{message}\n\n{_marketText.Text}";
                }
            }

            if (sell && !_marketSellWasDown)
            {
                if (_tradingService.TrySell(_commander, _marketSelected, _marketQuantity, _galaxy.SelectedEconomy, out string message))
                {
                    UpdateMarketDisplay();
                }
                else
                {
                    _marketText!.Text = $"{message}\n\n{_marketText.Text}";
                }
            }

            if (escape && !_marketEscapeWasDown)
            {
                JumpTo(EliteGameState.Docked);
            }

            _marketUpWasDown = up;
            _marketDownWasDown = down;
            _marketLeftWasDown = left;
            _marketRightWasDown = right;
            _marketBuyWasDown = buy;
            _marketSellWasDown = sell;
            _marketEscapeWasDown = escape;
        }

        private void UpdateHyperspaceCountdown()
        {
            if (_hyperspaceCountdown == 0)
            {
                return;
            }

            _hyperspaceTicks--;

            if (_hyperspaceTicks > 0)
            {
                return;
            }

            _hyperspaceTicks = 5;
            _hyperspaceCountdown--;

            if (_hyperspaceCountdown == 0)
            {
                _messageText!.Text = "HYPERSPACE";
                CompleteHyperspace();
                return;
            }

            _messageText!.Text = $"HYPERSPACE {_hyperspaceCountdown}";
        }

        private void UpdateGalacticChartInput(float deltaSeconds)
        {
            const float crosshairSpeed = 100.0f;

            if (Input.IsKeyDown(Key.Left))
            {
                _chartCrosshairX -= (int)(crosshairSpeed * deltaSeconds);
            }

            if (Input.IsKeyDown(Key.Right))
            {
                _chartCrosshairX += (int)(crosshairSpeed * deltaSeconds);
            }

            if (Input.IsKeyDown(Key.Up))
            {
                _chartCrosshairY -= (int)(crosshairSpeed * deltaSeconds);
            }

            if (Input.IsKeyDown(Key.Down))
            {
                _chartCrosshairY += (int)(crosshairSpeed * deltaSeconds);
            }

            _chartCrosshairX = Math.Clamp(_chartCrosshairX, 0, 255);
            _chartCrosshairY = Math.Clamp(_chartCrosshairY, 0, 255);

            _galaxy.MoveCrosshairTo(_chartCrosshairX, _chartCrosshairY);
        }

private void CompleteHyperspace()
        {
            int distance = _galaxy.DestinationDistance;

            if (distance <= 0 || distance > _commander.Fuel)
            {
                _hyperspaceCountdown = 0;
                _pendingFlightMessage = "HYPERSPACE FAILED";
                JumpTo(EliteGameState.Flight);
                return;
            }

            _commander.Fuel -= distance;
            _commander.CurrentSystemX = _galaxy.DestinationX;
            _commander.CurrentSystemY = _galaxy.DestinationY;
            _galaxy.ArriveAtDestination();
            _commander.MarketRandom = _galaxy.SelectedSeeds[2] & 0xFF;
            CommodityMarket.GenerateMarket(_galaxy.SelectedEconomy, _commander.MarketRandom, _commander.MarketAvailability);

            ResetGame();
            _pendingFlightMessage = $"ARRIVED AT {_galaxy.SelectedSystemName}";
            JumpTo(EliteGameState.Flight);
        }

        private List<CommanderSave> LoadSaves()
        {
            try
            {
                //_commanderStore.EnsureSampleSaves();
                return _commanderStore!.LoadAll();
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                return [];
            }
        }

        private bool CanDockAtStation()
        {
            if (_playerShip.Speed > 1.0f)
            {
                return false;
            }

            foreach (WorldObject worldObject in _worldObjects)
            {
                if (worldObject.Model is not (CoriolisStationModel or DodecahedronStationModel))
                {
                    continue;
                }

                if (Vector3.DistanceSquared(_playerShip.Position, worldObject.Position) <= 125.0f * 125.0f)
                {
                    return true;
                }
            }

            return false;
        }

        private void FireLaser()
        {
            Matrix4x4 viewMatrix = _worldRenderer.CreateViewMatrix(_playerShip);
            Vector2 screenCenter = new(WorldWidth * 0.5f, FlightViewportHeight * 0.5f);

            if (_flightSimulation.FireLaser(_worldObjects, _playerShip, viewMatrix, screenCenter))
            {
                _commander.KillTally++;
            }

            _laserTemperature = Math.Min(255, _laserTemperature + 8);
        }

        private void DrawShip(List<JollyLineSnapshot> lines, List<JollyTriangleSnapshot> triangles, ShipModel model, Vector2 position, float scale, float rotationY, float rotationX)
        {
            _shipRenderer.Draw(lines, triangles, model, position, scale, rotationY, rotationX);
        }

        private void DrawLaunchTunnel(List<JollyLineSnapshot> lines)
        {
            Vector2 centre = new(WorldWidth * 0.5f, FlightViewportHeight * 0.5f);
            float phase = (_launchSequenceTime * 5.0f) % 1.0f;

            for (int ring = 0; ring < LaunchTunnelRingCount; ring++)
            {
                float depth = (ring + phase) / LaunchTunnelRingCount;
                float radius = 12.0f + depth * depth * WorldHeight * 0.75f;

                Vector2 previous = Vector2.Zero;

                for (int side = 0; side <= LaunchTunnelSides; side++)
                {
                    float angle = side * (MathF.Tau / LaunchTunnelSides);
                    Vector2 point = centre + new Vector2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);

                    if (side > 0)
                    {
                        lines.Add(new JollyLineSnapshot(previous, point, JollyColor.White, 1.0f)
                        {
                            GlowEnabled = false
                        });
                    }

                    previous = point;
                }
            }
        }

        private void CreateBorder()
        {
            ArgumentNullException.ThrowIfNull(_renderer);
            ArgumentNullException.ThrowIfNull(_assets);

            JollyTexture borderTexture = _assets.LoadTexture("TopBorder.png");
            float topHeight = borderTexture.Height;
            JollySprite sprite;
            sprite = _renderer.CreateSprite(borderTexture);
            sprite.Position = new JollyVector2(WorldWidth * 0.5f, borderTexture.Height * 0.5f);
            sprite.Layer = 0;
            _borderSprites.Add(sprite);

            borderTexture = _assets.LoadTexture("LeftBorder.png");
            sprite = _renderer.CreateSprite(borderTexture);
            sprite.Position = new JollyVector2(borderTexture.Width * 0.5f, topHeight + borderTexture.Height * 0.5f);
            sprite.Layer = 0;
            _borderSprites.Add(sprite);

            borderTexture = _assets.LoadTexture("RightBorder.png");
            sprite = _renderer.CreateSprite(borderTexture);
            sprite.Position = new JollyVector2(WorldWidth - borderTexture.Width * 0.5f, topHeight + borderTexture.Height * 0.5f);
            sprite.Layer = 0;
            _borderSprites.Add(sprite);
        }

        private void DrawGalacticChart(List<JollyLineSnapshot> lines, List<JollyTriangleSnapshot> triangles)
        {
            GalaxyChartSystem[] systems = _galaxy.GetChartSystems();

            float scaleX = (float)WorldWidth / 256.0f;
            float scaleY = (float)FlightViewportHeight / 256.0f;

            foreach (GalaxyChartSystem system in systems)
            {
                float x = system.X * scaleX;
                float y = system.Y * scaleY;

                lines.Add(new JollyLineSnapshot(new Vector2(x - 1.5f, y), new Vector2(x + 1.5f, y), new JollyColor(204, 204, 204, 255), 1.0f));
            }

            float currentX = _galaxy.CurrentSystemX * scaleX;
            float currentY = _galaxy.CurrentSystemY * scaleY;

            DrawChartMarker(lines, currentX, currentY, 5.0f, new JollyColor(0, 255, 255, 255));

            _galaxy.SelectNearestSystemAtCrosshair();
            _messageText!.Text = $"GALACTIC CHART  {_galaxy.SelectedSystemName}  DISTANCE {_galaxy.SelectedDistance} LY   F5: FLIGHT";
            _messageText.Visible = true;

            float selectedX = _galaxy.SelectedSeeds[3] * scaleX;
            float selectedY = _galaxy.SelectedSeeds[1] * scaleY;

            DrawChartMarker(lines, selectedX, selectedY, 4.0f, new JollyColor(255, 255, 0, 255));

            float crosshairX = _chartCrosshairX * scaleX;
            float crosshairY = _chartCrosshairY * scaleY;

            DrawChartMarker(lines, crosshairX, crosshairY, 7.0f, new JollyColor(255, 0, 0, 255));
        }

        private static void DrawChartMarker(List<JollyLineSnapshot> lines, float x, float y, float size, JollyColor color)
        {
            lines.Add(new JollyLineSnapshot(new Vector2(x - size, y), new Vector2(x + size, y), color, 1.0f));
            lines.Add(new JollyLineSnapshot(new Vector2(x, y - size), new Vector2(x, y + size), color, 1.0f));
        }

        private void CreateCockpit()
        {
            ArgumentNullException.ThrowIfNull(_renderer);
            ArgumentNullException.ThrowIfNull(_assets);

            _cockpitDashboard = new CockpitDashboard(_renderer, _assets, WorldWidth, WorldHeight, CockpitHeight);
            _cockpitDashboard.Initialise();     
        }
        
        private void CreateUI()
        {
            float left = _renderer.Camera.Position.X - WorldWidth * 0.5f;
            float top = _renderer.Camera.Position.Y - WorldHeight * 0.5f;

            _titleText = new JollyUIText
            {
                Text = "---- E L I T E ----",
                FontFamily = "Arial",
                FontSize = 32,
                Color = JollyColor.White,
                Bold = true,
                Position = new Vector2(0, 20),
                Layer = 10
            };
            _titleText.Position = new Vector2((WorldWidth - DirectXJollyRenderer.MeasureTextWidth(_titleText.Text, _titleText.FontFamily, _titleText.FontSize, _titleText.Bold, _titleText.UltraBold)) * 0.5f, 20);

            _messageText = new JollyUIText
            {
                Text = "LOAD NEW COMMANDER  (Y/N)?",
                Position = new Vector2(80, 90),
                FontFamily = "Arial",
                FontSize = 18,
                Color = JollyColor.White,
                Bold = true,
                Layer = 10
            };
            _messageText.Position = new Vector2((WorldWidth - DirectXJollyRenderer.MeasureTextWidth(_messageText.Text, _messageText.FontFamily, _messageText.FontSize, _messageText.Bold, _messageText.UltraBold)) * 0.5f, 400);

            _commanderText = new JollyUIText
            {
                Text = "COMMANDER: ",
                FontFamily = "Arial",
                FontSize = 22,
                Color = JollyColor.White,
                Bold = true,
                Layer = 10
            };

            _commanderListText = new JollyUIText
            {
                Text = string.Empty,
                Position = new Vector2(40, 110),
                FontFamily = "Consolas",
                FontSize = 16,
                Color = JollyColor.White,
                Layer = 10,
                Bold = true,
                Visible = false
            };

            _dockedStatusText = new JollyUIText
            {
                Text = string.Empty,
                FontFamily = "Consolas",
                FontSize = 16,
                Color = JollyColor.White,
                Position = new Vector2(60, 70),
                Bold = true,
                Layer = 10
            };
            
            _flightStatusText = new JollyUIText
            {
                Text = "---- ELITE ----",
                Position = new Vector2(left + WorldWidth * 0.5f - 260.0f, top + WorldHeight * 0.5f - 120.0f),
                FontFamily = "Arial",
                FontSize = 28,
                Color = new JollyColor(255, 255, 255, 255),
                UltraBold = true,
                Layer = 10
            };

            _instrumentText = new JollyUIText
            {
                Text = "SPEED  0\nENERGY  EEEE\nSHIELDS EEEE\nLASER  FRONT",
                Position = new Vector2(10, 150),
                FontFamily = "Consolas",
                FontSize = 16,
                Color = new JollyColor(0, 255, 0, 255),
                Layer = 10
            };

            _radarText = new JollyUIText
            {
                Text = "RADAR\n\n\n   +",
                Position = new Vector2(240, 145),
                FontFamily = "Consolas",
                FontSize = 10,
                Color = new JollyColor(0, 255, 0, 255),
                Layer = 10
            };

            _marketText = new JollyUIText
            {
                Text = string.Empty,
                Position = new Vector2(35, 65),
                FontFamily = "Consolas",
                FontSize = 12,
                Color = JollyColor.White,
                Layer = 10,
                UltraBold = true,
                Visible = false
            };


            _uiManager!.Add(_titleText);
            _uiManager!.Add(_messageText);
            _uiManager.Add(_commanderText);
            _uiManager.Add(_dockedStatusText);

            _uiManager!.Add(_flightStatusText);
            _uiManager.Add(_instrumentText);
            _uiManager.Add(_radarText);
            _uiManager.Add(_commanderListText);
            _uiManager.Add(_marketText);
        }

        private static string GetCombatRating(int kills)
        {
            return kills switch
            {
                < 8 => "HARMLESS",
                < 16 => "MOSTLY HARMLESS",
                < 32 => "POOR",
                < 64 => "AVERAGE",
                < 128 => "ABOVE AVERAGE",
                < 512 => "COMPETENT",
                < 2560 => "DANGEROUS",
                < 6400 => "DEADLY",
                _ => "ELITE"
            };
        }

        private static string GetLegalStatus(int legalStatus)
        {
            return legalStatus switch
            {
                0 => "CLEAN",
                < 50 => "OFFENDER",
                _ => "FUGITIVE"
            };
        }

        private string GetEquipmentList()
        {
            List<string> equipment = [];

            if (_commander.Lasers[0] > 0) equipment.Add("\n     FRONT LASER");
            if (_commander.Lasers[1] > 0) equipment.Add("\n     REAR LASER");
            if (_commander.Lasers[2] > 0) equipment.Add("\n     LEFT LASER");
            if (_commander.Lasers[3] > 0) equipment.Add("\n     RIGHT LASER");

            if (_commander.Ecm > 0) equipment.Add("\n     ECM");
            if (_commander.FuelScoops > 0) equipment.Add("\n     FUEL SCOOPS");
            if (_commander.EnergyBomb > 0) equipment.Add("\n     ENERGY BOMB");
            if (_commander.EnergyUnit > 0) equipment.Add("\n     ENERGY UNIT");
            if (_commander.DockingComputer > 0) equipment.Add("\n     DOCKING COMPUTER");
            if (_commander.GalacticHyperdrive > 0) equipment.Add("\n     GALACTIC HYPERDRIVE");
            if (_commander.EscapePod > 0) equipment.Add("\n     ESCAPE POD");

            return equipment.Count > 0 ? string.Join(", ", equipment) : "\n     NONE";
        }

        protected override void OnShutdown()
        {
            _gameView.RenderSizeChanged -= GameView_RenderSizeChanged;

            _uiRenderer?.Clear();
            _uiRenderer = null;

            _uiManager?.Clear();
            _uiManager = null;

            _assets?.Dispose();
            _assets = null;

            _renderer = null;
        }

    }
}

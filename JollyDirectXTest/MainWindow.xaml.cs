using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Jolly2DEngine.Rendering.DirectX;

namespace JollyDirectXTestCtrl
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _statisticsTimer;
        private MyGame? _game;

        public MainWindow()
        {
            InitializeComponent();

            _statisticsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _statisticsTimer.Tick += StatisticsTimer_Tick;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;

            DirectXControl.MouseScreenPositionChanged += DirectXControl_MouseScreenPositionChanged;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_game != null)
            {
                return;
            }

            await DirectXControl.WaitForReadyAsync();

            _game = new MyGame(DirectXControl);
            _game.Input.Attach(this);
            _game.Start();

            _statisticsTimer.Start();
        }

        private void StatisticsTimer_Tick(object? sender, EventArgs e)
        {
            if (_game == null)
            {
                return;
            }

            FpsText.Text = $"Render FPS: {DirectXControl.FPS:F1}";
            FrameTimeText.Text = $"Render Time: {DirectXControl.FrameTime:F2} ms";

            GameFpsText.Text = $"Game Loop FPS: {_game.GameLoopFPS:F1}";
            GameFrameTimeText.Text = $"Game Time: {_game.GameFrameTime:F2} ms";

            PlayerPositionText.Text = $"Position: {_game.PlayerX:F1}, {_game.PlayerY:F1}";
            PlayerVelocityText.Text = $"Velocity: {_game.PlayerVelocityX:F1}, {_game.PlayerVelocityY:F1}";
            PlayerGroundedText.Text = $"Grounded: {_game.PlayerGrounded}";
            PhysicsObjectsText.Text = $"Physics Objects: {_game.PhysicsObjectCount}";

            WorldPositionText.Text = $"Camera: {_game.CameraWorldX:F1}, {_game.CameraWorldY:F1}\nZoom: {_game.CameraZoom:F2}\nRotation: {_game.CameraRotationDegrees:F1}°";

            PhysicsDebugText.Text = $"Debug: {(_game.PhysicsDebugEnabled ? "ON" : "OFF")} (F3)";
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            _statisticsTimer.Stop();

            if (_game != null)
            {
                _game.Input.Detach(this);
                _game.Stop();
                ((IDisposable)_game).Dispose();
                _game = null;
            }
        }

        private void DirectXControl_MouseScreenPositionChanged(object? sender, System.Windows.Point position)
        {
            if (_game == null)
            {
                return;
            }

            Vector2 worldPosition = _game.ScreenToWorld((float)position.X, (float)position.Y);

            MouseWorldXText.Text = $"World X: {worldPosition.X:F1}";
            MouseWorldYText.Text = $"World Y: {worldPosition.Y:F1}";
        }

        private void ApplyCameraButton_Click(object sender, RoutedEventArgs e)
        {
            if (_game == null)
            {
                return;
            }

            if (!float.TryParse(CameraXTextBox.Text, out float x))
            {
                return;
            }

            if (!float.TryParse(CameraYTextBox.Text, out float y))
            {
                return;
            }

            if (!float.TryParse(CameraZoomTextBox.Text, out float zoom))
            {
                return;
            }

            if (!float.TryParse(CameraRotationTextBox.Text, out float rotationDegrees))
            {
                return;
            }

            _game.SetCameraPosition(x, y, zoom, rotationDegrees);
        }

        private void ReadCameraButton_Click(object sender, RoutedEventArgs e)
        {
            if (_game == null)
            {
                return;
            }

            CameraXTextBox.Text = _game.CameraWorldX.ToString("F1");
            CameraYTextBox.Text = _game.CameraWorldY.ToString("F1");
            CameraZoomTextBox.Text = _game.CameraZoom.ToString("F2");
            CameraRotationTextBox.Text = _game.CameraRotationDegrees.ToString("F1");
        }
    }
}
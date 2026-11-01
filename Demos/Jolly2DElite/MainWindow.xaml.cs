using Jolly2DElite.Game;
using System.Windows;

namespace Jolly2DElite
{
    public partial class MainWindow : Window
    {
        private EliteGame? _game;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
            GameView.SizeChanged += AnimationView_SizeChanged;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_game is not null)
            {
                return;
            }

            await GameView.WaitForReadyAsync();

            _game = new EliteGame(GameView);
            _game.Input.Attach(this);
            _game.Start();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if (_game != null)
            {
                _game.Input.Detach(this);
                _game.Stop();
                ((IDisposable)_game).Dispose();
                _game = null;
            }
        }

        private void AnimationView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // The DirectX control should resize through its existing
            // WindowsFormsHost and panel resize handling.
        }
    }
}

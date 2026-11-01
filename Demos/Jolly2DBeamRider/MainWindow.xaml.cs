using System.Windows;
using Jolly2DBeamRider.Game;

namespace Jolly2DBeamRider
{
    public partial class MainWindow : Window
    {
        private BeamRiderGame? _game;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_game != null)
            {
                return;
            }

            await GameView.WaitForReadyAsync();

            _game = new BeamRiderGame(GameView);
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
    }
}
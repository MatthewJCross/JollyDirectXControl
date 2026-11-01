using Jolly2DEngine.Input;
using System.Windows;
using System.Windows.Input;

namespace Jolly2DEngine.Input
{
    public sealed class JollyKeyboard
    {
        private readonly object _lock = new();
        private readonly HashSet<Key> _keysDown = new();
        private readonly HashSet<Key> _keysPressed = new();
        private readonly HashSet<Key> _keysReleased = new();

        public void Attach(Window window)
        {
            ArgumentNullException.ThrowIfNull(window);

            window.KeyDown += OnKeyDown;
            window.KeyUp += OnKeyUp;
            window.PreviewKeyDown += OnPreviewKeyDown;
            window.PreviewKeyUp += OnPreviewKeyUp;
        }

        public void Detach(Window window)
        {
            ArgumentNullException.ThrowIfNull(window);

            window.KeyDown -= OnKeyDown;
            window.KeyUp -= OnKeyUp;
            window.PreviewKeyDown -= OnPreviewKeyDown;
            window.PreviewKeyUp -= OnPreviewKeyUp;
        }

        public bool IsKeyDown(Key key)
        {
            lock (_lock)
            {
                return _keysDown.Contains(key);
            }
        }

        public bool IsKeyPressed(Key key)
        {
            lock (_lock)
            {
                return _keysPressed.Contains(key);
            }
        }

        public bool IsKeyReleased(Key key)
        {
            lock (_lock)
            {
                return _keysReleased.Contains(key);
            }
        }

        internal void EndFrame()
        {
            lock (_lock)
            {
                _keysPressed.Clear();
                _keysReleased.Clear();
            }
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HandleKeyDown(e.Key);
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HandleKeyDown(e.Key);
        }

        private void OnPreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HandleKeyUp(e.Key);
        }

        private void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HandleKeyUp(e.Key);
        }

        private void HandleKeyDown(Key key)
        {
            lock (_lock)
            {
                if (_keysDown.Add(key))
                {
                    _keysPressed.Add(key);
                }
            }
        }

        private void HandleKeyUp(Key key)
        {
            lock (_lock)
            {
                _keysDown.Remove(key);
                _keysReleased.Add(key);
            }
        }
    }
}

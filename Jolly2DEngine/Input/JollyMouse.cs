using System.Windows;
using System.Windows.Input;

namespace Jolly2DEngine.Input
{
    public enum JollyMouseButton
    {
        Left,
        Middle,
        Right,
        X1,
        X2
    }

    public sealed class JollyMouse
    {
        private readonly object _lock = new();
        private readonly HashSet<JollyMouseButton> _buttonsDown = new();
        private readonly HashSet<JollyMouseButton> _buttonsPressed = new();
        private readonly HashSet<JollyMouseButton> _buttonsReleased = new();

        private double _x;
        private double _y;
        private int _wheel;

        public double X
        {
            get
            {
                lock (_lock)
                {
                    return _x;
                }
            }
        }

        public double Y
        {
            get
            {
                lock (_lock)
                {
                    return _y;
                }
            }
        }

        public System.Windows.Point Position
        {
            get
            {
                lock (_lock)
                {
                    return new System.Windows.Point(_x, _y);
                }
            }
        }

        public int Wheel
        {
            get
            {
                lock (_lock)
                {
                    return _wheel;
                }
            }
        }

        public void Attach(Window window)
        {
            ArgumentNullException.ThrowIfNull(window);

            window.MouseMove += OnMouseMove;
            window.MouseDown += OnMouseDown;
            window.MouseUp += OnMouseUp;
            window.MouseWheel += OnMouseWheel;
        }

        public void Detach(Window window)
        {
            ArgumentNullException.ThrowIfNull(window);

            window.MouseMove -= OnMouseMove;
            window.MouseDown -= OnMouseDown;
            window.MouseUp -= OnMouseUp;
            window.MouseWheel -= OnMouseWheel;
        }

        public bool IsButtonDown(JollyMouseButton button)
        {
            lock (_lock)
            {
                return _buttonsDown.Contains(button);
            }
        }

        public bool IsButtonPressed(JollyMouseButton button)
        {
            lock (_lock)
            {
                return _buttonsPressed.Contains(button);
            }
        }

        public bool IsButtonReleased(JollyMouseButton button)
        {
            lock (_lock)
            {
                return _buttonsReleased.Contains(button);
            }
        }

        internal void EndFrame()
        {
            lock (_lock)
            {
                _buttonsPressed.Clear();
                _buttonsReleased.Clear();
                _wheel = 0;
            }
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition((IInputElement)sender);

            lock (_lock)
            {
                _x = position.X;
                _y = position.Y;
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            JollyMouseButton? button = GetMouseButton(e.ChangedButton);

            if (button == null)
            {
                return;
            }

            lock (_lock)
            {
                if (_buttonsDown.Add(button.Value))
                {
                    _buttonsPressed.Add(button.Value);
                }
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            JollyMouseButton? button = GetMouseButton(e.ChangedButton);

            if (button == null)
            {
                return;
            }

            lock (_lock)
            {
                _buttonsDown.Remove(button.Value);
                _buttonsReleased.Add(button.Value);
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lock (_lock)
            {
                _wheel += e.Delta;
            }
        }

        private static JollyMouseButton? GetMouseButton(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => JollyMouseButton.Left,
                MouseButton.Middle => JollyMouseButton.Middle,
                MouseButton.Right => JollyMouseButton.Right,
                MouseButton.XButton1 => JollyMouseButton.X1,
                MouseButton.XButton2 => JollyMouseButton.X2,
                _ => null
            };
        }
    }
}

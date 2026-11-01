using Jolly2DEngine.Rendering.Camera;
using System.Numerics;
using System.Windows;
using System.Windows.Input;

namespace Jolly2DEngine.Input
{
    public sealed class JollyInput
    {
        private readonly JollyKeyboard _keyboard = new();
        private readonly JollyMouse _mouse = new();
        private readonly JollyGamepad[] _gamepads =
        [
            new JollyGamepad(0),
            new JollyGamepad(1),
            new JollyGamepad(2),
            new JollyGamepad(3)
        ];

        public JollyKeyboard Keyboard => _keyboard;
        public JollyMouse Mouse => _mouse;
        public IReadOnlyList<JollyGamepad> Gamepads => _gamepads;

        public void Attach(Window window)
        {
            ArgumentNullException.ThrowIfNull(window);

            _keyboard.Attach(window);
            _mouse.Attach(window);
        }

        public void Detach(Window window)
        {
            ArgumentNullException.ThrowIfNull(window);

            _keyboard.Detach(window);
            _mouse.Detach(window);
        }

        public bool IsKeyDown(Key key)
        {
            return _keyboard.IsKeyDown(key);
        }

        public bool IsKeyPressed(Key key)
        {
            return _keyboard.IsKeyPressed(key);
        }

        public bool IsKeyReleased(Key key)
        {
            return _keyboard.IsKeyReleased(key);
        }

        public double MouseX => _mouse.X;

        public double MouseY => _mouse.Y;

        public System.Windows.Point MousePosition => _mouse.Position;

        public int MouseWheel => _mouse.Wheel;

        public bool IsMouseButtonDown(JollyMouseButton button)
        {
            return _mouse.IsButtonDown(button);
        }

        public bool IsMouseButtonPressed(JollyMouseButton button)
        {
            return _mouse.IsButtonPressed(button);
        }

        public bool IsMouseButtonReleased(JollyMouseButton button)
        {
            return _mouse.IsButtonReleased(button);
        }

        internal void Update()
        {
            for (int i = 0; i < _gamepads.Length; i++)
            {
                _gamepads[i].Update();
            }
        }

        internal void EndFrame()
        {
            _keyboard.EndFrame();
            _mouse.EndFrame();

            for (int i = 0; i < _gamepads.Length; i++)
            {
                _gamepads[i].EndFrame();
            }
        }

        public Vector2 GetMouseWorldPosition(Camera2D camera)
        {
            ArgumentNullException.ThrowIfNull(camera);
            System.Windows.Point mouse = _mouse.Position;

            return camera.ScreenToWorld(new Vector2((float)mouse.X, (float)mouse.Y));
        }
    }
}


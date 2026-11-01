using System.Runtime.InteropServices;

namespace Jolly2DEngine.Input
{
    public sealed class JollyGamepad
    {
        private const float LeftStickDeadZone = 0.24f;
        private const float RightStickDeadZone = 0.24f;
        private const float TriggerDeadZone = 0.05f;

        private readonly object _lock = new();
        private readonly HashSet<JollyGamepadButton> _buttonsDown = new();
        private readonly HashSet<JollyGamepadButton> _buttonsPressed = new();
        private readonly HashSet<JollyGamepadButton> _buttonsReleased = new();

        private bool _isConnected;

        private float _leftStickX;
        private float _leftStickY;
        private float _rightStickX;
        private float _rightStickY;
        private float _leftTrigger;
        private float _rightTrigger;

        public int Index { get; }

        public bool IsConnected
        {
            get
            {
                lock (_lock)
                {
                    return _isConnected;
                }
            }
        }

        public float LeftStickX
        {
            get
            {
                lock (_lock)
                {
                    return _leftStickX;
                }
            }
        }

        public float LeftStickY
        {
            get
            {
                lock (_lock)
                {
                    return _leftStickY;
                }
            }
        }

        public float RightStickX
        {
            get
            {
                lock (_lock)
                {
                    return _rightStickX;
                }
            }
        }

        public float RightStickY
        {
            get
            {
                lock (_lock)
                {
                    return _rightStickY;
                }
            }
        }

        public float LeftTrigger
        {
            get
            {
                lock (_lock)
                {
                    return _leftTrigger;
                }
            }
        }

        public float RightTrigger
        {
            get
            {
                lock (_lock)
                {
                    return _rightTrigger;
                }
            }
        }

        public JollyGamepad(int index)
        {
            if (index < 0 || index >= 4)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Index = index;
        }

        public bool IsButtonDown(JollyGamepadButton button)
        {
            lock (_lock)
            {
                return _buttonsDown.Contains(button);
            }
        }

        public bool IsButtonPressed(JollyGamepadButton button)
        {
            lock (_lock)
            {
                return _buttonsPressed.Contains(button);
            }
        }

        public bool IsButtonReleased(JollyGamepadButton button)
        {
            lock (_lock)
            {
                return _buttonsReleased.Contains(button);
            }
        }

        internal void Update()
        {
            lock (_lock)
            {
                int result = XInputGetState((uint)Index, out XInputState state);

                if (result != 0)
                {
                    Disconnect();
                    return;
                }

                _isConnected = true;

                _leftStickX = ApplyDeadZone(state.Gamepad.ThumbLX, LeftStickDeadZone);
                _leftStickY = ApplyDeadZone(state.Gamepad.ThumbLY, LeftStickDeadZone);
                _rightStickX = ApplyDeadZone(state.Gamepad.ThumbRX, RightStickDeadZone);
                _rightStickY = ApplyDeadZone(state.Gamepad.ThumbRY, RightStickDeadZone);
                _leftTrigger = ApplyTriggerDeadZone(state.Gamepad.LeftTrigger);
                _rightTrigger = ApplyTriggerDeadZone(state.Gamepad.RightTrigger);

                XInputButtons buttons = (XInputButtons)state.Gamepad.Buttons;

                foreach (JollyGamepadButton button in Enum.GetValues<JollyGamepadButton>())
                {
                    bool isDown = IsXInputButtonDown(buttons, button);

                    if (isDown)
                    {
                        if (_buttonsDown.Add(button))
                        {
                            _buttonsPressed.Add(button);
                        }
                    }
                    else
                    {
                        if (_buttonsDown.Remove(button))
                        {
                            _buttonsReleased.Add(button);
                        }
                    }
                }
            }
        }

        internal void EndFrame()
        {
            lock (_lock)
            {
                _buttonsPressed.Clear();
                _buttonsReleased.Clear();
            }
        }

        private void Disconnect()
        {
            _isConnected = false;

            _leftStickX = 0.0f;
            _leftStickY = 0.0f;
            _rightStickX = 0.0f;
            _rightStickY = 0.0f;
            _leftTrigger = 0.0f;
            _rightTrigger = 0.0f;

            _buttonsDown.Clear();
            _buttonsPressed.Clear();
            _buttonsReleased.Clear();
        }

        private static bool IsXInputButtonDown(XInputButtons buttons, JollyGamepadButton button)
        {
            XInputButtons mask = button switch
            {
                JollyGamepadButton.A => XInputButtons.A,
                JollyGamepadButton.B => XInputButtons.B,
                JollyGamepadButton.X => XInputButtons.X,
                JollyGamepadButton.Y => XInputButtons.Y,
                JollyGamepadButton.Back => XInputButtons.Back,
                JollyGamepadButton.Start => XInputButtons.Start,
                JollyGamepadButton.LeftShoulder => XInputButtons.LeftShoulder,
                JollyGamepadButton.RightShoulder => XInputButtons.RightShoulder,
                JollyGamepadButton.LeftStick => XInputButtons.LeftStick,
                JollyGamepadButton.RightStick => XInputButtons.RightStick,
                JollyGamepadButton.DPadUp => XInputButtons.DPadUp,
                JollyGamepadButton.DPadDown => XInputButtons.DPadDown,
                JollyGamepadButton.DPadLeft => XInputButtons.DPadLeft,
                JollyGamepadButton.DPadRight => XInputButtons.DPadRight,
                _ => 0
            };

            return (buttons & mask) != 0;
        }

        private static float ApplyDeadZone(short value, float deadZone)
        {
            float normalized = value < 0 ? value / 32768.0f : value / 32767.0f;
            float magnitude = MathF.Abs(normalized);

            if (magnitude <= deadZone)
            {
                return 0.0f;
            }

            float sign = MathF.Sign(normalized);
            float adjusted = (magnitude - deadZone) / (1.0f - deadZone);

            return sign * MathF.Min(adjusted, 1.0f);
        }

        private static float ApplyTriggerDeadZone(byte value)
        {
            float normalized = value / 255.0f;

            if (normalized <= TriggerDeadZone)
            {
                return 0.0f;
            }

            return (normalized - TriggerDeadZone) / (1.0f - TriggerDeadZone);
        }

        [DllImport("xinput1_4.dll")]
        private static extern int XInputGetState(uint dwUserIndex, out XInputState pState);

        [StructLayout(LayoutKind.Sequential)]
        private struct XInputState
        {
            public uint PacketNumber;
            public XInputGamepad Gamepad;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XInputGamepad
        {
            public ushort Buttons;
            public byte LeftTrigger;
            public byte RightTrigger;
            public short ThumbLX;
            public short ThumbLY;
            public short ThumbRX;
            public short ThumbRY;
        }

        [Flags]
        private enum XInputButtons : ushort
        {
            DPadUp = 0x0001,
            DPadDown = 0x0002,
            DPadLeft = 0x0004,
            DPadRight = 0x0008,
            Start = 0x0010,
            Back = 0x0020,
            LeftStick = 0x0040,
            RightStick = 0x0080,
            LeftShoulder = 0x0100,
            RightShoulder = 0x0200,
            A = 0x1000,
            B = 0x2000,
            X = 0x4000,
            Y = 0x8000
        }
    }
}


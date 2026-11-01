using Jolly2DBeamRider.Game;
using Jolly2DEngine.Audio;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Input;
using Jolly2DEngine.Math;
using System.Numerics;

public sealed class BeamRiderPlayer : IDisposable
{
    private const float PlayerY = 750.0f;
    private const float NoseOffset = -25.0f;
    private const float MovementTilt = 0.30f;
    private const float RotationSpeed = 10.0f;

    private const float LaneMovementSpeed = 3.0f;
    private const float KeyRepeatDelay = 0.18f;
    private const float KeyRepeatRate = 0.10f;

    private const float HaloRadiusX = 33.0f;
    private const float HaloRadiusY = 11.0f;
    private const float HaloRotationSpeed = 20.0f;

    private float _leftRepeatTimer;
    private float _rightRepeatTimer;
    private bool _leftWasDown;
    private bool _rightWasDown;

    private bool _haloDetached;

    private readonly JollyColor _haloColor = JollyColor.FromBytes(220, 220, 220);

    private bool _fireWasDown;
    private bool _fireQueued;
    private bool _fireReady;
    private int _fireBeamLane = -1;

    private readonly List<JollyLineSnapshot> _lines = [];
    private readonly BeamRiderArena _arena;

    private float _lane;
    private float _targetLane;
    private float _rotation;
    private float _targetRotation;
    private float _haloRotation;
    private bool _disposed;

    public IReadOnlyList<JollyLineSnapshot> Lines => _lines;

    public BeamRiderPlayer(BeamRiderArena arena)
    {
        _arena = arena;
    }

    public void Create()
    {
        _lane = 0.0f;
        _targetLane = 0.0f;
        _rotation = _arena.GetBeamRotation(_lane);
        _targetRotation = _rotation;

        _fireQueued = false;
        _fireReady = false;
        _fireBeamLane = -1;

        RebuildLines();
    }

    public void Update(JollyInput input, float deltaTime)
    {
        if (_disposed)
        {
            return;
        }

        bool leftDown = input.IsKeyDown(System.Windows.Input.Key.Left) || input.IsKeyDown(System.Windows.Input.Key.A);
        bool rightDown = input.IsKeyDown(System.Windows.Input.Key.Right) || input.IsKeyDown(System.Windows.Input.Key.D);
        bool fireDown = input.IsKeyDown(System.Windows.Input.Key.Space);

        if (leftDown)
        {
            if (!_leftWasDown)
            {
                _targetLane = MathF.Max(-2.0f, _targetLane - 1.0f);
                _leftRepeatTimer = KeyRepeatDelay;
            }
            else
            {
                _leftRepeatTimer -= deltaTime;

                while (_leftRepeatTimer <= 0.0f)
                {
                    _targetLane = MathF.Max(-2.0f, _targetLane - 1.0f);
                    _leftRepeatTimer += KeyRepeatRate;

                    if (_targetLane <= -2.0f)
                    {
                        _leftRepeatTimer = KeyRepeatRate;
                        break;
                    }
                }
            }
        }
        else
        {
            _leftRepeatTimer = 0.0f;
        }

        if (rightDown)
        {
            if (!_rightWasDown)
            {
                _targetLane = MathF.Min(2.0f, _targetLane + 1.0f);
                _rightRepeatTimer = KeyRepeatDelay;
            }
            else
            {
                _rightRepeatTimer -= deltaTime;

                while (_rightRepeatTimer <= 0.0f)
                {
                    _targetLane = MathF.Min(2.0f, _targetLane + 1.0f);
                    _rightRepeatTimer += KeyRepeatRate;

                    if (_targetLane >= 2.0f)
                    {
                        _rightRepeatTimer = KeyRepeatRate;
                        break;
                    }
                }
            }
        }
        else
        {
            _rightRepeatTimer = 0.0f;
        }

        if (fireDown && !_fireWasDown)
        {
            QueueFire();
        }

        _leftWasDown = leftDown;
        _rightWasDown = rightDown;
        _fireWasDown = fireDown;

        float previousLane = _lane;
        float laneDistance = _targetLane - _lane;
        float laneStep = LaneMovementSpeed * deltaTime;

        if (MathF.Abs(laneDistance) <= laneStep)
        {
            _lane = _targetLane;
        }
        else
        {
            _lane += MathF.Sign(laneDistance) * laneStep;
        }

        CheckQueuedFireBeam(previousLane);

        float normalRotation = _arena.GetBeamRotation(_lane);
        float laneMovement = _targetLane - _lane;
        float movementTilt = Math.Clamp(laneMovement * MovementTilt, -MovementTilt, MovementTilt);

        _targetRotation = normalRotation + movementTilt;

        float rotationAmount = MathF.Min(1.0f, deltaTime * RotationSpeed);
        float rotationDifference = NormalizeAngle(_targetRotation - _rotation);

        _rotation += rotationDifference * rotationAmount;

        if (MathF.Abs(_targetLane - _lane) < 0.001f)
        {
            _lane = _targetLane;
            _rotation = _targetRotation;
        }

        _haloRotation += deltaTime * HaloRotationSpeed;

        RebuildLines();
    }

    private void QueueFire()
    {
        _fireReady = false;

        if (MathF.Abs(_targetLane - _lane) <= 0.001f)
        {
            _fireQueued = true;
            _fireBeamLane = (int)MathF.Round(_lane);
            return;
        }

        if (_targetLane > _lane)
        {
            _fireBeamLane = (int)MathF.Floor(_lane) + 1;
        }
        else
        {
            _fireBeamLane = (int)MathF.Ceiling(_lane) - 1;
        }

        _fireQueued = true;
    }

    private void CheckQueuedFireBeam(float previousLane)
    {
        if (!_fireQueued || _fireBeamLane < -2 || _fireBeamLane > 2)
        {
            return;
        }

        float fireBeam = _fireBeamLane;

        if (MathF.Abs(previousLane - fireBeam) <= 0.001f)
        {
            _lane = fireBeam;
            _fireQueued = false;
            _fireReady = true;
            return;
        }

        bool movingRight = _lane > previousLane;
        bool movingLeft = _lane < previousLane;

        if (movingRight && previousLane < fireBeam && _lane >= fireBeam)
        {
            _lane = fireBeam;
            _fireQueued = false;
            _fireReady = true;
            return;
        }

        if (movingLeft && previousLane > fireBeam && _lane <= fireBeam)
        {
            _lane = fireBeam;
            _fireQueued = false;
            _fireReady = true;
        }
    }

    private void RebuildLines()
    {
        _lines.Clear();

        float noseY = PlayerY + (MathF.Cos(_rotation) * NoseOffset);
        Vector2 noseBeamPosition = _arena.GetBeamPosition(_lane, noseY);
        Vector2 noseOffset = Rotate(new Vector2(0.0f, NoseOffset), _rotation);
        Vector2 centre = noseBeamPosition - noseOffset;

        float flamePulse = 10.0f + MathF.Abs(MathF.Sin(Environment.TickCount * 0.015f)) * 12.0f;

        JollyColor shipColor = JollyColor.FromBytes(255, 250, 0);
        JollyColor flameColor = JollyColor.FromBytes(200, 0, 0);

        Vector2 nose = Rotate(new Vector2(0.0f, -25.0f), _rotation);
        Vector2 leftWing = Rotate(new Vector2(-34.0f, 18.0f), _rotation);
        Vector2 rightWing = Rotate(new Vector2(34.0f, 18.0f), _rotation);
        Vector2 lowerCentre = Rotate(new Vector2(0.0f, 8.0f), _rotation);

        _lines.Add(new JollyLineSnapshot(centre + nose, centre + leftWing, shipColor, 5.0f, 2.0f, 0.9f));
        _lines.Add(new JollyLineSnapshot(centre + nose, centre + rightWing, shipColor, 5.0f, 2.0f, 0.9f));
        _lines.Add(new JollyLineSnapshot(centre + leftWing, centre + lowerCentre, shipColor, 4.0f, 2.0f, 0.8f));
        _lines.Add(new JollyLineSnapshot(centre + rightWing, centre + lowerCentre, shipColor, 4.0f, 2.0f, 0.8f));

        Vector2 leftFlameStart = Rotate(new Vector2(-18.0f, 14.0f), _rotation);
        Vector2 leftFlameEnd = Rotate(new Vector2(-8.0f, 14.0f + flamePulse), _rotation);
        Vector2 rightFlameStart = Rotate(new Vector2(18.0f, 14.0f), _rotation);
        Vector2 rightFlameEnd = Rotate(new Vector2(8.0f, 14.0f + flamePulse), _rotation);

        _lines.Add(new JollyLineSnapshot(centre + leftFlameStart, centre + leftFlameEnd, flameColor, 4.0f, 2.0f, 0.9f));
        _lines.Add(new JollyLineSnapshot(centre + rightFlameStart, centre + rightFlameEnd, flameColor, 4.0f, 2.0f, 0.9f));

        Vector2 firingPosition = centre + Rotate(new Vector2(0.0f, -42.0f), _rotation);

        if (!_haloDetached)
        {
            AddHalo(firingPosition, _haloColor);
        }
    }

    private void AddHalo(Vector2 centre, JollyColor haloColor)
    {
        const int segments = 5;
        const float arcSize = MathF.PI * 0.5f;

        float startAngle = _haloRotation;
        float angleStep = arcSize / segments;

        for (int index = 0; index < segments; index++)
        {
            float angle1 = startAngle + (index * angleStep);
            float angle2 = startAngle + ((index + 1) * angleStep);

            Vector2 start = centre + new Vector2(MathF.Cos(angle1) * HaloRadiusX, MathF.Sin(angle1) * HaloRadiusY);
            Vector2 end = centre + new Vector2(MathF.Cos(angle2) * HaloRadiusX, MathF.Sin(angle2) * HaloRadiusY);

            _lines.Add(new JollyLineSnapshot(start, end, haloColor, 1.25f, 2.0f, 0.8f));
        }
    }

    public void RestoreHalo()
    {
        if (_disposed)
        {
            return;
        }

        _haloDetached = false;
        RebuildLines();
    }

    private static Vector2 Rotate(Vector2 point, float angle)
    {
        float cosine = MathF.Cos(angle);
        float sine = MathF.Sin(angle);

        return new Vector2(
            (point.X * cosine) - (point.Y * sine),
            (point.X * sine) + (point.Y * cosine));
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > MathF.PI)
        {
            angle -= MathF.PI * 2.0f;
        }

        while (angle < -MathF.PI)
        {
            angle += MathF.PI * 2.0f;
        }

        return angle;
    }

    public bool ShouldFire()
    {
        if (_disposed || !_fireReady)
        {
            return false;
        }

        _fireReady = false;
        _fireBeamLane = -1;

        return true;
    }

    public BeamRiderProjectile? CreateProjectile()
    {
        if (_disposed || _haloDetached)
        {
            return null;
        }

        Vector2 position = _arena.GetBeamPosition(_lane, 720.0f);

        _haloDetached = true;

        return new BeamRiderProjectile(
            _arena,
            _lane,
            position.Y,
            _haloRotation,
            HaloRadiusX * 0.6f,
            HaloRadiusY * 0.4f,
            _haloColor);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _lines.Clear();
    }
}

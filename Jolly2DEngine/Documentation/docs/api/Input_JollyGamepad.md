# `Input/JollyGamepad.cs`

Public API declarations from `Input/JollyGamepad.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyGamepad
```

```csharp
public int Index { get; }  public bool IsConnected
```

```csharp
public float LeftStickX
```

```csharp
public float LeftStickY
```

```csharp
public float RightStickX
```

```csharp
public float RightStickY
```

```csharp
public float LeftTrigger
```

```csharp
public float RightTrigger
```

```csharp
public JollyGamepad(int index)
```

```csharp
public bool IsButtonDown(JollyGamepadButton button)
```

```csharp
public bool IsButtonPressed(JollyGamepadButton button)
```

```csharp
public bool IsButtonReleased(JollyGamepadButton button)
```

```csharp
public uint PacketNumber;
```

```csharp
public XInputGamepad Gamepad;
```

```csharp
public ushort Buttons;
```

```csharp
public byte LeftTrigger;
```

```csharp
public byte RightTrigger;
```

```csharp
public short ThumbLX;
```

```csharp
public short ThumbLY;
```

```csharp
public short ThumbRX;
```

```csharp
public short ThumbRY;
```

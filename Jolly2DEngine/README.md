# Jolly2DEngine

Jolly2DEngine is a DirectX 11 2D game engine for .NET 10 WPF applications.

## Features

- DirectX 11 rendering
- Sprites and sprite sheets
- Animation
- 2D camera
- Physics and collision
- Particles
- Audio
- Input
- Scenes
- UI
- Save and load
- Reliable TCP networking

## Networking

The networking API provides a lightweight length-framed TCP server and client without requiring an additional networking package.

- `JollyNetworkServer` accepts multiple clients and supports broadcast messages.
- `JollyNetworkClient` connects to a server and exchanges messages.
- `JollyNetworkMessage` provides byte and UTF-8 text payloads.
- Network receive processing runs in background tasks so it does not block the game or WPF UI thread.

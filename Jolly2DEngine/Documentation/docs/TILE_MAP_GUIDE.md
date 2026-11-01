# Tile-map Guide

This guide documents the tile-map functionality present in Jolly2DEngine 1.2.0. It is based on `World/JollyTileMap.cs`, `World/JollyTileMapRenderer.cs`, `Scene/JollyScene.cs`, and `Physics/JollyPhysicsSystem.cs` in the supplied engine source.

## What the current tile-map system provides

- A fixed-size grid with configurable tile width and height.
- A single texture atlas divided into equal-sized tile cells.
- `-1` as the empty-cell value; non-negative values are atlas tile indices.
- Tile-to-world and world-to-tile coordinate helpers.
- A cached sprite renderer that updates and submits only tiles in the camera's visible world bounds.
- Per-cell solid flags.
- Per-tile-index animation, hazard and conveyor settings.
- Scene integration through `JollyScene.SetTileMap` and physics resolution for game objects using physics and a non-trigger collider.

## Coordinate and atlas rules

Tile coordinates start at `(0, 0)` in the top-left of the map. `GetTilePosition(x, y)` returns the tile's local world position as `(x * TileWidth, y * TileHeight)`. The map's scene position is separate: pass it to `JollyScene.SetTileMap(map, x, y)` and set the same `X` and `Y` on the map renderer.

The atlas image must be divisible evenly by the tile width and height. For example, a 256 × 128 atlas with 32 × 32 tiles has 8 columns, 4 rows and 32 tile indices. Indices are row-major, starting at zero in the top-left. `SetTexture` validates divisibility; it does not resize the source image or infer a different tile size.

## Create and populate a map

```csharp
using Jolly2DEngine.World;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Rendering.DirectX;

// Load a tileset atlas whose cell size is 32 × 32 pixels.
JollyTexture atlas = renderer.LoadTexture("Assets/Tiles.png");

// Create a 40-column by 20-row map; each tile occupies 32 × 32 world units.
JollyTileMap map = new(40, 20, 32.0f, 32.0f);
map.SetTexture(atlas);

// Place tiles by atlas index. -1 means the cell is empty.
map.SetTile(0, 19, 0);
map.SetTile(1, 19, 1);
map.SetTile(2, 19, 1);
map.SetTile(3, 19, 2);

// Solid state is per map cell, not automatically inferred from the atlas index.
map.SetSolid(0, 19, true);
map.SetSolid(1, 19, true);
map.SetSolid(2, 19, true);
map.SetSolid(3, 19, true);
```

The code above uses `renderer` as an already-created `DirectXJollyRenderer`. Ensure the texture is created by that renderer, because the renderer only accepts its own DirectX textures.

## Create the tile-map renderer

```csharp
JollyTileMapRenderer mapRenderer = new(map, renderer.CreateSprite)
{
    X = 0.0f,
    Y = 0.0f,
    Layer = 0
};

mapRenderer.Rebuild();
```

Call `Update` with the camera and elapsed time before assembling the render frame. Then include the returned `Sprites` alongside the game's other sprites when creating a `JollyRenderFrame`.

```csharp
mapRenderer.Update(renderer.Camera, (float)gameTime.DeltaSeconds);

List<JollySprite> visibleSprites = new();
visibleSprites.AddRange(mapRenderer.Sprites);
visibleSprites.AddRange(otherSprites);

renderer.SubmitFrame(new JollyRenderFrame(visibleSprites));
```

This is an integration pattern, not a complete `JollyGame` subclass. Your game's render scheduling must call the update/submission code at the appropriate point. The tile-map renderer caches sprite instances by map coordinate and calculates a visible tile range from the camera. When you change map cells at runtime, `Update` reflects those tile values; call `Rebuild` when you want to discard and recreate the sprite cache.

## Collision and scene integration

The scene can store one tile map and its world-space origin:

```csharp
scene.SetTileMap(map, 0.0f, 0.0f);
```

`JollyPhysicsSystem.ResolveTileMap` / `JollyScene.ResolveTileMap` resolve tile collisions for game objects that have physics enabled and an enabled, non-trigger collider. Solid cells block movement and can set `IsGrounded` on landing. A cell's solid flag is separate from the visual tile index, so set both explicitly when needed.

For a moving map, use the same origin coordinates consistently for rendering and collision. `JollyTileMapRenderer.X/Y` affect visual placement; `JollyScene.SetTileMap`'s `x/y` are used for collision placement. They are separate properties and are not automatically synchronised.

## Animated, hazard and conveyor tiles

These settings are associated with the **atlas tile index**, rather than a single grid cell. Every occurrence of that tile index shares the setting.

```csharp
// Animate atlas tiles starting at index 10, cycling through 4 frames at 8 FPS.
map.SetAnimatedTile(tileIndex: 10, startFrame: 10, frameCount: 4, framesPerSecond: 8.0f, loop: true);

// Mark every occurrence of atlas tile 20 as a hazard.
map.SetHazardTile(20, true);
map.HazardContact += (gameObject, tileX, tileY, tileIndex) =>
{
    // Apply game-specific damage or other hazard behaviour here.
};

// A conveyor tile pushes grounded physics objects horizontally at this speed.
map.SetConveyorTile(21, 80.0f);
```

The physics system raises `HazardContact` when a qualifying physics collider intersects a hazard tile. The engine reports the contact; your game decides what damage or response to apply. Conveyor speed is applied when the collision normal indicates the object is standing on the tile. A zero conveyor speed removes the conveyor setting.

For a non-looping animation, the selected frame stops at the final frame. `RemoveAnimatedTile`, `RemoveConveyorTile` and `SetHazardTile(tileIndex, false)` remove the corresponding behaviour.

## Query map data

```csharp
int tileIndex = map.GetTile(tileX, tileY);
bool solid = map.IsSolid(tileX, tileY);
System.Numerics.Vector2 tileOrigin = map.GetTilePosition(tileX, tileY);
System.Numerics.Vector2 tileCoordinate = map.WorldToTile(worldX, worldY);

if (map.TryGetTileAtWorldPosition(worldX, worldY, out int foundTileIndex))
{
    // A non-empty tile was found at this world coordinate.
}
```

`GetTile` returns `-1` for coordinates outside the map. `IsSolid` returns `false` outside the map. `TryGetTileAtWorldPosition` returns `false` for positions outside the map or for an empty cell. `WorldToTile` returns a `Vector2` containing floored tile coordinates; convert those components to integers when using them as grid indices.

## Current limitations to be aware of

The supplied implementation is a useful runtime tile-map foundation, but it is not a full map-authoring pipeline:

- There is no built-in Tiled (`.tmx` / `.tsx`) importer in the supplied tile-map classes.
- `JollyTileMap` represents one grid and one atlas; there is no first-class collection of named tile layers in these classes.
- The solid state is per cell, while animation, hazard and conveyor configuration is shared by atlas tile index.
- There is no built-in visual tile-map editor or map-file serializer in `JollyTileMap`.
- Tile animation is updated using the renderer's elapsed animation time; animation settings are based on atlas indices.

These are possible future improvements, not features this version already promises. For an initial game, create maps in code or write a small JSON loader that fills the grid, tile indices and solid flags.

## API reference

| Member | Purpose |
|---|---|
| `JollyTileMap(width, height, tileWidth, tileHeight)` | Allocate a fixed grid. |
| `SetTexture(texture)` | Assign an evenly divisible tile atlas. |
| `SetTile(x, y, tileIndex)` / `GetTile(x, y)` | Write/read a cell; `-1` is empty. |
| `SetSolid(x, y, solid)` / `IsSolid(x, y)` | Set/read a cell's solid flag. |
| `GetTilePosition(x, y)` | Return the local world origin of a cell. |
| `WorldToTile(x, y)` | Convert world coordinates to floored grid coordinates. |
| `TryGetTileAtWorldPosition(x, y, out tileIndex)` | Query a non-empty tile at a world position. |
| `SetAnimatedTile(...)` | Associate animation frames and rate with an atlas index. |
| `SetHazardTile(tileIndex, hazard)` | Mark/remove an atlas index as hazardous. |
| `SetConveyorTile(tileIndex, speed)` | Associate conveyor speed with an atlas index. |
| `JollyTileMapRenderer(map, spriteFactory)` | Create the sprite cache renderer. |
| `Rebuild()` / `Update(camera, deltaSeconds)` | Recreate the cache / update visible sprites and animation. |

See `docs/api/World_JollyTileMap.md`, `docs/api/World_JollyTileMapRenderer.md`, and the physics/scene API pages for declaration details.

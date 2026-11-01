using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class EliteFlightSimulation
    {
        private const float BubbleRemovalDistance = 2600.0f;
        private const float StationSpawnDistance = 1250.0f;
        private const int MaximumActiveShips = 5;

        private readonly Dictionary<WorldObject, EntityState> _states = [];
        private Random _random = new(1);
        private float _encounterCountdown = 2.5f;

        private enum EntityKind
        {
            Planet,
            Sun,
            Station,
            Ship,
            Asteroid,
            Wreckage
        }

        private sealed class EntityState
        {
            public EntityKind Kind;
            public bool Hostile;
            public float Speed;
            public float MaximumSpeed;
            public float Energy;
            public float CollisionRadius;
            public float FireCountdown;
            public float Lifetime;
            public float Age;
            public Vector3 RotationRate;
            public Vector3 Velocity;
        }

        public void Reset(IReadOnlyList<WorldObject> objects, int[] selectedSeeds, int techLevel, int government)
        {
            ArgumentNullException.ThrowIfNull(objects);
            ArgumentNullException.ThrowIfNull(selectedSeeds);
            if (selectedSeeds.Length != 6)
            {
                throw new ArgumentException("Exactly six selected-system seeds are required.", nameof(selectedSeeds));
            }

            _states.Clear();
            _random = new Random(MakeSeed(selectedSeeds));
            _encounterCountdown = 2.5f + Math.Clamp(techLevel, 0, 15) * 0.12f;

            foreach (WorldObject worldObject in objects)
            {
                _states[worldObject] = CreateInitialState(worldObject);
            }
        }

        public void Update(List<WorldObject> objects, PlayerShip player, float deltaSeconds, int techLevel, int government)
        {
            if (deltaSeconds <= 0.0f)
            {
                return;
            }

            EnsureRegistered(objects);
            SpawnStationWhenPlanetIsClose(objects, player);

            int activeShips = 0;
            foreach (EntityState state in _states.Values)
            {
                if (state.Kind is EntityKind.Ship or EntityKind.Asteroid)
                {
                    activeShips++;
                }
            }

            _encounterCountdown -= deltaSeconds;
            if (_encounterCountdown <= 0.0f && activeShips < MaximumActiveShips)
            {
                SpawnEncounter(objects, player, techLevel, government);
                _encounterCountdown = 5.0f + _random.NextSingle() * 5.0f;
            }

            for (int index = objects.Count - 1; index >= 0; index--)
            {
                WorldObject worldObject = objects[index];
                if (!_states.TryGetValue(worldObject, out EntityState? state))
                {
                    state = CreateInitialState(worldObject);
                    _states[worldObject] = state;
                }

                state.Age += deltaSeconds;
                state.FireCountdown = MathF.Max(0.0f, state.FireCountdown - deltaSeconds);

                if (state.Lifetime > 0.0f && state.Age >= state.Lifetime)
                {
                    RemoveEntity(objects, worldObject);
                    continue;
                }

                if (state.Kind is EntityKind.Planet or EntityKind.Sun)
                {
                    continue;
                }

                if (state.Kind == EntityKind.Station)
                {
                    worldObject.Rotation += state.RotationRate * deltaSeconds;
                    continue;
                }

                if (state.Kind == EntityKind.Wreckage)
                {
                    worldObject.Position += state.Velocity * deltaSeconds;
                    worldObject.Rotation += state.RotationRate * deltaSeconds;
                    continue;
                }

                if (state.Kind == EntityKind.Ship && !state.Hostile && TryDockCivilianShip(objects, worldObject, state, deltaSeconds))
                {
                    continue;
                }

                UpdateShipTactics(worldObject, state, player, deltaSeconds);
                worldObject.Position += Forward(worldObject.Rotation) * state.Speed * deltaSeconds;
                worldObject.Rotation += state.RotationRate * deltaSeconds;

                if (Vector3.DistanceSquared(worldObject.Position, player.Position) > BubbleRemovalDistance * BubbleRemovalDistance)
                {
                    RemoveEntity(objects, worldObject);
                }
            }
        }

        /// <returns>True only when the shot destroys a ship.</returns>
        public bool FireLaser(List<WorldObject> objects, PlayerShip player, Matrix4x4 viewMatrix, Vector2 screenCentre, float focalLength = 256.0f)
        {
            WorldObject? target = null;
            float nearestDepth = float.MaxValue;

            foreach (WorldObject worldObject in objects)
            {
                if (!_states.TryGetValue(worldObject, out EntityState? state) || state.Kind is not (EntityKind.Ship or EntityKind.Asteroid))
                {
                    continue;
                }

                Vector3 viewPosition = Vector3.Transform(worldObject.Position, viewMatrix);
                if (viewPosition.Z <= 2.0f || viewPosition.Z >= nearestDepth)
                {
                    continue;
                }

                Vector2 projected = new(screenCentre.X + viewPosition.X * focalLength / viewPosition.Z, screenCentre.Y - viewPosition.Y * focalLength / viewPosition.Z);
                float targetRadius = MathF.Max(8.0f, state.CollisionRadius * focalLength / viewPosition.Z);

                if (Vector2.DistanceSquared(projected, screenCentre) <= targetRadius * targetRadius)
                {
                    target = worldObject;
                    nearestDepth = viewPosition.Z;
                }
            }

            if (target is null || !_states.TryGetValue(target, out EntityState? targetState))
            {
                return false;
            }

            targetState.Energy -= 35.0f;
            if (targetState.Energy > 0.0f)
            {
                return false;
            }

            Vector3 wreckageVelocity = Vector3.Normalize(target.Position - player.Position) * 0.8f;
            Vector3 wreckagePosition = target.Position;
            RemoveEntity(objects, target);

            if (_random.Next(2) == 0)
            {
                WorldObject wreckage = new(ShipCatalogue.CreateAsteroid(), wreckagePosition)
                {
                    Scale = 0.12f,
                    Rotation = new Vector3(0.4f, 0.7f, 0.2f)
                };

                objects.Add(wreckage);
                _states[wreckage] = new EntityState
                {
                    Kind = EntityKind.Wreckage,
                    Velocity = wreckageVelocity,
                    RotationRate = new Vector3(0.7f, 0.9f, 0.4f),
                    Lifetime = 8.0f,
                    CollisionRadius = 10.0f
                };
            }

            return true;
        }

        private void SpawnStationWhenPlanetIsClose(List<WorldObject> objects, PlayerShip player)
        {
            WorldObject? planet = objects.FirstOrDefault(static item => item.Model is PlanetModel);
            if (planet is null || Vector3.DistanceSquared(planet.Position, player.Position) > StationSpawnDistance * StationSpawnDistance)
            {
                return;
            }

            if (objects.Any(static item => item.Model is CoriolisStationModel or DodecahedronStationModel))
            {
                return;
            }

            WorldObject station = new(new CoriolisStationModel(), planet.Position + new Vector3(0.0f, 0.0f, -240.0f))
            {
                Scale = 0.38f,
                Rotation = new Vector3(0.0f, 0.2f, 0.0f)
            };

            objects.Add(station);
            _states[station] = new EntityState
            {
                Kind = EntityKind.Station,
                RotationRate = new Vector3(0.0f, 0.16f, 0.0f),
                CollisionRadius = 100.0f
            };
        }

        private void SpawnEncounter(List<WorldObject> objects, PlayerShip player, int techLevel, int government)
        {
            int type = _random.Next(4);
            ShipModel model = type switch
            {
                0 => ShipCatalogue.CreateAdder(),
                1 => ShipCatalogue.CreateCobraMkIII(),
                2 => ShipCatalogue.CreateAnaconda(),
                _ => ShipCatalogue.CreateAsteroid()
            };

            bool asteroid = type == 3;
            bool hostile = !asteroid && _random.Next(100) < Math.Clamp(30 + government * 5 + techLevel, 20, 85);
            float distance = 650.0f + _random.NextSingle() * 550.0f;
            float lateral = (_random.NextSingle() * 2.0f - 1.0f) * 260.0f;
            float vertical = (_random.NextSingle() * 2.0f - 1.0f) * 180.0f;
            Vector3 position = player.Position + player.Forward * distance + player.Right * lateral + player.Up * vertical;

            WorldObject ship = new(model, position)
            {
                Scale = type == 2 ? 0.65f : asteroid ? 0.32f : 0.65f,
                Rotation = new Vector3(0.0f, MathF.Atan2(-lateral, distance), 0.0f)
            };

            objects.Add(ship);
            _states[ship] = new EntityState
            {
                Kind = asteroid ? EntityKind.Asteroid : EntityKind.Ship,
                Hostile = hostile,
                Speed = asteroid ? 0.4f : type == 2 ? 1.8f : hostile ? 2.4f : 2.0f,
                MaximumSpeed = asteroid ? 0.6f : type == 2 ? 3.0f : 4.2f,
                Energy = type == 2 ? 160.0f : type == 1 ? 90.0f : 55.0f,
                CollisionRadius = asteroid ? 30.0f : type == 2 ? 60.0f : 35.0f,
                RotationRate = new Vector3(0.0f, (_random.NextSingle() - 0.5f) * 0.15f, 0.0f),
                FireCountdown = 1.5f + _random.NextSingle() * 2.0f
            };
        }


        private bool TryDockCivilianShip(List<WorldObject> objects, WorldObject ship, EntityState state, float deltaSeconds)
        {
            WorldObject? station = null;
            float nearestDistanceSquared = float.MaxValue;

            foreach (WorldObject candidate in objects)
            {
                if (candidate.Model is not (CoriolisStationModel or DodecahedronStationModel))
                {
                    continue;
                }

                float distanceSquared = Vector3.DistanceSquared(ship.Position, candidate.Position);
                if (distanceSquared < nearestDistanceSquared)
                {
                    nearestDistanceSquared = distanceSquared;
                    station = candidate;
                }
            }

            if (station is null || nearestDistanceSquared > 1000.0f * 1000.0f)
            {
                return false;
            }

            if (nearestDistanceSquared < 115.0f * 115.0f)
            {
                RemoveEntity(objects, ship);
                return true;
            }

            Vector3 toStation = station.Position - ship.Position;
            float horizontalDistance = MathF.Sqrt(toStation.X * toStation.X + toStation.Z * toStation.Z);
            float desiredYaw = MathF.Atan2(toStation.X, toStation.Z);
            float desiredPitch = -MathF.Atan2(toStation.Y, MathF.Max(1.0f, horizontalDistance));

            ship.Rotation = new Vector3(
                MoveTowardsAngle(ship.Rotation.X, desiredPitch, deltaSeconds * 0.25f),
                MoveTowardsAngle(ship.Rotation.Y, desiredYaw, deltaSeconds * 0.3f),
                MoveTowardsAngle(ship.Rotation.Z, 0.0f, deltaSeconds * 0.2f));
            state.Speed = MathF.Min(state.MaximumSpeed, state.Speed + deltaSeconds * 0.1f);
            return false;
        }

        private static void UpdateShipTactics(WorldObject ship, EntityState state, PlayerShip player, float deltaSeconds)
        {
            if (!state.Hostile)
            {
                ship.Rotation = new Vector3(MoveTowardsAngle(ship.Rotation.X, 0.0f, deltaSeconds * 0.15f), ship.Rotation.Y + state.RotationRate.Y * deltaSeconds, MoveTowardsAngle(ship.Rotation.Z, 0.0f, deltaSeconds * 0.15f));
                return;
            }

            Vector3 toPlayer = player.Position - ship.Position;
            float horizontalDistance = MathF.Sqrt(toPlayer.X * toPlayer.X + toPlayer.Z * toPlayer.Z);
            float desiredYaw = MathF.Atan2(toPlayer.X, toPlayer.Z);
            float desiredPitch = -MathF.Atan2(toPlayer.Y, MathF.Max(1.0f, horizontalDistance));

            ship.Rotation = new Vector3(MoveTowardsAngle(ship.Rotation.X, desiredPitch, deltaSeconds * 0.55f), MoveTowardsAngle(ship.Rotation.Y, desiredYaw, deltaSeconds * 0.7f), MoveTowardsAngle(ship.Rotation.Z, 0.0f, deltaSeconds * 0.5f));
            state.Speed = MathF.Min(state.MaximumSpeed, state.Speed + deltaSeconds * 0.2f);

            if (toPlayer.LengthSquared() < 1800.0f * 1800.0f && state.FireCountdown <= 0.0f)
            {
                if (Vector3.Dot(Forward(ship.Rotation), Vector3.Normalize(toPlayer)) > 0.92f)
                {
                    Vector3 playerToAttacker = ship.Position - player.Position;
                    Vector3 playerForward = Forward(new Vector3(player.Pitch, player.Yaw, player.Roll));

                    bool hitFromFront = Vector3.Dot(playerForward, Vector3.Normalize(playerToAttacker)) > 0.0f;

                    player.TakeDamage(3.0f, hitFromFront);

                    state.FireCountdown = 2.0f;
                }
            }
        }

        private static EntityState CreateInitialState(WorldObject worldObject)
        {
            if (worldObject.Model is PlanetModel)
            {
                return new EntityState { Kind = EntityKind.Planet, CollisionRadius = 150.0f };
            }

            if (worldObject.Model is SunModel)
            {
                return new EntityState { Kind = EntityKind.Sun, CollisionRadius = 115.0f };
            }

            if (worldObject.Model is CoriolisStationModel or DodecahedronStationModel)
            {
                return new EntityState
                {
                    Kind = EntityKind.Station,
                    RotationRate = new Vector3(0.0f, 0.16f, 0.0f),
                    CollisionRadius = 100.0f
                };
            }

            bool asteroid = worldObject.Model is AsteroidModel;
            return new EntityState
            {
                Kind = asteroid ? EntityKind.Asteroid : EntityKind.Ship,
                Speed = asteroid ? 0.4f : 2.0f,
                MaximumSpeed = asteroid ? 0.6f : 3.2f,
                Energy = asteroid ? 30.0f : 80.0f,
                CollisionRadius = asteroid ? 30.0f : 35.0f,
                RotationRate = new Vector3(0.0f, 0.05f, 0.0f)
            };
        }

        private void EnsureRegistered(IEnumerable<WorldObject> objects)
        {
            foreach (WorldObject worldObject in objects)
            {
                if (!_states.ContainsKey(worldObject))
                {
                    _states[worldObject] = CreateInitialState(worldObject);
                }
            }
        }

        private void RemoveEntity(List<WorldObject> objects, WorldObject worldObject)
        {
            objects.Remove(worldObject);
            _states.Remove(worldObject);
        }

        private static Vector3 Forward(Vector3 rotation)
        {
            float cosPitch = MathF.Cos(rotation.X);
            return Vector3.Normalize(new Vector3(MathF.Sin(rotation.Y) * cosPitch, -MathF.Sin(rotation.X), MathF.Cos(rotation.Y) * cosPitch));
        }

        private static float MoveTowardsAngle(float current, float target, float maxDelta)
        {
            float difference = MathF.IEEERemainder(target - current, MathF.Tau);
            if (MathF.Abs(difference) <= maxDelta)
            {
                return target;
            }

            return current + MathF.Sign(difference) * maxDelta;
        }

        private static int MakeSeed(int[] seeds)
        {
            unchecked
            {
                int hash = 17;
                foreach (int seed in seeds)
                {
                    hash = hash * 31 + seed;
                }

                return hash;
            }
        }
    }
}

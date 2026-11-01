using Jolly2DEngine.Math;

namespace Jolly2DBeamRider.Game
{
    public sealed class BeamRiderSector : IDisposable
    {
        private const int SaucersPerSector = 15;

        private const float FirstSaucerDelay = 0.75f;
        private const float SaucerSpawnDelay = 0.65f;

        private readonly BeamRiderArena _arena;

        private int _sectorNumber;
        private int _spawnedSaucers;
        private int _destroyedSaucers;

        private float _spawnTimer;

        private bool _sectorComplete;
        private bool _sentinelReady;
        private bool _disposed;

        public BeamRiderSector(BeamRiderArena arena, int sectorNumber = 1)
        {
            ArgumentNullException.ThrowIfNull(arena);

            ArgumentOutOfRangeException.ThrowIfLessThan(sectorNumber, 1);

            _arena = arena;
            _sectorNumber = sectorNumber;
            _spawnTimer = FirstSaucerDelay;
        }

        public int Number => _sectorNumber;

        public int TotalSaucers => SaucersPerSector;

        public int SpawnedSaucers => _spawnedSaucers;

        public int DestroyedSaucers => _destroyedSaucers;

        public int RemainingSaucers => SaucersPerSector - _destroyedSaucers;

        public bool SectorComplete => _sectorComplete;

        public bool SentinelReady => _sentinelReady;

        public bool AllSaucersSpawned => _spawnedSaucers >= SaucersPerSector;

        public event Action<BeamRiderSaucer>? SaucerSpawned;

        public event Action<BeamRiderEnemy>? EnemyDestroyed;

        public event Action? AllSaucersDestroyed;

        public event Action? SentinelReadyEvent;

        public void Update(float deltaTime, IList<BeamRiderEnemy> enemies)
        {
            if (_disposed)
            {
                return;
            }

            ArgumentNullException.ThrowIfNull(enemies);

            if (_sectorComplete)
            {
                return;
            }

            if (deltaTime <= 0.0f)
            {
                return;
            }

            ProcessDestroyedEnemies(enemies);

            if (_sentinelReady)
            {
                return;
            }

            if (_spawnedSaucers < SaucersPerSector)
            {
                UpdateSaucerSpawning(deltaTime, enemies);
            }

            if (_spawnedSaucers >= SaucersPerSector && _destroyedSaucers >= SaucersPerSector)
            {
                CompleteSector();
            }
        }

        public void StartNextSector(int sectorNumber, IList<BeamRiderEnemy> enemies)
        {
            if (_disposed)
            {
                return;
            }

            ArgumentNullException.ThrowIfNull(enemies);
            ArgumentOutOfRangeException.ThrowIfLessThan(sectorNumber, 1);

            for (int index = enemies.Count - 1; index >= 0; index--)
            {
                BeamRiderEnemy enemy = enemies[index];
                enemy.Dispose();
                enemies.RemoveAt(index);
            }

            _sectorNumber = sectorNumber;
            _spawnedSaucers = 0;
            _destroyedSaucers = 0;
            _spawnTimer = FirstSaucerDelay;
            _sectorComplete = false;
            _sentinelReady = false;
        }

        public void Reset(IList<BeamRiderEnemy> enemies)
        {
            ArgumentNullException.ThrowIfNull(enemies);

            StartNextSector(_sectorNumber, enemies);
        }

        private void UpdateSaucerSpawning(float deltaTime, IList<BeamRiderEnemy> enemies)
        {
            _spawnTimer -= deltaTime;

            if (_spawnTimer > 0.0f)
            {
                return;
            }

            BeamRiderSaucer saucer = CreateSaucer();
            enemies.Add(saucer);
            _spawnedSaucers++;
            SaucerSpawned?.Invoke(saucer);
            _spawnTimer = SaucerSpawnDelay;
        }

        private BeamRiderSaucer CreateSaucer()
        {
            float lane = SelectSaucerLane();

            float depth = 0.0f;

            float speed = GetSaucerSpeed();

            return new BeamRiderSaucer(_arena, lane, depth, speed);
        }

        private float SelectSaucerLane()
        {
            int patternIndex = _spawnedSaucers % 10;

            return patternIndex switch
            {
                0 => 0.0f,
                1 => -1.0f,
                2 => 1.0f,
                3 => -2.0f,
                4 => 2.0f,
                5 => -1.0f,
                6 => 1.0f,
                7 => 0.0f,
                8 => 2.0f,
                9 => -2.0f,
                _ => 0.0f
            };
        }

        private float GetSaucerSpeed()
        {
            float sectorBonus = MathF.Min((_sectorNumber - 1) * 0.015f, 0.20f);
            return 0.22f + sectorBonus;
        }

        private void ProcessDestroyedEnemies(IList<BeamRiderEnemy> enemies)
        {
            for (int index = 0; index < enemies.Count; index++)
            {
                BeamRiderEnemy enemy = enemies[index];

                if (enemy.Alive)
                {
                    continue;
                }

                if (enemy.Type != BeamRiderEnemyType.Saucer)
                {
                    continue;
                }

                if (enemy is not BeamRiderSaucer)
                {
                    continue;
                }

                _destroyedSaucers++;
                EnemyDestroyed?.Invoke(enemy);
                enemy.Dispose();
                enemies.RemoveAt(index);
                index--;

                if (_destroyedSaucers >= SaucersPerSector)
                {
                    _sentinelReady = true;

                    AllSaucersDestroyed?.Invoke();

                    SentinelReadyEvent?.Invoke();

                    return;
                }
            }
        }

        private void CompleteSector()
        {
            if (_sectorComplete)
            {
                return;
            }

            _sectorComplete = true;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            SaucerSpawned = null;
            EnemyDestroyed = null;
            AllSaucersDestroyed = null;
            SentinelReadyEvent = null;
        }
    }
}

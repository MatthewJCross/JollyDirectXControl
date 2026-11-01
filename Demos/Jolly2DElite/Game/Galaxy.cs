namespace Jolly2DElite.Game
{
    internal readonly record struct GalaxyChartSystem(int X, int Y, string Name);
    
    internal sealed class Galaxy
    {
        private readonly int[] _galaxySeeds = new int[6];
        private readonly int[] _selectedSeeds = new int[6];
        private readonly int[] _scratch = new int[6];
        private readonly int[] _destinationSeeds = new int[6];

        private int _selectedEconomy;
        private int _selectedGovernment;
        private int _selectedTechLevel;
        private int _selectedPopulation;
        private int _selectedProductivity;
        private int _crosshairX;
        private int _crosshairY;

        public int[] SelectedSeeds => _selectedSeeds;
        public int SelectedEconomy => _selectedEconomy;
        public int SelectedGovernment => _selectedGovernment;
        public int SelectedTechLevel => _selectedTechLevel;
        public int SelectedPopulation => _selectedPopulation;
        public int SelectedProductivity => _selectedProductivity;
        public int CrosshairX => _crosshairX;
        public int CrosshairY => _crosshairY;
        public int[] DestinationSeeds => _destinationSeeds;
        public int DestinationX { get; private set; }
        public int DestinationY { get; private set; }
        public int DestinationDistance { get; private set; }

        public int SystemNumber { get; private set; }

        public int CurrentSystemX { get; set; }
        public int CurrentSystemY { get; set; }
        public int SelectedDistance { get; private set; }
        public string SelectedSystemName { get; private set; } = string.Empty;


        public void Reset()
        {
            Array.Clear(_selectedSeeds);
            Array.Clear(_scratch);
            _selectedEconomy = 0;
            _selectedGovernment = 0;
            _selectedTechLevel = 0;
            _selectedPopulation = 0;
            _selectedProductivity = 0;
            _crosshairX = 0;
            _crosshairY = 0;
        }

        /// <summary>
        /// Initialises the selected system to the first system in the current galaxy.
        /// </summary>
        private void SelectFirstSystem()
        {
            Array.Copy(_galaxySeeds, _selectedSeeds, _galaxySeeds.Length);
        }

        /// <summary>
        /// Selects the system nearest to the current galactic coordinates and calculates its distance and system properties.
        /// </summary>
        public void SelectNearestSystem()
        {
            SelectFirstSystem();

            int shortestDistance = 127;
            int selectedSystemIndex = 0;
            int currentSystemIndex = 0;

            do
            {
                int distance = CalculateCrosshairDistance();

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    CopySeeds(_selectedSeeds, _scratch);
                    selectedSystemIndex = currentSystemIndex;
                }

                NextSystem();
                currentSystemIndex = (currentSystemIndex + 1) & 0xFF;
            }
            while (currentSystemIndex != 0);

            CopySeeds(_scratch, _selectedSeeds);

            SystemNumber = selectedSystemIndex;
            _crosshairX = _selectedSeeds[3];
            _crosshairY = _selectedSeeds[1];

            CalculateDistance(_crosshairX);
        }

        /// <summary>
        /// Selects the nearest system to the chart crosshair and stores it as the hyperspace destination.
        /// </summary>
        public bool TrySelectHyperspaceDestination()
        {
            int crosshairX = _crosshairX;
            int crosshairY = _crosshairY;

            SelectNearestSystem();

            _crosshairX = crosshairX;
            _crosshairY = crosshairY;

            if (SelectedDistance <= 0)
            {
                return false;
            }

            Array.Copy(_selectedSeeds, _destinationSeeds, _selectedSeeds.Length);
            DestinationX = _selectedSeeds[3];
            DestinationY = _selectedSeeds[1];
            DestinationDistance = SelectedDistance;
            return true;
        }
        
        /// <summary>
        /// Advances the galaxy seeds to generate the next system.
        /// </summary>
        private void NextSystem()
        {
            for (int i = 0; i < 4; i++)
            {
                AdvanceSystemSeeds();
            }
        }

        /// <summary>
        /// Calculates the distance from the current system to the selected system.
        /// </summary>
        private void CalculateDistance(int systemX)
        {
            int dx = Math.Abs(systemX - CurrentSystemX);
            int dy = Math.Abs(_selectedSeeds[1] - CurrentSystemY) >> 1;

            SelectedDistance = 4 * (int)Math.Sqrt(dx * dx + dy * dy);

            CalculateSystemData();
        }

        public void ArriveAtDestination()
        {
            Array.Copy(_destinationSeeds, _selectedSeeds, _destinationSeeds.Length);
            CurrentSystemX = DestinationX;
            CurrentSystemY = DestinationY;
            _crosshairX = DestinationX;
            _crosshairY = DestinationY;
            CalculateDistance(DestinationX);
        }

        private int CalculateCrosshairDistance()
        {
            int horizontalDistance = Math.Abs(_selectedSeeds[3] - _crosshairX) >> 1;
            int verticalDistance = Math.Abs(_selectedSeeds[1] - _crosshairY) >> 1;

            return horizontalDistance + verticalDistance;
        }

        public void SetGalaxySeeds(int[] seeds)
        {
            if (seeds.Length != _galaxySeeds.Length)
            {
                throw new ArgumentException("Exactly six galaxy seeds are required.", nameof(seeds));
            }

            Array.Copy(seeds, _galaxySeeds, _galaxySeeds.Length);
        }

        private static void CopySeeds(int[] source, int[] destination)
        {
            Array.Copy(source, destination, 6);
        }

        /// <summary>
        /// Advances the galaxy's seed values to generate the next system.
        /// </summary>
        private void AdvanceSystemSeeds()
        {
            int lo = _selectedSeeds[0] + _selectedSeeds[2];
            int hi = _selectedSeeds[1] + _selectedSeeds[3] + (lo > 0xFF ? 1 : 0);
            lo &= 0xFF;
            hi &= 0xFF;

            _selectedSeeds[0] = _selectedSeeds[2];
            _selectedSeeds[1] = _selectedSeeds[3];
            _selectedSeeds[3] = _selectedSeeds[5];
            _selectedSeeds[2] = _selectedSeeds[4];

            int newLo = lo + _selectedSeeds[2];
            _selectedSeeds[4] = newLo & 0xFF;
            _selectedSeeds[5] = (hi + _selectedSeeds[3] + (newLo > 0xFF ? 1 : 0)) & 0xFF;
        }

        /// <summary>
        /// Calculates the selected system's economy, government, technology level, population and productivity from its seed values.
        /// </summary>
        private void CalculateSystemData()
        {
            _selectedEconomy = _selectedSeeds[1] & 0x07;
            _selectedGovernment = (_selectedSeeds[2] >> 3) & 0x07;

            bool isAnarchyOrFeudal = (_selectedGovernment >> 1) == 0;

            if (isAnarchyOrFeudal)
            {
                _selectedEconomy |= 0x02;
            }

            int technologyLevel = (_selectedEconomy ^ 0x07) + (_selectedSeeds[3] & 0x03);
            technologyLevel += (_selectedGovernment >> 1) + (_selectedGovernment & 0x01);

            _selectedTechLevel = technologyLevel & 0xFF;

            _selectedPopulation = (_selectedTechLevel * 4 + _selectedEconomy + _selectedGovernment + 1) & 0xFF;

            int productivityBase = ((_selectedEconomy ^ 0x07) + 3) * (_selectedGovernment + 4);
            int productivity = (productivityBase & 0xFF) * _selectedPopulation;

            _selectedProductivity = (productivity * 8) & 0xFFFF;
            SelectedSystemName = GetSystemName(_selectedSeeds);
        }

        /// <summary>
        /// Positions the crosshair at the specified galactic coordinates.
        /// </summary>
        public void MoveCrosshairTo(int x, int y)
        {
            _crosshairX = Math.Clamp(x, 0, 255);
            _crosshairY = Math.Clamp(y, 0, 255);
        }

        public GalaxyChartSystem[] GetChartSystems()
        {
            int[] seeds = (int[])_galaxySeeds.Clone();
            GalaxyChartSystem[] systems = new GalaxyChartSystem[256];

            for (int i = 0; i < systems.Length; i++)
            {
                systems[i] = new GalaxyChartSystem(seeds[3], seeds[1], GetSystemName(seeds));

                for (int j = 0; j < 4; j++)
                {
                    AdvanceSeeds(seeds);
                }
            }

            return systems;
        }

        private static string GetSystemName(int[] sourceSeeds)
        {
            int[] seeds = (int[])sourceSeeds.Clone();
            System.Text.StringBuilder name = new();

            int pairs = (seeds[0] & 0x40) != 0 ? 3 : 2;

            for (int pair = pairs; pair >= 0; pair--)
            {
                int token = seeds[5] & 31;

                if (token != 0)
                {
                    name.Append(SystemNames.LetterPairs[token]);
                }

                AdvanceSeeds(seeds);
            }

            return name.ToString();
        }

        private static void AdvanceSeeds(int[] seeds)
        {
            int lo = seeds[0] + seeds[2];
            int hi = seeds[1] + seeds[3] + (lo > 0xFF ? 1 : 0);

            lo &= 0xFF;
            hi &= 0xFF;

            seeds[0] = seeds[2];
            seeds[1] = seeds[3];
            seeds[3] = seeds[5];
            seeds[2] = seeds[4];

            int newLo = lo + seeds[2];

            seeds[4] = newLo & 0xFF;
            seeds[5] = (hi + seeds[3] + (newLo > 0xFF ? 1 : 0)) & 0xFF;
        }

        public void SelectNearestSystemAtCrosshair()
        {
            int crosshairX = _crosshairX;
            int crosshairY = _crosshairY;

            SelectNearestSystem();

            _crosshairX = crosshairX;
            _crosshairY = crosshairY;
        }
    }
}
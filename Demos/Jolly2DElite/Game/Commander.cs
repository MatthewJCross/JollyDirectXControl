using System.IO;

namespace Jolly2DElite.Game;

internal sealed class Commander
{
    private const int CommanderDataSize = 256;    
    
    public string Name { get; set; } = "JAMESON";
    public int MissionStatus { get; set; }
    public int CurrentSystemX { get; set; }
    public int CurrentSystemY { get; set; }
    public int[] GalaxySeeds { get; } = new int[6];
    public uint Cash { get; set; }
    public int Fuel { get; set; }
    public int CompetitionFlags { get; set; }
    public int GalaxyNumber { get; set; }
    public int[] Lasers { get; } = new int[4];
    public int CargoCapacity { get; set; }
    public int[] Cargo { get; } = new int[17];
    public int Ecm { get; set; }
    public int FuelScoops { get; set; }
    public int EnergyBomb { get; set; }
    public int EnergyUnit { get; set; }
    public int DockingComputer { get; set; }
    public int GalacticHyperdrive { get; set; }
    public int EscapePod { get; set; }
    public int KillTallyFraction { get; set; }
    public int Missiles { get; set; }
    public int LegalStatus { get; set; }
    public int[] MarketAvailability { get; } = new int[17];
    public int MarketRandom { get; set; }
    public int KillTally { get; set; }
    public int SaveCount { get; set; }

    public void ApplySavedCommander(CommanderSave save)
    {
        ApplySavedCommander(save.Data, save.Commander);
    }

    public void ApplySavedCommander(byte[] data, string commanderName)
    {
        if (data.Length < CommanderDataSize)
        {
            throw new InvalidDataException("Commander save data is incomplete.");
        }

        Name = commanderName;

        MissionStatus = data[0];
        CurrentSystemX = data[1];
        CurrentSystemY = data[2];

        Array.Copy(data, 3, GalaxySeeds, 0, GalaxySeeds.Length);

        Cash = (uint)(data[9] << 24 | data[10] << 16 | data[11] << 8 | data[12]);

        Fuel = data[13];
        CompetitionFlags = data[14];
        GalaxyNumber = data[15];

        Array.Copy(data, 16, Lasers, 0, Lasers.Length);

        CargoCapacity = data[22];
        Array.Copy(data, 23, Cargo, 0, Cargo.Length);

        Ecm = data[40];
        FuelScoops = data[41];
        EnergyBomb = data[42];
        EnergyUnit = data[43];
        DockingComputer = data[44];
        GalacticHyperdrive = data[45];
        EscapePod = data[46];
        KillTallyFraction = data[50];
        Missiles = data[51];
        LegalStatus = data[52];

        Array.Copy(data, 53, MarketAvailability, 0, MarketAvailability.Length);

        MarketRandom = data[70];
        KillTally = data[71] | (data[72] << 8);
        SaveCount = data[73];
    }

    public void Reset()
    {
        Name = DefaultCommander.Name;
        MissionStatus = 0;
        CurrentSystemX = DefaultCommander.SystemX;
        CurrentSystemY = DefaultCommander.SystemY;

        ReadOnlySpan<byte> galaxySeeds = DefaultCommander.GalaxySeeds;
        for (int i = 0; i < GalaxySeeds.Length; i++)
        {
            GalaxySeeds[i] = galaxySeeds[i];
        }

        Cash = DefaultCommander.Cash;
        Fuel = DefaultCommander.Fuel;
        CompetitionFlags = 0;
        GalaxyNumber = 0;

        Array.Clear(Lasers);
        Lasers[0] = DefaultCommander.FrontLaser;

        CargoCapacity = DefaultCommander.CargoCapacity;
        Array.Clear(Cargo);

        Ecm = 0;
        FuelScoops = 0;
        EnergyBomb = 0;
        EnergyUnit = 0;
        DockingComputer = 0;
        GalacticHyperdrive = 0;
        EscapePod = 0;
        KillTallyFraction = 0;
        Missiles = DefaultCommander.Missiles;
        LegalStatus = 0;

        ReadOnlySpan<byte> marketAvailability = DefaultCommander.MarketAvailability;
        for (int i = 0; i < MarketAvailability.Length; i++)
        {
            MarketAvailability[i] = marketAvailability[i];
        }

        MarketRandom = 0;
        KillTally = 0;
        SaveCount = DefaultCommander.SaveCount;
    }

    public byte[] CreateSaveData()
    {
        byte[] data = new byte[256];

        data[0] = (byte)MissionStatus;
        data[1] = (byte)CurrentSystemX;
        data[2] = (byte)CurrentSystemY;

        for (int i = 0; i < GalaxySeeds.Length; i++)
            data[3 + i] = (byte)GalaxySeeds[i];

        data[9] = (byte)(Cash >> 24);
        data[10] = (byte)(Cash >> 16);
        data[11] = (byte)(Cash >> 8);
        data[12] = (byte)Cash;

        data[13] = (byte)Fuel;
        data[14] = (byte)CompetitionFlags;
        data[15] = (byte)GalaxyNumber;

        for (int i = 0; i < Lasers.Length; i++)
            data[16 + i] = (byte)Lasers[i];

        data[22] = (byte)CargoCapacity;

        for (int i = 0; i < Cargo.Length; i++)
            data[23 + i] = (byte)Cargo[i];

        data[40] = (byte)Ecm;
        data[41] = (byte)FuelScoops;
        data[42] = (byte)EnergyBomb;
        data[43] = (byte)EnergyUnit;
        data[44] = (byte)DockingComputer;
        data[45] = (byte)GalacticHyperdrive;
        data[46] = (byte)EscapePod;
        data[50] = (byte)KillTallyFraction;
        data[51] = (byte)Missiles;
        data[52] = (byte)LegalStatus;

        for (int i = 0; i < MarketAvailability.Length; i++)
            data[53 + i] = (byte)MarketAvailability[i];

        data[70] = (byte)MarketRandom;
        data[71] = (byte)KillTally;
        data[72] = (byte)(KillTally >> 8);
        data[73] = (byte)SaveCount;

        return data;
    }
    
    //private int CalculateChecksum()
    //{
    //    int index = CommanderDataSize - 3;
    //    int checksum = index;
    //    int carry = 0;
    //    for (; index > 0; index--)
    //    {
    //        int sum = checksum + _savedCommander[7 + index] + carry;
    //        carry = sum > 0xFF ? 1 : 0;
    //        checksum = (sum & 0xFF) ^ _savedCommander[8 + index];
    //    }

    //    return checksum;
    //}
}

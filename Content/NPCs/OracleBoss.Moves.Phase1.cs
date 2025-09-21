using Terraria.ModLoader;

namespace TheOracle.Content.NPCs;

public partial class OracleBoss : ModNPC
{
    int DoOrbConjure()
    {
        IdleCrystal(1);
        
        return OrbConjure;
    }

    int DoCrystalSliceDash()
    {
        return CrystalSliceDash;
    }

    int DoMagicRain()
    {
        return MagicRain;
    }

    int DoOrbClockHandSwordForm()
    {
        return OrbClockHandSwordForm;
    }

    int DoTeleportOrbWeb()
    {
        return TeleportOrbWeb;
    }

    int DoSweepingProjectilesThatReverse()
    {
        return SweepingProjectilesThatReverse;
    }

    int DoLaserRefraction()
    {
        return LaserRefraction;
    }
}
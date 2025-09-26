using Terraria.ModLoader;

namespace TheOracle.Content.NPCs;

public partial class OracleBoss : ModNPC
{
    int DoDespawn()
    {
        return Despawn;
    }

    int DoIntro()
    {
        IdleOrbs(1);
        return Intro;
    }
}
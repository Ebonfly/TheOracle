using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheOracle.Common.Utils;

namespace TheOracle.Content.NPCs;

public partial class OracleBoss : ModNPC
{
    public override string Texture => "TheOracle/Assets/Images/NPCs/OracleBoss";

    public bool Phase2;

    Vector2[] _cachedEyeOffsets = new Vector2[5];
    public Vector2 EyeTarget;

    public Vector2 CrystalPosition;
    public float CrystalRotation;

    private const int OrbAmount = 4;
    public float[] OrbRotation = new float[OrbAmount];
    public Vector2[] OrbPosition = new Vector2[OrbAmount];
    public float IdleSpeed = 1f;
    public int ConstantTimer;
    public ref float AIState => ref NPC.ai[0];
    public ref float AITimer => ref NPC.ai[1];
    public ref float AITimer2 => ref NPC.ai[2];
    public ref float AITimer3 => ref NPC.ai[3];

    private const int Despawn = -1,
        Intro = 0,
        OrbConjure = 1,
        CrystalSliceDash = 2,
        MagicRain = 3,
        OrbClockHandSwordForm = 4,
        TeleportOrbWeb = 5,
        SweepingProjectilesThatReverse = 6,
        LaserRefraction = 7;

    void DoAI()
    {
        AIState = AIState switch
        {
            Despawn => DoDespawn(),
            Intro => DoIntro(),
            OrbConjure => DoOrbConjure(),
            CrystalSliceDash => DoCrystalSliceDash(),
            MagicRain => DoMagicRain(),
            OrbClockHandSwordForm => DoOrbClockHandSwordForm(),
            TeleportOrbWeb => DoTeleportOrbWeb(),
            SweepingProjectilesThatReverse => DoSweepingProjectilesThatReverse(),
            LaserRefraction => DoLaserRefraction(),
            _ => AIState
        };
    }

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 2;
        NPCID.Sets.TrailCacheLength[Type] = 10;
        NPCID.Sets.TrailingMode[Type] = 3;
        NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
    }

    public override void SetDefaults()
    {
        NPC.Size = new Vector2(202, 304);
        NPC.lifeMax = 150000;
        NPC.defense = 50;
        NPC.aiStyle = -1;
        NPC.boss = true;
        NPC.noGravity = NPC.noTileCollide = NPC.lavaImmune = true;
        NPC.buffImmune[BuffID.Confused] = NPC.buffImmune[BuffID.OnFire] = NPC.buffImmune[BuffID.OnFire3] =
            NPC.buffImmune[BuffID.Frostburn] = NPC.buffImmune[BuffID.Frostburn2] = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.knockBackResist = 0f;
    }
}
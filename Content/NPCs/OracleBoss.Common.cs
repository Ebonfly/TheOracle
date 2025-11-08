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
    public Vector2 DisposablePosition;

    public Vector2 CrystalPosition;
    public float CrystalRotation;

    private const int OrbAmount = 4;
    public float[] OrbRotation = new float[OrbAmount];
    public Vector2[] OrbPosition = new Vector2[OrbAmount];
    public Vector2[,] OldOrbPosition = new Vector2[OrbAmount, 5];
    public float IdleSpeed = 1f;
    public int ConstantTimer;
    public int Substate;
    public ref float AIState => ref NPC.ai[0];
    public ref float AITimer => ref NPC.ai[1];
    public ref float AITimer2 => ref NPC.ai[2];
    public ref float AITimer3 => ref NPC.ai[3];

    private const int Despawn = -1,
        Intro = 0,
        OrbConjure = 1,
        CrystalSliceDash = 2,
        OrbClockHandSwordForm = 3,
        MagicRain = 4,
        TeleportOrbWeb = 5,
        SweepingProjectilesThatReverse = 6,
        LaserRefraction = 7,
        HourGlassOrbs = 8, // #22 
        GiantClockLaser = 9,
        MagolorFields = 10,
        SigilCannonballs = 11, // #12     yes
        PolaritiesClocks = 12, // yes
        QuartzWatch = 13, // #24  yes
        ElfilisOrbClones = 14, // yes
        CrystalCrackHoming = 15; // #20  uhh maybe

    void DoAI()
    {
        AIState = AIState switch
        {
            Despawn => DoDespawn(),
            Intro => DoIntro(),
            OrbConjure => DoOrbConjure(),
            CrystalSliceDash => DoCrystalSliceDash(),
            OrbClockHandSwordForm => DoOrbClockHandSwordForm(),
            MagicRain => DoMagicRain(),
            TeleportOrbWeb => DoTeleportOrbWeb(),
            SweepingProjectilesThatReverse => DoSweepingProjectilesThatReverse(),
            LaserRefraction => DoLaserRefraction(),
            HourGlassOrbs => DoHourGlassOrbs(),
            GiantClockLaser => DoGiantClockLaser(),
            MagolorFields => DoMagolorFields(),
            SigilCannonballs => DoSigilCannonballs(),
            PolaritiesClocks => DoPolaritiesClocks(),
            QuartzWatch => DoQuartzWatch(),
            ElfilisOrbClones => DoElfilisOrbClones(),
            CrystalCrackHoming => DoCrystalCrackHoming(),
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
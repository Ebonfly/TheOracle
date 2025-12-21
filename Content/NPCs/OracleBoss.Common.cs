using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheOracle.Common.Utils;
using TheOracle.Content.Items;

namespace TheOracle.Content.NPCs;

public partial class OracleBoss : ModNPC
{
    public override string Texture => "TheOracle/Assets/Images/NPCs/OracleBoss";

    public static bool AnyOracleIsPhase2;
    public override void ResetEffects() => AnyOracleIsPhase2 = false;

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
    public SlotId SlotIdIdleSound;

    private const int PhaseTransition = -2,
        Despawn = -1,
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
        SigilCannonballs = 11, // #12  
        OrbitalBeamPortals = 12,
        CannonSweep = 13,
        PolaritiesClocks = 14,
        QuartzWatch = 15, // #24  
        ElfilisOrbClones = 16,
        CrystalCrackHoming = 17; // #20 

    void DoAI()
    {
        AIState = AIState switch
        {
            PhaseTransition => DoPhaseTransition(),
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
            OrbitalBeamPortals => DoOrbitalBeamPortals(),
            CannonSweep => DoCannonSweep(),
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
        NPC.value = Item.buyPrice(0, 80, 0, 0);
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Placeable.Furniture.MinionBossTrophy>(), 10));

        LeadingConditionRule notExpertRule = new(new Conditions.NotExpert());

        //notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<MinionBossMask>(), 7));

        npcLoot.Add(notExpertRule);

        npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<OracleBossBag>()));

        //npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<Items.Placeable.Furniture.MinionBossRelic>()));

        npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<KeepsakeOfOblivion>(), 4));
    }
}
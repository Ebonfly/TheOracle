using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TheOracle.Content.NPCs;

public partial class OracleBoss : ModNPC
{
    public override void AI()
    {
        TargetingLogic(0);
        ConstantTimer++;

        NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.1f, -0.3f, 0.3f);
        NPC.DiscourageDespawn(10);

        for (int i = 0; i < OrbPosition.Length; i++)
            SaveOldOrbPosition(i);

        if (!Main.dedServ)
            HandleIdleSound();

        if (Main.mouseRight)
        {
            Substate = 0;
            NPC.velocity = (Main.MouseWorld - NPC.Center) * 0.05f;
            AIState = OrbitalBeamPortals;
            EyeTarget = Player.Center;
            AITimer = -50;
            AITimer2 = 0;
            DisposablePosition = Vector2.Zero;
            AITimer3 = 0;
            for (int i = 0; i < NPC.localAI.Length; i++)
                NPC.localAI[0] = 0;
            CrystalOpacity = 1f;
            IdleOrbs();
            IdleCrystal();
            return;
        }

        AITimer++;
        if (AITimer < 0)
        {
            NPC.velocity = Vector2.Lerp(NPC.velocity, (Player.Center - NPC.Center) * 0.05f, 0.1f);
            CrystalOpacity = MathHelper.Lerp(CrystalOpacity, 1f, IdleSpeed);
            EyeTarget = Vector2.Lerp(EyeTarget, Player.Center, IdleSpeed);
            IdleSpeed = MathHelper.Lerp(IdleSpeed, 1f, 0.025f);
            IdleCrystal(IdleSpeed);
            IdleOrbs(IdleSpeed);
            if (AITimer > -30)
                NPC.velocity *= MathHelper.Lerp(0.98f, 0, (AITimer + 30) / 30f);
        }
        else
        {
            IdleSpeed = 0;
            DoAI();
        }
    }
}
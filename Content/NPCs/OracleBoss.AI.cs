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
        if (Main.mouseRight)
        {
            NPC.velocity = (Main.MouseWorld - NPC.Center) * 0.05f;
            AIState = CrystalSliceDash;
            EyeTarget = Player.Center;
            AITimer = -50;
            AITimer2 = 0;
            AITimer3 = 0;
            CrystalOpacity = 1f;
            IdleOrbs();
            IdleCrystal();
            return;
        }

        AITimer++;
        if (AITimer < 0)
        {
            CrystalOpacity = MathHelper.Lerp(CrystalOpacity, 1f, IdleSpeed);
            EyeTarget = Vector2.Lerp(EyeTarget, Player.Center, IdleSpeed);
            IdleSpeed = MathHelper.Lerp(IdleSpeed, 1f, 0.1f);
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
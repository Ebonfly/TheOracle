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
            NPC.velocity = (Main.MouseWorld - NPC.Center) * 0.01f;
            AIState = CrystalSliceDash;
            AITimer = 0;
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
            EyeTarget = Player.Center;
            IdleSpeed = MathHelper.Lerp(IdleSpeed, 1f, 0.1f);
            IdleCrystal(IdleSpeed);
            IdleOrbs(IdleSpeed);
        }
        else
        {
            IdleSpeed = 0;
            DoAI();
        }
    }
}
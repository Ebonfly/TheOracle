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

        NPC.velocity = (Main.MouseWorld - NPC.Center) * 0.01f;
        NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.1f, -0.3f, 0.3f);

        AITimer++;
        if (AITimer < 0)
        {
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
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheOracle.Content.Projectiles;

namespace TheOracle.Content.NPCs;

public partial class OracleBoss : ModNPC
{
    int DoOrbConjure()
    {
        IdleCrystal(1);
        float factor = MathF.Pow(MathHelper.Clamp((AITimer - 70) / 120f, 0, 1f), 2);
        float factor2 = MathF.Pow(MathHelper.Clamp(AITimer / 120f, 0, 1f), 2);
        if (AITimer > 550)
        {
            factor = MathHelper.Lerp(factor, 0, MathHelper.Clamp(AITimer / 60f, 0, 1f));
            factor2 = MathHelper.Lerp(factor2, 0, MathHelper.Clamp(AITimer / 60f, 0, 1f));
        }

        AITimer2 += factor;
        if (AITimer < 50 || AITimer > 560)
        {
            AITimer3 = MathHelper.Lerp(AITimer3, 1, 0.05f);
            OminousYVelHover(AITimer3);
            NPC.velocity.X *= 0.98f;
            EyeTarget = Player.Center;
        }
        else
        {
            AITimer3 = 0;
            NPC.velocity = Vector2.Lerp(NPC.velocity,
                ((EyeTarget + new Vector2(600).RotatedBy(MathHelper.ToRadians(ConstantTimer))) - NPC.Center) * 0.01f,
                0.05f);
        }

        for (int i = 0; i < OrbPosition.Length; i++)
        {
            float angle = MathHelper.TwoPi * i / (float)OrbPosition.Length;
            angle += MathHelper.ToRadians(AITimer2 * 7);
            Vector2 offset = new Vector2(50 + 100 * factor).RotatedBy(angle);
            OrbPosition[i] = Vector2.Lerp(OrbPosition[i], EyeTarget + offset, factor2 * 0.12f);
        }

        if ((int)AITimer == 110 && Main.netMode != NetmodeID.MultiplayerClient)
            Projectile.NewProjectile(null, EyeTarget, Vector2.Zero, ModContent.ProjectileType<OracleMiniClock>(), 0, 0);

        if (AITimer > 560)
            IdleOrbs(MathHelper.Clamp((AITimer - 560) / 100f, 0, 1f));
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
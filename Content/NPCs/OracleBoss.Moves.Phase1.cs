using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheOracle.Content.Dusts;
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
        IdleOrbs();
        if (AITimer < 2)
            NPC.velocity = Vector2.Zero;
        if (AITimer < 30)
        {
            EyeTarget = Vector2.Lerp(EyeTarget, CrystalPosition, AITimer / 30f);
            NPC.velocity.Y -= 0.05f;
        }
        else
        {
            EyeTarget = CrystalPosition;
            NPC.velocity *= 0.95f;
        }

        if (AITimer < 40)
            IdleCrystal();
        else if (AITimer < 80)
        {
            CrystalRotation = Utils.AngleLerp(CrystalRotation, 0, 0.1f);
            CrystalPosition.Y -= MathHelper.SmoothStep(1, 2, MathF.Sin((AITimer - 40f) / 40f * MathHelper.Pi));
            CrystalPosition.X = MathHelper.Lerp(CrystalPosition.X, NPC.Center.X, 0.1f);
            Vector2 pos = CrystalPosition +
                          Main.rand.NextVector2Unit() * 700;
            for (int i = 0; i < 3; i++)
                Dust.NewDustPerfect(pos, ModContent.DustType<GlowDust>(),
                    (CrystalPosition - new Vector2(0, 40) - pos).SafeNormalize(Vector2.Zero) * 16,
                    newColor: Color.CornflowerBlue with { A = 0 }, Scale: 0.9f).noGravity = true;
        }
        else if ((int)AITimer == 140)
        {
            CrystalFlash = 1;
            SoundEngine.PlaySound(SoundID.Item4.WithPitchOffset(-0.5f), CrystalPosition);
        }

        if (AITimer > 140)
        {
            if (AITimer < 150)
                CrystalRotation =
                    CrystalRotation.AngleLerp((Player.Center - CrystalPosition).ToRotation() + MathHelper.PiOver2,
                        MathHelper.Lerp(0.2f, 0.5f, (AITimer - 140f) / 10f));

            if ((int)AITimer == 150)
                Projectile.NewProjectile(null, CrystalPosition, CrystalTipDirection,
                    ModContent.ProjectileType<CrystalSlice>(), 25, 0);

            else if (AITimer < 190)
                CrystalPosition -= CrystalTipDirection * MathF.Sin(MathF.Pow((AITimer - 170) / 20f, 2) * MathHelper.Pi);
            else if (AITimer < 200)
            {
                CrystalPosition += CrystalTipDirection *
                                   MathHelper.SmoothStep(100, 20, MathHelper.Clamp((AITimer - 190) / 10f, 0, 1));
                CrystalOpacity = MathHelper.SmoothStep(1, 0, (AITimer - 190) / 10f);
            }
            else if (AITimer < 250)
            {
                CrystalOpacity = 0;
                if (AITimer % 4 == 0)
                {
                    Vector2 position = Player.Center + Main.rand.NextVector2CircularEdge(800, 800);
                    Projectile.NewProjectile(null, position,
                        (Player.Center - position).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.6f),
                        ModContent.ProjectileType<CrystalSlice>(), 25, 0);
                }
            }
            else if ((int)AITimer == 270)
            {
                Vector2 usualPos = NPC.Center - new Vector2(IdleSwayFactor * 6,
                        NPC.height * 0.28f - MathF.Sin(MathF.Abs(IdleSwayFactor) * MathHelper.Pi) * 6)
                    .RotatedBy(NPC.rotation);
                Projectile.NewProjectile(null, CrystalPosition + CrystalTipDirection * 750,
                    (usualPos - (CrystalPosition + CrystalTipDirection * 750)).SafeNormalize(Vector2.UnitX),
                    ModContent.ProjectileType<CrystalSlice>(), 0, 0);

                float rot = CrystalRotation;
                IdleCrystal();
                CrystalRotation = rot;
            }

            if (AITimer > 330)
                CrystalOpacity = MathHelper.Lerp(CrystalOpacity, 1f, 0.15f);

            if (AITimer > 345)
                IdleCrystal(AITimer2 = MathHelper.Lerp(AITimer2, 1, 0.025f));
        }

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
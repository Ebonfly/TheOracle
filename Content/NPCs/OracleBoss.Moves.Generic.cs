using System;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;
using TheOracle.Content.Dusts;
using TheOracle.Content.Projectiles;
using TheOracle.Content.Projectiles.VFX;
using TheOracle.Content.Skies;

namespace TheOracle.Content.NPCs;

public partial class OracleBoss : ModNPC
{
    int DoPhaseTransition()
    {
        switch (Substate)
        {
            case 0:
            {
                if ((int)AITimer == 1)
                {
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center, Main.rand.NextVector2Unit(),
                        15, 30, 20));
                    OracleSky.SkyFlash = 1f;
                }

                EyeTarget = Vector2.Lerp(EyeTarget, NPC.Center + Main.rand.NextVector2Circular(100, 100), 0.4f);

                NPC.rotation = MathF.Sin(ConstantTimer) * MathF.Sin(AITimer / 40f * MathF.PI) * 0.1f;

                if (AITimer > 40)
                    IncrementSubstate();
            }
                break;

            case 1:
            {
                NPC.rotation = Utils.AngleLerp(NPC.rotation, 0, 0.1f);
                EyeTarget = Vector2.Lerp(EyeTarget, NPC.Center, 0.1f);
                IdleOrbs(MathHelper.Clamp(AITimer / 70f, 0, 1f));

                if ((int)AITimer % 5 == 0 && AITimer >= 40)
                {
                    CrystalOpacity = 0;
                    Projectile.NewProjectile(null, NPC.Center,
                        Vector2.Zero, ModContent.ProjectileType<VioletExplosion>(), 0,
                        0, ai1: MathHelper.Lerp(1.5f, 0.5f, (AITimer - 40) / 20f));
                    Phase2 = true;
                }

                if (AITimer > 60)
                    IncrementSubstate();
            }
                break;
        }


        return PhaseTransition;
    }

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
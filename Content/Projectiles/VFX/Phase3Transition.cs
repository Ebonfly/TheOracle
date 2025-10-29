using System;
using System.Collections.Generic;
using TheOracle.Common.Utils;

namespace TheOracle.Content.Projectiles.VFX;

public class Phase3Transition_TheHorns : ModProjectile
{
    public override string Texture => QuickAssets.EMPTY_KEY;

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 300;
        Projectile.aiStyle = -1;
    }

    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;

    public override void AI()
    {
        Projectile.Center = Main.LocalPlayer.Center;
        Projectile.timeLeft = 10;
        base.AI();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D glow = Images.Extras.Textures.Glow.Value;
        Texture2D flare = Images.Extras.Textures.Crosslight.Value;
        Texture2D streak = Images.Extras.Textures.Slash.Value;

        Vector2 pos = new Vector2(Main.screenWidth / 2f, 400);
        for (int c = 0; c < 2; c++)
        {
            UnifiedRandom rand = new UnifiedRandom(1291144);
            Color col = c == 0 ? Color.White with { A = 0 } : Color.Gold with { A = 0 };
            col *= (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.1f) * 0.1f + 1f);

            Main.spriteBatch.Draw(glow, pos + Main.rand.NextVector2Unit(), null, col, 0,
                glow.Size() / 2f, .2f, SpriteEffects.None, 0);

            for (int i = 0; i < 40; i++)
            {
                // Draw the little petals
                float mult = (Main.GlobalTimeWrappedHourly * rand.NextFloat(0.1f, 0.2f) + i * 0.2f) % 1f;
                Vector2 pos2 = pos + rand.NextVector2Unit().RotatedByRandom(0.01f * MathF.Sin(mult * MathF.PI))
                    .RotatedBy(Main.GlobalTimeWrappedHourly * 0.01f) * rand.NextFloat(10, 250) * mult;

                float rotation = (pos - pos2).ToRotation();

                Main.spriteBatch.Draw(glow, pos2 + Main.rand.NextVector2Unit(), null,
                    col * MathF.Sin(mult * MathF.PI) * 0.1f, rotation + MathHelper.PiOver2,
                    glow.Size() / 2f, new Vector2(0.1f + 0.3f * (1 - mult), 1.1f - mult * mult), SpriteEffects.None,
                    0);

                // Draw the tentacles
                if (i % 4 == 0)
                {
                    Vector2 tentacleEnd = pos +
                                          rand.NextVector2Unit()
                                              .RotatedBy(MathF.Sin(Main.GlobalTimeWrappedHourly *
                                                                   rand.NextFloat(0.2f, 1) * 0.1f)) *
                                          rand.NextFloat(400, 600);
                    rotation = (tentacleEnd - pos).ToRotation();
                    List<VertexPositionColorTexture> vertices = [];

                    for (float j = 0; j < 1f; j += 0.02f)
                    {
                        UnifiedRandom rand2 = new UnifiedRandom(12415 + i);
                        Vector2 tPos = Vector2.Lerp(pos, tentacleEnd, j) +
                                       new Vector2(0, MathF.Sin(j * MathHelper.TwoPi - Main.GlobalTimeWrappedHourly *
                                           rand2.NextFloat(0.2f, 1)) * 40).RotatedBy(rotation);
                        vertices.Add(PrimitiveUtils.AsVertex(
                            tPos + new Vector2(40 * (1 - j), 0).RotatedBy(rotation + MathHelper.PiOver2),
                            col * (1 - j) * 0.2f,
                            new Vector2(0, 0)));
                        vertices.Add(PrimitiveUtils.AsVertex(
                            tPos + new Vector2(40 * (1 - j), 0).RotatedBy(rotation - MathHelper.PiOver2),
                            col * (1 - j) * 0.2f,
                            new Vector2(1, 1)));
                    }

                    if (vertices.Count > 2)
                    {
                        PrimitiveUtils.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip,
                            Images.Extras.Textures.Tentacle.Value);
                    }
                }
            }
        }

        return base.PreDraw(ref lightColor);
    }
}

public class Phase3Transition_Spear : ModProjectile
{
    public override string Texture => QuickAssets.EMPTY_KEY;

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 120;
        Projectile.aiStyle = -1;
    }

    public override bool? CanDamage() => false;

    public override void AI()
    {
        base.AI();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = Images.Extras.Textures.ClockHandLong.Value;
        return base.PreDraw(ref lightColor);
    }
}
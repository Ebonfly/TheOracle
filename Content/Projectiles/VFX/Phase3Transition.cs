using System;
using System.Collections.Generic;
using TheOracle.Common.Utils;

namespace TheOracle.Content.Projectiles.VFX;

public class Phase3Transition_TheHorns : ModProjectile
{
    public override void Load()
    {
        On_Main.DrawBG += (orig, self) =>
        {
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.type == ModContent.ProjectileType<Phase3Transition_TheHorns>())
                {
                    Color c = Color.White;
                    proj.ModProjectile?.PreDraw(ref c);
                }
            }

            orig(self);
        };
    }

    public override string Texture => QuickAssets.EMPTY_KEY;

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 300;
        Projectile.aiStyle = -1;
        Projectile.hide = true;
    }

    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;

    public override void AI()
    {
        if (Main.mouseRight)
            Projectile.Center = Main.MouseWorld;
        Projectile.timeLeft = 10;
        base.AI();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        //float time = Main.GameUpdateCount * 0.02f;
        float time = Main.GlobalTimeWrappedHourly;
        Texture2D glow = Images.Extras.Textures.Glow.Value;
        Texture2D flare = Images.Extras.Textures.Crosslight.Value;
        Texture2D flare2 = Images.Extras.Textures.Slash.Value;

        Vector2 pos = Vector2.Lerp(Projectile.Center, Main.LocalPlayer.Center, 0.9f) - Main.screenPosition;
        for (int c = 0; c < 2; c++)
        {
            UnifiedRandom rand = new UnifiedRandom(1291144);

            for (int i = 0; i < 40; i++)
            {
                Color col = c == 0
                    ? Color.Gold with { A = 0 }
                    : Main.hslToRgb((i / 40f + time * 0.1f) % 1f, 1f, .75f, 0);
                col *= (MathF.Sin(time * 0.1f) * 0.1f + 1f);

                float angle = MathHelper.TwoPi * i / 40f;
                float mult = (time * rand.NextFloat(0.1f, 0.2f) + i * 0.2f) % 1f;

                Vector2 pos2 = pos + angle.ToRotationVector2().RotatedBy(time * 0.01f + rand.NextFloat(-0.1f, 0.1f)) *
                    rand.NextFloat(10, 250) * mult;

                float rotation = (pos - pos2).ToRotation();

                // Draw the little petals

                Main.spriteBatch.Draw(glow, pos2, null,
                    col * MathF.Sin(mult * MathF.PI) * 0.2f, rotation + MathHelper.PiOver2,
                    glow.Size() / 2f, new Vector2(0.1f + 0.2f * (1 - mult), rand.NextFloat(0.7f, 1.1f)),
                    SpriteEffects.None,
                    0);

                if (i % 4 == 0)
                {
                    Main.spriteBatch.Draw(flare, pos, null,
                        col * MathF.Sin(mult * MathF.PI) * 0.25f, rotation + MathHelper.PiOver2,
                        flare.Size() / 2f, (1.1f - mult * mult) * 2 * rand.NextFloat(0.75f, 1.5f),
                        SpriteEffects.None, 0);


                    // Draw the tentacles
                    Vector2 tentacleEnd = pos + angle.ToRotationVector2().RotatedBy(MathF.Sin(
                        time * rand.NextFloat(0.2f, 1)) * 0.1f) * rand.NextFloat(400, 600);
                    rotation = (tentacleEnd - pos).ToRotation();
                    List<VertexPositionColorTexture> vertices = [];

                    for (float j = 0; j < 1f; j += 0.02f)
                    {
                        UnifiedRandom rand2 = new UnifiedRandom(12415 + i);
                        Vector2 tPos = Vector2.Lerp(pos, tentacleEnd, j) +
                                       new Vector2(0, MathF.Sin(j * MathHelper.TwoPi - time *
                                           rand2.NextFloat(0.2f, 1)) * 40).RotatedBy(rotation);
                        vertices.Add(PrimitiveUtils.AsVertex(
                            tPos + new Vector2(40 * (1 - j), 0).RotatedBy(rotation + MathHelper.PiOver2),
                            col * (1 - j) * 0.5f,
                            new Vector2(0, 0)));
                        vertices.Add(PrimitiveUtils.AsVertex(
                            tPos + new Vector2(40 * (1 - j), 0).RotatedBy(rotation - MathHelper.PiOver2),
                            col * (1 - j) * 0.5f,
                            new Vector2(1, 1)));
                    }

                    if (vertices.Count > 2)
                    {
                        PrimitiveUtils.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip,
                            Images.Extras.Textures.Tentacle.Value, unscaled: true);
                    }
                }
            }
        }

        // Draw lens flare
        {
            UnifiedRandom rand = new UnifiedRandom(1291144);
            for (int i = 0; i < 20f; i++)
            {
                float mult = (time * rand.NextFloat(0.1f, 0.2f) + i * 0.2f) % 1f;
                Vector2 pos2 = pos +
                               (Main.rand.NextFloat(-0.005f, 0.005f) + MathHelper.TwoPi * i / 40f +
                                MathHelper.TwoPi * 0.85f).ToRotationVector2() *
                               rand.NextFloat(250, 300 + 100 * mult);
                float rotation = (pos - pos2).ToRotation();
                Color col = Main.hslToRgb(i / 20f, 1f, .75f, 0) * 0.4f;
                Main.spriteBatch.Draw(flare2, pos2, null,
                    col * MathF.Sin(mult * MathF.PI), rotation,
                    flare2.Size() / 2f,
                    new Vector2(3f, (1.1f - mult * mult) * rand.NextFloat(2.5f, 3f)) * rand.NextFloat(.25f, .7f),
                    SpriteEffects.None, 0);

                Main.spriteBatch.Draw(flare, pos - rotation.ToRotationVector2() * rand.NextFloat(200, 250), null,
                    col * MathF.Sin(mult * MathF.PI) * 0.3f, rotation,
                    flare.Size() / 2f, new Vector2(1f, 1.1f - mult * mult) * rand.NextFloat(.5f, 1f) * mult,
                    SpriteEffects.None, 0);

                pos2 = pos + rotation.ToRotationVector2() * rand.NextFloat(200, 250 + 100 * mult);
                Main.spriteBatch.Draw(flare2, pos2, null,
                    col * MathF.Sin(mult * MathF.PI), rotation,
                    flare2.Size() / 2f, new Vector2(0.5f, (1.1f - mult * mult) * 0.8f) * rand.NextFloat(.5f, 1f),
                    SpriteEffects.None, 0);
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
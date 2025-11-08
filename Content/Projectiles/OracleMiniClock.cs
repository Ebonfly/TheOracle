using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TheOracle.Content.Dusts;

namespace TheOracle.Content.Projectiles;

public class OracleMiniClock : ModProjectile
{
    public override string Texture => "TheOracle/Assets/Images/Extras/clock";

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.scale = 0f;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 560;
        Projectile.aiStyle = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
    }

    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;

    public override void OnSpawn(IEntitySource source)
    {
    }

    public override void AI()
    {
        Projectile.localAI[0] = MathHelper.Lerp(Projectile.localAI[0], 0, 0.1f);
        Projectile.localAI[2] = MathHelper.Lerp(Projectile.localAI[2], 0, 0.1f);
        Projectile.scale = MathHelper.Lerp(Projectile.scale, 0.3f, 0.2f);
        if (Projectile.ai[0] > 420)
        {
            Projectile.ai[1] = MathHelper.Lerp(Projectile.ai[1], 0, 0.05f);
            alpha = MathHelper.Lerp(alpha, 1, 0.05f);
        }
        else if (Projectile.ai[0] > 50)
            Projectile.ai[1] = MathHelper.Lerp(Projectile.ai[1], 1, 0.05f);

        if ((int)Projectile.ai[0] == 375 && Projectile.ai[2] < 1)
        {
            if (Main.netMode != 1)
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Projectile.NewProjectile(null, Projectile.Center,
                        (angle).ToRotationVector2(),
                        ModContent.ProjectileType<OracleJetBeam>(),
                        Projectile.damage, 0);
                }
        }

        if ((int)Projectile.ai[0] == 410)
        {
            Projectile.localAI[0] = 1;
            SoundEngine.PlaySound(new SoundStyle("TheOracle/Assets/Sounds/OracleBoss/ClockBell"));
        }

        if (Projectile.ai[0] > 340 && Projectile.ai[0] < 360 && Projectile.ai[2] < 1)
        {
            Vector2 pos = Projectile.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(250, 500);
            for (int i = 0; i < 3; i++)
                Dust.NewDustPerfect(pos, ModContent.DustType<GlowDust>(),
                        (Projectile.Center - pos).SafeNormalize(Vector2.Zero) * 12,
                        newColor: Color.CornflowerBlue with { A = 0 })
                    .noGravity = true;
        }

        Projectile.ai[0]++;
        if ((int)Projectile.ai[0] == 76)
            SoundEngine.PlaySound(new SoundStyle("TheOracle/Assets/Sounds/clockTickLoop"),
                updateCallback: instance => Projectile.ai[0] - 50 < 353);

        if ((int)(Projectile.ai[0] - 50) % 30 < 2 && Projectile.ai[0] > 52 && Projectile.ai[0] < 440)
        {
            if (Main.netMode != 1 && Projectile.ai[0] < 410 && (int)(Projectile.ai[0] - 50) % 30 == 1 &&
                Projectile.ai[2] < 1)
            {
                for (int i = -1; i < 2; i += 2)
                for (int j = -1; j < 2; j++)
                    Projectile.NewProjectile(null, Projectile.Center,
                        (Projectile.localAI[1] - MathHelper.PiOver2 + j * 0.3f).ToRotationVector2() *
                        (i - MathF.Abs(j * 0.5f)),
                        ModContent.ProjectileType<OracleSkipperProjectile>(),
                        Projectile.damage, 0, ai1: Projectile.timeLeft);
            }

            Projectile.localAI[1] += MathHelper.Pi / 12f;
            Projectile.localAI[2] = 1;
        }
    }

    private float alpha;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D glow = Images.Extras.Textures.Glow.Value;
        Texture2D lensflare = Images.Extras.Textures.Lensflare.Value;
        Texture2D clock = Images.Extras.Textures.Clock.Value;
        Texture2D clockHand1 = Images.Extras.Textures.ClockHandShortAlt.Value;
        Texture2D clockHand2 = Images.Extras.Textures.ClockHandLongAlt.Value;
        Texture2D twirl = Images.Extras.Textures.Twirl[2].Value;
        Texture2D flare = Images.Extras.Textures.Crosslight.Value;

        for (int i = 0; i < 2; i++)
        {
            Color col = (i == 0 ? Color.CornflowerBlue : Color.White * 0.5f) * (Projectile.scale / 0.3f) *
                        (1 - alpha);

            Main.spriteBatch.Draw(clock, Projectile.Center - Main.screenPosition, null,
                col with { A = 0 } * Projectile.ai[1] * 0.1f, 0, clock.Size() / 2, .18f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(lensflare, Projectile.Center - Main.screenPosition, null,
                col with { A = 0 } * MathF.Pow(Projectile.localAI[0], 2), 0, lensflare.Size() / 2, 2f,
                SpriteEffects.None, 0);

            Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null,
                col with { A = 0 } * Projectile.localAI[0] * 2, 0, glow.Size() / 2, 1f, SpriteEffects.None, 0);

            for (int j = 0; j < 4; j++)
            {
                Main.spriteBatch.Draw(twirl, Projectile.Center - Main.screenPosition, null,
                    col with { A = 0 } * 0.5f * Projectile.ai[1],
                    Main.GameUpdateCount * 0.05f + MathHelper.PiOver2 * j, twirl.Size() / 2,
                    Projectile.scale * 4 + i * 0.005f,
                    SpriteEffects.None,
                    0);
                Main.spriteBatch.Draw(twirl, Projectile.Center - Main.screenPosition, null,
                    col with { A = 0 } * 0.5f * Projectile.ai[1],
                    MathHelper.PiOver2 * j, twirl.Size() / 2,
                    Projectile.scale * 4 + i * 0.005f,
                    SpriteEffects.None,
                    0);
                Main.spriteBatch.Draw(twirl, Projectile.Center - Main.screenPosition, null,
                    col with { A = 0 } * 0.5f * Projectile.localAI[0],
                    MathHelper.PiOver2 * j, twirl.Size() / 2,
                    Projectile.scale * 8 + i * 0.005f - Projectile.localAI[0] * 2.5f,
                    SpriteEffects.None,
                    0);
            }

            Main.spriteBatch.Draw(clockHand1, Projectile.Center - Main.screenPosition, null,
                col with { A = 0 } * Projectile.ai[1], Projectile.localAI[1] / 12f - MathHelper.Pi / 6f,
                clockHand1.Size() / 2,
                Projectile.scale * 0.8f + i * 0.005f,
                SpriteEffects.None, 0);

            Main.spriteBatch.Draw(clockHand2, Projectile.Center - Main.screenPosition, null,
                col with { A = 0 } * Projectile.ai[1], Projectile.localAI[1], clockHand2.Size() / 2,
                Projectile.scale * 0.8f + i * 0.005f,
                SpriteEffects.None, 0);


            Main.spriteBatch.Draw(clockHand1, Projectile.Center - Main.screenPosition, null,
                col with { A = 0 } * Projectile.localAI[2],
                Projectile.localAI[1] / 12f - MathHelper.Pi / 6f,
                clockHand1.Size() / 2,
                Projectile.scale * 0.8f,
                SpriteEffects.None, 0);
            for (float k = 0; k < 1f; k += 0.25f)
            {
                Main.spriteBatch.Draw(clockHand2, Projectile.Center - Main.screenPosition, null,
                    col with { A = 0 } * Projectile.localAI[2] * (1 - k),
                    Projectile.localAI[1] - MathHelper.Pi / 12f * k, clockHand2.Size() / 2, Projectile.scale * 0.8f,
                    SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(flare, Projectile.Center - Main.screenPosition, null, col with { A = 0 }, 0,
                flare.Size() / 2, Projectile.scale * 3 + i * 0.005f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(flare, Projectile.Center - Main.screenPosition, null, col with { A = 0 },
                MathHelper.PiOver4, flare.Size() / 2, Projectile.scale * 1.5f + i * 0.005f, SpriteEffects.None, 0);
        }

        return false;
    }
}
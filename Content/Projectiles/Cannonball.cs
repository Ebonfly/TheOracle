using System;
using System.Collections.Generic;
using TheOracle.Content.Dusts;

namespace TheOracle.Content.Projectiles;

public class Cannonball : ModProjectile
{
    public override string Texture => "TheOracle/Assets/Images/NPCs/OracleOrbs";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 15;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One * 58;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 100;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        if (Projectile.velocity.Y < 20f && Projectile.timeLeft > 60)
            Projectile.velocity.Y += 1f;

        if (Projectile.timeLeft < 55)
            Projectile.velocity.Y *= 0.97f;
    }

    public override void OnKill(int timeLeft)
    {
        Color color = Color.CornflowerBlue with { A = 0 };
        SoundEngine.PlaySound(SoundID.Item62.WithPitchOffset(Main.rand.NextFloat(-0.2f, 0.4f)), Projectile.Center);
        for (float num614 = 0f; num614 < 1f; num614 += 0.05f)
        {
            Vector2 velocity = Vector2.UnitY.RotatedBy(num614 * MathHelper.TwoPi + Main.rand.NextFloat() * 0.5f) *
                               (4f + Main.rand.NextFloat() * 4f) * Main.rand.NextFloat(0.6f, 1.5f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<ColoredFireDust>(), velocity, 255,
                color, 2f);
            dust.noGravity = true;


            Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowDust>(), velocity, 255,
                color, 0.5f);
        }

        for (int num905 = 0; num905 < 10; num905++)
        {
            int d = Dust.NewDust(Projectile.Center, 1, 1, 31, 0f, 0f, 0,
                default(Color), 2.5f);
            Main.dust[d].noGravity = true;
            Main.dust[d].velocity *= 3f;
        }

        for (int num899 = 0; num899 < 4; num899++)
        {
            Dust.NewDust(Projectile.Center, 1, 1, 31, 0f, 0f, 100, Scale: 1.5f);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;


        for (int i = 1; i < Projectile.oldPos.Length; i++)
        {
            Rectangle frame = tex.Frame(5, 6, (int)(Main.GameUpdateCount * 0.25f + i) % 4, 0);

            float mult = 1f - i / (float)Projectile.oldPos.Length;
            for (float j = 0; j < 1; j += 0.25f)
                Main.spriteBatch.Draw(tex,
                    Vector2.Lerp(Projectile.oldPos[i], Projectile.oldPos[i - 1], (1 - mult) * j) + Projectile.Size / 2 -
                    Main.screenPosition, frame,
                    Color.CornflowerBlue with { A = 0 } * mult, 0,
                    frame.Size() / 2, Projectile.scale * mult, SpriteEffects.None, 0);
        }

        return false;
    }
}
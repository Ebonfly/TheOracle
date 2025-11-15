using System;
using TheOracle.Content.Dusts;

namespace TheOracle.Content.Projectiles;

public class Explosion : ModProjectile
{
    public override string Texture => QuickAssets.EMPTY_KEY;

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One * 250;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 20;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnSpawn(IEntitySource source)
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
}
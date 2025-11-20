using System;
using TheOracle.Content.Dusts;

namespace TheOracle.Content.Projectiles;

public class OrbitalStrikePortal : ModProjectile
{
    public override string Texture => QuickAssets.EMPTY_KEY;

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.scale = 0f;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 400;
        Projectile.aiStyle = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
    }

    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => Projectile.ai[0] < 100;

    public override void AI()
    {
        if (Projectile.timeLeft == 52)
        {
            Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<Explosion>(), Projectile.damage, 0);

            for (int i = 0; i < 25; i++)
                Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center,
                    Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(5, 10),
                    ModContent.ProjectileType<Blast>(),
                    Projectile.damage, 0, ai2: 6);
        }

        Projectile.velocity *= 0.9f;

        if (Projectile.ai[0] < 60)
            Projectile.scale = MathHelper.Lerp(Projectile.scale, 1, 0.1f);

        if (Projectile.timeLeft < 60)
            Projectile.scale = MathHelper.Lerp(Projectile.scale, 0, 0.1f);

        if (Projectile.timeLeft > 120)
            for (int i = 0; i < 2; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Unit() * 380;
                Dust.NewDustPerfect(pos, ModContent.DustType<GlowDustSine>(),
                    (Projectile.Center - pos).SafeNormalize(Vector2.Zero) * 16,
                    newColor: Color.CornflowerBlue with { A = 0 } * Main.rand.NextFloat(0.2f, 1f),
                    Scale: Main.rand.NextFloat(0.7f, 1.3f)).noGravity = true;
            }

        Projectile.ai[0]++;

        if ((int)Projectile.ai[0] % 3 == 0 && Projectile.ai[0] is > 90 and < 250)
        {
            Vector2 pos = Projectile.Center + Main.rand.NextVector2CircularEdge(1200, 1200);
            Projectile.NewProjectile(Projectile.InheritSource(Projectile), pos,
                (Projectile.Center - pos).SafeNormalize(Vector2.One).RotatedBy(0.26f) * 20,
                ModContent.ProjectileType<Blast>(),
                Projectile.damage, 0, -1, Projectile.Center.X, Projectile.Center.Y, 6);
        }

        if ((int)Projectile.ai[0] % 100 == 0 && Projectile.ai[0] is > 90 and < 300)
        {
            for (int i = 0; i < 3; i++)
                Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center,
                    Vector2.UnitY.RotatedByRandom(MathHelper.Pi), ModContent.ProjectileType<JetBeam>(),
                    Projectile.damage, 0);

            Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center,
                (Main.player[(int)Projectile.ai[1]].Center - Projectile.Center).SafeNormalize(Vector2.One),
                ModContent.ProjectileType<JetBeam>(), Projectile.damage, 0);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D rift = Images.Extras.Textures.Vortex;
        Texture2D bloom = Images.Extras.Textures.Bloom;
        Texture2D twirl = Images.Extras.Textures.Twirl[0];

        for (int i = 0; i < 2; i++)
        {
            Color col = (i == 0 ? Color.CornflowerBlue : Color.White * 0.5f) with { A = 0 } * Projectile.scale;

            for (float j = -0.9f; j < 3; j += 2)
                Main.EntitySpriteDraw(rift, Projectile.Center - Main.screenPosition, null,
                    col, Main.GameUpdateCount * 0.08f * j, rift.Size() / 2f, Projectile.scale,
                    SpriteEffects.None);

            Main.EntitySpriteDraw(bloom, Projectile.Center - Main.screenPosition, null,
                col, Main.GameUpdateCount * -0.08f, bloom.Size() / 2f, Projectile.scale,
                SpriteEffects.None);


            float bloomPulse = (Main.GlobalTimeWrappedHourly * 0.5f + Projectile.whoAmI * 0.2f) % 1f;
            Main.EntitySpriteDraw(bloom, Projectile.Center - Main.screenPosition, null,
                col * MathF.Sin(bloomPulse * MathHelper.Pi) * 0.5f, Main.GameUpdateCount * -0.08f, bloom.Size() / 2f,
                Projectile.scale + bloomPulse * 0.4f, SpriteEffects.None);

            for (int j = 0; j < 2; j++)
                Main.EntitySpriteDraw(twirl, Projectile.Center - Main.screenPosition, null,
                    col, Main.GameUpdateCount * -0.2f + MathHelper.Pi * j, twirl.Size() / 2f,
                    Projectile.scale + MathF.Sin(Main.GlobalTimeWrappedHourly * 10 + j * 10) * 0.05f,
                    SpriteEffects.None);
        }

        return base.PreDraw(ref lightColor);
    }
}
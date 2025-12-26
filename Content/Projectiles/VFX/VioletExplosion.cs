using System;
using TheOracle.Content.Dusts;

namespace TheOracle.Content.Projectiles.VFX;

public class VioletExplosion : ModProjectile
{
    public override string Texture => QuickAssets.EMPTY_KEY;

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 20;
        Projectile.aiStyle = -1;
        Projectile.scale = 0;
    }

    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;

    public override void OnSpawn(IEntitySource source)
    {
        for (int i = 0; i < 50; i++)
        {
            Color col = Color.Lerp(Color.DarkViolet, Color.Purple, i / 50f);
            if (i % 2 == 0)
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SparkleDust>(),
                    Main.rand.NextVector2Circular(35, 35), newColor: col);
            else
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowDust>(),
                    Main.rand.NextVector2Circular(15, 15), newColor: col);
        }
    }

    public override void AI()
    {
        Projectile.scale = MathF.Pow(Utils.GetLerpValue(30, 0, Projectile.timeLeft), 1.5f);
        Projectile.ai[0] = MathF.Sin(Projectile.scale * MathHelper.Pi);
        if (Projectile.ai[1] > 0f)
        {
            Projectile.scale *= Projectile.ai[1];
            Projectile.ai[0] *= Projectile.ai[1];
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = Images.Extras.Textures.Lensflare;
        for (int i = 0; i < 4; i++)
            Main.EntitySpriteDraw(tex, Projectile.Center + Main.rand.NextVector2Circular(30, 30) - Main.screenPosition,
                null, Color.DarkViolet with { A = 0 } * 0.15f * Projectile.ai[0], Main.rand.NextFloat(MathHelper.Pi),
                tex.Size() / 2, Main.rand.NextFloat(0.98f, 1.02f) * Projectile.scale * 4, SpriteEffects.None);

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
            Color.White with { A = 0 } * Projectile.ai[0], 0, tex.Size() / 2,
            Projectile.scale * 2.5f * new Vector2(1, 0.25f), SpriteEffects.None);

        tex = Images.Extras.Textures.Glow;
        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
            Color.DarkViolet with { A = 0 } * 0.5f * Projectile.ai[0], 0, tex.Size() / 2,
            Projectile.scale * 4.5f, SpriteEffects.None);


        tex = Images.Extras.Textures.Crosslight;
        for (int i = 0; i < 2; i++)
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                (i == 0 ? Color.White : Color.DarkViolet) with { A = 0 } * Projectile.ai[0],
                i * MathHelper.PiOver4, tex.Size() / 2,
                Projectile.scale * (4.5f - i),
                SpriteEffects.None);
        return false;
    }
}
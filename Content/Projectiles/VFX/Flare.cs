using System;

namespace TheOracle.Content.Projectiles.VFX;

public class Flare : ModProjectile
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

    public override void AI()
    {
        float progress = Utils.GetLerpValue(20, 0, Projectile.timeLeft);
        Projectile.scale = MathF.Sin(progress * MathHelper.Pi);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = Images.Extras.Textures.Crosslight.Value;
        Texture2D flare = Images.Extras.Textures.Lensflare.Value;
        Texture2D ring = Images.Extras.Textures.Twirl[0].Value;
        float progress = Utils.GetLerpValue(20, 0, Projectile.timeLeft);
        for (int i = 0; i < 2; i++)
        {
            if (i == 0)
                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
                    Color.White with { A = 0 } * (Projectile.ai[2] == 0 ? 1 : Projectile.ai[2]) * Projectile.scale *
                    0.5f, Projectile.rotation, tex.Size() / 2f,
                    progress * (Projectile.ai[0] == 0 ? 1 : Projectile.ai[0]),
                    SpriteEffects.None, 0);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
                Color.CornflowerBlue with { A = 0 } * (Projectile.ai[2] == 0 ? 1 : Projectile.ai[2]) * Projectile.scale,
                Projectile.rotation + i * MathHelper.PiOver4, tex.Size() / 2f,
                progress * (Projectile.ai[0] == 0 ? 1 : Projectile.ai[0]) * 1.01f * (i == 0 ? 1 : 0.5f),
                SpriteEffects.None, 0);

            switch ((int)Projectile.ai[1])
            {
                case 1:
                    Main.spriteBatch.Draw(ring, Projectile.Center - Main.screenPosition, null,
                        Color.CornflowerBlue with { A = 0 } * (Projectile.ai[2] == 0 ? 1 : Projectile.ai[2]) *
                        Projectile.scale, Projectile.rotation + MathHelper.Pi * i - progress * MathHelper.Pi,
                        ring.Size() / 2f,
                        progress * (Projectile.ai[0] == 0 ? 1 : Projectile.ai[0]) * 0.35f, SpriteEffects.None, 0);
                    break;
            }
        }

        Main.spriteBatch.Draw(flare, Projectile.Center - Main.screenPosition, null,
            Color.White with { A = 0 } * (Projectile.ai[2] == 0 ? 1 : Projectile.ai[2]) *
            MathF.Pow(Projectile.scale, 4), Projectile.rotation, flare.Size() / 2f,
            progress * (Projectile.ai[0] == 0 ? 1 : Projectile.ai[0]) * 0.5f,
            SpriteEffects.None, 0);

        return false;
    }
}
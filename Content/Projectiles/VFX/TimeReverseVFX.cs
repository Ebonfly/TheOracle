using System;

namespace TheOracle.Content.Projectiles.VFX;

public class TimeReverseVFX : ModProjectile
{
    public override string Texture => QuickAssets.EMPTY_KEY;

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 160;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        if (Projectile.timeLeft is > 38 and < 120 && Projectile.ai[0] < 1)
        {
            Projectile.localAI[0] = MathF.Sin(Utils.GetLerpValue(120, 40, Projectile.timeLeft) * MathHelper.Pi);
        }

        if ((Projectile.ai[0] < 1
                ? Projectile.localAI[1] > -MathHelper.TwoPi
                : Projectile.localAI[1] < MathHelper.TwoPi) && Projectile.timeLeft < 108)
            Projectile.localAI[1] -=
                MathHelper.TwoPi / 70f * (Projectile.ai[0] < 1 ? 1 : -1);

        if (Projectile.timeLeft == 108)
        {
            Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Flare>(), 0, 0,
                -1, 2.5f, ai2: 0.2f);

            if (Projectile.ai[0] >= 1)
                SoundEngine.PlaySound(new SoundStyle("TheOracle/Assets/Sounds/clockMagicBurst").WithVolumeScale(3)
                    .WithPitchOffset(.4f));
            else
                SoundEngine.PlaySound(new SoundStyle("TheOracle/Assets/Sounds/clockMagicBurstReverse")
                    .WithVolumeScale(3)
                    .WithPitchOffset(.4f));
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D glow = Assets.Extras.glow.Value;
        Texture2D clock = Assets.Extras.clock_premult.Value;
        Texture2D clockHand1 = Assets.Extras.clockHand1_premult.Value;
        Texture2D clockHand2 = Assets.Extras.clockHand2_premult.Value;

        float alpha = MathF.Sin(Utils.GetLerpValue(160, 0, Projectile.timeLeft) *
                                MathHelper.Pi);
        Vector2 RandPos() => Main.rand.NextVector2Circular(alpha, alpha) * 15 * (Projectile.ai[0] < 1 ? 1 : 0);
        for (int i = 0; i < 2; i++)
        {
            Color col = (i == 0 ? Color.CornflowerBlue : Color.White * 0.5f) * alpha * 0.5f;

            for (int j = 0; j < 4; j++)
                Main.spriteBatch.Draw(clock, Projectile.Center + RandPos() - Main.screenPosition, null,
                    col with { A = 0 } * (Projectile.ai[0] < 1 ? 1 : 0.5f), 0, clock.Size() / 2, .28f,
                    SpriteEffects.None, 0);

            Main.spriteBatch.Draw(glow, Projectile.Center + RandPos() - Main.screenPosition, null,
                col with { A = 0 } * 0.1f, 0, glow.Size() / 2, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(clockHand1, Projectile.Center + RandPos() - Main.screenPosition, null,
                col with { A = 0 }, Projectile.localAI[1] / 12f,
                clockHand1.Size() / 2,
                Projectile.scale * 0.4f + i * 0.005f,
                SpriteEffects.None, 0);

            Main.spriteBatch.Draw(clockHand2, Projectile.Center + RandPos() - Main.screenPosition, null,
                col with { A = 0 }, Projectile.localAI[1], clockHand2.Size() / 2,
                Projectile.scale * 0.4f + i * 0.005f,
                SpriteEffects.None, 0);


            Main.spriteBatch.Draw(clockHand1, Projectile.Center - Main.screenPosition, null,
                col with { A = 0 } * Projectile.localAI[0],
                Projectile.localAI[1] / 12f - MathHelper.Pi / 100f,
                clockHand1.Size() / 2,
                Projectile.scale * 0.4f,
                SpriteEffects.None, 0);
            for (float k = 0; k < 1f; k += 0.25f)
            {
                Main.spriteBatch.Draw(clockHand2, Projectile.Center - Main.screenPosition, null,
                    col with { A = 0 } * (1 - k) * Projectile.localAI[0],
                    Projectile.localAI[1] - MathHelper.Pi / 12f * k, clockHand2.Size() / 2, Projectile.scale * 0.4f,
                    SpriteEffects.None, 0);
            }
        }

        return false;
    }
}
using System;
using TheOracle.Content.Dusts;

namespace TheOracle.Content.Projectiles;

public class OracleBladeClock : ModProjectile
{
    public override string Texture => "TheOracle/Assets/Images/Extras/clock";

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.scale = 0f;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 930;
        Projectile.aiStyle = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override bool? CanDamage()
    {
        return base.CanDamage();
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI()
    {
        Projectile.localAI[0] = MathHelper.Lerp(Projectile.localAI[0], 0, 0.1f);
        Projectile.localAI[2] = MathHelper.Lerp(Projectile.localAI[2], 0, 0.1f);

        Projectile.ai[0]++;
        if (Projectile.ai[0] < 10)
        {
            Projectile.scale = MathHelper.Lerp(Projectile.scale, 0.3f, 0.2f);
            _clockHandRotation = -MathHelper.PiOver2;
        }

        if (Main.rand.NextBool(4))
        {
            Vector2 pos = Projectile.Center;
            Vector2 vel = Main.rand.NextVector2Unit();
            Dust.NewDustPerfect(pos, ModContent.DustType<GlowDustSine>(),
                vel * Main.rand.NextFloat(10, 15),
                newColor: Color.CornflowerBlue);
        }

        if (Projectile.ai[0] is > 10 and < 150)
        {
            for (int i = 0; i < (int)MathHelper.Clamp(Projectile.ai[0] / 20, 0, 4); i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2CircularEdge(400, 400) +
                              Main.rand.NextVector2Circular(50, 50);
                Vector2 vel = (Projectile.Center - pos).SafeNormalize(Vector2.UnitX);
                if (Main.rand.NextBool())
                {
                    pos = Projectile.Center;
                    vel = Main.rand.NextVector2Unit();
                }

                if (i < 2)
                    Dust.NewDustPerfect(pos, ModContent.DustType<GlowDustSine>(),
                        vel * Main.rand.NextFloat(10, 15),
                        newColor: Color.CornflowerBlue);
                else
                    Dust.NewDustPerfect(pos, ModContent.DustType<GlowDust>(),
                        vel * Main.rand.NextFloat(5, 10),
                        newColor: Color.CornflowerBlue);
            }
        }

        if (Projectile.ai[0] > 110 && _additionalFlare < 1f)
            _additionalFlare += 0.014f;

        if ((int)Projectile.ai[0] == 170)
        {
            Projectile.NewProjectile(null, Projectile.Center,
                (_clockHandRotation).ToRotationVector2(),
                ModContent.ProjectileType<OracleJetBeam>(), Projectile.damage, 0, ai2: 1);
            SoundEngine.PlaySound(new SoundStyle("TheOracle/Assets/Sounds/sliceMagic").WithVolumeScale(3)
                .WithPitchOffset(-.5f));
        }

        if (Projectile.ai[0] is > 164 and < 167)
        {
            _alpha = 1f;
            Projectile.localAI[0] = 1f;
        }


        if ((int)(Projectile.ai[0] - 50) % 30 < 2 && Projectile.ai[0] is > 190 and <= 910)
        {
            if (Main.netMode != 1 && (int)(Projectile.ai[0] - 50) % 30 == 1 && Projectile.ai[0] <= 897)
            {
                Projectile.NewProjectile(null, Projectile.Center,
                    (_clockHandRotation + MathHelper.Pi / 12f).ToRotationVector2(),
                    ModContent.ProjectileType<OracleJetBeam>(), Projectile.damage, 0, ai2: 1);
            }

            _clockHandRotation += MathHelper.Pi / 12f;
            Projectile.localAI[2] = 1;
        }

        if (Projectile.ai[0] > 910)
        {
            _alpha = MathHelper.Lerp(_alpha, 0, 0.1f);
            Projectile.scale = MathHelper.Lerp(Projectile.scale, 0f, 0.1f);
        }
    }

    private float _alpha, _clockHandRotation, _additionalFlare;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D glow = Images.Extras.Textures.Glow.Value;
        Texture2D lensflare = Images.Extras.Textures.Lensflare.Value;
        Texture2D clock = Images.Extras.Textures.Clock.Value;
        Texture2D clockHand1 = Images.Extras.Textures.ClockHandShort.Value;
        Texture2D clockHand2 = Images.Extras.Textures.ClockHandLong.Value;
        Texture2D twirl = Images.Extras.Textures.Twirl[0].Value;
        Texture2D flare = Images.Extras.Textures.Crosslight.Value;

        for (int i = 0; i < 2; i++)
        {
            Color col = (i == 0 ? Color.CornflowerBlue : Color.White * 0.5f) * (Projectile.scale / 0.3f);
            col.A = 0;

            Main.spriteBatch.Draw(flare, Projectile.Center - Main.screenPosition, null, col,
                0,
                flare.Size() / 2, Projectile.scale * 5 + i * 0.005f + _alpha, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(flare, Projectile.Center - Main.screenPosition, null, col,
                MathHelper.Pi / 6f, flare.Size() / 2,
                Projectile.scale * 3.5f + i * 0.005f + _alpha * 0.5f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(flare, Projectile.Center - Main.screenPosition, null, col,
                MathHelper.Pi / 3f, flare.Size() / 2,
                Projectile.scale * 2.5f + i * 0.005f + _alpha * 0.5f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(flare,
                Projectile.Center - Main.screenPosition + Main.rand.NextVector2Circular(15, 15) * _additionalFlare,
                null,
                col * MathF.Sin(_additionalFlare * MathHelper.Pi) * 0.5f,
                Main.GlobalTimeWrappedHourly + MathHelper.PiOver4,
                flare.Size() / 2, Projectile.scale * 3 + i * 0.005f + _additionalFlare * 2, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(flare,
                Projectile.Center - Main.screenPosition + Main.rand.NextVector2Circular(15, 15) * _additionalFlare,
                null, col * MathF.Sin(_additionalFlare * MathHelper.Pi) * 0.5f,
                Main.GlobalTimeWrappedHourly, flare.Size() / 2,
                Projectile.scale * 1.5f + i * 0.005f + _additionalFlare * 4,
                SpriteEffects.None, 0);

            col *= _alpha;


            Main.spriteBatch.Draw(lensflare, Projectile.Center - Main.screenPosition, null,
                col with { A = 0 } * MathF.Pow(Projectile.localAI[0], 2), 0, lensflare.Size() / 2, 3f,
                SpriteEffects.None, 0);

            Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null,
                col with { A = 0 } * Projectile.localAI[0] * 2, 0, glow.Size() / 2, 2f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(clockHand2, Projectile.Center - Main.screenPosition, null,
                col with { A = 0 }, _clockHandRotation, new Vector2(84, clockHand2.Height / 2f),
                Projectile.scale * 1.2f, SpriteEffects.None, 0);

            for (float k = 0; k < 1f; k += 0.25f)
            {
                Main.spriteBatch.Draw(clockHand2, Projectile.Center - Main.screenPosition, null,
                    col with { A = 0 } * Projectile.localAI[2] * (1 - k),
                    _clockHandRotation - MathHelper.Pi / 12f * k, new Vector2(84, clockHand2.Height / 2f),
                    Projectile.scale * 1.2f, SpriteEffects.None, 0);
            }
        }


        return false;
    }
}
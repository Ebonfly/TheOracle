using System;
using TheOracle.Common.Utils;

namespace TheOracle.Content.Projectiles;

public class OracleBlast : ModProjectile
{
    public override string Texture => "TheOracle/Assets/Images/Projectiles/OracleBlast";

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 4;
        ProjectileID.Sets.TrailCacheLength[Type] = 10;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.aiStyle = -1;
        Projectile.Size = new(33);
        Projectile.timeLeft = 700;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        // V
        if ((int)Projectile.ai[2] == 5)
        {
            Rectangle rect = new Rectangle(906, 1081, 148, 180);
            Texture2D clock = Images.Extras.Textures.Clock.Value;
            int dir = Projectile.velocity.X > 0 ? 1 : -1;
            SpriteEffects effect = dir == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float rot = Projectile.rotation + MathHelper.PiOver2 * 1.43f;
            if (dir == -1)
                rot -= MathHelper.PiOver2 * .84f;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float mult = 1f - i / (float)Projectile.oldPos.Length;
                Main.spriteBatch.Draw(clock, Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition, rect,
                    Color.CornflowerBlue with { A = 0 } * mult, rot, new Vector2(74, 90),
                    Projectile.scale * 0.3f * mult, effect, 0);
            }

            for (int i = 0; i < 2; i++)
            {
                Color col = (i == 0 ? Color.White * 0.4f : Color.CornflowerBlue) with { A = 0 };
                Main.spriteBatch.Draw(clock, Projectile.Center - Main.screenPosition, rect, col,
                    rot, new Vector2(74, 90), Projectile.scale * 0.3f, effect, 0);
            }

            return false;
        }

        Texture2D tex = TextureAssets.Projectile[Type].Value;

        if ((int)Projectile.ai[2] == 4)
        {
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.spriteBatch.Draw(tex, Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition,
                    tex.Frame(1, 4, 0, Projectile.frame),
                    Color.CornflowerBlue with { A = 0 } * (1f - i / (float)Projectile.oldPos.Length),
                    Projectile.oldRot[i],
                    new Vector2(65, 35) / 2, Projectile.scale,
                    SpriteEffects.None, 0);
            }
        }

        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(1, 4, 0, Projectile.frame),
            Color.White with { A = 0 }, Projectile.rotation, new Vector2(65, 35) / 2, Projectile.scale,
            SpriteEffects.None, 0);

        if ((int)Projectile.ai[2] == 4)
        {
            Texture2D flare = Images.Extras.Textures.Crosslight.Value;
            Texture2D lensflare = Images.Extras.Textures.Lensflare.Value;
            for (int j = 0; j < 2; j++)
            {
                Main.spriteBatch.Draw(lensflare, Projectile.Center - Main.screenPosition, null,
                    Color.CornflowerBlue with { A = 0 } * (1 - Projectile.localAI[0]), 0,
                    lensflare.Size() / 2, 0.5f,
                    SpriteEffects.None, 0);
                for (int i = 0; i < 2; i++)
                    Main.spriteBatch.Draw(flare, Projectile.Center - Main.screenPosition, null,
                        Color.CornflowerBlue with { A = 0 } * (1 - Projectile.localAI[0]), MathHelper.PiOver4 * i,
                        flare.Size() / 2, 0.9f - i * 0.4f,
                        SpriteEffects.None, 0);
            }
        }

        return false;
    }

    private Vector2 _initVel, _initPos;

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, TorchID.White);
        if (++Projectile.frameCounter % 5 == 0)
            if (++Projectile.frame > 3)
                Projectile.frame = 0;


        switch ((int)Projectile.ai[2])
        {
            // Curve
            case 1:
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity = Projectile.velocity.RotatedBy(0.01f * Projectile.ai[1]) * 1.01f;
                break;

            // Delayed + Curve
            case 2:
                Projectile.rotation = Projectile.velocity.ToRotation();
                if (Projectile.timeLeft > 290)
                    Projectile.velocity = Projectile.velocity.RotatedBy(0.01f * Projectile.ai[1]) * 1.01f;
                break;

            // Sine
            case 3:
                if (_initPos == Vector2.Zero)
                {
                    _initPos = Projectile.Center;
                    _initVel = Projectile.velocity;
                }
                else
                {
                    Projectile.SineMovement(_initPos, _initPos, 0.05f, 100);

                    float wave = MathF.Sin(Projectile.ai[1] * 0.05f);
                    Vector2 vector = _initVel.RotatedBy(MathHelper.ToRadians(90));
                    vector.Normalize();
                    wave *= Projectile.ai[0] * 100;
                    Vector2 offset = vector * wave;
                    Projectile.rotation = ((Projectile.velocity + offset * 0.1f) * Projectile.ai[1] -
                                           (Projectile.velocity + offset * 0.1f) * (Projectile.ai[1] - 10))
                        .ToRotation();
                }

                break;

            // Hourglass
            case 4:
                Projectile.rotation = Projectile.velocity.ToRotation();

                Projectile.localAI[0] = MathHelper.Lerp(Projectile.localAI[0], 1, 0.2f);

                Projectile.ai[1]++;
                if (Projectile.ai[1] is > 180 and < 461)
                    Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.Pi / 280f);
                break;

            // V
            case 5:
                if (Projectile.ai[0] is < 0 or > Main.maxPlayers)
                    return;
                Player target = Main.player[(int)Projectile.ai[0]];
                if (Projectile.velocity.Length() > 0f)
                    Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.ai[1]++;
                if (Projectile.ai[1] < 120)
                    Projectile.velocity *= 0.99f;
                else if (Projectile.ai[1] < 200 && target.active)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity,
                        (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) *
                        MathHelper.Lerp(10, 20, (Projectile.ai[1] - 120f) / 80f), 0.04f);
                }
                else if (Projectile.velocity.Length() < 20f)
                    Projectile.velocity *= 1.1f;

                break;
            default:
                Projectile.rotation = Projectile.velocity.ToRotation();
                break;
        }
    }
}
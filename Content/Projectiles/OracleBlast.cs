using System;
using TheOracle.Common.Utils;

namespace TheOracle.Content.Projectiles;

public class OracleBlast : ModProjectile
{
    public override string Texture => "TheOracle/Assets/Images/Projectiles/OracleBlast";

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 4;
    }

    public override void SetDefaults()
    {
        Projectile.aiStyle = -1;
        Projectile.Size = new(33);
        Projectile.timeLeft = 360;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(1, 4, 0, Projectile.frame),
            Color.White with { A = 0 }, Projectile.rotation, new Vector2(65, 35) / 2, Projectile.scale,
            SpriteEffects.None, 0);
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
            case 1:
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity = Projectile.velocity.RotatedBy(0.01f * Projectile.ai[1]) * 1.01f;
                break;
            case 2:
                Projectile.rotation = Projectile.velocity.ToRotation();
                if (Projectile.timeLeft > 290)
                    Projectile.velocity = Projectile.velocity.RotatedBy(0.01f * Projectile.ai[1]) * 1.01f;
                break;
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
            default:
                Projectile.rotation = Projectile.velocity.ToRotation();
                break;
        }
    }
}
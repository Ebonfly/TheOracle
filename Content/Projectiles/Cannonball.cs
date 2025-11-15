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
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 100;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        if (Projectile.velocity.Y < Projectile.ai[0] && Projectile.timeLeft > 60)
            Projectile.velocity.Y += 1f;

        if (Projectile.timeLeft < 55)
            Projectile.velocity.Y *= 0.97f;
    }

    public override void OnKill(int timeLeft)
    {
        Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, Vector2.Zero,
            ModContent.ProjectileType<Explosion>(), Projectile.damage, 0);
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
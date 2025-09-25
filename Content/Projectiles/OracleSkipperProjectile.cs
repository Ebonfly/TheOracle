using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TheOracle.Content.Dusts;

namespace TheOracle.Content.Projectiles;

public class OracleSkipperProjectile : ModProjectile
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

    public override void OnKill(int timeLeft)
    {
        base.OnKill(timeLeft);
    }

    public override void OnSpawn(IEntitySource source)
    {
        base.OnSpawn(source);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(1, 4, 0, Projectile.frame),
            Color.White with { A = 0 }, Projectile.rotation, new Vector2(65, 35) / 2, Projectile.scale,
            SpriteEffects.None, 0);
        return false;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, TorchID.White);
        if (++Projectile.frameCounter % 5 == 0)
            if (++Projectile.frame > 3)
                Projectile.frame = 0;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.ai[0]++;
        Projectile.ai[1]--;
        if ((int)Projectile.ai[0] % 30 < 2 && Projectile.velocity.Length() < 25f && Projectile.ai[1] > 0)
            Projectile.velocity =
                new Vector2(Projectile.velocity.Length() + 1f, 0).RotatedBy(Projectile.velocity.ToRotation() +
                                                                            MathHelper.Pi / 24f);
    }
}
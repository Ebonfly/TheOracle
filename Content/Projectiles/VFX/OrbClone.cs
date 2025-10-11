using System.Collections.Generic;

namespace TheOracle.Content.Projectiles.VFX;

public class OrbClone : ModProjectile
{
    public override string Texture => "TheOracle/Assets/Images/NPCs/OracleOrbs";

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 30;
        Projectile.aiStyle = -1;
        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
        List<int> behindProjectiles, List<int> overPlayers,
        List<int> overWiresUI)
    {
        behindNPCs.Add(index);
    }

    public override void AI()
    {
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;

        Rectangle frame = tex.Frame(5, 6, 0, 0);
        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame,
            Color.White with { A = 0 } * Utils.GetLerpValue(0, 30, Projectile.timeLeft), 0,
            frame.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

        return false;
    }
}
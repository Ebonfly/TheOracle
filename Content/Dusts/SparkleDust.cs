namespace TheOracle.Content.Dusts;

public class SparkleDust : ModDust
{
    public override string Texture => "TheOracle/Assets/Images/Extras/crosslight";

    public override void OnSpawn(Dust dust)
    {
        dust.alpha = 0;
        dust.noLight = true;
        dust.noGravity = true;
        if (dust.scale is > 0.99f and < 1.01f)
            dust.scale = Main.rand.NextFloat(0.95f, 1.05f);
    }

    public override bool Update(Dust dust)
    {
        if (dust.alpha < 255)
            dust.alpha += 17;
        dust.position += dust.velocity;
        dust.velocity *= 0.9f;
        dust.scale *= 0.98f;
        if (dust.scale < 0.1f)
            dust.scale -= 0.01f;
        if (dust.scale <= 0)
            dust.active = false;

        return false;
    }

    public override bool PreDraw(Dust dust)
    {
        Texture2D tex = Images.Extras.Textures.Crosslight;
        Vector2 scale = new Vector2(5f, 2.5f + MathHelper.Clamp(dust.velocity.Length(), 0, 2.5f));
        Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null,
            dust.color with { A = 0 } * dust.scale * (float)(dust.alpha / 255f), dust.velocity.ToRotation(),
            tex.Size() / 2,
            scale * dust.scale * 0.1f, SpriteEffects.None,
            0);
        return false;
    }
}
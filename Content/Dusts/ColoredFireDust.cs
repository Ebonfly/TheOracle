namespace TheOracle.Content.Dusts;

public class ColoredFireDust : ModDust
{
    public override string Texture => QuickAssets.EMPTY_KEY;

    public override void OnSpawn(Dust dust)
    {
        dust.alpha = 255;
        dust.noLight = true;
        dust.noGravity = true;
        dust.customData = dust.scale;
        base.OnSpawn(dust);
    }

    public override bool Update(Dust dust)
    {
        dust.position += dust.velocity;
        dust.rotation = dust.velocity.ToRotation();
        dust.scale -= 0.04f;
        dust.scale *= 0.96f;
        dust.velocity *= 0.99f;
        if (dust.scale <= 0)
            dust.active = false;
        return false;
    }

    public override bool PreDraw(Dust d)
    {
        Texture2D tex = Images.Extras.Textures.Fire.Value;
        float alpha = d.scale / (float)d.customData * 0.25f;
        Main.spriteBatch.Draw(tex, d.position - Main.screenPosition, null, d.color * (alpha * 5), d.rotation,
            tex.Size() / 2, d.scale * 0.6f, SpriteEffects.None, 0);
        Main.spriteBatch.Draw(tex, d.position - Main.screenPosition, null, Color.White with { A = 0 } * (alpha * .1f),
            d.rotation,
            tex.Size() / 2, d.scale * 0.6f, SpriteEffects.None, 0);
        return false;
    }
}
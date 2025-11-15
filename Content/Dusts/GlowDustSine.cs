using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace TheOracle.Content.Dusts;

public class GlowDustSine : ModDust
{
    public override string Texture => "TheOracle/Assets/Images/Extras/Bloom";

    public override void OnSpawn(Dust dust)
    {
        dust.alpha = 255;
        dust.noLight = true;
        dust.noGravity = true;
        if (dust.scale is > 0.99f and < 1.01f)
            dust.scale = Main.rand.NextFloat(0.95f, 1.05f);
    }

    public override bool Update(Dust dust)
    {
        if (dust.customData is null)
            dust.customData = 0f;
        dust.customData = (float)dust.customData + 0.1f;
        dust.position += dust.velocity.RotatedBy(MathF.Sin((float)dust.customData) * 0.4f);
        dust.velocity *= 0.965f;
        dust.scale *= 0.99f;
        if (dust.scale < 0.1f)
            dust.scale -= 0.01f;
        if (dust.scale <= 0)
            dust.active = false;

        return false;
    }

    public override bool PreDraw(Dust dust)
    {
        Texture2D tex = Images.Extras.Textures.Slash.Value;
        Vector2 scale = new Vector2(MathHelper.Clamp(dust.velocity.Length(), 0, 5f), 5f);
        float rotation = dust.velocity.RotatedBy(MathF.Sin((float)dust.customData) * 0.4f).ToRotation();
        Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null,
            dust.color with { A = 0 } * dust.scale * MathHelper.Clamp((float)dust.customData, 0, 1), rotation,
            tex.Size() / 2,
            scale * dust.scale * 0.1f, SpriteEffects.None,
            0);
        return false;
    }
}
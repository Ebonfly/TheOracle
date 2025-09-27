using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.Utilities;
using TheOracle.Common.Utils;
using TheOracle.Content.Dusts;

namespace TheOracle.Content.Projectiles;

public class CrystalSlice : ModProjectile
{
    public override string Texture => QuickAssets.EMPTY_KEY;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1000;
    }

    public override void SetDefaults()
    {
        Projectile.Size = new(16);
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 80;
        Projectile.aiStyle = -1;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.friendly = false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float a = 0;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
            Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 100,
            Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 1200, 10, ref a);
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool? CanDamage() => Projectile.timeLeft is > 20 and < 40;

    public override void AI()
    {
        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Projectile.rotation = Projectile.velocity.ToRotation();
        if (Projectile.timeLeft > 40)
            Projectile.ai[0] = MathHelper.Lerp(1, 0, MathHelper.Clamp((Projectile.timeLeft - 60) / 20f, 0, 1));
        else if (Projectile.timeLeft > 10)
        {
            if (Projectile.timeLeft < 15)
                for (int i = 0; i < 3; i++)
                    Dust.NewDustPerfect(Projectile.Center +
                                        Projectile.velocity * Main.rand.NextFloat(100, 1000) +
                                        Main.rand.NextVector2Circular(4, 4), ModContent.DustType<GlowDust>(),
                        Projectile.velocity.RotatedByRandom(.2f) * Main.rand.NextFloat(1, 3),
                        newColor: Color.White with { A = 0 } * 0.6f).noGravity = true;

            Projectile.ai[1] = MathHelper.SmoothStep(1, 0, (Projectile.timeLeft - 10) / 30f);
            Projectile.ai[0] = MathHelper.Lerp(0, 1, (Projectile.timeLeft - 10) / 30f);
        }
        else if (Projectile.timeLeft < 8)
            Projectile.ai[1] = MathHelper.Lerp(0, 1, Projectile.timeLeft / 7f);

        if (Projectile.timeLeft < 15)
        {
            Projectile.ai[2] = MathHelper.Lerp(1, 0, Projectile.timeLeft / 15f);
        }

        if (Projectile.timeLeft == 40)
        {
            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { pitchVariance = 0.5f },
                Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 400);
            SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot with { pitchVariance = 0.5f },
                Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 400);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = Assets.Extras.Tentacle.Value;

        Texture2D crystalTex = Assets.NPCs.OracleCrystal.Value;
        Rectangle crystalFrame = new Rectangle(0, 0, 50, 92);

        List<VertexPositionColorTexture> vertices = new();
        List<VertexPositionColorTexture> verticesTelegraph = new();
        Vector2 start = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 100;
        Vector2 end = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 1700;
        start -= Main.screenPosition;
        end -= Main.screenPosition;
        for (float i = 0; i <= 1f; i += 0.05f)
        {
            float factor = MathF.Sin(i * MathHelper.Pi) * (1 - i) * 2;
            Vector2 position = Vector2.Lerp(start, end,
                i * (Projectile.timeLeft > 20 ? MathHelper.Clamp(Projectile.ai[1] * 3, 0, 1) : 1));
            Color color = Color.Lerp(Color.CornflowerBlue, Color.White, factor) with { A = 0 };

            Main.spriteBatch.Draw(crystalTex,
                position + Projectile.velocity * 750 - Projectile.velocity * Projectile.timeLeft * 40, crystalFrame,
                color * i * Projectile.ai[1] * Projectile.ai[0] * 4, Projectile.rotation + MathHelper.PiOver2,
                crystalFrame.Size() / 2, new Vector2(1, 3 + i), SpriteEffects.None, 0);

            color *= factor * MathF.Pow(Projectile.ai[1], 2) * 0.5f * (1 - Projectile.ai[2]);

            for (int j = 0; j < 2; j++)
                vertices.Add(PrimitiveUtils.AsVertex(
                    position + new Vector2(4, 0).RotatedBy(
                        Projectile.rotation + MathHelper.PiOver2 * (j == 0 ? -1 : 1)),
                    color * 3, new Vector2(j, j)));

            position = Vector2.Lerp(start, end, i);
            color = Color.Lerp(Color.CornflowerBlue, Color.White, factor) with { A = 0 };
            color *= factor * Projectile.ai[0] * 0.6f;
            for (int j = 0; j < 2; j++)
                verticesTelegraph.Add(PrimitiveUtils.AsVertex(
                    position + new Vector2(2, 0).RotatedBy(
                        Projectile.rotation + MathHelper.PiOver2 * (j == 0 ? -1 : 1)),
                    color, new Vector2(j, j)));
        }

        if (vertices.Count > 2 && verticesTelegraph.Count > 2)
        {
            Main.spriteBatch.End(out var ss);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, ss.blendState, SamplerState.PointWrap,
                ss.depthStencilState, RasterizerState.CullNone, null, ss.matrix);

            PrimitiveUtils.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, tex);
            PrimitiveUtils.DrawTexturedPrimitives(verticesTelegraph.ToArray(), PrimitiveType.TriangleStrip, tex);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(ss);
        }

        return false;
    }
}
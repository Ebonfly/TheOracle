using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using TheOracle.Common.Utils;
using TheOracle.Content.Dusts;

namespace TheOracle.Content.Projectiles;

public class OracleJetBeam : ModProjectile
{
    public override string Texture => QuickAssets.EMPTY_KEY;

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
        Projectile.scale = 0f;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float a = 0;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
            Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 1200, 40, ref a);
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool? CanDamage() => Projectile.timeLeft < 40;

    public override void AI()
    {
        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Projectile.scale = MathHelper.Lerp(Projectile.scale, 1, 0.1f);
        if (Projectile.scale > 0.9f && Projectile.timeLeft < 40)
        {
            Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 1, 0.1f);
        }

        if (Projectile.ai[0] > 0.95f)
        {
            Dust.NewDustPerfect(Projectile.Center +
                                Projectile.velocity * Main.rand.NextFloat(100, 1000 * Projectile.ai[0]) +
                                Main.rand.NextVector2Circular(30, 30), ModContent.DustType<GlowDust>(),
                Projectile.velocity.RotatedByRandom(.4f) * Main.rand.NextFloat(1, 3),
                newColor: Color.CornflowerBlue with { A = 0 }).noGravity = true;

            Projectile.ai[1] = MathHelper.Lerp(Projectile.ai[1], 1, 0.15f);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = Assets.Extras.wavyLaser.Value;
        Texture2D tex2 = Assets.Extras.Tentacle.Value;
        List<VertexPositionColorTexture> vertices = new();
        Vector2 start = Projectile.Center - Main.screenPosition;
        Vector2 end = Projectile.Center + Projectile.velocity
            * MathHelper.Lerp(300, 1200, Projectile.ai[0]) * Projectile.scale - Main.screenPosition;
        for (float i = 0; i < 1f; i += 0.05f)
        {
            Color color = Color.Lerp(Color.CornflowerBlue, Color.Transparent, MathHelper.SmoothStep(0, 1, i)) *
                          MathHelper.Clamp(i * 4, 0, 1) * 2 * (1 - Projectile.ai[1]);
            vertices.Add(PrimitiveUtils.AsVertex(Vector2.Lerp(start, end, i) +
                                                 new Vector2((i + 0.5f) * 20 + Projectile.ai[0] * 20, 0).RotatedBy(
                                                     Projectile.rotation + MathHelper.PiOver2),
                color, new Vector2(-2 * i * Projectile.ai[0] + Main.GlobalTimeWrappedHourly * 3, 0)));

            vertices.Add(PrimitiveUtils.AsVertex(Vector2.Lerp(start, end, i) +
                                                 new Vector2((i + 0.5f) * 20 + Projectile.ai[0] * 20, 0).RotatedBy(
                                                     Projectile.rotation - MathHelper.PiOver2),
                color, new Vector2(-2 * i * Projectile.ai[0] + Main.GlobalTimeWrappedHourly * 3, 1)));
        }

        if (vertices.Count > 2)
        {
            Main.spriteBatch.End(out var ss);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap,
                ss.depthStencilState, RasterizerState.CullNone, null, ss.matrix);

            PrimitiveUtils.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, tex, false);
            PrimitiveUtils.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, tex2, false);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(ss);
        }

        return false;
    }
}
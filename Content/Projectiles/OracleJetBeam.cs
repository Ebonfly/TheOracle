using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using TheOracle.Common.Utils;
using TheOracle.Content.Dusts;
using TheOracle.Content.Projectiles.VFX;

namespace TheOracle.Content.Projectiles;

public class OracleJetBeam : ModProjectile
{
    public override string Texture => QuickAssets.EMPTY_KEY;

    public override void SetDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;
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
            Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) *
            ((int)Projectile.ai[2] == 1 ? 2700 : 1200), 40, ref a);
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool? CanDamage() => Projectile.timeLeft is > 20 and < 40;

    public override void AI()
    {
        if ((int)Projectile.ai[2] == 1)
        {
            if (Projectile.timeLeft >= 45)
            {
                Projectile.localAI[0] = 0.5f;
                Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 0.2f, 0.1f);
            }

            if (Projectile.timeLeft == 50)
                Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Flare>(),
                    0,
                    0);

            if (Projectile.timeLeft > 40 && Projectile.timeLeft < 45)
            {
                Projectile.scale = 1;
                Projectile.ai[0] = 1;
                Projectile.localAI[0] = 0;
                Projectile.timeLeft = 40;
                if (!Main.dedServ)
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center,
                        Projectile.velocity,
                        3f, 20, 15));

                for (int j = 0; j < 5; j++)
                {
                    Vector2 vel = Projectile.velocity.RotatedByRandom(1.5f);
                    Dust.NewDustPerfect(Projectile.Center + vel.RotatedByRandom(0.1f) * 32,
                        ModContent.DustType<GlowDust>(),
                        vel * Main.rand.NextFloat(5, 8), 0,
                        Color.CornflowerBlue with { A = 0 });
                }

                return;
            }
        }

        if ((int)Projectile.ai[2] == 1)
            Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 0, 0.1f);

        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Projectile.scale = MathHelper.Lerp(Projectile.scale, 1, 0.1f);
        if (Projectile.scale > 0.9f && Projectile.timeLeft < 40 && Projectile.ai[2] < 1)
        {
            Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 1, 0.1f);
        }

        if ((int)Projectile.ai[2] == 1)
        {
            if (Projectile.timeLeft < 15)
                Projectile.localAI[0] = MathHelper.Lerp(Projectile.localAI[0], 1, 0.15f);
        }
        else if (Projectile.ai[0] > 0.95f)
        {
            Dust.NewDustPerfect(Projectile.Center +
                                Projectile.velocity * Main.rand.NextFloat(100, 1000 * Projectile.ai[0]) +
                                Main.rand.NextVector2Circular(30, 30), ModContent.DustType<GlowDust>(),
                Projectile.velocity.RotatedByRandom(.4f) * Main.rand.NextFloat(1, 3),
                newColor: Color.CornflowerBlue with { A = 0 }).noGravity = true;

            Projectile.localAI[0] = MathHelper.Lerp(Projectile.localAI[0], 1, 0.15f);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = Assets.Extras.wavyLaser.Value;
        Texture2D tex2 = Assets.Extras.Tentacle.Value;

        float len = MathHelper.Lerp(300, 1200, Projectile.ai[0]);

        if ((int)Projectile.ai[2] == 1)
        {
            tex = Assets.Extras.laser2.Value;
            len = 2700;
        }

        List<VertexPositionColorTexture> vertices = new();
        Vector2 start = Projectile.Center - Main.screenPosition;
        Vector2 end = Projectile.Center + Projectile.velocity
            * len * Projectile.scale - Main.screenPosition;
        for (float i = 0; i < 1f; i += 0.05f)
        {
            Color color = Color.Lerp(Color.CornflowerBlue, Color.Transparent, MathHelper.SmoothStep(0, 1, i)) *
                          MathHelper.Clamp(i * 4, 0, 1) * 2 * (1 - Projectile.localAI[0]);
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
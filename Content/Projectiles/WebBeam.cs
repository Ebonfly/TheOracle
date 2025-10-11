using System;
using System.Collections.Generic;
using TheOracle.Common.Utils;

namespace TheOracle.Content.Projectiles;

public class WebBeam : ModProjectile
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
        Projectile.timeLeft = 500;
        Projectile.aiStyle = -1;
        Projectile.ignoreWater = true;
        Projectile.hostile = true;
        Projectile.friendly = false;
    }

    public override bool ShouldUpdatePosition() => Projectile.ai[2] < 80;
    public override bool? CanDamage() => Projectile.ai[2] > 90 && Projectile.timeLeft > 30;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float a = 0;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
            OtherPosition, 25, ref a);
    }

    public Vector2 OtherPosition
    {
        get
        {
            Vector2 pos = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            return Vector2.Clamp(pos, Projectile.Center - Vector2.One * 2000, Projectile.Center + Vector2.One * 2000);
        }
    }

    public override void AI()
    {
        if (Projectile.ai[2] > 1 && Projectile.timeLeft >= 499)
        {
            Projectile.timeLeft = (int)Projectile.ai[2];
            Projectile.ai[2] = 2;
            return;
        }

        if (Projectile.ai[0] == 0)
        {
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.type == Type && proj.whoAmI != Projectile.whoAmI)
                {
                    Projectile.ai[0] = proj.Center.X;
                    Projectile.ai[1] = proj.Center.Y;
                    break;
                }
            }
        }

        Projectile.rotation = (OtherPosition - Projectile.Center).ToRotation();
        Projectile.ai[2]++;

        if (Projectile.ai[2] < 30)
            Projectile.localAI[0] = MathHelper.SmoothStep(0, 0.5f, Projectile.ai[2] / 30f);

        else if (Projectile.ai[2] < 50)
            Projectile.localAI[1] = MathHelper.SmoothStep(0, 0.2f, (Projectile.ai[2] - 30f) / 20f);

        if (Projectile.ai[2] > 60)
            Projectile.velocity *= 0.98f;

        if (Projectile.ai[2] is > 80 and < 120)
        {
            Projectile.localAI[1] = MathHelper.SmoothStep(0.2f, 1, (Projectile.ai[2] - 80f) / 40f);
            Projectile.localAI[2] = MathHelper.SmoothStep(0, 1, (Projectile.ai[2] - 80f) / 40f);
        }

        if (Projectile.timeLeft < 40)
            Projectile.localAI[0] = MathHelper.SmoothStep(0.5f, 1f, Utils.GetLerpValue(40, 0, Projectile.timeLeft));
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D flare = Images.Extras.Textures.Crosslight.Value;
        Texture2D lensflare = Images.Extras.Textures.Lensflare.Value;
        Texture2D beam = Images.Extras.Textures.WavyLaser.Value;


        for (int j = 0; j < 2; j++)
        {
            Color col = (j == 0 ? Color.White * 0.25f : Color.CornflowerBlue) with { A = 0 };

            for (int i = 0; i < 3; i++)
            {
                Main.spriteBatch.Draw(flare,
                    Projectile.Center + Main.rand.NextVector2Unit() * i * 3 * Projectile.localAI[0] -
                    Main.screenPosition, null, col * MathF.Sin(Projectile.localAI[0] * MathHelper.Pi) * 0.125f, 0,
                    flare.Size() / 2f,
                    Projectile.localAI[0] * 2, SpriteEffects.None, 0);

                Main.spriteBatch.Draw(flare,
                    Projectile.Center + Main.rand.NextVector2Unit() * i * 3 * Projectile.localAI[0] -
                    Main.screenPosition, null, col * MathF.Sin(Projectile.localAI[0] * MathHelper.Pi) * 0.25f,
                    MathHelper.PiOver4,
                    flare.Size() / 2f, Projectile.localAI[0], SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(lensflare, Projectile.Center - Main.screenPosition, null,
                col * MathF.Sin(Projectile.localAI[0] * MathHelper.Pi) *
                (1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4 + Projectile.whoAmI * 0.7f) * 0.4f),
                0, lensflare.Size() / 2f, Projectile.localAI[0] * new Vector2(1.5f, 0.5f), SpriteEffects.None, 0);

            if (OtherPosition.Distance(Projectile.Center) > 20)
            {
                List<VertexPositionColorTexture> vertices = new();
                for (float i = 0; i <= MathHelper.Clamp(Projectile.localAI[1], 0.3f, 1); i += 0.05f)
                {
                    Vector2 pos =
                        Vector2.Lerp(Projectile.Center,
                            OtherPosition + Main.rand.NextVector2Unit() * (1.05f - Projectile.localAI[2]) * 100 *
                            Projectile.localAI[1], i) -
                        Main.screenPosition;
                    Color col2 = col * (1 - i / MathHelper.Clamp(Projectile.localAI[1], 0.3f, 1)) * i *
                                 (4 / MathHelper.Clamp(Projectile.localAI[1], 0.3f, 1)) * (1 - i) *
                                 MathF.Sin(Projectile.localAI[0] * MathHelper.Pi) * 2;

                    for (int k = -1; k < 2; k += 2)
                        vertices.Add(PrimitiveUtils.AsVertex(pos + new Vector2(30, 0).RotatedBy(Projectile.rotation +
                                MathHelper.PiOver2 * k), col2 * Projectile.localAI[k < 0 ? 1 : 2],
                            new Vector2(i + Main.GlobalTimeWrappedHourly * 0.1f, k < 0 ? 0 : 1)));
                }

                if (vertices.Count > 2)
                {
                    Main.spriteBatch.End(out var ss);
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap,
                        ss.depthStencilState, RasterizerState.CullNone, null, ss.matrix);

                    PrimitiveUtils.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, beam, false);

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(ss);
                }
            }
        }

        return base.PreDraw(ref lightColor);
    }
}
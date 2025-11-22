using System;
using System.Collections.Generic;
using TheOracle.Common.Utils;
using TheOracle.Content.Dusts;

namespace TheOracle.Content.Projectiles;

public class CurveClock : ModProjectile
{
    public override string Texture => "TheOracle/Assets/Images/Extras/clock";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 4000;
    }

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.scale = 0f;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 930;
        Projectile.aiStyle = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override bool? CanDamage()
    {
        return base.CanDamage();
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI()
    {
        Projectile.ai[0]++;

        if (Projectile.ai[2] == 0 && Projectile.ai[1] == 0 && Main.netMode != 1)
        {
            Projectile.ai[2] = 1;
            Projectile.ai[1] = Projectile.NewProjectile(Projectile.InheritSource(Projectile),
                Projectile.Center + new Vector2(0, -1500), Vector2.Zero, Type,
                Projectile.damage, Projectile.knockBack, Projectile.owner, 0, Projectile.whoAmI, ai2: -2);
        }

        if (Projectile.ai[0] < 10)
        {
            Projectile.scale = MathHelper.Lerp(Projectile.scale, 0.3f, 0.2f);
            Projectile.rotation = -MathHelper.PiOver2;
            if (Projectile.ai[2] < 0)
                Projectile.rotation = MathHelper.Pi - MathHelper.PiOver2;
        }

        if (Projectile.ai[0] < 60)
        {
            Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(50, 50) +
                          Projectile.rotation.ToRotationVector2() * 600;
            Dust.NewDustPerfect(pos,
                ModContent.DustType<GlowDustSine>(), Vector2.UnitY * 20 * Projectile.ai[2],
                newColor: Color.CornflowerBlue with { A = 0 }).customData = Main.rand.NextFloat(-1, 1);
        }

        if (Projectile.ai[0] > 100f)
        {
            Projectile.localAI[0] = MathHelper.Lerp(Projectile.localAI[0], 1, 0.05f);
            if (Projectile.ai[0] > 120)
                Projectile.rotation += MathHelper.ToRadians(Projectile.ai[2]);
        }

        if (Projectile.ai[0] > 910)
        {
            _alpha = MathHelper.Lerp(_alpha, 0, 0.1f);
            Projectile.scale = MathHelper.Lerp(Projectile.scale, 0f, 0.1f);
        }
    }

    private float _alpha, _clockHandRotation, _additionalFlare;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D glow = Images.Extras.Textures.Glow;
        Texture2D lensflare = Images.Extras.Textures.Lensflare;
        Texture2D clockHand1 = Images.Extras.Textures.ClockHandShort;
        Texture2D clockHand2 = Images.Extras.Textures.ClockHandLong;
        Texture2D twirl = Images.Extras.Textures.Twirl[0];
        Texture2D clock = Images.Extras.Textures.Clock;
        Texture2D flare = Images.Extras.Textures.Crosslight;

        for (int i = 0; i < 2; i++)
        {
            Color col = (i == 0 ? Color.CornflowerBlue : Color.White * 0.5f) * (Projectile.scale / 0.3f);
            col.A = 0;

            Main.spriteBatch.Draw(clock, Projectile.Center - Main.screenPosition, null, col,
                Main.GameUpdateCount * -0.01f, clock.Size() / 2, Projectile.scale * 1.1f + i * 0.005f,
                SpriteEffects.None, 0);

            Main.spriteBatch.Draw(clock, Projectile.Center - Main.screenPosition, null, col,
                MathHelper.Pi / 6f + Main.GameUpdateCount * 0.02f, clock.Size() / 2,
                Projectile.scale * .75f + i * 0.005f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(clock, Projectile.Center - Main.screenPosition, null, col,
                MathHelper.Pi / 3f + Main.GameUpdateCount * 0.04f, clock.Size() / 2,
                Projectile.scale * .5f + i * 0.0025f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(flare, Projectile.Center - Main.screenPosition, null, col,
                0, flare.Size() / 2, Projectile.scale * 4.5f + i * 0.0025f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(flare, Projectile.Center - Main.screenPosition, null, col,
                MathHelper.Pi / 3f, flare.Size() / 2, Projectile.scale * 2.5f + i * 0.0025f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(flare, Projectile.Center - Main.screenPosition, null, col,
                MathHelper.Pi / 6f, flare.Size() / 2, Projectile.scale * 3.5f + i * 0.0025f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(clockHand2, Projectile.Center - Main.screenPosition, null,
                col with { A = 0 }, Projectile.rotation, new Vector2(84, clockHand2.Height / 2f),
                Projectile.scale * 1.2f, SpriteEffects.None, 0);
        }

        if (Projectile.ai[0] <= 2 || Projectile.ai[2] < 0)
            return false;

        Projectile proj = Main.projectile[(int)Projectile.ai[1]];

        List<VertexPositionColorTexture> vertices = new();

        for (int i = 0; i < 100; i++)
        {
            Vector2 pos1 = Vector2.Lerp(Projectile.Center + Projectile.rotation.ToRotationVector2() * 300,
                Projectile.Center + Projectile.rotation.ToRotationVector2() * 1400, i / 100f);
            Vector2 pos2 = Vector2.Lerp(
                Projectile.Center + (proj.Center - Projectile.Center).SafeNormalize(Vector2.One) * 1400,
                proj.Center + proj.rotation.ToRotationVector2() * 300, i / 100f);
            Vector2 sPos = Vector2.Lerp(pos1, pos2, i / 100f) - Main.screenPosition;

            Color col = Color.CornflowerBlue with { A = 0 } *
                        MathF.Pow(MathF.Sin(i / 100f * MathHelper.Pi), 3) *
                        (Projectile.scale / 0.3f) * Projectile.localAI[0];

            for (int j = -1; j < 2; j += 2)
            {
                Vector2 off = new Vector2(40, 0).RotatedBy((pos2 - pos1).ToRotation() - MathHelper.PiOver2 * j);
                vertices.Add(PrimitiveUtils.AsVertex(sPos + off, col,
                    new Vector2(i / 100f - Main.GameUpdateCount * 0.04f, MathHelper.Clamp(-j, 0, 1))));
            }
        }

        if (vertices.Count > 2)
        {
            Main.spriteBatch.End(out var ss);
            Main.spriteBatch.Begin(ss with { samplerState = SamplerState.PointWrap });

            PrimitiveUtils.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip,
                Images.Extras.Textures.Tentacle);
            PrimitiveUtils.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip,
                Images.Extras.Textures.WavyLaser);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(ss);
        }

        return false;
    }
}
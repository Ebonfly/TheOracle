using System;
using System.Collections.Generic;
using TheOracle.Common.Utils;

namespace TheOracle.Content.Projectiles;

public class OracleBlastReversal : ModProjectile
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
        Projectile.timeLeft = 340;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
    }

    public override bool? CanDamage() => Projectile.ai[1] < 0.1f;
    public override bool ShouldUpdatePosition() => Projectile.ai[1] < 0.1f;

    public override bool PreDraw(ref Color lightColor)
    {
        float alpha = MathHelper.Clamp(
            MathF.Sin(Utils.GetLerpValue(340, 0, Projectile.timeLeft) * MathHelper.Pi) * 5 * (1 - Projectile.ai[1]),
            0, 1);
        float length = MathHelper.Clamp(MathF.Sin(Utils.GetLerpValue(340, 0, Projectile.timeLeft) * MathHelper.Pi) * 2,
            0, 1);
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(1, 4, 0, Projectile.frame),
            Color.White with { A = 0 } * alpha, Projectile.rotation, new Vector2(65, 35) / 2, Projectile.scale,
            SpriteEffects.None, 0);

        Vector2 start = Projectile.Center + Projectile.rotation.ToRotationVector2() * 20 - Main.screenPosition;
        Vector2 end = start - Projectile.rotation.ToRotationVector2() * 200 * length;
        List<VertexPositionColorTexture> vertices = new();
        for (float i = 0; i < 1f; i += 0.1f)
        {
            Vector2 pos = Vector2.Lerp(start, end, i);
            for (int j = -1; j < 2; j += 2)
                vertices.Add(PrimitiveUtils.AsVertex(
                    pos + new Vector2(40, 0).RotatedBy(Projectile.rotation + MathHelper.PiOver2 * j),
                    Color.CornflowerBlue with { A = 0 } * MathF.Pow(1f - i, 2) * alpha * 2,
                    new Vector2(i, (j < 0 ? 0 : 1))));
        }

        if (vertices.Count > 2)
        {
            Main.spriteBatch.End(out var ss);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap,
                ss.depthStencilState, RasterizerState.CullNone, null, ss.matrix);

            PrimitiveUtils.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip,
                Images.Extras.Textures.Ex.Value,
                false);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(ss);
        }


        return false;
    }

    private Vector2 _startVelocity;

    public override void AI()
    {
        if (_startVelocity == Vector2.Zero) _startVelocity = Projectile.velocity;

        Lighting.AddLight(Projectile.Center, TorchID.White);
        if (++Projectile.frameCounter % 5 == 0)
            if (++Projectile.frame > 3)
                Projectile.frame = 0;

        Projectile.rotation = _startVelocity.ToRotation();

        Projectile.ai[0]++;
        if (Projectile.ai[0] < 40 && Projectile.velocity.Length() < 23f)
            Projectile.velocity *= 1.1f;

        if (Projectile.ai[0] is > 100 and < 140)
        {
            Projectile.ai[1] = MathHelper.Lerp(0, 1, (Projectile.ai[0] - 100f) / 38f);
            Projectile.velocity *= 0.9f;
        }

        if (Projectile.ai[0] is > 190 and < 230)
            Projectile.ai[1] = MathHelper.Lerp(1, 0, (Projectile.ai[0] - 190f) / 38f);

        if ((int)Projectile.ai[0] == 240)
            Projectile.velocity = _startVelocity * -21;

        if (Projectile.ai[0] > 320 && Projectile.velocity.Length() > 1f)
            Projectile.velocity /= 1.1f;
    }
}
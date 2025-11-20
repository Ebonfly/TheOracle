using System;
using TheOracle.Content.Buffs;

namespace TheOracle.Content.Projectiles;

// TODO:
// - clamp eye position
// - kill ebon and take his place
// - item sprite
public class OracleP2Pet : ModProjectile
{
    private Player Owner => Main.player[Projectile.owner];

    public override string Texture => "TheOracle/Assets/Images/Projectiles/OracleP2Pet";

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 4;
        Main.projPet[Projectile.type] = true;

        ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets
            .SimpleLoop(0, Main.projFrames[Projectile.type], 5)
            .WithOffset(-4, -11.5f)
            .WithCode(CharacterPreviewCustomization);
    }

    public static void CharacterPreviewCustomization(Projectile proj, bool walking)
    {
        proj.position.Y += MathF.Cos(((float)Main.timeForVisualEffects % 60f) / 60f * MathHelper.TwoPi);
    }

    public override void SetDefaults()
    {
        Projectile.Size = new(38);
        Projectile.penetrate = -1;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.netImportant = true;
        Projectile.aiStyle = -1;
        AIType = -1;
    }

    public override void AI()
    {
        if (!Owner.dead && Owner.HasBuff(ModContent.BuffType<OracleP2PetBuff>()))
            Projectile.timeLeft = 2;

        Vector2 targetPosition = Owner.MountedCenter + new Vector2(-45 * Owner.direction, -45);
        targetPosition.Y += (float)Math.Sin(++Projectile.ai[0] * 0.05f) * 6;

        if (Projectile.Distance(Owner.Center) > 1200f)
            Projectile.Center = targetPosition;

        Projectile.velocity = Vector2.Lerp(Projectile.velocity, (targetPosition - Projectile.Center) * 0.12f, 0.15f);
        Projectile.rotation = Math.Clamp(Projectile.velocity.X * 0.04f, -0.9f, 0.9f);

        if (++Projectile.frameCounter > 5)
        {
            Projectile.frameCounter = 0;

            if (++Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.frame = 0;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Texture2D glowTexture = Images.Projectiles.Textures.OraclePPet_Glow;
        Texture2D eyeTexture = Images.Projectiles.Textures.OraclePPet_Eye;
        Texture2D orbTexture = Images.Projectiles.Textures.OraclePPet_Orb;

        int startY = Projectile.frame * texture.Height / Main.projFrames[Projectile.type];
        Rectangle sourceRectangle = new(0, startY, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRectangle, lightColor,
            Projectile.rotation, sourceRectangle.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(glowTexture, Projectile.Center - Main.screenPosition, sourceRectangle, Color.White,
            Projectile.rotation, sourceRectangle.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

        Vector2 eyeOffset = Projectile.velocity * new Vector2(0.4f, 0.15f);

        if (eyeOffset.Length() > 4)
            eyeOffset = Vector2.Normalize(eyeOffset) * 4;

        Main.EntitySpriteDraw(eyeTexture, Projectile.Center + eyeOffset - Main.screenPosition, eyeTexture.Bounds,
            Color.White, Projectile.rotation, eyeTexture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

        startY = Projectile.frame * orbTexture.Height / Main.projFrames[Projectile.type];
        sourceRectangle = new(0, startY, orbTexture.Width, orbTexture.Height / Main.projFrames[Projectile.type]);

        Main.EntitySpriteDraw(orbTexture,
            Projectile.Center - new Vector2(22, -20).RotatedBy(Projectile.rotation) +
            Vector2.One.RotatedBy(Main.GlobalTimeWrappedHourly * 5) * 2 - Main.screenPosition, sourceRectangle,
            Color.White, Projectile.rotation, sourceRectangle.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

        Main.EntitySpriteDraw(orbTexture,
            Projectile.Center + new Vector2(22, 20).RotatedBy(Projectile.rotation) +
            -Vector2.One.RotatedBy(Main.GlobalTimeWrappedHourly * 5) * 2 - Main.screenPosition, sourceRectangle,
            Color.White, Projectile.rotation, sourceRectangle.Size() * 0.5f, Projectile.scale,
            SpriteEffects.FlipHorizontally, 0);

        return false;
    }
}
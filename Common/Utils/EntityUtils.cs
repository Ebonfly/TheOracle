using System;

namespace TheOracle.Common.Utils;

public static class EntityUtils
{
    public static void SineMovement(this Projectile projectile, Vector2 initialCenter, Vector2 initialVel,
        float frequencyMultiplier, float amplitude)
    {
        projectile.ai[1]++;
        float wave = MathF.Sin(projectile.ai[1] * frequencyMultiplier);
        Vector2 vector = new Vector2(initialVel.X, initialVel.Y).RotatedBy(MathHelper.ToRadians(90));
        vector.Normalize();
        wave *= projectile.ai[0];
        wave *= amplitude;
        Vector2 offset = vector * wave;
        projectile.Center = initialCenter + (projectile.velocity * projectile.ai[1]);
        projectile.Center = projectile.Center + offset;
    }
}
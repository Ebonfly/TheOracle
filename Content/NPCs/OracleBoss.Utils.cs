using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TheOracle.Content.NPCs;

public partial class OracleBoss : ModNPC
{
    public Player Player => Main.player[NPC.target];

    public void TargetingLogic(int take = 0)
    {
        if (AIState <= Intro)
            NPC.TargetClosest(false);
        if (!NPC.HasPlayerTarget && take < 3)
            TargetingLogic(take + 1);
        if (take >= 3)
            AIState = Despawn;
    }

    public void IdleAnimations()
    {
        float factor = MathF.Sin(ConstantTimer * 0.0125f);

        EyeTarget = Player.Center;
        CrystalPosition =
            NPC.Center -
            new Vector2(factor * 6, NPC.height * 0.28f - MathF.Sin(MathF.Abs(factor) * MathHelper.Pi) * 6).RotatedBy(
                NPC.rotation);
        CrystalRotation = NPC.rotation + factor * 0.05f;

        for (int i = 0; i < OrbPosition.Length; i++)
            OrbPosition[i] = NPC.Center +
                             new Vector2(150 + MathF.Sin(ConstantTimer * 0.025f + i * 10) * 20).RotatedBy(
                                 MathHelper.TwoPi * i / (float)OrbPosition.Length + factor * 0.2f);
    }

    public void CacheEyePosition(Vector2 target)
    {
        for (int i = _cachedEyeOffsets.Length - 1; i > 0; i--)
        {
            _cachedEyeOffsets[i] = Vector2.Lerp(_cachedEyeOffsets[i], _cachedEyeOffsets[i - 1], 0.1f);
        }

        _cachedEyeOffsets[0] = target;
    }
}
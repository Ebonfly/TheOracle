using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TheOracle.Content.NPCs;

public partial class OracleBoss : ModNPC
{
    public Player Player => Main.player[NPC.target];
    private float IdleSwayFactor => MathF.Sin(ConstantTimer * 0.0125f);

    int ResetTo(int move, int idleTime = 200)
    {
        AITimer = -idleTime;
        AITimer2 = 0;
        AITimer3 = 0;
        return move;
    }

    public void TargetingLogic(int take = 0)
    {
        if (AIState <= Intro)
            NPC.TargetClosest(false);
        if (!NPC.HasPlayerTarget && take < 3)
            TargetingLogic(take + 1);
        if (take >= 3)
            AIState = Despawn;
    }

    public void IdleCrystal(float t = 1f)
    {
        CrystalPosition = Vector2.Lerp(CrystalPosition, NPC.Center - new Vector2(IdleSwayFactor * 6,
            NPC.height * 0.28f - MathF.Sin(MathF.Abs(IdleSwayFactor) * MathHelper.Pi) * 6).RotatedBy(NPC.rotation), t);
        CrystalRotation = NPC.rotation + IdleSwayFactor * 0.05f;
    }

    public void IdleOrbs(float t = 1f)
    {
        for (int i = 0; i < OrbPosition.Length; i++)
            OrbPosition[i] = Vector2.Lerp(OrbPosition[i], NPC.Center + new Vector2(150 +
                MathF.Sin(ConstantTimer * 0.025f + i * 10) * 20).RotatedBy(
                MathHelper.TwoPi * i / (float)OrbPosition.Length +
                IdleSwayFactor * 0.2f), t);
    }

    public void OminousYVelHover(float ampFactor = 0.05f, float freqFactor = 0.5f, float t = 1f)
    {
        NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y,
            MathHelper.Clamp(MathF.Sin(ConstantTimer * ampFactor) * freqFactor, -0.5f, 0.2f), t);
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
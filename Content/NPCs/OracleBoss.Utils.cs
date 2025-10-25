using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TheOracle.Content.NPCs;

public partial class OracleBoss : ModNPC
{
    public Player Player => Main.player[NPC.target];
    private float IdleSwayFactor => MathF.Sin(ConstantTimer * 0.0125f);
    public Vector2 CrystalTipDirection => CrystalRotation.ToRotationVector2().RotatedBy(-MathHelper.PiOver2);

    int ResetTo(int move, int idleTime = 200)
    {
        AttackPart = 0;
        AITimer = -idleTime;
        AITimer2 = 0;
        AITimer3 = 0;
        for (int i = 0; i < NPC.localAI.Length; i++)
            NPC.localAI[0] = 0;
        DisposablePosition = Vector2.Zero;
        NPC.netUpdate = true;
        return move;
    }

    void IncrementAttackPart(bool leaveTimer2 = false, bool leaveTimer3 = false, bool leaveLocals = false,
        bool leaveDisposable = false)
    {
        AttackPart++;
        AITimer = 0;
        if (!leaveTimer2)
            AITimer2 = 0;
        if (!leaveTimer3)
            AITimer3 = 0;
        if (!leaveLocals)
            for (int i = 0; i < NPC.localAI.Length; i++)
                NPC.localAI[0] = 0;
        if (!leaveDisposable)
            DisposablePosition = Vector2.Zero;
        NPC.netUpdate = true;
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
        CrystalRotation = Utils.AngleLerp(CrystalRotation, NPC.rotation + IdleSwayFactor * 0.05f, t);
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

    public void SaveOldOrbPosition(int orbIndex)
    {
        for (int i = OldOrbPosition.GetLength(1) - 1; i > 0; i--)
        {
            OldOrbPosition[orbIndex, i] =
                Vector2.Lerp(OldOrbPosition[orbIndex, i], OldOrbPosition[orbIndex, i - 1], 0.1f);
        }

        OldOrbPosition[orbIndex, 0] = OrbPosition[orbIndex];
    }
}
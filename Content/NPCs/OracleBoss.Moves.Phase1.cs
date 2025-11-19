using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using TheOracle.Common.Utils;
using TheOracle.Content.Dusts;
using TheOracle.Content.Projectiles;
using TheOracle.Content.Projectiles.VFX;

namespace TheOracle.Content.NPCs;

public partial class OracleBoss : ModNPC
{
    int DoOrbConjure()
    {
        IdleCrystal(1);
        float factor = MathF.Pow(MathHelper.Clamp((AITimer - 70) / 120f, 0, 1f), 2);
        float factor2 = MathF.Pow(MathHelper.Clamp(AITimer / 120f, 0, 1f), 2);
        if (AITimer > 550)
        {
            factor = MathHelper.Lerp(factor, 0, MathHelper.Clamp(AITimer / 60f, 0, 1f));
            factor2 = MathHelper.Lerp(factor2, 0, MathHelper.Clamp(AITimer / 60f, 0, 1f));
        }

        AITimer2 += factor;
        if (AITimer < 50 || AITimer > 560)
        {
            AITimer3 = MathHelper.Lerp(AITimer3, 1, 0.05f);
            OminousYVelHover(AITimer3);
            NPC.velocity.X *= 0.98f;
            EyeTarget = Player.Center;
        }
        else
        {
            AITimer3 = 0;
            NPC.velocity = Vector2.Lerp(NPC.velocity,
                ((EyeTarget + new Vector2(600).RotatedBy(MathHelper.ToRadians(ConstantTimer))) - NPC.Center) * 0.01f,
                0.05f);
        }

        for (int i = 0; i < OrbPosition.Length; i++)
        {
            float angle = MathHelper.TwoPi * i / (float)OrbPosition.Length;
            angle += MathHelper.ToRadians(AITimer2 * 7);
            Vector2 offset = new Vector2(50 + 100 * factor).RotatedBy(angle);
            OrbPosition[i] = Vector2.Lerp(OrbPosition[i], EyeTarget + offset, factor2 * 0.12f);
        }

        if ((int)AITimer == 110 && Main.netMode != NetmodeID.MultiplayerClient)
            Projectile.NewProjectile(null, EyeTarget, Vector2.Zero, ModContent.ProjectileType<OracleMiniClock>(), 0, 0);

        if (AITimer > 460)
            return ResetTo((int)AIState + 1, 80);
        return OrbConjure;
    }

    int DoCrystalSliceDash()
    {
        IdleOrbs();

        if (Substate > 0)
        {
            EyeTarget = CrystalPosition;
            NPC.velocity *= 0.95f;
        }

        switch (Substate)
        {
            // Startup
            case 0:
                IdleCrystal();
                if (AITimer < 2)
                    NPC.velocity = Vector2.Zero;
                if (AITimer < 30)
                {
                    EyeTarget = Vector2.Lerp(EyeTarget, CrystalPosition, AITimer / 30f);
                    NPC.velocity.Y -= 0.05f;
                }

                if (AITimer >= 40)
                    IncrementSubstate();
                break;

            // Chargeup
            case 1:
                if (AITimer < 40)
                {
                    CrystalRotation = Utils.AngleLerp(CrystalRotation, 0, 0.1f);
                    CrystalPosition.Y -= MathHelper.SmoothStep(1, 2, MathF.Sin(AITimer / 40f * MathHelper.Pi));
                    CrystalPosition.X = MathHelper.Lerp(CrystalPosition.X, NPC.Center.X, 0.1f);
                    Vector2 pos = CrystalPosition +
                                  Main.rand.NextVector2Unit() * 700;
                    for (int i = 0; i < 3; i++)
                        Dust.NewDustPerfect(pos, ModContent.DustType<GlowDust>(),
                            (CrystalPosition - new Vector2(0, 40) - pos).SafeNormalize(Vector2.Zero) * 16,
                            newColor: Color.CornflowerBlue with { A = 0 }, Scale: 0.9f).noGravity = true;
                }

                if (AITimer >= 100)
                {
                    CrystalFlash = 1;
                    SoundEngine.PlaySound(SoundID.Item4.WithPitchOffset(-0.5f), CrystalPosition);
                    IncrementSubstate();
                }

                break;

            // 1st slice
            case 2:
                if (AITimer < 10)
                    CrystalRotation =
                        CrystalRotation.AngleLerp((Player.Center - CrystalPosition).ToRotation() + MathHelper.PiOver2,
                            MathHelper.Lerp(0.2f, 0.5f, AITimer / 10f));

                if ((int)AITimer == 10)
                    Projectile.NewProjectile(null, CrystalPosition, CrystalTipDirection,
                        ModContent.ProjectileType<CrystalSlice>(), 25, 0);

                if (AITimer < 50)
                    CrystalPosition -= CrystalTipDirection *
                                       MathF.Sin(MathF.Pow(AITimer / 50f, 2) * MathHelper.Pi);
                if (AITimer >= 60)
                    IncrementSubstate();
                break;

            // Rest of the slices
            case 3:
                NPC.velocity = Vector2.Lerp(NPC.velocity, (Player.Center - new Vector2(0, 240) - NPC.Center) * 0.05f,
                    0.04f * (Substate == 1 ? MathHelper.Clamp(AITimer / 20f, 0, 1) : 1));
                if (AITimer < 10)
                {
                    CrystalPosition += CrystalTipDirection *
                                       MathHelper.SmoothStep(100, 20, MathHelper.Clamp(AITimer / 10f, 0, 1));
                    CrystalOpacity = MathHelper.SmoothStep(1, 0, AITimer / 10f);
                }
                else if (AITimer < 50)
                {
                    CrystalOpacity = 0;
                    if (AITimer % 4 == 0)
                    {
                        Vector2 position = Player.Center + Main.rand.NextVector2CircularEdge(800, 800);
                        Projectile.NewProjectile(null, position,
                            (Player.Center - position).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.6f),
                            ModContent.ProjectileType<CrystalSlice>(), 25, 0);
                    }
                }
                else if ((int)AITimer == 70)
                {
                    Vector2 usualPos = NPC.Center - new Vector2(IdleSwayFactor * 6,
                            NPC.height * 0.28f - MathF.Sin(MathF.Abs(IdleSwayFactor) * MathHelper.Pi) * 6)
                        .RotatedBy(NPC.rotation);
                    Projectile.NewProjectile(null, CrystalPosition + CrystalTipDirection * 750,
                        (usualPos - (CrystalPosition + CrystalTipDirection * 750)).SafeNormalize(Vector2.UnitX),
                        ModContent.ProjectileType<CrystalSlice>(), 0, 0);

                    float rot = CrystalRotation;
                    IdleCrystal();
                    CrystalRotation = rot;
                }

                if (AITimer > 130)
                    CrystalOpacity = MathHelper.Lerp(CrystalOpacity, 1f, 0.15f);

                if (AITimer > 145)
                    return ResetTo((int)AIState + 1, 80);
                break;
        }

        return CrystalSliceDash;
    }

    int DoOrbClockHandSwordForm()
    {
        IdleCrystal();
        for (int i = 1; i < 4; i++)
        {
            if (Substate == 0 ? AITimer > 10 : Substate < 7 && (int)AITimer % 3 == i - 1)
            {
                bool inner = Main.rand.NextBool(5);
                Dust d = Dust.NewDustPerfect(OrbPosition[0], ModContent.DustType<GlowDust>(),
                    (OrbPosition[i] - new Vector2(0, 40) - OrbPosition[0]).SafeNormalize(Vector2.Zero)
                    .RotatedByRandom(0.1f) * Main.rand.NextFloat(4, 7) * (inner ? 0.2f : 1),
                    newColor: Color.CornflowerBlue with { A = 0 }, Scale: 0.9f);
                d.noGravity = true;
                if (inner)
                    d.customData = true;
            }

            OrbPosition[i] = Vector2.Lerp(OrbPosition[i],
                OrbPosition[0] + new Vector2(120).RotatedBy(MathHelper.TwoPi * i / 3f + ConstantTimer * 0.1f),
                Substate == 0 ? MathHelper.Clamp(AITimer / 60f, 0, 1) : 1);
        }

        if (Substate > 0)
        {
            NPC.velocity = Vector2.Lerp(NPC.velocity, (Player.Center - new Vector2(0, 240) - NPC.Center) * 0.05f,
                0.04f * (Substate == 1 ? MathHelper.Clamp(AITimer / 20f, 0, 1) : 1));
        }

        void Slice()
        {
            NPC.localAI[2] = 3f;
            Projectile.NewProjectile(null, OrbPosition[0],
                (EyeTarget - OrbPosition[0]).SafeNormalize(Vector2.UnitX),
                ModContent.ProjectileType<CrystalSlice>(), 25, 0, ai2: 1);

            Main.instance.CameraModifiers.Add(new PunchCameraModifier(OrbPosition[0],
                (EyeTarget - OrbPosition[0]).SafeNormalize(Vector2.UnitX),
                30, 20, 20, 3000));

            SoundEngine.PlaySound(
                new SoundStyle("TheOracle/Assets/Sounds/sliceMagic") with { Volume = 2, PitchVariance = 0.1f });
        }

        switch (Substate)
        {
            // Charge up the spear
            case 0:
                NPC.localAI[2] = 0.2f;
                OrbPosition[0] = Vector2.Lerp(OrbPosition[0], NPC.Center + new Vector2(0, 300), 0.2f);
                NPC.velocity = Vector2.Lerp(NPC.velocity, -Vector2.UnitY * 3, 0.1f);

                if ((int)AITimer % 5 == 0 && !Main.dedServ)
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(OrbPosition[0],
                        Main.rand.NextVector2Unit(), AITimer / 7f + 2, 20, 5, 1000));

                if (AITimer > 60)
                    IncrementSubstate(leaveLocals: true);
                break;

            // Ominous hover
            case 1:
                EyeTarget = Vector2.Lerp(EyeTarget, Player.Center +
                                                    (Player.Center - OrbPosition[0]).SafeNormalize(Vector2.UnitX) *
                                                    2000, AITimer / 120f);
                if (AITimer < 30)
                    AITimer3 = MathHelper.Lerp(AITimer3, 1.8f, 0.05f);
                else if (AITimer < 34)
                    AITimer3 = MathHelper.Lerp(AITimer3, 2.3f, 0.3f);

                if ((int)AITimer == 34 && !Main.dedServ)
                {
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(OrbPosition[0],
                        (EyeTarget - OrbPosition[0]).SafeNormalize(Vector2.UnitX),
                        30, 20, 20, 3000));

                    SoundEngine.PlaySound(
                        new SoundStyle("TheOracle/Assets/Sounds/clockMagicBurst") with
                        {
                            Volume = 2, PitchVariance = 0.1f
                        });
                }

                if (AITimer < 52)
                    NPC.localAI[1] = MathHelper.Clamp(AITimer / 50f, 0, 1);

                if (AITimer is > 30 and < 60)
                    OrbPosition[0] += Main.rand.NextVector2Circular(15, 15);
                if (AITimer is > 60 and < 120)
                {
                    NPC.localAI[1] = 0;

                    OrbPosition[0] = Vector2.Lerp(OrbPosition[0],
                        Player.Center + (OrbPosition[0] - Player.Center).SafeNormalize(Vector2.UnitX)
                        .RotatedBy(MathF.Sin(ConstantTimer * 0.06f) * 0.05f) *
                        (700 + MathF.Sin(ConstantTimer * 0.06f) * 100), 0.1f);
                }

                if (AITimer >= 120)
                    IncrementSubstate(leaveTimer3: true, leaveLocals: true);
                break;

            // Impale move #1
            case 2:
                if ((int)AITimer == 30)
                    Slice();

                NPC.localAI[2] = MathHelper.Lerp(NPC.localAI[2], 0.2f, 0.05f);

                if (AITimer < 30)
                {
                    if (AITimer < 15)
                        AITimer2 = .5f;
                    EyeTarget = Player.Center +
                                (Player.Center - OrbPosition[0]).SafeNormalize(Vector2.UnitX) * 2000;
                    OrbPosition[0] += (OrbPosition[0] - Player.Center).SafeNormalize(Vector2.UnitX) * 15 *
                                      MathF.Sin(AITimer / 30f * MathF.PI);
                }
                else if (AITimer < 50)
                {
                    NPC.localAI[1] = MathHelper.Lerp(NPC.localAI[1], 1, 0.1f);
                    OrbPosition[0] += (EyeTarget - OrbPosition[0]).SafeNormalize(Vector2.UnitX) * 120;
                }
                else if (AITimer < 60)
                    EyeTarget = Vector2.Lerp(EyeTarget, Player.Center, 0.05f);
                else if (AITimer < 100)
                {
                    NPC.localAI[1] = 0;

                    OrbPosition[0] = Vector2.Lerp(OrbPosition[0],
                        Player.Center + (OrbPosition[0] - Player.Center).SafeNormalize(Vector2.UnitX)
                        .RotatedBy(MathF.Sin(ConstantTimer * 0.06f) * 0.05f) *
                        (700 + MathF.Sin(ConstantTimer * 0.06f) * 100), 0.1f);

                    EyeTarget = Vector2.Lerp(EyeTarget, Player.Center, 0.1f);

                    NPC.localAI[0] = (OrbPosition[0] - Player.Center).ToRotation();
                }
                else
                    IncrementSubstate(leaveTimer2: true, leaveTimer3: true, leaveLocals: true);

                break;

            // Small jab #1
            case 3:
                if (AITimer < 20)
                {
                    AITimer2 = 0.5f;
                    NPC.localAI[1] = AITimer / 20f;
                    OrbPosition[0] += (OrbPosition[0] - Player.Center).SafeNormalize(Vector2.UnitX) * 12.5f *
                                      MathF.Sin(AITimer / 20f * MathF.PI);

                    if ((int)AITimer == 19)
                        SoundEngine.PlaySound(
                            new SoundStyle("TheOracle/Assets/Sounds/quickConjure") with
                            {
                                Volume = 2, PitchVariance = 0.1f
                            });
                }
                else if (AITimer < 30)
                {
                    NPC.localAI[1] = 0;
                    if (AITimer < 26)
                        OrbPosition[0] += (EyeTarget - OrbPosition[0]).SafeNormalize(Vector2.UnitX)
                            .RotatedBy(MathF.Sin((AITimer - 20) / 10f) * 0.03f) * 80;
                }
                else
                    IncrementSubstate(leaveTimer2: true, leaveTimer3: true, leaveLocals: true);

                break;

            // Small jab #2
            case 4:
                if (AITimer < 50)
                {
                    if (AITimer > 10)
                        AITimer2 = 0.5f;
                    OrbPosition[0] = Vector2.Lerp(OrbPosition[0],
                        Player.Center +
                        new Vector2(700 + (AITimer / 50f) * 100 + MathF.Sin(ConstantTimer * 0.06f) * 100, 0)
                            .RotatedBy(NPC.localAI[0] + MathF.Sin(ConstantTimer * 0.06f)), 0.1f);

                    EyeTarget = Vector2.Lerp(EyeTarget, Player.Center, 0.1f);

                    if ((int)AITimer == 49)
                        SoundEngine.PlaySound(
                            new SoundStyle("TheOracle/Assets/Sounds/quickConjure") with
                            {
                                Volume = 2, PitchVariance = 0.1f
                            });
                }
                else if (AITimer < 60)
                {
                    NPC.localAI[1] = (AITimer - 50) / 10f;
                    if (AITimer < 426)
                        OrbPosition[0] += (EyeTarget - OrbPosition[0]).SafeNormalize(Vector2.UnitX)
                            .RotatedBy(MathF.Sin((AITimer - 10) / 10f) * 0.03f) * 70;
                }
                else if (AITimer < 110)
                {
                    NPC.localAI[1] = 0;

                    OrbPosition[0] = Vector2.Lerp(OrbPosition[0],
                        Player.Center + (OrbPosition[0] - Player.Center).SafeNormalize(Vector2.UnitX)
                        .RotatedBy(MathF.Sin(ConstantTimer * 0.06f) * 0.05f) *
                        (700 + MathF.Sin(ConstantTimer * 0.06f) * 100), 0.1f);
                    EyeTarget = Vector2.Lerp(EyeTarget, Player.Center, 0.1f);
                    DisposablePosition = OrbPosition[0];
                }
                else
                    IncrementSubstate(leaveTimer2: true, leaveTimer3: true, leaveLocals: true, leaveDisposable: true);

                break;

            // Swing
            case 5:
                if (AITimer < 50)
                {
                    float x = AITimer / 50f;
                    float progress = MathF.Pow(x, 2);

                    float defRot = (Player.Center - OrbPosition[0]).ToRotation();
                    float end = defRot + (MathHelper.PiOver2 + MathHelper.PiOver4) * 0.5f;
                    float rotation = end - MathHelper.Pi * 3 / 2 * progress * 0.5f;

                    EyeTarget = Vector2.Lerp(EyeTarget, DisposablePosition + rotation.ToRotationVector2() * 1000, 0.2f);

                    if ((int)AITimer % 3 == 0 && AITimer > 20)
                        Projectile.NewProjectile(null, OrbPosition[0] + rotation.ToRotationVector2() * 300,
                            (EyeTarget - OrbPosition[0]).SafeNormalize(Vector2.UnitX),
                            ModContent.ProjectileType<CrystalSlice>(), 25, 0);
                }
                else
                    IncrementSubstate(leaveTimer2: true, leaveTimer3: true, leaveLocals: true);

                break;

            // Impale move #2
            case 6:
                if ((int)AITimer == 70)
                    Slice();

                if ((int)AITimer == 90)
                {
                    for (int i = 0; i < 25; i++)
                        Projectile.NewProjectile(null, EyeTarget,
                            (MathHelper.TwoPi * i / 25f).ToRotationVector2(),
                            ModContent.ProjectileType<CrystalSlice>(), 25, 0, ai2: 3);
                }

                if ((int)AITimer == 100)
                    SoundEngine.PlaySound(
                        new SoundStyle("TheOracle/Assets/Sounds/bigExplosion") with
                        {
                            Volume = 2, PitchVariance = 0.1f, Pitch = .5f
                        });

                if (AITimer < 70)
                {
                    NPC.localAI[1] = 0;
                    AITimer2 = MathF.Pow(AITimer / 70f, 2);

                    OrbPosition[0] = Vector2.Lerp(OrbPosition[0],
                                         Player.Center + (OrbPosition[0] - Player.Center).SafeNormalize(Vector2.UnitX)
                                         .RotatedBy(MathF.Sin(ConstantTimer * 0.06f) * 0.05f) *
                                         (700 + MathF.Sin(ConstantTimer * 0.06f) * 100 + AITimer2 * 400), 0.1f) +
                                     Main.rand.NextVector2Unit() * AITimer2 * 25;
                    if (AITimer < 60)
                        EyeTarget = Vector2.Lerp(EyeTarget, Player.Center, 0.1f);
                    else
                        EyeTarget = Player.Center +
                                    (Player.Center - OrbPosition[0]).SafeNormalize(Vector2.UnitX) * 1000;
                }
                else if (AITimer < 90)
                {
                    OrbPosition[0] += (EyeTarget - OrbPosition[0]).SafeNormalize(Vector2.UnitX) *
                                      MathHelper.Lerp(100, 150, (AITimer - 70f) / 20f);
                    NPC.localAI[1] = 2 + MathHelper.Clamp((AITimer - 70) / 15f, 0, 1);
                }
                else if (AITimer < 112)
                {
                    AITimer3 = MathHelper.SmoothStep(2, 0, (AITimer - 90) / 20f);
                }
                else
                    return ResetTo((int)AIState + 1, 80);

                break;
        }

        if (Substate is >= 1 and < 7 && (Substate == 1 ? AITimer > 90 : true))
            AITimer3 = MathHelper.Lerp(AITimer3, 2f, 0.2f);

        AITimer2 = MathHelper.Lerp(AITimer2, 0, 0.1f);
        return OrbClockHandSwordForm;
    }

    int DoMagicRain()
    {
        IdleCrystal();

        switch (Substate)
        {
            // Telegraph
            case 0:
                for (int i = 0; i < 4; i++)
                {
                    float fac = MathF.Sin(ConstantTimer * 0.05f + (MathHelper.TwoPi * i / 4f));
                    Vector2 pos = NPC.Center - new Vector2(0, 300) +
                                  new Vector2((i - 1.5f) * 100, 40 + fac * 20);
                    OrbPosition[i] = Vector2.Lerp(OrbPosition[i], pos, 0.1f);

                    if (AITimer > 20 && (int)AITimer % 5 == 0 && Main.netMode != 1)
                    {
                        if (AITimer >= 60 && AITimer < 170)
                        {
                            foreach (Projectile proj in Main.ActiveProjectiles)
                            {
                                if (proj.type == ModContent.ProjectileType<OracleJetBeam>() &&
                                    proj.Distance(OrbPosition[i]) < 10f)
                                    OrbPosition[i] += new Vector2(0, 5).RotatedByRandom(0.5f);
                            }
                        }

                        if (AITimer < 130 && Main.rand.Next(4) == i)
                            Projectile.NewProjectile(null, OrbPosition[i], -Vector2.UnitY.RotatedByRandom(0.25f),
                                ModContent.ProjectileType<OracleJetBeam>(), 25, 0, ai2: 1);
                    }
                }

                if ((int)AITimer == 175)
                {
                    Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ModContent.ProjectileType<Flare>(), 0, 0,
                        -1,
                        1,
                        1);
                    Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ModContent.ProjectileType<Flare>(), 0, 0,
                        -1,
                        3,
                        1, 0.1f);

                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDustPerfect(NPC.Center, ModContent.DustType<GlowDust>(),
                            Main.rand.NextVector2Unit() * Main.rand.NextFloat(5, 8), 0,
                            Color.CornflowerBlue with { A = 0 });
                    }

                    if (!Main.dedServ)
                        Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center,
                            Main.rand.NextVector2Unit(),
                            AITimer / 10f + 2, 20, 5, 1000));
                }

                EyeTarget = Vector2.Lerp(EyeTarget,
                    NPC.Center - new Vector2(0, 300) + Main.rand.NextVector2Unit() * AITimer / 180f * 100,
                    AITimer / 180f * 0.5f);

                if (AITimer > 180)
                    IncrementSubstate();
                break;

            // Main part
            case 1:
                NPC.velocity = Vector2.Lerp(NPC.velocity, (Player.Center - new Vector2(0, 240) - NPC.Center) * 0.05f,
                    0.04f * (Substate == 1 ? MathHelper.Clamp(AITimer / 20f, 0, 1) : 1));
                IdleOrbs(AITimer2 = MathHelper.Lerp(AITimer2, 1, 0.04f));
                EyeTarget = Vector2.Lerp(EyeTarget, Player.Center, 0.1f);

                if (Main.netMode != 1 && (int)AITimer % 6 == 0 && AITimer < 120)
                {
                    Projectile.NewProjectile(null, Player.Center + new Vector2(Main.rand.NextFloat(-600, 600), -1260),
                        Vector2.UnitY.RotatedByRandom(0.25f),
                        ModContent.ProjectileType<OracleJetBeam>(), 25, 0, ai2: 1);
                }

                if (AITimer > 140)
                    return ResetTo((int)AIState + 1, 80);
                break;
        }

        return MagicRain;
    }

    int DoTeleportOrbWeb()
    {
        if (AITimer < 60)
        {
            EyeTarget = Vector2.Lerp(EyeTarget, NPC.Center, 0.1f);
            Vector2 pos = NPC.Center +
                          Main.rand.NextVector2Unit() * 700;
            for (int i = 0; i < 3; i++)
                Dust.NewDustPerfect(pos, ModContent.DustType<GlowDust>(),
                    (NPC.Center - pos).SafeNormalize(Vector2.Zero) * 16,
                    newColor: Color.CornflowerBlue with { A = 0 }, Scale: 0.9f).noGravity = true;
        }
        else if (AITimer < 180)
        {
            if (AITimer > 120)
                EyeTarget = Vector2.Lerp(EyeTarget, NPC.Center + new Vector2(300, 0).RotatedBy(ConstantTimer * 0.3f),
                    0.1f);
            if (AITimer % 7 == 0)
            {
                Vector2 pos = Player.Center + new Vector2(500, 225).RotatedBy(ConstantTimer) *
                    Main.rand.NextFloat(0.5f, 1.5f) * (AITimer % 20 == 0 ? -1 : 1);
                if (DisposablePosition == Vector2.Zero)
                    DisposablePosition = pos;
                Vector2 vel = Main.rand.NextVector2Unit() * 0.6f;
                Vector2 cachePos = DisposablePosition;
                if (DisposablePosition != Vector2.Zero)
                    DisposablePosition = pos;

                Projectile.NewProjectile(null, DisposablePosition, vel,
                    ModContent.ProjectileType<WebBeam>(),
                    25, 0, -1, cachePos.X, cachePos.Y, 480 - AITimer);
            }
        }
        else
            return ResetTo((int)AIState + 1, 80);

        return TeleportOrbWeb;
    }

    int DoSweepingProjectilesThatReverse()
    {
        for (int i = 0; i < 4; i++)
            OrbPosition[i] = Vector2.Lerp(OrbPosition[i],
                NPC.Center +
                new Vector2(120 + MathF.Sin(ConstantTimer) * 20).RotatedBy(ConstantTimer * 0.1f +
                                                                           MathHelper.TwoPi * i / 4f), 0.1f);
        IdleCrystal();
        if (AITimer < 10)
            EyeTarget = Player.Center;
        else
            EyeTarget = Vector2.Lerp(EyeTarget, NPC.Center, 0.1f);
        if ((int)AITimer == 30)
        {
            Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ModContent.ProjectileType<TimeReverseVFX>(), 0, 0,
                ai0: 2);
        }

        if (AITimer is > 85 and < 150 && (int)AITimer % 2 == 0)
        {
            Projectile.NewProjectile(null, NPC.Center, Main.rand.NextVector2Unit(),
                ModContent.ProjectileType<OracleBlastReversal>(), 25, 0);
        }

        if ((int)AITimer == 300)
        {
            Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ModContent.ProjectileType<TimeReverseVFX>(), 0, 0);
        }

        if (AITimer > 430)
            return ResetTo((int)AIState + 1, 80);

        return SweepingProjectilesThatReverse;
    }

    int DoLaserRefraction()
    {
        if (AITimer < 100)
        {
            IdleOrbs();
            EyeTarget = Vector2.Lerp(EyeTarget, NPC.Center, 0.1f);
        }
        else
        {
            EyeTarget = Vector2.Lerp(EyeTarget, CrystalPosition, 0.1f);
            for (int i = 0; i < 4; i++)
                OrbPosition[i] = Vector2.Lerp(OrbPosition[i],
                    NPC.Center - new Vector2(0, 300) +
                    new Vector2(200 * MathF.Sin(ConstantTimer * 0.1f + MathHelper.TwoPi * i / 8f),
                        MathF.Sin(ConstantTimer * 0.1f + MathHelper.TwoPi * i / 4f) * 150),
                    0.05f);
        }

        CrystalPosition = Vector2.Lerp(CrystalPosition,
            NPC.Center - new Vector2(0, 300) + new Vector2(10).RotatedBy(ConstantTimer * 0.02f), 0.1f);
        CrystalRotation = CrystalRotation.AngleLerp(MathF.Sin(ConstantTimer * 0.02f) * -0.2f, 0.1f);
        if ((int)AITimer == 30)
        {
            Projectile.NewProjectile(null, NPC.Center, Vector2.Zero,
                ModContent.ProjectileType<WebBeam>(),
                25, 0, -1, CrystalPosition.X, CrystalPosition.Y, 100);
        }

        if (AITimer is > 30 and < 65)
        {
            Vector2 pos = CrystalPosition +
                          Main.rand.NextVector2Unit() * 700;
            for (int i = 0; i < 3; i++)
                Dust.NewDustPerfect(pos, ModContent.DustType<GlowDust>(),
                    (CrystalPosition - pos).SafeNormalize(Vector2.Zero) * 16,
                    newColor: Color.CornflowerBlue with { A = 0 }, Scale: 0.9f).noGravity = true;
        }

        if (AITimer is > 110 and <= 120)
        {
            CrystalFlash = 2;
            Projectile.NewProjectile(null, Vector2.Lerp(NPC.Center, CrystalPosition, (AITimer - 110) / 10f),
                Vector2.Zero, ModContent.ProjectileType<Flare>(), 0, 0);
        }

        if (AITimer is > 120 and < 130)
        {
            CrystalFlash = 2;
            Projectile.NewProjectile(null, CrystalPosition + Main.rand.NextVector2Circular(50, 50),
                Vector2.Zero, ModContent.ProjectileType<Flare>(), 0, 0);
        }

        if (AITimer is > 150 and < 300 && (int)AITimer % 3 == 0)
        {
            Vector2 pos = CrystalPosition + Main.rand.NextVector2CircularEdge(1700, 1700);
            Projectile.NewProjectile(null, CrystalPosition, Vector2.Zero,
                ModContent.ProjectileType<WebBeam>(),
                25, 0, -1, pos.X, pos.Y, 200);
        }

        if (AITimer is > 90 and < 330 && (int)AITimer % 10 == 0)
            Projectile.NewProjectile(null, NPC.Center, Vector2.Zero,
                ModContent.ProjectileType<WebBeam>(),
                25, 0, -1, CrystalPosition.X, CrystalPosition.Y, 100);

        if (AITimer > 350)
            return ResetTo((int)AIState + 1, 80);
        return LaserRefraction;
    }

    int DoHourGlassOrbs()
    {
        if (AITimer < 60)
        {
            CrystalPosition = Vector2.Lerp(CrystalPosition, Player.Center - new Vector2(0, 300), 0.1f);
            for (int i = 0; i < 4; i++)
                OrbPosition[i] = Vector2.Lerp(OrbPosition[i],
                    CrystalPosition + new Vector2(100).RotatedBy(ConstantTimer * 0.01f + MathHelper.TwoPi * i / 4f),
                    0.1f);
        }

        else
        {
            if ((int)AITimer == 61)
                Projectile.NewProjectile(null, CrystalPosition, Vector2.Zero,
                    ModContent.ProjectileType<OracleMiniClock>(), 0,
                    0, ai2: 2);

            AITimer2 = MathHelper.Lerp(AITimer2, 0.25f, 0.1f);
            if (AITimer > 180 && AITimer3 < MathHelper.Pi)
                AITimer3 += MathHelper.Pi / 280f;

            CrystalRotation = AITimer3;

            Vector2[] pos =
            [
                new(20, 0),
                new(150, 200),
                new(0, 250),
                new(-150, 200),
                new(-20, 0),
                new(-150, -200),
                new(0, -250),
                new(150, -200),
                new(20, 0)
            ];

            if ((AITimer is > 70 and < 180 || (AITimer > 180 && AITimer3 < MathHelper.Pi)) & ConstantTimer % 10 == 0)
            {
                if (AITimer > 80)
                    for (int i = 0; i < 6; i++)
                    {
                        int index = i + (i > 2 ? 2 : 1);

                        int dir = index switch
                        {
                            1 => -1,
                            3 => 1,
                            5 => -1,
                            7 => 1,
                            _ => 0
                        };
                        Projectile.NewProjectile(null,
                            CrystalPosition + pos[index].RotatedBy(AITimer3) -
                            (pos[index].RotatedBy(AITimer3)).SafeNormalize(Vector2.UnitY) * 40,
                            (-pos[index]).SafeNormalize(Vector2.UnitY),
                            ModContent.ProjectileType<OracleJetBeam>(), 15, 0, ai1: dir * MathHelper.PiOver4,
                            ai2: 2);
                    }

                for (int i = 0; i < Main.rand.Next(1, 5); i++)
                {
                    for (int j = 0; j < 2; j++)
                        Projectile.NewProjectile(null,
                            Player.Center +
                            new Vector2(Main.rand.NextFloat(-700, 1000), -700 + 1400 * j).RotatedBy(AITimer3),
                            Vector2.UnitY.RotatedBy(AITimer3) * Main.rand.NextFloat(5, 9),
                            ModContent.ProjectileType<OracleBlast>(),
                            25,
                            0, ai1: AITimer, ai2: 4);
                }
            }

            for (int j = 0; j < pos.Length; j++)
                pos[j] = CrystalPosition + pos[j].RotatedBy(AITimer3);

            for (int i = 0; i < 4; i++)
            {
                float t = (i / 4f + ConstantTimer * 0.01f) % 1f;

                OrbPosition[i] = Vector2.Lerp(OrbPosition[i], OrbPosition[i].MultiLerp(t, pos), AITimer2);

                if (ConstantTimer % 4 == i)
                    Projectile.NewProjectile(null, OrbPosition[i], Vector2.Zero, ModContent.ProjectileType<OrbClone>(),
                        0, 0);
            }
        }

        if (AITimer3 >= MathHelper.Pi)
            return ResetTo((int)AIState + 1, 80);

        return HourGlassOrbs;
    }

    int DoGiantClockLaser()
    {
        IdleCrystal();
        switch (Substate)
        {
            // Init
            case 0:
                if (AITimer2 < 1)
                    AITimer3 = 1;
                EyeTarget = Vector2.Lerp(EyeTarget, Player.Center, 0.1f);
                IdleOrbs(AITimer3);
                AITimer3 = MathHelper.Lerp(AITimer3, 1, 0.04f);
                NPC.velocity = Vector2.Lerp(NPC.velocity, (Player.Center - NPC.Center) * 0.04f, 0.1f);
                if (AITimer > 60)
                {
                    IncrementSubstate(leaveTimer2: true);
                    AITimer3 = ConstantTimer;
                }

                break;

            // Main part
            case 1:
                Vector2 pos = (int)AITimer2 switch
                {
                    0 => NPC.Center - new Vector2(0, 400),
                    1 => NPC.Center - new Vector2(0, 200).RotatedBy(AITimer3),
                    _ => NPC.Center + new Vector2(0, 400).RotatedBy(AITimer3)
                };
                EyeTarget = Vector2.Lerp(EyeTarget, pos, 0.1f);
                NPC.velocity *= 0.9f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f +
                                  MathHelper.ToRadians(ConstantTimer + AITimer * AITimer * 0.1f);
                    OrbPosition[i] = Vector2.Lerp(OrbPosition[i], pos + new Vector2(100).RotatedBy(angle), 0.1f);
                }

                if ((int)AITimer == 30)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero,
                        ModContent.ProjectileType<OracleBladeClock>(), 25, 0);
                }

                // Attack loops twice in phase 2
                if (NPC.life < NPC.lifeMax / 2 && AITimer > 210)
                {
                    AITimer2++;
                    if (AITimer2 < 3)
                    {
                        AITimer = 0;
                        AITimer3 = 0;
                        Substate = 0;
                        return GiantClockLaser;
                    }
                }

                if (AITimer > 210)
                    return ResetTo((int)AIState + 1, 80);

                break;
        }

        return GiantClockLaser;
    }

    int DoMagolorFields()
    {
        IdleCrystal();
        if (Substate > 0)
            NPC.velocity *= 0.9f;
        switch (Substate)
        {
            // Init
            case 0:
                IdleOrbs();
                if (AITimer < 60)
                    NPC.velocity = Vector2.Lerp(NPC.velocity, (Player.Center - NPC.Center) * 0.04f, 0.1f);
                else
                    NPC.velocity *= 0.9f;

                if (AITimer > 120)
                {
                    IncrementSubstate();
                    DisposablePosition = Player.Center;

                    // Shape is determined by the Substate
                    Substate = new UnifiedRandom(ConstantTimer).Next(1, 3);
                }

                break;

            // V
            case 1:
                EyeTarget = Vector2.Lerp(EyeTarget, OrbPosition[1], 0.1f);
                if (AITimer < 60)
                {
                    OrbPosition[3] = NPC.Center +
                                     new Vector2(150 + MathF.Sin(ConstantTimer * 0.025f + 3 * 10) * 20).RotatedBy(
                                         MathHelper.TwoPi * 3 / (float)OrbPosition.Length + IdleSwayFactor * 0.2f);

                    OrbPosition[2] = Vector2.Lerp(OrbPosition[2], DisposablePosition + new Vector2(0, 300), 0.1f);

                    for (int i = 0; i < 2; i++)
                    {
                        OrbPosition[i] = Vector2.Lerp(OrbPosition[i],
                            DisposablePosition + new Vector2(350 * (i == 0 ? -1 : 1), -400), 0.1f);
                    }
                }

                if (AITimer is > 30 and < 100)
                {
                    for (int j = 0; j < 1 + (AITimer > 80).ToInt() * 3; j++)
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 pos = OrbPosition[i] + Main.rand.NextVector2CircularEdge(500, 500) *
                            Main.rand.NextFloat(0.9f, 1.1f);
                        Dust.NewDustPerfect(pos, ModContent.DustType<GlowDust>(),
                            (OrbPosition[i] - pos).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(8, 15), 0,
                            Color.CornflowerBlue with { A = 0 }, Main.rand.NextFloat(0.9f, 1f));
                    }
                }

                if ((int)AITimer == 80)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 pos = OrbPosition[2];
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), OrbPosition[i], Vector2.Zero,
                            ModContent.ProjectileType<WebBeam>(), 25, 0, ai0: pos.X, ai1: pos.Y, ai2: 190);

                        pos = OrbPosition[i];
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), OrbPosition[2], Vector2.Zero,
                            ModContent.ProjectileType<WebBeam>(), 25, 0, ai0: pos.X, ai1: pos.Y, ai2: 190);
                    }
                }

                if (AITimer > 230)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (float j = 0; j < 1; j += 0.1f)
                        {
                            for (int k = -1; k < 2; k += 2)
                            {
                                Vector2 pos = Vector2.Lerp(OrbPosition[i], OrbPosition[2], j);
                                Vector2 vel = (OrbPosition[2] - OrbPosition[i]).SafeNormalize(Vector2.UnitX)
                                              .RotatedBy(MathHelper.PiOver2 + (i == 0 ? 0 : MathHelper.Pi)) * k *
                                              (3 + MathF.Floor(MathF.Sin(MathHelper.Pi * j) * 10) / 10f * 6);

                                if (ConstantTimer % 2 == 0 && Main.rand.NextBool())
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(),
                                        pos + Main.rand.NextVector2Circular(5, 5), Vector2.Zero,
                                        ModContent.ProjectileType<Flare>(), 25, 0);

                                if (AITimer > 250)
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, vel,
                                        ModContent.ProjectileType<OracleBlast>(), 25, 0, ai2: 5);
                            }
                        }
                    }

                    if (AITimer > 250)
                        return ResetTo((int)AIState + 1, 80);
                }

                break;

            // X
            case 2:
                EyeTarget = Vector2.Lerp(EyeTarget, OrbPosition[1], 0.1f);
                if (AITimer < 60)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        OrbPosition[i] = Vector2.Lerp(OrbPosition[i],
                            DisposablePosition + new Vector2(300).RotatedBy(MathHelper.TwoPi * i / 4f), 0.1f);
                    }
                }

                if (AITimer is > 30 and < 100)
                {
                    for (int j = 0; j < 1 + (AITimer > 80).ToInt() * 3; j++)
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 pos = OrbPosition[i] + Main.rand.NextVector2CircularEdge(500, 500) *
                            Main.rand.NextFloat(0.9f, 1.1f);
                        Dust.NewDustPerfect(pos, ModContent.DustType<GlowDust>(),
                            (OrbPosition[i] - pos).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(8, 15), 0,
                            Color.CornflowerBlue with { A = 0 }, Main.rand.NextFloat(0.9f, 1f));
                    }
                }

                if ((int)AITimer == 80)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 pos = OrbPosition[(i + 2) % 4];
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), OrbPosition[i], Vector2.Zero,
                            ModContent.ProjectileType<WebBeam>(), 25, 0, ai0: pos.X, ai1: pos.Y, ai2: 250);

                        pos = OrbPosition[i];
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), OrbPosition[(i + 2) % 4], Vector2.Zero,
                            ModContent.ProjectileType<WebBeam>(), 25, 0, ai0: pos.X, ai1: pos.Y, ai2: 250);
                    }
                }

                if ((int)AITimer == 140)
                {
                    int amt = 3;
                    for (int j = -amt; j < amt; j++)
                    for (int k = -amt; k < amt; k++)
                    {
                        Vector2 off = new Vector2(600 * j, 600 * k);
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 pos = OrbPosition[(i + 2) % 4] + off;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), OrbPosition[i] + off, Vector2.Zero,
                                ModContent.ProjectileType<WebBeam>(), 25, 0, ai0: pos.X, ai1: pos.Y, ai2: 190);
                        }
                    }
                }

                if (AITimer > 220)
                    return ResetTo((int)AIState + 1, 80);

                break;
        }

        return MagolorFields;
    }

    int DoSigilCannonballs()
    {
        IdleCrystal(1);
        switch (Substate)
        {
            // Init
            case 0:
                IdleOrbs();
                NPC.localAI[0] = MathHelper.Lerp(NPC.localAI[0], 1, 0.05f);

                if (AITimer > 20)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Dust.NewDustPerfect(OrbPosition[i] - new Vector2(0, 10), ModContent.DustType<GlowDustSine>(),
                            -Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(10, 20), 0,
                            Color.CornflowerBlue with { A = 0 }, Scale: AITimer / 60f);
                    }
                }

                if (AITimer > 50)
                    IncrementSubstate(leaveLocals: true);
                break;

            // Fire
            case 1:
                NPC.velocity = Vector2.Lerp(NPC.velocity,
                    (Player.Center + new Vector2(Player.velocity.X * 25, -200) - NPC.Center) * 0.06f, 0.1f);

                EyeTarget = Vector2.Lerp(EyeTarget, NPC.Center - new Vector2(0, 50), 0.1f);
                for (int i = 0; i < 4; i++)
                {
                    if (ConstantTimer % 4 == i)
                        Dust.NewDustPerfect(OrbPosition[i] - new Vector2(0, 10), ModContent.DustType<GlowDustSine>(),
                            -Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(5, 10), 0,
                            Color.CornflowerBlue with { A = 0 });

                    OrbPosition[i] = Vector2.Lerp(OrbPosition[i],
                        NPC.Center - new Vector2((i - 1.5f) * 400, 200 - 100 * MathF.Abs(i - 1.5f)),
                        0.025f + MathF.Abs(i - 1.5f) * 0.1f);

                    if (ConstantTimer % 17 == 0 && ConstantTimer % 4 == i && AITimer < 220)
                    {
                        for (int j = 0; j < Main.rand.Next(1, 4); j++)
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), OrbPosition[i],
                                new Vector2(Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-20, -15)),
                                ModContent.ProjectileType<Cannonball>(), 40, 0, ai0: Main.rand.NextFloat(7, 24));

                        AITimer3 = i;
                        AITimer2 = 1f;
                    }
                }

                if (AITimer2 > 0)
                {
                    AITimer2 -= 0.1f;
                    OrbPosition[(int)AITimer3].Y += AITimer2 * 4;
                }

                if (AITimer is > 100 and < 240)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 orb = OrbPosition[Main.rand.Next(4)];
                        Vector2 pos = orb +
                                      Main.rand.NextVector2Unit() * 500;
                        Dust.NewDustPerfect(pos, ModContent.DustType<GlowDust>(),
                            (orb - new Vector2(0, 40) - pos).SafeNormalize(Vector2.Zero) * 10,
                            newColor: Color.CornflowerBlue with { A = 0 },
                            Scale: MathHelper.Lerp(0.9f, 2, (AITimer - 100) / 140f)).noGravity = true;
                    }
                }

                if (AITimer > 300)
                {
                    SoundEngine.PlaySound(new SoundStyle("TheOracle/Assets/Sounds/OracleBoss/ClockBell"));
                    for (int i = 0; i < 4; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), OrbPosition[i], Vector2.Zero,
                            ModContent.ProjectileType<Explosion>(), 40, 0);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), OrbPosition[i], Vector2.Zero,
                            ModContent.ProjectileType<Flare>(), 0, 0, ai0: 3);
                    }

                    IncrementSubstate();
                }

                break;

            // Breather
            case 2:
                NPC.velocity *= 0.98f;
                if (AITimer > 20)
                    return ResetTo(OrbitalBeamPortals);
                break;
        }

        return SigilCannonballs;
    }

    int DoOrbitalBeamPortals()
    {
        NPC.velocity = Vector2.Lerp(NPC.velocity,
            (Player.Center + new Vector2(Player.velocity.X * 25, -200) - NPC.Center) * 0.06f, 0.1f);
        switch (Substate)
        {
            // Init
            case 0:

                if (AITimer > 50)
                    IncrementSubstate();
                break;

            // Fly overhead
            case 1:
                break;

            // Additional phase 2 burst
            case 2:
                break;
        }

        return OrbitalBeamPortals;
    }

    int DoPolaritiesClocks()
    {
        return PolaritiesClocks;
    }

    int DoQuartzWatch()
    {
        return QuartzWatch;
    }

    int DoElfilisOrbClones()
    {
        return ElfilisOrbClones;
    }

    int DoCrystalCrackHoming()
    {
        return CrystalCrackHoming;
    }
}
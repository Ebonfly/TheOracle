using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using TheOracle.Common.Utils;

namespace TheOracle.Content.NPCs;

public partial class OracleBoss : ModNPC
{
    readonly Vector2 _mainOrigin = new Vector2(202, 306) * 0.5f;
    public float CrystalFlash, CrystalOpacity = 1f;


    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy) return false;
        Texture2D underlayer = Images.NPCs.Textures.OracleBoss_Underlayer[0];
        Texture2D underlayer2 = Images.NPCs.Textures.OracleBoss_Underlayer[1];

        Texture2D crystalTex = Images.NPCs.Textures.OracleCrystal;
        Texture2D orbTex = Images.NPCs.Textures.OracleOrbs;

        spriteBatch.Draw(underlayer, NPC.Center - screenPos, null, drawColor, NPC.rotation, _mainOrigin, NPC.scale,
            SpriteEffects.None, 0);

        Rectangle crystalFrame = new Rectangle(0, NPC.frame.X / NPC.frame.Width * 92, 50, 92);

        spriteBatch.Draw(crystalTex, CrystalPosition - screenPos, crystalFrame,
            Color.White with { A = 0 } * CrystalFlash * 2, CrystalRotation,
            crystalFrame.Size() / 2f, NPC.scale + (1 - CrystalFlash) * CrystalOpacity, SpriteEffects.None, 0);

        spriteBatch.Draw(crystalTex, CrystalPosition - screenPos, crystalFrame, Color.White * CrystalOpacity,
            CrystalRotation, crystalFrame.Size() / 2f, NPC.scale, SpriteEffects.None, 0);

        CrystalFlash = MathHelper.Lerp(CrystalFlash, 0, 0.15f);

        spriteBatch.Draw(underlayer2, NPC.Center - screenPos, null, drawColor, NPC.rotation, _mainOrigin, NPC.scale,
            SpriteEffects.None, 0);

        DrawEye(spriteBatch, screenPos);

        DrawJet(spriteBatch, screenPos);

        DrawBody(spriteBatch, screenPos);

        DrawTentacles(spriteBatch, screenPos);

        for (int i = 0; i < OrbPosition.Length; i++)
            spriteBatch.Draw(orbTex, OrbPosition[i] - screenPos, OrbFrame, Color.White, OrbRotation[i],
                OrbFrame.Size() / 2f, NPC.scale, SpriteEffects.None, 0);

        // Special Attack VFX
        if (AITimer >= 0)
        {
            if ((int)AIState == OrbClockHandSwordForm)
                DrawSword(spriteBatch, screenPos);

            if ((int)AIState == SigilCannonballs)
                DrawSigils(spriteBatch, screenPos);
        }

        return false;
    }

    void DrawTentacles(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        List<VertexPositionColorTexture>[] vertices = [new(), new(), new(), new(), new(), new()];
        for (int i = 0; i < 6; i++)
        {
            int direction = (i < 3 ? 1 : -1);
            float startOffset = ((i + 2) % 3f) * 4 * direction;
            Vector2 start = NPC.Center +
                new Vector2(_mainOrigin.X * 0.67f * direction + startOffset,
                    _mainOrigin.Y * 0.115f - startOffset * direction * 0.5f).RotatedBy(NPC.rotation) - screenPos;

            Vector2 end = start +
                          new Vector2(-NPC.velocity.X * 4, 40 - MathHelper.Clamp(NPC.velocity.Y * 4, -70, 70))
                              .RotatedBy(
                                  i % 3f * 0.3f * -direction);

            Vector2 halfway = Vector2.SmoothStep(start, end, 0.5f) +
                              new Vector2((i < 3 ? 1 : -1), 0).RotatedBy(NPC.rotation) * 10;

            for (float j = 0; j < 1f; j += 0.1f)
            {
                Vector2 pos = Vector2.Lerp(Vector2.Lerp(start, halfway, j * 2), Vector2.Lerp(halfway, end, j * 2), j);
                Color col = Color.CornflowerBlue * MathF.Pow(1 - j, 2) *
                            (1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 10) * 0.2f) *
                            MathHelper.Lerp(0, 1, MathHelper.Clamp(j * 10, 0, 1));
                Vector2 offset = new Vector2(20, 0).RotatedBy((end - start).ToRotation());
                vertices[i].Add(PrimitiveUtils.AsVertex(pos + offset.RotatedBy(MathHelper.PiOver2), col,
                    new Vector2(-j + Main.GlobalTimeWrappedHourly * 2, 0)));
                vertices[i].Add(PrimitiveUtils.AsVertex(pos + offset.RotatedBy(-MathHelper.PiOver2), col,
                    new Vector2(-j + Main.GlobalTimeWrappedHourly * 2, 1)));
            }
        }

        spriteBatch.End(out var ss);
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap,
            ss.depthStencilState, RasterizerState.CullNone, null, ss.matrix);

        for (int i = 0; i < 6; i++)
        {
            if (vertices[i].Count > 2)
            {
                PrimitiveUtils.DrawTexturedPrimitives(vertices[i].ToArray(), PrimitiveType.TriangleStrip,
                    Images.Extras.Textures.LintyTrail);
                PrimitiveUtils.DrawTexturedPrimitives(vertices[i].ToArray(), PrimitiveType.TriangleStrip,
                    Images.Extras.Textures.Tentacle);
            }
        }

        spriteBatch.End();
        spriteBatch.Begin(ss);
    }

    void DrawJet(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        int index = MathHelper.Clamp(NPC.frame.X / NPC.frame.Width, 0, 3);
        Texture2D usedTex = Images.NPCs.Jet.Textures.OracleJet[index];

        Vector2 start = NPC.Center + new Vector2(0, 46).RotatedBy(NPC.rotation);
        Vector2 end = new Vector2(0, 49 + usedTex.Width).RotatedBy(NPC.oldRot[^1]);
        end += NPC.Center + Vector2.Clamp((NPC.oldPos[^1] + NPC.Size / 2f - NPC.Center), Vector2.One * -60,
            Vector2.One * 60);
        start -= screenPos;
        end -= screenPos;

        List<VertexPositionColorTexture> vertices = new();

        Vector2 half = start + new Vector2(0, usedTex.Width / 2f).RotatedBy(NPC.rotation);
        for (float j = 0; j < 1f; j += 0.2f)
        {
            Vector2 pos = Vector2.Lerp(Vector2.Lerp(start, half, j), Vector2.Lerp(half, end, j), j);
            for (int i = -1; i < 2; i += 2)
            {
                Vector2 position = pos +
                                   new Vector2(0, usedTex.Height * 0.6f).RotatedBy(
                                       NPC.rotation + MathHelper.PiOver2 * i);
                vertices.Add(new VertexPositionColorTexture(
                    new Vector3(position.X, position.Y, 0),
                    Color.White,
                    new Vector2(j, (i + 1) * 0.5f)));
            }
        }

        if (vertices.Count > 2)
        {
            spriteBatch.End(out var ss);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap,
                ss.depthStencilState, RasterizerState.CullNone, null, ss.matrix);

            PrimitiveUtils.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, usedTex, false,
                true);
            spriteBatch.End();
            spriteBatch.Begin(ss);
        }
    }

    void DrawEye(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        Texture2D eyeTex = Images.NPCs.Textures.OracleEye;
        Rectangle eyeFrame = new Rectangle(0, NPC.frame.X / NPC.frame.Width * 20, 18, 20);
        if (Phase2)
            eyeFrame.Y = 4 * 18;

        Vector2 eyeOffset = Vector2.Zero;
        eyeOffset += (EyeTarget - NPC.Center) * 0.1f;
        eyeOffset = Vector2.Clamp(eyeOffset,
            -new Vector2(12, 4 + 4 * MathF.Sin((eyeOffset.X + 12) / 24f * MathF.PI)),
            new Vector2(12, 4 + 4 * MathF.Sin((eyeOffset.X + 12) / 24f * MathF.PI)));
        eyeOffset += new Vector2(0, 5 + MathF.Sin(Main.GlobalTimeWrappedHourly * 3) * 2);
        CacheEyePosition(eyeOffset);

        for (int i = 0; i < _cachedEyeOffsets.Length; i++)
        {
            spriteBatch.Draw(eyeTex, NPC.Center + _cachedEyeOffsets[i] - screenPos, new Rectangle(0, 0, 18, 20),
                Color.Gray * MathF.Pow(1f - i / (float)_cachedEyeOffsets.Length, 2f), 0, new Vector2(18) / 2,
                NPC.scale + MathF.Sin(Main.GlobalTimeWrappedHourly * 2) * 0.3f,
                SpriteEffects.None, 0);
        }

        spriteBatch.Draw(eyeTex, NPC.Center + eyeOffset - screenPos, eyeFrame, Color.White, 0, new Vector2(18) / 2,
            NPC.scale,
            SpriteEffects.None, 0);
    }

    void DrawBody(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        Texture2D tex = Images.NPCs.Textures.OracleBoss;
        Texture2D glow = Images.NPCs.Textures.OracleBoss_Glow;

        Vector4 LightingColor(Vector2 offset) =>
            new Vector4(Lighting.GetSubLight(NPC.Center + offset.RotatedBy(NPC.rotation)), 1) * 1.25f;

        if (Effects.FourPointGradient.IsReady)
        {
            spriteBatch.End(out var ss);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap,
                ss.depthStencilState,
                ss.rasterizerState, Effects.FourPointGradient.Value, ss.matrix);


            Effects.FourPointGradient.ColorTL = LightingColor(-_mainOrigin * 0.75f);

            Effects.FourPointGradient.ColorTR =
                LightingColor(new Vector2(_mainOrigin.X, -_mainOrigin.Y) * 0.75f);

            Effects.FourPointGradient.ColorBL =
                LightingColor(new Vector2(-_mainOrigin.X, _mainOrigin.Y) * 0.75f);

            Effects.FourPointGradient.ColorBR = LightingColor(_mainOrigin * 0.75f);

            spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, _mainOrigin, NPC.scale,
                SpriteEffects.None, 0);

            spriteBatch.End();
            spriteBatch.Begin(ss);
        }
        else
        {
            spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, _mainOrigin, NPC.scale,
                SpriteEffects.None, 0);
        }

        spriteBatch.Draw(glow, NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, _mainOrigin, NPC.scale,
            SpriteEffects.None, 0);
    }

    public Rectangle OrbFrame;

    public override void FindFrame(int frameHeight)
    {
        OrbFrame.Width = 110;
        OrbFrame.Height = 136;
        NPC.frame.Width = 202;

        if (Phase2)
            NPC.frame.Y = frameHeight;

        NPC.frameCounter++;
        if (NPC.frameCounter % 5 == 0)
        {
            if (NPC.frame.X < 3 * NPC.frame.Width)
                NPC.frame.X += NPC.frame.Width;
            else
                NPC.frame.X = 0;

            if (OrbFrame.X < 3 * OrbFrame.Width)
                OrbFrame.X += OrbFrame.Width;
            else
                OrbFrame.X = 0;
        }
    }
}
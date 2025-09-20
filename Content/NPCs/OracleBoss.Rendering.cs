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

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D underlayer = Assets.NPCs.OracleBoss_Underlayer.Value;
        Texture2D underlayer2 = Assets.NPCs.OracleBoss_Underlayer2.Value;

        Texture2D crystalTex = Assets.NPCs.OracleCrystal.Value;
        Texture2D orbTex = Assets.NPCs.OracleOrbs.Value;

        spriteBatch.Draw(underlayer, NPC.Center - screenPos, null, drawColor, NPC.rotation, _mainOrigin, NPC.scale,
            SpriteEffects.None, 0);

        Rectangle crystalFrame = new Rectangle(0, NPC.frame.X / NPC.frame.Width * 92, 50, 92);
        spriteBatch.Draw(crystalTex, CrystalPosition - screenPos, crystalFrame, Color.White, CrystalRotation,
            crystalFrame.Size() / 2f, NPC.scale, SpriteEffects.None, 0);

        spriteBatch.Draw(underlayer2, NPC.Center - screenPos, null, drawColor, NPC.rotation, _mainOrigin, NPC.scale,
            SpriteEffects.None, 0);

        DrawEye(spriteBatch, screenPos);

        DrawJet(spriteBatch, screenPos);

        DrawBody(spriteBatch, screenPos);

        for (int i = 0; i < OrbPosition.Length; i++)
            spriteBatch.Draw(orbTex, OrbPosition[i] - screenPos, OrbFrame, Color.White, OrbRotation[i],
                OrbFrame.Size() / 2, NPC.scale, SpriteEffects.None, 0);
        return false;
    }

    void DrawJet(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        Asset<Texture2D>[] jetTex =
        [
            Assets.NPCs.Jet.OracleJet0, Assets.NPCs.Jet.OracleJet1,
            Assets.NPCs.Jet.OracleJet2, Assets.NPCs.Jet.OracleJet3
        ];
        int index = MathHelper.Clamp(NPC.frame.X / NPC.frame.Width, 0, 3);
        Texture2D usedTex = jetTex[index].Value;

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
        Texture2D eyeTex = Assets.NPCs.OracleEye.Value;
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
        Texture2D tex = Assets.NPCs.OracleBoss.Value;
        Texture2D glow = Assets.NPCs.OracleBoss_Glow.Value;
        Effect lightingEffect = TheOracle.FourPointGradient.Value;

        Vector4 LightingColor(Vector2 offset) =>
            new Vector4(Lighting.GetSubLight(NPC.Center + offset.RotatedBy(NPC.rotation)), 1) * 1.25f;

        spriteBatch.End(out var ss);
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, ss.depthStencilState,
            ss.rasterizerState, lightingEffect, ss.matrix);

        lightingEffect.Parameters["colorTL"].SetValue(LightingColor(-_mainOrigin * 0.75f));
        lightingEffect.Parameters["colorTR"]
            .SetValue(LightingColor(new Vector2(_mainOrigin.X, -_mainOrigin.Y) * 0.75f));
        lightingEffect.Parameters["colorBL"]
            .SetValue(LightingColor(new Vector2(-_mainOrigin.X, _mainOrigin.Y) * 0.75f));
        lightingEffect.Parameters["colorBR"].SetValue(LightingColor(_mainOrigin * 0.75f));

        spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, _mainOrigin, NPC.scale,
            SpriteEffects.None, 0);

        spriteBatch.End();
        spriteBatch.Begin(ss);

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
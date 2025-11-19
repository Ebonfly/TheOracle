using System;
using System.Diagnostics;
using TheOracle.Common.Utils;

namespace TheOracle.Content.NPCs;

public partial class OracleBoss : ModNPC
{
    void DrawSword(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        Texture2D sword = Images.Extras.Textures.ClockHandShort;
        Texture2D sword2 = Images.Extras.Textures.ClockHandLong;
        Texture2D flare = Images.Extras.Textures.Lensflare;
        Texture2D crosslight = Images.Extras.Textures.Crosslight;
        Texture2D clock = Images.Extras.Textures.Clock;
        Texture2D glow = Images.Extras.Textures.Glow;

        float scaleX = MathF.Pow(MathHelper.Clamp(AITimer3 * 0.5f, 0, 2), 2);
        float scale = MathF.Pow(MathHelper.Clamp(AITimer3 - 1f, 0, 2), 2);
        Vector2 position = OrbPosition[0] - screenPos;
        float rotation = (EyeTarget - OrbPosition[0]).ToRotation() + MathHelper.Pi;

        float aura = (Main.GlobalTimeWrappedHourly * 0.5f) % 1f;

        for (int i = 0; i < 2; i++)
        {
            Color color = (i == 0 ? Color.White : Color.CornflowerBlue) with { A = 0 } * scaleX;

            if (NPC.localAI[2] > 0)
            {
                for (int j = 0; j < OldOrbPosition.GetLength(1); j++)
                {
                    float oldRotation = (EyeTarget - OldOrbPosition[0, j]).ToRotation() + MathHelper.Pi;
                    float mult = (1 - j / (float)OldOrbPosition.GetLength(1)) * NPC.localAI[2];

                    spriteBatch.Draw(sword, OldOrbPosition[0, j] - screenPos, null, color * 0.4f * mult,
                        oldRotation + MathHelper.Pi,
                        new Vector2(84, sword.Height / 2f),
                        new Vector2(scaleX, scale * scale + i * 0.07f) * 0.5f, SpriteEffects.None, 0);

                    spriteBatch.Draw(sword2, OldOrbPosition[0, j] - screenPos, null, color * 0.2f * mult, oldRotation,
                        new Vector2(84, sword2.Height / 2f),
                        new Vector2(scaleX, scale * scale + i * 0.07f) * 0.5f, SpriteEffects.None, 0);
                }
            }

            spriteBatch.Draw(sword, position, null, color * 0.1f * MathF.Sin(aura * MathHelper.Pi),
                rotation + MathHelper.Pi, new Vector2(84, sword.Height / 2f),
                new Vector2(scaleX, scale * scale + i * 0.07f) * (0.5f + aura * 0.4f), SpriteEffects.None, 0);

            spriteBatch.Draw(sword2, position, null, color * 0.1f * MathF.Sin(aura * MathHelper.Pi), rotation,
                new Vector2(84, sword2.Height / 2f),
                new Vector2(scaleX, scale * scale + i * 0.07f) * (0.5f + aura * 0.4f),
                SpriteEffects.None, 0);

            spriteBatch.Draw(sword, position, null, color * 0.7f, rotation + MathHelper.Pi,
                new Vector2(84, sword.Height / 2f),
                new Vector2(scaleX, scale * scale + i * 0.07f) * 0.5f, SpriteEffects.None, 0);

            spriteBatch.Draw(sword2, position, null, color * 0.6f, rotation, new Vector2(84, sword2.Height / 2f),
                new Vector2(scaleX, scale * scale + i * 0.07f) * 0.5f, SpriteEffects.None, 0);

            spriteBatch.Draw(flare, position, null, color * (0.9f + MathF.Sin(Main.GlobalTimeWrappedHourly * 2) * 0.3f),
                rotation + MathHelper.PiOver2, flare.Size() / 2f, new Vector2(scale * 0.4f + i * 0.01f, scale * 0.6f),
                SpriteEffects.None, 0);

            spriteBatch.Draw(flare, position, null, color * (0.9f + MathF.Sin(Main.GlobalTimeWrappedHourly * 2) * 0.3f),
                rotation + MathHelper.PiOver2, flare.Size() / 2f, new Vector2(scale * 0.6f + i * 0.01f, scale * 0.3f),
                SpriteEffects.None, 0);

            spriteBatch.Draw(clock, position, null,
                color * (0.9f + MathF.Sin(Main.GlobalTimeWrappedHourly * 2) * 0.3f) * 0.6f,
                0, clock.Size() / 2f, scale * 0.2f + i * 0.005f,
                SpriteEffects.None, 0);

            spriteBatch.Draw(glow, position, null,
                color * MathF.Sin(NPC.localAI[1] * MathHelper.Pi) * 0.5f,
                0, glow.Size() / 2f, scale * 1.4f * NPC.localAI[1] + i * 0.005f,
                SpriteEffects.None, 0);

            spriteBatch.Draw(crosslight, position + new Vector2(-sword2.Width * 0.33f, 0).RotatedBy(rotation),
                null, color * (0.9f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4) * 0.3f) * AITimer2,
                rotation + MathHelper.PiOver4 + Main.GameUpdateCount * 0.1f,
                crosslight.Size() / 2f, scale * AITimer2 + i * 0.01f,
                SpriteEffects.None, 0);

            spriteBatch.Draw(flare, position + new Vector2(-sword2.Width * 0.18f * scaleX, 0).RotatedBy(rotation), null,
                color * 1.4f, rotation, flare.Size() / 2f, new Vector2(scale * 0.7f + i * 0.01f, scale * 0.4f),
                SpriteEffects.None, 0);
        }
    }

    void DrawSigils(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        if (!Effects.SpriteRotation.IsReady)
            return;
        Texture2D sigil = Images.Extras.Textures.Rune_alt;
        Texture2D vortex = Images.Extras.Textures.Vortex;

        Main.spriteBatch.End(out var ss);
        Main.spriteBatch.Begin(ss with { effect = Effects.SpriteRotation.Value });

        Effects.SpriteRotation.Scale = new Vector2(0.25f, 1);
        Effects.SpriteRotation.Color = (Color.CornflowerBlue with { A = 0 }).ToVector4() * 4 * NPC.localAI[0];
        Effects.SpriteRotation.Rotation = Main.GameUpdateCount * 0.1f;
        Effects.SpriteRotation.Apply();

        for (int i = 0; i < 4; i++)
        {
            float fac = MathF.Sin(Main.GlobalTimeWrappedHourly + i);
            Vector2 pos = OrbPosition[i] - new Vector2(0, 20).RotatedBy(fac) -
                          screenPos;

            for (int j = 0; j < 3; j++)
            {
                spriteBatch.Draw(vortex, pos - new Vector2(0, j * 6), null, Color.White,
                    MathHelper.PiOver2 + fac * 0.1f, vortex.Size() / 2f, 0.5f + j * 0.1f,
                    SpriteEffects.None, 0);
            }
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(ss);
    }
}
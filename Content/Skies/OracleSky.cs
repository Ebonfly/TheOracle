using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using TheOracle.Common.Utils;
using TheOracle.Content.NPCs;

namespace TheOracle.Content.Skies;

public class P1SkyHandler : ModSceneEffect
{
    public override bool IsSceneEffectActive(Player player) =>
        NPC.AnyNPCs(ModContent.NPCType<OracleBoss>());

    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override void SpecialVisuals(Player player, bool isActive) =>
        player.ManageSpecialBiomeVisuals("OracleSky", isActive);

    public override void Load()
    {
        Filters.Scene["OracleSky"] =
            new Filter(new ScreenShaderData("FilterMiniTower").UseOpacity(0), EffectPriority.VeryHigh);
        SkyManager.Instance["OracleSky"] = new OracleSky();
    }
}

public class OracleSky : CustomSky
{
    public bool Active;
    public float Intensity;
    public override float GetCloudAlpha() => MathHelper.Lerp(base.GetCloudAlpha(), .05f, Intensity);

    public override void Reset()
    {
        Active = false;
        Intensity = 0f;
    }

    public override bool IsActive() => Intensity > 0;

    public override void Activate(Vector2 position, params object[] args)
    {
        Active = true;
        Intensity += 0.01f;
    }

    public override void Deactivate(params object[] args) => Active = false;

    public float Time => Main.GameUpdateCount * 0.005f;

    public override void Update(GameTime gameTime)
    {
        Intensity = MathHelper.Lerp(Intensity, (float)Active.ToInt(), 0.1f);
        if (Intensity < 0.01f)
            Intensity = 0;
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        if (OracleBoss.AnyOracleIsPhase2 || (maxDepth >= 3.40282347E+38f && minDepth < 3.40282347E+38f))
        {
            spriteBatch.End(out var ss);
            spriteBatch.Begin(ss with
            {
                blendState = BlendState.Additive, samplerState = SamplerState.LinearWrap,
                effect = OracleBoss.AnyOracleIsPhase2 ? Effects.PhaseTwoSky.Shader : Effects.PhaseOneSky.Shader
            });

            if (OracleBoss.AnyOracleIsPhase2)
            {
                Effects.PhaseTwoSky.Time = Time;
                Effects.PhaseTwoSky.Opacity = Intensity * 0.2f;
                if (maxDepth >= 3.40282347E+38f && minDepth < 3.40282347E+38f)
                {
                    Effects.PhaseTwoSky.Opacity = Intensity * 1.5f;
                    Effects.PhaseTwoSky.Time = Time * 0.25f;
                }

                Effects.PhaseTwoSky.ApplySky();
            }
            else
            {
                Effects.PhaseOneSky.Time = Time;
                Effects.PhaseOneSky.Opacity = Intensity;
                Effects.PhaseOneSky.ApplySky();
            }

            Main.graphics.GraphicsDevice.Textures[1] = Images.Noise.Textures.SmearNoise;
            Rectangle rect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            spriteBatch.Draw(Images.Noise.Textures.SwirlyNoise, rect, Color.White);

            spriteBatch.End();
            spriteBatch.Begin(ss);
        }
    }
}
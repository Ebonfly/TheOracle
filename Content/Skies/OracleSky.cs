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
    public float Transition;
    public static float SkyFlash, SkyFlashRate = 0.1f;
    public override float GetCloudAlpha() => MathHelper.Lerp(base.GetCloudAlpha(), .05f, Intensity);

    public override void Reset()
    {
        Active = false;
        Transition = 0f;
        Intensity = 0f;
        SkyFlash = 0f;
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
        SkyFlash = MathHelper.Lerp(SkyFlash, 0, SkyFlashRate);
        if (SkyFlash < 0.01f)
        {
            SkyFlashRate = 0.1f;
            SkyFlash = 0;
        }

        Intensity = MathHelper.Lerp(Intensity, Active.ToInt(), 0.1f);
        if (Intensity < 0.01f)
            Intensity = 0;

        if (OracleBoss.AnyOracleIsPhase2)
            Transition = MathHelper.Lerp(Transition, 1f, 0.001f + MathHelper.Clamp(Transition, 0, 0.01f));
        else
            Transition = MathHelper.Lerp(Transition, 0f, 0.02f);
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        Rectangle rect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
        if (maxDepth >= 3.40282347E+38f && minDepth < 3.40282347E+38f)
        {
            spriteBatch.End(out var ss);
            spriteBatch.Begin(ss with
            {
                blendState = BlendState.Additive, samplerState = SamplerState.LinearWrap,
                effect = Effects.PhaseTwoSky.Shader
            });

            Effects.PhaseTwoSky.Opacity = Intensity * 1.5f * (SkyFlash + 1);
            Effects.PhaseTwoSky.Time = Time * 0.25f;
            Effects.PhaseTwoSky.TransitionTime = Time;
            Effects.PhaseTwoSky.Transition = Transition * 50;
            Effects.PhaseTwoSky.ApplySky();
            Main.graphics.GraphicsDevice.Textures[1] = Images.Noise.Textures.SmearNoise;
            Main.graphics.GraphicsDevice.Textures[2] = Images.Noise.Textures.SmearNoise;
            spriteBatch.Draw(Images.Noise.Textures.SwirlyNoise, rect, Color.White);

            spriteBatch.End();
            spriteBatch.Begin(ss with
            {
                blendState = BlendState.Additive, samplerState = SamplerState.LinearWrap,
                effect = Effects.PhaseOneSky.Shader
            });
            Main.graphics.GraphicsDevice.Textures[1] = Images.Noise.Textures.SmearNoise;
            Main.graphics.GraphicsDevice.Textures[2] = Images.Noise.Textures.SmearNoise;

            Effects.PhaseOneSky.Time = Time;
            Effects.PhaseOneSky.Opacity = Intensity * (SkyFlash + 1);
            Effects.PhaseOneSky.TransitionTime = Time;
            Effects.PhaseOneSky.Transition = Transition * 2;
            Effects.PhaseOneSky.ApplySky();

            spriteBatch.Draw(Images.Noise.Textures.SwirlyNoise, rect, Color.White);

            spriteBatch.End();
            spriteBatch.Begin(ss);
        }

        spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, Color.White with { A = 0 } * SkyFlash);
    }
}
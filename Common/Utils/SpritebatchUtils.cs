using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace TheOracle.Common.Utils;

public readonly struct SpritebatchParameters
{
    public SpriteSortMode sortMode { get; init; }
    public BlendState blendState { get; init; }
    public SamplerState samplerState { get; init; }
    public DepthStencilState depthStencilState { get; init; }
    public RasterizerState rasterizerState { get; init; }
    public Effect effect { get; init; }
    public Matrix matrix { get; init; }

    public SpritebatchParameters(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState,
        DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix matrix)
    {
        this.sortMode = sortMode;
        this.blendState = blendState;
        this.samplerState = samplerState;
        this.depthStencilState = depthStencilState;
        this.rasterizerState = rasterizerState;
        this.effect = effect;
        this.matrix = matrix;
    }
}

public static class SpritebatchUtils
{
    public static SpritebatchParameters Snapshot(this SpriteBatch spriteBatch)
    {
        SpriteSortMode sortMode = spriteBatch.sortMode;
        BlendState blendState = spriteBatch.blendState;
        SamplerState samplerState = spriteBatch.samplerState;
        DepthStencilState depthStencilState = spriteBatch.depthStencilState;
        RasterizerState rasterizerState = spriteBatch.rasterizerState;
        Effect effect = spriteBatch.customEffect;
        Matrix matrix = spriteBatch.transformMatrix;
        SpritebatchParameters sbParams = new(sortMode, blendState, samplerState, depthStencilState, rasterizerState,
            effect, matrix);
        return sbParams;
    }

    public static SpritebatchParameters Snapshot(this SpriteBatch spriteBatch, out SpritebatchParameters sbParams)
    {
        sbParams = spriteBatch.Snapshot();
        return sbParams;
    }

    public static void End(this SpriteBatch spriteBatch, out SpritebatchParameters sbParams)
    {
        sbParams = spriteBatch.Snapshot();
        spriteBatch.End();
    }

    public static void ApplySaved(this SpriteBatch spriteBatch, in SpritebatchParameters sbParams)
    {
        if (spriteBatch.beginCalled)
            spriteBatch.End();
        spriteBatch.Begin(sbParams);
    }

    public static void Begin(this SpriteBatch spriteBatch, in SpritebatchParameters sbParams)
    {
        SpriteSortMode sortMode = sbParams.sortMode;
        BlendState blendState = sbParams.blendState;
        SamplerState samplerState = sbParams.samplerState;
        DepthStencilState depthStencilState = sbParams.depthStencilState;
        RasterizerState rasterizerState = sbParams.rasterizerState;
        Effect effect = sbParams.effect;
        Matrix matrix = sbParams.matrix;
        spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
    }

    public static void BeginDefault(this SpriteBatch spriteBatch) => spriteBatch.Begin(SpriteSortMode.Deferred,
        BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, null,
        Main.GameViewMatrix.TransformationMatrix);

    public static bool BeginCalled(this SpriteBatch spriteBatch) => spriteBatch.beginCalled;

    public static void Reload(this SpriteBatch spriteBatch, SamplerState _samplerState = default)
    {
        if (spriteBatch.beginCalled)
            spriteBatch.End();

        SpriteSortMode sortMode = spriteBatch.sortMode;
        SamplerState samplerState = _samplerState;
        BlendState blendState = spriteBatch.blendState;
        DepthStencilState depthStencilState = spriteBatch.depthStencilState;
        RasterizerState rasterizerState = spriteBatch.rasterizerState;
        Effect effect = spriteBatch.customEffect;
        Matrix matrix = spriteBatch.transformMatrix;
        spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
    }

    public static void Reload(this SpriteBatch spriteBatch, SpriteSortMode sortMode = SpriteSortMode.Deferred)
    {
        if (spriteBatch.beginCalled)
            spriteBatch.End();

        BlendState blendState = spriteBatch.blendState;
        SamplerState samplerState = spriteBatch.samplerState;
        DepthStencilState depthStencilState = spriteBatch.depthStencilState;
        RasterizerState rasterizerState = spriteBatch.rasterizerState;
        Effect effect = spriteBatch.customEffect;
        Matrix matrix = spriteBatch.transformMatrix;
        spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
    }

    public static void Reload(this SpriteBatch spriteBatch, BlendState blendState = default)
    {
        if (spriteBatch.beginCalled)
            spriteBatch.End();

        SpriteSortMode sortMode = spriteBatch.sortMode;
        SamplerState samplerState = spriteBatch.samplerState;
        DepthStencilState depthStencilState = spriteBatch.depthStencilState;
        RasterizerState rasterizerState = spriteBatch.rasterizerState;
        Effect effect = spriteBatch.customEffect;
        Matrix matrix = spriteBatch.transformMatrix;
        spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
    }

    public static void Reload(this SpriteBatch spriteBatch, Effect effect = null)
    {
        if (spriteBatch.beginCalled)
            spriteBatch.End();

        SpriteSortMode sortMode = spriteBatch.sortMode;
        BlendState blendState = spriteBatch.blendState;
        SamplerState samplerState = spriteBatch.samplerState;
        DepthStencilState depthStencilState = spriteBatch.depthStencilState;
        RasterizerState rasterizerState = spriteBatch.rasterizerState;
        Matrix matrix = spriteBatch.transformMatrix;
        spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
    }
}
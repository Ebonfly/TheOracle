using TheOracle.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace TheOracle.Common.Graphics;

public abstract class CommonRenderTarget : ARenderTargetContentByRequest, INeedRenderTargetContent, ILoadable
{
    public RenderTarget2D _target2;

    public void RequestAndPrepare(bool? conditional = null)
    {
        if (!conditional ?? false)
            return;
        Request();
        //PrepareRenderTarget(Main.graphics.GraphicsDevice, Main.spriteBatch);
    }

    public void PrepareATarget(ref RenderTarget2D rt, GraphicsDevice gd, int? width = null, int? height = null,
        RenderTargetUsage usage = RenderTargetUsage.PlatformContents) =>
        PrepareARenderTarget_AndListenToEvents(ref rt, gd, width ?? Main.screenWidth, height ?? Main.screenHeight,
            usage);

    public void PrepareAndSet(ref RenderTarget2D rt, GraphicsDevice gd, int? width = null, int? height = null,
        RenderTargetUsage usage = RenderTargetUsage.PlatformContents, Color? clearColor = null)
    {
        PrepareATarget(ref rt, gd, width, height, usage);
        gd.SetRenderTarget(rt);
        gd.Clear(clearColor ?? Color.Transparent);
    }

    protected sealed override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
    {
        var old = device.GetRenderTargets();

        foreach (RenderTargetBinding oldTarget in old)
            if (oldTarget.RenderTarget is RenderTarget2D rt)
                rt.RenderTargetUsage = RenderTargetUsage.PreserveContents;

        HandleUseRequest(device, spriteBatch); // Advancement. Evolution. Progress.

        device.SetRenderTargets(old);
        _wasPrepared = true;
    }

    public abstract void HandleUseRequest(GraphicsDevice device, SpriteBatch spriteBatch);
    void ILoadable.Load(Mod mod) => Main.ContentThatNeedsRenderTargets.Add(this);
    void ILoadable.Unload() => Main.ContentThatNeedsRenderTargets.Remove(this);

    void INeedRenderTargetContent.Reset()
    {
        base.Reset();
        _target2 = null;
        OnReset();
    }

    public virtual void OnReset()
    {
    }
}
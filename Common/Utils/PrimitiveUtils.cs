using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using TheOracle.GeneratedAssets.DataStructures;

namespace TheOracle.Common.Utils;

public static class PrimitiveUtils
{
    public static VertexPositionColorTexture AsVertex(Vector2 position, Color color, Vector2 texCoord) =>
        new(new Vector3(position, 50), color, texCoord);

    public static VertexPositionColorTexture AsVertex(Vector2 position, Vector2 texCoord, Color color) =>
        new(new Vector3(position, 50), color, texCoord);

    public static VertexPositionColorTexture AsVertex(Vector3 position, Color color, Vector2 texCoord) =>
        new(position, color, texCoord);

    public static VertexPositionColorTexture AsVertex(Vector3 position, Vector2 texCoord, Color color) =>
        new(position, color, texCoord);

    private static Matrix view;
    private static Matrix projection;
    private static Matrix view2;
    private static Matrix projection2;

    public static Matrix GetMatrix()
    {
        GraphicsDevice device = Main.graphics.GraphicsDevice;
        int width = device.Viewport.Width;
        int height = device.Viewport.Height;
        Vector2 zoom = Main.GameViewMatrix.Zoom;
        view =
            Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up)
            * Matrix.CreateTranslation(width / 2, height / -2, 0)
            * Matrix.CreateRotationZ(MathHelper.Pi)
            * Matrix.CreateScale(zoom.X, zoom.Y, 1f);
        projection = Matrix.CreateOrthographic(width, height * Main.LocalPlayer.gravDir, 0, 1000);

        return view * projection;
    }

    public static Matrix GetMatrixUnscaled()
    {
        GraphicsDevice device = Main.graphics.GraphicsDevice;
        int width = device.Viewport.Width;
        int height = device.Viewport.Height;
        view2 =
            Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up)
            * Matrix.CreateTranslation(width / 2, height / -2, 0)
            * Matrix.CreateRotationZ(MathHelper.Pi)
            * Matrix.CreateScale(1f, 1f, 1f);
        projection2 = Matrix.CreateOrthographic(width, height * Main.LocalPlayer.gravDir, 0, 1000);

        return view2 * projection2;
    }

    private static int GetPrimitiveCount(int vertexCount, PrimitiveType type)
    {
        switch (type)
        {
            case PrimitiveType.LineList:
                return vertexCount / 2;
            case PrimitiveType.LineStrip:
                return vertexCount - 1;
            case PrimitiveType.TriangleList:
                return vertexCount / 3;
            case PrimitiveType.TriangleStrip:
                return vertexCount - 2;
            default: return 0;
        }
    }

    public static void DrawPrimitives(VertexPositionColorTexture[] vertices, PrimitiveType type,
        bool drawBacksides = true)
    {
        if (!Effects.TrailShader.IsReady) return;
        if (vertices.Length < 6) return;
        GraphicsDevice device = Main.graphics.GraphicsDevice;
        Effect effect = Effects.TrailShader.Value;
        effect.Parameters["WorldViewProjection"].SetValue(GetMatrix());
        effect.CurrentTechnique.Passes["Default"].Apply();
        if (drawBacksides)
        {
            short[] indices = new short[vertices.Length * 2];
            for (int i = 0; i < vertices.Length; i += 3)
            {
                indices[i * 2] = (short)i;
                indices[i * 2 + 1] = (short)(i + 1);
                indices[i * 2 + 2] = (short)(i + 2);

                indices[i * 2 + 5] = (short)i;
                indices[i * 2 + 4] = (short)(i + 1);
                indices[i * 2 + 3] = (short)(i + 2);
            }

            device.DrawUserIndexedPrimitives(type, vertices, 0, vertices.Length, indices, 0,
                GetPrimitiveCount(vertices.Length, type) * 2);
        }
        else
        {
            device.DrawUserPrimitives(type, vertices, 0, GetPrimitiveCount(vertices.Length, type));
        }
    }

    public static void DrawTexturedPrimitives(VertexPositionColorTexture[] vertices, PrimitiveType type,
        Asset<Texture2D> texture, bool drawBacksides = false, bool actualColor = false, bool unscaled = false) =>
        DrawTexturedPrimitives(vertices, type, texture.Value, drawBacksides, actualColor, unscaled);

    public static void DrawTexturedPrimitives(VertexPositionColorTexture[] vertices, PrimitiveType type,
        LazyAsset<Texture2D> texture, bool drawBacksides = false, bool actualColor = false, bool unscaled = false) =>
        DrawTexturedPrimitives(vertices, type, texture.Value, drawBacksides, actualColor, unscaled);

    public static void DrawTexturedPrimitives(VertexPositionColorTexture[] vertices, PrimitiveType type,
        Texture2D texture, bool drawBacksides = false, bool actualColor = false, bool unscaled = false,
        bool dontMult = false)
    {
        if (!Effects.TrailShader.IsReady) return;
        GraphicsDevice device = Main.graphics.GraphicsDevice;
        Effect effect = Effects.TrailShader.Value;
        effect.Parameters["WorldViewProjection"].SetValue(unscaled ? GetMatrixUnscaled() : GetMatrix());
        effect.Parameters["tex"].SetValue(texture);
        effect.Parameters["useActualCol"].SetValue(actualColor);
        effect.Parameters["dontMult"].SetValue(dontMult);
        effect.CurrentTechnique.Passes["Texture"].Apply();
        if (drawBacksides)
        {
            short[] indices = new short[vertices.Length * 2];
            for (int i = 0; i < vertices.Length; i += 3)
            {
                indices[i * 2] = (short)i;
                indices[i * 2 + 1] = (short)(i + 1);
                indices[i * 2 + 2] = (short)(i + 2);

                indices[i * 2 + 5] = (short)i;
                indices[i * 2 + 4] = (short)(i + 1);
                indices[i * 2 + 3] = (short)(i + 2);
            }

            device.DrawUserIndexedPrimitives(type, vertices, 0, vertices.Length, indices, 0,
                GetPrimitiveCount(vertices.Length, type) * 2);
        }
        else
        {
            device.DrawUserPrimitives(type, vertices, 0, GetPrimitiveCount(vertices.Length, type));
        }
    }

    public static Vector2 GetRotation(List<Vector2> oldPos, int index)
    {
        if (oldPos.Count == 1)
            return oldPos[0];

        if (index == 0) return Vector2.Normalize(oldPos[1] - oldPos[0]).RotatedBy(MathHelper.Pi / 2);

        return (index == oldPos.Count - 1
            ? Vector2.Normalize(oldPos[index] - oldPos[index - 1])
            : Vector2.Normalize(oldPos[index + 1] - oldPos[index - 1])).RotatedBy(MathHelper.Pi / 2);
    }
}
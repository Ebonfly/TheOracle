namespace TheOracle.Common.Utils;

public static class MiscUtils
{
    public static Vector2 MultiLerp(this Vector2 a, float t, params Vector2[] b)
    {
        float[] X = new float[b.Length];
        float[] Y = new float[b.Length];
        for (int i = 0; i < b.Length; i++)
        {
            X[i] = b[i].X;
            Y[i] = b[i].Y;
        }

        return new Vector2(Terraria.Utils.MultiLerp(t, X), Terraria.Utils.MultiLerp(t, Y));
    }
}
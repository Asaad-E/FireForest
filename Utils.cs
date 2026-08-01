
using Raylib_cs;

namespace FireForest;


public static class Utils
{
    private static readonly Random rand = new();
    private static readonly FastNoiseLite noise = new();

    static Utils()
    {
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(SimParams.NoiseFrecuency);

        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(SimParams.NoiseOctaves);

        SetRandomNoiseSeed();
    }

    public static void SetRandomNoiseSeed()
    {
        noise.SetSeed(NextInt());
    }

    public static float NextFloat()
    {
        return Random.Shared.NextSingle();
    }

    public static int NextInt()
    {
        return Random.Shared.Next();
    }

    public static float GetNoise(float x, float y)
    {
        return noise.GetNoise(x, y);
    }

    public static Color LerpColor(Color start, Color end, float value)
    {
        int deltaR = end.R - start.R;
        int deltaG = end.G - start.G;
        int deltaB = end.B - start.B;

        return new Color(
            start.R + (int)(deltaR * value),
            start.G + (int)(deltaG * value),
            start.B + (int)(deltaB * value)
        );

    }

}
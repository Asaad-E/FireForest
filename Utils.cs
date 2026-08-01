
using Raylib_cs;

namespace FireForest;


public static class Utils
{
    private static readonly Random Rand = new();
    private static readonly FastNoiseLite ElevationNoise = new();
    private static readonly FastNoiseLite FuelNoise = new();

    static Utils()
    {
        // Initialize the noise generator
        ElevationNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        ElevationNoise.SetFractalType(FastNoiseLite.FractalType.FBm);

        FuelNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        FuelNoise.SetFractalType(FastNoiseLite.FractalType.FBm);

        FuelNoise.SetFrequency(0.004f);
        FuelNoise.SetFractalOctaves(3);

        SetupNoiseParams();
        SetRandomNoiseSeed();
    }

    public static void SetupNoiseParams()
    {
        ElevationNoise.SetFrequency(SimParams.NoiseFrecuency);
        ElevationNoise.SetFractalOctaves(SimParams.NoiseOctaves);
    }

    public static void SetRandomNoiseSeed()
    {
        ElevationNoise.SetSeed(NextInt());
        FuelNoise.SetSeed(NextInt());
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
        return ElevationNoise.GetNoise(x, y);
    }

    public static float GetFuelNoise(float x, float y)
    {
        return FuelNoise.GetNoise(x, y) + 1f;
    }


    public static float NextGaussian()
    {
        // Box-Muller transform for generate a quick Noraml variable
        float u1 = 1.0f - Random.Shared.NextSingle();
        float u2 = 1.0f - Random.Shared.NextSingle();
        float randNormal = MathF.Sqrt(-2.0f * MathF.Log(u1)) * MathF.Sin(-2.0f * MathF.PI * u2);

        return randNormal;
    }

    public static float EasingFunctionFuel(float fuelCapacity)
    {
        if (fuelCapacity < 1)
        {
            return MathF.Pow(fuelCapacity, 3.2f);
        }
        else if(fuelCapacity < 1.5f)
        {
            return MathF.Pow(fuelCapacity, 0.7f);
        }
        else 
        {
            return MathF.Pow(fuelCapacity, 1.5f);
        }
    }

}
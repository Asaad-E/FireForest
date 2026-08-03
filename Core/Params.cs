using Raylib_cs;

namespace FireForest.Core;

public readonly record struct SnapshotParams(float SpontaneousFireProb, float TreeProb, float SoilProb, float FireProb, int FireDuration);

#pragma warning disable CA2211
public static class CAParams
{
    public static int GridSizeX = 100;
    public static int GridSizeY = 100;
    public static int Totalcells = 10000;
    public static int FireDuration = 40;
    public static float FireProb = 0.25f;

    public static float TreeProbBase = 0.00003f;
    public static int TreeProbMult = 3;
    public static float TreeProb = TreeProbBase * TreeProbMult;
    public static float TreeProbconst = 1.6f;


    public static float SoilProbBase = 0.00001f;
    public static int SoilProbMult = 7;
    public static float SoilProb = SoilProbBase * SoilProbMult;
    public static float SoilProbconst = 1.2f;


    public static float SpontaneousFireProbBase = 0.0000000001f;
    public static int SpontaneousFireProbMult = 5;
    public static float SpontaneousFireProb = SpontaneousFireProbBase * SpontaneousFireProbMult;

    public static int Shitf = 4;

    public static SnapshotParams GetSnapshotParams()
    {
        return new SnapshotParams(SpontaneousFireProb, TreeProb, SoilProb, FireProb, FireDuration);
    }
}

public static class SimParams
{

    public static int LauncherScreenWidth = 500;
    public static int LauncherScreenHeight = 500;


    public static int ScreenWidth = 1280;
    public static int ScreenHeight = 720;
    public static int SimulationHeight = 900;

    public static int SimulationWidth = SimulationHeight * ScreenWidth / ScreenHeight;


    public static float RatioH => SimulationHeight / (float)ScreenHeight;
    public static float RatioW => SimulationWidth / (float)ScreenWidth;



    public static int MaxSimulationWidth = 5500;
    public static int MaxSimulationHeight = 5500;


    public static float StartingZoom = 1.5f;


    public static float NoiseFrecuency = 0.003f;
    public static int NoiseOctaves = 5;
    public const int FPS = 30;
    public static int SimulationSpeed = 1;
    public static float WaterLevel = 0.4f;
    public static float RockLevel = 0.7f;
}
#pragma warning restore CA2211
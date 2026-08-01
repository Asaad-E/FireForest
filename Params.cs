using Raylib_cs;

namespace FireForest;


#pragma warning disable CA2211
public static class CAParams
{
    public const int CellSize = 1;
    public const int GridSizeX = 1500 / CellSize;
    public const int GridSizeY = 1000 / CellSize;
    public const int Totalcells = GridSizeX * GridSizeY;
    public static int FireDuration = 40;
    public static float FireProb = 0.3f;
    public static float TreeProbBase = 0.00005f;
    public static int TreeProbMult = 6;
    public static float TreeProb = TreeProbBase * TreeProbMult;
    public static float SpontaneousFireProbBase = 0.000000001f;
    public static int SpontaneousFireProbMult = 5;
    public static float SpontaneousFireProb = SpontaneousFireProbBase * SpontaneousFireProbMult;
}

public static class SimParams
{
    public const int ScreenWidth = CAParams.GridSizeX * CAParams.CellSize;
    public const int ScreenHeight = CAParams.GridSizeY * CAParams.CellSize;

    public const float NoiseFrecuency = 0.003f;
    public const int NoiseOctaves = 2;
    public const int FPS = 60;
    public static int FrameStep = 1;
    public static float WaterLevel = 0.4f;
    public static float RockLevel = 0.7f;
}
#pragma warning restore CA2211
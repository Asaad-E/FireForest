using Raylib_cs;

namespace FireForrest;

public static class Params
{

    public const int CellSize = 1;
    public const int GridSizeX = 1500 / CellSize;
    public const int GridSizeY = 1000 / CellSize;

    public const int ScreenSizeX = GridSizeX * CellSize;
    public const int ScreenSizeY = GridSizeY * CellSize;
    public const float noiseFrecuency = 0.003f;
    public static int noiseOctaves = 2;



    public const int FPS = 60;
    public static int frameStep = 1;

    public static float waterLevel = 0.41f;
    public static float rockLevel = 0.7f;


    public const int waterCode = 0;
    public const int calcinatedCode = 1;
    public const int treeCode = 2;
    public const int fireCode = 3;
    public const int rockCode = 4;

    // Colors

    public static readonly Color[] colorTypesStart = [new Color(0, 20, 71), new Color(140, 140, 140), new Color(19, 110, 44), new Color(240, 5, 41), new Color(40, 40, 40)];
    public static readonly Color[] colorTypesEnd = [new Color(1, 73, 255), new Color(100, 100, 100), new Color(17, 85, 40), new Color(115, 2, 19), new Color(0, 0, 0)];

    public static int fireDuration = 40;

    public static float fireProb = 0.3f;
    public static float treeProbBase = 0.00005f;
    public static int treeProbMult = 6;
    public static float treeProb = treeProbBase * treeProbMult;


    public static float spontaneousFireProbBase = 0.000000001f;
    public static int spontaneousFireMult = 5;
    public static float spontaneousFireProb = spontaneousFireProbBase * spontaneousFireMult;



    public static float UIPadding = 20f;


}
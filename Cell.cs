using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Raylib_cs;

namespace FireForest;

public struct Cell
{
     public enum Types : byte
    {
        Water,
        Soil,
        Calcined,
        Tree,
        Fire,
        Rock
    }

    static readonly Color[] StartColors =
        [
        new Color(0, 20, 71),
        new Color(166, 95, 27),
        new Color(140, 140, 140),
        new Color(19, 110, 44),
        new Color(240, 5, 41),
        new Color(40, 40, 40)
        ];

    static readonly Color[] FinalColors =
        [
        new Color(1, 73, 255),
        new Color(208, 135, 46),
        new Color(100, 100, 100),
        new Color(17, 85, 40),
        new Color(115, 2, 19),
        new Color(0, 0, 0)
        ];
    public Types Type = Types.Tree;
    public ushort Count = 0;
    public float ElevationValue = 0;

    public Cell()
    {
        Type = Types.Tree;
    }
    public Color GetColor()
    {
        float value = 1;

        switch (Type)
        {
            case Types.Fire:
                value = (float)Count / (float)CAParams.FireDuration;
                value = MathF.Pow(value, 2f);

                break;
            case Types.Tree:
                value = (ElevationValue - SimParams.WaterLevel) / (SimParams.RockLevel - SimParams.WaterLevel);
                value = MathF.Pow(value, 1.5f);
                break;
            case Types.Water:
                value = ElevationValue / SimParams.WaterLevel;
                value = 1 - MathF.Pow(1 - value, 1.5f);
                break;
            case Types.Rock:
                value = (ElevationValue - SimParams.RockLevel) / (1 - SimParams.WaterLevel);
                value = 1 - MathF.Pow(1 - value, 2);
                break;
            case Types.Calcined:
                value = (ElevationValue - SimParams.WaterLevel) / (SimParams.RockLevel - SimParams.WaterLevel);
                value = MathF.Pow(value, 2);
                break;
        }

        return Raylib.ColorLerp(StartColors[(int)Type], FinalColors[(int)Type], value);
    }

    public void SetOnFire(int fireDuration)
    {
        if (Type != Types.Tree) return;

        Type = Types.Fire;
        Count = (ushort)fireDuration;
    }
}
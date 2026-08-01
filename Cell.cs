using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Raylib_cs;

namespace FireForrest;

public struct Cell
{
     
    public int type = 0;
    public int count = 0;
    public float elevationValue;

    private static readonly (int, int)[] offsets = [(-1, 0), (1, 0), (0, -1), (0, 1)];

    public Cell(int x, int y)
    {
        type = Params.treeCode;
    }
    public Color GetColor()
    {
        float value = 1;

        switch (type)
        {
            case Params.fireCode:
                value = (float)count / (float)Params.fireDuration;
                value = MathF.Pow(value, 2f);

                break;
            case Params.treeCode:
                value = (elevationValue - Params.waterLevel) / (Params.rockLevel - Params.waterLevel);
                value = MathF.Pow(value, 1.5f);
                break;
            case Params.waterCode:
                value = elevationValue / Params.waterLevel;
                value = 1 - MathF.Pow(1 - value, 1.5f);
                break;
            case Params.rockCode:
                value = (elevationValue - Params.rockLevel) / (1 - Params.waterLevel);
                value = 1 - MathF.Pow(1 - value, 2);
                break;
            case Params.calcinatedCode:
                value = (elevationValue - Params.waterLevel) / (Params.rockLevel - Params.waterLevel);
                value = MathF.Pow(value, 2);
                break;
        }

        return Raylib.ColorLerp(Params.colorTypesStart[type], Params.colorTypesEnd[type], value);
    }

    public void SetOnFire()
    {
        if (type != Params.treeCode) return;

        type = Params.fireCode;
        count = Params.fireDuration;
    }

    public Cell Update(int x, int y, Cell[] grid)
    {
        Cell currentCell = this;

        // Skip rock adn water cell
        if (currentCell.type == Params.rockCode || currentCell.type == Params.waterCode)
        {
            return currentCell;
        }


        int neighnorsTree = 1;
        // Calculate influence of other cells
        for (int i = 0; i < 4; i++)
        {
            (int dx, int dy) = offsets[i];
            int newX = (x + dx + Params.GridSizeX) % Params.GridSizeX;
            int newY = (y + dy + Params.GridSizeY) % Params.GridSizeY;


            int neighnorType = grid[newX + newY * Params.GridSizeX].type;


            if (neighnorType == Params.treeCode)
            {
                neighnorsTree += 1;
            }
            else if (currentCell.type == Params.treeCode && neighnorType == Params.fireCode && Utils.NextFloat() <= Params.fireProb)
            {
                currentCell.SetOnFire();
                break;
            }
        }

        // Fire duration
        if (currentCell.type == Params.fireCode)
        {
            if (--currentCell.count < 0)
            {
                currentCell.type = Params.calcinatedCode;
            }
            return currentCell;
        }

        // espotaneus tree
        if (currentCell.type == Params.calcinatedCode)
        {
            if (Utils.NextFloat() <= Params.treeProb * MathF.Pow(neighnorsTree, 4.2f))
            {
                currentCell.type = Params.treeCode;
            }
        }

        // Logic of trees
        if (currentCell.type == Params.treeCode)
        {
            // Fast siple check
            if ((Utils.NextInt() & 1023) == 0)
            {
                // Slow acurate check
                if (Utils.NextFloat() <= Params.spontaneousFireProb * 1024f)
                {
                    currentCell.SetOnFire();
                }
            }
        }



        return currentCell;

    }
}
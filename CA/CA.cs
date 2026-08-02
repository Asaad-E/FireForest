using System;
using System.Numerics;
using Raylib_cs;

using FireForest.Core;
using System.Collections.Concurrent;

namespace FireForest.CA;

public class CAEnv
{
    private int Totalcells;
    private int GridSizeX;
    private int GridSizeY;

    private Cell[] Grid = [];
    private Cell[] nextGrid = [];
    private static readonly (int, int)[] offsets = [(-1, 0), (1, 0), (0, -1), (0, 1)];

    // Variables for Fast draw
    private Color[] PixelBuffer = [];
    private Image Image;
    private Texture2D GridTexture;
    public void Setup(int gridSizeX, int gridSizeY)
    {
        Totalcells = gridSizeX * gridSizeY;

        GridSizeX = gridSizeX;
        GridSizeY = gridSizeY;

        // Re seize the world arrays
        Grid = new Cell[Totalcells];
        nextGrid = new Cell[Totalcells];
        PixelBuffer = new Color[Totalcells];

        // init
        ReGenerate();
    }

    public void LoadTextures()
    {
        // Init
        Image = Raylib.GenImageColor(GridSizeX, GridSizeY, Color.Black);
        GridTexture = Raylib.LoadTextureFromImage(Image);
        Raylib.SetTextureFilter(GridTexture, TextureFilter.Point);
    }

    public void ReGenerate()
    {
        Utils.SetRandomNoiseSeed();
        Restart();

    }
    public void Restart()
    {
        Parallel.For(0, GridSizeY, j =>
        {
            for (int i = 0; i < GridSizeX; i++)
            {
                Cell newCell = new();
                int faltCoord = i + j * GridSizeX;

                float noise = Utils.GetNoise(i, j);
                float fuelCapacity = Utils.GetFuelNoise(i, j);

                // Cell initializatino
                newCell.Type = GetTypeFromLevel(noise);
                newCell.ElevationValue = noise;
                newCell.FuelCapacity = fuelCapacity;
                Grid[faltCoord] = newCell;
                nextGrid[faltCoord] = newCell;
                PixelBuffer[faltCoord] = newCell.GetColor();

            }
        });
    }

    public static Cell.Types GetTypeFromLevel(float Level)
    {
        // Terrain segmentation
        if (Level < SimParams.WaterLevel)
        {
            return Cell.Types.Water;
        }
        else if (Level > SimParams.RockLevel)
        {
            return Cell.Types.Rock;
        }
        else
        {
            return Cell.Types.Tree;
        }

    }

    public void ChangeTerrain()
    {
        int gridSizeY = GridSizeY;
        int gridSizeX = GridSizeX;

        Parallel.For(0, gridSizeY, j =>
        {
            for (int i = 0; i < gridSizeX; i++)
            {
                int flatCoord = i + j * gridSizeX;

                float elevation = Utils.GetNoise(i, j);

                // Calculate new terrain
                Cell.Types newType = GetTypeFromLevel(elevation);

                // Only update when the new type it is rock/water, or when a tree reaplce a roock/water;
                if (newType != Cell.Types.Tree)
                {
                    Grid[flatCoord].Type = newType;
                }
                else if (Grid[flatCoord].Type == Cell.Types.Rock || Grid[flatCoord].Type == Cell.Types.Water)
                {
                    Grid[flatCoord].Type = newType;
                }

                // Always change the elevation value and recalculate the pixel buffer
                Grid[flatCoord].ElevationValue = elevation;
                PixelBuffer[flatCoord] = Grid[flatCoord].GetColor();
            }
        });
    }

    public void Update(SnapshotParams caparams)
    {
        SnapshotParams currentParams = caparams;

        int gridSizeY = GridSizeY;
        int gridSizeX = GridSizeX;

        var rangePartitioner = Partitioner.Create(0, gridSizeY);

        Parallel.ForEach(rangePartitioner, range =>
        {
            for (int j = range.Item1; j < range.Item2; j++)
            {
                int offset = j * GridSizeX;
                for (int i = 0; i < GridSizeX; i++)
                {
                    Cell updatedCell = UpdateCell(i, j, in currentParams);
                    nextGrid[i + offset] = updatedCell;

                    // Only update the color when a changed of type occurs or it's fire
                    if (updatedCell.Type != Grid[i + offset].Type || updatedCell.Type == Cell.Types.Fire)
                    {
                        PixelBuffer[i + offset] = updatedCell.GetColor();
                    }

                }
            }
        });

        (Grid, nextGrid) = (nextGrid, Grid);
    }

    public Cell UpdateCell(int x, int y, in SnapshotParams caparams)
    {
        Cell currentCell = Grid[x + y * GridSizeX];

        // Skip rock adn water cell
        if (currentCell.Type == Cell.Types.Rock || currentCell.Type == Cell.Types.Water)
        {
            return currentCell;
        }

        // Fire Type
        if (currentCell.Type == Cell.Types.Fire)
        {
            if (--currentCell.Count <= 0)
            {
                currentCell.Type = Cell.Types.Calcined;
            }
            return currentCell;
        }


        int neighnorsTree = 1;
        int neighnorsSoil = 1;

        // Calculate influence of other cells
        for (int i = 0; i < 4; i++)
        {
            (int dx, int dy) = offsets[i];
            int newX = (x + dx + GridSizeX) % GridSizeX;
            int newY = (y + dy + GridSizeY) % GridSizeY;


            Cell.Types neighnorType = Grid[newX + newY * GridSizeX].Type;



            if (currentCell.Type == Cell.Types.Tree && neighnorType == Cell.Types.Fire && Utils.NextFloat() <= caparams.FireProb * Utils.EasingFunctionFuel(currentCell.FuelCapacity))
            {
                currentCell.SetOnFire(caparams.FireDuration);
                break;
            }
            else
            {
                if (neighnorType == Cell.Types.Tree)
                {
                    neighnorsTree += 1;
                }
                else if (neighnorType == Cell.Types.Tree)
                {
                    neighnorsSoil += 1;
                }
            }
        }

        // Calcined Type
        if (currentCell.Type == Cell.Types.Calcined)
        {
            if (Utils.NextFloat() <= caparams.SoilProb * currentCell.FuelCapacity * MathF.Exp(CAParams.SoilProbconst * (neighnorsTree + neighnorsSoil)))
            {
                currentCell.Type = Cell.Types.Soil;
            }
        }

        // Soil Type
        else if (currentCell.Type == Cell.Types.Soil)
        {
            if (Utils.NextFloat() <= caparams.TreeProb * currentCell.FuelCapacity * MathF.Exp(CAParams.TreeProbconst * neighnorsTree))
            {
                currentCell.Type = Cell.Types.Tree;
            }
        }

        // Tree Type
        else if (currentCell.Type == Cell.Types.Tree)
        {
            // Fast siple check
            if ((Utils.NextInt() & 1023) == 0)
            {
                // Slow acurate check
                if (Utils.NextFloat() <= caparams.SpontaneousFireProb * 1024f)
                {
                    currentCell.SetOnFire(caparams.FireDuration);
                }
            }
        }

        return currentCell;
    }

    public void Draw(int width, int height)
    {
        Raylib.UpdateTexture(GridTexture, PixelBuffer);

        Raylib.DrawTexturePro(
        GridTexture,
        new Rectangle(0, 0, GridSizeX, GridSizeY),
        new Rectangle(0, 0, width, height),
        Vector2.Zero,
        0.0f,
        Color.White
        );

    }

    public void Close()
    {
        Raylib.UnloadTexture(GridTexture);
        Raylib.UnloadImage(Image);
    }

    public void SetCellOnFire(Vector2 globalPos, int fireDuration)
    {
        int gridX = (int)globalPos.X;
        int gridY = (int)globalPos.Y;

        // return ;
        Grid[gridX + gridY * GridSizeX].SetOnFire(fireDuration);
    }

}
using System;
using System.Numerics;
using Raylib_cs;

using FireForest.Core;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace FireForest.CA;

public class CAEnv
{
    private int TotalCells;
    private int GridSizeX;
    private int GridSizeY;

    private int ChunkSize;
    private int ChunkSizeX;
    private int ChunkSizeY;

    private Cell[] Grid = [];
    private Cell[] nextGrid = [];

    private bool[] ChunkActive = [];
    private bool[] NextChunkActive = [];

    private static readonly (int, int)[] offsets = [(-1, 0), (1, 0), (0, -1), (0, 1)];

    // Variables for Fast draw
    private Color[] PixelBuffer = [];
    private Image Image;
    private Texture2D GridTexture;
    public void Setup(int gridSizeX, int gridSizeY)
    {
        // Set CA grid
        TotalCells = gridSizeX * gridSizeY;

        GridSizeX = gridSizeX;
        GridSizeY = gridSizeY;

        ChunkSizeX = (int)Math.Ceiling(GridSizeX / 16f);
        ChunkSizeY = (int)Math.Ceiling(GridSizeY / 16f);

        ChunkSize = ChunkSizeX * ChunkSizeY;

        // Reseize the world arrays
        Grid = new Cell[TotalCells];
        nextGrid = new Cell[TotalCells];

        ChunkActive = new bool[ChunkSize];
        NextChunkActive = new bool[ChunkSize];

        PixelBuffer = new Color[TotalCells];

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
        int gridSizeY = GridSizeY;
        int gridSizeX = GridSizeX;

        var rangePartitioner = Partitioner.Create(0, gridSizeY);

        Parallel.ForEach(rangePartitioner, range =>
        {
            for (int j = range.Item1; j < range.Item2; j++)
            {
                int offset = j * gridSizeX;
                for (int i = 0; i < gridSizeX; i++)
                {
                    Cell newCell = new();
                    int flatCoord = i + offset;

                    float elevation = Utils.GetNoise(i, j);
                    float fuelCapacity = Utils.GetFuelNoise(i, j);

                    // Cell initializatino
                    newCell.Type = GetTypeFromElevation(elevation);
                    newCell.ElevationValue = elevation;
                    newCell.FuelCapacity = fuelCapacity;
                    Grid[flatCoord] = newCell;
                    PixelBuffer[flatCoord] = newCell.GetColor();
                }
            }
        });
    }
    public static Cell.Types GetTypeFromElevation(float elevation)
    {
        // Terrain segmentation
        if (elevation < SimParams.WaterLevel)
        {
            return Cell.Types.Water;
        }
        else if (elevation > SimParams.RockLevel)
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

        var rangePartitioner = Partitioner.Create(0, gridSizeY);

        Parallel.ForEach(rangePartitioner, range =>
        {
            for (int j = range.Item1; j < range.Item2; j++)
            {
                int offset = j * gridSizeX;
                for (int i = 0; i < gridSizeX; i++)
                {
                    int flatCoord = i + offset;

                    float elevation = Utils.GetNoise(i, j);

                    // Calculate new terrain
                    Cell.Types newType = GetTypeFromElevation(elevation);

                    // Only update when the new type is a rock/water, or when a tree replace a rock/water;
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
            }
        });
    }
    public void Update(SnapshotParams caparams)
    {
        SnapshotParams currentParams = caparams;

        int gridSizeY = GridSizeY;
        int gridSizeX = GridSizeX;

        var rangePartitioner = Partitioner.Create(0, gridSizeY);

        // Set desactive all chunk at the start of the new frame
        Array.Clear(NextChunkActive);

        Parallel.ForEach(rangePartitioner, range =>
        {
            for (int j = range.Item1; j < range.Item2; j++)
            {
                int offset = j * GridSizeX;
                for (int i = 0; i < GridSizeX; i++)
                {
                    Cell updatedCell = UpdateCell(i, j, in currentParams);
                    nextGrid[i + offset] = updatedCell;

                    if (updatedCell.Type == Cell.Types.Fire)
                    {
                        NextChunkActive[GetChunkFlatCoord(i, j)] = true;
                    }

                    // Only update the color when a changed of type occurs or it's fire
                    if (updatedCell.Type != Grid[i + offset].Type || updatedCell.Type == Cell.Types.Fire)
                    {
                        PixelBuffer[i + offset] = updatedCell.GetColor();
                    }

                }
            }
        });

        // Swap the buffers
        (Grid, nextGrid) = (nextGrid, Grid);
        (ChunkActive, NextChunkActive) = (NextChunkActive, ChunkActive);

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

        if (currentCell.Type == Cell.Types.Tree)
        {
            // Fast siple check
            if ((Utils.NextInt() & 1023) == 0)
            {
                // Slow accurate check
                if (Utils.NextFloat() <= caparams.SpontaneousFireProb * 1024f)
                {
                    currentCell.SetOnFire(caparams.FireDuration);
                }
            }
        }

        if (currentCell.Type == Cell.Types.Tree && !IsNeighborhoodActive(x, y)) return currentCell;

        int neighborsTree = 1;
        int neighborsSoil = 1;

        // Calculate influence of other cells
        for (int i = 0; i < 4; i++)
        {
            (int dx, int dy) = offsets[i];
            int newX = x + dx;
            int newY = y + dy;

            if (newX < 0) newX += GridSizeX;
            if (newX >= GridSizeX) newX -= GridSizeX;

            if (newY < 0) newY += GridSizeY;
            if (newY >= GridSizeY) newY -= GridSizeY;

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
                    neighborsTree += 1;
                }
                else if (neighnorType == Cell.Types.Soil)
                {
                    neighborsSoil += 1;
                }
            }
        }

        // Calcined Type
        if (currentCell.Type == Cell.Types.Calcined)
        {
            if (Utils.NextFloat() <= caparams.SoilProb * currentCell.FuelCapacity * MathF.Exp(CAParams.SoilProbconst * (neighborsTree + neighborsSoil)))
            {
                currentCell.Type = Cell.Types.Soil;
            }
        }
        // Soil Type
        else if (currentCell.Type == Cell.Types.Soil)
        {
            if (Utils.NextFloat() <= caparams.TreeProb * currentCell.FuelCapacity * MathF.Exp(CAParams.TreeProbconst * neighborsTree))
            {
                currentCell.Type = Cell.Types.Tree;
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

        Grid[gridX + gridY * GridSizeX].SetOnFire(fireDuration);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public (int, int) GetChunkCoord(int x, int y)
    {
        return (x >> CAParams.Shitf, y >> CAParams.Shitf);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public int GetChunkFlatCoord(int x, int y)
    {
        (int cx, int cy) = GetChunkCoord(x, y);
        return cx + cy * ChunkSizeX;
    }

    public bool IsNeighborhoodActive(int x, int y)
    {
        (int cx, int cy) = GetChunkCoord(x, y);

        // Current chunk
        if (ChunkActive[cx + cy * ChunkSizeX]) return true;

        // Neighbors chunks
        for (int i = 0; i < 4; i++)
        {
            (int dx, int dy) = offsets[i];

            int newX = cx + dx;
            int newY = cy + dy;

            if (newX < 0) { newX += ChunkSizeX; }
            else if (newX >= ChunkSizeX) { newX -= ChunkSizeX; }

            if (newY < 0) { newY += ChunkSizeY; }
            else if (newY >= ChunkSizeY) { newY -= ChunkSizeY; }

            if (ChunkActive[newX + newY * ChunkSizeX]) return true;
        }

        return false;
    }
}
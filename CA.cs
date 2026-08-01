using System;
using System.Numerics;
using Raylib_cs;

namespace FireForest;

public class CA
{
    private Cell[] Grid = new Cell[CAParams.Totalcells];
    private Cell[] nextGrid = new Cell[CAParams.Totalcells];
    private static readonly (int, int)[] offsets = [(-1, 0), (1, 0), (0, -1), (0, 1)];

    // Variables for Fast draw
    private Color[] PixelBuffer = new Color[CAParams.Totalcells];
    private Image Image;
    private Texture2D GridTexture;

    public CA()
    {
        Restart();

        // Init
        Image = Raylib.GenImageColor(CAParams.GridSizeX, CAParams.GridSizeY, Color.Black);
        GridTexture = Raylib.LoadTextureFromImage(Image);
        Raylib.SetTextureFilter(GridTexture, TextureFilter.Point);
    }


    public void Restart()
    {
        Utils.SetRandomNoiseSeed();

        for (int i = 0; i < CAParams.GridSizeX; i++)
        {
            for (int j = 0; j < CAParams.GridSizeY; j++)
            {
                Cell newCell = new();
                int faltCoord = i + j * CAParams.GridSizeX;

                float noise = (Utils.GetNoise(i * CAParams.CellSize, j * CAParams.CellSize) + 1f) / 2f;


                // Cell initializatino
                newCell.Type = GetTypeFromLevel(noise);
                newCell.ElevationValue = noise;
                Grid[faltCoord] = newCell;
                nextGrid[faltCoord] = newCell;
                PixelBuffer[faltCoord] = newCell.GetColor();

            }
        }
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

    public void TerrainLevelChanged()
    {
        for (int i = 0; i < CAParams.GridSizeX; i++)
        {
            for (int j = 0; j < CAParams.GridSizeY; j++)
            {
                int flatCoord = i + j * CAParams.GridSizeX;

                float noise = (Utils.GetNoise(i * CAParams.CellSize, j * CAParams.CellSize) + 1f) / 2f;

                // Terrain segmentation
                Cell.Types newType = GetTypeFromLevel(noise);

                if (newType != Cell.Types.Tree)
                {
                    Grid[flatCoord].Type = newType;
                    nextGrid[flatCoord].Type = newType;
                }
                else if (nextGrid[flatCoord].Type == Cell.Types.Rock || nextGrid[flatCoord].Type == Cell.Types.Water)
                {
                    Grid[flatCoord].Type = newType;
                    nextGrid[flatCoord].Type = newType;
                }



            }
        }
    }

    public void Update()
    {
        Parallel.For(0, CAParams.GridSizeY, j =>
        {
            int offset = j * CAParams.GridSizeX;
            for (int i = 0; i < CAParams.GridSizeX; i++)
            {
                Cell updatedCell = UpdateCell(i, j);
                nextGrid[i + offset] = updatedCell;
                PixelBuffer[i + offset] = updatedCell.GetColor();
            }
        });

        (Grid, nextGrid) = (nextGrid, Grid);
    }

    public Cell UpdateCell(int x, int y)
    {
        Cell currentCell = Grid[x + y * CAParams.GridSizeX];

        // Skip rock adn water cell
        if (currentCell.Type == Cell.Types.Rock || currentCell.Type == Cell.Types.Water)
        {
            return currentCell;
        }


        int neighnorsTree = 1;
        // Calculate influence of other cells
        for (int i = 0; i < 4; i++)
        {
            (int dx, int dy) = offsets[i];
            int newX = (x + dx + CAParams.GridSizeX) % CAParams.GridSizeX;
            int newY = (y + dy + CAParams.GridSizeY) % CAParams.GridSizeY;


            Cell.Types neighnorType = Grid[newX + newY * CAParams.GridSizeX].Type;


            if (neighnorType == Cell.Types.Tree)
            {
                neighnorsTree += 1;
            }
            else if (currentCell.Type == Cell.Types.Tree && neighnorType == Cell.Types.Fire && Utils.NextFloat() <= CAParams.FireProb)
            {
                currentCell.SetOnFire();
                break;
            }
        }

        // Fire duration
        if (currentCell.Type == Cell.Types.Fire)
        {
            if (--currentCell.Count <= 0)
            {
                currentCell.Type = Cell.Types.Calcined;
            }
            return currentCell;
        }

        // espotaneus tree
        if (currentCell.Type == Cell.Types.Calcined)
        {
            if (Utils.NextFloat() <= CAParams.TreeProb * MathF.Pow(neighnorsTree, 4.2f))
            {
                currentCell.Type = Cell.Types.Tree;
            }
        }

        // Logic of trees
        if (currentCell.Type == Cell.Types.Tree)
        {
            // Fast siple check
            if ((Utils.NextInt() & 1023) == 0)
            {
                // Slow acurate check
                if (Utils.NextFloat() <= CAParams.SpontaneousFireProb * 1024f)
                {
                    currentCell.SetOnFire();
                }
            }
        }



        return currentCell;

    }

    public void Draw()
    {
        Raylib.UpdateTexture(GridTexture, PixelBuffer);

        Raylib.DrawTexturePro(
        GridTexture,
        new Rectangle(0, 0, CAParams.GridSizeX, CAParams.GridSizeY),
        new Rectangle(0, 0, SimParams.ScreenWidth, SimParams.ScreenHeight),
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

    public void SetCellOnFire(Vector2 globalPos)
    {
        int gridX = (int)globalPos.X / CAParams.CellSize;
        int gridY = (int)globalPos.Y / CAParams.CellSize;
        Grid[gridX + gridY * CAParams.GridSizeX].SetOnFire();
    }
}
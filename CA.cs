using System;
using System.Numerics;
using Raylib_cs;

namespace FireForrest;

public class CA
{
    private Cell[] Grid = new Cell[Params.GridSizeX * Params.GridSizeY];
    private Cell[] nextGrid = new Cell[Params.GridSizeX * Params.GridSizeY];


    // Variables for Fast draw
    private Color[] pixelBuffer = new Color[Params.GridSizeX * Params.GridSizeY];
    private Image image;
    private Texture2D gridTexture;

    public CA()
    {
        Reset();

        // Init
        image = Raylib.GenImageColor(Params.GridSizeX, Params.GridSizeY, Color.Black);
        gridTexture = Raylib.LoadTextureFromImage(image);
        Raylib.SetTextureFilter(gridTexture, TextureFilter.Point);
    }


    public void Reset()
    {
        Utils.SetRandomNoiseSeed();

        for (int i = 0; i < Params.GridSizeX; i++)
        {
            for (int j = 0; j < Params.GridSizeY; j++)
            {
                Cell newCell = new(i, j);
                int faltCoord = i + j * Params.GridSizeX;

                float noise = (Utils.GetNoise(i * Params.CellSize, j * Params.CellSize) + 1f) / 2f;


                // Cell initializatino
                newCell.type = GetTypeFromLevel(noise);
                newCell.elevationValue = noise;
                Grid[faltCoord] = newCell;
                nextGrid[faltCoord] = newCell;
                pixelBuffer[faltCoord] = newCell.GetColor(); 

            }
        }
    }

    public static int GetTypeFromLevel(float Level)
    {
        // Terrain segmentation
        if (Level < Params.waterLevel)
        {
            return Params.waterCode;
        }
        else if (Level > Params.rockLevel)
        {
            return Params.rockCode;
        }
        else
        {
            return Params.treeCode;
        }

    }

    public void TerrainLevelChanged()
    {
        for (int i = 0; i < Params.GridSizeX; i++)
        {
            for (int j = 0; j < Params.GridSizeY; j++)
            {
                int flatCoord = i + j * Params.GridSizeX;

                float noise = (Utils.GetNoise(i * Params.CellSize, j * Params.CellSize) + 1f) / 2f;

                // Terrain segmentation
                int newType = GetTypeFromLevel(noise);

                if (newType != Params.treeCode)
                {
                    Grid[flatCoord].type = newType;
                    nextGrid[flatCoord].type = newType;
                }
                else if (nextGrid[flatCoord].type == Params.rockCode || nextGrid[flatCoord].type == Params.waterCode)
                {
                    Grid[flatCoord].type = newType;
                    nextGrid[flatCoord].type = newType;
                }



            }
        }
    }

    public void Update()
    {
        Parallel.For(0, Params.GridSizeY, j =>
        {
            int offset = j * Params.GridSizeX;
            for (int i = 0; i < Params.GridSizeX; i++)
            {
                Cell updatedCell = Grid[i + offset].Update(i, j, Grid);
                nextGrid[i + offset] = updatedCell;
                // pixelBuffer[i + offset] = updatedCell.GetColor(); ;
            }
        });

        (Grid, nextGrid) = (nextGrid, Grid);
    }

    public void Draw()
    {
        Raylib.UpdateTexture(gridTexture, pixelBuffer);

        Raylib.DrawTexturePro(
        gridTexture,
        new Rectangle(0, 0, Params.GridSizeX, Params.GridSizeY),
        new Rectangle(0, 0, Params.ScreenSizeX, Params.ScreenSizeY),
        Vector2.Zero,
        0.0f,
        Color.White
        );

    }

    public void Close()
    {
        Raylib.UnloadTexture(gridTexture);
        Raylib.UnloadImage(image);
    }

    public void SetCellOnFire(Vector2 globalPos)
    {
        int gridX = (int)globalPos.X / Params.CellSize;
        int gridY = (int)globalPos.Y / Params.CellSize;
        Grid[gridX + gridY * Params.GridSizeX].SetOnFire();
    }
}
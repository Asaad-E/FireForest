using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;

using FireForest.Core;
using FireForest.CA;
using ScottPlot;
using ScottPlot.Plottables;
using SkiaSharp;
using System.Runtime.InteropServices;
using System.Data.Common;

namespace FireForest.UI;

public class PlotWidget
{
    public int PlotWidth = 350;
    public int PlotHeight = 210;
    public int MaxPoints = 200;

    double[] Data = [];
    double MaxData = 0;
    double MinData = 0;

    public Plot myPlot;
    public Signal Line;
    private Text LastLabel;
    private Marker LastMarker;

    public Texture2D PlotTexture;
    public Raylib_cs.Image UpdatedImage;

    public float Phase = 0;

    // Params for fast draw usign skiaSharp 
    private SKSurface Surface;

    private bool Updated = true;

    public PlotWidget()
    {
        myPlot = new Plot();

        // Deafult points
        Data = new double[MaxPoints];
        for (int i = 0; i < MaxPoints; i++)
        {
            Data[i] = 0;
        }

        // Apply transparent background
        myPlot.FigureBackground.Color = ScottPlot.Colors.Transparent;
        myPlot.DataBackground.Color = ScottPlot.Color.FromHex("#101010d0");

        // Dark theme axes text & lines

        myPlot.Axes.Color(ScottPlot.Color.FromHex("#f2f2f2"));
        myPlot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#444444");
        myPlot.Grid.MinorLineWidth = 2;

        // Title style
        myPlot.Title("Fire Count");
        myPlot.Axes.Title.Label.OffsetX = -100;
        myPlot.Axes.Title.Label.OffsetY = 15;


        // Line        
        Line = myPlot.Add.Signal(Data);
        Line.Color = ScottPlot.Color.FromHex("#C11007");
        Line.LineWidth = 1.5f;
        Line.Data.XOffset = -MaxPoints;
        myPlot.Axes.AutoScale();


        // Last point label and marker

        LastLabel = myPlot.Add.Text("0", MaxPoints - 1, 0);
        LastLabel.LabelFontColor = ScottPlot.Color.FromHex("#FFFFFF");
        LastLabel.LabelFontSize = 12;
        LastLabel.LabelBold = true;
        LastLabel.LabelAlignment = ScottPlot.Alignment.LowerRight;
        LastLabel.LabelOffsetX = -5;
        LastLabel.LabelOffsetY = 0;

        LastMarker = myPlot.Add.Marker(MaxPoints - 1, 0);
        LastMarker.Color = ScottPlot.Color.FromHex("#C11007");
        LastMarker.Size = 8;

        // Create the texture to render the plot on it

        var imageInfo = new SKImageInfo(PlotWidth, PlotHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        Surface = SKSurface.Create(imageInfo);

        Raylib_cs.Image blank = Raylib.GenImageColor(PlotWidth, PlotHeight, Raylib_cs.Color.Blank);
        PlotTexture = Raylib.LoadTextureFromImage(blank);
        Raylib.UnloadImage(blank);
    }

    public void Draw()
    {
        if (Updated)
        {
            // create the plot and write the texture with the raw byte info
            Surface.Canvas.Clear(SKColors.Transparent);
            myPlot.Render(Surface);

            using SKImage snapshot = Surface.Snapshot();
            using SKPixmap pixmap = snapshot.PeekPixels();

            unsafe
            {
                Raylib.UpdateTexture(PlotTexture, (void*)pixmap.GetPixels()); // raw upload, no re-encode

            }
            Updated = false;
        }

        Raylib.DrawTexture(PlotTexture, 0, SimParams.ScreenHeight - PlotHeight, Raylib_cs.Color.White);
    }

    public void AddPoint(int point)
    {
        Data.AsSpan()[1..].CopyTo(Data);
        Data[MaxPoints - 1] = point;

        if (point < MinData)
        {
            MinData = point;
        }
        else if (point > MaxData)
        {
            MaxData = point;
        }

        double padding = (MaxData - MinData) * 0.1;
        if (padding <= 0) padding = 1;

        myPlot.Axes.SetLimitsY(MinData - padding, MaxData + padding * 3);

        double coordX = MaxPoints - 1 + Line.Data.XOffset;
        LastMarker.Location = new Coordinates(coordX, point);
        LastLabel.Location = new Coordinates(coordX, point);
        LastLabel.LabelText = point.ToString();

        Updated = true;
    }
}

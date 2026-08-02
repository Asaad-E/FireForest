using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;

using FireForest.Core;
using FireForest.CA;

namespace FireForest;

public class SimulationScreen : IScreen
{
    public IScreen? NextScreen => null;
    public bool ShouldClose { get; private set; } = false;

    private Vector2 LastMousePos;
    private Camera2D Camera = new();

    private readonly CAEnv Ca;
    private readonly HUD Hud;
    private int frameCount = 0;

    // UI States
    const float UIPadding = 20f;

    private ImGuiIOPtr IO;
    private bool TerrainChanged = false;

    public SimulationScreen(int gridSizeX, int gridSizeY)
    {
        // Initialize CA
        Ca = new();
        Ca.Setup(gridSizeX, gridSizeY);
        Ca.LoadTextures();

        // Change the scren size
        Raylib.SetWindowSize(SimParams.ScreenWidth, SimParams.ScreenHeight);

        int monitor = Raylib.GetCurrentMonitor();
        int monitorWidth = Raylib.GetMonitorWidth(monitor);
        int monitorHeight = Raylib.GetMonitorHeight(monitor);

        int posX = (monitorWidth - SimParams.ScreenWidth) / 2;
        int posY = (monitorHeight - SimParams.ScreenHeight) / 2;
        Raylib.SetWindowPosition(posX, posY);

        // Initialize Camera
        Camera.Target = new Vector2(SimParams.SimulationWidth * CAParams.CellSize / 2f, SimParams.SimulationHeight * CAParams.CellSize / 2f);
        Camera.Offset = new Vector2(SimParams.ScreenWidth / 2, SimParams.ScreenHeight / 2);

        Camera.Zoom = 1;
        Camera.Rotation = 0;

        SimParams.MinZoom = SimParams.ScreenHeight / (float)SimParams.SimulationHeight;

        // // UI
        IO = ImGui.GetIO();

        float UIPadding = 20f;
        Hud = new(new Vector2(SimParams.ScreenWidth - UIPadding, UIPadding));

        Hud.RestartCAResquested += () => Ca.Restart();
        Hud.RegenerateCAResquested += () => Ca.ReGenerate();
        Hud.ZoomOutResquested += () => ZoomOut();
        Hud.ZoomResetResquested += () => ZoomReset();
        Hud.TerrainChangedResquested += () => ChangeTerrain();

    }

    public void Update(float deltaTime)
    {
        // --------------------------- Controls ---------------------------

        // Restart
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            Ca.Restart();
        }

        // Show UI
        if (Raylib.IsKeyPressed(KeyboardKey.LeftControl) || Raylib.IsKeyPressed(KeyboardKey.RightControl))
        {
            Hud.ShowUi = !Hud.ShowUi;
        }

        // Drag
        Vector2 CurrentMousePos = Raylib.GetMousePosition();
        if (Raylib.IsMouseButtonDown(MouseButton.Left) && (!IO.WantCaptureMouse || !Hud.ShowUi))
        {
            if (CurrentMousePos != LastMousePos)
            {
                Camera.Target += (CurrentMousePos - LastMousePos) * -0.8f * (1 / Camera.Zoom);
            }

        }
        LastMousePos = CurrentMousePos;

        // spawn fire at mouse pos whe click
        if (Raylib.IsMouseButtonPressed(MouseButton.Right) && (!IO.WantCaptureMouse || !Hud.ShowUi))
        {
            Ca.SetCellOnFire(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), Camera), CAParams.FireDuration);
        }

        // Change Zoom
        Camera.Zoom = MathF.Max(MathF.Exp(Raylib.GetMouseWheelMove() * 0.1f + MathF.Log(Camera.Zoom)), SimParams.MinZoom);

        // Clamp the camera in case of drag or change of zoom
        if (Raylib.IsMouseButtonDown(MouseButton.Left) || Raylib.GetMouseWheelMove() != 0)
        {
            ClampCamera();
        }

        // --------------------------- Update ---------------------------

        if (TerrainChanged)
        {
            Ca.ChangeTerrain();
            TerrainChanged = false;
            Hud.Stop = false;
            return;
        }

        frameCount++;
        if (frameCount >= SimParams.FrameStep && !Hud.Stop)
        {
            frameCount = 0;

            long startUpdate = Stopwatch.GetTimestamp();

            Ca.Update(CAParams.GetSnapshotParams());

            TimeSpan ElapsedTimeUpdate = Stopwatch.GetElapsedTime(startUpdate);

            Console.WriteLine(ElapsedTimeUpdate.TotalMilliseconds);

        }

    }
    public void Draw()
    {
        Raylib.BeginMode2D(Camera);

        long startDraw = Stopwatch.GetTimestamp();

        Ca.Draw(SimParams.SimulationWidth, SimParams.SimulationHeight);

        TimeSpan ElapsedTimeDraw = Stopwatch.GetElapsedTime(startDraw);

        Console.WriteLine("-------------------------------------------------------");
        Console.WriteLine(ElapsedTimeDraw.TotalMilliseconds);


        Raylib.EndMode2D();

        // --------------------------- UI ---------------------------

        Hud.Draw();
    }

    private void ChangeTerrain()
    {
        Utils.SetupNoiseParams();
        TerrainChanged = true;
        Hud.Stop = true;
    }

    private void ClampCamera()
    {
        Vector2 MinTarget = (1 / Camera.Zoom) * new Vector2(SimParams.ScreenWidth, SimParams.ScreenHeight) / 2;
        Vector2 MaxTarget = new Vector2(SimParams.SimulationWidth, SimParams.SimulationHeight) - MinTarget;

        Camera.Target = Vector2.Clamp(Camera.Target, MinTarget, MaxTarget);
    }

    private void ZoomOut()
    {
        Camera.Zoom = SimParams.MinZoom;
        ClampCamera();
    }

    private void ZoomReset()
    {
        Camera.Zoom = 1;
    }

    public void Close()
    {
        Ca.Close();
    }
}

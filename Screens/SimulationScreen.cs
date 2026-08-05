using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;

using FireForest.Core;
using FireForest.CA;
using FireForest.UI;

namespace FireForest.Screens;

public class SimulationScreen : IScreen
{
    public IScreen? NextScreen => null;
    public bool ShouldClose { get; private set; } = false;

    private Vector2 LastMousePos;
    private Camera2D Camera = new();

    private readonly CAEnv Ca;
    private readonly HUD Hud;

    // UI States

    private readonly float UIPadding = 20f;
    private readonly ImGuiIOPtr IO;
    private bool TerrainChanged = false;
    private readonly float PlotTimerDuration = 5 / 60f;
    private float PlotTimer = 0;
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
        Camera.Target = new Vector2(SimParams.ScreenWidth / 2f, SimParams.ScreenHeight / 2f);
        Camera.Offset = new Vector2(SimParams.ScreenWidth / 2, SimParams.ScreenHeight / 2);

        Camera.Zoom = SimParams.StartingZoom;
        Camera.Rotation = 0;

        // // UI
        IO = ImGui.GetIO();

        Hud = new();
        Hud.SetPosition(new Vector2(SimParams.ScreenWidth - UIPadding, UIPadding));

        Hud.RestartCAResquested += () => Ca.Restart();
        Hud.RegenerateCAResquested += () => Ca.ReGenerate();
        Hud.ZoomOutResquested += () => ZoomOut();
        Hud.ZoomResetResquested += () => ZoomReset();
        Hud.TerrainChangedResquested += () => ChangeTerrain();
        Hud.FullscreenResquested += () => ToggleFullscreen();
    }

    public void Controls()
    {
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
        if ((Raylib.IsMouseButtonDown(MouseButton.Left) || Raylib.IsMouseButtonDown(MouseButton.Middle))&& (!IO.WantCaptureMouse || !Hud.ShowUi))
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
            Vector2 pos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), Camera);
            pos = new Vector2(pos.X * SimParams.RatioW, pos.Y * SimParams.RatioH);
            Ca.SetCellOnFire(pos, CAParams.FireDuration);
        }

        // Change Zoom
        Camera.Zoom = MathF.Max(MathF.Exp(Raylib.GetMouseWheelMove() * 0.1f + MathF.Log(Camera.Zoom)), 1);

        // Clamp the camera in case of drag or change of zoom
        if (Raylib.IsMouseButtonDown(MouseButton.Left) || Raylib.GetMouseWheelMove() != 0)
        {
            ClampCamera();
        }
    }

    public void Update(float deltaTime)
    {
        // --------------------------- Controls ---------------------------

        Controls();

        // --------------------------- Update ---------------------------

        if (TerrainChanged)
        {
            Ca.ChangeTerrain();
            TerrainChanged = false;
            return;
        }

        if (!Hud.Stop)
        {
            for (int i = 0; i < SimParams.SimulationSpeed; i++)
            {
                Ca.Update(CAParams.GetSnapshotParams());
            }
        }

        PlotTimer += deltaTime;
        if (!Hud.Stop && PlotTimer >= PlotTimerDuration)
        {
            Hud.Plot.AddPoint(Ca.FireCount);
            PlotTimer = 0;
        }
    }
    public void Draw()
    {
        Raylib.BeginMode2D(Camera);

        Ca.Draw(SimParams.ScreenWidth, SimParams.ScreenHeight);

        Raylib.EndMode2D();

        // --------------------------- UI ---------------------------

        Hud.Draw();
    }
    private void ChangeTerrain()
    {
        Utils.SetupNoiseParams();
        TerrainChanged = true;
    }

    private void ClampCamera()
    {
        Vector2 MinTarget = (0.5f / Camera.Zoom) * new Vector2(SimParams.ScreenWidth, SimParams.ScreenHeight);
        Vector2 MaxTarget = new Vector2(SimParams.ScreenWidth, SimParams.ScreenHeight) - MinTarget;

        Camera.Target = Vector2.Clamp(Camera.Target, MinTarget, MaxTarget);
    }

    private void ZoomOut()
    {
        Camera.Zoom = 1;
        ClampCamera();
    }

    private void ZoomReset()
    {
        Camera.Zoom = SimParams.StartingZoom;
    }

    public void Close()
    {
        Hud.Close();
        Ca.Close();
    }

    public void ToggleFullscreen()
    {
        Raylib.ToggleFullscreen();

        SimParams.ScreenWidth = Raylib.GetRenderWidth();
        SimParams.ScreenHeight = Raylib.GetRenderHeight();

        Camera.Target = new Vector2(SimParams.ScreenWidth / 2f, SimParams.ScreenHeight / 2f);
        Camera.Offset = new Vector2(SimParams.ScreenWidth / 2, SimParams.ScreenHeight / 2);

        Hud.SetPosition(new Vector2(SimParams.ScreenWidth - UIPadding, UIPadding));

        ZoomOut();
    }


}

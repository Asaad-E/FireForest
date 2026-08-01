using System;
using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

using rlImGui_cs;
using ImGuiNET;

namespace FireForest;


static class Program
{

    static int frameCount = 0;
    static readonly CA Ca = new();

    static bool ShowUi = true;
    static bool ShowFPS = true;
    static bool ShowGuide = true;

    const float UIPadding = 20f;

    [System.STAThread]
    static void Main()
    {
        // init windows
        Raylib.InitWindow(SimParams.ScreenWidth, SimParams.ScreenHeight, "Fire Forrest CA");
        Raylib.SetTargetFPS(SimParams.FPS);

        rlImGui.Setup(true);
        ImGui.SetNextWindowCollapsed(true);

        var io = ImGui.GetIO();

        // Game loop
        while (!Raylib.WindowShouldClose())
        {
            // New frame
            float deltatime = Raylib.GetFrameTime();

            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && !io.WantCaptureMouse)
            {
                Ca.SetCellOnFire(Raylib.GetMousePosition());
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                Ca.Restart();
            }

            if (Raylib.IsKeyPressed(KeyboardKey.LeftControl))
            {
                ShowUi = !ShowUi;
            }

            // Draw
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);


            // Draw Grid
            long startDraw = Stopwatch.GetTimestamp();

            Ca.Draw();

            TimeSpan ElapsedTimeDraw = Stopwatch.GetElapsedTime(startDraw);

            // Update
            if (frameCount >= SimParams.FrameStep)
            {
                frameCount = 0;

                long startUpdate = Stopwatch.GetTimestamp();

                Ca.Update();

                TimeSpan ElapsedTimeUpdate = Stopwatch.GetElapsedTime(startUpdate);

                Console.WriteLine("-------------------------------------------------------");
                Console.WriteLine(ElapsedTimeDraw.TotalMilliseconds);
                Console.WriteLine(ElapsedTimeUpdate.TotalMilliseconds);

            }

            // Text
            if (ShowFPS)
            {
                Raylib.DrawRectangle(15, 15, 115, 28, Color.Black);
                Raylib.DrawText($"FPS: {1 / deltatime:F2}", 20, 20, 20, Color.White);
            }

            if (!ShowUi && ShowGuide)
            {
                Raylib.DrawText($"Press Control to open the config or Space to Reset the World", 10, SimParams.ScreenHeight - 20, 20, Color.White);
            }

            if (ShowUi)
            {
                DrawUI();
            }

            // End
            Raylib.EndDrawing();
            frameCount++;
        }

        // Close
        rlImGui.Shutdown();
        Raylib.CloseWindow();
        Ca.Close();
    }


    static void DrawUI()
    {

        rlImGui.Begin();

        ImGui.SetNextWindowPos(new Vector2(SimParams.ScreenWidth - UIPadding, UIPadding), ImGuiCond.Always, new Vector2(1, 0));


        if (ImGui.Begin("Options", ref ShowUi, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings))
        {
            ImGui.SeparatorText("Simulation");

            ImGui.Button("Stop");

            if (ImGui.Button("Restart"))
            {
                Ca.Restart();
            }

            ImGui.SeparatorText("Simulation Parametres");

            ImGui.SliderInt("Simulation Steps: ", ref SimParams.FrameStep, 1, 10);

            if (ImGui.SliderFloat("Fire Expand Prob %", ref CAParams.FireProb, 0, 1))
            {
                CAParams.FireProb = MathF.Floor(CAParams.FireProb / 0.05f) * 0.05f;
            }

            ImGui.SliderInt("Fire Duration: ", ref CAParams.FireDuration, 1, 80);

            if (ImGui.SliderInt("Spontaneus Fire Mult", ref CAParams.SpontaneousFireProbMult, 1, 20))
            {
                CAParams.SpontaneousFireProb = CAParams.SpontaneousFireProbBase * CAParams.SpontaneousFireProbMult;
            }

            if (ImGui.SliderInt("Spontaneus Tree mult", ref CAParams.TreeProbMult, 1, 20))
            {
                CAParams.TreeProb = CAParams.TreeProbBase * CAParams.TreeProbMult;
            }

            ImGui.Checkbox("Show FPS", ref ShowFPS);
            ImGui.SameLine();
            ImGui.Checkbox("Show Guide", ref ShowGuide);

            ImGui.SeparatorText("World Geenration Parametres");

            bool TerrainLevelChanged = false;

            TerrainLevelChanged |= ImGui.SliderFloat("Water Level", ref SimParams.WaterLevel, 0, SimParams.RockLevel);
            TerrainLevelChanged |= ImGui.SliderFloat("Rock Level", ref SimParams.RockLevel, SimParams.WaterLevel, 1);

            if (TerrainLevelChanged)
            {
                Ca.TerrainChanged = true;
            }

        }

        ImGui.End();
        rlImGui.End();

    }

}


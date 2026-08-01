using System;
using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

using rlImGui_cs;
using ImGuiNET;

namespace FireForrest;


static class Program
{

    static int frameCount = 0;

    static readonly CA Ca = new();

    static bool ShowUi = true;
    static bool ShowFPS = true;
    static bool ShowGuide = true;

    [System.STAThread]
    static void Main()
    {
        // init windows
        Raylib.InitWindow(Params.ScreenSizeX, Params.ScreenSizeY, "Fire Forrest CA");
        Raylib.SetTargetFPS(Params.FPS);

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
                Ca.Reset();
            }

            if (Raylib.IsKeyPressed(KeyboardKey.LeftControl))
            {
                ShowUi = !ShowUi;
            }

            // Draw
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);


            // Draw Grid

            Ca.Draw();

            // Update
            if (frameCount >= Params.frameStep)
            {
                frameCount = 0;

                long start = Stopwatch.GetTimestamp();

                Ca.Update();
                
                TimeSpan ElapsedTime = Stopwatch.GetElapsedTime(start);
                Console.WriteLine(ElapsedTime.TotalMilliseconds);

            }

            // Text
            if (ShowFPS)
            {
                Raylib.DrawRectangle(15, 15, 115, 28, Color.Black);
                Raylib.DrawText($"FPS: {1 / deltatime:F2}", 20, 20, 20, Color.White);
            }

            if (!ShowUi && ShowGuide)
            {
                Raylib.DrawText($"Press Control to open the config or Space to Reset the World", 10, Params.ScreenSizeY - 20, 20, Color.White);
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

        ImGui.SetNextWindowPos(new Vector2(Params.ScreenSizeX - Params.UIPadding, Params.UIPadding), ImGuiCond.Always, new Vector2(1, 0));


        if (ImGui.Begin("Options", ref ShowUi, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings))
        {
            ImGui.SeparatorText("Simulation Parametres");
            ImGui.SliderInt("Simulation Steps: ", ref Params.frameStep, 1, 10);
            if (ImGui.SliderFloat("Fire Expand Prob %", ref Params.fireProb, 0, 1))
            {
                Params.fireProb = MathF.Floor(Params.fireProb / 0.05f) * 0.05f;
            }
            ImGui.SliderInt("Fire Duration: ", ref Params.fireDuration, 1, 80);
            if (ImGui.SliderInt("Spontaneus Fire Mult", ref Params.spontaneousFireMult, 1, 20))
            {
                Params.spontaneousFireProb = Params.spontaneousFireProbBase * Params.spontaneousFireMult;
            }
            if (ImGui.SliderInt("Spontaneus Tree mult", ref Params.treeProbMult, 1, 20))
            {
                Params.treeProb = Params.treeProbBase * Params.treeProbMult;
            }

            ImGui.Checkbox("Show FPS", ref ShowFPS);
            ImGui.SameLine();
            ImGui.Checkbox("Show Guide", ref ShowGuide);

            ImGui.SeparatorText("World Parametres");
            if (ImGui.SliderFloat("Water Level", ref Params.waterLevel, 0, Params.rockLevel))
            {
                Ca.TerrainLevelChanged();
            }
            if (ImGui.SliderFloat("Rock Level", ref Params.rockLevel, Params.waterLevel, 1))
            {
                Ca.TerrainLevelChanged();

            }

        }

        ImGui.End();
        rlImGui.End();

    }

}


using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;


using FireForest.Core;
using FireForest.CA;
using FireForest.Screens;

namespace FireForest;

static class Program
{

    [System.STAThread]
    static void Main()
    {
        Raylib.InitWindow(SimParams.LauncherScreenWidth, SimParams.LauncherScreenHeight, "Launcher");
        Raylib.SetTargetFPS(SimParams.FPS);

        Raylib.SetTraceLogLevel(TraceLogLevel.None);

        rlImGui.Setup(true);

        IScreen current = new LauncherScreen();

        while (!Raylib.WindowShouldClose())
        {
            // Update
            float deltaTime = Raylib.GetFrameTime();

            current.Update(deltaTime);

            // Drawing
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            rlImGui.Begin();
            
            current.Draw();

            rlImGui.End();
            Raylib.EndDrawing();

            // Screen Change
            if (current.NextScreen is not null)
            {
                // Close the current screen and changed to the next one is the next is available
                current.Close();
                current = current.NextScreen;
            }
        }

        rlImGui.Shutdown();
        current.Close();
        Raylib.CloseWindow();
    }


}


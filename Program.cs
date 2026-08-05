using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;


using FireForest.Core;
using FireForest.CA;
using FireForest.Screens;
using System.Reflection;
using System.Text;

namespace FireForest;

static class Program
{

    [STAThread]
    static void Main()
    {
        // Raylib setup
        Raylib.InitWindow(SimParams.LauncherScreenWidth, SimParams.LauncherScreenHeight, "Launcher");
        Raylib.SetTargetFPS(SimParams.FPS);
        Raylib.SetTraceLogLevel(TraceLogLevel.None);

        // Icon
        LoadEmbedIcon();

        // Imgui setup
        rlImGui.Setup(true);
        unsafe
        {
            ImGui.GetIO().NativePtr->LogFilename = null;
            ImGui.GetIO().NativePtr->IniFilename = null;
        }

        // Start with launcher screen
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

        current.Close();
        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }

    static void LoadEmbedIcon()
    {
        string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "icon.png");
        Image icon = Raylib.LoadImage(iconPath);
        Raylib.SetWindowIcon(icon);
        Raylib.UnloadImage(icon);

    }
}


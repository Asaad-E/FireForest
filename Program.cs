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

    // UI States
    static bool ShowUi = true;
    static bool ShowFPS = true;
    static bool ShowGuide = true;
    static bool Stop = false;
    const float UIPadding = 20f;

    static Vector2 LastMousePos;

    static Camera2D Camera = new();

    [System.STAThread]
    static void Main()
    {
        // Show launcher for user selected settings
        bool Start = Launcher();

        // Main loop with the Selected settings
        if (Start)
        {
            MainLoop();
        }
    }

    static bool Launcher()
    {
        Raylib.InitWindow(SimParams.LauncherScreenWidth, SimParams.LauncherScreenWidth, "Launcher - Fire Forrest CA");
        Raylib.SetTargetFPS(SimParams.FPS);

        rlImGui.Setup(true);

        ImGui.SetNextWindowCollapsed(false);

        float w = 200.0f;
        float h = 40;

        bool Open = false;

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            rlImGui.Begin();

            ImGui.SetNextWindowSize(new Vector2(SimParams.LauncherScreenWidth, SimParams.LauncherScreenHeight));
            ImGui.SetNextWindowPos(new Vector2(0, 0));


            if (ImGui.Begin("Launcher", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar))
            {


                ImGui.SetWindowFontScale(2);
                ImGui.SeparatorText("Simulation Settings");


                ImGui.SetCursorPosY(80);

                bool changedSceenSize = false;

                ImGui.SeparatorText("Window");

                ImGui.SetWindowFontScale(1f);
                ImGui.Text("Screen Width");
                changedSceenSize |= ImGui.SliderInt("##1", ref SimParams.ScreenWidth, 600, 2000);

                ImGui.Text("Screen Height");
                changedSceenSize |= ImGui.SliderInt("##2", ref SimParams.ScreenHeight, 600, 2000);

                float aspectRatio = SimParams.ScreenWidth / (float)SimParams.ScreenHeight;
                if (changedSceenSize)
                {
                    SimParams.SimulationWidth = (int)MathF.Floor(SimParams.SimulationHeight * aspectRatio);
                }

                ImGui.SetCursorPosY(230);

                bool changeSimulationWidth = false;
                bool changeSimulationHeight = false;
                ImGui.SetWindowFontScale(2f);


                ImGui.SeparatorText("Celular Automata");

                ImGui.SetWindowFontScale(1f);

                ImGui.Text("Simulation Width");
                changeSimulationWidth |= ImGui.SliderInt("##3", ref SimParams.SimulationWidth, 100, SimParams.MaxSimulationWidth);

                ImGui.Text("Simulation Height");
                changeSimulationHeight |= ImGui.SliderInt("##4", ref SimParams.SimulationHeight, 100, SimParams.MaxSimulationHeight);

                if (changeSimulationHeight)
                {
                    SimParams.SimulationWidth = (int)MathF.Floor(SimParams.SimulationHeight * aspectRatio);

                    if (SimParams.SimulationWidth > SimParams.MaxSimulationWidth)
                    {
                        SimParams.SimulationWidth = SimParams.MaxSimulationWidth;
                        SimParams.SimulationHeight = (int)MathF.Floor(SimParams.SimulationWidth / aspectRatio);
                    }
                }
                else if (changeSimulationWidth)
                {
                    SimParams.SimulationHeight = (int)MathF.Floor(SimParams.SimulationWidth / aspectRatio);

                    if (SimParams.SimulationHeight > SimParams.MaxSimulationHeight)
                    {
                        SimParams.SimulationHeight = SimParams.MaxSimulationHeight;
                        SimParams.SimulationWidth = (int)MathF.Floor(SimParams.SimulationHeight * aspectRatio);
                    }
                }


                ImGui.SetWindowFontScale(2);
                ImGui.SetCursorPosY(SimParams.LauncherScreenHeight - h - 120);
                ImGui.SetCursorPosX((SimParams.LauncherScreenWidth - w) * 0.5f);
                if (ImGui.Button("Start", new Vector2(w, h)))
                {
                    Open = true;
                    break;
                }
            }
            ImGui.End();

            rlImGui.End();
            Raylib.EndDrawing();

        }

        rlImGui.Shutdown();
        Raylib.CloseWindow();

        CAParams.GridSizeX = SimParams.SimulationWidth;
        CAParams.GridSizeY = SimParams.SimulationHeight;
        CAParams.Totalcells = CAParams.GridSizeX * CAParams.GridSizeY;

        // Start loadign the CA
        Ca.Setup();


        return Open;
    }

    static void ClampCamera()
    {
        Vector2 MinTarget = (1 / Camera.Zoom) * new Vector2(SimParams.ScreenWidth, SimParams.ScreenHeight) / 2;
        Vector2 MaxTarget = new Vector2(SimParams.SimulationWidth, SimParams.SimulationHeight) - MinTarget;

        Camera.Target = Vector2.Clamp(Camera.Target, MinTarget, MaxTarget);
    }

    static void MainLoop()
    {
        // init windows
        Raylib.InitWindow(SimParams.ScreenWidth, SimParams.ScreenHeight, "Fire Forrest CA");
        Raylib.SetTargetFPS(SimParams.FPS);

        rlImGui.Setup(true);
        ImGui.SetNextWindowCollapsed(true);



        var io = ImGui.GetIO();
        // init Camera
        Camera.Target = new Vector2(SimParams.SimulationWidth * CAParams.CellSize / 2f, SimParams.SimulationHeight * CAParams.CellSize / 2f);
        Camera.Offset = new Vector2(SimParams.ScreenWidth / 2, SimParams.ScreenHeight / 2);

        Camera.Zoom = 1;
        Camera.Rotation = 0;

        SimParams.MinZoom = SimParams.ScreenHeight / (float)SimParams.SimulationHeight;

        // init ca

        Ca.LoadTextures();

        // Game loop
        while (!Raylib.WindowShouldClose())
        {
            // New frame

            float deltatime = Raylib.GetFrameTime();

            // Controls
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                Ca.Restart();
            }

            if (Raylib.IsKeyPressed(KeyboardKey.LeftControl))
            {
                ShowUi = !ShowUi;
            }

            Vector2 CurrentMousePos = Raylib.GetMousePosition();
            if (Raylib.IsMouseButtonDown(MouseButton.Left) && (!io.WantCaptureMouse || !ShowUi))
            {
                if (CurrentMousePos != LastMousePos)
                {
                    Camera.Target += (CurrentMousePos - LastMousePos) * -0.8f * (1 / Camera.Zoom);
                }

            }
            if (Raylib.IsMouseButtonPressed(MouseButton.Right) && (!io.WantCaptureMouse || !ShowUi))
            {
                Ca.SetCellOnFire(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), Camera));
            }
            LastMousePos = CurrentMousePos;

            Camera.Zoom = MathF.Max(MathF.Exp(Raylib.GetMouseWheelMove() * 0.1f + MathF.Log(Camera.Zoom)), SimParams.MinZoom);

            // Clamp the camera it case of move or change of zoom
            if (Raylib.IsMouseButtonDown(MouseButton.Left) || Raylib.GetMouseWheelMove() != 0)
            {
                ClampCamera();
            }

            // Draw

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            Raylib.BeginMode2D(Camera);

            // Draw Grid

            // If terrain is changed, recalculate colors
            if (Ca.TerrainChanged)
            {
                if (Stop) Stop = false;
                Ca.ChangeTerrain();
                Ca.TerrainChanged = false;
            }

            long startDraw = Stopwatch.GetTimestamp();

            Ca.Draw();

            TimeSpan ElapsedTimeDraw = Stopwatch.GetElapsedTime(startDraw);

            Raylib.EndMode2D();

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

            // Update
            if (frameCount >= SimParams.FrameStep && !Stop)
            {
                frameCount = 0;

                long startUpdate = Stopwatch.GetTimestamp();

                Ca.Update();

                TimeSpan ElapsedTimeUpdate = Stopwatch.GetElapsedTime(startUpdate);

                Console.WriteLine("-------------------------------------------------------");
                Console.WriteLine(ElapsedTimeDraw.TotalMilliseconds);
                Console.WriteLine(ElapsedTimeUpdate.TotalMilliseconds);

            }
        }

        Ca.Close();
        rlImGui.Shutdown();

    }

    static void DrawUI()
    {

        rlImGui.Begin();

        ImGui.SetNextWindowPos(new Vector2(SimParams.ScreenWidth - UIPadding, UIPadding), ImGuiCond.Always, new Vector2(1, 0));

        if (ImGui.Begin("Options", ref ShowUi, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings))
        {

            //---------- Simulation Buttoms ----------
            ImGui.SeparatorText("Simulation");

            if (ImGui.Button(Stop ? "Play" : "Stop"))
            {
                Stop = !Stop;
            }

            ImGui.SameLine();

            if (ImGui.Button("Restart"))
            {
                Ca.Restart();
            }
            ImGui.SameLine();

            if (ImGui.Button("Regenerate World"))
            {
                Ca.ReGenerate();
            }

            if (ImGui.Button("Zoom Out"))
            {
                Camera.Zoom = SimParams.MinZoom;
                ClampCamera();
            }
            ImGui.SameLine();

            if (ImGui.Button("Reset Zoom"))
            {
                Camera.Zoom = 1;
            }

            //---------- Simulation Paramss ----------

            ImGui.SeparatorText("Simulation Params");

            ImGui.SliderInt("Sim Speed ", ref SimParams.FrameStep, 1, 10);

            if (ImGui.SliderFloat("Fire Expand Prob %", ref CAParams.FireProb, 0, 1))
            {
                CAParams.FireProb = MathF.Round(CAParams.FireProb / 0.05f) * 0.05f;
            }

            ImGui.SliderInt("AVG Fire Duration ", ref CAParams.FireDuration, 1, 150);

            if (ImGui.SliderInt("Spont Fire Mult", ref CAParams.SpontaneousFireProbMult, 1, 20))
            {
                CAParams.SpontaneousFireProb = CAParams.SpontaneousFireProbBase * CAParams.SpontaneousFireProbMult;
            }

            if (ImGui.SliderInt("Spont Tree Prob mult", ref CAParams.TreeProbMult, 1, 20))
            {
                CAParams.TreeProb = CAParams.TreeProbBase * CAParams.TreeProbMult;
            }

            if (ImGui.SliderInt("Soil Prob mult", ref CAParams.SoilProbMult, 1, 20))
            {
                CAParams.SoilProb = CAParams.SoilProbBase * CAParams.SoilProbMult;
            }


            //---------- World Generation Params ----------
            ImGui.SeparatorText("World Generation Params");

            bool TerrainLevelChanged = false;

            TerrainLevelChanged |= ImGui.SliderFloat("Water Level", ref SimParams.WaterLevel, 0, SimParams.RockLevel);
            TerrainLevelChanged |= ImGui.SliderFloat("Rock Level", ref SimParams.RockLevel, SimParams.WaterLevel, 1);
            TerrainLevelChanged |= ImGui.SliderFloat("Frecuency", ref SimParams.NoiseFrecuency, 0.001f, 0.007f);
            TerrainLevelChanged |= ImGui.SliderInt("Octaves", ref SimParams.NoiseOctaves, 1, 10);

            if (TerrainLevelChanged)
            {
                Utils.SetupNoiseParams();
                Ca.TerrainChanged = true;
                Stop = true;
            }


            // UI
            ImGui.SeparatorText("UI");
            ImGui.Checkbox("Show FPS", ref ShowFPS);
            ImGui.SameLine();
            ImGui.Checkbox("Show Guide", ref ShowGuide);
        }

        ImGui.End();
        rlImGui.End();

    }

}


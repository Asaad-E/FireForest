using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;

using FireForest.Core;
using FireForest.CA;

namespace FireForest;


class HUD(Vector2 pos)
{
    private Vector2 Position = pos;
    public bool ShowUi = true;
    public bool ShowFPS = true;
    public bool ShowGuide = false;
    public bool Stop = false;
    public bool AutoStop = false;

    public event Action? RestartCAResquested;
    public event Action? RegenerateCAResquested;
    public event Action? ZoomOutResquested;
    public event Action? ZoomResetResquested;
    public event Action? TerrainChangedResquested;


    public void Draw()
    {
        float deltatime = Raylib.GetFrameTime();

        if (ShowFPS)
        {
            Raylib.DrawRectangle(15, 15, 115, 28, Color.Black);
            Raylib.DrawText($"FPS: {1 / deltatime:F2}", 20, 20, 20, Color.White);
        }

        if (!ShowUi && ShowGuide)
        {
            Raylib.DrawText($"Press Control to open the config or Space to Reset the World", 10, SimParams.ScreenHeight - 20, 20, Color.White);
        }

        // IMGUI
        if (!ShowUi) return;

        ImGui.SetNextWindowPos(Position, ImGuiCond.Always, new Vector2(1, 0));

        if (ImGui.Begin("Options", ref ShowUi, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings))
        {

            //---------- Simulation Buttoms ----------
            ImGui.SeparatorText("Simulation");

            if (ImGui.Button(Stop ? "Play" : "Stop"))
            {
                Stop = !Stop;
            }

            ImGui.SameLine();
            if (ImGui.Button("Restart")) RestartCAResquested?.Invoke();
            ImGui.SameLine();
            if (ImGui.Button("Regenerate World")) RegenerateCAResquested?.Invoke();
            if (ImGui.Button("Zoom Out")) ZoomOutResquested?.Invoke();
            ImGui.SameLine();
            if (ImGui.Button("Reset Zoom")) ZoomResetResquested?.Invoke();

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

            bool terrainChanged = false;
            terrainChanged |= ImGui.SliderFloat("Water Level", ref SimParams.WaterLevel, 0, SimParams.RockLevel);
            terrainChanged |= ImGui.SliderFloat("Rock Level", ref SimParams.RockLevel, SimParams.WaterLevel, 1);
            terrainChanged |= ImGui.SliderFloat("Frecuency", ref SimParams.NoiseFrecuency, 0.001f, 0.007f);
            terrainChanged |= ImGui.SliderInt("Octaves", ref SimParams.NoiseOctaves, 1, 10);

            if (terrainChanged) TerrainChangedResquested?.Invoke();

            // Utils.SetupNoiseParams();
            // Ca.TerrainChanged = true;
            // Stop = true;

            // UI
            ImGui.SeparatorText("UI");
            ImGui.Checkbox("Show FPS", ref ShowFPS);
            ImGui.SameLine();
            ImGui.Checkbox("Show Guide", ref ShowGuide);

            ImGui.SameLine();

            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 70);
            if (ImGui.Button("Close", new Vector2(50, 0)))
            {
                ShowUi = !ShowUi;
            }
        }

        ImGui.End();

    }
}
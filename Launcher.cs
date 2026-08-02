using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;
namespace FireForest;

using FireForest.Core;
using FireForest.CA;

public class Launcher : IScreen
{
    private IScreen? Next;
    public bool ShouldClose { get; private set; } = false;
    public IScreen? NextScreen => Next;

    private readonly float StartButtonW = 200.0f;
    private readonly float StartButtonH = 40;

    private string MSG = "Start";

    public Launcher()
    {
    }

    public void Update(float deltaTime)
    {

    }
    public void Draw()
    {
        // Skip Draw when the next screen is ready
        if (Next is not null) return;

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
            ImGui.SetCursorPosY(SimParams.LauncherScreenHeight - StartButtonH - 120);
            ImGui.SetCursorPosX((SimParams.LauncherScreenWidth - StartButtonW) * 0.5f);
            if (ImGui.Button(MSG, new Vector2(StartButtonW, StartButtonH)))
            {
                MSG = "Loading...";
                StartSimulation();
            }
        }
        ImGui.End();
    }

    public void StartSimulation()
    {
        CAParams.GridSizeX = SimParams.SimulationWidth;
        CAParams.GridSizeY = SimParams.SimulationHeight;
        CAParams.Totalcells = CAParams.GridSizeX * CAParams.GridSizeY;

        Next = new SimulationScreen(CAParams.GridSizeX, CAParams.GridSizeY);
    }

    public void Close()
    {
    }
}
# FireForest

![Showcase](Docs/showcase.gif)

A real-time wildfire cellular automaton simulation built in C# with Raylib. Simulates fire ignition, spread, burnout, and forest regrowth over a procedurally generated terrain, with live-tunable parameters and a real-time stats overlay.

## Features

- Real-time CA simulation running in parallel across all CPU cores
- Procedurally generated terrain (water / tree / rock) using layered simplex noise for elevation and fuel capacity
- Live parameter tuning panel (fire spread probability, fire duration, spontaneous ignition rate, regrowth rates, terrain thresholds) with no restart required
- Live Fire Count plot to track burn dynamics over time
- Toroidal (wraparound) grid — fire and terrain effects wrap seamlessly at map edges
- Fast texture-based rendering pipeline, decoupled from simulation resolution
- Runs at target framerate on multi-million-cell grids

## Controls
* **Left Click:** Click to ignite a cell manually
* **Right Click + Drag:** Pan camera
* **Scroll Wheel:** Zoom in / out
* **Spacebar:** Regenerate terrain
* **Control:** Show/hide UI.

## Getting Started
 
### Windows (prebuilt binary)
 
Download `FireForest.exe` from the [Releases](https://github.com/Asaad-E/FireForest/releases) page and run it directly. No .NET install required if published as self-contained.
 
### Build from source (Windows / Linux / macOS)
 
Requirements:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
```bash
git clone https://github.com/xxxx/FireForest.git
cd FireForest
dotnet restore
dotnet run --project FireForest.csproj -c Release
```

## Simulation characteristics

Each cell is one of six states: `Water`, `Rock`, `Tree`, `Fire`, `Calcined`, `Soil`. The simualtion uses a 4-neighbor (von Neumann) neighborhood and updates every cell in parallel each frame based only on its previous state and its neighbors' previous state.

**State cycle:**
```
Tree --(ignition)--> Fire --(burns out)--> Calcined --(regrowth)--> Soil --(regrowth)--> Tree
```

- **Ignition** — a `Tree` cell adjacent to a `Fire` cell ignites with a probability derived from a base fire-spread probability, scaled by a nonlinear easing function of the cell's local fuel capacity (low-fuel cells resist ignition, high-fuel cells catch more easily).
- **Spontaneous ignition** — every `Tree` cell has a small independent chance per frame to self-ignite (lightning-strike style events), sampled with a cheap bitmask pre-check before the full probability roll to avoid a full RNG call on every cell every frame.
- **Fire duration** — sampled from a **log-normal distribution** per ignition event, scaled by the cell's fuel capacity (more fuel → longer burn). This gives natural variance in burn times instead of a fixed duration, so fire fronts don't all extinguish in lockstep.
- **Regrowth** — `Calcined` cells transition to `Soil`, and `Soil` cells transition to `Tree`, with probabilities that grow with the number of `Tree`/`Soil` neighbors (via an exponential neighbor-density term) and caled by fuel capacity — regrowth spreads outward from existing vegetation rather than reappearing randomly.
- **Terrain** — generated once from elevation noise thresholded into `Water` / `Tree` / `Rock` bands, with a separate fuel-capacity noise layer modulating fire and regrowth behavior independently of terrain type.

## Optimizations

Key techniques:

- **Parallelized update loop** — the grid is partitioned into row ranges and updated concurrently via `Parallel.ForEach` with a range partitioner, rather than per-cell parallel dispatch, to keep scheduling overhead low relative to per-cell work.
- **Double buffering** — the simulation reads from one grid buffer and writes to a second, then swaps, avoiding in-place mutation hazards under parallel updates.
- **Branch-based edge wraparound** — neighbor lookups near grid edges use conditional add/subtract instead of modulo, since offsets are always ±1; this avoids the cost of integer division on every neighbor check across the whole grid, every frame.
- **Spatial chunk activation** — the grid is divided into fixed-size chunks. A chunk (and its immediate neighbors, to catch fire spreading across chunk borders) is only marked "active" if it contains a burning cell. Cells outside any active chunk skip the expensive neighbor-fire-check and RNG calls entirely, only running the cheap spontaneous-ignition and regrowth checks. This cuts idle/sparse-fire frame cost significantly, at a small, bounded cost when most of the grid is active.
- **CPU color buffer + single GPU texture upload** — Rendering doesn't draw per-cell primitives. Each cell writes its color into a flat Color buffer sized to the grid, which is only touched when a cell's type changes (or it's actively burning). Once per frame, that whole buffer is pushed to a single GPU texture, which is then drawn scaled to the screen with one draw call. This makes rendering cost stay flat and small (low single-digit ms) regardless of grid size.
- **Thread-safe shared RNG** — all random sampling goes through `Random.Shared`, avoiding both lock contention and the correctness risk of a single non-thread-safe RNG instance under parallel updates.

## Tech stack
 
- **C# / .NET** — simulation core and app logic
- **[Raylib-cs](https://github.com/ChrisDill/Raylib-cs)** — window, input, and GPU texture rendering
- **[ImGui.NET](https://github.com/ImGuiNET/ImGui.NET)** + **[rlImgui-cs](https://github.com/raylib-extras/rlImGui-cs)** — UI for the parameter and options panels.
- **[ScottPlot](https://scottplot.net/)** + **SkiaSharp** — real-time Fire Count plotting with fast rendering

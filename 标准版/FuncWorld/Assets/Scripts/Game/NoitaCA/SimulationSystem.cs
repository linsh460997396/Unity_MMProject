// 职责：兼容旧版 SimulationSystem API，并把调用转发到新的 PixelSimulation。
// Responsibility: Preserves the legacy SimulationSystem API and forwards calls to PixelSimulation.
using System;

namespace NoitaCA
{
    [Obsolete("Use PixelSimulation. This wrapper keeps older bootstrap code compiling.")]
    public sealed class SimulationSystem
    {
        // 新模拟器是真正实现；本类只保留旧字段和旧入口。
        // The new simulator owns the implementation; this class keeps old fields and entry points.
        private readonly PixelSimulation simulation;

        public float SideFlowProbability { get; set; } = 0.45f;
        public float SplashProbability { get; set; } = 0.55f;
        public int SplashFallThreshold { get; set; } = 5;
        public float PressureFlowProbability { get; set; } = 0.9f;
        public int PressureFlowMaxDistance { get; set; } = 6;

        public SimulationSystem(int seed = 0)
        {
            // 旧构造参数继续作为新模拟器 seed 使用。
            // The legacy constructor parameter continues to seed the new simulator.
            simulation = new PixelSimulation(seed);
        }

        public void Step(PixelGrid grid)
        {
            // 旧 API 的 Step 直接映射到新模拟器的 Step。
            // The legacy Step API maps directly to the new simulator Step.
            simulation.Step(grid);
        }
    }
}

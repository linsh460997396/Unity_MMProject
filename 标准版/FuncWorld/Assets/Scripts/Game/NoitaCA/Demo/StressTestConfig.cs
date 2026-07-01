// 职责：保存压力测试场景的可调参数，供 Inspector 和运行时代码共同使用。
// Responsibility: Stores tunable stress-test scene parameters for both the Inspector and runtime code.
using System;
using UnityEngine;

namespace NoitaCA
{
    [Serializable]
    public sealed class StressTestConfig
    {
        // 网格规模与显示比例直接决定压力测试工作量。
        // Grid size and display scale directly determine the stress-test workload.
        [Header("Grid")]
        public int gridWidth = 384;
        public int gridHeight = 216;
        public int pixelsPerUnit = 16;

        [Header("Water Tank")]
        // 水箱参数控制初始水量和自动开闸节奏。
        // Water-tank values control initial water volume and automatic gate timing.
        public int initialWaterWidth = 112;
        public int initialWaterHeight = 78;
        public bool autoStart;
        public float gateOpenDelay = 1.25f;

        [Header("Simulation Budget")]
        [Tooltip("0 means unlimited. Use a smaller value to demonstrate visible budget throttling.")]
        // 处理预算可用于演示降级和帧耗限制。
        // Processing budget can demonstrate throttling and frame-cost limits.
        public int maxSimulatedPixelsPerStep;
        public int simulationStepsPerFrame = 1;
        public float simulationStepInterval = 0.016f;

        [Header("Optimization Demo")]
        // 这些开关用于对比全图扫描、活跃像素和区块更新。
        // These toggles compare full scan, active pixels, and chunk updates.
        public PixelSimulationMode initialMode = PixelSimulationMode.FullScan;
        public bool enableChunkUpdates = true;
        public bool enableActiveRegionOptimization = true;
        public int chunkSize = 16;
        public int chunkSleepDelay = 2;

        [Header("Debug Display")]
        // 调试显示项只影响可视化，不改变模拟结果。
        // Debug display options affect visualization only, not simulation results.
        public bool showPerformancePanel = true;
        public bool showChunkBoundaries;
        public bool showActivePixelCount = true;
        public bool showActiveRegions;

        [Header("Camera")]
        // 相机边距让世界边缘和覆盖层更容易观察。
        // Camera padding makes world edges and overlays easier to inspect.
        public float cameraPadding = 0.65f;
    }
}

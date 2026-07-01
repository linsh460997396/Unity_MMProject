// 职责：定义像素世界中可用的材料编号，供模拟、渲染和输入统一引用。
// Responsibility: Defines shared material identifiers used by simulation, rendering, and input.
namespace NoitaCA
{
    public enum MaterialType
    {
        // 空气代表空格子；Empty 保持旧命名兼容。
        // Air is the empty cell; Empty preserves legacy naming compatibility.
        Air = 0,
        Empty = Air,
        // 可移动或可交互材料。
        // Materials that can move or participate in interactions.
        Sand = 1,
        Water = 2,
        Smoke = 3,
        Fire = 4,
        // 固体和燃烧后的残留材料。
        // Solid materials and post-burn residue.
        Stone = 5,
        Wood = 6,
        Ash = 7
    }
}

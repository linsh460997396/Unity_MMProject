using Vector2F = UnityEngine.Vector2;

namespace MetalMaxSystem
{
    /// <summary>
    /// 技能命令。一个技能可以有多个命令，本类代表了具体命令
    /// </summary>
    public class Abilcmd
    {
        public int Index { get; set; }
        public string Link { get; set; }
        public Unit TargetUnit { get; set; }
        private Vector2F _targetVector;
        public Vector2F TargetVector
        {
            get
            {
                return _targetVector;
            }

            set
            {
                _targetVector = value;
            }
        }

        /// <summary>
        /// 目标类型：0=无，1=单位，2=二维点，3=物品
        /// </summary>
        public int TargetType { get; set; }

        public Abilcmd(string abilLink, int abilIndex)
        {
            Link = abilLink;
            Index = abilIndex;
        }
    }
}

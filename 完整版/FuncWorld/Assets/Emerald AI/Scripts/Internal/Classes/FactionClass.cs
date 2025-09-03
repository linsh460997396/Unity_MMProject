namespace EmeraldAI
{
    /// <summary>
    /// 派系类.管理和存储派系(Faction)及其关系类型的信息
    /// </summary>
    [System.Serializable]
    public class FactionClass
    {
        /// <summary>
        /// 派系索引
        /// </summary>
        public int FactionIndex;
        /// <summary>
        /// 关系类型
        /// </summary>
        public RelationTypes RelationType;
        /// <summary>
        /// 通过构造函数创建派系类.管理和存储派系(Faction)及其关系类型的信息
        /// </summary>
        /// <param name="m_FactionIndex"></param>
        /// <param name="m_RelationType"></param>
        public FactionClass(int m_FactionIndex, int m_RelationType)
        {
            FactionIndex = m_FactionIndex;
            RelationType = (RelationTypes)m_RelationType;
        }
    }
}

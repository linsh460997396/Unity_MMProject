namespace MetalMaxSystem
{
    /// <summary>
    /// 单位组
    /// </summary>
    public class UnitGroup
    {
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = "UnitGroup_" + value;
            }
        }

        public UnitGroup(string name)
        {
            Name = name;
        }

        public void Add(Unit unit)
        {
            MMCore.HD_AddObjectToGroup(unit, Name);
        }

        public void AddGroup(UnitGroup unitGroup)
        {
            MMCore.HD_AddObjectGToObjectG(unitGroup.Name, Name);
        }

        public void Del(Unit unit)
        {
            MMCore.HD_RemoveObject(unit, Name);
        }

        public void DelAll()
        {
            MMCore.HD_RemoveObjectGAll(Name);
        }

        public int Count()
        {
            return MMCore.HD_ReturnObjectNumMax(Name);
        }

        public bool IsUnitInGroup(Unit unit)
        {
            int ae = MMCore.HD_ReturnObjectNumMax(Name);
            int va = 1, ai = 1;
            for (; (ai >= 0 && va <= ae) || (ai < 0 && va >= ae); va += ai)
            {
                //直接对比单位也是可以的，但以防从Object转换出来的Unit是新的实例，最好给字典追加一个单位类型
                //这里采用标签句柄对比：每个单位组元素序号必然对应一个已注册单位标签，且注册序号从1开始，若单位的标签句柄不存在（为0）不会匹配到
                if (MMCore.HD_ReturnObjectTagFromRegNumStr(va, Name) == MMCore.HD_ReturnObjectTagStr(unit))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 玩家闲置单位
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="workerOnly">true=仅输出闲置工人</param>
        /// <returns></returns>
        public static UnitGroup Idle(int player, bool workerOnly)
        {
            UnitGroup idle = new UnitGroup("Idle_" + workerOnly.ToString() + "_" + player.ToString());
            return idle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inGroup"></param>
        /// <param name="inIndex"></param>
        /// <returns></returns>
        public static Unit UnitFromEnd(UnitGroup inGroup, int inIndex)
        {
            int ae = MMCore.HD_ReturnObjectNumMax(inGroup.Name);
            if (inIndex > 0 && inIndex <= ae)
            {
                return (Unit)MMCore.HD_ReturnObjectFromRegNum(inIndex, inGroup.Name);
            }
            return null;

        }

    }
}

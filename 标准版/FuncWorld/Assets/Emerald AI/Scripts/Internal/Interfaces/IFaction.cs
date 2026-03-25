using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 派系
/// </summary>
public interface IFaction
{
    /// <summary>
    /// Returns the object's faction index (based on the Faction Data's Faction Name List).
    /// 返回对象的阵营索引(基于阵营数据的阵营名称列表)
    /// </summary>
    int GetFaction();
}

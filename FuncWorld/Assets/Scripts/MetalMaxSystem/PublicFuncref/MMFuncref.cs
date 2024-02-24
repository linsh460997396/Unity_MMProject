using System;
using UnityEngine;

namespace MetalMaxSystem
{
    #region 公用委托类型

    //委托是引用类型

    //个人书写习惯↓
    //声明的委托类型首字母大写
    //结尾Funcref 表示无事件event记号的常规委托类型（不安全使用）
    //结尾Handler 表示有事件event记号的事件委托类型（可安全使用）

    /// <summary>
    /// 【MM_函数库】键鼠常规函数引用（委托类型），特征：void KeyMouseEventFuncref(bool ifKeyDown, int player)
    /// </summary>
    /// <param name="ifKeyDown"></param>
    /// <param name="player"></param>
    public delegate void KeyMouseEventFuncref(bool ifKeyDown, int player);

    /// <summary>
    /// 【MM_函数库】主副循环入口常规函数引用（委托类型），特征：void EntryEventFuncref()
    /// </summary>
    public delegate void EntryEventFuncref();

    /// <summary>
    /// 【MM_函数库】计时器事件函数引用（委托类型），特征：void TimerEventHandler(object sender, EventArgs e)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void TimerEventHandler(object sender, EventArgs e);

    /// <summary>
    /// 【MM_函数库】子函数动作集合常规函数引用（委托类型），特征：void SubActionEventFuncref(int lp_var)
    /// </summary>
    public delegate void SubActionEventFuncref(object sender);

    #region 监听服务预制委托

    /// <summary>
    /// 【MM_函数库】监听服务键盘按键常规函数引用（委托类型），特征：bool KeyDownEventFuncref(int player, int key)
    /// </summary>
    /// <param name="player"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public delegate bool KeyDownEventFuncref(int player, int key);
    /// <summary>
    /// 【MM_函数库】监听服务键盘双击常规函数引用（委托类型），特征：bool KeyDoubleClickEventFuncref(int player, int key)
    /// </summary>
    /// <param name="player"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public delegate bool KeyDoubleClickEventFuncref(int player, int key);
    /// <summary>
    /// 【MM_函数库】监听服务键盘弹起常规函数引用（委托类型），特征：bool KeyUpEventFuncref(int player, int key)
    /// </summary>
    /// <param name="player"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public delegate bool KeyUpEventFuncref(int player, int key);
    /// <summary>
    /// 【MM_函数库】监听服务鼠标移动常规函数引用（委托类型），特征：void MouseMoveEventFuncref(int player, Vector3 lp_mouseVector3, int uiX, int uiY)
    /// </summary>
    /// <param name="player"></param>
    /// <param name="lp_mouseVector3"></param>
    /// <param name="uiX"></param>
    /// <param name="uiY"></param>
    public delegate void MouseMoveEventFuncref(int player, Vector3 lp_mouseVector3, int uiX, int uiY);
    /// <summary>
    /// 【MM_函数库】监听服务鼠标按下常规函数引用（委托类型），特征：bool MouseDownEventFuncref(int player, int key, Vector3 lp_mouseVector3, int uiX, int uiY)
    /// </summary>
    /// <param name="player"></param>
    /// <param name="key"></param>
    /// <param name="lp_mouseVector3"></param>
    /// <param name="uiX"></param>
    /// <param name="uiY"></param>
    /// <returns></returns>
    public delegate bool MouseDownEventFuncref(int player, int key, Vector3 lp_mouseVector3, int uiX, int uiY);
    /// <summary>
    /// 【MM_函数库】监听服务鼠标左键双击常规函数引用（委托类型），特征：bool MouseLDoubleClickEventFuncref(int player, Vector3 lp_mouseVector3, int uiX, int uiY)
    /// </summary>
    /// <param name="player"></param>
    /// <param name="lp_mouseVector3"></param>
    /// <param name="uiX"></param>
    /// <param name="uiY"></param>
    /// <returns></returns>
    public delegate bool MouseLDoubleClickEventFuncref(int player, Vector3 lp_mouseVector3, int uiX, int uiY);
    /// <summary>
    /// 【MM_函数库】监听服务鼠标右键双击常规函数引用（委托类型），特征：bool MouseRDoubleClickEventFuncref(int player, Vector3 lp_mouseVector3, int uiX, int uiY)
    /// </summary>
    /// <param name="player"></param>
    /// <param name="lp_mouseVector3"></param>
    /// <param name="uiX"></param>
    /// <param name="uiY"></param>
    /// <returns></returns>
    public delegate bool MouseRDoubleClickEventFuncref(int player, Vector3 lp_mouseVector3, int uiX, int uiY);
    /// <summary>
    /// 【MM_函数库】监听服务鼠标弹起常规函数引用（委托类型），特征：bool MouseUpEventFuncref(int player, int key, Vector3 lp_mouseVector3, int uiX, int uiY)
    /// </summary>
    /// <param name="player"></param>
    /// <param name="key"></param>
    /// <param name="lp_mouseVector3"></param>
    /// <param name="uiX"></param>
    /// <param name="uiY"></param>
    /// <returns></returns>
    public delegate bool MouseUpEventFuncref(int player, int key, Vector3 lp_mouseVector3, int uiX, int uiY);

    #endregion

    #endregion
}

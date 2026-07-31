//#define UNITY_STANDALONE //BepInEx制作UnityMOD时可手动启用
#if UNITY_EDITOR || UNITY_STANDALONE

using System.Collections;
using UnityEngine;

namespace MetalMaxSystem.Unity
{
    public static class UTime
    {
        // 预定义缓存 (保持原有定义)
        public static readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        public static readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
        public static readonly WaitForSeconds waitForSeconds0_0625 = new WaitForSeconds(0.0625f);
        public static readonly WaitForSeconds waitForSeconds0_125 = new WaitForSeconds(0.125f);
        public static readonly WaitForSeconds waitForSeconds0_25 = new WaitForSeconds(0.25f);
        public static readonly WaitForSeconds waitForSeconds0_5 = new WaitForSeconds(0.5f);
        public static readonly WaitForSeconds waitForSeconds1 = new WaitForSeconds(1f);

        /// <summary>
        /// 等待指定时间.利用MainThreadDispatcher确保在主线程执行.
        /// </summary>
        /// <param name="waitSeconds">等待时间</param>
        public static void Wait(float waitSeconds)
        {
            MainThreadDispatcher.Call(IWait(waitSeconds));
        }

        /// <summary>
        /// 自定义等待时间,根据等待时间返回不同的缓存对象或自定义IEnumerator.
        /// </summary>
        /// <param name="waitSeconds">等待时间</param>
        /// <returns></returns>
        private static IEnumerator IWait(float waitSeconds)
        {
            yield return UTime.GetWaitInstruction(waitSeconds);
        }

        /// <summary>
        /// 获取等待指令,根据等待时间返回不同的缓存对象或自定义IEnumerator.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns>
        /// 由于C# 类型系统限制,这里返回object或在调用处进行处理.
        /// 注意:若返回的是IEnumerator请在协程中直接yield return GetWaitInstruction(t);
        /// 若返回的是YieldInstruction,同样yield return即可.
        /// </returns>
        private static object GetWaitInstruction(float seconds)
        {
            // 获取 Yield 指令
            // 注意:如果返回的是 IEnumerator,请在协程中直接 yield return GetWaitInstruction(t);
            // 如果返回的是 YieldInstruction,同样 yield return 即可
            // 由于 C# 类型系统限制,这里返回 object 或在调用处处理

            if (seconds <= 0f) return null;

            if (seconds <= 0.0625f) return waitForSeconds0_0625;
            if (seconds <= 0.125f) return waitForSeconds0_125;
            if (seconds <= 0.25f) return waitForSeconds0_25;
            if (seconds <= 0.5f) return waitForSeconds0_5;
            if (seconds <= 1f) return waitForSeconds1;

            // 大于1秒,返回自定义 IEnumerator
            return new OptimizedWaitEnumerator(seconds);
        }

        /// <summary>
        /// 自定义迭代器,利用缓存对象组合等待时间,避免频繁 new WaitForSeconds
        /// </summary>
        private class OptimizedWaitEnumerator : IEnumerator
        {
            private float _remainingTime;

            public OptimizedWaitEnumerator(float totalSeconds)
            {
                _remainingTime = totalSeconds;
            }

            public object Current
            {
                get
                {
                    // 每次 MoveNext 被调用时,决定当前 yield 返回什么
                    if (_remainingTime <= 0f) return null;

                    // 贪心算法:从最大的缓存开始匹配
                    if (_remainingTime >= 1f)
                    {
                        _remainingTime -= 1f;
                        return waitForSeconds1;
                    }
                    if (_remainingTime >= 0.5f)
                    {
                        _remainingTime -= 0.5f;
                        return waitForSeconds0_5;
                    }
                    if (_remainingTime >= 0.25f)
                    {
                        _remainingTime -= 0.25f;
                        return waitForSeconds0_25;
                    }
                    if (_remainingTime >= 0.125f)
                    {
                        _remainingTime -= 0.125f;
                        return waitForSeconds0_125;
                    }
                    if (_remainingTime >= 0.0625f)
                    {
                        _remainingTime -= 0.0625f;
                        return waitForSeconds0_0625;
                    }

                    // 剩余极小片段,直接等待一帧或忽略误差
                    // 这里选择等待一帧以消耗掉剩余微小时间,或者直接结束
                    return waitForEndOfFrame;
                }
            }

            public bool MoveNext()
            {
                // 只要还有剩余时间,就继续迭代
                // 注意:这种写法在 Unity Coroutine 中是合法的,
                // Unity 会根据 Current 返回的对象进行等待,然后再次调用 MoveNext
                return _remainingTime > 0f;
            }

            public void Reset() { }
        }
    }
}
#endif

//在协程中yield return接受object类型的返回值并根据实际类型处理
//IEnumerator MyCoroutine()
//{
//    // 正确:yield return 可以处理 object 返回的 YieldInstruction 或 IEnumerator
//    yield return WaitUtils.GetWaitInstruction(2.75f);
//}
//或以下方式
//StartCoroutine(WaitWithCustomTime(3.25f));
//等待后做某事
//StartCoroutine(WaitAndDoSomething(5f, () =>
//{
//    transform.position = Vector3.zero;
//}));
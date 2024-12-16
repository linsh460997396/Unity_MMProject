#if UNITY_EDITOR|| UNITY_STANDALONE
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MetalMaxSystem.Unity
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;
        private Queue<Action> _actions = new Queue<Action>();

        public static MainThreadDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("MainThreadDispatcher");
                    _instance = obj.AddComponent<MainThreadDispatcher>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        void Update()
        {
            while (_actions.Count > 0)
            {
                var action = _actions.Dequeue();
                action?.Invoke();
            }
        }

        public void Enqueue(Action action)
        {
            if (action == null) return;

            lock (_actions)
            {
                _actions.Enqueue(action);
            }
        }
    }
}
#endif

//示范操作由主线程获取当前游戏对象上的MainThreadDispatcher组件，并进行所需动作
//private void TodoOnMainThread(Sprite sprite)
//{
//    //可在另一个线程或者异步操作中调用
//    MainThreadDispatcher.Instance.Enqueue(() =>
//    {
//        var c = GetComponent<MainThreadDispatcher>();
//        if (c != null)
//        {
//            //涉及主线程对象的操作
//        }
//    });
//}

// UnityMainThreadDispatcher允许在Unity的任何线程或异步操作中安全地向主线程发送操作。这是非常重要的，因为Unity的很多API（特别是与UI和游戏对象相关的API）都必须在主线程上调用。

// 成员变量
// _instance: 这是一个静态的UnityMainThreadDispatcher实例，用于实现单例模式。单例模式确保整个应用程序中只有一个UnityMainThreadDispatcher实例。
// _actions: 这是一个Action类型的队列，用于存储需要在主线程上执行的操作。Action是一个代表没有参数且不返回值的委托的类。
// 属性
// Instance: 这是一个静态属性，用于获取UnityMainThreadDispatcher的实例。如果实例不存在，它会创建一个新的游戏对象，将UnityMainThreadDispatcher组件添加到该游戏对象上，并确保该游戏对象在加载新场景时不会被销毁。
// 方法
// Update(): 这是Unity的一个生命周期方法，每帧都会被调用。在这个方法中，它检查_actions队列是否有待执行的操作，如果有，则依次取出并执行它们。
// Enqueue(Action action): 这个方法允许你将一个操作添加到_actions队列中，以便在主线程上执行。它首先检查传入的操作是否为null，如果是，则直接返回。否则，它会锁定_actions队列（以确保线程安全），然后将操作添加到队列中。

// 方法UnityMainThreadDispatcher.Instance.Enqueue将一个操作添加到主线程的执行队列中。
// 工作原理
// 当你需要在主线程上执行某个操作时（比如更新UI或修改游戏对象的状态），你创建一个Action委托来表示这个操作。
// 你调用UnityMainThreadDispatcher.Instance.Enqueue方法，将这个Action添加到_actions队列中。
// 在每帧的Update方法中，UnityMainThreadDispatcher会检查_actions队列是否有待执行的操作。
// 如果有，它会从队列中取出操作并执行它。因为Update方法是在主线程上调用的，所以这些操作也会在主线程上执行。
// 线程安全
// Enqueue方法使用了lock语句来确保对_actions队列的访问是线程安全的。这意味着你可以从多个线程或异步操作中安全地调用Enqueue方法，而不用担心数据竞争或队列损坏。

// 使用场景
// 当你从后台线程或异步操作（如网络请求、文件读写或长时间计算）中更新UI时。
// 当你需要在特定时间点（如每帧开始时）执行一系列操作时。
// 当你想要确保某些操作在Unity的生命周期方法（如Update、FixedUpdate等）之间执行时。

// 队列用法
// Clear(): 清空队列中的所有元素。
// Contains(T item): 检查队列是否包含指定的元素。
// CopyTo(T[] array, int arrayIndex): 将队列中的元素复制到指定的数组中，从指定的数组索引开始。
// Dequeue(): 移除并返回队列的第一个元素。如果队列为空，则抛出异常。
// Enqueue(T item): 将指定的元素添加到队列的末尾。
// GetEnumerator(): 返回一个用于枚举队列中元素的枚举器。
// Peek(): 返回队列的第一个元素，但不移除它。如果队列为空，则抛出异常。
// ToArray(): 将队列中的元素复制到一个新数组中。
// TrimExcess(): 设置队列的容量为其当前大小，释放多余的存储空间。
// TryDequeue(out T result): 尝试移除并返回队列的第一个元素，如果队列为空，则返回 false 并将 result 设置为默认值。
// TryPeek(out T result): 尝试返回队列的第一个元素但不移除它，如果队列为空，则返回 false 并将 result 设置为默认值。
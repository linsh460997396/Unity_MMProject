//关于单例模式和反射
//MonoBehaviour类是由Unity引擎管理的特定行为，你不能在类外部通过new或反射来创建MonoBehaviour的实例，但是对于普通的C#类，私有构造函数虽然会阻止使用new关键字，但反射则可能绕过这一限制（取决于权限和上下文）

//Unity游戏物体出现在场景，挂载脚本会实例化里面的类（规定1个cs文件只能一个类，内部采用反射方式绕开私有构造函数来创建实例，可设计单例模式确保类的实例唯一（在其他类中new形成报错））
//其中Awake这种Unity生命周期函数会自动执行，字段如属于本类类型再去声明EngineInstance = this; //这里this关键字引用了当前类的一个实例，但它不能用在静态字段的初始化中所以写在Awake
//在C#中，new这个动作是通过类的显示声明的公有构造函数来进行实例创建，自定义类做单例模式设计时，需类里声明一个私有构造函数并且不能有公有构造函数，这样无法在其他类通过new的方式实例化

//反射绕开了私有构造函数并检查其他构造函数是否可用（包括默认无参那个隐式的），如果有任何公有构造函数就自动选择其一，来完成实例化，但过程是自动的并遵循以下顺序：
//1）公共无参构造函数（自定义类没声明时，默认采用与类相同访问修饰符）
//2）公共带有一个参数的构造函数
//3）受保护的带有一个参数的构造函数（如果你调用 Activator.CreateInstance 并传递 false 作为第二个参数，那么此构造函数会被忽略）
//如果有两个公共构造函数，且没有指定要使用的特定构造函数（通过传递一个参数数组来指定），那么默认情况下会调用无参构造函数
//如果有多个无参构造函数，则必须指定要使用的那一个，否则编译器会报错，因为它不知道应该使用哪一个

//Activator.CreateInstance 方法的设计初衷是为了简化对象实例化的过程，它不会直接告诉你调用了哪个构造函数
//如果你需要这种级别的控制，最好直接使用 ConstructorInfo.Invoke 方法，范例如下
//Type type = typeof(YourType);
//// 获取所有公共构造函数
//ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
//// 遍历所有公共构造函数，找到匹配的并调用它
//foreach (ConstructorInfo constructor in constructors)
//{
//    ParameterInfo[] parameters = constructor.GetParameters();
//    if (parameters.Length == 0) // 无参构造函数
//    {
//        object instance = constructor.Invoke(null);
//        Debug.WriteLine("无参构造函数被调用");
//        break;
//    }
//    else
//    {
//        // 检查参数是否匹配，并调用相应的构造函数
//    }
//}

//1）下方范例阻止了在其他类中使用new但并未阻止反射
//public class Engine
//{
//    public static Engine EngineInstance;

//    static Engine()
//    {
//        EngineInstance = new Engine();
//    }
//}

//2）下方范例阻止了在其他类中使用new并阻止了多次反射
//public class Engine
//{
//    public static Engine EngineInstance;

//    private Engine() { } //对无参构造函数声明私有，否则反射可任意使用

//    static Engine()
//    {
//        EngineInstance = new Engine(); //静态构造函数会在加载到内存时进行动作调用，在模板加载阶段就完成实例化，就会有一个实例，结合上方对默认无参构造函数的私有声明，可防止反射
//    }
//}

//3）在Unity中所有游戏逻辑应在一个MonoBehaviour类中处理，这意味应将这个Engine类作为一个MonoBehaviour类来实现而不是一个普通的C#类，但下方范例依然有些问题
//public class Engine : MonoBehaviour
//{
//    public static Engine EngineInstance;

//    static Engine()
//    {
//        EngineInstance = new Engine();
//    }
//}
//首先Engine类继承MonoBehaviour后，静态构造函数写EngineInstance = new Engine(); 在语法上是没有问题的，但是在实际使用中可能会有一些限制和注意事项
//因为MonoBehaviour是Unity引擎中的一个基类，它有一些特殊的行为和生命周期方法。例如，当一个MonoBehaviour被添加到游戏对象时，Unity会自动调用它的构造函数
//在这种情况下，手动调用new Engine()可能不会按照预期工作，因为Unity已经为你创建了一个实例
//其次如要在其他脚本中使用EngineInstance，需确保在使用前Engine脚本已被加载到场景，否则EngineInstance将为null将导致程序崩溃
//实际使用中的常规自定义类建议参考范例2），通过私有化无参构造函数和静态构造函数来实现单例模式，避免与Unity引擎特殊行为类型冲突同时也保证只有一个实例，提高代码的封装性和安全性

//4）为了解决多线程问题，我们需要确保只有一个Engine实例，并且在多个线程之间共享它，可用以下方法：
// 使用System.Threading.Lock()方法来确保只有一个线程进入关键区域
// 使用System.Threading.Interlocked.CompareExchange()方法来确保只有一个线程创建Engine实例
// using System;
// using System.Threading;
// public class Engine
// {
//     private static Engine instance;
//     private static object lockObject = new object();
//     private Engine() { } // 对无参构造函数声明私有，否则反射可任意使用
//     public static Engine Instance
//     {
//         get
//         {
//             if (instance == null)
//             {
//                 lock (lockObject)
//                 {
//                     if (instance == null)
//                     {
//                         instance = new Engine();
//                     }
//                 }
//             }
//             return instance;
//         }
//     }
// }
//示例使用名为lockObject的私有对象来确保只有一个线程访问instance变量，还使用了instance == null检查来确保只有一个线程创建Engine实例
//这方法被称为“双检查锁”（double-checked locking），它帮助减少锁的竞争并提高多线程性能
//请注意示例仍允许反射创建额外的Engine实例，如想完全禁止反射请使用Type.IsAbstract和Type.GetConstructors()方法来检查类是否为抽象类及是否有公共构造函数
//.NET框架从4.0版本开始提供了更简洁且线程安全的Lazy<T>类型，它内部实现了双重检查锁定和其他必要的同步机制，因此你可以使用Lazy<T>来简化代码：
//public class Engine
//{
//    private Engine()
//    {
//        // 私有构造函数，防止在类外部使用 new 关键字创建实例
//    }
//    public static Lazy<Engine> EngineInstance = new Lazy<Engine>(() => new Engine());
//    //如需要1个属性而不是字段，可添加以下属性
//    public static Engine Instance => EngineInstance.Value;
//}
//在这个使用Lazy<T> 的示例中，EngineInstance是一个静态的Lazy<Engine>类型的字段，它会在第一次访问其Value属性时创建Engine的实例，并且由于是静态的，这个实例会在整个应用程序域中共享
//由于Lazy<T>的内部实现已经确保了线程安全，因此不需要额外的锁定机制，使用Lazy<T>的好处是代码更简洁且不需要担心线程安全问题，因为.NET框架已处理这些细节

//5）在C#中，单例模式（Singleton Pattern）通常有两种主要实现方式：懒汉式（Lazy Initialization）和饿汉式（Eager Initialization）

// 饿汉式（Eager Initialization）
// 饿汉式单例模式的特点是类加载时就完成了初始化，所以类加载比较慢，但获取对象的速度快。在C#中，由于静态构造函数的存在，饿汉式单例模式通常如下所示：
// public class Singleton
// {
//     // 静态变量，持有单例的唯一实例
//     private static Singleton _instance = new Singleton();
//     // 私有构造函数，防止外部通过new创建实例
//     private Singleton()
//     {
//         // 初始化代码
//     }
//     // 静态属性，提供全局访问点
//     public static Singleton Instance
//     {
//         get { return _instance; }
//     }
//     // 其他成员...
// }
// 由于静态构造函数会在类型第一次引用之前由.NET运行时自动调用，因此_instance会在类型加载时就被初始化，这是饿汉式名称的由来

// 懒汉式（Lazy Initialization）
// 懒汉式单例模式的特点是类加载速度快，但获取对象的速度慢，因为单例实例是在第一次使用时才进行初始化。在C#中，你可以使用Lazy<T>类来实现线程安全的懒汉式单例：
// using System.Lazy;
// public class Singleton
// {
//     // 使用Lazy<T>确保线程安全的初始化
//     private static readonly Lazy<Singleton> _lazyInstance = new Lazy<Singleton>(() => new Singleton());
//     // 私有构造函数，防止外部通过new创建实例
//     private Singleton()
//     {
//         // 初始化代码
//     }
//     // 静态属性，提供全局访问点
//     public static Singleton Instance
//     {
//         get { return _lazyInstance.Value; }
//     }
//     // 其他成员...
// }
// Lazy<T>构造函数中的lambda表达式定义了单例实例的创建逻辑，并且Lazy<T>会确保该逻辑只执行一次，即使在多线程环境下也是如此
// 除了Lazy<T>，你也可以手动实现线程安全的懒汉式单例，如下所示：
// public class Singleton
// {
//     // 静态变量，持有单例的唯一实例
//     private static Singleton _instance;
//     // 静态锁对象，用于同步
//     private static readonly object _lock = new object();
//     // 私有构造函数，防止外部通过new创建实例
//     private Singleton()
//     {
//         // 初始化代码
//     }
//     // 静态属性，提供全局访问点
//     public static Singleton Instance
//     {
//         get
//         {
//             // 如果实例未初始化，则加锁并检查
//             if (_instance == null)
//             {
//                 lock (_lock)
//                 {
//                     // 再次检查实例是否已初始化，防止在锁定后其他线程已初始化
//                     if (_instance == null)
//                     {
//                         _instance = new Singleton();
//                     }
//                 }
//             }
//             return _instance;
//         }
//     }
//     // 其他成员...
// }
// 这种手动实现的方式称为双重检查锁定（Double-Checked Locking），在.NET Framework 2.0及更高版本中，由于编译器和运行时环境的改进，双重检查锁定是安全有效的。不过，在.NET Core和.NET 5+中，直接使用Lazy<T>通常更为简洁和可靠。

//6）C# 中的 volatile 关键字用于修饰变量，它确保对这个变量的修改对于所有的线程都是可见的。
//换句话说，当一个变量被声明为 volatile 时，编译器不会将对该变量的读写操作进行优化，每次访问都会直接从内存中读取或写入。
// 下面是一个例子，演示了 volatile 的作用
// using System;
// using System.Threading;
// class Program
// {
//     static volatile int counter = 0;

//     static void Main(string[] args)
//     {
//         Thread t1 = new Thread(IncrementCounter);
//         Thread t2 = new Thread(IncrementCounter);
//         Thread t3 = new Thread(IncrementCounter);

//         t1.Start();
//         t2.Start();
//         t3.Start();

//         t1.Join();
//         t2.Join();
//         t3.Join();

//         Console.WriteLine("Counter value: " + counter);
//     }

//     static void IncrementCounter()
//     {
//         for (int i = 0; i < 100000; i++)
//         {
//             counter++;
//         }
//     }
// }
// 在这个例子中，我们创建了一个名为 `counter` 的全局变量，并将其声明为 volatile。我们启动三个线程，每个线程执行一个循环，每次循环都将 `counter` 值递增
// 由于 `counter` 被声明为 volatile，因此在每个线程中对 `counter` 的修改都是可见的。当所有线程执行完毕后，我们将 `counter` 的值打印到控制台结果是300000
// 如果没有使用 volatile，可能会出现以下情况：
// - 编译器可能会缓存 `counter` 的值，导致在多个线程中对它的修改无法立即反映出来
// - 由于编译器优化，对 `counter` 的递增操作可能不会按照预期的顺序执行，这可能导致最终的 `counter` 值不正确
// 通过使用 volatile，我们可以确保对 `counter` 的修改对于所有线程都是可见的，并且每次访问都会直接从内存中读取或写入，避免了这些问题

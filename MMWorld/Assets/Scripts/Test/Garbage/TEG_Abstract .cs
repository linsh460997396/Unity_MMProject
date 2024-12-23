//using System;

//namespace Test.Example.Garbage
//{
//// 定义一个抽象类
//public abstract class Animal
//{
//    // 抽象方法，没有方法体，子类必须实现
//    public abstract void MakeSound();

//    // 具体方法，有方法体，子类可以直接调用或重写
//    public void Eat()
//    {
//        Console.WriteLine("This animal is eating.");
//    }

//    // 构造函数，用于初始化抽象类中的成员变量
//    protected Animal(string name)
//    {
//        this.Name = name;
//    }

//    // 成员变量
//    protected string Name { get; private set; }
//}

//// 定义一个子类继承自抽象类
//public class Dog : Animal
//{
//    // 实现抽象方法
//    public override void MakeSound()
//    {
//        Console.WriteLine("Woof!");
//    }

//    // 子类自己的构造函数，可以调用基类的构造函数
//    public Dog(string name) : base(name)
//    {
//    }
//}

//// 使用子类
//public class Program
//{
//    public static void Main(string[] args)
//    {
//        Dog dog = new Dog("Buddy");
//        dog.MakeSound(); // 输出: Woof!
//        dog.Eat();       // 输出: This animal is eating.
//    }
//}

//}
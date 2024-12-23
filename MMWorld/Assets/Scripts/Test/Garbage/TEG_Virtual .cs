//// 定义一个抽象类
//using System;

//namespace Test.Example.Garbage
//{
//public abstract class Animal
//{
//    // 虚函数，有方法体，子类可以选择重写
//    public virtual void MakeSound()
//    {
//        Console.WriteLine("The animal makes a sound.");
//    }

//    // 抽象方法，没有方法体，子类必须实现
//    public abstract void Move();
//}

//// 定义一个子类继承自抽象类
//public class Dog : Animal
//{
//    // 重写虚函数
//    public override void MakeSound()
//    {
//        Console.WriteLine("Woof!");
//    }

//    // 实现抽象方法
//    public override void Move()
//    {
//        Console.WriteLine("The dog runs.");
//    }
//}

//// 使用子类
//public class Program
//{
//    public static void Main(string[] args)
//    {
//        Dog dog = new Dog();
//        dog.MakeSound(); // 输出: Woof!
//        dog.Move();      // 输出: The dog runs.

//        // 使用基类引用来调用虚函数和抽象方法
//        Animal animal = dog;
//        animal.MakeSound(); // 仍然输出: Woof!（因为调用的是子类的重写方法）
//        animal.Move();      // 输出: The dog runs.（因为调用的是子类的实现方法）
//    }
//}
//}

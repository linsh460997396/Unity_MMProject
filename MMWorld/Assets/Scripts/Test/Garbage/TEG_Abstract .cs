//using System;

//namespace Test.Example.Garbage
//{
//// ����һ��������
//public abstract class Animal
//{
//    // ���󷽷���û�з����壬�������ʵ��
//    public abstract void MakeSound();

//    // ���巽�����з����壬�������ֱ�ӵ��û���д
//    public void Eat()
//    {
//        Console.WriteLine("This animal is eating.");
//    }

//    // ���캯�������ڳ�ʼ���������еĳ�Ա����
//    protected Animal(string name)
//    {
//        this.Name = name;
//    }

//    // ��Ա����
//    protected string Name { get; private set; }
//}

//// ����һ������̳��Գ�����
//public class Dog : Animal
//{
//    // ʵ�ֳ��󷽷�
//    public override void MakeSound()
//    {
//        Console.WriteLine("Woof!");
//    }

//    // �����Լ��Ĺ��캯�������Ե��û���Ĺ��캯��
//    public Dog(string name) : base(name)
//    {
//    }
//}

//// ʹ������
//public class Program
//{
//    public static void Main(string[] args)
//    {
//        Dog dog = new Dog("Buddy");
//        dog.MakeSound(); // ���: Woof!
//        dog.Eat();       // ���: This animal is eating.
//    }
//}

//}
//// ����һ��������
//using System;

//namespace Test.Example.Garbage
//{
//public abstract class Animal
//{
//    // �麯�����з����壬�������ѡ����д
//    public virtual void MakeSound()
//    {
//        Console.WriteLine("The animal makes a sound.");
//    }

//    // ���󷽷���û�з����壬�������ʵ��
//    public abstract void Move();
//}

//// ����һ������̳��Գ�����
//public class Dog : Animal
//{
//    // ��д�麯��
//    public override void MakeSound()
//    {
//        Console.WriteLine("Woof!");
//    }

//    // ʵ�ֳ��󷽷�
//    public override void Move()
//    {
//        Console.WriteLine("The dog runs.");
//    }
//}

//// ʹ������
//public class Program
//{
//    public static void Main(string[] args)
//    {
//        Dog dog = new Dog();
//        dog.MakeSound(); // ���: Woof!
//        dog.Move();      // ���: The dog runs.

//        // ʹ�û��������������麯���ͳ��󷽷�
//        Animal animal = dog;
//        animal.MakeSound(); // ��Ȼ���: Woof!����Ϊ���õ����������д������
//        animal.Move();      // ���: The dog runs.����Ϊ���õ��������ʵ�ַ�����
//    }
//}
//}

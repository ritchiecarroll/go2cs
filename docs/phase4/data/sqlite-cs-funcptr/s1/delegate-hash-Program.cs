// Delegate.GetHashCode for two delegates of one type over two different static methods.
using System;
public static class Program
{
    static int A(int x) => x;
    static int B(int x) => -x;
    public static void Main()
    {
        Func<int, int> a = A, b = B, a2 = A;
        Console.WriteLine($"hash A {a.GetHashCode()} hash B {b.GetHashCode()} equal-hash {a.GetHashCode() == b.GetHashCode()}; A equals A' {a.Equals(a2)}; A equals B {a.Equals(b)}");
    }
}

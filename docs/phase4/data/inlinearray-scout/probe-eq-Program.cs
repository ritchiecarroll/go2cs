// Order sensitivity of the default ValueType.Equals on a holder of an inline array. Arg "dict-first" or "equals-first".
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[InlineArray(4)] public struct Arr4<T> { private T _e; }
public struct HoldsInline { public Arr4<int> A; public int N; }
public struct HoldsInlineRef { public Arr4<int> A; public string S; }

public static class Program
{
    static string Try(Func<object> f) { try { return $"returned {f()}"; } catch (Exception ex) { return ex.GetType().Name; } }
    public static void Main(string[] args)
    {
        var h1 = new HoldsInline(); var h2 = new HoldsInline(); var h3 = new HoldsInline(); h3.A[2] = 7;
        Func<object> dict = () => { var d = new Dictionary<HoldsInline, int>(); d[h1] = 1; return d.ContainsKey(h2) + "/" + d.ContainsKey(h3); };
        Func<object> eq = () => h1.Equals(h2) + "/" + h1.Equals(h3);
        if (args[0] == "dict-first") { Console.WriteLine($"dict: {Try(dict)}"); Console.WriteLine($"equals: {Try(eq)}"); Console.WriteLine($"equals again: {Try(eq)}"); }
        else { Console.WriteLine($"equals: {Try(eq)}"); Console.WriteLine($"equals again: {Try(eq)}"); Console.WriteLine($"dict: {Try(dict)}"); }
        Console.WriteLine($"hash h1/h3: {Try(() => h1.GetHashCode())} {Try(() => h3.GetHashCode())}");
        var r1 = new HoldsInlineRef { S = "x" }; var r2 = new HoldsInlineRef { S = "x" };
        Console.WriteLine($"reference-bearing holder Equals: {Try(() => r1.Equals(r2))}; again: {Try(() => r1.Equals(r2))}");
    }
}

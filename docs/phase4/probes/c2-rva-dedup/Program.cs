using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Does Roslyn store ONE copy of identical u8 literal data per module, so that every site spelling
// "abc"u8 hands out the SAME address? The module-init registration table depends on it.
static unsafe class Program
{
    internal static nint Addr(ReadOnlySpan<byte> s) => (nint)Unsafe.AsPointer(ref MemoryMarshal.GetReference(s));

    [MethodImpl(MethodImplOptions.NoInlining)] static nint SiteA() => Addr("abc"u8);
    [MethodImpl(MethodImplOptions.NoInlining)] static nint SiteB() => Addr("abc"u8);
    [MethodImpl(MethodImplOptions.NoInlining)] static nint SiteC() => Addr("abcd"u8);         // a longer literal with the same prefix
    [MethodImpl(MethodImplOptions.NoInlining)] static nint SiteD() => Addr("xabc"u8[1..]);     // a suffix view of another literal
    [MethodImpl(MethodImplOptions.NoInlining)] static nint SiteE() => Addr("n"u8);
    [MethodImpl(MethodImplOptions.NoInlining)] static nint SiteF() => Addr("n"u8);
    [MethodImpl(MethodImplOptions.NoInlining)] static nint SiteG() => Addr("0123456789abcdef0123456789abcdef"u8);
    [MethodImpl(MethodImplOptions.NoInlining)] static nint SiteH() => Addr("0123456789abcdef0123456789abcdef"u8);
    static class Nested { [MethodImpl(MethodImplOptions.NoInlining)] public static nint Site() => Addr("abc"u8); }
    static class Generic<T> { [MethodImpl(MethodImplOptions.NoInlining)] public static nint Site() => Addr("abc"u8); }

    static void Main()
    {
        Console.WriteLine($"abc  A==B {SiteA() == SiteB()}  A==Nested {SiteA() == Nested.Site()}  A==Generic<int> {SiteA() == Generic<int>.Site()}  A==Generic<string> {SiteA() == Generic<string>.Site()}");
        Console.WriteLine($"abc vs abcd same start {SiteA() == SiteC()}   abc vs \"xabc\"[1..] same {SiteA() == SiteD()}");
        Console.WriteLine($"n    E==F {SiteE() == SiteF()}");
        Console.WriteLine($"32B  G==H {SiteG() == SiteH()}");
        Console.WriteLine($"stable across calls {SiteA() == SiteA()}");
        Console.WriteLine($"other file  abc {SiteA() == OtherFile.Abc()}  n {SiteE() == OtherFile.N()}");
        Console.WriteLine($"build: {(typeof(Program).Assembly.GetCustomAttributes(typeof(System.Diagnostics.DebuggableAttribute), false).Length > 0 && ((System.Diagnostics.DebuggableAttribute)typeof(Program).Assembly.GetCustomAttributes(typeof(System.Diagnostics.DebuggableAttribute), false)[0]).IsJITOptimizerDisabled ? "Debug (optimizer off)" : "optimized")}  runtime {Environment.Version}");
        Console.WriteLine($"addresses A={SiteA():x} E={SiteE():x} G={SiteG():x}");
    }
}

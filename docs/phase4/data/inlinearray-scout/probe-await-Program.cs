// MUST FAIL TO BUILD. Each method is one face of an inline-array element across an await; the build's error list is
// the measurement (which faces C# refuses, and with which code).
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[InlineArray(4)] public struct Arr4<T> { private T _e; }
public struct P { public Arr4<int> a; }
public struct Wrapped4<T>
{
    private Arr4<T> m_storage;
    public const int Length = 4;
    [System.Diagnostics.CodeAnalysis.UnscopedRef] public ref T this[int i] => ref m_storage[i];
}

public sealed class C
{
    private P p;
    private static async Task<int> F() { await Task.Yield(); return 1; }

    // compound assignment on an element of a LOCAL's inline array, the await on the right
    public static async Task CompoundLocal() { var p = new P(); p.a[0] += await F(); }
    // the same on a FIELD of a class (a heap owner)
    public async Task CompoundField() { p.a[0] += await F(); }
    // a ref local to an element, used after an await
    public static async Task RefLocalAcross() { var p = new P(); ref int e = ref p.a[0]; await F(); e = 1; }
    // a Span over the inline array, used after an await
    public static async Task SpanAcross() { var p = new P(); Span<int> s = p.a; await F(); s[0] = 1; }
    // a plain store (control: expected to compile)
    public static async Task PlainStore() { var p = new P(); p.a[0] = await F(); }
    // an increment with the await in the index (control)
    public static async Task AwaitInIndex() { var p = new P(); p.a[await F()] += 1; }

    // the language's inline-array -> Span conversion on the [InlineArray] type itself (control: compiles)
    public static void SpanOfInline() { var p = new P(); Span<int> s = p.a; s[0] = 1; }
    // the same through a WRAPPER that exposes only an element indexer and a length (expected: refused)
    public static void SpanOfWrapper() { var w = new Wrapped4<int>(); Span<int> s = w; w[0] = 1; }

    public static void Main() { }
}

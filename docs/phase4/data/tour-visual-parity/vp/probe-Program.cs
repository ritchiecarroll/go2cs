// The visual-parity scout's probes for items 2-4, laid out as converted code is: types in namespace go, golib's builtin
// brought in by `using static go.builtin`, a package as a static partial class.
using System;
using System.Linq;
using System.Reflection;
using static go.builtin;

namespace go
{
    public readonly struct slice<T> { public readonly T[] A; public slice(T[] a) { A = a; } public int Len => A.Length; }
    public readonly struct array<T> { public readonly T[] A; public array(T[] a) { A = a; } public int Len => A.Length; public array(int n) { A = new T[n]; } }
    public static class ArrayExtensions
    {
        public static array<T> array<T>(this T[] a) => new(a);                                     // today's `.array()`
        public static array<T> array<T>(this T[] a, int length) { var r = new T[length]; a.CopyTo(r, 0); return new(r); }
    }
    public static class builtin
    {
        public static slice<T> slice<T>(T[] source) => new(source);                                 // golib builtin.cs:3881
        public static array<T> array<T>(T[] source) => new(source);                                 // item 4: proposed
        public static array<T> array<T>(int length, T[] source) { var r = new T[length]; source.CopyTo(r, 0); return new(r); }
    }

    public static partial class main_package
    {
        // item 2: Go `var c, python, java bool` at package level, as ONE declaration
        internal static bool c, python, java;
        // item 2: with values, and readonly / const
        internal static nint a = 1, b = 2;
        internal static readonly nint ra = 3, rb = 4;
        internal const nint ca = 5, cb = 6;

        static int fail;
        static void Check(string what, bool ok, string detail) { Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {what}: {detail}"); if (!ok) fail++; }

        public static int Main()
        {
            // item 4: name resolution beside the array<T> type, as slice<T> manages
            var s = slice<nint>([2, 3, 5, 7, 11, 13]);
            var primes = array<nint>([2, 3, 5, 7, 11, 13]);
            var shorter = array<nint>(6, [2, 3]);
            var old = new nint[] { 2, 3, 5, 7, 11, 13 }.array();
            Check("item 4: array<nint>([...]) binds the builtin method, not the type", primes.Len == 6 && primes.A[5] == 13, $"len {primes.Len}");
            Check("item 4: the sized form array<nint>(6, [2, 3])", shorter.Len == 6 && shorter.A[1] == 3 && shorter.A[5] == 0, string.Join(",", shorter.A));
            Check("item 4: slice<nint>([...]) unchanged beside it", s.Len == 6, $"len {s.Len}");
            Check("item 4: a fresh array per evaluation (no shared elements)", !ReferenceEquals(Make().A, Make().A), "two evaluations");
            // item 4: the IL of the new form against today's (the array creation is the same; only the called method differs)
            byte[] ilNew = typeof(main_package).GetMethod(nameof(NewForm), BindingFlags.NonPublic | BindingFlags.Static)!.GetMethodBody()!.GetILAsByteArray()!;
            byte[] ilOld = typeof(main_package).GetMethod(nameof(OldForm), BindingFlags.NonPublic | BindingFlags.Static)!.GetMethodBody()!.GetILAsByteArray()!;
            byte[] blobNew = typeof(main_package).GetMethod(nameof(NewBytes), BindingFlags.NonPublic | BindingFlags.Static)!.GetMethodBody()!.GetILAsByteArray()!;
            byte[] blobOld = typeof(main_package).GetMethod(nameof(OldBytes), BindingFlags.NonPublic | BindingFlags.Static)!.GetMethodBody()!.GetILAsByteArray()!;
            Check("item 4: nint literal IL, new vs today: same length", ilNew.Length == ilOld.Length, $"{ilNew.Length} vs {ilOld.Length} bytes");
            Check("item 4: byte literal IL (static-data blob), new vs today: same length", blobNew.Length == blobOld.Length, $"{blobNew.Length} vs {blobOld.Length} bytes");
            Check("item 4: the IL differs only in the call token (4 bytes)", ilNew.Zip(ilOld).Count(p => p.First != p.Second) <= 4, $"{ilNew.Zip(ilOld).Count(p => p.First != p.Second)} bytes differ");

            // item 2: locals
            nint x = default, y = default;
            Check("item 2: one declaration, several names (fields, readonly, const, locals)", !c && !python && !java && a + b == 3 && ra + rb == 7 && ca + cb == 11 && x + y == 0, "");

            // item 3: temp names derived from the tag
            var osᴛ1 = "linux"; var stringᴛ1 = "x"; var ΔLockᴛ1 = 1;
            Check("item 3: osᴛ1, stringᴛ1 (from @string) and ΔLockᴛ1 are valid C# names", osᴛ1.Length + stringᴛ1.Length + ΔLockᴛ1 == 7, "");
            return fail;
        }

        static array<nint> Make() => array<nint>([1, 2]);
        static array<nint> NewForm() => array<nint>([2, 3, 5, 7, 11, 13]);
        static array<nint> OldForm() => new nint[] { 2, 3, 5, 7, 11, 13 }.array();
        static array<byte> NewBytes() => array<byte>([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]);
        static array<byte> OldBytes() => new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }.array();
    }
}

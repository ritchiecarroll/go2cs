using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using reflect = go.reflect_package;

namespace GolibTests;

// REFLECT'S HASH-DERIVED POINTER TOKENS (census CENSUS-reflect-hash-tokens-go1.24.13.md, G's review
// 652745506c). Seven mints hand out an identity hash where Go hands out an address: Value.Pointer()
// of a map, a slice, a func, a channel or an unregistered object, InterfaceData's words, and the
// named-pointer and named-channel wrappers' defaults. The registry never answers a ж<T> for any of
// them, so a hash converted back to a pointer builds a NativeBox over a LOW USER ADDRESS: a fatal
// access violation, or a silent garbage read if that page is mapped. The band
// 0xC000_0000_0000_0000 | h puts every one where golib's guards refuse by name: NamesNoUserMemory
// (bit 63) and IsTaggedToken (bit 63 set, bit 47 clear).
[TestClass]
public class ReflectHashTokenBandTests
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AnyMethod() { }

    private static void AssertInBand(string what, uintptr token)
    {
        nuint number = (nuint)token;
        string reading = $"{what}: token 0x{(ulong)number:x}";

        Assert.IsTrue(ManagedPointerTokens.NamesNoUserMemory(number), $"{reading} names user memory: a dereference reads a low address");
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(number), $"{reading} is not a tagged token: memmove and the syscall doors pass it through");
    }

    [TestMethod]
    public void EveryHashDerivedReflectTokenNamesNoUserMemory()
    {
        map<@string, nint> m = new() { ["a"] = 1 };
        AssertInBand("map Pointer()", reflect.ValueOf(m).Pointer());

        slice<nint> s = make<slice<nint>>(3);
        AssertInBand("slice Pointer()", reflect.ValueOf(s).Pointer());

        Action f = AnyMethod;
        AssertInBand("func Pointer()", reflect.ValueOf(f).Pointer());

        channel<nint> c = new(1);
        AssertInBand("chan Pointer()", reflect.ValueOf(c).Pointer());

        // Go: `var x any = 5; reflect.ValueOf(&x).Elem().InterfaceData()[1]`, the data word.
        ref object word = ref heap((object)(nint)5, out ж<object> x);
        _ = word;
        AssertInBand("InterfaceData word", reflect.ValueOf(x).Elem().InterfaceData()[1]);
    }

    // UNIQUENESS (gojq's prerequisite, roadmap-post100: its allocator keys a Go map on Pointer() of a
    // map or []any to decide whether it may mutate in place). Two LIVE backings must never share a
    // token; identity hashes collide (measured: 2 among 10k live objects, 80 among 100k).
    [TestMethod]
    public void LiveMapsAndSlicesHaveDistinctPointers()
    {
        const int N = 50_000;
        object[] live = new object[2 * N];
        System.Collections.Generic.Dictionary<ulong, int> seen = new(2 * N);
        int collisions = 0;

        for (int i = 0; i < N; i++)
        {
            map<@string, nint> m = new() { ["k"] = i };
            slice<nint> s = make<slice<nint>>(1);
            live[2 * i] = m;
            live[2 * i + 1] = s;

            foreach (ulong token in new[] { (ulong)(nuint)reflect.ValueOf(m).Pointer(), (ulong)(nuint)reflect.ValueOf(s).Pointer() })
            {
                if (!seen.TryAdd(token, i))
                    collisions++;
            }
        }

        GC.KeepAlive(live);
        Assert.AreEqual(0, collisions, $"{collisions} of {2 * N} live map and slice backings shared a Pointer() token with another live one");
    }

    // AMENDMENT (b) of G's review: the band's DISJOINTNESS from ж tokens rests on the CLR's identity
    // hash width. AllocationBase packs `hash >> 15` into bits 48 and up, so hash bit 29 lands on token
    // bit 62, the band's own bit; a hash with any of bits 29..31 set would alias a ж token into the
    // band. Measured on CoreCLR x64 as 26 bits wide (the AllocationBase comment); this is the guard that
    // says so every run. UNMEASURED under Native AOT until a GolibTests AOT run reads it.
    [TestMethod]
    public void PointerTokensKeepBit62ClearSoTheHashBandIsDisjoint()
    {
        const ulong Bit62 = 1UL << 62;
        ulong seen = 0;

        for (int i = 0; i < 200_000; i++)
        {
            ж<nint> box = @new<nint>();
            seen |= (ulong)box.PointerOrderToken;
            seen |= (ulong)(uint)RuntimeHelpers.GetHashCode(new object()) << 33;
        }

        Assert.AreEqual(0UL, seen & Bit62,
            $"a pointer token (or an identity hash shifted to its token position) set bit 62: OR 0x{seen:x}. " +
            "The hash band 0xC000_0000_0000_0000 | h is no longer disjoint from ж tokens.");
    }
}

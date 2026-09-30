using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using reflect = go.reflect_package;
using @unsafe = go.unsafe_package;

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

    // THE REFUSAL ITSELF (the census's P1, after only): an identity token converted back to a pointer
    // and dereferenced is Go's nil-dereference panic, not a read of a low user address. Before the
    // band this read was a fatal access violation on windows or a silent garbage read, so there is no
    // in-process red form of this arm.
    [TestMethod]
    public void ADereferencedIdentityTokenPanicsAsANilDereference()
    {
        map<@string, nint> m = new() { ["a"] = 1 };
        uintptr token = reflect.ValueOf(m).Pointer();
        ж<long> p = (ж<long>)token;

        PanicException refusal = Assert.ThrowsException<PanicException>(() => p.Value);

        StringAssert.Contains(refusal.Message, "nil pointer dereference", $"token 0x{(ulong)(nuint)token:x}");
    }

    // A SLICE WINDOW'S TOKEN IS GO'S &s[low] ARITHMETIC: two windows of one backing differ by the element
    // size per element (G's review, the displacement claim).
    [TestMethod]
    public void ASliceWindowsTokenStepsByTheElementSize()
    {
        slice<nint> s = make<slice<nint>>(4);
        ulong whole = (ulong)(nuint)reflect.ValueOf(s).Pointer();
        ulong fromOne = (ulong)(nuint)reflect.ValueOf(s[1..]).Pointer();

        Assert.AreEqual(8UL, fromOne - whole, $"Pointer(s[1:]) - Pointer(s): 0x{fromOne:x} - 0x{whole:x}");
    }

    // unsafe.Pointer EQUALITY AGREES WITH ITS uintptr for a RESOLVED NON-BOX referent (G's review): a
    // channel's pointer resolves to the boxed channel, and its equality token must be the same number
    // uintptr(p) reads, or two live referents whose identity hashes collide compare equal while their
    // uintptrs differ. A planted collision is not constructible, so this pins the SHAPE.
    [TestMethod]
    public void AResolvedNonBoxReferentKeysEqualityOnItsOwnToken()
    {
        System.Reflection.MethodInfo referentToken = typeof(@unsafe.Pointer).GetMethod("ReferentToken",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

        channel<nint> c1 = new(1), c2 = new(1);
        var v1 = reflect.ValueOf(c1);
        var v2 = reflect.ValueOf(c2);
        uintptr u1 = v1.Pointer(), u2 = v2.Pointer();
        @unsafe.Pointer p1 = u1, p1Again = u1, p2 = u2;

        object r1 = p1.Referent ?? throw new AssertFailedException("the channel's pointer did not resolve to its referent");
        object r2 = p2.Referent ?? throw new AssertFailedException("the second channel's pointer did not resolve");

        Assert.AreEqual((nuint)u1, (nuint)referentToken.Invoke(null, [r1])!, "ReferentToken must be the number uintptr(p) reads");
        Assert.AreEqual((nuint)u2, (nuint)referentToken.Invoke(null, [r2])!, "ReferentToken must be the number uintptr(p) reads");
        Assert.IsTrue(p1.Equals(p1Again), "one referent, one number: equal");
        Assert.AreEqual(p1.GetHashCode(), p1Again.GetHashCode(), "equal pointers must hash alike");
        Assert.IsFalse(p1.Equals(p2), "two referents, two numbers: unequal");

        GC.KeepAlive(v1);
        GC.KeepAlive(v2);
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

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using Δruntime = go.runtime_package;
using fmt = go.fmt_package;
using reflect = go.reflect_package;

namespace GolibTests;

// The panic VALUE golib raises for a Go runtime error, read through recover() exactly as runtime's
// TestRuntimePanicWithRuntimeError (crash_test.go:433) reads it: `_, ok := got.(runtime.Error)`.
//
// Go raises these as runtime.plainError (nil-map write, close of a closed or nil channel, send on a
// closed channel, makechan with a negative size) and runtime.boundsError (an index out of range). golib
// sits UNDER the runtime package and cannot name either, so the runtime registers them through
// RuntimeErrorPanic's hooks (runtime/panicvalues_impl.cs), the inverted dependency the divide-by-zero
// value already uses. Sizing: docs/phase4/CENSUS-runtime-error-factories-go1.24.13.md.
//
// One arm per THROW SITE, not per case: a nil-map write has three doors (indexer, Add, Set), a send on a
// closed channel has four (the immediate and the woken send, the select poll and the woken select), and
// makechan has four (three constructors and ISupportMake). A site left raising a string reads green in
// every arm but its own.
[TestClass]
public class RuntimeErrorPanicValueTests
{
    private const int TimeoutMs = 10_000;

    // The hooks are registered by the runtime module's initializer, which a converted program runs long
    // before it can recover anything (every converted package's init touches runtime). A unit test holds
    // no such guarantee, so it is run here, once, explicitly.
    [ClassInitialize]
    public static void RunRuntimeModuleInitializer(TestContext _)
    {
        RuntimeHelpers.RunModuleConstructor(typeof(Δruntime).Module.ModuleHandle);
    }

    // ---- helpers ------------------------------------------------------------------------------

    // Runs fn inside a frame shaped as the converter emits `defer func() { recovered = recover() }()`,
    // and returns what recover() saw. A .NET exception that is not a Go panic escapes, which is itself
    // the failure (it is what makechan's ArgumentOutOfRangeException did).
    private static object? PanicValue(Action fn)
    {
        object? recovered = null;
        GoFrame frame = default;

        try
        {
            frame.Push(() => recovered = recover());
            fn();
        }
        catch (Exception ex) when (GoFrame.IsPanic(ex, out PanicException? p))
        {
            GoFrame.Capture(p);
        }
        finally
        {
            frame.Run();
        }

        return recovered;
    }

    private static void AssertRuntimeError(object? recovered, string want, string site)
    {
        Assert.IsNotNull(recovered, $"{site}: did not panic");

        Assert.IsTrue(recovered._<Δruntime.ΔError>(out Δruntime.ΔError? error),
            $"{site}: recovered value {recovered} (type {recovered.GetType().FullName}) does not implement runtime.Error");

        Assert.AreEqual(want, error!.Error().ToString(), $"{site}: runtime.Error text");
    }

    // Runs fn on a goroutine, waits until it PARKS, runs wake, and returns what recover() saw there:
    // the woken-by-close door of a blocked send, which the immediate door never reaches.
    private static object? PanicValueAfterPark(Action fn, Action wake)
    {
        object? recovered = null;
        Exception? failure = null;
        Goroutine? g = null;
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                g = Goroutine.Current;
                recovered = PanicValue(fn);
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                done.Set();
            }
        });

        long deadline = Environment.TickCount64 + TimeoutMs;

        while (Volatile.Read(ref g) is not { IsParked: true })
        {
            if (done.IsSet)
                Assert.Fail($"the sender finished before parking: {failure?.ToString() ?? recovered?.ToString() ?? "no panic"}");

            if (Environment.TickCount64 > deadline)
                Assert.Fail("the sender never parked");

            Thread.Sleep(1);
        }

        wake();

        Assert.IsTrue(done.Wait(TimeoutMs), "the woken sender never finished");

        if (failure is not null)
            Assert.Fail($"the woken sender escaped with a non-panic exception: {failure}");

        return recovered;
    }

    // ---- runtime.plainError: assignment to entry in nil map -----------------------------------

    [TestMethod]
    public void NilMapIndexerWriteIsARuntimeError()
    {
        AssertRuntimeError(PanicValue(() =>
        {
            map<uint64, bool> m = default!;
            m[1234] = true;
        }), "assignment to entry in nil map", "map indexer set");
    }

    [TestMethod]
    public void NilMapAddIsARuntimeError()
    {
        AssertRuntimeError(PanicValue(() =>
        {
            map<uint64, bool> m = default!;
            m.Add(1234, true);
        }), "assignment to entry in nil map", "map.Add");
    }

    [TestMethod]
    public void NilMapSetIsARuntimeError()
    {
        AssertRuntimeError(PanicValue(() =>
        {
            map<uint64, bool> m = default!;
            m.Set(1234, true);
        }), "assignment to entry in nil map", "map.Set");
    }

    // ---- runtime.plainError: channels ---------------------------------------------------------

    [TestMethod]
    public void CloseOfClosedChannelIsARuntimeError()
    {
        AssertRuntimeError(PanicValue(() =>
        {
            channel<EmptyStruct> ch = new(0);
            close(ch);
            close(ch);
        }), "close of closed channel", "close of a closed channel");
    }

    [TestMethod]
    public void CloseOfNilChannelIsARuntimeError()
    {
        AssertRuntimeError(PanicValue(() =>
        {
            close((channel<bool>)default!);
        }), "close of nil channel", "close of a nil channel");
    }

    [TestMethod]
    public void SendOnClosedChannelIsARuntimeError()
    {
        AssertRuntimeError(PanicValue(() =>
        {
            channel<EmptyStruct> ch = new(0);
            close(ch);
            ch.ᐸꟷ(new EmptyStruct());
        }), "send on closed channel", "send on a closed channel");
    }

    [TestMethod]
    public void BlockedSendWokenByCloseIsARuntimeError()
    {
        channel<EmptyStruct> ch = new(0);

        AssertRuntimeError(PanicValueAfterPark(() => ch.ᐸꟷ(new EmptyStruct()), () => close(ch)),
            "send on closed channel", "a parked send woken by close");
    }

    [TestMethod]
    public void SelectSendOnClosedChannelIsARuntimeError()
    {
        AssertRuntimeError(PanicValue(() =>
        {
            channel<EmptyStruct> ch = new(0);
            close(ch);
            select(ch.Sending(new EmptyStruct()));
        }), "send on closed channel", "select send on a closed channel");
    }

    [TestMethod]
    public void BlockedSelectSendWokenByCloseIsARuntimeError()
    {
        channel<EmptyStruct> ch = new(0);

        AssertRuntimeError(PanicValueAfterPark(() => select(ch.Sending(new EmptyStruct())), () => close(ch)),
            "send on closed channel", "a parked select send woken by close");
    }

    // ---- runtime.plainError: makechan: size out of range --------------------------------------

    [TestMethod]
    public void MakeChanNegativeSizeIsARuntimeError()
    {
        nint n = -1;

        AssertRuntimeError(PanicValue(() => _ = new channel<bool>(n)),
            "makechan: size out of range", "channel(size)");
    }

    [TestMethod]
    public void MakeDirectionalChanNegativeSizeIsARuntimeError()
    {
        nint n = -1;

        AssertRuntimeError(PanicValue(() => _ = new channel<bool>(n, GoChanDir.Recv)),
            "makechan: size out of range", "channel(size, direction)");
    }

    [TestMethod]
    public void MakeCargoChanNegativeSizeIsARuntimeError()
    {
        nint n = -1;

        AssertRuntimeError(PanicValue(() => _ = new channel<bool>(n, (ChanCargo?)null)),
            "makechan: size out of range", "channel(size, cargo)");
    }

    [TestMethod]
    public void MakeChanThroughISupportMakeNegativeSizeIsARuntimeError()
    {
        nint n = -1;

        AssertRuntimeError(PanicValue(() => _ = channel<bool>.Make(n)),
            "makechan: size out of range", "channel<T>.Make");
    }

    // ---- runtime.boundsError: index out of range ----------------------------------------------

    [TestMethod]
    public void SliceIndexOutOfRangeIsARuntimeError()
    {
        AssertRuntimeError(PanicValue(() =>
        {
            slice<nint> s = new(2);
            _ = s[2];
        }), "runtime error: index out of range [2] with length 2", "slice index");
    }

    [TestMethod]
    public void ArrayIndexOutOfRangeIsARuntimeError()
    {
        nint i = 3;

        AssertRuntimeError(PanicValue(() =>
        {
            array<int> a = new(3);
            _ = a[i];
        }), "runtime error: index out of range [3] with length 3", "array index");
    }

    [TestMethod]
    public void StringIndexOutOfRangeIsARuntimeError()
    {
        AssertRuntimeError(PanicValue(() =>
        {
            @string s = "ab";
            _ = s[2];
        }), "runtime error: index out of range [2] with length 2", "string index");
    }

    [TestMethod]
    public void NegativeSliceIndexIsARuntimeError()
    {
        nint i = -1;

        AssertRuntimeError(PanicValue(() =>
        {
            slice<nint> s = new(2);
            _ = s[i];
        }), "runtime error: index out of range [-1]", "negative slice index");
    }

    // ---- fmt prints the recovered VALUE as Go does -----------------------------------------------

    // `fmt.Println(name, "->", recover())` is how the behavioral suite reads these panics
    // (CloseWakesBlocked), and its golden is Go's output. A plainError or boundsError reaches fmt as a
    // converted struct, and fmt must find its Error() (the GoImplement row) rather than print its
    // fields, or every such golden moves.
    [TestMethod]
    public void RecoveredValuesPrintAsGosText()
    {
        (Action fn, string want)[] cases =
        [
            (() => { map<uint64, bool> m = default!; m[1] = true; }, "assignment to entry in nil map"),
            (() => { channel<bool> ch = new(0); close(ch); close(ch); }, "close of closed channel"),
            (() => close((channel<bool>)default!), "close of nil channel"),
            (() => { channel<int> ch = new(1); close(ch); ch.ᐸꟷ(1); }, "send on closed channel"),
            (() => { nint n = -1; _ = new channel<bool>(n); }, "makechan: size out of range"),
            (() => { slice<nint> s = new(2); _ = s[2]; }, "runtime error: index out of range [2] with length 2"),
            (() => { nint i = -1; slice<nint> s = new(2); _ = s[i]; }, "runtime error: index out of range [-1]"),
        ];

        foreach ((Action fn, string want) in cases)
        {
            object? recovered = PanicValue(fn);

            Assert.IsNotNull(recovered, $"{want}: did not panic");
            Assert.AreEqual(want, fmt.Sprint(recovered).ToString(), "fmt.Sprint of the recovered value");
            Assert.AreEqual(want, fmt.Sprintf("%v", recovered).ToString(), "fmt %v of the recovered value");
        }
    }

    // ---- the fallback: with no runtime registered, the message stands as a string --------------

    // A converted program that never loads the runtime package has nobody to register the values, and
    // golib must still raise a readable panic. Also the control that every arm above reads the HOOK:
    // unregistered, the same sites recover strings again. MSTest runs an assembly's tests serially,
    // so the process-global hooks can be swapped and restored here.
    [TestMethod]
    public void UnregisteredHooksFallBackToThePlainMessage()
    {
        Func<string, object>? plain = RuntimeErrorPanic.PlainErrorValue;
        Func<long, long, bool, byte, object>? bounds = RuntimeErrorPanic.BoundsErrorValue;

        Assert.IsNotNull(plain, "the runtime module did not register PlainErrorValue");
        Assert.IsNotNull(bounds, "the runtime module did not register BoundsErrorValue");

        try
        {
            RuntimeErrorPanic.PlainErrorValue = null;
            RuntimeErrorPanic.BoundsErrorValue = null;

            object? closed = PanicValue(() => close((channel<bool>)default!));
            object? index = PanicValue(() => { slice<nint> s = new(2); _ = s[2]; });

            Assert.IsTrue(closed is string or @string, $"unregistered plain error recovered {closed?.GetType().FullName}");
            Assert.IsTrue(index is string or @string, $"unregistered bounds error recovered {index?.GetType().FullName}");
            Assert.AreEqual("close of nil channel", closed.ToString());
            Assert.AreEqual("runtime error: index out of range [2] with length 2", index.ToString());
        }
        finally
        {
            RuntimeErrorPanic.PlainErrorValue = plain;
            RuntimeErrorPanic.BoundsErrorValue = bounds;
        }
    }

    // ---- the TEXT is not this change's business: it must read exactly as it did ----------------

    [TestMethod]
    public void PanicTextIsGosText()
    {
        (Action fn, string want)[] cases =
        [
            (() => { map<uint64, bool> m = default!; m[1] = true; }, "assignment to entry in nil map"),
            (() => { channel<bool> ch = new(0); close(ch); close(ch); }, "close of closed channel"),
            (() => { slice<nint> s = new(2); _ = s[2]; }, "runtime error: index out of range [2] with length 2"),
        ];

        foreach ((Action fn, string want) in cases)
        {
            PanicException ex = Assert.ThrowsException<PanicException>(fn);
            Assert.AreEqual(want, ex.Message);
        }
    }

    // ---- the follow-up (COORD review, ledger 02:38): the doors the first cut did not reach ------

    // reflect's hand-owned nil-map write raised the text as a string, so `m[k] = v` and
    // `reflect.ValueOf(m).SetMapIndex(k, v)` recovered different types for one Go panic.
    [TestMethod]
    public void ReflectSetMapIndexOnANilMapIsARuntimeError()
    {
        AssertRuntimeError(PanicValue(() =>
        {
            map<@string, nint> m = default!;
            reflect.ValueOf(m).SetMapIndex(reflect.ValueOf((@string)"k"), reflect.ValueOf((nint)1));
        }), "assignment to entry in nil map", "reflect SetMapIndex on a nil map");
    }

    [TestMethod]
    public void ReflectCloseOfANilChannelIsARuntimeError()
    {
        AssertRuntimeError(PanicValue(() =>
        {
            channel<nint> ch = default!;
            reflect.ValueOf(ch).Close();
        }), "close of nil channel", "reflect Close of a nil channel");
    }

    // Go's makechan predicate: size < 0, or elem.Size_*size overflowing or above maxAlloc-hchanSize,
    // panics with plainError. The CLR threw OverflowException narrowing the size, which escaped recover().
    [TestMethod]
    public void MakeChanTooLargeIsARuntimeError()
    {
        nint n = (nint)1 << 62;

        AssertRuntimeError(PanicValue(() => _ = new channel<nint>(n)),
            "makechan: size out of range", "make(chan int, 1<<62)");
    }

    // A size Go ACCEPTS (a zero-size element has no memory bound) but the CLR cannot buffer: a named,
    // recoverable panic, never an escaping .NET exception.
    [TestMethod]
    public void MakeChanBeyondTheManagedBufferPanicsByName()
    {
        nint n = (nint)1 << 31;

        object? recovered = PanicValue(() => _ = new channel<EmptyStruct>(n));

        Assert.IsNotNull(recovered, "make(chan struct{}, 1<<31) did not panic");
        StringAssert.Contains(recovered.ToString(), "makechan", $"recovered {recovered}");
        StringAssert.Contains(recovered.ToString(), "2147483648", $"recovered {recovered}");
    }

    // An unsigned index Go reports as unsigned: the text keeps the length, and boundsError.signed is false.
    [TestMethod]
    public void AnUnsignedSliceIndexPrintsGosTextWithTheLength()
    {
        ulong u = ulong.MaxValue;

        object? recovered = PanicValue(() => { slice<nint> s = new(2); _ = s[u]; });

        AssertRuntimeError(recovered, "runtime error: index out of range [18446744073709551615] with length 2", "slice[uint64 max]");
        StringAssert.Contains(fmt.Sprintf("%#v", recovered).ToString(), "signed:false", "%#v of the recovered value");
    }

    [TestMethod]
    public void AnUnsignedArrayIndexPastTheSignedRangePrintsGosText()
    {
        ulong u = 1UL << 63;

        AssertRuntimeError(PanicValue(() => { array<nint> a = new(3); _ = a[u]; }),
            "runtime error: index out of range [9223372036854775808] with length 3", "array[1<<63]");
    }

    [TestMethod]
    public void AnUnsignedStringIndexPrintsGosText()
    {
        ulong u = (1UL << 32) + 5;

        AssertRuntimeError(PanicValue(() => { @string s = "abcdefgh"; _ = s[u]; }),
            "runtime error: index out of range [4294967301] with length 8", "string[1<<32+5]");
    }

    [TestMethod]
    public void AnUnsignedIndexInRangeStillReads()
    {
        slice<nint> s = new(3);
        s[1] = 7;
        ulong u = 1;

        Assert.AreEqual((nint)7, s[u]);
    }

    // &p[i] through a pointer-to-array (ж.at) threw a raw IndexOutOfRangeException.
    [TestMethod]
    public void AnElementAddressPastTheEndIsARuntimeError()
    {
        nint i = 5;

        AssertRuntimeError(PanicValue(() =>
        {
            ж<array<nint>> p = Ꮡ(new array<nint>(3));
            _ = p.at<nint>(i);
        }), "runtime error: index out of range [5] with length 3", "&p[5] through *[3]int");
    }

    // With nothing registered (a program whose import closure never reaches runtime), the value is a
    // string, and its TEXT is still Go's: a negative index prints the boundsNeg shape, no length.
    [TestMethod]
    public void UnregisteredFallbackTextIsGosForNegativeAndUnsignedIndexes()
    {
        Func<long, long, bool, byte, object>? bounds = RuntimeErrorPanic.BoundsErrorValue;

        try
        {
            RuntimeErrorPanic.BoundsErrorValue = null;

            nint neg = -1;
            ulong big = ulong.MaxValue;

            object? negative = PanicValue(() => { slice<nint> s = new(2); _ = s[neg]; });
            object? unsigned = PanicValue(() => { slice<nint> s = new(2); _ = s[big]; });

            Assert.AreEqual("runtime error: index out of range [-1]", negative?.ToString());
            Assert.AreEqual("runtime error: index out of range [18446744073709551615] with length 2", unsigned?.ToString());
        }
        finally
        {
            RuntimeErrorPanic.BoundsErrorValue = bounds;
        }
    }

    // ---- unhashable map keys (Go 1.26: maps.unhashableTypeError vs runtime.errorString) -------------
    //
    // Go 1.26 splits one panic in two. mapKeyError (internal/runtime/maps/map.go) refuses an unhashable
    // dynamic key on an EMPTY or NIL map without hashing it, and panics with unhashableTypeError, whose
    // Error() is "hash of unhashable type: T": no "runtime error: " prefix, a colon. It is still a
    // runtime.Error (RuntimeError()). The hash itself (runtime/alg.go's typehash and nilinterhash, reached
    // by a populated map's lookup and delete and by EVERY assignment) still panics with errorString, so
    // those keep "runtime error: hash of unhashable type T". Go 1.24 used errorString on both. Measured
    // by running behavioral MapKeyUpdateAndHashPanic under go1.24.13 and go1.26.9: exactly the empty-map
    // and nil-map lookup/delete lines and reflect's MapIndex/delete-on-nil lines moved.

    private static string TextOf(object? recovered) =>
        recovered is error e ? e.Error().ToString() : recovered?.ToString() ?? "<no panic>";

    private static slice<nint> OneInt() => new(new nint[] { 1 });

    [TestMethod]
    public void EmptyAndNilMapLookupsAndDeletesRaiseGos126UnhashableTypeError()
    {
        const string want = "hash of unhashable type: []int";

        map<object, bool> empty = new();
        map<object, bool> nilMap = default!;

        AssertRuntimeError(PanicValue(() => { _ = empty[OneInt()]; }), want, "empty map lookup");
        AssertRuntimeError(PanicValue(() => { empty.TryGetValue(OneInt(), out _); }), want, "empty map comma-ok");
        AssertRuntimeError(PanicValue(() => { empty.ContainsKey(OneInt()); }), want, "empty map ContainsKey");
        AssertRuntimeError(PanicValue(() => { empty.Remove(OneInt()); }), want, "empty map delete");
        AssertRuntimeError(PanicValue(() => { _ = nilMap[OneInt()]; }), want, "nil map lookup");
        AssertRuntimeError(PanicValue(() => { nilMap.Remove(OneInt()); }), want, "nil map delete");
    }

    [TestMethod]
    public void TheNamedTypeIsTheKeysSliceMapOrFuncType()
    {
        map<object, bool> empty = new();

        AssertRuntimeError(PanicValue(() => { _ = empty[new map<@string, nint>()]; }), "hash of unhashable type: map[string]int", "map key");
        AssertRuntimeError(PanicValue(() => { _ = empty[new Action(() => { })]; }), "hash of unhashable type: func()", "func key");
    }

    [TestMethod]
    public void PopulatedMapLookupAndEveryAssignmentKeepTheErrorStringText()
    {
        const string want = "runtime error: hash of unhashable type []int";

        map<object, bool> full = new() { [(nint)1] = true };
        map<object, bool> empty = new();

        // runtime.errorString IS a runtime.Error (`r.(runtime.Error)` is ok), and its Error() carries the prefix.
        AssertRuntimeError(PanicValue(() => { _ = full[OneInt()]; }), want, "populated map lookup");
        AssertRuntimeError(PanicValue(() => { full[OneInt()] = true; }), want, "populated map assign");
        AssertRuntimeError(PanicValue(() => { empty[OneInt()] = true; }), want, "empty map assign");
        AssertRuntimeError(PanicValue(() => reflect.ValueOf(empty).SetMapIndex(reflect.ValueOf(OneInt()), reflect.ValueOf(true))), want, "reflect SetMapIndex");
    }

    // A nil-wrapped named-func key reaches GetHashCode as its adapter shell, and Go's hash refuses it with
    // the same errorString (GoEqualityComparer's second throw site).
    [TestMethod]
    public void UnregisteredFallbackTextIsGosForTheHashPath()
    {
        Func<string, object>? hook = RuntimeErrorPanic.ErrorStringValue;

        try
        {
            RuntimeErrorPanic.ErrorStringValue = null;

            map<object, bool> full = new() { [(nint)1] = true };
            Assert.AreEqual("runtime error: hash of unhashable type []int", TextOf(PanicValue(() => { _ = full[OneInt()]; })));
        }
        finally
        {
            RuntimeErrorPanic.ErrorStringValue = hook;
        }
    }

    [TestMethod]
    public void ReflectNilAndEmptyMapArmsRaiseGos126UnhashableTypeError()
    {
        const string want = "hash of unhashable type: []int";

        map<object, bool> empty = new();
        map<object, bool> nilMap = default!;

        AssertRuntimeError(PanicValue(() => reflect.ValueOf(empty).MapIndex(reflect.ValueOf(OneInt()))), want, "reflect MapIndex on empty map");
        AssertRuntimeError(PanicValue(() => reflect.ValueOf(nilMap).MapIndex(reflect.ValueOf(OneInt()))), want, "reflect MapIndex on nil map");
        AssertRuntimeError(PanicValue(() => reflect.ValueOf(nilMap).SetMapIndex(reflect.ValueOf(OneInt()), new reflect.ΔValue(nil))), want, "reflect delete on nil map");
    }

    // With nothing registered, the text is still Go 1.26's (a string value, as the other fallbacks are).
    [TestMethod]
    public void UnregisteredFallbackTextIsGos126ForAnUnhashableKey()
    {
        Func<Type, object>? hook = RuntimeErrorPanic.UnhashableKeyErrorValue;

        try
        {
            RuntimeErrorPanic.UnhashableKeyErrorValue = null;

            map<object, bool> empty = new();
            Assert.AreEqual("hash of unhashable type: []int", TextOf(PanicValue(() => { _ = empty[OneInt()]; })));
        }
        finally
        {
            RuntimeErrorPanic.UnhashableKeyErrorValue = hook;
        }
    }
}

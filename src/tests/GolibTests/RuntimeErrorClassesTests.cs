using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using Δruntime = go.runtime_package;

namespace GolibTests;

// golib raised several Go runtime errors as plain strings: a recover() saw a string where Go's
// recover() sees a value satisfying runtime.Error, so `r.(runtime.Error)` and `r.(error)` missed.
// Go's types (go1.26.9 src/runtime):
//   runtime.errorString  nil dereference (panic.go:332 memoryError), makeslice (slice.go:30,34), growslice
//                        (slice.go:192,260), comparing uncomparable (alg.go:320,337), range func continued
//                        (panic.go:371,380 rangeDoneError)
//   runtime.plainError   panicwrap, a value method called on a nil pointer (error.go:348)
//   runtime.boundsError  slice to array conversion too short (abi.BoundsConvert)
// Each arm raises the site the way a program reaches it, and asks recover() for the value's type, its
// runtime.Error assertion and its Error() text. The hooks are registered by the runtime module initializer
// (panicvalues_impl.cs), as for the classes already served.
[TestClass]
public class RuntimeErrorClassesTests
{
    [ClassInitialize]
    public static void RunRuntimeModuleInitializer(TestContext _) =>
        RuntimeHelpers.RunModuleConstructor(typeof(Δruntime).Module.ModuleHandle);

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

    private static void AssertRuntimeError(object? recovered, string goType, string want, string site)
    {
        Assert.IsNotNull(recovered, $"{site}: did not panic");
        Assert.AreEqual(goType, recovered.GetType().Name, $"{site}: the recovered value's Go type");

        Assert.IsTrue(recovered._<Δruntime.ΔError>(out Δruntime.ΔError? error),
            $"{site}: recovered value {recovered} (type {recovered.GetType().FullName}) does not implement runtime.Error");

        Assert.AreEqual(want, error!.Error().ToString(), $"{site}: runtime.Error text");
    }

    // ---- errorString: nil pointer dereference ---------------------------------------------------

    private const string NilDeref = "runtime error: invalid memory address or nil pointer dereference";

    [TestMethod]
    public void ANilBoxReadIsAnErrorString()
    {
        AssertRuntimeError(PanicValue(() => { ж<nint> p = default!; _ = p.Value; }), "errorString", NilDeref, "nil box read");
    }

    [TestMethod]
    public void TheClrNullReferenceMapsToAnErrorString()
    {
        AssertRuntimeError(PanicValue(() => { string? n = null; _ = n!.Length; }), "errorString", NilDeref, "NullReferenceException");
    }

    [TestMethod]
    public void TheNilDereferenceFactoriesRaiseAnErrorString()
    {
        AssertRuntimeError(PanicValue(() => throw RuntimeErrorPanic.NilPointerDereference()), "errorString", NilDeref, "NilPointerDereference");
        AssertRuntimeError(PanicValue(() => throw RuntimeErrorPanic.WrapperNilReceiver()), "errorString", NilDeref, "WrapperNilReceiver");
        AssertRuntimeError(PanicValue(() => _ = wrapperRecv<object>(null)), "errorString", NilDeref, "wrapperRecv");
    }

    // ---- errorString: makeslice, growslice, comparing, range func ---------------------------------

    [TestMethod]
    public void MakeSliceLengthOutOfRangeIsAnErrorString()
    {
        nint negative = -1;
        nint huge = nint.MaxValue;

        AssertRuntimeError(PanicValue(() => _ = new slice<nint>(negative)), "errorString", "runtime error: makeslice: len out of range", "negative length");
        AssertRuntimeError(PanicValue(() => _ = new slice<nint>(huge)), "errorString", "runtime error: makeslice: len out of range", "huge length");
        AssertRuntimeError(PanicValue(() => throw RuntimeErrorPanic.MakeSliceLenOutOfRange()), "errorString", "runtime error: makeslice: len out of range", "factory");
    }

    [TestMethod]
    public void MakeSliceCapacityOutOfRangeIsAnErrorString()
    {
        nint huge = nint.MaxValue;

        AssertRuntimeError(PanicValue(() => _ = new slice<nint>(1, huge)), "errorString", "runtime error: makeslice: cap out of range", "huge capacity");
        AssertRuntimeError(PanicValue(() => throw RuntimeErrorPanic.MakeSliceCapOutOfRange()), "errorString", "runtime error: makeslice: cap out of range", "factory");
    }

    [TestMethod]
    public void GrowSliceLengthOutOfRangeIsAnErrorString()
    {
        slice<nint> s = new(1);
        nint past = (nint)Array.MaxLength + 1;

        AssertRuntimeError(PanicValue(() => _ = go.slice<nint>.GrowCapacity(s, past)), "errorString", "runtime error: growslice: len out of range", "GrowCapacity");
        AssertRuntimeError(PanicValue(() => throw RuntimeErrorPanic.GrowSliceLenOutOfRange()), "errorString", "runtime error: growslice: len out of range", "factory");
    }

    [TestMethod]
    public void ComparingUncomparableTypesIsAnErrorString()
    {
        object a = new slice<nint>(1), b = new slice<nint>(1);

        AssertRuntimeError(PanicValue(() => _ = AreEqual(a, b)), "errorString", "runtime error: comparing uncomparable type []int", "slice ==");
    }

    [TestMethod]
    public void ARangeFunctionContinuedIsAnErrorString()
    {
        AssertRuntimeError(PanicValue(() => throw RuntimeErrorPanic.RangeFunctionContinued()), "errorString",
            "runtime error: range function continued iteration after function for loop body returned false", "factory");
    }

    // ---- plainError: panicwrap --------------------------------------------------------------------

    [TestMethod]
    public void APanicwrapIsAPlainError()
    {
        const string text = "value method main.T.M called using nil *T pointer";

        AssertRuntimeError(PanicValue(() => _ = panicwrapRecv<nint>(null, text)), "plainError", text, "panicwrapRecv");
        AssertRuntimeError(PanicValue(() => throw RuntimeErrorPanic.Panicwrap(text)), "plainError", text, "factory");
    }

    // ---- boundsError: slice to array conversion ----------------------------------------------------

    [TestMethod]
    public void ASliceToArrayConversionTooShortIsABoundsError()
    {
        const string want = "runtime error: cannot convert slice with length 2 to array or pointer to array with length 4";
        slice<nint> s = new(2);

        AssertRuntimeError(PanicValue(() => _ = new array<nint>(s, 4)), "boundsError", want, "[4]T(s)");
        AssertRuntimeError(PanicValue(() => _ = array<nint>.Alias(s, 4)), "boundsError", want, "(*[4]T)(s)");
    }

    // ---- errorString: unsafe.Slice and unsafe.String refusals (runtime/unsafe.go:46,50,102,114) -------

    [TestMethod]
    public void UnsafeSliceAndStringRefusalsAreErrorStringsWithGosText()
    {
        ж<byte> nilPtr = default!;
        ж<byte> one = new StandardBox<byte>(1);
        nint negative = -1;
        nint two = 2;

        AssertRuntimeError(PanicValue(() => _ = go.unsafe_package.Slice(one, negative)), "errorString", "runtime error: unsafe.Slice: len out of range", "Slice negative");
        AssertRuntimeError(PanicValue(() => _ = go.unsafe_package.Slice(nilPtr, two)), "errorString", "runtime error: unsafe.Slice: ptr is nil and len is not zero", "Slice nil");
        AssertRuntimeError(PanicValue(() => _ = go.unsafe_package.String(one, negative)), "errorString", "runtime error: unsafe.String: len out of range", "String negative");
        AssertRuntimeError(PanicValue(() => _ = go.unsafe_package.String(nilPtr, two)), "errorString", "runtime error: unsafe.String: ptr is nil and len is not zero", "String nil");
    }

    // ---- the fallback: with no runtime registered the text stands as it was ----------------------

    [TestMethod]
    public void UnregisteredFallbacksKeepTheTextAsAString()
    {
        Func<string, object>? errorString = RuntimeErrorPanic.ErrorStringValue;
        Func<string, object>? plain = RuntimeErrorPanic.PlainErrorValue;
        Func<long, long, bool, byte, object>? bounds = RuntimeErrorPanic.BoundsErrorValue;

        try
        {
            RuntimeErrorPanic.ErrorStringValue = null;
            RuntimeErrorPanic.PlainErrorValue = null;
            RuntimeErrorPanic.BoundsErrorValue = null;

            (Func<PanicException> make, string want)[] cases =
            [
                (RuntimeErrorPanic.NilPointerDereference, NilDeref),
                (RuntimeErrorPanic.WrapperNilReceiver, NilDeref),
                (RuntimeErrorPanic.MakeSliceLenOutOfRange, "runtime error: makeslice: len out of range"),
                (RuntimeErrorPanic.MakeSliceCapOutOfRange, "runtime error: makeslice: cap out of range"),
                (RuntimeErrorPanic.GrowSliceLenOutOfRange, "runtime error: growslice: len out of range"),
                (() => RuntimeErrorPanic.ComparingUncomparableType("[]int"), "runtime error: comparing uncomparable type []int"),
                (RuntimeErrorPanic.RangeFunctionContinued, "runtime error: range function continued iteration after function for loop body returned false"),
                (() => RuntimeErrorPanic.Panicwrap("value method main.T.M called using nil *T pointer"), "value method main.T.M called using nil *T pointer"),
                (() => RuntimeErrorPanic.ArrayConversionLength(2, 4), "runtime error: cannot convert slice with length 2 to array or pointer to array with length 4"),
            ];

            foreach ((Func<PanicException> make, string want) in cases)
            {
                PanicException panic = make();

                Assert.AreEqual(want, PanicException.PanicText(panic.State), "unregistered text");
                Assert.IsTrue(panic.State is string or @string, $"unregistered state is {panic.State?.GetType().Name}");
            }
        }
        finally
        {
            RuntimeErrorPanic.ErrorStringValue = errorString;
            RuntimeErrorPanic.PlainErrorValue = plain;
            RuntimeErrorPanic.BoundsErrorValue = bounds;
        }
    }
}

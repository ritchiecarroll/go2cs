using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using Δruntime = go.runtime_package;

namespace GolibTests;

// Guards golib's panicCheck1 (golib/runtime/RuntimePanicCheck.cs): a bounds or shift panic raised by
// code IN package runtime is Go's fatal error, while the same panic raised anywhere else stays an
// ordinary, recoverable panic. It is decided where the panic is RECOVERED or REPORTED, from the frames
// the panic carries, so the factories that raise it walk nothing.
//
// WHAT THIS FILE CAN AND CANNOT ASSERT. A HIT ends in FatalReport.Fatal, which exits the process, so
// the HIT's CONSEQUENCE is runtime's own TestRuntimePanic child (crash_test.go), never an arm here.
// What is here is the TAG (the six panicCheck1 factories carry Go's throw text, and the divide and
// nil-dereference panics, panicCheck2's, carry none) and the PREDICATE, read from the trace of a panic
// really thrown from each marker class: a HIT, the nested HIT, and the excluded package runtime_test.
//
// The marker classes are declared in RuntimePanicCheckFixtures.cs, in THIS assembly: the predicate is
// structural (the outermost class in a frame's declaring-type name), so a fixture named exactly
// like the converter's class exercises the same test without loading a -tests build.
//
// A8b rides here too: a nil *Func's Entry and FileLine fault as Go's do, and Name() still answers "".
[TestClass]
public class RuntimePanicCheckTests
{
    private static readonly Type s_check = typeof(FatalReport).Assembly.GetType("go.golib.RuntimePanicCheck", throwOnError: true)!;

    private static readonly PropertyInfo s_throwText =
        typeof(PanicException).GetProperty("RuntimeThrowText", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static bool IsRuntimePackageType(Type type) =>
        (bool)s_check.GetMethod("IsRuntimePackageType", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [type.FullName])!;

    private static bool IsRuntimePackageTypeName(string typeName) =>
        (bool)s_check.GetMethod("IsRuntimePackageType", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [typeName])!;

    // The predicate over a thrown panic's trace. The trace is the exception's own, so reflection adds no
    // frame to it: what the walk reads is exactly the frames from the throw site to the catch.
    private static bool RaisedInRuntimePackage(Exception thrown, out string frameName)
    {
        object[] args = [new StackTrace(thrown, false), null];
        bool raised = (bool)s_check.GetMethod("RaisedInRuntimePackage", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, args)!;
        frameName = (string)args[1]!;
        return raised;
    }

    private static string ThrowText(PanicException panic) => (string)s_throwText.GetValue(panic);

    [TestMethod]
    public void TheMarkerIsPackageRuntimeItsInternalTestBridgeOrANestedType()
    {
        Assert.IsTrue(IsRuntimePackageType(typeof(Δruntime)), "package runtime's class is the marker");
        Assert.IsTrue(IsRuntimePackageType(typeof(Δruntime.Func)), "a type nested in package runtime's class is runtime");
        Assert.IsTrue(IsRuntimePackageType(typeof(runtime_internal_test_package)), "package runtime's own _test.go files are runtime.*");
        Assert.IsTrue(IsRuntimePackageType(typeof(runtime_internal_test_package.Nested)), "nested in the internal-test bridge is runtime");

        Assert.IsFalse(IsRuntimePackageType(typeof(runtime_test_package)), "package runtime_test is not runtime.*");
        Assert.IsFalse(IsRuntimePackageType(typeof(RuntimePanicCheckTests)), "a non-runtime class is not the marker");
    }

    [TestMethod]
    public void TheTraceNamesTheFirstFrameOutsideGolib()
    {
        Assert.IsTrue(RaisedInRuntimePackage(runtime_internal_test_package.Raise(), out string frame), "a panic raised on the internal-test bridge must HIT");
        Assert.AreEqual("go.runtime_internal_test_package.Raise", frame);

        Assert.IsTrue(RaisedInRuntimePackage(runtime_internal_test_package.Nested.Raise(), out frame), "a panic raised on a nested type must HIT");
        Assert.AreEqual("go.runtime_internal_test_package+Nested.Raise", frame);

        Assert.IsTrue(RaisedInRuntimePackage(runtime_internal_test_package.RaiseInGolib(), out frame),
            "a panic golib's bounds check raises for package runtime must HIT: golib's own frames are skipped");
        Assert.AreEqual("go.runtime_internal_test_package.RaiseInGolib", frame);

        Assert.IsFalse(RaisedInRuntimePackage(runtime_test_package.Raise(), out frame), "a panic raised in package runtime_test must MISS");
        Assert.AreEqual("go.runtime_test_package.Raise", frame);

        Assert.IsFalse(RaisedInRuntimePackage(ThrowHere(runtime_internal_test_package.Build()), out frame),
            "a panic built in package runtime but thrown outside it carries no runtime frame");
        Assert.AreEqual($"{typeof(RuntimePanicCheckTests).FullName}.{nameof(ThrowHere)}", frame);
    }

    // From 1.26.10.4 the forwarders of a struct's same-package promoted methods are in a class of the struct's own,
    // `<pkg>ᴛ<Struct>ᴛpkg`, and no longer in the package class. Go names a promoted method's wrapper runtime.*, and
    // the forwarder's frame is the one the walk meets when the JIT inlines the promoted method into it (a .NET trace
    // has no frame for an inlined method). The reader has names only, so the class is read by its name's form.
    [TestMethod]
    public void APromotedMethodClassOfPackageRuntimeIsTheMarkerToo()
    {
        Assert.IsTrue(IsRuntimePackageType(typeof(runtimeᴛheldᴛpkg)), "the promoted-method class of a struct of package runtime is runtime");
        Assert.IsTrue(IsRuntimePackageType(typeof(runtime_internal_testᴛheldᴛpkg)), "and of a struct of package runtime's own _test.go files");
        Assert.IsTrue(IsRuntimePackageTypeName("go.runtimeᴛmPaddedᴛpkg"), "the form, as the converted runtime's own class is named");

        Assert.IsTrue(RaisedInRuntimePackage(runtimeᴛheldᴛpkg.Raise(), out string frame), "a panic raised on package runtime's promoted-method class must HIT");
        Assert.AreEqual("go.runtimeᴛheldᴛpkg.Raise", frame);

        Assert.IsTrue(RaisedInRuntimePackage(runtime_internal_testᴛheldᴛpkg.Raise(), out frame), "a panic raised on the bridge's promoted-method class must HIT");
        Assert.AreEqual("go.runtime_internal_testᴛheldᴛpkg.Raise", frame);
    }

    // The control for the test above: the form is exact. Another package's promoted-method class is not runtime's,
    // and neither is a name that only starts or only ends as the form does.
    [TestMethod]
    public void AnotherPackagesPromotedMethodClassIsNotTheMarker()
    {
        Assert.IsFalse(IsRuntimePackageType(typeof(runtime_testᴛheldᴛpkg)), "package runtime_test's promoted-method class is not runtime.*");
        Assert.IsFalse(RaisedInRuntimePackage(runtime_testᴛheldᴛpkg.Raise(), out string frame), "a panic raised on it must MISS");
        Assert.AreEqual("go.runtime_testᴛheldᴛpkg.Raise", frame);

        Assert.IsFalse(IsRuntimePackageTypeName("go.runtime.debugᴛBuildInfoᴛpkg"), "runtime/debug is another package");
        Assert.IsFalse(IsRuntimePackageTypeName("go.runtimeᴛheld"), "the ending is part of the form");
        Assert.IsFalse(IsRuntimePackageTypeName("go.runtimeᴛpkg"), "a struct name is part of the form");
        Assert.IsFalse(IsRuntimePackageTypeName("go.syncᴛheldᴛpkg"), "another package's class is not runtime");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Exception ThrowHere(PanicException panic)
    {
        try
        {
            throw panic;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    // THE TAG. Every panicCheck1 factory carries Go's throw text, and panicCheck2's panics carry none:
    // a divide or nil dereference in package runtime stays recoverable, as in Go.
    [TestMethod]
    public void OnlyPanicCheck1sFactoriesCarryGosThrowText()
    {
        Assert.AreEqual("index out of range", ThrowText(RuntimeErrorPanic.IndexOutOfRange(5L, 3L)));
        Assert.AreEqual("index out of range", ThrowText(RuntimeErrorPanic.IndexOutOfRange(5UL, 3L)));
        // Every slice builder, since each reaches the ONE stamped constructor (SliceBoundsError) by its own path.
        Assert.AreEqual("slice bounds out of range", ThrowText(RuntimeErrorPanic.SliceBoundsOutOfRange(0, 5, 3)));
        Assert.AreEqual("slice bounds out of range", ThrowText(RuntimeErrorPanic.SliceBoundsOutOfRange(0, 5, 5, 3)));
        Assert.AreEqual("slice bounds out of range", ThrowText(RuntimeErrorPanic.LengthSliceBoundsOutOfRange(0, 5, 3)));
        Assert.AreEqual("slice bounds out of range", ThrowText(RuntimeErrorPanic.LengthSliceBoundsOutOfRange(0, 5, 5, 3)));
        Assert.AreEqual("slice length too short to convert to array or pointer to array", ThrowText(RuntimeErrorPanic.ArrayConversionLength(1, 2)));
        Assert.AreEqual("negative shift amount", ThrowText(RuntimeErrorPanic.NegativeShiftAmount()));

        Assert.IsNull(ThrowText(RuntimeErrorPanic.IntegerDivideByZero()), "a divide is panicCheck2's: recoverable in package runtime");
        Assert.IsNull(ThrowText(RuntimeErrorPanic.NilPointerDereference()), "a nil dereference is panicCheck2's: recoverable in package runtime");
    }

    // A8b. Go's (*Func).Entry and FileLine read f.raw() with no nil check, so a nil *Func faults and the
    // program dies naming runtime.(*Func).Entry (TestTracebackRuntimeMethod). Only Name() checks.
    [TestMethod]
    public void ANilFuncFaultsInEntryAndFileLineAndNameAnswersEmpty()
    {
        ж<Δruntime.Func> f = null!;

        PanicException entry = Assert.ThrowsException<PanicException>(() => Δruntime.Entry(f));
        StringAssert.Contains(entry.Message, "invalid memory address or nil pointer dereference");

        PanicException fileLine = Assert.ThrowsException<PanicException>(() => Δruntime.FileLine(f, 0));
        StringAssert.Contains(fileLine.Message, "invalid memory address or nil pointer dereference");

        Assert.AreEqual("", (string)Δruntime.Name(f), "Name() on a nil *Func answers \"\"");
    }
}

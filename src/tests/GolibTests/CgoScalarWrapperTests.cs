using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

// cgo Phase 2 increment 3b1 (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §4.2, read through §11): the scalar bindings the
// converter's cgoScalarWrapper writes, compiled and RUN against the C library. GoCgoWrappers.<os>.golden.cs is the
// generator's output byte for byte (src/go2cs's TestCgoScalarWrapperGoldensAreTheGeneratorsOutput holds it so), so what
// passes here is what the converter emits. Each call runs on a goroutine, as converted code does.
//
// The flavour compiled is $(GoTargetOS)'s; one built for another OS than the host reads Inconclusive, never a pass.
[TestClass]
public partial class CgoScalarWrapperTests
{
    internal static bool HostRunsThisFlavour() =>
#if GOCGO_FLAVOUR_LINUX
        OperatingSystem.IsLinux();
#elif GOCGO_FLAVOUR_DARWIN
        OperatingSystem.IsMacOS();
#else
        OperatingSystem.IsWindows();
#endif

    internal static T OnAGoroutine<T>(Func<T> call)
    {
        if (!HostRunsThisFlavour())
            Assert.Inconclusive("the cgo wrapper golden compiled here is another OS's: NOT MEASURED on this host");

        T result = default!;
        Exception? thrown = null;
        using ManualResetEventSlim done = new();

        goǃ(() =>
        {
            try
            {
                result = call();
            }
            catch (Exception ex)
            {
                thrown = ex;
            }
            finally
            {
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(30)), "the goroutine did not finish");
        Assert.IsNull(thrown, $"the call threw on a goroutine: {thrown}");

        return result;
    }

    [TestMethod]
    public void AScalarWrapperCallsTheCLibrary() =>
        Assert.AreEqual(7, OnAGoroutine(() => (int32)cgotestꓸC_package.abs(-7)), "abs(-7) through the generated binding");

    // C long follows the data model: 32 bits on windows (LLP64), 64 on linux and darwin (LP64). The value is chosen so
    // each width is exercised at its own edge, and the test says which it asserted.
    [TestMethod]
    public void CLongFollowsTheDataModel()
    {
#if GOCGO_FLAVOUR_LINUX || GOCGO_FLAVOUR_DARWIN
        Assert.AreEqual(5_000_000_000L, OnAGoroutine(() => (int64)cgotestꓸC_package.labs(-5_000_000_000L)), "labs on LP64: C long is 64 bits");
#else
        Assert.AreEqual(2_000_000_000, OnAGoroutine(() => (int32)cgotestꓸC_package.labs(-2_000_000_000)), "labs on LLP64: C long is 32 bits");
#endif
    }

    [TestMethod]
    public void AVoidWrapperReturns() =>
        OnAGoroutine(() => cgotestꓸC_package.srand(1));

    // The two-result form, r, err := C.f(x): a nonzero errno is syscall.Errno(errno), read inside the bracket (§11.3).
    [TestMethod]
    public void TheErrnoFormReturnsCgosError()
    {
#if GOCGO_FLAVOUR_LINUX || GOCGO_FLAVOUR_DARWIN
        (int32 result, error err) = OnAGoroutine(() =>
        {
            (cgotestꓸC_package.@int r, error e) = cgotestꓸC_package.closeꓸerrno(-1);
            return ((int32)r, e);
        });

        Assert.AreEqual(-1, result, "close(-1) fails");
        Assert.IsNotNull(err, "and its errno arrives as the error");
        Assert.AreEqual(new syscall_package.Errno(9), (syscall_package.Errno)err, "EBADF");
#else
        (float64 result, error err) = OnAGoroutine(() =>
        {
            (cgotestꓸC_package.@double r, error e) = cgotestꓸC_package.expꓸerrno(1000.0);
            return ((float64)r, e);
        });

        Assert.IsTrue(double.IsPositiveInfinity(result), "exp(1000) overflows");
        Assert.IsNotNull(err, "and its errno arrives as the error");
        Assert.AreEqual(new syscall_package.Errno(34), (syscall_package.Errno)err, "ERANGE");

        (float64 fine, error none) = OnAGoroutine(() =>
        {
            (cgotestꓸC_package.@double r, error e) = cgotestꓸC_package.expꓸerrno(0.0);
            return ((float64)r, e);
        });

        Assert.AreEqual(1.0, fine, "exp(0)");
        Assert.IsNull(none, "a zero errno is a nil error, as cgo makes it");
#endif
    }

#if !(GOCGO_FLAVOUR_LINUX || GOCGO_FLAVOUR_DARWIN)
    [LibraryImport("ucrtbase.dll", EntryPoint = "_errno")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial nint CrtErrnoLocation();

    // The windows errno form takes the C runtime's errno ADDRESS before the call and reads through it after (§11.3, as
    // amended 2026-11-10). That is sound only if the address is the same thread slot on both sides of the call, which
    // this holds: on the wrapper's own goroutine, the address read before a wrapped call equals the one read after.
    [TestMethod]
    public void TheCRuntimesErrnoSlotIsTheSameBeforeAndAfterAWrappedCall()
    {
        (nint before, nint after) = OnAGoroutine(() =>
        {
            nint slot = CrtErrnoLocation();
            cgotestꓸC_package.expꓸerrno(1000.0);
            return (slot, CrtErrnoLocation());
        });

        Assert.AreNotEqual((nint)0, before, "the C runtime has an errno slot for this thread");
        Assert.AreEqual(before, after, "the same slot after the call");
    }
#endif
}

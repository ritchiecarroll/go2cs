using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards the memory profile's on/off decision (class M, M1; COORD ruling 2026-09-27, option (a)), the
/// managed analog of Go's disableMemoryProfiling: Go's linker sets it when runtime.memProfileInternal is
/// unreachable, and the runtime then starts with MemProfileRate 0. Here the rate stays at Go's default only
/// when runtime.pprof is in the program's STATIC assembly closure (its deps.json list, read once at
/// runtime's module initialisation), never from the assemblies loaded so far, which load lazily. Red
/// against the first M1 cut, which kept the rate on in every program.
/// The predicate is golib's (COORD ruling 2026-09-29 08:43, map-growth option (a)): one answer, read by
/// runtime's module initializer for the rate and folded by the JIT into the map store's growth branch.
/// Red against cb33faec95, where the predicate was runtime's and golib had no answer to fold; the
/// compiled-in arm is red against 868157de98, where a list without runtime.pprof answered "unreachable"
/// for runtime/pprof's own test binary.
/// </summary>
[TestClass]
public class MemProfileReachabilityTests
{
    private static string Closure(params string[] names)
    {
        string dir = Path.Combine(Path.GetTempPath(), "app");
        return string.Join(Path.PathSeparator, System.Array.ConvertAll(names, name => Path.Combine(dir, name)));
    }

    private static System.Type? Absent() => null;

    private static System.Type? Present() => typeof(object);

    private static System.Type? Unreadable() => throw new System.PlatformNotSupportedException();

    [TestMethod]
    public void MemoryProfilingIsOnOnlyWhenPprofIsInTheStaticClosure()
    {
        // A converted program that never imports runtime/pprof: Go's linker would drop memProfileInternal.
        Assert.IsFalse(GoMemProfile.PprofReachableFrom(Closure("golib.dll", "go.runtime.dll", "go.fmt.dll", "main.dll"), Absent),
            "a closure without runtime.pprof must start with MemProfileRate 0");

        // One that imports it, directly or through net/http/pprof.
        Assert.IsTrue(GoMemProfile.PprofReachableFrom(Closure("golib.dll", "go.runtime.dll", "go.runtime.pprof.dll", "main.dll"), Absent),
            "a closure with runtime.pprof keeps Go's default MemProfileRate");

        // A program whose own assembly compiles runtime/pprof in, as runtime/pprof's own test binary does
        // (its production files are compile items of go.runtime.pprof.tests): the list does not name it, and
        // the package type, found in the program, decides.
        Assert.IsTrue(GoMemProfile.PprofReachableFrom(Closure("golib.dll", "go.runtime.dll", "go.runtime.pprof.tests.dll"), Present),
            "runtime/pprof compiled into the program itself is reachable");

        // A name that only contains it is not it.
        Assert.IsFalse(GoMemProfile.PprofReachableFrom(Closure("go.runtime.dll", "go.runtime.pprof.extra.dll"), Absent),
            "only the runtime.pprof assembly itself enables the profile");

        // No list (native AOT, single file): runtime.pprof's package type, looked up by its constant name, decides,
        // and a lookup that cannot answer keeps Go's default.
        Assert.IsFalse(GoMemProfile.PprofReachableFrom(null, Absent), "no list and no runtime.pprof type: off");
        Assert.IsTrue(GoMemProfile.PprofReachableFrom(null, Present), "no list and the runtime.pprof type resolves: on");
        Assert.IsTrue(GoMemProfile.PprofReachableFrom(null, Unreadable), "no answer: Go's default stands");

        // The live decision for this host (GolibTests.csproj references runtime.pprof, so it is on) is read by
        // MemProfileRecordTests, whose first assertion is Go's default rate.
    }

    [TestMethod]
    public void TheLiveDecisionIsGolibsOneAnswer()
    {
        // GolibTests.csproj references runtime.pprof, so this host's static closure holds it.
        Assert.IsTrue(GoMemProfile.PprofReachable, "this host's closure holds runtime.pprof, so the profile is reachable");
        Assert.AreEqual(GoMemProfile.PprofReachableFrom(System.AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string, Absent),
            GoMemProfile.PprofReachable, "the startup answer is the predicate over the host's own list");
    }
}

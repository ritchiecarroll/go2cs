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
        Assert.IsFalse(runtime_package.GoMemProfileReachable(Closure("golib.dll", "runtime.dll", "fmt.dll", "main.dll"), Absent),
            "a closure without runtime.pprof must start with MemProfileRate 0");

        // One that imports it, directly or through net/http/pprof.
        Assert.IsTrue(runtime_package.GoMemProfileReachable(Closure("golib.dll", "runtime.dll", "runtime.pprof.dll", "main.dll"), Absent),
            "a closure with runtime.pprof keeps Go's default MemProfileRate");

        // A name that only contains it is not it.
        Assert.IsFalse(runtime_package.GoMemProfileReachable(Closure("runtime.dll", "runtime.pprof.extra.dll"), Absent),
            "only the runtime.pprof assembly itself enables the profile");

        // No list (native AOT, single file): runtime.pprof's package type, looked up by its constant name, decides,
        // and a lookup that cannot answer keeps Go's default.
        Assert.IsFalse(runtime_package.GoMemProfileReachable(null, Absent), "no list and no runtime.pprof type: off");
        Assert.IsTrue(runtime_package.GoMemProfileReachable(null, Present), "no list and the runtime.pprof type resolves: on");
        Assert.IsTrue(runtime_package.GoMemProfileReachable(null, Unreadable), "no answer: Go's default stands");

        // The live decision for this host (GolibTests.csproj references runtime.pprof, so it is on) is read by
        // MemProfileRecordTests, whose first assertion is Go's default rate.
    }
}

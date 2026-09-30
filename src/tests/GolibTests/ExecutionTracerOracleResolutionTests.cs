using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// The tracer parser arm's oracle is the PINNED toolchain's <c>go tool trace</c>. A go of another release
/// parses a different flag set (go1.22's <c>-d</c> is an int, so <c>-d=parsed</c> prints usage), and the
/// arm then reads as a converter failure. Resolution therefore refuses a go that is not the pinned release,
/// by name, and never runs it silently.
/// </summary>
[TestClass]
public class ExecutionTracerOracleResolutionTests
{
    private static string Exe => OperatingSystem.IsWindows() ? "go.exe" : "go";

    // A directory tree holding an empty file where a go binary would be: resolution only asks File.Exists.
    private static string FakeGo(string root, string name, bool underBin)
    {
        string dir = underBin ? Path.Combine(root, name, "bin") : Path.Combine(root, name);
        Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, Exe), []);
        return dir;
    }

    private static string Scratch()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"go2cs-oracle-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        return dir;
    }

    [TestMethod]
    public void AnAmbientGoOfAnotherReleaseIsRefusedByName()
    {
        string root = Scratch();

        try
        {
            string ambient = FakeGo(root, "ambient", underBin: false);
            (string? go, string? skip) = ExecutionTracerParserTests.ResolveOracle(null, ambient, "1.24.13", _ => "go1.22.12");

            Assert.IsNull(go, "a go of another release must not be returned as the oracle");
            StringAssert.Contains(skip, "go1.22.12");
            StringAssert.Contains(skip, "1.24.13");
            StringAssert.Contains(skip, Path.Combine(ambient, Exe), "the reason names the go that was refused");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void AGoRootToolchainOfAnotherReleaseIsRefusedToo()
    {
        string root = Scratch();

        try
        {
            FakeGo(root, "goroot", underBin: true);
            (string? go, string? skip) = ExecutionTracerParserTests.ResolveOracle(Path.Combine(root, "goroot"), null, "1.24.13", _ => "go1.23.12");

            Assert.IsNull(go);
            StringAssert.Contains(skip, "go1.23.12");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void ThePinnedReleaseResolvesFromGoRootThenPath()
    {
        string root = Scratch();

        try
        {
            string ambient = FakeGo(root, "ambient", underBin: false);
            FakeGo(root, "goroot", underBin: true);

            (string? viaRoot, string? none) = ExecutionTracerParserTests.ResolveOracle(Path.Combine(root, "goroot"), ambient, "1.24.13", _ => "go1.24.13");
            Assert.AreEqual(Path.Combine(root, "goroot", "bin", Exe), viaRoot, "GOROOT wins over PATH");
            Assert.IsNull(none);

            (string? viaPath, _) = ExecutionTracerParserTests.ResolveOracle(null, ambient, "1.24.13", _ => "go1.24.13");
            Assert.AreEqual(Path.Combine(ambient, Exe), viaPath);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void ARootReleaseSpelledWithoutItsZeroPatchMatches()
    {
        string root = Scratch();

        try
        {
            string ambient = FakeGo(root, "ambient", underBin: false);
            (string? go, _) = ExecutionTracerParserTests.ResolveOracle(null, ambient, "1.25.0", _ => "go1.25");

            Assert.IsNotNull(go, "Go prints go1.25 for the x.y.0 release");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void AnUnreadablePinOrVersionSkipsByNameRatherThanRunningUnverified()
    {
        string root = Scratch();

        try
        {
            string ambient = FakeGo(root, "ambient", underBin: false);

            (string? noPin, string? pinSkip) = ExecutionTracerParserTests.ResolveOracle(null, ambient, null, _ => "go1.24.13");
            Assert.IsNull(noPin);
            StringAssert.Contains(pinSkip, "version.props");

            (string? noVersion, string? versionSkip) = ExecutionTracerParserTests.ResolveOracle(null, ambient, "1.24.13", _ => null);
            Assert.IsNull(noVersion);
            StringAssert.Contains(versionSkip, "go version");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void NoGoAnywhereStillSkipsWithItsOwnReason()
    {
        (string? go, string? skip) = ExecutionTracerParserTests.ResolveOracle(null, "", "1.24.13", _ => "go1.24.13");

        Assert.IsNull(go);
        StringAssert.Contains(skip, "no Go toolchain resolves");
    }
}

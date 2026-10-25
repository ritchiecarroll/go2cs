using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;
using Δdebug = go.runtime.debug_package;

namespace GolibTests;

/// <summary>
/// The run-time half of the build-info stamp: golib's <see cref="GoModInfoAttribute.Compose(string, string, string, bool, string)"/>,
/// which runtime/debug's <c>modinfo()</c> returns and <c>ReadBuildInfo</c> unwraps.
/// </summary>
/// <remarks>
/// The consuming suite is Go's own <c>crypto/internal/fips140test</c> <c>TestVersion</c>, which skips "no build info"
/// when <c>ReadBuildInfo</c> reports <c>ok == false</c>, and runs only through the -tests pipeline. These arms hold
/// the composition from a clone, under the standing GolibTests gate: the sentinels <c>ReadBuildInfo</c> strips, a
/// round-trip through the REAL converted <c>ParseBuildInfo</c>, and "" when nothing is stamped.
/// </remarks>
[TestClass]
public class GoModInfoComposeTests
{
    // cmd/go/internal/modload/build.go infoStart / infoEnd.
    private static readonly byte[] s_infoStart = Convert.FromHexString("3077af0c9274080241e1c107e6d618e6");
    private static readonly byte[] s_infoEnd = Convert.FromHexString("f932433186182072008242104116d8f2");

    private const string Modules = "path\texample.test/prog\nmod\texample.test/prog\t(devel)\t\ndep\texample.test/dep\tv0.1.0\t\n";

    /// <summary>The value is wrapped in cmd/go's sentinels, sixteen bytes each side, around Go's line form.</summary>
    [TestMethod]
    public void ComposedValueCarriesTheSentinels()
    {
        byte[] data = GoModInfoAttribute.Compose(Modules, "windows", "amd64", false, "");

        CollectionAssert.AreEqual(s_infoStart, data[..16], "the leading sentinel is not cmd/go's infoStart");
        CollectionAssert.AreEqual(s_infoEnd, data[^16..], "the trailing sentinel is not cmd/go's infoEnd");

        string text = System.Text.Encoding.UTF8.GetString(data[16..^16]);

        Assert.AreEqual(Modules + "build\tCGO_ENABLED=0\nbuild\tGOARCH=amd64\nbuild\tGOOS=windows\n", text);
    }

    /// <summary>
    /// What <c>ReadBuildInfo</c> does with the value: strip <c>data[16:len-16]</c> and parse it with the real
    /// converted <c>ParseBuildInfo</c>. Every stamped and composed field comes back, in cmd/go's setting order.
    /// </summary>
    [TestMethod]
    public void ComposedValueRoundTripsThroughParseBuildInfo()
    {
        @string data = GoModInfoAttribute.Compose(Modules, "linux", "arm64", true, "panicnil=1,x509sha1=1");

        Assert.IsTrue(len(data) >= 32, "ReadBuildInfo treats a value shorter than 32 bytes as no build info");

        var (bi, err) = Δdebug.ParseBuildInfo(data.slice(16, len(data) - 16));

        Assert.IsTrue(err == default!, $"ParseBuildInfo refused the composed value: {err?.Error()}");

        ref Δdebug.BuildInfo info = ref bi.Value;

        Assert.AreEqual("example.test/prog", info.Path.ToString());
        Assert.AreEqual("example.test/prog", info.Main.Path.ToString());
        Assert.AreEqual("(devel)", info.Main.Version.ToString());
        Assert.AreEqual("", info.Main.Sum.ToString());
        Assert.AreEqual(1, (int)len(info.Deps));
        Assert.AreEqual("example.test/dep", info.Deps[0].Value.Path.ToString());
        Assert.AreEqual("v0.1.0", info.Deps[0].Value.Version.ToString());

        string[] expected = ["DefaultGODEBUG=panicnil=1,x509sha1=1", "CGO_ENABLED=1", "GOARCH=arm64", "GOOS=linux"];

        Assert.AreEqual(expected.Length, (int)len(info.Settings), "the composed value carries a setting it should not");

        for (int i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], $"{info.Settings[i].Key}={info.Settings[i].Value}");

        // BuildInfo.String() is the inverse ParseBuildInfo documents; with no GoVersion it reproduces the text.
        Assert.AreEqual(Modules + "build\tDefaultGODEBUG=panicnil=1,x509sha1=1\nbuild\tCGO_ENABLED=1\nbuild\tGOARCH=arm64\nbuild\tGOOS=linux\n",
            Δdebug.String(ref info).ToString());
    }

    // An assembly holding exactly one converted package class, the way a test host's entry assembly holds the
    // package under test.
    private static Assembly CompilePackageAssembly(string assemblyName, string packageClass, string packageNamespace = "go")
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText($"namespace {packageNamespace} {{ public static partial class {packageClass} {{ }} }}")],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream image = new();
        EmitResult result = compilation.Emit(image);

        Assert.IsTrue(result.Success, string.Join("\n", result.Diagnostics));
        return Assembly.Load(image.ToArray());
    }

    /// <summary>
    /// The host's main-package detection (<c>TestHost.HostsMainPackage</c>), which picks the bare path over
    /// <c>&lt;path&gt;.test</c>: true for an entry assembly carrying a converted <c>main_package</c> class in any namespace, false
    /// for one carrying any other package's class, and false for no entry assembly at all.
    /// </summary>
    [TestMethod]
    public void HostDetectsAMainPackageUnderTest()
    {
        Assert.IsTrue(TestHost.HostsMainPackage(CompilePackageAssembly("BuildInfoMainProbe", "main_package")),
            "an entry assembly carrying go.main_package was not read as a main package under test");

        // A MODULE-ROOT main converts into its module path's namespace (example.com/app -> go.example.com).
        Assert.IsTrue(TestHost.HostsMainPackage(CompilePackageAssembly("BuildInfoRootMainProbe", "main_package", "go.example.com")),
            "an entry assembly carrying go.example.com.main_package (a module-root main) was not read as a main package under test");

        Assert.IsFalse(TestHost.HostsMainPackage(CompilePackageAssembly("BuildInfoLibProbe", "lib_package")),
            "an entry assembly carrying only go.lib_package was read as a main package under test");

        Assert.IsFalse(TestHost.HostsMainPackage(null));
    }

    /// <summary>
    /// Go's <c>runtime.modinfo</c> is a static string, so reading it allocates nothing: the converted value is composed
    /// once and every later read returns the SAME bytes -- one backing, so the second read composed nothing. A
    /// registration composes the next read afresh, since the value describes it.
    /// </summary>
    [TestMethod]
    public void SecondReadComposesNothing()
    {
        GoModInfoAttribute.Register("example.test/once", "example.test/once", mainPackage: false);

        @string first = GoModInfoAttribute.Compose("windows", "amd64", false);
        @string second = GoModInfoAttribute.Compose("windows", "amd64", false);

        Assert.IsTrue(len(first) >= 32, "the registration composed no build info");
        Assert.IsTrue(Unsafe.AreSame(ref MemoryMarshal.GetReference(first.ToSpan()), ref MemoryMarshal.GetReference(second.ToSpan())),
            "the second read composed its own bytes; Go's modinfo is one static string");

        var (bi1, ok1) = Δdebug.ReadBuildInfo();
        var (bi2, ok2) = Δdebug.ReadBuildInfo();

        Assert.IsTrue(ok1 && ok2);
        Assert.AreEqual(bi1.Value.Path.ToString(), bi2.Value.Path.ToString());

        GoModInfoAttribute.Register("example.test/twice", "example.test/twice", mainPackage: false);

        @string third = GoModInfoAttribute.Compose("windows", "amd64", false);

        Assert.IsFalse(Unsafe.AreSame(ref MemoryMarshal.GetReference(first.ToSpan()), ref MemoryMarshal.GetReference(third.ToSpan())),
            "a registration did not reset the composed value");
        StringAssert.Contains(System.Text.Encoding.UTF8.GetString(third.ToSpan()), "path\texample.test/twice.test\n");
    }

    /// <summary>
    /// A main package's test binary carries the package's own dep lines, as Go's test main copies its build info: the host
    /// passes each dep line's fields, and the registration writes them after the <c>mod</c> line, in order. A replaced
    /// module carries no sum column. <c>ReadBuildInfo</c> then reports them as <c>Deps</c>.
    /// </summary>
    [TestMethod]
    public void RegisteredDependenciesAnswerReadBuildInfoDeps()
    {
        GoModInfoAttribute.Register("example.test/m/cmd/p", "example.test/m", mainPackage: true,
            ["golang.org/x/sync\tv0.24.0\th1:sync=", "golang.org/x/text\tv0.3.0"]);

        Assert.AreEqual("path\texample.test/m/cmd/p\nmod\texample.test/m\t(devel)\t\n" +
            "dep\tgolang.org/x/sync\tv0.24.0\th1:sync=\ndep\tgolang.org/x/text\tv0.3.0\n", GoModInfoAttribute.Registered);

        var (bi, ok) = Δdebug.ReadBuildInfo();

        Assert.IsTrue(ok, "ReadBuildInfo reported no build info after a registration with dependencies");
        Assert.AreEqual(2, (int)len(bi.Value.Deps));
        Assert.AreEqual("golang.org/x/sync", bi.Value.Deps[0].Value.Path.ToString());
        Assert.AreEqual("v0.24.0", bi.Value.Deps[0].Value.Version.ToString());
        Assert.AreEqual("h1:sync=", bi.Value.Deps[0].Value.Sum.ToString());
        Assert.AreEqual("golang.org/x/text", bi.Value.Deps[1].Value.Path.ToString());
        Assert.AreEqual("", bi.Value.Deps[1].Value.Sum.ToString());
    }

    /// <summary>No stamp and no registration is no build info: "", so <c>ReadBuildInfo</c> reports <c>ok == false</c>.</summary>
    [TestMethod]
    public void AbsentModulesComposeToEmpty()
    {
        Assert.AreEqual(0, (int)len(GoModInfoAttribute.Compose("", "windows", "amd64", false, "panicnil=1")));
    }

    /// <summary>
    /// A test host's registration is what <c>ReadBuildInfo</c> answers from, end to end through runtime/debug's own
    /// <c>modinfo()</c>: <c>path &lt;pkg&gt;.test</c> and, for a module package, <c>mod &lt;module&gt; (devel)</c>. The MSTest
    /// host's entry assembly carries no stamp, so this is the registration alone -- one path line.
    /// </summary>
    [TestMethod]
    public void RegisteredTestHostAnswersReadBuildInfo()
    {
        // A MAIN package's test binary keeps the package's own path, as cmd/go's test main copies its build info.
        GoModInfoAttribute.Register("example.test/prog", "example.test/prog", mainPackage: true);

        Assert.AreEqual("path\texample.test/prog\nmod\texample.test/prog\t(devel)\t\n", GoModInfoAttribute.Registered);

        GoModInfoAttribute.Register("example.test/prog/sub", "example.test/prog", mainPackage: false);

        Assert.AreEqual("path\texample.test/prog/sub.test\nmod\texample.test/prog\t(devel)\t\n", GoModInfoAttribute.Registered);

        var (bi, ok) = Δdebug.ReadBuildInfo();

        Assert.IsTrue(ok, "ReadBuildInfo reported no build info after a test-host registration");
        Assert.AreEqual("example.test/prog/sub.test", bi.Value.Path.ToString());
        Assert.AreEqual("example.test/prog", bi.Value.Main.Path.ToString());
        Assert.AreEqual(go.runtime_package.Version().ToString(), bi.Value.GoVersion.ToString());

        GoModInfoAttribute.Register("crypto/internal/fips140test", "", mainPackage: false);

        Assert.AreEqual("path\tcrypto/internal/fips140test.test\n", GoModInfoAttribute.Registered,
            "a standard-library test binary has no main module, so it registers no mod line");

        (bi, ok) = Δdebug.ReadBuildInfo();

        Assert.IsTrue(ok);
        Assert.AreEqual("crypto/internal/fips140test.test", bi.Value.Path.ToString());
        Assert.AreEqual("", bi.Value.Main.Path.ToString());
    }
}

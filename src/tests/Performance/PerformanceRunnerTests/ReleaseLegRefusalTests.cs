// ReleaseLegRefusalTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerformanceRunner;

namespace PerformanceRunnerTests;

/// <summary>
/// The release legs' refusals (docs/PLAN-perf-rebaseline.md, S0). A re-baseline number is only worth publishing if the
/// binary it was timed on is the one its column names, so each shape that is not is refused BY NAME before anything
/// is timed: a shipped AOT binary the manifest does not vouch for, a JIT binary that is not framework-dependent, a
/// ReadyToRun binary that is not the template's self-contained publish, and a converted project that is not the
/// published packages at the leg's version.
/// </summary>
[TestClass]
public class ReleaseLegRefusalTests
{
    private const string Sha = "9e8797df87c83976297da28452f94eb2ec53fe2b2b3c904ec84bee2fe61e9666";
    private const string Source = "13e1e1d23b8c1cf67f65412236acfcf89cfbb17832f1120e8a9f6ec39a1856a5";

    private static readonly ReleaseLeg s_leg = new("1.26", @"C:\tools\go2cs.exe", "1.26.10.1", @"C:\sdk\go1.26.10\bin\go.exe");

    private static AotManifestEntry Entry() => new("1.26", "PerfFib", Sha, Source, 298189824, "10.0.11", "10.0.400");

    // ---- the AOT handoff ----

    [TestMethod]
    public void AShippedAotBinaryWhoseHashDiffersIsRefusedByName()
    {
        string? why = AotManifest.Refusal("1.26", "PerfFib", Entry(), binaryPresent: true,
            actualSha256: "78a092bbe93ddc002fe07764be7cc67e12dd2495858c14190b5679d2afcd3f0c", actualSize: 298189824, localSourceHash: Source);

        Assert.IsNotNull(why, "a binary whose SHA-256 is not the manifest's was admitted for timing");
        StringAssert.Contains(why, "1.26/PerfFib");
        StringAssert.Contains(why, "78a092bbe93d");
        StringAssert.Contains(why, "never measure");
    }

    [TestMethod]
    public void AnAotBinaryWithNoManifestEntryIsNeverTimed()
    {
        string? why = AotManifest.Refusal("1.26", "PerfFib", entry: null, binaryPresent: true,
            actualSha256: Sha, actualSize: 298189824, localSourceHash: Source);

        Assert.IsNotNull(why, "a foreign AOT binary with no manifest entry was admitted for timing");
        StringAssert.Contains(why, "1.26/PerfFib");
        StringAssert.Contains(why, "no manifest entry");
    }

    [TestMethod]
    public void AnAotBinaryOfADifferentConvertedProgramIsRefused()
    {
        string? why = AotManifest.Refusal("1.26", "PerfFib", Entry(), binaryPresent: true,
            actualSha256: Sha, actualSize: 298189824, localSourceHash: "b3f75657c35be585c003db76fd10a81e72cc7360c3ba52bb209a89b5b520a410");

        Assert.IsNotNull(why, "a binary built from another box's different conversion was admitted");
        StringAssert.Contains(why, "converted a different program");
    }

    [TestMethod]
    public void AnAotBinaryOfTheWrongSizeIsRefused()
    {
        string? why = AotManifest.Refusal("1.26", "PerfFib", Entry(), binaryPresent: true,
            actualSha256: Sha, actualSize: 298189825, localSourceHash: Source);

        Assert.IsNotNull(why);
        StringAssert.Contains(why, "298189825 bytes");
    }

    [TestMethod]
    public void AMissingShippedBinaryIsRefused()
    {
        string? why = AotManifest.Refusal("1.26", "PerfFib", Entry(), binaryPresent: false, actualSha256: null, actualSize: 0, localSourceHash: Source);

        Assert.IsNotNull(why);
        StringAssert.Contains(why, "missing");
    }

    [TestMethod]
    public void TheShippedBinaryTheManifestVouchesForIsAdmitted() =>
        Assert.IsNull(AotManifest.Refusal("1.26", "PerfFib", Entry(), binaryPresent: true, actualSha256: Sha.ToUpperInvariant(), actualSize: 298189824, localSourceHash: Source));

    [TestMethod]
    public void TheManifestParsesItsLinesAndRefusesAMalformedOne()
    {
        string text = $"# leg program sha256 sourceHash size ilc sdk\r\n1.26\tPerfFib\t{Sha}\t{Source}\t298189824\t10.0.11\t10.0.400\r\n\r\n";
        IReadOnlyList<AotManifestEntry> entries = AotManifest.Parse(text);

        Assert.AreEqual(1, entries.Count);
        Assert.AreEqual(Entry(), entries[0]);

        Assert.ThrowsException<FormatException>(() => AotManifest.Parse($"1.26\tPerfFib\t{Sha}\t{Source}\t298189824\t10.0.11"));
        Assert.ThrowsException<FormatException>(() => AotManifest.Parse($"1.26\tPerfFib\t{Sha}\t{Source}\tlarge\t10.0.11\t10.0.400"));
        Assert.ThrowsException<FormatException>(() => AotManifest.Parse(text + text));
    }

    // ---- the binaries' shapes ----

    private const string FrameworkDependent = """{ "runtimeOptions": { "tfm": "net10.0", "framework": { "name": "Microsoft.NETCore.App", "version": "10.0.0" } } }""";
    private const string SelfContained = """{ "runtimeOptions": { "tfm": "net10.0", "includedFrameworks": [ { "name": "Microsoft.NETCore.App", "version": "10.0.11" } ] } }""";
    private const string NoDynamicCode = """{ "runtimeOptions": { "tfm": "net10.0", "includedFrameworks": [ { "name": "Microsoft.NETCore.App", "version": "10.0.11" } ], "configProperties": { "System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported": false } } }""";

    [TestMethod]
    public void ASelfContainedJitBinaryIsRefused()
    {
        string? why = BinaryShape.JitProblem(SelfContained);

        Assert.IsNotNull(why, "a self-contained binary was accepted as the framework-dependent JIT column");
        StringAssert.Contains(why, "self-contained");
    }

    private const string FrameworkDependentNoDynamicCode = """{ "runtimeOptions": { "tfm": "net10.0", "framework": { "name": "Microsoft.NETCore.App", "version": "10.0.0" }, "configProperties": { "System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported": false } } }""";

    [TestMethod]
    public void AJitBinaryWithoutDynamicCodeIsRefused() =>
        StringAssert.Contains(BinaryShape.JitProblem(FrameworkDependentNoDynamicCode) ?? "", "IsDynamicCodeSupported");

    [TestMethod]
    public void AFrameworkDependentJitBinaryIsAdmitted() => Assert.IsNull(BinaryShape.JitProblem(FrameworkDependent));

    [TestMethod]
    public void AMissingRuntimeConfigIsRefused() => Assert.AreEqual("runtimeconfig.json not found", BinaryShape.JitProblem(null));

    [TestMethod]
    public void AFrameworkDependentReadyToRunBinaryIsRefused() =>
        StringAssert.Contains(BinaryShape.ReadyToRunProblem(FrameworkDependent) ?? "", "framework-dependent");

    [TestMethod]
    public void ANativeAotShapedBinaryIsNotTheReadyToRunColumn() =>
        StringAssert.Contains(BinaryShape.ReadyToRunProblem(NoDynamicCode) ?? "", "Native AOT shape");

    [TestMethod]
    public void TheTemplatesSelfContainedReadyToRunPublishIsAdmitted() => Assert.IsNull(BinaryShape.ReadyToRunProblem(SelfContained));

    // ---- the consumer project ----

    private static string Project(string references) =>
        $"<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <ItemGroup>\r\n{references}  </ItemGroup>\r\n</Project>\r\n";

    [TestMethod]
    public void AProjectReferenceIntoTheLocalStandardLibraryIsRefused()
    {
        string csproj = Project("    <ProjectReference Include=\"$(go2csPath)core/fmt/fmt.csproj\" />\r\n    <PackageReference Include=\"go.lib\" Version=\"1.26.10.1\" />\r\n");
        IReadOnlyList<string> refusals = ConsumerProject.Refusals("PerfFib", csproj, s_leg);

        Assert.AreEqual(1, refusals.Count, string.Join("; ", refusals));
        StringAssert.Contains(refusals[0], "1.26/PerfFib");
        StringAssert.Contains(refusals[0], "core/fmt/fmt.csproj");
    }

    [TestMethod]
    public void APackageAtAnotherVersionIsRefused()
    {
        string csproj = Project("    <PackageReference Include=\"go.lib\" Version=\"1.26.10.1\" />\r\n    <PackageReference Include=\"go.fmt\" Version=\"1.24.13.7\" />\r\n");
        IReadOnlyList<string> refusals = ConsumerProject.Refusals("PerfFib", csproj, s_leg);

        Assert.AreEqual(1, refusals.Count, string.Join("; ", refusals));
        StringAssert.Contains(refusals[0], "go.fmt 1.24.13.7");
    }

    [TestMethod]
    public void AFloatingReferenceCoveringTheLegIsAdmittedAndOneThatDoesNotIsRefused()
    {
        string covering = Project("    <PackageReference Include=\"go.lib\" Version=\"1.26.10.*\" />\r\n");
        string other = Project("    <PackageReference Include=\"go.lib\" Version=\"1.24.13.*\" />\r\n");

        Assert.AreEqual(0, ConsumerProject.Refusals("PerfFib", covering, s_leg).Count);
        StringAssert.Contains(ConsumerProject.Refusals("PerfFib", other, s_leg).Single(), "does not cover");
    }

    // What a release's converter writes: every go.* version is $(GoStdLibVersion), set in Directory.Build.props.
    private static readonly string s_propertyProject = Project(
        "    <PackageReference Include=\"go.gen\" Version=\"$(GoStdLibVersion)\" PrivateAssets=\"all\" />\r\n" +
        "    <PackageReference Include=\"go.lib\" Version=\"$(GoStdLibVersion)\" />\r\n" +
        "    <PackageReference Include=\"go.fmt\" Version=\"$(GoStdLibVersion)\" />\r\n");

    [TestMethod]
    public void TheVersionPropertyIsNamedOnce() =>
        CollectionAssert.AreEqual(new[] { "GoStdLibVersion" }, ConsumerProject.VersionProperties(s_propertyProject).ToArray());

    [TestMethod]
    public void APropertyVersionIsJudgedByItsEvaluatedValue()
    {
        Dictionary<string, string> covering = new() { ["GoStdLibVersion"] = "1.26.10.*" };
        Dictionary<string, string> other = new() { ["GoStdLibVersion"] = "1.24.13.*" };

        Assert.AreEqual(0, ConsumerProject.Refusals("PerfFib", s_propertyProject, s_leg, covering).Count);
        Assert.AreEqual(3, ConsumerProject.Refusals("PerfFib", s_propertyProject, s_leg, other).Count(r => r.Contains("does not cover")));
    }

    [TestMethod]
    public void APropertyTheEvaluationDidNotAnswerIsRefusedByName()
    {
        IReadOnlyList<string> refusals = ConsumerProject.Refusals("PerfFib", s_propertyProject, s_leg, new Dictionary<string, string>());

        Assert.AreEqual(3, refusals.Count, string.Join("; ", refusals));
        StringAssert.Contains(refusals[0], "gave GoStdLibVersion no value");
    }

    private static string Assets(params string[] libraries) =>
        "{ \"version\": 3, \"libraries\": { " + string.Join(", ", libraries.Select(l => $"\"{l}\": {{ \"type\": \"package\" }}")) + " } }";

    [TestMethod]
    public void APackageRestoredAtAnotherVersionIsRefused()
    {
        IReadOnlyList<string> refusals = ConsumerProject.ResolvedRefusals("PerfFib", Assets("go.lib/1.26.10.1", "go.fmt/1.26.10.2", "System.Memory/4.5.5"), s_leg);

        Assert.AreEqual(1, refusals.Count, string.Join("; ", refusals));
        StringAssert.Contains(refusals[0], "restored go.fmt 1.26.10.2");
    }

    [TestMethod]
    public void TheRestoreAtTheLegsVersionIsAdmittedAndAnUnrestoredProjectIsNot()
    {
        Assert.AreEqual(0, ConsumerProject.ResolvedRefusals("PerfFib", Assets("go.lib/1.26.10.1", "go.gen/1.26.10.1", "go.fmt/1.26.10.1"), s_leg).Count);
        StringAssert.Contains(ConsumerProject.ResolvedRefusals("PerfFib", null, s_leg).Single(), "never restored");
        StringAssert.Contains(ConsumerProject.ResolvedRefusals("PerfFib", Assets("System.Memory/4.5.5"), s_leg).Single(), "no go.* package");
    }

    [TestMethod]
    public void AProjectWithNoGo2csPackageIsRefused() =>
        StringAssert.Contains(ConsumerProject.Refusals("PerfFib", Project(""), s_leg).Single(), "not a -recurse=nuget conversion");

    [TestMethod]
    public void TheConsumerProjectAtTheLegsVersionIsAdmitted()
    {
        string csproj = Project("    <PackageReference Include=\"go.lib\" Version=\"1.26.10.1\" />\r\n    <PackageReference Include=\"go.gen\" Version=\"1.26.10.1\" />\r\n    <PackageReference Include=\"go.fmt\" Version=\"1.26.10.1\" />\r\n    <PackageReference Include=\"Some.Other\" Version=\"2.0.0\" />\r\n");
        Assert.AreEqual(0, ConsumerProject.Refusals("PerfFib", csproj, s_leg).Count);
    }

    [TestMethod]
    public void TheSourceHashIgnoresCarriageReturnsAndOrderButNotPathsOrContent()
    {
        string a = ConsumerProject.SourceHash([("PerfFib.cs", "line\r\nnext\r\n"), ("PerfFib.csproj", "<Project />\r\n")]);
        string b = ConsumerProject.SourceHash([("PerfFib.csproj", "<Project />\n"), ("PerfFib.cs", "line\nnext\n")]);
        string renamed = ConsumerProject.SourceHash([("Other.cs", "line\nnext\n"), ("PerfFib.csproj", "<Project />\n")]);
        string edited = ConsumerProject.SourceHash([("PerfFib.cs", "line\nnext!\n"), ("PerfFib.csproj", "<Project />\n")]);

        Assert.AreEqual(a, b, "the same program on two boxes, CRLF against LF and a different listing order, hashed differently");
        Assert.AreNotEqual(a, renamed);
        Assert.AreNotEqual(a, edited);
    }

    [TestMethod]
    public void BuildOutputAndGeneratedCopiesAreNotTheProgram()
    {
        Assert.IsTrue(ConsumerProject.IsProgramFile("PerfFib.cs"));
        Assert.IsTrue(ConsumerProject.IsProgramFile("PerfFib.csproj"));
        Assert.IsFalse(ConsumerProject.IsProgramFile(@"obj\Release\PerfFib.AssemblyInfo.cs"));
        Assert.IsFalse(ConsumerProject.IsProgramFile("bin/Release/x.cs"));
        Assert.IsFalse(ConsumerProject.IsProgramFile(@"Generated\go2cs-gen\x.g.cs"));
        Assert.IsFalse(ConsumerProject.IsProgramFile("go2cs.ico"));
    }

    // ---- the leg ----

    [TestMethod]
    public void ALegNamesItsConverterVersionAndGo()
    {
        ReleaseLeg leg = ReleaseLeg.Parse(@"name=1.26; go2cs=C:\tools\go2cs.exe; version=1.26.10.1; go=C:\sdk\go1.26.10\bin\go.exe");
        Assert.AreEqual(s_leg, leg);

        Assert.ThrowsException<FormatException>(() => ReleaseLeg.Parse(@"name=1.26;go2cs=C:\tools\go2cs.exe;go=C:\go.exe"));
        Assert.ThrowsException<FormatException>(() => ReleaseLeg.Parse(@"name=1 26;go2cs=a;version=b;go=c"));
    }
}

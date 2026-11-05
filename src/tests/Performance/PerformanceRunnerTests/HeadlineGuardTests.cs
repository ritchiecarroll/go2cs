// HeadlineGuardTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerformanceRunner;

namespace PerformanceRunnerTests;

/// <summary>
/// The headline guard (HeadlineSettings.cs): docs/Performance.md says nothing is tuned for the benchmarks, so a headline
/// column is built and run with the converter's defaults, the project file it writes, and for Native AOT only the
/// standard publish set. Each test plants one opt-in where it could enter -- a command line, a build file, the
/// environment, a project file, the runtimeconfig.json -- and requires the refusal to name it. The clean inputs, the
/// repository's own build files and template among them, are the positive controls.
/// </summary>
[TestClass]
public class HeadlineGuardTests
{
    private const string Column = "C# (JIT)";

    // The repository root, found from this test's own location rather than spelled.
    private static readonly string s_srcRoot = FindSrcRoot();

    private static string FindSrcRoot()
    {
        for (string? dir = AppContext.BaseDirectory; dir is not null; dir = Path.GetDirectoryName(dir))
        {
            if (File.Exists(Path.Combine(dir, "go2cs", "csproj-template.xml")))
                return dir;
        }

        throw new InvalidOperationException("src/go2cs/csproj-template.xml not found above the test assembly");
    }

    private static string Template => File.ReadAllText(Path.Combine(s_srcRoot, "go2cs", "csproj-template.xml"));

    private static void AssertNames(IReadOnlyList<HeadlineRefusal> refusals, string name)
    {
        Assert.IsTrue(refusals.Count > 0, $"nothing refused; {name} was planted");
        Assert.IsTrue(refusals.Any(r => r.ToString().Contains(name, StringComparison.Ordinal)),
            $"no refusal names {name}: {string.Join(" | ", refusals)}");
    }

    // ---- the command lines ----

    private static IEnumerable<(HeadlineCommand Command, string Argv)> Builders() =>
    [
        (HeadlineCommand.InTreeTranspile, Headline.InTreeTranspile(@"C:\go2cs\src", @"C:\go2cs\src\tests\Performance\PerfFib")),
        (HeadlineCommand.GoBuild, Headline.GoBuild(@"C:\go2cs\bin\PerfFib.exe")),
        (HeadlineCommand.InTreeJitBuild, Headline.InTreeJitBuild(@"C:\go2cs\obj\_AllTargets.proj", "C:/go2cs/src/")),
        (HeadlineCommand.InTreeAotPublish, Headline.InTreeAotPublish(@"C:\go2cs\PerfFib\PerfFib.csproj", "C:/go2cs/src/", @"C:\go2cs\bin\aot")),
        (HeadlineCommand.ReleaseConvert, Headline.ReleaseConvert(@"D:\work\1.26\src\PerfFib", @"D:\work\1.26\out\PerfFib")),
        (HeadlineCommand.ReleaseJitBuild, Headline.ReleaseJitBuild(@"D:\work\out\PerfFib.csproj", @"D:\work\bin\jit\PerfFib\")),
        (HeadlineCommand.ReleaseReadyToRunPublish, Headline.ReleaseReadyToRunPublish(@"D:\work\r2r-src\PerfFib.csproj", @"D:\work\bin\r2r\PerfFib")),
    ];

    [TestMethod]
    public void EveryCommandLineABuilderWritesIsAdmitted()
    {
        foreach ((HeadlineCommand command, string argv) in Builders())
        {
            IReadOnlyList<HeadlineRefusal> refused = Headline.ArgvRefusals(Column, command, argv);
            Assert.AreEqual(0, refused.Count, $"{command} [{argv}]: {string.Join(" | ", refused)}");
        }

        Assert.AreEqual(Enum.GetValues<HeadlineCommand>().Length, Builders().Count(), "a headline command has no builder under test");
    }

    [TestMethod]
    public void AnOptInOnAHeadlineCommandLineIsRefusedByName()
    {
        AssertNames(Headline.ArgvRefusals(Column, HeadlineCommand.InTreeTranspile, Headline.InTreeTranspile("src", "proj") + " -hoist-literals"), "-hoist-literals");
        AssertNames(Headline.ArgvRefusals(Column, HeadlineCommand.ReleaseConvert, "-hoist-literals " + Headline.ReleaseConvert("src", "out")), "-hoist-literals");
        AssertNames(Headline.ArgvRefusals(Column, HeadlineCommand.InTreeJitBuild, Headline.InTreeJitBuild("all.proj", "src/") + " -p:GoCallCountingDelayMs=100"), "GoCallCountingDelayMs");
        AssertNames(Headline.ArgvRefusals(Column, HeadlineCommand.ReleaseReadyToRunPublish, Headline.ReleaseReadyToRunPublish("p.csproj", "out") + " \"-p:GoReadyToRunUserCode=true\""), "GoReadyToRunUserCode");
        AssertNames(Headline.ArgvRefusals(Column, HeadlineCommand.ReleaseReadyToRunPublish, Headline.ReleaseReadyToRunPublish("p.csproj", "out") + " --self-contained false"), "--self-contained");
        AssertNames(Headline.ArgvRefusals(Column, HeadlineCommand.InTreeAotPublish, "publish p.csproj -c Debug -p:PerfAot=true -o out"), "-c Debug");
        AssertNames(Headline.ArgvRefusals(Column, HeadlineCommand.InTreeJitBuild, "build all.proj -p:Configuration=Debug -p:go2csPath=src/"), "Configuration=Debug");
        AssertNames(Headline.ArgvRefusals(Column, HeadlineCommand.GoBuild, "build -gcflags=-B -o x.exe ."), "-gcflags=-B");
    }

    // A pinned leg (ReleaseLeg.Pin) sets exactly one property, GoStdLibVersion, at exactly its own version, on exactly
    // its release build and publish. Both directions: what the builder writes is admitted for that leg, and the same
    // property is refused for an unpinned leg, at another value, on another command, or beside another property.
    [TestMethod]
    public void APinnedLegCarriesExactlyItsOwnVersionAndNothingElse()
    {
        const string pin = "1.26.10.1";

        foreach ((HeadlineCommand command, Func<string?, string> build) in new (HeadlineCommand, Func<string?, string>)[]
                 {
                     (HeadlineCommand.ReleaseJitBuild, v => Headline.ReleaseJitBuild(@"D:\work\out\PerfFib.csproj", @"D:\work\bin\jit\PerfFib\", v)),
                     (HeadlineCommand.ReleaseReadyToRunPublish, v => Headline.ReleaseReadyToRunPublish(@"D:\work\r2r-src\PerfFib.csproj", @"D:\work\bin\r2r\PerfFib", v)),
                 })
        {
            string pinned = build(pin);
            StringAssert.Contains(pinned, $"-p:GoStdLibVersion={pin}");
            Assert.IsFalse(build(null).Contains("GoStdLibVersion", StringComparison.OrdinalIgnoreCase), $"{command}: an unpinned leg writes the property");

            IReadOnlyList<HeadlineRefusal> admitted = Headline.ArgvRefusals(Column, command, pinned, pin);
            Assert.AreEqual(0, admitted.Count, $"{command} [{pinned}]: {string.Join(" | ", admitted)}");

            AssertNames(Headline.ArgvRefusals(Column, command, pinned), $"GoStdLibVersion={pin}");
            AssertNames(Headline.ArgvRefusals(Column, command, build("1.26.10.2"), pin), "GoStdLibVersion=1.26.10.2");
            AssertNames(Headline.ArgvRefusals(Column, command, build("1.26.*"), pin), "GoStdLibVersion=1.26.*");
            AssertNames(Headline.ArgvRefusals(Column, command, pinned + " -p:GoReadyToRunUserCode=true", pin), "GoReadyToRunUserCode");
        }

        AssertNames(Headline.ArgvRefusals(Column, HeadlineCommand.InTreeJitBuild,
            Headline.InTreeJitBuild("all.proj", "src/") + Headline.Pin(pin), pin), $"GoStdLibVersion={pin}");
    }

    // The process, not the guard, decides what MSBuild receives. The JIT build's OutDir ends in a separator, so its
    // closing quote is written \" -- which Windows reads as a literal quote, swallowing everything after it into the
    // OutDir value. A pin written there reaches no restore (seen on the first integration run: the pinned leg restored
    // 1.26.10.2). So each pinned builder's pin must be its own argument as the Windows command line splits it.
    [TestMethod]
    public void APinIsItsOwnArgumentAsWindowsSplitsTheCommandLine()
    {
        const string pin = "1.26.10.1";

        foreach (string argv in new[]
                 {
                     Headline.ReleaseJitBuild(@"D:\work\out\PerfFib.csproj", @"D:\work\bin\jit\PerfFib\", pin),
                     Headline.ReleaseReadyToRunPublish(@"D:\work\r2r-src\PerfFib.csproj", @"D:\work\bin\r2r\PerfFib", pin),
                 })
        {
            List<string> args = WindowsArguments(argv);
            CollectionAssert.Contains(args, $"-p:GoStdLibVersion={pin}", $"[{argv}] splits as: {string.Join(" | ", args)}");
        }
    }

    // The Windows C runtime's rule (CommandLineToArgvW): 2n backslashes before a quote are n backslashes and the quote
    // toggles quoting; 2n+1 are n backslashes and a literal quote; backslashes elsewhere are literal.
    private static List<string> WindowsArguments(string commandLine)
    {
        List<string> args = new();
        System.Text.StringBuilder current = new();
        bool quoted = false, any = false;

        for (int i = 0; i < commandLine.Length; i++)
        {
            char c = commandLine[i];

            if (c == '\\')
            {
                int run = 0;

                while (i < commandLine.Length && commandLine[i] == '\\')
                {
                    run++;
                    i++;
                }

                if (i < commandLine.Length && commandLine[i] == '"')
                {
                    current.Append('\\', run / 2);

                    if (run % 2 == 1)
                        current.Append('"');
                    else
                        quoted = !quoted;
                }
                else
                {
                    current.Append('\\', run);
                    i--;
                }

                any = true;
            }
            else if (c == '"')
            {
                quoted = !quoted;
                any = true;
            }
            else if (char.IsWhiteSpace(c) && !quoted)
            {
                if (any)
                    args.Add(current.ToString());

                current.Clear();
                any = false;
            }
            else
            {
                current.Append(c);
                any = true;
            }
        }

        if (any)
            args.Add(current.ToString());

        return args;
    }

    // ---- the build files ----

    [TestMethod]
    public void TheNativeAotSetIsTheDocumentedOne() =>
        CollectionAssert.AreEquivalent(
            new[] { "PublishAot", "PublishReadyToRun", "PublishTrimmed", "RuntimeIdentifier", "SelfContained", "TrimMode" },
            Headline.BuildFiles["tests/Performance/Directory.Build.targets"].Keys.ToArray());

    [TestMethod]
    public void TheRepositorysBuildFilesPassTheirAllowlists()
    {
        foreach ((string relative, IReadOnlyDictionary<string, string?> allowed) in Headline.BuildFiles)
        {
            string path = Path.Combine(s_srcRoot, relative);
            Assert.IsTrue(File.Exists(path), $"{path} is listed but missing");

            IReadOnlyList<HeadlineRefusal> refused = Headline.BuildFileRefusals(relative, File.ReadAllText(path), allowed);
            Assert.AreEqual(0, refused.Count, string.Join(" | ", refused));
        }
    }

    [TestMethod]
    public void APlantedBuildFileSettingIsRefusedByName()
    {
        const string Props = "tests/Performance/Directory.Build.props";
        const string Targets = "tests/Performance/Directory.Build.targets";
        string props = File.ReadAllText(Path.Combine(s_srcRoot, Props));
        string targets = File.ReadAllText(Path.Combine(s_srcRoot, Targets));

        string planted = props.Replace("</Project>", "<PropertyGroup><GoCallCountingDelayMs>100</GoCallCountingDelayMs></PropertyGroup></Project>");
        AssertNames(Headline.BuildFileRefusals(Props, planted, Headline.BuildFiles[Props]), "GoCallCountingDelayMs");

        planted = targets.Replace("<TrimMode>partial</TrimMode>", "<TrimMode>full</TrimMode>");
        Assert.AreNotEqual(targets, planted, "the plant did not land");
        AssertNames(Headline.BuildFileRefusals(Targets, planted, Headline.BuildFiles[Targets]), "TrimMode");

        planted = targets.Replace("</Project>", "<ItemGroup><RuntimeHostConfigurationOption Include=\"System.GC.Server\" Value=\"true\" /></ItemGroup></Project>");
        AssertNames(Headline.BuildFileRefusals(Targets, planted, Headline.BuildFiles[Targets]), "<ItemGroup>");
    }

    [TestMethod]
    public void ABuildFileOutsideTheListedChainIsRefused()
    {
        // The guard walks up a real folder with Path.GetDirectoryName, which splits at this host's separators only,
        // so the folders are spelled from this host's root: a Windows spelling is one segment on linux and macOS.
        string root = Path.GetPathRoot(AppContext.BaseDirectory)!;
        string project = Path.Combine(root, "go2cs", "src", "tests", "Performance", "PerfFib");
        string rsp = Path.Combine(root, "go2cs", "Directory.Build.rsp");
        string shadow = Path.Combine(project, "Directory.Build.props");

        Assert.AreEqual(0, Headline.StrayBuildFiles(project, _ => false).Count);
        AssertNames(Headline.StrayBuildFiles(project, f => f == rsp), rsp);
        AssertNames(Headline.StrayBuildFiles(project, f => f == shadow), shadow);

        string work = Path.Combine(root, "perf", "work");
        string above = Path.Combine(root, "perf", "Directory.Build.props");
        Assert.AreEqual(0, Headline.BuildFilesAtOrAbove(work, _ => false).Count);
        AssertNames(Headline.BuildFilesAtOrAbove(work, f => f == above), above);
    }

    // ---- the environment ----

    [TestMethod]
    public void ATuningVariableIsRefusedAndHostPlumbingIsAdmitted()
    {
        Dictionary<string, string?> environment = new()
        {
            ["DOTNET_ROOT"] = @"C:\Program Files\dotnet", ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1", ["DOTNET_NOLOGO"] = "1",
            ["PATH"] = @"C:\Windows", ["GOROOT"] = @"C:\go",
        };

        Assert.AreEqual(0, Headline.EnvironmentRefusals(environment).Count);
        CollectionAssert.AreEqual(new[] { "DOTNET_CLI_TELEMETRY_OPTOUT", "DOTNET_NOLOGO", "DOTNET_ROOT" }, Headline.AdmittedHostVariables(environment).ToArray());

        foreach (string knob in new[] { "DOTNET_TieredPGO", "DOTNET_TC_CallCountingDelayMs", "COMPlus_TieredCompilation", "DOTNET_gcServer", "GoCallCountingDelayMs", "TieredPGO", "IlcOptimizationPreference", "GOGC", "GOAMD64" })
            AssertNames(Headline.EnvironmentRefusals(new Dictionary<string, string?>(environment) { [knob] = "0" }), knob);
    }

    // ---- what reaches the runtime ----

    private static string Config(string properties) =>
        $$"""{ "runtimeOptions": { "tfm": "net10.0", "framework": { "name": "Microsoft.NETCore.App", "version": "10.0.0" }, "configProperties": { {{properties}} } } }""";

    [TestMethod]
    public void TheTemplatesOwnCallCountingDelayIsReadFromTheProject()
    {
        Assert.AreEqual("0", Headline.TemplateCallCountingDelay(Template));
        Assert.IsNull(Headline.TemplateCallCountingDelay("<Project><PropertyGroup><OutputType>Exe</OutputType></PropertyGroup></Project>"));
    }

    [TestMethod]
    public void ARuntimeSettingTheTemplateDoesNotWriteIsRefused()
    {
        const string Delay0 = "\"System.Runtime.TieredCompilation.CallCountingDelayMs\": 0";
        const string Benign = "\"System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization\": false";

        Assert.AreEqual(0, Headline.RuntimeConfigRefusals(Column, "PerfFib", Config($"{Delay0}, {Benign}"), "0").Count);
        Assert.AreEqual(0, Headline.RuntimeConfigRefusals(Column, "PerfFib", Config(Benign), null).Count, "an older template writes no delay");

        AssertNames(Headline.RuntimeConfigRefusals(Column, "PerfFib", Config("\"System.Runtime.TieredCompilation.CallCountingDelayMs\": 100"), "0"), "CallCountingDelayMs: 100");
        AssertNames(Headline.RuntimeConfigRefusals(Column, "PerfFib", Config(Benign), "0"), "CallCountingDelayMs: absent");
        AssertNames(Headline.RuntimeConfigRefusals(Column, "PerfFib", Config(Delay0), null), "CallCountingDelayMs: 0");
        AssertNames(Headline.RuntimeConfigRefusals(Column, "PerfFib", Config($"{Delay0}, \"System.Runtime.TieredPGO\": false"), "0"), "System.Runtime.TieredPGO");
        AssertNames(Headline.RuntimeConfigRefusals(Column, "PerfFib", Config($"{Delay0}, \"System.GC.Server\": true"), "0"), "System.GC.Server");
        AssertNames(Headline.RuntimeConfigRefusals(Column, "PerfFib", null, "0"), "not found");
    }

    // ---- an in-tree project file ----

    private const string Converted = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>Exe</OutputType>
            <AssemblyName>PerfFib</AssemblyName>
            <Nullable>annotations</Nullable>
          </PropertyGroup>
          <PropertyGroup Condition="'$(OutputType)'!='Library'">
            <PublishReadyToRun>true</PublishReadyToRun>
            <TrimMode Condition="'$(TrimMode)'==''">partial</TrimMode>
          </PropertyGroup>
        </Project>
        """;

    [TestMethod]
    public void AProjectFileIsHeldToTheConvertersTemplate()
    {
        Assert.AreEqual(0, Headline.ProjectFileRefusals("PerfFib.csproj", Converted, Template).Count);

        AssertNames(Headline.ProjectFileRefusals("PerfFib.csproj", Converted.Replace("</Project>", "<PropertyGroup><TieredPGO>false</TieredPGO></PropertyGroup></Project>"), Template), "TieredPGO");
        AssertNames(Headline.ProjectFileRefusals("PerfFib.csproj", Converted.Replace(">partial<", ">full<"), Template), "TrimMode");
        AssertNames(Headline.ProjectFileRefusals("PerfFib.csproj", Converted.Replace("</Project>", "<PropertyGroup><GoCallCountingDelayMs>100</GoCallCountingDelayMs></PropertyGroup></Project>"), Template), "GoCallCountingDelayMs");
    }

    // ---- the ReadyToRun column's shape, as the converted project's template publishes it ----

    // CoreLib is precompiled; this test assembly is a plain build, IL only.
    private static readonly string s_readyToRun = typeof(object).Assembly.Location;
    private static readonly string s_ilOnly = typeof(HeadlineGuardTests).Assembly.Location;

    [TestMethod]
    public void TheTemplateFromTheDot2ReleaseKeepsOwnCodeOutOfReadyToRun()
    {
        Assert.IsTrue(BinaryShape.KeepsOwnCodeOutOfReadyToRun(Template));
        Assert.IsFalse(BinaryShape.KeepsOwnCodeOutOfReadyToRun(Converted));
    }

    [TestMethod]
    public void OwnCodeILOnlyWithGolibReadyToRunIsTheDot2Shape()
    {
        Assert.IsNull(BinaryShape.ReadyToRunShapeProblem(s_ilOnly, s_readyToRun, ownCodeIlOnly: true, out string shape));
        Assert.AreEqual("own code IL-only, golib ReadyToRun", shape);

        StringAssert.Contains(BinaryShape.ReadyToRunShapeProblem(s_readyToRun, s_readyToRun, ownCodeIlOnly: true, out _) ?? "", "carries ReadyToRun code");
        StringAssert.Contains(BinaryShape.ReadyToRunShapeProblem(s_ilOnly, s_ilOnly, ownCodeIlOnly: true, out _) ?? "", "carries no ReadyToRun code");

        // The pre-.2 check, which the .2 leg's own-code-IL-only publish would have failed.
        StringAssert.Contains(BinaryShape.ReadyToRunCodeProblem(s_ilOnly) ?? "", "carries no ReadyToRun code");
    }

    [TestMethod]
    public void OwnCodeReadyToRunIsTheOlderTemplatesShape()
    {
        Assert.IsNull(BinaryShape.ReadyToRunShapeProblem(s_readyToRun, s_ilOnly, ownCodeIlOnly: false, out string shape));
        Assert.AreEqual("own code ReadyToRun", shape);
        StringAssert.Contains(BinaryShape.ReadyToRunShapeProblem(s_ilOnly, s_readyToRun, ownCodeIlOnly: false, out _) ?? "", "carries no ReadyToRun code");
    }

    [TestMethod]
    public void EveryRefusalPrintsInTheOneShape()
    {
        HeadlineRefusal refusal = Headline.ArgvRefusals(Column, HeadlineCommand.InTreeTranspile, "-go2cspath src proj -hoist-literals").Single();
        Assert.AreEqual("REFUSED [C# (JIT)] InTreeTranspile command line: -hoist-literals; expected only -go2cspath, with configuration Release", refusal.ToString());
    }
}

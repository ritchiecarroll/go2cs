// ReleaseRun.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The post-hop re-baseline's release legs (docs/PLAN-perf-rebaseline.md, step S0). Where the in-tree mode measures
// the suite's own projects against src\core, a release leg measures a PUBLISHED release as a consumer gets it: each
// benchmark is converted by that release's converter with -recurse=nuget, so its project references go.lib, go.gen
// and the go.* packages from NuGet at the leg's version. Two legs measured in one session give the runbook's paired
// same-session comparison (docs/DotNetMigration.md section 6).
//
// Per leg and program the arms are: Go (the leg's Go toolchain); JIT (framework-dependent `dotnet build`); R2R (the
// converter template's own `dotnet publish -c Release -r win-x64`); AOT (a binary published on another box, admitted
// only by its manifest); and TC0, a DIAGNOSTIC that runs the JIT binary under DOTNET_TieredCompilation=0 and never
// reaches the published table. Every arm's binary is verified against the leg's Go output, and both legs' Go outputs
// against each other, before anything is timed. The legs are measured interleaved, A B B A, so drift falls on both.
//
// What this mode refuses, each by name (the decisions are ReleaseLegs.cs, guarded by PerformanceRunnerTests): a
// converted project that is not the published packages at the leg's version; a JIT binary that is not
// framework-dependent; an R2R binary that is not the template's self-contained publish; and any AOT binary the
// manifest does not vouch for (no entry, a different hash or size, or a different converted program).

using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PerformanceRunner
{
    // Tc0 and Fd1 are diagnostic arms (the plan's section 2): recorded, never in the published table.
    internal enum ReleaseArm { Go, Jit, ReadyToRun, Aot, Tc0, Fd1 }

    internal static class ReleaseRun
    {
        private const int ConvertTimeoutMs = 1_800_000;
        private const int BuildTimeoutMs = 1_800_000;

        // The rows a regression cannot be read from a median of 5 alone (the plan's PERF-REVIEW list): they take
        // --review-runs runs and report their spread.
        private static readonly HashSet<string> s_reviewRows = new(StringComparer.OrdinalIgnoreCase)
        {
            "PerfStartup", "PerfSort", "PerfRefLower", "PerfIface", "PerfIfaceCall", "PerfChannel"
        };

        private sealed class Slot
        {
            public string? Exe { get; set; }
            public Dictionary<string, string>? Environment { get; set; }
            public bool Admitted { get; set; }
            public string? Why { get; set; }
            public List<RunSample> Samples { get; } = new();
        }

        private sealed class LegProgram
        {
            public required string WorkDir { get; init; }
            public string? SourceHash { get; set; }
            public Dictionary<ReleaseArm, Slot> Slots { get; } = Enum.GetValues<ReleaseArm>().ToDictionary(a => a, _ => new Slot());
        }

        internal sealed record Options(
            IReadOnlyList<ReleaseLeg> Legs, string WorkRoot, string? AotManifestPath, string? AotDropRoot,
            int Runs, int ReviewRuns, bool Tc0, bool Fd1);

        internal static int Run(string perfDir, IReadOnlyList<string> programs, Options options)
        {
            Directory.CreateDirectory(options.WorkRoot);
            Console.WriteLine($"[Release]  legs [{string.Join(", ", options.Legs.Select(l => $"{l.Name} = {l.PackageVersion}"))}], work {options.WorkRoot}");

            IReadOnlyList<AotManifestEntry> manifest = options.AotManifestPath is null
                ? []
                : AotManifest.Parse(File.ReadAllText(options.AotManifestPath));

            Dictionary<(string Leg, string Program), LegProgram> table = new();
            List<string> refusals = new();

            foreach (ReleaseLeg leg in options.Legs)
            {
                StringBuilder sources = new();

                foreach (string program in programs)
                {
                    LegProgram lp = PrepareProgram(perfDir, program, leg, options, manifest, refusals);
                    table[(leg.Name, program)] = lp;

                    if (lp.SourceHash is not null)
                        sources.Append(program).Append('\t').Append(lp.SourceHash).Append('\n');
                }

                // The other half of the AOT handoff: the box that publishes compares its own converted-source hashes to
                // these, and the manifest it ships back carries them.
                File.WriteAllText(Path.Combine(options.WorkRoot, $"sources-{leg.Name}.tsv"), sources.ToString());
            }

            Verify(programs, options.Legs, table, refusals);
            Measure(programs, options, table);

            string report = Report(programs, options, table, refusals);
            string reportPath = Path.Combine(options.WorkRoot, "report.md");
            File.WriteAllText(reportPath, report);
            Console.WriteLine();
            Console.WriteLine(report);
            Console.WriteLine($"[Release]  report written to {reportPath}");

            return refusals.Count == 0 ? 0 : 1;
        }

        // ---- one leg, one program: convert, check, build, admit ----

        private static LegProgram PrepareProgram(string perfDir, string program, ReleaseLeg leg, Options options,
            IReadOnlyList<AotManifestEntry> manifest, List<string> refusals)
        {
            string legRoot = Path.Combine(options.WorkRoot, leg.Name);
            string source = Path.Combine(legRoot, "src", program);
            string output = Path.Combine(legRoot, "out", program);
            LegProgram lp = new() { WorkDir = source };

            void Refuse(ReleaseArm arm, string why)
            {
                lp.Slots[arm].Why = why;
                refusals.Add(why);
            }

            // A refusal of the program's C# as a whole: every C# arm carries it, but the refused list reads it once.
            void RefuseCSharp(string why)
            {
                foreach (ReleaseArm arm in Enum.GetValues<ReleaseArm>().Where(a => a != ReleaseArm.Go))
                {
                    lp.Slots[arm].Admitted = false;
                    lp.Slots[arm].Why = why;
                }

                refusals.Add(why);
            }

            Console.Write($"           {leg.Name}/{program}: convert... ");

            // The program's Go module alone, never its committed C#: the leg's converter writes the whole consumer tree.
            RecreateDirectory(source);
            RecreateDirectory(output);

            foreach (string file in Directory.GetFiles(Path.Combine(perfDir, program)).Where(f => f.EndsWith(".go", StringComparison.Ordinal) || Path.GetFileName(f) == "go.mod"))
                File.Copy(file, Path.Combine(source, Path.GetFileName(file)));

            Runner.ProcResult convert = Runner.Exec(leg.Converter, $"-recurse=nuget \"{source}\" \"{output}\"", source, ConvertTimeoutMs, GoEnvironment(leg));

            if (convert.ExitCode != 0)
            {
                RefuseCSharp($"{leg.Name}/{program}: conversion exit {convert.ExitCode}: {Runner.Truncate(convert.StdOut + convert.StdErr)}");

                Console.WriteLine("FAILED");
                BuildGo(program, leg, source, legRoot, lp, Refuse);
                return lp;
            }

            string? csproj = FindProgramProject(output, program);

            if (csproj is null)
            {
                RefuseCSharp($"{leg.Name}/{program}: the conversion wrote no project for the program under {output}");

                Console.WriteLine("no project");
                BuildGo(program, leg, source, legRoot, lp, Refuse);
                return lp;
            }

            string projectDir = Path.GetDirectoryName(csproj)!;
            // Versions are read as MSBuild evaluates them: the converter writes $(GoStdLibVersion), set in the
            // Directory.Build.props beside the program, and only an evaluation sees its conditions and imports.
            string csprojText = File.ReadAllText(csproj);
            IReadOnlyList<string> consumer = ConsumerProject.Refusals(program, csprojText, leg,
                EvaluatedProperties(csproj, projectDir, ConsumerProject.VersionProperties(csprojText)));

            lp.SourceHash = ConsumerProject.SourceHash(Directory.GetFiles(projectDir, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(projectDir, f))
                .Where(ConsumerProject.IsProgramFile)
                .Select(rel => (rel, File.ReadAllText(Path.Combine(projectDir, rel)))));

            Console.WriteLine($"source {lp.SourceHash[..12]}");

            BuildGo(program, leg, source, legRoot, lp, Refuse);

            if (consumer.Count > 0)
            {
                // Not the published release as a consumer gets it: no C# arm of this program is timed.
                RefuseCSharp(string.Join("; ", consumer));

                return lp;
            }

            BuildJit(program, leg, projectDir, csproj, legRoot, lp, options.Tc0, Refuse);

            // The build restored the packages: a floating reference is the leg's release only if it RESOLVED to it.
            // Where the restore wrote it, as MSBuild evaluates it: the release template redirects obj out of the
            // project tree (Directory.Build.props, BaseIntermediateOutputPath under .artifacts), so obj\ is a guess.
            string assets = EvaluatedProperties(csproj, projectDir, ["ProjectAssetsFile"]).TryGetValue("ProjectAssetsFile", out string? evaluated)
                ? evaluated
                : Path.Combine(projectDir, "obj", "project.assets.json");
            IReadOnlyList<string> resolved = ConsumerProject.ResolvedRefusals(program, File.Exists(assets) ? File.ReadAllText(assets) : null, leg);

            if (resolved.Count > 0)
            {
                RefuseCSharp(string.Join("; ", resolved));

                return lp;
            }

            PublishReadyToRun(program, leg, output, csproj, legRoot, lp, Refuse);
            PublishFrameworkDependentReadyToRun(program, leg, output, csproj, legRoot, lp, options.Fd1, Refuse);
            AdmitAot(program, leg, options, manifest, lp, Refuse);
            return lp;
        }

        private static void BuildGo(string program, ReleaseLeg leg, string source, string legRoot, LegProgram lp, Action<ReleaseArm, string> refuse)
        {
            string exe = Path.Combine(legRoot, "bin", "go", program + ".exe");
            Directory.CreateDirectory(Path.GetDirectoryName(exe)!);
            Runner.ProcResult r = Runner.Exec(leg.Go, $"build -o \"{exe}\" .", source, BuildTimeoutMs, GoEnvironment(leg));

            if (r.ExitCode != 0)
            {
                refuse(ReleaseArm.Go, $"{leg.Name}/{program}: go build exit {r.ExitCode}: {Runner.Truncate(r.StdErr)}");
                return;
            }

            lp.Slots[ReleaseArm.Go].Exe = exe;
            lp.Slots[ReleaseArm.Go].Admitted = true;
        }

        private static void BuildJit(string program, ReleaseLeg leg, string projectDir, string csproj, string legRoot, LegProgram lp, bool tc0, Action<ReleaseArm, string> refuse)
        {
            string outDir = Path.Combine(legRoot, "bin", "jit", program) + Path.DirectorySeparatorChar;
            Runner.ProcResult r = Runner.Exec("dotnet", $"build \"{csproj}\" -nologo -clp:ErrorsOnly -c Release \"-p:OutDir={outDir}\"", projectDir, BuildTimeoutMs);

            if (r.ExitCode != 0)
            {
                refuse(ReleaseArm.Jit, $"{leg.Name}/{program}: JIT build exit {r.ExitCode}: {Runner.Truncate(r.StdOut + r.StdErr)}");
                refuse(ReleaseArm.Tc0, $"{leg.Name}/{program}: no JIT binary for the TC0 diagnostic");
                return;
            }

            string exe = Path.Combine(outDir, AssemblyName(csproj, program) + ".exe");
            string configPath = Path.ChangeExtension(exe, ".runtimeconfig.json");
            string? problem = BinaryShape.JitProblem(File.Exists(configPath) ? File.ReadAllText(configPath) : null);

            if (problem is not null || !File.Exists(exe))
            {
                refuse(ReleaseArm.Jit, $"{leg.Name}/{program}: JIT binary rejected: {problem ?? "exe missing"} [{exe}]");
                refuse(ReleaseArm.Tc0, $"{leg.Name}/{program}: no JIT binary for the TC0 diagnostic");
                return;
            }

            lp.Slots[ReleaseArm.Jit].Exe = exe;
            lp.Slots[ReleaseArm.Jit].Admitted = true;

            if (tc0)
            {
                lp.Slots[ReleaseArm.Tc0].Exe = exe;
                lp.Slots[ReleaseArm.Tc0].Environment = new() { ["DOTNET_TieredCompilation"] = "0" };
                lp.Slots[ReleaseArm.Tc0].Admitted = true;
            }
            else
            {
                lp.Slots[ReleaseArm.Tc0].Why = "not requested (--tc0)";
            }
        }

        // The template-default publish, nothing added: what a consumer gets from `dotnet publish -c Release -r
        // win-x64` on the converter-written project. Published from its own copy, because a RID-specific self-contained
        // restore and the framework-dependent build cross-pollute one obj\. The copy is the whole converted tree, not
        // the project folder: the Directory.Build.props above the project sets $(GoStdLibVersion) and redirects obj.
        private static void PublishReadyToRun(string program, ReleaseLeg leg, string output, string csproj, string legRoot, LegProgram lp, Action<ReleaseArm, string> refuse)
        {
            string copy = Path.Combine(legRoot, "r2r-src", program);
            RecreateDirectory(copy);
            CopyProgram(output, copy);

            string copiedProject = Path.Combine(copy, Path.GetRelativePath(output, csproj));
            string outDir = Path.Combine(legRoot, "bin", "r2r", program);
            Runner.ProcResult r = Runner.Exec("dotnet", $"publish \"{copiedProject}\" -nologo -clp:ErrorsOnly -c Release -r win-x64 -o \"{outDir}\"", copy, BuildTimeoutMs);

            if (r.ExitCode != 0)
            {
                refuse(ReleaseArm.ReadyToRun, $"{leg.Name}/{program}: ReadyToRun publish exit {r.ExitCode}: {Runner.Truncate(r.StdOut + r.StdErr)}");
                return;
            }

            string exe = Path.Combine(outDir, AssemblyName(csproj, program) + ".exe");
            string configPath = Path.ChangeExtension(exe, ".runtimeconfig.json");
            string? problem = BinaryShape.ReadyToRunProblem(File.Exists(configPath) ? File.ReadAllText(configPath) : null)
                ?? BinaryShape.ReadyToRunCodeProblem(Path.ChangeExtension(exe, ".dll"));

            if (problem is not null || !File.Exists(exe))
            {
                refuse(ReleaseArm.ReadyToRun, $"{leg.Name}/{program}: ReadyToRun binary rejected: {problem ?? "exe missing"} [{exe}]");
                return;
            }

            lp.Slots[ReleaseArm.ReadyToRun].Exe = exe;
            lp.Slots[ReleaseArm.ReadyToRun].Admitted = true;
        }

        // The diagnostic fifth arm (P1's consumer-side ReadyToRun DATA, the plan's section 2): ReadyToRun without the
        // template's trim, so framework-dependent, the recipe P1 measured as fd1. Recorded, never published; whether
        // the template changes is a separate ruling. Its own copy of the tree, as for the ReadyToRun column.
        private static void PublishFrameworkDependentReadyToRun(string program, ReleaseLeg leg, string output, string csproj, string legRoot,
            LegProgram lp, bool requested, Action<ReleaseArm, string> refuse)
        {
            if (!requested)
            {
                lp.Slots[ReleaseArm.Fd1].Why = "not requested (--fd1)";
                return;
            }

            string copy = Path.Combine(legRoot, "fd1-src", program);
            RecreateDirectory(copy);
            CopyProgram(output, copy);

            string copiedProject = Path.Combine(copy, Path.GetRelativePath(output, csproj));
            string outDir = Path.Combine(legRoot, "bin", "fd1", program);
            Runner.ProcResult r = Runner.Exec("dotnet",
                $"publish \"{copiedProject}\" -nologo -clp:ErrorsOnly -c Release -r win-x64 --self-contained false -p:PublishTrimmed=false -p:PublishReadyToRun=true -o \"{outDir}\"",
                copy, BuildTimeoutMs);

            if (r.ExitCode != 0)
            {
                refuse(ReleaseArm.Fd1, $"{leg.Name}/{program}: framework-dependent ReadyToRun publish exit {r.ExitCode}: {Runner.Truncate(r.StdOut + r.StdErr)}");
                return;
            }

            string exe = Path.Combine(outDir, AssemblyName(csproj, program) + ".exe");
            string configPath = Path.ChangeExtension(exe, ".runtimeconfig.json");
            string? problem = BinaryShape.JitProblem(File.Exists(configPath) ? File.ReadAllText(configPath) : null)
                ?? BinaryShape.ReadyToRunCodeProblem(Path.ChangeExtension(exe, ".dll"));

            if (problem is not null || !File.Exists(exe))
            {
                refuse(ReleaseArm.Fd1, $"{leg.Name}/{program}: framework-dependent ReadyToRun binary rejected: {problem ?? "exe missing"} [{exe}]");
                return;
            }

            lp.Slots[ReleaseArm.Fd1].Exe = exe;
            lp.Slots[ReleaseArm.Fd1].Admitted = true;
        }

        private static void AdmitAot(string program, ReleaseLeg leg, Options options, IReadOnlyList<AotManifestEntry> manifest, LegProgram lp, Action<ReleaseArm, string> refuse)
        {
            if (options.AotDropRoot is null)
            {
                lp.Slots[ReleaseArm.Aot].Why = "no --aot-drop given";
                return;
            }

            string exe = Path.Combine(options.AotDropRoot, leg.Name, program + ".exe");
            bool present = File.Exists(exe);
            AotManifestEntry? entry = manifest.FirstOrDefault(e => e.Leg == leg.Name && e.Program == program);
            string? sha256 = present ? Sha256Of(exe) : null;
            long size = present ? new FileInfo(exe).Length : 0;
            string? why = AotManifest.Refusal(leg.Name, program, entry, present, sha256, size, lp.SourceHash ?? "");

            if (why is not null)
            {
                refuse(ReleaseArm.Aot, why);
                return;
            }

            lp.Slots[ReleaseArm.Aot].Exe = exe;
            lp.Slots[ReleaseArm.Aot].Admitted = true;
        }

        // ---- verify, measure, report ----

        private static void Verify(IReadOnlyList<string> programs, IReadOnlyList<ReleaseLeg> legs, Dictionary<(string, string), LegProgram> table, List<string> refusals)
        {
            Console.Write("[Verify]   every admitted binary against its leg's Go output, and the legs' Go outputs against each other... ");
            int failed = 0;

            foreach (string program in programs)
            {
                string? reference = null;
                string? referenceLeg = null;

                foreach (ReleaseLeg leg in legs)
                {
                    LegProgram lp = table[(leg.Name, program)];

                    foreach (ReleaseArm arm in Enum.GetValues<ReleaseArm>())
                    {
                        Slot slot = lp.Slots[arm];

                        if (!slot.Admitted)
                            continue;

                        Runner.ProcResult r = Runner.Exec(slot.Exe!, null, lp.WorkDir, 120_000, slot.Environment);
                        string output = Runner.FilterTimingLines(r.StdOut);
                        string? why = null;

                        if (r.ExitCode != 0)
                            why = $"{leg.Name}/{program} {arm}: exit {r.ExitCode}: {Runner.Truncate(r.StdErr)}";
                        else if (reference is null)
                            (reference, referenceLeg) = (output, $"{leg.Name} {arm}");
                        else if (!string.Equals(output, reference, StringComparison.Ordinal))
                            why = $"{leg.Name}/{program} {arm}: output differs from {referenceLeg}: [{Runner.Truncate(output, 120)}] vs [{Runner.Truncate(reference, 120)}]";

                        if (why is not null)
                        {
                            slot.Admitted = false;
                            slot.Why = why;
                            refusals.Add(why);
                            failed++;
                        }
                    }
                }
            }

            Console.WriteLine(failed == 0 ? "ok" : $"{failed} REFUSED");
        }

        private static void Measure(IReadOnlyList<string> programs, Options options, Dictionary<(string, string), LegProgram> table)
        {
            Console.WriteLine($"[Measure]  legs interleaved A B B A; {options.Runs} run(s) per binary, {options.ReviewRuns} for the review rows; 1 discarded warmup each");

            foreach (string program in programs)
            {
                int runs = s_reviewRows.Contains(program) ? options.ReviewRuns : options.Runs;
                Console.Write($"           {program}... ");

                foreach (ReleaseArm arm in Enum.GetValues<ReleaseArm>())
                {
                    List<(ReleaseLeg Leg, Slot Slot)> legs = options.Legs
                        .Select(l => (l, table[(l.Name, program)].Slots[arm]))
                        .Where(x => x.Item2.Admitted)
                        .ToList();

                    foreach ((ReleaseLeg _, Slot slot) in legs)
                        Runner.RunMeasured(slot.Exe!, table[(options.Legs[0].Name, program)].WorkDir, slot.Environment);

                    for (int i = 0; i < runs; i++)
                    {
                        // A B, then B A: each pair's order alternates, so neither leg always runs warmer.
                        IEnumerable<(ReleaseLeg Leg, Slot Slot)> order = (i % 2 == 0) ? legs : Enumerable.Reverse(legs);

                        foreach ((ReleaseLeg leg, Slot slot) in order)
                        {
                            (int exitCode, string stdOut, double wallMs, long peakBytes) =
                                Runner.RunMeasured(slot.Exe!, table[(leg.Name, program)].WorkDir, slot.Environment);

                            if (exitCode != 0)
                            {
                                slot.Admitted = false;
                                slot.Why = $"{leg.Name}/{program} {arm}: measure run exit {exitCode}";
                                break;
                            }

                            slot.Samples.Add(new RunSample(wallMs, Runner.ParseInnerMs(stdOut), peakBytes));
                        }
                    }
                }

                Console.WriteLine("done");
            }
        }

        private static string Report(IReadOnlyList<string> programs, Options options, Dictionary<(string, string), LegProgram> table, List<string> refusals)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            StringBuilder sb = new();
            string sdk = Runner.Exec("dotnet", "--version", options.WorkRoot, 30_000) is { ExitCode: 0 } v ? v.StdOut.Trim() : "?";

            sb.AppendLine($"**Environment:** {Runner.CpuName()} · {System.Runtime.InteropServices.RuntimeInformation.OSDescription.Trim()} · .NET SDK {sdk} · {DateTime.Now:yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine("Columns (RID and shape): Go = `windows/amd64`, default `GOAMD64`; C# (JIT) = framework-dependent, `win-x64` apphost; " +
                "C# (ReadyToRun) = `win-x64`, self-contained, partial trim, ReadyToRun (the converter template's own `dotnet publish -c Release -r win-x64`); " +
                "C# (Native AOT) = `win-x64`, self-contained, partial trim, default instruction set, published on another box and admitted by its manifest. " +
                "Diagnostic arms: Tc0 = the JIT binary with `DOTNET_TieredCompilation=0`; Fd1 = `win-x64`, framework-dependent, untrimmed, ReadyToRun. " +
                $"Median of {options.Runs} runs ({options.ReviewRuns} for the review rows), 1 discarded warmup, legs interleaved. Workload time is in-program; the Startup row is process wall time.");

            foreach (ReleaseLeg leg in options.Legs)
            {
                sb.AppendLine();
                sb.AppendLine($"### Leg {leg.Name}: go.* {leg.PackageVersion}");
                sb.AppendLine();
                sb.AppendLine("| Benchmark | Go | C# (JIT) | C# (ReadyToRun) | C# (Native AOT) |");
                sb.AppendLine("|---|---:|---:|---:|---:|");

                foreach (string program in programs)
                {
                    LegProgram lp = table[(leg.Name, program)];
                    double go = Metric(program, lp.Slots[ReleaseArm.Go]);
                    sb.Append($"| {Name(program)} | {Cell(go, 0, ci)} ");

                    foreach (ReleaseArm arm in new[] { ReleaseArm.Jit, ReleaseArm.ReadyToRun, ReleaseArm.Aot })
                        sb.Append($"| {Cell(Metric(program, lp.Slots[arm]), go, ci)} ");

                    sb.AppendLine("|");
                }
            }

            // Diagnostics: never the published table (the plan's section 6). Spread is min..max of the timed runs.
            sb.AppendLine();
            sb.AppendLine("### Diagnostics (record only)");
            sb.AppendLine();
            sb.AppendLine("| Benchmark | Leg | Arm | Median | Min..Max | Runs |");
            sb.AppendLine("|---|---|---|---:|---:|---:|");

            foreach (string program in programs)
            {
                foreach (ReleaseLeg leg in options.Legs)
                {
                    foreach (ReleaseArm arm in Enum.GetValues<ReleaseArm>())
                    {
                        Slot slot = table[(leg.Name, program)].Slots[arm];

                        if (slot.Samples.Count == 0)
                            continue;

                        List<double> values = slot.Samples.Select(s => program == "PerfStartup" ? s.WallMs : s.InnerMs).ToList();
                        sb.AppendLine($"| {Name(program)} | {leg.Name} | {arm} | {Runner.Median(values).ToString("N1", ci)} | {values.Min().ToString("N1", ci)}..{values.Max().ToString("N1", ci)} | {values.Count} |");
                    }
                }
            }

            if (refusals.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("### Refused (not timed)");
                sb.AppendLine();

                foreach (string why in refusals)
                    sb.AppendLine($"- {why}");
            }

            return sb.ToString();
        }

        // ---- helpers ----

        private static double Metric(string program, Slot slot) =>
            slot.Samples.Count == 0 ? double.NaN : Runner.Median(slot.Samples.Select(s => program == "PerfStartup" ? s.WallMs : s.InnerMs));

        private static string Cell(double value, double go, CultureInfo ci)
        {
            if (double.IsNaN(value))
                return "n/a";

            string cell = value.ToString("N1", ci);
            return go > 0 && !double.IsNaN(go) ? $"{cell} ({(value / go).ToString("N2", ci)}×)" : cell;
        }

        private static string Name(string program) => program.StartsWith("Perf", StringComparison.Ordinal) ? program[4..] : program;

        // The named properties as MSBuild evaluates the project (`dotnet msbuild -getProperty`); a property the
        // evaluation does not answer is left out, so the refusal names it.
        private static Dictionary<string, string> EvaluatedProperties(string csproj, string projectDir, IReadOnlyList<string> names)
        {
            Dictionary<string, string> properties = new(StringComparer.OrdinalIgnoreCase);

            foreach (string name in names)
            {
                Runner.ProcResult r = Runner.Exec("dotnet", $"msbuild \"{csproj}\" -nologo -getProperty:{name}", projectDir, BuildTimeoutMs);
                string value = r.StdOut.Trim();

                if (r.ExitCode == 0 && value.Length > 0)
                    properties[name] = value;
            }

            return properties;
        }

        private static Dictionary<string, string> GoEnvironment(ReleaseLeg leg)
        {
            // The leg's own toolchain and nothing else: GOROOT from the go.exe it names, that go.exe first on PATH, no
            // toolchain switch, and CGO_ENABLED=0 as the corpus is emitted. PATH matters as much as GOROOT: the converter
            // asks the `go` on PATH which release it converts for (`go env GOVERSION`), and a newer go there makes a
            // release converter refuse -recurse=nuget for a release it was never asked about.
            string goBin = Path.GetDirectoryName(Path.GetFullPath(leg.Go))!;
            string goRoot = Path.GetFullPath(Path.Combine(goBin, ".."));
            string path = goBin + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH");
            return new() { ["GOROOT"] = goRoot, ["PATH"] = path, ["GOTOOLCHAIN"] = "local", ["CGO_ENABLED"] = "0" };
        }

        private static string? FindProgramProject(string output, string program) =>
            Directory.GetFiles(output, "*.csproj", SearchOption.AllDirectories)
                .Where(p => !p.EndsWith(".tests.csproj", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(p => string.Equals(AssemblyName(p, ""), program, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(Path.GetFileNameWithoutExtension(p), program, StringComparison.OrdinalIgnoreCase));

        private static string AssemblyName(string csproj, string fallback)
        {
            string text = File.ReadAllText(csproj);
            int at = text.IndexOf("<AssemblyName>", StringComparison.Ordinal);

            if (at < 0)
                return fallback.Length > 0 ? fallback : Path.GetFileNameWithoutExtension(csproj);

            int start = at + "<AssemblyName>".Length;
            int end = text.IndexOf('<', start);
            return text[start..end].Trim();
        }

        private static void CopyProgram(string from, string to)
        {
            foreach (string file in Directory.GetFiles(from, "*", SearchOption.AllDirectories))
            {
                string rel = Path.GetRelativePath(from, file);

                // Build output at any depth, and the template's redirected .artifacts root, stay behind.
                if (rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(s => s is "bin" or "obj" or ".artifacts"))
                    continue;

                string target = Path.Combine(to, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target);
            }
        }

        private static void RecreateDirectory(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);

            Directory.CreateDirectory(path);
        }

        private static string Sha256Of(string file)
        {
            using FileStream stream = File.OpenRead(file);
            return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        }
    }
}

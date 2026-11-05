// HeadlineSettings.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// What the published table's columns may be built and run with: the converter's default settings, the project file it
// writes, and for the Native AOT column only the standard Native AOT publish properties (docs/Performance.md, "Nothing
// is tuned for these benchmarks"; docs/PLAN-perf-rebaseline.md, the S5 fairness check). Every headline command line
// comes from one builder here and is checked against that command's allowlist before it runs. The build files, the
// environment the runner inherits, an in-tree project file and the runtimeconfig.json a headline binary carries are
// checked against what the template writes. Each refusal names what it found. Pure functions over text, so
// PerformanceRunnerTests links this file and plants each opt-in without a build.

using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PerformanceRunner
{
    /// <summary>The commands a headline column's binary comes from.</summary>
    internal enum HeadlineCommand
    {
        InTreeTranspile, GoBuild, InTreeJitBuild, InTreeAotPublish, ReleaseConvert, ReleaseJitBuild, ReleaseReadyToRunPublish
    }

    /// <summary>
    /// One finding, in the one shape every check reports: the column it disqualifies, what was read (a command line, a
    /// file's property, a variable), what that is, and what it must be. Any finding stops the run, exit 2, before
    /// anything is timed.
    /// </summary>
    internal sealed record HeadlineRefusal(string Column, string Subject, string Found, string Expected)
    {
        public override string ToString() => $"REFUSED [{Column}] {Subject}: {Found}; expected {Expected}";
    }

    internal static class Headline
    {
        internal const string Configuration = "Release";

        // ---- the command lines: one builder per command, nothing else spells them ----

        internal static string InTreeTranspile(string srcRoot, string projectDir) => $"-go2cspath \"{srcRoot}\" \"{projectDir}\"";

        internal static string GoBuild(string exe) => $"build -o \"{exe}\" .";

        internal static string InTreeJitBuild(string project, string go2csPath) =>
            $"build \"{project}\" -nologo -clp:ErrorsOnly -p:Configuration={Configuration} -p:go2csPath={go2csPath}";

        internal static string InTreeAotPublish(string csproj, string go2csPath, string outDir) =>
            $"publish \"{csproj}\" -nologo -clp:ErrorsOnly -c {Configuration} -p:PerfAot=true -p:go2csPath={go2csPath} -o \"{outDir}\"";

        internal static string ReleaseConvert(string source, string output) => $"-recurse=nuget \"{source}\" \"{output}\"";

        internal static string ReleaseJitBuild(string csproj, string outDir, string? pinnedVersion = null) =>
            $"build \"{csproj}\" -nologo -clp:ErrorsOnly -c {Configuration}{Pin(pinnedVersion)} \"-p:OutDir={outDir}\"";

        internal static string ReleaseReadyToRunPublish(string csproj, string outDir, string? pinnedVersion = null) =>
            $"publish \"{csproj}\" -nologo -clp:ErrorsOnly -c {Configuration}{Pin(pinnedVersion)} -r win-x64 -o \"{outDir}\"";

        // A pinned release leg (ReleaseLeg.Pin) sets the package version the converted Directory.Build.props otherwise
        // floats; it chooses which published release is measured, never how it is compiled. It goes before any quoted
        // path: OutDir ends in a separator, so its closing quote is \", which Windows reads as a literal quote and
        // which swallows whatever follows into the OutDir value.
        internal const string PinProperty = "GoStdLibVersion";

        internal static string Pin(string? pinnedVersion) => pinnedVersion is null ? "" : $" -p:{PinProperty}={pinnedVersion}";

        private static bool Pins(HeadlineCommand command) => command is HeadlineCommand.ReleaseJitBuild or HeadlineCommand.ReleaseReadyToRunPublish;

        // The options each command may carry, and the MSBuild properties it may set with -p:. A path or verb is not an
        // option. PerfAot is the suite's gate for the Native AOT set in its Directory.Build.targets; go2csPath and OutDir
        // say where things are, not how they are compiled.
        internal static readonly IReadOnlyDictionary<HeadlineCommand, (string[] Options, string[] Properties)> Allowed =
            new Dictionary<HeadlineCommand, (string[], string[])>
            {
                [HeadlineCommand.InTreeTranspile] = (["-go2cspath"], []),
                [HeadlineCommand.GoBuild] = (["-o"], []),
                [HeadlineCommand.InTreeJitBuild] = (["-nologo", "-clp:ErrorsOnly"], ["Configuration", "go2csPath"]),
                [HeadlineCommand.InTreeAotPublish] = (["-nologo", "-clp:ErrorsOnly", "-c", "-o"], ["PerfAot", "go2csPath"]),
                [HeadlineCommand.ReleaseConvert] = (["-recurse=nuget"], []),
                [HeadlineCommand.ReleaseJitBuild] = (["-nologo", "-clp:ErrorsOnly", "-c"], ["OutDir"]),
                [HeadlineCommand.ReleaseReadyToRunPublish] = (["-nologo", "-clp:ErrorsOnly", "-c", "-r", "-o"], []),
            };

        // An option followed by its value as the next token.
        private static readonly HashSet<string> s_valued = new(StringComparer.Ordinal) { "-go2cspath", "-o", "-c", "-r" };

        /// <summary>
        /// Why <paramref name="argv"/> is not a headline <paramref name="command"/>, one finding per token: an option or
        /// a -p: property outside the command's allowlist, or a configuration other than Release. Empty when it is. A
        /// pinned leg's release build and publish may also carry <see cref="PinProperty"/>, at exactly
        /// <paramref name="pinnedVersion"/> and nothing else; an unpinned one may not carry it at all.
        /// </summary>
        internal static IReadOnlyList<HeadlineRefusal> ArgvRefusals(string column, HeadlineCommand command, string argv, string? pinnedVersion = null)
        {
            (string[] options, string[] properties) = Allowed[command];
            List<string> tokens = Tokens(argv);
            List<string> refused = new();
            bool pinned = pinnedVersion is not null && Pins(command);
            string expected = $"only {string.Join(" ", options.Concat(properties.Select(p => $"-p:{p}")))}" +
                $"{(pinned ? $" -p:{PinProperty}={pinnedVersion}" : "")}, with configuration {Configuration}";

            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i];

                if (!token.StartsWith('-') && !token.StartsWith("/p:", StringComparison.OrdinalIgnoreCase) &&
                    !token.StartsWith("/property:", StringComparison.OrdinalIgnoreCase))
                    continue;

                string? property = PropertyName(token, out string value);

                if (property is not null)
                {
                    if (pinned && property.Equals(PinProperty, StringComparison.OrdinalIgnoreCase) && value == pinnedVersion)
                        continue;

                    if (!properties.Contains(property, StringComparer.OrdinalIgnoreCase))
                        refused.Add(token);
                    else if (property.Equals("Configuration", StringComparison.OrdinalIgnoreCase) && value != Configuration)
                        refused.Add(token);

                    continue;
                }

                if (!options.Contains(token, StringComparer.Ordinal))
                {
                    refused.Add(token);
                    continue;
                }

                if (s_valued.Contains(token) && i + 1 < tokens.Count)
                {
                    i++;

                    if (token == "-c" && tokens[i] != Configuration)
                        refused.Add($"-c {tokens[i]}");
                }
            }

            return refused.Select(token => new HeadlineRefusal(column, $"{command} command line", token, expected)).ToList();
        }

        private static string? PropertyName(string token, out string value)
        {
            value = "";

            foreach (string prefix in new[] { "-p:", "-property:", "/p:", "/property:" })
            {
                if (!token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                string assignment = token[prefix.Length..];
                int eq = assignment.IndexOf('=');

                if (eq < 0)
                    return assignment;

                value = assignment[(eq + 1)..];
                return assignment[..eq];
            }

            return null;
        }

        // Splits a command line as the process start does: whitespace separates, double quotes group and are dropped.
        private static List<string> Tokens(string argv)
        {
            List<string> tokens = new();
            System.Text.StringBuilder current = new();
            bool quoted = false, any = false;

            foreach (char c in argv)
            {
                if (c == '"')
                {
                    quoted = !quoted;
                    any = true;
                }
                else if (char.IsWhiteSpace(c) && !quoted)
                {
                    if (any)
                        tokens.Add(current.ToString());

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
                tokens.Add(current.ToString());

            return tokens;
        }

        // ---- the build files the in-tree projects import ----

        /// <summary>
        /// The properties each build file above the in-tree projects may set, keyed by its path under src/, with the
        /// value pinned where the documented set names one (null: any value). The Native AOT set is the
        /// Directory.Build.targets row, as docs/Performance.md names it.
        /// </summary>
        internal static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string?>> BuildFiles =
            new Dictionary<string, IReadOnlyDictionary<string, string?>>(StringComparer.Ordinal)
            {
                ["Directory.Build.props"] = new Dictionary<string, string?>(StringComparer.Ordinal)
                {
                    ["TargetFramework"] = null, ["LangVersion"] = null, ["WarningsAsErrors"] = null, ["InterceptorsNamespaces"] = null,
                },
                ["tests/Performance/Directory.Build.props"] = new Dictionary<string, string?>(StringComparer.Ordinal)
                {
                    ["SuppressTrimAnalysisWarnings"] = "true", ["BaseIntermediateOutputPath"] = null, ["BaseOutputPath"] = null,
                },
                ["tests/Performance/Directory.Build.targets"] = new Dictionary<string, string?>(StringComparer.Ordinal)
                {
                    ["PublishAot"] = "true", ["PublishReadyToRun"] = "false", ["PublishTrimmed"] = "true",
                    ["RuntimeIdentifier"] = "$(NETCoreSdkRuntimeIdentifier)", ["SelfContained"] = "true", ["TrimMode"] = "partial",
                },
            };

        /// <summary>
        /// Why the build file <paramref name="label"/> sets more than <paramref name="allowed"/>, one line per finding:
        /// a property outside the list, a pinned property at another value, or any item, target or import other than
        /// the chain to the repository's Directory.Build.props.
        /// </summary>
        internal static IReadOnlyList<HeadlineRefusal> BuildFileRefusals(string label, string xml, IReadOnlyDictionary<string, string?> allowed)
        {
            const string Column = "every C# column";
            List<HeadlineRefusal> refusals = new();
            XElement root = XDocument.Parse(xml).Root!;

            foreach (XElement element in root.Elements())
            {
                switch (element.Name.LocalName)
                {
                    case "PropertyGroup":
                        foreach (XElement property in element.Elements())
                        {
                            string name = property.Name.LocalName;
                            string value = property.Value.Trim();

                            if (!allowed.TryGetValue(name, out string? pinned))
                                refusals.Add(new(Column, $"{label} property {name}", $"\"{value}\"", $"not set: {label} may set only {string.Join(", ", allowed.Keys)}"));
                            else if (pinned is not null && value != pinned)
                                refusals.Add(new(Column, $"{label} property {name}", $"\"{value}\"", $"\"{pinned}\", the documented value"));
                        }

                        break;

                    case "Import" when (element.Attribute("Project")?.Value ?? "").Contains("GetPathOfFileAbove('Directory.Build.props'", StringComparison.Ordinal):
                        break;

                    default:
                        refusals.Add(new(Column, $"{label} element <{element.Name.LocalName}>", "present", "absent: a build file above the benchmarks sets only its listed properties"));
                        break;
                }
            }

            return refusals;
        }

        /// <summary>
        /// Build files that would reach a headline build from outside the listed chain: a Directory.Build.* in a
        /// benchmark's own folder (it would shadow the suite's), and a Directory.Build.rsp (extra command-line
        /// arguments) at any level from that folder up. <paramref name="exists"/> answers whether a file exists.
        /// </summary>
        internal static IReadOnlyList<HeadlineRefusal> StrayBuildFiles(string projectDir, Func<string, bool> exists)
        {
            List<string> found = new();

            foreach (string name in new[] { "Directory.Build.props", "Directory.Build.targets" })
            {
                if (exists(Path.Combine(projectDir, name)))
                    found.Add(Path.Combine(projectDir, name));
            }

            for (string? dir = projectDir; dir is not null; dir = Path.GetDirectoryName(dir))
            {
                if (exists(Path.Combine(dir, "Directory.Build.rsp")))
                    found.Add(Path.Combine(dir, "Directory.Build.rsp"));
            }

            return found.Select(f => new HeadlineRefusal("every C# column", f, "present", "absent: it would reach the build outside the listed chain")).ToList();
        }

        /// <summary>
        /// Build files at or above a release leg's work root. The converter writes the only Directory.Build.props a
        /// release program may see, beside the program; anything at or above the root would reach every leg.
        /// </summary>
        internal static IReadOnlyList<HeadlineRefusal> BuildFilesAtOrAbove(string workRoot, Func<string, bool> exists)
        {
            List<HeadlineRefusal> found = new();

            for (string? dir = workRoot; dir is not null; dir = Path.GetDirectoryName(dir))
            {
                foreach (string name in new[] { "Directory.Build.props", "Directory.Build.targets", "Directory.Build.rsp" })
                {
                    if (exists(Path.Combine(dir, name)))
                        found.Add(new("every C# column", Path.Combine(dir, name), "present", "absent: only the converter's own build file may reach a release program"));
                }
            }

            return found;
        }

        // ---- the environment every build and timed run inherits ----

        // DOTNET_ variables that locate or quiet the host and the SDK, and never change how code is compiled or run.
        private static readonly string[] s_hostVariablePrefixes =
        [
            "DOTNET_ROOT", "DOTNET_CLI_", "DOTNET_NOLOGO", "DOTNET_MULTILEVEL_LOOKUP", "DOTNET_SKIP_FIRST_TIME_EXPERIENCE",
            "DOTNET_GENERATE_ASPNET_CERTIFICATE", "DOTNET_ADD_GLOBAL_TOOLS_TO_PATH", "DOTNET_INSTALL_DIR", "DOTNET_HOST_PATH",
        ];

        // MSBuild reads every environment variable as a property, so a variable with a knob's name sets the knob in
        // every build the runner starts. The Go runtime and toolchain read theirs at build or run time.
        private static readonly HashSet<string> s_knobVariables = new(StringComparer.OrdinalIgnoreCase)
        {
            "TieredCompilation", "TieredPGO", "TieredCompilationQuickJit", "TieredCompilationQuickJitForLoops",
            "GoCallCountingDelayMs", "GoReadyToRunUserCode", "ServerGarbageCollector", "ConcurrentGarbageCollection",
            "OptimizationPreference", "IlcOptimizationPreference", "IlcInstructionSet", "IlcFoldIdenticalMethodBodies",
            "PublishAot", "PublishReadyToRun", "PublishTrimmed", "TrimMode", "SelfContained", "Optimize", "Configuration",
            "GOGC", "GOMEMLIMIT", "GOMAXPROCS", "GODEBUG", "GOAMD64", "GOFLAGS", "GOEXPERIMENT",
        };

        /// <summary>
        /// The inherited variables that would tune a headline build or run, one finding each: any COMPlus_ variable, a
        /// DOTNET_ variable that is not host or SDK plumbing, and any variable named like an MSBuild or Go knob.
        /// </summary>
        internal static IReadOnlyList<HeadlineRefusal> EnvironmentRefusals(IEnumerable<KeyValuePair<string, string?>> environment) =>
            environment.Where(e => Tunes(e.Key)).OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
                .Select(e => new HeadlineRefusal("every column", $"environment variable {e.Key}", $"\"{e.Value}\"", "unset: every build and timed run this runner starts inherits it"))
                .ToList();

        /// <summary>The DOTNET_ host and SDK variables the environment carries and the guard admits, by name.</summary>
        internal static IReadOnlyList<string> AdmittedHostVariables(IEnumerable<KeyValuePair<string, string?>> environment) =>
            environment.Select(e => e.Key).Where(n => n.StartsWith("DOTNET_", StringComparison.OrdinalIgnoreCase) && !Tunes(n))
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();

        private static bool Tunes(string name) =>
            name.StartsWith("COMPlus_", StringComparison.OrdinalIgnoreCase)
            || (name.StartsWith("DOTNET_", StringComparison.OrdinalIgnoreCase)
                && !s_hostVariablePrefixes.Any(p => name.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            || s_knobVariables.Contains(name);

        // ---- what reaches the runtime ----

        internal const string CallCountingDelayKey = "System.Runtime.TieredCompilation.CallCountingDelayMs";

        // Runtime settings a headline binary may not carry: the collector, tiering, PGO and the thread pool.
        private static readonly string[] s_tuningKeyPrefixes =
            ["System.GC.", "System.Runtime.TieredCompilation", "System.Runtime.TieredPGO", "System.Threading.ThreadPool."];

        private static readonly Regex s_templateDelay = new(
            "<GoCallCountingDelayMs Condition=\"'\\$\\(GoCallCountingDelayMs\\)'==''\">(?<value>[^<]*)</GoCallCountingDelayMs>", RegexOptions.Compiled);

        /// <summary>
        /// The call-counting delay a converted project's template writes into its runtimeconfig, or null when the
        /// template has none (a converter older than the setting).
        /// </summary>
        internal static string? TemplateCallCountingDelay(string csproj) =>
            s_templateDelay.Match(csproj) is { Success: true } m ? m.Groups["value"].Value.Trim() : null;

        /// <summary>
        /// Why a headline binary's runtimeconfig.json carries a runtime setting the converted project's template does
        /// not write, one line each. The template's own call-counting delay is the one tiering key it may carry, and
        /// then at the template's value.
        /// </summary>
        internal static IReadOnlyList<HeadlineRefusal> RuntimeConfigRefusals(string column, string label, string? runtimeConfig, string? templateDelay)
        {
            if (runtimeConfig is null)
                return [new(column, label, "not found", "the runtimeconfig.json the build writes beside the binary")];

            List<HeadlineRefusal> refusals = new();
            using JsonDocument doc = JsonDocument.Parse(runtimeConfig);
            Dictionary<string, string> settings = new(StringComparer.Ordinal);

            if (doc.RootElement.TryGetProperty("runtimeOptions", out JsonElement options) &&
                options.TryGetProperty("configProperties", out JsonElement properties))
            {
                foreach (JsonProperty setting in properties.EnumerateObject())
                    settings[setting.Name] = setting.Value.ToString();
            }

            foreach ((string key, string value) in settings)
            {
                if (key == CallCountingDelayKey || !s_tuningKeyPrefixes.Any(p => key.StartsWith(p, StringComparison.Ordinal)))
                    continue;

                refusals.Add(new(column, $"{label} {key}", value, "absent: the template does not write it"));
            }

            bool hasDelay = settings.TryGetValue(CallCountingDelayKey, out string? delay);

            if (templateDelay is null && hasDelay)
                refusals.Add(new(column, $"{label} {CallCountingDelayKey}", delay!, "absent: this converter's template has no call-counting setting"));
            else if (templateDelay is not null && delay != templateDelay)
                refusals.Add(new(column, $"{label} {CallCountingDelayKey}", hasDelay ? delay! : "absent", $"{templateDelay}, the template's default"));

            return refusals;
        }

        // ---- an in-tree project file against the template ----

        /// <summary>
        /// Why an in-tree benchmark's project file is not what the converter's template writes, one line each: a
        /// property the template never sets, or one at another value where the template's value is literal (no
        /// placeholder). The runner skips an up-to-date transpile, so a hand edit to the project would survive it.
        /// </summary>
        internal static IReadOnlyList<HeadlineRefusal> ProjectFileRefusals(string label, string csproj, string template)
        {
            Dictionary<string, List<string>> templateValues = new(StringComparer.Ordinal);

            foreach (XElement property in Properties(ResolveTemplateMarkers(template)))
            {
                if (!templateValues.TryGetValue(property.Name.LocalName, out List<string>? values))
                    templateValues[property.Name.LocalName] = values = new();

                values.Add(property.Value.Trim());
            }

            const string Column = "every C# column";
            List<HeadlineRefusal> refusals = new();

            foreach (XElement property in Properties(csproj))
            {
                string name = property.Name.LocalName;
                string value = property.Value.Trim();

                if (!templateValues.TryGetValue(name, out List<string>? values))
                    refusals.Add(new(Column, $"{label} property {name}", $"\"{value}\"", "not set: the converter's template never sets it"));
                else if (!values.Any(v => v.Contains('%')) && !values.Contains(value))
                    refusals.Add(new(Column, $"{label} property {name}", $"\"{value}\"", $"\"{string.Join("\" or \"", values)}\", what the template writes"));
            }

            return refusals;
        }

        // The template's markers, as the converter fills them for a program (src/go2cs: packageIdLine, the licensing
        // resolver, validationPackBlock): the package ID and license properties, with a placeholder value because they
        // depend on the package; and nothing for the standard library's validation pack, which a program never has.
        private static string ResolveTemplateMarkers(string template) => template
            .Replace(">>MARKER:PACKAGE_ID<<", "<PackageId>%s</PackageId>", StringComparison.Ordinal)
            .Replace("<!-- GO2CS:PACKAGE-LICENSE -->", "<PackageLicenseFile>%s</PackageLicenseFile><PackageLicenseExpression>%s</PackageLicenseExpression>", StringComparison.Ordinal)
            .Replace(">>MARKER:VALIDATION_PACK<<", "", StringComparison.Ordinal);

        private static IEnumerable<XElement> Properties(string project) =>
            XDocument.Parse(project).Root!.Elements().Where(e => e.Name.LocalName == "PropertyGroup").SelectMany(g => g.Elements());
    }
}

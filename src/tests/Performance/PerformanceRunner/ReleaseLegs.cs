// ReleaseLegs.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The decisions behind the post-hop re-baseline's release legs (docs/PLAN-perf-rebaseline.md, S0): what a leg is,
// what a consumer-view project must reference, how a converted program is identified across two boxes, whether a
// shipped Native AOT binary may be timed, and whether a JIT or ReadyToRun binary has the shape its column claims.
// Pure functions over text (and one PE header read), so PerformanceRunnerTests links this file and refuses each by
// name without a build.

using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PerformanceRunner
{
    /// <summary>
    /// One release leg: the converter that converts the programs, the package version their project files must
    /// reference, and the Go toolchain that builds the Go column. Spelled on the command line as
    /// <c>name=1.26;go2cs=C:\...\go2cs.exe;version=1.26.10.1;go=C:\...\go.exe</c>, with an optional <c>pin=true</c>.
    /// <para>
    /// A converted program floats: its Directory.Build.props sets <c>GoStdLibVersion</c> to <c>1.26.*</c>, which restores
    /// the newest matching release on the feed, so an older leg stops resolving to itself once a later release
    /// publishes. <c>pin=true</c> pins the leg the way that props file tells a consumer to, with
    /// <c>-p:GoStdLibVersion=&lt;version&gt;</c> on every build and publish of the leg; the headline guard admits exactly
    /// that property at exactly that value. Off by default: an unpinned leg measures what a consumer gets.
    /// </para>
    /// </summary>
    internal sealed record ReleaseLeg(string Name, string Converter, string PackageVersion, string Go, bool Pin = false)
    {
        /// <summary>The version this leg's builds and publishes pin, or null for an unpinned leg.</summary>
        internal string? PinnedVersion => Pin ? PackageVersion : null;

        internal static ReleaseLeg Parse(string spec)
        {
            Dictionary<string, string> parts = new(StringComparer.OrdinalIgnoreCase);

            foreach (string part in spec.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                int eq = part.IndexOf('=');

                if (eq <= 0)
                    throw new FormatException($"--leg \"{spec}\": \"{part}\" is not key=value");

                parts[part[..eq].Trim()] = part[(eq + 1)..].Trim();
            }

            foreach (string key in new[] { "name", "go2cs", "version", "go" })
            {
                if (!parts.TryGetValue(key, out string? value) || value.Length == 0)
                    throw new FormatException($"--leg \"{spec}\": missing {key}=");
            }

            if (parts["name"].IndexOfAny(['/', '\\', ' ', '\t']) >= 0)
                throw new FormatException($"--leg \"{spec}\": a leg name names a folder; it takes no separator or space");

            bool pin = false;

            if (parts.TryGetValue("pin", out string? pinValue) && !bool.TryParse(pinValue, out pin))
                throw new FormatException($"--leg \"{spec}\": pin= takes true or false, not \"{pinValue}\"");

            return new ReleaseLeg(parts["name"], parts["go2cs"], parts["version"], parts["go"], pin);
        }
    }

    /// <summary>
    /// What a consumer-view project file must say: every go2cs package it uses comes from NuGet, at the leg's version.
    /// </summary>
    internal static class ConsumerProject
    {
        private static readonly Regex s_packageReference = new(
            "<PackageReference\\s+Include=\"(?<id>[^\"]+)\"\\s+Version=\"(?<version>[^\"]+)\"", RegexOptions.Compiled);

        private static readonly Regex s_projectReference = new("<ProjectReference\\s+Include=\"(?<path>[^\"]+)\"", RegexOptions.Compiled);

        // go.lib (golib), go.gen (the analyzer) and every go.<pkg> standard-library package.
        private static bool IsGo2csPackage(string id) => id.StartsWith("go.", StringComparison.OrdinalIgnoreCase);

        internal static IReadOnlyList<(string Id, string Version)> PackageReferences(string csproj) =>
            s_packageReference.Matches(csproj).Select(m => (m.Groups["id"].Value, m.Groups["version"].Value)).ToList();

        /// <summary>
        /// Why <paramref name="csproj"/> is not the published packages as a consumer gets them, one line per reason;
        /// empty when it is. The subject of the re-baseline is the published release, so a project reference into a
        /// local standard library, a go2cs package at another version, or no go2cs package at all is refused by name.
        /// </summary>
        private static readonly Regex s_property = new("^\\$\\((?<name>[A-Za-z_][A-Za-z0-9_.-]*)\\)$", RegexOptions.Compiled);

        /// <summary>
        /// The MSBuild properties the go2cs package references take their version from: a release's converter writes
        /// <c>Version="$(GoStdLibVersion)"</c> and sets the property in the Directory.Build.props beside the program.
        /// </summary>
        internal static IReadOnlyList<string> VersionProperties(string csproj) =>
            PackageReferences(csproj).Where(p => IsGo2csPackage(p.Id))
                .Select(p => s_property.Match(p.Version)).Where(m => m.Success)
                .Select(m => m.Groups["name"].Value).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        internal static IReadOnlyList<string> Refusals(string program, string csproj, ReleaseLeg leg,
            IReadOnlyDictionary<string, string>? properties = null)
        {
            List<string> refusals = new();

            foreach (Match m in s_projectReference.Matches(csproj))
                refusals.Add($"{leg.Name}/{program}: a project reference ({m.Groups["path"].Value}) where the published package belongs");

            List<(string Id, string Version)> packages = PackageReferences(csproj).Where(p => IsGo2csPackage(p.Id)).ToList();

            if (packages.Count == 0)
                refusals.Add($"{leg.Name}/{program}: no go.* package reference; this is not a -recurse=nuget conversion");

            foreach ((string id, string spec) in packages)
            {
                // A property-valued version is judged by what the evaluation gave it; one it did not answer is refused.
                string version = spec;
                Match property = s_property.Match(spec);

                if (property.Success)
                {
                    string name = property.Groups["name"].Value;

                    if (properties is null || !properties.TryGetValue(name, out string? value))
                    {
                        refusals.Add($"{leg.Name}/{program}: references {id} {spec}, and the evaluation gave {name} no value");
                        continue;
                    }

                    version = value;
                }

                if (!VersionCovers(version, leg.PackageVersion))
                    refusals.Add($"{leg.Name}/{program}: references {id} {version}, which does not cover the leg's {leg.PackageVersion}");
            }

            return refusals;
        }

        /// <summary>
        /// Whether a PackageReference version admits the leg's version: the same version, or a floating one
        /// (<c>1.24.13.*</c>, what a release's converter writes) whose prefix it matches. A floating spec says what may
        /// resolve, not what did, so <see cref="ResolvedRefusals"/> reads the restore's answer as well.
        /// </summary>
        internal static bool VersionCovers(string spec, string version) =>
            string.Equals(spec, version, StringComparison.OrdinalIgnoreCase) ||
            (spec.EndsWith(".*", StringComparison.Ordinal) && version.StartsWith(spec[..^1], StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Why the RESTORED packages (<c>obj/project.assets.json</c>'s libraries) are not the leg's release: a go2cs
        /// package resolved at another version, or none at all. A floating reference resolves to the newest matching
        /// package on the feed, which is the leg's only while no later one exists.
        /// </summary>
        internal static IReadOnlyList<string> ResolvedRefusals(string program, string? assetsJson, ReleaseLeg leg)
        {
            if (assetsJson is null)
                return [$"{leg.Name}/{program}: no project.assets.json; the project was never restored"];

            List<string> refusals = new();
            int go2csLibraries = 0;

            using JsonDocument doc = JsonDocument.Parse(assetsJson);

            if (doc.RootElement.TryGetProperty("libraries", out JsonElement libraries))
            {
                foreach (JsonProperty library in libraries.EnumerateObject())
                {
                    int slash = library.Name.IndexOf('/');

                    if (slash <= 0 || !IsGo2csPackage(library.Name[..slash]))
                        continue;

                    go2csLibraries++;
                    string version = library.Name[(slash + 1)..];

                    if (!string.Equals(version, leg.PackageVersion, StringComparison.OrdinalIgnoreCase))
                        refusals.Add($"{leg.Name}/{program}: restored {library.Name[..slash]} {version}, but the leg is {leg.PackageVersion}");
                }
            }

            if (go2csLibraries == 0)
                refusals.Add($"{leg.Name}/{program}: the restore resolved no go.* package");

            return refusals;
        }

        /// <summary>
        /// The identity of a converted program across two boxes: SHA-256 over every <c>.cs</c> and <c>.csproj</c> of
        /// its project, keyed by relative path (forward slashes) in ordinal order, line endings stripped of CR, and its
        /// two host-specific inputs masked (<see cref="MaskHostSpecific"/>). Build output and the generators' copies
        /// are not the program and are excluded by the caller.
        /// </summary>
        internal static string SourceHash(IEnumerable<(string RelativePath, string Contents)> files)
        {
            using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            foreach ((string relativePath, string contents) in files.OrderBy(f => f.RelativePath.Replace('\\', '/'), StringComparer.Ordinal))
            {
                hash.AppendData(Encoding.UTF8.GetBytes(relativePath.Replace('\\', '/')));
                hash.AppendData([0]);
                hash.AppendData(Encoding.UTF8.GetBytes(MaskHostSpecific(contents.Replace("\r", ""))));
                hash.AppendData([0]);
            }

            return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
        }

        // GoPositionMap's first argument is the Go file's ABSOLUTE path under the --work root, which differs by box;
        // only its file name identifies the program (a package's files are one directory, so names are unique).
        private static readonly Regex s_positionMapPath = new(@"(GoPositionMap\(\s*"")([^""]*)("")", RegexOptions.CultureInvariant);

        // GoInputDigest folds in a hash of the converter executable's own bytes, so two builds of one converter differ.
        private static readonly Regex s_inputDigest = new(@"(<GoInputDigest>)[^<]*(</GoInputDigest>)", RegexOptions.CultureInvariant);

        /// <summary>
        /// Masks the two inputs a converted consumer project carries that name the box rather than the program
        /// (PLAN-perf-rebaseline.md section 10, P0): GoPositionMap's absolute Go file path becomes its file name, and
        /// GoInputDigest's value becomes empty. Everything else, the position map's payload included, is hashed as
        /// written.
        /// </summary>
        internal static string MaskHostSpecific(string contents)
        {
            string masked = s_positionMapPath.Replace(contents, m =>
            {
                string path = m.Groups[2].Value;
                int cut = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
                return m.Groups[1].Value + path[(cut + 1)..] + m.Groups[3].Value;
            });

            return s_inputDigest.Replace(masked, "$1$2");
        }

        /// <summary>
        /// Whether a path under a converted project is part of the program: <c>.cs</c> and <c>.csproj</c>, outside
        /// <c>bin</c>, <c>obj</c> and the generators' <c>Generated</c> copies.
        /// </summary>
        internal static bool IsProgramFile(string relativePath)
        {
            string normalized = relativePath.Replace('\\', '/');
            string first = normalized.Split('/')[0];

            if (first is "bin" or "obj" or "Generated")
                return false;

            return normalized.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || normalized.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// One shipped Native AOT binary, as the box that published it describes it.
    /// </summary>
    internal sealed record AotManifestEntry(string Leg, string Program, string Sha256, string SourceHash, long Size, string Ilc, string Sdk);

    /// <summary>
    /// The handoff record for Native AOT binaries built on another box (docs/PLAN-perf-rebaseline.md section 4): one
    /// tab-separated line per binary, <c>leg program sha256 sourceHash size ilc sdk</c>, with <c>#</c> comments. A
    /// hash certifies transport, never measurement identity, so admission also requires the publishing box's
    /// converted source to be this box's.
    /// </summary>
    internal static class AotManifest
    {
        internal static IReadOnlyList<AotManifestEntry> Parse(string text)
        {
            List<AotManifestEntry> entries = new();
            int lineNumber = 0;

            foreach (string raw in text.Replace("\r", "").Split('\n'))
            {
                lineNumber++;
                string line = raw.Trim();

                if (line.Length == 0 || line.StartsWith('#'))
                    continue;

                string[] fields = line.Split('\t');

                if (fields.Length != 7)
                    throw new FormatException($"AOT manifest line {lineNumber}: {fields.Length} fields, want 7 (leg program sha256 sourceHash size ilc sdk)");

                if (!long.TryParse(fields[4], out long size) || size <= 0)
                    throw new FormatException($"AOT manifest line {lineNumber}: size \"{fields[4]}\" is not a positive integer");

                entries.Add(new AotManifestEntry(fields[0], fields[1], fields[2].ToLowerInvariant(), fields[3].ToLowerInvariant(), size, fields[5], fields[6]));
            }

            foreach (IGrouping<(string, string), AotManifestEntry> twice in entries.GroupBy(e => (e.Leg, e.Program)).Where(g => g.Count() > 1))
                throw new FormatException($"AOT manifest: {twice.Key.Item1}/{twice.Key.Item2} is listed {twice.Count()} times");

            return entries;
        }

        /// <summary>
        /// Why the AOT slot of <paramref name="leg"/>/<paramref name="program"/> may not be timed, or null when its
        /// binary is admitted. Every refusal names the binary, and none is ever filled from another build.
        /// </summary>
        internal static string? Refusal(string leg, string program, AotManifestEntry? entry, bool binaryPresent,
            string? actualSha256, long actualSize, string localSourceHash)
        {
            string slot = $"{leg}/{program}";

            if (entry is null)
            {
                return binaryPresent
                    ? $"{slot}: an AOT binary with no manifest entry is never timed (it is not the shipped one)"
                    : $"{slot}: no AOT binary was shipped";
            }

            if (!binaryPresent)
                return $"{slot}: the manifest lists it ({entry.Sha256[..12]}...), but the binary is missing";

            if (!string.Equals(actualSha256, entry.Sha256, StringComparison.OrdinalIgnoreCase))
                return $"{slot}: sha256 {actualSha256} does not match the manifest's {entry.Sha256}; re-ship it, never measure it";

            if (actualSize != entry.Size)
                return $"{slot}: {actualSize} bytes, the manifest says {entry.Size}";

            if (!string.Equals(localSourceHash, entry.SourceHash, StringComparison.OrdinalIgnoreCase))
                return $"{slot}: the publishing box converted a different program (source {entry.SourceHash[..12]}..., here {localSourceHash[..Math.Min(12, localSourceHash.Length)]}...)";

            return null;
        }
    }

    /// <summary>
    /// Whether a built binary has the shape its column claims, read from the <c>runtimeconfig.json</c> beside it.
    /// </summary>
    internal static class BinaryShape
    {
        internal const string DynamicCodeSwitch = "System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported";

        /// <summary>
        /// The JIT column: framework-dependent, dynamic code enabled. Null when the file says so.
        /// </summary>
        internal static string? JitProblem(string? runtimeConfig)
        {
            if (!TryOptions(runtimeConfig, out JsonElement options, out string? problem))
                return problem;

            if (options.TryGetProperty("includedFrameworks", out _))
                return "self-contained (includedFrameworks present), but the JIT row is documented as framework-dependent -- the output tree has been overwritten by a publish";

            if (options.TryGetProperty("configProperties", out JsonElement props) &&
                props.TryGetProperty(DynamicCodeSwitch, out JsonElement dynamicCode) &&
                dynamicCode.ValueKind == JsonValueKind.False)
                return $"{DynamicCodeSwitch} is false -- every reflective invoker would run its non-emitting fallback";

            return null;
        }

        /// <summary>
        /// The ReadyToRun column: the template-default publish is trimmed, so self-contained, and still jitted, so
        /// dynamic code stays enabled. Null when the file says so.
        /// </summary>
        internal static string? ReadyToRunProblem(string? runtimeConfig)
        {
            if (!TryOptions(runtimeConfig, out JsonElement options, out string? problem))
                return problem;

            if (!options.TryGetProperty("includedFrameworks", out _))
                return "framework-dependent (no includedFrameworks), but the template-default publish is trimmed and therefore self-contained";

            if (options.TryGetProperty("configProperties", out JsonElement props) &&
                props.TryGetProperty(DynamicCodeSwitch, out JsonElement dynamicCode) &&
                dynamicCode.ValueKind == JsonValueKind.False)
                return $"{DynamicCodeSwitch} is false -- that is a Native AOT shape, not ReadyToRun";

            return null;
        }

        /// <summary>
        /// Both ReadyToRun arms: the program's own assembly carries precompiled code (a CLI header whose
        /// ManagedNativeHeader directory is non-empty). A runtimeconfig cannot say this, and a self-contained publish
        /// without ReadyToRun has the same one. Null when it does.
        /// </summary>
        internal static string? ReadyToRunCodeProblem(string assemblyPath) =>
            ReadsReadyToRunCode(assemblyPath, out string? problem) switch
            {
                null => problem,
                true => null,
                false => $"{Path.GetFileName(assemblyPath)} carries no ReadyToRun code (empty ManagedNativeHeader) -- every method would be jitted",
            };

        // The converted project's template target (seat #53, part 3) that publishes the program's own code IL-only.
        internal const string OwnCodeTarget = "GoKeepOwnCodeOutOfReadyToRun";

        /// <summary>
        /// Whether a converted project's template keeps the program's own code out of ReadyToRun: the target is in
        /// templates from the .2 release on, and no headline publish sets the GoReadyToRunUserCode opt-in that turns it off.
        /// </summary>
        internal static bool KeepsOwnCodeOutOfReadyToRun(string csproj) =>
            csproj.Contains($"<Target Name=\"{OwnCodeTarget}\"", StringComparison.Ordinal);

        /// <summary>
        /// Both ReadyToRun arms, as the converted project's template publishes them. With <paramref name="ownCodeIlOnly"/>
        /// the program's own assembly is IL-only and golib carries ReadyToRun code; without it the program's own
        /// assembly carries it. Null when the published files say so; <paramref name="shape"/> says what they showed.
        /// </summary>
        internal static string? ReadyToRunShapeProblem(string programAssembly, string golibAssembly, bool ownCodeIlOnly, out string shape)
        {
            shape = "";

            if (!ownCodeIlOnly)
            {
                string? problem = ReadyToRunCodeProblem(programAssembly);
                shape = problem is null ? "own code ReadyToRun" : "";
                return problem;
            }

            switch (ReadsReadyToRunCode(programAssembly, out string? programProblem))
            {
                case null:
                    return programProblem;
                case true:
                    return $"{Path.GetFileName(programAssembly)} carries ReadyToRun code, but the converted project's template ({OwnCodeTarget}) publishes the program's own code IL-only";
            }

            string? golibProblem = ReadyToRunCodeProblem(golibAssembly);
            shape = golibProblem is null ? "own code IL-only, golib ReadyToRun" : "";
            return golibProblem;
        }

        // True or false for a managed assembly's ReadyToRun code (a non-empty ManagedNativeHeader); null with the problem
        // when the file is missing or not managed.
        private static bool? ReadsReadyToRunCode(string assemblyPath, out string? problem)
        {
            problem = null;

            if (!File.Exists(assemblyPath))
            {
                problem = $"{Path.GetFileName(assemblyPath)} not found";
                return null;
            }

            using FileStream stream = File.OpenRead(assemblyPath);
            using PEReader reader = new(stream);
            CorHeader? cor = reader.PEHeaders.CorHeader;

            if (cor is null)
            {
                problem = $"{Path.GetFileName(assemblyPath)} is not a managed assembly";
                return null;
            }

            return cor.ManagedNativeHeaderDirectory.Size > 0;
        }

        private static bool TryOptions(string? runtimeConfig, out JsonElement options, out string? problem)
        {
            options = default;
            problem = null;

            if (runtimeConfig is null)
            {
                problem = "runtimeconfig.json not found";
                return false;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(runtimeConfig);

                if (!doc.RootElement.TryGetProperty("runtimeOptions", out JsonElement raw))
                {
                    problem = "no runtimeOptions section";
                    return false;
                }

                options = raw.Clone();
                return true;
            }
            catch (JsonException ex)
            {
                problem = $"unreadable runtimeconfig.json ({ex.Message})";
                return false;
            }
        }
    }
}

// AotPublishReuse.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// When PerformanceRunner may reuse a completed Native AOT publish instead of paying for another one. Like
// ReleaseLegs.cs it is a pure decision LINKED into PerformanceRunnerTests, which pins it
// (AotPublishReuseTests), so the runner stays dependency-free.

using System;
using System.IO;

namespace PerformanceRunner
{
    internal static class AotPublishReuse
    {
        /// <summary>
        /// The stamp a successful publish of <paramref name="csproj"/> writes beside its output, and that a
        /// later run must match to reuse it. Empty when the SDK version is unknown, which disables reuse.
        /// </summary>
        public static string Stamp(string sdkVersion, string config, string csproj, string srcRoot) =>
            sdkVersion.Length > 0 ? $"sdk={sdkVersion};config={config};rid=win-x64;mode=PerfAot" : "";

        /// <summary>
        /// Whether the publish at <paramref name="exe"/> is current for the project at
        /// <paramref name="projPath"/>: it exists, its stamp matches <paramref name="currentStamp"/>, and it
        /// is newer than the converter, the project's csproj and every one of its .cs.
        /// </summary>
        public static bool IsUpToDate(string exe, string stampPath, string currentStamp, string projPath, string csproj, string converterExe, out string why)
        {
            why = "";

            if (!File.Exists(exe) || !File.Exists(stampPath))
                return false;

            if (!string.Equals(File.ReadAllText(stampPath).Trim(), currentStamp, StringComparison.Ordinal))
                return false;

            DateTime exeTime = File.GetLastWriteTimeUtc(exe);

            if (exeTime <= File.GetLastWriteTimeUtc(converterExe))
                return false;

            if (exeTime <= File.GetLastWriteTimeUtc(csproj))
                return false;

            foreach (string cs in Directory.GetFiles(projPath, "*.cs", SearchOption.AllDirectories))
            {
                // bin holds the publish outputs themselves (and obj the intermediates); neither is
                // an input, and the output can never be newer than a file the publish itself wrote.
                // Generated holds the source generators' EmitCompilerGeneratedFiles copies -- also
                // OUTPUT, refreshed by any compile of the project, so counting it as an input made
                // the predicate self-invalidating: the run's own JIT build phase re-emitted them
                // and every publish older than that build spuriously republished (measured
                // 2026-08-25, PerfIfaceCall -- a farm-adopted row re-paid 3.3h for a .g.cs whose
                // real inputs had not moved). The generators' true upstream is the analyzer
                // assembly, checked explicitly below.
                string rel = Path.GetRelativePath(projPath, cs);

                if (rel.StartsWith("bin", StringComparison.OrdinalIgnoreCase) || rel.StartsWith("obj", StringComparison.OrdinalIgnoreCase) || rel.StartsWith("Generated", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (exeTime <= File.GetLastWriteTimeUtc(cs))
                    return false;
            }

            // Deliberately NOT an input: the go2cs-gen analyzer assembly. Its mtime is rewritten
            // by ordinary dependency builds of UNCHANGED sources (measured 2026-08-25: a
            // benchmark's publish re-touched bin/Release/.../go2cs-gen.dll while nothing in the
            // analyzer moved), so as a signal it is the same self-poisoning class as Generated.
            // The accepted gap: an analyzer-SOURCE change can alter emitted C# with no
            // project-local input moving -- such a change owes a manual bin purge (or a stamp
            // scheme bump) before the next measured run, the same discipline a converter change
            // already owes the corpus.
            why = $"output newer than all inputs, stamp matches [{currentStamp}]";
            return true;
        }
    }
}

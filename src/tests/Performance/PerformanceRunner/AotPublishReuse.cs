// AotPublishReuse.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// When PerformanceRunner may reuse a completed Native AOT publish instead of paying for another one. Like
// ReleaseLegs.cs it is a pure decision LINKED into PerformanceRunnerTests, which pins it
// (AotPublishReuseTests), so the runner stays dependency-free.
//
// A program's own .cs, its csproj and go2cs.exe are not its only build inputs: golib, the go2cs-gen
// analyzer and every converted package it references compile into it too. Until parity seat #37 the
// predicate read none of them, so a golib-only change's tip arm reused the base arm's binary and timed
// it. Their identity is their CONTENT, carried in the stamp, rather than a write time: ordinary builds of
// unchanged sources rewrite the analyzer dll's time (measured 2026-08-25), and a farm-adopted binary
// arrives with foreign times.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PerformanceRunner
{
    internal static class AotPublishReuse
    {
        /// <summary>
        /// The stamp a successful publish of <paramref name="csproj"/> writes beside its output, and that a
        /// later run must match to reuse it. Empty when the SDK version is unknown, which disables reuse.
        /// </summary>
        public static string Stamp(string sdkVersion, string config, string csproj, string srcRoot) =>
            sdkVersion.Length > 0 ? $"sdk={sdkVersion};config={config};rid=win-x64;mode=PerfAot;deps={DependencyIdentity(csproj, srcRoot)}" : "";

        /// <summary>
        /// A content hash of what <paramref name="csproj"/> compiles in besides its own folder: every project
        /// in its ProjectReference closure (golib, the go2cs-gen analyzer, the converted packages), the shared
        /// projects those import (golib's core/go2cs), and the MSBuild build files on the way up to
        /// <paramref name="srcRoot"/>. A SUPERSET of what MSBuild reads, so it can only re-publish too often.
        /// </summary>
        public static string DependencyIdentity(string csproj, string srcRoot)
        {
            string root = Path.GetFullPath(csproj);
            SortedSet<string> projects = new(StringComparer.Ordinal);
            Queue<string> pending = new([root]);

            while (pending.Count > 0)
            {
                string project = pending.Dequeue();

                if (!File.Exists(project))
                    continue;

                foreach (string reference in Includes(project, "ProjectReference Include=\"", srcRoot))
                {
                    if (projects.Add(reference))
                        pending.Enqueue(reference);
                }
            }

            SortedSet<string> files = new(StringComparer.Ordinal);

            foreach (string project in projects)
                AddTree(files, Path.GetDirectoryName(project)!, isRoot: true);

            foreach (string project in projects.Append(root))
            {
                // A shared project (*.projitems) compiles its folder's sources into the importer.
                foreach (string import in Includes(project, "Import Project=\"", srcRoot).Where(path => path.EndsWith(".projitems", StringComparison.OrdinalIgnoreCase)))
                    AddTree(files, Path.GetDirectoryName(import)!, isRoot: true);

                AddBuildFiles(files, Path.GetDirectoryName(project)!, Path.GetFullPath(srcRoot));
            }

            using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            foreach (string file in files)
            {
                hash.AppendData(Encoding.UTF8.GetBytes(Path.GetRelativePath(srcRoot, file).Replace('\\', '/') + "\n"));

                // Carriage returns dropped: a windows and a linux checkout of one commit read the same.
                hash.AppendData(File.ReadAllBytes(file).Where(b => b != (byte)'\r').ToArray());
                hash.AppendData([0]);
            }

            return Convert.ToHexString(hash.GetHashAndReset())[..16].ToLowerInvariant();
        }

        // The literal paths an MSBuild file names after `marker`, resolved the way MSBuild resolves them:
        // $(go2csPath) is the src root, $(MSBuildThisFileDirectory) and a relative path are the file's own
        // folder. A path built from any other property is not followed.
        private static IEnumerable<string> Includes(string project, string marker, string srcRoot)
        {
            string projectDir = Path.GetDirectoryName(project)!;

            foreach (string line in File.ReadLines(project))
            {
                int index = line.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

                if (index < 0)
                    continue;

                int start = index + marker.Length;
                int end = line.IndexOf('"', start);

                if (end < 0)
                    continue;

                string raw = line[start..end]
                    .Replace("$(go2csPath)", srcRoot.TrimEnd('/', '\\') + "/")
                    .Replace("$(MSBuildThisFileDirectory)", projectDir + "/")
                    .Replace('\\', '/');

                if (!raw.Contains("$("))
                    yield return Path.GetFullPath(raw, projectDir);
            }
        }

        // Every file under a project folder but build output (bin, obj, the generators' Generated copies) and
        // the folders of other projects nested in it, which are inputs only when the closure reaches them.
        private static void AddTree(SortedSet<string> files, string dir, bool isRoot)
        {
            if (!Directory.Exists(dir) || !isRoot && Directory.EnumerateFiles(dir, "*.csproj").Any())
                return;

            foreach (string file in Directory.EnumerateFiles(dir))
                files.Add(file);

            foreach (string child in Directory.EnumerateDirectories(dir))
            {
                string name = Path.GetFileName(child);

                if (name is "bin" or "obj" || name.StartsWith("Generated", StringComparison.Ordinal))
                    continue;

                AddTree(files, child, isRoot: false);
            }
        }

        // The build files MSBuild imports implicitly or by convention, from a project's folder up to the src root.
        private static void AddBuildFiles(SortedSet<string> files, string dir, string srcRoot)
        {
            for (string? current = Path.GetFullPath(dir); current is not null; current = Path.GetDirectoryName(current))
            {
                foreach (string name in new[] { "Directory.Build.props", "Directory.Build.targets", "version.props" })
                {
                    string path = Path.Combine(current, name);

                    if (File.Exists(path))
                        files.Add(path);
                }

                if (string.Equals(current.TrimEnd(Path.DirectorySeparatorChar), srcRoot.TrimEnd(Path.DirectorySeparatorChar), StringComparison.Ordinal))
                    break;
            }
        }

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
                // assembly, covered by the stamp (below).
                string rel = Path.GetRelativePath(projPath, cs);

                if (rel.StartsWith("bin", StringComparison.OrdinalIgnoreCase) || rel.StartsWith("obj", StringComparison.OrdinalIgnoreCase) || rel.StartsWith("Generated", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (exeTime <= File.GetLastWriteTimeUtc(cs))
                    return false;
            }

            // Deliberately NOT an input here: the go2cs-gen analyzer assembly. Its mtime is rewritten
            // by ordinary dependency builds of UNCHANGED sources (measured 2026-08-25: a
            // benchmark's publish re-touched bin/Release/.../go2cs-gen.dll while nothing in the
            // analyzer moved), so as a signal it is the same self-poisoning class as Generated.
            // The analyzer's SOURCE, golib's and the referenced packages' are inputs through the
            // stamp's deps= identity (DependencyIdentity), which currentStamp already carries.
            why = $"output newer than all inputs, stamp matches [{currentStamp}]";
            return true;
        }
    }
}

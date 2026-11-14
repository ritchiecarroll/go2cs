// AssemblyQualifiedNameTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// Every assembly-qualified type name that golib and the hand-owned C# under <c>src/core</c> spell as a
/// string resolves to a real type, and every assembly name they spell loads.
/// </summary>
/// <remarks>
/// <para>
/// A name looked up by string fails SILENTLY. <c>GoMemProfile</c> asks
/// <c>Type.GetType("go.runtime.pprof_package, runtime.pprof", throwOnError: false)</c>: were runtime/pprof's
/// assembly renamed and the literal not, the lookup answers null and the memory profile quietly turns off.
/// The host's flag and syscall bridges, and the trimmer's <c>[DynamicDependency]</c> roots, are the same
/// shape. Nothing else notices, so this guard reads the tree for every such string and resolves each one.
/// </para>
/// <para>
/// Three shapes are read: a <c>"go.Type, Assembly"</c> literal (a <c>Type.GetType</c> argument, or a named
/// constant that feeds one); a <c>[DynamicDependency(..., "Type", "Assembly")]</c>, the same reference split
/// in two, whose type name the trimmer reads as a documentation ID (a nested type joined by <c>.</c>); and a
/// <c>const string ...AssemblyName = "Assembly"</c>, an assembly named on its own, unless the file defines
/// that assembly itself (<c>DefineDynamicAssembly</c>) rather than loading it.
/// </para>
/// <para>
/// RESOLUTION is the test process's own: every assembly named here is a ProjectReference of this project,
/// so it is deployed beside the test binaries and listed in this project's deps.json, which is where
/// <c>Type.GetType</c> and <c>Assembly.Load</c> find it by name — the same name lookup the program makes.
/// A rename changes the deployed file and the name it answers to, so a literal left at the old name stops
/// resolving here. A new literal naming an assembly this project does not reference fails too; the fix is
/// the ProjectReference, which also makes the guard cover it.
/// </para>
/// </remarks>
[TestClass]
public class AssemblyQualifiedNameTests
{
    // "go.runtime.pprof_package, runtime.pprof": a go2cs type name, a comma, an assembly name.
    private static readonly Regex s_qualifiedName = new(@"""(go\.[\w.+`]+),\s*([\w.]+)""", RegexOptions.Compiled);

    // [DynamicDependency("FromBoxObject", "go.unsafe_package.Pointer", "unsafe")], or with a member-types first argument.
    private static readonly Regex s_dynamicDependency = new(@"DynamicDependency\((?:""[^""]*""\s*,\s*|[^""()]*)""([\w.+`]+)""\s*,\s*""([\w.]+)""\s*\)", RegexOptions.Compiled);

    // private const string PprofAssemblyName = "runtime.pprof";
    private static readonly Regex s_assemblyNameConstant = new(@"const\s+string\s+(\w*AssemblyName)\s*=\s*""([\w.]+)""", RegexOptions.Compiled);

    private static readonly Regex s_manualConversion = new(@"\[module:\s*(go\.)?GoManualConversion\]", RegexOptions.Compiled);

    // golib and the wholly hand-owned trees. Hand-owned FILES inside converted packages are found by their marks.
    private static readonly string[] s_handOwnedTrees = ["golib/", "testing/", "unsafe/", "go2cs.CpuProfiler/"];

    private static string CoreRoot([CallerFilePath] string thisFile = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", "..", "core"));

    private static bool IsHandOwned(string relative, string source) =>
        s_handOwnedTrees.Any(relative.StartsWith) || relative.EndsWith("_impl.cs", StringComparison.Ordinal) || s_manualConversion.IsMatch(source);

    [TestMethod]
    public void EveryAssemblyQualifiedNameInHandOwnedSource_Resolves()
    {
        string root = CoreRoot();

        if (!Directory.Exists(Path.Combine(root, "golib")))
            Assert.Inconclusive($"source not found at {root}; this guard reads the tree it was built from");

        List<string> failures = [];
        int qualifiedNames = 0, dynamicDependencies = 0, assemblyNameConstants = 0;

        foreach (string file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');

            if (relative.Contains("/bin/") || relative.Contains("/obj/"))
                continue;

            string source = File.ReadAllText(file);

            if (!source.Contains('"') || !IsHandOwned(relative, source))
                continue;

            string[] lines = source.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.TrimStart().StartsWith("//", StringComparison.Ordinal))
                    continue;

                string site = $"{relative}:{i + 1}";

                foreach (Match match in s_qualifiedName.Matches(line))
                {
                    qualifiedNames++;

                    if (Type.GetType(match.Value.Trim('"'), throwOnError: false) is null)
                        failures.Add($"{site}: {match.Value} does not resolve to a type");
                }

                foreach (Match match in s_dynamicDependency.Matches(line))
                {
                    dynamicDependencies++;

                    string typeName = match.Groups[1].Value, assemblyName = match.Groups[2].Value;

                    if (LoadOrNull(assemblyName) is not { } assembly)
                        failures.Add($"{site}: DynamicDependency names assembly \"{assemblyName}\", which does not load");
                    else if (FindDocumentationIdType(assembly, typeName) is null)
                        failures.Add($"{site}: DynamicDependency names type \"{typeName}\", which assembly \"{assemblyName}\" does not declare");
                }

                foreach (Match match in s_assemblyNameConstant.Matches(line))
                {
                    string constant = match.Groups[1].Value, assemblyName = match.Groups[2].Value;

                    // An assembly this process DEFINES (GoStructSynthesis' dynamic one) is not one it loads.
                    if (source.Contains($"DefineDynamicAssembly(new AssemblyName({constant})", StringComparison.Ordinal))
                        continue;

                    assemblyNameConstants++;

                    if (LoadOrNull(assemblyName) is null)
                        failures.Add($"{site}: {constant} = \"{assemblyName}\" names an assembly that does not load");
                }
            }
        }

        // Each shape found at least once, or its pattern has rotted and the guard checks nothing.
        Assert.IsTrue(qualifiedNames > 0, "control: no \"go.Type, Assembly\" literal found; the census pattern no longer matches the tree");
        Assert.IsTrue(dynamicDependencies > 0, "control: no [DynamicDependency(..., \"Type\", \"Assembly\")] found; the census pattern no longer matches the tree");
        Assert.IsTrue(assemblyNameConstants > 0, "control: no ...AssemblyName constant found; the census pattern no longer matches the tree");

        Assert.AreEqual(0, failures.Count,
            $"assembly-qualified names that no longer resolve (a renamed assembly, or one this project does not reference):{Environment.NewLine}  " +
            string.Join($"{Environment.NewLine}  ", failures));
    }

    private static Assembly? LoadOrNull(string assemblyName)
    {
        try
        {
            return Assembly.Load(new AssemblyName(assemblyName));
        }
        catch (Exception ex) when (ex is FileNotFoundException or FileLoadException or BadImageFormatException)
        {
            return null;
        }
    }

    // A documentation ID joins a nested type with '.', where reflection joins it with '+'.
    private static Type? FindDocumentationIdType(Assembly assembly, string typeName) =>
        assembly.GetType(typeName, throwOnError: false) ??
        assembly.GetTypes().FirstOrDefault(type => type.FullName?.Replace('+', '.') == typeName);
}

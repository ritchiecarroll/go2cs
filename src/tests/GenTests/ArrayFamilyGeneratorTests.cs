// ArrayFamilyGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// go2cs-arrays-gen, the generator of go.arrays.dll's inline-array family (docs/phase4/DESIGN-array-value-storage.md,
/// 7.7): it generates into go.arrays alone, byte-stable, every length 1-4096 once, and an unanalysed face of a member
/// is a compile error (7.6.1).
/// </summary>
[TestClass]
public class ArrayFamilyGeneratorTests
{
    private static Dictionary<string, string> Generate(string assemblyName)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, [CSharpSyntaxTree.ParseText("")], References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new ArrayFamilyGenerator()).RunGenerators(compilation).GetRunResult();

        return result.Results.Single().GeneratedSources.ToDictionary(source => source.HintName, source => source.SourceText.ToString());
    }

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.IGoEquatable).Assembly.Location));
    }

    // The family's hand-written helpers, compiled with a generated chunk.
    private static string InlineArraysSource()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            string path = Path.Combine(directory.FullName, "src", "core", "go.arrays", "InlineArrays.cs");

            if (File.Exists(path))
                return File.ReadAllText(path);

            path = Path.Combine(directory.FullName, "core", "go.arrays", "InlineArrays.cs");

            if (File.Exists(path))
                return File.ReadAllText(path);
        }

        throw new FileNotFoundException("src/core/go.arrays/InlineArrays.cs not found above " + AppContext.BaseDirectory);
    }

    [TestMethod]
    public void TheFamilyIsGeneratedIntoGoArraysAlone()
    {
        Assert.AreEqual(0, Generate("go.strings").Count, "another compilation gets nothing");
        Assert.AreEqual(ArrayFamilyGenerator.MaxLength / ArrayFamilyGenerator.ChunkSize, Generate(ArrayFamilyGenerator.FamilyAssemblyName).Count);
    }

    // Q5's condition: the packed go.arrays.dll is built from this output, so two runs must give the same bytes.
    [TestMethod]
    public void TheOutputIsByteStable()
    {
        Dictionary<string, string> first = Generate(ArrayFamilyGenerator.FamilyAssemblyName);
        Dictionary<string, string> second = Generate(ArrayFamilyGenerator.FamilyAssemblyName);

        CollectionAssert.AreEquivalent(first.Keys.ToList(), second.Keys.ToList());

        foreach (KeyValuePair<string, string> source in first)
            Assert.AreEqual(source.Value, second[source.Key], $"{source.Key} differs between two runs");
    }

    [TestMethod]
    public void EveryLengthToTheCapIsGeneratedOnce()
    {
        List<int> lengths = Generate(ArrayFamilyGenerator.FamilyAssemblyName).Values
            .SelectMany(source => Regex.Matches(source, @"public struct array(\d+)<T>").Select(match => int.Parse(match.Groups[1].Value)))
            .OrderBy(length => length).ToList();

        CollectionAssert.AreEqual(Enumerable.Range(1, ArrayFamilyGenerator.MaxLength).ToList(), lengths);
    }

    // 7.6.1: a face the converter did not analyse must fail to compile, never become a silent copy. The member exposes
    // an element and its length, and nothing that reaches its storage as a whole.
    [TestMethod]
    public void AnUnanalysedFaceIsACompileError()
    {
        string chunk = ArrayFamilyGenerator.Chunk(193, 256);

        string Compile(string body)
        {
            string consumer = "using System;\nusing go.arrays;\nstruct S22 { public array201<sbyte> Fa; }\nstatic class C\n{\n    static void M(ref S22 s)\n    {\n        " + body + "\n    }\n}\n";

            CSharpCompilation compilation = CSharpCompilation.Create("go.arrays",
                [CSharpSyntaxTree.ParseText(chunk), CSharpSyntaxTree.ParseText(InlineArraysSource()), CSharpSyntaxTree.ParseText(consumer, path: "consumer.cs")],
                References(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            return string.Join(",", compilation.GetDiagnostics()
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Location.SourceTree?.FilePath == "consumer.cs")
                .Select(diagnostic => diagnostic.Id).Distinct().OrderBy(id => id));
        }

        Assert.AreEqual("", Compile("s.Fa[3] = 1; s.Fa[(nint)4] += s.Fa[3]; int n = array201<sbyte>.Length; bool same = s.Fa == s.Fa;"), "the analysed faces compile");
        Assert.AreEqual("CS0029", Compile("Span<sbyte> span = s.Fa;"), "no Span conversion");
        Assert.AreEqual("CS0029", Compile("ReadOnlySpan<sbyte> span = s.Fa;"), "no ReadOnlySpan conversion");
        Assert.AreNotEqual("", Compile("var part = s.Fa[1..3];"), "no slicing");
        Assert.AreEqual("CS1929", Compile("var span = s.Fa.AsSpan();"), "no AsSpan: the extension for arrays and spans does not apply");
    }
}

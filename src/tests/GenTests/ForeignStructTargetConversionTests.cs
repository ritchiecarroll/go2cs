// ForeignStructTargetConversionTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// A Go conversion from a LOCAL struct to a struct declared in ANOTHER package —
/// <c>structs.AssignB(struct{ A int }{3})</c>, where the anonymous source is lifted to a local C# type —
/// is recorded as <c>GoImplicitConv&lt;main_type, structs.AssignB&gt;</c>. The ImplicitConvGenerator read
/// the members to pass to the target's constructor from the TARGET's struct declaration, which a
/// referenced assembly does not have in syntax, so it skipped the record and the cast site was CS0030.
/// Go converts between struct types only when their fields are identical in name, type and order, so
/// the local SOURCE's declaration enumerates the same members.
/// </summary>
[TestClass]
public class ForeignStructTargetConversionTests
{
    private const string Upstream =
        """
        namespace go
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public class GoImplicitConvAttribute<TSource, TTarget> : System.Attribute
            {
                public bool Inverted { get; set; }
                public bool Indirect { get; set; }
                public string? ValueType { get; set; }
            }

            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }

            public static partial class structs_package
            {
                public partial struct AssignB
                {
                    public nint A;

                    public AssignB(nint A = default!) { this.A = A; }
                }
            }
        }
        """;

    private const string Consumer =
        """
        using go;

        [assembly: GoImplicitConv<go.main_package.main_type, go.structs_package.AssignB>]

        namespace go;

        public static partial class main_package
        {
            [GoType("dyn")] internal partial struct main_type
            {
                public nint A;
            }

            internal static nint Convert(main_type value) => ((structs_package.AssignB)value).A;
        }
        """;

    [TestMethod]
    public void AConversionToAForeignStructTargetEnumeratesTheLocalSourceMembers()
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        List<MetadataReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"))
        ];

        CSharpCompilation upstream = CSharpCompilation.Create("foreign-structs", [CSharpSyntaxTree.ParseText(Upstream, parseOptions)], references, libraryOptions);

        using MemoryStream image = new();
        EmitResult emitted = upstream.Emit(image);
        Assert.IsTrue(emitted.Success, $"the upstream assembly must emit clean: {string.Join("; ", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))}");
        references.Add(MetadataReference.CreateFromImage(image.ToArray()));

        CSharpCompilation consumer = CSharpCompilation.Create("foreign-main", [CSharpSyntaxTree.ParseText(Consumer, parseOptions)], references, libraryOptions);

        CSharpGeneratorDriver.Create(new ImplicitConvGenerator())
            .RunGeneratorsAndUpdateCompilation(consumer, out Compilation updated, out _);

        GeneratorRunResult run = CSharpGeneratorDriver.Create(new ImplicitConvGenerator()).RunGenerators(consumer).GetRunResult().Results.Single();

        Assert.IsNull(run.Exception, $"the generator must not throw: {run.Exception}");
        Assert.AreEqual(1, run.GeneratedSources.Length, "the record to a foreign struct target must generate its operator");
        StringAssert.Contains(run.GeneratedSources[0].SourceText.ToString(), "(src.A)");

        Diagnostic[] errors = [.. updated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
        Assert.AreEqual(0, errors.Length, $"the cast must compile with the generated operator: {string.Join("; ", errors.Select(error => error.ToString()))}");
    }
}

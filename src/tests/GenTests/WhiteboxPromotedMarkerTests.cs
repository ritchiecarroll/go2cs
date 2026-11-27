// WhiteboxPromotedMarkerTests.cs - Gbtc
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
/// A member internal to another assembly's interface, which a struct meets only by promotion, is stubbed in the
/// struct's value implementation only where this assembly cannot reach the type that provides it. An internal
/// (white-box) test file's <c>type fake struct{ prodT }</c>, converted to its package's own interface with an
/// unexported method, reaches prodT's method through the InternalsVisibleTo grant the test model mints, so the value
/// implementation forwards to it; a stub there compiles and answers a silent zero value in a <c>-tests</c> host.
/// Without the grant the same shape is the package-sealing marker, and the stub stands in as before.
/// </summary>
[TestClass]
public class WhiteboxPromotedMarkerTests
{
    private const string ProductionSource =
        """
        using System.Runtime.CompilerServices;

        [assembly: InternalsVisibleTo("whitebox-test")]

        namespace go
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public class GoImplementAttribute<TStruct, TInterface> : System.Attribute
            {
                public bool Promoted { get; set; }
                public bool Pointer { get; set; }
            }
        }

        namespace go.example.com
        {
            public static partial class lib_package
            {
                public partial struct prodT { public int N; }

                internal static int m(this prodT t) => 7;

                internal partial interface I
                {
                    int m();
                }
            }
        }
        """;

    private const string TestSource =
        """
        using go;

        [assembly: GoImplement<global::go.example.com.lib_internal_test_package.fake, global::go.example.com.lib_package.I>]

        namespace go.example.com
        {
            public static partial class lib_internal_test_package
            {
                internal partial struct fake
                {
                    public partial ref global::go.example.com.lib_package.prodT prodT { get; }
                }
            }
        }
        """;

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
    }

    // The value implementation the generator writes for (fake, I) in a test assembly of the given name; production
    // grants internals access to "whitebox-test" only.
    private static string ValueImplementation(string testAssemblyName)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);

        CSharpCompilation production = CSharpCompilation.Create(
            "whitebox-marker-production",
            [CSharpSyntaxTree.ParseText(ProductionSource, parseOptions)],
            CoreReferences(),
            libraryOptions);

        using MemoryStream image = new();
        EmitResult emitted = production.Emit(image);
        Assert.IsTrue(emitted.Success, $"production compilation must emit clean: {string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))}");

        CSharpCompilation test = CSharpCompilation.Create(
            testAssemblyName,
            [CSharpSyntaxTree.ParseText(TestSource, parseOptions)],
            CoreReferences().Append(MetadataReference.CreateFromImage(image.ToArray())),
            libraryOptions);

        GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new ImplementGenerator()).RunGenerators(test).GetRunResult();

        foreach (GeneratorRunResult each in result.Results)
            Assert.IsNull(each.Exception, $"the generator must not throw: {each.Exception}");

        List<string> matches = result.Results
            .SelectMany(generator => generator.GeneratedSources)
            .Where(source => source.HintName.Contains("_package.fake-") && !source.HintName.EndsWith("-ptr.g.cs"))
            .Select(source => source.SourceText.ToString())
            .ToList();

        Assert.AreEqual(1, matches.Count, "one value implementation for (fake, I)");
        return matches[0];
    }

    [TestMethod]
    public void AGrantedTestAssemblyForwardsThePromotedMember()
    {
        string implementation = ValueImplementation("whitebox-test");

        StringAssert.Contains(implementation, "this.prodT", "the member forwards through the embed: " + implementation);
        Assert.IsFalse(implementation.Contains("default!"), "a reachable member must not be stubbed: " + implementation);
    }

    [TestMethod]
    public void AnUngrantedAssemblyStubsTheMarker()
    {
        string implementation = ValueImplementation("other-test");

        StringAssert.Contains(implementation, "=> default!;", "the unreachable member is the sealing marker: " + implementation);
    }
}

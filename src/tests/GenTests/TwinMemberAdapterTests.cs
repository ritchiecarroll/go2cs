// TwinMemberAdapterTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using go2cs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenTests;

/// <summary>
/// M:N (experimental, opt-in), stage (c) (2026-11-15): under the interface twin shape `default`, a twinned interface carries a
/// default-bodied hidden member beside each marked one (`Readꞏtwin` beside `Read`). An adapter must not forward it: the
/// implementer's own twin may not exist (refused: by-ref receiver), and then `m_box.Readꞏtwin(p)` binds nothing
/// (CS1929, encoding/base64's decoder). The interface's default body is the sync fallback; the twin generator adds a
/// twin forwarder where the implementer really has a twin (its forwarders, in the adapter's second part).
/// </summary>
[TestClass]
public class TwinMemberAdapterTests
{
    private const string ProductionSource =
        """
        namespace go
        {
            public class ж<T>
            {
                public static readonly ж<T> NilBox = new(default!);
                public ж(T value) { Value = value; }
                public T Value;
            }

            public interface IжAdapter { object? Box { get; } }

            public static class AdapterRegistry
            {
                public static void Register(System.Type boxType, System.Type interfaceType, System.Func<object, object> factory) { }
            }

            public sealed class GoDeferredRegistrations
            {
                public static readonly GoDeferredRegistrations Adapters = new();
                public void Defer(System.Action register) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public class GoImplementAttribute<TStruct, TInterface> : System.Attribute
            {
                public bool Promoted { get; set; }
                public bool Pointer { get; set; }
                public bool ConstraintProxy { get; set; }
            }

            public static partial class io_package
            {
                // io.Reader as a package built with interface twins (shape `default`) emits it: the marked member and
                // its hidden default-bodied twin.
                public partial interface Reader
                {
                    int Read(int p);

                    object Readꞏtwin(int p) => Read(p);
                }
            }
        }
        """;

    private const string ConsumingSource =
        """
        using go;

        [assembly: GoImplement<global::go.probe_package.decoder, global::go.io_package.Reader>(Pointer = true)]

        namespace go;

        public static partial class probe_package
        {
            public partial struct decoder
            {
            }

            // A pointer-receiver method with NO twin (the twin generator refused it: by-ref receiver).
            internal static int Read(this ж<decoder> Ꮡd, int p) => p;
        }
        """;

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
    }

    [TestMethod]
    public void AnAdapterDoesNotForwardAnInterfacesTwinMember()
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);

        CSharpCompilation production = CSharpCompilation.Create("twin-member-production",
            [CSharpSyntaxTree.ParseText(ProductionSource, parseOptions)], CoreReferences(), libraryOptions);

        using MemoryStream image = new();
        EmitResult emitted = production.Emit(image);
        Assert.IsTrue(emitted.Success, string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));

        CSharpCompilation consuming = CSharpCompilation.Create("twin-member-test",
            [CSharpSyntaxTree.ParseText(ConsumingSource, parseOptions)],
            CoreReferences().Append(MetadataReference.CreateFromImage(image.ToArray())), libraryOptions);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ImplementGenerator())
            .RunGeneratorsAndUpdateCompilation(consuming, out Compilation updated, out ImmutableArray<Diagnostic> _);

        string adapters = string.Join("\n", driver.GetRunResult().Results.SelectMany(result => result.GeneratedSources).Select(source => source.SourceText.ToString()));
        Diagnostic[] errors = updated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        StringAssert.Contains(adapters, "global::go.io_package.Reader.Read(", "the adapter forwards Read");
        Assert.IsFalse(adapters.Contains("Readꞏtwin", System.StringComparison.Ordinal), "the adapter must not forward the twin member:\n" + adapters);
        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
    }
}

// ConvertedGoTypeRuleTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Guards the converted-Go-type rule (Common.IsConvertedGoTypeDeclaration) that lets the converter stop
/// writing a plain <c>[GoType]</c>: a struct or interface declared directly inside a <c>*_package</c>
/// class, in a converted file, not a descriptor carrier, in an assembly that does not opt out with
/// <c>[assembly: GoHandOwnedPackage]</c>, is generated for exactly as if it carried the attribute, and
/// its generated part re-emits <c>[GoType]</c> so reflection reads it as before. Each exclusion arm is a
/// type the rule must NOT select: a hand-written companion, a whole-file hand conversion, a metadata
/// part, a descriptor carrier, a nested type, and the hand-owned assembly.
/// </summary>
[TestClass]
public class ConvertedGoTypeRuleTests
{
    private const string GolibSource =
        """
        namespace go
        {
            public class ж<T> { public ж(T value) { } public T Value = default!; }

            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Interface | System.AttributeTargets.Class)]
            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }

            public class GoLocalNameAttribute : System.Attribute { public GoLocalNameAttribute(string name) { } }

            [System.AttributeUsage(System.AttributeTargets.Module)]
            public class GoManualConversionAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Assembly)]
            public class GoHandOwnedPackageAttribute : System.Attribute { }
        }
        """;

    private const string Converted =
        """
        namespace go
        {
            public static partial class rule_package
            {
                public partial struct Plain { public int X; }

                [GoType] public partial struct Marked { public int X; }

                public partial interface Shape { int Area(); }

                // Descriptor carrier for `Token` -- uninhabited; never a Go type anything is emitted for.
                [GoLocalName("Token")] public interface Tokenᴅ { }

                public delegate int Fn(int x);

                public partial struct Outer
                {
                    public partial struct Nested { public int Y; }
                }
            }
        }
        """;

    private const string Companion =
        """
        namespace go
        {
            public static partial class rule_package
            {
                internal partial struct Helper { public int Z; }

                [GoType] internal partial struct OptedIn { public int Z; }
            }
        }
        """;

    private const string ManualFile =
        """
        [module: go.GoManualConversion]

        namespace go
        {
            public static partial class rule_package
            {
                internal partial struct Manual { public int W; }
            }
        }
        """;

    private const string PackageInfo =
        """
        namespace go
        {
            public static partial class rule_package
            {
                public partial struct Plain {}
                public partial struct Marked {}
            }
        }
        """;

    private static string[] Generate(params (string source, string path)[] files)
    {
        CSharpCompilation compilation = CSharpCompilation.Create("rule",
            files.Select(file => CSharpSyntaxTree.ParseText(file.source, path: file.path)).Prepend(CSharpSyntaxTree.ParseText(GolibSource, path: "golib.cs")),
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TypeGenerator()).RunGenerators(compilation);

        return driver.GetRunResult().GeneratedTrees.Select(tree => tree.ToString()).ToArray();
    }

    private static string? PartFor(string[] generated, string kind, string name) =>
        generated.FirstOrDefault(text => text.Contains($" partial {kind} {name}"));

    private static readonly (string, string)[] Package =
    [
        (Converted, @"C:\go2cs\src\core\rule\rule.cs"),
        (Companion, @"C:\go2cs\src\core\rule\rule_impl.cs"),
        (ManualFile, @"C:\go2cs\src\core\rule\manual.cs"),
        (PackageInfo, @"C:\go2cs\src\core\rule\package_info.cs")
    ];

    [TestMethod]
    public void AnUnmarkedConvertedStructIsGeneratedAndReemitsGoType()
    {
        string? part = PartFor(Generate(Package), "struct", "Plain");

        Assert.IsNotNull(part, "a struct declared in a package class in a converted file is a Go type without the attribute");
        StringAssert.Contains(part, "[GoType] [", "its generated part re-emits [GoType] so reflection reads it as before");
    }

    [TestMethod]
    public void AnUnmarkedConvertedInterfaceIsGeneratedAndReemitsGoType()
    {
        string? part = PartFor(Generate(Package), "interface", "Shape");

        Assert.IsNotNull(part, "an interface declared in a package class in a converted file is a Go type without the attribute");
        StringAssert.Contains(part, "[GoType] ", "its generated part re-emits [GoType]");
    }

    [TestMethod]
    public void AMarkedTypeIsGeneratedWithoutASecondGoType()
    {
        string[] generated = Generate(Package);

        foreach (string name in new[] { "Marked", "OptedIn" })
        {
            string? part = PartFor(generated, "struct", name);
            Assert.IsNotNull(part, $"{name} carries [GoType] and is generated as before");
            Assert.IsFalse(part!.Contains("[GoType] "), $"{name}'s declaration already carries [GoType]; a second one is CS0579");
        }
    }

    [TestMethod]
    public void NothingOutsideTheRuleIsGenerated()
    {
        string[] generated = Generate(Package);

        Assert.IsNull(PartFor(generated, "struct", "Helper"), "a hand-written companion (*_impl.cs) is not converted code");
        Assert.IsNull(PartFor(generated, "struct", "Manual"), "a [module: GoManualConversion] file is not converted code");
        Assert.IsNull(PartFor(generated, "interface", "Tokenᴅ"), "a descriptor carrier draws no generated output");
        Assert.IsNull(PartFor(generated, "struct", "Nested"), "only a type declared DIRECTLY in the package class is a Go type");
        Assert.IsFalse(generated.Any(text => text.Contains(" Fn(")), "a delegate is never a Go type for the generators");
        Assert.AreEqual(1, generated.Count(text => text.Contains(" partial struct Plain")), "the package_info.cs accessibility part is not a second definition");
    }

    [TestMethod]
    public void ABridgeUnitRecordIsNotASecondDefinition()
    {
        // Section 11 rows 1+2: a white-box bridge records a lifted type's [GoLocalName] on an attribute-only
        // partial in its own metadata unit. Seen FIRST, that partial must not be the type's rule-selected
        // declaration, or the type is generated from a declaration with no definition comment.
        const string converted = "namespace go { public static partial class rule_package { internal partial struct Lifted /*[]int*/; } }";

        foreach (string unit in new[] { "package_info_internal_test.cs", "package_info_external_test.cs" })
        {
            const string record = "namespace go { public static partial class rule_package { [GoLocalName(\"Lifted\")] partial struct Lifted {} } }";

            string[] generated = Generate(
                (record, $"C:/go2cs/src/core/rule/{unit}"),
                (converted, "C:/go2cs/src/core/rule/rule_test.cs"));

            string[] parts = generated.Where(text => text.Contains(" partial struct Lifted")).ToArray();

            Assert.AreEqual(1, parts.Length, $"{unit}: one generated part");
            StringAssert.Contains(parts[0], "[GoType(\"[]int\")] ", $"{unit}: the part is generated from the declaration that carries the definition");
        }
    }

    [TestMethod]
    public void AHandOwnedAssemblySelectsOnlyByTheAttribute()
    {
        string[] generated = Generate([.. Package, ("[assembly: go.GoHandOwnedPackage]", @"C:\go2cs\src\core\rule\opt_out.cs")]);

        Assert.IsNull(PartFor(generated, "struct", "Plain"), "inside a hand-owned package a type is a Go type only by the attribute");
        Assert.IsNull(PartFor(generated, "interface", "Shape"), "inside a hand-owned package a type is a Go type only by the attribute");
        Assert.IsNotNull(PartFor(generated, "struct", "Marked"), "the attribute still selects inside a hand-owned package");
    }

    [TestMethod]
    public void ATypeIsGeneratedOnceWhateverItsDeclarations()
    {
        const string first = "namespace go { public static partial class rule_package { public partial struct Dual { public int A; } public partial struct Split { public int A; } } }";
        const string second = "namespace go { public static partial class rule_package { public partial struct Split { public int B; } } }";
        const string optIn = "namespace go { public static partial class rule_package { [GoType] public partial struct Dual { public int B; } } }";

        string[] generated = Generate(
            (first, @"C:\go2cs\src\core\rule\a.cs"),
            (second, @"C:\go2cs\src\core\rule\b.cs"),
            (optIn, @"C:\go2cs\src\core\rule\dual_impl.cs"));

        Assert.AreEqual(1, generated.Count(text => text.Contains(" partial struct Dual")), "a hand-written [GoType] partial's type is generated once, by its attribute");
        Assert.IsFalse(PartFor(generated, "struct", "Dual")!.Contains("[GoType] "), "its declared [GoType] is the only one (CS0579 otherwise)");
        Assert.AreEqual(1, generated.Count(text => text.Contains(" partial struct Split")), "a type declared in two converted files is generated once");
    }

    [TestMethod]
    public void TheGeneratedOutputIsTheMarkedOutputPlusTheAttribute()
    {
        string unmarked = string.Join("\n", Generate((Converted.Replace("[GoType] public partial struct Marked", "public partial struct Marked"), @"C:\go2cs\src\core\rule\rule.cs")));
        string marked = string.Join("\n", Generate((Converted
            .Replace("public partial struct Plain", "[GoType] public partial struct Plain")
            .Replace("public partial interface Shape", "[GoType] public partial interface Shape")
            .Replace("public partial struct Outer", "[GoType] public partial struct Outer"), @"C:\go2cs\src\core\rule\rule.cs")));

        Assert.IsFalse(marked.Contains("[GoType] "), "with every Go type marked, nothing is re-emitted");
        Assert.AreEqual(marked, unmarked.Replace("[GoType] ", ""), "removing the plain attribute changes the generated output by the re-emitted attribute only");
        Assert.AreEqual(4, unmarked.Split(["[GoType] "], System.StringSplitOptions.None).Length - 1, "one re-emitted attribute per unmarked type (Plain, Marked, Shape, Outer)");
    }
}

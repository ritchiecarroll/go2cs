// LiftedAliasAccessibilityTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// sqlite-cs S16: an exported field typed by an UNEXPORTED alias of an anonymous struct. The struct lifts to a type
/// named after the alias (<c>t__WIDL_name_00000003ᴛ1</c>), and the TypeGenerator scopes a type by its name unless
/// the converter's declaration carries an explicit modifier. The converter now publicizes such a lift and declares it
/// <c>public</c>; these tests pin the generator's half of that agreement: the explicit modifier wins over the name,
/// and the public owner's accessor over the field is then legal C#.
/// </summary>
[TestClass]
public class LiftedAliasAccessibilityTests
{
    private const string Source =
        """
        namespace go
        {
            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }

            partial class demo_package
            {
                [GoType] MODIFIERpartial struct t__WIDL_name_00000003ᴛ1 {
                    public int FhInproc;
                }

                [GoType] public partial struct TuserHPALETTEᴛ1 {
                    public int FfContext;
                    public t__WIDL_name_00000003ᴛ1 Fu;
                }
            }
        }
        """;

    private static (Dictionary<string, string> Sources, string[] Errors) RunTypeGenerator(string modifier)
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create("lifted-alias-accessibility-test",
            [CSharpSyntaxTree.ParseText(Source.Replace("MODIFIER", modifier), new CSharpParseOptions(LanguageVersion.Latest))],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"))
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TypeGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        Dictionary<string, string> sources = driver.GetRunResult().Results
            .SelectMany(generator => generator.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());

        // Only the accessibility diagnostics are this test's subject; the stub compilation lacks golib, so the
        // generated members' other references do not bind.
        string[] errors = output.GetDiagnostics()
            .Where(diagnostic => diagnostic.Id is "CS0050" or "CS0052" or "CS0262")
            .Select(diagnostic => diagnostic.ToString())
            .ToArray();

        return (sources, errors);
    }

    private static string GeneratedFor(Dictionary<string, string> sources, string type)
    {
        string? key = sources.Keys.FirstOrDefault(hint => hint.Contains($".{type}."));
        Assert.IsNotNull(key, $"the TypeGenerator must generate {type}; got: {string.Join(", ", sources.Keys)}");
        return sources[key!];
    }

    [TestMethod]
    public void AnExplicitlyPublicLiftIsGeneratedPublicWhateverItsName()
    {
        (Dictionary<string, string> sources, string[] errors) = RunTypeGenerator("public ");

        StringAssert.Contains(GeneratedFor(sources, "t__WIDL_name_00000003ᴛ1"), "public partial struct t__WIDL_name_00000003ᴛ1");
        StringAssert.Contains(GeneratedFor(sources, "TuserHPALETTEᴛ1"), "public static ref global::go.demo_package.t__WIDL_name_00000003ᴛ1 ᏑFu(");
        Assert.AreEqual(0, errors.Length, string.Join("\n", errors));
    }

    [TestMethod]
    public void ABareLiftIsScopedByItsNameAndThePublicFieldOutranksIt()
    {
        // The red: what the converter emitted before it publicized the lift.
        (Dictionary<string, string> sources, string[] errors) = RunTypeGenerator("");

        StringAssert.Contains(GeneratedFor(sources, "t__WIDL_name_00000003ᴛ1"), "internal partial struct t__WIDL_name_00000003ᴛ1");
        Assert.IsTrue(errors.Any(error => error.Contains("CS0052") || error.Contains("CS0050")), "a bare lift under a public field must be an accessibility error");
    }
}

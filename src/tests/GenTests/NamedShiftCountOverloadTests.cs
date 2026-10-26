// NamedShiftCountOverloadTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

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
/// A shift on a named integer type (`type W uint32`) binds the wrapper's generated operator, which routes through
/// golib's GoShift guard. An unbounded count reaches it WIDENED, never narrowed to <c>int</c>, so the wrapper carries
/// <c>uint64</c> and <c>int64</c> count overloads beside the <c>int</c> one: the unsigned overload keeps the count's
/// full magnitude (<c>w &lt;&lt; (1 &lt;&lt; 32)</c> is 0) and the signed one binds golib's signed-count guard, which
/// raises Go's negative-shift panic. These tests pin the three counts on every integer width, routed to Lsh/Rsh, and
/// no shift operator on a float wrapper.
/// </summary>
[TestClass]
public class NamedShiftCountOverloadTests
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
                [GoType("num:uint32")] partial struct ShiftWord;
                [GoType("num:int64")] partial struct ShiftSigned;
                [GoType("num:uint8")] partial struct ShiftByte;
                [GoType("num:uintptr")] partial struct ShiftPtr;
                [GoType("num:float64")] partial struct ShiftFloat;
            }
        }
        """;

    private static readonly Regex ShiftOperator = new(@"operator (<<|>>)\((\w+) value, (\w+) shift\) => \(\w+\)value\.m_value\.(Lsh|Rsh)\(");

    private static Dictionary<string, string> RunTypeGenerator()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create("shift-count-test",
            [CSharpSyntaxTree.ParseText(Source, new CSharpParseOptions(LanguageVersion.Latest))],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"))
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TypeGenerator());
        driver = driver.RunGenerators(compilation);

        return driver.GetRunResult().Results
            .SelectMany(generator => generator.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());
    }

    // Every guarded shift operator the generator emitted for the wrapper, as "op count"
    private static HashSet<string> ShiftOperatorsOf(Dictionary<string, string> sources, string wrapper)
    {
        string source = string.Join(Environment.NewLine, sources.Values);

        return ShiftOperator.Matches(source)
            .Where(match => match.Groups[2].Value == wrapper)
            .Select(match => $"{match.Groups[1].Value} {match.Groups[3].Value}")
            .ToHashSet();
    }

    [TestMethod]
    public void IntegerWrapperShiftsTakeEveryCountWidth()
    {
        Dictionary<string, string> sources = RunTypeGenerator();

        foreach (string wrapper in new[] { "ShiftWord", "ShiftSigned", "ShiftByte", "ShiftPtr" })
        {
            HashSet<string> operators = ShiftOperatorsOf(sources, wrapper);

            foreach (string expected in new[] { "<< int", ">> int", "<< uint64", ">> uint64", "<< int64", ">> int64" })
                Assert.IsTrue(operators.Contains(expected), $"{wrapper} lacks the guarded `operator {expected}` count overload; it has: {string.Join(", ", operators)}");
        }
    }

    [TestMethod]
    public void FloatWrapperHasNoShift()
    {
        Dictionary<string, string> sources = RunTypeGenerator();

        Assert.AreEqual(0, ShiftOperatorsOf(sources, "ShiftFloat").Count, "a float wrapper carries a shift operator");
    }
}

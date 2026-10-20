// SelfBoxConversionRecordTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// A GoImplicitConv record whose operator would return a box from a value is a converter fault: Go has no
/// implicit conversion from a value to a pointer, so the record means the converter put a value where Go
/// passes a pointer. The operator such a record used to generate boxed a COPY (or a fresh value built from
/// the source's fields), so the site compiled while a write through the box never reached the original
/// (runtime's runqput stored a copy of the caller's g in runnext). The generator refuses the record by name
/// with GO2CS0008, an ERROR, and generates nothing for it; the record beside it still generates.
/// </summary>
/// <remarks>
/// The refusal is by DIRECTION, read after Inverted: the operator's result type is a box and its parameter
/// type is not. A self-to-box record, a value-to-OTHER-box record and an Inverted record whose source is the
/// box are all refused; a box-to-value record returns no box and is not.
/// </remarks>
[TestClass]
public class SelfBoxConversionRecordTests
{
    private const string BoxTypes =
        """
        namespace go
        {
            public class ж<T>
            {
                public ж(T value) { }
            }

            public static partial class box_package
            {
                public partial struct cell { public int A; }

                public partial struct other { public int A; }
            }
        }
        """;

    private static (GeneratorRunResult Run, List<Diagnostic> Refusals) Run(string record)
    {
        (GeneratorRunResult run, IReadOnlyList<Diagnostic> diagnostics) = GeneratorSkipRecordTests.Generate(new ImplicitConvGenerator(), $"using go;\n\n{record}\n\n{BoxTypes}");

        return (run, [.. diagnostics.Where(diagnostic => diagnostic.Id == "GO2CS0008")]);
    }

    private static void AssertRefused(string record)
    {
        (GeneratorRunResult run, List<Diagnostic> refusals) = Run(record);

        Assert.AreEqual(1, refusals.Count, $"one GO2CS0008 for {record}");
        Assert.AreEqual(DiagnosticSeverity.Error, refusals[0].Severity, "GO2CS0008 is an error");
        StringAssert.Contains(refusals[0].GetMessage(), "cell", "the refusal names the value type");
        Assert.IsFalse(run.GeneratedSources.Any(source => source.HintName.Contains("box_package")),
            $"no operator for {record}: {string.Join(", ", run.GeneratedSources.Select(source => source.HintName))}");
        GeneratorSkipRecordTests.AssertGenerated(run, "src");
    }

    [TestMethod]
    public void ASelfToBoxRecordIsRefused() =>
        AssertRefused("[assembly: GoImplicitConv<go.box_package.cell, go.ж<go.box_package.cell>>(Indirect = true)]");

    [TestMethod]
    public void AValueToOtherBoxRecordIsRefused() =>
        AssertRefused("[assembly: GoImplicitConv<go.box_package.cell, go.ж<go.box_package.other>>(Indirect = true)]");

    [TestMethod]
    public void AnInvertedRecordReturningTheBoxIsRefused() =>
        AssertRefused("[assembly: GoImplicitConv<go.ж<go.box_package.cell>, go.box_package.cell>(Inverted = true)]");

    [TestMethod]
    public void ABoxToValueRecordReturnsNoBoxAndIsNotRefused()
    {
        (_, List<Diagnostic> refusals) = Run("[assembly: GoImplicitConv<go.ж<go.box_package.cell>, go.box_package.cell>]");

        Assert.AreEqual(0, refusals.Count, $"a record whose operator returns the value is not GO2CS0008: {string.Join("; ", refusals)}");
    }
}

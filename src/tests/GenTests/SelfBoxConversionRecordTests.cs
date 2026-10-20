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
/// A GoImplicitConv record that pairs a type with its own box, <c>&lt;T, ж&lt;T&gt;&gt;</c>, is a
/// converter fault: Go has no implicit conversion between a value and a pointer, so the record means the
/// converter put a value where Go passes a pointer. The operator such a record used to generate boxed a
/// COPY of the value, so the site compiled while a write through the box never reached the original
/// (runtime's runqput stored a copy of the caller's g in runnext). The generator now refuses the record
/// by name with GO2CS0008, an ERROR, and generates nothing for it; the record beside it still generates.
/// </summary>
[TestClass]
public class SelfBoxConversionRecordTests
{
    private const string BoxFile =
        """
        using go;

        [assembly: GoImplicitConv<go.box_package.cell, go.ж<go.box_package.cell>>(Indirect = true)]
        [assembly: GoImplicitConv<go.ж<go.box_package.cell>, go.box_package.cell>]

        namespace go
        {
            public class ж<T>
            {
                public ж(T value) { }
            }

            public static partial class box_package
            {
                public partial struct cell { public int A; }
            }
        }
        """;

    [TestMethod]
    public void ARecordPairingATypeWithItsOwnBoxIsAnErrorAndGeneratesNothing()
    {
        (GeneratorRunResult run, IReadOnlyList<Diagnostic> diagnostics) = GeneratorSkipRecordTests.Generate(new ImplicitConvGenerator(), BoxFile);

        List<Diagnostic> refusals = [.. diagnostics.Where(diagnostic => diagnostic.Id == "GO2CS0008")];

        Assert.AreEqual(2, refusals.Count, $"one GO2CS0008 per self-to-box record, either order: {string.Join("; ", diagnostics)}");
        Assert.IsTrue(refusals.All(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), "GO2CS0008 is an error");
        Assert.IsTrue(refusals.All(diagnostic => diagnostic.GetMessage().Contains("cell")), $"the refusal names the type: {string.Join("; ", refusals)}");
        Assert.IsFalse(run.GeneratedSources.Any(source => source.HintName.Contains("cell")),
            $"no operator for a self-to-box record: {string.Join(", ", run.GeneratedSources.Select(source => source.HintName))}");
        GeneratorSkipRecordTests.AssertGenerated(run, "src");
    }
}

// TypeGeneratorSkipTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// A [GoType] on a declaration the TypeGenerator does not generate for must cost that type only. It
/// THREW (NotSupportedException), and a throwing generator contributes nothing: every other type in
/// the compilation lost its generated members with it. It is an ERROR, not a skip: a type missing its
/// generated members (constructors, operators, the forwarders golib reads its method set from) could
/// otherwise leave a green build that is wrong at run time.
/// </summary>
[TestClass]
public class TypeGeneratorSkipTests
{
    private const string UnsupportedGoTypeTarget =
        """
        using go;

        namespace go
        {
            public static partial class main_package
            {
                [GoType] public partial record struct rec(int A);
            }
        }
        """;

    [TestMethod]
    public void AGoTypeOnAnUnsupportedDeclarationIsAnErrorAtThatTypeOnly()
    {
        (GeneratorRunResult run, IReadOnlyList<Diagnostic> diagnostics) = GeneratorSkipRecordTests.Generate(new TypeGenerator(), UnsupportedGoTypeTarget);

        StringAssert.Contains(GeneratorSkipRecordTests.Single(diagnostics, "GO2CS0003", DiagnosticSeverity.Error).GetMessage(), "rec");
        GeneratorSkipRecordTests.AssertGenerated(run, "good");
    }
}

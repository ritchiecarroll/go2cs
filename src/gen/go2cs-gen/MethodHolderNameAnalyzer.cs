// MethodHolderNameAnalyzer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace go2cs;

/// <summary>
/// GO2CS0013: inside a class marked <c>[GoMethodHost]</c>, no name the package class declares binds another member,
/// and no name written through the file's alias of the package binds a local.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodHolderNameAnalyzer : DiagnosticAnalyzer
{
    // GO2CS0013, declared with every other go2cs-gen code in GeneratorDiagnostics.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [GeneratorDiagnostics.HolderNameBindsElsewhere];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
    }
}

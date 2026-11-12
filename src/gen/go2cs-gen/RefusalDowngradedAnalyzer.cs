// RefusalDowngradedAnalyzer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace go2cs;

/// <summary>
/// Reports each <c>[assembly: GoRefusalDowngraded(code, site, text)]</c> record as GO2CS0011, a warning: the converter
/// wrote one per refused site it converted under <c>-warn</c> (the converter's warnOption.go), and the site panics
/// with the refusal text when reached. The warning is raised in every build, so a project that is never converted
/// again still shows that it carries a known-broken path.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RefusalDowngradedAnalyzer : DiagnosticAnalyzer
{
    private const string RecordAttribute = "go.GoRefusalDowngradedAttribute";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [GeneratorDiagnostics.RefusalDowngraded];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(Analyze);
    }

    private static void Analyze(CompilationAnalysisContext context)
    {
        foreach (AttributeData attribute in context.Compilation.Assembly.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != RecordAttribute || attribute.ConstructorArguments.Length != 3)
                continue;

            Location location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? Location.None;

            context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.RefusalDowngraded, location,
                attribute.ConstructorArguments[0].Value, attribute.ConstructorArguments[1].Value, attribute.ConstructorArguments[2].Value));
        }
    }
}

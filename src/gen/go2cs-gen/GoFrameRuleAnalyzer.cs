// GoFrameRuleAnalyzer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace go2cs;

/// <summary>
/// GO2CS0016's second net: the uses of a <c>GoFrame</c> that <see cref="GoFrameRuleGenerator"/> found in an allowed
/// position but could not bind, bound here against the whole compilation.
/// </summary>
/// <remarks>
/// <para>
/// A source generator does not see the other generators' output, so a sound <c>defer(…, ref ᒐ)</c> whose lambda names a
/// generated member does not bind there, and neither does a copy that only binding shows: a <c>ref ᒐ</c> that infers a
/// generic argument, or the receiver of a by-value extension, in such a call. The generator lists each of those uses in a
/// comment-only generated file (<see cref="GoFrameRuleGenerator.DeferredHintPrefix"/>); this analyzer sees that file with
/// the rest of the generators' output, binds each listed use, and reports it with the same code where binding refuses it.
/// It reads ONLY the listed uses, so no use is reported twice: one the generator decided is never listed.
/// </para>
/// <para>
/// An analyzer is switched off by <c>RunAnalyzers=false</c>. A project that sets it keeps the generator's rule and loses
/// only this net, the uses that bind only with another generator's output.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GoFrameRuleAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [GeneratorDiagnostics.GoFrameEscapes];

    public override void Initialize(AnalysisContext context)
    {
        // A converted file keeps its Go file's name and leading comments, so it can be one Roslyn classes as generated code;
        // the action reads only the listed spans, so reading such files costs nothing elsewhere.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(start =>
        {
            INamedTypeSymbol? frame = start.Compilation.GetTypeByMetadataName(GoFrameRuleGenerator.GoFrameMetadataName);

            if (frame is null)
                return;

            Dictionary<string, List<TextSpan>> deferred = ReadDeferred(start.Compilation, start.CancellationToken);

            if (deferred.Count == 0)
                return;

            start.RegisterSemanticModelAction(analysis =>
            {
                SyntaxTree tree = analysis.SemanticModel.SyntaxTree;

                if (!deferred.TryGetValue(GoFrameRuleGenerator.DeferredKey(tree.FilePath), out List<TextSpan>? spans))
                    return;

                SyntaxNode root = tree.GetRoot(analysis.CancellationToken);

                foreach (TextSpan span in spans)
                {
                    if (span.End > root.FullSpan.End || root.FindNode(span, getInnermostNodeForTie: true) is not IdentifierNameSyntax identifier || identifier.Span != span)
                        continue;

                    GoFrameRuleGenerator.Bound bound = GoFrameRuleGenerator.Classify(analysis.SemanticModel, identifier, frame, analysis.CancellationToken);

                    if (bound is { Known: true, Use: { } use })
                    {
                        analysis.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.GoFrameEscapes, identifier.GetLocation(),
                            GoFrameRuleGenerator.Describe(identifier.Identifier.ValueText, use)));
                    }
                }
            });
        });
    }

    // The generator's lists, keyed by the source file each use is in (GoFrameRuleGenerator.DeferredKey; never a path).
    private static Dictionary<string, List<TextSpan>> ReadDeferred(Compilation compilation, System.Threading.CancellationToken cancellationToken)
    {
        Dictionary<string, List<TextSpan>> deferred = new(StringComparer.Ordinal);

        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            if (!Path.GetFileName(tree.FilePath).StartsWith(GoFrameRuleGenerator.DeferredHintPrefix, StringComparison.Ordinal))
                continue;

            foreach (TextLine line in tree.GetText(cancellationToken).Lines)
            {
                string text = line.ToString();

                if (!text.StartsWith(GoFrameRuleGenerator.DeferredLinePrefix, StringComparison.Ordinal))
                    continue;

                string[] fields = text.Substring(GoFrameRuleGenerator.DeferredLinePrefix.Length).Split([' '], 3);

                if (fields.Length != 3 ||
                    !int.TryParse(fields[0], NumberStyles.None, CultureInfo.InvariantCulture, out int start) ||
                    !int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out int length))
                {
                    continue;
                }

                if (!deferred.TryGetValue(fields[2], out List<TextSpan>? spans))
                    deferred[fields[2]] = spans = [];

                spans.Add(new TextSpan(start, length));
            }
        }

        return deferred;
    }
}

// StackAppendBufferAnalyzer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace go2cs;

/// <summary>
/// GO2CS0007: every call that binds golib's stack-append overload, <c>append(in stackAppend&lt;T&gt;, ...)</c>, passes
/// the buffer as a plain local of the calling method or lambda that no lambda captures.
/// </summary>
/// <remarks>
/// <para>
/// Go keeps a non-escaping append's backing store, and the flag that says it is taken, in the frame of the function
/// that appends (stackAppend.cs). The converter mirrors the frame with a local declared at the entry of that function,
/// <c>var xᴷ = new stackAppend&lt;T&gt;();</c>, and the overload sets the flag through the reference <c>in</c> passes.
/// That reference is the caller's own flag only when the argument IS such a local: a field, a new object, a copy
/// a lambda captured (<c>xᴷʗ1</c>), or a foreach variable would have the overload set a copy's flag, and a second
/// empty append in the same call would take the buffer again. The converter never hints across a function literal,
/// so it emits none of these; this analyzer makes that a build error rather than a property of today's emission.
/// </para>
/// <para>
/// It reads every call in the compilation, converted or hand-written: the soundness condition is the overload's,
/// whoever calls it.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StackAppendBufferAnalyzer : DiagnosticAnalyzer
{
    // GO2CS0007, declared with every other go2cs-gen code in GeneratorDiagnostics.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [GeneratorDiagnostics.StackAppendBufferNotALocal];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(start =>
        {
            INamedTypeSymbol? buffer = start.Compilation.GetTypeByMetadataName("go.stackAppend`1");

            if (buffer is null)
                return;

            start.RegisterOperationAction(operation => Analyze(operation, buffer), OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context, INamedTypeSymbol buffer)
    {
        IInvocationOperation invocation = (IInvocationOperation)context.Operation;

        foreach (IArgumentOperation argument in invocation.Arguments)
        {
            if (argument.Parameter is not { RefKind: RefKind.In } parameter || !SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, buffer))
                continue;

            if (Refusal(argument.Value) is { } refusal)
                context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.StackAppendBufferNotALocal, argument.Syntax.GetLocation(), argument.Value.Syntax.ToString(), refusal));
        }
    }

    // Why the argument is not the calling frame's own buffer local, or null when it is.
    private static string? Refusal(IOperation value)
    {
        if (value is not ILocalReferenceOperation { Local: { } local })
        {
            return value switch
            {
                IFieldReferenceOperation { Field.IsReadOnly: true } => "a readonly field",
                IFieldReferenceOperation => "a field",
                IParameterReferenceOperation => "a parameter",
                IObjectCreationOperation or IDefaultValueOperation => "a new value, not a local",
                _ => "not a local"
            };
        }

        if (local.RefKind != RefKind.None)
            return "a ref local";

        if (local.IsForEach)
            return "a foreach variable";

        if (local.IsUsing || local.IsConst || local.IsFixed)
            return "a read-only local";

        SemanticModel? model = value.SemanticModel;

        if (model is null)
            return "a local whose frame could not be read";

        ISymbol? caller = model.GetEnclosingSymbol(value.Syntax.SpanStart);

        if (!SymbolEqualityComparer.Default.Equals(local.ContainingSymbol, caller))
            return "a local of another method or lambda";

        // Fail closed: a body whose captures cannot be read is not proved uncaptured.
        if (CapturedInBody(model, caller) is not { } captured)
            return "a local whose captures could not be read";

        return captured.Contains(local, SymbolEqualityComparer.Default) ? "a local a lambda captures" : null;
    }

    // The locals a lambda or local function captures within the calling method's or lambda's own body.
    private static ImmutableArray<ISymbol>? CapturedInBody(SemanticModel model, ISymbol? caller)
    {
        SyntaxNode? declaration = caller?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

        SyntaxNode? body = declaration switch
        {
            BaseMethodDeclarationSyntax method => (SyntaxNode?)method.Body ?? method.ExpressionBody?.Expression,
            LocalFunctionStatementSyntax function => (SyntaxNode?)function.Body ?? function.ExpressionBody?.Expression,
            AnonymousFunctionExpressionSyntax lambda => lambda.Body,
            AccessorDeclarationSyntax accessor => (SyntaxNode?)accessor.Body ?? accessor.ExpressionBody?.Expression,
            _ => null
        };

        DataFlowAnalysis? flow = body switch
        {
            StatementSyntax statement => model.AnalyzeDataFlow(statement),
            ExpressionSyntax expression => model.AnalyzeDataFlow(expression),
            _ => null
        };

        return flow is { Succeeded: true } ? flow.Captured : null;
    }
}

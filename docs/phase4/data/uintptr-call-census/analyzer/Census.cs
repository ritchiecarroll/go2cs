// UintptrCallCensus: a census-only analyzer (R, queue item 4). It reports every cast to go.uintptr whose operand,
// parentheses stripped, is a call (method, delegate or local function) whose return type is unsafe.Pointer
// (go.unsafe_package.Pointer), with the type the cast's value is CONVERTED to, so a site is classed by its consumer:
//   BACK-TO-POINTER  the cast's value converts straight back to unsafe.Pointer: the box the call returned is dropped
//   POINTER-CAST     the cast is the operand of another cast (to a typed or native pointer)
//   NUMBER           the cast's value is used as a uintptr: a correct conversion to a number
// It reports as a warning (UCC001) whose message is tab-separated and machine-read; it changes nothing.
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UintptrCallCensus : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        "UCC001", "uintptr cast of an unsafe.Pointer call", "UCC\t{0}\t{1}\t{2}\t{3}", "Census",
        DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CastExpression);
    }

    private static bool IsUintptr(ITypeSymbol? t) =>
        t is { Name: "uintptr", ContainingType: null } && t.ContainingNamespace?.ToDisplayString() == "go";

    private static bool IsUnsafePointer(ITypeSymbol? t) =>
        t is { Name: "Pointer" } && t.ContainingType is { Name: "unsafe_package" } &&
        t.ContainingNamespace?.ToDisplayString() == "go";

    private static void Analyze(SyntaxNodeAnalysisContext ctx)
    {
        var cast = (CastExpressionSyntax)ctx.Node;
        var model = ctx.SemanticModel;

        if (!IsUintptr(model.GetTypeInfo(cast.Type, ctx.CancellationToken).Type))
            return;

        ExpressionSyntax operand = cast.Expression;
        while (operand is ParenthesizedExpressionSyntax paren)
            operand = paren.Expression;

        if (operand is not InvocationExpressionSyntax call)
            return;

        // The call's own result type, before any conversion the cast applies.
        ITypeSymbol? result = model.GetTypeInfo(call, ctx.CancellationToken).Type;
        if (!IsUnsafePointer(result))
            return;

        // Where the cast's value goes.
        SyntaxNode? up = cast.Parent;
        while (up is ParenthesizedExpressionSyntax)
            up = up.Parent;

        string consumer;
        ITypeSymbol? converted = model.GetTypeInfo(cast, ctx.CancellationToken).ConvertedType;
        if (up is CastExpressionSyntax outer)
            consumer = "POINTER-CAST:" + (model.GetTypeInfo(outer.Type, ctx.CancellationToken).Type?.ToDisplayString() ?? "?");
        else if (IsUnsafePointer(converted))
            consumer = "BACK-TO-POINTER";
        else
            consumer = "NUMBER:" + (converted?.ToDisplayString() ?? "?");

        string callee = model.GetSymbolInfo(call, ctx.CancellationToken).Symbol?.ToDisplayString() ?? call.Expression.ToString();
        string text = cast.ToString().Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
        if (text.Length > 160) text = text.Substring(0, 160);

        ctx.ReportDiagnostic(Diagnostic.Create(Rule, cast.GetLocation(), consumer, callee, text,
            cast.SyntaxTree.FilePath));
    }
}

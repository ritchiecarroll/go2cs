// StringLiteralBindingAnalyzer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace go2cs;

/// <summary>
/// Fails the build when a C# string literal in CONVERTED code binds a C# type Go's `string` is not.
/// </summary>
/// <remarks>
/// The converter emits Go's `"hello"` in a `string` slot as the bare C# literal (plain string literals,
/// owner ruling 2026-10-17), which reaches go.@string through its implicit conversion. Unlike a `u8`
/// span, a bare C# string ALSO converts to object, binds a C# type parameter and binds a System.String
/// overload, each without a compile error, and each delivers System.String where Go has a string. So
/// every such binding is an error here, unless the bound parameter is marked
/// <c>[GoTakesCSharpString]</c>: golib's declaration that it takes a C# string on purpose (panic
/// normalizes one to @string; len(string) counts its UTF-8 bytes).
/// <list type="bullet">
/// <item>A1, an object or interface target: the dynamic type would be System.String, which no Go type
/// switch, assertion or comparison matches.</item>
/// <item>A2, a type parameter inferred from the literal: `Echo("x")` makes T = System.String (and
/// `Pair("k", 1)` a map&lt;string, nint&gt;), which compiles and diverges.</item>
/// <item>A3, an unmarked System.String parameter where the method group also takes @string at that
/// position: overload resolution picked the C# overload over the Go one.</item>
/// </list>
/// It reads converted sources only (<see cref="Common.IsConvertedSourceFile"/>) of a converted package
/// (never golib, which has no analyzer reference, nor a hand-owned package). Attribute arguments, nameof
/// and parameter defaults (the [Caller*] ones) are not value slots and are not read.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StringLiteralBindingAnalyzer : DiagnosticAnalyzer
{
#pragma warning disable RS2008
    public static readonly DiagnosticDescriptor StringLiteralBindsWrongType = new(
        id: "GO2CS0004",
        title: "A C# string literal binds a type Go's string is not",
        messageFormat: "The C# string literal {0} binds {1}; converted code must pass a Go string (go.@string) here",
        category: "go2cs-gen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
#pragma warning restore RS2008

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [StringLiteralBindsWrongType];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(start =>
        {
            INamedTypeSymbol? goString = start.Compilation.GetTypeByMetadataName("go.string");

            if (goString is null || Common.IsHandOwnedAssembly(start.Compilation))
                return;

            INamedTypeSymbol? marker = start.Compilation.GetTypeByMetadataName("go.GoTakesCSharpStringAttribute");

            start.RegisterOperationAction(operation => Analyze(operation, goString, marker), OperationKind.Literal);
        });
    }

    private static void Analyze(OperationAnalysisContext context, INamedTypeSymbol goString, INamedTypeSymbol? marker)
    {
        ILiteralOperation literal = (ILiteralOperation)context.Operation;

        if (literal.Type?.SpecialType != SpecialType.System_String || literal.ConstantValue.Value is not string)
            return;

        SyntaxNode syntax = literal.Syntax;

        if (!Common.IsConvertedSourceFile(syntax.SyntaxTree) || syntax.Ancestors().Any(IsNotAValueSlot))
            return;

        IOperation? parent = literal.Parent;

        while (parent is IParenthesizedOperation)
            parent = parent.Parent;

        // A1: string -> object / interface is an implicit REFERENCE conversion (no operator method).
        if (parent is IConversionOperation { OperatorMethod: null } conversion &&
            conversion.Type is { } target &&
            (target.SpecialType == SpecialType.System_Object || target.TypeKind == TypeKind.Interface))
        {
            if (conversion.Parent is IArgumentOperation { Parameter: { } boxedParameter } && IsMarked(boxedParameter, marker))
                return;

            Report(context, literal, $"'{target.ToDisplayString()}' (its dynamic type would be System.String)");
            return;
        }

        if (parent is not IArgumentOperation { Parameter: { } parameter } argument || parameter.Type.SpecialType != SpecialType.System_String)
            return;

        // A2: the parameter's declared type is a METHOD type parameter, inferred here as System.String.
        if (parameter.OriginalDefinition.Type is ITypeParameterSymbol { TypeParameterKind: TypeParameterKind.Method } typeParameter)
        {
            Report(context, literal, $"type parameter '{typeParameter.Name}' of '{parameter.ContainingSymbol.OriginalDefinition.ToDisplayString()}', inferred as System.String");
            return;
        }

        // A3: an unmarked System.String parameter whose method group also takes @string at that position.
        if (IsMarked(parameter, marker) || argument.Parent is not IInvocationOperation invocation)
            return;

        IMethodSymbol method = invocation.TargetMethod;
        int ordinal = parameter.Ordinal;

        bool goOverloadExists = method.ContainingType.GetMembers(method.Name)
            .OfType<IMethodSymbol>()
            .Any(candidate => !SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, method.OriginalDefinition) &&
                              candidate.Parameters.Length > ordinal &&
                              SymbolEqualityComparer.Default.Equals(candidate.Parameters[ordinal].Type, goString));

        if (goOverloadExists)
            Report(context, literal, $"the System.String parameter '{parameter.Name}' of '{method.OriginalDefinition.ToDisplayString()}' where an @string overload exists");
    }

    private static bool IsNotAValueSlot(SyntaxNode node) =>
        node is AttributeSyntax or ParameterSyntax ||
        node is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } };

    private static bool IsMarked(IParameterSymbol parameter, INamedTypeSymbol? marker) =>
        marker is not null &&
        parameter.OriginalDefinition.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, marker));

    private static void Report(OperationAnalysisContext context, ILiteralOperation literal, string binding) =>
        context.ReportDiagnostic(Diagnostic.Create(StringLiteralBindsWrongType, literal.Syntax.GetLocation(), literal.Syntax.ToString(), binding));
}

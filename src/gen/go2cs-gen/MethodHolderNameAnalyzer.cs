// MethodHolderNameAnalyzer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace go2cs;

/// <summary>
/// GO2CS0013: inside a class marked <c>[GoMethodHost]</c>, no name the package class declares binds another member,
/// and no name written through the file's alias of the package binds a local.
/// </summary>
/// <remarks>
/// <para>
/// The methods a Go type declares are extension methods of a class of their own, a sibling of the package class,
/// marked <c>[GoMethodHost(typeof(T))]</c>. A Go method body reads its package's functions, constants, variables and
/// types by their simple names. In the package class those names are the class's own members and bind first. In the
/// holder they arrive through <c>using static</c> of the package class, and C# reads that import last:
/// </para>
/// <list type="bullet">
/// <item><description>a method of the holder binds first in an expression. A constant or a variable of that name is
/// then a compile error, and so is a function with another arity; a function whose parameter list fits compiles and
/// runs the METHOD;</description></item>
/// <item><description>a member of <c>go.builtin</c> arrives through a second <c>using static</c> and merges with the
/// package's: overload resolution chooses among both, with no diagnostic;</description></item>
/// <item><description>a type of the enclosing namespace binds before any import is read, so the package's type of
/// that name is never found, with no diagnostic;</description></item>
/// <item><description>the file's alias of its own package (<c>using time = time_package;</c>, written
/// <c>time.X</c>) is bound by a local or a parameter of that name, with no diagnostic when its type has a member
/// <c>X</c>.</description></item>
/// </list>
/// <para>
/// All four were measured with compiled cases (.NET SDK 10.0.403, C# 14, 2026-11-11; the record is
/// docs/phase4/SCOUT-method-holder.md). The converter therefore qualifies such a name by rule, never by reading a
/// compile error, and this analyzer fails the build where a name is not qualified. It reads hand-written and
/// generated holders too, which no rule over Go source can reach.
/// </para>
/// <para>
/// What it reports is narrow on purpose: a name is read only when the package class declares a member of that
/// name, so a holder method the package does not shadow, a builtin the package does not redeclare, and every type
/// the package does not also declare are never reported. The alias arm reads a member access only in a file that
/// declares the alias, and only when the package declares the selected member. Correct Go has that last shape
/// (<c>flag.Value</c> on a variable named <c>flag</c>, in package flag): the converter gives a file with such a
/// selector no alias, so the arm has no false positive on converted code.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodHolderNameAnalyzer : DiagnosticAnalyzer
{
    private const string HostAttributeName = "GoMethodHostAttribute";

    // GO2CS0013, declared with every other go2cs-gen code in GeneratorDiagnostics.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [GeneratorDiagnostics.HolderNameBindsElsewhere];

    public override void Initialize(AnalysisContext context)
    {
        // A generated part of a holder (a pointer-receiver twin) is a holder body like any other.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilation =>
        {
            // One package class has a holder per receiver type, so its names are read once for all of them.
            ConcurrentDictionary<INamedTypeSymbol, HashSet<string>> names = new(SymbolEqualityComparer.Default);
            ConcurrentDictionary<(SyntaxTree, INamedTypeSymbol), string?> aliases = new();

            compilation.RegisterSymbolStartAction(start =>
            {
                if (start.Symbol is not INamedTypeSymbol { IsStatic: true } holder || PackageClassOf(holder) is not { } package)
                    return;

                // GetMembers and not MemberNames, which leaves out the nested types of a source type.
                HashSet<string> declared = names.GetOrAdd(package, static package => [.. package.GetMembers().Select(member => member.Name)]);

                start.RegisterSyntaxNodeAction(node => Analyze(node, holder, package, declared, aliases), SyntaxKind.IdentifierName, SyntaxKind.GenericName);
            }, SymbolKind.NamedType);
        });
    }

    // The package class of the type a holder's [GoMethodHost(typeof(T))] names: the class T is nested in.
    private static INamedTypeSymbol? PackageClassOf(INamedTypeSymbol holder)
    {
        foreach (AttributeData attribute in holder.GetAttributes())
        {
            if (attribute.AttributeClass is { Name: HostAttributeName, ContainingNamespace: { Name: "go", ContainingNamespace.IsGlobalNamespace: true } } &&
                attribute.ConstructorArguments is [{ Value: INamedTypeSymbol receiver }])
            {
                return receiver.ContainingType?.OriginalDefinition;
            }
        }

        return null;
    }

    private static void Analyze(SyntaxNodeAnalysisContext context, INamedTypeSymbol holder, INamedTypeSymbol package, HashSet<string> declared, ConcurrentDictionary<(SyntaxTree, INamedTypeSymbol), string?> aliases)
    {
        SimpleNameSyntax name = (SimpleNameSyntax)context.Node;

        // Only a name C# looks up by itself: the right of a dot is found in what the left names.
        if (name.Parent is MemberAccessExpressionSyntax { Name: var selected } && selected == name ||
            name.Parent is QualifiedNameSyntax { Right: var right } && right == name ||
            name.Parent is MemberBindingExpressionSyntax or AliasQualifiedNameSyntax or NameColonSyntax or NameEqualsSyntax)
        {
            return;
        }

        string text = name.Identifier.ValueText;
        SemanticModel model = context.SemanticModel;

        // The alias arm: `time.X` where the file declares `time` as its alias of the package and the package
        // declares X, while `time` binds a local or a parameter.
        if (name.Parent is MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.SimpleMemberAccessExpression } access &&
            declared.Contains(access.Name.Identifier.ValueText) &&
            aliases.GetOrAdd((name.SyntaxTree, package), key => SelfAlias(key.Item1, model, key.Item2)) == text)
        {
            string? variable = model.GetSymbolInfo(name, context.CancellationToken).Symbol switch
            {
                ILocalSymbol local => $"the local '{local.Name}'",
                IParameterSymbol parameter => $"the parameter '{parameter.Name}'",
                _ => null
            };

            if (variable is not null)
            {
                Report(context, name, text, variable, holder, package, access.Name.Identifier.ValueText,
                    $"leave this file without the alias '{text}' when the variable's member is meant");

                return;
            }
        }

        if (!declared.Contains(text))
            return;

        SymbolInfo info = model.GetSymbolInfo(name, context.CancellationToken);
        ISymbol? symbol = info.Symbol ?? (info.CandidateReason == CandidateReason.MemberGroup ? info.CandidateSymbols.FirstOrDefault() : null);

        switch (symbol)
        {
            case IMethodSymbol method when SymbolEqualityComparer.Default.Equals(method.ContainingType?.OriginalDefinition, holder):
                Report(context, name, text, $"the method '{method.OriginalDefinition.ToDisplayString()}'", holder, package, text,
                    "call the method through its receiver");

                break;
            case { ContainingType: { Name: "builtin", ContainingType: null, ContainingNamespace: { Name: "go", ContainingNamespace.IsGlobalNamespace: true } } }:
                Report(context, name, text, $"the go.builtin member '{symbol.Name}'", holder, package, text,
                    $"'builtin.{text}' for the builtin");

                break;
            // The package class reads its member for this name only where C# would: a generic name reads a type or
            // a method of its own arity, and a name where only a type is read reads a type. `Func<bool>` is
            // System.Func<bool> in both when the package's Func is a constant or a type without type arguments.
            case INamedTypeSymbol { ContainingType: null } type when !SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, package) &&
                package.GetMembers(text).Any(member => ReadsInPackageClass(member, type.Arity, SyntaxFacts.IsInTypeOnlyContext(name))):
                Report(context, name, text, $"the type '{type.OriginalDefinition.ToDisplayString()}'", holder, package, text,
                    "the type with its namespace");

                break;
        }
    }

    // Whether a name of the given arity, in the package class, reads the package's member: a type of that
    // arity, a method of that arity where a type is not all that is read, or any other member where neither is.
    private static bool ReadsInPackageClass(ISymbol member, int arity, bool typeOnly) => member switch
    {
        INamedTypeSymbol declaredType => declaredType.Arity == arity,
        IMethodSymbol declaredMethod => !typeOnly && declaredMethod.Arity == arity,
        _ => !typeOnly && arity == 0
    };

    // The name under which a file's using directives alias the package class, or null when none does.
    private static string? SelfAlias(SyntaxTree tree, SemanticModel model, INamedTypeSymbol package)
    {
        foreach (UsingDirectiveSyntax directive in tree.GetRoot().DescendantNodes(node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax).OfType<UsingDirectiveSyntax>())
        {
            if (directive.Alias is not null && model.GetDeclaredSymbol(directive) is { Target: INamedTypeSymbol target } alias &&
                SymbolEqualityComparer.Default.Equals(target.OriginalDefinition, package))
            {
                return alias.Name;
            }
        }

        return null;
    }

    private static void Report(SyntaxNodeAnalysisContext context, SimpleNameSyntax name, string text, string bound, INamedTypeSymbol holder, INamedTypeSymbol package, string member, string otherwise)
    {
        context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.HolderNameBindsElsewhere, name.Identifier.GetLocation(),
            text, bound, holder.Name, package.Name, member, otherwise));
    }
}

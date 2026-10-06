// NoInliningPartialGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;

namespace go2cs;

/// <summary>
/// The shape the converter gives a method that must keep its own stack frame: a partial method's
/// IMPLEMENTING part, with its body, and no declaring part anywhere in the compilation.
/// </summary>
/// <remarks>
/// The converter marks a function no-inline when Go code reaches it through <c>runtime.Caller</c>, since
/// an inlined frame would change the answer. It used to spell that as a
/// <c>[MethodImpl(MethodImplOptions.NoInlining)]</c> prefix; it now writes the word <c>partial</c> there
/// instead (owner ruling 2026-10-06, docs/PLAN-marker-comment-parity.md section 10), and
/// <see cref="NoInliningPartialGenerator"/> writes the declaring part carrying the attribute, which C#
/// merges into the one compiled method. Hand-owned <c>*_impl.cs</c> files also hold partial methods with
/// bodies, but each implements a converted bodyless DECLARATION, so it always has a declaring part and
/// never matches.
/// </remarks>
public static class NoInliningPartials
{
    public const string Attribute =
        "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]";

    /// <summary>Syntax half: a <c>partial</c> method that has a body. Cheap; the receiver's filter.</summary>
    public static bool IsPartialWithBody(MethodDeclarationSyntax method) =>
        method.Modifiers.Any(SyntaxKind.PartialKeyword) && (method.Body is not null || method.ExpressionBody is not null);

    /// <summary>
    /// The full test: a partial method with a body whose symbol has no declaring part. A null model, or a
    /// method the model cannot bind, is not a carrier — never a widening by default.
    /// </summary>
    public static bool IsCarrier(MethodDeclarationSyntax method, SemanticModel? semanticModel) =>
        IsPartialWithBody(method) &&
        semanticModel?.GetDeclaredSymbol(method) is IMethodSymbol { IsPartialDefinition: false, PartialDefinitionPart: null };

    /// <summary>
    /// A module initializer (a Go init, <c>[GoInit]</c>) must never be carried: C# runs module
    /// initializers in declaration order, and a partial method's declaration is its declaring part, so a
    /// generated declaring part would run the init after every source file's (ruled 2026-10-06). The
    /// converter writes such a method with the attribute; one written by hand is refused.
    /// </summary>
    public static bool IsModuleInitializer(IMethodSymbol method) =>
        method.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.Runtime.CompilerServices.ModuleInitializerAttribute");
}

public sealed class NoInliningPartialFinder : ISyntaxReceiver
{
    public List<MethodDeclarationSyntax> Candidates { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is MethodDeclarationSyntax method && NoInliningPartials.IsPartialWithBody(method))
            Candidates.Add(method);
    }
}

/// <summary>
/// Writes the declaring part of every <see cref="NoInliningPartials.IsCarrier">no-inline carrier</see>:
/// the implementing part's signature copied exactly (modifiers, return type, name, type parameters,
/// parameters, constraints), its attributes (its parameters' and type parameters' too) and body removed, carrying
/// <c>[MethodImpl(MethodImplOptions.NoInlining)]</c>.
/// </summary>
/// <remarks>
/// The copy is TEXTUAL, so tuple element names, parameter names, <c>this</c>/<c>ref</c>/<c>params</c>,
/// constraints and <c>unsafe</c> match the implementing part byte for byte, which is what C# requires of
/// the two parts. The generated file repeats the source file's using directives and namespace, so every
/// type name resolves as it does in the source. A default parameter value would belong on this part
/// only; the converter emits none on these methods.
/// </remarks>
[Generator]
public class NoInliningPartialGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new NoInliningPartialFinder());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not NoInliningPartialFinder { Candidates.Count: > 0 } finder)
            return;

        HashSet<string> hintNames = new(System.StringComparer.OrdinalIgnoreCase);

        foreach (IGrouping<SyntaxTree, MethodDeclarationSyntax> file in finder.Candidates.GroupBy(method => method.SyntaxTree))
        {
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(file.Key);
            List<MethodDeclarationSyntax> carriers = [];

            foreach (MethodDeclarationSyntax method in file.Where(method => NoInliningPartials.IsCarrier(method, semanticModel)))
            {
                if (semanticModel.GetDeclaredSymbol(method) is IMethodSymbol symbol && NoInliningPartials.IsModuleInitializer(symbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.UngeneratableRecord, method.Identifier.GetLocation(),
                        $"The no-inline declaring part of module initializer '{method.Identifier.ValueText}'",
                        "a module initializer runs in declaration order, and a generated declaring part would run it after every source file; write [MethodImpl(MethodImplOptions.NoInlining)] on the method instead of `partial`"));

                    continue;
                }

                carriers.Add(method);
            }

            if (carriers.Count == 0)
                continue;

            CompilationUnitSyntax unit = (CompilationUnitSyntax)file.Key.GetRoot();
            StringBuilder source = new();
            source.Append("// <auto-generated/>\r\n");

            foreach (ExternAliasDirectiveSyntax alias in unit.Externs)
                source.Append(alias.WithoutTrivia().ToFullString()).Append("\r\n");

            // A `global using` is already in scope everywhere in the compilation; repeating one is CS1537 (an
            // alias) or a duplicate-using warning, so only the file's own directives are copied.
            foreach (UsingDirectiveSyntax usingDirective in unit.Usings.Where(directive => directive.GlobalKeyword.IsKind(SyntaxKind.None)))
                source.Append(usingDirective.WithoutTrivia().ToFullString()).Append("\r\n");

            // The carriers of one containing type share its namespace and type chain, written outermost
            // first exactly as the source declares it (a converted file's methods all sit in its package class).
            foreach (IGrouping<SyntaxNode?, MethodDeclarationSyntax> group in carriers.GroupBy(method => (SyntaxNode?)method.Parent))
            {
                List<SyntaxNode> chain = group.First().Ancestors().Where(node => node is BaseNamespaceDeclarationSyntax or TypeDeclarationSyntax).Reverse().ToList();
                int depth = 0;

                foreach (SyntaxNode node in chain)
                {
                    string indent = new(' ', depth * 4);

                    if (node is BaseNamespaceDeclarationSyntax ns)
                    {
                        source.Append($"{indent}namespace {ns.Name}\r\n{indent}{{\r\n");

                        foreach (UsingDirectiveSyntax usingDirective in ns.Usings)
                            source.Append(indent).Append("    ").Append(usingDirective.WithoutTrivia().ToFullString()).Append("\r\n");
                    }
                    else if (node is TypeDeclarationSyntax type)
                    {
                        source.Append($"{indent}partial {type.Keyword.ValueText} {type.Identifier.ValueText}{type.TypeParameterList?.WithoutTrivia().ToFullString()}\r\n{indent}{{\r\n");
                    }

                    depth++;
                }

                string memberIndent = new(' ', depth * 4);

                foreach (MethodDeclarationSyntax method in group)
                {
                    // No attribute is repeated: C# merges a partial method's parameter and type-parameter
                    // attributes across its parts, so a copied [GoArrayDims] on a parameter is CS0579.
                    MethodDeclarationSyntax declaring = method
                        .WithAttributeLists(default)
                        .WithParameterList(method.ParameterList.WithParameters(SyntaxFactory.SeparatedList(
                            method.ParameterList.Parameters.Select(parameter => parameter.WithAttributeLists(default)),
                            method.ParameterList.Parameters.GetSeparators())))
                        .WithTypeParameterList(method.TypeParameterList?.WithParameters(SyntaxFactory.SeparatedList(
                            method.TypeParameterList.Parameters.Select(typeParameter => typeParameter.WithAttributeLists(default)),
                            method.TypeParameterList.Parameters.GetSeparators())))
                        .WithBody(null)
                        .WithExpressionBody(null)
                        .WithSemicolonToken(default);

                    // The signature's last token (its `)` or last constraint) still carries the space that
                    // stood before the body.
                    SyntaxToken last = declaring.GetLastToken();

                    declaring = declaring
                        .ReplaceToken(last, last.WithTrailingTrivia())
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        .WithoutTrivia();

                    source.Append(memberIndent).Append(NoInliningPartials.Attribute).Append("\r\n");
                    source.Append(memberIndent).Append(declaring.ToFullString().TrimEnd()).Append("\r\n");
                }

                for (int level = depth - 1; level >= 0; level--)
                    source.Append(new string(' ', level * 4)).Append("}\r\n");
            }

            string hint = $"{System.IO.Path.GetFileNameWithoutExtension(file.Key.FilePath)}.noinline.g.cs";
            string unique = hint;

            for (int index = 1; !hintNames.Add(unique); index++)
                unique = $"{index}.{hint}";

            context.AddSource(GetValidFileName(unique), source.ToString());
        }
    }
}

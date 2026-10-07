// ChanDirSigGenerator.cs - Gbtc
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
/// The syntax half of <see cref="ChanDirSigGenerator"/>: reads the channel direction the converter writes
/// beside a <c>channel&lt;T&gt;</c> type as the <c>/*&lt;-*/</c> marker comment.
/// </summary>
/// <remarks>
/// The converter renders a Go channel type's direction in exactly two spellings (typeNameResolution.go):
/// RECEIVE-only <c>/*&lt;-*/channel&lt;T&gt;</c>, the marker immediately BEFORE the type, and SEND-only
/// <c>channel/*&lt;-*/&lt;T&gt;</c>, the marker between the identifier and its type-argument list. Roslyn
/// attaches a comment on the same line as trailing trivia of the token before it, so the receive marker is
/// read from the type's first token's leading trivia AND the preceding token's trailing trivia. Only a
/// position's OWN channel is read; a direction nested inside it is not carried.
/// </remarks>
public static class ChanDirMarkers
{
    public const string Marker = "/*<-*/";

    public const byte Unstamped = 0, Recv = 1, Send = 2;

    public static byte DirOf(TypeSyntax? type)
    {
        GenericNameSyntax? channel = type switch
        {
            GenericNameSyntax { Identifier.Text: "channel" } generic => generic,
            QualifiedNameSyntax { Right: GenericNameSyntax { Identifier.Text: "channel" } right } => right,
            AliasQualifiedNameSyntax { Name: GenericNameSyntax { Identifier.Text: "channel" } aliased } => aliased,
            _ => null
        };

        if (channel is null)
            return Unstamped;

        if (HasMarker(channel.Identifier.TrailingTrivia) || HasMarker(channel.TypeArgumentList.LessThanToken.LeadingTrivia))
            return Send;

        // The receive marker precedes the WHOLE type, so read it before the type's first token: for a
        // qualified `go./*<-*/channel<T>` that is the `channel` identifier itself.
        SyntaxToken first = type!.GetFirstToken();

        if (HasMarker(first.LeadingTrivia) || HasMarker(first.GetPreviousToken().TrailingTrivia) ||
            HasMarker(channel.Identifier.LeadingTrivia) || HasMarker(channel.Identifier.GetPreviousToken().TrailingTrivia))
        {
            return Recv;
        }

        return Unstamped;
    }

    public static byte[] ParameterDirs(MethodDeclarationSyntax method) =>
        method.ParameterList.Parameters.Select(parameter => DirOf(parameter.Type)).ToArray();

    public static byte[] ResultDirs(MethodDeclarationSyntax method) => method.ReturnType switch
    {
        PredefinedTypeSyntax { Keyword.Text: "void" } => [],
        TupleTypeSyntax tuple => tuple.Elements.Select(element => DirOf(element.Type)).ToArray(),
        TypeSyntax single => [DirOf(single)]
    };

    public static bool HasAny(MethodDeclarationSyntax method) =>
        ParameterDirs(method).Any(dir => dir != Unstamped) || ResultDirs(method).Any(dir => dir != Unstamped);

    private static bool HasMarker(SyntaxTriviaList trivia) =>
        trivia.Any(item => item.IsKind(SyntaxKind.MultiLineCommentTrivia) && item.ToString() == Marker);
}

public sealed class ChanDirMarkerFinder : ISyntaxReceiver
{
    public List<MethodDeclarationSyntax> Candidates { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is MethodDeclarationSyntax method && ChanDirMarkers.HasAny(method))
            Candidates.Add(method);
    }
}

/// <summary>
/// Emits, for every method whose signature carries a directional channel, a <c>[GoSigChanDir]</c> on a
/// generated partial of the method's declaring type, so reflection can recover the direction the C#
/// signature drops (NEW-1b, COORD ruling 2026-10-05: the generator-side route, zero change to converted
/// code). golib's <c>GoReflect.MethodSigChanDirs</c> reads it back by method name and parameter types.
/// </summary>
/// <remarks>
/// Not carried, and recorded as such: a func LITERAL (a lambda or local function compiles to a
/// compiler-named method that source cannot key), a generic method or one whose signature mentions a
/// type parameter (an attribute argument cannot name one), a method of a type with an enclosing type that
/// is not partial everywhere, and a signature with an unmanaged pointer (typeof of a pointer needs an
/// unsafe context). A NESTED type is carried by repeating its enclosing partials around the generated one:
/// every converted Go interface is nested in its package class. A <c>[GoRecv]</c> method with a
/// <c>ref</c> receiver gets a second entry keyed on <c>ж&lt;T&gt;</c>, the receiver of the overload
/// RecvGenerator adds for it, since a generator cannot see another generator's output.
/// </remarks>
[Generator]
public class ChanDirSigGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ChanDirMarkerFinder());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not ChanDirMarkerFinder { Candidates.Count: > 0 } finder)
            return;

        Dictionary<INamedTypeSymbol, (string ns, List<string> entries)> byType = new(SymbolEqualityComparer.Default);

        foreach (MethodDeclarationSyntax methodSyntax in finder.Candidates)
        {
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(methodSyntax) is not IMethodSymbol symbol ||
                symbol.ContainingType is not { } declaringType ||
                symbol.IsGenericMethod ||
                symbol.Parameters.Any(parameter => !IsNameable(parameter.Type)) ||
                !EnclosingChain(declaringType).All(IsPartialEverywhere))
            {
                continue;
            }

            string ns = methodSyntax.GetNamespaceName();

            if (ns.Length == 0)
                continue;

            byte[] parameterDirs = ChanDirMarkers.ParameterDirs(methodSyntax);
            byte[] resultDirs = ChanDirMarkers.ResultDirs(methodSyntax);
            string[] parameterTypes = symbol.Parameters.Select(parameter => TypeOf(parameter.Type)).ToArray();

            if (!byType.TryGetValue(declaringType, out (string ns, List<string> entries) slot))
            {
                slot = (ns, []);
                byType[declaringType] = slot;
            }

            slot.entries.Add(Entry(symbol.Name, parameterTypes, parameterDirs, resultDirs));

            bool isGoRecv = symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name is "GoRecvAttribute" or "GoRecv");

            if (isGoRecv && symbol.Parameters.Length > 0 && symbol.Parameters[0].RefKind == RefKind.Ref)
            {
                string[] boxed = (string[])parameterTypes.Clone();
                boxed[0] = $"typeof(global::go.ж<{symbol.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>)";
                slot.entries.Add(Entry(symbol.Name, boxed, parameterDirs, resultDirs));
            }
        }

        HashSet<string> hintNames = new(System.StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<INamedTypeSymbol, (string ns, List<string> entries)> pair in byType)
        {
            INamedTypeSymbol type = pair.Key;
            List<INamedTypeSymbol> chain = EnclosingChain(type).Reverse().ToList();

            StringBuilder source = new();
            source.Append("// <auto-generated/>\r\n#nullable enable\r\n\r\n");
            source.Append($"namespace {pair.Value.ns};\r\n\r\n");

            // Every enclosing type opens around the decorated one, outermost first; the entries sit on
            // the innermost declaration, which is the method's declaring type.
            for (int depth = 0; depth < chain.Count; depth++)
            {
                string indent = new(' ', depth * 4);

                if (depth == chain.Count - 1)
                {
                    foreach (string entry in pair.Value.entries.Distinct())
                        source.Append(indent).Append(entry).Append("\r\n");
                }

                source.Append($"{indent}partial {Keyword(chain[depth])} {EscapeIdentifier(chain[depth].Name)}{TypeParameters(chain[depth])}\r\n{indent}{{\r\n");
            }

            for (int depth = chain.Count - 1; depth >= 0; depth--)
                source.Append(new string(' ', depth * 4)).Append("}\r\n");

            string hint = $"{pair.Value.ns}.{string.Join(".", chain.Select(item => item.MetadataName))}.chandir.g.cs";
            string unique = hint;

            for (int index = 1; !hintNames.Add(unique); index++)
                unique = $"{index}.{hint}";

            context.AddSource(GetValidFileName(unique), source.ToString());
        }
    }

    private static string Entry(string name, string[] parameterTypes, byte[] parameterDirs, byte[] resultDirs) =>
        $"[global::go.GoSigChanDir(\"{name}\", new global::System.Type[] {{ {string.Join(", ", parameterTypes)} }}, " +
        $"new global::go.GoChanDir[] {{ {string.Join(", ", parameterDirs.Select(DirName))} }}, " +
        $"new global::go.GoChanDir[] {{ {string.Join(", ", resultDirs.Select(DirName))} }})]";

    private static string DirName(byte dir) => dir switch
    {
        ChanDirMarkers.Recv => "global::go.GoChanDir.Recv",
        ChanDirMarkers.Send => "global::go.GoChanDir.Send",
        _ => "global::go.GoChanDir.Unstamped"
    };

    // The declaring type and every type enclosing it, innermost first.
    private static IEnumerable<INamedTypeSymbol> EnclosingChain(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
            yield return current;
    }

    private static string Keyword(INamedTypeSymbol type) => type.TypeKind switch
    {
        TypeKind.Interface => "interface",
        TypeKind.Struct => "struct",
        _ => "class"
    };

    private static string TypeParameters(INamedTypeSymbol type) =>
        type.TypeParameters.Length == 0 ? "" : $"<{string.Join(", ", type.TypeParameters.Select(parameter => parameter.Name))}>";

    private static string TypeOf(ITypeSymbol type) => $"typeof({type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})";

    // A type an attribute argument can name: no type parameter anywhere in it, and no unmanaged pointer.
    private static bool IsNameable(ITypeSymbol type) => type switch
    {
        ITypeParameterSymbol => false,
        IPointerTypeSymbol or IFunctionPointerTypeSymbol => false,
        IArrayTypeSymbol array => IsNameable(array.ElementType),
        INamedTypeSymbol named => named.TypeArguments.All(IsNameable) && (named.ContainingType is null || IsNameable(named.ContainingType)),
        _ => true
    };

    // A generated `partial` part is legal only when every declaration of the type is partial (CS0260).
    private static bool IsPartialEverywhere(INamedTypeSymbol type) =>
        type.DeclaringSyntaxReferences.Length > 0 &&
        type.DeclaringSyntaxReferences.All(reference =>
            reference.GetSyntax() is TypeDeclarationSyntax declaration &&
            declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)));

    private static string EscapeIdentifier(string name) =>
        SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;
}

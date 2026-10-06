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
/// every converted Go interface is nested in its package class. A pointer-receiver method (an unmarked
/// <c>this ref</c>, or <c>[GoRecv]</c>) gets a second entry keyed on <c>ж&lt;T&gt;</c>, the receiver of the overload
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
                symbol.Parameters.Any(parameter => !GeneratedPartials.IsNameable(parameter.Type)) ||
                !GeneratedPartials.CanReopen(declaringType))
            {
                continue;
            }

            string ns = methodSyntax.GetNamespaceName();

            if (ns.Length == 0)
                continue;

            byte[] parameterDirs = ChanDirMarkers.ParameterDirs(methodSyntax);
            byte[] resultDirs = ChanDirMarkers.ResultDirs(methodSyntax);
            string[] parameterTypes = symbol.Parameters.Select(parameter => GeneratedPartials.TypeOf(parameter.Type)).ToArray();

            if (!byType.TryGetValue(declaringType, out (string ns, List<string> entries) slot))
            {
                slot = (ns, []);
                byType[declaringType] = slot;
            }

            slot.entries.Add(Entry(symbol.Name, parameterTypes, parameterDirs, resultDirs));

            // A pointer receiver by RecvGenerator's own rule (an unmarked `this ref`, or [GoRecv]), so the
            // entry follows exactly the methods that get the ж<T> overload.
            if (symbol.IsPointerSetMethod() && symbol.Parameters.Length > 0 && symbol.Parameters[0].RefKind == RefKind.Ref)
            {
                string[] boxed = (string[])parameterTypes.Clone();
                boxed[0] = $"typeof(global::go.ж<{symbol.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>)";
                slot.entries.Add(Entry(symbol.Name, boxed, parameterDirs, resultDirs));
            }
        }

        HashSet<string> hintNames = new(System.StringComparer.OrdinalIgnoreCase);

        // The entries sit on the method's declaring type, reopened inside each enclosing partial.
        foreach (KeyValuePair<INamedTypeSymbol, (string ns, List<string> entries)> pair in byType)
            context.AddSource(GeneratedPartials.HintName(hintNames, pair.Value.ns, pair.Key, "chandir"), GeneratedPartials.Source(pair.Value.ns, pair.Key, pair.Value.entries));
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
}

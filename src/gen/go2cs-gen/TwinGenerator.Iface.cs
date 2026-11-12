// TwinGenerator.Iface.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

// M:N SPIKE (R5b, B2). Spike code: it stays on the spike branch and never lands.
// Interface twins, in the two shapes the public-surface plan allows. Hidden: a separate interface beside the Go one
// holds the twin members and a static call helper, and the Go-named interface gains nothing. Default: the Go-named
// interface gains a default-bodied twin member. Either way an implementer that has no twin runs its own member.
public sealed partial class TwinGenerator
{
    internal enum IfaceShape { None, Hidden, Default }

    internal sealed class IfaceMember
    {
        public IMethodSymbol Symbol = null!;
        public string? Refusal;
        public bool Parks;
    }

    // An explicit interface implementation a sibling generator wrote: it forwards one interface member to a method.
    internal sealed class Forwarder
    {
        public IMethodSymbol Symbol = null!;
        public IMethodSymbol Member = null!;
        public IMethodSymbol? Target;
        public string Receiver = "";
        public string? Skip;
    }

    internal sealed partial class Run
    {
        private const string Hidden = "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]";
        private const string Task = "global::go.golib.GoTask";
        private static readonly SymbolDisplayFormat s_full = SymbolDisplayFormat.FullyQualifiedFormat;

        private readonly Dictionary<ISymbol, IfaceMember> m_ifaceMembers = new(SymbolEqualityComparer.Default);
        private readonly List<IfaceMember> m_ifaceOrdered = [];
        private readonly List<Forwarder> m_forwarders = [];
        private readonly Dictionary<ISymbol, bool> m_ifaceExternal = new(SymbolEqualityComparer.Default);
        private int m_ifaceAwaits, m_ifaceForwarded;

        internal bool IfaceHidden => ifaceShape == IfaceShape.Hidden;

        private void CollectIface()
        {
            if (ifaceShape == IfaceShape.None)
                return;

            HashSet<SyntaxTree> own = [];

            foreach (SyntaxTree tree in Context.Compilation.SyntaxTrees)
            {
                own.Add(tree);

                foreach (InterfaceDeclarationSyntax declaration in tree.GetRoot(Context.CancellationToken).DescendantNodes().OfType<InterfaceDeclarationSyntax>())
                {
                    if (Model(tree).GetDeclaredSymbol(declaration, Context.CancellationToken) is not { } type)
                        continue;

                    foreach (MethodDeclarationSyntax method in declaration.Members.OfType<MethodDeclarationSyntax>())
                    {
                        if (method.Body is not null || method.ExpressionBody is not null ||
                            Model(tree).GetDeclaredSymbol(method, Context.CancellationToken) is not { } symbol || m_ifaceMembers.ContainsKey(symbol))
                            continue;

                        IfaceMember member = new() { Symbol = symbol, Refusal = IfaceRefusal(type, symbol) };
                        member.Parks = ifaceAll && member.Refusal is null;
                        m_ifaceMembers[symbol] = member;
                        m_ifaceOrdered.Add(member);
                    }
                }
            }

            foreach (SyntaxTree tree in m_augmented.SyntaxTrees)
            {
                if (own.Contains(tree))
                    continue;

                foreach (MethodDeclarationSyntax method in tree.GetRoot(Context.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    if (method.ExplicitInterfaceSpecifier is null || method.ExpressionBody?.Expression is not InvocationExpressionSyntax call ||
                        Model(tree).GetDeclaredSymbol(method, Context.CancellationToken) is not { ExplicitInterfaceImplementations.Length: 1 } symbol)
                        continue;

                    Forwarder forwarder = new() { Symbol = symbol, Member = symbol.ExplicitInterfaceImplementations[0].OriginalDefinition };
                    forwarder.Skip = ForwarderSkip(forwarder, method, call, Model(tree));
                    m_forwarders.Add(forwarder);
                }
            }
        }

        private static string? IfaceRefusal(INamedTypeSymbol type, IMethodSymbol symbol)
        {
            for (INamedTypeSymbol? outer = type; outer is not null; outer = outer.ContainingType)
            {
                if (outer.TypeParameters.Length > 0)
                    return "generic-interface";
            }

            if (symbol.IsStatic)
                return "static";

            if (symbol.IsGenericMethod)
                return "generic-method";

            if (symbol.ReturnsByRef || symbol.ReturnsByRefReadonly)
                return "ref-return";

            if (symbol.Parameters.Any(parameter => parameter.RefKind != RefKind.None || parameter.IsParams))
                return "byref-param";

            if (symbol.ReturnType.IsRefLikeType || symbol.Parameters.Any(parameter => parameter.Type.IsRefLikeType))
                return "reflike";

            if (symbol.ReturnType.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer ||
                symbol.Parameters.Any(parameter => parameter.Type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer))
                return "unsafe";

            return null;
        }

        private static string? ForwarderSkip(Forwarder forwarder, MethodDeclarationSyntax method, InvocationExpressionSyntax call, SemanticModel model)
        {
            if (method.Parent is not TypeDeclarationSyntax host)
                return "host";

            if (!host.Modifiers.Any(SyntaxKind.PartialKeyword))
                return "host-not-partial";

            if (host.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().Any(type => type.TypeParameterList is not null))
                return "generic-host";

            if (call.Expression is not MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.SimpleMemberAccessExpression, Name: IdentifierNameSyntax } access ||
                access.Expression is not (ThisExpressionSyntax or IdentifierNameSyntax))
                return "shape";

            SeparatedSyntaxList<ArgumentSyntax> arguments = call.ArgumentList.Arguments;

            if (arguments.Count != forwarder.Symbol.Parameters.Length)
                return "shape";

            for (int i = 0; i < arguments.Count; i++)
            {
                if (!arguments[i].RefKindKeyword.IsKind(SyntaxKind.None) || arguments[i].Expression is not IdentifierNameSyntax name ||
                    name.Identifier.ValueText != forwarder.Symbol.Parameters[i].Name)
                    return "shape";
            }

            if (model.GetSymbolInfo(call).Symbol is not IMethodSymbol target)
                return "target-unbound";

            IMethodSymbol definition = Normalize(target);

            if (!definition.IsStatic || !definition.IsExtensionMethod || definition.IsGenericMethod || definition.ContainingType.IsGenericType)
                return "target-shape";

            if (!SymbolEqualityComparer.Default.Equals(target.ReturnType, forwarder.Symbol.ReturnType))
                return "return-type";

            forwarder.Target = definition;
            forwarder.Receiver = access.Expression.ToString();
            return null;
        }

        // An interface member of this compilation has a twin once one of its implementers here can park (or always,
        // under the "all" policy); one of a referenced assembly has a twin when that assembly was built with it.
        internal bool IfaceCanPark(IMethodSymbol? member)
        {
            if (member is null || ifaceShape == IfaceShape.None)
                return false;

            if (m_ifaceMembers.TryGetValue(member, out IfaceMember? own))
                return own.Parks;

            if (m_ifaceExternal.TryGetValue(member, out bool has))
                return has;

            INamedTypeSymbol type = member.ContainingType;

            if (SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, m_augmented.Assembly))
                has = false;
            else if (IfaceHidden)
                has = (type.ContainingType?.GetTypeMembers(type.Name + Suffix) ?? type.ContainingNamespace.GetTypeMembers(type.Name + Suffix))
                    .Any(twin => twin.GetMembers(member.Name + "ꞏcall").Any());
            else
                has = type.GetMembers(member.Name + Suffix).OfType<IMethodSymbol>().Any(twin => twin.Parameters.Length == member.Parameters.Length);

            return m_ifaceExternal[member] = has;
        }

        private bool ComputeIfaceParks()
        {
            bool changed = false;

            foreach (Forwarder forwarder in m_forwarders)
            {
                if (forwarder.Skip is not null || !m_ifaceMembers.TryGetValue(forwarder.Member, out IfaceMember? member) ||
                    member.Parks || member.Refusal is not null || !CanPark(forwarder.Target))
                    continue;

                member.Parks = true;
                changed = true;
            }

            return changed;
        }

        private static string Escaped(string name) =>
            SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None ? "@" + name : name;

        private static string Result(IMethodSymbol method) =>
            method.ReturnsVoid ? Task : Task + "<" + method.ReturnType.ToDisplayString(s_full) + ">";

        private static string Parameters(IMethodSymbol method) =>
            string.Join(", ", method.Parameters.Select(parameter => parameter.Type.ToDisplayString(s_full) + " " + Escaped(parameter.Name)));

        private static string Arguments(IMethodSymbol method) =>
            string.Join(", ", method.Parameters.Select(parameter => Escaped(parameter.Name)));

        // recv.M(a, b) becomes Iꞏtwin.Mꞏcall(recv, a, b) under the hidden shape.
        internal InvocationExpressionSyntax IfaceCall(IMethodSymbol member, InvocationExpressionSyntax visited)
        {
            MemberAccessExpressionSyntax access = (MemberAccessExpressionSyntax)visited.Expression;
            SeparatedSyntaxList<ArgumentSyntax> arguments = visited.ArgumentList.Arguments;
            List<SyntaxNodeOrToken> list = [SyntaxFactory.Argument(access.Expression.WithoutTrivia())];

            for (int i = 0; i < arguments.Count; i++)
            {
                list.Add(i == 0 ? SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space) : arguments.GetSeparator(i - 1));
                list.Add(arguments[i]);
            }

            ExpressionSyntax helper = SyntaxFactory.ParseExpression(member.ContainingType.ToDisplayString(s_full) + Suffix + "." + member.Name + "ꞏcall");

            return SyntaxFactory.InvocationExpression(helper, visited.ArgumentList.WithArguments(SyntaxFactory.SeparatedList<ArgumentSyntax>(list))).WithoutTrivia();
        }
    }
}

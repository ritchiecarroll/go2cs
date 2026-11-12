// TwinGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace go2cs;

/// <summary>
/// M:N SPIKE (R5b, B2). Spike code: it stays on the spike branch and never lands.
/// Writes a hidden async twin beside each converted function that can park: the function's own text, with each call
/// that has a twin awaited. It runs only for a project built with <c>GoTwins=true</c>.
/// </summary>
[Generator]
public sealed partial class TwinGenerator : ISourceGenerator
{
    internal const string Suffix = "ꞏtwin";

    internal enum GoKind { None, Literal, Group, Other }

    internal sealed class Site
    {
        public IMethodSymbol? Target;
        public IMethodSymbol? GoTarget;
        public GoKind Go;
        public bool Active, Direct, Renameable, Unbound, Delegate, Interface, IfaceCallable;
        public IMethodSymbol? IfaceTarget;
    }

    internal sealed class Candidate
    {
        public MethodDeclarationSyntax Syntax = null!;
        public IMethodSymbol Symbol = null!;
        public string? Refusal;
        public bool Parks, GoOnly;
        public int Awaits;
        public readonly List<Site> Sites = [];
        public bool Twinned => Refusal is null && Parks;
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context) =>
        Execute(context, typeof(TwinGenerator));

    // GoTwinsSiblings=last (B4) hands the work to ZTwinGenerator, which the compiler runs after every sibling.
    internal static void Execute(GeneratorExecutionContext context, Type self)
    {
        AnalyzerConfigOptions options = context.AnalyzerConfigOptions.GlobalOptions;

        if (!options.TryGetValue("build_property.GoTwins", out string? on) || !string.Equals(on, "true", StringComparison.OrdinalIgnoreCase))
            return;

        if (SiblingMemo.Last(options) != (self == typeof(ZTwinGenerator)))
            return;

        bool goSites = !(options.TryGetValue("build_property.GoTwinsGo", out string? go) && string.Equals(go, "false", StringComparison.OrdinalIgnoreCase));

        // GoTwinsIface picks the interface twin shape (hidden or default; none without it); GoTwinsIfaceAll twins
        // every interface member, not only the ones an implementer of this assembly can park in.
        IfaceShape ifaceShape = !options.TryGetValue("build_property.GoTwinsIface", out string? shape) ? IfaceShape.None :
            string.Equals(shape, "hidden", StringComparison.OrdinalIgnoreCase) ? IfaceShape.Hidden :
            string.Equals(shape, "default", StringComparison.OrdinalIgnoreCase) ? IfaceShape.Default : IfaceShape.None;

        bool ifaceAll = options.TryGetValue("build_property.GoTwinsIfaceAll", out string? all) && string.Equals(all, "true", StringComparison.OrdinalIgnoreCase);

        new Run(context, goSites, ifaceShape, ifaceAll) { Self = self }.Execute();
    }

    internal sealed partial class Run(GeneratorExecutionContext context, bool goSites, IfaceShape ifaceShape, bool ifaceAll)
    {
        internal readonly GeneratorExecutionContext Context = context;
        internal readonly Dictionary<InvocationExpressionSyntax, Site> Sites = [];
        private readonly Dictionary<ISymbol, Candidate> m_candidates = new(SymbolEqualityComparer.Default);
        private readonly List<Candidate> m_ordered = [];
        private readonly Dictionary<ISymbol, bool> m_hasTwin = new(SymbolEqualityComparer.Default);
        private readonly Dictionary<SyntaxTree, SemanticModel> m_models = [];
        private readonly List<(string path, int line, string id)> m_refuse = [];
        private Compilation m_augmented = null!;

        internal SemanticModel Model(SyntaxTree tree)
        {
            if (!m_models.TryGetValue(tree, out SemanticModel? model))
                m_models[tree] = model = m_augmented.GetSemanticModel(tree);

            return model;
        }

        internal void Execute()
        {
            // A call to a member a sibling generator adds (a receiver overload, an interface implementation) must
            // bind, and a generator never sees another generator's output. Two ways to get it (B4): the siblings'
            // own output of this run, kept by SiblingMemo (GoTwinsSiblings=memo), or a second run of them in a
            // nested driver (the form B2 and B3 measured, and the fallback when the memo holds nothing).
            m_watch = System.Diagnostics.Stopwatch.StartNew();

            if (!AddSiblingsFromMemo())
                AddSiblingsNested();

            m_siblingMs = m_watch.ElapsedMilliseconds;

            ReadRefusals();

            foreach (SyntaxTree tree in Context.Compilation.SyntaxTrees)
            {
                foreach (MethodDeclarationSyntax method in tree.GetRoot(Context.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>())
                    AddCandidate(method);
            }

            CollectIface();

            foreach (Candidate candidate in m_ordered)
                new Collector(this, Model(candidate.Syntax.SyntaxTree), candidate).Visit((SyntaxNode?)candidate.Syntax.Body ?? candidate.Syntax.ExpressionBody);

            for (bool changed = true; changed;)
            {
                changed = false;

                foreach (Candidate candidate in m_ordered)
                {
                    if (candidate.Parks || !ComputeParks(candidate))
                        continue;

                    candidate.Parks = true;
                    changed = true;
                }

                changed |= ComputeIfaceParks();
            }

            foreach (Candidate candidate in m_ordered)
                candidate.GoOnly = candidate.Parks && !candidate.Sites.Any(site => site.Direct && ShouldAwait(site));

            m_analysisMs = m_watch.ElapsedMilliseconds - m_siblingMs;
            Emit();
        }

        private void ReadRefusals()
        {
            AdditionalText? file = Context.AdditionalFiles.FirstOrDefault(text => string.Equals(Path.GetFileName(text.Path), "twin-refuse.txt", StringComparison.OrdinalIgnoreCase));

            if (file?.GetText(Context.CancellationToken) is not { } lines)
                return;

            foreach (string line in lines.ToString().Split('\n'))
            {
                string[] parts = line.Trim().Split('|');

                if (parts.Length == 3 && int.TryParse(parts[1], out int number))
                    m_refuse.Add((parts[0], number, parts[2]));
            }
        }

        private void AddCandidate(MethodDeclarationSyntax method)
        {
            if (method.Body is null && method.ExpressionBody is null || method.Parent is not TypeDeclarationSyntax ||
                method.Modifiers.Any(SyntaxKind.AsyncKeyword) || method.Identifier.Text.EndsWith(Suffix, StringComparison.Ordinal))
                return;

            if (Model(method.SyntaxTree).GetDeclaredSymbol(method, Context.CancellationToken) is not { } declared)
                return;

            IMethodSymbol symbol = Normalize(declared);

            if (m_candidates.ContainsKey(symbol))
                return;

            Candidate candidate = new() { Syntax = method, Symbol = symbol, Refusal = PreRefusal(method, symbol) };
            m_candidates[symbol] = candidate;
            m_ordered.Add(candidate);
        }

        private string? PreRefusal(MethodDeclarationSyntax method, IMethodSymbol symbol)
        {
            if (HasTwinMember(symbol))
                return "has-twin";

            if (method.Parent is InterfaceDeclarationSyntax)
                return "interface-member";

            if (method.ExplicitInterfaceSpecifier is not null)
                return "explicit-impl";

            if (!symbol.IsStatic)
                return "instance";

            if (symbol.ReturnsByRef || symbol.ReturnsByRefReadonly)
                return "ref-return";

            for (int i = 0; i < symbol.Parameters.Length; i++)
            {
                if (symbol.Parameters[i].RefKind != RefKind.None)
                    return i == 0 && symbol.IsExtensionMethod ? "byref-recv" : "byref-param";
            }

            if (symbol.ReturnType.IsRefLikeType || symbol.Parameters.Any(parameter => parameter.Type.IsRefLikeType))
                return "reflike";

            if (method.Modifiers.Any(SyntaxKind.UnsafeKeyword) || method.Ancestors().OfType<TypeDeclarationSyntax>().Any(type => type.Modifiers.Any(SyntaxKind.UnsafeKeyword)) ||
                symbol.ReturnType.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer ||
                symbol.Parameters.Any(parameter => parameter.Type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer))
                return "unsafe";

            if (symbol.ReturnType.Name is "Task" or "ValueTask")
                return "async-shaped";

            if (method.DescendantNodes().OfType<YieldStatementSyntax>().Any())
                return "iterator";

            FileLinePositionSpan span = method.SyntaxTree.GetLineSpan(method.Span);

            foreach ((string path, int line, string id) in m_refuse)
            {
                if (line >= span.StartLinePosition.Line + 1 && line <= span.EndLinePosition.Line + 1 &&
                    string.Equals(Path.GetFullPath(path), Path.GetFullPath(method.SyntaxTree.FilePath), StringComparison.OrdinalIgnoreCase))
                    return id;
            }

            return null;
        }

        internal static IMethodSymbol Normalize(IMethodSymbol method)
        {
            IMethodSymbol definition = (method.ReducedFrom ?? method).OriginalDefinition;
            return definition.PartialDefinitionPart ?? definition;
        }

        // Whether the method's type holds a twin of it: a hand-written golib twin, or a generated twin of a
        // referenced assembly, which is how a call crosses assemblies.
        private bool HasTwinMember(IMethodSymbol definition)
        {
            if (m_hasTwin.TryGetValue(definition, out bool has))
                return has;

            if (definition.ContainingType is { } type)
            {
                foreach (IMethodSymbol twin in type.GetMembers(definition.Name + Suffix).OfType<IMethodSymbol>())
                {
                    if (twin.IsStatic != definition.IsStatic || twin.Parameters.Length != definition.Parameters.Length ||
                        twin.TypeParameters.Length != definition.TypeParameters.Length)
                        continue;

                    has = true;

                    for (int i = 0; has && i < twin.Parameters.Length; i++)
                        has = twin.Parameters[i].Type.ToDisplayString() == definition.Parameters[i].Type.ToDisplayString();

                    if (has)
                        break;
                }
            }

            return m_hasTwin[definition] = has;
        }

        internal bool CanPark(IMethodSymbol? target)
        {
            if (target is null)
                return false;

            return HasTwinMember(target) || m_candidates.TryGetValue(target, out Candidate? candidate) && candidate.Twinned;
        }

        internal bool ShouldAwait(Site site) =>
            site is { Active: true, Go: GoKind.None, Renameable: true, Delegate: false } &&
            (site.Interface ? site.IfaceCallable && IfaceCanPark(site.IfaceTarget) : CanPark(site.Target));

        internal bool GoTwinnable(Site site) =>
            site.Active && (site.Go == GoKind.Literal || site.Go == GoKind.Group && CanPark(site.GoTarget));

        private bool ComputeParks(Candidate candidate)
        {
            foreach (Site site in candidate.Sites)
            {
                if (site.Direct && (ShouldAwait(site) || goSites && GoTwinnable(site)))
                    return true;
            }

            return false;
        }

        internal static bool IsRenameable(ExpressionSyntax expression) => expression switch
        {
            IdentifierNameSyntax name => name.Identifier.Text != "goǃ",
            GenericNameSyntax => true,
            MemberAccessExpressionSyntax access => access.IsKind(SyntaxKind.SimpleMemberAccessExpression),
            _ => false
        };

        internal Site Classify(InvocationExpressionSyntax node, SemanticModel model)
        {
            Site site = new();

            if (node.Expression is IdentifierNameSyntax { Identifier.Text: "goǃ" })
            {
                SeparatedSyntaxList<ArgumentSyntax> arguments = node.ArgumentList.Arguments;

                if (arguments.Count == 1 && arguments[0].Expression is ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 0 } lambda &&
                    !lambda.Modifiers.Any(SyntaxKind.AsyncKeyword))
                {
                    site.Go = GoKind.Literal;
                }
                else if (arguments.Count is >= 1 and <= 5 && arguments.All(argument => argument.RefKindKeyword.IsKind(SyntaxKind.None) && argument.NameColon is null) &&
                         IsRenameable(arguments[0].Expression) && model.GetSymbolInfo(arguments[0].Expression).Symbol is IMethodSymbol { ReturnsVoid: true } target)
                {
                    site.Go = GoKind.Group;
                    site.GoTarget = Normalize(target);
                }
                else
                {
                    site.Go = GoKind.Other;
                }

                return site;
            }

            site.Renameable = IsRenameable(node.Expression);

            if (model.GetSymbolInfo(node).Symbol is not IMethodSymbol method)
            {
                site.Unbound = node.Expression is not IdentifierNameSyntax { Identifier.Text: "nameof" };
                return site;
            }

            if (method.MethodKind == MethodKind.DelegateInvoke)
                site.Delegate = true;
            else if (method.MethodKind != MethodKind.LocalFunction && method.ContainingType?.TypeKind == TypeKind.Interface)
            {
                // An interface call is awaited only in its plain shape, a member read off an interface-typed value.
                site.Interface = true;
                site.IfaceTarget = method.OriginalDefinition;
                site.IfaceCallable = !method.IsStatic &&
                    node.Expression is MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.SimpleMemberAccessExpression, Name: IdentifierNameSyntax } access &&
                    model.GetTypeInfo(access.Expression).Type?.TypeKind == TypeKind.Interface;
            }
            else if (method.MethodKind != MethodKind.LocalFunction)
                site.Target = Normalize(method);

            return site;
        }
    }

    // Records every call of a candidate: active in the function's own body and in the bodies of its go
    // literals, passive inside any other closure, where nothing is awaited.
    private sealed class Collector(Run run, SemanticModel model, Candidate candidate) : CSharpSyntaxWalker
    {
        private int m_passive, m_literal;

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            Site site = run.Classify(node, model);
            site.Active = m_passive == 0;
            site.Direct = m_passive == 0 && m_literal == 0;
            run.Sites[node] = site;
            candidate.Sites.Add(site);

            if (site is { Go: GoKind.Literal, Active: true })
            {
                m_literal++;
                Visit(((ParenthesizedLambdaExpressionSyntax)node.ArgumentList.Arguments[0].Expression).Body);
                m_literal--;
                return;
            }

            base.VisitInvocationExpression(node);
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            m_passive++;
            base.VisitParenthesizedLambdaExpression(node);
            m_passive--;
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            m_passive++;
            base.VisitSimpleLambdaExpression(node);
            m_passive--;
        }

        public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            m_passive++;
            base.VisitAnonymousMethodExpression(node);
            m_passive--;
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            m_passive++;
            base.VisitLocalFunctionStatement(node);
            m_passive--;
        }
    }
}

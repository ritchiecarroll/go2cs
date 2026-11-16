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

        // A call through a func value that a twin CAN test for a pair: the callee is a local or parameter whose delegate
        // type has a canonical twin delegate (the rewriter's own PairShape test, asked here for N3's caller rule).
        public bool PairCallable;
        public IMethodSymbol? IfaceTarget;

        // A go statement the generator cannot launch as a twin (GoKind.Other): why, and whether its goroutine could park
        // (the LOUD fallback, GO2CS0014). PairLaunch: a func value a twin's go statement tests for a pair (fix 6).
        public string? GoReason;
        public bool GoCouldPark, PairLaunch, PairBody;
    }

    // golib's goǃ and goǃtwin ladders take the goroutine's function and at most 16 arguments.
    internal const int MaxGoArguments = 16;

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

        // GoTwinsPairs=false: N3's D3 core off (no pairs, no pair test), the A/B control.
        bool pairs = !(options.TryGetValue("build_property.GoTwinsPairs", out string? pairing) && string.Equals(pairing, "false", StringComparison.OrdinalIgnoreCase));

        // GoTwinsCallerRule=false: N3's caller rule alone off, the pair core kept -- the rule's own A/B control.
        bool callerRule = !(options.TryGetValue("build_property.GoTwinsCallerRule", out string? ruling) && string.Equals(ruling, "false", StringComparison.OrdinalIgnoreCase));

        new Run(context, goSites, ifaceShape, ifaceAll) { Self = self, Pairs = pairs, CallerRule = callerRule }.Execute();
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

            // C1's refusal, by name, after the fixpoint: a function whose twin would hold a ref it cannot derive again
            // across an await (TwinGenerator.RefAliases.cs). Its callers keep their twins; their calls to it run plain.
            foreach (Candidate candidate in m_ordered)
            {
                if (candidate.Twinned && UnrederivableLive(candidate))
                    candidate.Refusal = "ref-unrederivable";
            }

            foreach (Candidate candidate in m_ordered)
            {
                candidate.GoOnly = candidate.Parks && !candidate.Sites.Any(site => site.Direct && (ShouldAwait(site) || site.Delegate && CallerRuleTwins(site)));

                // Must-fix 3 (DESIGN-mn-func-values.md 10.4): a function refused a twin whose body calls through a func
                // value warns ONCE, at the function, naming its refusal. Its calls run the plain delegate even when the
                // value is a pair, so a can-park func value blocks a carrier there; iterators, the yield callers, are the
                // largest such set. A function with a twin of its own (has-twin) is not a hole.
                if (candidate.Refusal is { } refusal && refusal != "has-twin" &&
                    candidate.Sites.Any(site => site.Direct && site.Delegate && CallerRuleTwins(site)))
                {
                    Context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.RefusedFuncValueCaller,
                        candidate.Syntax.Identifier.GetLocation(), candidate.Symbol.ToDisplayString(), refusal));
                }

                ReportPlainLaunches(candidate);
            }

            m_analysisMs = m_watch.ElapsedMilliseconds - m_siblingMs;
            Emit();
        }

        internal int GoPlain;

        // The LOUD fallback (COORD's A3 ruling; the owner's severity, FREEZE-SHEET.md 2026-11-13): a go statement the
        // generator does not launch as a twin keeps its plain launch -- a thread, where a goroutine that parks blocks it --
        // and warns ONCE, at the statement, naming the reason. It does not warn where the goroutine's function provably
        // never parks: nothing is lost there. Named limit: a go statement inside a literal that may become a pair (its
        // launch follows the pair's) is not judged here.
        private void ReportPlainLaunches(Candidate candidate)
        {
            if (candidate.Refusal == "has-twin")
                return;

            foreach (InvocationExpressionSyntax node in candidate.Syntax.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (!Sites.TryGetValue(node, out Site? site) || site is not { Active: true, PairBody: false } || site.Go == GoKind.None)
                    continue;

                bool couldPark = site.Go switch
                {
                    GoKind.Literal => true,
                    GoKind.Group => CanPark(site.GoTarget),
                    _ => site.GoCouldPark || CanPark(site.GoTarget)
                };

                if (!couldPark)
                    continue;

                // In a twin, only an Other launch stays plain (a pair-testing func value does not). In a function refused a
                // twin, every launch does. A function that is neither parks nowhere: its literal and group launches are
                // plain only because GoTwinsGo=false asked for it, so they do not warn.
                string? reason = candidate.Twinned ? site.Go == GoKind.Other && !site.PairLaunch ? site.GoReason : null
                    : candidate.Refusal is { } refusal ? "refused " + refusal
                    : site.Go == GoKind.Other ? site.GoReason : null;

                if (reason is null)
                    continue;

                GoPlain++;
                Context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.PlainGoLaunch, node.GetLocation(), candidate.Symbol.ToDisplayString(), reason));
            }
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

        // M:N PROTOTYPE (A1): the never-twinned rule for native wrappers (the cgo design's M4). A method that is extern, that
        // carries DllImport or LibraryImport, or whose body pins with `fixed`, is synchronous: a `fixed` scope cannot span a
        // park. A twin that calls one calls the plain method, and blocks its carrier for the length of the call.
        //
        // M:N PROTOTYPE (A2): the closed list. A method is refused as "native" when any one of these holds, and for no
        // other reason:
        //   1. it is extern;
        //   2. it carries DllImport or LibraryImport;
        //   3. its body holds a `fixed` statement, anywhere;
        //   4. its body holds an `unsafe` block, anywhere (C# cannot await in an unsafe context, CS4004).
        // Named limits of the list:
        //   - rows 3 and 4 refuse the whole method, also when its parking call sits outside the fixed or unsafe scope;
        //   - UnmanagedCallersOnly is not a row. Such a method is only ever entered from native code, so no twin calls it.
        // A method with the `unsafe` modifier, or with a pointer in its signature, is refused further down as "unsafe".
        private static bool IsNative(MethodDeclarationSyntax method, IMethodSymbol symbol) =>
            symbol.IsExtern || method.Modifiers.Any(SyntaxKind.ExternKeyword) ||
            symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name is "DllImportAttribute" or "LibraryImportAttribute") ||
            method.DescendantNodes().OfType<FixedStatementSyntax>().Any() ||
            method.DescendantNodes().OfType<UnsafeStatementSyntax>().Any();

        private string? PreRefusal(MethodDeclarationSyntax method, IMethodSymbol symbol)
        {
            // First, so that relaxing a row below it can never admit a native wrapper.
            if (IsNative(method, symbol))
                return "native";

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

            if (symbol.ReturnType.Name is "Task" or "ValueTask" or "GoTask")
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
                // A call through a func value twins its function only by N3's caller rule (TwinGenerator.Pairs.cs).
                if (site.Direct && (ShouldAwait(site) || goSites && GoTwinnable(site) || site.Delegate && CallerRuleTwins(site)))
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

        // A go statement the generator launches plain, and why. couldPark false: it could park only if its function can,
        // which the fallback asks once the fixpoint has settled (site.GoTarget).
        private static Site GoOther(Site site, string reason, bool couldPark)
        {
            site.Go = GoKind.Other;
            site.GoReason = reason;
            site.GoCouldPark = couldPark;
            return site;
        }

        internal Site Classify(InvocationExpressionSyntax node, SemanticModel model)
        {
            Site site = new();

            if (node.Expression is IdentifierNameSyntax { Identifier.Text: "goǃ" })
            {
                SeparatedSyntaxList<ArgumentSyntax> arguments = node.ArgumentList.Arguments;

                bool plainArguments = arguments.Count is >= 1 and <= MaxGoArguments + 1 &&
                    arguments.All(argument => argument.RefKindKeyword.IsKind(SyntaxKind.None) && argument.NameColon is null);

                // A go literal, with or without arguments: `go func(i int) { ... }(v)` is written with the literal's
                // parameter types spelled out, so its twin lambda takes them as they are.
                if (arguments.Count >= 1 && arguments[0].Expression is ParenthesizedLambdaExpressionSyntax lambda)
                {
                    if (plainArguments && !lambda.Modifiers.Any(SyntaxKind.AsyncKeyword) && lambda.ParameterList.Parameters.Count == arguments.Count - 1 &&
                        lambda.ParameterList.Parameters.All(parameter => parameter.Type is not null && parameter.Modifiers.Count == 0))
                    {
                        site.Go = GoKind.Literal;
                        return site;
                    }

                    return GoOther(site, "literal-shape", true);
                }

                if (arguments.Count > MaxGoArguments + 1)
                    return GoOther(site, "arity", true);

                ISymbol? callee = arguments.Count >= 1 ? model.GetSymbolInfo(arguments[0].Expression).Symbol : null;

                if (callee is IMethodSymbol { MethodKind: not MethodKind.DelegateInvoke } goMethod)
                {
                    IMethodSymbol target = Normalize(goMethod);

                    if (plainArguments && IsRenameable(arguments[0].Expression) && goMethod.ReturnsVoid)
                    {
                        site.Go = GoKind.Group;
                        site.GoTarget = target;
                        return site;
                    }

                    site.GoTarget = target;

                    return GoOther(site, !plainArguments ? "ref-argument" : !goMethod.ReturnsVoid ? "returns-value" : "callee-shape", false);
                }

                // A func value: a twin's go statement tests it for a pair when it is a void local or parameter (fix 6).
                if (arguments.Count >= 1 && model.GetTypeInfo(arguments[0].Expression).Type is INamedTypeSymbol { TypeKind: TypeKind.Delegate } delegateType)
                {
                    site.PairLaunch = Pairs && plainArguments && arguments[0].Expression is IdentifierNameSyntax &&
                        delegateType.DelegateInvokeMethod is { ReturnsVoid: true } invoke && invoke.Parameters.Length == arguments.Count - 1 &&
                        TwinDelegateType(delegateType) is not null;

                    return GoOther(site, "func-value", TwinDelegateType(delegateType) is not null);
                }

                return GoOther(site, !plainArguments ? "ref-argument" : "unbound", true);
            }

            site.Renameable = IsRenameable(node.Expression);

            if (model.GetSymbolInfo(node).Symbol is not IMethodSymbol method)
            {
                site.Unbound = node.Expression is not IdentifierNameSyntax { Identifier.Text: "nameof" };
                return site;
            }

            if (method.MethodKind == MethodKind.DelegateInvoke)
            {
                site.Delegate = true;
                site.PairCallable = node.Expression is IdentifierNameSyntax &&
                    model.GetTypeInfo(node.Expression).Type is INamedTypeSymbol { TypeKind: TypeKind.Delegate } delegateType &&
                    TwinDelegateType(delegateType) is not null;
            }
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
        private int m_passive, m_literal, m_pair;

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            Site site = run.Classify(node, model);
            site.Active = m_passive == 0;
            site.Direct = m_passive == 0 && m_literal == 0;
            site.PairBody = m_pair > 0;
            run.Sites[node] = site;
            candidate.Sites.Add(site);

            if (site is { Go: GoKind.Literal, Active: true })
            {
                m_literal++;
                Visit(((ParenthesizedLambdaExpressionSyntax)node.ArgumentList.Arguments[0].Expression).Body);
                m_literal--;

                // A go literal's arguments are evaluated by the go statement, in the function's own body.
                foreach (ArgumentSyntax argument in node.ArgumentList.Arguments.Skip(1))
                    Visit(argument);

                return;
            }

            base.VisitInvocationExpression(node);
        }

        // N3's D3 core: a literal in the function's own body may become a pair, so its calls are ACTIVE (its
        // twin lambda awaits them) but never DIRECT, as a go literal's: they never make the enclosing function park.
        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            bool pairable = run.Pairs && m_passive == 0 && !node.Modifiers.Any(SyntaxKind.AsyncKeyword);

            if (pairable) { m_literal++; m_pair++; } else m_passive++;
            base.VisitParenthesizedLambdaExpression(node);
            if (pairable) { m_literal--; m_pair--; } else m_passive--;
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            bool pairable = run.Pairs && m_passive == 0 && !node.Modifiers.Any(SyntaxKind.AsyncKeyword);

            if (pairable) { m_literal++; m_pair++; } else m_passive++;
            base.VisitSimpleLambdaExpression(node);
            if (pairable) { m_literal--; m_pair--; } else m_passive--;
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

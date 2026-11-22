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
/// M:N scheduler, experimental and opt-in (GoTwins=true to build, GO2CS_MN=1 to run); inert otherwise.
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
        // (the LOUD fallback, GO2CS0015). PairLaunch: a func value a twin's go statement tests for a pair (fix 6).
        public string? GoReason;
        public bool GoCouldPark, PairLaunch, PairBody;

        // byref-param (12.1, Thunks): a by-ref go wrapper whose every by-ref argument is a box the go statement bound, and
        // its one call (set once the collector has visited the literal). It launches its twin only where that call is awaited.
        public bool RefThunk;
        public Site? Thunk;

        // N2 (DESIGN-mn-box-twin.md 9): a call to a function whose twin takes its receiver's BOX. BoxOf is the box this
        // call site names for the receiver without a copy (null: none, and the call stays plain, named); ViaBoxOverload,
        // that the call binds RecvGenerator's box overload `M(this ж<T>)`, whose receiver already is the box.
        public string? BoxOf;
        public bool ViaBoxOverload;

        // The box overload a ViaBoxOverload call bound, kept: where the `this ref` method behind it gets no box twin, the
        // call parks as at the base, through the overload's own twin (stage (c)'s forwarder twin, `Lockꞏtwin(this ж<T>)`).
        public IMethodSymbol? BoxOverload;

        // Code review 4: a generic `this ref` method this box-overload call would reach once constructed, not followed.
        public IMethodSymbol? BoxGenericUnfollowed;

        // byref-param (12.1): the box each by-ref argument hands a byref-param box twin (ArgBoxes[i] for argument i), or
        // null with ArgPlain naming the first by-ref argument that names none -- the whole call is then plain (all or
        // nothing per call).
        public string?[]? ArgBoxes;
        public string? ArgPlain;

        // The call, for the report's named-plain row of a function that parks nowhere else (SN-7).
        public InvocationExpressionSyntax? Node;
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

        // N2: the receiver parameter's name when this `this ref T` method's twin takes the box (`Mꞏtwin(this ж<T> Ꮡt)`).
        public string? BoxReceiver;

        // byref-param (12.1): the names of the plain `ref` parameters whose boxes this function's twin takes (`Ꮡp`).
        public List<string>? BoxParams;

        // PB-5 (12.5): the stack locals a box can be named for (`Ꮡx`, the twin declares x a heap local), by name with
        // their type, and those refused, with the reason a `// P` row names.
        public readonly Dictionary<string, ITypeSymbol> Promotable = new(System.StringComparer.Ordinal);
        public readonly Dictionary<string, string> PromoteRefused = new(System.StringComparer.Ordinal);
        public readonly List<Site> Sites = [];
        public bool Twinned => Refusal is null && Parks;

        // 7a (DESIGN-mn-twin-set.md 12.1, R3): a member of the population with no state-machine twin gets a FORWARDER,
        // `[GoParkForwarder] Mꞏtwin(...)`, which calls the plain member and returns a completed task.
        public bool Forwarded;
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context) =>
        Execute(context, typeof(TwinGenerator));

    // Every build property that changes what the twin generator writes, other than GoTwins itself. Outside a prototype
    // arm (GoTwinsArm=true) each one is refused by name (GO2CS0018); none may reach a package that ships. The guard's
    // test seam (GoTwinsGuardDrop) is here too, and a twin-refuse.txt additional file is refused the same way.
    internal static readonly string[] ArmSwitches =
    [
        "GoTwinsIface", "GoTwinsIfaceAll", "GoTwinsIfaceUnmark", "GoTwinsGo", "GoTwinsPairs", "GoTwinsCallerRule",
        "GoTwinsForwarders", "GoTwinsGuardWarn", "GoTwinsGuardDrop",
        "GoTwinsBoxRecv", "GoTwinsBoxForwarders", "GoTwinsBoxCopy", "GoTwinsBoxParam", "GoTwinsBoxArgCopy"
    ];

    // GoTwinsSiblings=last (B4) hands the work to ZTwinGenerator, which the compiler runs after every sibling.
    internal static void Execute(GeneratorExecutionContext context, Type self)
    {
        AnalyzerConfigOptions options = context.AnalyzerConfigOptions.GlobalOptions;

        if (!options.TryGetValue("build_property.GoTwins", out string? on) || !string.Equals(on, "true", StringComparison.OrdinalIgnoreCase))
            return;

        if (SiblingMemo.Last(options) != (self == typeof(ZTwinGenerator)))
            return;

        bool goSites = !(options.TryGetValue("build_property.GoTwinsGo", out string? go) && string.Equals(go, "false", StringComparison.OrdinalIgnoreCase));

        // O2 (7a, DESIGN-mn-twin-set.md 12.1 and section 10): a package built with twins carries the WHOLE set, built one
        // way, so every switch that changes what is generated is refused (GO2CS0018), naming it: the ones that select part
        // of the set, and the measurement arms' A/B controls, two of which (GoTwinsBoxCopy, GoTwinsBoxArgCopy) are wrong on
        // purpose. A prototype arm sets GoTwinsArm=true, which admits the switches and keeps their old defaults.
        // GoTwinsSiblings and GoTwinsTiming are not refused: the first picks how the generator reads its siblings' output,
        // not what it twins; the second only adds a stopwatch to the report. A property MSBuild makes visible arrives
        // empty when unset.
        bool arm = options.TryGetValue("build_property.GoTwinsArm", out string? armed) && string.Equals(armed, "true", StringComparison.OrdinalIgnoreCase);
        bool Set(string name) => options.TryGetValue("build_property." + name, out string? value) && !string.IsNullOrWhiteSpace(value);

        if (!arm)
        {
            foreach (string name in ArmSwitches)
            {
                if (Set(name))
                    context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.PartialTwinSet, Location.None, name));
            }

            if (context.AdditionalFiles.Any(text => string.Equals(Path.GetFileName(text.Path), "twin-refuse.txt", StringComparison.OrdinalIgnoreCase)))
                context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.PartialTwinSet, Location.None, "GoTwinsRefuse"));
        }

        // GoTwinsIface picks the interface twin shape (hidden or default); GoTwinsIfaceAll twins every interface member,
        // not only the ones an implementer of this assembly can park in. Unset, a package built with twins gets the
        // whole set, the hidden shape for every member (R7); an arm gets none, as the prototype always did.
        options.TryGetValue("build_property.GoTwinsIface", out string? shape);

        IfaceShape ifaceShape = !Set("GoTwinsIface") ? (arm ? IfaceShape.None : IfaceShape.Hidden) :
            string.Equals(shape, "hidden", StringComparison.OrdinalIgnoreCase) ? IfaceShape.Hidden :
            string.Equals(shape, "default", StringComparison.OrdinalIgnoreCase) ? IfaceShape.Default : IfaceShape.None;

        bool ifaceAll = !Set("GoTwinsIfaceAll") ? !arm :
            options.TryGetValue("build_property.GoTwinsIfaceAll", out string? all) && string.Equals(all, "true", StringComparison.OrdinalIgnoreCase);

        // GoTwinsPairs=false: N3's D3 core off (no pairs, no pair test), the A/B control.
        bool pairs = !(options.TryGetValue("build_property.GoTwinsPairs", out string? pairing) && string.Equals(pairing, "false", StringComparison.OrdinalIgnoreCase));

        // GoTwinsCallerRule=false: N3's caller rule alone off, the pair core kept -- the rule's own A/B control.
        bool callerRule = !(options.TryGetValue("build_property.GoTwinsCallerRule", out string? ruling) && string.Equals(ruling, "false", StringComparison.OrdinalIgnoreCase));

        // GoTwinsIfaceUnmark (stage (c), record 10.3): interface members, `Namespace.Type.Member` with ',' or ';' between,
        // that carry no park mark: no twin slot, so a caller in any assembly calls them plain.
        HashSet<string> unmark = new(StringComparer.Ordinal);

        if (options.TryGetValue("build_property.GoTwinsIfaceUnmark", out string? unmarked))
        {
            foreach (string entry in unmarked.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries))
                unmark.Add(entry.Trim());
        }

        // 7a's closure arms (section 12.3): GoTwinsForwarders=false writes no forwarder, the same-tree control for their
        // bytes and build wall (Q4, Q5); GoTwinsGuardWarn=true reports GO2CS0017 as a warning, so a census build runs to
        // the end and counts every GUARD row (Q2). Both are arm-only (O2).
        bool forwarders = !(options.TryGetValue("build_property.GoTwinsForwarders", out string? forwarding) && string.Equals(forwarding, "false", StringComparison.OrdinalIgnoreCase));
        bool guardWarn = options.TryGetValue("build_property.GoTwinsGuardWarn", out string? warn) && string.Equals(warn, "true", StringComparison.OrdinalIgnoreCase);

        // 7a's test seam for the guard (F9): GoTwinsGuardDrop names functions, ',' or ';' between, whose forwarders are
        // dropped, so GO2CS0017 must name each one. Never a build anyone ships.
        HashSet<string> guardDrop = new(StringComparer.Ordinal);

        if (options.TryGetValue("build_property.GoTwinsGuardDrop", out string? dropped))
        {
            foreach (string entry in dropped.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries))
                guardDrop.Add(entry.Trim());
        }

        // N2 (DESIGN-mn-box-twin.md 9.8): GoTwinsBoxRecv=false keeps every by-ref receiver refused, the cut's A/B control;
        // GoTwinsBoxForwarders=false keeps the interface forwarders to box twins sync, their own arm; GoTwinsBoxCopy=true
        // makes a box twin COPY its receiver -- wrong on purpose, the aliasing probe's negative control (SN-6), never a
        // build anyone ships.
        bool boxRecv = !(options.TryGetValue("build_property.GoTwinsBoxRecv", out string? box) && string.Equals(box, "false", StringComparison.OrdinalIgnoreCase));
        bool boxForwarders = !(options.TryGetValue("build_property.GoTwinsBoxForwarders", out string? forward) && string.Equals(forward, "false", StringComparison.OrdinalIgnoreCase));
        bool boxCopy = options.TryGetValue("build_property.GoTwinsBoxCopy", out string? copy) && string.Equals(copy, "true", StringComparison.OrdinalIgnoreCase);

        // byref-param (DESIGN-mn-box-twin.md 12.1): GoTwinsBoxParam=false keeps every ref-lowered parameter refused, its A/B
        // control (equal to arm N by construction); GoTwinsBoxArgCopy=true makes each call hand a COPY of its box, the
        // call-site control of the probe (SB-4), as GoTwinsBoxCopy is the callee's.
        bool boxParam = !(options.TryGetValue("build_property.GoTwinsBoxParam", out string? param) && string.Equals(param, "false", StringComparison.OrdinalIgnoreCase));
        bool boxArgCopy = options.TryGetValue("build_property.GoTwinsBoxArgCopy", out string? argCopy) && string.Equals(argCopy, "true", StringComparison.OrdinalIgnoreCase);

        new Run(context, goSites, ifaceShape, ifaceAll)
        {
            Self = self, Pairs = pairs, CallerRule = callerRule, IfaceUnmark = unmark, GuardDrop = guardDrop, Forwarders = forwarders, GuardWarn = guardWarn, BoxRecv = boxRecv, BoxForwarders = boxForwarders, BoxCopy = boxCopy,
            BoxParam = boxParam, BoxArgCopy = boxArgCopy
        }.Execute();
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

        // 7a: whether this assembly has a population at all (it is not hand-owned, R5).
        private bool m_populationAssembly;

        // 7a's guard seam (F9): the functions whose forwarders are dropped.
        internal HashSet<string> GuardDrop { get; init; } = [];

        // 7a's closure-arm switches (Execute): forwarders off, and the guard as a warning.
        internal bool Forwarders { get; init; } = true;
        internal bool GuardWarn { get; init; }

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
                else if (candidate.Twinned && AwaitsInLock(candidate))
                    candidate.Refusal = "await-in-lock";
                // Gated on GoTwinsBoxRecv (code review 5): with the switch off the base is reproduced by construction, not
                // by the detector's zero count on one closure. Its hazard predates N2, so it is N2's own change, kept with N2.
                else if (candidate.Twinned && BoxRecv && SharedMemberRef(candidate))
                    candidate.Refusal = "ref-shared-member";
            }

            m_populationAssembly = !Common.IsHandOwnedAssembly(Context.Compilation);

            foreach (Candidate candidate in m_ordered)
                candidate.Forwarded = m_populationAssembly && Forwarders && !candidate.Twinned && InPopulation(candidate) && !IsE1(candidate.Symbol) && NotForwardable(candidate) is null &&
                    !GuardDrop.Contains(candidate.Symbol.Name);

            // The guard (GO2CS0017, section 8): a member of the population that is neither twinned, forwarded nor E1.
            // GoTwinsForwarders=false (an arm's control) reproduces the build before 7a: no forwarder, so no guard either.
            if (m_populationAssembly && Forwarders)
            {
                foreach (Candidate candidate in m_ordered.Where(InPopulation))
                {
                    string verdict = Classify(candidate);

                    if (verdict.StartsWith("GUARD:", StringComparison.Ordinal))
                        Context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.TwinSetGap, candidate.Syntax.Identifier.GetLocation(),
                            GuardWarn ? DiagnosticSeverity.Warning : DiagnosticSeverity.Error, null, null, candidate.Symbol.ToDisplayString(), verdict.Substring("GUARD:".Length)));
                }
            }

            foreach (Candidate candidate in m_ordered)
            {
                // N2 (SN-7): a function whose only parks were calls to a box twin it has no box for parks NOT AT ALL, so
                // no twin's rewriter sees those calls; they are named here, beside the plain calls inside twins. A REFUSED
                // function is not: it never has a twin, and its calls are plain under its own refusal's name (closure arm
                // N: 12 of 37 rows were fmt's `ss` methods, refused reflike for their sstring parameters).
                if (!candidate.Parks && candidate.Refusal is null)
                {
                    foreach (Site site in candidate.Sites)
                    {
                        if (site is { Active: true, PairBody: false, Node: { } node } && IsBoxTwinTarget(site.Target) && !BoxCallable(site))
                            NoteBoxPlain(node, site.Target!, text: site.ArgPlain);
                    }
                }

                // Code review 4: a box-overload call to a GENERIC method with a box twin, not followed, stays plain: named, in a
                // twin or not (no rewriter sees it as a box-twin call, so no row is written twice).
                if (candidate.Refusal is null)
                {
                    foreach (Site site in candidate.Sites)
                    {
                        if (site is { Active: true, PairBody: false, Node: { } node, BoxGenericUnfollowed: { } generic } && IsBoxTwinTarget(generic) && !ShouldAwait(site))
                            NoteBoxPlain(node, generic, "generic-unfollowed");
                    }
                }

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

        // Stage (c) (2026-11-15): C# cannot await inside a `lock` (CS1996), and a Monitor is owned by a thread, so a twin
        // that resumed elsewhere could not release it. A hand-owned function that would await inside one (runtime's
        // gcCycle) is refused its twin by name; its callers call it plain, holding the carrier, as the lock always did.
        private bool AwaitsInLock(Candidate candidate) =>
            candidate.Syntax.DescendantNodes().OfType<InvocationExpressionSyntax>().Any(node =>
                Sites.TryGetValue(node, out Site? site) && site.Active && (ShouldAwait(site) || site.Delegate && CallerRuleTwins(site)) &&
                node.Ancestors().TakeWhile(ancestor => ancestor != candidate.Syntax).OfType<LockStatementSyntax>().Any());

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
                string? reason = candidate.Twinned ? PlainGoReason(site)
                    : candidate.Refusal is { } refusal ? "refused " + refusal
                    : site.Go == GoKind.Other ? site.GoReason : PlainGoReason(site);

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
            // A bodyless `extern` member is a candidate too (P3's read of 4afbe98217): it can never be twinned (IsNative
            // refuses it), but a PUBLIC one in a converted file is in the population and gets a forwarder (R3).
            // Before this it was never seen, so it had no twin, no forwarder and no guard row. Any other bodyless
            // method (an abstract or partial declaration) still is not a candidate. A bodyless `partial` declaration
            // in a converted file is a Go function whose body is hand-written (its implementing part in an `_impl.cs`
            // companion: assembly, linkname or runtime-provided). That is R5, the hand-owned population, which moves
            // with row 7 (DESIGN-mn-twin-set.md 12.1 and 12.6, P3's finding 6: 127 distinct public functions in 15
            // packages, sync/atomic and internal/runtime/atomic the bulk).
            if (method.Body is null && method.ExpressionBody is null && !method.Modifiers.Any(SyntaxKind.ExternKeyword) || method.Parent is not TypeDeclarationSyntax ||
                method.Modifiers.Any(SyntaxKind.AsyncKeyword) || method.Identifier.Text.EndsWith(Suffix, StringComparison.Ordinal))
                return;

            if (Model(method.SyntaxTree).GetDeclaredSymbol(method, Context.CancellationToken) is not { } declared)
                return;

            IMethodSymbol symbol = Normalize(declared);

            if (m_candidates.ContainsKey(symbol))
                return;

            Candidate candidate = new() { Syntax = method, Symbol = symbol, Refusal = PreRefusal(method, symbol) };

            if (candidate.Refusal is null)
            {
                candidate.BoxReceiver = BoxReceiverOf(method, symbol);
                candidate.BoxParams = BoxParamsOf(method, symbol);
            }

            m_candidates[symbol] = candidate;
            m_ordered.Add(candidate);
        }

        // M:N (A1): the never-twinned rule for native wrappers (the cgo design's M4). A method that is extern, that
        // carries DllImport or LibraryImport, or whose body pins with `fixed`, is synchronous: a `fixed` scope cannot span a
        // park. A twin that calls one calls the plain method, and blocks its carrier for the length of the call.
        //
        // M:N (A2): the closed list. A method is refused as "native" when any one of these holds, and for no
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

            // Stage (c)'s SC-1 (COORD's GO, 2026-11-15): a twin is written into its host reopened as `partial`, so a method
            // whose host chain is not all partial (a hand-owned helper such as runtime's Sysmon) cannot get one: CS0260.
            // Refused by name, as the interface forwarders skip such a host (ForwarderSkip). Its callers keep their twins
            // and call it plain: a thread, never a wrong answer. The count is record 10.8's owed number.
            if (method.Ancestors().OfType<TypeDeclarationSyntax>().Any(type => !type.Modifiers.Any(SyntaxKind.PartialKeyword)))
                return "host-not-partial";

            if (method.Parent is InterfaceDeclarationSyntax)
                return "interface-member";

            if (method.ExplicitInterfaceSpecifier is not null)
                return "explicit-impl";

            if (!symbol.IsStatic)
                return "instance";

            if (symbol.ReturnsByRef || symbol.ReturnsByRefReadonly)
                return "ref-return";

            // N2 and byref-param: a by-ref receiver or parameter that gets a box is no refusal (ByRefRefusal).
            if (ByRefRefusal(method, symbol) is { } byRef)
                return byRef;

            if (symbol.ReturnType.IsRefLikeType || symbol.Parameters.Any(parameter => parameter.Type.IsRefLikeType))
                return "reflike";

            if (method.Modifiers.Any(SyntaxKind.UnsafeKeyword) || method.Ancestors().OfType<TypeDeclarationSyntax>().Any(type => type.Modifiers.Any(SyntaxKind.UnsafeKeyword)) ||
                symbol.ReturnType.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer ||
                symbol.Parameters.Any(parameter => parameter.Type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer))
                return "unsafe";

            if (IsTaskShaped(symbol.ReturnType))
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

        // P3's read of 80c099ee33: a twin outside this compilation's own assembly binds only when it is PUBLIC. A friend grant
        // (InternalsVisibleTo) must not hand a caller a twin that is not that assembly's surface: sync's and time's hand
        // twins are internal by ruling (FREEZE-SHEET.md, 7b), and a twin is part of the frozen surface or not bound at all.
        internal bool TwinVisible(IMethodSymbol twin) =>
            SymbolEqualityComparer.Default.Equals(twin.ContainingAssembly, m_augmented.Assembly) || twin.DeclaredAccessibility == Accessibility.Public;

        // 7a (R3): a twin a caller may await -- visible, and not a forwarder. A forwarder only wraps its plain member, so a
        // caller in any assembly calls that member plain and never awaits a completed task.
        internal bool TwinBinds(IMethodSymbol twin) =>
            TwinVisible(twin) && !twin.GetAttributes().Any(attribute =>
                attribute.AttributeClass is { Name: "GoParkForwarderAttribute", ContainingNamespace: { Name: "golib", ContainingNamespace.Name: "go" } });

        // 7a (12.1, R1): the population is every PUBLIC method, its every containing type public, declared in a converted
        // file (Common's predicate) of an assembly that is not hand-owned (the caller checks that).
        private static bool InPopulation(Candidate candidate)
        {
            if (!Common.IsConvertedSourceFile(candidate.Syntax.SyntaxTree))
                return false;

            for (ISymbol? member = candidate.Symbol; member is not null and not INamespaceSymbol; member = member.ContainingType)
            {
                if (member.DeclaredAccessibility != Accessibility.Public)
                    return false;
            }

            return true;
        }

        // E1, one named rule (12.1): a generic method, or a method of a generic type. Excluded at the freeze; the lift is
        // owed and additive. (A member of a generic interface is the interface side's `generic-interface`.)
        private static bool IsE1(IMethodSymbol symbol)
        {
            if (symbol.IsGenericMethod)
                return true;

            for (INamedTypeSymbol? type = symbol.ContainingType; type is not null; type = type.ContainingType)
            {
                if (type.IsGenericType)
                    return true;
            }

            return false;
        }

        // Why a member of the population with no state-machine twin gets no forwarder, or null when it gets one. A
        // forwarder is written only into a partial host, for a static method whose signature it can carry; any member
        // left here is the guard's (F9) to name. `async-shaped` (a BCL or golib task result) is no forwarder's: a
        // converted file never returns one, so the guard names it (section 8's control for keying it on a name).
        private static string? NotForwardable(Candidate candidate)
        {
            IMethodSymbol symbol = candidate.Symbol;

            if (candidate.Refusal is "has-twin" or "host-not-partial" or "interface-member" or "explicit-impl" or "instance" or "ref-return" or "async-shaped")
                return candidate.Refusal;

            if (!symbol.IsStatic)
                return "instance";

            if (symbol.ReturnsByRef || symbol.ReturnsByRefReadonly || symbol.ReturnType.IsRefLikeType ||
                symbol.ReturnType.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer)
                return "result-shape";

            for (int i = 0; i < symbol.Parameters.Length; i++)
            {
                IParameterSymbol parameter = symbol.Parameters[i];

                if (IsBoxedReceiver(symbol, i) || VariadicElement(parameter) is not null)
                    continue;

                if (parameter.RefKind != RefKind.None || parameter.Type.IsRefLikeType && !IsSString(parameter.Type))
                    return "parameter-shape";
            }

            return null;
        }

        // Section 9's row per member of the population: `twin | forwarder:<reason> | E1 | GUARD:<reason>`.
        internal string Classify(Candidate candidate) =>
            candidate.Twinned ? "twin" :
            candidate.Forwarded ? (candidate.Parks ? "forwarder-unmarked:" : "forwarder:") + (candidate.Refusal ?? "no-park") :
            IsE1(candidate.Symbol) ? "E1" :
            "GUARD:" + (NotForwardable(candidate) ?? (GuardDrop.Contains(candidate.Symbol.Name) ? "forwarder-dropped" : null) ?? (Forwarders ? null : "forwarders-off") ?? candidate.Refusal ?? "unknown");

        // Async-shaped keys on the SYMBOLS (12.1, F7): a Go type that happens to be named Task is none.
        private static bool IsTaskShaped(ITypeSymbol type) =>
            type.OriginalDefinition is INamedTypeSymbol
            {
                Name: "Task" or "ValueTask", ContainingNamespace:
                { Name: "Tasks", ContainingNamespace: { Name: "Threading", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } }
            } or INamedTypeSymbol { Name: "GoTask", ContainingNamespace: { Name: "golib", ContainingNamespace: { Name: "go", ContainingNamespace.IsGlobalNamespace: true } } };

        // The plain lowerings a forwarder gives back their Go shape (section 4): a pointer receiver, `this ref T`, takes
        // its box (F3), and an sstring-pilot parameter its @string (F5), so the twin is the same whichever lowering the
        // plain member got (P8).
        internal static bool IsBoxedReceiver(IMethodSymbol symbol, int index) =>
            index == 0 && symbol.IsExtensionMethod && symbol.Parameters[0].RefKind == RefKind.Ref;

        // F4: Go's `...T`, as the converter lowers it (`params ꓸꓸꓸT`, a Span<T> alias): its element type, or null. The
        // forwarder takes `slice<T>` and spreads it back with `.ꓸꓸꓸ` (11.6).
        internal static ITypeSymbol? VariadicElement(IParameterSymbol parameter) =>
            parameter is { IsParams: true, Type: INamedTypeSymbol { Name: "Span" or "ReadOnlySpan", TypeArguments: [{ } element], ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } }
                ? element : null;

        internal static bool IsSString(ITypeSymbol type) =>
            type is INamedTypeSymbol { Name: "sstring", ContainingNamespace: { Name: "go", ContainingNamespace.IsGlobalNamespace: true } };

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
                    if (!TwinBinds(twin) || twin.IsStatic != definition.IsStatic || twin.Parameters.Length != definition.Parameters.Length ||
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

            return HasTwinMember(target) || HasBoxTwinMember(target) || m_candidates.TryGetValue(target, out Candidate? candidate) && candidate.Twinned;
        }

        internal bool ShouldAwait(Site site) =>
            site is { Active: true, Go: GoKind.None, Renameable: true, Delegate: false } &&
            (site.Interface ? site.IfaceCallable && IfaceCanPark(site.IfaceTarget) :
             site is { ViaBoxOverload: true, BoxOverload: { } overload } && !IsBoxTwinTarget(site.Target) ? CanPark(overload) :
             CanPark(site.Target) && BoxCallable(site));

        // N2: a call to a box twin is awaited only through a box -- the box overload's receiver, or the box BoxExpression
        // named (9.1); without one it stays plain, named. A box-overload call to a method with no box twin parks as at the
        // base, through the overload's own twin (ShouldAwait; closure arm N lost syscall's 199 twins to a plain call there).
        // byref-param (12.1, must-fix 2): a call to a parameter box twin is awaited only when EVERY by-ref argument names a box.
        internal bool BoxCallable(Site site) =>
            IsBoxTwinTarget(site.Target)
                ? IsParamBoxTwin(site.Target) ? site.ArgBoxes is not null : site.ViaBoxOverload || site.BoxOf is not null
                : !site.ViaBoxOverload;

        internal bool GoTwinnable(Site site) =>
            site.Active && (site.Go == GoKind.Literal && LiteralLaunches(site) || site.Go == GoKind.Group && CanPark(site.GoTarget));

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

        // The converter's go wrapper: an expression-bodied literal over ONE call whose callee is a name (or a member of
        // one) and whose arguments are names or literals. Null for any other literal; else whether an argument is by-ref.
        private static bool? WrapperCall(ParenthesizedLambdaExpressionSyntax lambda)
        {
            if (lambda.ExpressionBody is not InvocationExpressionSyntax call ||
                call.Expression is not (IdentifierNameSyntax or GenericNameSyntax or MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax }))
            {
                return null;
            }

            bool byRef = false;

            foreach (ArgumentSyntax argument in call.ArgumentList.Arguments)
            {
                if (!argument.RefKindKeyword.IsKind(SyntaxKind.None))
                {
                    byRef = true;
                    continue;
                }

                if (argument.NameColon is not null || argument.Expression is not (IdentifierNameSyntax or LiteralExpressionSyntax))
                    return null;
            }

            return byRef;
        }

        // A by-ref wrapper's thunk is followed only where every argument is what the go statement evaluated, as Go evaluates
        // them all there: each by-ref argument a `ref ᴛk.DerefOrNull()` over one of the literal's own parameters, and each
        // by-value name one of them too, or a constant or a type's member (P3's finding 3: never a captured variable the
        // goroutine reads when it runs).
        private static bool BoundThunk(ParenthesizedLambdaExpressionSyntax lambda, SemanticModel model)
        {
            if (lambda.ExpressionBody is not InvocationExpressionSyntax call)
                return false;

            HashSet<string> bound = [.. lambda.ParameterList.Parameters.Select(parameter => parameter.Identifier.ValueText)];

            return call.ArgumentList.Arguments.All(argument => argument.RefKindKeyword.IsKind(SyntaxKind.None)
                ? argument.Expression is not IdentifierNameSyntax name || bound.Contains(name.Identifier.ValueText) ||
                  model.GetSymbolInfo(name).Symbol is IFieldSymbol { IsConst: true } or ILocalSymbol { IsConst: true } or IMethodSymbol { IsStatic: true } or ITypeSymbol
                : argument.RefKindKeyword.IsKind(SyntaxKind.RefKeyword) &&
                  argument.Expression is InvocationExpressionSyntax
                  {
                      Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax box, Name.Identifier.ValueText: "DerefOrNull" },
                      ArgumentList.Arguments.Count: 0
                  } && bound.Contains(box.Identifier.ValueText));
        }

        // A go literal launches its twin; a by-ref thunk, only where its call is awaited through the boxes (12.1, Thunks).
        internal bool LiteralLaunches(Site site) => !site.RefThunk || site.Thunk is { } thunk && ShouldAwait(thunk);

        // The name a go statement left plain is reported under (GO2CS0015), or null where it launched its twin.
        private string? PlainGoReason(Site site) =>
            site.Go == GoKind.Other ? site.PairLaunch ? null : site.GoReason :
            site.Go == GoKind.Literal && !LiteralLaunches(site) ? "ref-wrapper" : null;

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
                    SeparatedSyntaxList<ParameterSyntax> parameters = lambda.ParameterList.Parameters;
                    bool shaped = plainArguments && !lambda.Modifiers.Any(SyntaxKind.AsyncKeyword) && parameters.Count == arguments.Count - 1 &&
                                  parameters.All(parameter => parameter.Modifiers.Count == 0);

                    // Typed: a go literal, its parameter types spelled out.
                    if (shaped && parameters.All(parameter => parameter.Type is not null))
                    {
                        site.Go = GoKind.Literal;
                        return site;
                    }

                    // Implicitly typed: the converter's WRAPPER of a go call with a constant or by-ref argument,
                    // `goǃ((ᴛ1, ᴛ2) => f(ᴛ1, ᴛ2, 1), a, b)`; C# infers its types from a and b. One call over the parameters,
                    // names and literals, so nothing in it spills. A by-ref argument means f takes a ref, which refuses f a
                    // twin (byref-param): the wrapper's twin would only move the block from a thread onto a carrier.
                    if (shaped && parameters.All(parameter => parameter.Type is null) && WrapperCall(lambda) is { } wrapped)
                    {
                        if (wrapped && !(BoxParam && BoundThunk(lambda, model)))
                            return GoOther(site, "ref-wrapper", true);

                        site.RefThunk = wrapped;

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
                // 7a (F11): a call to a variadic member stays plain, as before it had a slot: its arguments are the
                // expanded `params` form, not the slot's slice. Named limit; the slot exists, so the surface is complete.
                site.IfaceCallable = !method.IsStatic && !method.Parameters.Any(parameter => VariadicElement(parameter) is not null) &&
                    node.Expression is MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.SimpleMemberAccessExpression, Name: IdentifierNameSyntax } access &&
                    model.GetTypeInfo(access.Expression).Type?.TypeKind == TypeKind.Interface;
            }
            else if (method.MethodKind != MethodKind.LocalFunction)
            {
                site.Target = Normalize(method);

                // N2: a call through a ж<T> expression binds RecvGenerator's box overload; its target is the `this ref`
                // method behind it, and its receiver already is the box (no copy, no row of 9.1 needed).
                if (BoxOverloadTarget(site.Target, out IMethodSymbol? genericUnfollowed) is { } boxed)
                {
                    NoteViaOverload(node, site.Target, boxed);
                    site.BoxOverload = site.Target;
                    site.Target = boxed;
                    site.ViaBoxOverload = true;
                }
                else
                {
                    site.BoxGenericUnfollowed = genericUnfollowed;
                }
            }

            return site;
        }
    }

    // Records every call of a candidate: active in the function's own body and in the bodies of its go
    // literals, passive inside any other closure, where nothing is awaited.
    private sealed class Collector(Run run, SemanticModel model, Candidate candidate) : CSharpSyntaxWalker
    {
        private int m_passive, m_literal, m_pair;
        private RefAliases? m_aliases;

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            Site site = run.Classify(node, model);
            site.Active = m_passive == 0;
            site.Direct = m_passive == 0 && m_literal == 0;
            site.PairBody = m_pair > 0;
            site.Node = node;

            // N2: the box a `this ref` call can name for its receiver, whatever the target becomes after the fixpoint.
            if (site is { ViaBoxOverload: false, Target: { IsExtensionMethod: true, Parameters.Length: > 0 } target } && target.Parameters[0].RefKind == RefKind.Ref)
            {
                site.BoxOf = run.BoxExpression(node, model, candidate, m_aliases ??= new RefAliases(candidate.Syntax));

                // PB-5 (12.5): a receiver local refused promotion names its reason in the call's `// P` row.
                if (site.BoxOf is null && node.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax receiverName } &&
                    candidate.PromoteRefused.TryGetValue(receiverName.Identifier.ValueText, out string? refusal))
                {
                    site.ArgPlain = receiverName.Identifier.ValueText + " (" + refusal + ")";
                }
            }

            // byref-param: the boxes a call to a function with plain `ref` parameters can hand (12.1), whatever the target
            // becomes after the fixpoint.
            if (run.BoxParam && site is { ViaBoxOverload: false, Target: { IsExtensionMethod: false } callee } && callee.Parameters.Any(parameter => parameter.RefKind == RefKind.Ref))
            {
                site.ArgBoxes = run.ArgumentBoxes(node, callee, model, candidate, m_aliases ??= new RefAliases(candidate.Syntax), out string? plain);
                site.ArgPlain = plain;
            }

            run.Sites[node] = site;
            candidate.Sites.Add(site);

            if (site is { Go: GoKind.Literal, Active: true })
            {
                m_literal++;
                ParenthesizedLambdaExpressionSyntax literal = (ParenthesizedLambdaExpressionSyntax)node.ArgumentList.Arguments[0].Expression;
                Visit(literal.Body);
                m_literal--;

                if (site.RefThunk && literal.ExpressionBody is InvocationExpressionSyntax thunk)
                    site.Thunk = run.Sites.TryGetValue(thunk, out Site? called) ? called : null;

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

// TwinGenerator.ParkCuts.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace go2cs;

// M:N scheduler, experimental and opt-in (GoTwins=true to build, GO2CS_MN=1 to run); inert otherwise.
// N1 (docs/phase4/DESIGN-mn-nopark.md): edges that are not parks for the generator's purposes. Two forms, one meaning.
//
// W, a WEAK member: its twin may park, but a call to it never makes its caller a twin. A caller that is a twin for
// another reason awaits it. The callee's twin carries [GoParkWeak], which a caller in any assembly reads from metadata,
// as it reads [GoParkForwarder]. Locks are the case: sync's Mutex.Lockꞏtwin carries the mark by hand (sync is hand-owned,
// and no generator runs over it). RWMutex has no twin, so a call to it is no park to weaken.
//
// C, a CUT site: a named (caller, callee) call is not a park at all, and stays plain even inside a twin. Nothing crosses
// assemblies: the caller's own build decides, and its members' marks say the rest. fmt's method hooks and rune reader
// are the case.
//
// Neither form changes the twin set's population (R1): every public member still gets its twin or its forwarder. So
// GoTwinsWeak and GoTwinsCut are not arm switches, and O2 (GO2CS0018) does not refuse them.
public sealed partial class TwinGenerator
{
    // The standard library's entries (section 3.1 of the design): one table, reviewed like code, versioned with the
    // generator that reads it. An entry binds only in the build of the package that declares its package class, and the
    // keys are MemberKey's: `<receiver type or package class>.<member>`. Every entry here is checked for staleness in
    // that package's build (GO2CS0019), and the GenTests row reads the warning as an error, so the table cannot rot.
    internal static readonly (string Package, string Caller, string Callee)[] StdlibCuts =
    [
        // (*pp).handleMethods calls the operand's methods through an interface; a String somewhere that prints to an
        // *os.File would otherwise mark every formatter.
        ("go.fmt_package", "go.fmt_package.pp.handleMethods", "go.fmt_package.Formatter.Format"),
        ("go.fmt_package", "go.fmt_package.pp.handleMethods", "go.fmt_package.GoStringer.GoString"),
        ("go.fmt_package", "go.fmt_package.pp.handleMethods", "go.error.Error"),
        ("go.fmt_package", "go.fmt_package.pp.handleMethods", "go.fmt_package.Stringer.String"),

        // The rune reader: (*ss).ReadRune reads its io.RuneScanner, and (*readRune).readByte calls io.ReadFull. NOT cut:
        // io.ReadAtLeast's Reader.Read, which would make every io.ReadFull in every program plain, not only fmt's.
        ("go.fmt_package", "go.fmt_package.ss.ReadRune", "go.io_package.RuneReader.ReadRune"),
        ("go.fmt_package", "go.fmt_package.readRune.readByte", "go.io_package.ReadFull")
    ];

    // Weak members whose twin the GENERATOR writes, by package class. sync's Mutex.Lockꞏtwin is hand-owned and carries
    // [GoParkWeak] in its own source (sync/mutex.cs).
    internal static readonly (string Package, string Member)[] StdlibWeak = [];

    internal sealed partial class Run
    {
        // GoTwinsWeak and GoTwinsCut (MSBuild properties, ';' or ',' between entries), read in Execute.
        internal string? WeakProperty { get; init; }
        internal string? CutProperty { get; init; }

        // This build's weak members and cut sites, with where each entry came from, for GO2CS0019.
        private readonly Dictionary<string, string> m_weakEntries = new(StringComparer.Ordinal);
        private readonly Dictionary<(string Caller, string Callee), string> m_cutEntries = [];
        private readonly HashSet<(string Caller, string Callee)> m_cutHits = [];

        internal int WeakSites, CutSites;

        // `<receiver type>.<member>` for a Go method (an extension method's receiver, through a box), else
        // `<containing type>.<member>`: the key GoTwinsWeak, GoTwinsCut and the stdlib table are written in.
        internal static string MemberKey(IMethodSymbol method)
        {
            IMethodSymbol definition = Normalize(method);
            ITypeSymbol owner = definition.ContainingType;

            if (definition.IsExtensionMethod && definition.Parameters.Length > 0)
            {
                ITypeSymbol receiver = definition.Parameters[0].Type;
                owner = receiver is INamedTypeSymbol { Name: "ж", TypeArguments.Length: 1 } box ? box.TypeArguments[0] : receiver;
            }

            return owner.OriginalDefinition.ToDisplayString() + "." + definition.Name;
        }

        private static IEnumerable<string> Entries(string? property) =>
            property is null ? [] : property.Split([';', ','], StringSplitOptions.RemoveEmptyEntries).Select(entry => entry.Trim()).Where(entry => entry.Length > 0);

        private bool DeclaresPackage(string packageClass) =>
            m_augmented.Assembly.GetTypeByMetadataName(packageClass) is not null;

        // Fills the two sets: the stdlib table's rows for this package, then the properties.
        private void ReadParkCuts()
        {
            foreach ((string package, string member) in StdlibWeak)
            {
                if (DeclaresPackage(package))
                    m_weakEntries[member] = "stdlib";
            }

            foreach ((string package, string caller, string callee) in StdlibCuts)
            {
                if (DeclaresPackage(package))
                    m_cutEntries[(caller, callee)] = "stdlib";
            }

            foreach (string member in Entries(WeakProperty))
                m_weakEntries[member] = "GoTwinsWeak";

            foreach (string entry in Entries(CutProperty))
            {
                int arrow = entry.IndexOf("->", StringComparison.Ordinal);

                if (arrow <= 0)
                {
                    Context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.StaleParkEntry, Location.None, entry, "GoTwinsCut", "not a Caller->Callee pair"));
                    continue;
                }

                m_cutEntries[(entry.Substring(0, arrow).Trim(), entry.Substring(arrow + 2).Trim())] = "GoTwinsCut";
            }
        }

        // A candidate of THIS assembly that a weak entry names.
        private bool WeakListed(IMethodSymbol symbol) =>
            m_weakEntries.Count > 0 && m_weakEntries.ContainsKey(MemberKey(symbol));

        // W: the call's target is weak, by its twin's [GoParkWeak] in metadata (any assembly) or by this build's list. A
        // box-overload call is weak by the overload's twin (sync's `Lockꞏtwin(this ж<Mutex>)` is that twin).
        internal bool WeakTarget(Site site) =>
            !site.Interface && (IsWeak(site.Target) || site is { ViaBoxOverload: true, BoxOverload: { } overload } && IsWeak(overload));

        private readonly Dictionary<ISymbol, bool> m_weak = new(SymbolEqualityComparer.Default);

        internal bool IsWeak(IMethodSymbol? target)
        {
            if (target is null)
                return false;

            IMethodSymbol definition = Normalize(target);

            if (m_weak.TryGetValue(definition, out bool weak))
                return weak;

            weak = m_candidates.TryGetValue(definition, out Candidate? candidate) ? candidate.Weak :
                definition.ContainingType is { } type && type.GetMembers(definition.Name + Suffix).OfType<IMethodSymbol>().Any(twin =>
                    TwinBinds(twin) && twin.Parameters.Length == definition.Parameters.Length && twin.IsStatic == definition.IsStatic && CarriesWeak(twin));

            return m_weak[definition] = weak;
        }

        internal static bool CarriesWeak(IMethodSymbol twin) =>
            twin.GetAttributes().Any(attribute =>
                attribute.AttributeClass is { Name: "GoParkWeakAttribute", ContainingNamespace: { Name: "golib", ContainingNamespace.Name: "go" } });

        // C: the site is a named cut of its caller. Set once per site, after the collector (MarkCuts).
        private void MarkCuts()
        {
            if (m_cutEntries.Count == 0)
                return;

            foreach (Candidate candidate in m_ordered)
            {
                string caller = MemberKey(candidate.Symbol);

                foreach (Site site in candidate.Sites)
                {
                    IMethodSymbol? callee = site.Interface ? site.IfaceTarget : site.Target;

                    if (callee is null || !m_cutEntries.ContainsKey((caller, MemberKey(callee))))
                        continue;

                    site.Cut = true;
                    m_cutHits.Add((caller, MemberKey(callee)));
                }
            }
        }

        // After the fixpoint: the report's counts, and GO2CS0019 for every entry that does nothing, on its four arms.
        private void CheckParkCuts()
        {
            HashSet<string> members = [];
            HashSet<(string, string)> live = [];

            foreach (Candidate candidate in m_ordered)
            {
                string caller = MemberKey(candidate.Symbol);
                members.Add(caller);

                foreach (Site site in candidate.Sites)
                {
                    if (!site.Direct || !WouldAwait(site))
                        continue;

                    if (site.Cut)
                    {
                        CutSites++;
                        live.Add((caller, MemberKey(site.Interface ? site.IfaceTarget! : site.Target!)));
                    }
                    else if (WeakTarget(site))
                    {
                        WeakSites++;
                    }
                }
            }

            foreach (KeyValuePair<string, string> entry in m_weakEntries)
            {
                Candidate? candidate = m_ordered.FirstOrDefault(c => MemberKey(c.Symbol) == entry.Key);

                if (candidate is null)
                    Report(entry.Key, entry.Value, "names no member of this assembly");
                else if (!candidate.Parks)
                    Report(entry.Key, entry.Value, "dead: the member's own twin cannot park, so it carries [GoParkForwarder] and no weak mark");
            }

            foreach (KeyValuePair<(string Caller, string Callee), string> entry in m_cutEntries)
            {
                string name = entry.Key.Caller + "->" + entry.Key.Callee;

                if (!members.Contains(entry.Key.Caller))
                    Report(name, entry.Value, "names no member of this assembly");
                else if (!m_cutHits.Contains(entry.Key))
                    Report(name, entry.Value, "names no such call site");
                else if (!live.Contains(entry.Key))
                    Report(name, entry.Value, "dead: the callee no longer parks, so the cut does nothing");
            }
        }

        private void Report(string entry, string source, string arm) =>
            Context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.StaleParkEntry, Location.None, entry, source, arm));
    }
}

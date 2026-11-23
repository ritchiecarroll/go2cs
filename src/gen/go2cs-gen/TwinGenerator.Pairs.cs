// TwinGenerator.Pairs.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace go2cs;

// M:N (experimental), N3's D3 core (docs/phase4/DESIGN-mn-func-values.md sections 4, 9-11): func-value PAIRS. A can-park
// literal created in a twin body becomes a pair of its plain and twin lambdas, handed out as its own delegate type bound
// to the pair's Invoke; a call through a func value in an EXISTING twin tests its target for a pair and awaits the
// twin.
public sealed partial class TwinGenerator
{
    internal sealed partial class Run
    {
        // N3's CALLER RULE (the owner's ruling, 2026-11-14 via COORD: PROPAGATING, sections 10.2-10.3): a function whose
        // body calls through a func value that a twin can test for a pair is twinned for that call alone. It propagates
        // through CanPark: a twinned function's callers park on it, so they are twinned too. The test is the rewriter's
        // own (PairShape), so every function twinned by this rule has a call that takes the pair test -- HOISTED to the
        // twin's entry for a parameter never reassigned. Off with the pair core (GoTwinsPairs=false), the A/B control.
        internal bool CallerRuleTwins(Site site) => Pairs && CallerRule && site.PairCallable;

        // GoTwinsCallerRule (default on): the caller rule alone; off keeps the pair core, the rule's own A/B control.
        internal bool CallerRule { get; init; } = true;

        // GoTwinsPairs (default on): the D3 core itself. Off, every piece below is skipped and the emission is the one
        // before N3's core: the A/B control.
        internal bool Pairs { get; init; } = true;

        private readonly Dictionary<string, string> m_pairClassOf = new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, string> m_staticPairOf = new(System.StringComparer.Ordinal);

        // A static function's pair is made ONCE per (class, function, delegate type): the name of the cache field in the
        // function's own class, and whether this request is the first (whose twin then declares the field).
        internal (string Name, bool IsNew) StaticPairCache(INamedTypeSymbol owner, IMethodSymbol function, INamedTypeSymbol delegateType)
        {
            string key = owner.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + "|" + function.ToDisplayString() + "|" +
                         delegateType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            if (m_staticPairOf.TryGetValue(key, out string? name))
                return (name, false);

            name = "ꞏpair" + m_staticPairOf.Count;
            m_staticPairOf[key] = name;
            return (name, true);
        }
        private readonly StringBuilder m_pairClasses = new();
        internal int PairsMade, PairTests;

        // The canonical twin delegate of a func value's Go signature (fix 4 of 9.3, accepted): the delegate's Invoke
        // signature with its result wrapped in GoTask, so a named Go func type and its structural spelling agree. Null
        // when the signature cannot carry a twin (a ref, out or in parameter, a ref-like or open generic type).
        internal static string? TwinDelegateType(INamedTypeSymbol delegateType)
        {
            if (delegateType.DelegateInvokeMethod is not { } invoke || invoke.ReturnsByRef || invoke.ReturnsByRefReadonly ||
                invoke.Parameters.Length > 16 || ContainsTypeParameter(delegateType))
            {
                return null;
            }

            List<string> types = [];

            foreach (IParameterSymbol parameter in invoke.Parameters)
            {
                if (parameter.RefKind != RefKind.None || parameter.Type.IsRefLikeType)
                    return null;

                types.Add(parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }

            if (!invoke.ReturnsVoid && invoke.ReturnType.IsRefLikeType)
                return null;

            types.Add(invoke.ReturnsVoid ? "global::go.golib.GoTask" : "global::go.golib.GoTask<" + invoke.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ">");

            return "global::System.Func<" + string.Join(", ", types) + ">";
        }

        // The pair class for a delegate type, emitted once per type into go2cs.twinpairs.g.cs.
        internal string PairClass(INamedTypeSymbol delegateType, string twinType)
        {
            string plainType = delegateType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            if (m_pairClassOf.TryGetValue(plainType, out string? name))
                return name;

            name = "global::go2cs.TwinPairs.ꞏFuncPair" + m_pairClassOf.Count;
            m_pairClassOf[plainType] = name;

            IMethodSymbol invoke = delegateType.DelegateInvokeMethod!;
            string parameters = string.Join(", ", invoke.Parameters.Select((p, i) => p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + " a" + i));
            string arguments = string.Join(", ", invoke.Parameters.Select((_, i) => "a" + i));
            string result = invoke.ReturnsVoid ? "void" : invoke.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            m_pairClasses.Append("    internal sealed class ꞏFuncPair").Append(m_pairClassOf.Count - 1).Append('(').Append(plainType).Append(" plain, ").Append(twinType)
                .Append(" twin) : global::go.golib.GoFuncPair\r\n    {\r\n")
                .Append("        public override global::System.Delegate Plain => plain;\r\n")
                .Append("        public override global::System.Delegate Twin => twin;\r\n")
                .Append("        public ").Append(result).Append(" Invoke(").Append(parameters).Append(") => plain(").Append(arguments).Append(");\r\n")
                .Append("    }\r\n");

            return name;
        }

        private void EmitPairs()
        {
            if (m_pairClasses.Length == 0)
                return;

            Context.AddLineExactSource("go2cs.twinpairs.g.cs",
                "// <auto-generated/>\r\n#nullable enable annotations\r\n#pragma warning disable\r\nnamespace go2cs.TwinPairs\r\n{\r\n" + m_pairClasses + "}\r\n");
        }

        private static bool ContainsTypeParameter(ITypeSymbol type) => type switch
        {
            ITypeParameterSymbol => true,
            INamedTypeSymbol named => named.TypeArguments.Any(ContainsTypeParameter) || named.ContainingType is { } outer && ContainsTypeParameter(outer),
            IArrayTypeSymbol array => ContainsTypeParameter(array.ElementType),
            IPointerTypeSymbol pointer => ContainsTypeParameter(pointer.PointedAtType),
            _ => false
        };
    }
}

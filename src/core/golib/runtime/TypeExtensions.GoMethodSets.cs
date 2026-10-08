// TypeExtensions.GoMethodSets.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using static go2cs.Symbols;

#pragma warning disable IL2075
#pragma warning disable IL2067
#pragma warning disable IL2055
#pragma warning disable IL2060
#pragma warning disable IL2080
#pragma warning disable IL2070
#pragma warning disable IL2026

namespace go.golib;

// ---------------------------------------------------------------------------------------------
// GO METHOD SETS — Go's duck typing, decided at run time.
//
// WHAT LIVES HERE
//   The answer to "does this VALUE satisfy this interface?" when nothing declared that it does:
//   Go's value-vs-pointer receiver rule, the structural (name AND signature) satisfaction probe
//   built on it, and the interface-side and struct-side name projections it needs.
//
// WHY IT EXISTS
//   Go interfaces are satisfied STRUCTURALLY — a type implements an interface by having the
//   methods, never by saying so. C# interfaces are nominal, so a converted type can only declare
//   the interfaces the converter could SEE. Anything it could not — an anonymous interface in a
//   type assertion (`x.(interface{ Temporary() bool })`), an interface declared in a package that
//   does not import the type's package — has to be decided here, at the moment of the assert.
//
//   Live consumers: `AdapterBinder` (the duck-typing shell binder), `GoReflect.GoImplements` (so
//   reflection and direct asserts can never disagree about a method set), the `GoReflect` method-
//   TABLE surface (reflect.Type.NumMethod/Method(i)/MethodByName and Value.Method(i) — the same
//   non-disagreement rule, applied to the method set's SIZE and ORDER), and `builtin`'s per-
//   interface assert cache.
//
// THE COUNT AND THE ENUMERATION ARE ONE LIST, NOT TWO DERIVATIONS
//   GetGoMethodSetEntries builds the ordered, deduplicated, exported-only table ONCE, and
//   GoMethodSetCount is its `.Count`. That is deliberate and was learned the expensive way: the
//   count shipped one increment ahead of the enumeration, and `reflect.Type.NumMethod` answering
//   16 while `Type.Method(i)` still read the absent uncommon() tables turned a silent under-count
//   into a loud `panic: reflect: Method index out of range` in every consumer that WALKS a method
//   set (math/rand and math/rand/v2's TestRegress). A size and an order derived separately are
//   free to disagree; one list cannot.
//
// THE RULE THIS FILE EXISTS TO GET RIGHT
//   In Go, the method set of `*T` contains T's value-receiver AND pointer-receiver methods; the
//   method set of `T` contains only the value-receiver ones. GetGoMethodSetCandidates is the ONE
//   place that rule is applied at run time, and both the probe and the shell binder resolve
//   through it deliberately — if they each applied it themselves they would be free to disagree,
//   and a value-sourced shell that picked up a pointer-receiver method would make an assertion Go
//   REJECTS succeed.
//
//   A pointer receiver reaches this file in TWO emitted shapes — the RecvGenerator's `ж<X>`
//   overload and the original `[GoRecv] this ref X` extension — and the byref strip that
//   normalizes receiver types erases the second one's pointer-ness. That is why the filter also
//   asks the `[GoRecv]` marker directly.
//
// COUNTEREXAMPLES — WHAT A CHEAPER TEST GETS WRONG
//   Both are the reason StructurallyImplements is shaped the way it is, and neither is
//   hypothetical:
//     * Matching by NAME alone conflates methods whose signatures differ. `Unwrap() error` and
//       `Unwrap() []error` are different Go methods; a value implementing one satisfied the
//       interface for the other, and the generated adapter then failed to bind a delegate
//       (NotImplementedException at the call, far from the bad match).
//     * Asking GetExtensionMethodNames about a pointer value admits EVERY type's pointer-receiver
//       methods, because that lookup collapses a closed `ж<X>` to the open `ж<>` definition. That
//       collapse is correct for the registry's precedence-ranked single dispatch and wrong for a
//       membership test — same data, different question.
//   The one place a name-only match survives is an open-generic receiver method, whose signature
//   carries type parameters that cannot be compared against the interface's concrete one. Both
//   defects above were on closed types, so the narrow exemption keeps them fixed.
//
// THE CANDIDATE'S EMITTED NAME IS NOT ALWAYS THE GO METHOD NAME
//   A Go method set is a GO-level fact reconstructed here from EMITTED C#, and the emitted name can
//   carry the converter's collision-avoidance marker instead of the Go name. A `-tests` variant
//   Δ-renames a test-file method declarator whose bare name would hijack a same-named dot-imported
//   function at every unqualified call site (io's `func (c *writeStringChecker) WriteString` against
//   the dot-imported `io.WriteString`, B9 in the converter's name-collision analysis) — so the
//   candidate carrying the Go method `WriteString` is emitted `ΔWriteString`, while the interface
//   member it must satisfy keeps the bare name. Comparing the emitted names answered MISS, and the
//   miss is SILENT: `io.MultiWriter`'s `w.(StringWriter)` fell through to `Write`, which returns the
//   same (n, err), so only Go's own assertion that WriteString was CALLED could see it.
//
//   GoMethodNameMatches projects a candidate's leading marker away, the same way GoReflect's type
//   naming and struct-field projection recover a Go name from an emitted one. It runs as a SECOND
//   pass, after an exact-name pass finds nothing, so a Δ-renamed candidate can never displace a
//   plainly-named one; both this file's probe and AdapterBinder's shell binder resolve through it,
//   for the same reason they share GetGoMethodSetCandidates.
//
// THE BindingFlags ARE LOAD-BEARING, NOT DECORATION
//   GetInterfaceMethods passes `BindingFlags.Public | BindingFlags.Instance` explicitly, and the
//   base-interface loop MUST pass the same. Reflection's DEFAULT flags include STATICS, and
//   golib's hand-written core interfaces expose static duck-typing helpers (`error.As<T>`,
//   `fmt.Stringer.As<T>`) that are not part of any Go method set. Collect those and the probe
//   demands a static `As` from the value's Go method set — which no Go type can ever satisfy — so
//   every interface that EMBEDS such a base answers FALSE for everything. Measured when it
//   happened: an anonymous `interface{ error; Temporary() bool }` (net.Error's shape) asserted
//   against a `*tempErr` that plainly has both methods answered MISS.
//
// CACHES ARE CLEARED FROM THE SIBLING FILE
//   s_interfaceMethodNames, s_goMethodSetCandidates, s_goMethodSetEntries and
//   s_goInterfaceMethodEntries are emptied by `ClearTypeCaches` in
//   TypeExtensions.ExtensionMethodRegistry.cs on every assembly load, because their contents are
//   derived from the extension-method scan and a late-loading package assembly changes the answer.
//   s_structFieldNames is NOT cleared, and correctly so — a type's own fields are fixed when the
//   type is loaded. A new cache here belongs on that clear list only if the scan feeds it.
// ---------------------------------------------------------------------------------------------
/// <summary>
/// One method of a Go method set: the projected GO method name, and the emitted receiver method
/// that carries it (its first parameter is the receiver).
/// </summary>
internal readonly record struct GoMethodSetEntry(string GoName, MethodInfo Method);

public static partial class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, ImmutableHashSet<string>> s_interfaceMethodNames = [];
    private static readonly ConcurrentDictionary<(Type element, bool isPointer), List<MethodInfo>> s_goMethodSetCandidates = [];
    private static readonly ConcurrentDictionary<Assembly, bool> s_handOwnedPackages = [];
    private static readonly ConcurrentDictionary<(Type element, bool isPointer), GoMethodSetEntry[]> s_goMethodSetEntries = [];
    private static readonly ConcurrentDictionary<Type, GoMethodSetEntry[]> s_goInterfaceMethodEntries = [];
    private static readonly ConcurrentDictionary<Type, ImmutableHashSet<string>> s_structFieldNames = [];

    /// <summary>
    /// Determines, by Go DUCK-TYPING (structural) rules, whether the method set of
    /// <paramref name="valueType"/> satisfies every method of <paramref name="interfaceType"/> —
    /// matching each interface method by NAME <em>and</em> full SIGNATURE (parameter and return types).
    /// </summary>
    /// <param name="valueType">
    /// Concrete runtime type of the value being asserted. A pointer value is the box <c>ж&lt;X&gt;</c>.
    /// </param>
    /// <param name="interfaceType">Anonymous or named interface the value is asserted against.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="valueType"/> structurally implements <paramref name="interfaceType"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This is the runtime gate for duck-typed type assertions (<c>x.(interface{ … })</c>). It replaces a
    /// prior NAME-ONLY test that had two defects:
    /// <list type="number">
    /// <item>It conflated same-named methods whose signatures differ — <c>Unwrap() error</c> versus
    /// <c>Unwrap() []error</c> — so a value implementing one matched the interface for the other and the
    /// generated duck-typed adapter then failed to bind a delegate (a <see cref="NotImplementedException"/>).</item>
    /// <item>For a pointer value <c>ж&lt;X&gt;</c> it admitted the pointer-receiver methods of <em>every</em>
    /// type, because <see cref="GetExtensionMethodNames"/> collapses a closed <c>ж&lt;X&gt;</c> to the open
    /// <c>ж&lt;&gt;</c> generic definition (correct for MinBy-precedence single dispatch, wrong for a
    /// name-set membership test).</item>
    /// </list>
    /// Both are corrected here: candidates are restricted to the extension methods whose receiver actually
    /// belongs to <paramref name="valueType"/>'s Go method set (a pointer box <c>ж&lt;X&gt;</c> exposes the
    /// value- and pointer-receiver methods of <c>X</c>; a plain value <c>X</c> exposes only its
    /// value-receiver methods), and each candidate is compared by full signature. Open-generic receiver
    /// methods keep the prior name-only match, since their signatures carry type parameters that cannot be
    /// compared structurally against the interface's concrete signature (the defects being fixed are all on
    /// closed types).
    /// </remarks>
    public static bool StructurallyImplements(this Type valueType, Type interfaceType)
    {
        MethodInfo[] interfaceMethods = [.. interfaceType.GetInterfaceMethods()];

        // All types implement an empty interface.
        if (interfaceMethods.Length == 0)
            return true;

        // Resolve the value's Go method-set receiver element: a pointer box ж<X> exposes BOTH the value-
        // and pointer-receiver methods of X; a plain value X exposes only its value-receiver methods.
        ResolveReceiverElement(valueType, out Type valueElement, out bool valueIsPointer);

        // Collect the extension methods whose receiver belongs to this value's method set.
        List<MethodInfo> candidates = GetGoMethodSetCandidates(valueElement, valueIsPointer);

        // Every interface method must be satisfied by a candidate with the same name AND signature.
        // The EXACT emitted name is a full first pass; only when it satisfies nothing does the Go-name
        // projection run, so a Δ-renamed candidate can never displace a plainly-named one.
        foreach (MethodInfo interfaceMethod in interfaceMethods)
        {
            if (!Satisfies(interfaceMethod, candidates, false) && !Satisfies(interfaceMethod, candidates, true))
                return false;
        }

        return true;
    }

    // Determines whether any candidate satisfies one interface method, comparing names either exactly
    // or against the candidate's Go-name projection (see GoMethodNameMatches).
    private static bool Satisfies(MethodInfo interfaceMethod, List<MethodInfo> candidates, bool projectGoName)
    {
        foreach (MethodInfo candidate in candidates)
        {
            if (!GoMethodNameMatches(candidate, interfaceMethod.Name, projectGoName))
                continue;

            if (!UnexportedMethodPackageMatches(interfaceMethod, candidate))
                continue;

            // An open-generic receiver method (a method generic over its receiver's type parameter)
            // carries type parameters in its signature that cannot be compared structurally against the
            // interface's concrete signature — keep the prior name-only match for those.
            if (candidate.IsGenericMethodDefinition || candidate.GetParameters()[0].ParameterType.ContainsGenericParameters)
                return true;

            if (SignaturesMatch(interfaceMethod, candidate))
                return true;
        }

        return false;
    }

    // Go's UNEXPORTED-method rule: two same-named unexported methods from DIFFERENT packages are
    // DIFFERENT methods, so an interface carrying one (go/ast's `Expr { exprNode() }` marker
    // pattern) is satisfiable only by a method declared in the INTERFACE's own package. A
    // name+signature probe cannot see that — reflectlite's TestImplements measured
    // `*reflectlite_test.notAnExpr` "implementing" ast.Expr through its own private exprNode —
    // and a false positive in an implements relation is read by every caller as permission.
    //
    // The package identities are the managed nesting's: the interface method's declaring
    // interface sits in its `<pkg>_package` class, and a candidate extension method's declaring
    // type IS its package class (GoReflect.GoPackageClassPath; a `-tests` whitebox bridge class
    // carries the production package's [GoPackage] stamp, so an internal-test method rightly
    // counts as the production package's). An exported method carries no constraint; a
    // hand-written golib interface has no Go package identity and keeps the pure structural
    // answer. The stated residual: a PROMOTED method's candidate is the generator's wrapper in
    // the EMBEDDING package, so a cross-package embed satisfying a foreign marker interface
    // answers false — the conservative direction, with no measured consumer.
    private static bool UnexportedMethodPackageMatches(MethodInfo interfaceMethod, MethodInfo candidate)
    {
        string goName = interfaceMethod.Name;

        if (goName.StartsWith(ShadowVarMarker, StringComparison.Ordinal))
            goName = goName[ShadowVarMarker.Length..];

        if (goName.Length == 0 || char.IsUpper(goName[0]))
            return true;

        string interfacePackage = GoReflect.GoPackageClassPath(interfaceMethod.DeclaringType?.DeclaringType);

        if (interfacePackage.Length == 0)
            return true;

        return string.Equals(interfacePackage, GoReflect.GoPackageClassPath(candidate.DeclaringType), StringComparison.Ordinal);
    }

    /// <summary>
    /// Matches an emitted extension method's name against the Go method name
    /// <paramref name="goMethodName"/> an interface asks for.
    /// </summary>
    /// <param name="candidate">Candidate receiver method from a Go method set.</param>
    /// <param name="goMethodName">Go method name the interface member carries.</param>
    /// <param name="projectGoName">
    /// <c>false</c> to compare the emitted name as-is; <c>true</c> to compare the candidate's Go-name
    /// PROJECTION — its emitted name with a leading collision-avoidance marker removed.
    /// </param>
    /// <returns><c>true</c> when the candidate carries that Go method; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The projection exists because a `-tests` variant Δ-renames a test-file method declarator whose
    /// bare emitted name would hijack a same-named dot-imported function (see this file's header), and
    /// a Go method set must be read in GO names. Callers run it as a SECOND pass so the exact name
    /// always wins; the marker strip mirrors <c>GoReflect</c>'s type-name and struct-field projections.
    /// </remarks>
    internal static bool GoMethodNameMatches(MethodInfo candidate, string goMethodName, bool projectGoName)
    {
        string name = candidate.Name;

        if (!projectGoName)
            return string.Equals(name, goMethodName, StringComparison.Ordinal);

        return name.Length > ShadowVarMarker.Length &&
               name.StartsWith(ShadowVarMarker, StringComparison.Ordinal) &&
               string.Equals(name[ShadowVarMarker.Length..], goMethodName, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether <paramref name="method"/> takes its receiver <c>this ref X</c> and is marked
    /// <see cref="GoCopyBoundAttribute"/> — a method of <c>X</c>'s VALUE method set whose emitted receiver
    /// is by reference, which every run-time binder must therefore call on a COPY.
    /// </summary>
    /// <param name="method">Receiver-first static method from a Go method set.</param>
    /// <param name="receiverType">
    /// The receiver type Go sees: the by-ref's element type when the method is copy-bound; the
    /// declared parameter type, unchanged, otherwise.
    /// </param>
    /// <returns><c>true</c> when the receiver must be bound through a copy; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The shape is the generated forwarder of a POINTER-receiver method promoted through an embedded
    /// POINTER: Go puts that method in the outer type's value set (the embedded pointer is what the
    /// method mutates through), while the forwarder mirrors its source's <c>this ref</c>. A by-ref
    /// receiver fits no delegate and no <c>Func&lt;&gt;</c> type argument, so reflect's method table,
    /// method values and the interface shells all refused it. Copying is exactly Go's semantics for a
    /// value receiver, and nothing is lost by it: the forwarder reaches its target through the
    /// embedded pointer, which the copy shares. The generator marks that forwarder
    /// <see cref="GoCopyBoundAttribute"/>; every OTHER by-ref receiver is a pointer-set method, bound
    /// through its <c>ж&lt;X&gt;</c> twin and never through a copy, whether or not it carries
    /// <c>[GoRecv]</c> (converted code no longer writes it; hand-written files keep it). Inside a
    /// <see cref="GoHandOwnedPackageAttribute"/> package a method is a pointer receiver only by
    /// <c>[GoRecv]</c>, so there an unmarked by-ref receiver is copy-bound too, and RecvGenerator mints
    /// it no twin.
    /// </remarks>
    internal static bool IsCopyBoundReceiver(MethodInfo method, out Type receiverType)
    {
        ParameterInfo[] parameters = method.GetParameters();

        if (parameters.Length == 0)
        {
            receiverType = typeof(void);
            return false;
        }

        receiverType = parameters[0].ParameterType;

        if (!receiverType.IsByRef)
            return false;

        if (method.GetCustomAttribute<GoCopyBoundAttribute>() is null &&
            !(IsHandOwnedPackage(method) && method.GetCustomAttribute<GoRecvAttribute>() is null))
            return false;

        receiverType = receiverType.GetElementType()!;
        return true;
    }

    /// <summary>
    /// Determines whether <paramref name="method"/>, whose first parameter is <paramref name="receiver"/>,
    /// is a POINTER-receiver method that a by-reference receiver alone does not show once the byref is
    /// stripped: a by-ref receiver not marked <see cref="GoCopyBoundAttribute"/> (outside a
    /// <see cref="GoHandOwnedPackageAttribute"/> package), or any method still marked
    /// <see cref="GoRecvAttribute"/>.
    /// </summary>
    /// <param name="method">Receiver-first static method from a Go method set.</param>
    /// <param name="receiver">The method's first parameter type, byref included.</param>
    /// <returns><c>true</c> when the method belongs to the pointer method set only.</returns>
    internal static bool IsPointerSetByRefReceiver(MethodInfo method, Type receiver) =>
        method.GetCustomAttribute<GoRecvAttribute>() is not null ||
        receiver.IsByRef && method.GetCustomAttribute<GoCopyBoundAttribute>() is null && !IsHandOwnedPackage(method);

    // Whether the method's assembly opts out with [assembly: GoHandOwnedPackage]: there a method is a
    // pointer receiver only by [GoRecv], as it was before converted code stopped writing it.
    private static bool IsHandOwnedPackage(MethodInfo method) =>
        s_handOwnedPackages.GetOrAdd(method.Module.Assembly, static assembly => assembly.IsDefined(typeof(GoHandOwnedPackageAttribute), false));

    /// <summary>
    /// Collects the extension methods whose receiver belongs to the Go METHOD SET of element type
    /// <paramref name="valueElement"/>, viewed as a pointer (<c>*X</c>) or as a value (<c>X</c>).
    /// </summary>
    /// <param name="valueElement">Go element type, i.e. the pointee of a receiver box or the value's own type.</param>
    /// <param name="valueIsPointer"><c>true</c> for the <c>*X</c> method set (value AND pointer receivers); <c>false</c> for <c>X</c>'s (value receivers only).</param>
    /// <returns>Candidate receiver methods, in no particular order.</returns>
    /// <remarks>
    /// This is the ONE place Go's value-vs-pointer receiver rule is applied at run time. Both the
    /// structural probe (<see cref="StructurallyImplements"/>) and the duck-typing shell binder
    /// (<see cref="AdapterBinder"/>) resolve through it, so a shell can never bind a method the probe
    /// would not have counted — the two would otherwise be free to disagree about a method set, and a
    /// value-sourced shell that picked up a pointer-receiver method would make an assertion Go REJECTS
    /// succeed. Deliberately keyed on the element + pointer-ness rather than on the concrete value type
    /// so no caller has to construct a <c>ж&lt;X&gt;</c> <see cref="Type"/> just to ask the question.
    /// </remarks>
    internal static List<MethodInfo> GetGoMethodSetCandidates(Type valueElement, bool valueIsPointer)
    {
        return s_goMethodSetCandidates.GetOrAdd((valueElement, valueIsPointer), static key =>
        {
            (Type valueElement, bool valueIsPointer) = key;
            (MethodInfo method, Type receiver)[] allExtensionMethods = GetExtensionMethods();
            List<MethodInfo> candidates = [];

            foreach ((MethodInfo method, Type receiver) in allExtensionMethods)
            {
                Type receiverType = receiver.IsByRef ? receiver.GetElementType()! : receiver;
                ResolveReceiverElement(receiverType, out Type receiverElement, out bool receiverIsPointer);

                // Pointer-receiver methods are NOT part of a plain value's method set (Go semantics).
                // A pointer receiver appears in TWO emitted shapes: the RecvGenerator's ж<X> overload
                // (receiverIsPointer above) and the original `this ref X` extension — the byref strip on
                // the line above erases the latter's pointer-ness, so ask the receiver itself
                // (IsPointerSetByRefReceiver). A `this ref X` receiver marked [GoCopyBound] is the third
                // shape, and it IS a value-set method: the forwarder of a pointer-receiver method
                // promoted through an embedded POINTER (bufio.ReadWriter's ReadString, through *Reader).
                // It stays in the set here and is bound through a COPY of the receiver (see
                // IsCopyBoundReceiver).
                if (!valueIsPointer && (receiverIsPointer || IsPointerSetByRefReceiver(method, receiver)))
                    continue;

                if (ReceiverElementMatches(receiverElement, valueElement))
                    candidates.Add(method);
            }

            // A RUNTIME-MINTED struct carries its promoted methods as its OWN declared statics, not
            // as extension methods, so the scan above cannot see them: nothing writes an extension
            // method for a type that did not exist when the process started. GoStructSynthesis emits
            // them receiver-first (argument 0 is the struct), which is the same shape an extension
            // method has, so they drop into the candidate list unchanged.
            //
            // They belong HERE, in the candidate source, and not in the entries table alone. Both
            // GetGoMethodSetEntries and StructurallyImplements read this list — that is what the ONE
            // SOURCE RULE means — and a promotion added downstream in the table gave
            // reflect.Type.MethodByName the right answer while an interface assertion on the same
            // type still answered MISS. Measured, on TestStructOfEmbeddedIfaceMethodCall.
            if (key.element.IsValueType && key.element.IsDynamicType())
            {
                foreach (MethodInfo declared in key.element.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    ParameterInfo[] parameters = declared.GetParameters();

                    if (parameters.Length > 0 && parameters[0].ParameterType == key.element)
                        candidates.Add(declared);
                }
            }

            return candidates;
        });
    }

    /// <summary>
    /// The EXPORTED methods in the Go method set of element type <paramref name="valueElement"/>,
    /// viewed as a pointer (<c>*X</c>, value- and pointer-receiver methods) or as a value (<c>X</c>,
    /// value-receiver only), in GO'S ORDER — sorted by method name — which is the order
    /// <c>reflect.Type.Method(i)</c> indexes.
    /// </summary>
    /// <param name="valueElement">Go element type, i.e. the pointee of a receiver box or the value's own type.</param>
    /// <param name="valueIsPointer"><c>true</c> for the <c>*X</c> method set; <c>false</c> for <c>X</c>'s.</param>
    /// <returns>The method table, ordered by Go method name.</returns>
    /// <remarks>
    /// Built over <see cref="GetGoMethodSetCandidates"/> — the SAME candidate source the structural
    /// probe and the shell binder resolve through — so a NumMethod gate, a Method(i) walk and the
    /// assert behind them can never disagree about a method set (encoding/json's <c>indirect()</c>
    /// only ATTEMPTS its Unmarshaler assert when <c>NumMethod() &gt; 0</c>; a count from any other
    /// source could answer 0 for a set the assert would bind, silently skipping every custom
    /// UnmarshalJSON). Candidates are DEDUPLICATED by projected Go name, because one Go
    /// pointer-receiver method reaches the registry in two emitted shapes (the RecvGenerator's
    /// <c>ж&lt;X&gt;</c> overload and the original <c>[GoRecv]</c> <c>this ref X</c> extension);
    /// the shape kept is the one a delegate can bind — a by-ref receiver cannot appear in a
    /// <c>Func&lt;&gt;</c>, and the generated <c>ж&lt;X&gt;</c> overload always exists beside it.
    /// Exported-ness is judged on the projected Go name (first rune uppercase), after the same
    /// leading collision-marker strip <see cref="GoMethodNameMatches"/> applies when matching.
    /// Ordering is ORDINAL on that projected name, which is Go's own method-table order (verified
    /// against <c>go run</c>: a promoted embedded method sorts in place, it is not appended).
    /// </remarks>
    internal static GoMethodSetEntry[] GetGoMethodSetEntries(Type valueElement, bool valueIsPointer)
    {
        return s_goMethodSetEntries.GetOrAdd((valueElement, valueIsPointer), static key =>
        {
            Dictionary<string, MethodInfo> byGoName = [];

            foreach (MethodInfo candidate in GetGoMethodSetCandidates(key.element, key.isPointer))
            {
                if (IsUniversalReceiver(candidate, key.element))
                    continue;

                string name = ProjectGoMethodName(candidate.Name);

                if (Rune.DecodeFromUtf16(name, out Rune first, out _) != OperationStatus.Done || !Rune.IsUpper(first))
                    continue;

                if (!byGoName.TryGetValue(name, out MethodInfo? kept) || PrefersBindableShape(candidate, kept))
                    byGoName[name] = candidate;
            }

            return [.. byGoName.Select(static pair => new GoMethodSetEntry(pair.Key, pair.Value))
                               .OrderBy(static entry => entry.GoName, StringComparer.Ordinal)];
        });
    }

    /// <summary>
    /// The methods of the INTERFACE type <paramref name="interfaceType"/> in Go's order (sorted by
    /// name) — <c>reflect.Type.Method(i)</c>'s table for an interface, which unlike a concrete
    /// type's includes the unexported methods (Go's interface contract).
    /// </summary>
    /// <param name="interfaceType">Interface type to tabulate.</param>
    /// <returns>The interface's method table, ordered by Go method name.</returns>
    internal static GoMethodSetEntry[] GetGoInterfaceMethodEntries(Type interfaceType)
    {
        return s_goInterfaceMethodEntries.GetOrAdd(interfaceType, static t =>
        {
            Dictionary<string, MethodInfo> byGoName = [];

            foreach (MethodInfo method in t.GetInterfaceMethods())
                byGoName[ProjectGoMethodName(method.Name)] = method;

            return [.. byGoName.Select(static pair => new GoMethodSetEntry(pair.Key, pair.Value))
                               .OrderBy(static entry => entry.GoName, StringComparer.Ordinal)];
        });
    }

    /// <summary>
    /// Counts the EXPORTED methods in the Go method set of <paramref name="valueElement"/> — the
    /// answer <c>reflect.Type.NumMethod</c> gives for a concrete type. It is the SIZE of the table
    /// <see cref="GetGoMethodSetEntries"/> enumerates, never a second derivation (see this file's
    /// header: a count and an order derived separately are free to disagree).
    /// </summary>
    /// <param name="valueElement">Go element type, i.e. the pointee of a receiver box or the value's own type.</param>
    /// <param name="valueIsPointer"><c>true</c> to count the <c>*X</c> method set; <c>false</c> for <c>X</c>'s.</param>
    /// <returns>Number of exported methods in the method set.</returns>
    internal static int GoMethodSetCount(Type valueElement, bool valueIsPointer)
    {
        return GetGoMethodSetEntries(valueElement, valueIsPointer).Length;
    }

    // Projects an emitted receiver-method name to its GO method name — the leading collision-avoidance
    // marker stripped (see GoMethodNameMatches, which applies the same strip when matching).
    internal static string ProjectGoMethodName(string emittedName)
    {
        return emittedName.Length > ShadowVarMarker.Length && emittedName.StartsWith(ShadowVarMarker, StringComparison.Ordinal)
            ? emittedName[ShadowVarMarker.Length..]
            : emittedName;
    }

    // Of two emitted shapes of the SAME Go method, prefer the one a delegate can bind: a `this ref X`
    // receiver cannot be a Func<> parameter, while the RecvGenerator's ж<X> overload beside it can.
    private static bool PrefersBindableShape(MethodInfo candidate, MethodInfo kept)
    {
        return !candidate.GetParameters()[0].ParameterType.IsByRef && kept.GetParameters()[0].ParameterType.IsByRef;
    }

    // An extension method on `this object` is golib PLUMBING, never a Go method of a Go type: no Go
    // declaration can have `any` as its receiver. GetGoMethodSetCandidates admits them anyway, because
    // its assignability safety net (for promotion and base relationships) is satisfied by EVERY type
    // when the receiver is object — so `TypeExtensions.TryCastAsInteger(this object, out ulong)` was
    // reaching every type's method table, and reaching it NONDETERMINISTICALLY, since the candidate
    // scan is redone whenever a late assembly load clears the caches: the same binary reported
    // NumMethod 4 or 6 for the same type depending only on which assemblies had loaded by the first
    // read. That is filtered HERE rather than in the shared candidate source, whose admission rule the
    // duck-typing assert and the shell binder also resolve through — a Go METHOD SET is a stricter
    // question than "could this extension method dispatch on this value?".
    private static bool IsUniversalReceiver(MethodInfo candidate, Type valueElement)
    {
        if (valueElement == typeof(object))
            return false;

        Type receiver = candidate.GetParameters()[0].ParameterType;

        return (receiver.IsByRef ? receiver.GetElementType()! : receiver) == typeof(object);
    }

    // Splits a receiver type into its concrete element type and whether it is a pointer box ж<X>.
    // Fix W (B1 §3.1): the box test walks the base chain, so both the DECLARED receiver-parameter
    // feed (always exactly ж<X> — walk depth 0, unchanged) and a RUNTIME value-type feed (a
    // per-kind subclass under the split) answer the same isPointer — the :321 pointer-receiver
    // exclusion depends on it. @unsafe.Pointer is exempt and keeps its non-pointer route: it has
    // no method set, and reclassifying it would change which (empty) candidate arm it takes.
    internal static void ResolveReceiverElement(Type type, out Type element, out bool isPointer)
    {
        if (!typeof(IUnsafePointer).IsAssignableFrom(type) && GoReflect.TryBoxPointee(type, out Type? pointee))
        {
            element = pointee;
            isPointer = true;
        }
        else
        {
            element = type;
            isPointer = false;
        }
    }

    // Determines whether an extension method's receiver element belongs to the asserted value's method set.
    private static bool ReceiverElementMatches(Type receiverElement, Type valueElement)
    {
        if (receiverElement == valueElement)
            return true;

        // An open-generic receiver (a method on a Go generic type, receiver e.g. List<T>) matches a
        // constructed value of the same generic definition (List<int>).
        if (receiverElement.ContainsGenericParameters && receiverElement.IsGenericType &&
            valueElement.IsGenericType &&
            receiverElement.GetGenericTypeDefinition() == valueElement.GetGenericTypeDefinition())
            return true;

        // Safety net for promotion/embedding and base relationships — mirrors the assignability rule the
        // legacy value-type extension lookup used.
        return receiverElement.IsAssignableFrom(valueElement);
    }

    // Compares a duck-typed interface method against a candidate extension method by full signature.
    // The candidate's first parameter is the extension receiver; the interface method has none.
    private static bool SignaturesMatch(MethodInfo interfaceMethod, MethodInfo candidate)
    {
        if (!TypesEquivalent(interfaceMethod.ReturnType, candidate.ReturnType))
            return false;

        ParameterInfo[] interfaceParameters = interfaceMethod.GetParameters();
        ParameterInfo[] candidateParameters = candidate.GetParameters();

        if (interfaceParameters.Length != candidateParameters.Length - 1)
            return false;

        for (int i = 0; i < interfaceParameters.Length; i++)
        {
            if (!TypesEquivalent(interfaceParameters[i].ParameterType, candidateParameters[i + 1].ParameterType))
                return false;
        }

        return true;
    }

    // Type identity for signature comparison, disregarding by-ref decoration (ref/in/out parameters).
    private static bool TypesEquivalent(Type left, Type right)
    {
        if (left.IsByRef)
            left = left.GetElementType()!;

        if (right.IsByRef)
            right = right.GetElementType()!;

        return left == right;
    }

    /// <summary>
    /// Returns all method names defined in an interface type, including those inherited from other interfaces.
    /// </summary>
    /// <param name="interfaceType">The interface type to examine. Must be an interface.</param>
    /// <returns>A collection of method names defined in the interface and its base interfaces.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided type is not an interface.</exception>
    public static ImmutableHashSet<string> GetInterfaceMethodNames(this Type interfaceType)
    {
        return s_interfaceMethodNames.GetOrAdd(interfaceType, _ => [..interfaceType.GetInterfaceMethods().Select(info => info.Name)]);
    }

    /// <summary>
    /// Returns detailed information about methods defined in an interface type, including those inherited from other interfaces.
    /// </summary>
    /// <param name="interfaceType">The interface type to examine. Must be an interface.</param>
    /// <returns>A collection of MethodInfo objects for methods defined in the interface and its base interfaces.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided type is not an interface.</exception>
    public static IEnumerable<MethodInfo> GetInterfaceMethods(this Type interfaceType)
    {
        // Verify the type is an interface
        if (!interfaceType.IsInterface)
            throw new ArgumentException($"The type '{interfaceType.FullName}' is not an interface.", nameof(interfaceType));

        // Get all methods directly defined on this interface
        MethodInfo[] methods = interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        // Get all base interfaces
        Type[] baseInterfaces = interfaceType.GetInterfaces();

        // If there are no base interfaces, return just the direct methods
        if (baseInterfaces.Length == 0)
            return methods;

        // Otherwise, combine the direct methods with inherited methods
        HashSet<MethodInfo> allMethods = [..methods];

        // Add methods from all base interfaces. The explicit BindingFlags MUST match the direct-member
        // call above, and the reason is that reflection's DEFAULT flags include STATICS: golib's
        // hand-written core interfaces expose static duck-typing conversion helpers (`error.As<T>`,
        // `fmt.Stringer.As<T>`), which are not part of any Go method set. Collect those and
        // StructurallyImplements demands a static `As` from the dynamic value's Go METHOD SET — which
        // no Go type can ever satisfy — so every interface that EMBEDS such a base probes FALSE.
        // Measured when that happened: an anonymous `interface{ error; Temporary() bool }` (net.Error's
        // shape) asserted against a *tempErr that plainly has both methods answered MISS.
        foreach (Type baseInterface in baseInterfaces)
        {
            MethodInfo[] baseMethods = baseInterface.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (MethodInfo method in baseMethods)
                allMethods.Add(method);
        }

        return allMethods;
    }

    /// <summary>
    /// Gets the names of all fields in a struct type.
    /// </summary>
    /// <param name="valueType">Struct type to search.</param>
    /// <returns>Names of all fields in the struct type.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided type is not a value type.</exception>
    public static ImmutableHashSet<string> GetStructFieldNames(this Type valueType)
    {
        return s_structFieldNames.GetOrAdd(valueType, _ =>
        {
            if (!valueType.IsValueType)
                throw new ArgumentException($"Type '{valueType.FullName}' is not a value type.", nameof(valueType));

            FieldInfo[] fields = valueType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            return [..fields.Select(field => field.Name)];
        });
    }
}

// GoDelegateSynthesis.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace go;

// ---------------------------------------------------------------------------------------------
// RUNTIME DELEGATE SYNTHESIS — the second place golib mints a CLR type, and the last one the
// reflection bridge needs.
//
// WHY THERE IS A FILE HERE AT ALL
//   A Go func value IS a managed delegate, so `reflect.FuncOf` COMPOSES a delegate type where Go
//   assembles a funcType record (GoReflect.MakeGoFuncType, TryFuncShape's exact inverse). Composing
//   means NAMING a declared delegate family, and every family is finite: System.Func/Action stop at
//   16 parameters, golib's own ladder continues them to 24 (funcArity.cs), and the variadic families
//   carry 8 fixed parameters ahead of the `params Span<T>` tail (variadic.cs). Go's own limit is 128
//   (`reflect.FuncOf: too many arguments`) and its reflect suite drives past every one of those
//   ceilings — all_test.go's TestFuncOf builds a 51-parameter func for issue #54669. Past the
//   declared rungs there is no type to name, so one is minted: the same answer, and the same
//   mechanism, GoStructSynthesis gives `reflect.StructOf`.
//
// THE POINT IS THAT NOTHING DOWNSTREAM CHANGES
//   A minted delegate is described by TryFuncShape, GoTypeName, KindOf and the Value machinery
//   exactly as a declared one is, because not one of them asks where a System.Type came from. The
//   round trip IS the contract — NumIn/In/NumOut/Out/IsVariadic unchanged through
//   MakeGoFuncType → TryFuncShape — and it is guarded rather than argued
//   (GolibTests.DelegateArityMintTests).
//
// MINTED ONLY AS THE RESIDUAL — DECLARED-FIRST IS A CONTRACT, NOT A PREFERENCE
//   Go INTERNS func types: `FuncOf(in, out, variadic) == TypeOf(f)` for a matching declared f, which
//   is precisely what TestFuncOf's checkSameType asserts. A converted `func(T1) string` IS
//   `Func<T1, @string>`, so answering FuncOf with a freshly minted type of that shape would be a
//   DIFFERENT System.Type and the identity would fail. GoReflect.MakeDelegateType therefore routes
//   to the declared family wherever one exists and reaches this file only past every ceiling — where
//   the two domains cannot overlap, since a converted package could not spell a 25-parameter func
//   type either (the ladder is what it would bind, and the ladder stops at 24).
//
// THE INTERN IS THE IDENTITY
//   Two FuncOf calls with one signature must answer ONE type, for the same reason. The intern is a
//   plain lock rather than a ConcurrentDictionary factory ON PURPOSE, GoStructSynthesis's reason
//   exactly: GetOrAdd runs its factory CONCURRENTLY on racing threads and discards the losers' work,
//   and the work here is DefineType, whose duplicate name THROWS. It is also deliberately absent
//   from ClearTypeCaches' list — a signature's minted type IS that signature's identity for the life
//   of the process, and clearing it would hand the next caller a different type for the same Go func
//   type, which is the one thing this mechanism exists to prevent.
//
// ONE ASSEMBLY, SHARED WITH THE STRUCT MINT — THE FRIEND GRANT DECIDES IT
//   A minted delegate's Invoke signature names whatever types the Go signature names, and an
//   UNEXPORTED Go type is emitted `internal`. Reaching one from a dynamic assembly needs that
//   assembly's friend grant, and every converted csproj already carries exactly one —
//   `<InternalsVisibleTo Include="go2cs.SynthesizedStructs" />`, emitted by the converter
//   (projectFileWriter.go) and by the test-project template. Minting into the SAME assembly rides
//   that grant and needs NO converter change; a separately named dynamic assembly would need a
//   second grant in every generated csproj plus a corpus-wide regen to carry it, for no gain. Type
//   identity is unaffected — identity is (assembly, name) — and the two mints cannot collide on a
//   name: `structᴛN` and `funcᴛN` come from separate counters under separate prefixes.
// ---------------------------------------------------------------------------------------------

/// <summary>
/// Mints a real CLR delegate type per Go func signature that no declared delegate family can spell,
/// so <c>reflect.FuncOf</c> can hand back a <c>reflect.Type</c> the rest of the bridge describes
/// exactly as it describes a converted func type.
/// </summary>
/// <remarks>
/// Reached only from <c>GoReflect.MakeDelegateType</c>, which routes every width a declared family
/// covers to that family instead — see this file's header for why that order is a contract.
/// </remarks>
internal static class GoDelegateSynthesis
{
    // Everything below is guarded by this one lock: the signature intern, the counter and the mint.
    // It is held across DefineType/CreateType, which is the point — see the header note on the
    // intern.
    private static readonly object s_mintLock = new();

    private static readonly Dictionary<string, Type> s_bySignature = new(StringComparer.Ordinal);

    private static int s_nextTypeId;

    /// <summary>
    /// The delegate type for a signature no declared family spells, minted once and interned
    /// thereafter.
    /// </summary>
    /// <param name="parameterTypes">
    /// The <c>Invoke</c> parameters, in Go order — already carrying a variadic tail as the trailing
    /// <c>Span&lt;T&gt;</c>. That convention is <see cref="GoReflect.MakeGoFuncType"/>'s and is NOT
    /// re-decided here: this mint carries the parameter types it is handed, which is what keeps one
    /// authority for the tail.
    /// </param>
    /// <param name="returnType">
    /// The <c>Invoke</c> return type — <c>void</c> for no results, the single result's type, or the
    /// ValueTuple a Go multi-return collapses into.
    /// </param>
    /// <returns>The minted delegate type, identical across calls with the same signature.</returns>
    internal static Type SynthesizeDelegateType(Type[] parameterTypes, Type returnType)
    {
        string signature = signatureKey(parameterTypes, returnType);

        lock (s_mintLock)
        {
            if (s_bySignature.TryGetValue(signature, out Type? existing))
                return existing;

            Type minted = mint(parameterTypes, returnType);
            s_bySignature[signature] = minted;
            return minted;
        }
    }

    // A Go func type is identified by its ordered parameter types and its result, so those ARE the
    // whole key — there is no dims-cargo counterpart to GoStructSynthesis's shapeKey here. The
    // residual that implies is real and is not this key's to fix: an `array<nint>` parameter of Go
    // length 1 and one of length 2 are ONE System.Type, so they are one composed func type whichever
    // family answers, minted or declared. The descriptor side owns array length; a signature carries
    // its types by identity.
    private static string signatureKey(Type[] parameterTypes, Type returnType)
    {
        // A separator no assembly-qualified type name can contain, so no parameter list can render
        // as another signature's key.
        const char PartSeparator = '\u0001';

        StringBuilder builder = new(64);

        foreach (Type parameter in parameterTypes)
            builder.Append(typeKey(parameter)).Append(PartSeparator);

        return builder.Append(PartSeparator).Append(typeKey(returnType)).ToString();
    }

    private static string typeKey(Type type)
    {
        return type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
    }

    // Called under s_mintLock. DefineType's parent parameter is DynamicallyAccessedMembers(All), so naming
    // MulticastDelegate as the parent asks the trimmer to keep all of its members; the two warnings are that
    // request seen from the members' side. This path's real limit is Native AOT's missing Reflection.Emit, and
    // that is GoStructSynthesis.SharedModule's DefineDynamicAssembly (IL3050, trim stage 3d), not a trim one.
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "Names MulticastDelegate as a dynamic type's parent, which keeps every member of it. The minted type " +
                        "only derives from it; nothing calls its reflection-only constructors or CreateDelegate overloads " +
                        "(trim class E, docs/PLAN-golib-full-trim.md).")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2111",
        Justification = "As IL2026: the parent's members are kept whole for derivation, and none is invoked through reflection.")]
    private static Type mint(Type[] parameterTypes, Type returnType)
    {
        // TOP-LEVEL, never nested in a package container — the opposite of GoStructSynthesis's
        // pkgPath arm, and for a reason that reads the same metadata: GoReflect's isUnnamedFuncType
        // decides a func type's namedness from its DECLARING TYPE, because a delegate the converter
        // declared inside a `<pkg>_package` class IS a Go defined func type and keeps its name. A
        // minted delegate is Go's UNNAMED func type, so it must declare nowhere; Name() then answers
        // "" and String() renders structurally through goFuncTypeString, which is what a FuncOf
        // result reports.
        TypeBuilder tb = GoStructSynthesis.SharedModule.DefineType(
            "go.synth.funcᴛ" + (++s_nextTypeId).ToString(CultureInfo.InvariantCulture),
            TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
            typeof(MulticastDelegate));

        // The shape the CLR requires of a delegate: an (object, nint) constructor and an Invoke,
        // both `runtime`-implemented — the runtime supplies both bodies, so no IL is emitted here at
        // all. The BeginInvoke/EndInvoke pair a C# `delegate` declaration also emits is deliberately
        // absent: .NET does not support it, and nothing in the bridge asks for it.
        ConstructorBuilder ctor = tb.DefineConstructor(
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            CallingConventions.Standard,
            [typeof(object), typeof(nint)]);

        ctor.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

        MethodBuilder invoke = tb.DefineMethod("Invoke",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
            returnType,
            parameterTypes);

        invoke.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

        // Nothing here READS a parameter name — TryFuncShape reads types, and the expression-compiled
        // wrappers fall back to `arg{i}` when a name is absent — but every diagnostic surface that
        // prints a signature prints them, and the declared families name theirs `arg1`… So do these.
        for (int index = 0; index < parameterTypes.Length; index++)
            invoke.DefineParameter(index + 1, ParameterAttributes.None, "arg" + (index + 1).ToString(CultureInfo.InvariantCulture));

        return tb.CreateType();
    }
}

//******************************************************************************************************
//  GoReflect.FinalizerBinding.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/08/2026 - runtime.SetFinalizer's argument rule (COORD ruling, mailbox 7ec93b2b5)
//       Generated original version of source code.
//
//******************************************************************************************************

// WHY THIS FILE EXISTS, AND THE MEASUREMENT THAT PRODUCED IT.
//
// runtime.SetFinalizer's finalizer is invoked through Delegate.DynamicInvoke, which binds through the
// default binder: identity, reference, boxing and primitive-widening conversions ONLY. It never
// invokes a user-defined conversion operator.
//
// A Go DEFINED POINTER TYPE is emitted as a WRAPPER, not as a subclass. go2cs-gen's
// InheritedTypeTemplate gives `type Tintptr *int` this shape:
//
//     partial class Tintptr : IPointer<nint>, INilPointer
//     {
//         private ж<nint> m_value;
//         public static implicit operator Tintptr(ж<nint> value) => new Tintptr(value);
//         public static implicit operator ж<nint>(Tintptr value) => value.Value;
//     }
//
// so the relation between `Tintptr` and `ж<nint>` is a USER-DEFINED operator pair and NOT a reference
// conversion. The template's own comment says why: Go itself requires an explicit conversion between
// a defined type and its underlying.
//
// Go's runtime accepts `SetFinalizer(Tintptr(x), func(v *int){...})` -- both are pointers, one is
// unnamed, and the element types match, so the argument is assignable. Our dispatch could not bind it,
// DynamicInvoke threw, and the finalizer runner's catch swallowed the throw: the item was dequeued,
// the outstanding count decremented, the queue reported idle, and the finalizer NEVER RAN. Measured on
// runtime's own TestFinalizerType, which loops six such shapes and blocks on a receive per iteration:
// it delivers shapes 1 and 2 and hangs forever on the third, consuming the package deadline with ZERO
// converted verdicts (i9, 2026-09-08, an iteration-index probe printing three markers per iteration --
// 3 + 3 + 2 markers, no third delivery, ever).
//
// ⚠ The failure had NO error surface anywhere. That is what makes it worth a file rather than a patch.
//
// WHAT THIS IS NOT. It is not a second implementation of assignability. The two hard parts already
// existed and are CALLED here: GoReflect.TryConvertTo already unwraps a named wrapper to its
// underlying (and constructs one in the other direction), memoizing the wrapper constructor per type;
// and AdapterBinder.TryCreate already builds the duck-typing shell for an interface a converted value
// satisfies structurally, memoizing the factory per (valueType, interfaceType) pair -- including the
// negative, because Go fixes a method set at compile time. What this file adds is Go's GATE in front
// of them, because both helpers are deliberately MORE permissive than SetFinalizer's rule and using
// either unguarded would accept pairings Go REJECTS.

using System;
using System.Reflection;
using go.golib;

namespace go;

public static partial class GoReflect
{
    /// <summary>
    /// Binds a finalizer delegate to a referent under Go's <c>runtime.SetFinalizer</c> argument rule.
    /// </summary>
    /// <param name="referent">The object the finalizer was registered against.</param>
    /// <param name="finalizer">The finalizer delegate.</param>
    /// <param name="argument">The value to pass to <paramref name="finalizer"/>, when the pair binds.</param>
    /// <param name="rejection">Go's rejection text, when the pair does not bind.</param>
    /// <returns><c>true</c> when the pair binds; otherwise, <c>false</c> with <paramref name="rejection"/> set.</returns>
    /// <remarks>
    /// <para>
    /// A TRANSCRIPTION of runtime/mfinal.go's own switch (1.23.12 lines 468-499; 1.24.13 lines 490-521 --
    /// the rule and every message string are BYTE-IDENTICAL across the two releases, checked at both
    /// pinned GOROOTs rather than carried from one):
    /// </para>
    /// <code>
    ///   the finalizer must be a func, NOT variadic, with exactly ONE input
    ///   fint == etyp                                            -> ok, same type
    ///   fint is a pointer, one of the two is UNNAMED,
    ///     and the element types match                           -> ok, assignable
    ///   fint is an interface, and it is EMPTY                   -> ok
    ///   fint is an interface the object's type implements       -> ok
    ///   otherwise                                               -> panic naming BOTH types
    /// </code>
    /// <para>
    /// ⚠ <b>One caller, two questions.</b> The same predicate answers registration ("would Go accept
    /// this pair") and dispatch ("what value does the delegate receive"), so the two can never disagree
    /// -- which is the defect this replaces, where registration asked nothing and dispatch asked a .NET
    /// binder a DIFFERENT question.
    /// </para>
    /// <para>
    /// ⚠ <b>Nothing derived from the referent is retained.</b> The conversion happens at DISPATCH and
    /// never at registration: an adapter shell built at registration would hold the referent strongly
    /// and the finalizer could never run at all. Registration validates TYPES only.
    /// </para>
    /// </remarks>
    public static bool TryBindFinalizerArgument(object? referent, Delegate? finalizer, out object? argument, out string? rejection)
    {
        argument = null;
        rejection = null;

        if (referent is null || finalizer is null)
        {
            rejection = "runtime.SetFinalizer: first argument is nil";
            return false;
        }

        MethodInfo? invoke = finalizer.GetType().GetMethod("Invoke");

        if (invoke is null)
        {
            // Not reachable for a real delegate; stated rather than assumed away.
            rejection = "runtime.SetFinalizer: second argument is " + GoTypeName(finalizer.GetType()) + ", not a function";
            return false;
        }

        Type etyp = referent.GetType();
        ParameterInfo[] parameters = invoke.GetParameters();

        // Go rejects a variadic finalizer with its own message ("because dotdotdot"). A converted Go
        // variadic parameter is a params array, which is what this tests.
        if (parameters.Length > 0 && parameters[^1].IsDefined(typeof(ParamArrayAttribute), false))
        {
            rejection = cannotPass(etyp, finalizer.GetType()) + " because dotdotdot";
            return false;
        }

        if (parameters.Length != 1)
        {
            rejection = cannotPass(etyp, finalizer.GetType());
            return false;
        }

        Type fint = parameters[0].ParameterType;

        // 1. THE REFERENT ALREADY IS ONE. Assignability, NOT type equality -- the fix for the defect
        //    this predicate introduced at TestFinalizerType's iteration 0, where a five-minute silent
        //    hang became a 3.5-second refusal at a shape Go accepts (i9's run, COORD 1c4b349bb).
        //
        //    ⚠ ONE GO POINTER TYPE IS A FAMILY OF C# TYPES. `*int` is `ж<nint>` as a PARAMETER, and at
        //    run time the argument is whichever box kind produced it -- StandardBox, FieldRefBox,
        //    ElemRefBox, NativeBox, each a SUBCLASS of ж<T>. Go's rule reads "fint == etyp" over GO
        //    types, and transcribing that as C# type EQUALITY makes every box that is not the exact
        //    parameter type fall through to the pointer arm, where it depends on a conversion it
        //    should never have needed. `&new(T).v` emits `@new<T>().of(T.Ꮡv)` -- a ж<nint> whose
        //    runtime type is FieldRefBox<nint> -- and Go passes it to `func(v *int)` without a murmur.
        //
        //    IsInstanceOfType is also exactly what the default binder would have accepted here: this
        //    case is a reference conversion, so agreeing with it is not a widening of Go's rule.
        if (fint.IsInstanceOfType(referent))
        {
            argument = referent;
            return true;
        }

        // 2. Go's empty interface. `any` is emitted as `object`, whose conversion is a reference one --
        //    it bound before this file existed and still does; it is listed so the rule is complete.
        if (fint == typeof(object))
        {
            argument = referent;
            return true;
        }

        // 3. A non-empty interface parameter. Direct implementation first (a reference conversion), then
        //    the duck-typing shell, which is how a converted value satisfies a Go interface it does not
        //    implement in the CLR sense. TryCreate is FAIL-SOFT and memoized per (value, interface) pair.
        if (fint.IsInterface)
        {
            // (An interface the referent's own type implements is already handled by case 1's
            // assignability test above; only the duck-typed shell can reach here now.)
            if (AdapterBinder.TryCreate(referent, fint, out object? shell))
            {
                argument = shell;
                return true;
            }

            rejection = cannotPass(etyp, finalizer.GetType());
            return false;
        }

        // 4. Both pointers, ELEMENTS EQUAL, and at least one UNNAMED. Go's own condition is
        //    `(fint.Uncommon() == nil || etyp.Uncommon() == nil) && fint.Elem == ot.Elem`; a raw ж<T> is
        //    our unnamed pointer type and a generated wrapper is the named one.
        //
        //    ⚠ THE GATE IS THE POINT. TryConvertTo below would also convert a named INTEGER wrapper to
        //    its underlying, which Go rejects here -- so the structural test runs FIRST and the helper
        //    is only asked to perform a conversion Go has already agreed to.
        if (tryPointeeOf(fint, out Type? finElem, out bool finUnnamed) &&
            tryPointeeOf(etyp, out Type? etypElem, out bool etypUnnamed) &&
            finElem == etypElem &&
            (finUnnamed || etypUnnamed) &&
            TryConvertTo(referent, fint, out object? converted) && converted is not null)
        {
            argument = converted;
            return true;
        }

        rejection = cannotPass(etyp, finalizer.GetType());
        return false;
    }

    /// <summary>Go's exact rejection text, both releases: "cannot pass X to finalizer Y".</summary>
    private static string cannotPass(Type objectType, Type finalizerType) =>
        "runtime.SetFinalizer: cannot pass " + GoTypeName(objectType) + " to finalizer " + GoTypeName(finalizerType);

    /// <summary>
    /// Yields a Go pointer type's element type, and whether the pointer is UNNAMED.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An unnamed Go pointer is the box itself (<c>ж&lt;T&gt;</c>), which <see cref="TryBoxPointee"/>
    /// finds by walking the base chain. A DEFINED pointer type is a generated WRAPPER and does NOT
    /// derive from the box, so that walk answers false for it: it carries a <c>[GoType]</c> marker and
    /// holds its underlying in a private <c>m_value</c> field.
    /// </para>
    /// <para>
    /// ⚠ <b>That pair -- the marker and the field -- is deliberately the SAME key
    /// <see cref="TryUnwrapWrapperValue"/> uses</b>, because <see cref="TryConvertTo"/> performs the
    /// conversion through exactly that unwrap. Keying this test on anything else (the generated
    /// <c>IPointer&lt;T&gt;</c> contract, say, or the operator pair) would let the two disagree: a type
    /// this predicate calls a pointer but the converter cannot unwrap would be ACCEPTED here and then
    /// fail to convert, which is the shape of the defect this whole file replaces.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2070", Justification = GoTypeRegistry.RegisteredTypeJustification)]
    private static bool tryPointeeOf(Type? t, out Type? pointee, out bool unnamed)
    {
        pointee = null;
        unnamed = false;

        if (t is null)
            return false;

        if (TryBoxPointee(t, out pointee))
        {
            unnamed = true;
            return true;
        }

        if (goTypeMarkerOf(t) is not { Definition.Length: > 0 } marker || marker.Definition == "dyn")
            return false;

        FieldInfo? valueField = t.GetField("m_value", BindingFlags.Instance | BindingFlags.NonPublic);

        return valueField is not null && TryBoxPointee(valueField.FieldType, out pointee);
    }
}

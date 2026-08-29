// builtin.DelegateConversions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

public static partial class builtin
{
    /// <summary>
    /// Converts a delegate value to a compatible NAMED delegate type the way Go converts a func
    /// value to a defined func type — never panicking for a nil source.
    /// </summary>
    /// <remarks>
    /// The converter emits this for <c>NamedFuncType(x)</c> where <c>x</c> is not a compile-time
    /// nil literal (that shape converts to a plain typed-nil cast instead, since delegate creation
    /// cannot express it). <c>x</c> CAN still be null at RUNTIME — a nil <c>handler
    /// func(ResponseWriter, *Request)</c> parameter reaching <c>HandlerFunc(handler)</c>
    /// (net/http's own <c>HandleFunc</c>) is exactly this case. The direct emission,
    /// <c>new HandlerFunc(handler)</c>, is the .NET delegate-copy CONSTRUCTOR — eager, and it
    /// throws <c>ArgumentException</c> ("Delegate to an instance method cannot have null 'this'")
    /// for a null source, before Go's own nil check ever runs (net/http's <c>registerErr</c> is
    /// what is supposed to observe and panic on the nil handler). Go's conversion itself never
    /// panics — <c>HandlerFunc(nil)</c> is ordinary, nil-carrying code — so this mirrors that:
    /// null in, null out, and only a non-null source pays for the delegate construction.
    /// </remarks>
    public static TTarget? NilSafeDelegateConversion<TTarget, TSource>(TSource? source)
        where TTarget : Delegate
        where TSource : Delegate
    {
        return source is null ? null : (TTarget)Delegate.CreateDelegate(typeof(TTarget), source.Target, source.Method);
    }

    /// <summary>
    /// Returns <paramref name="value"/> as an interface value, substituting the canonical typed nil
    /// for a null delegate — the FUNC twin of <c>ж&lt;T&gt;.OrTypedNil</c>, and the same one-nil-
    /// encoding rule.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Go's interface is a (dynamic type, value) pair, so a nil func inside one is NOT the nil
    /// interface: <c>var x any = (func())(nil)</c> gives a NON-nil interface whose <c>%T</c> prints
    /// <c>func()</c> and whose type assertion succeeds with a nil result. A Go func emits as a
    /// managed delegate whose nil IS <c>null</c> — correct in every func-typed slot, and
    /// type-ERASING the moment it is boxed, because a null reference carries nothing. Left alone,
    /// <c>x == nil</c> answered true where Go says false and <c>%T</c> printed <c>&lt;nil&gt;</c>.
    /// </para>
    /// <para>
    /// The choice cannot be made where the nil is PRODUCED — a declared variable, a struct field, a
    /// map miss — because in a func-typed slot null is exactly right. It is made HERE, at the one
    /// boundary where the difference becomes observable, which is the identical argument
    /// <see cref="ж{T}.NilBox"/> rests on for pointers.
    /// </para>
    /// <para>
    /// The converter emits this at every func-into-<c>any</c> site. A NON-empty interface target
    /// needs it not, and neither does a non-null delegate, which already carries its own type.
    /// </para>
    /// </remarks>
    public static object OrTypedNilFunc<TDelegate>(this TDelegate? value)
        where TDelegate : Delegate
    {
        // The interned instance, never a fresh one — two typed nils of one func type must compare
        // reference-equal wherever the comparison is an untyped object reference compare.
        return value ?? GoReflect.CanonicalNilFunc(typeof(TDelegate));
    }
}

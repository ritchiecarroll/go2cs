// GoMethodText.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using go.golib;
using GoTypeExtensions = go.golib.TypeExtensions;

namespace go;

/// <summary>
/// The text a Go value's <c>Error()</c> or <c>String()</c> method gives it, which is what a C#
/// <c>ToString()</c> of a converted Go value returns.
/// </summary>
/// <remarks>
/// <para>
/// This is fmt's <c>handleMethods</c> for <c>%v</c>. An <c>error</c> prints its <c>Error()</c>, otherwise
/// a <c>Stringer</c> prints its <c>String()</c>, and only a value with neither prints its fields or its
/// underlying value. A C# caller (string interpolation, a debugger, a log line) then sees what a Go
/// developer sees from <c>fmt.Println</c>. The converted <c>fmt</c> never calls <c>ToString</c>, so Go's
/// own output does not move.
/// </para>
/// <para>
/// The method is chosen by KIND across the whole method set Go gives the operand: <c>Error</c> before
/// <c>String</c>, whatever their receivers. For a value <c>T</c> that set holds the value-receiver
/// methods only; for a pointer <c>*T</c> it holds the value AND the pointer receivers, so a value
/// <c>Error()</c> wins over a pointer <c>String()</c>, exactly as <c>fmt.Println(&amp;t)</c> chooses. The set
/// is read from <see cref="GoTypeExtensions.GetGoMethodSetCandidates"/>, the one place Go's receiver
/// rule is applied at run time, so promoted methods (the generator's own forwarders, which no
/// generator can see at compile time) count exactly as the interface binder counts them.
/// </para>
/// <para>
/// A method that panics prints Go's <c>%!v(PANIC=String method: …)</c> text instead of throwing, and a
/// nil pointer whose method panics prints <c>&lt;nil&gt;</c>, both as fmt's <c>catchPanic</c> prints them.
/// A <c>ToString</c> therefore never throws a Go panic, which a debugger display relies on. A
/// <c>runtime.Goexit</c> still unwinds. The stated gap: a GENERIC type's methods are generic extension
/// methods, which are not bound here, so such a type keeps its structural text.
/// </para>
/// </remarks>
public static class GoMethodText
{
    /// <summary>
    /// Gets the <c>Error()</c> or <c>String()</c> text of <paramref name="value"/>, when the value method
    /// set of <typeparamref name="T"/> holds either.
    /// </summary>
    /// <typeparam name="T">Go value type.</typeparam>
    /// <param name="value">Go value to render.</param>
    /// <returns>The method's text, or Go's panic text when the method panicked; <c>null</c> when the type has neither method.</returns>
    [DebuggerNonUserCode]
    public static string? Of<T>(in T value)
    {
        if (ValueMethods<T>.Method is not { } method)
            return null;

        try
        {
            return method.call(value).ToString();
        }
        catch (Exception ex) when (ex is not GoexitException)
        {
            return PanicText(method.name, ex);
        }
    }

    /// <summary>
    /// Gets the <c>Error()</c> or <c>String()</c> text of the pointer <paramref name="box"/>, when the
    /// pointer method set of <typeparamref name="T"/> holds either.
    /// </summary>
    /// <typeparam name="T">Pointee type.</typeparam>
    /// <param name="box">Go pointer to render; a <c>null</c> reference reads as nil.</param>
    /// <returns>
    /// The method's text, Go's panic text, or <c>&lt;nil&gt;</c> for a nil pointer whose method panicked;
    /// <c>null</c> when the type has neither method.
    /// </returns>
    /// <remarks>
    /// A nil pointer still calls its method, because Go dispatches on the TYPE: a pointer-receiver
    /// <c>Error()</c> may answer a nil receiver itself. Only a panic turns it into <c>&lt;nil&gt;</c>, and a
    /// value-receiver method through a nil pointer always panics, as in Go.
    /// </remarks>
    [DebuggerNonUserCode]
    public static string? OfPointer<T>(ж<T>? box)
    {
        if (PointerMethods<T>.Method is not { } method)
            return null;

        try
        {
            return method.call(box!).ToString();
        }
        catch (Exception ex) when (ex is not GoexitException)
        {
            return box == nil ? "<nil>" : PanicText(method.name, ex);
        }
    }

    // Go's catchPanic: `%!v(PANIC=<Method> method: <panic value as %v>)`. The value is mapped the way an
    // emitted catch filter maps it, so a nil dereference reads as Go's runtime error.
    private static string PanicText(string method, Exception ex)
    {
        string value = RuntimeErrorPanic.TryAsPanic(ex, out PanicException? panic) ? PanicException.PanicText(panic.State) : ex.Message;
        return $"%!v(PANIC={method} method: {value})";
    }

    private delegate @string RefMethod<T>(ref T target);

    // One binding per (T, value-or-pointer), kept with the candidate list it was resolved from. That list is
    // GetGoMethodSetCandidates' own cache entry, which an assembly load clears (TypeExtensions.ClearTypeCaches),
    // so a binding is re-resolved exactly when the method-set scan it came from is: a cache derived from the
    // scan must never outlive it (the rule in TypeExtensions.ExtensionMethodRegistry.cs).
    private sealed class Binding<TDelegate>(List<MethodInfo>? source, (TDelegate call, string name)? method) where TDelegate : Delegate
    {
        public readonly List<MethodInfo>? Source = source;
        public readonly (TDelegate call, string name)? Method = method;
    }

    private static (TDelegate call, string name)? Bound<T, TDelegate>(ref Binding<TDelegate>? cache, bool valueIsPointer, Func<MethodInfo, TDelegate?> bind) where TDelegate : Delegate
    {
        List<MethodInfo>? candidates;

        try
        {
            candidates = GoTypeExtensions.GetGoMethodSetCandidates(typeof(T), valueIsPointer);
        }
        catch (Exception ex) when (ex is not GoexitException)
        {
            candidates = null;
        }

        Binding<TDelegate>? binding = System.Threading.Volatile.Read(ref cache);

        if (binding is null || !ReferenceEquals(binding.Source, candidates))
        {
            binding = new Binding<TDelegate>(candidates, candidates is null ? null : Resolve<T, TDelegate>(candidates, bind));
            System.Threading.Volatile.Write(ref cache, binding);
        }

        return binding.Method;
    }

    private static class ValueMethods<T>
    {
        private static Binding<Func<T, @string>>? s_binding;

        public static (Func<T, @string> call, string name)? Method => Bound<T, Func<T, @string>>(ref s_binding, valueIsPointer: false, Bind);

        private static Func<T, @string>? Bind(MethodInfo method)
        {
            Type receiver = method.GetParameters()[0].ParameterType;

            // `this T`, or a copy-bound `this ref T` (a value-set forwarder; CreateStaticDelegate binds it through a copy).
            if (receiver == typeof(T) || receiver == typeof(T).MakeByRefType() && GoTypeExtensions.IsCopyBoundReceiver(method, out _))
                return method.CreateStaticDelegate(typeof(Func<T, @string>)) as Func<T, @string>;

            return null;
        }
    }

    private static class PointerMethods<T>
    {
        private static Binding<Func<ж<T>, @string>>? s_binding;

        public static (Func<ж<T>, @string> call, string name)? Method => Bound<T, Func<ж<T>, @string>>(ref s_binding, valueIsPointer: true, Bind);

        private static Func<ж<T>, @string>? Bind(MethodInfo method)
        {
            Type receiver = method.GetParameters()[0].ParameterType;

            if (receiver == typeof(ж<T>))
                return Delegate.CreateDelegate(typeof(Func<ж<T>, @string>), method, false) as Func<ж<T>, @string>;

            if (receiver == typeof(T) || receiver == typeof(T).MakeByRefType() && GoTypeExtensions.IsCopyBoundReceiver(method, out _))
            {
                // A value-receiver method through a pointer reads the pointee (a nil pointer panics there, as in Go).
                if (method.CreateStaticDelegate(typeof(Func<T, @string>)) is not Func<T, @string> byValue)
                    return null;

                return box => byValue(box.Value);
            }

            if (receiver == typeof(T).MakeByRefType())
            {
                // A pointer-receiver `this ref T` with no ж<T> twin (a hand-owned [GoRecv] method).
                if (Delegate.CreateDelegate(typeof(RefMethod<T>), method, false) is not RefMethod<T> byRef)
                    return null;

                return box => byRef(ref box.Value);
            }

            return null;
        }
    }

    // Error before String, by kind: the first name with a bindable `func() string` method wins. Within one
    // name the ж<T> receiver is preferred, then a value receiver, so a pointer method set binds the form that
    // never dereferences a nil pointer it does not have to.
    private static (TDelegate call, string name)? Resolve<T, TDelegate>(List<MethodInfo> candidates, Func<MethodInfo, TDelegate?> bind) where TDelegate : Delegate
    {
        foreach (string name in (string[])["Error", "String"])
        {
            foreach (bool projectGoName in (bool[])[false, true])
            {
                IEnumerable<MethodInfo> matches = candidates
                    .Where(method => !method.IsGenericMethod && method.ReturnType == typeof(@string) &&
                                     method.GetParameters().Length == 1 && GoTypeExtensions.GoMethodNameMatches(method, name, projectGoName))
                    .OrderBy(method => method.GetParameters()[0].ParameterType == typeof(ж<T>) ? 0 : method.GetParameters()[0].ParameterType == typeof(T) ? 1 : 2);

                foreach (MethodInfo method in matches)
                {
                    if (bind(method) is { } call)
                        return (call, name);
                }
            }
        }

        return null;
    }
}

/// <summary>
/// A Go pointer's address token, for <c>print</c>/<c>println</c>, which print a pointer's address and
/// never its <c>Error()</c> or <c>String()</c>.
/// </summary>
internal interface IPrintPointer
{
    /// <summary>
    /// Renders the pointer's address token, or <c>&lt;nil&gt;</c>.
    /// </summary>
    string PrintPointer();
}

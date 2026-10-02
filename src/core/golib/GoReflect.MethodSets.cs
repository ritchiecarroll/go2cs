// GoReflect.MethodSets.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using go.golib;
using static go2cs.Symbols;

#pragma warning disable IL2026
#pragma warning disable IL2060
#pragma warning disable IL2070
#pragma warning disable IL3050

namespace go;

// ---------------------------------------------------------------------------------------------
// THE METHOD TABLE — what reflect.Type.NumMethod / Method(i) / MethodByName and Value.Method(i)
// read, and what a method VALUE is once bound.
//
// WHY IT EXISTS
//   Go's reflect answers all of these out of the type descriptor's `uncommon()` method tables:
//   a name offset into the linker-built name blob, a type offset for the signature, and a text
//   offset for the code pointer. A SYNTHESIZED descriptor (the bridge stamps a System.Type onto an
//   abi.Type and nothing else) never populates any of them — so `NumMethod` answered 0 for every
//   concrete type, `Method(i)` panicked "reflect: Method index out of range", and `MethodByName`
//   answered not-found. The first was SILENT (0 is most types' correct count), the second is loud.
//
// THE ATOMIC-PAIR LESSON — WHY COUNT AND ENUMERATION SHIP TOGETHER
//   The count landed one increment ahead of the enumeration and had to be reverted. Answering
//   `NumMethod() > 0` truthfully is what lets a consumer's `for i := 0; i < n; i++` loop get as far
//   as `Type.Method(i)` at all: encoding/json's indirect() only reads the count, but math/rand and
//   math/rand/v2's TestRegress WALK the set — `typ.Method(i).Name`, `rv.Method(i)`, `mv.Type()`,
//   `mv.Call(args)` — and both packages regressed from validated to panicking on the very next
//   sweep. A truthful count is a promise that the table behind it can be indexed.
//
// THE ONE SOURCE RULE (inherited from golib TypeExtensions.GoMethodSets)
//   Every answer here comes from `GetGoMethodSetEntries` / `GetGoInterfaceMethodEntries` — the same
//   candidate machinery `StructurallyImplements` (the duck-typing assert) and `AdapterBinder` (the
//   shell binder) resolve through. Reflection therefore cannot believe in a method the assert would
//   not bind, nor miss one it would; and the count is that table's `.Count`, not a parallel
//   derivation.
//
// A METHOD VALUE IS AN ORDINARY BOUND DELEGATE — THAT IS THE WHOLE DESIGN
//   Go represents `v.Method(i)` as the receiver's Value with a `flagMethod` bit and the method
//   index packed into the flag's high bits; `Type()` then rebuilds the signature through
//   `typeSlow()` and `Call` re-resolves the receiver through `methodReceiver` — all descriptor
//   reads. The bridge instead BINDS the receiver into a managed delegate at Method(i) time, so the
//   result is an ordinary Kind-Func Value carrying a Delegate. Everything downstream is then reuse,
//   not new surface: `mv.Type()` is the ordinary canonical Type of a delegate, `mv.Type().NumIn/In/
//   NumOut/Out` are the existing TryFuncShape readers, and `mv.Call(args)` is the existing
//   Value.Call unchanged (with the receiver already gone from the signature, which is exactly Go's
//   contract for a method value). `Type.Method(i).Func` is the same delegate UNBOUND — receiver
//   first — which is Go's contract for the type-side Func.
//
// BINDING IS EXPRESSION-COMPILED, AND THAT IS NOT A PREFERENCE
//   `Delegate.CreateDelegate(type, firstArgument, method)` cannot close over a VALUE-type first
//   argument (measured: ArgumentException) and Go value receivers are by-value structs, so the
//   closed-delegate form would fail for every value-receiver method — time.Time's whole method set
//   among them. A compiled expression lambda binds both cases. Each MethodInfo is compiled ONCE
//   into a `Func<object?, Delegate>` factory (a nested lambda: the outer takes the receiver, the
//   inner IS the bound Go-signature delegate), so per-bind cost is a closure allocation rather than
//   a compile — the same memoization rule the bridge's ValueSlot accessors already follow.
//
// CACHES
//   s_boundFactories / s_methodFuncs / s_methodFuncTypes are keyed by MethodInfo and are NOT on
//   ClearTypeCaches' list, correctly so: a MethodInfo's signature is fixed when its type loads, and
//   a late-loading assembly cannot change it. Only the TABLES (which methods belong to a set) are
//   derived from the extension-method scan, and those are cleared in golib beside the scan itself.
// ---------------------------------------------------------------------------------------------
public static partial class GoReflect
{
    private static readonly ConcurrentDictionary<MethodInfo, Func<object?, Delegate>> s_boundFactories = [];
    private static readonly ConcurrentDictionary<MethodInfo, Type> s_methodFuncTypes = [];

    // The pointer-lookup form of s_methodFuncTypes, and the reason it needs its OWN cache: the same
    // MethodInfo yields a DIFFERENT Go func type depending on whether it was reached through T or
    // through *T, so the receiver type has to be part of the key. (The note above about these caches
    // being MethodInfo-keyed "correctly so" holds for a method's own fixed signature; Go's type-side
    // Func is not that signature — it is the signature with the LOOKUP type as its receiver.)
    private static readonly ConcurrentDictionary<(Type Receiver, MethodInfo Method), Type> s_pointerReceiverFuncTypes = [];

    // Its delegate counterpart, keyed the same way and for the same reason.
    private static readonly ConcurrentDictionary<(Type Receiver, MethodInfo Method), Delegate> s_pointerReceiverFuncs = [];
    private static readonly ConcurrentDictionary<MethodInfo, Delegate> s_methodFuncs = [];

    /// <summary>
    /// The number of methods <c>reflect.Type.NumMethod</c> reports for the Go type
    /// <paramref name="t"/> represents: for an interface type ALL of its methods (exported and
    /// unexported — Go's interface contract), for a concrete type the EXPORTED methods in its
    /// method set — where a pointer box <c>ж&lt;X&gt;</c> sees X's value- AND pointer-receiver
    /// methods and a plain X only the value-receiver ones.
    /// </summary>
    /// <param name="t">Managed type standing for the Go type, or <c>null</c>.</param>
    /// <returns>Size of the method set.</returns>
    /// <remarks>
    /// The auto <c>rtype.NumMethod</c> reads <c>uncommon()</c> method tables a synthesized
    /// descriptor never populates, so it answered 0 for every concrete type — and
    /// <c>encoding/json</c>'s <c>indirect()</c> gates its Unmarshaler/TextUnmarshaler discovery on
    /// <c>NumMethod() &gt; 0</c>, so no custom <c>UnmarshalJSON</c> was ever dispatched
    /// ("json: cannot unmarshal string into Go value of type time.Time"). This is the SIZE of the
    /// table <see cref="GoMethodName"/>/<see cref="GoMethodValue"/> index, so a truthful count is
    /// always backed by an indexable table. An adapter shell answers as the Go dynamic type it
    /// stands for — a pointer-sourced adapter as <c>*T</c>, a value-sourced one as the wrapped
    /// struct — mirroring the <see cref="KindOf"/>/<see cref="ElementType"/> unwrap (R10).
    /// </remarks>
    public static int GoMethodCount(Type? t)
    {
        return GoMethodTable(t).Length;
    }

    /// <summary>
    /// Whether the Go type <paramref name="t"/> declares ANY method — value- OR pointer-receiver —
    /// which is what Go's <c>rtype.Uncommon() != nil</c> reports.
    /// </summary>
    /// <remarks>
    /// NOT the same question as <see cref="GoMethodCount"/>, and the difference is load-bearing where
    /// Go's own guards are phrased over <c>Uncommon()</c>: the method SET of a value <c>T</c> excludes
    /// pointer-receiver methods, so a struct whose only method is <c>func (p *T) Set(int)</c> answers
    /// <c>GoMethodCount == 0</c> while Go still sees an uncommon section and applies its
    /// <c>StructOf</c> refusals to it (<c>reflect.TestStructOfWithInterface</c>'s <c>SettablePointer</c>
    /// arm, whose two-field embed Go rejects and a method-count test does not).
    /// </remarks>
    /// <param name="t">Managed type standing for the Go type, or <c>null</c>.</param>
    /// <returns><c>true</c> when either method set is non-empty.</returns>
    public static bool GoHasAnyMethods(Type? t)
    {
        if (t is null)
            return false;

        if (GoMethodTable(t).Length > 0)
            return true;

        // The pointer view of the same element: `ж<T>` sees T's value- AND pointer-receiver methods.
        // An interface or an already-pointer type has nothing further to ask about.
        if (t.IsInterface || TryBoxPointee(t, out _))
            return false;

        return golib.TypeExtensions.GetGoMethodSetEntries(t, valueIsPointer: true).Length > 0;
    }

    /// <summary>
    /// The Go NAME of the <paramref name="index"/>'th method of <paramref name="t"/>'s method set,
    /// in Go's method order (sorted by name).
    /// </summary>
    /// <param name="t">Managed type standing for the Go type.</param>
    /// <param name="index">Method index, <c>0 &lt;= index &lt; GoMethodCount(t)</c>.</param>
    /// <returns>The Go method name.</returns>
    public static string GoMethodName(Type? t, int index)
    {
        return MethodAt(t, index).GoName;
    }

    /// <summary>
    /// The index of the method named <paramref name="name"/> in <paramref name="t"/>'s method set,
    /// or <c>-1</c> when the method set does not contain it.
    /// </summary>
    /// <param name="t">Managed type standing for the Go type.</param>
    /// <param name="name">Go method name.</param>
    /// <returns>Method index, or <c>-1</c>.</returns>
    public static int GoMethodIndex(Type? t, string name)
    {
        GoMethodSetEntry[] table = GoMethodTable(t);

        for (int i = 0; i < table.Length; i++)
        {
            if (string.Equals(table[i].GoName, name, StringComparison.Ordinal))
                return i;
        }

        return -1;
    }

    /// <summary>
    /// The managed delegate type of the <paramref name="index"/>'th method viewed as
    /// <c>reflect.Type.Method(i).Type</c> — the Go func type WITH the receiver as its first
    /// parameter for a concrete type, and WITHOUT one for an interface method (Go's contract).
    /// </summary>
    /// <param name="t">Managed type standing for the Go type.</param>
    /// <param name="index">Method index.</param>
    /// <returns>Delegate type describing the method's Go signature.</returns>
    public static Type GoMethodFuncType(Type? t, int index)
    {
        MethodInfo method = MethodAt(t, index).Method;
        ParameterInfo[] parameters = method.GetParameters();

        // Go's Type.Method(i).Func describes "a function whose first argument is the receiver", and
        // that receiver is the type the method was LOOKED UP ON, never the one it was declared with
        // — reflect's own rtype.Method builds the signature with `in = append(in, t)`, t being the
        // type Method was called on. A VALUE-receiver method belongs to *T's method set as well as
        // T's, so reaching it through TypeOf(&p) must present *Point as argument 0. Presenting the
        // declared Point there is what made Value.Call reject ValueOf(&p) with
        // "reflect: Call using *Point as type Point" (TestMethod, TestDirectIfaceMethod).
        //
        // Exactly one substitution, and only where it is provable: t is a POINTER whose pointee is
        // precisely the declared receiver. A pointer-receiver method already declares ж<X> and is
        // untouched; an interface method carries no receiver parameter to substitute, which is also
        // Go's rule (interfaceType.Method prepends nothing).
        //
        // The receiver compared is the one GO sees: a copy-bound `this ref X` method (see
        // IsCopyBoundReceiver) is a value-receiver method of X, so its by-ref is read through.
        golib.TypeExtensions.IsCopyBoundReceiver(method, out Type receiverType);

        if (t is not null && parameters.Length > 0 &&
            KindOf(t) == Pointer && ElementType(t) == receiverType)
        {
            return s_pointerReceiverFuncTypes.GetOrAdd((t, method), static key =>
            {
                Type[] ins = [.. key.Method.GetParameters().Select(static p => p.ParameterType)];
                ins[0] = key.Receiver;
                return MakeDelegateType(ins, key.Method.ReturnType);
            });
        }

        // A copy-bound receiver presents its VALUE type as argument 0 — Go's func(X, …) — where the
        // declared `ref X` has no delegate form at all (the reflect walk over a value whose only
        // shape for a promoted pointer-receiver method is the by-ref forwarder died here).
        return s_methodFuncTypes.GetOrAdd(method, static m =>
        {
            Type[] ins = [.. m.GetParameters().Select(static p => p.ParameterType)];

            if (golib.TypeExtensions.IsCopyBoundReceiver(m, out Type valueReceiver))
                ins[0] = valueReceiver;

            return MakeDelegateType(ins, m.ReturnType);
        });
    }

    /// <summary>
    /// The <paramref name="index"/>'th method as an UNBOUND func value — receiver first — which is
    /// <c>reflect.Type.Method(i).Func</c>. An interface method has no such value (Go leaves
    /// <c>Method.Func</c> the zero Value there), reported as <c>null</c>.
    /// </summary>
    /// <param name="t">Managed type standing for the Go type.</param>
    /// <param name="index">Method index.</param>
    /// <returns>The unbound delegate, or <c>null</c> for an interface method.</returns>
    public static Delegate? GoMethodFunc(Type? t, int index)
    {
        GoMethodSetEntry entry = MethodAt(t, index);

        if (entry.Method.DeclaringType is { IsInterface: true })
            return null;

        Type funcType = GoMethodFuncType(t, index);
        ParameterInfo[] parameters = entry.Method.GetParameters();

        // When GoMethodFuncType retyped the receiver to the POINTER it was looked up through (see
        // there), the method itself still takes the pointee by value, so CreateDelegate would refuse
        // the signature. Compile a thin adapter that dereferences the box and calls through — the
        // same expression-compiled route the bound factories use, and keyed by receiver AND method
        // for the same reason the type cache is.
        //
        // A copy-bound `this ref X` method (IsCopyBoundReceiver) is read through to its value
        // receiver here too, and takes the same adapter with nothing to dereference: argument 0 is
        // the lambda's own by-value parameter, so the by-ref call lands on a COPY — Go's value
        // receiver. An expression tree passes a non-addressable argument to a by-ref parameter
        // through a temporary, which is that same copy for the dereferenced arm.
        bool copyBound = golib.TypeExtensions.IsCopyBoundReceiver(entry.Method, out Type receiverType);

        if (t is not null && parameters.Length > 0 &&
            KindOf(t) == Pointer && ElementType(t) == receiverType)
        {
            return s_pointerReceiverFuncs.GetOrAdd((t, entry.Method), key =>
            {
                ParameterInfo[] ps = key.Method.GetParameters();
                ParameterExpression[] args = new ParameterExpression[ps.Length];
                args[0] = Expression.Parameter(key.Receiver, "recv");

                for (int i = 1; i < ps.Length; i++)
                    args[i] = Expression.Parameter(ps[i].ParameterType, ps[i].Name);

                MethodInfo read = typeof(GoReflect).GetMethod(nameof(ReadPointerSlot),
                    BindingFlags.Public | BindingFlags.Static)!;

                Expression[] call = new Expression[ps.Length];
                // ReadPointerSlot handles every box shape the bridge mints, so the adapter does not
                // need to know which one it was handed.
                call[0] = Expression.Convert(
                    Expression.Call(read, Expression.Convert(args[0], typeof(object))),
                    receiverType);

                for (int i = 1; i < ps.Length; i++)
                    call[i] = args[i];

                return Expression.Lambda(funcType, Expression.Call(key.Method, call), args).Compile();
            });
        }

        if (copyBound)
        {
            return s_methodFuncs.GetOrAdd(entry.Method, m =>
            {
                ParameterInfo[] ps = m.GetParameters();
                ParameterExpression[] args = new ParameterExpression[ps.Length];
                args[0] = Expression.Parameter(receiverType, "recv");

                for (int i = 1; i < ps.Length; i++)
                    args[i] = Expression.Parameter(ps[i].ParameterType, ps[i].Name);

                return Expression.Lambda(funcType, Expression.Call(m, args), args).Compile();
            });
        }

        return s_methodFuncs.GetOrAdd(entry.Method, m => Delegate.CreateDelegate(funcType, m));
    }

    /// <summary>
    /// Binds <paramref name="receiver"/> to the <paramref name="index"/>'th method of
    /// <paramref name="staticType"/>'s method set and returns the resulting METHOD VALUE — a
    /// delegate carrying the Go signature with the receiver removed, which is what
    /// <c>reflect.Value.Method(i)</c> hands back.
    /// </summary>
    /// <param name="staticType">The Value's type — the type whose method table <paramref name="index"/> indexes.</param>
    /// <param name="index">Method index.</param>
    /// <param name="receiver">The receiver value to bind.</param>
    /// <returns>The bound method value.</returns>
    /// <remarks>
    /// An INTERFACE-typed slot indexes the interface's table but dispatches to the DYNAMIC value,
    /// exactly as Go does: the name is taken from the interface table and re-resolved against the
    /// receiver's own method set, so a method value obtained through an interface calls the same
    /// implementation a direct call would. A value-receiver method reached through a pointer takes a
    /// COPY of the pointee (Go's rule), which is why the box is dereferenced when the receiver
    /// parameter is the element type rather than the box.
    /// </remarks>
    public static Delegate GoMethodValue(Type? staticType, int index, object? receiver)
    {
        object? bindTarget = GoDynamicValueOf(receiver);
        GoMethodSetEntry entry;

        if (IsGoInterfaceType(staticType))
        {
            GoMethodSetEntry interfaceEntry = MethodAt(staticType, index);
            string name = interfaceEntry.GoName;

            // Go's methodReceiver decides this refusal from the INTERFACE's own table, before it
            // resolves anything against the dynamic value (reflect/value.go, the abi.Interface arm).
            // Mirroring that order matters: the answer must not depend on whether the dynamic type
            // happens to expose the method, or a same-package receiver that DOES expose it would
            // dispatch where Go refuses.
            //
            // The refusal does not belong at THIS call, though. Go's Value.Method(i) succeeds — it
            // only sets flagMethod — and the panic comes later, from Call, recoverably. So it is bound
            // INTO the returned method value and raised on INVOCATION, which puts it back exactly
            // where Go has it and leaves the Value itself obtainable.
            //
            // Throwing MissingMethodException here got BOTH halves wrong, and the second is why the
            // symptom was unrecognizable: a managed exception is not a Go panic, so the deferred
            // recover() in reflect's own shouldPanic saw nil and raised its own "did not panic",
            // masking the real exception entirely. A wrong-KIND failure does not even surface as
            // itself — which is the argument for the thunk over any eager throw.
            //
            // An interface's table carries its unexported methods — go2cs emits them as ordinary C#
            // interface members, so NumMethod counts them as Go documents. A concrete type's table is
            // exported-only, in golib BY DESIGN (GetGoMethodSetEntries skips a non-uppercase projected
            // name) and in Go by ExportedMethods(), which is why the non-interface arm below needs no
            // equivalent: an exported-only list cannot yield an unexported name.
            //
            // Go's OTHER interface-arm refusal — "of method on nil interface value" — is deliberately
            // absent: Value.Method already refuses a nil interface receiver before reaching here, as
            // Go's own Value.Method does, so an arm for it would be unreachable.
            if (name.Length > 0 && char.IsLower(name[0]))
                return UnexportedMethodRefusal(interfaceEntry, "reflect: Call of unexported method");

            Type dynamicType = GoDynamicTypeOf(bindTarget);
            int dynamicIndex = GoMethodIndex(dynamicType, name);

            // Past both of Go's refusals, a miss has no Go meaning left — it is a genuine lookup
            // failure and still fails loud, rather than being swallowed by the arms above.
            if (dynamicIndex < 0)
                throw new MissingMethodException(GoTypeName(dynamicType), name);

            entry = MethodAt(dynamicType, dynamicIndex);
        }
        else
        {
            entry = MethodAt(staticType, index);
        }

        // The receiver GO sees: a copy-bound `this ref X` method is a value-receiver method of X.
        golib.TypeExtensions.IsCopyBoundReceiver(entry.Method, out Type receiverType);

        // A value-receiver method reached through *X receives a COPY of the pointee (Go's rule).
        // Fix W: the box test walks the base chain, so a per-kind subclass instance still takes
        // the pointee-copy read (@unsafe.Pointer is inert here — it has no methods to reach).
        if (bindTarget is not null && !receiverType.IsInstanceOfType(bindTarget) &&
            TryBoxPointee(bindTarget.GetType(), out _))
            bindTarget = ReadPointerSlot(bindTarget);

        return s_boundFactories.GetOrAdd(entry.Method, static m => CompileBoundFactory(m))(bindTarget);
    }

    private static readonly ConcurrentDictionary<Type, Func<Func<object?[], object?>, Delegate>> s_goFuncDelegateFactories = [];

    /// <summary>
    /// Builds a delegate of EXACTLY <paramref name="delegateType"/> whose invocation boxes its
    /// arguments into an <c>object?[]</c>, hands them to <paramref name="invoker"/>, and returns the
    /// invoker's result cast to the delegate's declared return type — the delegate-side half of
    /// <c>reflect.MakeFunc</c>, and the exact inverse of <c>Value.Call</c>'s DynamicInvoke direction.
    /// </summary>
    /// <param name="delegateType">The delegate type the result must have (a converted Go func type).</param>
    /// <param name="invoker">Receives the boxed arguments; its result becomes the call's result
    /// (ignored for a void delegate, a <c>ValueTuple</c> for a Go multi-return).</param>
    /// <returns>A delegate of <paramref name="delegateType"/> backed by <paramref name="invoker"/>.</returns>
    /// <remarks>
    /// Expression-compiled for the same reason the method-value binder is (see this file's header):
    /// the wrapper must exist for ANY delegate type, and only a compiled lambda can take on an
    /// arbitrary delegate's exact signature. The LAMBDA is compiled once per delegate type into a
    /// factory (outer lambda takes the invoker, inner IS the typed delegate), so per-MakeFunc cost is
    /// a closure allocation rather than a compile — the s_boundFactories memoization rule. The cache
    /// is keyed by the delegate type and is correctly absent from ClearTypeCaches' list: a delegate
    /// type's Invoke signature is fixed when its type loads. A VARIADIC func type is refused loud:
    /// its tail is <c>params Span&lt;T&gt;</c>, a byref-like type expression trees reject outright —
    /// the route that exists is the reverse of <see cref="InvokeVariadic"/>'s typed family
    /// trampolines (one generic factory per golib <c>Actionꓸꓸꓸ</c>/<c>Funcꓸꓸꓸ</c> arity, rebound onto
    /// the natural type through <c>Invoke</c>), left unbuilt for want of a demonstrated consumer.
    /// </remarks>
    public static Delegate MakeGoFuncDelegate(Type delegateType, Func<object?[], object?> invoker)
    {
        return s_goFuncDelegateFactories.GetOrAdd(delegateType, static t => CompileGoFuncFactory(t))(invoker);
    }

    // Compiles the ONE-per-delegate-type wrapper factory: an outer lambda taking the invoker, whose
    // body IS the typed delegate boxing its arguments through to that invoker.
    private static Func<Func<object?[], object?>, Delegate> CompileGoFuncFactory(Type delegateType)
    {
        MethodInfo? invoke = typeof(Delegate).IsAssignableFrom(delegateType) ? delegateType.GetMethod("Invoke") : null;

        if (invoke is null)
            throw new ArgumentException($"'{GoTypeName(delegateType)}' is not a delegate type", nameof(delegateType));

        ParameterInfo[] parameters = invoke.GetParameters();

        // `Array` alone is the GoReflect.Array Kind constant in this scope, so System.Array is qualified.
        // A byref-like `params Span<T>` variadic tail cannot ride an expression tree, so the typed
        // makeVariadicFunc{N}/makeVariadicAction{N} family carries it in a LAMBDA instead -- the reverse
        // of InvokeVariadic's call family (GoReflect.MakeVariadicDelegate.cs).
        if (System.Array.Exists(parameters, static p => p.ParameterType.IsByRef || p.ParameterType.IsByRefLike))
        {
            return BuildVariadicMakeFactory(delegateType, invoke, parameters);
        }

        ParameterExpression invokerParameter = Expression.Parameter(typeof(Func<object?[], object?>), "invoker");
        ParameterExpression[] arguments = [.. parameters.Select(static (p, i) => Expression.Parameter(p.ParameterType, p.Name ?? $"arg{i}"))];

        Expression argumentArray = arguments.Length == 0
            ? Expression.Constant(System.Array.Empty<object?>(), typeof(object[]))
            : Expression.NewArrayInit(typeof(object), arguments.Select(static a => Expression.Convert(a, typeof(object))));

        Expression body = Expression.Invoke(invokerParameter, argumentArray);

        if (invoke.ReturnType != typeof(void))
            body = Expression.Convert(body, invoke.ReturnType);

        LambdaExpression made = Expression.Lambda(delegateType, body, arguments);

        return Expression.Lambda<Func<Func<object?[], object?>, Delegate>>(Expression.Convert(made, typeof(Delegate)), invokerParameter).Compile();
    }

    // The ordered method table of the Go type `t` stands for — the ONE source every answer above
    // reads (see this file's header). An adapter shell answers as the Go dynamic type it stands for
    // (R10); the EMPTY interface (`any` is object) has no methods.
    private static GoMethodSetEntry[] GoMethodTable(Type? t)
    {
        if (t is null || t == typeof(object))
            return [];

        if (TryAdapterWrappedType(t, out Type? adapterWrapped, out bool adapterPointerSourced))
        {
            return adapterPointerSourced
                ? golib.TypeExtensions.GetGoMethodSetEntries(adapterWrapped, valueIsPointer: true)
                : GoMethodTable(adapterWrapped);
        }

        if (t.IsInterface)
            return golib.TypeExtensions.GetGoInterfaceMethodEntries(t);

        golib.TypeExtensions.ResolveReceiverElement(t, out Type element, out bool isPointer);

        return golib.TypeExtensions.GetGoMethodSetEntries(element, isPointer);
    }

    private static GoMethodSetEntry MethodAt(Type? t, int index)
    {
        GoMethodSetEntry[] table = GoMethodTable(t);

        if (index < 0 || index >= table.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        return table[index];
    }

    // Whether the Go type `t` stands for is an interface — including the EMPTY interface, which is
    // System.Object and reports IsInterface false (the same arm KindOf carries).
    private static bool IsGoInterfaceType(Type? t)
    {
        if (t is null)
            return false;

        if (TryAdapterWrappedType(t, out Type? wrapped, out bool pointerSourced))
            return !pointerSourced && IsGoInterfaceType(wrapped);

        return t.IsInterface || t == typeof(object);
    }

    // Unwraps a value to the one its GO DYNAMIC TYPE describes — the value-level mirror of
    // GoDynamicTypeOf: an interface-carrier chain unwraps to the original value, a pointer-sourced
    // adapter to its receiver box, a value-sourced adapter to the struct copy it wraps.
    //
    // The value-adapter tier unwraps UNCONDITIONALLY, unlike GoDynamicTypeOf's own null-guarded
    // gate: a nil-wrapped delegate's Go dynamic VALUE genuinely is null (a nil Greeter IS a null
    // delegate), and the sole caller, GoMethodValue, binds this result as the receiver Expression
    // it compiles against the delegate's OWN type — the shell is never assignable to it. Left
    // gated here as GoDynamicTypeOf is, a nil-wrapped receiver reached GoMethodValue as the shell
    // and crashed with an uncaught InvalidCastException from inside the compiled receiver cast
    // ("Unable to cast object of type 'GreeterᴠGreetable' to type 'Greeter'") — never reaching
    // Go's real panic at all. Unwrapping to null instead lets the cast succeed and the eventual
    // call correctly fail invoking a null delegate, which Go's own semantics already expect
    // (calling a nil func panics with "invalid memory address or nil pointer dereference").
    // Rhymes with the registererr chip's Defect A (GoDynamicTypeOf): the same null-Value shell
    // leaking into a path that needs the value it stands for, not the wrapper itself.
    private static object? GoDynamicValueOf(object? value)
    {
        while (value is IInterfaceAdapter { Value: not null } interfaceAdapter)
            value = interfaceAdapter.Value;

        if (value is IжAdapter { Box: not null } pointerAdapter)
            return pointerAdapter.Box;

        if (value is IValueAdapter valueAdapter)
            return valueAdapter.Value;

        return value;
    }

    // Compiles the ONE-per-method receiver binder: an outer lambda taking the receiver as `object`,
    // whose body IS the bound Go-signature delegate. Compiling once and closing per bind keeps a
    // method value cheap; see this file's header for why CreateDelegate cannot serve here.
    private static Func<object?, Delegate> CompileBoundFactory(MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();

        // A copy-bound `this ref X` receiver (IsCopyBoundReceiver) binds as the value receiver it is
        // in Go: the converted receiver below is not addressable, so the expression tree hands the
        // by-ref parameter a TEMPORARY — a fresh copy per call, which is Go's value-receiver rule.
        golib.TypeExtensions.IsCopyBoundReceiver(method, out Type receiverType);

        // What is left by-ref is a [GoRecv] POINTER-receiver method with no ж<T> twin beside it. It
        // has no value to copy from, so it is refused — as a Go PANIC, the form recover() sees. It
        // was a NotImplementedException, which no Go frame adopts (RuntimeErrorPanic.TryAsPanic), so
        // a deferred recover() watched it go by and the process died on a managed exception instead.
        if (receiverType.IsByRef)
        {
            throw builtin.panic(
                $"reflect: method value for '{method.DeclaringType?.Name}.{method.Name}' — the only emitted shape " +
                "takes its receiver by reference, which a delegate cannot carry (the generated ж<T> overload is absent)");
        }

        ParameterExpression receiver = Expression.Parameter(typeof(object), "receiver");
        ParameterExpression[] arguments = [.. parameters.Skip(1).Select(static (p, i) => Expression.Parameter(p.ParameterType, p.Name ?? $"arg{i}"))];
        Type delegateType = MakeDelegateType([.. arguments.Select(static a => a.Type)], method.ReturnType);

        Expression body = Expression.Call(method, [Expression.Convert(receiver, receiverType), .. arguments]);
        LambdaExpression bound = Expression.Lambda(delegateType, body, arguments);

        return Expression.Lambda<Func<object?, Delegate>>(Expression.Convert(bound, typeof(Delegate)), receiver).Compile();
    }

    // A METHOD VALUE that carries one of Go's two interface-method refusals instead of an
    // implementation: the delegate has the method's own signature, so it is an ordinary method value
    // to everything that inspects it, and raises the panic only when INVOKED — which is where Go
    // raises it. Go's Value.Method(i) hands back a refusing method value just like this one; nothing
    // observes the difference until Call.
    //
    // The panic state is a bare string, the form the converter emits for a Go string panic
    // (`throw panic("unknown type kind")` and its ~200 siblings in the corpus), so recover() sees
    // what it sees for any other reflect panic.
    private static Delegate UnexportedMethodRefusal(GoMethodSetEntry entry, string message) =>
        s_refusalThunks.GetOrAdd((entry.Method, message), static key => CompileRefusalThunk(key.Method, key.Message));

    private static readonly ConcurrentDictionary<(MethodInfo Method, string Message), Delegate> s_refusalThunks = new();

    private static Delegate CompileRefusalThunk(MethodInfo method, string message)
    {
        // An INTERFACE method's receiver is `this`, not a leading parameter — unlike the static
        // emission CompileBoundFactory takes apart — so every parameter here is a real one and none
        // is skipped. The signature is reproduced exactly rather than approximated, because Call
        // measures the method value against it before the body ever runs.
        ParameterExpression[] arguments =
            [.. method.GetParameters().Select(static (p, i) => Expression.Parameter(p.ParameterType, p.Name ?? $"arg{i}"))];

        Type delegateType = MakeDelegateType([.. arguments.Select(static a => a.Type)], method.ReturnType);

        MethodInfo panicMethod = typeof(builtin).GetMethod(nameof(builtin.panic), [typeof(object)])!;

        Expression body = Expression.Throw(
            Expression.Call(panicMethod, Expression.Constant(message, typeof(object))),
            method.ReturnType);

        return Expression.Lambda(delegateType, body, arguments).Compile();
    }

    // The delegate type for a Go signature — a DECLARED family wherever one exists, a MINTED type
    // for the residual.
    //
    // DECLARED-FIRST IS A CONTRACT, NOT A PREFERENCE. Go INTERNS func types, so `FuncOf(in, out,
    // variadic)` must answer the SAME type a converted package spells that signature with — which is
    // exactly what reflect's own TestFuncOf asserts (checkSameType). A converted `func(T1) string`
    // IS `Func<T1, @string>`, so a freshly minted type of the same shape would be a different
    // System.Type and the identity would fail. Three families declare, in widening order: the BCL's
    // Func<>/Action<> through 16 parameters, golib's own ladder continuing them through 24
    // (funcArity.cs), and the variadic families whose tail is `params Span<T>` through 8 fixed
    // parameters (variadic.cs). Every one of those ceilings is ASKED OF THE METADATA below rather
    // than restated here, so a rung added to either file is reached the day it is written and this
    // routing cannot drift from what the corpus actually binds.
    //
    // Past every ceiling nothing declares a delegate — and nothing could, since a converted package
    // could not spell one either — so the type is MINTED (GoDelegateSynthesis). Go's own limit is 128
    // parameters, and its reflect suite drives a 51-parameter FuncOf (all_test.go, issue #54669).
    //
    // A shape no family can take as a TYPE ARGUMENT — a by-ref or pointer parameter — still fails
    // LOUD, at every width, rather than yielding a delegate that would misinterpret the signature.
    // The mint is reached by WIDTH, never by shape: an expression tree carries neither of those, so
    // a minted delegate of that shape is one MakeGoFuncDelegate and CompileBoundFactory would refuse
    // anyway, and a type that exists but cannot be bound is worse than the refusal this has always
    // been. The `Span<T>` variadic tail is the ONE exception, and it is not a shape the mint invents:
    // the variadic families carry it themselves and InvokeVariadic's typed trampolines consume it.
    private static Type MakeDelegateType(Type[] parameterTypes, Type returnType)
    {
        bool isAction = returnType == typeof(void);

        // A trailing `Span<T>` IS Go's variadic tail. MakeGoFuncType writes it and TryFuncShape reads
        // it; this is the third and last place that convention is known, and it is read here only to
        // pick the family — the tail is passed to the mint untouched. The variadic families take the
        // tail's ELEMENT as their type argument, because a byref-like type cannot be one at all.
        if (parameterTypes.Length > 0 && parameterTypes[^1] is { IsGenericType: true } tail &&
            tail.GetGenericTypeDefinition() == typeof(Span<>))
        {
            int fixedArity = parameterTypes.Length - 1;

            Type? family = GolibFamily((isAction ? "go.Action" : "go.Func") + EllipsisOperator + '`' +
                                       (isAction ? fixedArity + 1 : fixedArity + 2));

            if (family is null)
                return GoDelegateSynthesis.SynthesizeDelegateType(parameterTypes, returnType);

            Type[] withElement = [.. parameterTypes[..fixedArity], tail.GetGenericArguments()[0]];
            Type[] arguments = isAction ? withElement : [.. withElement, returnType];

            return InstantiateFamily(family, arguments, parameterTypes, returnType);
        }

        // The BCL family, through its own ceiling — the route every signature the corpus has ever
        // produced takes, unchanged, and the one whose loud refusal a by-ref or pointer parameter
        // meets.
        if (BclDeclaresFamily(parameterTypes.Length, isAction))
        {
            try
            {
                return isAction
                    ? Expression.GetActionType(parameterTypes)
                    : Expression.GetFuncType([.. parameterTypes, returnType]);
            }
            catch (Exception ex) when (ex is ArgumentException or TypeLoadException)
            {
                throw NoDelegateForm(parameterTypes, returnType, ex);
            }
        }

        Type? rung = GolibFamily((isAction ? "go.Action`" : "go.Func`") +
                                 (isAction ? parameterTypes.Length : parameterTypes.Length + 1));

        if (rung is null)
            return GoDelegateSynthesis.SynthesizeDelegateType(parameterTypes, returnType);

        return InstantiateFamily(rung, isAction ? parameterTypes : [.. parameterTypes, returnType], parameterTypes, returnType);
    }

    // One family rung instantiated, with the SAME loud refusal the BCL arm raises — a parameter a
    // generic family cannot take is exactly what MakeGenericType rejects here.
    private static Type InstantiateFamily(Type family, Type[] arguments, Type[] parameterTypes, Type returnType)
    {
        try
        {
            return family.MakeGenericType(arguments);
        }
        catch (Exception ex) when (ex is ArgumentException or TypeLoadException)
        {
            throw NoDelegateForm(parameterTypes, returnType, ex);
        }
    }

    // The refusal, unchanged in text: a signature that reaches it has no delegate form the bridge can
    // bind, and `reflect.FuncOf` re-raises this message as its own panic.
    private static NotImplementedException NoDelegateForm(Type[] parameterTypes, Type returnType, Exception inner)
    {
        return new NotImplementedException(
            $"reflect: Go method signature ({string.Join(", ", parameterTypes.Select(static p => p.Name))}) -> " +
            $"{returnType.Name} has no Func<>/Action<> delegate form", inner);
    }

    private static readonly ConcurrentDictionary<string, Type?> s_declaredFamilies = new(StringComparer.Ordinal);

    // Whether the BCL declares a delegate family rung of this width — i.e. where
    // Expression.GetActionType/GetFuncType stop. Asked of corelib rather than written down as a 16
    // here: a ceiling recorded in two places drifts, and this one is not ours to record. (`Func` with
    // no parameters is `Func`1`, so the formula covers arity zero; `Action` with none is the
    // non-generic type, which is the one name that carries no arity suffix.)
    private static bool BclDeclaresFamily(int arity, bool isAction)
    {
        string name = isAction
            ? arity == 0 ? "System.Action" : "System.Action`" + arity
            : "System.Func`" + (arity + 1);

        return DeclaredFamily(typeof(object).Assembly, name) is not null;
    }

    // A DECLARED family rung, by metadata name, memoized: the routing asks per call and the answer is
    // fixed when the assembly loads — the s_boundFactories rule, and correctly absent from
    // ClearTypeCaches for the same reason (a late-loading assembly cannot add a rung to corelib or to
    // golib itself).
    private static Type? GolibFamily(string name)
    {
        return DeclaredFamily(typeof(GoReflect).Assembly, name);
    }

    private static Type? DeclaredFamily(Assembly assembly, string name)
    {
        return s_declaredFamilies.GetOrAdd(name, (n, a) => a.GetType(n, throwOnError: false), assembly);
    }
}

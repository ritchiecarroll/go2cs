// GoReflect.ValueMarshalling.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using go.golib;

namespace go;

// ---------------------------------------------------------------------------------------------
// VALUE MARSHALLING — producing a value of a target Go type.
//
// WHAT LIVES HERE
//   The two ways the bridge produces one, kept together because they share the same rules and the
//   same wrapper machinery:
//     * CONVERT an existing value — `GoImplements`, `TryMarshalAssignable` (the write half of
//       `reflect.Value.Set`), `TryConvertTo` (the `Set{Int,Uint,Float,Complex,String,Bool}` and
//       `Convert` family), and the scalar coercion under them.
//     * FABRICATE one from nothing — `CanonicalNilPointer`, `ZeroValueOf`/`DefaultValueOf`
//       (`reflect.Zero`, `SetZero`), `MakeSizedArray`, `MakeContainer` (`reflect.MakeSlice`/
//       `MakeMap`), `NewPointerBox` (`reflect.New`).
//   Fabrication is the degenerate case of conversion — "make me the value of this type that
//   carries no information" — and it reaches the same wrapper constructors, which is why splitting
//   the two apart would put one rule in two files.
//
// THE ZERO VALUE IS PER KIND, AND `default` IS OFTEN WRONG
//   Go's zero value has one encoding per kind and the managed representations do not line up:
//     * a pointer zero is the CANONICAL typed-nil instance, not a fresh nil box and not `null` —
//       one nil encoding system-wide is what makes `any((*T)(nil)) != nil` and `%T` on a typed nil
//       work at all;
//     * an interface or func zero IS `null` (a nil interface has no type; a nil func is a null
//       delegate);
//     * a slice/map/chan zero is the container STRUCT's default, never `Activator.CreateInstance`
//       — the golib containers have explicit parameterless constructors that allocate a NON-nil
//       backing, so `Activator` would hand back an empty-but-non-nil container where Go wants nil;
//     * a struct zero is a default instance and its FIELD INITIALIZERS are part of the Go zero —
//       a blank `_ [4]byte` field materializes its own length there.
//
// TWO TRAPS THAT `System.Object` SETS, BOTH ALREADY PAID FOR
//   Go's empty interface is emitted as `object`, and `typeof(object).IsInterface` is FALSE. Both
//   `GoImplements` and `TryMarshalAssignable` therefore carry an explicit `object` arm ahead of
//   their interface arm; without it `reflect.Type.AssignableTo`/`Implements` answered false for
//   every type against `interface{}`, and `Set` into an `any` slot rejected every value gob had
//   just decoded ("gob: int is not assignable to type interface {}"). `KindOf` in the primary file
//   carries the same arm for the same reason. A new interface-shaped test needs it too.
//
// NAMED WRAPPERS CONVERT IN BOTH DIRECTIONS, AND ONLY THROUGH THEIR CONSTRUCTOR
//   `type TestPtrAlias *int` is a generated wrapper over a raw `ж<int>`. Go's named/unnamed
//   assignability rule — identical underlying types with at least one side unnamed — is
//   implemented by constructing through the wrapper's generated single-argument constructor one
//   way and unwrapping `m_value` the other. Constructing WRAPS THE SAME BOX rather than copying,
//   so pointer identity and write-through survive the round trip. Two DIFFERENT named types match
//   neither arm, which is Go-correct.
//
// SCALAR STORES TRUNCATE — THEY DO NOT PANIC
//   `coerceScalar` widens to `long`/`double` and narrows unchecked, because that is the Go 1.23
//   `reflect` contract for the `Set*` family: no overflow panic on a store. Adding a checked
//   conversion here would introduce a panic Go does not have.
//
// THE HISTORY POINTER
//   The construction/conversion/size/func-shape/field-projection work landed together as the
//   reflection bridge's Phase-3 increment 2; its design and adversarial-review ledger is
//   docs/phase4/DESIGN-reflection-bridge-phase3-plan.md (I2.R). The other halves of that increment
//   now live in GoReflect.TypeLayout.cs and GoReflect.FieldAccess.cs.
// ---------------------------------------------------------------------------------------------
public static partial class GoReflect
{
    /// <summary>
    /// The canonical typed nil instance for a closed <c>ж&lt;T&gt;</c> pointer type (<see cref="ж{T}.NilBox"/>),
    /// resolved from the runtime <see cref="Type"/> — what <c>reflect.Zero</c> of a pointer kind yields.
    /// </summary>
    public static object? CanonicalNilPointer(Type pointerType)
    {
        return s_canonicalNils.GetOrAdd(pointerType, static t =>
        {
            // Fix W with the M exemption: a ж box at any base-chain depth answers the base's
            // static NilBox (read off the WALKED ж<T> type, where the static lives — identical to
            // reading off t itself today, when every box type sits at depth 0). @unsafe.Pointer is
            // exempt and keeps its NilInstance-probe fall-through: its canonical nil is its own,
            // never the raw ж<uintptr> box's.
            if (!typeof(IUnsafePointer).IsAssignableFrom(t) && TryBoxPointee(t, out Type? pointee))
                return typeof(ж<>).MakeGenericType(pointee).GetProperty(nameof(ж<int>.NilBox), BindingFlags.Public | BindingFlags.Static)?.GetValue(null);

            // A generated NAMED pointer wrapper exposes its canonical typed nil as NilInstance
            // (declared internal by the template — probe both visibilities).
            return t.GetProperty("NilInstance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
        });
    }

    private static readonly ConcurrentDictionary<Type, object?> s_canonicalNils = new();

    /// <summary>
    /// The canonical typed nil for a pointer type whose POINTEE is an array of the given dims -- the
    /// dims-carrying nil the language mints for <c>(*[N]T)(nil)</c> (<see cref="ж{T}.NilBoxOfDims"/>),
    /// so a reflect-made nil re-describes with its length. Falls back to the plain canonical nil
    /// when there are no dims or the type is not a plain <c>ж&lt;T&gt;</c> box.
    /// </summary>
    public static object? CanonicalNilPointer(Type pointerType, nint[]? arrayDims)
    {
        if (arrayDims is not { Length: > 0 } || typeof(IUnsafePointer).IsAssignableFrom(pointerType) || !TryBoxPointee(pointerType, out Type? pointee))
            return CanonicalNilPointer(pointerType);
        long[] dims = new long[arrayDims.Length];
        for (int i = 0; i < dims.Length; i++)
            dims[i] = arrayDims[i];
        MethodInfo? mint = typeof(ж<>).MakeGenericType(pointee).GetMethod(nameof(ж<int>.NilBoxOfDims), BindingFlags.Public | BindingFlags.Static);
        return mint is null ? CanonicalNilPointer(pointerType) : mint.Invoke(null, [dims]);
    }

    /// <summary>
    /// The canonical typed nil for a FUNC type — the second shape of the one-nil-encoding rule
    /// <see cref="CanonicalNilPointer"/> established for pointers. A Go func emits as a managed
    /// delegate, whose nil IS <c>null</c>: correct in every func-typed slot, and type-erasing the
    /// moment it crosses into INTERFACE space, where Go packs (type=func-type, value=nil) — a
    /// NON-nil interface `%T` prints and a type assertion succeeds against with a nil result
    /// (reflectlite's TestFunctionValue/TestTypes measured `&lt;nil&gt;` where Go prints
    /// <c>func()</c>). The carrier exists ONLY at that boundary: the eface packers
    /// (reflect's packInterfaceValue / reflectlite's valueInterface) mint it for a null read out
    /// of a FUNC-kinded slot, and every read-back path — <c>GoDynamicTypeOf</c>, the type
    /// assertion, <c>TryMarshalAssignable</c>, <c>IsNilGoValue</c> — resolves it back to the nil
    /// delegate, so it can never be stored into a func-typed slot or observed as itself.
    /// </summary>
    public static object CanonicalNilFunc(Type delegateType)
    {
        return s_canonicalNilFuncs.GetOrAdd(delegateType, static t => new NilFuncValue(t));
    }

    private static readonly ConcurrentDictionary<Type, NilFuncValue> s_canonicalNilFuncs = new();

    /// <summary>
    /// Go's <c>implements</c> relation over managed types: <paramref name="ifaceType"/> is an
    /// interface that <paramref name="valueType"/> satisfies nominally OR structurally by Go
    /// method-set rules — the SAME probe the emitted <c>_&lt;T&gt;</c> asserts use, so reflection
    /// and direct asserts can never disagree about a method set.
    /// </summary>
    /// <remarks>
    /// Go's EMPTY interface (<c>interface{}</c> / <c>any</c>) is emitted as <c>object</c>, which is
    /// not a CLR interface — but it IS the interface with no methods, and every Go type satisfies
    /// it. <see cref="Type.IsAssignableFrom"/> already answers that (boxing included); only the
    /// <see cref="Type.IsInterface"/> gate stood in the way, so <c>reflect.Type.AssignableTo</c> /
    /// <c>Implements</c> answered FALSE for every type against <c>interface{}</c> — gob's
    /// <c>decodeInterface</c> rejected every concrete value it had just decoded ("gob: int is not
    /// assignable to type interface {}", TestInterfaceBasic / TestInterfacePointer /
    /// TestNestedInterfaces).
    /// </remarks>
    public static bool GoImplements(Type? ifaceType, Type? valueType)
    {
        if (ifaceType is null || valueType is null)
            return false;

        if (!ifaceType.IsInterface && ifaceType != typeof(object))
            return false;

        return ifaceType.IsAssignableFrom(valueType) || valueType.StructurallyImplements(ifaceType);
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Func<object, bool>?> s_nilOperators = new();

    /// <summary>
    /// Go nilness of a boxed container/pointer/func value, through the SAME machinery the
    /// emitted <c>x == nil</c> comparisons use: the structural pointer predicate
    /// (<see cref="INilPointer"/>), the map's representational nilness, or the type's
    /// generated/golib <c>== nil</c> operator (slices raw and named, channels, wrappers) — one
    /// rule, never a second nilness implementation. Home here (moved from reflect's
    /// value_impl.cs, 2026-08-18) so <c>reflect.Value.IsNil</c> and internal/reflectlite's
    /// mirror read one answer: reflectlite's own switch lacked the operator probe, so a nil
    /// slice/chan read out of a struct field answered NOT nil (its TestIsNil rows).
    /// </summary>
    public static bool IsNilGoValue(object? cur)
    {
        switch (cur)
        {
            case null:
                return true;
            case INilPointer nilable:
                return nilable.IsNilPointer;
            case IMap m:
                return m.IsNil;
            case NilFuncValue:
                return true;
        }

        Func<object, bool>? probe = s_nilOperators.GetOrAdd(cur.GetType(), static t =>
        {
            MethodInfo? op = t.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static, [t, typeof(NilType)]);
            return op is null ? null : v => (bool)op.Invoke(null, [v, default(NilType)])!;
        });

        return probe is not null && probe(cur);
    }

    /// <summary>
    /// Which of Go's two type relations the caller is asking about. They are NOT the same question,
    /// and this helper serves both: <c>reflect.Value.Set</c> asks ASSIGNABILITY, while
    /// <see cref="TryConvertTo"/> (<c>reflect.Value.Convert</c> and the <c>Set{Int,Uint,…}</c>
    /// family) asks CONVERTIBILITY, which is strictly more permissive — two DIFFERENT named types
    /// with identical underlying convert (<c>string</c> ⇄ <c>type S string</c>) and do not assign.
    /// </summary>
    /// <remarks>
    /// A census over reflect's own suite measured the split: <b>70,065 of 70,070</b> admits through
    /// this helper's wrapper-constructor and unwrap arms arrive from <c>TryConvertTo</c>, and ZERO
    /// from an assignment caller — so a single rule at those arms would answer whichever question it
    /// was written for and be wrong for the traffic that actually dominates. Passing the mode is
    /// what lets one rule live at the arms without conflating the two relations.
    /// </remarks>
    public enum GoTypeRelation
    {
        /// <summary>Go's CONVERTIBILITY — the historical behaviour, and the default for every
        /// caller that has not been examined and switched deliberately.</summary>
        Convertible = 0,

        /// <summary>Go's ASSIGNABILITY — two different NAMED types never assign, predeclared types
        /// counted named per the spec.</summary>
        Assignable = 1,
    }


    // Go's assignability refusal, spelled ONCE for both named/unnamed arms below: two DIFFERENT
    // NAMED types are never assignable. Predeclared types (`string`, `int8`, `float32`) are NAMED
    // by the spec, which is the point the arms used to miss -- `HasGoName` already answers that
    // correctly, it was simply never asked here. An INTERFACE destination is its own arm (a
    // concrete type IS assignable to an interface it satisfies) and an identity pair has already
    // returned at the identity arm above, so neither reaches this.
    //
    // It applies ONLY under GoTypeRelation.Assignable: under Convertible these very pairs are legal
    // (`string` -> `type S string` is a conversion Go performs), and refusing them here would ask
    // the assignment question of conversion traffic -- which a census measured as 70,065 of 70,070
    // admits through these arms.
    private static bool RefusedByGoAssignability(GoTypeRelation relation, Type srcType, Type dstType)
    {
        return relation == GoTypeRelation.Assignable &&
               srcType != dstType && !dstType.IsInterface &&
               HasGoName(srcType) && HasGoName(dstType);
    }
    /// <summary>
    /// Marshals a bridge Value's live source object for assignment into a destination slot of
    /// <paramref name="dstType"/> under Go assignability (identity, or interface-implements) —
    /// the write half of <c>reflect.Value.Set</c>. Returns false when Go would panic
    /// ("value of type X is not assignable to type Y").
    /// </summary>
    /// <param name="relation">
    /// Which relation to enforce at the named/unnamed arms (see <see cref="GoTypeRelation"/>).
    /// Defaults to <see cref="GoTypeRelation.Convertible"/> so an un-examined caller keeps exactly
    /// today's behaviour; the assignment entry points pass <see cref="GoTypeRelation.Assignable"/>.
    /// </param>
    public static bool TryMarshalAssignable(object? src, Type dstType, out object? marshalled,
                                            GoTypeRelation relation = GoTypeRelation.Convertible)
    {
        marshalled = null;

        // `any` destination — Go's EMPTY interface holds the value DIRECTLY, and every type is
        // assignable to it. System.Object reports IsInterface false (the same trap KindOf and
        // TryConvertTo already carry an explicit arm for), so without this the interface arm below
        // never fired and reflect.Value.Set into an `any` slot rejected every concrete value gob
        // had just decoded ("reflect.Set: value of type int is not assignable to type
        // interface {}" — TestInterfaceBasic / TestInterfacePointer / TestNestedInterfaces).
        if (dstType == typeof(object))
        {
            marshalled = src;
            return true;
        }

        // A valid-but-nil SOURCE: assignable to an interface destination (stays the nil
        // interface) and to any REFERENCE-typed slot (a null slot value IS the nil pointer/
        // func — X2.3 equality treats null-reference and the canonical nil box as one nil);
        // never to a value-typed slot (a nil-able container STRUCT's nil is its default value,
        // which is a non-null source).
        if (src is null)
        {
            if (dstType.IsInterface)
                return true;

            marshalled = null;
            return !dstType.IsValueType;
        }

        // The CANONICAL NIL FUNC resolves back to the nil delegate at every store: into its own
        // delegate type as null (the slot representation of a nil func), never into a different
        // func type — Go's identity rule, since two distinct func types are never assignable.
        if (src is NilFuncValue nilFunc)
        {
            marshalled = null;
            return dstType == nilFunc.Type;
        }

        // A DIRECTIONAL channel value is not assignable to a channel slot the C# type erases to the
        // bidirectional `channel<T>`: Go refuses a `<-chan T`/`chan<- T` SOURCE flowing into a
        // `chan T` result (a directional channel cannot widen). The slot carries no direction here
        // (`channel<T>` IS the bidirectional representation), so a stamped-directional source is
        // rejected treating the slot as bidirectional — the case reflect's
        // TestMakeFuncInvalidReturnAssignments asserts (a `RecvOnly` channel returned into a
        // `chan int` result must panic). A BIDIRECTIONAL source (Unstamped) never trips this and
        // narrows into a directional slot freely (the valid direction — the identity arm below
        // admits it). This arm is INERT until the converter's live-copy narrowing stamp makes a
        // source directional at all: the two halves of one cut, and a census found ZERO directional
        // channel sources marshalled today, so it can regress none of the 108 current admits.
        if (src is IChannel { Direction: not GoChanDir.Unstamped } &&
            typeof(IChannel).IsAssignableFrom(dstType))
        {
            marshalled = null;
            return false;
        }

        // Identity — including a pointer-sourced interface value unwrapping to its receiver box
        // (Go: the interface holds the *T) and the canonical typed-nil box of the same type.
        object dynamicSrc = src;

        while (dynamicSrc is IInterfaceAdapter { Value: not null } interfaceAdapter)
            dynamicSrc = interfaceAdapter.Value;

        if (dynamicSrc is IжAdapter { Box: not null } pointerAdapter)
            dynamicSrc = pointerAdapter.Box;
        else if (dynamicSrc is IValueAdapter { Value: not null } valueAdapter)
            dynamicSrc = valueAdapter.Value;

        // Subsumption rather than exact equality (fix W: a per-kind box subclass instance fills a
        // declared ж<T> slot), with the N5 M-guard: @unsafe.Pointer must never marshal into a
        // ж<uintptr> destination — it is not an ordinary *uintptr, and plain subsumption would
        // admit it. Its own exact type remains assignable to itself.
        if (dstType.IsAssignableFrom(dynamicSrc.GetType()) &&
            (dynamicSrc.GetType() == dstType || dynamicSrc is not IUnsafePointer))
        {
            marshalled = dynamicSrc;
            return true;
        }

        // Go assignability's named↔unnamed rule: identical UNDERLYING types with at least one
        // side unnamed. A raw value assigning into a NAMED wrapper slot constructs the wrapper
        // through its generated single-argument constructor (`reflect.New(TestPtrAlias.Elem())`
        // yields a raw *int; `Set` into a `type TestPtrAlias *int` slot wraps the SAME box, so
        // pointer identity and write-through survive); a named wrapper assigning into its raw
        // underlying slot unwraps. Two DIFFERENT named types never match either arm (Go-correct).
        ConstructorInfo? dstWrapperCtor = wrapperConstructorOf(dstType);

        if (dstWrapperCtor is not null)
        {
            // SUBSUMPTION, not exact equality — the same relation (and the same N5 M-guard) as the
            // two arms either side of this one, and for the same reason: a live value of an
            // underlying type is routinely a SUBCLASS instance rather than the declared type itself.
            //
            // For a POINTER underlying that is not an edge case, it is the ONLY case. The parameter
            // type is `ж<T>`, which is ABSTRACT, so no value can ever equal it exactly — every live
            // pointer is a StandardBox<T> or another box subclass. The exact test was therefore
            // UNSATISFIABLE for every `type P *T`, and the whole named/unnamed rule was dead for the
            // pointer kind: a plain `*int` was rejected by a `type TestPtrAlias *int` slot, which is
            // exactly the assignment Go permits (identical underlying types, the source side
            // unnamed). testing/quick's TestCheckEqual died on it — `reflect.Set: value of type *int
            // is not assignable to type quick.TestPtrAlias` — inside quick.Value's
            // `v.Set(reflect.New(concrete.Elem()))`.
            //
            // Go's method-set interference clause cannot bite for this kind: a named POINTER type
            // can carry no methods AT ALL, because a receiver base type may not be a pointer type
            // (spec, "Method declarations"). So there is no method set on either side to lose, and
            // admitting the assignment cannot make an interface satisfaction appear or vanish.
            //
            // The M-guard rides along because `unsafe.Pointer` derives from `StandardBox<uintptr>`:
            // without it, subsumption would wrap it into a `type P *uintptr` slot it is not
            // assignable to. Two DIFFERENT named types still match neither arm — a generated wrapper
            // has no base class, so no wrapper is ever a subclass of another wrapper or of `ж<T>`
            // (guarded by NamedPointerAssignabilityTests).
            Type underlyingParam = dstWrapperCtor.GetParameters()[0].ParameterType;

            if (underlyingParam.IsAssignableFrom(dynamicSrc.GetType()) &&
                (dynamicSrc.GetType() == underlyingParam || dynamicSrc is not IUnsafePointer))
            {
                if (RefusedByGoAssignability(relation, dynamicSrc.GetType(), dstType))
                    return false;

                marshalled = dstWrapperCtor.Invoke([dynamicSrc]);
                return true;
            }
        }

        // Same subsumption + N5 M-guard as the direct arm above, on the unwrapped value.
        if (!RefusedByGoAssignability(relation, dynamicSrc.GetType(), dstType) &&
            TryUnwrapWrapperValue(dynamicSrc, out object? unwrappedSrc) &&
            dstType.IsAssignableFrom(unwrappedSrc.GetType()) &&
            (unwrappedSrc.GetType() == dstType || unwrappedSrc is not IUnsafePointer))
        {
            marshalled = unwrappedSrc;
            return true;
        }

        // Interface destination: nominal instance passes through unchanged (preserving the
        // original interface value's identity); otherwise the golib assert machinery builds the
        // duck-typed wrapper — the same route emitted `_<T>` asserts take.
        if (dstType.IsInterface)
        {
            if (dstType.IsInstanceOfType(src))
            {
                marshalled = src;
                return true;
            }

            if (builtin.TryTypeAssert(src, dstType, out object? wrapped))
            {
                marshalled = wrapped;
                return true;
            }
        }

        // Go's struct named/unnamed assignability: two struct types with IDENTICAL underlying
        // (same fields — name, managed type, tag, embeddedness, in declaration order) are
        // assignable when at least one side is unnamed. A struct reaches HERE rather than the
        // wrapper arm above because it has a FIELD constructor, not a single-argument wrapper
        // constructor, so wrapperConstructorOf is null and every arm to this point missed it.
        // reflect's TestMakeFuncValidReturnAssignments — an unnamed struct{a,b,c int} returned
        // into a named T slot — is the measured consumer; a census over the reflect suite
        // (569,986 marshalling calls) found EXACTLY ONE such pair and ZERO both-named admits in
        // the struct space, so the rule stays narrow: genuine STRUCT kind only (slices and Complex
        // are value-types too, but convert through the wrapper arm above), identical underlying, at
        // least one side unnamed. Two DIFFERENT NAMED structs match neither the unnamed clause here
        // nor any arm above, which is Go-correct (the invalid direction stays rejected — reflect's
        // TestMakeFuncInvalidReturnAssignments' U->T still panics).
        if (KindOf(dynamicSrc.GetType()) == Struct && KindOf(dstType) == Struct &&
            !RefusedByGoAssignability(relation, dynamicSrc.GetType(), dstType) &&
            haveIdenticalGoStructLayout(dynamicSrc.GetType(), dstType) &&
            tryCopyGoStructFields(dynamicSrc, dstType, out marshalled))
        {
            return true;
        }

        return false;
    }

    // Go's haveIdenticalUnderlyingType for two struct types, over the bridge's own field
    // projection (GoFields, which keeps an embed as a single embedded field exactly as Go's
    // direct-field walk does): same field count, each field identical by name, managed type, tag
    // and embeddedness, in order. Stricter than Go only in comparing the managed field TYPE by
    // identity rather than recursively — which cannot OVER-admit (a genuinely identical Go layout
    // emits identical managed field types), and the census confirmed the one suite consumer is
    // admitted with nothing else moving.
    private static bool haveIdenticalGoStructLayout(Type a, Type b)
    {
        if (a == b)
            return true;

        GoFieldInfo[] fa = GoFields(a), fb = GoFields(b);

        if (fa.Length != fb.Length)
            return false;

        bool anyUnexported = false;

        for (int i = 0; i < fa.Length; i++)
        {
            if (fa[i].Name != fb[i].Name || fa[i].Type != fb[i].Type ||
                fa[i].Tag != fb[i].Tag || fa[i].Embedded != fb[i].Embedded)
                return false;

            if (!fa[i].Exported)
                anyUnexported = true;
        }

        // Go's haveIdenticalUnderlyingType folds each UNEXPORTED field's declaring package into its
        // identity: an unexported field's PkgPath IS its struct's import path (value_impl.cs mints a
        // field descriptor exactly so — `f.Exported ? "" : GoPackagePath(st)`). Two structs whose
        // field names and types match but which carry a matching UNEXPORTED field from DIFFERENT
        // packages are therefore NOT identical. reflect's TestUnaddressableField is the measured
        // consumer of this clause the other way round: it sets an unnamed `struct{buf []byte}` in
        // reflect_test from a `reflect.Buffer` whose `buf` is reflect's own, and Go REFUSES the Set
        // — so omitting this check wrongly ADMITTED it and moved a second row. An exported-only
        // layout is package-independent (an exported field's PkgPath is ""), so the check is gated
        // on there being an unexported field at all.
        if (anyUnexported && GoPackagePath(a) != GoPackagePath(b))
            return false;

        return true;
    }

    // Field-copy construction for an identical-underlying struct assignment: a fresh dst instance
    // carrying every field from src by name. A value-type struct boxes through Activator, so the
    // sets land on the box this returns; src and dst share managed field names because the same
    // converter emitted both, so a name miss means the layouts were not identical after all.
    private static bool tryCopyGoStructFields(object src, Type dstType, out object? marshalled)
    {
        marshalled = null;

        object? dst = Activator.CreateInstance(dstType);

        if (dst is null)
            return false;

        Type srcType = src.GetType();

        foreach (FieldInfo df in dstType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (df.Name.Contains("k__BackingField", StringComparison.Ordinal))
                continue;

            FieldInfo? sf = srcType.GetField(df.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (sf is null)
                return false;

            df.SetValue(dst, sf.GetValue(src));
        }

        marshalled = dst;
        return true;
    }

    // ==== FABRICATION — producing a value of a type from nothing ====
    // The other half of this file. Everything above CONVERTS a value that already exists; everything
    // from here down MAKES one, and reaches the same named-wrapper constructors to do it. Both halves
    // came from the reflection bridge's Phase-3 increment 2, whose design and adversarial-review
    // ledger is docs/phase4/DESIGN-reflection-bridge-phase3-plan.md (I2.R); the size/dims and
    // field-projection halves of that increment live in the TypeLayout and FieldAccess siblings.

    // -------- zero construction (reflect.Zero / reflect.New / SetZero share ONE rule) --------

    /// <summary>
    /// The boxed Go ZERO value for a managed type: pointer kinds yield the canonical typed-nil
    /// instance (one nil encoding system-wide); interface/func kinds a null reference (Go's nil
    /// interface has no type; a nil func is a null delegate); slice/map/chan kinds their nil
    /// container STRUCT default (never <see cref="Activator"/> — the golib containers' explicit
    /// parameterless constructors allocate NON-nil backings); array kinds a backing sized from
    /// <paramref name="arrayDims"/> when known; everything else a default instance (whose field
    /// initializers ARE the Go zero — a blank `_ [4]byte` field materializes its length).
    /// </summary>
    public static object? ZeroValueOf(Type t, nint[]? arrayDims = null, GoChanDir chanDir = GoChanDir.Unstamped)
    {
        switch (KindOf(t))
        {
            case Pointer:
            case UnsafePointer:
                // A pointer-to-ARRAY zero carries the descriptor's dims on its nil, exactly as the
                // language's typed nil does (`(*[0]byte)(nil)` is NilBoxOfDims(0)): reflect.Zero's
                // Interface() then re-describes as *[0]uint8, not *[]uint8 (increment E3 root 5).
                return CanonicalNilPointer(t, arrayDims);
            case Interface:
            case Func:
                return null;
            case String:
                // A NAMED string type's zero is the zero WRAPPER, not a raw @string (the slot
                // is wrapper-typed); raw @string keeps the explicit empty-string form.
                return t == typeof(@string) || t == typeof(string) ? (@string)"" : Activator.CreateInstance(t);
            case Chan:
                // A CHANNEL's zero is the NIL channel of its own Go type, and for a directional
                // type that includes the DIRECTION — the same reason an array's zero is sized from
                // the descriptor's dims below. Without it, reflect.Zero of a `chan<- string`
                // descriptor answers a value whose own dynamic type reads back bidirectional, so
                // the direction survives Type.String() but dies the moment the value is boxed and
                // re-described (internal/reflectlite's TypeString does exactly that: `%T` of
                // ToInterface(Zero(typ))).
                return chanDir is GoChanDir.Recv or GoChanDir.Send ? MakeDirectionalNilChannel(t, chanDir) : DefaultValueOf(t);
            case Slice:
            case Map:
                return DefaultValueOf(t);
            case Array:
                return arrayDims is { Length: > 0 } && t.IsGenericType ? MakeSizedArray(t, arrayDims, 0) : DefaultValueOf(t);
            default:
                return Activator.CreateInstance(t);
        }
    }

    // The NIL channel of a DIRECTIONAL Go channel type, boxed — channel<T>.SendOnly/.RecvOnly,
    // reached generically off the closed type. A generated named-channel wrapper is refused (its
    // managed form is not channel<T>, the same carve-out the whole cargo draws) and falls back to
    // the plain default, which is what it carried before.
    private static readonly ConcurrentDictionary<(Type, GoChanDir), object?> s_directionalNilChannels = new();

    private static object? MakeDirectionalNilChannel(Type t, GoChanDir chanDir)
    {
        return s_directionalNilChannels.GetOrAdd((t, chanDir), static key =>
        {
            (Type type, GoChanDir dir) = key;

            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(channel<>))
                return DefaultValueOf(type);

            PropertyInfo? factory = type.GetProperty(dir == GoChanDir.Recv ? nameof(channel<int>.RecvOnly) : nameof(channel<int>.SendOnly),
                BindingFlags.Public | BindingFlags.Static);

            return factory is null ? DefaultValueOf(type) : factory.GetValue(null);
        });
    }

    // Cached boxed default(T) — the nil form of the golib container STRUCTS (a null reference is
    // NOT the nil map/chan/slice; the zero struct is).
    private static readonly ConcurrentDictionary<Type, Func<object?>> s_defaultFactories = new();

    /// <summary>The boxed <c>default(T)</c> for a managed type (cached factory).</summary>
    public static object? DefaultValueOf(Type t)
    {
        return s_defaultFactories.GetOrAdd(t, static ct =>
            typeof(GoReflect).GetMethod(nameof(defaultOf), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(ct).CreateDelegate<Func<object?>>())();
    }

    private static object? defaultOf<T>()
    {
        return default(T);
    }

    /// <summary>
    /// Constructs a raw <c>array&lt;E&gt;</c> with real backing storage sized from a dims vector
    /// (nested dims build nested element factories, mirroring the converter's own
    /// <c>new(128, () =&gt; new(4))</c> field-initializer form).
    /// </summary>
    public static object MakeSizedArray(Type arrayType, nint[] dims, int level)
    {
        Type? elem = ElementType(arrayType);

        if (elem is null)
            throw new InvalidOperationException($"MakeSizedArray: {arrayType} has no element type.");

        if (level >= dims.Length - 1 || KindOf(elem) != Array)
            return Activator.CreateInstance(arrayType, dims[level])!;

        MethodInfo factoryMaker = typeof(GoReflect).GetMethod(nameof(sizedArrayElementFactory), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(elem);

        object elementFactory = factoryMaker.Invoke(null, [elem, dims, level + 1])!;

        return Activator.CreateInstance(arrayType, dims[level], elementFactory)!;
    }

    private static Func<E> sizedArrayElementFactory<E>(Type elemType, nint[] dims, int level)
    {
        return () => (E)MakeSizedArray(elemType, dims, level)!;
    }

    // -------- container construction (reflect.MakeSlice / MakeMap; named wrappers included) --------

    private static readonly ConcurrentDictionary<Type, Func<nint, nint, object>> s_containerMakers = new();

    /// <summary>
    /// Constructs a golib container (or a generated NAMED container wrapper) through its
    /// <c>ISupportMake</c> surface — the same construction <c>make()</c> emissions use, so
    /// <c>reflect.MakeSlice(namedSliceType, …)</c> yields the WRAPPER, exactly Go's named result.
    /// </summary>
    public static object MakeContainer(Type containerType, nint p1 = 0, nint p2 = -1)
    {
        return s_containerMakers.GetOrAdd(containerType, static ct =>
            typeof(GoReflect).GetMethod(nameof(makeSupported), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(ct).CreateDelegate<Func<nint, nint, object>>())(p1, p2);
    }

    private static object makeSupported<T>(nint p1, nint p2) where T : ISupportMake<T>
    {
        return T.Make(p1, p2)!;
    }

    // -------- pointer-box construction (reflect.New) --------

    private static readonly ConcurrentDictionary<Type, Func<object?, object>> s_boxMakers = new();

    /// <summary>A fresh heap box <c>ж&lt;T&gt;</c> holding <paramref name="value"/> — <c>reflect.New</c>'s allocation.</summary>
    public static object NewPointerBox(Type pointeeType, object? value)
    {
        return s_boxMakers.GetOrAdd(pointeeType, static pt =>
            typeof(GoReflect).GetMethod(nameof(newBox), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(pt).CreateDelegate<Func<object?, object>>())(value);
    }

    private static object newBox<T>(object? value)
    {
        return value is null ? new StandardBox<T>(default(T)!) : new StandardBox<T>((T)value);
    }

    // -------- the []byte VIEW of any Uint8-element slice (reflect.Value.Bytes / SetBytes) --------
    //
    // Go's Bytes()/SetBytes() are defined over the element KIND, not the element TYPE: any slice
    // whose element is Uint8-kinded qualifies — `[]byte`, `[]renamedByte` where
    // `type renamedByte byte`, and a defined slice type over either — and both reach the storage by
    // re-typing the slice HEADER. That is an ALIAS, and it has to stay one: `Bytes()` is how a
    // caller writes INTO a reflected byte slice, so a copy would compile, satisfy every read-only
    // consumer, and silently drop every write.
    //
    // Two of the three shapes were already aliased (a raw slice<byte> is itself; a defined slice
    // type over plain byte answers through its ISlice<byte> view, which shares its backing). The
    // third — a DEFINED byte ELEMENT — had no route at all: slice<renamedByte> holds a
    // renamedByte[] of one-field wrapper structs, an unrelated instantiation with no conversion to
    // slice<byte>, so the catch-all cast threw InvalidCastException out of a core reflect API
    // (encoding/json's TestSliceOfCustomByte and TestEncodeRenamedByteSlice; fmt's `%x` of a
    // []renamedUint8 and five siblings of one table-driven test).
    //
    // The route is slice<T>.AliasOfElement, and the gate below is its whole safety argument: the
    // two element types must be ONE representation under two Go names. That is asked of the managed
    // types directly — value type, no managed references, exactly one byte wide — and never
    // inferred from the [GoType] token, so a wrapper that ever stopped being a bare byte would fall
    // out of the alias rather than pun something wider.
    private static class ByteAliasableElement<E>
    {
        internal static readonly bool Value =
            typeof(E).IsValueType &&
            !RuntimeHelpers.IsReferenceOrContainsReferences<E>() &&
            Unsafe.SizeOf<E>() == 1 &&
            KindOf(typeof(E)) == Uint8;
    }

    private static readonly ConcurrentDictionary<Type, Func<object, slice<byte>?>?> s_byteSliceViews = new();

    /// <summary>
    /// The <c>[]byte</c> ALIASING <paramref name="container"/>'s storage, when
    /// <paramref name="container"/> is a Go slice of Uint8-kinded elements however the element and
    /// the slice type are NAMED; <c>false</c> when it is not such a slice (Go panics there, and the
    /// caller owns that message).
    /// </summary>
    public static bool TryByteSliceView(object? container, out slice<byte> view)
    {
        view = default;

        switch (container)
        {
            case null:
                return false;
            // The two shapes that were always aliased: a raw []byte, and a defined slice type over
            // plain byte (its ISlice<byte> view shares the backing store — see slice's view ctor).
            case slice<byte> raw:
                view = raw;
                return true;
            case ISlice<byte> named:
                view = new slice<byte>(named);
                return true;
        }

        Func<object, slice<byte>?>? viewer = s_byteSliceViews.GetOrAdd(container.GetType(), static ct =>
        {
            Type? elem = KindOf(ct) == Slice ? ElementType(ct) : null;

            return elem is null
                ? null
                : typeof(GoReflect).GetMethod(nameof(byteSliceViewOf), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(elem).CreateDelegate<Func<object, slice<byte>?>>();
        });

        if (viewer?.Invoke(container) is not { } aliased)
            return false;

        view = aliased;
        return true;
    }

    private static slice<byte>? byteSliceViewOf<E>(object container)
    {
        if (!ByteAliasableElement<E>.Value)
            return null;

        // A defined SLICE type over a defined byte element reaches its window through the same
        // shared-backing view ctor the plain-byte case above uses; a raw slice<E> unboxes.
        slice<E> source = container is slice<E> raw ? raw : new slice<E>((ISlice<E>)container);

        return slice<byte>.AliasOfElement(in source);
    }

    private static readonly ConcurrentDictionary<Type, Func<slice<byte>, object?>?> s_byteSliceStores = new();

    /// <summary>
    /// <paramref name="bytes"/> re-spelled as a value of the Uint8-element slice type
    /// <paramref name="sliceType"/>, ALIASING the same storage — the write half of
    /// <see cref="TryByteSliceView"/> (<c>reflect.Value.SetBytes</c>, whose Go form assigns the
    /// slice HEADER into the slot). <c>false</c> when <paramref name="sliceType"/> is not such a
    /// slice type.
    /// </summary>
    public static bool TryByteSliceAs(Type sliceType, slice<byte> bytes, out object? stored)
    {
        stored = null;

        Func<slice<byte>, object?>? storer = s_byteSliceStores.GetOrAdd(sliceType, static st =>
        {
            Type? elem = KindOf(st) == Slice ? ElementType(st) : null;

            return elem is null
                ? null
                : typeof(GoReflect).GetMethod(nameof(byteSliceAsOf), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(elem).CreateDelegate<Func<slice<byte>, object?>>();
        });

        if (storer?.Invoke(bytes) is not { } aliased)
            return false;

        // The ELEMENT is now spelled right; a DEFINED slice type still needs its wrapper, and that
        // is the one assignability relation Value.Set already routes through — so a named []byte
        // and a named []DefinedByte are reached by one rule rather than two.
        return TryMarshalAssignable(aliased, sliceType, out stored);
    }

    private static object? byteSliceAsOf<E>(slice<byte> bytes)
    {
        if (typeof(E) == typeof(byte))
            return bytes;

        if (!ByteAliasableElement<E>.Value)
            return null;

        return slice<E>.AliasOfElement(in bytes);
    }

    // -------- Go convertibility (Set{Int,Uint,Float,Complex,String,Bool} + future Convert) --------

    /// <summary>
    /// Go's CONVERTIBILITY relation over boxed managed values — the single conversion rule the
    /// reflection Set* family routes through (assignability, then named-wrapper construction via
    /// the wrapper's generated single-argument constructor, then the kinded scalar conversions
    /// with Go semantics: integer stores TRUNCATE to the destination width, floats/complex narrow).
    /// </summary>
    public static bool TryConvertTo(object? src, Type dstType, out object? result)
    {
        result = null;

        // `any` destination: everything (including nil) passes through unchanged.
        if (dstType == typeof(object))
        {
            result = src;
            return true;
        }

        if (src is null)
            return dstType.IsInterface;

        if (TryMarshalAssignable(src, dstType, out result))
            return true;

        // A named-wrapper source converts through its underlying value first.
        if (TryUnwrapWrapperValue(src, out object? unwrapped) && TryConvertTo(unwrapped, dstType, out result))
            return true;

        // A named-wrapper destination constructs through its generated single-argument
        // constructor (parameter type discovered, never assumed primitive — golib struct
        // underlyings like @string/uintptr/complex64 included).
        ConstructorInfo? wrapperCtor = wrapperConstructorOf(dstType);

        if (wrapperCtor is not null)
        {
            Type underlying = wrapperCtor.GetParameters()[0].ParameterType;

            if (underlying != dstType && TryConvertTo(src, underlying, out object? underlyingValue))
            {
                result = wrapperCtor.Invoke([underlyingValue]);
                return true;
            }

            return false;
        }

        result = coerceScalar(KindOf(dstType), dstType, src);
        return result is not null;
    }

    private static readonly ConcurrentDictionary<Type, ConstructorInfo?> s_wrapperConstructors = new();

    // The generated wrapper constructor taking the underlying value (never the NilType form).
    // Memoized WHOLE, not just its [GoType] gate: the answer is one per-type-immutable
    // ConstructorInfo, and TryConvertTo reaches this per VALUE from reflect.Value.Set /
    // SetMapIndex / Call / Convert — so the constructor scan was repeated per call too.
    //
    // THE BINDING FLAGS ARE LOAD-BEARING, and public-only was a latent bug the W3 accessibility arc
    // made reachable — the same shape TypeParamCaster carried in builtin.TypeParamConversions.cs.
    // A wrapper for a Go-UNEXPORTED named type is emitted with an `internal` underlying-value
    // constructor (`internal namedPlainBytes(slice<byte> value)`), so a public-only scan sees only
    // the two PUBLIC forms — the make ctor `(nint length, nint capacity, nint low)`, whose declared
    // arity is 3, and the `(NilType)` form this loop excludes by design — finds no one-argument
    // candidate, and answers null. Null here does not degrade: it silently DELETES Go's
    // named/unnamed assignability rule for every unexported named type, so `reflect.Value.Set` of a
    // raw underlying into such a slot is refused as unassignable. Measured on ReflectBridgeClosure,
    // where it surfaced two arms away as `panic: reflect.Value.SetBytes of non-byte slice` —
    // TryByteSliceAs had already spelled the ELEMENT correctly and was refused only when it asked
    // this probe for the `type namedPlainBytes []byte` wrapper.
    //
    // The fix is on the PROBE, not on the generated accessibility. That constructor is golib
    // MARSHALLING surface — Go has no such member, and Go exportedness is decided by the Go name's
    // case, never by C# accessibility. Promoting it to public to satisfy a probe would widen the C#
    // surface for something Go never asked for, which is never-more-permissive-than-Go pointing the
    // other way; a probe reading members golib itself owns is not a permission question. The sibling
    // probe on this same shape — TryUnwrapWrapperValue's `m_value` field — was already NonPublic, so
    // this only brings the constructor half into step with the field half.
    //
    // Widening cannot make the scan ambiguous: the emitted wrapper set is exactly {make ctor (arity
    // 3, public), underlying ctor (arity 1), NilType ctor (arity 1, excluded)}, so precisely one
    // candidate qualifies whether or not it is public.
    private static ConstructorInfo? wrapperConstructorOf(Type t)
    {
        return s_wrapperConstructors.GetOrAdd(t, static type =>
        {
            if (goTypeMarkerOf(type) is null)
                return null;

            foreach (ConstructorInfo ctor in type.GetConstructors(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                ParameterInfo[] parameters = ctor.GetParameters();

                if (parameters.Length == 1 && parameters[0].ParameterType != typeof(NilType))
                    return ctor;
            }

            return null;
        });
    }

    /// <summary>Unwraps a generated named-type wrapper value to its single underlying field value.</summary>
    public static bool TryUnwrapWrapperValue(object src, [NotNullWhen(true)] out object? underlying)
    {
        underlying = null;
        Type t = src.GetType();

        if (goTypeMarkerOf(t) is not { Definition.Length: > 0 } def || def.Definition == "dyn")
            return false;

        FieldInfo? valueField = t.GetField("m_value", BindingFlags.Instance | BindingFlags.NonPublic);

        if (valueField is null)
            return false;

        underlying = valueField.GetValue(src);

        // A named FIXED-SIZE ARRAY wrapper holds its `array<T>` inside a one-word publish holder, so
        // that the lazily-allocated backing can be installed with an interlocked CAS instead of a racy
        // `??=` (go2cs-gen's InheritedTypeTemplate, door 2 of the element-aliasing family). The holder
        // is storage, never a Go value: unwrap the extra level so every caller sees exactly the
        // `array<T>` it saw when the slot held the value inline. No converted or golib type is ever an
        // IStrongBox, so nothing else can be caught by this.
        if (underlying is IStrongBox holder)
            underlying = holder.Value;

        return underlying is not null;
    }

    // Go scalar conversion into a destination kind: integers truncate (unchecked), floats and
    // complex narrow — exactly the reflect Set{Int,Uint,Float,Complex} contract (verified against
    // Go 1.23 reflect: no overflow panic on Set*).
    private static object? coerceScalar(int dstKind, Type dstType, object src)
    {
        switch (dstKind)
        {
            case Bool:
                return src is bool b ? b : null;
            case String:
                return src switch { @string gs => (object)gs, string s => (@string)s, _ => null };
            case Complex128:
                return src switch { Complex c => c, complex64 c64 => (Complex)c64, _ => null };
            case Complex64:
                return src switch
                {
                    Complex c => new complex64((float)c.Real, (float)c.Imaginary),
                    complex64 c64 => c64,
                    _ => null
                };
            case Float32:
                return tryWideFloat(src, out double f32) ? (float)f32 : null;
            case Float64:
                return tryWideFloat(src, out double f64) ? f64 : null;
        }

        if (!tryWideInteger(src, out long wide))
            return null;

        return dstKind switch
        {
            Int => (nint)wide,
            Int8 => (sbyte)wide,
            Int16 => (short)wide,
            Int32 => (int)wide,
            Int64 => wide,
            Uint => unchecked((nuint)wide),
            Uint8 => unchecked((byte)wide),
            Uint16 => unchecked((ushort)wide),
            Uint32 => unchecked((uint)wide),
            Uint64 => unchecked((ulong)wide),
            Uintptr => new uintptr(unchecked((nuint)wide)),
            _ => null
        };
    }

    private static bool tryWideInteger(object src, out long wide)
    {
        switch (src)
        {
            case long l: wide = l; return true;
            case ulong ul: wide = unchecked((long)ul); return true;
            case nint n: wide = n; return true;
            case nuint nu: wide = unchecked((long)nu); return true;
            case int i: wide = i; return true;
            case uint u: wide = u; return true;
            case short s: wide = s; return true;
            case ushort us: wide = us; return true;
            case sbyte sb: wide = sb; return true;
            case byte bt: wide = bt; return true;
            case uintptr up: wide = unchecked((long)up.Value); return true;
            case double d: wide = unchecked((long)d); return true;
            case float f: wide = unchecked((long)f); return true;
            default: wide = 0; return false;
        }
    }

    private static bool tryWideFloat(object src, out double wide)
    {
        switch (src)
        {
            case double d: wide = d; return true;
            case float f: wide = f; return true;
            default:
                bool ok = tryWideInteger(src, out long l);
                wide = l;
                return ok;
        }
    }
}

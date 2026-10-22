using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// The golib half of the reflection-bridge CLOSURE arc — the facts a Go program cannot reach
/// directly, so the behavioral <c>ReflectBridgeClosure</c> test (which compares the whole surface
/// against <c>go run</c>) cannot own them.
/// </summary>
/// <remarks>
/// Division of labour, the same one <see cref="GoTypeDefinednessTests"/> states: anything a Go
/// program can observe end to end belongs to the behavioral test, which is strictly stronger
/// because Go itself supplies the expected answer. What lives here is the golib API contract
/// underneath — the shapes only golib can construct (a natural-delegate variadic tail, a
/// <c>[GoType]</c> wrapper never emitted by any Go source in the suite) and the ALIASING promise,
/// which is a statement about storage rather than about printed output.
/// </remarks>
[TestClass]
public class GoReflectBridgeClosureTests
{
    // Stands in for a converted `<pkg>_package` static class — the nesting GoReflect's package
    // qualification looks for when it decides a type has a Go name.
    private static class functest_package
    {
        // `type handler func(int) error` WITH a method: a defined func type acquires a managed
        // identity exactly then, and the converter emits it as its own delegate nested here.
        public delegate error handler(nint n);
    }

    // `type definedByte byte` — a one-field wrapper over byte, the element representation the
    // Bytes()/SetBytes() alias is defined over.
    [GoType("num:byte")]
    private readonly struct definedByte
    {
        private readonly byte m_value;

        private definedByte(byte value) => m_value = value;

        public static implicit operator definedByte(byte value) => new(value);

        public static implicit operator byte(definedByte value) => value.m_value;
    }

    // ---- an UNNAMED func type renders structurally; a DEFINED one keeps its name ----------------

    [TestMethod]
    public void AnUnnamedFuncTypeRendersItsGoSignature()
    {
        Assert.AreEqual("func()", GoReflect.GoTypeName(typeof(Action)));
        Assert.AreEqual("func(int)", GoReflect.GoTypeName(typeof(Action<nint>)));
        Assert.AreEqual("func() int", GoReflect.GoTypeName(typeof(Func<nint>)));
        Assert.AreEqual("func(int, string) bool", GoReflect.GoTypeName(typeof(Func<nint, @string, bool>)));

        // Go's multi-return: a ValueTuple result unpacks to one Go result per element, parenthesized.
        Assert.AreEqual("func(int) (bool, int)", GoReflect.GoTypeName(typeof(Func<nint, (bool, nint)>)));

        // A pointer parameter is the shape fmt's own table reads back — `%#v` of a func value
        // printed the CLR delegate family (`Action`1`) until this arm existed.
        Assert.AreEqual("func(*int)", GoReflect.GoTypeName(typeof(Action<ж<nint>>)));
    }

    [TestMethod]
    public void AnUnnamedFuncTypeHasNoGoName()
    {
        // HasGoName must agree with GoTypeName ARM FOR ARM: a type that renders structurally has no
        // name, so reflect.Type.Name() answers "" and the descriptor's TFlagNamed stays clear.
        Assert.IsFalse(GoReflect.HasGoName(typeof(Action)));
        Assert.IsFalse(GoReflect.HasGoName(typeof(Func<nint, error>)));

        // A DEFINED func type is a delegate the converter declared inside a `<pkg>_package` class,
        // and it keeps its name — the distinction the whole arm turns on.
        Assert.IsTrue(GoReflect.HasGoName(typeof(functest_package.handler)));
        Assert.AreEqual("functest.handler", GoReflect.GoTypeName(typeof(functest_package.handler)));
    }

    [TestMethod]
    public void AVariadicTailIsRecognizedByShapeNotByDelegateFamilyName()
    {
        // The golib variadic families carry a name marker, and that is what the shape test used to
        // rely on. A declared `func(string, ...int)` used as a method group in an `any` position
        // instead acquires C#'s NATURAL delegate type, whose name carries no marker at all — so the
        // name test called it non-variadic and In(1) handed back a raw Span<int>, which rendered as
        // `func(string, Span\`1)`. A Span<T> parameter cannot arise any other way in converted code.
        Assert.IsTrue(GoReflect.TryFuncShape(typeof(SpanTail), out Type[]? ins, out Type[]? outs, out bool isVariadic));
        Assert.IsTrue(isVariadic);
        Assert.AreEqual(typeof(slice<nint>), ins![1]);
        Assert.AreEqual(0, outs!.Length);
        Assert.AreEqual("func(string, ...int)", GoReflect.GoTypeName(typeof(SpanTail)));

        // The non-variadic control: no Span tail, no variadic claim.
        Assert.IsTrue(GoReflect.TryFuncShape(typeof(Action<@string, nint>), out _, out _, out bool plain));
        Assert.IsFalse(plain);
    }

    private delegate void SpanTail(@string s, params Span<nint> rest);

    // ---- InvokeVariadic: the typed dispatch a `params Span<T>` tail forces ------------------------
    //
    // These live here rather than in the behavioral tier for the reason this file's header states:
    // the shapes are ones only golib can construct. A Go program reaches Value.Call through the
    // reflect package and observes only the RESULT; what is asserted below is that the dispatch
    // reaches the callee at all for each of the two delegate identities a converted variadic takes,
    // and that the tail arrives as a real Span over the array handed in — neither of which a printed
    // value can distinguish from an accidental pass.

    [TestMethod]
    public void AVariadicFamilyDelegateIsCalledWithItsTailIntact()
    {
        // The FAMILY identity: what a declared variadic func TYPE (a parameter, a field, a defined
        // func type) lowers to. It is already the family type, so no rebind is involved.
        Funcꓸꓸꓸ<@string, nint, @string> join = (prefix, parts) =>
        {
            @string result = prefix;

            foreach (nint part in parts)
                result += (@string)("/" + part);

            return result;
        };

        Assert.AreEqual((@string)"go/1/2/3", (@string)GoReflect.InvokeVariadic(join, [(@string)"go"], new nint[] { 1, 2, 3 })!);

        // An EMPTY tail is Go's `f(prefix)` — the slice is empty, not absent, and not nil-hostile.
        Assert.AreEqual((@string)"go", (@string)GoReflect.InvokeVariadic(join, [(@string)"go"], Array.Empty<nint>())!);
    }

    [TestMethod]
    public void ANaturalTypedVariadicDelegateRebindsOntoItsFamily()
    {
        // The NATURAL identity: a variadic func literal in an `any` slot, or a declared variadic
        // used as a method group there, gets a compiler-synthesized delegate type instead of the
        // family one — the same shape difference TryFuncShape had to stop reading off the type NAME.
        // A family cast would throw InvalidCastException on it; the rebind is what makes it callable.
        SpanTail record = (s, rest) =>
        {
            s_spanTailLog = s;

            foreach (nint value in rest)
                s_spanTailLog += (@string)("," + value);
        };

        Assert.AreNotEqual(typeof(Actionꓸꓸꓸ<@string, nint>), record.GetType());
        Assert.IsNull(GoReflect.InvokeVariadic(record, [(@string)"n"], new nint[] { 7, 8 }));
        Assert.AreEqual((@string)"n,7,8", s_spanTailLog);
    }

    private static @string s_spanTailLog;

    [TestMethod]
    public void AnExpressionCompiledVariadicDelegateRebindsToo()
    {
        // The third identity, and the one that has no Go source at all: a delegate the BRIDGE built.
        // Value.Method binds a receiver by compiling an expression lambda, so a variadic method value
        // arrives here as a compiled `Func<Span<T>, R>` whose Method is NOT a runtime MethodInfo —
        // Delegate.CreateDelegate refuses to re-bind that method at all. Retargeting through Invoke
        // is what makes the rebind total; binding target-and-method would throw here.
        ParameterExpression args = Expression.Parameter(typeof(Span<nint>), "args");
        Delegate compiled = Expression.Lambda<Func<Span<nint>, nint>>(
            Expression.Convert(Expression.Property(args, nameof(Span<nint>.Length)), typeof(nint)), args).Compile();

        Assert.IsFalse(compiled.Method is { } m && m.GetType().Name.Contains("Runtime"));
        Assert.AreEqual((nint)3, (nint)GoReflect.InvokeVariadic(compiled, [], new nint[] { 4, 5, 6 })!);
    }

    private delegate @string SByteTail(sbyte head, params Span<ushort> rest);

    [TestMethod]
    public void ARegisteredVariadicSignatureIsServedByItsCompiledTrampolines()
    {
        // go2cs-gen registers each variadic signature that can reach reflect (GoReflect.VariadicRegistry.cs), so the call
        // and make trampolines are closed by compiled code: Native AOT has no other route for a value-type signature. The
        // signature here is used by no other test, so the per-type caches start empty and the registered pair is what
        // serves the family, a natural-typed delegate rebinding onto it, and MakeFunc's factory.
        Type family = typeof(Funcꓸꓸꓸ<sbyte, ushort, @string>);

        Assert.IsFalse(GoReflect.IsVariadicRegistered(family));
        GoReflect.RegisterVariadicFunc<sbyte, ushort, @string>();
        GoReflect.RegisterVariadicFunc<sbyte, ushort, @string>();
        Assert.IsTrue(GoReflect.IsVariadicRegistered(family));

        Funcꓸꓸꓸ<sbyte, ushort, @string> sum = (head, rest) =>
        {
            int total = head;

            foreach (ushort value in rest)
                total += value;

            return (@string)total.ToString();
        };

        SByteTail natural = (head, rest) => (@string)(head + ":" + rest.Length);

        Assert.AreEqual((@string)"10", (@string)GoReflect.InvokeVariadic(sum, [(sbyte)1], new ushort[] { 4, 5 })!);
        Assert.AreEqual((@string)"2:3", (@string)GoReflect.InvokeVariadic(natural, [(sbyte)2], new ushort[] { 1, 1, 1 })!);

        System.Reflection.MethodInfo invoke = GoReflect.DelegateInvoke(family)!;
        Delegate made = GoReflect.BuildVariadicMakeFactory(family, invoke, invoke.GetParameters())(args => (@string)(args[0] + "/" + ((slice<ushort>)args[1]!).Length));

        Assert.AreEqual(family, made.GetType());
        Assert.AreEqual((@string)"7/2", ((Funcꓸꓸꓸ<sbyte, ushort, @string>)made)(7, 8, 9));
    }

    [TestMethod]
    public void AnUnregisteredValueTypeVariadicIsRefusedByNameWithoutDynamicCode()
    {
        // Native AOT has no code for a value-type trampoline compiled code never named, and the runtime's own answer is a
        // NotSupportedException ("missing native code"). Without dynamic code (this hook stands in for Native AOT under
        // the JIT) such a family is refused by name instead, as a Go panic naming the family and the registry, for both
        // the call and MakeFunc's factory. A registered value-type family and a reference-type one (shared code exists
        // under Native AOT) are still served. Each signature is used by no other test.
        Funcꓸꓸꓸ<short, short, short> unregistered = (head, rest) => (short)(head + rest.Length);
        Actionꓸꓸꓸ<object, object> shared = (head, rest) => s_sharedLog = rest.Length;
        Funcꓸꓸꓸ<ushort, sbyte, @string> registered = (head, rest) => (@string)(head + "+" + rest.Length);
        GoReflect.RegisterVariadicFunc<ushort, sbyte, @string>();

        go.golib.TypeExtensions.DynamicCodeSupportedOverride = false;

        try
        {
            PanicException call = Assert.ThrowsException<PanicException>(() => GoReflect.InvokeVariadic(unregistered, [(short)1], new short[] { 2 }));
            StringAssert.Contains(call.Message, "Funcꓸꓸꓸ<int16, int16, int16>");
            StringAssert.Contains(call.Message, "GoReflect.RegisterVariadicFunc");

            System.Reflection.MethodInfo invoke = GoReflect.DelegateInvoke(typeof(Funcꓸꓸꓸ<short, short, short>))!;
            PanicException make = Assert.ThrowsException<PanicException>(() => GoReflect.BuildVariadicMakeFactory(typeof(Funcꓸꓸꓸ<short, short, short>), invoke, invoke.GetParameters()));
            StringAssert.Contains(make.Message, "Funcꓸꓸꓸ<int16, int16, int16>");

            Assert.IsNull(GoReflect.InvokeVariadic(shared, [new object()], new object[] { 1, 2, 3 }));
            Assert.AreEqual(3, s_sharedLog);
            Assert.AreEqual((@string)"4+2", (@string)GoReflect.InvokeVariadic(registered, [(ushort)4], new sbyte[] { 1, 1 })!);
        }
        finally
        {
            go.golib.TypeExtensions.DynamicCodeSupportedOverride = null;
        }

        // With dynamic code, as under the JIT, the same unregistered family closes.
        Assert.AreEqual((short)3, (short)GoReflect.InvokeVariadic(unregistered, [(short)1], new short[] { 2, 2 })!);
    }

    private static int s_sharedLog;

    [TestMethod]
    public void AVariadicTailAliasesTheArrayItWasHandedRatherThanACopy()
    {
        // The tail is a Span OVER the array, so the callee sees the caller's storage. Asserting it
        // through a write is the only way to tell that apart from a defensive copy, and a copy is
        // exactly what any object?[]-based invoke path would have been forced into.
        Actionꓸꓸꓸ<nint> bump = args =>
        {
            for (int i = 0; i < args.Length; i++)
                args[i] *= 10;
        };

        nint[] tail = [1, 2, 3];
        GoReflect.InvokeVariadic(bump, [], tail);

        CollectionAssert.AreEqual(new nint[] { 10, 20, 30 }, tail);
    }

    [TestMethod]
    public void AVariadicMultiReturnArrivesAsItsValueTuple()
    {
        // Go's multi-return is a ValueTuple result, and the family's TResult carries it whole — the
        // same shape Value.Call destructures positionally on the non-variadic path.
        Funcꓸꓸꓸ<nint, (nint sum, bool any)> total = args =>
        {
            nint sum = 0;

            foreach (nint value in args)
                sum += value;

            return (sum, args.Length > 0);
        };

        Assert.AreEqual((4, true), ((nint, bool))GoReflect.InvokeVariadic(total, [], new nint[] { 1, 3 })!);
    }

    [TestMethod]
    public void EveryFamilyArityDispatchesToTheRightTrampoline()
    {
        // The dispatch tables are indexed BY FIXED-PARAMETER COUNT, and the two families count their
        // type arguments differently (Action: n+1, Func: n+2). A mis-indexed entry fails loudly —
        // MakeGenericType rejects the arity, or the cast throws — but only for the arity that is
        // wrong, and only when some package finally uses it. The behavioral test reaches 0, 1 and 2
        // through real Go source; 3 through 8 have no consumer in the corpus yet, so they are
        // exercised here rather than left to be discovered.
        //
        // `call` passes 1 << i as fixed argument i and a tail summing to 100, so arity n answers
        // 100 + (2^n - 1): the fixed half is a bit set, and a trampoline that dropped an argument,
        // passed one twice, or lost the tail lands on a different number rather than a plausible
        // one. (Two arguments TRANSPOSED still sums the same — addition is commutative — so that is
        // deliberately not claimed.)
        Assert.AreEqual((nint)100, call(new Funcꓸꓸꓸ<nint, nint>(t => sum(t))));
        Assert.AreEqual((nint)101, call(new Funcꓸꓸꓸ<nint, nint, nint>((a, t) => a + sum(t))));
        Assert.AreEqual((nint)103, call(new Funcꓸꓸꓸ<nint, nint, nint, nint>((a, b, t) => a + b + sum(t))));
        Assert.AreEqual((nint)107, call(new Funcꓸꓸꓸ<nint, nint, nint, nint, nint>((a, b, c, t) => a + b + c + sum(t))));
        Assert.AreEqual((nint)115, call(new Funcꓸꓸꓸ<nint, nint, nint, nint, nint, nint>((a, b, c, d, t) => a + b + c + d + sum(t))));
        Assert.AreEqual((nint)131, call(new Funcꓸꓸꓸ<nint, nint, nint, nint, nint, nint, nint>((a, b, c, d, e, t) => a + b + c + d + e + sum(t))));
        Assert.AreEqual((nint)163, call(new Funcꓸꓸꓸ<nint, nint, nint, nint, nint, nint, nint, nint>((a, b, c, d, e, f, t) => a + b + c + d + e + f + sum(t))));
        Assert.AreEqual((nint)227, call(new Funcꓸꓸꓸ<nint, nint, nint, nint, nint, nint, nint, nint, nint>((a, b, c, d, e, f, g, t) => a + b + c + d + e + f + g + sum(t))));
        Assert.AreEqual((nint)355, call(new Funcꓸꓸꓸ<nint, nint, nint, nint, nint, nint, nint, nint, nint, nint>((a, b, c, d, e, f, g, h, t) => a + b + c + d + e + f + g + h + sum(t))));

        assertAction(100, new Actionꓸꓸꓸ<nint>(t => s_arityLog = sum(t)));
        assertAction(101, new Actionꓸꓸꓸ<nint, nint>((a, t) => s_arityLog = a + sum(t)));
        assertAction(103, new Actionꓸꓸꓸ<nint, nint, nint>((a, b, t) => s_arityLog = a + b + sum(t)));
        assertAction(107, new Actionꓸꓸꓸ<nint, nint, nint, nint>((a, b, c, t) => s_arityLog = a + b + c + sum(t)));
        assertAction(115, new Actionꓸꓸꓸ<nint, nint, nint, nint, nint>((a, b, c, d, t) => s_arityLog = a + b + c + d + sum(t)));
        assertAction(131, new Actionꓸꓸꓸ<nint, nint, nint, nint, nint, nint>((a, b, c, d, e, t) => s_arityLog = a + b + c + d + e + sum(t)));
        assertAction(163, new Actionꓸꓸꓸ<nint, nint, nint, nint, nint, nint, nint>((a, b, c, d, e, f, t) => s_arityLog = a + b + c + d + e + f + sum(t)));
        assertAction(227, new Actionꓸꓸꓸ<nint, nint, nint, nint, nint, nint, nint, nint>((a, b, c, d, e, f, g, t) => s_arityLog = a + b + c + d + e + f + g + sum(t)));
        assertAction(355, new Actionꓸꓸꓸ<nint, nint, nint, nint, nint, nint, nint, nint, nint>((a, b, c, d, e, f, g, h, t) => s_arityLog = a + b + c + d + e + f + g + h + sum(t)));

        // A fixed prefix beyond the family's last arity has no delegate to bind, and says so.
        NineFixed tooManyFixed = (a, b, c, d, e, f, g, h, i, rest) => { };
        NotImplementedException tooWide = Assert.ThrowsException<NotImplementedException>(
            () => GoReflect.InvokeVariadic(tooManyFixed, new object?[9], Array.Empty<nint>()));

        StringAssert.Contains(tooWide.Message, "9 fixed parameters");
    }

    // Fixed argument i is 1 << i and the tail sums to 100, so arity n answers 100 + (2^n - 1).
    private static object? call(Delegate d)
    {
        int fixedCount = d.GetType().GetMethod("Invoke")!.GetParameters().Length - 1;
        object?[] args = new object?[fixedCount];

        for (int i = 0; i < fixedCount; i++)
            args[i] = (nint)(1 << i);

        return GoReflect.InvokeVariadic(d, args, new nint[] { 40, 60 });
    }

    // An Action family member returns nothing, so its arrival is observed through the side effect.
    private static void assertAction(nint expected, Delegate d)
    {
        s_arityLog = -1;
        Assert.IsNull(call(d), "an Action family member has no result");
        Assert.AreEqual(expected, s_arityLog);
    }

    private static nint sum(Span<nint> values)
    {
        nint total = 0;

        foreach (nint value in values)
            total += value;

        return total;
    }

    private static nint s_arityLog;

    private delegate void NineFixed(nint a, nint b, nint c, nint d, nint e, nint f, nint g, nint h, nint i, params Span<nint> rest);

    [TestMethod]
    public void ANonVariadicDelegateIsRefusedByName()
    {
        // The dispatch is only ever reached behind TryFuncShape's variadic verdict, so a plain
        // delegate arriving here is a bridge defect — it must say so rather than mis-index.
        NotImplementedException ex = Assert.ThrowsException<NotImplementedException>(
            () => GoReflect.InvokeVariadic(new Action<nint>(_ => { }), [(nint)1], Array.Empty<nint>()));

        StringAssert.Contains(ex.Message, "no Span<T> tail");
    }

    // ---- Value.Bytes / SetBytes ALIAS, they never copy -------------------------------------------

    [TestMethod]
    public void AByteSliceViewOverADefinedElementAliasesItsStorage()
    {
        slice<definedByte> source = new(new definedByte[] { (byte)'h', (byte)'i', (byte)'!' });

        Assert.IsTrue(GoReflect.TryByteSliceView(source, out slice<byte> view));
        Assert.AreEqual(3, (int)view.Length);
        Assert.AreEqual((byte)'h', view[0]);

        // The whole point: a WRITE through the view lands in the source's storage. A copy would pass
        // every read-only consumer and silently drop this.
        view[2] = (byte)'?';
        Assert.AreEqual((byte)'?', (byte)source[2]);

        // ...and back the other way, which is what a write through the SOURCE must show.
        source[0] = (byte)'H';
        Assert.AreEqual((byte)'H', view[0]);
    }

    [TestMethod]
    public void AByteSliceViewCarriesTheWindowAndTheNilSlice()
    {
        slice<definedByte> source = new(new definedByte[] { 1, 2, 3, 4, 5 });
        slice<definedByte> window = source.slice(1, 4);

        Assert.IsTrue(GoReflect.TryByteSliceView(window, out slice<byte> view));
        Assert.AreEqual(3, (int)view.Length);
        Assert.AreEqual(4, (int)view.Capacity);
        Assert.AreEqual((byte)2, view[0]);

        // The nil slice re-spells as the nil slice — Go's own header re-typing carries the nil data
        // pointer across the same way.
        Assert.IsTrue(GoReflect.TryByteSliceView(default(slice<definedByte>), out slice<byte> nilView));
        Assert.AreEqual(0, (int)nilView.Length);
    }

    [TestMethod]
    public void AByteSliceViewIsRefusedForANonByteElement()
    {
        // Go panics for a non-byte slice, and the caller owns that message — so the probe must say
        // no rather than pun something wider. `nint` is the case that would corrupt memory outright.
        Assert.IsFalse(GoReflect.TryByteSliceView(new slice<nint>(new nint[] { 1, 2 }), out _));
        Assert.IsFalse(GoReflect.TryByteSliceView(new map<@string, nint>(), out _));
        Assert.IsFalse(GoReflect.TryByteSliceView(null, out _));
    }

    [TestMethod]
    public void AByteSliceStoresBackUnderTheSlotsOwnElementName()
    {
        slice<byte> bytes = new(new byte[] { (byte)'a', (byte)'b' });

        Assert.IsTrue(GoReflect.TryByteSliceAs(typeof(slice<definedByte>), bytes, out object? stored));
        Assert.IsInstanceOfType(stored, typeof(slice<definedByte>));

        // The store is an alias too (Go's SetBytes assigns the slice HEADER), so the two views are
        // one slice: a write through either is visible in the other.
        slice<definedByte> typed = (slice<definedByte>)stored!;
        typed[0] = (byte)'z';
        Assert.AreEqual((byte)'z', bytes[0]);

        // A plain []byte slot passes straight through, and a non-byte slot is refused.
        Assert.IsTrue(GoReflect.TryByteSliceAs(typeof(slice<byte>), bytes, out object? plain));
        Assert.IsInstanceOfType(plain, typeof(slice<byte>));
        Assert.IsFalse(GoReflect.TryByteSliceAs(typeof(slice<nint>), bytes, out _));
    }

    // ---- new(T) is Go's ZERO value ----------------------------------------------------------------

    [TestMethod]
    public void NewOfAContainerKindIsTheNilContainerNotAnEmptyOne()
    {
        // golib's container structs declare a parameterless constructor that ALLOCATES, and
        // Activator.CreateInstance<T> honors it — so `new([]T)`/`new(map[K]V)`/`new(chan T)` used to
        // point at a non-nil EMPTY container where Go points at nil. The two zero-fabrication paths
        // must agree, so each is asserted against ZeroValueOf, the rule reflect.Zero/New already use.
        Assert.IsTrue(@new<slice<nint>>().Value == nil);
        Assert.IsTrue(@new<map<@string, nint>>().Value == nil);
        Assert.IsTrue(@new<channel<nint>>().Value == nil);

        Assert.AreEqual(GoReflect.ZeroValueOf(typeof(slice<nint>)), @new<slice<nint>>().Value);
        Assert.AreEqual(GoReflect.ZeroValueOf(typeof(map<@string, nint>)), @new<map<@string, nint>>().Value);
    }

    [TestMethod]
    public void NewOfAStructStillRunsItsFieldInitializers()
    {
        // The other half of the same rule, and the reason the carve-out is by KIND rather than
        // wholesale: running the parameterless constructor is what materializes a struct's
        // fixed-size ARRAY fields from the initializers the converter emits into it.
        withArray value = @new<withArray>().Value;

        Assert.IsNotNull(value.Fixed);
        Assert.AreEqual(4, (int)value.Fixed.Length);
    }

    [GoType]
    private struct withArray
    {
        public array<byte> Fixed = new(4);

        public withArray() { }
    }

    // ---- a pointer descriptor carries its POINTEE's array dims -------------------------------------

    [TestMethod]
    public void APointerToAnArrayReportsThePointeesDimensions()
    {
        // A POINTER carries its pointee's dims unshifted — the rule Elem() already applies when it
        // hands the cargo down. Nothing populated it, so `reflect.TypeOf(new([3]int)).Elem()`
        // described a dimension-LESS array and reflect.New of it allocated a zero-length one.
        ж<array<nint>> box = new StandardBox<array<nint>>(new array<nint>(3));

        CollectionAssert.AreEqual(new nint[] { 3 }, GoReflect.PointeeArrayDims(box));

        // Not a pointer to an array, a nil pointer, and a non-pointer each answer null rather than
        // guessing (the r39d rule).
        Assert.IsNull(GoReflect.PointeeArrayDims(new StandardBox<nint>(1)));
        Assert.IsNull(GoReflect.PointeeArrayDims(new StandardBox<array<nint>>(nil)));
        Assert.IsNull(GoReflect.PointeeArrayDims(new array<nint>(3)));
        Assert.IsNull(GoReflect.PointeeArrayDims(null));
    }

    // ---- a NaN map key is never equal to anything, including itself --------------------------------

    [TestMethod]
    public void AFloatMapKeyFollowsGosIEEERuleRatherThanTheBCLCollectionRule()
    {
        // BCL Double.Equals reports NaN equal to NaN so a NaN stored in a collection can be found
        // again; Go applies `==` unchanged, so each insert makes a NEW entry and none can ever be
        // read back or deleted. fmt's own TestSprintf reads the difference out of `%v` of a map.
        double nan = double.NaN;
        map<double, nint> m = new();

        m[nan] = 1;
        m[nan] = 2;

        Assert.AreEqual(2, (int)len(m));
        Assert.IsFalse(m.ContainsKey(nan));

        // An ordinary float key is unaffected — the comparer changes ONE answer, not the relation.
        m[1.5] = 3;
        Assert.IsTrue(m.ContainsKey(1.5));
        Assert.AreEqual((nint)3, m[1.5]);

        // The same rule at the other three float representations.
        map<float, nint> f32 = new();
        f32[float.NaN] = 1;
        f32[float.NaN] = 2;
        Assert.AreEqual(2, (int)len(f32));

        map<Complex, nint> c128 = new();
        c128[new Complex(double.NaN, 0)] = 1;
        c128[new Complex(double.NaN, 0)] = 2;
        Assert.AreEqual(2, (int)len(c128));
    }
}

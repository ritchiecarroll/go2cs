using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

[TestClass]
public class DelegateArityMintTests
{
    // GoReflect.MakeGoFuncType composes the delegate type `reflect.FuncOf` hands back, as
    // TryFuncShape's exact inverse. Composing means NAMING a declared delegate family, and every
    // family is finite — System.Func/Action stop at 16 parameters, golib's ladder (funcArity.cs)
    // continues them to 24, the variadic families (variadic.cs) carry 8 fixed parameters ahead of
    // the `params variadic<T>` tail — while Go's own limit is 128 and its reflect suite drives a
    // 51-parameter FuncOf (all_test.go, issue #54669). Past every ceiling the type is MINTED with
    // Reflection.Emit (GoDelegateSynthesis).
    //
    // TWO PROPERTIES ARE GUARDED, AND THEY ARE INDEPENDENT — which is the point of testing both:
    //
    //   1. THE ROUND TRIP is the mint's contract. NumIn/In/NumOut/Out/IsVariadic must come back
    //      unchanged through MakeGoFuncType → TryFuncShape, at widths no family declares. Each
    //      position takes a DISTINCT type (see parameterTypeAt), so a transposed, dropped or
    //      duplicated parameter moves the reading — a same-type ladder could not see any of them.
    //
    //   2. THE ROUTE is a separate claim, and a passing round trip says NOTHING about it: a minted
    //      17-parameter delegate would round-trip perfectly and still be WRONG, because Go interns
    //      func types and `FuncOf(...) == TypeOf(f)` for a matching declared f (reflect's own
    //      checkSameType). So the route assertions name the exact type each width must answer with —
    //      the BCL's at 16, golib's ladder at 17 and 24, the variadic family at a small variadic —
    //      and only past all of those may the answer be minted.
    //
    // The two were verified independent by deliberate regression, one at a time: reversing the
    // minted parameter list failed only the round-trip tests (every route assertion stayed green),
    // and routing 17 into the mint failed only the ladder assertion (every round trip stayed green).

    // A DISTINCT type per position: the base rotates every 8 and the slice nesting deepens every 8,
    // so no two positions in a 128-parameter signature share a type and no permutation reproduces
    // the sequence.
    private static readonly Type[] s_bases =
    [
        typeof(sbyte), typeof(short), typeof(int), typeof(long),
        typeof(float), typeof(double), typeof(bool), typeof(@string)
    ];

    private static Type parameterTypeAt(int index)
    {
        Type type = s_bases[index % s_bases.Length];

        for (int depth = index / s_bases.Length; depth > 0; depth--)
            type = typeof(slice<>).MakeGenericType(type);

        return type;
    }

    private static Type[] parameterTypes(int count)
    {
        return [.. Enumerable.Range(0, count).Select(parameterTypeAt)];
    }

    // The round trip, asserted per component so a failure names which of Go's five readings moved.
    private static Type assertRoundTrip(Type[] ins, Type[] outs, bool isVariadic)
    {
        Type delegateType = GoReflect.MakeGoFuncType(ins, outs, isVariadic);

        Assert.IsTrue(GoReflect.TryFuncShape(delegateType, out Type[]? readIns, out Type[]? readOuts, out bool readVariadic),
            "TryFuncShape must read back a type MakeGoFuncType composed — the two are inverses");

        Assert.AreEqual(ins.Length, readIns!.Length, "NumIn must round-trip unchanged");
        CollectionAssert.AreEqual(ins, readIns, "In(i) must round-trip unchanged, in order");
        Assert.AreEqual(outs.Length, readOuts!.Length, "NumOut must round-trip unchanged");
        CollectionAssert.AreEqual(outs, readOuts, "Out(i) must round-trip unchanged, in order");
        Assert.AreEqual(isVariadic, readVariadic, "IsVariadic must round-trip unchanged");

        return delegateType;
    }

    private static void assertMinted(Type delegateType, string because)
    {
        Assert.IsTrue(typeof(Delegate).IsAssignableFrom(delegateType), $"{because}: the answer must be a delegate type");

        // THE DECISION, asserted where it is recorded: minted types ride the struct mint's dynamic
        // assembly, because every converted csproj already grants IT the friend access an internal
        // parameter type needs (`<InternalsVisibleTo Include="go2cs.SynthesizedStructs" />`). A
        // second assembly name would need a second grant in every generated csproj.
        Assert.AreEqual("go2cs.SynthesizedStructs", delegateType.Assembly.GetName().Name,
            $"{because}: a minted delegate must share the struct mint's assembly, which is the one converted code grants");

        // A minted delegate is Go's UNNAMED func type, and GoReflect decides that from the DECLARING
        // type alone — a delegate declared inside a `<pkg>_package` class is a Go DEFINED func type.
        Assert.IsNull(delegateType.DeclaringType, $"{because}: a minted delegate must declare nowhere, or it would claim a Go package");
        Assert.IsFalse(GoReflect.HasGoName(delegateType), $"{because}: a minted func type is Go's UNNAMED func type");
        StringAssert.StartsWith(GoReflect.GoTypeName(delegateType), "func(",
            $"{because}: an unnamed func type must render structurally, as reflect.Type.String() reports it");
    }

    [TestMethod]
    public void SixteenParametersStillBindTheBclFamily()
    {
        // THE DISCRIMINATION CASE. 16 is the BCL's last rung and must still be answered by it —
        // unchanged, and by the SAME System.Type converted code binds at that width.
        Type[] ins = parameterTypes(16);

        Assert.AreSame(
            typeof(Func<sbyte, short, int, long, float, double, bool, @string,
                        slice<sbyte>, slice<short>, slice<int>, slice<long>,
                        slice<float>, slice<double>, slice<bool>, slice<@string>, long>),
            GoReflect.MakeGoFuncType(ins, [typeof(long)], false),
            "16 parameters must still bind System.Func through Expression.GetFuncType");

        Assert.AreSame(
            typeof(Action<sbyte, short, int, long, float, double, bool, @string,
                          slice<sbyte>, slice<short>, slice<int>, slice<long>,
                          slice<float>, slice<double>, slice<bool>, slice<@string>>),
            GoReflect.MakeGoFuncType(ins, [], false),
            "16 parameters and no result must still bind System.Action through Expression.GetActionType");
    }

    [TestMethod]
    public void SeventeenParametersBindGolibsDeclaredLadderRatherThanMinting()
    {
        // 17 is the first width the BCL lacks and golib's ladder supplies. Answering it with a MINT
        // would round-trip perfectly and still be wrong: converted code spells a 17-parameter func
        // type with THIS type, and Go interns func types.
        Type[] ins = parameterTypes(17);

        Type funcType = GoReflect.MakeGoFuncType(ins, [typeof(long)], false);

        Assert.AreSame(
            typeof(Func<sbyte, short, int, long, float, double, bool, @string,
                        slice<sbyte>, slice<short>, slice<int>, slice<long>,
                        slice<float>, slice<double>, slice<bool>, slice<@string>,
                        slice<slice<sbyte>>, long>),
            funcType,
            "17 parameters must bind golib's declared Func rung, not a minted type");

        Assert.AreEqual("go", funcType.Namespace, "the 17-parameter rung is golib's, not the BCL's");

        Assert.AreSame(
            typeof(Action<sbyte, short, int, long, float, double, bool, @string,
                          slice<sbyte>, slice<short>, slice<int>, slice<long>,
                          slice<float>, slice<double>, slice<bool>, slice<@string>,
                          slice<slice<sbyte>>>),
            GoReflect.MakeGoFuncType(ins, [], false),
            "17 parameters and no result must bind golib's declared Action rung");

        // And the round trip holds through the declared rung exactly as through a minted type.
        assertRoundTrip(ins, [typeof(long)], false);
    }

    [TestMethod]
    public void TwentyFourParametersAreTheLastDeclaredRung()
    {
        // The ladder's top. Naming it here is what makes a change to that ceiling a deliberate edit
        // to funcArity.cs AND to this test, rather than a silent shift of the mint's lower boundary.
        Type[] ins = parameterTypes(24);
        Type funcType = GoReflect.MakeGoFuncType(ins, [typeof(long)], false);

        Assert.AreEqual("go", funcType.Namespace, "24 parameters must still bind golib's declared ladder");
        Assert.AreNotEqual("go2cs.SynthesizedStructs", funcType.Assembly.GetName().Name,
            "24 parameters must not be minted — the ladder declares that width");

        assertRoundTrip(ins, [typeof(long)], false);
    }

    [TestMethod]
    public void TwentyFiveParametersAreTheFirstMintedWidth()
    {
        // One past the last declared rung: the first width nothing declares, and the first the mint
        // must answer. A converted package could not spell this func type either, so the minted
        // domain and the declared domain cannot overlap.
        Type[] ins = parameterTypes(25);

        assertMinted(assertRoundTrip(ins, [typeof(long)], false), "25 parameters");
    }

    [TestMethod]
    public void ThirtyTwoParametersRoundTripWithAGoMultiResult()
    {
        // A minted delegate whose RESULT is a Go multi-return — the ValueTuple makeGoResultType
        // builds and FlattenValueTuple unnests, carried through the mint unchanged.
        Type[] ins = parameterTypes(32);
        Type[] outs = [typeof(int), typeof(@string), typeof(bool)];

        assertMinted(assertRoundTrip(ins, outs, false), "32 parameters with three results");
    }

    [TestMethod]
    public void FiftyOneParametersRoundTrip()
    {
        // Go's own case: reflect all_test.go's TestFuncOf builds a 51-parameter func for issue
        // #54669, which is what this whole capability is owed to.
        Type[] ins = parameterTypes(51);

        assertMinted(assertRoundTrip(ins, [], false), "51 parameters and no result");
    }

    [TestMethod]
    public void OneHundredTwentyEightParametersRoundTrip()
    {
        // Go's ceiling: `reflect.FuncOf: too many arguments` fires above 128 inputs plus outputs, so
        // this is the widest signature the mint can ever be asked for.
        Type[] ins = parameterTypes(128);

        assertMinted(assertRoundTrip(ins, [], false), "128 parameters");
    }

    [TestMethod]
    public void AVariadicTwentyIsMintedAndKeepsItsTailASlice()
    {
        // The variadic tail past the variadic families' 8 fixed parameters. The tail goes IN as
        // Go's `[]T`, becomes the `variadic<T>` the read side tests for, and comes back as `[]T` — the
        // one convention MakeGoFuncType owns and the mint carries untouched.
        Type[] ins = [.. parameterTypes(19), typeof(slice<@string>)];

        Type delegateType = assertRoundTrip(ins, [typeof(int)], true);

        assertMinted(delegateType, "a variadic 20");

        Assert.AreSame(typeof(variadic<@string>), delegateType.GetMethod("Invoke")!.GetParameters()[19].ParameterType,
            "the minted Invoke must carry the tail as variadic<T> — a slice<T> there would read back non-variadic");
    }

    [TestMethod]
    public void AVariadicInsideTheFamiliesBindsTheDeclaredVariadicDelegate()
    {
        // reflect's own TestFuncOf case: `FuncOf([int, string, []bool], nil, true)`. The variadic
        // families declare this width, so the answer must be the type converted code uses — the same
        // interning contract the ladder assertion above makes for the non-variadic side.
        Type[] ins = [typeof(int), typeof(@string), typeof(slice<bool>)];

        Assert.AreSame(typeof(Actionꓸꓸꓸ<int, @string, bool>),
            GoReflect.MakeGoFuncType(ins, [], true),
            "a variadic func inside the declared families must bind the declared variadic delegate");

        assertRoundTrip(ins, [], true);

        Assert.AreSame(typeof(Funcꓸꓸꓸ<int, @string, bool, long>),
            GoReflect.MakeGoFuncType(ins, [typeof(long)], true),
            "and so must the one with a result");
    }

    [TestMethod]
    public void OneSignatureIsOneMintedType()
    {
        // THE INTERN IS THE IDENTITY. Go interns func types, so two FuncOf calls with one signature
        // must answer one type — a fresh mint per call would break `==` on every reflect.Type
        // comparison downstream, and encoding/gob-style Type-keyed maps with it.
        Type[] ins = parameterTypes(40);

        Assert.AreSame(
            GoReflect.MakeGoFuncType(ins, [typeof(int)], false),
            GoReflect.MakeGoFuncType(parameterTypes(40), [typeof(int)], false),
            "one signature must mint one type, however many times it is asked for");

        Assert.AreNotSame(
            GoReflect.MakeGoFuncType(ins, [typeof(int)], false),
            GoReflect.MakeGoFuncType(ins, [typeof(long)], false),
            "a different RESULT is a different Go func type");

        Assert.AreNotSame(
            GoReflect.MakeGoFuncType(ins, [typeof(int)], false),
            GoReflect.MakeGoFuncType(parameterTypes(41), [typeof(int)], false),
            "a different parameter list is a different Go func type");
    }

    [TestMethod]
    public void AMintedDelegateIsInvocableThroughMakeFunc()
    {
        // A type that exists but cannot be bound would be a hollow capability, so the mint is driven
        // through the MakeFunc half it exists to serve: MakeGoFuncDelegate expression-compiles a
        // wrapper of EXACTLY this type. Uniform parameter types with distinct VALUES is the other
        // half of the positional guard — the round trips above check the order of the TYPES, this
        // checks that argument i reaches parameter i.
        const int Arity = 32;

        Type[] ins = [.. Enumerable.Repeat(typeof(int), Arity)];
        Type delegateType = GoReflect.MakeGoFuncType(ins, [typeof(long)], false);

        assertMinted(delegateType, "32 uniform parameters");

        long weighted = 0;

        Delegate wrapper = GoReflect.MakeGoFuncDelegate(delegateType, args =>
        {
            for (int i = 0; i < args.Length; i++)
                weighted += (long)(int)args[i]! * (i + 1);

            return weighted;
        });

        Assert.AreSame(delegateType, wrapper.GetType(), "the wrapper must BE the minted delegate type");

        object?[] arguments = [.. Enumerable.Range(1, Arity).Cast<object?>()];
        object? result = wrapper.DynamicInvoke(arguments);

        // Weighting argument i by its position makes the sum order-SENSITIVE: 1..32 weighted by
        // 1..32 is the sum of squares, and any transposition of positions i and j moves it by
        // (i-j)^2 — the same reason FuncArityLadderTests weights its ladder probes.
        const long SumOfSquaresTo32 = 32L * 33 * 65 / 6;

        Assert.AreEqual(SumOfSquaresTo32, weighted, "every argument must reach the parameter of its own position");
        Assert.AreEqual(SumOfSquaresTo32, result, "the minted delegate must return its wrapper's result");
    }
}

namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface Animal {
    @string Type();
    @string Swim();
}

[GoType] partial interface Test {
}

[GoType] partial struct Dog {
    public @string Name;
    public @string Breed;
}

[GoType] partial struct Frog {
    public @string Name;
    public @string Color;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object gobˢ = (@string)"gob"u8;
private static readonly object canˢ = (@string)"can"u8;

internal static void Main() {
    var f = @new<Frog>();
    var d = @new<Dog>();
    ref var zoo = ref heap<array<Animal>>(out var Ꮡzoo);
    zoo = new Animal[]{new FrogжAnimal(f), new DogжAnimal(d)}.array();
    Test t = default!;
    fmt.Printf("Iface cmp result = %v\n"u8, AreEqual(zoo[0], f));
    fmt.Printf("Iface cmp result = %v\n"u8, AreEqual(zoo[0], zoo[0]));
    fmt.Printf("Iface cmp result = %v\n"u8, !AreEqual(zoo[0], t));
    any stored = gobˢ;
    fmt.Printf("any cmp = %v %v\n"u8, !AreEqual(stored, (@string)("gob")), !AreEqual(stored, (@string)("xml")));
    checkErr(1);
    checkErr(0);
    useAndRelease();
    Animal a = default!;
    fmt.Printf("%T\n"u8, a);
    foreach (var (_, aΔ1) in zoo.ΔRangeSnapshot()) {
        fmt.Println(aΔ1.Type(), canˢ, aΔ1.Swim());
    }
    fmt.Printf("%T\n"u8, a);
    ShowZoo(Ꮡzoo);
    fmt.Printf("%T\n"u8, a);
    var vowels = new array<bool>(128){[(rune)'a'] = true, [(rune)'e'] = true, [(rune)'i'] = true, [(rune)'o'] = true, [(rune)'u'] = true, [(rune)'y'] = true};
    fmt.Println(vowels);
}

[GoType("num:uintptr")] partial struct errno;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string errnoˢ = "errno"u8;

internal static @string Error(this errno e) {
    return errnoˢ;
}

internal static errno errAgain => 11;

internal static error mayFail(nint n) {
    if (n > 0) {
        return errAgain;
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object gotAgainˢ = (@string)"got again"u8;
private static readonly object notAgainˢ = (@string)"not again"u8;
private static readonly object switchAgainˢ = (@string)"switch: again"u8;
private static readonly object switchNilˢ = (@string)"switch: nil"u8;
private static readonly object switchOtherˢ = (@string)"switch: other"u8;

internal static void checkErr(nint n) {
    var err = mayFail(n);
    if (AreEqual(err, errAgain)) {
        fmt.Println(gotAgainˢ);
    }
    if (!AreEqual(err, errAgain)) {
        fmt.Println(notAgainˢ);
    }
    var exprᴛ1 = err;
    if (AreEqual(exprᴛ1, errAgain)) {
        fmt.Println(switchAgainˢ);
    }
    else if (AreEqual(exprᴛ1, default!)) {
        fmt.Println(switchNilˢ);
    }
    else { /* default: */
        fmt.Println(switchOtherˢ);
    }

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object releasedˢ = (@string)"released"u8;

internal static error release(errno e) {
    fmt.Println(releasedˢ, (uintptr)e);
    return errAgain;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object usingˢ = (@string)"using"u8;

internal static void useAndRelease() {
    GoFrame ᒐ = default;
    try {
        defer(release, errAgain, ref ᒐ);
        fmt.Println(usingˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void ShowZoo([GoArrayDims(2)] ж<array<Animal>> Ꮡzoo) {
    ref var zoo = ref Ꮡzoo.DerefOrNull();

    Animal a = default!;
    foreach (var (_, vᴛ1) in zoo.ΔRangeSnapshot()) {
        a = vᴛ1;

        fmt.Println(a.Type(), canˢ, a.Swim());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string frogˢ = "Frog"u8;

[GoRecv] public static @string Type(this ref Frog f) {
    return frogˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string kickˢ = "Kick"u8;

[GoRecv] public static @string Swim(this ref Frog f) {
    return kickˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string paddleˢ = "Paddle"u8;

[GoRecv] public static @string Swim(this ref Dog d) {
    return paddleˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string doggieˢ = "Doggie"u8;

[GoRecv] public static @string Type(this ref Dog d) {
    return doggieˢ;
}

} // end main_package

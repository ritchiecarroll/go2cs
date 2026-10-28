global using Test = object;

namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Animal {
    @string Type();
    @string Swim();
}

partial struct Dog {
    public @string Name;
    public @string Breed;
}

partial struct Frog {
    public @string Name;
    public @string Color;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object gobˢ = (@string)"gob"u8;

internal static void Main() {
    var f = @new<Frog>();
    var d = @new<Dog>();
    ref var zoo = ref heap<array<Animal>>(out var Ꮡzoo);
    zoo = new Animal[]{new FrogжAnimal(f), new DogжAnimal(d)}.array();
    Test t = default;

    fmt.Printf("Iface cmp result = %v\n"u8, AreEqual(zoo[0], f));
    fmt.Printf("Iface cmp result = %v\n"u8, AreEqual(zoo[0], zoo[0]));
    fmt.Printf("Iface cmp result = %v\n"u8, !AreEqual(zoo[0], t));

    any stored = gobˢ;
    fmt.Printf("any cmp = %v %v\n"u8, !AreEqual(stored, (@string)("gob")), !AreEqual(stored, (@string)("xml")));

    checkErr(1);
    checkErr(0);
    useAndRelease();

    Animal a = default;
    fmt.Printf("%T\n"u8, a);

    foreach (var (_, aΔ1) in zoo.ΔRangeSnapshot()) {
        fmt.Println(aΔ1.Type(), "can", aΔ1.Swim());
    }

    fmt.Printf("%T\n"u8, a);

    ShowZoo(Ꮡzoo);

    fmt.Printf("%T\n"u8, a);

    var vowels = new array<bool>(128){['a'] = true, ['e'] = true, ['i'] = true, ['o'] = true, ['u'] = true, ['y'] = true};
    fmt.Println(vowels);
}

partial struct errno /*num:uintptr*/;

internal static @string Error(this errno e) {
    return "errno";
}

internal static errno errAgain => 11;

internal static error mayFail(nint n) {
    if (n > 0) {
        return errAgain;
    }
    return default;
}

internal static void checkErr(nint n) {
    var err = mayFail(n);
    if (AreEqual(err, errAgain)) {
        fmt.Println("got again");
    }
    if (!AreEqual(err, errAgain)) {
        fmt.Println("not again");
    }
    switch (err) {
    case var ᴛ when AreEqual(ᴛ, errAgain): {
        fmt.Println("switch: again");
        break;
    }
    case var ᴛ when AreEqual(ᴛ, default): {
        fmt.Println("switch: nil");
        break;
    }
    default: {
        fmt.Println("switch: other");
        break;
    }}

}

internal static error release(errno e) {
    fmt.Println("released", (uintptr)e);
    return errAgain;
}

internal static void useAndRelease() {
    GoFrame ᒐ = default;
    try {
        defer(() => release(errAgain), ref ᒐ);
        fmt.Println("using");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void ShowZoo(/*[2]*/ ж<array<Animal>> Ꮡzoo) {
    ref var zoo = ref Ꮡzoo.DerefOrNull();

    Animal a = default;

    foreach (var (_, vᴛ1) in zoo.ΔRangeSnapshot()) {
        a = vᴛ1;

        fmt.Println(a.Type(), "can", a.Swim());
    }
}

public static @string Type(this ref Frog f) {
    return "Frog";
}

public static @string Swim(this ref Frog f) {
    return "Kick";
}

public static @string Swim(this ref Dog d) {
    return "Paddle";
}

public static @string Type(this ref Dog d) {
    return "Doggie";
}

} // end main_package

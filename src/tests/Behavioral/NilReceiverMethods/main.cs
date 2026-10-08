namespace go;

using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

internal static error errInvalid = errors.New("invalid argument"u8);

partial struct File {
    internal @string name;
    internal nint fd;
}

internal static error checkValid(this ж<File> Ꮡf, @string op) {
    if (Ꮡf == nil) {
        return errInvalid;
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string chdirˢ = "chdir"u8;

public static error Chdir(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    {
        var err = Ꮡf.checkValid(chdirˢ); if (err != default!) {
            return err;
        }
    }
    return fmt.Errorf("chdir %s"u8, f.name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string boxnameˢ = "boxname"u8;

public static @string BoxName(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    _ = Ꮡf.checkValid(boxnameˢ);
    return f.name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sideEffectRanBoxShapeˢ = (@string)"  side effect ran (box shape)"u8;
private static readonly @string boxannounceˢ = "boxannounce"u8;

public static @string BoxAnnounce(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    fmt.Println(sideEffectRanBoxShapeˢ);
    _ = Ꮡf.checkValid(boxannounceˢ);
    return f.name;
}

public static @string ValName(this ref File f) {
    return f.name;
}

public static nint ValFd(this ref File f) {
    return f.fd + 1;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sideEffectRanRefShapeˢ = (@string)"  side effect ran (ref shape)"u8;

public static @string ValAnnounce(this ref File f) {
    fmt.Println(sideEffectRanRefShapeˢ);
    return f.name;
}

internal static void @try(@string label, Action fn) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Printf("%s -> panic: %v\n"u8, label, r);
                    return;
                }
            }
        }, ref ᒐ);
        fn();
        fmt.Printf("%s -> returned\n"u8, label);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nilReceiverDelegatedˢ = (@string)"== 1. nil receiver, DELEGATED guard: must not panic =="u8;
private static readonly @string nilFileCheckValidˢ = "nilFile.checkValid"u8;
private static readonly @string nilFileChdirˢ = "nilFile.Chdir"u8;
private static readonly object nilReceiverUnconditionalˢ = (@string)"== 2. nil receiver, UNCONDITIONAL deref: must panic =="u8;
private static readonly @string nilFileBoxNameˢ = "nilFile.BoxName"u8;
private static readonly @string nilFileValNameˢ = "nilFile.ValName"u8;
private static readonly object nilReceiverSideEffectˢ = (@string)"== 3. nil receiver, side effect BEFORE the deref: effect then panic =="u8;
private static readonly @string nilFileBoxAnnounceˢ = "nilFile.BoxAnnounce"u8;
private static readonly @string nilFileValAnnounceˢ = "nilFile.ValAnnounce"u8;
private static readonly object nonNilReceiverBothShapesˢ = (@string)"== 4. non-nil receiver, both shapes: values identical =="u8;
private static readonly @string realCheckValidˢ = "real.checkValid"u8;
private static readonly @string realChdirˢ = "real.Chdir"u8;
private static readonly @string realBoxNameˢ = "real.BoxName"u8;
private static readonly @string realValNameˢ = "real.ValName"u8;
private static readonly @string realValFdˢ = "real.ValFd"u8;
private static readonly @string realBoxAnnounceˢ = "real.BoxAnnounce"u8;
private static readonly object theReceiverSurvivesAˢ = (@string)"== 5. the receiver survives a write through the pointer =="u8;
private static readonly @string renamedTxtˢ = "renamed.txt"u8;

internal static void Main() {
    ж<File> nilFile = default!;
    fmt.Println(nilReceiverDelegatedˢ);
    var nilFileʗ1 = nilFile;
    @try(nilFileCheckValidˢ, () => {
        fmt.Printf("  checkValid -> %v (is errInvalid: %v)\n"u8, nilFileʗ1.checkValid("x"u8), AreEqual(nilFileʗ1.checkValid("x"u8), errInvalid));
    });
    var nilFileʗ2 = nilFile;
    @try(nilFileChdirˢ, () => {
        var err = nilFileʗ2.Chdir();
        fmt.Printf("  Chdir -> %v (is errInvalid: %v)\n"u8, err, AreEqual(err, errInvalid));
    });
    fmt.Println();
    fmt.Println(nilReceiverUnconditionalˢ);
    var nilFileʗ3 = nilFile;
    @try(nilFileBoxNameˢ, () => {
        _ = nilFileʗ3.BoxName();
    });
    var nilFileʗ4 = nilFile;
    @try(nilFileValNameˢ, () => {
        _ = nilFileʗ4.ValName();
    });
    fmt.Println();
    fmt.Println(nilReceiverSideEffectˢ);
    var nilFileʗ5 = nilFile;
    @try(nilFileBoxAnnounceˢ, () => {
        _ = nilFileʗ5.BoxAnnounce();
    });
    var nilFileʗ6 = nilFile;
    @try(nilFileValAnnounceˢ, () => {
        _ = nilFileʗ6.ValAnnounce();
    });
    fmt.Println();
    fmt.Println(nonNilReceiverBothShapesˢ);
    var real = Ꮡ(new File(name: "real.txt"u8, fd: 41));
    var realʗ1 = real;
    @try(realCheckValidˢ, () => {
        fmt.Printf("  checkValid -> %v\n"u8, realʗ1.checkValid("x"u8));
    });
    var realʗ2 = real;
    @try(realChdirˢ, () => {
        fmt.Printf("  Chdir -> %v\n"u8, realʗ2.Chdir());
    });
    var realʗ3 = real;
    @try(realBoxNameˢ, () => {
        fmt.Printf("  BoxName -> %s\n"u8, realʗ3.BoxName());
    });
    var realʗ4 = real;
    @try(realValNameˢ, () => {
        fmt.Printf("  ValName -> %s\n"u8, realʗ4.ValName());
    });
    var realʗ5 = real;
    @try(realValFdˢ, () => {
        fmt.Printf("  ValFd -> %d\n"u8, realʗ5.ValFd());
    });
    var realʗ6 = real;
    @try(realBoxAnnounceˢ, () => {
        fmt.Printf("  BoxAnnounce -> %s\n"u8, realʗ6.BoxAnnounce());
    });
    fmt.Printf("  box/ref shapes agree on the name: %v\n"u8, real.BoxName() == real.ValName());
    fmt.Println();
    fmt.Println(theReceiverSurvivesAˢ);
    real.Value.name = renamedTxtˢ;
    fmt.Printf("  BoxName -> %s, ValName -> %s\n"u8, real.BoxName(), real.ValName());
}

} // end main_package

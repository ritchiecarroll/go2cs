namespace go;

using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

internal static error errInvalid = errors.New("invalid argument");

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

public static error Chdir(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    {
        var err = Ꮡf.checkValid("chdir"); if (err != default!) {
            return err;
        }
    }
    return fmt.Errorf("chdir %s"u8, f.name);
}

public static @string BoxName(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    _ = Ꮡf.checkValid("boxname");
    return f.name;
}

public static @string BoxAnnounce(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    fmt.Println("  side effect ran (box shape)");
    _ = Ꮡf.checkValid("boxannounce");
    return f.name;
}

public static @string ValName(this ref File f) {
    return f.name;
}

public static nint ValFd(this ref File f) {
    return f.fd + 1;
}

public static @string ValAnnounce(this ref File f) {
    fmt.Println("  side effect ran (ref shape)");
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

internal static void Main() {
    ж<File> nilFile = default!;

    fmt.Println("== 1. nil receiver, DELEGATED guard: must not panic ==");
    var nilFileʗ1 = nilFile;
    @try("nilFile.checkValid", () => {
        fmt.Printf("  checkValid -> %v (is errInvalid: %v)\n"u8, nilFileʗ1.checkValid("x"), AreEqual(nilFileʗ1.checkValid("x"), errInvalid));
    });
    var nilFileʗ2 = nilFile;
    @try("nilFile.Chdir", () => {
        var err = nilFileʗ2.Chdir();
        fmt.Printf("  Chdir -> %v (is errInvalid: %v)\n"u8, err, AreEqual(err, errInvalid));
    });

    fmt.Println();
    fmt.Println("== 2. nil receiver, UNCONDITIONAL deref: must panic ==");
    var nilFileʗ3 = nilFile;
    @try("nilFile.BoxName", () => {
        _ = nilFileʗ3.BoxName();
    });
    var nilFileʗ4 = nilFile;
    @try("nilFile.ValName", () => {
        _ = nilFileʗ4.ValName();
    });

    fmt.Println();
    fmt.Println("== 3. nil receiver, side effect BEFORE the deref: effect then panic ==");
    var nilFileʗ5 = nilFile;
    @try("nilFile.BoxAnnounce", () => {
        _ = nilFileʗ5.BoxAnnounce();
    });
    var nilFileʗ6 = nilFile;
    @try("nilFile.ValAnnounce", () => {
        _ = nilFileʗ6.ValAnnounce();
    });

    fmt.Println();
    fmt.Println("== 4. non-nil receiver, both shapes: values identical ==");
    var real = Ꮡ(new File(name: "real.txt", fd: 41));
    var realʗ1 = real;
    @try("real.checkValid", () => {
        fmt.Printf("  checkValid -> %v\n"u8, realʗ1.checkValid("x"));
    });
    var realʗ2 = real;
    @try("real.Chdir", () => {
        fmt.Printf("  Chdir -> %v\n"u8, realʗ2.Chdir());
    });
    var realʗ3 = real;
    @try("real.BoxName", () => {
        fmt.Printf("  BoxName -> %s\n"u8, realʗ3.BoxName());
    });
    var realʗ4 = real;
    @try("real.ValName", () => {
        fmt.Printf("  ValName -> %s\n"u8, realʗ4.ValName());
    });
    var realʗ5 = real;
    @try("real.ValFd", () => {
        fmt.Printf("  ValFd -> %d\n"u8, realʗ5.ValFd());
    });
    var realʗ6 = real;
    @try("real.BoxAnnounce", () => {
        fmt.Printf("  BoxAnnounce -> %s\n"u8, realʗ6.BoxAnnounce());
    });
    fmt.Printf("  box/ref shapes agree on the name: %v\n"u8, real.BoxName() == real.ValName());

    fmt.Println();
    fmt.Println("== 5. the receiver survives a write through the pointer ==");
    real.Value.name = "renamed.txt";
    fmt.Printf("  BoxName -> %s, ValName -> %s\n"u8, real.BoxName(), real.ValName());
}

} // end main_package

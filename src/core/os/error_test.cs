// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using fs = go.io.fs_package;
using Δos = os_package;
using filepath = go.path.filepath_package;
using Δtesting = testing_package;
using go.io;
using go.path;
using static go.os_internal_test_package;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goErrIsExistˢ => ref goErrIsExistˢᵇ;
private static readonly @string goErrIsExistˢᵇ = "_Go_ErrIsExist"u8;
internal static ref readonly @string openShouldHaveFailedˢ => ref openShouldHaveFailedˢᵇ;
private static readonly @string openShouldHaveFailedˢᵇ = "Open should have failed"u8;
internal static ref readonly @string osIsExistˢ => ref osIsExistˢᵇ;
private static readonly @string osIsExistˢᵇ = "os.IsExist"u8;

public static void TestErrIsExist(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();

        var (f, err) = Δos.CreateTemp(""u8, goErrIsExistˢ);
        if (err != nil) {
            t.Fatalf("open ErrIsExist tempfile: %s", err);
            return;
        }
        defer(Δos.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var f2, err) = Δos.OpenFile(f.Name(), (nint)((nint)(nint)(Δos.O_RDWR | Δos.O_CREATE) | Δos.O_EXCL), 384);
        if (err == nil) {
            f2.Close();
            t.Fatal(openShouldHaveFailedˢ);
        }
        @string s = checkErrorPredicate(osIsExistˢ, Δos.IsExist, err, fs.ErrExist); if (s != ""u8) {
            t.Fatal(s);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string osIsNotExistˢ => ref osIsNotExistˢᵇ;
private static readonly @string osIsNotExistˢᵇ = "os.IsNotExist"u8;
internal static ref readonly @string chdirShouldHaveFailedˢ => ref chdirShouldHaveFailedˢᵇ;
private static readonly @string chdirShouldHaveFailedˢᵇ = "Chdir should have failed, restored original working directory"u8;

internal static @string testErrNotExist(ж<Δtesting.T> t, @string name) {
    var (originalWD, err) = Δos.Getwd();
    if (err != nil) {
        t.Fatal(err);
    }

    (var f, err) = Δos.Open(name);
    if (err == nil) {
        f.Close();
        return openShouldHaveFailedˢ;
    }
    {
        @string s = checkErrorPredicate(osIsNotExistˢ, Δos.IsNotExist, err, fs.ErrNotExist); if (s != ""u8) {
            return s;
        }
    }

    err = Δos.Chdir(name);
    if (err == nil) {
        var errΔ1 = Δos.Chdir(originalWD); if (errΔ1 != nil) {
            t.Fatalf("Chdir should have failed, failed to restore original working directory: %v", errΔ1);
        }
        return chdirShouldHaveFailedˢ;
    }
    {
        @string s = checkErrorPredicate(osIsNotExistˢ, Δos.IsNotExist, err, fs.ErrNotExist); if (s != ""u8) {
            return s;
        }
    }
    return ""u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string notExistsˢ => ref notExistsˢᵇ;
private static readonly @string notExistsˢᵇ = "NotExists"u8;
internal static ref readonly @string notExists2ˢ => ref notExists2ˢᵇ;
private static readonly @string notExists2ˢᵇ = "NotExists2"u8;

public static void TestErrIsNotExist(ж<Δtesting.T> t) {
    @string tmpDir = t.TempDir();
    @string name = filepath.Join(tmpDir, notExistsˢ);
    {
        @string s = testErrNotExist(t, name); if (s != ""u8) {
            t.Fatal(s);
        }
    }

    name = filepath.Join(name, notExists2ˢ);
    {
        @string s = testErrNotExist(t, name); if (s != ""u8) {
            t.Fatal(s);
        }
    }
}

internal static @string checkErrorPredicate(@string predName, Func<error, bool> pred, error err, error target) {
    if (!pred(err)) {
        return fmt.Sprintf("%s does not work as expected for %#v"u8, predName, err);
    }
    if (!errors.Is(err, target)) {
        return fmt.Sprintf("errors.Is(%#v, %#v) = false, want true"u8, err, target);
    }
    return ""u8;
}

partial struct isExistTest {
    internal error err;
    internal bool @is;
    internal bool isnot;
}

internal static slice<isExistTest> isExistTests = [
    new(new fs.PathErrorжerror(Ꮡ(new fs.PathError(Err: fs.ErrInvalid))), false, false),
    new(new fs.PathErrorжerror(Ꮡ(new fs.PathError(Err: fs.ErrPermission))), false, false),
    new(new fs.PathErrorжerror(Ꮡ(new fs.PathError(Err: fs.ErrExist))), true, false),
    new(new fs.PathErrorжerror(Ꮡ(new fs.PathError(Err: fs.ErrNotExist))), false, true),
    new(new fs.PathErrorжerror(Ꮡ(new fs.PathError(Err: fs.ErrClosed))), false, false),
    new(new Δos.LinkErrorжerror(Ꮡ(new Δos.LinkError(Err: fs.ErrInvalid))), false, false),
    new(new Δos.LinkErrorжerror(Ꮡ(new Δos.LinkError(Err: fs.ErrPermission))), false, false),
    new(new Δos.LinkErrorжerror(Ꮡ(new Δos.LinkError(Err: fs.ErrExist))), true, false),
    new(new Δos.LinkErrorжerror(Ꮡ(new Δos.LinkError(Err: fs.ErrNotExist))), false, true),
    new(new Δos.LinkErrorжerror(Ꮡ(new Δos.LinkError(Err: fs.ErrClosed))), false, false),
    new(new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Err: fs.ErrNotExist))), false, true),
    new(new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Err: fs.ErrExist))), true, false),
    new(default, false, false)
];

public static void TestIsExist(ж<Δtesting.T> t) {
    foreach (var (_, tt) in range(isExistTests)) {
        {
            var @is = Δos.IsExist(tt.err); if (@is != tt.@is) {
                t.Errorf("os.IsExist(%T %v) = %v, want %v", tt.err, tt.err, @is, tt.@is);
            }
        }
        {
            var @is = errors.Is(tt.err, fs.ErrExist); if (@is != tt.@is) {
                t.Errorf("errors.Is(%T %v, fs.ErrExist) = %v, want %v", tt.err, tt.err, @is, tt.@is);
            }
        }
        {
            var isnot = Δos.IsNotExist(tt.err); if (isnot != tt.isnot) {
                t.Errorf("os.IsNotExist(%T %v) = %v, want %v", tt.err, tt.err, isnot, tt.isnot);
            }
        }
        {
            var isnot = errors.Is(tt.err, fs.ErrNotExist); if (isnot != tt.isnot) {
                t.Errorf("errors.Is(%T %v, fs.ErrNotExist) = %v, want %v", tt.err, tt.err, isnot, tt.isnot);
            }
        }
    }
}

partial struct isPermissionTest {
    internal error err;
    internal bool want;
}

internal static slice<isPermissionTest> isPermissionTests = [
    new(default, false),
    new(new fs.PathErrorжerror(Ꮡ(new fs.PathError(Err: fs.ErrPermission))), true),
    new(new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Err: fs.ErrPermission))), true)
];

public static void TestIsPermission(ж<Δtesting.T> t) {
    foreach (var (_, tt) in range(isPermissionTests)) {
        {
            var got = Δos.IsPermission(tt.err); if (got != tt.want) {
                t.Errorf("os.IsPermission(%#v) = %v; want %v", tt.err, got, tt.want);
            }
        }
        {
            var got = errors.Is(tt.err, fs.ErrPermission); if (got != tt.want) {
                t.Errorf("errors.Is(%#v, fs.ErrPermission) = %v; want %v", tt.err, got, tt.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goErrPathNULˢ => ref goErrPathNULˢᵇ;
private static readonly @string goErrPathNULˢᵇ = "_Go_ErrPathNUL\x00"u8;
internal static readonly object tempFileShouldHaveFailedˢ = (@string)"TempFile should have failed"u8;
internal static ref readonly @string goErrPathNULˢ2 => ref goErrPathNULˢ2ᵇ;
private static readonly @string goErrPathNULˢ2ᵇ = "_Go_ErrPathNUL"u8;

public static void TestErrPathNUL(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();

        var (f, err) = Δos.CreateTemp(""u8, goErrPathNULˢ);
        if (err == nil) {
            f.Close();
            t.Fatal(tempFileShouldHaveFailedˢ);
        }
        (f, err) = Δos.CreateTemp(""u8, goErrPathNULˢ2);
        if (err != nil) {
            t.Fatalf("open ErrPathNUL tempfile: %s", err);
        }
        defer(Δos.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var f2, err) = Δos.OpenFile(f.Name(), Δos.O_RDWR, 384);
        if (err != nil) {
            t.Fatalf("open ErrPathNUL: %s", err);
        }
        f2.Close();
        (f2, err) = Δos.OpenFile(f.Name() + "\x00"u8, Δos.O_RDWR, 384);
        if (err == nil) {
            f2.Close();
            t.Fatal(openShouldHaveFailedˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorsIsFailedWantedˢ = (@string)"errors.Is failed, wanted success"u8;

public static partial void TestPathErrorUnwrap(ж<Δtesting.T> t) {
    var pe = Ꮡ(new fs.PathError(Err: fs.ErrInvalid));
    if (!errors.Is(new fs.PathErrorжerror(pe), fs.ErrInvalid)) {
        t.Error(errorsIsFailedWantedˢ);
    }
}

partial struct myErrorIs {
    /*embed*/ internal error error;
}

internal static bool Is(this myErrorIs e, error target) { return AreEqual(target, e.error); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object osIsPermissionErrTrueˢ = (@string)"os.IsPermission(err) = true when err.Is(fs.ErrPermission), wanted false"u8;

public static partial void TestErrorIsMethods(ж<Δtesting.T> t) {
    if (Δos.IsPermission(new myErrorIs(fs.ErrPermission))) {
        t.Error(osIsPermissionErrTrueˢ);
    }
}

} // end os_test_package

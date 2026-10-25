// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using fs = io.fs_package;
using os = os_package;
using time = time_package;
using io;

partial class errors_test_package {

// MyError is an error implementation that includes a time and message.
partial struct MyError {
    public time.Time When;
    public @string What;
}

public static @string Error(this MyError e) {
    return fmt.Sprintf("%v: %v"u8, e.When, e.What);
}

internal static error oops() {
    return new MyError(
        time.Date(1989, 3, 15, 22, 30, 0, 0, time.ΔUTC),
        "the file system has gone away"u8
    );
}

public static void Example() {
    {
        var err = oops(); if (err != default!) {
            fmt.Println(err);
        }
    }
    // Output: 1989-03-15 22:30:00 +0000 UTC: the file system has gone away
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string emitMachoDwarfElfHeaderˢ = "emit macho dwarf: elf header corrupted"u8;

public static void ExampleNew() {
    var err = errors.New(emitMachoDwarfElfHeaderˢ);
    if (err != default!) {
        fmt.Print(err);
    }
    // Output: emit macho dwarf: elf header corrupted
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string anErrorˢ = "an error"u8;

public static error OopsNew() {
    return errors.New(anErrorˢ);
}

public static error ErrSentinel = errors.New("an error"u8);

public static error OopsSentinel() {
    return ErrSentinel;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object errorsUsingDistinctˢ = (@string)"Errors using distinct errors.New calls:"u8;
private static readonly object errorsUsingASentinelˢ = (@string)"Errors using a sentinel error:"u8;

// Each call to [errors.New] returns an unique instance of the error,
// even if the arguments are the same. To match against errors
// created by [errors.New], declare a sentinel error and reuse it.
public static void ExampleNew_unique() {
    var err1 = OopsNew();
    var err2 = OopsNew();
    fmt.Println(errorsUsingDistinctˢ);
    fmt.Printf("Is(%q, %q) = %v\n"u8, err1, err2, errors.Is(err1, err2));

    var err3 = OopsSentinel();
    var err4 = OopsSentinel();
    fmt.Println();
    fmt.Println(errorsUsingASentinelˢ);
    fmt.Printf("Is(%q, %q) = %v\n"u8, err3, err4, errors.Is(err3, err4));

    // Output:
    // Errors using distinct errors.New calls:
    // Is("an error", "an error") = false
    //
    // Errors using a sentinel error:
    // Is("an error", "an error") = true
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string nameᶜ = "bimmler"u8;

// The fmt package's Errorf function lets us use the package's formatting
// features to create descriptive error messages.
public static void ExampleNew_errorf() {
    @string name = nameᶜ;
    const nint id = 17;
    var err = fmt.Errorf("user %q (id %d) not found"u8, name, (nint)(id));
    if (err != default!) {
        fmt.Print(err);
    }
    // Output: user "bimmler" (id 17) not found
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string err1ˢ = "err1"u8;
private static readonly @string err2ˢ = "err2"u8;
private static readonly object errIsErr1ˢ = (@string)"err is err1"u8;
private static readonly object errIsErr2ˢ = (@string)"err is err2"u8;

internal partial interface ExampleJoin_type /*dyn*/ {
    slice<error> Unwrap();
}

public static void ExampleJoin() {
    var err1 = errors.New(err1ˢ);
    var err2 = errors.New(err2ˢ);
    var err = errors.Join(err1, err2);
    fmt.Println(err);
    if (errors.Is(err, err1)) {
        fmt.Println(errIsErr1ˢ);
    }
    if (errors.Is(err, err2)) {
        fmt.Println(errIsErr2ˢ);
    }
    fmt.Println(err._<ExampleJoin_type>().Unwrap());
    // Output:
    // err1
    // err2
    // err is err1
    // err is err2
    // [err1 err2]
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nonExistingˢ = "non-existing"u8;
private static readonly object fileDoesNotExistˢ = (@string)"file does not exist"u8;

public static void ExampleIs() {
    {
        var (_, err) = os.Open(nonExistingˢ); if (err != default!) {
            if (errors.Is(err, fs.ErrNotExist)){
                fmt.Println(fileDoesNotExistˢ);
            } else {
                fmt.Println(err);
            }
        }
    }

    // Output:
    // file does not exist
}

partial struct MyIsError {
    internal @string err;
}

public static @string Error(this MyIsError e) {
    return e.err;
}

public static bool Is(this MyIsError e, error err) {
    return AreEqual(err, fs.ErrPermission);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object errorEqualsFsˢ = (@string)"Error equals fs.ErrPermission:"u8;
private static readonly object errorIsFsErrPermissionˢ = (@string)"Error is fs.ErrPermission:"u8;

// Custom errors can implement a method "Is(error) bool" to match other error values,
// overriding the default matching of [errors.Is].
public static void ExampleIs_custom_match() {
    error err = new MyIsError("an error"u8);
    fmt.Println(errorEqualsFsˢ, AreEqual(err, fs.ErrPermission));
    fmt.Println(errorIsFsErrPermissionˢ, errors.Is(err, fs.ErrPermission));

    // Output:
    // Error equals fs.ErrPermission: false
    // Error is fs.ErrPermission: true
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object failedAtPathˢ = (@string)"Failed at path:"u8;

public static void ExampleAs() {
    {
        var (_, err) = os.Open(nonExistingˢ); if (err != default!) {
            ref var pathError = ref heap<ж<fs.PathError>>(out var ᏑpathError);
            if (errors.As(err, ᏑpathError)){
                fmt.Println(failedAtPathˢ, (~pathError).Path);
            } else {
                fmt.Println(err);
            }
        }
    }

    // Output:
    // Failed at path: non-existing
}

public static void ExampleAsType() {
    {
        var (_, err) = os.Open(nonExistingˢ); if (err != default!) {
            {
                var (pathError, ok) = errors.AsType<ж<fs.PathError>>(err); if (ok){
                    fmt.Println(failedAtPathˢ, (~pathError).Path);
                } else {
                    fmt.Println(err);
                }
            }
        }
    }
    // Output:
    // Failed at path: non-existing
}

partial struct MyAsError {
    internal @string err;
}

public static @string Error(this MyAsError e) {
    return e.err;
}

public static bool As(this MyAsError e, any target) {
    var (pe, ok) = target._<ж<ж<fs.PathError>>>(ᐧ);
    if (!ok) {
        return false;
    }
    pe.ValueSlot = Ꮡ(new fs.PathError(
        Op: "custom"u8,
        Path: "/"u8,
        Err: errors.New(e.err)
    ));
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object errorˢ = (@string)"Error:"u8;
private static readonly object errorAsFsPathErrorˢ = (@string)"Error as fs.PathError:"u8;
private static readonly object fsPathErrorˢ = (@string)"fs.PathError:"u8;

// Custom errors can implement a method "As(any) bool" to match against other error types,
// overriding the default matching of [errors.As].
public static void ExampleAs_custom_match() {
    error err = new MyAsError("an error"u8);
    fmt.Println(errorˢ, err);
    fmt.Printf("TypeOf err: %T\n"u8, err);

    ref var pathError = ref heap<ж<fs.PathError>>(out var ᏑpathError);
    var ok = errors.As(err, ᏑpathError);
    fmt.Println(errorAsFsPathErrorˢ, ok);
    fmt.Println(fsPathErrorˢ, pathError.OrTypedNil());

    // Output:
    // Error: an error
    // TypeOf err: errors_test.MyAsError
    // Error as fs.PathError: true
    // fs.PathError: custom /: an error
}

// Custom errors can implement a method "As(any) bool" to match against other error types,
// overriding the default matching of [errors.AsType].
public static void ExampleAsType_custom_match() {
    error err = new MyAsError("an error"u8);
    fmt.Println(errorˢ, err);
    fmt.Printf("TypeOf err: %T\n"u8, err);

    var (pathError, ok) = errors.AsType<ж<fs.PathError>>(err);
    fmt.Println(errorAsFsPathErrorˢ, ok);
    fmt.Println(fsPathErrorˢ, pathError.OrTypedNil());

    // Output:
    // Error: an error
    // TypeOf err: errors_test.MyAsError
    // Error as fs.PathError: true
    // fs.PathError: custom /: an error
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string error1ˢ = "error1"u8;

public static void ExampleUnwrap() {
    var err1 = errors.New(error1ˢ);
    var err2 = fmt.Errorf("error2: [%w]"u8, err1);
    fmt.Println(err2);
    fmt.Println(errors.Unwrap(err2));
    // Output:
    // error2: [error1]
    // error1
}

} // end errors_test_package

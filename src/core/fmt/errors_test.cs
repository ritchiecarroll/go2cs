// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;
using Δtesting = testing_package;
using static go.fmt_internal_test_package;

partial class fmt_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string innerErrorˢ = "inner error"u8;
internal static readonly object prefixˢ = (@string)"prefix"u8;
internal static readonly object suffixˢ = (@string)"suffix"u8;
internal static readonly object positionalVerbˢ = (@string)"positional verb"u8;
internal static readonly object notAnErrorˢ = (@string)"not-an-error"u8;

internal partial struct TestErrorf_type /*dyn*/ {
    internal error err;
    internal @string wantText;
    internal error wantUnwrap;
    internal slice<error> wantSplit;
}

public static void TestErrorf(ж<Δtesting.T> Ꮡt) {
    // noVetErrorf is an alias for fmt.Errorf that does not trigger vet warnings for
    // %w format strings.
    Funcꓸꓸꓸ<@string, any, error> noVetErrorf = fmt.Errorfᶠ;
    var wrapped = errors.New(innerErrorˢ);
    foreach (var (_, test) in new TestErrorf_type[]{new(
        err: fmt.Errorf("%w"u8, wrapped),
        wantText: "inner error"u8,
        wantUnwrap: wrapped
    ), new(
        err: fmt.Errorf("added context: %w"u8, wrapped),
        wantText: "added context: inner error"u8,
        wantUnwrap: wrapped
    ), new(
        err: fmt.Errorf("%w with added context"u8, wrapped),
        wantText: "inner error with added context"u8,
        wantUnwrap: wrapped
    ), new(
        err: fmt.Errorf("%s %w %v"u8, prefixˢ, wrapped, suffixˢ),
        wantText: "prefix inner error suffix"u8,
        wantUnwrap: wrapped
    ), new(
        err: fmt.Errorf("%[2]s: %[1]w"u8, wrapped, positionalVerbˢ),
        wantText: "positional verb: inner error"u8,
        wantUnwrap: wrapped
    ), new(
        err: fmt.Errorf("%v"u8, wrapped),
        wantText: "inner error"u8
    ), new(
        err: fmt.Errorf("added context: %v"u8, wrapped),
        wantText: "added context: inner error"u8
    ), new(
        err: fmt.Errorf("%v with added context"u8, wrapped),
        wantText: "inner error with added context"u8
    ), new(
        err: noVetErrorf("%w is not an error"u8, notAnErrorˢ),
        wantText: "%!w(string=not-an-error) is not an error"u8
    ), new(
        err: noVetErrorf("wrapped two errors: %w %w"u8, ((errString)(@string)"1"u8), ((errString)(@string)"2"u8)),
        wantText: "wrapped two errors: 1 2"u8,
        wantSplit: new error[]{((errString)(@string)"1"u8), ((errString)(@string)"2"u8)}.slice()
    ), new(
        err: noVetErrorf("wrapped three errors: %w %w %w"u8, ((errString)(@string)"1"u8), ((errString)(@string)"2"u8), ((errString)(@string)"3"u8)),
        wantText: "wrapped three errors: 1 2 3"u8,
        wantSplit: new error[]{((errString)(@string)"1"u8), ((errString)(@string)"2"u8), ((errString)(@string)"3"u8)}.slice()
    ), new(
        err: noVetErrorf("wrapped nil error: %w %w %w"u8, ((errString)(@string)"1"u8), (any)(default!), ((errString)(@string)"2"u8)),
        wantText: "wrapped nil error: 1 %!w(<nil>) 2"u8,
        wantSplit: new error[]{((errString)(@string)"1"u8), ((errString)(@string)"2"u8)}.slice()
    ), new(
        err: noVetErrorf("wrapped one non-error: %w %w %w"u8, ((errString)(@string)"1"u8), notAnErrorˢ, ((errString)(@string)"3"u8)),
        wantText: "wrapped one non-error: 1 %!w(string=not-an-error) 3"u8,
        wantSplit: new error[]{((errString)(@string)"1"u8), ((errString)(@string)"3"u8)}.slice()
    ), new(
        err: fmt.Errorf("wrapped errors out of order: %[3]w %[2]w %[1]w"u8, ((errString)(@string)"1"u8), ((errString)(@string)"2"u8), ((errString)(@string)"3"u8)),
        wantText: "wrapped errors out of order: 3 2 1"u8,
        wantSplit: new error[]{((errString)(@string)"1"u8), ((errString)(@string)"2"u8), ((errString)(@string)"3"u8)}.slice()
    ), new(
        err: fmt.Errorf("wrapped several times: %[1]w %[1]w %[2]w %[1]w"u8, ((errString)(@string)"1"u8), ((errString)(@string)"2"u8)),
        wantText: "wrapped several times: 1 1 2 1"u8,
        wantSplit: new error[]{((errString)(@string)"1"u8), ((errString)(@string)"2"u8)}.slice()
    ), new(
        err: fmt.Errorf("%w"u8, (any)(default!)),
        wantText: "%!w(<nil>)"u8,
        wantUnwrap: default! // still nil

    )
    }.slice()) {
        {
            var (got, want) = (errors.Unwrap(test.err), test.wantUnwrap); if (!AreEqual(got, want)) {
                Ꮡt.Errorf("Formatted error: %v\nerrors.Unwrap() = %v, want %v"u8, test.err, got, want);
            }
        }
        {
            var (got, want) = (splitErr(test.err), test.wantSplit); if (!reflect.DeepEqual(got, want)) {
                Ꮡt.Errorf("Formatted error: %v\nUnwrap() []error = %v, want %v"u8, test.err, got, want);
            }
        }
        {
            @string got = test.err.Error();
            @string want = test.wantText; if (got != want) {
                Ꮡt.Errorf("err.Error() = %q, want %q"u8, got, want);
            }
        }
    }
}

internal partial interface splitErr_type /*dyn*/ {
    slice<error> Unwrap();
}

internal static slice<error> splitErr(error err) {
    {
        var (e, ok) = err._<splitErr_type>(ᐧ); if (ok) {
            return e.Unwrap();
        }
    }
    return default!;
}

partial struct errString /*@string*/;

internal static @string Error(this errString e) {
    return ((@string)e);
}

} // end fmt_test_package

// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements a parser test harness. The files in the testdata
// directory are parsed and the errors reported are compared against the
// error messages expected in the test files. The test files must end in
// .src rather than .go so that they are not disturbed by gofmt runs.
//
// Expected errors are indicated in the test files by putting a comment
// of the form /* ERROR "rx" */ immediately following an offending token.
// The harness will verify that an error matching the regular expression
// rx is reported at that source position.
//
// For instance, the following test file indicates that a "not declared"
// error should be reported for the undeclared variable x:
//
//	package p
//	func f() {
//		_ = x /* ERROR "not declared" */ + 1
//	}
namespace go.go;

using flag = flag_package;
using scanner = global::go.go.scanner_package;
using token = global::go.go.token_package;
using os = os_package;
using filepath = path.filepath_package;
using regexp = regexp_package;
using strings = strings_package;
using testing = testing_package;
using ast = global::go.go.ast_package;
using fs = global::go.io.fs_package;
using global::go.go;
using global::go.io;
using path;
using static global::go.go.parser_package;

partial class parser_internal_test_package {

internal static ж<bool> traceErrs = flag.Bool("trace_errs"u8, false, "whether to enable tracing for error tests"u8);

internal static readonly @string testdata = "testdata"u8;

// getFile assumes that each filename occurs at most once
internal static ж<tokenꓸFile> /*file*/ getFile(ж<token.FileSet> Ꮡfset, @string filename) {
    ж<tokenꓸFile> @file = default!;

    Ꮡfset.Iterate((ж<tokenꓸFile> f) => {
        if (f.Name() == filename) {
            if (@file != nil) {
                throw panic(filename + " used multiple times");
            }
            @file = f;
        }
        return true;
    });
    return @file;
}

internal static tokenꓸPos getPos(ж<token.FileSet> Ꮡfset, @string filename, nint offset) {
    {
        var f = getFile(Ꮡfset, filename); if (f != nil) {
            return f.Pos(offset);
        }
    }
    return token.NoPos;
}

// ERROR comments must be of the form /* ERROR "rx" */ and rx is
// a regular expression that matches the expected error message.
// The special form /* ERROR HERE "rx" */ must be used for error
// messages that appear immediately after a token, rather than at
// a token's position, and ERROR AFTER means after the comment
// (e.g. at end of line).
internal static ж<regexp.Regexp> errRx = regexp.MustCompile(@"^/\* *ERROR *(HERE|AFTER)? *""([^""]*)"" *\*/$"u8);

// expectedErrors collects the regular expressions of ERROR comments found
// in files and returns them as a map of error positions to error messages.
internal static map<tokenꓸPos, @string> expectedErrors(ж<token.FileSet> Ꮡfset, @string filename, slice<byte> src) {
    var errors = new map<tokenꓸPos, @string>();

    scanner.Scanner s = default!;
    // file was parsed already - do not add it again to the file
    // set otherwise the position information returned here will
    // not match the position information collected by the parser
    s.Init(getFile(Ꮡfset, filename), src, default!, scanner.ScanComments);
    tokenꓸPos prev = default!;                // position of last non-comment, non-semicolon token
    tokenꓸPos here = default!;                // position immediately after the token at position prev

    while (ᐧ) {
        var (pos, tok, lit) = s.Scan();
        var exprᴛ1 = tok;
        var matchᴛ1 = false;
        if (exprᴛ1 == token.EOF) { matchᴛ1 = true;
            return errors;
        }
        if (exprᴛ1 == token.COMMENT) { matchᴛ1 = true;
            var sΔ2 = errRx.FindStringSubmatch(lit);
            if (len(sΔ2) == 3) {
                if (sΔ2[1] == "HERE"){
                    pos = here; // start of comment
                } else 
                if (sΔ2[1] == "AFTER"){
                    pos += ((tokenꓸPos)len(lit)); // end of comment
                } else {
                    pos = prev; // token prior to comment
                }
                errors[pos] = sΔ2[2];
            }
        }
        else if (exprᴛ1 == token.SEMICOLON) { matchᴛ1 = true;
            do {
                // don't use the position of auto-inserted (invisible) semicolons
                if (lit != ";"u8) {
                    break;
                }
                fallthrough = true;
            } while (false);
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            prev = pos;
            nint l = default!;       // token length
            if (tok.IsLiteral()){
                l = len(lit);
            } else {
                l = len(tok.String());
            }
            here = prev + ((tokenꓸPos)l);
        }
    }
}

// compareErrors compares the map of expected error messages with the list
// of found errors and reports discrepancies.
internal static void compareErrors(ж<testing.T> Ꮡt, ж<token.FileSet> Ꮡfset, map<tokenꓸPos, @string> expected, scanner.ErrorList found) {
    Ꮡt.Helper();
    foreach (var (_, error) in found) {
        // error.Pos is a token.Position, but we want
        // a token.Pos so we can do a map lookup
        tokenꓸPos pos = getPos(Ꮡfset, (~error).Pos.Filename, (~error).Pos.Offset);
        {
            var (msg, foundΔ1) = expected[pos, ꟷ]; if (foundΔ1){
                // we expect a message at pos; check if it matches
                var (rx, err) = regexp.Compile(msg);
                if (err != default!) {
                    Ꮡt.Errorf("%s: %v"u8, (~error).Pos, err);
                    continue;
                }
                {
                    var match = rx.MatchString((~error).Msg); if (!match) {
                        Ꮡt.Errorf("%s: %q does not match %q"u8, (~error).Pos, (~error).Msg, msg);
                        continue;
                    }
                }
                // we have a match - eliminate this error
                delete(expected, pos);
            } else {
                // To keep in mind when analyzing failed test output:
                // If the same error position occurs multiple times in errors,
                // this message will be triggered (because the first error at
                // the position removes this position from the expected errors).
                Ꮡt.Errorf("%s: unexpected error: %s"u8, (~error).Pos, (~error).Msg);
            }
        }
    }

    // there should be no expected errors left
    if (len(expected) > 0) {
        Ꮡt.Errorf("%d errors not reported:"u8, len(expected));
        foreach (var (pos, msg) in expected) {
            Ꮡt.Errorf("%s: %s\n"u8, Ꮡfset.Position(pos), msg);
        }
    }
}

internal static void checkErrors(ж<testing.T> Ꮡt, @string filename, any input, global::go.go.parser_package.Mode mode, bool expectErrors) {
    Ꮡt.Helper();
    var (src, err) = readSource(filename, input);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }

    var fset = token.NewFileSet();
    (_, err) = ParseFile(fset, filename, src, mode);
    ref var found = ref heap<scanner.ErrorList>(out var Ꮡfound);
    (found, var ok) = err._<scanner.ErrorList>(ᐧ);
    if (err != default! && !ok) {
        Ꮡt.Error(err);
        return;
    }
    Ꮡfound.RemoveMultiples();

    var expected = new map<tokenꓸPos, @string>{};
    if (expectErrors) {
        // we are expecting the following errors
        // (collect these after parsing a file so that it is found in the file set)
        expected = expectedErrors(fset, filename, src);
    }

    // verify errors returned by the parser
    compareErrors(Ꮡt, fset, expected, found);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string srcˢ = ".src"u8;
internal static readonly @string go2ˢ = ".go2"u8;

public static void TestErrors(ж<testing.T> Ꮡt) {
    var (list, err) = os.ReadDir(testdata);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, d) in list) {
        @string name = d.Name();
        var dʗ1 = d;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            if (!dʗ1.IsDir() && !strings.HasPrefix(name, "."u8) && (strings.HasSuffix(name, srcˢ) || strings.HasSuffix(name, go2ˢ))) {
                global::go.go.parser_package.Mode mode = (global::go.go.parser_package.Mode)(DeclarationErrors | AllErrors);
                if (traceErrs.Value) {
                    mode |= (global::go.go.parser_package.Mode)(Trace);
                }
                checkErrors(tΔ1, filepath.Join(testdata, name), default!, mode, true);
            }
        });
    }
}

} // end parser_internal_test_package

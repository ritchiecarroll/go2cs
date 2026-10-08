// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using regexp = regexp_package;
using strconv = strconv_package;
using strings = strings_package;
using io = io_package;

partial class testtrace_package {

// Expectation represents the expected result of some operation.
partial struct Expectation {
    internal bool failure;
    internal ж<regexp.Regexp> errorMatcher;
}

// ExpectSuccess returns an Expectation that trivially expects success.
public static ж<Expectation> ExpectSuccess() {
    return @new<Expectation>();
}

// Check validates whether err conforms to the expectation. Returns
// an error if it does not conform.
//
// Conformance means that if failure is true, then err must be non-nil.
// If err is non-nil, then it must match errorMatcher.
public static error Check(this ref Expectation e, error err) {
    if (!e.failure && err != default!) {
        return fmt.Errorf("unexpected error while reading the trace: %v"u8, err);
    }
    if (e.failure && err == default!) {
        return fmt.Errorf("expected error while reading the trace: want something matching %q, got none"u8, e.errorMatcher.OrTypedNil());
    }
    if (e.failure && err != default! && !e.errorMatcher.MatchString(err.Error())) {
        return fmt.Errorf("unexpected error while reading the trace: want something matching %q, got %s"u8, e.errorMatcher.OrTypedNil(), err.Error());
    }
    return default!;
}

// ParseExpectation parses the serialized form of an Expectation.
public static (ж<Expectation>, error) ParseExpectation(slice<byte> data) {
    var exp = @new<Expectation>();
    var s = bufio.NewScanner(new bytes_ReaderжReader(bytes.NewReader(data)));
    if (s.Scan()) {
        var c = strings.SplitN(s.Text(), " "u8, 2);
        var exprᴛ1 = c[0];
        if (exprᴛ1 == "SUCCESS"u8) {
        }
        else if (exprᴛ1 == "FAILURE"u8) {
            exp.Value.failure = true;
            if (len(c) != 2) {
                return (exp, fmt.Errorf("bad header line for FAILURE: %q"u8, s.Text()));
            }
            var (matcher, err) = parseMatcher(c[1]);
            if (err != default!) {
                return (exp, err);
            }
            exp.Value.errorMatcher = matcher;
        }
        else { /* default: */
            return (exp, fmt.Errorf("bad header line: %q"u8, s.Text()));
        }

        return (exp, default!);
    }
    return (exp, s.Err());
}

internal static (ж<regexp.Regexp>, error) parseMatcher(@string quoted) {
    var (pattern, err) = strconv.Unquote(quoted);
    if (err != default!) {
        return (default!, fmt.Errorf("malformed pattern: not correctly quoted: %s: %v"u8, quoted, err));
    }
    (var matcher, err) = regexp.Compile(pattern);
    if (err != default!) {
        return (default!, fmt.Errorf("malformed pattern: not a valid regexp: %s: %v"u8, pattern, err));
    }
    return (matcher, default!);
}

} // end testtrace_package

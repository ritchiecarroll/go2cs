// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using token = global::go.go.token_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = global::go.unicode.utf8_package;
using global::go.go;
using global::go.unicode;

partial class ast_package {

// A Directive is a comment of this form:
//
//	//tool:name args
//
// For example, this directive:
//
//	//go:generate stringer -type Op -trimprefix Op
//
// would have Tool "go", Name "generate", and Args "stringer -type Op
// -trimprefix Op".
//
// While Args does not have a strict syntax, by convention it is a
// space-separated sequence of unquoted words, '"'-quoted Go strings, or
// '`'-quoted raw strings.
//
// See https://go.dev/doc/comment#directives for specification.
partial struct Directive {
    public @string Tool;
    public @string Name;
    public @string Args; // no leading or trailing whitespace
    // Slash is the position of the "//" at the beginning of the directive.
    public tokenꓸPos Slash;
    // ArgsPos is the position where Args begins, based on the position passed
    // to ParseDirective.
    public tokenꓸPos ArgsPos;
}

// ParseDirective parses a single comment line for a directive comment.
//
// If the line is not a directive comment, it returns false.
//
// The provided text must be a single line and should include the leading "//".
// If the text does not start with "//", it returns false.
//
// The caller may provide a file position of the start of c. This will be used
// to track the position of the arguments. This may be [Comment.Slash],
// synthesized by the caller, or simply 0. If the caller passes 0, then the
// positions are effectively byte offsets into the string c.
public static (Directive, bool) ParseDirective(tokenꓸPos pos, @string c) {
    // Fast path to eliminate most non-directive comments. Must be a line
    // comment starting with [a-z0-9]
    if (!(len(c) >= 3 && c[0] == (rune)'/' && c[1] == (rune)'/' && isalnum(c[2]))) {
        return (new Directive(nil), false);
    }

    var buf = new directiveScanner(c, pos);
    buf.skip(len("//"));

    // Check for a valid directive and parse tool part.
    //
    // This logic matches isDirective. (We could combine them, but isDirective
    // itself is duplicated in several places.)
    nint colon = strings.Index(buf.str, ":"u8);
    if (colon <= 0 || colon + 1 >= len(buf.str)) {
        return (new Directive(nil), false);
    }
    for (nint i = 0; i <= colon + 1; i++) {
        if (i == colon) {
            continue;
        }
        if (!isalnum(buf.str[i])) {
            return (new Directive(nil), false);
        }
    }
    @string tool = buf.take(colon);
    buf.skip(len(":"));

    // Parse name and args.
    @string name = buf.takeNonSpace();
    buf.skipSpace();
    tokenꓸPos argsPos = buf.pos;
    @string args = strings.TrimRightFunc(buf.str, unicode.IsSpace);

    return (new Directive(tool, name, args, pos, argsPos), true);
}

internal static bool isalnum(byte b) {
    return (rune)'a' <= b && b <= (rune)'z' || (rune)'0' <= b && b <= (rune)'9';
}

public static tokenꓸPos Pos(this ref Directive d) {
    return d.Slash;
}

public static tokenꓸPos End(this ref Directive d) {
    return (tokenꓸPos)((nint)d.ArgsPos + len(d.Args));
}

// A DirectiveArg is an argument to a directive comment.
partial struct DirectiveArg {
    // Arg is the parsed argument string. If the argument was a quoted string,
    // this is its unquoted form.
    public @string Arg;
    // Pos is the position of the first character in this argument.
    public tokenꓸPos Pos;
}

// ParseArgs parses a [Directive]'s arguments using the standard convention,
// which is a sequence of tokens, where each token may be a bare word, or a
// double quoted Go string, or a back quoted raw Go string. Each token must be
// separated by one or more Unicode spaces.
//
// If the arguments do not conform to this syntax, it returns an error.
public static (slice<DirectiveArg>, error) ParseArgs(this ref Directive d) {
    var args = new directiveScanner(d.Args, d.ArgsPos);

    var list = new DirectiveArg[]{}.slice();
    for (args.skipSpace(); args.str != ""u8; args.skipSpace()) {
        @string arg = default!;
        tokenꓸPos argPos = args.pos;

        switch (args.str[0]) {
        default: {
            arg = args.takeNonSpace();
            break;
        }
        case (rune)'`' or (rune)'"': {
            var (q, err) = strconv.QuotedPrefix(args.str);
            if (err != default!) { // Always strconv.ErrSyntax
                return (default!, fmt.Errorf("invalid quoted string in //%s:%s: %s"u8, d.Tool, d.Name, args.str));
            }
            // Any errors will have been returned by QuotedPrefix
            (arg, _) = strconv.Unquote(args.take(len(q)));

            // Check that the quoted string is followed by a space (or nothing)
            if (args.str != ""u8) {
                var (r, _) = utf8.DecodeRuneInString(args.str);
                if (!unicode.IsSpace(r)) {
                    return (default!, fmt.Errorf("invalid quoted string in //%s:%s: %s"u8, d.Tool, d.Name, args.str));
                }
            }
            break;
        }}

        list = append(list, new DirectiveArg(arg, argPos));
    }
    return (list, default!);
}

// directiveScanner is a helper for parsing directive comments while maintaining
// position information.
partial struct directiveScanner {
    internal @string str;
    internal tokenꓸPos pos;
}

internal static void skip(this ref directiveScanner s, nint n) {
    s.pos += ((tokenꓸPos)n);
    s.str = s.str.slice(n);
}

internal static @string take(this ref directiveScanner s, nint n) {
    @string res = s.str.slice(0, n);
    s.skip(n);
    return res;
}

internal static @string takeNonSpace(this ref directiveScanner s) {
    nint i = strings.IndexFunc(s.str, unicode.IsSpace);
    if (i == -1) {
        i = len(s.str);
    }
    return s.take(i);
}

internal static void skipSpace(this ref directiveScanner s) {
    @string trim = strings.TrimLeftFunc(s.str, unicode.IsSpace);
    s.skip(len(s.str) - len(trim));
}

} // end ast_package

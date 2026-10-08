// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmtsort = @internal.fmtsort_package;
using Δio = io_package;
using os = os_package;
using reflect = reflect_package;
using strconv = strconv_package;
using Δsync = sync_package;
using utf8 = unicode.utf8_package;
using @internal;
using @unsafe = unsafe_package;
using unicode;
using ꓸꓸꓸany = Span<any>;

partial class fmt_package {

// Strings for use with buffer.WriteString.
// This is less overhead than using buffer.Write with byte arrays.
internal static readonly @string commaSpaceString = ", "u8;

internal static readonly @string nilAngleString = "<nil>"u8;

internal static readonly @string nilParenString = "(nil)"u8;

internal static readonly @string nilString = "nil"u8;

internal static readonly @string mapString = "map["u8;

internal static readonly @string percentBangString = "%!"u8;

internal static readonly @string missingString = "(MISSING)"u8;

internal static readonly @string badIndexString = "(BADINDEX)"u8;

internal static readonly @string panicString = "(PANIC="u8;

internal static readonly @string extraString = "%!(EXTRA "u8;

internal static readonly @string badWidthString = "%!(BADWIDTH)"u8;

internal static readonly @string badPrecString = "%!(BADPREC)"u8;

internal static readonly @string noVerbString = "%!(NOVERB)"u8;

internal static readonly @string invReflectString = "<invalid reflect.Value>"u8;

// State represents the printer state passed to custom formatters.
// It provides access to the [io.Writer] interface plus information about
// the flags and options for the operand's format specifier.
partial interface State :
    Δio.Writer
{
    // Width returns the value of the width option and whether it has been set.
    (nint wid, bool ok) Width();
    // Precision returns the value of the precision option and whether it has been set.
    (nint prec, bool ok) Precision();
    // Flag reports whether the flag c, a character, has been set.
    bool Flag(nint c);
}

// Formatter is implemented by any value that has a Format method.
// The implementation controls how [State] and rune are interpreted,
// and may call [Sprint] or [Fprint](f) etc. to generate its output.
partial interface Formatter {
    void Format(State f, rune verb);
}

// Stringer is implemented by any value that has a String method,
// which defines the “native” format for that value.
// The String method is used to print values passed as an operand
// to any format that accepts a string or to an unformatted printer
// such as [Print].
partial interface Stringer {
    @string String();
}

// GoStringer is implemented by any value that has a GoString method,
// which defines the Go syntax for that value.
// The GoString method is used to print values passed as an operand
// to a %#v format.
partial interface GoStringer {
    @string GoString();
}

// FormatString returns a string representing the fully qualified formatting
// directive captured by the [State], followed by the argument verb. ([State] does not
// itself contain the verb.) The result has a leading percent sign followed by any
// flags, the width, and the precision. Missing flags, width, and precision are
// omitted. This function allows a [Formatter] to reconstruct the original
// directive triggering the call to Format.
public static @string FormatString(State state, rune verb) {
    array<byte> tmp = new(16);               // Use a local buffer.
    var b = append(tmp[..0], (byte)((rune)'%'));
    foreach (var (_, c) in (@string)" +-#0"u8) {
        // All known flags
        if (state.Flag((nint)c)) {
            // The argument is an int for historical reasons.
            b = append(b, (byte)c);
        }
    }
    {
        var (w, ok) = state.Width(); if (ok) {
            b = strconv.AppendInt(b, (int64)w, 10);
        }
    }
    {
        var (p, ok) = state.Precision(); if (ok) {
            b = append(b, (byte)((rune)'.'));
            b = strconv.AppendInt(b, (int64)p, 10);
        }
    }
    b = utf8.AppendRune(b, verb);
    return ((@string)b);
}

partial struct buffer /*[]byte*/;

internal static void write(this ref buffer b, slice<byte> p) {
    b = appendꓸꓸꓸ(b, p);
}

internal static void writeString(this ref buffer b, sstring s) {
    b = append(b, s.ꓸꓸꓸ);
}

internal static void writeByte(this ref buffer b, byte c) {
    b = append(b, c);
}

internal static void writeRune(this ref buffer b, rune r) {
    b = utf8.AppendRune(b, r);
}

// pp is used to store a printer's state and is reused with sync.Pool to avoid allocations.
partial struct pp {
    internal buffer buf;
    // arg holds the current item, as an interface{}.
    internal any arg;
    // value is used instead of arg for reflect values.
    internal reflectꓸValue value;
    // fmt is used to format basic items such as integers or strings.
    internal fmt fmt;
    // reordered records whether the format string used argument reordering.
    internal bool reordered;
    // goodArgNum records whether the most recent reordering directive was valid.
    internal bool goodArgNum;
    // panicking is set by catchPanic to avoid infinite panic, recover, panic, ... recursion.
    internal bool panicking;
    // erroring is set when printing an error string to guard against calling handleMethods.
    internal bool erroring;
    // wrapErrs is set when the format string may contain a %w verb.
    internal bool wrapErrs;
    // wrappedErrs records the targets of the %w verb.
    internal slice<nint> wrappedErrs;
}

internal static ж<Δsync.Pool> ᏑppFree = new StandardBox<Δsync.Pool>(new Δsync.Pool(
    New: () => @new<pp>()
));
internal static ref Δsync.Pool ppFree => ref ᏑppFree.Value;

// newPrinter allocates a new pp struct or grabs a cached one.
internal static ж<pp> newPrinter() {
    var p = ᏑppFree.Get()._<ж<pp>>();
    p.Value.panicking = false;
    p.Value.erroring = false;
    p.Value.wrapErrs = false;
    p.of(pp.Ꮡfmt).init(p.of(pp.Ꮡbuf));
    return p;
}

// free saves used pp structs in ppFree; avoids an allocation per invocation.
internal static void free(this ж<pp> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    // Proper usage of a sync.Pool requires each entry to have approximately
    // the same memory cost. To obtain this property when the stored type
    // contains a variably-sized buffer, we add a hard limit on the maximum
    // buffer to place back in the pool. If the buffer is larger than the
    // limit, we drop the buffer and recycle just the printer.
    //
    // See https://golang.org/issue/23199.
    if (cap(p.buf) > 64 * 1024){
        p.buf = default!;
    } else {
        p.buf = p.buf[..0];
    }
    if (cap(p.wrappedErrs) > 8) {
        p.wrappedErrs = default!;
    }
    p.arg = default!;
    p.value = new reflectꓸValue(nil);
    p.wrappedErrs = p.wrappedErrs[..0];
    ᏑppFree.Put(Ꮡp.OrTypedNil());
}

internal static (nint wid, bool ok) Width(this ref pp p) {
    return (p.fmt.wid, p.fmt.widPresent);
}

internal static (nint prec, bool ok) Precision(this ref pp p) {
    return (p.fmt.prec, p.fmt.precPresent);
}

internal static bool Flag(this ref pp p, nint b) {
    switch (b) {
    case (rune)'-': {
        return p.fmt.minus;
    }
    case (rune)'+': {
        return p.fmt.plus || p.fmt.plusV;
    }
    case (rune)'#': {
        return p.fmt.sharp || p.fmt.sharpV;
    }
    case (rune)' ': {
        return p.fmt.space;
    }
    case (rune)'0': {
        return p.fmt.zero;
    }}

    return false;
}

// Write implements [io.Writer] so we can call [Fprintf] on a pp (through [State]), for
// recursive use in custom verbs.
internal static (nint ret, error err) Write(this ref pp p, slice<byte> b) {
    p.buf.write(b);
    return (len(b), default!);
}

// WriteString implements [io.StringWriter] so that we can call [io.WriteString]
// on a pp (through state), for efficiency.
internal static (nint ret, error err) WriteString(this ref pp p, @string s) {
    p.buf.writeString(s);
    return (len(s), default!);
}

// These routines end in 'f' and take a format string.

// Fprintf formats according to a format specifier and writes to w.
// It returns the number of bytes written and any write error encountered.
public static (nint n, error err) Fprintf(Δio.Writer w, sstring format, params ꓸꓸꓸany aʗp) {
    nint n = default!;
    error err = default!;
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrintf(format, a);
    (n, err) = w.Write((~p).buf);
    p.free();
    return (n, err);
}

// Printf formats according to a format specifier and writes to standard output.
// It returns the number of bytes written and any write error encountered.
public static (nint n, error err) Printf(sstring format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    return Fprintf(new os.FileжWriter(os.Stdout), format, a.ꓸꓸꓸ);
}

// Sprintf formats according to a format specifier and returns the resulting string.
public static @string Sprintf(sstring format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrintf(format, a);
    @string s = ((@string)(slice<byte>)(~p).buf);
    p.free();
    return s;
}

// Appendf formats according to a format specifier, appends the result to the byte
// slice, and returns the updated slice.
public static slice<byte> Appendf(slice<byte> b, sstring format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrintf(format, a);
    b = appendꓸꓸꓸ(b, (~p).buf);
    p.free();
    return b;
}

// These routines do not take a format string

// Fprint formats using the default formats for its operands and writes to w.
// Spaces are added between operands when neither is a string.
// It returns the number of bytes written and any write error encountered.
public static (nint n, error err) Fprint(Δio.Writer w, params ꓸꓸꓸany aʗp) {
    nint n = default!;
    error err = default!;
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrint(a);
    (n, err) = w.Write((~p).buf);
    p.free();
    return (n, err);
}

// Print formats using the default formats for its operands and writes to standard output.
// Spaces are added between operands when neither is a string.
// It returns the number of bytes written and any write error encountered.
public static (nint n, error err) Print(params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    return Fprint(new os.FileжWriter(os.Stdout), a.ꓸꓸꓸ);
}

// Sprint formats using the default formats for its operands and returns the resulting string.
// Spaces are added between operands when neither is a string.
public static @string Sprint(params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrint(a);
    @string s = ((@string)(slice<byte>)(~p).buf);
    p.free();
    return s;
}

// Append formats using the default formats for its operands, appends the result to
// the byte slice, and returns the updated slice.
public static slice<byte> Append(slice<byte> b, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrint(a);
    b = appendꓸꓸꓸ(b, (~p).buf);
    p.free();
    return b;
}

// These routines end in 'ln', do not take a format string,
// always add spaces between operands, and add a newline
// after the last operand.

// Fprintln formats using the default formats for its operands and writes to w.
// Spaces are always added between operands and a newline is appended.
// It returns the number of bytes written and any write error encountered.
public static (nint n, error err) Fprintln(Δio.Writer w, params ꓸꓸꓸany aʗp) {
    nint n = default!;
    error err = default!;
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrintln(a);
    (n, err) = w.Write((~p).buf);
    p.free();
    return (n, err);
}

// Println formats using the default formats for its operands and writes to standard output.
// Spaces are always added between operands and a newline is appended.
// It returns the number of bytes written and any write error encountered.
public static (nint n, error err) Println(params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    return Fprintln(new os.FileжWriter(os.Stdout), a.ꓸꓸꓸ);
}

// Sprintln formats using the default formats for its operands and returns the resulting string.
// Spaces are always added between operands and a newline is appended.
public static @string Sprintln(params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrintln(a);
    @string s = ((@string)(slice<byte>)(~p).buf);
    p.free();
    return s;
}

// Appendln formats using the default formats for its operands, appends the result
// to the byte slice, and returns the updated slice. Spaces are always added
// between operands and a newline is appended.
public static slice<byte> Appendln(slice<byte> b, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    var p = newPrinter();
    p.doPrintln(a);
    b = appendꓸꓸꓸ(b, (~p).buf);
    p.free();
    return b;
}

// getField gets the i'th field of the struct value.
// If the field itself is a non-nil interface, return a value for
// the thing inside the interface, not the interface itself.
internal static reflectꓸValue getField(reflectꓸValue v, nint i) {
    var val = v.Field(i);
    if (val.Kind() == reflect.ΔInterface && !val.IsNil()) {
        val = val.Elem();
    }
    return val;
}

// tooLarge reports whether the magnitude of the integer is
// too large to be used as a formatting width or precision.
internal static bool tooLarge(nint x) {
    const nint max = 1000000;
    return x > max || x < -max;
}

// parsenum converts ASCII to integer.  num is 0 (and isnum is false) if no number present.
internal static (nint num, bool isnum, nint newi) parsenum(sstring s, nint start, nint end) {
    nint num = default!;
    bool isnum = default!;
    nint newi = default!;

    if (start >= end) {
        return (0, false, end);
    }
    for (newi = start; newi < end && (rune)'0' <= s[newi] && s[newi] <= (rune)'9'; newi++) {
        if (tooLarge(num)) {
            return (0, false, end); // Overflow; crazy long number most likely.
        }
        num = num * 10 + (nint)((byte)(s[newi] - (rune)'0'));
        isnum = true;
    }
    return (num, isnum, newi);
}

internal static void unknownType(this ref pp p, reflectꓸValue v) {
    if (!v.IsValid()) {
        p.buf.writeString(nilAngleString);
        return;
    }
    p.buf.writeByte((rune)'?');
    p.buf.writeString(v.Type().String());
    p.buf.writeByte((rune)'?');
}

internal static void badVerb(this ж<pp> Ꮡp, rune verb) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.erroring = true;
    p.buf.writeString(percentBangString);
    p.buf.writeRune(verb);
    p.buf.writeByte((rune)'(');
    switch (ᐧ) {
    case {} when p.arg != default!: {
        p.buf.writeString(reflect.TypeOf(p.arg).String());
        p.buf.writeByte((rune)'=');
        Ꮡp.printArg(p.arg, (rune)'v');
        break;
    }
    case {} when p.value.IsValid(): {
        p.buf.writeString(p.value.Type().String());
        p.buf.writeByte((rune)'=');
        Ꮡp.printValue(p.value, (rune)'v', 0);
        break;
    }
    default: {
        p.buf.writeString(nilAngleString);
        break;
    }}

    p.buf.writeByte((rune)')');
    p.erroring = false;
}

internal static void fmtBool(this ж<pp> Ꮡp, bool v, rune verb) {
    ref var p = ref Ꮡp.DerefOrNull();

    switch (verb) {
    case (rune)'t' or (rune)'v': {
        p.fmt.fmtBoolean(v);
        break;
    }
    default: {
        Ꮡp.badVerb(verb);
        break;
    }}

}

// fmt0x64 formats a uint64 in hexadecimal and prefixes it with 0x or
// not, as requested, by temporarily setting the sharp flag.
internal static void fmt0x64(this ref pp p, uint64 v, bool leading0x) {
    var sharp = p.fmt.sharp;
    p.fmt.sharp = leading0x;
    p.fmt.fmtInteger(v, 16, unsigned, (rune)'v', ldigits);
    p.fmt.sharp = sharp;
}

// fmtInteger formats a signed or unsigned integer.
internal static void fmtInteger(this ж<pp> Ꮡp, uint64 v, bool isSigned, rune verb) {
    ref var p = ref Ꮡp.DerefOrNull();

    switch (verb) {
    case (rune)'v': {
        if (p.fmt.sharpV && !isSigned){
            p.fmt0x64(v, true);
        } else {
            p.fmt.fmtInteger(v, 10, isSigned, verb, ldigits);
        }
        break;
    }
    case (rune)'d': {
        p.fmt.fmtInteger(v, 10, isSigned, verb, ldigits);
        break;
    }
    case (rune)'b': {
        p.fmt.fmtInteger(v, 2, isSigned, verb, ldigits);
        break;
    }
    case (rune)'o' or (rune)'O': {
        p.fmt.fmtInteger(v, 8, isSigned, verb, ldigits);
        break;
    }
    case (rune)'x': {
        p.fmt.fmtInteger(v, 16, isSigned, verb, ldigits);
        break;
    }
    case (rune)'X': {
        p.fmt.fmtInteger(v, 16, isSigned, verb, udigits);
        break;
    }
    case (rune)'c': {
        p.fmt.fmtC(v);
        break;
    }
    case (rune)'q': {
        p.fmt.fmtQc(v);
        break;
    }
    case (rune)'U': {
        p.fmt.fmtUnicode(v);
        break;
    }
    default: {
        Ꮡp.badVerb(verb);
        break;
    }}

}

// fmtFloat formats a float. The default precision for each verb
// is specified as last argument in the call to fmt_float.
internal static void fmtFloat(this ж<pp> Ꮡp, float64 v, nint size, rune verb) {
    ref var p = ref Ꮡp.DerefOrNull();

    switch (verb) {
    case (rune)'v': {
        p.fmt.fmtFloat(v, size, (rune)'g', -1);
        break;
    }
    case (rune)'b' or (rune)'g' or (rune)'G' or (rune)'x' or (rune)'X': {
        p.fmt.fmtFloat(v, size, verb, -1);
        break;
    }
    case (rune)'f' or (rune)'e' or (rune)'E': {
        p.fmt.fmtFloat(v, size, verb, 6);
        break;
    }
    case (rune)'F': {
        p.fmt.fmtFloat(v, size, (rune)'f', 6);
        break;
    }
    default: {
        Ꮡp.badVerb(verb);
        break;
    }}

}

// fmtComplex formats a complex number v with
// r = real(v) and j = imag(v) as (r+ji) using
// fmtFloat for r and j formatting.
internal static void fmtComplex(this ж<pp> Ꮡp, complex128 v, nint size, rune verb) {
    ref var p = ref Ꮡp.DerefOrNull();

    // Make sure any unsupported verbs are found before the
    // calls to fmtFloat to not generate an incorrect error string.
    switch (verb) {
    case (rune)'v' or (rune)'b' or (rune)'g' or (rune)'G' or (rune)'x' or (rune)'X' or (rune)'f' or (rune)'F' or (rune)'e' or (rune)'E': {
        var oldPlus = p.fmt.plus;
        p.buf.writeByte((rune)'(');
        Ꮡp.fmtFloat(real(v), size / 2, verb);
        p.fmt.plus = true;
        Ꮡp.fmtFloat(imag(v), // Imaginary part always has a sign.
 size / 2, verb);
        p.buf.writeString("i)"u8);
        p.fmt.plus = oldPlus;
        break;
    }
    default: {
        Ꮡp.badVerb(verb);
        break;
    }}

}

internal static void fmtString(this ж<pp> Ꮡp, @string v, rune verb) {
    ref var p = ref Ꮡp.DerefOrNull();

    switch (verb) {
    case (rune)'v': {
        if (p.fmt.sharpV){
            p.fmt.fmtQ(v);
        } else {
            p.fmt.fmtS(v);
        }
        break;
    }
    case (rune)'s': {
        p.fmt.fmtS(v);
        break;
    }
    case (rune)'x': {
        p.fmt.fmtSx(v, ldigits);
        break;
    }
    case (rune)'X': {
        p.fmt.fmtSx(v, udigits);
        break;
    }
    case (rune)'q': {
        p.fmt.fmtQ(v);
        break;
    }
    default: {
        Ꮡp.badVerb(verb);
        break;
    }}

}

internal static void fmtBytes(this ж<pp> Ꮡp, slice<byte> v, rune verb, sstring typeString) {
    ref var p = ref Ꮡp.DerefOrNull();

    switch (verb) {
    case (rune)'v' or (rune)'d': {
        if (p.fmt.sharpV){
            p.buf.writeString(typeString);
            if (v == default!) {
                p.buf.writeString(nilParenString);
                return;
            }
            p.buf.writeByte((rune)'{');
            foreach (var (i, c) in v) {
                if (i > 0) {
                    p.buf.writeString(commaSpaceString);
                }
                p.fmt0x64((uint64)c, true);
            }
            p.buf.writeByte((rune)'}');
        } else {
            p.buf.writeByte((rune)'[');
            foreach (var (i, c) in v) {
                if (i > 0) {
                    p.buf.writeByte((rune)' ');
                }
                p.fmt.fmtInteger((uint64)c, 10, unsigned, verb, ldigits);
            }
            p.buf.writeByte((rune)']');
        }
        break;
    }
    case (rune)'s': {
        p.fmt.fmtBs(v);
        break;
    }
    case (rune)'x': {
        p.fmt.fmtBx(v, ldigits);
        break;
    }
    case (rune)'X': {
        p.fmt.fmtBx(v, udigits);
        break;
    }
    case (rune)'q': {
        p.fmt.fmtQ(((@string)v));
        break;
    }
    default: {
        Ꮡp.printValue(reflect.ValueOf(v), verb, 0);
        break;
    }}

}

internal static void fmtPointer(this ж<pp> Ꮡp, reflectꓸValue value, rune verb) {
    ref var p = ref Ꮡp.DerefOrNull();

    uintptr u = default!;
    var exprᴛ1 = value.Kind();
    if (exprᴛ1 == reflect.Chan || exprᴛ1 == reflect.Func || exprᴛ1 == reflect.Map || exprᴛ1 == reflect.ΔPointer || exprᴛ1 == reflect.ΔSlice || exprᴛ1 == reflect.ΔUnsafePointer) {
        u = (uintptr)(uintptr)value.UnsafePointer();
    }
    else { /* default: */
        Ꮡp.badVerb(verb);
        return;
    }

    switch (verb) {
    case (rune)'v': {
        if (p.fmt.sharpV){
            p.buf.writeByte((rune)'(');
            p.buf.writeString(value.Type().String());
            p.buf.writeString(")("u8);
            if (u == 0){
                p.buf.writeString(nilString);
            } else {
                p.fmt0x64((uint64)u, true);
            }
            p.buf.writeByte((rune)')');
        } else {
            if (u == 0){
                p.fmt.padString(nilAngleString);
            } else {
                p.fmt0x64((uint64)u, !p.fmt.sharp);
            }
        }
        break;
    }
    case (rune)'p': {
        p.fmt0x64((uint64)u, !p.fmt.sharp);
        break;
    }
    case (rune)'b' or (rune)'o' or (rune)'d' or (rune)'x' or (rune)'X': {
        Ꮡp.fmtInteger((uint64)u, unsigned, verb);
        break;
    }
    default: {
        Ꮡp.badVerb(verb);
        break;
    }}

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string methodˢ = " method: "u8;

internal static void catchPanic(this ж<pp> Ꮡp, any arg, rune verb, @string method) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        {
            var err = recover(); if (err != default!) {
                // If it's a nil pointer, just say "<nil>". The likeliest causes are a
                // Stringer that fails to guard against nil or a nil pointer for a
                // value receiver, and in either case, "<nil>" is a nice result.
                {
                    var v = reflect.ValueOf(arg); if (v.Kind() == reflect.ΔPointer && v.IsNil()) {
                        p.buf.writeString(nilAngleString);
                        return;
                    }
                }
                // Otherwise print a concise panic message. Most of the time the panic
                // value will print itself nicely.
                if (p.panicking) {
                    // Nested panics; the recursion in printArg cannot succeed.
                    throw panic(err);
                }
                var oldFlags = p.fmt.fmtFlags;
                // For this output we want default behavior.
                p.fmt.clearflags();
                p.buf.writeString(percentBangString);
                p.buf.writeRune(verb);
                p.buf.writeString(panicString);
                p.buf.writeString(method);
                p.buf.writeString(methodˢ);
                p.panicking = true;
                Ꮡp.printArg(err, (rune)'v');
                p.panicking = false;
                p.buf.writeByte((rune)')');
                p.fmt.fmtFlags = oldFlags;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string formatˢ = "Format"u8;
internal static readonly @string goStringˢ = "GoString"u8;
internal static readonly @string errorˢ = "Error"u8;
internal static readonly @string stringˢ = "String"u8;

internal static bool /*handled*/ handleMethods(this ж<pp> Ꮡp, rune verb) {
    bool handled = default!;
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        if (p.erroring) {
            goto ᒐdone;
        }
        if (verb == (rune)'w') {
            // It is invalid to use %w other than with Errorf or with a non-error arg.
            var (_, ok) = p.arg._<error>(ᐧ);
            if (!ok || !p.wrapErrs) {
                Ꮡp.badVerb(verb);
                handled = true; goto ᒐdone;
            }
            // If the arg is a Formatter, pass 'v' as the verb to it.
            verb = (rune)'v';
        }
        // Is it a Formatter?
        {
            var (formatter, ok) = p.arg._<Formatter>(ᐧ); if (ok) {
                handled = true;
                defer(Ꮡp.catchPanic, Ꮡp.Value.arg, verb, formatˢ, ref ᒐ);
                formatter.Format(new ppжState(Ꮡp), verb);
                goto ᒐdone;
            }
        }
        // If we're doing Go syntax and the argument knows how to supply it, take care of it now.
        if (p.fmt.sharpV){
            {
                var (stringer, ok) = p.arg._<GoStringer>(ᐧ); if (ok) {
                    handled = true;
                    defer(Ꮡp.catchPanic, Ꮡp.Value.arg, verb, goStringˢ, ref ᒐ);
                    // Print the result of GoString unadorned.
                    p.fmt.fmtS(stringer.GoString());
                    goto ᒐdone;
                }
            }
        } else {
            // If a string is acceptable according to the format, see if
            // the value satisfies one of the string-valued interfaces.
            // Println etc. set verb to %v, which is "stringable".
            switch (verb) {
            case (rune)'v' or (rune)'s' or (rune)'x' or (rune)'X' or (rune)'q': {
                switch (p.arg.type()) {
                case {} Δv when Δv._<error>(out var v): {
                    handled = true;
                    defer(Ꮡp.catchPanic, Ꮡp.Value.arg, verb, errorˢ, ref ᒐ);
                    Ꮡp.fmtString(v.Error(), verb);
                    goto ᒐdone;
                }
                case {} Δv when Δv._<Stringer>(out var v): {
                    handled = true;
                    defer(Ꮡp.catchPanic, Ꮡp.Value.arg, verb, stringˢ, ref ᒐ);
                    Ꮡp.fmtString(v.String(), verb);
                    goto ᒐdone;
                }}
                break;
            }}

        }
        handled = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return handled;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string byteˢ = "[]byte"u8;

internal static void printArg(this ж<pp> Ꮡp, any arg, rune verb) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.arg = arg;
    p.value = new reflectꓸValue(nil);
    if (arg == default!) {
        switch (verb) {
        case (rune)'T' or (rune)'v': {
            p.fmt.padString(nilAngleString);
            break;
        }
        default: {
            Ꮡp.badVerb(verb);
            break;
        }}

        return;
    }
    // Special processing considerations.
    // %T (the value's type) and %p (its address) are special; we always do them first.
    switch (verb) {
    case (rune)'T': {
        p.fmt.fmtS(reflect.TypeOf(arg).String());
        return;
    }
    case (rune)'p': {
        Ꮡp.fmtPointer(reflect.ValueOf(arg), (rune)'p');
        return;
    }}

    // Some types can be done without reflection.
    switch (arg.type()) {
    case bool f: {
        Ꮡp.fmtBool(f, verb);
        break;
    }
    case float32 f: {
        Ꮡp.fmtFloat((float64)f, 32, verb);
        break;
    }
    case float64 f: {
        Ꮡp.fmtFloat(f, 64, verb);
        break;
    }
    case complex64 f: {
        Ꮡp.fmtComplex((complex128)f, 64, verb);
        break;
    }
    case complex128 f: {
        Ꮡp.fmtComplex(f, 128, verb);
        break;
    }
    case nint f: {
        Ꮡp.fmtInteger((uint64)f, signed, verb);
        break;
    }
    case int8 f: {
        Ꮡp.fmtInteger((uint64)f, signed, verb);
        break;
    }
    case int16 f: {
        Ꮡp.fmtInteger((uint64)f, signed, verb);
        break;
    }
    case int32 f: {
        Ꮡp.fmtInteger((uint64)f, signed, verb);
        break;
    }
    case int64 f: {
        Ꮡp.fmtInteger((uint64)f, signed, verb);
        break;
    }
    case nuint f: {
        Ꮡp.fmtInteger((uint64)f, unsigned, verb);
        break;
    }
    case uint8 f: {
        Ꮡp.fmtInteger((uint64)f, unsigned, verb);
        break;
    }
    case uint16 f: {
        Ꮡp.fmtInteger((uint64)f, unsigned, verb);
        break;
    }
    case uint32 f: {
        Ꮡp.fmtInteger((uint64)f, unsigned, verb);
        break;
    }
    case uint64 f: {
        Ꮡp.fmtInteger(f, unsigned, verb);
        break;
    }
    case uintptr f: {
        Ꮡp.fmtInteger((uint64)f, unsigned, verb);
        break;
    }
    case @string f: {
        Ꮡp.fmtString(f, verb);
        break;
    }
    case slice<byte> f: {
        Ꮡp.fmtBytes(f, verb, byteˢ);
        break;
    }
    case reflectꓸValue f: {
        if (f.IsValid() && f.CanInterface()) {
            // Handle extractable values with special methods
            // since printValue does not handle them at depth 0.
            p.arg = f.Interface();
            if (Ꮡp.handleMethods(verb)) {
                return;
            }
        }
        Ꮡp.printValue(f, verb, 0);
        break;
    }
    default: {
        var f = arg;
        if (!Ꮡp.handleMethods(verb)) {
            // If the type is not simple, it might have methods.
            // Need to use reflection, since the type had no
            // interface methods that could be used for formatting.
            Ꮡp.printValue(reflect.ValueOf(f), verb, 0);
        }
        break;
    }}
}

// printValue is similar to printArg but starts with a reflect value, not an interface{} value.
// It does not handle 'p' and 'T' verbs because these should have been already handled by printArg.
internal static void printValue(this ж<pp> Ꮡp, reflectꓸValue value, rune verb, nint depth) {
    ref var p = ref Ꮡp.DerefOrNull();

    // Handle values with special methods if not already handled by printArg (depth == 0).
    if (depth > 0 && value.IsValid() && value.CanInterface()) {
        p.arg = value.Interface();
        if (Ꮡp.handleMethods(verb)) {
            return;
        }
    }
    p.arg = default!;
    p.value = value;
    {
        var f = value;
        var exprᴛ1 = value.Kind();
        var matchᴛ1 = false;
        if (exprᴛ1 == reflect.Invalid) { matchᴛ1 = true;
            if (depth == 0){
                p.buf.writeString(invReflectString);
            } else {
                switch (verb) {
                case (rune)'v': {
                    p.buf.writeString(nilAngleString);
                    break;
                }
                default: {
                    Ꮡp.badVerb(verb);
                    break;
                }}

            }
        }
        else if (exprᴛ1 == reflect.ΔBool) { matchᴛ1 = true;
            Ꮡp.fmtBool(f.Bool(), verb);
        }
        else if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) { matchᴛ1 = true;
            Ꮡp.fmtInteger((uint64)f.Int(), signed, verb);
        }
        else if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) { matchᴛ1 = true;
            Ꮡp.fmtInteger(f.Uint(), unsigned, verb);
        }
        else if (exprᴛ1 == reflect.Float32) { matchᴛ1 = true;
            Ꮡp.fmtFloat(f.Float(), 32, verb);
        }
        else if (exprᴛ1 == reflect.Float64) { matchᴛ1 = true;
            Ꮡp.fmtFloat(f.Float(), 64, verb);
        }
        else if (exprᴛ1 == reflect.Complex64) { matchᴛ1 = true;
            Ꮡp.fmtComplex(f.Complex(), 64, verb);
        }
        else if (exprᴛ1 == reflect.Complex128) { matchᴛ1 = true;
            Ꮡp.fmtComplex(f.Complex(), 128, verb);
        }
        else if (exprᴛ1 == reflect.ΔString) { matchᴛ1 = true;
            Ꮡp.fmtString(f.String(), verb);
        }
        else if (exprᴛ1 == reflect.Map) { matchᴛ1 = true;
            if (p.fmt.sharpV){
                p.buf.writeString(f.Type().String());
                if (f.IsNil()) {
                    p.buf.writeString(nilParenString);
                    return;
                }
                p.buf.writeByte((rune)'{');
            } else {
                p.buf.writeString(mapString);
            }
            var sorted = fmtsort.Sort(f);
            foreach (var (i, m) in sorted) {
                if (i > 0) {
                    if (p.fmt.sharpV){
                        p.buf.writeString(commaSpaceString);
                    } else {
                        p.buf.writeByte((rune)' ');
                    }
                }
                Ꮡp.printValue(m.Key, verb, depth + 1);
                p.buf.writeByte((rune)':');
                Ꮡp.printValue(m.Value, verb, depth + 1);
            }
            if (p.fmt.sharpV){
                p.buf.writeByte((rune)'}');
            } else {
                p.buf.writeByte((rune)']');
            }
        }
        else if (exprᴛ1 == reflect.Struct) { matchᴛ1 = true;
            if (p.fmt.sharpV) {
                p.buf.writeString(f.Type().String());
            }
            p.buf.writeByte((rune)'{');
            for (nint i = 0; i < f.NumField(); i++) {
                if (i > 0) {
                    if (p.fmt.sharpV){
                        p.buf.writeString(commaSpaceString);
                    } else {
                        p.buf.writeByte((rune)' ');
                    }
                }
                if (p.fmt.plusV || p.fmt.sharpV) {
                    {
                        @string name = f.Type().Field(i).Name; if (name != ""u8) {
                            p.buf.writeString(name);
                            p.buf.writeByte((rune)':');
                        }
                    }
                }
                Ꮡp.printValue(getField(f, i), verb, depth + 1);
            }
            p.buf.writeByte((rune)'}');
        }
        else if (exprᴛ1 == reflect.ΔInterface) { matchᴛ1 = true;
            var valueΔ2 = f.Elem();
            if (!valueΔ2.IsValid()){
                if (p.fmt.sharpV){
                    p.buf.writeString(f.Type().String());
                    p.buf.writeString(nilParenString);
                } else {
                    p.buf.writeString(nilAngleString);
                }
            } else {
                Ꮡp.printValue(valueΔ2, verb, depth + 1);
            }
        }
        else if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.ΔSlice) { matchᴛ1 = true;
            switch (verb) {
            case (rune)'s' or (rune)'q' or (rune)'x' or (rune)'X': {
                var t = f.Type();
                if (t.Elem().Kind() == reflect.Uint8) {
                    // Handle byte and uint8 slices and arrays special for the above verbs.
                    slice<byte> bytes = default!;
                    if (f.Kind() == reflect.ΔSlice || f.CanAddr()){
                        bytes = f.Bytes();
                    } else {
                        // We have an array, but we cannot Bytes() a non-addressable array,
                        // so we build a slice by hand. This is a rare case but it would be nice
                        // if reflection could help a little more.
                        bytes = new slice<byte>(f.Len());
                        foreach (var (i, _) in bytes) {
                            bytes[i] = (byte)f.Index(i).Uint();
                        }
                    }
                    Ꮡp.fmtBytes(bytes, verb, t.String());
                    return;
                }
                break;
            }}

            if (p.fmt.sharpV){
                p.buf.writeString(f.Type().String());
                if (f.Kind() == reflect.ΔSlice && f.IsNil()) {
                    p.buf.writeString(nilParenString);
                    return;
                }
                p.buf.writeByte((rune)'{');
                for (nint i = 0; i < f.Len(); i++) {
                    if (i > 0) {
                        p.buf.writeString(commaSpaceString);
                    }
                    Ꮡp.printValue(f.Index(i), verb, depth + 1);
                }
                p.buf.writeByte((rune)'}');
            } else {
                p.buf.writeByte((rune)'[');
                for (nint i = 0; i < f.Len(); i++) {
                    if (i > 0) {
                        p.buf.writeByte((rune)' ');
                    }
                    Ꮡp.printValue(f.Index(i), verb, depth + 1);
                }
                p.buf.writeByte((rune)']');
            }
        }
        else if (exprᴛ1 == reflect.ΔPointer) { matchᴛ1 = true;
            if (depth == 0 && (uintptr)f.UnsafePointer() != nil) {
                // pointer to array or slice or struct? ok at top level
                // but not embedded (avoid loops)
                {
                    var a = f.Elem();
                    var exprᴛ2 = a.Kind();
                    if (exprᴛ2 == reflect.Array || exprᴛ2 == reflect.ΔSlice || exprᴛ2 == reflect.Struct || exprᴛ2 == reflect.Map) {
                        p.buf.writeByte((rune)'&');
                        Ꮡp.printValue(a, verb, depth + 1);
                        return;
                    }
                }

            }
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && (exprᴛ1 == reflect.Chan || exprᴛ1 == reflect.Func || exprᴛ1 == reflect.ΔUnsafePointer)) {
            Ꮡp.fmtPointer(f, verb);
        }
        else if (!matchᴛ1) { /* default: */
            p.unknownType(f);
        }
    }

}

// intFromArg gets the argNumth element of a. On return, isInt reports whether the argument has integer type.
internal static (nint num, bool isInt, nint newArgNum) intFromArg(slice<any> a, nint argNum) {
    nint num = default!;
    bool isInt = default!;
    nint newArgNum = default!;

    newArgNum = argNum;
    if (argNum < len(a)) {
        (num, isInt) = a[argNum]._<nint>(ᐧ); // Almost always OK.
        if (!isInt) {
            // Work harder.
            {
                var v = reflect.ValueOf(a[argNum]);
                var exprᴛ1 = v.Kind();
                if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
                    var n = v.Int();
                    if ((int64)(nint)n == n) {
                        num = (nint)n;
                        isInt = true;
                    }
                }
                else if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
                    var n = v.Uint();
                    if ((int64)n >= 0 && (uint64)(nint)n == n) {
                        num = (nint)n;
                        isInt = true;
                    }
                }
                else { /* default: */
                }
            }

        }
        // Already 0, false.
        newArgNum = argNum + 1;
        if (tooLarge(num)) {
            num = 0;
            isInt = false;
        }
    }
    return (num, isInt, newArgNum);
}

// parseArgNumber returns the value of the bracketed number, minus 1
// (explicit argument numbers are one-indexed but we want zero-indexed).
// The opening bracket is known to be present at format[0].
// The returned values are the index, the number of bytes to consume
// up to the closing paren, if present, and whether the number parsed
// ok. The bytes to consume will be 1 if no closing paren is present.
internal static (nint index, nint wid, bool ok) parseArgNumber(sstring format) {
    // There must be at least 3 bytes: [n].
    if (len(format) < 3) {
        return (0, 1, false);
    }
    // Find closing bracket.
    for (nint i = 1; i < len(format); i++) {
        if (format[i] == (rune)']') {
            var (width, okΔ1, newi) = parsenum(format, 1, i);
            if (!okΔ1 || newi != i) {
                return (0, i + 1, false);
            }
            return (width - 1, i + 1, true); // arg numbers are one-indexed and skip paren.
        }
    }
    return (0, 1, false);
}

// argNumber returns the next argument to evaluate, which is either the value of the passed-in
// argNum or the value of the bracketed integer that begins format[i:]. It also returns
// the new value of i, that is, the index of the next byte of the format to process.
internal static (nint newArgNum, nint newi, bool found) argNumber(this ref pp p, nint argNum, sstring format, nint i, nint numArgs) {
    if (len(format) <= i || format[i] != (rune)'[') {
        return (argNum, i, false);
    }
    p.reordered = true;
    var (index, wid, ok) = parseArgNumber(format.slice(i));
    if (ok && 0 <= index && index < numArgs) {
        return (index, i + wid, true);
    }
    p.goodArgNum = false;
    return (argNum, i + wid, ok);
}

internal static void badArgNum(this ref pp p, rune verb) {
    p.buf.writeString(percentBangString);
    p.buf.writeRune(verb);
    p.buf.writeString(badIndexString);
}

internal static void missingArg(this ref pp p, rune verb) {
    p.buf.writeString(percentBangString);
    p.buf.writeRune(verb);
    p.buf.writeString(missingString);
}

internal static void doPrintf(this ж<pp> Ꮡp, sstring format, slice<any> a) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint end = len(format);
    nint argNum = 0; // we process one argument per non-trivial format
    var afterIndex = false; // previous item in format was an index like [3].
    p.reordered = false;
formatLoop:
    for (nint i = 0; i < end; ) {
        p.goodArgNum = true;
        nint lasti = i;
        while (i < end && format[i] != (rune)'%') {
            i++;
        }
        if (i > lasti) {
            p.buf.writeString(format.slice(lasti, i));
        }
        if (i >= end) {
            // done processing format string
            break;
        }
        // Process one verb
        i++;
        // Do we have flags?
        p.fmt.clearflags();
simpleFormat:
        for (; i < end; i++) {
            var c = format[i];
            switch (c) {
            case (rune)'#': {
                p.fmt.sharp = true;
                break;
            }
            case (rune)'0': {
                p.fmt.zero = true;
                break;
            }
            case (rune)'+': {
                p.fmt.plus = true;
                break;
            }
            case (rune)'-': {
                p.fmt.minus = true;
                break;
            }
            case (rune)' ': {
                p.fmt.space = true;
                break;
            }
            default: {
                if ((rune)'a' <= c && c <= (rune)'z' && argNum < len(a)) {
                    // Fast path for common case of ascii lower case simple verbs
                    // without precision or width or argument indices.
                    var exprᴛ1 = c;
                    var matchᴛ1 = false;
                    if (exprᴛ1 is (rune)'w') {
                        p.wrappedErrs = append(p.wrappedErrs, argNum);
                        fallthrough = true;
                    }
                    if (fallthrough || !matchᴛ1 && exprᴛ1 is (rune)'v') { matchᴛ1 = true;
                        p.fmt.sharpV = p.fmt.sharp;
                        p.fmt.sharp = false;
                        p.fmt.plusV = p.fmt.plus;
                        p.fmt.plus = false;
                    }

                    // Go syntax
                    // Struct-field syntax
                    Ꮡp.printArg(a[argNum], (rune)c);
                    argNum++;
                    i++;
                    goto continue_formatLoop;
                }
                goto break_simpleFormat;
                break;
            }}

continue_simpleFormat:;
        }
break_simpleFormat:;
        // Format is more complex than simple flags and a verb or is malformed.
        // Do we have an explicit argument index?
        (argNum, i, afterIndex) = p.argNumber(argNum, format, i, len(a));
        // Do we have width?
        if (i < end && format[i] == (rune)'*'){
            i++;
            (p.fmt.wid, p.fmt.widPresent, argNum) = intFromArg(a, argNum);
            if (!p.fmt.widPresent) {
                p.buf.writeString(badWidthString);
            }
            // We have a negative width, so take its value and ensure
            // that the minus flag is set
            if (p.fmt.wid < 0) {
                p.fmt.wid = -p.fmt.wid;
                p.fmt.minus = true;
                p.fmt.zero = false; // Do not pad with zeros to the right.
            }
            afterIndex = false;
        } else {
            (p.fmt.wid, p.fmt.widPresent, i) = parsenum(format, i, end);
            if (afterIndex && p.fmt.widPresent) {
                // "%[3]2d"
                p.goodArgNum = false;
            }
        }
        // Do we have precision?
        if (i + 1 < end && format[i] == (rune)'.') {
            i++;
            if (afterIndex) {
                // "%[3].2d"
                p.goodArgNum = false;
            }
            (argNum, i, afterIndex) = p.argNumber(argNum, format, i, len(a));
            if (i < end && format[i] == (rune)'*'){
                i++;
                (p.fmt.prec, p.fmt.precPresent, argNum) = intFromArg(a, argNum);
                // Negative precision arguments don't make sense
                if (p.fmt.prec < 0) {
                    p.fmt.prec = 0;
                    p.fmt.precPresent = false;
                }
                if (!p.fmt.precPresent) {
                    p.buf.writeString(badPrecString);
                }
                afterIndex = false;
            } else {
                (p.fmt.prec, p.fmt.precPresent, i) = parsenum(format, i, end);
                if (!p.fmt.precPresent) {
                    p.fmt.prec = 0;
                    p.fmt.precPresent = true;
                }
            }
        }
        if (!afterIndex) {
            (argNum, i, afterIndex) = p.argNumber(argNum, format, i, len(a));
        }
        if (i >= end) {
            p.buf.writeString(noVerbString);
            break;
        }
        var verb = (rune)format[i];
        nint size = 1;
        if (verb >= utf8.RuneSelf) {
            (verb, size) = utf8.DecodeRuneInString(format.slice(i));
        }
        i += size;
        var matchᴛ2 = false;
        if (verb is (rune)'%') { matchᴛ2 = true;
            p.buf.writeByte((rune)'%');
        }
        else if (!p.goodArgNum) { matchᴛ2 = true;
            p.badArgNum(verb);
        }
        else if (argNum >= len(a)) { matchᴛ2 = true;
            p.missingArg(verb);
        }
        else if (verb is (rune)'w') { matchᴛ2 = true;
            p.wrappedErrs = append(p.wrappedErrs, // Percent does not absorb operands and ignores f.wid and f.prec.
 // No argument left over to print for the current verb.
 argNum);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2 && verb is (rune)'v') { matchᴛ2 = true;
            p.fmt.sharpV = p.fmt.sharp;
            p.fmt.sharp = false;
            p.fmt.plusV = p.fmt.plus;
            p.fmt.plus = false;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2) { /* default: */
            Ꮡp.printArg(a[argNum], // Go syntax
 // Struct-field syntax
 verb);
            argNum++;
        }

continue_formatLoop:;
    }
break_formatLoop:;
    // Check for extra arguments unless the call accessed the arguments
    // out of order, in which case it's too expensive to detect if they've all
    // been used and arguably OK if they're not.
    if (!p.reordered && argNum < len(a)) {
        p.fmt.clearflags();
        p.buf.writeString(extraString);
        foreach (var (i, arg) in a.slice(argNum)) {
            if (i > 0) {
                p.buf.writeString(commaSpaceString);
            }
            if (arg == default!){
                p.buf.writeString(nilAngleString);
            } else {
                p.buf.writeString(reflect.TypeOf(arg).String());
                p.buf.writeByte((rune)'=');
                Ꮡp.printArg(arg, (rune)'v');
            }
        }
        p.buf.writeByte((rune)')');
    }
}

internal static void doPrint(this ж<pp> Ꮡp, slice<any> a) {
    ref var p = ref Ꮡp.DerefOrNull();

    var prevString = false;
    foreach (var (argNum, arg) in a) {
        var isString = arg != default! && reflect.TypeOf(arg).Kind() == reflect.ΔString;
        // Add a space between two non-string arguments.
        if (argNum > 0 && !isString && !prevString) {
            p.buf.writeByte((rune)' ');
        }
        Ꮡp.printArg(arg, (rune)'v');
        prevString = isString;
    }
}

// doPrintln is like doPrint but always adds a space between arguments
// and a newline after the last argument.
internal static void doPrintln(this ж<pp> Ꮡp, slice<any> a) {
    ref var p = ref Ꮡp.DerefOrNull();

    foreach (var (argNum, arg) in a) {
        if (argNum > 0) {
            p.buf.writeByte((rune)' ');
        }
        Ꮡp.printArg(arg, (rune)'v');
    }
    p.buf.writeByte((rune)'\n');
}

} // end fmt_package

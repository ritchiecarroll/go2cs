// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;

partial class json_package {

// HTMLEscape appends to dst the JSON-encoded src with <, >, &, U+2028 and U+2029
// characters inside string literals changed to \u003c, \u003e, \u0026, \u2028, \u2029
// so that the JSON will be safe to embed inside HTML <script> tags.
// For historical reasons, web browsers don't honor standard HTML
// escaping within <script> tags, so an alternative JSON encoding must be used.
public static void HTMLEscape(ж<bytes.Buffer> Ꮡdst, slice<byte> src) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    dst.Grow(len(src));
    dst.Write(appendHTMLEscape(dst.AvailableBuffer(), src));
}

internal static slice<byte> appendHTMLEscape(slice<byte> dst, slice<byte> src) {
    // The characters can only appear in string literals,
    // so just scan the string one byte at a time.
    nint start = 0;
    foreach (var (i, c) in src) {
        if (c == (rune)'<' || c == (rune)'>' || c == (rune)'&') {
            dst = appendꓸꓸꓸ(dst, src.slice(start, i));
            dst = append(dst, (byte)((rune)'\\'), (byte)((rune)'u'), (byte)((rune)'0'), (byte)((rune)'0'), hex[(c >> (int)(4))], hex[(byte)(c & 0xF)]);
            start = i + 1;
        }
        // Convert U+2028 and U+2029 (E2 80 A8 and E2 80 A9).
        if (c == 0xE2 && i + 2 < len(src) && src[i + 1] == 0x80 && (byte)(src[i + 2] & ~1) == 0xA8) {
            dst = appendꓸꓸꓸ(dst, src.slice(start, i));
            dst = append(dst, (byte)((rune)'\\'), (byte)((rune)'u'), (byte)((rune)'2'), (byte)((rune)'0'), (byte)((rune)'2'), hex[(byte)(src[i + 2] & 0xF)]);
            start = i + len("\u2029");
        }
    }
    return appendꓸꓸꓸ(dst, src.slice(start));
}

// Compact appends to dst the JSON-encoded src with
// insignificant space characters elided.
public static error Compact(ж<bytes.Buffer> Ꮡdst, slice<byte> src) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    dst.Grow(len(src));
    var b = dst.AvailableBuffer();
    (b, var err) = appendCompact(b, src, false);
    dst.Write(b);
    return err;
}

internal static (slice<byte>, error) appendCompact(slice<byte> dst, slice<byte> src, bool escape) {
    GoFrame ᒐ = default;
    try {
        nint origLen = len(dst);
        var scan = newScanner();
        defer(freeScanner, scan, ref ᒐ);
        nint start = 0;
        foreach (var (i, c) in src) {
            if (escape && (c == (rune)'<' || c == (rune)'>' || c == (rune)'&')) {
                if (start < i) {
                    dst = appendꓸꓸꓸ(dst, src.slice(start, i));
                }
                dst = append(dst, (byte)((rune)'\\'), (byte)((rune)'u'), (byte)((rune)'0'), (byte)((rune)'0'), hex[(c >> (int)(4))], hex[(byte)(c & 0xF)]);
                start = i + 1;
            }
            // Convert U+2028 and U+2029 (E2 80 A8 and E2 80 A9).
            if (escape && c == 0xE2 && i + 2 < len(src) && src[i + 1] == 0x80 && (byte)(src[i + 2] & ~1) == 0xA8) {
                if (start < i) {
                    dst = appendꓸꓸꓸ(dst, src.slice(start, i));
                }
                dst = append(dst, (byte)((rune)'\\'), (byte)((rune)'u'), (byte)((rune)'2'), (byte)((rune)'0'), (byte)((rune)'2'), hex[(byte)(src[i + 2] & 0xF)]);
                start = i + 3;
            }
            nint v = (~scan).step(scan, c);
            if (v >= scanSkipSpace) {
                if (v == scanError) {
                    break;
                }
                if (start < i) {
                    dst = appendꓸꓸꓸ(dst, src.slice(start, i));
                }
                start = i + 1;
            }
        }
        if (scan.eof() == scanError) {
            return (dst.slice(0, origLen), (~scan).err);
        }
        if (start < len(src)) {
            dst = appendꓸꓸꓸ(dst, src.slice(start));
        }
        return (dst, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static slice<byte> appendNewline(slice<byte> dst, @string prefix, @string indent, nint depth) {
    dst = append(dst, (byte)((rune)'\n'));
    dst = append(dst, prefix.ꓸꓸꓸ);
    for (nint i = 0; i < depth; i++) {
        dst = append(dst, indent.ꓸꓸꓸ);
    }
    return dst;
}

// indentGrowthFactor specifies the growth factor of indenting JSON input.
// Empirically, the growth factor was measured to be between 1.4x to 1.8x
// for some set of compacted JSON with the indent being a single tab.
// Specify a growth factor slightly larger than what is observed
// to reduce probability of allocation in appendIndent.
// A factor no higher than 2 ensures that wasted space never exceeds 50%.
internal static UntypedInt indentGrowthFactor => 2;

// Indent appends to dst an indented form of the JSON-encoded src.
// Each element in a JSON object or array begins on a new,
// indented line beginning with prefix followed by one or more
// copies of indent according to the indentation nesting.
// The data appended to dst does not begin with the prefix nor
// any indentation, to make it easier to embed inside other formatted JSON data.
// Although leading space characters (space, tab, carriage return, newline)
// at the beginning of src are dropped, trailing space characters
// at the end of src are preserved and copied to dst.
// For example, if src has no trailing spaces, neither will dst;
// if src ends in a trailing newline, so will dst.
public static error Indent(ж<bytes.Buffer> Ꮡdst, slice<byte> src, @string prefix, @string indent) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    dst.Grow((nint)indentGrowthFactor * len(src));
    var b = dst.AvailableBuffer();
    (b, var err) = appendIndent(b, src, prefix, indent);
    dst.Write(b);
    return err;
}

internal static (slice<byte>, error) appendIndent(slice<byte> dst, slice<byte> src, @string prefix, @string indent) {
    GoFrame ᒐ = default;
    try {
        nint origLen = len(dst);
        var scan = newScanner();
        defer(freeScanner, scan, ref ᒐ);
        var needIndent = false;
        nint depth = 0;
        foreach (var (_, c) in src) {
            scan.Value.bytes++;
            nint v = (~scan).step(scan, c);
            if (v == scanSkipSpace) {
                continue;
            }
            if (v == scanError) {
                break;
            }
            if (needIndent && v != scanEndObject && v != scanEndArray) {
                needIndent = false;
                depth++;
                dst = appendNewline(dst, prefix, indent, depth);
            }
            // Emit semantically uninteresting bytes
            // (in particular, punctuation in strings) unmodified.
            if (v == scanContinue) {
                dst = append(dst, c);
                continue;
            }
            // Add spacing around real punctuation.
            switch (c) {
            case (rune)'{' or (rune)'[': {
                needIndent = true;
                dst = append(dst, // delay indent so that empty object and array are formatted as {} and [].
 c);
                break;
            }
            case (rune)',': {
                dst = append(dst, c);
                dst = appendNewline(dst, prefix, indent, depth);
                break;
            }
            case (rune)':': {
                dst = append(dst, c, (byte)((rune)' '));
                break;
            }
            case (rune)'}' or (rune)']': {
                if (needIndent){
                    // suppress indent in empty object/array
                    needIndent = false;
                } else {
                    depth--;
                    dst = appendNewline(dst, prefix, indent, depth);
                }
                dst = append(dst, c);
                break;
            }
            default: {
                dst = append(dst, c);
                break;
            }}

        }
        if (scan.eof() == scanError) {
            return (dst.slice(0, origLen), (~scan).err);
        }
        return (dst, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end json_package

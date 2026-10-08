// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bufio = bufio_package;
using io = io_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;

partial class csv_package {

// A Writer writes records using CSV encoding.
//
// As returned by [NewWriter], a Writer writes records terminated by a
// newline and uses ',' as the field delimiter. The exported fields can be
// changed to customize the details before
// the first call to [Writer.Write] or [Writer.WriteAll].
//
// [Writer.Comma] is the field delimiter.
//
// If [Writer.UseCRLF] is true,
// the Writer ends each output line with \r\n instead of \n.
//
// The writes of individual records are buffered.
// After all data has been written, the client should call the
// [Writer.Flush] method to guarantee all data has been forwarded to
// the underlying [io.Writer].  Any errors that occurred should
// be checked by calling the [Writer.Error] method.
partial struct Writer {
    public rune Comma; // Field delimiter (set to ',' by NewWriter)
    public bool UseCRLF; // True to use \r\n as the line terminator
    internal ж<bufio.Writer> w;
}

// NewWriter returns a new Writer that writes to w.
public static ж<Writer> NewWriter(io.Writer w) {
    return Ꮡ(new Writer(
        Comma: (rune)',',
        w: bufio.NewWriter(w)
    ));
}

// Write writes a single CSV record to w along with any necessary quoting.
// A record is a slice of strings with each string being one field.
// Writes are buffered, so [Writer.Flush] must eventually be called to ensure
// that the record is written to the underlying [io.Writer].
public static error Write(this ref Writer w, slice<@string> record) {
    if (!validDelim(w.Comma)) {
        return errInvalidDelim;
    }
    foreach (var (n, vᴛ1) in record) {
        var field = vᴛ1;

        if (n > 0) {
            {
                var (_, errΔ1) = w.w.WriteRune(w.Comma); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
        }
        // If we don't have to have a quoted field then just
        // write out the field and continue to the next field.
        if (!w.fieldNeedsQuotes(field)) {
            {
                var (_, errΔ2) = w.w.WriteString(field); if (errΔ2 != default!) {
                    return errΔ2;
                }
            }
            continue;
        }
        {
            var errΔ3 = w.w.WriteByte((rune)'"'); if (errΔ3 != default!) {
                return errΔ3;
            }
        }
        while (len(field) > 0) {
            // Search for special characters.
            nint i = strings.IndexAny(field, "\"\r\n"u8);
            if (i < 0) {
                i = len(field);
            }
            // Copy verbatim everything before the special character.
            {
                var (_, errΔ4) = w.w.WriteString(field.slice(0, i)); if (errΔ4 != default!) {
                    return errΔ4;
                }
            }
            field = field.slice(i);
            // Encode the special character.
            if (len(field) > 0) {
                error errΔ5 = default!;
                switch (field[0]) {
                case (rune)'"': {
                    (_, errΔ5) = w.w.WriteString(@""""""u8);
                    break;
                }
                case (rune)'\r': {
                    if (!w.UseCRLF) {
                        errΔ5 = w.w.WriteByte((rune)'\r');
                    }
                    break;
                }
                case (rune)'\n': {
                    if (w.UseCRLF){
                        (_, errΔ5) = w.w.WriteString("\r\n"u8);
                    } else {
                        errΔ5 = w.w.WriteByte((rune)'\n');
                    }
                    break;
                }}

                field = field[1..];
                if (errΔ5 != default!) {
                    return errΔ5;
                }
            }
        }
        {
            var errΔ6 = w.w.WriteByte((rune)'"'); if (errΔ6 != default!) {
                return errΔ6;
            }
        }
    }
    error err = default!;
    if (w.UseCRLF){
        (_, err) = w.w.WriteString("\r\n"u8);
    } else {
        err = w.w.WriteByte((rune)'\n');
    }
    return err;
}

// Flush writes any buffered data to the underlying [io.Writer].
// To check if an error occurred during Flush, call [Writer.Error].
public static void Flush(this ref Writer w) {
    w.w.Flush();
}

// Error reports any error that has occurred during
// a previous [Writer.Write] or [Writer.Flush].
public static error Error(this ref Writer w) {
    var (_, err) = w.w.Write(default!);
    return err;
}

// WriteAll writes multiple CSV records to w using [Writer.Write] and
// then calls [Writer.Flush], returning any error from the Flush.
public static error WriteAll(this ref Writer w, slice<slice<@string>> records) {
    foreach (var (_, record) in records) {
        var err = w.Write(record);
        if (err != default!) {
            return err;
        }
    }
    return w.w.Flush();
}

// fieldNeedsQuotes reports whether our field must be enclosed in quotes.
// Fields with a Comma, fields with a quote or newline, and
// fields which start with a space must be enclosed in quotes.
// We used to quote empty strings, but we do not anymore (as of Go 1.4).
// The two representations should be equivalent, but Postgres distinguishes
// quoted vs non-quoted empty string during database imports, and it has
// an option to force the quoted behavior for non-quoted CSV but it has
// no option to force the non-quoted behavior for quoted CSV, making
// CSV with quoted empty strings strictly less useful.
// Not quoting the empty string also makes this package match the behavior
// of Microsoft Excel and Google Drive.
// For Postgres, quote the data terminating string `\.`.
internal static bool fieldNeedsQuotes(this ref Writer w, @string field) {
    if (field == ""u8) {
        return false;
    }
    if (field == @"\."u8) {
        return true;
    }
    if (w.Comma < utf8.RuneSelf){
        for (nint i = 0; i < len(field); i++) {
            var c = field[i];
            if (c == (rune)'\n' || c == (rune)'\r' || c == (rune)'"' || c == (byte)w.Comma) {
                return true;
            }
        }
    } else {
        if (strings.ContainsRune(field, w.Comma) || strings.ContainsAny(field, "\"\r\n"u8)) {
            return true;
        }
    }
    var (r1, _) = utf8.DecodeRuneInString(field);
    return unicode.IsSpace(r1);
}

} // end csv_package

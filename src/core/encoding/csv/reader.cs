// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package csv reads and writes comma-separated values (CSV) files.
// There are many kinds of CSV files; this package supports the format
// described in RFC 4180, except that [Writer] uses LF
// instead of CRLF as newline character by default.
//
// A csv file contains zero or more records of one or more fields per record.
// Each record is separated by the newline character. The final record may
// optionally be followed by a newline character.
//
//	field1,field2,field3
//
// White space is considered part of a field.
//
// Carriage returns before newline characters are silently removed.
//
// Blank lines are ignored. A line with only whitespace characters (excluding
// the ending newline character) is not considered a blank line.
//
// Fields which start and stop with the quote character " are called
// quoted-fields. The beginning and ending quote are not part of the
// field.
//
// The source:
//
//	normal string,"quoted-field"
//
// results in the fields
//
//	{`normal string`, `quoted-field`}
//
// Within a quoted-field a quote character followed by a second quote
// character is considered a single quote.
//
//	"the ""word"" is true","a ""quoted-field"""
//
// results in
//
//	{`the "word" is true`, `a "quoted-field"`}
//
// Newlines and commas may be included in a quoted-field
//
//	"Multi-line
//	field","comma is ,"
//
// results in
//
//	{`Multi-line
//	field`, `comma is ,`}
namespace go.encoding;

using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;

partial class csv_package {

// A ParseError is returned for parsing errors.
// Line and column numbers are 1-indexed.
partial struct ParseError {
    public nint StartLine;  // Line where the record starts
    public nint Line;  // Line where the error occurred
    public nint Column;  // Column (1-based byte index) where the error occurred
    public error Err; // The actual error
}

public static @string Error(this ref ParseError e) {
    if (AreEqual(e.Err, ErrFieldCount)) {
        return fmt.Sprintf("record on line %d: %v"u8, e.Line, e.Err);
    }
    if (e.StartLine != e.Line) {
        return fmt.Sprintf("record on line %d; parse error on line %d, column %d: %v"u8, e.StartLine, e.Line, e.Column, e.Err);
    }
    return fmt.Sprintf("parse error on line %d, column %d: %v"u8, e.Line, e.Column, e.Err);
}

public static error Unwrap(this ref ParseError e) {
    return e.Err;
}

// These are the errors that can be returned in [ParseError.Err].
public static error ErrBareQuote = errors.New("bare \" in non-quoted-field"u8);

public static error ErrQuote = errors.New("extraneous or missing \" in quoted-field"u8);

public static error ErrFieldCount = errors.New("wrong number of fields"u8);

public static error ErrTrailingComma = errors.New("extra delimiter at end of line"u8);

internal static error errInvalidDelim = errors.New("csv: invalid field or comment delimiter"u8);

internal static bool validDelim(rune r) {
    return r != 0 && r != (rune)'"' && r != (rune)'\r' && r != (rune)'\n' && utf8.ValidRune(r) && r != utf8.RuneError;
}

// A Reader reads records from a CSV-encoded file.
//
// As returned by [NewReader], a Reader expects input conforming to RFC 4180.
// The exported fields can be changed to customize the details before the
// first call to [Reader.Read] or [Reader.ReadAll].
//
// The Reader converts all \r\n sequences in its input to plain \n,
// including in multiline field values, so that the returned data does
// not depend on which line-ending convention an input file uses.
partial struct Reader {
    // Comma is the field delimiter.
    // It is set to comma (',') by NewReader.
    // Comma must be a valid rune and must not be \r, \n,
    // or the Unicode replacement character (0xFFFD).
    public rune Comma;
    // Comment, if not 0, is the comment character. Lines beginning with the
    // Comment character without preceding whitespace are ignored.
    // With leading whitespace the Comment character becomes part of the
    // field, even if TrimLeadingSpace is true.
    // Comment must be a valid rune and must not be \r, \n,
    // or the Unicode replacement character (0xFFFD).
    // It must also not be equal to Comma.
    public rune Comment;
    // FieldsPerRecord is the number of expected fields per record.
    // If FieldsPerRecord is positive, Read requires each record to
    // have the given number of fields. If FieldsPerRecord is 0, Read sets it to
    // the number of fields in the first record, so that future records must
    // have the same field count. If FieldsPerRecord is negative, no check is
    // made and records may have a variable number of fields.
    public nint FieldsPerRecord;
    // If LazyQuotes is true, a quote may appear in an unquoted field and a
    // non-doubled quote may appear in a quoted field.
    public bool LazyQuotes;
    // If TrimLeadingSpace is true, leading white space in a field is ignored.
    // This is done even if the field delimiter, Comma, is white space.
    public bool TrimLeadingSpace;
    // ReuseRecord controls whether calls to Read may return a slice sharing
    // the backing array of the previous call's returned slice for performance.
    // By default, each call to Read returns newly allocated memory owned by the caller.
    public bool ReuseRecord;
    // Deprecated: TrailingComma is no longer used.
    public bool TrailingComma;
    internal ж<bufio.Reader> r;
    // numLine is the current line being read in the CSV file.
    internal nint numLine;
    // offset is the input stream byte offset of the current reader position.
    internal int64 offset;
    // rawBuffer is a line buffer only used by the readLine method.
    internal slice<byte> rawBuffer;
    // recordBuffer holds the unescaped fields, one after another.
    // The fields can be accessed by using the indexes in fieldIndexes.
    // E.g., For the row `a,"b","c""d",e`, recordBuffer will contain `abc"de`
    // and fieldIndexes will contain the indexes [1, 2, 5, 6].
    internal slice<byte> recordBuffer;
    // fieldIndexes is an index of fields inside recordBuffer.
    // The i'th field ends at offset fieldIndexes[i] in recordBuffer.
    internal slice<nint> fieldIndexes;
    // fieldPositions is an index of field positions for the
    // last record returned by Read.
    internal slice<position> fieldPositions;
    // lastRecord is a record cache and only used when ReuseRecord == true.
    internal slice<@string> lastRecord;
}

// NewReader returns a new Reader that reads from r.
public static ж<Reader> NewReader(io.Reader r) {
    return Ꮡ(new Reader(
        Comma: (rune)',',
        r: bufio.NewReader(r)
    ));
}

// Read reads one record (a slice of fields) from r.
// If the record has an unexpected number of fields,
// Read returns the record along with the error [ErrFieldCount].
// If the record contains a field that cannot be parsed,
// Read returns a partial record along with the parse error.
// The partial record contains all fields read before the error.
// If there is no data left to be read, Read returns nil, [io.EOF].
// If [Reader.ReuseRecord] is true, the returned slice may be shared
// between multiple calls to Read.
public static (slice<@string> record, error err) Read(this ref Reader r) {
    slice<@string> record = default!;
    error err = default!;

    if (r.ReuseRecord){
        (record, err) = r.readRecord(r.lastRecord);
        r.lastRecord = record;
    } else {
        (record, err) = r.readRecord(default!);
    }
    return (record, err);
}

// FieldPos returns the line and column corresponding to
// the start of the field with the given index in the slice most recently
// returned by [Reader.Read]. Numbering of lines and columns starts at 1;
// columns are counted in bytes, not runes.
//
// If this is called with an out-of-bounds index, it panics.
public static (nint line, nint column) FieldPos(this ref Reader r, nint field) {
    if (field < 0 || field >= len(r.fieldPositions)) {
        throw panic("out of range index passed to FieldPos");
    }
    var p = Ꮡ(r.fieldPositions, field);
    return ((~p).line, (~p).col);
}

// InputOffset returns the input stream byte offset of the current reader
// position. The offset gives the location of the end of the most recently
// read row and the beginning of the next row.
public static int64 InputOffset(this ref Reader r) {
    return r.offset;
}

// pos holds the position of a field in the current line.
partial struct position {
    internal nint line, col;
}

// ReadAll reads all the remaining records from r.
// Each record is a slice of fields.
// A successful call returns err == nil, not err == [io.EOF]. Because ReadAll is
// defined to read until EOF, it does not treat end of file as an error to be
// reported.
public static (slice<slice<@string>> records, error err) ReadAll(this ref Reader r) {
    slice<slice<@string>> records = default!;

    while (ᐧ) {
        var (record, errΔ1) = r.readRecord(default!);
        if (AreEqual(errΔ1, io.EOF)) {
            return (records, default!);
        }
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        records = append(records, record);
    }
}

// readLine reads the next line (with the trailing endline).
// If EOF is hit without a trailing endline, it will be omitted.
// If some bytes were read, then the error is never [io.EOF].
// The result is only valid until the next call to readLine.
internal static (slice<byte>, error) readLine(this ref Reader r) {
    var (line, err) = r.r.ReadSlice((rune)'\n');
    if (AreEqual(err, bufio.ErrBufferFull)) {
        r.rawBuffer = appendꓸꓸꓸ(r.rawBuffer[..0], line);
        while (AreEqual(err, bufio.ErrBufferFull)) {
            (line, err) = r.r.ReadSlice((rune)'\n');
            r.rawBuffer = appendꓸꓸꓸ(r.rawBuffer, line);
        }
        line = r.rawBuffer;
    }
    nint readSize = len(line);
    if (readSize > 0 && AreEqual(err, io.EOF)) {
        err = default!;
        // For backwards compatibility, drop trailing \r before EOF.
        if (line[readSize - 1] == (rune)'\r') {
            line = line.slice(0, readSize - 1);
        }
    }
    r.numLine++;
    r.offset += (int64)readSize;
    // Normalize \r\n to \n on all input lines.
    {
        nint n = len(line); if (n >= 2 && line[n - 2] == (rune)'\r' && line[n - 1] == (rune)'\n') {
            line[n - 2] = (rune)'\n';
            line = line.slice(0, n - 1);
        }
    }
    return (line, err);
}

// lengthNL reports the number of bytes for the trailing \n.
internal static nint lengthNL(slice<byte> b) {
    if (len(b) > 0 && b[len(b) - 1] == (rune)'\n') {
        return 1;
    }
    return 0;
}

// nextRune returns the next rune in b or utf8.RuneError.
internal static rune nextRune(slice<byte> b) {
    var (r, _) = utf8.DecodeRune(b);
    return r;
}

internal static (slice<@string>, error) readRecord(this ref Reader r, slice<@string> dst) {
    if (r.Comma == r.Comment || !validDelim(r.Comma) || (r.Comment != 0 && !validDelim(r.Comment))) {
        return (default!, errInvalidDelim);
    }
    // Read line (automatically skipping past empty lines and any comments).
    slice<byte> line = default!;
    error errRead = default!;
    while (errRead == default!) {
        (line, errRead) = r.readLine();
        if (r.Comment != 0 && nextRune(line) == r.Comment) {
            line = default!;
            continue; // Skip comment lines
        }
        if (errRead == default! && len(line) == lengthNL(line)) {
            line = default!;
            continue; // Skip empty lines
        }
        break;
    }
    if (AreEqual(errRead, io.EOF)) {
        return (default!, errRead);
    }
    // Parse each field in the record.
    error err = default!;
    const nint quoteLen = /* len(`"`) */ 1;
    nint commaLen = utf8.RuneLen(r.Comma);
    ref var recLine = ref heap<nint>(out var ᏑrecLine);
    recLine = r.numLine; // Starting line for record
    r.recordBuffer = r.recordBuffer[..0];
    r.fieldIndexes = r.fieldIndexes[..0];
    r.fieldPositions = r.fieldPositions[..0];
    var pos = new position(line: r.numLine, col: 1);
parseField:
    while (ᐧ) {
        if (r.TrimLeadingSpace) {
            nint i = bytes.IndexFunc(line, (rune rΔ1) => !unicode.IsSpace(rΔ1));
            if (i < 0) {
                i = len(line);
                pos.col -= lengthNL(line);
            }
            line = line.slice(i);
            pos.col += i;
        }
        if (len(line) == 0 || line[0] != (rune)'"'){
            // Non-quoted string field
            nint i = bytes.IndexRune(line, r.Comma);
            var field = line;
            if (i >= 0){
                field = field.slice(0, i);
            } else {
                field = field.slice(0, len(field) - lengthNL(field));
            }
            // Check to make sure a quote does not appear in field.
            if (!r.LazyQuotes) {
                {
                    nint j = bytes.IndexByte(field, (rune)'"'); if (j >= 0) {
                        ref var col = ref heap<nint>(out var Ꮡcol);
                        col = pos.col + j;
                        err = new ParseErrorжerror(Ꮡ(new ParseError(StartLine: recLine, Line: r.numLine, Column: col, Err: ErrBareQuote)));
                        goto break_parseField;
                    }
                }
            }
            r.recordBuffer = appendꓸꓸꓸ(r.recordBuffer, field);
            r.fieldIndexes = append(r.fieldIndexes, len(r.recordBuffer));
            r.fieldPositions = append(r.fieldPositions, pos);
            if (i >= 0) {
                line = line.slice(i + commaLen);
                pos.col += i + commaLen;
                goto continue_parseField;
            }
            goto break_parseField;
        } else {
            // Quoted string field
            var fieldPos = pos;
            line = line[(int)(quoteLen)..];
            pos.col += quoteLen;
            while (ᐧ) {
                nint i = bytes.IndexByte(line, (rune)'"');
                if (i >= 0){
                    // Hit next quote.
                    r.recordBuffer = appendꓸꓸꓸ(r.recordBuffer, line.slice(0, i));
                    line = line.slice(i + quoteLen);
                    pos.col += i + quoteLen;
                    {
                        var rn = nextRune(line);
                        switch (ᐧ) {
                        case {} when rn is (rune)'"': {
                            r.recordBuffer = append(r.recordBuffer, // `""` sequence (append quote).
 (byte)((rune)'"'));
                            line = line[(int)(quoteLen)..];
                            pos.col += quoteLen;
                            break;
                        }
                        case {} when rn == r.Comma: {
                            line = line.slice(commaLen);
                            pos.col += commaLen;
                            r.fieldIndexes = append(r.fieldIndexes, // `",` sequence (end of field).
 len(r.recordBuffer));
                            r.fieldPositions = append(r.fieldPositions, fieldPos);
                            goto continue_parseField;
                            break;
                        }
                        case {} when lengthNL(line) == len(line): {
                            r.fieldIndexes = append(r.fieldIndexes, // `"\n` sequence (end of line).
 len(r.recordBuffer));
                            r.fieldPositions = append(r.fieldPositions, fieldPos);
                            goto break_parseField;
                            break;
                        }
                        case {} when r.LazyQuotes: {
                            r.recordBuffer = append(r.recordBuffer, // `"` sequence (bare quote).
 (byte)((rune)'"'));
                            break;
                        }
                        default: {
                            err = new ParseErrorжerror(Ꮡ(new ParseError( // `"*` sequence (invalid non-escaped quote).
StartLine: recLine, Line: r.numLine, Column: pos.col - quoteLen, Err: ErrQuote)));
                            goto break_parseField;
                            break;
                        }}
                    }

                } else 
                if (len(line) > 0){
                    // Hit end of line (copy all data so far).
                    r.recordBuffer = appendꓸꓸꓸ(r.recordBuffer, line);
                    if (errRead != default!) {
                        goto break_parseField;
                    }
                    pos.col += len(line);
                    (line, errRead) = r.readLine();
                    if (len(line) > 0) {
                        pos.line++;
                        pos.col = 1;
                    }
                    if (AreEqual(errRead, io.EOF)) {
                        errRead = default!;
                    }
                } else {
                    // Abrupt end of file (EOF or error).
                    if (!r.LazyQuotes && errRead == default!) {
                        err = new ParseErrorжerror(Ꮡ(new ParseError(StartLine: recLine, Line: pos.line, Column: pos.col, Err: ErrQuote)));
                        goto break_parseField;
                    }
                    r.fieldIndexes = append(r.fieldIndexes, len(r.recordBuffer));
                    r.fieldPositions = append(r.fieldPositions, fieldPos);
                    goto break_parseField;
                }
            }
        }
continue_parseField:;
    }
break_parseField:;
    if (err == default!) {
        err = errRead;
    }
    // Create a single string and create slices out of it.
    // This pins the memory of the fields together, but allocates once.
    @string str = ((@string)r.recordBuffer); // Convert to string once to batch allocations
    dst = dst[..0];
    if (cap(dst) < len(r.fieldIndexes)) {
        dst = new slice<@string>(len(r.fieldIndexes));
    }
    dst = dst.slice(0, len(r.fieldIndexes));
    nint preIdx = default!;
    foreach (var (i, idx) in r.fieldIndexes) {
        dst[i] = str.slice(preIdx, idx);
        preIdx = idx;
    }
    // Check or update the expected fields per record.
    if (r.FieldsPerRecord > 0){
        if (len(dst) != r.FieldsPerRecord && err == default!) {
            err = new ParseErrorжerror(Ꮡ(new ParseError(
                StartLine: recLine,
                Line: recLine,
                Column: 1,
                Err: ErrFieldCount
            )));
        }
    } else 
    if (r.FieldsPerRecord == 0) {
        r.FieldsPerRecord = len(dst);
    }
    return (dst, err);
}

} // end csv_package

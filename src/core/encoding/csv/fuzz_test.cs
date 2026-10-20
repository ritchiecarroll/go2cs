// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using io = io_package;
using static go.encoding.csv_package;

partial class csv_internal_test_package {

public static void FuzzRoundtrip(ж<testing.F> Ꮡf) {
    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> @in) => {
        var buf = @new<bytes.Buffer>();

        t.Logf("input = %q"u8, @in);
        foreach (var (_, tt) in new global::go.encoding.csv_package.Reader[]{
            new(Comma: (rune)','),
            new(Comma: (rune)';'),
            new(Comma: (rune)'\t'),
            new(Comma: (rune)',', LazyQuotes: true),
            new(Comma: (rune)',', TrimLeadingSpace: true),
            new(Comma: (rune)',', Comment: (rune)'#'),
            new(Comma: (rune)',', Comment: (rune)';')
        }.slice()) {
            t.Logf("With options:"u8);
            t.Logf("  Comma            = %q"u8, tt.Comma);
            t.Logf("  LazyQuotes       = %t"u8, tt.LazyQuotes);
            t.Logf("  TrimLeadingSpace = %t"u8, tt.TrimLeadingSpace);
            t.Logf("  Comment          = %q"u8, tt.Comment);
            var r = NewReader(new csv_test_package.bytes_ReaderжReader(bytes.NewReader(@in)));
            r.Value.Comma = tt.Comma;
            r.Value.Comment = tt.Comment;
            r.Value.LazyQuotes = tt.LazyQuotes;
            r.Value.TrimLeadingSpace = tt.TrimLeadingSpace;

            var (records, err) = r.ReadAll();
            if (err != default!) {
                continue;
            }
            t.Logf("first records = %#v"u8, records);

            buf.Reset();
            var w = NewWriter(new csv_test_package.bytes_BufferжWriter(buf));
            w.Value.Comma = tt.Comma;
            err = w.WriteAll(records);
            if (err != default!) {
                t.Logf("writer  = %#v\n"u8, w.OrTypedNil());
                t.Logf("records = %v\n"u8, records);
                t.Fatal(err);
            }
            if (tt.Comment != 0) {
                // Writer doesn't support comments, so it can turn the quoted record "#"
                // into a non-quoted comment line, failing the roundtrip check below.
                continue;
            }
            t.Logf("second input = %q"u8, buf.Bytes());

            r = NewReader(new csv_test_package.bytes_BufferжReader(buf));
            r.Value.Comma = tt.Comma;
            r.Value.Comment = tt.Comment;
            r.Value.LazyQuotes = tt.LazyQuotes;
            r.Value.TrimLeadingSpace = tt.TrimLeadingSpace;
            (var result, err) = r.ReadAll();
            if (err != default!) {
                t.Logf("reader  = %#v\n"u8, r.OrTypedNil());
                t.Logf("records = %v\n"u8, records);
                t.Fatal(err);
            }

            // The reader turns \r\n into \n.
            foreach (var (_, record) in records) {
                foreach (var (i, s) in record) {
                    record[i] = strings.ReplaceAll(s, "\r\n"u8, "\n"u8);
                }
            }
            // Note that the reader parses the quoted record "" as an empty string,
            // and the writer turns that into an empty line, which the reader skips over.
            // Filter those out to avoid false positives.
            records = slices.DeleteFunc(records, (slice<@string> record) => len(record) == 1 && record[0] == "");
            // The reader uses nil when returning no records at all.
            if (len(records) == 0) {
                records = default!;
            }

            if (!reflect.DeepEqual(records, result)) {
                t.Fatalf("first read got %#v, second got %#v"u8, records, result);
            }
        }
    });
}

} // end csv_internal_test_package

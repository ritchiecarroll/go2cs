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

public static void FuzzRoundtrip(ж<testing.F> f) {
    f.Fuzz((ж<testing.T> t, slice<byte> @in) => {
        var buf = @new<bytes.Buffer>();

        t.Logf("input = %q", @in);
        foreach (var (_, tt) in slice<global::go.encoding.csv_package.Reader>([
            new(Comma: ','),
            new(Comma: ';'),
            new(Comma: '\t'),
            new(Comma: ',', LazyQuotes: true),
            new(Comma: ',', TrimLeadingSpace: true),
            new(Comma: ',', Comment: '#'),
            new(Comma: ',', Comment: ';')
        ])) {
            t.Logf("With options:");
            t.Logf("  Comma            = %q", tt.Comma);
            t.Logf("  LazyQuotes       = %t", tt.LazyQuotes);
            t.Logf("  TrimLeadingSpace = %t", tt.TrimLeadingSpace);
            t.Logf("  Comment          = %q", tt.Comment);
            var r = NewReader(new csv_test_package.bytes_ReaderжReader(bytes.NewReader(@in)));
            r.Comma = tt.Comma;
            r.Comment = tt.Comment;
            r.LazyQuotes = tt.LazyQuotes;
            r.TrimLeadingSpace = tt.TrimLeadingSpace;

            var (records, err) = r.ReadAll();
            if (err != nil) {
                continue;
            }
            t.Logf("first records = %#v", records);

            buf.Reset();
            var w = NewWriter(new csv_test_package.bytes_BufferжWriter(buf));
            w.Comma = tt.Comma;
            err = w.WriteAll(records);
            if (err != nil) {
                t.Logf("writer  = %#v\n", w.OrTypedNil());
                t.Logf("records = %v\n", records);
                t.Fatal(err);
            }
            if (tt.Comment != 0) {
                // Writer doesn't support comments, so it can turn the quoted record "#"
                // into a non-quoted comment line, failing the roundtrip check below.
                continue;
            }
            t.Logf("second input = %q", buf.Bytes());

            r = NewReader(new csv_test_package.bytes_BufferжReader(buf));
            r.Comma = tt.Comma;
            r.Comment = tt.Comment;
            r.LazyQuotes = tt.LazyQuotes;
            r.TrimLeadingSpace = tt.TrimLeadingSpace;
            (var result, err) = r.ReadAll();
            if (err != nil) {
                t.Logf("reader  = %#v\n", r.OrTypedNil());
                t.Logf("records = %v\n", records);
                t.Fatal(err);
            }

            // The reader turns \r\n into \n.
            foreach (var (_, record) in records) {
                foreach (var (i, s) in record) {
                    record[i] = strings.ReplaceAll(s, "\r\n", "\n");
                }
            }
            // Note that the reader parses the quoted record "" as an empty string,
            // and the writer turns that into an empty line, which the reader skips over.
            // Filter those out to avoid false positives.
            records = slices.DeleteFunc(records, (slice<@string> record) => len(record) == 1 && record[0] == "");
            // The reader uses nil when returning no records at all.
            if (len(records) == 0) {
                records = default;
            }

            if (!reflect.DeepEqual(records, result)) {
                t.Fatalf("first read got %#v, second got %#v", records, result);
            }
        }
    });
}

} // end csv_internal_test_package

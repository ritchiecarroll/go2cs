// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !goexperiment.jsonv2
namespace go.encoding;

using bytes = bytes_package;
using io = io_package;
using testing = testing_package;
using static go.encoding.json_package;

partial class json_internal_test_package {

public static void FuzzUnmarshalJSON(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.Add(slice<byte>("""
{
"object": {
	"slice": [
		1,
		2.0,
		"3",
		[4],
		{5: {}}
	]
},
"slice": [[]],
"string": ":)",
"int": 1e5,
"float": 3e-9"
}
"""u8.Raw()));

    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> b) => {
        foreach (var (_, typ) in range(slice<Func<any>>([
            () => @new<any>(),
            () => @new<map<@string, any>>(),
            () => @new<slice<any>>()
        ]))) {
            var i = typ();
            {
                var errΔ1 = Unmarshal(b, i); if (errΔ1 != nil) {
                    return;
                }
            }

            var (encoded, err) = Marshal(i);
            if (err != nil) {
                t.Fatalf("failed to marshal: %s", err);
            }

            {
                var errΔ1 = Unmarshal(encoded, i); if (errΔ1 != nil) {
                    t.Fatalf("failed to roundtrip: %s", errΔ1);
                }
            }
        }
    });
}

public static void FuzzDecoderToken(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.Add(slice<byte>("""
{
"object": {
	"slice": [
		1,
		2.0,
		"3",
		[4],
		{5: {}}
	]
},
"slice": [[]],
"string": ":)",
"int": 1e5,
"float": 3e-9"
}
"""u8.Raw()));

    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> b) => {
        var r = bytes.NewReader(b);
        var d = NewDecoder(new json_test_package.bytes_ReaderжReader(r));
        for (;;) {
            var (_, err) = d.Token();
            if (err != nil) {
                if (AreEqual(err, io.EOF)) {
                    break;
                }
                return;
            }
        }
    });
}

} // end json_internal_test_package

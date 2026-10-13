// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using bytes = bytes_package;
using json = encoding.json_package;
using slog = go.log.slog_package;
using testing = testing_package;
using slogtest = go.testing.slogtest_package;
using encoding;
using go.log;
using go.testing;
using io = io_package;

partial class slogtest_test_package {

public static void TestRun(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);

    var newHandler = (ж<testing.T> _) => {
        Ꮡbuf.Value.Reset();
        return new slog.JSONHandlerжΔHandler(slog.NewJSONHandler(new bytes_BufferжWriter(Ꮡbuf), nil));
    };
    var result = (ж<testing.T> tΔ1) => {
        ref var m = ref heap<map<@string, any>>(out var Ꮡm);
        m = new map<@string, any>{};
        {
            var err = json.Unmarshal(Ꮡbuf.Value.Bytes(), Ꮡm); if (err != default!) {
                tΔ1.Fatal(err);
            }
        }
        return m;
    };

    slogtest.Run(Ꮡt, newHandler, result);
}

} // end slogtest_test_package

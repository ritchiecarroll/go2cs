// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using context = context_package;
using fmt = fmt_package;
using Δslog = go.log.slog_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.log;
using io = io_package;
using path;
using static go.log.slog_internal_test_package;
using ꓸꓸꓸany = Span<any>;

partial class slog_test_package {

// Infof is an example of a user-defined logging function that wraps slog.
// The log record contains the source position of the caller of Infof.
public static partial void Infof(ж<Δslog.Logger> Ꮡlogger, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    ref var logger = ref Ꮡlogger.DerefOrNull();
    if (!logger.Enabled(context.Background(), Δslog.LevelInfo)) {
        return;
    }
    array<uintptr> pcs = new(1);
    runtime.Callers(2, pcs[..]); // skip [Callers, Infof]
    var r = Δslog.NewRecord(time.Now(), Δslog.LevelInfo, fmt.Sprintf(format, args.ꓸꓸꓸ), pcs[0]);
    _ = logger.Handler().Handle(context.Background(), r);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object formattedˢ = (@string)"formatted"u8;

public static void Example_wrapping() {
    var replace = (slice<@string> groups, Δslog.Attr a) => {
        // Remove time.
        if (a.Key == Δslog.TimeKey && len(groups) == 0) {
            return new Δslog.Attr(nil);
        }
        // Remove the directory from the source's filename.
        if (a.Key == Δslog.SourceKey) {
            var source = a.Value.Any()._<ж<Δslog.Source>>();
            source.Value.File = filepath.Base((~source).File);
        }
        return a;
    };
    var logger = Δslog.New(new Δslog.TextHandlerжΔHandler(Δslog.NewTextHandler(new os.FileжWriter(os.Stdout), Ꮡ(new Δslog.HandlerOptions(AddSource: true, ReplaceAttr: replace)))));
    Infof(logger, "message, %s"u8, formattedˢ);
}

// Output:
// level=INFO source=example_wrap_test.go:43 msg="message, formatted"

} // end slog_test_package

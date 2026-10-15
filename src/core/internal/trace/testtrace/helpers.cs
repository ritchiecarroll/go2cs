// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using bytes = bytes_package;
using fmt = fmt_package;
using testenv = go.@internal.testenv_package;
using raw = go.@internal.trace.raw_package;
using version = go.@internal.trace.version_package;
using io = io_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using go.@internal;
using go.@internal.trace;

partial class testtrace_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gotipˢ = "gotip"u8;
internal static readonly @string go1ˢ = "go1"u8;
internal static readonly object convertThisToARawTraceˢ = (@string)"Convert this to a raw trace with `go test internal/trace/testtrace -convert in.tracetxt -out out.trace`"u8;

// Dump saves the trace to a file or the test log.
public static void Dump(ж<testing.T> Ꮡt, @string testName, slice<byte> traceBytes, bool forceToFile) {
    var onBuilder = testenv.Builder() != ""u8;
    var onOldBuilder = !strings.Contains(testenv.Builder(), gotipˢ) && !strings.Contains(testenv.Builder(), go1ˢ);

    if (onBuilder && !forceToFile){
        // Dump directly to the test log on the builder, since this
        // data is critical for debugging and this is the only way
        // we can currently make sure it's retained.
        @string s = dumpTraceToText(Ꮡt, traceBytes);
        if (onOldBuilder && len(s) > (1 << (int)(20)) + (512 << (int)(10))){
            // The old build infrastructure truncates logs at ~2 MiB.
            // Let's assume we're the only failure and give ourselves
            // up to 1.5 MiB to dump the trace.
            //
            // TODO(mknyszek): Remove this when we've migrated off of
            // the old infrastructure.
            Ꮡt.Logf("text trace too large to dump (%d bytes)"u8, len(s));
        } else {
            Ꮡt.Log(s);
            Ꮡt.Log(convertThisToARawTraceˢ);
        }
    } else {
        // We asked to dump the trace or failed. Write the trace to a file.
        Ꮡt.Logf("wrote trace to file: %s"u8, dumpTraceToFile(Ꮡt, testName, traceBytes));
    }
}

internal static @string dumpTraceToText(ж<testing.T> Ꮡt, slice<byte> b) {
    Ꮡt.Helper();

    var (br, err) = raw.NewReader(new bytes_ReaderжReader(bytes.NewReader(b)));
    if (err != default!) {
        Ꮡt.Fatalf("dumping trace: %v"u8, err);
    }
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    (var tw, err) = raw.NewTextWriter(new strings_BuilderжWriter(Ꮡsb), version.Current);
    if (err != default!) {
        Ꮡt.Fatalf("dumping trace: %v"u8, err);
    }
    while (ᐧ) {
        var (ev, errΔ1) = br.ReadEvent();
        if (AreEqual(errΔ1, io.EOF)) {
            break;
        }
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("dumping trace: %v"u8, errΔ1);
        }
        {
            var errΔ2 = tw.WriteEvent(ev); if (errΔ2 != default!) {
                Ꮡt.Fatalf("dumping trace: %v"u8, errΔ2);
            }
        }
    }
    return sb.String();
}

internal static @string dumpTraceToFile(ж<testing.T> Ꮡt, @string testName, slice<byte> b) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Helper();

        @string name = fmt.Sprintf("%s.trace."u8, testName);
        var (f, err) = os.CreateTemp(Ꮡt.ArtifactDir(), name);
        if (err != default!) {
            Ꮡt.Fatalf("creating temp file: %v"u8, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        {
            var (_, errΔ1) = io.Copy(new os.FileжWriter(f), new bytes_ReaderжReader(bytes.NewReader(b))); if (errΔ1 != default!) {
                Ꮡt.Fatalf("writing trace dump to %q: %v"u8, f.Name(), errΔ1);
            }
        }
        return f.Name();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end testtrace_package

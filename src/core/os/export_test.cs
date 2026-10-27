// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fs = go.io.fs_package;
using static go.os_package;
using time = time_package;

partial class os_internal_test_package {

// Export for testing.
public static readonly ж<Func<fs.FileInfo, time.Time>> ᏑAtime = new StaticBox<Func<fs.FileInfo, time.Time>>(atime);
public static ref Func<fs.FileInfo, time.Time> Atime => ref ((StaticBox<Func<fs.FileInfo, time.Time>>)ᏑAtime).ValueSlot;

public static error ErrWriteAtInAppendMode;
internal static void initᴛErrWriteAtInAppendMode() { ErrWriteAtInAppendMode = errWriteAtInAppendMode; }

public static error ErrPatternHasSeparator;
internal static void initᴛErrPatternHasSeparator() { ErrPatternHasSeparator = errPatternHasSeparator; }

public static error ErrPathEscapes;
internal static void initᴛErrPathEscapes() { ErrPathEscapes = errPathEscapes; }

[GoInit] internal static void init() {
    checkWrapErr = true;
}

public static readonly ж<Func<int64, Func<slice<byte>, (nint, error)>, (slice<byte>, error)>> ᏑExportReadFileContents = new StaticBox<Func<int64, Func<slice<byte>, (nint, error)>, (slice<byte>, error)>>(readFileContents);
public static ref Func<int64, Func<slice<byte>, (nint, error)>, (slice<byte>, error)> ExportReadFileContents => ref ((StaticBox<Func<int64, Func<slice<byte>, (nint, error)>, (slice<byte>, error)>>)ᏑExportReadFileContents).ValueSlot;

// cleanuper stands in for *testing.T, since we can't import testing in os.
public partial interface cleanuper {
    void Cleanup(Action _);
}

public static void SetStatHook(cleanuper t, Func<ж<global::go.os_package.File>, @string, (fs.FileInfo, error)> f) {
    var oldstathook = stathook;
    var oldstathookʗ1 = oldstathook;
    t.Cleanup(() => {
        stathook = oldstathookʗ1;
    });
    stathook = f;
}

} // end os_internal_test_package

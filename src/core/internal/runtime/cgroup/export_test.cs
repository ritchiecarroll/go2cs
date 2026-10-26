// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using LineReader = global::go.@internal.runtime.cgroup_package.lineReader;

namespace go.@internal.runtime;

using static go.@internal.runtime.cgroup_package;

partial class cgroup_internal_test_package {

internal static error Next(this ref LineReader l) {
    return l.next();
}

internal static slice<byte> Line(this ref LineReader l) {
    return l.line();
}

internal static ж<LineReader> NewLineReader(nint fd, slice<byte> scratch, Func<nint, slice<byte>, (nint, uintptr)> read) {
    return newLineReader(fd, scratch, read);
}

public static error ErrEOF;
internal static void initᴛErrEOF() { ErrEOF = errEOF; }
public static error ErrIncompleteLine;
internal static void initᴛErrIncompleteLine() { ErrIncompleteLine = errIncompleteLine; }
public static error ErrMalformedFile;
internal static void initᴛErrMalformedFile() { ErrMalformedFile = errMalformedFile; }

public static Func<slice<byte>, bool> ContainsCPU = containsCPU;

public static Func<slice<byte>, (int64, error)> ParseV1Number;
internal static void initᴛParseV1Number() { ParseV1Number = parseV1Number; }

public static Func<slice<byte>, (float64, bool, error)> ParseV2Limit;
internal static void initᴛParseV2Limit() { ParseV2Limit = parseV2Limit; }

public static Func<nint, Func<nint, slice<byte>, (nint, uintptr)>, slice<byte>, slice<byte>, (nint, global::go.@internal.runtime.cgroup_package.Version, error)> ParseCPUCgroup;
internal static void initᴛParseCPUCgroup() { ParseCPUCgroup = parseCPUCgroup; }

public static Func<nint, Func<nint, slice<byte>, (nint, uintptr)>, slice<byte>, slice<byte>, global::go.@internal.runtime.cgroup_package.Version, slice<byte>, (nint, error)> ParseCPUMount;
internal static void initᴛParseCPUMount() { ParseCPUMount = parseCPUMount; }

public static Func<slice<byte>, slice<byte>, (nint, error)> UnescapePath;
internal static void initᴛUnescapePath() { UnescapePath = unescapePath; }

} // end cgroup_internal_test_package

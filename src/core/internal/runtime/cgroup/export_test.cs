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

public static readonly ж<Func<slice<byte>, bool>> ᏑContainsCPU = new StaticBox<Func<slice<byte>, bool>>(containsCPU);
public static ref Func<slice<byte>, bool> ContainsCPU => ref ((StaticBox<Func<slice<byte>, bool>>)ᏑContainsCPU).ValueSlot;

public static readonly ж<Func<slice<byte>, (int64, error)>> ᏑParseV1Number = new StaticBox<Func<slice<byte>, (int64, error)>>(default(Func<slice<byte>, (int64, error)>));
public static ref Func<slice<byte>, (int64, error)> ParseV1Number => ref ((StaticBox<Func<slice<byte>, (int64, error)>>)ᏑParseV1Number).ValueSlot;
internal static void initᴛParseV1Number() { ParseV1Number = parseV1Number; }

public static readonly ж<Func<slice<byte>, (float64, bool, error)>> ᏑParseV2Limit = new StaticBox<Func<slice<byte>, (float64, bool, error)>>(default(Func<slice<byte>, (float64, bool, error)>));
public static ref Func<slice<byte>, (float64, bool, error)> ParseV2Limit => ref ((StaticBox<Func<slice<byte>, (float64, bool, error)>>)ᏑParseV2Limit).ValueSlot;
internal static void initᴛParseV2Limit() { ParseV2Limit = parseV2Limit; }

public static readonly ж<Func<nint, Func<nint, slice<byte>, (nint, uintptr)>, slice<byte>, slice<byte>, (nint, global::go.@internal.runtime.cgroup_package.Version, error)>> ᏑParseCPUCgroup = new StaticBox<Func<nint, Func<nint, slice<byte>, (nint, uintptr)>, slice<byte>, slice<byte>, (nint, global::go.@internal.runtime.cgroup_package.Version, error)>>(default(Func<nint, Func<nint, slice<byte>, (nint, uintptr)>, slice<byte>, slice<byte>, (nint, global::go.@internal.runtime.cgroup_package.Version, error)>));
public static ref Func<nint, Func<nint, slice<byte>, (nint, uintptr)>, slice<byte>, slice<byte>, (nint, global::go.@internal.runtime.cgroup_package.Version, error)> ParseCPUCgroup => ref ((StaticBox<Func<nint, Func<nint, slice<byte>, (nint, uintptr)>, slice<byte>, slice<byte>, (nint, global::go.@internal.runtime.cgroup_package.Version, error)>>)ᏑParseCPUCgroup).ValueSlot;
internal static void initᴛParseCPUCgroup() { ParseCPUCgroup = parseCPUCgroup; }

public static readonly ж<Func<nint, Func<nint, slice<byte>, (nint, uintptr)>, slice<byte>, slice<byte>, global::go.@internal.runtime.cgroup_package.Version, slice<byte>, (nint, error)>> ᏑParseCPUMount = new StaticBox<Func<nint, Func<nint, slice<byte>, (nint, uintptr)>, slice<byte>, slice<byte>, global::go.@internal.runtime.cgroup_package.Version, slice<byte>, (nint, error)>>(default(Func<nint, Func<nint, slice<byte>, (nint, uintptr)>, slice<byte>, slice<byte>, global::go.@internal.runtime.cgroup_package.Version, slice<byte>, (nint, error)>));
public static ref Func<nint, Func<nint, slice<byte>, (nint, uintptr)>, slice<byte>, slice<byte>, global::go.@internal.runtime.cgroup_package.Version, slice<byte>, (nint, error)> ParseCPUMount => ref ((StaticBox<Func<nint, Func<nint, slice<byte>, (nint, uintptr)>, slice<byte>, slice<byte>, global::go.@internal.runtime.cgroup_package.Version, slice<byte>, (nint, error)>>)ᏑParseCPUMount).ValueSlot;
internal static void initᴛParseCPUMount() { ParseCPUMount = parseCPUMount; }

public static readonly ж<Func<slice<byte>, slice<byte>, (nint, error)>> ᏑUnescapePath = new StaticBox<Func<slice<byte>, slice<byte>, (nint, error)>>(default(Func<slice<byte>, slice<byte>, (nint, error)>));
public static ref Func<slice<byte>, slice<byte>, (nint, error)> UnescapePath => ref ((StaticBox<Func<slice<byte>, slice<byte>, (nint, error)>>)ᏑUnescapePath).ValueSlot;
internal static void initᴛUnescapePath() { UnescapePath = unescapePath; }

} // end cgroup_internal_test_package

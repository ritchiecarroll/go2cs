// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class coverage_package {

// Types and constants related to the output files written
// by code coverage tooling. When a coverage-instrumented binary
// is run, it emits two output files: a meta-data output file, and
// a counter data output file.
//.....................................................................
//
// Meta-data definitions:
//
// The meta-data file is composed of a file header, a series of
// meta-data blobs/sections (one per instrumented package), and an offsets
// area storing the offsets of each section. Format of the meta-data
// file looks like:
//
// --header----------
//  | magic: [4]byte magic string
//  | version
//  | total length of meta-data file in bytes
//  | numPkgs: number of package entries in file
//  | hash: [16]byte hash of entire meta-data payload
//  | offset to string table section
//  | length of string table
//  | number of entries in string table
//  | counter mode
//  | counter granularity
//  --package offsets table------
//  <offset to pkg 0>
//  <offset to pkg 1>
//  ...
//  --package lengths table------
//  <length of pkg 0>
//  <length of pkg 1>
//  ...
//  --string table------
//  <uleb128 len> 8
//  <data> "somestring"
//  ...
//  --package payloads------
//  <meta-symbol for pkg 0>
//  <meta-symbol for pkg 1>
//  ...
//
// Each package payload is a stand-alone blob emitted by the compiler,
// and does not depend on anything else in the meta-data file. In
// particular, each blob has it's own string table. Note that the
// file-level string table is expected to be very short (most strings
// will be in the meta-data blobs themselves).

// CovMetaMagic holds the magic string for a meta-data file.
public static array<byte> CovMetaMagic = new byte[]{(rune)'\x00', (rune)'\x63', (rune)'\x76', (rune)'\x6d'}.array();

// MetaFilePref is a prefix used when emitting meta-data files; these
// files are of the form "covmeta.<hash>", where hash is a hash
// computed from the hashes of all the package meta-data symbols in
// the program.
public static readonly @string MetaFilePref = "covmeta"u8;

// MetaFileVersion contains the current (most recent) meta-data file version.
public static UntypedInt MetaFileVersion => 1;

// MetaFileHeader stores file header information for a meta-data file.
partial struct MetaFileHeader {
    public array<byte> Magic = new(4);
    public uint32 Version;
    public uint64 TotalLength;
    public uint64 Entries;
    public array<byte> MetaFileHash = new(16);
    public uint32 StrTabOffset;
    public uint32 StrTabLength;
    public CounterMode CMode;
    public CounterGranularity CGranularity;
    internal array<byte> _ = new(6); // padding
}

// MetaSymbolHeader stores header information for a single
// meta-data blob, e.g. the coverage meta-data payload
// computed for a given Go package.
partial struct MetaSymbolHeader {
    public uint32 Length; // size of meta-symbol payload in bytes
    public uint32 PkgName; // string table index
    public uint32 PkgPath; // string table index
    public uint32 ModulePath; // string table index
    public array<byte> MetaHash = new(16);
    internal byte _;    // currently unused
    internal array<byte> __ = new(3); // padding
    public uint32 NumFiles;
    public uint32 NumFuncs;
}

public static UntypedInt CovMetaHeaderSize => /* 16 + 4 + 4 + 4 + 4 + 4 + 4 + 4 */ 44; // keep in sync with above

// As an example, consider the following Go package:
//
// 01: package p
// 02:
// 03: var v, w, z int
// 04:
// 05: func small(x, y int) int {
// 06:   v++
// 07:   // comment
// 08:   if y == 0 {
// 09:     return x
// 10:   }
// 11:   return (x << 1) ^ (9 / y)
// 12: }
// 13:
// 14: func Medium(q, r int) int {
// 15:   s1 := small(q, r)
// 16:   z += s1
// 17:   s2 := small(r, q)
// 18:   w -= s2
// 19:   return w + z
// 20: }
//
// The meta-data blob for the single package above might look like the
// following:
//
// -- MetaSymbolHeader header----------
//  | size: size of this blob in bytes
//  | packagepath: <path to p>
//  | modulepath: <modpath for p>
//  | nfiles: 1
//  | nfunctions: 2
//  --func offsets table------
//  <offset to func 0>
//  <offset to func 1>
//  --string table (contains all files and functions)------
//  | <uleb128 len> 4
//  | <data> "p.go"
//  | <uleb128 len> 5
//  | <data> "small"
//  | <uleb128 len> 6
//  | <data> "Medium"
//  --func 0------
//  | <uleb128> num units: 3
//  | <uleb128> func name: S1 (index into string table)
//  | <uleb128> file: S0 (index into string table)
//  | <unit 0>:  S0   L6     L8    2
//  | <unit 1>:  S0   L9     L9    1
//  | <unit 2>:  S0   L11    L11   1
//  --func 1------
//  | <uleb128> num units: 1
//  | <uleb128> func name: S2 (index into string table)
//  | <uleb128> file: S0 (index into string table)
//  | <unit 0>:  S0   L15    L19   5
//  ---end-----------
// The following types and constants used by the meta-data encoder/decoder.

// FuncDesc encapsulates the meta-data definitions for a single Go function.
// This version assumes that we're looking at a function before inlining;
// if we want to capture a post-inlining view of the world, the
// representations of source positions would need to be a good deal more
// complicated.
partial struct FuncDesc {
    public @string Funcname;
    public @string Srcfile;
    public slice<CoverableUnit> Units;
    public bool Lit; // true if this is a function literal
}

// CoverableUnit describes the source characteristics of a single
// program unit for which we want to gather coverage info. Coverable
// units are either "simple" or "intraline"; a "simple" coverable unit
// corresponds to a basic block (region of straight-line code with no
// jumps or control transfers). An "intraline" unit corresponds to a
// logical clause nested within some other simple unit. A simple unit
// will have a zero Parent value; for an intraline unit NxStmts will
// be zero and Parent will be set to 1 plus the index of the
// containing simple statement. Example:
//
//	L7:   q := 1
//	L8:   x := (y == 101 || launch() == false)
//	L9:   r := x * 2
//
// For the code above we would have three simple units (one for each
// line), then an intraline unit describing the "launch() == false"
// clause in line 8, with Parent pointing to the index of the line 8
// unit in the units array.
//
// Note: in the initial version of the coverage revamp, only simple
// units will be in use.
partial struct CoverableUnit {
    public uint32 StLine, StCol;
    public uint32 EnLine, EnCol;
    public uint32 NxStmts;
    public uint32 Parent;
}

partial struct CounterMode /*num:uint8*/;

public static CounterMode CtrModeInvalid => /* iota */ 0;
public static CounterMode CtrModeSet => 1; // "set" mode
public static CounterMode CtrModeCount => 2; // "count" mode
public static CounterMode CtrModeAtomic => 3; // "atomic" mode
public static CounterMode CtrModeRegOnly => 4; // registration-only pseudo-mode
public static CounterMode CtrModeTestMain => 5; // testmain pseudo-mode

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string setˢ = "set"u8;
private static readonly @string countˢ = "count"u8;
private static readonly @string atomicˢ = "atomic"u8;
private static readonly @string regonlyˢ = "regonly"u8;
private static readonly @string testmainˢ = "testmain"u8;
private static readonly @string invalidˢ = "<invalid>"u8;

public static @string String(this CounterMode cm) {
    var exprᴛ1 = cm;
    if (exprᴛ1 == CtrModeSet) {
        return setˢ;
    }
    if (exprᴛ1 == CtrModeCount) {
        return countˢ;
    }
    if (exprᴛ1 == CtrModeAtomic) {
        return atomicˢ;
    }
    if (exprᴛ1 == CtrModeRegOnly) {
        return regonlyˢ;
    }
    if (exprᴛ1 == CtrModeTestMain) {
        return testmainˢ;
    }

    return invalidˢ;
}

public static CounterMode ParseCounterMode(@string mode) {
    CounterMode cm = default!;
    var exprᴛ1 = mode;
    if (exprᴛ1 == "set"u8) {
        cm = CtrModeSet;
    }
    else if (exprᴛ1 == "count"u8) {
        cm = CtrModeCount;
    }
    else if (exprᴛ1 == "atomic"u8) {
        cm = CtrModeAtomic;
    }
    else if (exprᴛ1 == "regonly"u8) {
        cm = CtrModeRegOnly;
    }
    else if (exprᴛ1 == "testmain"u8) {
        cm = CtrModeTestMain;
    }
    else { /* default: */
        cm = CtrModeInvalid;
    }

    return cm;
}

partial struct CounterGranularity /*num:uint8*/;

public static CounterGranularity CtrGranularityInvalid => /* iota */ 0;
public static CounterGranularity CtrGranularityPerBlock => 1;
public static CounterGranularity CtrGranularityPerFunc => 2;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string perblockˢ = "perblock"u8;
private static readonly @string perfuncˢ = "perfunc"u8;

public static @string String(this CounterGranularity cm) {
    var exprᴛ1 = cm;
    if (exprᴛ1 == CtrGranularityPerBlock) {
        return perblockˢ;
    }
    if (exprᴛ1 == CtrGranularityPerFunc) {
        return perfuncˢ;
    }

    return invalidˢ;
}

// Name of file within the "go test -cover" temp coverdir directory
// containing a list of meta-data files for packages being tested
// in a "go test -coverpkg=... ..." run. This constant is shared
// by the Go command and by the coverage runtime.
public static readonly @string MetaFilesFileName = "metafiles.txt"u8;

// MetaFileCollection contains information generated by the Go command and
// the read in by coverage test support functions within an executing
// "go test -cover" binary.
partial struct MetaFileCollection {
    public slice<@string> ImportPaths;
    public slice<@string> MetaFileFragments;
}

//.....................................................................
//
// Counter data definitions:
//
// A counter data file is composed of a file header followed by one or
// more "segments" (each segment representing a given run or partial
// run of a give binary) followed by a footer.

// CovCounterMagic holds the magic string for a coverage counter-data file.
public static array<byte> CovCounterMagic = new byte[]{(rune)'\x00', (rune)'\x63', (rune)'\x77', (rune)'\x6d'}.array();

// CounterFileVersion stores the most recent counter data file version.
public static UntypedInt CounterFileVersion => 1;

// CounterFileHeader stores files header information for a counter-data file.
partial struct CounterFileHeader {
    public array<byte> Magic = new(4);
    public uint32 Version;
    public array<byte> MetaHash = new(16);
    public CounterFlavor CFlavor;
    public bool BigEndian;
    internal array<byte> _ = new(6); // padding
}

// CounterSegmentHeader encapsulates information about a specific
// segment in a counter data file, which at the moment contains
// counters data from a single execution of a coverage-instrumented
// program. Following the segment header will be the string table and
// args table, and then (possibly) padding bytes to bring the byte
// size of the preamble up to a multiple of 4. Immediately following
// that will be the counter payloads.
//
// The "args" section of a segment is used to store annotations
// describing where the counter data came from; this section is
// basically a series of key-value pairs (can be thought of as an
// encoded 'map[string]string'). At the moment we only write os.Args()
// data to this section, using pairs of the form "argc=<integer>",
// "argv0=<os.Args[0]>", "argv1=<os.Args[1]>", and so on. In the
// future the args table may also include things like GOOS/GOARCH
// values, and/or tags indicating which tests were run to generate the
// counter data.
partial struct CounterSegmentHeader {
    public uint64 FcnEntries;
    public uint32 StrTabLen;
    public uint32 ArgsLen;
}

// CounterFileFooter appears at the tail end of a counter data file,
// and stores the number of segments it contains.
partial struct CounterFileFooter {
    public array<byte> Magic = new(4);
    internal array<byte> _ = new(4); // padding
    public uint32 NumSegments;
    internal array<byte> __ = new(4); // padding
}

// CounterFilePref is the file prefix used when emitting coverage data
// output files. CounterFileTemplate describes the format of the file
// name: prefix followed by meta-file hash followed by process ID
// followed by emit UnixNanoTime.
public static readonly @string CounterFilePref = "covcounters"u8;

public static readonly @string CounterFileTempl = "%s.%x.%d.%d"u8;

public static readonly @string CounterFileRegexp = @"^%s\.(\S+)\.(\d+)\.(\d+)+$"u8;

partial struct CounterFlavor /*num:uint8*/;

public static CounterFlavor CtrRaw => /* iota + 1 */ 1;
public static CounterFlavor CtrULeb128 => 2;

public static nint Round4(nint x) {
    return (nint)((x + 3) & ~(nint)3);
}

//.....................................................................
//
// Runtime counter data definitions.
//
// At runtime within a coverage-instrumented program, the "counters"
// object we associated with instrumented function can be thought of
// as a struct of the following form:
//
// struct {
//     numCtrs uint32
//     pkgid uint32
//     funcid uint32
//     counterArray [numBlocks]uint32
// }
//
// where "numCtrs" is the number of blocks / coverable units within the
// function, "pkgid" is the unique index assigned to this package by
// the runtime, "funcid" is the index of this function within its containing
// package, and "counterArray" stores the actual counters.
//
// The counter variable itself is created not as a struct but as a flat
// array of uint32's; we then use the offsets below to index into it.
public static UntypedInt NumCtrsOffset => 0;

public static UntypedInt PkgIdOffset => 1;

public static UntypedInt FuncIdOffset => 2;

public static UntypedInt FirstCtrOffset => 3;

} // end coverage_package

// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using bytes = bytes_package;
using obscuretestdata = @internal.obscuretestdata_package;
using io = io_package;
using reflect = reflect_package;
using testing = testing_package;
using @internal;
using static go.debug.macho_package;

partial class macho_internal_test_package {

internal partial struct fileTest {
    internal @string @file;
    internal global::go.debug.macho_package.FileHeader hdr;
    internal slice<any> loads;
    internal slice<ж<global::go.debug.macho_package.SectionHeader>> sections;
    internal map<@string, slice<global::go.debug.macho_package.Reloc>> relocations;
}

// LC_SYMTAB
// LC_DYSYMTAB
// LC_LOAD_DYLINKER
// LC_UUID
// LC_UNIXTHREAD
// LC_SYMTAB
// LC_DYSYMTAB
// LC_LOAD_DYLINKER
// LC_UUID
// LC_UNIXTHREAD
// LC_UUID
// LC_SEGMENT
// LC_SEGMENT
// LC_SEGMENT
// LC_SEGMENT
// LC_DYLD_INFO_ONLY
// LC_SYMTAB
// LC_DYSYMTAB
// LC_LOAD_DYLINKER
// LC_UUID
// LC_VERSION_MIN_MACOSX
// LC_SOURCE_VERSION
// LC_MAIN
// LC_LOAD_DYLIB
// LC_FUNCTION_STARTS
// LC_DATA_IN_CODE
// LC_SEGMENT
// LC_SEGMENT
// LC_SEGMENT
// LC_SEGMENT
// LC_DYLD_INFO_ONLY
// LC_SYMTAB
// LC_DYSYMTAB
// LC_LOAD_DYLINKER
// LC_UUID
// LC_VERSION_MIN_MACOSX
// LC_SOURCE_VERSION
// LC_MAIN
// LC_LOAD_DYLIB
// LC_FUNCTION_STARTS
// LC_DATA_IN_CODE
internal static ж<slice<fileTest>> ᏑfileTests = new StandardBox<slice<fileTest>>(new fileTest[]{
    new(
        "testdata/gcc-386-darwin-exec.base64"u8,
        new FileHeader(0xfeedfaceU, Cpu386, 0x3, 0x2, 0xc, 0x3c0, 0x85),
        new any[]{
            Ꮡ(new SegmentHeader(LoadCmdSegment, 0x38, "__PAGEZERO"u8, 0x0, 0x1000, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment, 0xc0, "__TEXT"u8, 0x1000, 0x1000, 0x0, 0x1000, 0x7, 0x5, 0x2, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment, 0xc0, "__DATA"u8, 0x2000, 0x1000, 0x1000, 0x1000, 0x7, 0x3, 0x2, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment, 0x7c, "__IMPORT"u8, 0x3000, 0x1000, 0x2000, 0x1000, 0x7, 0x7, 0x1, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment, 0x38, "__LINKEDIT"u8, 0x4000, 0x1000, 0x3000, 0x12c, 0x7, 0x1, 0x0, 0x0)),
            default!,
            default!,
            default!,
            default!,
            default!,
            Ꮡ(new Dylib(default!, "/usr/lib/libgcc_s.1.dylib"u8, 0x2, 0x10000, 0x10000)),
            Ꮡ(new Dylib(default!, "/usr/lib/libSystem.B.dylib"u8, 0x2, 0x6f0104, 0x10000))
        }.slice(),
        new ж<global::go.debug.macho_package.SectionHeader>[]{
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__text"u8, "__TEXT"u8, 0x1f68, 0x88, 0xf68, 0x2, 0x0, 0x0, 0x80000400U)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__cstring"u8, "__TEXT"u8, 0x1ff0, 0xd, 0xff0, 0x0, 0x0, 0x0, 0x2)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__data"u8, "__DATA"u8, 0x2000, 0x14, 0x1000, 0x2, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__dyld"u8, "__DATA"u8, 0x2014, 0x1c, 0x1014, 0x2, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__jump_table"u8, "__IMPORT"u8, 0x3000, 0xa, 0x2000, 0x6, 0x0, 0x0, 0x4000008))
        }.slice(),
        default!
    ),
    new(
        "testdata/gcc-amd64-darwin-exec.base64"u8,
        new FileHeader(0xfeedfacfU, CpuAmd64, 0x80000003U, 0x2, 0xb, 0x568, 0x85),
        new any[]{
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x48, "__PAGEZERO"u8, 0x0, 0x100000000UL, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x1d8, "__TEXT"u8, 0x100000000UL, 0x1000, 0x0, 0x1000, 0x7, 0x5, 0x5, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x138, "__DATA"u8, 0x100001000UL, 0x1000, 0x1000, 0x1000, 0x7, 0x3, 0x3, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x48, "__LINKEDIT"u8, 0x100002000UL, 0x1000, 0x2000, 0x140, 0x7, 0x1, 0x0, 0x0)),
            default!,
            default!,
            default!,
            default!,
            default!,
            Ꮡ(new Dylib(default!, "/usr/lib/libgcc_s.1.dylib"u8, 0x2, 0x10000, 0x10000)),
            Ꮡ(new Dylib(default!, "/usr/lib/libSystem.B.dylib"u8, 0x2, 0x6f0104, 0x10000))
        }.slice(),
        new ж<global::go.debug.macho_package.SectionHeader>[]{
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__text"u8, "__TEXT"u8, 0x100000f14UL, 0x6d, 0xf14, 0x2, 0x0, 0x0, 0x80000400U)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__symbol_stub1"u8, "__TEXT"u8, 0x100000f81UL, 0xc, 0xf81, 0x0, 0x0, 0x0, 0x80000408U)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__stub_helper"u8, "__TEXT"u8, 0x100000f90UL, 0x18, 0xf90, 0x2, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__cstring"u8, "__TEXT"u8, 0x100000fa8UL, 0xd, 0xfa8, 0x0, 0x0, 0x0, 0x2)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__eh_frame"u8, "__TEXT"u8, 0x100000fb8UL, 0x48, 0xfb8, 0x3, 0x0, 0x0, 0x6000000b)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__data"u8, "__DATA"u8, 0x100001000UL, 0x1c, 0x1000, 0x3, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__dyld"u8, "__DATA"u8, 0x100001020UL, 0x38, 0x1020, 0x3, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__la_symbol_ptr"u8, "__DATA"u8, 0x100001058UL, 0x10, 0x1058, 0x2, 0x0, 0x0, 0x7))
        }.slice(),
        default!
    ),
    new(
        "testdata/gcc-amd64-darwin-exec-debug.base64"u8,
        new FileHeader(0xfeedfacfU, CpuAmd64, 0x80000003U, 0xa, 0x4, 0x5a0, 0),
        new any[]{
            default!,
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x1d8, "__TEXT"u8, 0x100000000UL, 0x1000, 0x0, 0x0, 0x7, 0x5, 0x5, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x138, "__DATA"u8, 0x100001000UL, 0x1000, 0x0, 0x0, 0x7, 0x3, 0x3, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x278, "__DWARF"u8, 0x100002000UL, 0x1000, 0x1000, 0x1bc, 0x7, 0x3, 0x7, 0x0))
        }.slice(),
        new ж<global::go.debug.macho_package.SectionHeader>[]{
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__text"u8, "__TEXT"u8, 0x100000f14UL, 0x0, 0x0, 0x2, 0x0, 0x0, 0x80000400U)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__symbol_stub1"u8, "__TEXT"u8, 0x100000f81UL, 0x0, 0x0, 0x0, 0x0, 0x0, 0x80000408U)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__stub_helper"u8, "__TEXT"u8, 0x100000f90UL, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__cstring"u8, "__TEXT"u8, 0x100000fa8UL, 0x0, 0x0, 0x0, 0x0, 0x0, 0x2)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__eh_frame"u8, "__TEXT"u8, 0x100000fb8UL, 0x0, 0x0, 0x3, 0x0, 0x0, 0x6000000b)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__data"u8, "__DATA"u8, 0x100001000UL, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__dyld"u8, "__DATA"u8, 0x100001020UL, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__la_symbol_ptr"u8, "__DATA"u8, 0x100001058UL, 0x0, 0x0, 0x2, 0x0, 0x0, 0x7)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_abbrev"u8, "__DWARF"u8, 0x100002000UL, 0x36, 0x1000, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_aranges"u8, "__DWARF"u8, 0x100002036UL, 0x30, 0x1036, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_frame"u8, "__DWARF"u8, 0x100002066UL, 0x40, 0x1066, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_info"u8, "__DWARF"u8, 0x1000020a6UL, 0x54, 0x10a6, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_line"u8, "__DWARF"u8, 0x1000020faUL, 0x47, 0x10fa, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_pubnames"u8, "__DWARF"u8, 0x100002141UL, 0x1b, 0x1141, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_str"u8, "__DWARF"u8, 0x10000215cUL, 0x60, 0x115c, 0x0, 0x0, 0x0, 0x0))
        }.slice(),
        default!
    ),
    new(
        "testdata/clang-386-darwin-exec-with-rpath.base64"u8,
        new FileHeader(0xfeedfaceU, Cpu386, 0x3, 0x2, 0x10, 0x42c, 0x1200085),
        new any[]{
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            Ꮡ(new Rpath(default!, "/my/rpath"u8)),
            default!,
            default!
        }.slice(),
        default!,
        default!
    ),
    new(
        "testdata/clang-amd64-darwin-exec-with-rpath.base64"u8,
        new FileHeader(0xfeedfacfU, CpuAmd64, 0x80000003U, 0x2, 0x10, 0x4c8, 0x200085),
        new any[]{
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            Ꮡ(new Rpath(default!, "/my/rpath"u8)),
            default!,
            default!
        }.slice(),
        default!,
        default!
    ),
    new(
        "testdata/clang-386-darwin.obj.base64"u8,
        new FileHeader(0xfeedfaceU, Cpu386, 0x3, 0x1, 0x4, 0x138, 0x2000),
        default!,
        default!,
        new map<@string, slice<global::go.debug.macho_package.Reloc>>{
            ["__text"u8] = new global::go.debug.macho_package.Reloc[]{
                new(
                    Addr: 0x1d,
                    Type: (uint8)(nint)GENERIC_RELOC_VANILLA,
                    Len: 2,
                    Pcrel: true,
                    Extern: true,
                    Value: 1,
                    Scattered: false
                ),
                new(
                    Addr: 0xe,
                    Type: (uint8)(nint)GENERIC_RELOC_LOCAL_SECTDIFF,
                    Len: 2,
                    Pcrel: false,
                    Value: 0x2d,
                    Scattered: true
                ),
                new(
                    Addr: 0x0,
                    Type: (uint8)(nint)GENERIC_RELOC_PAIR,
                    Len: 2,
                    Pcrel: false,
                    Value: 0xb,
                    Scattered: true
                )}.slice()
        }
    ),
    new(
        "testdata/clang-amd64-darwin.obj.base64"u8,
        new FileHeader(0xfeedfacfU, CpuAmd64, 0x3, 0x1, 0x4, 0x200, 0x2000),
        default!,
        default!,
        new map<@string, slice<global::go.debug.macho_package.Reloc>>{
            ["__text"u8] = new global::go.debug.macho_package.Reloc[]{
                new(
                    Addr: 0x19,
                    Type: (uint8)(nint)X86_64_RELOC_BRANCH,
                    Len: 2,
                    Pcrel: true,
                    Extern: true,
                    Value: 1
                ),
                new(
                    Addr: 0xb,
                    Type: (uint8)(nint)X86_64_RELOC_SIGNED,
                    Len: 2,
                    Pcrel: true,
                    Extern: false,
                    Value: 2
                )}.slice(),
            ["__compact_unwind"u8] = new global::go.debug.macho_package.Reloc[]{
                new(
                    Addr: 0x0,
                    Type: (uint8)(nint)X86_64_RELOC_UNSIGNED,
                    Len: 3,
                    Pcrel: false,
                    Extern: false,
                    Value: 1
                )}.slice()
        }
    )
}.slice());
internal static ref slice<fileTest> fileTests => ref ᏑfileTests.ValueSlot;

internal static (io.ReaderAt, error) readerAtFromObscured(@string name) {
    var (b, err) = obscuretestdata.ReadFile(name);
    if (err != default!) {
        return (default!, err);
    }
    return (new macho_internal_test_package.bytes_ReaderжReaderAt(bytes.NewReader(b)), default!);
}

internal static (ж<global::go.debug.macho_package.File>, error) openObscured(@string name) {
    var (ra, err) = readerAtFromObscured(name);
    if (err != default!) {
        return (default!, err);
    }
    (var ff, err) = NewFile(ra);
    if (err != default!) {
        return (default!, err);
    }
    return (ff, default!);
}

internal static (ж<global::go.debug.macho_package.FatFile>, error) openFatObscured(@string name) {
    var (ra, err) = readerAtFromObscured(name);
    if (err != default!) {
        return (default!, err);
    }
    (var ff, err) = NewFatFile(ra);
    if (err != default!) {
        return (default!, err);
    }
    return (ff, default!);
}

public static void TestOpen(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (i, _) in fileTests) {
        var tt = Ꮡ(fileTests, i);
        // Use obscured files to prevent Apple’s notarization service from
        // mistaking them as candidates for notarization and rejecting the entire
        // toolchain.
        // See golang.org/issue/34986
        var (f, err) = openObscured((~tt).@file);
        if (err != default!) {
            Ꮡt.Error(err);
            continue;
        }
        if (!reflect.DeepEqual((~f).FileHeader, (~tt).hdr)) {
            Ꮡt.Errorf("open %s:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, (~f).FileHeader, (~tt).hdr);
            continue;
        }
        foreach (var (iΔ1, l) in (~f).Loads) {
            if (len(l.Raw()) < 8) {
                Ꮡt.Errorf("open %s, command %d:\n\tload command %T don't have enough data\n"u8, (~tt).@file, iΔ1, l);
            }
        }
        if ((~tt).loads != default!) {
            foreach (var (iΔ2, l) in (~f).Loads) {
                if (iΔ2 >= len((~tt).loads)) {
                    break;
                }
                var want = (~tt).loads[iΔ2];
                if (want == default!) {
                    continue;
                }
                switch (l.type()) {
                case ж<global::go.debug.macho_package.ΔSegment> lΔ1: {
                    var have = lΔ1.of(global::go.debug.macho_package.ΔSegment.ᏑSegmentHeader);
                    if (!reflect.DeepEqual(have.OrTypedNil(), want)) {
                        Ꮡt.Errorf("open %s, command %d:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, iΔ2, have.OrTypedNil(), want);
                    }
                    break;
                }
                case ж<global::go.debug.macho_package.Dylib> lΔ1: {
                    var have = lΔ1;
                    have.Value.LoadBytes = default!;
                    if (!reflect.DeepEqual(have.OrTypedNil(), want)) {
                        Ꮡt.Errorf("open %s, command %d:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, iΔ2, have.OrTypedNil(), want);
                    }
                    break;
                }
                case ж<global::go.debug.macho_package.Rpath> lΔ1: {
                    var have = lΔ1;
                    have.Value.LoadBytes = default!;
                    if (!reflect.DeepEqual(have.OrTypedNil(), want)) {
                        Ꮡt.Errorf("open %s, command %d:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, iΔ2, have.OrTypedNil(), want);
                    }
                    break;
                }
                default: {
                    var lΔ1 = l;
                    Ꮡt.Errorf("open %s, command %d: unknown load command\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, iΔ2, lΔ1, want);
                    break;
                }}
            }
            nint tn = len((~tt).loads);
            nint fn = len((~f).Loads);
            if (tn != fn) {
                Ꮡt.Errorf("open %s: len(Loads) = %d, want %d"u8, (~tt).@file, fn, tn);
            }
        }
        if ((~tt).sections != default!) {
            foreach (var (iΔ3, sh) in (~f).Sections) {
                if (iΔ3 >= len((~tt).sections)) {
                    break;
                }
                var have = sh.of(global::go.debug.macho_package.ΔSection.ᏑSectionHeader);
                var want = (~tt).sections[iΔ3];
                if (!reflect.DeepEqual(have.OrTypedNil(), want.OrTypedNil())) {
                    Ꮡt.Errorf("open %s, section %d:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, iΔ3, have.OrTypedNil(), want.OrTypedNil());
                }
            }
            nint tn = len((~tt).sections);
            nint fn = len((~f).Sections);
            if (tn != fn) {
                Ꮡt.Errorf("open %s: len(Sections) = %d, want %d"u8, (~tt).@file, fn, tn);
            }
        }
        if ((~tt).relocations != default!) {
            foreach (var (iΔ4, sh) in (~f).Sections) {
                var have = sh.Value.Relocs;
                var want = (~tt).relocations[(~sh).Name];
                if (!reflect.DeepEqual(have, want)) {
                    Ꮡt.Errorf("open %s, relocations in section %d (%s):\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, iΔ4, (~sh).Name, have, want);
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileGoˢ = "file.go"u8;

public static void TestOpenFailure(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string filename = fileGoˢ; // not a Mach-O file
    var (_, err) = Open(filename); // don't crash
    if (err == default!) {
        Ꮡt.Errorf("open %s: succeeded unexpectedly"u8, filename);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataFatGcc386Amd64ˢ = "testdata/fat-gcc-386-amd64-darwin-exec.base64"u8;

public static void TestOpenFat(ж<testing.T> Ꮡt) {
    var (ff, err) = openFatObscured(testdataFatGcc386Amd64ˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((~ff).Magic != MagicFat) {
        Ꮡt.Errorf("OpenFat: got magic number %#x, want %#x"u8, (~ff).Magic, MagicFat);
    }
    if (len((~ff).Arches) != 2) {
        Ꮡt.Errorf("OpenFat: got %d architectures, want 2"u8, len((~ff).Arches));
    }
    foreach (var (i, _) in (~ff).Arches) {
        var arch = Ꮡ((~ff).Arches, i);
        var ftArch = Ꮡ(fileTests, i);
        if ((~arch).Cpu != (~ftArch).hdr.Cpu || (~arch).SubCpu != (~ftArch).hdr.SubCpu) {
            Ꮡt.Errorf("OpenFat: architecture #%d got cpu=%#x subtype=%#x, expected cpu=%#x, subtype=%#x"u8, i, (~arch).Cpu, (~arch).SubCpu, (~ftArch).hdr.Cpu, (~ftArch).hdr.SubCpu);
        }
        if (!reflect.DeepEqual((~arch).FileHeader, (~ftArch).hdr)) {
            Ꮡt.Errorf("OpenFat header:\n\tgot %#v\n\twant %#v\n"u8, (~arch).FileHeader, (~ftArch).hdr);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataGcc386DarwinExecˢ = "testdata/gcc-386-darwin-exec.base64"u8;

public static void TestOpenFatFailure(ж<testing.T> Ꮡt) {
    @string filename = fileGoˢ; // not a Mach-O file
    {
        var (_, errΔ1) = OpenFat(filename); if (errΔ1 == default!) {
            Ꮡt.Errorf("OpenFat %s: succeeded unexpectedly"u8, filename);
        }
    }
    filename = testdataGcc386DarwinExecˢ; // not a fat Mach-O
    var (ff, err) = openFatObscured(filename);
    if (!AreEqual(err, ErrNotFat)) {
        Ꮡt.Errorf("OpenFat %s: got %v, want ErrNotFat"u8, filename, err);
    }
    if (ff != nil) {
        Ꮡt.Errorf("OpenFat %s: got %v, want nil"u8, filename, ff.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object x8664RelocBranchˢ = (@string)"X86_64_RELOC_BRANCH"u8;
internal static readonly object machoX8664RelocBranchˢ = (@string)"macho.X86_64_RELOC_BRANCH"u8;

public static void TestRelocTypeString(ж<testing.T> Ꮡt) {
    if (X86_64_RELOC_BRANCH.String() != "X86_64_RELOC_BRANCH"u8) {
        Ꮡt.Errorf("got %v, want %v"u8, X86_64_RELOC_BRANCH.String(), x8664RelocBranchˢ);
    }
    if (X86_64_RELOC_BRANCH.GoString() != "macho.X86_64_RELOC_BRANCH"u8) {
        Ꮡt.Errorf("got %v, want %v"u8, X86_64_RELOC_BRANCH.GoString(), machoX8664RelocBranchˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object execˢ = (@string)"Exec"u8;
internal static readonly object machoExecˢ = (@string)"macho.Exec"u8;

public static void TestTypeString(ж<testing.T> Ꮡt) {
    if (TypeExec.String() != "Exec"u8) {
        Ꮡt.Errorf("got %v, want %v"u8, TypeExec.String(), execˢ);
    }
    if (TypeExec.GoString() != "macho.Exec"u8) {
        Ꮡt.Errorf("got %v, want %v"u8, TypeExec.GoString(), machoExecˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataGccAmd64Darwinˢ = "testdata/gcc-amd64-darwin-exec-with-bad-dysym.base64"u8;
internal static readonly object openObscuredDidNotFailˢ = (@string)"openObscured did not fail when opening a file with an invalid dynamic symbol table command"u8;

public static void TestOpenBadDysymCmd(ж<testing.T> Ꮡt) {
    var (_, err) = openObscured(testdataGccAmd64Darwinˢ);
    if (err == default!) {
        Ꮡt.Fatal(openObscuredDidNotFailˢ);
    }
}

} // end macho_internal_test_package

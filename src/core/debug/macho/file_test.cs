// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using bytes = bytes_package;
using obscuretestdata = @internal.obscuretestdata_package;
using io = io_package;
using reflect = reflect_package;
using slices = slices_package;
using testing = testing_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.debug.macho_package;

partial class macho_internal_test_package {

internal partial struct fileTest {
    internal @string @file;
    internal global::go.debug.macho_package.FileHeader hdr;
    internal slice<any> loads;
    internal slice<ж<global::go.debug.macho_package.SectionHeader>> sections;
    internal map<@string, slice<global::go.debug.macho_package.Reloc>> relocations;
    internal slice<@string> importedSyms;
}

internal static readonly ж<slice<fileTest>> ᏑfileTests = new StaticBox<slice<fileTest>>(slice<fileTest>([
    new(
        "testdata/gcc-386-darwin-exec.base64",
        new FileHeader(0xfeedfaceU, Cpu386, 0x3, 0x2, 0xc, 0x3c0, 0x85),
        slice<any>([
            Ꮡ(new SegmentHeader(LoadCmdSegment, 0x38, "__PAGEZERO", 0x0, 0x1000, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment, 0xc0, "__TEXT", 0x1000, 0x1000, 0x0, 0x1000, 0x7, 0x5, 0x2, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment, 0xc0, "__DATA", 0x2000, 0x1000, 0x1000, 0x1000, 0x7, 0x3, 0x2, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment, 0x7c, "__IMPORT", 0x3000, 0x1000, 0x2000, 0x1000, 0x7, 0x7, 0x1, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment, 0x38, "__LINKEDIT", 0x4000, 0x1000, 0x3000, 0x12c, 0x7, 0x1, 0x0, 0x0)),
            default, // LC_SYMTAB
            default, // LC_DYSYMTAB
            default, // LC_LOAD_DYLINKER
            default, // LC_UUID
            default, // LC_UNIXTHREAD
            Ꮡ(new Dylib(default, "/usr/lib/libgcc_s.1.dylib", 0x2, 0x10000, 0x10000)),
            Ꮡ(new Dylib(default, "/usr/lib/libSystem.B.dylib", 0x2, 0x6f0104, 0x10000))
        ]),
        slice<ж<global::go.debug.macho_package.SectionHeader>>([
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__text", "__TEXT", 0x1f68, 0x88, 0xf68, 0x2, 0x0, 0x0, 0x80000400U)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__cstring", "__TEXT", 0x1ff0, 0xd, 0xff0, 0x0, 0x0, 0x0, 0x2)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__data", "__DATA", 0x2000, 0x14, 0x1000, 0x2, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__dyld", "__DATA", 0x2014, 0x1c, 0x1014, 0x2, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__jump_table", "__IMPORT", 0x3000, 0xa, 0x2000, 0x6, 0x0, 0x0, 0x4000008))
        ]),
        default,
        default
    ),
    new(
        "testdata/gcc-amd64-darwin-exec.base64",
        new FileHeader(0xfeedfacfU, CpuAmd64, 0x80000003U, 0x2, 0xb, 0x568, 0x85),
        slice<any>([
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x48, "__PAGEZERO", 0x0, 0x100000000UL, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x1d8, "__TEXT", 0x100000000UL, 0x1000, 0x0, 0x1000, 0x7, 0x5, 0x5, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x138, "__DATA", 0x100001000UL, 0x1000, 0x1000, 0x1000, 0x7, 0x3, 0x3, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x48, "__LINKEDIT", 0x100002000UL, 0x1000, 0x2000, 0x140, 0x7, 0x1, 0x0, 0x0)),
            default, // LC_SYMTAB
            default, // LC_DYSYMTAB
            default, // LC_LOAD_DYLINKER
            default, // LC_UUID
            default, // LC_UNIXTHREAD
            Ꮡ(new Dylib(default, "/usr/lib/libgcc_s.1.dylib", 0x2, 0x10000, 0x10000)),
            Ꮡ(new Dylib(default, "/usr/lib/libSystem.B.dylib", 0x2, 0x6f0104, 0x10000))
        ]),
        slice<ж<global::go.debug.macho_package.SectionHeader>>([
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__text", "__TEXT", 0x100000f14UL, 0x6d, 0xf14, 0x2, 0x0, 0x0, 0x80000400U)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__symbol_stub1", "__TEXT", 0x100000f81UL, 0xc, 0xf81, 0x0, 0x0, 0x0, 0x80000408U)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__stub_helper", "__TEXT", 0x100000f90UL, 0x18, 0xf90, 0x2, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__cstring", "__TEXT", 0x100000fa8UL, 0xd, 0xfa8, 0x0, 0x0, 0x0, 0x2)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__eh_frame", "__TEXT", 0x100000fb8UL, 0x48, 0xfb8, 0x3, 0x0, 0x0, 0x6000000b)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__data", "__DATA", 0x100001000UL, 0x1c, 0x1000, 0x3, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__dyld", "__DATA", 0x100001020UL, 0x38, 0x1020, 0x3, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__la_symbol_ptr", "__DATA", 0x100001058UL, 0x10, 0x1058, 0x2, 0x0, 0x0, 0x7))
        ]),
        default,
        default
    ),
    new(
        "testdata/gcc-amd64-darwin-exec-debug.base64",
        new FileHeader(0xfeedfacfU, CpuAmd64, 0x80000003U, 0xa, 0x4, 0x5a0, 0),
        slice<any>([
            default, // LC_UUID
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x1d8, "__TEXT", 0x100000000UL, 0x1000, 0x0, 0x0, 0x7, 0x5, 0x5, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x138, "__DATA", 0x100001000UL, 0x1000, 0x0, 0x0, 0x7, 0x3, 0x3, 0x0)),
            Ꮡ(new SegmentHeader(LoadCmdSegment64, 0x278, "__DWARF", 0x100002000UL, 0x1000, 0x1000, 0x1bc, 0x7, 0x3, 0x7, 0x0))
        ]),
        slice<ж<global::go.debug.macho_package.SectionHeader>>([
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__text", "__TEXT", 0x100000f14UL, 0x0, 0x0, 0x2, 0x0, 0x0, 0x80000400U)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__symbol_stub1", "__TEXT", 0x100000f81UL, 0x0, 0x0, 0x0, 0x0, 0x0, 0x80000408U)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__stub_helper", "__TEXT", 0x100000f90UL, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__cstring", "__TEXT", 0x100000fa8UL, 0x0, 0x0, 0x0, 0x0, 0x0, 0x2)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__eh_frame", "__TEXT", 0x100000fb8UL, 0x0, 0x0, 0x3, 0x0, 0x0, 0x6000000b)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__data", "__DATA", 0x100001000UL, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__dyld", "__DATA", 0x100001020UL, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__la_symbol_ptr", "__DATA", 0x100001058UL, 0x0, 0x0, 0x2, 0x0, 0x0, 0x7)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_abbrev", "__DWARF", 0x100002000UL, 0x36, 0x1000, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_aranges", "__DWARF", 0x100002036UL, 0x30, 0x1036, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_frame", "__DWARF", 0x100002066UL, 0x40, 0x1066, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_info", "__DWARF", 0x1000020a6UL, 0x54, 0x10a6, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_line", "__DWARF", 0x1000020faUL, 0x47, 0x10fa, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_pubnames", "__DWARF", 0x100002141UL, 0x1b, 0x1141, 0x0, 0x0, 0x0, 0x0)),
            Ꮡ(new global::go.debug.macho_package.SectionHeader("__debug_str", "__DWARF", 0x10000215cUL, 0x60, 0x115c, 0x0, 0x0, 0x0, 0x0))
        ]),
        default,
        default
    ),
    new(
        "testdata/clang-386-darwin-exec-with-rpath.base64",
        new FileHeader(0xfeedfaceU, Cpu386, 0x3, 0x2, 0x10, 0x42c, 0x1200085),
        slice<any>([
            default, // LC_SEGMENT
            default, // LC_SEGMENT
            default, // LC_SEGMENT
            default, // LC_SEGMENT
            default, // LC_DYLD_INFO_ONLY
            default, // LC_SYMTAB
            default, // LC_DYSYMTAB
            default, // LC_LOAD_DYLINKER
            default, // LC_UUID
            default, // LC_VERSION_MIN_MACOSX
            default, // LC_SOURCE_VERSION
            default, // LC_MAIN
            default, // LC_LOAD_DYLIB
            Ꮡ(new Rpath(default, "/my/rpath")),
            default, // LC_FUNCTION_STARTS
            default  // LC_DATA_IN_CODE
        ]),
        default,
        default,
        default
    ),
    new(
        "testdata/clang-amd64-darwin-exec-with-rpath.base64",
        new FileHeader(0xfeedfacfU, CpuAmd64, 0x80000003U, 0x2, 0x10, 0x4c8, 0x200085),
        slice<any>([
            default, // LC_SEGMENT
            default, // LC_SEGMENT
            default, // LC_SEGMENT
            default, // LC_SEGMENT
            default, // LC_DYLD_INFO_ONLY
            default, // LC_SYMTAB
            default, // LC_DYSYMTAB
            default, // LC_LOAD_DYLINKER
            default, // LC_UUID
            default, // LC_VERSION_MIN_MACOSX
            default, // LC_SOURCE_VERSION
            default, // LC_MAIN
            default, // LC_LOAD_DYLIB
            Ꮡ(new Rpath(default, "/my/rpath")),
            default, // LC_FUNCTION_STARTS
            default  // LC_DATA_IN_CODE
        ]),
        default,
        default,
        default
    ),
    new(
        "testdata/clang-386-darwin.obj.base64",
        new FileHeader(0xfeedfaceU, Cpu386, 0x3, 0x1, 0x4, 0x138, 0x2000),
        default,
        default,
        new map<@string, slice<global::go.debug.macho_package.Reloc>>{
            ["__text"] = slice<global::go.debug.macho_package.Reloc>([
                new(
                    Addr: 0x1d,
                    Type: (uint8)GENERIC_RELOC_VANILLA,
                    Len: 2,
                    Pcrel: true,
                    Extern: true,
                    Value: 1,
                    Scattered: false
                ),
                new(
                    Addr: 0xe,
                    Type: (uint8)GENERIC_RELOC_LOCAL_SECTDIFF,
                    Len: 2,
                    Pcrel: false,
                    Value: 0x2d,
                    Scattered: true
                ),
                new(
                    Addr: 0x0,
                    Type: (uint8)GENERIC_RELOC_PAIR,
                    Len: 2,
                    Pcrel: false,
                    Value: 0xb,
                    Scattered: true
                )])
        },
        default
    ),
    new(
        "testdata/clang-amd64-darwin.obj.base64",
        new FileHeader(0xfeedfacfU, CpuAmd64, 0x3, 0x1, 0x4, 0x200, 0x2000),
        default,
        default,
        new map<@string, slice<global::go.debug.macho_package.Reloc>>{
            ["__text"] = slice<global::go.debug.macho_package.Reloc>([
                new(
                    Addr: 0x19,
                    Type: (uint8)X86_64_RELOC_BRANCH,
                    Len: 2,
                    Pcrel: true,
                    Extern: true,
                    Value: 1
                ),
                new(
                    Addr: 0xb,
                    Type: (uint8)X86_64_RELOC_SIGNED,
                    Len: 2,
                    Pcrel: true,
                    Extern: false,
                    Value: 2
                )]),
            ["__compact_unwind"] = slice<global::go.debug.macho_package.Reloc>([
                new(
                    Addr: 0x0,
                    Type: (uint8)X86_64_RELOC_UNSIGNED,
                    Len: 3,
                    Pcrel: false,
                    Extern: false,
                    Value: 1
                )])
        },
        slice<@string>(["_printf"])
    ),
    new(
        "testdata/clang-amd64-darwin-ld-r.obj.base64",
        new FileHeader(0xfeedfacfU, CpuAmd64, 0x3, 0x1, 0x4, 0x1c0, 0x2000),
        default,
        default,
        default,
        slice<@string>(["_printf"])
    )
]));
internal static ref slice<fileTest> fileTests => ref ((StaticBox<slice<fileTest>>)ᏑfileTests).ValueSlot;

internal static (io.ReaderAt, error) readerAtFromObscured(@string name) {
    var (b, err) = obscuretestdata.ReadFile(name);
    if (err != nil) {
        return (default, err);
    }
    return (new macho_internal_test_package.bytes_ReaderжReaderAt(bytes.NewReader(b)), default);
}

internal static (ж<global::go.debug.macho_package.File>, error) openObscured(@string name) {
    var (ra, err) = readerAtFromObscured(name);
    if (err != nil) {
        return (default, err);
    }
    (var ff, err) = NewFile(ra);
    if (err != nil) {
        return (default, err);
    }
    return (ff, default);
}

internal static (ж<global::go.debug.macho_package.FatFile>, error) openFatObscured(@string name) {
    var (ra, err) = readerAtFromObscured(name);
    if (err != nil) {
        return (default, err);
    }
    (var ff, err) = NewFatFile(ra);
    if (err != nil) {
        return (default, err);
    }
    return (ff, default);
}

public static void TestOpen(ж<testing.T> t) {
    foreach (var i in range(fileTests)) {
        var tt = Ꮡ(fileTests, i);

        // Use obscured files to prevent Apple’s notarization service from
        // mistaking them as candidates for notarization and rejecting the entire
        // toolchain.
        // See golang.org/issue/34986
        var (f, err) = openObscured(tt.@file);
        if (err != nil) {
            t.Error(err);
            continue;
        }
        if (!reflect.DeepEqual(f.FileHeader, tt.hdr)) {
            t.Errorf("open %s:\n\thave %#v\n\twant %#v\n", tt.@file, f.FileHeader, tt.hdr);
            continue;
        }
        foreach (var (iΔ1, l) in f.Loads) {
            if (len(l.Raw()) < 8) {
                t.Errorf("open %s, command %d:\n\tload command %T don't have enough data\n", tt.@file, iΔ1, l);
            }
        }
        if (tt.loads != nil) {
            foreach (var (iΔ2, l) in f.Loads) {
                if (iΔ2 >= len(tt.loads)) {
                    break;
                }

                var want = tt.loads[iΔ2];
                if (want == nil) {
                    continue;
                }

                switch (l.type()) {
                case ж<global::go.debug.macho_package.ΔSegment> lΔ1: {
                    var have = lΔ1.ᏑSegmentHeader;
                    if (!reflect.DeepEqual(have.OrTypedNil(), want)) {
                        t.Errorf("open %s, command %d:\n\thave %#v\n\twant %#v\n", tt.@file, iΔ2, have.OrTypedNil(), want);
                    }
                    break;
                }
                case ж<global::go.debug.macho_package.Dylib> lΔ1: {
                    var have = lΔ1;
                    have.LoadBytes = default;
                    if (!reflect.DeepEqual(have.OrTypedNil(), want)) {
                        t.Errorf("open %s, command %d:\n\thave %#v\n\twant %#v\n", tt.@file, iΔ2, have.OrTypedNil(), want);
                    }
                    break;
                }
                case ж<global::go.debug.macho_package.Rpath> lΔ1: {
                    var have = lΔ1;
                    have.LoadBytes = default;
                    if (!reflect.DeepEqual(have.OrTypedNil(), want)) {
                        t.Errorf("open %s, command %d:\n\thave %#v\n\twant %#v\n", tt.@file, iΔ2, have.OrTypedNil(), want);
                    }
                    break;
                }
                default: {
                    var lΔ1 = l;
                    t.Errorf("open %s, command %d: unknown load command\n\thave %#v\n\twant %#v\n", tt.@file, iΔ2, lΔ1, want);
                    break;
                }}
            }
            nint tn = len(tt.loads);
            nint fn = len(f.Loads);
            if (tn != fn) {
                t.Errorf("open %s: len(Loads) = %d, want %d", tt.@file, fn, tn);
            }
        }

        if (tt.sections != nil) {
            foreach (var (iΔ3, sh) in f.Sections) {
                if (iΔ3 >= len(tt.sections)) {
                    break;
                }
                var have = sh.ᏑSectionHeader;
                var want = tt.sections[iΔ3];
                if (!reflect.DeepEqual(have.OrTypedNil(), want.OrTypedNil())) {
                    t.Errorf("open %s, section %d:\n\thave %#v\n\twant %#v\n", tt.@file, iΔ3, have.OrTypedNil(), want.OrTypedNil());
                }
            }
            nint tn = len(tt.sections);
            nint fn = len(f.Sections);
            if (tn != fn) {
                t.Errorf("open %s: len(Sections) = %d, want %d", tt.@file, fn, tn);
            }
        }

        if (tt.relocations != nil) {
            foreach (var (iΔ4, sh) in f.Sections) {
                var have = sh.Relocs;
                var want = tt.relocations[sh.Name];
                if (!reflect.DeepEqual(have, want)) {
                    t.Errorf("open %s, relocations in section %d (%s):\n\thave %#v\n\twant %#v\n", tt.@file, iΔ4, sh.Name, have, want);
                }
            }
        }

        if (tt.importedSyms != nil) {
            var (ss, errΔ1) = f.ImportedSymbols();
            if (errΔ1 != nil) {
                t.Errorf("open %s: fail to read imported symbols: %v", tt.@file, errΔ1);
            }
            var want = tt.importedSyms;
            if (!slices.Equal<slice<@string>, @string>(ss, want)) {
                t.Errorf("open %s: imported symbols differ:\n\thave %v\n\twant %v", tt.@file, ss, want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fileGoˢ => ref fileGoˢᵇ;
private static readonly @string fileGoˢᵇ = "file.go"u8;

public static void TestOpenFailure(ж<testing.T> t) {
    @string filename = fileGoˢ;    // not a Mach-O file
    var (_, err) = Open(filename); // don't crash
    if (err == nil) {
        t.Errorf("open %s: succeeded unexpectedly", filename);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataFatGcc386Amd64ˢ => ref testdataFatGcc386Amd64ˢᵇ;
private static readonly @string testdataFatGcc386Amd64ˢᵇ = "testdata/fat-gcc-386-amd64-darwin-exec.base64"u8;

public static void TestOpenFat(ж<testing.T> t) {
    var (ff, err) = openFatObscured(testdataFatGcc386Amd64ˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    if (ff.Magic != MagicFat) {
        t.Errorf("OpenFat: got magic number %#x, want %#x", ff.Magic, MagicFat);
    }
    if (len(ff.Arches) != 2) {
        t.Errorf("OpenFat: got %d architectures, want 2", len(ff.Arches));
    }

    foreach (var i in range(ff.Arches)) {
        var arch = Ꮡ(ff.Arches, i);
        var ftArch = Ꮡ(fileTests, i);

        if (arch.Cpu != ftArch.hdr.Cpu || arch.SubCpu != ftArch.hdr.SubCpu) {
            t.Errorf("OpenFat: architecture #%d got cpu=%#x subtype=%#x, expected cpu=%#x, subtype=%#x", i, arch.Cpu, arch.SubCpu, ftArch.hdr.Cpu, ftArch.hdr.SubCpu);
        }

        if (!reflect.DeepEqual(arch.FileHeader, ftArch.hdr)) {
            t.Errorf("OpenFat header:\n\tgot %#v\n\twant %#v\n", arch.FileHeader, ftArch.hdr);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataGcc386DarwinExecˢ => ref testdataGcc386DarwinExecˢᵇ;
private static readonly @string testdataGcc386DarwinExecˢᵇ = "testdata/gcc-386-darwin-exec.base64"u8;

public static void TestOpenFatFailure(ж<testing.T> t) {
    @string filename = fileGoˢ; // not a Mach-O file
    {
        var (_, errΔ1) = OpenFat(filename); if (errΔ1 == nil) {
            t.Errorf("OpenFat %s: succeeded unexpectedly", filename);
        }
    }

    filename = testdataGcc386DarwinExecˢ; // not a fat Mach-O
    var (ff, err) = openFatObscured(filename);
    if (!AreEqual(err, ErrNotFat)) {
        t.Errorf("OpenFat %s: got %v, want ErrNotFat", filename, err);
    }
    if (ff != nil) {
        t.Errorf("OpenFat %s: got %v, want nil", filename, ff.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object x8664RelocBranchˢ = (@string)"X86_64_RELOC_BRANCH"u8;
internal static readonly object machoX8664RelocBranchˢ = (@string)"macho.X86_64_RELOC_BRANCH"u8;

public static void TestRelocTypeString(ж<testing.T> t) {
    if (X86_64_RELOC_BRANCH.String() != "X86_64_RELOC_BRANCH"u8) {
        t.Errorf("got %v, want %v", X86_64_RELOC_BRANCH.String(), x8664RelocBranchˢ);
    }
    if (X86_64_RELOC_BRANCH.GoString() != "macho.X86_64_RELOC_BRANCH"u8) {
        t.Errorf("got %v, want %v", X86_64_RELOC_BRANCH.GoString(), machoX8664RelocBranchˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object execˢ = (@string)"Exec"u8;
internal static readonly object machoExecˢ = (@string)"macho.Exec"u8;

public static void TestTypeString(ж<testing.T> t) {
    if (TypeExec.String() != "Exec"u8) {
        t.Errorf("got %v, want %v", TypeExec.String(), execˢ);
    }
    if (TypeExec.GoString() != "macho.Exec"u8) {
        t.Errorf("got %v, want %v", TypeExec.GoString(), machoExecˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataGccAmd64Darwinˢ => ref testdataGccAmd64Darwinˢᵇ;
private static readonly @string testdataGccAmd64Darwinˢᵇ = "testdata/gcc-amd64-darwin-exec-with-bad-dysym.base64"u8;
internal static readonly object openObscuredDidNotFailˢ = (@string)"openObscured did not fail when opening a file with an invalid dynamic symbol table command"u8;

public static partial void TestOpenBadDysymCmd(ж<testing.T> t) {
    var (_, err) = openObscured(testdataGccAmd64Darwinˢ);
    if (err == nil) {
        t.Fatal(openObscuredDidNotFailˢ);
    }
}

} // end macho_internal_test_package

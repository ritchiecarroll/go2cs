// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using bytes = bytes_package;
using dwarf = go.debug.dwarf_package;
using testenv = @internal.testenv_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using reflect = reflect_package;
using regexp = regexp_package;
using runtime = runtime_package;
using strconv = strconv_package;
using testing = testing_package;
using template = text.template_package;
using @internal;
using fs = go.io.fs_package;
using go.debug;
using go.os;
using io = io_package;
using path;
using static go.debug.pe_package;
using text;

partial class pe_internal_test_package {

[GoType] internal partial struct fileTest {
    internal @string @file;
    internal global::go.debug.pe_package.FileHeader hdr;
    internal any opthdr;
    internal slice<ж<global::go.debug.pe_package.SectionHeader>> sections;
    internal slice<ж<global::go.debug.pe_package.Symbol>> symbols;
    internal bool hasNoDwarfInfo;
}

// testdata/vmlinuz-4.15.0-47-generic is a trimmed down version of Linux Kernel image.
// The original Linux Kernel image is about 8M and it is not recommended to add such a big binary file to the repo.
// Moreover only a very small portion of the original Kernel image was being parsed by debug/pe package.
// In order to identify this portion, the original image was first parsed by modified debug/pe package.
// Modification essentially communicated reader's positions before and after parsing.
// Finally, bytes between those positions where written to a separate file,
// generating trimmed down version Linux Kernel image used in this test case.
internal static ж<slice<fileTest>> ᏑfileTests = new StandardBox<slice<fileTest>>(new fileTest[]{
    new(
        @file: "testdata/gcc-386-mingw-obj"u8,
        hdr: new FileHeader(0x014c, 0x000c, 0x0, 0x64a, 0x1e, 0x0, 0x104),
        sections: new ж<global::go.debug.pe_package.SectionHeader>[]{
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".text"u8, 0, 0, 36, 500, 1440, 0, 3, 0, 0x60300020)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".data"u8, 0, 0, 0, 0, 0, 0, 0, 0, 3224371264U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".bss"u8, 0, 0, 0, 0, 0, 0, 0, 0, 3224371328U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_abbrev"u8, 0, 0, 137, 536, 0, 0, 0, 0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_info"u8, 0, 0, 418, 673, 1470, 0, 7, 0, 1108344832)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_line"u8, 0, 0, 128, 1091, 1540, 0, 1, 0, 1108344832)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".rdata"u8, 0, 0, 16, 1219, 0, 0, 0, 0, 1076887616)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_frame"u8, 0, 0, 52, 1235, 1550, 0, 2, 0, 1110441984)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_loc"u8, 0, 0, 56, 1287, 0, 0, 0, 0, 1108344832)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_pubnames"u8, 0, 0, 27, 1343, 1570, 0, 1, 0, 1108344832)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_pubtypes"u8, 0, 0, 38, 1370, 1580, 0, 1, 0, 1108344832)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_aranges"u8, 0, 0, 32, 1408, 1590, 0, 2, 0, 1108344832))
        }.slice(),
        symbols: new ж<global::go.debug.pe_package.Symbol>[]{
            Ꮡ(new global::go.debug.pe_package.Symbol(".file"u8, 0x0, -2, 0x0, 0x67)),
            Ꮡ(new global::go.debug.pe_package.Symbol("_main"u8, 0x0, 1, 0x20, 0x2)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".text"u8, 0x0, 1, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".data"u8, 0x0, 2, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".bss"u8, 0x0, 3, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_abbrev"u8, 0x0, 4, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_info"u8, 0x0, 5, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_line"u8, 0x0, 6, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".rdata"u8, 0x0, 7, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_frame"u8, 0x0, 8, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_loc"u8, 0x0, 9, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_pubnames"u8, 0x0, 10, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_pubtypes"u8, 0x0, 11, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_aranges"u8, 0x0, 12, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol("___main"u8, 0x0, 0, 0x20, 0x2)),
            Ꮡ(new global::go.debug.pe_package.Symbol("_puts"u8, 0x0, 0, 0x20, 0x2))
        }.slice()
    ),
    new(
        @file: "testdata/gcc-386-mingw-exec"u8,
        hdr: new FileHeader(0x014c, 0x000f, 0x4c6a1b60, 0x3c00, 0x282, 0xe0, 0x107),
        opthdr: Ꮡ(new OptionalHeader32(
            0x10b, 0x2, 0x38, 0xe00, 0x1a00, 0x200, 0x1160, 0x1000, 0x2000, 0x400000, 0x1000, 0x200, 0x4, 0x0, 0x1, 0x0, 0x4, 0x0, 0x0, 0x10000, 0x400, 0x14abb, 0x3, 0x0, 0x200000, 0x1000, 0x100000, 0x1000, 0x0, 0x10,
            new global::go.debug.pe_package.DataDirectory[]{
                new(0x0, 0x0),
                new(0x5000, 0x3c8),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x7000, 0x18),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0)
            }.array()
        )),
        sections: new ж<global::go.debug.pe_package.SectionHeader>[]{
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".text"u8, 0xcd8, 0x1000, 0xe00, 0x400, 0x0, 0x0, 0x0, 0x0, 0x60500060)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".data"u8, 0x10, 0x2000, 0x200, 0x1200, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".rdata"u8, 0x120, 0x3000, 0x200, 0x1400, 0x0, 0x0, 0x0, 0x0, 0x40300040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".bss"u8, 0xdc, 0x4000, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc0400080U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".idata"u8, 0x3c8, 0x5000, 0x400, 0x1600, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".CRT"u8, 0x18, 0x6000, 0x200, 0x1a00, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".tls"u8, 0x20, 0x7000, 0x200, 0x1c00, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_aranges"u8, 0x20, 0x8000, 0x200, 0x1e00, 0x0, 0x0, 0x0, 0x0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_pubnames"u8, 0x51, 0x9000, 0x200, 0x2000, 0x0, 0x0, 0x0, 0x0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_pubtypes"u8, 0x91, 0xa000, 0x200, 0x2200, 0x0, 0x0, 0x0, 0x0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_info"u8, 0xe22, 0xb000, 0x1000, 0x2400, 0x0, 0x0, 0x0, 0x0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_abbrev"u8, 0x157, 0xc000, 0x200, 0x3400, 0x0, 0x0, 0x0, 0x0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_line"u8, 0x144, 0xd000, 0x200, 0x3600, 0x0, 0x0, 0x0, 0x0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_frame"u8, 0x34, 0xe000, 0x200, 0x3800, 0x0, 0x0, 0x0, 0x0, 0x42300000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_loc"u8, 0x38, 0xf000, 0x200, 0x3a00, 0x0, 0x0, 0x0, 0x0, 0x42100000))
        }.slice()
    ),
    new(
        @file: "testdata/gcc-386-mingw-no-symbols-exec"u8,
        hdr: new FileHeader(0x14c, 0x8, 0x69676572, 0x0, 0x0, 0xe0, 0x30f),
        opthdr: Ꮡ(new OptionalHeader32(0x10b, 0x2, 0x18, 0xe00, 0x1e00, 0x200, 0x1280, 0x1000, 0x2000, 0x400000, 0x1000, 0x200, 0x4, 0x0, 0x1, 0x0, 0x4, 0x0, 0x0, 0x9000, 0x400, 0x5306, 0x3, 0x0, 0x200000, 0x1000, 0x100000, 0x1000, 0x0, 0x10,
            new global::go.debug.pe_package.DataDirectory[]{
                new(0x0, 0x0),
                new(0x6000, 0x378),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x8004, 0x18),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x60b8, 0x7c),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0)
            }.array()
        )),
        sections: new ж<global::go.debug.pe_package.SectionHeader>[]{
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".text"u8, 0xc64, 0x1000, 0xe00, 0x400, 0x0, 0x0, 0x0, 0x0, 0x60500060)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".data"u8, 0x10, 0x2000, 0x200, 0x1200, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".rdata"u8, 0x134, 0x3000, 0x200, 0x1400, 0x0, 0x0, 0x0, 0x0, 0x40300040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".eh_fram"u8, 0x3a0, 0x4000, 0x400, 0x1600, 0x0, 0x0, 0x0, 0x0, 0x40300040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".bss"u8, 0x60, 0x5000, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc0300080U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".idata"u8, 0x378, 0x6000, 0x400, 0x1a00, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".CRT"u8, 0x18, 0x7000, 0x200, 0x1e00, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".tls"u8, 0x20, 0x8000, 0x200, 0x2000, 0x0, 0x0, 0x0, 0x0, 0xc0300040U))
        }.slice(),
        hasNoDwarfInfo: true
    ),
    new(
        @file: "testdata/gcc-amd64-mingw-obj"u8,
        hdr: new FileHeader(0x8664, 0x6, 0x0, 0x198, 0x12, 0x0, 0x4),
        sections: new ж<global::go.debug.pe_package.SectionHeader>[]{
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".text"u8, 0x0, 0x0, 0x30, 0x104, 0x15c, 0x0, 0x3, 0x0, 0x60500020)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".data"u8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc0500040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".bss"u8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc0500080U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".rdata"u8, 0x0, 0x0, 0x10, 0x134, 0x0, 0x0, 0x0, 0x0, 0x40500040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".xdata"u8, 0x0, 0x0, 0xc, 0x144, 0x0, 0x0, 0x0, 0x0, 0x40300040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".pdata"u8, 0x0, 0x0, 0xc, 0x150, 0x17a, 0x0, 0x3, 0x0, 0x40300040))
        }.slice(),
        symbols: new ж<global::go.debug.pe_package.Symbol>[]{
            Ꮡ(new global::go.debug.pe_package.Symbol(".file"u8, 0x0, -2, 0x0, 0x67)),
            Ꮡ(new global::go.debug.pe_package.Symbol("main"u8, 0x0, 1, 0x20, 0x2)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".text"u8, 0x0, 1, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".data"u8, 0x0, 2, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".bss"u8, 0x0, 3, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".rdata"u8, 0x0, 4, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".xdata"u8, 0x0, 5, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".pdata"u8, 0x0, 6, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol("__main"u8, 0x0, 0, 0x20, 0x2)),
            Ꮡ(new global::go.debug.pe_package.Symbol("puts"u8, 0x0, 0, 0x20, 0x2))
        }.slice(),
        hasNoDwarfInfo: true
    ),
    new(
        @file: "testdata/gcc-amd64-mingw-exec"u8,
        hdr: new FileHeader(0x8664, 0x11, 0x53e4364f, 0x39600, 0x6fc, 0xf0, 0x27),
        opthdr: Ꮡ(new OptionalHeader64(
            0x20b, 0x2, 0x16, 0x6a00, 0x2400, 0x1600, 0x14e0, 0x1000, 0x400000, 0x1000, 0x200, 0x4, 0x0, 0x0, 0x0, 0x5, 0x2, 0x0, 0x45000, 0x600, 0x46f19, 0x3, 0x0, 0x200000, 0x1000, 0x100000, 0x1000, 0x0, 0x10,
            new global::go.debug.pe_package.DataDirectory[]{
                new(0x0, 0x0),
                new(0xe000, 0x990),
                new(0x0, 0x0),
                new(0xa000, 0x498),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x10000, 0x28),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0xe254, 0x218),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0)
            }.array()
        )),
        sections: new ж<global::go.debug.pe_package.SectionHeader>[]{
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".text"u8, 0x6860, 0x1000, 0x6a00, 0x600, 0x0, 0x0, 0x0, 0x0, 0x60500020)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".data"u8, 0xe0, 0x8000, 0x200, 0x7000, 0x0, 0x0, 0x0, 0x0, 0xc0500040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".rdata"u8, 0x6b0, 0x9000, 0x800, 0x7200, 0x0, 0x0, 0x0, 0x0, 0x40600040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".pdata"u8, 0x498, 0xa000, 0x600, 0x7a00, 0x0, 0x0, 0x0, 0x0, 0x40300040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".xdata"u8, 0x488, 0xb000, 0x600, 0x8000, 0x0, 0x0, 0x0, 0x0, 0x40300040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".bss"u8, 0x1410, 0xc000, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc0600080U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".idata"u8, 0x990, 0xe000, 0xa00, 0x8600, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".CRT"u8, 0x68, 0xf000, 0x200, 0x9000, 0x0, 0x0, 0x0, 0x0, 0xc0400040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".tls"u8, 0x48, 0x10000, 0x200, 0x9200, 0x0, 0x0, 0x0, 0x0, 0xc0600040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_aranges"u8, 0x600, 0x11000, 0x600, 0x9400, 0x0, 0x0, 0x0, 0x0, 0x42500040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_info"u8, 0x1316e, 0x12000, 0x13200, 0x9a00, 0x0, 0x0, 0x0, 0x0, 0x42100040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_abbrev"u8, 0x2ccb, 0x26000, 0x2e00, 0x1cc00, 0x0, 0x0, 0x0, 0x0, 0x42100040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_line"u8, 0x3c4d, 0x29000, 0x3e00, 0x1fa00, 0x0, 0x0, 0x0, 0x0, 0x42100040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_frame"u8, 0x18b8, 0x2d000, 0x1a00, 0x23800, 0x0, 0x0, 0x0, 0x0, 0x42400040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_str"u8, 0x396, 0x2f000, 0x400, 0x25200, 0x0, 0x0, 0x0, 0x0, 0x42100040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_loc"u8, 0x13240, 0x30000, 0x13400, 0x25600, 0x0, 0x0, 0x0, 0x0, 0x42100040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_ranges"u8, 0xa70, 0x44000, 0xc00, 0x38a00, 0x0, 0x0, 0x0, 0x0, 0x42100040))
        }.slice()
    ),
    new(
        @file: "testdata/vmlinuz-4.15.0-47-generic"u8,
        hdr: new FileHeader(0x8664, 0x4, 0x0, 0x0, 0x1, 0xa0, 0x206),
        opthdr: Ꮡ(new OptionalHeader64(
            0x20b, 0x2, 0x14, 0x7c0590, 0x0, 0x168f870, 0x4680, 0x200, 0x0, 0x20, 0x20, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x1e50000, 0x200, 0x7c3ab0, 0xa, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x6,
            new global::go.debug.pe_package.DataDirectory[]{
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x7c07a0, 0x778),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0),
                new(0x0, 0x0)
            }.array()
        )),
        sections: new ж<global::go.debug.pe_package.SectionHeader>[]{
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".setup"u8, 0x41e0, 0x200, 0x41e0, 0x200, 0x0, 0x0, 0x0, 0x0, 0x60500020)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".reloc"u8, 0x20, 0x43e0, 0x20, 0x43e0, 0x0, 0x0, 0x0, 0x0, 0x42100040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".text"u8, 0x7bc390, 0x4400, 0x7bc390, 0x4400, 0x0, 0x0, 0x0, 0x0, 0x60500020)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".bss"u8, 0x168f870, 0x7c0790, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc8000080U))
        }.slice(),
        hasNoDwarfInfo: true
    )
}.slice());
internal static ref slice<fileTest> fileTests => ref ᏑfileTests.ValueSlot;

internal static bool isOptHdrEq(any a, any b) {
    switch (a.type()) {
    case ж<global::go.debug.pe_package.OptionalHeader32> va: {
        var (vb, ok) = b._<ж<global::go.debug.pe_package.OptionalHeader32>>(ᐧ);
        if (!ok) {
            return false;
        }
        return vb.Value == va.Value;
    }
    case ж<global::go.debug.pe_package.OptionalHeader64> va: {
        var (vb, ok) = b._<ж<global::go.debug.pe_package.OptionalHeader64>>(ᐧ);
        if (!ok) {
            return false;
        }
        return vb.Value == va.Value;
    }
    case null: {
        return b == default!;
    }}
    return false;
}

public static void TestOpen(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in fileTests) {
        var tt = Ꮡ(fileTests, i);
        var (f, err) = Open((~tt).@file);
        if (err != default!) {
            Ꮡt.Error(err);
            continue;
        }
        if (!reflect.DeepEqual((~f).FileHeader, (~tt).hdr)) {
            Ꮡt.Errorf("open %s:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, (~f).FileHeader, (~tt).hdr);
            continue;
        }
        if (!isOptHdrEq((~tt).opthdr, (~f).OptionalHeader)) {
            Ꮡt.Errorf("open %s:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, (~f).OptionalHeader, (~tt).opthdr);
            continue;
        }
        foreach (var (iΔ1, sh) in (~f).Sections) {
            if (iΔ1 >= len((~tt).sections)) {
                break;
            }
            var have = sh.of(global::go.debug.pe_package.ΔSection.ᏑSectionHeader);
            var want = (~tt).sections[iΔ1];
            if (!reflect.DeepEqual(have.OrTypedNil(), want.OrTypedNil())) {
                Ꮡt.Errorf("open %s, section %d:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, iΔ1, have.OrTypedNil(), want.OrTypedNil());
            }
        }
        nint tn = len((~tt).sections);
        nint fn = len((~f).Sections);
        if (tn != fn) {
            Ꮡt.Errorf("open %s: len(Sections) = %d, want %d"u8, (~tt).@file, fn, tn);
        }
        foreach (var (iΔ2, have) in (~f).Symbols) {
            if (iΔ2 >= len((~tt).symbols)) {
                break;
            }
            var want = (~tt).symbols[iΔ2];
            if (!reflect.DeepEqual(have.OrTypedNil(), want.OrTypedNil())) {
                Ꮡt.Errorf("open %s, symbol %d:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, iΔ2, have.OrTypedNil(), want.OrTypedNil());
            }
        }
        if (!(~tt).hasNoDwarfInfo) {
            (_, err) = f.DWARF();
            if (err != default!) {
                Ꮡt.Errorf("fetching %s dwarf details failed: %v"u8, (~tt).@file, err);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileGoˢ = "file.go"u8;

public static void TestOpenFailure(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string filename = fileGoˢ; // not a PE file
    var (_, err) = Open(filename); // don't crash
    if (err == default!) {
        Ꮡt.Errorf("open %s: succeeded unexpectedly"u8, filename);
    }
}

internal static UntypedInt linkNoCgo => iota;
internal static UntypedInt linkCgoDefault => 1;
internal static UntypedInt linkCgoInternal => 2;
internal static UntypedInt linkCgoExternal => 3;

internal static uintptr getImageBase(ж<global::go.debug.pe_package.File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    switch (f.OptionalHeader.type()) {
    case ж<global::go.debug.pe_package.OptionalHeader32> oh: {
        return (uintptr)(~oh).ImageBase;
    }
    case ж<global::go.debug.pe_package.OptionalHeader64> oh: {
        return (uintptr)(~oh).ImageBase;
    }
    default: {
        var oh = f.OptionalHeader;
        throw panic("unexpected optionalheader type");
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingWindowsOnlyTestˢ = (@string)"skipping windows only test"u8;
internal static readonly @string aGoˢ = "a.go"u8;
internal static readonly @string mainˢ = "main"u8;
internal static readonly @string aExeˢ = "a.exe"u8;
internal static readonly @string offsetˢ = "offset=(.*)\n"u8;
internal static readonly object debugGdbScriptsSectionIsˢ = (@string)".debug_gdb_scripts section is not found"u8;
internal static readonly object rNextˢ = (@string)"r.Next:"u8;
internal static readonly object failedToGetAttrLowpcˢ = (@string)"Failed to get AttrLowpc"u8;
internal static readonly object mainMainNotFoundˢ = (@string)"main.main not found"u8;

internal static void testDWARF(ж<testing.T> Ꮡt, nint linktype) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS != "windows"u8) {
            Ꮡt.Skip(skippingWindowsOnlyTestˢ);
        }
        testenv.MustHaveGoRun(new pe_internal_test_package.testing_TжTB(Ꮡt));
        @string tmpdir = Ꮡt.TempDir();
        @string src = filepath.Join(tmpdir, aGoˢ);
        var (@file, err) = os.Create(src);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var (ᴛ1, ᴛ2) = template.New(mainˢ).Parse(testprog);
        err = template.Must(ᴛ1, ᴛ2).Execute(new os.FileжWriter(@file), linktype != linkNoCgo);
        if (err != default!) {
            {
                var errΔ1 = @file.Close(); if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                }
            }
            Ꮡt.Fatal(err);
        }
        {
            var errΔ2 = @file.Close(); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        @string exe = filepath.Join(tmpdir, aExeˢ);
        var args = new @string[]{"build"u8, "-o"u8, exe}.slice();
        var exprᴛ1 = linktype;
        if (exprᴛ1 == linkNoCgo) {
        }
        else if (exprᴛ1 == linkCgoDefault) {
        }
        else if (exprᴛ1 == linkCgoInternal) {
            args = append(args, "-ldflags"u8, "-linkmode=internal");
        }
        else if (exprᴛ1 == linkCgoExternal) {
            args = append(args, "-ldflags"u8, "-linkmode=external");
        }
        else { /* default: */
            Ꮡt.Fatalf("invalid linktype parameter of %v"u8, linktype);
        }

        args = append(args, src);
        (var @out, err) = exec.Command(testenv.GoToolPath(new pe_internal_test_package.testing_TжTB(Ꮡt)), args.ꓸꓸꓸ).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("building test executable for linktype %d failed: %s %s"u8, linktype, err, @out);
        }
        (@out, err) = exec.Command(exe).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("running test executable failed: %s %s"u8, err, @out);
        }
        Ꮡt.Logf("Testprog output:\n%s"u8, ((@string)@out));
        var matches = regexp.MustCompile(offsetˢ).FindStringSubmatch(((@string)@out));
        if (len(matches) < 2) {
            Ꮡt.Fatalf("unexpected program output: %s"u8, @out);
        }
        (var wantoffset, err) = strconv.ParseUint(matches[1], 0, 64);
        if (err != default!) {
            Ꮡt.Fatalf("unexpected main offset %q: %s"u8, matches[1], err);
        }
        (var f, err) = Open(exe);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var imageBase = getImageBase(f);
        bool foundDebugGDBScriptsSection = default!;
        foreach (var (_, sect) in (~f).Sections) {
            if ((~sect).Name == ".debug_gdb_scripts"u8) {
                foundDebugGDBScriptsSection = true;
            }
        }
        if (!foundDebugGDBScriptsSection) {
            Ꮡt.Error(debugGdbScriptsSectionIsˢ);
        }
        (var d, err) = f.DWARF();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // look for main.main
        var r = d.Reader();
        while (ᐧ) {
            var (e, errΔ3) = r.Next();
            if (errΔ3 != default!) {
                Ꮡt.Fatal(rNextˢ, errΔ3);
            }
            if (e == nil) {
                break;
            }
            if ((~e).Tag == dwarf.TagSubprogram) {
                var (name, ok) = e.Val(dwarf.AttrName)._<@string>(ᐧ);
                if (ok && name == "main.main"u8) {
                    Ꮡt.Logf("Found main.main"u8);
                    var (addr, okΔ1) = e.Val(dwarf.AttrLowpc)._<uint64>(ᐧ);
                    if (!okΔ1) {
                        Ꮡt.Fatal(failedToGetAttrLowpcˢ);
                    }
                    var offset = (uintptr)addr - imageBase;
                    if (offset != (uintptr)wantoffset) {
                        Ꮡt.Fatalf("Runtime offset (0x%x) did "u8 + "not match dwarf offset "u8 + "(0x%x)"u8, wantoffset, offset);
                    }
                    return;
                }
            }
        }
        Ꮡt.Fatal(mainMainNotFoundˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gccˢ = "gcc"u8;
internal static readonly object skippingTestGccIsMissingˢ = (@string)"skipping test: gcc is missing"u8;
internal static readonly @string includeStdioHIntZero0Intˢ = """

#include <stdio.h>

int zero = 0;

int
main(void)
{
	printf("%d\n", zero);
	return 0;
}

"""u8;
internal static readonly @string aObjˢ = "a.obj"u8;
internal static readonly object couldNotFindBssSectionˢ = (@string)"could not find .bss section"u8;
internal static readonly object bssDataSucceededExpectedˢ = (@string)"bss.Data succeeded, expected error"u8;

public static void TestBSSHasZeros(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExec(new pe_internal_test_package.testing_TжTB(Ꮡt));
        if (runtime.GOOS != "windows"u8) {
            Ꮡt.Skip(skippingWindowsOnlyTestˢ);
        }
        var (gccpath, err) = exec.LookPath(gccˢ);
        if (err != default!) {
            Ꮡt.Skip(skippingTestGccIsMissingˢ);
        }
        @string tmpdir = Ꮡt.TempDir();
        @string srcpath = filepath.Join(tmpdir, "a.c");
        @string src = includeStdioHIntZero0Intˢ;
        err = os.WriteFile(srcpath, slice<byte>(src), 420);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string objpath = filepath.Join(tmpdir, aObjˢ);
        var cmd = exec.Command(gccpath, "-c"u8, srcpath, "-o", objpath);
        (var @out, err) = cmd.CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("failed to build object file: %v - %v"u8, err, ((@string)@out));
        }
        (var f, err) = Open(objpath);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        ж<global::go.debug.pe_package.ΔSection> bss = default!;
        foreach (var (_, sect) in (~f).Sections) {
            if ((~sect).Name == ".bss"u8) {
                bss = sect;
                break;
            }
        }
        if (bss == nil) {
            Ꮡt.Fatal(couldNotFindBssSectionˢ);
        }
        // We expect an error from bss.Data, as there are no contents.
        {
            var (_, errΔ1) = bss.Data(); if (errΔ1 == default!) {
                Ꮡt.Error(bssDataSucceededExpectedˢ);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestDWARF(ж<testing.T> Ꮡt) {
    testDWARF(Ꮡt, linkNoCgo);
}

internal static readonly @string testprog = """

package main

import "fmt"
import "syscall"
import "unsafe"
{{if .}}import "C"
{{end}}

// struct MODULEINFO from the Windows SDK
type moduleinfo struct {
	BaseOfDll uintptr
	SizeOfImage uint32
	EntryPoint uintptr
}

func add(p unsafe.Pointer, x uintptr) unsafe.Pointer {
	return unsafe.Pointer(uintptr(p) + x)
}

func funcPC(f interface{}) uintptr {
	var a uintptr
	return **(**uintptr)(add(unsafe.Pointer(&f), unsafe.Sizeof(a)))
}

func main() {
	kernel32 := syscall.MustLoadDLL("kernel32.dll")
	psapi := syscall.MustLoadDLL("psapi.dll")
	getModuleHandle := kernel32.MustFindProc("GetModuleHandleW")
	getCurrentProcess := kernel32.MustFindProc("GetCurrentProcess")
	getModuleInformation := psapi.MustFindProc("GetModuleInformation")

	procHandle, _, _ := getCurrentProcess.Call()
	moduleHandle, _, err := getModuleHandle.Call(0)
	if moduleHandle == 0 {
		panic(fmt.Sprintf("GetModuleHandle() failed: %d", err))
	}

	var info moduleinfo
	ret, _, err := getModuleInformation.Call(procHandle, moduleHandle,
		uintptr(unsafe.Pointer(&info)), unsafe.Sizeof(info))

	if ret == 0 {
		panic(fmt.Sprintf("GetModuleInformation() failed: %d", err))
	}

	offset := funcPC(main) - info.BaseOfDll
	fmt.Printf("base=0x%x\n", info.BaseOfDll)
	fmt.Printf("main=%p\n", main)
	fmt.Printf("offset=0x%x\n", offset)
}

"""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string buildˢ = "build"u8;
internal static readonly @string ldflagsˢ = "-ldflags"u8;
internal static readonly @string hWindowsguiˢ = "-H=windowsgui"u8;

public static void TestBuildingWindowsGUI(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoBuild(new pe_internal_test_package.testing_TжTB(Ꮡt));
        if (runtime.GOOS != "windows"u8) {
            Ꮡt.Skip(skippingWindowsOnlyTestˢ);
        }
        @string tmpdir = Ꮡt.TempDir();
        @string src = filepath.Join(tmpdir, aGoˢ);
        {
            var errΔ1 = os.WriteFile(src, slice<byte>(@"package main; func main() {}"u8), 420); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        @string exe = filepath.Join(tmpdir, aExeˢ);
        var cmd = exec.Command(testenv.GoToolPath(new pe_internal_test_package.testing_TжTB(Ꮡt)), buildˢ, ldflagsˢ, hWindowsguiˢ, "-o", exe, src);
        var (@out, err) = cmd.CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("building test executable failed: %s %s"u8, err, @out);
        }
        (var f, err) = Open(exe);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        switch ((~f).OptionalHeader.type()) {
        case ж<global::go.debug.pe_package.OptionalHeader32> oh: {
            if ((~oh).Subsystem != IMAGE_SUBSYSTEM_WINDOWS_GUI) {
                Ꮡt.Errorf("unexpected Subsystem value: have %d, but want %d"u8, (~oh).Subsystem, (nint)(IMAGE_SUBSYSTEM_WINDOWS_GUI));
            }
            break;
        }
        case ж<global::go.debug.pe_package.OptionalHeader64> oh: {
            if ((~oh).Subsystem != IMAGE_SUBSYSTEM_WINDOWS_GUI) {
                Ꮡt.Errorf("unexpected Subsystem value: have %d, but want %d"u8, (~oh).Subsystem, (nint)(IMAGE_SUBSYSTEM_WINDOWS_GUI));
            }
            break;
        }
        default: {
            var oh = (~f).OptionalHeader;
            Ꮡt.Fatalf("unexpected OptionalHeader type: have %T, but want *pe.OptionalHeader32 or *pe.OptionalHeader64"u8, oh);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingWindowsOnlyTestˢ2 = (@string)"skipping Windows-only test"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string filenameᶜ = "ws2_32.dll"u8;

public static void TestImportTableInUnknownSection(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS != "windows"u8) {
            Ꮡt.Skip(skippingWindowsOnlyTestˢ2);
        }
        // ws2_32.dll import table is located in ".rdata" section,
        // so it is good enough to test issue #16103.
        @string filename = filenameᶜ;
        var (path, err) = exec.LookPath(filename);
        if (err != default!) {
            Ꮡt.Fatalf("unable to locate required file %q in search path: %s"u8, filename, err);
        }
        (var f, err) = Open(path);
        if (err != default!) {
            Ꮡt.Error(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        // now we can extract its imports
        (var symbols, err) = f.ImportedSymbols();
        if (err != default!) {
            Ꮡt.Error(err);
        }
        if (len(symbols) == 0) {
            Ꮡt.Fatalf("unable to locate any imported symbols within file %q."u8, path);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object newFileSucceededˢ = (@string)"NewFile succeeded unexpectedly"u8;

public static void TestInvalidOptionalHeaderMagic(ж<testing.T> Ꮡt) {
    // Files with invalid optional header magic should return error from NewFile()
    // (see https://golang.org/issue/30250 and https://golang.org/issue/32126 for details).
    // Input generated by gofuzz
    var data = slice<byte>(((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30})) + "00000000000000000000" + ((@string)(new byte[]{0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + "00000000000000000000" + "0000000000000000");
    var (_, err) = NewFile(new pe_internal_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)));
    if (err == default!) {
        Ꮡt.Fatal(newFileSucceededˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataGccAmd64MingwObjˢ = "testdata/gcc-amd64-mingw-obj"u8;
internal static readonly object expectedFOptionalHeaderˢ = (@string)"expected f.OptionalHeader to be nil, received non-nil optional header"u8;

public static void TestImportedSymbolsNoPanicMissingOptionalHeader(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // https://golang.org/issue/30250
    // ImportedSymbols shouldn't panic if optional headers is missing
    var (data, err) = os.ReadFile(testdataGccAmd64MingwObjˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var f, err) = NewFile(new pe_internal_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((~f).OptionalHeader != default!) {
        Ꮡt.Fatal(expectedFOptionalHeaderˢ);
    }
    (var syms, err) = f.ImportedSymbols();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (len(syms) != 0) {
        Ꮡt.Fatalf("expected len(syms) == 0, received len(syms) = %d"u8, len(syms));
    }
}

public static void TestImportedSymbolsNoPanicWithSliceOutOfBound(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // https://golang.org/issue/30253
    // ImportedSymbols shouldn't panic with slice out of bounds
    // Input generated by gofuzz
    var data = slice<byte>(((@string)(new byte[]{0x4c, 0x01, 0x08, 0x00, 0x72, 0x65, 0x67, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe0, 0x00, 0x0f, 0x03})) + ((@string)(new byte[]{0x0b, 0x01, 0x02, 0x18, 0x00, 0x0e, 0x00, 0x00, 0x00, 0x1e, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x80, 0x12, 0x00, 0x00})) + "\x00\x10\x00\x00\x00 \x00\x00\x00\x00@\x00\x00\x10\x00\x00\x00\x02\x00\x00" + ((@string)(new byte[]{0x04, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x00, 0x00})) + "\x00\x04\x00\x00\x06S\x00\x00\x03\x00\x00\x00\x00\x00 \x00\x00\x10\x00\x00" + "\x00\x00\x10\x00\x00\x10\x00\x00\x00\x00\x00\x00\x10\x00\x00\x00\x00\x00\x00\x00" + "\x00\x00\x00\x00\x00`\x00\x00x\x03\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xb8, 0x60, 0x00, 0x00, 0x7c, 0x00, 0x00, 0x00})) + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x2e, 0x74, 0x65, 0x78, 0x74, 0x00, 0x00, 0x00, 0x64, 0x0c, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00})) + "\x00\x0e\x00\x00\x00\x04\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + "`\x00P`.data\x00\x00\x00\x10\x00\x00\x00\x00 \x00\x00" + "\x00\x02\x00\x00\x00\x12\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + ((@string)(new byte[]{0x40, 0x00, 0x30, 0xc0, 0x2e, 0x72, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x34, 0x01, 0x00, 0x00, 0x00, 0x30, 0x00, 0x00})) + "\x00\x02\x00\x00\x00\x14\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + ((@string)(new byte[]{0x40, 0x00, 0x30, 0x40, 0x2e, 0x65, 0x68, 0x5f, 0x66, 0x72, 0x61, 0x6d, 0xa0, 0x03, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00})) + "\x00\x04\x00\x00\x00\x16\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + ((@string)(new byte[]{0x40, 0x00, 0x30, 0x40, 0x2e, 0x62, 0x73, 0x73, 0x00, 0x00, 0x00, 0x00, 0x60, 0x00, 0x00, 0x00, 0x00, 0x50, 0x00, 0x00})) + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + ((@string)(new byte[]{0x80, 0x00, 0x30, 0xc0, 0x2e, 0x69, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x78, 0x03, 0x00, 0x00, 0x00, 0x60, 0x00, 0x00})) + "\x04\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00@\x00" + ((@string)(new byte[]{0x30, 0xc0, 0x2e, 0x43, 0x52, 0x54, 0x00, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x70, 0x00, 0x00, 0x00, 0x02})) + "\x00\x00\x00\x1e\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00@\x00" + ((@string)(new byte[]{0x30, 0xc0, 0x2e, 0x74, 0x6c, 0x73, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x02})) + ((@string)(new byte[]{0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x31, 0xc9})) + ((@string)(new byte[]{0x48, 0x89, 0x35, 0x1d})));
    var (f, err) = NewFile(new pe_internal_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var syms, err) = f.ImportedSymbols();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (len(syms) != 0) {
        Ꮡt.Fatalf("expected len(syms) == 0, received len(syms) = %d"u8, len(syms));
    }
}

} // end pe_internal_test_package

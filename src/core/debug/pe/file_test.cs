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

internal partial struct fileTest {
    internal @string @file;
    internal global::go.debug.pe_package.FileHeader hdr;
    internal any opthdr;
    internal slice<ж<global::go.debug.pe_package.SectionHeader>> sections;
    internal slice<ж<global::go.debug.pe_package.Symbol>> symbols;
    internal bool hasNoDwarfInfo;
}

internal static readonly ж<slice<fileTest>> ᏑfileTests = new StaticBox<slice<fileTest>>([
    new(
        @file: "testdata/gcc-386-mingw-obj",
        hdr: new FileHeader(0x014c, 0x000c, 0x0, 0x64a, 0x1e, 0x0, 0x104),
        sections: [
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".text", 0, 0, 36, 500, 1440, 0, 3, 0, 0x60300020)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".data", 0, 0, 0, 0, 0, 0, 0, 0, 3224371264U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".bss", 0, 0, 0, 0, 0, 0, 0, 0, 3224371328U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_abbrev", 0, 0, 137, 536, 0, 0, 0, 0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_info", 0, 0, 418, 673, 1470, 0, 7, 0, 1108344832)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_line", 0, 0, 128, 1091, 1540, 0, 1, 0, 1108344832)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".rdata", 0, 0, 16, 1219, 0, 0, 0, 0, 1076887616)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_frame", 0, 0, 52, 1235, 1550, 0, 2, 0, 1110441984)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_loc", 0, 0, 56, 1287, 0, 0, 0, 0, 1108344832)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_pubnames", 0, 0, 27, 1343, 1570, 0, 1, 0, 1108344832)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_pubtypes", 0, 0, 38, 1370, 1580, 0, 1, 0, 1108344832)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_aranges", 0, 0, 32, 1408, 1590, 0, 2, 0, 1108344832))
        ],
        symbols: [
            Ꮡ(new global::go.debug.pe_package.Symbol(".file", 0x0, -2, 0x0, 0x67)),
            Ꮡ(new global::go.debug.pe_package.Symbol("_main", 0x0, 1, 0x20, 0x2)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".text", 0x0, 1, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".data", 0x0, 2, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".bss", 0x0, 3, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_abbrev", 0x0, 4, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_info", 0x0, 5, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_line", 0x0, 6, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".rdata", 0x0, 7, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_frame", 0x0, 8, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_loc", 0x0, 9, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_pubnames", 0x0, 10, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_pubtypes", 0x0, 11, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".debug_aranges", 0x0, 12, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol("___main", 0x0, 0, 0x20, 0x2)),
            Ꮡ(new global::go.debug.pe_package.Symbol("_puts", 0x0, 0, 0x20, 0x2))
        ]
    ),
    new(
        @file: "testdata/gcc-386-mingw-exec",
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
        sections: [
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".text", 0xcd8, 0x1000, 0xe00, 0x400, 0x0, 0x0, 0x0, 0x0, 0x60500060)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".data", 0x10, 0x2000, 0x200, 0x1200, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".rdata", 0x120, 0x3000, 0x200, 0x1400, 0x0, 0x0, 0x0, 0x0, 0x40300040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".bss", 0xdc, 0x4000, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc0400080U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".idata", 0x3c8, 0x5000, 0x400, 0x1600, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".CRT", 0x18, 0x6000, 0x200, 0x1a00, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".tls", 0x20, 0x7000, 0x200, 0x1c00, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_aranges", 0x20, 0x8000, 0x200, 0x1e00, 0x0, 0x0, 0x0, 0x0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_pubnames", 0x51, 0x9000, 0x200, 0x2000, 0x0, 0x0, 0x0, 0x0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_pubtypes", 0x91, 0xa000, 0x200, 0x2200, 0x0, 0x0, 0x0, 0x0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_info", 0xe22, 0xb000, 0x1000, 0x2400, 0x0, 0x0, 0x0, 0x0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_abbrev", 0x157, 0xc000, 0x200, 0x3400, 0x0, 0x0, 0x0, 0x0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_line", 0x144, 0xd000, 0x200, 0x3600, 0x0, 0x0, 0x0, 0x0, 0x42100000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_frame", 0x34, 0xe000, 0x200, 0x3800, 0x0, 0x0, 0x0, 0x0, 0x42300000)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_loc", 0x38, 0xf000, 0x200, 0x3a00, 0x0, 0x0, 0x0, 0x0, 0x42100000))
        ]
    ),
    new(
        @file: "testdata/gcc-386-mingw-no-symbols-exec",
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
        sections: [
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".text", 0xc64, 0x1000, 0xe00, 0x400, 0x0, 0x0, 0x0, 0x0, 0x60500060)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".data", 0x10, 0x2000, 0x200, 0x1200, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".rdata", 0x134, 0x3000, 0x200, 0x1400, 0x0, 0x0, 0x0, 0x0, 0x40300040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".eh_fram", 0x3a0, 0x4000, 0x400, 0x1600, 0x0, 0x0, 0x0, 0x0, 0x40300040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".bss", 0x60, 0x5000, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc0300080U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".idata", 0x378, 0x6000, 0x400, 0x1a00, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".CRT", 0x18, 0x7000, 0x200, 0x1e00, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".tls", 0x20, 0x8000, 0x200, 0x2000, 0x0, 0x0, 0x0, 0x0, 0xc0300040U))
        ],
        hasNoDwarfInfo: true
    ),
    new(
        @file: "testdata/gcc-amd64-mingw-obj",
        hdr: new FileHeader(0x8664, 0x6, 0x0, 0x198, 0x12, 0x0, 0x4),
        sections: [
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".text", 0x0, 0x0, 0x30, 0x104, 0x15c, 0x0, 0x3, 0x0, 0x60500020)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".data", 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc0500040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".bss", 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc0500080U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".rdata", 0x0, 0x0, 0x10, 0x134, 0x0, 0x0, 0x0, 0x0, 0x40500040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".xdata", 0x0, 0x0, 0xc, 0x144, 0x0, 0x0, 0x0, 0x0, 0x40300040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".pdata", 0x0, 0x0, 0xc, 0x150, 0x17a, 0x0, 0x3, 0x0, 0x40300040))
        ],
        symbols: [
            Ꮡ(new global::go.debug.pe_package.Symbol(".file", 0x0, -2, 0x0, 0x67)),
            Ꮡ(new global::go.debug.pe_package.Symbol("main", 0x0, 1, 0x20, 0x2)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".text", 0x0, 1, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".data", 0x0, 2, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".bss", 0x0, 3, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".rdata", 0x0, 4, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".xdata", 0x0, 5, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol(".pdata", 0x0, 6, 0x0, 0x3)),
            Ꮡ(new global::go.debug.pe_package.Symbol("__main", 0x0, 0, 0x20, 0x2)),
            Ꮡ(new global::go.debug.pe_package.Symbol("puts", 0x0, 0, 0x20, 0x2))
        ],
        hasNoDwarfInfo: true
    ),
    new(
        @file: "testdata/gcc-amd64-mingw-exec",
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
        sections: [
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".text", 0x6860, 0x1000, 0x6a00, 0x600, 0x0, 0x0, 0x0, 0x0, 0x60500020)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".data", 0xe0, 0x8000, 0x200, 0x7000, 0x0, 0x0, 0x0, 0x0, 0xc0500040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".rdata", 0x6b0, 0x9000, 0x800, 0x7200, 0x0, 0x0, 0x0, 0x0, 0x40600040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".pdata", 0x498, 0xa000, 0x600, 0x7a00, 0x0, 0x0, 0x0, 0x0, 0x40300040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".xdata", 0x488, 0xb000, 0x600, 0x8000, 0x0, 0x0, 0x0, 0x0, 0x40300040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".bss", 0x1410, 0xc000, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc0600080U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".idata", 0x990, 0xe000, 0xa00, 0x8600, 0x0, 0x0, 0x0, 0x0, 0xc0300040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".CRT", 0x68, 0xf000, 0x200, 0x9000, 0x0, 0x0, 0x0, 0x0, 0xc0400040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".tls", 0x48, 0x10000, 0x200, 0x9200, 0x0, 0x0, 0x0, 0x0, 0xc0600040U)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_aranges", 0x600, 0x11000, 0x600, 0x9400, 0x0, 0x0, 0x0, 0x0, 0x42500040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_info", 0x1316e, 0x12000, 0x13200, 0x9a00, 0x0, 0x0, 0x0, 0x0, 0x42100040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_abbrev", 0x2ccb, 0x26000, 0x2e00, 0x1cc00, 0x0, 0x0, 0x0, 0x0, 0x42100040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_line", 0x3c4d, 0x29000, 0x3e00, 0x1fa00, 0x0, 0x0, 0x0, 0x0, 0x42100040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_frame", 0x18b8, 0x2d000, 0x1a00, 0x23800, 0x0, 0x0, 0x0, 0x0, 0x42400040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_str", 0x396, 0x2f000, 0x400, 0x25200, 0x0, 0x0, 0x0, 0x0, 0x42100040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_loc", 0x13240, 0x30000, 0x13400, 0x25600, 0x0, 0x0, 0x0, 0x0, 0x42100040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".debug_ranges", 0xa70, 0x44000, 0xc00, 0x38a00, 0x0, 0x0, 0x0, 0x0, 0x42100040))
        ]
    ),
    new(
        // testdata/vmlinuz-4.15.0-47-generic is a trimmed down version of Linux Kernel image.
        // The original Linux Kernel image is about 8M and it is not recommended to add such a big binary file to the repo.
        // Moreover only a very small portion of the original Kernel image was being parsed by debug/pe package.
        // In order to identify this portion, the original image was first parsed by modified debug/pe package.
        // Modification essentially communicated reader's positions before and after parsing.
        // Finally, bytes between those positions where written to a separate file,
        // generating trimmed down version Linux Kernel image used in this test case.
        @file: "testdata/vmlinuz-4.15.0-47-generic",
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
        sections: [
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".setup", 0x41e0, 0x200, 0x41e0, 0x200, 0x0, 0x0, 0x0, 0x0, 0x60500020)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".reloc", 0x20, 0x43e0, 0x20, 0x43e0, 0x0, 0x0, 0x0, 0x0, 0x42100040)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".text", 0x7bc390, 0x4400, 0x7bc390, 0x4400, 0x0, 0x0, 0x0, 0x0, 0x60500020)),
            Ꮡ(new global::go.debug.pe_package.SectionHeader(".bss", 0x168f870, 0x7c0790, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc8000080U))
        ],
        hasNoDwarfInfo: true
    )
]);
internal static ref slice<fileTest> fileTests => ref ((StaticBox<slice<fileTest>>)ᏑfileTests).ValueSlot;

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
        return b == nil;
    }}
    return false;
}

public static void TestOpen(ж<testing.T> t) {
    foreach (var i in range(len(fileTests))) {
        var tt = Ꮡ(fileTests, i);

        var (f, err) = Open(tt.@file);
        if (err != nil) {
            t.Error(err);
            continue;
        }
        if (!reflect.DeepEqual(f.FileHeader, tt.hdr)) {
            t.Errorf("open %s:\n\thave %#v\n\twant %#v\n", tt.@file, f.FileHeader, tt.hdr);
            continue;
        }
        if (!isOptHdrEq(tt.opthdr, f.OptionalHeader)) {
            t.Errorf("open %s:\n\thave %#v\n\twant %#v\n", tt.@file, f.OptionalHeader, tt.opthdr);
            continue;
        }

        foreach (var (iΔ1, sh) in range(f.Sections)) {
            if (iΔ1 >= len(tt.sections)) {
                break;
            }
            var have = sh.ᏑSectionHeader;
            var want = tt.sections[iΔ1];
            if (!reflect.DeepEqual(have.OrTypedNil(), want.OrTypedNil())) {
                t.Errorf("open %s, section %d:\n\thave %#v\n\twant %#v\n", tt.@file, iΔ1, have.OrTypedNil(), want.OrTypedNil());
            }
        }
        nint tn = len(tt.sections);
        nint fn = len(f.Sections);
        if (tn != fn) {
            t.Errorf("open %s: len(Sections) = %d, want %d", tt.@file, fn, tn);
        }
        foreach (var (iΔ2, have) in range(f.Symbols)) {
            if (iΔ2 >= len(tt.symbols)) {
                break;
            }
            var want = tt.symbols[iΔ2];
            if (!reflect.DeepEqual(have.OrTypedNil(), want.OrTypedNil())) {
                t.Errorf("open %s, symbol %d:\n\thave %#v\n\twant %#v\n", tt.@file, iΔ2, have.OrTypedNil(), want.OrTypedNil());
            }
        }
        if (!tt.hasNoDwarfInfo) {
            (_, err) = f.DWARF();
            if (err != nil) {
                t.Errorf("fetching %s dwarf details failed: %v", tt.@file, err);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fileGoˢ => ref fileGoˢᵇ;
private static readonly @string fileGoˢᵇ = "file.go"u8;

public static void TestOpenFailure(ж<testing.T> t) {
    @string filename = fileGoˢ;    // not a PE file
    var (_, err) = Open(filename); // don't crash
    if (err == nil) {
        t.Errorf("open %s: succeeded unexpectedly", filename);
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
        return (uintptr)oh.ImageBase;
    }
    case ж<global::go.debug.pe_package.OptionalHeader64> oh: {
        return (uintptr)oh.ImageBase;
    }
    default: {
        var oh = f.OptionalHeader;
        throw panic("unexpected optionalheader type");
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingWindowsOnlyTestˢ = (@string)"skipping windows only test"u8;
internal static ref readonly @string aGoˢ => ref aGoˢᵇ;
private static readonly @string aGoˢᵇ = "a.go"u8;
internal static ref readonly @string mainˢ => ref mainˢᵇ;
private static readonly @string mainˢᵇ = "main"u8;
internal static ref readonly @string aExeˢ => ref aExeˢᵇ;
private static readonly @string aExeˢᵇ = "a.exe"u8;
internal static ref readonly @string offsetˢ => ref offsetˢᵇ;
private static readonly @string offsetˢᵇ = "offset=(.*)\n"u8;
internal static readonly object debugGdbScriptsSectionIsˢ = (@string)".debug_gdb_scripts section is not found"u8;
internal static readonly object rNextˢ = (@string)"r.Next:"u8;
internal static readonly object failedToGetAttrLowpcˢ = (@string)"Failed to get AttrLowpc"u8;
internal static readonly object mainMainNotFoundˢ = (@string)"main.main not found"u8;

internal static void testDWARF(ж<testing.T> t, nint linktype) {
    GoFrame ᒐ = default;
    try {
        var argsᴷ = new stackAppend<@string>();

        if (runtime.GOOS != "windows"u8) {
            t.Skip(skippingWindowsOnlyTestˢ);
        }
        testenv.MustHaveGoRun(new pe_internal_test_package.testing_TжTB(t));

        @string tmpdir = t.TempDir();

        @string src = filepath.Join(tmpdir, aGoˢ);
        var (@file, err) = os.Create(src);
        if (err != nil) {
            t.Fatal(err);
        }
        var (ᴛ1, ᴛ2) = template.New(mainˢ).Parse(testprog);
        err = template.Must(ᴛ1, ᴛ2).Execute(new os.FileжWriter(@file), linktype != linkNoCgo);
        if (err != nil) {
            var errΔ1 = @file.Close(); if (errΔ1 != nil) {
                t.Error(errΔ1);
            }
            t.Fatal(err);
        }
        var errΔ2 = @file.Close(); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }

        @string exe = filepath.Join(tmpdir, aExeˢ);
        var args = slice<@string>(["build", "-o", exe]);
        switch (linktype) {
        case var ᴛ when ᴛ == linkNoCgo: {
            break;
        }
        case var ᴛ when ᴛ == linkCgoDefault: {
            break;
        }
        case var ᴛ when ᴛ == linkCgoInternal: {
            args = append(argsᴷ, args, (@string)"-ldflags", (@string)"-linkmode=internal");
            break;
        }
        case var ᴛ when ᴛ == linkCgoExternal: {
            args = append(args, (@string)"-ldflags", (@string)"-linkmode=external");
            break;
        }
        default: {
            t.Fatalf("invalid linktype parameter of %v", linktype);
            break;
        }}
        args = append(args, src);
        (var @out, err) = exec.Command(testenv.GoToolPath(new pe_internal_test_package.testing_TжTB(t)), args.ꓸꓸꓸ).CombinedOutput();
        if (err != nil) {
            t.Fatalf("building test executable for linktype %d failed: %s %s", linktype, err, @out);
        }
        (@out, err) = exec.Command(exe).CombinedOutput();
        if (err != nil) {
            t.Fatalf("running test executable failed: %s %s", err, @out);
        }
        t.Logf("Testprog output:\n%s", (@string)@out);

        var matches = regexp.MustCompile(offsetˢ).FindStringSubmatch((@string)@out);
        if (len(matches) < 2) {
            t.Fatalf("unexpected program output: %s", @out);
        }
        (var wantoffset, err) = strconv.ParseUint(matches[1], 0, 64);
        if (err != nil) {
            t.Fatalf("unexpected main offset %q: %s", matches[1], err);
        }

        (var f, err) = Open(exe);
        if (err != nil) {
            t.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        var imageBase = getImageBase(f);

        bool foundDebugGDBScriptsSection = default;
        foreach (var (_, sect) in range(f.Sections)) {
            if (sect.Name == ".debug_gdb_scripts"u8 || sect.Name == ".zdebug_gdb_scripts"u8) {
                foundDebugGDBScriptsSection = true;
            }
        }
        if (!foundDebugGDBScriptsSection) {
            t.Error(debugGdbScriptsSectionIsˢ);
        }

        (var d, err) = f.DWARF();
        if (err != nil) {
            t.Fatal(err);
        }

        // look for main.main
        var r = d.Reader();
        for (;;) {
            var (e, errΔ3) = r.Next();
            if (errΔ3 != nil) {
                t.Fatal(rNextˢ, errΔ3);
            }
            if (e == nil) {
                break;
            }
            if (e.Tag == dwarf.TagSubprogram) {
                var (name, ok) = e.Val(dwarf.AttrName)._<@string>(ᐧ);
                if (ok && name == "main.main"u8) {
                    t.Logf("Found main.main");
                    var (addr, okΔ1) = e.Val(dwarf.AttrLowpc)._<uint64>(ᐧ);
                    if (!okΔ1) {
                        t.Fatal(failedToGetAttrLowpcˢ);
                    }
                    var offset = (uintptr)addr - imageBase;
                    if (offset != (uintptr)wantoffset) {
                        t.Fatalf("Runtime offset (0x%x) did "u8 + "not match dwarf offset "u8 + "(0x%x)"u8, wantoffset, offset);
                    }
                    return;
                }
            }
        }
        t.Fatal(mainMainNotFoundˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gccˢ => ref gccˢᵇ;
private static readonly @string gccˢᵇ = "gcc"u8;
internal static readonly object skippingTestGccIsMissingˢ = (@string)"skipping test: gcc is missing"u8;
internal static ref readonly @string includeStdioHIntZero0Intˢ => ref includeStdioHIntZero0Intˢᵇ;
private static readonly @string includeStdioHIntZero0Intˢᵇ = """

#include <stdio.h>

int zero = 0;

int
main(void)
{
	printf("%d\n", zero);
	return 0;
}

"""u8.Raw();
internal static ref readonly @string aObjˢ => ref aObjˢᵇ;
private static readonly @string aObjˢᵇ = "a.obj"u8;
internal static readonly object couldNotFindBssSectionˢ = (@string)"could not find .bss section"u8;
internal static readonly object bssDataSucceededExpectedˢ = (@string)"bss.Data succeeded, expected error"u8;

public static void TestBSSHasZeros(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExec(new pe_internal_test_package.testing_TжTB(t));

        if (runtime.GOOS != "windows"u8) {
            t.Skip(skippingWindowsOnlyTestˢ);
        }
        var (gccpath, err) = exec.LookPath(gccˢ);
        if (err != nil) {
            t.Skip(skippingTestGccIsMissingˢ);
        }

        @string tmpdir = t.TempDir();

        @string srcpath = filepath.Join(tmpdir, "a.c");
        @string src = includeStdioHIntZero0Intˢ;
        err = os.WriteFile(srcpath, slice<byte>(src), 420);
        if (err != nil) {
            t.Fatal(err);
        }

        @string objpath = filepath.Join(tmpdir, aObjˢ);
        var cmd = exec.Command(gccpath, "-c", srcpath, "-o", objpath);
        (var @out, err) = cmd.CombinedOutput();
        if (err != nil) {
            t.Fatalf("failed to build object file: %v - %v", err, (@string)@out);
        }

        (var f, err) = Open(objpath);
        if (err != nil) {
            t.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        ж<global::go.debug.pe_package.ΔSection> bss = default;
        foreach (var (_, sect) in range(f.Sections)) {
            if (sect.Name == ".bss"u8) {
                bss = sect;
                break;
            }
        }
        if (bss == nil) {
            t.Fatal(couldNotFindBssSectionˢ);
        }
        // We expect an error from bss.Data, as there are no contents.
        var (_, errΔ1) = bss.Data(); if (errΔ1 == nil) {
            t.Error(bssDataSucceededExpectedˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestDWARF(ж<testing.T> t) {
    testDWARF(t, linkNoCgo);
}

internal static ref readonly @string testprog => ref testprogᵇ;
private static readonly @string testprogᵇ = """

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

"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;
internal static ref readonly @string ldflagsˢ => ref ldflagsˢᵇ;
private static readonly @string ldflagsˢᵇ = "-ldflags"u8;
internal static ref readonly @string hWindowsguiˢ => ref hWindowsguiˢᵇ;
private static readonly @string hWindowsguiˢᵇ = "-H=windowsgui"u8;

public static void TestBuildingWindowsGUI(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoBuild(new pe_internal_test_package.testing_TжTB(t));

        if (runtime.GOOS != "windows"u8) {
            t.Skip(skippingWindowsOnlyTestˢ);
        }
        @string tmpdir = t.TempDir();

        @string src = filepath.Join(tmpdir, aGoˢ);
        var errΔ1 = os.WriteFile(src, slice<byte>(@"package main; func main() {}"u8), 420); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        @string exe = filepath.Join(tmpdir, aExeˢ);
        var cmd = exec.Command(testenv.GoToolPath(new pe_internal_test_package.testing_TжTB(t)), buildˢ, ldflagsˢ, hWindowsguiˢ, "-o", exe, src);
        var (@out, err) = cmd.CombinedOutput();
        if (err != nil) {
            t.Fatalf("building test executable failed: %s %s", err, @out);
        }

        (var f, err) = Open(exe);
        if (err != nil) {
            t.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        switch (f.OptionalHeader.type()) {
        case ж<global::go.debug.pe_package.OptionalHeader32> oh: {
            if (oh.Subsystem != IMAGE_SUBSYSTEM_WINDOWS_GUI) {
                t.Errorf("unexpected Subsystem value: have %d, but want %d", oh.Subsystem, (nint)(IMAGE_SUBSYSTEM_WINDOWS_GUI));
            }
            break;
        }
        case ж<global::go.debug.pe_package.OptionalHeader64> oh: {
            if (oh.Subsystem != IMAGE_SUBSYSTEM_WINDOWS_GUI) {
                t.Errorf("unexpected Subsystem value: have %d, but want %d", oh.Subsystem, (nint)(IMAGE_SUBSYSTEM_WINDOWS_GUI));
            }
            break;
        }
        default: {
            var oh = f.OptionalHeader;
            t.Fatalf("unexpected OptionalHeader type: have %T, but want *pe.OptionalHeader32 or *pe.OptionalHeader64", oh);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingWindowsOnlyTestˢ2 = (@string)"skipping Windows-only test"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string filenameᶜ => ref filenameᶜᵇ;
private static readonly @string filenameᶜᵇ = "ws2_32.dll"u8;

public static void TestImportTableInUnknownSection(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS != "windows"u8) {
            t.Skip(skippingWindowsOnlyTestˢ2);
        }

        // ws2_32.dll import table is located in ".rdata" section,
        // so it is good enough to test issue #16103.
        @string filename = filenameᶜ;
        var (path, err) = exec.LookPath(filename);
        if (err != nil) {
            t.Fatalf("unable to locate required file %q in search path: %s", filename, err);
        }

        (var f, err) = Open(path);
        if (err != nil) {
            t.Error(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        // now we can extract its imports
        (var symbols, err) = f.ImportedSymbols();
        if (err != nil) {
            t.Error(err);
        }

        if (len(symbols) == 0) {
            t.Fatalf("unable to locate any imported symbols within file %q.", path);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object newFileSucceededˢ = (@string)"NewFile succeeded unexpectedly"u8;

public static void TestInvalidOptionalHeaderMagic(ж<testing.T> t) {
    // Files with invalid optional header magic should return error from NewFile()
    // (see https://golang.org/issue/30250 and https://golang.org/issue/32126 for details).
    // Input generated by gofuzz
    var data = slice<byte>(((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30})) + "00000000000000000000" + ((@string)(new byte[]{0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + "00000000000000000000" + "0000000000000000");

    var (_, err) = NewFile(new pe_internal_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)));
    if (err == nil) {
        t.Fatal(newFileSucceededˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataGccAmd64MingwObjˢ => ref testdataGccAmd64MingwObjˢᵇ;
private static readonly @string testdataGccAmd64MingwObjˢᵇ = "testdata/gcc-amd64-mingw-obj"u8;
internal static readonly object expectedFOptionalHeaderˢ = (@string)"expected f.OptionalHeader to be nil, received non-nil optional header"u8;

public static void TestImportedSymbolsNoPanicMissingOptionalHeader(ж<testing.T> t) {
    // https://golang.org/issue/30250
    // ImportedSymbols shouldn't panic if optional headers is missing
    var (data, err) = os.ReadFile(testdataGccAmd64MingwObjˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    (var f, err) = NewFile(new pe_internal_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)));
    if (err != nil) {
        t.Fatal(err);
    }

    if (f.OptionalHeader != nil) {
        t.Fatal(expectedFOptionalHeaderˢ);
    }

    (var syms, err) = f.ImportedSymbols();
    if (err != nil) {
        t.Fatal(err);
    }

    if (len(syms) != 0) {
        t.Fatalf("expected len(syms) == 0, received len(syms) = %d", len(syms));
    }
}

public static void TestImportedSymbolsNoPanicWithSliceOutOfBound(ж<testing.T> t) {
    // https://golang.org/issue/30253
    // ImportedSymbols shouldn't panic with slice out of bounds
    // Input generated by gofuzz
    var data = slice<byte>(((@string)(new byte[]{0x4c, 0x01, 0x08, 0x00, 0x72, 0x65, 0x67, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe0, 0x00, 0x0f, 0x03})) + ((@string)(new byte[]{0x0b, 0x01, 0x02, 0x18, 0x00, 0x0e, 0x00, 0x00, 0x00, 0x1e, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x80, 0x12, 0x00, 0x00})) + "\x00\x10\x00\x00\x00 \x00\x00\x00\x00@\x00\x00\x10\x00\x00\x00\x02\x00\x00" + ((@string)(new byte[]{0x04, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x00, 0x00})) + "\x00\x04\x00\x00\x06S\x00\x00\x03\x00\x00\x00\x00\x00 \x00\x00\x10\x00\x00" + "\x00\x00\x10\x00\x00\x10\x00\x00\x00\x00\x00\x00\x10\x00\x00\x00\x00\x00\x00\x00" + "\x00\x00\x00\x00\x00`\x00\x00x\x03\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xb8, 0x60, 0x00, 0x00, 0x7c, 0x00, 0x00, 0x00})) + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x2e, 0x74, 0x65, 0x78, 0x74, 0x00, 0x00, 0x00, 0x64, 0x0c, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00})) + "\x00\x0e\x00\x00\x00\x04\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + "`\x00P`.data\x00\x00\x00\x10\x00\x00\x00\x00 \x00\x00" + "\x00\x02\x00\x00\x00\x12\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + ((@string)(new byte[]{0x40, 0x00, 0x30, 0xc0, 0x2e, 0x72, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x34, 0x01, 0x00, 0x00, 0x00, 0x30, 0x00, 0x00})) + "\x00\x02\x00\x00\x00\x14\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + ((@string)(new byte[]{0x40, 0x00, 0x30, 0x40, 0x2e, 0x65, 0x68, 0x5f, 0x66, 0x72, 0x61, 0x6d, 0xa0, 0x03, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00})) + "\x00\x04\x00\x00\x00\x16\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + ((@string)(new byte[]{0x40, 0x00, 0x30, 0x40, 0x2e, 0x62, 0x73, 0x73, 0x00, 0x00, 0x00, 0x00, 0x60, 0x00, 0x00, 0x00, 0x00, 0x50, 0x00, 0x00})) + "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00" + ((@string)(new byte[]{0x80, 0x00, 0x30, 0xc0, 0x2e, 0x69, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x78, 0x03, 0x00, 0x00, 0x00, 0x60, 0x00, 0x00})) + "\x04\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00@\x00" + ((@string)(new byte[]{0x30, 0xc0, 0x2e, 0x43, 0x52, 0x54, 0x00, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x70, 0x00, 0x00, 0x00, 0x02})) + "\x00\x00\x00\x1e\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00@\x00" + ((@string)(new byte[]{0x30, 0xc0, 0x2e, 0x74, 0x6c, 0x73, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x02})) + ((@string)(new byte[]{0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x31, 0xc9})) + ((@string)(new byte[]{0x48, 0x89, 0x35, 0x1d})));

    var (f, err) = NewFile(new pe_internal_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)));
    if (err != nil) {
        t.Fatal(err);
    }

    (var syms, err) = f.ImportedSymbols();
    if (err != nil) {
        t.Fatal(err);
    }

    if (len(syms) != 0) {
        t.Fatalf("expected len(syms) == 0, received len(syms) = %d", len(syms));
    }
}

} // end pe_internal_test_package

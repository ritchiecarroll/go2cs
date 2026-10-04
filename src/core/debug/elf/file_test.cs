// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using bytes = bytes_package;
using gzip = compress.gzip_package;
using zlib = compress.zlib_package;
using dwarf = go.debug.dwarf_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using rand = math.rand_package;
using net = net_package;
using os = os_package;
using path = path_package;
using reflect = reflect_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using compress;
using encoding;
using go.debug;
using math;
using static go.debug.elf_package;

partial class elf_internal_test_package {

[GoType] internal partial struct fileTest {
    internal @string @file;
    internal global::go.debug.elf_package.FileHeader hdr;
    internal slice<global::go.debug.elf_package.SectionHeader> sections;
    internal slice<global::go.debug.elf_package.ProgHeader> progs;
    internal slice<@string> needed;
    internal slice<global::go.debug.elf_package.Symbol> symbols;
}

internal static ж<slice<fileTest>> ᏑfileTests = new StandardBox<slice<fileTest>>(new fileTest[]{
    new(
        "testdata/gcc-386-freebsd-exec"u8,
        new FileHeader(ELFCLASS32, ELFDATA2LSB, EV_CURRENT, ELFOSABI_FREEBSD, 0, binary.LittleEndian, ET_EXEC, EM_386, 0x80483cc),
        new global::go.debug.elf_package.SectionHeader[]{
            new(""u8, SHT_NULL, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0),
            new(".interp"u8, SHT_PROGBITS, SHF_ALLOC, 0x80480d4, 0xd4, 0x15, 0x0, 0x0, 0x1, 0x0, 0x15),
            new(".hash"u8, SHT_HASH, SHF_ALLOC, 0x80480ec, 0xec, 0x90, 0x3, 0x0, 0x4, 0x4, 0x90),
            new(".dynsym"u8, SHT_DYNSYM, SHF_ALLOC, 0x804817c, 0x17c, 0x110, 0x4, 0x1, 0x4, 0x10, 0x110),
            new(".dynstr"u8, SHT_STRTAB, SHF_ALLOC, 0x804828c, 0x28c, 0xbb, 0x0, 0x0, 0x1, 0x0, 0xbb),
            new(".rel.plt"u8, SHT_REL, SHF_ALLOC, 0x8048348, 0x348, 0x20, 0x3, 0x7, 0x4, 0x8, 0x20),
            new(".init"u8, SHT_PROGBITS, SHF_ALLOC + SHF_EXECINSTR, 0x8048368, 0x368, 0x11, 0x0, 0x0, 0x4, 0x0, 0x11),
            new(".plt"u8, SHT_PROGBITS, SHF_ALLOC + SHF_EXECINSTR, 0x804837c, 0x37c, 0x50, 0x0, 0x0, 0x4, 0x4, 0x50),
            new(".text"u8, SHT_PROGBITS, SHF_ALLOC + SHF_EXECINSTR, 0x80483cc, 0x3cc, 0x180, 0x0, 0x0, 0x4, 0x0, 0x180),
            new(".fini"u8, SHT_PROGBITS, SHF_ALLOC + SHF_EXECINSTR, 0x804854c, 0x54c, 0xc, 0x0, 0x0, 0x4, 0x0, 0xc),
            new(".rodata"u8, SHT_PROGBITS, SHF_ALLOC, 0x8048558, 0x558, 0xa3, 0x0, 0x0, 0x1, 0x0, 0xa3),
            new(".data"u8, SHT_PROGBITS, SHF_WRITE + SHF_ALLOC, 0x80495fc, 0x5fc, 0xc, 0x0, 0x0, 0x4, 0x0, 0xc),
            new(".eh_frame"u8, SHT_PROGBITS, SHF_ALLOC, 0x8049608, 0x608, 0x4, 0x0, 0x0, 0x4, 0x0, 0x4),
            new(".dynamic"u8, SHT_DYNAMIC, SHF_WRITE + SHF_ALLOC, 0x804960c, 0x60c, 0x98, 0x4, 0x0, 0x4, 0x8, 0x98),
            new(".ctors"u8, SHT_PROGBITS, SHF_WRITE + SHF_ALLOC, 0x80496a4, 0x6a4, 0x8, 0x0, 0x0, 0x4, 0x0, 0x8),
            new(".dtors"u8, SHT_PROGBITS, SHF_WRITE + SHF_ALLOC, 0x80496ac, 0x6ac, 0x8, 0x0, 0x0, 0x4, 0x0, 0x8),
            new(".jcr"u8, SHT_PROGBITS, SHF_WRITE + SHF_ALLOC, 0x80496b4, 0x6b4, 0x4, 0x0, 0x0, 0x4, 0x0, 0x4),
            new(".got"u8, SHT_PROGBITS, SHF_WRITE + SHF_ALLOC, 0x80496b8, 0x6b8, 0x1c, 0x0, 0x0, 0x4, 0x4, 0x1c),
            new(".bss"u8, SHT_NOBITS, SHF_WRITE + SHF_ALLOC, 0x80496d4, 0x6d4, 0x20, 0x0, 0x0, 0x4, 0x0, 0x20),
            new(".comment"u8, SHT_PROGBITS, 0x0, 0x0, 0x6d4, 0x12d, 0x0, 0x0, 0x1, 0x0, 0x12d),
            new(".debug_aranges"u8, SHT_PROGBITS, 0x0, 0x0, 0x801, 0x20, 0x0, 0x0, 0x1, 0x0, 0x20),
            new(".debug_pubnames"u8, SHT_PROGBITS, 0x0, 0x0, 0x821, 0x1b, 0x0, 0x0, 0x1, 0x0, 0x1b),
            new(".debug_info"u8, SHT_PROGBITS, 0x0, 0x0, 0x83c, 0x11d, 0x0, 0x0, 0x1, 0x0, 0x11d),
            new(".debug_abbrev"u8, SHT_PROGBITS, 0x0, 0x0, 0x959, 0x41, 0x0, 0x0, 0x1, 0x0, 0x41),
            new(".debug_line"u8, SHT_PROGBITS, 0x0, 0x0, 0x99a, 0x35, 0x0, 0x0, 0x1, 0x0, 0x35),
            new(".debug_frame"u8, SHT_PROGBITS, 0x0, 0x0, 0x9d0, 0x30, 0x0, 0x0, 0x4, 0x0, 0x30),
            new(".debug_str"u8, SHT_PROGBITS, 0x0, 0x0, 0xa00, 0xd, 0x0, 0x0, 0x1, 0x0, 0xd),
            new(".shstrtab"u8, SHT_STRTAB, 0x0, 0x0, 0xa0d, 0xf8, 0x0, 0x0, 0x1, 0x0, 0xf8),
            new(".symtab"u8, SHT_SYMTAB, 0x0, 0x0, 0xfb8, 0x4b0, 0x1d, 0x38, 0x4, 0x10, 0x4b0),
            new(".strtab"u8, SHT_STRTAB, 0x0, 0x0, 0x1468, 0x206, 0x0, 0x0, 0x1, 0x0, 0x206)
        }.slice(),
        new global::go.debug.elf_package.ProgHeader[]{
            new(PT_PHDR, PF_R + PF_X, 0x34, 0x8048034, 0x8048034, 0xa0, 0xa0, 0x4),
            new(PT_INTERP, PF_R, 0xd4, 0x80480d4, 0x80480d4, 0x15, 0x15, 0x1),
            new(PT_LOAD, PF_R + PF_X, 0x0, 0x8048000, 0x8048000, 0x5fb, 0x5fb, 0x1000),
            new(PT_LOAD, PF_R + PF_W, 0x5fc, 0x80495fc, 0x80495fc, 0xd8, 0xf8, 0x1000),
            new(PT_DYNAMIC, PF_R + PF_W, 0x60c, 0x804960c, 0x804960c, 0x98, 0x98, 0x4)
        }.slice(),
        new @string[]{"libc.so.6"u8}.slice(),
        new global::go.debug.elf_package.Symbol[]{
            new(""u8, 3, 0, false, 0, 1, 134512852, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 2, 134512876, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 3, 134513020, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 4, 134513292, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 5, 134513480, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 6, 134513512, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 7, 134513532, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 8, 134513612, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 9, 134513996, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 10, 134514008, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 11, 134518268, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 12, 134518280, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 13, 134518284, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 14, 134518436, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 15, 134518444, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 16, 134518452, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 17, 134518456, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 18, 134518484, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 19, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 20, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 21, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 22, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 23, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 24, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 25, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 26, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 27, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 28, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 29, 0, 0, ""u8, ""u8),
            new("crt1.c"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("/usr/src/lib/csu/i386-elf/crti.S"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("<command line>"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("<built-in>"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("/usr/src/lib/csu/i386-elf/crti.S"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("crtstuff.c"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("__CTOR_LIST__"u8, 1, 0, false, 0, 14, 134518436, 0, ""u8, ""u8),
            new("__DTOR_LIST__"u8, 1, 0, false, 0, 15, 134518444, 0, ""u8, ""u8),
            new("__EH_FRAME_BEGIN__"u8, 1, 0, false, 0, 12, 134518280, 0, ""u8, ""u8),
            new("__JCR_LIST__"u8, 1, 0, false, 0, 16, 134518452, 0, ""u8, ""u8),
            new("p.0"u8, 1, 0, false, 0, 11, 134518276, 0, ""u8, ""u8),
            new("completed.1"u8, 1, 0, false, 0, 18, 134518484, 1, ""u8, ""u8),
            new("__do_global_dtors_aux"u8, 2, 0, false, 0, 8, 134513760, 0, ""u8, ""u8),
            new("object.2"u8, 1, 0, false, 0, 18, 134518488, 24, ""u8, ""u8),
            new("frame_dummy"u8, 2, 0, false, 0, 8, 134513836, 0, ""u8, ""u8),
            new("crtstuff.c"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("__CTOR_END__"u8, 1, 0, false, 0, 14, 134518440, 0, ""u8, ""u8),
            new("__DTOR_END__"u8, 1, 0, false, 0, 15, 134518448, 0, ""u8, ""u8),
            new("__FRAME_END__"u8, 1, 0, false, 0, 12, 134518280, 0, ""u8, ""u8),
            new("__JCR_END__"u8, 1, 0, false, 0, 16, 134518452, 0, ""u8, ""u8),
            new("__do_global_ctors_aux"u8, 2, 0, false, 0, 8, 134513960, 0, ""u8, ""u8),
            new("/usr/src/lib/csu/i386-elf/crtn.S"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("<command line>"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("<built-in>"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("/usr/src/lib/csu/i386-elf/crtn.S"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("hello.c"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("printf"u8, 18, 0, false, 0, 0, 0, 44, ""u8, ""u8),
            new("_DYNAMIC"u8, 17, 0, false, 0, 65521, 134518284, 0, ""u8, ""u8),
            new("__dso_handle"u8, 17, 2, false, 0, 11, 134518272, 0, ""u8, ""u8),
            new("_init"u8, 18, 0, false, 0, 6, 134513512, 0, ""u8, ""u8),
            new("environ"u8, 17, 0, false, 0, 18, 134518512, 4, ""u8, ""u8),
            new("__deregister_frame_info"u8, 32, 0, false, 0, 0, 0, 0, ""u8, ""u8),
            new("__progname"u8, 17, 0, false, 0, 11, 134518268, 4, ""u8, ""u8),
            new("_start"u8, 18, 0, false, 0, 8, 134513612, 145, ""u8, ""u8),
            new("__bss_start"u8, 16, 0, false, 0, 65521, 134518484, 0, ""u8, ""u8),
            new("main"u8, 18, 0, false, 0, 8, 134513912, 46, ""u8, ""u8),
            new("_init_tls"u8, 18, 0, false, 0, 0, 0, 5, ""u8, ""u8),
            new("_fini"u8, 18, 0, false, 0, 9, 134513996, 0, ""u8, ""u8),
            new("atexit"u8, 18, 0, false, 0, 0, 0, 43, ""u8, ""u8),
            new("_edata"u8, 16, 0, false, 0, 65521, 134518484, 0, ""u8, ""u8),
            new("_GLOBAL_OFFSET_TABLE_"u8, 17, 0, false, 0, 65521, 134518456, 0, ""u8, ""u8),
            new("_end"u8, 16, 0, false, 0, 65521, 134518516, 0, ""u8, ""u8),
            new("exit"u8, 18, 0, false, 0, 0, 0, 68, ""u8, ""u8),
            new("_Jv_RegisterClasses"u8, 32, 0, false, 0, 0, 0, 0, ""u8, ""u8),
            new("__register_frame_info"u8, 32, 0, false, 0, 0, 0, 0, ""u8, ""u8)
        }.slice()
    ),
    new(
        "testdata/gcc-amd64-linux-exec"u8,
        new FileHeader(ELFCLASS64, ELFDATA2LSB, EV_CURRENT, ELFOSABI_NONE, 0, binary.LittleEndian, ET_EXEC, EM_X86_64, 0x4003e0),
        new global::go.debug.elf_package.SectionHeader[]{
            new(""u8, SHT_NULL, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0),
            new(".interp"u8, SHT_PROGBITS, SHF_ALLOC, 0x400200, 0x200, 0x1c, 0x0, 0x0, 0x1, 0x0, 0x1c),
            new(".note.ABI-tag"u8, SHT_NOTE, SHF_ALLOC, 0x40021c, 0x21c, 0x20, 0x0, 0x0, 0x4, 0x0, 0x20),
            new(".hash"u8, SHT_HASH, SHF_ALLOC, 0x400240, 0x240, 0x24, 0x5, 0x0, 0x8, 0x4, 0x24),
            new(".gnu.hash"u8, SHT_LOOS + 268435446, SHF_ALLOC, 0x400268, 0x268, 0x1c, 0x5, 0x0, 0x8, 0x0, 0x1c),
            new(".dynsym"u8, SHT_DYNSYM, SHF_ALLOC, 0x400288, 0x288, 0x60, 0x6, 0x1, 0x8, 0x18, 0x60),
            new(".dynstr"u8, SHT_STRTAB, SHF_ALLOC, 0x4002e8, 0x2e8, 0x3d, 0x0, 0x0, 0x1, 0x0, 0x3d),
            new(".gnu.version"u8, SHT_HIOS, SHF_ALLOC, 0x400326, 0x326, 0x8, 0x5, 0x0, 0x2, 0x2, 0x8),
            new(".gnu.version_r"u8, SHT_LOOS + 268435454, SHF_ALLOC, 0x400330, 0x330, 0x20, 0x6, 0x1, 0x8, 0x0, 0x20),
            new(".rela.dyn"u8, SHT_RELA, SHF_ALLOC, 0x400350, 0x350, 0x18, 0x5, 0x0, 0x8, 0x18, 0x18),
            new(".rela.plt"u8, SHT_RELA, SHF_ALLOC, 0x400368, 0x368, 0x30, 0x5, 0xc, 0x8, 0x18, 0x30),
            new(".init"u8, SHT_PROGBITS, SHF_ALLOC + SHF_EXECINSTR, 0x400398, 0x398, 0x18, 0x0, 0x0, 0x4, 0x0, 0x18),
            new(".plt"u8, SHT_PROGBITS, SHF_ALLOC + SHF_EXECINSTR, 0x4003b0, 0x3b0, 0x30, 0x0, 0x0, 0x4, 0x10, 0x30),
            new(".text"u8, SHT_PROGBITS, SHF_ALLOC + SHF_EXECINSTR, 0x4003e0, 0x3e0, 0x1b4, 0x0, 0x0, 0x10, 0x0, 0x1b4),
            new(".fini"u8, SHT_PROGBITS, SHF_ALLOC + SHF_EXECINSTR, 0x400594, 0x594, 0xe, 0x0, 0x0, 0x4, 0x0, 0xe),
            new(".rodata"u8, SHT_PROGBITS, SHF_ALLOC, 0x4005a4, 0x5a4, 0x11, 0x0, 0x0, 0x4, 0x0, 0x11),
            new(".eh_frame_hdr"u8, SHT_PROGBITS, SHF_ALLOC, 0x4005b8, 0x5b8, 0x24, 0x0, 0x0, 0x4, 0x0, 0x24),
            new(".eh_frame"u8, SHT_PROGBITS, SHF_ALLOC, 0x4005e0, 0x5e0, 0xa4, 0x0, 0x0, 0x8, 0x0, 0xa4),
            new(".ctors"u8, SHT_PROGBITS, SHF_WRITE + SHF_ALLOC, 0x600688, 0x688, 0x10, 0x0, 0x0, 0x8, 0x0, 0x10),
            new(".dtors"u8, SHT_PROGBITS, SHF_WRITE + SHF_ALLOC, 0x600698, 0x698, 0x10, 0x0, 0x0, 0x8, 0x0, 0x10),
            new(".jcr"u8, SHT_PROGBITS, SHF_WRITE + SHF_ALLOC, 0x6006a8, 0x6a8, 0x8, 0x0, 0x0, 0x8, 0x0, 0x8),
            new(".dynamic"u8, SHT_DYNAMIC, SHF_WRITE + SHF_ALLOC, 0x6006b0, 0x6b0, 0x1a0, 0x6, 0x0, 0x8, 0x10, 0x1a0),
            new(".got"u8, SHT_PROGBITS, SHF_WRITE + SHF_ALLOC, 0x600850, 0x850, 0x8, 0x0, 0x0, 0x8, 0x8, 0x8),
            new(".got.plt"u8, SHT_PROGBITS, SHF_WRITE + SHF_ALLOC, 0x600858, 0x858, 0x28, 0x0, 0x0, 0x8, 0x8, 0x28),
            new(".data"u8, SHT_PROGBITS, SHF_WRITE + SHF_ALLOC, 0x600880, 0x880, 0x18, 0x0, 0x0, 0x8, 0x0, 0x18),
            new(".bss"u8, SHT_NOBITS, SHF_WRITE + SHF_ALLOC, 0x600898, 0x898, 0x8, 0x0, 0x0, 0x4, 0x0, 0x8),
            new(".comment"u8, SHT_PROGBITS, 0x0, 0x0, 0x898, 0x126, 0x0, 0x0, 0x1, 0x0, 0x126),
            new(".debug_aranges"u8, SHT_PROGBITS, 0x0, 0x0, 0x9c0, 0x90, 0x0, 0x0, 0x10, 0x0, 0x90),
            new(".debug_pubnames"u8, SHT_PROGBITS, 0x0, 0x0, 0xa50, 0x25, 0x0, 0x0, 0x1, 0x0, 0x25),
            new(".debug_info"u8, SHT_PROGBITS, 0x0, 0x0, 0xa75, 0x1a7, 0x0, 0x0, 0x1, 0x0, 0x1a7),
            new(".debug_abbrev"u8, SHT_PROGBITS, 0x0, 0x0, 0xc1c, 0x6f, 0x0, 0x0, 0x1, 0x0, 0x6f),
            new(".debug_line"u8, SHT_PROGBITS, 0x0, 0x0, 0xc8b, 0x13f, 0x0, 0x0, 0x1, 0x0, 0x13f),
            new(".debug_str"u8, SHT_PROGBITS, SHF_MERGE + SHF_STRINGS, 0x0, 0xdca, 0xb1, 0x0, 0x0, 0x1, 0x1, 0xb1),
            new(".debug_ranges"u8, SHT_PROGBITS, 0x0, 0x0, 0xe80, 0x90, 0x0, 0x0, 0x10, 0x0, 0x90),
            new(".shstrtab"u8, SHT_STRTAB, 0x0, 0x0, 0xf10, 0x149, 0x0, 0x0, 0x1, 0x0, 0x149),
            new(".symtab"u8, SHT_SYMTAB, 0x0, 0x0, 0x19a0, 0x6f0, 0x24, 0x39, 0x8, 0x18, 0x6f0),
            new(".strtab"u8, SHT_STRTAB, 0x0, 0x0, 0x2090, 0x1fc, 0x0, 0x0, 0x1, 0x0, 0x1fc)
        }.slice(),
        new global::go.debug.elf_package.ProgHeader[]{
            new(PT_PHDR, PF_R + PF_X, 0x40, 0x400040, 0x400040, 0x1c0, 0x1c0, 0x8),
            new(PT_INTERP, PF_R, 0x200, 0x400200, 0x400200, 0x1c, 0x1c, 1),
            new(PT_LOAD, PF_R + PF_X, 0x0, 0x400000, 0x400000, 0x684, 0x684, 0x200000),
            new(PT_LOAD, PF_R + PF_W, 0x688, 0x600688, 0x600688, 0x210, 0x218, 0x200000),
            new(PT_DYNAMIC, PF_R + PF_W, 0x6b0, 0x6006b0, 0x6006b0, 0x1a0, 0x1a0, 0x8),
            new(PT_NOTE, PF_R, 0x21c, 0x40021c, 0x40021c, 0x20, 0x20, 0x4),
            new(PT_LOOS + 0x474E550, PF_R, 0x5b8, 0x4005b8, 0x4005b8, 0x24, 0x24, 0x4),
            new(PT_LOOS + 0x474E551, PF_R + PF_W, 0x0, 0x0, 0x0, 0x0, 0x0, 0x8)
        }.slice(),
        new @string[]{"libc.so.6"u8}.slice(),
        new global::go.debug.elf_package.Symbol[]{
            new(""u8, 3, 0, false, 0, 1, 4194816, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 2, 4194844, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 3, 4194880, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 4, 4194920, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 5, 4194952, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 6, 4195048, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 7, 4195110, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 8, 4195120, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 9, 4195152, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 10, 4195176, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 11, 4195224, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 12, 4195248, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 13, 4195296, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 14, 4195732, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 15, 4195748, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 16, 4195768, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 17, 4195808, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 18, 6293128, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 19, 6293144, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 20, 6293160, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 21, 6293168, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 22, 6293584, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 23, 6293592, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 24, 6293632, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 25, 6293656, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 26, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 27, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 28, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 29, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 30, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 31, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 32, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 33, 0, 0, ""u8, ""u8),
            new("init.c"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("initfini.c"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("call_gmon_start"u8, 2, 0, false, 0, 13, 4195340, 0, ""u8, ""u8),
            new("crtstuff.c"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("__CTOR_LIST__"u8, 1, 0, false, 0, 18, 6293128, 0, ""u8, ""u8),
            new("__DTOR_LIST__"u8, 1, 0, false, 0, 19, 6293144, 0, ""u8, ""u8),
            new("__JCR_LIST__"u8, 1, 0, false, 0, 20, 6293160, 0, ""u8, ""u8),
            new("__do_global_dtors_aux"u8, 2, 0, false, 0, 13, 4195376, 0, ""u8, ""u8),
            new("completed.6183"u8, 1, 0, false, 0, 25, 6293656, 1, ""u8, ""u8),
            new("p.6181"u8, 1, 0, false, 0, 24, 6293648, 0, ""u8, ""u8),
            new("frame_dummy"u8, 2, 0, false, 0, 13, 4195440, 0, ""u8, ""u8),
            new("crtstuff.c"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("__CTOR_END__"u8, 1, 0, false, 0, 18, 6293136, 0, ""u8, ""u8),
            new("__DTOR_END__"u8, 1, 0, false, 0, 19, 6293152, 0, ""u8, ""u8),
            new("__FRAME_END__"u8, 1, 0, false, 0, 17, 4195968, 0, ""u8, ""u8),
            new("__JCR_END__"u8, 1, 0, false, 0, 20, 6293160, 0, ""u8, ""u8),
            new("__do_global_ctors_aux"u8, 2, 0, false, 0, 13, 4195680, 0, ""u8, ""u8),
            new("initfini.c"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("hello.c"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new("_GLOBAL_OFFSET_TABLE_"u8, 1, 2, false, 0, 23, 6293592, 0, ""u8, ""u8),
            new("__init_array_end"u8, 0, 2, false, 0, 18, 6293124, 0, ""u8, ""u8),
            new("__init_array_start"u8, 0, 2, false, 0, 18, 6293124, 0, ""u8, ""u8),
            new("_DYNAMIC"u8, 1, 2, false, 0, 21, 6293168, 0, ""u8, ""u8),
            new("data_start"u8, 32, 0, false, 0, 24, 6293632, 0, ""u8, ""u8),
            new("__libc_csu_fini"u8, 18, 0, false, 0, 13, 4195520, 2, ""u8, ""u8),
            new("_start"u8, 18, 0, false, 0, 13, 4195296, 0, ""u8, ""u8),
            new("__gmon_start__"u8, 32, 0, false, 0, 0, 0, 0, ""u8, ""u8),
            new("_Jv_RegisterClasses"u8, 32, 0, false, 0, 0, 0, 0, ""u8, ""u8),
            new("puts@@GLIBC_2.2.5"u8, 18, 0, false, 0, 0, 0, 396, ""u8, ""u8),
            new("_fini"u8, 18, 0, false, 0, 14, 4195732, 0, ""u8, ""u8),
            new("__libc_start_main@@GLIBC_2.2.5"u8, 18, 0, false, 0, 0, 0, 450, ""u8, ""u8),
            new("_IO_stdin_used"u8, 17, 0, false, 0, 15, 4195748, 4, ""u8, ""u8),
            new("__data_start"u8, 16, 0, false, 0, 24, 6293632, 0, ""u8, ""u8),
            new("__dso_handle"u8, 17, 2, false, 0, 24, 6293640, 0, ""u8, ""u8),
            new("__libc_csu_init"u8, 18, 0, false, 0, 13, 4195536, 137, ""u8, ""u8),
            new("__bss_start"u8, 16, 0, false, 0, 65521, 6293656, 0, ""u8, ""u8),
            new("_end"u8, 16, 0, false, 0, 65521, 6293664, 0, ""u8, ""u8),
            new("_edata"u8, 16, 0, false, 0, 65521, 6293656, 0, ""u8, ""u8),
            new("main"u8, 18, 0, false, 0, 13, 4195480, 27, ""u8, ""u8),
            new("_init"u8, 18, 0, false, 0, 11, 4195224, 0, ""u8, ""u8)
        }.slice()
    ),
    new(
        "testdata/hello-world-core.gz"u8,
        new FileHeader(ELFCLASS64, ELFDATA2LSB, EV_CURRENT, ELFOSABI_NONE, 0x0, binary.LittleEndian, ET_CORE, EM_X86_64, 0x0),
        new global::go.debug.elf_package.SectionHeader[]{}.slice(),
        new global::go.debug.elf_package.ProgHeader[]{
            new(Type: PT_NOTE, Flags: 0x0, Off: 0x3f8, Vaddr: 0x0, Paddr: 0x0, Filesz: 0x8ac, Memsz: 0x0, Align: 0x0),
            new(Type: PT_LOAD, Flags: PF_X + PF_R, Off: 0x1000, Vaddr: 0x400000, Paddr: 0x0, Filesz: 0x0, Memsz: 0x1000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_R, Off: 0x1000, Vaddr: 0x401000, Paddr: 0x0, Filesz: 0x1000, Memsz: 0x1000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_W + PF_R, Off: 0x2000, Vaddr: 0x402000, Paddr: 0x0, Filesz: 0x1000, Memsz: 0x1000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_X + PF_R, Off: 0x3000, Vaddr: 0x7f54078b8000UL, Paddr: 0x0, Filesz: 0x0, Memsz: 0x1b5000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: 0x0, Off: 0x3000, Vaddr: 0x7f5407a6d000UL, Paddr: 0x0, Filesz: 0x0, Memsz: 0x1ff000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_R, Off: 0x3000, Vaddr: 0x7f5407c6c000UL, Paddr: 0x0, Filesz: 0x4000, Memsz: 0x4000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_W + PF_R, Off: 0x7000, Vaddr: 0x7f5407c70000UL, Paddr: 0x0, Filesz: 0x2000, Memsz: 0x2000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_W + PF_R, Off: 0x9000, Vaddr: 0x7f5407c72000UL, Paddr: 0x0, Filesz: 0x5000, Memsz: 0x5000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_X + PF_R, Off: 0xe000, Vaddr: 0x7f5407c77000UL, Paddr: 0x0, Filesz: 0x0, Memsz: 0x22000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_W + PF_R, Off: 0xe000, Vaddr: 0x7f5407e81000UL, Paddr: 0x0, Filesz: 0x3000, Memsz: 0x3000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_W + PF_R, Off: 0x11000, Vaddr: 0x7f5407e96000UL, Paddr: 0x0, Filesz: 0x3000, Memsz: 0x3000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_R, Off: 0x14000, Vaddr: 0x7f5407e99000UL, Paddr: 0x0, Filesz: 0x1000, Memsz: 0x1000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_W + PF_R, Off: 0x15000, Vaddr: 0x7f5407e9a000UL, Paddr: 0x0, Filesz: 0x2000, Memsz: 0x2000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_W + PF_R, Off: 0x17000, Vaddr: 0x7fff79972000UL, Paddr: 0x0, Filesz: 0x23000, Memsz: 0x23000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_X + PF_R, Off: 0x3a000, Vaddr: 0x7fff799f8000UL, Paddr: 0x0, Filesz: 0x1000, Memsz: 0x1000, Align: 0x1000),
            new(Type: PT_LOAD, Flags: PF_X + PF_R, Off: 0x3b000, Vaddr: 0xffffffffff600000UL, Paddr: 0x0, Filesz: 0x1000, Memsz: 0x1000, Align: 0x1000)
        }.slice(),
        default!,
        default!
    ),
    new(
        "testdata/compressed-32.obj"u8,
        new FileHeader(ELFCLASS32, ELFDATA2LSB, EV_CURRENT, ELFOSABI_NONE, 0x0, binary.LittleEndian, ET_REL, EM_386, 0x0),
        new global::go.debug.elf_package.SectionHeader[]{
            new(""u8, SHT_NULL, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0),
            new(".text"u8, SHT_PROGBITS, (global::go.debug.elf_package.SectionFlag)(SHF_ALLOC | SHF_EXECINSTR), 0x0, 0x34, 0x17, 0x0, 0x0, 0x1, 0x0, 0x17),
            new(".rel.text"u8, SHT_REL, SHF_INFO_LINK, 0x0, 0x3dc, 0x10, 0x13, 0x1, 0x4, 0x8, 0x10),
            new(".data"u8, SHT_PROGBITS, (global::go.debug.elf_package.SectionFlag)(SHF_WRITE | SHF_ALLOC), 0x0, 0x4b, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0),
            new(".bss"u8, SHT_NOBITS, (global::go.debug.elf_package.SectionFlag)(SHF_WRITE | SHF_ALLOC), 0x0, 0x4b, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0),
            new(".rodata"u8, SHT_PROGBITS, SHF_ALLOC, 0x0, 0x4b, 0xd, 0x0, 0x0, 0x1, 0x0, 0xd),
            new(".debug_info"u8, SHT_PROGBITS, SHF_COMPRESSED, 0x0, 0x58, 0xb4, 0x0, 0x0, 0x1, 0x0, 0x84),
            new(".rel.debug_info"u8, SHT_REL, SHF_INFO_LINK, 0x0, 0x3ec, 0xa0, 0x13, 0x6, 0x4, 0x8, 0xa0),
            new(".debug_abbrev"u8, SHT_PROGBITS, 0x0, 0x0, 0xdc, 0x5a, 0x0, 0x0, 0x1, 0x0, 0x5a),
            new(".debug_aranges"u8, SHT_PROGBITS, 0x0, 0x0, 0x136, 0x20, 0x0, 0x0, 0x1, 0x0, 0x20),
            new(".rel.debug_aranges"u8, SHT_REL, SHF_INFO_LINK, 0x0, 0x48c, 0x10, 0x13, 0x9, 0x4, 0x8, 0x10),
            new(".debug_line"u8, SHT_PROGBITS, 0x0, 0x0, 0x156, 0x5c, 0x0, 0x0, 0x1, 0x0, 0x5c),
            new(".rel.debug_line"u8, SHT_REL, SHF_INFO_LINK, 0x0, 0x49c, 0x8, 0x13, 0xb, 0x4, 0x8, 0x8),
            new(".debug_str"u8, SHT_PROGBITS, (global::go.debug.elf_package.SectionFlag)((global::go.debug.elf_package.SectionFlag)(SHF_MERGE | SHF_STRINGS) | SHF_COMPRESSED), 0x0, 0x1b2, 0x10f, 0x0, 0x0, 0x1, 0x1, 0xb3),
            new(".comment"u8, SHT_PROGBITS, (global::go.debug.elf_package.SectionFlag)(SHF_MERGE | SHF_STRINGS), 0x0, 0x265, 0x2a, 0x0, 0x0, 0x1, 0x1, 0x2a),
            new(".note.GNU-stack"u8, SHT_PROGBITS, 0x0, 0x0, 0x28f, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0),
            new(".eh_frame"u8, SHT_PROGBITS, SHF_ALLOC, 0x0, 0x290, 0x38, 0x0, 0x0, 0x4, 0x0, 0x38),
            new(".rel.eh_frame"u8, SHT_REL, SHF_INFO_LINK, 0x0, 0x4a4, 0x8, 0x13, 0x10, 0x4, 0x8, 0x8),
            new(".shstrtab"u8, SHT_STRTAB, 0x0, 0x0, 0x4ac, 0xab, 0x0, 0x0, 0x1, 0x0, 0xab),
            new(".symtab"u8, SHT_SYMTAB, 0x0, 0x0, 0x2c8, 0x100, 0x14, 0xe, 0x4, 0x10, 0x100),
            new(".strtab"u8, SHT_STRTAB, 0x0, 0x0, 0x3c8, 0x13, 0x0, 0x0, 0x1, 0x0, 0x13)
        }.slice(),
        new global::go.debug.elf_package.ProgHeader[]{}.slice(),
        default!,
        new global::go.debug.elf_package.Symbol[]{
            new("hello.c"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 1, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 3, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 4, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 5, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 6, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 8, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 9, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 11, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 13, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 15, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 16, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 14, 0, 0, ""u8, ""u8),
            new("main"u8, 18, 0, false, 0, 1, 0, 23, ""u8, ""u8),
            new("puts"u8, 16, 0, false, 0, 0, 0, 0, ""u8, ""u8)
        }.slice()
    ),
    new(
        "testdata/compressed-64.obj"u8,
        new FileHeader(ELFCLASS64, ELFDATA2LSB, EV_CURRENT, ELFOSABI_NONE, 0x0, binary.LittleEndian, ET_REL, EM_X86_64, 0x0),
        new global::go.debug.elf_package.SectionHeader[]{
            new(""u8, SHT_NULL, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0),
            new(".text"u8, SHT_PROGBITS, (global::go.debug.elf_package.SectionFlag)(SHF_ALLOC | SHF_EXECINSTR), 0x0, 0x40, 0x1b, 0x0, 0x0, 0x1, 0x0, 0x1b),
            new(".rela.text"u8, SHT_RELA, SHF_INFO_LINK, 0x0, 0x488, 0x30, 0x13, 0x1, 0x8, 0x18, 0x30),
            new(".data"u8, SHT_PROGBITS, (global::go.debug.elf_package.SectionFlag)(SHF_WRITE | SHF_ALLOC), 0x0, 0x5b, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0),
            new(".bss"u8, SHT_NOBITS, (global::go.debug.elf_package.SectionFlag)(SHF_WRITE | SHF_ALLOC), 0x0, 0x5b, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0),
            new(".rodata"u8, SHT_PROGBITS, SHF_ALLOC, 0x0, 0x5b, 0xd, 0x0, 0x0, 0x1, 0x0, 0xd),
            new(".debug_info"u8, SHT_PROGBITS, SHF_COMPRESSED, 0x0, 0x68, 0xba, 0x0, 0x0, 0x1, 0x0, 0x72),
            new(".rela.debug_info"u8, SHT_RELA, SHF_INFO_LINK, 0x0, 0x4b8, 0x1c8, 0x13, 0x6, 0x8, 0x18, 0x1c8),
            new(".debug_abbrev"u8, SHT_PROGBITS, 0x0, 0x0, 0xda, 0x5c, 0x0, 0x0, 0x1, 0x0, 0x5c),
            new(".debug_aranges"u8, SHT_PROGBITS, SHF_COMPRESSED, 0x0, 0x136, 0x30, 0x0, 0x0, 0x1, 0x0, 0x2f),
            new(".rela.debug_aranges"u8, SHT_RELA, SHF_INFO_LINK, 0x0, 0x680, 0x30, 0x13, 0x9, 0x8, 0x18, 0x30),
            new(".debug_line"u8, SHT_PROGBITS, 0x0, 0x0, 0x165, 0x60, 0x0, 0x0, 0x1, 0x0, 0x60),
            new(".rela.debug_line"u8, SHT_RELA, SHF_INFO_LINK, 0x0, 0x6b0, 0x18, 0x13, 0xb, 0x8, 0x18, 0x18),
            new(".debug_str"u8, SHT_PROGBITS, (global::go.debug.elf_package.SectionFlag)((global::go.debug.elf_package.SectionFlag)(SHF_MERGE | SHF_STRINGS) | SHF_COMPRESSED), 0x0, 0x1c5, 0x104, 0x0, 0x0, 0x1, 0x1, 0xc3),
            new(".comment"u8, SHT_PROGBITS, (global::go.debug.elf_package.SectionFlag)(SHF_MERGE | SHF_STRINGS), 0x0, 0x288, 0x2a, 0x0, 0x0, 0x1, 0x1, 0x2a),
            new(".note.GNU-stack"u8, SHT_PROGBITS, 0x0, 0x0, 0x2b2, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0),
            new(".eh_frame"u8, SHT_PROGBITS, SHF_ALLOC, 0x0, 0x2b8, 0x38, 0x0, 0x0, 0x8, 0x0, 0x38),
            new(".rela.eh_frame"u8, SHT_RELA, SHF_INFO_LINK, 0x0, 0x6c8, 0x18, 0x13, 0x10, 0x8, 0x18, 0x18),
            new(".shstrtab"u8, SHT_STRTAB, 0x0, 0x0, 0x6e0, 0xb0, 0x0, 0x0, 0x1, 0x0, 0xb0),
            new(".symtab"u8, SHT_SYMTAB, 0x0, 0x0, 0x2f0, 0x180, 0x14, 0xe, 0x8, 0x18, 0x180),
            new(".strtab"u8, SHT_STRTAB, 0x0, 0x0, 0x470, 0x13, 0x0, 0x0, 0x1, 0x0, 0x13)
        }.slice(),
        new global::go.debug.elf_package.ProgHeader[]{}.slice(),
        default!,
        new global::go.debug.elf_package.Symbol[]{
            new("hello.c"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 1, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 3, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 4, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 5, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 6, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 8, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 9, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 11, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 13, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 15, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 16, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 14, 0, 0, ""u8, ""u8),
            new("main"u8, 18, 0, false, 0, 1, 0, 27, ""u8, ""u8),
            new("puts"u8, 16, 0, false, 0, 0, 0, 0, ""u8, ""u8)
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc620-sparc64.obj"u8,
        new FileHeader(Class: ELFCLASS64, Data: ELFDATA2MSB, Version: EV_CURRENT, OSABI: ELFOSABI_NONE, ABIVersion: 0x0, ByteOrder: binary.BigEndian, Type: ET_REL, Machine: EM_SPARCV9, Entry: 0x0),
        new global::go.debug.elf_package.SectionHeader[]{
            new(""u8, SHT_NULL, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0),
            new(".text"u8, SHT_PROGBITS, SHF_ALLOC + SHF_EXECINSTR, 0x0, 0x40, 0x2c, 0x0, 0x0, 0x4, 0x0, 0x2c),
            new(".rela.text"u8, SHT_RELA, SHF_INFO_LINK, 0x0, 0xa58, 0x48, 0x13, 0x1, 0x8, 0x18, 0x48),
            new(".data"u8, SHT_PROGBITS, SHF_WRITE + SHF_ALLOC, 0x0, 0x6c, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0),
            new(".bss"u8, SHT_NOBITS, SHF_WRITE + SHF_ALLOC, 0x0, 0x6c, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0),
            new(".rodata"u8, SHT_PROGBITS, SHF_ALLOC, 0x0, 0x70, 0xd, 0x0, 0x0, 0x8, 0x0, 0xd),
            new(".debug_info"u8, SHT_PROGBITS, 0x0, 0x0, 0x7d, 0x346, 0x0, 0x0, 0x1, 0x0, 0x346),
            new(".rela.debug_info"u8, SHT_RELA, SHF_INFO_LINK, 0x0, 0xaa0, 0x630, 0x13, 0x6, 0x8, 0x18, 0x630),
            new(".debug_abbrev"u8, SHT_PROGBITS, 0x0, 0x0, 0x3c3, 0xf1, 0x0, 0x0, 0x1, 0x0, 0xf1),
            new(".debug_aranges"u8, SHT_PROGBITS, 0x0, 0x0, 0x4b4, 0x30, 0x0, 0x0, 0x1, 0x0, 0x30),
            new(".rela.debug_aranges"u8, SHT_RELA, SHF_INFO_LINK, 0x0, 0x10d0, 0x30, 0x13, 0x9, 0x8, 0x18, 0x30),
            new(".debug_line"u8, SHT_PROGBITS, 0x0, 0x0, 0x4e4, 0xd3, 0x0, 0x0, 0x1, 0x0, 0xd3),
            new(".rela.debug_line"u8, SHT_RELA, SHF_INFO_LINK, 0x0, 0x1100, 0x18, 0x13, 0xb, 0x8, 0x18, 0x18),
            new(".debug_str"u8, SHT_PROGBITS, SHF_MERGE + SHF_STRINGS, 0x0, 0x5b7, 0x2a3, 0x0, 0x0, 0x1, 0x1, 0x2a3),
            new(".comment"u8, SHT_PROGBITS, SHF_MERGE + SHF_STRINGS, 0x0, 0x85a, 0x2e, 0x0, 0x0, 0x1, 0x1, 0x2e),
            new(".note.GNU-stack"u8, SHT_PROGBITS, 0x0, 0x0, 0x888, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0),
            new(".debug_frame"u8, SHT_PROGBITS, 0x0, 0x0, 0x888, 0x38, 0x0, 0x0, 0x8, 0x0, 0x38),
            new(".rela.debug_frame"u8, SHT_RELA, SHF_INFO_LINK, 0x0, 0x1118, 0x30, 0x13, 0x10, 0x8, 0x18, 0x30),
            new(".shstrtab"u8, SHT_STRTAB, 0x0, 0x0, 0x1148, 0xb3, 0x0, 0x0, 0x1, 0x0, 0xb3),
            new(".symtab"u8, SHT_SYMTAB, 0x0, 0x0, 0x8c0, 0x180, 0x14, 0xe, 0x8, 0x18, 0x180),
            new(".strtab"u8, SHT_STRTAB, 0x0, 0x0, 0xa40, 0x13, 0x0, 0x0, 0x1, 0x0, 0x13)
        }.slice(),
        new global::go.debug.elf_package.ProgHeader[]{}.slice(),
        default!,
        new global::go.debug.elf_package.Symbol[]{
            new("hello.c"u8, 4, 0, false, 0, 65521, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 1, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 3, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 4, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 5, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 6, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 8, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 9, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 11, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 13, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 15, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 16, 0, 0, ""u8, ""u8),
            new(""u8, 3, 0, false, 0, 14, 0, 0, ""u8, ""u8),
            new("main"u8, 18, 0, false, 0, 1, 0, 44, ""u8, ""u8),
            new("puts"u8, 16, 0, false, 0, 0, 0, 0, ""u8, ""u8)
        }.slice()
    )
}.slice());
internal static ref slice<fileTest> fileTests => ref ᏑfileTests.ValueSlot;

public static void TestOpen(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        foreach (var (i, _) in fileTests) {
            var tt = Ꮡ(fileTests, i);
            ж<global::go.debug.elf_package.File> f = default!;
            error err = default!;
            if (path.Ext((~tt).@file) == ".gz"u8){
                io.ReaderAt r = default!;
                {
                    (r, err) = decompress((~tt).@file); if (err == default!) {
                        (f, err) = NewFile(r);
                    }
                }
            } else {
                (f, err) = Open((~tt).@file);
            }
            if (err != default!) {
                Ꮡt.Errorf("cannot open file %s: %v"u8, (~tt).@file, err);
                continue;
            }
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);
            if ((~f).FileHeader != (~tt).hdr) {
                Ꮡt.Errorf("open %s:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, (~f).FileHeader, (~tt).hdr);
                continue;
            }
            foreach (var (iΔ1, s) in (~f).Sections) {
                if (iΔ1 >= len((~tt).sections)) {
                    break;
                }
                var sh = (~tt).sections[iΔ1];
                if ((~s).SectionHeader != sh) {
                    Ꮡt.Errorf("open %s, section %d:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, iΔ1, (~s).SectionHeader, sh);
                }
            }
            foreach (var (iΔ2, p) in (~f).Progs) {
                if (iΔ2 >= len((~tt).progs)) {
                    break;
                }
                var ph = (~tt).progs[iΔ2];
                if ((~p).ProgHeader != ph) {
                    Ꮡt.Errorf("open %s, program %d:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, iΔ2, (~p).ProgHeader, ph);
                }
            }
            nint tn = len((~tt).sections);
            nint fn = len((~f).Sections);
            if (tn != fn) {
                Ꮡt.Errorf("open %s: len(Sections) = %d, want %d"u8, (~tt).@file, fn, tn);
            }
            tn = len((~tt).progs);
            fn = len((~f).Progs);
            if (tn != fn) {
                Ꮡt.Errorf("open %s: len(Progs) = %d, want %d"u8, (~tt).@file, fn, tn);
            }
            var tl = tt.Value.needed;
            (var fl, err) = f.ImportedLibraries();
            if (err != default!) {
                Ꮡt.Error(err);
            }
            if (!reflect.DeepEqual(tl, fl)) {
                Ꮡt.Errorf("open %s: DT_NEEDED = %v, want %v"u8, (~tt).@file, tl, fl);
            }
            (var symbols, err) = f.Symbols();
            if ((~tt).symbols == default!){
                if (!errors.Is(err, ErrNoSymbols)) {
                    Ꮡt.Errorf("open %s: Symbols() expected ErrNoSymbols, have nil"u8, (~tt).@file);
                }
                if (symbols != default!) {
                    Ꮡt.Errorf("open %s: Symbols() expected no symbols, have %v"u8, (~tt).@file, symbols);
                }
            } else {
                if (err != default!) {
                    Ꮡt.Errorf("open %s: Symbols() unexpected error %v"u8, (~tt).@file, err);
                }
                if (!slices.Equal<slice<global::go.debug.elf_package.Symbol>, global::go.debug.elf_package.Symbol>(symbols, (~tt).symbols)) {
                    Ꮡt.Errorf("open %s: Symbols() = %v, want %v"u8, (~tt).@file, symbols, (~tt).symbols);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// elf.NewFile requires io.ReaderAt, which compress/gzip cannot
// provide. Decompress the file to a bytes.Reader.
internal static (io.ReaderAt, error) decompress(@string gz) {
    GoFrame ᒐ = default;
    try {
        var (@in, err) = os.Open(gz);
        if (err != default!) {
            return (default!, err);
        }
        var inʗ1 = @in;
        defer(() => inʗ1.Close(), ref ᒐ);
        (var r, err) = gzip.NewReader(new elf_internal_test_package.os_FileжReader(@in));
        if (err != default!) {
            return (default!, err);
        }
        ref var @out = ref heap(new bytes.Buffer(), out var Ꮡout);
        (_, err) = io.Copy(new elf_internal_test_package.bytes_BufferжWriter(Ꮡout), new elf_internal_test_package.gzip_ReaderжReader(r));
        return (new elf_internal_test_package.bytes_ReaderжReaderAt(bytes.NewReader(@out.Bytes())), err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct relocationTestEntry {
    internal nint entryNumber;
    internal ж<dwarf.Entry> entry;
    [GoArrayDims(2)]
    internal slice<array<uint64>> pcRanges;
}

[GoType] internal partial struct relocationTest {
    internal @string @file;
    internal slice<relocationTestEntry> entries;
}

internal static slice<relocationTest> relocationTests = new relocationTest[]{
    new(
        "testdata/go-relocation-test-gcc441-x86-64.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C 4.4.1"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)1, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"go-relocation-test.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (uint64)0x6, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x6}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc441-x86.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C 4.4.1"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)1, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"t.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (uint64)0x5, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x5}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc424-x86-64.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C 4.2.4 (Ubuntu 4.2.4-1ubuntu4)"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)1, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"go-relocation-test-gcc424.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (uint64)0x6, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x6}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc482-aarch64.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C 4.8.2 -g -fstack-protector"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)1, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"go-relocation-test-gcc482.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (int64)0x24, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x24}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc492-arm.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C 4.9.2 20141224 (prerelease) -march=armv7-a -mfloat-abi=hard -mfpu=vfpv3-d16 -mtls-dialect=gnu -g"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)1, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"go-relocation-test-gcc492.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/root/go/src/debug/elf/testdata"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (int64)0x28, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x28}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-clang-arm.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"Debian clang version 3.5.0-10 (tags/RELEASE_350/final) (based on LLVM 3.5.0)"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)12, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"hello.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0x0, Class: dwarf.ClassLinePtr),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (int64)0x30, Class: dwarf.ClassConstant)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x30}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc5-ppc.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C11 5.0.0 20150116 (experimental) -Asystem=linux -Asystem=unix -Asystem=posix -g"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)12, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"go-relocation-test-gcc5-ppc.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (int64)0x44, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x44}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc482-ppc64le.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C 4.8.2 -Asystem=linux -Asystem=unix -Asystem=posix -msecure-plt -mtune=power8 -mcpu=power7 -gdwarf-2 -fstack-protector"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)1, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"go-relocation-test-gcc482-ppc64le.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (uint64)0x24, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x24}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc492-mips64.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C 4.9.2 -meb -mabi=64 -march=mips3 -mtune=mips64 -mllsc -mno-shared -g"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)1, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"hello.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (int64)0x64, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x64}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc531-s390x.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C11 5.3.1 20160316 -march=zEC12 -m64 -mzarch -g -fstack-protector-strong"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)12, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"hello.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (int64)0x3a, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x3a}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc620-sparc64.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C11 6.2.0 20160914 -mcpu=v9 -g -fstack-protector-strong"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)12, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"hello.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (int64)0x2c, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x2c}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc492-mipsle.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C 4.9.2 -mel -march=mips2 -mtune=mips32 -mllsc -mno-shared -mabi=32 -g"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)1, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"hello.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (int64)0x58, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x58}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc540-mips.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C11 5.4.0 20160609 -meb -mips32 -mtune=mips32r2 -mfpxx -mllsc -mno-shared -mabi=32 -g -gdwarf-2"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)12, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"hello.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (uint64)0x5c, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x5c}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc493-mips64le.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C 4.9.3 -mel -mabi=64 -mllsc -mno-shared -g -fstack-protector-strong"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)1, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"hello.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (int64)0x64, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x64}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc720-riscv64.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C11 7.2.0 -march=rv64imafdc -mabi=lp64d -g -gdwarf-2"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)12, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"hello.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0x0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrHighpc, Val: (uint64)0x2c, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0, 0x2c}.array()}.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-clang-x86.obj"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"clang version google3-trunk (trunk r209387)"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)12, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"go-relocation-test-clang.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString)
                    }.slice()
                ))
            )
        }.slice()
    ),
    new(
        "testdata/gcc-amd64-openbsd-debug-with-rela.obj"u8,
        new relocationTestEntry[]{
            new(
                entryNumber: 203,
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xc62,
                    Tag: dwarf.TagMember,
                    Children: false,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrName, Val: (@string)"it_interval"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrDeclFile, Val: (int64)7, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrDeclLine, Val: (int64)236, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrType, Val: ((dwarf.Offset)0xb7f), Class: dwarf.ClassReference),
                        new(Attr: dwarf.AttrDataMemberLoc, Val: new byte[]{0x23, 0x0}.slice(), Class: dwarf.ClassExprLoc)
                    }.slice()
                ))
            ),
            new(
                entryNumber: 204,
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xc70,
                    Tag: dwarf.TagMember,
                    Children: false,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrName, Val: (@string)"it_value"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrDeclFile, Val: (int64)7, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrDeclLine, Val: (int64)237, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrType, Val: ((dwarf.Offset)0xb7f), Class: dwarf.ClassReference),
                        new(Attr: dwarf.AttrDataMemberLoc, Val: new byte[]{0x23, 0x10}.slice(), Class: dwarf.ClassExprLoc)
                    }.slice()
                ))
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc930-ranges-no-rela-x86-64"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C17 9.3.0 -mtune=generic -march=x86-64 -g -fno-asynchronous-unwind-tables"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)12, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"multiple-code-sections.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrRanges, Val: (int64)0, Class: dwarf.ClassRangeListPtr),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{
                    new uint64[]{0x765, 0x777}.array(),
                    new uint64[]{0x7e1, 0x7ec}.array()
                }.slice(), 2)
            )
        }.slice()
    ),
    new(
        "testdata/go-relocation-test-gcc930-ranges-with-rela-x86-64"u8,
        new relocationTestEntry[]{
            new(
                entry: Ꮡ(new dwarf.Entry(
                    Offset: 0xb,
                    Tag: dwarf.TagCompileUnit,
                    Children: true,
                    Field: new dwarf.Field[]{
                        new(Attr: dwarf.AttrProducer, Val: (@string)"GNU C17 9.3.0 -mtune=generic -march=x86-64 -g -fno-asynchronous-unwind-tables"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrLanguage, Val: (int64)12, Class: dwarf.ClassConstant),
                        new(Attr: dwarf.AttrName, Val: (@string)"multiple-code-sections.c"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrCompDir, Val: (@string)"/tmp"u8, Class: dwarf.ClassString),
                        new(Attr: dwarf.AttrRanges, Val: (int64)0, Class: dwarf.ClassRangeListPtr),
                        new(Attr: dwarf.AttrLowpc, Val: (uint64)0, Class: dwarf.ClassAddress),
                        new(Attr: dwarf.AttrStmtList, Val: (int64)0, Class: dwarf.ClassLinePtr)
                    }.slice()
                )),
                pcRanges: GoReflect.WithElemDims(new array<uint64>[]{
                    new uint64[]{0x765, 0x777}.array(),
                    new uint64[]{0x7e1, 0x7ec}.array()
                }.slice(), 2)
            )
        }.slice()
    )
}.slice();

public static void TestDWARFRelocations(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in relocationTests) {
        ref var testΔ1 = ref heap<relocationTest>(out var ᏑtestΔ1);
        testΔ1 = test;
        var testʗ1 = testΔ1;
        Ꮡt.Run(testΔ1.@file, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            var (f, err) = Open(testʗ1.@file);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            (var dwarf, err) = f.DWARF();
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var reader = dwarf.Reader();
            nint idx = 0;
            foreach (var (_, testEntry) in testʗ1.entries) {
                if (testEntry.entryNumber < idx) {
                    tΔ1.Fatalf("internal test error: %d < %d"u8, testEntry.entryNumber, idx);
                }
                for (; idx < testEntry.entryNumber; idx++) {
                    var (entryΔ1, errΔ1) = reader.Next();
                    if (entryΔ1 == nil || errΔ1 != default!) {
                        tΔ1.Fatalf("Failed to skip to entry %d: %v"u8, testEntry.entryNumber, errΔ1);
                    }
                }
                var (entry, errΔ2) = reader.Next();
                idx++;
                if (errΔ2 != default!) {
                    tΔ1.Fatal(errΔ2);
                }
                if (!reflect.DeepEqual(testEntry.entry.OrTypedNil(), entry.OrTypedNil())) {
                    tΔ1.Errorf("entry %d mismatch: got:%#v want:%#v"u8, testEntry.entryNumber, entry.OrTypedNil(), testEntry.entry.OrTypedNil());
                }
                (var pcRanges, errΔ2) = dwarf.Ranges(entry);
                if (errΔ2 != default!) {
                    tΔ1.Fatal(errΔ2);
                }
                if (!reflect.DeepEqual(testEntry.pcRanges, pcRanges)) {
                    tΔ1.Errorf("entry %d: PC range mismatch: got:%#v want:%#v"u8, testEntry.entryNumber, pcRanges, testEntry.pcRanges);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataZdebugTestGcc484ˢ = "testdata/zdebug-test-gcc484-x86-64.obj"u8;

public static void TestCompressedDWARF(ж<testing.T> Ꮡt) {
    // Test file built with GCC 4.8.4 and as 2.24 using:
    // gcc -Wa,--compress-debug-sections -g -c -o zdebug-test-gcc484-x86-64.obj hello.c
    var (f, err) = Open(testdataZdebugTestGcc484ˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var dwarf, err) = f.DWARF();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var reader = dwarf.Reader();
    nint n = 0;
    while (ᐧ) {
        var (entry, errΔ1) = reader.Next();
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        if (entry == nil) {
            break;
        }
        n++;
    }
    if (n != 18) {
        Ꮡt.Fatalf("want %d DWARF entries, got %d"u8, (nint)(18), n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataCompressed64Objˢ = "testdata/compressed-64.obj"u8;
internal static readonly @string debugInfoˢ = ".debug_info"u8;

public static void TestCompressedSection(ж<testing.T> Ꮡt) {
    // Test files built with gcc -g -S hello.c and assembled with
    // --compress-debug-sections=zlib-gabi.
    var (f, err) = Open(testdataCompressed64Objˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var sec = f.Section(debugInfoˢ);
    var wantData = new byte[]{
        182, 0, 0, 0, 4, 0, 0, 0, 0, 0, 8, 1, 0, 0, 0, 0,
        1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 27, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 8, 7,
        0, 0, 0, 0, 2, 1, 8, 0, 0, 0, 0, 2, 2, 7, 0, 0,
        0, 0, 2, 4, 7, 0, 0, 0, 0, 2, 1, 6, 0, 0, 0, 0,
        2, 2, 5, 0, 0, 0, 0, 3, 4, 5, 105, 110, 116, 0, 2, 8,
        5, 0, 0, 0, 0, 2, 8, 7, 0, 0, 0, 0, 4, 8, 114, 0,
        0, 0, 2, 1, 6, 0, 0, 0, 0, 5, 0, 0, 0, 0, 1, 4,
        0, 0, 0, 0, 0, 0, 0, 0, 27, 0, 0, 0, 0, 0, 0, 0,
        1, 156, 179, 0, 0, 0, 6, 0, 0, 0, 0, 1, 4, 87, 0, 0,
        0, 2, 145, 108, 6, 0, 0, 0, 0, 1, 4, 179, 0, 0, 0, 2,
        145, 96, 0, 4, 8, 108, 0, 0, 0, 0
    }.slice();
    // Test Data method.
    (var b, err) = sec.Data();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!bytes.Equal(wantData, b)) {
        Ꮡt.Fatalf("want data %x, got %x"u8, wantData, b);
    }
    // Test Open method and seeking.
    var buf = new slice<byte>(len(b));
    var have = new slice<bool>(len(b));
    nint count = 0;
    var sf = sec.Open();
    {
        var (got, errΔ1) = sf.Seek(0, io.SeekEnd); if (got != (int64)len(b) || errΔ1 != default!) {
            Ꮡt.Fatalf("want seek end %d, got %d error %v"u8, len(b), got, errΔ1);
        }
    }
    {
        var (n, errΔ2) = sf.Read(buf); if (n != 0 || !AreEqual(errΔ2, io.EOF)) {
            Ꮡt.Fatalf("want EOF with 0 bytes, got %v with %d bytes"u8, errΔ2, n);
        }
    }
    var pos = (int64)len(buf);
    while (count < len(buf)) {
        // Construct random seek arguments.
        nint whence = rand.Intn(3);
        var target = rand.Int63n((int64)len(buf));
        int64 offset = default!;
        var exprᴛ1 = whence;
        if (exprᴛ1 == io.SeekStart) {
            offset = target;
        }
        else if (exprᴛ1 == io.SeekCurrent) {
            offset = target - pos;
        }
        else if (exprᴛ1 == io.SeekEnd) {
            offset = target - (int64)len(buf);
        }

        (pos, err) = sf.Seek(offset, whence);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (pos != target) {
            Ꮡt.Fatalf("want position %d, got %d"u8, target, pos);
        }
        // Read data from the new position.
        var end = pos + 16;
        if (end > (int64)len(buf)) {
            end = (int64)len(buf);
        }
        var (n, errΔ3) = io.ReadFull(sf, buf.slice((nint)(pos), (nint)(end)));
        if (errΔ3 != default!) {
            Ꮡt.Fatal(errΔ3);
        }
        for (nint i = 0; i < n; i++) {
            if (!have[(nint)(pos)]) {
                have[(nint)(pos)] = true;
                count++;
            }
            pos++;
        }
    }
    if (!bytes.Equal(wantData, buf)) {
        Ꮡt.Fatalf("want data %x, got %x"u8, wantData, buf);
    }
}

public static void TestNoSectionOverlaps(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Ensure cmd/link outputs sections without overlaps.
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "aix"u8 || exprᴛ1 == "android"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "wasip1"u8) {
        Ꮡt.Skipf("cmd/link doesn't produce ELF binaries on %s"u8, runtime.GOOS);
    }

    _ = (Func<@string, @string, (ж<net.IPAddr>, error)>)(net.ResolveIPAddr); // force dynamic linkage
    var (f, err) = Open(os.Args[0]);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    foreach (var (i, si) in (~f).Sections) {
        var sih = si.Value.SectionHeader;
        if (sih.Type == SHT_NOBITS) {
            continue;
        }
        // checking for overlap in file
        foreach (var (j, sj) in (~f).Sections) {
            var sjh = sj.Value.SectionHeader;
            if (i == j || sjh.Type == SHT_NOBITS || sih.Offset == sjh.Offset && sih.FileSize == 0) {
                continue;
            }
            if (sih.Offset >= sjh.Offset && sih.Offset < sjh.Offset + sjh.FileSize) {
                Ꮡt.Errorf("ld produced ELF with section offset %s within %s: 0x%x <= 0x%x..0x%x < 0x%x"u8,
                    sih.Name, sjh.Name, sjh.Offset, sih.Offset, sih.Offset + sih.FileSize, sjh.Offset + sjh.FileSize);
            }
        }
        if ((global::go.debug.elf_package.SectionFlag)(sih.Flags & SHF_ALLOC) == 0) {
            continue;
        }
        // checking for overlap in address space
        foreach (var (j, sj) in (~f).Sections) {
            var sjh = sj.Value.SectionHeader;
            if (i == j || (global::go.debug.elf_package.SectionFlag)(sjh.Flags & SHF_ALLOC) == 0 || sjh.Type == SHT_NOBITS || sih.Addr == sjh.Addr && sih.Size == 0) {
                continue;
            }
            if (sih.Addr >= sjh.Addr && sih.Addr < sjh.Addr + sjh.Size) {
                Ꮡt.Errorf("ld produced ELF with section address %s within %s: 0x%x <= 0x%x..0x%x < 0x%x"u8,
                    sih.Name, sjh.Name, sjh.Addr, sih.Addr, sih.Addr + sih.Size, sjh.Addr + sjh.Size);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bssˢ = ".bss"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testdataᶜ = "testdata/gcc-amd64-linux-exec"u8;

public static void TestNobitsSection(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string testdata = testdataᶜ;
        var (f, err) = Open(testdata);
        if (err != default!) {
            Ꮡt.Fatalf("could not read %s: %v"u8, testdata, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        @string wantError = unexpectedReadFromShtˢ;
        var bss = f.Section(bssˢ);
        (_, err) = bss.Data();
        if (err == default! || err.Error() != wantError) {
            Ꮡt.Fatalf("bss.Data() got error %q, want error %q"u8, err, wantError);
        }
        var r = bss.Open();
        var p = new slice<byte>(1);
        (_, err) = r.Read(p);
        if (err == default! || err.Error() != wantError) {
            Ꮡt.Fatalf("r.Read(p) got error %q, want error %q"u8, err, wantError);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestLargeNumberOfSections tests the case that a file has greater than or
// equal to 65280 (0xff00) sections.
public static void TestLargeNumberOfSections(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // A file with >= 0xff00 sections is too big, so we will construct it on the
        // fly. The original file "y.o" is generated by these commands:
        // 1. generate "y.c":
        //   for i in `seq 1 65288`; do
        //     printf -v x "%04x" i;
        //     echo "int var_$x __attribute__((section(\"section_$x\"))) = $i;"
        //   done > y.c
        // 2. compile: gcc -c y.c -m32
        //
        // $readelf -h y.o
        // ELF Header:
        //   Magic:   7f 45 4c 46 01 01 01 00 00 00 00 00 00 00 00 00
        //   Class:                             ELF32
        //   Data:                              2's complement, little endian
        //   Version:                           1 (current)
        //   OS/ABI:                            UNIX - System V
        //   ABI Version:                       0
        //   Type:                              REL (Relocatable file)
        //   Machine:                           Intel 80386
        //   Version:                           0x1
        //   Entry point address:               0x0
        //   Start of program headers:          0 (bytes into file)
        //   Start of section headers:          3003468 (bytes into file)
        //   Flags:                             0x0
        //   Size of this header:               52 (bytes)
        //   Size of program headers:           0 (bytes)
        //   Number of program headers:         0
        //   Size of section headers:           40 (bytes)
        //   Number of section headers:         0 (65298)
        //   Section header string table index: 65535 (65297)
        //
        // $readelf -S y.o
        // There are 65298 section headers, starting at offset 0x2dd44c:
        // Section Headers:
        //   [Nr]    Name              Type            Addr     Off    Size   ES Flg Lk Inf Al
        //   [    0]                   NULL            00000000 000000 00ff12 00     65297   0  0
        //   [    1] .text             PROGBITS        00000000 000034 000000 00  AX  0   0  1
        //   [    2] .data             PROGBITS        00000000 000034 000000 00  WA  0   0  1
        //   [    3] .bss              NOBITS          00000000 000034 000000 00  WA  0   0  1
        //   [    4] section_0001      PROGBITS        00000000 000034 000004 00  WA  0   0  4
        //   [    5] section_0002      PROGBITS        00000000 000038 000004 00  WA  0   0  4
        //   [ section_0003 ~ section_ff06 truncated ]
        //   [65290] section_ff07      PROGBITS        00000000 03fc4c 000004 00  WA  0   0  4
        //   [65291] section_ff08      PROGBITS        00000000 03fc50 000004 00  WA  0   0  4
        //   [65292] .comment          PROGBITS        00000000 03fc54 000027 01  MS  0   0  1
        //   [65293] .note.GNU-stack   PROGBITS        00000000 03fc7b 000000 00      0   0  1
        //   [65294] .symtab           SYMTAB          00000000 03fc7c 0ff0a0 10     65296   2  4
        //   [65295] .symtab_shndx     SYMTAB SECTION  00000000 13ed1c 03fc28 04     65294   0  4
        //   [65296] .strtab           STRTAB          00000000 17e944 08f74d 00      0   0  1
        //   [65297] .shstrtab         STRTAB          00000000 20e091 0cf3bb 00      0   0  1
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        {
            buf.Grow(0x55AF1C); // 3003468 + 40 * 65298
            var h = new Header32(
                Ident: new byte[]{0x7F, (rune)'E', (rune)'L', (rune)'F', 0x01, 0x01, 0x01}.array(16),
                Type: 1,
                Machine: 3,
                Version: 1,
                Shoff: 0x2DD44C,
                Ehsize: 0x34,
                Shentsize: 0x28,
                Shnum: 0,
                Shstrndx: 0xFFFF
            );
            binary.Write(new elf_internal_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, h);
            // Zero out sections [1]~[65294].
            buf.Write(bytes.Repeat(new byte[]{0}.slice(), 0x13ED1C - binary.Size(h)));
            // Write section [65295]. Section [65295] are all zeros except for the
            // last 48 bytes.
            buf.Write(bytes.Repeat(new byte[]{0}.slice(), 0x03FC28 - 12 * 4));
            for (nint i = 0; i < 12; i++) {
                binary.Write(new elf_internal_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, (uint32)((nint)(0xFF00 | i)));
            }
            // Write section [65296].
            buf.Write(new byte[]{0}.slice());
            buf.Write(slice<byte>("y.c\x00"u8));
            for (nint i = 1; i <= 65288; i++) {
                // var_0001 ~ var_ff08
                @string name = fmt.Sprintf("var_%04x"u8, i);
                buf.Write(slice<byte>(name));
                buf.Write(new byte[]{0}.slice());
            }
            // Write section [65297].
            buf.Write(new byte[]{0}.slice());
            buf.Write(slice<byte>(".symtab\x00"u8));
            buf.Write(slice<byte>(".strtab\x00"u8));
            buf.Write(slice<byte>(".shstrtab\x00"u8));
            buf.Write(slice<byte>(".text\x00"u8));
            buf.Write(slice<byte>(".data\x00"u8));
            buf.Write(slice<byte>(".bss\x00"u8));
            for (nint i = 1; i <= 65288; i++) {
                // s_0001 ~ s_ff08
                @string name = fmt.Sprintf("section_%04x"u8, i);
                buf.Write(slice<byte>(name));
                buf.Write(new byte[]{0}.slice());
            }
            buf.Write(slice<byte>(".comment\x00"u8));
            buf.Write(slice<byte>(".note.GNU-stack\x00"u8));
            buf.Write(slice<byte>(".symtab_shndx\x00"u8));
            // Write section header table.
            // NULL
            binary.Write(new elf_internal_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, new Section32(Name: 0, Size: 0xFF12, Link: 0xFF11));
            // .text
            binary.Write(new elf_internal_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, new Section32(
                Name: 0x1B,
                Type: (uint32)SHT_PROGBITS,
                Flags: (uint32)((global::go.debug.elf_package.SectionFlag)(SHF_ALLOC | SHF_EXECINSTR)),
                Off: 0x34,
                Addralign: 0x01
            ));
            // .data
            binary.Write(new elf_internal_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, new Section32(
                Name: 0x21,
                Type: (uint32)SHT_PROGBITS,
                Flags: (uint32)((global::go.debug.elf_package.SectionFlag)(SHF_WRITE | SHF_ALLOC)),
                Off: 0x34,
                Addralign: 0x01
            ));
            // .bss
            binary.Write(new elf_internal_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, new Section32(
                Name: 0x27,
                Type: (uint32)SHT_NOBITS,
                Flags: (uint32)((global::go.debug.elf_package.SectionFlag)(SHF_WRITE | SHF_ALLOC)),
                Off: 0x34,
                Addralign: 0x01
            ));
            // s_1 ~ s_65537
            for (nint i = 0; i < 65288; i++) {
                var s = new Section32(
                    Name: (uint32)(0x2C + i * 13),
                    Type: (uint32)SHT_PROGBITS,
                    Flags: (uint32)((global::go.debug.elf_package.SectionFlag)(SHF_WRITE | SHF_ALLOC)),
                    Off: (uint32)(0x34 + i * 4),
                    Size: 0x04,
                    Addralign: 0x04
                );
                binary.Write(new elf_internal_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, s);
            }
            // .comment
            binary.Write(new elf_internal_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, new Section32(
                Name: 0x0CF394,
                Type: (uint32)SHT_PROGBITS,
                Flags: (uint32)((global::go.debug.elf_package.SectionFlag)(SHF_MERGE | SHF_STRINGS)),
                Off: 0x03FC54,
                Size: 0x27,
                Addralign: 0x01,
                Entsize: 0x01
            ));
            // .note.GNU-stack
            binary.Write(new elf_internal_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, new Section32(
                Name: 0x0CF39D,
                Type: (uint32)SHT_PROGBITS,
                Off: 0x03FC7B,
                Addralign: 0x01
            ));
            // .symtab
            binary.Write(new elf_internal_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, new Section32(
                Name: 0x01,
                Type: (uint32)SHT_SYMTAB,
                Off: 0x03FC7C,
                Size: 0x0FF0A0,
                Link: 0xFF10,
                Info: 0x02,
                Addralign: 0x04,
                Entsize: 0x10
            ));
            // .symtab_shndx
            binary.Write(new elf_internal_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, new Section32(
                Name: 0x0CF3AD,
                Type: (uint32)SHT_SYMTAB_SHNDX,
                Off: 0x13ED1C,
                Size: 0x03FC28,
                Link: 0xFF0E,
                Addralign: 0x04,
                Entsize: 0x04
            ));
            // .strtab
            binary.Write(new elf_internal_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, new Section32(
                Name: 0x09,
                Type: (uint32)SHT_STRTAB,
                Off: 0x17E944,
                Size: 0x08F74D,
                Addralign: 0x01
            ));
            // .shstrtab
            binary.Write(new elf_internal_test_package.bytes_BufferжWriter(Ꮡbuf), binary.LittleEndian, new Section32(
                Name: 0x11,
                Type: (uint32)SHT_STRTAB,
                Off: 0x20E091,
                Size: 0x0CF3BB,
                Addralign: 0x01
            ));
        }
        var data = buf.Bytes();
        var (f, err) = NewFile(new elf_internal_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)));
        if (err != default!) {
            Ꮡt.Errorf("cannot create file from data: %v"u8, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var wantFileHeader = new FileHeader(
            Class: ELFCLASS32,
            Data: ELFDATA2LSB,
            Version: EV_CURRENT,
            OSABI: ELFOSABI_NONE,
            ByteOrder: binary.LittleEndian,
            Type: ET_REL,
            Machine: EM_386
        );
        if ((~f).FileHeader != wantFileHeader) {
            Ꮡt.Errorf("\nhave %#v\nwant %#v\n"u8, (~f).FileHeader, wantFileHeader);
        }
        nint wantSectionNum = 65298;
        if (len((~f).Sections) != wantSectionNum) {
            Ꮡt.Errorf("len(Sections) = %d, want %d"u8, len((~f).Sections), wantSectionNum);
        }
        var wantSectionHeader = new SectionHeader(
            Name: "section_0007"u8,
            Type: SHT_PROGBITS,
            Flags: SHF_WRITE + SHF_ALLOC,
            Offset: 0x4c,
            Size: 0x4,
            Addralign: 0x4,
            FileSize: 0x4
        );
        if ((~(~f).Sections[10]).SectionHeader != wantSectionHeader) {
            Ꮡt.Errorf("\nhave %#v\nwant %#v\n"u8, (~(~f).Sections[10]).SectionHeader, wantSectionHeader);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIssue10996(ж<testing.T> Ꮡt) {
    var data = slice<byte>(((@string)(new byte[]{0x7f, 0x45, 0x4c, 0x46, 0x02, 0x01, 0x01, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x01, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x00, 0x00, 0x00, 0x00})) + "0000");
    var (_, err) = NewFile(new elf_internal_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)));
    if (err == default!) {
        Ꮡt.Fatalf("opening invalid ELF file unexpectedly succeeded"u8);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testdataᶜ1 = "testdata/gcc-amd64-linux-exec"u8;

public static void TestDynValue(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string testdata = testdataᶜ1;
        var (f, err) = Open(testdata);
        if (err != default!) {
            Ꮡt.Fatalf("could not read %s: %v"u8, testdata, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var vals, err) = f.DynValue(DT_VERNEEDNUM);
        if (err != default!) {
            Ꮡt.Fatalf("DynValue(DT_VERNEEDNUM): got unexpected error %v"u8, err);
        }
        if (len(vals) != 1 || vals[0] != 1) {
            Ꮡt.Errorf("DynValue(DT_VERNEEDNUM): got %v, want [1]"u8, vals);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string origᶜ = "testdata/compressed-64.obj"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = "decoding dwarf section info"u8;

public static void TestIssue59208(ж<testing.T> Ꮡt) {
    // corrupted dwarf data should raise invalid dwarf data instead of invalid zlib
    @string orig = origᶜ;
    var (f, err) = Open(orig);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var sec = f.Section(debugInfoˢ);
    (var data, err) = os.ReadFile(orig);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var dn = new slice<byte>(len(data));
    var zoffset = (~sec).Offset + (uint64)(~sec).compressionOffset;
    copy(dn, data.slice(0, (nint)(zoffset)));
    (var ozd, err) = sec.Data();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var buf = bytes.NewBuffer(default!);
    var wr = zlib.NewWriter(new elf_internal_test_package.bytes_BufferжWriter(buf));
    // corrupt origin data same as COMPRESS_ZLIB
    copy(ozd, new byte[]{1, 0, 0, 0}.slice());
    wr.Write(ozd);
    wr.Close();
    copy(dn.slice((nint)(zoffset)), buf.Bytes());
    copy(dn.slice((nint)((~sec).Offset + (~sec).FileSize)), data.slice((nint)((~sec).Offset + (~sec).FileSize)));
    (var nf, err) = NewFile(new elf_internal_test_package.bytes_ReaderжReaderAt(bytes.NewReader(dn)));
    if (err != default!) {
        Ꮡt.Error(err);
    }
    @string want = wantᶜ;
    (_, err) = nf.DWARF();
    if (err == default! || !strings.Contains(err.Error(), want)) {
        Ꮡt.Errorf("DWARF = %v; want %q"u8, err, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testdataᶜ2 = "testdata/gcc-amd64-linux-exec"u8;

public static void BenchmarkSymbols64(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        @string testdata = testdataᶜ2;
        var (f, err) = Open(testdata);
        if (err != default!) {
            Ꮡb.Fatalf("could not read %s: %v"u8, testdata, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        b.ResetTimer();
        for (nint i = 0; i < b.N; i++) {
            var (symbols, errΔ1) = f.Symbols();
            if (errΔ1 != default!) {
                Ꮡb.Fatalf("Symbols(): got unexpected error %v"u8, errΔ1);
            }
            if (len(symbols) != 73) {
                Ꮡb.Errorf("\nhave %d symbols\nwant %d symbols\n"u8, len(symbols), (nint)(73));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testdataᶜ3 = "testdata/gcc-386-freebsd-exec"u8;

public static void BenchmarkSymbols32(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        @string testdata = testdataᶜ3;
        var (f, err) = Open(testdata);
        if (err != default!) {
            Ꮡb.Fatalf("could not read %s: %v"u8, testdata, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        b.ResetTimer();
        for (nint i = 0; i < b.N; i++) {
            var (symbols, errΔ1) = f.Symbols();
            if (errΔ1 != default!) {
                Ꮡb.Fatalf("Symbols(): got unexpected error %v"u8, errΔ1);
            }
            if (len(symbols) != 74) {
                Ꮡb.Errorf("\nhave %d symbols\nwant %d symbols\n"u8, len(symbols), (nint)(74));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end elf_internal_test_package

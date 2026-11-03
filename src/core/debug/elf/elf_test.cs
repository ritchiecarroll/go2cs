// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using fmt = fmt_package;
using testing = testing_package;
using static go.debug.elf_package;

partial class elf_internal_test_package {

internal partial struct nameTest {
    internal any val;
    internal @string str;
}

internal static slice<nameTest> nameTests = slice<nameTest>([
    new(ELFOSABI_LINUX, "ELFOSABI_LINUX"),
    new(ET_EXEC, "ET_EXEC"),
    new(EM_860, "EM_860"),
    new(SHN_LOPROC, "SHN_LOPROC"),
    new(SHT_PROGBITS, "SHT_PROGBITS"),
    new(SHF_MERGE + SHF_TLS, "SHF_MERGE+SHF_TLS"),
    new(PT_LOAD, "PT_LOAD"),
    new(PF_W + PF_R + 0x50, "PF_W+PF_R+0x50"),
    new(DT_SYMBOLIC, "DT_SYMBOLIC"),
    new(DF_BIND_NOW, "DF_BIND_NOW"),
    new(DF_1_PIE, "DF_1_PIE"),
    new(NT_FPREGSET, "NT_FPREGSET"),
    new(STB_GLOBAL, "STB_GLOBAL"),
    new(STT_COMMON, "STT_COMMON"),
    new(STV_HIDDEN, "STV_HIDDEN"),
    new(R_X86_64_PC32, "R_X86_64_PC32"),
    new(R_ALPHA_OP_PUSH, "R_ALPHA_OP_PUSH"),
    new(R_ARM_THM_ABS5, "R_ARM_THM_ABS5"),
    new(R_386_GOT32, "R_386_GOT32"),
    new(R_LARCH_CALL36, "R_LARCH_CALL36"),
    new(R_PPC_GOT16_HI, "R_PPC_GOT16_HI"),
    new(R_SPARC_GOT22, "R_SPARC_GOT22"),
    new(ET_LOOS + 5, "ET_LOOS+5"),
    new((global::go.debug.elf_package.ProgFlag)0x50, "0x50"),
    new(COMPRESS_ZLIB + 2, "COMPRESS_ZSTD+1")
]);

public static void TestNames(ж<testing.T> t) {
    foreach (var (i, tt) in nameTests) {
        @string s = fmt.Sprint(tt.val);
        if (s != tt.str) {
            t.Errorf("#%d: Sprint(%d) = %q, want %q", i, tt.val, s, tt.str);
        }
    }
}

} // end elf_internal_test_package

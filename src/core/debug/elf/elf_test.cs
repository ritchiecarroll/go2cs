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

internal static slice<nameTest> nameTests = new nameTest[]{
    new(ELFOSABI_LINUX, "ELFOSABI_LINUX"u8),
    new(ET_EXEC, "ET_EXEC"u8),
    new(EM_860, "EM_860"u8),
    new(SHN_LOPROC, "SHN_LOPROC"u8),
    new(SHT_PROGBITS, "SHT_PROGBITS"u8),
    new(SHF_MERGE + SHF_TLS, "SHF_MERGE+SHF_TLS"u8),
    new(PT_LOAD, "PT_LOAD"u8),
    new(PF_W + PF_R + 0x50, "PF_W+PF_R+0x50"u8),
    new(DT_SYMBOLIC, "DT_SYMBOLIC"u8),
    new(DF_BIND_NOW, "DF_BIND_NOW"u8),
    new(DF_1_PIE, "DF_1_PIE"u8),
    new(NT_FPREGSET, "NT_FPREGSET"u8),
    new(STB_GLOBAL, "STB_GLOBAL"u8),
    new(STT_COMMON, "STT_COMMON"u8),
    new(STV_HIDDEN, "STV_HIDDEN"u8),
    new(R_X86_64_PC32, "R_X86_64_PC32"u8),
    new(R_ALPHA_OP_PUSH, "R_ALPHA_OP_PUSH"u8),
    new(R_ARM_THM_ABS5, "R_ARM_THM_ABS5"u8),
    new(R_386_GOT32, "R_386_GOT32"u8),
    new(R_PPC_GOT16_HI, "R_PPC_GOT16_HI"u8),
    new(R_SPARC_GOT22, "R_SPARC_GOT22"u8),
    new(ET_LOOS + 5, "ET_LOOS+5"u8),
    new(((global::go.debug.elf_package.ProgFlag)0x50), "0x50"u8),
    new(COMPRESS_ZLIB + 2, "COMPRESS_ZSTD+1"u8)
}.slice();

public static void TestNames(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in nameTests) {
        @string s = fmt.Sprint(tt.val);
        if (s != tt.str) {
            Ꮡt.Errorf("#%d: Sprint(%d) = %q, want %q"u8, i, tt.val, s, tt.str);
        }
    }
}

} // end elf_internal_test_package

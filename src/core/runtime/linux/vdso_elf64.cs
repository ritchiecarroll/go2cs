// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux && (amd64 || arm64 || loong64 || mips64 || mips64le || ppc64 || ppc64le || riscv64 || s390x)
namespace go;

partial class runtime_package {

// ELF64 structure definitions for use by the vDSO loader
partial struct elfSym {
    internal uint32 st_name;
    internal byte st_info;
    internal byte st_other;
    internal uint16 st_shndx;
    internal uint64 st_value;
    internal uint64 st_size;
}

partial struct elfVerdef {
    internal uint16 vd_version; /* Version revision */
    internal uint16 vd_flags; /* Version information */
    internal uint16 vd_ndx; /* Version Index */
    internal uint16 vd_cnt; /* Number of associated aux entries */
    internal uint32 vd_hash; /* Version name hash value */
    internal uint32 vd_aux; /* Offset in bytes to verdaux array */
    internal uint32 vd_next; /* Offset in bytes to next verdef entry */
}

partial struct elfEhdr {
    internal array<byte> e_ident = new(_EI_NIDENT); /* Magic number and other info */
    internal uint16 e_type;           /* Object file type */
    internal uint16 e_machine;           /* Architecture */
    internal uint32 e_version;           /* Object file version */
    internal uint64 e_entry;           /* Entry point virtual address */
    internal uint64 e_phoff;           /* Program header table file offset */
    internal uint64 e_shoff;           /* Section header table file offset */
    internal uint32 e_flags;           /* Processor-specific flags */
    internal uint16 e_ehsize;           /* ELF header size in bytes */
    internal uint16 e_phentsize;           /* Program header table entry size */
    internal uint16 e_phnum;           /* Program header table entry count */
    internal uint16 e_shentsize;           /* Section header table entry size */
    internal uint16 e_shnum;           /* Section header table entry count */
    internal uint16 e_shstrndx;           /* Section header string table index */
}

partial struct elfPhdr {
    internal uint32 p_type; /* Segment type */
    internal uint32 p_flags; /* Segment flags */
    internal uint64 p_offset; /* Segment file offset */
    internal uint64 p_vaddr; /* Segment virtual address */
    internal uint64 p_paddr; /* Segment physical address */
    internal uint64 p_filesz; /* Segment size in file */
    internal uint64 p_memsz; /* Segment size in memory */
    internal uint64 p_align; /* Segment alignment */
}

partial struct elfShdr {
    internal uint32 sh_name; /* Section name (string tbl index) */
    internal uint32 sh_type; /* Section type */
    internal uint64 sh_flags; /* Section flags */
    internal uint64 sh_addr; /* Section virtual addr at execution */
    internal uint64 sh_offset; /* Section file offset */
    internal uint64 sh_size; /* Section size in bytes */
    internal uint32 sh_link; /* Link to another section */
    internal uint32 sh_info; /* Additional section information */
    internal uint64 sh_addralign; /* Section alignment */
    internal uint64 sh_entsize; /* Entry size if section holds table */
}

partial struct elfDyn {
    internal int64 d_tag;  /* Dynamic entry type */
    internal uint64 d_val; /* Integer value */
}

partial struct elfVerdaux {
    internal uint32 vda_name; /* Version or dependency names */
    internal uint32 vda_next; /* Offset in bytes to next verdaux entry */
}

} // end runtime_package

// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class xcoff_package {

// File Header.
partial struct FileHeader32 {
    public uint16 Fmagic; // Target machine
    public uint16 Fnscns; // Number of sections
    public uint32 Ftimedat; // Time and date of file creation
    public uint32 Fsymptr; // Byte offset to symbol table start
    public uint32 Fnsyms; // Number of entries in symbol table
    public uint16 Fopthdr; // Number of bytes in optional header
    public uint16 Fflags; // Flags
}

partial struct FileHeader64 {
    public uint16 Fmagic; // Target machine
    public uint16 Fnscns; // Number of sections
    public uint32 Ftimedat; // Time and date of file creation
    public uint64 Fsymptr; // Byte offset to symbol table start
    public uint16 Fopthdr; // Number of bytes in optional header
    public uint16 Fflags; // Flags
    public uint32 Fnsyms; // Number of entries in symbol table
}

public static UntypedInt FILHSZ_32 => 20;
public static UntypedInt FILHSZ_64 => 24;

public static UntypedInt U802TOCMAGIC => /* 0737 */ 479; // AIX 32-bit XCOFF
public static UntypedInt U64_TOCMAGIC => /* 0767 */ 503; // AIX 64-bit XCOFF

// Flags that describe the type of the object file.
public static UntypedInt F_RELFLG => 0x0001;

public static UntypedInt F_EXEC => 0x0002;

public static UntypedInt F_LNNO => 0x0004;

public static UntypedInt F_FDPR_PROF => 0x0010;

public static UntypedInt F_FDPR_OPTI => 0x0020;

public static UntypedInt F_DSA => 0x0040;

public static UntypedInt F_VARPG => 0x0100;

public static UntypedInt F_DYNLOAD => 0x1000;

public static UntypedInt F_SHROBJ => 0x2000;

public static UntypedInt F_LOADONLY => 0x4000;

// Section Header.
partial struct SectionHeader32 {
    public array<byte> Sname = new(8); // Section name
    public uint32 Spaddr;  // Physical address
    public uint32 Svaddr;  // Virtual address
    public uint32 Ssize;  // Section size
    public uint32 Sscnptr;  // Offset in file to raw data for section
    public uint32 Srelptr;  // Offset in file to relocation entries for section
    public uint32 Slnnoptr;  // Offset in file to line number entries for section
    public uint16 Snreloc;  // Number of relocation entries
    public uint16 Snlnno;  // Number of line number entries
    public uint32 Sflags;  // Flags to define the section type
}

partial struct SectionHeader64 {
    public array<byte> Sname = new(8); // Section name
    public uint64 Spaddr;  // Physical address
    public uint64 Svaddr;  // Virtual address
    public uint64 Ssize;  // Section size
    public uint64 Sscnptr;  // Offset in file to raw data for section
    public uint64 Srelptr;  // Offset in file to relocation entries for section
    public uint64 Slnnoptr;  // Offset in file to line number entries for section
    public uint32 Snreloc;  // Number of relocation entries
    public uint32 Snlnno;  // Number of line number entries
    public uint32 Sflags;  // Flags to define the section type
    public uint32 Spad;  // Needs to be 72 bytes long
}

// Flags defining the section type.
public static UntypedInt STYP_DWARF => 0x0010;

public static UntypedInt STYP_TEXT => 0x0020;

public static UntypedInt STYP_DATA => 0x0040;

public static UntypedInt STYP_BSS => 0x0080;

public static UntypedInt STYP_EXCEPT => 0x0100;

public static UntypedInt STYP_INFO => 0x0200;

public static UntypedInt STYP_TDATA => 0x0400;

public static UntypedInt STYP_TBSS => 0x0800;

public static UntypedInt STYP_LOADER => 0x1000;

public static UntypedInt STYP_DEBUG => 0x2000;

public static UntypedInt STYP_TYPCHK => 0x4000;

public static UntypedInt STYP_OVRFLO => 0x8000;

public static UntypedInt SSUBTYP_DWINFO => 0x10000; // DWARF info section
public static UntypedInt SSUBTYP_DWLINE => 0x20000; // DWARF line-number section
public static UntypedInt SSUBTYP_DWPBNMS => 0x30000; // DWARF public names section
public static UntypedInt SSUBTYP_DWPBTYP => 0x40000; // DWARF public types section
public static UntypedInt SSUBTYP_DWARNGE => 0x50000; // DWARF aranges section
public static UntypedInt SSUBTYP_DWABREV => 0x60000; // DWARF abbreviation section
public static UntypedInt SSUBTYP_DWSTR => 0x70000; // DWARF strings section
public static UntypedInt SSUBTYP_DWRNGES => 0x80000; // DWARF ranges section
public static UntypedInt SSUBTYP_DWLOC => 0x90000; // DWARF location lists section
public static UntypedInt SSUBTYP_DWFRAME => 0xA0000; // DWARF frames section
public static UntypedInt SSUBTYP_DWMAC => 0xB0000; // DWARF macros section

// Symbol Table Entry.
partial struct SymEnt32 {
    public array<byte> Nname = new(8); // Symbol name
    public uint32 Nvalue;  // Symbol value
    public uint16 Nscnum;  // Section number of symbol
    public uint16 Ntype;  // Basic and derived type specification
    public uint8 Nsclass;   // Storage class of symbol
    public uint8 Nnumaux;   // Number of auxiliary entries
}

partial struct SymEnt64 {
    public uint64 Nvalue; // Symbol value
    public uint32 Noffset; // Offset of the name in string table or .debug section
    public uint16 Nscnum; // Section number of symbol
    public uint16 Ntype; // Basic and derived type specification
    public uint8 Nsclass;  // Storage class of symbol
    public uint8 Nnumaux;  // Number of auxiliary entries
}

public static UntypedInt SYMESZ => 18;

public static UntypedInt N_DEBUG => -2;
public static UntypedInt N_ABS => -1;
public static UntypedInt N_UNDEF => 0;
public static UntypedInt SYM_V_INTERNAL => 0x1000;
public static UntypedInt SYM_V_HIDDEN => 0x2000;
public static UntypedInt SYM_V_PROTECTED => 0x3000;
public static UntypedInt SYM_V_EXPORTED => 0x4000;
public static UntypedInt SYM_TYPE_FUNC => 0x0020; // is function

// Storage Class.
public static UntypedInt C_NULL => 0; // Symbol table entry marked for deletion

public static UntypedInt C_EXT => 2; // External symbol

public static UntypedInt C_STAT => 3; // Static symbol

public static UntypedInt C_BLOCK => 100; // Beginning or end of inner block

public static UntypedInt C_FCN => 101; // Beginning or end of function

public static UntypedInt C_FILE => 103; // Source file name and compiler information

public static UntypedInt C_HIDEXT => 107; // Unnamed external symbol

public static UntypedInt C_BINCL => 108; // Beginning of include file

public static UntypedInt C_EINCL => 109; // End of include file

public static UntypedInt C_WEAKEXT => 111; // Weak external symbol

public static UntypedInt C_DWARF => 112; // DWARF symbol

public static UntypedInt C_GSYM => 128; // Global variable

public static UntypedInt C_LSYM => 129; // Automatic variable allocated on stack

public static UntypedInt C_PSYM => 130; // Argument to subroutine allocated on stack

public static UntypedInt C_RSYM => 131; // Register variable

public static UntypedInt C_RPSYM => 132; // Argument to function or procedure stored in register

public static UntypedInt C_STSYM => 133; // Statically allocated symbol

public static UntypedInt C_BCOMM => 135; // Beginning of common block

public static UntypedInt C_ECOML => 136; // Local member of common block

public static UntypedInt C_ECOMM => 137; // End of common block

public static UntypedInt C_DECL => 140; // Declaration of object

public static UntypedInt C_ENTRY => 141; // Alternate entry

public static UntypedInt C_FUN => 142; // Function or procedure

public static UntypedInt C_BSTAT => 143; // Beginning of static block

public static UntypedInt C_ESTAT => 144; // End of static block

public static UntypedInt C_GTLS => 145; // Global thread-local variable

public static UntypedInt C_STTLS => 146; // Static thread-local variable

// File Auxiliary Entry
partial struct AuxFile64 {
    public array<byte> Xfname = new(8); // Name or offset inside string table
    public uint8 Xftype;   // Source file string type
    public uint8 Xauxtype;   // Type of auxiliary entry
}

// Function Auxiliary Entry
partial struct AuxFcn32 {
    public uint32 Xexptr; // File offset to exception table entry
    public uint32 Xfsize; // Size of function in bytes
    public uint32 Xlnnoptr; // File pointer to line number
    public uint32 Xendndx; // Symbol table index of next entry
    public uint16 Xpad; // Unused
}

partial struct AuxFcn64 {
    public uint64 Xlnnoptr; // File pointer to line number
    public uint32 Xfsize; // Size of function in bytes
    public uint32 Xendndx; // Symbol table index of next entry
    public uint8 Xpad;  // Unused
    public uint8 Xauxtype;  // Type of auxiliary entry
}

partial struct AuxSect64 {
    public uint64 Xscnlen; // section length
    public uint64 Xnreloc; // Num RLDs
    internal uint8 pad;
    public uint8 Xauxtype; // Type of auxiliary entry
}

// csect Auxiliary Entry.
partial struct AuxCSect32 {
    public uint32 Xscnlen; // Length or symbol table index
    public uint32 Xparmhash; // Offset of parameter type-check string
    public uint16 Xsnhash; // .typchk section number
    public uint8 Xsmtyp;  // Symbol alignment and type
    public uint8 Xsmclas;  // Storage-mapping class
    public uint32 Xstab; // Reserved
    public uint16 Xsnstab; // Reserved
}

partial struct AuxCSect64 {
    public uint32 Xscnlenlo; // Lower 4 bytes of length or symbol table index
    public uint32 Xparmhash; // Offset of parameter type-check string
    public uint16 Xsnhash; // .typchk section number
    public uint8 Xsmtyp;  // Symbol alignment and type
    public uint8 Xsmclas;  // Storage-mapping class
    public uint32 Xscnlenhi; // Upper 4 bytes of length or symbol table index
    public uint8 Xpad;  // Unused
    public uint8 Xauxtype;  // Type of auxiliary entry
}

// Auxiliary type
internal static UntypedInt _AUX_EXCEPT => 255;

internal static UntypedInt _AUX_FCN => 254;

internal static UntypedInt _AUX_SYM => 253;

internal static UntypedInt _AUX_FILE => 252;

internal static UntypedInt _AUX_CSECT => 251;

internal static UntypedInt _AUX_SECT => 250;

// Symbol type field.
public static UntypedInt XTY_ER => 0; // External reference

public static UntypedInt XTY_SD => 1; // Section definition

public static UntypedInt XTY_LD => 2; // Label definition

public static UntypedInt XTY_CM => 3; // Common csect definition

// Defines for File auxiliary definitions: x_ftype field of x_file
public static UntypedInt XFT_FN => 0; // Source File Name

public static UntypedInt XFT_CT => 1; // Compile Time Stamp

public static UntypedInt XFT_CV => 2; // Compiler Version Number

public static UntypedInt XFT_CD => 128; // Compiler Defined Information

// Storage-mapping class.
public static UntypedInt XMC_PR => 0; // Program code

public static UntypedInt XMC_RO => 1; // Read-only constant

public static UntypedInt XMC_DB => 2; // Debug dictionary table

public static UntypedInt XMC_TC => 3; // TOC entry

public static UntypedInt XMC_UA => 4; // Unclassified

public static UntypedInt XMC_RW => 5; // Read/Write data

public static UntypedInt XMC_GL => 6; // Global linkage

public static UntypedInt XMC_XO => 7; // Extended operation

public static UntypedInt XMC_SV => 8; // 32-bit supervisor call descriptor

public static UntypedInt XMC_BS => 9; // BSS class

public static UntypedInt XMC_DS => 10; // Function descriptor

public static UntypedInt XMC_UC => 11; // Unnamed FORTRAN common

public static UntypedInt XMC_TC0 => 15; // TOC anchor

public static UntypedInt XMC_TD => 16; // Scalar data entry in the TOC

public static UntypedInt XMC_SV64 => 17; // 64-bit supervisor call descriptor

public static UntypedInt XMC_SV3264 => 18; // Supervisor call descriptor for both 32-bit and 64-bit

public static UntypedInt XMC_TL => 20; // Read/Write thread-local data

public static UntypedInt XMC_UL => 21; // Read/Write thread-local data (.tbss)

public static UntypedInt XMC_TE => 22; // TOC entry

// Loader Header.
partial struct LoaderHeader32 {
    public uint32 Lversion; // Loader section version number
    public uint32 Lnsyms; // Number of symbol table entries
    public uint32 Lnreloc; // Number of relocation table entries
    public uint32 Listlen; // Length of import file ID string table
    public uint32 Lnimpid; // Number of import file IDs
    public uint32 Limpoff; // Offset to start of import file IDs
    public uint32 Lstlen; // Length of string table
    public uint32 Lstoff; // Offset to start of string table
}

partial struct LoaderHeader64 {
    public uint32 Lversion; // Loader section version number
    public uint32 Lnsyms; // Number of symbol table entries
    public uint32 Lnreloc; // Number of relocation table entries
    public uint32 Listlen; // Length of import file ID string table
    public uint32 Lnimpid; // Number of import file IDs
    public uint32 Lstlen; // Length of string table
    public uint64 Limpoff; // Offset to start of import file IDs
    public uint64 Lstoff; // Offset to start of string table
    public uint64 Lsymoff; // Offset to start of symbol table
    public uint64 Lrldoff; // Offset to start of relocation entries
}

public static UntypedInt LDHDRSZ_32 => 32;
public static UntypedInt LDHDRSZ_64 => 56;

// Loader Symbol.
partial struct LoaderSymbol32 {
    public array<byte> Lname = new(8); // Symbol name or byte offset into string table
    public uint32 Lvalue;  // Address field
    public uint16 Lscnum;  // Section number containing symbol
    public uint8 Lsmtype;   // Symbol type, export, import flags
    public uint8 Lsmclas;   // Symbol storage class
    public uint32 Lifile;  // Import file ID; ordinal of import file IDs
    public uint32 Lparm;  // Parameter type-check field
}

partial struct LoaderSymbol64 {
    public uint64 Lvalue; // Address field
    public uint32 Loffset; // Byte offset into string table of symbol name
    public uint16 Lscnum; // Section number containing symbol
    public uint8 Lsmtype;  // Symbol type, export, import flags
    public uint8 Lsmclas;  // Symbol storage class
    public uint32 Lifile; // Import file ID; ordinal of import file IDs
    public uint32 Lparm; // Parameter type-check field
}

partial struct Reloc32 {
    public uint32 Rvaddr; // (virtual) address of reference
    public uint32 Rsymndx; // Index into symbol table
    public uint8 Rsize;  // Sign and reloc bit len
    public uint8 Rtype;  // Toc relocation type
}

partial struct Reloc64 {
    public uint64 Rvaddr; // (virtual) address of reference
    public uint32 Rsymndx; // Index into symbol table
    public uint8 Rsize;  // Sign and reloc bit len
    public uint8 Rtype;  // Toc relocation type
}

public static UntypedInt R_POS => 0x00; // A(sym) Positive Relocation
public static UntypedInt R_NEG => 0x01; // -A(sym) Negative Relocation
public static UntypedInt R_REL => 0x02; // A(sym-*) Relative to self
public static UntypedInt R_TOC => 0x03; // A(sym-TOC) Relative to TOC
public static UntypedInt R_TRL => 0x12; // A(sym-TOC) TOC Relative indirect load.
public static UntypedInt R_TRLA => 0x13; // A(sym-TOC) TOC Rel load address. modifiable inst
public static UntypedInt R_GL => 0x05; // A(external TOC of sym) Global Linkage
public static UntypedInt R_TCL => 0x06; // A(local TOC of sym) Local object TOC address
public static UntypedInt R_RL => 0x0C; // A(sym) Pos indirect load. modifiable instruction
public static UntypedInt R_RLA => 0x0D; // A(sym) Pos Load Address. modifiable instruction
public static UntypedInt R_REF => 0x0F; // AL0(sym) Non relocating ref. No garbage collect
public static UntypedInt R_BA => 0x08; // A(sym) Branch absolute. Cannot modify instruction
public static UntypedInt R_RBA => 0x18; // A(sym) Branch absolute. modifiable instruction
public static UntypedInt R_BR => 0x0A; // A(sym-*) Branch rel to self. non modifiable
public static UntypedInt R_RBR => 0x1A; // A(sym-*) Branch rel to self. modifiable instr
public static UntypedInt R_TLS => 0x20; // General-dynamic reference to TLS symbol
public static UntypedInt R_TLS_IE => 0x21; // Initial-exec reference to TLS symbol
public static UntypedInt R_TLS_LD => 0x22; // Local-dynamic reference to TLS symbol
public static UntypedInt R_TLS_LE => 0x23; // Local-exec reference to TLS symbol
public static UntypedInt R_TLSM => 0x24; // Module reference to TLS symbol
public static UntypedInt R_TLSML => 0x25; // Module reference to local (own) module
public static UntypedInt R_TOCU => 0x30; // Relative to TOC - high order bits
public static UntypedInt R_TOCL => 0x31; // Relative to TOC - low order bits

} // end xcoff_package

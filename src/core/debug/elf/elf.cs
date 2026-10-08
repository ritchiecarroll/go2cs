/*
 * ELF constants and data structures
 *
 * Derived from:
 * $FreeBSD: src/sys/sys/elf32.h,v 1.8.14.1 2005/12/30 22:13:58 marcel Exp $
 * $FreeBSD: src/sys/sys/elf64.h,v 1.10.14.1 2005/12/30 22:13:58 marcel Exp $
 * $FreeBSD: src/sys/sys/elf_common.h,v 1.15.8.1 2005/12/30 22:13:58 marcel Exp $
 * $FreeBSD: src/sys/alpha/include/elf.h,v 1.14 2003/09/25 01:10:22 peter Exp $
 * $FreeBSD: src/sys/amd64/include/elf.h,v 1.18 2004/08/03 08:21:48 dfr Exp $
 * $FreeBSD: src/sys/arm/include/elf.h,v 1.5.2.1 2006/06/30 21:42:52 cognet Exp $
 * $FreeBSD: src/sys/i386/include/elf.h,v 1.16 2004/08/02 19:12:17 dfr Exp $
 * $FreeBSD: src/sys/powerpc/include/elf.h,v 1.7 2004/11/02 09:47:01 ssouhlal Exp $
 * $FreeBSD: src/sys/sparc64/include/elf.h,v 1.12 2003/09/25 01:10:26 peter Exp $
 * "System V ABI" (http://www.sco.com/developers/gabi/latest/ch4.eheader.html)
 * "ELF for the ARM® 64-bit Architecture (AArch64)" (ARM IHI 0056B)
 * "RISC-V ELF psABI specification" (https://github.com/riscv-non-isa/riscv-elf-psabi-doc/blob/master/riscv-elf.adoc)
 * llvm/BinaryFormat/ELF.h - ELF constants and structures
 *
 * Copyright (c) 1996-1998 John D. Polstra.  All rights reserved.
 * Copyright (c) 2001 David E. O'Brien
 * Portions Copyright 2009 The Go Authors. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */
namespace go.debug;

using strconv = strconv_package;

partial class elf_package {

/*
 * Constants
 */

// Indexes into the Header.Ident array.
public static UntypedInt EI_CLASS => 4; /* Class of machine. */

public static UntypedInt EI_DATA => 5; /* Data format. */

public static UntypedInt EI_VERSION => 6; /* ELF format version. */

public static UntypedInt EI_OSABI => 7; /* Operating system / ABI identification */

public static UntypedInt EI_ABIVERSION => 8; /* ABI version */

public static UntypedInt EI_PAD => 9; /* Start of padding (per SVR4 ABI). */

public static UntypedInt EI_NIDENT => 16; /* Size of e_ident array. */

// Initial magic number for ELF files.
public static readonly @string ELFMAG = "\u007fELF"u8;

partial struct Version /*num:byte*/;

public static Version EV_NONE => 0;
public static Version EV_CURRENT => 1;

internal static slice<intName> versionStrings = new intName[]{
    new(0, "EV_NONE"u8),
    new(1, "EV_CURRENT"u8)
}.slice();

public static @string String(this Version i) {
    return stringName((uint32)(byte)i, versionStrings, false);
}

public static @string GoString(this Version i) {
    return stringName((uint32)(byte)i, versionStrings, true);
}

partial struct Class /*num:byte*/;

public static Class ELFCLASSNONE => 0; /* Unknown class. */
public static Class ELFCLASS32 => 1; /* 32-bit architecture. */
public static Class ELFCLASS64 => 2; /* 64-bit architecture. */

internal static slice<intName> classStrings = new intName[]{
    new(0, "ELFCLASSNONE"u8),
    new(1, "ELFCLASS32"u8),
    new(2, "ELFCLASS64"u8)
}.slice();

public static @string String(this Class i) {
    return stringName((uint32)(byte)i, classStrings, false);
}

public static @string GoString(this Class i) {
    return stringName((uint32)(byte)i, classStrings, true);
}

partial struct ΔData /*num:byte*/;

public static ΔData ELFDATANONE => 0; /* Unknown data format. */
public static ΔData ELFDATA2LSB => 1; /* 2's complement little-endian. */
public static ΔData ELFDATA2MSB => 2; /* 2's complement big-endian. */

internal static slice<intName> dataStrings = new intName[]{
    new(0, "ELFDATANONE"u8),
    new(1, "ELFDATA2LSB"u8),
    new(2, "ELFDATA2MSB"u8)
}.slice();

public static @string String(this ΔData i) {
    return stringName((uint32)(byte)i, dataStrings, false);
}

public static @string GoString(this ΔData i) {
    return stringName((uint32)(byte)i, dataStrings, true);
}

partial struct OSABI /*num:byte*/;

public static OSABI ELFOSABI_NONE => 0;       /* UNIX System V ABI */
public static OSABI ELFOSABI_HPUX => 1;       /* HP-UX operating system */
public static OSABI ELFOSABI_NETBSD => 2;     /* NetBSD */
public static OSABI ELFOSABI_LINUX => 3;      /* Linux */
public static OSABI ELFOSABI_HURD => 4;       /* Hurd */
public static OSABI ELFOSABI_86OPEN => 5;     /* 86Open common IA32 ABI */
public static OSABI ELFOSABI_SOLARIS => 6;    /* Solaris */
public static OSABI ELFOSABI_AIX => 7;        /* AIX */
public static OSABI ELFOSABI_IRIX => 8;       /* IRIX */
public static OSABI ELFOSABI_FREEBSD => 9;    /* FreeBSD */
public static OSABI ELFOSABI_TRU64 => 10;      /* TRU64 UNIX */
public static OSABI ELFOSABI_MODESTO => 11;    /* Novell Modesto */
public static OSABI ELFOSABI_OPENBSD => 12;    /* OpenBSD */
public static OSABI ELFOSABI_OPENVMS => 13;    /* Open VMS */
public static OSABI ELFOSABI_NSK => 14;        /* HP Non-Stop Kernel */
public static OSABI ELFOSABI_AROS => 15;       /* Amiga Research OS */
public static OSABI ELFOSABI_FENIXOS => 16;    /* The FenixOS highly scalable multi-core OS */
public static OSABI ELFOSABI_CLOUDABI => 17;   /* Nuxi CloudABI */
public static OSABI ELFOSABI_ARM => 97;        /* ARM */
public static OSABI ELFOSABI_STANDALONE => 255; /* Standalone (embedded) application */

internal static slice<intName> osabiStrings = new intName[]{
    new(0, "ELFOSABI_NONE"u8),
    new(1, "ELFOSABI_HPUX"u8),
    new(2, "ELFOSABI_NETBSD"u8),
    new(3, "ELFOSABI_LINUX"u8),
    new(4, "ELFOSABI_HURD"u8),
    new(5, "ELFOSABI_86OPEN"u8),
    new(6, "ELFOSABI_SOLARIS"u8),
    new(7, "ELFOSABI_AIX"u8),
    new(8, "ELFOSABI_IRIX"u8),
    new(9, "ELFOSABI_FREEBSD"u8),
    new(10, "ELFOSABI_TRU64"u8),
    new(11, "ELFOSABI_MODESTO"u8),
    new(12, "ELFOSABI_OPENBSD"u8),
    new(13, "ELFOSABI_OPENVMS"u8),
    new(14, "ELFOSABI_NSK"u8),
    new(15, "ELFOSABI_AROS"u8),
    new(16, "ELFOSABI_FENIXOS"u8),
    new(17, "ELFOSABI_CLOUDABI"u8),
    new(97, "ELFOSABI_ARM"u8),
    new(255, "ELFOSABI_STANDALONE"u8)
}.slice();

public static @string String(this OSABI i) {
    return stringName((uint32)(byte)i, osabiStrings, false);
}

public static @string GoString(this OSABI i) {
    return stringName((uint32)(byte)i, osabiStrings, true);
}

partial struct Type /*num:uint16*/;

public static Type ET_NONE => 0;      /* Unknown type. */
public static Type ET_REL => 1;       /* Relocatable. */
public static Type ET_EXEC => 2;      /* Executable. */
public static Type ET_DYN => 3;       /* Shared object. */
public static Type ET_CORE => 4;      /* Core file. */
public static Type ET_LOOS => 0xfe00;      /* First operating system specific. */
public static Type ET_HIOS => 0xfeff;      /* Last operating system-specific. */
public static Type ET_LOPROC => 0xff00;    /* First processor-specific. */
public static Type ET_HIPROC => 0xffff;    /* Last processor-specific. */

internal static slice<intName> typeStrings = new intName[]{
    new(0, "ET_NONE"u8),
    new(1, "ET_REL"u8),
    new(2, "ET_EXEC"u8),
    new(3, "ET_DYN"u8),
    new(4, "ET_CORE"u8),
    new(0xfe00, "ET_LOOS"u8),
    new(0xfeff, "ET_HIOS"u8),
    new(0xff00, "ET_LOPROC"u8),
    new(0xffff, "ET_HIPROC"u8)
}.slice();

public static @string String(this Type i) {
    return stringName((uint32)(uint16)i, typeStrings, false);
}

public static @string GoString(this Type i) {
    return stringName((uint32)(uint16)i, typeStrings, true);
}

partial struct Machine /*num:uint16*/;

public static Machine EM_NONE => 0;          /* Unknown machine. */
public static Machine EM_M32 => 1;           /* AT&T WE32100. */
public static Machine EM_SPARC => 2;         /* Sun SPARC. */
public static Machine EM_386 => 3;           /* Intel i386. */
public static Machine EM_68K => 4;           /* Motorola 68000. */
public static Machine EM_88K => 5;           /* Motorola 88000. */
public static Machine EM_860 => 7;           /* Intel i860. */
public static Machine EM_MIPS => 8;          /* MIPS R3000 Big-Endian only. */
public static Machine EM_S370 => 9;          /* IBM System/370. */
public static Machine EM_MIPS_RS3_LE => 10;   /* MIPS R3000 Little-Endian. */
public static Machine EM_PARISC => 15;        /* HP PA-RISC. */
public static Machine EM_VPP500 => 17;        /* Fujitsu VPP500. */
public static Machine EM_SPARC32PLUS => 18;   /* SPARC v8plus. */
public static Machine EM_960 => 19;           /* Intel 80960. */
public static Machine EM_PPC => 20;           /* PowerPC 32-bit. */
public static Machine EM_PPC64 => 21;         /* PowerPC 64-bit. */
public static Machine EM_S390 => 22;          /* IBM System/390. */
public static Machine EM_V800 => 36;          /* NEC V800. */
public static Machine EM_FR20 => 37;          /* Fujitsu FR20. */
public static Machine EM_RH32 => 38;          /* TRW RH-32. */
public static Machine EM_RCE => 39;           /* Motorola RCE. */
public static Machine EM_ARM => 40;           /* ARM. */
public static Machine EM_SH => 42;            /* Hitachi SH. */
public static Machine EM_SPARCV9 => 43;       /* SPARC v9 64-bit. */
public static Machine EM_TRICORE => 44;       /* Siemens TriCore embedded processor. */
public static Machine EM_ARC => 45;           /* Argonaut RISC Core. */
public static Machine EM_H8_300 => 46;        /* Hitachi H8/300. */
public static Machine EM_H8_300H => 47;       /* Hitachi H8/300H. */
public static Machine EM_H8S => 48;           /* Hitachi H8S. */
public static Machine EM_H8_500 => 49;        /* Hitachi H8/500. */
public static Machine EM_IA_64 => 50;         /* Intel IA-64 Processor. */
public static Machine EM_MIPS_X => 51;        /* Stanford MIPS-X. */
public static Machine EM_COLDFIRE => 52;      /* Motorola ColdFire. */
public static Machine EM_68HC12 => 53;        /* Motorola M68HC12. */
public static Machine EM_MMA => 54;           /* Fujitsu MMA. */
public static Machine EM_PCP => 55;           /* Siemens PCP. */
public static Machine EM_NCPU => 56;          /* Sony nCPU. */
public static Machine EM_NDR1 => 57;          /* Denso NDR1 microprocessor. */
public static Machine EM_STARCORE => 58;      /* Motorola Star*Core processor. */
public static Machine EM_ME16 => 59;          /* Toyota ME16 processor. */
public static Machine EM_ST100 => 60;         /* STMicroelectronics ST100 processor. */
public static Machine EM_TINYJ => 61;         /* Advanced Logic Corp. TinyJ processor. */
public static Machine EM_X86_64 => 62;        /* Advanced Micro Devices x86-64 */
public static Machine EM_PDSP => 63;          /* Sony DSP Processor */
public static Machine EM_PDP10 => 64;         /* Digital Equipment Corp. PDP-10 */
public static Machine EM_PDP11 => 65;         /* Digital Equipment Corp. PDP-11 */
public static Machine EM_FX66 => 66;          /* Siemens FX66 microcontroller */
public static Machine EM_ST9PLUS => 67;       /* STMicroelectronics ST9+ 8/16 bit microcontroller */
public static Machine EM_ST7 => 68;           /* STMicroelectronics ST7 8-bit microcontroller */
public static Machine EM_68HC16 => 69;        /* Motorola MC68HC16 Microcontroller */
public static Machine EM_68HC11 => 70;        /* Motorola MC68HC11 Microcontroller */
public static Machine EM_68HC08 => 71;        /* Motorola MC68HC08 Microcontroller */
public static Machine EM_68HC05 => 72;        /* Motorola MC68HC05 Microcontroller */
public static Machine EM_SVX => 73;           /* Silicon Graphics SVx */
public static Machine EM_ST19 => 74;          /* STMicroelectronics ST19 8-bit microcontroller */
public static Machine EM_VAX => 75;           /* Digital VAX */
public static Machine EM_CRIS => 76;          /* Axis Communications 32-bit embedded processor */
public static Machine EM_JAVELIN => 77;       /* Infineon Technologies 32-bit embedded processor */
public static Machine EM_FIREPATH => 78;      /* Element 14 64-bit DSP Processor */
public static Machine EM_ZSP => 79;           /* LSI Logic 16-bit DSP Processor */
public static Machine EM_MMIX => 80;          /* Donald Knuth's educational 64-bit processor */
public static Machine EM_HUANY => 81;         /* Harvard University machine-independent object files */
public static Machine EM_PRISM => 82;         /* SiTera Prism */
public static Machine EM_AVR => 83;           /* Atmel AVR 8-bit microcontroller */
public static Machine EM_FR30 => 84;          /* Fujitsu FR30 */
public static Machine EM_D10V => 85;          /* Mitsubishi D10V */
public static Machine EM_D30V => 86;          /* Mitsubishi D30V */
public static Machine EM_V850 => 87;          /* NEC v850 */
public static Machine EM_M32R => 88;          /* Mitsubishi M32R */
public static Machine EM_MN10300 => 89;       /* Matsushita MN10300 */
public static Machine EM_MN10200 => 90;       /* Matsushita MN10200 */
public static Machine EM_PJ => 91;            /* picoJava */
public static Machine EM_OPENRISC => 92;      /* OpenRISC 32-bit embedded processor */
public static Machine EM_ARC_COMPACT => 93;   /* ARC International ARCompact processor (old spelling/synonym: EM_ARC_A5) */
public static Machine EM_XTENSA => 94;        /* Tensilica Xtensa Architecture */
public static Machine EM_VIDEOCORE => 95;     /* Alphamosaic VideoCore processor */
public static Machine EM_TMM_GPP => 96;       /* Thompson Multimedia General Purpose Processor */
public static Machine EM_NS32K => 97;         /* National Semiconductor 32000 series */
public static Machine EM_TPC => 98;           /* Tenor Network TPC processor */
public static Machine EM_SNP1K => 99;         /* Trebia SNP 1000 processor */
public static Machine EM_ST200 => 100;         /* STMicroelectronics (www.st.com) ST200 microcontroller */
public static Machine EM_IP2K => 101;          /* Ubicom IP2xxx microcontroller family */
public static Machine EM_MAX => 102;           /* MAX Processor */
public static Machine EM_CR => 103;            /* National Semiconductor CompactRISC microprocessor */
public static Machine EM_F2MC16 => 104;        /* Fujitsu F2MC16 */
public static Machine EM_MSP430 => 105;        /* Texas Instruments embedded microcontroller msp430 */
public static Machine EM_BLACKFIN => 106;      /* Analog Devices Blackfin (DSP) processor */
public static Machine EM_SE_C33 => 107;        /* S1C33 Family of Seiko Epson processors */
public static Machine EM_SEP => 108;           /* Sharp embedded microprocessor */
public static Machine EM_ARCA => 109;          /* Arca RISC Microprocessor */
public static Machine EM_UNICORE => 110;       /* Microprocessor series from PKU-Unity Ltd. and MPRC of Peking University */
public static Machine EM_EXCESS => 111;        /* eXcess: 16/32/64-bit configurable embedded CPU */
public static Machine EM_DXP => 112;           /* Icera Semiconductor Inc. Deep Execution Processor */
public static Machine EM_ALTERA_NIOS2 => 113;  /* Altera Nios II soft-core processor */
public static Machine EM_CRX => 114;           /* National Semiconductor CompactRISC CRX microprocessor */
public static Machine EM_XGATE => 115;         /* Motorola XGATE embedded processor */
public static Machine EM_C166 => 116;          /* Infineon C16x/XC16x processor */
public static Machine EM_M16C => 117;          /* Renesas M16C series microprocessors */
public static Machine EM_DSPIC30F => 118;      /* Microchip Technology dsPIC30F Digital Signal Controller */
public static Machine EM_CE => 119;            /* Freescale Communication Engine RISC core */
public static Machine EM_M32C => 120;          /* Renesas M32C series microprocessors */
public static Machine EM_TSK3000 => 131;       /* Altium TSK3000 core */
public static Machine EM_RS08 => 132;          /* Freescale RS08 embedded processor */
public static Machine EM_SHARC => 133;         /* Analog Devices SHARC family of 32-bit DSP processors */
public static Machine EM_ECOG2 => 134;         /* Cyan Technology eCOG2 microprocessor */
public static Machine EM_SCORE7 => 135;        /* Sunplus S+core7 RISC processor */
public static Machine EM_DSP24 => 136;         /* New Japan Radio (NJR) 24-bit DSP Processor */
public static Machine EM_VIDEOCORE3 => 137;    /* Broadcom VideoCore III processor */
public static Machine EM_LATTICEMICO32 => 138; /* RISC processor for Lattice FPGA architecture */
public static Machine EM_SE_C17 => 139;        /* Seiko Epson C17 family */
public static Machine EM_TI_C6000 => 140;      /* The Texas Instruments TMS320C6000 DSP family */
public static Machine EM_TI_C2000 => 141;      /* The Texas Instruments TMS320C2000 DSP family */
public static Machine EM_TI_C5500 => 142;      /* The Texas Instruments TMS320C55x DSP family */
public static Machine EM_TI_ARP32 => 143;      /* Texas Instruments Application Specific RISC Processor, 32bit fetch */
public static Machine EM_TI_PRU => 144;        /* Texas Instruments Programmable Realtime Unit */
public static Machine EM_MMDSP_PLUS => 160;    /* STMicroelectronics 64bit VLIW Data Signal Processor */
public static Machine EM_CYPRESS_M8C => 161;   /* Cypress M8C microprocessor */
public static Machine EM_R32C => 162;          /* Renesas R32C series microprocessors */
public static Machine EM_TRIMEDIA => 163;      /* NXP Semiconductors TriMedia architecture family */
public static Machine EM_QDSP6 => 164;         /* QUALCOMM DSP6 Processor */
public static Machine EM_8051 => 165;          /* Intel 8051 and variants */
public static Machine EM_STXP7X => 166;        /* STMicroelectronics STxP7x family of configurable and extensible RISC processors */
public static Machine EM_NDS32 => 167;         /* Andes Technology compact code size embedded RISC processor family */
public static Machine EM_ECOG1 => 168;         /* Cyan Technology eCOG1X family */
public static Machine EM_ECOG1X => 168;        /* Cyan Technology eCOG1X family */
public static Machine EM_MAXQ30 => 169;        /* Dallas Semiconductor MAXQ30 Core Micro-controllers */
public static Machine EM_XIMO16 => 170;        /* New Japan Radio (NJR) 16-bit DSP Processor */
public static Machine EM_MANIK => 171;         /* M2000 Reconfigurable RISC Microprocessor */
public static Machine EM_CRAYNV2 => 172;       /* Cray Inc. NV2 vector architecture */
public static Machine EM_RX => 173;            /* Renesas RX family */
public static Machine EM_METAG => 174;         /* Imagination Technologies META processor architecture */
public static Machine EM_MCST_ELBRUS => 175;   /* MCST Elbrus general purpose hardware architecture */
public static Machine EM_ECOG16 => 176;        /* Cyan Technology eCOG16 family */
public static Machine EM_CR16 => 177;          /* National Semiconductor CompactRISC CR16 16-bit microprocessor */
public static Machine EM_ETPU => 178;          /* Freescale Extended Time Processing Unit */
public static Machine EM_SLE9X => 179;         /* Infineon Technologies SLE9X core */
public static Machine EM_L10M => 180;          /* Intel L10M */
public static Machine EM_K10M => 181;          /* Intel K10M */
public static Machine EM_AARCH64 => 183;       /* ARM 64-bit Architecture (AArch64) */
public static Machine EM_AVR32 => 185;         /* Atmel Corporation 32-bit microprocessor family */
public static Machine EM_STM8 => 186;          /* STMicroeletronics STM8 8-bit microcontroller */
public static Machine EM_TILE64 => 187;        /* Tilera TILE64 multicore architecture family */
public static Machine EM_TILEPRO => 188;       /* Tilera TILEPro multicore architecture family */
public static Machine EM_MICROBLAZE => 189;    /* Xilinx MicroBlaze 32-bit RISC soft processor core */
public static Machine EM_CUDA => 190;          /* NVIDIA CUDA architecture */
public static Machine EM_TILEGX => 191;        /* Tilera TILE-Gx multicore architecture family */
public static Machine EM_CLOUDSHIELD => 192;   /* CloudShield architecture family */
public static Machine EM_COREA_1ST => 193;     /* KIPO-KAIST Core-A 1st generation processor family */
public static Machine EM_COREA_2ND => 194;     /* KIPO-KAIST Core-A 2nd generation processor family */
public static Machine EM_ARC_COMPACT2 => 195;  /* Synopsys ARCompact V2 */
public static Machine EM_OPEN8 => 196;         /* Open8 8-bit RISC soft processor core */
public static Machine EM_RL78 => 197;          /* Renesas RL78 family */
public static Machine EM_VIDEOCORE5 => 198;    /* Broadcom VideoCore V processor */
public static Machine EM_78KOR => 199;         /* Renesas 78KOR family */
public static Machine EM_56800EX => 200;       /* Freescale 56800EX Digital Signal Controller (DSC) */
public static Machine EM_BA1 => 201;           /* Beyond BA1 CPU architecture */
public static Machine EM_BA2 => 202;           /* Beyond BA2 CPU architecture */
public static Machine EM_XCORE => 203;         /* XMOS xCORE processor family */
public static Machine EM_MCHP_PIC => 204;      /* Microchip 8-bit PIC(r) family */
public static Machine EM_INTEL205 => 205;      /* Reserved by Intel */
public static Machine EM_INTEL206 => 206;      /* Reserved by Intel */
public static Machine EM_INTEL207 => 207;      /* Reserved by Intel */
public static Machine EM_INTEL208 => 208;      /* Reserved by Intel */
public static Machine EM_INTEL209 => 209;      /* Reserved by Intel */
public static Machine EM_KM32 => 210;          /* KM211 KM32 32-bit processor */
public static Machine EM_KMX32 => 211;         /* KM211 KMX32 32-bit processor */
public static Machine EM_KMX16 => 212;         /* KM211 KMX16 16-bit processor */
public static Machine EM_KMX8 => 213;          /* KM211 KMX8 8-bit processor */
public static Machine EM_KVARC => 214;         /* KM211 KVARC processor */
public static Machine EM_CDP => 215;           /* Paneve CDP architecture family */
public static Machine EM_COGE => 216;          /* Cognitive Smart Memory Processor */
public static Machine EM_COOL => 217;          /* Bluechip Systems CoolEngine */
public static Machine EM_NORC => 218;          /* Nanoradio Optimized RISC */
public static Machine EM_CSR_KALIMBA => 219;   /* CSR Kalimba architecture family */
public static Machine EM_Z80 => 220;           /* Zilog Z80 */
public static Machine EM_VISIUM => 221;        /* Controls and Data Services VISIUMcore processor */
public static Machine EM_FT32 => 222;          /* FTDI Chip FT32 high performance 32-bit RISC architecture */
public static Machine EM_MOXIE => 223;         /* Moxie processor family */
public static Machine EM_AMDGPU => 224;        /* AMD GPU architecture */
public static Machine EM_RISCV => 243;         /* RISC-V */
public static Machine EM_LANAI => 244;         /* Lanai 32-bit processor */
public static Machine EM_BPF => 247;           /* Linux BPF – in-kernel virtual machine */
public static Machine EM_LOONGARCH => 258;     /* LoongArch */
public static Machine EM_486 => 6;            /* Intel i486. */
public static Machine EM_MIPS_RS4_BE => 10;    /* MIPS R4000 Big-Endian */
public static Machine EM_ALPHA_STD => 41;      /* Digital Alpha (standard value). */
public static Machine EM_ALPHA => 0x9026;          /* Alpha (written in the absence of an ABI) */

/* Non-standard or deprecated. */
internal static slice<intName> machineStrings = new intName[]{
    new(0, "EM_NONE"u8),
    new(1, "EM_M32"u8),
    new(2, "EM_SPARC"u8),
    new(3, "EM_386"u8),
    new(4, "EM_68K"u8),
    new(5, "EM_88K"u8),
    new(7, "EM_860"u8),
    new(8, "EM_MIPS"u8),
    new(9, "EM_S370"u8),
    new(10, "EM_MIPS_RS3_LE"u8),
    new(15, "EM_PARISC"u8),
    new(17, "EM_VPP500"u8),
    new(18, "EM_SPARC32PLUS"u8),
    new(19, "EM_960"u8),
    new(20, "EM_PPC"u8),
    new(21, "EM_PPC64"u8),
    new(22, "EM_S390"u8),
    new(36, "EM_V800"u8),
    new(37, "EM_FR20"u8),
    new(38, "EM_RH32"u8),
    new(39, "EM_RCE"u8),
    new(40, "EM_ARM"u8),
    new(42, "EM_SH"u8),
    new(43, "EM_SPARCV9"u8),
    new(44, "EM_TRICORE"u8),
    new(45, "EM_ARC"u8),
    new(46, "EM_H8_300"u8),
    new(47, "EM_H8_300H"u8),
    new(48, "EM_H8S"u8),
    new(49, "EM_H8_500"u8),
    new(50, "EM_IA_64"u8),
    new(51, "EM_MIPS_X"u8),
    new(52, "EM_COLDFIRE"u8),
    new(53, "EM_68HC12"u8),
    new(54, "EM_MMA"u8),
    new(55, "EM_PCP"u8),
    new(56, "EM_NCPU"u8),
    new(57, "EM_NDR1"u8),
    new(58, "EM_STARCORE"u8),
    new(59, "EM_ME16"u8),
    new(60, "EM_ST100"u8),
    new(61, "EM_TINYJ"u8),
    new(62, "EM_X86_64"u8),
    new(63, "EM_PDSP"u8),
    new(64, "EM_PDP10"u8),
    new(65, "EM_PDP11"u8),
    new(66, "EM_FX66"u8),
    new(67, "EM_ST9PLUS"u8),
    new(68, "EM_ST7"u8),
    new(69, "EM_68HC16"u8),
    new(70, "EM_68HC11"u8),
    new(71, "EM_68HC08"u8),
    new(72, "EM_68HC05"u8),
    new(73, "EM_SVX"u8),
    new(74, "EM_ST19"u8),
    new(75, "EM_VAX"u8),
    new(76, "EM_CRIS"u8),
    new(77, "EM_JAVELIN"u8),
    new(78, "EM_FIREPATH"u8),
    new(79, "EM_ZSP"u8),
    new(80, "EM_MMIX"u8),
    new(81, "EM_HUANY"u8),
    new(82, "EM_PRISM"u8),
    new(83, "EM_AVR"u8),
    new(84, "EM_FR30"u8),
    new(85, "EM_D10V"u8),
    new(86, "EM_D30V"u8),
    new(87, "EM_V850"u8),
    new(88, "EM_M32R"u8),
    new(89, "EM_MN10300"u8),
    new(90, "EM_MN10200"u8),
    new(91, "EM_PJ"u8),
    new(92, "EM_OPENRISC"u8),
    new(93, "EM_ARC_COMPACT"u8),
    new(94, "EM_XTENSA"u8),
    new(95, "EM_VIDEOCORE"u8),
    new(96, "EM_TMM_GPP"u8),
    new(97, "EM_NS32K"u8),
    new(98, "EM_TPC"u8),
    new(99, "EM_SNP1K"u8),
    new(100, "EM_ST200"u8),
    new(101, "EM_IP2K"u8),
    new(102, "EM_MAX"u8),
    new(103, "EM_CR"u8),
    new(104, "EM_F2MC16"u8),
    new(105, "EM_MSP430"u8),
    new(106, "EM_BLACKFIN"u8),
    new(107, "EM_SE_C33"u8),
    new(108, "EM_SEP"u8),
    new(109, "EM_ARCA"u8),
    new(110, "EM_UNICORE"u8),
    new(111, "EM_EXCESS"u8),
    new(112, "EM_DXP"u8),
    new(113, "EM_ALTERA_NIOS2"u8),
    new(114, "EM_CRX"u8),
    new(115, "EM_XGATE"u8),
    new(116, "EM_C166"u8),
    new(117, "EM_M16C"u8),
    new(118, "EM_DSPIC30F"u8),
    new(119, "EM_CE"u8),
    new(120, "EM_M32C"u8),
    new(131, "EM_TSK3000"u8),
    new(132, "EM_RS08"u8),
    new(133, "EM_SHARC"u8),
    new(134, "EM_ECOG2"u8),
    new(135, "EM_SCORE7"u8),
    new(136, "EM_DSP24"u8),
    new(137, "EM_VIDEOCORE3"u8),
    new(138, "EM_LATTICEMICO32"u8),
    new(139, "EM_SE_C17"u8),
    new(140, "EM_TI_C6000"u8),
    new(141, "EM_TI_C2000"u8),
    new(142, "EM_TI_C5500"u8),
    new(143, "EM_TI_ARP32"u8),
    new(144, "EM_TI_PRU"u8),
    new(160, "EM_MMDSP_PLUS"u8),
    new(161, "EM_CYPRESS_M8C"u8),
    new(162, "EM_R32C"u8),
    new(163, "EM_TRIMEDIA"u8),
    new(164, "EM_QDSP6"u8),
    new(165, "EM_8051"u8),
    new(166, "EM_STXP7X"u8),
    new(167, "EM_NDS32"u8),
    new(168, "EM_ECOG1"u8),
    new(168, "EM_ECOG1X"u8),
    new(169, "EM_MAXQ30"u8),
    new(170, "EM_XIMO16"u8),
    new(171, "EM_MANIK"u8),
    new(172, "EM_CRAYNV2"u8),
    new(173, "EM_RX"u8),
    new(174, "EM_METAG"u8),
    new(175, "EM_MCST_ELBRUS"u8),
    new(176, "EM_ECOG16"u8),
    new(177, "EM_CR16"u8),
    new(178, "EM_ETPU"u8),
    new(179, "EM_SLE9X"u8),
    new(180, "EM_L10M"u8),
    new(181, "EM_K10M"u8),
    new(183, "EM_AARCH64"u8),
    new(185, "EM_AVR32"u8),
    new(186, "EM_STM8"u8),
    new(187, "EM_TILE64"u8),
    new(188, "EM_TILEPRO"u8),
    new(189, "EM_MICROBLAZE"u8),
    new(190, "EM_CUDA"u8),
    new(191, "EM_TILEGX"u8),
    new(192, "EM_CLOUDSHIELD"u8),
    new(193, "EM_COREA_1ST"u8),
    new(194, "EM_COREA_2ND"u8),
    new(195, "EM_ARC_COMPACT2"u8),
    new(196, "EM_OPEN8"u8),
    new(197, "EM_RL78"u8),
    new(198, "EM_VIDEOCORE5"u8),
    new(199, "EM_78KOR"u8),
    new(200, "EM_56800EX"u8),
    new(201, "EM_BA1"u8),
    new(202, "EM_BA2"u8),
    new(203, "EM_XCORE"u8),
    new(204, "EM_MCHP_PIC"u8),
    new(205, "EM_INTEL205"u8),
    new(206, "EM_INTEL206"u8),
    new(207, "EM_INTEL207"u8),
    new(208, "EM_INTEL208"u8),
    new(209, "EM_INTEL209"u8),
    new(210, "EM_KM32"u8),
    new(211, "EM_KMX32"u8),
    new(212, "EM_KMX16"u8),
    new(213, "EM_KMX8"u8),
    new(214, "EM_KVARC"u8),
    new(215, "EM_CDP"u8),
    new(216, "EM_COGE"u8),
    new(217, "EM_COOL"u8),
    new(218, "EM_NORC"u8),
    new(219, "EM_CSR_KALIMBA "u8),
    new(220, "EM_Z80 "u8),
    new(221, "EM_VISIUM "u8),
    new(222, "EM_FT32 "u8),
    new(223, "EM_MOXIE"u8),
    new(224, "EM_AMDGPU"u8),
    new(243, "EM_RISCV"u8),
    new(244, "EM_LANAI"u8),
    new(247, "EM_BPF"u8),
    new(258, "EM_LOONGARCH"u8),
    new(6, "EM_486"u8),
    new(10, "EM_MIPS_RS4_BE"u8),
    new(41, "EM_ALPHA_STD"u8),
    new(0x9026, "EM_ALPHA"u8)
}.slice();

public static @string String(this Machine i) {
    return stringName((uint32)(uint16)i, machineStrings, false);
}

public static @string GoString(this Machine i) {
    return stringName((uint32)(uint16)i, machineStrings, true);
}

partial struct SectionIndex /*num:nint*/;

public static SectionIndex SHN_UNDEF => 0;        /* Undefined, missing, irrelevant. */
public static SectionIndex SHN_LORESERVE => 0xff00;    /* First of reserved range. */
public static SectionIndex SHN_LOPROC => 0xff00;       /* First processor-specific. */
public static SectionIndex SHN_HIPROC => 0xff1f;       /* Last processor-specific. */
public static SectionIndex SHN_LOOS => 0xff20;         /* First operating system-specific. */
public static SectionIndex SHN_HIOS => 0xff3f;         /* Last operating system-specific. */
public static SectionIndex SHN_ABS => 0xfff1;          /* Absolute values. */
public static SectionIndex SHN_COMMON => 0xfff2;       /* Common data. */
public static SectionIndex SHN_XINDEX => 0xffff;       /* Escape; index stored elsewhere. */
public static SectionIndex SHN_HIRESERVE => 0xffff;    /* Last of reserved range. */

internal static slice<intName> shnStrings = new intName[]{
    new(0, "SHN_UNDEF"u8),
    new(0xff00, "SHN_LOPROC"u8),
    new(0xff20, "SHN_LOOS"u8),
    new(0xfff1, "SHN_ABS"u8),
    new(0xfff2, "SHN_COMMON"u8),
    new(0xffff, "SHN_XINDEX"u8)
}.slice();

public static @string String(this SectionIndex i) {
    return stringName((uint32)(nint)i, shnStrings, false);
}

public static @string GoString(this SectionIndex i) {
    return stringName((uint32)(nint)i, shnStrings, true);
}

partial struct SectionType /*num:uint32*/;

public static SectionType SHT_NULL => 0;                  /* inactive */
public static SectionType SHT_PROGBITS => 1;              /* program defined information */
public static SectionType SHT_SYMTAB => 2;                /* symbol table section */
public static SectionType SHT_STRTAB => 3;                /* string table section */
public static SectionType SHT_RELA => 4;                  /* relocation section with addends */
public static SectionType SHT_HASH => 5;                  /* symbol hash table section */
public static SectionType SHT_DYNAMIC => 6;               /* dynamic section */
public static SectionType SHT_NOTE => 7;                  /* note section */
public static SectionType SHT_NOBITS => 8;                /* no space section */
public static SectionType SHT_REL => 9;                   /* relocation section - no addends */
public static SectionType SHT_SHLIB => 10;                 /* reserved - purpose unknown */
public static SectionType SHT_DYNSYM => 11;                /* dynamic symbol table section */
public static SectionType SHT_INIT_ARRAY => 14;            /* Initialization function pointers. */
public static SectionType SHT_FINI_ARRAY => 15;            /* Termination function pointers. */
public static SectionType SHT_PREINIT_ARRAY => 16;         /* Pre-initialization function ptrs. */
public static SectionType SHT_GROUP => 17;                 /* Section group. */
public static SectionType SHT_SYMTAB_SHNDX => 18;          /* Section indexes (see SHN_XINDEX). */
public static SectionType SHT_LOOS => 0x60000000;                  /* First of OS specific semantics */
public static SectionType SHT_GNU_ATTRIBUTES => 0x6ffffff5;        /* GNU object attributes */
public static SectionType SHT_GNU_HASH => 0x6ffffff6;              /* GNU hash table */
public static SectionType SHT_GNU_LIBLIST => 0x6ffffff7;           /* GNU prelink library list */
public static SectionType SHT_GNU_VERDEF => 0x6ffffffd;            /* GNU version definition section */
public static SectionType SHT_GNU_VERNEED => 0x6ffffffe;           /* GNU version needs section */
public static SectionType SHT_GNU_VERSYM => 0x6fffffff;            /* GNU version symbol table */
public static SectionType SHT_HIOS => 0x6fffffff;                  /* Last of OS specific semantics */
public static SectionType SHT_LOPROC => 0x70000000;                /* reserved range for processor */
public static SectionType SHT_MIPS_ABIFLAGS => 0x7000002a;         /* .MIPS.abiflags */
public static SectionType SHT_HIPROC => 0x7fffffff;                /* specific section header types */
public static SectionType SHT_LOUSER => 0x80000000;                /* reserved range for application */
public static SectionType SHT_HIUSER => 0xffffffff;                /* specific indexes */

internal static slice<intName> shtStrings = new intName[]{
    new(0, "SHT_NULL"u8),
    new(1, "SHT_PROGBITS"u8),
    new(2, "SHT_SYMTAB"u8),
    new(3, "SHT_STRTAB"u8),
    new(4, "SHT_RELA"u8),
    new(5, "SHT_HASH"u8),
    new(6, "SHT_DYNAMIC"u8),
    new(7, "SHT_NOTE"u8),
    new(8, "SHT_NOBITS"u8),
    new(9, "SHT_REL"u8),
    new(10, "SHT_SHLIB"u8),
    new(11, "SHT_DYNSYM"u8),
    new(14, "SHT_INIT_ARRAY"u8),
    new(15, "SHT_FINI_ARRAY"u8),
    new(16, "SHT_PREINIT_ARRAY"u8),
    new(17, "SHT_GROUP"u8),
    new(18, "SHT_SYMTAB_SHNDX"u8),
    new(0x60000000, "SHT_LOOS"u8),
    new(0x6ffffff5, "SHT_GNU_ATTRIBUTES"u8),
    new(0x6ffffff6, "SHT_GNU_HASH"u8),
    new(0x6ffffff7, "SHT_GNU_LIBLIST"u8),
    new(0x6ffffffd, "SHT_GNU_VERDEF"u8),
    new(0x6ffffffe, "SHT_GNU_VERNEED"u8),
    new(0x6fffffff, "SHT_GNU_VERSYM"u8),
    new(0x70000000, "SHT_LOPROC"u8),
    new(0x7000002a, "SHT_MIPS_ABIFLAGS"u8),
    new(0x7fffffff, "SHT_HIPROC"u8),
    new(0x80000000U, "SHT_LOUSER"u8),
    new(0xffffffffU, "SHT_HIUSER"u8)
}.slice();

public static @string String(this SectionType i) {
    return stringName((uint32)i, shtStrings, false);
}

public static @string GoString(this SectionType i) {
    return stringName((uint32)i, shtStrings, true);
}

partial struct SectionFlag /*num:uint32*/;

public static SectionFlag SHF_WRITE => 0x1;                   /* Section contains writable data. */
public static SectionFlag SHF_ALLOC => 0x2;                   /* Section occupies memory. */
public static SectionFlag SHF_EXECINSTR => 0x4;               /* Section contains instructions. */
public static SectionFlag SHF_MERGE => 0x10;                   /* Section may be merged. */
public static SectionFlag SHF_STRINGS => 0x20;                 /* Section contains strings. */
public static SectionFlag SHF_INFO_LINK => 0x40;               /* sh_info holds section index. */
public static SectionFlag SHF_LINK_ORDER => 0x80;              /* Special ordering requirements. */
public static SectionFlag SHF_OS_NONCONFORMING => 0x100;        /* OS-specific processing required. */
public static SectionFlag SHF_GROUP => 0x200;                   /* Member of section group. */
public static SectionFlag SHF_TLS => 0x400;                     /* Section contains TLS data. */
public static SectionFlag SHF_COMPRESSED => 0x800;              /* Section is compressed. */
public static SectionFlag SHF_MASKOS => 0x0ff00000;                  /* OS-specific semantics. */
public static SectionFlag SHF_MASKPROC => 0xf0000000;                /* Processor-specific semantics. */

internal static slice<intName> shfStrings = new intName[]{
    new(0x1, "SHF_WRITE"u8),
    new(0x2, "SHF_ALLOC"u8),
    new(0x4, "SHF_EXECINSTR"u8),
    new(0x10, "SHF_MERGE"u8),
    new(0x20, "SHF_STRINGS"u8),
    new(0x40, "SHF_INFO_LINK"u8),
    new(0x80, "SHF_LINK_ORDER"u8),
    new(0x100, "SHF_OS_NONCONFORMING"u8),
    new(0x200, "SHF_GROUP"u8),
    new(0x400, "SHF_TLS"u8),
    new(0x800, "SHF_COMPRESSED"u8)
}.slice();

public static @string String(this SectionFlag i) {
    return flagName((uint32)i, shfStrings, false);
}

public static @string GoString(this SectionFlag i) {
    return flagName((uint32)i, shfStrings, true);
}

partial struct CompressionType /*num:nint*/;

public static CompressionType COMPRESS_ZLIB => 1;          /* ZLIB compression. */
public static CompressionType COMPRESS_ZSTD => 2;          /* ZSTD compression. */
public static CompressionType COMPRESS_LOOS => 0x60000000;          /* First OS-specific. */
public static CompressionType COMPRESS_HIOS => 0x6fffffff;          /* Last OS-specific. */
public static CompressionType COMPRESS_LOPROC => 0x70000000;        /* First processor-specific type. */
public static CompressionType COMPRESS_HIPROC => 0x7fffffff;        /* Last processor-specific type. */

internal static slice<intName> compressionStrings = new intName[]{
    new(1, "COMPRESS_ZLIB"u8),
    new(2, "COMPRESS_ZSTD"u8),
    new(0x60000000, "COMPRESS_LOOS"u8),
    new(0x6fffffff, "COMPRESS_HIOS"u8),
    new(0x70000000, "COMPRESS_LOPROC"u8),
    new(0x7fffffff, "COMPRESS_HIPROC"u8)
}.slice();

public static @string String(this CompressionType i) {
    return stringName((uint32)(nint)i, compressionStrings, false);
}

public static @string GoString(this CompressionType i) {
    return stringName((uint32)(nint)i, compressionStrings, true);
}

partial struct ProgType /*num:nint*/;

public static ProgType PT_NULL => 0;  /* Unused entry. */
public static ProgType PT_LOAD => 1;  /* Loadable segment. */
public static ProgType PT_DYNAMIC => 2; /* Dynamic linking information segment. */
public static ProgType PT_INTERP => 3; /* Pathname of interpreter. */
public static ProgType PT_NOTE => 4;  /* Auxiliary information. */
public static ProgType PT_SHLIB => 5; /* Reserved (not used). */
public static ProgType PT_PHDR => 6;  /* Location of program header itself. */
public static ProgType PT_TLS => 7;   /* Thread local storage segment */
public static ProgType PT_LOOS => 0x60000000;        /* First OS-specific. */
public static ProgType PT_GNU_EH_FRAME => 0x6474e550;        /* Frame unwind information */
public static ProgType PT_GNU_STACK => 0x6474e551;           /* Stack flags */
public static ProgType PT_GNU_RELRO => 0x6474e552;           /* Read only after relocs */
public static ProgType PT_GNU_PROPERTY => 0x6474e553;        /* GNU property */
public static ProgType PT_GNU_MBIND_LO => 0x6474e555;        /* Mbind segments start */
public static ProgType PT_GNU_MBIND_HI => 0x6474f554;        /* Mbind segments finish */
public static ProgType PT_PAX_FLAGS => 0x65041580;        /* PAX flags */
public static ProgType PT_OPENBSD_RANDOMIZE => 0x65a3dbe6;        /* Random data */
public static ProgType PT_OPENBSD_WXNEEDED => 0x65a3dbe7;         /* W^X violations */
public static ProgType PT_OPENBSD_NOBTCFI => 0x65a3dbe8;          /* No branch target CFI */
public static ProgType PT_OPENBSD_BOOTDATA => 0x65a41be6;         /* Boot arguments */
public static ProgType PT_SUNW_EH_FRAME => 0x6474e550;        /* Frame unwind information */
public static ProgType PT_SUNWSTACK => 0x6ffffffb;            /* Stack segment */
public static ProgType PT_HIOS => 0x6fffffff;        /* Last OS-specific. */
public static ProgType PT_LOPROC => 0x70000000;        /* First processor-specific type. */
public static ProgType PT_ARM_ARCHEXT => 0x70000000;        /* Architecture compatibility */
public static ProgType PT_ARM_EXIDX => 0x70000001;          /* Exception unwind tables */
public static ProgType PT_AARCH64_ARCHEXT => 0x70000000;        /* Architecture compatibility */
public static ProgType PT_AARCH64_UNWIND => 0x70000001;         /* Exception unwind tables */
public static ProgType PT_MIPS_REGINFO => 0x70000000;         /* Register usage */
public static ProgType PT_MIPS_RTPROC => 0x70000001;          /* Runtime procedures */
public static ProgType PT_MIPS_OPTIONS => 0x70000002;         /* Options */
public static ProgType PT_MIPS_ABIFLAGS => 0x70000003;        /* ABI flags */
public static ProgType PT_S390_PGSTE => 0x70000000;        /* 4k page table size */
public static ProgType PT_HIPROC => 0x7fffffff;        /* Last processor-specific type. */

// We don't list the processor-dependent ProgTypes,
// as the values overlap.
internal static slice<intName> ptStrings = new intName[]{
    new(0, "PT_NULL"u8),
    new(1, "PT_LOAD"u8),
    new(2, "PT_DYNAMIC"u8),
    new(3, "PT_INTERP"u8),
    new(4, "PT_NOTE"u8),
    new(5, "PT_SHLIB"u8),
    new(6, "PT_PHDR"u8),
    new(7, "PT_TLS"u8),
    new(0x60000000, "PT_LOOS"u8),
    new(0x6474e550, "PT_GNU_EH_FRAME"u8),
    new(0x6474e551, "PT_GNU_STACK"u8),
    new(0x6474e552, "PT_GNU_RELRO"u8),
    new(0x6474e553, "PT_GNU_PROPERTY"u8),
    new(0x65041580, "PT_PAX_FLAGS"u8),
    new(0x65a3dbe6, "PT_OPENBSD_RANDOMIZE"u8),
    new(0x65a3dbe7, "PT_OPENBSD_WXNEEDED"u8),
    new(0x65a41be6, "PT_OPENBSD_BOOTDATA"u8),
    new(0x6ffffffb, "PT_SUNWSTACK"u8),
    new(0x6fffffff, "PT_HIOS"u8),
    new(0x70000000, "PT_LOPROC"u8),
    new(0x7fffffff, "PT_HIPROC"u8)
}.slice();

public static @string String(this ProgType i) {
    return stringName((uint32)(nint)i, ptStrings, false);
}

public static @string GoString(this ProgType i) {
    return stringName((uint32)(nint)i, ptStrings, true);
}

partial struct ProgFlag /*num:uint32*/;

public static ProgFlag PF_X => 0x1;               /* Executable. */
public static ProgFlag PF_W => 0x2;               /* Writable. */
public static ProgFlag PF_R => 0x4;               /* Readable. */
public static ProgFlag PF_MASKOS => 0x0ff00000;          /* Operating system-specific. */
public static ProgFlag PF_MASKPROC => 0xf0000000;        /* Processor-specific. */

internal static slice<intName> pfStrings = new intName[]{
    new(0x1, "PF_X"u8),
    new(0x2, "PF_W"u8),
    new(0x4, "PF_R"u8)
}.slice();

public static @string String(this ProgFlag i) {
    return flagName((uint32)i, pfStrings, false);
}

public static @string GoString(this ProgFlag i) {
    return flagName((uint32)i, pfStrings, true);
}

partial struct DynTag /*num:nint*/;

public static DynTag DT_NULL => 0;        /* Terminating entry. */
public static DynTag DT_NEEDED => 1;      /* String table offset of a needed shared library. */
public static DynTag DT_PLTRELSZ => 2;    /* Total size in bytes of PLT relocations. */
public static DynTag DT_PLTGOT => 3;      /* Processor-dependent address. */
public static DynTag DT_HASH => 4;        /* Address of symbol hash table. */
public static DynTag DT_STRTAB => 5;      /* Address of string table. */
public static DynTag DT_SYMTAB => 6;      /* Address of symbol table. */
public static DynTag DT_RELA => 7;        /* Address of ElfNN_Rela relocations. */
public static DynTag DT_RELASZ => 8;      /* Total size of ElfNN_Rela relocations. */
public static DynTag DT_RELAENT => 9;     /* Size of each ElfNN_Rela relocation entry. */
public static DynTag DT_STRSZ => 10;       /* Size of string table. */
public static DynTag DT_SYMENT => 11;      /* Size of each symbol table entry. */
public static DynTag DT_INIT => 12;        /* Address of initialization function. */
public static DynTag DT_FINI => 13;        /* Address of finalization function. */
public static DynTag DT_SONAME => 14;      /* String table offset of shared object name. */
public static DynTag DT_RPATH => 15;       /* String table offset of library path. [sup] */
public static DynTag DT_SYMBOLIC => 16;    /* Indicates "symbolic" linking. [sup] */
public static DynTag DT_REL => 17;         /* Address of ElfNN_Rel relocations. */
public static DynTag DT_RELSZ => 18;       /* Total size of ElfNN_Rel relocations. */
public static DynTag DT_RELENT => 19;      /* Size of each ElfNN_Rel relocation. */
public static DynTag DT_PLTREL => 20;      /* Type of relocation used for PLT. */
public static DynTag DT_DEBUG => 21;       /* Reserved (not used). */
public static DynTag DT_TEXTREL => 22;     /* Indicates there may be relocations in non-writable segments. [sup] */
public static DynTag DT_JMPREL => 23;      /* Address of PLT relocations. */
public static DynTag DT_BIND_NOW => 24;    /* [sup] */
public static DynTag DT_INIT_ARRAY => 25;  /* Address of the array of pointers to initialization functions */
public static DynTag DT_FINI_ARRAY => 26;  /* Address of the array of pointers to termination functions */
public static DynTag DT_INIT_ARRAYSZ => 27; /* Size in bytes of the array of initialization functions. */
public static DynTag DT_FINI_ARRAYSZ => 28; /* Size in bytes of the array of termination functions. */
public static DynTag DT_RUNPATH => 29;     /* String table offset of a null-terminated library search path string. */
public static DynTag DT_FLAGS => 30;       /* Object specific flag values. */
public static DynTag DT_ENCODING => 32;    /* Values greater than or equal to DT_ENCODING
	   and less than DT_LOOS follow the rules for
	   the interpretation of the d_un union
	   as follows: even == 'd_ptr', even == 'd_val'
	   or none */
public static DynTag DT_PREINIT_ARRAY => 32;  /* Address of the array of pointers to pre-initialization functions. */
public static DynTag DT_PREINIT_ARRAYSZ => 33; /* Size in bytes of the array of pre-initialization functions. */
public static DynTag DT_SYMTAB_SHNDX => 34;   /* Address of SHT_SYMTAB_SHNDX section. */
public static DynTag DT_LOOS => 0x6000000d;        /* First OS-specific */
public static DynTag DT_HIOS => 0x6ffff000;        /* Last OS-specific */
public static DynTag DT_VALRNGLO => 0x6ffffd00;
public static DynTag DT_GNU_PRELINKED => 0x6ffffdf5;
public static DynTag DT_GNU_CONFLICTSZ => 0x6ffffdf6;
public static DynTag DT_GNU_LIBLISTSZ => 0x6ffffdf7;
public static DynTag DT_CHECKSUM => 0x6ffffdf8;
public static DynTag DT_PLTPADSZ => 0x6ffffdf9;
public static DynTag DT_MOVEENT => 0x6ffffdfa;
public static DynTag DT_MOVESZ => 0x6ffffdfb;
public static DynTag DT_FEATURE => 0x6ffffdfc;
public static DynTag DT_POSFLAG_1 => 0x6ffffdfd;
public static DynTag DT_SYMINSZ => 0x6ffffdfe;
public static DynTag DT_SYMINENT => 0x6ffffdff;
public static DynTag DT_VALRNGHI => 0x6ffffdff;
public static DynTag DT_ADDRRNGLO => 0x6ffffe00;
public static DynTag DT_GNU_HASH => 0x6ffffef5;
public static DynTag DT_TLSDESC_PLT => 0x6ffffef6;
public static DynTag DT_TLSDESC_GOT => 0x6ffffef7;
public static DynTag DT_GNU_CONFLICT => 0x6ffffef8;
public static DynTag DT_GNU_LIBLIST => 0x6ffffef9;
public static DynTag DT_CONFIG => 0x6ffffefa;
public static DynTag DT_DEPAUDIT => 0x6ffffefb;
public static DynTag DT_AUDIT => 0x6ffffefc;
public static DynTag DT_PLTPAD => 0x6ffffefd;
public static DynTag DT_MOVETAB => 0x6ffffefe;
public static DynTag DT_SYMINFO => 0x6ffffeff;
public static DynTag DT_ADDRRNGHI => 0x6ffffeff;
public static DynTag DT_VERSYM => 0x6ffffff0;
public static DynTag DT_RELACOUNT => 0x6ffffff9;
public static DynTag DT_RELCOUNT => 0x6ffffffa;
public static DynTag DT_FLAGS_1 => 0x6ffffffb;
public static DynTag DT_VERDEF => 0x6ffffffc;
public static DynTag DT_VERDEFNUM => 0x6ffffffd;
public static DynTag DT_VERNEED => 0x6ffffffe;
public static DynTag DT_VERNEEDNUM => 0x6fffffff;
public static DynTag DT_LOPROC => 0x70000000;        /* First processor-specific type. */
public static DynTag DT_MIPS_RLD_VERSION => 0x70000001;
public static DynTag DT_MIPS_TIME_STAMP => 0x70000002;
public static DynTag DT_MIPS_ICHECKSUM => 0x70000003;
public static DynTag DT_MIPS_IVERSION => 0x70000004;
public static DynTag DT_MIPS_FLAGS => 0x70000005;
public static DynTag DT_MIPS_BASE_ADDRESS => 0x70000006;
public static DynTag DT_MIPS_MSYM => 0x70000007;
public static DynTag DT_MIPS_CONFLICT => 0x70000008;
public static DynTag DT_MIPS_LIBLIST => 0x70000009;
public static DynTag DT_MIPS_LOCAL_GOTNO => 0x7000000a;
public static DynTag DT_MIPS_CONFLICTNO => 0x7000000b;
public static DynTag DT_MIPS_LIBLISTNO => 0x70000010;
public static DynTag DT_MIPS_SYMTABNO => 0x70000011;
public static DynTag DT_MIPS_UNREFEXTNO => 0x70000012;
public static DynTag DT_MIPS_GOTSYM => 0x70000013;
public static DynTag DT_MIPS_HIPAGENO => 0x70000014;
public static DynTag DT_MIPS_RLD_MAP => 0x70000016;
public static DynTag DT_MIPS_DELTA_CLASS => 0x70000017;
public static DynTag DT_MIPS_DELTA_CLASS_NO => 0x70000018;
public static DynTag DT_MIPS_DELTA_INSTANCE => 0x70000019;
public static DynTag DT_MIPS_DELTA_INSTANCE_NO => 0x7000001a;
public static DynTag DT_MIPS_DELTA_RELOC => 0x7000001b;
public static DynTag DT_MIPS_DELTA_RELOC_NO => 0x7000001c;
public static DynTag DT_MIPS_DELTA_SYM => 0x7000001d;
public static DynTag DT_MIPS_DELTA_SYM_NO => 0x7000001e;
public static DynTag DT_MIPS_DELTA_CLASSSYM => 0x70000020;
public static DynTag DT_MIPS_DELTA_CLASSSYM_NO => 0x70000021;
public static DynTag DT_MIPS_CXX_FLAGS => 0x70000022;
public static DynTag DT_MIPS_PIXIE_INIT => 0x70000023;
public static DynTag DT_MIPS_SYMBOL_LIB => 0x70000024;
public static DynTag DT_MIPS_LOCALPAGE_GOTIDX => 0x70000025;
public static DynTag DT_MIPS_LOCAL_GOTIDX => 0x70000026;
public static DynTag DT_MIPS_HIDDEN_GOTIDX => 0x70000027;
public static DynTag DT_MIPS_PROTECTED_GOTIDX => 0x70000028;
public static DynTag DT_MIPS_OPTIONS => 0x70000029;
public static DynTag DT_MIPS_INTERFACE => 0x7000002a;
public static DynTag DT_MIPS_DYNSTR_ALIGN => 0x7000002b;
public static DynTag DT_MIPS_INTERFACE_SIZE => 0x7000002c;
public static DynTag DT_MIPS_RLD_TEXT_RESOLVE_ADDR => 0x7000002d;
public static DynTag DT_MIPS_PERF_SUFFIX => 0x7000002e;
public static DynTag DT_MIPS_COMPACT_SIZE => 0x7000002f;
public static DynTag DT_MIPS_GP_VALUE => 0x70000030;
public static DynTag DT_MIPS_AUX_DYNAMIC => 0x70000031;
public static DynTag DT_MIPS_PLTGOT => 0x70000032;
public static DynTag DT_MIPS_RWPLT => 0x70000034;
public static DynTag DT_MIPS_RLD_MAP_REL => 0x70000035;
public static DynTag DT_PPC_GOT => 0x70000000;
public static DynTag DT_PPC_OPT => 0x70000001;
public static DynTag DT_PPC64_GLINK => 0x70000000;
public static DynTag DT_PPC64_OPD => 0x70000001;
public static DynTag DT_PPC64_OPDSZ => 0x70000002;
public static DynTag DT_PPC64_OPT => 0x70000003;
public static DynTag DT_SPARC_REGISTER => 0x70000001;
public static DynTag DT_AUXILIARY => 0x7ffffffd;
public static DynTag DT_USED => 0x7ffffffe;
public static DynTag DT_FILTER => 0x7fffffff;
public static DynTag DT_HIPROC => 0x7fffffff;        /* Last processor-specific type. */

// We don't list the processor-dependent DynTags,
// as the values overlap.
internal static slice<intName> dtStrings = new intName[]{
    new(0, "DT_NULL"u8),
    new(1, "DT_NEEDED"u8),
    new(2, "DT_PLTRELSZ"u8),
    new(3, "DT_PLTGOT"u8),
    new(4, "DT_HASH"u8),
    new(5, "DT_STRTAB"u8),
    new(6, "DT_SYMTAB"u8),
    new(7, "DT_RELA"u8),
    new(8, "DT_RELASZ"u8),
    new(9, "DT_RELAENT"u8),
    new(10, "DT_STRSZ"u8),
    new(11, "DT_SYMENT"u8),
    new(12, "DT_INIT"u8),
    new(13, "DT_FINI"u8),
    new(14, "DT_SONAME"u8),
    new(15, "DT_RPATH"u8),
    new(16, "DT_SYMBOLIC"u8),
    new(17, "DT_REL"u8),
    new(18, "DT_RELSZ"u8),
    new(19, "DT_RELENT"u8),
    new(20, "DT_PLTREL"u8),
    new(21, "DT_DEBUG"u8),
    new(22, "DT_TEXTREL"u8),
    new(23, "DT_JMPREL"u8),
    new(24, "DT_BIND_NOW"u8),
    new(25, "DT_INIT_ARRAY"u8),
    new(26, "DT_FINI_ARRAY"u8),
    new(27, "DT_INIT_ARRAYSZ"u8),
    new(28, "DT_FINI_ARRAYSZ"u8),
    new(29, "DT_RUNPATH"u8),
    new(30, "DT_FLAGS"u8),
    new(32, "DT_ENCODING"u8),
    new(32, "DT_PREINIT_ARRAY"u8),
    new(33, "DT_PREINIT_ARRAYSZ"u8),
    new(34, "DT_SYMTAB_SHNDX"u8),
    new(0x6000000d, "DT_LOOS"u8),
    new(0x6ffff000, "DT_HIOS"u8),
    new(0x6ffffd00, "DT_VALRNGLO"u8),
    new(0x6ffffdf5, "DT_GNU_PRELINKED"u8),
    new(0x6ffffdf6, "DT_GNU_CONFLICTSZ"u8),
    new(0x6ffffdf7, "DT_GNU_LIBLISTSZ"u8),
    new(0x6ffffdf8, "DT_CHECKSUM"u8),
    new(0x6ffffdf9, "DT_PLTPADSZ"u8),
    new(0x6ffffdfa, "DT_MOVEENT"u8),
    new(0x6ffffdfb, "DT_MOVESZ"u8),
    new(0x6ffffdfc, "DT_FEATURE"u8),
    new(0x6ffffdfd, "DT_POSFLAG_1"u8),
    new(0x6ffffdfe, "DT_SYMINSZ"u8),
    new(0x6ffffdff, "DT_SYMINENT"u8),
    new(0x6ffffdff, "DT_VALRNGHI"u8),
    new(0x6ffffe00, "DT_ADDRRNGLO"u8),
    new(0x6ffffef5, "DT_GNU_HASH"u8),
    new(0x6ffffef6, "DT_TLSDESC_PLT"u8),
    new(0x6ffffef7, "DT_TLSDESC_GOT"u8),
    new(0x6ffffef8, "DT_GNU_CONFLICT"u8),
    new(0x6ffffef9, "DT_GNU_LIBLIST"u8),
    new(0x6ffffefa, "DT_CONFIG"u8),
    new(0x6ffffefb, "DT_DEPAUDIT"u8),
    new(0x6ffffefc, "DT_AUDIT"u8),
    new(0x6ffffefd, "DT_PLTPAD"u8),
    new(0x6ffffefe, "DT_MOVETAB"u8),
    new(0x6ffffeff, "DT_SYMINFO"u8),
    new(0x6ffffeff, "DT_ADDRRNGHI"u8),
    new(0x6ffffff0, "DT_VERSYM"u8),
    new(0x6ffffff9, "DT_RELACOUNT"u8),
    new(0x6ffffffa, "DT_RELCOUNT"u8),
    new(0x6ffffffb, "DT_FLAGS_1"u8),
    new(0x6ffffffc, "DT_VERDEF"u8),
    new(0x6ffffffd, "DT_VERDEFNUM"u8),
    new(0x6ffffffe, "DT_VERNEED"u8),
    new(0x6fffffff, "DT_VERNEEDNUM"u8),
    new(0x70000000, "DT_LOPROC"u8),
    new(0x7ffffffd, "DT_AUXILIARY"u8),
    new(0x7ffffffe, "DT_USED"u8),
    new(0x7fffffff, "DT_FILTER"u8)
}.slice();

public static @string String(this DynTag i) {
    return stringName((uint32)(nint)i, dtStrings, false);
}

public static @string GoString(this DynTag i) {
    return stringName((uint32)(nint)i, dtStrings, true);
}

partial struct DynFlag /*num:nint*/;

public static DynFlag DF_ORIGIN => 0x0001;    /* Indicates that the object being loaded may
	   make reference to the
	   $ORIGIN substitution string */
public static DynFlag DF_SYMBOLIC => 0x0002;    /* Indicates "symbolic" linking. */
public static DynFlag DF_TEXTREL => 0x0004;     /* Indicates there may be relocations in non-writable segments. */
public static DynFlag DF_BIND_NOW => 0x0008;    /* Indicates that the dynamic linker should
	   process all relocations for the object
	   containing this entry before transferring
	   control to the program. */
public static DynFlag DF_STATIC_TLS => 0x0010;    /* Indicates that the shared object or
	   executable contains code using a static
	   thread-local storage scheme. */

internal static slice<intName> dflagStrings = new intName[]{
    new(0x0001, "DF_ORIGIN"u8),
    new(0x0002, "DF_SYMBOLIC"u8),
    new(0x0004, "DF_TEXTREL"u8),
    new(0x0008, "DF_BIND_NOW"u8),
    new(0x0010, "DF_STATIC_TLS"u8)
}.slice();

public static @string String(this DynFlag i) {
    return flagName((uint32)(nint)i, dflagStrings, false);
}

public static @string GoString(this DynFlag i) {
    return flagName((uint32)(nint)i, dflagStrings, true);
}

partial struct DynFlag1 /*num:uint32*/;

public static DynFlag1 DF_1_NOW => 0x00000001;
public static DynFlag1 DF_1_GLOBAL => 0x00000002;
public static DynFlag1 DF_1_GROUP => 0x00000004;
public static DynFlag1 DF_1_NODELETE => 0x00000008;
public static DynFlag1 DF_1_LOADFLTR => 0x00000010;
public static DynFlag1 DF_1_INITFIRST => 0x00000020;
public static DynFlag1 DF_1_NOOPEN => 0x00000040;
public static DynFlag1 DF_1_ORIGIN => 0x00000080;
public static DynFlag1 DF_1_DIRECT => 0x00000100;
public static DynFlag1 DF_1_TRANS => 0x00000200;
public static DynFlag1 DF_1_INTERPOSE => 0x00000400;
public static DynFlag1 DF_1_NODEFLIB => 0x00000800;
public static DynFlag1 DF_1_NODUMP => 0x00001000;
public static DynFlag1 DF_1_CONFALT => 0x00002000;
public static DynFlag1 DF_1_ENDFILTEE => 0x00004000;
public static DynFlag1 DF_1_DISPRELDNE => 0x00008000;
public static DynFlag1 DF_1_DISPRELPND => 0x00010000;
public static DynFlag1 DF_1_NODIRECT => 0x00020000;
public static DynFlag1 DF_1_IGNMULDEF => 0x00040000;
public static DynFlag1 DF_1_NOKSYMS => 0x00080000;
public static DynFlag1 DF_1_NOHDR => 0x00100000;
public static DynFlag1 DF_1_EDITED => 0x00200000;
public static DynFlag1 DF_1_NORELOC => 0x00400000;
public static DynFlag1 DF_1_SYMINTPOSE => 0x00800000;
public static DynFlag1 DF_1_GLOBAUDIT => 0x01000000;
public static DynFlag1 DF_1_SINGLETON => 0x02000000;
public static DynFlag1 DF_1_STUB => 0x04000000;
public static DynFlag1 DF_1_PIE => 0x08000000;
public static DynFlag1 DF_1_KMOD => 0x10000000;
public static DynFlag1 DF_1_WEAKFILTER => 0x20000000;
public static DynFlag1 DF_1_NOCOMMON => 0x40000000;

internal static slice<intName> dflag1Strings = new intName[]{
    new(0x00000001, "DF_1_NOW"u8),
    new(0x00000002, "DF_1_GLOBAL"u8),
    new(0x00000004, "DF_1_GROUP"u8),
    new(0x00000008, "DF_1_NODELETE"u8),
    new(0x00000010, "DF_1_LOADFLTR"u8),
    new(0x00000020, "DF_1_INITFIRST"u8),
    new(0x00000040, "DF_1_NOOPEN"u8),
    new(0x00000080, "DF_1_ORIGIN"u8),
    new(0x00000100, "DF_1_DIRECT"u8),
    new(0x00000200, "DF_1_TRANS"u8),
    new(0x00000400, "DF_1_INTERPOSE"u8),
    new(0x00000800, "DF_1_NODEFLIB"u8),
    new(0x00001000, "DF_1_NODUMP"u8),
    new(0x00002000, "DF_1_CONFALT"u8),
    new(0x00004000, "DF_1_ENDFILTEE"u8),
    new(0x00008000, "DF_1_DISPRELDNE"u8),
    new(0x00010000, "DF_1_DISPRELPND"u8),
    new(0x00020000, "DF_1_NODIRECT"u8),
    new(0x00040000, "DF_1_IGNMULDEF"u8),
    new(0x00080000, "DF_1_NOKSYMS"u8),
    new(0x00100000, "DF_1_NOHDR"u8),
    new(0x00200000, "DF_1_EDITED"u8),
    new(0x00400000, "DF_1_NORELOC"u8),
    new(0x00800000, "DF_1_SYMINTPOSE"u8),
    new(0x01000000, "DF_1_GLOBAUDIT"u8),
    new(0x02000000, "DF_1_SINGLETON"u8),
    new(0x04000000, "DF_1_STUB"u8),
    new(0x08000000, "DF_1_PIE"u8),
    new(0x10000000, "DF_1_KMOD"u8),
    new(0x20000000, "DF_1_WEAKFILTER"u8),
    new(0x40000000, "DF_1_NOCOMMON"u8)
}.slice();

public static @string String(this DynFlag1 i) {
    return flagName((uint32)i, dflag1Strings, false);
}

public static @string GoString(this DynFlag1 i) {
    return flagName((uint32)i, dflag1Strings, true);
}

partial struct NType /*num:nint*/;

public static NType NT_PRSTATUS => 1; /* Process status. */
public static NType NT_FPREGSET => 2; /* Floating point registers. */
public static NType NT_PRPSINFO => 3; /* Process state info. */

internal static slice<intName> ntypeStrings = new intName[]{
    new(1, "NT_PRSTATUS"u8),
    new(2, "NT_FPREGSET"u8),
    new(3, "NT_PRPSINFO"u8)
}.slice();

public static @string String(this NType i) {
    return stringName((uint32)(nint)i, ntypeStrings, false);
}

public static @string GoString(this NType i) {
    return stringName((uint32)(nint)i, ntypeStrings, true);
}

partial struct SymBind /*num:nint*/;

public static SymBind STB_LOCAL => 0; /* Local symbol */
public static SymBind STB_GLOBAL => 1; /* Global symbol */
public static SymBind STB_WEAK => 2;  /* like global - lower precedence */
public static SymBind STB_LOOS => 10;  /* Reserved range for operating system */
public static SymBind STB_HIOS => 12;  /*   specific semantics. */
public static SymBind STB_LOPROC => 13; /* reserved range for processor */
public static SymBind STB_HIPROC => 15; /*   specific semantics. */

internal static slice<intName> stbStrings = new intName[]{
    new(0, "STB_LOCAL"u8),
    new(1, "STB_GLOBAL"u8),
    new(2, "STB_WEAK"u8),
    new(10, "STB_LOOS"u8),
    new(12, "STB_HIOS"u8),
    new(13, "STB_LOPROC"u8),
    new(15, "STB_HIPROC"u8)
}.slice();

public static @string String(this SymBind i) {
    return stringName((uint32)(nint)i, stbStrings, false);
}

public static @string GoString(this SymBind i) {
    return stringName((uint32)(nint)i, stbStrings, true);
}

partial struct SymType /*num:nint*/;

public static SymType STT_NOTYPE => 0; /* Unspecified type. */
public static SymType STT_OBJECT => 1; /* Data object. */
public static SymType STT_FUNC => 2;   /* Function. */
public static SymType STT_SECTION => 3; /* Section. */
public static SymType STT_FILE => 4;   /* Source file. */
public static SymType STT_COMMON => 5; /* Uninitialized common block. */
public static SymType STT_TLS => 6;    /* TLS object. */
public static SymType STT_LOOS => 10;   /* Reserved range for operating system */
public static SymType STT_HIOS => 12;   /*   specific semantics. */
public static SymType STT_LOPROC => 13; /* reserved range for processor */
public static SymType STT_HIPROC => 15; /*   specific semantics. */
public static SymType STT_RELC => 8;     /* Complex relocation expression. */
public static SymType STT_SRELC => 9;    /* Signed complex relocation expression. */
public static SymType STT_GNU_IFUNC => 10; /* Indirect code object. */

internal static slice<intName> sttStrings = new intName[]{
    new(0, "STT_NOTYPE"u8),
    new(1, "STT_OBJECT"u8),
    new(2, "STT_FUNC"u8),
    new(3, "STT_SECTION"u8),
    new(4, "STT_FILE"u8),
    new(5, "STT_COMMON"u8),
    new(6, "STT_TLS"u8),
    new(8, "STT_RELC"u8),
    new(9, "STT_SRELC"u8),
    new(10, "STT_LOOS"u8),
    new(12, "STT_HIOS"u8),
    new(13, "STT_LOPROC"u8),
    new(15, "STT_HIPROC"u8)
}.slice();

public static @string String(this SymType i) {
    return stringName((uint32)(nint)i, sttStrings, false);
}

public static @string GoString(this SymType i) {
    return stringName((uint32)(nint)i, sttStrings, true);
}

partial struct SymVis /*num:nint*/;

public static SymVis STV_DEFAULT => 0x0;   /* Default visibility (see binding). */
public static SymVis STV_INTERNAL => 0x1;  /* Special meaning in relocatable objects. */
public static SymVis STV_HIDDEN => 0x2;    /* Not visible. */
public static SymVis STV_PROTECTED => 0x3; /* Visible but not preemptible. */

internal static slice<intName> stvStrings = new intName[]{
    new(0x0, "STV_DEFAULT"u8),
    new(0x1, "STV_INTERNAL"u8),
    new(0x2, "STV_HIDDEN"u8),
    new(0x3, "STV_PROTECTED"u8)
}.slice();

public static @string String(this SymVis i) {
    return stringName((uint32)(nint)i, stvStrings, false);
}

public static @string GoString(this SymVis i) {
    return stringName((uint32)(nint)i, stvStrings, true);
}

partial struct R_X86_64 /*num:nint*/;

/*
 * Relocation types.
 */
public static R_X86_64 R_X86_64_NONE => 0;           /* No relocation. */
public static R_X86_64 R_X86_64_64 => 1;             /* Add 64 bit symbol value. */
public static R_X86_64 R_X86_64_PC32 => 2;           /* PC-relative 32 bit signed sym value. */
public static R_X86_64 R_X86_64_GOT32 => 3;          /* PC-relative 32 bit GOT offset. */
public static R_X86_64 R_X86_64_PLT32 => 4;          /* PC-relative 32 bit PLT offset. */
public static R_X86_64 R_X86_64_COPY => 5;           /* Copy data from shared object. */
public static R_X86_64 R_X86_64_GLOB_DAT => 6;       /* Set GOT entry to data address. */
public static R_X86_64 R_X86_64_JMP_SLOT => 7;       /* Set GOT entry to code address. */
public static R_X86_64 R_X86_64_RELATIVE => 8;       /* Add load address of shared object. */
public static R_X86_64 R_X86_64_GOTPCREL => 9;       /* Add 32 bit signed pcrel offset to GOT. */
public static R_X86_64 R_X86_64_32 => 10;             /* Add 32 bit zero extended symbol value */
public static R_X86_64 R_X86_64_32S => 11;            /* Add 32 bit sign extended symbol value */
public static R_X86_64 R_X86_64_16 => 12;             /* Add 16 bit zero extended symbol value */
public static R_X86_64 R_X86_64_PC16 => 13;           /* Add 16 bit signed extended pc relative symbol value */
public static R_X86_64 R_X86_64_8 => 14;              /* Add 8 bit zero extended symbol value */
public static R_X86_64 R_X86_64_PC8 => 15;            /* Add 8 bit signed extended pc relative symbol value */
public static R_X86_64 R_X86_64_DTPMOD64 => 16;       /* ID of module containing symbol */
public static R_X86_64 R_X86_64_DTPOFF64 => 17;       /* Offset in TLS block */
public static R_X86_64 R_X86_64_TPOFF64 => 18;        /* Offset in static TLS block */
public static R_X86_64 R_X86_64_TLSGD => 19;          /* PC relative offset to GD GOT entry */
public static R_X86_64 R_X86_64_TLSLD => 20;          /* PC relative offset to LD GOT entry */
public static R_X86_64 R_X86_64_DTPOFF32 => 21;       /* Offset in TLS block */
public static R_X86_64 R_X86_64_GOTTPOFF => 22;       /* PC relative offset to IE GOT entry */
public static R_X86_64 R_X86_64_TPOFF32 => 23;        /* Offset in static TLS block */
public static R_X86_64 R_X86_64_PC64 => 24;           /* PC relative 64-bit sign extended symbol value. */
public static R_X86_64 R_X86_64_GOTOFF64 => 25;
public static R_X86_64 R_X86_64_GOTPC32 => 26;
public static R_X86_64 R_X86_64_GOT64 => 27;
public static R_X86_64 R_X86_64_GOTPCREL64 => 28;
public static R_X86_64 R_X86_64_GOTPC64 => 29;
public static R_X86_64 R_X86_64_GOTPLT64 => 30;
public static R_X86_64 R_X86_64_PLTOFF64 => 31;
public static R_X86_64 R_X86_64_SIZE32 => 32;
public static R_X86_64 R_X86_64_SIZE64 => 33;
public static R_X86_64 R_X86_64_GOTPC32_TLSDESC => 34;
public static R_X86_64 R_X86_64_TLSDESC_CALL => 35;
public static R_X86_64 R_X86_64_TLSDESC => 36;
public static R_X86_64 R_X86_64_IRELATIVE => 37;
public static R_X86_64 R_X86_64_RELATIVE64 => 38;
public static R_X86_64 R_X86_64_PC32_BND => 39;
public static R_X86_64 R_X86_64_PLT32_BND => 40;
public static R_X86_64 R_X86_64_GOTPCRELX => 41;
public static R_X86_64 R_X86_64_REX_GOTPCRELX => 42;

internal static slice<intName> rx86_64Strings = new intName[]{
    new(0, "R_X86_64_NONE"u8),
    new(1, "R_X86_64_64"u8),
    new(2, "R_X86_64_PC32"u8),
    new(3, "R_X86_64_GOT32"u8),
    new(4, "R_X86_64_PLT32"u8),
    new(5, "R_X86_64_COPY"u8),
    new(6, "R_X86_64_GLOB_DAT"u8),
    new(7, "R_X86_64_JMP_SLOT"u8),
    new(8, "R_X86_64_RELATIVE"u8),
    new(9, "R_X86_64_GOTPCREL"u8),
    new(10, "R_X86_64_32"u8),
    new(11, "R_X86_64_32S"u8),
    new(12, "R_X86_64_16"u8),
    new(13, "R_X86_64_PC16"u8),
    new(14, "R_X86_64_8"u8),
    new(15, "R_X86_64_PC8"u8),
    new(16, "R_X86_64_DTPMOD64"u8),
    new(17, "R_X86_64_DTPOFF64"u8),
    new(18, "R_X86_64_TPOFF64"u8),
    new(19, "R_X86_64_TLSGD"u8),
    new(20, "R_X86_64_TLSLD"u8),
    new(21, "R_X86_64_DTPOFF32"u8),
    new(22, "R_X86_64_GOTTPOFF"u8),
    new(23, "R_X86_64_TPOFF32"u8),
    new(24, "R_X86_64_PC64"u8),
    new(25, "R_X86_64_GOTOFF64"u8),
    new(26, "R_X86_64_GOTPC32"u8),
    new(27, "R_X86_64_GOT64"u8),
    new(28, "R_X86_64_GOTPCREL64"u8),
    new(29, "R_X86_64_GOTPC64"u8),
    new(30, "R_X86_64_GOTPLT64"u8),
    new(31, "R_X86_64_PLTOFF64"u8),
    new(32, "R_X86_64_SIZE32"u8),
    new(33, "R_X86_64_SIZE64"u8),
    new(34, "R_X86_64_GOTPC32_TLSDESC"u8),
    new(35, "R_X86_64_TLSDESC_CALL"u8),
    new(36, "R_X86_64_TLSDESC"u8),
    new(37, "R_X86_64_IRELATIVE"u8),
    new(38, "R_X86_64_RELATIVE64"u8),
    new(39, "R_X86_64_PC32_BND"u8),
    new(40, "R_X86_64_PLT32_BND"u8),
    new(41, "R_X86_64_GOTPCRELX"u8),
    new(42, "R_X86_64_REX_GOTPCRELX"u8)
}.slice();

public static @string String(this R_X86_64 i) {
    return stringName((uint32)(nint)i, rx86_64Strings, false);
}

public static @string GoString(this R_X86_64 i) {
    return stringName((uint32)(nint)i, rx86_64Strings, true);
}

partial struct R_AARCH64 /*num:nint*/;

public static R_AARCH64 R_AARCH64_NONE => 0;
public static R_AARCH64 R_AARCH64_P32_ABS32 => 1;
public static R_AARCH64 R_AARCH64_P32_ABS16 => 2;
public static R_AARCH64 R_AARCH64_P32_PREL32 => 3;
public static R_AARCH64 R_AARCH64_P32_PREL16 => 4;
public static R_AARCH64 R_AARCH64_P32_MOVW_UABS_G0 => 5;
public static R_AARCH64 R_AARCH64_P32_MOVW_UABS_G0_NC => 6;
public static R_AARCH64 R_AARCH64_P32_MOVW_UABS_G1 => 7;
public static R_AARCH64 R_AARCH64_P32_MOVW_SABS_G0 => 8;
public static R_AARCH64 R_AARCH64_P32_LD_PREL_LO19 => 9;
public static R_AARCH64 R_AARCH64_P32_ADR_PREL_LO21 => 10;
public static R_AARCH64 R_AARCH64_P32_ADR_PREL_PG_HI21 => 11;
public static R_AARCH64 R_AARCH64_P32_ADD_ABS_LO12_NC => 12;
public static R_AARCH64 R_AARCH64_P32_LDST8_ABS_LO12_NC => 13;
public static R_AARCH64 R_AARCH64_P32_LDST16_ABS_LO12_NC => 14;
public static R_AARCH64 R_AARCH64_P32_LDST32_ABS_LO12_NC => 15;
public static R_AARCH64 R_AARCH64_P32_LDST64_ABS_LO12_NC => 16;
public static R_AARCH64 R_AARCH64_P32_LDST128_ABS_LO12_NC => 17;
public static R_AARCH64 R_AARCH64_P32_TSTBR14 => 18;
public static R_AARCH64 R_AARCH64_P32_CONDBR19 => 19;
public static R_AARCH64 R_AARCH64_P32_JUMP26 => 20;
public static R_AARCH64 R_AARCH64_P32_CALL26 => 21;
public static R_AARCH64 R_AARCH64_P32_GOT_LD_PREL19 => 25;
public static R_AARCH64 R_AARCH64_P32_ADR_GOT_PAGE => 26;
public static R_AARCH64 R_AARCH64_P32_LD32_GOT_LO12_NC => 27;
public static R_AARCH64 R_AARCH64_P32_TLSGD_ADR_PAGE21 => 81;
public static R_AARCH64 R_AARCH64_P32_TLSGD_ADD_LO12_NC => 82;
public static R_AARCH64 R_AARCH64_P32_TLSIE_ADR_GOTTPREL_PAGE21 => 103;
public static R_AARCH64 R_AARCH64_P32_TLSIE_LD32_GOTTPREL_LO12_NC => 104;
public static R_AARCH64 R_AARCH64_P32_TLSIE_LD_GOTTPREL_PREL19 => 105;
public static R_AARCH64 R_AARCH64_P32_TLSLE_MOVW_TPREL_G1 => 106;
public static R_AARCH64 R_AARCH64_P32_TLSLE_MOVW_TPREL_G0 => 107;
public static R_AARCH64 R_AARCH64_P32_TLSLE_MOVW_TPREL_G0_NC => 108;
public static R_AARCH64 R_AARCH64_P32_TLSLE_ADD_TPREL_HI12 => 109;
public static R_AARCH64 R_AARCH64_P32_TLSLE_ADD_TPREL_LO12 => 110;
public static R_AARCH64 R_AARCH64_P32_TLSLE_ADD_TPREL_LO12_NC => 111;
public static R_AARCH64 R_AARCH64_P32_TLSDESC_LD_PREL19 => 122;
public static R_AARCH64 R_AARCH64_P32_TLSDESC_ADR_PREL21 => 123;
public static R_AARCH64 R_AARCH64_P32_TLSDESC_ADR_PAGE21 => 124;
public static R_AARCH64 R_AARCH64_P32_TLSDESC_LD32_LO12_NC => 125;
public static R_AARCH64 R_AARCH64_P32_TLSDESC_ADD_LO12_NC => 126;
public static R_AARCH64 R_AARCH64_P32_TLSDESC_CALL => 127;
public static R_AARCH64 R_AARCH64_P32_COPY => 180;
public static R_AARCH64 R_AARCH64_P32_GLOB_DAT => 181;
public static R_AARCH64 R_AARCH64_P32_JUMP_SLOT => 182;
public static R_AARCH64 R_AARCH64_P32_RELATIVE => 183;
public static R_AARCH64 R_AARCH64_P32_TLS_DTPMOD => 184;
public static R_AARCH64 R_AARCH64_P32_TLS_DTPREL => 185;
public static R_AARCH64 R_AARCH64_P32_TLS_TPREL => 186;
public static R_AARCH64 R_AARCH64_P32_TLSDESC => 187;
public static R_AARCH64 R_AARCH64_P32_IRELATIVE => 188;
public static R_AARCH64 R_AARCH64_NULL => 256;
public static R_AARCH64 R_AARCH64_ABS64 => 257;
public static R_AARCH64 R_AARCH64_ABS32 => 258;
public static R_AARCH64 R_AARCH64_ABS16 => 259;
public static R_AARCH64 R_AARCH64_PREL64 => 260;
public static R_AARCH64 R_AARCH64_PREL32 => 261;
public static R_AARCH64 R_AARCH64_PREL16 => 262;
public static R_AARCH64 R_AARCH64_MOVW_UABS_G0 => 263;
public static R_AARCH64 R_AARCH64_MOVW_UABS_G0_NC => 264;
public static R_AARCH64 R_AARCH64_MOVW_UABS_G1 => 265;
public static R_AARCH64 R_AARCH64_MOVW_UABS_G1_NC => 266;
public static R_AARCH64 R_AARCH64_MOVW_UABS_G2 => 267;
public static R_AARCH64 R_AARCH64_MOVW_UABS_G2_NC => 268;
public static R_AARCH64 R_AARCH64_MOVW_UABS_G3 => 269;
public static R_AARCH64 R_AARCH64_MOVW_SABS_G0 => 270;
public static R_AARCH64 R_AARCH64_MOVW_SABS_G1 => 271;
public static R_AARCH64 R_AARCH64_MOVW_SABS_G2 => 272;
public static R_AARCH64 R_AARCH64_LD_PREL_LO19 => 273;
public static R_AARCH64 R_AARCH64_ADR_PREL_LO21 => 274;
public static R_AARCH64 R_AARCH64_ADR_PREL_PG_HI21 => 275;
public static R_AARCH64 R_AARCH64_ADR_PREL_PG_HI21_NC => 276;
public static R_AARCH64 R_AARCH64_ADD_ABS_LO12_NC => 277;
public static R_AARCH64 R_AARCH64_LDST8_ABS_LO12_NC => 278;
public static R_AARCH64 R_AARCH64_TSTBR14 => 279;
public static R_AARCH64 R_AARCH64_CONDBR19 => 280;
public static R_AARCH64 R_AARCH64_JUMP26 => 282;
public static R_AARCH64 R_AARCH64_CALL26 => 283;
public static R_AARCH64 R_AARCH64_LDST16_ABS_LO12_NC => 284;
public static R_AARCH64 R_AARCH64_LDST32_ABS_LO12_NC => 285;
public static R_AARCH64 R_AARCH64_LDST64_ABS_LO12_NC => 286;
public static R_AARCH64 R_AARCH64_LDST128_ABS_LO12_NC => 299;
public static R_AARCH64 R_AARCH64_GOT_LD_PREL19 => 309;
public static R_AARCH64 R_AARCH64_LD64_GOTOFF_LO15 => 310;
public static R_AARCH64 R_AARCH64_ADR_GOT_PAGE => 311;
public static R_AARCH64 R_AARCH64_LD64_GOT_LO12_NC => 312;
public static R_AARCH64 R_AARCH64_LD64_GOTPAGE_LO15 => 313;
public static R_AARCH64 R_AARCH64_TLSGD_ADR_PREL21 => 512;
public static R_AARCH64 R_AARCH64_TLSGD_ADR_PAGE21 => 513;
public static R_AARCH64 R_AARCH64_TLSGD_ADD_LO12_NC => 514;
public static R_AARCH64 R_AARCH64_TLSGD_MOVW_G1 => 515;
public static R_AARCH64 R_AARCH64_TLSGD_MOVW_G0_NC => 516;
public static R_AARCH64 R_AARCH64_TLSLD_ADR_PREL21 => 517;
public static R_AARCH64 R_AARCH64_TLSLD_ADR_PAGE21 => 518;
public static R_AARCH64 R_AARCH64_TLSIE_MOVW_GOTTPREL_G1 => 539;
public static R_AARCH64 R_AARCH64_TLSIE_MOVW_GOTTPREL_G0_NC => 540;
public static R_AARCH64 R_AARCH64_TLSIE_ADR_GOTTPREL_PAGE21 => 541;
public static R_AARCH64 R_AARCH64_TLSIE_LD64_GOTTPREL_LO12_NC => 542;
public static R_AARCH64 R_AARCH64_TLSIE_LD_GOTTPREL_PREL19 => 543;
public static R_AARCH64 R_AARCH64_TLSLE_MOVW_TPREL_G2 => 544;
public static R_AARCH64 R_AARCH64_TLSLE_MOVW_TPREL_G1 => 545;
public static R_AARCH64 R_AARCH64_TLSLE_MOVW_TPREL_G1_NC => 546;
public static R_AARCH64 R_AARCH64_TLSLE_MOVW_TPREL_G0 => 547;
public static R_AARCH64 R_AARCH64_TLSLE_MOVW_TPREL_G0_NC => 548;
public static R_AARCH64 R_AARCH64_TLSLE_ADD_TPREL_HI12 => 549;
public static R_AARCH64 R_AARCH64_TLSLE_ADD_TPREL_LO12 => 550;
public static R_AARCH64 R_AARCH64_TLSLE_ADD_TPREL_LO12_NC => 551;
public static R_AARCH64 R_AARCH64_TLSDESC_LD_PREL19 => 560;
public static R_AARCH64 R_AARCH64_TLSDESC_ADR_PREL21 => 561;
public static R_AARCH64 R_AARCH64_TLSDESC_ADR_PAGE21 => 562;
public static R_AARCH64 R_AARCH64_TLSDESC_LD64_LO12_NC => 563;
public static R_AARCH64 R_AARCH64_TLSDESC_ADD_LO12_NC => 564;
public static R_AARCH64 R_AARCH64_TLSDESC_OFF_G1 => 565;
public static R_AARCH64 R_AARCH64_TLSDESC_OFF_G0_NC => 566;
public static R_AARCH64 R_AARCH64_TLSDESC_LDR => 567;
public static R_AARCH64 R_AARCH64_TLSDESC_ADD => 568;
public static R_AARCH64 R_AARCH64_TLSDESC_CALL => 569;
public static R_AARCH64 R_AARCH64_TLSLE_LDST128_TPREL_LO12 => 570;
public static R_AARCH64 R_AARCH64_TLSLE_LDST128_TPREL_LO12_NC => 571;
public static R_AARCH64 R_AARCH64_TLSLD_LDST128_DTPREL_LO12 => 572;
public static R_AARCH64 R_AARCH64_TLSLD_LDST128_DTPREL_LO12_NC => 573;
public static R_AARCH64 R_AARCH64_COPY => 1024;
public static R_AARCH64 R_AARCH64_GLOB_DAT => 1025;
public static R_AARCH64 R_AARCH64_JUMP_SLOT => 1026;
public static R_AARCH64 R_AARCH64_RELATIVE => 1027;
public static R_AARCH64 R_AARCH64_TLS_DTPMOD64 => 1028;
public static R_AARCH64 R_AARCH64_TLS_DTPREL64 => 1029;
public static R_AARCH64 R_AARCH64_TLS_TPREL64 => 1030;
public static R_AARCH64 R_AARCH64_TLSDESC => 1031;
public static R_AARCH64 R_AARCH64_IRELATIVE => 1032;

internal static slice<intName> raarch64Strings = new intName[]{
    new(0, "R_AARCH64_NONE"u8),
    new(1, "R_AARCH64_P32_ABS32"u8),
    new(2, "R_AARCH64_P32_ABS16"u8),
    new(3, "R_AARCH64_P32_PREL32"u8),
    new(4, "R_AARCH64_P32_PREL16"u8),
    new(5, "R_AARCH64_P32_MOVW_UABS_G0"u8),
    new(6, "R_AARCH64_P32_MOVW_UABS_G0_NC"u8),
    new(7, "R_AARCH64_P32_MOVW_UABS_G1"u8),
    new(8, "R_AARCH64_P32_MOVW_SABS_G0"u8),
    new(9, "R_AARCH64_P32_LD_PREL_LO19"u8),
    new(10, "R_AARCH64_P32_ADR_PREL_LO21"u8),
    new(11, "R_AARCH64_P32_ADR_PREL_PG_HI21"u8),
    new(12, "R_AARCH64_P32_ADD_ABS_LO12_NC"u8),
    new(13, "R_AARCH64_P32_LDST8_ABS_LO12_NC"u8),
    new(14, "R_AARCH64_P32_LDST16_ABS_LO12_NC"u8),
    new(15, "R_AARCH64_P32_LDST32_ABS_LO12_NC"u8),
    new(16, "R_AARCH64_P32_LDST64_ABS_LO12_NC"u8),
    new(17, "R_AARCH64_P32_LDST128_ABS_LO12_NC"u8),
    new(18, "R_AARCH64_P32_TSTBR14"u8),
    new(19, "R_AARCH64_P32_CONDBR19"u8),
    new(20, "R_AARCH64_P32_JUMP26"u8),
    new(21, "R_AARCH64_P32_CALL26"u8),
    new(25, "R_AARCH64_P32_GOT_LD_PREL19"u8),
    new(26, "R_AARCH64_P32_ADR_GOT_PAGE"u8),
    new(27, "R_AARCH64_P32_LD32_GOT_LO12_NC"u8),
    new(81, "R_AARCH64_P32_TLSGD_ADR_PAGE21"u8),
    new(82, "R_AARCH64_P32_TLSGD_ADD_LO12_NC"u8),
    new(103, "R_AARCH64_P32_TLSIE_ADR_GOTTPREL_PAGE21"u8),
    new(104, "R_AARCH64_P32_TLSIE_LD32_GOTTPREL_LO12_NC"u8),
    new(105, "R_AARCH64_P32_TLSIE_LD_GOTTPREL_PREL19"u8),
    new(106, "R_AARCH64_P32_TLSLE_MOVW_TPREL_G1"u8),
    new(107, "R_AARCH64_P32_TLSLE_MOVW_TPREL_G0"u8),
    new(108, "R_AARCH64_P32_TLSLE_MOVW_TPREL_G0_NC"u8),
    new(109, "R_AARCH64_P32_TLSLE_ADD_TPREL_HI12"u8),
    new(110, "R_AARCH64_P32_TLSLE_ADD_TPREL_LO12"u8),
    new(111, "R_AARCH64_P32_TLSLE_ADD_TPREL_LO12_NC"u8),
    new(122, "R_AARCH64_P32_TLSDESC_LD_PREL19"u8),
    new(123, "R_AARCH64_P32_TLSDESC_ADR_PREL21"u8),
    new(124, "R_AARCH64_P32_TLSDESC_ADR_PAGE21"u8),
    new(125, "R_AARCH64_P32_TLSDESC_LD32_LO12_NC"u8),
    new(126, "R_AARCH64_P32_TLSDESC_ADD_LO12_NC"u8),
    new(127, "R_AARCH64_P32_TLSDESC_CALL"u8),
    new(180, "R_AARCH64_P32_COPY"u8),
    new(181, "R_AARCH64_P32_GLOB_DAT"u8),
    new(182, "R_AARCH64_P32_JUMP_SLOT"u8),
    new(183, "R_AARCH64_P32_RELATIVE"u8),
    new(184, "R_AARCH64_P32_TLS_DTPMOD"u8),
    new(185, "R_AARCH64_P32_TLS_DTPREL"u8),
    new(186, "R_AARCH64_P32_TLS_TPREL"u8),
    new(187, "R_AARCH64_P32_TLSDESC"u8),
    new(188, "R_AARCH64_P32_IRELATIVE"u8),
    new(256, "R_AARCH64_NULL"u8),
    new(257, "R_AARCH64_ABS64"u8),
    new(258, "R_AARCH64_ABS32"u8),
    new(259, "R_AARCH64_ABS16"u8),
    new(260, "R_AARCH64_PREL64"u8),
    new(261, "R_AARCH64_PREL32"u8),
    new(262, "R_AARCH64_PREL16"u8),
    new(263, "R_AARCH64_MOVW_UABS_G0"u8),
    new(264, "R_AARCH64_MOVW_UABS_G0_NC"u8),
    new(265, "R_AARCH64_MOVW_UABS_G1"u8),
    new(266, "R_AARCH64_MOVW_UABS_G1_NC"u8),
    new(267, "R_AARCH64_MOVW_UABS_G2"u8),
    new(268, "R_AARCH64_MOVW_UABS_G2_NC"u8),
    new(269, "R_AARCH64_MOVW_UABS_G3"u8),
    new(270, "R_AARCH64_MOVW_SABS_G0"u8),
    new(271, "R_AARCH64_MOVW_SABS_G1"u8),
    new(272, "R_AARCH64_MOVW_SABS_G2"u8),
    new(273, "R_AARCH64_LD_PREL_LO19"u8),
    new(274, "R_AARCH64_ADR_PREL_LO21"u8),
    new(275, "R_AARCH64_ADR_PREL_PG_HI21"u8),
    new(276, "R_AARCH64_ADR_PREL_PG_HI21_NC"u8),
    new(277, "R_AARCH64_ADD_ABS_LO12_NC"u8),
    new(278, "R_AARCH64_LDST8_ABS_LO12_NC"u8),
    new(279, "R_AARCH64_TSTBR14"u8),
    new(280, "R_AARCH64_CONDBR19"u8),
    new(282, "R_AARCH64_JUMP26"u8),
    new(283, "R_AARCH64_CALL26"u8),
    new(284, "R_AARCH64_LDST16_ABS_LO12_NC"u8),
    new(285, "R_AARCH64_LDST32_ABS_LO12_NC"u8),
    new(286, "R_AARCH64_LDST64_ABS_LO12_NC"u8),
    new(299, "R_AARCH64_LDST128_ABS_LO12_NC"u8),
    new(309, "R_AARCH64_GOT_LD_PREL19"u8),
    new(310, "R_AARCH64_LD64_GOTOFF_LO15"u8),
    new(311, "R_AARCH64_ADR_GOT_PAGE"u8),
    new(312, "R_AARCH64_LD64_GOT_LO12_NC"u8),
    new(313, "R_AARCH64_LD64_GOTPAGE_LO15"u8),
    new(512, "R_AARCH64_TLSGD_ADR_PREL21"u8),
    new(513, "R_AARCH64_TLSGD_ADR_PAGE21"u8),
    new(514, "R_AARCH64_TLSGD_ADD_LO12_NC"u8),
    new(515, "R_AARCH64_TLSGD_MOVW_G1"u8),
    new(516, "R_AARCH64_TLSGD_MOVW_G0_NC"u8),
    new(517, "R_AARCH64_TLSLD_ADR_PREL21"u8),
    new(518, "R_AARCH64_TLSLD_ADR_PAGE21"u8),
    new(539, "R_AARCH64_TLSIE_MOVW_GOTTPREL_G1"u8),
    new(540, "R_AARCH64_TLSIE_MOVW_GOTTPREL_G0_NC"u8),
    new(541, "R_AARCH64_TLSIE_ADR_GOTTPREL_PAGE21"u8),
    new(542, "R_AARCH64_TLSIE_LD64_GOTTPREL_LO12_NC"u8),
    new(543, "R_AARCH64_TLSIE_LD_GOTTPREL_PREL19"u8),
    new(544, "R_AARCH64_TLSLE_MOVW_TPREL_G2"u8),
    new(545, "R_AARCH64_TLSLE_MOVW_TPREL_G1"u8),
    new(546, "R_AARCH64_TLSLE_MOVW_TPREL_G1_NC"u8),
    new(547, "R_AARCH64_TLSLE_MOVW_TPREL_G0"u8),
    new(548, "R_AARCH64_TLSLE_MOVW_TPREL_G0_NC"u8),
    new(549, "R_AARCH64_TLSLE_ADD_TPREL_HI12"u8),
    new(550, "R_AARCH64_TLSLE_ADD_TPREL_LO12"u8),
    new(551, "R_AARCH64_TLSLE_ADD_TPREL_LO12_NC"u8),
    new(560, "R_AARCH64_TLSDESC_LD_PREL19"u8),
    new(561, "R_AARCH64_TLSDESC_ADR_PREL21"u8),
    new(562, "R_AARCH64_TLSDESC_ADR_PAGE21"u8),
    new(563, "R_AARCH64_TLSDESC_LD64_LO12_NC"u8),
    new(564, "R_AARCH64_TLSDESC_ADD_LO12_NC"u8),
    new(565, "R_AARCH64_TLSDESC_OFF_G1"u8),
    new(566, "R_AARCH64_TLSDESC_OFF_G0_NC"u8),
    new(567, "R_AARCH64_TLSDESC_LDR"u8),
    new(568, "R_AARCH64_TLSDESC_ADD"u8),
    new(569, "R_AARCH64_TLSDESC_CALL"u8),
    new(570, "R_AARCH64_TLSLE_LDST128_TPREL_LO12"u8),
    new(571, "R_AARCH64_TLSLE_LDST128_TPREL_LO12_NC"u8),
    new(572, "R_AARCH64_TLSLD_LDST128_DTPREL_LO12"u8),
    new(573, "R_AARCH64_TLSLD_LDST128_DTPREL_LO12_NC"u8),
    new(1024, "R_AARCH64_COPY"u8),
    new(1025, "R_AARCH64_GLOB_DAT"u8),
    new(1026, "R_AARCH64_JUMP_SLOT"u8),
    new(1027, "R_AARCH64_RELATIVE"u8),
    new(1028, "R_AARCH64_TLS_DTPMOD64"u8),
    new(1029, "R_AARCH64_TLS_DTPREL64"u8),
    new(1030, "R_AARCH64_TLS_TPREL64"u8),
    new(1031, "R_AARCH64_TLSDESC"u8),
    new(1032, "R_AARCH64_IRELATIVE"u8)
}.slice();

public static @string String(this R_AARCH64 i) {
    return stringName((uint32)(nint)i, raarch64Strings, false);
}

public static @string GoString(this R_AARCH64 i) {
    return stringName((uint32)(nint)i, raarch64Strings, true);
}

partial struct R_ALPHA /*num:nint*/;

public static R_ALPHA R_ALPHA_NONE => 0;          /* No reloc */
public static R_ALPHA R_ALPHA_REFLONG => 1;       /* Direct 32 bit */
public static R_ALPHA R_ALPHA_REFQUAD => 2;       /* Direct 64 bit */
public static R_ALPHA R_ALPHA_GPREL32 => 3;       /* GP relative 32 bit */
public static R_ALPHA R_ALPHA_LITERAL => 4;       /* GP relative 16 bit w/optimization */
public static R_ALPHA R_ALPHA_LITUSE => 5;        /* Optimization hint for LITERAL */
public static R_ALPHA R_ALPHA_GPDISP => 6;        /* Add displacement to GP */
public static R_ALPHA R_ALPHA_BRADDR => 7;        /* PC+4 relative 23 bit shifted */
public static R_ALPHA R_ALPHA_HINT => 8;          /* PC+4 relative 16 bit shifted */
public static R_ALPHA R_ALPHA_SREL16 => 9;        /* PC relative 16 bit */
public static R_ALPHA R_ALPHA_SREL32 => 10;        /* PC relative 32 bit */
public static R_ALPHA R_ALPHA_SREL64 => 11;        /* PC relative 64 bit */
public static R_ALPHA R_ALPHA_OP_PUSH => 12;       /* OP stack push */
public static R_ALPHA R_ALPHA_OP_STORE => 13;      /* OP stack pop and store */
public static R_ALPHA R_ALPHA_OP_PSUB => 14;       /* OP stack subtract */
public static R_ALPHA R_ALPHA_OP_PRSHIFT => 15;    /* OP stack right shift */
public static R_ALPHA R_ALPHA_GPVALUE => 16;
public static R_ALPHA R_ALPHA_GPRELHIGH => 17;
public static R_ALPHA R_ALPHA_GPRELLOW => 18;
public static R_ALPHA R_ALPHA_IMMED_GP_16 => 19;
public static R_ALPHA R_ALPHA_IMMED_GP_HI32 => 20;
public static R_ALPHA R_ALPHA_IMMED_SCN_HI32 => 21;
public static R_ALPHA R_ALPHA_IMMED_BR_HI32 => 22;
public static R_ALPHA R_ALPHA_IMMED_LO32 => 23;
public static R_ALPHA R_ALPHA_COPY => 24;          /* Copy symbol at runtime */
public static R_ALPHA R_ALPHA_GLOB_DAT => 25;      /* Create GOT entry */
public static R_ALPHA R_ALPHA_JMP_SLOT => 26;      /* Create PLT entry */
public static R_ALPHA R_ALPHA_RELATIVE => 27;      /* Adjust by program base */

internal static slice<intName> ralphaStrings = new intName[]{
    new(0, "R_ALPHA_NONE"u8),
    new(1, "R_ALPHA_REFLONG"u8),
    new(2, "R_ALPHA_REFQUAD"u8),
    new(3, "R_ALPHA_GPREL32"u8),
    new(4, "R_ALPHA_LITERAL"u8),
    new(5, "R_ALPHA_LITUSE"u8),
    new(6, "R_ALPHA_GPDISP"u8),
    new(7, "R_ALPHA_BRADDR"u8),
    new(8, "R_ALPHA_HINT"u8),
    new(9, "R_ALPHA_SREL16"u8),
    new(10, "R_ALPHA_SREL32"u8),
    new(11, "R_ALPHA_SREL64"u8),
    new(12, "R_ALPHA_OP_PUSH"u8),
    new(13, "R_ALPHA_OP_STORE"u8),
    new(14, "R_ALPHA_OP_PSUB"u8),
    new(15, "R_ALPHA_OP_PRSHIFT"u8),
    new(16, "R_ALPHA_GPVALUE"u8),
    new(17, "R_ALPHA_GPRELHIGH"u8),
    new(18, "R_ALPHA_GPRELLOW"u8),
    new(19, "R_ALPHA_IMMED_GP_16"u8),
    new(20, "R_ALPHA_IMMED_GP_HI32"u8),
    new(21, "R_ALPHA_IMMED_SCN_HI32"u8),
    new(22, "R_ALPHA_IMMED_BR_HI32"u8),
    new(23, "R_ALPHA_IMMED_LO32"u8),
    new(24, "R_ALPHA_COPY"u8),
    new(25, "R_ALPHA_GLOB_DAT"u8),
    new(26, "R_ALPHA_JMP_SLOT"u8),
    new(27, "R_ALPHA_RELATIVE"u8)
}.slice();

public static @string String(this R_ALPHA i) {
    return stringName((uint32)(nint)i, ralphaStrings, false);
}

public static @string GoString(this R_ALPHA i) {
    return stringName((uint32)(nint)i, ralphaStrings, true);
}

partial struct R_ARM /*num:nint*/;

public static R_ARM R_ARM_NONE => 0;             /* No relocation. */
public static R_ARM R_ARM_PC24 => 1;
public static R_ARM R_ARM_ABS32 => 2;
public static R_ARM R_ARM_REL32 => 3;
public static R_ARM R_ARM_PC13 => 4;
public static R_ARM R_ARM_ABS16 => 5;
public static R_ARM R_ARM_ABS12 => 6;
public static R_ARM R_ARM_THM_ABS5 => 7;
public static R_ARM R_ARM_ABS8 => 8;
public static R_ARM R_ARM_SBREL32 => 9;
public static R_ARM R_ARM_THM_PC22 => 10;
public static R_ARM R_ARM_THM_PC8 => 11;
public static R_ARM R_ARM_AMP_VCALL9 => 12;
public static R_ARM R_ARM_SWI24 => 13;
public static R_ARM R_ARM_THM_SWI8 => 14;
public static R_ARM R_ARM_XPC25 => 15;
public static R_ARM R_ARM_THM_XPC22 => 16;
public static R_ARM R_ARM_TLS_DTPMOD32 => 17;
public static R_ARM R_ARM_TLS_DTPOFF32 => 18;
public static R_ARM R_ARM_TLS_TPOFF32 => 19;
public static R_ARM R_ARM_COPY => 20;              /* Copy data from shared object. */
public static R_ARM R_ARM_GLOB_DAT => 21;          /* Set GOT entry to data address. */
public static R_ARM R_ARM_JUMP_SLOT => 22;         /* Set GOT entry to code address. */
public static R_ARM R_ARM_RELATIVE => 23;          /* Add load address of shared object. */
public static R_ARM R_ARM_GOTOFF => 24;            /* Add GOT-relative symbol address. */
public static R_ARM R_ARM_GOTPC => 25;             /* Add PC-relative GOT table address. */
public static R_ARM R_ARM_GOT32 => 26;             /* Add PC-relative GOT offset. */
public static R_ARM R_ARM_PLT32 => 27;             /* Add PC-relative PLT offset. */
public static R_ARM R_ARM_CALL => 28;
public static R_ARM R_ARM_JUMP24 => 29;
public static R_ARM R_ARM_THM_JUMP24 => 30;
public static R_ARM R_ARM_BASE_ABS => 31;
public static R_ARM R_ARM_ALU_PCREL_7_0 => 32;
public static R_ARM R_ARM_ALU_PCREL_15_8 => 33;
public static R_ARM R_ARM_ALU_PCREL_23_15 => 34;
public static R_ARM R_ARM_LDR_SBREL_11_10_NC => 35;
public static R_ARM R_ARM_ALU_SBREL_19_12_NC => 36;
public static R_ARM R_ARM_ALU_SBREL_27_20_CK => 37;
public static R_ARM R_ARM_TARGET1 => 38;
public static R_ARM R_ARM_SBREL31 => 39;
public static R_ARM R_ARM_V4BX => 40;
public static R_ARM R_ARM_TARGET2 => 41;
public static R_ARM R_ARM_PREL31 => 42;
public static R_ARM R_ARM_MOVW_ABS_NC => 43;
public static R_ARM R_ARM_MOVT_ABS => 44;
public static R_ARM R_ARM_MOVW_PREL_NC => 45;
public static R_ARM R_ARM_MOVT_PREL => 46;
public static R_ARM R_ARM_THM_MOVW_ABS_NC => 47;
public static R_ARM R_ARM_THM_MOVT_ABS => 48;
public static R_ARM R_ARM_THM_MOVW_PREL_NC => 49;
public static R_ARM R_ARM_THM_MOVT_PREL => 50;
public static R_ARM R_ARM_THM_JUMP19 => 51;
public static R_ARM R_ARM_THM_JUMP6 => 52;
public static R_ARM R_ARM_THM_ALU_PREL_11_0 => 53;
public static R_ARM R_ARM_THM_PC12 => 54;
public static R_ARM R_ARM_ABS32_NOI => 55;
public static R_ARM R_ARM_REL32_NOI => 56;
public static R_ARM R_ARM_ALU_PC_G0_NC => 57;
public static R_ARM R_ARM_ALU_PC_G0 => 58;
public static R_ARM R_ARM_ALU_PC_G1_NC => 59;
public static R_ARM R_ARM_ALU_PC_G1 => 60;
public static R_ARM R_ARM_ALU_PC_G2 => 61;
public static R_ARM R_ARM_LDR_PC_G1 => 62;
public static R_ARM R_ARM_LDR_PC_G2 => 63;
public static R_ARM R_ARM_LDRS_PC_G0 => 64;
public static R_ARM R_ARM_LDRS_PC_G1 => 65;
public static R_ARM R_ARM_LDRS_PC_G2 => 66;
public static R_ARM R_ARM_LDC_PC_G0 => 67;
public static R_ARM R_ARM_LDC_PC_G1 => 68;
public static R_ARM R_ARM_LDC_PC_G2 => 69;
public static R_ARM R_ARM_ALU_SB_G0_NC => 70;
public static R_ARM R_ARM_ALU_SB_G0 => 71;
public static R_ARM R_ARM_ALU_SB_G1_NC => 72;
public static R_ARM R_ARM_ALU_SB_G1 => 73;
public static R_ARM R_ARM_ALU_SB_G2 => 74;
public static R_ARM R_ARM_LDR_SB_G0 => 75;
public static R_ARM R_ARM_LDR_SB_G1 => 76;
public static R_ARM R_ARM_LDR_SB_G2 => 77;
public static R_ARM R_ARM_LDRS_SB_G0 => 78;
public static R_ARM R_ARM_LDRS_SB_G1 => 79;
public static R_ARM R_ARM_LDRS_SB_G2 => 80;
public static R_ARM R_ARM_LDC_SB_G0 => 81;
public static R_ARM R_ARM_LDC_SB_G1 => 82;
public static R_ARM R_ARM_LDC_SB_G2 => 83;
public static R_ARM R_ARM_MOVW_BREL_NC => 84;
public static R_ARM R_ARM_MOVT_BREL => 85;
public static R_ARM R_ARM_MOVW_BREL => 86;
public static R_ARM R_ARM_THM_MOVW_BREL_NC => 87;
public static R_ARM R_ARM_THM_MOVT_BREL => 88;
public static R_ARM R_ARM_THM_MOVW_BREL => 89;
public static R_ARM R_ARM_TLS_GOTDESC => 90;
public static R_ARM R_ARM_TLS_CALL => 91;
public static R_ARM R_ARM_TLS_DESCSEQ => 92;
public static R_ARM R_ARM_THM_TLS_CALL => 93;
public static R_ARM R_ARM_PLT32_ABS => 94;
public static R_ARM R_ARM_GOT_ABS => 95;
public static R_ARM R_ARM_GOT_PREL => 96;
public static R_ARM R_ARM_GOT_BREL12 => 97;
public static R_ARM R_ARM_GOTOFF12 => 98;
public static R_ARM R_ARM_GOTRELAX => 99;
public static R_ARM R_ARM_GNU_VTENTRY => 100;
public static R_ARM R_ARM_GNU_VTINHERIT => 101;
public static R_ARM R_ARM_THM_JUMP11 => 102;
public static R_ARM R_ARM_THM_JUMP8 => 103;
public static R_ARM R_ARM_TLS_GD32 => 104;
public static R_ARM R_ARM_TLS_LDM32 => 105;
public static R_ARM R_ARM_TLS_LDO32 => 106;
public static R_ARM R_ARM_TLS_IE32 => 107;
public static R_ARM R_ARM_TLS_LE32 => 108;
public static R_ARM R_ARM_TLS_LDO12 => 109;
public static R_ARM R_ARM_TLS_LE12 => 110;
public static R_ARM R_ARM_TLS_IE12GP => 111;
public static R_ARM R_ARM_PRIVATE_0 => 112;
public static R_ARM R_ARM_PRIVATE_1 => 113;
public static R_ARM R_ARM_PRIVATE_2 => 114;
public static R_ARM R_ARM_PRIVATE_3 => 115;
public static R_ARM R_ARM_PRIVATE_4 => 116;
public static R_ARM R_ARM_PRIVATE_5 => 117;
public static R_ARM R_ARM_PRIVATE_6 => 118;
public static R_ARM R_ARM_PRIVATE_7 => 119;
public static R_ARM R_ARM_PRIVATE_8 => 120;
public static R_ARM R_ARM_PRIVATE_9 => 121;
public static R_ARM R_ARM_PRIVATE_10 => 122;
public static R_ARM R_ARM_PRIVATE_11 => 123;
public static R_ARM R_ARM_PRIVATE_12 => 124;
public static R_ARM R_ARM_PRIVATE_13 => 125;
public static R_ARM R_ARM_PRIVATE_14 => 126;
public static R_ARM R_ARM_PRIVATE_15 => 127;
public static R_ARM R_ARM_ME_TOO => 128;
public static R_ARM R_ARM_THM_TLS_DESCSEQ16 => 129;
public static R_ARM R_ARM_THM_TLS_DESCSEQ32 => 130;
public static R_ARM R_ARM_THM_GOT_BREL12 => 131;
public static R_ARM R_ARM_THM_ALU_ABS_G0_NC => 132;
public static R_ARM R_ARM_THM_ALU_ABS_G1_NC => 133;
public static R_ARM R_ARM_THM_ALU_ABS_G2_NC => 134;
public static R_ARM R_ARM_THM_ALU_ABS_G3 => 135;
public static R_ARM R_ARM_IRELATIVE => 160;
public static R_ARM R_ARM_RXPC25 => 249;
public static R_ARM R_ARM_RSBREL32 => 250;
public static R_ARM R_ARM_THM_RPC22 => 251;
public static R_ARM R_ARM_RREL32 => 252;
public static R_ARM R_ARM_RABS32 => 253;
public static R_ARM R_ARM_RPC24 => 254;
public static R_ARM R_ARM_RBASE => 255;

internal static slice<intName> rarmStrings = new intName[]{
    new(0, "R_ARM_NONE"u8),
    new(1, "R_ARM_PC24"u8),
    new(2, "R_ARM_ABS32"u8),
    new(3, "R_ARM_REL32"u8),
    new(4, "R_ARM_PC13"u8),
    new(5, "R_ARM_ABS16"u8),
    new(6, "R_ARM_ABS12"u8),
    new(7, "R_ARM_THM_ABS5"u8),
    new(8, "R_ARM_ABS8"u8),
    new(9, "R_ARM_SBREL32"u8),
    new(10, "R_ARM_THM_PC22"u8),
    new(11, "R_ARM_THM_PC8"u8),
    new(12, "R_ARM_AMP_VCALL9"u8),
    new(13, "R_ARM_SWI24"u8),
    new(14, "R_ARM_THM_SWI8"u8),
    new(15, "R_ARM_XPC25"u8),
    new(16, "R_ARM_THM_XPC22"u8),
    new(17, "R_ARM_TLS_DTPMOD32"u8),
    new(18, "R_ARM_TLS_DTPOFF32"u8),
    new(19, "R_ARM_TLS_TPOFF32"u8),
    new(20, "R_ARM_COPY"u8),
    new(21, "R_ARM_GLOB_DAT"u8),
    new(22, "R_ARM_JUMP_SLOT"u8),
    new(23, "R_ARM_RELATIVE"u8),
    new(24, "R_ARM_GOTOFF"u8),
    new(25, "R_ARM_GOTPC"u8),
    new(26, "R_ARM_GOT32"u8),
    new(27, "R_ARM_PLT32"u8),
    new(28, "R_ARM_CALL"u8),
    new(29, "R_ARM_JUMP24"u8),
    new(30, "R_ARM_THM_JUMP24"u8),
    new(31, "R_ARM_BASE_ABS"u8),
    new(32, "R_ARM_ALU_PCREL_7_0"u8),
    new(33, "R_ARM_ALU_PCREL_15_8"u8),
    new(34, "R_ARM_ALU_PCREL_23_15"u8),
    new(35, "R_ARM_LDR_SBREL_11_10_NC"u8),
    new(36, "R_ARM_ALU_SBREL_19_12_NC"u8),
    new(37, "R_ARM_ALU_SBREL_27_20_CK"u8),
    new(38, "R_ARM_TARGET1"u8),
    new(39, "R_ARM_SBREL31"u8),
    new(40, "R_ARM_V4BX"u8),
    new(41, "R_ARM_TARGET2"u8),
    new(42, "R_ARM_PREL31"u8),
    new(43, "R_ARM_MOVW_ABS_NC"u8),
    new(44, "R_ARM_MOVT_ABS"u8),
    new(45, "R_ARM_MOVW_PREL_NC"u8),
    new(46, "R_ARM_MOVT_PREL"u8),
    new(47, "R_ARM_THM_MOVW_ABS_NC"u8),
    new(48, "R_ARM_THM_MOVT_ABS"u8),
    new(49, "R_ARM_THM_MOVW_PREL_NC"u8),
    new(50, "R_ARM_THM_MOVT_PREL"u8),
    new(51, "R_ARM_THM_JUMP19"u8),
    new(52, "R_ARM_THM_JUMP6"u8),
    new(53, "R_ARM_THM_ALU_PREL_11_0"u8),
    new(54, "R_ARM_THM_PC12"u8),
    new(55, "R_ARM_ABS32_NOI"u8),
    new(56, "R_ARM_REL32_NOI"u8),
    new(57, "R_ARM_ALU_PC_G0_NC"u8),
    new(58, "R_ARM_ALU_PC_G0"u8),
    new(59, "R_ARM_ALU_PC_G1_NC"u8),
    new(60, "R_ARM_ALU_PC_G1"u8),
    new(61, "R_ARM_ALU_PC_G2"u8),
    new(62, "R_ARM_LDR_PC_G1"u8),
    new(63, "R_ARM_LDR_PC_G2"u8),
    new(64, "R_ARM_LDRS_PC_G0"u8),
    new(65, "R_ARM_LDRS_PC_G1"u8),
    new(66, "R_ARM_LDRS_PC_G2"u8),
    new(67, "R_ARM_LDC_PC_G0"u8),
    new(68, "R_ARM_LDC_PC_G1"u8),
    new(69, "R_ARM_LDC_PC_G2"u8),
    new(70, "R_ARM_ALU_SB_G0_NC"u8),
    new(71, "R_ARM_ALU_SB_G0"u8),
    new(72, "R_ARM_ALU_SB_G1_NC"u8),
    new(73, "R_ARM_ALU_SB_G1"u8),
    new(74, "R_ARM_ALU_SB_G2"u8),
    new(75, "R_ARM_LDR_SB_G0"u8),
    new(76, "R_ARM_LDR_SB_G1"u8),
    new(77, "R_ARM_LDR_SB_G2"u8),
    new(78, "R_ARM_LDRS_SB_G0"u8),
    new(79, "R_ARM_LDRS_SB_G1"u8),
    new(80, "R_ARM_LDRS_SB_G2"u8),
    new(81, "R_ARM_LDC_SB_G0"u8),
    new(82, "R_ARM_LDC_SB_G1"u8),
    new(83, "R_ARM_LDC_SB_G2"u8),
    new(84, "R_ARM_MOVW_BREL_NC"u8),
    new(85, "R_ARM_MOVT_BREL"u8),
    new(86, "R_ARM_MOVW_BREL"u8),
    new(87, "R_ARM_THM_MOVW_BREL_NC"u8),
    new(88, "R_ARM_THM_MOVT_BREL"u8),
    new(89, "R_ARM_THM_MOVW_BREL"u8),
    new(90, "R_ARM_TLS_GOTDESC"u8),
    new(91, "R_ARM_TLS_CALL"u8),
    new(92, "R_ARM_TLS_DESCSEQ"u8),
    new(93, "R_ARM_THM_TLS_CALL"u8),
    new(94, "R_ARM_PLT32_ABS"u8),
    new(95, "R_ARM_GOT_ABS"u8),
    new(96, "R_ARM_GOT_PREL"u8),
    new(97, "R_ARM_GOT_BREL12"u8),
    new(98, "R_ARM_GOTOFF12"u8),
    new(99, "R_ARM_GOTRELAX"u8),
    new(100, "R_ARM_GNU_VTENTRY"u8),
    new(101, "R_ARM_GNU_VTINHERIT"u8),
    new(102, "R_ARM_THM_JUMP11"u8),
    new(103, "R_ARM_THM_JUMP8"u8),
    new(104, "R_ARM_TLS_GD32"u8),
    new(105, "R_ARM_TLS_LDM32"u8),
    new(106, "R_ARM_TLS_LDO32"u8),
    new(107, "R_ARM_TLS_IE32"u8),
    new(108, "R_ARM_TLS_LE32"u8),
    new(109, "R_ARM_TLS_LDO12"u8),
    new(110, "R_ARM_TLS_LE12"u8),
    new(111, "R_ARM_TLS_IE12GP"u8),
    new(112, "R_ARM_PRIVATE_0"u8),
    new(113, "R_ARM_PRIVATE_1"u8),
    new(114, "R_ARM_PRIVATE_2"u8),
    new(115, "R_ARM_PRIVATE_3"u8),
    new(116, "R_ARM_PRIVATE_4"u8),
    new(117, "R_ARM_PRIVATE_5"u8),
    new(118, "R_ARM_PRIVATE_6"u8),
    new(119, "R_ARM_PRIVATE_7"u8),
    new(120, "R_ARM_PRIVATE_8"u8),
    new(121, "R_ARM_PRIVATE_9"u8),
    new(122, "R_ARM_PRIVATE_10"u8),
    new(123, "R_ARM_PRIVATE_11"u8),
    new(124, "R_ARM_PRIVATE_12"u8),
    new(125, "R_ARM_PRIVATE_13"u8),
    new(126, "R_ARM_PRIVATE_14"u8),
    new(127, "R_ARM_PRIVATE_15"u8),
    new(128, "R_ARM_ME_TOO"u8),
    new(129, "R_ARM_THM_TLS_DESCSEQ16"u8),
    new(130, "R_ARM_THM_TLS_DESCSEQ32"u8),
    new(131, "R_ARM_THM_GOT_BREL12"u8),
    new(132, "R_ARM_THM_ALU_ABS_G0_NC"u8),
    new(133, "R_ARM_THM_ALU_ABS_G1_NC"u8),
    new(134, "R_ARM_THM_ALU_ABS_G2_NC"u8),
    new(135, "R_ARM_THM_ALU_ABS_G3"u8),
    new(160, "R_ARM_IRELATIVE"u8),
    new(249, "R_ARM_RXPC25"u8),
    new(250, "R_ARM_RSBREL32"u8),
    new(251, "R_ARM_THM_RPC22"u8),
    new(252, "R_ARM_RREL32"u8),
    new(253, "R_ARM_RABS32"u8),
    new(254, "R_ARM_RPC24"u8),
    new(255, "R_ARM_RBASE"u8)
}.slice();

public static @string String(this R_ARM i) {
    return stringName((uint32)(nint)i, rarmStrings, false);
}

public static @string GoString(this R_ARM i) {
    return stringName((uint32)(nint)i, rarmStrings, true);
}

partial struct R_386 /*num:nint*/;

public static R_386 R_386_NONE => 0;         /* No relocation. */
public static R_386 R_386_32 => 1;           /* Add symbol value. */
public static R_386 R_386_PC32 => 2;         /* Add PC-relative symbol value. */
public static R_386 R_386_GOT32 => 3;        /* Add PC-relative GOT offset. */
public static R_386 R_386_PLT32 => 4;        /* Add PC-relative PLT offset. */
public static R_386 R_386_COPY => 5;         /* Copy data from shared object. */
public static R_386 R_386_GLOB_DAT => 6;     /* Set GOT entry to data address. */
public static R_386 R_386_JMP_SLOT => 7;     /* Set GOT entry to code address. */
public static R_386 R_386_RELATIVE => 8;     /* Add load address of shared object. */
public static R_386 R_386_GOTOFF => 9;       /* Add GOT-relative symbol address. */
public static R_386 R_386_GOTPC => 10;        /* Add PC-relative GOT table address. */
public static R_386 R_386_32PLT => 11;
public static R_386 R_386_TLS_TPOFF => 14;    /* Negative offset in static TLS block */
public static R_386 R_386_TLS_IE => 15;       /* Absolute address of GOT for -ve static TLS */
public static R_386 R_386_TLS_GOTIE => 16;    /* GOT entry for negative static TLS block */
public static R_386 R_386_TLS_LE => 17;       /* Negative offset relative to static TLS */
public static R_386 R_386_TLS_GD => 18;       /* 32 bit offset to GOT (index,off) pair */
public static R_386 R_386_TLS_LDM => 19;      /* 32 bit offset to GOT (index,zero) pair */
public static R_386 R_386_16 => 20;
public static R_386 R_386_PC16 => 21;
public static R_386 R_386_8 => 22;
public static R_386 R_386_PC8 => 23;
public static R_386 R_386_TLS_GD_32 => 24;    /* 32 bit offset to GOT (index,off) pair */
public static R_386 R_386_TLS_GD_PUSH => 25;  /* pushl instruction for Sun ABI GD sequence */
public static R_386 R_386_TLS_GD_CALL => 26;  /* call instruction for Sun ABI GD sequence */
public static R_386 R_386_TLS_GD_POP => 27;   /* popl instruction for Sun ABI GD sequence */
public static R_386 R_386_TLS_LDM_32 => 28;   /* 32 bit offset to GOT (index,zero) pair */
public static R_386 R_386_TLS_LDM_PUSH => 29; /* pushl instruction for Sun ABI LD sequence */
public static R_386 R_386_TLS_LDM_CALL => 30; /* call instruction for Sun ABI LD sequence */
public static R_386 R_386_TLS_LDM_POP => 31;  /* popl instruction for Sun ABI LD sequence */
public static R_386 R_386_TLS_LDO_32 => 32;   /* 32 bit offset from start of TLS block */
public static R_386 R_386_TLS_IE_32 => 33;    /* 32 bit offset to GOT static TLS offset entry */
public static R_386 R_386_TLS_LE_32 => 34;    /* 32 bit offset within static TLS block */
public static R_386 R_386_TLS_DTPMOD32 => 35; /* GOT entry containing TLS index */
public static R_386 R_386_TLS_DTPOFF32 => 36; /* GOT entry containing TLS offset */
public static R_386 R_386_TLS_TPOFF32 => 37;  /* GOT entry of -ve static TLS offset */
public static R_386 R_386_SIZE32 => 38;
public static R_386 R_386_TLS_GOTDESC => 39;
public static R_386 R_386_TLS_DESC_CALL => 40;
public static R_386 R_386_TLS_DESC => 41;
public static R_386 R_386_IRELATIVE => 42;
public static R_386 R_386_GOT32X => 43;

internal static slice<intName> r386Strings = new intName[]{
    new(0, "R_386_NONE"u8),
    new(1, "R_386_32"u8),
    new(2, "R_386_PC32"u8),
    new(3, "R_386_GOT32"u8),
    new(4, "R_386_PLT32"u8),
    new(5, "R_386_COPY"u8),
    new(6, "R_386_GLOB_DAT"u8),
    new(7, "R_386_JMP_SLOT"u8),
    new(8, "R_386_RELATIVE"u8),
    new(9, "R_386_GOTOFF"u8),
    new(10, "R_386_GOTPC"u8),
    new(11, "R_386_32PLT"u8),
    new(14, "R_386_TLS_TPOFF"u8),
    new(15, "R_386_TLS_IE"u8),
    new(16, "R_386_TLS_GOTIE"u8),
    new(17, "R_386_TLS_LE"u8),
    new(18, "R_386_TLS_GD"u8),
    new(19, "R_386_TLS_LDM"u8),
    new(20, "R_386_16"u8),
    new(21, "R_386_PC16"u8),
    new(22, "R_386_8"u8),
    new(23, "R_386_PC8"u8),
    new(24, "R_386_TLS_GD_32"u8),
    new(25, "R_386_TLS_GD_PUSH"u8),
    new(26, "R_386_TLS_GD_CALL"u8),
    new(27, "R_386_TLS_GD_POP"u8),
    new(28, "R_386_TLS_LDM_32"u8),
    new(29, "R_386_TLS_LDM_PUSH"u8),
    new(30, "R_386_TLS_LDM_CALL"u8),
    new(31, "R_386_TLS_LDM_POP"u8),
    new(32, "R_386_TLS_LDO_32"u8),
    new(33, "R_386_TLS_IE_32"u8),
    new(34, "R_386_TLS_LE_32"u8),
    new(35, "R_386_TLS_DTPMOD32"u8),
    new(36, "R_386_TLS_DTPOFF32"u8),
    new(37, "R_386_TLS_TPOFF32"u8),
    new(38, "R_386_SIZE32"u8),
    new(39, "R_386_TLS_GOTDESC"u8),
    new(40, "R_386_TLS_DESC_CALL"u8),
    new(41, "R_386_TLS_DESC"u8),
    new(42, "R_386_IRELATIVE"u8),
    new(43, "R_386_GOT32X"u8)
}.slice();

public static @string String(this R_386 i) {
    return stringName((uint32)(nint)i, r386Strings, false);
}

public static @string GoString(this R_386 i) {
    return stringName((uint32)(nint)i, r386Strings, true);
}

partial struct R_MIPS /*num:nint*/;

public static R_MIPS R_MIPS_NONE => 0;
public static R_MIPS R_MIPS_16 => 1;
public static R_MIPS R_MIPS_32 => 2;
public static R_MIPS R_MIPS_REL32 => 3;
public static R_MIPS R_MIPS_26 => 4;
public static R_MIPS R_MIPS_HI16 => 5;         /* high 16 bits of symbol value */
public static R_MIPS R_MIPS_LO16 => 6;         /* low 16 bits of symbol value */
public static R_MIPS R_MIPS_GPREL16 => 7;      /* GP-relative reference  */
public static R_MIPS R_MIPS_LITERAL => 8;      /* Reference to literal section  */
public static R_MIPS R_MIPS_GOT16 => 9;        /* Reference to global offset table */
public static R_MIPS R_MIPS_PC16 => 10;         /* 16 bit PC relative reference */
public static R_MIPS R_MIPS_CALL16 => 11;       /* 16 bit call through glbl offset tbl */
public static R_MIPS R_MIPS_GPREL32 => 12;
public static R_MIPS R_MIPS_SHIFT5 => 16;
public static R_MIPS R_MIPS_SHIFT6 => 17;
public static R_MIPS R_MIPS_64 => 18;
public static R_MIPS R_MIPS_GOT_DISP => 19;
public static R_MIPS R_MIPS_GOT_PAGE => 20;
public static R_MIPS R_MIPS_GOT_OFST => 21;
public static R_MIPS R_MIPS_GOT_HI16 => 22;
public static R_MIPS R_MIPS_GOT_LO16 => 23;
public static R_MIPS R_MIPS_SUB => 24;
public static R_MIPS R_MIPS_INSERT_A => 25;
public static R_MIPS R_MIPS_INSERT_B => 26;
public static R_MIPS R_MIPS_DELETE => 27;
public static R_MIPS R_MIPS_HIGHER => 28;
public static R_MIPS R_MIPS_HIGHEST => 29;
public static R_MIPS R_MIPS_CALL_HI16 => 30;
public static R_MIPS R_MIPS_CALL_LO16 => 31;
public static R_MIPS R_MIPS_SCN_DISP => 32;
public static R_MIPS R_MIPS_REL16 => 33;
public static R_MIPS R_MIPS_ADD_IMMEDIATE => 34;
public static R_MIPS R_MIPS_PJUMP => 35;
public static R_MIPS R_MIPS_RELGOT => 36;
public static R_MIPS R_MIPS_JALR => 37;
public static R_MIPS R_MIPS_TLS_DTPMOD32 => 38;   /* Module number 32 bit */
public static R_MIPS R_MIPS_TLS_DTPREL32 => 39;   /* Module-relative offset 32 bit */
public static R_MIPS R_MIPS_TLS_DTPMOD64 => 40;   /* Module number 64 bit */
public static R_MIPS R_MIPS_TLS_DTPREL64 => 41;   /* Module-relative offset 64 bit */
public static R_MIPS R_MIPS_TLS_GD => 42;         /* 16 bit GOT offset for GD */
public static R_MIPS R_MIPS_TLS_LDM => 43;        /* 16 bit GOT offset for LDM */
public static R_MIPS R_MIPS_TLS_DTPREL_HI16 => 44; /* Module-relative offset, high 16 bits */
public static R_MIPS R_MIPS_TLS_DTPREL_LO16 => 45; /* Module-relative offset, low 16 bits */
public static R_MIPS R_MIPS_TLS_GOTTPREL => 46;   /* 16 bit GOT offset for IE */
public static R_MIPS R_MIPS_TLS_TPREL32 => 47;    /* TP-relative offset, 32 bit */
public static R_MIPS R_MIPS_TLS_TPREL64 => 48;    /* TP-relative offset, 64 bit */
public static R_MIPS R_MIPS_TLS_TPREL_HI16 => 49; /* TP-relative offset, high 16 bits */
public static R_MIPS R_MIPS_TLS_TPREL_LO16 => 50; /* TP-relative offset, low 16 bits */
public static R_MIPS R_MIPS_PC32 => 248; /* 32 bit PC relative reference */

internal static slice<intName> rmipsStrings = new intName[]{
    new(0, "R_MIPS_NONE"u8),
    new(1, "R_MIPS_16"u8),
    new(2, "R_MIPS_32"u8),
    new(3, "R_MIPS_REL32"u8),
    new(4, "R_MIPS_26"u8),
    new(5, "R_MIPS_HI16"u8),
    new(6, "R_MIPS_LO16"u8),
    new(7, "R_MIPS_GPREL16"u8),
    new(8, "R_MIPS_LITERAL"u8),
    new(9, "R_MIPS_GOT16"u8),
    new(10, "R_MIPS_PC16"u8),
    new(11, "R_MIPS_CALL16"u8),
    new(12, "R_MIPS_GPREL32"u8),
    new(16, "R_MIPS_SHIFT5"u8),
    new(17, "R_MIPS_SHIFT6"u8),
    new(18, "R_MIPS_64"u8),
    new(19, "R_MIPS_GOT_DISP"u8),
    new(20, "R_MIPS_GOT_PAGE"u8),
    new(21, "R_MIPS_GOT_OFST"u8),
    new(22, "R_MIPS_GOT_HI16"u8),
    new(23, "R_MIPS_GOT_LO16"u8),
    new(24, "R_MIPS_SUB"u8),
    new(25, "R_MIPS_INSERT_A"u8),
    new(26, "R_MIPS_INSERT_B"u8),
    new(27, "R_MIPS_DELETE"u8),
    new(28, "R_MIPS_HIGHER"u8),
    new(29, "R_MIPS_HIGHEST"u8),
    new(30, "R_MIPS_CALL_HI16"u8),
    new(31, "R_MIPS_CALL_LO16"u8),
    new(32, "R_MIPS_SCN_DISP"u8),
    new(33, "R_MIPS_REL16"u8),
    new(34, "R_MIPS_ADD_IMMEDIATE"u8),
    new(35, "R_MIPS_PJUMP"u8),
    new(36, "R_MIPS_RELGOT"u8),
    new(37, "R_MIPS_JALR"u8),
    new(38, "R_MIPS_TLS_DTPMOD32"u8),
    new(39, "R_MIPS_TLS_DTPREL32"u8),
    new(40, "R_MIPS_TLS_DTPMOD64"u8),
    new(41, "R_MIPS_TLS_DTPREL64"u8),
    new(42, "R_MIPS_TLS_GD"u8),
    new(43, "R_MIPS_TLS_LDM"u8),
    new(44, "R_MIPS_TLS_DTPREL_HI16"u8),
    new(45, "R_MIPS_TLS_DTPREL_LO16"u8),
    new(46, "R_MIPS_TLS_GOTTPREL"u8),
    new(47, "R_MIPS_TLS_TPREL32"u8),
    new(48, "R_MIPS_TLS_TPREL64"u8),
    new(49, "R_MIPS_TLS_TPREL_HI16"u8),
    new(50, "R_MIPS_TLS_TPREL_LO16"u8),
    new(248, "R_MIPS_PC32"u8)
}.slice();

public static @string String(this R_MIPS i) {
    return stringName((uint32)(nint)i, rmipsStrings, false);
}

public static @string GoString(this R_MIPS i) {
    return stringName((uint32)(nint)i, rmipsStrings, true);
}

partial struct R_LARCH /*num:nint*/;

public static R_LARCH R_LARCH_NONE => 0;
public static R_LARCH R_LARCH_32 => 1;
public static R_LARCH R_LARCH_64 => 2;
public static R_LARCH R_LARCH_RELATIVE => 3;
public static R_LARCH R_LARCH_COPY => 4;
public static R_LARCH R_LARCH_JUMP_SLOT => 5;
public static R_LARCH R_LARCH_TLS_DTPMOD32 => 6;
public static R_LARCH R_LARCH_TLS_DTPMOD64 => 7;
public static R_LARCH R_LARCH_TLS_DTPREL32 => 8;
public static R_LARCH R_LARCH_TLS_DTPREL64 => 9;
public static R_LARCH R_LARCH_TLS_TPREL32 => 10;
public static R_LARCH R_LARCH_TLS_TPREL64 => 11;
public static R_LARCH R_LARCH_IRELATIVE => 12;
public static R_LARCH R_LARCH_MARK_LA => 20;
public static R_LARCH R_LARCH_MARK_PCREL => 21;
public static R_LARCH R_LARCH_SOP_PUSH_PCREL => 22;
public static R_LARCH R_LARCH_SOP_PUSH_ABSOLUTE => 23;
public static R_LARCH R_LARCH_SOP_PUSH_DUP => 24;
public static R_LARCH R_LARCH_SOP_PUSH_GPREL => 25;
public static R_LARCH R_LARCH_SOP_PUSH_TLS_TPREL => 26;
public static R_LARCH R_LARCH_SOP_PUSH_TLS_GOT => 27;
public static R_LARCH R_LARCH_SOP_PUSH_TLS_GD => 28;
public static R_LARCH R_LARCH_SOP_PUSH_PLT_PCREL => 29;
public static R_LARCH R_LARCH_SOP_ASSERT => 30;
public static R_LARCH R_LARCH_SOP_NOT => 31;
public static R_LARCH R_LARCH_SOP_SUB => 32;
public static R_LARCH R_LARCH_SOP_SL => 33;
public static R_LARCH R_LARCH_SOP_SR => 34;
public static R_LARCH R_LARCH_SOP_ADD => 35;
public static R_LARCH R_LARCH_SOP_AND => 36;
public static R_LARCH R_LARCH_SOP_IF_ELSE => 37;
public static R_LARCH R_LARCH_SOP_POP_32_S_10_5 => 38;
public static R_LARCH R_LARCH_SOP_POP_32_U_10_12 => 39;
public static R_LARCH R_LARCH_SOP_POP_32_S_10_12 => 40;
public static R_LARCH R_LARCH_SOP_POP_32_S_10_16 => 41;
public static R_LARCH R_LARCH_SOP_POP_32_S_10_16_S2 => 42;
public static R_LARCH R_LARCH_SOP_POP_32_S_5_20 => 43;
public static R_LARCH R_LARCH_SOP_POP_32_S_0_5_10_16_S2 => 44;
public static R_LARCH R_LARCH_SOP_POP_32_S_0_10_10_16_S2 => 45;
public static R_LARCH R_LARCH_SOP_POP_32_U => 46;
public static R_LARCH R_LARCH_ADD8 => 47;
public static R_LARCH R_LARCH_ADD16 => 48;
public static R_LARCH R_LARCH_ADD24 => 49;
public static R_LARCH R_LARCH_ADD32 => 50;
public static R_LARCH R_LARCH_ADD64 => 51;
public static R_LARCH R_LARCH_SUB8 => 52;
public static R_LARCH R_LARCH_SUB16 => 53;
public static R_LARCH R_LARCH_SUB24 => 54;
public static R_LARCH R_LARCH_SUB32 => 55;
public static R_LARCH R_LARCH_SUB64 => 56;
public static R_LARCH R_LARCH_GNU_VTINHERIT => 57;
public static R_LARCH R_LARCH_GNU_VTENTRY => 58;
public static R_LARCH R_LARCH_B16 => 64;
public static R_LARCH R_LARCH_B21 => 65;
public static R_LARCH R_LARCH_B26 => 66;
public static R_LARCH R_LARCH_ABS_HI20 => 67;
public static R_LARCH R_LARCH_ABS_LO12 => 68;
public static R_LARCH R_LARCH_ABS64_LO20 => 69;
public static R_LARCH R_LARCH_ABS64_HI12 => 70;
public static R_LARCH R_LARCH_PCALA_HI20 => 71;
public static R_LARCH R_LARCH_PCALA_LO12 => 72;
public static R_LARCH R_LARCH_PCALA64_LO20 => 73;
public static R_LARCH R_LARCH_PCALA64_HI12 => 74;
public static R_LARCH R_LARCH_GOT_PC_HI20 => 75;
public static R_LARCH R_LARCH_GOT_PC_LO12 => 76;
public static R_LARCH R_LARCH_GOT64_PC_LO20 => 77;
public static R_LARCH R_LARCH_GOT64_PC_HI12 => 78;
public static R_LARCH R_LARCH_GOT_HI20 => 79;
public static R_LARCH R_LARCH_GOT_LO12 => 80;
public static R_LARCH R_LARCH_GOT64_LO20 => 81;
public static R_LARCH R_LARCH_GOT64_HI12 => 82;
public static R_LARCH R_LARCH_TLS_LE_HI20 => 83;
public static R_LARCH R_LARCH_TLS_LE_LO12 => 84;
public static R_LARCH R_LARCH_TLS_LE64_LO20 => 85;
public static R_LARCH R_LARCH_TLS_LE64_HI12 => 86;
public static R_LARCH R_LARCH_TLS_IE_PC_HI20 => 87;
public static R_LARCH R_LARCH_TLS_IE_PC_LO12 => 88;
public static R_LARCH R_LARCH_TLS_IE64_PC_LO20 => 89;
public static R_LARCH R_LARCH_TLS_IE64_PC_HI12 => 90;
public static R_LARCH R_LARCH_TLS_IE_HI20 => 91;
public static R_LARCH R_LARCH_TLS_IE_LO12 => 92;
public static R_LARCH R_LARCH_TLS_IE64_LO20 => 93;
public static R_LARCH R_LARCH_TLS_IE64_HI12 => 94;
public static R_LARCH R_LARCH_TLS_LD_PC_HI20 => 95;
public static R_LARCH R_LARCH_TLS_LD_HI20 => 96;
public static R_LARCH R_LARCH_TLS_GD_PC_HI20 => 97;
public static R_LARCH R_LARCH_TLS_GD_HI20 => 98;
public static R_LARCH R_LARCH_32_PCREL => 99;
public static R_LARCH R_LARCH_RELAX => 100;
public static R_LARCH R_LARCH_DELETE => 101;
public static R_LARCH R_LARCH_ALIGN => 102;
public static R_LARCH R_LARCH_PCREL20_S2 => 103;
public static R_LARCH R_LARCH_CFA => 104;
public static R_LARCH R_LARCH_ADD6 => 105;
public static R_LARCH R_LARCH_SUB6 => 106;
public static R_LARCH R_LARCH_ADD_ULEB128 => 107;
public static R_LARCH R_LARCH_SUB_ULEB128 => 108;
public static R_LARCH R_LARCH_64_PCREL => 109;

internal static slice<intName> rlarchStrings = new intName[]{
    new(0, "R_LARCH_NONE"u8),
    new(1, "R_LARCH_32"u8),
    new(2, "R_LARCH_64"u8),
    new(3, "R_LARCH_RELATIVE"u8),
    new(4, "R_LARCH_COPY"u8),
    new(5, "R_LARCH_JUMP_SLOT"u8),
    new(6, "R_LARCH_TLS_DTPMOD32"u8),
    new(7, "R_LARCH_TLS_DTPMOD64"u8),
    new(8, "R_LARCH_TLS_DTPREL32"u8),
    new(9, "R_LARCH_TLS_DTPREL64"u8),
    new(10, "R_LARCH_TLS_TPREL32"u8),
    new(11, "R_LARCH_TLS_TPREL64"u8),
    new(12, "R_LARCH_IRELATIVE"u8),
    new(20, "R_LARCH_MARK_LA"u8),
    new(21, "R_LARCH_MARK_PCREL"u8),
    new(22, "R_LARCH_SOP_PUSH_PCREL"u8),
    new(23, "R_LARCH_SOP_PUSH_ABSOLUTE"u8),
    new(24, "R_LARCH_SOP_PUSH_DUP"u8),
    new(25, "R_LARCH_SOP_PUSH_GPREL"u8),
    new(26, "R_LARCH_SOP_PUSH_TLS_TPREL"u8),
    new(27, "R_LARCH_SOP_PUSH_TLS_GOT"u8),
    new(28, "R_LARCH_SOP_PUSH_TLS_GD"u8),
    new(29, "R_LARCH_SOP_PUSH_PLT_PCREL"u8),
    new(30, "R_LARCH_SOP_ASSERT"u8),
    new(31, "R_LARCH_SOP_NOT"u8),
    new(32, "R_LARCH_SOP_SUB"u8),
    new(33, "R_LARCH_SOP_SL"u8),
    new(34, "R_LARCH_SOP_SR"u8),
    new(35, "R_LARCH_SOP_ADD"u8),
    new(36, "R_LARCH_SOP_AND"u8),
    new(37, "R_LARCH_SOP_IF_ELSE"u8),
    new(38, "R_LARCH_SOP_POP_32_S_10_5"u8),
    new(39, "R_LARCH_SOP_POP_32_U_10_12"u8),
    new(40, "R_LARCH_SOP_POP_32_S_10_12"u8),
    new(41, "R_LARCH_SOP_POP_32_S_10_16"u8),
    new(42, "R_LARCH_SOP_POP_32_S_10_16_S2"u8),
    new(43, "R_LARCH_SOP_POP_32_S_5_20"u8),
    new(44, "R_LARCH_SOP_POP_32_S_0_5_10_16_S2"u8),
    new(45, "R_LARCH_SOP_POP_32_S_0_10_10_16_S2"u8),
    new(46, "R_LARCH_SOP_POP_32_U"u8),
    new(47, "R_LARCH_ADD8"u8),
    new(48, "R_LARCH_ADD16"u8),
    new(49, "R_LARCH_ADD24"u8),
    new(50, "R_LARCH_ADD32"u8),
    new(51, "R_LARCH_ADD64"u8),
    new(52, "R_LARCH_SUB8"u8),
    new(53, "R_LARCH_SUB16"u8),
    new(54, "R_LARCH_SUB24"u8),
    new(55, "R_LARCH_SUB32"u8),
    new(56, "R_LARCH_SUB64"u8),
    new(57, "R_LARCH_GNU_VTINHERIT"u8),
    new(58, "R_LARCH_GNU_VTENTRY"u8),
    new(64, "R_LARCH_B16"u8),
    new(65, "R_LARCH_B21"u8),
    new(66, "R_LARCH_B26"u8),
    new(67, "R_LARCH_ABS_HI20"u8),
    new(68, "R_LARCH_ABS_LO12"u8),
    new(69, "R_LARCH_ABS64_LO20"u8),
    new(70, "R_LARCH_ABS64_HI12"u8),
    new(71, "R_LARCH_PCALA_HI20"u8),
    new(72, "R_LARCH_PCALA_LO12"u8),
    new(73, "R_LARCH_PCALA64_LO20"u8),
    new(74, "R_LARCH_PCALA64_HI12"u8),
    new(75, "R_LARCH_GOT_PC_HI20"u8),
    new(76, "R_LARCH_GOT_PC_LO12"u8),
    new(77, "R_LARCH_GOT64_PC_LO20"u8),
    new(78, "R_LARCH_GOT64_PC_HI12"u8),
    new(79, "R_LARCH_GOT_HI20"u8),
    new(80, "R_LARCH_GOT_LO12"u8),
    new(81, "R_LARCH_GOT64_LO20"u8),
    new(82, "R_LARCH_GOT64_HI12"u8),
    new(83, "R_LARCH_TLS_LE_HI20"u8),
    new(84, "R_LARCH_TLS_LE_LO12"u8),
    new(85, "R_LARCH_TLS_LE64_LO20"u8),
    new(86, "R_LARCH_TLS_LE64_HI12"u8),
    new(87, "R_LARCH_TLS_IE_PC_HI20"u8),
    new(88, "R_LARCH_TLS_IE_PC_LO12"u8),
    new(89, "R_LARCH_TLS_IE64_PC_LO20"u8),
    new(90, "R_LARCH_TLS_IE64_PC_HI12"u8),
    new(91, "R_LARCH_TLS_IE_HI20"u8),
    new(92, "R_LARCH_TLS_IE_LO12"u8),
    new(93, "R_LARCH_TLS_IE64_LO20"u8),
    new(94, "R_LARCH_TLS_IE64_HI12"u8),
    new(95, "R_LARCH_TLS_LD_PC_HI20"u8),
    new(96, "R_LARCH_TLS_LD_HI20"u8),
    new(97, "R_LARCH_TLS_GD_PC_HI20"u8),
    new(98, "R_LARCH_TLS_GD_HI20"u8),
    new(99, "R_LARCH_32_PCREL"u8),
    new(100, "R_LARCH_RELAX"u8),
    new(101, "R_LARCH_DELETE"u8),
    new(102, "R_LARCH_ALIGN"u8),
    new(103, "R_LARCH_PCREL20_S2"u8),
    new(104, "R_LARCH_CFA"u8),
    new(105, "R_LARCH_ADD6"u8),
    new(106, "R_LARCH_SUB6"u8),
    new(107, "R_LARCH_ADD_ULEB128"u8),
    new(108, "R_LARCH_SUB_ULEB128"u8),
    new(109, "R_LARCH_64_PCREL"u8)
}.slice();

public static @string String(this R_LARCH i) {
    return stringName((uint32)(nint)i, rlarchStrings, false);
}

public static @string GoString(this R_LARCH i) {
    return stringName((uint32)(nint)i, rlarchStrings, true);
}

partial struct R_PPC /*num:nint*/;

public static R_PPC R_PPC_NONE => 0;           // R_POWERPC_NONE
public static R_PPC R_PPC_ADDR32 => 1;         // R_POWERPC_ADDR32
public static R_PPC R_PPC_ADDR24 => 2;         // R_POWERPC_ADDR24
public static R_PPC R_PPC_ADDR16 => 3;         // R_POWERPC_ADDR16
public static R_PPC R_PPC_ADDR16_LO => 4;      // R_POWERPC_ADDR16_LO
public static R_PPC R_PPC_ADDR16_HI => 5;      // R_POWERPC_ADDR16_HI
public static R_PPC R_PPC_ADDR16_HA => 6;      // R_POWERPC_ADDR16_HA
public static R_PPC R_PPC_ADDR14 => 7;         // R_POWERPC_ADDR14
public static R_PPC R_PPC_ADDR14_BRTAKEN => 8; // R_POWERPC_ADDR14_BRTAKEN
public static R_PPC R_PPC_ADDR14_BRNTAKEN => 9; // R_POWERPC_ADDR14_BRNTAKEN
public static R_PPC R_PPC_REL24 => 10;          // R_POWERPC_REL24
public static R_PPC R_PPC_REL14 => 11;          // R_POWERPC_REL14
public static R_PPC R_PPC_REL14_BRTAKEN => 12;  // R_POWERPC_REL14_BRTAKEN
public static R_PPC R_PPC_REL14_BRNTAKEN => 13; // R_POWERPC_REL14_BRNTAKEN
public static R_PPC R_PPC_GOT16 => 14;          // R_POWERPC_GOT16
public static R_PPC R_PPC_GOT16_LO => 15;       // R_POWERPC_GOT16_LO
public static R_PPC R_PPC_GOT16_HI => 16;       // R_POWERPC_GOT16_HI
public static R_PPC R_PPC_GOT16_HA => 17;       // R_POWERPC_GOT16_HA
public static R_PPC R_PPC_PLTREL24 => 18;
public static R_PPC R_PPC_COPY => 19;           // R_POWERPC_COPY
public static R_PPC R_PPC_GLOB_DAT => 20;       // R_POWERPC_GLOB_DAT
public static R_PPC R_PPC_JMP_SLOT => 21;       // R_POWERPC_JMP_SLOT
public static R_PPC R_PPC_RELATIVE => 22;       // R_POWERPC_RELATIVE
public static R_PPC R_PPC_LOCAL24PC => 23;
public static R_PPC R_PPC_UADDR32 => 24;        // R_POWERPC_UADDR32
public static R_PPC R_PPC_UADDR16 => 25;        // R_POWERPC_UADDR16
public static R_PPC R_PPC_REL32 => 26;          // R_POWERPC_REL32
public static R_PPC R_PPC_PLT32 => 27;          // R_POWERPC_PLT32
public static R_PPC R_PPC_PLTREL32 => 28;       // R_POWERPC_PLTREL32
public static R_PPC R_PPC_PLT16_LO => 29;       // R_POWERPC_PLT16_LO
public static R_PPC R_PPC_PLT16_HI => 30;       // R_POWERPC_PLT16_HI
public static R_PPC R_PPC_PLT16_HA => 31;       // R_POWERPC_PLT16_HA
public static R_PPC R_PPC_SDAREL16 => 32;
public static R_PPC R_PPC_SECTOFF => 33;        // R_POWERPC_SECTOFF
public static R_PPC R_PPC_SECTOFF_LO => 34;     // R_POWERPC_SECTOFF_LO
public static R_PPC R_PPC_SECTOFF_HI => 35;     // R_POWERPC_SECTOFF_HI
public static R_PPC R_PPC_SECTOFF_HA => 36;     // R_POWERPC_SECTOFF_HA
public static R_PPC R_PPC_TLS => 67;            // R_POWERPC_TLS
public static R_PPC R_PPC_DTPMOD32 => 68;       // R_POWERPC_DTPMOD32
public static R_PPC R_PPC_TPREL16 => 69;        // R_POWERPC_TPREL16
public static R_PPC R_PPC_TPREL16_LO => 70;     // R_POWERPC_TPREL16_LO
public static R_PPC R_PPC_TPREL16_HI => 71;     // R_POWERPC_TPREL16_HI
public static R_PPC R_PPC_TPREL16_HA => 72;     // R_POWERPC_TPREL16_HA
public static R_PPC R_PPC_TPREL32 => 73;        // R_POWERPC_TPREL32
public static R_PPC R_PPC_DTPREL16 => 74;       // R_POWERPC_DTPREL16
public static R_PPC R_PPC_DTPREL16_LO => 75;    // R_POWERPC_DTPREL16_LO
public static R_PPC R_PPC_DTPREL16_HI => 76;    // R_POWERPC_DTPREL16_HI
public static R_PPC R_PPC_DTPREL16_HA => 77;    // R_POWERPC_DTPREL16_HA
public static R_PPC R_PPC_DTPREL32 => 78;       // R_POWERPC_DTPREL32
public static R_PPC R_PPC_GOT_TLSGD16 => 79;    // R_POWERPC_GOT_TLSGD16
public static R_PPC R_PPC_GOT_TLSGD16_LO => 80; // R_POWERPC_GOT_TLSGD16_LO
public static R_PPC R_PPC_GOT_TLSGD16_HI => 81; // R_POWERPC_GOT_TLSGD16_HI
public static R_PPC R_PPC_GOT_TLSGD16_HA => 82; // R_POWERPC_GOT_TLSGD16_HA
public static R_PPC R_PPC_GOT_TLSLD16 => 83;    // R_POWERPC_GOT_TLSLD16
public static R_PPC R_PPC_GOT_TLSLD16_LO => 84; // R_POWERPC_GOT_TLSLD16_LO
public static R_PPC R_PPC_GOT_TLSLD16_HI => 85; // R_POWERPC_GOT_TLSLD16_HI
public static R_PPC R_PPC_GOT_TLSLD16_HA => 86; // R_POWERPC_GOT_TLSLD16_HA
public static R_PPC R_PPC_GOT_TPREL16 => 87;    // R_POWERPC_GOT_TPREL16
public static R_PPC R_PPC_GOT_TPREL16_LO => 88; // R_POWERPC_GOT_TPREL16_LO
public static R_PPC R_PPC_GOT_TPREL16_HI => 89; // R_POWERPC_GOT_TPREL16_HI
public static R_PPC R_PPC_GOT_TPREL16_HA => 90; // R_POWERPC_GOT_TPREL16_HA
public static R_PPC R_PPC_EMB_NADDR32 => 101;
public static R_PPC R_PPC_EMB_NADDR16 => 102;
public static R_PPC R_PPC_EMB_NADDR16_LO => 103;
public static R_PPC R_PPC_EMB_NADDR16_HI => 104;
public static R_PPC R_PPC_EMB_NADDR16_HA => 105;
public static R_PPC R_PPC_EMB_SDAI16 => 106;
public static R_PPC R_PPC_EMB_SDA2I16 => 107;
public static R_PPC R_PPC_EMB_SDA2REL => 108;
public static R_PPC R_PPC_EMB_SDA21 => 109;
public static R_PPC R_PPC_EMB_MRKREF => 110;
public static R_PPC R_PPC_EMB_RELSEC16 => 111;
public static R_PPC R_PPC_EMB_RELST_LO => 112;
public static R_PPC R_PPC_EMB_RELST_HI => 113;
public static R_PPC R_PPC_EMB_RELST_HA => 114;
public static R_PPC R_PPC_EMB_BIT_FLD => 115;
public static R_PPC R_PPC_EMB_RELSDA => 116;

internal static slice<intName> rppcStrings = new intName[]{
    new(0, "R_PPC_NONE"u8),
    new(1, "R_PPC_ADDR32"u8),
    new(2, "R_PPC_ADDR24"u8),
    new(3, "R_PPC_ADDR16"u8),
    new(4, "R_PPC_ADDR16_LO"u8),
    new(5, "R_PPC_ADDR16_HI"u8),
    new(6, "R_PPC_ADDR16_HA"u8),
    new(7, "R_PPC_ADDR14"u8),
    new(8, "R_PPC_ADDR14_BRTAKEN"u8),
    new(9, "R_PPC_ADDR14_BRNTAKEN"u8),
    new(10, "R_PPC_REL24"u8),
    new(11, "R_PPC_REL14"u8),
    new(12, "R_PPC_REL14_BRTAKEN"u8),
    new(13, "R_PPC_REL14_BRNTAKEN"u8),
    new(14, "R_PPC_GOT16"u8),
    new(15, "R_PPC_GOT16_LO"u8),
    new(16, "R_PPC_GOT16_HI"u8),
    new(17, "R_PPC_GOT16_HA"u8),
    new(18, "R_PPC_PLTREL24"u8),
    new(19, "R_PPC_COPY"u8),
    new(20, "R_PPC_GLOB_DAT"u8),
    new(21, "R_PPC_JMP_SLOT"u8),
    new(22, "R_PPC_RELATIVE"u8),
    new(23, "R_PPC_LOCAL24PC"u8),
    new(24, "R_PPC_UADDR32"u8),
    new(25, "R_PPC_UADDR16"u8),
    new(26, "R_PPC_REL32"u8),
    new(27, "R_PPC_PLT32"u8),
    new(28, "R_PPC_PLTREL32"u8),
    new(29, "R_PPC_PLT16_LO"u8),
    new(30, "R_PPC_PLT16_HI"u8),
    new(31, "R_PPC_PLT16_HA"u8),
    new(32, "R_PPC_SDAREL16"u8),
    new(33, "R_PPC_SECTOFF"u8),
    new(34, "R_PPC_SECTOFF_LO"u8),
    new(35, "R_PPC_SECTOFF_HI"u8),
    new(36, "R_PPC_SECTOFF_HA"u8),
    new(67, "R_PPC_TLS"u8),
    new(68, "R_PPC_DTPMOD32"u8),
    new(69, "R_PPC_TPREL16"u8),
    new(70, "R_PPC_TPREL16_LO"u8),
    new(71, "R_PPC_TPREL16_HI"u8),
    new(72, "R_PPC_TPREL16_HA"u8),
    new(73, "R_PPC_TPREL32"u8),
    new(74, "R_PPC_DTPREL16"u8),
    new(75, "R_PPC_DTPREL16_LO"u8),
    new(76, "R_PPC_DTPREL16_HI"u8),
    new(77, "R_PPC_DTPREL16_HA"u8),
    new(78, "R_PPC_DTPREL32"u8),
    new(79, "R_PPC_GOT_TLSGD16"u8),
    new(80, "R_PPC_GOT_TLSGD16_LO"u8),
    new(81, "R_PPC_GOT_TLSGD16_HI"u8),
    new(82, "R_PPC_GOT_TLSGD16_HA"u8),
    new(83, "R_PPC_GOT_TLSLD16"u8),
    new(84, "R_PPC_GOT_TLSLD16_LO"u8),
    new(85, "R_PPC_GOT_TLSLD16_HI"u8),
    new(86, "R_PPC_GOT_TLSLD16_HA"u8),
    new(87, "R_PPC_GOT_TPREL16"u8),
    new(88, "R_PPC_GOT_TPREL16_LO"u8),
    new(89, "R_PPC_GOT_TPREL16_HI"u8),
    new(90, "R_PPC_GOT_TPREL16_HA"u8),
    new(101, "R_PPC_EMB_NADDR32"u8),
    new(102, "R_PPC_EMB_NADDR16"u8),
    new(103, "R_PPC_EMB_NADDR16_LO"u8),
    new(104, "R_PPC_EMB_NADDR16_HI"u8),
    new(105, "R_PPC_EMB_NADDR16_HA"u8),
    new(106, "R_PPC_EMB_SDAI16"u8),
    new(107, "R_PPC_EMB_SDA2I16"u8),
    new(108, "R_PPC_EMB_SDA2REL"u8),
    new(109, "R_PPC_EMB_SDA21"u8),
    new(110, "R_PPC_EMB_MRKREF"u8),
    new(111, "R_PPC_EMB_RELSEC16"u8),
    new(112, "R_PPC_EMB_RELST_LO"u8),
    new(113, "R_PPC_EMB_RELST_HI"u8),
    new(114, "R_PPC_EMB_RELST_HA"u8),
    new(115, "R_PPC_EMB_BIT_FLD"u8),
    new(116, "R_PPC_EMB_RELSDA"u8)
}.slice();

public static @string String(this R_PPC i) {
    return stringName((uint32)(nint)i, rppcStrings, false);
}

public static @string GoString(this R_PPC i) {
    return stringName((uint32)(nint)i, rppcStrings, true);
}

partial struct R_PPC64 /*num:nint*/;

public static R_PPC64 R_PPC64_NONE => 0;              // R_POWERPC_NONE
public static R_PPC64 R_PPC64_ADDR32 => 1;            // R_POWERPC_ADDR32
public static R_PPC64 R_PPC64_ADDR24 => 2;            // R_POWERPC_ADDR24
public static R_PPC64 R_PPC64_ADDR16 => 3;            // R_POWERPC_ADDR16
public static R_PPC64 R_PPC64_ADDR16_LO => 4;         // R_POWERPC_ADDR16_LO
public static R_PPC64 R_PPC64_ADDR16_HI => 5;         // R_POWERPC_ADDR16_HI
public static R_PPC64 R_PPC64_ADDR16_HA => 6;         // R_POWERPC_ADDR16_HA
public static R_PPC64 R_PPC64_ADDR14 => 7;            // R_POWERPC_ADDR14
public static R_PPC64 R_PPC64_ADDR14_BRTAKEN => 8;    // R_POWERPC_ADDR14_BRTAKEN
public static R_PPC64 R_PPC64_ADDR14_BRNTAKEN => 9;   // R_POWERPC_ADDR14_BRNTAKEN
public static R_PPC64 R_PPC64_REL24 => 10;             // R_POWERPC_REL24
public static R_PPC64 R_PPC64_REL14 => 11;             // R_POWERPC_REL14
public static R_PPC64 R_PPC64_REL14_BRTAKEN => 12;     // R_POWERPC_REL14_BRTAKEN
public static R_PPC64 R_PPC64_REL14_BRNTAKEN => 13;    // R_POWERPC_REL14_BRNTAKEN
public static R_PPC64 R_PPC64_GOT16 => 14;             // R_POWERPC_GOT16
public static R_PPC64 R_PPC64_GOT16_LO => 15;          // R_POWERPC_GOT16_LO
public static R_PPC64 R_PPC64_GOT16_HI => 16;          // R_POWERPC_GOT16_HI
public static R_PPC64 R_PPC64_GOT16_HA => 17;          // R_POWERPC_GOT16_HA
public static R_PPC64 R_PPC64_COPY => 19;              // R_POWERPC_COPY
public static R_PPC64 R_PPC64_GLOB_DAT => 20;          // R_POWERPC_GLOB_DAT
public static R_PPC64 R_PPC64_JMP_SLOT => 21;          // R_POWERPC_JMP_SLOT
public static R_PPC64 R_PPC64_RELATIVE => 22;          // R_POWERPC_RELATIVE
public static R_PPC64 R_PPC64_UADDR32 => 24;           // R_POWERPC_UADDR32
public static R_PPC64 R_PPC64_UADDR16 => 25;           // R_POWERPC_UADDR16
public static R_PPC64 R_PPC64_REL32 => 26;             // R_POWERPC_REL32
public static R_PPC64 R_PPC64_PLT32 => 27;             // R_POWERPC_PLT32
public static R_PPC64 R_PPC64_PLTREL32 => 28;          // R_POWERPC_PLTREL32
public static R_PPC64 R_PPC64_PLT16_LO => 29;          // R_POWERPC_PLT16_LO
public static R_PPC64 R_PPC64_PLT16_HI => 30;          // R_POWERPC_PLT16_HI
public static R_PPC64 R_PPC64_PLT16_HA => 31;          // R_POWERPC_PLT16_HA
public static R_PPC64 R_PPC64_SECTOFF => 33;           // R_POWERPC_SECTOFF
public static R_PPC64 R_PPC64_SECTOFF_LO => 34;        // R_POWERPC_SECTOFF_LO
public static R_PPC64 R_PPC64_SECTOFF_HI => 35;        // R_POWERPC_SECTOFF_HI
public static R_PPC64 R_PPC64_SECTOFF_HA => 36;        // R_POWERPC_SECTOFF_HA
public static R_PPC64 R_PPC64_REL30 => 37;             // R_POWERPC_ADDR30
public static R_PPC64 R_PPC64_ADDR64 => 38;
public static R_PPC64 R_PPC64_ADDR16_HIGHER => 39;
public static R_PPC64 R_PPC64_ADDR16_HIGHERA => 40;
public static R_PPC64 R_PPC64_ADDR16_HIGHEST => 41;
public static R_PPC64 R_PPC64_ADDR16_HIGHESTA => 42;
public static R_PPC64 R_PPC64_UADDR64 => 43;
public static R_PPC64 R_PPC64_REL64 => 44;
public static R_PPC64 R_PPC64_PLT64 => 45;
public static R_PPC64 R_PPC64_PLTREL64 => 46;
public static R_PPC64 R_PPC64_TOC16 => 47;
public static R_PPC64 R_PPC64_TOC16_LO => 48;
public static R_PPC64 R_PPC64_TOC16_HI => 49;
public static R_PPC64 R_PPC64_TOC16_HA => 50;
public static R_PPC64 R_PPC64_TOC => 51;
public static R_PPC64 R_PPC64_PLTGOT16 => 52;
public static R_PPC64 R_PPC64_PLTGOT16_LO => 53;
public static R_PPC64 R_PPC64_PLTGOT16_HI => 54;
public static R_PPC64 R_PPC64_PLTGOT16_HA => 55;
public static R_PPC64 R_PPC64_ADDR16_DS => 56;
public static R_PPC64 R_PPC64_ADDR16_LO_DS => 57;
public static R_PPC64 R_PPC64_GOT16_DS => 58;
public static R_PPC64 R_PPC64_GOT16_LO_DS => 59;
public static R_PPC64 R_PPC64_PLT16_LO_DS => 60;
public static R_PPC64 R_PPC64_SECTOFF_DS => 61;
public static R_PPC64 R_PPC64_SECTOFF_LO_DS => 62;
public static R_PPC64 R_PPC64_TOC16_DS => 63;
public static R_PPC64 R_PPC64_TOC16_LO_DS => 64;
public static R_PPC64 R_PPC64_PLTGOT16_DS => 65;
public static R_PPC64 R_PPC64_PLTGOT_LO_DS => 66;
public static R_PPC64 R_PPC64_TLS => 67;               // R_POWERPC_TLS
public static R_PPC64 R_PPC64_DTPMOD64 => 68;          // R_POWERPC_DTPMOD64
public static R_PPC64 R_PPC64_TPREL16 => 69;           // R_POWERPC_TPREL16
public static R_PPC64 R_PPC64_TPREL16_LO => 70;        // R_POWERPC_TPREL16_LO
public static R_PPC64 R_PPC64_TPREL16_HI => 71;        // R_POWERPC_TPREL16_HI
public static R_PPC64 R_PPC64_TPREL16_HA => 72;        // R_POWERPC_TPREL16_HA
public static R_PPC64 R_PPC64_TPREL64 => 73;           // R_POWERPC_TPREL64
public static R_PPC64 R_PPC64_DTPREL16 => 74;          // R_POWERPC_DTPREL16
public static R_PPC64 R_PPC64_DTPREL16_LO => 75;       // R_POWERPC_DTPREL16_LO
public static R_PPC64 R_PPC64_DTPREL16_HI => 76;       // R_POWERPC_DTPREL16_HI
public static R_PPC64 R_PPC64_DTPREL16_HA => 77;       // R_POWERPC_DTPREL16_HA
public static R_PPC64 R_PPC64_DTPREL64 => 78;          // R_POWERPC_DTPREL64
public static R_PPC64 R_PPC64_GOT_TLSGD16 => 79;       // R_POWERPC_GOT_TLSGD16
public static R_PPC64 R_PPC64_GOT_TLSGD16_LO => 80;    // R_POWERPC_GOT_TLSGD16_LO
public static R_PPC64 R_PPC64_GOT_TLSGD16_HI => 81;    // R_POWERPC_GOT_TLSGD16_HI
public static R_PPC64 R_PPC64_GOT_TLSGD16_HA => 82;    // R_POWERPC_GOT_TLSGD16_HA
public static R_PPC64 R_PPC64_GOT_TLSLD16 => 83;       // R_POWERPC_GOT_TLSLD16
public static R_PPC64 R_PPC64_GOT_TLSLD16_LO => 84;    // R_POWERPC_GOT_TLSLD16_LO
public static R_PPC64 R_PPC64_GOT_TLSLD16_HI => 85;    // R_POWERPC_GOT_TLSLD16_HI
public static R_PPC64 R_PPC64_GOT_TLSLD16_HA => 86;    // R_POWERPC_GOT_TLSLD16_HA
public static R_PPC64 R_PPC64_GOT_TPREL16_DS => 87;    // R_POWERPC_GOT_TPREL16_DS
public static R_PPC64 R_PPC64_GOT_TPREL16_LO_DS => 88; // R_POWERPC_GOT_TPREL16_LO_DS
public static R_PPC64 R_PPC64_GOT_TPREL16_HI => 89;    // R_POWERPC_GOT_TPREL16_HI
public static R_PPC64 R_PPC64_GOT_TPREL16_HA => 90;    // R_POWERPC_GOT_TPREL16_HA
public static R_PPC64 R_PPC64_GOT_DTPREL16_DS => 91;   // R_POWERPC_GOT_DTPREL16_DS
public static R_PPC64 R_PPC64_GOT_DTPREL16_LO_DS => 92; // R_POWERPC_GOT_DTPREL16_LO_DS
public static R_PPC64 R_PPC64_GOT_DTPREL16_HI => 93;   // R_POWERPC_GOT_DTPREL16_HI
public static R_PPC64 R_PPC64_GOT_DTPREL16_HA => 94;   // R_POWERPC_GOT_DTPREL16_HA
public static R_PPC64 R_PPC64_TPREL16_DS => 95;
public static R_PPC64 R_PPC64_TPREL16_LO_DS => 96;
public static R_PPC64 R_PPC64_TPREL16_HIGHER => 97;
public static R_PPC64 R_PPC64_TPREL16_HIGHERA => 98;
public static R_PPC64 R_PPC64_TPREL16_HIGHEST => 99;
public static R_PPC64 R_PPC64_TPREL16_HIGHESTA => 100;
public static R_PPC64 R_PPC64_DTPREL16_DS => 101;
public static R_PPC64 R_PPC64_DTPREL16_LO_DS => 102;
public static R_PPC64 R_PPC64_DTPREL16_HIGHER => 103;
public static R_PPC64 R_PPC64_DTPREL16_HIGHERA => 104;
public static R_PPC64 R_PPC64_DTPREL16_HIGHEST => 105;
public static R_PPC64 R_PPC64_DTPREL16_HIGHESTA => 106;
public static R_PPC64 R_PPC64_TLSGD => 107;
public static R_PPC64 R_PPC64_TLSLD => 108;
public static R_PPC64 R_PPC64_TOCSAVE => 109;
public static R_PPC64 R_PPC64_ADDR16_HIGH => 110;
public static R_PPC64 R_PPC64_ADDR16_HIGHA => 111;
public static R_PPC64 R_PPC64_TPREL16_HIGH => 112;
public static R_PPC64 R_PPC64_TPREL16_HIGHA => 113;
public static R_PPC64 R_PPC64_DTPREL16_HIGH => 114;
public static R_PPC64 R_PPC64_DTPREL16_HIGHA => 115;
public static R_PPC64 R_PPC64_REL24_NOTOC => 116;
public static R_PPC64 R_PPC64_ADDR64_LOCAL => 117;
public static R_PPC64 R_PPC64_ENTRY => 118;
public static R_PPC64 R_PPC64_PLTSEQ => 119;
public static R_PPC64 R_PPC64_PLTCALL => 120;
public static R_PPC64 R_PPC64_PLTSEQ_NOTOC => 121;
public static R_PPC64 R_PPC64_PLTCALL_NOTOC => 122;
public static R_PPC64 R_PPC64_PCREL_OPT => 123;
public static R_PPC64 R_PPC64_REL24_P9NOTOC => 124;
public static R_PPC64 R_PPC64_D34 => 128;
public static R_PPC64 R_PPC64_D34_LO => 129;
public static R_PPC64 R_PPC64_D34_HI30 => 130;
public static R_PPC64 R_PPC64_D34_HA30 => 131;
public static R_PPC64 R_PPC64_PCREL34 => 132;
public static R_PPC64 R_PPC64_GOT_PCREL34 => 133;
public static R_PPC64 R_PPC64_PLT_PCREL34 => 134;
public static R_PPC64 R_PPC64_PLT_PCREL34_NOTOC => 135;
public static R_PPC64 R_PPC64_ADDR16_HIGHER34 => 136;
public static R_PPC64 R_PPC64_ADDR16_HIGHERA34 => 137;
public static R_PPC64 R_PPC64_ADDR16_HIGHEST34 => 138;
public static R_PPC64 R_PPC64_ADDR16_HIGHESTA34 => 139;
public static R_PPC64 R_PPC64_REL16_HIGHER34 => 140;
public static R_PPC64 R_PPC64_REL16_HIGHERA34 => 141;
public static R_PPC64 R_PPC64_REL16_HIGHEST34 => 142;
public static R_PPC64 R_PPC64_REL16_HIGHESTA34 => 143;
public static R_PPC64 R_PPC64_D28 => 144;
public static R_PPC64 R_PPC64_PCREL28 => 145;
public static R_PPC64 R_PPC64_TPREL34 => 146;
public static R_PPC64 R_PPC64_DTPREL34 => 147;
public static R_PPC64 R_PPC64_GOT_TLSGD_PCREL34 => 148;
public static R_PPC64 R_PPC64_GOT_TLSLD_PCREL34 => 149;
public static R_PPC64 R_PPC64_GOT_TPREL_PCREL34 => 150;
public static R_PPC64 R_PPC64_GOT_DTPREL_PCREL34 => 151;
public static R_PPC64 R_PPC64_REL16_HIGH => 240;
public static R_PPC64 R_PPC64_REL16_HIGHA => 241;
public static R_PPC64 R_PPC64_REL16_HIGHER => 242;
public static R_PPC64 R_PPC64_REL16_HIGHERA => 243;
public static R_PPC64 R_PPC64_REL16_HIGHEST => 244;
public static R_PPC64 R_PPC64_REL16_HIGHESTA => 245;
public static R_PPC64 R_PPC64_REL16DX_HA => 246;         // R_POWERPC_REL16DX_HA
public static R_PPC64 R_PPC64_JMP_IREL => 247;
public static R_PPC64 R_PPC64_IRELATIVE => 248;          // R_POWERPC_IRELATIVE
public static R_PPC64 R_PPC64_REL16 => 249;              // R_POWERPC_REL16
public static R_PPC64 R_PPC64_REL16_LO => 250;           // R_POWERPC_REL16_LO
public static R_PPC64 R_PPC64_REL16_HI => 251;           // R_POWERPC_REL16_HI
public static R_PPC64 R_PPC64_REL16_HA => 252;           // R_POWERPC_REL16_HA
public static R_PPC64 R_PPC64_GNU_VTINHERIT => 253;
public static R_PPC64 R_PPC64_GNU_VTENTRY => 254;

internal static slice<intName> rppc64Strings = new intName[]{
    new(0, "R_PPC64_NONE"u8),
    new(1, "R_PPC64_ADDR32"u8),
    new(2, "R_PPC64_ADDR24"u8),
    new(3, "R_PPC64_ADDR16"u8),
    new(4, "R_PPC64_ADDR16_LO"u8),
    new(5, "R_PPC64_ADDR16_HI"u8),
    new(6, "R_PPC64_ADDR16_HA"u8),
    new(7, "R_PPC64_ADDR14"u8),
    new(8, "R_PPC64_ADDR14_BRTAKEN"u8),
    new(9, "R_PPC64_ADDR14_BRNTAKEN"u8),
    new(10, "R_PPC64_REL24"u8),
    new(11, "R_PPC64_REL14"u8),
    new(12, "R_PPC64_REL14_BRTAKEN"u8),
    new(13, "R_PPC64_REL14_BRNTAKEN"u8),
    new(14, "R_PPC64_GOT16"u8),
    new(15, "R_PPC64_GOT16_LO"u8),
    new(16, "R_PPC64_GOT16_HI"u8),
    new(17, "R_PPC64_GOT16_HA"u8),
    new(19, "R_PPC64_COPY"u8),
    new(20, "R_PPC64_GLOB_DAT"u8),
    new(21, "R_PPC64_JMP_SLOT"u8),
    new(22, "R_PPC64_RELATIVE"u8),
    new(24, "R_PPC64_UADDR32"u8),
    new(25, "R_PPC64_UADDR16"u8),
    new(26, "R_PPC64_REL32"u8),
    new(27, "R_PPC64_PLT32"u8),
    new(28, "R_PPC64_PLTREL32"u8),
    new(29, "R_PPC64_PLT16_LO"u8),
    new(30, "R_PPC64_PLT16_HI"u8),
    new(31, "R_PPC64_PLT16_HA"u8),
    new(33, "R_PPC64_SECTOFF"u8),
    new(34, "R_PPC64_SECTOFF_LO"u8),
    new(35, "R_PPC64_SECTOFF_HI"u8),
    new(36, "R_PPC64_SECTOFF_HA"u8),
    new(37, "R_PPC64_REL30"u8),
    new(38, "R_PPC64_ADDR64"u8),
    new(39, "R_PPC64_ADDR16_HIGHER"u8),
    new(40, "R_PPC64_ADDR16_HIGHERA"u8),
    new(41, "R_PPC64_ADDR16_HIGHEST"u8),
    new(42, "R_PPC64_ADDR16_HIGHESTA"u8),
    new(43, "R_PPC64_UADDR64"u8),
    new(44, "R_PPC64_REL64"u8),
    new(45, "R_PPC64_PLT64"u8),
    new(46, "R_PPC64_PLTREL64"u8),
    new(47, "R_PPC64_TOC16"u8),
    new(48, "R_PPC64_TOC16_LO"u8),
    new(49, "R_PPC64_TOC16_HI"u8),
    new(50, "R_PPC64_TOC16_HA"u8),
    new(51, "R_PPC64_TOC"u8),
    new(52, "R_PPC64_PLTGOT16"u8),
    new(53, "R_PPC64_PLTGOT16_LO"u8),
    new(54, "R_PPC64_PLTGOT16_HI"u8),
    new(55, "R_PPC64_PLTGOT16_HA"u8),
    new(56, "R_PPC64_ADDR16_DS"u8),
    new(57, "R_PPC64_ADDR16_LO_DS"u8),
    new(58, "R_PPC64_GOT16_DS"u8),
    new(59, "R_PPC64_GOT16_LO_DS"u8),
    new(60, "R_PPC64_PLT16_LO_DS"u8),
    new(61, "R_PPC64_SECTOFF_DS"u8),
    new(62, "R_PPC64_SECTOFF_LO_DS"u8),
    new(63, "R_PPC64_TOC16_DS"u8),
    new(64, "R_PPC64_TOC16_LO_DS"u8),
    new(65, "R_PPC64_PLTGOT16_DS"u8),
    new(66, "R_PPC64_PLTGOT_LO_DS"u8),
    new(67, "R_PPC64_TLS"u8),
    new(68, "R_PPC64_DTPMOD64"u8),
    new(69, "R_PPC64_TPREL16"u8),
    new(70, "R_PPC64_TPREL16_LO"u8),
    new(71, "R_PPC64_TPREL16_HI"u8),
    new(72, "R_PPC64_TPREL16_HA"u8),
    new(73, "R_PPC64_TPREL64"u8),
    new(74, "R_PPC64_DTPREL16"u8),
    new(75, "R_PPC64_DTPREL16_LO"u8),
    new(76, "R_PPC64_DTPREL16_HI"u8),
    new(77, "R_PPC64_DTPREL16_HA"u8),
    new(78, "R_PPC64_DTPREL64"u8),
    new(79, "R_PPC64_GOT_TLSGD16"u8),
    new(80, "R_PPC64_GOT_TLSGD16_LO"u8),
    new(81, "R_PPC64_GOT_TLSGD16_HI"u8),
    new(82, "R_PPC64_GOT_TLSGD16_HA"u8),
    new(83, "R_PPC64_GOT_TLSLD16"u8),
    new(84, "R_PPC64_GOT_TLSLD16_LO"u8),
    new(85, "R_PPC64_GOT_TLSLD16_HI"u8),
    new(86, "R_PPC64_GOT_TLSLD16_HA"u8),
    new(87, "R_PPC64_GOT_TPREL16_DS"u8),
    new(88, "R_PPC64_GOT_TPREL16_LO_DS"u8),
    new(89, "R_PPC64_GOT_TPREL16_HI"u8),
    new(90, "R_PPC64_GOT_TPREL16_HA"u8),
    new(91, "R_PPC64_GOT_DTPREL16_DS"u8),
    new(92, "R_PPC64_GOT_DTPREL16_LO_DS"u8),
    new(93, "R_PPC64_GOT_DTPREL16_HI"u8),
    new(94, "R_PPC64_GOT_DTPREL16_HA"u8),
    new(95, "R_PPC64_TPREL16_DS"u8),
    new(96, "R_PPC64_TPREL16_LO_DS"u8),
    new(97, "R_PPC64_TPREL16_HIGHER"u8),
    new(98, "R_PPC64_TPREL16_HIGHERA"u8),
    new(99, "R_PPC64_TPREL16_HIGHEST"u8),
    new(100, "R_PPC64_TPREL16_HIGHESTA"u8),
    new(101, "R_PPC64_DTPREL16_DS"u8),
    new(102, "R_PPC64_DTPREL16_LO_DS"u8),
    new(103, "R_PPC64_DTPREL16_HIGHER"u8),
    new(104, "R_PPC64_DTPREL16_HIGHERA"u8),
    new(105, "R_PPC64_DTPREL16_HIGHEST"u8),
    new(106, "R_PPC64_DTPREL16_HIGHESTA"u8),
    new(107, "R_PPC64_TLSGD"u8),
    new(108, "R_PPC64_TLSLD"u8),
    new(109, "R_PPC64_TOCSAVE"u8),
    new(110, "R_PPC64_ADDR16_HIGH"u8),
    new(111, "R_PPC64_ADDR16_HIGHA"u8),
    new(112, "R_PPC64_TPREL16_HIGH"u8),
    new(113, "R_PPC64_TPREL16_HIGHA"u8),
    new(114, "R_PPC64_DTPREL16_HIGH"u8),
    new(115, "R_PPC64_DTPREL16_HIGHA"u8),
    new(116, "R_PPC64_REL24_NOTOC"u8),
    new(117, "R_PPC64_ADDR64_LOCAL"u8),
    new(118, "R_PPC64_ENTRY"u8),
    new(119, "R_PPC64_PLTSEQ"u8),
    new(120, "R_PPC64_PLTCALL"u8),
    new(121, "R_PPC64_PLTSEQ_NOTOC"u8),
    new(122, "R_PPC64_PLTCALL_NOTOC"u8),
    new(123, "R_PPC64_PCREL_OPT"u8),
    new(124, "R_PPC64_REL24_P9NOTOC"u8),
    new(128, "R_PPC64_D34"u8),
    new(129, "R_PPC64_D34_LO"u8),
    new(130, "R_PPC64_D34_HI30"u8),
    new(131, "R_PPC64_D34_HA30"u8),
    new(132, "R_PPC64_PCREL34"u8),
    new(133, "R_PPC64_GOT_PCREL34"u8),
    new(134, "R_PPC64_PLT_PCREL34"u8),
    new(135, "R_PPC64_PLT_PCREL34_NOTOC"u8),
    new(136, "R_PPC64_ADDR16_HIGHER34"u8),
    new(137, "R_PPC64_ADDR16_HIGHERA34"u8),
    new(138, "R_PPC64_ADDR16_HIGHEST34"u8),
    new(139, "R_PPC64_ADDR16_HIGHESTA34"u8),
    new(140, "R_PPC64_REL16_HIGHER34"u8),
    new(141, "R_PPC64_REL16_HIGHERA34"u8),
    new(142, "R_PPC64_REL16_HIGHEST34"u8),
    new(143, "R_PPC64_REL16_HIGHESTA34"u8),
    new(144, "R_PPC64_D28"u8),
    new(145, "R_PPC64_PCREL28"u8),
    new(146, "R_PPC64_TPREL34"u8),
    new(147, "R_PPC64_DTPREL34"u8),
    new(148, "R_PPC64_GOT_TLSGD_PCREL34"u8),
    new(149, "R_PPC64_GOT_TLSLD_PCREL34"u8),
    new(150, "R_PPC64_GOT_TPREL_PCREL34"u8),
    new(151, "R_PPC64_GOT_DTPREL_PCREL34"u8),
    new(240, "R_PPC64_REL16_HIGH"u8),
    new(241, "R_PPC64_REL16_HIGHA"u8),
    new(242, "R_PPC64_REL16_HIGHER"u8),
    new(243, "R_PPC64_REL16_HIGHERA"u8),
    new(244, "R_PPC64_REL16_HIGHEST"u8),
    new(245, "R_PPC64_REL16_HIGHESTA"u8),
    new(246, "R_PPC64_REL16DX_HA"u8),
    new(247, "R_PPC64_JMP_IREL"u8),
    new(248, "R_PPC64_IRELATIVE"u8),
    new(249, "R_PPC64_REL16"u8),
    new(250, "R_PPC64_REL16_LO"u8),
    new(251, "R_PPC64_REL16_HI"u8),
    new(252, "R_PPC64_REL16_HA"u8),
    new(253, "R_PPC64_GNU_VTINHERIT"u8),
    new(254, "R_PPC64_GNU_VTENTRY"u8)
}.slice();

public static @string String(this R_PPC64 i) {
    return stringName((uint32)(nint)i, rppc64Strings, false);
}

public static @string GoString(this R_PPC64 i) {
    return stringName((uint32)(nint)i, rppc64Strings, true);
}

partial struct R_RISCV /*num:nint*/;

public static R_RISCV R_RISCV_NONE => 0;         /* No relocation. */
public static R_RISCV R_RISCV_32 => 1;           /* Add 32 bit zero extended symbol value */
public static R_RISCV R_RISCV_64 => 2;           /* Add 64 bit symbol value. */
public static R_RISCV R_RISCV_RELATIVE => 3;     /* Add load address of shared object. */
public static R_RISCV R_RISCV_COPY => 4;         /* Copy data from shared object. */
public static R_RISCV R_RISCV_JUMP_SLOT => 5;    /* Set GOT entry to code address. */
public static R_RISCV R_RISCV_TLS_DTPMOD32 => 6; /* 32 bit ID of module containing symbol */
public static R_RISCV R_RISCV_TLS_DTPMOD64 => 7; /* ID of module containing symbol */
public static R_RISCV R_RISCV_TLS_DTPREL32 => 8; /* 32 bit relative offset in TLS block */
public static R_RISCV R_RISCV_TLS_DTPREL64 => 9; /* Relative offset in TLS block */
public static R_RISCV R_RISCV_TLS_TPREL32 => 10;  /* 32 bit relative offset in static TLS block */
public static R_RISCV R_RISCV_TLS_TPREL64 => 11;  /* Relative offset in static TLS block */
public static R_RISCV R_RISCV_BRANCH => 16;       /* PC-relative branch */
public static R_RISCV R_RISCV_JAL => 17;          /* PC-relative jump */
public static R_RISCV R_RISCV_CALL => 18;         /* PC-relative call */
public static R_RISCV R_RISCV_CALL_PLT => 19;     /* PC-relative call (PLT) */
public static R_RISCV R_RISCV_GOT_HI20 => 20;     /* PC-relative GOT reference */
public static R_RISCV R_RISCV_TLS_GOT_HI20 => 21; /* PC-relative TLS IE GOT offset */
public static R_RISCV R_RISCV_TLS_GD_HI20 => 22;  /* PC-relative TLS GD reference */
public static R_RISCV R_RISCV_PCREL_HI20 => 23;   /* PC-relative reference */
public static R_RISCV R_RISCV_PCREL_LO12_I => 24; /* PC-relative reference */
public static R_RISCV R_RISCV_PCREL_LO12_S => 25; /* PC-relative reference */
public static R_RISCV R_RISCV_HI20 => 26;         /* Absolute address */
public static R_RISCV R_RISCV_LO12_I => 27;       /* Absolute address */
public static R_RISCV R_RISCV_LO12_S => 28;       /* Absolute address */
public static R_RISCV R_RISCV_TPREL_HI20 => 29;   /* TLS LE thread offset */
public static R_RISCV R_RISCV_TPREL_LO12_I => 30; /* TLS LE thread offset */
public static R_RISCV R_RISCV_TPREL_LO12_S => 31; /* TLS LE thread offset */
public static R_RISCV R_RISCV_TPREL_ADD => 32;    /* TLS LE thread usage */
public static R_RISCV R_RISCV_ADD8 => 33;         /* 8-bit label addition */
public static R_RISCV R_RISCV_ADD16 => 34;        /* 16-bit label addition */
public static R_RISCV R_RISCV_ADD32 => 35;        /* 32-bit label addition */
public static R_RISCV R_RISCV_ADD64 => 36;        /* 64-bit label addition */
public static R_RISCV R_RISCV_SUB8 => 37;         /* 8-bit label subtraction */
public static R_RISCV R_RISCV_SUB16 => 38;        /* 16-bit label subtraction */
public static R_RISCV R_RISCV_SUB32 => 39;        /* 32-bit label subtraction */
public static R_RISCV R_RISCV_SUB64 => 40;        /* 64-bit label subtraction */
public static R_RISCV R_RISCV_GNU_VTINHERIT => 41; /* GNU C++ vtable hierarchy */
public static R_RISCV R_RISCV_GNU_VTENTRY => 42;  /* GNU C++ vtable member usage */
public static R_RISCV R_RISCV_ALIGN => 43;        /* Alignment statement */
public static R_RISCV R_RISCV_RVC_BRANCH => 44;   /* PC-relative branch offset */
public static R_RISCV R_RISCV_RVC_JUMP => 45;     /* PC-relative jump offset */
public static R_RISCV R_RISCV_RVC_LUI => 46;      /* Absolute address */
public static R_RISCV R_RISCV_GPREL_I => 47;      /* GP-relative reference */
public static R_RISCV R_RISCV_GPREL_S => 48;      /* GP-relative reference */
public static R_RISCV R_RISCV_TPREL_I => 49;      /* TP-relative TLS LE load */
public static R_RISCV R_RISCV_TPREL_S => 50;      /* TP-relative TLS LE store */
public static R_RISCV R_RISCV_RELAX => 51;        /* Instruction pair can be relaxed */
public static R_RISCV R_RISCV_SUB6 => 52;         /* Local label subtraction */
public static R_RISCV R_RISCV_SET6 => 53;         /* Local label subtraction */
public static R_RISCV R_RISCV_SET8 => 54;         /* Local label subtraction */
public static R_RISCV R_RISCV_SET16 => 55;        /* Local label subtraction */
public static R_RISCV R_RISCV_SET32 => 56;        /* Local label subtraction */
public static R_RISCV R_RISCV_32_PCREL => 57;     /* 32-bit PC relative */

internal static slice<intName> rriscvStrings = new intName[]{
    new(0, "R_RISCV_NONE"u8),
    new(1, "R_RISCV_32"u8),
    new(2, "R_RISCV_64"u8),
    new(3, "R_RISCV_RELATIVE"u8),
    new(4, "R_RISCV_COPY"u8),
    new(5, "R_RISCV_JUMP_SLOT"u8),
    new(6, "R_RISCV_TLS_DTPMOD32"u8),
    new(7, "R_RISCV_TLS_DTPMOD64"u8),
    new(8, "R_RISCV_TLS_DTPREL32"u8),
    new(9, "R_RISCV_TLS_DTPREL64"u8),
    new(10, "R_RISCV_TLS_TPREL32"u8),
    new(11, "R_RISCV_TLS_TPREL64"u8),
    new(16, "R_RISCV_BRANCH"u8),
    new(17, "R_RISCV_JAL"u8),
    new(18, "R_RISCV_CALL"u8),
    new(19, "R_RISCV_CALL_PLT"u8),
    new(20, "R_RISCV_GOT_HI20"u8),
    new(21, "R_RISCV_TLS_GOT_HI20"u8),
    new(22, "R_RISCV_TLS_GD_HI20"u8),
    new(23, "R_RISCV_PCREL_HI20"u8),
    new(24, "R_RISCV_PCREL_LO12_I"u8),
    new(25, "R_RISCV_PCREL_LO12_S"u8),
    new(26, "R_RISCV_HI20"u8),
    new(27, "R_RISCV_LO12_I"u8),
    new(28, "R_RISCV_LO12_S"u8),
    new(29, "R_RISCV_TPREL_HI20"u8),
    new(30, "R_RISCV_TPREL_LO12_I"u8),
    new(31, "R_RISCV_TPREL_LO12_S"u8),
    new(32, "R_RISCV_TPREL_ADD"u8),
    new(33, "R_RISCV_ADD8"u8),
    new(34, "R_RISCV_ADD16"u8),
    new(35, "R_RISCV_ADD32"u8),
    new(36, "R_RISCV_ADD64"u8),
    new(37, "R_RISCV_SUB8"u8),
    new(38, "R_RISCV_SUB16"u8),
    new(39, "R_RISCV_SUB32"u8),
    new(40, "R_RISCV_SUB64"u8),
    new(41, "R_RISCV_GNU_VTINHERIT"u8),
    new(42, "R_RISCV_GNU_VTENTRY"u8),
    new(43, "R_RISCV_ALIGN"u8),
    new(44, "R_RISCV_RVC_BRANCH"u8),
    new(45, "R_RISCV_RVC_JUMP"u8),
    new(46, "R_RISCV_RVC_LUI"u8),
    new(47, "R_RISCV_GPREL_I"u8),
    new(48, "R_RISCV_GPREL_S"u8),
    new(49, "R_RISCV_TPREL_I"u8),
    new(50, "R_RISCV_TPREL_S"u8),
    new(51, "R_RISCV_RELAX"u8),
    new(52, "R_RISCV_SUB6"u8),
    new(53, "R_RISCV_SET6"u8),
    new(54, "R_RISCV_SET8"u8),
    new(55, "R_RISCV_SET16"u8),
    new(56, "R_RISCV_SET32"u8),
    new(57, "R_RISCV_32_PCREL"u8)
}.slice();

public static @string String(this R_RISCV i) {
    return stringName((uint32)(nint)i, rriscvStrings, false);
}

public static @string GoString(this R_RISCV i) {
    return stringName((uint32)(nint)i, rriscvStrings, true);
}

partial struct R_390 /*num:nint*/;

public static R_390 R_390_NONE => 0;
public static R_390 R_390_8 => 1;
public static R_390 R_390_12 => 2;
public static R_390 R_390_16 => 3;
public static R_390 R_390_32 => 4;
public static R_390 R_390_PC32 => 5;
public static R_390 R_390_GOT12 => 6;
public static R_390 R_390_GOT32 => 7;
public static R_390 R_390_PLT32 => 8;
public static R_390 R_390_COPY => 9;
public static R_390 R_390_GLOB_DAT => 10;
public static R_390 R_390_JMP_SLOT => 11;
public static R_390 R_390_RELATIVE => 12;
public static R_390 R_390_GOTOFF => 13;
public static R_390 R_390_GOTPC => 14;
public static R_390 R_390_GOT16 => 15;
public static R_390 R_390_PC16 => 16;
public static R_390 R_390_PC16DBL => 17;
public static R_390 R_390_PLT16DBL => 18;
public static R_390 R_390_PC32DBL => 19;
public static R_390 R_390_PLT32DBL => 20;
public static R_390 R_390_GOTPCDBL => 21;
public static R_390 R_390_64 => 22;
public static R_390 R_390_PC64 => 23;
public static R_390 R_390_GOT64 => 24;
public static R_390 R_390_PLT64 => 25;
public static R_390 R_390_GOTENT => 26;
public static R_390 R_390_GOTOFF16 => 27;
public static R_390 R_390_GOTOFF64 => 28;
public static R_390 R_390_GOTPLT12 => 29;
public static R_390 R_390_GOTPLT16 => 30;
public static R_390 R_390_GOTPLT32 => 31;
public static R_390 R_390_GOTPLT64 => 32;
public static R_390 R_390_GOTPLTENT => 33;
public static R_390 R_390_GOTPLTOFF16 => 34;
public static R_390 R_390_GOTPLTOFF32 => 35;
public static R_390 R_390_GOTPLTOFF64 => 36;
public static R_390 R_390_TLS_LOAD => 37;
public static R_390 R_390_TLS_GDCALL => 38;
public static R_390 R_390_TLS_LDCALL => 39;
public static R_390 R_390_TLS_GD32 => 40;
public static R_390 R_390_TLS_GD64 => 41;
public static R_390 R_390_TLS_GOTIE12 => 42;
public static R_390 R_390_TLS_GOTIE32 => 43;
public static R_390 R_390_TLS_GOTIE64 => 44;
public static R_390 R_390_TLS_LDM32 => 45;
public static R_390 R_390_TLS_LDM64 => 46;
public static R_390 R_390_TLS_IE32 => 47;
public static R_390 R_390_TLS_IE64 => 48;
public static R_390 R_390_TLS_IEENT => 49;
public static R_390 R_390_TLS_LE32 => 50;
public static R_390 R_390_TLS_LE64 => 51;
public static R_390 R_390_TLS_LDO32 => 52;
public static R_390 R_390_TLS_LDO64 => 53;
public static R_390 R_390_TLS_DTPMOD => 54;
public static R_390 R_390_TLS_DTPOFF => 55;
public static R_390 R_390_TLS_TPOFF => 56;
public static R_390 R_390_20 => 57;
public static R_390 R_390_GOT20 => 58;
public static R_390 R_390_GOTPLT20 => 59;
public static R_390 R_390_TLS_GOTIE20 => 60;

internal static slice<intName> r390Strings = new intName[]{
    new(0, "R_390_NONE"u8),
    new(1, "R_390_8"u8),
    new(2, "R_390_12"u8),
    new(3, "R_390_16"u8),
    new(4, "R_390_32"u8),
    new(5, "R_390_PC32"u8),
    new(6, "R_390_GOT12"u8),
    new(7, "R_390_GOT32"u8),
    new(8, "R_390_PLT32"u8),
    new(9, "R_390_COPY"u8),
    new(10, "R_390_GLOB_DAT"u8),
    new(11, "R_390_JMP_SLOT"u8),
    new(12, "R_390_RELATIVE"u8),
    new(13, "R_390_GOTOFF"u8),
    new(14, "R_390_GOTPC"u8),
    new(15, "R_390_GOT16"u8),
    new(16, "R_390_PC16"u8),
    new(17, "R_390_PC16DBL"u8),
    new(18, "R_390_PLT16DBL"u8),
    new(19, "R_390_PC32DBL"u8),
    new(20, "R_390_PLT32DBL"u8),
    new(21, "R_390_GOTPCDBL"u8),
    new(22, "R_390_64"u8),
    new(23, "R_390_PC64"u8),
    new(24, "R_390_GOT64"u8),
    new(25, "R_390_PLT64"u8),
    new(26, "R_390_GOTENT"u8),
    new(27, "R_390_GOTOFF16"u8),
    new(28, "R_390_GOTOFF64"u8),
    new(29, "R_390_GOTPLT12"u8),
    new(30, "R_390_GOTPLT16"u8),
    new(31, "R_390_GOTPLT32"u8),
    new(32, "R_390_GOTPLT64"u8),
    new(33, "R_390_GOTPLTENT"u8),
    new(34, "R_390_GOTPLTOFF16"u8),
    new(35, "R_390_GOTPLTOFF32"u8),
    new(36, "R_390_GOTPLTOFF64"u8),
    new(37, "R_390_TLS_LOAD"u8),
    new(38, "R_390_TLS_GDCALL"u8),
    new(39, "R_390_TLS_LDCALL"u8),
    new(40, "R_390_TLS_GD32"u8),
    new(41, "R_390_TLS_GD64"u8),
    new(42, "R_390_TLS_GOTIE12"u8),
    new(43, "R_390_TLS_GOTIE32"u8),
    new(44, "R_390_TLS_GOTIE64"u8),
    new(45, "R_390_TLS_LDM32"u8),
    new(46, "R_390_TLS_LDM64"u8),
    new(47, "R_390_TLS_IE32"u8),
    new(48, "R_390_TLS_IE64"u8),
    new(49, "R_390_TLS_IEENT"u8),
    new(50, "R_390_TLS_LE32"u8),
    new(51, "R_390_TLS_LE64"u8),
    new(52, "R_390_TLS_LDO32"u8),
    new(53, "R_390_TLS_LDO64"u8),
    new(54, "R_390_TLS_DTPMOD"u8),
    new(55, "R_390_TLS_DTPOFF"u8),
    new(56, "R_390_TLS_TPOFF"u8),
    new(57, "R_390_20"u8),
    new(58, "R_390_GOT20"u8),
    new(59, "R_390_GOTPLT20"u8),
    new(60, "R_390_TLS_GOTIE20"u8)
}.slice();

public static @string String(this R_390 i) {
    return stringName((uint32)(nint)i, r390Strings, false);
}

public static @string GoString(this R_390 i) {
    return stringName((uint32)(nint)i, r390Strings, true);
}

partial struct R_SPARC /*num:nint*/;

public static R_SPARC R_SPARC_NONE => 0;
public static R_SPARC R_SPARC_8 => 1;
public static R_SPARC R_SPARC_16 => 2;
public static R_SPARC R_SPARC_32 => 3;
public static R_SPARC R_SPARC_DISP8 => 4;
public static R_SPARC R_SPARC_DISP16 => 5;
public static R_SPARC R_SPARC_DISP32 => 6;
public static R_SPARC R_SPARC_WDISP30 => 7;
public static R_SPARC R_SPARC_WDISP22 => 8;
public static R_SPARC R_SPARC_HI22 => 9;
public static R_SPARC R_SPARC_22 => 10;
public static R_SPARC R_SPARC_13 => 11;
public static R_SPARC R_SPARC_LO10 => 12;
public static R_SPARC R_SPARC_GOT10 => 13;
public static R_SPARC R_SPARC_GOT13 => 14;
public static R_SPARC R_SPARC_GOT22 => 15;
public static R_SPARC R_SPARC_PC10 => 16;
public static R_SPARC R_SPARC_PC22 => 17;
public static R_SPARC R_SPARC_WPLT30 => 18;
public static R_SPARC R_SPARC_COPY => 19;
public static R_SPARC R_SPARC_GLOB_DAT => 20;
public static R_SPARC R_SPARC_JMP_SLOT => 21;
public static R_SPARC R_SPARC_RELATIVE => 22;
public static R_SPARC R_SPARC_UA32 => 23;
public static R_SPARC R_SPARC_PLT32 => 24;
public static R_SPARC R_SPARC_HIPLT22 => 25;
public static R_SPARC R_SPARC_LOPLT10 => 26;
public static R_SPARC R_SPARC_PCPLT32 => 27;
public static R_SPARC R_SPARC_PCPLT22 => 28;
public static R_SPARC R_SPARC_PCPLT10 => 29;
public static R_SPARC R_SPARC_10 => 30;
public static R_SPARC R_SPARC_11 => 31;
public static R_SPARC R_SPARC_64 => 32;
public static R_SPARC R_SPARC_OLO10 => 33;
public static R_SPARC R_SPARC_HH22 => 34;
public static R_SPARC R_SPARC_HM10 => 35;
public static R_SPARC R_SPARC_LM22 => 36;
public static R_SPARC R_SPARC_PC_HH22 => 37;
public static R_SPARC R_SPARC_PC_HM10 => 38;
public static R_SPARC R_SPARC_PC_LM22 => 39;
public static R_SPARC R_SPARC_WDISP16 => 40;
public static R_SPARC R_SPARC_WDISP19 => 41;
public static R_SPARC R_SPARC_GLOB_JMP => 42;
public static R_SPARC R_SPARC_7 => 43;
public static R_SPARC R_SPARC_5 => 44;
public static R_SPARC R_SPARC_6 => 45;
public static R_SPARC R_SPARC_DISP64 => 46;
public static R_SPARC R_SPARC_PLT64 => 47;
public static R_SPARC R_SPARC_HIX22 => 48;
public static R_SPARC R_SPARC_LOX10 => 49;
public static R_SPARC R_SPARC_H44 => 50;
public static R_SPARC R_SPARC_M44 => 51;
public static R_SPARC R_SPARC_L44 => 52;
public static R_SPARC R_SPARC_REGISTER => 53;
public static R_SPARC R_SPARC_UA64 => 54;
public static R_SPARC R_SPARC_UA16 => 55;

internal static slice<intName> rsparcStrings = new intName[]{
    new(0, "R_SPARC_NONE"u8),
    new(1, "R_SPARC_8"u8),
    new(2, "R_SPARC_16"u8),
    new(3, "R_SPARC_32"u8),
    new(4, "R_SPARC_DISP8"u8),
    new(5, "R_SPARC_DISP16"u8),
    new(6, "R_SPARC_DISP32"u8),
    new(7, "R_SPARC_WDISP30"u8),
    new(8, "R_SPARC_WDISP22"u8),
    new(9, "R_SPARC_HI22"u8),
    new(10, "R_SPARC_22"u8),
    new(11, "R_SPARC_13"u8),
    new(12, "R_SPARC_LO10"u8),
    new(13, "R_SPARC_GOT10"u8),
    new(14, "R_SPARC_GOT13"u8),
    new(15, "R_SPARC_GOT22"u8),
    new(16, "R_SPARC_PC10"u8),
    new(17, "R_SPARC_PC22"u8),
    new(18, "R_SPARC_WPLT30"u8),
    new(19, "R_SPARC_COPY"u8),
    new(20, "R_SPARC_GLOB_DAT"u8),
    new(21, "R_SPARC_JMP_SLOT"u8),
    new(22, "R_SPARC_RELATIVE"u8),
    new(23, "R_SPARC_UA32"u8),
    new(24, "R_SPARC_PLT32"u8),
    new(25, "R_SPARC_HIPLT22"u8),
    new(26, "R_SPARC_LOPLT10"u8),
    new(27, "R_SPARC_PCPLT32"u8),
    new(28, "R_SPARC_PCPLT22"u8),
    new(29, "R_SPARC_PCPLT10"u8),
    new(30, "R_SPARC_10"u8),
    new(31, "R_SPARC_11"u8),
    new(32, "R_SPARC_64"u8),
    new(33, "R_SPARC_OLO10"u8),
    new(34, "R_SPARC_HH22"u8),
    new(35, "R_SPARC_HM10"u8),
    new(36, "R_SPARC_LM22"u8),
    new(37, "R_SPARC_PC_HH22"u8),
    new(38, "R_SPARC_PC_HM10"u8),
    new(39, "R_SPARC_PC_LM22"u8),
    new(40, "R_SPARC_WDISP16"u8),
    new(41, "R_SPARC_WDISP19"u8),
    new(42, "R_SPARC_GLOB_JMP"u8),
    new(43, "R_SPARC_7"u8),
    new(44, "R_SPARC_5"u8),
    new(45, "R_SPARC_6"u8),
    new(46, "R_SPARC_DISP64"u8),
    new(47, "R_SPARC_PLT64"u8),
    new(48, "R_SPARC_HIX22"u8),
    new(49, "R_SPARC_LOX10"u8),
    new(50, "R_SPARC_H44"u8),
    new(51, "R_SPARC_M44"u8),
    new(52, "R_SPARC_L44"u8),
    new(53, "R_SPARC_REGISTER"u8),
    new(54, "R_SPARC_UA64"u8),
    new(55, "R_SPARC_UA16"u8)
}.slice();

public static @string String(this R_SPARC i) {
    return stringName((uint32)(nint)i, rsparcStrings, false);
}

public static @string GoString(this R_SPARC i) {
    return stringName((uint32)(nint)i, rsparcStrings, true);
}

// Magic number for the elf trampoline, chosen wisely to be an immediate value.
public static UntypedInt ARM_MAGIC_TRAMP_NUMBER => 0x5c000003;

// ELF32 File header.
partial struct Header32 {
    public array<byte> Ident = new(EI_NIDENT); /* File identification. */
    public uint16 Type;          /* File type. */
    public uint16 Machine;          /* Machine architecture. */
    public uint32 Version;          /* ELF format version. */
    public uint32 Entry;          /* Entry point. */
    public uint32 Phoff;          /* Program header file offset. */
    public uint32 Shoff;          /* Section header file offset. */
    public uint32 Flags;          /* Architecture-specific flags. */
    public uint16 Ehsize;          /* Size of ELF header in bytes. */
    public uint16 Phentsize;          /* Size of program header entry. */
    public uint16 Phnum;          /* Number of program header entries. */
    public uint16 Shentsize;          /* Size of section header entry. */
    public uint16 Shnum;          /* Number of section header entries. */
    public uint16 Shstrndx;          /* Section name strings section. */
}

// ELF32 Section header.
partial struct Section32 {
    public uint32 Name; /* Section name (index into the section header string table). */
    public uint32 Type; /* Section type. */
    public uint32 Flags; /* Section flags. */
    public uint32 Addr; /* Address in memory image. */
    public uint32 Off; /* Offset in file. */
    public uint32 Size; /* Size in bytes. */
    public uint32 Link; /* Index of a related section. */
    public uint32 Info; /* Depends on section type. */
    public uint32 Addralign; /* Alignment in bytes. */
    public uint32 Entsize; /* Size of each entry in section. */
}

// ELF32 Program header.
partial struct Prog32 {
    public uint32 Type; /* Entry type. */
    public uint32 Off; /* File offset of contents. */
    public uint32 Vaddr; /* Virtual address in memory image. */
    public uint32 Paddr; /* Physical address (not used). */
    public uint32 Filesz; /* Size of contents in file. */
    public uint32 Memsz; /* Size of contents in memory. */
    public uint32 Flags; /* Access permission flags. */
    public uint32 Align; /* Alignment in memory and file. */
}

// ELF32 Dynamic structure. The ".dynamic" section contains an array of them.
partial struct Dyn32 {
    public int32 Tag;  /* Entry type. */
    public uint32 Val; /* Integer/Address value. */
}

// ELF32 Compression header.
partial struct Chdr32 {
    public uint32 Type;
    public uint32 Size;
    public uint32 Addralign;
}

/*
 * Relocation entries.
 */

// ELF32 Relocations that don't need an addend field.
partial struct Rel32 {
    public uint32 Off; /* Location to be relocated. */
    public uint32 Info; /* Relocation type and symbol index. */
}

// ELF32 Relocations that need an addend field.
partial struct Rela32 {
    public uint32 Off; /* Location to be relocated. */
    public uint32 Info; /* Relocation type and symbol index. */
    public int32 Addend;  /* Addend. */
}

public static uint32 R_SYM32(uint32 info) {
    return (info >> (int)(8));
}

public static uint32 R_TYPE32(uint32 info) {
    return (uint32)(info & 0xff);
}

public static uint32 R_INFO32(uint32 sym, uint32 typ) {
    return (uint32)((sym << (int)(8)) | typ);
}

// ELF32 Symbol.
partial struct Sym32 {
    public uint32 Name;
    public uint32 Value;
    public uint32 Size;
    public uint8 Info;
    public uint8 Other;
    public uint16 Shndx;
}

public static UntypedInt Sym32Size => 16;

public static SymBind ST_BIND(uint8 info) {
    return ((SymBind)(nint)((info >> (int)(4))));
}

public static SymType ST_TYPE(uint8 info) {
    return ((SymType)(nint)((uint8)(info & 0xF)));
}

public static uint8 ST_INFO(SymBind bind, SymType typ) {
    return (uint8)((uint8)((uint8)(nint)bind << (int)(4)) | (uint8)((uint8)(nint)typ & 0xf));
}

public static SymVis ST_VISIBILITY(uint8 other) {
    return ((SymVis)(nint)((uint8)(other & 3)));
}

/*
 * ELF64
 */

// ELF64 file header.
partial struct Header64 {
    public array<byte> Ident = new(EI_NIDENT); /* File identification. */
    public uint16 Type;          /* File type. */
    public uint16 Machine;          /* Machine architecture. */
    public uint32 Version;          /* ELF format version. */
    public uint64 Entry;          /* Entry point. */
    public uint64 Phoff;          /* Program header file offset. */
    public uint64 Shoff;          /* Section header file offset. */
    public uint32 Flags;          /* Architecture-specific flags. */
    public uint16 Ehsize;          /* Size of ELF header in bytes. */
    public uint16 Phentsize;          /* Size of program header entry. */
    public uint16 Phnum;          /* Number of program header entries. */
    public uint16 Shentsize;          /* Size of section header entry. */
    public uint16 Shnum;          /* Number of section header entries. */
    public uint16 Shstrndx;          /* Section name strings section. */
}

// ELF64 Section header.
partial struct Section64 {
    public uint32 Name; /* Section name (index into the section header string table). */
    public uint32 Type; /* Section type. */
    public uint64 Flags; /* Section flags. */
    public uint64 Addr; /* Address in memory image. */
    public uint64 Off; /* Offset in file. */
    public uint64 Size; /* Size in bytes. */
    public uint32 Link; /* Index of a related section. */
    public uint32 Info; /* Depends on section type. */
    public uint64 Addralign; /* Alignment in bytes. */
    public uint64 Entsize; /* Size of each entry in section. */
}

// ELF64 Program header.
partial struct Prog64 {
    public uint32 Type; /* Entry type. */
    public uint32 Flags; /* Access permission flags. */
    public uint64 Off; /* File offset of contents. */
    public uint64 Vaddr; /* Virtual address in memory image. */
    public uint64 Paddr; /* Physical address (not used). */
    public uint64 Filesz; /* Size of contents in file. */
    public uint64 Memsz; /* Size of contents in memory. */
    public uint64 Align; /* Alignment in memory and file. */
}

// ELF64 Dynamic structure. The ".dynamic" section contains an array of them.
partial struct Dyn64 {
    public int64 Tag;  /* Entry type. */
    public uint64 Val; /* Integer/address value */
}

// ELF64 Compression header.
partial struct Chdr64 {
    public uint32 Type;
    internal uint32 _; /* Reserved. */
    public uint64 Size;
    public uint64 Addralign;
}

/*
 * Relocation entries.
 */

/* ELF64 relocations that don't need an addend field. */
partial struct Rel64 {
    public uint64 Off; /* Location to be relocated. */
    public uint64 Info; /* Relocation type and symbol index. */
}

/* ELF64 relocations that need an addend field. */
partial struct Rela64 {
    public uint64 Off; /* Location to be relocated. */
    public uint64 Info; /* Relocation type and symbol index. */
    public int64 Addend;  /* Addend. */
}

public static uint32 R_SYM64(uint64 info) {
    return (uint32)((info >> (int)(32)));
}

public static uint32 R_TYPE64(uint64 info) {
    return (uint32)info;
}

public static uint64 R_INFO(uint32 sym, uint32 typ) {
    return (uint64)(((uint64)sym << (int)(32)) | (uint64)typ);
}

// ELF64 symbol table entries.
partial struct Sym64 {
    public uint32 Name; /* String table index of name. */
    public uint8 Info;  /* Type and binding information. */
    public uint8 Other;  /* Reserved (not used). */
    public uint16 Shndx; /* Section index of symbol. */
    public uint64 Value; /* Symbol value. */
    public uint64 Size; /* Size of associated object. */
}

public static UntypedInt Sym64Size => 24;

partial struct intName {
    internal uint32 i;
    internal @string s;
}

partial struct DynamicVersionFlag /*num:uint16*/;

public static DynamicVersionFlag VER_FLG_BASE => 0x1; /* Version definition of the file. */
public static DynamicVersionFlag VER_FLG_WEAK => 0x2; /* Weak version identifier. */
public static DynamicVersionFlag VER_FLG_INFO => 0x4; /* Reference exists for informational purposes. */

internal static @string stringName(uint32 i, slice<intName> names, bool goSyntax) {
    foreach (var (_, n) in names) {
        if (n.i == i) {
            if (goSyntax) {
                return "elf."u8 + n.s;
            }
            return n.s;
        }
    }
    // second pass - look for smaller to add with.
    // assume sorted already
    for (nint j = len(names) - 1; j >= 0; j--) {
        var n = names[j];
        if (n.i < i) {
            @string s = n.s;
            if (goSyntax) {
                s = "elf."u8 + s;
            }
            return s + "+"u8 + strconv.FormatUint((uint64)(i - n.i), 10);
        }
    }
    return strconv.FormatUint((uint64)i, 10);
}

internal static @string flagName(uint32 i, slice<intName> names, bool goSyntax) {
    @string s = ""u8;
    foreach (var (_, n) in names) {
        if ((uint32)(n.i & i) == n.i) {
            if (len(s) > 0) {
                s += "+"u8;
            }
            if (goSyntax) {
                s += "elf."u8;
            }
            s += n.s;
            i -= n.i;
        }
    }
    if (len(s) == 0) {
        return "0x"u8 + strconv.FormatUint((uint64)i, 16);
    }
    if (i != 0) {
        s += "+0x"u8 + strconv.FormatUint((uint64)i, 16);
    }
    return s;
}

} // end elf_package

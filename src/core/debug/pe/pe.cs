// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

partial class pe_package {

partial struct FileHeader {
    public uint16 Machine;
    public uint16 NumberOfSections;
    public uint32 TimeDateStamp;
    public uint32 PointerToSymbolTable;
    public uint32 NumberOfSymbols;
    public uint16 SizeOfOptionalHeader;
    public uint16 Characteristics;
}

partial struct DataDirectory {
    public uint32 VirtualAddress;
    public uint32 Size;
}

partial struct OptionalHeader32 {
    public uint16 Magic;
    public uint8 MajorLinkerVersion;
    public uint8 MinorLinkerVersion;
    public uint32 SizeOfCode;
    public uint32 SizeOfInitializedData;
    public uint32 SizeOfUninitializedData;
    public uint32 AddressOfEntryPoint;
    public uint32 BaseOfCode;
    public uint32 BaseOfData;
    public uint32 ImageBase;
    public uint32 SectionAlignment;
    public uint32 FileAlignment;
    public uint16 MajorOperatingSystemVersion;
    public uint16 MinorOperatingSystemVersion;
    public uint16 MajorImageVersion;
    public uint16 MinorImageVersion;
    public uint16 MajorSubsystemVersion;
    public uint16 MinorSubsystemVersion;
    public uint32 Win32VersionValue;
    public uint32 SizeOfImage;
    public uint32 SizeOfHeaders;
    public uint32 CheckSum;
    public uint16 Subsystem;
    public uint16 DllCharacteristics;
    public uint32 SizeOfStackReserve;
    public uint32 SizeOfStackCommit;
    public uint32 SizeOfHeapReserve;
    public uint32 SizeOfHeapCommit;
    public uint32 LoaderFlags;
    public uint32 NumberOfRvaAndSizes;
    public array<DataDirectory> DataDirectory = new(16);
}

partial struct OptionalHeader64 {
    public uint16 Magic;
    public uint8 MajorLinkerVersion;
    public uint8 MinorLinkerVersion;
    public uint32 SizeOfCode;
    public uint32 SizeOfInitializedData;
    public uint32 SizeOfUninitializedData;
    public uint32 AddressOfEntryPoint;
    public uint32 BaseOfCode;
    public uint64 ImageBase;
    public uint32 SectionAlignment;
    public uint32 FileAlignment;
    public uint16 MajorOperatingSystemVersion;
    public uint16 MinorOperatingSystemVersion;
    public uint16 MajorImageVersion;
    public uint16 MinorImageVersion;
    public uint16 MajorSubsystemVersion;
    public uint16 MinorSubsystemVersion;
    public uint32 Win32VersionValue;
    public uint32 SizeOfImage;
    public uint32 SizeOfHeaders;
    public uint32 CheckSum;
    public uint16 Subsystem;
    public uint16 DllCharacteristics;
    public uint64 SizeOfStackReserve;
    public uint64 SizeOfStackCommit;
    public uint64 SizeOfHeapReserve;
    public uint64 SizeOfHeapCommit;
    public uint32 LoaderFlags;
    public uint32 NumberOfRvaAndSizes;
    public array<DataDirectory> DataDirectory = new(16);
}

public static UntypedInt IMAGE_FILE_MACHINE_UNKNOWN => 0x0;
public static UntypedInt IMAGE_FILE_MACHINE_AM33 => 0x1d3;
public static UntypedInt IMAGE_FILE_MACHINE_AMD64 => 0x8664;
public static UntypedInt IMAGE_FILE_MACHINE_ARM => 0x1c0;
public static UntypedInt IMAGE_FILE_MACHINE_ARMNT => 0x1c4;
public static UntypedInt IMAGE_FILE_MACHINE_ARM64 => 0xaa64;
public static UntypedInt IMAGE_FILE_MACHINE_EBC => 0xebc;
public static UntypedInt IMAGE_FILE_MACHINE_I386 => 0x14c;
public static UntypedInt IMAGE_FILE_MACHINE_IA64 => 0x200;
public static UntypedInt IMAGE_FILE_MACHINE_LOONGARCH32 => 0x6232;
public static UntypedInt IMAGE_FILE_MACHINE_LOONGARCH64 => 0x6264;
public static UntypedInt IMAGE_FILE_MACHINE_M32R => 0x9041;
public static UntypedInt IMAGE_FILE_MACHINE_MIPS16 => 0x266;
public static UntypedInt IMAGE_FILE_MACHINE_MIPSFPU => 0x366;
public static UntypedInt IMAGE_FILE_MACHINE_MIPSFPU16 => 0x466;
public static UntypedInt IMAGE_FILE_MACHINE_POWERPC => 0x1f0;
public static UntypedInt IMAGE_FILE_MACHINE_POWERPCFP => 0x1f1;
public static UntypedInt IMAGE_FILE_MACHINE_R4000 => 0x166;
public static UntypedInt IMAGE_FILE_MACHINE_SH3 => 0x1a2;
public static UntypedInt IMAGE_FILE_MACHINE_SH3DSP => 0x1a3;
public static UntypedInt IMAGE_FILE_MACHINE_SH4 => 0x1a6;
public static UntypedInt IMAGE_FILE_MACHINE_SH5 => 0x1a8;
public static UntypedInt IMAGE_FILE_MACHINE_THUMB => 0x1c2;
public static UntypedInt IMAGE_FILE_MACHINE_WCEMIPSV2 => 0x169;
public static UntypedInt IMAGE_FILE_MACHINE_RISCV32 => 0x5032;
public static UntypedInt IMAGE_FILE_MACHINE_RISCV64 => 0x5064;
public static UntypedInt IMAGE_FILE_MACHINE_RISCV128 => 0x5128;

// IMAGE_DIRECTORY_ENTRY constants
public static UntypedInt IMAGE_DIRECTORY_ENTRY_EXPORT => 0;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_IMPORT => 1;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_RESOURCE => 2;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_EXCEPTION => 3;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_SECURITY => 4;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_BASERELOC => 5;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_DEBUG => 6;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_ARCHITECTURE => 7;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_GLOBALPTR => 8;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_TLS => 9;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG => 10;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT => 11;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_IAT => 12;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT => 13;

public static UntypedInt IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR => 14;

// Values of IMAGE_FILE_HEADER.Characteristics. These can be combined together.
public static UntypedInt IMAGE_FILE_RELOCS_STRIPPED => 0x0001;

public static UntypedInt IMAGE_FILE_EXECUTABLE_IMAGE => 0x0002;

public static UntypedInt IMAGE_FILE_LINE_NUMS_STRIPPED => 0x0004;

public static UntypedInt IMAGE_FILE_LOCAL_SYMS_STRIPPED => 0x0008;

public static UntypedInt IMAGE_FILE_AGGRESIVE_WS_TRIM => 0x0010;

public static UntypedInt IMAGE_FILE_LARGE_ADDRESS_AWARE => 0x0020;

public static UntypedInt IMAGE_FILE_BYTES_REVERSED_LO => 0x0080;

public static UntypedInt IMAGE_FILE_32BIT_MACHINE => 0x0100;

public static UntypedInt IMAGE_FILE_DEBUG_STRIPPED => 0x0200;

public static UntypedInt IMAGE_FILE_REMOVABLE_RUN_FROM_SWAP => 0x0400;

public static UntypedInt IMAGE_FILE_NET_RUN_FROM_SWAP => 0x0800;

public static UntypedInt IMAGE_FILE_SYSTEM => 0x1000;

public static UntypedInt IMAGE_FILE_DLL => 0x2000;

public static UntypedInt IMAGE_FILE_UP_SYSTEM_ONLY => 0x4000;

public static UntypedInt IMAGE_FILE_BYTES_REVERSED_HI => 0x8000;

// OptionalHeader64.Subsystem and OptionalHeader32.Subsystem values.
public static UntypedInt IMAGE_SUBSYSTEM_UNKNOWN => 0;

public static UntypedInt IMAGE_SUBSYSTEM_NATIVE => 1;

public static UntypedInt IMAGE_SUBSYSTEM_WINDOWS_GUI => 2;

public static UntypedInt IMAGE_SUBSYSTEM_WINDOWS_CUI => 3;

public static UntypedInt IMAGE_SUBSYSTEM_OS2_CUI => 5;

public static UntypedInt IMAGE_SUBSYSTEM_POSIX_CUI => 7;

public static UntypedInt IMAGE_SUBSYSTEM_NATIVE_WINDOWS => 8;

public static UntypedInt IMAGE_SUBSYSTEM_WINDOWS_CE_GUI => 9;

public static UntypedInt IMAGE_SUBSYSTEM_EFI_APPLICATION => 10;

public static UntypedInt IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER => 11;

public static UntypedInt IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER => 12;

public static UntypedInt IMAGE_SUBSYSTEM_EFI_ROM => 13;

public static UntypedInt IMAGE_SUBSYSTEM_XBOX => 14;

public static UntypedInt IMAGE_SUBSYSTEM_WINDOWS_BOOT_APPLICATION => 16;

// OptionalHeader64.DllCharacteristics and OptionalHeader32.DllCharacteristics
// values. These can be combined together.
public static UntypedInt IMAGE_DLLCHARACTERISTICS_HIGH_ENTROPY_VA => 0x0020;

public static UntypedInt IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE => 0x0040;

public static UntypedInt IMAGE_DLLCHARACTERISTICS_FORCE_INTEGRITY => 0x0080;

public static UntypedInt IMAGE_DLLCHARACTERISTICS_NX_COMPAT => 0x0100;

public static UntypedInt IMAGE_DLLCHARACTERISTICS_NO_ISOLATION => 0x0200;

public static UntypedInt IMAGE_DLLCHARACTERISTICS_NO_SEH => 0x0400;

public static UntypedInt IMAGE_DLLCHARACTERISTICS_NO_BIND => 0x0800;

public static UntypedInt IMAGE_DLLCHARACTERISTICS_APPCONTAINER => 0x1000;

public static UntypedInt IMAGE_DLLCHARACTERISTICS_WDM_DRIVER => 0x2000;

public static UntypedInt IMAGE_DLLCHARACTERISTICS_GUARD_CF => 0x4000;

public static UntypedInt IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE => 0x8000;

} // end pe_package

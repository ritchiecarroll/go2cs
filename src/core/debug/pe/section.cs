// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using binary = encoding.binary_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using io = io_package;
using strconv = strconv_package;
using @internal;
using encoding;

partial class pe_package {

// SectionHeader32 represents real PE COFF section header.
[GoType] partial struct SectionHeader32 {
    public array<uint8> Name = new(8);
    public uint32 VirtualSize;
    public uint32 VirtualAddress;
    public uint32 SizeOfRawData;
    public uint32 PointerToRawData;
    public uint32 PointerToRelocations;
    public uint32 PointerToLineNumbers;
    public uint16 NumberOfRelocations;
    public uint16 NumberOfLineNumbers;
    public uint32 Characteristics;
}

// fullName finds real name of section sh. Normally name is stored
// in sh.Name, but if it is longer then 8 characters, it is stored
// in COFF string table st instead.
[GoRecv] internal static (@string, error) fullName(this ref SectionHeader32 sh, StringTable st) {
    if (sh.Name[0] != (rune)'/') {
        return (cstring(sh.Name[..]), default!);
    }
    var (i, err) = strconv.Atoi(cstring(sh.Name[1..]));
    if (err != default!) {
        return ("", err);
    }
    return st.String((uint32)i);
}

// TODO(brainman): copy all IMAGE_REL_* consts from ldpe.go here

// Reloc represents a PE COFF relocation.
// Each section contains its own relocation list.
[GoType] partial struct Reloc {
    public uint32 VirtualAddress;
    public uint32 SymbolTableIndex;
    public uint16 Type;
}

internal static (slice<Reloc>, error) readRelocs(ж<SectionHeader> Ꮡsh, io.ReadSeeker r) {
    ref var sh = ref Ꮡsh.DerefOrNull();

    if (sh.NumberOfRelocations <= 0) {
        return (default!, default!);
    }
    var (_, err) = r.Seek((int64)sh.PointerToRelocations, io.SeekStart);
    if (err != default!) {
        return (default!, fmt.Errorf("fail to seek to %q section relocations: %v"u8, sh.Name, err));
    }
    var relocs = new slice<Reloc>(sh.NumberOfRelocations);
    err = binary.Read(r, binary.LittleEndian, relocs);
    if (err != default!) {
        return (default!, fmt.Errorf("fail to read section relocations: %v"u8, err));
    }
    return (relocs, default!);
}

// SectionHeader is similar to [SectionHeader32] with Name
// field replaced by Go string.
[GoType] partial struct SectionHeader {
    public @string Name;
    public uint32 VirtualSize;
    public uint32 VirtualAddress;
    public uint32 Size;
    public uint32 Offset;
    public uint32 PointerToRelocations;
    public uint32 PointerToLineNumbers;
    public uint16 NumberOfRelocations;
    public uint16 NumberOfLineNumbers;
    public uint32 Characteristics;
}

// Section provides access to PE COFF section.
[GoType] partial struct ΔSection {
    public partial ref SectionHeader SectionHeader { get; }
    public slice<Reloc> Relocs;
    // Embed ReaderAt for ReadAt method.
    // Do not embed SectionReader directly
    // to avoid having Read and Seek.
    // If a client wants Read and Seek it must use
    // Open() to avoid fighting over the seek offset
    // with other clients.
    [GoEmbedded] public io_package.ReaderAt ReaderAt;
    internal ж<io.SectionReader> sr;
}

// Data reads and returns the contents of the PE section s.
//
// If s.Offset is 0, the section has no contents,
// and Data will always return a non-nil error.
[GoRecv] public static (slice<byte>, error) Data(this ref ΔSection s) {
    return saferio.ReadDataAt(new io.SectionReaderжReaderAt(s.sr), (uint64)s.Size, 0);
}

// Open returns a new ReadSeeker reading the PE section s.
//
// If s.Offset is 0, the section has no contents, and all calls
// to the returned reader will return a non-nil error.
[GoRecv] public static io.ReadSeeker Open(this ref ΔSection s) {
    return new io.SectionReaderжReadSeeker(io.NewSectionReader(new io.SectionReaderжReaderAt(s.sr), 0, 9223372036854775807L));
}

// Section characteristics flags.
public static UntypedInt IMAGE_SCN_CNT_CODE => 0x00000020;

public static UntypedInt IMAGE_SCN_CNT_INITIALIZED_DATA => 0x00000040;

public static UntypedInt IMAGE_SCN_CNT_UNINITIALIZED_DATA => 0x00000080;

public static UntypedInt IMAGE_SCN_LNK_COMDAT => 0x00001000;

public static UntypedInt IMAGE_SCN_MEM_DISCARDABLE => 0x02000000;

public static UntypedInt IMAGE_SCN_MEM_EXECUTE => 0x20000000;

public static UntypedInt IMAGE_SCN_MEM_READ => 0x40000000;

public static UntypedInt IMAGE_SCN_MEM_WRITE => 0x80000000;

} // end pe_package

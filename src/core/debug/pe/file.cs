// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package pe implements access to PE (Microsoft Windows Portable Executable) files.

# Security

This package is not designed to be hardened against adversarial inputs, and is
outside the scope of https://go.dev/security/policy. In particular, only basic
validation is done when parsing object files. As such, care should be taken when
parsing untrusted inputs, as parsing malformed files may consume significant
resources, or cause panics.
*/
namespace go.debug;

using bytes = bytes_package;
using zlib = compress.zlib_package;
using dwarf = go.debug.dwarf_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using strings = strings_package;
using compress;
using encoding;
using go.debug;

partial class pe_package {

// A File represents an open PE file.
partial struct File {
    public partial ref FileHeader FileHeader { get; }
    public any OptionalHeader; // of type *OptionalHeader32 or *OptionalHeader64
    public slice<ж<ΔSection>> Sections;
    public slice<ж<Symbol>> Symbols; // COFF symbols with auxiliary symbol records removed
    public slice<COFFSymbol> COFFSymbols; // all COFF symbols (including auxiliary symbol records)
    public StringTable StringTable;
    internal io.Closer closer;
}

// Open opens the named file using [os.Open] and prepares it for use as a PE binary.
public static (ж<File>, error) Open(@string name) {
    var (f, err) = os.Open(name);
    if (err != default!) {
        return (default!, err);
    }
    (var ff, err) = NewFile(new os_FileжReaderAt(f));
    if (err != default!) {
        f.Close();
        return (default!, err);
    }
    ff.Value.closer = new os_FileжCloser(f);
    return (ff, default!);
}

// Close closes the [File].
// If the [File] was created using [NewFile] directly instead of [Open],
// Close has no effect.
public static error Close(this ref File f) {
    error err = default!;
    if (f.closer != default!) {
        err = f.closer.Close();
        f.closer = default!;
    }
    return err;
}

// TODO(brainman): add Load function, as a replacement for NewFile, that does not call removeAuxSymbols (for performance)

// NewFile creates a new [File] for accessing a PE binary in an underlying reader.
public static (ж<File>, error) NewFile(io.ReaderAt r) {
    var f = @new<File>();
    var sr = io.NewSectionReader(r, 0, 9223372036854775807L);
    array<byte> dosheader = new(96);
    {
        var (_, errΔ1) = r.ReadAt(dosheader[0..], 0); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    int64 @base = default!;
    if (dosheader[0] == (rune)'M' && dosheader[1] == (rune)'Z'){
        var signoff = (int64)binary.LittleEndian.Uint32(dosheader[0x3c..]);
        array<byte> sign = new(4);
        r.ReadAt(sign[..], signoff);
        if (!(sign[0] == (rune)'P' && sign[1] == (rune)'E' && sign[2] == 0 && sign[3] == 0)) {
            return (default!, fmt.Errorf("invalid PE file signature: % x"u8, sign));
        }
        @base = signoff + 4;
    } else {
        @base = (int64)0;
    }
    sr.Seek(@base, io.SeekStart);
    {
        var errΔ2 = binary.Read(new io.SectionReaderжReader(sr), binary.LittleEndian, f.of(File.ᏑFileHeader)); if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
    }
    var exprᴛ1 = (~f).FileHeader.Machine;
    if (exprᴛ1 == IMAGE_FILE_MACHINE_AMD64 || exprᴛ1 == IMAGE_FILE_MACHINE_ARM64 || exprᴛ1 == IMAGE_FILE_MACHINE_ARMNT || exprᴛ1 == IMAGE_FILE_MACHINE_I386 || exprᴛ1 == IMAGE_FILE_MACHINE_RISCV32 || exprᴛ1 == IMAGE_FILE_MACHINE_RISCV64 || exprᴛ1 == IMAGE_FILE_MACHINE_RISCV128 || exprᴛ1 == IMAGE_FILE_MACHINE_UNKNOWN) {
    }
    else { /* default: */
        return (default!, fmt.Errorf("unrecognized PE machine: %#x"u8, // ok
 (~f).FileHeader.Machine));
    }

    error err = default!;
    // Read string table.
    (f.Value.StringTable, err) = readStringTable(ref (f.of(File.ᏑFileHeader)).DerefOrNull(), new io.SectionReaderжReadSeeker(sr));
    if (err != default!) {
        return (default!, err);
    }
    // Read symbol table.
    (f.Value.COFFSymbols, err) = readCOFFSymbols(ref (f.of(File.ᏑFileHeader)).DerefOrNull(), new io.SectionReaderжReadSeeker(sr));
    if (err != default!) {
        return (default!, err);
    }
    (f.Value.Symbols, err) = removeAuxSymbols((~f).COFFSymbols, (~f).StringTable);
    if (err != default!) {
        return (default!, err);
    }
    // Seek past file header.
    (_, err) = sr.Seek(@base + (int64)binary.Size((~f).FileHeader), io.SeekStart);
    if (err != default!) {
        return (default!, err);
    }
    // Read optional header.
    (f.Value.OptionalHeader, err) = readOptionalHeader(new io.SectionReaderжReadSeeker(sr), (~f).FileHeader.SizeOfOptionalHeader);
    if (err != default!) {
        return (default!, err);
    }
    // Process sections.
    f.Value.Sections = new slice<ж<ΔSection>>((~f).FileHeader.NumberOfSections);
    for (nint i = 0; i < (nint)(~f).FileHeader.NumberOfSections; i++) {
        var sh = @new<SectionHeader32>();
        {
            var errΔ3 = binary.Read(new io.SectionReaderжReader(sr), binary.LittleEndian, sh.OrTypedNil()); if (errΔ3 != default!) {
                return (default!, errΔ3);
            }
        }
        var (name, errΔ4) = sh.fullName((~f).StringTable);
        if (errΔ4 != default!) {
            return (default!, errΔ4);
        }
        var s = @new<ΔSection>();
        s.Value.SectionHeader = new SectionHeader(
            Name: name,
            VirtualSize: (~sh).VirtualSize,
            VirtualAddress: (~sh).VirtualAddress,
            Size: (~sh).SizeOfRawData,
            Offset: (~sh).PointerToRawData,
            PointerToRelocations: (~sh).PointerToRelocations,
            PointerToLineNumbers: (~sh).PointerToLineNumbers,
            NumberOfRelocations: (~sh).NumberOfRelocations,
            NumberOfLineNumbers: (~sh).NumberOfLineNumbers,
            Characteristics: (~sh).Characteristics
        );
        var r2 = r;
        if ((~sh).PointerToRawData == 0) {
            // .bss must have all 0s
            r2 = new nobitsSectionReaderжReaderAt(Ꮡ(new nobitsSectionReader(nil)));
        }
        s.Value.sr = io.NewSectionReader(r2, (int64)(~s).SectionHeader.Offset, (int64)(~s).SectionHeader.Size);
        s.Value.ReaderAt = new io.SectionReaderжReaderAt(s.Value.sr);
        f.Value.Sections[i] = s;
    }
    foreach (var (i, _) in (~f).Sections) {
        error errΔ5 = default!;
        ((~f).Sections[i].Value.Relocs, errΔ5) = readRelocs((~f).Sections[i].of(pe_package.ΔSection.ᏑSectionHeader), new io.SectionReaderжReadSeeker(sr));
        if (errΔ5 != default!) {
            return (default!, errΔ5);
        }
    }
    return (f, default!);
}

partial struct nobitsSectionReader {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedReadFromˢ = "unexpected read from section with uninitialized data"u8;

internal static (nint n, error err) ReadAt(this ref nobitsSectionReader _, slice<byte> p, int64 off) {
    return (0, errors.New(unexpectedReadFromˢ));
}

// getString extracts a string from symbol string table.
internal static (@string, bool) getString(slice<byte> section, nint start) {
    if (start < 0 || start >= len(section)) {
        return ("", false);
    }
    for (nint end = start; end < len(section); end++) {
        if (section[end] == 0) {
            return (((@string)(section.slice(start, end))), true);
        }
    }
    return ("", false);
}

// Section returns the first section with the given name, or nil if no such
// section exists.
public static ж<ΔSection> Section(this ref File f, @string name) {
    foreach (var (_, s) in f.Sections) {
        if ((~s).Name == name) {
            return s;
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string debugˢ = ".debug_"u8;
internal static readonly @string zdebugˢ = ".zdebug_"u8;
internal static readonly @string abbrevˢ = "abbrev"u8;
internal static readonly @string infoˢ = "info"u8;
internal static readonly @string lineˢ = "line"u8;
internal static readonly @string rangesˢ = "ranges"u8;
internal static readonly @string strˢ = "str"u8;

public static (ж<dwarf.Data>, error) DWARF(this ref File f) {
    @string dwarfSuffix(ж<ΔSection> s) {
        switch (ᐧ) {
        case {} when strings.HasPrefix((~s).Name, debugˢ): {
            return (~s).Name[7..];
        }
        case {} when strings.HasPrefix((~s).Name, zdebugˢ): {
            return (~s).Name[8..];
        }
        default: {
            return ""u8;
        }}

    }
    // sectionData gets the data for s and checks its size.
    (slice<byte>, error) sectionData(ж<ΔSection> s) {
        var (b, errΔ1) = s.Data();
        if (errΔ1 != default! && (uint32)len(b) < (~s).Size) {
            return (default!, errΔ1);
        }
        if (0 < (~s).VirtualSize && (~s).VirtualSize < (~s).Size) {
            b = b.slice(0, (nint)((~s).VirtualSize));
        }
        if (len(b) >= 12 && ((sstring)(b[..4])) == "ZLIB"u8) {
            var dlen = binary.BigEndian.Uint64(b[4..12]);
            var dbuf = new slice<byte>((nint)(dlen));
            var (r, errΔ2) = zlib.NewReader(new bytes_BufferжReader(bytes.NewBuffer(b[12..])));
            if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
            {
                var (_, errΔ3) = io.ReadFull(r, dbuf); if (errΔ3 != default!) {
                    return (default!, errΔ3);
                }
            }
            {
                var errΔ4 = r.Close(); if (errΔ4 != default!) {
                    return (default!, errΔ4);
                }
            }
            b = dbuf;
        }
        return (b, default!);
    }
    // There are many other DWARF sections, but these
    // are the ones the debug/dwarf package uses.
    // Don't bother loading others.
    map<@string, slice<byte>> dat = new map<@string, slice<byte>>{["abbrev"u8] = default!, ["info"u8] = default!, ["str"u8] = default!, ["line"u8] = default!, ["ranges"u8] = default!};
    foreach (var (_, s) in f.Sections) {
        @string suffix = dwarfSuffix(s);
        if (suffix == ""u8) {
            continue;
        }
        {
            var (_, ok) = dat[suffix, ꟷ]; if (!ok) {
                continue;
            }
        }
        var (b, errΔ5) = sectionData(s);
        if (errΔ5 != default!) {
            return (default!, errΔ5);
        }
        dat[suffix] = b;
    }
    var (d, err) = dwarf.New(dat[abbrevˢ], default!, default!, dat[infoˢ], dat[lineˢ], default!, dat[rangesˢ], dat[strˢ]);
    if (err != default!) {
        return (default!, err);
    }
    // Look for DWARF4 .debug_types sections and DWARF5 sections.
    foreach (var (i, s) in f.Sections) {
        @string suffix = dwarfSuffix(s);
        if (suffix == ""u8) {
            continue;
        }
        {
            var (_, ok) = dat[suffix, ꟷ]; if (ok) {
                // Already handled.
                continue;
            }
        }
        var (b, errΔ6) = sectionData(s);
        if (errΔ6 != default!) {
            return (default!, errΔ6);
        }
        if (suffix == "types"u8){
            errΔ6 = d.AddTypes(fmt.Sprintf("types-%d"u8, i), b);
        } else {
            errΔ6 = d.AddSection(".debug_"u8 + suffix, b);
        }
        if (errΔ6 != default!) {
            return (default!, errΔ6);
        }
    }
    return (d, default!);
}

// TODO(brainman): document ImportDirectory once we decide what to do with it.
partial struct ImportDirectory {
    public uint32 OriginalFirstThunk;
    public uint32 TimeDateStamp;
    public uint32 ForwarderChain;
    public uint32 Name;
    public uint32 FirstThunk;
    internal @string dll;
}

// ImportedSymbols returns the names of all symbols
// referred to by the binary f that are expected to be
// satisfied by other libraries at dynamic load time.
// It does not return weak symbols.
public static (slice<@string>, error) ImportedSymbols(this ref File f) {
    if (f.OptionalHeader == default!) {
        return (default!, default!);
    }
    var (_, pe64) = f.OptionalHeader._<ж<OptionalHeader64>>(ᐧ);
    // grab the number of data directory entries
    uint32 dd_length = default!;
    if (pe64){
        dd_length = f.OptionalHeader._<ж<OptionalHeader64>>().Value.NumberOfRvaAndSizes;
    } else {
        dd_length = f.OptionalHeader._<ж<OptionalHeader32>>().Value.NumberOfRvaAndSizes;
    }
    // check that the length of data directory entries is large
    // enough to include the imports directory.
    if (dd_length < (uint32)(IMAGE_DIRECTORY_ENTRY_IMPORT + 1)) {
        return (default!, default!);
    }
    // grab the import data directory entry
    DataDirectory idd = default!;
    if (pe64){
        idd = (~f.OptionalHeader._<ж<OptionalHeader64>>()).DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT];
    } else {
        idd = (~f.OptionalHeader._<ж<OptionalHeader32>>()).DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT];
    }
    // figure out which section contains the import directory table
    ж<ΔSection> ds = default!;
    ds = default!;
    foreach (var (_, s) in f.Sections) {
        if ((~s).Offset == 0) {
            continue;
        }
        // We are using distance between s.VirtualAddress and idd.VirtualAddress
        // to avoid potential overflow of uint32 caused by addition of s.VirtualSize
        // to s.VirtualAddress.
        if ((~s).VirtualAddress <= idd.VirtualAddress && idd.VirtualAddress - (~s).VirtualAddress < (~s).VirtualSize) {
            ds = s;
            break;
        }
    }
    // didn't find a section, so no import libraries were found
    if (ds == nil) {
        return (default!, default!);
    }
    var (d, err) = ds.Data();
    if (err != default!) {
        return (default!, err);
    }
    // seek to the virtual address specified in the import data directory
    d = d.slice((nint)(idd.VirtualAddress - (~ds).VirtualAddress));
    // start decoding the import directory
    slice<ImportDirectory> ida = default!;
    while (len(d) >= 20) {
        ImportDirectory dt = default!;
        dt.OriginalFirstThunk = binary.LittleEndian.Uint32(d[0..4]);
        dt.TimeDateStamp = binary.LittleEndian.Uint32(d[4..8]);
        dt.ForwarderChain = binary.LittleEndian.Uint32(d[8..12]);
        dt.Name = binary.LittleEndian.Uint32(d[12..16]);
        dt.FirstThunk = binary.LittleEndian.Uint32(d[16..20]);
        d = d[20..];
        if (dt.OriginalFirstThunk == 0) {
            break;
        }
        ida = append(ida, dt);
    }
    // TODO(brainman): this needs to be rewritten
    //  ds.Data() returns contents of section containing import table. Why store in variable called "names"?
    //  Why we are retrieving it second time? We already have it in "d", and it is not modified anywhere.
    //  getString does not extracts a string from symbol string table (as getString doco says).
    //  Why ds.Data() called again and again in the loop?
    //  Needs test before rewrite.
    var (names, _) = ds.Data();
    slice<@string> all = default!;
    foreach (var (_, vᴛ1) in ida) {
        var dt = vᴛ1;

        (dt.dll, _) = getString(names, (nint)(dt.Name - (~ds).VirtualAddress));
        (d, _) = ds.Data();
        // seek to OriginalFirstThunk
        d = d.slice((nint)(dt.OriginalFirstThunk - (~ds).VirtualAddress));
        while (len(d) > 0) {
            if (pe64){
                // 64bit
                var va = binary.LittleEndian.Uint64(d[0..8]);
                d = d[8..];
                if (va == 0) {
                    break;
                }
                if ((uint64)(va & 0x8000000000000000UL) > 0){
                } else {
                    // is Ordinal
                    // TODO add dynimport ordinal support.
                    var (fn, _) = getString(names, (nint)((uint32)va - (~ds).VirtualAddress + 2));
                    all = append(all, fn + ":"u8 + dt.dll);
                }
            } else {
                // 32bit
                var va = binary.LittleEndian.Uint32(d[0..4]);
                d = d[4..];
                if (va == 0) {
                    break;
                }
                if ((uint32)(va & 0x80000000U) > 0){
                } else {
                    // is Ordinal
                    // TODO add dynimport ordinal support.
                    //ord := va&0x0000FFFF
                    var (fn, _) = getString(names, (nint)(va - (~ds).VirtualAddress + 2));
                    all = append(all, fn + ":"u8 + dt.dll);
                }
            }
        }
    }
    return (all, default!);
}

// ImportedLibraries returns the names of all libraries
// referred to by the binary f that are expected to be
// linked with the binary at dynamic link time.
public static (slice<@string>, error) ImportedLibraries(this ref File f) {
    // TODO
    // cgo -dynimport don't use this for windows PE, so just return.
    return (default!, default!);
}

// FormatError is unused.
// The type is retained for compatibility.
partial struct FormatError {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownErrorˢ = "unknown error"u8;

public static @string Error(this ref FormatError e) {
    return unknownErrorˢ;
}

// readOptionalHeader accepts an io.ReadSeeker pointing to optional header in the PE file
// and its size as seen in the file header.
// It parses the given size of bytes and returns optional header. It infers whether the
// bytes being parsed refer to 32 bit or 64 bit version of optional header.
internal static (any, error) readOptionalHeader(io.ReadSeeker r, uint16 sz) {
    // If optional header size is 0, return empty optional header.
    if (sz == 0) {
        return (default!, default!);
    }
    ref var ohMagic = ref heap(new uint16(), out var ᏑohMagic);
    nint ohMagicSz = binary.Size(ohMagic);
    // If optional header size is greater than 0 but less than its magic size, return error.
    if (sz < (uint16)ohMagicSz) {
        return (default!, fmt.Errorf("optional header size is less than optional header magic size"u8));
    }
    // read reads from io.ReadSeeke, r, into data.
    ref var err = ref heap<error>(out var Ꮡerr);
    bool read(any data) {
        Ꮡerr.ValueSlot = binary.Read(r, binary.LittleEndian, data);
        return Ꮡerr.ValueSlot == default!;
    }
    if (!read(ᏑohMagic)) {
        return (default!, fmt.Errorf("failure to read optional header magic: %v"u8, err));
    }
    switch (ohMagic) {
    case 0x10b: {
// PE32
        ref var oh32 = ref heap(new OptionalHeader32(), out var Ꮡoh32);
        nint oh32MinSz = binary.Size(oh32) - binary.Size(oh32.DataDirectory);
        if (sz < (uint16)oh32MinSz) {
            return (default!, fmt.Errorf("optional header size(%d) is less minimum size (%d) of PE32 optional header"u8, sz, oh32MinSz));
        }
        oh32.Magic = ohMagic;
        if (!read(Ꮡoh32.of(OptionalHeader32.ᏑMajorLinkerVersion)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑMinorLinkerVersion)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑSizeOfCode)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑSizeOfInitializedData)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑSizeOfUninitializedData)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑAddressOfEntryPoint)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑBaseOfCode)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑBaseOfData)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑImageBase)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑSectionAlignment)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑFileAlignment)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑMajorOperatingSystemVersion)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑMinorOperatingSystemVersion)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑMajorImageVersion)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑMinorImageVersion)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑMajorSubsystemVersion)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑMinorSubsystemVersion)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑWin32VersionValue)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑSizeOfImage)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑSizeOfHeaders)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑCheckSum)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑSubsystem)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑDllCharacteristics)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑSizeOfStackReserve)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑSizeOfStackCommit)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑSizeOfHeapReserve)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑSizeOfHeapCommit)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑLoaderFlags)) || !read(Ꮡoh32.of(OptionalHeader32.ᏑNumberOfRvaAndSizes))) {
            // Init oh32 fields
            return (default!, fmt.Errorf("failure to read PE32 optional header: %v"u8, err));
        }
        var (dd, errΔ2) = readDataDirectories(r, (uint16)(sz - (uint16)oh32MinSz), oh32.NumberOfRvaAndSizes);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        copy(oh32.DataDirectory[..], dd);
        return (Ꮡoh32, default!);
    }
    case 0x20b: {
// PE32+
        ref var oh64 = ref heap(new OptionalHeader64(), out var Ꮡoh64);
        nint oh64MinSz = binary.Size(oh64) - binary.Size(oh64.DataDirectory);
        if (sz < (uint16)oh64MinSz) {
            return (default!, fmt.Errorf("optional header size(%d) is less minimum size (%d) for PE32+ optional header"u8, sz, oh64MinSz));
        }
        oh64.Magic = ohMagic;
        if (!read(Ꮡoh64.of(OptionalHeader64.ᏑMajorLinkerVersion)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑMinorLinkerVersion)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑSizeOfCode)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑSizeOfInitializedData)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑSizeOfUninitializedData)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑAddressOfEntryPoint)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑBaseOfCode)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑImageBase)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑSectionAlignment)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑFileAlignment)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑMajorOperatingSystemVersion)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑMinorOperatingSystemVersion)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑMajorImageVersion)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑMinorImageVersion)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑMajorSubsystemVersion)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑMinorSubsystemVersion)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑWin32VersionValue)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑSizeOfImage)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑSizeOfHeaders)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑCheckSum)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑSubsystem)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑDllCharacteristics)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑSizeOfStackReserve)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑSizeOfStackCommit)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑSizeOfHeapReserve)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑSizeOfHeapCommit)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑLoaderFlags)) || !read(Ꮡoh64.of(OptionalHeader64.ᏑNumberOfRvaAndSizes))) {
            // Init oh64 fields
            return (default!, fmt.Errorf("failure to read PE32+ optional header: %v"u8, err));
        }
        var (dd, errΔ3) = readDataDirectories(r, (uint16)(sz - (uint16)oh64MinSz), oh64.NumberOfRvaAndSizes);
        if (errΔ3 != default!) {
            return (default!, errΔ3);
        }
        copy(oh64.DataDirectory[..], dd);
        return (Ꮡoh64, default!);
    }
    default: {
        return (default!, fmt.Errorf("optional header has unexpected Magic of 0x%x"u8, ohMagic));
    }}

}

// readDataDirectories accepts an io.ReadSeeker pointing to data directories in the PE file,
// its size and number of data directories as seen in optional header.
// It parses the given size of bytes and returns given number of data directories.
internal static (slice<DataDirectory>, error) readDataDirectories(io.ReadSeeker r, uint16 sz, uint32 n) {
    var ddSz = (uint64)binary.Size(new DataDirectory(nil));
    if ((uint64)sz != (uint64)n * ddSz) {
        return (default!, fmt.Errorf("size of data directories(%d) is inconsistent with number of data directories(%d)"u8, sz, n));
    }
    var dd = new slice<DataDirectory>((nint)(n));
    {
        var err = binary.Read(r, binary.LittleEndian, dd); if (err != default!) {
            return (default!, fmt.Errorf("failure to read data directories: %v"u8, err));
        }
    }
    return (dd, default!);
}

} // end pe_package

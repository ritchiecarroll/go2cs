// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package buildinfo provides access to information embedded in a Go binary
// about how it was built. This includes the Go toolchain version, and the
// set of modules used (for binaries built in module mode).
//
// Build information is available for the currently running binary in
// runtime/debug.ReadBuildInfo.
global using BuildInfo = go.runtime.debug_package.BuildInfo;

namespace go.debug;

using bytes = bytes_package;
using elf = go.debug.elf_package;
using macho = go.debug.macho_package;
using pe = go.debug.pe_package;
using plan9obj = go.debug.plan9obj_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using xcoff = @internal.xcoff_package;
using io = io_package;
using fs = go.io.fs_package;
using os = os_package;
using debug = runtime.debug_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using @internal;
using encoding;
using go.debug;
using go.io;
using runtime;

partial class buildinfo_package {

// errUnrecognizedFormat is returned when a given executable file doesn't
// appear to be in a known format, or it breaks the rules of that format,
// or when there are I/O errors reading the file.
internal static error errUnrecognizedFormat = errors.New("unrecognized file format"u8);

// errNotGoExe is returned when a given executable file is valid but does
// not contain Go build information.
//
// errNotGoExe should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/quay/claircore
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname errNotGoExe
public static error errNotGoExe = errors.New("not a Go executable"u8);

// The build info blob left by the linker is identified by a 32-byte header,
// consisting of buildInfoMagic (14 bytes), followed by version-dependent
// fields.
internal static slice<byte> buildInfoMagic = slice<byte>(((@string)(new byte[]{0xff, 0x20, 0x47, 0x6f, 0x20, 0x62, 0x75, 0x69, 0x6c, 0x64, 0x69, 0x6e, 0x66, 0x3a})));

internal static UntypedInt buildInfoAlign => 16;
internal static UntypedInt buildInfoHeaderSize => 32;

// ReadFile returns build information embedded in a Go binary
// file at the given path. Most information is only available for binaries built
// with module support.
public static (ж<BuildInfo> info, error err) ReadFile(@string name) {
    ж<BuildInfo> info = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var err = ref Ꮡerr.ValueSlot;

        defer(() => {
            {
                ref var pathErr = ref heap<ж<fs.PathError>>(out var ᏑpathErr);
                pathErr = ((ж<fs.PathError>)nil); if (errors.As(Ꮡerr.ValueSlot, ᏑpathErr)){
                    Ꮡerr.ValueSlot = fmt.Errorf("could not read Go build info: %w"u8, Ꮡerr.ValueSlot);
                } else 
                if (Ꮡerr.ValueSlot != default!) {
                    Ꮡerr.ValueSlot = fmt.Errorf("could not read Go build info from %s: %w"u8, name, Ꮡerr.ValueSlot);
                }
            }
        }, ref ᒐ);
        (var f, err) = os.Open(name);
        if (err != default!) {
            (info, err) = (default!, err); goto ᒐdone;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (info, err) = Read(new os_FileжReaderAt(f));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (info, Ꮡerr.ValueSlot);
}

// Read returns build information embedded in a Go binary file
// accessed through the given ReaderAt. Most information is only available for
// binaries built with module support.
public static (ж<BuildInfo>, error) Read(io.ReaderAt r) {
    var (vers, mod, err) = readRawBuildInfo(r);
    if (err != default!) {
        return (default!, err);
    }
    (var bi, err) = debug.ParseBuildInfo(mod);
    if (err != default!) {
        return (default!, err);
    }
    bi.Value.GoVersion = vers;
    return (bi, default!);
}

[GoType] partial interface exe {
    // DataStart returns the virtual address and size of the segment or section that
    // should contain build information. This is either a specially named section
    // or the first writable non-zero data segment.
    (uint64, uint64) DataStart();
    // DataReader returns an io.ReaderAt that reads from addr until the end
    // of segment or section that contains addr.
    (io.ReaderAt, error) DataReader(uint64 addr);
}

// readRawBuildInfo extracts the Go toolchain version and module information
// strings from a Go binary. On success, vers should be non-empty. mod
// is empty if the binary was not built with modules enabled.
internal static (@string vers, @string mod, error err) readRawBuildInfo(io.ReaderAt r) {
    @string vers = default!;
    @string mod = default!;
    error err = default!;

    // Read the first bytes of the file to identify the format, then delegate to
    // a format-specific function to load segment and section headers.
    var ident = new slice<byte>(16);
    {
        var (n, errΔ1) = r.ReadAt(ident, 0); if (n < len(ident) || errΔ1 != default!) {
            return ("", "", errUnrecognizedFormat);
        }
    }
    exe x = default!;
    switch (ᐧ) {
    case {} when bytes.HasPrefix(ident, slice<byte>(((@string)(new byte[]{0x7f, 0x45, 0x4c, 0x46})))): {
        var (f, errΔ3) = elf.NewFile(r);
        if (errΔ3 != default!) {
            return ("", "", errUnrecognizedFormat);
        }
        x = new elfExeжexe(Ꮡ(new elfExe(f)));
        break;
    }
    case {} when bytes.HasPrefix(ident, slice<byte>("MZ"u8)): {
        var (f, errΔ4) = pe.NewFile(r);
        if (errΔ4 != default!) {
            return ("", "", errUnrecognizedFormat);
        }
        x = new peExeжexe(Ꮡ(new peExe(f)));
        break;
    }
    case {} when bytes.HasPrefix(ident, slice<byte>(((@string)(new byte[]{0xfe, 0xed, 0xfa})))) || bytes.HasPrefix(ident[1..], slice<byte>(((@string)(new byte[]{0xfa, 0xed, 0xfe})))): {
        var (f, errΔ5) = macho.NewFile(r);
        if (errΔ5 != default!) {
            return ("", "", errUnrecognizedFormat);
        }
        x = new machoExeжexe(Ꮡ(new machoExe(f)));
        break;
    }
    case {} when bytes.HasPrefix(ident, slice<byte>(((@string)(new byte[]{0xca, 0xfe, 0xba, 0xbe})))) || bytes.HasPrefix(ident, slice<byte>(((@string)(new byte[]{0xca, 0xfe, 0xba, 0xbf})))): {
        var (f, errΔ6) = macho.NewFatFile(r);
        if (errΔ6 != default! || len((~f).Arches) == 0) {
            return ("", "", errUnrecognizedFormat);
        }
        x = new machoExeжexe(Ꮡ(new machoExe((~f).Arches[0].File)));
        break;
    }
    case {} when bytes.HasPrefix(ident, new byte[]{0x01, 0xDF}.slice()) || bytes.HasPrefix(ident, new byte[]{0x01, 0xF7}.slice()): {
        var (f, errΔ7) = xcoff.NewFile(r);
        if (errΔ7 != default!) {
            return ("", "", errUnrecognizedFormat);
        }
        x = new xcoffExeжexe(Ꮡ(new xcoffExe(f)));
        break;
    }
    case {} when hasPlan9Magic(ident): {
        var (f, errΔ8) = plan9obj.NewFile(r);
        if (errΔ8 != default!) {
            return ("", "", errUnrecognizedFormat);
        }
        x = new plan9objExeжexe(Ꮡ(new plan9objExe(f)));
        break;
    }
    default: {
        return ("", "", errUnrecognizedFormat);
    }}

    // Read segment or section to find the build info blob.
    // On some platforms, the blob will be in its own section, and DataStart
    // returns the address of that section. On others, it's somewhere in the
    // data segment; the linker puts it near the beginning.
    // See cmd/link/internal/ld.Link.buildinfo.
    var (dataAddr, dataSize) = x.DataStart();
    if (dataSize == 0) {
        return ("", "", errNotGoExe);
    }
    (var addr, err) = searchMagic(x, dataAddr, dataSize);
    if (err != default!) {
        return ("", "", err);
    }
    // Read in the full header first.
    (var header, err) = readData(x, addr, buildInfoHeaderSize);
    if (AreEqual(err, io.EOF)){
        return ("", "", errNotGoExe);
    } else 
    if (err != default!) {
        return ("", "", err);
    }
    if (len(header) < buildInfoHeaderSize) {
        return ("", "", errNotGoExe);
    }
    const nint ptrSizeOffset = 14;
    const nint flagsOffset = 15;
    const nint versPtrOffset = 16;
    const byte flagsEndianMask = 0x1;
    UntypedInt flagsEndianLittle = 0x0;
    const byte flagsEndianBig = 0x1;
    const byte flagsVersionMask = 0x2;
    UntypedInt flagsVersionPtr = 0x0;
    const byte flagsVersionInl = 0x2;
    // Decode the blob. The blob is a 32-byte header, optionally followed
    // by 2 varint-prefixed string contents.
    //
    // type buildInfoHeader struct {
    // 	magic       [14]byte
    // 	ptrSize     uint8 // used if flagsVersionPtr
    // 	flags       uint8
    // 	versPtr     targetUintptr // used if flagsVersionPtr
    // 	modPtr      targetUintptr // used if flagsVersionPtr
    // }
    //
    // The version bit of the flags field determines the details of the format.
    //
    // Prior to 1.18, the flags version bit is flagsVersionPtr. In this
    // case, the header includes pointers to the version and modinfo Go
    // strings in the header. The ptrSize field indicates the size of the
    // pointers and the endian bit of the flag indicates the pointer
    // endianness.
    //
    // Since 1.18, the flags version bit is flagsVersionInl. In this case,
    // the header is followed by the string contents inline as
    // length-prefixed (as varint) string contents. First is the version
    // string, followed immediately by the modinfo string.
    var flags = header[flagsOffset];
    if ((byte)(flags & flagsVersionMask) == flagsVersionInl){
        (vers, addr, err) = decodeString(x, addr + (uint64)buildInfoHeaderSize);
        if (err != default!) {
            return ("", "", err);
        }
        (mod, _, err) = decodeString(x, addr);
        if (err != default!) {
            return ("", "", err);
        }
    } else {
        // flagsVersionPtr (<1.18)
        nint ptrSize = (nint)header[ptrSizeOffset];
        var bigEndian = (byte)(flags & flagsEndianMask) == flagsEndianBig;
        binary.ByteOrder bo = default!;
        if (bigEndian){
            bo = binary.BigEndian;
        } else {
            bo = binary.LittleEndian;
        }
        Func<slice<byte>, uint64> readPtr = default!;
        if (ptrSize == 4){
            var boʗ1 = bo;
            readPtr = (slice<byte> b) => (uint64)boʗ1.Uint32(b);
        } else 
        if (ptrSize == 8){
            var boʗ2 = bo;
                        readPtr = boʗ2.Uint64;
        } else {
            return ("", "", errNotGoExe);
        }
        vers = readString(x, ptrSize, readPtr, readPtr(header[(int)(versPtrOffset)..]));
        mod = readString(x, ptrSize, readPtr, readPtr(header.slice(versPtrOffset + ptrSize)));
    }
    if (vers == ""u8) {
        return ("", "", errNotGoExe);
    }
    if (len(mod) >= 33 && mod[len(mod) - 17] == (rune)'\n'){
        // Strip module framing: sentinel strings delimiting the module info.
        // These are cmd/go/internal/modload.infoStart and infoEnd.
        mod = mod.slice(16, len(mod) - 16);
    } else {
        mod = ""u8;
    }
    return (vers, mod, default!);
}

internal static bool hasPlan9Magic(slice<byte> magic) {
    if (len(magic) >= 4) {
        var m = binary.BigEndian.Uint32(magic);
        var exprᴛ1 = m;
        if (exprᴛ1 == plan9obj.Magic386 || exprᴛ1 == plan9obj.MagicAMD64 || exprᴛ1 == plan9obj.MagicARM) {
            return true;
        }

    }
    return false;
}

internal static (@string, uint64, error) decodeString(exe x, uint64 addr) {
    // varint length followed by length bytes of data.
    // N.B. ReadData reads _up to_ size bytes from the section containing
    // addr. So we don't need to check that size doesn't overflow the
    // section.
    var (b, err) = readData(x, addr, binary.MaxVarintLen64);
    if (AreEqual(err, io.EOF)){
        return ("", 0, errNotGoExe);
    } else 
    if (err != default!) {
        return ("", 0, err);
    }
    var (length, n) = binary.Uvarint(b);
    if (n <= 0) {
        return ("", 0, errNotGoExe);
    }
    addr += (uint64)n;
    (b, err) = readData(x, addr, length);
    if (AreEqual(err, io.EOF)){
        return ("", 0, errNotGoExe);
    } else 
    if (AreEqual(err, io.ErrUnexpectedEOF)){
        // Length too large to allocate. Clearly bogus value.
        return ("", 0, errNotGoExe);
    } else 
    if (err != default!) {
        return ("", 0, err);
    }
    if ((uint64)len(b) < length) {
        // Section ended before we could read the full string.
        return ("", 0, errNotGoExe);
    }
    return (((@string)b), addr + length, default!);
}

// readString returns the string at address addr in the executable x.
internal static @string readString(exe x, nint ptrSize, Func<slice<byte>, uint64> readPtr, uint64 addr) {
    var (hdr, err) = readData(x, addr, (uint64)(2 * ptrSize));
    if (err != default! || len(hdr) < 2 * ptrSize) {
        return ""u8;
    }
    var dataAddr = readPtr(hdr);
    var dataLen = readPtr(hdr.slice(ptrSize));
    (var data, err) = readData(x, dataAddr, dataLen);
    if (err != default! || (uint64)len(data) < dataLen) {
        return ""u8;
    }
    return ((@string)data);
}

internal static UntypedInt searchChunkSize => /* 1 << 20 */ 1048576; // 1 MB

// searchMagic returns the aligned first instance of buildInfoMagic in the data
// range [addr, addr+size). Returns false if not found.
internal static (uint64, error) searchMagic(exe x, uint64 start, uint64 size) {
    var end = start + size;
    if (end < start) {
        // Overflow.
        return (0, errUnrecognizedFormat);
    }
    // Round up start; magic can't occur in the initial unaligned portion.
    start = (uint64)((start + (uint64)buildInfoAlign - 1) & ~(uint64)((buildInfoAlign - 1)));
    if (start >= end) {
        return (0, errNotGoExe);
    }
    slice<byte> buf = default!;
    while (start < end) {
        // Read in chunks to avoid consuming too much memory if data is large.
        //
        // Normally it would be somewhat painful to handle the magic crossing a
        // chunk boundary, but since it must be 16-byte aligned we know it will
        // fall within a single chunk.
        var remaining = end - start;
        var chunkSize = (uint64)searchChunkSize;
        if (chunkSize > remaining) {
            chunkSize = remaining;
        }
        if (buf == default!){
            buf = new slice<byte>((nint)(chunkSize));
        } else {
            // N.B. chunkSize can only decrease, and only on the
            // last chunk.
            buf = buf.slice(0, (nint)(chunkSize));
            clear(buf);
        }
        var (n, err) = readDataInto(x, start, buf);
        if (AreEqual(err, io.EOF)){
            // EOF before finding the magic; must not be a Go executable.
            return (0, errNotGoExe);
        } else 
        if (err != default!) {
            return (0, err);
        }
        var data = buf.slice(0, n);
        while (len(data) > 0) {
            nint i = bytes.Index(data, buildInfoMagic);
            if (i < 0) {
                break;
            }
            if (remaining - (uint64)i < buildInfoHeaderSize) {
                // Found magic, but not enough space left for the full header.
                return (0, errNotGoExe);
            }
            if (i % (nint)buildInfoAlign != 0) {
                // Found magic, but misaligned. Keep searching.
                nint next = (nint)((i + (nint)buildInfoAlign - 1) & ~(nint)(buildInfoAlign - 1));
                if (next > len(data)) {
                    // Corrupt object file: the remaining
                    // count says there is more data,
                    // but we didn't read it.
                    return (0, errNotGoExe);
                }
                data = data.slice(next);
                continue;
            }
            // Good match!
            return (start + (uint64)i, default!);
        }
        start += chunkSize;
    }
    return (0, errNotGoExe);
}

internal static (slice<byte>, error) readData(exe x, uint64 addr, uint64 size) {
    var (r, err) = x.DataReader(addr);
    if (err != default!) {
        return (default!, err);
    }
    (var b, err) = saferio.ReadDataAt(r, size, 0);
    if (len(b) > 0 && AreEqual(err, io.EOF)) {
        err = default!;
    }
    return (b, err);
}

internal static (nint, error) readDataInto(exe x, uint64 addr, slice<byte> b) {
    var (r, err) = x.DataReader(addr);
    if (err != default!) {
        return (0, err);
    }
    (var n, err) = r.ReadAt(b, 0);
    if (n > 0 && AreEqual(err, io.EOF)) {
        err = default!;
    }
    return (n, err);
}

// elfExe is the ELF implementation of the exe interface.
[GoType] partial struct elfExe {
    internal ж<elf.File> f;
}

[GoRecv] internal static (io.ReaderAt, error) DataReader(this ref elfExe x, uint64 addr) {
    foreach (var (_, prog) in (~x.f).Progs) {
        if ((~prog).Vaddr <= addr && addr <= (~prog).Vaddr + (~prog).Filesz - 1) {
            var remaining = (~prog).Vaddr + (~prog).Filesz - addr;
            return (new io.SectionReaderжReaderAt(io.NewSectionReader(new elf_ProgжReaderAt(prog), (int64)(addr - (~prog).Vaddr), (int64)remaining)), default!);
        }
    }
    return (default!, errUnrecognizedFormat);
}

[GoRecv] internal static (uint64, uint64) DataStart(this ref elfExe x) {
    foreach (var (_, s) in (~x.f).Sections) {
        if ((~s).Name == ".go.buildinfo"u8) {
            return ((~s).Addr, (~s).Size);
        }
    }
    foreach (var (_, p) in (~x.f).Progs) {
        if ((~p).Type == elf.PT_LOAD && (elf.ProgFlag)((~p).Flags & ((elf.ProgFlag)(elf.PF_X | elf.PF_W))) == elf.PF_W) {
            return ((~p).Vaddr, (~p).Memsz);
        }
    }
    return (0, 0);
}

// peExe is the PE (Windows Portable Executable) implementation of the exe interface.
[GoType] partial struct peExe {
    internal ж<pe.File> f;
}

[GoRecv] internal static uint64 imageBase(this ref peExe x) {
    switch ((~x.f).OptionalHeader.type()) {
    case ж<pe.OptionalHeader32> oh: {
        return (uint64)(~oh).ImageBase;
    }
    case ж<pe.OptionalHeader64> oh: {
        return (~oh).ImageBase;
    }}
    return 0;
}

[GoRecv] internal static (io.ReaderAt, error) DataReader(this ref peExe x, uint64 addr) {
    addr -= x.imageBase();
    foreach (var (_, sect) in (~x.f).Sections) {
        if ((uint64)(~sect).VirtualAddress <= addr && addr <= (uint64)((~sect).VirtualAddress + (~sect).Size - 1)) {
            var remaining = (uint64)((~sect).VirtualAddress + (~sect).Size) - addr;
            return (new io.SectionReaderжReaderAt(io.NewSectionReader(new pe_ΔSectionжReaderAt(sect), (int64)(addr - (uint64)(~sect).VirtualAddress), (int64)remaining)), default!);
        }
    }
    return (default!, errUnrecognizedFormat);
}

[GoRecv] internal static (uint64, uint64) DataStart(this ref peExe x) {
    // Assume data is first writable section.
    UntypedInt IMAGE_SCN_CNT_CODE = 0x00000020;
    
    UntypedInt IMAGE_SCN_CNT_INITIALIZED_DATA = 0x00000040;
    
    UntypedInt IMAGE_SCN_CNT_UNINITIALIZED_DATA = 0x00000080;
    
    UntypedInt IMAGE_SCN_MEM_EXECUTE = 0x20000000;
    
    UntypedInt IMAGE_SCN_MEM_READ = 0x40000000;
    
    UntypedInt IMAGE_SCN_MEM_WRITE = 0x80000000;
    
    UntypedInt IMAGE_SCN_MEM_DISCARDABLE = 0x2000000;
    
    UntypedInt IMAGE_SCN_LNK_NRELOC_OVFL = 0x1000000;
    
    const uint32 IMAGE_SCN_ALIGN_32BYTES = 0x600000;
    foreach (var (_, sect) in (~x.f).Sections) {
        if ((~sect).VirtualAddress != 0 && (~sect).Size != 0 && (uint32)((~sect).Characteristics & ~IMAGE_SCN_ALIGN_32BYTES) == (uint32)((UntypedInt)(IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ) | (uint32)IMAGE_SCN_MEM_WRITE)) {
            return ((uint64)(~sect).VirtualAddress + x.imageBase(), (uint64)(~sect).VirtualSize);
        }
    }
    return (0, 0);
}

// machoExe is the Mach-O (Apple macOS/iOS) implementation of the exe interface.
[GoType] partial struct machoExe {
    internal ж<macho.File> f;
}

[GoRecv] internal static (io.ReaderAt, error) DataReader(this ref machoExe x, uint64 addr) {
    foreach (var (_, load) in (~x.f).Loads) {
        var (seg, ok) = load._<ж<machoꓸSegment>>(ᐧ);
        if (!ok) {
            continue;
        }
        if ((~seg).Addr <= addr && addr <= (~seg).Addr + (~seg).Filesz - 1) {
            if ((~seg).Name == "__PAGEZERO"u8) {
                continue;
            }
            var remaining = (~seg).Addr + (~seg).Filesz - addr;
            return (new io.SectionReaderжReaderAt(io.NewSectionReader(new macho_ΔSegmentжReaderAt(seg), (int64)(addr - (~seg).Addr), (int64)remaining)), default!);
        }
    }
    return (default!, errUnrecognizedFormat);
}

[GoRecv] internal static (uint64, uint64) DataStart(this ref machoExe x) {
    // Look for section named "__go_buildinfo".
    foreach (var (_, sec) in (~x.f).Sections) {
        if ((~sec).Name == "__go_buildinfo"u8) {
            return ((~sec).Addr, (~sec).Size);
        }
    }
    // Try the first non-empty writable segment.
    const uint32 RW = 3;
    foreach (var (_, load) in (~x.f).Loads) {
        var (seg, ok) = load._<ж<machoꓸSegment>>(ᐧ);
        if (ok && (~seg).Addr != 0 && (~seg).Filesz != 0 && (~seg).Prot == RW && (~seg).Maxprot == RW) {
            return ((~seg).Addr, (~seg).Memsz);
        }
    }
    return (0, 0);
}

// xcoffExe is the XCOFF (AIX eXtended COFF) implementation of the exe interface.
[GoType] partial struct xcoffExe {
    internal ж<xcoff.File> f;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addressNotMappedˢ = "address not mapped"u8;

[GoRecv] internal static (io.ReaderAt, error) DataReader(this ref xcoffExe x, uint64 addr) {
    foreach (var (_, sect) in (~x.f).Sections) {
        if ((~sect).VirtualAddress <= addr && addr <= (~sect).VirtualAddress + (~sect).Size - 1) {
            var remaining = (~sect).VirtualAddress + (~sect).Size - addr;
            return (new io.SectionReaderжReaderAt(io.NewSectionReader(new xcoff_ΔSectionжReaderAt(sect), (int64)(addr - (~sect).VirtualAddress), (int64)remaining)), default!);
        }
    }
    return (default!, errors.New(addressNotMappedˢ));
}

[GoRecv] internal static (uint64, uint64) DataStart(this ref xcoffExe x) {
    {
        var s = x.f.SectionByType(xcoff.STYP_DATA); if (s != nil) {
            return ((~s).VirtualAddress, (~s).Size);
        }
    }
    return (0, 0);
}

// plan9objExe is the Plan 9 a.out implementation of the exe interface.
[GoType] partial struct plan9objExe {
    internal ж<plan9obj.File> f;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dataˢ = "data"u8;

[GoRecv] internal static (uint64, uint64) DataStart(this ref plan9objExe x) {
    {
        var s = x.f.Section(dataˢ); if (s != nil) {
            return ((uint64)(~s).Offset, (uint64)(~s).Size);
        }
    }
    return (0, 0);
}

[GoRecv] internal static (io.ReaderAt, error) DataReader(this ref plan9objExe x, uint64 addr) {
    foreach (var (_, sect) in (~x.f).Sections) {
        if ((uint64)(~sect).Offset <= addr && addr <= (uint64)((~sect).Offset + (~sect).Size - 1)) {
            var remaining = (uint64)((~sect).Offset + (~sect).Size) - addr;
            return (new io.SectionReaderжReaderAt(io.NewSectionReader(new plan9obj_ΔSectionжReaderAt(sect), (int64)(addr - (uint64)(~sect).Offset), (int64)remaining)), default!);
        }
    }
    return (default!, errors.New(addressNotMappedˢ));
}

} // end buildinfo_package

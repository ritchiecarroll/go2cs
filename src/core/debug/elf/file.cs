// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package elf implements access to ELF object files.

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
using saferio = @internal.saferio_package;
using zstd = @internal.zstd_package;
using io = io_package;
using os = os_package;
using strings = strings_package;
using @unsafe = unsafe_package;
using @internal;
using compress;
using encoding;
using go.debug;

partial class elf_package {

// TODO: error reporting detail
/*
 * Internal ELF representation
 */

// A FileHeader represents an ELF file header.
[GoType] partial struct FileHeader {
    public Class Class;
    public ΔData Data;
    public Version Version;
    public OSABI OSABI;
    public uint8 ABIVersion;
    public binary.ByteOrder ByteOrder;
    public Type Type;
    public Machine Machine;
    public uint64 Entry;
}

// A File represents an open ELF file.
[GoType] partial struct File {
    public partial ref FileHeader FileHeader { get; }
    public slice<ж<ΔSection>> Sections;
    public slice<ж<Prog>> Progs;
    internal io.Closer closer;
    internal slice<DynamicVersion> dynVers;
    internal slice<DynamicVersionNeed> dynVerNeeds;
    internal slice<byte> gnuVersym;
}

// A SectionHeader represents a single ELF section header.
[GoType] partial struct SectionHeader {
    public @string Name;
    public SectionType Type;
    public SectionFlag Flags;
    public uint64 Addr;
    public uint64 Offset;
    public uint64 Size;
    public uint32 Link;
    public uint32 Info;
    public uint64 Addralign;
    public uint64 Entsize;
    // FileSize is the size of this section in the file in bytes.
    // If a section is compressed, FileSize is the size of the
    // compressed data, while Size (above) is the size of the
    // uncompressed data.
    public uint64 FileSize;
}

// A Section represents a single section in an ELF file.
[GoType] partial struct ΔSection {
    public partial ref SectionHeader SectionHeader { get; }
    // Embed ReaderAt for ReadAt method.
    // Do not embed SectionReader directly
    // to avoid having Read and Seek.
    // If a client wants Read and Seek it must use
    // Open() to avoid fighting over the seek offset
    // with other clients.
    //
    // ReaderAt may be nil if the section is not easily available
    // in a random-access form. For example, a compressed section
    // may have a nil ReaderAt.
    public io_package.ReaderAt ReaderAt;
    internal ж<io.SectionReader> sr;
    internal CompressionType compressionType;
    internal int64 compressionOffset;
}

// Data reads and returns the contents of the ELF section.
// Even if the section is stored compressed in the ELF file,
// Data returns uncompressed data.
//
// For an [SHT_NOBITS] section, Data always returns a non-nil error.
public static (slice<byte>, error) Data(this ж<ΔSection> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    return saferio.ReadData(new io_ReadSeekerᴠReader(Ꮡs.Open()), s.Size);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sectionHasInvalidStringˢ = "section has invalid string table link"u8;

// stringTable reads and returns the string table given by the
// specified link value.
[GoRecv] internal static (slice<byte>, error) stringTable(this ref File f, uint32 link) {
    if (link <= 0 || link >= (uint32)len(f.Sections)) {
        return (default!, errors.New(sectionHasInvalidStringˢ));
    }
    return f.Sections[(nint)(link)].Data();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zdebugˢ = ".zdebug"u8;

// Open returns a new ReadSeeker reading the ELF section.
// Even if the section is stored compressed in the ELF file,
// the ReadSeeker reads uncompressed data.
//
// For an [SHT_NOBITS] section, all calls to the opened reader
// will return a non-nil error.
public static io.ReadSeeker Open(this ж<ΔSection> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (s.Type == SHT_NOBITS) {
        return new io.SectionReaderжReadSeeker(io.NewSectionReader(new nobitsSectionReaderжReaderAt(Ꮡ(new nobitsSectionReader(nil))), 0, (int64)s.Size));
    }
    Func<io.Reader, (io.ReadCloser, error)> zrd = default!;
    if ((SectionFlag)(s.Flags & SHF_COMPRESSED) == 0){
        if (!strings.HasPrefix(s.Name, zdebugˢ)) {
            return new io.SectionReaderжReadSeeker(io.NewSectionReader(new io.SectionReaderжReaderAt(s.sr), 0, 9223372036854775807L));
        }
        var b = new slice<byte>(12);
        var (n, _) = s.sr.ReadAt(b, 0);
        if (n != 12 || ((sstring)(b[..4])) != "ZLIB"u8) {
            return new io.SectionReaderжReadSeeker(io.NewSectionReader(new io.SectionReaderжReaderAt(s.sr), 0, 9223372036854775807L));
        }
        s.compressionOffset = 12;
        s.compressionType = COMPRESS_ZLIB;
        s.Size = binary.BigEndian.Uint64(b[4..12]);
        zrd = zlib.NewReader;
    } else 
    if ((SectionFlag)(s.Flags & SHF_ALLOC) != 0) {
        return new errorReader(new FormatErrorжerror(Ꮡ(new FormatError((int64)s.Offset,
            "SHF_COMPRESSED applies only to non-allocable sections"u8, s.compressionType)))
        );
    }
    var exprᴛ1 = s.compressionType;
    if (exprᴛ1 == COMPRESS_ZLIB) {
        zrd = zlib.NewReader;
    }
    else if (exprᴛ1 == COMPRESS_ZSTD) {
        zrd = (io.ReadCloser, error) (io.Reader r) => (io.NopCloser(new zstd_ReaderжReader(zstd.NewReader(r))), default!);
    }

    if (zrd == default!) {
        return new errorReader(new FormatErrorжerror(Ꮡ(new FormatError((int64)s.Offset, "unknown compression type"u8, s.compressionType))));
    }
        var zrdʗ1 = zrd;
    return new readSeekerFromReaderжReadSeeker(Ꮡ(new readSeekerFromReader(
        reset: () => {
            var fr = io.NewSectionReader(new io.SectionReaderжReaderAt(Ꮡs.Value.sr), Ꮡs.Value.compressionOffset, (int64)Ꮡs.Value.FileSize - Ꮡs.Value.compressionOffset);
            return zrdʗ1(new io.SectionReaderжReader(fr));
        },
        size: (int64)s.Size
    )));
}

// A ProgHeader represents a single ELF program header.
[GoType] partial struct ProgHeader {
    public ProgType Type;
    public ProgFlag Flags;
    public uint64 Off;
    public uint64 Vaddr;
    public uint64 Paddr;
    public uint64 Filesz;
    public uint64 Memsz;
    public uint64 Align;
}

// A Prog represents a single ELF program header in an ELF binary.
[GoType] partial struct Prog {
    public partial ref ProgHeader ProgHeader { get; }
    // Embed ReaderAt for ReadAt method.
    // Do not embed SectionReader directly
    // to avoid having Read and Seek.
    // If a client wants Read and Seek it must use
    // Open() to avoid fighting over the seek offset
    // with other clients.
    public io_package.ReaderAt ReaderAt;
    internal ж<io.SectionReader> sr;
}

// Open returns a new ReadSeeker reading the ELF program body.
[GoRecv] public static io.ReadSeeker Open(this ref Prog p) {
    return new io.SectionReaderжReadSeeker(io.NewSectionReader(new io.SectionReaderжReaderAt(p.sr), 0, 9223372036854775807L));
}

// A Symbol represents an entry in an ELF symbol table section.
[GoType] partial struct Symbol {
    public @string Name;
    public byte Info, Other;
    // HasVersion reports whether the symbol has any version information.
    // This will only be true for the dynamic symbol table.
    public bool HasVersion;
    // VersionIndex is the symbol's version index.
    // Use the methods of the [VersionIndex] type to access it.
    // This field is only meaningful if HasVersion is true.
    public VersionIndex VersionIndex;
    public SectionIndex Section;
    public uint64 Value, Size;
    // These fields are present only for the dynamic symbol table.
    public @string Version;
    public @string Library;
}

/*
 * ELF reader
 */
[GoType] partial struct FormatError {
    internal int64 off;
    internal @string msg;
    internal any val;
}

[GoRecv] public static @string Error(this ref FormatError e) {
    @string msg = e.msg;
    if (e.val != default!) {
        msg += fmt.Sprintf(" '%v' "u8, e.val);
    }
    msg += fmt.Sprintf("in record at byte %#x"u8, e.off);
    return msg;
}

// Open opens the named file using [os.Open] and prepares it for use as an ELF binary.
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
[GoRecv] public static error Close(this ref File f) {
    error err = default!;
    if (f.closer != default!) {
        err = f.closer.Close();
        f.closer = default!;
    }
    return err;
}

// SectionByType returns the first section in f with the
// given type, or nil if there is no such section.
[GoRecv] public static ж<ΔSection> SectionByType(this ref File f, SectionType typ) {
    foreach (var (_, s) in f.Sections) {
        if ((~s).Type == typ) {
            return s;
        }
    }
    return default!;
}

// NewFile creates a new [File] for accessing an ELF binary in an underlying reader.
// The ELF binary is expected to start at position 0 in the ReaderAt.
public static (ж<File>, error) NewFile(io.ReaderAt r) {
    var sr = io.NewSectionReader(r, 0, 9223372036854775807L);
    // Read and decode ELF identifier
    array<uint8> ident = new(16);
    {
        var (_, errΔ1) = r.ReadAt(ident[0..], 0); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    if (ident[0] != (rune)'\x7f' || ident[1] != (rune)'E' || ident[2] != (rune)'L' || ident[3] != (rune)'F') {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "bad magic number"u8, ident[0..4]))));
    }
    var f = @new<File>();
    f.Value.Class = ((Class)ident[EI_CLASS]);
    var exprᴛ1 = (~f).Class;
    if (exprᴛ1 == ELFCLASS32) {
    }
    else if (exprᴛ1 == ELFCLASS64) {
    }
    else { /* default: */
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError( // ok
0, "unknown ELF class"u8, (~f).Class))));
    }

    f.Value.Data = ((ΔData)ident[EI_DATA]);
    binary.ByteOrder bo = default!;
    var exprᴛ2 = (~f).Data;
    if (exprᴛ2 == ELFDATA2LSB) {
        bo = binary.LittleEndian;
    }
    else if (exprᴛ2 == ELFDATA2MSB) {
        bo = binary.BigEndian;
    }
    else { /* default: */
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "unknown ELF data encoding"u8, (~f).Data))));
    }

    f.Value.ByteOrder = bo;
    f.Value.Version = ((Version)ident[EI_VERSION]);
    if ((~f).Version != EV_CURRENT) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "unknown ELF version"u8, (~f).Version))));
    }
    f.Value.OSABI = ((OSABI)ident[EI_OSABI]);
    f.Value.ABIVersion = ident[EI_ABIVERSION];
    // Read ELF file header
    ref var phoff = ref heap(new int64(), out var Ꮡphoff);
    ref var phentsize = ref heap(new nint(), out var Ꮡphentsize);
    nint phnum = default!;
    ref var shoff = ref heap(new int64(), out var Ꮡshoff);
    ref var shentsize = ref heap(new nint(), out var Ꮡshentsize);
    ref var shnum = ref heap(new nint(), out var Ꮡshnum);
    ref var shstrndx = ref heap(new nint(), out var Ꮡshstrndx);
    var exprᴛ3 = (~f).Class;
    if (exprᴛ3 == ELFCLASS32) {
        Header32 hdr = new();
        var data = new slice<byte>((nint)(/* unsafe.Sizeof(hdr) */ (uintptr)52));
        {
            var (_, errΔ4) = sr.ReadAt(data, 0); if (errΔ4 != default!) {
                return (default!, errΔ4);
            }
        }
        f.Value.Type = ((Type)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Type) */ (uintptr)16)..]));
        f.Value.Machine = ((Machine)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Machine) */ (uintptr)18)..]));
        f.Value.Entry = (uint64)bo.Uint32(data[(int)(/* unsafe.Offsetof(hdr.Entry) */ (uintptr)24)..]);
        {
            ref var v = ref heap<Version>(out var Ꮡv);
            v = ((Version)(byte)bo.Uint32(data[(int)(/* unsafe.Offsetof(hdr.Version) */ (uintptr)20)..])); if (v != (~f).Version) {
                return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "mismatched ELF version"u8, v))));
            }
        }
        phoff = (int64)bo.Uint32(data[(int)(/* unsafe.Offsetof(hdr.Phoff) */ (uintptr)28)..]);
        phentsize = (nint)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Phentsize) */ (uintptr)42)..]);
        phnum = (nint)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Phnum) */ (uintptr)44)..]);
        shoff = (int64)bo.Uint32(data[(int)(/* unsafe.Offsetof(hdr.Shoff) */ (uintptr)32)..]);
        shentsize = (nint)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Shentsize) */ (uintptr)46)..]);
        shnum = (nint)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Shnum) */ (uintptr)48)..]);
        shstrndx = (nint)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Shstrndx) */ (uintptr)50)..]);
    }
    else if (exprᴛ3 == ELFCLASS64) {
        Header64 hdr = new();
        var data = new slice<byte>((nint)(/* unsafe.Sizeof(hdr) */ (uintptr)64));
        {
            var (_, errΔ5) = sr.ReadAt(data, 0); if (errΔ5 != default!) {
                return (default!, errΔ5);
            }
        }
        f.Value.Type = ((Type)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Type) */ (uintptr)16)..]));
        f.Value.Machine = ((Machine)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Machine) */ (uintptr)18)..]));
        f.Value.Entry = bo.Uint64(data[(int)(/* unsafe.Offsetof(hdr.Entry) */ (uintptr)24)..]);
        {
            ref var v = ref heap<Version>(out var Ꮡv);
            v = ((Version)(byte)bo.Uint32(data[(int)(/* unsafe.Offsetof(hdr.Version) */ (uintptr)20)..])); if (v != (~f).Version) {
                return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "mismatched ELF version"u8, v))));
            }
        }
        phoff = (int64)bo.Uint64(data[(int)(/* unsafe.Offsetof(hdr.Phoff) */ (uintptr)32)..]);
        phentsize = (nint)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Phentsize) */ (uintptr)54)..]);
        phnum = (nint)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Phnum) */ (uintptr)56)..]);
        shoff = (int64)bo.Uint64(data[(int)(/* unsafe.Offsetof(hdr.Shoff) */ (uintptr)40)..]);
        shentsize = (nint)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Shentsize) */ (uintptr)58)..]);
        shnum = (nint)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Shnum) */ (uintptr)60)..]);
        shstrndx = (nint)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Shstrndx) */ (uintptr)62)..]);
    }

    if (shoff < 0) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "invalid shoff"u8, shoff))));
    }
    if (phoff < 0) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "invalid phoff"u8, phoff))));
    }
    if (shoff == 0 && shnum != 0) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "invalid ELF shnum for shoff=0"u8, shnum))));
    }
    if (shnum > 0 && shstrndx >= shnum) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "invalid ELF shstrndx"u8, shstrndx))));
    }
    nint wantPhentsize = default!;
    nint wantShentsize = default!;
    var exprᴛ4 = (~f).Class;
    if (exprᴛ4 == ELFCLASS32) {
        wantPhentsize = 8 * 4;
        wantShentsize = 10 * 4;
    }
    else if (exprᴛ4 == ELFCLASS64) {
        wantPhentsize = 2 * 4 + 6 * 8;
        wantShentsize = 4 * 4 + 6 * 8;
    }

    if (phnum > 0 && phentsize < wantPhentsize) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "invalid ELF phentsize"u8, phentsize))));
    }
    // Read program headers
    f.Value.Progs = new slice<ж<Prog>>(phnum);
    var (phdata, err) = saferio.ReadDataAt(new io.SectionReaderжReaderAt(sr), (uint64)phnum * (uint64)phentsize, phoff);
    if (err != default!) {
        return (default!, err);
    }
    for (nint i = 0; i < phnum; i++) {
        var off = (uintptr)i * (uintptr)phentsize;
        var p = @new<Prog>();
        var exprᴛ5 = (~f).Class;
        if (exprᴛ5 == ELFCLASS32) {
            Prog32 ph = default!;
            p.Value.ProgHeader = new ProgHeader(
                Type: ((ProgType)(nint)bo.Uint32(phdata[(int)(off + /* unsafe.Offsetof(ph.Type) */ (uintptr)0)..])),
                Flags: ((ProgFlag)bo.Uint32(phdata[(int)(off + /* unsafe.Offsetof(ph.Flags) */ (uintptr)24)..])),
                Off: (uint64)bo.Uint32(phdata[(int)(off + /* unsafe.Offsetof(ph.Off) */ (uintptr)4)..]),
                Vaddr: (uint64)bo.Uint32(phdata[(int)(off + /* unsafe.Offsetof(ph.Vaddr) */ (uintptr)8)..]),
                Paddr: (uint64)bo.Uint32(phdata[(int)(off + /* unsafe.Offsetof(ph.Paddr) */ (uintptr)12)..]),
                Filesz: (uint64)bo.Uint32(phdata[(int)(off + /* unsafe.Offsetof(ph.Filesz) */ (uintptr)16)..]),
                Memsz: (uint64)bo.Uint32(phdata[(int)(off + /* unsafe.Offsetof(ph.Memsz) */ (uintptr)20)..]),
                Align: (uint64)bo.Uint32(phdata[(int)(off + /* unsafe.Offsetof(ph.Align) */ (uintptr)28)..])
            );
        }
        else if (exprᴛ5 == ELFCLASS64) {
            Prog64 ph = default!;
            p.Value.ProgHeader = new ProgHeader(
                Type: ((ProgType)(nint)bo.Uint32(phdata[(int)(off + /* unsafe.Offsetof(ph.Type) */ (uintptr)0)..])),
                Flags: ((ProgFlag)bo.Uint32(phdata[(int)(off + /* unsafe.Offsetof(ph.Flags) */ (uintptr)4)..])),
                Off: bo.Uint64(phdata[(int)(off + /* unsafe.Offsetof(ph.Off) */ (uintptr)8)..]),
                Vaddr: bo.Uint64(phdata[(int)(off + /* unsafe.Offsetof(ph.Vaddr) */ (uintptr)16)..]),
                Paddr: bo.Uint64(phdata[(int)(off + /* unsafe.Offsetof(ph.Paddr) */ (uintptr)24)..]),
                Filesz: bo.Uint64(phdata[(int)(off + /* unsafe.Offsetof(ph.Filesz) */ (uintptr)32)..]),
                Memsz: bo.Uint64(phdata[(int)(off + /* unsafe.Offsetof(ph.Memsz) */ (uintptr)40)..]),
                Align: bo.Uint64(phdata[(int)(off + /* unsafe.Offsetof(ph.Align) */ (uintptr)48)..])
            );
        }

        if ((int64)(~p).Off < 0) {
            return (default!, new FormatErrorжerror(Ꮡ(new FormatError(phoff + (int64)off, "invalid program header offset"u8, (~p).Off))));
        }
        if ((int64)(~p).Filesz < 0) {
            return (default!, new FormatErrorжerror(Ꮡ(new FormatError(phoff + (int64)off, "invalid program header file size"u8, (~p).Filesz))));
        }
        p.Value.sr = io.NewSectionReader(r, (int64)(~p).Off, (int64)(~p).Filesz);
        p.Value.ReaderAt = new io.SectionReaderжReaderAt(p.Value.sr);
        f.Value.Progs[i] = p;
    }
    // If the number of sections is greater than or equal to SHN_LORESERVE
    // (0xff00), shnum has the value zero and the actual number of section
    // header table entries is contained in the sh_size field of the section
    // header at index 0.
    if (shoff > 0 && shnum == 0) {
        uint32 typ = default!;
        uint32 link = default!;
        sr.Seek(shoff, io.SeekStart);
        var exprᴛ6 = (~f).Class;
        if (exprᴛ6 == ELFCLASS32) {
            var sh = @new<Section32>();
            {
                var errΔ8 = binary.Read(new io.SectionReaderжReader(sr), bo, sh.OrTypedNil()); if (errΔ8 != default!) {
                    return (default!, errΔ8);
                }
            }
            shnum = (nint)(~sh).Size;
            typ = sh.Value.Type;
            link = sh.Value.Link;
        }
        else if (exprᴛ6 == ELFCLASS64) {
            var sh = @new<Section64>();
            {
                var errΔ9 = binary.Read(new io.SectionReaderжReader(sr), bo, sh.OrTypedNil()); if (errΔ9 != default!) {
                    return (default!, errΔ9);
                }
            }
            shnum = (nint)(~sh).Size;
            typ = sh.Value.Type;
            link = sh.Value.Link;
        }

        if (((SectionType)typ) != SHT_NULL) {
            return (default!, new FormatErrorжerror(Ꮡ(new FormatError(shoff, "invalid type of the initial section"u8, ((SectionType)typ)))));
        }
        if (shnum < (nint)SHN_LORESERVE) {
            return (default!, new FormatErrorжerror(Ꮡ(new FormatError(shoff, "invalid ELF shnum contained in sh_size"u8, shnum))));
        }
        // If the section name string table section index is greater than or
        // equal to SHN_LORESERVE (0xff00), this member has the value
        // SHN_XINDEX (0xffff) and the actual index of the section name
        // string table section is contained in the sh_link field of the
        // section header at index 0.
        if (shstrndx == (nint)SHN_XINDEX) {
            shstrndx = (nint)link;
            if (shstrndx < (nint)SHN_LORESERVE) {
                return (default!, new FormatErrorжerror(Ꮡ(new FormatError(shoff, "invalid ELF shstrndx contained in sh_link"u8, shstrndx))));
            }
        }
    }
    if (shnum > 0 && shentsize < wantShentsize) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "invalid ELF shentsize"u8, shentsize))));
    }
    // Read section headers
    nint c = saferio.SliceCap<ΔSection>((uint64)shnum);
    if (c < 0) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "too many sections"u8, shnum))));
    }
    if (shnum > 0 && (18446744073709551615UL) / (uint64)shnum < (uint64)shentsize) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "section header overflow"u8, shnum))));
    }
    f.Value.Sections = new slice<ж<ΔSection>>(0, c);
    var names = new slice<uint32>(0, c);
    (var shdata, err) = saferio.ReadDataAt(new io.SectionReaderжReaderAt(sr), (uint64)shnum * (uint64)shentsize, shoff);
    if (err != default!) {
        return (default!, err);
    }
    for (nint i = 0; i < shnum; i++) {
        var off = (uintptr)i * (uintptr)shentsize;
        var s = @new<ΔSection>();
        var exprᴛ7 = (~f).Class;
        if (exprᴛ7 == ELFCLASS32) {
            Section32 sh = default!;
            names = append(names, bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Name) */ (uintptr)0)..]));
            s.Value.SectionHeader = new SectionHeader(
                Type: ((SectionType)bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Type) */ (uintptr)4)..])),
                Flags: ((SectionFlag)bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Flags) */ (uintptr)8)..])),
                Addr: (uint64)bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Addr) */ (uintptr)12)..]),
                Offset: (uint64)bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Off) */ (uintptr)16)..]),
                FileSize: (uint64)bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Size) */ (uintptr)20)..]),
                Link: bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Link) */ (uintptr)24)..]),
                Info: bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Info) */ (uintptr)28)..]),
                Addralign: (uint64)bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Addralign) */ (uintptr)32)..]),
                Entsize: (uint64)bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Entsize) */ (uintptr)36)..])
            );
        }
        else if (exprᴛ7 == ELFCLASS64) {
            Section64 sh = default!;
            names = append(names, bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Name) */ (uintptr)0)..]));
            s.Value.SectionHeader = new SectionHeader(
                Type: ((SectionType)bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Type) */ (uintptr)4)..])),
                Flags: ((SectionFlag)(uint32)bo.Uint64(shdata[(int)(off + /* unsafe.Offsetof(sh.Flags) */ (uintptr)8)..])),
                Offset: bo.Uint64(shdata[(int)(off + /* unsafe.Offsetof(sh.Off) */ (uintptr)24)..]),
                FileSize: bo.Uint64(shdata[(int)(off + /* unsafe.Offsetof(sh.Size) */ (uintptr)32)..]),
                Addr: bo.Uint64(shdata[(int)(off + /* unsafe.Offsetof(sh.Addr) */ (uintptr)16)..]),
                Link: bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Link) */ (uintptr)40)..]),
                Info: bo.Uint32(shdata[(int)(off + /* unsafe.Offsetof(sh.Info) */ (uintptr)44)..]),
                Addralign: bo.Uint64(shdata[(int)(off + /* unsafe.Offsetof(sh.Addralign) */ (uintptr)48)..]),
                Entsize: bo.Uint64(shdata[(int)(off + /* unsafe.Offsetof(sh.Entsize) */ (uintptr)56)..])
            );
        }

        if ((int64)(~s).Offset < 0) {
            return (default!, new FormatErrorжerror(Ꮡ(new FormatError(shoff + (int64)off, "invalid section offset"u8, (int64)(~s).Offset))));
        }
        if ((int64)(~s).FileSize < 0) {
            return (default!, new FormatErrorжerror(Ꮡ(new FormatError(shoff + (int64)off, "invalid section size"u8, (int64)(~s).FileSize))));
        }
        s.Value.sr = io.NewSectionReader(r, (int64)(~s).Offset, (int64)(~s).FileSize);
        if ((SectionFlag)((~s).Flags & SHF_COMPRESSED) == 0){
            s.Value.ReaderAt = new io.SectionReaderжReaderAt(s.Value.sr);
            s.Value.Size = s.Value.FileSize;
        } else {
            // Read the compression header.
            var exprᴛ8 = (~f).Class;
            if (exprᴛ8 == ELFCLASS32) {
                Chdr32 ch = default!;
                var chdata = new slice<byte>((nint)(/* unsafe.Sizeof(ch) */ (uintptr)12));
                {
                    var (_, errΔ12) = (~s).sr.ReadAt(chdata, 0); if (errΔ12 != default!) {
                        return (default!, errΔ12);
                    }
                }
                s.Value.compressionType = ((CompressionType)(nint)bo.Uint32(chdata[(int)(/* unsafe.Offsetof(ch.Type) */ (uintptr)0)..]));
                s.Value.Size = (uint64)bo.Uint32(chdata[(int)(/* unsafe.Offsetof(ch.Size) */ (uintptr)4)..]);
                s.Value.Addralign = (uint64)bo.Uint32(chdata[(int)(/* unsafe.Offsetof(ch.Addralign) */ (uintptr)8)..]);
                s.Value.compressionOffset = (int64)/* unsafe.Sizeof(ch) */ (uintptr)12;
            }
            else if (exprᴛ8 == ELFCLASS64) {
                Chdr64 ch = default!;
                var chdata = new slice<byte>((nint)(/* unsafe.Sizeof(ch) */ (uintptr)24));
                {
                    var (_, errΔ13) = (~s).sr.ReadAt(chdata, 0); if (errΔ13 != default!) {
                        return (default!, errΔ13);
                    }
                }
                s.Value.compressionType = ((CompressionType)(nint)bo.Uint32(chdata[(int)(/* unsafe.Offsetof(ch.Type) */ (uintptr)0)..]));
                s.Value.Size = bo.Uint64(chdata[(int)(/* unsafe.Offsetof(ch.Size) */ (uintptr)8)..]);
                s.Value.Addralign = bo.Uint64(chdata[(int)(/* unsafe.Offsetof(ch.Addralign) */ (uintptr)16)..]);
                s.Value.compressionOffset = (int64)/* unsafe.Sizeof(ch) */ (uintptr)24;
            }

        }
        f.Value.Sections = append((~f).Sections, s);
    }
    if (len((~f).Sections) == 0) {
        return (f, default!);
    }
    // Load section header string table.
    if (shstrndx == 0) {
        // If the file has no section name string table,
        // shstrndx holds the value SHN_UNDEF (0).
        return (f, default!);
    }
    var shstr = (~f).Sections[shstrndx];
    if ((~shstr).Type != SHT_STRTAB) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(shoff + (int64)(shstrndx * shentsize), "invalid ELF section name string table type"u8, (~shstr).Type))));
    }
    (var shstrtab, err) = shstr.Data();
    if (err != default!) {
        return (default!, err);
    }
    foreach (var (i, s) in (~f).Sections) {
        bool ok = default!;
        (s.Value.Name, ok) = getString(shstrtab, (nint)names[i]);
        if (!ok) {
            return (default!, new FormatErrorжerror(Ꮡ(new FormatError(shoff + (int64)(i * shentsize), "bad section name index"u8, names[i]))));
        }
    }
    return (f, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notImplementedˢ = "not implemented"u8;

// getSymbols returns a slice of Symbols from parsing the symbol table
// with the given type, along with the associated string table.
[GoRecv] internal static (slice<Symbol>, slice<byte>, error) getSymbols(this ref File f, SectionType typ) {
    var exprᴛ1 = f.Class;
    if (exprᴛ1 == ELFCLASS64) {
        return f.getSymbols64(typ);
    }
    if (exprᴛ1 == ELFCLASS32) {
        return f.getSymbols32(typ);
    }

    return (default!, default!, errors.New(notImplementedˢ));
}

// ErrNoSymbols is returned by [File.Symbols] and [File.DynamicSymbols]
// if there is no such section in the File.
public static error ErrNoSymbols = errors.New("no symbol section"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string symbolSectionIsEmptyˢ = "symbol section is empty"u8;
internal static readonly @string lengthOfSymbolSectionIsˢ = "length of symbol section is not a multiple of SymSize"u8;

[GoRecv] internal static (slice<Symbol>, slice<byte>, error) getSymbols32(this ref File f, SectionType typ) {
    var symtabSection = f.SectionByType(typ);
    if (symtabSection == nil) {
        return (default!, default!, ErrNoSymbols);
    }
    var (data, err) = symtabSection.Data();
    if (err != default!) {
        return (default!, default!, fmt.Errorf("cannot load symbol section: %w"u8, err));
    }
    if (len(data) == 0) {
        return (default!, default!, errors.New(symbolSectionIsEmptyˢ));
    }
    if (len(data) % (nint)Sym32Size != 0) {
        return (default!, default!, errors.New(lengthOfSymbolSectionIsˢ));
    }
    (var strdata, err) = f.stringTable((~symtabSection).Link);
    if (err != default!) {
        return (default!, default!, fmt.Errorf("cannot load string table section: %w"u8, err));
    }
    // The first entry is all zeros.
    data = data[(int)(Sym32Size)..];
    var symbols = new slice<Symbol>(len(data) / (nint)Sym32Size);
    nint i = 0;
    Sym32 sym = default!;
    while (len(data) > 0) {
        sym.Name = f.ByteOrder.Uint32(data[0..4]);
        sym.Value = f.ByteOrder.Uint32(data[4..8]);
        sym.Size = f.ByteOrder.Uint32(data[8..12]);
        sym.Info = data[12];
        sym.Other = data[13];
        sym.Shndx = f.ByteOrder.Uint16(data[14..16]);
        var (str, _) = getString(strdata, (nint)sym.Name);
        symbols[i].Name = str;
        symbols[i].Info = sym.Info;
        symbols[i].Other = sym.Other;
        symbols[i].Section = ((SectionIndex)(nint)sym.Shndx);
        symbols[i].Value = (uint64)sym.Value;
        symbols[i].Size = (uint64)sym.Size;
        i++;
        data = data[(int)(Sym32Size)..];
    }
    return (symbols, strdata, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lengthOfSymbolSectionIsˢ2 = "length of symbol section is not a multiple of Sym64Size"u8;

[GoRecv] internal static (slice<Symbol>, slice<byte>, error) getSymbols64(this ref File f, SectionType typ) {
    var symtabSection = f.SectionByType(typ);
    if (symtabSection == nil) {
        return (default!, default!, ErrNoSymbols);
    }
    var (data, err) = symtabSection.Data();
    if (err != default!) {
        return (default!, default!, fmt.Errorf("cannot load symbol section: %w"u8, err));
    }
    if (len(data) % (nint)Sym64Size != 0) {
        return (default!, default!, errors.New(lengthOfSymbolSectionIsˢ2));
    }
    (var strdata, err) = f.stringTable((~symtabSection).Link);
    if (err != default!) {
        return (default!, default!, fmt.Errorf("cannot load string table section: %w"u8, err));
    }
    // The first entry is all zeros.
    data = data[(int)(Sym64Size)..];
    var symbols = new slice<Symbol>(len(data) / (nint)Sym64Size);
    nint i = 0;
    Sym64 sym = default!;
    while (len(data) > 0) {
        sym.Name = f.ByteOrder.Uint32(data[0..4]);
        sym.Info = data[4];
        sym.Other = data[5];
        sym.Shndx = f.ByteOrder.Uint16(data[6..8]);
        sym.Value = f.ByteOrder.Uint64(data[8..16]);
        sym.Size = f.ByteOrder.Uint64(data[16..24]);
        var (str, _) = getString(strdata, (nint)sym.Name);
        symbols[i].Name = str;
        symbols[i].Info = sym.Info;
        symbols[i].Other = sym.Other;
        symbols[i].Section = ((SectionIndex)(nint)sym.Shndx);
        symbols[i].Value = sym.Value;
        symbols[i].Size = sym.Size;
        i++;
        data = data[(int)(Sym64Size)..];
    }
    return (symbols, strdata, default!);
}

// getString extracts a string from an ELF string table.
internal static (@string, bool) getString(slice<byte> section, nint start) {
    if (start < 0 || start >= len(section)) {
        return ("", false);
    }
    for (nint end = start; end < len(section); end++) {
        if (section[end] == 0) {
            return (((@string)(section[(int)(start)..(int)(end)])), true);
        }
    }
    return ("", false);
}

// Section returns a section with the given name, or nil if no such
// section exists.
[GoRecv] public static ж<ΔSection> Section(this ref File f, @string name) {
    foreach (var (_, s) in f.Sections) {
        if ((~s).Name == name) {
            return s;
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string applyRelocationsNotˢ = "applyRelocations: not implemented"u8;

// applyRelocations applies relocations to dst. rels is a relocations section
// in REL or RELA format.
[GoRecv] internal static error applyRelocations(this ref File f, slice<byte> dst, slice<byte> rels) {
    switch (ᐧ) {
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_X86_64: {
        return f.applyRelocationsAMD64(dst, rels);
    }
    case {} when f.Class == ELFCLASS32 && f.Machine == EM_386: {
        return f.applyRelocations386(dst, rels);
    }
    case {} when f.Class == ELFCLASS32 && f.Machine == EM_ARM: {
        return f.applyRelocationsARM(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_AARCH64: {
        return f.applyRelocationsARM64(dst, rels);
    }
    case {} when f.Class == ELFCLASS32 && f.Machine == EM_PPC: {
        return f.applyRelocationsPPC(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_PPC64: {
        return f.applyRelocationsPPC64(dst, rels);
    }
    case {} when f.Class == ELFCLASS32 && f.Machine == EM_MIPS: {
        return f.applyRelocationsMIPS(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_MIPS: {
        return f.applyRelocationsMIPS64(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_LOONGARCH: {
        return f.applyRelocationsLOONG64(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_RISCV: {
        return f.applyRelocationsRISCV64(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_S390: {
        return f.applyRelocationss390x(dst, rels);
    }
    case {} when f.Class == ELFCLASS64 && f.Machine == EM_SPARCV9: {
        return f.applyRelocationsSPARC64(dst, rels);
    }
    default: {
        return errors.New(applyRelocationsNotˢ);
    }}

}

// canApplyRelocation reports whether we should try to apply a
// relocation to a DWARF data section, given a pointer to the symbol
// targeted by the relocation.
// Most relocations in DWARF data tend to be section-relative, but
// some target non-section symbols (for example, low_PC attrs on
// subprogram or compilation unit DIEs that target function symbols).
internal static bool canApplyRelocation(ref Symbol sym) {
    return sym.Section != SHN_UNDEF && sym.Section < SHN_LORESERVE;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lengthOfRelocationˢ = "length of relocation section is not a multiple of 24"u8;

[GoRecv] internal static error applyRelocationsAMD64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New(lengthOfRelocationˢ);
    }
    var (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(new bytes_ReaderжReader(b), f.ByteOrder, Ꮡrela);
        var symNo = (rela.Info >> (int)(32));
        R_X86_64 t = ((R_X86_64)(nint)((uint64)(rela.Info & 0xffff)));
        if (symNo == 0 || symNo > (uint64)len(symbols)) {
            continue;
        }
        var sym = Ꮡ(symbols, (int)(symNo - 1));
        if (!canApplyRelocation(ref (sym).DerefOrNull())) {
            continue;
        }
        // There are relocations, so this must be a normal
        // object file.  The code below handles only basic relocations
        // of the form S + A (symbol plus addend).
        var exprᴛ1 = t;
        if (exprᴛ1 == R_X86_64_64) {
            if (rela.Off + 8 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + (uint64)rela.Addend;
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_X86_64_32) {
            if (rela.Off + 4 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val32 = (uint32)(~sym).Value + (uint32)rela.Addend;
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lengthOfRelocationˢ2 = "length of relocation section is not a multiple of 8"u8;

[GoRecv] internal static error applyRelocations386(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 8 is the size of Rel32.
    if (len(rels) % 8 != 0) {
        return errors.New(lengthOfRelocationˢ2);
    }
    var (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rel = ref heap(new Rel32(), out var Ꮡrel);
    while (b.Len() > 0) {
        binary.Read(new bytes_ReaderжReader(b), f.ByteOrder, Ꮡrel);
        var symNo = (rel.Info >> (int)(8));
        R_386 t = ((R_386)(nint)((uint32)(rel.Info & 0xff)));
        if (symNo == 0 || symNo > (uint32)len(symbols)) {
            continue;
        }
        var sym = Ꮡ(symbols, (int)(symNo - 1));
        if (t == R_386_32) {
            if (rel.Off + 4 >= (uint32)len(dst)) {
                continue;
            }
            var val = f.ByteOrder.Uint32(dst[(int)(rel.Off)..(int)(rel.Off + 4)]);
            val += (uint32)(~sym).Value;
            f.ByteOrder.PutUint32(dst[(int)(rel.Off)..(int)(rel.Off + 4)], val);
        }
    }
    return default!;
}

[GoRecv] internal static error applyRelocationsARM(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 8 is the size of Rel32.
    if (len(rels) % 8 != 0) {
        return errors.New(lengthOfRelocationˢ2);
    }
    var (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rel = ref heap(new Rel32(), out var Ꮡrel);
    while (b.Len() > 0) {
        binary.Read(new bytes_ReaderжReader(b), f.ByteOrder, Ꮡrel);
        var symNo = (rel.Info >> (int)(8));
        R_ARM t = ((R_ARM)(nint)((uint32)(rel.Info & 0xff)));
        if (symNo == 0 || symNo > (uint32)len(symbols)) {
            continue;
        }
        var sym = Ꮡ(symbols, (int)(symNo - 1));
        var exprᴛ1 = t;
        if (exprᴛ1 == R_ARM_ABS32) {
            if (rel.Off + 4 >= (uint32)len(dst)) {
                continue;
            }
            var val = f.ByteOrder.Uint32(dst[(int)(rel.Off)..(int)(rel.Off + 4)]);
            val += (uint32)(~sym).Value;
            f.ByteOrder.PutUint32(dst[(int)(rel.Off)..(int)(rel.Off + 4)], val);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsARM64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New(lengthOfRelocationˢ);
    }
    var (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(new bytes_ReaderжReader(b), f.ByteOrder, Ꮡrela);
        var symNo = (rela.Info >> (int)(32));
        R_AARCH64 t = ((R_AARCH64)(nint)((uint64)(rela.Info & 0xffff)));
        if (symNo == 0 || symNo > (uint64)len(symbols)) {
            continue;
        }
        var sym = Ꮡ(symbols, (int)(symNo - 1));
        if (!canApplyRelocation(ref (sym).DerefOrNull())) {
            continue;
        }
        // There are relocations, so this must be a normal
        // object file.  The code below handles only basic relocations
        // of the form S + A (symbol plus addend).
        var exprᴛ1 = t;
        if (exprᴛ1 == R_AARCH64_ABS64) {
            if (rela.Off + 8 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + (uint64)rela.Addend;
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_AARCH64_ABS32) {
            if (rela.Off + 4 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val32 = (uint32)(~sym).Value + (uint32)rela.Addend;
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lengthOfRelocationˢ3 = "length of relocation section is not a multiple of 12"u8;

[GoRecv] internal static error applyRelocationsPPC(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 12 is the size of Rela32.
    if (len(rels) % 12 != 0) {
        return errors.New(lengthOfRelocationˢ3);
    }
    var (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela32(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(new bytes_ReaderжReader(b), f.ByteOrder, Ꮡrela);
        var symNo = (rela.Info >> (int)(8));
        R_PPC t = ((R_PPC)(nint)((uint32)(rela.Info & 0xff)));
        if (symNo == 0 || symNo > (uint32)len(symbols)) {
            continue;
        }
        var sym = Ꮡ(symbols, (int)(symNo - 1));
        if (!canApplyRelocation(ref (sym).DerefOrNull())) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_PPC_ADDR32) {
            if (rela.Off + 4 >= (uint32)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val32 = (uint32)(~sym).Value + (uint32)rela.Addend;
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsPPC64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New(lengthOfRelocationˢ);
    }
    var (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(new bytes_ReaderжReader(b), f.ByteOrder, Ꮡrela);
        var symNo = (rela.Info >> (int)(32));
        R_PPC64 t = ((R_PPC64)(nint)((uint64)(rela.Info & 0xffff)));
        if (symNo == 0 || symNo > (uint64)len(symbols)) {
            continue;
        }
        var sym = Ꮡ(symbols, (int)(symNo - 1));
        if (!canApplyRelocation(ref (sym).DerefOrNull())) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_PPC64_ADDR64) {
            if (rela.Off + 8 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + (uint64)rela.Addend;
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_PPC64_ADDR32) {
            if (rela.Off + 4 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val32 = (uint32)(~sym).Value + (uint32)rela.Addend;
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsMIPS(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 8 is the size of Rel32.
    if (len(rels) % 8 != 0) {
        return errors.New(lengthOfRelocationˢ2);
    }
    var (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rel = ref heap(new Rel32(), out var Ꮡrel);
    while (b.Len() > 0) {
        binary.Read(new bytes_ReaderжReader(b), f.ByteOrder, Ꮡrel);
        var symNo = (rel.Info >> (int)(8));
        R_MIPS t = ((R_MIPS)(nint)((uint32)(rel.Info & 0xff)));
        if (symNo == 0 || symNo > (uint32)len(symbols)) {
            continue;
        }
        var sym = Ꮡ(symbols, (int)(symNo - 1));
        var exprᴛ1 = t;
        if (exprᴛ1 == R_MIPS_32) {
            if (rel.Off + 4 >= (uint32)len(dst)) {
                continue;
            }
            var val = f.ByteOrder.Uint32(dst[(int)(rel.Off)..(int)(rel.Off + 4)]);
            val += (uint32)(~sym).Value;
            f.ByteOrder.PutUint32(dst[(int)(rel.Off)..(int)(rel.Off + 4)], val);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsMIPS64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New(lengthOfRelocationˢ);
    }
    var (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(new bytes_ReaderжReader(b), f.ByteOrder, Ꮡrela);
        uint64 symNo = default!;
        R_MIPS t = default!;
        if (AreEqual(f.ByteOrder, binary.BigEndian)){
            symNo = (rela.Info >> (int)(32));
            t = ((R_MIPS)(nint)((uint64)(rela.Info & 0xff)));
        } else {
            symNo = (uint64)(rela.Info & 0xffffffffU);
            t = ((R_MIPS)(nint)((rela.Info >> (int)(56))));
        }
        if (symNo == 0 || symNo > (uint64)len(symbols)) {
            continue;
        }
        var sym = Ꮡ(symbols, (int)(symNo - 1));
        if (!canApplyRelocation(ref (sym).DerefOrNull())) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_MIPS_64) {
            if (rela.Off + 8 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + (uint64)rela.Addend;
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_MIPS_32) {
            if (rela.Off + 4 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val32 = (uint32)(~sym).Value + (uint32)rela.Addend;
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsLOONG64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New(lengthOfRelocationˢ);
    }
    var (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(new bytes_ReaderжReader(b), f.ByteOrder, Ꮡrela);
        uint64 symNo = default!;
        R_LARCH t = default!;
        symNo = (rela.Info >> (int)(32));
        t = ((R_LARCH)(nint)((uint64)(rela.Info & 0xffff)));
        if (symNo == 0 || symNo > (uint64)len(symbols)) {
            continue;
        }
        var sym = Ꮡ(symbols, (int)(symNo - 1));
        if (!canApplyRelocation(ref (sym).DerefOrNull())) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_LARCH_64) {
            if (rela.Off + 8 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + (uint64)rela.Addend;
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_LARCH_32) {
            if (rela.Off + 4 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val32 = (uint32)(~sym).Value + (uint32)rela.Addend;
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsRISCV64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New(lengthOfRelocationˢ);
    }
    var (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(new bytes_ReaderжReader(b), f.ByteOrder, Ꮡrela);
        var symNo = (rela.Info >> (int)(32));
        R_RISCV t = ((R_RISCV)(nint)((uint64)(rela.Info & 0xffff)));
        if (symNo == 0 || symNo > (uint64)len(symbols)) {
            continue;
        }
        var sym = Ꮡ(symbols, (int)(symNo - 1));
        if (!canApplyRelocation(ref (sym).DerefOrNull())) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_RISCV_64) {
            if (rela.Off + 8 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + (uint64)rela.Addend;
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_RISCV_32) {
            if (rela.Off + 4 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val32 = (uint32)(~sym).Value + (uint32)rela.Addend;
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationss390x(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New(lengthOfRelocationˢ);
    }
    var (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(new bytes_ReaderжReader(b), f.ByteOrder, Ꮡrela);
        var symNo = (rela.Info >> (int)(32));
        R_390 t = ((R_390)(nint)((uint64)(rela.Info & 0xffff)));
        if (symNo == 0 || symNo > (uint64)len(symbols)) {
            continue;
        }
        var sym = Ꮡ(symbols, (int)(symNo - 1));
        if (!canApplyRelocation(ref (sym).DerefOrNull())) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_390_64) {
            if (rela.Off + 8 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + (uint64)rela.Addend;
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_390_32) {
            if (rela.Off + 4 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val32 = (uint32)(~sym).Value + (uint32)rela.Addend;
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

[GoRecv] internal static error applyRelocationsSPARC64(this ref File f, slice<byte> dst, slice<byte> rels) {
    // 24 is the size of Rela64.
    if (len(rels) % 24 != 0) {
        return errors.New(lengthOfRelocationˢ);
    }
    var (symbols, _, err) = f.getSymbols(SHT_SYMTAB);
    if (err != default!) {
        return err;
    }
    var b = bytes.NewReader(rels);
    ref var rela = ref heap(new Rela64(), out var Ꮡrela);
    while (b.Len() > 0) {
        binary.Read(new bytes_ReaderжReader(b), f.ByteOrder, Ꮡrela);
        var symNo = (rela.Info >> (int)(32));
        R_SPARC t = ((R_SPARC)(nint)((uint64)(rela.Info & 0xff)));
        if (symNo == 0 || symNo > (uint64)len(symbols)) {
            continue;
        }
        var sym = Ꮡ(symbols, (int)(symNo - 1));
        if (!canApplyRelocation(ref (sym).DerefOrNull())) {
            continue;
        }
        var exprᴛ1 = t;
        if (exprᴛ1 == R_SPARC_64 || exprᴛ1 == R_SPARC_UA64) {
            if (rela.Off + 8 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val64 = (~sym).Value + (uint64)rela.Addend;
            f.ByteOrder.PutUint64(dst[(int)(rela.Off)..(int)(rela.Off + 8)], val64);
        }
        else if (exprᴛ1 == R_SPARC_32 || exprᴛ1 == R_SPARC_UA32) {
            if (rela.Off + 4 >= (uint64)len(dst) || rela.Addend < 0) {
                continue;
            }
            var val32 = (uint32)(~sym).Value + (uint32)rela.Addend;
            f.ByteOrder.PutUint32(dst[(int)(rela.Off)..(int)(rela.Off + 4)], val32);
        }

    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string debugˢ = ".debug_"u8;
internal static readonly @string zdebugˢ2 = ".zdebug_"u8;
internal static readonly @string abbrevˢ = "abbrev"u8;
internal static readonly @string infoˢ = "info"u8;
internal static readonly @string lineˢ = "line"u8;
internal static readonly @string rangesˢ = "ranges"u8;
internal static readonly @string strˢ = "str"u8;

public static (ж<dwarf.Data>, error) DWARF(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    @string dwarfSuffix(ж<ΔSection> s) {
        switch (ᐧ) {
        case {} when strings.HasPrefix((~s).Name, debugˢ): {
            return (~s).Name[7..];
        }
        case {} when strings.HasPrefix((~s).Name, zdebugˢ2): {
            return (~s).Name[8..];
        }
        default: {
            return ""u8;
        }}

    }
    // sectionData gets the data for s, checks its size, and
    // applies any applicable relations.
    (slice<byte>, error) sectionData(nint i, ж<ΔSection> s) {
        var (b, errΔ1) = s.Data();
        if (errΔ1 != default! && (uint64)len(b) < (~s).Size) {
            return (default!, errΔ1);
        }
        if (Ꮡf.Value.Type == ET_EXEC) {
            // Do not apply relocations to DWARF sections for ET_EXEC binaries.
            // Relocations should already be applied, and .rela sections may
            // contain incorrect data.
            return (b, default!);
        }
        foreach (var (_, r) in Ꮡf.Value.Sections) {
            if ((~r).Type != SHT_RELA && (~r).Type != SHT_REL) {
                continue;
            }
            if ((nint)(~r).Info != i) {
                continue;
            }
            var (rd, errΔ2) = r.Data();
            if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
            errΔ2 = Ꮡf.Value.applyRelocations(b, rd);
            if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
        }
        return (b, default!);
    }
    // There are many DWARf sections, but these are the ones
    // the debug/dwarf package started with.
    map<@string, slice<byte>> dat = new map<@string, slice<byte>>{["abbrev"u8] = default!, ["info"u8] = default!, ["str"u8] = default!, ["line"u8] = default!, ["ranges"u8] = default!};
    foreach (var (i, s) in f.Sections) {
        @string suffix = dwarfSuffix(s);
        if (suffix == ""u8) {
            continue;
        }
        {
            var (_, ok) = dat[suffix, ꟷ]; if (!ok) {
                continue;
            }
        }
        var (b, errΔ3) = sectionData(i, s);
        if (errΔ3 != default!) {
            return (default!, errΔ3);
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
        var (b, errΔ4) = sectionData(i, s);
        if (errΔ4 != default!) {
            return (default!, errΔ4);
        }
        if (suffix == "types"u8){
            {
                var errΔ5 = d.AddTypes(fmt.Sprintf("types-%d"u8, i), b); if (errΔ5 != default!) {
                    return (default!, errΔ5);
                }
            }
        } else {
            {
                var errΔ6 = d.AddSection(".debug_"u8 + suffix, b); if (errΔ6 != default!) {
                    return (default!, errΔ6);
                }
            }
        }
    }
    return (d, default!);
}

// Symbols returns the symbol table for f. The symbols will be listed in the order
// they appear in f.
//
// For compatibility with Go 1.0, Symbols omits the null symbol at index 0.
// After retrieving the symbols as symtab, an externally supplied index x
// corresponds to symtab[x-1], not symtab[x].
[GoRecv] public static (slice<Symbol>, error) Symbols(this ref File f) {
    var (sym, _, err) = f.getSymbols(SHT_SYMTAB);
    return (sym, err);
}

// DynamicSymbols returns the dynamic symbol table for f. The symbols
// will be listed in the order they appear in f.
//
// If f has a symbol version table, the returned [File.Symbols] will have
// initialized Version and Library fields.
//
// For compatibility with [File.Symbols], [File.DynamicSymbols] omits the null symbol at index 0.
// After retrieving the symbols as symtab, an externally supplied index x
// corresponds to symtab[x-1], not symtab[x].
[GoRecv] public static (slice<Symbol>, error) DynamicSymbols(this ref File f) {
    var (sym, str, err) = f.getSymbols(SHT_DYNSYM);
    if (err != default!) {
        return (default!, err);
    }
    (var hasVersions, err) = f.gnuVersionInit(str);
    if (err != default!) {
        return (default!, err);
    }
    if (hasVersions) {
        foreach (var (i, _) in sym) {
            (sym[i].HasVersion, sym[i].VersionIndex, sym[i].Version, sym[i].Library) = f.gnuVersion(i);
        }
    }
    return (sym, default!);
}

[GoType] partial struct ImportedSymbol {
    public @string Name;
    public @string Version;
    public @string Library;
}

// ImportedSymbols returns the names of all symbols
// referred to by the binary f that are expected to be
// satisfied by other libraries at dynamic load time.
// It does not return weak symbols.
[GoRecv] public static (slice<ImportedSymbol>, error) ImportedSymbols(this ref File f) {
    var (sym, str, err) = f.getSymbols(SHT_DYNSYM);
    if (err != default!) {
        return (default!, err);
    }
    {
        var (_, errΔ1) = f.gnuVersionInit(str); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    slice<ImportedSymbol> all = default!;
    foreach (var (i, s) in sym) {
        if (ST_BIND(s.Info) == STB_GLOBAL && s.Section == SHN_UNDEF) {
            all = append(all, new ImportedSymbol(Name: s.Name));
            var symΔ1 = Ꮡ(all, len(all) - 1);
            (_, _, symΔ1.Value.Version, symΔ1.Value.Library) = f.gnuVersion(i);
        }
    }
    return (all, default!);
}

[GoType("num:uint16")] partial struct VersionIndex;

// IsHidden reports whether the symbol is hidden within the version.
// This means that the symbol can only be seen by specifying the exact version.
public static bool IsHidden(this VersionIndex vi) {
    return (VersionIndex)(vi & 0x8000) != 0;
}

// Index returns the version index.
// If this is the value 0, it means that the symbol is local,
// and is not visible externally.
// If this is the value 1, it means that the symbol is in the base version,
// and has no specific version; it may or may not match a
// [DynamicVersion.Index] in the slice returned by [File.DynamicVersions].
// Other values will match either [DynamicVersion.Index]
// in the slice returned by [File.DynamicVersions],
// or [DynamicVersionDep.Index] in the Needs field
// of the elements of the slice returned by [File.DynamicVersionNeeds].
// In general, a defined symbol will have an index referring
// to DynamicVersions, and an undefined symbol will have an index
// referring to some version in DynamicVersionNeeds.
public static uint16 Index(this VersionIndex vi) {
    return (uint16)((VersionIndex)(vi & 0x7fff));
}

// DynamicVersion is a version defined by a dynamic object.
// This describes entries in the ELF SHT_GNU_verdef section.
// We assume that the vd_version field is 1.
// Note that the name of the version appears here;
// it is not in the first Deps entry as it is in the ELF file.
[GoType] partial struct DynamicVersion {
    public @string Name; // Name of version defined by this index.
    public uint16 Index; // Version index.
    public DynamicVersionFlag Flags;
    public slice<@string> Deps; // Names of versions that this version depends upon.
}

// DynamicVersionNeed describes a shared library needed by a dynamic object,
// with a list of the versions needed from that shared library.
// This describes entries in the ELF SHT_GNU_verneed section.
// We assume that the vn_version field is 1.
[GoType] partial struct DynamicVersionNeed {
    public @string Name;             // Shared library name.
    public slice<DynamicVersionDep> Needs; // Dependencies.
}

// DynamicVersionDep is a version needed from some shared library.
[GoType] partial struct DynamicVersionDep {
    public DynamicVersionFlag Flags;
    public uint16 Index; // Version index.
    public @string Dep; // Name of required version.
}

// dynamicVersions returns version information for a dynamic object.
[GoRecv] internal static error dynamicVersions(this ref File f, slice<byte> str) {
    if (f.dynVers != default!) {
        // Already initialized.
        return default!;
    }
    // Accumulate verdef information.
    var vd = f.SectionByType(SHT_GNU_VERDEF);
    if (vd == nil) {
        return default!;
    }
    var (d, _) = vd.Data();
    slice<DynamicVersion> dynVers = default!;
    nint i = 0;
    while (ᐧ) {
        if (i + 20 > len(d)) {
            break;
        }
        ref var version = ref heap<uint16>(out var Ꮡversion);
        version = f.ByteOrder.Uint16(d[(int)(i)..(int)(i + 2)]);
        if (version != 1) {
            return new FormatErrorжerror(Ꮡ(new FormatError((int64)((~vd).Offset + (uint64)i), "unexpected dynamic version"u8, version)));
        }
        var flags = ((DynamicVersionFlag)f.ByteOrder.Uint16(d[(int)(i + 2)..(int)(i + 4)]));
        var ndx = f.ByteOrder.Uint16(d[(int)(i + 4)..(int)(i + 6)]);
        var cnt = f.ByteOrder.Uint16(d[(int)(i + 6)..(int)(i + 8)]);
        var aux = f.ByteOrder.Uint32(d[(int)(i + 12)..(int)(i + 16)]);
        var next = f.ByteOrder.Uint32(d[(int)(i + 16)..(int)(i + 20)]);
        if (cnt == 0) {
            return new FormatErrorжerror(Ꮡ(new FormatError((int64)((~vd).Offset + (uint64)i), "dynamic version has no name"u8, default!)));
        }
        @string name = default!;
        @string depName = default!;
        slice<@string> deps = default!;
        nint j = i + (nint)aux;
        for (nint c = 0; c < (nint)cnt; c++) {
            if (j + 8 > len(d)) {
                break;
            }
            var vname = f.ByteOrder.Uint32(d[(int)(j)..(int)(j + 4)]);
            var vnext = f.ByteOrder.Uint32(d[(int)(j + 4)..(int)(j + 8)]);
            (depName, _) = getString(str, (nint)vname);
            if (c == 0){
                name = depName;
            } else {
                deps = append(deps, depName);
            }
            j += (nint)vnext;
        }
        dynVers = append(dynVers, new DynamicVersion(
            Name: name,
            Index: ndx,
            Flags: flags,
            Deps: deps
        ));
        if (next == 0) {
            break;
        }
        i += (nint)next;
    }
    f.dynVers = dynVers;
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dynamicVersionsMissingˢ = "DynamicVersions: missing version table"u8;

// DynamicVersions returns version information for a dynamic object.
[GoRecv] public static (slice<DynamicVersion>, error) DynamicVersions(this ref File f) {
    if (f.dynVers == default!) {
        var (_, str, err) = f.getSymbols(SHT_DYNSYM);
        if (err != default!) {
            return (default!, err);
        }
        (var hasVersions, err) = f.gnuVersionInit(str);
        if (err != default!) {
            return (default!, err);
        }
        if (!hasVersions) {
            return (default!, errors.New(dynamicVersionsMissingˢ));
        }
    }
    return (f.dynVers, default!);
}

// dynamicVersionNeeds returns version dependencies for a dynamic object.
[GoRecv] internal static error dynamicVersionNeeds(this ref File f, slice<byte> str) {
    if (f.dynVerNeeds != default!) {
        // Already initialized.
        return default!;
    }
    // Accumulate verneed information.
    var vn = f.SectionByType(SHT_GNU_VERNEED);
    if (vn == nil) {
        return default!;
    }
    var (d, _) = vn.Data();
    slice<DynamicVersionNeed> dynVerNeeds = default!;
    nint i = 0;
    while (ᐧ) {
        if (i + 16 > len(d)) {
            break;
        }
        ref var vers = ref heap<uint16>(out var Ꮡvers);
        vers = f.ByteOrder.Uint16(d[(int)(i)..(int)(i + 2)]);
        if (vers != 1) {
            return new FormatErrorжerror(Ꮡ(new FormatError((int64)((~vn).Offset + (uint64)i), "unexpected dynamic need version"u8, vers)));
        }
        var cnt = f.ByteOrder.Uint16(d[(int)(i + 2)..(int)(i + 4)]);
        var fileoff = f.ByteOrder.Uint32(d[(int)(i + 4)..(int)(i + 8)]);
        var aux = f.ByteOrder.Uint32(d[(int)(i + 8)..(int)(i + 12)]);
        var next = f.ByteOrder.Uint32(d[(int)(i + 12)..(int)(i + 16)]);
        var (@file, _) = getString(str, (nint)fileoff);
        slice<DynamicVersionDep> deps = default!;
        nint j = i + (nint)aux;
        for (nint c = 0; c < (nint)cnt; c++) {
            if (j + 16 > len(d)) {
                break;
            }
            var flags = ((DynamicVersionFlag)f.ByteOrder.Uint16(d[(int)(j + 4)..(int)(j + 6)]));
            var index = f.ByteOrder.Uint16(d[(int)(j + 6)..(int)(j + 8)]);
            var nameoff = f.ByteOrder.Uint32(d[(int)(j + 8)..(int)(j + 12)]);
            var nextΔ1 = f.ByteOrder.Uint32(d[(int)(j + 12)..(int)(j + 16)]);
            var (depName, _) = getString(str, (nint)nameoff);
            deps = append(deps, new DynamicVersionDep(
                Flags: flags,
                Index: index,
                Dep: depName
            ));
            if (nextΔ1 == 0) {
                break;
            }
            j += (nint)nextΔ1;
        }
        dynVerNeeds = append(dynVerNeeds, new DynamicVersionNeed(
            Name: @file,
            Needs: deps
        ));
        if (next == 0) {
            break;
        }
        i += (nint)next;
    }
    f.dynVerNeeds = dynVerNeeds;
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dynamicVersionNeedsˢ = "DynamicVersionNeeds: missing version table"u8;

// DynamicVersionNeeds returns version dependencies for a dynamic object.
[GoRecv] public static (slice<DynamicVersionNeed>, error) DynamicVersionNeeds(this ref File f) {
    if (f.dynVerNeeds == default!) {
        var (_, str, err) = f.getSymbols(SHT_DYNSYM);
        if (err != default!) {
            return (default!, err);
        }
        (var hasVersions, err) = f.gnuVersionInit(str);
        if (err != default!) {
            return (default!, err);
        }
        if (!hasVersions) {
            return (default!, errors.New(dynamicVersionNeedsˢ));
        }
    }
    return (f.dynVerNeeds, default!);
}

// gnuVersionInit parses the GNU version tables
// for use by calls to gnuVersion.
// It reports whether any version tables were found.
[GoRecv] internal static (bool, error) gnuVersionInit(this ref File f, slice<byte> str) {
    // Versym parallels symbol table, indexing into verneed.
    var vs = f.SectionByType(SHT_GNU_VERSYM);
    if (vs == nil) {
        return (false, default!);
    }
    var (d, _) = vs.Data();
    f.gnuVersym = d;
    {
        var err = f.dynamicVersions(str); if (err != default!) {
            return (false, err);
        }
    }
    {
        var err = f.dynamicVersionNeeds(str); if (err != default!) {
            return (false, err);
        }
    }
    return (true, default!);
}

// gnuVersion adds Library and Version information to sym,
// which came from offset i of the symbol table.
[GoRecv] internal static (bool hasVersion, VersionIndex versionIndex, @string version, @string library) gnuVersion(this ref File f, nint i) {
    // Each entry is two bytes; skip undef entry at beginning.
    i = (i + 1) * 2;
    if (i >= len(f.gnuVersym)) {
        return (false, 0, "", "");
    }
    var s = f.gnuVersym[(int)(i)..];
    if (len(s) < 2) {
        return (false, 0, "", "");
    }
    var vi = ((VersionIndex)f.ByteOrder.Uint16(s));
    var ndx = vi.Index();
    if (ndx == 0 || ndx == 1) {
        return (true, vi, "", "");
    }
    foreach (var (_, v) in f.dynVerNeeds) {
        foreach (var (_, n) in v.Needs) {
            if (ndx == n.Index) {
                return (true, vi, n.Dep, v.Name);
            }
        }
    }
    foreach (var (_, v) in f.dynVers) {
        if (ndx == v.Index) {
            return (true, vi, v.Name, "");
        }
    }
    return (false, 0, "", "");
}

// ImportedLibraries returns the names of all libraries
// referred to by the binary f that are expected to be
// linked with the binary at dynamic link time.
[GoRecv] public static (slice<@string>, error) ImportedLibraries(this ref File f) {
    return f.DynString(DT_NEEDED);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lengthOfDynamicSectionIsˢ = "length of dynamic section is not a multiple of dynamic entry size"u8;

// DynString returns the strings listed for the given tag in the file's dynamic
// section.
//
// The tag must be one that takes string values: [DT_NEEDED], [DT_SONAME], [DT_RPATH], or
// [DT_RUNPATH].
[GoRecv] public static (slice<@string>, error) DynString(this ref File f, DynTag tag) {
    var exprᴛ1 = tag;
    if (exprᴛ1 == DT_NEEDED || exprᴛ1 == DT_SONAME || exprᴛ1 == DT_RPATH || exprᴛ1 == DT_RUNPATH) {
    }
    else { /* default: */
        return (default!, fmt.Errorf("non-string-valued tag %v"u8, tag));
    }

    var ds = f.SectionByType(SHT_DYNAMIC);
    if (ds == nil) {
        // not dynamic, so no libraries
        return (default!, default!);
    }
    var (d, err) = ds.Data();
    if (err != default!) {
        return (default!, err);
    }
    nint dynSize = 8;
    if (f.Class == ELFCLASS64) {
        dynSize = 16;
    }
    if (len(d) % dynSize != 0) {
        return (default!, errors.New(lengthOfDynamicSectionIsˢ));
    }
    (var str, err) = f.stringTable((~ds).Link);
    if (err != default!) {
        return (default!, err);
    }
    slice<@string> all = default!;
    while (len(d) > 0) {
        DynTag t = default!;
        uint64 v = default!;
        var exprᴛ2 = f.Class;
        if (exprᴛ2 == ELFCLASS32) {
            t = ((DynTag)(nint)f.ByteOrder.Uint32(d[0..4]));
            v = (uint64)f.ByteOrder.Uint32(d[4..8]);
            d = d[8..];
        }
        else if (exprᴛ2 == ELFCLASS64) {
            t = ((DynTag)(nint)f.ByteOrder.Uint64(d[0..8]));
            v = f.ByteOrder.Uint64(d[8..16]);
            d = d[16..];
        }

        if (t == tag) {
            var (s, ok) = getString(str, (nint)v);
            if (ok) {
                all = append(all, s);
            }
        }
    }
    return (all, default!);
}

// DynValue returns the values listed for the given tag in the file's dynamic
// section.
[GoRecv] public static (slice<uint64>, error) DynValue(this ref File f, DynTag tag) {
    var ds = f.SectionByType(SHT_DYNAMIC);
    if (ds == nil) {
        return (default!, default!);
    }
    var (d, err) = ds.Data();
    if (err != default!) {
        return (default!, err);
    }
    nint dynSize = 8;
    if (f.Class == ELFCLASS64) {
        dynSize = 16;
    }
    if (len(d) % dynSize != 0) {
        return (default!, errors.New(lengthOfDynamicSectionIsˢ));
    }
    // Parse the .dynamic section as a string of bytes.
    slice<uint64> vals = default!;
    while (len(d) > 0) {
        DynTag t = default!;
        uint64 v = default!;
        var exprᴛ1 = f.Class;
        if (exprᴛ1 == ELFCLASS32) {
            t = ((DynTag)(nint)f.ByteOrder.Uint32(d[0..4]));
            v = (uint64)f.ByteOrder.Uint32(d[4..8]);
            d = d[8..];
        }
        else if (exprᴛ1 == ELFCLASS64) {
            t = ((DynTag)(nint)f.ByteOrder.Uint64(d[0..8]));
            v = f.ByteOrder.Uint64(d[8..16]);
            d = d[16..];
        }

        if (t == tag) {
            vals = append(vals, v);
        }
    }
    return (vals, default!);
}

[GoType] partial struct nobitsSectionReader {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedReadFromShtˢ = "unexpected read from SHT_NOBITS section"u8;

[GoRecv] internal static (nint n, error err) ReadAt(this ref nobitsSectionReader _, slice<byte> p, int64 off) {
    return (0, errors.New(unexpectedReadFromShtˢ));
}

} // end elf_package

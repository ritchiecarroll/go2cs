// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package macho implements access to Mach-O object files.

# Security

This package is not designed to be hardened against adversarial inputs, and is
outside the scope of https://go.dev/security/policy. In particular, only basic
validation is done when parsing object files. As such, care should be taken when
parsing untrusted inputs, as parsing malformed files may consume significant
resources, or cause panics.
*/
namespace go.debug;

// High level access to low level data structures.
using bytes = bytes_package;
using zlib = compress.zlib_package;
using dwarf = go.debug.dwarf_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using io = io_package;
using os = os_package;
using strings = strings_package;
using @internal;
using compress;
using encoding;
using go.debug;

partial class macho_package {

// A File represents an open Mach-O file.
[GoType] partial struct File {
    public partial ref FileHeader FileHeader { get; }
    public binary.ByteOrder ByteOrder;
    public slice<Load> Loads;
    public slice<ж<ΔSection>> Sections;
    public ж<Symtab> Symtab;
    public ж<Dysymtab> Dysymtab;
    internal io.Closer closer;
}

// A Load represents any Mach-O load command.
[GoType] partial interface Load {
    slice<byte> Raw();
}

[GoType("[]byte")] partial struct LoadBytes;

public static slice<byte> Raw(this LoadBytes b) {
    return b;
}

// A SegmentHeader is the header for a Mach-O 32-bit or 64-bit load segment command.
[GoType] partial struct SegmentHeader {
    public LoadCmd Cmd;
    public uint32 Len;
    public @string Name;
    public uint64 Addr;
    public uint64 Memsz;
    public uint64 Offset;
    public uint64 Filesz;
    public uint32 Maxprot;
    public uint32 Prot;
    public uint32 Nsect;
    public uint32 Flag;
}

// A Segment represents a Mach-O 32-bit or 64-bit load segment command.
[GoType] partial struct ΔSegment {
    public partial ref LoadBytes LoadBytes { get; }
    public partial ref SegmentHeader SegmentHeader { get; }
    // Embed ReaderAt for ReadAt method.
    // Do not embed SectionReader directly
    // to avoid having Read and Seek.
    // If a client wants Read and Seek it must use
    // Open() to avoid fighting over the seek offset
    // with other clients.
    [GoEmbedded] public io_package.ReaderAt ReaderAt;
    internal ж<io.SectionReader> sr;
}

// Data reads and returns the contents of the segment.
[GoRecv] public static (slice<byte>, error) Data(this ref ΔSegment s) {
    return saferio.ReadDataAt(new io.SectionReaderжReaderAt(s.sr), s.Filesz, 0);
}

// Open returns a new ReadSeeker reading the segment.
[GoRecv] public static io.ReadSeeker Open(this ref ΔSegment s) {
    return new io.SectionReaderжReadSeeker(io.NewSectionReader(new io.SectionReaderжReaderAt(s.sr), 0, 9223372036854775807L));
}

[GoType] partial struct SectionHeader {
    public @string Name;
    public @string Seg;
    public uint64 Addr;
    public uint64 Size;
    public uint32 Offset;
    public uint32 Align;
    public uint32 Reloff;
    public uint32 Nreloc;
    public uint32 Flags;
}

// A Reloc represents a Mach-O relocation.
[GoType] partial struct Reloc {
    public uint32 Addr;
    public uint32 Value;
    // when Scattered == false && Extern == true, Value is the symbol number.
    // when Scattered == false && Extern == false, Value is the section number.
    // when Scattered == true, Value is the value that this reloc refers to.
    public uint8 Type;
    public uint8 Len; // 0=byte, 1=word, 2=long, 3=quad
    public bool Pcrel;
    public bool Extern; // valid if Scattered == false
    public bool Scattered;
}

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

// Data reads and returns the contents of the Mach-O section.
[GoRecv] public static (slice<byte>, error) Data(this ref ΔSection s) {
    return saferio.ReadDataAt(new io.SectionReaderжReaderAt(s.sr), s.Size, 0);
}

// Open returns a new ReadSeeker reading the Mach-O section.
[GoRecv] public static io.ReadSeeker Open(this ref ΔSection s) {
    return new io.SectionReaderжReadSeeker(io.NewSectionReader(new io.SectionReaderжReaderAt(s.sr), 0, 9223372036854775807L));
}

// A Dylib represents a Mach-O load dynamic library command.
[GoType] partial struct Dylib {
    public partial ref LoadBytes LoadBytes { get; }
    public @string Name;
    public uint32 Time;
    public uint32 CurrentVersion;
    public uint32 CompatVersion;
}

// A Symtab represents a Mach-O symbol table command.
[GoType] partial struct Symtab {
    public partial ref LoadBytes LoadBytes { get; }
    public partial ref SymtabCmd SymtabCmd { get; }
    public slice<Symbol> Syms;
}

// A Dysymtab represents a Mach-O dynamic symbol table command.
[GoType] partial struct Dysymtab {
    public partial ref LoadBytes LoadBytes { get; }
    public partial ref DysymtabCmd DysymtabCmd { get; }
    public slice<uint32> IndirectSyms; // indices into Symtab.Syms
}

// A Rpath represents a Mach-O rpath command.
[GoType] partial struct Rpath {
    public partial ref LoadBytes LoadBytes { get; }
    public @string Path;
}

// A Symbol is a Mach-O 32-bit or 64-bit symbol table entry.
[GoType] partial struct Symbol {
    public @string Name;
    public uint8 Type;
    public uint8 Sect;
    public uint16 Desc;
    public uint64 Value;
}

/*
 * Mach-O reader
 */

// FormatError is returned by some operations if the data does
// not have the correct format for an object file.
[GoType] partial struct FormatError {
    internal int64 off;
    internal @string msg;
    internal any val;
}

[GoRecv] public static @string Error(this ref FormatError e) {
    @string msg = e.msg;
    if (e.val != default!) {
        msg += fmt.Sprintf(" '%v'"u8, e.val);
    }
    msg += fmt.Sprintf(" in record at byte %#x"u8, e.off);
    return msg;
}

// Open opens the named file using [os.Open] and prepares it for use as a Mach-O binary.
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

// NewFile creates a new [File] for accessing a Mach-O binary in an underlying reader.
// The Mach-O binary is expected to start at position 0 in the ReaderAt.
public static (ж<File>, error) NewFile(io.ReaderAt r) {
    var f = @new<File>();
    var sr = io.NewSectionReader(r, 0, 9223372036854775807L);
    // Read and decode Mach magic to determine byte order, size.
    // Magic32 and Magic64 differ only in the bottom bit.
    array<byte> ident = new(4);
    {
        var (_, errΔ1) = r.ReadAt(ident[0..], 0); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    var be = binary.BigEndian.Uint32(ident[0..]);
    var le = binary.LittleEndian.Uint32(ident[0..]);
    var exprᴛ1 = (uint32)(Magic32 & ~1);
    if (exprᴛ1 == (uint32)(be & ~1)) {
        f.Value.ByteOrder = binary.BigEndian;
        f.Value.Magic = be;
    }
    else if (exprᴛ1 == (uint32)(le & ~1)) {
        f.Value.ByteOrder = binary.LittleEndian;
        f.Value.Magic = le;
    }
    else { /* default: */
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "invalid magic number"u8, default!))));
    }

    // Read entire file header.
    {
        var errΔ2 = binary.Read(new io.SectionReaderжReader(sr), (~f).ByteOrder, f.of(File.ᏑFileHeader)); if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
    }
    // Then load commands.
    ref var offset = ref heap<int64>(out var Ꮡoffset);
    offset = (int64)fileHeaderSize32;
    if ((~f).Magic == Magic64) {
        offset = fileHeaderSize64;
    }
    var (dat, err) = saferio.ReadDataAt(r, (uint64)(~f).Cmdsz, offset);
    if (err != default!) {
        return (default!, err);
    }
    nint c = saferio.SliceCap<Load>((uint64)(~f).Ncmd);
    if (c < 0) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "too many load commands"u8, default!))));
    }
    f.Value.Loads = new slice<Load>(0, c);
    var bo = f.Value.ByteOrder;
    for (var i = (uint32)0; i < (~f).Ncmd; i++) {
        // Each load command begins with uint32 command and length.
        if (len(dat) < 8) {
            return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "command block too small"u8, default!))));
        }
        var (cmd, siz) = (((LoadCmd)bo.Uint32(dat[0..4])), bo.Uint32(dat[4..8]));
        if (siz < 8 || siz > (uint32)len(dat)) {
            return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "invalid command block size"u8, default!))));
        }
        slice<byte> cmddat = default!;
        (cmddat, dat) = (dat.slice(0, (nint)(siz)), dat.slice((nint)(siz)));
        offset += (int64)siz;
        ж<ΔSegment> s = default!;
        var exprᴛ2 = cmd;
        if (exprᴛ2 == LoadCmdRpath) {
            ref var hdr = ref heap(new RpathCmd(), out var Ꮡhdr);
            var b = bytes.NewReader(cmddat);
            {
                var errΔ15 = binary.Read(new bytes_ReaderжReader(b), bo, Ꮡhdr); if (errΔ15 != default!) {
                    return (default!, errΔ15);
                }
            }
            var l = @new<Rpath>();
            if (hdr.Path >= (uint32)len(cmddat)) {
                return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "invalid path in rpath command"u8, hdr.Path))));
            }
            l.Value.Path = cstring(cmddat.slice((nint)(hdr.Path)));
            l.Value.LoadBytes = ((LoadBytes)cmddat);
            f.Value.Loads = append((~f).Loads, (Load)(new RpathжLoad(l)));
        }
        else if (exprᴛ2 == LoadCmdDylib) {
            ref var hdr = ref heap(new DylibCmd(), out var Ꮡhdr);
            var b = bytes.NewReader(cmddat);
            {
                var errΔ16 = binary.Read(new bytes_ReaderжReader(b), bo, Ꮡhdr); if (errΔ16 != default!) {
                    return (default!, errΔ16);
                }
            }
            var l = @new<Dylib>();
            if (hdr.Name >= (uint32)len(cmddat)) {
                return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "invalid name in dynamic library command"u8, hdr.Name))));
            }
            l.Value.Name = cstring(cmddat.slice((nint)(hdr.Name)));
            l.Value.Time = hdr.Time;
            l.Value.CurrentVersion = hdr.CurrentVersion;
            l.Value.CompatVersion = hdr.CompatVersion;
            l.Value.LoadBytes = ((LoadBytes)cmddat);
            f.Value.Loads = append((~f).Loads, (Load)(new DylibжLoad(l)));
        }
        else if (exprᴛ2 == LoadCmdSymtab) {
            ref var hdr = ref heap(new SymtabCmd(), out var Ꮡhdr);
            var b = bytes.NewReader(cmddat);
            {
                var errΔ17 = binary.Read(new bytes_ReaderжReader(b), bo, Ꮡhdr); if (errΔ17 != default!) {
                    return (default!, errΔ17);
                }
            }
            var (strtab, errΔ18) = saferio.ReadDataAt(r, (uint64)hdr.Strsize, (int64)hdr.Stroff);
            if (errΔ18 != default!) {
                return (default!, errΔ18);
            }
            nint symsz = default!;
            if ((~f).Magic == Magic64){
                symsz = 16;
            } else {
                symsz = 12;
            }
            (var symdat, errΔ18) = saferio.ReadDataAt(r, (uint64)hdr.Nsyms * (uint64)symsz, (int64)hdr.Symoff);
            if (errΔ18 != default!) {
                return (default!, errΔ18);
            }
            (var st, errΔ18) = f.parseSymtab(symdat, strtab, cmddat, Ꮡhdr, offset);
            if (errΔ18 != default!) {
                return (default!, errΔ18);
            }
            f.Value.Loads = append((~f).Loads, (Load)(new SymtabжLoad(st)));
            f.Value.Symtab = st;
        }
        else if (exprᴛ2 == LoadCmdDysymtab) {
            ref var hdr = ref heap(new DysymtabCmd(), out var Ꮡhdr);
            var b = bytes.NewReader(cmddat);
            {
                var errΔ19 = binary.Read(new bytes_ReaderжReader(b), bo, Ꮡhdr); if (errΔ19 != default!) {
                    return (default!, errΔ19);
                }
            }
            if ((~f).Symtab == nil){
                return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "dynamic symbol table seen before any ordinary symbol table"u8, default!))));
            } else 
            if (hdr.Iundefsym > (uint32)len((~(~f).Symtab).Syms)){
                return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, fmt.Sprintf(
                    "undefined symbols index in dynamic symbol table command is greater than symbol table length (%d > %d)"u8,
                    hdr.Iundefsym, len((~(~f).Symtab).Syms)), default!))));
            } else 
            if (hdr.Iundefsym + hdr.Nundefsym > (uint32)len((~(~f).Symtab).Syms)) {
                return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, fmt.Sprintf(
                    "number of undefined symbols after index in dynamic symbol table command is greater than symbol table length (%d > %d)"u8,
                    hdr.Iundefsym + hdr.Nundefsym, len((~(~f).Symtab).Syms)), default!))));
            }
            var (datΔ2, errΔ20) = saferio.ReadDataAt(r, (uint64)hdr.Nindirectsyms * 4, (int64)hdr.Indirectsymoff);
            if (errΔ20 != default!) {
                return (default!, errΔ20);
            }
            var x = new slice<uint32>((nint)(hdr.Nindirectsyms));
            {
                var errΔ21 = binary.Read(new bytes_ReaderжReader(bytes.NewReader(datΔ2)), bo, x); if (errΔ21 != default!) {
                    return (default!, errΔ21);
                }
            }
            var st = @new<Dysymtab>();
            st.Value.LoadBytes = ((LoadBytes)cmddat);
            st.Value.DysymtabCmd = hdr;
            st.Value.IndirectSyms = x;
            f.Value.Loads = append((~f).Loads, (Load)(new DysymtabжLoad(st)));
            f.Value.Dysymtab = st;
        }
        else if (exprᴛ2 == LoadCmdSegment) {
            ref var seg32 = ref heap(new Segment32(), out var Ꮡseg32);
            var b = bytes.NewReader(cmddat);
            {
                var errΔ22 = binary.Read(new bytes_ReaderжReader(b), bo, Ꮡseg32); if (errΔ22 != default!) {
                    return (default!, errΔ22);
                }
            }
            s = @new<ΔSegment>();
            s.Value.LoadBytes = cmddat;
            s.Value.Cmd = cmd;
            s.Value.Len = siz;
            s.Value.Name = cstring(seg32.Name[0..]);
            s.Value.Addr = (uint64)seg32.Addr;
            s.Value.Memsz = (uint64)seg32.Memsz;
            s.Value.Offset = (uint64)seg32.Offset;
            s.Value.Filesz = (uint64)seg32.Filesz;
            s.Value.Maxprot = seg32.Maxprot;
            s.Value.Prot = seg32.Prot;
            s.Value.Nsect = seg32.Nsect;
            s.Value.Flag = seg32.Flag;
            f.Value.Loads = append((~f).Loads, (Load)(new ΔSegmentжLoad(s)));
            for (nint iΔ3 = 0; iΔ3 < (nint)(~s).Nsect; iΔ3++) {
                ref var sh32 = ref heap(new Section32(), out var Ꮡsh32);
                {
                    var errΔ23 = binary.Read(new bytes_ReaderжReader(b), bo, Ꮡsh32); if (errΔ23 != default!) {
                        return (default!, errΔ23);
                    }
                }
                var sh = @new<ΔSection>();
                sh.Value.Name = cstring(sh32.Name[0..]);
                sh.Value.Seg = cstring(sh32.Seg[0..]);
                sh.Value.Addr = (uint64)sh32.Addr;
                sh.Value.Size = (uint64)sh32.Size;
                sh.Value.Offset = sh32.Offset;
                sh.Value.Align = sh32.Align;
                sh.Value.Reloff = sh32.Reloff;
                sh.Value.Nreloc = sh32.Nreloc;
                sh.Value.Flags = sh32.Flags;
                {
                    var errΔ24 = f.pushSection(sh, r); if (errΔ24 != default!) {
                        return (default!, errΔ24);
                    }
                }
            }
        }
        else if (exprᴛ2 == LoadCmdSegment64) {
            ref var seg64 = ref heap(new Segment64(), out var Ꮡseg64);
            var b = bytes.NewReader(cmddat);
            {
                var errΔ25 = binary.Read(new bytes_ReaderжReader(b), bo, Ꮡseg64); if (errΔ25 != default!) {
                    return (default!, errΔ25);
                }
            }
            s = @new<ΔSegment>();
            s.Value.LoadBytes = cmddat;
            s.Value.Cmd = cmd;
            s.Value.Len = siz;
            s.Value.Name = cstring(seg64.Name[0..]);
            s.Value.Addr = seg64.Addr;
            s.Value.Memsz = seg64.Memsz;
            s.Value.Offset = seg64.Offset;
            s.Value.Filesz = seg64.Filesz;
            s.Value.Maxprot = seg64.Maxprot;
            s.Value.Prot = seg64.Prot;
            s.Value.Nsect = seg64.Nsect;
            s.Value.Flag = seg64.Flag;
            f.Value.Loads = append((~f).Loads, (Load)(new ΔSegmentжLoad(s)));
            for (nint iΔ4 = 0; iΔ4 < (nint)(~s).Nsect; iΔ4++) {
                ref var sh64 = ref heap(new Section64(), out var Ꮡsh64);
                {
                    var errΔ26 = binary.Read(new bytes_ReaderжReader(b), bo, Ꮡsh64); if (errΔ26 != default!) {
                        return (default!, errΔ26);
                    }
                }
                var sh = @new<ΔSection>();
                sh.Value.Name = cstring(sh64.Name[0..]);
                sh.Value.Seg = cstring(sh64.Seg[0..]);
                sh.Value.Addr = sh64.Addr;
                sh.Value.Size = sh64.Size;
                sh.Value.Offset = sh64.Offset;
                sh.Value.Align = sh64.Align;
                sh.Value.Reloff = sh64.Reloff;
                sh.Value.Nreloc = sh64.Nreloc;
                sh.Value.Flags = sh64.Flags;
                {
                    var errΔ27 = f.pushSection(sh, r); if (errΔ27 != default!) {
                        return (default!, errΔ27);
                    }
                }
            }
        }
        else { /* default: */
            f.Value.Loads = append((~f).Loads, (Load)(((LoadBytes)cmddat)));
        }

        if (s != nil) {
            if ((int64)(~s).Offset < 0) {
                return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "invalid section offset"u8, (~s).Offset))));
            }
            if ((int64)(~s).Filesz < 0) {
                return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "invalid section file size"u8, (~s).Filesz))));
            }
            s.Value.sr = io.NewSectionReader(r, (int64)(~s).Offset, (int64)(~s).Filesz);
            s.Value.ReaderAt = new io.SectionReaderжReaderAt(s.Value.sr);
        }
    }
    return (f, default!);
}

[GoRecv] internal static (ж<Symtab>, error) parseSymtab(this ref File f, slice<byte> symdat, slice<byte> strtab, slice<byte> cmddat, ж<SymtabCmd> Ꮡhdr, int64 offset) {
    ref var hdr = ref Ꮡhdr.DerefOrNull();

    var bo = f.ByteOrder;
    nint c = saferio.SliceCap<Symbol>((uint64)hdr.Nsyms);
    if (c < 0) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "too many symbols"u8, default!))));
    }
    var symtab = new slice<Symbol>(0, c);
    var b = bytes.NewReader(symdat);
    for (nint i = 0; i < (nint)hdr.Nsyms; i++) {
        ref var n = ref heap(new Nlist64(), out var Ꮡn);
        if (f.Magic == Magic64){
            {
                var err = binary.Read(new bytes_ReaderжReader(b), bo, Ꮡn); if (err != default!) {
                    return (default!, err);
                }
            }
        } else {
            ref var n32 = ref heap(new Nlist32(), out var Ꮡn32);
            {
                var err = binary.Read(new bytes_ReaderжReader(b), bo, Ꮡn32); if (err != default!) {
                    return (default!, err);
                }
            }
            n.Name = n32.Name;
            n.Type = n32.Type;
            n.Sect = n32.Sect;
            n.Desc = n32.Desc;
            n.Value = (uint64)n32.Value;
        }
        if (n.Name >= (uint32)len(strtab)) {
            return (default!, new FormatErrorжerror(Ꮡ(new FormatError(offset, "invalid name in symbol table"u8, n.Name))));
        }
        // We add "_" to Go symbols. Strip it here. See issue 33808.
        @string name = cstring(strtab.slice((nint)(n.Name)));
        if (strings.Contains(name, "."u8) && name[0] == (rune)'_') {
            name = name[1..];
        }
        symtab = append(symtab, new Symbol(
            Name: name,
            Type: n.Type,
            Sect: n.Sect,
            Desc: n.Desc,
            Value: n.Value
        ));
    }
    var st = @new<Symtab>();
    st.Value.LoadBytes = ((LoadBytes)cmddat);
    st.Value.Syms = symtab;
    return (st, default!);
}

[GoType] partial struct relocInfo {
    public uint32 Addr;
    public uint32 Symnum;
}

[GoRecv] internal static error pushSection(this ref File f, ж<ΔSection> Ꮡsh, io.ReaderAt r) {
    ref var sh = ref Ꮡsh.DerefOrNull();

    f.Sections = append(f.Sections, Ꮡsh);
    sh.sr = io.NewSectionReader(r, (int64)sh.Offset, (int64)sh.Size);
    sh.ReaderAt = new io.SectionReaderжReaderAt(sh.sr);
    if (sh.Nreloc > 0) {
        var (reldat, err) = saferio.ReadDataAt(r, (uint64)sh.Nreloc * 8, (int64)sh.Reloff);
        if (err != default!) {
            return err;
        }
        var b = bytes.NewReader(reldat);
        var bo = f.ByteOrder;
        sh.Relocs = new slice<Reloc>((nint)(sh.Nreloc));
        foreach (var (i, _) in sh.Relocs) {
            var rel = Ꮡ(sh.Relocs, i);
            ref var ri = ref heap(new relocInfo(), out var Ꮡri);
            {
                var errΔ1 = binary.Read(new bytes_ReaderжReader(b), bo, Ꮡri); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
            if ((uint32)(ri.Addr & (((uint32)1 << (int)(31)))) != 0){
                // scattered
                rel.Value.Addr = (uint32)(ri.Addr & ((1 << (int)(24)) - 1));
                rel.Value.Type = (uint8)((uint32)(((ri.Addr >> (int)(24))) & ((1 << (int)(4)) - 1)));
                rel.Value.Len = (uint8)((uint32)(((ri.Addr >> (int)(28))) & ((1 << (int)(2)) - 1)));
                rel.Value.Pcrel = (uint32)(ri.Addr & (((uint32)1 << (int)(30)))) != 0;
                rel.Value.Value = ri.Symnum;
                rel.Value.Scattered = true;
            } else {
                var exprᴛ1 = bo;
                if (AreEqual(exprᴛ1, binary.LittleEndian)) {
                    rel.Value.Addr = ri.Addr;
                    rel.Value.Value = (uint32)(ri.Symnum & ((1 << (int)(24)) - 1));
                    rel.Value.Pcrel = (uint32)(ri.Symnum & (((uint32)1 << (int)(24)))) != 0;
                    rel.Value.Len = (uint8)((uint32)(((ri.Symnum >> (int)(25))) & ((1 << (int)(2)) - 1)));
                    rel.Value.Extern = (uint32)(ri.Symnum & (((uint32)1 << (int)(27)))) != 0;
                    rel.Value.Type = (uint8)((uint32)(((ri.Symnum >> (int)(28))) & ((1 << (int)(4)) - 1)));
                }
                else if (AreEqual(exprᴛ1, binary.BigEndian)) {
                    rel.Value.Addr = ri.Addr;
                    rel.Value.Value = (ri.Symnum >> (int)(8));
                    rel.Value.Pcrel = (uint32)(ri.Symnum & (((uint32)1 << (int)(7)))) != 0;
                    rel.Value.Len = (uint8)((uint32)(((ri.Symnum >> (int)(5))) & ((1 << (int)(2)) - 1)));
                    rel.Value.Extern = (uint32)(ri.Symnum & (((uint32)1 << (int)(4)))) != 0;
                    rel.Value.Type = (uint8)((uint32)(ri.Symnum & ((1 << (int)(4)) - 1)));
                }
                else { /* default: */
                    throw panic("unreachable");
                }

            }
        }
    }
    return default!;
}

internal static @string cstring(slice<byte> b) {
    nint i = bytes.IndexByte(b, 0);
    if (i == -1) {
        i = len(b);
    }
    return ((@string)(b.slice(0, i)));
}

// Segment returns the first Segment with the given name, or nil if no such segment exists.
[GoRecv] public static ж<ΔSegment> Segment(this ref File f, @string name) {
    foreach (var (_, l) in f.Loads) {
        {
            var (s, ok) = l._<ж<ΔSegment>>(ᐧ); if (ok && (~s).Name == name) {
                return s;
            }
        }
    }
    return default!;
}

// Section returns the first section with the given name, or nil if no such
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
internal static readonly @string debugˢ = "__debug_"u8;
internal static readonly @string zdebugˢ = "__zdebug_"u8;
internal static readonly @string abbrevˢ = "abbrev"u8;
internal static readonly @string infoˢ = "info"u8;
internal static readonly @string lineˢ = "line"u8;
internal static readonly @string rangesˢ = "ranges"u8;
internal static readonly @string strˢ = "str"u8;

// DWARF returns the DWARF debug information for the Mach-O file.
[GoRecv] public static (ж<dwarf.Data>, error) DWARF(this ref File f) {
    @string dwarfSuffix(ж<ΔSection> s) {
        @string sectname = s.Value.Name;
        nint pfx = default!;
        switch (ᐧ) {
        case {} when strings.HasPrefix(sectname, debugˢ): {
            pfx = 8;
            break;
        }
        case {} when strings.HasPrefix(sectname, zdebugˢ): {
            pfx = 9;
            break;
        }
        default: {
            return ""u8;
        }}

        // Mach-O executables truncate section names to 16 characters, mangling some DWARF sections.
        // As of DWARFv5 these are the only problematic section names (see DWARFv5 Appendix G).
        foreach (var (_, longname) in new @string[]{
            "__debug_str_offsets"u8,
            "__zdebug_line_str"u8,
            "__zdebug_loclists"u8,
            "__zdebug_pubnames"u8,
            "__zdebug_pubtypes"u8,
            "__zdebug_rnglists"u8,
            "__zdebug_str_offsets"u8
        }.slice()) {
            if (sectname == longname[..16]) {
                sectname = longname;
                break;
            }
        }
        return sectname.slice(pfx);
    }
    (slice<byte>, error) sectionData(ж<ΔSection> s) {
        var (b, errΔ1) = s.Data();
        if (errΔ1 != default! && (uint64)len(b) < (~s).Size) {
            return (default!, errΔ1);
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

// ImportedSymbols returns the names of all symbols
// referred to by the binary f that are expected to be
// satisfied by other libraries at dynamic load time.
[GoRecv] public static (slice<@string>, error) ImportedSymbols(this ref File f) {
    if (f.Dysymtab == nil || f.Symtab == nil) {
        return (default!, new FormatErrorжerror(Ꮡ(new FormatError(0, "missing symbol table"u8, default!))));
    }
    var st = f.Symtab;
    var dt = f.Dysymtab;
    slice<@string> all = default!;
    foreach (var (_, s) in (~st).Syms.slice((nint)((~dt).Iundefsym), (nint)((~dt).Iundefsym + (~dt).Nundefsym))) {
        all = append(all, s.Name);
    }
    return (all, default!);
}

// ImportedLibraries returns the paths of all libraries
// referred to by the binary f that are expected to be
// linked with the binary at dynamic link time.
[GoRecv] public static (slice<@string>, error) ImportedLibraries(this ref File f) {
    slice<@string> all = default!;
    foreach (var (_, l) in f.Loads) {
        {
            var (lib, ok) = l._<ж<Dylib>>(ᐧ); if (ok) {
                all = append(all, (~lib).Name);
            }
        }
    }
    return (all, default!);
}

} // end macho_package

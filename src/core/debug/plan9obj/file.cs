// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package plan9obj implements access to Plan 9 a.out object files.

# Security

This package is not designed to be hardened against adversarial inputs, and is
outside the scope of https://go.dev/security/policy. In particular, only basic
validation is done when parsing object files. As such, care should be taken when
parsing untrusted inputs, as parsing malformed files may consume significant
resources, or cause panics.
*/
namespace go.debug;

using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using io = io_package;
using os = os_package;
using @internal;
using encoding;

partial class plan9obj_package {

// A FileHeader represents a Plan 9 a.out file header.
partial struct FileHeader {
    public uint32 Magic;
    public uint32 Bss;
    public uint64 Entry;
    public nint PtrSize;
    public uint64 LoadAddress;
    public uint64 HdrSize;
}

// A File represents an open Plan 9 a.out file.
partial struct File {
    public partial ref FileHeader FileHeader { get; }
    public slice<ж<ΔSection>> Sections;
    internal io.Closer closer;
}

// A SectionHeader represents a single Plan 9 a.out section header.
// This structure doesn't exist on-disk, but eases navigation
// through the object file.
partial struct SectionHeader {
    public @string Name;
    public uint32 Size;
    public uint32 Offset;
}

// A Section represents a single section in a Plan 9 a.out file.
partial struct ΔSection {
    public partial ref SectionHeader SectionHeader { get; }
    // Embed ReaderAt for ReadAt method.
    // Do not embed SectionReader directly
    // to avoid having Read and Seek.
    // If a client wants Read and Seek it must use
    // Open() to avoid fighting over the seek offset
    // with other clients.
    /*embed*/ public io_package.ReaderAt ReaderAt;
    internal ж<io.SectionReader> sr;
}

// Data reads and returns the contents of the Plan 9 a.out section.
public static (slice<byte>, error) Data(this ref ΔSection s) {
    return saferio.ReadDataAt(new io.SectionReaderжReaderAt(s.sr), (uint64)s.Size, 0);
}

// Open returns a new ReadSeeker reading the Plan 9 a.out section.
public static io.ReadSeeker Open(this ref ΔSection s) {
    return new io.SectionReaderжReadSeeker(io.NewSectionReader(new io.SectionReaderжReaderAt(s.sr), 0, 9223372036854775807L));
}

// A Symbol represents an entry in a Plan 9 a.out symbol table section.
partial struct Sym {
    public uint64 Value;
    public rune Type;
    public @string Name;
}

/*
 * Plan 9 a.out reader
 */

// formatError is returned by some operations if the data does
// not have the correct format for an object file.
partial struct formatError {
    internal nint off;
    internal @string msg;
    internal any val;
}

internal static @string Error(this ref formatError e) {
    @string msg = e.msg;
    if (e.val != default!) {
        msg += fmt.Sprintf(" '%v'"u8, e.val);
    }
    msg += fmt.Sprintf(" in record at byte %#x"u8, e.off);
    return msg;
}

// Open opens the named file using [os.Open] and prepares it for use as a Plan 9 a.out binary.
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

internal static (uint32, error) parseMagic(slice<byte> magic) {
    var m = binary.BigEndian.Uint32(magic);
    var exprᴛ1 = m;
    if (exprᴛ1 == Magic386 || exprᴛ1 == MagicAMD64 || exprᴛ1 == MagicARM) {
        return (m, default!);
    }

    return (0, new formatErrorжerror(Ꮡ(new formatError(0, "bad magic number"u8, magic))));
}

internal partial struct NewFile_type /*dyn*/ {
    internal @string name;
    internal uint32 size;
}

// NewFile creates a new [File] for accessing a Plan 9 binary in an underlying reader.
// The Plan 9 binary is expected to start at position 0 in the ReaderAt.
public static (ж<File>, error) NewFile(io.ReaderAt r) {
    var sr = io.NewSectionReader(r, 0, 9223372036854775807L);
    // Read and decode Plan 9 magic
    array<byte> magic = new(4);
    {
        var (_, errΔ1) = r.ReadAt(magic[..], 0); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    var (_, err) = parseMagic(magic[..]);
    if (err != default!) {
        return (default!, err);
    }
    var ph = @new<prog>();
    {
        var errΔ2 = binary.Read(new io.SectionReaderжReader(sr), binary.BigEndian, ph.OrTypedNil()); if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
    }
    var f = Ꮡ(new File(FileHeader: new FileHeader(
        Magic: (~ph).Magic,
        Bss: (~ph).Bss,
        Entry: (uint64)(~ph).Entry,
        PtrSize: 4,
        LoadAddress: 0x1000,
        HdrSize: 4 * 8
    )
    ));
    if ((uint32)((~ph).Magic & (uint32)Magic64) != 0) {
        {
            var errΔ3 = binary.Read(new io.SectionReaderжReader(sr), binary.BigEndian, f.of(File.ᏑEntry)); if (errΔ3 != default!) {
                return (default!, errΔ3);
            }
        }
        f.Value.PtrSize = 8;
        f.Value.LoadAddress = 0x200000;
        f.Value.HdrSize += 8;
    }
    slice<NewFile_type> sects = new NewFile_type[]{
        new("text"u8, (~ph).Text),
        new("data"u8, (~ph).Data),
        new("syms"u8, (~ph).Syms),
        new("spsz"u8, (~ph).Spsz),
        new("pcsz"u8, (~ph).Pcsz)
    }.slice();
    f.Value.Sections = new slice<ж<ΔSection>>(5);
    var off = (uint32)(~f).HdrSize;
    foreach (var (i, sect) in sects) {
        var s = @new<ΔSection>();
        s.Value.SectionHeader = new SectionHeader(
            Name: sect.name,
            Size: sect.size,
            Offset: off
        );
        off += sect.size;
        s.Value.sr = io.NewSectionReader(r, (int64)(~s).Offset, (int64)(~s).Size);
        s.Value.ReaderAt = new io.SectionReaderжReaderAt(s.Value.sr);
        f.Value.Sections[i] = s;
    }
    return (f, default!);
}

internal static error walksymtab(slice<byte> data, nint ptrsz, Func<sym, error> fn) {
    binary.ByteOrder order = binary.BigEndian;
    sym s = default!;
    var p = data;
    while (len(p) >= 4) {
        // Symbol type, value.
        if (len(p) < ptrsz) {
            return new formatErrorжerror(Ꮡ(new formatError(len(data), "unexpected EOF"u8, default!)));
        }
        // fixed-width value
        if (ptrsz == 8){
            s.value = order.Uint64(p[0..8]);
            p = p[8..];
        } else {
            s.value = (uint64)order.Uint32(p[0..4]);
            p = p[4..];
        }
        if (len(p) < 1) {
            return new formatErrorжerror(Ꮡ(new formatError(len(data), "unexpected EOF"u8, default!)));
        }
        var typ = (byte)(p[0] & 0x7F);
        s.typ = typ;
        p = p[1..];
        // Name.
        nint i = default!;
        nint nnul = default!;
        for (i = 0; i < len(p); i++) {
            if (p[i] == 0) {
                nnul = 1;
                break;
            }
        }
        switch (typ) {
        case (rune)'z' or (rune)'Z': {
            p = p.slice(i + nnul);
            for (i = 0; i + 2 <= len(p); i += 2) {
                if (p[i] == 0 && p[i + 1] == 0) {
                    nnul = 2;
                    break;
                }
            }
            break;
        }}

        if (len(p) < i + nnul) {
            return new formatErrorжerror(Ꮡ(new formatError(len(data), "unexpected EOF"u8, default!)));
        }
        s.name = p.slice(0, i);
        i += nnul;
        p = p.slice(i);
        fn(s);
    }
    return default!;
}

// newTable decodes the Go symbol table in data,
// returning an in-memory representation.
internal static (slice<Sym>, error) newTable(slice<byte> symtab, nint ptrsz) {
    nint n = default!;
    var err = walksymtab(symtab, ptrsz, (sym s) => {
        n++;
        return default!;
    });
    if (err != default!) {
        return (default!, err);
    }
    var fname = new map<uint16, @string>();
    ref var syms = ref heap<slice<Sym>>(out var Ꮡsyms);
    syms = new slice<Sym>(0, n);
    var fnameʗ1 = fname;
    err = walksymtab(symtab, ptrsz, error (sym s) => {
        nint nΔ1 = len(Ꮡsyms.ValueSlot);
        Ꮡsyms.ValueSlot = Ꮡsyms.ValueSlot.slice(0, nΔ1 + 1);
        var ts = Ꮡ(Ꮡsyms.ValueSlot, nΔ1);
        ts.Value.Type = (rune)s.typ;
        ts.Value.Value = s.value;
        switch (s.typ) {
        default: {
            ts.Value.Name = ((@string)s.name);
            break;
        }
        case (rune)'z' or (rune)'Z': {
            for (nint i = 0; i < len(s.name); i += 2) {
                ref var eltIdx = ref heap<uint16>(out var ᏑeltIdx);
                eltIdx = binary.BigEndian.Uint16(s.name.slice(i, i + 2));
                var (elt, ok) = fnameʗ1[eltIdx, ꟷ];
                if (!ok) {
                    return new formatErrorжerror(Ꮡ(new formatError(-1, "bad filename code"u8, eltIdx)));
                }
                {
                    nint nΔ3 = len((~ts).Name); if (nΔ3 > 0 && (~ts).Name[nΔ3 - 1] != (rune)'/') {
                        ts.Value.Name += "/"u8;
                    }
                }
                ts.Value.Name += elt;
            }
            break;
        }}

        switch (s.typ) {
        case (rune)'f': {
            fnameʗ1[(uint16)s.value] = ts.Value.Name;
            break;
        }}

        return default!;
    });
    if (err != default!) {
        return (default!, err);
    }
    return (syms, default!);
}

// ErrNoSymbols is returned by [File.Symbols] if there is no such section
// in the File.
public static error ErrNoSymbols = errors.New("no symbol section"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string symsˢ = "syms"u8;
internal static readonly @string cannotLoadSymbolSectionˢ = "cannot load symbol section"u8;

// Symbols returns the symbol table for f.
public static (slice<Sym>, error) Symbols(this ref File f) {
    var symtabSection = f.Section(symsˢ);
    if (symtabSection == nil) {
        return (default!, ErrNoSymbols);
    }
    var (symtab, err) = symtabSection.Data();
    if (err != default!) {
        return (default!, errors.New(cannotLoadSymbolSectionˢ));
    }
    return newTable(symtab, f.PtrSize);
}

// Section returns a section with the given name, or nil if no such
// section exists.
public static ж<ΔSection> Section(this ref File f, @string name) {
    foreach (var (_, s) in f.Sections) {
        if ((~s).Name == name) {
            return s;
        }
    }
    return default!;
}

} // end plan9obj_package

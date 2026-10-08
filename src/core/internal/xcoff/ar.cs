// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using binary = encoding.binary_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using strconv = strconv_package;
using strings = strings_package;
using encoding;

partial class xcoff_package {

public static UntypedInt SAIAMAG => 0x8;
public static readonly @string AIAFMAG = "`\n"u8;
public static readonly @string AIAMAG = "<aiaff>\n"u8;
public static readonly @string AIAMAGBIG = "<bigaf>\n"u8;
public static UntypedInt FL_HSZ_BIG => 0x80;
public static UntypedInt AR_HSZ_BIG => 0x70;

partial struct bigarFileHeader {
    public array<byte> Flmagic = new(SAIAMAG); // Archive magic string
    public array<byte> Flmemoff = new(20);   // Member table offset
    public array<byte> Flgstoff = new(20);   // 32-bits global symtab offset
    public array<byte> Flgst64off = new(20);   // 64-bits global symtab offset
    public array<byte> Flfstmoff = new(20);   // First member offset
    public array<byte> Fllstmoff = new(20);   // Last member offset
    public array<byte> Flfreeoff = new(20);   // First member on free list offset
}

partial struct bigarMemberHeader {
    public array<byte> Arsize = new(20); // File member size
    public array<byte> Arnxtmem = new(20); // Next member pointer
    public array<byte> Arprvmem = new(20); // Previous member pointer
    public array<byte> Ardate = new(12); // File member date
    public array<byte> Aruid = new(12); // File member uid
    public array<byte> Argid = new(12); // File member gid
    public array<byte> Armode = new(12); // File member mode (octal)
    public array<byte> Arnamlen = new(4); // File member name length
}

// _ar_nam is removed because it's easier to get name without it.

// Archive represents an open AIX big archive.
partial struct Archive {
    public partial ref ArchiveHeader ArchiveHeader { get; }
    public slice<ж<Member>> Members;
    internal io.Closer closer;
}

// ArchiveHeader holds information about a big archive file header
partial struct ArchiveHeader {
    internal @string magic;
}

// Member represents a member of an AIX big archive.
partial struct Member {
    public partial ref MemberHeader MemberHeader { get; }
    internal ж<io.SectionReader> sr;
}

// MemberHeader holds information about a big archive member
partial struct MemberHeader {
    public @string Name;
    public uint64 Size;
}

// OpenArchive opens the named archive using os.Open and prepares it for use
// as an AIX big archive.
public static (ж<Archive>, error) OpenArchive(@string name) {
    var (f, err) = os.Open(name);
    if (err != default!) {
        return (default!, err);
    }
    (var arch, err) = NewArchive(new os_FileжReaderAt(f));
    if (err != default!) {
        f.Close();
        return (default!, err);
    }
    arch.Value.closer = new os_FileжCloser(f);
    return (arch, default!);
}

// Close closes the Archive.
// If the Archive was created using NewArchive directly instead of OpenArchive,
// Close has no effect.
public static error Close(this ref Archive a) {
    error err = default!;
    if (a.closer != default!) {
        err = a.closer.Close();
        a.closer = default!;
    }
    return err;
}

// NewArchive creates a new Archive for accessing an AIX big archive in an underlying reader.
public static (ж<Archive>, error) NewArchive(io.ReaderAt r) {
    (int64, error) parseDecimalBytes(slice<byte> b) => strconv.ParseInt(strings.TrimSpace(((@string)b)), 10, 64);
    var sr = io.NewSectionReader(r, 0, 9223372036854775807L);
    // Read File Header
    array<byte> magic = new(8); /* SAIAMAG */
    {
        var (_, errΔ1) = sr.ReadAt(magic[..], 0); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    var arch = @new<Archive>();
    var exprᴛ1 = ((sstring)(magic[..]));
    if (exprᴛ1 == AIAMAGBIG) {
        arch.Value.magic = ((@string)(magic[..]));
    }
    else if (exprᴛ1 == AIAMAG) {
        return (default!, fmt.Errorf("small AIX archive not supported"u8));
    }
    else { /* default: */
        return (default!, fmt.Errorf("unrecognised archive magic: 0x%x"u8, magic));
    }

    ref var fhdr = ref heap(new bigarFileHeader(), out var Ꮡfhdr);
    {
        var (_, errΔ2) = sr.Seek(0, io.SeekStart); if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
    }
    {
        var errΔ3 = binary.Read(new io.SectionReaderжReader(sr), binary.BigEndian, Ꮡfhdr); if (errΔ3 != default!) {
            return (default!, errΔ3);
        }
    }
    var (off, err) = parseDecimalBytes(fhdr.Flfstmoff[..]);
    if (err != default!) {
        return (default!, fmt.Errorf("error parsing offset of first member in archive header(%q); %v"u8, fhdr, err));
    }
    if (off == 0) {
        // Occurs if the archive is empty.
        return (arch, default!);
    }
    (var lastoff, err) = parseDecimalBytes(fhdr.Fllstmoff[..]);
    if (err != default!) {
        return (default!, fmt.Errorf("error parsing offset of first member in archive header(%q); %v"u8, fhdr, err));
    }
    // Read members
    while (ᐧ) {
        // Read Member Header
        // The member header is normally 2 bytes larger. But it's easier
        // to read the name if the header is read without _ar_nam.
        // However, AIAFMAG must be read afterward.
        {
            var (_, errΔ4) = sr.Seek(off, io.SeekStart); if (errΔ4 != default!) {
                return (default!, errΔ4);
            }
        }
        ref var mhdr = ref heap(new bigarMemberHeader(), out var Ꮡmhdr);
        {
            var errΔ5 = binary.Read(new io.SectionReaderжReader(sr), binary.BigEndian, Ꮡmhdr); if (errΔ5 != default!) {
                return (default!, errΔ5);
            }
        }
        var member = @new<Member>();
        arch.Value.Members = append((~arch).Members, member);
        var (size, errΔ6) = parseDecimalBytes(mhdr.Arsize[..]);
        if (errΔ6 != default!) {
            return (default!, fmt.Errorf("error parsing size in member header(%q); %v"u8, mhdr, errΔ6));
        }
        member.Value.Size = (uint64)size;
        // Read name
        (var namlen, errΔ6) = parseDecimalBytes(mhdr.Arnamlen[..]);
        if (errΔ6 != default!) {
            return (default!, fmt.Errorf("error parsing name length in member header(%q); %v"u8, mhdr, errΔ6));
        }
        var name = new slice<byte>((nint)(namlen));
        {
            var errΔ7 = binary.Read(new io.SectionReaderжReader(sr), binary.BigEndian, name); if (errΔ7 != default!) {
                return (default!, errΔ7);
            }
        }
        member.Value.Name = ((@string)name);
        var fileoff = off + (int64)AR_HSZ_BIG + namlen;
        if ((int64)(fileoff & 1) != 0) {
            fileoff++;
            {
                var (_, errΔ8) = sr.Seek(1, io.SeekCurrent); if (errΔ8 != default!) {
                    return (default!, errΔ8);
                }
            }
        }
        // Read AIAFMAG string
        ref var fmag = ref heap(new array<byte>(2), out var Ꮡfmag);
        {
            var errΔ9 = binary.Read(new io.SectionReaderжReader(sr), binary.BigEndian, Ꮡfmag); if (errΔ9 != default!) {
                return (default!, errΔ9);
            }
        }
        if (((sstring)(fmag[..])) != AIAFMAG) {
            return (default!, fmt.Errorf("AIAFMAG not found after member header"u8));
        }
        fileoff += 2; // Add the two bytes of AIAFMAG
        member.Value.sr = io.NewSectionReader(new io.SectionReaderжReaderAt(sr), fileoff, size);
        if (off == lastoff) {
            break;
        }
        (off, errΔ6) = parseDecimalBytes(mhdr.Arnxtmem[..]);
        if (errΔ6 != default!) {
            return (default!, fmt.Errorf("error parsing offset of first member in archive header(%q); %v"u8, fhdr, errΔ6));
        }
    }
    return (arch, default!);
}

// GetFile returns the XCOFF file defined by member name.
// FIXME: This doesn't work if an archive has two members with the same
// name which can occur if an archive has both 32-bits and 64-bits files.
public static (ж<File>, error) GetFile(this ref Archive arch, @string name) {
    foreach (var (_, mem) in arch.Members) {
        if ((~mem).Name == name) {
            return NewFile(new io.SectionReaderжReaderAt((~mem).sr));
        }
    }
    return (default!, fmt.Errorf("unknown member %s in archive"u8, name));
}

} // end xcoff_package

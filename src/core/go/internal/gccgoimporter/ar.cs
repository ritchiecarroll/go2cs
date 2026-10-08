// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.@internal;

using bytes = bytes_package;
using elf = debug.elf_package;
using errors = errors_package;
using fmt = fmt_package;
using xcoff = global::go.@internal.xcoff_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using debug;
using global::go.@internal;

partial class gccgoimporter_package {

// Magic strings for different archive file formats.
internal static readonly @string armag = "!<arch>\n"u8;

internal static readonly @string armagt = "!<thin>\n"u8;

internal static readonly @string armagb = "<bigaf>\n"u8;

// Offsets and sizes for fields in a standard archive header.
internal static UntypedInt arNameOff => 0;

internal static UntypedInt arNameSize => 16;

internal static UntypedInt arDateOff => /* arNameOff + arNameSize */ 16;

internal static UntypedInt arDateSize => 12;

internal static UntypedInt arUIDOff => /* arDateOff + arDateSize */ 28;

internal static UntypedInt arUIDSize => 6;

internal static UntypedInt arGIDOff => /* arUIDOff + arUIDSize */ 34;

internal static UntypedInt arGIDSize => 6;

internal static UntypedInt arModeOff => /* arGIDOff + arGIDSize */ 40;

internal static UntypedInt arModeSize => 8;

internal static UntypedInt arSizeOff => /* arModeOff + arModeSize */ 48;

internal static UntypedInt arSizeSize => 10;

internal static UntypedInt arFmagOff => /* arSizeOff + arSizeSize */ 58;

internal static UntypedInt arFmagSize => 2;

internal static UntypedInt arHdrSize => /* arFmagOff + arFmagSize */ 60;

// The contents of the fmag field of a standard archive header.
internal static readonly @string arfmag = "`\n"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsupportedThinArchiveˢ = "unsupported thin archive"u8;

// arExportData takes an archive file and returns a ReadSeeker for the
// export data in that file. This assumes that there is only one
// object in the archive containing export data, which is not quite
// what gccgo does; gccgo concatenates together all the export data
// for all the objects in the file.  In practice that case does not arise.
internal static (io.ReadSeeker, error) arExportData(io.ReadSeeker archive) {
    {
        var (_, err) = archive.Seek(0, io.SeekStart); if (err != default!) {
            return (default!, err);
        }
    }
    array<byte> buf = new(8); /* len(armag) */
    {
        var (_, err) = archive.Read(buf[..]); if (err != default!) {
            return (default!, err);
        }
    }
    var exprᴛ1 = ((sstring)(buf[..]));
    if (exprᴛ1 == armag) {
        return standardArExportData(archive);
    }
    if (exprᴛ1 == armagt) {
        return (default!, errors.New(unsupportedThinArchiveˢ));
    }
    if (exprᴛ1 == armagb) {
        return aixBigArExportData(archive);
    }
    { /* default: */
        return (default!, fmt.Errorf("unrecognized archive file format %q"u8, buf[..]));
    }

}

// standardArExportData returns export data from a standard archive.
internal static (io.ReadSeeker, error) standardArExportData(io.ReadSeeker archive) {
    var off = (int64)len(armag);
    while (ᐧ) {
        array<byte> hdrBuf = new(60); /* arHdrSize */
        {
            var (_, errΔ1) = archive.Read(hdrBuf[..]); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        off += arHdrSize;
        if (!bytes.Equal(hdrBuf[(int)(arFmagOff)..(int)(arFmagOff + arFmagSize)], slice<byte>(arfmag))) {
            return (default!, fmt.Errorf("archive header format header (%q)"u8, hdrBuf[..]));
        }
        var (size, err) = strconv.ParseInt(strings.TrimSpace(((@string)(hdrBuf[(int)(arSizeOff)..(int)(arSizeOff + arSizeSize)]))), 10, 64);
        if (err != default!) {
            return (default!, fmt.Errorf("error parsing size in archive header (%q): %v"u8, hdrBuf[..], err));
        }
        var fn = hdrBuf[(int)(arNameOff)..(int)(arNameOff + arNameSize)];
        if (fn[0] == (rune)'/' && (fn[1] == (rune)' ' || fn[1] == (rune)'/' || ((sstring)(fn[..8])) == "/SYM64/ "u8)){
        } else {
            // Archive symbol table or extended name table,
            // which we don't care about.
            var archiveAt = readerAtFromSeeker(archive);
            var (ret, errΔ1) = elfFromAr(io.NewSectionReader(archiveAt, off, size));
            if (ret != default! || errΔ1 != default!) {
                return (ret, errΔ1);
            }
        }
        if ((int64)(size & 1) != 0) {
            size++;
        }
        off += size;
        {
            var (_, errΔ2) = archive.Seek(off, io.SeekStart); if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goExportˢ = ".go_export"u8;

// elfFromAr tries to get export data from an archive member as an ELF file.
// If there is no export data, this returns nil, nil.
internal static (io.ReadSeeker, error) elfFromAr(ж<io.SectionReader> Ꮡmember) {
    var (ef, err) = elf.NewFile(new io.SectionReaderжReaderAt(Ꮡmember));
    if (err != default!) {
        return (default!, err);
    }
    var sec = ef.Section(goExportˢ);
    if (sec == nil) {
        return (default!, default!);
    }
    return (sec.Open(), default!);
}

// aixBigArExportData returns export data from an AIX big archive.
internal static (io.ReadSeeker, error) aixBigArExportData(io.ReadSeeker archive) {
    var archiveAt = readerAtFromSeeker(archive);
    var (arch, err) = xcoff.NewArchive(archiveAt);
    if (err != default!) {
        return (default!, err);
    }
    foreach (var (_, mem) in (~arch).Members) {
        var (f, errΔ1) = arch.GetFile((~mem).Name);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        var sdat = f.CSect(goExportˢ);
        if (sdat != default!) {
            return (new bytes_ReaderжReadSeeker(bytes.NewReader(sdat)), default!);
        }
    }
    return (default!, fmt.Errorf(".go_export not found in this archive"u8));
}

// readerAtFromSeeker turns an io.ReadSeeker into an io.ReaderAt.
// This is only safe because there won't be any concurrent seeks
// while this code is executing.
internal static io.ReaderAt readerAtFromSeeker(io.ReadSeeker rs) {
    {
        var (ret, ok) = rs._<io.ReaderAt>(ᐧ); if (ok) {
            return ret;
        }
    }
    return new seekerReadAt(rs);
}

partial struct seekerReadAt {
    internal io.ReadSeeker seeker;
}

internal static (nint, error) ReadAt(this seekerReadAt sra, slice<byte> p, int64 off) {
    {
        var (_, err) = sra.seeker.Seek(off, io.SeekStart); if (err != default!) {
            return (0, err);
        }
    }
    return sra.seeker.Read(p);
}

} // end gccgoimporter_package

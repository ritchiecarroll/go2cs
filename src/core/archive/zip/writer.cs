// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using bufio = bufio_package;
using binary = encoding.binary_package;
using errors = errors_package;
using hash = hash_package;
using crc32 = go.hash.crc32_package;
using io = io_package;
using fs = go.io.fs_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;
using encoding;
using go.hash;
using go.io;
using time = time_package;
using unicode;

partial class zip_package {

internal static error errLongName = errors.New("zip: FileHeader.Name too long"u8);
internal static error errLongExtra = errors.New("zip: FileHeader.Extra too long"u8);

// Writer implements a zip file writer.
partial struct Writer {
    internal ж<countWriter> cw;
    internal slice<ж<header>> dir;
    internal ж<fileWriter> last;
    internal bool closed;
    internal map<uint16, Func<io.Writer, (io.WriteCloser, error)>> compressors;
    internal @string comment;
    // testHookCloseSizeOffset if non-nil is called with the size
    // of offset of the central directory at Close.
    internal Action<uint64, uint64> testHookCloseSizeOffset;
}

partial struct header {
    public partial ref ж<FileHeader> FileHeader { get; }
    internal uint64 offset;
    internal bool raw;
}

// NewWriter returns a new [Writer] writing a zip file to w.
public static ж<Writer> NewWriter(io.Writer w) {
    return Ꮡ(new Writer(cw: Ꮡ(new countWriter(w: new bufio_WriterжWriter(bufio.NewWriter(w))))));
}

// SetOffset sets the offset of the beginning of the zip data within the
// underlying writer. It should be used when the zip data is appended to an
// existing file, such as a binary executable.
// It must be called before any data is written.
public static void SetOffset(this ref Writer w, int64 n) {
    if ((~w.cw).count != 0) {
        throw panic("zip: SetOffset called after data was written");
    }
    w.cw.Value.count = n;
}

// Flush flushes any buffered data to the underlying writer.
// Calling Flush is not normally necessary; calling Close is sufficient.
public static error Flush(this ref Writer w) {
    return (~w.cw).w._<ж<bufio.Writer>>().Flush();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zipWriterCommentTooLongˢ = "zip: Writer.Comment too long"u8;

// SetComment sets the end-of-central-directory comment field.
// It can only be called before [Writer.Close].
public static error SetComment(this ref Writer w, @string comment) {
    if (len(comment) > uint16max) {
        return errors.New(zipWriterCommentTooLongˢ);
    }
    w.comment = comment;
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zipWriterClosedTwiceˢ = "zip: writer closed twice"u8;

// Close finishes writing the zip file by writing the central directory.
// It does not close the underlying writer.
public static error Close(this ref Writer w) {
    if (w.last != nil && !(~w.last).closed) {
        {
            var err = w.last.close(); if (err != default!) {
                return err;
            }
        }
        w.last = default!;
    }
    if (w.closed) {
        return errors.New(zipWriterClosedTwiceˢ);
    }
    w.closed = true;
    // write central directory
    var start = w.cw.Value.count;
    foreach (var (_, h) in w.dir) {
        array<byte> bufΔ1 = new(46); /* directoryHeaderLen */
        var bΔ1 = ((writeBuf)(bufΔ1[..]));
        bΔ1.uint32((uint32)directoryHeaderSignature);
        bΔ1.uint16((~h).CreatorVersion);
        bΔ1.uint16((~h).ReaderVersion);
        bΔ1.uint16((~h).Flags);
        bΔ1.uint16((~h).Method);
        bΔ1.uint16((~h).ModifiedTime);
        bΔ1.uint16((~h).ModifiedDate);
        bΔ1.uint32((~h).CRC32);
        if (h.isZip64() || (~h).offset >= uint32max){
            // the file needs a zip64 header. store maxint in both
            // 32 bit size fields (and offset later) to signal that the
            // zip64 extra header should be used.
            bΔ1.uint32(uint32max); // compressed size
            bΔ1.uint32(uint32max); // uncompressed size
            // append a zip64 extra block to Extra
            array<byte> bufΔ2 = new(28);            // 2x uint16 + 3x uint64
            var eb = ((writeBuf)(bufΔ2[..]));
            eb.uint16(zip64ExtraID);
            eb.uint16(24); // size = 3x uint64
            eb.uint64((~h).UncompressedSize64);
            eb.uint64((~h).CompressedSize64);
            eb.uint64((~h).offset);
            h.Value.Extra = appendꓸꓸꓸ((~h).Extra, bufΔ2[..]);
        } else {
            bΔ1.uint32((~h).CompressedSize);
            bΔ1.uint32((~h).UncompressedSize);
        }
        bΔ1.uint16((uint16)len((~h).Name));
        bΔ1.uint16((uint16)len((~h).Extra));
        bΔ1.uint16((uint16)len((~h).Comment));
        bΔ1 = bΔ1[4..]; // skip disk number start and internal file attr (2x uint16)
        bΔ1.uint32((~h).ExternalAttrs);
        if ((~h).offset > uint32max){
            bΔ1.uint32(uint32max);
        } else {
            bΔ1.uint32((uint32)(~h).offset);
        }
        {
            var (_, err) = w.cw.Write(bufΔ1[..]); if (err != default!) {
                return err;
            }
        }
        {
            var (_, err) = io.WriteString(new countWriterжWriter(w.cw), (~h).Name); if (err != default!) {
                return err;
            }
        }
        {
            var (_, err) = w.cw.Write((~h).Extra); if (err != default!) {
                return err;
            }
        }
        {
            var (_, err) = io.WriteString(new countWriterжWriter(w.cw), (~h).Comment); if (err != default!) {
                return err;
            }
        }
    }
    var end = w.cw.Value.count;
    var records = (uint64)len(w.dir);
    var size = (uint64)(end - start);
    var offset = (uint64)start;
    {
        var f = w.testHookCloseSizeOffset; if (f != default!) {
            f(size, offset);
        }
    }
    if (records >= uint16max || size >= uint32max || offset >= uint32max) {
        array<byte> bufΔ3 = new(76); /* directory64EndLen + directory64LocLen */
        var bΔ2 = ((writeBuf)(bufΔ3[..]));
        // zip64 end of central directory record
        bΔ2.uint32(directory64EndSignature);
        bΔ2.uint64(directory64EndLen - 12); // length minus signature (uint32) and length fields (uint64)
        bΔ2.uint16(zipVersion45); // version made by
        bΔ2.uint16(zipVersion45); // version needed to extract
        bΔ2.uint32(0); // number of this disk
        bΔ2.uint32(0); // number of the disk with the start of the central directory
        bΔ2.uint64(records); // total number of entries in the central directory on this disk
        bΔ2.uint64(records); // total number of entries in the central directory
        bΔ2.uint64(size); // size of the central directory
        bΔ2.uint64(offset); // offset of start of central directory with respect to the starting disk number
        // zip64 end of central directory locator
        bΔ2.uint32(directory64LocSignature);
        bΔ2.uint32(0); // number of the disk with the start of the zip64 end of central directory
        bΔ2.uint64((uint64)end); // relative offset of the zip64 end of central directory record
        bΔ2.uint32(1); // total number of disks
        {
            var (_, err) = w.cw.Write(bufΔ3[..]); if (err != default!) {
                return err;
            }
        }
        // store max values in the regular end record to signal
        // that the zip64 values should be used instead
        records = uint16max;
        size = uint32max;
        offset = uint32max;
    }
    // write end record
    array<byte> buf = new(22); /* directoryEndLen */
    var b = ((writeBuf)(buf[..]));
    b.uint32((uint32)directoryEndSignature);
    b = b[4..]; // skip over disk number and first disk number (2x uint16)
    b.uint16((uint16)records); // number of entries this disk
    b.uint16((uint16)records); // number of entries total
    b.uint32((uint32)size); // size of directory
    b.uint32((uint32)offset); // start of directory
    b.uint16((uint16)len(w.comment)); // byte size of EOCD comment
    {
        var (_, err) = w.cw.Write(buf[..]); if (err != default!) {
            return err;
        }
    }
    {
        var (_, err) = io.WriteString(new countWriterжWriter(w.cw), w.comment); if (err != default!) {
            return err;
        }
    }
    return (~w.cw).w._<ж<bufio.Writer>>().Flush();
}

// Create adds a file to the zip file using the provided name.
// It returns a [Writer] to which the file contents should be written.
// The file contents will be compressed using the [Deflate] method.
// The name must be a relative path: it must not start with a drive
// letter (e.g. C:) or leading slash, and only forward slashes are
// allowed. To create a directory instead of a file, add a trailing
// slash to the name. Duplicate names will not overwrite previous entries
// and are appended to the zip file.
// The file's contents must be written to the [io.Writer] before the next
// call to [Writer.Create], [Writer.CreateHeader], or [Writer.Close].
public static (io.Writer, error) Create(this ref Writer w, @string name) {
    var header = Ꮡ(new FileHeader(
        Name: name,
        Method: Deflate
    ));
    return w.CreateHeader(header);
}

// detectUTF8 reports whether s is a valid UTF-8 string, and whether the string
// must be considered UTF-8 encoding (i.e., not compatible with CP-437, ASCII,
// or any other common encoding).
internal static (bool valid, bool require) detectUTF8(@string s) {
    bool require = default!;

    for (nint i = 0; i < len(s); ) {
        var (r, size) = utf8.DecodeRuneInString(s.slice(i));
        i += size;
        // Officially, ZIP uses CP-437, but many readers use the system's
        // local character encoding. Most encoding are compatible with a large
        // subset of CP-437, which itself is ASCII-like.
        //
        // Forbid 0x7e and 0x5c since EUC-KR and Shift-JIS replace those
        // characters with localized currency and overline characters.
        if (r < 0x20 || r > 0x7d || r == 0x5c) {
            if (!utf8.ValidRune(r) || (r == utf8.RuneError && size == 1)) {
                return (false, false);
            }
            require = true;
        }
    }
    return (true, require);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string archiveZipInvalidˢ = "archive/zip: invalid duplicate FileHeader"u8;

// prepare performs the bookkeeping operations required at the start of
// CreateHeader and CreateRaw.
internal static error prepare(this ref Writer w, ж<FileHeader> Ꮡfh) {
    if (w.last != nil && !(~w.last).closed) {
        {
            var err = w.last.close(); if (err != default!) {
                return err;
            }
        }
    }
    if (len(w.dir) > 0 && (~w.dir[len(w.dir) - 1]).FileHeader == Ꮡfh) {
        // See https://golang.org/issue/11144 confusion.
        return errors.New(archiveZipInvalidˢ);
    }
    return default!;
}

// CreateHeader adds a file to the zip archive using the provided [FileHeader]
// for the file metadata. [Writer] takes ownership of fh and may mutate
// its fields. The caller must not modify fh after calling [Writer.CreateHeader].
//
// This returns a [Writer] to which the file contents should be written.
// The file's contents must be written to the io.Writer before the next
// call to [Writer.Create], [Writer.CreateHeader], [Writer.CreateRaw], or [Writer.Close].
public static (io.Writer, error) CreateHeader(this ref Writer w, ж<FileHeader> Ꮡfh) {
    ref var fh = ref Ꮡfh.DerefOrNull();

    {
        var err = w.prepare(Ꮡfh); if (err != default!) {
            return (default!, err);
        }
    }
    // The ZIP format has a sad state of affairs regarding character encoding.
    // Officially, the name and comment fields are supposed to be encoded
    // in CP-437 (which is mostly compatible with ASCII), unless the UTF-8
    // flag bit is set. However, there are several problems:
    //
    //	* Many ZIP readers still do not support UTF-8.
    //	* If the UTF-8 flag is cleared, several readers simply interpret the
    //	name and comment fields as whatever the local system encoding is.
    //
    // In order to avoid breaking readers without UTF-8 support,
    // we avoid setting the UTF-8 flag if the strings are CP-437 compatible.
    // However, if the strings require multibyte UTF-8 encoding and is a
    // valid UTF-8 string, then we set the UTF-8 bit.
    //
    // For the case, where the user explicitly wants to specify the encoding
    // as UTF-8, they will need to set the flag bit themselves.
    var (utf8Valid1, utf8Require1) = detectUTF8(fh.Name);
    var (utf8Valid2, utf8Require2) = detectUTF8(fh.Comment);
    switch (ᐧ) {
    case {} when fh.NonUTF8: {
        fh.Flags &= unchecked((uint16)~(uint16)(0x800));
        break;
    }
    case {} when (utf8Require1 || utf8Require2) && (utf8Valid1 && utf8Valid2): {
        fh.Flags |= (uint16)(0x800);
        break;
    }}

    fh.CreatorVersion = (uint16)((uint16)(fh.CreatorVersion & 0xff00) | (uint16)zipVersion20); // preserve compatibility byte
    fh.ReaderVersion = zipVersion20;
    // If Modified is set, this takes precedence over MS-DOS timestamp fields.
    if (!fh.Modified.IsZero()) {
        // Contrary to the FileHeader.SetModTime method, we intentionally
        // do not convert to UTC, because we assume the user intends to encode
        // the date using the specified timezone. A user may want this control
        // because many legacy ZIP readers interpret the timestamp according
        // to the local timezone.
        //
        // The timezone is only non-UTC if a user directly sets the Modified
        // field directly themselves. All other approaches sets UTC.
        (fh.ModifiedDate, fh.ModifiedTime) = timeToMsDosTime(fh.Modified);
        // Use "extended timestamp" format since this is what Info-ZIP uses.
        // Nearly every major ZIP implementation uses a different format,
        // but at least most seem to be able to understand the other formats.
        //
        // This format happens to be identical for both local and central header
        // if modification time is the only timestamp being encoded.
        array<byte> mbuf = new(9);            // 2*SizeOf(uint16) + SizeOf(uint8) + SizeOf(uint32)
        var mt = (uint32)fh.Modified.Unix();
        var eb = ((writeBuf)(mbuf[..]));
        eb.uint16(extTimeExtraID);
        eb.uint16(5); // Size: SizeOf(uint8) + SizeOf(uint32)
        eb.uint8(1); // Flags: ModTime
        eb.uint32(mt); // ModTime
        fh.Extra = appendꓸꓸꓸ(fh.Extra, mbuf[..]);
    }
    io.Writer ow = default!;
    ж<fileWriter> fw = default!;
    var h = Ꮡ(new header(
        FileHeader: Ꮡfh,
        offset: (uint64)(~w.cw).count
    ));
    if (strings.HasSuffix(fh.Name, "/"u8)){
        // Set the compression method to Store to ensure data length is truly zero,
        // which the writeHeader method always encodes for the size fields.
        // This is necessary as most compression formats have non-zero lengths
        // even when compressing an empty string.
        fh.Method = Store;
        fh.Flags &= unchecked((uint16)~(uint16)(0x8)); // we will not write a data descriptor
        // Explicitly clear sizes as they have no meaning for directories.
        fh.CompressedSize = 0;
        fh.CompressedSize64 = 0;
        fh.UncompressedSize = 0;
        fh.UncompressedSize64 = 0;
        ow = new dirWriter(nil);
    } else {
        fh.Flags |= (uint16)(0x8); // we will write a data descriptor
        fw = Ꮡ(new fileWriter(
            zipw: new countWriterжWriter(w.cw),
            compCount: Ꮡ(new countWriter(w: new countWriterжWriter(w.cw))),
            crc32: crc32.NewIEEE()
        ));
        var comp = w.compressor(fh.Method);
        if (comp == default!) {
            return (default!, ErrAlgorithm);
        }
        error err = default!;
        (fw.Value.comp, err) = comp(new countWriterжWriter((~fw).compCount));
        if (err != default!) {
            return (default!, err);
        }
        fw.Value.rawCount = Ꮡ(new countWriter(w: (~fw).comp));
        fw.Value.header = h;
        ow = new fileWriterжWriter(fw);
    }
    w.dir = append(w.dir, h);
    {
        var err = writeHeader(new countWriterжWriter(w.cw), h); if (err != default!) {
            return (default!, err);
        }
    }
    // If we're creating a directory, fw is nil.
    w.last = fw;
    return (ow, default!);
}

internal static error writeHeader(io.Writer w, ж<header> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    const nint maxUint16 = /* 1<<16 - 1 */ 65535;
    if (len(h.Name) > maxUint16) {
        return errLongName;
    }
    if (len(h.Extra) > maxUint16) {
        return errLongExtra;
    }
    array<byte> buf = new(30); /* fileHeaderLen */
    var b = ((writeBuf)(buf[..]));
    b.uint32((uint32)fileHeaderSignature);
    b.uint16(h.ReaderVersion);
    b.uint16(h.Flags);
    b.uint16(h.Method);
    b.uint16(h.ModifiedTime);
    b.uint16(h.ModifiedDate);
    // In raw mode (caller does the compression), the values are either
    // written here or in the trailing data descriptor based on the header
    // flags.
    if (h.raw && !h.hasDataDescriptor()){
        b.uint32(h.CRC32);
        b.uint32((uint32)min(h.CompressedSize64, (uint64)(uint32max)));
        b.uint32((uint32)min(h.UncompressedSize64, (uint64)(uint32max)));
    } else {
        // When this package handle the compression, these values are
        // always written to the trailing data descriptor.
        b.uint32(0); // crc32
        b.uint32(0); // compressed size
        b.uint32(0); // uncompressed size
    }
    b.uint16((uint16)len(h.Name));
    b.uint16((uint16)len(h.Extra));
    {
        var (_, errΔ1) = w.Write(buf[..]); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        var (_, errΔ2) = io.WriteString(w, h.Name); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    var (_, err) = w.Write(h.Extra);
    return err;
}

// CreateRaw adds a file to the zip archive using the provided [FileHeader] and
// returns a [Writer] to which the file contents should be written. The file's
// contents must be written to the io.Writer before the next call to [Writer.Create],
// [Writer.CreateHeader], [Writer.CreateRaw], or [Writer.Close].
//
// In contrast to [Writer.CreateHeader], the bytes passed to Writer are not compressed.
//
// CreateRaw's argument is stored in w. If the argument is a pointer to the embedded
// [FileHeader] in a [File] obtained from a [Reader] created from in-memory data,
// then w will refer to all of that memory.
public static (io.Writer, error) CreateRaw(this ref Writer w, ж<FileHeader> Ꮡfh) {
    ref var fh = ref Ꮡfh.DerefOrNull();

    {
        var err = w.prepare(Ꮡfh); if (err != default!) {
            return (default!, err);
        }
    }
    fh.CompressedSize = (uint32)min(fh.CompressedSize64, (uint64)(uint32max));
    fh.UncompressedSize = (uint32)min(fh.UncompressedSize64, (uint64)(uint32max));
    var h = Ꮡ(new header(
        FileHeader: Ꮡfh,
        offset: (uint64)(~w.cw).count,
        raw: true
    ));
    w.dir = append(w.dir, h);
    {
        var err = writeHeader(new countWriterжWriter(w.cw), h); if (err != default!) {
            return (default!, err);
        }
    }
    if (strings.HasSuffix(fh.Name, "/"u8)) {
        w.last = default!;
        return (new dirWriter(nil), default!);
    }
    var fw = Ꮡ(new fileWriter(
        header: h,
        zipw: new countWriterжWriter(w.cw)
    ));
    w.last = fw;
    return (new fileWriterжWriter(fw), default!);
}

// Copy copies the file f (obtained from a [Reader]) into w. It copies the raw
// form directly bypassing decompression, compression, and validation.
public static error Copy(this ref Writer w, ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    var (r, err) = f.OpenRaw();
    if (err != default!) {
        return err;
    }
    // Copy the FileHeader so w doesn't store a pointer to the data
    // of f's entire archive. See #65499.
    ref var fh = ref heap<FileHeader>(out var Ꮡfh);
    fh = f.FileHeader;
    (var fw, err) = w.CreateRaw(Ꮡfh);
    if (err != default!) {
        return err;
    }
    (_, err) = io.Copy(fw, r);
    return err;
}

// RegisterCompressor registers or overrides a custom compressor for a specific
// method ID. If a compressor for a given method is not found, [Writer] will
// default to looking up the compressor at the package level.
public static void RegisterCompressor(this ref Writer w, uint16 method, Func<io.Writer, (io.WriteCloser, error)> comp) {
    if (w.compressors == default!) {
        w.compressors = new map<uint16, Func<io.Writer, (io.WriteCloser, error)>>();
    }
    w.compressors[method] = comp;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zipCannotAddNonRegularˢ = "zip: cannot add non-regular file"u8;

// AddFS adds the files from fs.FS to the archive.
// It walks the directory tree starting at the root of the filesystem
// adding each file to the zip using deflate while maintaining the directory structure.
public static error AddFS(this ж<Writer> Ꮡw, fs.FS fsys) {
    return fs.WalkDir(fsys, "."u8, error (@string name, fs.DirEntry d, error err) => {
        GoFrame ᒐ = default;
        try {
            if (err != default!) {
                return err;
            }
            if (name == "."u8) {
                return default!;
            }
            (var info, err) = d.Info();
            if (err != default!) {
                return err;
            }
            if (!d.IsDir() && !info.Mode().IsRegular()) {
                return errors.New(zipCannotAddNonRegularˢ);
            }
            (var h, err) = FileInfoHeader(info);
            if (err != default!) {
                return err;
            }
            h.Value.Name = name;
            if (d.IsDir()) {
                h.Value.Name += "/"u8;
            }
            h.Value.Method = Deflate;
            (var fw, err) = Ꮡw.Value.CreateHeader(h);
            if (err != default!) {
                return err;
            }
            if (d.IsDir()) {
                return default!;
            }
            (var f, err) = fsys.Open(name);
            if (err != default!) {
                return err;
            }
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);
            (_, err) = io.Copy(fw, new fs_FileᴠReader(f));
            return err;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
}

internal static Func<io.Writer, (io.WriteCloser, error)> compressor(this ref Writer w, uint16 method) {
    var comp = w.compressors[method];
    if (comp == default!) {
        comp = compressor(method);
    }
    return comp;
}

partial struct dirWriter {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zipWriteToDirectoryˢ = "zip: write to directory"u8;

internal static (nint, error) Write(this dirWriter _, slice<byte> b) {
    if (len(b) == 0) {
        return (0, default!);
    }
    return (0, errors.New(zipWriteToDirectoryˢ));
}

partial struct fileWriter {
    internal partial ref ж<header> header { get; }
    internal io.Writer zipw;
    internal ж<countWriter> rawCount;
    internal io.WriteCloser comp;
    internal ж<countWriter> compCount;
    internal hash.Hash32 crc32;
    internal bool closed;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zipWriteToClosedFileˢ = "zip: write to closed file"u8;

internal static (nint, error) Write(this ref fileWriter w, slice<byte> p) {
    if (w.closed) {
        return (0, errors.New(zipWriteToClosedFileˢ));
    }
    if (w.raw) {
        return w.zipw.Write(p);
    }
    w.crc32.Write(p);
    return w.rawCount.Write(p);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zipFileClosedTwiceˢ = "zip: file closed twice"u8;

internal static error close(this ref fileWriter w) {
    if (w.closed) {
        return errors.New(zipFileClosedTwiceˢ);
    }
    w.closed = true;
    if (w.raw) {
        return w.writeDataDescriptor();
    }
    {
        var err = w.comp.Close(); if (err != default!) {
            return err;
        }
    }
    // update FileHeader
    var fh = w.header.Value.FileHeader;
    fh.Value.CRC32 = w.crc32.Sum32();
    fh.Value.CompressedSize64 = (uint64)(~w.compCount).count;
    fh.Value.UncompressedSize64 = (uint64)(~w.rawCount).count;
    if (fh.isZip64()){
        fh.Value.CompressedSize = uint32max;
        fh.Value.UncompressedSize = uint32max;
        fh.Value.ReaderVersion = zipVersion45; // requires 4.5 - File uses ZIP64 format extensions
    } else {
        fh.Value.CompressedSize = (uint32)(~fh).CompressedSize64;
        fh.Value.UncompressedSize = (uint32)(~fh).UncompressedSize64;
    }
    return w.writeDataDescriptor();
}

internal static error writeDataDescriptor(this ref fileWriter w) {
    if (!w.hasDataDescriptor()) {
        return default!;
    }
    // Write data descriptor. This is more complicated than one would
    // think, see e.g. comments in zipfile.c:putextended() and
    // https://bugs.openjdk.org/browse/JDK-7073588.
    // The approach here is to write 8 byte sizes if needed without
    // adding a zip64 extra in the local header (too late anyway).
    slice<byte> buf = default!;
    if (w.isZip64()){
        buf = new slice<byte>(dataDescriptor64Len);
    } else {
        buf = new slice<byte>(dataDescriptorLen);
    }
    var b = ((writeBuf)buf);
    b.uint32(dataDescriptorSignature); // de-facto standard, required by OS X
    b.uint32(w.CRC32);
    if (w.isZip64()){
        b.uint64(w.CompressedSize64);
        b.uint64(w.UncompressedSize64);
    } else {
        b.uint32(w.CompressedSize);
        b.uint32(w.UncompressedSize);
    }
    var (_, err) = w.zipw.Write(buf);
    return err;
}

partial struct countWriter {
    internal io.Writer w;
    internal int64 count;
}

internal static (nint, error) Write(this ref countWriter w, slice<byte> p) {
    var (n, err) = w.w.Write(p);
    w.count += (int64)n;
    return (n, err);
}

partial struct nopCloser {
    /*embed*/ public io_package.Writer Writer;
}

internal static error Close(this nopCloser w) {
    return default!;
}

partial struct writeBuf /*[]byte*/;

internal static void uint8(this ref writeBuf b, uint8 v) {
    (b)[0] = v;
    b = (b)[1..];
}

internal static void uint16(this ref writeBuf b, uint16 v) {
    binary.LittleEndian.PutUint16(b, v);
    b = (b)[2..];
}

internal static void uint32(this ref writeBuf b, uint32 v) {
    binary.LittleEndian.PutUint32(b, v);
    b = (b)[4..];
}

internal static void uint64(this ref writeBuf b, uint64 v) {
    binary.LittleEndian.PutUint64(b, v);
    b = (b)[8..];
}

} // end zip_package

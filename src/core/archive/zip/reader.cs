// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using bufio = bufio_package;
using binary = encoding.binary_package;
using errors = errors_package;
using hash = hash_package;
using crc32 = go.hash.crc32_package;
using godebug = @internal.godebug_package;
using io = io_package;
using fs = go.io.fs_package;
using os = os_package;
using path = path_package;
using filepath = go.path.filepath_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using @internal;
using encoding;
using go.hash;
using go.io;
using go.path;

partial class zip_package {

internal static ж<godebug.Setting> zipinsecurepath = godebug.New("zipinsecurepath"u8);

public static error ErrFormat = errors.New("zip: not a valid zip file"u8);
public static error ErrAlgorithm = errors.New("zip: unsupported compression algorithm"u8);
public static error ErrChecksum = errors.New("zip: checksum error"u8);
public static error ErrInsecurePath = errors.New("zip: insecure file path"u8);

// A Reader serves content from a ZIP archive.
[GoType] partial struct Reader {
    internal io.ReaderAt r;
    public slice<ж<File>> File;
    public @string Comment;
    internal map<uint16, Func<io.Reader, io.ReadCloser>> decompressors;
    // Some JAR files are zip files with a prefix that is a bash script.
    // The baseOffset field is the start of the zip file proper.
    internal int64 baseOffset;
    // fileList is a list of files sorted by ename,
    // for use by the Open method.
    internal sync.Once fileListOnce;
    internal slice<fileListEntry> fileList;
}

// A ReadCloser is a [Reader] that must be closed when no longer needed.
[GoType] partial struct ReadCloser {
    internal ж<os.File> f;
    public partial ref Reader Reader { get; }
}

// A File is a single file in a ZIP archive.
// The file information is in the embedded [FileHeader].
// The file content can be accessed by calling [File.Open].
[GoType] partial struct File {
    public partial ref FileHeader FileHeader { get; }
    internal ж<Reader> zip;
    internal io.ReaderAt zipr;
    internal int64 headerOffset; // includes overall ZIP archive baseOffset
    internal bool zip64;  // zip64 extended information extra field presence
}

// OpenReader will open the Zip file specified by name and return a ReadCloser.
//
// If any file inside the archive uses a non-local name
// (as defined by [filepath.IsLocal]) or a name containing backslashes
// and the GODEBUG environment variable contains `zipinsecurepath=0`,
// OpenReader returns the reader with an ErrInsecurePath error.
// A future version of Go may introduce this behavior by default.
// Programs that want to accept non-local names can ignore
// the ErrInsecurePath error and use the returned reader.
public static (ж<ReadCloser>, error) OpenReader(@string name) {
    var (f, err) = os.Open(name);
    if (err != default!) {
        return (default!, err);
    }
    (var fi, err) = f.Stat();
    if (err != default!) {
        f.Close();
        return (default!, err);
    }
    var r = @new<ReadCloser>();
    {
        err = r.of(ReadCloser.ᏑReader).init(new os_FileжReaderAt(f), fi.Size()); if (err != default! && !AreEqual(err, ErrInsecurePath)) {
            f.Close();
            return (default!, err);
        }
    }
    r.Value.f = f;
    return (r, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zipSizeCannotBeNegativeˢ = "zip: size cannot be negative"u8;

// NewReader returns a new [Reader] reading from r, which is assumed to
// have the given size in bytes.
//
// If any file inside the archive uses a non-local name
// (as defined by [filepath.IsLocal]) or a name containing backslashes
// and the GODEBUG environment variable contains `zipinsecurepath=0`,
// NewReader returns the reader with an [ErrInsecurePath] error.
// A future version of Go may introduce this behavior by default.
// Programs that want to accept non-local names can ignore
// the [ErrInsecurePath] error and use the returned reader.
public static (ж<Reader>, error) NewReader(io.ReaderAt r, int64 size) {
    if (size < 0) {
        return (default!, errors.New(zipSizeCannotBeNegativeˢ));
    }
    var zr = @new<Reader>();
    error err = default!;
    {
        err = zr.init(r, size); if (err != default! && !AreEqual(err, ErrInsecurePath)) {
            return (default!, err);
        }
    }
    return (zr, err);
}

internal static error init(this ж<Reader> Ꮡr, io.ReaderAt rdr, int64 size) {
    ref var r = ref Ꮡr.DerefOrNull();

    var (end, baseOffset, err) = readDirectoryEnd(rdr, size);
    if (err != default!) {
        return err;
    }
    r.r = rdr;
    r.baseOffset = baseOffset;
    // Since the number of directory records is not validated, it is not
    // safe to preallocate r.File without first checking that the specified
    // number of files is reasonable, since a malformed archive may
    // indicate it contains up to 1 << 128 - 1 files. Since each file has a
    // header which will be _at least_ 30 bytes we can safely preallocate
    // if (data size / 30) >= end.directoryRecords.
    if ((~end).directorySize < (uint64)size && ((uint64)size - (~end).directorySize) / 30 >= (~end).directoryRecords) {
        r.File = new slice<ж<File>>(0, (nint)((~end).directoryRecords));
    }
    r.Comment = end.Value.comment;
    var rs = io.NewSectionReader(rdr, 0, size);
    {
        (_, err) = rs.Seek(r.baseOffset + (int64)(~end).directoryOffset, io.SeekStart); if (err != default!) {
            return err;
        }
    }
    var buf = bufio.NewReader(new io.SectionReaderжReader(rs));
    // The count of files inside a zip is truncated to fit in a uint16.
    // Gloss over this by reading headers until we encounter
    // a bad one, and then only report an ErrFormat or UnexpectedEOF if
    // the file count modulo 65536 is incorrect.
    while (ᐧ) {
        var f = Ꮡ(new File(zip: Ꮡr, zipr: rdr));
        err = readDirectoryHeader(ref (f).DerefOrNull(), new bufio_ReaderжReader(buf));
        if (AreEqual(err, ErrFormat) || AreEqual(err, io.ErrUnexpectedEOF)) {
            break;
        }
        if (err != default!) {
            return err;
        }
        f.Value.headerOffset += r.baseOffset;
        r.File = append(r.File, f);
    }
    if ((uint16)len(r.File) != (uint16)(~end).directoryRecords) {
        // only compare 16 bits here
        // Return the readDirectoryHeader error if we read
        // the wrong number of directory entries.
        return err;
    }
    if (zipinsecurepath.Value() == "0"u8) {
        foreach (var (_, f) in r.File) {
            if ((~f).Name == ""u8) {
                // Zip permits an empty file name field.
                continue;
            }
            // The zip specification states that names must use forward slashes,
            // so consider any backslashes in the name insecure.
            if (!filepath.IsLocal((~f).Name) || strings.Contains((~f).Name, @"\"u8)) {
                zipinsecurepath.IncNonDefault();
                return ErrInsecurePath;
            }
        }
    }
    return default!;
}

// RegisterDecompressor registers or overrides a custom decompressor for a
// specific method ID. If a decompressor for a given method is not found,
// [Reader] will default to looking up the decompressor at the package level.
[GoRecv] public static void RegisterDecompressor(this ref Reader r, uint16 method, Func<io.Reader, io.ReadCloser> dcomp) {
    if (r.decompressors == default!) {
        r.decompressors = new map<uint16, Func<io.Reader, io.ReadCloser>>();
    }
    r.decompressors[method] = dcomp;
}

[GoRecv] internal static Func<io.Reader, io.ReadCloser> decompressor(this ref Reader r, uint16 method) {
    var dcomp = r.decompressors[method];
    if (dcomp == default!) {
        dcomp = decompressor(method);
    }
    return dcomp;
}

// Close closes the Zip file, rendering it unusable for I/O.
[GoRecv] public static error Close(this ref ReadCloser rc) {
    return rc.f.Close();
}

// DataOffset returns the offset of the file's possibly-compressed
// data, relative to the beginning of the zip file.
//
// Most callers should instead use [File.Open], which transparently
// decompresses data and verifies checksums.
[GoRecv] public static (int64 offset, error err) DataOffset(this ref File f) {
    int64 offset = default!;
    error err = default!;

    (var bodyOffset, err) = f.findBodyOffset();
    if (err != default!) {
        return (offset, err);
    }
    return (f.headerOffset + bodyOffset, default!);
}

// Open returns a [ReadCloser] that provides access to the [File]'s contents.
// Multiple files may be read concurrently.
public static (io.ReadCloser, error) Open(this ж<File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    var (bodyOffset, err) = f.findBodyOffset();
    if (err != default!) {
        return (default!, err);
    }
    if (strings.HasSuffix(f.Name, "/"u8)) {
        // The ZIP specification (APPNOTE.TXT) specifies that directories, which
        // are technically zero-byte files, must not have any associated file
        // data. We previously tried failing here if f.CompressedSize64 != 0,
        // but it turns out that a number of implementations (namely, the Java
        // jar tool) don't properly set the storage method on directories
        // resulting in a file with compressed size > 0 but uncompressed size ==
        // 0. We still want to fail when a directory has associated uncompressed
        // data, but we are tolerant of cases where the uncompressed size is
        // zero but compressed size is not.
        if (f.UncompressedSize64 != 0){
            return (new dirReaderжReadCloser(Ꮡ(new dirReader(ErrFormat))), default!);
        } else {
            return (new dirReaderжReadCloser(Ꮡ(new dirReader(io.EOF))), default!);
        }
    }
    var size = (int64)f.CompressedSize64;
    var r = io.NewSectionReader(f.zipr, f.headerOffset + bodyOffset, size);
    var dcomp = f.zip.decompressor(f.Method);
    if (dcomp == default!) {
        return (default!, ErrAlgorithm);
    }
    io.ReadCloser rc = dcomp(new io.SectionReaderжReader(r));
    io.Reader desr = default!;
    if (Ꮡf.of(File.ᏑFileHeader).hasDataDescriptor()) {
        desr = new io.SectionReaderжReader(io.NewSectionReader(f.zipr, f.headerOffset + bodyOffset + size, dataDescriptorLen));
    }
    rc = new checksumReaderжReadCloser(Ꮡ(new checksumReader(
        rc: rc,
        hash: crc32.NewIEEE(),
        f: Ꮡf,
        desr: desr
    )));
    return (rc, default!);
}

// OpenRaw returns a [Reader] that provides access to the [File]'s contents without
// decompression.
[GoRecv] public static (io.Reader, error) OpenRaw(this ref File f) {
    var (bodyOffset, err) = f.findBodyOffset();
    if (err != default!) {
        return (default!, err);
    }
    var r = io.NewSectionReader(f.zipr, f.headerOffset + bodyOffset, (int64)f.CompressedSize64);
    return (new io.SectionReaderжReader(r), default!);
}

[GoType] partial struct dirReader {
    internal error err;
}

[GoRecv] internal static (nint, error) Read(this ref dirReader r, slice<byte> _) {
    return (0, r.err);
}

[GoRecv] internal static error Close(this ref dirReader r) {
    return default!;
}

[GoType] partial struct checksumReader {
    internal io.ReadCloser rc;
    internal hash.Hash32 hash;
    internal uint64 nread; // number of bytes read so far
    internal ж<File> f;
    internal io.Reader desr; // if non-nil, where to read the data descriptor
    internal error err;     // sticky error
}

[GoRecv] internal static (fs.FileInfo, error) Stat(this ref checksumReader r) {
    return (new headerFileInfo(r.f.of(File.ᏑFileHeader)), default!);
}

[GoRecv] internal static (nint n, error err) Read(this ref checksumReader r, slice<byte> b) {
    nint n = default!;
    error err = default!;

    if (r.err != default!) {
        return (0, r.err);
    }
    (n, err) = r.rc.Read(b);
    r.hash.Write(b.slice(0, n));
    r.nread += (uint64)n;
    if (r.nread > (~r.f).UncompressedSize64) {
        return (0, ErrFormat);
    }
    if (err == default!) {
        return (n, err);
    }
    if (AreEqual(err, io.EOF)) {
        if (r.nread != (~r.f).UncompressedSize64) {
            return (0, io.ErrUnexpectedEOF);
        }
        if (r.desr != default!){
            {
                var err1 = readDataDescriptor(r.desr, ref (r.f).DerefOrNull()); if (err1 != default!){
                    if (AreEqual(err1, io.EOF)){
                        err = io.ErrUnexpectedEOF;
                    } else {
                        err = err1;
                    }
                } else 
                if (r.hash.Sum32() != (~r.f).CRC32) {
                    err = ErrChecksum;
                }
            }
        } else {
            // If there's not a data descriptor, we still compare
            // the CRC32 of what we've read against the file header
            // or TOC's CRC32, if it seems like it was set.
            if ((~r.f).CRC32 != 0 && r.hash.Sum32() != (~r.f).CRC32) {
                err = ErrChecksum;
            }
        }
    }
    r.err = err;
    return (n, err);
}

[GoRecv] internal static error Close(this ref checksumReader r) {
    return r.rc.Close();
}

// findBodyOffset does the minimum work to verify the file has a header
// and returns the file body offset.
[GoRecv] internal static (int64, error) findBodyOffset(this ref File f) {
    array<byte> buf = new(30); /* fileHeaderLen */
    {
        var (_, err) = f.zipr.ReadAt(buf[..], f.headerOffset); if (err != default!) {
            return (0, err);
        }
    }
    var b = ((readBuf)(buf[..]));
    {
        var sig = b.uint32(); if (sig != fileHeaderSignature) {
            return (0, ErrFormat);
        }
    }
    b = b[22..]; // skip over most of the header
    nint filenameLen = (nint)b.uint16();
    nint extraLen = (nint)b.uint16();
    return ((int64)((nint)fileHeaderLen + filenameLen + extraLen), default!);
}

// readDirectoryHeader attempts to read a directory header from r.
// It returns io.ErrUnexpectedEOF if it cannot read a complete header,
// and ErrFormat if it doesn't find a valid header signature.
internal static error readDirectoryHeader(ref File f, io.Reader r) {
    array<byte> buf = new(46); /* directoryHeaderLen */
    {
        var (_, err) = io.ReadFull(r, buf[..]); if (err != default!) {
            return err;
        }
    }
    var b = ((readBuf)(buf[..]));
    {
        var sig = b.uint32(); if (sig != directoryHeaderSignature) {
            return ErrFormat;
        }
    }
    f.CreatorVersion = b.uint16();
    f.ReaderVersion = b.uint16();
    f.Flags = b.uint16();
    f.Method = b.uint16();
    f.ModifiedTime = b.uint16();
    f.ModifiedDate = b.uint16();
    f.CRC32 = b.uint32();
    f.CompressedSize = b.uint32();
    f.UncompressedSize = b.uint32();
    f.CompressedSize64 = (uint64)f.CompressedSize;
    f.UncompressedSize64 = (uint64)f.UncompressedSize;
    nint filenameLen = (nint)b.uint16();
    nint extraLen = (nint)b.uint16();
    nint commentLen = (nint)b.uint16();
    b = b[4..]; // skipped start disk number and internal attributes (2x uint16)
    f.ExternalAttrs = b.uint32();
    f.headerOffset = (int64)b.uint32();
    var d = new slice<byte>(filenameLen + extraLen + commentLen);
    {
        var (_, err) = io.ReadFull(r, d); if (err != default!) {
            return err;
        }
    }
    f.Name = ((@string)(d.slice(0, filenameLen)));
    f.Extra = d.slice(filenameLen, filenameLen + extraLen);
    f.Comment = ((@string)(d.slice(filenameLen + extraLen)));
    // Determine the character encoding.
    var (utf8Valid1, utf8Require1) = detectUTF8(f.Name);
    var (utf8Valid2, utf8Require2) = detectUTF8(f.Comment);
    switch (ᐧ) {
    case {} when !utf8Valid1 || !utf8Valid2: {
        f.NonUTF8 = true;
        break;
    }
    case {} when !utf8Require1 && !utf8Require2: {
        f.NonUTF8 = false;
        break;
    }
    default: {
        f.NonUTF8 = (uint16)(f.Flags & 0x800) == 0;
        break;
    }}

    // Name and Comment definitely not UTF-8.
    // Name and Comment use only single-byte runes that overlap with UTF-8.
    // Might be UTF-8, might be some other encoding; preserve existing flag.
    // Some ZIP writers use UTF-8 encoding without setting the UTF-8 flag.
    // Since it is impossible to always distinguish valid UTF-8 from some
    // other encoding (e.g., GBK or Shift-JIS), we trust the flag.
    var needUSize = f.UncompressedSize == ~(uint32)0;
    var needCSize = f.CompressedSize == ~(uint32)0;
    var needHeaderOffset = f.headerOffset == (int64)(~(uint32)0);
    // Best effort to find what we need.
    // Other zip authors might not even follow the basic format,
    // and we'll just ignore the Extra content in that case.
    time.Time modified = default!;
parseExtras:
    for (var extra = ((readBuf)f.Extra); len(extra) >= 4; ) {
        // need at least tag and size
        var fieldTag = extra.uint16();
        nint fieldSize = (nint)extra.uint16();
        if (len(extra) < fieldSize) {
            break;
        }
        var fieldBuf = extra.sub(fieldSize);
        var exprᴛ1 = fieldTag;
        if (exprᴛ1 == zip64ExtraID) {
            f.zip64 = true;
            if (needUSize) {
                // update directory values from the zip64 extra block.
                // They should only be consulted if the sizes read earlier
                // are maxed out.
                // See golang.org/issue/13367.
                needUSize = false;
                if (len(fieldBuf) < 8) {
                    return ErrFormat;
                }
                f.UncompressedSize64 = fieldBuf.uint64();
            }
            if (needCSize) {
                needCSize = false;
                if (len(fieldBuf) < 8) {
                    return ErrFormat;
                }
                f.CompressedSize64 = fieldBuf.uint64();
            }
            if (needHeaderOffset) {
                needHeaderOffset = false;
                if (len(fieldBuf) < 8) {
                    return ErrFormat;
                }
                f.headerOffset = (int64)fieldBuf.uint64();
            }
        }
        else if (exprᴛ1 == ntfsExtraID) {
            if (len(fieldBuf) < 4) {
                goto continue_parseExtras;
            }
            fieldBuf.uint32(); // reserved (ignored)
            while (len(fieldBuf) >= 4) {
                // need at least tag and size
                var attrTag = fieldBuf.uint16();
                nint attrSize = (nint)fieldBuf.uint16();
                if (len(fieldBuf) < attrSize) {
                    goto continue_parseExtras;
                }
                var attrBuf = fieldBuf.sub(attrSize);
                if (attrTag != 1 || attrSize != 24) {
                    continue; // Ignore irrelevant attributes
                }
                UntypedFloat ticksPerSecond = 1e7; // Windows timestamp resolution
                var ts = (int64)attrBuf.uint64(); // ModTime since Windows epoch
                var secs = ts / (int64)ticksPerSecond;
                var nsecs = (int64)(1e9D / ticksPerSecond) * (ts % (int64)ticksPerSecond);
                var epoch = time.Date(1601, time.January, 1, 0, 0, 0, 0, time.ΔUTC);
                modified = time.Unix(epoch.Unix() + secs, nsecs);
            }
        }
        else if (exprᴛ1 == unixExtraID || exprᴛ1 == infoZipUnixExtraID) {
            if (len(fieldBuf) < 8) {
                goto continue_parseExtras;
            }
            fieldBuf.uint32(); // AcTime (ignored)
            var ts = (int64)fieldBuf.uint32(); // ModTime since Unix epoch
            modified = time.Unix(ts, 0);
        }
        else if (exprᴛ1 == extTimeExtraID) {
            if (len(fieldBuf) < 5 || (uint8)(fieldBuf.uint8() & 1) == 0) {
                goto continue_parseExtras;
            }
            var ts = (int64)fieldBuf.uint32(); // ModTime since Unix epoch
            modified = time.Unix(ts, 0);
        }

continue_parseExtras:;
    }
break_parseExtras:;
    var msdosModified = msDosTimeToTime(f.ModifiedDate, f.ModifiedTime);
    f.Modified = msdosModified;
    if (!modified.IsZero()) {
        f.Modified = modified.UTC();
        // If legacy MS-DOS timestamps are set, we can use the delta between
        // the legacy and extended versions to estimate timezone offset.
        //
        // A non-UTC timezone is always used (even if offset is zero).
        // Thus, FileHeader.Modified.Location() == time.UTC is useful for
        // determining whether extended timestamps are present.
        // This is necessary for users that need to do additional time
        // calculations when dealing with legacy ZIP formats.
        if (f.ModifiedTime != 0 || f.ModifiedDate != 0) {
            f.Modified = modified.In(timeZone(msdosModified.Sub(modified)));
        }
    }
    // Assume that uncompressed size 2³²-1 could plausibly happen in
    // an old zip32 file that was sharding inputs into the largest chunks
    // possible (or is just malicious; search the web for 42.zip).
    // If needUSize is true still, it means we didn't see a zip64 extension.
    // As long as the compressed size is not also 2³²-1 (implausible)
    // and the header is not also 2³²-1 (equally implausible),
    // accept the uncompressed size 2³²-1 as valid.
    // If nothing else, this keeps archive/zip working with 42.zip.
    _ = needUSize;
    if (needCSize || needHeaderOffset) {
        return ErrFormat;
    }
    return default!;
}

internal static error readDataDescriptor(io.Reader r, ref File f) {
    array<byte> buf = new(16); /* dataDescriptorLen */
    // The spec says: "Although not originally assigned a
    // signature, the value 0x08074b50 has commonly been adopted
    // as a signature value for the data descriptor record.
    // Implementers should be aware that ZIP files may be
    // encountered with or without this signature marking data
    // descriptors and should account for either case when reading
    // ZIP files to ensure compatibility."
    //
    // dataDescriptorLen includes the size of the signature but
    // first read just those 4 bytes to see if it exists.
    {
        var (_, err) = io.ReadFull(r, buf[..4]); if (err != default!) {
            return err;
        }
    }
    nint off = 0;
    var maybeSig = ((readBuf)(buf[..4]));
    if (maybeSig.uint32() != dataDescriptorSignature) {
        // No data descriptor signature. Keep these four
        // bytes.
        off += 4;
    }
    {
        var (_, err) = io.ReadFull(r, buf.slice(off, 12)); if (err != default!) {
            return err;
        }
    }
    var b = ((readBuf)(buf[..12]));
    if (b.uint32() != f.CRC32) {
        return ErrChecksum;
    }
    // The two sizes that follow here can be either 32 bits or 64 bits
    // but the spec is not very clear on this and different
    // interpretations has been made causing incompatibilities. We
    // already have the sizes from the central directory so we can
    // just ignore these.
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zipInvalidCommentLengthˢ = "zip: invalid comment length"u8;

internal static (ж<directoryEnd> dir, int64 baseOffset, error err) readDirectoryEnd(io.ReaderAt r, int64 size) {
    int64 baseOffset = default!;

    // look for directoryEndSignature in the last 1k, then in the last 65k
    slice<byte> buf = default!;
    int64 directoryEndOffset = default!;
    foreach (var (i, vᴛ1) in new int64[]{1024, 65 * 1024}.slice()) {
        var bLen = vᴛ1;

        if (bLen > size) {
            bLen = size;
        }
        buf = new slice<byte>((nint)bLen);
        {
            var (_, errΔ1) = r.ReadAt(buf, size - bLen); if (errΔ1 != default! && !AreEqual(errΔ1, io.EOF)) {
                return (default!, 0, errΔ1);
            }
        }
        {
            nint p = findSignatureInBlock(buf); if (p >= 0) {
                buf = buf.slice(p);
                directoryEndOffset = size - bLen + (int64)p;
                break;
            }
        }
        if (i == 1 || bLen == size) {
            return (default!, 0, ErrFormat);
        }
    }
    // read header into struct
    var b = ((readBuf)(buf[4..])); // skip signature
    var d = Ꮡ(new directoryEnd(
        diskNbr: (uint32)b.uint16(),
        dirDiskNbr: (uint32)b.uint16(),
        dirRecordsThisDisk: (uint64)b.uint16(),
        directoryRecords: (uint64)b.uint16(),
        directorySize: (uint64)b.uint32(),
        directoryOffset: (uint64)b.uint32(),
        commentLen: b.uint16()
    ));
    nint l = (nint)(~d).commentLen;
    if (l > len(b)) {
        return (default!, 0, errors.New(zipInvalidCommentLengthˢ));
    }
    d.Value.comment = ((@string)(slice<byte>)b.slice(0, l));
    // These values mean that the file can be a zip64 file
    if ((~d).directoryRecords == 0xffff || (~d).directorySize == 0xffff || (~d).directoryOffset == 0xffffffffU) {
        var (p, errΔ2) = findDirectory64End(r, directoryEndOffset);
        if (errΔ2 == default! && p >= 0) {
            directoryEndOffset = p;
            errΔ2 = readDirectory64End(r, p, ref (d).DerefOrNull());
        }
        if (errΔ2 != default!) {
            return (default!, 0, errΔ2);
        }
    }
    var maxInt64 = (uint64)(9223372036854775807UL);
    if ((~d).directorySize > maxInt64 || (~d).directoryOffset > maxInt64) {
        return (default!, 0, ErrFormat);
    }
    baseOffset = directoryEndOffset - (int64)(~d).directorySize - (int64)(~d).directoryOffset;
    // Make sure directoryOffset points to somewhere in our file.
    {
        var o = baseOffset + (int64)(~d).directoryOffset; if (o < 0 || o >= size) {
            return (default!, 0, ErrFormat);
        }
    }
    // If the directory end data tells us to use a non-zero baseOffset,
    // but we would find a valid directory entry if we assume that the
    // baseOffset is 0, then just use a baseOffset of 0.
    // We've seen files in which the directory end data gives us
    // an incorrect baseOffset.
    if (baseOffset > 0) {
        var off = (int64)(~d).directoryOffset;
        var rs = io.NewSectionReader(r, off, size - off);
        var ᴛ1 = new File(nil);
        if (readDirectoryHeader(ref ᴛ1, new io.SectionReaderжReader(rs)) == default!) {
            baseOffset = 0;
        }
    }
    return (d, baseOffset, default!);
}

// findDirectory64End tries to read the zip64 locator just before the
// directory end and returns the offset of the zip64 directory end if
// found.
internal static (int64, error) findDirectory64End(io.ReaderAt r, int64 directoryEndOffset) {
    var locOffset = directoryEndOffset - (int64)directory64LocLen;
    if (locOffset < 0) {
        return (-1, default!); // no need to look for a header outside the file
    }
    var buf = new slice<byte>(directory64LocLen);
    {
        var (_, err) = r.ReadAt(buf, locOffset); if (err != default!) {
            return (-1, err);
        }
    }
    var b = ((readBuf)buf);
    {
        var sig = b.uint32(); if (sig != directory64LocSignature) {
            return (-1, default!);
        }
    }
    if (b.uint32() != 0) {
        // number of the disk with the start of the zip64 end of central directory
        return (-1, default!); // the file is not a valid zip64-file
    }
    var p = b.uint64(); // relative offset of the zip64 end of central directory record
    if (b.uint32() != 1) {
        // total number of disks
        return (-1, default!); // the file is not a valid zip64-file
    }
    return ((int64)p, default!);
}

// readDirectory64End reads the zip64 directory end and updates the
// directory end with the zip64 directory end values.
internal static error /*err*/ readDirectory64End(io.ReaderAt r, int64 offset, ref directoryEnd d) {
    var buf = new slice<byte>(directory64EndLen);
    {
        var (_, errΔ1) = r.ReadAt(buf, offset); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var b = ((readBuf)buf);
    {
        var sig = b.uint32(); if (sig != directory64EndSignature) {
            return ErrFormat;
        }
    }
    b = b[12..]; // skip dir size, version and version needed (uint64 + 2x uint16)
    d.diskNbr = b.uint32(); // number of this disk
    d.dirDiskNbr = b.uint32(); // number of the disk with the start of the central directory
    d.dirRecordsThisDisk = b.uint64(); // total number of entries in the central directory on this disk
    d.directoryRecords = b.uint64(); // total number of entries in the central directory
    d.directorySize = b.uint64(); // size of the central directory
    d.directoryOffset = b.uint64(); // offset of start of central directory with respect to the starting disk number
    return default!;
}

internal static nint findSignatureInBlock(slice<byte> b) {
    for (nint i = len(b) - (nint)directoryEndLen; i >= 0; i--) {
        // defined from directoryEndSignature in struct.go
        if (b[i] == (rune)'P' && b[i + 1] == (rune)'K' && b[i + 2] == 0x05 && b[i + 3] == 0x06) {
            // n is length of comment
            nint n = (nint)((nint)b[i + (nint)directoryEndLen - 2] | ((nint)b[i + (nint)directoryEndLen - 1] << (int)(8)));
            if (n + (nint)directoryEndLen + i > len(b)) {
                // Truncated comment.
                // Some parsers (such as Info-ZIP) ignore the truncated comment
                // rather than treating it as a hard error.
                return -1;
            }
            return i;
        }
    }
    return -1;
}

[GoType("[]byte")] partial struct readBuf;

[GoRecv] internal static uint8 uint8(this ref readBuf b) {
    var v = (b)[0];
    b = (b)[1..];
    return v;
}

[GoRecv] internal static uint16 uint16(this ref readBuf b) {
    var v = binary.LittleEndian.Uint16(b);
    b = (b)[2..];
    return v;
}

[GoRecv] internal static uint32 uint32(this ref readBuf b) {
    var v = binary.LittleEndian.Uint32(b);
    b = (b)[4..];
    return v;
}

[GoRecv] internal static uint64 uint64(this ref readBuf b) {
    var v = binary.LittleEndian.Uint64(b);
    b = (b)[8..];
    return v;
}

[GoRecv] internal static readBuf sub(this ref readBuf b, nint n) {
    var b2 = (b).slice(0, n);
    b = (b).slice(n);
    return b2;
}

// A fileListEntry is a File and its ename.
// If file == nil, the fileListEntry describes a directory without metadata.
[GoType] partial struct fileListEntry {
    internal @string name;
    internal ж<File> @file;
    internal bool isDir;
    internal bool isDup;
}

[GoType] partial interface fileInfoDirEntry :
    fs.FileInfo,
    fs.DirEntry
{
}

internal static (fileInfoDirEntry, error) stat(this ж<fileListEntry> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (f.isDup) {
        return (default!, errors.New(f.name + ": duplicate entries in zip file"u8));
    }
    if (!f.isDir) {
        return (new headerFileInfo(f.@file.of(File.ᏑFileHeader)), default!);
    }
    return (new fileListEntryжfileInfoDirEntry(Ꮡf), default!);
}

// Only used for directories.
[GoRecv] internal static @string Name(this ref fileListEntry f) {
    var (_, elem, _) = split(f.name);
    return elem;
}

[GoRecv] internal static int64 Size(this ref fileListEntry f) {
    return 0;
}

[GoRecv] internal static fs.FileMode Mode(this ref fileListEntry f) {
    return (fs.FileMode)(fs.ModeDir | 365);
}

[GoRecv] internal static fs.FileMode Type(this ref fileListEntry f) {
    return fs.ModeDir;
}

[GoRecv] internal static bool IsDir(this ref fileListEntry f) {
    return true;
}

[GoRecv] internal static any Sys(this ref fileListEntry f) {
    return default!;
}

[GoRecv] internal static time.Time ModTime(this ref fileListEntry f) {
    if (f.@file == nil) {
        return new time.Time(nil);
    }
    return (~f.@file).FileHeader.Modified.UTC();
}

internal static (fs.FileInfo, error) Info(this ж<fileListEntry> Ꮡf) {
    return (new fileListEntryжFileInfo(Ꮡf), default!);
}

internal static @string String(this ж<fileListEntry> Ꮡf) {
    return fs.FormatDirEntry(new fileListEntryжDirEntry(Ꮡf));
}

// toValidName coerces name to be a valid name for fs.FS.Open.
internal static @string toValidName(@string name) {
    name = strings.ReplaceAll(name, @"\"u8, @"/"u8);
    @string p = path.Clean(name);
    p = strings.TrimPrefix(p, "/"u8);
    while (strings.HasPrefix(p, "../"u8)) {
        p = p[(int)(len("../"))..];
    }
    return p;
}

internal static void initFileList(this ж<Reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.of(Reader.ᏑfileListOnce).Do(() => {
        // files and knownDirs map from a file/directory name
        // to an index into the r.fileList entry that we are
        // building. They are used to mark duplicate entries.
        var files = new map<@string, nint>();
        var knownDirs = new map<@string, nint>();
        // dirs[name] is true if name is known to be a directory,
        // because it appears as a prefix in a path.
        var dirs = new map<@string, bool>();
        foreach (var (_, @file) in Ꮡr.Value.File) {
            var isDir = len((~@file).Name) > 0 && (~@file).Name[len((~@file).Name) - 1] == (rune)'/';
            @string name = toValidName((~@file).Name);
            if (name == ""u8) {
                continue;
            }
            {
                var (idxΔ1, ok) = files[name, ꟷ]; if (ok) {
                    Ꮡr.Value.fileList[idxΔ1].isDup = true;
                    continue;
                }
            }
            {
                var (idxΔ2, ok) = knownDirs[name, ꟷ]; if (ok) {
                    Ꮡr.Value.fileList[idxΔ2].isDup = true;
                    continue;
                }
            }
            @string dir = name;
            while (ᐧ) {
                {
                    nint idxΔ3 = strings.LastIndex(dir, "/"u8); if (idxΔ3 < 0){
                        break;
                    } else {
                        dir = dir.slice(0, idxΔ3);
                    }
                }
                if (dirs[dir]) {
                    break;
                }
                dirs[dir] = true;
            }
            nint idx = len(Ꮡr.Value.fileList);
            var entry = new fileListEntry(
                name: name,
                @file: @file,
                isDir: isDir
            );
            Ꮡr.Value.fileList = append(Ꮡr.Value.fileList, entry);
            if (isDir){
                knownDirs[name] = idx;
            } else {
                files[name] = idx;
            }
        }
        foreach (var (dir, _) in dirs) {
            {
                var (_, ok) = knownDirs[dir, ꟷ]; if (!ok) {
                    {
                        var (idx, okΔ1) = files[dir, ꟷ]; if (okΔ1){
                            Ꮡr.Value.fileList[idx].isDup = true;
                        } else {
                            var entry = new fileListEntry(
                                name: dir,
                                @file: nil,
                                isDir: true
                            );
                            Ꮡr.Value.fileList = append(Ꮡr.Value.fileList, entry);
                        }
                    }
                }
            }
        }
        slices.SortFunc(Ꮡr.Value.fileList, (fileListEntry a, fileListEntry b) => fileEntryCompare(a.name, b.name));
    });
}

internal static nint fileEntryCompare(@string x, @string y) {
    var (xdir, xelem, _) = split(x);
    var (ydir, yelem, _) = split(y);
    if (xdir != ydir) {
        return strings.Compare(xdir, ydir);
    }
    return strings.Compare(xelem, yelem);
}

// Open opens the named file in the ZIP archive,
// using the semantics of fs.FS.Open:
// paths are always slash separated, with no
// leading / or ../ elements.
public static (fs.File, error) Open(this ж<Reader> Ꮡr, @string name) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.initFileList();
    if (!fs.ValidPath(name)) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new fs.PathError(Op: "open"u8, Path: name, Err: fs.ErrInvalid))));
    }
    var e = r.openLookup(name);
    if (e == nil) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new fs.PathError(Op: "open"u8, Path: name, Err: fs.ErrNotExist))));
    }
    if ((~e).isDir) {
        return (new openDirжFile(Ꮡ(new openDir(e, r.openReadDir(name), 0))), default!);
    }
    var (rc, err) = (~e).@file.Open();
    if (err != default!) {
        return (default!, err);
    }
    return (rc._<fs.File>(), default!);
}

internal static (@string dir, @string elem, bool isDir) split(@string name) {
    bool isDir = default!;

    (name, isDir) = strings.CutSuffix(name, "/"u8);
    nint i = strings.LastIndexByte(name, (rune)'/');
    if (i < 0) {
        return (".", name, isDir);
    }
    return (name.slice(0, i), name.slice(i + 1), isDir);
}

internal static ж<fileListEntry> dotFile = Ꮡ(new fileListEntry(name: "./"u8, isDir: true));

[GoRecv] internal static ж<fileListEntry> openLookup(this ref Reader r, @string name) {
    if (name == "."u8) {
        return dotFile;
    }
    var (dir, elem, _) = split(name);
    var files = r.fileList;
    var (i, _) = slices.BinarySearchFunc(files, dir, (fileListEntry a, @string dirΔ1) => {
        var (idir, ielem, _) = split(a.name);
        if (dirΔ1 != idir) {
            return strings.Compare(idir, dirΔ1);
        }
        return strings.Compare(ielem, elem);
    });
    if (i < len(files)) {
        @string fname = files[i].name;
        if (fname == name || len(fname) == len(name) + 1 && fname[len(name)] == (rune)'/' && fname.slice(0, len(name)) == name) {
            return Ꮡ(files, i);
        }
    }
    return default!;
}

[GoRecv] internal static slice<fileListEntry> openReadDir(this ref Reader r, @string dir) {
    var files = r.fileList;
    var (i, _) = slices.BinarySearchFunc(files, dir, (fileListEntry a, @string dirΔ1) => {
        var (idir, _, _) = split(a.name);
        if (dirΔ1 != idir) {
            return strings.Compare(idir, dirΔ1);
        }
        // find the first entry with dir
        return +1;
    });
    var (j, _) = slices.BinarySearchFunc(files, dir, (fileListEntry a, @string dirΔ2) => {
        var (jdir, _, _) = split(a.name);
        if (dirΔ2 != jdir) {
            return strings.Compare(jdir, dirΔ2);
        }
        // find the last entry with dir
        return -1;
    });
    return files.slice(i, j);
}

[GoType] partial struct openDir {
    internal ж<fileListEntry> e;
    internal slice<fileListEntry> files;
    internal nint offset;
}

[GoRecv] internal static error Close(this ref openDir d) {
    return default!;
}

[GoRecv] internal static (fs.FileInfo, error) Stat(this ref openDir d) {
    return d.e.stat();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string isADirectoryˢ = "is a directory"u8;

[GoRecv] internal static (nint, error) Read(this ref openDir d, slice<byte> _) {
    return (0, new fs.PathErrorжerror(Ꮡ(new fs.PathError(Op: "read"u8, Path: (~d.e).name, Err: errors.New(isADirectoryˢ)))));
}

[GoRecv] internal static (slice<fs.DirEntry>, error) ReadDir(this ref openDir d, nint count) {
    nint n = len(d.files) - d.offset;
    if (count > 0 && n > count) {
        n = count;
    }
    if (n == 0) {
        if (count <= 0) {
            return (default!, default!);
        }
        return (default!, io.EOF);
    }
    var list = new slice<fs.DirEntry>(n);
    foreach (var (i, _) in list) {
        var (s, err) = Ꮡ(d.files, d.offset + i).stat();
        if (err != default!) {
            return (default!, err);
        }
        list[i] = s;
    }
    d.offset += n;
    return (list, default!);
}

} // end zip_package

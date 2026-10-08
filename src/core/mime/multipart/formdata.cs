// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.mime;

using bytes = bytes_package;
using errors = errors_package;
using godebug = @internal.godebug_package;
using io = io_package;
using math = math_package;
using textproto = net.textproto_package;
using os = os_package;
using strconv = strconv_package;
using @internal;
using net;

partial class multipart_package {

// ErrMessageTooLarge is returned by ReadForm if the message form
// data is too large to be processed.
public static error ErrMessageTooLarge = errors.New("multipart: message too large"u8);

// TODO(adg,bradfitz): find a way to unify the DoS-prevention strategy here
// with that of the http package's ParseForm.

// ReadForm parses an entire multipart message whose parts have
// a Content-Disposition of "form-data".
// It stores up to maxMemory bytes + 10MB (reserved for non-file parts)
// in memory. File parts which can't be stored in memory will be stored on
// disk in temporary files.
// It returns [ErrMessageTooLarge] if all non-file parts can't be stored in
// memory.
public static (ж<Form>, error) ReadForm(this ж<Reader> Ꮡr, int64 maxMemory) {
    return Ꮡr.readForm(maxMemory);
}

internal static ж<godebug.Setting> multipartfiles = godebug.New("#multipartfiles"u8);   // TODO: document and remove #
internal static ж<godebug.Setting> multipartmaxparts = godebug.New("multipartmaxparts"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string multipartˢ = "multipart-"u8;

// os.File.ReadFrom will allocate its own copy buffer if we let io.Copy use it.
internal partial struct readForm_writerOnly /*dyn*/ {
    /*embed*/ public io_package.Writer Writer;
}

internal static (ж<Form>, error err) readForm(this ж<Reader> Ꮡr, int64 maxMemory) {
    ж<Form> _ᴛ1 = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        var form = Ꮡ(new Form(new map<@string, slice<@string>>(), new map<@string, slice<ж<FileHeader>>>()));
        ref var @file = ref heap<ж<os.File>>(out var Ꮡfile);
        int64 fileOff = default!;
        nint numDiskFiles = 0;
        var combineFiles = true;
        if (multipartfiles.Value() == "distinct"u8) {
            combineFiles = false;
        }
        // multipartfiles.IncNonDefault() // TODO: uncomment after documenting
        nint maxParts = 1000;
        {
            @string s = multipartmaxparts.Value(); if (s != ""u8) {
                {
                    var (v, errΔ1) = strconv.Atoi(s); if (errΔ1 == default! && v >= 0) {
                        maxParts = v;
                        multipartmaxparts.IncNonDefault();
                    }
                }
            }
        }
        var maxHeaders = maxMIMEHeaders();
        var formʗ1 = form;
        defer(() => {
            if (Ꮡfile.ValueSlot != nil) {
                {
                    var cerr = Ꮡfile.ValueSlot.Close(); if (err == default!) {
                        err = cerr;
                    }
                }
            }
            if (combineFiles && numDiskFiles > 1) {
                foreach (var (_, fhs) in (~formʗ1).File) {
                    foreach (var (_, fh) in fhs) {
                        fh.Value.tmpshared = true;
                    }
                }
            }
            if (err != default!) {
                formʗ1.RemoveAll();
                if (Ꮡfile.ValueSlot != nil) {
                    os.Remove(Ꮡfile.ValueSlot.Name());
                }
            }
        }, ref ᒐ);
        // maxFileMemoryBytes is the maximum bytes of file data we will store in memory.
        // Data past this limit is written to disk.
        // This limit strictly applies to content, not metadata (filenames, MIME headers, etc.),
        // since metadata is always stored in memory, not disk.
        //
        // maxMemoryBytes is the maximum bytes we will store in memory, including file content,
        // non-file part values, metadata, and map entry overhead.
        //
        // We reserve an additional 10 MB in maxMemoryBytes for non-file data.
        //
        // The relationship between these parameters, as well as the overly-large and
        // unconfigurable 10 MB added on to maxMemory, is unfortunate but difficult to change
        // within the constraints of the API as documented.
        var maxFileMemoryBytes = maxMemory;
        if (maxFileMemoryBytes == math.MaxInt64) {
            maxFileMemoryBytes--;
        }
        var maxMemoryBytes = maxMemory + (int64)(((int64)10 << (int)(20)));
        if (maxMemoryBytes <= 0) {
            if (maxMemory < 0){
                maxMemoryBytes = 0;
            } else {
                maxMemoryBytes = math.MaxInt64;
            }
        }
        slice<byte> copyBuf = default!;
        while (ᐧ) {
            var (p, errΔ2) = Ꮡr.nextPart(false, maxMemoryBytes, maxHeaders);
            if (AreEqual(errΔ2, io.EOF)) {
                break;
            }
            if (errΔ2 != default!) {
                (_ᴛ1, err) = (default!, errΔ2); goto ᒐdone;
            }
            if (maxParts <= 0) {
                (_ᴛ1, err) = (default!, ErrMessageTooLarge); goto ᒐdone;
            }
            maxParts--;
            @string name = p.FormName();
            if (name == ""u8) {
                continue;
            }
            ref var filename = ref heap<@string>(out var Ꮡfilename);
            filename = p.FileName();
            // Multiple values for the same key (one map entry, longer slice) are cheaper
            // than the same number of values for different keys (many map entries), but
            // using a consistent per-value cost for overhead is simpler.
            const int64 mapEntryOverhead = 200;
            maxMemoryBytes -= (int64)len(name);
            maxMemoryBytes -= mapEntryOverhead;
            if (maxMemoryBytes < 0) {
                // We can't actually take this path, since nextPart would already have
                // rejected the MIME headers for being too large. Check anyway.
                (_ᴛ1, err) = (default!, ErrMessageTooLarge); goto ᒐdone;
            }
            ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
            if (filename == ""u8) {
                // value, store as string in memory
                var (nΔ1, errΔ3) = io.CopyN(new bytes_BufferжWriter(Ꮡb), new PartжReader(p), maxMemoryBytes + 1);
                if (errΔ3 != default! && !AreEqual(errΔ3, io.EOF)) {
                    (_ᴛ1, err) = (default!, errΔ3); goto ᒐdone;
                }
                maxMemoryBytes -= nΔ1;
                if (maxMemoryBytes < 0) {
                    (_ᴛ1, err) = (default!, ErrMessageTooLarge); goto ᒐdone;
                }
                form.Value.Value[name] = append((~form).Value[name], Ꮡb.String());
                continue;
            }
            // file, store in memory or on disk
            const int64 fileHeaderSize = 100;
            maxMemoryBytes -= mimeHeaderSize((~p).Header);
            maxMemoryBytes -= mapEntryOverhead;
            maxMemoryBytes -= fileHeaderSize;
            if (maxMemoryBytes < 0) {
                (_ᴛ1, err) = (default!, ErrMessageTooLarge); goto ᒐdone;
            }
            foreach (var (_, v) in (~p).Header) {
                maxHeaders -= (int64)len(v);
            }
            var fh = Ꮡ(new FileHeader(
                Filename: filename,
                Header: (~p).Header
            ));
            (var n, errΔ2) = io.CopyN(new bytes_BufferжWriter(Ꮡb), new PartжReader(p), maxFileMemoryBytes + 1);
            if (errΔ2 != default! && !AreEqual(errΔ2, io.EOF)) {
                (_ᴛ1, err) = (default!, errΔ2); goto ᒐdone;
            }
            if (n > maxFileMemoryBytes){
                if (@file == nil) {
                    (@file, errΔ2) = os.CreateTemp(r.tempDir, multipartˢ);
                    if (errΔ2 != default!) {
                        (_ᴛ1, err) = (default!, errΔ2); goto ᒐdone;
                    }
                }
                numDiskFiles++;
                {
                    var (_, errΔ4) = @file.Write(b.Bytes()); if (errΔ4 != default!) {
                        (_ᴛ1, err) = (default!, errΔ4); goto ᒐdone;
                    }
                }
                if (copyBuf == default!) {
                    copyBuf = new slice<byte>(32 * 1024); // same buffer size as io.Copy uses
                }
                var (remainingSize, errΔ5) = io.CopyBuffer(new readForm_writerOnly(new os.FileжWriter(@file)), new PartжReader(p), copyBuf);
                if (errΔ5 != default!) {
                    (_ᴛ1, err) = (default!, errΔ5); goto ᒐdone;
                }
                fh.Value.tmpfile = @file.Name();
                fh.Value.Size = (int64)b.Len() + remainingSize;
                fh.Value.tmpoff = fileOff;
                fileOff += fh.Value.Size;
                if (!combineFiles) {
                    {
                        var errΔ6 = @file.Close(); if (errΔ6 != default!) {
                            (_ᴛ1, err) = (default!, errΔ6); goto ᒐdone;
                        }
                    }
                    @file = default!;
                }
            } else {
                fh.Value.content = b.Bytes();
                fh.Value.Size = (int64)len((~fh).content);
                maxFileMemoryBytes -= n;
                maxMemoryBytes -= n;
            }
            form.Value.File[name] = append((~form).File[name], fh);
        }
        (_ᴛ1, err) = (form, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (_ᴛ1, err);
}

internal static int64 /*size*/ mimeHeaderSize(textproto.MIMEHeader h) {
    int64 size = default!;

    size = 400;
    foreach (var (k, vs) in h) {
        size += (int64)len(k);
        size += 200; // map entry overhead
        foreach (var (_, v) in vs) {
            size += (int64)len(v);
        }
    }
    return size;
}

// Form is a parsed multipart form.
// Its File parts are stored either in memory or on disk,
// and are accessible via the [*FileHeader]'s Open method.
// Its Value parts are stored as strings.
// Both are keyed by field name.
partial struct Form {
    public map<@string, slice<@string>> Value;
    public map<@string, slice<ж<FileHeader>>> File;
}

// RemoveAll removes any temporary files associated with a [Form].
public static error RemoveAll(this ref Form f) {
    error err = default!;
    foreach (var (_, fhs) in f.File) {
        foreach (var (_, fh) in fhs) {
            if ((~fh).tmpfile != ""u8) {
                var e = os.Remove((~fh).tmpfile);
                if (e != default! && !errors.Is(e, os.ErrNotExist) && err == default!) {
                    err = e;
                }
            }
        }
    }
    return err;
}

// A FileHeader describes a file part of a multipart request.
partial struct FileHeader {
    public @string Filename;
    public textproto.MIMEHeader Header;
    public int64 Size;
    internal slice<byte> content;
    internal @string tmpfile;
    internal int64 tmpoff;
    internal bool tmpshared;
}

// Open opens and returns the [FileHeader]'s associated File.
public static (File, error) Open(this ref FileHeader fh) {
    {
        var b = fh.content; if (b != default!) {
            var r = io.NewSectionReader(new bytes_ReaderжReaderAt(bytes.NewReader(b)), 0, (int64)len(b));
            return (new sectionReadCloser(r, default!), default!);
        }
    }
    if (fh.tmpshared) {
        var (f, err) = os.Open(fh.tmpfile);
        if (err != default!) {
            return (default!, err);
        }
        var r = io.NewSectionReader(new os_FileжReaderAt(f), fh.tmpoff, fh.Size);
        return (new sectionReadCloser(r, new os_FileжCloser(f)), default!);
    }
    var (ᴛ1, ᴛ2) = os.Open(fh.tmpfile);
    return (new os_FileжFile(ᴛ1), ᴛ2);
}

// File is an interface to access the file part of a multipart message.
// Its contents may be either stored in memory or on disk.
// If stored on disk, the File's underlying concrete type will be an *os.File.
partial interface File :
    io.Reader,
    io.ReaderAt,
    io.Seeker,
    io.Closer,
    io.ReadSeekCloser
{
}

// helper types to turn a []byte into a File
partial struct sectionReadCloser {
    public partial ref ж<io_package.SectionReader> SectionReader { get; }
    /*embed*/ public io_package.Closer Closer;
}

internal static error Close(this sectionReadCloser rc) {
    if (rc.Closer != default!) {
        return rc.Closer.Close();
    }
    return default!;
}

} // end multipart_package

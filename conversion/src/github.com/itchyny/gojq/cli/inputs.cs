namespace go.github.com.itchyny.gojq;

using bufio = bufio_package;
using bytes = bytes_package;
using json = encoding.json_package;
using io = io_package;
using os = os_package;
using strings = strings_package;
using yaml = go.github.com.itchyny.go_yaml.yaml_package;
using gojq = go.github.com.itchyny.gojq_package;
using encoding;
using go.github.com.itchyny;
using go.github.com.itchyny.go_yaml;

partial class cli_package {

partial struct inputReader {
    /*embed*/ public io_package.Reader Reader;
    internal io.ReadSeeker rs;
    internal ж<bytes.Buffer> buf;
}

internal static ж<inputReader> newInputReader(io.Reader r) {
    var (rΔ1, ok) = r._<io.ReadSeeker>(ᐧ); if (ok) {
        var (_, err) = rΔ1.Seek(0, io.SeekCurrent); if (err == nil) {
            return Ꮡ(new inputReader(rΔ1, rΔ1, nil));
        }
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    return Ꮡ(new inputReader(io.TeeReader(r, new bytes_BufferжWriter(Ꮡbuf)), default, Ꮡbuf));
}

internal static @string getContents(this ж<inputReader> Ꮡir, ж<int64> Ꮡoffset, ж<nint> Ꮡline) {
    GoFrame ᒐ = default;
    try {
        ref var ir = ref Ꮡir.DerefOrNull();
        ref var offset = ref Ꮡoffset.DerefOrNull();
        ref var line = ref Ꮡline.DerefOrNull();

        var bufΔ1 = ir.buf; if (bufΔ1 != nil) {
            return bufΔ1.String();
        }
        {
            var (current, err) = ir.rs.Seek(0, io.SeekCurrent); if (err == nil) {
                defer(Ꮡir.Value.rs.Seek, current, (nint)(io.SeekStart), ref ᒐ);
            }
        }
        (_, _) = ir.rs.Seek(0, io.SeekStart);
        UntypedInt bufSize = /* 16 * 1024 */ 16384;
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        while (Ꮡoffset != nil && offset > (int64)(bufSize * 3 / 4)) {
            var (n, err) = io.Copy(new bytes_BufferжWriter(Ꮡbuf),
                io.LimitReader(ir.rs, min((int64)(bufSize), offset - (int64)(bufSize / 4))));
            offset -= n;
            line += bytes.Count(buf.Bytes(), [(byte)'\n']);
            buf.Reset();
            if (err != nil || n == 0) {
                break;
            }
        }
        io.Reader r = default;
        if (Ꮡoffset == nil) {
            r = ir.rs;
        } else {
            r = io.LimitReader(ir.rs, bufSize);
        }
        (_, _) = io.Copy(new bytes_BufferжWriter(Ꮡbuf), r);
        return Ꮡbuf.String();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

partial interface inputIter :
    gojq.Iter,
    io.Closer
{
    @string Name();
}

partial struct jsonInputIter {
    internal Func<(any, error)> next;
    internal ж<inputReader> ir;
    internal @string fname;
    internal int64 offset;
    internal nint line;
    internal error err;
}

internal static inputIter newJSONInputIter(io.Reader r, @string fname) {
    var ir = newInputReader(r);
    var dec = json.NewDecoder(new inputReaderжReader(ir));
    dec.UseNumber();
    var decʗ1 = dec;
    var next = () => {
        ref var v = ref heap<any>(out var Ꮡv);
        error err = default;
        err = decʗ1.Decode(Ꮡv);
        return (v, err);
    };
    return new jsonInputIterжinputIter(Ꮡ(new jsonInputIter(next: next, ir: ir, fname: fname)));
}

internal static (any, bool) Next(this ж<jsonInputIter> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    if (i.err != nil) {
        return (default, false);
    }
    var (v, err) = i.next();
    if (err != nil) {
        if (AreEqual(err, io.EOF)) {
            i.err = err;
            return (default, false);
        }
        ж<int64> offset = default;
        ж<nint> line = default;
        var (e, ok) = err._<ж<json.SyntaxError>>(ᐧ); if (ok) {
            e.Offset -= i.offset;
            (offset, line) = (e.ᏑOffset, Ꮡi.Ꮡline);
        } else if (AreEqual(err, io.ErrUnexpectedEOF) && i.ir.rs != nil) {
            ref var pos = ref heap<int64>(out var Ꮡpos);
            (pos, var errΔ1) = i.ir.rs.Seek(0, io.SeekEnd); if (errΔ1 == nil) {
                (offset, line) = (Ꮡpos, Ꮡi.Ꮡline);
            }
        }
        i.err = new jsonParseErrorжerror(Ꮡ(new jsonParseError(i.fname, i.ir.getContents(offset, line), i.line, err)));
        return (i.err, true);
    }
    var buf = i.ir.buf; if (buf != nil && buf.Len() >= 16 * 1024) {
        i.offset += (int64)buf.Len();
        i.line += bytes.Count(buf.Bytes(), [(byte)'\n']);
        buf.Reset();
    }
    return (v, true);
}

internal static error Close(this ref jsonInputIter i) {
    i.err = io.EOF;
    return default;
}

internal static @string Name(this ref jsonInputIter i) {
    return i.fname;
}

internal static inputIter newStreamInputIter(io.Reader r, @string fname) {
    var ir = newInputReader(r);
    var dec = json.NewDecoder(new inputReaderжReader(ir));
    dec.UseNumber();
    return new jsonInputIterжinputIter(Ꮡ(new jsonInputIter(next: newJSONStream(dec).next, ir: ir, fname: fname)));
}

partial struct nullInputIter {
    internal error err;
}

internal static inputIter newNullInputIter() {
    return new nullInputIterжinputIter(Ꮡ(new nullInputIter()));
}

internal static (any, bool) Next(this ref nullInputIter i) {
    if (i.err != nil) {
        return (default, false);
    }
    i.err = io.EOF;
    return (default, true);
}

internal static error Close(this ref nullInputIter i) {
    i.err = io.EOF;
    return default;
}

internal static @string Name(this ref nullInputIter _) {
    return "";
}

partial struct filesInputIter {
    internal Func<io.Reader, @string, inputIter> newIter;
    internal slice<@string> fnames;
    internal io.Reader stdin;
    internal inputIter iter;
    internal io.Reader @file;
    internal error err;
}

internal static inputIter newFilesInputIter(Func<io.Reader, @string, inputIter> newIter, slice<@string> fnames, io.Reader stdin) {
    return new filesInputIterжinputIter(Ꮡ(new filesInputIter(newIter: newIter, fnames: fnames, stdin: stdin)));
}

internal static (any, bool) Next(this ref filesInputIter i) {
    if (i.err != nil) {
        return (default, false);
    }
    for (;;) {
        if (i.@file == nil) {
            if (len(i.fnames) == 0) {
                i.err = io.EOF;
                if (i.iter != nil) {
                    i.iter.Close();
                    i.iter = default;
                }
                return (default, false);
            }
            @string fname = i.fnames[0];
            i.fnames = i.fnames[1..];
            if (fname == "-"u8 && i.stdin != nil) {
                (i.@file, fname) = (i.stdin, "<stdin>");
            } else {
                var (@file, err) = os.Open(fname);
                if (err != nil) {
                    return (err, true);
                }
                i.@file = new os_FileжReader(@file);
            }
            if (i.iter != nil) {
                i.iter.Close();
            }
            i.iter = i.newIter(i.@file, fname);
        }
        {
            var (v, ok) = i.iter.Next(); if (ok) {
                return (v, ok);
            }
        }
        {
            var (r, ok) = i.@file._<io.Closer>(ᐧ); if (ok && !AreEqual(i.@file, i.stdin)) {
                r.Close();
            }
        }
        i.@file = default;
    }
}

internal static error Close(this ref filesInputIter i) {
    if (i.@file != nil) {
        var (r, ok) = i.@file._<io.Closer>(ᐧ); if (ok && !AreEqual(i.@file, i.stdin)) {
            r.Close();
        }
        i.@file = default;
        i.err = io.EOF;
    }
    return default;
}

internal static @string Name(this ref filesInputIter i) {
    if (i.iter != nil) {
        return i.iter.Name();
    }
    return "";
}

partial struct rawInputIter {
    internal ж<bufio.Reader> r;
    internal @string fname;
    internal error err;
}

internal static inputIter newRawInputIter(io.Reader r, @string fname) {
    return new rawInputIterжinputIter(Ꮡ(new rawInputIter(r: bufio.NewReader(r), fname: fname)));
}

internal static (any, bool) Next(this ref rawInputIter i) {
    if (i.err != nil) {
        return (default, false);
    }
    var (line, err) = i.r.ReadString((byte)'\n');
    if (err != nil) {
        i.err = err;
        if (!AreEqual(err, io.EOF)) {
            return (err, true);
        }
        if (line == ""u8) {
            return (default, false);
        }
    }
    return (strings.TrimSuffix(line, "\n"), true);
}

internal static error Close(this ref rawInputIter i) {
    i.err = io.EOF;
    return default;
}

internal static @string Name(this ref rawInputIter i) {
    return i.fname;
}

partial struct yamlInputIter {
    internal ж<yaml.Decoder> dec;
    internal ж<inputReader> ir;
    internal @string fname;
    internal error err;
}

internal static inputIter newYAMLInputIter(io.Reader r, @string fname) {
    var ir = newInputReader(r);
    var dec = yaml.NewDecoder(new inputReaderжReader(ir));
    return new yamlInputIterжinputIter(Ꮡ(new yamlInputIter(dec: dec, ir: ir, fname: fname)));
}

internal static (any, bool) Next(this ref yamlInputIter i) {
    if (i.err != nil) {
        return (default, false);
    }
    ref var v = ref heap<any>(out var Ꮡv);
    var err = i.dec.Decode(Ꮡv); if (err != nil) {
        if (AreEqual(err, io.EOF)) {
            i.err = err;
            return (default, false);
        }
        i.err = new yamlParseErrorжerror(Ꮡ(new yamlParseError(i.fname, i.ir.getContents(nil, nil), err)));
        return (i.err, true);
    }
    return (v, true);
}

internal static error Close(this ref yamlInputIter i) {
    i.err = io.EOF;
    return default;
}

internal static @string Name(this ref yamlInputIter i) {
    return i.fname;
}

partial struct slurpInputIter {
    internal inputIter iter;
    internal error err;
}

internal static inputIter newSlurpInputIter(inputIter iter) {
    return new slurpInputIterжinputIter(Ꮡ(new slurpInputIter(iter: iter)));
}

internal static (any, bool) Next(this ref slurpInputIter i) {
    if (i.err != nil) {
        return (default, false);
    }
    slice<any> vs = default;
    any v = default;
    bool ok = default;
    for (;;) {
        (v, ok) = i.iter.Next();
        if (!ok) {
            i.err = io.EOF;
            return (vs, true);
        }
        (i.err, ok) = v._<error>(ᐧ); if (ok) {
            return (i.err, true);
        }
        vs = append(vs, v);
    }
}

internal static error Close(this ref slurpInputIter i) {
    if (i.iter != nil) {
        i.iter.Close();
        i.iter = default;
        i.err = io.EOF;
    }
    return default;
}

internal static @string Name(this ref slurpInputIter i) {
    return i.iter.Name();
}

partial struct readAllIter {
    internal io.Reader r;
    internal @string fname;
    internal error err;
}

internal static inputIter newReadAllIter(io.Reader r, @string fname) {
    return new readAllIterжinputIter(Ꮡ(new readAllIter(r: r, fname: fname)));
}

internal static (any, bool) Next(this ref readAllIter i) {
    if (i.err != nil) {
        return (default, false);
    }
    i.err = io.EOF;
    var (cnt, err) = io.ReadAll(i.r);
    if (err != nil) {
        return (err, true);
    }
    return ((@string)cnt, true);
}

internal static error Close(this ref readAllIter i) {
    i.err = io.EOF;
    return default;
}

internal static @string Name(this ref readAllIter i) {
    return i.fname;
}

partial struct slurpRawInputIter {
    internal inputIter iter;
    internal error err;
}

internal static inputIter newSlurpRawInputIter(inputIter iter) {
    return new slurpRawInputIterжinputIter(Ꮡ(new slurpRawInputIter(iter: iter)));
}

internal static (any, bool) Next(this ref slurpRawInputIter i) {
    var vsᴷ = new stackAppend<@string>();

    if (i.err != nil) {
        return (default, false);
    }
    slice<@string> vs = default;
    any v = default;
    bool ok = default;
    for (;;) {
        (v, ok) = i.iter.Next();
        if (!ok) {
            i.err = io.EOF;
            return (strings.Join(vs, ""), true);
        }
        (i.err, ok) = v._<error>(ᐧ); if (ok) {
            return (i.err, true);
        }
        vs = append(vsᴷ, vs, v._<@string>());
    }
}

internal static error Close(this ref slurpRawInputIter i) {
    if (i.iter != nil) {
        i.iter.Close();
        i.iter = default;
        i.err = io.EOF;
    }
    return default;
}

internal static @string Name(this ref slurpRawInputIter i) {
    return i.iter.Name();
}

} // end cli_package

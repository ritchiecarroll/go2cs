// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using bytes = bytes_package;
using gob = encoding.gob_package;
using fmt = fmt_package;
using slices = slices_package;
using testing = testing_package;
using encoding;
using io = io_package;
using iter = iter_package;
using static global::go.go.token_package;

partial class token_internal_test_package {

// equal returns nil if p and q describe the same file set;
// otherwise it returns an error describing the discrepancy.
internal static error equal(ж<global::go.go.token_package.FileSet> Ꮡp, ж<global::go.go.token_package.FileSet> Ꮡq) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();
        ref var q = ref Ꮡq.DerefOrNull();

        if (Ꮡp == Ꮡq) {
            // avoid deadlock if p == q
            return default!;
        }

        // not strictly needed for the test
        Ꮡp.of(global::go.go.token_package.FileSet.Ꮡmutex).Lock();
        Ꮡq.of(global::go.go.token_package.FileSet.Ꮡmutex).Lock();
        defer(Ꮡq.of(global::go.go.token_package.FileSet.Ꮡmutex).Unlock, ref ᒐ);
        defer(Ꮡp.of(global::go.go.token_package.FileSet.Ꮡmutex).Unlock, ref ᒐ);

        if (p.@base != q.@base) {
            return fmt.Errorf("different bases: %d != %d"u8, p.@base, q.@base);
        }

        var pfiles = slices.Collect(Ꮡp.of(global::go.go.token_package.FileSet.Ꮡtree).all());
        var qfiles = slices.Collect(Ꮡq.of(global::go.go.token_package.FileSet.Ꮡtree).all());
        if (len(pfiles) != len(qfiles)) {
            return fmt.Errorf("different number of files: %d != %d"u8, len(pfiles), len(qfiles));
        }

        foreach (var (i, f) in pfiles) {
            var g = qfiles[i];
            if ((~f).name != (~g).name) {
                return fmt.Errorf("different filenames: %q != %q"u8, (~f).name, (~g).name);
            }
            if ((~f).@base != (~g).@base) {
                return fmt.Errorf("different base for %q: %d != %d"u8, (~f).name, (~f).@base, (~g).@base);
            }
            if ((~f).size != (~g).size) {
                return fmt.Errorf("different size for %q: %d != %d"u8, (~f).name, (~f).size, (~g).size);
            }
            foreach (var (j, l) in (~f).lines) {
                nint m = (~g).lines[j];
                if (l != m) {
                    return fmt.Errorf("different offsets for %q"u8, (~f).name);
                }
            }
            foreach (var (j, l) in (~f).infos) {
                var m = (~g).infos[j];
                if (l.Offset != m.Offset || l.Filename != m.Filename || l.Line != m.Line) {
                    return fmt.Errorf("different infos for %q"u8, (~f).name);
                }
            }
        }

        // we don't care about .last - it's just a cache
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void checkSerialize(ж<testing.T> Ꮡt, ж<global::go.go.token_package.FileSet> Ꮡp) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var encode = (any x) => gob.NewEncoder(new token_test_package.bytes_BufferжWriter(Ꮡbuf)).Encode(x);
    {
        var err = Ꮡp.Write(encode); if (err != default!) {
            Ꮡt.Errorf("writing fileset failed: %s"u8, err);
            return;
        }
    }
    var q = NewFileSet();
    var decode = (any x) => gob.NewDecoder(new token_test_package.bytes_BufferжReader(Ꮡbuf)).Decode(x);
    {
        var err = q.Read(decode); if (err != default!) {
            Ꮡt.Errorf("reading fileset failed: %s"u8, err);
            return;
        }
    }
    {
        var err = equal(Ꮡp, q); if (err != default!) {
            Ꮡt.Errorf("filesets not identical: %s"u8, err);
        }
    }
}

public static void TestSerialization(ж<testing.T> Ꮡt) {
    var p = NewFileSet();
    checkSerialize(Ꮡt, p);
    // add some files
    foreach (var i in range(10)) {
        var f = p.AddFile(fmt.Sprintf("file%d"u8, i), p.Base() + i, i * 100);
        checkSerialize(Ꮡt, p);
        // add some lines and alternative file infos
        nint line = 1000;
        for (nint offs = 0; offs < f.Size(); offs += 40 + i) {
            f.AddLine(offs);
            if (offs % 7 == 0) {
                f.AddLineInfo(offs, fmt.Sprintf("file%d"u8, offs), line);
                line += 33;
            }
        }
        checkSerialize(Ꮡt, p);
    }
}

} // end token_internal_test_package

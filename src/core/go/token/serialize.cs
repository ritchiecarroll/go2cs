// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using slices = slices_package;
using global::go.sync;
using iter = iter_package;

partial class token_package {

public partial struct serializedFile {
    // fields correspond 1:1 to fields with same (lower-case) name in File
    public @string Name;
    public nint Base;
    public nint Size;
    public slice<nint> Lines;
    public slice<lineInfo> Infos;
}

partial struct serializedFileSet {
    public nint Base;
    public slice<serializedFile> Files;
}

// Read calls decode to deserialize a file set into s; s must not be nil.
internal static error Read⁔(/*this*/ ж<FileSet> Ꮡs, Func<any, error> decode) {
    ref var s = ref Ꮡs.DerefOrNull();

    ref var ss = ref heap(new serializedFileSet(), out var Ꮡss);
    var err = decode(Ꮡss); if (err != nil) {
        return err;
    }

    Ꮡs.Ꮡmutex.Lock();
    s.@base = ss.Base;
    foreach (var (_, f) in range(ss.Files)) {
        Ꮡs.Ꮡtree.add(Ꮡ(new File(
            name: f.Name,
            @base: f.Base,
            size: f.Size,
            lines: f.Lines,
            infos: f.Infos
        )));
    }
    Ꮡs.Ꮡlast.Store(nil);
    Ꮡs.Ꮡmutex.Unlock();

    return default;
}

// Write calls encode to serialize the file set s.
internal static error Write⁔(/*this*/ ж<FileSet> Ꮡs, Func<any, error> encode) {
    ref var s = ref Ꮡs.DerefOrNull();

    serializedFileSet ss = default;

    Ꮡs.Ꮡmutex.Lock();
    ss.Base = s.@base;
    slice<serializedFile> files = default;
    foreach (var f in range<ж<File>>(Ꮡs.Ꮡtree.all().Invoke)) {
        f.Ꮡmutex.Lock();
        var evalᴛ1 = slices.Clone<slice<nint>, nint>(f.lines);
        var evalᴛ2 = slices.Clone<slice<lineInfo>, lineInfo>(f.infos);
        files = append(files, new serializedFile(
            Name: f.name,
            Base: f.@base,
            Size: f.size,
            Lines: evalᴛ1,
            Infos: evalᴛ2
        ));
        f.Ꮡmutex.Unlock();
    }
    ss.Files = files;
    Ꮡs.Ꮡmutex.Unlock();

    return encode(ss);
}

} // end token_package

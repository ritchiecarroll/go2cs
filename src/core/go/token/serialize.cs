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
public static error Read(this ж<FileSet> Ꮡs, Func<any, error> decode) {
    ref var s = ref Ꮡs.DerefOrNull();

    ref var ss = ref heap(new serializedFileSet(), out var Ꮡss);
    {
        var err = decode(Ꮡss); if (err != nil) {
            return err;
        }
    }

    Ꮡs.of(FileSet.Ꮡmutex).Lock();
    s.@base = ss.Base;
    foreach (var (_, f) in ss.Files) {
        Ꮡs.of(FileSet.Ꮡtree).add(Ꮡ(new ΔFile(
            name: f.Name,
            @base: f.Base,
            size: f.Size,
            lines: f.Lines,
            infos: f.Infos
        )));
    }
    Ꮡs.of(FileSet.Ꮡlast).Store(nil);
    Ꮡs.of(FileSet.Ꮡmutex).Unlock();

    return default;
}

// Write calls encode to serialize the file set s.
public static error Write(this ж<FileSet> Ꮡs, Func<any, error> encode) {
    ref var s = ref Ꮡs.DerefOrNull();

    serializedFileSet ss = default;

    Ꮡs.of(FileSet.Ꮡmutex).Lock();
    ss.Base = s.@base;
    slice<serializedFile> files = default;
    foreach (var f in range<ж<ΔFile>>(Ꮡs.of(FileSet.Ꮡtree).all().Invoke)) {
        f.of(token_package.ΔFile.Ꮡmutex).Lock();
        files = append(files, new serializedFile(
            Name: f.name,
            Base: f.@base,
            Size: f.size,
            Lines: slices.Clone<slice<nint>, nint>(f.lines),
            Infos: slices.Clone<slice<lineInfo>, lineInfo>(f.infos)
        ));
        f.of(token_package.ΔFile.Ꮡmutex).Unlock();
    }
    ss.Files = files;
    Ꮡs.of(FileSet.Ꮡmutex).Unlock();

    return encode(ss);
}

} // end token_package

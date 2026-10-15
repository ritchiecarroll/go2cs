// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

partial class fs_package {

// ReadLinkFS is the interface implemented by a file system
// that supports reading symbolic links.
partial interface ReadLinkFS :
    FS
{
    // ReadLink returns the destination of the named symbolic link.
    // If there is an error, it should be of type [*PathError].
    (@string, error) ReadLink(@string name);
    // Lstat returns a [FileInfo] describing the named file.
    // If the file is a symbolic link, the returned [FileInfo] describes the symbolic link.
    // Lstat makes no attempt to follow the link.
    // If there is an error, it should be of type [*PathError].
    (FileInfo, error) Lstat(@string name);
}

// ReadLink returns the destination of the named symbolic link.
//
// If fsys does not implement [ReadLinkFS], then ReadLink returns an error.
public static (@string, error) ReadLink(FS fsys, @string name) {
    var (sym, ok) = fsys._<ReadLinkFS>(ᐧ);
    if (!ok) {
        return ("", new PathErrorжerror(Ꮡ(new PathError(Op: "readlink"u8, Path: name, Err: ErrInvalid))));
    }
    return sym.ReadLink(name);
}

// Lstat returns a [FileInfo] describing the named file.
// If the file is a symbolic link, the returned [FileInfo] describes the symbolic link.
// Lstat makes no attempt to follow the link.
//
// If fsys does not implement [ReadLinkFS], then Lstat is identical to [Stat].
public static (FileInfo, error) Lstat(FS fsys, @string name) {
    var (sym, ok) = fsys._<ReadLinkFS>(ᐧ);
    if (!ok) {
        return Stat(fsys, name);
    }
    return sym.Lstat(name);
}

} // end fs_package

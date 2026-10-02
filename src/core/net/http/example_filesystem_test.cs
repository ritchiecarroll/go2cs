// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using io = io_package;
using fs = global::go.io.fs_package;
using log = log_package;
using Δhttp = global::go.net.http_package;
using strings = strings_package;
using global::go.io;
using global::go.net;
using static global::go.net.http_internal_test_package;

partial class http_test_package {

// containsDotFile reports whether name contains a path element starting with a period.
// The name is assumed to be a delimited by forward slashes, as guaranteed
// by the http.FileSystem interface.
internal static bool containsDotFile(@string name) {
    var parts = strings.Split(name, "/"u8);
    foreach (var (_, part) in parts) {
        if (strings.HasPrefix(part, "."u8)) {
            return true;
        }
    }
    return false;
}

// dotFileHidingFile is the http.File use in dotFileHidingFileSystem.
// It is used to wrap the Readdir method of http.File so that we can
// remove files and directories that start with a period from its output.
[GoType] partial struct dotFileHidingFile {
    [GoEmbedded] public global::go.net.http_package.File File;
}

// Readdir is a wrapper around the Readdir method of the embedded File
// that filters out all files that start with a period in their name.
internal static (slice<fs.FileInfo> fis, error err) Readdir(this dotFileHidingFile f, nint n) {
    slice<fs.FileInfo> fis = default!;
    error err = default!;

    (var files, err) = f.File.Readdir(n);
    foreach (var (_, @file) in files) {
        // Filters out the dot files
        if (!strings.HasPrefix(@file.Name(), "."u8)) {
            fis = append(fis, @file);
        }
    }
    if (err == default! && n > 0 && len(fis) == 0) {
        err = io.EOF;
    }
    return (fis, err);
}

// dotFileHidingFileSystem is an http.FileSystem that hides
// hidden "dot files" from being served.
[GoType] partial struct dotFileHidingFileSystem {
    [GoEmbedded] public global::go.net.http_package.FileSystem FileSystem;
}

// Open is a wrapper around the Open method of the embedded FileSystem
// that serves a 403 permission error when name has a file or directory
// with whose name starts with a period in its path.
internal static (Δhttp.File, error) Open(this dotFileHidingFileSystem fsys, @string name) {
    if (containsDotFile(name)) {
        // If dot file, return 403 response
        return (default!, fs.ErrPermission);
    }
    var (@file, err) = fsys.FileSystem.Open(name);
    if (err != default!) {
        return (default!, err);
    }
    return (new dotFileHidingFile(@file), err);
}

public static void ExampleFileServer_dotFileHiding() {
    var fsys = new dotFileHidingFileSystem(((Δhttp.Dir)(@string)"."u8));
    Δhttp.Handle("/"u8, Δhttp.FileServer(fsys));
    log.Fatal(Δhttp.ListenAndServe(":8080"u8, default!));
}

} // end http_test_package

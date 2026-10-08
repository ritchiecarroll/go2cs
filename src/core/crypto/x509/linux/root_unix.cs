// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris || wasip1
namespace go.crypto;

using fs = go.io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using go.io;
using path;

partial class x509_package {

internal static readonly @string certFileEnv = "SSL_CERT_FILE"u8;
internal static readonly @string certDirEnv = "SSL_CERT_DIR"u8;

internal static (slice<slice<ж<Certificate>>> chains, error err) systemVerify(this ref Certificate c, ж<VerifyOptions> Ꮡopts) {
    return (default!, default!);
}

internal static (ж<CertPool>, error) loadSystemRoots() {
    var roots = NewCertPool();
    var files = certFiles;
    {
        @string f = os.Getenv(certFileEnv); if (f != ""u8) {
            files = new @string[]{f}.slice();
        }
    }
    error firstErr = default!;
    foreach (var (_, @file) in files) {
        var (data, err) = os.ReadFile(@file);
        if (err == default!) {
            roots.AppendCertsFromPEM(data);
            break;
        }
        if (firstErr == default! && !os.IsNotExist(err)) {
            firstErr = err;
        }
    }
    var dirs = certDirectories;
    {
        @string d = os.Getenv(certDirEnv); if (d != ""u8) {
            // OpenSSL and BoringSSL both use ":" as the SSL_CERT_DIR separator.
            // See:
            //  * https://golang.org/issue/35325
            //  * https://www.openssl.org/docs/man1.0.2/man1/c_rehash.html
            dirs = strings.Split(d, ":"u8);
        }
    }
    foreach (var (_, directory) in dirs) {
        var (fis, err) = readUniqueDirectoryEntries(directory);
        if (err != default!) {
            if (firstErr == default! && !os.IsNotExist(err)) {
                firstErr = err;
            }
            continue;
        }
        foreach (var (_, fi) in fis) {
            var (data, errΔ1) = os.ReadFile(directory + "/"u8 + fi.Name());
            if (errΔ1 == default!) {
                roots.AppendCertsFromPEM(data);
            }
        }
    }
    if (roots.len() > 0 || firstErr == default!) {
        return (roots, default!);
    }
    return (default!, firstErr);
}

// readUniqueDirectoryEntries is like os.ReadDir but omits
// symlinks that point within the directory.
internal static (slice<fs.DirEntry>, error) readUniqueDirectoryEntries(@string dir) {
    var (files, err) = os.ReadDir(dir);
    if (err != default!) {
        return (default!, err);
    }
    var uniq = files[..0];
    foreach (var (_, f) in files) {
        if (!isSameDirSymlink(f, dir)) {
            uniq = append(uniq, f);
        }
    }
    return (uniq, default!);
}

// isSameDirSymlink reports whether fi in dir is a symlink with a
// target not containing a slash.
internal static bool isSameDirSymlink(fs.DirEntry f, @string dir) {
    if ((fs.FileMode)(f.Type() & fs.ModeSymlink) == 0) {
        return false;
    }
    var (target, err) = os.Readlink(filepath.Join(dir, f.Name()));
    return err == default! && !strings.Contains(target, "/"u8);
}

} // end x509_package

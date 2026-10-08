// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package lazyregexp is a thin wrapper over regexp, allowing the use of global
// regexp variables without forcing them to be compiled at init.
namespace go.@internal;

using os = os_package;
using regexp = regexp_package;
using strings = strings_package;
using sync = go.sync_package;
using go;

partial class lazyregexp_package {

// Regexp is a wrapper around regexp.Regexp, where the underlying regexp will be
// compiled the first time it is needed.
partial struct Regexp {
    internal @string str;
    internal sync.Once once;
    internal ж<regexp.Regexp> rx;
}

internal static ж<regexp.Regexp> re(this ж<Regexp> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.of(Regexp.Ꮡonce).Do(Ꮡr.build);
    return r.rx;
}

internal static void build(this ref Regexp r) {
    r.rx = regexp.MustCompile(r.str);
    r.str = ""u8;
}

public static slice<slice<byte>> FindSubmatch(this ж<Regexp> Ꮡr, slice<byte> s) {
    return Ꮡr.re().FindSubmatch(s);
}

public static slice<@string> FindStringSubmatch(this ж<Regexp> Ꮡr, @string s) {
    return Ꮡr.re().FindStringSubmatch(s);
}

public static slice<nint> FindStringSubmatchIndex(this ж<Regexp> Ꮡr, @string s) {
    return Ꮡr.re().FindStringSubmatchIndex(s);
}

public static @string ReplaceAllString(this ж<Regexp> Ꮡr, @string src, @string repl) {
    return Ꮡr.re().ReplaceAllString(src, repl);
}

public static @string FindString(this ж<Regexp> Ꮡr, @string s) {
    return Ꮡr.re().FindString(s);
}

public static slice<@string> FindAllString(this ж<Regexp> Ꮡr, @string s, nint n) {
    return Ꮡr.re().FindAllString(s, n);
}

public static bool MatchString(this ж<Regexp> Ꮡr, @string s) {
    return Ꮡr.re().MatchString(s);
}

public static slice<@string> SubexpNames(this ж<Regexp> Ꮡr) {
    return Ꮡr.re().SubexpNames();
}

internal static bool inTest = len(os.Args) > 0 && strings.HasSuffix(strings.TrimSuffix(os.Args[0], ".exe"u8), ".test"u8);

// New creates a new lazy regexp, delaying the compiling work until it is first
// needed. If the code is being run as part of tests, the regexp compiling will
// happen immediately.
public static ж<Regexp> New(@string str) {
    var lr = Ꮡ(new Regexp(str: str));
    if (inTest) {
        // In tests, always compile the regexps early.
        lr.re();
    }
    return lr;
}

} // end lazyregexp_package

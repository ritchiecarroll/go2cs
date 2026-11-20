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
using static lazyregexp_package;

partial class lazyregexp_package {

// Regexp is a wrapper around regexp.Regexp, where the underlying regexp will be
// compiled the first time it is needed.
partial struct Regexp {
    internal @string str;
    internal sync.Once once;
    internal ж<regexp.Regexp> rx;
}

} partial class lazyregexp_package {

internal static ж<regexp.Regexp> reᴍ(ж<Regexp> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.Ꮡonce.Do(Ꮡr.build);
    return r.rx;
}

internal static void buildᴍ(ref Regexp r) {
    r.rx = regexp.MustCompile(r.str);
    r.str = ""u8;
}

internal static slice<slice<byte>> FindSubmatchᴍ(ж<Regexp> Ꮡr, slice<byte> s) {
    return Ꮡr.re().FindSubmatch(s);
}

internal static slice<@string> FindStringSubmatchᴍ(ж<Regexp> Ꮡr, @string s) {
    return Ꮡr.re().FindStringSubmatch(s);
}

internal static slice<nint> FindStringSubmatchIndexᴍ(ж<Regexp> Ꮡr, @string s) {
    return Ꮡr.re().FindStringSubmatchIndex(s);
}

internal static @string ReplaceAllStringᴍ(ж<Regexp> Ꮡr, @string src, @string repl) {
    return Ꮡr.re().ReplaceAllString(src, repl);
}

internal static @string FindStringᴍ(ж<Regexp> Ꮡr, @string s) {
    return Ꮡr.re().FindString(s);
}

internal static slice<@string> FindAllStringᴍ(ж<Regexp> Ꮡr, @string s, nint n) {
    return Ꮡr.re().FindAllString(s, n);
}

internal static bool MatchStringᴍ(ж<Regexp> Ꮡr, @string s) {
    return Ꮡr.re().MatchString(s);
}

internal static slice<@string> SubexpNamesᴍ(ж<Regexp> Ꮡr) {
    return Ꮡr.re().SubexpNames();
}

} partial class lazyregexp_package {

internal static bool inTest = len(os.Args) > 0 && strings.HasSuffix(strings.TrimSuffix(os.Args[0], ".exe"), ".test");

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

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class lazyregexpᴛRegexp {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<regexp.Regexp> re(this ж<Regexp> Ꮡr) => lazyregexp_package.reᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void build(this ref Regexp r) => lazyregexp_package.buildᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<slice<byte>> FindSubmatch(this ж<Regexp> Ꮡr, slice<byte> s) => lazyregexp_package.FindSubmatchᴍ(Ꮡr, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> FindStringSubmatch(this ж<Regexp> Ꮡr, @string s) => lazyregexp_package.FindStringSubmatchᴍ(Ꮡr, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<nint> FindStringSubmatchIndex(this ж<Regexp> Ꮡr, @string s) => lazyregexp_package.FindStringSubmatchIndexᴍ(Ꮡr, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string ReplaceAllString(this ж<Regexp> Ꮡr, @string src, @string repl) => lazyregexp_package.ReplaceAllStringᴍ(Ꮡr, src, repl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string FindString(this ж<Regexp> Ꮡr, @string s) => lazyregexp_package.FindStringᴍ(Ꮡr, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> FindAllString(this ж<Regexp> Ꮡr, @string s, nint n) => lazyregexp_package.FindAllStringᴍ(Ꮡr, s, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool MatchString(this ж<Regexp> Ꮡr, @string s) => lazyregexp_package.MatchStringᴍ(Ꮡr, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> SubexpNames(this ж<Regexp> Ꮡr) => lazyregexp_package.SubexpNamesᴍ(Ꮡr);
}
// </GlyphForwarders>

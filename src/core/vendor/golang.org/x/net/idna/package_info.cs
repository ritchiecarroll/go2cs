// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.vendor.golang.org.x.net.idna_package;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<labelError, error>(Pointer = true)]
[assembly: GoImplement<runeError, error>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<info, category>(Inverted = true, ValueType = "uint16")]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Profile))]
[assembly: GoFieldAccess(typeof(options))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("vendor/golang.org/x/net/idna/idna10.0.0.go", "idna10.0.0.cs", "AB9cABACqJIABRbSgL7CgLqigAADENKmgpSCgoKClAAEFNKAvsKCggADHAAKAoAAAxTygLqigoKCgoIAAxgACAKCgoIAGj6CggADGAAKAoKCzsKuwoKCAAgKooKClJSClIKUgpSClPzKuLiUggAKFAAJEgAUJIKCAAgKgoLKwoKCgqaC3IKUgoKCpoKUlIKCgoKCppSClIKCgpS4lIKmgoKCgriCgoKCgoKUgpSCgriClIKClIKmxoKCgqam6IKCpsSClIKCgpSU6oKUlMaCgriClJTGwgABEMKCgoKCgoKClJSCgoKUlLSCgpS0grTGgqSUlIKmgoKmlAANGIKCgqaCpoKClKaCgpSCgoKUqJKCgoKmgoLKgoKUAAoMgpSClMaClMaC2KTGooK4goKClIKUlAAQJgAqYKKCgpSUgoKUgqaClJSCgoKmgpSCgoKCgoKUgoKUgpSClIKUxoKCgqY=", "64-64:1;72-72:1;78-78:1;87-101:1;110-110:1;118-121:1;135-135:1;146-146:1;152-158:1;170-174:1")]
[assembly: GoPositionMap("vendor/golang.org/x/net/idna/punycode.go", "punycode.cs", "ABo+lrKClIKClIKUgoKCpoKCgoKCgpSCgpSCgoKUgoKCgpSClIKCpoKUgoKCgoKUgoKClAACEPKCgoKCgoKClKaCgpSCgoKCgqaCgpSCgoKCgpSUgpSCgoKCgoKUgpSClIKCgoKUgpSokoKClKaClKSklKaClKSUqJKClJSCgoKClA==")]
[assembly: GoPositionMap("vendor/golang.org/x/net/idna/tables15.0.0.go", "tables15.0.0.cs", "AA9yAPwBxgeygpSkpIKUgoKClKSClIKCgpSCgoKClKSClIKCgpSCgoKClIKCgoKUpqqigoKUgoKUgoKUgoKUrLKClKSkgpSCgoKUpIKUgoKClIKCgoKUpIKUgoKClIKCgoKUgoKCgpSmqqKCgpSCgpSCgpSCgpQACAyCyJKUpIL8AM0MnBkAuQL0BKY=")]
[assembly: GoPositionMap("vendor/golang.org/x/net/idna/trie.go", "trie.cs", "ABUq7LzCgoKCgoKCgoKUgpSm")]
[assembly: GoPositionMap("vendor/golang.org/x/net/idna/trie13.0.0.go", "trie13.0.0.cs", "AA0aooKCgpSClJSCgqY=")]
[assembly: GoPositionMap("vendor/golang.org/x/net/idna/trieval.go", "trieval.cs", "AFq8AYKmgoKClKaCgpSmgqaC")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.net;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using fmt = fmt_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;
using bidirule = go.vendor.golang.org.x.text.secure.bidirule_package;
using bidi = go.vendor.golang.org.x.text.unicode.bidi_package;
using norm = go.vendor.golang.org.x.text.unicode.norm_package;
using go.vendor.golang.org.x.text.secure;
using go.vendor.golang.org.x.text.unicode;
using unicode;
using static idna_package;
// </GlyphForwarderUsings>

[GoPackage("idna", ImportPath = "vendor/golang.org/x/net/idna")]
public static partial class idna_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct category {}
    internal partial struct idnaTrie {}
    internal partial struct info {}
    internal partial struct joinState {}
    internal partial struct labelError {}
    internal partial struct labelIter {}
    internal partial struct runeError {}
    internal partial struct sparseBlocks {}
    internal partial struct valueRange {}
    public partial struct Profile {}
    public partial struct options {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸtextꓸsecureꓸbidirule() => builtin.initPackage(typeof(go.vendor.golang.org.x.text.secure.bidirule_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸtextꓸunicodeꓸbidi() => builtin.initPackage(typeof(go.vendor.golang.org.x.text.unicode.bidi_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸtextꓸunicodeꓸnorm() => builtin.initPackage(typeof(go.vendor.golang.org.x.text.unicode.norm_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(idna_package.Profile)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class idnaᴛProfile;
[GoMethodHost(typeof(idna_package.idnaTrie)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class idnaᴛidnaTrie;
[GoMethodHost(typeof(idna_package.info)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class idnaᴛinfo;
[GoMethodHost(typeof(idna_package.labelError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class idnaᴛlabelError;
[GoMethodHost(typeof(idna_package.labelIter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class idnaᴛlabelIter;
[GoMethodHost(typeof(idna_package.runeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class idnaᴛruneError;
[GoMethodHost(typeof(idna_package.sparseBlocks)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class idnaᴛsparseBlocks;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class idnaᴛProfile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@string, error) ToASCII(this ж<Profile> Ꮡp, @string s) => idna_package.ToASCIIᴍ(Ꮡp, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@string, error) ToUnicode(this ref Profile p, @string s) => idna_package.ToUnicodeᴍ(ref p, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref Profile p) => idna_package.Stringᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, error) process(this ж<Profile> Ꮡp, @string s, bool toASCII) => idna_package.processᴍ(Ꮡp, s, toASCII);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static category simplify(this ref Profile p, category cat) => idna_package.simplifyᴍ(ref p, cat);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error /*err*/ validateLabel(this ref Profile p, @string s) => idna_package.validateLabelᴍ(ref p, s);
}
partial class idnaᴛlabelError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string code(this labelError e) => idna_package.codeᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this labelError e) => idna_package.Errorᴍ(e);
}
partial class idnaᴛruneError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string code(this runeError e) => idna_package.codeᴍ(e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this runeError e) => idna_package.Errorᴍ(e);
}
partial class idnaᴛinfo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isBidi(this info c, @string s) => idna_package.isBidiᴍ(c, s);
}
partial class idnaᴛlabelIter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ref labelIter l) => idna_package.resetᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool done(this ref labelIter l) => idna_package.doneᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string result(this ref labelIter l) => idna_package.resultᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string label(this ref labelIter l) => idna_package.labelᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void next(this ref labelIter l) => idna_package.nextᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set(this ref labelIter l, @string s) => idna_package.setᴍ(ref l, s);
}
partial class idnaᴛidnaTrie {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint16 v, nint sz) lookup(this ref idnaTrie t, slice<byte> s) => idna_package.lookupᴍ(ref t, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 lookupUnsafe(this ref idnaTrie t, slice<byte> s) => idna_package.lookupUnsafeᴍ(ref t, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint16 v, nint sz) lookupString(this ref idnaTrie t, @string s) => idna_package.lookupStringᴍ(ref t, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 lookupStringUnsafe(this ref idnaTrie t, @string s) => idna_package.lookupStringUnsafeᴍ(ref t, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 lookupValue(this ref idnaTrie t, uint32 n, byte b) => idna_package.lookupValueᴍ(ref t, n, b);
}
partial class idnaᴛsparseBlocks {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 lookup(this ref sparseBlocks t, uint32 n, byte b) => idna_package.lookupᴍ(ref t, n, b);
}
partial class idnaᴛinfo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> appendMapping(this info c, slice<byte> b, @string s) => idna_package.appendMappingᴍ(c, b, s);
}
partial class idnaᴛinfo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isMapped(this info c) => idna_package.isMappedᴍ(c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static category category(this info c) => idna_package.categoryᴍ(c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static info joinType(this info c) => idna_package.joinTypeᴍ(c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isModifier(this info c) => idna_package.isModifierᴍ(c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isViramaModifier(this info c) => idna_package.isViramaModifierᴍ(c);
}
// </GlyphForwarders>

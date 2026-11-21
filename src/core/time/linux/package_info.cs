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
using static go.time_package;

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
[assembly: GoImplement<ParseError, error>(Pointer = true)]
[assembly: GoImplement<fileSizeError, error>]
[assembly: GoImplement<parseDurationError, error>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Weekday, absDays>(Inverted = true, ValueType = "nint")]
[assembly: GoImplicitConv<absJanFeb, Month>(Inverted = true, ValueType = "nint")]
[assembly: GoImplicitConv<absMonth, Month>(Inverted = true, ValueType = "nint")]
[assembly: GoImplicitConv<absSeconds, absDays>(Inverted = true, ValueType = "uint64")]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Location))]
[assembly: GoFieldAccess(typeof(Timer))]
[assembly: GoFieldAccess(typeof(zone))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("time/format.go", "format.cs", "AMMB5gKYooKUggACHgAMAoKkgoKUgtqCgoKUgqaC2oKUgsiClKaClKaUgpSUgsimpqaCyILIgpSClIKUgpSCyIKUgpSClIKUgsiCgoKCpoKCgpSC2qYACRQACRQADh4ADiKigoKClIKCgrimgoKCpqyygoKCuIKUpKiSgpSCuIK4gpS4goKCgoKUgriW4oKCgpSCgoKUgpSsxIKUpoKmgoKUqqKCgoKUgoKCgpSCgpSCpgAIHAANAqaCgoKCgpSSkoKCgoKCgpSCgoKUqsKCgpSCgoKCgriUgoKCgoKCgoKCgoKktAASIoKCpIIAAhLigoKCgoKUlIKqtJSkpLiCgpQACBaCgoKUgpSmgpSCuIKmlIKClLS0tIK0tLS0grS0gpS0tIKCgqa0tMaCgpTGgoKUtLS0tLSClMaClOqCgpSCgoKCgpSUgoKUgriCgpTIgoK4goKClJSCtLbGAAoaooKCAAkWooKCgo7igoKCppSCgoKmgpSmgsiSgpzKsoKUgqyygpSCgpSUrLKSgpSClKaCgpSqooKCgpSCgpSClIKUAAJaACsEgoKmAAIQ5IKCpgAaBoKSgoKWAA0egoKCgoKClIKClJSCgoK0goKUgoKClIKU1oKClIK0gqSCpIKCyKSkgpTIgoKmyIKCtoKCtoKCxoKClIKCuIKCgpSmgpSC1oKClIKUtLTmgoKUgpS0tOaCgoKUxLKCgoKUgoKUgoKCgpSCgoKClIKClIKCgoKUlIKClJSigoKCgu6ClIKUgqaCxrT4goKClIKCgpTqgoKClILGlLiCgpSCpIKUgqaCgoK4goKCgoKCgoKmgpSCgoKUuIKUgoKUlIKUgsqCpoKmgoK4goKCuIKCpoKmgoKCgriCgpSCgrgAAhoADAKCpoKmgoKmgoKClpKCgpSCppSkgtiCxpSssoKCpqyygoKUpoKUgpSmgubigoKUgoKUgpSCgriCgpSmluKCgoKClJSUgpSmrOKCgoKCgoKUgpSUgpSCgoKUgpQAChKCxgAKIOSCgqaCgoKCuIKUgpSCuqaCpoKCgpSmgoKCgoKUlLiCgoKCpoKUgoKCgpSUlIKmgpSmgoKmgpSClA==", "426-426:1", "", "1008=quote/1/4/1,quote/2/4/2,quote/3/4/3,quote/4/4/4;1010=quote/1/1/1")]
[assembly: GoPositionMap("time/format_rfc3339.go", "format_rfc3339.cs", "AAwkAAgCpoKCgoKClKaCgoKCgpSCgqaCuIKCgpSUgoKCAAgGgoLKgpSkgoK2xuqCkoKCgpSUgoKUuIKUgoKCgoKCgpSWkoKClIK4goKClIKCgpSCgpSmgpSmpoKCgoKC3IK4pKSklKTGpg==", "69-69:1;88-101:1;166-166:1")]
[assembly: GoPositionMap("time/sleep.go", "sleep.cs", "AAwctAADSMKClIKmlAACFAAIBqYAEkYAFQKClAAENAAXAoKCggAEMAAXAoKUgsjsAAkgAAoCrsKmgg==")]
[assembly: GoPositionMap("time/sys_unix.go", "sys_unix.cs", "AAwg7IK4goKClKaCpoLmgoKClICCpIKCgoKUlJQ=")]
[assembly: GoPositionMap("time/time.go", "time.cs", "AKwB4ALSqJKClKiAlpKCgoKCgri4goKCgpS6soKUgqiSgoIAAxDCgoKClJQAAhDSgpSq6qiSgpSCgqiSgpSCgqqikoKUgoKmlKSUAAIQ0oKUABYskoKUgoIAESKSgpSCggDgAcgDxIKCgpSCABMolIKCgpTIksjkggAJFAARJIKCgsgADx6CqKSCgpTIpMikgoKUgoKUgoKUqKTIpMjSgoKCgoKowoKCgoLOwpSClIKCgpSCpurigoKmgoKCgpSUlJSCqJKokoKCqJKCgqiSgoKoksikzgAQFoKCgqiSyNKCgoKCgsiSqJKokqqiqqKCACFMxqKCqtSUgoKCpoaigoKClIK2gsaUgsaCpIKUgpSmgqaCgoKCuIKCgsqCgqbO1IKCgoKCgoKUlIKClKqigoKClIKCgqbIgJaAloAAARgACgKCgqiSgoKokoKCqqKClMqiAAQS4oKUgoKCgpSClJSClICCpKyylKSk2pKCgoKCgoKClIKCgoKUlKauwoKUlJSkpNiCgoKUgpSqopSUqqKUlAAEKAARAoKCyqKCggAMGqaCgoKU3AACJAAQDNzWAAgQnMKCgpSCgriUpoLIkoKokoIAAhDSgpSCqJKCgpSq0oIAAhAACAKCgoKUgoKUAAIS4gACENIAAhDSAAIS4gANEpKCgpSClIKCgqaCgpSmgoIADyKClKiSgoKUAAoIkoKCpoKCppKClIKmgpaClIKCgqaCgpSCgoKUpgACENKokq7CgoKCgoKU6qKCpoKUgoKCpoKCgpTuwu7CqqKCggAEENKCgoKCgoKmqqKqosiSgsbKgoKUrsKCgoKUgoKClAACKgASAoK4goKmgoKCpgAFEIKCpoKUpoKCAAQU8oKClIIAAhYACAKCgpSCgpTM4oKCgpSCgoKCgrimgsiCggAIEIKCgoKCkoKUgoLKgoKUgoKClIKCgpSUgpSCgpS27oKUAAJAAB0CqJKo4oKCgoI=")]
[assembly: GoPositionMap("time/zoneinfo.go", "zoneinfo.cs", "AEOOAcoACRAACg6CgpSClKqiAAgQpqKCgoKCgoKmlKaC7oIACBQADgKUgoKCgoKCpoKCgoKCgqaCgoKCgoKUlILKgoKCgoKCgoKClKaCgoKUuIKCuAACJAAQBIK4goKC3IKCyqqigoKmAAgSAAgCyoKClILclJSmgoKClIKmgqaUpoKUlJKCgpSCgoLKgoKUgoK4goKCgu6CgoKUvKKClIKClIKUpoKUlIKCpr7SgpSCgoKCgqqigoKUgoKClKaCgoKUgoKClKaCgoKUlIKUABIuooKClIKCgoKClIKCgoKCgpaCgoKUgoKClIKCgpSCgoKUgqaCgqaCgpSUrNKClIKCgoKUlIKCgqaClKyygpSCgsbGgoKClIKCgoK4goKUgoKUlIKClLbM8gAHEIKCgoKC3IKCgsrsAAkoAA8CgpSClKaUgpKUgoKCgpSCgqaAgoKCpKiSgpSCgqY=", "119-124:1;675-678:1")]
[assembly: GoPositionMap("time/zoneinfo_goroot.go", "zoneinfo_goroot.cs", "AAgSgoKU")]
[assembly: GoPositionMap("time/zoneinfo_read.go", "zoneinfo_read.cs", "ABMwwgARIIIAESCCgoKClIKCpoKCgoKUpoKCgoKClKaCgoKClKiSgoLIkoKUppzCpoKokoKClJS0tLQAAxYADBCCgoKClIKUAAcQgpQAARCClKaCgoKUgpS4goK4pqampri4lIKmgoKC3IKmlIKCgoKClIKUgoKClIKClIKmlNyCgoKCgpSmgpSmgoKUgoKUgrimuLiCgoKCgoKCgqaygpSClAAHELimgoKCpqqigpSClAACGAAJAoKUqJKClKrCgoKUlAAGEoKClIKClIKCpgAXMIKUgoKCgoKCgoKClIIAEiaCipSUgoKmpgAKGMKClK7ygoKCgqaCpoKCgoKmgqaAgoKCgqaCtoKUruKCgpSCqoKCgpSClIKm")]
[assembly: GoPositionMap("time/zoneinfo_unix.go", "zoneinfo_unix.cs", "ABEqAA4OAAcQgpSCgoKCxoKUgoKCgpSUlIKCguw=")]
// </GoSourcePositionMaps>

namespace go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using @internal;
using static time_package;
using math;
// </GlyphForwarderUsings>

[GoPackage("time")]
public static partial class time_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct absCentury {}
    internal partial struct absCyear {}
    internal partial struct absDays {}
    internal partial struct absJanFeb {}
    internal partial struct absLeap {}
    internal partial struct absMonth {}
    internal partial struct absSeconds {}
    internal partial struct absYday {}
    internal partial struct dataIO {}
    internal partial struct fileSizeError {}
    internal partial struct parseDurationError {}
    internal partial struct rule {}
    internal partial struct ruleKind {}
    internal partial struct zone {}
    internal partial struct zoneTrans {}
    public partial struct Duration {}
    public partial struct Location {}
    public partial struct Month {}
    public partial struct ParseError {}
    public partial struct Time {}
    public partial struct Timer {}
    public partial struct Weekday {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(time_package.Duration)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛDuration;
[GoMethodHost(typeof(time_package.Location)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛLocation;
[GoMethodHost(typeof(time_package.Month)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛMonth;
[GoMethodHost(typeof(time_package.ParseError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛParseError;
[GoMethodHost(typeof(time_package.Ticker)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛTicker;
[GoMethodHost(typeof(time_package.Time)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛTime;
[GoMethodHost(typeof(time_package.Timer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛTimer;
[GoMethodHost(typeof(time_package.Weekday)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛWeekday;
[GoMethodHost(typeof(time_package.absCentury)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛabsCentury;
[GoMethodHost(typeof(time_package.absDays)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛabsDays;
[GoMethodHost(typeof(time_package.absMonth)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛabsMonth;
[GoMethodHost(typeof(time_package.absSeconds)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛabsSeconds;
[GoMethodHost(typeof(time_package.absYday)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛabsYday;
[GoMethodHost(typeof(time_package.dataIO)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛdataIO;
[GoMethodHost(typeof(time_package.fileSizeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛfileSizeError;
[GoMethodHost(typeof(time_package.parseDurationError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class timeᴛparseDurationError;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class timeᴛTime {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Time t) => time_package.Stringᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this Time t) => time_package.GoStringᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Format(this Time t, @string layout) => time_package.Formatᴍ(t, layout);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> AppendFormat(this Time t, slice<byte> b, @string layout) => time_package.AppendFormatᴍ(t, b, layout);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> appendFormat(this Time t, slice<byte> b, @string layout) => time_package.appendFormatᴍ(t, b, layout);
}
partial class timeᴛParseError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref ParseError e) => time_package.Errorᴍ(ref e);
}
partial class timeᴛparseDurationError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this ref parseDurationError e) => time_package.Errorᴍ(ref e);
}
partial class timeᴛTime {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> appendFormatRFC3339(this Time t, slice<byte> b, bool nanos) => time_package.appendFormatRFC3339ᴍ(t, b, nanos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) appendStrictRFC3339(this Time t, slice<byte> b) => time_package.appendStrictRFC3339ᴍ(t, b);
}
partial class timeᴛTimer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Stop(this ж<Timer> Ꮡt) => time_package.Stopᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Reset(this ж<Timer> Ꮡt, Duration d) => time_package.Resetᴍ(Ꮡt, d);
}
partial class timeᴛTime {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int32 nsec(this ref Time t) => time_package.nsecᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 sec(this ref Time t) => time_package.secᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 unixSec(this ref Time t) => time_package.unixSecᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addSec(this ref Time t, int64 d) => time_package.addSecᴍ(ref t, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setLoc(this ref Time t, ж<Location> Ꮡloc) => time_package.setLocᴍ(ref t, Ꮡloc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void stripMono(this ref Time t) => time_package.stripMonoᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setMono(this ref Time t, int64 m) => time_package.setMonoᴍ(ref t, m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 mono(this ref Time t) => time_package.monoᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsZero(this Time t) => time_package.IsZeroᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool After(this Time t, Time u) => time_package.Afterᴍ(t, u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Before(this Time t, Time u) => time_package.Beforeᴍ(t, u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Compare(this Time t, Time u) => time_package.Compareᴍ(t, u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Equal(this Time t, Time u) => time_package.Equalᴍ(t, u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static absSeconds absSec(this Time t) => time_package.absSecᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string name, nint offset, absSeconds abs) locabs(this Time t) => time_package.locabsᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint year, Month month, nint day) Date(this Time t) => time_package.Dateᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Year(this Time t) => time_package.Yearᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Month Month(this Time t) => time_package.Monthᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Day(this Time t) => time_package.Dayᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Weekday Weekday(this Time t) => time_package.Weekdayᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint year, nint week) ISOWeek(this Time t) => time_package.ISOWeekᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint hour, nint min, nint sec) Clock(this Time t) => time_package.Clockᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Hour(this Time t) => time_package.Hourᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Minute(this Time t) => time_package.Minuteᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Second(this Time t) => time_package.Secondᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Nanosecond(this Time t) => time_package.Nanosecondᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint YearDay(this Time t) => time_package.YearDayᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Time Add(this Time t, Duration d) => time_package.Addᴍ(t, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Duration Sub(this Time t, Time u) => time_package.Subᴍ(t, u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Time AddDate(this Time t, nint years, nint months, nint days) => time_package.AddDateᴍ(t, years, months, days);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Time UTC(this Time t) => time_package.UTCᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Time Local(this Time t) => time_package.Localᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Time In(this Time t, ж<Location> loc) => time_package.Inᴍ(t, loc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Location> Location(this Time t) => time_package.Locationᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@string name, nint offset) Zone(this Time t) => time_package.Zoneᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Time start, Time end) ZoneBounds(this Time t) => time_package.ZoneBoundsᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 Unix(this Time t) => time_package.Unixᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 UnixMilli(this Time t) => time_package.UnixMilliᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 UnixMicro(this Time t) => time_package.UnixMicroᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 UnixNano(this Time t) => time_package.UnixNanoᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) AppendBinary(this Time t, slice<byte> b) => time_package.AppendBinaryᴍ(t, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) MarshalBinary(this Time t) => time_package.MarshalBinaryᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnmarshalBinary(this ref Time t, slice<byte> data) => time_package.UnmarshalBinaryᴍ(ref t, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) GobEncode(this Time t) => time_package.GobEncodeᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error GobDecode(this ref Time t, slice<byte> data) => time_package.GobDecodeᴍ(ref t, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) MarshalJSON(this Time t) => time_package.MarshalJSONᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnmarshalJSON(this ref Time t, slice<byte> data) => time_package.UnmarshalJSONᴍ(ref t, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) appendTo(this Time t, slice<byte> b, @string errPrefix) => time_package.appendToᴍ(t, b, errPrefix);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) AppendText(this Time t, slice<byte> b) => time_package.AppendTextᴍ(t, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) MarshalText(this Time t) => time_package.MarshalTextᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnmarshalText(this ref Time t, slice<byte> data) => time_package.UnmarshalTextᴍ(ref t, data);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsDST(this Time t) => time_package.IsDSTᴍ(t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Time Truncate(this Time t, Duration d) => time_package.Truncateᴍ(t, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Time Round(this Time t, Duration d) => time_package.Roundᴍ(t, d);
}
partial class timeᴛMonth {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Month m) => time_package.Stringᴍ(m);
}
partial class timeᴛWeekday {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Weekday d) => time_package.Stringᴍ(d);
}
partial class timeᴛabsSeconds {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static absDays days(this absSeconds abs) => time_package.daysᴍ(abs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint hour, nint min, nint sec) clock(this absSeconds abs) => time_package.clockᴍ(abs);
}
partial class timeᴛabsDays {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (absCentury century, absCyear cyear, absYday ayday) split(this absDays days) => time_package.splitᴍ(days);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint year, Month month, nint day) date(this absDays days) => time_package.dateᴍ(days);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint year, nint yday) yearYday(this absDays days) => time_package.yearYdayᴍ(days);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Weekday weekday(this absDays days) => time_package.weekdayᴍ(days);
}
partial class timeᴛabsYday {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (absMonth m, nint mday) split(this absYday ayday) => time_package.splitᴍ(ayday);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static absJanFeb janFeb(this absYday ayday) => time_package.janFebᴍ(ayday);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint yday(this absYday ayday, absJanFeb janFeb, absLeap leap) => time_package.ydayᴍ(ayday, janFeb, leap);
}
partial class timeᴛabsMonth {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Month month(this absMonth m, absJanFeb janFeb) => time_package.monthᴍ(m, janFeb);
}
partial class timeᴛabsCentury {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static absLeap leap(this absCentury century, absCyear cyear) => time_package.leapᴍ(century, cyear);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint year(this absCentury century, absCyear cyear, absJanFeb janFeb) => time_package.yearᴍ(century, cyear, janFeb);
}
partial class timeᴛDuration {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Duration d) => time_package.Stringᴍ(d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint format(this Duration d, /*[32]*/ ж<array<byte>> Ꮡbuf) => time_package.formatᴍ(d, Ꮡbuf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 Nanoseconds(this Duration d) => time_package.Nanosecondsᴍ(d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 Microseconds(this Duration d) => time_package.Microsecondsᴍ(d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 Milliseconds(this Duration d) => time_package.Millisecondsᴍ(d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static float64 Seconds(this Duration d) => time_package.Secondsᴍ(d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static float64 Minutes(this Duration d) => time_package.Minutesᴍ(d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static float64 Hours(this Duration d) => time_package.Hoursᴍ(d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Duration Truncate(this Duration d, Duration m) => time_package.Truncateᴍ(d, m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Duration Round(this Duration d, Duration m) => time_package.Roundᴍ(d, m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Duration Abs(this Duration d) => time_package.Absᴍ(d);
}
partial class timeᴛLocation {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Location> get(this ж<Location> Ꮡl) => time_package.getᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Location> Ꮡl) => time_package.Stringᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string name, nint offset, int64 start, int64 end, bool isDST) lookup(this ж<Location> Ꮡl, int64 sec) => time_package.lookupᴍ(Ꮡl, sec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint lookupFirstZone(this ref Location l) => time_package.lookupFirstZoneᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool firstZoneUsed(this ref Location l) => time_package.firstZoneUsedᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint offset, bool ok) lookupName(this ж<Location> Ꮡl, @string name, int64 unix) => time_package.lookupNameᴍ(Ꮡl, name, unix);
}
partial class timeᴛfileSizeError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this fileSizeError f) => time_package.Errorᴍ(f);
}
partial class timeᴛdataIO {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> read(this ref dataIO d, nint n) => time_package.readᴍ(ref d, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint32 n, bool ok) big4(this ref dataIO d) => time_package.big4ᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint64 n, bool ok) big8(this ref dataIO d) => time_package.big8ᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (byte n, bool ok) @byte(this ref dataIO d) => time_package.@byteᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> rest(this ref dataIO d) => time_package.restᴍ(ref d);
}
// </GlyphForwarders>

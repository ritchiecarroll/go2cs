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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
[assembly: GoFieldAccess(typeof(syscall_package.Systemtime))]
[assembly: GoFieldAccess(typeof(syscall_package.Timezoneinformation))]
[assembly: GoFieldAccess(typeof(zone))]
[assembly: GoFieldAccess(typeof(zoneTrans))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("time/format.go", "format.cs", "AMEB5gKYooKUggACHgAMAoKkgoKUgtqCgoKUgqaC2oKUgsiClKaClKaUgpSUgsimpqaCyILIgpSClIKUgpSCyIKUgpSClIKUgsiCgoKCpoKCgpSC2qYACRQACRQADh4ADiKigoKClIKCgrimgoKCpqyygoKCuIKUpKiSgpSCuIK4gpS4goKCgoKUgriW4oKCgpSCgoKUgpSsxIKUpoKmgoKUqqKCgoKUgoKCgpSCgpSCpgAGHAANAqaCgoKCgpSSkoKCgoKCgpSCgoKUqsKCgpSCgoKCgriUgoKCgoKCgoKCgoKktAASIoKCpIIAAhLigoKCgoKUlIKqtJSkpLiCgpQACBaCgoKUgpSmgpSCuIKmlIKClLS0tIK0tLS0grS0gpS0tIKCgqa0tMaCgpTGgoKUtLS0tLSClMaClOqCgpSCgoKCgpSUgoKUgriCgpTIgoK4goKClJSCtLamAAoaooKCAAkWooKCgo7igoKCppSCgoKmgpSmgqiSgpyqsoKUgqyygpSCgpSUrLKSgpSClKaCgpSqooKCgpSCgpSClIKUAAJaACsEgoKmAAIQ5IKCpgAaBoKSgoKWAA0egoKCgoKClIKClJSCgoK0goKUgoKClIKU1oKClIK0gqSCpIKCyKSkgpTIgoKmyIKCtoKCtoKCxoKClIKCuIKCgpSmgpSC1oKClIKUtLTmgoKUgpS0tOaCgoKUxLKCgoKUgoKUgoKCgpSCgoKClIKClIKCgoKUlIKClJSigoKCgu6ClIKUgqaCxrT4goKClIKCgpTqgoKClILGlLiCgpSCpIKUgqaCgoK4goKCgoKCgoKmgpSCgoKUuIKUgoKUlIKUgsqCpoKmgoK4goKCuIKCpoKmgoKCgriCgpSCgrgAAhoADAKCpoKmgoKmgoKClpKCgpSCppSkgtiCxpSssoKCpqyygoKUpoKUgpSmgubigoKUgoKUgpSCgriCgpSmluKCgoKClJSUgpSmrOKCgoKCgoKUgpSUgpSCgoKUgpQACBKCtgAKIOSCgqaCgoKCuIKUgpSCuqaCpoKCgpSmgoKCgoKUlLiCgoKCpoKUgoKCgpSUlIKmgpSmgoKmgpSClA==", "426-426:1", "", "1000=quote/1/4/1,quote/2/4/2,quote/3/4/3,quote/4/4/4;1002=quote/1/1/1")]
[assembly: GoPositionMap("time/format_rfc3339.go", "format_rfc3339.cs", "AAkkAAgCpoKCgoKClKaCgoKCgpSCgqaCuIKCgpSUgoKCAAgGgoLKgpSkgoK2puqCkoKCgpSUgoKUuIKUgoKCgoKCgpSWkoKClIK4goKClIKCgpSCgpSmgpSmpoKCgoKC3IK4pKSklKTGpg==", "69-69:1;88-101:1;166-166:1")]
[assembly: GoPositionMap("time/sleep.go", "sleep.cs", "AAsctAADSMKClIKmlAACFAAIBqYAEEYAFQKClAACNAAXAoKCggACMAAXAoKUgqjsAAkgAAoCrsKmgg==")]
[assembly: GoPositionMap("time/sys_windows.go", "sys_windows.cs", "AAoatoKClIKUlKaCpoLmgoKClICCpIKCgoKUlJQ=")]
[assembly: GoPositionMap("time/time.go", "time.cs", "AKgB4ALSqJKClKiAlpKCgoKCgri4goKCgpS6soKUgqiSgoIAAxDCgoKClJQAAhDSgpSq6qiSgpSCgqiSgpSCgqqikoKUgoKmlKSUAAIQ0oKUABIskoKUgoIADSKSgpSCggDeAcgDxIKCgpSCABMolIKCgpSokqjkggAJFAARJIKCgqgADx6CqKSCgpSopKikgoKUgoKUgoKUqKSopKjSgoKCgoKowoKCgoKuwpSClIKCgpSCpurigoKmgoKCgpSUlJSCqJKokoKCqJKCgqiSgoKokqikrgAQFoKCgqiSqNKCgoKCgqiSqJKokqqiqqKCAB1MxqKCqtSUgoKCpoaigoKClIK2gsaUgsaCpIKUgpSmgqaCgoKCuIKCgsqCgqau1IKCgoKCgoKUlIKClKqigoKClIKCgqaogJaAloAAARgACgKCgqiSgoKokoKCqqKClKqiAAIS4oKUgoKCgpSClJSClICCpKyylKSkupKCgoKCgoKClIKCgoKUlKauwoKUlJSkpLiCgoKUgpSqopSUqqKUlAACKAARAoKCqqKCggAMGqaCgoKU3AACJAAQDNzWAAgQnMKCgpSCgriUpoKokoKokoIAAhDSgpSCqJKCgpSq0oIAAhAACAKCgoKUgoKUAAIS4gACENIAAhDSAAIS4gAJEpKCgpSClIKCgqaCgpSmgoIADyKClKiSgoKUAAoIkoKCpoKCppKClIKmgpaClIKCgqaCgpSCgoKUpgACENKokq7CgoKCgoKU6qKCpoKUgoKCpoKCgpTuwu7CqqKCggACENKCgoKCgoKmqqKqoqiSgqbKgoKUrsKCgoKUgoKClAACKgASAoK4goKmgoKCpgAFEIKCpoKUpoKCAAIU8oKClIIAAhYACAKCgpSCgpSs4oKCgpSCgoKCgrimgsiCggAIEIKCgoKCkoKUgoLKgoKUgoKClIKCgpSUgpSCgpS27oKUAAJAAB0CqJKo4oKCgoI=")]
[assembly: GoPositionMap("time/zoneinfo.go", "zoneinfo.cs", "AEKOAcoACRAACA6CgpSClKqiAAYQpqKCgoKCgoK2lKaC7oIAAhQADgKUgoKCgoKCpoKCgoKCgqaCkoKCgoKUlILKgoKCgoKCgoKClKaCgoKUuIKCuAACJAAQBIK4goKC3IKCyqqigoKmAAYSAAgCyoKClILclJSmgoKClIKmgqaUpoKUlJKCgpSCgoLKgoKUgoK4goKCgu6CgoKUvKKClIKClIKUpoKUlIKCpr7SgpSCgoKCgqqigoKUgoKClKaCgoKUgoKClKaCgoKUlIKUABIuooKClIKCgoKClIKCgoKCgpaCgoKUgoKClIKCgpSCgoKUgqaCgqaCgpSUrNKClIKCgoKUlIKCgqaClKyygpSCgsbGgoKClIKCgoK4goKUgoKUlIKClLas8gAHEIKCgoKC3IKCgsrMAAkoAA8CgpSClKaUgpKUgoKCgpSCgqaAgoKCpKiSgpSCgqY=", "119-124:1;675-678:1")]
[assembly: GoPositionMap("time/zoneinfo_abbrs_windows.go", "zoneinfo_abbrs_windows.cs", "AA4e")]
[assembly: GoPositionMap("time/zoneinfo_goroot.go", "zoneinfo_goroot.cs", "AAgSgoKU")]
[assembly: GoPositionMap("time/zoneinfo_read.go", "zoneinfo_read.cs", "ABIwwgAPIIIADSCCgoKClIKCpoKCgoKUpoKCgoKClKaCgoKClKiSgoKokoKUppzCpoKokoKClJS0tLQAAxYADBCCgoKClIKUAAcQgpQAARCClKaCgoKUgpS4goK4pqampri4lIKmgoKC3IKmlIKCgoKClIKUgoKClIKClIKmlNyCgoKCgpSmgpSmgoKUgoKUgrimuLiCgoKCgoKCgqaygpSClAAHELimgoKCpqqigpSClAACGAAJAoKUqJKClKrCgoKUlAAGEoKClIKClIKCpgAXMIKUgoKCgoKCgoKClIIAEiaCipSUgoKmpgAKGMKClK7ygoKCgqaCpoKCgoKmgqaAgoKCgqaCtoKUruKCgpSCqoKCgpSClIKm")]
[assembly: GoPositionMap("time/zoneinfo_windows.go", "zoneinfo_windows.cs", "ABgyAA0CgoKUlKSCgpSCgpSCuIKUgpQACgrCgoKUlIKClIKCgqbYsoKCgqaokoKCgpSCgoKCuJSsAAgMgoKSgpSCgqaCgqbmopSUgoKUlJSCgpSCgoKCgoKC3JSCgoK4goKCgoKCuJSCgoKCgoKClIKCgrgADRoADRqCgoKClA==", "", "", "", "43:21,87:21")]
// </GoSourcePositionMaps>

namespace go;

using static go.time_package;

[GoPackage("time")]
public static partial class time_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct abbr {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindowsꓸregistry() => builtin.initPackage(typeof(@internal.syscall.windows.registry_package));
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
[GoMethodHost(typeof(time_package.Duration)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛDuration;
[GoMethodHost(typeof(time_package.Location)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛLocation;
[GoMethodHost(typeof(time_package.Month)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛMonth;
[GoMethodHost(typeof(time_package.ParseError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛParseError;
[GoMethodHost(typeof(time_package.Ticker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛTicker;
[GoMethodHost(typeof(time_package.Time)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛTime;
[GoMethodHost(typeof(time_package.Timer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛTimer;
[GoMethodHost(typeof(time_package.Weekday)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛWeekday;
[GoMethodHost(typeof(time_package.absCentury)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛabsCentury;
[GoMethodHost(typeof(time_package.absDays)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛabsDays;
[GoMethodHost(typeof(time_package.absMonth)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛabsMonth;
[GoMethodHost(typeof(time_package.absSeconds)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛabsSeconds;
[GoMethodHost(typeof(time_package.absYday)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛabsYday;
[GoMethodHost(typeof(time_package.dataIO)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛdataIO;
[GoMethodHost(typeof(time_package.fileSizeError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛfileSizeError;
[GoMethodHost(typeof(time_package.parseDurationError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class timeᴛparseDurationError;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class timeᴛDuration {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Duration d) => time_package.String⁔(d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint format(this Duration d, /*[32]*/ ж<array<byte>> Ꮡbuf) => time_package.format⁔(d, Ꮡbuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Nanoseconds(this Duration d) => time_package.Nanoseconds⁔(d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Microseconds(this Duration d) => time_package.Microseconds⁔(d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Milliseconds(this Duration d) => time_package.Milliseconds⁔(d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float64 Seconds(this Duration d) => time_package.Seconds⁔(d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float64 Minutes(this Duration d) => time_package.Minutes⁔(d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static float64 Hours(this Duration d) => time_package.Hours⁔(d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Duration Truncate(this Duration d, Duration m) => time_package.Truncate⁔(d, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Duration Round(this Duration d, Duration m) => time_package.Round⁔(d, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Duration Abs(this Duration d) => time_package.Abs⁔(d);
}
partial class timeᴛLocation {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Location> get(this ж<Location> Ꮡl) => time_package.get⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Location> Ꮡl) => time_package.String⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string name, nint offset, int64 start, int64 end, bool isDST) lookup(this ж<Location> Ꮡl, int64 sec) => time_package.lookup⁔(Ꮡl, sec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint lookupFirstZone(this ref Location l) => time_package.lookupFirstZone⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool firstZoneUsed(this ref Location l) => time_package.firstZoneUsed⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint offset, bool ok) lookupName(this ж<Location> Ꮡl, @string name, int64 unix) => time_package.lookupName⁔(Ꮡl, name, unix);
}
partial class timeᴛMonth {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Month m) => time_package.String⁔(m);
}
partial class timeᴛParseError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref ParseError e) => time_package.Error⁔(ref e);
}
partial class timeᴛTime {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Time t) => time_package.String⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string GoString(this Time t) => time_package.GoString⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Format(this Time t, @string layout) => time_package.Format⁔(t, layout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AppendFormat(this Time t, slice<byte> b, @string layout) => time_package.AppendFormat⁔(t, b, layout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> appendFormat(this Time t, slice<byte> b, @string layout) => time_package.appendFormat⁔(t, b, layout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> appendFormatRFC3339(this Time t, slice<byte> b, bool nanos) => time_package.appendFormatRFC3339⁔(t, b, nanos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) appendStrictRFC3339(this Time t, slice<byte> b) => time_package.appendStrictRFC3339⁔(t, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int32 nsec(this ref Time t) => time_package.nsec⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 sec(this ref Time t) => time_package.sec⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 unixSec(this ref Time t) => time_package.unixSec⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addSec(this ref Time t, int64 d) => time_package.addSec⁔(ref t, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setLoc(this ref Time t, ж<Location> Ꮡloc) => time_package.setLoc⁔(ref t, Ꮡloc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stripMono(this ref Time t) => time_package.stripMono⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setMono(this ref Time t, int64 m) => time_package.setMono⁔(ref t, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 mono(this ref Time t) => time_package.mono⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsZero(this Time t) => time_package.IsZero⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool After(this Time t, Time u) => time_package.After⁔(t, u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Before(this Time t, Time u) => time_package.Before⁔(t, u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Compare(this Time t, Time u) => time_package.Compare⁔(t, u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Equal(this Time t, Time u) => time_package.Equal⁔(t, u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static absSeconds absSec(this Time t) => time_package.absSec⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string name, nint offset, absSeconds abs) locabs(this Time t) => time_package.locabs⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint year, Month month, nint day) Date(this Time t) => time_package.Date⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Year(this Time t) => time_package.Year⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Month Month(this Time t) => time_package.Month⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Day(this Time t) => time_package.Day⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Weekday Weekday(this Time t) => time_package.Weekday⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint year, nint week) ISOWeek(this Time t) => time_package.ISOWeek⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint hour, nint min, nint sec) Clock(this Time t) => time_package.Clock⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Hour(this Time t) => time_package.Hour⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Minute(this Time t) => time_package.Minute⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Second(this Time t) => time_package.Second⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Nanosecond(this Time t) => time_package.Nanosecond⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint YearDay(this Time t) => time_package.YearDay⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Time Add(this Time t, Duration d) => time_package.Add⁔(t, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Duration Sub(this Time t, Time u) => time_package.Sub⁔(t, u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Time AddDate(this Time t, nint years, nint months, nint days) => time_package.AddDate⁔(t, years, months, days);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Time UTC(this Time t) => time_package.UTC⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Time Local(this Time t) => time_package.Local⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Time In(this Time t, ж<Location> Ꮡloc) => time_package.In⁔(t, Ꮡloc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Location> Location(this Time t) => time_package.Location⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string name, nint offset) Zone(this Time t) => time_package.Zone⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Time start, Time end) ZoneBounds(this Time t) => time_package.ZoneBounds⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Unix(this Time t) => time_package.Unix⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 UnixMilli(this Time t) => time_package.UnixMilli⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 UnixMicro(this Time t) => time_package.UnixMicro⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 UnixNano(this Time t) => time_package.UnixNano⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendBinary(this Time t, slice<byte> b) => time_package.AppendBinary⁔(t, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalBinary(this Time t) => time_package.MarshalBinary⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalBinary(this ref Time t, slice<byte> data) => time_package.UnmarshalBinary⁔(ref t, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) GobEncode(this Time t) => time_package.GobEncode⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error GobDecode(this ref Time t, slice<byte> data) => time_package.GobDecode⁔(ref t, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalJSON(this Time t) => time_package.MarshalJSON⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalJSON(this ref Time t, slice<byte> data) => time_package.UnmarshalJSON⁔(ref t, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) appendTo(this Time t, slice<byte> b, @string errPrefix) => time_package.appendTo⁔(t, b, errPrefix);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendText(this Time t, slice<byte> b) => time_package.AppendText⁔(t, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalText(this Time t) => time_package.MarshalText⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ref Time t, slice<byte> data) => time_package.UnmarshalText⁔(ref t, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsDST(this Time t) => time_package.IsDST⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Time Truncate(this Time t, Duration d) => time_package.Truncate⁔(t, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Time Round(this Time t, Duration d) => time_package.Round⁔(t, d);
}
partial class timeᴛTimer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Stop(this ж<Timer> Ꮡt) => time_package.Stop⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Reset(this ж<Timer> Ꮡt, Duration d) => time_package.Reset⁔(Ꮡt, d);
}
partial class timeᴛWeekday {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Weekday d) => time_package.String⁔(d);
}
partial class timeᴛabsCentury {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static absLeap leap(this absCentury century, absCyear cyear) => time_package.leap⁔(century, cyear);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint year(this absCentury century, absCyear cyear, absJanFeb janFeb) => time_package.year⁔(century, cyear, janFeb);
}
partial class timeᴛabsDays {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (absCentury century, absCyear cyear, absYday ayday) split(this absDays days) => time_package.split⁔(days);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint year, Month month, nint day) date(this absDays days) => time_package.date⁔(days);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint year, nint yday) yearYday(this absDays days) => time_package.yearYday⁔(days);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Weekday weekday(this absDays days) => time_package.weekday⁔(days);
}
partial class timeᴛabsMonth {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Month month(this absMonth m, absJanFeb janFeb) => time_package.month⁔(m, janFeb);
}
partial class timeᴛabsSeconds {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static absDays days(this absSeconds abs) => time_package.days⁔(abs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint hour, nint min, nint sec) clock(this absSeconds abs) => time_package.clock⁔(abs);
}
partial class timeᴛabsYday {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (absMonth m, nint mday) split(this absYday ayday) => time_package.split⁔(ayday);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static absJanFeb janFeb(this absYday ayday) => time_package.janFeb⁔(ayday);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint yday(this absYday ayday, absJanFeb janFeb, absLeap leap) => time_package.yday⁔(ayday, janFeb, leap);
}
partial class timeᴛdataIO {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> read(this ref dataIO d, nint n) => time_package.read⁔(ref d, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint32 n, bool ok) big4(this ref dataIO d) => time_package.big4⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint64 n, bool ok) big8(this ref dataIO d) => time_package.big8⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (byte n, bool ok) @byte(this ref dataIO d) => time_package.byte⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> rest(this ref dataIO d) => time_package.rest⁔(ref d);
}
partial class timeᴛfileSizeError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this fileSizeError f) => time_package.Error⁔(f);
}
partial class timeᴛparseDurationError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref parseDurationError e) => time_package.Error⁔(ref e);
}
// </MethodForwarders>

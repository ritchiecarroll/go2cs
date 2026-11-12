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
[assembly: GoPositionMap("time/format.go", "format.cs", "AMMB5gKYooKUggACHgAMAoKkgoKUgtqCgoKUgqaC2oKUgsiClKaClKaUgpSUgsimpqaCyILIgpSClIKUgpSCyIKUgpSClIKUgsiCgoKCpoKCgpSC2qYACRQACRQADh4ADiKigoKClIKCgrimgoKCpqyygoKCuIKUpKiSgpSCuIK4gpS4goKCgoKUgriW4oKCgpSCgoKUgpSsxIKUpoKmgoKUqqKCgoKUgoKCgpSCgpSCpgAIHAANAqaCgoKCgpSSkoKCgoKCgpSCgoKUqsKCgpSCgoKCgriUgoKCgoKCgoKCgoKktAASIoKCpIIAAhLigoKCgoKUlIKqtKSkpLiCgpQACBaCgoKUgpSmgpSCuIKmlIKClLS0tIK0tLS0grS0gpS0tIKCgqa0tMaCgpTGgoKUtLS0tLSClMaClOqCgpSCgoKCgpSUgoKUgriCgpTIgoK4goKClJSCtLbGAAoaooKCAAkWooKCgo7igoKCppSCgoKmgpSmgsiSgpzKsoKUgqyygpSCgpSUrLKSgpSClKaCgpSqooKCgpSCgpSClIKUAAJaACsEgoKmAAIQ5IKCpgAaBoKSgoKWAA0egoKCgoKClIKClJSCgoK0goKUgoKClIKU1oKClIK0gqSCpIKCyKSkgpTIgoKmyIKCtoKCtoKCxoKClIKCuIKCgpSmgpSC1oKClIKktLTmgoKUgqS0tOaCgoKUxLKCgoKUgoKUgoKCgpSCgoKClIKClIKCgoKUlIKClJSigoKCgu6ClIKUgqaCxrT4goKClIKCgpTqgoKClILGlLiCgpSCpIKUgqaCgoK4goKCgoKCgoKmgpSCgoKUuIKUgoKUlIKUgsqCpoKmgoK4goKCuIKCpoKmgoKCgriCgpSCgrgAAhoADAKCpoKmgoKmgoKClpKCgpSCppSkgtiCxpSssoKCpqyygoKUpoKUgpSmgubigoKUgoKUgpSCgriCgpSmluKCgoKClJSUgpSmrOKCgoKCgoKUgpSUgpSCgoKUgpQAChKCxgAKIOSCgqaCgoKCuIKUgpSCuqaCpoKCgpSmgoKCgoKUlLiCgoKCpoKUgoKCgpSUlIKmgpSmgoKmgpSClA==", "426-426:1", "", "1009=quote/1/4/1,quote/2/4/2,quote/3/4/3,quote/4/4/4;1011=quote/1/1/1")]
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

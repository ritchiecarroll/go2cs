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
global using runtimeꓸError = go.runtime_package.ΔError;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
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
[assembly: GoTypeAlias("Hour", "const:ΔHour")]
[assembly: GoTypeAlias("Local", "const:ΔLocal")]
[assembly: GoTypeAlias("Location", "ΔLocation")]
[assembly: GoTypeAlias("Minute", "const:ΔMinute")]
[assembly: GoTypeAlias("Month", "ΔMonth")]
[assembly: GoTypeAlias("Nanosecond", "const:ΔNanosecond")]
[assembly: GoTypeAlias("Second", "const:ΔSecond")]
[assembly: GoTypeAlias("UTC", "const:ΔUTC")]
[assembly: GoTypeAlias("Weekday", "ΔWeekday")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<ParseError, error>(Pointer = true)]
[assembly: GoImplement<fileSizeError, error>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<absJanFeb, ΔMonth>(Inverted = true, ValueType = "nint")]
[assembly: GoImplicitConv<absMonth, ΔMonth>(Inverted = true, ValueType = "nint")]
[assembly: GoImplicitConv<absSeconds, absDays>(Inverted = true, ValueType = "uint64")]
[assembly: GoImplicitConv<ΔWeekday, absDays>(Inverted = true, ValueType = "nint")]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("time/format.go", "format.cs", "AL4B5gKYooKUggACHgAMAoKApJKClILakoKClIKmgtqSlILIkpSmkpSmpIKUlILIpqamksiSyJKUgpSClIKUgsiSlIKUgpSClILIkoKCgqaCgoKUgvqmAAkUAAkUAA4eAA4iooKCgpSCgoK4poKCgqassoKCgqiClKSokoKUgqiCqIKUqIKCgoKClIK4luKCgoKUgoKClIKUrMSClKaCpoKClKqigoKClIKCgoKUgoKUgqYABRwACwKWgoKCgoKUkpKCgoKCgoKUgoKClKqigoKEgoKCgoK4lIKCgoKCgoKCgoKCgKSkAAIiAA8CgrSCAAIS4oKCgoKClJSCqrSUpKTIgoKEAAcWgoKClIKUloKUgqiClpSCgpSkpKSCpKSkpIKkpIKUpKSCgoKmpKSmgpKUpoKSlKSkpKSkgpS2gpTKooKUgoKCgoKUlIKClIKYkoKU2IKCmIKigpSUgrS2pgAKGqKSkgAKFoKCgoKO4oKCgqaUgoKCpoKUpoKokoKcqrKClIKssoKUgoKUlKyykoKUgpSmgoKUqqKCgoKUgoKUgpSClAACWgArBIKAgrYAAhDkgoCCtgAPBqKSgoKGABYegoKCgoKClIKClJSCgoK0goKUgoKClJKU1oKClIK0gqSCpIKCuLSkgpSoooKmqKKCtoKCtoKCxoKClIKCmKKCgpSmgpSC1oKClIKUpKTmgoKUgpSkpOaCgoKUxLKCgoKUgoKUpIKClKSCgpSCgpSkgoKUlIKClJSigoKCgq6ylIKUgpaCxrT4koKClIKCgpTKgqKClILGlJiCopSCtIKUgqaCpKiCgoKCgoKkpoKUgoKClLiClIKClJSClIK6gpaCloKCqIKCgqiCgpaCpoKCgoKogoKUgoKoAAIaAAwCgqaCpoKCpoKCgpaSgoKUgIK2lKSSyJLGpKyygoKWrLKCgpSWgpSClKaC1uKCgpSCgpSAgqSCgriCgpSmluKCgoKClJSUgpSmrOKCgoKCgoKUgpSUgpSCgoKUgpTGAAog5IKCloKCgoK4gpSClIKqloKmgoKClJaCgoKCgpSUqIKCgoKmgpSCgoKClJSUgqaClKaCgqaClIKU", "426-426:1")]
[assembly: go.GoPositionMap("time/format_rfc3339.go", "format_rfc3339.cs", "AAkkAAgCloKCgoKChJaCgoKCgoSCgpaCqIKCgpSUgoKC5oKCuoKUpIKSxqYACAqCkoKCgpSUgoKUqIKUgoKCgoKCgpSGkoKClIKogoKClIKCgpSCgpSWgIKUtqaCgoKCgsyCmKTUtJS05sY=", "69-69:1;88-101:1;166-166:1")]
[assembly: go.GoPositionMap("time/sleep.go", "sleep.cs", "AAwctAADSMKClIKmlAACFAAIBqYAEEYAFQKClAACNAAXAoKCggACMAAXAoKUgqjsAAkgAAoCrsKmgg==")]
[assembly: go.GoPositionMap("time/sys_unix.go", "sys_unix.cs", "AAwg7IK4goKClKaCpoLWgoKClICCpIKCgoKUlJQ=")]
[assembly: go.GoPositionMap("time/time.go", "time.cs", "AKgB4ALSqJKClKiQppKCgoKCgriogoKklLqygpSCqJKCggADEMKCgoKUlAACENKClKqiqJKClIKCqJKClIKCqqKSgpSCgqaUpKQAAhDSgpQAESySgpSCggAMIpKClIKCANEByAPEgoKClIIAEiiEgoKChKiSqOSCAAgUABAkgoKCqAAPHoKopIKClKikqKSCgpSCgpSCgpSopKikqNKCgoKCgqjCgoKCgq7ClIKUgoKClIKm2uKCgqaCgoKClJSUlIKokqiSgoKokoKCqJKCgqiSqKSuAA8WgoKCqJKo0oKCgoKCqJKokqiSqqKqooIAH0zGooKq1ISCgoKWhqKCgoKUgqaCtoSC5oLEgpSChJaCloKCgoKogoKCuoKClq7UgoKCgoKCgpSUgoKUqqKCgoKUgoKCpqiQppCmkAACGAAKAoKCqJKCgqiSgoKqooKUqqIAAhLigpSCgoKClICCpJSClICCpKyylKSkypKCgoKCpIKUgoKCgpSUpq7CgpSUlKSkyIKCgpSClKqilJSqopSUAAIoABECgoKqooKCAAsapoKCgpTcAAIkABAM3AALEJzCgoKUgoK4lKaCqJKCqJKCAAIQ0oKUgqiSgoKUqtKCAAIQAAgCgoKClIKClAACEuIAAhDSAAIQ0gACEuIACBKSgoKEgpSCgoKWgoKUloKCABMigpSokoKClPiSgoKWgoKWwoKUgpaChoKEgoKCloKChIKigpSmAAIQ0qiSrsKCgoKCgpTaooKmgpSCgoKmgoKClN7C3sKqooKCAAIQ0oKCgoKCgqaqoqqiqJKCpsqCgpSuwoKCgpSCgoKUAAIqABICgqiCgpaCgoKWAAQQgoKmgpSWgoIAAhTygoKUggACFgAIAoKClIKClKzigoKClIKCgoKCqJaCuIKCAAMQgoKCgoKS8pSCgpqCopSCgoKUgoKClJSClIKClLbugpQAAkAAHQKokqjigoKCgg==")]
[assembly: go.GoPositionMap("time/zoneinfo.go", "zoneinfo.cs", "AEOOAcoACRAACQ6CgpSClKqiAAcQpqKCgoKCgoKmlKaC7oIAAhQADgKEgoKCgoKCloCCgoKCgoKmgoKCgoKClJSCuoKCgoKCgoKCgpSmgoKClKiCgIK4AAIkABAEgqiCgoLMgoK6qqKCgqYABRIACAK6goKUgsyElJaCgoKUgqaClpSmgpSEkoKClIKCgrqCgoSCgriCgoKC3oKklLyigpSCgpSClLaClJSCgqa+0oKUgoKkgpqigoKUgoKClJaCgoKUgoKClJaCgoKUhIKUABEuooKClIKCgoKClIKkgoKCloKCgpSCgoKUgoKClIKCgpSCloKCloKClISs0oKUgoKCgpSUgoKCpoKUrLKClIKCtqaCgpKUgoKCgpiCopSCgpSUgoKUpqzyAAYQgoKCgoLMgoKCuswACSgADwKClIKUppSCkpSCgoCCgIKkpLaAgqSkqJKClIKCpg==", "119-124:1;675-678:1")]
[assembly: go.GoPositionMap("time/zoneinfo_goroot.go", "zoneinfo_goroot.cs", "AAgSgoKU")]
[assembly: go.GoPositionMap("time/zoneinfo_read.go", "zoneinfo_read.cs", "ABIwwgAPIIIADyCCgoKClIKCpoKCgoKUpoKCgoKClKaCgoKClKiSgoKokoCCpKacwpaAgqiSgoCClJS0tLQABBYAERCCgoKClIKUAAUQgpQAARCChJaCgoKUgpSogoKolpaWlqiohJKWgoKCzIKmlIKCgoKAgqSClIKCgIKkgoCCpIKmlMyCgoKCgIKUtoCClLaCgpSCgpSCqKaoqIKCgoKCgoLIgLKClICClAAJEKimgoKCpqqigpSClAACGAAJAoKUqJKClKrCgoKUhAAEEoKAgqSCgoSCgIKmABcwgpSCgoKCgoKCgoKUggARJoKAiqSEgoCCppYAChjCgpSu8oKCgoCCtoKmgoKCgIK2gqaAgoKCgIK2graClK7igoKUgqqCgoKUgpSCpg==")]
[assembly: go.GoPositionMap("time/zoneinfo_unix.go", "zoneinfo_unix.cs", "ABEqAAoOAAYQgpSCgoKCxoKUgoCCgoKUlMaAgoL8")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("time")]
public static partial class time_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
    internal partial struct rule {}
    internal partial struct ruleKind {}
    internal partial struct zone {}
    internal partial struct zoneTrans {}
    public partial struct Duration {}
    public partial struct ParseError {}
    public partial struct Time {}
    public partial struct Timer {}
    public partial struct ΔLocation {}
    public partial struct ΔMonth {}
    public partial struct ΔWeekday {}
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

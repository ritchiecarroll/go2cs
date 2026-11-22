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
using static go.@internal.trace.@internal.tracev1_package;

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
[assembly: GoDynamicTypeLift("7374727563747b4e616d6520737472696e673b206d696e56657273696f6e20696e7465726e616c2f74726163652f76657273696f6e2e56657273696f6e3b20537461636b20626f6f6c3b2041726773205b5d737472696e673b205341726773205b5d737472696e677d", "EventDescriptionsᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Events, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Event))]
[assembly: GoFieldAccess(typeof(proc))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/trace/internal/tracev1/order.go", "order.cs", "AB9GAAgGlIKCgqSCgoKkgoKCAAgQgoKCqIKCgqSCgoKkgoKCpIKCgqSCgoK2griC5oKClIKmlJS0pILKgqaygqaigsKCgoKmooKCgpTCuKKCgoKClIKClIKUwpQ=")]
[assembly: GoPositionMap("internal/trace/internal/tracev1/parser.go", "parser.cs", "AG7oAYKClIKUlKaCgqaCgpSCgpSCgqaCgoKmgoKCpgACFAAJBoKClOjSggASJoKCgoKClIKmgoKmgqaUlIKCgpSClILKgqYABxAAJU6igoKUgoKosoKCpoKCprimgqaCpoLmgoKUgoKCppSmgoKUgqaCgoKCgoIABR4AEAyipoKClJSmAAAQAAgEgoKUgoKClJSCgpSCgqaCgoKUuIIACxiClIKCgrjGgoKCppTcppS0tKSUgIKk3IIACxiCgoKUtIKClIKClIKUtpSolJKCgoKClIKUgqaCgoKmgoKmlIKUgoKClIKClLiCgoIADh64AAUQgoKCgpS4gsqUgoIACArI1oKClAAMGoKCppSUgpSCgpSCuIKClIKUgpSCgoKUgpSClIKClKaCpIK4lIKClIKmgoKUpoKCptyCpIKCgoKCgpSmgoLugoKmpqaCgoKUgoKCgpS4gqaUlIKClKaCgpSC3IK6koKClIKCgpSUgqaCgoKCgoKUgpSClIKClLiCgpSCuKaigoKUgpSClIKClKrSgoKUgoKUlIKClIKUlIK0grjGtIKUgoKUgoKUgoKCgoKCgoKUgqYACQyCgoKCgoKClKaUgrq0AA4SgpSCgoLIpoLKAA8M4rwAABqCgoKCgqSCgoKUgpSClKaClJSCgpSUtIKClIKUlLSClJS0gpS0goKUtIKClLSCgpSUtIKClJTYgoKmtIKClJS0goKUgpS0goKUgpS0goKAgqSClLaCgoKUgpSCgoKCgqaCpoK0goKAgqSCgpSCpoK0goKAgqSCgoKUgrSCgoKUgpSCgpSClIKCtoKCgIKklLSCgoCCpIKClIK0goKUgpS2goKAgqSCgoKUgrSCgpS2graCgoKCgoKCgoKmgpS4yMrMppaSgoKUgqaigoKUpoKCgoKClKqigoKUgoKUlJSCAEF6ADV4koLugoKUgqaCgoKUlIKClKY=", "180-182:1;354-362:1;380-382:1;1041-1053:1", "", "", "", "183:55,184:13")]
// </GoSourcePositionMaps>

namespace go.@internal.trace.@internal;

using static go.@internal.trace.@internal.tracev1_package;

[GoPackage("tracev1")]
public static partial class tracev1_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct batchOffset {}
    internal partial struct gState {}
    internal partial struct gStatus {}
    [GoValueClone("ev")] internal partial struct orderEvent {}
    internal partial struct orderEventList {}
    internal partial struct parser {}
    [GoLocalName("gdesc")] internal partial struct postProcessTrace_gdesc {}
    [GoLocalName("pdesc")] internal partial struct postProcessTrace_pdesc {}
    internal partial struct proc {}
    internal partial struct rawEvent {}
    [GoValueClone("Args")] public partial struct Event {}
    public partial struct EventDescriptionsᴛ1 {}
    public partial struct EventType {}
    public partial struct Events {}
    public partial struct Frame {}
    public partial struct STWReason {}
    public partial struct Timestamp {}
    public partial struct Trace {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸversion() => builtin.initPackage(typeof(go.@internal.trace.version_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(tracev1_package.Event)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tracev1ᴛEvent;
[GoMethodHost(typeof(tracev1_package.Events)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tracev1ᴛEvents;
[GoMethodHost(typeof(tracev1_package.Trace)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tracev1ᴛTrace;
[GoMethodHost(typeof(tracev1_package.orderEventList)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tracev1ᴛorderEventList;
[GoMethodHost(typeof(tracev1_package.parser)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tracev1ᴛparser;
[GoMethodHost(typeof(tracev1_package.rawEvent)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class tracev1ᴛrawEvent;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class tracev1ᴛEvent {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref Event ev) => tracev1_package.String⁔(ref ev);
}
partial class tracev1ᴛEvents {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Event> grow(this ref Events l) => tracev1_package.grow⁔(ref l);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Event> append(this ref Events l, Event v) => tracev1_package.append⁔(ref l, v);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Event> Ptr(this ref Events l, nint i) => tracev1_package.Ptr⁔(ref l, i);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, nint) index(this ref Events l, nint i) => tracev1_package.index⁔(ref l, i);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ref Events l) => tracev1_package.Len⁔(ref l);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Less(this ref Events l, nint i, nint j) => tracev1_package.Less⁔(ref l, i, j);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Swap(this ref Events l, nint i, nint j) => tracev1_package.Swap⁔(ref l, i, j);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Event>, bool) Pop(this ref Events l) => tracev1_package.Pop⁔(ref l);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Event>, bool) Peek(this ref Events l) => tracev1_package.Peek⁔(ref l);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Action<Func<ж<Event>, bool>> All(this ж<Events> Ꮡl) => tracev1_package.All⁔(Ꮡl);
}
partial class tracev1ᴛTrace {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static STWReason STWReason(this ref Trace tr, uint64 kindID) => tracev1_package.STWReason⁔(ref tr, kindID);
}
partial class tracev1ᴛorderEventList {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this ref orderEventList l, nint i, nint j) => tracev1_package.Less⁔(ref l, i, j);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Push(this ж<orderEventList> Ꮡh, orderEvent x) => tracev1_package.Push⁔(Ꮡh, x);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static orderEvent Pop(this ж<orderEventList> Ꮡh) => tracev1_package.Pop⁔(Ꮡh);
}
partial class tracev1ᴛparser {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool discard(this ref parser p, uint64 n) => tracev1_package.discard⁔(ref p, n);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Trace, error) parse(this ж<parser> Ꮡp) => tracev1_package.parse⁔(Ꮡp);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Events, error) parseEventBatches(this ref parser p) => tracev1_package.parseEventBatches⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error collectBatchesAndCPUSamples(this ref parser p) => tracev1_package.collectBatchesAndCPUSamples⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (byte, bool) readByte(this ref parser p) => tracev1_package.readByte⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) readFull(this ref parser p, nint n) => tracev1_package.readFull⁔(ref p, n);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error readRawEvent(this ref parser p, nuint flags, ж<rawEvent> Ꮡev) => tracev1_package.readRawEvent⁔(ref p, flags, Ꮡev);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<Event>, error) loadBatch(this ref parser p, int32 pid, slice<Event> events) => tracev1_package.loadBatch⁔(ref p, pid, events);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string s, error err) readStr(this ref parser p) => tracev1_package.readStr⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error parseEvent(this ref parser p, ж<rawEvent> Ꮡraw, ж<Event> Ꮡev) => tracev1_package.parseEvent⁔(ref p, Ꮡraw, Ꮡev);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error postProcessTrace(this ж<parser> Ꮡp, Events events) => tracev1_package.postProcessTrace⁔(Ꮡp, events);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint64, error) readVal(this ref parser p) => tracev1_package.readVal⁔(ref p);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<uint64> allocateStack(this ref parser p, uint64 size) => tracev1_package.allocateStack⁔(ref p, size);
}
partial class tracev1ᴛrawEvent {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint argNum(this ref rawEvent raw) => tracev1_package.argNum⁔(ref raw);
}
// </MethodForwarders>

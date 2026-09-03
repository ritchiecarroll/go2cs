// GoReflect.TypeNaming.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static go2cs.Symbols;

namespace go;

// ---------------------------------------------------------------------------------------------
// TYPE NAMING — what a converted type is CALLED in Go.
//
// WHAT LIVES HERE
//   Everything behind `reflect.Type.String()`, `Name()`, `PkgPath()` and `fmt`'s `%T`: the Go
//   source spelling of a managed type, the package qualification that turns
//   `go.main_package.Point` into `main.Point`, and the import path that turns
//   `go.encoding.gob_package.N2` into `"encoding/gob"`.
//
// WHY THIS IS RECONSTRUCTION AND NOT LOOKUP
//   Go's own reflect reads a type's NAME out of a runtime type descriptor the compiler emitted.
//   There is no such descriptor here — a converted type is an ordinary CLR type — so the Go name
//   has to be rebuilt from what the converter DID leave behind: the managed nesting
//   (`namespace go.<parents>` + class `<pkg>_package` + the type nested inside it), the
//   `[GoPackage]` / `[GoLocalName]` stamps, and the golib container generics. Every rule in this
//   file is one step of that reconstruction, and each one is exact for the shapes the converter
//   emits and only for those.
//
// THE STAMP OUTRANKS THE NAME
//   `goPackageNameOf` prefers the class's `[GoPackage]` stamp and only falls back to trimming
//   `_package` off the class name. They agree for every ordinary converted package; where they
//   disagree the stamp is right, and the case is not exotic — a `-tests` white-box bridge is class
//   `binary_internal_test_package` yet Go-declares its contents in `package binary`. Trimming
//   would report `binary_internal_test.Person`, and encoding/binary's own tests assert the type
//   name inside an error string and name their subtests from it.
//
// THE TWO PLACES THE MAPPING IS NOT AN EXACT INVERSE
//   Both are naming-only losses, both are recorded rather than fixed, and both come from a package
//   whose import path's last segment is not its package name: a major-version directory
//   (`math/rand/v2` emits namespace `go.math.rand` + class `rand_package`, so `PkgPath` recovers
//   `"math/rand"`), and a module dependency whose declared package name differs from its directory.
//   Everything else round-trips exactly.
//
// ADAPTERS RENDER AS WHAT THEY STAND FOR, NEVER AS THEMSELVES
//   A generated interface-implementation adapter is a class the converter minted; Go has no such
//   type. So `GoTypeName` unwraps it — a pointer-sourced adapter renders `*T`, a value-sourced one
//   renders the wrapped struct — exactly as `KindOf` and `ElementType` unwrap it in the primary
//   file. Those three must agree; an adapter that named itself would print a C# class name from
//   `%T` and make `reflect.TypeOf` return a type no Go program can name.
//
// THE MEMOIZATION IS NOT OPTIONAL, AND IT IS NOT INVALIDATED
//   Reading a custom attribute materializes a fresh attribute instance on EVERY call — measured
//   at ~361 ns and 200 bytes for one probe — and these reads sit under callers that cache nothing
//   of their own, so the cost was being paid per VALUE printed rather than per type. Deliberately
//   NOT hooked to any assembly-load cache clear, unlike the extension-method caches in
//   runtime/TypeExtensions: a loaded type's own attributes cannot change, so there is nothing a
//   later assembly could invalidate.
// ---------------------------------------------------------------------------------------------
public static partial class GoReflect
{
    /// <summary>
    /// The Go source type string for a managed <see cref="Type"/> — what `reflect.Type.String()` and
    /// `%T` print. Recurses over the golib container types (`[]int`, `map[string]int`, `*main.Point`),
    /// maps the scalar representations to their Go spelling, and package-qualifies a named/struct type
    /// (`go.main_package.Point` → `main.Point`).
    /// </summary>
    public static string GoTypeName(Type? t)
    {
        return GoTypeName(t, null);
    }

    /// <summary>
    /// The Go source type string with ARRAY DIMS threaded (a dims-carrying array descriptor
    /// renders Go's <c>[4]uint8</c>; without dims the managed type cannot distinguish
    /// <c>[N]T</c> from <c>[]T</c> — the recorded limitation).
    /// </summary>
    public static string GoTypeName(Type? t, nint[]? arrayDims)
    {
        return GoTypeName(t, arrayDims, GoChanDir.Unstamped);
    }

    /// <summary>
    /// The Go source type string with the CHANNEL DIRECTION threaded as well — a direction-carrying
    /// channel descriptor renders Go's <c>chan&lt;- string</c> / <c>&lt;-chan int</c>, where the
    /// managed <c>channel&lt;T&gt;</c> alone can only say <c>chan T</c>. The same shape as the array
    /// dims above and for the same reason: the datum is part of the Go type and not of the managed
    /// one, so it arrives as descriptor cargo from whichever source knew it.
    /// </summary>
    public static string GoTypeName(Type? t, nint[]? arrayDims, GoChanDir chanDir)
    {
        return GoTypeName(t, arrayDims, chanDir, null);
    }

    /// <summary>
    /// The Go source type string with a map KEY's dims threaded as well — the second half of the
    /// cargo a <c>map[[2]string][2]*float64</c> descriptor carries, where the positional dims are
    /// the ELEMENT's (what <c>Elem()</c> hands down) and these are the KEY's. Without them the
    /// managed <c>map&lt;array&lt;@string&gt;, …&gt;</c> can only say <c>map[[]string][]*float64</c>.
    /// </summary>
    public static string GoTypeName(Type? t, nint[]? arrayDims, GoChanDir chanDir, nint[]? keyDims)
    {
        if (t is null) return "<nil>";

        if (chanDir is GoChanDir.Recv or GoChanDir.Send && t.IsGenericType &&
            t.GetGenericTypeDefinition() == typeof(channel<>))
        {
            // The DIRECTION belongs to this channel and stops here; the DIMS are the element's and
            // pass unshifted, exactly as the bidirectional arm below hands them on. `chan<- [3]int`
            // needs both — the arrow from this frame, the length from the element's.
            string elem = GoTypeName(t.GetGenericArguments()[0], arrayDims);
            return chanDir == GoChanDir.Recv ? "<-chan " + elem : "chan<- " + elem;
        }

        if (arrayDims is { Length: > 0 } && t.IsGenericType && t.GetGenericTypeDefinition() == typeof(array<>))
        {
            nint[]? innerDims = arrayDims.Length > 1 ? arrayDims[1..] : null;
            return "[" + arrayDims[0] + "]" + GoTypeName(t.GetGenericArguments()[0], innerDims);
        }

        if (t == typeof(bool)) return "bool";
        if (t == typeof(nint)) return "int";
        if (t == typeof(sbyte)) return "int8";
        if (t == typeof(short)) return "int16";
        if (t == typeof(int)) return "int32";
        if (t == typeof(long)) return "int64";
        if (t == typeof(nuint)) return "uint";
        if (t == typeof(byte)) return "uint8";
        if (t == typeof(ushort)) return "uint16";
        if (t == typeof(uint)) return "uint32";
        if (t == typeof(ulong)) return "uint64";
        if (t == typeof(uintptr)) return "uintptr";
        if (t == typeof(float)) return "float32";
        if (t == typeof(double)) return "float64";
        if (t == typeof(Complex)) return "complex128";
        if (t == typeof(complex64)) return "complex64";
        if (t == typeof(@string) || t == typeof(string)) return "string";
        if (t == typeof(object)) return "interface {}";

        // @unsafe.Pointer names itself by its marker — BEFORE the box arm, whose base-chain walk
        // would otherwise render it `*uintptr` (M-before-W, DESIGN-zh-box-b1.md §3.1).
        if (typeof(IUnsafePointer).IsAssignableFrom(t)) return "unsafe.Pointer";

        if (t.IsGenericType)
        {
            Type gd = t.GetGenericTypeDefinition();
            Type[] a = t.GetGenericArguments();

            // A SLICE's dims are its ELEMENT's and pass UNSHIFTED — the rule the map arm below and
            // the pointer arm further down already follow, and the one Elem() hands them down by.
            // Rendered with the cargo-less overload, `[][6]uint8` printed `[][]uint8`: the element's
            // length was applied at the position that owned it and dropped on the way in.
            // See docs/phase4/DESIGN-descriptor-cargo.md.
            if (gd == typeof(slice<>)) return "[]" + GoTypeName(a[0], arrayDims, chanDir, keyDims);
            if (gd == typeof(array<>)) return "[]" + GoTypeName(a[0]);   // length is not carried on the managed type
            // A MAP descriptor's positional dims are its ELEMENT's and its key dims are its KEY's —
            // the two accessors, each fed from the slot that reaches it, so
            // `map[[2]string][2]*float64` renders both lengths instead of neither.
            if (gd == typeof(map<,>)) return "map[" + GoTypeName(a[0], keyDims) + "]" + GoTypeName(a[1], arrayDims);
            // A CHANNEL's are its element's too, for the same reason a slice's are: no length of its
            // own to consume. The direction has already been applied by the arm at the top of this
            // method (which handles Recv/Send); reaching here means bidirectional, so the element
            // takes the dims and its own direction, never this channel's.
            if (gd == typeof(channel<>)) return "chan " + GoTypeName(a[0], arrayDims);
        }

        // A pointer descriptor's dims are the POINTEE's, unshifted (the same rule Elem() hands the
        // cargo down by) — so `*[10]int` renders its array, not `*[]int`. The channel direction and
        // the map key dims ride down the same way, so `*chan<- string` keeps its arrow and
        // `*map[[2]string]V` keeps its key length. Resolved by the base-chain walk (fix W) so a
        // per-kind subclass instance names the same `*T` its declared type does.
        if (TryBoxPointee(t, out Type? pointee)) return "*" + GoTypeName(pointee, arrayDims, chanDir, keyDims);

        // An UNNAMED func type renders STRUCTURALLY, exactly as an unnamed struct does — `func()`,
        // `func(*testing.T)`, `func(int, ...string) (bool, error)`. A Go DEFINED func type keeps its
        // name, and the two are told apart by the SAME test every other named type uses: a defined
        // func type is emitted as a `delegate` nested in its `<pkg>_package` class
        // (`http.HandlerFunc`), while an unnamed one lands on a BCL/golib delegate family
        // (`Action<ж<T>>`, `Func<…>`, the variadic `Funcꓸꓸꓸ`/`Actionꓸꓸꓸ`) that no Go package
        // declares. Without this arm the family's own C# name leaked out of `%v`/`%T` —
        // `Sprintf("%#v", TestFmtInterface)` printed ``(Action`1)(0x…)`` where Go prints
        // `(func(*testing.T))(0x…)`.
        if (isUnnamedFuncType(t))
            return goFuncTypeString(t);

        // A generated interface-implementation adapter stands in for the Go dynamic value it
        // wraps: a pointer-sourced ж-adapter renders as Go's *T, a value-sourced ᴠ-adapter as
        // the wrapped struct type itself — never as the adapter class.
        if (TryAdapterWrappedType(t, out Type? wrapped, out bool pointerSourced))
            return pointerSourced ? "*" + GoTypeName(wrapped) : GoTypeName(wrapped);

        // An UNNAMED struct type has no Go name to report, so Go renders it STRUCTURALLY —
        // `struct { X int; y int }`. golib's EmptyStruct IS Go's `struct{}`, and the converter
        // LIFTS every other anonymous struct into a named C# type stamped [GoType("dyn")]; that
        // stamp is what distinguishes a lift from an ordinary declared struct, whose Go name is
        // its own. Without this arm the lift's synthesized C# name leaked out of
        // reflect.Type.String()/%T — go/ast's TestPrint reported `ast_internal_test.typeᴛ1`.
        // A lift that ALSO carries [GoLocalName] is not anonymous at all: it is a NAMED
        // function-local type (`type Person struct{...}` inside a func), which Go renders by
        // name — the stamp GoQualifiedName prefers — so the structural arm must skip it
        // (guarded by GolibTests' ALiftedTypesLocalNameStampIsNotReReadPerCall).
        if (t == typeof(EmptyStruct))
            return "struct {}";

        if (t.IsValueType && goLocalNameOf(t) is null && goTypeMarkerOf(t) is { Definition: "dyn" })
            return goStructTypeString(t);

        // An anonymous INTERFACE lift is the struct lift's rule one type category over: the same
        // [GoType("dyn")] stamp, and Go renders it structurally — `interface { F() }` — never by
        // the lifted C# identifier (internal/reflectlite's TestNames measured `typeᴛ30` escaping
        // from Name(), which is this method's output with the qualifier trimmed).
        if (t.IsInterface && goLocalNameOf(t) is null && goTypeMarkerOf(t) is { Definition: "dyn" })
            return goInterfaceTypeString(t);

        return GoQualifiedName(t);
    }

    /// <summary>
    /// Whether a managed <see cref="Type"/> stands for a Go DEFINED (named) type — the gate behind
    /// <c>reflect.Type.Name()</c>, which reports a name for a defined type and <c>""</c> for every
    /// other one. This is the managed reconstruction of the descriptor bit Go's own
    /// <c>rtype.Name()</c> consults, <c>abi.Type.HasName()</c>.
    /// </summary>
    /// <remarks>
    /// Mirrors <see cref="GoTypeName"/> ARM FOR ARM, and has to: <c>Name()</c> is that method's
    /// output with the package qualifier trimmed off, so the two disagreeing would let a type report
    /// a name it does not have — or hide one it does. False for exactly the arms that render Go
    /// STRUCTURALLY (the raw golib containers <c>[]T</c>/<c>[N]T</c>/<c>map[K]V</c>/<c>chan T</c>/
    /// <c>*T</c>, <c>interface {}</c>, <c>struct {}</c>, an anonymous-struct lift, and the
    /// pointer-sourced adapter that stands for <c>*T</c>); true everywhere else — including the
    /// predeclared scalars, since Go's <c>int</c> IS a named type, and, the case this exists for, a
    /// DEFINED type whose underlying type is a composite.
    ///
    /// That last case is the whole point, and it is why the test cannot be
    /// <c>ElementType(t) is null</c> — which is what stood in <c>rtype.Name()</c> until 2026-08-11.
    /// A named container HAS an element type exactly as the unnamed one does, so that proxy answered
    /// <c>""</c> for <c>type testSET []int</c> while <c>PkgPath()</c>, reading the same managed
    /// nesting, correctly answered <c>"main"</c>. encoding/asn1's <c>getUniversalType</c> chooses
    /// between SEQUENCE and SET on <c>strings.HasSuffix(t.Name(), "SET")</c> and nothing else, so
    /// the entire visible symptom was one byte: <c>30</c> where Go writes <c>31</c>. Guarded by the
    /// ReflectStructTagCopy behavioral test, which pairs every named shape with its unnamed control.
    /// </remarks>
    public static bool HasGoName(Type? t)
    {
        if (t is null)
            return false;

        // Go's empty interface is an unnamed type.
        if (t == typeof(object))
            return false;

        // The raw golib containers ARE Go's unnamed composites. A DEFINED container type is a
        // converted wrapper struct that merely IMPLEMENTS one of the container interfaces, so it
        // never matches here — which is the distinction the old element-type proxy could not draw.
        if (t.IsGenericType)
        {
            Type gd = t.GetGenericTypeDefinition();

            if (gd == typeof(slice<>) || gd == typeof(array<>) || gd == typeof(map<,>) ||
                gd == typeof(channel<>))
                return false;
        }

        // A ж box at any depth is unnamed too (fix W — a per-kind subclass instance must not fall
        // past this arm and claim a Go name it does not have; the asn1 SET/SEQUENCE class). The
        // M-before-W order holds here in the EXEMPTION polarity: @unsafe.Pointer's chain carries
        // ж<uintptr>, but it is Go's one NAMED pointer type ("unsafe.Pointer") and must keep
        // answering true through the fall-through, as it always has.
        if (!typeof(IUnsafePointer).IsAssignableFrom(t) && TryBoxPointee(t, out _))
            return false;

        // An adapter renders as what it stands for (R10): a pointer-sourced one as the unnamed
        // `*T`, a value-sourced one as the struct it wraps — whose own name is then the answer.
        if (TryAdapterWrappedType(t, out Type? wrapped, out bool pointerSourced))
            return !pointerSourced && HasGoName(wrapped);

        // `struct {}` and every lifted anonymous struct render structurally; a lift that carries
        // [GoLocalName] is a NAMED function-local type and keeps its name, matching GoTypeName.
        if (t == typeof(EmptyStruct))
            return false;

        if ((t.IsValueType || t.IsInterface) && goLocalNameOf(t) is null && goTypeMarkerOf(t) is { Definition: "dyn" })
            return false;

        // An unnamed func type renders structurally, so it has no name — the same arm GoTypeName
        // grew for it. A DEFINED func type is a delegate the converter declared inside a
        // `<pkg>_package` class and keeps its name here, as it does there.
        if (isUnnamedFuncType(t))
            return false;

        return true;
    }

    /// <summary>
    /// Whether a managed type is a Go UNNAMED func type — a delegate that no converted package
    /// declares, i.e. one of the BCL/golib delegate families the converter uses to spell a func
    /// type it has no name for.
    /// </summary>
    /// <remarks>
    /// A Go DEFINED func type (<c>type HandlerFunc func(ResponseWriter, *Request)</c>) IS emitted
    /// as its own <c>delegate</c> nested in the declaring <c>&lt;pkg&gt;_package</c> class, so the
    /// declaring-class test is exact for both directions and needs no name parsing. The residual
    /// is stated rather than hidden: a defined METHODLESS func type the converter renders inline as
    /// its base delegate family is indistinguishable from an unnamed one here, and reports the
    /// unnamed answer — the conservative direction, and the same "describe the type the bridge can
    /// actually build a descriptor for" rule <c>ChanDir</c> settles on.
    /// </remarks>
    private static bool isUnnamedFuncType(Type t)
    {
        return typeof(Delegate).IsAssignableFrom(t) && goPackageNameOf(t.DeclaringType).Length == 0;
    }

    /// <summary>
    /// Go's structural spelling of an unnamed func type — the text <c>reflect.Type.String()</c>
    /// reports for a func type literal.
    /// </summary>
    /// <remarks>
    /// Built from <see cref="TryFuncShape"/>, the SAME projection <c>rtype.NumIn</c>/<c>In</c>/
    /// <c>NumOut</c>/<c>Out</c>/<c>IsVariadic</c> read, so the name a func type reports and the
    /// signature it hands out cannot disagree. Go's format, verified against the toolchain:
    /// <c>func(</c> + the inputs joined by <c>", "</c> + <c>)</c>, the variadic tail written
    /// <c>...T</c> over its ELEMENT type; then nothing for no results, <c>" T"</c> for one, and
    /// <c>" (T, U)"</c> for several.
    /// </remarks>
    private static string goFuncTypeString(Type t)
    {
        if (!TryFuncShape(t, out Type[]? ins, out Type[]? outs, out bool isVariadic))
            return "func()";

        StringBuilder builder = new("func(");

        for (int i = 0; i < ins.Length; i++)
        {
            if (i > 0)
                builder.Append(", ");

            if (isVariadic && i == ins.Length - 1)
                builder.Append("...").Append(GoTypeName(ElementType(ins[i])));
            else
                builder.Append(GoTypeName(ins[i]));
        }

        builder.Append(')');

        if (outs.Length == 1)
        {
            builder.Append(' ').Append(GoTypeName(outs[0]));
        }
        else if (outs.Length > 1)
        {
            builder.Append(" (");

            for (int i = 0; i < outs.Length; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append(GoTypeName(outs[i]));
            }

            builder.Append(')');
        }

        return builder.ToString();
    }

    /// <summary>
    /// Go's structural spelling of an unnamed struct type — the text
    /// <c>reflect.Type.String()</c> reports for a type literal.
    /// </summary>
    /// <remarks>
    /// Built from <see cref="GoFields"/>, the SAME projection <c>NumField</c>/<c>Field</c> and the
    /// value side read, so the name a type reports and the fields it hands out cannot disagree. Go's
    /// format, verified against the toolchain: <c>struct {}</c> when empty, otherwise
    /// <c>struct { </c> + <c>Name Type</c> members joined by <c>"; "</c> + <c> }</c>; an EMBEDDED
    /// field contributes its type alone, and a tagged field appends the Go-quoted tag.
    /// </remarks>
    private static string goStructTypeString(Type t)
    {
        GoFieldInfo[] fields = GoFields(t);

        if (fields.Length == 0)
            return "struct {}";

        StringBuilder builder = new("struct { ");

        for (int i = 0; i < fields.Length; i++)
        {
            if (i > 0)
                builder.Append("; ");

            string fieldType = GoTypeName(fields[i].Type, fields[i].ArrayDims);

            // Go names an EMBEDDED field after its type's unqualified name, and prints only the
            // type — so that coincidence is exactly how the embed is recognized here.
            bool embedded = fields[i].Name.Length > 0 &&
                            (fieldType == fields[i].Name || fieldType.EndsWith("." + fields[i].Name, StringComparison.Ordinal));

            if (!embedded)
                builder.Append(fields[i].Name).Append(' ');

            builder.Append(fieldType);

            if (fields[i].Tag.Length > 0)
                builder.Append(' ').Append(quoteGoTag(fields[i].Tag));
        }

        return builder.Append(" }").ToString();
    }

    /// <summary>
    /// Go's structural spelling of an unnamed interface type — the text
    /// <c>reflect.Type.String()</c> reports for an interface literal the converter lifted.
    /// </summary>
    /// <remarks>
    /// Methods are gathered flattened (Go flattens embedded interfaces) and rendered in sorted
    /// order, which is the descriptor order Go's own String() walks. Signatures render types
    /// only — parameter names are not in Go's descriptors either — in goFuncTypeString's format:
    /// a ValueTuple return is Go's multi-result list, a params-array tail is Go's variadic.
    /// Property accessors and other special-name members are skipped; a converted Go interface
    /// declares methods alone.
    /// </remarks>
    private static string goInterfaceTypeString(Type t)
    {
        List<System.Reflection.MethodInfo> methods = [];

        foreach (System.Reflection.MethodInfo method in t.GetMethods())
        {
            if (!method.IsSpecialName)
                methods.Add(method);
        }

        foreach (Type embedded in t.GetInterfaces())
        {
            foreach (System.Reflection.MethodInfo method in embedded.GetMethods())
            {
                if (!method.IsSpecialName)
                    methods.Add(method);
            }
        }

        if (methods.Count == 0)
            return "interface {}";

        methods.Sort(static (a, b) => string.CompareOrdinal(a.Name, b.Name));

        StringBuilder builder = new("interface { ");

        for (int i = 0; i < methods.Count; i++)
        {
            if (i > 0)
                builder.Append("; ");

            System.Reflection.MethodInfo method = methods[i];
            System.Reflection.ParameterInfo[] parameters = method.GetParameters();

            builder.Append(method.Name).Append('(');

            for (int j = 0; j < parameters.Length; j++)
            {
                if (j > 0)
                    builder.Append(", ");

                if (j == parameters.Length - 1 && parameters[j].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                    builder.Append("...").Append(GoTypeName(ElementType(parameters[j].ParameterType)));
                else
                    builder.Append(GoTypeName(parameters[j].ParameterType));
            }

            builder.Append(')');

            Type returnType = method.ReturnType;

            if (returnType != typeof(void))
            {
                if (returnType.IsGenericType && returnType.FullName?.StartsWith("System.ValueTuple", StringComparison.Ordinal) == true)
                {
                    Type[] results = returnType.GetGenericArguments();

                    builder.Append(" (");

                    for (int j = 0; j < results.Length; j++)
                    {
                        if (j > 0)
                            builder.Append(", ");

                        builder.Append(GoTypeName(results[j]));
                    }

                    builder.Append(')');
                }
                else
                {
                    builder.Append(' ').Append(GoTypeName(returnType));
                }
            }
        }

        return builder.Append(" }").ToString();
    }

    /// <summary>
    /// Go's <c>strconv.Quote</c> of a struct tag, which is how a tag appears inside a struct type's
    /// string. Tags are printable text by convention, where Quote escapes only the quote and the
    /// backslash; the C0 controls carry Go's own escapes so an unconventional tag still round-trips.
    /// </summary>
    private static string quoteGoTag(string tag)
    {
        StringBuilder builder = new("\"");

        foreach (char c in tag)
        {
            switch (c)
            {
                case '"': builder.Append("\\\""); break;
                case '\\': builder.Append("\\\\"); break;
                case '\a': builder.Append("\\a"); break;
                case '\b': builder.Append("\\b"); break;
                case '\f': builder.Append("\\f"); break;
                case '\n': builder.Append("\\n"); break;
                case '\r': builder.Append("\\r"); break;
                case '\t': builder.Append("\\t"); break;
                case '\v': builder.Append("\\v"); break;
                default:
                    if (c < 0x20 || c == 0x7F)
                        builder.Append("\\x").Append(((int)c).ToString("x2"));
                    else
                        builder.Append(c);
                    break;
            }
        }

        return builder.Append('"').ToString();
    }

    private static readonly ConcurrentDictionary<Type, GoLocalNameAttribute?> s_goLocalNames = new();

    /// <summary>The type's own <c>[GoLocalName]</c> stamp, or <c>null</c> when it carries none.</summary>
    /// <remarks>Memoized per type for the reason given on <see cref="goTypeMarkerOf"/>.</remarks>
    private static GoLocalNameAttribute? goLocalNameOf(Type t)
    {
        return s_goLocalNames.GetOrAdd(t, static type =>
            type.GetCustomAttributes(typeof(GoLocalNameAttribute), false) is [GoLocalNameAttribute localName] ? localName : null);
    }

    private static readonly ConcurrentDictionary<Type, string> s_goPackageNames = new();

    /// <summary>
    /// The GO package name a converted type's declaring class stands for, or <c>""</c> when the
    /// class is not a package class at all.
    /// </summary>
    /// <remarks>
    /// The class's own <c>[GoPackage]</c> stamp is the authority; trimming <c>_package</c> off the
    /// class NAME is only a fallback for a hand-written class that carries no stamp. The two agree
    /// for every ordinary converted package, and where they disagree the stamp is right: a `-tests`
    /// white-box bridge is class <c>binary_internal_test_package</c> yet stamped
    /// <c>[GoPackage("binary")]</c>, because the internal <c>_test.go</c> declarations it hosts are
    /// Go-declared in <c>package binary</c> — which is what <c>reflect</c> must report
    /// (<c>binary.Person</c>, not <c>binary_internal_test.Person</c>: encoding/binary's
    /// TestNoFixedSize asserts the type name inside an error string, and TestSizeAllocs names its
    /// subtests from it). Memoized per declaring type for the reason given on
    /// <see cref="goTypeMarkerOf"/> — this sits under every <c>GoTypeName</c>/<c>PkgPath</c> read.
    /// </remarks>
    private static string goPackageNameOf(Type? decl)
    {
        if (decl is null)
            return "";

        return s_goPackageNames.GetOrAdd(decl, static type =>
        {
            if (!type.Name.EndsWith(PackageSuffix, StringComparison.Ordinal))
                return "";

            if (type.GetCustomAttributes(typeof(GoPackageAttribute), false) is [GoPackageAttribute marker] &&
                marker.PackageName.Length > 0)
                return marker.PackageName;

            return type.Name[..^PackageSuffix.Length];
        });
    }

    // The package-qualified Go name of a converted named type: a converted type is nested in a
    // `<pkg>_package` class, so `go.main_package.Point` → `main.Point`. A lifted function-local
    // type prefers its stamped original Go name ([GoLocalName] — `binary.Person`, never the
    // lifted `TestNoFixedSize_Person`). A Δ-collision rename (ΔHandle) strips the marker; a type
    // with no `_package` declaring class falls back to its bare name.
    private static string GoQualifiedName(Type t)
    {
        string name = goBareTypeName(t);

        if (goPackageNameOf(t.DeclaringType) is { Length: > 0 } packageName)
            return packageName + "." + name;

        return name;
    }

    // The UNQUALIFIED Go name of a converted named type — GoQualifiedName without the package
    // prefix. An INSTANTIATED generic type replaces the CLR arity spelling (`B`1`) with Go's
    // bracket instantiation (`B[<args>]`), each type argument qualified by IMPORT PATH per
    // goTypeArgumentName — which is what makes rtype.Name()'s trim-at-the-last-dot-outside-
    // brackets recover Go's exact `B[internal/reflectlite_test.A]`.
    private static string goBareTypeName(Type t)
    {
        string name = t.Name;

        if (goLocalNameOf(t) is { } localName)
            name = localName.Name;

        if (name.StartsWith(ShadowVarMarker, StringComparison.Ordinal))
            name = name[ShadowVarMarker.Length..];

        if (t.IsGenericType && !t.IsGenericTypeDefinition)
        {
            int arity = name.IndexOf('`');

            if (arity >= 0)
            {
                StringBuilder builder = new(name[..arity]);
                Type[] args = t.GetGenericArguments();

                builder.Append('[');

                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                        builder.Append(',');

                    builder.Append(goTypeArgumentName(args[i]));
                }

                name = builder.Append(']').ToString();
            }
        }

        return name;
    }

    // Go qualifies the type ARGUMENTS of an instantiated type by IMPORT PATH, never by package
    // name — `B[internal/reflectlite_test.A]`, verified against the toolchain by reflectlite's
    // TestNames. A predeclared or unnamed argument keeps its ordinary Go spelling (`B[int]`,
    // `B[[]byte]`), and nesting recurses through the same rule.
    private static string goTypeArgumentName(Type t)
    {
        string path = GoPackagePath(t);

        if (path.Length == 0)
            return GoTypeName(t);

        return path + "." + goBareTypeName(t);
    }

    /// <summary>
    /// The Go IMPORT PATH of the package that DEFINES a converted named type
    /// (<c>go.encoding.gob_package.N2</c> → <c>"encoding/gob"</c>) — <c>reflect.Type.PkgPath</c>.
    /// Empty for a type that is not a defined Go type: a primitive, a raw container
    /// (<c>slice&lt;T&gt;</c> / <c>map&lt;K,V&gt;</c> / <c>ж&lt;T&gt;</c>), or anything not nested in a
    /// <c>&lt;pkg&gt;_package</c> class — exactly Go's rule that only a DEFINED type has a package path.
    /// </summary>
    /// <remarks>
    /// Derived from the managed nesting, which is where the converter puts the package identity: the
    /// declaring class names the package and the enclosing namespace names its parent directories
    /// (<c>go</c> is the emission root). The mapping is not a strict inverse for the two cases where
    /// the class name is not the path's last segment — a major-version directory
    /// (<c>math/rand/v2</c> emits namespace <c>go.math.rand</c> + class <c>rand_package</c>, so this
    /// recovers <c>"math/rand"</c>) and a module dependency whose package name differs from its path
    /// segment. Both are naming-only losses; the Go-visible path is exact for every other package.
    /// </remarks>
    public static string GoPackagePath(Type? t)
    {
        return t is null ? "" : GoPackageClassPath(t.DeclaringType);
    }

    /// <summary>
    /// The Go IMPORT PATH a <c>&lt;pkg&gt;_package</c> CLASS itself stands for — the same answer
    /// <see cref="GoPackagePath"/> gives for a type nested in it, reachable when what is in hand
    /// is the package class (an extension METHOD's declaring type) rather than a nested type.
    /// Empty for a class that is not a package class at all — a hand-written golib type has no
    /// Go package identity.
    /// </summary>
    public static string GoPackageClassPath(Type? packageClass)
    {
        if (goPackageNameOf(packageClass) is not { Length: > 0 } pkg)
            return "";

        string ns = packageClass!.Namespace ?? "";

        if (ns.Length > EmissionRootNamespace.Length + 1 && ns.StartsWith(EmissionRootNamespace + ".", StringComparison.Ordinal))
            return ns[(EmissionRootNamespace.Length + 1)..].Replace('.', '/') + "/" + pkg;

        return pkg;
    }

    // The namespace every converted package is emitted under; its dotted tail mirrors the import
    // path's parent directories (go.encoding.gob_package ⇒ "encoding/gob").
    private const string EmissionRootNamespace = "go";
}
